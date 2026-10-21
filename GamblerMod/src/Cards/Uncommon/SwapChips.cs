using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using GamblerMod.Core;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace GamblerMod.Cards.Uncommon;

// 罕见 #71·换牌（游戏设计 §5.2 / §5.5，v1.14 改版）：1 能（升级 0 能），
// 将任意张手牌放回抽牌堆，再抽等量的牌。钩子由「消耗换抽」改为「特异·重抽」。
// 注：选卡界面沿用引擎自带的"弃置"提示文案（无"放回抽牌堆"预设提示），实际动作是放回抽牌堆。
public sealed class SwapChips : GamblerCard
{
    public SwapChips() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.None) { }

    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);

    protected override async Task ApplyEffect(PlayerChoiceContext ctx, CardPlay play, bool heads)
    {
        var hand = PileType.Hand.GetPile(Owner).Cards.Count;
        if (hand <= 0) return;

        var prefs = new CardSelectorPrefs(CardSelectorPrefs.DiscardSelectionPrompt, 1, hand);
        var picked = (await CardSelectCmd.FromHand(ctx, Owner, prefs, c => c != this, this)).ToList();
        if (picked.Count == 0) return;

        foreach (var card in picked)
            await CardPileCmd.Add(card, PileType.Draw, CardPilePosition.Random, this, false);

        await CardPileCmd.Draw(ctx, picked.Count, Owner);
    }

    public override List<(string, string)>? Localization => new CardLoc(
        "换牌",
        "#将任意张手牌放回抽牌堆，再抽等量的牌。");
}
