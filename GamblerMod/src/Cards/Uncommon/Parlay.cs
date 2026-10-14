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

// 罕见 #16·叠注（游戏设计 §5.2）：1 能，8 伤害；若你的上一掷为正面，改为 16 伤害（升级 10 / 20）。
// 注意：本卡自身不掷硬币，读取的是"最近一次定案"（GambleService.LastFlipHeads）。
public sealed class Parlay() : GamblerCard(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(8, ValueProp.Move),
        new IntVar("HotDamage", 16),
    ];

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2);
        DynamicVars["HotDamage"].UpgradeValueBy(4);
    }

    protected override Task ApplyEffect(PlayerChoiceContext ctx, CardPlay play, bool heads)
    {
        var last = GambleService.LastFlipHeads;
        var damage = last == true ? DynamicVars["HotDamage"].BaseValue : DynamicVars.Damage.BaseValue;
        return DamageCmd.Attack(damage)
            .FromCard(this, play)
            .Targeting(play.Target)
            .WithHitFx(last == true ? HeadsHitFx : TailsHitFx, null, last == true ? HeadsHitSfx : null)
            .Execute(ctx);
    }

    public override List<(string, string)>? Localization => new CardLoc(
        "叠注",
        "#造成 !D! 伤害。若你的上一掷为*正面*，改为造成 !HotDamage! 伤害。");
}
