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

namespace GamblerMod.Cards.Common;

// 普通 #10·下绊子（游戏设计 §5.1）：1 能 + 4 金，施加 2 虚弱（升级 3）。
public sealed class TripUp : GamblerCard
{
    public TripUp() : base(1, CardType.Skill, CardRarity.Common, TargetType.AnyEnemy)
    {
        BaseGoldCost = 4;
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [new IntVar("Weak", 2)];

    protected override void OnUpgrade() => DynamicVars["Weak"].UpgradeValueBy(1);

    protected override Task ApplyEffect(PlayerChoiceContext ctx, CardPlay play, bool heads)
        => PowerCmd.Apply<WeakPower>(ctx, play.Target, DynamicVars["Weak"].BaseValue, Owner.Creature, this);

    public override List<(string, string)>? Localization => new CardLoc(
        "下绊子",
        "#*额外花费 4 金币*。施加 !Weak! 虚弱。");
}
