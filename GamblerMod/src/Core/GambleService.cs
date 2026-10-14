using System.Collections.Generic;
using Godot;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;

namespace GamblerMod.Core;

// 掷硬币管线（技术设计 §4.2）：掷骰 → 做局干预窗口（M2 接幸运筹码/暗记/赌神）→ 定案 → 记录。
// 随机源 = 每战斗一个 Rng(player, characterId, combatMixin)——官方"内容专属 RNG"构造器，
// 按 局种子+玩家槽位+内容ID 无状态派生（多人隔离、回放友好，红线 R6）；
// mixin 用战斗序号：官方文档明示"同内容再见会出同结果"，必须逐战斗区分。
// 已知取舍：战斗序号不进存档，读档后新战斗可能复现旧战斗的硬币序列（单机无感，多人留 M4 校准）。
// 无重掷 API（评审 D4 已定）；记录容器跨战斗复用（红线 R3）。
public static class GambleService
{
    public readonly record struct FlipRecord(CardModel Source, bool OriginalHeads, bool FinalHeads);

    private static readonly List<FlipRecord> Flips = new();
    private static object? _combat;
    private static int _combatCount = -1;
    private static Rng? _rng;

    public static bool Flip(Player player, CardModel source)
    {
        ResetIfNewCombat(source.CombatState);

        _rng ??= new Rng(player, player.Character.Id, (ulong)_combatCount);

        var original = _rng.NextBool();
        var final = original;
        // M2 做局干预窗口在此插入：赌神(全正) > 暗记(必正,消耗层数) > 幸运筹码(反面时花5金改正面)

        Flips.Add(new FlipRecord(source, original, final));
        GD.Print($"[GamblerMod] 掷硬币 {source.Id.Entry}：{(final ? "正面" : "反面")}");
        return final;
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

    private static void ResetIfNewCombat(object combatIdentity)
    {
        if (ReferenceEquals(_combat, combatIdentity)) return;
        _combat = combatIdentity;
        _combatCount++;
        Flips.Clear();
        _rng = null;
    }
}
