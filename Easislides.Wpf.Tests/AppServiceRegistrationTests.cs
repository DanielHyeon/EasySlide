using System;
using System.IO;
using System.Reflection;
using System.Windows.Threading;
using Easislides.Wpf.Data;
using Easislides.Wpf.Library;
using Easislides.Wpf.Media;
using Easislides.Wpf.Onboarding;
using Easislides.Wpf.Platform;
using Easislides.Wpf.Rendering;
using Easislides.Wpf.Settings;
using Easislides.Wpf.Shell;
using Easislides.Wpf.Support;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Easislides.Wpf.Tests;

[Collection("WPF Application")]
public class AppServiceRegistrationTests
{
    [Fact]
    public void ConfigureServices_ResolvesPowerPointRenderServiceWithSettingsBackedConstructor()
    {
        StaHelper.RunOnSta(() =>
        {
            using var settingsFolder = TempSettingsFolder.Create();
            var services = new ServiceCollection();
            var options = new SettingsServiceOptions(settingsFolder.SettingsPath, settingsFolder.BackupRoot);

            App.ConfigureServices(services, options, Dispatcher.CurrentDispatcher);

            using var provider = services.BuildServiceProvider();
            var settings = provider.GetRequiredService<ISettingsService>();
            var legacySettings = provider.GetRequiredService<ILegacySettingsSource>();
            var service = provider.GetRequiredService<IPowerPointRenderService>();
            var mediaPlayback = provider.GetRequiredService<IMediaPlaybackService>();
            var placement = provider.GetRequiredService<IWindowPlacementService>();
            var adminRepository = provider.GetRequiredService<IAdminDatabaseRepository>();
            var songDetails = provider.GetRequiredService<IAdminSongDetailRepository>();
            var songMergeService = provider.GetRequiredService<ISongMergeService>();
            var externalFiles = provider.GetRequiredService<IExternalFileOperationService>();
            var documentTextExtractor = provider.GetRequiredService<IDocumentTextExtractor>();
            var importExportService = provider.GetRequiredService<IImportExportService>();
            var searchUsageService = provider.GetRequiredService<ISearchUsageService>();
            var usageDeleteConfirmation = provider.GetRequiredService<IUsageDeleteConfirmation>();
            var bibleRepository = provider.GetRequiredService<IBibleRepository>();
            var rehearsal = provider.GetRequiredService<IOperationalDataRehearsalService>();
            var onboarding = provider.GetRequiredService<IOnboardingCoordinator>();
            var onboardingDialog = provider.GetRequiredService<IOnboardingDialogService>();
            var supportInfo = provider.GetRequiredService<ISupportInfoService>();
            var supportLauncher = provider.GetRequiredService<ISupportLauncher>();
            var libraryViewModel = provider.GetRequiredService<LibraryViewModel>();
            var folderEditorViewModel = provider.GetRequiredService<FolderEditorViewModel>();
            var songEditorViewModel = provider.GetRequiredService<SongEditorViewModel>();
            var songCopyViewModel = provider.GetRequiredService<SongCopyViewModel>();
            var songMoveViewModel = provider.GetRequiredService<SongMoveViewModel>();
            var songDeleteViewModel = provider.GetRequiredService<SongDeleteViewModel>();
            var songRecoveryViewModel = provider.GetRequiredService<SongRecoveryViewModel>();
            var songMergeViewModel = provider.GetRequiredService<SongMergeViewModel>();
            var externalFileOperationViewModel = provider.GetRequiredService<ExternalFileOperationViewModel>();
            var importExportViewModel = provider.GetRequiredService<ImportExportViewModel>();
            var searchUsageViewModel = provider.GetRequiredService<SearchUsageViewModel>();
            var bibleViewModel = provider.GetRequiredService<BibleViewModel>();
            var aboutViewModel = provider.GetRequiredService<AboutWindowViewModel>();
            var helpViewModel = provider.GetRequiredService<HelpWindowViewModel>();
            var registrationViewModel = provider.GetRequiredService<RegistrationWindowViewModel>();
            var settingsViewModel = provider.GetRequiredService<SettingsWindowViewModel>();
            var outputHost = provider.GetRequiredService<IOutputWindowHost>();

            service.Should().BeOfType<PowerPointRenderService>();
            var settingsField = typeof(PowerPointRenderService).GetField("_settings", BindingFlags.Instance | BindingFlags.NonPublic);
            settingsField.Should().NotBeNull();
            settingsField!.GetValue(service).Should().BeSameAs(settings);

            placement.Should().BeOfType<WindowPlacementService>();
            var placementSettingsField = typeof(WindowPlacementService).GetField("_settings", BindingFlags.Instance | BindingFlags.NonPublic);
            placementSettingsField.Should().NotBeNull();
            placementSettingsField!.GetValue(placement).Should().BeSameAs(settings);

            mediaPlayback.Should().BeOfType<MediaPlaybackService>();
            var mediaSettingsField = typeof(MediaPlaybackService).GetField("_settings", BindingFlags.Instance | BindingFlags.NonPublic);
            mediaSettingsField.Should().NotBeNull();
            mediaSettingsField!.GetValue(mediaPlayback).Should().BeSameAs(settings);

            legacySettings.Should().BeOfType<CompositeLegacySettingsSource>();
            adminRepository.Should().BeOfType<AdminDatabaseRepository>();
            songDetails.Should().BeSameAs(adminRepository);
            songMergeService.Should().BeOfType<SongMergeService>();
            externalFiles.Should().BeOfType<ExternalFileOperationService>();
            documentTextExtractor.Should().BeOfType<OfficeDocumentTextExtractor>();
            importExportService.Should().BeOfType<ImportExportService>();
            searchUsageService.Should().BeOfType<SearchUsageService>();
            usageDeleteConfirmation.Should().BeOfType<WpfUsageDeleteConfirmation>();
            bibleRepository.Should().BeOfType<BibleRepository>();
            rehearsal.Should().BeOfType<OperationalDataRehearsalService>();
            onboarding.Should().BeOfType<OnboardingCoordinator>();
            onboardingDialog.Should().BeOfType<WelcomeWindowDialogService>();
            supportInfo.Should().BeOfType<SupportInfoService>();
            supportLauncher.Should().BeOfType<SupportLauncher>();
            libraryViewModel.Should().NotBeNull();
            folderEditorViewModel.Should().NotBeNull();
            songEditorViewModel.Should().NotBeNull();
            songCopyViewModel.Should().NotBeNull();
            songMoveViewModel.Should().NotBeNull();
            songDeleteViewModel.Should().NotBeNull();
            songRecoveryViewModel.Should().NotBeNull();
            songMergeViewModel.Should().NotBeNull();
            externalFileOperationViewModel.Should().NotBeNull();
            importExportViewModel.Should().NotBeNull();
            searchUsageViewModel.Should().NotBeNull();
            bibleViewModel.Should().NotBeNull();
            aboutViewModel.Should().NotBeNull();
            helpViewModel.Should().NotBeNull();
            registrationViewModel.Should().NotBeNull();

            settingsViewModel.Should().NotBeNull();
            var rehearsalField = typeof(SettingsWindowViewModel).GetField(
                "_operationalDataRehearsal",
                BindingFlags.Instance | BindingFlags.NonPublic);
            rehearsalField.Should().NotBeNull();
            rehearsalField!.GetValue(settingsViewModel).Should().BeSameAs(rehearsal);

            outputHost.Should().BeOfType<OutputWindowHost>();
            var hostSettingsField = typeof(OutputWindowHost).GetField("_settings", BindingFlags.Instance | BindingFlags.NonPublic);
            hostSettingsField.Should().NotBeNull();
            hostSettingsField!.GetValue(outputHost).Should().BeSameAs(settings);
        });
    }

    [Fact]
    public void ConfigureServices_RegistersPreviewStageMonitor_ClosedAndSettingsBacked()
    {
        // 증분160-C — 스테이지(Preview) 모니터 서비스·호스트가 DI 에 등록되고, 기본으로 닫힌 채 시작하며,
        // 출력 호스트와 같은 설정 인스턴스를 주입받는지 검증. surface 는 OutputWindow 재사용(여기선 창을 만들지 않으므로 안전).
        StaHelper.RunOnSta(() =>
        {
            using var settingsFolder = TempSettingsFolder.Create();
            var services = new ServiceCollection();
            var options = new SettingsServiceOptions(settingsFolder.SettingsPath, settingsFolder.BackupRoot);

            App.ConfigureServices(services, options, Dispatcher.CurrentDispatcher);

            using var provider = services.BuildServiceProvider();
            var settings = provider.GetRequiredService<ISettingsService>();
            var previewService = provider.GetRequiredService<IPreviewWindowService>();
            var previewHost = provider.GetRequiredService<PreviewWindowHost>();

            previewService.Should().BeOfType<PreviewWindowService>();
            previewService.Current.IsOpen.Should().BeFalse("스테이지 모니터는 기본으로 닫힌 채 시작(사용자가 열기 전)");

            previewHost.Should().NotBeNull();
            // 같은 설정 인스턴스를 주입받았는지(출력 호스트와 동일 검증).
            var hostSettingsField = typeof(PreviewWindowHost).GetField("_settings", BindingFlags.Instance | BindingFlags.NonPublic);
            hostSettingsField.Should().NotBeNull();
            hostSettingsField!.GetValue(previewHost).Should().BeSameAs(settings);

            // 스테이지가 회중과 **같은 라이브 세션·렌더러 싱글톤**을 봐야 한다 — 누군가 AddTransient 로 바꾸면
            // 스테이지/출력이 서로 다른 세션을 보는 회귀가 생기므로 동일 인스턴스를 고정한다.
            var hostSessionField = typeof(PreviewWindowHost).GetField("_session", BindingFlags.Instance | BindingFlags.NonPublic);
            hostSessionField!.GetValue(previewHost).Should().BeSameAs(provider.GetRequiredService<ILiveSessionService>());
            var hostRendererField = typeof(PreviewWindowHost).GetField("_renderer", BindingFlags.Instance | BindingFlags.NonPublic);
            hostRendererField!.GetValue(previewHost).Should().BeSameAs(provider.GetRequiredService<IOutputRenderer>());

            // 스테이지 서비스는 싱글톤 — 호스트가 주입받은 것과 컨테이너가 주는 것이 같은 인스턴스.
            provider.GetRequiredService<IPreviewWindowService>().Should().BeSameAs(previewService);
        });
    }

    private sealed class TempSettingsFolder : IDisposable
    {
        private TempSettingsFolder(string root)
        {
            Root = root;
            SettingsPath = Path.Combine(root, "settings.json");
            BackupRoot = Path.Combine(root, "Backups");
        }

        public string Root { get; }

        public string SettingsPath { get; }

        public string BackupRoot { get; }

        public static TempSettingsFolder Create()
            => new(Path.Combine(Path.GetTempPath(), $"EasiSlides_AppServices_{Guid.NewGuid():N}"));

        public void Dispose()
        {
            try
            {
                Directory.Delete(Root, recursive: true);
            }
            catch
            {
            }
        }
    }
}
