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

// 罕见 #62·反水（游戏设计 §5.2 / §5.5，v1.14 改版）：1 能 + 3 金，赌博：
// 正面 → 10 格挡 + 10 伤害；反面 → 6 格挡 + 6 伤害（升级 12+12 / 7+7）。
// 钩子：赌博·格挡反击（守攻混合形态；v1.14 反面前也带伤害，不再只是纯防御）。
public sealed class Backhand : GamblerCard
{
    public Backhand() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
        BaseGoldCost = 3;
        IsGambleCard = true;
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(6, ValueProp.Move),
        new BlockVar("HeadsBlock", 10, ValueProp.Move),
        new DamageVar(6, ValueProp.Move),
        new DamageVar("HeadsDamage", 10, ValueProp.Move),
    ];

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(1);
        DynamicVars["HeadsBlock"].UpgradeValueBy(2);
        DynamicVars.Damage.UpgradeValueBy(1);
        DynamicVars["HeadsDamage"].UpgradeValueBy(2);
    }

    protected override async Task ApplyEffect(PlayerChoiceContext ctx, CardPlay play, bool heads)
    {
        await CreatureCmd.GainBlock(
            Owner.Creature,
            heads ? DynamicVars["HeadsBlock"].BaseValue : DynamicVars.Block.BaseValue,
            ValueProp.Move, play, false);

        await DamageCmd.Attack(heads ? DynamicVars["HeadsDamage"].BaseValue : DynamicVars.Damage.BaseValue)
            .FromCard(this, play)
            .Targeting(play.Target)
            .WithHitFx(heads ? HeadsHitFx : TailsHitFx, null, heads ? HeadsHitSfx : null)
            .Execute(ctx);
    }

    public override List<(string, string)>? Localization => new CardLoc(
        "反水",
        "#花费 *3 金币*。\n硬币为正时：获得 !HeadsBlock! 格挡并造成 !HeadsDamage! 伤害；硬币为负时：获得 !B! 格挡并造成 !D! 伤害。");
}
