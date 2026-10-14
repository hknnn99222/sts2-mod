# STS2 赌徒 Mod 项目交接摘要

> 供后续开发的智能体/协作者使用。生成于 2026-10-14，对应设计文档 v1.2、main 分支 `14b6a70`。

## 项目概况

- **目标**：为 Slay the Spire 2（杀戮尖塔2）开发自定义角色"赌徒"（金币+掷硬币主题）
- **仓库**：`D:\worlk\sts2-mod`，远程 `github.com:hknnn99222/sts2-mod.git`（main 分支，工作区干净）
- **阶段**：设计已定稿（v1.2）并推送；开发环境已就绪并通过构建测试；**尚未写任何项目代码**（仓库里的 MyMod 是原模板）

## 已核实的事实（勿重复调研）

- 游戏本体 `D:\SteamLibrary\steamapps\common\Slay the Spire 2`，v0.111.0，Godot 4 + C#/.NET 9
- **官方 mod 支持**（无需 ModTheSpire）：`mods\` 目录放 C# DLL + 同名 JSON 清单 + 可选 .pck；游戏内置 Harmony；`ModManager` 自动加载
- API 全量文档在 `lib\sts2.xml`（105k 行）；`lib\` 有 sts2.dll / 0Harmony.dll / GodotSharp.dll
- 关键 API：`Modding.ModHelper.AddModelToPool`、`CardModel(energyCost, CardType, CardRarity, TargetType)` + `OnPlay`、`Commands.PlayerCmd.GainGold/LoseGold(GoldLossType.Spent)`、`RichTextGold`（卡面金色富文本）、`PlayerRng`
- **引擎原生费用只有能量+星**（摄政王 Stars）；金币不是原生费用 → 混合费用需 `CanPlay` 检查金币 + `OnPlay` 里 `LoseGold` 实现
- 原版角色模板：80 常规卡（20普/35罕/25稀）+ 10 起始 + 5 多人 + 2 特殊；遗物 1初始+1替换+7职业；3 药水；起始 99 金
- 社区库 **BaseLib-StS2**（NuGet `Alchyr.Sts2.BaseLib`）支持自定义角色/卡/遗物/关键词，尚未引入
- 原版战斗内金币先例：金印（3金=1能量/回合）、债务诅咒、贪婪之手——赌徒设计有官方背书

## 设计定稿要点（详见 `docs\design\gambler-design.md`）

- 赌徒：HP 72，起始 99 金，起始牌 = 4打击 + 4防御 + 私房钱（产金）+ 试手气（赌博攻击）
- **四大机制**：
  1. 混合费用"X能量+Y金币"（约18张，补贴率≈5-15金/能量档）
  2. "赌博"词条：打出掷硬币、正面强化×2 / 反面基础×0.8、期望锚定 E=1.2×白板、公式 `0.8+0.8p`、加法上限75%、共14张
  3. 做局：初始遗物"幸运筹码"（每回合1次、5金、事后改反面为正面）+ 换骰/暗记/操纵牌局/手气正旺/赌神
  4. 产金经济：即时产金10张 + 引擎3张，小幅战斗盈利（+5~15金/场）；真赌牌3张（反面有惩罚）、头奖2张
- **v1.2 卡池原则：一张卡=一个钩子**（已消灭纯数值变体）；§5.5 机制词汇表（10钩子家族+空位）是继续加卡的语法
- 范围：精简核心版 45 常规卡（11普/20罕/14稀，18攻/21技/6能），后续扩到 80；多人卡/职业遗物/药水/事件留扩展期
- debuff 用标准库+金币附加费，**不自造敌人金币系统**
- **TBD 待用户拍板**：角色显示名、费用牌面记法、初始遗物名（词条名已定"赌博"，卡名为赌场风占位）

## 本机环境（已就绪）

- .NET SDK 9.0.318 装在 **D:\dotnet**（用户要求：软件非必要不装 C 盘）
- 用户环境变量：`DOTNET_ROOT=D:\dotnet`、`NUGET_PACKAGES=D:\nuget-packages`、用户 PATH 已前置 `D:\dotnet`
- **⚠️ 已知坑**：机器级 PATH 含 `C:\Program Files\dotnet`（仅运行时无SDK）且优先于用户 PATH——新终端 `dotnet` 可能解析失败。修法（管理员 PowerShell）：
  ```powershell
  [Environment]::SetEnvironmentVariable('Path','D:\dotnet;'+[Environment]::GetEnvironmentVariable('Path','Machine'),'Machine')
  ```
  或全路径调用 `D:\dotnet\dotnet.exe`
- 构建链已验证：`cd MyMod && dotnet build` → 0警告0错误，自动部署 dll+json 到游戏 mods ✓
- **未完成**：游戏内 Mod 管理界面的加载确认（需人工开游戏看一眼）

## 历史事故（别重蹈）

- 曾装 LibreOffice 做 Excel，已全部卸载回退；期间 `setx PATH "...;$PATH"` 在 git-bash 里污染了用户 PATH 注册表，已用"会话PATH−系统PATH"重建（条目齐全，顺序或与原不同）。**教训：改 PATH 一律用 PowerShell `[Environment]::SetEnvironmentVariable`，禁用 setx+bash 变量展开**
- Excel 方案已被用户否决，卡牌设计以 markdown 为唯一载体；残留 `C:\Program Files\LibreOffice\` 里 1 个需管理员删的日志文件

## 建议下一步

1. 开游戏人工确认 MyMod 加载
2. （可选）管理员命令修机器 PATH
3. 开始编码：项目改名、引入 BaseLib-StS2、实现顺序建议 混合费用基建 → 起始牌+赌博词条 → 初始遗物 → 45卡分批
4. 向用户要 TBD 命名决定
