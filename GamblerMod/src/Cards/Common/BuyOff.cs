using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using GamblerMod.Core;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace GamblerMod.Cards.Common;

// 普通 #58·换码（游戏设计 §5.1 / §5.5）：0 能 + 6 金，获得 2 点能量（升级 3 点）。
// 钩子：附加费·买能量。价格锚点用原版金印的市场比（3 金 = 1 能量）。
public sealed class BuyOff : GamblerCard
{
    public BuyOff() : base(0, CardType.Skill, CardRarity.Common, TargetType.None)
    {
        BaseGoldCost = 6;
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [new IntVar("Energy", 2)];

    protected override void OnUpgrade() => DynamicVars["Energy"].UpgradeValueBy(1);

    protected override Task ApplyEffect(PlayerChoiceContext ctx, CardPlay play, bool heads)
        => PlayerCmd.GainEnergy((int)DynamicVars["Energy"].BaseValue, Owner);

    public override List<(string, string)>? Localization => new CardLoc(
        "换码",
        "#花费 *6 金币*。\n获得 !Energy! 点能量。");
}
