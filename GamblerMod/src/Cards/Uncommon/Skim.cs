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

// 罕见 #60·做手脚（游戏设计 §5.2 / §5.5，v1.12 改版）：1 能，获得 2 次必正 + 5 点活力（升级 3 次必正）。
// 钩子由「即时产金·手牌数」改为「出千·必正与活力」——同时给确定性资源与下一次攻击的爆发。
public sealed class Skim : GamblerCard
{
    public Skim() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.None) { }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new IntVar("Force", 2),
        new IntVar("Vigor", 5),
    ];

    protected override void OnUpgrade() => DynamicVars["Force"].UpgradeValueBy(1);

    protected override async Task ApplyEffect(PlayerChoiceContext ctx, CardPlay play, bool heads)
    {
        GambleService.ForceNextFlips((int)DynamicVars["Force"].BaseValue);
        await PowerCmd.Apply<VigorPower>(ctx, Owner.Creature, DynamicVars["Vigor"].BaseValue, Owner.Creature, this);
    }

    public override List<(string, string)>? Localization => new CardLoc(
        "做手脚",
        "#获得 !Force! 次必正，以及 !Vigor! 点活力。");
}
