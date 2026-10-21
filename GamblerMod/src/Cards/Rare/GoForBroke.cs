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

// 稀有 #51·割肉（游戏设计 §5.3 / §2.8）：1 能，获得 10 格挡 ×风头倍率（升级 13）；
// 自身失去「倍率 × 3」生命（向上取整）。裸值高于同费常规牌（10 > 5），因为自伤需要补偿。
public sealed class GoForBroke : GamblerCard
{
    public GoForBroke() : base(1, CardType.Skill, CardRarity.Rare, TargetType.None)
    {
        IsHeadsStreakCard = true;
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(10, ValueProp.Move)];

    protected override void OnUpgrade() => DynamicVars.Block.UpgradeValueBy(3);

    protected override async Task ApplyEffect(PlayerChoiceContext ctx, CardPlay play, bool heads)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, play);

        // 自伤：走**不带 cardSource** 的重载（dealer = 自己）。
        // 本卡是风头卡，若把 cardSource 传成 this，ModifyDamageMultiplicative 会对自伤再乘一次
        // 倍率（实际 ≈ 3×Factor²）——2026-10-16 审查修正为手动乘、交给无卡来源通道。
        var selfHurt = (int)Math.Ceiling(MultiplierService.Factor * 3m);
        await CreatureCmd.Damage(ctx, Owner.Creature, selfHurt, ValueProp.Move, Owner.Creature);
    }

    public override List<(string, string)>? Localization => new CardLoc(
        "割肉",
        "#获得 !B! 格挡。格挡随头顶倍率放大，自身失去「倍率 ×3」生命，结算后清空。");
}
