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

// 罕见 #86·拼命（v1.8 四轴改造·赌命轴放大器）：1 能能力，你造成的伤害 +「已失去生命」的 10%（升级 15%）。
// 赌命轴的放大器：把"自伤"从成本变成收益来源，与放血/分红/押宝/止损形成正循环。
public sealed class BettingCard() : GamblerCard(1, CardType.Power, CardRarity.Uncommon, TargetType.None)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new IntVar("Percent", 10)];

    protected override void OnUpgrade() => DynamicVars["Percent"].UpgradeValueBy(5);

    protected override Task ApplyEffect(PlayerChoiceContext ctx, CardPlay play, bool heads)
        => PowerCmd.Apply<Powers.BettingPower>(ctx, Owner.Creature, DynamicVars["Percent"].BaseValue, Owner.Creature, this);

    public override List<(string, string)>? Localization => new CardLoc(
        "拼命",
        "#你造成的伤害获得「已失去生命」的 !Percent!% 加成。");
}
