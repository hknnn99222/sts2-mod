using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using GamblerMod.Core;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace GamblerMod.Cards.Rare;

// 稀有 #79·提现（游戏设计 §5.3 / §5.5 / §2.8）：1 能，按倍率**折算能量**：获得「倍率取整」点能量（升级 +1 点）。
// 钩子：风头正盛·兑现能量（倍率当货币，不走乘法通道——能量不在可乘数值集内）。
public sealed class CashChips : GamblerCard
{
    public CashChips() : base(1, CardType.Skill, CardRarity.Rare, TargetType.None)
    {
        IsHeadsStreakCard = true;
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [new IntVar("Bonus", 0)];

    protected override void OnUpgrade() => DynamicVars["Bonus"].UpgradeValueBy(1);

    protected override Task ApplyEffect(PlayerChoiceContext ctx, CardPlay play, bool heads)
    {
        var energy = (int)MultiplierService.Factor + (int)DynamicVars["Bonus"].BaseValue;
        if (energy <= 0) return Task.CompletedTask;
        return PlayerCmd.GainEnergy(energy, Owner);
    }

    public override List<(string, string)>? Localization => new CardLoc(
        "提现",
        "#按倍率折算：获得「倍率取整」点能量，结算后清空倍率。");
}
