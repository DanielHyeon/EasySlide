using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Axicultus.Wpf.Data;
using Axicultus.Wpf.Demo;
using Axicultus.Wpf.Input;
using Axicultus.Wpf.Library;
using Axicultus.Wpf.Localization;
using Axicultus.Wpf.Media;
using Axicultus.Wpf.Platform;
using Axicultus.Wpf.Rendering;
using Axicultus.Wpf.Settings;
using Axicultus.Wpf.Shell;
using Axicultus.Wpf.Startup;
using Axicultus.Wpf.Support;
using Axicultus.Wpf.Theme;
using Microsoft.Extensions.DependencyInjection;

namespace Axicultus.Wpf;

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
        string profileDirectory = Path.GetDirectoryName(Path.GetFullPath(settingsOptions.SettingsFilePath))!;

        // 디자인 시스템 — 런타임 테마/스케일 변경 (ADR-0006)
        services.AddSingleton<IThemeService, ThemeService>();

        // 단축키 단일 소스 (ADR-0004)
        services.AddSingleton<ShortcutRegistry>();
        services.AddSingleton<ICommandCatalog>(_ => new CommandCatalog());
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
        services.AddSingleton<IDocumentTextExtractor, OfficeDocumentTextExtractor>();
        services.AddSingleton<IImportExportService, ImportExportService>();
        services.AddSingleton<ISearchUsageService, SearchUsageService>();
        services.AddSingleton<IUsageDeleteConfirmation, WpfUsageDeleteConfirmation>();
        services.AddSingleton<IBibleRepository, BibleRepository>();
        services.AddSingleton<IPowerPointRenderBackend, PowerPointWorker>();
        services.AddSingleton<IPowerPointRenderService>(sp => new PowerPointRenderService(
            sp.GetRequiredService<IPowerPointRenderBackend>(),
            sp.GetRequiredService<ISettingsService>(),
            Path.Combine(profileDirectory, "Cache", "PowerPoint")));
        services.AddSingleton<IPowerPointSlideShowControl, OfficePowerPointSlideShowControl>();

        // M1 운영 셸 — 라이브 세션, 출력 창 상태, 안전 확인, 명령 기록
        services.AddSingleton<ILiveSessionService, LiveSessionService>();
        services.AddSingleton(sp => new Captions.CaptionSession(sp.GetRequiredService<ILiveSessionService>(),
            sp.GetService<IPowerPointSlideShowControl>(), Path.Combine(profileDirectory, "caption-style.json")));
        services.AddSingleton<Captions.CaptionBroadcastServer>();
        services.AddSingleton(_ => new Captions.ObsOperator(
            profileDirectory, dispatcher));
        services.AddSingleton(_ => new Captions.CaptionRecoveryStore(Path.Combine(profileDirectory, "CaptionRecovery")));
        services.AddSingleton<IOutputWindowService, OutputWindowService>();
        // 미디어 백엔드 — 생명주기 브리지(출력 창의 MediaElement 가 만들어지면 실제 백엔드를 Attach).
        // 동일 인스턴스를 IMediaPlaybackBackend(서비스용)과 구체 타입(출력 창 Attach용) 양쪽으로 노출.
        services.AddSingleton<CameraOwnership>();
        services.AddSingleton(sp => new AttachableMediaPlaybackBackend(() => new OutputMonitorMediaPlayer(
            sp.GetRequiredService<MediaPlaybackService>(), sp.GetRequiredService<IWindowPlacementService>(),
            sp.GetRequiredService<ShortcutRegistry>(), sp.GetRequiredService<IPowerPointSlideShowControl>())));
        services.AddSingleton<IMediaPlaybackBackend>(sp => sp.GetRequiredService<AttachableMediaPlaybackBackend>());
        services.AddSingleton(sp => new MediaPlaybackService(
            sp.GetRequiredService<IMediaPlaybackBackend>(),
            sp.GetRequiredService<ISettingsService>()));
        services.AddSingleton<IMediaPlaybackService>(sp => sp.GetRequiredService<MediaPlaybackService>());
        services.AddSingleton<ILiveSafetyPrompt, WpfLiveSafetyPrompt>();
        services.AddSingleton<ICommandTelemetry, InMemoryCommandTelemetry>();
        services.AddSingleton<IWorshipListStore>(sp => new WorshipListStore(Path.Combine(profileDirectory, "WorshipLists"), sp.GetRequiredService<ISettingsService>()));
        services.AddSingleton(_ => new WorshipSessionNotesService(Path.Combine(profileDirectory, "SessionNotes")));
        services.AddSingleton(_ => new RecentItemEdits(Path.Combine(profileDirectory, "recent-item-edits.json")));
        services.AddSingleton<IPraiseBookIndexService, PraiseBookIndexService>();
        services.AddSingleton<IPraiseBookStore>(sp => new PraiseBookStore(Path.Combine(profileDirectory, "PraiseBooks"), sp.GetRequiredService<ISettingsService>()));
        services.AddSingleton<IInfoScreenStore>(sp => new InfoScreenStore(Path.Combine(profileDirectory, "InfoScreens"), sp.GetRequiredService<ISettingsService>()));
        services.AddSingleton<IRecentWorshipLists>(_ => new RecentWorshipListsService(Path.Combine(profileDirectory, "recent-worshiplists.json")));
        services.AddSingleton<IAppearanceTemplateStore>(_ => new AppearanceTemplateStore(Path.Combine(profileDirectory, "AppearanceTemplates")));
        services.AddTransient<MediaPlaybackViewModel>();
        services.AddTransient<PowerPointPreviewViewModel>();
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
        services.AddTransient<SearchUsageViewModel>();
        services.AddTransient<BibleViewModel>();
        services.AddTransient<SettingsWindowViewModel>();
        services.AddTransient<AboutWindowViewModel>();
        services.AddTransient<HelpWindowViewModel>();
        services.AddTransient<RegistrationWindowViewModel>();
        services.AddTransient<OutputWindow>();
        services.AddTransient<PreviewWindow>();
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
        services.AddTransient<SearchUsageWindow>();
        services.AddTransient<BibleWindow>();
        services.AddTransient<SettingsWindow>();
        services.AddTransient<AboutWindow>();
        services.AddTransient<HelpWindow>();
        services.AddTransient<RegistrationWindow>();
        services.AddSingleton<OutputSurfaceFactory>(sp => () =>
        {
            // 출력 창 생성 시 MediaElement 를 미디어 브리지에 부착(실제 미디어 백엔드 트랙 2단계).
            var window = sp.GetRequiredService<OutputWindow>();
            window.CameraCaption.DataContext = sp.GetRequiredService<Captions.CaptionSession>();
            window.AttachMedia(sp.GetRequiredService<AttachableMediaPlaybackBackend>(), sp.GetRequiredService<MediaPlaybackService>(), sp.GetRequiredService<CameraOwnership>());
            return window;
        });
        services.AddSingleton<IOutputWindowHost, OutputWindowHost>();
        // 스테이지(Preview) 모니터 — 회중용 출력과 별개로 워십 리더·밴드가 보는 확인 모니터(gap §3.1).
        // surface 는 전용 PreviewWindow(가사 Region1/2 + 시계 + 다음 항목 오버레이) — 같은 OutputWindowViewModel 에
        // 바인딩하되 출력의 전환 엔진·미디어는 쓰지 않는다. 세션은 PreviewWindowHost 가 정규화(Hidden→Active)해
        // 회중 화면이 검정/비움 중에도 가사를 계속 보여 준다.
        services.AddSingleton<IPreviewWindowService, PreviewWindowService>();
        services.AddSingleton<PreviewWindowHost>(sp => new PreviewWindowHost(
            sp.GetRequiredService<IPreviewWindowService>(),
            sp.GetRequiredService<ILiveSessionService>(),
            () => sp.GetRequiredService<PreviewWindow>(),
            sp.GetRequiredService<IOutputRenderer>(),
            sp.GetRequiredService<ISettingsService>()));
        services.AddTransient<MainViewModel>();
        services.AddTransient<MainWindow>();
        services.AddTransient<DemoWindow>();
    }

    private async void OnStartup(object sender, StartupEventArgs e)
    {
        if (e.Args.Length == 1 && e.Args[0] == PowerPointWorker.Switch)
        {
            ShutdownMode = ShutdownMode.OnExplicitShutdown;
            RunPowerPointWorker();
            return;
        }
        DispatcherUnhandledException += OperationalErrorLog.Default.OnUiException;
        AppDomain.CurrentDomain.UnhandledException += OperationalErrorLog.Default.OnDomainException;
        TaskScheduler.UnobservedTaskException += OperationalErrorLog.Default.OnTaskException;

        ShutdownMode = ShutdownMode.OnExplicitShutdown;

        var startup = new StartupArguments(e.Args);

        // --legacy-ui 안전망 (ADR-0007): 신규 빌드에서 legacy WinForms 앱으로 즉시 롤백.
        // legacy 실행 파일을 찾아 시작하면 WPF UI 를 띄우지 않고 종료한다.
        // 찾지 못하거나 시작에 실패하면 경고 후 신규 UI 로 계속 — 사용자가 빈손이 되지 않도록.
        // 주의: 이 분기에서는 DI 컨테이너/MainWindow 를 절대 초기화하지 않는다(Launched 시 즉시 종료).
        // 런처는 결과 enum 으로만 보고하고 throw 하지 않지만, 안전망이 시작을 깨지 않도록 한 번 더 방어.
        if (startup.UseLegacyUi)
        {
            var outcome = LegacyUiLaunchOutcome.LaunchFailed;
            try
            {
                outcome = LegacyUiLauncher.CreateDefault().LaunchIfRequested(true);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[LEGACY-UI] Safety-net exception (ignored; continuing with the new UI): {ex}");
            }

            if (outcome == LegacyUiLaunchOutcome.Launched)
            {
                Shutdown();
                return;
            }

            MessageBox.Show(
                UiText.Format("App", "LegacyUiFailed", LegacyUiLauncher.LegacyExecutableName),
                UiText.Get("App", "LegacyUiTitle"),
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }

        // Copying an earlier WPF profile is a convenience: a failure warns and the app still starts.
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        Exception? profileError = null;
        bool profileCopied = await Task.Run(() => WpfProfileUpgrade.TryImport(appData, ex => profileError = ex));
        // The earlier settings file itself was not copied: nothing is saved in this session, so the next start can
        // still bring it over (a saved file would be kept and the earlier settings lost).
        bool earlierSettingsPending = !profileCopied && WpfProfileUpgrade.EarlierSettingsPending(appData);
        if (!profileCopied)
        {
            OperationalErrorLog.Default.Write("Profile migration", profileError!);
            MessageBox.Show(
                (earlierSettingsPending
                    ? UiText.Get("App", "EarlierSettingsNotCopied")
                    : UiText.Get("App", "SomeSettingsNotCopied"))
                + $"\n\n{profileError!.Message}",
                UiText.Get("App", "ProfileMigrationTitle"), MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        // DI 컨테이너 구성
        var services = new ServiceCollection();
        var settingsOptions = SettingsServiceOptions.CreateDefault() with { Persist = !earlierSettingsPending };
        ConfigureServices(services, settingsOptions, Current.Dispatcher);
        var provider = services.BuildServiceProvider();

        // First run only (no WPF settings.json yet): take over the WinForms settings. Skipped while an earlier
        // WPF settings.json is still waiting to be copied, so the import does not take its place.
        if (!earlierSettingsPending)
        {
            var imported = await LegacySettingsBootstrap.ImportOnFirstRunAsync(
                provider.GetRequiredService<ISettingsService>(),
                settingsOptions.SettingsFilePath,
                () => LegacySettingsBootstrap.OpenDefaultSource(appData),
                ex => OperationalErrorLog.Default.Write("Legacy settings import", ex));
            if (imported is { Succeeded: false })
                OperationalErrorLog.Default.Write("Legacy settings import", string.Join("; ", imported.Issues));
        }

        OpenShell(provider, startup.UseDemo);
    }

    internal void OpenShell(IServiceProvider services, bool useDemo)
    {
        Services = services;
        // The menus' language (View > Language), before any window reads its texts.
        Axicultus.Localization.UiLanguage.Apply(Services.GetRequiredService<ISettingsService>().Get(AxicultusSettingKeys.Language));
        _ = Services.GetRequiredService<IOutputWindowHost>();
        // 스테이지(Preview) 모니터 호스트도 미리 띄워 세션·창 상태 구독을 시작한다(창은 사용자가 열기 전엔 닫힌 채 대기 — 160-D 에서 열기 명령 추가).
        _ = Services.GetRequiredService<PreviewWindowHost>();

        if (!useDemo)
        {
            var size = Services.GetRequiredService<ISettingsService>().Get(AxicultusSettingKeys.InterfaceSize);
            Services.GetRequiredService<IThemeService>().ApplyInterfaceSize(size);
        }

        Window window = useDemo
            ? Services.GetRequiredService<DemoWindow>()
            : Services.GetRequiredService<MainWindow>();
        MainWindow = window;
        ShutdownMode = ShutdownMode.OnMainWindowClose;
        // An unexpected error must not take the audience screen down in the middle of a show.
        if (window.DataContext is Shell.MainViewModel shell)
            OperationalErrorLog.Default.ReportDuringShow = shell.ReportErrorDuringShow;
        window.Show();

        if (!useDemo)
        {
            var globalInput = Services.GetRequiredService<IGlobalInputService>();
            if (!globalInput.Start())
            {
                MessageBox.Show(
                    UiText.Get("App", "GlobalHotkeysFailed"),
                    UiText.Get("App", "GlobalHotkeysTitle"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }
    }

    private async void RunPowerPointWorker()
    {
        try { await PowerPointWorker.RunAsync(); Shutdown(0); }
        catch (Exception ex) { System.Diagnostics.Trace.WriteLine(ex); Shutdown(1); }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        if (Services is IDisposable disposable)
        {
            disposable.Dispose();
        }

        DispatcherUnhandledException -= OperationalErrorLog.Default.OnUiException;
        AppDomain.CurrentDomain.UnhandledException -= OperationalErrorLog.Default.OnDomainException;
        TaskScheduler.UnobservedTaskException -= OperationalErrorLog.Default.OnTaskException;
        base.OnExit(e);
    }
}
