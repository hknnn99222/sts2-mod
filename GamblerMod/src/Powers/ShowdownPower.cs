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
// 走 AfterModifyingGoldGained(player, amount) 拿到"这一笔金币的实际数额"，
// 与利滚利（AfterGoldGained，不看数额）互补。
public sealed class ShowdownPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterModifyingGoldGained(Player player, decimal amount)
    {
        if (amount <= 0) return;
        var block = amount + Amount;
        await CreatureCmd.GainBlock(player.Creature, new BlockVar(block, ValueProp.Move), null);
    }

    public override List<(string, string)>? Localization => new PowerLoc(
        "存钱",
        "#每次获得金币时，获得等量格挡。",
        "#每次获得金币时获得格挡。");
}
