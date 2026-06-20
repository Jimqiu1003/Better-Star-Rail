namespace BetterStarRail.Automation.Safety;

public sealed class EmergencyStopController
{
    private int stopped;

    public bool IsStopped => Volatile.Read(ref stopped) != 0;

    public void Stop() => Interlocked.Exchange(ref stopped, 1);

    public void Reset() => Interlocked.Exchange(ref stopped, 0);
}
