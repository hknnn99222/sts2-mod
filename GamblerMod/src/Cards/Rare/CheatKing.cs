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

// 稀有 #87·千王（v1.8 四轴改造·出千轴生成器）：2 能能力，每回合前 1 次掷出正面 → 1 次必正（升级前 2 次）。
// 每回合封顶（power 内部计数），防止"必正→正面→再给必正"的无限循环。
public sealed class CheatKingCard() : GamblerCard(2, CardType.Power, CardRarity.Rare, TargetType.None)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new IntVar("Uses", 1)];

    protected override void OnUpgrade() => DynamicVars["Uses"].UpgradeValueBy(1);

    protected override Task ApplyEffect(PlayerChoiceContext ctx, CardPlay play, bool heads)
        => PowerCmd.Apply<Powers.CheatKingPower>(ctx, Owner.Creature, DynamicVars["Uses"].BaseValue, Owner.Creature, this);

    public override List<(string, string)>? Localization => new CardLoc(
        "千王",
        "#每回合前 !Uses! 次掷出正面时，获得 1 次必正。");
}
