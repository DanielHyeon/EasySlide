using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32;
using NetOffice.OfficeApi.Enums;
using NetOffice.PowerPointApi;
using NetOffice.PowerPointApi.Enums;

namespace Easislides.Wpf.Interop;

public sealed record OfficePptSlideExport(
    int SlideCount,
    byte[] ImageBytes,
    string ContentType);

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

    public OfficePptSession()
    {
        _workQueue = new System.Collections.Concurrent.BlockingCollection<Action>();
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
        var tcs = new TaskCompletionSource<T>();
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

    public Task<OfficePptSlideExport> ExportSlideAsync(string filePath, int slideNumber, int width, int height)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        ArgumentOutOfRangeException.ThrowIfLessThan(slideNumber, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(width, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(height, 1);

        return RunOnStaAsync(() =>
        {
            _ppt ??= new Application();
            _ = _ppt.Presentations.Count;

            _Presentation? presentation = null;
            var tempFile = Path.Combine(Path.GetTempPath(), $"EasiSlidesPpt_{Guid.NewGuid():N}.png");

            try
            {
                presentation = _ppt.Presentations.Open(filePath, MsoTriState.msoFalse, MsoTriState.msoTrue, MsoTriState.msoFalse);
                var slideCount = presentation.Slides.Count;
                if (slideNumber > slideCount)
                {
                    throw new ArgumentOutOfRangeException(nameof(slideNumber), slideNumber, $"Slide number exceeds slide count {slideCount}.");
                }

                presentation.Slides[slideNumber].Export(tempFile, "PNG", width, height);
                return new OfficePptSlideExport(slideCount, File.ReadAllBytes(tempFile), "image/png");
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

    /// <summary>Start a PowerPoint slide show on the configured output monitor.</summary>
    public Task<bool> StartSlideShowAsync(
        string filePath,
        int slideNumber,
        string outputMonitorName,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        ArgumentOutOfRangeException.ThrowIfLessThan(slideNumber, 1);

        var fullPath = Path.GetFullPath(filePath);
        return RunOnStaAsync(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();

            _ppt ??= new Application();
            _ = _ppt.Presentations.Count;

            SaveSlideShowMonitor(_ppt.Version, outputMonitorName);
            var presentation = FindOrOpenPresentation(fullPath);
            if (slideNumber > presentation.Slides.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(slideNumber), slideNumber, $"Slide number exceeds slide count {presentation.Slides.Count}.");
            }

            ConfigureSlideShowForLive(presentation, 1);
            var slideShowWindow = TryGetSlideShowWindow(presentation)
                ?? presentation.SlideShowSettings.Run();
            if (slideShowWindow is null)
            {
                return false;
            }

            var view = slideShowWindow.View;
            if (slideNumber < 2)
            {
                view.First();
            }
            else if (view.Slide.SlideIndex != slideNumber)
            {
                view.GotoSlide(slideNumber, MsoTriState.msoFalse);
            }

            slideShowWindow.Activate();
            return true;
        });
    }

    public Task<bool> TriggerSlideShowNextAsync(
        string filePath,
        int slideNumber,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        ArgumentOutOfRangeException.ThrowIfLessThan(slideNumber, 1);

        var fullPath = Path.GetFullPath(filePath);
        return RunOnStaAsync(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();

            _ppt ??= new Application();
            _ = _ppt.Presentations.Count;

            var presentation = FindOrOpenPresentation(fullPath);
            if (slideNumber > presentation.Slides.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(slideNumber), slideNumber, $"Slide number exceeds slide count {presentation.Slides.Count}.");
            }

            ConfigureSlideShowForLive(presentation, slideNumber);
            var slideShowWindow = TryGetSlideShowWindow(presentation)
                ?? presentation.SlideShowSettings.Run();
            if (slideShowWindow is null)
            {
                return false;
            }

            var view = slideShowWindow.View;
            if (view.Slide.SlideIndex != slideNumber)
            {
                view.GotoSlide(slideNumber, MsoTriState.msoTrue);
            }

            slideShowWindow.Activate();
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

    private _Presentation FindOrOpenPresentation(string fullPath)
    {
        if (_ppt is null)
        {
            throw new InvalidOperationException("PowerPoint session is not open.");
        }

        for (var i = 1; i <= _ppt.Presentations.Count; i++)
        {
            var candidate = _ppt.Presentations[i];
            if (string.Equals(Path.GetFullPath(candidate.FullName), fullPath, StringComparison.OrdinalIgnoreCase))
            {
                return candidate;
            }
        }

        return _ppt.Presentations.Open(
            fullPath,
            MsoTriState.msoFalse,
            MsoTriState.msoFalse,
            MsoTriState.msoTrue);
    }

    private static SlideShowWindow? TryGetSlideShowWindow(_Presentation presentation)
    {
        try
        {
            var window = presentation.SlideShowWindow;
            _ = window.View.Slide.SlideIndex;
            return window;
        }
        catch (COMException)
        {
            return null;
        }
        catch (InvalidOperationException)
        {
            return null;
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

    private static void SaveSlideShowMonitor(string version, string outputMonitorName)
    {
        if (string.IsNullOrWhiteSpace(version) || string.IsNullOrWhiteSpace(outputMonitorName))
        {
            return;
        }

        try
        {
            using var key = Registry.CurrentUser.CreateSubKey($@"Software\Microsoft\Office\{version}\PowerPoint\Options");
            key?.SetValue("DisplayMonitor", outputMonitorName, RegistryValueKind.String);
            key?.SetValue("UseAutoMonSelection", 0, RegistryValueKind.DWord);
            key?.SetValue("UseMonMgr", 0, RegistryValueKind.DWord);
        }
        catch
        {
            // WinForms treats monitor registry writes as best-effort; keep text live output available.
        }
    }

    public Task CloseAsync() => RunOnStaAsync(() =>
    {
        if (_ppt is not null)
        {
            try { _ppt.Quit(); }
            catch { /* COM 종료 시 발생 가능, 무시 */ }

            try { _ppt.Dispose(); }
            catch { }

            try { Marshal.FinalReleaseComObject(_ppt); }
            catch { }

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
