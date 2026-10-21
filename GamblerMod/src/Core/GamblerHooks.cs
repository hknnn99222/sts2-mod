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
//   - 存钱：每次获得金币 → 等量格挡（转发给 Powers.ShowdownPower）
// 实例化即订阅（CustomSingletonModel(HookType.Combat) 语义）。
// ⚠️ **不要在 Entry 里 new 本类**（2026-10-21 修）：BaseLib 会在 ModelDb.Init 自动实例化
// 所有 CustomSingletonModel 子类——Entry 再 new 一次 = ModHelper 拒绝第二次订阅并打 ERROR
// （游戏日志实测：Tried to subscribe ... GAMBLERMOD-GAMBLER_HOOKS, already been used）。
// ⚠️ **run 级金币钩子只派发给订阅单例（IterateHookListeners），power 收不到**——
// 需要金币触发的 power 效果必须在这里转发（存钱/利滚利同款；数值留在 power 文件，约束④）。
public sealed class GamblerHooks : CustomSingletonModel
{
    private static bool _inGoldGain;   // 利滚利防递归

    // 显式无参构造器是 BaseLib ModelDb.Init 反射实例化的入口（CreateInstanceDefaultCtor）
    public GamblerHooks() : base(HookType.Combat) { }

    // ⚠️ 存钱的金额版钩子：在金币**入账前**拿到这一笔的最终数额
    public override Task AfterModifyingGoldGained(Player player, decimal amount)
    {
        if (amount <= 0) return Task.CompletedTask;
        return Powers.ShowdownPower.OnGoldGained(player, amount);
    }

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
