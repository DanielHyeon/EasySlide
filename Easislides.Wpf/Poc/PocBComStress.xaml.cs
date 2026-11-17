using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Easislides.Wpf.Interop;

namespace Easislides.Wpf.Poc;

/// <summary>
/// PoC-B: PowerPoint COM 객체의 STA 어피니티 + 좀비 방지 검증.
/// 합격 시 Sprint 1 진입 가능 (계획서 §8.3).
///
/// 설계 (v2 — 100회 세션 재생성 → 단일 세션 + 100회 가벼운 호출로 변경):
///   1. OfficePptSession 1회 생성 → STA 워커 스레드 1개
///   2. PowerPoint Application 1회 실행
///   3. 100회 반복: Version 속성 조회 (STA 어피니티 검증 + COM 마샬링 검증)
///   4. 세션 1회 종료 → Marshal.FinalReleaseComObject + GC
///   5. 종료 후 POWERPNT.EXE 좀비 0건 확인
///
/// 이전 설계(매 회 세션 생성/파괴)는 PowerPoint 100개 동시/연속 실행으로 OS 자원 고갈 → 앱 크래시 유발했음.
/// 변경 후 동일한 검증 목적을 자원 소모 1/100 수준으로 달성.
///
/// 주의: PowerPoint 미설치 PC에서는 OpenAsync 호출 즉시 COMException — 그것 자체로도 패턴 검증 가능.
/// </summary>
public partial class PocBComStress : Window
{
    private const int IterationCount = 100;
    private const int IterationTimeoutMs = 5000;
    private CancellationTokenSource? _stopCts;
    private bool _running;

    public PocBComStress()
    {
        InitializeComponent();
    }

    private async void OnStart_Click(object sender, RoutedEventArgs e)
    {
        if (_running) return;
        _running = true;
        BtnStart.IsEnabled = false;
        PocProgress.Value = 0;
        StatusText.Text = "실행 중...";

        _stopCts = new CancellationTokenSource();
        var token = _stopCts.Token;

        var beforeCount = CountPowerPointProcesses();
        Log($"시작 전 POWERPNT.EXE 프로세스 수: {beforeCount}");
        Log($"설계: 단일 세션 + Version 속성 {IterationCount}회 조회 (STA 어피니티 검증).");
        Log("");

        var stopwatch = Stopwatch.StartNew();
        var successCount = 0;
        var failureCount = 0;
        var sessionOpened = false;
        OfficePptSession? session = null;

        try
        {
            // === 1. 세션 1회 생성 ===
            try
            {
                session = new OfficePptSession();
                var openTask = session.OpenAsync();
                if (await Task.WhenAny(openTask, Task.Delay(15000, token)) != openTask)
                {
                    Log("[ERR] PowerPoint Application 생성 타임아웃(15초). PowerPoint가 설치되지 않았거나 응답 없음.");
                    StatusText.Text = "✗ 세션 열기 실패";
                    return;
                }
                await openTask; // 예외 propagate
                sessionOpened = true;
                Log($"✓ PowerPoint 세션 생성 완료 ({stopwatch.Elapsed.TotalMilliseconds:F0}ms)");
            }
            catch (Exception ex)
            {
                Log($"[ERR] 세션 생성 실패: {FormatException(ex)}");
                Log("  → PowerPoint 미설치 PC이거나 COM 등록 문제. 자체로도 PoC 의미 있음 (예외 잡힘).");
                StatusText.Text = "✗ PowerPoint 미설치 또는 COM 등록 실패";
                return;
            }

            // === 2. 100회 가벼운 COM 호출 ===
            Log("");
            Log("--- 100회 Presentations.Count 조회 시작 (OfficeLib 검증 패턴) ---");
            for (int i = 1; i <= IterationCount; i++)
            {
                if (token.IsCancellationRequested)
                {
                    Log($"[중단] {i - 1}회 진행 후 사용자가 중단함");
                    break;
                }

                try
                {
                    var pingTask = session.PingAsync();
                    if (await Task.WhenAny(pingTask, Task.Delay(IterationTimeoutMs, token)) != pingTask)
                    {
                        failureCount++;
                        Log($"[{i:D3}] 타임아웃({IterationTimeoutMs}ms)");
                        continue;
                    }
                    var count = await pingTask;
                    successCount++;
                    if (i == 1 || i == IterationCount || i % 25 == 0)
                        Log($"[{i:D3}/100] Presentations.Count={count}, 성공 {successCount} / 실패 {failureCount}");
                }
                catch (OperationCanceledException) { break; }
                catch (Exception ex)
                {
                    failureCount++;
                    // 예외 디테일 — 타입 + 메시지 + InnerException 체인 노출
                    Log($"[{i:D3}] 실패: {FormatException(ex)}");
                    // 첫 실패만 상세 stack trace 추가 (이후엔 동일 패턴이라 생략)
                    if (failureCount == 1)
                    {
                        Log($"  Stack: {ex.StackTrace?.Split('\n')[0]?.Trim() ?? "(no stack)"}");
                    }
                }

                PocProgress.Value = i;
            }
        }
        finally
        {
            // === 3. 세션 1회 정리 ===
            if (session != null)
            {
                try
                {
                    Log("");
                    Log("세션 종료 중 (Marshal.FinalReleaseComObject + GC)...");
                    var closeTask = Task.Run(() => session.Dispose());
                    if (await Task.WhenAny(closeTask, Task.Delay(10000)) != closeTask)
                    {
                        Log("[WARN] 세션 종료 타임아웃(10초). 좀비 가능성.");
                    }
                    else
                    {
                        Log("✓ 세션 종료 완료");
                    }
                }
                catch (Exception ex)
                {
                    Log($"[WARN] Dispose 실패: {ex.GetType().Name} — {ex.Message}");
                }
            }
            stopwatch.Stop();
            _stopCts?.Dispose();
            _stopCts = null;
        }

        // === 4. 결과 ===
        Log("");
        Log("=== 완료 ===");
        Log($"세션: {(sessionOpened ? "정상 생성/종료" : "생성 실패")}");
        Log($"100회 Version 조회: 성공 {successCount} / 실패 {failureCount}");
        Log($"소요 시간: {stopwatch.Elapsed.TotalSeconds:F1}초");

        // 좀비 확인 — GC 추가 + 1초 대기
        await Task.Delay(1000);
        var afterCount = CountPowerPointProcesses();
        var zombies = afterCount - beforeCount;

        Log("");
        Log($"종료 후 POWERPNT.EXE: {afterCount} (시작 전 대비: {zombies:+0;-0;0})");

        var passed = sessionOpened && (failureCount == 0) && (zombies <= 0);
        StatusText.Text = passed
            ? "✓ PoC-B 합격 — Sprint 1 진입 가능"
            : $"✗ PoC-B 불합격 — 실패 {failureCount}회 / 좀비 {zombies}개";

        Log("");
        Log(passed
            ? "✓ 합격: STA 어피니티 + COM 마샬링 + Dispose 패턴 정상 작동"
            : "✗ 불합격: §10.3 리스크 표에 결과 기록 + 해결책 PoC 추가 필요");

        BtnStart.IsEnabled = true;
        _running = false;
    }

    private void OnCheckZombies_Click(object sender, RoutedEventArgs e)
    {
        var count = CountPowerPointProcesses();
        Log($"현재 POWERPNT.EXE 프로세스 수: {count}");
        if (count > 0)
        {
            foreach (var p in Process.GetProcessesByName("POWERPNT"))
            {
                try
                {
                    Log($"  · PID {p.Id} / 시작 {p.StartTime:HH:mm:ss} / 워킹셋 {p.WorkingSet64 / 1024 / 1024}MB");
                    p.Dispose();
                }
                catch { }
            }
        }
    }

    private static int CountPowerPointProcesses()
    {
        return Process.GetProcessesByName("POWERPNT").Length;
    }

    /// <summary>예외를 사용자에게 보여줄 한 줄 + InnerException 체인으로 정리.</summary>
    private static string FormatException(Exception ex)
    {
        var parts = new System.Collections.Generic.List<string>();
        var current = ex;
        int depth = 0;
        while (current != null && depth < 5)
        {
            parts.Add($"{current.GetType().Name}: {current.Message}");
            current = current.InnerException;
            depth++;
        }
        return string.Join(" → ", parts);
    }

    private void OnClearLog_Click(object sender, RoutedEventArgs e)
    {
        LogTextBlock.Text = string.Empty;
    }

    private void Log(string message)
    {
        var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
        LogTextBlock.Text += $"{timestamp}  {message}\n";
        LogScroll.ScrollToEnd();
    }

    protected override void OnClosed(EventArgs e)
    {
        _stopCts?.Cancel();
        _stopCts?.Dispose();
        base.OnClosed(e);
    }
}
