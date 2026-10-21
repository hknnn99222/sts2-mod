using System;
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

namespace GamblerMod.Cards.Rare;

// 稀有 #51·割肉（游戏设计 §5.3 / §2.8，v1.15 改版）：1 能，获得 12 格挡 ×风头倍率（升级 15），**不再自伤**。
public sealed class GoForBroke : GamblerCard
{
    public GoForBroke() : base(1, CardType.Skill, CardRarity.Rare, TargetType.None)
    {
        IsHeadsStreakCard = true;
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(12, ValueProp.Move)];

    protected override void OnUpgrade() => DynamicVars.Block.UpgradeValueBy(3);

    protected override Task ApplyEffect(PlayerChoiceContext ctx, CardPlay play, bool heads)
        => CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, play);

    public override List<(string, string)>? Localization => new CardLoc(
        "割肉",
        "#获得 !B! 格挡。数值随头顶倍率放大，结算后清空。");
}
