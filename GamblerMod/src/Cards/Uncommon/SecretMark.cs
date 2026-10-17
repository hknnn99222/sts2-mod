using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using GamblerMod.Core;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace GamblerMod.Cards.Uncommon;

// 罕见 #20·暗记（游戏设计 §5.2）：1 能技能，下一次赌博必为正面（升级：接下来 2 次）。
public sealed class SecretMark() : GamblerCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.None)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new IntVar("Times", 2)];

    protected override void OnUpgrade() => DynamicVars["Times"].UpgradeValueBy(1);

    protected override Task ApplyEffect(PlayerChoiceContext ctx, CardPlay play, bool heads)
    {
        GambleService.ForceNextFlips((int)DynamicVars["Times"].BaseValue);
        return Task.CompletedTask;
    }

    public override List<(string, string)>? Localization => new CardLoc(
        "暗记",
        "#你的接下来 !Times! 次赌博必为正面。");
}
