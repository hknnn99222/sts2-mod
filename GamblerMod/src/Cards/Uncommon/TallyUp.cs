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

// 罕见 #64·回收（游戏设计 §5.2 / §5.5，v1.14 改版）：1 能，每张**已消耗**的牌获得 2 金币（升级 3）。
// 钩子由「消耗堆协同·转格挡」改为「消耗堆协同·产金」——从防御牌变成消耗流的经济出口。
public sealed class TallyUp : GamblerCard
{
    public TallyUp() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.None) { }

    protected override IEnumerable<DynamicVar> CanonicalVars => [new IntVar("PerExhaust", 2)];

    protected override void OnUpgrade() => DynamicVars["PerExhaust"].UpgradeValueBy(1);

    protected override Task ApplyEffect(PlayerChoiceContext ctx, CardPlay play, bool heads)
    {
        var exhausted = PileType.Exhaust.GetPile(Owner).Cards.Count;
        if (exhausted <= 0) return Task.CompletedTask;
        return PlayerCmd.GainGold(exhausted * (int)DynamicVars["PerExhaust"].BaseValue, Owner, false);
    }

    public override List<(string, string)>? Localization => new CardLoc(
        "回收",
        "#每张*已消耗*的牌，获得 !PerExhaust! 金币。");
}
