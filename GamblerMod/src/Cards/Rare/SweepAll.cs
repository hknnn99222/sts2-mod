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

namespace GamblerMod.Cards.Rare;

// 稀有 #77·截胡（游戏设计 §5.3 / §5.5，v1.15 改型：攻击 → 技能）：1 能 + 6 金，
// 获得 16 格挡 + 等量金币（升级 20 / 20）。钩子：特异·格挡返金。
public sealed class SweepAll : GamblerCard
{
    public SweepAll() : base(1, CardType.Skill, CardRarity.Rare, TargetType.None)
    {
        BaseGoldCost = 6;
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(16, ValueProp.Move)];

    protected override void OnUpgrade() => DynamicVars.Block.UpgradeValueBy(4);

    protected override async Task ApplyEffect(PlayerChoiceContext ctx, CardPlay play, bool heads)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, play);
        await PlayerCmd.GainGold(DynamicVars.Block.BaseValue, Owner, false);
    }

    public override List<(string, string)>? Localization => new CardLoc(
        "截胡",
        "#花费 *6 金币*。\n获得 !B! 格挡，并获得等量金币。");
}
