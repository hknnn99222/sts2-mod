using System;
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

// 稀有 #75·保本（游戏设计 §5.3 / §5.5）：2 能 + 10 金，**消耗你一半金币，获得等量格挡**（至多 30；升级至多 40）。
// 钩子：特异·金币转格挡（v1.7 由"金币转伤"改为防御向——把钱押在台面上换护甲）。
public sealed class TheVault : GamblerCard
{
    public TheVault() : base(2, CardType.Skill, CardRarity.Rare, TargetType.None)
    {
        BaseGoldCost = 10;
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [new IntVar("Cap", 30)];

    protected override void OnUpgrade() => DynamicVars["Cap"].UpgradeValueBy(10);

    protected override async Task ApplyEffect(PlayerChoiceContext ctx, CardPlay play, bool heads)
    {
        var half = Owner.Gold / 2;
        var cap = (int)DynamicVars["Cap"].BaseValue;
        var amount = Math.Min(half, cap);
        if (amount <= 0) return;

        await GoldCost.Pay(Owner, amount);
        await CreatureCmd.GainBlock(Owner.Creature, new BlockVar(amount, ValueProp.Move), play);
    }

    public override List<(string, string)>? Localization => new CardLoc(
        "保本",
        "#花费 *10 金币*。\n消耗你一半金币，获得等量格挡（至多 !Cap!）。");
}
