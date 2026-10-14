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

// 罕见 #13·连续下注（游戏设计 §5.2）：1 能，7 伤害；本回合每花费过 5 金币，额外 +4 伤害（升级 +6）。
public sealed class ConsecutiveBet() : GamblerCard(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(7, ValueProp.Move),
        new IntVar("PerGold", 4),
    ];

    protected override void OnUpgrade() => DynamicVars["PerGold"].UpgradeValueBy(2);

    protected override Task ApplyEffect(PlayerChoiceContext ctx, CardPlay play, bool heads)
    {
        var bonus = SpendTracker.SpentThisTurn / 5 * (int)DynamicVars["PerGold"].BaseValue;
        return DamageCmd.Attack(DynamicVars.Damage.BaseValue + bonus)
            .FromCard(this, play)
            .Targeting(play.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(ctx);
    }

    public override List<(string, string)>? Localization => new CardLoc(
        "连续下注",
        "#造成 !D! 伤害。本回合每花费过 5 金币，额外 +!PerGold! 伤害。");
}
