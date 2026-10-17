using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using GamblerMod.Core;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Models;

namespace GamblerMod.Cards.Basic;

// 起始签名牌·试手气（游戏设计 §3 / §5.4 期望锚定）：
// 1 能攻击，造成 5 伤害；赌博：正面 → 改为 10（升级 7 / 14，期望 ≈ 1.2× 打击）。
public sealed class TryYourLuck : GamblerCard
{
    public TryYourLuck() : base(1, CardType.Attack, CardRarity.Basic, TargetType.AnyEnemy)
    {
        IsGambleCard = true;
    }
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(5, ValueProp.Move),
        // 原生 DamageVar/BlockVar（带名字的重载）：卡面自动算附魔/力量/虚弱（2026-10-17 修正）
        new DamageVar("HeadsDamage", 10, ValueProp.Move),
    ];

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2);
        DynamicVars["HeadsDamage"].UpgradeValueBy(4);
    }

    protected override Task ApplyEffect(PlayerChoiceContext ctx, CardPlay play, bool heads)
        => DamageCmd.Attack(heads ? DynamicVars["HeadsDamage"].BaseValue : DynamicVars.Damage.BaseValue)
            .FromCard(this, play)
            .Targeting(play.Target)
            .WithHitFx(heads ? HeadsHitFx : TailsHitFx, null, heads ? HeadsHitSfx : null)
            .Execute(ctx);

    public override List<(string, string)>? Localization => new CardLoc(
        "试手气",
        "#硬币为正时：造成 !HeadsDamage! 伤害；硬币为负时：造成 !D! 伤害。");
}
