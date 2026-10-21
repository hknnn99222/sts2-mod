using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using GamblerMod.Core;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace GamblerMod.Cards.Uncommon;

// 罕见 #24·抽头（游戏设计 §5.2，v1.12 上调）：1 能，获得 10 金币；赌博：正面 → 20（升级 12 / 24）。
public sealed class Rake : GamblerCard
{
    public Rake() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
        IsGambleCard = true;
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new IntVar("GoldGain", 10),
        new IntVar("HeadsGold", 20),
    ];

    protected override void OnUpgrade()
    {
        DynamicVars["GoldGain"].UpgradeValueBy(2);
        DynamicVars["HeadsGold"].UpgradeValueBy(4); // 20→24
    }

    protected override Task ApplyEffect(PlayerChoiceContext ctx, CardPlay play, bool heads)
        => PlayerCmd.GainGold(heads ? DynamicVars["HeadsGold"].BaseValue : DynamicVars["GoldGain"].BaseValue, Owner, false);

    public override List<(string, string)>? Localization => new CardLoc(
        "抽头",
        "#硬币为正时：获得 !HeadsGold! 金币；硬币为负时：获得 !GoldGain! 金币。");
}
