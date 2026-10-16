using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;

namespace GamblerMod.Core;

// 掷硬币管线（技术设计 §4.2，M4 完整版）：
//   掷骰（正面率聚合）→ 干预窗口 → 定案 → 记录。
// 干预顺序（固定，勿改）：赌神(全正) > 必正层数(暗记持久层 + 出千回合层) > 内容层干预(Intervenor，幸运筹码)。
// 随机源 = 每战斗一个 Rng(player, characterId, combatMixin)——官方"内容专属 RNG"构造器。
// 记录按"整场战斗"存续（战斗结束清空；满贯消费"本场正面数"，徽章正反也不跨战斗残留）。
// 无重掷 API（评审 D4 已定）；容器跨战斗复用（红线 R3）。
public static class GambleService
{
    public const int BaseRatePercent = 50;
    public const int RateCapPercent = 75; // 设计 §2.2：正面率加法上限 75%（"必为正面"无视上限）

    public readonly record struct FlipRecord(CardModel Source, bool OriginalHeads, bool FinalHeads);

    /// <summary>定案为正面时触发（赌场规则等消费；静态方法组订阅，红线 R5）。</summary>
    public static event Action<Player>? OnHeads;

    private static readonly List<FlipRecord> Flips = new();
    private static object? _combat;
    private static int _combatCount = -1;
    private static Rng? _rng;
    private static int _forcedHeads;         // 暗记必正层数，跨回合存续直到消耗
    private static int _forcedHeadsThisTurn; // 出千本回合必正层数，回合开始清零（OnTurnStart）

    /// <summary>内容层干预窗口（幸运筹码在 Entry 注册）：反面后、结算前调用，返回 true = 已改为正面。</summary>
    public static Func<Player, Task<bool>>? Intervenor;

    public static void ForceNextFlips(int count) => _forcedHeads += count;

    /// <summary>出千：本回合接下来的 N 次赌博必为正面（回合开始清零，与暗记的持久层数分开记账）。</summary>
    public static void ForceNextFlipsThisTurn(int count) => _forcedHeadsThisTurn += count;

    /// <summary>回合开始：清空本回合必正层数（GamblerHooks.AfterPlayerTurnStart 调用）。</summary>
    public static void OnTurnStart() => _forcedHeadsThisTurn = 0;

    public static async Task<bool> Flip(Player player, CardModel source)
    {
        ResetIfNewCombat(source.CombatState);
        _rng ??= new Rng(player, player.Character.Id, (ulong)_combatCount);

        var original = Roll(player);
        var final = original;

        // 干预①：必正层数——先扣"本回合"层（出千），再扣持久层（暗记）；无视概率上限
        if (!final && _forcedHeadsThisTurn + _forcedHeads > 0)
        {
            if (_forcedHeadsThisTurn > 0) _forcedHeadsThisTurn--;
            else _forcedHeads--;
            final = true;
            GD.Print($"[GamblerMod] 做局必正：剩余 {_forcedHeadsThisTurn + _forcedHeads} 次");
        }

        // 干预②：内容层干预（幸运筹码等）——掷出反面后、结算前询问（Intervenor 由 Entry 注册）
        var intervened = false;
        if (!final && Intervenor != null && await Intervenor(player))
        {
            final = true;
            intervened = true;
        }

        // 「风头正盛」倍率（设计 §2.8，2026-10-16 用户拍板）：倍率只认**赌博本身**的结果——
        // 掷出反面就归零，**哪怕幸运筹码花钱把它改成了正面**（遗物只保牌效，不保连胜）；
        // 赌神 / 暗记 / 出千属于"做局换确定性"，照常 +0.5。
        if (!final || intervened) MultiplierService.Reset();
        else MultiplierService.AddHeads();

        Flips.Add(new FlipRecord(source, original, final));
        GD.Print($"[GamblerMod] 掷硬币 {source.Id.Entry}：{(original ? "正面" : "反面")}{(final != original ? " → 改为正面" : "")}（风头 {MultiplierService.Label}{(intervened ? "，改币不计" : "")}）");

        if (final) OnHeads?.Invoke(player);

        return final;
    }

    // 正面率 = 50 + Σ(手气正旺层数)，加法上限 75（设计 §2.2）
    public static int HeadsRatePercent(Player player)
    {
        var bonus = 0;
        var hotStreak = player.Creature.GetPower<Powers.HotStreakPower>();
        if (hotStreak != null) bonus += (int)hotStreak.Amount;
        return Math.Min(BaseRatePercent + bonus, RateCapPercent);
    }

    public static IReadOnlyList<FlipRecord> Records => Flips;

    /// <summary>战斗结束：清空掷骰记录/RNG/必正层数（否则新战斗开局徽章会残留上一场的正反）。</summary>
    public static void OnCombatEnd()
    {
        Flips.Clear();
        _rng = null;
        _forcedHeads = 0;
        _forcedHeadsThisTurn = 0;
    }

    /// <summary>本场战斗定案正面次数（满贯消费）。</summary>
    public static int HeadsCountThisCombat
    {
        get
        {
            var count = 0;
            for (var i = 0; i < Flips.Count; i++)
                if (Flips[i].FinalHeads) count++;
            return count;
        }
    }

    /// <summary>最近一次定案结果；当前无消费者，属 M1 冻结接口按 §4.8② 保留（v2 清理时一并移除）。</summary>
    public static bool? LastFlipHeads => Flips.Count > 0 ? Flips[^1].FinalHeads : null;

    private static bool Roll(Player player)
    {
        var god = player.Creature.GetPower<Powers.GamblingGodPower>();
        if (god != null) return true;
        var rate = HeadsRatePercent(player);
        return _rng!.NextDouble() * 100 < rate;
    }

    private static void ResetIfNewCombat(object combatIdentity)
    {
        if (ReferenceEquals(_combat, combatIdentity)) return;
        _combat = combatIdentity;
        _combatCount++;
        Flips.Clear();
        _rng = null;
        _forcedHeads = 0;
        _forcedHeadsThisTurn = 0;
    }
}
