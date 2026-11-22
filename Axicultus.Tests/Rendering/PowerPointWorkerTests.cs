using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Axicultus.Wpf.Captions;
using Axicultus.Wpf.Interop;
using Axicultus.Wpf.Rendering;
using Xunit;

namespace Axicultus.Wpf.Tests.Rendering;

public class PowerPointWorkerTests
{
    private const int RpcServerUnavailable = unchecked((int)0x800706BA);

    // A stand-in child: answers every request line after a delay, echoing the requested slide as the slide count,
    // or with fixedReply when one is given.
    private static Process StartEchoChild(string afterReply = "", string? fixedReply = null)
    {
        var info = new ProcessStartInfo(Path.Combine(Environment.SystemDirectory, @"WindowsPowerShell\v1.0\powershell.exe"))
        {
            UseShellExecute = false, CreateNoWindow = true, RedirectStandardInput = true, RedirectStandardOutput = true
        };
        info.ArgumentList.Add("-NoProfile");
        info.ArgumentList.Add("-NonInteractive");
        info.ArgumentList.Add("-Command");
        info.ArgumentList.Add(fixedReply != null
            ? "while ($null -ne ($line = [Console]::ReadLine())) { [Console]::WriteLine('" + fixedReply + "') }"
            : """while ($null -ne ($line = [Console]::ReadLine())) { $slide = ($line | ConvertFrom-Json).Slide; Start-Sleep -Milliseconds 700; [Console]::WriteLine('{"Image":{"SlideCount":' + $slide + ',"ImageBytes":"AQID","ContentType":"image/png","PixelWidth":4,"PixelHeight":3},"Error":0AFTER}') }"""
                .Replace("AFTER", afterReply));
        return Process.Start(info)!;
    }

    private static PowerPointRenderRequest Request(int slide) =>
        new("deck.pptx", slide, 4, 3, TimeSpan.FromSeconds(30));

    [Fact]
    public async Task CallerCancellationAbandonsTheReplyAndKeepsTheChild()
    {
        int starts = 0;
        var started = new TaskCompletionSource<Process>(TaskCreationOptions.RunContinuationsAsynchronously);
        using var worker = new PowerPointWorker(() =>
        {
            Interlocked.Increment(ref starts);
            var child = StartEchoChild();
            started.TrySetResult(child);
            return child;
        });
        using var stop = new CancellationTokenSource();

        var cancelled = worker.RenderSlideAsync(Request(1), stop.Token);
        var child = await started.Task.WaitAsync(TimeSpan.FromSeconds(10));
        stop.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => cancelled.WaitAsync(TimeSpan.FromSeconds(5)));

        // The export in flight was left to finish: the same child answers the next request with its own reply.
        var next = await worker.RenderSlideAsync(Request(2), CancellationToken.None).WaitAsync(TimeSpan.FromSeconds(30));
        Assert.Equal(2, next.SlideCount);
        Assert.Equal(1, starts);
        Assert.False(child.HasExited);
    }

    [Fact]
    public async Task ExportOverItsTimeLimitStillKillsTheChild()
    {
        var started = new TaskCompletionSource<Process>(TaskCreationOptions.RunContinuationsAsynchronously);
        using var worker = new PowerPointWorker(() =>
        {
            var info = new ProcessStartInfo(Path.Combine(Environment.SystemDirectory, @"WindowsPowerShell\v1.0\powershell.exe"))
            {
                UseShellExecute = false, CreateNoWindow = true, RedirectStandardInput = true, RedirectStandardOutput = true
            };
            info.ArgumentList.Add("-NoProfile");
            info.ArgumentList.Add("-NonInteractive");
            info.ArgumentList.Add("-Command");
            info.ArgumentList.Add("Start-Sleep -Seconds 60");
            var child = Process.Start(info)!;
            started.TrySetResult(child);
            return child;
        });

        var render = worker.RenderSlideAsync(Request(1) with { Timeout = TimeSpan.FromMilliseconds(300) }, CancellationToken.None);
        var child = await started.Task.WaitAsync(TimeSpan.FromSeconds(10));
        using var running = Process.GetProcessById(child.Id);
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => render.WaitAsync(TimeSpan.FromSeconds(10)));
        Assert.True(running.WaitForExit(2000));
    }

    [Fact]
    public async Task ChildThatLostPowerPointIsReplacedBeforeTheNextRequest()
    {
        int starts = 0;
        using var worker = new PowerPointWorker(() => StartEchoChild(Interlocked.Increment(ref starts) == 1 ? ""","Restart":true""" : ""));

        Assert.Equal(1, (await worker.RenderSlideAsync(Request(1), CancellationToken.None).WaitAsync(TimeSpan.FromSeconds(30))).SlideCount);
        Assert.Equal(2, (await worker.RenderSlideAsync(Request(2), CancellationToken.None).WaitAsync(TimeSpan.FromSeconds(30))).SlideCount);
        Assert.Equal(2, starts);
    }

    // The child lost PowerPoint during the export: the file is fine, so the job goes once to the new child
    // instead of being reported as a corrupt file.
    [Fact]
    public async Task ExportThatLostPowerPointIsSentOnceMoreToTheNewChild()
    {
        int starts = 0;
        string lost = $$"""{"Error":{{(int)PowerPointRenderErrorKind.CorruptFile}},"Message":"The RPC server is unavailable.","Restart":true}""";
        using var worker = new PowerPointWorker(() => Interlocked.Increment(ref starts) == 1 ? StartEchoChild(fixedReply: lost) : StartEchoChild());

        var slide = await worker.RenderSlideAsync(Request(3), CancellationToken.None).WaitAsync(TimeSpan.FromSeconds(30));

        Assert.Equal(3, slide.SlideCount);
        Assert.Equal(2, starts);
    }

    [Fact]
    public async Task ExportIsNotResentASecondTimeWhenTheNewChildLosesPowerPointToo()
    {
        int starts = 0;
        string lost = $$"""{"Error":{{(int)PowerPointRenderErrorKind.CorruptFile}},"Message":"The RPC server is unavailable.","Restart":true}""";
        using var worker = new PowerPointWorker(() =>
        {
            Interlocked.Increment(ref starts);
            return StartEchoChild(fixedReply: lost);
        });

        await Assert.ThrowsAsync<PowerPointRenderException>(() => worker.RenderSlideAsync(Request(3), CancellationToken.None).WaitAsync(TimeSpan.FromSeconds(30)));
        Assert.Equal(2, starts);
    }

    [Fact]
    public async Task ChildLoopEndsWhenPowerPointIsGone()
    {
        int exports = 0;
        string job = """{"File":"deck.pptx","Slide":1,"Width":4,"Height":3,"TextOnly":false}""";
        using var input = new StringReader(job + "\n" + job + "\n");
        using var output = new StringWriter { NewLine = "\n" };

        await PowerPointWorker.RunAsync(input, output,
            (_, _, _, _) =>
            {
                exports++;
                return Task.FromException<OfficePptSlideExport>(
                    new InvalidOperationException("wrapped", new COMException("The RPC server is unavailable.", RpcServerUnavailable)));
            },
            _ => Task.FromException<CaptionDeck>(new InvalidOperationException()));

        Assert.Equal(1, exports);
        string reply = Assert.Single(output.ToString().Split('\n', StringSplitOptions.RemoveEmptyEntries));
        Assert.Contains("\"Restart\":true", reply);
    }
}
