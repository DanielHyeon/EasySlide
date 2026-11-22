using System;
using System.Collections.Generic;
using System.Windows.Media.Imaging;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Axicultus.Wpf.Shell;
using Axicultus.Wpf.Controls;
using Axicultus.Wpf.Media;
using Axicultus.Wpf.Rendering;
using Axicultus.Wpf.Support;
using System.Threading;
using Microsoft.Win32;

namespace Axicultus.Wpf.Captions;

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
    private readonly IPowerPointRenderService? _render;
    private bool _closed, _dirty;
    private long _revision, _deckRequest;
    private readonly string _plan;

    public CaptionConsole(MainViewModel main, CaptionSession captions, CaptionBroadcastServer server, CaptionRecoveryStore store, CameraOwnership camera, ObsOperator obs,
        IPowerPointRenderService? render = null)
        : this(main, captions, server, store, camera, obs, OperationalErrorLog.Default, render) { }

    internal CaptionConsole(MainViewModel main, CaptionSession captions, CaptionBroadcastServer server, CaptionRecoveryStore store, CameraOwnership camera, ObsOperator obs, OperationalErrorLog errors,
        IPowerPointRenderService? render = null)
    {
        _render = render;
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
        CameraControls.DataContext = camera; // before the window's own, or the camera bindings look at the editor first
        DataContext = _editor;
        _editor.SaveRequested += Changed;
        Host.Text = obs.Host;
        Port.Text = obs.Port.ToString();
        AutoObs.IsChecked = obs.AutoConnect;
        CameraSource.Text = obs.CameraSource;
        PreviewObsCamera.IsChecked = obs.CameraSource.Length > 0;
        SavedPassword.Visibility = obs.HasSavedPassword ? Visibility.Visible : Visibility.Collapsed;
        BoxCheck.IsChecked = captions.Style.Box;
        _editor.IsInstant = true;
        _main.Session.SessionChanged += LiveChanged;
        _main.OutputPowerPoint.PropertyChanged += SlideImageChanged;
        _main.PowerPoint.PropertyChanged += SlideImageChanged;
        Loaded += StartAutomatically;
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
            else if (e.Key is Key.Up or Key.Down && ScreenPanel.IsVisible && e.OriginalSource is not TextBox and not ComboBox)
            {
                Step(e.Key == Key.Down ? 1 : -1);
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
        else if (e.PropertyName is nameof(MainViewModel.OutputLyricsText) or nameof(MainViewModel.OutputItem)) _ = SyncScreenAsync();
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
        PreviewHint.Text = _previewTimer.IsEnabled ? "OBS에서 카메라 영상을 기다리는 중…" : "카메라 미리보기는 설정에서 켭니다";
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
        {
            if (e.PropertyName == nameof(CaptionSession.CurrentSlide)) _scoreRow = 0;
            Status.Text = _captions.Following ? $"PPT 따라가기 · {_captions.CurrentSlide}장" :
                _captions.Mirroring ? "송출 화면 자막을 따라가는 중" : "직접 입력";
            if (_captions.Following && Slides.SelectedItem is int shown && shown != _captions.CurrentSlide) Slides.SelectedItem = _captions.CurrentSlide;
            UpdateScore();
        }
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
            SelectedSlide, SelectedKey, [with(_captions.Overrides)], [.. _captions.History],
            [.. _main.Queue.Select(CaptionRecoveryStore.Capture)], _main.SelectedItem == null ? -1 : _main.Queue.IndexOf(_main.SelectedItem));
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
            _main.LoadRecoveredQueue(saved.Items.Select(CaptionRecoveryStore.Restore), saved.Plan);
            if (saved.SelectedIndex >= 0 && saved.SelectedIndex < _main.Queue.Count)
                _main.SelectedItem = _main.Queue[saved.SelectedIndex];
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
    // Enter (or Ctrl+Enter) sends the line and empties the box; Shift+Enter is a new line.
    private void DraftKey(object sender, KeyEventArgs e)
    {
        bool shift = Keyboard.Modifiers.HasFlag(ModifierKeys.Shift);
        if (e.Key == Key.Enter && !shift)
        {
            e.Handled = true;
            if (!e.IsRepeat) _editor.TakeAndClear();
        }
        else if (e.Key == Key.ImeProcessed && e.ImeProcessedKey == Key.Enter && !shift && !e.IsRepeat)
        {
            // The IME commits its syllable first (and may add a line break); send after that.
            Dispatcher.InvokeAsync(() =>
            {
                _editor.IsComposing = false;
                if (_editor.DraftText.EndsWith('\n')) _editor.DraftText = _editor.DraftText.TrimEnd('\r', '\n');
                _editor.TakeAndClear();
            }, DispatcherPriority.Input);
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
        if (dialog.ShowDialog(this) == true && await PrepareDeckAsync(dialog.FileName, 1)) FollowLoadedDeck();
    }

    // Reads the deck again: the one on the output screen, or the selected one.
    private async void OpenLiveDeck(object sender, RoutedEventArgs e)
    {
        if (FindCurrentDeck(includeSelected: true) is not var (path, slide)) { Status.Text = "송출 화면이나 선택한 항목에 PPT가 없습니다."; return; }
        if (await PrepareDeckAsync(path, slide) && ScreenMode.IsChecked == true) FollowLoadedDeck();
    }

    private static bool SamePath(string? a, string b) =>
        a != null && string.Equals(Path.GetFullPath(a), Path.GetFullPath(b), StringComparison.OrdinalIgnoreCase);

    private string? _readingDeck;

    // Screen mode follows the output screen whether or not the show is live: its deck, or else its text.
    private async Task SyncScreenAsync()
    {
        if (_closed || ScreenMode.IsChecked != true || !IsLoaded) return;
        if (FindCurrentDeck(includeSelected: false) is var (path, slide))
        {
            if (_captions.Deck == null || !SamePath(_captions.Deck.Path, path))
            {
                if (_readingDeck != null && SamePath(_readingDeck, path)) return;
                _captions.ShowScreenText("");
                _readingDeck = path;
                try { if (!await PrepareDeckAsync(path, slide)) return; }
                finally { _readingDeck = null; }
                FollowLoadedDeck();
                return;
            }
            if (!_captions.Following) FollowLoadedDeck();
            // Not live: the output panel's slide drives the caption (live, the session does).
            else if (_main.Session.Current.State == LiveState.Off && SamePath(_main.OutputPowerPoint.LoadedContentPath, path)
                && _main.OutputPowerPoint.SlideNumber >= 1 && _main.OutputPowerPoint.SlideNumber != _captions.CurrentSlide)
                _captions.MoveToSlide(_main.OutputPowerPoint.SlideNumber);
            UpdateScore();
            return;
        }
        // A deck just put in the output panel goes live a moment later; its (empty) panel text is not a caption.
        if (_main.OutputItem is { ContentPath.Length: > 0 } pending && LiveItemKindMatcher.IsPowerPoint(LiveItemKindMatcher.InferLegacyKind(pending))) return;
        _captions.ShowScreenText(_main.OutputLyricsText ?? "");
        UpdateScore();
        Status.Text = _main.HasOutputLyricsText ? "송출 화면의 자막을 따라가는 중" : "송출 화면에 자막으로 보낼 글자가 없습니다.";
    }

    private void FollowLoadedDeck()
    {
        if (_captions.Deck == null) return;
        _captions.Follow(SelectedSlide, SelectedKey);
        Status.Text = $"PPT 자막 따라가는 중 · {Path.GetFileName(_captions.Deck.Path)}";
        UpdateScore();
        Changed();
    }

    private void LiveChanged(object? sender, LiveSessionChangedEventArgs e) => Dispatcher.InvokeAsync(SyncScreenAsync);

    private void SourceChanged(object sender, RoutedEventArgs e)
    {
        if (DirectPanel == null || ScreenPanel == null) return; // during InitializeComponent
        bool screen = ScreenMode.IsChecked == true;
        DirectPanel.Visibility = screen ? Visibility.Collapsed : Visibility.Visible;
        ScreenPanel.Visibility = screen ? Visibility.Visible : Visibility.Collapsed;
        if (!IsLoaded) return;
        if (screen)
        {
            _captions.Mirror(true);
            _ = SyncScreenAsync();
        }
        else
        {
            _captions.SetImage(null);
            _captions.Publish(_editor.DraftText);
            Status.Text = "직접 입력한 자막을 송출합니다.";
            Draft.Focus();
        }
    }

    // A row of the current slide as rendered on screen (a staff with its lyrics), in place of or above the text.
    private int _scoreRow, _scoreRows = 2;

    // 자막만 / 악보만 / 악보 + 자막
    private bool ScoreOn => ScoreOnly.IsChecked == true || ScoreAndText.IsChecked == true;

    private void ScoreChanged(object sender, RoutedEventArgs e)
    {
        if (ScoreRowLabel == null) return; // during InitializeComponent
        _scoreRow = 0;
        UpdateScore();
    }
    private void SetScoreRow(int row) { _scoreRow = Math.Clamp(row, 0, Math.Max(0, _scoreRows - 1)); UpdateScore(); }

    private void SlideImageChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is not (nameof(PowerPointPreviewViewModel.PreviewImage) or nameof(PowerPointPreviewViewModel.SlideNumber))) return;
        if (ReferenceEquals(sender, _main.OutputPowerPoint)) _ = SyncScreenAsync();
        UpdateScore();
    }

    private void UpdateScore()
    {
        if (!ScoreOn || ScreenMode.IsChecked != true || !_captions.Following || _captions.Deck is not { } deck)
        {
            ScoreRowLabel.Text = _captions.Following && _captions.Deck != null ? $"PPT {_captions.CurrentSlide}/{_captions.Deck.Slides.Length}장"
                : _captions.Mirroring ? "송출 화면의 글자" : "";
            DeckLabel.Visibility = _captions.Following ? Visibility.Visible : Visibility.Collapsed; // the deck name only while it is followed
            _captions.SetImage(null);
            return;
        }
        static bool Same(string? a, string b) => a != null && string.Equals(Path.GetFullPath(a), b, StringComparison.OrdinalIgnoreCase);
        // A full-size render of the slide; the small operator picture of it stands in until that is ready.
        var image = SharpSlide(deck, _captions.CurrentSlide) ?? new[] { _main.OutputPowerPoint, _main.PowerPoint }.FirstOrDefault(p =>
            Same(p.LoadedContentPath, deck.Path) && p.SlideNumber == _captions.CurrentSlide && p.PreviewImage is BitmapSource { PixelHeight: > 1 })?.PreviewImage as BitmapSource;
        if (image == null)
        {
            Status.Text = "악보 그림을 만드는 중…";
            return;
        }
        if (CropRow(image, _scoreRow) is not var (cropped, rows, found))
        {
            Status.Text = "악보 그림을 만들 수 없습니다. 슬라이드를 다시 송출해 보세요.";
            return;
        }
        _scoreRows = rows;
        _scoreRow = Math.Min(_scoreRow, rows - 1);
        bool withText = ScoreAndText.IsChecked == true;
        // With the recognized lyrics as the caption, the row shown gets its own line of them.
        string? rowLyrics = withText && _captions.BoxKey == CaptionSession.RecognizedKey && !_captions.Overrides.ContainsKey(_captions.CurrentSlide)
            && _rowLyrics.TryGetValue((deck.Path, _captions.CurrentSlide), out var lyrics) && lyrics.Length == rows ? lyrics[_scoreRow] : null;
        _captions.SetImage(cropped, withText, rowLyrics);
        DeckLabel.Visibility = Visibility.Visible;
        ScoreRowLabel.Text = $"PPT {_captions.CurrentSlide}/{deck.Slides.Length}장 · " +
            (found ? $"{_scoreRow + 1}/{rows}줄" : $"{(_scoreRow == 0 ? "위" : "아래")} 절반 (악보 줄을 찾지 못함)");
        Status.Text = $"악보 {_captions.CurrentSlide}장 {_scoreRow + 1}/{rows}{(withText ? " + 자막" : "")}";
    }

    // Full-size slide pictures for the score (the operator's own are 640×360): rendered once, the next one ahead.
    private readonly Dictionary<(string Path, int Slide), BitmapSource> _sharp = [];
    private readonly HashSet<(string Path, int Slide)> _rendering = [];

    private BitmapSource? SharpSlide(CaptionDeck deck, int slide)
    {
        if (_render == null) return null;
        if (slide < deck.Slides.Length) _ = RenderSharpAsync(deck.Path, slide + 1);
        if (_sharp.TryGetValue((deck.Path, slide), out var image)) return image;
        _ = RenderSharpAsync(deck.Path, slide);
        return null;
    }

    private async Task RenderSharpAsync(string path, int slide)
    {
        if (_closed || _render == null || _sharp.ContainsKey((path, slide)) || !_rendering.Add((path, slide))) return;
        try
        {
            if (await RenderSlideAsync(path, slide) is not { } rendered || _closed) return;
            if (_captions.Deck?.Path == path && _captions.CurrentSlide == slide) UpdateScore();
        }
        catch (Exception error) when (error is OperationCanceledException or IOException or NotSupportedException or FileFormatException or ArgumentException)
        {
            System.Diagnostics.Debug.WriteLine(error.Message);
        }
        finally { _rendering.Remove((path, slide)); }
    }

    // A full-size picture of the slide, kept for the score; null when PowerPoint could not render it.
    private async Task<(BitmapSource Image, byte[] Bytes, string ContentType)?> RenderSlideAsync(string path, int slide)
    {
        var result = await _render!.RenderSlideAsync(new(path, slide, 1920, 1080, TimeSpan.FromSeconds(30)), _lifetime.Token);
        if (!result.Succeeded) return null;
        using var stream = new MemoryStream(result.Slide!.ImageBytes);
        var frame = BitmapDecoder.Create(stream, BitmapCreateOptions.PreservePixelFormat | BitmapCreateOptions.IgnoreColorProfile,
            BitmapCacheOption.OnLoad).Frames[0];
        frame.Freeze();
        if (_sharp.Count >= 40) _sharp.Clear();
        _sharp[(path, slide)] = frame;
        return (frame, result.Slide.ImageBytes, result.Slide.ContentType);
    }

    // The lyrics of each score row, per slide, read from the slide pictures.
    private readonly Dictionary<(string Path, int Slide), string[]> _rowLyrics = [];

    // After a deck is read: the lyrics inside its pictures are read in the background (OCR helper) and offered as a box.
    private async Task RecognizeDeckAsync(CaptionDeck deck)
    {
        var slides = Enumerable.Range(1, deck.Slides.Length).Where(n => deck.Slides[n - 1].HasPicture != false).ToArray();
        if (_render == null || slides.Length == 0) return;
        if (!LyricsReader.Available) { Status.Text = "그림 속 가사 읽기 도우미(ocr 폴더)가 없어 가사를 읽지 못했습니다."; return; }
        var folder = Path.Combine(Path.GetTempPath(), "Axicultus", "ocr", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        try
        {
            var files = new List<string>();
            var layouts = new Dictionary<string, (int Slide, ScoreRow[] Rows)>(StringComparer.OrdinalIgnoreCase);
            foreach (var slide in slides)
            {
                if (_closed || !ReferenceEquals(_captions.Deck, deck)) return;
                DeckLabel.Text = $"{Path.GetFileName(deck.Path)} · 그림 속 가사 읽는 중 {slide}/{deck.Slides.Length}";
                if (await RenderSlideAsync(deck.Path, slide) is not var (image, bytes, type)) continue;
                var file = Path.Combine(folder, $"{slide}{(type.Contains("jp", StringComparison.OrdinalIgnoreCase) ? ".jpg" : ".png")}");
                await File.WriteAllBytesAsync(file, bytes, _lifetime.Token);
                files.Add(file);
                layouts[file] = (slide, ScoreLayout.FindRows(image));
            }
            if (files.Count == 0 || _closed || !ReferenceEquals(_captions.Deck, deck)) return;
            var pages = await LyricsReader.ReadAsync(files, _lifetime.Token);
            if (_closed || !ReferenceEquals(_captions.Deck, deck)) return;
            int found = 0;
            foreach (var page in pages)
            {
                if (page.Error != null || !layouts.TryGetValue(page.File, out var layout)) continue;
                var rows = LyricsReader.RowLyrics(page, layout.Rows);
                _rowLyrics[(deck.Path, layout.Slide)] = rows;
                var lyrics = string.Join("\n", rows.Where(row => row.Length > 0));
                if (lyrics.Length > 0) found++;
                _captions.AddRecognized(layout.Slide, lyrics);
            }
            for (int n = 1; n <= deck.Slides.Length; n++)
                if (deck.Slides[n - 1].Boxes.All(box => box.Key != CaptionSession.RecognizedKey)) _captions.AddRecognized(n, "");
            DeckLabel.Text = Path.GetFileName(deck.Path) + (found > 0 ? $" · 그림 속 가사 {found}장 읽음" : "");
            if (found > 0) OfferRecognizedLyrics(deck);
        }
        catch (Exception error) when (error is IOException or System.ComponentModel.Win32Exception or System.Text.Json.JsonException
            or InvalidOperationException or UnauthorizedAccessException or OperationCanceledException or TimeoutException)
        {
            if (_closed) return;
            _errors.Write("그림 속 가사 읽기", error);
            DeckLabel.Text = Path.GetFileName(deck.Path);
            Status.Text = "그림 속 가사를 읽지 못했습니다 · " + error.Message;
        }
        finally
        {
            try { Directory.Delete(folder, true); }
            catch (Exception error) when (error is IOException or UnauthorizedAccessException) { System.Diagnostics.Debug.WriteLine(error.Message); }
        }
    }

    // The recognized lyrics become the caption when the chosen box says the same on every slide
    // (a verse button, a title) or there is none; a box with real per-slide text stays.
    private void OfferRecognizedLyrics(CaptionDeck deck)
    {
        var key = SelectedKey;
        bool sameEverywhere = key != CaptionSession.RecognizedKey &&
            deck.Slides.Select(s => s.Boxes.FirstOrDefault(b => b.Key == key)?.Text ?? "").Distinct().Count() <= 1;
        RefreshBoxes(sameEverywhere ? CaptionSession.RecognizedKey : key);
        if (sameEverywhere) Status.Text = "그림 속 가사를 자막으로 씁니다. 텍스트 상자에서 바꿀 수 있습니다.";
    }

    private void RefreshBoxes(string key)
    {
        if (_captions.Deck is not { } deck || SelectedSlide > deck.Slides.Length) return;
        var boxes = deck.Slides[SelectedSlide - 1].Boxes;
        Boxes.ItemsSource = boxes;
        Boxes.SelectedItem = boxes.FirstOrDefault(b => b.Key == key) ?? boxes.FirstOrDefault();
        RefreshCue();
    }

    // The rows of the last slide picture (a picture is never changed once rendered).
    private static BitmapSource? _rowsOf;
    private static ScoreRow[] _rows = [];

    /// <summary>
    /// The row-th row of the slide (the staves found on it), or its top or bottom half when none is found.
    /// A slide picture may belong to a render thread; the work runs there and the result is a frozen copy.
    /// </summary>
    internal static (BitmapSource Image, int Rows, bool Found)? CropRow(BitmapSource image, int row)
    {
        (BitmapSource, int, bool) Make()
        {
            if (!ReferenceEquals(image, _rowsOf)) { _rows = ScoreLayout.FindRows(image); _rowsOf = image; }
            bool found = _rows.Length > 0;
            int half = image.PixelHeight / 2;
            Int32Rect[] rows = found ? [.. _rows.Select(r => r.Area)] :
                [new(0, 0, image.PixelWidth, half), new(0, image.PixelHeight - half, image.PixelWidth, half)];
            var copy = new WriteableBitmap(new CroppedBitmap(image, rows[Math.Clamp(row, 0, rows.Length - 1)]));
            copy.Freeze();
            return (copy, rows.Length, found);
        }
        try
        {
            return image.Dispatcher is not { } owner || owner.CheckAccess() ? Make()
                : owner.Invoke(Make, DispatcherPriority.Send, CancellationToken.None, TimeSpan.FromSeconds(2));
        }
        catch (Exception error) when (error is InvalidOperationException or TimeoutException or TaskCanceledException)
        {
            System.Diagnostics.Debug.WriteLine(error.Message);
            return null;
        }
    }

    private void PickColor(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { Tag: string color }) _captions.SetStyle(_captions.Style with { Color = color });
    }

    private void PickCustomColor(object sender, RoutedEventArgs e)
    {
        var current = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(_captions.Style.Color);
        using var dialog = new System.Windows.Forms.ColorDialog { Color = System.Drawing.Color.FromArgb(current.R, current.G, current.B), FullOpen = true };
        if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            _captions.SetStyle(_captions.Style with { Color = $"#{dialog.Color.R:X2}{dialog.Color.G:X2}{dialog.Color.B:X2}" });
    }

    private void BoxChanged(object sender, RoutedEventArgs e) => _captions.SetStyle(_captions.Style with { Box = BoxCheck.IsChecked == true });

    private void CopyBrowserUrl(object sender, RoutedEventArgs e)
    {
        try { Clipboard.SetText(CaptionBroadcastServer.Url); Status.Text = "브라우저 소스 주소를 복사했습니다."; }
        catch (System.Runtime.InteropServices.ExternalException ex) { Status.Text = "복사 실패 · " + ex.Message; }
    }

    // Opening the console starts the caption server and connects OBS with the saved settings.
    private async void StartAutomatically(object sender, RoutedEventArgs e)
    {
        Loaded -= StartAutomatically;
        await StartServerAsync();
        if (_obs.Connection == null && (_obs.HasSavedPassword || _obs.AutoConnect || File.Exists(_obs.ProfilePath))) await ConnectAsync();
        else if (_obs.Connection == null) SettingsToggle.IsChecked = true;
    }

    private async Task StartServerAsync()
    {
        try { await _server.StartAsync(); ServerLabel.Text = "● 자막 서버 실행 중"; }
        catch (Exception ex) { ServerLabel.Text = "자막 서버 시작 실패 · " + ex.GetBaseException().Message; }
    }

    // The live deck first; then the deck in the output panel (show not live) and, if asked, the selected one.
    private (string Path, int Slide)? FindCurrentDeck(bool includeSelected)
    {
        static bool IsDeck(LiveQueueItem? item) => item is { ContentPath.Length: > 0 } &&
            LiveItemKindMatcher.IsPowerPoint(LiveItemKindMatcher.InferLegacyKind(item));
        var live = _main.Session.Current;
        if (live.PptPath is { Length: > 0 } path) return (path, live.PptSlideNumber);
        // Not live: the deck in the output panel is what the output screen holds.
        if (live.State == LiveState.Off && IsDeck(_main.OutputItem)) return (_main.OutputItem!.ContentPath!, Math.Max(1, _main.OutputPowerPoint.SlideNumber));
        if (includeSelected && IsDeck(_main.SelectedItem)) return (_main.SelectedItem!.ContentPath!, Math.Max(1, _main.PowerPoint.SlideNumber));
        return null;
    }

    internal async Task<bool> PrepareDeckAsync(string path, int slide)
    {
        if (_editor.IsRestoring) return false;
        long request = ++_deckRequest;
        CaptionDeck deck;
        try
        {
            deck = await ReadDeckAsync(path);
        }
        catch (OperationCanceledException) when (_lifetime.IsCancellationRequested) { return false; }
        catch (Exception ex) when (IsDeckReadError(ex))
        {
            _errors.Write("자막 PPT 열기", ex);
            if (!_closed && request == _deckRequest) Status.Text = "PPT 읽기 실패 · " + ex.Message;
            return false;
        }
        if (_closed || request != _deckRequest) return false;
        var key = _captions.BoxKey;
        _captions.Prepare(deck);
        ShowDeck(deck, slide, key);
        Status.Text = $"PPT를 읽었습니다 · {Path.GetFileName(deck.Path)}";
        Changed();
        _ = RecognizeDeckAsync(deck);
        return true;
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
        // While another slide's boxes replace the list, nothing is selected for a moment: not a choice of "no box".
        if (!IsLoaded || Boxes.SelectedItem == null) return;
        if (_captions.Following && SelectedKey != _captions.BoxKey) _captions.Follow(_captions.CurrentSlide, SelectedKey);
        Changed();
    }
    private void RefreshCue() => Cue.Text = _captions.TextFor(SelectedSlide, SelectedKey);
    private void SaveOverride(object sender, RoutedEventArgs e)
    {
        if (_captions.Deck == null) return;
        _captions.Overrides[SelectedSlide] = Cue.Text;
        Status.Text = $"{SelectedSlide}장 수동 문구를 저장했습니다.";
        Changed();
    }

    private void Previous(object sender, RoutedEventArgs e) => Step(-1);
    private void Next(object sender, RoutedEventArgs e) => Step(1);
    private async void Step(int change)
    {
        // A score goes row by row, then to the next slide's first row.
        int next = _scoreRow + change;
        if (_captions.Following && ScoreOn && next >= 0 && next < _scoreRows) { SetScoreRow(next); return; }
        if (_captions.SyncShow)
        {
            // Sync: the output screen turns (a slide, a page of a song or passage) and the caption follows it.
            var turn = change > 0 ? _main.NextOutputSlideCommand : _main.PreviousOutputSlideCommand;
            if (!turn.CanExecute(null)) { Status.Text = change > 0 ? "송출 화면에 다음 장이 없습니다." : "송출 화면에 이전 장이 없습니다."; return; }
            await turn.ExecuteAsync(null);
            return;
        }
        if (!_captions.Following) { Status.Text = "송출 화면을 같이 넘기려면 싱크를 켜세요. 끄면 PPT 자막만 넘깁니다."; return; }
        _captions.MoveToSlide(_captions.CurrentSlide + change);
        Slides.SelectedItem = _captions.CurrentSlide;
        Changed();
    }

    private async void ConnectObs(object sender, RoutedEventArgs e)
    {
        await ConnectAsync();
        SettingsToggle.IsChecked = _obs.Connection == null;
    }

    private async Task ConnectAsync()
    {
        if (!int.TryParse(Port.Text, out int port) || port is < 1 or > 65535) { ObsLabel.Text = "OBS 포트를 확인하세요."; return; }
        try
        {
            await _obs.ConfigureAsync(Host.Text, port, Password.Password, AutoObs.IsChecked == true,
                [Scene1.Text, Scene2.Text, Scene3.Text, Scene4.Text], CameraSource.Text);
            Password.Clear();
            SavedPassword.Visibility = _obs.HasSavedPassword ? Visibility.Visible : Visibility.Collapsed;
            await StartServerAsync();
        }
        catch (Exception ex) { Status.Text = ex.Message; }
    }

    private void ShowObsStatus(ObsStatus status)
    {
        ObsLabel.Text = (status.Connected ? "● OBS " : "OBS ") + status.Message + (status.Scene.Length > 0 ? " · " + status.Scene : "");
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
        _main.Session.SessionChanged -= LiveChanged;
        _main.OutputPowerPoint.PropertyChanged -= SlideImageChanged;
        _main.PowerPoint.PropertyChanged -= SlideImageChanged;
        // The last write runs on a pool thread. Finish before the process can exit.
        _obs.Changed -= ShowObsStatus;
        await SaveAsync();
    }
}
