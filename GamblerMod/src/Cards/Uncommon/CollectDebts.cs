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

// 罕见 #66·清码（游戏设计 §5.2 / §5.5）：1 能，弃置全部手牌，每弃 1 张获得 4 金币（升级 5）。
// 钩子：手牌换金。
public sealed class CollectDebts : GamblerCard
{
    public CollectDebts() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.None) { }

    protected override IEnumerable<DynamicVar> CanonicalVars => [new IntVar("PerCard", 4)];

    protected override void OnUpgrade() => DynamicVars["PerCard"].UpgradeValueBy(1);

    protected override async Task ApplyEffect(PlayerChoiceContext ctx, CardPlay play, bool heads)
    {
        // 先取快照：弃置过程会改动手牌集合
        var hand = new List<CardModel>(PileType.Hand.GetPile(Owner).Cards);
        if (hand.Count == 0) return;

        foreach (var card in hand)
            await CardCmd.Discard(ctx, card);

        await PlayerCmd.GainGold(hand.Count * (int)DynamicVars["PerCard"].BaseValue, Owner, false);
    }

    public override List<(string, string)>? Localization => new CardLoc(
        "清码",
        "#弃置全部手牌，每弃 1 张获得 !PerCard! 金币。");
}
