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

// 稀有 #32·俄罗斯轮盘（游戏设计 §5.3，v1.15 改型：攻击 → 能力）：2 能，
// 每回合开始随机开一枪——自己（受 3 伤 + 得 5 金）或随机敌人（10 伤）。
// 升级档：自己 2 伤 + 7 金 / 敌人 14 伤（层数 1 表示升级档，逻辑在 RoulettePower）。
public sealed class RussianRoulette : GamblerCard
{
    public RussianRoulette() : base(2, CardType.Power, CardRarity.Rare, TargetType.None) { }

    protected override void OnUpgrade() { /* 升级档由 Apply 的 amount 表达，无数值变量 */ }

    protected override Task ApplyEffect(PlayerChoiceContext ctx, CardPlay play, bool heads)
        => PowerCmd.Apply<Powers.RoulettePower>(ctx, Owner.Creature, IsUpgraded ? 1 : 0, Owner.Creature, this);

    public override List<(string, string)>? Localization => new CardLoc(
        "俄罗斯轮盘",
        "#每回合开始随机开一枪：自己受到 3 点伤害并获得 5 金币，或对随机敌人造成 10 点伤害。");
}
