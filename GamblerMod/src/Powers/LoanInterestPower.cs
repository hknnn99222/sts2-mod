using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Gold;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace GamblerMod.Powers;

// 高利贷的利息（普通 #27 的能力本体，v1.12 新增）：
// 每回合开始失去 X 金币（X = 层数，默认 5）；金币不足则把身上剩的全扣掉。
// 同一场战斗里多次打出高利贷会叠层（借得越多、每回合还得越多）。
public sealed class LoanInterestPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext ctx, Player player)
    {
        var due = (int)Amount;
        if (due <= 0) return;

        var actual = System.Math.Min(due, player.Gold);
        if (actual <= 0) return;

        await PlayerCmd.LoseGold(actual, player, GoldLossType.Lost);
    }

    public override List<(string, string)>? Localization => new PowerLoc(
        "利息",
        "#每回合开始失去 !Amount! 金币。",
        "#每回合开始失去金币。");
}
