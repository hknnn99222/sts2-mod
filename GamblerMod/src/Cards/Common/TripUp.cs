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

// 普通 #10·放水（游戏设计 §5.1，v1.10 改版）：1 能 + 3 金，施加 3 虚弱（升级 4）。
public sealed class TripUp : GamblerCard
{
    public TripUp() : base(1, CardType.Skill, CardRarity.Common, TargetType.AnyEnemy)
    {
        BaseGoldCost = 3;
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [new IntVar("Weak", 3)];

    protected override void OnUpgrade() => DynamicVars["Weak"].UpgradeValueBy(1);

    protected override Task ApplyEffect(PlayerChoiceContext ctx, CardPlay play, bool heads)
        => PowerCmd.Apply<WeakPower>(ctx, play.Target, DynamicVars["Weak"].BaseValue, Owner.Creature, this);

    public override List<(string, string)>? Localization => new CardLoc(
        "放水",
        "#花费 *3 金币*。\n施加 !Weak! 虚弱。");
}
