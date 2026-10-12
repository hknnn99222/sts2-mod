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

## Mod 清单 JSON 字段

从 `sts2.dll` 元数据字符串簇中提取（紧邻出现的连续字段名）：

```
min_version, author, description, version,
has_pck, has_dll, dependencies,
affects_gameplay, min_game_version
```

对照社区 Mod 的发布包（如 BaseLib：`BaseLib.dll` + `BaseLib.json` + `BaseLib.pck`），
manifest 与 DLL 同名放在一起。`has_pck` / `has_dll` 疑似加载器探测标志，
模板里已按实际包内容填写。字段集合在游戏版本更新后可能变化，若加载报错
（ModLoadState.Failed），优先回来核对这些字段名。

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
