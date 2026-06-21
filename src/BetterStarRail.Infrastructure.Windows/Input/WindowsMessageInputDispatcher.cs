using System.Runtime.InteropServices;
using BetterStarRail.Automation.Abstractions;
using BetterStarRail.Automation.Models;
using BetterStarRail.Core.Models;
using BetterStarRail.Core.Windows;
using BetterStarRail.Infrastructure.Windows.Windowing;

namespace BetterStarRail.Infrastructure.Windows.Input;

public sealed class WindowsMessageInputDispatcher : IWindowInputDispatcher
{
    public const string TestWindowTitle = "Better Star Rail V1 Test Window";
    private readonly TargetWindowIdentity targetIdentity;
    private readonly WindowsTargetWindowService windowService;

    public WindowsMessageInputDispatcher(TargetWindowIdentity targetIdentity)
    {
        if (!targetIdentity.IsValid || !string.Equals(targetIdentity.Title, TestWindowTitle, StringComparison.Ordinal))
            throw new ArgumentException("输入发送器只能绑定项目自建 V1 测试窗口。", nameof(targetIdentity));
        this.targetIdentity = targetIdentity;
        windowService = new WindowsTargetWindowService(TestWindowTitle);
    }

    public ValueTask<OperationResult> ClickAsync(nint windowHandle, ClientPoint point, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var validation = ValidateWindow(windowHandle);
        if (!validation.Succeeded) return ValueTask.FromResult(validation);
        if (point.X < 0 || point.Y < 0 || point.X > ushort.MaxValue || point.Y > ushort.MaxValue)
            return ValueTask.FromResult(OperationResult.Failure("客户区点击坐标无效。"));
        var parameter = new nint((point.Y << 16) | (point.X & 0xFFFF));
        if (!NativeMethods.PostMessage(windowHandle, NativeMethods.WmLbuttondown, new nint(1), parameter) ||
            !NativeMethods.PostMessage(windowHandle, NativeMethods.WmLbuttonup, 0, parameter))
        {
            return ValueTask.FromResult(OperationResult.Failure(LastError("定向鼠标点击失败")));
        }

        return ValueTask.FromResult(OperationResult.Success("定向鼠标点击已发送。"));
    }

    public ValueTask<OperationResult> PressKeyAsync(nint windowHandle, AllowedKey key, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var validation = ValidateWindow(windowHandle);
        if (!validation.Succeeded) return ValueTask.FromResult(validation);
        var virtualKey = ToVirtualKey(key);
        if (!NativeMethods.PostMessage(windowHandle, NativeMethods.WmKeydown, new nint(virtualKey), 0) ||
            !NativeMethods.PostMessage(windowHandle, NativeMethods.WmKeyup, new nint(virtualKey), 0))
        {
            return ValueTask.FromResult(OperationResult.Failure(LastError("定向按键输入失败")));
        }

        return ValueTask.FromResult(OperationResult.Success("允许按键已发送。"));
    }

    public ValueTask ReleaseAllAsync(nint windowHandle, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!ValidateWindow(windowHandle).Succeeded) return ValueTask.CompletedTask;
        NativeMethods.PostMessage(windowHandle, NativeMethods.WmKeyup, new nint(ToVirtualKey(AllowedKey.Enter)), 0);
        NativeMethods.PostMessage(windowHandle, NativeMethods.WmKeyup, new nint(ToVirtualKey(AllowedKey.Escape)), 0);
        NativeMethods.PostMessage(windowHandle, NativeMethods.WmLbuttonup, 0, 0);
        return ValueTask.CompletedTask;
    }

    private OperationResult ValidateWindow(nint handle)
    {
        if (handle == 0 || handle != targetIdentity.Handle)
            return OperationResult.Failure("输入目标句柄与绑定的自建测试窗口不一致。");
        var validation = windowService.ValidateSaved(targetIdentity);
        return validation.Succeeded
            ? OperationResult.Success()
            : OperationResult.Failure(validation.Error);
    }

    private static int ToVirtualKey(AllowedKey key) => key switch
    {
        AllowedKey.Enter => 0x0D,
        AllowedKey.Escape => 0x1B,
        _ => throw new ArgumentOutOfRangeException(nameof(key)),
    };

    private static string LastError(string operation) => $"{operation}，Win32 错误码：{Marshal.GetLastWin32Error()}。";

    private static class NativeMethods
    {
        public const uint WmKeydown = 0x0100;
        public const uint WmKeyup = 0x0101;
        public const uint WmLbuttondown = 0x0201;
        public const uint WmLbuttonup = 0x0202;

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsWindow(nint windowHandle);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool PostMessage(nint windowHandle, uint message, nint wordParameter, nint longParameter);
    }
}
