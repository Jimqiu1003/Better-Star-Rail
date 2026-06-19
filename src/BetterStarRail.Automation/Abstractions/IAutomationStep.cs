using BetterStarRail.Automation.Models;

namespace BetterStarRail.Automation.Abstractions;

public interface IAutomationStep
{
    ValueTask<AutomationStepResult> ExecuteAsync(AutomationContext context, CancellationToken cancellationToken);
}
