using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using GamblerMod.Core;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace GamblerMod.Cards.Common;

// 普通 #52·散码（游戏设计 §5.1 / §5.5）：1 能，每 2 张弃牌堆的牌获得 1 金币（至多 8；升级至多 12）。
// 钩子：即时产金·弃牌堆（按弃牌堆张数产金）。
public sealed class LooseChange : GamblerCard
{
    public LooseChange() : base(1, CardType.Skill, CardRarity.Common, TargetType.None) { }

    protected override IEnumerable<DynamicVar> CanonicalVars => [new IntVar("Cap", 8)];

    protected override void OnUpgrade() => DynamicVars["Cap"].UpgradeValueBy(4);

    protected override Task ApplyEffect(PlayerChoiceContext ctx, CardPlay play, bool heads)
    {
        var discarded = PileType.Discard.GetPile(Owner).Cards.Count;
        var gold = discarded / 2;
        if (gold > (int)DynamicVars["Cap"].BaseValue) gold = (int)DynamicVars["Cap"].BaseValue;
        if (gold <= 0) return Task.CompletedTask;
        return PlayerCmd.GainGold(gold, Owner, true);
    }

    public override List<(string, string)>? Localization => new CardLoc(
        "散码",
        "#每 2 张*弃牌堆*的牌，获得 1 金币（至多 !Cap! 金币）。");
}
