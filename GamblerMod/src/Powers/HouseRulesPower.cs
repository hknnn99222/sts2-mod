using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace GamblerMod.Powers;

// 赌场规则（罕见能力）：每回合开始获得 4 金（升级后层数=5）；每次掷出正面额外 +3 金
// （由本类静态方法订阅 GambleService.OnHeads 实现——Entry 注册；升级只改回合定额）。
public sealed class HouseRulesPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    /// <summary>
    /// 保护费·正面 +3 金（设计 §5.2 #29）。从 GamblerHooks 移入本文件（2026-10-16）：
    /// 数值与卡面/描述同处内容层，改数值只动本文件（约束④，Core 不再持有卡牌专属数值）。
    /// OnHeads 是 Action 事件（无法 await），GainGold 走 fire-and-forget——多掷牌连续正面时
    /// 会有多个并发命令，命令队列天然串行，属已知口径。
    /// </summary>
    internal static void OnHeadsFlipped(Player player)
    {
        if (player.Creature.GetPower<HouseRulesPower>() == null) return;
        _ = PlayerCmd.GainGold(3, player, false);
    }

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext ctx, Player player)
    {
        await PlayerCmd.GainGold(Amount, player, false);
    }

    public override List<(string, string)>? Localization => new PowerLoc(
        "赌场规则",
        "#每回合开始获得 !Amount! 金币。每次掷出*正面*，额外获得 3 金币。",
        "#赌场永远赢。");
}
