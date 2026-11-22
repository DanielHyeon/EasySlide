using System;
using System.IO;
using Axicultus.Wpf.Settings;
using Xunit;

namespace Axicultus.Wpf.Tests.Settings;

public sealed class WpfProfileUpgradeTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void WpfStartsFromItsOwnDefaultsOrExistingJsonAndKeepsItsEdits(bool upgrade)
    {
        var appData = Directory.CreateTempSubdirectory();
        try
        {
            SettingsServiceOptions Profile(string name) => new(
                Path.Combine(appData.FullName, name, "settings.json"), Path.Combine(appData.FullName, name, "Backups"));
            var legacyFile = Path.Combine(appData.CreateSubdirectory("Axicultuss").FullName, "settings.ini");
            File.WriteAllText(legacyFile, "OutputMonitorName=WinForms projector");
            if (upgrade)
                Assert.True(new SettingsService(Profile("AxicultusNext"))
                    .Set(AxicultusSettingKeys.DefaultOutputMonitorId, "Previous WPF projector").Succeeded);

            var current = Profile("Axicultus");
            WpfProfileUpgrade.Import(appData.FullName);
            var wpf = new SettingsService(current);
            Assert.Equal(upgrade ? "Previous WPF projector" : AxicultusSettingKeys.DefaultOutputMonitorId.DefaultValue,
                wpf.Get(AxicultusSettingKeys.DefaultOutputMonitorId));
            Assert.True(wpf.Set(AxicultusSettingKeys.DefaultOutputMonitorId, "WPF projector").Succeeded);
            File.WriteAllText(legacyFile, "OutputMonitorName=Different projector");

            WpfProfileUpgrade.Import(appData.FullName);
            Assert.Equal("WPF projector", new SettingsService(current).Get(AxicultusSettingKeys.DefaultOutputMonitorId));
        }
        finally { appData.Delete(recursive: true); }
    }

    [Fact]
    public void EarlierSettingsArePendingOnlyWhileTheirCopyHasNotArrived()
    {
        var appData = Directory.CreateTempSubdirectory();
        try
        {
            Assert.False(WpfProfileUpgrade.EarlierSettingsPending(appData.FullName));
            var settings = Path.Combine(appData.CreateSubdirectory("AxicultusNext").FullName, "settings.json");
            File.WriteAllText(settings, "{}");

            using (new FileStream(settings, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
            {
                Assert.False(WpfProfileUpgrade.TryImport(appData.FullName, _ => { }));
                Assert.True(WpfProfileUpgrade.EarlierSettingsPending(appData.FullName));
            }

            Assert.True(WpfProfileUpgrade.TryImport(appData.FullName, _ => { }));
            Assert.False(WpfProfileUpgrade.EarlierSettingsPending(appData.FullName));
        }
        finally { appData.Delete(recursive: true); }
    }

    [Fact]
    public void FailedCopyIsReportedWithoutThrowingAndSettingsFileIsCopiedFirst()
    {
        var appData = Directory.CreateTempSubdirectory();
        try
        {
            var previous = appData.CreateSubdirectory("AxicultusNext");
            File.WriteAllText(Path.Combine(previous.FullName, "settings.json"), "{}");
            File.WriteAllText(Path.Combine(previous.CreateSubdirectory("Cache").FullName, "slide.bin"), "x");
            // A file where the copy needs a folder makes the copy fail part-way.
            File.WriteAllText(Path.Combine(appData.CreateSubdirectory("Axicultus").FullName, "Cache"), "blocked");
            Exception? reported = null;

            bool copied = WpfProfileUpgrade.TryImport(appData.FullName, ex => reported = ex);

            Assert.False(copied);
            Assert.NotNull(reported);
            Assert.True(File.Exists(Path.Combine(appData.FullName, "Axicultus", "settings.json")));
        }
        finally { appData.Delete(recursive: true); }
    }

    [Fact]
    public void MergesBothProfilesWithoutOverwritingOrReimportingDeletedSettings()
    {
        var temp = Directory.CreateTempSubdirectory();
        try
        {
            var oldest = temp.CreateSubdirectory("EasiSlidesNext");
            var previous = temp.CreateSubdirectory("AxicultusNext");
            string current = ProductStorage.WpfProfileDirectory(temp.FullName);
            Assert.Equal(Path.Combine(temp.FullName, "Axicultus"), current);
            Directory.CreateDirectory(current);
            File.WriteAllText(Path.Combine(current, "settings.json"), "current");
            File.WriteAllText(Path.Combine(previous.FullName, "settings.json"), "previous");
            File.WriteAllText(Path.Combine(previous.FullName, "recent.json"), "recent");
            File.WriteAllText(Path.Combine(oldest.FullName, "recent.json"), "older");
            var oldBackup = Path.Combine(oldest.CreateSubdirectory("Backups").FullName, "backup.bin");
            byte[] bytes = [0, 255, 4];
            File.WriteAllBytes(oldBackup, bytes);

            WpfProfileUpgrade.Import(temp.FullName);
            string backup = Path.Combine(current, "Backups", "backup.bin");
            Assert.Equal(bytes, File.ReadAllBytes(backup));
            Assert.Equal("current", File.ReadAllText(Path.Combine(current, "settings.json")));
            Assert.Equal("recent", File.ReadAllText(Path.Combine(current, "recent.json")));
            File.Delete(backup);
            WpfProfileUpgrade.Import(temp.FullName);
            Assert.False(File.Exists(backup));
            Assert.Equal(bytes, File.ReadAllBytes(oldBackup));
            Assert.Equal("older", File.ReadAllText(Path.Combine(oldest.FullName, "recent.json")));
        }
        finally { temp.Delete(recursive: true); }
    }

    [Fact]
    public void IncompleteImportCanRetryAndFreshInstallUsesNewName()
    {
        var temp = Directory.CreateTempSubdirectory();
        try
        {
            WpfProfileUpgrade.Import(temp.FullName);
            string current = ProductStorage.WpfProfileDirectory(temp.FullName);
            Assert.False(Directory.Exists(current));
            var source = temp.CreateSubdirectory("EasiSlidesNext").CreateSubdirectory("SessionNotes");
            File.WriteAllText(Path.Combine(source.FullName, "cue"), "기도");
            Directory.CreateDirectory(current);
            string conflictingFile = Path.Combine(current, "SessionNotes");
            File.WriteAllText(conflictingFile, "existing");
            Assert.ThrowsAny<IOException>(() => WpfProfileUpgrade.Import(temp.FullName));
            Assert.Equal("existing", File.ReadAllText(conflictingFile));
            File.Delete(conflictingFile);
            WpfProfileUpgrade.Import(temp.FullName);
            Assert.Equal("기도", File.ReadAllText(Path.Combine(current, "SessionNotes", "cue")));
        }
        finally { temp.Delete(recursive: true); }
    }
}
