using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Axicultus.Wpf.Input;
using Axicultus.Wpf.Platform;
using Axicultus.Wpf.Rendering;
using Axicultus.Wpf.Shell;

namespace Axicultus.Wpf.Media;

// A native PowerPoint slide show is a separate top-level window. Its video overlay needs its own HWND.
internal sealed class OutputMonitorMediaPlayer : IMediaPlaybackBackend, IDisposable
{
    private readonly Window _window;
    private readonly IWindowPlacementService _placement;
    private readonly ShortcutRegistry _shortcuts;
    private readonly IPowerPointSlideShowControl _slides;
    private readonly MediaPlaybackService _playback;
    private readonly WpfMediaElementPlaybackBackend _backend;
    private readonly FilePlaybackEvents _events;
    private OutputWindowPlacement? _bounds;
    private OutputWindowPlacement? _slideBounds;
    private bool _closed;
    private bool _presentOnPlay;

    internal OutputMonitorMediaPlayer(MediaPlaybackService playback, IWindowPlacementService placement, ShortcutRegistry shortcuts, IPowerPointSlideShowControl slides)
    {
        _placement = placement;
        _shortcuts = shortcuts;
        _slides = slides;
        _playback = playback;
        var element = new MediaElement { LoadedBehavior = MediaState.Manual, UnloadedBehavior = MediaState.Manual };
        var fallback = new Image();
        _window = new Window
        {
            Title = "Axicultus Media Output", Background = Brushes.Black,
            Content = new Grid { ClipToBounds = true, Children = { element, fallback } },
            WindowStyle = WindowStyle.None, ResizeMode = ResizeMode.NoResize, WindowStartupLocation = WindowStartupLocation.Manual,
            Topmost = true, ShowInTaskbar = false
        };
        _backend = new(element, fallback);
        _events = new(element, _backend, playback, standalone: true);
        _window.Closing += Closing;
        _window.Closed += (_, _) => _closed = true;
        _window.PreviewKeyDown += KeyDown;
        _window.MouseDoubleClick += DoubleClick;
        _playback.PlaybackChanged += PlaybackChanged;
    }

    public void Load(MediaPlaybackSnapshot snapshot)
    {
        var display = snapshot.PlayerDisplay ?? throw new InvalidOperationException();
        _bounds = _placement.CreateMediaPlacement(display);
        _slideBounds = _placement.CreateOutputPlacement(display, windowed: false);
        DesktopWindowPlacement.Apply(_window, _bounds);
        _backend.Load(snapshot);
        _presentOnPlay = true;
    }

    public void Play()
    {
        if (_bounds is null) return;
        if (_presentOnPlay || !_window.IsVisible)
        {
            if (!_window.IsVisible) _window.Show();
            DesktopWindowPlacement.Apply(_window, _bounds);
            DesktopWindowPlacement.SetTopmost(new System.Windows.Interop.WindowInteropHelper(_window).Handle, true);
            _window.Activate();
            _presentOnPlay = false;
        }
        _backend.Play();
    }

    public void Pause() => _backend.Pause();
    public void Seek(TimeSpan position) => _backend.Seek(position);
    public void ApplySettings(MediaPlaybackSnapshot snapshot) => _backend.ApplySettings(snapshot);

    public void Stop()
    {
        var slideBounds = _slideBounds;
        bool hadFocus = _window.IsActive;
        Unload();
        if (slideBounds != null) _slides.RevealOutputAfterMedia(slideBounds, hadFocus);
    }

    public void Unload()
    {
        try { _backend.Unload(); }
        finally
        {
            _bounds = _slideBounds = null;
            if (_window.IsVisible) _window.Hide();
        }
    }

    private void PlaybackChanged(object? sender, MediaPlaybackChangedEventArgs e)
    {
        if (e.Snapshot is { PlayerDisplay: not null, State: MediaPlaybackState.Failed }) Unload();
    }

    private void Closing(object? sender, CancelEventArgs e)
    {
        e.Cancel = true;
        _window.Dispatcher.BeginInvoke(() =>
        {
            if (!_closed && _window.IsVisible) _playback.Stop();
        });
    }

    private void DoubleClick(object sender, MouseButtonEventArgs e) => _playback.Stop();

    private void KeyDown(object sender, KeyEventArgs e)
    {
        if (Keyboard.Modifiers != ModifierKeys.None) return;
        if (e.Key is Key.F9 or Key.F10 && _shortcuts.TryHandle(e.Key, ModifierKeys.None)) { e.Handled = true; return; }
        var key = (System.Windows.Forms.Keys)KeyInterop.VirtualKeyFromKey(e.Key);
        if (!_shortcuts.TryResolveMedia(key, System.Windows.Forms.Keys.None, out var command)) return;
        e.Handled = true;
        if (!e.IsRepeat) command();
    }

    public void Dispose()
    {
        _playback.PlaybackChanged -= PlaybackChanged;
        _window.Closing -= Closing;
        _window.PreviewKeyDown -= KeyDown;
        _window.MouseDoubleClick -= DoubleClick;
        _events.Dispose();
        _backend.Unload();
        if (!_closed) _window.Close();
    }
}
