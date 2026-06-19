using System.IO;
using BetterStarRail.App.Configuration;
using BetterStarRail.App.ViewModels;
using BetterStarRail.Core.Abstractions;
using BetterStarRail.Core.Diagnostics;
using BetterStarRail.Infrastructure.Windows.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Serilog;

namespace BetterStarRail.App.Hosting;

public static class ApplicationHost
{
    public static IHost Build(string[] args, IAppPathProvider? pathProvider = null)
    {
        var paths = pathProvider ?? new LocalAppDataPathProvider();
        paths.EnsureDirectories();

        var settings = new HostApplicationBuilderSettings
        {
            Args = args,
            ContentRootPath = AppContext.BaseDirectory,
        };
        var builder = Host.CreateApplicationBuilder(settings);
        builder.Configuration.AddJsonFile(
            Path.Combine(paths.ConfigurationDirectory, "appsettings.json"),
            optional: true,
            reloadOnChange: false);

        builder.Services.AddSingleton(paths);
        builder.Services.Configure<ApplicationOptions>(
            builder.Configuration.GetSection(ApplicationOptions.SectionName));
        builder.Services.AddSingleton(static services =>
        {
            var options = services.GetRequiredService<IOptions<ApplicationOptions>>().Value;
            return new AppStatus(options.Stage, options.Status);
        });
        builder.Services.AddSingleton<MainWindowViewModel>();
        builder.Services.AddSingleton<MainWindow>();
        builder.Services.AddSerilog((services, configuration) =>
        {
            var appPaths = services.GetRequiredService<IAppPathProvider>();
            configuration
                .MinimumLevel.Information()
                .Enrich.FromLogContext()
                .WriteTo.File(
                    Path.Combine(appPaths.LogsDirectory, "better-star-rail-.log"),
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 7);
        });

        return builder.Build();
    }
}
