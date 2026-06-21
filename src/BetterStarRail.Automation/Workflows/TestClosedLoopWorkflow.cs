using BetterStarRail.Automation.Models;
using BetterStarRail.Automation.Safety;
using BetterStarRail.Core.Vision;
using BetterStarRail.Core.Windows;

namespace BetterStarRail.Automation.Workflows;

public sealed record WorkflowObservation(
    TargetWindowSnapshot Window,
    PageDetectionResult Page,
    SafetyStopReason FailureReason = SafetyStopReason.None);

public interface ITestWorkflowEnvironment
{
    TargetWindowSnapshot Baseline { get; }
    bool EmergencyStopped { get; }
    ValueTask<WorkflowObservation> ObserveAsync(CancellationToken cancellationToken);
    ValueTask<SafetyDecision> DispatchAsync(TestAction action, CancellationToken cancellationToken);
}

public sealed record ClosedLoopResult(bool Succeeded, SafetyStopReason StopReason, string Message, int VerificationAttempts)
{
    public static ClosedLoopResult Success(int attempts) => new(true, SafetyStopReason.None, "两次页面跳转均已验证。", attempts);
    public static ClosedLoopResult Failure(SafetyStopReason reason, string message, int attempts = 0) => new(false, reason, message, attempts);
}

public sealed class TestClosedLoopWorkflow(
    ITestWorkflowEnvironment environment,
    SafetyGuard safetyGuard,
    int maxPostActionChecks,
    TimeSpan? postActionTimeout = null,
    TimeSpan? pollInterval = null)
{
    private static readonly (TestPageId Before, TestAction Action, TestPageId After, int MaxRetries)[] Steps =
    [
        (TestPageId.Welcome, TestAction.Advance, TestPageId.Confirm, 0),
        (TestPageId.Confirm, TestAction.Confirm, TestPageId.Complete, 0),
    ];

    public async ValueTask<ClosedLoopResult> RunAsync(CancellationToken cancellationToken)
    {
        if (maxPostActionChecks <= 0) throw new InvalidOperationException("动作后验证次数必须大于零。");
        var timeout = postActionTimeout ?? TimeSpan.FromSeconds(2);
        var interval = pollInterval ?? TimeSpan.FromMilliseconds(10);
        if (timeout <= TimeSpan.Zero) throw new InvalidOperationException("动作后验证超时必须大于零。");
        if (interval < TimeSpan.Zero) throw new InvalidOperationException("轮询间隔不能为负数。");
        cancellationToken.ThrowIfCancellationRequested();
        var totalChecks = 0;
        foreach (var step in Steps)
        {
            var before = await environment.ObserveAsync(cancellationToken).ConfigureAwait(false);
            if (before.FailureReason != SafetyStopReason.None)
                return ClosedLoopResult.Failure(before.FailureReason, before.Page.Diagnostic, totalChecks);
            if (before.Page.PageId != step.Before && before.Page.PageId != TestPageId.UnknownState)
                return ClosedLoopResult.Failure(SafetyStopReason.ActionNotAllowed, $"前置页面应为 {step.Before}，实际为 {before.Page.PageId}。", totalChecks);
            var decision = safetyGuard.Evaluate(new SafetyEvaluationRequest(
                environment.Baseline, before.Window, before.Page, step.Action, environment.EmergencyStopped));
            if (!decision.Allowed) return ClosedLoopResult.Failure(decision.StopReason, decision.Message, totalChecks);

            var dispatchDecision = await environment.DispatchAsync(step.Action, cancellationToken).ConfigureAwait(false);
            if (!dispatchDecision.Allowed)
                return ClosedLoopResult.Failure(dispatchDecision.StopReason, dispatchDecision.Message, totalChecks);
            var reached = false;
            var verificationStarted = TimeProvider.System.GetTimestamp();
            for (var attempt = 0; attempt < maxPostActionChecks; attempt++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                totalChecks++;
                var after = await environment.ObserveAsync(cancellationToken).ConfigureAwait(false);
                if (after.FailureReason != SafetyStopReason.None)
                    return ClosedLoopResult.Failure(after.FailureReason, after.Page.Diagnostic, totalChecks);
                var observationDecision = safetyGuard.EvaluateState(
                    environment.Baseline, after.Window, after.Page, environment.EmergencyStopped);
                if (!observationDecision.Allowed)
                    return ClosedLoopResult.Failure(observationDecision.StopReason, observationDecision.Message, totalChecks);
                if (after.Page.PageId == step.After)
                {
                    reached = true;
                    break;
                }

                if (TimeProvider.System.GetElapsedTime(verificationStarted) >= timeout) break;
                if (attempt + 1 < maxPostActionChecks && interval > TimeSpan.Zero)
                    await Task.Delay(interval, cancellationToken).ConfigureAwait(false);
            }

            if (!reached)
                return ClosedLoopResult.Failure(
                    SafetyStopReason.PostConditionTimeout,
                    $"动作后未在 {timeout.TotalMilliseconds:F0} ms、{maxPostActionChecks} 次检查内到达 {step.After}；最大动作重试 {step.MaxRetries}。",
                    totalChecks);
        }

        return ClosedLoopResult.Success(totalChecks);
    }
}
