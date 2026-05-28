using System;
using System.IO;
using System.Reflection;
using System.Windows.Threading;
using Easislides.Wpf.Rendering;
using Easislides.Wpf.Settings;
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
            var service = provider.GetRequiredService<IPowerPointRenderService>();

            service.Should().BeOfType<PowerPointRenderService>();
            var settingsField = typeof(PowerPointRenderService).GetField("_settings", BindingFlags.Instance | BindingFlags.NonPublic);
            settingsField.Should().NotBeNull();
            settingsField!.GetValue(service).Should().BeSameAs(settings);
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
