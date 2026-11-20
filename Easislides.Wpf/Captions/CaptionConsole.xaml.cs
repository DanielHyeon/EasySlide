using System;
using System.Windows.Media.Imaging;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Easislides.Wpf.Shell;
using Easislides.Wpf.Media;
using Easislides.Wpf.Rendering;
using Easislides.Wpf.Support;
using System.Threading;
using Microsoft.Win32;

namespace Easislides.Wpf.Captions;

public partial class CaptionConsole : Window
{
    private readonly CaptionSession _captions;
    private readonly CaptionConsoleViewModel _editor;
    private readonly OperationalErrorLog _errors;
    private DispatcherOperation? _compositionCommit;
    private readonly CaptionBroadcastServer _server;
    private readonly CaptionRecoveryStore _store;
    private readonly MainViewModel _main;
    private readonly CameraOwnership _camera;
    private bool _ending;
    private bool _fetchingPreview;
    private readonly DispatcherTimer _previewTimer = new(DispatcherPriority.Background) { Interval = TimeSpan.FromMilliseconds(500) };
    private readonly CancellationTokenSource _lifetime = new();
    private readonly DispatcherTimer _debounce = new() { Interval = TimeSpan.FromMilliseconds(600) };
    private CaptionRecovery? _recoverable;
    private readonly ObsOperator _obs;
    private bool _closed, _dirty;
    private long _revision, _deckRequest;
    private readonly string _plan;

    public CaptionConsole(MainViewModel main, CaptionSession captions, CaptionBroadcastServer server, CaptionRecoveryStore store, CameraOwnership camera, ObsOperator obs)
        : this(main, captions, server, store, camera, obs, OperationalErrorLog.Default) { }

    internal CaptionConsole(MainViewModel main, CaptionSession captions, CaptionBroadcastServer server, CaptionRecoveryStore store, CameraOwnership camera, ObsOperator obs, OperationalErrorLog errors)
    {
        _errors = errors;
        _editor = new(captions);
        _main = main;
        _camera = camera;
        _obs = obs;
        _captions = captions;
        _server = server;
        _store = store;
        _plan = main.CurrentWorshipListName;
        captions.BeginPlan(_plan);
        InitializeComponent();
        DataContext = _editor;
        _editor.SaveRequested += Changed;
        CameraControls.DataContext = camera;
        Port.Text = obs.Port.ToString();
        AutoObs.IsChecked = obs.AutoConnect;
        var shortcuts = obs.Shortcuts;
        Scene1.Text = shortcuts[0];
        Scene2.Text = shortcuts[1];
        Scene3.Text = shortcuts[2];
        Scene4.Text = shortcuts[3];
        _obs.Changed += ShowObsStatus;
        ShowObsStatus(obs.Status);
        Draft.AddHandler(TextCompositionManager.PreviewTextInputStartEvent, new TextCompositionEventHandler(OnCompositionStarted), true);
        Draft.AddHandler(TextCompositionManager.PreviewTextInputUpdateEvent, new TextCompositionEventHandler(OnCompositionStarted), true);
        Draft.AddHandler(TextCompositionManager.TextInputEvent, new TextCompositionEventHandler((_, _) =>
        {
            if (!_editor.IsComposing || _compositionCommit != null) return;
            // Let TextBox and its binding finish the IME commit before publishing.
            _compositionCommit = Dispatcher.InvokeAsync(() =>
            {
                _compositionCommit = null;
                _editor.CompleteComposition();
            }, DispatcherPriority.DataBind);
        }), true);
        Draft.LostKeyboardFocus += (_, _) => { if (_compositionCommit == null) _editor.IsComposing = false; };
        _debounce.Tick += async (_, _) => { _debounce.Stop(); await SaveAsync(); };
        _previewTimer.Tick += ReadObsPreview;
        IsVisibleChanged += (_, _) => UpdatePreviewTimer();
        PreviewObsCamera.Checked += (_, _) => UpdatePreviewTimer();
        PreviewObsCamera.Unchecked += (_, _) => UpdatePreviewTimer();
        _camera.PropertyChanged += CameraChanged;
        _main.QueueReplacing += BeforeQueueReplacement;
        _camera.ReleaseLocalCamera += OnCameraRelease;
        Closing += (_, e) => { if (!_ending) { e.Cancel = true; Hide(); } };
        PreviewKeyDown += (_, e) =>
        {
            if (_obs.TrySceneShortcut(e)) { e.Handled = true; return; }
            if (e.Key == Key.Escape && !_editor.IsComposing && !e.IsRepeat)
            {
                _editor.HideCommand.Execute(null);
                e.Handled = true;
            }
        };
        _main.PropertyChanged += MainChanged;
        _captions.PropertyChanged += CaptionChanged;
        Loaded += LoadRecovery;
        Closed += OnConsoleClosed;
        if (captions.Deck is { } deck) ShowDeck(deck, captions.CurrentSlide, captions.BoxKey);
    }

    private void MainChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainViewModel.CurrentWorshipListName) && _main.CurrentWorshipListName != _plan)
        {
            EndSession();
            _captions.BeginPlan(_main.CurrentWorshipListName);
        }
    }

    internal void EndSession()
    {
        _ending = true;
        Close();
    }

    private void BeforeQueueReplacement()
    {
        if (!_editor.IsRestoring && _dirty) _ = SaveAsync();
    }

    private void OnCameraRelease()
    {
        if (_main.Media.MediaType == "Live Camera") _main.Media.Unload();
    }

    private void CameraChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(CameraOwnership.ObsOwnsCamera))
        {
            _camera.Preview = null;
            UpdatePreviewTimer();
        }
    }

    private void UpdatePreviewTimer()
    {
        _previewTimer.IsEnabled = !_closed && IsVisible && _camera.ObsOwnsCamera && PreviewObsCamera.IsChecked == true;
        if (!_previewTimer.IsEnabled && _camera.ObsOwnsCamera) _camera.Preview = null;
    }

    private async void ReadObsPreview(object? sender, EventArgs e)
    {
        var connection = _obs.Connection;
        var name = CameraSource.Text.Trim();
        if (connection == null || name.Length == 0) { _camera.Preview = null; return; }
        if (_fetchingPreview) return;
        _fetchingPreview = true;
        try
        {
            var bytes = await connection.ReadPreviewAsync(name, _lifetime.Token);
            var bitmap = await Task.Run(() =>
            {
                using var input = new MemoryStream(bytes, false);
                var image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.StreamSource = input;
                image.EndInit();
                image.Freeze();
                return image;
            });
            if (connection != _obs.Connection || !_previewTimer.IsEnabled || name != CameraSource.Text.Trim()) return;
            _camera.ScaleY = 1;
            _camera.Preview = bitmap;
            _camera.Status = "OBS 카메라 미리보기";
        }
        catch (Exception ex)
        {
            if (connection == _obs.Connection && _previewTimer.IsEnabled)
            {
                _camera.Preview = null;
                _camera.Status = "OBS 미리보기 대기 · " + ex.Message;
            }
        }
        finally { _fetchingPreview = false; }
    }

    private void CaptionChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(CaptionSession.CurrentSlide) or nameof(CaptionSession.Following))
            Status.Text = _captions.Following ? $"PPT 따라가기 · {_captions.CurrentSlide}장" : "직접 입력";
    }

    private async void LoadRecovery(object sender, RoutedEventArgs e)
    {
        try
        {
            _recoverable = await Task.Run(() => _store.Read(_plan));
            if (_closed) return;
            RecoverButton.IsEnabled = _recoverable != null;
            if (_recoverable != null)
            {
                _revision = Math.Max(_revision, _recoverable.Revision);
                Status.Text = "이 예배의 저장된 초안과 순서를 복구할 수 있습니다.";
            }
        }
        catch (Exception ex) when (IsFileError(ex))
        {
            _errors.Write("자막 복구본 읽기", ex);
            if (!_closed) Status.Text = ex.Message;
        }
    }

    // The main window journals queue edits even while the caption editor is closed.
    private void Changed()
    {
        if (_editor.IsRestoring || _closed) return;
        _dirty = true;
        _debounce.Stop();
        _debounce.Start();
    }

    private async Task SaveAsync()
    {
        if (!_dirty) return;
        _dirty = false;
        var snapshot = new CaptionRecovery(1, ++_revision, _plan, _editor.DraftText, _captions.Deck?.Path,
            SelectedSlide, SelectedKey, new(_captions.Overrides), _captions.History.ToArray(),
            _main.Queue.Select(CaptionRecoveryStore.Capture).ToArray(), _main.SelectedItem == null ? -1 : _main.Queue.IndexOf(_main.SelectedItem));
        try { await _store.SaveAsync(snapshot); }
        catch (Exception ex) when (IsFileError(ex))
        {
            _dirty = true;
            _errors.Write("자막 복구본 저장", ex);
            if (!_closed) Status.Text = "복구본 저장 실패 · " + ex.Message;
        }
    }

    private async void Recover(object sender, RoutedEventArgs e)
    {
        if (_editor.IsRestoring || _recoverable is not { } saved) return;
        if (MessageBox.Show(this,"현재 편집 초안과 예배 순서를 저장한 시점으로 복구할까요?", "예배 편집 복구",
            MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;
        _editor.IsRestoring = true;
        ++_deckRequest; // A pending Open must not replace the recovered deck.
        try
        {
            CaptionDeck? deck = null;
            string? warning = null;
            if (saved.DeckPath != null)
            {
                try { deck = await ReadDeckAsync(saved.DeckPath); }
                catch (OperationCanceledException) when (_lifetime.IsCancellationRequested) { return; }
                catch (Exception ex) when (IsDeckReadError(ex))
                {
                    _errors.Write("자막 PPT 복구", ex);
                    warning = "PPT를 다시 열어 주세요. " + ex.Message;
                }
            }
            if (_closed) return;
            _editor.IsInstant = false;
            _editor.DraftText = saved.Draft;
            _main.LoadQueue(saved.Items.Select(CaptionRecoveryStore.Restore));
            if (saved.SelectedIndex >= 0 && saved.SelectedIndex < _main.Queue.Count)
                _main.SelectedItem = _main.Queue[saved.SelectedIndex];
            _main.WorshipListHasUnsavedChanges = true;
            if (deck != null)
            {
                _captions.Prepare(deck);
                foreach (var pair in saved.Overrides) _captions.Overrides[pair.Key] = pair.Value;
                ShowDeck(deck, saved.Slide, saved.Box);
            }
            _captions.History.Clear();
            _captions.History.AddRange(saved.History);
            Status.Text = warning ?? "편집을 복구했습니다. 송출할 문구를 확인하세요.";
        }
        finally { _editor.IsRestoring = false; }
    }

    private void OnCompositionStarted(object sender, TextCompositionEventArgs e)
    {
        _compositionCommit?.Abort();
        _compositionCommit = null;
        _editor.IsComposing = true;
    }

    private void ToggleCamera(object sender, RoutedEventArgs e)
    {
        if (CameraControls.DataContext is CameraOwnership { ObsOwnsCamera: true }) return;
        _main.ToggleOutputCameraCommand.Execute(null);
    }
    private void DraftKey(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && Keyboard.Modifiers == ModifierKeys.Control)
        {
            e.Handled = true;
            _editor.TakeFromShortcut(e.IsRepeat);
        }
    }

    private async Task<CaptionDeck> ReadDeckAsync(string path)
    {
        if (Path.GetExtension(path).ToLowerInvariant() is ".ppt" or ".pps")
        {
            using var worker = new PowerPointWorker();
            return await worker.ReadLegacyDeckAsync(path, _lifetime.Token).ConfigureAwait(false);
        }
        var token = _lifetime.Token;
        return await Task.Run(() => CaptionDeck.Read(path, token), token).ConfigureAwait(false);
    }

    private async void OpenDeck(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog { Filter = "PowerPoint|*.pptx;*.ppsx;*.pptm;*.ppsm;*.ppt;*.pps", CheckFileExists = true };
        if (dialog.ShowDialog(this) == true) await PrepareDeckAsync(dialog.FileName, 1);
    }

    private async void OpenLiveDeck(object sender, RoutedEventArgs e)
    {
        var live = _main.Session.Current;
        if (live.PptPath == null) { Status.Text = "현재 송출 중인 PPT가 없습니다."; return; }
        await PrepareDeckAsync(live.PptPath, live.PptSlideNumber);
    }

    internal async Task PrepareDeckAsync(string path, int slide)
    {
        if (_editor.IsRestoring) return;
        long request = ++_deckRequest;
        CaptionDeck deck;
        try
        {
            deck = await ReadDeckAsync(path);
        }
        catch (OperationCanceledException) when (_lifetime.IsCancellationRequested) { return; }
        catch (Exception ex) when (IsDeckReadError(ex))
        {
            _errors.Write("자막 PPT 열기", ex);
            if (!_closed && request == _deckRequest) Status.Text = "PPT 읽기 실패 · " + ex.Message;
            return;
        }
        if (_closed || request != _deckRequest) return;
        _captions.Prepare(deck);
        ShowDeck(deck, slide, "");
        Status.Text = "텍스트 상자를 고른 뒤 PPT 따라가기를 누르세요.";
        Changed();
    }

    private static bool IsFileError(Exception error) => error is
        IOException or InvalidDataException or UnauthorizedAccessException or System.Text.Json.JsonException;

    private static bool IsDeckReadError(Exception error) => IsFileError(error) || error is System.Xml.XmlException
        or FormatException or InvalidOperationException or ArgumentException or NotSupportedException or TimeoutException
        or Win32Exception or PowerPointRenderException;

    private int SelectedSlide => Slides.SelectedItem is int slide ? slide : 1;
    private string SelectedKey => (Boxes.SelectedItem as CaptionBox)?.Key ?? "";
    private void ShowDeck(CaptionDeck deck, int slide, string key)
    {
        DeckLabel.Text = Path.GetFileName(deck.Path);
        Slides.ItemsSource = Enumerable.Range(1, deck.Slides.Length);
        Slides.SelectedItem = Math.Clamp(slide, 1, deck.Slides.Length);
        if (Boxes.Items.Cast<CaptionBox>().FirstOrDefault(b => b.Key == key) is { } selected)
            Boxes.SelectedItem = selected;
        RefreshCue();
    }

    private void SlideSelected(object sender, SelectionChangedEventArgs e)
    {
        if (_captions.Deck is not {} deck || SelectedSlide > deck.Slides.Length) return;
        var key = SelectedKey;
        var boxes = deck.Slides[SelectedSlide - 1].Boxes;
        Boxes.ItemsSource = boxes;
        Boxes.SelectedItem = boxes.FirstOrDefault(b => b.Key == key) ?? boxes.FirstOrDefault();
        RefreshCue();
        if (IsLoaded) Changed();
    }

    private void BoxSelected(object sender, SelectionChangedEventArgs e)
    {
        RefreshCue();
        if (IsLoaded) Changed();
    }
    private void RefreshCue() => Cue.Text = _captions.TextFor(SelectedSlide, SelectedKey);
    private void SaveOverride(object sender, RoutedEventArgs e)
    {
        if (_captions.Deck == null) return;
        _captions.Overrides[SelectedSlide] = Cue.Text;
        Status.Text = $"{SelectedSlide}장 수동 문구를 저장했습니다.";
        Changed();
    }

    private void Follow(object sender, RoutedEventArgs e)
    {
        _editor.IsInstant = false;
        _captions.Follow(SelectedSlide, SelectedKey);
        Changed();
    }

    private void Previous(object sender, RoutedEventArgs e) => Step(-1);
    private void Next(object sender, RoutedEventArgs e) => Step(1);
    private void Step(int change)
    {
        if (!_captions.Following) { Status.Text = "먼저 PPT 따라가기를 선택하세요."; return; }
        _captions.MoveToSlide(_captions.CurrentSlide + change);
        Slides.SelectedItem = _captions.CurrentSlide;
        Changed();
    }

    private async void StartBrowser(object sender, RoutedEventArgs e)
    {
        try { await _server.StartAsync(); Status.Text = "OBS 자막 소스 준비됨 · " + CaptionBroadcastServer.Url; }
        catch (Exception ex) { Status.Text = "자막 서버 시작 실패 · " + ex.GetBaseException().Message; }
    }

    private async void ConnectObs(object sender, RoutedEventArgs e)
    {
        if (!int.TryParse(Port.Text, out int port) || port is < 1 or > 65535) { ObsLabel.Text = "포트를 확인하세요."; return; }
        try
        {
            await _obs.ConfigureAsync(port, Password.Password, AutoObs.IsChecked == true,
                [Scene1.Text, Scene2.Text, Scene3.Text, Scene4.Text]);
            if (AutoObs.IsChecked == true) await _server.StartAsync();
        }
        catch (Exception ex) { Status.Text = ex.Message; }
    }

    private void ShowObsStatus(ObsStatus status)
    {
        ObsLabel.Text = status.Message + (status.Scene.Length > 0 ? " · " + status.Scene : "");
        if (Scenes.Items.Cast<string>().SequenceEqual(status.Scenes)) return;
        Scenes.ItemsSource = status.Scenes;
        Scenes.SelectedItem = status.Scene;
    }

    private async void SwitchScene(object sender, RoutedEventArgs e)
    {
        if (_obs.Connection is not { } connection || Scenes.SelectedItem is not string scene) return;
        try { await connection.SwitchAsync(scene, SourceName.Text.Trim(), EnableSource.IsChecked == true); }
        catch (Exception ex) { Status.Text = ex.Message; }
    }

    private async void OnConsoleClosed(object? sender, EventArgs e)
    {
        _closed = true;
        _compositionCommit?.Abort();
        _editor.SaveRequested -= Changed;
        _lifetime.Cancel();
        _lifetime.Dispose();
        _debounce.Stop();
        _previewTimer.Stop();
        _previewTimer.Tick -= ReadObsPreview;
        _camera.PropertyChanged -= CameraChanged;
        if (_camera.ObsOwnsCamera) _camera.Preview = null;
        _main.QueueReplacing -= BeforeQueueReplacement;
        _camera.ReleaseLocalCamera -= OnCameraRelease;
        _main.PropertyChanged -= MainChanged;
        _captions.PropertyChanged -= CaptionChanged;
        // The last write runs on a pool thread. Finish before the process can exit.
        _obs.Changed -= ShowObsStatus;
        await SaveAsync();
    }
}
