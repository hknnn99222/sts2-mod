using System.Collections.Generic;
using BaseLib.Abstracts;
using GamblerMod.Cards;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Characters;

namespace GamblerMod.Character;

// M0 占位角色（技术设计 D2）：PlaceholderCharacterModel 只借原版战士的资产路径（场景/贴图）。
// 数值按游戏设计 v1.3：HP 72、起始 99 金。
// 卡池 / 起始牌 / 起始遗物 / 药水池暂借战士（M1 起换成本职业内容）。
// 注意：原版模型是 ModelDb 单例，禁止 new（DuplicateModelException），
// 一律用 ModelDb.GetById + GetId 运行时查库；不做静态字段初始化，避免 cctor 抛异常连累类型加载。
public sealed class GamblerCharacter : PlaceholderCharacterModel
{
    private static CharacterModel Source => ModelDb.GetById<CharacterModel>(ModelDb.GetId<Ironclad>());

    // 必须小写：Godot res:// 路径区分大小写，资产文件名为 ironclad_*
    // （写成 "Ironclad" 会导致选人背景/转场材质加载失败，选人界面状态切换中断）
    public override string PlaceholderID => "ironclad";

    public override int StartingHp => 72;

    public override int StartingGold => 99;

    public override CharacterGender Gender => Source.Gender;

    public override Godot.Color NameColor => Source.NameColor;

    // 起始套牌（游戏设计 §3）：4 打击 + 4 防御（借原版）+ 私房钱 + 试手气（本职业签名）。
    // 飞牌为 M1 验收用临时牌（TODO：M1 验收后移除）。
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
        GamblerCards.FlyingCard,
    ];

    public override IReadOnlyList<RelicModel> StartingRelics => Source.StartingRelics;

    // 卡池必须是自己的 GamblerCardPool：BaseLib 规定"卡池必须被角色引用，否则不会被找到"，
    // 引用他人池会导致本池成孤儿池、卡牌实例化抛 InvalidProgramException（M1 踩过）。
    // 后果：M1 期间卡牌奖励较薄（池里只有 3 张），M3 分批入池后正常。
    public override CardPoolModel CardPool => ModelDb.CardPool<GamblerCardPool>();

    public override RelicPoolModel RelicPool => Source.RelicPool;

    public override PotionPoolModel PotionPool => Source.PotionPool;

    public override List<(string, string)> Localization => new CharacterLoc(
        Title: "赌徒",
        TitleObject: "赌徒",
        Description: "钱能买运气。",
        PronounObject: "他",
        PronounSubject: "他",
        PronounPossessive: "他的",
        PossessiveAdjective: "他的",
        AromaPrinciple: "",
        EndTurnPingAlive: "",
        EndTurnPingDead: "",
        EventDeathPrevention: "",
        GoldMonologue: "",
        CardsModifierTitle: "",
        CardsModifierDescription: "",
        ExtraLoc: []);
}
