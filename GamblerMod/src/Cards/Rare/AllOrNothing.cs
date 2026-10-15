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

// 稀有 #31·孤注一掷（游戏设计 §5.3 真赌）：1 能 + 10 金。
// 赌博：正面 → 43 伤害；反面 → 25 伤害且自己受到 6 伤害（升级：正面 50 / 反面 29）。
public sealed class AllOrNothing : GamblerCard
{
    public AllOrNothing() : base(1, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
    {
        BaseGoldCost = 10;
        IsGambleCard = true;
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(27, ValueProp.Move),
        new IntVar("HeadsDamage", 43),
        new IntVar("TailsDamage", 25),
        new IntVar("TailsSelfHurt", 6),
    ];

    protected override void OnUpgrade()
    {
        DynamicVars["HeadsDamage"].UpgradeValueBy(7);
        DynamicVars["TailsDamage"].UpgradeValueBy(4);
    }

    protected override async Task ApplyEffect(PlayerChoiceContext ctx, CardPlay play, bool heads)
    {
        var damage = heads ? DynamicVars["HeadsDamage"].BaseValue : DynamicVars["TailsDamage"].BaseValue;

        await DamageCmd.Attack(damage)
            .FromCard(this, play)
            .Targeting(play.Target)
            .WithHitFx(heads ? HeadsHitFx : TailsHitFx, null, heads ? HeadsHitSfx : null)
            .Execute(ctx);

        if (!heads)
            await CreatureCmd.Damage(ctx, Owner.Creature, DynamicVars["TailsSelfHurt"].BaseValue, ValueProp.Move, this, play);
    }

    public override List<(string, string)>? Localization => new CardLoc(
        "孤注一掷",
        "#花费 *10 金币*。\n造成 !D! 伤害。*赌博*：正面 → 改为造成 !HeadsDamage! 伤害；反面 → 改为 !TailsDamage! 伤害且自己受到 !TailsSelfHurt! 伤害。");
}
