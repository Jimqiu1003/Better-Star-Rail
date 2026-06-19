namespace BetterStarRail.Core.Models;

/// <summary>
/// Represents a deterministic operation result without throwing for expected failures.
/// </summary>
public sealed record OperationResult(bool Succeeded, string Message)
{
    public static OperationResult Success(string message = "Success") => new(true, message);

    public static OperationResult Failure(string message) => new(false, message);
}
