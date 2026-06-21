using BetterStarRail.Core.Vision;
using BetterStarRail.Vision.Models;

namespace BetterStarRail.Vision.Abstractions;

public interface IPageDetector
{
    PageDetectionResult Detect(CapturedFrame frame, CancellationToken cancellationToken = default);
}
