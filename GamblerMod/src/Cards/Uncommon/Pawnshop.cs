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

namespace GamblerMod.Cards.Uncommon;

// 罕见 #23·兑筹（游戏设计 §5.2 / §5.5，v1.12 改版）：1 能 + 6 金，获得 2 点力量（升级 3）。
// 钩子由「牌换金·卡组精简」改为「附加费·买力量」——金币直接换成战斗属性。
public sealed class Pawnshop : GamblerCard
{
    public Pawnshop() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
        BaseGoldCost = 6;
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [new IntVar("Strength", 2)];

    protected override void OnUpgrade() => DynamicVars["Strength"].UpgradeValueBy(1);

    protected override Task ApplyEffect(PlayerChoiceContext ctx, CardPlay play, bool heads)
        => PowerCmd.Apply<StrengthPower>(ctx, Owner.Creature, DynamicVars["Strength"].BaseValue, Owner.Creature, this);

    public override List<(string, string)>? Localization => new CardLoc(
        "兑筹",
        "#花费 *6 金币*。\n获得 !Strength! 点力量。");
}
