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

// 稀有 #89·赌命（v1.8 四轴改造·赌命轴大炮）：2能+10金，造成「已失去生命」的 50% 伤害（升级 100%）。
// 赌命轴的兑现器：血线越低越爆——把"自伤换节奏"的代价转成终结手段。
public sealed class StakeYourLife : GamblerCard
{
    public StakeYourLife() : base(2, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
    {
        BaseGoldCost = 10;
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [new IntVar("Factor", 50)];

    protected override void OnUpgrade() => DynamicVars["Factor"].UpgradeValueBy(50);

    protected override async Task ApplyEffect(PlayerChoiceContext ctx, CardPlay play, bool heads)
    {
        var lost = Owner.Creature.MaxHp - Owner.Creature.CurrentHp;
        var damage = lost * DynamicVars["Factor"].BaseValue / 100m;
        if (damage <= 0m) return;

        await DamageCmd.Attack(damage)
            .FromCard(this, play)
            .Targeting(play.Target)
            .WithHitFx(HeadsHitFx)
            .Execute(ctx);
    }

    public override List<(string, string)>? Localization => new CardLoc(
        "赌命",
        "#花费 *10 金币*。\n造成等于「你已失去生命」的 !Factor!% 的伤害。");
}
