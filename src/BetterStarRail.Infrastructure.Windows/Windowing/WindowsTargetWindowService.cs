using System.Runtime.InteropServices;
using System.Text;
using BetterStarRail.Core.Windows;

namespace BetterStarRail.Infrastructure.Windows.Windowing;

public sealed record WindowSnapshotResult(bool Succeeded, TargetWindowSnapshot? Snapshot, string Error)
{
    public static WindowSnapshotResult Success(TargetWindowSnapshot snapshot) => new(true, snapshot, string.Empty);
    public static WindowSnapshotResult Failure(string error) => new(false, null, error);
}

public sealed class WindowsTargetWindowService
{
    private readonly string expectedTitle;

    public WindowsTargetWindowService(string expectedTitle)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(expectedTitle);
        this.expectedTitle = expectedTitle;
    }

    public WindowSnapshotResult Locate()
    {
        var handle = NativeMethods.FindWindow(null, expectedTitle);
        return handle == 0
            ? WindowSnapshotResult.Failure($"未找到项目自建测试窗口：{expectedTitle}。")
            : ReadSnapshot(handle);
    }

    public WindowSnapshotResult ReadSnapshot(nint handle)
    {
        if (handle == 0 || !NativeMethods.IsWindow(handle))
            return WindowSnapshotResult.Failure("目标窗口句柄为空或已失效。");
        var title = ReadWindowText(handle);
        if (!string.Equals(title, expectedTitle, StringComparison.Ordinal))
            return WindowSnapshotResult.Failure("目标窗口标题与项目自建测试窗口不匹配。");
        var className = ReadClassName(handle);
        if (string.IsNullOrWhiteSpace(className))
            return WindowSnapshotResult.Failure("无法读取目标窗口类名。");
        NativeMethods.GetWindowThreadProcessId(handle, out var processId);
        if (processId == 0) return WindowSnapshotResult.Failure("无法读取目标窗口进程 ID。");
        if (!NativeMethods.GetClientRect(handle, out var rectangle))
            return WindowSnapshotResult.Failure(LastError("无法读取目标窗口客户区"));
        var origin = new Point();
        if (!NativeMethods.ClientToScreen(handle, ref origin))
            return WindowSnapshotResult.Failure(LastError("无法换算客户区屏幕位置"));
        var width = rectangle.Right - rectangle.Left;
        var height = rectangle.Bottom - rectangle.Top;
        if (width <= 0 || height <= 0) return WindowSnapshotResult.Failure("目标窗口客户区尺寸无效。");
        var dpi = NativeMethods.GetDpiForWindow(handle);
        if (dpi == 0) dpi = 96;
        var lastInput = new LastInputInfo { Size = (uint)Marshal.SizeOf<LastInputInfo>() };
        var generation = NativeMethods.GetLastInputInfo(ref lastInput) ? lastInput.Time : 0;
        var identity = new TargetWindowIdentity(handle, title, className, checked((int)processId));
        var clientArea = new ClientArea(origin.X, origin.Y, width, height, dpi);
        return WindowSnapshotResult.Success(new TargetWindowSnapshot(
            identity,
            clientArea,
            NativeMethods.GetForegroundWindow() == handle,
            NativeMethods.IsIconic(handle),
            generation));
    }

    public WindowSnapshotResult ValidateSaved(TargetWindowIdentity savedIdentity)
    {
        var current = ReadSnapshot(savedIdentity.Handle);
        if (!current.Succeeded || current.Snapshot is null) return current;
        var identity = current.Snapshot.Identity;
        return identity == savedIdentity
            ? current
            : WindowSnapshotResult.Failure("保存的窗口身份与当前句柄不一致。");
    }

    private static string ReadWindowText(nint handle)
    {
        var buffer = new StringBuilder(Math.Max(2, NativeMethods.GetWindowTextLength(handle) + 1));
        NativeMethods.GetWindowText(handle, buffer, buffer.Capacity);
        return buffer.ToString();
    }

    private static string ReadClassName(nint handle)
    {
        var buffer = new StringBuilder(256);
        NativeMethods.GetClassName(handle, buffer, buffer.Capacity);
        return buffer.ToString();
    }

    private static string LastError(string operation) => $"{operation}，Win32 错误码：{Marshal.GetLastWin32Error()}。";

    [StructLayout(LayoutKind.Sequential)]
    private struct Rect
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct Point
    {
        public int X;
        public int Y;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct LastInputInfo
    {
        public uint Size;
        public uint Time;
    }

    private static class NativeMethods
    {
        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern nint FindWindow(string? className, string windowName);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsWindow(nint windowHandle);

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern int GetWindowText(nint windowHandle, StringBuilder text, int maximumCount);

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern int GetWindowTextLength(nint windowHandle);

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern int GetClassName(nint windowHandle, StringBuilder className, int maximumCount);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern uint GetWindowThreadProcessId(nint windowHandle, out uint processId);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetClientRect(nint windowHandle, out Rect rectangle);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ClientToScreen(nint windowHandle, ref Point point);

        [DllImport("user32.dll")]
        public static extern nint GetForegroundWindow();

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsIconic(nint windowHandle);

        [DllImport("user32.dll")]
        public static extern uint GetDpiForWindow(nint windowHandle);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetLastInputInfo(ref LastInputInfo lastInputInfo);
    }
}
