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

// 普通 #55·点码（游戏设计 §5.1 / §5.5）：1 能，获得 5 格挡；本回合每花费过 5 金币，额外 +3 格挡（升级 +4）。
// 钩子：花费追踪·转格挡。
public sealed class CountMoney : GamblerCard
{
    public CountMoney() : base(1, CardType.Skill, CardRarity.Common, TargetType.None) { }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(5, ValueProp.Move),
        new IntVar("SpendStep", 5), // 每花费 SpendStep 金币多一次结算
        new IntVar("PerSpend", 3),
    ];

    protected override void OnUpgrade() => DynamicVars["PerSpend"].UpgradeValueBy(1);

    protected override async Task ApplyEffect(PlayerChoiceContext ctx, CardPlay play, bool heads)
    {
        var step = (int)DynamicVars["SpendStep"].BaseValue;
        var steps = SpendTracker.SpentThisTurn / step;
        var block = DynamicVars.Block.BaseValue + steps * (int)DynamicVars["PerSpend"].BaseValue;
        await CreatureCmd.GainBlock(Owner.Creature, new BlockVar(block, ValueProp.Move), play);
    }

    public override List<(string, string)>? Localization => new CardLoc(
        "点码",
        "#获得 !B! 格挡。本回合每花费过 *!SpendStep! 金币*，额外 +!PerSpend! 格挡。");
}
