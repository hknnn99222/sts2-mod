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
// ⚠⚠ 两个实测踩到的坑（都写在代码里了，别删）：
//  1) `MegaLabel._Ready()` 里有 `AssertThemeFontOverride(this, ThemeConstants.Label.Font)`，
//     缺主题字体覆盖时它**抛异常**（不是打日志！）。2026-10-16：因为把它调在最前面，
//     异常一抛后面设文字的代码全被跳过 → 徽章整块消失（"啥也没有 倍率也没有了"）。
//     现在：先 `AddThemeFontOverride("font", …)` 让断言通过，再 try/catch 调 base._Ready()。
//  2) `NCreature.GetTopOfHitbox()` 返回的是**全局坐标**（IL 核实：Hitbox.GlobalPosition + 半宽），
//     所以直接写徽章自己的 GlobalPosition；每帧只在目标点/文本变化时才写（无分配，守红线 R4/R7）。
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
    private Font? _font;
    private bool _isPlayer;
    private bool _sawFirstProcess;
    private int _lastSteps = int.MinValue;
    private int _lastFlipCount = -1;
    private Vector2 _lastTarget = new(float.NaN, float.NaN);

    public override void _Ready()
    {
        _parent = GetParent() as NCreature;
        var entity = _parent?.Entity;
        if (_parent == null || entity == null || !entity.IsPlayer)
        {
            // 非玩家生物：不初始化字体、不挂正反标签（也免掉 MegaLabel 那次字体断言）
            _isPlayer = false;
            Visible = false;
            return;
        }

        // ① 先备好主题字体覆盖：MegaLabel._Ready 的断言要求"本节点自己有字体覆盖"
        _font = GetThemeFont(FontItem, "Label") ?? ThemeDB.FallbackFont;
        if (_font != null) AddThemeFontOverride(FontItem, _font);

        // ② 再调 base._Ready()（它做 RefreshFont + AdjustFontSize）；万一仍抛也别让整块消失
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
        if (_font != null) _flipLabel.AddThemeFontOverride(FontItem, _font);
        AddChild(_flipLabel);

        Refresh(MultiplierService.Steps);
        RefreshFlip(force: true);
        SyncPosition();
        GD.Print($"[GamblerMod] 风头徽章已挂载：全局锚点 {_parent.GetTopOfHitbox()}，偏移 {HeadOffsetY}，初始 {MultiplierService.Label}");
    }

    public override void _Process(double delta)
    {
        if (!_isPlayer) return;

        if (!_sawFirstProcess)
        {
            _sawFirstProcess = true;
            GD.Print("[GamblerMod] 风头徽章 _Process 已启动（每帧刷新倍率/掷币结果）");
        }

        if (MultiplierService.Steps != _lastSteps)
            Refresh(MultiplierService.Steps);

        RefreshFlip();
        SyncPosition(); // 生物位置/动画变化时跟着走
    }

    private void Refresh(int steps)
    {
        _lastSteps = steps;
        SetTextAutoSize(MultiplierService.LabelOf(steps)); // 只在刻度变化时重建文本（红线 R4）
        GD.Print($"[GamblerMod] 徽章刷新 → {MultiplierService.LabelOf(steps)}");
    }

    /// <summary>把"最近一次定案结果"同步成 正/反（用掷骰条数当版本号，变化才写）。</summary>
    private void RefreshFlip(bool force = false)
    {
        if (_flipLabel == null) return;

        var count = GambleService.Records.Count;
        if (!force && count == _lastFlipCount) return;
        _lastFlipCount = count;

        var last = GambleService.LastFlipHeads;
        if (last == null)
        {
            _flipLabel.SetTextAutoSize("—");
            _flipLabel.Modulate = IdleColor; // 本场还没掷过
            return;
        }

        var heads = last.Value;
        _flipLabel.SetTextAutoSize(heads ? "正" : "反");
        _flipLabel.Modulate = heads ? HeadsColor : TailsColor;
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
