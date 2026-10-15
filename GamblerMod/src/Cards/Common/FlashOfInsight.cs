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

// 普通 #8·盲注（游戏设计 §5.1 / §5.4）：1 能，4 格挡；赌博：正面 → 8（升级 6 / 13）。
public sealed class FlashOfInsight : GamblerCard
{
    public FlashOfInsight() : base(1, CardType.Skill, CardRarity.Common, TargetType.None)
    {
        IsGambleCard = true;
    }
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(4, ValueProp.Move),
        new IntVar("HeadsBlock", 8),
    ];

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(2);
        DynamicVars["HeadsBlock"].UpgradeValueBy(5);
    }

    protected override Task ApplyEffect(PlayerChoiceContext ctx, CardPlay play, bool heads)
        => CreatureCmd.GainBlock(
            Owner.Creature,
            heads ? DynamicVars["HeadsBlock"].BaseValue : DynamicVars.Block.BaseValue,
            ValueProp.Move, play, false);

    public override List<(string, string)>? Localization => new CardLoc(
        "盲注",
        "#获得 !B! 格挡。*赌博*：正面 → 改为获得 !HeadsBlock! 格挡。");
}
