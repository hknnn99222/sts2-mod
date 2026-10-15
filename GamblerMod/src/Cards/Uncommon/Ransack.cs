using System.Collections.Generic;
using System.Linq;
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

// 罕见 #14·清台（游戏设计 §5.2）：1 能 + 4 金，对所有敌人 13 伤害，每击杀一个 +7 金（升级 15 伤）。
public sealed class Ransack : GamblerCard
{
    public Ransack() : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AllEnemies)
    {
        BaseGoldCost = 4;
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(13, ValueProp.Move),
        new IntVar("Loot", 7),
    ];

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(2);

    protected override async Task ApplyEffect(PlayerChoiceContext ctx, CardPlay play, bool heads)
    {
        var aliveBefore = CombatState.Enemies.Count(e => e.IsAlive);

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, play)
            .TargetingAllOpponents(CombatState)
            .WithHitFx("vfx/vfx_attack_blunt")
            .Execute(ctx);

        var kills = aliveBefore - CombatState.Enemies.Count(e => e.IsAlive);
        if (kills > 0)
            await PlayerCmd.GainGold(DynamicVars["Loot"].BaseValue * kills, Owner, true);
    }

    public override List<(string, string)>? Localization => new CardLoc(
        "清台",
        "#花费 *4 金币*。\n对所有敌人造成 !D! 伤害。每*击杀*一个敌人，获得 !Loot! 金币。");
}
