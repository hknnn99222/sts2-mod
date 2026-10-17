using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using GamblerMod.Core;
using Godot;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace GamblerMod.Powers;

// 千王（稀有能力，2026-10-17 四轴改造·出千轴）：每回合前 Amount 次掷出正面 → 转化成 1 次必正。
//
// 定位：出千轴的"生成器"——把偶然的运气固化成确定性资源（必正），再由「收网」一次性兑现成伤害。
// 订阅 GambleService.OnHeads（Entry 注册，静态方法组，红线 R5）；每回合计数放 power 实例上
// （与 Core 的静态掷币状态同口径：单机；多人隔离仍待做，见 HANDOFF 待办）。
// 必须**每回合封顶**，否则"必正→正面→千王再给必正"会变成无限循环。
public sealed class CheatKingPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    private int _usedThisTurn;

    internal static void OnHeadsFlipped(Player player)
    {
        var power = player.Creature.GetPower<CheatKingPower>();
        if (power == null || power._usedThisTurn >= power.Amount) return;
        power._usedThisTurn++;
        GambleService.ForceNextFlips(1);
        GD.Print($"[GamblerMod] 千王：正面 → 1 次必正（本回合 {power._usedThisTurn}/{power.Amount}）");
    }

    public override Task AfterPlayerTurnStart(PlayerChoiceContext ctx, Player player)
    {
        _usedThisTurn = 0;
        return Task.CompletedTask;
    }

    public override List<(string, string)>? Localization => new PowerLoc(
        "千王",
        "#每回合前 !Amount! 次掷出*正面*时，获得 1 次*必正*。",
        "#赢得多了，自然知道怎么赢。");
}
