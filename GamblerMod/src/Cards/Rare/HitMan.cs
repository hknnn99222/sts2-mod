using System.Collections.Generic;
using System.Linq;
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

// 稀有 #38·打庄（游戏设计 §5.3，v1.15 改版）：1 能，对生命值最低的敌人造成 15 伤害；
// 若你的金币 <100，伤害翻倍（升级 19 / 翻倍 38）。钩子：定向斩杀（最低生命）+ 穷鬼加成。
public sealed class HitMan : GamblerCard
{
    private const int PoorThreshold = 100;

    public HitMan() : base(1, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy) { }

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(15, ValueProp.Move)];

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(4);

    protected override Task ApplyEffect(PlayerChoiceContext ctx, CardPlay play, bool heads)
    {
        var target = CombatState.Enemies.Where(e => e.IsAlive).OrderBy(e => e.CurrentHp).FirstOrDefault();
        if (target == null) return Task.CompletedTask;

        var damage = DynamicVars.Damage.BaseValue;
        var poor = Owner.Gold < PoorThreshold;
        if (poor) damage *= 2;

        return DamageCmd.Attack(damage)
            .FromCard(this, play)
            .Targeting(target)
            .WithHitFx(poor ? HeadsHitFx : "vfx/vfx_bloody_impact", null, HeadsHitSfx)
            .Execute(ctx);
    }

    public override List<(string, string)>? Localization => new CardLoc(
        "打庄",
        "#对生命值最低的敌人造成 !D! 伤害。若你的金币少于 *100*，伤害翻倍。");
}
