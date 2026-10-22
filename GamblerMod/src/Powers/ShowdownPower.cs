using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace GamblerMod.Powers;

// 存钱（稀有 #49 的能力本体，v1.15 由攻击牌改造成能力）：
// 每次获得金币时，获得等量格挡（Amount = 额外加成，基础 0、升级 5）。
//
// ⚠️ 2026-10-21 修（用户实测"没有效果"）：power **收不到 run 级金币钩子**——
// 引擎的金币钩子（AfterModifyingGoldGained / AfterGoldGained）只派发给
// runState.IterateHookListeners（= ModHelper 订阅的单例模型，如 Core.GamblerHooks），
// 生物身上的 power 不在那个列表里（原版也没有任何"获得金币时触发"的力量，佐证此路不通）。
// 因此本类不再覆写引擎钩子，改由 Core.GamblerHooks.AfterModifyingGoldGained 转发到
// 下面的 OnGoldGained——数值仍留在本文件（约束④），转发模式与利滚利同款。
public sealed class ShowdownPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    /// <summary>由 GamblerHooks 转发：这一笔金币到账前，给等量（+Amount）格挡。</summary>
    internal static async Task OnGoldGained(Player player, decimal amount)
    {
        var power = player.Creature.GetPower<ShowdownPower>();
        if (power == null) return;

        await CreatureCmd.GainBlock(player.Creature,
            new BlockVar(amount + power.Amount, ValueProp.Move), null);
    }

    // 能力图标（v1.17，美术资源已入 pck）
    public override string? CustomPackedIconPath =>
        "res://GamblerMod/images/powers/showdown_power.png";

    public override string? CustomBigIconPath =>
        "res://GamblerMod/images/powers/big/showdown_power_big.png";

    public override List<(string, string)>? Localization => new PowerLoc(
        "存钱",
        "#每次获得金币时，获得等量格挡。",
        "#每次获得金币时获得格挡。");
}
