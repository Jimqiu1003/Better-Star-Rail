using BetterStarRail.Vision.Models;

namespace BetterStarRail.Vision.Tests;

public sealed class CaptureRegionTests
{
    [Fact]
    public void Constructor_rejects_non_positive_width()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new CaptureRegion(0, 0, 0, 10));
    }

    [Fact]
    public void Constructor_rejects_non_positive_height()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new CaptureRegion(0, 0, 10, -1));
    }

    [Fact]
    public void Constructor_accepts_positive_size()
    {
        var region = new CaptureRegion(0, 0, 10, 10);
        Assert.Equal(10, region.Width);
    }
}
