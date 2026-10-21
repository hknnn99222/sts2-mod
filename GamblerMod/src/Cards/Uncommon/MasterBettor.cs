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

// 罕见 #59·再加注（游戏设计 §5.2，v1.12 改型：攻击 → 技能）：0 能 + 3 金，获得 5 点活力（升级 8）。
// 钩子由「可变投入·档位解锁」改为「附加费·买活力」——活力 = 引擎原生 VigorPower
// （储君同款：下一次攻击额外造成等量伤害，打出后消耗）。
public sealed class MasterBettor : GamblerCard
{
    public MasterBettor() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
        BaseGoldCost = 3;
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [new IntVar("Vigor", 5)];

    protected override void OnUpgrade() => DynamicVars["Vigor"].UpgradeValueBy(3);

    protected override Task ApplyEffect(PlayerChoiceContext ctx, CardPlay play, bool heads)
        => PowerCmd.Apply<VigorPower>(ctx, Owner.Creature, DynamicVars["Vigor"].BaseValue, Owner.Creature, this);

    public override List<(string, string)>? Localization => new CardLoc(
        "再加注",
        "#花费 *3 金币*。\n获得 !Vigor! 点活力（下次攻击额外造成等量伤害）。");
}
