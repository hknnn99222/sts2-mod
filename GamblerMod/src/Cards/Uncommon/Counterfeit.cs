using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using GamblerMod.Core;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace GamblerMod.Cards.Uncommon;

// 罕见 #26·假币（游戏设计 §5.2）：1 能，你的下一次金币花费不消耗金币（升级：本场战斗前 2 次）。
// 免单走 GoldCost 的免单层数：免掉的支出不计入花费记账（SpendTracker）。
public sealed class Counterfeit() : GamblerCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.None)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new IntVar("Times", 1)];

    protected override void OnUpgrade() => DynamicVars["Times"].UpgradeValueBy(1);

    protected override Task ApplyEffect(PlayerChoiceContext ctx, CardPlay play, bool heads)
    {
        GoldCost.GrantFreeSpending((int)DynamicVars["Times"].BaseValue);
        return Task.CompletedTask;
    }

    public override List<(string, string)>? Localization => new CardLoc(
        "假币",
        "#你的接下来 !Times! 次*金币花费*不消耗金币。");
}
