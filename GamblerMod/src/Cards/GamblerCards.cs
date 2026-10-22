namespace GamblerMod.Cards;

// 卡牌单例仓：所有卡实例必须在 Entry.Initialize（池合并前）创建——
// CustomCardModel 构造器自动注册进池/ModelDb，晚于池合并会抛异常（BaseLib 源码语义）。
// 加新卡：写卡类 → 这里加一行属性 + Init 一行 → Entry 不动。
// 当前进度：85/85 全量（起始 4 + 普 24 + 罕 35 + 稀 26，含 M2 做局三件、v1.6「风头正盛」8 张、v1.7 扩池 29 张；
// 2026-10-17 删 4 张自掷牌 豹子/老虎机/连庄/连胜，卡池 80→76）。
public static class GamblerCards
{
    // Basic（起始牌：打击/防御 + 两张签名牌）
    public static Basic.StrikeGambler StrikeGambler { get; private set; } = null!;
    public static Basic.DefendGambler DefendGambler { get; private set; } = null!;
    public static Basic.PrivateStash PrivateStash { get; private set; } = null!;
    public static Basic.TryYourLuck TryYourLuck { get; private set; } = null!;

    // Common（普通档 24）
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
    public static Common.HoldBack HoldBack { get; private set; } = null!;
    public static Common.RideTheWin RideTheWin { get; private set; } = null!;
    public static Common.LooseChange LooseChange { get; private set; } = null!;
    public static Common.HeadTax HeadTax { get; private set; } = null!;
    public static Common.TakeCut TakeCut { get; private set; } = null!;
    public static Common.CountMoney CountMoney { get; private set; } = null!;
    public static Common.ShuffleUp ShuffleUp { get; private set; } = null!;
    public static Common.PileOn PileOn { get; private set; } = null!;
    public static Common.BuyOff BuyOff { get; private set; } = null!;
    // v1.8 四轴改造（出千 +5 / 赌命 +4）
    public static Common.HushMoney HushMoney { get; private set; } = null!;
    public static Common.BaitAndSwitch BaitAndSwitch { get; private set; } = null!;
    public static Common.BloodCapital BloodCapital { get; private set; } = null!;
    public static Common.BleedOut BleedOut { get; private set; } = null!;

    // Uncommon（罕见档 35）
    public static Uncommon.AllIn AllIn { get; private set; } = null!;
    public static Uncommon.ConsecutiveBet ConsecutiveBet { get; private set; } = null!;
    public static Uncommon.Ransack Ransack { get; private set; } = null!;
    public static Uncommon.DoubleDown DoubleDown { get; private set; } = null!;
    public static Uncommon.Parlay Parlay { get; private set; } = null!;
    public static Uncommon.Comeback Comeback { get; private set; } = null!;
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
    public static Uncommon.QuitWhileAhead QuitWhileAhead { get; private set; } = null!;
    public static Uncommon.SettleUp SettleUp { get; private set; } = null!;
    public static Uncommon.TheFinale TheFinale { get; private set; } = null!;
    public static Uncommon.MasterBettor MasterBettor { get; private set; } = null!;
    public static Uncommon.Skim Skim { get; private set; } = null!;
    public static Uncommon.DesperateThrow DesperateThrow { get; private set; } = null!;
    public static Uncommon.Backhand Backhand { get; private set; } = null!;
    public static Uncommon.BankWind BankWind { get; private set; } = null!;
    public static Uncommon.TallyUp TallyUp { get; private set; } = null!;
    public static Uncommon.RakeFee RakeFee { get; private set; } = null!;
    public static Uncommon.CollectDebts CollectDebts { get; private set; } = null!;
    public static Uncommon.SinkBoats SinkBoats { get; private set; } = null!;
    public static Uncommon.UnderTheTable UnderTheTable { get; private set; } = null!;
    public static Uncommon.SkipStone SkipStone { get; private set; } = null!;
    public static Uncommon.SwapChips SwapChips { get; private set; } = null!;
    public static Uncommon.CloseTheNet CloseTheNet { get; private set; } = null!;
    public static Uncommon.BettingCard Betting { get; private set; } = null!;

    // Rare（稀有档 26）
    public static Rare.AllOrNothing AllOrNothing { get; private set; } = null!;
    public static Rare.RussianRoulette RussianRoulette { get; private set; } = null!;
    public static Rare.GreedSmash GreedSmash { get; private set; } = null!;
    public static Rare.Flush Flush { get; private set; } = null!;
    public static Rare.Destiny Destiny { get; private set; } = null!;
    public static Rare.HitMan HitMan { get; private set; } = null!;
    public static Rare.TearContract TearContract { get; private set; } = null!;
    public static Rare.Blackmail Blackmail { get; private set; } = null!;
    public static Rare.HoleCard HoleCard { get; private set; } = null!;
    public static Rare.GamblingGodCard GamblingGod { get; private set; } = null!;
    public static Rare.CompoundInterestCard CompoundInterest { get; private set; } = null!;
    public static Rare.AllInToTheEnd AllInToTheEnd { get; private set; } = null!;
    public static Rare.ComebackRound ComebackRound { get; private set; } = null!;
    public static Rare.GoForBroke GoForBroke { get; private set; } = null!;
    public static Rare.ShoveAllIn ShoveAllIn { get; private set; } = null!;
    public static Rare.CheatDeal CheatDeal { get; private set; } = null!;
    public static Rare.PipCount PipCount { get; private set; } = null!;
    public static Rare.TheVault TheVault { get; private set; } = null!;
    public static Rare.AnteUp AnteUp { get; private set; } = null!;
    public static Rare.SweepAll SweepAll { get; private set; } = null!;
    public static Rare.Redouble Redouble { get; private set; } = null!;
    public static Rare.CashChips CashChips { get; private set; } = null!;
    public static Rare.GrandSlam GrandSlam { get; private set; } = null!;
    public static Rare.CheatKingCard CheatKing { get; private set; } = null!;
    public static Rare.SmokeAndMirrors SmokeAndMirrors { get; private set; } = null!;
    public static Rare.StakeYourLife StakeYourLife { get; private set; } = null!;

    public static void Init()
    {
        StrikeGambler = new Basic.StrikeGambler();
        DefendGambler = new Basic.DefendGambler();
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
        HoldBack = new Common.HoldBack();
        RideTheWin = new Common.RideTheWin();
        LooseChange = new Common.LooseChange();
        HeadTax = new Common.HeadTax();
        TakeCut = new Common.TakeCut();
        CountMoney = new Common.CountMoney();
        ShuffleUp = new Common.ShuffleUp();
        PileOn = new Common.PileOn();
        BuyOff = new Common.BuyOff();
        HushMoney = new Common.HushMoney();
        BaitAndSwitch = new Common.BaitAndSwitch();
        BloodCapital = new Common.BloodCapital();
        BleedOut = new Common.BleedOut();

        AllIn = new Uncommon.AllIn();
        ConsecutiveBet = new Uncommon.ConsecutiveBet();
        Ransack = new Uncommon.Ransack();
        DoubleDown = new Uncommon.DoubleDown();
        Parlay = new Uncommon.Parlay();
        Comeback = new Uncommon.Comeback();
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
        QuitWhileAhead = new Uncommon.QuitWhileAhead();
        SettleUp = new Uncommon.SettleUp();
        TheFinale = new Uncommon.TheFinale();
        MasterBettor = new Uncommon.MasterBettor();
        Skim = new Uncommon.Skim();
        DesperateThrow = new Uncommon.DesperateThrow();
        Backhand = new Uncommon.Backhand();
        BankWind = new Uncommon.BankWind();
        TallyUp = new Uncommon.TallyUp();
        RakeFee = new Uncommon.RakeFee();
        CollectDebts = new Uncommon.CollectDebts();
        SinkBoats = new Uncommon.SinkBoats();
        UnderTheTable = new Uncommon.UnderTheTable();
        SkipStone = new Uncommon.SkipStone();
        SwapChips = new Uncommon.SwapChips();
        CloseTheNet = new Uncommon.CloseTheNet();
        Betting = new Uncommon.BettingCard();

        AllOrNothing = new Rare.AllOrNothing();
        RussianRoulette = new Rare.RussianRoulette();
        GreedSmash = new Rare.GreedSmash();
        Flush = new Rare.Flush();
        Destiny = new Rare.Destiny();
        HitMan = new Rare.HitMan();
        TearContract = new Rare.TearContract();
        Blackmail = new Rare.Blackmail();
        HoleCard = new Rare.HoleCard();
        GamblingGod = new Rare.GamblingGodCard();
        CompoundInterest = new Rare.CompoundInterestCard();
        AllInToTheEnd = new Rare.AllInToTheEnd();
        ComebackRound = new Rare.ComebackRound();
        GoForBroke = new Rare.GoForBroke();
        ShoveAllIn = new Rare.ShoveAllIn();
        CheatDeal = new Rare.CheatDeal();
        PipCount = new Rare.PipCount();
        TheVault = new Rare.TheVault();
        AnteUp = new Rare.AnteUp();
        SweepAll = new Rare.SweepAll();
        Redouble = new Rare.Redouble();
        CashChips = new Rare.CashChips();
        GrandSlam = new Rare.GrandSlam();
        CheatKing = new Rare.CheatKingCard();
        SmokeAndMirrors = new Rare.SmokeAndMirrors();
        StakeYourLife = new Rare.StakeYourLife();
    }
}
