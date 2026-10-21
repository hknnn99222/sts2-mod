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

// 罕见 #27·高利贷（游戏设计 §5.2 / §5.5，v1.12 改型：技能 → 能力）：
// 0 能，立即获得 50 金币（升级 60）；此后每回合开始失去 5 金币（由 LoanInterestPower 结算）。
// 钩子由「跨战斗贷款」改为「借贷·回合利息」——先拿一大笔现金，代价是本场战斗的持续失血（钱包）。
public sealed class LoanShark : GamblerCard
{
    private const int InterestPerTurn = 5;

    public LoanShark() : base(0, CardType.Power, CardRarity.Uncommon, TargetType.None) { }

    protected override IEnumerable<DynamicVar> CanonicalVars => [new IntVar("GoldGain", 50)];

    protected override void OnUpgrade() => DynamicVars["GoldGain"].UpgradeValueBy(10);

    protected override async Task ApplyEffect(PlayerChoiceContext ctx, CardPlay play, bool heads)
    {
        await PlayerCmd.GainGold(DynamicVars["GoldGain"].BaseValue, Owner, false);
        await PowerCmd.Apply<Powers.LoanInterestPower>(
            ctx, Owner.Creature, InterestPerTurn, Owner.Creature, this);
    }

    public override List<(string, string)>? Localization => new CardLoc(
        "高利贷",
        "#获得 !GoldGain! 金币。每回合开始失去 5 金币。");
}
