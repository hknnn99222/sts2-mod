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

// 罕见 #66·清码（游戏设计 §5.2 / §5.5，v1.14 改版）：1 能，**消耗**全部手牌，每张获得 7 金币（升级 9）。
// 钩子：手牌换金（v1.14 由弃置改消耗——配合保底/加码的消耗堆协同）。
public sealed class CollectDebts : GamblerCard
{
    public CollectDebts() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.None) { }

    protected override IEnumerable<DynamicVar> CanonicalVars => [new IntVar("PerCard", 7)];

    protected override void OnUpgrade() => DynamicVars["PerCard"].UpgradeValueBy(2);

    protected override async Task ApplyEffect(PlayerChoiceContext ctx, CardPlay play, bool heads)
    {
        // 先取快照：消耗过程会改动手牌集合
        var hand = new List<CardModel>(PileType.Hand.GetPile(Owner).Cards);
        if (hand.Count == 0) return;

        foreach (var card in hand)
            await CardCmd.Exhaust(ctx, card);

        await PlayerCmd.GainGold(hand.Count * (int)DynamicVars["PerCard"].BaseValue, Owner, false);
    }

    public override List<(string, string)>? Localization => new CardLoc(
        "清码",
        "#消耗全部手牌，每张获得 !PerCard! 金币。");
}
