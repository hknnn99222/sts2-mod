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

// 稀有 #49·晒冷（游戏设计 §5.3 / §2.8）：3 能 + 15 金，造成 50 伤害 ×风头倍率（升级 62）。
// 全池最高费的兑现大炮：倍率越高越爆炸（不设上限，设计 §2.8 已记录经济/战斗风险）。
public sealed class AllInToTheEnd : GamblerCard
{
    public AllInToTheEnd() : base(3, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
    {
        BaseGoldCost = 15;
        IsHeadsStreakCard = true;
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(50, ValueProp.Move)];

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(12);

    protected override Task ApplyEffect(PlayerChoiceContext ctx, CardPlay play, bool heads)
        => DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, play)
            .Targeting(play.Target)
            .WithHitFx("vfx/vfx_rock_shatter", null, HeadsHitSfx)
            .Execute(ctx);

    public override List<(string, string)>? Localization => new CardLoc(
        "晒冷",
        "#花费 *15 金币*。\n造成 !D! 伤害。数值随头顶倍率放大，结算后清空。");
}
