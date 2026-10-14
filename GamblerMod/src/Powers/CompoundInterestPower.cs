using System.Collections.Generic;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Powers;

namespace GamblerMod.Powers;

// 利滚利（稀有能力）：本场战斗中你每次获得金币时，额外获得 !Amount! 金币（由 GamblerHooks 消费）。
public sealed class CompoundInterestPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override List<(string, string)>? Localization => new PowerLoc(
        "利滚利",
        "#本场战斗中你每次获得金币时，额外获得 !Amount! 金币。",
        "#利息会自己长大。");
}
