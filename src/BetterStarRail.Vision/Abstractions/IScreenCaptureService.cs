using BetterStarRail.Vision.Models;

namespace BetterStarRail.Vision.Abstractions;

public interface IScreenCaptureService
{
    ValueTask<CaptureResult> CaptureAsync(
        nint windowHandle,
        CaptureRegion? region,
        CancellationToken cancellationToken);
}
