using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace GamblerMod.Powers;

// 存钱罐（普通能力卡的效果）：每回合开始获得 X 金币（X = 层数）。
// 钩子签名 = 游戏 Hook.AfterPlayerTurnStart 去掉首个 ICombatState 参数（原版遗物同款模式）。
public sealed class PiggyBankPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext ctx, Player player)
    {
        await PlayerCmd.GainGold(Amount, player, false);
    }

    // 能力图标（v1.17，美术资源已入 pck）
    public override string? CustomPackedIconPath =>
        "res://GamblerMod/images/powers/piggy_bank_power.png";

    public override string? CustomBigIconPath =>
        "res://GamblerMod/images/powers/big/piggy_bank_power_big.png";

    public override List<(string, string)>? Localization => new PowerLoc(
        "存钱罐",
        "#每回合开始获得 !Amount! 金币。",
        "#每回合开始获得金币。");
}
