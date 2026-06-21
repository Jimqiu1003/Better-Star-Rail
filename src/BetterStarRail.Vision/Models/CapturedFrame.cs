namespace BetterStarRail.Vision.Models;

public sealed class CapturedFrame
{
    private readonly byte[] pixels;

    public CapturedFrame(int width, int height, int stride, ReadOnlySpan<byte> pixels)
    {
        if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width));
        if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height));
        if (stride < width * 4) throw new ArgumentOutOfRangeException(nameof(stride));
        if (pixels.Length < stride * height) throw new ArgumentException("像素缓冲区长度不足。", nameof(pixels));
        Width = width;
        Height = height;
        Stride = stride;
        this.pixels = pixels[..(stride * height)].ToArray();
    }

    public int Width { get; }
    public int Height { get; }
    public int Stride { get; }
    public ReadOnlyMemory<byte> Pixels => pixels;

    public static CapturedFrame CreateSolid(int width, int height, byte red, byte green, byte blue)
    {
        var pixels = new byte[width * height * 4];
        for (var i = 0; i < width * height; i++)
        {
            pixels[(i * 4)] = blue;
            pixels[(i * 4) + 1] = green;
            pixels[(i * 4) + 2] = red;
            pixels[(i * 4) + 3] = 255;
        }

        return new CapturedFrame(width, height, width * 4, pixels);
    }

    public CapturedFrame Crop(CaptureRegion region)
    {
        if (region.X < 0 || region.Y < 0 || region.X + region.Width > Width || region.Y + region.Height > Height)
        {
            throw new ArgumentOutOfRangeException(nameof(region), "ROI 必须完全位于截图内。\n");
        }

        var stride = region.Width * 4;
        var cropped = new byte[stride * region.Height];
        for (var y = 0; y < region.Height; y++)
        {
            pixels.AsSpan(((region.Y + y) * Stride) + (region.X * 4), stride)
                .CopyTo(cropped.AsSpan(y * stride, stride));
        }

        return new CapturedFrame(region.Width, region.Height, stride, cropped);
    }
}
