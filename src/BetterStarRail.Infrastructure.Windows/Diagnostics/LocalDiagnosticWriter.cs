using System.Text;
using System.Text.Json;
using BetterStarRail.Core.Abstractions;
using BetterStarRail.Core.Models;
using BetterStarRail.Vision.Models;

namespace BetterStarRail.Infrastructure.Windows.Diagnostics;

public sealed class LocalDiagnosticWriter(IAppPathProvider paths)
{
    public async ValueTask<OperationResult> WriteAsync(
        Guid runId,
        string failureReason,
        CapturedFrame frame,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(failureReason);
        Directory.CreateDirectory(paths.DiagnosticsDirectory);
        var stem = $"v1-{DateTimeOffset.UtcNow:yyyyMMdd-HHmmssfff}-{runId:N}";
        var imagePath = Path.Combine(paths.DiagnosticsDirectory, stem + ".ppm");
        var metadataPath = Path.Combine(paths.DiagnosticsDirectory, stem + ".json");
        try
        {
            await WritePpmAsync(imagePath, frame, cancellationToken).ConfigureAwait(false);
            var metadata = JsonSerializer.Serialize(new
            {
                SchemaVersion = 1,
                RunId = runId,
                FailureReason = failureReason,
                CapturedAtUtc = DateTimeOffset.UtcNow,
                frame.Width,
                frame.Height,
                LocalOnly = true,
            });
            await File.WriteAllTextAsync(metadataPath, metadata, Encoding.UTF8, cancellationToken).ConfigureAwait(false);
            return OperationResult.Success("本地诊断已保存。");
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            return OperationResult.Failure($"保存本地诊断失败：{exception.Message}");
        }
    }

    private static async Task WritePpmAsync(string path, CapturedFrame frame, CancellationToken cancellationToken)
    {
        await using var stream = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None, 8192, useAsync: true);
        var header = Encoding.ASCII.GetBytes($"P6\n{frame.Width} {frame.Height}\n255\n");
        await stream.WriteAsync(header, cancellationToken).ConfigureAwait(false);
        var rgb = new byte[frame.Width * frame.Height * 3];
        var source = frame.Pixels.Span;
        var target = 0;
        for (var y = 0; y < frame.Height; y++)
        {
            for (var x = 0; x < frame.Width; x++)
            {
                var offset = (y * frame.Stride) + (x * 4);
                rgb[target++] = source[offset + 2];
                rgb[target++] = source[offset + 1];
                rgb[target++] = source[offset];
            }
        }

        await stream.WriteAsync(rgb, cancellationToken).ConfigureAwait(false);
    }
}
