using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using GamblerMod.Core;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace GamblerMod.Cards.Basic;

// 起始签名牌·本钱（游戏设计 §3）：0 能技能，获得 5 金币（升级 7）。
// 免能量打出——产金基准 1 能量 = 7 金币（§2.1），本钱零费小额、每回合可白拿。
// 产金走真金币（PlayerCmd），与商店同一袋钱——"金币是第二费用系统"的收入面样板。
public sealed class PrivateStash() : GamblerCard(0, CardType.Skill, CardRarity.Basic, TargetType.None)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new IntVar("GoldGain", 5)];

    protected override void OnUpgrade() => DynamicVars["GoldGain"].UpgradeValueBy(2);

    protected override Task ApplyEffect(PlayerChoiceContext ctx, CardPlay play, bool heads)
        => PlayerCmd.GainGold(DynamicVars["GoldGain"].BaseValue, Owner, false);

    public override List<(string, string)>? Localization => new CardLoc("本钱", "#获得 *!GoldGain! 金币*。");
}
