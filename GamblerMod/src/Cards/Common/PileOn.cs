using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using GamblerMod.Core;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace GamblerMod.Cards.Common;

// 普通 #57·护池（游戏设计 §5.1 / §5.5，v1.10 改版）：0 能 + 5 金，获得 4 格挡 4 次（升级 6 格挡 4 次）。
// 钩子：附加费·多段格挡（v1.7 由攻击牌转为防御牌，提高防备浓度）。
public sealed class PileOn : GamblerCard
{
    public PileOn() : base(0, CardType.Skill, CardRarity.Common, TargetType.None)
    {
        BaseGoldCost = 5;
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(4, ValueProp.Move),
        new IntVar("Hits", 4),
    ];

    protected override void OnUpgrade() => DynamicVars.Block.UpgradeValueBy(2);

    protected override async Task ApplyEffect(PlayerChoiceContext ctx, CardPlay play, bool heads)
    {
        var hits = (int)DynamicVars["Hits"].BaseValue;
        for (var i = 0; i < hits; i++)
            await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, play);
    }

    public override List<(string, string)>? Localization => new CardLoc(
        "护池",
        "#花费 *5 金币*。\n获得 !B! 格挡 !Hits! 次。");
}
