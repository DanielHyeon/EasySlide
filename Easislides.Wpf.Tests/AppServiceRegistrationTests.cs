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
