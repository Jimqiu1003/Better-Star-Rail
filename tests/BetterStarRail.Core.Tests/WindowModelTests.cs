using BetterStarRail.Core.Windows;

namespace BetterStarRail.Core.Tests;

public sealed class WindowModelTests
{
    [Theory]
    [InlineData(-0.01, 0.5)]
    [InlineData(1.01, 0.5)]
    [InlineData(0.5, -0.01)]
    [InlineData(0.5, 1.01)]
    public void NormalizedPoint_rejects_values_outside_client_area(double x, double y)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new NormalizedPoint(x, y));
    }

    [Fact]
    public void NormalizedPoint_converts_to_client_pixel_without_screen_coordinates()
    {
        var area = new ClientArea(900, 500, 800, 600, 144);

        var pixel = new NormalizedPoint(0.5, 0.25).ToClientPixel(area);

        Assert.Equal(new ClientPoint(400, 150), pixel);
    }

    [Fact]
    public void Snapshot_detects_handle_size_and_dpi_changes()
    {
        var identity = new TargetWindowIdentity((nint)42, "Better Star Rail V1 Test Window", "TestClass", 7);
        var before = new TargetWindowSnapshot(identity, new ClientArea(10, 20, 800, 600, 96), true, false, 10);
        var after = new TargetWindowSnapshot(identity with { Handle = (nint)43 }, new ClientArea(10, 20, 1024, 768, 144), true, false, 10);

        var changes = before.CompareSafetyCriticalState(after);

        Assert.Contains(WindowChange.Handle, changes);
        Assert.Contains(WindowChange.Size, changes);
        Assert.Contains(WindowChange.Dpi, changes);
    }
}
