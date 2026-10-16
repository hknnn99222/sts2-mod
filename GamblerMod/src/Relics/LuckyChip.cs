using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using GamblerMod.Core;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Gold;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace GamblerMod.Relics;

// 幸运筹码（初始遗物，占位名）：每回合一次，掷出反面后花 5 金币改为正面（设计 §4）。
// M2 简化：金币足够时自动触发（无询问弹窗——弹窗 UI 待美术资产批一起做，已在技术设计记档）。
// 价格 5 金是头号平衡旋钮（设计 §7.3-1）。
[Pool(typeof(Character.GamblerRelicPool))]
public sealed class LuckyChip : CustomRelicModel
{
    public const int Cost = 5;

    public override RelicRarity Rarity => RelicRarity.Starter;

    private static bool _usedThisTurn;

    // GambleService 干预窗口调用（Intervenor 由 Entry 注册）：反面且本回合未用且够钱 → 扣 5 金改正面
    public static async Task<bool> TryIntervene(Player player)
    {
        if (_usedThisTurn) return false;
        if (player.GetRelic<LuckyChip>() == null) return false;
        if (player.Gold < Cost) return false;

        await PlayerCmd.LoseGold(Cost, player, GoldLossType.Spent);
        _usedThisTurn = true; // 扣款成功后再置位（2026-10-16 审查修正：扣款异常不白耗本回合次数）
        GD.Print($"[GamblerMod] 幸运筹码：花费 {Cost} 金币将反面改为正面（本回合已用）");
        return true;
    }

    // 每回合重置使用次数（原版遗物同款钩子模式）
    public override Task AfterPlayerTurnStart(PlayerChoiceContext ctx, Player player)
    {
        _usedThisTurn = false;
        return Task.CompletedTask;
    }

    public override List<(string, string)>? Localization => new RelicLoc(
        "幸运筹码",
        "#每回合一次：掷出*反面*时，自动花费 5 金币将其改为*正面*。",
        "运气不好？砸钱买。");
}
