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

namespace GamblerMod.Cards.Rare;

// 稀有 #80·满贯（游戏设计 §5.3 / §5.5）：2 能 + 18 金，造成 30 伤害；
// 若本场战斗中你掷出过**正面**，改为 50 伤害（升级 38 / 62）。
// 钩子：特异·赌博计数（复用 GambleService 的掷骰记录，注：该记录按**整场战斗**累积，非单回合）。
public sealed class GrandSlam : GamblerCard
{
    public GrandSlam() : base(2, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
    {
        BaseGoldCost = 18;
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(30, ValueProp.Move),
        new IntVar("HotDamage", 50),
    ];

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(8);
        DynamicVars["HotDamage"].UpgradeValueBy(12);
    }

    protected override Task ApplyEffect(PlayerChoiceContext ctx, CardPlay play, bool heads)
    {
        var hot = GambleService.HeadsCountThisTurn > 0;
        var damage = hot ? (int)DynamicVars["HotDamage"].BaseValue : DynamicVars.Damage.BaseValue;

        return DamageCmd.Attack(damage)
            .FromCard(this, play)
            .Targeting(play.Target)
            .WithHitFx(hot ? HeadsHitFx : "vfx/vfx_attack_blunt", null, hot ? HeadsHitSfx : null)
            .Execute(ctx);
    }

    public override List<(string, string)>? Localization => new CardLoc(
        "满贯",
        "#花费 *18 金币*。\n造成 !D! 伤害。若本场战斗中你掷出过*正面*，改为 !HotDamage! 伤害。");
}
