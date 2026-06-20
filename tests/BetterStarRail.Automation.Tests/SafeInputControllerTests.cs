using BetterStarRail.Automation.Abstractions;
using BetterStarRail.Automation.Models;
using BetterStarRail.Automation.Safety;
using BetterStarRail.Core.Models;
using BetterStarRail.Core.Vision;
using BetterStarRail.Core.Windows;

namespace BetterStarRail.Automation.Tests;

public sealed class SafeInputControllerTests
{
    private static readonly TargetWindowIdentity Identity = new((nint)42, "Better Star Rail V1 Test Window", "TestClass", 7);
    private static readonly TargetWindowSnapshot Snapshot = new(Identity, new ClientArea(0, 0, 800, 600, 96), true, false, 1);

    [Fact]
    public async Task ClickAsync_dispatches_only_after_guard_approval()
    {
        var raw = new RecordingInputDispatcher();
        var controller = CreateController(raw);

        var result = await controller.ClickAsync(Request(), new NormalizedPoint(0.5, 0.75), CancellationToken.None);

        Assert.True(result.Allowed);
        Assert.Equal(1, raw.Clicks);
        Assert.Equal(new ClientPoint(400, 450), raw.LastPoint);
    }

    [Fact]
    public async Task ClickAsync_never_dispatches_to_changed_window()
    {
        var raw = new RecordingInputDispatcher();
        var controller = CreateController(raw);
        var changed = Snapshot with { Identity = Identity with { Handle = (nint)999 } };

        var result = await controller.ClickAsync(Request(current: changed), new NormalizedPoint(0.5, 0.75), CancellationToken.None);

        Assert.Equal(SafetyStopReason.TargetWindowChanged, result.StopReason);
        Assert.Equal(0, raw.Clicks);
    }

    [Fact]
    public async Task StopAsync_blocks_new_input_and_releases_pressed_keys()
    {
        var raw = new RecordingInputDispatcher();
        var emergency = new EmergencyStopController();
        var controller = CreateController(raw, emergency);

        await controller.StopAsync(Snapshot, CancellationToken.None);
        var result = await controller.ClickAsync(Request(), new NormalizedPoint(0.5, 0.75), CancellationToken.None);

        Assert.Equal(SafetyStopReason.EmergencyStop, result.StopReason);
        Assert.Equal(0, raw.Clicks);
        Assert.Equal(1, raw.ReleaseCalls);
    }

    private static SafeInputController CreateController(RecordingInputDispatcher raw, EmergencyStopController? emergency = null) =>
        new(new SafetyGuard(new ActionWhitelist()), raw, emergency ?? new EmergencyStopController());

    private static SafetyEvaluationRequest Request(TargetWindowSnapshot? current = null) => new(
        Snapshot,
        current ?? Snapshot,
        new PageDetectionResult(TestPageId.Welcome, 0.99, [new EvidenceRegion(0, 0, 10, 10)], "测试"),
        TestAction.Advance,
        false);

    private sealed class RecordingInputDispatcher : IWindowInputDispatcher
    {
        public int Clicks { get; private set; }
        public int ReleaseCalls { get; private set; }
        public ClientPoint LastPoint { get; private set; }

        public ValueTask<OperationResult> ClickAsync(nint windowHandle, ClientPoint point, CancellationToken cancellationToken)
        {
            Clicks++;
            LastPoint = point;
            return ValueTask.FromResult(OperationResult.Success());
        }

        public ValueTask<OperationResult> PressKeyAsync(nint windowHandle, AllowedKey key, CancellationToken cancellationToken) =>
            ValueTask.FromResult(OperationResult.Success());

        public ValueTask ReleaseAllAsync(nint windowHandle, CancellationToken cancellationToken)
        {
            ReleaseCalls++;
            return ValueTask.CompletedTask;
        }
    }
}
