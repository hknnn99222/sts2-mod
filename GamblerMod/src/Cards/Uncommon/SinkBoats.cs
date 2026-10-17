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

// 罕见 #67·翻盘（游戏设计 §5.2 / §5.5）：1 能 + 3 金，造成 10 伤害；若你生命低于 50%，改为 24（升级 14 / 30）。
// 钩子：特异·生命条件。
public sealed class SinkBoats : GamblerCard
{
    public SinkBoats() : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
        BaseGoldCost = 3;
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(10, ValueProp.Move),
        new DamageVar("LowDamage", 24, ValueProp.Move),
    ];

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(4);
        DynamicVars["LowDamage"].UpgradeValueBy(6);
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
        "#花费 *3 金币*。\n造成 !D! 伤害。若你的生命低于 50%，改为 !LowDamage! 伤害。");
}
