# STS2 Modding API 笔记（v0.111.0）

以下内容全部提取自本机游戏自带的 API 文档：
`D:\SteamLibrary\steamapps\common\Slay the Spire 2\data_sts2_windows_x86_64\sts2.xml`
（一份拷贝在 `..\lib\sts2.xml`）。游戏更新后可重新从游戏目录拷贝刷新。

## 游戏技术栈

- 引擎：Godot + C#（.NET 9 运行时），主程序集 `sts2.dll`
- 自带 Lib.Harmony（`0Harmony.dll`）与 MonoMod —— 官方 Mod 系统直接基于 Harmony
- 本机游戏版本：v0.111.0（2026-08-13，commit 41cef1ea）

## 官方 Modding 命名空间：`MegaCrit.Sts2.Core.Modding`

| 类型 | 作用 |
|---|---|
| `ModManager` | 加载/排序/禁用所有 Mod 的管理器，游戏启动最早期初始化 |
| `Mod` | 一个已加载的 Mod（字段：manifest, assemblies, modSource, path, state, version, workshopId, errors） |
| `ModManifest` | Mod 的 JSON 清单文件对应的类 |
| `ModInitializerAttribute` | 声明 Mod 入点类；不写则自动 Harmony PatchAll |
| `ModHelper` | Mod 侧调用的辅助 API（内容注册、钩子订阅） |
| `ModSource` | Mod 来源：`ModsDirectory`（游戏旁 mods 目录）或 `SteamWorkshop` |
| `ModLoadState` | Loaded / Disabled / Failed / DisabledDuplicate / AddedAtRuntime |
| `SettingsSaveMod` | settings.save 中每个 Mod 的启用状态 |

关键行为（来自 XML 文档原文的转述）：

- `ModManager.Initialize`："Loads all mods from the appropriate directories. This includes the
  **'mods' directory next to the executable**, as well as Steam workshop files."
- `ReadModsInDirRecursive`：递归读取目录中的 **mod PCK 和 DLL**，允许用子目录组织文件。
- `SortModList`：按依赖关系拓扑排序（Kahn 算法），依赖先加载。
- `RemoveDisabledMods`：settings.save 里禁用的 Mod 一律禁用。
- `ModInitializerAttribute`："Declares a class as the main entry point for the mod.
  If this is present, we'll call the method named `initializerMethod` within the class.
  **Otherwise, we'll create a harmony instance for the mod and call Harmony.PatchAll.**"

## Mod 清单 JSON 字段（已实测验证，2026-10-14）

对照本机正常运行的工坊 Mod（BaseLib v3.4.7 / QuickRestart v2.0.0）+ 加载日志确认：

```json
{
  "id": "QuickRestart",          // 必填！缺失时加载器直接报错：
                                 // "looks like a mod manifest but is missing the 'id' field"
  "name": "Quick Restart",       // 显示名
  "author": "erasels",
  "description": "...",
  "version": "v2.0.0",           // 带 v 前缀
  "min_game_version": "0.107.0",
  "has_pck": true,               // 有 .pck 资源包才写 true
  "has_dll": true,
  "dependencies": [              // 依赖数组，每项 { "id", "min_version" }
    { "id": "BaseLib", "min_version": "3.4.5" }
  ],
  "affects_gameplay": false
}
```

注意：`min_version` 是** dependencies 内条目**的字段，不是清单顶层字段。
游戏版本更新后字段若变化，优先翻 `%APPDATA%\SlayTheSpire2\logs\godot.log` 看加载报错。

## BaseLib 使用要点（源码核实，2026-10-14，BaseLib 3.4.7）

BaseLib 源码：https://github.com/Alchyr/BaseLib-StS2（关键文件 `Patches/Content/ContentPatches.cs`、`Abstracts/CustomCharacterModel.cs`、`Abstracts/ILocalizationProvider.cs`、`Patches/Localization/ModelLocPatch.cs`）

1. **内容注册是"实例化即注册"**：`Custom*Model` 的构造器自动调
   `CustomContentDictionary.AddCharacter(this)` / `AddModel(...)`（卡牌走 `PoolAttribute` 指定池）。
   Mod 里只要 `new` 一次模型即可，**不要**自己调 `RegisterType(typeof(...))`——
   它只把类型塞进去重集合，不挂载内容，且会让之后构造器里的 AddCharacter 因"类型已登记"而跳过。
2. **角色挂载机制**：BaseLib 给 `ModelDb.AllCharacters` 打 Postfix，追加
   `CustomContentDictionary.CustomCharacters` 列表；选人/开局即走这条管线。
3. **原版模型全是 ModelDb 单例**：任何 `new 原版模型()` 都会抛 `DuplicateModelException`。
   取已有实例：`ModelDb.GetById<T>(ModelDb.GetId<T>())`（T 也可写具体子类）。
4. **代码内本地化**：模型实现 `ILocalizationProvider.Localization`（返回 `CardLoc`/`CharacterLoc` 等
   record，隐式转 `List<(string,string)>`）。在 `ModelDb.Init` 后被 `ModelLocPatch` 消费，
   写入对应 loc 表，键 = `{ModelId.Entry}.{title|description|...}`。
5. **占位角色**：`PlaceholderCharacterModel` 按 `PlaceholderID` 拼原版资产路径
   （creature_visuals / 选人背景 / 营火 / 商人 / 能量球等全套），默认 "ironclad"。

## 本项目实测 API 速查（M0-M4 全部在用，2026-10-14）

> 以下接口全部在本项目源码中实际使用并验证过，按用途分类；括号内是本项目的用例文件。
> 命名空间前缀省略 `MegaCrit.Sts2.Core.`。

### 1. 卡牌模型（GamblerMod/src/Core/GamblerCard.cs + 各卡）

| API | 说明 |
|---|---|
| `CardModel.#ctor(int energyCost, CardType, CardRarity, TargetType, bool costsX=false)` | costsX=true 做 X 费卡（梭哈）；TargetType：None/AnyEnemy/AllEnemies/RandomEnemy |
| `protected override Task OnPlay(PlayerChoiceContext ctx, CardPlay play)` | 打出效果主入口（必须 protected，改 public 编译报 CS0507） |
| `protected override IEnumerable<DynamicVar> CanonicalVars` | 数值声明：`new DamageVar(n, ValueProp.Move)`、`new BlockVar(n, ValueProp.Move)`、`new IntVar("名字", n)` |
| `protected override void OnUpgrade()` | 升级：`DynamicVars.Damage.UpgradeValueBy(n)`、`DynamicVars["自定义名"].UpgradeValueBy(n)`；能量费 `EnergyCost.UpgradeBy(-n)` |
| `DynamicVars.Damage/Block.BaseValue` / `DynamicVars["名"].BaseValue` | 效果结算取当前值（含升级/力量），decimal |
| `CardModel.ResolveEnergyXValue()` | X 费卡取实际 X（梭哈） |
| `public override void AfterCreated()` | 挂关键词：`AddKeyword(CardKeyword.Exhaust)`（撕毁契约；枚举还有 Ethereal/Innate/Retain 等） |
| `protected override IEnumerable<CardKeyword> CanonicalKeywords` | 声明**模型固有词条**（v1.8 起「赌博」/「风头正盛」走这条，优于 `AfterCreated`+`AddKeyword`——后者拿不到模型本体的词条，详见 §9） |
| `PlayerCombatState.HasEnoughResourcesFor` Postfix | 可打出检查的正规出口（GoldCostCheckPatch；`ref bool __result, ref UnplayableReason reason`） |

### 2. 战斗命令（流式，cookbook 范式）

| API | 用例 |
|---|---|
| `DamageCmd.Attack(decimal).FromCard(this, play).Targeting(play.Target).WithHitFx("vfx/vfx_attack_slash").Execute(ctx)` | 单体攻击（试手气） |
| `.TargetingAllOpponents(CombatState)` / `.TargetingRandomOpponents(CombatState)` / `.WithHitCount(n)` | AoE/随机/多段（赌一把/雇佣打手） |
| `.WithHitFx(path, null, "heavy_attack.mp3")` | 特效可带音效（掷硬币正反面反馈） |
| `CreatureCmd.GainBlock(Creature, BlockVar, CardPlay)` 或 `(Creature, decimal, ValueProp, CardPlay, bool)` | 格挡（买平安/灵光一闪） |
| `CreatureCmd.Damage(ctx, Creature, decimal, ValueProp, CardModel, CardPlay)` | 直接伤害，自伤用 Owner.Creature（孤注一掷/轮盘） |
| `PowerCmd.Apply<TPower>(ctx, Creature 目标, decimal 层数, Creature 来源, CardModel)` | 上力量/易伤/虚弱（打手/下绊子/黑吃黑；负数 StrengthPower=力量削弱） |
| `PlayerCmd.GainGold(decimal, Player, bool 动画)` / `LoseGold(decimal, Player, GoldLossType.Spent)` | 真金币（全部产金/扣费卡） |
| `PlayerCmd.GainEnergy(decimal, Player)` | 能量增减，负数即失去（天命） |
| `CardPileCmd.Draw(ctx, decimal 张数, Player)` | 抽牌（买路钱/分红） |
| `CardSelectCmd.FromChooseACardScreen(ctx, IReadOnlyList<CardModel>, Player, bool)` | 弹选卡界面，返回选中卡（数牌/当铺） |
| `CardCmd.Discard(ctx, card)` / `CardCmd.Exhaust(ctx, card)` | 弃置/消耗指定卡（当铺） |

### 3. 状态查询

| API | 说明 |
|---|---|
| `ModelDb.Card<T>()` / `ModelDb.CardPool<T>()` / `ModelDb.RelicPool<T>()` / `ModelDb.Relic<T>()` | 取原版模型单例（起始牌借打击/防御走这个） |
| `Player.Gold`（int）/ `Player.Relics` / `Player.GetRelic<T>()` | 金币与遗物归属（幸运筹码） |
| `Player.PlayerRng`（PlayerRngSet：Rewards/Shops/Transformations + Seed） | 玩家随机源集合 |
| `Creature.GetPower<T>()` | 查力量（正面率聚合/流水判定），返回可空 |
| `Creature.CurrentHp / MaxHp / IsDead / IsAlive` | 击杀判定与定向（赏金猎人/买凶） |
| `CardModel.Owner`（Player）/ `CardModel.CombatState`（ICombatState） | 卡牌上下文 |
| `ICombatState.Enemies`（IReadOnlyList<Creature>）/ `ICombatState.RoundNumber`（int） | 敌人枚举与回合计数 |

### 4. 随机（GambleService.cs）

| API | 说明 |
|---|---|
| `new Rng(Player, ModelId 内容ID, ulong mixin)` | 官方"内容专属 RNG"：按 局种子+槽位+内容ID 无状态派生，多人隔离、回放友好。文档明示**同内容再见出同结果，必须用 mixin 区分**（我们用战斗序号） |
| `Rng.NextBool()` / `Rng.NextDouble()` | 掷硬币/加权概率（NextDouble ∈ [0,1)） |

### 5. 模型钩子（AbstractModel 虚方法，**必须 override 不能 new**——CS0114 警告即此因）

| 钩子 | 签名 | 用例 |
|---|---|---|
| 回合开始 | `Task AfterPlayerTurnStart(PlayerChoiceContext, Player)` | 存钱罐/赌场规则产金、幸运筹码重置、SpendTracker 清零 |
| 战斗结束 | `Task AfterCombatEnd(CombatRoom)`（Rooms 命名空间） | 战斗态复位（GamblerHooks） |
| 获得金币后 | `Task AfterGoldGained(Player)` | 利滚利放大（注意防递归） |
| 卡牌创建后 | `void AfterCreated()` | 挂关键词 |
| 全局常驻钩子 | `class X : CustomSingletonModel(HookType.Combat)` | 无卡/遗物载体的监听（GamblerHooks：高利贷扣款） |

### 6. 遗物与力量

| API | 说明 |
|---|---|
| `CustomRelicModel` + `[Pool(typeof(自有遗物池))]` | 遗物**必须挂 Pool** 且池被角色引用，否则初始化抛异常（M4 踩过）；`RelicRarity.Starter/Common/Rare/Shop/Event` |
| `CustomPowerModel` | `PowerType.Buff/Debuff` + `PowerStackType.Counter/Single/None`；PowerModel.Amount（decimal）= 层数 |
| `CustomCardPoolModel` / `CustomRelicPoolModel` | 自有池：DeckEntryCardColor（卡池必覆写）/ LabOutlineColor（遗物池）、H/S/V 卡背色 |

### 7. 本地化（SimpleLoc）

| API | 说明 |
|---|---|
| `CardLoc(Title, Description)` / `RelicLoc(T,D,Flavor)` / `PowerLoc(T,D,SmartD)` / `CharacterLoc(14 个具名参数)` | 隐式转 `List<(string,string)>`；在 `ModelDb.Init` 后由 ModelLocPatch 写入 loc 表 |
| 语法 | **描述必须 `#` 开头**才处理；`!Var!`→数值（D/B/C/E/H 为 Damage/Block/Cards/Energy/Heal 缩写）、`*文字*`→金色、`-旧-+新+`→升级差异、`\n` 换行（C# 字符串转义） |

### 8. 角色（GamblerCharacter.cs）

| API | 说明 |
|---|---|
| `PlaceholderCharacterModel.PlaceholderID`（**小写**） | 复用原版资产；Godot res:// 区分大小写 |
| `StartingHp/StartingGold/StartingDeck/StartingRelics/CardPool/RelicPool/PotionPool/Gender/NameColor` | 覆写点；起始牌直接引用自有卡单例 |
| `CustomCharacterSelectIconPath`（png）/ `CustomCharacterSelectBg`（tscn 场景） | 选人封面/背景，走 pck |

### 9. 词条 / 悬停提示 / 卡面数值显示（v1.8 新增，2026-10-17 实测）

**自定义卡牌词条（BaseLib「自定义枚举」机制）**

| 项 | 做法 |
|---|---|
| 声明 | `[CustomEnum, KeywordProperties(AutoKeywordPosition.Before)] public static CardKeyword HotStreak;`——字段**必须 public static**（readonly 会赋值失败），放任意 mod 类型里（本项目 `Core/GamblerKeywords.cs`） |
| 挂载 | 覆写 `CardModel.CanonicalKeywords`。**别用 `AfterCreated` + `AddKeyword`**：那个钩子的语义是"卡**实例**创建后"（`CardModel.AfterCreated` 原文 "after the card is created (NOT during deserialization)"），图鉴/牌堆里的**模型本体**拿不到词条 |
| 词条 ID | `根命名空间大写 + '-' + 字段名大写`（BaseLib `TypePrefix.GetPrefix()`）→ 本项目 `GAMBLERMOD-GAMBLE` / `GAMBLERMOD-HOTSTREAK`；`[CustomEnum("自定义名")]` 可换后半段 |
| 文案 | pck 内 `GamblerMod/localization/<lang>/card_keywords.json`，键 = `词条ID.title` / `.description`（BaseLib 自带词条 BASELIB-PURGE 同款；lang 取 `zhs`/`zht`/`eng`…） |
| 自动文字 | `[KeywordProperties(AutoKeywordPosition.Before/After)]` 让引擎把词条**标题**自动拼进卡描述（BaseLib 对 `CardKeywordOrder` 静态构造器打 Postfix）→ 卡文里**不要再手写**词条名 |
| 悬停提示 | 白拿：BaseLib `CustomTooltips` 前缀补丁拦 `HoverTipFactory.FromKeyword`，用上面的 loc 键造 `HoverTip`；悬停卡牌由 `NCardHolder.CreateHoverTips` 触发。**引擎没有"点击才弹"的通道** |
| ⚠️ | 词条值由 BaseLib 在 `ModelDb.Init` 反射写入 → **绝不可把词条值缓存进静态字段**（静态初始化更早，会固化成 0，即"陷阱 #8"的变体）；每次访问现读字段 |

**自定义静态悬停提示（给非卡牌 UI 用）**：`[CustomEnum] public static StaticHoverTip X;` + pck 内 `static_hover_tips.json`，键 = `前缀-字段名UPPER_SNAKE.title/.description`（`BundledRewards` → `BUNDLED_REWARDS`）；取用 `HoverTipFactory.Static(X, Array.Empty<DynamicVar>())`。参考 BaseLib `Utils/BaseLibTip.cs`。

**任意 Control 挂悬停提示**（本项目头顶倍率徽章 `UI/MultiplierBadge.cs`）：

```csharp
MouseFilter = MouseFilterEnum.Pass;   // Pass：自己收事件、事件继续上传父节点 → 不挡点角色（用 Ignore 则永远收不到悬停）
Connect(Control.SignalName.MouseEntered, Callable.From(OnHovered));
Connect(Control.SignalName.MouseExited,  Callable.From(OnUnhovered));

// OnHovered：★ 必须给对齐，否则提示节点停在 (0,0) = 屏幕左上角（本项目实测踩过）
if (NHoverTipSet.CreateAndShow(this, tip) is { } tipSet)
{
    tipSet.SetAlignment(this, HoverTipAlignment.Center);  // None/Left/Right/Center
    tipSet.SetFollowOwner();
}
// 仍不满意再叠加显式偏移：tipSet.SetGlobalPosition(GlobalPosition + new Vector2(0f, -140f))
// OnUnhovered → NHoverTipSet.Remove(this)（_ExitTree 也要 Remove）
```

⚠️ 提示构建包 try/catch 并**懒建**：`HoverTipFactory.Static` 在自定义值没注册时可能抛，别让它把节点初始化带崩（本项目这个节点已被字体断言/_Process 坑过三次）。

**卡牌图鉴（百科）可见性**：`CustomCardPoolModel.SeenByDefault`，**BaseLib 默认 false** → 打开图鉴时 `CustomCardPoolMarkAsSeenPatch` 跳过该池，整池被渲染成"未知/未解锁"。覆写 `=> true` 即"开局全可见"（写进存档 `ProgressState.DiscoveredCards`）。图鉴里的池筛选按钮由 BaseLib `CustomPoolFilters` 自动补。

**附魔与卡面数值（本批最关键的一组事实）**：

| API | 说明 |
|---|---|
| `DynamicVar.BaseValue` | **改游戏状态的计算用这个**（文档原文："the value that should be used when performing calculations that will modify the game's state"） |
| `DynamicVar.EnchantedValue` | 附魔后的值（"附魔属于卡牌自身"的显示通道）；无附魔时 == BaseValue |
| `DynamicVar.PreviewValue` | **仅供显示**，由 `CardModel.UpdateDynamicVarPreview` 刷新；别拿去结算 |
| `EnchantmentModel.EnchantDamageAdditive(decimal, ValueProp)` / `EnchantDamageMultiplicative(decimal, ValueProp)` / `EnchantBlockAdditive(decimal)` / `EnchantBlockMultiplicative(decimal)` | 附魔数值钩子；**乘法钩子返回倍率**（`num *= hook(...)`，见 BaseLib `CustomExtraDamageVar`）。这两个钩子会被伤害/格挡管线调用（`ModifyDamageHookType.Additive/Multiplicative` 的文档明示包含 enchant 版） |
| ⚠️ 陷阱 | 附魔**只修标准 `DamageVar`/`BlockVar`**；把数值写进自定义 `IntVar`（"正面改为 X"这种）→ 卡面永远显示裸值，玩家会以为吃不到附魔。修法见下 |
| ⚠️ | **金币/能量没有任何附魔通道**，结构上不可能吃到附魔，别当 bug 查 |

**★ 卡面数值：伤害/格挡一律用原生变量，别用 `IntVar`**（2026-10-17 实测踩坑，本项目 v1.8 第一版就踩了）

引擎只为**它认得的变量类型**跑完整卡面预览管线（附魔 + 力量/虚弱/易伤/敏捷）。用 `IntVar` 承载伤害/格挡数值 → 卡面永远显示裸值：玩家会看到"正面数值不吃增伤、不吃虚弱"，而且卡面与结算对不上。
**引擎的 `DamageVar` / `BlockVar` 都带 `(string 名字, decimal 数值, ValueProp props)` 构造器**（元数据核实）——"一张卡两套伤害数值"直接这样写，自动获得与标准 `!D!`/`!B!` 完全一致的预览行为：

```csharp
new DamageVar("HeadsDamage", 10, ValueProp.Move),   // 自定义名的伤害变量，与标准 DamageVar 并存
new BlockVar("TailsBlock",  16, ValueProp.Move),
```

补充事实：`DynamicVar.Name` **只有 getter**（元数据核实），所以想要自定义名字必须走上面的构造器重载，别指望"先 new 再改名"（BaseLib `CustomCalculatedDamageVar` 用反射找 Name setter，找不到会静默不设名）。

**引擎没有管线的资源才需要自定义变量**（本项目 `Core/CardValueVars.cs` 的 `MultiplierAwareVar`：金币 × 倍率显示）：继承 `IntVar`（**名字走 `IntVar(string, decimal)` 构造器**）覆写 `UpdateCardPreview(CardModel, CardPreviewMode, Creature?, bool)`，算好写 `EnchantedValue` + `PreviewValue`。

**`DamageCmd.Attack` 只有两个重载**：`(decimal)` 与 `(CalculatedDamageVar)` —— 动态数值卡（如 PerfectedStrike）用后者。

**pck 里放本地化 json**：直接把 `assets/GamblerMod/localization/<lang>/xxx.json` 交给现有 Godot 导出流程即可（`export_filter="all_resources"` 会带上），导出后用 `strings GamblerMod.pck | grep localization` 核对；运行时游戏按 `res://<ModId>/localization/<lang>/<表名>.json` 取（BaseLib `DefaultLoc` 的路径构造即此形式）。

## 2026-10-21 批次新增/核实的 API（卡牌重构用）

> 这批是 v1.10–v1.15 改卡时新用到或踩过的接口；签名均从 `lib/sts2.xml` / 程序集元数据核实。

### 活力（Vigor）
- 原版力量：`MegaCrit.Sts2.Core.Models.Powers.VigorPower`（储君在用）。语义 = **下一次攻击额外造成等量伤害，打出后消耗**。
- 用法与其它力量一致：`PowerCmd.Apply<VigorPower>(ctx, Owner.Creature, amount, Owner.Creature, this)`，不需要自己实现消耗逻辑。

### 把牌放回抽牌堆
- `CardPileCmd.Add(card, PileType.Draw, CardPilePosition.Random, this, false)`：把一张牌随机洗回抽牌堆（`CardPilePosition` 枚举 = `None / Bottom / Top / Random`）。
- **没有** `PutOnTopOfDrawPile` 之类的专用命令；`CardCmd.DiscardAndDraw` 是"弃置后抽等量"（弃牌堆语义），不能替代"回牌堆"。

### 选卡提示（CardSelectorPrefs）
- 构造器：`CardSelectorPrefs(LocString prompt, int min, int max)`。
- 引擎只提供 5 个预设：`DiscardSelectionPrompt` / `ExhaustSelectionPrompt` / `TransformSelectionPrompt` / `RemoveSelectionPrompt` / `EnchantSelectionPrompt` / `UpgradeSelectionPrompt`（都是 `LocString`）。
- **没有"放回抽牌堆"的预设提示**。本项目「抵押/换牌」暂时借用 `DiscardSelectionPrompt`，动作仍是回牌堆（已知 UI 文案偏离，记在 test-plan）。
- 要自定义文案需要 `new LocString(locTable, locEntryKey)` + 往 pck 的 `localization/<lang>/<表名>.json` 补条目，属于后续本地化工作。

### PowerCmd.Apply 的签名陷阱
```csharp
PowerCmd.Apply<T>(PlayerChoiceContext ctx, Creature target, decimal amount,
                  Creature applier, CardModel? cardSource, bool silent = false)
```
- **第 5 个参数是 `CardModel`，不是力量本体**。从力量里再施加力量时传 `null`（本项目保本在 `SpendTracker` 里给敏捷/力量时踩过）。
- 力量要造成伤害时用 `CreatureCmd.Damage(ctx, target, amount, ValueProp.Move, dealerCreature)`（5 参重载，dealer 是 Creature，没有 cardSource 通道）。

### 金额相关的金币钩子
- `AfterGoldGained(Player)` **拿不到这次的金额**（利滚利只能按固定值加钱）。
- 要按金额做事用 `AbstractModel.AfterModifyingGoldGained(Player player, decimal amount)`（晒冷的"金币→等量格挡"靠它）。
- **金币"花费"没有钩子**：本项目把唯一记账点放在 `Core/SpendTracker.Record`（`GoldCost.Pay` 调用它），保本在那里探测 `CapitalGuardPower` 并施加敏捷/力量。

### 卡牌永久成长
- `DynamicVar.BaseValue` / `PreviewValue` 都有公开 setter（元数据核实）。
- 做法：结算后 `DynamicVars.Damage.BaseValue += 4; DynamicVars.Damage.PreviewValue = BaseValue;`
  —— 同一次冒险内跨战斗保留（清台 #14）；读档是否保留取决于引擎对卡牌模型状态的序列化（test-plan 已列为验收观察项）。
- 别用 `UpgradeValueBy` 做成长：它带"刚升级"的预览语义。

### 其它确认过的小事实
- `PowerStackType` 枚举 = `None / Counter / Single`；`PowerType` = `None / Buff / Debuff`。
- `CardModel.IsUpgraded` 可在 `ApplyEffect` 里读，用来把"升级档"编码进力量层数（轮盘/晒冷/保本都用 Amount 0/1 表示档位）。
- `DamageResult` 有 `BlockedDamage`（被格挡抵消的伤害）与 `WasBlockBroken`；"破盾转金"直接累加前者（吃水 #54）。
- "消耗"关键词要挂在**卡牌实例**上：`AfterCreated()` 里 `AddKeyword(CardKeyword.Exhaust)`，别写构造器（会污染 canonical 模型）。
- 条件/档位数值（伤害）仍然必须用原生 `DamageVar`/`BlockVar`（见上节），`IntVar` 只用于次数、金币、层数。

## ModHelper 可用方法（XML 文档摘要）


- `AddModelToPool<...>()` — Mod 向内容池（卡牌/遗物等）注册自定义模型；
  池一旦完成合并后再调用会抛异常（要在加载早期调用）。
- `ConcatModelsFromMods<T>(IPoolModel, IEnumerable<T>)` — 由池调用，合并各 Mod 内容。
- `SubscribeForRunStateHooks(string id, RunHookSubscriptionDelegate)` — 让自定义模型接收局内（Run）钩子。
- `SubscribeForCombatStateHooks(string id, CombatHookSubscriptionDelegate)` — 让自定义模型接收战斗钩子。
- `IterateAllRunStateSubscribers(RunState)` / `IterateAllCombatStateSubscribers(CombatState)` —
  游戏内部迭代钩子监听者时回调，保证 Mod 自定义模型的钩子被调用。

## 如何在游戏更新后重新提取这些信息

```bash
G="D:/SteamLibrary/steamapps/common/Slay the Spire 2"
# 版本
cat "$G/release_info.json"
# Modding API 文档条目
grep -oE '"[TMP]:MegaCrit\.Sts2\.Core\.Modding\.[A-Za-z0-9_]+' "$G/data_sts2_windows_x86_64/sts2.xml" | sort -u
```

## 社区资源

- BaseLib-StS2（社区基础库，相当于一代的 BaseMod）：https://github.com/Alchyr/BaseLib-StS2
  Wiki：https://alchyr.github.io/BaseLib-Wiki/ ，NuGet 包 `Alchyr.Sts2.BaseLib`
- Nexus Mods STS2 分区：https://www.nexusmods.com/slaythespire2
- Steam 创意工坊（游戏已支持订阅安装）+ 官方 Discord 的 modding 频道
