using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace GamblerMod.Powers;

// 赌场规则（罕见能力）：每回合开始获得 4 金（升级后层数=5）；每次掷出正面额外 +3 金
// （由 GamblerHooks 订阅 GambleService.OnHeads 实现，升级只改回合定额）。
public sealed class HouseRulesPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext ctx, Player player)
    {
        await PlayerCmd.GainGold(Amount, player, true);
    }

    public override List<(string, string)>? Localization => new PowerLoc(
        "赌场规则",
        "#每回合开始获得 !Amount! 金币。每次掷出*正面*，额外获得 3 金币。",
        "#赌场永远赢。");
}
