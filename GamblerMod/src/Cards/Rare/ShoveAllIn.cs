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

namespace GamblerMod.Cards.Rare;

// 稀有 #72·全下（游戏设计 §5.3 / §5.5）：X 能 + **7×X 金**（金币系数 = 能量↔金币换算率），
// 对随机敌人造成 9×X 伤害共 X 次（升级金币费降为 5×X）。
// 钩子：附加费·X费多段。可打出检查按最低 X=1 判灰（同梭哈）。
public sealed class ShoveAllIn : GamblerCard
{
    public ShoveAllIn() : base(1, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy, costsX: true)
    {
        GoldPerX = 7;
    }

    public int GoldPerX { get; set; }

    private int ResolvedX => Math.Max(1, ResolveEnergyXValue());

    public override int EffectiveGoldCost => GoldPerX * ResolvedX;

    protected override void OnUpgrade() => GoldPerX = 5;

    protected override async Task ApplyEffect(PlayerChoiceContext ctx, CardPlay play, bool heads)
    {
        var x = ResolvedX;
        await GoldCost.Pay(Owner, GoldPerX * x);

        await DamageCmd.Attack(9 * x)
            .FromCard(this, play)
            .TargetingRandomOpponents(CombatState)
            .WithHitCount(x)
            .WithHitFx("vfx/vfx_attack_blunt", null, HeadsHitSfx)
            .Execute(ctx);
    }

    public override List<(string, string)>? Localization => new CardLoc(
        "全下",
        "#花费 *7×X 金币*。\n对随机敌人造成 9×X 伤害，共 X 次。");
}
