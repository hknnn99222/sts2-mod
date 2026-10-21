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

// 稀有 #50·追数（游戏设计 §5.3 / §2.8，v1.15 改价）：2 能，造成 20 伤害 ×风头倍率（升级 24）；
// **击杀则本次不清空倍率**——「兑现后清空」规则的唯一例外（设计 §2.8）。
public sealed class ComebackRound : GamblerCard
{
    public ComebackRound() : base(2, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
    {
        IsHeadsStreakCard = true;
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(20, ValueProp.Move)];

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(4);

    protected override async Task ApplyEffect(PlayerChoiceContext ctx, CardPlay play, bool heads)
    {
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, play)
            .Targeting(play.Target)
            .WithHitFx("vfx/vfx_heavy_blunt", null, HeadsHitSfx)
            .Execute(ctx);

        if (play.Target.IsDead) KeepStreakAfterPlay = true; // 击杀续风（基类据此跳过清空）
    }

    public override List<(string, string)>? Localization => new CardLoc(
        "追数",
        "#造成 !D! 伤害。数值随头顶倍率放大；若*击杀*则倍率不清空。");
}
