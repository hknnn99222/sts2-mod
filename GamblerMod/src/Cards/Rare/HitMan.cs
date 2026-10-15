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

// 稀有 #38·买凶（游戏设计 §5.3）：2 能 + 15 金，对生命值最低的敌人造成 43 伤害（升级 50）。
public sealed class HitMan : GamblerCard
{
    public HitMan() : base(2, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
    {
        BaseGoldCost = 15;
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(43, ValueProp.Move)];

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(7);

    protected override Task ApplyEffect(PlayerChoiceContext ctx, CardPlay play, bool heads)
    {
        var target = CombatState.Enemies.Where(e => e.IsAlive).OrderBy(e => e.CurrentHp).FirstOrDefault();
        if (target == null) return Task.CompletedTask;

        return DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, play)
            .Targeting(target)
            .WithHitFx("vfx/vfx_bloody_impact", null, "heavy_attack.mp3")
            .Execute(ctx);
    }

    public override List<(string, string)>? Localization => new CardLoc(
        "买凶",
        "#花费 *15 金币*。\n对生命值最低的敌人造成 !D! 伤害。");
}
