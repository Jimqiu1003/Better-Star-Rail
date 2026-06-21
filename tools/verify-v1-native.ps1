param(
    [ValidateRange(1, 1000)]
    [int] $Iterations = 100
)

$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path -Parent $PSScriptRoot
Set-Location -LiteralPath $repoRoot

Add-Type -TypeDefinition @'
using System;
using System.Runtime.InteropServices;

public static class BetterStarRailV1VerificationNativeMethods
{
    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    public static extern IntPtr FindWindow(string className, string windowName);

    [DllImport("user32.dll")]
    public static extern IntPtr SendMessage(IntPtr windowHandle, uint message, IntPtr wordParameter, IntPtr longParameter);

    [DllImport("user32.dll")]
    public static extern bool UpdateWindow(IntPtr windowHandle);

    [DllImport("user32.dll")] private static extern IntPtr GetForegroundWindow();
    [DllImport("user32.dll")] private static extern uint GetWindowThreadProcessId(IntPtr windowHandle, out uint processId);
    [DllImport("kernel32.dll")] private static extern uint GetCurrentThreadId();
    [DllImport("user32.dll")] private static extern bool AttachThreadInput(uint attach, uint attachTo, bool attachState);
    [DllImport("user32.dll")] private static extern bool BringWindowToTop(IntPtr windowHandle);
    [DllImport("user32.dll")] private static extern bool ShowWindow(IntPtr windowHandle, int command);
    [DllImport("user32.dll")] private static extern bool SetForegroundWindow(IntPtr windowHandle);
    [DllImport("user32.dll")] private static extern IntPtr SetFocus(IntPtr windowHandle);

    public static bool ActivateWindow(IntPtr windowHandle)
    {
        IntPtr foreground = GetForegroundWindow();
        uint ignoredProcessId;
        uint currentThread = GetCurrentThreadId();
        uint foregroundThread = GetWindowThreadProcessId(foreground, out ignoredProcessId);
        uint targetThread = GetWindowThreadProcessId(windowHandle, out ignoredProcessId);
        bool attachedForeground = foregroundThread != 0 && foregroundThread != currentThread &&
            AttachThreadInput(currentThread, foregroundThread, true);
        bool attachedTarget = targetThread != 0 && targetThread != currentThread &&
            AttachThreadInput(currentThread, targetThread, true);
        try
        {
            ShowWindow(windowHandle, 5);
            BringWindowToTop(windowHandle);
            SetForegroundWindow(windowHandle);
            SetFocus(windowHandle);
            return GetForegroundWindow() == windowHandle;
        }
        finally
        {
            if (attachedTarget) AttachThreadInput(currentThread, targetThread, false);
            if (attachedForeground) AttachThreadInput(currentThread, foregroundThread, false);
        }
    }
}
'@

$testWindowPath = Join-Path $repoRoot 'tools\BetterStarRail.TestWindow\bin\x64\Release\net10.0-windows\BetterStarRail.TestWindow.exe'
$runnerPath = Join-Path $repoRoot 'tools\BetterStarRail.V1.Runner\bin\x64\Release\net10.0-windows\BetterStarRail.V1.Runner.exe'
if (-not (Test-Path -LiteralPath $testWindowPath) -or -not (Test-Path -LiteralPath $runnerPath)) {
    throw '缺少 Release 构建产物。请先运行 dotnet build --configuration Release --no-restore。'
}

$testWindow = Start-Process -FilePath $testWindowPath -PassThru
$passed = 0
$failed = @()
$watch = [Diagnostics.Stopwatch]::StartNew()
$windowClass = 'BetterStarRailV1TestWindowClass'
$windowTitle = 'Better Star Rail V1 Test Window'
$welcomeKey = [IntPtr] 0x31

try {
    $deadline = [DateTime]::UtcNow.AddSeconds(5)
    $handle = [IntPtr]::Zero
    do {
        if ($testWindow.HasExited) {
            throw "测试窗口提前退出，退出码：$($testWindow.ExitCode)。"
        }

        $handle = [BetterStarRailV1VerificationNativeMethods]::FindWindow($windowClass, $windowTitle)
        if ($handle -ne [IntPtr]::Zero) {
            break
        }

        Start-Sleep -Milliseconds 50
    } while ([DateTime]::UtcNow -lt $deadline)

    if ($handle -eq [IntPtr]::Zero) {
        throw '5 秒内未找到项目自建测试窗口。'
    }

    for ($iteration = 1; $iteration -le $Iterations; $iteration++) {
        # 同步重置并完成重绘，避免验证夹具自身产生过渡帧。
        [void] [BetterStarRailV1VerificationNativeMethods]::SendMessage($handle, 0x0101, $welcomeKey, [IntPtr]::Zero)
        [void] [BetterStarRailV1VerificationNativeMethods]::UpdateWindow($handle)
        if (-not [BetterStarRailV1VerificationNativeMethods]::ActivateWindow($handle)) {
            throw "第 $iteration 次无法将自建测试窗口切换到前台。"
        }

        $runner = Start-Process -FilePath $runnerPath -WindowStyle Hidden -PassThru
        if (-not $runner.WaitForExit(5000)) {
            $runner.Kill()
            $runner.WaitForExit()
            $failed += [pscustomobject]@{
                Iteration = $iteration
                ExitCode = 'Timeout'
            }
        }
        elseif ($runner.ExitCode -eq 0) {
            $passed++
        }
        else {
            $failed += [pscustomobject]@{
                Iteration = $iteration
                ExitCode = $runner.ExitCode
            }
        }
    }

    $watch.Stop()
    Write-Output "NATIVE_CLOSED_LOOP=$passed/$Iterations"
    Write-Output "FAILED_COUNT=$($failed.Count)"
    Write-Output "ELAPSED_MS=$($watch.ElapsedMilliseconds)"
    if ($failed.Count -gt 0) {
        $failed | Format-Table -AutoSize
        exit 1
    }
}
finally {
    if (-not $testWindow.HasExited) {
        [void] $testWindow.CloseMainWindow()
        if (-not $testWindow.WaitForExit(3000)) {
            $testWindow.Kill()
            $testWindow.WaitForExit()
        }
    }
}
