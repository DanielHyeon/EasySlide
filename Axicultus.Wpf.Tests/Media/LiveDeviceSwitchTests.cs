using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Axicultus.Wpf.Media;
using Xunit;

namespace Axicultus.Wpf.Tests.Media;

[Collection("Device diagnostics window")]
public sealed class LiveDeviceSwitchTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task LiveSnapshotChangesOnlyAfterDeviceIsReady(bool success)
    {
        var backend = new TestBackend();
        var bridge = new AttachableMediaPlaybackBackend();
        bridge.Attach(backend);
        using var service = new MediaPlaybackService(bridge);
        service.LoadLiveCamera(TimeSpan.Zero);
        service.Play();
        var previous = service.Current;
        var chosen = new InputDevice("camera-path", "강대상", 7);
        var switching = service.SwitchLiveCameraAsync(chosen, "projector", CancellationToken.None);
        Assert.Same(previous, service.Current);
        if (success)
        {
            backend.Ready.SetResult();
            await switching;
            Assert.Equal(chosen.Id, service.Current.CameraDevicePath);
            Assert.Equal(MediaPlaybackState.Playing, service.Current.State);
            Assert.Equal("<<Capture>>7", service.Current.Source);
        }
        else
        {
            backend.Ready.SetException(new IOException("영상 없음"));
            await Assert.ThrowsAsync<IOException>(() => switching);
            Assert.Same(previous, service.Current);
        }
        var replacement = new TestBackend();
        bridge.Detach();
        bridge.Attach(replacement);
        Assert.Equal(service.Current.Source, replacement.Loaded?.Source);
        Assert.Equal(service.Current.CameraDevicePath, replacement.Loaded?.CameraDevicePath);
        Assert.True(replacement.Playing);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task StopOrUnloadCancelsFirstCameraBeforeAnySignal(bool stop)
    {
        var backend = new TestBackend();
        using var service = new MediaPlaybackService(backend);
        var switching = service.SwitchLiveCameraAsync(new("camera", "camera", 1), "display", default);
        if (stop) service.Stop(); else service.Unload();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => switching);
        backend.Ready.TrySetResult();
        Assert.Equal(MediaPlaybackState.Empty, service.Current.State);
    }

    [Fact]
    public Task AbortedCameraWarmupPreservesOutputImageAndReleasesOwnership() => StaHelper.RunOnStaAsync(async () =>
    {
        var oldImage = new System.Windows.Media.DrawingImage();
        var audience = new System.Windows.Controls.Image { Source = oldImage };
        var ownership = new CameraOwnership();
        ILiveCameraSwitchBackend backend = new WpfMediaElementPlaybackBackend(new System.Windows.Controls.MediaElement(), audience, ownership);
        using var cancel = new CancellationTokenSource();
        var device = new InputDevice("missing-camera-" + Guid.NewGuid(), "미연결", 1);
        var switchTask = backend.SwitchCameraAsync(device, MediaPlaybackSnapshot.Empty, cancel.Token);
        Assert.True(ownership.HasLocalCapture);
        Assert.Same(oldImage, audience.Source);
        cancel.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => switchTask);
        Assert.Same(oldImage, audience.Source);
        Assert.False(ownership.HasLocalCapture);
    });

    private sealed class TestBackend : IMediaPlaybackBackend, ILiveCameraSwitchBackend
    {
        internal TaskCompletionSource Ready { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        internal MediaPlaybackSnapshot? Loaded;
        internal bool Playing;
        public Task SwitchCameraAsync(InputDevice device, MediaPlaybackSnapshot next, CancellationToken ct) => Ready.Task.WaitAsync(ct);
        public void Load(MediaPlaybackSnapshot snapshot) => Loaded = snapshot;
        public void Play() => Playing = true;
        public void Pause() { }
        public void Stop() { }
        public void Unload() { }
        public void Seek(TimeSpan position) { }
        public void ApplySettings(MediaPlaybackSnapshot snapshot) { }
    }
}
