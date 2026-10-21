using System.Reflection;

namespace GamblerMod.Tests;

// 极简自校验 runner：不依赖任何 NuGet 测试框架（本机离线）。
// 用法：在 GamblerMod.Tests 目录 `dotnet run`；退出码 = 失败测试组数。
// Fact 方法签名约定：static void M(List<string> failures) —— 失败项往 failures 里加。
[AttributeUsage(AttributeTargets.Method)]
internal sealed class FactAttribute : Attribute;

internal static class Runner
{
    internal static List<string> Warnings { get; } = [];

    internal static int RunAll()
    {
        var tests = new List<(string Name, List<string> Failures, Action Body)>();
        foreach (var type in Assembly.GetExecutingAssembly().GetTypes())
        {
            foreach (var m in type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (m.GetCustomAttribute<FactAttribute>() == null) continue;
                var failures = new List<string>();
                tests.Add(($"{type.Name}.{m.Name}", failures, () => m.Invoke(null, [failures])));
            }
        }

        var failed = 0;
        Console.WriteLine($"GamblerMod 自校验测试 · {tests.Count} 组\n" + new string('=', 72));
        foreach (var (name, failures, body) in tests)
        {
            failures.Clear();
            try { body(); }
            catch (Exception e) { failures.Add("抛异常: " + (e.InnerException ?? e).Message); }
            if (failures.Count == 0)
            {
                Console.WriteLine($"[PASS] {name}");
            }
            else
            {
                failed++;
                Console.WriteLine($"[FAIL] {name}  ({failures.Count} 项)");
                foreach (var f in failures) Console.WriteLine($"        - {f}");
            }
        }
        if (Warnings.Count > 0)
        {
            Console.WriteLine("\n警告（不算失败，供人工确认）：");
            foreach (var w in Warnings) Console.WriteLine($"        ! {w}");
        }
        Console.WriteLine(new string('=', 72));
        Console.WriteLine(failed == 0 ? "全部通过" : $"{failed} 组失败");
        return failed;
    }
}
