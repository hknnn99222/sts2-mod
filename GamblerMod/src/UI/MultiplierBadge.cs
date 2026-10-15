using System;
using BaseLib.Utils;
using GamblerMod.Core;
using Godot;
using MegaCrit.Sts2.addons.mega_text;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace GamblerMod.UI;

// 「风头正盛」倍率徽章（技术设计 gambler-tech-multiplier §3.6）：
// 用 BaseLib AddedNode 给所有 NCreature 挂一个纯文字标签，只在"玩家生物"上显示（NCreature.Entity.IsPlayer）。
// 徽章 = 左侧「倍率 x2.5」+ 右侧「最近一次掷币结果 正/反」（用户 2026-10-16 要求）。
//
// ⚠⚠⚠ 三个实测踩到的坑（代码里都有注释，别删）：
//  1) `MegaLabel._Ready()` 里的 `AssertThemeFontOverride` 缺字体覆盖时**抛异常**（不是打日志），
//     调在最前面会让后面代码全跳过 → 整块消失。所以先 AddThemeFontOverride 再 try/catch base._Ready()。
//  2) **刷新不能依赖本节点的 `_Process`**：C# 节点即使 `SetProcess(true)` 也没被引擎回调
//     （2026-10-16 两次实测：日志只有「已挂载」、没有「_Process 已启动」）。
//     现在改挂 **SceneTree.ProcessFrame 信号**——信号连接不受 process 标志控制，稳。
//  3) `NCreature.GetTopOfHitbox()` 返回**全局坐标**（IL 核实：Hitbox.GlobalPosition + 半宽），
//     所以写徽章自己的 GlobalPosition；只在目标点/文本变化时才写（无分配，守 R4/R7）。
public sealed class MultiplierBadge : MegaLabel
{
    private const float HeadOffsetY = -58f; // ← 校准点（像素，负值 = 往上）
    private const float BadgeWidth = 76f;   // 倍率文字框
    private const float FlipWidth = 44f;    // 正/反文字框
    private const float Gap = 4f;
    private const float BoxHeight = 34f;
    private static readonly StringName FontItem = "font";

    private static readonly Color HeadsColor = new("e8c34a"); // 金（同角色名色）
    private static readonly Color TailsColor = new("d95b5b"); // 红
    private static readonly Color IdleColor = new(1f, 1f, 1f, 0.45f);

    /// <summary>挂钩注册：静态字段初始化即向 BaseLib 登记（Entry.Initialize 里 touch 一下）。</summary>
    public static readonly AddedNode<NCreature, MultiplierBadge> Attached =
        new(parent => new MultiplierBadge());

    private NCreature? _parent;
    private MegaLabel? _flipLabel;
    private SceneTree? _tree;
    private bool _isPlayer;
    private bool _sawFirstFrame;
    private int _lastSteps = int.MinValue;
    private int _lastFlipCount = -1;
    private Vector2 _lastTarget = new(float.NaN, float.NaN);

    public override void _Ready()
    {
        _parent = GetParent() as NCreature;
        var entity = _parent?.Entity;
        if (_parent == null || entity == null || !entity.IsPlayer)
        {
            // 非玩家生物：不初始化字体、不挂正反标签、不接帧回调（也免掉 MegaLabel 那次字体断言）
            _isPlayer = false;
            Visible = false;
            return;
        }

        // ① 先备好主题字体覆盖，让 MegaLabel._Ready 的断言通过
        var font = GetThemeFont(FontItem, "Label") ?? ThemeDB.FallbackFont;
        if (font != null) AddThemeFontOverride(FontItem, font);

        // ② 再调 base._Ready()（RefreshFont + AdjustFontSize）；万一仍抛也别让整块消失
        try
        {
            base._Ready();
        }
        catch (Exception e)
        {
            GD.PrintErr($"[GamblerMod] 徽章 base._Ready 抛异常（已吞掉继续渲染）：{e.Message}");
        }

        _isPlayer = true;
        MouseFilter = MouseFilterEnum.Ignore; // 别挡住点击
        SetProcess(true);                     // 留着无害；实际刷新走下面的 ProcessFrame 信号
        Size = new Vector2(BadgeWidth, BoxHeight);
        HorizontalAlignment = HorizontalAlignment.Center;
        VerticalAlignment = VerticalAlignment.Center;

        // ③ 右侧的掷币结果指示（同样先给字体覆盖，免得它自己的 _Ready 再抛）
        _flipLabel = new MegaLabel
        {
            Position = new Vector2(BadgeWidth + Gap, 0f),
            Size = new Vector2(FlipWidth, BoxHeight),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            MouseFilter = MouseFilterEnum.Ignore,
        };
        if (font != null) _flipLabel.AddThemeFontOverride(FontItem, font);
        AddChild(_flipLabel);

        // ④ 刷新驱动：SceneTree.ProcessFrame 信号（不依赖本节点 process 标志）
        _tree = GetTree();
        if (_tree != null) _tree.ProcessFrame += OnFrame;

        Refresh(MultiplierService.Steps);
        RefreshFlip(force: true);
        SyncPosition();
        GD.Print($"[GamblerMod] 风头徽章已挂载：全局锚点 {_parent.GetTopOfHitbox()}，偏移 {HeadOffsetY}，初始 {MultiplierService.Label}，帧驱动={(_tree != null ? "ProcessFrame 信号" : "无")}");
    }

    public override void _ExitTree()
    {
        if (_tree != null)
        {
            _tree.ProcessFrame -= OnFrame;
            _tree = null;
        }
    }

    /// <summary>每帧（SceneTree 信号）：只在倍率刻度或掷骰条数变化时才重建文本。</summary>
    private void OnFrame()
    {
        if (!_isPlayer) return;

        if (!_sawFirstFrame)
        {
            _sawFirstFrame = true;
            GD.Print("[GamblerMod] 徽章帧回调已启动（ProcessFrame 信号）");
        }

        if (MultiplierService.Steps != _lastSteps)
            Refresh(MultiplierService.Steps);

        RefreshFlip();
        SyncPosition();
    }

    private void Refresh(int steps)
    {
        _lastSteps = steps;
        SetTextAutoSize(MultiplierService.LabelOf(steps)); // 只在刻度变化时重建文本（红线 R4）
        GD.Print($"[GamblerMod] 徽章刷新 → {MultiplierService.LabelOf(steps)}");
    }

    /// <summary>
    /// 显示"最近一次**计入倍率**的判定"（用户 2026-10-16 定：只显示赌博那一掷，遗物改币不算）——
    /// 自然正面/赌神/暗记 → 正；反面 → 反；幸运筹码改币 → 反（保命但不涨，和倍率口径一致）。
    /// 记录按**整场战斗**（GamblerHooks 战斗结束时清空），所以新战斗自动回到 —。
    /// </summary>
    private void RefreshFlip(bool force = false)
    {
        if (_flipLabel == null) return;

        var count = GambleService.Records.Count;
        if (!force && count == _lastFlipCount) return;
        _lastFlipCount = count;

        if (count == 0)
        {
            _flipLabel.SetTextAutoSize("—");
            _flipLabel.Modulate = IdleColor; // 本场还没掷过
            return;
        }

        var counted = MultiplierService.LastFlipCounted;
        _flipLabel.SetTextAutoSize(counted ? "正" : "反");
        _flipLabel.Modulate = counted ? HeadsColor : TailsColor;
    }

    private void SyncPosition()
    {
        var top = _parent!.GetTopOfHitbox();
        // 让「倍率 + 正反」这一组的**中心**落在头顶中线上
        var totalWidth = BadgeWidth + Gap + FlipWidth;
        var target = new Vector2(top.X - totalWidth * 0.5f, top.Y + HeadOffsetY);
        if (target == _lastTarget) return;
        _lastTarget = target;
        GlobalPosition = target;
    }
}
