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

// 普通 #84·放血（v1.9 四轴·赌命轴，v1.10 改型：攻击 → 技能）：
// 0 能，失去 2 点生命，获得 20 金币（升级 26）。
// 赌命轴的金币生成器：把血量直接换成"钱"，再由金币流水买回血量（分红）或转成伤害（押注）。
public sealed class BleedOut() : GamblerCard(0, CardType.Skill, CardRarity.Common, TargetType.None)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new IntVar("GoldGain", 20), new IntVar("HpLoss", 2)];

    protected override void OnUpgrade() => DynamicVars["GoldGain"].UpgradeValueBy(6);

    protected override async Task ApplyEffect(PlayerChoiceContext ctx, CardPlay play, bool heads)
    {
        await CreatureCmd.Damage(ctx, Owner.Creature, DynamicVars["HpLoss"].BaseValue, ValueProp.Move, this, play);
        await PlayerCmd.GainGold(DynamicVars["GoldGain"].BaseValue, Owner, false);
    }

    public override List<(string, string)>? Localization => new CardLoc(
        "放血",
        "#失去 !HpLoss! 点生命，获得 !GoldGain! 金币。");
}
