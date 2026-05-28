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
