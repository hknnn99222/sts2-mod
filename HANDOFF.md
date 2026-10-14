# STS2 赌徒 Mod 项目交接摘要

> 供后续开发的智能体/协作者使用。更新于 2026-10-14，对应设计文档 v1.3 + 技术设计（已评审）、main 分支 `c52de05`。
> 前一版摘要（v1.2 时代）的历史教训已合并进本文。

## 项目概况

- **目标**：为 Slay the Spire 2 开发自定义角色"赌徒"（金币混合费用 + 掷硬币赌博）
- **仓库**：`D:\worlk\sts2-mod`，远程 `github.com:hknnn99222/sts2-mod.git`（main）
- **阶段**：**M0 已验收，M1 代码完成（待最终游戏内验收）**，里程碑 M0-M4 见技术设计 §6

## 当前状态（2026-10-14）

| 里程碑 | 状态 | 内容 |
|---|---|---|
| M0 骨架 | ✅ 验收通过 | GamblerMod 项目、BaseLib 3.4.7 引入、战士贴图占位角色进选人界面（HP72/99金/名字"赌徒"） |
| M1 核心管线 | ✅ 验收通过 | Core 基建 5 件套 + 私房钱/试手气 + 自有卡池 + 掷硬币视听反馈 |
| M2 做局 | ✅ 代码完成，待验收 | 幸运筹码遗物 + 暗记/手气正旺/赌神 + 干预窗口/正面率上限75% |
| M3/M4 卡池 | ✅ 代码完成，待验收（43/43 全量） | 普通档 11 + 罕见 19 + 稀有 13，含 X 费梭哈/真赌三件/引擎产金/全局钩子单例 |

**测试文档**：[docs/testing/test-plan.md](docs/testing/test-plan.md)（M1-M4 逐项验收清单 + 已知简化偏离列表）。

M1 验收清单（游戏内）：①私房钱 +4 真金 ②试手气掷硬币正反伤害（重击特效=正面）③飞牌扣 2 金、金币不足变灰 ④营火升级数值变化。**验收通过后：从起始牌移除飞牌（代码里有 TODO 标记）**。

## 已核实的事实（勿重复调研）

### 环境
- .NET SDK 9.0.318 在 **D:\dotnet**（`DOTNET_ROOT`、`NUGET_PACKAGES=D:\nuget-packages` 已配置）；shell 里 PATH 可能解析到 C 盘运行时，用全路径 `/d/dotnet/dotnet.exe` 最稳
- 构建即部署：`cd GamblerMod && dotnet build` 自动拷 dll+json 到游戏 mods 目录；**构建前必须关游戏**（dll 被锁部署会失败，编译仍成功——看报错是不是 MSB3027）
- 游戏日志（排障第一现场）：`%APPDATA%\SlayTheSpire2\logs\godot.log`
- BaseLib 3.4.7 源码本地克隆在 `/tmp/baselib-src`（临时目录，丢了重新 `git clone https://github.com/Alchyr/BaseLib-StS2.git`）；官方 ModTemplate 与 wiki 同理（`Alchyr/ModTemplate-StS2`，wiki 里有命令手册 Adding-Cards / Common-Commands-Cookbook 必读）

### 引擎/BaseLib 语义（全部踩坑或源码核实）
1. **内容注册 = 实例化即注册**：`Custom*Model` 构造器自动调 `AddCharacter/AddModel`。不要调 `RegisterType(typeof)`——它只登记去重，还会让构造器里的真注册被跳过
2. **原版模型全是 ModelDb 单例**：`new Ironclad()` 抛 DuplicateModelException。取实例用 `ModelDb.Card<T>()` / `ModelDb.CardPool<T>()` / `ModelDb.GetById<T>(ModelDb.GetId<T>())`
3. **卡池必须被角色引用**：自定义卡池挂了卡但没角色引用 = 孤儿池，卡牌实例化抛 "not in any card pool"，抽牌/牌组 UI 全崩
4. **SimpleLoc**：卡描述必须 `#` 开头才会把 `!D!`/`!Var!` 转数值（`*文字*` 金色高亮，升级文本 `-旧-+新+`）；不写 `#` 就原样显示占位符
5. **Godot res:// 区分大小写**：`PlaceholderID` 必须小写 `"ironclad"`（大写导致选人背景/转场加载失败，选人状态卡死）
6. **静态字段初始化陷阱**：模型类里不要放会触发异常的静态字段（TypeInitializationException 会连累整个类型，症状是"选人后 UI 不刷新"这类玄学）
7. 卡牌升级 API：`DynamicVars.X.UpgradeValueBy(n)`；改能量费 `EnergyCost.UpgradeBy(n)`
8. 卡牌效果写法：看 ModTemplate wiki 的 Common-Commands-Cookbook（DamageCmd/CreatureCmd/PlayerCmd/CardPileCmd 流式命令）
9. 掷硬币随机：官方"内容专属 Rng"构造器 `new Rng(player, contentId, mixin)`——同内容重见会出同结果，**必须用战斗序号做 mixin**（GambleService 已实现）
10. 金币：`Player.Gold`（int）、`PlayerCmd.GainGold(dec, player, bool)` / `LoseGold(dec, player, GoldLossType.Spent)`；可打出检查挂 `PlayerCombatState.HasEnoughResourcesFor` Postfix（与 BaseLib 附加费用同出口）

## 代码架构（技术设计 §3/§4 已冻结口径）

```
GamblerMod/src/
├── Entry.cs               # 唯一注册点：PatchAll → 卡牌实例 → 角色（顺序固定）
├── Core/                  # 共用层，约束④：改卡不碰这里
│   ├── GamblerCard.cs     # 模板基类：扣金→掷币→ApplyEffect(heads)；含正反面特效常量
│   ├── GoldCost.cs / GoldCostCheckPatch.cs / SpendTracker.cs
│   └── GambleService.cs   # 掷硬币管线（M2 做局干预窗口已留好）
├── Character/             # GamblerCharacter（占位）+ GamblerCardPool
└── Cards/                 # 每卡一文件 + GamblerCards 单例仓（新卡：写类→仓里加两行）
```

**加一张新卡的完整流程**：复制 PrivateStash.cs 改类名/构造四元组/CanonicalVars/OnUpgrade/ApplyEffect/Localization → GamblerCards.cs 加属性+Init 一行 → `dotnet build`（游戏关闭状态）。改数值只动卡文件（已演练验证）。

## 待办与已知问题

- [ ] M1 游戏内验收 → 移除飞牌出起始牌（`GamblerCharacter.cs` 有 TODO）
- [ ] M2：幸运筹码遗物（反面后花5金改正面，GambleService 干预窗口已留）、暗记/手气正旺/赌神、正面率加法上限75%
- [ ] 卡面立绘全是占位图（"NoPE"）——美术资产后补；正式硬币翻转 UI 动画同批
- [ ] SpendTracker 回合级清零未实现（M2 接遗物钩子时做）；多人战斗序号 mixin 的读档确定性 M4 校准
- [ ] 设计侧 TBD：角色显示名、初始遗物名（不阻塞）
- [ ] `docs/design/gambler-design.xlsx` 是被否决的 Excel 残留（未跟踪状态，用户待定删否）

## 历史事故（别重蹈）

- 改 PATH 一律用 PowerShell `[Environment]::SetEnvironmentVariable`，**禁用 setx + git-bash 变量展开**（曾污染注册表 PATH）
- 卡牌设计以 markdown 为唯一载体（Excel 方案已被用户否决）
- git-bash 里路径结尾 `\"` 会转义引号导致整条命令解析失败（用正斜杠或去掉尾反斜杠）
- MSB3027 部署失败 = 游戏没关（或残留进程），`taskkill //PID <pid> //F` 后重试
