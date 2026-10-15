# 「风头正盛」倍率系统 · 技术设计（v1.6 新增机制的实现设计）

> 状态：**已实现并编译通过（2026-10-16）**，待游戏内验收｜依据：游戏设计 [gambler-design.md](gambler-design.md) §2.8 / §5（v1.6）｜延续 [gambler-tech-design.md](gambler-tech-design.md) 的全部约定
> 本文档只做设计，不含实现代码；文中代码均为**签名草图**，用于评审接口形态。
> **落地与设计的差异见 §8 实现记录**（含一处待游戏内校准的常量）。

## 0. 先说约束④的门禁（本机制必须过这一关）

本机制**不是**"现有 Core 原语能组合出来的卡"，它需要新增一个钩子家族。按既有技术设计 **§3.1 / §4.8**，动 Core 前必须满足三条，逐条答复：

| 门禁条款 | 本机制的答复 |
|---|---|
| ① 为什么卡牌子类内组合现有原语做不到 | 倍率要在**任何**带词条的牌结算时改数值，且要改**卡面预览数字**。卡类内只能改自己的效果，无法拦截"所有带词条的牌"，也无法影响预览渲染——必须走引擎的伤害/格挡修改钩子（只有 Hook 监听者能实现）。 |
| ② 改动只加不改 | 不修改任何既有签名：新增 1 个静态服务（`MultiplierService`）+ 1 个纯显示用 Power；在既有 `GamblerHooks` 上**追加** 3 个 override（新增方法，不动既有）；`GamblerCard` **追加** 1 个数据字段 + 1 个可空清空开关（不动 `OnPlay` 既有分支语义，只在末尾追加"清空"一步）。 |
| ③ 追加变更记录 | 见 §7 变更记录（已补）。 |

结论：**属于"只加不改"的轻量 Core 扩展**，不触碰 M1 冻结的既有接口形态。

---

## 1. 事实基础（三个子代理查证，2026-10-15/16；勿再调研）

| 事实 | 结论 | 证据 |
|---|---|---|
| 倍率能否非整数存储 | 能。`PowerCmd.Apply(..., System.Decimal 层数, ...)`、`PowerModel.Amount` 相关 API 全是 `decimal` | `lib/sts2.xml` 的 `PowerCmd.*` 签名；本项目 `notes/modding-api.md` §6 |
| 改伤害/格挡的钩子 | `AbstractModel.ModifyDamageMultiplicative(target, amount, props, dealer, **cardSource**, cardPlay)`、`ModifyBlockMultiplicative(...)`——**都带 cardSource**，可只对带词条的牌生效 | `lib/sts2.xml`（`ModifyDamageMultiplicative` 文档："Use this for effects that multiply existing damage…like VulnerablePower"） |
| 卡面预览会不会跟着变 | **会**。`DamageVar/BlockVar.UpdateCardPreview` → `Hook.ModifyDamage`/`Hook.ModifyBlock` → 生物身上 Power/Hook 的 `Modify*Multiplicative` → 写进 `PreviewValue`。**无需自己接管预览渲染** | `sts2.dll` IL 反汇编（`BlockVar.UpdateCardPreview`）+ `DynamicVar.UpdateCardPreview` 文档原文"so powers and relic modifications to these values can be reflected in card descriptions" |
| 预览的生效范围 | 仅**战斗中且牌在 Hand/Play 堆**（`runGlobalHooks` 门控）；牌库浏览/战斗外显示裸值 | 同上 IL 门控；预览时 `cardPlay` 恒为 `null`（不要在钩子里解引用） |
| 金币钩子有没有 cardSource | **没有**。`ModifyGoldGained(Player, decimal)` 不带卡来源 → **无法靠钩子判断"这次加金是哪张牌"** | `lib/sts2.xml` 签名 |
| BaseLib 每卡通道 | `CardModifier.ModifyBaseDamageMultiplicative`（每卡、含预览）可用；但 `ModifyBaseBlock*` 标了 `[Obsolete("Not currently functional.")]`（`ModifyBaseDamagePatches.cs` 里 `//TODO - Patches for block vars`）→ **不用它**，统一走引擎全局钩子 | BaseLib 源码 |
| 头顶显示非整数 | 引擎无现成能力（Power 图标原生只显示整数层数）；BaseLib `AddedNode<TParent, TNode>` 可给所有 `NCreature` 挂自定义子节点 | BaseLib `Utils/SpireField.cs:303`；`Patches/UI/ExtraCardUi.cs` 同款用法 |

---

## 2. 方案总览

```
                     ┌───────────────────────────────┐
   掷硬币（GambleService.Flip）│  MultiplierService（Core，静态真源）│
   ├ 正面 → AddHeads() ──────▶│  Steps（int，单位 0.5）        │
   └ 反面 → Reset()   ──────▶│  Factor = Steps * 0.5m        │
                     └───────────────┬───────────────┘
                                     │ 读 Factor
   ┌─────────────────────────────────┴──────────────────────────┐
   │ GamblerHooks（既有战斗钩子单例，追加 3 个 override）           │
   │  ModifyDamageMultiplicative / ModifyBlockMultiplicative      │
   │    → cardSource 带「风头正盛」？返回 Factor : 1m              │
   └────────────────────────────────────────────────────────────┘
   ┌────────────────────────────────────────────────────────────┐
   │ 卡面（自动）：DamageVar/BlockVar 预览 → 钩子 → 显示乘后数字    │
   │ 金币（无卡来源钩子）：卡内自算 7 * Factor + MakeCalculatedVar  │
   └────────────────────────────────────────────────────────────┘
   ┌────────────────────────────────────────────────────────────┐
   │ 头顶徽章：AddedNode<NCreature, MultiplierBadge> 显示 x{Factor}│
   └────────────────────────────────────────────────────────────┘
```

---

## 3. 组件设计

### 3.1 `Core/MultiplierService`（新增，静态真源）

```csharp
static class MultiplierService
{
    // 内部以 int 计"0.5 的个数"（R7：避免热路径 decimal 运算；1 = x1.0，3 = x1.5）
    static int Steps { get; }
    static decimal Factor { get; }        // = Steps * 0.5m

    static void AddHeads();               // Steps++（掷出正面时由 GambleService 调）
    static void Reset();                  // Steps = 2（掷出反面 / 战斗结束）
    static void Clear();                  // 兑现清空（语义与 Reset 分开，便于日志）
    static void OnCombatStart();          // Steps = 2
}
```

- **订阅方式**（R5）：`GambleService` 在 `Flip` 内部**直接调用** `MultiplierService.AddHeads()/Reset()`，不用事件（少一层间接）；需要"被做局改成正面的不算"的判定在 `GambleService.Flip` 里用 `FlipRecord.OriginalHeads` 区分——**这是本机制对 Core 的唯一必要侵入点，且只是加两行调用**。
- 判定规则（设计 §2.8）：只有**带「赌博」词条**的牌的**原始结果**为正面才 `AddHeads`；被赌神/暗记强制的、被幸运筹码改币的**都不计入**（用 `OriginalHeads` 与"是否被干预"两个标志区分——既有 `FlipRecord` 已同时记录 `OriginalHeads`/`FinalHeads`，够用）。

### 3.2 `Powers/HeadsStreakPower`（新增，**只用于显示**）

- `PowerStackType.Counter`（Counter 才显示数字），`Amount` = **Steps**（即 x1.0 显示为 2）。
- 之所以还要这个 Power：power 栏有图标 + tooltip 是可读性的兜底（头顶徽章是主显示）。
- **可选**：v1 可先不做，只做头顶徽章；若做，用 BaseLib `IHasSecondAmount` 把第二行标签写成 `x1.5`（覆盖 `Amount` 的整数显示）。

### 3.3 `Core/GamblerHooks`（既有，**追加** 3 个 override）

```csharp
// 既有类上追加（§4.8 ②：只加不改）
public override decimal ModifyDamageMultiplicative(Creature target, decimal amount, ValueProp props,
        Creature dealer, CardModel cardSource, CardPlay cardPlay)
    => cardSource is GamblerCard { IsHeadsStreakCard: true } ? MultiplierService.Factor : 1m;

public override decimal ModifyBlockMultiplicative(Creature target, decimal amount, ValueProp props,
        CardModel cardSource, CardPlay cardPlay)
    => cardSource is GamblerCard { IsHeadsStreakCard: true } ? MultiplierService.Factor : 1m;
```

- **热路径红线**：这两个方法在**每次伤害/格挡计算 + 每次卡面预览刷新**时都会被调用（R1/R7）。实现必须是"一次类型判断 + 一次属性读 + 一次乘法"，**零分配、零 LINQ、零字符串比较**——所以词条判定用 `GamblerCard` 上的 **bool 字段**，绝不去比 tag 字符串。
- `ModifyGoldGained` **不用**（无 cardSource，见 §1）——金币走卡内自算（§3.4）。
- 预览时 `cardPlay == null`，本实现不解引用它。

### 3.4 金币类兑现牌（兑现 / 割禾青）的写法

引擎金币钩子拿不到卡来源，所以这两张**在卡内自己算**：

```csharp
// 兑现：卡内自算 + 用 BaseLib MakeCalculatedVar 让卡面显示乘后值（R8）
protected override Task ApplyEffect(ctx, play, heads)
    => PlayerCmd.GainGold(7 * MultiplierService.Factor, Owner, true);
```

- 卡面数字：用 `MakeCalculatedVar`（BaseLib 的"计算型变量"，库内已做缓存）把显示值绑到 `7 * MultiplierService.Factor`，这样卡面也会跟着倍率变——**注意**：DisplayVar 这类"文本型变量"的数字是从显示字符串反解的，别用它做真实计算（数值一律走 `MultiplierService.Factor`）。
- 「割禾青」抽牌数同理由卡内自算：`Math.Min(3, (int)(MultiplierService.Factor * 2))`。

### 3.5 `Core/GamblerCard`（既有基类，**追加**两个成员）

```csharp
protected bool IsHeadsStreakCard;        // 数据字段（类比 IsGambleCard），子类构造器里设
protected bool KeepStreakAfterPlay;      // 「追数」击杀时置 true，跳过清空

// OnPlay 末尾追加（既有逻辑一行不动）：
if (IsHeadsStreakCard && !KeepStreakAfterPlay) MultiplierService.Clear();
KeepStreakAfterPlay = false;             // 下次默认清空，防状态残留
```

- 这样"结算后清空"只在基类写一次，8 张新卡各自的 `ApplyEffect` 里只管效果（约束①/④）。
- 「追数」在 `ApplyEffect` 里检测击杀后 `KeepStreakAfterPlay = true`。

### 3.6 头顶徽章（新增 UI，`Relics`/`UI` 层）

```csharp
// 挂在所有 NCreature 上的自定义子节点（BaseLib AddedNode）
public static readonly AddedNode<NCreature, MultiplierBadge> Badge = new(_ => new MultiplierBadge());

sealed class MultiplierBadge : Control      // 纯文字版：一个 MegaLabel 显示 "x1.5"
{
    // _Ready：定位到头顶偏移（在 NCreature 本地坐标上抬一圈）
    // 订阅 MultiplierService 的变化（静态方法组，R5）刷新文本；不在战斗/非玩家生物时隐藏
}
```

- **已知不确定项**：`NCreature` 的本地坐标原点与"头顶"偏移量需要**游戏内实测**（第一次跑通时用日志打印节点位置校准）。v1 只做纯文字（设计已定"美术后补"）。
- 若 `AddedNode<NCreature, …>` 的挂载点在实战中不稳定，退路是 §1 事实表里的第②条：复用 `%OrbPos` 挂点做一颗"筹码球"。

### 3.7 卡牌层（内容层，只碰卡文件 + 单例仓）

- **8 张新卡**（`Cards/Common|Uncommon|Rare/` 各加文件）：走既有加卡流程——复制同档现有卡 → 改类名/构造四元组/`IsHeadsStreakCard = true`/`CanonicalVars`/`OnUpgrade`/`ApplyEffect`/`Localization` → `GamblerCards.cs` 加属性 + `Init` 一行。
- **3 张改造**（只动这 3 个卡文件）：
  - `叠注`：条件从"上一掷正面"改为"`MultiplierService.Factor >= 1.5m`"（不设 `IsHeadsStreakCard`，不乘、不清空）。
  - `底牌`：条件从"本回合掷出过正面"改为"`Factor >= 1.5m`"（同上，只读）。原"抽到的牌降费"设计随之作废。
  - `同花顺`：设 `IsHeadsStreakCard = true`；删掉 `HotDamage/HeadsNeeded` 条件分支，只留 `43 伤 ×倍率`（`HotDamage` 变量随之删除）。
- **词条注册**：「风头正盛」按「赌博」的同款做法走 BaseLib `CustomKeywords`（高亮 + 悬停说明"数值乘以头顶倍率，结算后清空"）；真赌那种纯文字说明的做法不变。

---

## 4. 分配红线对照（约束②，逐条自查）

| # | 规则 | 本机制落实 |
|---|---|---|
| R1 | 热路径零 new | `Modify*Multiplicative` 全程 struct/decimal 运算，无委托、无闭包 |
| R2 | 静态只读数据初始化期构建 | 徽章文本模板、`x{}` 拼接策略在初始化期定好 |
| R3 | 战斗内集合复用 | `MultiplierService` 只有两个 int，无集合 |
| R4 | 文本缓存 | 徽章文本按"Steps 变化"才刷新（订阅式），不是每帧 |
| R5 | 订阅用静态方法组 | 徽章订阅 `MultiplierService` 变化用静态方法；`GambleService` 直接调用 `AddHeads/Reset`，不走事件 |
| R6 | 随机全走 PlayerRng | 本机制不引入随机 |
| R7 | 热路径禁 LINQ/装箱 | 词条判定用 `bool` 字段（非字符串/非 tag 查询）；倍率内部用 `int` Steps，仅在钩子出口做一次 `* 0.5m` |
| R8 | 动态数字走 BaseLib 计算型变量 | 金币/抽牌数用 `MakeCalculatedVar`，不自建刷新 |

---

## 5. 里程碑与验收

**建议作为 M5（独立里程碑，不阻塞 M2–M4 的游戏内验收）**：

| 验收项 | 预期 |
|---|---|
| 倍率显示 | 进战斗头顶显示 `x1.0`；打一张赌博牌掷出正面 → `x1.5`；掷出反面 → 立刻回 `x1.0` |
| 跨回合保留 | 上回合滚到 `x2.5`，本回合开始仍是 `x2.5`（战斗结束才清） |
| 卡面同步 | 手牌里带「风头正盛」的牌，其伤害/格挡数字**随倍率实时变化**（和力量改伤害同款表现） |
| 结算后清空 | 打出带词条的牌 → 数值吃满倍率 → 打完倍率回 `x1.0` |
| 只算赌博牌 | 非赌博牌的掷币/做局强制正面不涨倍率；幸运筹码改币不涨；赌神/暗记涨 |
| 3 张改造 | 叠注/底牌 按 `倍率 ≥1.5` 触发；同花顺 43 伤 ×倍率、无额外条件 |
| 挖矿演练 | 只改 1 个卡文件数值 → 不碰 Core → 重编译生效（约束④的标准动作） |

---

## 6. 风险与备选

| # | 风险 | 缓解 |
|---|---|---|
| M1 | 头顶徽章的定位/挂载点需实测 | 先纯文字 + 日志校准；退路是复用 `%OrbPos` 做筹码球 |
| M2 | 金币无 cardSource → 卡面预览要自己接 | 用 BaseLib `MakeCalculatedVar`；若该变量不支持金币上下文，退化为"卡面显示裸值 + tooltip 说明" |
| M3 | 预览只在 Hand/Play 堆生效 | 设计已接受（原版力量同款行为） |
| M4 | 倍率不设上限 + 金币永久 → 经济可被击穿 | 用户已明确接受（设计 §2.8 已记录），不再劝阻；若日后反悔，最小改动是在 `ModifyDamage/BlockMultiplicative` 出口夹一个 `Math.Min` |
| M5 | Core 侵入点（`Flip` 内两行调用）若后续要移除 | 调用点集中在 `GambleService.Flip` 一处，回退成本低 |

---

## 7. 变更记录（§4.8 ③ 要求的追加记录）

- **2026-10-16**｜新增「风头正盛」倍率系统。Core 追加：`Core/MultiplierService.cs`（新文件）、`GamblerHooks` 追加 `ModifyDamageMultiplicative`/`ModifyBlockMultiplicative` 两个 override、`GamblerCard` 追加 `IsHeadsStreakCard`/`KeepStreakAfterPlay` 两个字段与 `OnPlay` 末尾一步清空、`GambleService.Flip` 内追加两行倍率推进调用。**未修改任何既有签名**。理由：倍率需拦截所有带词条牌的数值与卡面预览，卡牌子类内无法实现（见 §0 门禁①）。

---

## 8. 实现记录（2026-10-16，已落地并编译通过）

**改动文件清单**（与 §3 设计一致，仅下列"落地偏离"）

- Core：`MultiplierService.cs`（新）、`GamblerCard.cs`、`GamblerHooks.cs`、`GambleService.cs`
- 新卡 8 张：`Common/HoldBack`（收手）、`Common/RideTheWin`（乘胜）、`Uncommon/QuitWhileAhead`（割禾青）、`Uncommon/SettleUp`（兑现）、`Uncommon/TheFinale`（通杀）、`Rare/AllInToTheEnd`（晒冷）、`Rare/ComebackRound`（追数）、`Rare/GoForBroke`（割肉）
- 改造 3 张：`Uncommon/Parlay`（叠注）、`Rare/HoleCard`（底牌）、`Rare/Flush`（同花顺）
- UI：`UI/MultiplierBadge.cs`（新）；注册：`Cards/GamblerCards.cs`、`Entry.cs`（版本 0.4.0）

### 落地偏离（与 §3 设计的差异，均已在下文标注原因）

| 项 | 设计 | 实际落地 | 原因 |
|---|---|---|---|
| 显示用 Power | `HeadsStreakPower`（可选） | **未做** | 钩子挂在既有 `GamblerHooks` 单例上（它已在战斗钩子监听集合里，`AfterPlayerTurnStart`/`AfterGoldGained`/`AfterCombatEnd` 都生效，与 `Modify*` 走同一套监听者迭代）。少一个类型、少一份状态副本 |
| 金币兑现 | 卡内自算 + `MakeCalculatedVar` 显示 | **卡内自算 + 卡面显示裸值**（`IntVar`），描述里写"×倍率" | `MakeCalculatedVar` 的语义是 `base + bonus×mult`（源码核实），与"7 × 倍率"不合，且升级增量会落到 `Base` 子变量上、很绕。按 §6 M2 的既定退化执行 |
| 词条注册 | 按「赌博」做法走 `CustomKeywords` | **不注册**：卡描述用 `*风头正盛*` 富文本高亮 | 复核实测：本项目「赌博」词条**从未注册过** `CustomKeywords`，就是 `*文字*` 高亮。照既有实践走，保持一致 |
| `GambleService.LastFlipHeads` / `HeadsCountThisTurn` | 供叠注/同花顺/底牌消费 | **已无消费者**（改造后不读它们） | 属 M1 冻结的 Core 公开接口，按 §4.8 ② **只加不改**，保留不删（v2 清理时可一并移除） |

### ⚠ 唯一需要游戏内校准的点

`UI/MultiplierBadge.cs` 的 `HeadOffsetY`（再往上抬多少像素）。定位方式经 **IL 核实**：`NCreature.GetTopOfHitbox()` 返回的是**全局坐标**（`Hitbox.GlobalPosition + (Hitbox.Size.X*0.5, 0)`，命中框顶边中点），所以徽章直接写自己的 `GlobalPosition`，不做坐标空间换算；每帧只在目标点变化时才写（无分配）。徽章只在 `NCreature.Entity.IsPlayer` 的生物上显示，并设 `MouseFilter = Ignore` 以免挡点击。
数据源：`GambleService.Records.Count`（当版本号，变化才写）+ `MultiplierService.LastOutcome`（枚举 **Heads/Tails/SavedByChip**）。**三态与倍率口径严格一致**：正（金，+0.5）/ 反（红，归零）/ **保**（蓝，幸运筹码花 5 金保命——没涨也没归零）。2026-10-16 用户三轮实测后的定稿：①记录改为**战斗结束时清空**（原来只在"新战斗首次掷币"清，导致新战斗开局残留上一场正反）；②改币**不能**显示成"反"——那会让人以为"显示反、倍率却没降"是 bug，所以单独立了「保」这一态。
整组（倍率 76px + 间隙 4px + 正反 44px = 124px）**以头顶中线居中**。

**⚠⚠ 实测踩到的坑（代码里都有注释，别删）**：
1. **`MegaLabel._Ready()` 里的 `AssertThemeFontOverride` 是「抛异常」而不是「打日志」**——本节点没有主题字体覆盖时它会抛 `InvalidOperationException: … has no theme font override`。2026-10-16 因为把它调在 `_Ready` 最前面，异常一抛、后面设文字的代码全被跳过 → **徽章整块消失（用户原话："啥也没有 倍率也没有了"）**。
   → 正确姿势：先 `AddThemeFontOverride("font", GetThemeFont("font","Label") ?? ThemeDB.FallbackFont)` 让断言通过，再 `try { base._Ready(); } catch { … }` 兜底；**非玩家生物提前 return**，不初始化字体（否则敌方每只各抛一次）。
   注：完全不调 `base._Ready()` 时文字**能**渲染（走 Godot 默认主题）——前一版"看得见"就是这么来的，但字体不是游戏 UI 字体。
2. **不要依赖本节点的 `_Process`（连 `SetProcess(true)` 都不行）**——`_Ready`/`_EnterTree` 是引擎必调的生命周期回调（所以看起来"回调通了"），但 `_Process` 受 process 标志控制，Godot 不像 GDScript 那样"检测到有 `_Process` 就自动打开"。2026-10-16 **两次**实测：徽章显示但不刷新（倍率/正反都冻在初始值），日志里只有 `已挂载`、没有 `_Process 已启动`；补了 `SetProcess(true)` 后**依然**没回调。最终改成挂 **`SceneTree.ProcessFrame` 信号**（`GetTree().ProcessFrame += OnFrame`，`_ExitTree` 里退订）——信号连接不受 process 标志控制，一挂就生效。
3. 另加了两条自证日志：`风头徽章 _Process 已启动` 与每次变化的 `徽章刷新 → x…`，用于区分"调用链没跑"和"渲染没生效"。

**当前校准值 `HeadOffsetY = -58f`**（2026-10-16 由 -34 上调 24px，用户反馈位置偏低）。实测锚点示例：`(960, 262)`（2560×1369 窗口下的全局坐标）。

### 运行日志实测（2026-10-16 首次跑通）

- 51 卡注册成功、我的代码**零异常**；`GAMBLERMOD-GO_FOR_BROKE 兑现风头，倍率清空回 x1.0` 与掷硬币记录均正常。
- 掷硬币日志已带上倍率轨迹（`（风头 x1.5）`），并标注"改币不计"，方便验收时对账。
- 启动时那条 `[ERROR] Tried to subscribe for CombatState hooks with id GAMBLERMOD-GAMBLER_HOOKS, but it's already been used!` 是**既有**噪音（`GamblerHooks` 被 ModelDb 与 Entry 各实例化一次；防重复订阅 guard 在 `base(HookType)` 之后运行，拦不住基类那次日志），BaseLib 会"Ignoring subscription"并继续，功能不受影响。
- 卡图缺失警告（`Missing sprite 'gamblermod/gamblermod-*'`）是占位图老问题，新卡同样命中，属已知项。

### 已知边界（沿用既有口径，不阻塞）

倍率是进程内静态量：**存档退出再读档**时不会恢复（会与存盘前不一致）；多人模式下所有玩家共用一个倍率。两者与既有 `GambleService` 掷骰记录的限制同源，按 M4 的"多人/读档确定性留待校准"口径一并处理。

### 编译与部署

`dotnet build` 0 错误（36 警告，其中 10 条是新卡的 `Targeting(play.Target)` 可空提示，与既有卡同款）；部署到 `D:\SteamLibrary\steamapps\common\Slay the Spire 2\mods\GamblerMod.dll` 成功。**尚未游戏内实测**（M5 验收见 [test-plan.md](../testing/test-plan.md) 四·五）。
