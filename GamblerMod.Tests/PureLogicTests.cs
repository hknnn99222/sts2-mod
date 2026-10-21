using System.Reflection;

namespace GamblerMod.Tests;

// 可执行逻辑测试：MultiplierService 是纯 C#（无 Godot 依赖），直接反射调用真实现。
internal static class PureLogicTests
{
    static object? Call(string methodName, params object?[] args)
    {
        var t = typeof(GamblerMod.Core.MultiplierService);
        return t.GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)?.Invoke(null, args);
    }

    static object? Prop(string name)
    {
        var t = typeof(GamblerMod.Core.MultiplierService);
        return t.GetProperty(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)?.GetValue(null);
    }

    static void ResetToBase()
    {
        Call("Reset");   // Reset 置回基数并写 LastOutcome=Tails
        Call("Clear");   // Clear 只置回基数
    }

    [Fact]
    internal static void MultiplierSemantics(List<string> f)
    {
        // 基数 = 1.0（2 个半步）
        ResetToBase();
        if (!Equals(Prop("Factor"), 0.5m * 2)) f.Add($"初始 Factor 应为 1.0，实际 {Prop("Factor")}");

        // AddHeads 每次半步 + 写 LastOutcome
        Call("AddHeads"); Call("AddHeads");
        if (!Equals(Prop("Factor"), 2.0m)) f.Add($"两次正面后 Factor 应为 2.0，实际 {Prop("Factor")}");
        if (Prop("LastOutcome")!.ToString() != "Heads") f.Add($"AddHeads 后 LastOutcome 应为 Heads，实际 {Prop("LastOutcome")}");

        // AddSteps 只推刻度、不写 LastOutcome
        ResetToBase();
        Call("AddHeads");
        Call("AddSteps", 3);
        if (!Equals(Prop("Factor"), 3.0m)) f.Add($"AddSteps(3) 后 Factor 应为 3.0，实际 {Prop("Factor")}");
        if (Prop("LastOutcome")!.ToString() != "Heads") f.Add($"AddSteps 不应改写 LastOutcome（做局注入，2026-10-16 审查口径）");

        // Reset = 归零 + 记反面；Clear = 兑现清空（不动 LastOutcome）
        ResetToBase();
        Call("AddHeads");
        Call("Clear");
        if (!Equals(Prop("Factor"), 1.0m)) f.Add($"Clear 后应回 1.0，实际 {Prop("Factor")}");
        if (Prop("LastOutcome")!.ToString() != "Heads") f.Add($"Clear 不应改写 LastOutcome");
        Call("AddHeads");
        Call("Reset");
        if (!Equals(Prop("Factor"), 1.0m)) f.Add($"Reset 后应回 1.0，实际 {Prop("Factor")}");
        if (Prop("LastOutcome")!.ToString() != "Tails") f.Add($"Reset 应写 LastOutcome=Tails（徽章显「反」）");

        // LabelOf 格式
        var label = Call("LabelOf", 5);
        if (label!.ToString() != "x2.5") f.Add($"LabelOf(5) 应为 x2.5，实际 {label}");
    }
}
