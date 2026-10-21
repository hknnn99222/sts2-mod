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

// 罕见 #17·翻本（游戏设计 §5.2，v1.12 改型：攻击 → 技能）：0 能 + 10 金；
// 硬币为正时获得 25 金币，为负时获得 5 金币（升级 6 / 正面 30）。
// 钩子：赌博·高方差产金——不再是"赌伤害"，而是"赌这一把的返点"。
public sealed class Comeback : GamblerCard
{
    public Comeback() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
        BaseGoldCost = 10;
        IsGambleCard = true;
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new IntVar("TailsGold", 5),
        new IntVar("HeadsGold", 25),
    ];

    protected override void OnUpgrade()
    {
        DynamicVars["TailsGold"].UpgradeValueBy(1);
        DynamicVars["HeadsGold"].UpgradeValueBy(5);
    }

    protected override Task ApplyEffect(PlayerChoiceContext ctx, CardPlay play, bool heads)
        => PlayerCmd.GainGold(
            heads ? DynamicVars["HeadsGold"].BaseValue : DynamicVars["TailsGold"].BaseValue,
            Owner, false);

    public override List<(string, string)>? Localization => new CardLoc(
        "翻本",
        "#花费 *10 金币*。\n硬币为正时：获得 !HeadsGold! 金币；硬币为负时：获得 !TailsGold! 金币。");
}
