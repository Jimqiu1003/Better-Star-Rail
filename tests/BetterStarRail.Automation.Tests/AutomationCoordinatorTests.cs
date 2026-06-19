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

    [Fact]
    public async Task RunStepAsync_throws_when_cancelled_before_execution()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var coordinator = new AutomationCoordinator();
        var step = new RecordingStep(AutomationStepResult.Success());

        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await coordinator.RunStepAsync(step, new AutomationContext(Guid.NewGuid()), cancellation.Token));

        Assert.Equal(0, step.ExecutionCount);
    }

    [Fact]
    public async Task RunStepAsync_returns_failed_step_result_without_swallowing_it()
    {
        var coordinator = new AutomationCoordinator();
        var expected = AutomationStepResult.Failure("步骤失败");

        var actual = await coordinator.RunStepAsync(
            new RecordingStep(expected),
            new AutomationContext(Guid.NewGuid()),
            CancellationToken.None);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task RunWorkflowAsync_completes_empty_workflow_without_executing_steps()
    {
        var coordinator = new AutomationCoordinator();
        var workflow = new TestWorkflow([]);

        var result = await coordinator.RunWorkflowAsync(
            workflow,
            new AutomationContext(Guid.NewGuid()),
            CancellationToken.None);

        Assert.True(result.Succeeded);
    }

    [Fact]
    public async Task RunWorkflowAsync_stops_after_first_failed_step()
    {
        var failedStep = new RecordingStep(AutomationStepResult.Failure("停止"));
        var skippedStep = new RecordingStep(AutomationStepResult.Success());
        var coordinator = new AutomationCoordinator();

        var result = await coordinator.RunWorkflowAsync(
            new TestWorkflow([failedStep, skippedStep]),
            new AutomationContext(Guid.NewGuid()),
            CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(1, failedStep.ExecutionCount);
        Assert.Equal(0, skippedStep.ExecutionCount);
    }

    private sealed class TestStep : IAutomationStep
    {
        public ValueTask<AutomationStepResult> ExecuteAsync(AutomationContext context, CancellationToken cancellationToken)
        {
            return ValueTask.FromResult(AutomationStepResult.Success());
        }
    }

    private sealed class RecordingStep(AutomationStepResult result) : IAutomationStep
    {
        public int ExecutionCount { get; private set; }

        public ValueTask<AutomationStepResult> ExecuteAsync(
            AutomationContext context,
            CancellationToken cancellationToken)
        {
            ExecutionCount++;
            return ValueTask.FromResult(result);
        }
    }

    private sealed class TestWorkflow(IReadOnlyList<IAutomationStep> steps) : IAutomationWorkflow
    {
        public IReadOnlyList<IAutomationStep> Steps { get; } = steps;
    }
}
