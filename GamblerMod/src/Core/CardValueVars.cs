using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace GamblerMod.Core;

/// <summary>额外数值变量吃哪一类附魔加成（金币/能量没有附魔通道 → None）。</summary>
public enum EnchantKind
{
    None,
    Damage,
    Block,
}

/// <summary>
/// 附魔修正的唯一出口（2026-10-17 游玩体验优化批次 4，技术设计「变更记录」有登记）。
/// 问题：赌博牌的正面/反面数值是独立 IntVar，而游戏的附魔（锋利 +X 伤害 / 灵巧 +X 格挡）
/// 只改标准 DamageVar/BlockVar → 独立变量在卡面上永远显示裸值，看起来"吃不到附魔"。
/// 做法照抄 BaseLib `Cards/Variables/CustomExtraDamageVar`：预览刷新时如实算出附魔的
/// 加法 + 乘法修正写入 EnchantedValue（"附魔属于卡牌自身"的那条显示通道）。
/// 结算侧不改：API 文档明示 BaseValue 才是"改游戏状态"该用的值，附魔由伤害/格挡管线的
/// EnchantDamage*/EnchantBlock* 钩子落地 → 两侧各算一次，不会重复计数。
/// </summary>
public static class EnchantAware
{
    public static decimal Apply(CardModel? card, decimal amount, EnchantKind kind)
    {
        var enchantment = card?.Enchantment;
        if (enchantment == null || kind == EnchantKind.None) return amount;

        return kind switch
        {
            EnchantKind.Damage => (amount + enchantment.EnchantDamageAdditive(amount, ValueProp.Move))
                                  * enchantment.EnchantDamageMultiplicative(amount, ValueProp.Move),
            EnchantKind.Block => (amount + enchantment.EnchantBlockAdditive(amount))
                                 * enchantment.EnchantBlockMultiplicative(amount),
            _ => amount,
        };
    }

    /// <summary>该卡是否带附魔（自检日志用）。</summary>
    public static bool HasEnchantment(CardModel? card) => card?.Enchantment != null;
}

/// <summary>
/// 会把附魔加成算进卡面显示的 IntVar。用于赌博牌的"非标准"数值变量
/// （HeadsDamage / TailsDamage / HeadsBlock / TailsBlock 等）。
/// </summary>
public sealed class EnchantAwareVar(string name, decimal value, EnchantKind kind) : IntVar(name, value)
{
    public EnchantKind Kind => kind;

    public override void UpdateCardPreview(
        CardModel card,
        CardPreviewMode previewMode,
        Creature? target,
        bool runGlobalHooks)
    {
        var enchanted = EnchantAware.Apply(card, BaseValue, kind);
        EnchantedValue = enchanted;
        PreviewValue = enchanted;
    }
}

/// <summary>
/// 卡面显示"当前倍率下实际到账值"的 IntVar（兑现/筹码用）：
/// 风头正盛是"卡内自算 + 卡面裸值"的已知偏离，这里让卡面跟着倍率跳动。
/// </summary>
public sealed class MultiplierAwareVar(string name, decimal value) : IntVar(name, value)
{
    public override void UpdateCardPreview(
        CardModel card,
        CardPreviewMode previewMode,
        Creature? target,
        bool runGlobalHooks)
    {
        var current = (int)(BaseValue * MultiplierService.Factor);
        EnchantedValue = current;
        PreviewValue = current;
    }
}
