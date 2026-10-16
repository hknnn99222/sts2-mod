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

// 罕见 #19·摇骰（游戏设计 §5.2 / §5.4）：1 能 + 5 金，对随机敌人 4×3 次；
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
        new EnchantAwareVar("HeadsDamage", 9, EnchantKind.Damage),
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
        "摇骰",
        "#花费 *5 金币*。\n硬币为正时：对随机敌人造成 !HeadsDamage! 伤害 !Hits! 次；硬币为负时：对随机敌人造成 !D! 伤害 !Hits! 次。");
}
