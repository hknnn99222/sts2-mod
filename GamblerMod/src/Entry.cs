using Godot;
using MegaCrit.Sts2.Core.Modding;

namespace GamblerMod;

// 唯一注册点（技术设计 §3）：所有内容模型集中在此登记，内容层互相不引用。
//
// BaseLib 注册语义（源码核实，勿凭猜测改）：
//   1. Custom*Model 的构造器会自动调 CustomContentDictionary.AddCharacter/AddModel(this)
//      —— 所以"实例化即注册"，这里只要 new 一次并让 CustomCharacters 列表持有引用即可；
//   2. 千万不要自己调 CustomContentDictionary.RegisterType(typeof(...))：它只把类型塞进
//      去重集合，既不挂载内容，还会让后续真正的 AddCharacter 因"类型已登记"而跳过（M0 踩过）。
[ModInitializer("Initialize")]
public static class Entry
{
    public const string ModId = "GamblerMod";
    public const string Version = "0.1.0";

    public static void Initialize()
    {
        GD.Print($"[{ModId}] v{Version} 初始化（M0：占位角色）");

        // 构造器内自动注册进 BaseLib 的 CustomCharacters（ModelDb.AllCharacters 补丁会追加）
        _ = new Character.GamblerCharacter();

        GD.Print($"[{ModId}] 注册完成：GamblerCharacter（占位视觉=Ironclad）");
    }
}
