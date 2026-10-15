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

// 罕见 #19·雇佣打手（游戏设计 §5.2 / §5.4）：1 能 + 5 金，对随机敌人 4×3 次；
// 赌博：正面 → 每次 9（升级 5×3 / 正面每次 10）。
public sealed class HiredThugs : GamblerCard
{
    public HiredThugs() : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.RandomEnemy)
    {
        BaseGoldCost = 5;
        IsGambleCard = true;
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(4, ValueProp.Move),
        new IntVar("HeadsDamage", 9),
        new IntVar("Hits", 3),
    ];

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(1);
        DynamicVars["HeadsDamage"].UpgradeValueBy(1);
    }

    protected override Task ApplyEffect(PlayerChoiceContext ctx, CardPlay play, bool heads)
        => DamageCmd.Attack(heads ? DynamicVars["HeadsDamage"].BaseValue : DynamicVars.Damage.BaseValue)
            .FromCard(this, play)
            .TargetingRandomOpponents(CombatState)
            .WithHitCount((int)DynamicVars["Hits"].BaseValue)
            .WithHitFx(heads ? HeadsHitFx : TailsHitFx, null, heads ? HeadsHitSfx : null)
            .Execute(ctx);

    public override List<(string, string)>? Localization => new CardLoc(
        "雇佣打手",
        "#花费 *5 金币*。\n对随机敌人造成 !D! 伤害 !Hits! 次。*赌博*：正面 → 每次 !HeadsDamage! 伤害。");
}
