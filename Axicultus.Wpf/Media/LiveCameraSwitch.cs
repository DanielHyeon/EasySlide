using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;

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
            throw new MediaPlaybackException("로컬 카메라 송출을 사용할 수 없습니다.");
        if (_cameraSwitch != null) throw new MediaPlaybackException("카메라를 전환하는 중입니다.");


        using var pending = CancellationTokenSource.CreateLinkedTokenSource(ct);
        if (_ownership != null && !_ownership.IsOwner(this) && !_ownership.TryAcquire(this))
            throw new MediaPlaybackException("카메라 미리보기를 종료한 뒤 다시 선택하세요.");
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
            _ownership?.Status = "영상 수신 중 · " + device.Name;
            _directShow = candidate;
            candidate = null;
            ApplySettings(next);
        }
        catch (TimeoutException) { throw new MediaPlaybackException("새 카메라의 영상 신호가 없습니다. 연결과 장치 점유 상태를 확인하세요."); }
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
