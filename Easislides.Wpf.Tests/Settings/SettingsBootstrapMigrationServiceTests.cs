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
    public async Task MigrateIfNeededAsync_WhenSettingsFileExists_RefreshesLegacyRuntimeSelections()
    {
        using var fixture = TempSettingsFolder.Create();
        var settings = fixture.CreateSettings();
        settings.Set(EasiSettingKeys.WorkingFolder, @"D:\Current");
        settings.Set(EasiSettingKeys.CurrentPraiseBookName, "");
        settings.Set(EasiSettingKeys.CurrentWorshipListName, "");
        settings.Set(EasiSettingKeys.UsePowerPointTab, false);
        settings.Set(EasiSettingKeys.UseMediaTab, false);
        settings.Set(EasiSettingKeys.MediaDirectory, @"D:\CurrentMedia");
        var legacy = new DictionaryLegacySettingsSource(new Dictionary<string, string?>
        {
            ["root_directory"] = @"C:\Legacy",
            ["current_praisebook"] = "PraiseBook 1",
            ["current_session"] = "1.주일예배",
            ["UsePowerpointTab"] = "1",
            ["UseMediaTab"] = "1",
            ["media_dir"] = @"C:\EasiSlides\Media\",
        });
        var sut = new SettingsBootstrapMigrationService(settings, legacy, fixture.Options);

        var result = await sut.MigrateIfNeededAsync();

        result.Should().NotBeNull();
        result!.Succeeded.Should().BeTrue();
        settings.Get(EasiSettingKeys.WorkingFolder).Should().Be(@"D:\Current", "기존 WPF 작업 폴더는 전체 재마이그레이션으로 덮어쓰지 않는다");
        settings.Get(EasiSettingKeys.CurrentPraiseBookName).Should().Be("PraiseBook 1");
        settings.Get(EasiSettingKeys.CurrentWorshipListName).Should().Be("1.주일예배");
        settings.Get(EasiSettingKeys.UsePowerPointTab).Should().BeTrue();
        settings.Get(EasiSettingKeys.UseMediaTab).Should().BeTrue();
        settings.Get(EasiSettingKeys.MediaDirectory).Should().Be(@"C:\EasiSlides\Media\");
    }

    [Fact]
    public async Task MigrateIfNeededAsync_WhenSettingsFileExistsAndRuntimeValuesMatch_ReturnsNull()
    {
        using var fixture = TempSettingsFolder.Create();
        var settings = fixture.CreateSettings();
        settings.Set(EasiSettingKeys.CurrentPraiseBookName, "PraiseBook 1");
        settings.Set(EasiSettingKeys.CurrentWorshipListName, "1.주일예배");
        settings.Set(EasiSettingKeys.UsePowerPointTab, false);
        settings.Set(EasiSettingKeys.UseMediaTab, false);
        settings.Set(EasiSettingKeys.MediaDirectory, @"C:\EasiSlides\Media\");
        var legacy = new DictionaryLegacySettingsSource(new Dictionary<string, string?>
        {
            ["current_praisebook"] = "PraiseBook 1",
            ["current_session"] = "1.주일예배",
            ["UsePowerpointTab"] = "0",
            ["UseMediaTab"] = "0",
            ["media_dir"] = @"C:\EasiSlides\Media\",
        });
        var sut = new SettingsBootstrapMigrationService(settings, legacy, fixture.Options);

        var result = await sut.MigrateIfNeededAsync();

        result.Should().BeNull();
    }

    [Fact]
    public async Task MigrateIfNeededAsync_WhenExistingRegistryBoolIsInvalid_ReturnsWarning()
    {
        using var fixture = TempSettingsFolder.Create();
        var settings = fixture.CreateSettings();
        settings.Set(EasiSettingKeys.UsePowerPointTab, false);
        var legacy = new DictionaryLegacySettingsSource(new Dictionary<string, string?>
        {
            ["UsePowerpointTab"] = "maybe",
        });
        var sut = new SettingsBootstrapMigrationService(settings, legacy, fixture.Options);

        var result = await sut.MigrateIfNeededAsync();

        result.Should().NotBeNull();
        result!.Succeeded.Should().BeTrue();
        result.Issues.Should().ContainSingle(issue =>
            issue.Key == "UsePowerpointTab" && issue.Severity == SettingsIssueSeverity.Warning);
        settings.Get(EasiSettingKeys.UsePowerPointTab).Should().BeFalse();
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
