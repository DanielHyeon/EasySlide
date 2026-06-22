using System;
using System.IO;
using System.Windows.Controls;
using System.Windows.Media;

namespace Easislides.Wpf.Media;

public sealed class MediaPlaybackException : Exception
{
    public MediaPlaybackException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }
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

public sealed class WpfMediaElementPlaybackBackend : IMediaPlaybackBackend
{
    private readonly MediaElement _element;

    public WpfMediaElementPlaybackBackend(MediaElement element)
    {
        _element = element ?? throw new ArgumentNullException(nameof(element));
        _element.LoadedBehavior = MediaState.Manual;
        _element.UnloadedBehavior = MediaState.Manual;
    }

    public void Load(MediaPlaybackSnapshot snapshot)
    {
        _element.Source = CreateSourceUri(snapshot);
        _element.Position = TimeSpan.Zero;
        ApplySettings(snapshot);
    }

    public void Play() => _element.Play();

    public void Pause() => _element.Pause();

    public void Stop()
    {
        _element.Stop();
        _element.Position = TimeSpan.Zero;
    }

    public void Seek(TimeSpan position) => _element.Position = position;

    public void Unload()
    {
        // 소스를 비우면 OutputWindow.xaml 의 트리거(Source==null → Collapsed)가 작동해
        // 출력 창에서 미디어가 사라지고 그 아래의 가사/타이틀이 다시 보인다.
        _element.Stop();
        _element.Source = null;
        _element.Position = TimeSpan.Zero;
    }

    public void ApplySettings(MediaPlaybackSnapshot snapshot)
    {
        _element.Volume = snapshot.Volume / 100d;
        _element.Balance = snapshot.Balance / 100d;
        _element.IsMuted = snapshot.IsMuted;
        _element.Stretch = snapshot.IsWidescreen ? Stretch.Uniform : Stretch.Fill;
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
