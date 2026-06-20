using System.Runtime.InteropServices;
using BetterStarRail.Core.Vision;

namespace BetterStarRail.TestWindow;

public static class Program
{
    public const string WindowTitle = "Better Star Rail V1 Test Window";
    public const string WindowClass = "BetterStarRailV1TestWindowClass";
    private static readonly WindowProcedure Procedure = HandleMessage;
    private static TestPageId page = TestPageId.Welcome;

    [STAThread]
    public static int Main()
    {
        var instance = NativeMethods.GetModuleHandle(null);
        var windowClass = new WindowClassEx
        {
            Size = (uint)Marshal.SizeOf<WindowClassEx>(),
            Instance = instance,
            Procedure = Procedure,
            ClassName = WindowClass,
            Cursor = NativeMethods.LoadCursor(0, new nint(32512)),
        };
        if (NativeMethods.RegisterClassEx(ref windowClass) == 0) return Marshal.GetLastWin32Error();
        var handle = NativeMethods.CreateWindowEx(
            0,
            WindowClass,
            WindowTitle,
            0x00CF0000,
            unchecked((int)0x80000000),
            unchecked((int)0x80000000),
            640,
            480,
            0,
            0,
            instance,
            0);
        if (handle == 0) return Marshal.GetLastWin32Error();
        NativeMethods.ShowWindow(handle, 5);
        NativeMethods.UpdateWindow(handle);
        Message message;
        while (NativeMethods.GetMessage(out message, 0, 0, 0) > 0)
        {
            NativeMethods.TranslateMessage(ref message);
            NativeMethods.DispatchMessage(ref message);
        }

        return checked((int)message.WordParameter);
    }

    private static nint HandleMessage(nint handle, uint message, nint wordParameter, nint longParameter)
    {
        switch (message)
        {
            case 0x000F:
                Paint(handle);
                return 0;
            case 0x0202:
                var x = unchecked((short)(longParameter.ToInt64() & 0xFFFF));
                var y = unchecked((short)((longParameter.ToInt64() >> 16) & 0xFFFF));
                if (IsPrimaryButton(handle, x, y))
                {
                    if (page == TestPageId.Welcome) page = TestPageId.Confirm;
                    else if (page == TestPageId.Confirm) page = TestPageId.Complete;
                    NativeMethods.InvalidateRect(handle, 0, true);
                }

                return 0;
            case 0x0101:
                if (wordParameter.ToInt32() is >= 0x31 and <= 0x35)
                {
                    page = (TestPageId)(wordParameter.ToInt32() - 0x30);
                    NativeMethods.InvalidateRect(handle, 0, true);
                }

                return 0;
            case 0x0002:
                NativeMethods.PostQuitMessage(0);
                return 0;
            default:
                return NativeMethods.DefWindowProc(handle, message, wordParameter, longParameter);
        }
    }

    private static void Paint(nint handle)
    {
        var deviceContext = NativeMethods.BeginPaint(handle, out var paint);
        try
        {
            NativeMethods.GetClientRect(handle, out var client);
            Fill(deviceContext, client, 0x00F4F4F4);
            Fill(deviceContext, new Rect { Left = 0, Top = 0, Right = 64, Bottom = 64 }, MarkerColor(page));
            NativeMethods.SetBkMode(deviceContext, 1);
            NativeMethods.SetTextColor(deviceContext, 0x00202020);
            var title = new Rect { Left = 100, Top = 80, Right = client.Right - 40, Bottom = 130 };
            NativeMethods.DrawText(deviceContext, "Better Star Rail V1 自建测试窗口", -1, ref title, 0x00000020);
            var pageText = new Rect { Left = 100, Top = 140, Right = client.Right - 40, Bottom = 190 };
            NativeMethods.DrawText(deviceContext, $"当前页面：{page}", -1, ref pageText, 0x00000020);
            var button = PrimaryButton(client);
            Fill(deviceContext, button, 0x00D8D8D8);
            var action = page switch
            {
                TestPageId.Welcome => "进入确认页",
                TestPageId.Confirm => "确认并完成",
                TestPageId.Complete => "流程已完成",
                _ => "当前页面无自动动作",
            };
            NativeMethods.DrawText(deviceContext, action, -1, ref button, 0x00000025);
        }
        finally
        {
            NativeMethods.EndPaint(handle, ref paint);
        }
    }

    private static bool IsPrimaryButton(nint handle, int x, int y)
    {
        NativeMethods.GetClientRect(handle, out var client);
        var button = PrimaryButton(client);
        return x >= button.Left && x < button.Right && y >= button.Top && y < button.Bottom &&
               page is TestPageId.Welcome or TestPageId.Confirm;
    }

    private static Rect PrimaryButton(Rect client)
    {
        var centerX = client.Right / 2;
        var centerY = (client.Bottom * 3) / 4;
        return new Rect { Left = centerX - 120, Top = centerY - 32, Right = centerX + 120, Bottom = centerY + 32 };
    }

    private static uint MarkerColor(TestPageId current) => current switch
    {
        TestPageId.Welcome => ColorRef(30, 120, 220),
        TestPageId.Confirm => ColorRef(220, 160, 30),
        TestPageId.Progress => ColorRef(120, 60, 200),
        TestPageId.Complete => ColorRef(40, 180, 90),
        TestPageId.Blocked => ColorRef(220, 50, 60),
        _ => 0,
    };

    private static uint ColorRef(byte red, byte green, byte blue) => (uint)(red | (green << 8) | (blue << 16));

    private static void Fill(nint deviceContext, Rect rectangle, uint color)
    {
        var brush = NativeMethods.CreateSolidBrush(color);
        try
        {
            NativeMethods.FillRect(deviceContext, ref rectangle, brush);
        }
        finally
        {
            if (brush != 0) NativeMethods.DeleteObject(brush);
        }
    }

    private delegate nint WindowProcedure(nint handle, uint message, nint wordParameter, nint longParameter);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct WindowClassEx
    {
        public uint Size;
        public uint Style;
        [MarshalAs(UnmanagedType.FunctionPtr)] public WindowProcedure Procedure;
        public int ClassExtra;
        public int WindowExtra;
        public nint Instance;
        public nint Icon;
        public nint Cursor;
        public nint Background;
        public string? MenuName;
        public string ClassName;
        public nint SmallIcon;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct Point
    {
        public int X;
        public int Y;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct Message
    {
        public nint Handle;
        public uint Id;
        public nint WordParameter;
        public nint LongParameter;
        public uint Time;
        public Point Point;
        public uint Private;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct Rect
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct PaintStruct
    {
        public nint DeviceContext;
        [MarshalAs(UnmanagedType.Bool)] public bool Erase;
        public Rect PaintRectangle;
        [MarshalAs(UnmanagedType.Bool)] public bool Restore;
        [MarshalAs(UnmanagedType.Bool)] public bool IncUpdate;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)] public byte[] Reserved;
    }

    private static class NativeMethods
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)] public static extern nint GetModuleHandle(string? moduleName);
        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)] public static extern ushort RegisterClassEx(ref WindowClassEx windowClass);
        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)] public static extern nint CreateWindowEx(uint exStyle, string className, string windowName, uint style, int x, int y, int width, int height, nint parent, nint menu, nint instance, nint parameter);
        [DllImport("user32.dll")] public static extern bool ShowWindow(nint handle, int command);
        [DllImport("user32.dll")] public static extern bool UpdateWindow(nint handle);
        [DllImport("user32.dll")] public static extern int GetMessage(out Message message, nint handle, uint minimum, uint maximum);
        [DllImport("user32.dll")] public static extern bool TranslateMessage(ref Message message);
        [DllImport("user32.dll", CharSet = CharSet.Unicode)] public static extern nint DispatchMessage(ref Message message);
        [DllImport("user32.dll", CharSet = CharSet.Unicode)] public static extern nint DefWindowProc(nint handle, uint message, nint wordParameter, nint longParameter);
        [DllImport("user32.dll")] public static extern void PostQuitMessage(int exitCode);
        [DllImport("user32.dll")] public static extern nint LoadCursor(nint instance, nint cursorName);
        [DllImport("user32.dll")] public static extern bool InvalidateRect(nint handle, nint rectangle, bool erase);
        [DllImport("user32.dll")] public static extern nint BeginPaint(nint handle, out PaintStruct paint);
        [DllImport("user32.dll")] public static extern bool EndPaint(nint handle, ref PaintStruct paint);
        [DllImport("user32.dll")] public static extern bool GetClientRect(nint handle, out Rect rectangle);
        [DllImport("user32.dll")] public static extern int FillRect(nint deviceContext, ref Rect rectangle, nint brush);
        [DllImport("user32.dll", CharSet = CharSet.Unicode)] public static extern int DrawText(nint deviceContext, string text, int count, ref Rect rectangle, uint format);
        [DllImport("gdi32.dll")] public static extern nint CreateSolidBrush(uint color);
        [DllImport("gdi32.dll")] public static extern bool DeleteObject(nint graphicsObject);
        [DllImport("gdi32.dll")] public static extern int SetBkMode(nint deviceContext, int mode);
        [DllImport("gdi32.dll")] public static extern uint SetTextColor(nint deviceContext, uint color);
    }
}
