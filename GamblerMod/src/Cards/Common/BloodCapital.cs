using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using GamblerMod.Core;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace GamblerMod.Cards.Common;

// 普通 #83·血本（v1.8 四轴改造·赌命轴）：0 能，失去 5 点生命换取 2 点能量（升级：失去 4）。
// 赌命轴的"燃料"：0 费换能量，把血量当成可透支的费用。配合押注（按已失去生命加成伤害）越用越强。
public sealed class BloodCapital() : GamblerCard(0, CardType.Skill, CardRarity.Common, TargetType.None)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new IntVar("HpLoss", 5), new IntVar("EnergyGain", 2)];

    protected override void OnUpgrade() => DynamicVars["HpLoss"].UpgradeValueBy(-1);

    protected override async Task ApplyEffect(PlayerChoiceContext ctx, CardPlay play, bool heads)
    {
        await CreatureCmd.Damage(ctx, Owner.Creature, DynamicVars["HpLoss"].BaseValue, ValueProp.Move, this, play);
        await PlayerCmd.GainEnergy(DynamicVars["EnergyGain"].BaseValue, Owner);
    }

    public override List<(string, string)>? Localization => new CardLoc(
        "血本",
        "#失去 !HpLoss! 点生命。获得 !EnergyGain! 点能量。");
}
