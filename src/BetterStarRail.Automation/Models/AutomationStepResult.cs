namespace BetterStarRail.Automation.Models;

public sealed record AutomationStepResult(bool Succeeded, string Message)
{
    public static AutomationStepResult Success(string message = "Step completed") => new(true, message);
    public static AutomationStepResult Failure(string message) => new(false, message);
}
