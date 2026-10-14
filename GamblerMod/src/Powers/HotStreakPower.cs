using System.Collections.Generic;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Powers;

namespace GamblerMod.Powers;

// 手气正旺（罕见能力卡的效果）：每层赌博正面率 +15%（升级版每层 +25）。
// 由 GambleService.HeadsRatePercent 聚合读取（加法，全局上限 75%）。
public sealed class HotStreakPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override List<(string, string)>? Localization => new PowerLoc(
        "手气正旺",
        "#你的赌博正面率 +!Amount!%（加法，上限 75%）。",
        "#你的赌博正面率提高。");
}
