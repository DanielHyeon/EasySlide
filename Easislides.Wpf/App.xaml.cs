using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Easislides.Wpf.Demo;
using Easislides.Wpf.Input;
using Easislides.Wpf.Theme;
using Microsoft.Extensions.DependencyInjection;

namespace Easislides.Wpf;

/// <summary>
/// 애플리케이션 진입점.
/// Sprint 0 단계: DI 컨테이너 부트스트랩 + DemoWindow 표시 + 전역 예외 트랩.
/// Sprint 2 이후: MainWindow(=WpfMainWindow, FrmMain 대체) 표시로 변경.
/// </summary>
public partial class App : Application
{
    /// <summary>전역 DI 서비스 프로바이더.</summary>
    public static IServiceProvider Services { get; private set; } = null!;

    private void OnStartup(object sender, StartupEventArgs e)
    {
        // 전역 예외 핸들러 — Sprint 0 PoC 중 silent crash 방지.
        // Sprint 1 이후: Serilog 등 로깅 인프라로 교체 + 사용자에게 비차단 알림.
        DispatcherUnhandledException += OnUiException;
        AppDomain.CurrentDomain.UnhandledException += OnDomainException;
        TaskScheduler.UnobservedTaskException += OnTaskException;

        // DI 컨테이너 구성
        var services = new ServiceCollection();

        // 디자인 시스템 — 런타임 테마/스케일 변경 (ADR-0006)
        services.AddSingleton<IThemeService, ThemeService>();

        // 단축키 단일 소스 (ADR-0004)
        services.AddSingleton<ShortcutRegistry>();

        Services = services.BuildServiceProvider();

        // 데모 윈도우 표시 (Sprint 0 산출물 확인용)
        var demo = new DemoWindow();
        demo.Show();
    }

    private static void OnUiException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine($"[UI EXCEPTION] {e.Exception}");
        MessageBox.Show(
            $"UI 스레드 예외:\n\n{e.Exception.GetType().Name}: {e.Exception.Message}\n\n계속 진행합니다.",
            "EasiSlides — 디버그 알림",
            MessageBoxButton.OK,
            MessageBoxImage.Warning);
        e.Handled = true; // 앱 종료 방지
    }

    private static void OnDomainException(object sender, UnhandledExceptionEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine($"[DOMAIN EXCEPTION] {e.ExceptionObject}");
        // AppDomain unhandled은 일반적으로 process 종료를 막을 수 없음. 로깅만.
    }

    private static void OnTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine($"[UNOBSERVED TASK] {e.Exception}");
        e.SetObserved(); // GC 시점 process 종료 방지
    }
}
