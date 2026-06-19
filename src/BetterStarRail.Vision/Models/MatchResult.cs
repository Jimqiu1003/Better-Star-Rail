namespace BetterStarRail.Vision.Models;

public sealed record MatchResult(bool IsMatch, double Confidence, CaptureRegion? Region);
