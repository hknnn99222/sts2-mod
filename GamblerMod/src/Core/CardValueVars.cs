using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace GamblerMod.Core;

// 卡面数值显示（2026-10-17 批次 4；2026-10-17 晚按实测 bug 修正为「优先用引擎原生类型」）
//
// ★ 血泪教训（务必先读）：**伤害/格挡数值不要用 IntVar 承载**。
//   卡面显示的那三个值（BaseValue / EnchantedValue / PreviewValue）只有**引擎认得的变量类型**
//   才会被塞进完整预览管线（附魔 / 力量 / 虚弱 / 易伤 / 敏捷）。用 IntVar 的话：
//   卡面永远是裸值 —— 玩家会看到"正面数值不吃增伤、不吃虚弱"，且与结算对不上。
//   引擎的 `DamageVar` / `BlockVar` 都带 **(名字, 数值, ValueProp)** 构造器（元数据核实，
//   本项目 2026-10-17 实测），所以"一张卡两套伤害/格挡数值"直接这么写即可：
//       new DamageVar("HeadsDamage", 10, ValueProp.Move)   // 正面
//       new BlockVar("TailsBlock", 16, ValueProp.Move)     // 反面
//   两张牌各自有名字、互不冲突，且自动获得与标准 `!D!`/`!B!` 完全一致的预览行为。
//
// 下面这个类只处理**引擎没有管线的东西**：金币（×倍率显示），因为没有附魔/力量通道，
// 只能自己算、自己写进预览值。

/// <summary>
/// 卡面显示「当前倍率下实际到账值」的 IntVar（兑现/产金牌用）。
/// 风头正盛是"卡内自算 + 卡面裸值"的已知偏离，这里让卡面跟着倍率跳动。
/// 注意：金币是引擎没有修饰管线的资源，所以只能用自定义变量；伤害/格挡请用原生 DamageVar/BlockVar。
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
