using System.Threading.Tasks;
using BaseLib.Abstracts;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace GamblerMod.Core;

// 全局钩子单例（技术设计 §4.5 的战斗状态仓，M4 落地）：
//   - 回合开始：SpendTracker 清零（连续下注按回合计）+ 出千本回合必正层数清零
//   - 战斗结束：金币费用/掷骰记录等战斗态复位
//   - 利滚利：每次获得金币 +2/+3（防递归标记）
// 实例化即订阅（CustomSingletonModel(HookType.Combat) 语义），在 Entry 中创建。
// 保护费"正面 +3 金"已移至 Powers.HouseRulesPower（内容层订阅 GambleService.OnHeads）——
// Core 不再持有任何卡牌专属数值（约束④）。
public sealed class GamblerHooks : CustomSingletonModel
{
    private static bool _inGoldGain;   // 利滚利防递归

    public GamblerHooks() : base(HookType.Combat)
    {
        if (_subscribed) return; // 初始化重试时防重复订阅（ModHelper 按幂等 id 拒绝二次订阅）
        _subscribed = true;
    }

    private static bool _subscribed;

    public override Task AfterPlayerTurnStart(PlayerChoiceContext ctx, Player player)
    {
        SpendTracker.OnTurnStart();
        GambleService.OnTurnStart(); // 出千的本回合必正层数清零

        return Task.CompletedTask;
    }

    public override Task AfterCombatEnd(MegaCrit.Sts2.Core.Rooms.CombatRoom room)
    {
        GoldCost.ResetForNextCombat();
        SpendTracker.OnCombatStart();
        MultiplierService.Reset(); // 「风头正盛」倍率：战斗结束清空（设计 §2.8）
        GambleService.OnCombatEnd(); // 掷骰记录一并清空（徽章的正/反不该跨战斗残留）
        return Task.CompletedTask;
    }

    // ===== 「风头正盛」倍率乘算（技术设计 gambler-tech-multiplier §3.3，§4.8 门禁：只加不改）=====
    // 热路径红线：这两个方法在每次伤害/格挡结算**以及每次卡面预览刷新**时都会调用，
    // 所以实现必须零分配、零 LINQ——词条判定用 GamblerCard 上的 bool 字段，绝不比字符串。
    // 卡面数字会跟着倍率实时变化（BlockVar/DamageVar 的预览同样走这两个钩子）。
    public override decimal ModifyDamageMultiplicative(Creature target, decimal amount, ValueProp props,
        Creature dealer, CardModel cardSource, CardPlay cardPlay)
        => IsStreakSource(cardSource) ? MultiplierService.Factor : 1m;

    public override decimal ModifyBlockMultiplicative(Creature target, decimal block, ValueProp props,
        CardModel cardSource, CardPlay cardPlay)
        => IsStreakSource(cardSource) ? MultiplierService.Factor : 1m;

    // 金币不走钩子：ModifyGoldGained 不带 cardSource，无法判断是哪张牌 → 金币类兑现牌在卡内自算（见清账）。
    private static bool IsStreakSource(CardModel? cardSource)
        => cardSource is GamblerCard { IsHeadsStreakCard: true };

    public override async Task AfterGoldGained(Player player)
    {
        // 利滚利：每次获得金币时额外 +Amount（层数），防递归
        if (_inGoldGain) return;
        var compound = player.Creature.GetPower<Powers.CompoundInterestPower>();
        if (compound == null || compound.Amount <= 0) return;

        _inGoldGain = true;
        try
        {
            // 守卫必须覆盖整个异步过程：金币入账发生在命令执行层（Task 完成前）。
            // 若在 finally 里复位、不等 Task 结束，递归窗口会再次敞开（2026-10-16 审查修复）。
            await PlayerCmd.GainGold(compound.Amount, player, false);
        }
        finally
        {
            _inGoldGain = false;
        }
    }
}
