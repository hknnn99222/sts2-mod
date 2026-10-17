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

// 罕见 #85·收网（v1.8 四轴改造·出千轴兑现器）：1 能，消耗你**所有**必正次数，每 1 次造成 9 伤害（升级 12）。
// 出千轴的核心兑现：把"确定性"这个资源一次性换成伤害——这条轴因此不再依赖赌博牌本身。
// 必正层数由 GambleService 持有（暗记的持久层 + 出千/瞒天过海的回合层合并计算）。
public sealed class CloseTheNet() : GamblerCard(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new IntVar("PerForce", 9)];

    protected override void OnUpgrade() => DynamicVars["PerForce"].UpgradeValueBy(3);

    protected override async Task ApplyEffect(PlayerChoiceContext ctx, CardPlay play, bool heads)
    {
        var times = GambleService.ConsumeForcedHeads();
        if (times <= 0) return;

        await DamageCmd.Attack(DynamicVars["PerForce"].BaseValue * times)
            .FromCard(this, play)
            .Targeting(play.Target)
            .WithHitFx(HeadsHitFx, null, HeadsHitSfx)
            .Execute(ctx);
    }

    public override List<(string, string)>? Localization => new CardLoc(
        "收网",
        "#消耗你所有的必正次数，每消耗 1 次造成 !PerForce! 伤害（没有必正时无事发生）。");
}
