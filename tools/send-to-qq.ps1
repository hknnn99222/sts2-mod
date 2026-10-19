<#
.SYNOPSIS
    一键把文件发送到 QQ 的「我的iPhone」会话。

.DESCRIPTION
    不依赖第三方模块，只用 Win32 窗口操作：
      1. 若聊天窗口已打开 → 直接激活
         否则在 QQ 主窗口搜索并双击结果打开会话
      2. 点击输入框，把文件放进剪贴板后 Ctrl+V（QQ 会自动上传并发送）
      3. 等待若干秒后报告结果（最终状态以 QQ 窗口显示为准）

    运行期间请不要操作鼠标键盘，否则点击位置可能错位。
    QQ 需已登录；主窗口为迷你模式（约 300px 宽）时坐标最稳。

.PARAMETER FilePath
    要发送的文件。省略时自动选仓库根目录最新的 GamblerMod_v*.zip。

.PARAMETER Target
    目标会话名，默认「我的iPhone」。

.PARAMETER DryRun
    只检查/打开会话，不发送。

.EXAMPLE
    .\send-to-qq.ps1 D:\worlk\sts2-mod\GamblerMod_v0.6.0.zip
#>
param(
    [Parameter(Position = 0)]
    [string]$FilePath,

    [string]$Target = '我的iPhone',

    [int]$WaitSeconds = 8,

    [switch]$DryRun
)

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Windows.Forms

Add-Type @'
using System;
using System.Text;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public static class QqWin {
    public delegate bool EnumProc(IntPtr h, IntPtr l);

    [DllImport("user32.dll")] public static extern bool EnumWindows(EnumProc cb, IntPtr l);
    [DllImport("user32.dll")] public static extern uint GetWindowThreadProcessId(IntPtr h, out uint pid);
    [DllImport("user32.dll")] public static extern bool IsWindowVisible(IntPtr h);
    [DllImport("user32.dll", CharSet=CharSet.Unicode)] public static extern int GetWindowText(IntPtr h, StringBuilder s, int n);
    [DllImport("user32.dll")] public static extern bool GetWindowRect(IntPtr h, out RECT r);
    [DllImport("user32.dll")] public static extern bool ShowWindow(IntPtr h, int n);
    [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr h);
    [DllImport("user32.dll")] public static extern bool SetCursorPos(int x, int y);
    [DllImport("user32.dll")] public static extern void mouse_event(uint f, uint dx, uint dy, uint d, UIntPtr e);

    public struct RECT { public int Left, Top, Right, Bottom; }

    public static List<IntPtr> WindowsOf(int pid) {
        var list = new List<IntPtr>();
        EnumWindows((h, l) => {
            uint p;
            GetWindowThreadProcessId(h, out p);
            if (p == pid && IsWindowVisible(h)) list.Add(h);
            return true;
        }, IntPtr.Zero);
        return list;
    }

    public static string Title(IntPtr h) {
        var sb = new StringBuilder(512);
        GetWindowText(h, sb, 512);
        return sb.ToString();
    }

    public static void Activate(IntPtr h) {
        ShowWindow(h, 9);            // SW_RESTORE
        SetForegroundWindow(h);
        System.Threading.Thread.Sleep(400);
    }

    public static void ClickAt(int x, int y, bool doubleClick) {
        SetCursorPos(x, y);
        System.Threading.Thread.Sleep(250);
        int times = doubleClick ? 2 : 1;
        for (int i = 0; i < times; i++) {
            mouse_event(0x0002, 0, 0, 0, UIntPtr.Zero);
            mouse_event(0x0004, 0, 0, 0, UIntPtr.Zero);
            if (doubleClick) System.Threading.Thread.Sleep(80);
        }
    }
}
'@

function Write-Step([string]$msg) {
    Write-Host "[QQ] $msg" -ForegroundColor Cyan
}

function Find-ChatWindow {
    $qq = Get-Process -Name QQ -ErrorAction SilentlyContinue | Select-Object -First 1
    if (-not $qq) { throw 'QQ 没有运行，请先登录 QQ。' }
    foreach ($h in [QqWin]::WindowsOf($qq.Id)) {
        if ([QqWin]::Title($h) -eq $Target) { return $h }
    }
    return [IntPtr]::Zero
}

function Find-MainWindow {
    $qq = Get-Process -Name QQ -ErrorAction SilentlyContinue | Select-Object -First 1
    if (-not $qq) { throw 'QQ 没有运行，请先登录 QQ。' }
    foreach ($h in [QqWin]::WindowsOf($qq.Id)) {
        if ([QqWin]::Title($h) -eq 'QQ') { return $h }
    }
    return [IntPtr]::Zero
}

function Wait-ChatWindow([int]$Seconds) {
    $deadline = (Get-Date).AddSeconds($Seconds)
    while ((Get-Date) -lt $deadline) {
        $h = Find-ChatWindow
        if ($h -ne [IntPtr]::Zero) { return $h }
        Start-Sleep -Milliseconds 400
    }
    return [IntPtr]::Zero
}

function Send-Keys([string]$keys) {
    [System.Windows.Forms.SendKeys]::SendWait($keys)
}

# ---------- 参数解析 ----------
if (-not $FilePath) {
    $repoRoot = Split-Path $PSScriptRoot -Parent
    $latest = Get-ChildItem -LiteralPath $repoRoot -Filter 'GamblerMod_v*.zip' -File -ErrorAction SilentlyContinue |
        Sort-Object LastWriteTime -Descending | Select-Object -First 1
    if (-not $latest) { throw "未指定文件，且 $repoRoot 下找不到 GamblerMod_v*.zip" }
    $FilePath = $latest.FullName
}
$FilePath = (Resolve-Path -LiteralPath $FilePath).Path
$fileName = Split-Path $FilePath -Leaf
Write-Step ("文件: {0} ({1:N0} 字节)" -f $FilePath, (Get-Item -LiteralPath $FilePath).Length)
Write-Step ("目标: {0}" -f $Target)

# ---------- 1. 打开会话 ----------
$chat = Find-ChatWindow
if ($chat -eq [IntPtr]::Zero) {
    Write-Step '聊天窗口未打开，在 QQ 主窗口搜索...'
    $main = Find-MainWindow
    if ($main -eq [IntPtr]::Zero) { throw '找不到 QQ 主窗口' }
    [QqWin]::Activate($main)
    Start-Sleep -Milliseconds 300

    $mr = New-Object QqWin+RECT
    [void][QqWin]::GetWindowRect($main, [ref]$mr)
    $mw = $mr.Right - $mr.Left
    $mh = $mr.Bottom - $mr.Top

    if ($mw -lt 450) {
        # 迷你模式：搜索框在顶部，结果第一行在约 28% 高度
        $sx = $mr.Left + [int]($mw * 0.50)
        $sy = $mr.Top + [int]($mh * 0.148)
        $rx = $mr.Left + [int]($mw * 0.50)
        $ry = $mr.Top + [int]($mh * 0.282)
    } else {
        # 普通模式：搜索框在左侧会话栏顶部
        $sx = $mr.Left + [int]($mw * 0.14)
        $sy = $mr.Top + [int]($mh * 0.07)
        $rx = $mr.Left + [int]($mw * 0.14)
        $ry = $mr.Top + [int]($mh * 0.12)
    }

    [QqWin]::ClickAt($sx, $sy, $false)
    Start-Sleep -Milliseconds 300
    Set-Clipboard -Value $Target
    Start-Sleep -Milliseconds 150
    Send-Keys '^a'
    Send-Keys '^v'
    Start-Sleep -Milliseconds 1500

    [QqWin]::ClickAt($rx, $ry, $true)
    Write-Step '等待会话窗口...'
    $chat = Wait-ChatWindow 12
    if ($chat -eq [IntPtr]::Zero) { throw "未能打开会话窗口「$Target」（坐标搜索失败，请手动打开一次会话后重试）" }
} else {
    Write-Step '聊天窗口已存在'
}

[QqWin]::Activate($chat)
Start-Sleep -Milliseconds 500

if ($DryRun) {
    Write-Step 'DryRun：会话已就绪，未发送文件。'
    exit 0
}

# ---------- 2. 点击输入框并粘贴文件 ----------
$cr = New-Object QqWin+RECT
[void][QqWin]::GetWindowRect($chat, [ref]$cr)
$cw = $cr.Right - $cr.Left
$ch = $cr.Bottom - $cr.Top
$ix = $cr.Left + [int]($cw * 0.45)
$iy = $cr.Top + [int]($ch * 0.88)

[QqWin]::ClickAt($ix, $iy, $false)
Start-Sleep -Milliseconds 300

Set-Clipboard -Path $FilePath
Start-Sleep -Milliseconds 300
Write-Step '粘贴文件并上传...'
Send-Keys '^v'

Start-Sleep -Seconds $WaitSeconds
Write-Step ("已执行发送：{0}" -f $fileName)
Write-Step '请在 QQ 窗口确认文件显示「已发送」。'
exit 0
