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

namespace GamblerMod.Cards.Common;

// 普通 #82·偷天换日（v1.8 四轴改造·出千轴）：1 能赌博攻击，5 伤；若本次为正面，获得 1 次必正（升级 10 伤）。
// 设计意图：把"偶然的运气"固化成"确定性资源"——出千轴的循环入口（正面 → 必正 → 收网）。
public sealed class BaitAndSwitch : GamblerCard
{
    public BaitAndSwitch() : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
    {
        IsGambleCard = true;
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(5, ValueProp.Move), new IntVar("Force", 1)];

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(3);

    protected override async Task ApplyEffect(PlayerChoiceContext ctx, CardPlay play, bool heads)
    {
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, play)
            .Targeting(play.Target)
            .WithHitFx(heads ? HeadsHitFx : TailsHitFx, null, heads ? HeadsHitSfx : null)
            .Execute(ctx);

        if (heads) GambleService.ForceNextFlips((int)DynamicVars["Force"].BaseValue);
    }

    public override List<(string, string)>? Localization => new CardLoc(
        "偷天换日",
        "#硬币为正时：造成 !D! 伤害，并获得 !Force! 次必正；硬币为负时：造成 !D! 伤害。");
}
