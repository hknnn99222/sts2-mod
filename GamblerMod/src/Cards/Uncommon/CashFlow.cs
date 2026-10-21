using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using GamblerMod.Core;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace GamblerMod.Cards.Uncommon;

// 罕见 #30·流水（游戏设计 §5.2，v1.14 提速）：1 能能力，你每累计花费 6 金币，抽 1 张牌（升级：每累计 5 金币）。
public sealed class CashFlowCard() : GamblerCard(1, CardType.Power, CardRarity.Uncommon, TargetType.None)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new IntVar("Threshold", 6)];

    protected override void OnUpgrade() => DynamicVars["Threshold"].UpgradeValueBy(-1);

    protected override Task ApplyEffect(PlayerChoiceContext ctx, CardPlay play, bool heads)
        => PowerCmd.Apply<Powers.CashFlowPower>(ctx, Owner.Creature, DynamicVars["Threshold"].BaseValue, Owner.Creature, this);

    public override List<(string, string)>? Localization => new CardLoc(
        "流水",
        "#你每累计花费 !Threshold! 金币，抽 1 张牌。");
}
