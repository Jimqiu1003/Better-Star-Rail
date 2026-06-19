using BetterStarRail.Vision.Models;

namespace BetterStarRail.Vision.Abstractions;

public interface IScreenCaptureService
{
    ValueTask<ReadOnlyMemory<byte>> CaptureAsync(
        CaptureRegion region,
        CancellationToken cancellationToken);
}
