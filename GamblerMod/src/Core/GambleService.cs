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
// 干预顺序（固定，勿改）：赌神(全正) > 暗记(必正,消耗层数) > 幸运筹码(反面后花5金改正面,每回合1次)。
// 随机源 = 每战斗一个 Rng(player, characterId, combatMixin)——官方"内容专属 RNG"构造器。
// 记录按"回合"清空（同花顺/底牌消费本回合正面数；GamblerHooks.AfterPlayerTurnStart 兜底重置）。
// 无重掷 API（评审 D4 已定）；容器跨战斗/回合复用（红线 R3）。
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
    private static int _round = -1;
    private static Rng? _rng;
    private static int _forcedHeads; // 暗记层数，跨回合存续直到消耗

    public static void ForceNextFlips(int count) => _forcedHeads += count;

    public static async Task<bool> Flip(Player player, CardModel source)
    {
        ResetIfNewCombat(source.CombatState);
        _rng ??= new Rng(player, player.Character.Id, (ulong)_combatCount);

        var original = Roll(player);
        var final = original;

        // 干预①：暗记——下一次赌博必为正面（无视概率上限，消耗层数）
        if (!final && _forcedHeads > 0)
        {
            _forcedHeads--;
            final = true;
            GD.Print("[GamblerMod] 暗记：必为正面");
        }

        // 干预②：幸运筹码——掷出反面后花 5 金改为正面（每回合 1 次；M2 简化：金币足够时自动触发）
        var byChip = false;
        if (!final && await Relics.LuckyChip.TryIntervene(player))
        {
            final = true;
            byChip = true;
        }

        // 「风头正盛」倍率（设计 §2.8）：自然正面 / 赌神 / 暗记 → +0.5；掷出反面 → 立刻归零；
        // 幸运筹码改币**只保命不涨**（用户拍板 Q4）——用 byChip 把它排除在 +0.5 之外（但也别归零）。
        if (!final)
        {
            MultiplierService.Reset();
        }
        else if (byChip)
        {
            MultiplierService.MarkSavedByChip();
        }
        else
        {
            MultiplierService.AddHeads();
        }

        Flips.Add(new FlipRecord(source, original, final));
        GD.Print($"[GamblerMod] 掷硬币 {source.Id.Entry}：{(original ? "正面" : "反面")}{(final != original ? " → 改为正面" : "")}（风头 {MultiplierService.Label}{(byChip ? "，改币不计" : "")}）");

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
        _round = -1;
    }

    /// <summary>本回合定案正面次数（同花顺/底牌消费）。</summary>
    public static int HeadsCountThisTurn
    {
        get
        {
            var count = 0;
            for (var i = 0; i < Flips.Count; i++)
                if (Flips[i].FinalHeads) count++;
            return count;
        }
    }

    /// <summary>最近一次定案结果（叠注消费）；无记录时返回 null。</summary>
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
        _round = -1;
        Flips.Clear();
        _rng = null;
        _forcedHeads = 0;
    }
}
