using BetterStarRail.Vision.Abstractions;
using BetterStarRail.Vision.Models;
using BetterStarRail.Vision.Services;

namespace BetterStarRail.Vision.Tests;

public sealed class CaptureStabilityProbeTests
{
    [Fact]
    public async Task RunAsync_reports_one_hundred_stable_captures()
    {
        var probe = new CaptureStabilityProbe(new StableCaptureService());

        var result = await probe.RunAsync((nint)42, 100, CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Equal(100, result.SuccessfulCaptures);
        Assert.Equal(0, result.DimensionChanges);
        Assert.Equal(0, result.Failures);
    }

    [Fact]
    public async Task RunAsync_honors_cancellation_without_retrying()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await new CaptureStabilityProbe(new StableCaptureService()).RunAsync((nint)42, 5, cancellation.Token));
    }

    private sealed class StableCaptureService : IScreenCaptureService
    {
        public ValueTask<CaptureResult> CaptureAsync(nint windowHandle, CaptureRegion? region, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return ValueTask.FromResult(CaptureResult.Success(CapturedFrame.CreateSolid(16, 16, 30, 120, 220)));
        }
    }
}
