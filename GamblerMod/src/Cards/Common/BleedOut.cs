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

// 普通 #84·放血（v1.8 四轴改造·赌命轴）：1 能攻击，失 3 血造成 9 伤害（升级 12）。
// 赌命轴的廉价输出：自伤是"成本"也是"燃料"（押注按已失去生命加成伤害）。
public sealed class BleedOut() : GamblerCard(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(9, ValueProp.Move), new IntVar("HpLoss", 3)];

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(4);

    protected override async Task ApplyEffect(PlayerChoiceContext ctx, CardPlay play, bool heads)
    {
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, play)
            .Targeting(play.Target)
            .Execute(ctx);
        await CreatureCmd.Damage(ctx, Owner.Creature, DynamicVars["HpLoss"].BaseValue, ValueProp.Move, this, play);
    }

    public override List<(string, string)>? Localization => new CardLoc(
        "放血",
        "#造成 !D! 伤害。自己失去 !HpLoss! 点生命。");
}
