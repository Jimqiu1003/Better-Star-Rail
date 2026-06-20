using BetterStarRail.Vision.Abstractions;

namespace BetterStarRail.Vision.Services;

public sealed record CaptureStabilityResult(bool Succeeded, int SuccessfulCaptures, int Failures, int DimensionChanges, string Diagnostic);

public sealed class CaptureStabilityProbe(IScreenCaptureService captureService)
{
    public async ValueTask<CaptureStabilityResult> RunAsync(
        nint windowHandle,
        int captureCount,
        CancellationToken cancellationToken)
    {
        if (captureCount <= 0) throw new ArgumentOutOfRangeException(nameof(captureCount));
        var successes = 0;
        var failures = 0;
        var dimensionChanges = 0;
        (int Width, int Height)? expected = null;
        for (var index = 0; index < captureCount; index++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var result = await captureService.CaptureAsync(windowHandle, null, cancellationToken).ConfigureAwait(false);
            if (!result.Succeeded || result.Frame is null)
            {
                failures++;
                continue;
            }

            successes++;
            var dimensions = (result.Frame.Width, result.Frame.Height);
            expected ??= dimensions;
            if (expected != dimensions) dimensionChanges++;
        }

        var succeeded = successes == captureCount && failures == 0 && dimensionChanges == 0;
        return new CaptureStabilityResult(succeeded, successes, failures, dimensionChanges,
            $"成功 {successes}/{captureCount}，失败 {failures}，尺寸变化 {dimensionChanges}。");
    }
}
