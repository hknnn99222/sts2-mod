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
    public static DividendPower Dividend { get; private set; } = null!;    // 2026-10-19 v1.10 分红（原血本）
    public static LoanInterestPower LoanInterest { get; private set; } = null!; // 2026-10-21 v1.12 高利贷利息
    public static RoulettePower Roulette { get; private set; } = null!;      // 2026-10-21 v1.15 俄罗斯轮盘
    public static ShowdownPower Showdown { get; private set; } = null!;      // 2026-10-21 v1.15 存钱
    public static CapitalGuardPower CapitalGuard { get; private set; } = null!; // 2026-10-21 v1.15 保本

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
        Dividend = new DividendPower();
        LoanInterest = new LoanInterestPower();
        Roulette = new RoulettePower();
        Showdown = new ShowdownPower();
        CapitalGuard = new CapitalGuardPower();
    }
}
