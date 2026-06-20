using BetterStarRail.Core.Vision;
using BetterStarRail.Vision.Abstractions;
using BetterStarRail.Vision.Models;

namespace BetterStarRail.Vision.Services;

public sealed class SolidColorPageDetector(double minimumConfidence = 0.95) : IPageDetector
{
    private static readonly IReadOnlyDictionary<TestPageId, (byte R, byte G, byte B)> Signatures =
        new Dictionary<TestPageId, (byte R, byte G, byte B)>
        {
            [TestPageId.Welcome] = (30, 120, 220),
            [TestPageId.Confirm] = (220, 160, 30),
            [TestPageId.Progress] = (120, 60, 200),
            [TestPageId.Complete] = (40, 180, 90),
            [TestPageId.Blocked] = (220, 50, 60),
        };

    public PageDetectionResult Detect(CapturedFrame frame, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(frame);
        cancellationToken.ThrowIfCancellationRequested();
        var pixels = frame.Pixels.Span;
        long red = 0, green = 0, blue = 0;
        var count = frame.Width * frame.Height;
        for (var y = 0; y < frame.Height; y++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            for (var x = 0; x < frame.Width; x++)
            {
                var offset = (y * frame.Stride) + (x * 4);
                blue += pixels[offset];
                green += pixels[offset + 1];
                red += pixels[offset + 2];
            }
        }

        var average = (R: red / (double)count, G: green / (double)count, B: blue / (double)count);
        var nearest = Signatures
            .Select(pair => (pair.Key, Distance: Distance(average, pair.Value)))
            .OrderBy(candidate => candidate.Distance)
            .First();
        var confidence = Math.Clamp(1 - (nearest.Distance / Math.Sqrt(3 * 255d * 255d)), 0, 1);
        var evidence = new[] { new EvidenceRegion(0, 0, frame.Width, frame.Height) };
        return confidence >= minimumConfidence
            ? new PageDetectionResult(nearest.Key, confidence, evidence, $"纯色锚点匹配：{nearest.Key}")
            : new PageDetectionResult(TestPageId.UnknownState, confidence, evidence, $"低置信度：{confidence:F3}");
    }

    private static double Distance((double R, double G, double B) left, (byte R, byte G, byte B) right) =>
        Math.Sqrt(Math.Pow(left.R - right.R, 2) + Math.Pow(left.G - right.G, 2) + Math.Pow(left.B - right.B, 2));
}
