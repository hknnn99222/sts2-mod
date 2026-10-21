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

// 普通 #9·摸牌（游戏设计 §5.1，v1.10 改版）：0 能 + 4 金，抽 2 张牌（升级抽 3）。
public sealed class BuyTheRoad : GamblerCard
{
    public BuyTheRoad() : base(0, CardType.Skill, CardRarity.Common, TargetType.None)
    {
        BaseGoldCost = 4;
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [new IntVar("Cards", 2)];

    protected override void OnUpgrade() => DynamicVars["Cards"].UpgradeValueBy(1);

    protected override Task ApplyEffect(PlayerChoiceContext ctx, CardPlay play, bool heads)
        => CardPileCmd.Draw(ctx, DynamicVars["Cards"].BaseValue, Owner);

    public override List<(string, string)>? Localization => new CardLoc(
        "摸牌",
        "#花费 *4 金币*。\n抽 !Cards! 张牌。");
}
