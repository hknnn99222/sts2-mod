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

// 罕见 #68·暗注（游戏设计 §5.2 / §5.5）：1 能 + 6 金，造成 6 点**无视格挡**的伤害（升级 9）。
// 钩子：附加费·穿透（走 CreatureCmd.Damage + ValueProp.Unblockable，不触发格挡）。
public sealed class UnderTheTable : GamblerCard
{
    public UnderTheTable() : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
        BaseGoldCost = 6;
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [new IntVar("Pierce", 6)];

    protected override void OnUpgrade() => DynamicVars["Pierce"].UpgradeValueBy(3);

    protected override Task ApplyEffect(PlayerChoiceContext ctx, CardPlay play, bool heads)
    {
        if (play.Target == null) return Task.CompletedTask;
        return CreatureCmd.Damage(ctx, play.Target, DynamicVars["Pierce"].BaseValue, ValueProp.Unblockable, this, play);
    }

    public override List<(string, string)>? Localization => new CardLoc(
        "暗注",
        "#花费 *6 金币*。\n造成 !Pierce! 点**无视格挡**的伤害。");
}
