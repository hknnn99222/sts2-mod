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

namespace GamblerMod.Cards.Common;

// 普通 #54·吃水（游戏设计 §5.1 / §5.5）：1 能 + 4 金，造成 12 伤害；**溢出的伤害**转为金币（升级 16）。
// 钩子：击杀产金·溢出转金。
public sealed class TakeCut : GamblerCard
{
    public TakeCut() : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
    {
        BaseGoldCost = 4;
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(12, ValueProp.Move)];

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(4);

    protected override async Task ApplyEffect(PlayerChoiceContext ctx, CardPlay play, bool heads)
    {
        var hpBefore = play.Target?.CurrentHp ?? 0;
        var damage = DynamicVars.Damage.BaseValue;

        await DamageCmd.Attack(damage)
            .FromCard(this, play)
            .Targeting(play.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(ctx);

        var overflow = damage - hpBefore;
        if (overflow > 0)
            await PlayerCmd.GainGold(overflow, Owner, true);
    }

    public override List<(string, string)>? Localization => new CardLoc(
        "吃水",
        "#花费 *4 金币*。\n造成 !D! 伤害；**溢出的伤害**转为金币。");
}
