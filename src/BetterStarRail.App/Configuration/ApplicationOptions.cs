namespace BetterStarRail.App.Configuration;

public sealed class ApplicationOptions
{
    public const string SectionName = "Application";

    public string Name { get; set; } = "Better Star Rail";

    public string Version { get; set; } = "0.1.0-dev";

    public string Stage { get; set; } = "V0";

    public string Status { get; set; } = "工程初始化完成";
}
