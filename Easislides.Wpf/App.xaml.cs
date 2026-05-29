using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Easislides.Wpf.Data;
using Easislides.Wpf.Demo;
using Easislides.Wpf.Input;
using Easislides.Wpf.Library;
using Easislides.Wpf.Media;
using Easislides.Wpf.Onboarding;
using Easislides.Wpf.Platform;
using Easislides.Wpf.Rendering;
using Easislides.Wpf.Settings;
using Easislides.Wpf.Shell;
using Easislides.Wpf.Support;
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

    public static void ConfigureServices(
        IServiceCollection services,
        SettingsServiceOptions settingsOptions,
        Dispatcher dispatcher)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(settingsOptions);
        ArgumentNullException.ThrowIfNull(dispatcher);

        // 디자인 시스템 — 런타임 테마/스케일 변경 (ADR-0006)
        services.AddSingleton<IThemeService, ThemeService>();

        // 단축키 단일 소스 (ADR-0004)
        services.AddSingleton<ShortcutRegistry>();
        services.AddSingleton<ICommandCatalog, CommandCatalog>();
        services.AddSingleton<IGlobalInputDispatcher>(_ => new WpfGlobalInputDispatcher(dispatcher));
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
        services.AddSingleton(settingsOptions);
        services.AddSingleton<ISettingsService>(sp => new SettingsService(sp.GetRequiredService<SettingsServiceOptions>()));
        services.AddSingleton<RegistryLegacySettingsSource>();
        services.AddSingleton(_ => FileLegacySettingsSource.CreateDefault());
        services.AddSingleton<ILegacySettingsSource>(sp => new CompositeLegacySettingsSource(
            sp.GetRequiredService<RegistryLegacySettingsSource>(),
            sp.GetRequiredService<FileLegacySettingsSource>()));
        services.AddSingleton<ISettingsBootstrapMigrationService, SettingsBootstrapMigrationService>();
        services.AddSingleton<IOnboardingDialogService, WelcomeWindowDialogService>();
        services.AddSingleton<IOnboardingCoordinator, OnboardingCoordinator>();
        services.AddSingleton<ISupportInfoService, SupportInfoService>();
        services.AddSingleton<ISupportLauncher, SupportLauncher>();
        services.AddSingleton<ISettingsPathPicker, SettingsPathPicker>();
        services.AddSingleton<IAssetMigrationService, AssetMigrationService>();
        services.AddSingleton<IOperationalDataRehearsalService, OperationalDataRehearsalService>();
        services.AddSingleton<IDatabaseMigrationService, DatabaseMigrationService>();
        services.AddSingleton<AdminDatabaseRepository>();
        services.AddSingleton<IAdminDatabaseRepository>(sp => sp.GetRequiredService<AdminDatabaseRepository>());
        services.AddSingleton<IAdminSongDetailRepository>(sp => sp.GetRequiredService<AdminDatabaseRepository>());
        services.AddSingleton<ISongMergeService, SongMergeService>();
        services.AddSingleton<IExternalFileOperationService, ExternalFileOperationService>();
        services.AddSingleton<IImportExportService, ImportExportService>();
        services.AddSingleton<IBibleRepository, BibleRepository>();
        services.AddSingleton<IPowerPointRenderBackend, OfficePowerPointRenderBackend>();
        services.AddSingleton<IPowerPointRenderService>(sp => new PowerPointRenderService(
            sp.GetRequiredService<IPowerPointRenderBackend>(),
            sp.GetRequiredService<ISettingsService>()));

        // M1 운영 셸 — 라이브 세션, 출력 창 상태, 안전 확인, 명령 기록
        services.AddSingleton<ILiveSessionService, LiveSessionService>();
        services.AddSingleton<IOutputWindowService, OutputWindowService>();
        services.AddSingleton<IMediaPlaybackBackend, NoOpMediaPlaybackBackend>();
        services.AddSingleton<IMediaPlaybackService>(sp => new MediaPlaybackService(
            sp.GetRequiredService<IMediaPlaybackBackend>(),
            sp.GetRequiredService<ISettingsService>()));
        services.AddSingleton<ILiveSafetyPrompt, WpfLiveSafetyPrompt>();
        services.AddSingleton<ICommandTelemetry, InMemoryCommandTelemetry>();
        services.AddTransient<MediaPlaybackViewModel>();
        services.AddTransient<LibraryViewModel>();
        services.AddTransient<FolderEditorViewModel>();
        services.AddTransient<SongEditorViewModel>();
        services.AddTransient<SongCopyViewModel>();
        services.AddTransient<SongMoveViewModel>();
        services.AddTransient<SongDeleteViewModel>();
        services.AddTransient<SongRecoveryViewModel>();
        services.AddTransient<SongMergeViewModel>();
        services.AddTransient<ExternalFileOperationViewModel>();
        services.AddTransient<ImportExportViewModel>();
        services.AddTransient<BibleViewModel>();
        services.AddTransient<SettingsWindowViewModel>();
        services.AddTransient<AboutWindowViewModel>();
        services.AddTransient<HelpWindowViewModel>();
        services.AddTransient<RegistrationWindowViewModel>();
        services.AddTransient<OutputWindow>();
        services.AddTransient<LibraryWindow>();
        services.AddTransient<FolderEditorWindow>();
        services.AddTransient<SongEditorWindow>();
        services.AddTransient<SongCopyWindow>();
        services.AddTransient<SongMoveWindow>();
        services.AddTransient<SongDeleteWindow>();
        services.AddTransient<SongRecoveryWindow>();
        services.AddTransient<SongMergeWindow>();
        services.AddTransient<ExternalFileOperationWindow>();
        services.AddTransient<ImportExportWindow>();
        services.AddTransient<BibleWindow>();
        services.AddTransient<SettingsWindow>();
        services.AddTransient<AboutWindow>();
        services.AddTransient<HelpWindow>();
        services.AddTransient<RegistrationWindow>();
        services.AddSingleton<OutputSurfaceFactory>(sp => () => sp.GetRequiredService<OutputWindow>());
        services.AddSingleton<IOutputWindowHost, OutputWindowHost>();
        services.AddTransient<MainViewModel>();
        services.AddTransient<MainWindow>();
        services.AddTransient<DemoWindow>();
    }

    private void OnStartup(object sender, StartupEventArgs e)
    {
        // 전역 예외 핸들러 — Sprint 0 PoC 중 silent crash 방지.
        // Sprint 1 이후: Serilog 등 로깅 인프라로 교체 + 사용자에게 비차단 알림.
        DispatcherUnhandledException += OnUiException;
        AppDomain.CurrentDomain.UnhandledException += OnDomainException;
        TaskScheduler.UnobservedTaskException += OnTaskException;

        // DI 컨테이너 구성
        var services = new ServiceCollection();
        var settingsOptions = SettingsServiceOptions.CreateDefault();
        var useDemo = Array.Exists(e.Args, arg => string.Equals(arg, "--demo", StringComparison.OrdinalIgnoreCase));
        ConfigureServices(services, settingsOptions, Current.Dispatcher);

        Services = services.BuildServiceProvider();
        _ = Services.GetRequiredService<IOutputWindowHost>();

        if (!useDemo)
        {
            var migrationResult = Services
                .GetRequiredService<ISettingsBootstrapMigrationService>()
                .MigrateIfNeededAsync()
                .GetAwaiter()
                .GetResult();

            if (migrationResult is { Succeeded: false })
            {
                MessageBox.Show(
                    "기존 설정을 자동으로 가져오지 못했습니다.\n\n기본 설정으로 계속 시작합니다.",
                    "EasiSlides — 설정 마이그레이션",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }

        Window window = useDemo
            ? Services.GetRequiredService<DemoWindow>()
            : Services.GetRequiredService<MainWindow>();
        window.Show();

        if (!useDemo)
        {
            Services.GetRequiredService<IOnboardingCoordinator>().RunIfNeeded(window);

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
