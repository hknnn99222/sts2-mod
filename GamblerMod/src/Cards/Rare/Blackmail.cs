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

namespace GamblerMod.Cards.Rare;

// 稀有 #40·抽佣（游戏设计 §5.3，v1.15 改版）：1 能 + 4 金，施加 6 点力量削弱与 4 易伤（升级 8 / 5）。
// 力量削弱 = 负数 StrengthPower。
public sealed class Blackmail : GamblerCard
{
    public Blackmail() : base(1, CardType.Skill, CardRarity.Rare, TargetType.AnyEnemy)
    {
        BaseGoldCost = 4;
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new IntVar("StrengthLoss", 6),
        new IntVar("Vuln", 4),
    ];

    protected override void OnUpgrade()
    {
        DynamicVars["StrengthLoss"].UpgradeValueBy(2);
        DynamicVars["Vuln"].UpgradeValueBy(1);
    }

    protected override async Task ApplyEffect(PlayerChoiceContext ctx, CardPlay play, bool heads)
    {
        await PowerCmd.Apply<StrengthPower>(ctx, play.Target, -DynamicVars["StrengthLoss"].BaseValue, Owner.Creature, this);
        await PowerCmd.Apply<VulnerablePower>(ctx, play.Target, DynamicVars["Vuln"].BaseValue, Owner.Creature, this);
    }

    public override List<(string, string)>? Localization => new CardLoc(
        "抽佣",
        "#花费 *4 金币*。\n施加 !StrengthLoss! 点*力量削弱*与 !Vuln! 易伤。");
}
