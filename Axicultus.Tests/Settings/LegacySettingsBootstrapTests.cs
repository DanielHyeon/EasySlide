using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Axicultus.Wpf.Settings;
using FluentAssertions;
using Xunit;

namespace Axicultus.Wpf.Tests.Settings;

// Every path is a temp folder: the real WinForms profile and registry are never opened.
public sealed class LegacySettingsBootstrapTests
{
    private const string WinFormsProfile = """
        {
          "config": {
            "root_directory": { "kind": "String", "value": "D:\\Worship\\" },
            "licCCLI_no": { "kind": "String", "value": "1234567" }
          },
          "options": {
            "JumpToA": { "kind": "DWord", "value": 7 },
            "LyricsMonitorNotationsFontSize": { "kind": "DWord", "value": 12 },
            "LiveCamNumber": { "kind": "DWord", "value": 0 },
            "ReferenceAlertSource": { "kind": "DWord", "value": 2 },
            "Broken": { "kind": "DWord" },
            "Blob": { "kind": "Binary", "value": [1, 2] }
          },
          "monitors": {
            "LyricsMonitorShowNotations": { "kind": "DWord", "value": 1 },
            "LiveCamMute": { "kind": "DWord", "value": 1 },
            "LyricsMonitorNotationsFontSize": { "kind": "DWord", "value": 30 }
          }
        }
        """;

    [Fact]
    public void WinFormsProfile_ReadsStringAndNumberEntriesAndSkipsTheRest()
    {
        using var temp = new TempFolder();
        var path = temp.Write("Axicultuss/settings.json", WinFormsProfile);

        var source = WinFormsProfileSettingsSource.TryLoad(path);

        source.Should().NotBeNull();
        source!.TryGetString("root_directory", out var root).Should().BeTrue();
        root.Should().Be(@"D:\Worship\");
        source.TryGetString("jumptoa", out var jump).Should().BeTrue("names are case-insensitive");
        jump.Should().Be("7");
        source.TryGetString("LyricsMonitorNotationsFontSize", out var chordSize).Should().BeTrue();
        chordSize.Should().Be("30", "WinForms reads monitors before the old options copy");
        source.TryGetString("Broken", out _).Should().BeFalse();
        source.TryGetString("Blob", out _).Should().BeFalse();
        File.ReadAllText(path).Should().Be(WinFormsProfile, "the WinForms profile is only read");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("{ \"options\": ")]
    [InlineData("[1, 2]")]
    public void WinFormsProfile_MissingOrUnreadableFileGivesNoSource(string? content)
    {
        using var temp = new TempFolder();
        var path = content is null ? Path.Combine(temp.Root, "settings.json") : temp.Write("settings.json", content);

        WinFormsProfileSettingsSource.TryLoad(path).Should().BeNull();
    }

    [Fact]
    public void DefaultSource_AnUnreadableProfileFileGivesNoSourceAndNeverFallsBackToTheRegistry()
    {
        using var temp = new TempFolder();
        temp.Write("Axicultuss/settings.json", "{ \"options\": ");

        var source = LegacySettingsBootstrap.OpenDefaultSource(
            temp.Root, () => throw new InvalidOperationException("the registry is the state before this file existed"));

        source.Should().BeNull("nothing is imported, the WPF settings file is not written, and the next start tries again");
    }

    [Fact]
    public async Task FirstRun_ImportsWinFormsSettingsOnceAndNeverAgain()
    {
        using var temp = new TempFolder();
        var profile = temp.Write("Axicultuss/settings.json", WinFormsProfile);
        var options = temp.WpfOptions();
        var settings = new SettingsService(options);

        var first = await LegacySettingsBootstrap.ImportOnFirstRunAsync(
            settings, options.SettingsFilePath, () => WinFormsProfileSettingsSource.TryLoad(profile));

        first.Should().NotBeNull();
        first!.Succeeded.Should().BeTrue();
        File.Exists(options.SettingsFilePath).Should().BeTrue("the import marks itself done by writing the WPF settings");
        settings.Get(AxicultusSettingKeys.JumpToA).Should().Be(7);
        settings.Get(AxicultusSettingKeys.LicenceNumber3).Should().Be("1234567");
        settings.Get(AxicultusSettingKeys.ReferenceAlertSource).Should().Be(2);
        settings.Get(AxicultusSettingKeys.LiveCameraNumber).Should().Be(1, "a half-valid profile still imports the rest");
        settings.Current.Operator.StageShowNotations.Should().BeTrue();
        settings.Current.Operator.CameraMuted.Should().BeTrue();
        settings.Get(AxicultusSettingKeys.LyricsMonitorShowNotations).Should().BeFalse("stage chords are not congregation chords");
        settings.Get(AxicultusSettingKeys.MediaMuted).Should().BeFalse();

        // Later WinForms changes are not taken over: the WPF profile now exists.
        File.WriteAllText(profile, WinFormsProfile.Replace("\"value\": 7", "\"value\": 3"));
        var restarted = new SettingsService(options);
        var second = await LegacySettingsBootstrap.ImportOnFirstRunAsync(
            restarted, options.SettingsFilePath, () => WinFormsProfileSettingsSource.TryLoad(profile));

        second.Should().BeNull();
        restarted.Get(AxicultusSettingKeys.JumpToA).Should().Be(7);
    }

    [Fact]
    public async Task ExistingWpfSettings_AreNeverOverwrittenByTheImport()
    {
        using var temp = new TempFolder();
        var options = temp.WpfOptions();
        new SettingsService(options).Set(AxicultusSettingKeys.JumpToA, 5).Succeeded.Should().BeTrue();
        var settings = new SettingsService(options);
        var opened = false;

        var result = await LegacySettingsBootstrap.ImportOnFirstRunAsync(settings, options.SettingsFilePath, () =>
        {
            opened = true;
            return new DictionaryLegacySettingsSource(new Dictionary<string, string?> { ["JumpToA"] = "7" });
        });

        result.Should().BeNull();
        opened.Should().BeFalse();
        settings.Get(AxicultusSettingKeys.JumpToA).Should().Be(5);
    }

    [Fact]
    public async Task MissingOrFailingSource_LeavesDefaultsAndDoesNotThrow()
    {
        using var temp = new TempFolder();
        var options = temp.WpfOptions();
        var settings = new SettingsService(options);
        Exception? reported = null;

        var missing = await LegacySettingsBootstrap.ImportOnFirstRunAsync(settings, options.SettingsFilePath, () => null);
        var failing = await LegacySettingsBootstrap.ImportOnFirstRunAsync(
            settings, options.SettingsFilePath, () => throw new UnauthorizedAccessException("denied"), ex => reported = ex);

        missing.Should().BeNull();
        failing.Should().BeNull();
        reported.Should().BeOfType<UnauthorizedAccessException>();
        File.Exists(options.SettingsFilePath).Should().BeFalse("nothing was imported, so a later start may try again");
        settings.Get(AxicultusSettingKeys.JumpToA).Should().Be(AxicultusSettingKeys.JumpToA.DefaultValue);
    }

    private sealed class TempFolder : IDisposable
    {
        public string Root { get; } = Directory.CreateTempSubdirectory("Axicultus_LegacyImport_").FullName;

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
        }
    }
}
