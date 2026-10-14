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
    public const string Version = "0.2.0";

    public static void Initialize()
    {
        GD.Print($"[{ModId}] v{Version} 初始化（M2：做局工具；M3：普通档卡池）");

        new Harmony(ModId).PatchAll(Assembly.GetExecutingAssembly());

        Powers.GamblerPowers.Init();
        Relics.GamblerRelics.Init();
        Cards.GamblerCards.Init();

        _ = new Character.GamblerCharacter();

        GD.Print($"[{ModId}] 注册完成：角色 + 15 卡（起始2/普11/罕2/稀1）+ 3 力量 + 幸运筹码");
    }
}
