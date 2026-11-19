using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using Easislides.Wpf.Shell;
using Microsoft.Win32;

namespace Easislides.Wpf.Media;

public partial class MediaAssignmentWindow : Window
{
    private readonly string _initialDirectory;
    private readonly ItemMediaOptions _initialOptions;
    private readonly WpfMediaElementPlaybackBackend _preview;
    private MediaPlaybackSnapshot? _loaded;
    private readonly System.Windows.Threading.DispatcherTimer _clock = new() { Interval = TimeSpan.FromMilliseconds(500) };

    public MediaAssignmentWindow(string title, ItemMediaOptions options, string directory, IEnumerable<OutputDisplay> displays)
    {
        InitializeComponent();
        Title = title;
        _initialOptions = Options = options;
        _initialDirectory = directory;
        _preview = new(Player, CameraImage);
        SourcePath.Text = options.FileName;
        ModeBox.SelectedIndex = options.Mode < 0 && options.FileName.Length > 0 ? 3 : options.Mode + 1;
        CameraBox.ItemsSource = Enumerable.Range(1, 5);
        CameraBox.SelectedItem = options.Camera;
        VolumeBox.Text = options.Volume?.ToString(CultureInfo.InvariantCulture) ?? "";
        BalanceBox.Text = options.Balance?.ToString(CultureInfo.InvariantCulture) ?? "";
        OverrideFlags.IsChecked = options.Flags.HasValue;
        int flags = options.Flags ?? 4;
        Mute.IsChecked = (flags & 1) != 0;
        Repeat.IsChecked = (flags & 2) != 0;
        Wide.IsChecked = (flags & 4) != 0;
        MonitorBox.ItemsSource = displays.Select(display => display.Id).Distinct().Prepend("");
        MonitorBox.Text = options.Monitor;
        _clock.Tick += PreviewTick;
        Closed += (_, _) => { _clock.Stop(); _clock.Tick -= PreviewTick; _preview.Unload(); };
    }

    public ItemMediaOptions Options { get; private set; }

    private void Browse_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog { InitialDirectory = _initialDirectory,
            Filter = "미디어|*.mp4;*.m4v;*.mov;*.avi;*.wmv;*.mpg;*.mpeg;*.mkv;*.webm;*.mp3;*.wav;*.wma;*.m4a;*.aac;*.flac;*.ogg|모든 파일|*.*" };
        if (dialog.ShowDialog(this) == true) { SourcePath.Text = dialog.FileName; ModeBox.SelectedIndex = 3; }
    }

    private bool ReadOptions(out ItemMediaOptions options)
    {
        options = _initialOptions;
        static bool ReadNumber(string text, int min, int max, out int? result)
        {
            result = null;
            if (string.IsNullOrWhiteSpace(text)) return true;
            if (!int.TryParse(text, out int value) || value < min || value > max) return false;
            result = value;
            return true;
        }
        if (!ReadNumber(VolumeBox.Text, 0, 100, out var volume) || !ReadNumber(BalanceBox.Text, -100, 100, out var balance))
        { Status.Text = "음량은 0~100, 좌우 균형은 -100~100 사이로 입력하세요."; return false; }
        int mode = ModeBox.SelectedIndex - 1;
        string path = SourcePath.Text.Trim();
        if (mode == -1 && path.Length > 0 && path != _initialOptions.FileName) mode = 2;
        int? flags = OverrideFlags.IsChecked != true ? null : ((_initialOptions.Flags ?? 0) & ~7)
            | (Mute.IsChecked == true ? 1 : 0) | (Repeat.IsChecked == true ? 2 : 0) | (Wide.IsChecked == true ? 4 : 0);
        options = new(mode, mode == 2 ? path : "", volume, balance, flags, CameraBox.SelectedItem is int camera ? camera : 1, MonitorBox.Text.Trim());
        return true;
    }

    private string ResolveFile(string path) => Path.IsPathFullyQualified(path) ? path : Path.Combine(_initialDirectory, path);

    private void Play_Click(object sender, RoutedEventArgs e)
    {
        if (!ReadOptions(out var options)) return;
        try
        {
            bool camera = options.Mode == 3;
            var path = camera ? MediaPlaybackService.CreateLiveCameraSource(options.Camera)
                : options.Mode == 1 && Directory.Exists(_initialDirectory) ? Directory.EnumerateFiles(_initialDirectory, "*", SearchOption.AllDirectories)
                    .FirstOrDefault(file => string.Equals(Path.GetFileNameWithoutExtension(file), Title, StringComparison.OrdinalIgnoreCase))
                : options.Mode == 2 ? ResolveFile(options.FileName) : null;
            if (!camera && !File.Exists(path)) { Status.Text = "재생할 파일을 찾을 수 없습니다."; return; }
            var snapshot = MediaPlaybackSnapshot.Empty with { State = MediaPlaybackState.Playing, Source = path!, SourceKind = camera ? MediaSourceKind.CaptureDevice : MediaSourceKind.File,
                Volume = options.Volume ?? 50, Balance = options.Balance ?? 0, IsMuted = Mute.IsChecked == true && options.Flags.HasValue,
                IsWidescreen = Wide.IsChecked == true || !options.Flags.HasValue, IsRepeatEnabled = Repeat.IsChecked == true && options.Flags.HasValue };
            if (_loaded?.Source != snapshot.Source) _preview.Load(snapshot); else _preview.ApplySettings(snapshot);
            _loaded = snapshot;
            _preview.Play();
            Status.Text = "";
        }
        catch (Exception ex) { _loaded = null; Status.Text = ex.Message; }
    }

    private void Pause_Click(object sender, RoutedEventArgs e)
    {
        _preview.Pause();
        if (_loaded != null) _loaded = _loaded with { State = MediaPlaybackState.Paused };
    }
    private void Stop_Click(object sender, RoutedEventArgs e)
    {
        _preview.Stop();
        if (_loaded != null) _loaded = _loaded with { State = MediaPlaybackState.Stopped };
    }
    private void PlaybackFailed(object sender, ExceptionRoutedEventArgs e)
    {
        string? details = null;
        if (_loaded != null && _preview.TryFileFallback(_loaded, Player.Position, out details)) { _clock.Start(); return; }
        _loaded = null;
        Status.Text = details ?? e.ErrorException.Message;
    }

    private void PreviewTick(object? sender, EventArgs args)
    {
        if (_preview.FileGraph is not { } graph) { _clock.Stop(); return; }
        try { if (graph.ReadClock().Ended && _loaded?.IsRepeatEnabled == true) { graph.Seek(TimeSpan.Zero); graph.Play(); } }
        catch (Exception ex) { _clock.Stop(); Status.Text = ex.Message; }
    }
    private void PreviewEnded(object sender, RoutedEventArgs e)
    {
        if (_loaded?.IsRepeatEnabled == true) { _preview.Seek(TimeSpan.Zero); _preview.Play(); }
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        if (!ReadOptions(out var options)) return;
        if (options.Mode == 2 && (options.FileName.Length == 0 ||
            (options.FileName != _initialOptions.FileName && !File.Exists(ResolveFile(options.FileName)))))
        { Status.Text = "파일 위치를 확인하세요."; return; }
        Options = options;
        DialogResult = true;
    }
}
