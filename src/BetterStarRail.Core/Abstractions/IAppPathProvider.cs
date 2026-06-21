namespace BetterStarRail.Core.Abstractions;

public interface IAppPathProvider
{
    string RootDirectory { get; }

    string LogsDirectory { get; }

    string ConfigurationDirectory { get; }

    string DiagnosticsDirectory { get; }

    void EnsureDirectories();
}
