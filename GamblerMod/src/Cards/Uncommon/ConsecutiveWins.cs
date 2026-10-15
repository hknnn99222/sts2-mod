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

namespace GamblerMod.Cards.Uncommon;

// 罕见 #69·连庄（游戏设计 §5.2 / §5.5）：2 能，掷硬币 3 次，每次正面获得 8 格挡（升级 10）。
// 锚定：白板 2 能 = 12 伤；期望 3×0.5×8 = 12 挡 = 14.4 伤当量 → 1.2× ✓
// 钩子：赌博·累积多掷（逐次独立掷，每次正面都推进倍率）。
public sealed class ConsecutiveWins : GamblerCard
{
    public ConsecutiveWins() : base(2, CardType.Skill, CardRarity.Uncommon, TargetType.None) { }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new IntVar("Block", 8),
        new IntVar("Spins", 3),
    ];

    protected override void OnUpgrade() => DynamicVars["Block"].UpgradeValueBy(2);

    protected override async Task ApplyEffect(PlayerChoiceContext ctx, CardPlay play, bool heads)
    {
        var spins = (int)DynamicVars["Spins"].BaseValue;
        for (var i = 0; i < spins; i++)
        {
            if (!await GambleService.Flip(Owner, this)) continue;
            await CreatureCmd.GainBlock(Owner.Creature, new BlockVar(DynamicVars["Block"].BaseValue, ValueProp.Move), play);
        }
    }

    public override List<(string, string)>? Localization => new CardLoc(
        "连庄",
        "#掷硬币 !Spins! 次，每次*正面*获得 !Block! 格挡。");
}
