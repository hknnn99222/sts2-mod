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
