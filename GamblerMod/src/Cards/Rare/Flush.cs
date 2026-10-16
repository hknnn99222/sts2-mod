using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using GamblerMod.Core;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace GamblerMod.Cards.Rare;

// 稀有 #35·同花顺（游戏设计 §5.3 / §2.8）：3 能 + 12 金，造成 43 伤害 ×风头倍率（升级 46）。
// v1.6 改造：挂「风头正盛」词条——原"本回合 ≥3 次正面 → 64 伤"的**条件删除**（Q25：倍率本身
// 已是对连胜的奖励，不再叠一层），HotDamage/HeadsNeeded 两个变量随之移除。
public sealed class Flush : GamblerCard
{
    public Flush() : base(3, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
    {
        BaseGoldCost = 12;
        IsHeadsStreakCard = true;
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(43, ValueProp.Move)];

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(3);

    protected override Task ApplyEffect(PlayerChoiceContext ctx, CardPlay play, bool heads)
        => DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, play)
            .Targeting(play.Target)
            .WithHitFx(HeadsHitFx, null, HeadsHitSfx)
            .Execute(ctx);

    public override List<(string, string)>? Localization => new CardLoc(
        "同花顺",
        "#花费 *12 金币*。\n造成 !D! 伤害。数值随头顶倍率放大，结算后清空。");
}
