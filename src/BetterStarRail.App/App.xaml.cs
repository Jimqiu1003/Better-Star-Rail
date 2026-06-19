using System.Windows;
using System.Windows.Threading;
using BetterStarRail.App.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BetterStarRail.App;

public partial class App : Application
{
    private IHost? host;
    private ILogger<App>? logger;

    public App()
    {
        DispatcherUnhandledException += OnDispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        try
        {
            host = ApplicationHost.Build(e.Args);
            await host.StartAsync().ConfigureAwait(true);
            logger = host.Services.GetRequiredService<ILogger<App>>();

            var window = host.Services.GetRequiredService<MainWindow>();
            MainWindow = window;
            window.Show();
            logger.LogInformation("Better Star Rail V0 application started");
        }
        catch (Exception exception)
        {
            logger?.LogCritical(exception, "Application startup failed");
            Shutdown(-1);
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        if (host is not null)
        {
            logger?.LogInformation("Better Star Rail application stopping");
            host.StopAsync(TimeSpan.FromSeconds(5)).GetAwaiter().GetResult();
            host.Dispose();
        }

        base.OnExit(e);
    }

    private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        logger?.LogCritical(e.Exception, "Unhandled dispatcher exception");
    }

    private void OnUnhandledException(object? sender, UnhandledExceptionEventArgs e)
    {
        logger?.LogCritical(e.ExceptionObject as Exception, "Unhandled application-domain exception");
    }

    private void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        logger?.LogError(e.Exception, "Unobserved task exception");
        e.SetObserved();
    }
}

