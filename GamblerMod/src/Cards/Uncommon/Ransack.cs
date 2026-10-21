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

namespace GamblerMod.Cards.Uncommon;

// 罕见 #14·清台（游戏设计 §5.2，v1.12 改版）：1 能 + 2 金，**消耗**；
// 对所有敌人 13 伤害，每击杀一个 +7 金（升级 15 伤）；**每打出一次，伤害永久 +4**。
// 钩子由「击杀产金·AoE收割」改为「击杀产金·自我成长」：牺牲复用性（消耗）换滚雪球。
// 成长直接写在卡牌实例的 DamageVar 上（与产线一致的"模型自带计数"做法），
// 同一次冒险内跨战斗保留；读档后的持久化依赖引擎对卡牌模型状态的序列化。
public sealed class Ransack : GamblerCard
{
    private const int GrowthPerPlay = 4;

    public Ransack() : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AllEnemies)
    {
        BaseGoldCost = 2;
    }

    public override void AfterCreated()
    {
        base.AfterCreated();
        AddKeyword(CardKeyword.Exhaust);
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(13, ValueProp.Move),
        new IntVar("Loot", 7),
    ];

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(2);

    protected override async Task ApplyEffect(PlayerChoiceContext ctx, CardPlay play, bool heads)
    {
        var aliveBefore = CombatState.Enemies.Count(e => e.IsAlive);

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, play)
            .TargetingAllOpponents(CombatState)
            .WithHitFx("vfx/vfx_attack_blunt")
            .Execute(ctx);

        var kills = aliveBefore - CombatState.Enemies.Count(e => e.IsAlive);
        if (kills > 0)
            await PlayerCmd.GainGold(DynamicVars["Loot"].BaseValue * kills, Owner, false);

        // 永久成长：先按当前值结算，再涨。BaseValue/PreviewValue 同步，卡面立即可见。
        var dmg = DynamicVars.Damage;
        dmg.BaseValue += GrowthPerPlay;
        dmg.PreviewValue = dmg.BaseValue;
    }

    public override List<(string, string)>? Localization => new CardLoc(
        "清台",
        "#花费 *2 金币*。\n对所有敌人造成 !D! 伤害。每*击杀*一个敌人，获得 !Loot! 金币。每打出一次，伤害永久 +4。");
}
