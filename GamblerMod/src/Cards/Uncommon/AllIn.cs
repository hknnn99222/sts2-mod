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

namespace GamblerMod.Cards.Uncommon;

// 罕见 #12·梭哈（游戏设计 §5.2）：X 能 + 10×X 金，造成 10×X 伤害（升级金币费降为 8×X）。
// 可打出检查按最低 X=1 的金币（10/8）判灰；打出时按实际 X 结算。
public sealed class AllIn : GamblerCard
{
    public AllIn() : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy, costsX: true)
    {
        GoldPerX = 10;
    }

    public int GoldPerX { get; set; }

    private int ResolvedX => Math.Max(1, ResolveEnergyXValue());

    public override int EffectiveGoldCost => GoldPerX * ResolvedX;

    protected override void OnUpgrade() => GoldPerX = 8;

    protected override async Task ApplyEffect(PlayerChoiceContext ctx, CardPlay play, bool heads)
    {
        var x = ResolvedX;
        await GoldCost.Pay(Owner, GoldPerX * x);

        await DamageCmd.Attack(10 * x)
            .FromCard(this, play)
            .Targeting(play.Target)
            .WithHitFx("vfx/vfx_heavy_blunt", null, HeadsHitSfx)
            .Execute(ctx);
    }

    public override List<(string, string)>? Localization => new CardLoc(
        "梭哈",
        "#花费 *10×X 金币*。\n造成 10×X 伤害。");
}
