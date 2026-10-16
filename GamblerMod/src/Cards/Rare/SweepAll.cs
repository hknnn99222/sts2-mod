using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using GamblerMod.Core;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace GamblerMod.Cards.Rare;

// 稀有 #77·截胡（游戏设计 §5.3 / §5.5）：2 能 + 8 金，对**生命值最低**的敌人造成 20 伤害；
// 若击杀，获得 15 金币（升级 26 伤害）。
// 钩子：击杀产金·处决（打庄的定向 + 击杀产金）。
public sealed class SweepAll : GamblerCard
{
    public SweepAll() : base(2, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
    {
        BaseGoldCost = 8;
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(20, ValueProp.Move),
        new IntVar("Bounty", 15),
    ];

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(6);

    protected override async Task ApplyEffect(PlayerChoiceContext ctx, CardPlay play, bool heads)
    {
        var target = CombatState.Enemies.Where(e => e.IsAlive).OrderBy(e => e.CurrentHp).FirstOrDefault();
        if (target == null) return;

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, play)
            .Targeting(target)
            .WithHitFx("vfx/vfx_bloody_impact", null, HeadsHitSfx)
            .Execute(ctx);

        if (target.IsDead)
            await PlayerCmd.GainGold(DynamicVars["Bounty"].BaseValue, Owner, false);
    }

    public override List<(string, string)>? Localization => new CardLoc(
        "截胡",
        "#花费 *8 金币*。\n对*生命值最低*的敌人造成 !D! 伤害；若击杀，获得 !Bounty! 金币。");
}
