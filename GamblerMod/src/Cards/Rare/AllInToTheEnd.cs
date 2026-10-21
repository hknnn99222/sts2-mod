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

// 稀有 #49·晒冷（游戏设计 §5.3 / §5.5，v1.15 改型：攻击 → 能力）：3 能，
// 本场战斗中你每次获得金币，就获得等量格挡（升级：额外 +5 格挡）。
// 钩子：产金转甲——把金币流水的每一笔都变成防线（层数 = 额外格挡，逻辑在 ShowdownPower）。
public sealed class AllInToTheEnd : GamblerCard
{
    public AllInToTheEnd() : base(3, CardType.Power, CardRarity.Rare, TargetType.None) { }

    protected override void OnUpgrade() { /* 升级档由 Apply 的 amount 表达 */ }

    protected override Task ApplyEffect(PlayerChoiceContext ctx, CardPlay play, bool heads)
        => PowerCmd.Apply<Powers.ShowdownPower>(ctx, Owner.Creature, IsUpgraded ? 5 : 0, Owner.Creature, this);

    public override List<(string, string)>? Localization => new CardLoc(
        "晒冷",
        "#本场战斗中，你每次获得金币时，获得等量格挡。");
}
