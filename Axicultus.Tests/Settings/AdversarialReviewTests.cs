using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Axicultus.Wpf.Settings;
using FluentAssertions;
using Xunit;

namespace Axicultus.Wpf.Tests.Settings;

// Reviewer tests: each one states what the church had and is expected to FAIL against the reviewed code.
// Every path is a temp folder; the real profiles and the registry are never opened.
public sealed class AdversarialReviewTests
{
    private static string Serialize(AxicultusSettingsSnapshot snapshot)
        => JsonSerializer.Serialize(snapshot, SettingsService.JsonOptions);

    private static AxicultusSettingsSnapshot SavedProfile()
    {
        var saved = AxicultusSettingsSnapshot.CreateDefault();
        return saved with { General = saved.General with { Language = "en-US", JumpToA = 7 } };
    }

    [Fact]
    public void Load_WhenAShortcutValueHasTheWrongType_KeepsTheOtherSettings()
    {
        using var temp = new TempFolder();
        var options = temp.WpfOptions();
        Directory.CreateDirectory(Path.GetDirectoryName(options.SettingsFilePath)!);
        var json = Serialize(SavedProfile()).Replace("\"Shortcuts\": {}", "\"Shortcuts\": { \"live.next:local\": 5 }");
        json.Should().Contain("live.next:local");
        File.WriteAllText(options.SettingsFilePath, json);

        var sut = new SettingsService(options);

        sut.Get(AxicultusSettingKeys.Language).Should().Be("en-US", "one wrong-typed value must reset only that key");
        sut.Get(AxicultusSettingKeys.JumpToA).Should().Be(7);
    }

    [Fact]
    public void Load_WhenShortcutSlotsDifferOnlyByCase_StillStarts()
    {
        using var temp = new TempFolder();
        var options = temp.WpfOptions();
        Directory.CreateDirectory(Path.GetDirectoryName(options.SettingsFilePath)!);
        var json = Serialize(SavedProfile()).Replace("\"Shortcuts\": {}",
            "\"Shortcuts\": { \"live.next:local\": \"F5\", \"LIVE.NEXT:LOCAL\": \"F6\" }");
        File.WriteAllText(options.SettingsFilePath, json);

        var act = () => new SettingsService(options);

        act.Should().NotThrow("the settings service is created during startup");
    }

    [Fact]
    public async Task Import_ReadsTheStageChordSizeFromTheSectionWinFormsReads()
    {
        // MonitorRegistrySettings.ReadNotationsFontSize: "monitors" first, "options" only when monitors has none.
        using var temp = new TempFolder();
        var profile = temp.Write("Axicultuss/settings.json", """
            {
              "options": { "LyricsMonitorNotationsFontSize": { "kind": "DWord", "value": 12 } },
              "monitors": { "LyricsMonitorNotationsFontSize": { "kind": "DWord", "value": 30 } }
            }
            """);
        var options = temp.WpfOptions();
        var settings = new SettingsService(options);

        await LegacySettingsBootstrap.ImportOnFirstRunAsync(
            settings, options.SettingsFilePath, () => WinFormsProfileSettingsSource.TryLoad(profile));

        settings.Current.Operator.StageNotationPoints.Should().Be(30, "WinForms shows the monitors value");
    }

    [Fact]
    public async Task Import_TakesOverTheSongNumberingChoice()
    {
        // gfDatabase.LoadSavedData / gfUtility.SaveOptionsValues: options\UseSongNumbers.
        using var temp = new TempFolder();
        var profile = temp.Write("Axicultuss/settings.json", """
            { "options": { "UseSongNumbers": { "kind": "DWord", "value": 1 } } }
            """);
        var options = temp.WpfOptions();
        var settings = new SettingsService(options);

        await LegacySettingsBootstrap.ImportOnFirstRunAsync(
            settings, options.SettingsFilePath, () => WinFormsProfileSettingsSource.TryLoad(profile));

        settings.Get(AxicultusSettingKeys.UseSongNumbering).Should().BeTrue();
    }

    [Fact]
    public async Task Import_DoesNotReplaceAnExistingWorkingFolderWithOneThatDoesNotExist()
    {
        using var temp = new TempFolder();
        var existing = Directory.CreateDirectory(Path.Combine(temp.Root, "EasiSlides")).FullName;
        var missing = Path.Combine(temp.Root, "GoneDrive", "Worship") + "\\";
        var profile = temp.Write("Axicultuss/settings.json",
            "{ \"config\": { \"root_directory\": { \"kind\": \"String\", \"value\": " + JsonSerializer.Serialize(missing) + " } } }");
        var options = temp.WpfOptions() with { LegacyWorkingFolderPath = existing };
        var settings = new SettingsService(options);
        settings.Get(AxicultusSettingKeys.WorkingFolder).Should().Be(existing, "precondition: the existing data folder was found");

        await LegacySettingsBootstrap.ImportOnFirstRunAsync(
            settings, options.SettingsFilePath, () => WinFormsProfileSettingsSource.TryLoad(profile));

        Directory.Exists(settings.Get(AxicultusSettingKeys.WorkingFolder)).Should().BeTrue(
            "WinForms asks for a folder when root_directory is gone (Gf.ValidateRootFolder); the import must not point WPF at nothing");
    }

    [Fact]
    public async Task Import_OutOfRangeValuesBecomeWhatWinFormsShowed()
    {
        // gfDatabase.LoadSavedData: ad hoc limit > 500 -> 200; parental duration outside 1..60 -> 30; message duration outside 1..999 -> 30.
        using var temp = new TempFolder();
        var profile = temp.Write("Axicultuss/settings.json", """
            {
              "options": {
                "BibleMaxAdhocVersesSelection": { "kind": "DWord", "value": 600 },
                "ParentalAlertDuration": { "kind": "DWord", "value": 120 },
                "MessageAlertDuration": { "kind": "DWord", "value": 0 }
              }
            }
            """);
        var options = temp.WpfOptions();
        var settings = new SettingsService(options);

        await LegacySettingsBootstrap.ImportOnFirstRunAsync(
            settings, options.SettingsFilePath, () => WinFormsProfileSettingsSource.TryLoad(profile));

        settings.Get(AxicultusSettingKeys.BibleAdHocLimit).Should().Be(200);
        settings.Get(AxicultusSettingKeys.ParentalAlertDuration).Should().Be(30);
        settings.Get(AxicultusSettingKeys.MessageAlertDuration).Should().Be(30);
    }

    [Fact]
    public void FailedProfileCopy_TheRetryOnTheNextStartStillBringsTheEarlierSettings()
    {
        // App.OnStartup: copy fails -> warning "retried on the next start" -> the app runs on defaults and saves them.
        using var temp = new TempFolder();
        var previous = Directory.CreateDirectory(Path.Combine(temp.Root, "AxicultusNext")).FullName;
        File.WriteAllText(Path.Combine(previous, "settings.json"), Serialize(SavedProfile()));
        var lockedPath = Path.Combine(previous, "a-locked.bin");
        File.WriteAllText(lockedPath, "x");
        var options = temp.WpfOptions();

        using (new FileStream(lockedPath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
        {
            WpfProfileUpgrade.TryImport(temp.Root, _ => { }).Should().BeFalse("precondition: the copy fails on a root file");
        }

        // The session that followed the warning: any setting the shell stores (window size, current list) writes settings.json.
        new SettingsService(options).Set(AxicultusSettingKeys.MainWindowWidth, 1200).Succeeded.Should().BeTrue();

        WpfProfileUpgrade.TryImport(temp.Root, _ => { }).Should().BeTrue("the lock is gone on the next start");
        var restarted = new SettingsService(options);

        restarted.Get(AxicultusSettingKeys.Language).Should().Be("en-US", "the warning promised a retry on the next start");
        restarted.Get(AxicultusSettingKeys.JumpToA).Should().Be(7);
    }

    private sealed class TempFolder : IDisposable
    {
        public string Root { get; } = Directory.CreateTempSubdirectory("Axicultus_Review_").FullName;

        public string Write(string relativePath, string content)
        {
            var path = Path.Combine(Root, relativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, content);
            return path;
        }

        public SettingsServiceOptions WpfOptions()
            => new(Path.Combine(Root, "Axicultus", "settings.json"), Path.Combine(Root, "Axicultus", "Backups"));

        public void Dispose()
        {
            try { Directory.Delete(Root, recursive: true); }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
        }
    }
}
