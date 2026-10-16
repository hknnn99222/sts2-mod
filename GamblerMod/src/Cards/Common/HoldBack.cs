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

// 普通 #44·收手（游戏设计 §5.1 / §2.8）：1 能，获得 5 格挡 ×风头倍率（升级 8）。
// 新机制「风头正盛」的教学卡：裸值 = 白板（纯奖励定价），格挡数由 Core 钩子乘倍率。
public sealed class HoldBack : GamblerCard
{
    public HoldBack() : base(1, CardType.Skill, CardRarity.Common, TargetType.None)
    {
        IsHeadsStreakCard = true;
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(5, ValueProp.Move)];

    protected override void OnUpgrade() => DynamicVars.Block.UpgradeValueBy(3);

    protected override Task ApplyEffect(PlayerChoiceContext ctx, CardPlay play, bool heads)
        => CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, play);

    public override List<(string, string)>? Localization => new CardLoc(
        "收手",
        "#获得 !B! 格挡。数值随头顶倍率放大，结算后清空。");
}
