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

// 罕见 #24·抽头（游戏设计 §5.2 / §5.4）：1 能，获得 6 金币；赌博：正面 → 11（升级 8 / 13）。
public sealed class Rake : GamblerCard
{
    public Rake() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
        IsGambleCard = true;
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new IntVar("GoldGain", 6),
        new IntVar("HeadsGold", 11),
    ];

    protected override void OnUpgrade()
    {
        DynamicVars["GoldGain"].UpgradeValueBy(2);
        DynamicVars["HeadsGold"].UpgradeValueBy(2);
    }

    protected override Task ApplyEffect(PlayerChoiceContext ctx, CardPlay play, bool heads)
        => PlayerCmd.GainGold(heads ? DynamicVars["HeadsGold"].BaseValue : DynamicVars["GoldGain"].BaseValue, Owner, true);

    public override List<(string, string)>? Localization => new CardLoc(
        "抽头",
        "#获得 !GoldGain! 金币。*赌博*：正面 → 改为获得 !HeadsGold! 金币。");
}
