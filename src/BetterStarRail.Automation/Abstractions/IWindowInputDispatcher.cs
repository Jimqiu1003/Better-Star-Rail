using BetterStarRail.Automation.Models;
using BetterStarRail.Core.Models;
using BetterStarRail.Core.Windows;

namespace BetterStarRail.Automation.Abstractions;

public interface IWindowInputDispatcher
{
    ValueTask<OperationResult> ClickAsync(nint windowHandle, ClientPoint point, CancellationToken cancellationToken);
    ValueTask<OperationResult> PressKeyAsync(nint windowHandle, AllowedKey key, CancellationToken cancellationToken);
    ValueTask ReleaseAllAsync(nint windowHandle, CancellationToken cancellationToken);
}
