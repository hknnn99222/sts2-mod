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

// 稀有 #39·弃牌（游戏设计 §5.3，v1.15 改版）：1 能，消耗。抽 2 张牌，获得 16 金币（升级 抽 3 / 20 金）。
public sealed class TearContract() : GamblerCard(1, CardType.Skill, CardRarity.Rare, TargetType.None)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new IntVar("Cards", 2),
        new IntVar("GoldGain", 16),
    ];

    public override void AfterCreated()
    {
        base.AfterCreated();
        AddKeyword(CardKeyword.Exhaust);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Cards"].UpgradeValueBy(1);
        DynamicVars["GoldGain"].UpgradeValueBy(4);
    }

    protected override async Task ApplyEffect(PlayerChoiceContext ctx, CardPlay play, bool heads)
    {
        await CardPileCmd.Draw(ctx, (int)DynamicVars["Cards"].BaseValue, Owner);
        await PlayerCmd.GainGold(DynamicVars["GoldGain"].BaseValue, Owner, false);
    }

    public override List<(string, string)>? Localization => new CardLoc(
        "弃牌",
        "#消耗。抽 !Cards! 张牌，获得 !GoldGain! 金币。");
}
