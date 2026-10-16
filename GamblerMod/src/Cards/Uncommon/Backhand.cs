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

// 罕见 #62·反水（游戏设计 §5.2 / §5.5）：1 能 + 2 金，赌博：正面 → 获得 9 格挡并造成 9 伤害；反面 → 5 格挡（升级 11/6）。
// 钩子：赌博·格挡反击（守攻混合形态）。
public sealed class Backhand : GamblerCard
{
    public Backhand() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
        BaseGoldCost = 2;
        IsGambleCard = true;
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(5, ValueProp.Move),
        new EnchantAwareVar("HeadsBlock", 9, EnchantKind.Block),
        new EnchantAwareVar("HeadsDamage", 9, EnchantKind.Damage),
    ];

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(1);
        DynamicVars["HeadsBlock"].UpgradeValueBy(2);
        DynamicVars["HeadsDamage"].UpgradeValueBy(2);
    }

    protected override async Task ApplyEffect(PlayerChoiceContext ctx, CardPlay play, bool heads)
    {
        var block = heads ? (int)DynamicVars["HeadsBlock"].BaseValue : DynamicVars.Block.BaseValue;
        await CreatureCmd.GainBlock(Owner.Creature, new BlockVar(block, ValueProp.Move), play);

        if (heads)
        {
            await DamageCmd.Attack(DynamicVars["HeadsDamage"].BaseValue)
                .FromCard(this, play)
                .Targeting(play.Target)
                .WithHitFx(HeadsHitFx, null, HeadsHitSfx)
                .Execute(ctx);
        }
    }

    public override List<(string, string)>? Localization => new CardLoc(
        "反水",
        "#花费 *2 金币*。\n硬币为正时：获得 !HeadsBlock! 格挡并造成 !HeadsDamage! 伤害；硬币为负时：获得 !B! 格挡。");
}
