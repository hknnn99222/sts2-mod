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

// 罕见 #67·翻盘（游戏设计 §5.2 / §5.5，v1.12 降费）：0 能，造成 6 伤害；若你生命低于 50%，改为 14（升级 8 / 18）。
// 钩子：特异·生命条件。
public sealed class SinkBoats : GamblerCard
{
    public SinkBoats() : base(0, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy) { }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(6, ValueProp.Move),
        new DamageVar("LowDamage", 14, ValueProp.Move),
    ];

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2);
        DynamicVars["LowDamage"].UpgradeValueBy(4);
    }

    protected override Task ApplyEffect(PlayerChoiceContext ctx, CardPlay play, bool heads)
    {
        var creature = Owner.Creature;
        var low = creature.CurrentHp * 2 < creature.MaxHp;
        var damage = low ? (int)DynamicVars["LowDamage"].BaseValue : DynamicVars.Damage.BaseValue;

        return DamageCmd.Attack(damage)
            .FromCard(this, play)
            .Targeting(play.Target)
            .WithHitFx(low ? HeadsHitFx : "vfx/vfx_attack_slash", null, low ? HeadsHitSfx : null)
            .Execute(ctx);
    }

    public override List<(string, string)>? Localization => new CardLoc(
        "翻盘",
        "#造成 !D! 伤害。若你的生命低于 50%，改为 !LowDamage! 伤害。");
}
