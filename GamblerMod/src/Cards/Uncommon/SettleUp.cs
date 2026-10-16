using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using GamblerMod.Core;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace GamblerMod.Cards.Uncommon;

// 罕见 #47·兑现（游戏设计 §5.2 / §2.8）：1 能，获得 7 金币 ×风头倍率（升级 9）。
// 金币没有带 cardSource 的钩子（ModifyGoldGained 拿不到牌），所以**在卡内自算**；
// 卡面用 MultiplierAwareVar 显示"当前倍率下的实际到账值"（2026-10-17 修掉原"卡面裸值"退化）。
public sealed class SettleUp : GamblerCard
{
    public SettleUp() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
        IsHeadsStreakCard = true;
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [new MultiplierAwareVar("GoldGain", 7)];

    protected override void OnUpgrade() => DynamicVars["GoldGain"].UpgradeValueBy(2);

    protected override Task ApplyEffect(PlayerChoiceContext ctx, CardPlay play, bool heads)
        => PlayerCmd.GainGold((int)(DynamicVars["GoldGain"].BaseValue * MultiplierService.Factor), Owner, false);

    public override List<(string, string)>? Localization => new CardLoc(
        "兑现",
        "#获得 !GoldGain! 金币（×倍率；结算后清空）。");
}
