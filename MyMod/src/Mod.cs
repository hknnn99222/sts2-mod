using HarmonyLib;

namespace MyMod;

// ===== STS2 官方 Mod 机制（提取自游戏 v0.111.0 自带的 API 文档 sts2.xml）=====
//
// 1) 默认入口 —— 什么都不用写：
//    游戏加载本 DLL 后会为它创建一个 Harmony 实例并自动调用 PatchAll()，
//    本程序集内所有 [HarmonyPatch] 补丁自动生效。
//    下面的空模板可以先跑通“构建 → 部署 → 游戏里看到 Mod 已加载”整个流程。
//
// 2) 自定义入口 —— 需要在加载时执行初始化代码时用：
//    给一个 public static 类加特性，游戏会改为调用其中指定名称的方法：
//
//    [MegaCrit.Sts2.Core.Modding.ModInitializer("Initialize")]
//    public static class Entry
//    {
//        public static void Initialize()
//        {
//            // 初始化逻辑（注册模型、读配置……）
//        }
//    }
//
// 3) 添加自定义内容（卡牌 / 遗物等模型池）：见 notes/modding-api.md 里的
//    ModHelper.AddModelToPool / SubscribeForRunStateHooks / SubscribeForCombatStateHooks。
//
// 4) 查游戏 API：lib\sts2.xml 是完整的 XML 文档注释，VS 里可直接转到定义，
//    或用文本编辑器搜索类名（命名空间均为 MegaCrit.Sts2.Core.*）。

public static class Mod
{
    public const string ModId = "MyMod";
    public const string Version = "0.1.0";

    // 第一个 Harmony 补丁示例（示意写法，替换成真实类型后取消注释）：
    //
    // [HarmonyPatch(typeof(MegaCrit.Sts2.Core.某命名空间.某类型), nameof(某类型.某方法))]
    // [HarmonyPostfix]
    // internal static class 某类型_某方法_Patch
    // {
    //     private static void Postfix()
    //     {
    //         // 方法调用后追加逻辑
    //     }
    // }
}
