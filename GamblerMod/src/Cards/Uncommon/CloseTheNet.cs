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

namespace GamblerMod.Cards.Uncommon;

// 罕见 #85·收网（v1.8 四轴·出千轴兑现器，v1.12 改版）：1 能，每 1 次必正造成 9 伤害，
// **不消耗**必正（升级每次 12）。钩子由「必正兑现」改为「出千·必正联动」——
// 从一次性清空资源，变成"你有多少确定性，这张牌就有多少伤害"的持续输出。
public sealed class CloseTheNet() : GamblerCard(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new IntVar("PerForce", 9)];

    protected override void OnUpgrade() => DynamicVars["PerForce"].UpgradeValueBy(3);

    protected override async Task ApplyEffect(PlayerChoiceContext ctx, CardPlay play, bool heads)
    {
        var times = GambleService.ForcedHeadsRemaining;
        if (times <= 0) return;

        for (var i = 0; i < times; i++)
        {
            await DamageCmd.Attack(DynamicVars["PerForce"].BaseValue)
                .FromCard(this, play)
                .Targeting(play.Target)
                .WithHitFx(HeadsHitFx, null, HeadsHitSfx)
                .Execute(ctx);
        }
    }

    public override List<(string, string)>? Localization => new CardLoc(
        "收网",
        "#你的每 1 次必正造成 !PerForce! 伤害（不消耗必正；没有必正时无事发生）。");
}
