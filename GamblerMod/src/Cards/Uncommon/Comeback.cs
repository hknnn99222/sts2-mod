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

// 罕见 #17·翻本（游戏设计 §5.2 / §5.4）：1 能 + 3 金，10 伤害；赌博：正面 → 20（升级 13 / 25）。
public sealed class Comeback : GamblerCard
{
    public Comeback() : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
        BaseGoldCost = 3;
        IsGambleCard = true;
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(10, ValueProp.Move),
        // 原生 DamageVar/BlockVar（带名字的重载）：卡面自动算附魔/力量/虚弱（2026-10-17 修正）
        new DamageVar("HeadsDamage", 20, ValueProp.Move),
    ];

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3);
        DynamicVars["HeadsDamage"].UpgradeValueBy(5);
    }

    protected override Task ApplyEffect(PlayerChoiceContext ctx, CardPlay play, bool heads)
        => DamageCmd.Attack(heads ? DynamicVars["HeadsDamage"].BaseValue : DynamicVars.Damage.BaseValue)
            .FromCard(this, play)
            .Targeting(play.Target)
            .WithHitFx(heads ? HeadsHitFx : TailsHitFx, null, heads ? HeadsHitSfx : null)
            .Execute(ctx);

    public override List<(string, string)>? Localization => new CardLoc(
        "翻本",
        "#花费 *3 金币*。\n硬币为正时：造成 !HeadsDamage! 伤害；硬币为负时：造成 !D! 伤害。");
}
