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

// 罕见 #48·通杀（游戏设计 §5.2 / §2.8）：2 能 + 5 金，对所有敌人造成 16 伤害 ×风头倍率（升级 20）。
public sealed class TheFinale : GamblerCard
{
    public TheFinale() : base(2, CardType.Attack, CardRarity.Uncommon, TargetType.AllEnemies)
    {
        BaseGoldCost = 5;
        IsHeadsStreakCard = true;
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(16, ValueProp.Move)];

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(4);

    protected override Task ApplyEffect(PlayerChoiceContext ctx, CardPlay play, bool heads)
        => DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, play)
            .TargetingAllOpponents(CombatState)
            .WithHitFx("vfx/vfx_attack_blunt")
            .Execute(ctx);

    public override List<(string, string)>? Localization => new CardLoc(
        "通杀",
        "#花费 *5 金币*。\n对所有敌人造成 !D! 伤害。数值随头顶倍率放大，结算后清空。");
}
