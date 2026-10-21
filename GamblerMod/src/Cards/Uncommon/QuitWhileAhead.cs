using System;
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

// 罕见 #46·见好就收（游戏设计 §5.2 / §2.8）：0 能，每 0.5 倍率抽 1 张牌（至多 3；升级至多 4）。
// 唯一把倍率**当货币**的兑现牌：不走乘法钩子，直接读倍率刻度换算抽牌数（x1.0 → 2 张，x1.5+ → 上限）。
public sealed class QuitWhileAhead : GamblerCard
{
    public QuitWhileAhead() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
        IsHeadsStreakCard = true;
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [new IntVar("Cap", 3)];

    protected override void OnUpgrade() => DynamicVars["Cap"].UpgradeValueBy(1);

    protected override Task ApplyEffect(PlayerChoiceContext ctx, CardPlay play, bool heads)
    {
        var draws = Math.Min((int)DynamicVars["Cap"].BaseValue, MultiplierService.Steps);
        return CardPileCmd.Draw(ctx, draws, Owner);
    }

    public override List<(string, string)>? Localization => new CardLoc(
        "见好就收",
        "#每 0.5 倍率抽 1 张牌（至多 !Cap! 张）。结算后清空倍率。");
}
