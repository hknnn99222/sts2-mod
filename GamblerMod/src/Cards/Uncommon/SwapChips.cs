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

// 罕见 #71·换牌（游戏设计 §5.2 / §5.5）：1 能，**消耗** 1 张手牌，抽 2 张牌（升级抽 3）。
// 钩子：特异·消耗换抽（与兑筹的"牌换金"区别：这是换资源而非换钱）。
public sealed class SwapChips : GamblerCard
{
    public SwapChips() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.None) { }

    protected override IEnumerable<DynamicVar> CanonicalVars => [new IntVar("Cards", 2)];

    protected override void OnUpgrade() => DynamicVars["Cards"].UpgradeValueBy(1);

    protected override async Task ApplyEffect(PlayerChoiceContext ctx, CardPlay play, bool heads)
    {
        var hand = PileType.Hand.GetPile(Owner).Cards;
        if (hand.Count > 0)
        {
            var prefs = new CardSelectorPrefs(CardSelectorPrefs.ExhaustSelectionPrompt, 1, 1);
            var picked = (await CardSelectCmd.FromHand(ctx, Owner, prefs, c => c != this, this)).ToList();
            if (picked.Count > 0) await CardCmd.Exhaust(ctx, picked[0]);
        }

        await CardPileCmd.Draw(ctx, (int)DynamicVars["Cards"].BaseValue, Owner);
    }

    public override List<(string, string)>? Localization => new CardLoc(
        "换牌",
        "#消耗 1 张手牌，抽 !Cards! 张牌。");
}
