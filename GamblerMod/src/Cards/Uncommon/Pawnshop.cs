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

// 罕见 #23·兑筹（游戏设计 §5.2）：1 能，选择消耗手牌 1 张，获得 12 金币（升级 16）。
// 选卡必须走 FromHand（整手牌选择）：FromChooseACardScreen 官方只支持 ≤3 张的小列表，
// 塞整手牌会出现选卡界面无法点选的软锁。
public sealed class Pawnshop() : GamblerCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.None)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new IntVar("GoldGain", 12)];

    protected override void OnUpgrade() => DynamicVars["GoldGain"].UpgradeValueBy(4);

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
        "兑筹",
        "#消耗手牌 1 张，获得 !GoldGain! 金币。");
}
