using System;
using System.Collections.Concurrent;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Axicultus.Wpf.Interop;
using Axicultus.Wpf.Rendering;
using Xunit;
using PptCall = Axicultus.Wpf.Interop.OfficePptSession.PptCall;

namespace Axicultus.Wpf.Tests.Rendering;

public class PowerPointSessionTests
{
    private const int RpcServerUnavailable = unchecked((int)0x800706BA);
    private const int RpcCallRejected = unchecked((int)0x80010001);
    private const int RpcRetryLater = unchecked((int)0x8001010A);

    // Stands for the part of a blocked call the message filter cannot cancel (the release of a proxy): a managed
    // wait on the STA worker would be cancelled at the limit like a pending COM call.
    private static void Block(ManualResetEventSlim release) => SpinWait.SpinUntil(() => release.IsSet);

    private static async Task UntilAsync(Func<bool> ready)
    {
        for (int i = 0; i < 200 && !ready(); i++) await Task.Delay(25);
        Assert.True(ready());
    }

    // A call's limit must measure PowerPoint, not the time the call spent queued behind a healthy, slower call
    // (a step behind a live reload, a start behind an export). The predecessor here takes 1.5 s; the start behind
    // it has a 100 ms limit and a PowerPoint that answers at once.
    [Fact]
    public async Task CallQueuedBehindAHealthySlowCallGetsPowerPointsOwnAnswer()
    {
        int creations = 0;
        var session = new OfficePptSession(() =>
        {
            if (Interlocked.Increment(ref creations) == 1) Thread.Sleep(1500);
            throw new InvalidOperationException("answered");
        }, limitMs: 100);
        try
        {
            var slow = session.ExportSlideAsync("deck.pptx", 1, 4, 3);
            var queued = session.StartSlideShowAsync("deck.pptx", 1, "");

            await Assert.ThrowsAsync<InvalidOperationException>(() => slow.WaitAsync(TimeSpan.FromSeconds(10)));
            // PowerPoint answered the queued call at once when its turn came: that answer is the result.
            var error = await Assert.ThrowsAnyAsync<Exception>(() => queued.WaitAsync(TimeSpan.FromSeconds(10)));
            Assert.IsType<InvalidOperationException>(error);
            Assert.Equal(2, Volatile.Read(ref creations));
        }
        finally { session.Dispose(); }
    }

    [Fact]
    public async Task CallThatPowerPointDoesNotAnswerReleasesItsCallerAtTheLimitAndTheSessionAnswersAgain()
    {
        using var release = new ManualResetEventSlim();
        int creations = 0;
        var session = new OfficePptSession(() =>
        {
            Interlocked.Increment(ref creations);
            Block(release);
            throw new COMException("The RPC server is unavailable.", RpcServerUnavailable);
        }, limitMs: 100);
        try
        {
            var blocked = session.StartSlideShowAsync("deck.pptx", 1, "");
            var error = await Assert.ThrowsAsync<TimeoutException>(() => blocked.WaitAsync(TimeSpan.FromSeconds(10)));
            Assert.Contains("did not answer", error.Message);

            release.Set();
            await Assert.ThrowsAsync<COMException>(() => session.StartSlideShowAsync("deck.pptx", 1, "").WaitAsync(TimeSpan.FromSeconds(10)));
            Assert.Equal(2, Volatile.Read(ref creations));
        }
        finally
        {
            release.Set();
            session.Dispose();
        }
    }

    // A relative move that waited too long is dropped for good; what sets a state runs when its turn comes.
    [Fact]
    public async Task BehindABlockedCallAStepIsDroppedForGoodAndACloseStillRunsOnceTheBlockClears()
    {
        using var release = new ManualResetEventSlim();
        var started = new ConcurrentQueue<PptCall>();
        var session = new OfficePptSession(() =>
        {
            Block(release);
            throw new InvalidOperationException("answered");
        }, queueMs: 100) { CallStarted = started.Enqueue };
        try
        {
            var blocked = session.StartSlideShowAsync("deck.pptx", 1, "");
            var step = session.AdvanceShowAsync("deck.pptx");
            var close = session.CloseAsync();

            var dropped = await Assert.ThrowsAsync<TimeoutException>(() => step.WaitAsync(TimeSpan.FromSeconds(10)));
            Assert.Contains("busy", dropped.Message);
            var waiting = await Assert.ThrowsAsync<TimeoutException>(() => close.WaitAsync(TimeSpan.FromSeconds(10)));
            Assert.Contains("busy", waiting.Message);
            Assert.Equal(new[] { PptCall.Start }, started.ToArray());

            release.Set();
            await Assert.ThrowsAsync<InvalidOperationException>(() => blocked.WaitAsync(TimeSpan.FromSeconds(10)));
            await UntilAsync(() => started.Count == 2);
            // One more call proves the queue was worked off in order: the step never started.
            Assert.Null(await session.ReadShowPositionAsync("deck.pptx").WaitAsync(TimeSpan.FromSeconds(10)));
            Assert.Equal(new[] { PptCall.Start, PptCall.Close, PptCall.Read }, started.ToArray());
        }
        finally
        {
            release.Set();
            session.Dispose();
        }
    }

    [Fact]
    public async Task EveryOperationRunsUnderItsOwnCallKind()
    {
        var started = new ConcurrentQueue<PptCall>();
        using var session = new OfficePptSession(() => throw new InvalidOperationException("no PowerPoint")) { CallStarted = started.Enqueue };
        var limit = TimeSpan.FromSeconds(10);

        Assert.Null(await session.AdvanceShowAsync("deck.pptx").WaitAsync(limit));
        Assert.Null(await session.RetreatShowAsync("deck.pptx").WaitAsync(limit));
        Assert.Null(await session.ReadShowPositionAsync("deck.pptx").WaitAsync(limit));
        await session.SetBlackoutAsync("deck.pptx", true).WaitAsync(limit);
        Assert.Null(await session.PollOutputAsync(new OfficePptSession.OutputShow("deck.pptx", 0, null, false), () => true).WaitAsync(limit));
        await Assert.ThrowsAsync<InvalidOperationException>(() => session.TriggerSlideShowNextAsync("deck.pptx", 1).WaitAsync(limit));
        await Assert.ThrowsAsync<InvalidOperationException>(() => session.StartSlideShowAsync("deck.pptx", 1, "").WaitAsync(limit));
        await session.CloseAsync().WaitAsync(limit);

        Assert.Equal(new[] { PptCall.Step, PptCall.Step, PptCall.Read, PptCall.Blackout, PptCall.Poll, PptCall.Trigger, PptCall.Start, PptCall.Close },
            started.ToArray());
    }

    [Theory]
    [InlineData((int)PptCall.Step, 5000, 2000, false)]
    [InlineData((int)PptCall.Read, 5000, 2000, false)]
    [InlineData((int)PptCall.Poll, 30000, 2000, false)]
    [InlineData((int)PptCall.Trigger, 30000, 2000, false)]
    [InlineData((int)PptCall.Blackout, 5000, 2000, true)]
    [InlineData((int)PptCall.Start, 60000, 30000, true)]
    [InlineData((int)PptCall.Close, 10000, 10000, true)]
    public void EachCallKindHasItsOwnLimitQueueWaitAndLatePolicy(int call, int limitMs, int queueMs, bool runLate) =>
        Assert.Equal(new OfficePptSession.CallPolicy(limitMs, queueMs, runLate), OfficePptSession.PolicyFor((PptCall)call));

    [Fact]
    public async Task DisposeDoesNotWaitForAPowerPointThatDoesNotAnswer()
    {
        using var release = new ManualResetEventSlim();
        var session = new OfficePptSession(() =>
        {
            Block(release);
            throw new InvalidOperationException();
        }, limitMs: 100, queueMs: 100);
        var blocked = session.StartSlideShowAsync("deck.pptx", 1, "");
        try
        {
            var dispose = Task.Run(session.Dispose);
            Assert.Same(dispose, await Task.WhenAny(dispose, Task.Delay(TimeSpan.FromSeconds(4))));
        }
        finally { release.Set(); }
        await Assert.ThrowsAnyAsync<Exception>(() => blocked);
    }

    [Theory]
    [InlineData(RpcServerUnavailable, true)]
    [InlineData(unchecked((int)0x800706BE), true)]
    [InlineData(unchecked((int)0x80010108), true)]
    [InlineData(RpcCallRejected, false)] // busy: PowerPoint is there
    public void OnlyALostProcessCountsAsPowerPointGone(int hresult, bool gone) =>
        Assert.Equal(gone, OfficePptSession.IsPowerPointGone(new InvalidOperationException("wrapped", new COMException("x", hresult))));

    // A modal dialog open in PowerPoint: a close that is refused has not closed anything.
    [Theory]
    [InlineData(RpcCallRejected, true)]
    [InlineData(RpcRetryLater, true)]
    [InlineData(RpcServerUnavailable, false)]
    [InlineData(unchecked((int)0x80004005), false)]
    public void ARefusedCallCountsAsPowerPointBusy(int hresult, bool busy) =>
        Assert.Equal(busy, OfficePptSession.IsPowerPointBusy(new InvalidOperationException("wrapped", new COMException("x", hresult))));

    [Theory]
    [InlineData(0, false, -1, true)]  // hidden PowerPoint this app started
    [InlineData(0, true, 1, true)]    // shown by a slide show, started after this app
    [InlineData(0, true, -1, false)]  // the operator's own: visible and already running
    [InlineData(1, false, 1, false)]  // a file is open in Protected View
    public void PowerPointIsQuitOnlyWhenItIsNotTheOperatorsOwn(int protectedViews, bool visible, int startedAfterMinutes, bool mayQuit)
    {
        var self = new DateTime(2026, 1, 1, 9, 0, 0);
        Assert.Equal(mayQuit, OfficePptSession.MayQuit(() => protectedViews, () => visible ? self.AddMinutes(startedAfterMinutes) : null, self));
    }

    [Fact]
    public void PowerPointIsNotQuitWhenProtectedViewCannotBeReadAndTheOtherChecksDecideWhenItsAgeCannot()
    {
        var self = new DateTime(2026, 1, 1, 9, 0, 0);
        Assert.False(OfficePptSession.MayQuit(() => throw new COMException("busy", RpcCallRejected), () => null, self));
        Assert.True(OfficePptSession.MayQuit(() => 0, () => throw new COMException("no HWND"), self));
    }

    [Fact]
    public async Task RetreatWithoutAnOutputShowDoesNotStartPowerPoint()
    {
        using var show = new OfficePowerPointSlideShowControl(() => throw new InvalidOperationException("PowerPoint must not be started"));
        Assert.Null(await show.RetreatOutputAsync("deck.pptx"));
        Assert.Null(await show.AdvanceOutputAsync("deck.pptx"));
    }

    // The live deck on a share that stopped answering: the file check must not hold up its caller or stack up.
    [Fact]
    public async Task SourceFileIsReadOffTheCallerOneReadAtATime()
    {
        string path = Path.Combine(Path.GetTempPath(), $"Axicultus_{Guid.NewGuid():N}.pptx");
        File.WriteAllText(path, "deck");
        using var release = new ManualResetEventSlim();
        int reads = 0;
        try
        {
            var watch = new PowerPointSourceWatch(file =>
            {
                Interlocked.Increment(ref reads);
                release.Wait();
                return OfficeLib.SlideImageManifest.ReadSource(file);
            }, intervalMs: 0);

            Assert.Null(watch.Latest(path));
            await UntilAsync(() => Volatile.Read(ref reads) == 1);
            Assert.Null(watch.Latest(path));
            Assert.Equal(1, Volatile.Read(ref reads));

            release.Set();
            await UntilAsync(() => watch.Latest(path) != null);
            Assert.True(OfficeLib.SlideImageManifest.ReadSource(path).Matches(watch.Latest(path)));
        }
        finally
        {
            release.Set();
            File.Delete(path);
        }
    }
}
