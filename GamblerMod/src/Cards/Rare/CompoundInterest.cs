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

// 稀有 #43·利滚利（游戏设计 §5.3，v1.15 上调）：2 能能力，本场战斗中你每次获得金币时，额外获得 3 金币（升级 5）。
public sealed class CompoundInterestCard() : GamblerCard(2, CardType.Power, CardRarity.Rare, TargetType.None)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new IntVar("Gold", 3)];

    protected override void OnUpgrade() => DynamicVars["Gold"].UpgradeValueBy(2);

    protected override Task ApplyEffect(PlayerChoiceContext ctx, CardPlay play, bool heads)
        => PowerCmd.Apply<Powers.CompoundInterestPower>(ctx, Owner.Creature, DynamicVars["Gold"].BaseValue, Owner.Creature, this);

    public override List<(string, string)>? Localization => new CardLoc(
        "利滚利",
        "#本场战斗中你每次获得金币时，额外获得 !Gold! 金币。");
}
