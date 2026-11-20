using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Axicultus.Wpf.Settings;
using FluentAssertions;
using Xunit;

namespace Axicultus.Wpf.Tests.Settings;

public class SettingsBootstrapMigrationServiceTests
{
    [Fact]
    public async Task MigrateIfNeededAsync_WhenSettingsFileIsMissing_ImportsLegacySettings()
    {
        using var fixture = TempSettingsFolder.Create();
        var settings = fixture.CreateSettings();
        var legacy = new DictionaryLegacySettingsSource(new Dictionary<string, string?>
        {
            ["root_directory"] = @"C:\Axicultus",
            ["media_dir"] = @"C:\Axicultus\Media",
        });
        var sut = new SettingsBootstrapMigrationService(settings, legacy, fixture.Options);

        var result = await sut.MigrateIfNeededAsync();

        result.Should().NotBeNull();
        result!.Succeeded.Should().BeTrue();
        settings.Get(AxicultusSettingKeys.WorkingFolder).Should().Be(@"C:\Axicultus");
        settings.Get(AxicultusSettingKeys.MediaDirectory).Should().Be(@"C:\Axicultus\Media");
        File.Exists(fixture.Options.SettingsFilePath).Should().BeTrue();
    }

    [Fact]
    public async Task MigrateIfNeededAsync_WhenSettingsFileExists_RefreshesLegacyRuntimeSelections()
    {
        using var fixture = TempSettingsFolder.Create();
        var settings = fixture.CreateSettings();
        settings.Set(AxicultusSettingKeys.WorkingFolder, @"D:\Current");
        settings.Set(AxicultusSettingKeys.CurrentPraiseBookName, "");
        settings.Set(AxicultusSettingKeys.CurrentWorshipListName, "");
        settings.Set(AxicultusSettingKeys.UsePowerPointTab, false);
        settings.Set(AxicultusSettingKeys.UseMediaTab, false);
        settings.Set(AxicultusSettingKeys.MediaDirectory, @"D:\CurrentMedia");
        settings.Set(AxicultusSettingKeys.DefaultOutputMonitorId, "");
        settings.Set(AxicultusSettingKeys.DisplayAlwaysUseSecondaryMonitor, false);
        settings.Set(AxicultusSettingKeys.LyricsMonitorFontSize, 48);
        var legacy = new DictionaryLegacySettingsSource(new Dictionary<string, string?>
        {
            ["root_directory"] = @"C:\Legacy",
            ["current_praisebook"] = "PraiseBook 1",
            ["current_session"] = "1.주일예배",
            ["UsePowerpointTab"] = "1",
            ["UseMediaTab"] = "1",
            ["media_dir"] = @"C:\Axicultus\Media\",
            ["OutputmonitorName"] = @"\\.\DISPLAY2",
            ["AlwaysTryDualMonitor"] = "1",
            ["LyricsMonitorFontSize"] = "22",
        });
        var sut = new SettingsBootstrapMigrationService(settings, legacy, fixture.Options);

        var result = await sut.MigrateIfNeededAsync();

        result.Should().NotBeNull();
        result!.Succeeded.Should().BeTrue();
        settings.Get(AxicultusSettingKeys.WorkingFolder).Should().Be(@"D:\Current", "기존 WPF 작업 폴더는 전체 재마이그레이션으로 덮어쓰지 않는다");
        settings.Get(AxicultusSettingKeys.CurrentPraiseBookName).Should().Be("PraiseBook 1");
        settings.Get(AxicultusSettingKeys.CurrentWorshipListName).Should().Be("1.주일예배");
        settings.Get(AxicultusSettingKeys.UsePowerPointTab).Should().BeTrue();
        settings.Get(AxicultusSettingKeys.UseMediaTab).Should().BeTrue();
        settings.Get(AxicultusSettingKeys.MediaDirectory).Should().Be(@"C:\Axicultus\Media\");
        settings.Get(AxicultusSettingKeys.DefaultOutputMonitorId).Should().Be(@"\\.\DISPLAY2");
        settings.Get(AxicultusSettingKeys.DisplayAlwaysUseSecondaryMonitor).Should().BeTrue();
        settings.Get(AxicultusSettingKeys.LyricsMonitorFontSize).Should().Be(
            24,
            "WinForms registry values can store the legacy 960px-base font size below the WPF validation minimum, so bootstrap clamps it to the closest supported output size");
    }

    [Fact]
    public async Task MigrateIfNeededAsync_WhenSettingsFileExistsAndRuntimeValuesMatch_ReturnsNull()
    {
        using var fixture = TempSettingsFolder.Create();
        var settings = fixture.CreateSettings();
        settings.Set(AxicultusSettingKeys.CurrentPraiseBookName, "PraiseBook 1");
        settings.Set(AxicultusSettingKeys.CurrentWorshipListName, "1.주일예배");
        settings.Set(AxicultusSettingKeys.UsePowerPointTab, false);
        settings.Set(AxicultusSettingKeys.UseMediaTab, false);
        settings.Set(AxicultusSettingKeys.MediaDirectory, @"C:\Axicultus\Media\");
        var legacy = new DictionaryLegacySettingsSource(new Dictionary<string, string?>
        {
            ["current_praisebook"] = "PraiseBook 1",
            ["current_session"] = "1.주일예배",
            ["UsePowerpointTab"] = "0",
            ["UseMediaTab"] = "0",
            ["media_dir"] = @"C:\Axicultus\Media\",
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
        settings.Set(AxicultusSettingKeys.UsePowerPointTab, false);
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
        settings.Get(AxicultusSettingKeys.UsePowerPointTab).Should().BeFalse();
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

    [Fact]
    public async Task UpgradedProfileKeepsUserSelectionsAndStillReceivesSubsequentWinFormsChanges()
    {
        using var fixture = TempSettingsFolder.Create();
        var previous = Directory.CreateDirectory(Path.Combine(fixture.Root, "EasiSlidesNext")).FullName;
        SettingsServiceOptions Options(string path) => new(Path.Combine(path, "settings.json"), Path.Combine(path, "Backups"));
        var previousSettings = new SettingsService(Options(previous));
        previousSettings.Set(AxicultusSettingKeys.DefaultOutputMonitorId, "DISPLAY3").Succeeded.Should().BeTrue();
        WpfProfileUpgrade.Import(fixture.Root);
        var options = Options(ProductStorage.WpfProfileDirectory(fixture.Root));
        var settings = new SettingsService(options);
        var selections = new Dictionary<string, string?> { ["OutputMonitorName"] = "DISPLAY2" };
        SettingsBootstrapMigrationService Start() => new(settings, new DictionaryLegacySettingsSource(selections), options);

        (await Start().MigrateIfNeededAsync()).Should().BeNull();
        settings.Get(AxicultusSettingKeys.DefaultOutputMonitorId).Should().Be("DISPLAY3");
        selections["OutputMonitorName"] = "DISPLAY4";
        (await Start().MigrateIfNeededAsync())!.Succeeded.Should().BeTrue();
        settings.Get(AxicultusSettingKeys.DefaultOutputMonitorId).Should().Be("DISPLAY4");
        settings.Set(AxicultusSettingKeys.DefaultOutputMonitorId, "DISPLAY5").Succeeded.Should().BeTrue();
        (await Start().MigrateIfNeededAsync()).Should().BeNull();
        settings.Get(AxicultusSettingKeys.DefaultOutputMonitorId).Should().Be("DISPLAY5");
        File.Delete(options.SettingsFilePath);
        (await Start().MigrateIfNeededAsync()).Should().BeNull();
        File.Exists(options.SettingsFilePath).Should().BeFalse();
    }

    [Fact]
    public async Task ContentOnlyProfileImportsPreferencesAndTracksRuntimeChangesFromTheFirstLaunch()
    {
        using var fixture = TempSettingsFolder.Create();
        Directory.CreateDirectory(Path.Combine(fixture.Root, "EasiSlidesNext", "SessionNotes"));
        WpfProfileUpgrade.Import(fixture.Root);
        string current = ProductStorage.WpfProfileDirectory(fixture.Root);
        var options = new SettingsServiceOptions(Path.Combine(current, "settings.json"), Path.Combine(current, "Backups"));
        var settings = new SettingsService(options);
        var values = new Dictionary<string, string?> { ["root_directory"] = @"C:\Library", ["current_session"] = "Sunday" };
        SettingsBootstrapMigrationService Start() => new(settings, new DictionaryLegacySettingsSource(values), options);

        (await Start().MigrateIfNeededAsync())!.Succeeded.Should().BeTrue();
        settings.Get(AxicultusSettingKeys.WorkingFolder).Should().Be(@"C:\Library");
        values["current_session"] = "Wednesday";
        (await Start().MigrateIfNeededAsync())!.Succeeded.Should().BeTrue();
        settings.Get(AxicultusSettingKeys.CurrentWorshipListName).Should().Be("Wednesday");
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
            => new(Path.Combine(Path.GetTempPath(), $"Axicultus_BootstrapMigration_{Guid.NewGuid():N}"));

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
