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

// 罕见 #70·抵押（游戏设计 §5.2 / §5.5）：0 能，获得 12 格挡；自己失去 4 点生命（升级 16 格挡）。
// 钩子：特异·卖血换甲（v1.7 由攻击牌转为防御牌；与「割肉」的倍率自伤版区分：这里是定额、无风头条件）。
public sealed class SkipStone : GamblerCard
{
    public SkipStone() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.None) { }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(12, ValueProp.Move),
        new IntVar("SelfHurt", 4),
    ];

    protected override void OnUpgrade() => DynamicVars.Block.UpgradeValueBy(4);

    protected override async Task ApplyEffect(PlayerChoiceContext ctx, CardPlay play, bool heads)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, play);
        await CreatureCmd.Damage(ctx, Owner.Creature, DynamicVars["SelfHurt"].BaseValue, ValueProp.Move, this, play);
    }

    public override List<(string, string)>? Localization => new CardLoc(
        "抵押",
        "#获得 !B! 格挡；自己失去 !SelfHurt! 点生命。");
}
