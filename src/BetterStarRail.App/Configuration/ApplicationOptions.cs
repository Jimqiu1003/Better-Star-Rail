namespace BetterStarRail.App.Configuration;

public sealed class ApplicationOptions
{
    public const string SectionName = "Application";

    public string Name { get; set; } = "Better Star Rail";

    public string Version { get; set; } = "0.2.0-dev";

    public string Stage { get; set; } = "V1";

    public string Status { get; set; } = "V1 自建测试窗口安全闭环已完成，待主线集成";
}
