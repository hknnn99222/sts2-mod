using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using GamblerMod.Core;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace GamblerMod.Cards.Uncommon;

// 罕见 #28·手气正旺（游戏设计 §5.2）：2 能能力，赌博正面率 +15%（升级 +25；上限 75%）。
public sealed class HotStreakCard() : GamblerCard(2, CardType.Power, CardRarity.Uncommon, TargetType.None)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new IntVar("Rate", 15)];

    protected override void OnUpgrade() => DynamicVars["Rate"].UpgradeValueBy(10);

    protected override Task ApplyEffect(PlayerChoiceContext ctx, CardPlay play, bool heads)
        => PowerCmd.Apply<Powers.HotStreakPower>(ctx, Owner.Creature, DynamicVars["Rate"].BaseValue, Owner.Creature, this);

    public override List<(string, string)>? Localization => new CardLoc(
        "手气正旺",
        "#*赌博*正面率 +!Rate!%（加法，上限 75%）。");
}
