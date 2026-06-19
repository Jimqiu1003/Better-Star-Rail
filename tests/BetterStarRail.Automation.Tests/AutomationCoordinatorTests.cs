using BetterStarRail.Automation.Abstractions;
using BetterStarRail.Automation.Models;
using BetterStarRail.Automation.Services;

namespace BetterStarRail.Automation.Tests;

public sealed class AutomationCoordinatorTests
{
    [Fact]
    public async Task RunStepAsync_returns_step_result()
    {
        var coordinator = new AutomationCoordinator();
        var result = await coordinator.RunStepAsync(new TestStep(), new AutomationContext(Guid.NewGuid()), CancellationToken.None);
        Assert.True(result.Succeeded);
    }

    private sealed class TestStep : IAutomationStep
    {
        public ValueTask<AutomationStepResult> ExecuteAsync(AutomationContext context, CancellationToken cancellationToken)
        {
            return ValueTask.FromResult(AutomationStepResult.Success());
        }
    }
}
