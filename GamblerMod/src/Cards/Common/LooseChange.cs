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

// 普通 #52·捡漏（游戏设计 §5.1 / §5.5，v1.10 改版）：1 能，每 2 张弃牌堆的牌获得 1 金币（不设上限）。
// 升级改为额外抽 1 张（v1.10：去掉封顶，让"越打越堆"的弃牌堆真正成为收益来源）。
// 钩子：即时产金·弃牌堆（按弃牌堆张数产金）。
public sealed class LooseChange : GamblerCard
{
    public LooseChange() : base(1, CardType.Skill, CardRarity.Common, TargetType.None) { }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new IntVar("Step", 2), // 每 Step 张弃牌堆的牌 = 1 金币
        new IntVar("Draw", 0), // 升级后额外抽 1 张
    ];

    protected override void OnUpgrade() => DynamicVars["Draw"].UpgradeValueBy(1);

    protected override async Task ApplyEffect(PlayerChoiceContext ctx, CardPlay play, bool heads)
    {
        var step = (int)DynamicVars["Step"].BaseValue;
        var discarded = PileType.Discard.GetPile(Owner).Cards.Count;
        var gold = discarded / step;
        if (gold > 0) await PlayerCmd.GainGold(gold, Owner, false);

        var draw = (int)DynamicVars["Draw"].BaseValue;
        if (draw > 0) await CardPileCmd.Draw(ctx, draw, Owner);
    }

    public override List<(string, string)>? Localization => new CardLoc(
        "捡漏",
        "#每 *!Step!* 张*弃牌堆*的牌，获得 1 金币。（升级：额外抽 1 张）");
}
