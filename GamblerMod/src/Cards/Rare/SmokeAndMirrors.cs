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

// 稀有 #88·瞒天过海（v1.8 四轴改造·出千轴）：1能+6金，本回合接下来 3 次赌博必为正（升级 4）。
// 与发牌（持久层）区分：瞒天过海只保本回合（ForceNextFlipsThisTurn），回合开始清零。
public sealed class SmokeAndMirrors : GamblerCard
{
    public SmokeAndMirrors() : base(1, CardType.Skill, CardRarity.Rare, TargetType.None)
    {
        BaseGoldCost = 6;
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [new IntVar("Flips", 3)];

    protected override void OnUpgrade() => DynamicVars["Flips"].UpgradeValueBy(1);

    protected override Task ApplyEffect(PlayerChoiceContext ctx, CardPlay play, bool heads)
    {
        GambleService.ForceNextFlipsThisTurn((int)DynamicVars["Flips"].BaseValue);
        return Task.CompletedTask;
    }

    public override List<(string, string)>? Localization => new CardLoc(
        "瞒天过海",
        "#花费 *6 金币*。\n本回合你接下来的 !Flips! 次赌博必为正面。");
}
