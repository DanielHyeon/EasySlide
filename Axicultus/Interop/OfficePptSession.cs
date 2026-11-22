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
public sealed partial class OfficePptSession : IDisposable
{
    private readonly Thread _staThread;
    private readonly System.Collections.Concurrent.BlockingCollection<Action> _workQueue;
    private readonly CancellationTokenSource _cts;
    private Application? _ppt;
    private bool _disposed;
    internal sealed record OutputShow(string Path, nint Handle, Shell.OutputWindowPlacement? Bounds, bool Topmost);
    private volatile OutputShow? _outputShow;
    internal OutputShow? CurrentOutput => _outputShow;
    private readonly System.Collections.Generic.Dictionary<string, ShowDeck> _showDecks = new(StringComparer.OrdinalIgnoreCase);

    private readonly System.Collections.Generic.Dictionary<IntPtr, PpWindowState> _editorStates = [];

    // Call limits, as OfficeLib.PowerPointGate: a PowerPoint that stopped answering must not hang End Show or app exit.
    private const int LightLimitMs = OfficeLib.PowerPointGate.LightStallMs; // slide moves, state lookups
    private const int HeavyLimitMs = OfficeLib.PowerPointGate.HeavyStallMs; // opening a file, running a show
    private const int StartLimitMs = OfficeLib.PowerPointGate.StartStallMs; // may have to start PowerPoint first
    private const int CloseLimitMs = 2 * LightLimitMs;
    // The most a relative move (a slide step, a replay, a position read) waits for its turn behind earlier calls.
    private const int StepQueueMs = 2000;
    // Time for a call cancelled at its limit to fail by itself before the caller stops waiting.
    private const int GraceMs = 1000;
    private const string NotAnswering = "PowerPoint did not answer.";
    private const string Busy = "PowerPoint is still busy with an earlier request.";

    private const int RPC_S_SERVER_UNAVAILABLE = unchecked((int)0x800706BA);
    private const int RPC_S_CALL_FAILED = unchecked((int)0x800706BE);
    private const int RPC_E_DISCONNECTED = unchecked((int)0x80010108);
    private const int RPC_E_SERVER_DIED = unchecked((int)0x80010007);
    private const int RPC_E_SERVER_DIED_DNE = unchecked((int)0x80010012);
    private const int RPC_E_CALL_REJECTED = unchecked((int)0x80010001);
    private const int RPC_E_SERVERCALL_RETRYLATER = unchecked((int)0x8001010A);

    internal enum PptCall { Step, Read, Poll, Trigger, Blackout, Start, Close }

    /// <summary>
    /// LimitMs: the most the calls may take once they started. QueueMs: the most the caller waits for them to
    /// start. RunLate: still carried out, in order, after the caller stopped waiting for the start.
    /// </summary>
    internal readonly record struct CallPolicy(int LimitMs, int QueueMs, bool RunLate);

    /// <summary>
    /// A relative move that could not start in time is dropped: a slide change that arrives seconds late is
    /// worse than none. What sets a state (start a show, black it, close) is carried out when its turn comes,
    /// so the last request wins.
    /// </summary>
    internal static CallPolicy PolicyFor(PptCall call) => call switch
    {
        PptCall.Start => new(StartLimitMs, HeavyLimitMs, RunLate: true),
        PptCall.Close => new(CloseLimitMs, CloseLimitMs, RunLate: true),
        PptCall.Blackout => new(LightLimitMs, StepQueueMs, RunLate: true),
        PptCall.Poll or PptCall.Trigger => new(HeavyLimitMs, StepQueueMs, RunLate: false),
        _ => new(LightLimitMs, StepQueueMs, RunLate: false),
    };

    private readonly Func<Application> _createApplication;
    private readonly int? _limitMs, _queueMs;
    // Test seam: raised on the worker when a limited call starts (a call dropped in the queue never does).
    internal Action<PptCall>? CallStarted { get; set; }

    // Left by a call that was given up or refused, put right when PowerPoint answers again (SweepLeftovers):
    // copies PowerPoint could not close, disk copies whose cancelled Open it may still carry out, and the macro
    // security setting an abandoned open could not restore.
    private readonly System.Collections.Generic.List<ShowDeck> _unclosed = [];
    private readonly System.Collections.Generic.List<string> _abandonedOpens = [];
    private MsoAutomationSecurity? _securityToRestore;

    public OfficePptSession() : this(static () => new Application())
    {
    }

    // Test seam: how PowerPoint is started, and one value for every call's limit or queue wait.
    internal OfficePptSession(Func<Application> createApplication, int? limitMs = null, int? queueMs = null)
    {
        _createApplication = createApplication;
        _limitMs = limitMs;
        _queueMs = queueMs;
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
    /// Runs a group of PowerPoint calls with a time limit, so a PowerPoint that stopped answering (a modal
    /// dialog, a hang) cannot block the caller. OfficeLib's message filter cancels the call that waits
    /// (RPC_E_CALL_CANCELED), so the group fails instead of finishing late, and the caller is released at the limit
    /// even while the worker still waits (releasing a proxy is not cancelled).
    /// The limit counts from the moment the calls start: time queued behind an earlier, healthy call is not
    /// PowerPoint failing to answer this one. Waiting for the start has its own bound (CallPolicy.QueueMs), and
    /// one atomic decision says whether a call started or was dropped in the queue, never both.
    /// </summary>
    private async Task<T> RunLimitedAsync<T>(Func<T> work, PptCall kind)
    {
        var policy = PolicyFor(kind);
        int limitMs = _limitMs ?? policy.LimitMs;
        // 0 queued, 1 started, 2 its caller stopped waiting for the start.
        var state = new System.Runtime.CompilerServices.StrongBox<int>();
        var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var call = RunOnStaAsync(() =>
        {
            if (Interlocked.CompareExchange(ref state.Value, 1, 0) != 0 && !policy.RunLate) throw new TimeoutException(Busy);
            started.TrySetResult();
            CallStarted?.Invoke(kind);
            using var limit = OfficeLib.PowerPointCallLimit.Begin(limitMs, limitMs);
            try { return work(); }
            catch (Exception ex) when (IsPowerPointGone(ex))
            {
                ForgetPowerPoint();
                throw;
            }
        });
        try
        {
            await Task.WhenAny(started.Task, call).WaitAsync(TimeSpan.FromMilliseconds(_queueMs ?? policy.QueueMs)).ConfigureAwait(false);
        }
        catch (TimeoutException)
        {
            if (Interlocked.CompareExchange(ref state.Value, 2, 0) == 0)
            {
                ObserveFault(call);
                throw new TimeoutException(Busy);
            }
        }
        try
        {
            return await call.WaitAsync(TimeSpan.FromMilliseconds(limitMs + GraceMs)).ConfigureAwait(false);
        }
        catch (TimeoutException) when (!call.IsCompleted)
        {
            ObserveFault(call);
            throw new TimeoutException(NotAnswering);
        }
    }

    private static void ObserveFault(Task call) =>
        _ = call.ContinueWith(static done => _ = done.Exception, CancellationToken.None,
            TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);

    // PowerPoint is there but refuses calls for now (a modal dialog is open in it), as OfficeLib.IsPowerPointBusy.
    internal static bool IsPowerPointBusy(Exception error)
    {
        for (Exception? e = error; e != null; e = e.InnerException)
        {
            if (e.HResult is RPC_E_CALL_REJECTED or RPC_E_SERVERCALL_RETRYLATER) return true;
        }
        return false;
    }

    /// <summary>The PowerPoint process is gone (closed, crashed, killed): every proxy made from it is dead.</summary>
    internal static bool IsPowerPointGone(Exception error)
    {
        for (Exception? e = error; e != null; e = e.InnerException)
        {
            if (e.HResult is RPC_S_SERVER_UNAVAILABLE or RPC_S_CALL_FAILED or RPC_E_DISCONNECTED
                or RPC_E_SERVER_DIED or RPC_E_SERVER_DIED_DNE)
                return true;
        }
        return false;
    }

    // Releasing a dead proxy returns at once, so everything owned is dropped here: the next show attaches to a
    // new PowerPoint instead of failing on the old reference until the session is closed.
    private void ForgetPowerPoint()
    {
        _outputShow = null;
        foreach (var owned in _showDecks.Values) owned.Dispose();
        _showDecks.Clear();
        foreach (var left in _unclosed) left.Dispose();
        _unclosed.Clear();
        foreach (var copy in _abandonedOpens) OfficeLib.PowerPoint.DeleteDiskCopy(copy);
        _abandonedOpens.Clear();
        _securityToRestore = null;
        foreach (var identity in _editorStates.Keys) Marshal.Release(identity);
        _editorStates.Clear();
        try { _ppt?.Dispose(); }
        catch (Exception ex) { System.Diagnostics.Debug.WriteLine(ex.Message); }
        _ppt = null;
    }

    // Closes an owned copy. When PowerPoint cannot close it now (it does not answer, or it is busy) the copy
    // stays owned, with its proxies, for the next sweep: released without a close it would stay open in
    // PowerPoint, its show on the audience screen, with nothing left to close it.
    private void CloseOrKeep(ShowDeck deck)
    {
        if (!deck.TryClose()) _unclosed.Add(deck);
    }

    /// <summary>
    /// Puts right what an earlier call left behind, as OfficeLib.PowerPoint.CloseLeftOpenPresentations: the macro
    /// security setting, copies that could not be closed, and disk copies whose cancelled Open PowerPoint may
    /// have carried out after all (found by their name, which is ours alone). What PowerPoint still refuses or
    /// does not answer stays for the next time; only a lost PowerPoint is thrown to the caller.
    /// </summary>
    private void SweepLeftovers(Application ppt)
    {
        static bool Later(Exception ex) => OfficeLib.PowerPointCallLimit.CurrentAbandoned || IsPowerPointBusy(ex);

        if (_securityToRestore is { } security)
        {
            try
            {
                ppt.AutomationSecurity = security;
                _securityToRestore = null;
            }
            catch (Exception ex) when (!IsPowerPointGone(ex))
            {
                if (!Later(ex)) _securityToRestore = null;
            }
        }
        for (int i = _unclosed.Count - 1; i >= 0; i--)
        {
            if (_unclosed[i].TryClose()) _unclosed.RemoveAt(i);
        }
        for (int i = _abandonedOpens.Count - 1; i >= 0; i--)
        {
            try
            {
                // By file name: it is unique, and PowerPoint may report the folder in another form.
                string name = Path.GetFileName(_abandonedOpens[i]);
                using var decks = ppt.Presentations;
                for (int n = decks.Count; n >= 1; n--)
                {
                    using var candidate = decks[n];
                    if (string.Equals(candidate.Name, name, StringComparison.OrdinalIgnoreCase)) candidate.Close();
                }
            }
            catch (Exception ex) when (!IsPowerPointGone(ex))
            {
                if (Later(ex)) continue;
            }
            OfficeLib.PowerPoint.DeleteDiskCopy(_abandonedOpens[i]);
            _abandonedOpens.RemoveAt(i);
        }
    }

    // As OfficeLib.PowerPoint.EnsureAttached: one cross-process call that fails at once when the process is gone.
    // A busy PowerPoint is alive: its error is the caller's.
    private Application EnsureApplication()
    {
        if (_ppt != null)
        {
            try
            {
                using var probe = _ppt.Presentations;
                _ = probe.Count;
                return _ppt;
            }
            catch (Exception ex) when (IsPowerPointGone(ex))
            {
                ForgetPowerPoint();
            }
        }
        return _ppt = _createApplication();
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
        var ppt = EnsureApplication();
        using var presentations = ppt.Presentations;
        using var deck = OpenSavedCopy(ppt, presentations, path);
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
        finally { deck.Close(); }
    });

    public Task<OfficePptSlideExport> ExportSlideAsync(string filePath, int slideNumber, int width, int height)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        ArgumentOutOfRangeException.ThrowIfLessThan(slideNumber, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(width, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(height, 1);

        return RunOnStaAsync(() =>
        {
            var ppt = EnsureApplication();
            using var presentations = ppt.Presentations;

            _Presentation? presentation = null;
            var tempFile = Path.Combine(Path.GetTempPath(), $"AxicultusPpt_{Guid.NewGuid():N}.png");

            try
            {
                presentation = OpenSavedCopy(ppt, presentations, filePath);
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
                    try { presentation.Close(); }
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
    private Presentation OpenSavedCopy(Application application, Presentations presentations, string path)
    {
        using var editor = FindPresentation(presentations, path);
        return OpenWithMacrosDisabled(application, () =>
            OfficeLib.PowerPoint.OpenShowCopy(presentations, path, MsoTriState.msoFalse, editor != null));
    }

    private Presentation OpenWithMacrosDisabled(Application application, Func<Presentation> open)
    {
        var previous = application.AutomationSecurity;
        try
        {
            application.AutomationSecurity = MsoAutomationSecurity.msoAutomationSecurityForceDisable;
            return open();
        }
        finally
        {
            try { application.AutomationSecurity = previous; }
            catch (Exception ex) when (OfficeLib.PowerPointCallLimit.CurrentAbandoned)
            {
                // Given up: this call is cancelled too. The operator's setting is put back when PowerPoint
                // answers again (SweepLeftovers), and the open's own error is not hidden by this one.
                System.Diagnostics.Debug.WriteLine(ex.Message);
                _securityToRestore ??= previous;
            }
        }
    }

    public Task<int?> ReadShowPositionAsync(string path) => RunLimitedAsync<int?>(() =>
    {
        if (_ppt == null || !_showDecks.TryGetValue(path, out var deck)) return null;
        using var windows = _ppt.SlideShowWindows;
        using var window = FindShowWindow(windows, deck.Presentation.FullName);
        if (window is null) return null;
        using var view = window.View;
        if (!IsShowingSlide(view.State)) return null;
        using var slide = view.Slide;
        return slide.SlideIndex;
    }, PptCall.Read);

    internal Task SetBlackoutAsync(string path, bool black) => RunLimitedAsync(() =>
    {
        if (_ppt == null || !_showDecks.TryGetValue(path, out var deck)) return false;
        using var windows = _ppt.SlideShowWindows;
        using var window = FindShowWindow(windows, deck.Presentation.FullName);
        using var view = window?.View;
        if (view != null && (black || view.State == PpSlideShowState.ppSlideShowBlackScreen))
            view.State = black ? PpSlideShowState.ppSlideShowBlackScreen : PpSlideShowState.ppSlideShowRunning;
        return true;
    }, PptCall.Blackout);

    internal Task<int?> AdvanceShowAsync(string path) => MoveShowAsync(path, forward: true);

    internal Task<int?> RetreatShowAsync(string path) => MoveShowAsync(path, forward: false);

    /// <summary>
    /// One native step of the owned show, as the Down and Up keys of OfficeLib.ImplementPowerpointSlideMovement:
    /// Next and Previous play click animations and skip hidden slides. Returns the slide now shown;
    /// <see cref="Rendering.IPowerPointSlideShowControl.EndOfShow"/> when Next ran past the last slide (the show
    /// is put back on its last slide, never left on "End of slide show"); null when there is no show to move.
    /// </summary>
    private Task<int?> MoveShowAsync(string path, bool forward) => RunLimitedAsync<int?>(() =>
    {
        if (_ppt == null || !_showDecks.TryGetValue(path, out var deck)) return null;
        using var windows = _ppt.SlideShowWindows;
        string name = deck.Presentation.FullName;
        using var window = FindShowWindow(windows, name);
        if (window is null) return null;
        using var view = window.View;
        bool ended = view.State == PpSlideShowState.ppSlideShowDone;
        if (!ended)
        {
            try
            {
                if (forward) view.Next(); else view.Previous();
                ended = view.State == PpSlideShowState.ppSlideShowDone;
            }
            catch (Exception ex) when (forward && !IsPowerPointGone(ex) && !OfficeLib.PowerPointCallLimit.CurrentAbandoned)
            {
                // With PowerPoint's "End with black slide" option off, Next on the last slide closes the show
                // and the view fails: the end of the show, not an error.
                using var stillOpen = FindShowWindow(windows, name);
                if (stillOpen != null) throw;
                return Rendering.IPowerPointSlideShowControl.EndOfShow;
            }
        }
        if (ended)
        {
            view.Last();
            if (forward) return Rendering.IPowerPointSlideShowControl.EndOfShow;
        }
        using var slide = view.Slide;
        return slide.SlideIndex;
    }, PptCall.Step);

    // Past its last slide ("End of slide show") the view has no slide and SlideShowView.Slide fails: put the show
    // back on its last slide first, as OfficeLib does.
    private static void LeaveEndScreen(SlideShowView view)
    {
        if (view.State == PpSlideShowState.ppSlideShowDone) view.Last();
    }

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
        return RunLimitedAsync(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();

            var ppt = EnsureApplication();
            SweepLeftovers(ppt);
            using var presentations = ppt.Presentations;

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
            var presentation = GetShowDeck(fullPath).Presentation;
            if (slideNumber > presentation.Slides.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(slideNumber), slideNumber, $"Slide number exceeds slide count {presentation.Slides.Count}.");
            }

            ConfigureSlideShowForLive(presentation, 1);
            using var showWindows = ppt.SlideShowWindows;
            using var slideShowWindow = FindShowWindow(showWindows, presentation.FullName)
                ?? presentation.SlideShowSettings.Run();
            if (slideShowWindow is null)
            {
                return false;
            }

            using var view = slideShowWindow.View;
            LeaveEndScreen(view);
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
        }, PptCall.Start);
    }

    public Task<bool> TriggerSlideShowNextAsync(
        string filePath,
        int slideNumber,
        CancellationToken cancellationToken = default, bool isOutput = true)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        ArgumentOutOfRangeException.ThrowIfLessThan(slideNumber, 1);

        var fullPath = Path.GetFullPath(filePath);
        return RunLimitedAsync(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();

            var ppt = EnsureApplication();
            SweepLeftovers(ppt);
            using var presentations = ppt.Presentations;

            if (isOutput) ClosePreviousOutput(presentations, fullPath);
            var presentation = GetShowDeck(fullPath).Presentation;
            if (slideNumber > presentation.Slides.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(slideNumber), slideNumber, $"Slide number exceeds slide count {presentation.Slides.Count}.");
            }

            ConfigureSlideShowForLive(presentation, slideNumber);
            using var showWindows = ppt.SlideShowWindows;
            using var slideShowWindow = FindShowWindow(showWindows, presentation.FullName)
                ?? presentation.SlideShowSettings.Run();
            if (slideShowWindow is null)
            {
                return false;
            }

            using var view = slideShowWindow.View;
            LeaveEndScreen(view);
            if (view.Slide.SlideIndex != slideNumber)
            {
                view.GotoSlide(slideNumber, MsoTriState.msoTrue);
            }

            ActivateShow(presentation, slideShowWindow, fullPath, isOutput);
            // Same three cases as OfficeLib.PowerPoint.SafePlayNext. Clicks, not effects: MainSequence.Count also
            // counts "With/After Previous" effects, and with no click effects Next() would leave the slide.
            var totalClicks = view.GetClickCount();
            var currentIndex = view.GetClickIndex();
            if (currentIndex < totalClicks)
            {
                view.Next();
            }
            else if (totalClicks > 0)
            {
                view.GotoClick(0);
                view.Next();
            }
            else
            {
                view.GotoSlide(slideNumber, MsoTriState.msoTrue);
            }

            return true;
        }, PptCall.Trigger);
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
        Shell.OutputWindowPlacement? bounds = null, bool topmost = false, SlideShowWindow? previousWindow = null)
    {
        if (previousWindow == null)
        {
            using var presentations = _ppt!.Presentations;
            using var editor = FindPresentation(presentations, path);
            if (editor != null) MinimizeEditors(editor);
            window.Activate();
        }
        if (!isOutput) return;
        var handle = OfficeLib.PowerPoint.FindSlideShowWindow(window, () => deck.Name, out _);
        // PowerPoint can recreate the window when the operator triggers the same slide again.
        if (bounds == null && _outputShow is { } previous && string.Equals(previous.Path, path, StringComparison.OrdinalIgnoreCase))
        {
            if (previousWindow == null && handle == previous.Handle) return;
            bounds = previous.Bounds;
            topmost = previous.Topmost;
        }
        // The show is running from here on: it is the output show before it is placed, so a placement that fails
        // still leaves a show the caller can see (HasOutputShow) and stop.
        _outputShow = new(path, handle, bounds, topmost);
        if (handle != 0 && bounds != null)
        {
            Platform.DesktopWindowPlacement.Apply(handle, bounds);
            Platform.DesktopWindowPlacement.SetTopmost(handle, topmost);
        }
        else if (previousWindow != null)
        {
            // Office may expose the new HWND only after the old show closes. Both COM windows use points.
            window.Left = previousWindow.Left;
            window.Top = previousWindow.Top;
            window.Width = previousWindow.Width;
            window.Height = previousWindow.Height;
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
        if (_outputShow is not { } previous)
        {
            // A start that failed or was given up left its copy owned and perhaps running.
            foreach (var (path, left) in (System.Collections.Generic.KeyValuePair<string, ShowDeck>[])[.. _showDecks])
            {
                if (string.Equals(path, nextPath, StringComparison.OrdinalIgnoreCase)) continue;
                _showDecks.Remove(path);
                CloseOrKeep(left);
            }
            return;
        }
        if (string.Equals(previous.Path, nextPath, StringComparison.OrdinalIgnoreCase)) return;
        if (_showDecks.Remove(previous.Path, out var owned)) CloseOrKeep(owned);
        using var editor = FindPresentation(decks, previous.Path);
        if (editor != null) RestoreEditors(editor);
        _outputShow = null;
    }

    public Task CloseAsync() => RunLimitedAsync(() =>
    {
        _outputShow = null;
        if (_ppt is null) return true;

        foreach (var (path, owned) in (System.Collections.Generic.KeyValuePair<string, ShowDeck>[])[.. _showDecks])
        {
            _showDecks.Remove(path);
            _unclosed.Add(owned);
        }
        try { SweepLeftovers(_ppt); }
        catch (Exception ex) when (IsPowerPointGone(ex))
        {
            // No PowerPoint, so nothing is open in it.
            ForgetPowerPoint();
            return true;
        }
        // PowerPoint stopped answering: a cancelled Close is not carried out and releasing a proxy would wait for
        // PowerPoint. The copies and the reference stay owned; the next close, or the next show, finishes this.
        if (OfficeLib.PowerPointCallLimit.CurrentAbandoned) throw new TimeoutException(NotAnswering);
        // PowerPoint is busy (a modal dialog is open in it): the show is still on the screen. The caller is told,
        // and the copies stay owned for the next close.
        if (_unclosed.Count != 0) throw new InvalidOperationException("PowerPoint is busy: the slide show could not be closed.");

        try
        {
            using var decks = _ppt.Presentations;
            for (int i = decks.Count; _editorStates.Count != 0 && i > 0; i--)
            {
                using var deck = decks[i];
                RestoreEditors(deck);
            }
            if (decks.Count == 0 && MayQuit(_ppt)) _ppt.Quit();
        }
        catch (Exception ex) when (!IsPowerPointBusy(ex)) { /* COM 종료 시 발생 가능, 무시 */ }
        if (OfficeLib.PowerPointCallLimit.CurrentAbandoned) throw new TimeoutException(NotAnswering);

        // Dispose releases the COM proxy. (_ppt is the NetOffice wrapper, not a __ComObject, so the
        // FinalReleaseComObject call that used to follow threw ArgumentException on every close.)
        try { _ppt.Dispose(); }
        catch { }

        foreach (var identity in _editorStates.Keys) Marshal.Release(identity);
        _editorStates.Clear();
        _ppt = null;
        // 명시적 GC — COM 참조 카운트 즉시 해제 유도
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        return true;
    }, PptCall.Close);

    /// <summary>
    /// PowerPoint is single-instance, so the process behind this session may be the operator's own. As
    /// OfficeLib.PowerPoint.QuitPowerPointApp: never quit while a file is open in Protected View (those are not in
    /// Presentations), nor a visible PowerPoint that was already running when this process started.
    /// </summary>
    private static bool MayQuit(Application app)
    {
        using var self = System.Diagnostics.Process.GetCurrentProcess();
        return MayQuit(
            () =>
            {
                using var windows = app.ProtectedViewWindows;
                return windows.Count;
            },
            () =>
            {
                if (app.Visible != MsoTriState.msoTrue
                    || global::Axicultus.Interop.DesktopWindows.GetWindowThreadProcessId(new IntPtr(app.HWND), out uint id) == 0 || id == 0)
                    return null;
                using var powerPoint = System.Diagnostics.Process.GetProcessById((int)id);
                return powerPoint.StartTime;
            },
            self.StartTime);
    }

    /// <summary>
    /// protectedViewWindows: how many files are open in Protected View. visibleSince: when the PowerPoint process
    /// started if PowerPoint is visible, null when it is hidden. Either read can fail on a real PowerPoint.
    /// </summary>
    internal static bool MayQuit(Func<int> protectedViewWindows, Func<DateTime?> visibleSince, DateTime selfStarted)
    {
        try
        {
            if (protectedViewWindows() != 0) return false;
        }
        catch (Exception ex)
        {
            // Cannot be read: PowerPoint is not quit.
            System.Diagnostics.Debug.WriteLine(ex.Message);
            return false;
        }

        DateTime? since = null;
        try { since = visibleSince(); }
        catch (Exception ex)
        {
            // Cannot be told: the presentation checks alone decide, as in OfficeLib.
            System.Diagnostics.Debug.WriteLine(ex.Message);
        }
        return !(since < selfStarted);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        // 종료 전 세션 닫기. The close is bounded by its call limit.
        bool blocked = false;
        try { CloseAsync().GetAwaiter().GetResult(); }
        catch (TimeoutException) { blocked = true; }
        catch { }

        _cts.Cancel();
        _workQueue.CompleteAdding();
        // A worker still waiting for PowerPoint is a background thread: it is left behind with the queue it
        // reads, so app exit never waits for a PowerPoint that does not answer.
        if (blocked || !_staThread.Join(TimeSpan.FromSeconds(5))) return;
        _cts.Dispose();
        _workQueue.Dispose();
    }
}
