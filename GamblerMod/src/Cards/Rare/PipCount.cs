using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using GamblerMod.Core;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace GamblerMod.Cards.Rare;

// 稀有 #74·摊牌（游戏设计 §5.3 / §5.5，v1.15 改版）：1 能，对随机敌人造成「弃牌堆张数」伤害（至多 30；升级至多 40）。
// 钩子：特异·弃牌堆规模（越弃越强的收尾牌）。
public sealed class PipCount : GamblerCard
{
    public PipCount() : base(1, CardType.Skill, CardRarity.Rare, TargetType.None) { }

    protected override IEnumerable<DynamicVar> CanonicalVars => [new IntVar("Cap", 30)];

    protected override void OnUpgrade() => DynamicVars["Cap"].UpgradeValueBy(10);

    protected override Task ApplyEffect(PlayerChoiceContext ctx, CardPlay play, bool heads)
    {
        var damage = PileType.Discard.GetPile(Owner).Cards.Count;
        if (damage > (int)DynamicVars["Cap"].BaseValue) damage = (int)DynamicVars["Cap"].BaseValue;
        if (damage <= 0) return Task.CompletedTask;

        return DamageCmd.Attack(damage)
            .FromCard(this, play)
            .TargetingRandomOpponents(CombatState)
            .WithHitCount(1)
            .WithHitFx("vfx/vfx_attack_blunt")
            .Execute(ctx);
    }

    public override List<(string, string)>? Localization => new CardLoc(
        "摊牌",
        "#对随机敌人造成等同于*弃牌堆张数*的伤害（至多 !Cap!）。");
}
