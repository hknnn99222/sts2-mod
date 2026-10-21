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

// 罕见 #65·底气（游戏设计 §5.2 / §5.5，v1.14 改版）：1 能，获得 1 点能量（升级基础 2）；
// 每拥有 50 金币额外 +1 点能量，不设上限。钩子由「花费追踪·转能量」改为「持有量·转能量」——
// 改读钱包余额而不是本回合流水，和"攒钱"路线绑定。
public sealed class RakeFee : GamblerCard
{
    private const int GoldPerEnergy = 50;

    public RakeFee() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.None) { }

    protected override IEnumerable<DynamicVar> CanonicalVars => [new IntVar("Energy", 1)];

    protected override void OnUpgrade() => DynamicVars["Energy"].UpgradeValueBy(1);

    protected override Task ApplyEffect(PlayerChoiceContext ctx, CardPlay play, bool heads)
    {
        var energy = (int)DynamicVars["Energy"].BaseValue + Owner.Gold / GoldPerEnergy;
        return PlayerCmd.GainEnergy(energy, Owner);
    }

    public override List<(string, string)>? Localization => new CardLoc(
        "底气",
        "#获得 !Energy! 点能量。每拥有 *50 金币*，额外 +1 点能量。");
}
