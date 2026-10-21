using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Modding;

namespace GamblerMod;

// 唯一注册点（技术设计 §3）：Harmony 补丁 → 卡牌实例 → 角色实例，顺序固定。
//
// BaseLib 注册语义（源码核实，勿凭猜测改）：
//   1. Custom*Model 的构造器会自动调 CustomContentDictionary.AddCharacter/AddModel(this)
//      —— 所以"实例化即注册"，且必须发生在池合并（ModInitializer 内）之前；
//   2. 千万不要自己调 CustomContentDictionary.RegisterType(typeof(...))：它只把类型塞进
//      去重集合，既不挂载内容，还会让后续真正的 AddCharacter 因"类型已登记"而跳过（M0 踩过）；
//   3. [ModInitializer] 存在时游戏不再自动 PatchAll，本类补丁要自己打（模板 MainFile 同款写法）。
[ModInitializer("Initialize")]
public static class Entry
{
    public const string ModId = "GamblerMod";
    public const string Version = "0.6.0";

    public static void Initialize()
    {
        GD.Print($"[{ModId}] v{Version} 初始化（85 卡全量：起始 2 + 常规 85 + 四轴引擎）");

        new Harmony(ModId).PatchAll(Assembly.GetExecutingAssembly());

        _ = Core.GamblerKeywords.Gamble; // 自定义词条：touch 一下确保类型已加载（值由 BaseLib 在 ModelDb.Init 写入）
        _ = Core.GamblerHoverTips.Multiplier; // 徽章的自定义静态悬停提示（同机制，走 static_hover_tips 表）
        _ = new Core.GamblerHooks(); // 全局钩子单例（战斗态复位/高利贷/利滚利/风头倍率乘算）
        Powers.GamblerPowers.Init();
        Relics.GamblerRelics.Init();
        Cards.GamblerCards.Init();

        // 内容层订阅（技术设计 §4.2/§4.6 干预窗口）：Core 不引用具体内容类（约束④）
        Core.GambleService.Intervenor = Relics.LuckyChip.TryIntervene;        // 幸运筹码：反面后花 5 金改正面
        Core.GambleService.OnHeads += Powers.HouseRulesPower.OnHeadsFlipped;  // 保护费：每次掷出正面 +3 金
        Core.GambleService.OnHeads += Powers.CheatKingPower.OnHeadsFlipped;   // 千王：每回合前 N 次正面 → 1 次必正

        _ = UI.MultiplierBadge.Attached; // 头顶倍率徽章（AddedNode 静态字段初始化即登记）

        _ = new Character.GamblerCharacter();

        GD.Print($"[{ModId}] 注册完成：角色 + 85 卡（起始 2 + 常规 85：普 24/罕 35/稀 26）+ 8 力量 + 幸运筹码");
    }
}
