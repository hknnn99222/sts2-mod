# STS2 赌徒 Mod 项目交接摘要

> 供后续开发的智能体/协作者使用。更新于 2026-10-21，对应设计文档 **v1.10**（v1.9 四轴 + v1.10 普通档 20 张手感调整；85 张常规卡 + 两个真词条 + 9 个力量）、main 分支 `1bc1186` 之上的未提交改动。
> 前一版摘要的历史教训已合并进本文。

## 项目概况

- **目标**：为 Slay the Spire 2 开发自定义角色"赌徒"（金币混合费用 + 掷硬币赌博）
- **仓库**：`D:\worlk\sts2-mod`，远程 `github.com:hknnn99222/sts2-mod.git`（main，提交信息用中文）
- **阶段**：**代码全量完成（85 张常规卡 = 普 24 / 罕 35 / 稀 26）**。M0/M1 游戏内验收通过；**M2-M5 + v1.7/v1.8/v1.9 全批待游戏内验收**，里程碑见技术设计 §6
- **发布**：仓库根 `GamblerMod_v0.3.0.zip` 是旧包（未跟踪，已过期）；**当前 mod 版本 v0.6.0**（`Entry.cs` 的 `Version` 与 `GamblerMod.json` 的 `version` 已对齐）

## 当前状态（2026-10-15）

| 里程碑 | 状态 | 内容 |
|---|---|---|
| M0 骨架 | ✅ 验收通过 | GamblerMod 项目、BaseLib 3.4.7 引用、角色进选人界面 |
| M1 核心管线 | ✅ 验收通过 | Core 基建 + 本钱/试手气 + 自有卡池 + 掷硬币视听反馈；**底注已移出起始牌**（保留在卡池作奖励卡，`GamblerCharacter.cs:44`） |
| M2 做局 | ✅ 代码完成，待验收 | 幸运筹码遗物 + 暗记/手气正旺/赌神 + 干预窗口/正面率上限 75% |
| M3/M4 卡池 | ✅ 代码完成，待验收（43/43 全量） | 普通 11 + 罕见 19 + 稀有 13，含 X 费梭哈/真赌三件/引擎产金/全局钩子单例 |
| M5 风头正盛倍率（v1.6） | ✅ 代码完成，待游戏内验收 | 第二签名词条：头顶倍率徽章、正面积累/反面归零/兑现清空；+8 张新卡（#44–51）、改造 3 张（叠注/底牌/同花顺），卡池 43→**51**。设计 §2.8，实现记录（含落地偏离 + 待校准常量）见 [gambler-tech-multiplier.md](docs/design/gambler-tech-multiplier.md) §8 |
| v1.7 扩池至 80 + 收尾 | ✅ 代码完成（已部署），**待游戏内验收** | 新增 **29 张**（普 +7 / 罕 +13 / 稀 +9），配齐原版模板 **20 普 / 35 罕 / 25 稀 = 80 张**。全部按「一卡一钩子」取 §5.5 的空位形态（附加费·多段/买能量/穿透、即时产金·弃牌堆/敌人数/手牌数/手牌换金、花费追踪·转格挡/转能量、赌博·累积多掷/格挡反击、做局·倍率注入/翻倍/本回合全正、真赌·押HP/押手牌、特异·生命条件/弃牌堆规模/金币转伤/消耗堆 等） |
| **v1.10 普通档手感批次** | ✅ 代码完成（已部署），**待游戏内验收** | 20 张普通牌调整（用户逐张指定）：费用下调（买定离手 2→1 能、看场 5→3 金、护盘 4→2 金、摸牌 1能+5金→0能+4金、放水 4→3 金、吃水 4→3 金、护池 1能+4金→0能+5金、换码 0能+6金→0能+2金）；数值上调（底注 10→11、悬红 7→9/赏金 10→15、盲注 4/8→5/9、收手 5→6、乘胜 6→7、吃水 16 伤/破盾转金、放血 20 金）；机制改造（加注 可变投入→花费追踪每金+1伤、散码 去上限/升级抽1、点码 每金+1挡、洗牌 弃抽→消耗换 6 金、封口 抽2/升级加 2 必正）；类型变更（血本→**分红** 能力·金买血、放血 攻→技）。类型分布变为 **33 攻 / 43 技 / 9 能**；对账 `tools/gen_card_table.py --audit` 归零。验收清单见 test-plan M3 / 四·七 B1–B4 |
| **v1.9 四轴改造** | ✅ 代码完成，**待游戏内验收** | 按原版角色（亡灵契约师）的机制浓度重排：金币流水 / 出千·确定性 / 气势·倍率 / 赌命·下注 四条**能各自独立成型**的引擎（设计 §2.9）。+9 张（出千 5：封口/偷天换日/收网/千王/瞒天过海；赌命 4：血本/放血/押注/赌命）→ 卡池 76→**85（24/35/26）**；暗记 1→2 次必正。**新增 Core 接口** `GambleService.ForcedHeadsRemaining` / `ConsumeForcedHeads`；**新增 2 个力量** `BettingPower`（按已失去生命加成伤害）/ `CheatKingPower`（每回合前 N 次正面→1 次必正，**封顶防无限循环**）。验收清单见 test-plan「四·七」 |
| **v1.8 游玩体验优化** | ✅ 代码完成（已部署），**待游戏内验收（本轮重点）** | 9 条体验诉求全落地：①「赌博」「风头正盛」**注册为真词条**（`Core/GamblerKeywords.cs` + pck 里 `localization/zhs/card_keywords.json`，卡面自动拼词条名、悬停出提示）②14 张赌博牌描述统一「硬币为正时…硬币为负时…」③**卡牌图鉴 76 张全可见**（`GamblerCardPool.SeenByDefault => true`）④头顶倍率徽章可悬停（`MouseFilter = Pass` + `static_hover_tips.json`）⑤赌博牌正/反面数值改吃附魔（`Core/CardValueVars.cs`）⑥兑现卡面金币随倍率跳动 ⑦⑧⑨按用户原句写三条提示文案。**并删 4 张自掷牌**（豹子/老虎机/连庄/连胜）→ 卡池 80→76。验收清单见 test-plan「四·五-A」 |
| v1.7 收尾改动 | ✅ 已部署 | ①**全池改名成真实赌业术语**（底注/筹码/看场/悬红/护盘/返水/清台/派彩/兑筹/假筹/台规/爆机/单双/打庄/弃牌/抽佣/割禾青/兑现/通杀/晒冷/追数/割肉/散码/买码/吃水/点码/护池/换码/再加注/押宝/反水/叠码/保底/加码/清码/翻盘/暗注/抵押/换牌/全下/摊牌/保本/截胡/倍投/换筹；起始 私房钱→**本钱**）②**4 张攻击牌转防御**（围殴→护池、清点→保底、打水漂→抵押、金库→保本，格挡牌 9→13）③倍率徽章修好 + 校准 ④3 张新赌博牌数值对齐 1.2× 锚定 |
| 美术 | 部分完成 | 选人封面（486×720）+ 选人背景（1280×720 全屏场景）已上线；**卡面立绘仍是占位图（NoPE）**，正式硬币翻转 UI 动画同批后补 |
| 角色包装 | ✅ 完成 | 角色设定（维克托"九指"科因）、台词包、金色名字色（`e8c34a`）、尖塔城市/先古之民设定已进游戏 |

`GamblerMod.json` 里 `version` 还是 `v0.1.0`，与发布包 v0.3.0 不一致，下次发版顺手改。

**未提交的工作区改动（2026-10-17 批）**：删 4 张卡文件 + `GamblerCards.cs` 注册行、新增 `Core/GamblerKeywords.cs` 与 `Core/CardValueVars.cs`、`GamblerCard.cs`（词条挂载 + 自检日志）、`GamblerCardPool.cs`（图鉴可见）、`MultiplierBadge.cs`（悬停提示）、14+10+9 张卡的描述与数值变量、`Entry.cs`（版本 + touch + 计数文案）、`GamblerMod.json`（版本）、`assets/` 新增两个本地化 json + 重导的 pck；文档侧 `gambler-design.md`（v1.8）、`gambler-tech-design.md`（§11 变更记录）、`gambler-tech-multiplier.md`、`test-plan.md`（v0.5.0 + 四·五-A）、本文件、两份 xlsx。**两份 xlsx 早已纳入 git 跟踪**（HANDOFF 旧文写"未跟踪"是过期信息，2026-10-17 核实 `git ls-files` 确认）；markdown 仍是权威源，数据以卡牌代码为准。v1.7 版工作簿留了三份本地备份（`gambler-cards_v17_backup.xlsx` / `gambler-design_v17_backup.xlsx`，已加 .gitignore），确认无误后可删。

**测试文档**：[docs/testing/test-plan.md](docs/testing/test-plan.md)（M1-M4 逐项验收清单 + 已知简化偏离列表）。验收重点：M2 做局干预（幸运筹码改币、暗记必正、手气正旺/赌神叠加与 75% 上限）、43 张卡逐张打一遍、整局冒烟。

## 已核实的事实（勿重复调研）

### 环境
- .NET SDK 9.0.318 在 **D:\dotnet**（`DOTNET_ROOT`、`NUGET_PACKAGES=D:\nuget-packages` 已配置）；shell 里 PATH 可能解析到 C 盘运行时，用全路径 `/d/dotnet/dotnet.exe` 最稳
- 构建即部署：`cd GamblerMod && dotnet build` 自动拷 dll+json+pck 到游戏 mods 目录；**构建前必须关游戏**（dll 被锁部署会失败，编译仍成功——看报错是不是 MSB3027）
- 游戏日志（排障第一现场）：`%APPDATA%\SlayTheSpire2\logs\godot.log`
- BaseLib 3.4.7 源码本地克隆在 `/tmp/baselib-src`（临时目录，丢了重新 `git clone https://github.com/Alchyr/BaseLib-StS2.git`）；官方 ModTemplate 与 wiki 同理（`Alchyr/ModTemplate-StS2`，wiki 里有命令手册 Adding-Cards / Common-Commands-Cookbook 必读）
- **本项目实测 API 速查**：[notes/modding-api.md](notes/modding-api.md)（卡牌模型/战斗命令/状态查询/随机/模型钩子/遗物力量/本地化/角色 八类，含签名、用例文件、踩坑注记）——写新效果先翻这里，别直接啃 BaseLib 源码

### 引擎/BaseLib 语义（全部踩坑或源码核实）
1. **内容注册 = 实例化即注册**：`Custom*Model` 构造器自动调 `AddCharacter/AddModel`。不要调 `RegisterType(typeof)`——它只登记去重，还会让构造器里的真注册被跳过
2. **原版模型全是 ModelDb 单例**：`new Ironclad()` 抛 DuplicateModelException。取实例用 `ModelDb.Card<T>()` / `ModelDb.CardPool<T>()` / `ModelDb.GetById<T>(ModelDb.GetId<T>())`
3. **卡池必须被角色引用**：自定义卡池挂了卡但没角色引用 = 孤儿池，卡牌实例化抛 "not in any card pool"，抽牌/牌组 UI 全崩
4. **遗物同理且更隐蔽**：遗物必须挂 `[Pool(typeof(XxxRelicPool))]` 特性、且角色 `RelicPool` 指向自有池（`GamblerRelicPool`），否则角色初始化中断、**选人界面选不了赌徒**（修复见 `89010c5`）
5. **SimpleLoc**：卡描述必须 `#` 开头才会把 `!D!`/`!Var!` 转数值（`*文字*` 金色高亮，升级文本 `-旧-+新+`）；不写 `#` 就原样显示占位符。**金币费用文案规范："花费 X 金币"独立成卡描述首行**（全部双费卡已统一，梭哈/加注特化措辞）
6. **Godot res:// 区分大小写**：`PlaceholderID` 必须小写 `"ironclad"`（大写导致选人背景/转场加载失败，选人状态卡死）
7. **C# 主构造器语法陷阱**：`class X(int a) : Base` 语法无处赋值自定义字段——掷硬币卡必须用显式构造器设置 `IsGambleCard = true`，漏设则**掷硬币永远反面**（修复见 `2718c53`）
8. **静态字段初始化陷阱**：模型类里不要放会触发异常的静态字段（TypeInitializationException 会连累整个类型，症状是"选人后 UI 不刷新"这类玄学）
9. 卡牌升级 API：`DynamicVars.X.UpgradeValueBy(n)`；改能量费 `EnergyCost.UpgradeBy(n)`
10. 卡牌效果写法：看 ModTemplate wiki 的 Common-Commands-Cookbook（DamageCmd/CreatureCmd/PlayerCmd/CardPileCmd 流式命令）
11. 掷硬币随机：官方"内容专属 Rng"构造器 `new Rng(player, contentId, mixin)`——同内容重见会出同结果，**必须用战斗序号做 mixin**（GambleService 已实现）；多人下读档确定性的校准仍待做
12. 金币：`Player.Gold`（int）、`PlayerCmd.GainGold(dec, player, bool)` / `LoseGold(dec, player, GoldLossType.Spent)`；可打出检查挂 `PlayerCombatState.HasEnoughResourcesFor` Postfix（与 BaseLib 附加费用同出口）
13. **`MegaLabel._Ready()` 的字体断言是「抛异常」不是「打日志」**：缺主题字体覆盖时它会抛 `InvalidOperationException: … has no theme font override`。覆盖 `_Ready` 时必须先 `AddThemeFontOverride("font", GetThemeFont("font","Label") ?? ThemeDB.FallbackFont)` 再 `try { base._Ready(); }`，否则异常一抛、后面代码全跳过（徽章整块消失；2026-10-16 踩过）
14. **C# 节点的 `_Process` 靠不住**：`SetProcess(true)` 也未必被引擎回调（实测两次都没回调）。UI 需要逐帧刷新时，挂 **`GetTree().ProcessFrame` 信号**（`_ExitTree` 里退订）——信号连接不受节点 process 标志控制
14. 卡池能量图标缺失（`CARD_POOL∴…_energy_icon.png`）是占位缺口，卡描述里的能量符号会报 No loader found
15. 掷骰记录 `GambleService.Flips` 的作用域 = **单场战斗**（`GamblerHooks.AfterCombatEnd` 里 `GambleService.OnCombatEnd()` 清空；此前只在"新战斗首次掷币"时清，会残留上一场数据）
16. 选人美术覆写：角色 `CustomCharacterSelectIconPath`（封面）/ `CustomCharacterSelectBg`（背景场景）两个 override，资产经 pck 提供（见下）
17. **自定义卡牌词条（v1.8 已验证做法）**：BaseLib `[CustomEnum, KeywordProperties(AutoKeywordPosition.Before)] public static CardKeyword X;`（字段必须 public static，**不能 readonly**）+ pck 内 `GamblerMod/localization/<lang>/card_keywords.json`，键 = `根命名空间大写-字段名大写.title/.description`（本项目 `GAMBLERMOD-GAMBLE`）。BaseLib 在 `ModelDb.Init` 反射写入枚举值 → **词条值不可缓存进静态字段**（静态初始化早于写入，会固化成 0）。自定义 StaticHoverTip 同机制、走 `static_hover_tips.json`（键 `GAMBLERMOD-MULTIPLIER.title`，slug 是字段名的 UPPER_SNAKE）
18. **词条挂载必须走 `CanonicalKeywords` override，不要用 `AfterCreated` + `AddKeyword`**：后者的语义是"卡**实例**创建后"（`CardModel.AfterCreated`，sts2.xml 原文 "after the card is created (NOT during deserialization)"），图鉴/牌堆里的**模型本体**拿不到词条（撕毁契约的「消耗」就是走的这条路，图鉴里看不到）
19. **卡牌图鉴（百科）可见性**：BaseLib `CustomCardPoolMarkAsSeenPatch` 在打开图鉴时对所有 `SeenByDefault == true` 的池调 `SaveManager.MarkCardAsSeen`，`CustomCardPoolModel` 默认 **false** → 不覆写的话整池在图鉴里渲染成"未知/未解锁"。覆写一行即"开局全可见"（落进存档进度）
20. **卡面数值必须用引擎的原生变量类型**（2026-10-17 游戏内实测踩坑）：卡面的 `BaseValue/EnchantedValue/PreviewValue` 只有**引擎认得的类型**才会跑完整预览管线（附魔 + 力量 + 虚弱 + 易伤 + 敏捷）。把伤害/格挡数值写在 `IntVar` 上 → 卡面永远裸值（玩家看到"正面不吃增伤、不吃虚弱"，与结算对不上）。**`DamageVar`/`BlockVar` 都有 `(string 名字, decimal 数值, ValueProp props)` 构造器**（元数据核实）→ 一张卡想放两套伤害/格挡数值就 `new DamageVar("HeadsDamage", 10, ValueProp.Move)`，自动与标准 `!D!` 行为一致。`DynamicVar.Name` 只有 getter，改不了名（要自定义名字必须走构造器重载）。**结算侧仍传 `BaseValue`**（文档明示 BaseValue 才是"改游戏状态"该用的值）。金币/能量**没有任何修饰/附魔通道**，只能用自定义变量（本项目 `MultiplierAwareVar`）
21. **悬停提示的定位**：`NHoverTipSet.CreateAndShow(owner, tip)` 之后**必须** `SetAlignment(owner, HoverTipAlignment.Center)`（或 Left/Right）+ `SetFollowOwner()`，否则提示节点停在 (0,0)，实测就是"提示跑到屏幕左上角"。参考 BaseLib `Patches/UI/CharacterSelectStartingRelicsPatch.cs` 的遗物悬停写法

## 数值口径（v1.6，改卡前必读）

- **金币汇率**：2.1 伤害/金币、2.5 格挡/金币；双费卡的"能+金"组合按此折算定价（17 张已全量重定价）
- **能量↔金币换算（v1.5）**：**1 能量 = 7 金币**——产金与补贴的统一换算基准（取代按稀有度分档的 4-6/5-10/8-15 口径）。产金牌按此重定：筹码 1 能 → 7 金（基准样板；原 0 能 3 金）、本钱 0 能 → 5 金（升级 7，免能量特例），其余产金牌同步上调（抽头 7/正 14、派彩 18、兑筹 12、高利贷 14、弃牌 14、老虎机每次 9、赏金 10、清台 7、爆机 25、返水 3/回合、台规 4/回合+正面 3、利滚利 +2）
- **赌博期望锚定**：E = 1.2×白板基准（反面 ≈ ×0.8、正面 ≈ ×1.6，公式 0.8+0.8p）；动态数据见设计文档 §5.4 期望校验表
- **「风头正盛」倍率（v1.6，已实装）**：正面积累 +0.5 / 反面归零 / 带词条的牌数值 ×倍率后清空；**裸值 = 白板**、**不设上限**（含金币，经济风险用户已接受）。关联 12 张牌（**10 挂词条**：8 张新卡 + 同花顺 + 换筹；**2 只读条件**：叠注、底牌）；**幸运筹码改币照常归零**（2026-10-16 用户拍板：遗物只保牌效、不保连胜）。实现要点：倍率存 `Core/MultiplierService`（int 记 0.5 刻度），乘算走 `GamblerHooks` 的 `ModifyDamage/BlockMultiplicative`（**金币没有带 cardSource 的钩子 → 兑现在卡内自算、卡面显示裸值**）
- **平衡旋钮**：设计文档 §7.3 / xlsx「平衡旋钮清单」sheet——改平衡先动那六个螺丝
- **卡池规模（v1.9/v1.10）**：**85 张常规卡 = 普 24 / 罕 35 / 稀 26**（v1.7 曾配平到 80；v1.8 删 4 张自掷牌；v1.9 四轴补 9 张；v1.10 只调不改数）+ 起始 10 张；类型 **33 攻 / 43 技 / 9 能**
- **四轴引擎（v1.9，设计 §2.9）**：①金币流水 ~28%（产金 15 + 花费追踪 4 + 持有量转化）②出千·确定性 ~12%（必正可攒 → 收网一次性兑现；做局 6 + 新 5）③气势·倍率 ~19%（风头 10 + 只读 2 + 注入 2）④赌命·下注 ~13%（真赌 5 + 自伤换收益 6）。**赌博已降级为「通用投递方式」**，不再是全池唯一主轴
- **两个真词条（v1.8）**：「赌博」（挂 14 张掷币牌，条件就是 `IsGambleCard`）、「风头正盛」（挂 10 张数值×倍率牌）；词条名由引擎自动拼进描述、悬停卡牌出提示；卡面正文**不再手写**词条名
- 起始金币 **148**（设定梗：本该 150，兑筹抽走两枚"手续费"）；HP 72
- 改数值只动卡文件（`GamblerMod/src/Cards/`），Core 层不动

## 代码架构（技术设计 §3/§4 已冻结口径）

```
GamblerMod/src/
├── Entry.cs               # 唯一注册点：PatchAll → 卡牌实例 → 角色（顺序固定）
├── Core/                  # 共用层，约束④：改卡不碰这里
│   ├── GamblerCard.cs     # 模板基类：扣金→掷币→ApplyEffect(heads)；含正反面特效常量
│   ├── GoldCost.cs / GoldCostCheckPatch.cs / SpendTracker.cs
│   ├── GambleService.cs   # 掷硬币管线（正面率上限 75%、做局干预窗口）
│   └── GamblerHooks.cs    # 全局钩子单例（含防重复订阅保险；回合开始重置 SpendTracker）
├── Character/             # GamblerCharacter + GamblerCardPool + GamblerRelicPool
├── Relics/ + Powers/      # 幸运筹码；手气正旺/赌神/台规/流水/利滚利/返水
└── Cards/                 # Basic/Common/Uncommon/Rare 每卡一文件 + GamblerCards 单例仓
```

**加一张新卡的完整流程**：复制同 rarity 现有卡改类名/构造四元组/CanonicalVars/OnUpgrade/ApplyEffect/Localization → GamblerCards.cs 加属性+Init 一行 → `dotnet build`（游戏关闭状态）。**掷硬币卡记得显式构造器里 `IsGambleCard = true`**（见坑 #7）。改数值只动卡文件（已演练验证）。

## 待办与已知问题

- [ ] **M5 游戏内验收**：按 [docs/testing/test-plan.md](docs/testing/test-plan.md) 四·五 逐项过。徽章偏移已校准到 `HeadOffsetY = -58f`（再调看日志"全局锚点 (x, y)"）
- [ ] **v1.8 验收**：test-plan「四·五-A」12 项——词条悬停/句式/图鉴全可见/徽章悬停/**锋利 2 落正反两支且卡面同步**/兑现卡面随倍率跳/删卡确认。附魔那两项有 `数值自检` 日志可对账
- [ ] **v1.9 验收（本轮重点）**：test-plan「四·七」10 项——9 张新卡逐张 + **四轴独立成型**（只用①或只用④各打一局）；重点看 `收网` 的必正消耗、`千王` 是否被封顶、`押注` 的卡面伤害是否跟着血线变
- [ ] **M2–M5 + v1.7 一起补游戏内验收**：M2–M4（做局/43 卡）原本就未验收，M5 改了其中 3 张、v1.7 又加 29 张，建议一轮过完；已知简化偏离不计 bug
- [ ] 卡面立绘全是占位图（"NoPE"）——美术资产后补；正式硬币翻转 UI 动画同批
- [ ] 多人战斗序号 mixin 的读档确定性校准
- [ ] `GamblerMod.json` version 字段 v0.1.0 → 跟发布包版本对齐
- [ ] xlsx 三件套待提交（见"当前状态"）；提交后 zip 是否进 .gitignore 一并定
- [ ] **实现 M5「风头正盛」倍率系统**：✅ 代码已完成（2026-10-16）；此后任何改卡数值/加卡，**先读 [gambler-tech-design.md](docs/design/gambler-tech-design.md)** 的约束①–④ 与 §5 分配红线（R1–R8），并遵循 [gambler-tech-multiplier.md](docs/design/gambler-tech-multiplier.md) §0 的门禁写法（说明理由 + 只加不改 + 追加变更记录）
- [x] ~~`GamblerMod.json` version 字段 v0.1.0 → 跟发布包版本对齐~~ **已修（2026-10-17）**：Entry 与 json 同步为 **v0.5.0**
- [ ] **文档同步状态（2026-10-16 更新；2026-10-17 已按 v1.8 再同步一轮）**：xlsx 三个 sheet + test-plan 验收数值曾整表滞后于代码（v1.4 重定价只同步了设计文档），已按「代码=真源」全量回灌；v1.6 整表重建为 51 行、**v1.7 再重建为 80 行**（说明行随之下移到第 86 行）。**改卡数值/加卡后要同步这四处**：设计文档 §5、xlsx 常规卡池 + 期望校验、test-plan、HANDOFF；xlsx 卡表列序**按 # 升序**（稀有度分组只在设计文档里保持）；**对账与重生工具（2026-10-16 新增）**：`python tools/gen_card_table.py --audit` 逐卡比对「代码 ↔ 设计文档 §3/§5/§6/§7.3 ↔ 两份 xlsx 的卡表/分布统计/期望校验」，报出差异并给出退出码；去掉 `--audit` 重生 `docs/design/gambler-cards-classified.{xlsx,md}`（功能分类 + 全卡表，数值直接取卡文件 CanonicalVars 真实值）。改完卡先跑 --audit 再决定重生。
- [x] ~~台规"正面 +X 金"硬编码在 Core~~ **已修（2026-10-16）**：迁移至 `Powers/HouseRulesPower.OnHeadsFlipped`（Entry 注册订阅），改数值只动 power 文件；详见 tech-design §10 变更记录
- [ ] 设计侧扩展期（不阻塞）：多人卡/职业遗物扩展/药水/事件（先古之民重逢事件可埋）

## 资产管线（.pck，已跑通）

- Godot 4.5.1（与游戏引擎 MegaDot 4.5.1 同版）装在 `D:\tools\godot`，用 console 版可执行文件
- 资产工程在 `assets/`：图片放 `GamblerMod/images/<分类>/`，场景放 `GamblerMod/scenes/`；
  已有：选人封面（charui/char_select_gambler.png，486×720 竖版裁剪）、选人背景
  （scenes/screens/char_select/char_select_bg_gambler.tscn = TextureRect 全屏锚点 + 保持比例铺满）
- 导出两条命令（在 assets/ 下）：
  `Godot_v4.5.1-stable_win64_console.exe --headless --import .`
  `Godot_v4.5.1-stable_win64_console.exe --headless --export-pack "GamblerMod" export/GamblerMod.pck`
- `dotnet build` 自动把 pck 随 dll 部署到游戏 mods；清单 `has_pck: true`
- 规格速查：卡牌立绘 1000×760（全艺术 606×852）、遗物 64/256、选人封面 2:3 竖版
- 手写 .tscn 的 ext_resource 可省 uid（按 path 解析）；pck 内容可用 `strings xxx.pck` 快速核对

## 历史事故（别重蹈）

- 改 PATH 一律用 PowerShell `[Environment]::SetEnvironmentVariable`，**禁用 setx + git-bash 变量展开**（曾污染注册表 PATH）
- git-bash 里路径结尾 `\"` 会转义引号导致整条命令解析失败（用正斜杠或去掉尾反斜杠）
- Windows 原生 python 读不到 git-bash 的 `/tmp`，临时文件放 `$USERPROFILE` 下
- MSB3027 部署失败 = 游戏没关（或残留进程），`taskkill //PID <pid> //F` 后重试
- zip 里中文文件名用 GBK 编码，git-bash 的 unzip 显示乱码但 Windows 资源管理器正常——不是 bug，别试图"修复"
