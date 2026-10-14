using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace GamblerMod.Core;

// 金币花费记账（技术设计 §4.4，M4 完整版）：
//   SpentThisTurn    —— 本回合花费（连续下注消费，回合开始由 GamblerHooks 重置）
//   流水抽牌         —— 累计花费每过阈值抽 1 张（阈值 = 流水能力层数，未持有则不触发）
// 容器与计数全复用（红线 R3）；多人注记：单静态按 M4 简化，M5 校准。
public static class SpendTracker
{
    private static int _spentThisTurn;
    private static int _cumulative;      // 流水累计（跨回合，按战斗）
    private static int _thresholdPool;   // 已触发过的阈值累计

    public static int SpentThisTurn => _spentThisTurn;
    public static int SpentThisCombat => _cumulative;

    public static void OnTurnStart() => _spentThisTurn = 0;

    public static void OnCombatStart()
    {
        _spentThisTurn = 0;
        _cumulative = 0;
        _thresholdPool = 0;
    }

    public static async Task Record(Player player, int amount)
    {
        if (amount <= 0) return;
        _spentThisTurn += amount;
        _cumulative += amount;

        // 流水：每累计花费过阈值 → 抽 1 张（持有 CashFlowPower 时阈值 = 其 Amount）
        var cashFlow = player.Creature.GetPower<Powers.CashFlowPower>();
        if (cashFlow == null) return;

        var threshold = (int)cashFlow.Amount;
        if (threshold <= 0) return;

        var draws = _cumulative / threshold - _thresholdPool / threshold;
        if (draws <= 0) return;
        _thresholdPool = _cumulative - _cumulative % threshold;
        for (var i = 0; i < draws; i++)
            await CardPileCmd.Draw(new ThrowingPlayerChoiceContext(), 1, player);
    }
}
