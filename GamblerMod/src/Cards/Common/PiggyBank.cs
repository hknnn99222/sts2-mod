using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using GamblerMod.Core;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace GamblerMod.Cards.Common;

// 普通 #11·返水（游戏设计 §5.1）：1 能能力，每回合开始获得 3 金币（升级 4）。
public sealed class PiggyBankCard() : GamblerCard(1, CardType.Power, CardRarity.Common, TargetType.None)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new IntVar("Gold", 3)];

    protected override void OnUpgrade() => DynamicVars["Gold"].UpgradeValueBy(1);

    protected override Task ApplyEffect(PlayerChoiceContext ctx, CardPlay play, bool heads)
        => PowerCmd.Apply<Powers.PiggyBankPower>(ctx, Owner.Creature, DynamicVars["Gold"].BaseValue, Owner.Creature, this);

    public override List<(string, string)>? Localization => new CardLoc(
        "返水",
        "#每回合开始获得 !Gold! 金币。");
}
