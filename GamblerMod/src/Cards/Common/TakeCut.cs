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
        var attack = DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, play)
            .Targeting(play.Target)
            .WithHitFx("vfx/vfx_attack_slash");

        await attack.Execute(ctx);

        // 溢出转金：读引擎结算的 OverkillDamage——只有击杀才 >0，且已自动扣除格挡、
        // 包含力量/易伤等修正（2026-10-16 审查修正：旧实现"裸伤害 − 战前 HP"不验击杀、不减格挡）。
        if (play.Target == null || !play.Target.IsDead) return;

        decimal overkill = 0m;
        foreach (var hit in attack.Results)
            foreach (var result in hit)
                if (result.Receiver == play.Target)
                    overkill += result.OverkillDamage;

        if (overkill > 0)
            await PlayerCmd.GainGold((int)overkill, Owner, false);
    }

    public override List<(string, string)>? Localization => new CardLoc(
        "吃水",
        "#花费 *4 金币*。\n造成 !D! 伤害；**溢出的伤害**转为金币。");
}
