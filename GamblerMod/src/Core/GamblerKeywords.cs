using BaseLib.Patches.Content;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.HoverTips;

namespace GamblerMod.Core;

// 自定义词条（BaseLib「自定义枚举」机制，2026-10-17 游玩体验优化批次 2）：
// 把「赌博」「风头正盛」注册成真正的 CardKeyword —— 于是
//   ①引擎把词条名自动拼进卡描述（AutoKeywordPosition.Before，走 BaseLib 的 AutoKeywordText 补丁）；
//   ②悬停卡牌时由 HoverTipFactory.FromKeyword 补丁弹出提示（BaseLib CustomTooltips）。
//
// 硬约束（BaseLib 源码语义，改前必读）：
//   - 字段必须 public static、类型 CardKeyword（readonly 会赋值失败）；
//   - 词条 ID = 根命名空间大写 + '-' + 字段名大写 → GAMBLERMOD-GAMBLE / GAMBLERMOD-HOTSTREAK；
//   - 文案在 pck：GamblerMod/localization/<lang>/card_keywords.json，
//     键 = 词条 ID + ".title" / ".description"（BaseLib 自带词条 BASELIB-PURGE 同款做法）；
//   - 词条值由 BaseLib 在 ModelDb.Init 反射写入 → **绝不可把词条值存进静态字段**：
//     静态初始化早于写入，会固化成 0（= 设计陷阱 #8 的变体）。GamblerCard 每次都现读本类字段。
public class GamblerKeywords
{
    /// <summary>赌博：投一枚硬币，根据硬币正反决定效果。挂所有会掷硬币的牌。</summary>
    [CustomEnum, KeywordProperties(AutoKeywordPosition.Before)]
    public static CardKeyword Gamble;

    /// <summary>风头正盛：卡牌数值随倍率变化。只挂真正「数值 ×倍率」的 10 张牌。</summary>
    [CustomEnum, KeywordProperties(AutoKeywordPosition.Before)]
    public static CardKeyword HotStreak;
}

// 自有静态悬停提示（2026-10-17 批次 5）：头顶倍率徽章不是卡牌，挂不了 CardKeyword，
// 所以走 BaseLib 的「自定义 StaticHoverTip 枚举」通道——文案在
// GamblerMod/localization/<lang>/static_hover_tips.json，键 = GAMBLERMOD-MULTIPLIER.title/.description。
// 用法：HoverTipFactory.Static(GamblerHoverTips.Multiplier, Array.Empty<DynamicVar>())。
public class GamblerHoverTips
{
    /// <summary>倍率徽章的悬停提示。标题「倍率」。</summary>
    [CustomEnum]
    public static StaticHoverTip Multiplier;
}
