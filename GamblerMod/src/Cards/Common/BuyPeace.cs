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

namespace GamblerMod.Cards.Common;

// 普通 #7·护盘（游戏设计 §5.1，v1.10 改价）：1 能 + 2 金，获得 15 格挡（升级 18）。
public sealed class BuyPeace : GamblerCard
{
    public BuyPeace() : base(1, CardType.Skill, CardRarity.Common, TargetType.None)
    {
        BaseGoldCost = 2;
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(15, ValueProp.Move)];

    protected override void OnUpgrade() => DynamicVars.Block.UpgradeValueBy(3);

    protected override Task ApplyEffect(PlayerChoiceContext ctx, CardPlay play, bool heads)
        => CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, play);

    public override List<(string, string)>? Localization => new CardLoc(
        "护盘",
        "#花费 *2 金币*。\n获得 !B! 格挡。");
}
