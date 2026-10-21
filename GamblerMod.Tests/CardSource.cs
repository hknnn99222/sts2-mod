using System.Text.RegularExpressions;

namespace GamblerMod.Tests;

// 卡牌源码的结构化模型：只做「读」，不做游戏内语义。
// 解析目标 = GamblerMod/src/Cards/{Basic,Common,Uncommon,Rare}/*.cs 的固定写法（模板一致）。
internal sealed partial class CardSource
{
    public string FileName = "";
    public string Raw = "";
    public string ClassName = "";
    public string Title = "";
    public string Desc = "";
    public string EffectBody = "";   // ApplyEffect 的实体（表达式体或块体）
    public string UpgradeBody = "";  // OnUpgrade 的实体
    public string CtorArgs = "";     // 基类构造实参文本（费用/类型/稀有度/目标）
    public bool IsGamble;
    public bool IsStreak;
    public int BaseGoldCost;
    public List<(string Kind, string? Name, decimal Value)> Vars = [];  // Kind = DamageVar/BlockVar/IntVar/…

    public string Type => Regex.Match(CtorArgs, @"CardType\.(\w+)").Groups[1].Value;
    public string Rarity => Regex.Match(CtorArgs, @"CardRarity\.(\w+)").Groups[1].Value;
    public string EnergyCost => Regex.Match(CtorArgs, @"\(\s*(-?\d+|X)\s*,").Groups[1].Value;

    public bool UsesInEffect(string varName) =>
        EffectBody.Contains($"\"{varName}\"") || (varName is "Damage" && Regex.IsMatch(EffectBody, @"DynamicVars\.Damage"))
        || (varName is "Block" && Regex.IsMatch(EffectBody, @"DynamicVars\.Block"));

    public static List<CardSource> LoadAll(string root)
    {
        var result = new List<CardSource>();
        foreach (var tier in new[] { "Basic", "Common", "Uncommon", "Rare" })
        {
            var dir = Path.Combine(root, tier);
            if (!Directory.Exists(dir)) continue;
            foreach (var file in Directory.GetFiles(dir, "*.cs"))
            {
                var src = File.ReadAllText(file);
                var c = Parse(file, src);
                if (c != null) result.Add(c);
            }
        }
        return result;
    }

    static CardSource? Parse(string file, string src)
    {
        var c = new CardSource { FileName = file, Raw = src };

        // Localization：CardLoc("名", "#描述")
        var loc = Regex.Match(src, @"new CardLoc\(\s*(?:\r?\n\s*)?""(?<t>[^""]+)""\s*,\s*(?:\r?\n\s*)?""(?<d>(?:[^""\\]|\\.)*)""", RegexOptions.ExplicitCapture);
        if (!loc.Success) return null; // 非卡牌文件（如单例仓）
        c.Title = loc.Groups["t"].Value;
        c.Desc = loc.Groups["d"].Value;

        // 构造实参：主构造器 GamblerCard(...) 或显式构造器的 : base(...)
        var ctor = Regex.Match(src, @"GamblerCard\((?<args>[^)]*\))", RegexOptions.ExplicitCapture);
        if (!ctor.Success)
            ctor = Regex.Match(src, @":\s*base\((?<args>[^)]*\))", RegexOptions.ExplicitCapture);
        if (ctor.Success) c.CtorArgs = ctor.Groups["args"].Value;

        c.ClassName = Regex.Match(src, @"(?:sealed\s+)?class\s+(\w+)").Groups[1].Value;

        c.IsGamble = Regex.IsMatch(src, @"IsGambleCard\s*=\s*true");
        c.IsStreak = Regex.IsMatch(src, @"IsHeadsStreakCard\s*=\s*true");
        var gold = Regex.Match(src, @"BaseGoldCost\s*=\s*(\d+)");
        if (gold.Success) c.BaseGoldCost = int.Parse(gold.Groups[1].Value);

        // CanonicalVars：具名与匿名两种
        foreach (Match m in Regex.Matches(src, @"new (\w+Var)\(""(?<n>\w+)"",\s*(?<v>-?\d+)"))
            c.Vars.Add((m.Groups[1].Value, m.Groups["n"].Value, decimal.Parse(m.Groups["v"].Value)));
        foreach (Match m in Regex.Matches(src, @"new (DamageVar|BlockVar)\((?<v>-?\d+),\s*ValueProp"))
            c.Vars.Add((m.Groups[1].Value, m.Groups[1].Value == "DamageVar" ? "Damage" : "Block", decimal.Parse(m.Groups["v"].Value)));

        c.EffectBody = ExtractBody(src, "ApplyEffect");
        c.UpgradeBody = ExtractBody(src, "OnUpgrade");
        return c;
    }

    // 提取 override 方法实体：表达式体（=> … ;）或块体（{ … }）
    static string ExtractBody(string src, string method)
    {
        var m = Regex.Match(src, method + @"\([^)]*\)\s*(=>|{)");
        if (!m.Success) return "";
        var start = m.Index + m.Length;
        if (m.Groups[2].Value == "=>")
        {
            var end = src.IndexOf(';', start);
            return end < 0 ? "" : src[start..end];
        }
        var depth = 1;
        for (var i = start; i < src.Length; i++)
        {
            if (src[i] == '{') depth++;
            else if (src[i] == '}' && --depth == 0) return src[start..i];
        }
        return "";
    }
}
