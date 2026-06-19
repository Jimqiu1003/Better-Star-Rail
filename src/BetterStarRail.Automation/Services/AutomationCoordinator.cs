using BetterStarRail.Automation.Abstractions;
using BetterStarRail.Automation.Models;

namespace BetterStarRail.Automation.Services;

public sealed class AutomationCoordinator
{
    public async ValueTask<AutomationStepResult> RunStepAsync(IAutomationStep step, AutomationContext context, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return await step.ExecuteAsync(context, cancellationToken).ConfigureAwait(false);
    }
}
