using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Easislides.Wpf.Data;
using Easislides.Wpf.Demo;
using Easislides.Wpf.Input;
using Easislides.Wpf.Media;
using Easislides.Wpf.Platform;
using Easislides.Wpf.Rendering;
using Easislides.Wpf.Settings;
using Easislides.Wpf.Shell;
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
        services.AddSingleton<ICommandCatalog, CommandCatalog>();
        services.AddSingleton<IGlobalInputDispatcher>(_ => new WpfGlobalInputDispatcher(Current.Dispatcher));
        services.AddSingleton<IGlobalKeySource, HookManagerGlobalKeySource>();
        services.AddSingleton<IGlobalInputService, GlobalInputService>();
        services.AddSingleton<IDisplayReader, SystemDisplayReader>();
        services.AddSingleton<IDisplayService, DisplayService>();
        services.AddSingleton<IWindowPlacementService, WindowPlacementService>();
        services.AddSingleton<IPlatformDiagnosticsService, PlatformDiagnosticsService>();
        services.AddSingleton<IImageAssetService, ImageAssetService>();
        services.AddSingleton<ITransitionEffectService, TransitionEffectService>();
        services.AddSingleton<IOutputRenderer, OutputRenderer>();
        services.AddSingleton<IThumbnailCache, ThumbnailCache>();
        services.AddSingleton<ISettingsService, SettingsService>();
        services.AddSingleton<IAssetMigrationService, AssetMigrationService>();
        services.AddSingleton<IDatabaseMigrationService, DatabaseMigrationService>();
        services.AddSingleton<IPowerPointRenderBackend, OfficePowerPointRenderBackend>();
        services.AddSingleton<IPowerPointRenderService, PowerPointRenderService>();

        // M1 운영 셸 — 라이브 세션, 출력 창 상태, 안전 확인, 명령 기록
        services.AddSingleton<ILiveSessionService, LiveSessionService>();
        services.AddSingleton<IOutputWindowService, OutputWindowService>();
        services.AddSingleton<IMediaPlaybackBackend, NoOpMediaPlaybackBackend>();
        services.AddSingleton<IMediaPlaybackService, MediaPlaybackService>();
        services.AddSingleton<ILiveSafetyPrompt, WpfLiveSafetyPrompt>();
        services.AddSingleton<ICommandTelemetry, InMemoryCommandTelemetry>();
        services.AddTransient<MediaPlaybackViewModel>();
        services.AddTransient<SettingsWindowViewModel>();
        services.AddTransient<OutputWindow>();
        services.AddTransient<SettingsWindow>();
        services.AddSingleton<OutputSurfaceFactory>(sp => () => sp.GetRequiredService<OutputWindow>());
        services.AddSingleton<IOutputWindowHost, OutputWindowHost>();
        services.AddTransient<MainViewModel>();
        services.AddTransient<MainWindow>();
        services.AddTransient<DemoWindow>();

        Services = services.BuildServiceProvider();
        _ = Services.GetRequiredService<IOutputWindowHost>();

        var useDemo = Array.Exists(e.Args, arg => string.Equals(arg, "--demo", StringComparison.OrdinalIgnoreCase));
        Window window = useDemo
            ? Services.GetRequiredService<DemoWindow>()
            : Services.GetRequiredService<MainWindow>();
        window.Show();

        if (!useDemo)
        {
            var globalInput = Services.GetRequiredService<IGlobalInputService>();
            if (!globalInput.Start())
            {
                MessageBox.Show(
                    "전역 단축키를 시작하지 못했습니다.\n\n앱 내부 단축키는 계속 사용할 수 있습니다.",
                    "EasiSlides — 전역 단축키",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        if (Services is IDisposable disposable)
        {
            disposable.Dispose();
        }

        base.OnExit(e);
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
