using BetterStarRail.Automation.Models;
using BetterStarRail.Automation.Safety;
using BetterStarRail.Automation.Workflows;
using BetterStarRail.Core.Models;
using BetterStarRail.Core.Vision;
using BetterStarRail.Core.Windows;
using BetterStarRail.Infrastructure.Windows.Diagnostics;
using BetterStarRail.Infrastructure.Windows.Windowing;
using BetterStarRail.Vision.Abstractions;
using BetterStarRail.Vision.Models;

namespace BetterStarRail.Infrastructure.Windows.Automation;

public sealed class WindowsTestWorkflowEnvironment(
    TargetWindowSnapshot baseline,
    WindowsTargetWindowService windowService,
    IScreenCaptureService captureService,
    IPageDetector pageDetector,
    SafeInputController inputController,
    EmergencyStopController emergencyStop,
    LocalDiagnosticWriter diagnosticWriter) : ITestWorkflowEnvironment
{
    private WorkflowObservation? lastObservation;
    private CapturedFrame? lastFrame;

    public TargetWindowSnapshot Baseline { get; } = baseline;
    public bool EmergencyStopped => emergencyStop.IsStopped;

    public async ValueTask<WorkflowObservation> ObserveAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var window = windowService.ReadSnapshot(Baseline.Identity.Handle);
        if (!window.Succeeded || window.Snapshot is null)
        {
            return FailureObservation(Baseline, SafetyStopReason.TargetWindowChanged, window.Error);
        }

        var region = new CaptureRegion(0, 0,
            Math.Min(32, window.Snapshot.ClientArea.Width),
            Math.Min(32, window.Snapshot.ClientArea.Height));
        var capture = await captureService.CaptureAsync(Baseline.Identity.Handle, region, cancellationToken).ConfigureAwait(false);
        if (!capture.Succeeded || capture.Frame is null)
        {
            return FailureObservation(window.Snapshot, SafetyStopReason.CaptureFailed, capture.Error);
        }

        lastFrame = capture.Frame;
        lastObservation = new WorkflowObservation(window.Snapshot, pageDetector.Detect(capture.Frame));
        return lastObservation;
    }

    public ValueTask<SafetyDecision> DispatchAsync(TestAction action, CancellationToken cancellationToken)
    {
        if (lastObservation is null)
            return ValueTask.FromResult(SafetyDecision.Reject(SafetyStopReason.CaptureFailed, "输入前没有新鲜页面观测。"));
        var request = new SafetyEvaluationRequest(
            Baseline,
            lastObservation.Window,
            lastObservation.Page,
            action,
            EmergencyStopped);
        return inputController.ClickAsync(request, new NormalizedPoint(0.5, 0.75), cancellationToken);
    }

    public ValueTask<OperationResult> WriteFailureAsync(ClosedLoopResult result, CancellationToken cancellationToken) =>
        lastFrame is null
            ? ValueTask.FromResult(OperationResult.Failure("没有可保存的失败帧。"))
            : diagnosticWriter.WriteAsync(Guid.NewGuid(), $"{result.StopReason}: {result.Message}", lastFrame, cancellationToken);

    private WorkflowObservation FailureObservation(
        TargetWindowSnapshot snapshot,
        SafetyStopReason reason,
        string diagnostic)
    {
        lastObservation = new WorkflowObservation(snapshot,
            new PageDetectionResult(TestPageId.UnknownState, 0, [], diagnostic), reason);
        return lastObservation;
    }
}
