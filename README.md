# 杀戮尖塔2（Slay the Spire 2）Mod 开发工作区

本机游戏：`D:\SteamLibrary\steamapps\common\Slay the Spire 2`（v0.111.0，Godot + C#/.NET 9）。
游戏自带官方 Mod 系统与 Harmony 补丁库，Mod 是 **C# DLL + 同名 JSON 清单**（可选同名 .pck 资源包），
放进游戏目录下的 `mods\` 即被加载（创意工坊订阅的也算）。

## 目录结构

```
sts2-mod\
├── MyMod\            Mod 源码项目（改名时同步改 csproj 的 AssemblyName 和 json 文件名）
│   ├── MyMod.csproj  构建后自动把 dll+json 部署到游戏 mods 目录
│   ├── MyMod.json    Mod 清单（名称/版本/依赖等，字段见 notes）
│   └── src\Mod.cs    入口与 Harmony 补丁模板
├── lib\              编译引用的游戏 DLL（sts2 / 0Harmony / GodotSharp + sts2.xml API 文档）
└── notes\            从游戏本体挖出的 Modding API 笔记
```

## 环境（唯一缺的东西）

本机只装了 .NET 运行时，没有 SDK。装一次即可：

```powershell
winget install Microsoft.DotNet.SDK.9
```

## 日常流程

```bash
cd D:\worlk\sts2-mod\MyMod
dotnet build        # 编译 + 自动部署到游戏 mods 目录
```

启动游戏，在主菜单的 Mod 管理界面确认 MyMod 显示为已加载。
（若显示 Failed：核对 MyMod.json 字段与 notes\modding-api.md 中的清单字段是否仍一致。）

## 下一步

1. 翻 `lib\sts2.xml` 找想改的游戏类型（文本搜索即可，命名空间 MegaCrit.Sts2.Core.*）。
2. 在 `src\Mod.cs` 里按模板写第一个 `[HarmonyPatch]`，或加 `ModInitializer` 做初始化。
3. 要做自定义卡牌/遗物，先看 `notes\modding-api.md` 的 ModHelper 一节，
   复杂内容建议直接依赖社区库 BaseLib-StS2（NuGet: `Alchyr.Sts2.BaseLib`）。
4. 游戏更新后：重新从 `游戏目录\data_sts2_windows_x86_64\` 拷贝最新 DLL 覆盖 `lib\`，
   并重新核对清单字段（notes 里有现成命令）。

## 发布

- Steam 创意工坊：用游戏自带的 ModUploader（Steam 库中右键游戏 → 管理 → 浏览本地文件，
  或用创意工坊上传工具）。
- Nexus / 手动分发：把 `bin\Debug\net9.0\MyMod.dll` + `MyMod.json`（+ 如有 `.pck`）打包即可。
