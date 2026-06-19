using BetterStarRail.App.Configuration;
using BetterStarRail.Core.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Options;

namespace BetterStarRail.App.ViewModels;

public sealed class MainWindowViewModel : ObservableObject
{
    public MainWindowViewModel(IOptions<ApplicationOptions> options, AppStatus appStatus)
    {
        ApplicationName = options.Value.Name;
        Version = options.Value.Version;
        Stage = appStatus.Stage;
        Status = appStatus.Message;
    }

    public string ApplicationName { get; }

    public string Version { get; }

    public string Stage { get; }

    public string Status { get; }
}
