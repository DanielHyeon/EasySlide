using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Axicultus.Wpf.Media;

internal sealed class FilePlaybackEvents : IDisposable
{
    private readonly MediaElement _element;
    private readonly WpfMediaElementPlaybackBackend _backend;
    private readonly MediaPlaybackService _playback;
    private readonly DispatcherTimer _clock;
    private readonly bool _standalone;

    internal FilePlaybackEvents(MediaElement element, WpfMediaElementPlaybackBackend backend, MediaPlaybackService playback, bool standalone)
    {
        _element = element;
        _backend = backend;
        _playback = playback;
        _standalone = standalone;
        _clock = new DispatcherTimer(DispatcherPriority.Background, element.Dispatcher) { Interval = TimeSpan.FromMilliseconds(500) };
        _clock.Tick += Tick;
        element.MediaOpened += Opened;
        element.MediaEnded += Ended;
        element.MediaFailed += Failed;
    }

    private bool OwnsPlayback => (_playback.Current.PlayerDisplay != null) == _standalone;

    private void Tick(object? sender, EventArgs e)
    {
        if (!OwnsPlayback) { _clock.Stop(); return; }
        if (_backend.FileGraph is { FileName: { } path } graph)
        {
            try
            {
                var clock = graph.ReadClock();
                _playback.ReportFilePosition(path, clock.Position, clock.Duration);
                if (clock.Ended) _playback.ReportFileEnded(path);
            }
            catch (Exception ex) { _clock.Stop(); _playback.ReportFileFailure(path, ex); }
        }
        else if (_element.Source is { IsFile: true } uri && _element.NaturalDuration.HasTimeSpan)
            _playback.ReportFilePosition(uri.LocalPath, _element.Position, _element.NaturalDuration.TimeSpan);
        else _clock.Stop();
    }

    private void Opened(object sender, RoutedEventArgs e)
    {
        if (!OwnsPlayback) return;
        _clock.Start();
        Tick(sender, e);
    }

    private void Ended(object sender, RoutedEventArgs e)
    {
        if (OwnsPlayback && _element.Source is { IsFile: true } uri) _playback.ReportFileEnded(uri.LocalPath);
    }

    private void Failed(object? sender, ExceptionRoutedEventArgs e)
    {
        if (!OwnsPlayback || _element.Source is not { IsFile: true } uri) return;
        if (_backend.TryFileFallback(_playback.Current, _playback.Current.Position, out var details))
        {
            _clock.Start();
            return;
        }
        _clock.Stop();
        var error = details == null ? e.ErrorException : new MediaPlaybackException($"{e.ErrorException.Message} / DirectShow: {details}", e.ErrorException);
        _playback.ReportFileFailure(uri.LocalPath, error);
    }

    public void Dispose()
    {
        _clock.Stop();
        _clock.Tick -= Tick;
        _element.MediaOpened -= Opened;
        _element.MediaEnded -= Ended;
        _element.MediaFailed -= Failed;
    }
}
