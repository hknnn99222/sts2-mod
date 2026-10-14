namespace GamblerMod.Cards;

// 卡牌单例仓：所有卡实例必须在 Entry.Initialize（池合并前）创建——
// CustomCardModel 构造器自动注册进池/ModelDb，晚于池合并会抛异常（BaseLib 源码语义）。
// 加新卡：写卡类 → 这里加一行属性 + Init 一行 → Entry 不动。
public static class GamblerCards
{
    public static Basic.PrivateStash PrivateStash { get; private set; } = null!;
    public static Basic.TryYourLuck TryYourLuck { get; private set; } = null!;
    public static Common.FlyingCard FlyingCard { get; private set; } = null!;

    public static void Init()
    {
        PrivateStash = new Basic.PrivateStash();
        TryYourLuck = new Basic.TryYourLuck();
        FlyingCard = new Common.FlyingCard();
    }
}
