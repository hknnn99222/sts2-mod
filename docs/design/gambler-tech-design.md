# 赌徒 Mod 技术设计文档

> 状态：**已评审（2026-10-14），按结论修订**｜依据：游戏设计 [gambler-design.md](gambler-design.md)（评审时 v1.3 / 卡池 43 张；其后经 v1.7 扩池、v1.8 词条化、v1.9 四轴、v1.10–v1.15 三档重调，**当前 v1.15 / 85 张常规卡**；本设计的分层与组件结构经这些版本沿用）｜API 调研完成于 2026-10-14（本机实测：sts2 v0.111.0 + BaseLib 3.4.7 工坊版反射导出）
> 本文档只做设计，不含实现代码；文中代码均为**签名草图**，用于评审接口形态。

## 0. 三条硬约束的落实位置（评审时重点看）

| 约束 | 落实章节 |
|---|---|
| ① 共用方法提取，不重复写 | §4 共用组件（GamblerCard 基类 + 4 个服务，76 张卡只写效果差异） |
| ② 减少内存分配（申请空间） | §5 分配红线（热路径零 new、集合复用、静态缓存） |
| ③ 多用成熟库，少自己写 | §2 依赖决策表（BaseLib 承担注册/本地化/关键词/UI 扩展，自写仅 5 个组件且均属"简单"级） |
| ④ **卡牌必改：公共类能不改就不改，改卡只改卡牌文件** | §3.1 变更影响半径规则 + §4.1 基类覆盖全部已知变体 + §4.8 Core 冻结门禁 |

---

## 1. 事实基础（已核实，勿再调研）

### 1.1 游戏原生 API（来自 lib/sts2.xml + 编译验证）

| 能力 | API | 备注 |
|---|---|---|
| 卡模型构造 | `CardModel.#ctor(int energyCost, CardType, CardRarity, TargetType, bool)` | 76 张卡全部走此构造 |
| 打出效果钩子 | `CardModel.OnPlay(PlayerChoiceContext, CardPlay)` | 异步管线 |
| 可打出检查 | `CardModel.CanPlay()` / `CanPlay(out UnplayableReason, ref AbstractModel)` | 金币不足 → 返回 UnplayableReason |
| 金币增减 | `PlayerCmd.GainGold(Decimal, Player, bool)` / `LoseGold(Decimal, Player, GoldLossType)` | `GoldLossType = Spent / Lost / Stolen`，挥金协同用 `Spent` |
| 玩家独立随机 | `Player.PlayerRng` → `Rng.NextBool()` | 掷硬币直接用，多人模式天然隔离 |
| 金色富文本 | `MegaCrit.Sts2.Core.RichTextTags.RichTextGold` | 卡面"金币费用/产金"文字标金 |
| 内容池注册 | `ModHelper.AddModelToPool<...>()` | BaseLib 已封装得更友好 |

### 1.2 BaseLib 3.4.7（工坊已装，NuGet: `Alchyr.Sts2.BaseLib`）

反射导出 411 个公开类型，与本 mod 相关的能力：

| 能力 | 类型 | 用途 |
|---|---|---|
| 自定义卡基类 | `Abstracts.CustomCardModel : CardModel` | 全部 45 卡的基类（提供 Localization、动态变量） |
| 自定义角色 | `Abstracts.CustomCharacterModel : CharacterModel` | 含 `StartingGold`（→99）、选人界面、能量球、贴图路径挂载 |
| 自定义遗物 | `Abstracts.CustomRelicModel : RelicModel` | 幸运筹码 + 扩展期 7 件 |
| 关键词注册 | `Patches.Content.CustomKeywords` + `KeywordPropertiesAttribute` | "赌博"词条高亮 + 悬停说明 |
| 卡面动态数字 | `MakeCalculatedDamage/Block/Var(...)` | 伤害/格挡数字随升级、力量实时计算（库内已做缓存） |
| 代码内本地化 | `Localization : List<(string,string)>` | 无 .pck 也能提供中英文卡名/描述 |
| 钩子聚合 | `Hooks.BaseLibHooks` + `IAfterSpendResource<T>` 等 | 战斗/回合钩子 |
| 节点池化 | Node pooling | UI 用，v1 基本不碰 |

### 1.3 明确不用的库能力（评审确认）

**`CustomResourceCost<T>` / `BasicCustomResource` 不用于金币费用。**
理由（反射实测）：泛型约束 `T : CustomResource`（无参构造），该体系是"战斗内第二资源条"（有 `Max`、`StartOfTurnReset`、每回合重置），而赌徒的设计支柱是**真金币互通**（跑图购物同一袋钱、战斗不重置）。强行套用需要重写重置/上限/显示三大行为，等于跟库对着干——违背约束③。
后果（已按 D3 拍板）：金币费用 UI 走"卡面描述文字 + RichTextGold"（文字记法方案）；**架构上预留"辉星式费用下方附加显示"扩展位**——GoldCost 的显示层与逻辑分离（§4.3），未来接 BaseLib `NAdditionalCostDisplay`（库自带的卡面费用下方附加显示机制，即辉星星费的同款管线），v1 不实现。

---

## 2. 依赖决策（约束③：多用成熟库）

### 2.1 承担方分配表

| 需求 | 承担方 | 自写量 |
|---|---|---|
| 卡/角色/遗物注册进游戏池 | BaseLib（Custom*Model + 注册管线） | 0 |
| 卡名/描述/中英文本 | BaseLib 代码内 Localization | 0（只写数据） |
| 卡面动态数字（伤害随力量等） | BaseLib MakeCalculated* | 0（只传委托） |
| "赌博"关键词高亮与悬浮说明 | BaseLib CustomKeywords | 数据 |
| 能量费用、稀有度、目标、升级 | 游戏原生 CardModel | 0 |
| 金币增减/花费类型 | 游戏原生 PlayerCmd | 0 |
| 掷硬币随机 | 游戏原生 PlayerRng.NextBool | 0 |
| **金币费用（检查+扣费+显示）** | 自写 `GoldCost`（见 §4.3） | ~60 行，"简单"级：一次比较 + 一次命令调用 + 一次字符串拼接 |
| **掷骰管线（含做局干预）** | 自写 `GambleService`（见 §4.2） | ~120 行，核心是纯函数 |
| **本回合花费累计** | 自写 `SpendTracker`（见 §4.4） | ~30 行，一个 int |
| **战斗状态仓** | 自写 `GamblerCombatState`（见 §4.5） | ~40 行，字段+重置 |
| 76 张卡效果 | 自写（不可避免） | 每张 5-20 行 |

### 2.2 引入方式与版本

- NuGet 包 `Alchyr.Sts2.BaseLib`（编译期引用，`Private=false` 不随包分发）；运行时由工坊订阅的 BaseLib 提供——**本机已装 3.4.7**，csproj 里 `dependencies` 加 `BaseLib min_version 3.4.7`（清单格式见 notes/modding-api.md）。
- **锁 3.4.x**：BaseLib 是活跃库，minor 升级可能破坏 API（风险 R3）。
- 自写组件总数 4 个（GoldCost / GambleService / SpendTracker / GamblerCombatState）+ 1 个基类，全部 <300 行，符合"少自己写，除非很简单"。

### 2.3 评审结论（2026-10-14 已拍板）

- **D1 项目名 ✅**：`MyMod` → `GamblerMod`（dll / 清单 / 命名空间三处同步，csproj 已预留说明）。
- **D2 角色视觉占位 ✅**：借用原版**战士（Ironclad）贴图**，选人/战斗/地图可完整跑通，美术后补只换贴图路径。
- **D3 金币费用记法 ✅**：v1 文字方案（`额外花费 X 金币`，RichTextGold 金色）+ **预留辉星式"费用下方显示"扩展位**（显示层与逻辑分离，未来接 `NAdditionalCostDisplay`，v1 不实现）。
- **D4 重掷语义 ✅**：**不做重掷机制**。删除「换骰」「操纵牌局」两张卡，不补新卡（当时卡池 45→43，游戏设计文档已同步 v1.3）。GambleService 无 Reroll API；掷骰记录仅作只读统计。

---

## 3. 总体架构与代码布局

```
GamblerMod/
├── GamblerMod.csproj          # 引用 lib/*.dll + NuGet BaseLib（Private=false）
├── GamblerMod.json            # 清单（id=GamblerMod，依赖 BaseLib）
└── src/
    ├── Entry.cs               # [ModInitializer] 注册角色/卡池/关键词/钩子（唯一注册点）
    ├── Core/                  # ===== 共用层（约束①的核心）=====
    │   ├── GamblerCard.cs     # 全部职业卡的抽象基类（模板方法）
    │   ├── GambleService.cs   # 掷骰管线 + 做局干预 + 掷骰记录
    │   ├── GoldCost.cs        # 金币费用：检查 / 扣费 / 文本
    │   ├── SpendTracker.cs    # 本回合花费累计
    │   └── CombatState.cs     # 战斗内状态仓（回合/战斗重置）
    ├── Character/
    │   └── GamblerCharacter.cs# CustomCharacterModel（HP72、起始148金、起始牌、初始遗物；视觉=战士贴图占位 D2）
    ├── Cards/
    │   ├── Basic/             # 起始 4 种（打击/防御/本钱/试手气）
    │   ├── Common/            # 11 普
    │   ├── Uncommon/          # 19 罕
    │   └── Rare/              # 13 稀
    └── Relics/
        └── LuckyChip.cs       # 幸运筹码（占位名）
```

分层规则：**内容层（Cards/Relics/Character）只许调用 Core，不许互相调用**；Core 不引用任何具体卡。注册全部集中在 Entry.cs。

### 3.1 变更影响半径（约束④：卡牌必改，基础要稳）

卡牌数值/效果后面一定会反复迭代，架构必须保证**改卡 = 只改一个卡牌文件**：

| 改动类型 | 允许碰的文件 | 碰 Core？ |
|---|---|---|
| 改数值（伤害/格挡/金币/费用/升级后数值） | 仅该卡文件 | 禁止 |
| 改效果写法（同钩子内换逻辑） | 仅该卡文件 | 禁止 |
| 改文案/本地化 | 仅该卡文件 | 禁止 |
| 新增一张现有钩子家族的卡 | 新增 1 个卡文件 + Entry 注册 1 行 | 禁止 |
| 全新钩子（现有 Core 原语组合不出来） | 先走 §4.8 门禁 | 需轻评审 |

保障手段：① 所有数值以**数据形式**写在卡构造器（不散落在流程里）；② 基类模板方法在 M1 就覆盖全部已知卡牌变体（见 §4.1 变体清单），之后卡牌怎么改都只是子类内变化；③ Core 接口 M1 冻结（§4.8）。

---

## 4. 共用组件设计（约束①）

### 4.1 `GamblerCard : CustomCardModel` —— 76 张卡的模板方法

把"金币费用 + 赌博"的公共流程固化在基类，子类只声明数据和效果：

```csharp
abstract class GamblerCard : CustomCardModel
{
    // ---- 子类在构造器里声明的数据（改数值只改这里，约束④）----
    protected int BaseGoldCost;      // 0 = 纯能量卡；含 X 金费/可选加投的变体见下方清单
    protected bool IsGambleCard;     // 是否带赌博词条

    // ---- 基类固定流程（子类不碰；M1 后冻结）----
    override CanPlay(...)            // 能量检查后追加：金币不足 → UnplayableReason
    override OnPlay(...)             // ①GoldCost.Pay ②若 IsGambleCard: Flip ③调 ApplyEffect(heads)

    // ---- 子类只实现这一个方法 ----
    protected abstract Task ApplyEffect(PlayerChoiceContext ctx, CardPlay play, bool heads);

    // 可选：升级差异（"-2~3金"或增强效果）走 OnUpgrade 钩子，每卡自己写
}
```

**基类必须覆盖的卡牌变体清单（M1 冻结前全部就位，之后改卡不再动基类）**：

| 变体 | 持有卡 | 基类/服务提供的支撑 |
|---|---|---|
| 固定金币附加费 | 双费 16 张 | `BaseGoldCost` 字段 + Pay/Check/CostText |
| X 金币费 | 梭哈（X能+10X金） | `GoldCostX` 数据 + ResolveX 桥接（复用引擎 X 费解析） |
| 可选加投（想花多少花多少） | 加注（至多 6 金） | GoldCost.BeginOptionalSpend 原语（打牌时选择投入额） |
| 赌博 | 13 张（v1.15 起无反向赌博：保险已改常规赌） | `IsGambleCard` + ApplyEffect(heads)；**该标志同时是「赌博」词条的挂载条件**（v1.8） |
| 真赌（反面惩罚） | 5 张 | 纯卡内逻辑，无特殊支撑 |
| 升级改金币费 | 按 §2.7 约定 | OnUpgrade 虚方法 + CostText(升级态) 缓存联动 |

清单之外的新形态（未来的新钩子）先查游戏设计 §5.5，确实组合不出来才走 §4.8 门禁动 Core。

一张卡的完整形态（示意，以「试手气」为例）：

```csharp
class TryYourLuck : GamblerCard
{
    public TryYourLuck() : base(1, CardType.Attack, CardRarity.Basic, TargetType.Enemy)
    { IsGambleCard = true; }
    protected override Task ApplyEffect(ctx, play, bool heads)
        => AttackCmd.Damage(heads ? 10 : 5, ...);   // 数字取自设计文档 §3
}
```

按此估算：76 张卡 × 平均 15 行 + 本地化数据；实测卡源码（含 using／注释／本地化声明）合计 3263 行，且**无一张卡需要知道"怎么扣金币/怎么掷硬币"**。

### 4.2 `GambleService` —— 掷骰管线（核心自写组件）

```csharp
static class GambleService
{
    // 掷一枚硬币：原始结果 → 做局干预窗口 → 最终结果
    // 干预顺序：赌神(视为正面) > 暗记(必正,消耗层数) > 幸运筹码(反面时询问:花5金改正面)
    static bool Flip(Player player, in FlipContext ctx);

    // 正面率 = 50% + Σ(做局修正, 加法, 上限 75%)；"必为正面"无视上限
    // 查询 player 身上实现 IGambleModifier 的 power（手气正旺/赌神）聚合而成
    static int HeadsRatePercent(Player player);

    // 本回合掷骰记录（只读统计，供 同花顺/底牌/台规/叠注 消费；无重掷 API——D4 已定）
    static IReadOnlyList<FlipRecord> FlipsThisTurn { get; }   // 复用 List，回合开始 Clear（§5 R3）
}

readonly struct FlipRecord { CardModel Source; bool OriginalHeads; bool FinalHeads; }
// Original/Final 之分来自做局干预（幸运筹码改币），非重掷；记录不可改写
```

- 随机源固定 `player.PlayerRng.NextBool()`（游戏原生，多人隔离，存档回放一致）。
- `FlipContext` 为 readonly struct（无分配，§5 R1）。
- 干预窗口同步执行（幸运筹码的"花 5 金改正面"发生在效果结算前，与游戏设计 §2.2 时点一致）。

### 4.3 `GoldCost` —— 金币费用三件事

```csharp
static class GoldCost
{
    // ① 检查（CanPlay 用）：金币不足返回不可打出原因
    static UnplayableReason? Check(Player player, int amount);
    // ② 扣费（OnPlay 用）：PlayerCmd.LoseGold(amount, player, GoldLossType.Spent)
    //    同时 SpendTracker.Record(amount)
    static void Pay(PlayerChoiceContext ctx, Player player, int amount);
    // ③ 文本：拼卡面费用行（RichTextGold），结果做静态缓存（§5 R4）
    static string CostText(int goldCost, bool upgraded);
}
```

假筹（下一次金币花费免单）：实现为 Pay 前查询的免单层数（存于战斗状态仓），不动 PlayerCmd。

### 4.4 `SpendTracker` —— 本回合花费累计

```csharp
static class SpendTracker
{
    static int SpentThisTurn(Player player);   // 连续下注 / 流水 消费
    static int SpentThisCombat(Player player); // 预留
    // 数据源 = GoldCost.Pay 的记录；回合开始归零（流水抽牌按累计阈值触发）
}
```

注意：金币不是引擎资源，`IAfterSpendResource<T>` 钩子**收不到**金币花费，所以必须自己记——这是自写它的唯一原因（库确实不覆盖）。

### 4.5 `GamblerCombatState` —— 战斗状态仓

集中存放所有战斗内临时量，统一生命周期：

```csharp
static class GamblerCombatState
{
    // 掷骰记录（List 复用）/ 本回合花费 / 假筹免单层数 / 暗记必正层数 / 幸运筹码本回合已用
    static void OnCombatStart();  // 订阅战斗钩子：Clear 全部，不 new（§5 R3）
    static void OnTurnStart();    // 回合级重置
}
```

订阅方式：`ModHelper.SubscribeForCombatStateHooks`（官方 API），钩子委托为**静态方法**（§5 R5）。

### 4.6 做局干预接口（能力卡/遗物实现）

```csharp
interface IGambleModifier   // 手气正旺(+15%) / 赌神(视为正面) 等 power 实现
{
    int HeadsRateBonus { get; }   // 加法，服务里统一钳制 75%
    bool TreatAllAsHeads { get; }
}
interface IFlipIntervenor   // 幸运筹码遗物实现（掷出反面后、结算前询问）
{
    bool ShouldIntervene(Player player, in FlipContext ctx);
}
```

### 4.7 D4 重掷——已按评审结论移除

「换骰」「操纵牌局」两张卡删除（不补新卡，当时卡池 43），GambleService 不提供 Reroll；`FlipRecord` 仅记录原始/干预后结果，供只读统计（同花顺计数、底牌条件、台规产金、叠注的"上一掷"查询）。游戏设计文档 v1.3 当时已同步。

### 4.8 Core 冻结与变更门禁（约束④的执行机制）

- **M1 验收通过即冻结 Core**（GamblerCard / GambleService / GoldCost / SpendTracker / GamblerCombatState 的公开接口）。
- 冻结后修改 Core 需满足：① 说明为什么卡牌子类内组合现有原语做不到；② 改动只加不改（优先新增虚方法/重载，不破坏既有签名）；③ 在本文档追加一行变更记录（日期 + 理由）。
- 改卡牌（数值/效果/文案/新增同家族卡）**不需要任何评审**，直接改卡文件 + 重编译。
- 每个里程碑的验收都包含一次"改卡演练"：随机挑一张卡改数值 → 确认只动了该卡文件 → 编译进游戏生效（见 §6）。

---

## 5. 分配与性能规范（约束②：减少申请空间）

**热路径定义**：`CanPlay`（鼠标悬停每帧）、`OnPlay`（每次出牌）、卡面文本刷新（升级/力量变化时）。以下为评审红线，新增代码逐条对照：

| # | 规则 |
|---|---|
| R1 | 热路径**零 new 引用类型**：传参用 `in`/readonly struct（`FlipContext`、`FlipRecord` 均为 readonly struct） |
| R2 | 卡模型、卡名/描述本地化列表、费用文本全部**静态只读**，初始化期构建一次 |
| R3 | 战斗内集合（掷骰记录等）**复用容器**：战斗/回合开始 `Clear()`，不重新分配 |
| R4 | `GoldCost.CostText` 结果按 (卡, 升级态) 缓存静态字符串；不做运行时 `$""` 拼接 |
| R5 | 事件/钩子订阅全部**静态方法组**，禁止 lambda 捕获（闭包=一次分配）；订阅一次、永不重复订阅 |
| R6 | 随机全走 `PlayerRng`，不自建 `Random`（每实例化一个 Random 就是一次分配 + 种子质量差） |
| R7 | 热路径禁 LINQ / `params` / 装箱（`Decimal`↔`int` 注意显式转换）；枚举集合用 `IReadOnlyList` for 循环 |
| R8 | 每张卡的动态数字（伤害随力量等）一律走 BaseLib `MakeCalculated*`（库内已缓存），不自建文本刷新 |

验收方式：M1 里程碑起，每次游戏内跑一局普通战斗，观察是否有 GC 卡顿；代码评审按本表逐条过。

---

## 6. 里程碑与验收（建议，评审可调）

| 里程碑 | 内容 | 游戏内验收标准 |
|---|---|---|
| M0 骨架 | 项目改名、引 BaseLib、Entry 注册、角色占位（D2）进选人界面 | 选人界面出现赌徒（占位视觉），能开一局，起始 148 金 |
| M1 核心管线 | GamblerCard + GoldCost + GambleService + 起始 4 种牌 | ①本钱加真金（商店同款钱）②试手气掷硬币、正/反伤害正确 ③金币不足卡变灰 ④掷骰记录可在日志查看 ⑤**改卡演练**：改试手气数值→只动 1 个卡文件→Core 零改动→重编译生效 |
| M2 做局 | 幸运筹码 + 暗记/手气正旺 + 赌神 | 反面后弹改币、暗记必正按层数消耗、正面率加法上限 75% 生效 |
| M3 卡池① | 普通档 11 张 + 本钱升级等 | 全部可玩、数值对照 §5 卡表抽查 |
| M4 卡池②③ | 罕见 19 + 稀有 13（真赌牌最后） | 43 卡全量可玩；一局完整跑通无报错 |

每个里程碑 = 一次 git 提交序列 + 游戏内实测截图/日志，通过后才进下一个。

---

## 7. 本地化与关键词（BaseLib 承担）

- 卡名/描述/角色名/遗物名：`CustomCardModel.Localization` 代码内双语（zh/en 键值对），v1 不做 .pck。
- "赌博"关键词：`CustomKeywords` 注册（高亮 + 悬停说明"打出时掷硬币：正面强化、反面基础"）。
- 真赌等变体在卡描述里用文字说明，不注册新关键词（v1 减负）。

## 8. 风险与备选

| # | 风险 | 缓解 |
|---|---|---|
| R1 | 占位视觉观感差 | D2 拍板；美术后补只换贴图路径 |
| R2 | RichTextGold 在卡面费用行的实际渲染效果未知 | M1 第一周用试手气验证，不行退化为纯文字"额外花费 5 金币" |
| R3 | BaseLib 升级破坏 API | 锁 3.4.x；升级需重跑 M1 验收 |
| R4 | 多人模式掷骰/改币时序 | PlayerRng 天然隔离；干预窗口为同步本地决策；M4 后加一次双人本地验证 |
| R5 | v2 想要辉星式"费用下方显示" | 已预留扩展位（D3）：GoldCost 显示层与逻辑分离，接 BaseLib `NAdditionalCostDisplay` |

## 9. 评审结论记录（2026-10-14）

- [x] **D1** 项目名 = `GamblerMod`
- [x] **D2** 角色视觉占位 = 战士（Ironclad）贴图
- [x] **D3** 金币费用记法 = 文字方案 + 预留辉星式"费用下方显示"扩展位（v1 不实现）
- [x] **D4** 重掷 = 不做；删「换骰」「操纵牌局」，不补新卡（卡池 45→43，两份文档已同步）
- [x] **追加约束④（2026-10-14）**：卡牌必改——公共类能不改就不改，改卡只改卡牌文件；Core M1 冻结 + 变更门禁（§3.1 / §4.8），每里程碑含改卡演练 |
- [x] 里程碑 M0-M4、BaseLib 锁 3.4.x、架构分层、共用组件接口、分配红线 —— 无异议，按本文档执行
- [ ] 游戏设计侧仍待定（不阻塞编码）：角色显示名、初始遗物名（当前占位"幸运筹码"）

**评审通过。下一步：按 §6 从 M0（项目骨架 + BaseLib 引入 + 战士占位角色进选人界面）开工。**


## 10. Core 变更记录（2026-10-16 代码审查修复批，§4.8 ③）

代码审查报告见 [code-review-2026-10-16.md](../testing/code-review-2026-10-16.md)。本批对 Core 的改动均为缺陷修复或"只加不改"的扩展，逐条说明理由：

| 文件 | 改动 | 理由（门禁①：为什么卡内做不了） |
|---|---|---|
| `GamblerHooks.cs` | ①利滚利 `AfterGoldGained` 的防递归守卫改为 `await` 包在 try/finally 内 | 原写法 finally 在 Task 完成前就复位守卫，金币入账发生在命令执行层时递归窗口重新敞开（潜在无限递归）——卡内无法修，属 Core 自身缺陷 |
| | ②移除台规 `OnHeadsFlipped` 及其订阅，迁移至 `Powers/HouseRulesPower`（Entry 注册） | 卡牌专属数值（+3 金）硬编码在 Core 违反约束④；数值与卡面同处内容层后，改台规只动 power 文件 |
| | ③`AfterPlayerTurnStart` 增加 `GambleService.OnTurnStart()` | 出千"本回合全正"需要回合级清空；回合钩子只在 Core |
| `GambleService.cs` | ④新增 `Intervenor` 委托 + `ForceNextFlipsThisTurn`/`OnTurnStart`，移除对 `Relics.LuckyChip` 的硬引用 | Core→内容层反向依赖违背分层规则；干预窗口做成注册点后新增做局遗物/卡不需再动 Core。**只加不改**：`ForceNextFlips`（暗记持久层）签名不动 |
| | ⑤`HeadsCountThisTurn` 更名 `HeadsCountThisCombat`；删死字段 `_round`；头注改"按整场战斗" | 记录从未按回合清空，原名/注释误导（未来按旧注释补回合清空会让满贯静默坏掉）。唯一消费者满贯同步更名 |
| `MultiplierService.cs` | ⑥新增 `AddSteps(int)`（不写 `LastOutcome`） | 叠码/倍投直接推进倍率是设计本意（做局·倍率注入），但 `AddHeads` 附带写"最近一次掷币归属"，会把正/反徽章写脏。**只加不改**：`AddHeads` 保留给掷币路径 |
| `GamblerCard.cs` | ⑦`KeepStreakAfterPlay` 复位移入 finally | 状态残留防护：ApplyEffect 中途异常时旧写法不复位，下一次打出会误判续风 |
| `GoldCost.cs` | ⑧`OnCombatStart()` 更名 `ResetForNextCombat()` | 旧名与调用点（AfterCombatEnd）语义相反；唯一调用点同步更新 |
| 内容层 | ⑨`Entry` 注册 Intervenor/OnHeads；`HouseRulesPower` 承接台规；`CheatDeal` 改用回合层原语；`BankWind`/`Redouble` 改用 `AddSteps`；`GrandSlam` 改用新名 | 均为上述 Core 改动的消费侧同步 |

**未改的口径（评估后保留）**：热路径 `GD.Print` 日志（M1-M5 验收的证据链，量级为每出牌 1-3 条、无性能实测问题，优先于 R1/R7 字面）；`GainGold` 第三参已全库修正为 `false`（wasStolenBack 语义，见审查报告 §3.4）。

## 11. Core / 内容层变更记录（2026-10-17「游玩体验优化」批，§4.8 ③）

本批把「赌博」「风头正盛」词条化、修掉赌博牌正面数值吃不到附魔的显示问题、让卡牌图鉴全可见、给头顶倍率徽章加悬停提示。按 §4.8 门禁逐条说明**为什么卡内做不了**：

| 文件 | 改动 | 理由（门禁①：为什么卡内做不到） |
|---|---|---|
| `Core/GamblerKeywords.cs`（新增） | `[CustomEnum]` 两个 `CardKeyword`（`Gamble`/`HotStreak`）+ 一个 `StaticHoverTip`（`Multiplier`） | 自定义词条只能注册成**类型上的静态字段**（BaseLib 反射扫描后写入枚举值），卡文件里没有可声明的载体；词条文案走 pck 的 `GamblerMod/localization/zhs/card_keywords.json` 与 `static_hover_tips.json` |
| `Core/GamblerCard.cs` | ①新增 `CanonicalKeywords` override：按 `IsGambleCard` / `IsHeadsStreakCard` 挂对应词条；②新增 `LogValueCheck()` | ① 词条是**模型固有属性**，在基类一处按既有标志派生，胜过 24 张卡各写一遍；**不能用 `AfterCreated` + `AddKeyword`**——那个钩子的语义是"卡**实例**创建后"（`sts2.xml` CardModel.AfterCreated），图鉴/牌堆里的模型本体拿不到词条。② 附魔与倍率的"卡面值 vs 结算值"需要日志证据链（与既有掷币日志同量级） |
| 14 张赌博牌 + 满贯/叠注/翻盘的正反面数值变量 | `IntVar` → **引擎原生 `DamageVar`/`BlockVar` 的 `(名字, 数值, ValueProp)` 重载**（`new DamageVar("HeadsDamage", 10, ValueProp.Move)`） | 附魔与力量/虚弱/易伤只作用于**引擎认得的变量类型**：写在 `IntVar` 上的正/反面数值卡面永远显示裸值（游戏内实测：正面数字不吃增伤、不吃虚弱，与结算对不上）。原生重载一步到位拿到与标准 `!D!` 一致的完整预览管线，**比自写 `UpdateCardPreview` 更少代码、更不易错**——本批第一版曾用自定义 `EnchantAwareVar`（照 BaseLib `CustomExtraDamageVar` 手算附魔），实测后废弃 |
| `Core/CardValueVars.cs`（新增） | 仅保留 `MultiplierAwareVar`（金币 × 倍率显示用） | 金币是引擎没有修饰管线的资源（无附魔/力量通道），只能自定义变量 + 自写 `UpdateCardPreview`；名字走 `IntVar(string, decimal)` |
| `UI/MultiplierBadge.cs` 悬停 | `NHoverTipSet.CreateAndShow` 后必须 `SetAlignment(this, HoverTipAlignment.Center)` + `SetFollowOwner()` | 只 CreateAndShow 不给对齐 → 提示节点停在 (0,0)，实测表现为"提示跑到屏幕左上角" |
| `Character/GamblerCardPool.cs` | `SeenByDefault => true` | 图鉴（百科）可见性由 BaseLib 的 `CustomCardPoolMarkAsSeenPatch` 以该属性为开关（默认 false → 76 张全渲染成"未知/未解锁"）；只能在池定义处覆写 |
| `UI/MultiplierBadge.cs` | `MouseFilter` `Ignore` → `Pass`；接 `MouseEntered/Exited` + `NHoverTipSet` 弹自定义静态提示 | 徽章是战斗 UI 节点，改的是**鼠标热区与提示接线**（`Pass` = 自己收事件、事件继续上传给父节点，点角色不受影响），不涉及 Core 语义 |
| 内容层同步 | 删 4 张自掷牌（豹子/老虎机/连庄/连胜）+ 注册行；14 张赌博牌描述改「硬币为正/为负」句式、数值变量换 `EnchantAwareVar`；10 张风头牌删与自动词条重复的 `*风头正盛*` 字头；9 张不挂词条的牌把正文里的词条名降为普通文字；兑现的 `GoldGain` 换 `MultiplierAwareVar`（卡面显示当前倍率下的实际到账值） | 均为上述 Core 改动的消费侧同步；描述文案里的 `!Var!` 只是取值出口，数值语义未变 |

**删卡理由（游戏设计侧同步记录在 gambler-design.md §5.3 v1.8 说明）**：豹子/老虎机/连庄/连胜都绕开基类单掷流程自行掷币（`GambleService.Flip` 写在 `ApplyEffect` 的循环里），因此 ①挂不上「赌博」词条（词条挂载条件就是 `IsGambleCard`）；②一次出牌里多次推进倍率、覆盖徽章正反。用户决定整形态退场，卡池 80 → 76。

**未改的口径**：`GambleService.Flip` 管线、`MultiplierService` 刻度语义、`GoldCost`、伤害/格挡钩子（`GamblerHooks.ModifyDamage/BlockMultiplicative`）本批一行未动。

## 12. Core / 内容层变更记录（2026-10-17 v1.9 四轴改造批，§4.8 ③）

本批把卡池从"一条博弈链"重排成四条独立引擎（设计 §2.9），新增 9 张卡。按 §4.8 门禁逐条说明：

| 文件 | 改动 | 理由（门禁①：为什么卡内做不到） |
|---|---|---|
| `Core/GambleService.cs` | 新增 `ForcedHeadsRemaining`（只读总数）与 `ConsumeForcedHeads()`（取走并清零） | 必正层数是 Core 私有字段（暗记的持久层 + 出千/瞒天过海的回合层），卡牌既读不到也清不掉；出千轴的兑现牌「收网」必须"一次取空"才能成立。**只加不改**：`ForceNextFlips` / `Flip` 的扣层顺序一行未动 |
| `Powers/BettingPower.cs`（新增） | 重写 `ModifyDamageAdditive`：伤害 +「已失去生命」的 Amount% | 赌命轴放大器。走伤害加法钩子（与力量同一条通道）而不是自己攒层数：①加成要随血线实时变化；②卡面预览同步（玩家看到的就是实际值）；③绕开 `PowerModel.Amount` 的 setter 是私有的（power 内部改不了自己的层数）。自伤（target == 自己）不吃加成 |
| `Powers/CheatKingPower.cs`（新增） | 订阅 `GambleService.OnHeads`：每回合前 Amount 次正面 → `ForceNextFlips(1)` | 出千轴生成器的"自动化"部分。**必须每回合封顶**（`_usedThisTurn`，回合开始清零），否则"必正→正面→千王再给必正"会变成无限循环。订阅走 Entry 静态方法组（红线 R5） |
| `Entry.cs` | 追加 `OnHeads += CheatKingPower.OnHeadsFlipped`；`GamblerPowers` 注册 2 个新力量；版本 v0.6.0 | 内容层订阅点（与台规同款） |
| 内容层 | 新增 9 张卡（出千 5 / 赌命 4）+ `GamblerCards` 注册；暗记 1→2 次必正；封口改为"1 次必正 + 抽 1"（与暗记错开定位） | 四轴的生成器/兑现器必须落在内容层；暗记与封口同为必正生成器，用"次数/附带抽牌"区分稀有度 |

**未改的口径**：`GambleService.Flip` 的干预顺序与正面率、`MultiplierService` 刻度语义、`GoldCost`、`GamblerHooks.ModifyDamage/BlockMultiplicative` 一行未动。

**已知风险 / 待验收**：
1. `BettingPower` 的加成同时作用于所有来源的伤害（含技能牌），验收时确认卡面数字与实际一致；
2. `CheatKingPower` 的每回合计数在 power 实例上（与 Core 的静态掷币状态同口径）——多人隔离仍待做；
3. `收网` 在必正为 0 时无事发生（描述已写明），验收确认不会空挥消耗能量。

### 12.1 v1.12/v1.13 罕见档批次的 Core 变更（2026-10-21，§4.8 ③）

| 文件 | 改动 | 理由 |
|---|---|---|
| `Core/GamblerHooks.cs` | 删除 `PendingLoanDebt` 字段与回合开始的扣款分支 | 高利贷（#27）由「技能·下场战斗扣款」改为「能力·每回合利息」，欠款逻辑移入内容层 `Powers/LoanInterestPower`（订阅回合开始钩子）。旧代码已无调用方，保留会变成死状态 |
| `Powers/LoanInterestPower.cs`（新增） | 每回合开始失去 Amount 金币（不足则扣光） | 卡内无法订阅回合开始；按 §4.8 的分层，能力本体放内容层 |
| `Powers/DividendPower.cs`（v1.10 已有） | 未改 | — |

**未改的口径**：`GambleService` 的必正接口（收网改为只读 `ForcedHeadsRemaining`，不消耗）、`SpendTracker`、`MultiplierService`、`GoldCost` 的行文未动。

### 12.2 v1.15 稀有档批次的 Core 变更（2026-10-21，§4.8 ③）

| 文件 | 改动 | 理由 |
|---|---|---|
| `Core/SpendTracker.cs` | `Record` 里新增「保本」结算：持有 `Powers/CapitalGuardPower` 时，按本次花费额施加等量敏捷（升级版再加等量力量） | 引擎没有"金币已花费"钩子（只有 AfterGoldGained），而保本要实时、按每一笔花费触发。`SpendTracker.Record` 是全局唯一的金币花费记账点（与流水抽牌同款做法：直接探测内容层力量） |
| `Powers/RoulettePower.cs`（新增） | 每回合开始随机自伤/打敌人，独立 Rng mixin | 能力型轮盘；随机源必须可复现（读档后同回合序列一致） |
| `Powers/ShowdownPower.cs`（新增） | `AfterModifyingGoldGained(player, amount)` → 获得 amount + 层数 的格挡 | 晒冷要"等量"而不是固定值，必须拿钩子里的 amount 参数 |
| `Powers/CapitalGuardPower.cs`（新增） | 仅持有档位（0/1），逻辑在 SpendTracker | 同上，卡内无法订阅花费 |

**未改的口径**：`GoldCost`、`GambleService`、`MultiplierService`、`GamblerHooks` 一行未动。
