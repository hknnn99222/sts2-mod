using System.Collections.Generic;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
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

    public override IEnumerable<CardModel> StartingDeck => Source.StartingDeck;

    public override IReadOnlyList<RelicModel> StartingRelics => Source.StartingRelics;

    public override CardPoolModel CardPool => Source.CardPool;

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
