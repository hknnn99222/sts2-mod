using System.Collections.Generic;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Characters;

namespace GamblerMod.Character;

// M0 占位角色（技术设计 D2）：PlaceholderCharacterModel 复用原版战士全套视觉资产。
// 数值按游戏设计 v1.3：HP 72、起始 99 金。
// 卡池 / 起始牌 / 起始遗物暂借战士（M1 起换成本职业内容），占位源为静态单例（分配红线 R2/R5）。
public sealed class GamblerCharacter : PlaceholderCharacterModel
{
    private static readonly Ironclad PlaceholderSource = new();

    public override string PlaceholderID => "Ironclad";

    public override int StartingHp => 72;

    public override int StartingGold => 99;

    public override CharacterGender Gender => PlaceholderSource.Gender;

    public override Godot.Color NameColor => PlaceholderSource.NameColor;

    public override IEnumerable<CardModel> StartingDeck => PlaceholderSource.StartingDeck;

    public override IReadOnlyList<RelicModel> StartingRelics => PlaceholderSource.StartingRelics;

    public override CardPoolModel CardPool => PlaceholderSource.CardPool;

    public override RelicPoolModel RelicPool => PlaceholderSource.RelicPool;

    public override PotionPoolModel PotionPool => PlaceholderSource.PotionPool;

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
