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

// 罕见 #25·保险（游戏设计 §5.2 / §5.4）：1 能 + 2 金，获得 8 格挡；
// 反向赌博：反面 → 改为 16 格挡（升级 10 / 反面 20）。全池唯一"盼反面"的赌博牌。
public sealed class Insurance : GamblerCard
{
    public Insurance() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
        BaseGoldCost = 2;
        IsGambleCard = true;
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(8, ValueProp.Move),
        new EnchantAwareVar("TailsBlock", 16, EnchantKind.Block),
    ];

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(2);
        DynamicVars["TailsBlock"].UpgradeValueBy(4);
    }

    protected override Task ApplyEffect(PlayerChoiceContext ctx, CardPlay play, bool heads)
        => CreatureCmd.GainBlock(
            Owner.Creature,
            heads ? DynamicVars.Block.BaseValue : DynamicVars["TailsBlock"].BaseValue,
            ValueProp.Move, play, false);

    public override List<(string, string)>? Localization => new CardLoc(
        "保险",
        "#花费 *2 金币*。\n硬币为正时：获得 !B! 格挡；硬币为负时：获得 !TailsBlock! 格挡。");
}
