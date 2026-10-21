using System.Text.RegularExpressions;

namespace GamblerMod.Tests;

// 卡牌契约测试：逐卡验证「描述承诺的 ↔ 代码真的做了」。
// 这些规则全部来自本项目真实踩过的坑（自掷牌、附魔裸值、heads 未用、漏注册）。
internal static class CardContracts
{
    static readonly string RepoRoot = Program.FindRepoRoot();
    static List<CardSource> Cards => CardSource.LoadAll(Path.Combine(RepoRoot, "GamblerMod", "src", "Cards"));

    // ---- R1 ApplyEffect 必须有实质内容（"卡没效果"的直接形态之一：方法体空/只返回 CompletedTask）----
    [Fact]
    internal static void ApplyEffectHasRealBody(List<string> f)
    {
        foreach (var c in Cards)
        {
            var body = c.EffectBody.Trim();
            var trivial = body.Length == 0
                || Regex.IsMatch(body, @"^return\s+Task\.CompletedTask;?\s*$")
                || Regex.IsMatch(body, @"^Task\.CompletedTask;?\s*$");
            // 能力牌的载体是 PowerCmd.Apply；技能/攻击牌必须真的调命令
            if (trivial && !body.Contains("PowerCmd"))
                f.Add($"{c.Title}（{c.FileName}）：ApplyEffect 空转——描述承诺了效果但方法体没有任何命令");
        }
    }

    // ---- R2 描述里的 !Var! 必须都有声明；声明且上卡的变量必须真的被 ApplyEffect 使用 ----
    [Fact]
    internal static void DescVarsMatchDeclarations(List<string> f)
    {
        foreach (var c in Cards)
        {
            var declared = c.Vars.Select(v => v.Name).ToHashSet();
            foreach (Match m in Regex.Matches(c.Desc, @"!(\w+)!"))
            {
                var token = m.Groups[1].Value;
                var canonical = token switch { "D" => "Damage", "B" => "Block", _ => token };
                if (!declared.Contains(canonical))
                    f.Add($"{c.Title}：描述用了 !{token}! 但 CanonicalVars 没声明它");
            }
            // 反向：声明 + 描述引用的变量，效果体里必须用（X 费金币变量在费用侧消费，豁免）
            var isXCost = c.CtorArgs.Contains("costsX: true") || c.CtorArgs.Contains("true)");
            foreach (var v in c.Vars)
            {
                if (v.Name is "Damage" or "Block") continue; // !D!/!B! 走标准变量，效果体用 DamageCmd 即视为已用
                var inDesc = c.Desc.Contains($"!{v.Name}!");
                if (!inDesc) continue;
                if (isXCost && v.Name.Contains("Gold")) continue;
                if (!c.UsesInEffect(v.Name))
                    f.Add($"{c.Title}：变量 {v.Name} 出现在卡面描述里，但 ApplyEffect 从未读它 → 这一支效果缺失");
            }
        }
    }

    // ---- R3 承诺→命令映射：描述写明的效果，效果体里必须有对应命令（能力牌允许转到 PowerCmd）----
    [Fact]
    internal static void PromisedEffectsHaveCommands(List<string> f)
    {
        foreach (var c in Cards)
        {
            var b = c.EffectBody;
            var isPower = c.Type == "Power";
            // 只对「效果侧」的描述做承诺映射：先剔除费用/否定/穿透等干扰短语
            var desc = Regex.Replace(c.Desc, @"花费\s*\*?[0-9!×X]*\s*金币\*?", "");   // 费用侧金币
            desc = Regex.Replace(desc, @"不消耗金币", "");                             // 假币：免单表述
            desc = Regex.Replace(desc, @"无视格挡", "");                               // 暗注：穿透表述
            desc = Regex.Replace(desc, @"消耗[^。；]*必正[^。；]*", "");                // 收网：消耗的是必正层数
            // 金币按子句判定：只有含「获得/失去/返还」的动作子句才算效果承诺（条件子句不算）
            var effectClauses = Regex.Split(desc, "[。；\n]")
                .Where(cl => cl.Contains("金币") && Regex.IsMatch(cl, @"获得|失去|返还"))
                .ToList();
            var promisesGold = effectClauses.Count > 0;

            void Need(string promise, bool condition, bool fileScope, params string[] tokens)
            {
                if (!condition) return;
                var scope = fileScope ? c.Raw : b;
                if (isPower && scope.Contains("PowerCmd.Apply")) return; // 能力牌把效果委托给 power
                if (!tokens.Any(t => scope.Contains(t)))
                    f.Add($"{c.Title}：描述承诺「{promise}」，但效果体没有任何对应命令（{string.Join("/", tokens)}）");
            }
            void NeedEffect(string promise, bool condition, params string[] tokens)
                => Need(promise, condition, false, tokens);

            NeedEffect("造成伤害", Regex.IsMatch(desc, @"造成.{0,12}伤害|!\w*Damage\w*!"), "DamageCmd.Attack", "CreatureCmd.Damage", "DamageVar", "VigorPower");
            NeedEffect("获得格挡", desc.Contains("格挡"), "GainBlock", "BlockVar", "Block");
            NeedEffect("金币变化", promisesGold, "GainGold", "LoseGold", "GoldCost", "MultiplierService");
            NeedEffect("获得能量", Regex.IsMatch(desc, @"获得.{0,6}能量|!Energy!"), "GainEnergy");
            NeedEffect("抽牌", Regex.IsMatch(desc, @"抽.{0,4}张|!Cards!"), "CardPileCmd.Draw", "Draw");
            NeedEffect("施加状态", Regex.IsMatch(desc, @"施加|虚弱|易伤|力量|活力"), "PowerCmd.Apply", "VigorPower", "WeakPower", "VulnerablePower");
            NeedEffect("必正", desc.Contains("必正") || desc.Contains("必为正"), "ForceNextFlips", "ForcedHeadsRemaining", "ConsumeForcedHeads");
            // 「消耗」词条可能挂在 AfterCreated 的 AddKeyword 上（撕毁契约的做法）——查整个文件
            Need("消耗", Regex.IsMatch(desc, @"消耗") && !Regex.IsMatch(desc, @"不消耗"), true,
                "Exhaust", "CardCmd.Exhaust", "CardKeyword.Exhaust", "AddKeyword");
        }
    }

    // ---- R4 语义伤害/格挡数值必须是 DamageVar/BlockVar（附魔/力量管线只认引擎原生变量）----
    [Fact]
    internal static void DamageBlockVarsUseEngineTypes(List<string> f)
    {
        foreach (var c in Cards)
        {
            foreach (var (kind, name, _) in c.Vars)
            {
                if (name is null) continue;
                var semantic = Regex.IsMatch(name, @"Damage|Block");
                if (semantic && kind is not ("DamageVar" or "BlockVar"))
                    f.Add($"{c.Title}：{name} 是伤害/格挡语义却声明为 {kind}——卡面会永远显示裸值（不吃附魔/力量），须用 {(name.Contains("Block") ? "BlockVar" : "DamageVar")} 的具名重载");
            }
        }
    }

    // ---- R5 升级钩子必须做点事 ----
    [Fact]
    internal static void UpgradeDoesSomething(List<string> f)
    {
        foreach (var c in Cards)
        {
            var u = c.UpgradeBody;
            var ok = u.Contains("UpgradeValueBy") || u.Contains("EnergyCost") || u.Contains("BaseGoldCost")
                     || u.Contains("UpgradeKeywords") || u.Contains("IsEnchantmentPreview")
                     || c.EffectBody.Contains("IsUpgraded");
            if (!ok)
                f.Add($"{c.Title}：OnUpgrade 空或没有改任何数值/费用（升级等于白升）");
        }
    }

    // ---- R6 赌博牌的掷币必须真的改变效果：IsGambleCard ⇒ 效果体必须读 heads；描述必须有正/负两支 ----
    [Fact]
    internal static void GambleCardsActuallyBranchOnHeads(List<string> f)
    {
        foreach (var c in Cards)
        {
            if (c.IsGamble && !c.EffectBody.Contains("heads"))
                f.Add($"{c.Title}：IsGambleCard=true 但 ApplyEffect 从不使用 heads —— 掷了硬币却没有任何分支，硬币是装饰");
            if (c.IsGamble && (!c.Desc.Contains("硬币为正") || !c.Desc.Contains("硬币为负")))
                f.Add($"{c.Title}：赌博牌描述缺少「硬币为正/硬币为负」两支表述（当前描述：{Trunc(c.Desc)}）");
            if (!c.IsGamble && c.Desc.Contains("硬币为"))
                f.Add($"{c.Title}：描述提到硬币但没设 IsGambleCard —— 永远掷不了币，描述与实现脱钩");
        }
    }

    // ---- R7 描述必须以 # 开头（SimpleLoc：否则 !Var! 原样显示）----
    [Fact]
    internal static void DescriptionStartsWithHash(List<string> f)
    {
        foreach (var c in Cards)
            if (!c.Desc.StartsWith("#"))
                f.Add($"{c.Title}：描述没有以 # 开头，卡面数值占位符会原样显示");
    }

    // ---- R8 名称唯一 + 每张卡都在 GamblerCards 单例仓注册（漏注册 = 图鉴/抽卡永远见不到）----
    [Fact]
    internal static void AllCardsRegisteredWithUniqueTitles(List<string> f)
    {
        var cards = Cards;
        var dup = cards.GroupBy(c => c.Title).Where(g => g.Count() > 1);
        foreach (var g in dup) f.Add($"卡名重复：{g.Key} ×{g.Count()}");

        var regSrc = File.ReadAllText(Path.Combine(RepoRoot, "GamblerMod", "src", "Cards", "GamblerCards.cs"));
        foreach (var c in cards)
        {
            if (!Regex.IsMatch(regSrc, @"new\s+\w+\." + c.ClassName + @"\("))
                f.Add($"{c.Title}（{c.ClassName}）：卡文件存在但 GamblerCards.Init 没有注册它——这张牌进不了卡池");
        }
        f.AddRange(cards.Count == 87 ? Array.Empty<string>()
            : [$"卡文件总数 {cards.Count} ≠ 87（起始 2 + 常规 85）——有文件被增删但没同步口径"]);
    }

    static string Trunc(string s) => s.Length <= 60 ? s : s[..60] + "…";
}
