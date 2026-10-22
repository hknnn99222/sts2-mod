# 赌徒 Mod 美术资源需求清单

> 版本：v1.0（2026-10-22）｜对应 mod v0.6.0 / 设计 v1.15（87 张卡、13 个能力、1 个遗物）
> 尺寸均为**原版资源实测**（用 Godot 4.5.1 加载游戏 pck 量取），不是估算值。

**目录**
1. 现状总览
2. 规格与命名规范
3. 卡牌立绘（87 张）
4. 能力图标（13 个）
5. 遗物（1 个）
6. 角色与角色 UI
7. 硬币翻转与倍率 UI
8. 交付与接入流程
9. 优先级与工作量估算
10. 验收清单
11. 进度勾选表

## 1. 现状总览

**总量**：**P0 基础套装 102 张**（87 卡图 + 13 能力小图标 + 1 遗物 + 1 能量字形）；**P1 体验套件 19 张**（13 能力大图标 + 2 角色 UI + 2 硬币 + 2 正反提示）；另加 1 套 Spine 战斗动画、1 段翻转动画/着色器、4 个音效。

| 类别 | 数量 | 尺寸 | 状态 |
|---|---|---|---|
| 卡牌立绘 | 87 | 1000×760 | 未提供（全部 NoPE） |
| 能力小图标 | 13 | 64×64 | 未提供 |
| 能力大图标 | 13 | 256×256 | 未提供 |
| 遗物图标 | 1 | 256×256 | 未提供 |
| 卡池能量字形 | 1 | 24×24 | 缺失（会报错） |
| 角色 UI（锁定头像/地图标记） | 2 | 132×195 / 64×64 | 未提供 |
| 硬币 UI | 2 面 + 动画 | 256×256 | 未做 |
| 战斗角色 Spine | 1 套 | — | 暂用铁甲战士 |

| 资源 | 状态 | 说明 |
|---|---|---|
| 选人封面（大图） | ✅ 已有 | `char_select_gambler.png`，486×720 |
| 选人背景 | ✅ 已有 | `char_select_bg_gambler.png`，1280×720 + 全屏 TextureRect 场景 |
| **卡牌立绘 ×87** | ❌ 全部占位 | 游戏内显示 "NoPE" 占位图 |
| **能力图标 ×13** | ❌ 未提供 | 状态栏显示 BaseLib 的占位图标 |
| **遗物图标 ×1** | ❌ 未提供 | 幸运筹码仍是占位 |
| **卡池能量字形** | ❌ 缺失 | 卡面费用里的能量符号触发 `No loader found` |
| 战斗角色 | ❌ 占位 | 当前借用铁甲战士的 Spine 动画 |
| 硬币翻转 UI | ❌ 未做 | 现在用通用打击特效 + 音效代替正/反反馈 |
| 卡框 / 卡背 / 卡牌底纹 | ⚪ 用库默认 | 暗金色调（HSL 0.12/0.70/0.80），可后换 |
| 营火/商人动画、地图标记、锁定头像、能量球、RPS 手、彩蛋饼干 | ❌ 未做 | 均属可选增强 |

## 2. 规格与命名规范

**目录**（全部放在 `assets/GamblerMod/images/...`，导出后会进 `res://GamblerMod/images/...`）：

```
images/
├─ card_portraits/            卡牌立绘（87）
├─ powers/                    能力小图标（13）
│   └─ big/                   能力大图标（13）
├─ relics/                    遗物图标（1）
├─ sprite_fonts/              卡池能量字形（24×24）
└─ charui/                    角色 UI（已有封面/背景，其余待补）
```

| 类别 | 尺寸（实测） | 格式 | 备注 |
|---|---|---|---|
| 卡牌立绘 | **1000×760** | PNG，不透明或带底图 | 四周留 ~60px 安全区，卡框/费用/描述会压边 |
| 遗物图标 | **256×256** | PNG 透明 | 建议同时给一张 512 大图 |
| 能力小图标 | **64×64** | PNG 透明 | 状态栏/图标行使用 |
| 能力大图标 | **256×256** | PNG 透明 | 悬停/详情使用，文件名加 `_big` |
| 卡池能量字形 | **24×24** | PNG，白色单色 | 卡面费用里的能量符号 |
| 选人小图标 | **132×195**（原版） | PNG | 锁定头像同尺寸，建议灰阶剪影 |

**命名**：一律 `snake_case`，卡牌用**类名转 snake**（下表已给出），不要用中文名或编号，代码按约定拼路径。

## 3. 卡牌立绘（87 张）

分档：起始 2 + 普通 24 + 罕见 35 + 稀有 26。
统一风格建议：暗金 + 墨绿赌场色、木质牌桌、金币高光；攻击牌偏动势、技能牌偏道具特写、能力牌偏符号化（图标感）。下方「参考画面」只是给画师的意象提示，不必逐字照搬。

| # | 卡名 | 档位 | 建议文件名 | 参考画面 |
|---|---|---|---|---|
| — | 本钱 | 起始 | `private_stash.png` | 一叠筹码与钱包 |
| — | 试手气 | 起始 | `try_your_luck.png` | 指尖弹起一枚硬币 |
| 1 | 底注 | 普通 | `flying_card.png` | 两枚金币压在牌角 |
| 2 | 加注 | 普通 | `add_more.png` | 筹码越堆越高的赌桌 |
| 3 | 买定离手 | 普通 | `gamble_all.png` | 双手推出一大把筹码 |
| 4 | 打手 | 普通 | `thug.png` | 黑衣保镖挥拳 |
| 5 | 悬红 | 普通 | `bounty_hunter.png` | 通缉令与赏金袋 |
| 6 | 筹码 | 普通 | `small_change.png` | 一枚发亮的筹码 |
| 7 | 护盘 | 普通 | `buy_peace.png` | 筹码拼成的盾墙 |
| 8 | 跟注 | 普通 | `flash_of_insight.png` | 硬币落在牌堆上 |
| 9 | 摸牌 | 普通 | `buy_the_road.png` | 指尖从牌堆抽出两张 |
| 10 | 放水 | 普通 | `trip_up.png` | 故意松手的骰子 |
| 11 | 返水 | 普通 | `piggy_bank.png` | 回流的小金币 |
| 44 | 收手 | 普通 | `hold_back.png` | 按住鼓起的筹码袋 |
| 45 | 乘胜 | 普通 | `ride_the_win.png` | 顺着势头再推筹码 |
| 52 | 捡漏 | 普通 | `loose_change.png` | 从弃牌堆里捡筹码 |
| 53 | 人头费 | 普通 | `head_tax.png` | 一排敌人头顶悬着金币 |
| 54 | 抽税 | 普通 | `take_cut.png` | 刀光劈开盾牌、金币迸出 |
| 55 | 记账 | 普通 | `count_money.png` | 账本与算盘 |
| 56 | 典当 | 普通 | `shuffle_up.png` | 当票压着一张旧牌 |
| 57 | 封盘 | 普通 | `pile_on.png` | 封条贴在牌桌上 |
| 81 | 发牌 | 普通 | `hush_money.png` | 荷官手指夹着两张牌 |
| 82 | 偷天换日 | 普通 | `bait_and_switch.png` | 袖中换牌 |
| 83 | 分红 | 普通 | `blood_capital.png` | 金币化成心形生命 |
| 84 | 放血 | 普通 | `bleed_out.png` | 一滴血落成金币 |
| 58 | 充能 | 普通 | `buy_off.png` | 金币灌进能量槽 |
| 12 | 梭哈 | 罕见 | `all_in.png` | 双手把全部筹码推出去 |
| 13 | 连续下注 | 罕见 | `consecutive_bet.png` | 连成一排的筹码柱 |
| 14 | 清台 | 罕见 | `ransack.png` | 桌面被一扫而空 |
| 15 | 双倍下注 | 罕见 | `double_down.png` | 两摞等高的筹码 |
| 16 | 叠注 | 罕见 | `parlay.png` | 筹码叠成小塔 |
| 17 | 翻本 | 罕见 | `comeback.png` | 抛起的硬币与钱袋 |
| 19 | 摇骰 | 罕见 | `hired_thugs.png` | 掷出多枚骰子 |
| 20 | 暗记 | 罕见 | `secret_mark.png` | 指甲在牌背刻记号 |
| 21 | 派彩 | 罕见 | `dividend.png` | 彩金雨 |
| 22 | 数牌 | 罕见 | `count_cards.png` | 手中扇形展开的牌 |
| 23 | 重注 | 罕见 | `pawnshop.png` | 一大摞金币压出力量 |
| 24 | 抽头 | 罕见 | `rake.png` | 从筹码堆抽走一枚 |
| 25 | 保险 | 罕见 | `insurance.png` | 盾牌上嵌着一枚硬币 |
| 26 | 假币 | 罕见 | `counterfeit.png` | 两枚重叠的假金币 |
| 27 | 高利贷 | 罕见 | `loan_shark.png` | 借据与鼓胀的钱袋 |
| 28 | 手气正旺 | 罕见 | `hot_streak.png` | 连续正面的硬币在发光 |
| 29 | 保护费 | 罕见 | `house_rules.png` | 守门人伸手收钱 |
| 30 | 流水 | 罕见 | `cash_flow.png` | 金币汇成一条河 |
| 46 | 见好就收 | 罕见 | `quit_while_ahead.png` | 抓起一把筹码离桌 |
| 47 | 兑现 | 罕见 | `settle_up.png` | 筹码换成金币 |
| 48 | 通杀 | 罕见 | `the_finale.png` | 一把牌通吃整桌 |
| 59 | 再加注 | 罕见 | `master_bettor.png` | 第三摞筹码落下 |
| 60 | 做手脚 | 罕见 | `skim.png` | 袖口露出换牌的手 |
| 61 | 押宝 | 罕见 | `desperate_throw.png` | 把心押在牌上 |
| 62 | 还手 | 罕见 | `backhand.png` | 盾与匕首交叉 |
| 63 | 加码 | 罕见 | `bank_wind.png` | 筹码塔再叠一层 |
| 64 | 回收 | 罕见 | `tally_up.png` | 从消耗堆里回收筹码 |
| 65 | 底气 | 罕见 | `rake_fee.png` | 钱袋撑起一团能量 |
| 66 | 清仓 | 罕见 | `collect_debts.png` | 手牌尽数抛向桌心 |
| 67 | 翻盘 | 罕见 | `sink_boats.png` | 逆风翻起的一张牌 |
| 68 | 暗注 | 罕见 | `under_the_table.png` | 桌下递出的一袋金币 |
| 70 | 留一手 | 罕见 | `skip_stone.png` | 一张牌藏在袖中 |
| 71 | 换牌 | 罕见 | `swap_chips.png` | 两张牌在空中交换 |
| 85 | 收网 | 罕见 | `close_the_net.png` | 网住发光的必正标记 |
| 86 | 拼命 | 罕见 | `betting_card.png` | 血线化作刀刃 |
| 31 | 孤注一掷 | 稀有 | `all_or_nothing.png` | 一枚硬币与空钱袋 |
| 32 | 俄罗斯轮盘 | 稀有 | `russian_roulette.png` | 左轮弹巢与一枚子弹 |
| 33 | 爆机 | 稀有 | `greed_smash.png` | 老虎机爆出金币 |
| 35 | 同花顺 | 稀有 | `flush.png` | 五张同花色牌 |
| 36 | 单双 | 稀有 | `destiny.png` | 骰子点数与金币、力敏符号 |
| 38 | 打庄 | 稀有 | `hit_man.png` | 重拳砸向庄家座位 |
| 39 | 弃牌 | 稀有 | `tear_contract.png` | 一张牌被撕成两半 |
| 40 | 暗算 | 稀有 | `blackmail.png` | 背后递出的匕首 |
| 41 | 底牌 | 稀有 | `hole_card.png` | 翻开一张底牌 |
| 42 | 赌神 | 稀有 | `gambling_god.png` | 赌神剪影与光环 |
| 43 | 利滚利 | 稀有 | `compound_interest.png` | 金币生出小金币 |
| 49 | 存钱 | 稀有 | `all_in_to_the_end.png` | 金币堆成盾墙 |
| 50 | 追击 | 稀有 | `comeback_round.png` | 连击的刀光 |
| 51 | 止损 | 稀有 | `go_for_broke.png` | 举盾挡住下坠的筹码 |
| 72 | 全下 | 稀有 | `shove_all_in.png` | 双手推空桌面 |
| 73 | 出千 | 稀有 | `cheat_deal.png` | 袖中藏牌 |
| 74 | 摊牌 | 稀有 | `pip_count.png` | 全部手牌摊开 |
| 75 | 保本 | 稀有 | `the_vault.png` | 金币流成护甲 |
| 76 | 押注 | 稀有 | `ante_up.png` | 押上五枚金币换能量 |
| 77 | 过路费 | 稀有 | `sweep_all.png` | 关卡前收钱与盾 |
| 78 | 倍投 | 稀有 | `redouble.png` | 倍率数字翻倍 |
| 79 | 提现 | 稀有 | `cash_chips.png` | 倍率化作能量 |
| 80 | 满贯 | 稀有 | `grand_slam.png` | 满桌牌同时亮起 |
| 87 | 千王 | 稀有 | `cheat_king.png` | 王冠与袖中牌 |
| 88 | 瞒天过海 | 稀有 | `smoke_and_mirrors.png` | 斗篷下换牌 |
| 89 | 赌命 | 稀有 | `stake_your_life.png` | 心跳曲线与血线 |

## 4. 能力图标（13 个，小图 64×64 / 大图 256×256）

| 类名 | 名称 | 小图文件名 | 大图文件名 | 参考画面 |
|---|---|---|---|---|
| HotStreakPower | 手气正旺 | `hot_streak_power.png` | `big/hot_streak_power_big.png` | 发光的硬币 |
| GamblingGodPower | 赌神 | `gambling_god_power.png` | `big/gambling_god_power_big.png` | 王冠与骰子 |
| PiggyBankPower | 返水 | `piggy_bank_power.png` | `big/piggy_bank_power_big.png` | 小猪存钱罐 |
| HouseRulesPower | 保护费 | `house_rules_power.png` | `big/house_rules_power_big.png` | 收钱的印章/门禁 |
| CashFlowPower | 流水 | `cash_flow_power.png` | `big/cash_flow_power_big.png` | 金币汇成水流 |
| CompoundInterestPower | 利滚利 | `compound_interest_power.png` | `big/compound_interest_power_big.png` | 金币生小金币 |
| BettingPower | 拼命 | `betting_power.png` | `big/betting_power_big.png` | 血线化作刀刃 |
| CheatKingPower | 千王 | `cheat_king_power.png` | `big/cheat_king_power_big.png` | 王冠 + 袖中牌 |
| DividendPower | 分红 | `dividend_power.png` | `big/dividend_power_big.png` | 金币与心形 |
| LoanInterestPower | 利息 | `loan_interest_power.png` | `big/loan_interest_power_big.png` | 借据/沙漏 |
| RoulettePower | 俄罗斯轮盘 | `roulette_power.png` | `big/roulette_power_big.png` | 左轮弹巢 |
| ShowdownPower | 存钱 | `showdown_power.png` | `big/showdown_power_big.png` | 金币堆成盾 |
| CapitalGuardPower | 保本 | `capital_guard_power.png` | `big/capital_guard_power_big.png` | 金币护甲 |

## 5. 遗物（1 个）

| 名称 | 文件名 | 尺寸 | 参考画面 |
|---|---|---|---|
| 幸运筹码 | `images/relics/lucky_chip.png` | 256×256（另可给 512 大图） | 边缘磨得发亮的老筹码，写着半行小字"改天再来" |

## 6. 角色与角色 UI

| 资源 | 尺寸/格式 | 状态 | 用途与备注 |
|---|---|---|---|
| 选人封面 | 486×720 PNG | ✅ 已有 | 角色选择界面大图 |
| 选人背景 | 1280×720 PNG + tscn | ✅ 已有 | 选人界面全屏背景 |
| 锁定头像 | 132×195 PNG | 待做 | 未解锁时显示的灰阶剪影 |
| 地图标记 | ~64×64 PNG | 待做 | 爬塔地图上的角色头像 |
| 能量字形 | 24×24 PNG（白色单色） | **缺失（会报错）** | 卡面/UI 里的能量符号；缺它时日志出现 `No loader found` |
| 能量球 | tscn 场景 + 贴图/粒子 | 待做 | 战斗中的能量计数器外观；可先沿用原版 |
| 战斗角色 Spine | `.skel` + `.atlas` + `.png` | 待做 | 需 idle / attack / hit / cast / die 五组动画；当前用铁甲战士占位 |
| 营火动画 | Spine/序列帧 | 可选 | 休息处角色动画 |
| 商人动画 | Spine/序列帧 | 可选 | 商店角色动画 |
| 石头剪刀布四手 | 4 张 PNG | 可选 | 猜拳事件的出手图（石头/剪刀/布/指向） |
| 彩蛋饼干 | PNG | 可选 | 角色彩蛋道具 |

## 7. 硬币翻转与倍率 UI（玩法核心，优先做）

| 资源 | 规格建议 | 说明 |
|---|---|---|
| 硬币正面 | 256×256 PNG | 金色，带"正/头"符号 |
| 硬币反面 | 256×256 PNG | 暗色，带"反/字"符号 |
| 翻转动画 | 序列帧 8–12 帧，或两份贴图 + shader | 推荐 shader：Y 轴旋转 + 高光扫过，省帧数 |
| 结果提示 | 2 张图形（正/反） | 掷出后 0.5 秒的放大提示 |
| 倍率徽章底图 | 可选 | 当前头顶倍率是纯文字，可加底框/刻度 |
| 音效 | `coin_flip` / `heads` / `tails` / `gold_gain` | 现在正反面靠通用打击音效区分 |

## 8. 交付与接入流程

1. 按上表命名放进 `assets/GamblerMod/images/...`（目录不存在就新建）。
2. 在 `assets/` 下跑一次 `Godot_v4.5.1 --headless --import .`（导入新图）。
3. 重新导出资源包：`--headless --export-pack "GamblerMod" export/GamblerMod.pck`。
4. 构建 mod（会自动把 dll/json/pck 拷进游戏 mods 目录）。
5. 代码侧接入点：卡牌 `CustomPortraitPath`、能力 `CustomPackedIconPath`/`CustomBigIconPath`、遗物图标 override、卡池能量字形（池 id 约定）。这些会在第一批图到位时一次接好。

> 注意：文件名一旦改了就要同步改代码约定；建议按本表命名，不要自行改名。卡图不需要升级版，升级只改数值和卡面文字。

## 9. 优先级与工作量估算

| 优先级 | 内容 | 数量 | 说明 |
|---|---|---|---|
| **P0（必须）** | 卡牌立绘 | 87 | 工作量绝对大头；决定"看起来像不像一个完整角色" |
| **P0** | 能力小图标 | 13 | 状态栏不占位 |
| **P0** | 遗物图标 | 1 | |
| **P0** | 卡池能量字形 | 1 | 缺了会报加载错误 |
| **P1（体验）** | 能力大图标 | 13 | 悬停详情 |
| **P1** | 锁定头像 / 地图标记 | 2 | 选人与地图完整度 |
| **P1** | 硬币正反面 + 翻转 | 2+动画 | 赌博是签名词条，反馈必须做 |
| **P2（增强）** | 战斗角色 Spine | 1 套 | 目前借铁甲战士，工作量仅次于卡图 |
| **P2** | 能量球 / 营火 / 商人 / RPS / 饼干 / 卡框 | — | 有余力再做 |

## 10. 验收清单

- [ ] 87 张卡在牌组、手牌、奖励、图鉴四处都显示新图，无 "NoPE"
- [ ] 13 个能力在状态栏显示小图标，悬停显示大图标
- [ ] 幸运筹码在遗物栏显示新图标
- [ ] 卡面费用里的能量符号正常显示、日志无 `No loader found`
- [ ] 硬币翻转有正/反两面 + 动画 + 音效（不再借用打击特效）
- [ ] 选人界面锁定/解锁头像、地图标记正确
- [ ] pck 重建后无缺失资源报错（`godot.log` 搜 ERROR）

## 11. 进度勾选表（给画师/对接人用）

**P0**
- [ ] 卡图 起始（2）：private_stash / try_your_luck
- [ ] 卡图 普通（24）
- [ ] 卡图 罕见（35）
- [ ] 卡图 稀有（26）
- [ ] 能力小图标 13
- [ ] 遗物图标 1
- [ ] 卡池能量字形 1

**P1**
- [ ] 能力大图标 13
- [ ] 锁定头像 1
- [ ] 地图标记 1
- [ ] 硬币正反面 2
- [ ] 翻转动画 / 着色器
- [ ] 正/反结果提示 2
- [ ] 音效 4（coin_flip / heads / tails / gold_gain）

**P2**
- [ ] 战斗角色 Spine（idle/attack/hit/cast/die）
- [ ] 能量球场景
- [ ] 营火动画 / 商人动画
- [ ] 石头剪刀布四手
- [ ] 彩蛋饼干
- [ ] 自定义卡框/卡背
