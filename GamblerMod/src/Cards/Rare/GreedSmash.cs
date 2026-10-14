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

// 稀有 #33·贪婪重击（游戏设计 §5.3）：3 能 + 8 金，20 伤害；击杀则获得 20 金币（升级 26 伤害）。
public sealed class GreedSmash : GamblerCard
{
    public GreedSmash() : base(3, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
    {
        BaseGoldCost = 8;
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(20, ValueProp.Move),
        new IntVar("Bounty", 20),
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
            await PlayerCmd.GainGold(DynamicVars["Bounty"].BaseValue, Owner, true);
    }

    public override List<(string, string)>? Localization => new CardLoc(
        "贪婪重击",
        "#花费 *8 金币*。\n造成 !D! 伤害。*击杀*则获得 !Bounty! 金币。");
}
