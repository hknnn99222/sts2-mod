using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Gold;
using MegaCrit.Sts2.Core.Entities.Players;

namespace GamblerMod.Core;

// 金币费用（技术设计 §4.3，M4 完整版）：可支付检查 / 扣费 / 花费记账 / 假币免单。
// 牌面为文字记法（评审 D3）："花费 X 金币"独立成行写在卡描述首行，本类不负责 UI。
public static class GoldCost
{
    private static int _freeSpends; // 假币：本场战斗剩余免单次数（不消耗金币，也不计入花费）

    public static bool CanAfford(Player player, int amount) =>
        amount <= 0 || _freeSpends > 0 || player.Gold >= amount;

    /// <summary>假币（升级版）授予的免单次数。</summary>
    public static void GrantFreeSpending(int count) => _freeSpends += count;

    /// <summary>战斗结束时复位免单次数（在 AfterCombatEnd 里调用，为下一场战斗准备）。</summary>
    public static void ResetForNextCombat() => _freeSpends = 0;

    public static async Task Pay(Player player, int amount)
    {
        if (amount <= 0) return;
        if (_freeSpends > 0)
        {
            _freeSpends--;
            GD.Print($"[GamblerMod] 假币免单：{amount} 金币未扣除（剩余 {_freeSpends} 次）");
            return;
        }

        await PlayerCmd.LoseGold(amount, player, GoldLossType.Spent);
        await SpendTracker.Record(player, amount);
    }
}
