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
//   - 回合开始：SpendTracker 清零（连续下注按回合计）
//   - 战斗结束：金币费用/掷骰记录等战斗态复位
//   - 高利贷：下场战斗首次回合开始时扣款（M4 简化：不做区分债务人的多人校准）
//   - 利滚利：每次获得金币 +2/+3（防递归标记）
//   - 赌场规则：每次掷出正面 +3 金（订阅 GambleService.OnHeads，静态方法组 R5）
// 实例化即订阅（CustomSingletonModel(HookType.Combat) 语义），在 Entry 中创建。
public sealed class GamblerHooks : CustomSingletonModel
{
    public static int PendingLoanDebt; // 高利贷：待扣欠款
    private static bool _inGoldGain;   // 利滚利防递归

    public GamblerHooks() : base(HookType.Combat)
    {
        if (_subscribed) return; // 初始化重试时防重复订阅（ModHelper 按幂等 id 拒绝二次订阅）
        _subscribed = true;
        GambleService.OnHeads += OnHeadsFlipped;
    }

    private static bool _subscribed;

    public override Task AfterPlayerTurnStart(PlayerChoiceContext ctx, Player player)
    {
        SpendTracker.OnTurnStart();

        // 高利贷欠款：下场战斗的首次回合开始时结算
        if (PendingLoanDebt > 0)
        {
            var debt = PendingLoanDebt;
            PendingLoanDebt = 0;
            var actual = System.Math.Min(debt, player.Gold);
            if (actual > 0)
            {
                GD.Print($"[GamblerMod] 高利贷：战斗开始扣除 {actual} 金币");
                return PlayerCmd.LoseGold(actual, player, MegaCrit.Sts2.Core.Entities.Gold.GoldLossType.Lost);
            }
        }

        return Task.CompletedTask;
    }

    public override Task AfterCombatEnd(MegaCrit.Sts2.Core.Rooms.CombatRoom room)
    {
        GoldCost.OnCombatStart();
        SpendTracker.OnCombatStart();
        MultiplierService.Reset(); // 「风头正盛」倍率：战斗结束清空（设计 §2.8）
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

    public override Task AfterGoldGained(Player player)
    {
        // 利滚利：每次获得金币时额外 +Amount（层数），防递归
        if (_inGoldGain) return Task.CompletedTask;
        var compound = player.Creature.GetPower<Powers.CompoundInterestPower>();
        if (compound == null || compound.Amount <= 0) return Task.CompletedTask;

        _inGoldGain = true;
        try
        {
            return PlayerCmd.GainGold(compound.Amount, player, true);
        }
        finally
        {
            _inGoldGain = false;
        }
    }

    private static void OnHeadsFlipped(Player player)
    {
        // 赌场规则：每次掷出正面 +2 金（固定值，升级只提升回合定额）
        if (player.Creature.GetPower<Powers.HouseRulesPower>() == null) return;
        _ = PlayerCmd.GainGold(3, player, true);
    }
}
