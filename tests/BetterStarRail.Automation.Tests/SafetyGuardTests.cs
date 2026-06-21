using BetterStarRail.Automation.Models;
using BetterStarRail.Automation.Safety;
using BetterStarRail.Core.Vision;
using BetterStarRail.Core.Windows;

namespace BetterStarRail.Automation.Tests;

public sealed class SafetyGuardTests
{
    private static readonly TargetWindowIdentity Identity = new((nint)42, "Better Star Rail V1 Test Window", "TestClass", 7);
    private static readonly TargetWindowSnapshot Baseline = new(Identity, new ClientArea(0, 0, 800, 600, 96), true, false, 100);

    [Fact]
    public void Evaluate_allows_whitelisted_action_when_all_checks_are_current()
    {
        var result = new SafetyGuard(new ActionWhitelist()).Evaluate(Request());

        Assert.True(result.Allowed);
    }

    [Theory]
    [InlineData(false, false, SafetyStopReason.WindowNotForeground)]
    [InlineData(true, true, SafetyStopReason.WindowMinimized)]
    public void Evaluate_rejects_focus_or_minimize_failures(bool foreground, bool minimized, SafetyStopReason expected)
    {
        var current = Baseline with { IsForeground = foreground, IsMinimized = minimized };

        var result = new SafetyGuard(new ActionWhitelist()).Evaluate(Request(current: current));

        Assert.False(result.Allowed);
        Assert.Equal(expected, result.StopReason);
    }

    [Fact]
    public void Evaluate_rejects_wrong_window_handle()
    {
        var current = Baseline with { Identity = Identity with { Handle = (nint)999 } };

        var result = new SafetyGuard(new ActionWhitelist()).Evaluate(Request(current: current));

        Assert.Equal(SafetyStopReason.TargetWindowChanged, result.StopReason);
    }

    [Fact]
    public void Evaluate_rejects_client_size_position_and_dpi_changes()
    {
        var guard = new SafetyGuard(new ActionWhitelist());
        var changedSnapshots = new[]
        {
            Baseline with { ClientArea = new ClientArea(1, 0, 800, 600, 96) },
            Baseline with { ClientArea = new ClientArea(0, 0, 1024, 600, 96) },
            Baseline with { ClientArea = new ClientArea(0, 0, 800, 600, 144) },
        };

        foreach (var current in changedSnapshots)
            Assert.Equal(SafetyStopReason.ClientAreaChanged, guard.Evaluate(Request(current: current)).StopReason);
    }

    [Fact]
    public void Evaluate_rejects_low_confidence_unknown_and_non_whitelisted_actions()
    {
        var guard = new SafetyGuard(new ActionWhitelist());

        Assert.Equal(SafetyStopReason.LowConfidence, guard.Evaluate(Request(detection: Detection(TestPageId.Welcome, 0.5))).StopReason);
        Assert.Equal(SafetyStopReason.UnknownPage, guard.Evaluate(Request(detection: Detection(TestPageId.UnknownState, 0.8))).StopReason);
        Assert.Equal(SafetyStopReason.ActionNotAllowed, guard.Evaluate(Request(action: TestAction.Cancel)).StopReason);
    }

    [Fact]
    public void Evaluate_rejects_user_input_and_emergency_stop()
    {
        var guard = new SafetyGuard(new ActionWhitelist());

        Assert.Equal(SafetyStopReason.UserInputDetected, guard.Evaluate(Request(current: Baseline with { UserInputGeneration = 101 })).StopReason);
        Assert.Equal(SafetyStopReason.EmergencyStop, guard.Evaluate(Request(emergencyStopped: true)).StopReason);
    }

    private static SafetyEvaluationRequest Request(
        TargetWindowSnapshot? current = null,
        PageDetectionResult? detection = null,
        TestAction action = TestAction.Advance,
        bool emergencyStopped = false) =>
        new(Baseline, current ?? Baseline, detection ?? Detection(TestPageId.Welcome, 0.99), action, emergencyStopped);

    private static PageDetectionResult Detection(TestPageId page, double confidence) =>
        new(page, confidence, [new EvidenceRegion(0, 0, 1, 1)], "测试");
}
