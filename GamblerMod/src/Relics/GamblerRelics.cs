namespace GamblerMod.Relics;

// 遗物单例仓：与卡牌同理，必须在 Entry.Initialize（池合并前）实例化注册。
public static class GamblerRelics
{
    public static LuckyChip LuckyChip { get; private set; } = null!;

    public static void Init()
    {
        LuckyChip = new LuckyChip();
    }
}
