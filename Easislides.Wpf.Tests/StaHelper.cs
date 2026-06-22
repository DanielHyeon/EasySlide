using System;
using System.Threading;
using System.Threading.Tasks;

namespace Easislides.Wpf.Tests;

/// <summary>
/// WPF UI 컴포넌트는 STA 스레드에서만 인스턴스화 가능 (System.Windows.Input.InputManager 제약).
/// xUnit 기본 ThreadPool은 MTA — 새 STA 스레드에서 실행 후 결과 마샬링.
///
/// 외부 패키지(xunit.stafact 등) 의존성 회피 — 자체 헬퍼로 통제 가능성 확보.
/// </summary>
internal static class StaHelper
{
    /// <summary>동기 Action을 STA 스레드에서 실행 후 예외를 다시 throw.</summary>
    public static void RunOnSta(Action action)
    {
        Exception? captured = null;
        var thread = new Thread(() =>
        {
            try { action(); }
            catch (Exception ex) { captured = ex; }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.IsBackground = true;
        thread.Start();
        thread.Join();
        if (captured is not null)
        {
            // 원래 stack trace 보존
            System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(captured).Throw();
        }
    }

    /// <summary>비동기 Func를 STA 스레드에서 실행 후 결과·예외를 마샬링.</summary>
    public static T RunOnSta<T>(Func<T> func)
    {
        T result = default!;
        Exception? captured = null;
        var thread = new Thread(() =>
        {
            try { result = func(); }
            catch (Exception ex) { captured = ex; }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.IsBackground = true;
        thread.Start();
        thread.Join();
        if (captured is not null)
        {
            System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(captured).Throw();
        }
        return result;
    }

    /// <summary>STA 스레드에서 비동기 작업 실행 — Dispatcher 펌프 포함.</summary>
    public static Task RunOnStaAsync(Func<Task> asyncAction)
    {
        var tcs = new TaskCompletionSource();
        var thread = new Thread(() =>
        {
            try
            {
                // STA 스레드에 Dispatcher 펌프 생성
                var sync = new System.Windows.Threading.DispatcherSynchronizationContext(
                    System.Windows.Threading.Dispatcher.CurrentDispatcher);
                SynchronizationContext.SetSynchronizationContext(sync);

                var task = asyncAction();
                task.ContinueWith(t =>
                {
                    if (t.IsFaulted) tcs.SetException(t.Exception!.InnerExceptions);
                    else if (t.IsCanceled) tcs.SetCanceled();
                    else tcs.SetResult();
                    // Dispatcher 종료
                    System.Windows.Threading.Dispatcher.CurrentDispatcher.InvokeShutdown();
                });

                // Dispatcher 메시지 펌프 — 비동기 작업 완료 시까지 메시지 처리
                System.Windows.Threading.Dispatcher.Run();
            }
            catch (Exception ex)
            {
                tcs.TrySetException(ex);
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.IsBackground = true;
        thread.Start();
        return tcs.Task;
    }
}
