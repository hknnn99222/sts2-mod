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

// 普通 #56·典当（游戏设计 §5.1 / §5.5，v1.10 改版）：1 能，消耗 1 张手牌，获得 6 金币（升级 8）。
// 钩子：牌换金·小额（普通档的小额版，负责早期把废牌折现）。
public sealed class ShuffleUp : GamblerCard
{
    public ShuffleUp() : base(1, CardType.Skill, CardRarity.Common, TargetType.None) { }

    protected override IEnumerable<DynamicVar> CanonicalVars => [new IntVar("GoldGain", 6)];

    protected override void OnUpgrade() => DynamicVars["GoldGain"].UpgradeValueBy(2);

    protected override async Task ApplyEffect(PlayerChoiceContext ctx, CardPlay play, bool heads)
    {
        if (PileType.Hand.GetPile(Owner).Cards.Count == 0) return;

        var prefs = new CardSelectorPrefs(CardSelectorPrefs.ExhaustSelectionPrompt, 1, 1);
        var selected = (await CardSelectCmd.FromHand(ctx, Owner, prefs, c => c != this, this)).ToList();
        if (selected.Count == 0) return;

        await CardCmd.Exhaust(ctx, selected[0]);
        await PlayerCmd.GainGold(DynamicVars["GoldGain"].BaseValue, Owner, false);
    }

    public override List<(string, string)>? Localization => new CardLoc(
        "典当",
        "#消耗 1 张手牌，获得 !GoldGain! 金币。");
}
