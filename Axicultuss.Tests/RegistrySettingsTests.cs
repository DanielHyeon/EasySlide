using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using Axicultus.Util;
using Microsoft.Win32;
using Xunit;

namespace Axicultus.Tests;

public class RegistrySettingsTests
{
    [Fact]
    public void RecordsAccessErrorsSeparatelyFromMissingValues()
    {
        var store = new MemoryStore { DeniedRead = ("options", "ModernMainUi"), DeniedWrite = ("monitors", "Test"), DeniedDelete = ("options", "Test") };
        var log = new List<string>();
        var settings = new RegistrySettings(store, log.Add);

        Assert.True(settings.TryReadValue("options", "Missing", out object missing));
        Assert.Null(missing);
        Assert.False(settings.TryReadValue("options", "ModernMainUi", out object denied));
        Assert.Null(denied);
        Assert.False(settings.TryWriteValue("monitors", "Test", 0, RegistryValueKind.DWord));
        Assert.False(settings.TryDeleteValue("options", "Test"));
        Assert.Collection(log,
            entry => Assert.Contains(@"read options\ModernMainUi", entry),
            entry => Assert.Contains(@"write monitors\Test", entry),
            entry => Assert.Contains(@"delete options\Test", entry));
    }

    [Fact]
    public void KeepsLegacyMonitorValuesOnPartialWriteAndRetriesWithoutOverwritingNewValues()
    {
        const string flag = "AlwaysTryDualMonitor";
        const string left = "DualMonitorOptionCustomLeft";
        const string width = "DualMonitorOptionCustomWidth";
        var store = new MemoryStore { DeniedWrite = ("monitors", width) };
        store.Values[("options", flag)] = 1;
        store.Values[("options", left)] = -1920;
        store.Values[("options", width)] = 1920;
        store.Values[("monitors", left)] = 2560;
        var log = new List<string>();
        var settings = new RegistrySettings(store, log.Add);

        MonitorRegistrySettings.MigrateLegacyDualMonitorValues(settings);

        Assert.Equal(3, store.Values.Keys.Count(key => key.Section == "options"));
        Assert.False(store.Values.ContainsKey(("monitors", width)));
        Assert.Equal(1920, MonitorRegistrySettings.ReadDualMonitorValue(settings, width, 1));
        Assert.Contains(@"write monitors\" + width, Assert.Single(log));

        store.DeniedWrite = null;
        MonitorRegistrySettings.MigrateLegacyDualMonitorValues(settings);

        Assert.DoesNotContain(store.Values.Keys, key => key.Section == "options");
        Assert.Equal(2560, store.Values[("monitors", left)]);
        Assert.Equal(1920, store.Values[("monitors", width)]);
    }

    [Fact]
    public void DeniedDestinationReadLeavesLegacyAndCurrentValuesIntact()
    {
        const string flag = "AlwaysTryDualMonitor";
        var store = new MemoryStore { DeniedRead = ("monitors", flag) };
        store.Values[("options", flag)] = 1;
        store.Values[("monitors", flag)] = 0;
        var log = new List<string>();
        var settings = new RegistrySettings(store, log.Add);

        MonitorRegistrySettings.MigrateLegacyDualMonitorValues(settings);

        Assert.Equal(1, store.Values[("options", flag)]);
        Assert.Equal(0, store.Values[("monitors", flag)]);
        Assert.Contains(@"read monitors\" + flag, Assert.Single(log));
    }

    [Fact]
    public void KeepsMigrationMarkerWhenCleanupFails()
    {
        const string flag = "AlwaysTryDualMonitor";
        const string width = "DualMonitorOptionCustomWidth";
        var store = new MemoryStore { DeniedDelete = ("options", width) };
        store.Values[("options", flag)] = 0;
        store.Values[("options", width)] = 1920;
        var settings = new RegistrySettings(store, _ => { });

        MonitorRegistrySettings.MigrateLegacyDualMonitorValues(settings);
        Assert.Equal(0, store.Values[("options", flag)]);
        Assert.Equal(1920, store.Values[("monitors", width)]);

        store.DeniedDelete = null;
        MonitorRegistrySettings.MigrateLegacyDualMonitorValues(settings);
        Assert.DoesNotContain(store.Values.Keys, key => key.Section == "options");
        Assert.Equal(0, store.Values[("monitors", flag)]);
    }

    [Fact]
    public void UsesTheSavedNotationsFontSizeOnReloadAndSupportsThePreviousKey()
    {
        const string fontSize = "LyricsMonitorNotationsFontSize";
        var store = new MemoryStore();
        var settings = new RegistrySettings(store, _ => { });
        Assert.Equal(22, MonitorRegistrySettings.ReadNotationsFontSize(settings));

        store.Values[("options", fontSize)] = 24;
        Assert.Equal(24, MonitorRegistrySettings.ReadNotationsFontSize(settings));

        MonitorRegistrySettings.SaveNotationsFontSize(settings, 32);
        var restarted = new RegistrySettings(store, _ => { });

        Assert.Equal(32, MonitorRegistrySettings.ReadNotationsFontSize(restarted));
        Assert.Equal(32, store.Values[("monitors", fontSize)]);
        Assert.Equal(24, store.Values[("options", fontSize)]);

        store.Values[("monitors", fontSize)] = 99;
        Assert.Equal(20, MonitorRegistrySettings.ReadNotationsFontSize(restarted));
    }

    [Theory]
    [InlineData(45, 45)]
    [InlineData(999, 999)]
    [InlineData(1000, 0)]
    [InlineData(-3, 0)]
    public void RotateGapOutsideItsRangeIsReadAsZero(int stored, int expected)
    {
        var store = new MemoryStore();
        store.Values[("options", "RotateGap")] = stored;

        Assert.Equal(expected, MonitorRegistrySettings.ReadRotateGap(new RegistrySettings(store, _ => { })));
    }

    [Theory]
    [InlineData(2, Axicultus.Module.GapType.Default)]
    [InlineData(9, Axicultus.Module.GapType.None)]
    [InlineData(-1, Axicultus.Module.GapType.None)]
    public void UnknownGapScreenValueIsReadAsNone(int stored, Axicultus.Module.GapType expected)
    {
        var store = new MemoryStore();
        store.Values[("options", "GapItemOption")] = stored;

        Assert.Equal(expected, MonitorRegistrySettings.ReadGapItemOption(new RegistrySettings(store, _ => { })));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void MissingPowerPointCanStillBeReported(string registeredPath)
    {
        Assert.Null(new OfficeVersion((_, _) => registeredPath).GetVersion());
    }

    internal sealed class MemoryStore : IRegistrySettingsStore
    {
        internal Dictionary<(string Section, string Name), object> Values { get; } = new();
        internal (string, string)? DeniedRead { get; set; }
        internal (string, string)? DeniedWrite { get; set; }
        internal (string, string)? DeniedDelete { get; set; }

        public object ReadValue(string section, string name) =>
            DeniedRead == (section, name) ? throw new SecurityException() : Values.GetValueOrDefault((section, name));

        public void WriteValue(string section, string name, object value, RegistryValueKind kind)
        {
            if (DeniedWrite == (section, name))
                throw new UnauthorizedAccessException();
            Values[(section, name)] = value;
        }

        public void DeleteValue(string section, string name)
        {
            if (DeniedDelete == (section, name))
                throw new UnauthorizedAccessException();
            Values.Remove((section, name));
        }
    }
}
