using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using GamblerMod.Core;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace GamblerMod.Cards.Uncommon;

// 罕见 #65·加码（游戏设计 §5.2 / §5.5）：1 能，获得 1 点能量；本回合每花费过 10 金币额外 +1（至多 3；升级每 8 金）。
// 钩子：花费追踪·转能量。
public sealed class RakeFee : GamblerCard
{
    public RakeFee() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.None) { }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new IntVar("Threshold", 10),
        new IntVar("Cap", 3),
    ];

    protected override void OnUpgrade() => DynamicVars["Threshold"].UpgradeValueBy(-2);

    protected override Task ApplyEffect(PlayerChoiceContext ctx, CardPlay play, bool heads)
    {
        var extra = SpendTracker.SpentThisTurn / (int)DynamicVars["Threshold"].BaseValue;
        var cap = (int)DynamicVars["Cap"].BaseValue;
        // 「至多 !Cap!」修饰的是额外 +1 → 总额 = 1 + Cap（2026-10-16 审查修正 off-by-one）
        var energy = 1 + (extra > cap ? cap : extra);
        return PlayerCmd.GainEnergy(energy, Owner);
    }

    public override List<(string, string)>? Localization => new CardLoc(
        "加码",
        "#获得 1 点能量。本回合每花费过 *!Threshold! 金币*，额外 +1（至多 !Cap! 点）。");
}
