using BetterStarRail.Infrastructure.Windows.Capture;
using BetterStarRail.Infrastructure.Windows.Diagnostics;
using BetterStarRail.Infrastructure.Windows.Storage;
using BetterStarRail.Vision.Models;

namespace BetterStarRail.Core.Tests;

public sealed class WindowsInfrastructureTests
{
    [Fact]
    public async Task Gdi_capture_returns_clear_failure_for_invalid_handle()
    {
        var result = await new GdiScreenCaptureService().CaptureAsync(0, null, CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Null(result.Frame);
        Assert.Contains("句柄", result.Error);
    }

    [Fact]
    public async Task Gdi_capture_honors_pre_cancelled_token_before_native_work()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await new GdiScreenCaptureService().CaptureAsync((nint)42, null, cancellation.Token));
    }

    [Fact]
    public async Task Diagnostic_writer_saves_local_image_and_structured_metadata()
    {
        var root = Path.Combine(Path.GetTempPath(), $"BetterStarRail.V1.{Guid.NewGuid():N}");
        try
        {
            var paths = new LocalAppDataPathProvider(root);
            paths.EnsureDirectories();
            var writer = new LocalDiagnosticWriter(paths);

            var result = await writer.WriteAsync(
                Guid.Parse("11111111-1111-1111-1111-111111111111"),
                "UnknownPage",
                CapturedFrame.CreateSolid(2, 2, 1, 2, 3),
                CancellationToken.None);

            Assert.True(result.Succeeded);
            Assert.Single(Directory.GetFiles(paths.DiagnosticsDirectory, "*.ppm"));
            var metadata = Assert.Single(Directory.GetFiles(paths.DiagnosticsDirectory, "*.json"));
            var json = await File.ReadAllTextAsync(metadata);
            Assert.Contains("UnknownPage", json);
            Assert.DoesNotContain("password", json, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
        }
    }
}
