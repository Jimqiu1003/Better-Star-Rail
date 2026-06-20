namespace BetterStarRail.Core.Vision;

public enum TestPageId
{
    UnknownState,
    Welcome,
    Confirm,
    Progress,
    Complete,
    Blocked,
}

public sealed record EvidenceRegion(int X, int Y, int Width, int Height);

public sealed record PageDetectionResult(
    TestPageId PageId,
    double Confidence,
    IReadOnlyList<EvidenceRegion> EvidenceRegions,
    string Diagnostic);
