using System.Collections.Generic;
using BaseLib.Abstracts;
using GamblerMod.Cards;
using GamblerMod.Relics;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Characters;

namespace GamblerMod.Character;

// M0 占位角色（技术设计 D2）：PlaceholderCharacterModel 只借原版战士的资产路径（场景/贴图）。
// 数值按游戏设计 v1.3 + 角色设定：HP 72、起始 148 金（本该 150，兑换筹码时抽走两枚"手续费"）。
// 卡池 / 起始牌 / 起始遗物 / 药水池暂借战士（M1 起换成本职业内容）。
// 注意：原版模型是 ModelDb 单例，禁止 new（DuplicateModelException），
// 一律用 ModelDb.GetById + GetId 运行时查库；不做静态字段初始化，避免 cctor 抛异常连累类型加载。
public sealed class GamblerCharacter : PlaceholderCharacterModel
{
    private static CharacterModel Source => ModelDb.GetById<CharacterModel>(ModelDb.GetId<Ironclad>());

    // 必须小写：Godot res:// 路径区分大小写，资产文件名为 ironclad_*
    // （写成 "Ironclad" 会导致选人背景/转场材质加载失败，选人界面状态切换中断）
    public override string PlaceholderID => "ironclad";

    // 选人封面图（用户提供，assets 工程导出的 pck 提供，486×720 竖版裁剪）
    public override string? CustomCharacterSelectIconPath =>
        "res://GamblerMod/images/charui/char_select_gambler.png";

    // 选人大图/背景（同一张图的完整 1280×720，TextureRect 全屏场景，保持比例铺满）
    public override string? CustomCharacterSelectBg =>
        "res://GamblerMod/scenes/screens/char_select/char_select_bg_gambler.tscn";

    public override int StartingHp => 72;

    public override int StartingGold => 148;

    public override CharacterGender Gender => Source.Gender;

    // 名字色：暗金（角色设定 §4 色彩基调，与卡背/牌组图标同色系）
    public override Godot.Color NameColor => new("e8c34a");

    // 起始套牌（游戏设计 §3）：4 打击 + 4 防御（借原版）+ 本钱 + 试手气（本职业签名）。
    // M1 验收已完成，底注移出起始牌（保留在卡池作为奖励卡）。
    public override IEnumerable<CardModel> StartingDeck =>
    [
        ModelDb.Card<StrikeIronclad>(),
        ModelDb.Card<StrikeIronclad>(),
        ModelDb.Card<StrikeIronclad>(),
        ModelDb.Card<StrikeIronclad>(),
        ModelDb.Card<DefendIronclad>(),
        ModelDb.Card<DefendIronclad>(),
        ModelDb.Card<DefendIronclad>(),
        ModelDb.Card<DefendIronclad>(),
        GamblerCards.PrivateStash,
        GamblerCards.TryYourLuck,
    ];

    // 初始遗物（游戏设计 §4，M2）：幸运筹码——"钱能买运气"的核心表达
    public override IReadOnlyList<RelicModel> StartingRelics => [GamblerRelics.LuckyChip];


    // 卡池必须是自己的 GamblerCardPool：BaseLib 规定"卡池必须被角色引用，否则不会被找到"，
    // 引用他人池会导致本池成孤儿池、卡牌实例化抛 InvalidProgramException（M1 踩过）。
    // 后果：M1 期间卡牌奖励较薄（池里只有 3 张），M3 分批入池后正常。
    public override CardPoolModel CardPool => ModelDb.CardPool<GamblerCardPool>();

    // 遗物池同卡池逻辑：必须引用自有池（LuckyChip 的 [Pool] 指向它），
    // 借战士池会让自有遗物池成孤儿、遗物注册在初始化期直接抛异常（M4 踩过）。
    public override RelicPoolModel RelicPool => ModelDb.RelicPool<GamblerRelicPool>();

    public override PotionPoolModel PotionPool => Source.PotionPool;

    public override List<(string, string)> Localization => new CharacterLoc(
        Title: "赌徒",
        TitleObject: "赌徒",
        Description: "他带着一袋金币和一枚幸运筹码，上塔去找一位欠他运气的先古之民。",
        PronounObject: "他",
        PronounSubject: "他",
        PronounPossessive: "他的",
        PossessiveAdjective: "他的",
        AromaPrinciple: "贪婪，但守规矩。",
        EndTurnPingAlive: "轮到我了？——下注吧。",
        EndTurnPingDead: "……这一手，先欠着。",
        EventDeathPrevention: "还轮不到我离场。",
        GoldMonologue: "叮——世上最好听的声音。",
        CardsModifierTitle: "",
        CardsModifierDescription: "",
        ExtraLoc: []);
}
