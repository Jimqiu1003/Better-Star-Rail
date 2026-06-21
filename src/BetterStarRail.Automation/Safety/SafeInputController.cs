using BetterStarRail.Automation.Abstractions;
using BetterStarRail.Automation.Models;
using BetterStarRail.Core.Windows;

namespace BetterStarRail.Automation.Safety;

public sealed class SafeInputController(
    SafetyGuard safetyGuard,
    IWindowInputDispatcher dispatcher,
    EmergencyStopController emergencyStop,
    Func<TargetWindowSnapshot?> currentSnapshotProvider)
{
    public async ValueTask<SafetyDecision> ClickAsync(
        SafetyEvaluationRequest request,
        NormalizedPoint point,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var observedRequest = request with { EmergencyStopped = request.EmergencyStopped || emergencyStop.IsStopped };
        var observedDecision = safetyGuard.Evaluate(observedRequest);
        if (!observedDecision.Allowed) return observedDecision;
        var current = currentSnapshotProvider();
        if (current is null)
            return SafetyDecision.Reject(SafetyStopReason.InvalidTargetWindow, "输入前无法重新读取目标窗口状态。");
        var guardedRequest = request with
        {
            Current = current,
            EmergencyStopped = request.EmergencyStopped || emergencyStop.IsStopped,
        };
        var decision = safetyGuard.Evaluate(guardedRequest);
        if (!decision.Allowed) return decision;
        var pixel = point.ToClientPixel(current.ClientArea);
        var dispatched = await dispatcher.ClickAsync(current.Identity.Handle, pixel, cancellationToken).ConfigureAwait(false);
        if (dispatched.Succeeded) return SafetyDecision.Allow();
        emergencyStop.Stop();
        await dispatcher.ReleaseAllAsync(current.Identity.Handle, CancellationToken.None).ConfigureAwait(false);
        return SafetyDecision.Reject(SafetyStopReason.InputDispatchFailed, dispatched.Message);
    }

    public async ValueTask<SafetyDecision> PressKeyAsync(
        SafetyEvaluationRequest request,
        AllowedKey key,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var observedRequest = request with { EmergencyStopped = request.EmergencyStopped || emergencyStop.IsStopped };
        var observedDecision = safetyGuard.Evaluate(observedRequest);
        if (!observedDecision.Allowed) return observedDecision;
        var current = currentSnapshotProvider();
        if (current is null)
            return SafetyDecision.Reject(SafetyStopReason.InvalidTargetWindow, "输入前无法重新读取目标窗口状态。");
        var guardedRequest = request with
        {
            Current = current,
            EmergencyStopped = request.EmergencyStopped || emergencyStop.IsStopped,
        };
        var decision = safetyGuard.Evaluate(guardedRequest);
        if (!decision.Allowed) return decision;
        var dispatched = await dispatcher.PressKeyAsync(current.Identity.Handle, key, cancellationToken).ConfigureAwait(false);
        if (dispatched.Succeeded) return SafetyDecision.Allow();
        emergencyStop.Stop();
        await dispatcher.ReleaseAllAsync(current.Identity.Handle, CancellationToken.None).ConfigureAwait(false);
        return SafetyDecision.Reject(SafetyStopReason.InputDispatchFailed, dispatched.Message);
    }

    public async ValueTask StopAsync(TargetWindowSnapshot target, CancellationToken cancellationToken)
    {
        // 急停后的释放动作必须完成，不能被调用方已取消的令牌阻断。
        _ = cancellationToken;
        emergencyStop.Stop();
        await dispatcher.ReleaseAllAsync(target.Identity.Handle, CancellationToken.None).ConfigureAwait(false);
    }
}
