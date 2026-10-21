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

// 稀有 #33·爆机（游戏设计 §5.3，v1.15 改版）：3 能 + 10 金，40 伤害；击杀则获得 30 金币（升级 46 伤害）。
public sealed class GreedSmash : GamblerCard
{
    public GreedSmash() : base(3, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
    {
        BaseGoldCost = 10;
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(40, ValueProp.Move),
        new IntVar("Bounty", 30),
    ];

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(6);

    protected override async Task ApplyEffect(PlayerChoiceContext ctx, CardPlay play, bool heads)
    {
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, play)
            .Targeting(play.Target)
            .WithHitFx("vfx/vfx_rock_shatter", null, HeadsHitSfx)
            .Execute(ctx);

        if (play.Target.IsDead)
            await PlayerCmd.GainGold(DynamicVars["Bounty"].BaseValue, Owner, false);
    }

    public override List<(string, string)>? Localization => new CardLoc(
        "爆机",
        "#花费 *10 金币*。\n造成 !D! 伤害。*击杀*则获得 !Bounty! 金币。");
}
