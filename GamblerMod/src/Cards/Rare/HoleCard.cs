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

// 稀有 #41·底牌（游戏设计 §5.3）：2 能，抽 2 张牌。若本回合掷出过正面，这些牌本回合费用 -1（升级 抽 3）。
// M4 简化：临时降费 UI 管线未接，改为"掷出过正面 → 多抽 1 张"（偏离已记入测试文档，v2 接费用修改器后还原）。
public sealed class HoleCard() : GamblerCard(2, CardType.Skill, CardRarity.Rare, TargetType.None)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new IntVar("Cards", 2)];

    protected override void OnUpgrade() => DynamicVars["Cards"].UpgradeValueBy(1);

    protected override async Task ApplyEffect(PlayerChoiceContext ctx, CardPlay play, bool heads)
    {
        var count = (int)DynamicVars["Cards"].BaseValue;
        if (GambleService.HeadsCountThisTurn > 0) count++;
        await CardPileCmd.Draw(ctx, count, Owner);
    }

    public override List<(string, string)>? Localization => new CardLoc(
        "底牌",
        "#抽 !Cards! 张牌。若本回合掷出过*正面*，额外抽 1 张牌。");
}
