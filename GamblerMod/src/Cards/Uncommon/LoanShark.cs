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

// 罕见 #27·高利贷（游戏设计 §5.2）：0 能，获得 14 金币；下场战斗开始时失去 6 金币（不足则全失，升级 +18 金）。
// M4 简化：欠款在下场战斗首次回合开始时结算（GamblerHooks），不做区分债务人的多人校准。
public sealed class LoanShark() : GamblerCard(0, CardType.Skill, CardRarity.Uncommon, TargetType.None)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new IntVar("GoldGain", 14),
        new IntVar("Debt", 6),
    ];

    protected override void OnUpgrade() => DynamicVars["GoldGain"].UpgradeValueBy(4);

    protected override Task ApplyEffect(PlayerChoiceContext ctx, CardPlay play, bool heads)
    {
        GamblerHooks.PendingLoanDebt += (int)DynamicVars["Debt"].BaseValue;
        return PlayerCmd.GainGold(DynamicVars["GoldGain"].BaseValue, Owner, true);
    }

    public override List<(string, string)>? Localization => new CardLoc(
        "高利贷",
        "#获得 !GoldGain! 金币。下场战斗开始时失去 !Debt! 金币（不足则全失）。");
}
