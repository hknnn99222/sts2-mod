using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using GamblerMod.Core;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace GamblerMod.Cards.Common;

// 普通 #4·打手（游戏设计 §5.1）：1 能 + 5 金，8 伤害并施加 1 易伤（升级 11 伤 / 2 易伤）。
public sealed class Thug : GamblerCard
{
    public Thug() : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
    {
        BaseGoldCost = 5;
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(8, ValueProp.Move),
        new IntVar("Vuln", 1),
    ];

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3);
        DynamicVars["Vuln"].UpgradeValueBy(1);
    }

    protected override async Task ApplyEffect(PlayerChoiceContext ctx, CardPlay play, bool heads)
    {
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, play)
            .Targeting(play.Target)
            .WithHitFx("vfx/vfx_attack_blunt")
            .Execute(ctx);

        await PowerCmd.Apply<VulnerablePower>(ctx, play.Target, DynamicVars["Vuln"].BaseValue, Owner.Creature, this);
    }

    public override List<(string, string)>? Localization => new CardLoc(
        "打手",
        "#花费 *5 金币*。\n造成 !D! 伤害，施加 !Vuln! 易伤。");
}
