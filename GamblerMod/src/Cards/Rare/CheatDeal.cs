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

// 稀有 #73·出千（游戏设计 §5.3 / §5.5，v1.15 降费）：1 能，本回合你接下来的所有赌博必为正面（升级 费用变为 0）。
// 钩子：做局·本回合全正（回合层必正原语，一次性给足 20 层，回合开始清零——与暗记的持久层数分开记账）。
public sealed class CheatDeal : GamblerCard
{
    private const int GuaranteedFlips = 20;

    public CheatDeal() : base(1, CardType.Skill, CardRarity.Rare, TargetType.None) { }

    protected override IEnumerable<DynamicVar> CanonicalVars => [new IntVar("Times", GuaranteedFlips)];

    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);

    protected override Task ApplyEffect(PlayerChoiceContext ctx, CardPlay play, bool heads)
    {
        GambleService.ForceNextFlipsThisTurn((int)DynamicVars["Times"].BaseValue);
        return Task.CompletedTask;
    }

    public override List<(string, string)>? Localization => new CardLoc(
        "出千",
        "#你接下来的所有赌博必为正面（本回合）。");
}
