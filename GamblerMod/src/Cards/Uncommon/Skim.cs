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

// 罕见 #60·抽水（游戏设计 §5.2 / §5.5）：1 能，每张手牌获得 1 金币（至多 8；升级至多 12）。
// 钩子：即时产金·手牌数。
public sealed class Skim : GamblerCard
{
    public Skim() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.None) { }

    protected override IEnumerable<DynamicVar> CanonicalVars => [new IntVar("Cap", 8)];

    protected override void OnUpgrade() => DynamicVars["Cap"].UpgradeValueBy(4);

    protected override Task ApplyEffect(PlayerChoiceContext ctx, CardPlay play, bool heads)
    {
        var cards = PileType.Hand.GetPile(Owner).Cards.Count;
        if (cards > (int)DynamicVars["Cap"].BaseValue) cards = (int)DynamicVars["Cap"].BaseValue;
        if (cards <= 0) return Task.CompletedTask;
        return PlayerCmd.GainGold(cards, Owner, false);
    }

    public override List<(string, string)>? Localization => new CardLoc(
        "抽水",
        "#每张手牌获得 1 金币（至多 !Cap! 金币）。");
}
