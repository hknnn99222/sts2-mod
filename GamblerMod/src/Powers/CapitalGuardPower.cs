using System.Collections.Generic;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Powers;

namespace GamblerMod.Powers;

// 保本（稀有 #75 的能力本体，v1.15 由技能牌改造成能力）：
// 每次花费金币获得等量敏捷；Amount > 0 时（升级版）再获得等量力量。
// 触发点在 Core/SpendTracker.Record（与流水同一条记账通道，引擎没有"金币已花费"钩子），
// 本类只持有档位，不做逻辑。
public sealed class CapitalGuardPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override List<(string, string)>? Localization => new PowerLoc(
        "保本",
        "#每次花费金币，获得等量敏捷。",
        "#每次花费金币获得敏捷。");
}
