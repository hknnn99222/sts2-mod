using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Gold;
using MegaCrit.Sts2.Core.Entities.Players;

namespace GamblerMod.Core;

// 金币费用（技术设计 §4.3）：可支付检查 + 扣费 + 花费记账。
// 牌面为文字记法（评审 D3）："额外花费 X 金币"直接写在卡描述里，本类不负责 UI。
public static class GoldCost
{
    public static bool CanAfford(Player player, int amount) => amount <= 0 || player.Gold >= amount;

    public static async Task Pay(Player player, int amount)
    {
        if (amount <= 0) return;
        await PlayerCmd.LoseGold(amount, player, GoldLossType.Spent);
        SpendTracker.Record(amount, player.RunState);
    }
}
