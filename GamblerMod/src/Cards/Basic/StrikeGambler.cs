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

namespace GamblerMod.Cards.Basic;

// 起始牌·打击（v1.16）：赌徒自己的打击，不再借用铁甲战士的 StrikeIronclad。
// 数值与原版基础牌一致：1 能造成 6 伤害（升级 9），带 Strike 标签（供"数打击"类效果识别）。
public sealed class StrikeGambler() : GamblerCard(1, CardType.Attack, CardRarity.Basic, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(6, ValueProp.Move)];

    protected override HashSet<CardTag> CanonicalTags => [CardTag.Strike];

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(3);

    protected override Task ApplyEffect(PlayerChoiceContext ctx, CardPlay play, bool heads)
        => DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, play)
            .Targeting(play.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(ctx);

    public override List<(string, string)>? Localization => new CardLoc(
        "打击",
        "#造成 !D! 伤害。");
}
