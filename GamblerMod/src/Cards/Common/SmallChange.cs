using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using GamblerMod.Core;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace GamblerMod.Cards.Common;

// 普通 #6·筹码（游戏设计 §5.1）：1 能，获得 7 金币（升级 9）——产金基准 1 能量 = 7 金币的样板卡。
public sealed class SmallChange() : GamblerCard(1, CardType.Skill, CardRarity.Common, TargetType.None)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new IntVar("GoldGain", 7)];

    protected override void OnUpgrade() => DynamicVars["GoldGain"].UpgradeValueBy(2);

    protected override Task ApplyEffect(PlayerChoiceContext ctx, CardPlay play, bool heads)
        => PlayerCmd.GainGold(DynamicVars["GoldGain"].BaseValue, Owner, true);

    public override List<(string, string)>? Localization => new CardLoc(
        "筹码",
        "#获得 *!GoldGain! 金币*。");
}
