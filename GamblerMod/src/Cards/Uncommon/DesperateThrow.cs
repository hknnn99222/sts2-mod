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

// 罕见 #61·押宝（游戏设计 §5.2 / §5.5，v1.12 上调）：1 能 + 5 金，造成 18 伤害；赌博：正面 → 30；
// 反面 → 自己失去 3 点生命（升级 24 / 36，反面失 4）。
// 钩子：真赌·押 HP。
public sealed class DesperateThrow : GamblerCard
{
    public DesperateThrow() : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
        BaseGoldCost = 5;
        IsGambleCard = true;
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(18, ValueProp.Move),
        // 原生 DamageVar/BlockVar（带名字的重载）：卡面自动算附魔/力量/虚弱（2026-10-17 修正）
        new DamageVar("HeadsDamage", 30, ValueProp.Move),
        new IntVar("SelfHurt", 3),
    ];

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(6);
        DynamicVars["HeadsDamage"].UpgradeValueBy(6);
        DynamicVars["SelfHurt"].UpgradeValueBy(1);
    }

    protected override async Task ApplyEffect(PlayerChoiceContext ctx, CardPlay play, bool heads)
    {
        await DamageCmd.Attack(heads ? (int)DynamicVars["HeadsDamage"].BaseValue : DynamicVars.Damage.BaseValue)
            .FromCard(this, play)
            .Targeting(play.Target)
            .WithHitFx(heads ? HeadsHitFx : TailsHitFx, null, heads ? HeadsHitSfx : null)
            .Execute(ctx);

        if (!heads)
            await CreatureCmd.Damage(ctx, Owner.Creature, DynamicVars["SelfHurt"].BaseValue, ValueProp.Move, this, play);
    }

    public override List<(string, string)>? Localization => new CardLoc(
        "押宝",
        "#花费 *5 金币*。\n硬币为正时：造成 !HeadsDamage! 伤害；硬币为负时：造成 !D! 伤害且自己失去 !SelfHurt! 点生命。");
}
