using BetterStarRail.Core.Abstractions;

namespace BetterStarRail.Infrastructure.Windows.Storage;

public sealed class LocalAppDataPathProvider : IAppPathProvider
{
    public LocalAppDataPathProvider()
        : this(Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "BetterStarRail"))
    {
    }

    public LocalAppDataPathProvider(string rootDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rootDirectory);
        RootDirectory = Path.GetFullPath(rootDirectory);
        LogsDirectory = Path.Combine(RootDirectory, "logs");
        ConfigurationDirectory = Path.Combine(RootDirectory, "config");
    }

    public string RootDirectory { get; }

    public string LogsDirectory { get; }

    public string ConfigurationDirectory { get; }

    public void EnsureDirectories()
    {
        Directory.CreateDirectory(LogsDirectory);
        Directory.CreateDirectory(ConfigurationDirectory);
    }
}
