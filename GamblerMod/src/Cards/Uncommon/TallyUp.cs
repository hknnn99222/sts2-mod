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

// 罕见 #64·保底（游戏设计 §5.2 / §5.5）：1 能，获得 10 格挡；每张**已消耗**的牌 +2 格挡（至多 +10；升级 12 格挡）。
// 钩子：消耗堆协同·转格挡（v1.7 由伤害牌转为防御牌）。
public sealed class TallyUp : GamblerCard
{
    public TallyUp() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.None) { }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(10, ValueProp.Move),
        new IntVar("PerExhaust", 2),
        new IntVar("Cap", 10),
    ];

    protected override void OnUpgrade() => DynamicVars.Block.UpgradeValueBy(2);

    protected override Task ApplyEffect(PlayerChoiceContext ctx, CardPlay play, bool heads)
    {
        var exhausted = PileType.Exhaust.GetPile(Owner).Cards.Count;
        var bonus = exhausted * (int)DynamicVars["PerExhaust"].BaseValue;
        if (bonus > (int)DynamicVars["Cap"].BaseValue) bonus = (int)DynamicVars["Cap"].BaseValue;

        return CreatureCmd.GainBlock(Owner.Creature, new BlockVar(DynamicVars.Block.BaseValue + bonus, ValueProp.Move), play);
    }

    public override List<(string, string)>? Localization => new CardLoc(
        "保底",
        "#获得 !B! 格挡。每张*已消耗*的牌额外 +!PerExhaust! 格挡（至多 +!Cap!）。");
}
