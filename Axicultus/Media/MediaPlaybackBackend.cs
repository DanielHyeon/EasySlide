using System;
using System.IO;
using System.Windows.Controls;
using System.Windows.Media;
using Axicultus.Wpf.Localization;

namespace Axicultus.Wpf.Media;

public sealed class MediaPlaybackException(string message, Exception? innerException = null) : Exception(message, innerException)
{
}

public interface IMediaPlaybackBackend
{
    void Load(MediaPlaybackSnapshot snapshot);
    void Play();
    void Pause();
    void Stop();
    void Seek(TimeSpan position);
    void ApplySettings(MediaPlaybackSnapshot snapshot);

    // 미디어를 화면에서 완전히 내린다(출력 소스 제거). Stop 과 다름:
    // Stop 은 "처음 위치로 정지(다시 Play 가능, 첫 프레임 유지)"지만,
    // Unload 는 소스 자체를 비워 출력 창에서 영상이 사라지고 가사가 다시 보이게 한다.
    void Unload();
}

public sealed class NoOpMediaPlaybackBackend : IMediaPlaybackBackend
{
    public void Load(MediaPlaybackSnapshot snapshot)
    {
    }

    public void Play()
    {
    }

    public void Pause()
    {
    }

    public void Stop()
    {
    }

    public void Seek(TimeSpan position)
    {
    }

    public void ApplySettings(MediaPlaybackSnapshot snapshot)
    {
    }

    public void Unload()
    {
    }
}

public sealed partial class WpfMediaElementPlaybackBackend(MediaElement element, Image? cameraSurface = null, CameraOwnership? ownership = null) : IMediaPlaybackBackend, ILiveCameraSwitchBackend
{
    private readonly MediaElement _element = element ?? throw new ArgumentNullException(nameof(element));

    private readonly Image? _videoSurface = cameraSurface;
    private DirectShowVideo? _directShow;

    private readonly CameraOwnership? _ownership = ownership;

    public void Load(MediaPlaybackSnapshot snapshot)
    {
        Unload();
        if (snapshot.SourceKind == MediaSourceKind.CaptureDevice)
        {
            const string prefix = "<<Capture>>";
            if (_videoSurface == null || !snapshot.Source.StartsWith(prefix, StringComparison.Ordinal)
                || !int.TryParse(snapshot.Source.AsSpan(prefix.Length), out int index))
                throw new MediaPlaybackException(UiText.Get("WpfMediaElementPlaybackBackend", "CameraNotSelectable"));
            if (_ownership?.ObsOwnsCamera == true) throw new MediaPlaybackException(UiText.Get("WpfMediaElementPlaybackBackend", "ObsCameraMode"));
            if (_ownership != null && !_ownership.TryAcquire(this))
                throw new MediaPlaybackException(UiText.Get("WpfMediaElementPlaybackBackend", "CameraInUse"));
            try { _directShow = new DirectShowVideo(_videoSurface, index, snapshot.CameraDevicePath, embeddedAudio: true); }
            catch { _ownership?.Release(this); throw; }
            ObserveCamera(_directShow);
        }
        else _element.Source = CreateSourceUri(snapshot);
        ApplySettings(snapshot);
    }

    internal DirectShowVideo? FileGraph => _directShow?.FileName == null ? null : _directShow;

    internal bool TryFileFallback(MediaPlaybackSnapshot snapshot, TimeSpan position, out string? error)
    {
        error = null;
        if (_videoSurface == null || snapshot.SourceKind != MediaSourceKind.File || _element.Source is not { IsFile: true } uri || !uri.Equals(CreateSourceUri(snapshot))) return false;
        try
        {
            var graph = new DirectShowVideo(_videoSurface, uri.LocalPath);
            _directShow = graph;
            _element.Close();
            _element.Source = null;
            ApplySettings(snapshot);
            if (position > TimeSpan.Zero) graph.Seek(position);
            if (snapshot.State == MediaPlaybackState.Playing) graph.Play();
            else if (snapshot.State == MediaPlaybackState.Paused) graph.Pause();
            return true;
        }
        catch (Exception ex)
        {
            _directShow?.Dispose(); _directShow = null;
            error = ex.Message;
            return false;
        }
    }

    public void Play()
    {
        if (_directShow != null) _directShow.Play(); else _element.Play();
    }

    public void Pause()
    {
        CancelSwitch();
        if (_directShow != null) _directShow.Pause(); else _element.Pause();
    }

    public void Stop()
    {
        CancelSwitch();
        if (_directShow != null) { _directShow.Stop(); return; }
        _element.Stop();
        _element.Position = TimeSpan.Zero;
    }

    public void Seek(TimeSpan position) { if (_directShow == null) _element.Position = position; else if (FileGraph != null) _directShow.Seek(position); }

    public void Unload()
    {
        CancelSwitch();
        ClearMedia(releaseCamera: true);
    }

    private void ClearMedia(bool releaseCamera)
    {
        _ownership?.ReleaseLocalCamera -= Unload;
        _directShow?.Dispose();
        _directShow = null;
        if (releaseCamera) _ownership?.Release(this);
        // 소스를 비우면 OutputWindow.xaml 의 트리거(Source==null → Collapsed)가 작동해
        // 출력 창에서 미디어가 사라지고 그 아래의 가사/타이틀이 다시 보인다.
        _element.Stop();
        _element.Source = null;
        _element.Position = TimeSpan.Zero;
    }

    public void ApplySettings(MediaPlaybackSnapshot snapshot)
    {
        _directShow?.SetAudio(snapshot);
        var stretch = snapshot.PlayerDisplay != null ? Stretch.UniformToFill
            : snapshot.IsWidescreen ? Stretch.Uniform : Stretch.Fill;
        _videoSurface?.Stretch = stretch;
        _element.Volume = snapshot.Volume / 100d;
        _element.Balance = snapshot.Balance / 100d;
        _element.IsMuted = snapshot.IsMuted;
        _element.Stretch = stretch;
    }

    private static Uri CreateSourceUri(MediaPlaybackSnapshot snapshot)
    {
        if (snapshot.SourceKind != MediaSourceKind.File)
        {
            throw new MediaPlaybackException($"Unsupported media source kind: {snapshot.SourceKind}");
        }

        return Uri.TryCreate(snapshot.Source, UriKind.Absolute, out var uri)
            ? uri
            : new Uri(Path.GetFullPath(snapshot.Source), UriKind.Absolute);
    }
}
