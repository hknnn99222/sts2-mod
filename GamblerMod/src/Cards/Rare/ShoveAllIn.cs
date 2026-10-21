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
// 对随机敌人造成 12×X 伤害共 X 次（升级金币费降为 5×X）。
// 钩子：可变投入·X费多段（§5.5 口径）。可打出检查按最低 X=1 判灰（同梭哈）。
// 2026-10-16 审查：TargetType 改为 RandomEnemy（旧 AnyEnemy 会强制选目标但选择被忽略）；
// "9×X × X 次 = 9X²" 的二次增长与"总伤 9X"两种读法待设计拍板，代码暂按现实现不动。
public sealed class ShoveAllIn : GamblerCard
{
    public ShoveAllIn() : base(1, CardType.Attack, CardRarity.Rare, TargetType.RandomEnemy, costsX: true)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new IntVar("GoldX", 7), // 每 X 的金币系数；升级 −2（7→5）。卡面费用随 DynamicVar 同步显示
    ];

    private int GoldPerX => (int)DynamicVars["GoldX"].BaseValue;

    private int ResolvedX => Math.Max(1, ResolveEnergyXValue());

    public override int EffectiveGoldCost => GoldPerX * ResolvedX;

    protected override void OnUpgrade() => DynamicVars["GoldX"].UpgradeValueBy(-2);

    protected override async Task ApplyEffect(PlayerChoiceContext ctx, CardPlay play, bool heads)
    {
        var x = ResolvedX;
        await GoldCost.Pay(Owner, GoldPerX * x);

        await DamageCmd.Attack(12 * x)
            .FromCard(this, play)
            .TargetingRandomOpponents(CombatState)
            .WithHitCount(x)
            .WithHitFx("vfx/vfx_attack_blunt", null, HeadsHitSfx)
            .Execute(ctx);
    }

    public override List<(string, string)>? Localization => new CardLoc(
        "全下",
        "#花费 *!GoldX!×X 金币*。\n对随机敌人造成 12×X 伤害，共 X 次。");
}
