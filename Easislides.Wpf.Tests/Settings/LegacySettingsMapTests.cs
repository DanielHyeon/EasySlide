using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Easislides.Wpf.Settings;
using FluentAssertions;
using Xunit;

namespace Easislides.Wpf.Tests.Settings;

public class LegacySettingsMapTests
{
    [Fact]
    public void AllCurrentSettingKeys_AreRepresentedInLegacyInventory()
    {
        var mappedIds = LegacySettingsMap.Entries
            .Where(entry => entry.WpfKeyId is not null)
            .Select(entry => entry.WpfKeyId)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var setting in EasiSettingKeys.All)
        {
            mappedIds.Should().Contain(GetSettingId(setting));
        }
    }

    [Fact]
    public void FrmOptionsSaveVariables_HighRiskKeysAreDocumented()
    {
        var legacyKeys = LegacySettingsMap.Entries
            .Select(entry => entry.LegacyKey)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        legacyKeys.Should().Contain([
            "UsePowerpointTab",
            "NoPowerpointPanelOverlay",
            "UseMediaTab",
            "ShowLyricsMonitorAlertBox",
            "AdvanceNextItem",
            "GapItemOption",
            "DMAlwaysUseSecondaryMonitor",
            "OutputMonitorName",
            "LMTextColour",
            "LiveCamVolume",
            "MediaDir",
            "GlobalHookKey_F7",
        ]);
    }

    [Fact]
    public void AutomatedAliases_ExposeLegacyAndCompatibilityNames()
    {
        LegacySettingsMap.GetAutomatedAliases(EasiSettingKeys.WorkingFolder.Id)
            .Should().Contain(["WorkingFolder", "RootEasiSlidesDir", "root_directory"]);
        LegacySettingsMap.GetAutomatedAliases(EasiSettingKeys.DefaultOutputMonitorId.Id)
            .Should().Contain(["DefaultOutputMonitorId", "OutputMonitorName"]);
        LegacySettingsMap.GetAutomatedAliases(EasiSettingKeys.MediaVolume.Id)
            .Should().Contain(["MediaVolume", "LiveCamVolume"]);
    }

    [Fact]
    public async Task MigrateLegacyAsync_UsesLegacyAliasesAndNormalizesMediaScales()
    {
        using var fixture = TempSettingsFolder.Create();
        var sut = fixture.CreateService();
        var legacy = new DictionaryLegacySettingsSource(new Dictionary<string, string?>
        {
            ["root_directory"] = fixture.LegacyFolder,
            ["OutputMonitorName"] = @"\\.\DISPLAY2",
            ["LiveCamVolume"] = "65",
            ["LiveCamBalance"] = "-25",
            ["LiveCamMute"] = "1",
            ["DBFileName"] = Path.Combine(fixture.LegacyFolder, "AdminDB", "EasiSlidesDb.db"),
        });

        var result = await sut.MigrateLegacyAsync(legacy);

        result.Succeeded.Should().BeTrue();
        sut.Get(EasiSettingKeys.WorkingFolder).Should().Be(fixture.LegacyFolder);
        sut.Get(EasiSettingKeys.DefaultOutputMonitorId).Should().Be(@"\\.\DISPLAY2");
        sut.Get(EasiSettingKeys.MediaVolume).Should().Be(0.65);
        sut.Get(EasiSettingKeys.MediaBalance).Should().Be(-0.25);
        sut.Get(EasiSettingKeys.MediaMuted).Should().BeTrue();
        sut.Get(EasiSettingKeys.AdminDatabasePath).Should().EndWith(Path.Combine("AdminDB", "EasiSlidesDb.db"));
    }

    private static string GetSettingId(object setting)
        => Convert.ToString(setting.GetType().GetProperty(nameof(EasiSettingKeys.Language.Id))?.GetValue(setting))
           ?? throw new InvalidOperationException("Setting key id is missing.");

    private sealed class TempSettingsFolder : IDisposable
    {
        private TempSettingsFolder(string root)
        {
            Root = root;
            LegacyFolder = Path.Combine(root, "LegacyRoot");
            Directory.CreateDirectory(LegacyFolder);
        }

        public string Root { get; }

        public string LegacyFolder { get; }

        public static TempSettingsFolder Create()
            => new(Path.Combine(Path.GetTempPath(), $"EasiSlides_LegacySettings_{Guid.NewGuid():N}"));

        public SettingsService CreateService()
            => new(new SettingsServiceOptions(
                Path.Combine(Root, "settings.json"),
                Path.Combine(Root, "Backups")));

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
