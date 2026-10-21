# 代码级自校验测试（GamblerMod.Tests）

> 建：2026-10-21，起因是用户实测"很多卡牌没有效果"（当次定位出**存钱**的派发断裂 bug）。
> 本文是测试套件的完整说明：每组规则验证什么、防的是哪类真实事故、哪些写法会被豁免。
> 游戏内人工验收清单在 [test-plan.md](test-plan.md)，两者互补：**这里管"代码契约"，那里管"运行行为"**。

## 1. 怎么跑

```
cd GamblerMod.Tests
dotnet run
```

- **零 NuGet 依赖**（自写 runner，离线可用）；
- 跑之前会**自动构建主项目**（保证被测 dll 与当前源码一致，顺带完成游戏部署）；
- **退出码 = 失败组数**（0 = 全绿），可直接接脚本/CI；
- 每组测试打印 `[PASS]`/`[FAIL]`，FAIL 下列出逐条失败项（含卡名与文件名）。

## 2. 十二组测试一览

| # | 组名 | 验证什么 | 防的真实事故 |
|---|---|---|---|
| R1 | `ApplyEffectHasRealBody` | 87 张逐卡：效果体非空（表达式体 `=>` 与块体 `{}` 都认） | "打出没效果"的直接形态——方法体只有 `Task.CompletedTask` |
| R2 | `DescVarsMatchDeclarations` | 描述 `!X!` ↔ `CanonicalVars` 双向对账：描述用的必须已声明；声明且上卡面的必须被效果体真读 | 卡面印了数字、但那一支效果从未实现 |
| R3 | `PromisedEffectsHaveCommands` | 描述承诺的效果 ↔ 效果体的命令映射（伤害/格挡/金币/能量/抽牌/状态/必正/消耗） | 描述与代码脱钩（"说好的格挡呢"） |
| R4 | `DamageBlockVarsUseEngineTypes` | 伤害/格挡语义的变量必须用原生 `DamageVar`/`BlockVar` | v1.8 实测坑：写成 `IntVar` → 卡面永远裸值，不吃附魔/力量/虚弱 |
| R5 | `UpgradeDoesSomething` | `OnUpgrade` 必须有效（改数值/改费用/挂关键词） | 升级白升 |
| R6 | `GambleCardsActuallyBranchOnHeads` | 赌博牌：效果体必须真用 `heads` + 描述正负两支齐全；非赌博牌描述不得提硬币 | v1.8 实测坑：掷了硬币却没有分支（硬币纯装饰）；押注描述缺反面分支（首轮抓到并修复） |
| R7 | `DescriptionStartsWithHash` | 描述必须 `#` 开头 | SimpleLoc 不处理 → `!Var!` 占位符原样上卡面 |
| R8 | `AllCardsRegisteredWithUniqueTitles` | 全卡在 `GamblerCards.Init` 注册 + 显示名唯一 + 总数 87 | 卡文件在但没注册 → 图鉴/抽卡永远见不到 |
| R9 | `PowersMustNotOverrideGoldHooks` | power 不得覆写金币三钩子（`ModifyGoldGained`/`AfterModifyingGoldGained`/`AfterGoldGained`） | **2026-10-21 存钱实测无效的根因**：run 级金币钩子只派发给订阅单例（`IterateHookListeners`），power 收不到——覆写=永远不触发。正确做法：`Core/GamblerHooks` 转发到 power 静态方法（利滚利/存钱同款） |
| R10 | `EntryMustNotCreateHookSingletons` | `Entry` 不得 `new GamblerHooks()` 等 `CustomSingletonModel` 子类 | 启动日志实测 ERROR：BaseLib 在 `ModelDb.Init` 自动实例化这些子类，Entry 再 new → ModHelper 拒绝二次订阅 |
| R11 | `EveryPowerHasAnApplier` | 每个 power 必须被至少一张卡 `Apply<Powers.X>` | 死 power（写好了但没有任何卡能挂上它） |
| P1 | `MultiplierSemantics`（可执行） | `MultiplierService` 刻度语义：`AddHeads`/`AddSteps`/`Reset`/`Clear`/`LabelOf`，含"AddSteps 不写 LastOutcome"的 2026-10-16 审查口径 | 倍率系统核心逻辑回归（反射直调真实现，非复刻） |

## 3. 规则的校准口径（哪些写法不算违规）

这些口径全部来自首轮跑出来的假阳性，每条都对应一类合法写法——**改规则前先看这里**：

- **R1**：能力牌效果体是 `PowerCmd.Apply` 的不算空转；
- **R2**：`!D!`/`!B!` 视为标准 `Damage`/`Block` 变量（效果体用 `DamageCmd` 即算已用）；X 费卡的金币变量（`GoldX`）在费用侧消费，豁免；
- **R3**：
  - 描述先剔除干扰短语再做映射：`花费 *N 金币*`（费用侧）、`不消耗金币`（假币免单）、`无视格挡`（暗注穿透）、`消耗…必正`（收网消耗的是必正层数）、`已花费过 N 金币`（花费追踪的触发条件）；
  - **金币按子句判定**：只有含"获得/失去/返还"的动作子句才算效果承诺（"每拥有 50 金币 +1 能量"这类条件子句不算）；
  - 能力牌（`CardType.Power`）效果体含 `PowerCmd.Apply` 即视为已兑现（效果委托给 power）；
  - "造成伤害"的兑现含 `VigorPower`（再加注买活力，伤害延后一拍）；
  - "必正"的兑现含 `ForcedHeadsRemaining`/`ConsumeForcedHeads`（收网 v1.13 改"读取不消耗"）；
  - "消耗"词条查**整个文件**而不只效果体——`AfterCreated` 里的 `AddKeyword(CardKeyword.Exhaust)` 也算（撕毁契约的做法）；
- **R5**：效果体用 `IsUpgraded` 编码升级档的算有效（v1.15 存钱/轮盘/保本的模式）；
- **R11**：文件里加注释 `// 测试豁免：无施加来源` 可显式豁免（当前无人使用）。

## 4. 测试本身怎么保证不是摆设

- **负向验证**：R9 上线时把坏代码（存钱的旧 override）临时塞回去跑过——确认能 FAIL、退出码 1，再还原复绿；
- **真实现直调**：P1 通过反射调用**编译后的 `GamblerMod.dll`** 里的 `MultiplierService`，不是复制粘贴的参考实现；
- **事实溯源**：R9/R10 的判定依据（IL 反汇编 `GainGold` 派发集合、启动日志的订阅 ERROR）记录在 [../design/gambler-tech-design.md](../design/gambler-tech-design.md) §13 与 [../../notes/modding-api.md](../../notes/modding-api.md)。

## 5. 已知边界（这些不归本套件管）

1. **触发端的行为正确性**：R11 焊死"施加端"，但"power 挂上后触发链是否完好"形式多样（钩子/C# 事件/SpendTracker 记账），静态无法全覆盖——2026-10-21 已人工全量审计一次（结论：87 卡 + 13 power 闭环全通，见技术设计 §13），此后新增 power 时按 HANDOFF 的引擎事实 #20 检查；
2. **数值平衡**：不判断"9 伤贵不贵"，只判断"说好的 9 伤有没有真的造成"；
3. **游戏内表现**：贴图/动画/图鉴渲染等，走 test-plan 人工验收。

## 6. 改动测试的注意

- 新规则先跑一遍全池，把假阳性按 §3 的方式**加口径**（而不是删规则）；
- 口径必须对应一类合法写法并写注释说明出处（哪张卡、哪个版本）；
- 新规则上线时做一次负向验证（构造一个坏样例确认能抓到）。
