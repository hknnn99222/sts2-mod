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

// 稀有 #31·孤注一掷（游戏设计 §5.3 真赌）：1 能 + 10 金。
// 赌博：正面 → 48 伤害；反面 → 28 伤害且自己受到 6 伤害（升级：正面 55 / 反面 32）。
public sealed class AllOrNothing : GamblerCard
{
    public AllOrNothing() : base(1, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
    {
        BaseGoldCost = 10;
        IsGambleCard = true;
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(30, ValueProp.Move),
        // 原生 DamageVar/BlockVar（带名字的重载）：卡面自动算附魔/力量/虚弱（2026-10-17 修正）
        new DamageVar("HeadsDamage", 48, ValueProp.Move),
        new DamageVar("TailsDamage", 28, ValueProp.Move),
        new IntVar("TailsSelfHurt", 6),
    ];

    protected override void OnUpgrade()
    {
        DynamicVars["HeadsDamage"].UpgradeValueBy(7);
        DynamicVars["TailsDamage"].UpgradeValueBy(4);
    }

    protected override async Task ApplyEffect(PlayerChoiceContext ctx, CardPlay play, bool heads)
    {
        var damage = heads ? DynamicVars["HeadsDamage"].BaseValue : DynamicVars["TailsDamage"].BaseValue;

        await DamageCmd.Attack(damage)
            .FromCard(this, play)
            .Targeting(play.Target)
            .WithHitFx(heads ? HeadsHitFx : TailsHitFx, null, heads ? HeadsHitSfx : null)
            .Execute(ctx);

        if (!heads)
            await CreatureCmd.Damage(ctx, Owner.Creature, DynamicVars["TailsSelfHurt"].BaseValue, ValueProp.Move, this, play);
    }

    public override List<(string, string)>? Localization => new CardLoc(
        "孤注一掷",
        "#花费 *10 金币*。\n硬币为正时：造成 !HeadsDamage! 伤害；硬币为负时：造成 !TailsDamage! 伤害且自己受到 !TailsSelfHurt! 伤害。");
}
