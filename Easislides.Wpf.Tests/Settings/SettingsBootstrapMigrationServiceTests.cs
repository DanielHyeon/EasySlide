using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Easislides.Wpf.Settings;
using FluentAssertions;
using Xunit;

namespace Easislides.Wpf.Tests.Settings;

public class SettingsBootstrapMigrationServiceTests
{
    [Fact]
    public async Task MigrateIfNeededAsync_WhenSettingsFileIsMissing_ImportsLegacySettings()
    {
        using var fixture = TempSettingsFolder.Create();
        var settings = fixture.CreateSettings();
        var legacy = new DictionaryLegacySettingsSource(new Dictionary<string, string?>
        {
            ["root_directory"] = @"C:\EasiSlides",
            ["media_dir"] = @"C:\EasiSlides\Media",
        });
        var sut = new SettingsBootstrapMigrationService(settings, legacy, fixture.Options);

        var result = await sut.MigrateIfNeededAsync();

        result.Should().NotBeNull();
        result!.Succeeded.Should().BeTrue();
        settings.Get(EasiSettingKeys.WorkingFolder).Should().Be(@"C:\EasiSlides");
        settings.Get(EasiSettingKeys.MediaDirectory).Should().Be(@"C:\EasiSlides\Media");
        File.Exists(fixture.Options.SettingsFilePath).Should().BeTrue();
    }

    [Fact]
    public async Task MigrateIfNeededAsync_WhenSettingsFileExists_SkipsLegacyMigration()
    {
        using var fixture = TempSettingsFolder.Create();
        var settings = fixture.CreateSettings();
        settings.Set(EasiSettingKeys.WorkingFolder, @"D:\Current");
        var legacy = new DictionaryLegacySettingsSource(new Dictionary<string, string?>
        {
            ["root_directory"] = @"C:\Legacy",
        });
        var sut = new SettingsBootstrapMigrationService(settings, legacy, fixture.Options);

        var result = await sut.MigrateIfNeededAsync();

        result.Should().BeNull();
        settings.Get(EasiSettingKeys.WorkingFolder).Should().Be(@"D:\Current");
    }

    [Fact]
    public async Task MigrateIfNeededAsync_WhenMigrationFails_DoesNotCreateSettingsFile()
    {
        using var fixture = TempSettingsFolder.Create();
        var settings = fixture.CreateSettings();
        var legacy = new DictionaryLegacySettingsSource(new Dictionary<string, string?>
        {
            ["root_directory"] = "",
        });
        var sut = new SettingsBootstrapMigrationService(settings, legacy, fixture.Options);

        var result = await sut.MigrateIfNeededAsync();

        result.Should().NotBeNull();
        result!.Succeeded.Should().BeFalse();
        File.Exists(fixture.Options.SettingsFilePath).Should().BeFalse();
    }

    private sealed class TempSettingsFolder : IDisposable
    {
        private TempSettingsFolder(string root)
        {
            Root = root;
            Options = new SettingsServiceOptions(
                Path.Combine(root, "settings.json"),
                Path.Combine(root, "Backups"));
        }

        public string Root { get; }

        public SettingsServiceOptions Options { get; }

        public static TempSettingsFolder Create()
            => new(Path.Combine(Path.GetTempPath(), $"EasiSlides_BootstrapMigration_{Guid.NewGuid():N}"));

        public SettingsService CreateSettings() => new(Options);

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
