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

// 普通 #55·点码（游戏设计 §5.1 / §5.5，v1.10 改版）：1 能，获得 5 格挡；本回合每花费 1 金币，额外 +1 格挡（升级基础 8）。
// 钩子：花费追踪·转格挡（v1.10 由"每 5 金 +3"改为"每 1 金 +1"，更直观地读整条金币流水）。
public sealed class CountMoney : GamblerCard
{
    public CountMoney() : base(1, CardType.Skill, CardRarity.Common, TargetType.None) { }

    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(5, ValueProp.Move)];

    protected override void OnUpgrade() => DynamicVars.Block.UpgradeValueBy(3);

    protected override async Task ApplyEffect(PlayerChoiceContext ctx, CardPlay play, bool heads)
    {
        var block = DynamicVars.Block.BaseValue + SpendTracker.SpentThisTurn;
        await CreatureCmd.GainBlock(Owner.Creature, new BlockVar(block, ValueProp.Move), play);
    }

    public override List<(string, string)>? Localization => new CardLoc(
        "点码",
        "#获得 !B! 格挡。本回合每花费 1 金币，额外 +1 格挡。");
}
