namespace GamblerMod.Cards;

// 卡牌单例仓：所有卡实例必须在 Entry.Initialize（池合并前）创建——
// CustomCardModel 构造器自动注册进池/ModelDb，晚于池合并会抛异常（BaseLib 源码语义）。
// 加新卡：写卡类 → 这里加一行属性 + Init 一行 → Entry 不动。
// 当前进度：43/43 全量（起始 2 + 普 11 + 罕 19 + 稀 13，含 M2 做局三件）。
public static class GamblerCards
{
    // Basic（起始签名牌）
    public static Basic.PrivateStash PrivateStash { get; private set; } = null!;
    public static Basic.TryYourLuck TryYourLuck { get; private set; } = null!;

    // Common（普通档 11）
    public static Common.FlyingCard FlyingCard { get; private set; } = null!;
    public static Common.AddMore AddMore { get; private set; } = null!;
    public static Common.GambleAll GambleAll { get; private set; } = null!;
    public static Common.Thug Thug { get; private set; } = null!;
    public static Common.BountyHunter BountyHunter { get; private set; } = null!;
    public static Common.SmallChange SmallChange { get; private set; } = null!;
    public static Common.BuyPeace BuyPeace { get; private set; } = null!;
    public static Common.FlashOfInsight FlashOfInsight { get; private set; } = null!;
    public static Common.BuyTheRoad BuyTheRoad { get; private set; } = null!;
    public static Common.TripUp TripUp { get; private set; } = null!;
    public static Common.PiggyBankCard PiggyBank { get; private set; } = null!;

    // Uncommon（罕见档 19）
    public static Uncommon.AllIn AllIn { get; private set; } = null!;
    public static Uncommon.ConsecutiveBet ConsecutiveBet { get; private set; } = null!;
    public static Uncommon.Ransack Ransack { get; private set; } = null!;
    public static Uncommon.DoubleDown DoubleDown { get; private set; } = null!;
    public static Uncommon.Parlay Parlay { get; private set; } = null!;
    public static Uncommon.Comeback Comeback { get; private set; } = null!;
    public static Uncommon.WinStreak WinStreak { get; private set; } = null!;
    public static Uncommon.HiredThugs HiredThugs { get; private set; } = null!;
    public static Uncommon.SecretMark SecretMark { get; private set; } = null!;
    public static Uncommon.Dividend Dividend { get; private set; } = null!;
    public static Uncommon.CountCards CountCards { get; private set; } = null!;
    public static Uncommon.Pawnshop Pawnshop { get; private set; } = null!;
    public static Uncommon.Rake Rake { get; private set; } = null!;
    public static Uncommon.Insurance Insurance { get; private set; } = null!;
    public static Uncommon.Counterfeit Counterfeit { get; private set; } = null!;
    public static Uncommon.LoanShark LoanShark { get; private set; } = null!;
    public static Uncommon.HotStreakCard HotStreak { get; private set; } = null!;
    public static Uncommon.HouseRulesCard HouseRules { get; private set; } = null!;
    public static Uncommon.CashFlowCard CashFlow { get; private set; } = null!;

    // Rare（稀有档 13）
    public static Rare.AllOrNothing AllOrNothing { get; private set; } = null!;
    public static Rare.RussianRoulette RussianRoulette { get; private set; } = null!;
    public static Rare.GreedSmash GreedSmash { get; private set; } = null!;
    public static Rare.TripleSevens TripleSevens { get; private set; } = null!;
    public static Rare.Flush Flush { get; private set; } = null!;
    public static Rare.Destiny Destiny { get; private set; } = null!;
    public static Rare.SlotMachine SlotMachine { get; private set; } = null!;
    public static Rare.HitMan HitMan { get; private set; } = null!;
    public static Rare.TearContract TearContract { get; private set; } = null!;
    public static Rare.Blackmail Blackmail { get; private set; } = null!;
    public static Rare.HoleCard HoleCard { get; private set; } = null!;
    public static Rare.GamblingGodCard GamblingGod { get; private set; } = null!;
    public static Rare.CompoundInterestCard CompoundInterest { get; private set; } = null!;

    public static void Init()
    {
        PrivateStash = new Basic.PrivateStash();
        TryYourLuck = new Basic.TryYourLuck();

        FlyingCard = new Common.FlyingCard();
        AddMore = new Common.AddMore();
        GambleAll = new Common.GambleAll();
        Thug = new Common.Thug();
        BountyHunter = new Common.BountyHunter();
        SmallChange = new Common.SmallChange();
        BuyPeace = new Common.BuyPeace();
        FlashOfInsight = new Common.FlashOfInsight();
        BuyTheRoad = new Common.BuyTheRoad();
        TripUp = new Common.TripUp();
        PiggyBank = new Common.PiggyBankCard();

        AllIn = new Uncommon.AllIn();
        ConsecutiveBet = new Uncommon.ConsecutiveBet();
        Ransack = new Uncommon.Ransack();
        DoubleDown = new Uncommon.DoubleDown();
        Parlay = new Uncommon.Parlay();
        Comeback = new Uncommon.Comeback();
        WinStreak = new Uncommon.WinStreak();
        HiredThugs = new Uncommon.HiredThugs();
        SecretMark = new Uncommon.SecretMark();
        Dividend = new Uncommon.Dividend();
        CountCards = new Uncommon.CountCards();
        Pawnshop = new Uncommon.Pawnshop();
        Rake = new Uncommon.Rake();
        Insurance = new Uncommon.Insurance();
        Counterfeit = new Uncommon.Counterfeit();
        LoanShark = new Uncommon.LoanShark();
        HotStreak = new Uncommon.HotStreakCard();
        HouseRules = new Uncommon.HouseRulesCard();
        CashFlow = new Uncommon.CashFlowCard();

        AllOrNothing = new Rare.AllOrNothing();
        RussianRoulette = new Rare.RussianRoulette();
        GreedSmash = new Rare.GreedSmash();
        TripleSevens = new Rare.TripleSevens();
        Flush = new Rare.Flush();
        Destiny = new Rare.Destiny();
        SlotMachine = new Rare.SlotMachine();
        HitMan = new Rare.HitMan();
        TearContract = new Rare.TearContract();
        Blackmail = new Rare.Blackmail();
        HoleCard = new Rare.HoleCard();
        GamblingGod = new Rare.GamblingGodCard();
        CompoundInterest = new Rare.CompoundInterestCard();
    }
}
