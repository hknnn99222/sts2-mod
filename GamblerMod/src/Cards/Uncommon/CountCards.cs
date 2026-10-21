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

// 罕见 #22·数牌（游戏设计 §5.2 / §5.5，v1.12 改版）：0 能，每张手牌获得 2 金币（升级 3）。
// 钩子由「信息（看顶牌）」改为「即时产金·手牌数」——原抽水的手牌折现定位移交本卡。
public sealed class CountCards() : GamblerCard(0, CardType.Skill, CardRarity.Uncommon, TargetType.None)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new IntVar("PerCard", 2)];

    protected override void OnUpgrade() => DynamicVars["PerCard"].UpgradeValueBy(1);

    protected override Task ApplyEffect(PlayerChoiceContext ctx, CardPlay play, bool heads)
    {
        var cards = PileType.Hand.GetPile(Owner).Cards.Count;
        if (cards <= 0) return Task.CompletedTask;
        return PlayerCmd.GainGold(cards * (int)DynamicVars["PerCard"].BaseValue, Owner, false);
    }

    public override List<(string, string)>? Localization => new CardLoc(
        "数牌",
        "#每张手牌获得 !PerCard! 金币。");
}
