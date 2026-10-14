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

// 罕见 #23·当铺（游戏设计 §5.2）：1 能，消耗手牌 1 张，获得其价值：
// 基础 3 / 普通 5 / 罕见 8 / 稀有 12 金币（升级金额 +2）。
public sealed class Pawnshop() : GamblerCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.None)
{
    private static readonly Dictionary<CardRarity, int> BasePrices = new()
    {
        [CardRarity.Basic] = 3,
        [CardRarity.Common] = 5,
        [CardRarity.Uncommon] = 8,
        [CardRarity.Rare] = 12,
    };

    protected override IEnumerable<DynamicVar> CanonicalVars => [new IntVar("Bonus", 0)];

    protected override void OnUpgrade() => DynamicVars["Bonus"].UpgradeValueBy(2);

    protected override async Task ApplyEffect(PlayerChoiceContext ctx, CardPlay play, bool heads)
    {
        var hand = PileType.Hand.GetPile(Owner).Cards;
        if (hand.Count == 0) return;

        var selected = await CardSelectCmd.FromChooseACardScreen(ctx, hand, Owner, true);
        if (selected == null || selected == this) return;

        BasePrices.TryGetValue(selected.Rarity, out var price);
        await CardCmd.Exhaust(ctx, selected);
        await PlayerCmd.GainGold(price + (int)DynamicVars["Bonus"].BaseValue, Owner, true);
    }

    public override List<(string, string)>? Localization => new CardLoc(
        "当铺",
        "#消耗手牌 1 张，获得其价值：基础 3 / 普通 5 / 罕见 8 / 稀有 12 金币。");
}
