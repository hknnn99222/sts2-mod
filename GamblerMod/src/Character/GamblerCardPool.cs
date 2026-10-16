using BaseLib.Abstracts;
using Godot;

namespace GamblerMod.Character;

// 赌徒职业卡池（技术设计 §3）：M1 含起始签名卡 + 飞牌；M3 起 43 张分批入池。
// 无 pck 资产：卡框/能量图标沿用库默认，卡背 HSV 取暗金色调。
public class GamblerCardPool : CustomCardPoolModel
{
    public override string Title => "GamblerMod";

    public override float H => 0.12f;
    public override float S => 0.70f;
    public override float V => 0.80f;

    public override Color DeckEntryCardColor => new("e8c34a");

    public override bool IsColorless => false;

    // 卡牌图鉴（百科）可见性（2026-10-17 批次 5）：BaseLib 的 CustomCardPoolMarkAsSeenPatch 在
    // 打开图鉴时对所有 SeenByDefault 的池调 SaveManager.MarkCardAsSeen，默认值是 false → 赌徒池
    // 的 76 张牌在图鉴里全被渲染成"未知/未解锁"。设为 true 即"开局全部可见"（写进存档进度）。
    public override bool SeenByDefault => true;
}
