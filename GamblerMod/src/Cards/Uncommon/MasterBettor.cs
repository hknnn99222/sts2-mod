using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using GamblerMod.Core;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace GamblerMod.Cards.Uncommon;

// 罕见 #59·再加注（游戏设计 §5.2 / §5.5）：1 能，自动花费至多 9 金，按档位解锁（升级 12 伤害）。
// 档位：造成 8 伤害 → ≥3 金另施 2 易伤 → ≥6 金另抽 1 张 → ≥9 金再 +8 伤害。
// 钩子：可变投入·档位解锁（M3 简化沿用加注的"自动花满"输入方式）。
public sealed class MasterBettor : GamblerCard
{
    private const int MaxSpend = 9;

    public MasterBettor() : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy) { }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(8, ValueProp.Move),
        new IntVar("Bonus", 8),
        new IntVar("Vuln", 2),
    ];

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(4);

    protected override async Task ApplyEffect(PlayerChoiceContext ctx, CardPlay play, bool heads)
    {
        var spend = Math.Min(MaxSpend, Owner.Gold);
        if (spend > 0) await GoldCost.Pay(Owner, spend);

        var damage = DynamicVars.Damage.BaseValue;
        if (spend >= 9) damage += (int)DynamicVars["Bonus"].BaseValue;

        if (spend >= 3 && play.Target != null)
            await PowerCmd.Apply<VulnerablePower>(ctx, play.Target, DynamicVars["Vuln"].BaseValue, Owner.Creature, this);

        await DamageCmd.Attack(damage)
            .FromCard(this, play)
            .Targeting(play.Target)
            .WithHitFx("vfx/vfx_attack_blunt")
            .Execute(ctx);

        if (spend >= 6) await CardPileCmd.Draw(ctx, 1, Owner);
    }

    public override List<(string, string)>? Localization => new CardLoc(
        "再加注",
        "#自动花费至多 *9 金币*，按投入解锁：造成 !D! 伤害；≥3 金再施加 !Vuln! 易伤；≥6 金再抽 1 张；≥9 金再 +!Bonus! 伤害。");
}
