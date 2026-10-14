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

// 稀有 #37·老虎机（游戏设计 §5.3）：2 能 + 5 金，赌博 3 次，每次正面：获得 7 金币并抽 1 张牌（升级每次 9 金）。
// 逐次独立掷（基类单掷流程不够用，卡内自行掷；heads 参数无效）。
public sealed class SlotMachine : GamblerCard
{
    public SlotMachine() : base(2, CardType.Skill, CardRarity.Rare, TargetType.None)
    {
        BaseGoldCost = 5;
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new IntVar("GoldGain", 7),
        new IntVar("Spins", 3),
    ];

    protected override void OnUpgrade() => DynamicVars["GoldGain"].UpgradeValueBy(2);

    protected override async Task ApplyEffect(PlayerChoiceContext ctx, CardPlay play, bool heads)
    {
        var spins = (int)DynamicVars["Spins"].BaseValue;
        for (var i = 0; i < spins; i++)
        {
            if (!await GambleService.Flip(Owner, this)) continue;
            await PlayerCmd.GainGold(DynamicVars["GoldGain"].BaseValue, Owner, true);
            await CardPileCmd.Draw(ctx, 1, Owner);
        }
    }

    public override List<(string, string)>? Localization => new CardLoc(
        "老虎机",
        "#*额外花费 5 金币*。*赌博* 3 次，每次正面：获得 !GoldGain! 金币并抽 1 张牌。");
}
