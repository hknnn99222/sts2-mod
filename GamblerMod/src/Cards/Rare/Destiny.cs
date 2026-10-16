using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using GamblerMod.Core;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace GamblerMod.Cards.Rare;

// 稀有 #36·单双（游戏设计 §5.3 真赌）：1 能，赌博：正面 → 获得 4 点能量；反面 → 失去 1 点能量（升级正面 5）。
public sealed class Destiny : GamblerCard
{
    public Destiny() : base(1, CardType.Skill, CardRarity.Rare, TargetType.None)
    {
        IsGambleCard = true;
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new IntVar("HeadsEnergy", 4),
        new IntVar("TailsEnergy", -1),
    ];

    protected override void OnUpgrade() => DynamicVars["HeadsEnergy"].UpgradeValueBy(1);

    protected override Task ApplyEffect(PlayerChoiceContext ctx, CardPlay play, bool heads)
        => PlayerCmd.GainEnergy(heads ? DynamicVars["HeadsEnergy"].BaseValue : DynamicVars["TailsEnergy"].BaseValue, Owner);

    public override List<(string, string)>? Localization => new CardLoc(
        "单双",
        "#硬币为正时：获得 !HeadsEnergy! 点能量；硬币为负时：失去 1 点能量。");
}
