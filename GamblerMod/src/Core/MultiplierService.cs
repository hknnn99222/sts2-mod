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

    /// <summary>最近一次掷币的**归属**（供徽章显示；与倍率口径严格一致）。</summary>
    public enum FlipOutcome
    {
        None = 0,  // 本场还没掷过
        Heads = 1, // 正面（自然/赌神/暗记）→ 倍率 +0.5
        Tails = 2, // 反面 → 倍率归零
    }

    /// <summary>
    /// 最近一次掷币的归属：自然正面 / 赌神 / 暗记 → Heads；掷出反面 → Tails。
    /// 注：幸运筹码改币**也记 Tails**——倍率只认赌博本身的结果（2026-10-16 用户拍板：要归零）。
    /// </summary>
    public static FlipOutcome LastOutcome { get; private set; }

    /// <summary>显示标签（x1.0 / x1.5 …）。仅显示层调用，不在热路径。</summary>
    public static string Label => LabelOf(_steps);

    public static string LabelOf(int steps) => "x" + (steps * 0.5m).ToString("0.0");

    /// <summary>掷出正面（自然正面 / 赌神 / 暗记）：+0.5。幸运筹码改币不调此方法。</summary>
    public static void AddHeads()
    {
        _steps++;
        LastOutcome = FlipOutcome.Heads;
    }

    /// <summary>掷出反面：立刻归零。</summary>
    public static void Reset()
    {
        _steps = BaseSteps;
        LastOutcome = FlipOutcome.Tails;
    }

    /// <summary>「风头正盛」牌兑现：结算后清空（与 Reset 语义分开，便于日志排查）。</summary>
    public static void Clear() => _steps = BaseSteps;
}
