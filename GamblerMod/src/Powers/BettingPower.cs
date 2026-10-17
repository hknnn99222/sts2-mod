using System.Collections.Generic;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace GamblerMod.Powers;

// 押注（罕见能力，2026-10-17 四轴改造·赌命轴）：按「已失去生命」的比例加成你造成的伤害。
//
// 为什么用 ModifyDamageAdditive 而不是自己攒层数：
//   ① 加成要随血线实时变化（自伤越多越强），预存层数做不到"实时"；
//   ② 走伤害钩子 = 卡面预览也会跟着变（与力量同一条通道），玩家看到的就是实际值；
//   ③ 不需要幂等/回合清空等状态管理——顺带绕开 `PowerModel.Amount` 的 setter 是私有的
//      （power 内部改不了自己的层数，只能靠外部 PowerCmd.Apply）。
public sealed class BettingPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override decimal ModifyDamageAdditive(Creature target, decimal amount, ValueProp props,
        Creature dealer, CardModel? cardSource, CardPlay? cardPlay)
    {
        // 只加自己打出去的伤害；自伤（target == 自己）不吃加成，否则自己越打越疼
        if (dealer != Owner || target == Owner) return 0m;
        var lost = Owner.MaxHp - Owner.CurrentHp;
        if (lost <= 0) return 0m;
        return lost * Amount / 100m;   // Amount = 10（10%）/ 15（15%）
    }

    public override List<(string, string)>? Localization => new PowerLoc(
        "押注",
        "#你造成的伤害获得「已失去生命」的 !Amount!% 加成。",
        "#血押在桌上，手自然就重。");
}
