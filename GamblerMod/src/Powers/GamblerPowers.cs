namespace GamblerMod.Powers;

// 力量单例仓：与卡牌同理，必须在 Entry.Initialize（池合并前）实例化注册。
public static class GamblerPowers
{
    public static HotStreakPower HotStreak { get; private set; } = null!;
    public static GamblingGodPower GamblingGod { get; private set; } = null!;
    public static PiggyBankPower PiggyBank { get; private set; } = null!;
    public static HouseRulesPower HouseRules { get; private set; } = null!;
    public static CashFlowPower CashFlow { get; private set; } = null!;
    public static CompoundInterestPower CompoundInterest { get; private set; } = null!;
    public static BettingPower Betting { get; private set; } = null!;      // 2026-10-17 四轴·赌命
    public static CheatKingPower CheatKing { get; private set; } = null!;  // 2026-10-17 四轴·出千

    public static void Init()
    {
        HotStreak = new HotStreakPower();
        GamblingGod = new GamblingGodPower();
        PiggyBank = new PiggyBankPower();
        HouseRules = new HouseRulesPower();
        CashFlow = new CashFlowPower();
        CompoundInterest = new CompoundInterestPower();
        Betting = new BettingPower();
        CheatKing = new CheatKingPower();
    }
}
