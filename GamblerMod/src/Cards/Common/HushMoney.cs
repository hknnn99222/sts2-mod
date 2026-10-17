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

// 普通 #81·封口（v1.8 四轴改造·出千轴生成器）：1 能，获得 1 次必正并抽 1 张（升级 2 次）。
// 出千轴原来只有"把赌博变确定"的辅助，没有自己的资源池与兑现出口；
// 封口负责堆资源，收网（罕见 #85）负责一次性花掉 → 这条轴才能脱离赌博牌独立成型。
public sealed class HushMoney() : GamblerCard(1, CardType.Skill, CardRarity.Common, TargetType.None)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new IntVar("Force", 1)];

    protected override void OnUpgrade() => DynamicVars["Force"].UpgradeValueBy(1);

    protected override async Task ApplyEffect(PlayerChoiceContext ctx, CardPlay play, bool heads)
    {
        GambleService.ForceNextFlips((int)DynamicVars["Force"].BaseValue);
        await CardPileCmd.Draw(ctx, 1, Owner);
    }

    public override List<(string, string)>? Localization => new CardLoc(
        "封口",
        "#获得 !Force! 次必正，抽 1 张牌。");
}
