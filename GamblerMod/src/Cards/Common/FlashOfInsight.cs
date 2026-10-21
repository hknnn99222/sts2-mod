using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using GamblerMod.Core;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace GamblerMod.Cards.Common;

// 普通 #8·盲注（游戏设计 §5.1，v1.10 改版）：1 能，5 格挡；赌博：正面 → 9（升级 7 / 13）。
public sealed class FlashOfInsight : GamblerCard
{
    public FlashOfInsight() : base(1, CardType.Skill, CardRarity.Common, TargetType.None)
    {
        IsGambleCard = true;
    }
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(5, ValueProp.Move),
        new BlockVar("HeadsBlock", 9, ValueProp.Move),
    ];

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(2);
        DynamicVars["HeadsBlock"].UpgradeValueBy(4);
    }

    protected override Task ApplyEffect(PlayerChoiceContext ctx, CardPlay play, bool heads)
        => CreatureCmd.GainBlock(
            Owner.Creature,
            heads ? DynamicVars["HeadsBlock"].BaseValue : DynamicVars.Block.BaseValue,
            ValueProp.Move, play, false);

    public override List<(string, string)>? Localization => new CardLoc(
        "盲注",
        "#硬币为正时：获得 !HeadsBlock! 格挡；硬币为负时：获得 !B! 格挡。");
}
