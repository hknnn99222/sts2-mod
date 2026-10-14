using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using GamblerMod.Core;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace GamblerMod.Cards.Uncommon;

// 罕见 #22·数牌（游戏设计 §5.2）：0 能，查看抽牌堆顶 3 张，可将其中任意张置入弃牌堆（升级顶 4）。
// M4 简化：从顶 N 张中"选择 1 张"置入弃牌堆（无 0-N 多选 UI；偏离已记入测试文档）。
public sealed class CountCards() : GamblerCard(0, CardType.Skill, CardRarity.Uncommon, TargetType.None)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new IntVar("Peek", 3)];

    protected override void OnUpgrade() => DynamicVars["Peek"].UpgradeValueBy(1);

    protected override async Task ApplyEffect(PlayerChoiceContext ctx, CardPlay play, bool heads)
    {
        var drawPile = PileType.Draw.GetPile(Owner).Cards;
        var top = drawPile.Take((int)DynamicVars["Peek"].BaseValue).ToList();
        if (top.Count == 0) return;

        var selected = await CardSelectCmd.FromChooseACardScreen(ctx, top, Owner, true);
        if (selected != null)
            await CardCmd.Discard(ctx, selected);
    }

    public override List<(string, string)>? Localization => new CardLoc(
        "数牌",
        "#查看抽牌堆顶 !Peek! 张，将其中 1 张置入弃牌堆。");
}
