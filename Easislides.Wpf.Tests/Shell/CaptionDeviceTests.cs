using System;
using System.IO;
using System.Diagnostics;
using System.Threading;
using Easislides.Wpf.Rendering;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Easislides.Wpf.Captions;
using Easislides.Wpf.Media;
using Xunit;

namespace Easislides.Wpf.Tests.Shell;

public sealed class CaptionDeviceTests
{
    [Fact]
    public void SlowPreviewDisplaysNewestCompleteFrameAndCameraHasASingleOwner() => StaHelper.RunOnSta(() =>
    {
        var owners = new CameraOwnership();
        var first = new object();
        var second = new object();
        Assert.True(owners.TryAcquire(first));
        Assert.False(owners.TryAcquire(second));
        owners.Release(second);
        Assert.False(owners.TryAcquire(second));
        owners.ReleaseLocalCamera += () => owners.Release(first);
        owners.ObsOwnsCamera = true;
        Assert.False(owners.TryAcquire(second));
        owners.ObsOwnsCamera = false;
        Assert.True(owners.TryAcquire(second));

        var frames = new DirectShowVideo.FrameBuffer(1,4,1,false);
        var bitmap = new WriteableBitmap(1,1,96,96,PixelFormats.Bgr24,null);
        var pointer = Marshal.AllocHGlobal(4);
        try
        {
            Marshal.WriteInt32(pointer,1);
            frames.BufferCB(0,pointer,4);
            Marshal.WriteInt32(pointer,2);
            frames.BufferCB(0,pointer,4);
            Marshal.WriteInt32(pointer,3);
            frames.BufferCB(0,pointer,2); // truncated capture frame
            frames.Draw(bitmap);
            byte[] pixels = new byte[4];
            bitmap.CopyPixels(pixels,4,0);
            Assert.Equal(2,pixels[0]);
        }
        finally { Marshal.FreeHGlobal(pointer); }
    });

    [Fact]
    public async Task LegacyDeckWithNoSlidesIsReportedAsAnInputError()
    {
        using var worker = new PowerPointWorker(() =>
        {
            var info = new ProcessStartInfo(Path.Combine(Environment.SystemDirectory, @"WindowsPowerShell\v1.0\powershell.exe"))
            {
                UseShellExecute = false, CreateNoWindow = true, RedirectStandardInput = true, RedirectStandardOutput = true
            };
            info.ArgumentList.Add("-NoProfile");
            info.ArgumentList.Add("-NonInteractive");
            info.ArgumentList.Add("-Command");
            info.ArgumentList.Add("""$null = [Console]::ReadLine(); [Console]::WriteLine('{"Deck":{"Path":"empty.ppt","Slides":[]}}')""");
            return Process.Start(info)!;
        });
        await Assert.ThrowsAsync<InvalidDataException>(() => worker.ReadLegacyDeckAsync("empty.ppt", CancellationToken.None));
    }

    [Fact]
    public async Task CancelledPowerPointConversionStopsUnresponsivePrivateWorker()
    {
        var started = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
        using var worker = new PowerPointWorker(() =>
        {
            var powershell = Path.Combine(Environment.SystemDirectory, @"WindowsPowerShell\v1.0\powershell.exe");
            var info = new ProcessStartInfo(powershell) { UseShellExecute = false, CreateNoWindow = true,
                RedirectStandardInput = true, RedirectStandardOutput = true };
            info.ArgumentList.Add("-NoProfile");
            info.ArgumentList.Add("-NonInteractive");
            info.ArgumentList.Add("-Command");
            info.ArgumentList.Add("Start-Sleep -Seconds 60");
            var child = Process.Start(info)!;
            started.SetResult(child.Id);
            return child;
        });
        using var stop = new CancellationTokenSource();
        var render = worker.ReadLegacyDeckAsync("test.ppt",stop.Token);
        var pid = await started.Task.WaitAsync(TimeSpan.FromSeconds(5));
        using var running = Process.GetProcessById(pid);
        stop.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => render.WaitAsync(TimeSpan.FromSeconds(5)));
        Assert.True(running.WaitForExit(2000));
    }

    [Fact]
    public Task SavedObsProfileKeepsSecretsPrivateAndRestoresSceneShortcuts() => StaHelper.RunOnStaAsync(async () =>
    {
        var dir = Directory.CreateTempSubdirectory();
        using var listener = new TcpListener(IPAddress.Loopback,0);
        listener.Start();
        int port = ((IPEndPoint)listener.LocalEndpoint).Port;
        try
        {
            using (var setup = new ObsOperator(dir.FullName, Dispatcher.CurrentDispatcher))
                await setup.ConfigureAsync(port,"operator-obs-password",true,["설교","찬양","",""]);
            var bytes = await File.ReadAllTextAsync(Path.Combine(dir.FullName,"obs.json"));
            Assert.DoesNotContain("operator-obs-password",bytes);
            using var reopened = new ObsOperator(dir.FullName, Dispatcher.CurrentDispatcher);
            await reopened.StartAsync();
            Assert.True(reopened.AutoConnect);
            Assert.Equal(port,reopened.Port);
            Assert.Equal("찬양",reopened.Shortcuts[1]);
            Assert.NotNull(reopened.Connection);
            await reopened.ConfigureAsync(port,"",false,["","","",""]);
            using var manual = new ObsOperator(dir.FullName, Dispatcher.CurrentDispatcher);
            await manual.StartAsync();
            Assert.False(manual.AutoConnect);
            Assert.Null(manual.Connection);
        }
        finally { dir.Delete(true); }
    });
}
