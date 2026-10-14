namespace GamblerMod.Cards;

// 卡牌单例仓：所有卡实例必须在 Entry.Initialize（池合并前）创建——
// CustomCardModel 构造器自动注册进池/ModelDb，晚于池合并会抛异常（BaseLib 源码语义）。
// 加新卡：写卡类 → 这里加一行属性 + Init 一行 → Entry 不动。
public static class GamblerCards
{
    // Basic（起始签名牌）
    public static Basic.PrivateStash PrivateStash { get; private set; } = null!;
    public static Basic.TryYourLuck TryYourLuck { get; private set; } = null!;

    // Common（M3：普通档 11 张）
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

    // Uncommon（M2：做局工具）
    public static Uncommon.SecretMark SecretMark { get; private set; } = null!;
    public static Uncommon.HotStreakCard HotStreak { get; private set; } = null!;

    // Rare（M2：终局做局）
    public static Rare.GamblingGodCard GamblingGod { get; private set; } = null!;

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

        SecretMark = new Uncommon.SecretMark();
        HotStreak = new Uncommon.HotStreakCard();

        GamblingGod = new Rare.GamblingGodCard();
    }
}
