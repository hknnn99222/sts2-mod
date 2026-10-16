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

// 稀有 #41·底牌（游戏设计 §5.3）：2 能，抽 2 张牌；若**风头倍率 ≥1.5**，额外抽 1 张（升级 抽 3）。
// v1.6 改造：条件从"本回合掷出过正面"改为"倍率 ≥1.5"——只读条件，不乘倍率、不清空（Q20 分开处理）。
// 原设计"抽到的牌本回合费用 -1"就此作废（费用修改器不再需要接入）。
public sealed class HoleCard() : GamblerCard(2, CardType.Skill, CardRarity.Rare, TargetType.None)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new IntVar("Cards", 2)];

    protected override void OnUpgrade() => DynamicVars["Cards"].UpgradeValueBy(1);

    protected override async Task ApplyEffect(PlayerChoiceContext ctx, CardPlay play, bool heads)
    {
        var count = (int)DynamicVars["Cards"].BaseValue;
        if (MultiplierService.Factor >= 1.5m) count++;
        await CardPileCmd.Draw(ctx, count, Owner);
    }

    public override List<(string, string)>? Localization => new CardLoc(
        "底牌",
        "#抽 !Cards! 张牌。若风头正盛倍率 ≥1.5，额外抽 1 张牌。");
}
