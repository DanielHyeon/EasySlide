using System;
using System.Threading.Tasks;
using Easislides.Wpf.Settings;
using FluentAssertions;
using Microsoft.Win32;
using Xunit;

namespace Easislides.Wpf.Tests.Settings;

public class RegistryLegacySettingsSourceTests
{
    [Fact]
    public void TryGetString_ReadsStringAndNumericValuesFromLegacySections()
    {
        using var fixture = RegistryFixture.Create();
        fixture.SetValue("config", "root_directory", @"C:\EasiSlides");
        fixture.SetValue("options", "PowerpointMaxFiles", 80, RegistryValueKind.DWord);
        fixture.SetValue("monitors", "AlwaysTryDualMonitor", 0, RegistryValueKind.DWord);
        var sut = fixture.CreateSource();

        sut.TryGetString("root_directory", out var root).Should().BeTrue();
        sut.TryGetString("PowerpointMaxFiles", out var maxFiles).Should().BeTrue();
        sut.TryGetString("AlwaysTryDualMonitor", out var alwaysTryDualMonitor).Should().BeTrue();

        root.Should().Be(@"C:\EasiSlides");
        maxFiles.Should().Be("80");
        alwaysTryDualMonitor.Should().Be("0");
    }

    [Fact]
    public void TryGetString_WhenValueIsMissing_DoesNotCreateLegacyRegistryKeys()
    {
        using var fixture = RegistryFixture.Create(createBaseKey: false);
        var sut = fixture.CreateSource();

        sut.TryGetString("root_directory", out var value).Should().BeFalse();

        value.Should().BeNull();
        Registry.CurrentUser.OpenSubKey(fixture.BasePath).Should().BeNull();
    }

    [Fact]
    public async Task MigrateLegacyAsync_UsesRegistryLegacySettingsSource()
    {
        using var fixture = RegistryFixture.Create();
        fixture.SetValue("config", "root_directory", @"C:\EasiSlides");
        fixture.SetValue("config", "media_dir", @"C:\EasiSlides\Media");
        fixture.SetValue("options", "PowerpointMaxFiles", 80, RegistryValueKind.DWord);
        fixture.SetValue("monitors", "AlwaysTryDualMonitor", 0, RegistryValueKind.DWord);
        fixture.SetValue("monitors", "DualMonitorOptionCustomWidth", 1920, RegistryValueKind.DWord);
        using var settingsFixture = TempSettingsFolder.Create();
        var settings = settingsFixture.CreateService();

        var result = await settings.MigrateLegacyAsync(fixture.CreateSource());

        result.Succeeded.Should().BeTrue();
        settings.Get(EasiSettingKeys.WorkingFolder).Should().Be(@"C:\EasiSlides");
        settings.Get(EasiSettingKeys.MediaDirectory).Should().Be(@"C:\EasiSlides\Media");
        settings.Get(EasiSettingKeys.PowerPointMaxFiles).Should().Be(80);
        settings.Get(EasiSettingKeys.DisplayAlwaysUseSecondaryMonitor).Should().BeFalse();
        settings.Get(EasiSettingKeys.DisplayCustomWidth).Should().Be(1920);
    }

    private sealed class RegistryFixture : IDisposable
    {
        private RegistryFixture(string basePath, bool createBaseKey)
        {
            BasePath = basePath;
            Registry.CurrentUser.DeleteSubKeyTree(BasePath, throwOnMissingSubKey: false);
            if (createBaseKey)
            {
                using var _ = Registry.CurrentUser.CreateSubKey(BasePath);
            }
        }

        public string BasePath { get; }

        public static RegistryFixture Create(bool createBaseKey = true)
            => new($@"Software\EasiSlides.Tests\{Guid.NewGuid():N}", createBaseKey);

        public RegistryLegacySettingsSource CreateSource()
            => new(Registry.CurrentUser, BasePath);

        public void SetValue(string section, string name, object value, RegistryValueKind valueKind = RegistryValueKind.String)
        {
            using var key = Registry.CurrentUser.CreateSubKey($@"{BasePath}\{section}");
            key.Should().NotBeNull();
            key!.SetValue(name, value, valueKind);
        }

        public void Dispose()
        {
            Registry.CurrentUser.DeleteSubKeyTree(BasePath, throwOnMissingSubKey: false);
        }
    }

    private sealed class TempSettingsFolder : IDisposable
    {
        private TempSettingsFolder(string root)
        {
            Root = root;
        }

        public string Root { get; }

        public static TempSettingsFolder Create()
            => new(System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"EasiSlides_RegistrySettings_{Guid.NewGuid():N}"));

        public SettingsService CreateService()
            => new(new SettingsServiceOptions(
                System.IO.Path.Combine(Root, "settings.json"),
                System.IO.Path.Combine(Root, "Backups")));

        public void Dispose()
        {
            try
            {
                System.IO.Directory.Delete(Root, recursive: true);
            }
            catch
            {
            }
        }
    }
}
