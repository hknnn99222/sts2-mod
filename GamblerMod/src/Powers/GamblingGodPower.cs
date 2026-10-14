using System.Collections.Generic;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Powers;

namespace GamblerMod.Powers;

// 赌神（稀有能力卡的效果）：所有赌博视为正面（无视 75% 上限，设计 §2.3）。
public sealed class GamblingGodPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override List<(string, string)>? Localization => new PowerLoc(
        "赌神",
        "#你的所有*赌博*视为正面。",
        "#你的所有赌博视为正面。");
}
