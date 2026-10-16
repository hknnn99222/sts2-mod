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

// 普通 #45·乘胜（游戏设计 §5.1 / §2.8）：1 能，造成 6 伤害 ×风头倍率（升级 9）。
public sealed class RideTheWin : GamblerCard
{
    public RideTheWin() : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
    {
        IsHeadsStreakCard = true;
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(6, ValueProp.Move)];

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(3);

    protected override Task ApplyEffect(PlayerChoiceContext ctx, CardPlay play, bool heads)
        => DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, play)
            .Targeting(play.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(ctx);

    public override List<(string, string)>? Localization => new CardLoc(
        "乘胜",
        "#造成 !D! 伤害。数值随头顶倍率放大，结算后清空。");
}
