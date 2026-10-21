namespace GamblerMod.Tests;

internal static class Program
{
    internal static string FindRepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null && !Directory.Exists(Path.Combine(dir.FullName, "GamblerMod", "src", "Cards")))
            dir = dir.Parent;
        return dir?.FullName ?? throw new InvalidOperationException("找不到仓库根（GamblerMod/src/Cards）");
    }

    private static int Main() => Runner.RunAll();
}
