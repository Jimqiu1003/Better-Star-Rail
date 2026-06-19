namespace BetterStarRail.Automation.Abstractions;

public interface IAutomationWorkflow
{
    IReadOnlyList<IAutomationStep> Steps { get; }
}
