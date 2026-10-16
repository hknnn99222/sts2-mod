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

// 罕见 #63·叠码（游戏设计 §5.2 / §5.5）：1 能，倍率 +1.0（升级 +1.5）——**不吃倍率、也不清空**。
// 钩子：做局·倍率注入（倍率是唯一真源，直接推进刻度；不进乘法通道）。
public sealed class BankWind : GamblerCard
{
    public BankWind() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.None) { }

    protected override IEnumerable<DynamicVar> CanonicalVars => [new IntVar("Steps", 2)]; // 2 步 = +1.0

    protected override void OnUpgrade() => DynamicVars["Steps"].UpgradeValueBy(1);

    protected override Task ApplyEffect(PlayerChoiceContext ctx, CardPlay play, bool heads)
    {
        // AddSteps 只推进刻度、不写 LastOutcome——正/反徽章只跟"最近一次掷币"走（2026-10-16 审查修正）
        MultiplierService.AddSteps((int)DynamicVars["Steps"].BaseValue);
        return Task.CompletedTask;
    }

    public override List<(string, string)>? Localization => new CardLoc(
        "叠码",
        "#你的风头倍率 +0.5 ×!Steps!（不清空）。");
}
