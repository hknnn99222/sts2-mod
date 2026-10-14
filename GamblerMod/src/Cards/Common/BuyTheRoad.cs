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

// 普通 #9·买路钱（游戏设计 §5.1）：1 能 + 5 金，抽 2 张牌（升级 3）。
public sealed class BuyTheRoad : GamblerCard
{
    public BuyTheRoad() : base(1, CardType.Skill, CardRarity.Common, TargetType.None)
    {
        BaseGoldCost = 5;
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [new IntVar("Cards", 2)];

    protected override void OnUpgrade() => DynamicVars["Cards"].UpgradeValueBy(1);

    protected override Task ApplyEffect(PlayerChoiceContext ctx, CardPlay play, bool heads)
        => CardPileCmd.Draw(ctx, DynamicVars["Cards"].BaseValue, Owner);

    public override List<(string, string)>? Localization => new CardLoc(
        "买路钱",
        "#*额外花费 5 金币*。抽 !Cards! 张牌。");
}
