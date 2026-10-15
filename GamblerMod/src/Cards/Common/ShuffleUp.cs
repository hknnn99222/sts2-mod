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

namespace GamblerMod.Cards.Common;

// 普通 #56·洗牌（游戏设计 §5.1 / §5.5）：1 能，弃置 1 张手牌，抽 2 张牌（升级抽 3）。
// 钩子：弃牌换抽。
public sealed class ShuffleUp : GamblerCard
{
    public ShuffleUp() : base(1, CardType.Skill, CardRarity.Common, TargetType.None) { }

    protected override IEnumerable<DynamicVar> CanonicalVars => [new IntVar("Cards", 2)];

    protected override void OnUpgrade() => DynamicVars["Cards"].UpgradeValueBy(1);

    protected override async Task ApplyEffect(PlayerChoiceContext ctx, CardPlay play, bool heads)
    {
        var hand = PileType.Hand.GetPile(Owner).Cards;
        if (hand.Count > 0)
        {
            var prefs = new CardSelectorPrefs(CardSelectorPrefs.DiscardSelectionPrompt, 1, 1);
            var picked = (await CardSelectCmd.FromHand(ctx, Owner, prefs, c => c != this, this)).ToList();
            if (picked.Count > 0) await CardCmd.Discard(ctx, picked[0]);
        }

        await CardPileCmd.Draw(ctx, (int)DynamicVars["Cards"].BaseValue, Owner);
    }

    public override List<(string, string)>? Localization => new CardLoc(
        "洗牌",
        "#弃置 1 张手牌，抽 !Cards! 张牌。");
}
