using BetterStarRail.Core.Vision;
using BetterStarRail.Vision.Models;
using BetterStarRail.Vision.Services;

namespace BetterStarRail.Vision.Tests;

public sealed class PageDetectorTests
{
    public static TheoryData<string, TestPageId> GoldenPages => new()
    {
        { "welcome.ppm", TestPageId.Welcome },
        { "confirm.ppm", TestPageId.Confirm },
        { "progress.ppm", TestPageId.Progress },
        { "complete.ppm", TestPageId.Complete },
        { "blocked.ppm", TestPageId.Blocked },
    };

    [Theory]
    [MemberData(nameof(GoldenPages))]
    public void Detect_recognizes_project_owned_golden_pages(string fileName, TestPageId expected)
    {
        var frame = PpmTestImage.Load(Path.Combine(AppContext.BaseDirectory, "GoldenImages", fileName));

        var result = new SolidColorPageDetector().Detect(frame);

        Assert.Equal(expected, result.PageId);
        Assert.True(result.Confidence >= 0.95);
        Assert.NotEmpty(result.EvidenceRegions);
    }

    [Fact]
    public void Detect_returns_unknown_for_negative_sample()
    {
        var frame = PpmTestImage.Load(Path.Combine(AppContext.BaseDirectory, "GoldenImages", "unknown.ppm"));

        var result = new SolidColorPageDetector().Detect(frame);

        Assert.Equal(TestPageId.UnknownState, result.PageId);
    }

    [Fact]
    public void Detect_returns_unknown_when_best_match_has_low_confidence()
    {
        var frame = CapturedFrame.CreateSolid(8, 8, 40, 100, 160);

        var result = new SolidColorPageDetector(minimumConfidence: 0.95).Detect(frame);

        Assert.Equal(TestPageId.UnknownState, result.PageId);
        Assert.InRange(result.Confidence, 0.01, 0.949999);
        Assert.Contains("低置信度", result.Diagnostic);
    }

    [Fact]
    public void Detect_honors_pre_cancelled_token()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        Assert.Throws<OperationCanceledException>(() =>
            new SolidColorPageDetector().Detect(
                CapturedFrame.CreateSolid(8, 8, 30, 120, 220),
                cancellation.Token));
    }

    [Fact]
    public void Crop_rejects_roi_outside_frame()
    {
        var frame = CapturedFrame.CreateSolid(4, 4, 1, 2, 3);

        Assert.Throws<ArgumentOutOfRangeException>(() => frame.Crop(new CaptureRegion(3, 3, 2, 2)));
    }
}
