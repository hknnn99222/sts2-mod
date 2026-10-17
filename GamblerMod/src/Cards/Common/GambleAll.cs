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

// 普通 #3·买定离手（游戏设计 §5.1 / §5.4）：2 能 AoE 攻击，6 伤（升级 9）；赌博：正面 → 13（升级 17）。
public sealed class GambleAll : GamblerCard
{
    public GambleAll() : base(2, CardType.Attack, CardRarity.Common, TargetType.AllEnemies)
    {
        IsGambleCard = true;
    }
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(6, ValueProp.Move),
        // 原生 DamageVar/BlockVar（带名字的重载）：卡面自动算附魔/力量/虚弱（2026-10-17 修正）
        new DamageVar("HeadsDamage", 13, ValueProp.Move),
    ];

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3);
        DynamicVars["HeadsDamage"].UpgradeValueBy(4);
    }

    protected override Task ApplyEffect(PlayerChoiceContext ctx, CardPlay play, bool heads)
        => DamageCmd.Attack(heads ? DynamicVars["HeadsDamage"].BaseValue : DynamicVars.Damage.BaseValue)
            .FromCard(this, play)
            .TargetingAllOpponents(CombatState)
            .WithHitFx(heads ? HeadsHitFx : TailsHitFx, null, heads ? HeadsHitSfx : null)
            .Execute(ctx);

    public override List<(string, string)>? Localization => new CardLoc(
        "买定离手",
        "#硬币为正时：对所有敌人造成 !HeadsDamage! 伤害；硬币为负时：对所有敌人造成 !D! 伤害。");
}
