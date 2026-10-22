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

namespace GamblerMod.Cards.Basic;

// 起始牌·防御（v1.16）：赌徒自己的防御，不再借用铁甲战士的 DefendIronclad。
// 数值与原版基础牌一致：1 能获得 5 格挡（升级 8），带 Defend 标签。
public sealed class DefendGambler() : GamblerCard(1, CardType.Skill, CardRarity.Basic, TargetType.None)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(5, ValueProp.Move)];

    protected override HashSet<CardTag> CanonicalTags => [CardTag.Defend];

    protected override void OnUpgrade() => DynamicVars.Block.UpgradeValueBy(3);

    protected override Task ApplyEffect(PlayerChoiceContext ctx, CardPlay play, bool heads)
        => CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, play);

    public override List<(string, string)>? Localization => new CardLoc(
        "防御",
        "#获得 !B! 格挡。");
}
