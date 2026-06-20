using BetterStarRail.Core.Windows;
using BetterStarRail.Infrastructure.Windows.Input;

namespace BetterStarRail.Core.Tests;

public sealed class WindowsInputTests
{
    [Fact]
    public async Task Dispatcher_rejects_zero_handle_without_sending_input()
    {
        var identity = new TargetWindowIdentity((nint)42, WindowsMessageInputDispatcher.TestWindowTitle, "TestClass", 7);
        var dispatcher = new WindowsMessageInputDispatcher(identity);

        var click = await dispatcher.ClickAsync(0, new ClientPoint(1, 1), CancellationToken.None);
        var key = await dispatcher.PressKeyAsync(0, BetterStarRail.Automation.Models.AllowedKey.Enter, CancellationToken.None);

        Assert.False(click.Succeeded);
        Assert.False(key.Succeeded);
        Assert.Contains("句柄", click.Message);
    }

    [Fact]
    public void Dispatcher_rejects_binding_to_any_other_window_title()
    {
        var identity = new TargetWindowIdentity((nint)42, "记事本", "Notepad", 7);

        Assert.Throws<ArgumentException>(() => new WindowsMessageInputDispatcher(identity));
    }
}
