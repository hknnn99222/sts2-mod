using BaseLib.Abstracts;
using Godot;

namespace GamblerMod.Character;

// 赌徒职业遗物池（模板 CharModRelicPool 同款范式）：遗物必须挂 [Pool] 且池被角色引用，
// 否则初始化抛 "must be marked with a PoolAttribute"（M4 踩过）。
// M4 只有幸运筹码（Starter 稀有度，不会作为奖励掉落）；7 件职业遗物扩展期入池。
public class GamblerRelicPool : CustomRelicPoolModel
{
    public override Color LabOutlineColor => new("e8c34a");
}
