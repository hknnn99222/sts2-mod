# 杀戮尖塔2 赌徒 Mod（GamblerMod）

为 Slay the Spire 2 开发自定义角色"**赌徒**"：金币混合费用 + 掷硬币赌博。
基于游戏官方 Mod 系统 + 社区库 BaseLib，C# DLL + pck 资源包（选人封面/背景已接入，卡面立绘暂为占位）。

## 当前状态

| 里程碑 | 状态 |
|---|---|
| M0 骨架（占位角色进选人界面） | ✅ 验收通过 |
| M1 核心管线（金币费用 + 赌博掷硬币 + 起始牌） | ✅ 验收通过 |
| M2 做局（幸运筹码遗物 + 暗记/手气正旺/赌神） | ✅ 代码完成，待验收 |
| M3/M4 卡池（43 张全量：普 11 / 罕 19 / 稀 13） | ✅ 代码完成，待验收 |
| M5 风头正盛倍率（+8 张新卡、改造 3 张，卡池 51） | ✅ 代码完成，待验收 |
| v1.7 扩池至 80（+29 张：普 7 / 罕 13 / 稀 9） | ✅ 代码完成，待验收 |
| **v1.8 游玩体验优化**（词条化 + 图鉴全可见 + 徽章悬停 + 附魔修正 + 删 4 张牌 → 76） | ✅ 代码完成，待验收（清单见 test-plan 四·五-A） |
| **v1.9 四轴改造**（金币/出千/气势/赌命 四条独立引擎，+9 张 → 85） | ✅ 代码完成，待验收（清单见 test-plan 四·七） |
| **v1.10 普通档手感批次**（20 张调费用/数值/钩子；血本→分红、放血转技能） | ✅ 代码完成，待验收（清单见 test-plan M3） |
| **v1.12 罕见档 8 张**（梭哈/连续下注/清台/双倍下注/叠注/翻本/摇骰/再加注） | ✅ 代码完成，待验收（清单见 test-plan 四） |
| **v1.13 罕见档 12 张**（暗记/派彩/数牌/兑筹/抽头/保险/高利贷/抽水/押宝/翻盘/暗注/收网） | ✅ 代码完成，待验收（清单见 test-plan 四） |
| **v1.14 罕见档 7 张**（流水/反水/保底/加码/清码/抵押/换牌） | ✅ 代码完成，待验收（清单见 test-plan 四） |
| **v1.15 稀有档 23 张**（全稀有档费用/数值重构，3 张转能力） | ✅ 代码完成，待验收（清单见 test-plan 四） |

测试清单见 [docs/testing/test-plan.md](docs/testing/test-plan.md)。

## 目录结构

```
sts2-mod\
├── GamblerMod\               Mod 源码
│   ├── GamblerMod.csproj     构建后自动部署 dll+json 到游戏 mods 目录
│   ├── GamblerMod.json       清单（依赖 BaseLib ≥3.4.7，工坊订阅提供运行时）
│   └── src\
│       ├── Entry.cs          唯一注册点（Harmony 补丁 → 力量/遗物/卡牌 → 角色）
│       ├── Core\             共用层：GamblerCard 基类 / GoldCost / GambleService /
│       │                     SpendTracker / GoldCostCheckPatch / GamblerHooks（全局钩子）
│       ├── Character\        角色 + 职业卡池
│       ├── Cards\            85 张常规卡（Basic/Common/Uncommon/Rare + 单例仓）
│       ├── Powers\           13 个力量 + 单例仓
│       ├── UI\               头顶「风头正盛」倍率徽章（倍率 x1.5 + 掷币 正/反）
│       └── Relics\           幸运筹码 + 单例仓
├── lib\                      编译引用（sts2 / 0Harmony / GodotSharp / BaseLib + sts2.xml API 文档）
├── docs\
│   ├── design\               游戏设计（gambler-design.md v1.5）+ 技术设计（gambler-tech-design.md）
│   └── testing\              测试文档
├── notes\modding-api.md      Modding API 笔记（含 BaseLib 使用要点/踩坑记录）
└── HANDOFF.md                交接摘要（必读：引擎语义、加卡流程、历史事故）
```

## 环境与构建

- .NET SDK 9 装在 **D:\dotnet**（PATH 异常时用全路径 `/d/dotnet/dotnet.exe`）
- BaseLib 3.4.7 需在创意工坊订阅（运行时依赖，本机已装）
- **构建前关游戏**——mods 目录里的 dll 被游戏进程锁定会导致部署失败（编译仍成功，看报错是否 MSB3027）

```bash
cd D:\worlk\sts2-mod\GamblerMod
dotnet build        # 编译 + 自动部署到 D:\SteamLibrary\...\Slay the Spire 2\mods
```

开游戏 → Mod 管理界面确认 GamblerMod 已加载 → 选赌徒开一局。

## 加一张新卡（3 步，不动 Core）

1. 复制任意现有卡文件（如 `src/Cards/Common/SmallChange.cs`），改类名、
   base 构造四元组（费用/类型/稀有度/目标）、构造体里设 `BaseGoldCost` / `IsGambleCard`、
   `CanonicalVars` 数值、`OnUpgrade` 升级增量、`ApplyEffect` 效果、`Localization` 文案；
2. `GamblerCards.cs` 加一行属性 + Init 一行；
3. 关游戏 → `dotnet build`。

改数值/效果只动卡文件（Core 层 M1 起冻结，约束见技术设计 §3.1/§4.8）。
卡牌文案用 BaseLib SimpleLoc 语法：**描述必须 `#` 开头**，`!D!`/`!变量名!` 显示数值、
`*文字*` 金色高亮、`-旧-+新+` 升级差异。

## 排障

- 游戏日志：`%APPDATA%\SlayTheSpire2\logs\godot.log`，搜 `GamblerMod`（掷硬币/扣金币均有记录）
- 常见坑（详见 [HANDOFF.md](HANDOFF.md)）：构建前没关游戏；`RegisterType` 不注册内容；
  原版模型禁止 new；卡池必须被角色引用；SimpleLoc 缺 `#`；资源路径区分大小写
- BaseLib 源码：`https://github.com/Alchyr/BaseLib-StS2`（语义不明时以源码为准）

## 发布

- Steam 创意工坊：游戏自带 ModUploader（需补 .pck 资源包后发布更完整）
- 手动分发：`bin\Debug\net9.0\GamblerMod.dll` + `GamblerMod.json` 打包即可（要求用户订阅 BaseLib）

### 一键发到手机（QQ）

双击根目录 `一键发送到QQ.cmd`，即把最新的 `GamblerMod_v*.zip` 发到 QQ 的「我的iPhone」会话；
也可把任意文件拖到该 cmd 上发送。脚本本体 `tools/send-to-qq.ps1`（Win32 窗口坐标 + 剪贴板，运行期间别动鼠标键盘）。

```powershell
powershell -ExecutionPolicy Bypass -File tools\send-to-qq.ps1 [-Target '我的iPhone'] [-DryRun] [文件路径]
```

