using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using GamblerMod.Core;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace GamblerMod.Cards.Rare;

// 稀有 #36·单双（游戏设计 §5.3，v1.15 改版）：1 能，看**风头倍率的奇偶**：
// 偶数 → 获得 20 金币；奇数 → 获得 2 点力量 + 2 点敏捷（升级 26 金 / 3+3）。
// 不再是赌博牌（不掷币），钩子改「特异·倍率奇偶」——把倍率本身当成可读的状态。
public sealed class Destiny : GamblerCard
{
    public Destiny() : base(1, CardType.Skill, CardRarity.Rare, TargetType.None) { }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new IntVar("GoldGain", 20),
        new IntVar("StatGain", 2),
    ];

    protected override void OnUpgrade()
    {
        DynamicVars["GoldGain"].UpgradeValueBy(6);
        DynamicVars["StatGain"].UpgradeValueBy(1);
    }

    protected override async Task ApplyEffect(PlayerChoiceContext ctx, CardPlay play, bool heads)
    {
        if (MultiplierService.Factor % 2 == 0)
        {
            await PlayerCmd.GainGold(DynamicVars["GoldGain"].BaseValue, Owner, false);
            return;
        }

        var gain = DynamicVars["StatGain"].BaseValue;
        await PowerCmd.Apply<StrengthPower>(ctx, Owner.Creature, gain, Owner.Creature, this);
        await PowerCmd.Apply<DexterityPower>(ctx, Owner.Creature, gain, Owner.Creature, this);
    }

    public override List<(string, string)>? Localization => new CardLoc(
        "单双",
        "#若风头倍率为偶数：获得 !GoldGain! 金币；若为奇数：获得 !StatGain! 点力量与 !StatGain! 点敏捷。");
}
