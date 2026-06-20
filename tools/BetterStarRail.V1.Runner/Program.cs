using BetterStarRail.Automation.Safety;
using BetterStarRail.Automation.Workflows;
using BetterStarRail.Infrastructure.Windows.Automation;
using BetterStarRail.Infrastructure.Windows.Capture;
using BetterStarRail.Infrastructure.Windows.Diagnostics;
using BetterStarRail.Infrastructure.Windows.Input;
using BetterStarRail.Infrastructure.Windows.Storage;
using BetterStarRail.Infrastructure.Windows.Windowing;
using BetterStarRail.Vision.Services;

const string TestWindowTitle = "Better Star Rail V1 Test Window";
using var cancellation = new CancellationTokenSource();
Console.CancelKeyPress += (_, eventArgs) =>
{
    eventArgs.Cancel = true;
    cancellation.Cancel();
};

var windows = new WindowsTargetWindowService(TestWindowTitle);
var located = windows.Locate();
if (!located.Succeeded || located.Snapshot is null)
{
    Console.Error.WriteLine(located.Error);
    return 2;
}

if (!located.Snapshot.IsForeground)
{
    Console.WriteLine("请在 15 秒内将项目自建测试窗口切到前台；等待只检查条件，可随时按 Ctrl+C 取消。");
    var deadline = DateTimeOffset.UtcNow + TimeSpan.FromSeconds(15);
    while (DateTimeOffset.UtcNow < deadline && !cancellation.IsCancellationRequested)
    {
        await Task.Delay(100);
        var current = windows.ReadSnapshot(located.Snapshot.Identity.Handle);
        if (current.Succeeded && current.Snapshot is { IsForeground: true })
        {
            located = current;
            break;
        }
    }

    if (!located.Snapshot.IsForeground)
    {
        if (cancellation.IsCancellationRequested)
        {
            Console.Error.WriteLine("等待已取消，未发送任何输入。");
            return 3;
        }
        Console.Error.WriteLine("等待目标窗口前台状态超时，未发送任何输入。");
        return 4;
    }
}

var paths = new LocalAppDataPathProvider();
paths.EnsureDirectories();
var emergency = new EmergencyStopController();
var safeInput = new SafeInputController(
    new SafetyGuard(new ActionWhitelist()),
    new WindowsMessageInputDispatcher(located.Snapshot.Identity),
    emergency,
    () => windows.ReadSnapshot(located.Snapshot.Identity.Handle).Snapshot);
var environment = new WindowsTestWorkflowEnvironment(
    located.Snapshot,
    windows,
    new GdiScreenCaptureService(),
    new SolidColorPageDetector(),
    safeInput,
    emergency,
    new LocalDiagnosticWriter(paths));

try
{
    var result = await new TestClosedLoopWorkflow(environment, new SafetyGuard(new ActionWhitelist()), 10)
        .RunAsync(cancellation.Token);
    Console.WriteLine($"结果：{result.Succeeded}；原因：{result.StopReason}；{result.Message}");
    if (result.Succeeded) return 0;
    var diagnostic = await environment.WriteFailureAsync(result, CancellationToken.None);
    Console.Error.WriteLine(diagnostic.Message);
    return 1;
}
catch (OperationCanceledException)
{
    await safeInput.StopAsync(located.Snapshot, CancellationToken.None);
    Console.Error.WriteLine("运行已取消，所有允许按键状态已释放。");
    return 3;
}
