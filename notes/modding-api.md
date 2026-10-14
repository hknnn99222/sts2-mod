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
