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

// 普通 #2·加注（游戏设计 §5.1，v1.10 改版）：1 能攻击，基础 7 伤（升级 10）。
// 钩子由「可变投入」改为「花费追踪·回响」：本回合每花费 1 金币，伤害 +1。
// 与老版本的区别：不再在出牌时主动选投入额（无输入 UI），读的是本回合已经花掉的钱。
public sealed class AddMore() : GamblerCard(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(7, ValueProp.Move)];

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(3);

    protected override Task ApplyEffect(PlayerChoiceContext ctx, CardPlay play, bool heads)
        => DamageCmd.Attack(DynamicVars.Damage.BaseValue + SpendTracker.SpentThisTurn)
            .FromCard(this, play)
            .Targeting(play.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(ctx);

    public override List<(string, string)>? Localization => new CardLoc(
        "加注",
        "#造成 !D! 伤害。本回合每花费 1 金币，伤害 +1。");
}
