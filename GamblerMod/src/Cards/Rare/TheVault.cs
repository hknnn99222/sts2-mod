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

// 稀有 #75·保本（游戏设计 §5.3 / §5.5，v1.15 改型：技能 → 能力）：2 能，
// 本场战斗中你每花费 1 金币，获得 1 点敏捷；升级版再加 1 点力量。
// 钩子：花费追踪·转敏捷——触发点在 Core/SpendTracker.Record（层数 1 = 升级档）。
public sealed class TheVault : GamblerCard
{
    public TheVault() : base(2, CardType.Power, CardRarity.Rare, TargetType.None) { }

    protected override void OnUpgrade() { /* 升级档由 Apply 的 amount 表达 */ }

    protected override Task ApplyEffect(PlayerChoiceContext ctx, CardPlay play, bool heads)
        => PowerCmd.Apply<Powers.CapitalGuardPower>(ctx, Owner.Creature, IsUpgraded ? 1 : 0, Owner.Creature, this);

    public override List<(string, string)>? Localization => new CardLoc(
        "保本",
        "#本场战斗中，你每花费 1 金币，获得 1 点敏捷。");
}
