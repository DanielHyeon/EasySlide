using System.IO;
using Axicultus.Wpf.Settings;
using Xunit;

namespace Axicultus.Wpf.Tests.Settings;

public sealed class WpfProfileUpgradeTests
{
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
