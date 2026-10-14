using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using GamblerMod.Core;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Models;

namespace GamblerMod.Cards.Common;

// 普通档 #1·飞牌（游戏设计 §5.1）：1 能 + 2 金，造成 9 伤害（升级 12）。
// 混合费用的最小样板（附加费·伤）。验收用：暂入起始牌，M1 验收后移除。
public sealed class FlyingCard : GamblerCard
{
    public FlyingCard() : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
    {
        BaseGoldCost = 2;
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(9, ValueProp.Move)];

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(3);

    protected override Task ApplyEffect(PlayerChoiceContext ctx, CardPlay play, bool heads)
        => DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, play)
            .Targeting(play.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(ctx);

    public override List<(string, string)>? Localization => new CardLoc(
        "飞牌",
        "#*额外花费 2 金币*。造成 !D! 伤害。");
}
