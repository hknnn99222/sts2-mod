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

// 罕见 #19·摇骰（游戏设计 §5.2，v1.12 改版）：1 能 + 3 金，对随机敌人 4 伤害 4 次；
// 赌博：正面 → 改为 6 次（升级 5×4 / 正面 5×6——升级只加伤害，不再改正面每段数值）。
public sealed class HiredThugs : GamblerCard
{
    public HiredThugs() : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.RandomEnemy)
    {
        BaseGoldCost = 3;
        IsGambleCard = true;
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(4, ValueProp.Move),
        new IntVar("Hits", 4),
        new IntVar("HeadsHits", 6),
    ];

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(1);

    protected override Task ApplyEffect(PlayerChoiceContext ctx, CardPlay play, bool heads)
        => DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, play)
            .TargetingRandomOpponents(CombatState)
            .WithHitCount((int)DynamicVars[heads ? "HeadsHits" : "Hits"].BaseValue)
            .WithHitFx(heads ? HeadsHitFx : TailsHitFx, null, heads ? HeadsHitSfx : null)
            .Execute(ctx);

    public override List<(string, string)>? Localization => new CardLoc(
        "摇骰",
        "#花费 *3 金币*。\n硬币为正时：对随机敌人造成 !D! 伤害 !HeadsHits! 次；硬币为负时：造成 !D! 伤害 !Hits! 次。");
}
