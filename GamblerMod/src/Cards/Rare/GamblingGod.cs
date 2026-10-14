using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using GamblerMod.Core;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace GamblerMod.Cards.Rare;

// 稀有 #42·赌神（游戏设计 §5.3）：3 能能力（升级 2 能），所有赌博视为正面。
public sealed class GamblingGodCard : GamblerCard
{
    public GamblingGodCard() : base(3, CardType.Power, CardRarity.Rare, TargetType.None)
    {
    }

    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);

    protected override Task ApplyEffect(PlayerChoiceContext ctx, CardPlay play, bool heads)
        => PowerCmd.Apply<Powers.GamblingGodPower>(ctx, Owner.Creature, 1, Owner.Creature, this);

    public override List<(string, string)>? Localization => new CardLoc(
        "赌神",
        "#你的所有*赌博*视为正面。");
}
