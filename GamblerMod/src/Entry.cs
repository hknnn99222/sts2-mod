using BaseLib.Patches.Content;
using Godot;
using MegaCrit.Sts2.Core.Modding;

namespace GamblerMod;

// 唯一注册点（技术设计 §3）：所有内容模型集中在此登记，内容层互相不引用。
[ModInitializer("Initialize")]
public static class Entry
{
    public const string ModId = "GamblerMod";
    public const string Version = "0.1.0";

    public static void Initialize()
    {
        GD.Print($"[{ModId}] v{Version} 初始化（M0：占位角色）");

        CustomContentDictionary.RegisterType(typeof(Character.GamblerCharacter));

        GD.Print($"[{ModId}] 注册完成：GamblerCharacter（占位视觉=Ironclad）");
    }
}
