using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Gold;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace GamblerMod.Powers;

// 分红（普通 #83·分红的能力本体，v1.10 由「血本」改造而来）：
// 每回合开始支付 3 金币，回复 X 点生命（X = 层数，升级 2）。
// 金币不足 3 时不触发（不做"付多少回多少"的比例折算——避免 0 金白嫖回血）。
// 赌命轴的续航件：把金币流水换成血量，让自伤牌能一直转下去。
public sealed class DividendPower : CustomPowerModel
{
    public const int Cost = 3;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext ctx, Player player)
    {
        if (player.Gold < Cost) return;

        // 直接扣款、不走 GoldCost.Pay：分红是"被动维持费"，不计入花费追踪（加注/点码等只读主动花费）。
        await PlayerCmd.LoseGold(Cost, player, GoldLossType.Spent);
        await CreatureCmd.Heal(player.Creature, Amount, true);
    }

    public override List<(string, string)>? Localization => new PowerLoc(
        "分红",
        "#每回合开始支付 *3 金币*，回复 !Amount! 点生命。",
        "#每回合开始支付金币，回复生命。");
}
