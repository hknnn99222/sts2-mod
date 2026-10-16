# 赌徒 Mod 代码审查报告（2026-10-16）

> 状态：审查完成，**未修改任何代码**｜范围：`GamblerMod/src` 全部 94 个 C# 文件（82 卡 + Core 6 + Powers 6 + Relics 2 + UI 1 + Character 2 + Entry）｜红线依据：`gambler-tech-design.md` §3.1/§4.8/§5（约束④ + R1-R8）、`gambler-tech-multiplier.md` §0/§4、`gambler-design.md` §2.8

## 方法

三个并行审查代理（Core/Powers/Relics/UI 一组，Basic+Common 22 张一组，Uncommon+Rare 60 张一组）+ 主审对**每条关键发现逐行复核**。本文档只收录主审核实过的事实；代理引用未经复核的条目一律剔除。

## 总评

**架构纪律是好的**：模板方法基类、倍率唯一真源、热路径钩子零分配零 LINQ、随机全走官方 Rng、`IsGambleCard`/`IsHeadsStreakCard` 全池互斥、79 张卡的数值/升级/描述与设计文档逐项一致。问题集中在：**1 个实锤数值 bug（抽头升级）、2 个高危待游戏内验证（利滚利递归、割肉自伤双乘）、2 个真逻辑 bug（吃水溢出、出千跨回合）**，外加一批约束④ 实例与注释漂移。没有需要动架构的问题。

---

## 一、红线冲突（约束④ / 分层 / R1-R8）

### 1.1 台规「正面 +3 金」硬编码在 Core（约束④，已知实例，仍未修）

- `GamblerMod/src/Core/GamblerHooks.cs:98-103`：`OnHeadsFlipped` 里写死 `PlayerCmd.GainGold(3, ...)`，数值散落三处（Core 硬编码 + `HouseRulesPower.cs` 描述 + `HouseRules.cs` 卡描述），改台规必须动 Core——正是约束④ 门禁要防的事。HANDOFF 待办里已记录。
- **附带 bug**：同一函数注释写「+2 金」（`GamblerHooks.cs:100`），代码给 3（3 = 设计正确值）。
- 建议：把 `OnHeads` 订阅挪进 `HouseRulesPower`（power 是内容层，数值随 power 文件走），Core 只保留事件。

### 1.2 Core→内容层反向依赖（分层规则违背，tech-design §4.6 设计的接口没落地）

- `GambleService.cs:54`：`await Relics.LuckyChip.TryIntervene(player)` —— Core 直调具体遗物。tech-design §4.6 设计的是 `IFlipIntervenor` 接口 + 干预窗口，落地时简化成了硬引用。
- 同类：Core 多处 `GetPower<Powers.Xxx>`（`GambleService.cs:78,111`、`SpendTracker.cs:38`、`GamblerHooks.cs:84,101`）。Power 类型引用属"结构性探测"，可辩护；但 `Relics.LuckyChip` 是纯业务内容，方向明确倒挂。
- 建议：Core 暴露干预事件（`Func<Player, Task<bool>>?`），`LuckyChip` 在内容层订阅。

### 1.3 利滚利防递归标记在 finally 里提前复位（高危，async 递归窗口未设防）

- `GamblerHooks.cs:87-95`：

```csharp
_inGoldGain = true;
try { return PlayerCmd.GainGold(compound.Amount, player, true); }
finally { _inGoldGain = false; }
```

- `finally` 在 `GainGold(...)` **返回 Task 的瞬间**执行，不等任务完成。若金币实际入账/`AfterGoldGained` 触发发生在命令执行层（首个 await 之后），守卫已失效 → 利滚利给自己发的钱再触发利滚利，**无限递归**（金币爆炸或栈溢出）。整个机制「尚未游戏内实测」（tech-multiplier §8），此路径从未跑过。
- 建议：改成 `await PlayerCmd.GainGold(...)` 包在 try 里，`finally { _inGoldGain = false; }` 之后 `return Task.CompletedTask;`。

### 1.4 出千「本回合」语义跨回合存续

- `CheatDeal.cs:17` `GuaranteedFlips = 20`，走 `GambleService.ForceNextFlips`；`_forcedHeads`（`GambleService.cs:32`）只在**战斗结束**清零（`OnCombatEnd`/`ResetIfNewCombat`）。当回合用不完的必正次数延续到下回合，与设计 #73「本回合」及卡面描述「（本回合）」不符。代码注释承认"复用暗记原语"，但暗记原语本来就该跨回合。
- 建议：要么在回合开始清 `_forcedHeads`（需 Core 动一行 + 门禁记录），要么描述改成"接下来 N 次"。

### 1.5 热路径 GD.Print 字符串插值（R1/R7 字面违反）

- `GamblerCard.cs:42,53,58`（每次出牌）、`GambleService.cs:49,67`（每次掷币）、`GoldCost.cs:29`、`LuckyChip.cs:36`——每次插值分配多个字符串。日志级开销，但按红线字面即违规。
- 建议：包一层 `#if DEBUG` 的日志开关，发布前摘除。

### 1.6 魔法数字散落在卡流程（约束④ 精神）

- `AddMore.cs:31` 每金 ×2、`LooseChange.cs:26` 每 2 张 ÷2、`CountMoney.cs:30` 每 5 金 ÷5——都写在 ApplyEffect 流程里，没进 `CanonicalVars`，且描述里也写死了同样数字。改数值要动两处。
- 建议：进 `IntVar("PerGold"/"Step"/"SpendStep")`，描述占位符化。

### 1.7 R2 本地化每次 new（判定：已知豁免，记录）

- 全库统一模式 `Localization => new CardLoc(...)`，每次访问新建列表。BaseLib 在初始化期消费一次，无热路径开销——**不修**，但若 BaseLib 未来变更消费频率需回看。

### 1.8 卡间分层（✅ 通过）

- 无任何卡文件引用其他卡文件；随机全走官方 Rng；无自建 Random；`Modify*Multiplicative` 热路径零分配。

---

## 二、明显错误（bug 级）

### 2.1 抽头升级值 14→16，设计与卡注释都写 18（全池唯一实锤数值 bug）

- `Rake.cs:30`：`HeadsGold.UpgradeValueBy(2)` → 14→**16**；设计 §5.2 #24「9 / 正面 **18**」（+4），文件头注释 `Rake.cs:13` 自己都写「升级 9 / 18」。§5.4 期望也随之偏低（升级后正面期望 12.5 金 vs 应有 13.5）。
- 修复：`UpgradeValueBy(4)`。

### 2.2 吃水「溢出转金」算错：不验击杀、不减格挡、忽略修正

- `TakeCut.cs:30-41`：`overflow = damage - hpBefore`。
  - 敌人 5 HP + 30 格挡 → 没打死也白拿 7 金（§5.5 归在「击杀产金」家族，击杀是前提）
  - 敌人 5 HP + 4 格挡 → 实际溢出 3，代码给 7（多算格挡）
  - 有力量/易伤时实际伤害 ≠ 12，少给或多给
- 建议：攻击前记格挡，攻击后判 `play.Target.IsDead`，溢出 = `max(0, 实际伤害 - hpBefore - blockBefore)`（击杀才给）。

### 2.3 押宝卡面描述缺正面分支（代码实现是对的）

- `DesperateThrow.cs:51`：描述只写「赌博：反面 → 自己失去 6 点生命」，**没写正面 → 改为 25 伤害**（`ApplyEffect:39` 正确实现了）。玩家看不到正面收益，卡片被低估。
- 附带：`DesperateThrow.cs:14` 头注「升级 22」应为 24（代码 +6）。

### 2.4 全下：目标类型矛盾 + 伤害疑似二次增长（需设计拍板）

- `ShoveAllIn.cs:19`：`TargetType.AnyEnemy` + `TargetingRandomOpponents` —— 要求玩家选目标但选择被忽略（同族随机牌 摇骰/俄罗斯轮盘 都用 `TargetType.RandomEnemy`）。
- 更重要的：`Attack(9 * x).WithHitCount(x)` → 每击 9×X、共 X 击，**总伤 9X²**（二次增长）。设计 #72 原文「对随机敌人造成 9×X 伤害，共 X 次」两种读法：若意图是总伤 9X（每击 9），应为 `Attack(9).WithHitCount(x)`。按 9X² 理解，X≥4 时反超梭哈且金费更便宜（7X vs 10X），强度曲线需复核。

### 2.5 割肉自伤疑似被倍率乘两次（高危，待游戏内验证）

- `GoForBroke.cs:32-33`：`selfHurt = ceil(Factor × 3)` 手动乘了一次，又以 `cardSource = this` 传入 `CreatureCmd.Damage`——本卡是风头卡，`GamblerHooks.cs:68-70` 会对 cardSource 为风头卡的伤害再乘 Factor。若引擎直接伤害也走该钩子（tech-multiplier §1 说"每次伤害计算"都过钩子），实际自伤 = `ceil(3F)·F`（x1.5 时 7.5 而非设计的 4.5→5；x2.0 时 12 而非 6）。格挡侧单次乘算是对的。
- 建议：游戏内打一张验证实际自伤值；若确认双乘，删掉手动 ceil 直接传 3 交给钩子（或改不带 cardSource 的重载）。

### 2.6 幸运筹码：先置「本回合已用」再扣款

- `LuckyChip.cs:34-35`：`_usedThisTurn = true` 在 `await LoseGold` 之前。扣款异常/被中断则白耗本回合次数。低概率，建议 try/catch 回滚或扣款成功后再置位。

### 2.7 反水：正面伤害随升级（9→11），设计升级列只写了格挡

- `Backhand.cs:35`：`HeadsDamage.UpgradeValueBy(2)`。设计 #62 升级列「11 / 6」= 正面格挡 11 / 反面格挡 6，没提正面伤害。待确认是否允许（若不允，删这行升级）。

### 2.8 加码「至多 3」封顶歧义

- `RakeFee.cs:29-31`：`energy = 1 + min(extra, cap-1)` → 总额至多 **3**。描述/设计「获得 1 点能量。每花费 10 金，额外 +1（至多 3）」自然读法：额外至多 3 → 总额 4。待设计拍板哪侧为准。

### 排除项（代理报过、主审核实为误报）

- **高利贷「余债豁免」**：设计原文「不足则全失」——扣到 0 就是正确行为，`GamblerHooks.cs:40-49` 无 bug。

---

## 三、显示/口径小问题（低优先级，不阻塞）

| # | 位置 | 问题 |
|---|---|---|
| 3.1 | `BankWind.cs:26` / `Redouble.cs:27` + `MultiplierService.cs:43,54` | 叠码/倍投直调 `AddHeads()` 会顺带写 `LastOutcome = Heads` → 掷出反面后再打这两张，徽章显示「正」（x1.5 对了、正反错了）；`Clear()` 不写 `LastOutcome` → 兑现后徽章残留上次正反。建议 Core 加不带归属写入的 `AddSteps(int)` |
| 3.2 | `CashChips.cs:22,35` | 换筹卡面渲染「（+0）」（`Bonus` 基值 0），观感怪 |
| 3.3 | `AllIn.cs:46` / `ShoveAllIn.cs:47` | X 卡金费是静态文案（10×X/7×X），升级后 GoldPerX 变 8/5，卡面仍显示旧价 |
| 3.4 | 全库 21 处 `PlayerCmd.GainGold(x, owner, true)` | 第三参语义是 `wasStolenBack`（"从敌人偷回的"），产金牌全部传 true 属复制粘贴；可能影响动效/统计，待游戏内看一眼 |
| 3.5 | `GamblerHooks.cs:102` | 台规 +3 金 fire-and-forget 未 await，多掷牌连正面时并发多次，异常被吞 |
| 3.6 | `GamblerHooks.cs:57` | `GoldCost.OnCombatStart()` 实际在 `AfterCombatEnd` 里调用——命名与调用时点相反，功能正确 |
| 3.7 | `GamblerCard.cs:62` | `KeepStreakAfterPlay` 复位依赖 OnPlay 走到末尾；ApplyEffect 中途异常会残留到下次打出（低概率） |

---

## 四、注释/口径漂移（逻辑正确，只改注释）

| # | 位置 | 现状 → 应为 |
|---|---|---|
| 4.1 | `GambleService.cs:15` + `:30,91,122` | 头注「记录按回合清空（AfterPlayerTurnStart 兜底重置）」+ `_round` 死代码——实际记录**整场战斗**，从未按回合清。`HeadsCountThisTurn`（:94-104）实为战斗级计数，满贯恰好依赖这个"误导语义"——**若未来有人按头注补齐回合清空，满贯会静默坏掉**。建议改名 `HeadsCountThisCombat` 或改注释 |
| 4.2 | `GambleService.cs:106` | `LastFlipHeads` 注释「（叠注消费）」过期——叠注已改用 `MultiplierService.Factor ≥ 1.5` |
| 4.3 | `GamblerCards.cs:13,35,72` | 档位注释「普通档 11 / 罕见档 19 / 稀有档 13」——应为 20 / 35 / 25 |
| 4.4 | `Entry.cs:24,36` | 「M5：51 卡全量」过期；「80 卡（起始2/普20/罕35/稀25）」四数相加是 82——80（常规）与 82（含起始 2）口径混用。统一为「2 起始 + 80 常规 = 82」 |
| 4.5 | `GamblerHooks.cs:100` | 台规注释「+2 金」→ +3 |
| 4.6 | 5 个卡文件的类名与卡名错位 | `FlashOfInsight`=盲注、`BuyTheRoad`=摸牌、`TripUp`=放水、`TallyUp`=保底、`RakeFee`=加码——文件头注释都有正确中文，但按卡名找文件很费劲，建议类名对齐卡名 |
| 4.7 | `AddMore.cs:16` / `DesperateThrow.cs:14` | 头注「每 1 金 +1 伤害」（实际 +2）/「升级 22」（实际 24） |

---

## 五、文档内部不一致（代码是对的，文档要改——不含本报告新引入）

| # | 位置 | 不一致 |
|---|---|---|
| 5.1 | `gambler-tech-multiplier.md` §3.1 | 「被赌神/暗记强制的、被幸运筹码改币的**都不计入**倍率」——与同文档 §5 验收表、§8 实现记录、`gambler-design.md` §2.8（赌神 ✅ 算、暗记 ✅ 算、改币 ❌ 归零，2026-10-16 用户拍板）**相反**。代码与后者一致。旧口径残留，应改 §3.1 |
| 5.2 | `docs/testing/test-plan.md:3,151` | 表头「v0.3.0（43 卡全量）」过期；「连庄 正面各 +6 格挡」应为 8（与设计/代码不符）。HANDOFF 待办已列 test-plan 为同步目标之一，仍未同步 |
| 5.3 | 设计侧拍板项 | 全下 9X² 强度曲线（§2.4）、加码「至多 3」歧义（§2.8）——需要设计明确后再对齐文档 |

---

## 六、建议的处理顺序（待你拍板，本次一律未动）

1. **高危先行**：利滚利 finally→await（1.3）；割肉自伤游戏内验证后再定（2.5）
2. **实锤数值**：Rake 升级 +2→+4（2.1）
3. **逻辑 bug**：吃水溢出（2.2）；出千跨回合（1.4，动 Core 一行需补门禁记录）
4. **红线清理**：台规移出 Core（1.1，顺带修注释）；Core→LuckyChip 解耦（1.2）
5. **批量小修**：三、四两节一次清完（多数只动注释）
6. **文档对齐**：tech-multiplier §3.1、test-plan（5.1/5.2）

---

## 附：审查通过清单（抽查确认无问题的项）

- 钩子签名与 `lib/sts2.xml` 逐一匹配（`ModifyDamage/BlockMultiplicative`、`AfterGoldGained`、`AfterPlayerTurnStart`、`AfterCombatEnd`）
- CanPlay 与 OnPlay 扣费口径一致；X 费卡（梭哈/全下）无双重扣费、判灰按 X=1
- `IsGambleCard` 与 `IsHeadsStreakCard` 全池互斥；追数击杀续风（`KeepStreakAfterPlay`）正确；只读条件卡（叠注/底牌）不挂词条
- 79 张卡（除 Rake/Backhand/RakeFee 歧义外）构造四元组、CanonicalVars 基础值、升级增量与设计文档一致
- 暗记层数/假筹免单/幸运筹码次数在战斗结束与回合开始正确复位
- 赌博期望：试手气 1.25 / 买定离手 1.19 / 盲注 1.20 / 连胜 8.75 等与 §5.4 吻合
