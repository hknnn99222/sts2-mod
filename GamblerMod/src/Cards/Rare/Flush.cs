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

// 稀有 #35·同花顺（游戏设计 §5.3）：3 能 + 12 金，43 伤害；若本回合你掷出过 ≥3 次正面，改为 64（升级 46 / 69）。
public sealed class Flush : GamblerCard
{
    public Flush() : base(3, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
    {
        BaseGoldCost = 12;
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(43, ValueProp.Move),
        new IntVar("HotDamage", 64),
        new IntVar("HeadsNeeded", 3),
    ];

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3);
        DynamicVars["HotDamage"].UpgradeValueBy(5);
    }

    protected override Task ApplyEffect(PlayerChoiceContext ctx, CardPlay play, bool heads)
    {
        var hot = GambleService.HeadsCountThisTurn >= (int)DynamicVars["HeadsNeeded"].BaseValue;
        return DamageCmd.Attack(hot ? DynamicVars["HotDamage"].BaseValue : DynamicVars.Damage.BaseValue)
            .FromCard(this, play)
            .Targeting(play.Target)
            .WithHitFx(hot ? HeadsHitFx : TailsHitFx, null, hot ? HeadsHitSfx : null)
            .Execute(ctx);
    }

    public override List<(string, string)>? Localization => new CardLoc(
        "同花顺",
        "#花费 *12 金币*。\n造成 !D! 伤害。若本回合你掷出过 ≥3 次*正面*，改为 !HotDamage! 伤害。");
}
