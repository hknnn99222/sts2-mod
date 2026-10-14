using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;

namespace GamblerMod.Core;

// 掷硬币管线（技术设计 §4.2，M2 完整版）：
//   掷骰（正面率聚合）→ 干预窗口 → 定案 → 记录。
// 干预顺序（固定，勿改）：赌神(全正) > 暗记(必正,消耗层数) > 幸运筹码(反面后花5金改正面,每回合1次)。
// 随机源 = 每战斗一个 Rng(player, characterId, combatMixin)——官方"内容专属 RNG"构造器；
// mixin 用战斗序号：官方文档明示"同内容再见会出同结果"，必须逐战斗区分。
// 无重掷 API（评审 D4 已定）；记录容器跨战斗复用（红线 R3）。
public static class GambleService
{
    public const int BaseRatePercent = 50;
    public const int RateCapPercent = 75; // 设计 §2.2：正面率加法上限 75%（"必为正面"无视上限）

    public readonly record struct FlipRecord(CardModel Source, bool OriginalHeads, bool FinalHeads);

    private static readonly List<FlipRecord> Flips = new();
    private static object? _combat;
    private static int _combatCount = -1;
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
        if (!final && await Relics.LuckyChip.TryIntervene(player))
        {
            final = true;
        }

        Flips.Add(new FlipRecord(source, original, final));
        GD.Print($"[GamblerMod] 掷硬币 {source.Id.Entry}：{(original ? "正面" : "反面")}{(final != original ? " → 改为正面" : "")}");
        return final;
    }

    // 正面率 = 50 + Σ(手气正旺层数)，加法上限 75（设计 §2.2）
    public static int HeadsRatePercent(Player player)
    {
        var bonus = 0;
        var hotStreak = player.Creature.GetPower<Powers.HotStreakPower>();
        if (hotStreak != null) bonus += (int)hotStreak.Amount;
        return System.Math.Min(BaseRatePercent + bonus, RateCapPercent);
    }

    public static IReadOnlyList<FlipRecord> Records(object combatIdentity)
    {
        ResetIfNewCombat(combatIdentity);
        return Flips;
    }

    public static int HeadsCount(object combatIdentity)
    {
        var records = Records(combatIdentity);
        var count = 0;
        for (var i = 0; i < records.Count; i++)
            if (records[i].FinalHeads) count++;
        return count;
    }

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
    }
}
