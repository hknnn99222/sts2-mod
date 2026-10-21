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

// 普通 #54·抽税（游戏设计 §5.1 / §5.5，v1.10 改版）：1 能 + 3 金，造成 16 伤害；**被格挡抵消的伤害**转为金币（升级 20）。
// 钩子：即时产金·破盾转金（v1.10 由「击杀产金·溢出转金」改为「破盾转金」——
// 收益不再要求击杀，而是奖励打在有格挡的敌人身上：盾越厚，抽水越多）。
public sealed class TakeCut : GamblerCard
{
    public TakeCut() : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
    {
        BaseGoldCost = 3;
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(16, ValueProp.Move)];

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(4);

    protected override async Task ApplyEffect(PlayerChoiceContext ctx, CardPlay play, bool heads)
    {
        var attack = DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, play)
            .Targeting(play.Target)
            .WithHitFx("vfx/vfx_attack_slash");

        await attack.Execute(ctx);

        // 破盾转金：读引擎结算的 BlockedDamage——即这一击实际打掉的格挡量，
        // 已包含力量/虚弱/易伤等修正，且不会把溢出的伤害算进来。
        decimal blocked = 0m;
        foreach (var hit in attack.Results)
            foreach (var result in hit)
                if (result.Receiver == play.Target)
                    blocked += result.BlockedDamage;

        if (blocked > 0)
            await PlayerCmd.GainGold((int)blocked, Owner, false);
    }

    public override List<(string, string)>? Localization => new CardLoc(
        "抽税",
        "#花费 *3 金币*。\n造成 !D! 伤害；**被格挡抵消的伤害**转为金币。");
}
