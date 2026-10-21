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

// 罕见 #15·双倍下注（游戏设计 §5.2，v1.12 降费）：1 能，10 伤害；赌博：正面 → 改为打两次（升级 12 / 12×2）。
public sealed class DoubleDown : GamblerCard
{
    public DoubleDown() : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
        IsGambleCard = true;
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(10, ValueProp.Move)];

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(2);

    protected override async Task ApplyEffect(PlayerChoiceContext ctx, CardPlay play, bool heads)
    {
        var hits = heads ? 2 : 1;
        for (var i = 0; i < hits; i++)
        {
            await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
                .FromCard(this, play)
                .Targeting(play.Target)
                .WithHitFx(heads ? HeadsHitFx : TailsHitFx, null, heads ? HeadsHitSfx : null)
                .Execute(ctx);
        }
    }

    public override List<(string, string)>? Localization => new CardLoc(
        "双倍下注",
        "#硬币为正时：造成 !D! 伤害两次；硬币为负时：造成 !D! 伤害。");
}
