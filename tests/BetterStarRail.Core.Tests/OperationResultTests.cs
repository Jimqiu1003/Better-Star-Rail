using BetterStarRail.Core.Models;

namespace BetterStarRail.Core.Tests;

public sealed class OperationResultTests
{
    [Fact]
    public void Success_sets_success_state()
    {
        var result = OperationResult.Success();
        Assert.True(result.Succeeded);
    }

    [Fact]
    public void Failure_sets_failure_state()
    {
        var result = OperationResult.Failure("No unsafe guesswork");
        Assert.False(result.Succeeded);
    }
}
