using System.Globalization;
using BetterStarRail.Vision.Models;

namespace BetterStarRail.Vision.Tests;

internal static class PpmTestImage
{
    public static CapturedFrame Load(string path)
    {
        var tokens = File.ReadAllText(path).Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        Assert.Equal("P3", tokens[0]);
        var width = int.Parse(tokens[1], CultureInfo.InvariantCulture);
        var height = int.Parse(tokens[2], CultureInfo.InvariantCulture);
        Assert.Equal("255", tokens[3]);
        var pixels = new byte[width * height * 4];
        for (var i = 0; i < width * height; i++)
        {
            var source = 4 + (i * 3);
            var target = i * 4;
            pixels[target] = byte.Parse(tokens[source + 2], CultureInfo.InvariantCulture);
            pixels[target + 1] = byte.Parse(tokens[source + 1], CultureInfo.InvariantCulture);
            pixels[target + 2] = byte.Parse(tokens[source], CultureInfo.InvariantCulture);
            pixels[target + 3] = 255;
        }

        return new CapturedFrame(width, height, width * 4, pixels);
    }
}
