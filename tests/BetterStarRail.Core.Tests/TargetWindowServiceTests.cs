using BetterStarRail.Infrastructure.Windows.Windowing;

namespace BetterStarRail.Core.Tests;

public sealed class TargetWindowServiceTests
{
    [Fact]
    public void Constructor_rejects_blank_test_window_title()
    {
        Assert.Throws<ArgumentException>(() => new WindowsTargetWindowService(" "));
    }

    [Fact]
    public void ReadSnapshot_returns_clear_failure_for_invalid_handle()
    {
        var service = new WindowsTargetWindowService("Better Star Rail V1 Test Window");

        var result = service.ReadSnapshot(0);

        Assert.False(result.Succeeded);
        Assert.Null(result.Snapshot);
        Assert.Contains("句柄", result.Error);
    }
}
