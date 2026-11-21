using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using NetOffice.OfficeApi.Enums;
using NetOffice.PowerPointApi;
using NetOffice.PowerPointApi.Enums;

namespace Axicultus.Wpf.Interop;

public sealed record OfficePptSlideExport(
    int SlideCount,
    byte[] ImageBytes,
    string ContentType)
{
    public int PixelWidth { get; init; }
    public int PixelHeight { get; init; }
}

/// <summary>
/// PowerPoint COM 세션 래퍼 — 계획서 §10.3 STA 스레드 모델 리스크 대응.
///
/// 핵심 규칙 (ADR-0001 / §10.3):
///   1. COM 객체는 생성 STA 스레드에서만 호출·소멸 (크로스-스레드 호출 금지)
///   2. Task.Run 결과는 Dispatcher.Invoke로 UI에 넘김
///   3. 본 래퍼는 자체 STA 워커 스레드를 보유하여 COM 어피니티 보장
///
/// Sprint 0 PoC-B 검증: 100회 순회 후 좀비 POWERPNT.EXE 0 확인.
/// </summary>
public sealed class OfficePptSession : IDisposable
{
    private readonly Thread _staThread;
    private readonly System.Collections.Concurrent.BlockingCollection<Action> _workQueue;
    private readonly CancellationTokenSource _cts;
    private Application? _ppt;
    private bool _disposed;
    internal sealed record OutputShow(string Path, nint Handle, Shell.OutputWindowPlacement? Bounds, bool Topmost);
    private volatile OutputShow? _outputShow;
    internal OutputShow? CurrentOutput => _outputShow;
    private readonly System.Collections.Generic.HashSet<string> _controlledShows = [with(StringComparer.OrdinalIgnoreCase)];
    private readonly System.Collections.Generic.HashSet<string> _openedDecks = [with(StringComparer.OrdinalIgnoreCase)];

    private readonly System.Collections.Generic.Dictionary<IntPtr, PpWindowState> _editorStates = [];

    public OfficePptSession()
    {
        _workQueue = [];
        _cts = new CancellationTokenSource();
        _staThread = new Thread(WorkerLoop)
        {
            IsBackground = true,
            Name = "EsOfficePptStaWorker",
        };
        _staThread.SetApartmentState(ApartmentState.STA);
        _staThread.Start();
    }

    private void WorkerLoop()
    {
        try
        {
            foreach (var work in _workQueue.GetConsumingEnumerable(_cts.Token))
            {
                work();
            }
        }
        catch (OperationCanceledException) { /* 정상 종료 */ }
    }

    /// <summary>STA 워커 스레드에서 동작을 실행하고 결과를 비동기 반환.</summary>
    private Task<T> RunOnStaAsync<T>(Func<T> work)
    {
        var tcs = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);
        _workQueue.Add(() =>
        {
            try { tcs.SetResult(work()); }
            catch (Exception ex) { tcs.SetException(ex); }
        });
        return tcs.Task;
    }

    /// <summary>
    /// PowerPoint Application 인스턴스를 STA 스레드에서 생성한다.
    /// 생성 직후 Presentations.Count로 워밍업 — OfficeLib/PowerPoint.cs:82 검증된 패턴.
    /// NetOffice의 일부 속성(예: Version)은 워밍업 전 접근 시 throw 가능.
    /// </summary>
    public Task OpenAsync() => RunOnStaAsync(() =>
    {
        _ppt ??= new Application();
        _ = _ppt.Presentations.Count; // 워밍업 (OfficeLib 패턴)
        return true;
    });

    /// <summary>
    /// 가벼운 COM 호출 — Presentations.Count 조회.
    /// STA 어피니티 + COM 마샬링 검증용. 크로스-스레드 호출 시 즉시 COMException.
    /// OfficeLib 헬스 체크 패턴(PowerPoint.cs:82) 재사용.
    /// </summary>
    public Task<int> PingAsync() => RunOnStaAsync(() =>
    {
        if (_ppt is null) throw new InvalidOperationException("PowerPoint 세션이 열려 있지 않음. OpenAsync 먼저 호출하세요.");
        return _ppt.Presentations.Count;
    });

    internal static (int Width, int Height) FitSlideSize(double slideWidth, double slideHeight, int width, int height)
    {
        if (!double.IsFinite(slideWidth) || !double.IsFinite(slideHeight) || slideWidth <= 0 || slideHeight <= 0)
            return (width, height);
        double factor = Math.Min(width / slideWidth, height / slideHeight);
        return (Math.Max(1, (int)Math.Round(slideWidth * factor)), Math.Max(1, (int)Math.Round(slideHeight * factor)));
    }

    internal Task<Captions.CaptionDeck> ReadCaptionDeckAsync(string path) => RunOnStaAsync(() =>
    {
        _ppt ??= new Application();
        using var presentations = _ppt.Presentations;
        using var deck = OpenForConversion(_ppt, presentations, path, out bool ownsCopy);
        try
        {
            using var slides = deck.Slides;
            var result = new Captions.CaptionSlide[slides.Count];
            for (int i = 1; i <= slides.Count; i++)
            {
                using var slide = slides[i];
                using var shapes = slide.Shapes;
                var boxes = new System.Collections.Generic.List<Captions.CaptionBox>();
                for (int j = 1; j <= shapes.Count; j++)
                {
                    using var shape = shapes[j];
                    if (shape.HasTextFrame != MsoTriState.msoTrue) continue;
                    using var frame = shape.TextFrame;
                    if (frame.HasText != MsoTriState.msoTrue) continue;
                    using var range = frame.TextRange;
                    var text = range.Text.Replace('\r', '\n').Replace('\v', '\n').Trim();
                    if (text.Length > 0) boxes.Add(new("shape:" + shape.Name, shape.Name, text));
                }
                result[i-1] = new([.. boxes]);
            }
            return new Captions.CaptionDeck(Path.GetFullPath(path), result);
        }
        finally { if (ownsCopy) deck.Close(); }
    });

    public Task<OfficePptSlideExport> ExportSlideAsync(string filePath, int slideNumber, int width, int height)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        ArgumentOutOfRangeException.ThrowIfLessThan(slideNumber, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(width, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(height, 1);

        return RunOnStaAsync(() =>
        {
            _ppt ??= new Application();
            using var presentations = _ppt.Presentations;

            _Presentation? presentation = null;
            bool ownsCopy = false;
            var tempFile = Path.Combine(Path.GetTempPath(), $"AxicultusPpt_{Guid.NewGuid():N}.png");

            try
            {
                presentation = OpenForConversion(_ppt, presentations, filePath, out ownsCopy);
                using var slides = presentation.Slides;
                var slideCount = slides.Count;
                if (slideNumber > slideCount)
                {
                    throw new ArgumentOutOfRangeException(nameof(slideNumber), slideNumber, $"Slide number exceeds slide count {slideCount}.");
                }

                using var page = presentation.PageSetup;
                var (Width, Height) = FitSlideSize(page.SlideWidth, page.SlideHeight, width, height);
                using var slide = slides[slideNumber];
                slide.Export(tempFile, "PNG", Width, Height);
                return new OfficePptSlideExport(slideCount, File.ReadAllBytes(tempFile), "image/png")
                {
                    PixelWidth = Width,
                    PixelHeight = Height
                };
            }
            finally
            {
                if (presentation is not null)
                {
                    try { if (ownsCopy) presentation.Close(); }
                    catch { }

                    try { presentation.Dispose(); }
                    catch { }
                }

                try { File.Delete(tempFile); }
                catch { }
            }
        });
    }

    // Office can return the already open document even when Untitled is requested.
    private static Presentation OpenForConversion(Application application, Presentations presentations, string path, out bool ownsCopy)
    {
        if (FindPresentation(presentations, path) is { } existing)
        {
            ownsCopy = false;
            return existing;
        }
        var previous = application.AutomationSecurity;
        try
        {
            application.AutomationSecurity = MsoAutomationSecurity.msoAutomationSecurityForceDisable;
            var presentation = presentations.Open(path, MsoTriState.msoTrue, MsoTriState.msoTrue, MsoTriState.msoFalse);
            ownsCopy = string.IsNullOrEmpty(presentation.Path);
            return presentation;
        }
        finally { application.AutomationSecurity = previous; }
    }

    public Task<int?> ReadShowPositionAsync(string path) => RunOnStaAsync<int?>(() =>
    {
        if (_ppt == null || !_controlledShows.Contains(path)) return null;
        using var windows = _ppt.SlideShowWindows;
        using var window = FindShowWindow(windows, path);
        if (window is null) return null;
        using var view = window.View;
        if (view.State != PpSlideShowState.ppSlideShowRunning) return null;
        using var slide = view.Slide;
        return slide.SlideIndex;
    });

    internal Task SetBlackoutAsync(string path, bool black) => RunOnStaAsync(() =>
    {
        if (_ppt == null || !_controlledShows.Contains(path)) return false;
        using var windows = _ppt.SlideShowWindows;
        using var window = FindShowWindow(windows, path);
        using var view = window?.View;
        if (view != null && (black || view.State == PpSlideShowState.ppSlideShowBlackScreen))
            view.State = black ? PpSlideShowState.ppSlideShowBlackScreen : PpSlideShowState.ppSlideShowRunning;
        return true;
    });

    internal Task<int?> AdvanceShowAsync(string path) => RunOnStaAsync<int?>(() =>
    {
        if (_ppt == null || !_controlledShows.Contains(path)) return null;
        using var windows = _ppt.SlideShowWindows;
        using var window = FindShowWindow(windows, path);
        if (window is null) return null;
        using var view = window.View;
        if (view.State == PpSlideShowState.ppSlideShowDone) return null;
        view.Next();
        if (view.State == PpSlideShowState.ppSlideShowDone) return null;
        using var slide = view.Slide;
        return slide.SlideIndex;
    });

    /// <summary>Start a PowerPoint slide show on the configured output monitor.</summary>
    public Task<bool> StartSlideShowAsync(
        string filePath,
        int slideNumber,
        string outputMonitorName,
        CancellationToken cancellationToken = default, bool isOutput = true, Shell.OutputWindowPlacement? bounds = null, bool topmost = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        ArgumentOutOfRangeException.ThrowIfLessThan(slideNumber, 1);

        var fullPath = Path.GetFullPath(filePath);
        return RunOnStaAsync(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();

            _ppt ??= new Application();
            using var presentations = _ppt.Presentations;

            if (isOutput && bounds == null && !string.IsNullOrWhiteSpace(outputMonitorName))
            {
                var screen = Array.Find(System.Windows.Forms.Screen.AllScreens,
                    item => string.Equals(item.DeviceName, outputMonitorName, StringComparison.OrdinalIgnoreCase));
                if (screen != null)
                {
                    var area = screen.Bounds;
                    bounds = new Shell.OutputWindowPlacement(area.X, area.Y, area.Width, area.Height, false);
                }
            }
            if (isOutput) ClosePreviousOutput(presentations, fullPath);
            using var presentation = FindOrOpenPresentation(presentations, fullPath);
            if (slideNumber > presentation.Slides.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(slideNumber), slideNumber, $"Slide number exceeds slide count {presentation.Slides.Count}.");
            }

            ConfigureSlideShowForLive(presentation, 1);
            using var showWindows = _ppt.SlideShowWindows;
            using var slideShowWindow = FindShowWindow(showWindows, fullPath)
                ?? presentation.SlideShowSettings.Run();
            if (slideShowWindow is null)
            {
                return false;
            }

            using var view = slideShowWindow.View;
            if (slideNumber < 2)
            {
                view.First();
            }
            else if (view.Slide.SlideIndex != slideNumber)
            {
                view.GotoSlide(slideNumber, MsoTriState.msoFalse);
            }

            ActivateShow(presentation, slideShowWindow, fullPath, isOutput, bounds, topmost);
            return true;
        });
    }

    public Task<bool> TriggerSlideShowNextAsync(
        string filePath,
        int slideNumber,
        CancellationToken cancellationToken = default, bool isOutput = true)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        ArgumentOutOfRangeException.ThrowIfLessThan(slideNumber, 1);

        var fullPath = Path.GetFullPath(filePath);
        return RunOnStaAsync(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();

            _ppt ??= new Application();
            using var presentations = _ppt.Presentations;

            if (isOutput) ClosePreviousOutput(presentations, fullPath);
            using var presentation = FindOrOpenPresentation(presentations, fullPath);
            if (slideNumber > presentation.Slides.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(slideNumber), slideNumber, $"Slide number exceeds slide count {presentation.Slides.Count}.");
            }

            ConfigureSlideShowForLive(presentation, slideNumber);
            using var showWindows = _ppt.SlideShowWindows;
            using var slideShowWindow = FindShowWindow(showWindows, fullPath)
                ?? presentation.SlideShowSettings.Run();
            if (slideShowWindow is null)
            {
                return false;
            }

            using var view = slideShowWindow.View;
            if (view.Slide.SlideIndex != slideNumber)
            {
                view.GotoSlide(slideNumber, MsoTriState.msoTrue);
            }

            ActivateShow(presentation, slideShowWindow, fullPath, isOutput);
            var totalClicks = view.Slide.TimeLine.MainSequence.Count;
            var currentIndex = view.GetClickIndex();
            if (currentIndex < totalClicks)
            {
                view.Next();
            }
            else
            {
                view.GotoClick(0);
                view.Next();
            }

            return true;
        });
    }

    private Presentation FindOrOpenPresentation(Presentations presentations, string fullPath)
    {
        if (FindPresentation(presentations, fullPath) is { } existing) return existing;

        var opened = presentations.Open(
            fullPath, MsoTriState.msoTrue, MsoTriState.msoFalse, MsoTriState.msoFalse);
        _openedDecks.Add(fullPath);
        return opened;
    }

    private static Presentation? FindPresentation(Presentations presentations, string path)
    {
        for (int i = 1; i <= presentations.Count; i++)
        {
            var deck = presentations[i];
            if (string.Equals(deck.FullName, path, StringComparison.OrdinalIgnoreCase)) return deck;
            deck.Dispose();
        }
        return null;
    }

    private static SlideShowWindow? FindShowWindow(SlideShowWindows windows, string path)
    {
        for (int i = 1; i <= windows.Count; i++)
        {
            var window = windows[i];
            using var deck = window.Presentation;
            if (string.Equals(deck.FullName, path, StringComparison.OrdinalIgnoreCase)) return window;
            window.Dispose();
        }
        return null;
    }

    private void ActivateShow(Presentation deck, SlideShowWindow window, string path, bool isOutput,
        Shell.OutputWindowPlacement? bounds = null, bool topmost = false)
    {
        _controlledShows.Add(path);
        MinimizeEditors(deck);
        window.Activate();
        if (!isOutput) return;
        var handle = OfficeLib.PowerPoint.FindSlideShowWindow(window, () => deck.Name, out _);
        // PowerPoint can recreate the window when the operator triggers the same slide again.
        if (bounds == null && _outputShow is { } previous && string.Equals(previous.Path, path, StringComparison.OrdinalIgnoreCase))
        {
            if (handle == previous.Handle) return;
            bounds = previous.Bounds;
            topmost = previous.Topmost;
        }
        _outputShow = new(path, handle, bounds, topmost);
        if (handle != 0 && bounds != null)
        {
            Platform.DesktopWindowPlacement.Apply(handle, bounds);
            Platform.DesktopWindowPlacement.SetTopmost(handle, topmost);
        }
    }

    private void MinimizeEditors(Presentation deck)
    {
        using var windows = deck.Windows;
        for (int i = 1; i <= windows.Count; i++)
        {
            using var editor = windows[i];
            var state = editor.WindowState;
            if (state == PpWindowState.ppWindowMinimized) continue;
            // Some Office versions do not expose DocumentWindow.HWND through automation.
            var identity = Marshal.GetIUnknownForObject(editor.UnderlyingObject);
            try
            {
                editor.WindowState = PpWindowState.ppWindowMinimized;
                if (_editorStates.TryAdd(identity, state)) identity = IntPtr.Zero;
            }
            finally { if (identity != IntPtr.Zero) Marshal.Release(identity); }
        }
    }

    private void RestoreEditors(Presentation deck)
    {
        if (_editorStates.Count == 0) return;
        using var windows = deck.Windows;
        for (int i = 1; i <= windows.Count; i++)
        {
            using var editor = windows[i];
            var identity = Marshal.GetIUnknownForObject(editor.UnderlyingObject);
            try
            {
                if (_editorStates.Remove(identity, out var state))
                {
                    Marshal.Release(identity); // release the reference retained when minimizing
                    if (editor.WindowState == PpWindowState.ppWindowMinimized) editor.WindowState = state;
                }
            }
            finally { Marshal.Release(identity); }
        }
    }

    private static void ConfigureSlideShowForLive(_Presentation presentation, int startingSlide)
    {
        var slideCount = Math.Max(presentation.Slides.Count, 1);
        var start = Math.Clamp(startingSlide, 1, slideCount);
        var settings = presentation.SlideShowSettings;
        settings.ShowPresenterView = MsoTriState.msoFalse;
        settings.RangeType = PpSlideShowRangeType.ppShowSlideRange;
        settings.StartingSlide = start;
        settings.EndingSlide = slideCount;
        settings.ShowType = PpSlideShowType.ppShowTypeSpeaker;
        settings.AdvanceMode = PpSlideShowAdvanceMode.ppSlideShowUseSlideTimings;
    }

    private void ClosePreviousOutput(Presentations decks, string nextPath)
    {
        if (_outputShow is not { } previous || string.Equals(previous.Path, nextPath, StringComparison.OrdinalIgnoreCase)) return;
        using var deck = FindPresentation(decks, previous.Path);
        if (deck != null) CloseDeck(deck);
        _outputShow = null;
    }

    private void CloseDeck(Presentation deck)
    {
        var path = deck.FullName;
        if (_controlledShows.Remove(path))
        {
            try
            {
                using var windows = _ppt!.SlideShowWindows;
                using var window = FindShowWindow(windows, path);
                using var view = window?.View;
                view?.Exit();
            }
            finally { RestoreEditors(deck); }
        }
        if (_openedDecks.Remove(path) && deck.Saved == MsoTriState.msoTrue) deck.Close();
    }

    public Task CloseAsync() => RunOnStaAsync(() =>
    {
        _outputShow = null;
        if (_ppt is not null)
        {
            try
            {
                using var decks = _ppt.Presentations;
                for (int i = decks.Count; i > 0; i--)
                {
                    using var deck = decks[i];
                    CloseDeck(deck);
                }
                if (decks.Count == 0) _ppt.Quit();
            }
            catch { /* COM 종료 시 발생 가능, 무시 */ }

            try { _ppt.Dispose(); }
            catch { }

            try { Marshal.FinalReleaseComObject(_ppt); }
            catch { }

            foreach (var identity in _editorStates.Keys) Marshal.Release(identity);
            _editorStates.Clear();
            _controlledShows.Clear();
            _openedDecks.Clear();
            _ppt = null;
            // 명시적 GC — COM 참조 카운트 즉시 해제 유도
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }
        return true;
    });

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        // 종료 전 세션 닫기
        try { CloseAsync().GetAwaiter().GetResult(); } catch { }

        _cts.Cancel();
        _workQueue.CompleteAdding();
        _staThread.Join(TimeSpan.FromSeconds(5));
        _cts.Dispose();
        _workQueue.Dispose();
    }
}
