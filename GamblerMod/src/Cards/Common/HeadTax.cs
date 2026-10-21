using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using GamblerMod.Core;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace GamblerMod.Cards.Common;

// 普通 #53·人头费（游戏设计 §5.1 / §5.5）：1 能，每个敌人给予 5 金币（升级 7）。
// 钩子：即时产金·按敌人数。
public sealed class HeadTax : GamblerCard
{
    public HeadTax() : base(1, CardType.Skill, CardRarity.Common, TargetType.None) { }

    protected override IEnumerable<DynamicVar> CanonicalVars => [new IntVar("PerEnemy", 5)];

    protected override void OnUpgrade() => DynamicVars["PerEnemy"].UpgradeValueBy(2);

    protected override Task ApplyEffect(PlayerChoiceContext ctx, CardPlay play, bool heads)
    {
        var alive = 0;
        var enemies = CombatState.Enemies;
        for (var i = 0; i < enemies.Count; i++)
            if (enemies[i].IsAlive) alive++;

        if (alive <= 0) return Task.CompletedTask;
        return PlayerCmd.GainGold((int)DynamicVars["PerEnemy"].BaseValue * alive, Owner, false);
    }

    public override List<(string, string)>? Localization => new CardLoc(
        "人头费",
        "#每个敌人给予 !PerEnemy! 金币。");
}
