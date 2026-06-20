using BetterStarRail.App.Hosting;
using BetterStarRail.App.ViewModels;
using BetterStarRail.Infrastructure.Windows.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BetterStarRail.Core.Tests;

public sealed class ApplicationBaselineTests
{
    [Fact]
    public void Default_paths_use_local_app_data()
    {
        var expectedRoot = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "BetterStarRail");
        var provider = new LocalAppDataPathProvider();

        Assert.Equal(expectedRoot, provider.RootDirectory);
        Assert.Equal(Path.Combine(expectedRoot, "logs"), provider.LogsDirectory);
        Assert.Equal(Path.Combine(expectedRoot, "config"), provider.ConfigurationDirectory);
        Assert.Equal(Path.Combine(expectedRoot, "diagnostics"), provider.DiagnosticsDirectory);
        Assert.False(provider.RootDirectory.StartsWith(Directory.GetCurrentDirectory(), StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void EnsureDirectories_creates_log_and_config_directories()
    {
        var root = CreateTemporaryRoot();

        try
        {
            var provider = new LocalAppDataPathProvider(root);
            provider.EnsureDirectories();

            Assert.True(Directory.Exists(provider.LogsDirectory));
            Assert.True(Directory.Exists(provider.ConfigurationDirectory));
            Assert.True(Directory.Exists(provider.DiagnosticsDirectory));
        }
        finally
        {
            if (Directory.Exists(root))
            {
                Directory.Delete(root, recursive: true);
            }
        }
    }

    [Fact]
    public void ApplicationHost_resolves_main_window_view_model()
    {
        var root = CreateTemporaryRoot();

        try
        {
            var paths = new LocalAppDataPathProvider(root);
            using var host = ApplicationHost.Build([], paths);

            var viewModel = host.Services.GetRequiredService<MainWindowViewModel>();

            Assert.Equal("Better Star Rail", viewModel.ApplicationName);
            Assert.Equal("V1", viewModel.Stage);
            Assert.Equal("V1 自建测试窗口安全闭环已完成，待主线集成", viewModel.Status);
        }
        finally
        {
            if (Directory.Exists(root))
            {
                Directory.Delete(root, recursive: true);
            }
        }
    }

    [Fact]
    public async Task ApplicationHost_writes_logs_to_configured_local_path()
    {
        var root = CreateTemporaryRoot();

        try
        {
            var paths = new LocalAppDataPathProvider(root);
            using (var host = ApplicationHost.Build([], paths))
            {
                await host.StartAsync();
                var logger = host.Services.GetRequiredService<ILogger<ApplicationBaselineTests>>();
                logger.LogInformation("V0 logging verification");
                await host.StopAsync();
            }

            var logFiles = Directory.GetFiles(
                Path.Combine(root, "logs"),
                "better-star-rail-*.log",
                SearchOption.TopDirectoryOnly);
            Assert.Single(logFiles);
            Assert.StartsWith(root, logFiles[0], StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            if (Directory.Exists(root))
            {
                Directory.Delete(root, recursive: true);
            }
        }
    }

    private static string CreateTemporaryRoot()
    {
        return Path.Combine(Path.GetTempPath(), $"BetterStarRail.Tests.{Guid.NewGuid():N}");
    }
}
