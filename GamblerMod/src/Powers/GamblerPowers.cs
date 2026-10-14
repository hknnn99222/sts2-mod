namespace GamblerMod.Powers;

// 力量单例仓：与卡牌同理，必须在 Entry.Initialize（池合并前）实例化注册。
public static class GamblerPowers
{
    public static HotStreakPower HotStreak { get; private set; } = null!;
    public static GamblingGodPower GamblingGod { get; private set; } = null!;
    public static PiggyBankPower PiggyBank { get; private set; } = null!;

    public static void Init()
    {
        HotStreak = new HotStreakPower();
        GamblingGod = new GamblingGodPower();
        PiggyBank = new PiggyBankPower();
    }
}
