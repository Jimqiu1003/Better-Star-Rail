using System.Runtime.InteropServices;
using BetterStarRail.Vision.Abstractions;
using BetterStarRail.Vision.Models;

namespace BetterStarRail.Infrastructure.Windows.Capture;

public sealed class GdiScreenCaptureService : IScreenCaptureService
{
    public ValueTask<CaptureResult> CaptureAsync(
        nint windowHandle,
        CaptureRegion? region,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (windowHandle == 0) return ValueTask.FromResult(CaptureResult.Failure("目标窗口句柄为空。"));
        if (!NativeMethods.IsWindow(windowHandle)) return ValueTask.FromResult(CaptureResult.Failure("目标窗口句柄已失效。"));
        if (!NativeMethods.GetClientRect(windowHandle, out var clientRect))
            return ValueTask.FromResult(CaptureResult.Failure(LastError("无法获取客户区")));

        var captureRegion = region ?? new CaptureRegion(0, 0, clientRect.Right, clientRect.Bottom);
        if (captureRegion.X < 0 || captureRegion.Y < 0 ||
            captureRegion.X + captureRegion.Width > clientRect.Right ||
            captureRegion.Y + captureRegion.Height > clientRect.Bottom)
        {
            return ValueTask.FromResult(CaptureResult.Failure("截图 ROI 超出目标窗口客户区。"));
        }

        nint sourceDc = 0;
        nint memoryDc = 0;
        nint bitmap = 0;
        nint previousObject = 0;
        try
        {
            sourceDc = NativeMethods.GetDC(windowHandle);
            if (sourceDc == 0) return ValueTask.FromResult(CaptureResult.Failure(LastError("无法获取客户区设备上下文")));
            memoryDc = NativeMethods.CreateCompatibleDC(sourceDc);
            if (memoryDc == 0) return ValueTask.FromResult(CaptureResult.Failure(LastError("无法创建内存设备上下文")));
            bitmap = NativeMethods.CreateCompatibleBitmap(sourceDc, captureRegion.Width, captureRegion.Height);
            if (bitmap == 0) return ValueTask.FromResult(CaptureResult.Failure(LastError("无法创建截图位图")));
            previousObject = NativeMethods.SelectObject(memoryDc, bitmap);
            if (previousObject == 0 || previousObject == new nint(-1))
                return ValueTask.FromResult(CaptureResult.Failure(LastError("无法选择截图位图")));
            if (!NativeMethods.BitBlt(memoryDc, 0, 0, captureRegion.Width, captureRegion.Height,
                    sourceDc, captureRegion.X, captureRegion.Y, NativeMethods.Srccopy))
                return ValueTask.FromResult(CaptureResult.Failure(LastError("客户区截图失败")));

            cancellationToken.ThrowIfCancellationRequested();
            var pixels = new byte[captureRegion.Width * captureRegion.Height * 4];
            var bitmapInfo = BitmapInfo.Create(captureRegion.Width, captureRegion.Height);
            var scanLines = NativeMethods.GetDIBits(memoryDc, bitmap, 0, (uint)captureRegion.Height, pixels, ref bitmapInfo, 0);
            if (scanLines != captureRegion.Height)
                return ValueTask.FromResult(CaptureResult.Failure(LastError("读取截图像素失败")));
            return ValueTask.FromResult(CaptureResult.Success(
                new CapturedFrame(captureRegion.Width, captureRegion.Height, captureRegion.Width * 4, pixels)));
        }
        finally
        {
            if (previousObject != 0 && memoryDc != 0) NativeMethods.SelectObject(memoryDc, previousObject);
            if (bitmap != 0) NativeMethods.DeleteObject(bitmap);
            if (memoryDc != 0) NativeMethods.DeleteDC(memoryDc);
            if (sourceDc != 0) NativeMethods.ReleaseDC(windowHandle, sourceDc);
        }
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
    private struct BitmapInfoHeader
    {
        public uint Size;
        public int Width;
        public int Height;
        public ushort Planes;
        public ushort BitCount;
        public uint Compression;
        public uint SizeImage;
        public int XPelsPerMeter;
        public int YPelsPerMeter;
        public uint ColorsUsed;
        public uint ColorsImportant;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct BitmapInfo
    {
        public BitmapInfoHeader Header;
        public uint Colors;

        public static BitmapInfo Create(int width, int height) => new()
        {
            Header = new BitmapInfoHeader
            {
                Size = (uint)Marshal.SizeOf<BitmapInfoHeader>(),
                Width = width,
                Height = -height,
                Planes = 1,
                BitCount = 32,
            },
        };
    }

    private static class NativeMethods
    {
        public const uint Srccopy = 0x00CC0020;

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsWindow(nint windowHandle);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetClientRect(nint windowHandle, out Rect rectangle);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern nint GetDC(nint windowHandle);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern int ReleaseDC(nint windowHandle, nint deviceContext);

        [DllImport("gdi32.dll", SetLastError = true)]
        public static extern nint CreateCompatibleDC(nint deviceContext);

        [DllImport("gdi32.dll", SetLastError = true)]
        public static extern nint CreateCompatibleBitmap(nint deviceContext, int width, int height);

        [DllImport("gdi32.dll", SetLastError = true)]
        public static extern nint SelectObject(nint deviceContext, nint graphicsObject);

        [DllImport("gdi32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeleteObject(nint graphicsObject);

        [DllImport("gdi32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeleteDC(nint deviceContext);

        [DllImport("gdi32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool BitBlt(
            nint destination, int xDestination, int yDestination, int width, int height,
            nint source, int xSource, int ySource, uint operation);

        [DllImport("gdi32.dll", SetLastError = true)]
        public static extern int GetDIBits(
            nint deviceContext, nint bitmap, uint startScan, uint scanLines,
            [Out] byte[] bits, ref BitmapInfo bitmapInfo, uint usage);
    }
}
