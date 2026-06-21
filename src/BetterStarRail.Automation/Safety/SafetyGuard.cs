using BetterStarRail.Automation.Models;
using BetterStarRail.Core.Vision;
using BetterStarRail.Core.Windows;

namespace BetterStarRail.Automation.Safety;

public enum SafetyStopReason
{
    None,
    InvalidTargetWindow,
    TargetWindowChanged,
    WindowNotForeground,
    WindowMinimized,
    ClientAreaChanged,
    UserInputDetected,
    UnknownPage,
    LowConfidence,
    ActionNotAllowed,
    EmergencyStop,
    PostConditionTimeout,
    CaptureFailed,
    InputDispatchFailed,
}

public sealed record SafetyDecision(bool Allowed, SafetyStopReason StopReason, string Message)
{
    public static SafetyDecision Allow() => new(true, SafetyStopReason.None, "安全校验通过");
    public static SafetyDecision Reject(SafetyStopReason reason, string message) => new(false, reason, message);
}

public sealed record SafetyEvaluationRequest(
    TargetWindowSnapshot Baseline,
    TargetWindowSnapshot Current,
    PageDetectionResult Detection,
    TestAction Action,
    bool EmergencyStopped);

public sealed class ActionWhitelist
{
    public bool IsAllowed(TestPageId page, TestAction action) =>
        (page, action) is (TestPageId.Welcome, TestAction.Advance) or (TestPageId.Confirm, TestAction.Confirm);
}

public sealed class SafetyGuard(ActionWhitelist whitelist, double minimumConfidence = 0.95)
{
    public SafetyDecision Evaluate(SafetyEvaluationRequest request)
    {
        var stateDecision = EvaluateState(request.Baseline, request.Current, request.Detection, request.EmergencyStopped);
        if (!stateDecision.Allowed) return stateDecision;
        return whitelist.IsAllowed(request.Detection.PageId, request.Action)
            ? SafetyDecision.Allow()
            : SafetyDecision.Reject(SafetyStopReason.ActionNotAllowed, "动作不在当前测试页面白名单内。");
    }

    public SafetyDecision EvaluateState(
        TargetWindowSnapshot baseline,
        TargetWindowSnapshot current,
        PageDetectionResult detection,
        bool emergencyStopped)
    {
        if (emergencyStopped) return SafetyDecision.Reject(SafetyStopReason.EmergencyStop, "紧急停止已触发。");
        if (!baseline.Identity.IsValid || !current.Identity.IsValid) return SafetyDecision.Reject(SafetyStopReason.InvalidTargetWindow, "目标窗口身份无效。");
        var changes = baseline.CompareSafetyCriticalState(current);
        if (changes.Overlaps([WindowChange.Handle, WindowChange.Title, WindowChange.ClassName, WindowChange.Process]))
            return SafetyDecision.Reject(SafetyStopReason.TargetWindowChanged, "目标窗口身份已变化。");
        if (current.IsMinimized) return SafetyDecision.Reject(SafetyStopReason.WindowMinimized, "目标窗口已最小化。");
        if (!current.IsForeground) return SafetyDecision.Reject(SafetyStopReason.WindowNotForeground, "目标窗口不在前台。");
        if (changes.Overlaps([WindowChange.Position, WindowChange.Size, WindowChange.Dpi]))
            return SafetyDecision.Reject(SafetyStopReason.ClientAreaChanged, "客户区位置、尺寸或 DPI 已变化。");
        if (baseline.UserInputGeneration != current.UserInputGeneration)
            return SafetyDecision.Reject(SafetyStopReason.UserInputDetected, "检测到用户输入。");
        if (detection.PageId == TestPageId.UnknownState)
            return SafetyDecision.Reject(SafetyStopReason.UnknownPage, "页面处于 UnknownState。");
        if (detection.Confidence < minimumConfidence)
            return SafetyDecision.Reject(SafetyStopReason.LowConfidence, "页面识别置信度不足。");
        return SafetyDecision.Allow();
    }
}
