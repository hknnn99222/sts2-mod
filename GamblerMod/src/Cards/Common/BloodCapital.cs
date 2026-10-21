using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using GamblerMod.Core;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace GamblerMod.Cards.Common;

// 普通 #83·分红（v1.10 由「血本」改名并改型：技能 → 能力）：
// 每回合开始支付 3 金币，回复 1 点生命（升级回 2）。金币不足 3 时不触发。
// 赌命轴的续航件：把金币流水换成血量，让放血/押宝等自伤牌能长期转下去。
// 类名与文件名保留 BloodCapital（历史名），显示名以 Localization 为准。
public sealed class BloodCapital() : GamblerCard(0, CardType.Power, CardRarity.Common, TargetType.None)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new IntVar("Heal", 1)];

    protected override void OnUpgrade() => DynamicVars["Heal"].UpgradeValueBy(1);

    protected override Task ApplyEffect(PlayerChoiceContext ctx, CardPlay play, bool heads)
        => PowerCmd.Apply<Powers.DividendPower>(ctx, Owner.Creature, DynamicVars["Heal"].BaseValue, Owner.Creature, this);

    public override List<(string, string)>? Localization => new CardLoc(
        "分红",
        "#每回合开始支付 *3 金币*，回复 !Heal! 点生命。");
}
