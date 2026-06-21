using BetterStarRail.Automation.Models;
using BetterStarRail.Automation.Safety;
using BetterStarRail.Automation.Workflows;
using BetterStarRail.Core.Vision;
using BetterStarRail.Core.Windows;

namespace BetterStarRail.Automation.Tests;

public sealed class TestClosedLoopWorkflowTests
{
    [Fact]
    public async Task RunAsync_completes_two_verified_page_transitions()
    {
        var environment = new InMemoryTestEnvironment();
        var runner = CreateRunner(environment);

        var result = await runner.RunAsync(CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Equal(TestPageId.Complete, environment.Page);
        Assert.Equal([TestAction.Advance, TestAction.Confirm], environment.Actions);
    }

    [Fact]
    public async Task RunAsync_stops_on_unknown_state_without_input()
    {
        var environment = new InMemoryTestEnvironment { Page = TestPageId.UnknownState };

        var result = await CreateRunner(environment).RunAsync(CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(SafetyStopReason.UnknownPage, result.StopReason);
        Assert.Empty(environment.Actions);
    }

    [Fact]
    public async Task RunAsync_never_dispatches_to_wrong_window()
    {
        var environment = new InMemoryTestEnvironment { ReturnWrongHandle = true };

        var result = await CreateRunner(environment).RunAsync(CancellationToken.None);

        Assert.Equal(SafetyStopReason.TargetWindowChanged, result.StopReason);
        Assert.Empty(environment.Actions);
    }

    [Fact]
    public async Task RunAsync_honors_cancellation()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await CreateRunner(new InMemoryTestEnvironment()).RunAsync(cancellation.Token));
    }

    [Fact]
    public async Task RunAsync_times_out_after_bounded_post_action_checks()
    {
        var environment = new InMemoryTestEnvironment { FreezePageAfterAction = true };
        var runner = new TestClosedLoopWorkflow(environment, new SafetyGuard(new ActionWhitelist()), maxPostActionChecks: 2);

        var result = await runner.RunAsync(CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(SafetyStopReason.PostConditionTimeout, result.StopReason);
        Assert.Single(environment.Actions);
        Assert.Equal(2, result.VerificationAttempts);
    }

    [Fact]
    public async Task RunAsync_completes_one_hundred_deterministic_closed_loops()
    {
        var completed = 0;
        for (var i = 0; i < 100; i++)
        {
            var result = await CreateRunner(new InMemoryTestEnvironment()).RunAsync(CancellationToken.None);
            if (result.Succeeded) completed++;
        }

        Assert.Equal(100, completed);
    }

    private static TestClosedLoopWorkflow CreateRunner(InMemoryTestEnvironment environment) =>
        new(environment, new SafetyGuard(new ActionWhitelist()), maxPostActionChecks: 3);

    private sealed class InMemoryTestEnvironment : ITestWorkflowEnvironment
    {
        private readonly TargetWindowIdentity identity = new((nint)42, "Better Star Rail V1 Test Window", "TestClass", 7);

        public TestPageId Page { get; set; } = TestPageId.Welcome;
        public bool ReturnWrongHandle { get; set; }
        public bool FreezePageAfterAction { get; set; }
        public List<TestAction> Actions { get; } = [];
        public TargetWindowSnapshot Baseline => new(identity, new ClientArea(0, 0, 800, 600, 96), true, false, 10);
        public bool EmergencyStopped => false;

        public ValueTask<WorkflowObservation> ObserveAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var observedIdentity = ReturnWrongHandle ? identity with { Handle = (nint)999 } : identity;
            var snapshot = new TargetWindowSnapshot(observedIdentity, new ClientArea(0, 0, 800, 600, 96), true, false, 10);
            var confidence = Page == TestPageId.UnknownState ? 0.2 : 0.99;
            return ValueTask.FromResult(new WorkflowObservation(snapshot,
                new PageDetectionResult(Page, confidence, [new EvidenceRegion(0, 0, 800, 600)], "内存测试窗口")));
        }

        public ValueTask<SafetyDecision> DispatchAsync(TestAction action, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Actions.Add(action);
            if (!FreezePageAfterAction)
            {
                Page = action switch
                {
                    TestAction.Advance => TestPageId.Confirm,
                    TestAction.Confirm => TestPageId.Complete,
                    _ => Page,
                };
            }

            return ValueTask.FromResult(SafetyDecision.Allow());
        }
    }
}
