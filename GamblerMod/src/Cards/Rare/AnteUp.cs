using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using GamblerMod.Core;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace GamblerMod.Cards.Rare;

// 稀有 #76·押注（游戏设计 §5.3 / §5.5，v1.15 改版）：1 能 + 5 金，获得 2 点能量（升级 3）；
// 赌博：正面 → 返还本次花费的 5 金币。钩子：真赌·退注（赌的是"这 5 金花得值不值"）。
public sealed class AnteUp : GamblerCard
{
    private const int GoldCostAmount = 5;

    public AnteUp() : base(1, CardType.Skill, CardRarity.Rare, TargetType.None)
    {
        BaseGoldCost = 5;
        IsGambleCard = true;
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [new IntVar("EnergyGain", 2)];

    protected override void OnUpgrade() => DynamicVars["EnergyGain"].UpgradeValueBy(1);

    protected override async Task ApplyEffect(PlayerChoiceContext ctx, CardPlay play, bool heads)
    {
        await PlayerCmd.GainEnergy(DynamicVars["EnergyGain"].BaseValue, Owner);
        if (heads)
            await PlayerCmd.GainGold(GoldCostAmount, Owner, false);
    }

    public override List<(string, string)>? Localization => new CardLoc(
        "押注",
        "#花费 *5 金币*。\n获得 !EnergyGain! 点能量。硬币为正时：返还这 5 金币；硬币为负时：不返还。");
}
