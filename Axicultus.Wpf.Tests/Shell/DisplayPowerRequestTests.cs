using System.Collections.Generic;
using Xunit;

namespace Axicultus.Wpf.Tests.Shell;

public class DisplayPowerRequestTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void PendingReleaseUsesTheLatestRequestAfterWindowReplacement(bool resumed)
    {
        var results = new Queue<uint>([1u, 0u, 0u, 1u]);
        var requests = new List<uint>();
        var retry = new Queue<System.Action>();
        var power = new DisplayPowerRequest(flags => { requests.Add(flags); return results.Dequeue(); }, retry.Enqueue);
        Assert.True(power.TrySet(true));
        Assert.False(power.TrySet(false));  // Old window closes.
        Assert.False(power.TrySet(false));  // A new window binds before Loaded.
        Assert.Single(retry);
        if (resumed) Assert.True(power.TrySet(true));
        retry.Dequeue()();
        Assert.Empty(retry);
        Assert.Equal(resumed ? 3 : 4, requests.Count);
        Assert.True(power.TrySet(resumed));
    }

    [Fact]
    public void FailedEnableAndReleaseAreRetriedButSuccessfulRequestsAreNotRepeated()
    {
        var results = new Queue<uint>([0u, 1u, 0u, 1u]);
        var requests = new List<uint>();
        var power = new DisplayPowerRequest(flags => { requests.Add(flags); return results.Dequeue(); });
        Assert.False(power.TrySet(true));
        Assert.True(power.TrySet(true));
        Assert.True(power.TrySet(true));
        Assert.False(power.TrySet(false));
        Assert.True(power.TrySet(false));
        Assert.True(power.TrySet(false));
        Assert.Equal(new uint[] { 0x80000002, 0x80000002, 0x80000000, 0x80000000 }, requests);
    }
}
