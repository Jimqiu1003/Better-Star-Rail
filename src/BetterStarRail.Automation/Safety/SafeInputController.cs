using BetterStarRail.Automation.Abstractions;
using BetterStarRail.Automation.Models;
using BetterStarRail.Core.Windows;

namespace BetterStarRail.Automation.Safety;

public sealed class SafeInputController(
    SafetyGuard safetyGuard,
    IWindowInputDispatcher dispatcher,
    EmergencyStopController emergencyStop)
{
    public async ValueTask<SafetyDecision> ClickAsync(
        SafetyEvaluationRequest request,
        NormalizedPoint point,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var guardedRequest = request with { EmergencyStopped = request.EmergencyStopped || emergencyStop.IsStopped };
        var decision = safetyGuard.Evaluate(guardedRequest);
        if (!decision.Allowed) return decision;
        var pixel = point.ToClientPixel(request.Current.ClientArea);
        var dispatched = await dispatcher.ClickAsync(request.Current.Identity.Handle, pixel, cancellationToken).ConfigureAwait(false);
        return dispatched.Succeeded
            ? SafetyDecision.Allow()
            : SafetyDecision.Reject(SafetyStopReason.InputDispatchFailed, dispatched.Message);
    }

    public async ValueTask<SafetyDecision> PressKeyAsync(
        SafetyEvaluationRequest request,
        AllowedKey key,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var guardedRequest = request with { EmergencyStopped = request.EmergencyStopped || emergencyStop.IsStopped };
        var decision = safetyGuard.Evaluate(guardedRequest);
        if (!decision.Allowed) return decision;
        var dispatched = await dispatcher.PressKeyAsync(request.Current.Identity.Handle, key, cancellationToken).ConfigureAwait(false);
        return dispatched.Succeeded
            ? SafetyDecision.Allow()
            : SafetyDecision.Reject(SafetyStopReason.InputDispatchFailed, dispatched.Message);
    }

    public async ValueTask StopAsync(TargetWindowSnapshot target, CancellationToken cancellationToken)
    {
        emergencyStop.Stop();
        await dispatcher.ReleaseAllAsync(target.Identity.Handle, cancellationToken).ConfigureAwait(false);
    }
}
