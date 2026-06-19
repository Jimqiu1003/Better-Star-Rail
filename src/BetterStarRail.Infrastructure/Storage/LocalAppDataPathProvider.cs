namespace BetterStarRail.Infrastructure.Storage;

public sealed class LocalAppDataPathProvider
{
    public string RootPath { get; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "BetterStarRail");
}
