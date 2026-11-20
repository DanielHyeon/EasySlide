using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using Axicultus.Module;
using Axicultus.Util;
using Microsoft.Win32;
using Xunit;

namespace Axicultus.Tests;

public sealed class ProductIdentityTests
{
    [Fact]
    public void BinariesExposeNewProductIdentity()
    {
        var assembly = typeof(FrmMain).Assembly;
        Assert.Equal("Axicultuss", assembly.GetName().Name);
        Assert.Equal("Axicultuss", assembly.GetCustomAttribute<AssemblyProductAttribute>()?.Product);
        Assert.Equal("Axicultuss", assembly.GetCustomAttribute<AssemblyTitleAttribute>()?.Title);
        Assert.Equal("Axicultuss.Tests", typeof(ProductIdentityTests).Assembly.GetName().Name);
        Assert.Equal("Axicultus.Core", typeof(SongExchangeFile).Assembly.GetName().Name);
    }

    [Theory]
    [InlineData(16)]
    [InlineData(32)]
    public void AboutWindowUsesTheExecutableIcon(int size)
    {
        MonitorPresentationTests.RunOnSta(() =>
        {
            string executable = Path.ChangeExtension(typeof(FrmMain).Assembly.Location, ".exe");
            using var shellIcon = Icon.ExtractIcon(executable, 0, size);
            Assert.NotNull(shellIcon);
            using var window = new FrmAbout();
            Assert.Equal("About Axicultuss", window.Text);
            using var frame = new Icon(window.Icon, size, size);
            using var expected = shellIcon.ToBitmap();
            using var actual = frame.ToBitmap();
            Assert.Equal(new Size(size, size), actual.Size);
            Assert.Equal(expected.Size, actual.Size);
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
                Assert.Equal(expected.GetPixel(x, y).ToArgb(), actual.GetPixel(x, y).ToArgb());
        });
    }

    [Theory]
    [InlineData("Axicultus")]
    [InlineData("EasiSlides")]
    public void SongExchangeReadsBothProductGenerations(string product)
    {
        string file = Path.GetTempFileName();
        try
        {
            File.WriteAllText(file, $"<{product}><Item><Title1>믿음</Title1></Item></{product}>");
            Assert.Equal("믿음", Assert.Single(SongExchangeFile.ReadSongs(file)).Title1);
        }
        finally { File.Delete(file); }
    }

    [Fact]
    public void ExistingProfilesDataAndHelpCanBeUsedInPlace()
    {
        var temp = Directory.CreateTempSubdirectory();
        try
        {
            string root = temp.FullName;
            string current = Path.Combine(root, "Axicultus");
            Assert.Equal(current, ProductStorage.WorkingDirectory(root));
            string previous = Directory.CreateDirectory(Path.Combine(root, "EasiSlides")).FullName;
            Assert.Equal(previous, ProductStorage.WorkingDirectory(root));
            Directory.CreateDirectory(current);
            Assert.Equal(current, ProductStorage.WorkingDirectory(root));

            var dbFolder = Directory.CreateDirectory(Path.Combine(root, "Admin", "Database"));
            string oldDb = Path.Combine(dbFolder.FullName, "EasiSlidesDb.db");
            File.WriteAllText(oldDb, "previous");
            Assert.Equal(oldDb, ProductStorage.SongDatabase(root));
            string newDb = Path.Combine(dbFolder.FullName, "AxicultusDb.db");
            File.WriteAllText(newDb, "current");
            Assert.Equal(newDb, ProductStorage.SongDatabase(root));
            Assert.Equal("previous", File.ReadAllText(oldDb));

            string help = Path.Combine(root, "EasiSlidesHelp.chm");
            File.WriteAllText(help, "previous");
            Assert.Equal(help, ProductStorage.HelpFileIn(root));
            string currentHelp = Path.Combine(root, "AxicultusHelp.chm");
            File.WriteAllText(currentHelp, "current");
            Assert.Equal(currentHelp, ProductStorage.HelpFileIn(root));

            string name = Guid.NewGuid().ToString("N") + ".jpg";
            string oldImage = Path.Combine(@"C:\EasiSlides\Images\Tiles", name);
            Assert.Equal(oldImage, ProductStorage.ResolveLegacyBackground(oldImage, root));
            string relocated = Path.Combine(Directory.CreateDirectory(Path.Combine(root, "Images", "Tiles")).FullName, name);
            File.WriteAllBytes(relocated, []);
            Assert.Equal(relocated, ProductStorage.ResolveLegacyBackground(oldImage, root));
            Assert.Equal(relocated, ProductStorage.ResolveLegacyBackground(relocated, root));
        }
        finally { temp.Delete(recursive: true); }
    }

    [Fact]
    public void RegistryUpgradePreservesExistingValuesAndResetDoesNotReimport()
    {
        string sandbox = @"Software\Axicultus.Tests\" + Guid.NewGuid().ToString("N");
        try
        {
            using var root = Registry.CurrentUser.CreateSubKey(sandbox);
            using var old = root.CreateSubKey(@"Software\EasiSlides\config");
            old.SetValue("Selected", 1);
            old.SetValue("Path", @"%TEMP%\Music", RegistryValueKind.ExpandString);
            using (var intermediate = root.CreateSubKey(@"Software\Axicultus\config"))
            {
                intermediate.SetValue("Priority", 8);
                intermediate.SetValue("", new byte[] { 0, 250 }, RegistryValueKind.Binary);
            }
            old.SetValue("Priority", 4);
            using (var current = root.CreateSubKey(@"Software\Axicultuss\config"))
                current.SetValue("Selected", 2);

            RegistryProfileUpgrade.Import(root);
            old.SetValue("Later", 3);
            RegistryProfileUpgrade.Import(root);
            using (var current = root.OpenSubKey(@"Software\Axicultuss\config"))
            {
                Assert.Equal(2, current.GetValue("Selected"));
                Assert.Equal(8, current.GetValue("Priority"));
                Assert.Equal(new byte[] { 0, 250 }, Assert.IsType<byte[]>(current.GetValue("")));
                Assert.Equal(RegistryValueKind.ExpandString, current.GetValueKind("Path"));
                Assert.Equal(@"%TEMP%\Music", current.GetValue("Path", null, RegistryValueOptions.DoNotExpandEnvironmentNames));
                Assert.Null(current.GetValue("Later"));
            }

            RegistryProfileUpgrade.Reset(root);
            RegistryProfileUpgrade.Import(root);
            using var cleared = root.OpenSubKey(@"Software\Axicultuss\config");
            Assert.Null(cleared);
            Assert.Equal(1, old.GetValue("Selected"));
        }
        finally { Registry.CurrentUser.DeleteSubKeyTree(sandbox, throwOnMissingSubKey: false); }
    }

    [Fact]
    public void ClearedIntermediateProfileDoesNotResurrectOriginalValues()
    {
        string sandbox = @"Software\Axicultus.Tests\" + Guid.NewGuid().ToString("N");
        try
        {
            using var root = Registry.CurrentUser.CreateSubKey(sandbox);
            using var oldest = root.CreateSubKey(@"Software\EasiSlides\config");
            oldest.SetValue("Removed", 18);
            using var previous = root.CreateSubKey(@"Software\Axicultus");
            previous.SetValue("LegacyProfileImported", 1);
            RegistryProfileUpgrade.Import(root);
            using var current = root.OpenSubKey(@"Software\Axicultuss");
            Assert.NotNull(current);
            Assert.Null(current.OpenSubKey("config"));
            Assert.Null(current.GetValue("LegacyProfileImported"));
            Assert.Equal(18, oldest.GetValue("Removed"));
        }
        finally { Registry.CurrentUser.DeleteSubKeyTree(sandbox, throwOnMissingSubKey: false); }
    }
}
