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

// 稀有 #78·倍投（游戏设计 §5.3 / §5.5）：2 能，你的**倍率翻倍**（升级 费用变为 1）。
// 钩子：做局·倍率翻倍（指数型做局：倍率越高收益越大）。
public sealed class Redouble : GamblerCard
{
    public Redouble() : base(2, CardType.Skill, CardRarity.Rare, TargetType.None) { }

    protected override IEnumerable<DynamicVar> CanonicalVars => [new IntVar("Dummy", 0)];

    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);

    protected override Task ApplyEffect(PlayerChoiceContext ctx, CardPlay play, bool heads)
    {
        // 当前刻度先取快照，再逐段推进（循环里 Steps 会变，不能用它当上界）
        var add = MultiplierService.Steps;
        for (var i = 0; i < add; i++) MultiplierService.AddHeads();
        return Task.CompletedTask;
    }

    public override List<(string, string)>? Localization => new CardLoc(
        "倍投",
        "#你的*风头*倍率**翻倍**（不清空）。");
}
