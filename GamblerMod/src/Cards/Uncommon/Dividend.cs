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

// 罕见 #21·派彩（游戏设计 §5.2）：2 能 + 3 金，获得 18 金币并抽 1 张牌（升级 22 金）。
public sealed class Dividend : GamblerCard
{
    public Dividend() : base(2, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
        BaseGoldCost = 3;
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [new IntVar("GoldGain", 18)];

    protected override void OnUpgrade() => DynamicVars["GoldGain"].UpgradeValueBy(4);

    protected override async Task ApplyEffect(PlayerChoiceContext ctx, CardPlay play, bool heads)
    {
        await PlayerCmd.GainGold(DynamicVars["GoldGain"].BaseValue, Owner, false);
        await CardPileCmd.Draw(ctx, 1, Owner);
    }

    public override List<(string, string)>? Localization => new CardLoc(
        "派彩",
        "#花费 *3 金币*。\n获得 !GoldGain! 金币，抽 1 张牌。");
}
