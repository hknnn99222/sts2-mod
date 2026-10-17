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

namespace GamblerMod.Cards.Rare;

// 稀有 #32·俄罗斯轮盘（游戏设计 §5.3 真赌）：2 能，对随机敌人 3×5 次；
// 赌博：正面 → 每次改为 5×5；反面 → 其中 2 发改为打自己（自己共受 10 伤害）（升级：每次 4 / 正面 6）。
public sealed class RussianRoulette : GamblerCard
{
    public RussianRoulette() : base(2, CardType.Attack, CardRarity.Rare, TargetType.RandomEnemy)
    {
        IsGambleCard = true;
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(3, ValueProp.Move),
        // 原生 DamageVar/BlockVar（带名字的重载）：卡面自动算附魔/力量/虚弱（2026-10-17 修正）
        new DamageVar("HeadsDamage", 5, ValueProp.Move),
        new IntVar("Hits", 5),
        new IntVar("TailsSelfHurt", 10),
    ];

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(1);
        DynamicVars["HeadsDamage"].UpgradeValueBy(1);
    }

    protected override async Task ApplyEffect(PlayerChoiceContext ctx, CardPlay play, bool heads)
    {
        if (heads)
        {
            await DamageCmd.Attack(DynamicVars["HeadsDamage"].BaseValue)
                .FromCard(this, play)
                .TargetingRandomOpponents(CombatState)
                .WithHitCount((int)DynamicVars["Hits"].BaseValue)
                .WithHitFx(HeadsHitFx, null, HeadsHitSfx)
                .Execute(ctx);
            return;
        }

        // 反面：3 发打敌人 + 2 发打自己（自伤共 10，按伤害分摊）
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, play)
            .TargetingRandomOpponents(CombatState)
            .WithHitCount(3)
            .WithHitFx(TailsHitFx)
            .Execute(ctx);

        await CreatureCmd.Damage(ctx, Owner.Creature, DynamicVars["TailsSelfHurt"].BaseValue, ValueProp.Move, this, play);
    }

    public override List<(string, string)>? Localization => new CardLoc(
        "俄罗斯轮盘",
        "#硬币为正时：对随机敌人造成 !HeadsDamage! 伤害 !Hits! 次；硬币为负时：对随机敌人造成 !D! 伤害 3 次，且自己受到 !TailsSelfHurt! 伤害。");
}
