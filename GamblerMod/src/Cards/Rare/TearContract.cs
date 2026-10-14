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

// 稀有 #39·撕毁契约（游戏设计 §5.3）：2 能，消耗。抽 2 张牌，获得 12 金币（升级 抽 3 / 16 金）。
public sealed class TearContract() : GamblerCard(2, CardType.Skill, CardRarity.Rare, TargetType.None)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new IntVar("Cards", 2),
        new IntVar("GoldGain", 12),
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
        await PlayerCmd.GainGold(DynamicVars["GoldGain"].BaseValue, Owner, true);
    }

    public override List<(string, string)>? Localization => new CardLoc(
        "撕毁契约",
        "#消耗。抽 !Cards! 张牌，获得 !GoldGain! 金币。");
}
