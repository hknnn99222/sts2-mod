namespace GamblerMod.Core;

// 本回合花费金币累计（技术设计 §4.4）：连续下注 / 流水 的数据源（M3 消费）。
// M1 实现：单计数器 + 战斗实例变更时惰性清零（零订阅、零分配，红线 R3）。
// TODO(M2)：回合级清零（做局遗物钩子接入时一并实现）；多人分槽计数留 M3。
public static class SpendTracker
{
    private static int _spent;
    private static object? _combat;

    public static int SpentThisCombat(object combatIdentity)
    {
        ResetIfNewCombat(combatIdentity);
        return _spent;
    }

    public static void Record(int amount, object combatIdentity)
    {
        ResetIfNewCombat(combatIdentity);
        _spent += amount;
    }

    private static void ResetIfNewCombat(object combatIdentity)
    {
        if (ReferenceEquals(_combat, combatIdentity)) return;
        _combat = combatIdentity;
        _spent = 0;
    }
}
