namespace BetterStarRail.Core.Windows;

public readonly record struct ClientPoint(int X, int Y);

public readonly record struct NormalizedPoint
{
    public NormalizedPoint(double x, double y)
    {
        if (x is < 0 or > 1) throw new ArgumentOutOfRangeException(nameof(x));
        if (y is < 0 or > 1) throw new ArgumentOutOfRangeException(nameof(y));
        X = x;
        Y = y;
    }

    public double X { get; }
    public double Y { get; }

    public ClientPoint ToClientPixel(ClientArea area) => new(
        Math.Clamp((int)Math.Round(X * area.Width, MidpointRounding.AwayFromZero), 0, area.Width - 1),
        Math.Clamp((int)Math.Round(Y * area.Height, MidpointRounding.AwayFromZero), 0, area.Height - 1));
}

public sealed record ClientArea
{
    public ClientArea(int screenX, int screenY, int width, int height, uint dpi)
    {
        if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width));
        if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height));
        if (dpi == 0) throw new ArgumentOutOfRangeException(nameof(dpi));
        ScreenX = screenX;
        ScreenY = screenY;
        Width = width;
        Height = height;
        Dpi = dpi;
    }

    public int ScreenX { get; }
    public int ScreenY { get; }
    public int Width { get; }
    public int Height { get; }
    public uint Dpi { get; }
}

public sealed record TargetWindowIdentity(nint Handle, string Title, string ClassName, int ProcessId)
{
    public bool IsValid => Handle != 0 && ProcessId > 0 && !string.IsNullOrWhiteSpace(Title) && !string.IsNullOrWhiteSpace(ClassName);
}

public enum WindowChange
{
    Handle,
    Title,
    ClassName,
    Process,
    Position,
    Size,
    Dpi,
}

public sealed record TargetWindowSnapshot(
    TargetWindowIdentity Identity,
    ClientArea ClientArea,
    bool IsForeground,
    bool IsMinimized,
    long UserInputGeneration)
{
    public IReadOnlySet<WindowChange> CompareSafetyCriticalState(TargetWindowSnapshot current)
    {
        var changes = new HashSet<WindowChange>();
        if (Identity.Handle != current.Identity.Handle) changes.Add(WindowChange.Handle);
        if (!string.Equals(Identity.Title, current.Identity.Title, StringComparison.Ordinal)) changes.Add(WindowChange.Title);
        if (!string.Equals(Identity.ClassName, current.Identity.ClassName, StringComparison.Ordinal)) changes.Add(WindowChange.ClassName);
        if (Identity.ProcessId != current.Identity.ProcessId) changes.Add(WindowChange.Process);
        if (ClientArea.ScreenX != current.ClientArea.ScreenX || ClientArea.ScreenY != current.ClientArea.ScreenY) changes.Add(WindowChange.Position);
        if (ClientArea.Width != current.ClientArea.Width || ClientArea.Height != current.ClientArea.Height) changes.Add(WindowChange.Size);
        if (ClientArea.Dpi != current.ClientArea.Dpi) changes.Add(WindowChange.Dpi);
        return changes;
    }
}
