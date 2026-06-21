namespace BetterStarRail.Vision.Models;

public sealed record CaptureResult(bool Succeeded, CapturedFrame? Frame, string Error)
{
    public static CaptureResult Success(CapturedFrame frame) => new(true, frame, string.Empty);
    public static CaptureResult Failure(string error) => new(false, null, error);
}
