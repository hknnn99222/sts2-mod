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

// 普通 #81·发牌（v1.8 四轴改造·出千轴生成器，v1.10 改版）：1 能，抽 2 张牌；升级后额外获得 2 次必正。
// 出千轴原来只有"把赌博变确定"的辅助，没有自己的资源池与兑现出口；
// 发牌负责堆资源，收网（罕见 #85）负责一次性花掉 → 这条轴才能脱离赌博牌独立成型。
// v1.10：基础只抽牌（0 次必正不起作用），升级把"生成必正"这一半加上——升级前后是两个不同定位。
public sealed class HushMoney() : GamblerCard(1, CardType.Skill, CardRarity.Common, TargetType.None)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new IntVar("Cards", 2),
        new IntVar("Force", 0),
    ];

    protected override void OnUpgrade() => DynamicVars["Force"].UpgradeValueBy(2);

    protected override async Task ApplyEffect(PlayerChoiceContext ctx, CardPlay play, bool heads)
    {
        var force = (int)DynamicVars["Force"].BaseValue;
        if (force > 0) GambleService.ForceNextFlips(force);
        await CardPileCmd.Draw(ctx, (int)DynamicVars["Cards"].BaseValue, Owner);
    }

    public override List<(string, string)>? Localization => new CardLoc(
        "发牌",
        "#抽 !Cards! 张牌。（升级：额外获得 2 次必正）");
}
