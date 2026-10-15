using BaseLib.Utils;
using GamblerMod.Core;
using Godot;
using MegaCrit.Sts2.addons.mega_text;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace GamblerMod.UI;

// 「风头正盛」倍率徽章（技术设计 gambler-tech-multiplier §3.6）：
// 用 BaseLib AddedNode 给所有 NCreature 挂一个纯文字标签，只在"玩家生物"上显示（NCreature.Entity.IsPlayer）。
// 美术后补：v1 只显示 x1.5 这样的文字。
//
// ⚠ 关键：MegaLabel._Ready() 里做的是 RefreshFont() + AdjustFontSize()（IL 核实）。
//    覆盖 _Ready 时**必须**调 base._Ready()，否则字体不初始化、文字根本不显示（2026-10-16 实测踩过）。
//
// 定位（IL 核实）：NCreature.GetTopOfHitbox() 返回**全局坐标**（Hitbox.GlobalPosition + 半宽），
// 所以直接写徽章自己的 GlobalPosition，不做坐标换算；每帧只在目标点变化时才写（无分配）。
public sealed class MultiplierBadge : MegaLabel
{
    private const float HeadOffsetY = -58f; // ← 校准点（像素，负值 = 往上）；2026-10-16 由 -34 上调 24px
    private const float BoxWidth = 96f;     // 文本自动缩放按这个盒子算，太大字会飞
    private const float BoxHeight = 34f;

    /// <summary>挂钩注册：静态字段初始化即向 BaseLib 登记（Entry.Initialize 里 touch 一下）。</summary>
    public static readonly AddedNode<NCreature, MultiplierBadge> Attached =
        new(parent => new MultiplierBadge());

    private NCreature? _parent;
    private bool _isPlayer;
    private bool _sawFirstProcess;
    private int _lastSteps = int.MinValue;
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
        Size = new Vector2(BoxWidth, BoxHeight);
        HorizontalAlignment = HorizontalAlignment.Center;
        VerticalAlignment = VerticalAlignment.Center;

        Refresh(MultiplierService.Steps);
        SyncPosition();
        GD.Print($"[GamblerMod] 风头徽章已挂载：全局锚点 {_parent.GetTopOfHitbox()}，偏移 {HeadOffsetY}，初始 {MultiplierService.Label}");
    }

    public override void _Process(double delta)
    {
        if (!_isPlayer) return;

        if (!_sawFirstProcess)
        {
            _sawFirstProcess = true;
            GD.Print("[GamblerMod] 风头徽章 _Process 已启动（每帧刷新倍率）");
        }

        if (MultiplierService.Steps != _lastSteps)
            Refresh(MultiplierService.Steps);

        SyncPosition(); // 生物位置/动画变化时跟着走
    }

    private void Refresh(int steps)
    {
        _lastSteps = steps;
        SetTextAutoSize(MultiplierService.LabelOf(steps)); // 只在刻度变化时重建文本（红线 R4）
        GD.Print($"[GamblerMod] 徽章刷新 → {MultiplierService.LabelOf(steps)}");
    }

    private void SyncPosition()
    {
        var top = _parent!.GetTopOfHitbox();
        var target = new Vector2(top.X - BoxWidth * 0.5f, top.Y + HeadOffsetY);
        if (target == _lastTarget) return;
        _lastTarget = target;
        GlobalPosition = target;
    }
}
