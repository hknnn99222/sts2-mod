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

// 罕见 #25·保险（游戏设计 §5.2，v1.12 改版）：1 能 + 3 金，获得 10 格挡；
// 赌博：正面 → 16 格挡（升级 12 / 正面 19）。由"反向赌博（盼反面）"改为常规赌博·格挡。
public sealed class Insurance : GamblerCard
{
    public Insurance() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
        BaseGoldCost = 3;
        IsGambleCard = true;
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(10, ValueProp.Move),
        new BlockVar("HeadsBlock", 16, ValueProp.Move),
    ];

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(2);
        DynamicVars["HeadsBlock"].UpgradeValueBy(3);
    }

    protected override Task ApplyEffect(PlayerChoiceContext ctx, CardPlay play, bool heads)
        => CreatureCmd.GainBlock(
            Owner.Creature,
            heads ? DynamicVars["HeadsBlock"].BaseValue : DynamicVars.Block.BaseValue,
            ValueProp.Move, play, false);

    public override List<(string, string)>? Localization => new CardLoc(
        "保险",
        "#花费 *3 金币*。\n硬币为正时：获得 !HeadsBlock! 格挡；硬币为负时：获得 !B! 格挡。");
}
