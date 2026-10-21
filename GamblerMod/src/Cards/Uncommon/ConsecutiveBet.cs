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

// 罕见 #13·连续下注（游戏设计 §5.2，v1.12 改版）：1 能，9 伤害；
// 本回合**花费过金币**时额外追加 1 次伤害（升级 2 次）。
// 钩子由「每 5 金 +4 伤」改为「花费追踪·追加伤害」——门槛只看"有没有花过"，收益改为多段。
public sealed class ConsecutiveBet() : GamblerCard(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(9, ValueProp.Move),
        new IntVar("ExtraHits", 1),
    ];

    protected override void OnUpgrade() => DynamicVars["ExtraHits"].UpgradeValueBy(1);

    protected override async Task ApplyEffect(PlayerChoiceContext ctx, CardPlay play, bool heads)
    {
        var hits = 1;
        if (SpendTracker.SpentThisTurn > 0) hits += (int)DynamicVars["ExtraHits"].BaseValue;

        for (var i = 0; i < hits; i++)
        {
            await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
                .FromCard(this, play)
                .Targeting(play.Target)
                .WithHitFx("vfx/vfx_attack_slash")
                .Execute(ctx);
        }
    }

    public override List<(string, string)>? Localization => new CardLoc(
        "连续下注",
        "#造成 !D! 伤害。本回合若花费过金币，额外造成 !ExtraHits! 次伤害。");
}
