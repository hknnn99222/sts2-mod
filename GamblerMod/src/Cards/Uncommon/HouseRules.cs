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

// 罕见 #29·保护费（游戏设计 §5.2）：2 能能力，每回合开始获得 4 金币；每次掷出正面，额外获得 3 金币
//（升级：每回合 5 金币）。
public sealed class HouseRulesCard() : GamblerCard(2, CardType.Power, CardRarity.Uncommon, TargetType.None)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new IntVar("Gold", 4)];

    protected override void OnUpgrade() => DynamicVars["Gold"].UpgradeValueBy(1);

    protected override Task ApplyEffect(PlayerChoiceContext ctx, CardPlay play, bool heads)
        => PowerCmd.Apply<Powers.HouseRulesPower>(ctx, Owner.Creature, DynamicVars["Gold"].BaseValue, Owner.Creature, this);

    public override List<(string, string)>? Localization => new CardLoc(
        "保护费",
        "#每回合开始获得 !Gold! 金币。每次掷出*正面*，额外获得 3 金币。");
}
