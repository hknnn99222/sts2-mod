using System.Collections.Generic;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Powers;

namespace GamblerMod.Powers;

// 流水（罕见能力）：你每累计花费 !Amount! 金币，抽 1 张牌（由 SpendTracker 消费）。
public sealed class CashFlowPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override List<(string, string)>? Localization => new PowerLoc(
        "流水",
        "#你每累计花费 !Amount! 金币，抽 1 张牌。",
        "#钱怎么流进来的，就怎么流出去。");
}
