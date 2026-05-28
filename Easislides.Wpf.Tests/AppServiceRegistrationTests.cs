using System;
using System.IO;
using System.Reflection;
using System.Windows.Threading;
using Easislides.Wpf.Data;
using Easislides.Wpf.Media;
using Easislides.Wpf.Platform;
using Easislides.Wpf.Rendering;
using Easislides.Wpf.Settings;
using Easislides.Wpf.Shell;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Easislides.Wpf.Tests;

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
