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

// 普通 #2·加注（游戏设计 §5.1）：1 能攻击，基础 6 伤（升级 8），可变投入。
// M3 简化：自动花费至多 6 金币、每 1 金 +2 伤害（"想花多少花多少"的输入 UI 留待美术资产批，
// 已在技术设计 §4.1 可变投入条目记档）。
public sealed class AddMore() : GamblerCard(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(6, ValueProp.Move),
        new IntVar("PerGold", 2),  // 每 1 金币 +2 伤害
        new IntVar("MaxSpend", 6), // 自动投入封顶
    ];

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(2);

    protected override async Task ApplyEffect(PlayerChoiceContext ctx, CardPlay play, bool heads)
    {
        var maxSpend = (int)DynamicVars["MaxSpend"].BaseValue;
        var perGold = (int)DynamicVars["PerGold"].BaseValue;
        var spend = Math.Min(maxSpend, Owner.Gold);
        if (spend > 0) await GoldCost.Pay(Owner, spend);

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue + spend * perGold)
            .FromCard(this, play)
            .Targeting(play.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(ctx);
    }

    public override List<(string, string)>? Localization => new CardLoc(
        "加注",
        "#花费至多 *!MaxSpend! 金币*，每 1 金币 +!PerGold! 伤害。\n造成 !D! 伤害。");
}
