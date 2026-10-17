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

namespace GamblerMod.Cards.Uncommon;

// 罕见 #16·叠注（游戏设计 §5.2）：1 能，8 伤害；若**风头倍率 ≥1.5**，改为 16 伤害（升级 10 / 20）。
// v1.6 改造：条件从"上一掷为正面"改为"倍率 ≥1.5"——只读条件，数值**不乘**倍率、打完**不清空**（Q20 分开处理）。
public sealed class Parlay() : GamblerCard(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(8, ValueProp.Move),
        new DamageVar("HotDamage", 16, ValueProp.Move),
    ];

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2);
        DynamicVars["HotDamage"].UpgradeValueBy(4);
    }

    protected override Task ApplyEffect(PlayerChoiceContext ctx, CardPlay play, bool heads)
    {
        var hot = MultiplierService.Factor >= 1.5m;
        var damage = hot ? DynamicVars["HotDamage"].BaseValue : DynamicVars.Damage.BaseValue;
        return DamageCmd.Attack(damage)
            .FromCard(this, play)
            .Targeting(play.Target)
            .WithHitFx(hot ? HeadsHitFx : TailsHitFx, null, hot ? HeadsHitSfx : null)
            .Execute(ctx);
    }

    public override List<(string, string)>? Localization => new CardLoc(
        "叠注",
        "#造成 !D! 伤害。若风头正盛倍率 ≥1.5，改为造成 !HotDamage! 伤害。");
}
