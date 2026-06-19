using BetterStarRail.Automation.Abstractions;
using BetterStarRail.Automation.Models;

namespace BetterStarRail.Automation.Services;

public sealed class AutomationCoordinator
{
    public async ValueTask<AutomationStepResult> RunWorkflowAsync(
        IAutomationWorkflow workflow,
        AutomationContext context,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(workflow);
        foreach (var step in workflow.Steps)
        {
            var result = await RunStepAsync(step, context, cancellationToken).ConfigureAwait(false);
            if (!result.Succeeded)
            {
                return result;
            }
        }

        return AutomationStepResult.Success("工作流执行完成");
    }

    public async ValueTask<AutomationStepResult> RunStepAsync(IAutomationStep step, AutomationContext context, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return await step.ExecuteAsync(context, cancellationToken).ConfigureAwait(false);
    }
}
