using BetterStarRail.Vision.Models;

namespace BetterStarRail.Vision.Abstractions;

public interface IImageMatcher
{
    MatchResult Match(CaptureRegion region);
}
