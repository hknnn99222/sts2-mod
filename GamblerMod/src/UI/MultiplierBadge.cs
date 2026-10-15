using BaseLib.Utils;
using GamblerMod.Core;
using Godot;
using MegaCrit.Sts2.addons.mega_text;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace GamblerMod.UI;

// 「风头正盛」倍率徽章（技术设计 gambler-tech-multiplier §3.6）：
// 用 BaseLib AddedNode 给所有 NCreature 挂一个纯文字标签，只在"玩家生物"上显示（NCreature.Entity.IsPlayer）。
// 徽章 = 左侧「倍率 x2.5」+ 右侧「最近一次掷币结果 正/反」（用户 2026-10-16 要求）。
// 美术后补：v1 只显示文字。
//
// ⚠ 关键：MegaLabel._Ready() 里做的是 RefreshFont() + AdjustFontSize()（IL 核实）。
//    覆盖 _Ready 时**必须**调 base._Ready()，否则字体不初始化、文字根本不显示（2026-10-16 实测踩过）。
//
// 定位（IL 核实）：NCreature.GetTopOfHitbox() 返回**全局坐标**（Hitbox.GlobalPosition + 半宽），
// 所以直接写徽章自己的 GlobalPosition，不做坐标换算；每帧只在目标点/文本变化时才写（无分配）。
public sealed class MultiplierBadge : MegaLabel
{
    private const float HeadOffsetY = -58f; // ← 校准点（像素，负值 = 往上）
    private const float BadgeWidth = 76f;   // 倍率文字框
    private const float FlipWidth = 44f;    // 正/反文字框
    private const float Gap = 4f;
    private const float BoxHeight = 34f;

    private static readonly Color HeadsColor = new("e8c34a"); // 金（同角色名色）
    private static readonly Color TailsColor = new("d95b5b"); // 红

    /// <summary>挂钩注册：静态字段初始化即向 BaseLib 登记（Entry.Initialize 里 touch 一下）。</summary>
    public static readonly AddedNode<NCreature, MultiplierBadge> Attached =
        new(parent => new MultiplierBadge());

    private NCreature? _parent;
    private MegaLabel? _flipLabel;
    private bool _isPlayer;
    private bool _sawFirstProcess;
    private int _lastSteps = int.MinValue;
    private int _lastFlipCount = -1;
    private Vector2 _lastTarget = new(float.NaN, float.NaN);

    public override void _Ready()
    {
        base._Ready(); // ← 不可省：MegaLabel 的字体初始化在这里

        _parent = GetParent() as NCreature;
        var entity = _parent?.Entity;
        if (_parent == null || entity == null || !entity.IsPlayer)
        {
            _isPlayer = false;
            Visible = false;
            return;
        }

        _isPlayer = true;
        MouseFilter = MouseFilterEnum.Ignore; // 别挡住点击
        Size = new Vector2(BadgeWidth, BoxHeight);
        HorizontalAlignment = HorizontalAlignment.Center;
        VerticalAlignment = VerticalAlignment.Center;

        // 右侧的掷币结果指示（AddChild 后引擎会自己跑它的 _Ready → 字体正常）
        _flipLabel = new MegaLabel
        {
            Position = new Vector2(BadgeWidth + Gap, 0f),
            Size = new Vector2(FlipWidth, BoxHeight),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            MouseFilter = MouseFilterEnum.Ignore,
        };
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
            _flipLabel.Modulate = new Color(1f, 1f, 1f, 0.45f); // 本场还没掷过
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
