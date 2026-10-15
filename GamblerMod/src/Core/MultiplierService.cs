namespace GamblerMod.Core;

// 「风头正盛」倍率（游戏设计 §2.8 / 技术设计 gambler-tech-multiplier §3.1）：
//   掷出正面 +0.5 → 掷出反面立刻归零 → 带词条的牌数值 ×倍率、结算后清空 → 战斗结束清空。
// 内部以 int 记"0.5 的个数"（Steps：2 = x1.0、3 = x1.5），热路径出口只做一次乘法（红线 R7：
// 避免在 Modify*Multiplicative 里做 decimal 运算与分配）。
// 唯一真源：卡片条件判断（叠注/底牌）与钩子取值都读这里，不另存副本。
public static class MultiplierService
{
    private const int BaseSteps = 2; // x1.0

    private static int _steps = BaseSteps;

    /// <summary>倍率系数（x1.0 → 1.0m）。伤害/格挡/金币的出口都乘它。</summary>
    public static decimal Factor => _steps * 0.5m;

    /// <summary>内部刻度（0.5 为单位），供显示与调试。</summary>
    public static int Steps => _steps;

    /// <summary>
    /// 最近一次掷币**是否计入倍率**（= 显示用的"正/反"口径）：
    /// 自然正面 / 赌神 / 暗记 → true（显示正）；掷出反面 → false；**幸运筹码改币 → false**（保命但不涨，显示反）。
    /// </summary>
    public static bool LastFlipCounted { get; private set; }

    /// <summary>显示标签（x1.0 / x1.5 …）。仅显示层调用，不在热路径。</summary>
    public static string Label => LabelOf(_steps);

    public static string LabelOf(int steps) => "x" + (steps * 0.5m).ToString("0.0");

    /// <summary>掷出正面（自然正面 / 赌神 / 暗记）：+0.5。幸运筹码改币不调此方法。</summary>
    public static void AddHeads()
    {
        _steps++;
        LastFlipCounted = true;
    }

    /// <summary>掷出反面：立刻归零。</summary>
    public static void Reset()
    {
        _steps = BaseSteps;
        LastFlipCounted = false;
    }

    /// <summary>幸运筹码改币保命：倍率**不动**（不涨也不归零），但显示口径按"反"（改币不计）。</summary>
    public static void MarkSavedByChip() => LastFlipCounted = false;

    /// <summary>「风头正盛」牌兑现：结算后清空（与 Reset 语义分开，便于日志排查）。</summary>
    public static void Clear() => _steps = BaseSteps;
}
