using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using Axicultus.Wpf.Localization;

namespace Axicultus.Wpf.Media;

internal interface ILiveCameraSwitchBackend
{
    Task SwitchCameraAsync(InputDevice device, MediaPlaybackSnapshot next, CancellationToken ct);
}

public sealed partial class WpfMediaElementPlaybackBackend
{
    private CancellationTokenSource? _cameraSwitch;
    internal string? CurrentCameraPath => _directShow?.CameraPath;
    private void CancelSwitch() => _cameraSwitch?.Cancel();

    private void ObserveCamera(DirectShowVideo graph)
    {
        if (_ownership == null) return;
        _ownership.ReleaseLocalCamera += Unload;
        graph.SignalChanged = text => _ownership.Status = text;
        graph.PreviewChanged = (image, flip) =>
        {
            _ownership.ScaleY = flip ? -1 : 1;
            _ownership.Preview = image;
        };
    }

    async Task ILiveCameraSwitchBackend.SwitchCameraAsync(InputDevice device, MediaPlaybackSnapshot next, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        if (_videoSurface == null || _ownership?.ObsOwnsCamera == true)
            throw new MediaPlaybackException(UiText.Get("WpfMediaElementPlaybackBackend", "LocalCameraUnavailable"));
        if (_cameraSwitch != null) throw new MediaPlaybackException(UiText.Get("WpfMediaElementPlaybackBackend", "CameraSwitching"));


        using var pending = CancellationTokenSource.CreateLinkedTokenSource(ct);
        if (_ownership != null && !_ownership.IsOwner(this) && !_ownership.TryAcquire(this))
            throw new MediaPlaybackException(UiText.Get("WpfMediaElementPlaybackBackend", "ClosePreviewFirst"));
        _cameraSwitch = pending;
        _ownership?.ReleaseLocalCamera += CancelSwitch;
        DirectShowVideo? candidate = null;
        try
        {
            if (_directShow is { CameraPath: { } path } existing && string.Equals(path, device.Id, StringComparison.OrdinalIgnoreCase))
            {
                await WaitForVideoAsync(existing, pending.Token);
                return;
            }

            candidate = new DirectShowVideo(new Image(), device.CameraNumber, device.Id, embeddedAudio: true);
            await WaitForVideoAsync(candidate, pending.Token);
            pending.Token.ThrowIfCancellationRequested();
            ClearMedia(releaseCamera: false);
            ObserveCamera(candidate);
            candidate.PresentOn(_videoSurface);
            _ownership?.Status = UiText.Format("WpfMediaElementPlaybackBackend", "Receiving", device.Name);
            _directShow = candidate;
            candidate = null;
            ApplySettings(next);
        }
        catch (TimeoutException) { throw new MediaPlaybackException(UiText.Get("WpfMediaElementPlaybackBackend", "NoNewCameraSignal")); }
        finally
        {
            candidate?.Dispose();
            if (_ownership != null)
            {
                _ownership.ReleaseLocalCamera -= CancelSwitch;
                if (_directShow == null || _directShow.FileName != null)
                    _ownership.Release(this);
            }
            _cameraSwitch = null;
        }
    }

    private static async Task WaitForVideoAsync(DirectShowVideo graph, CancellationToken ct)
    {
        var first = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        void Received() => first.TrySetResult();
        graph.FramePresented += Received;
        try
        {
            graph.Play();
            await first.Task.WaitAsync(TimeSpan.FromSeconds(6), ct);
            ct.ThrowIfCancellationRequested();
        }
        finally { graph.FramePresented -= Received; }
    }
}
