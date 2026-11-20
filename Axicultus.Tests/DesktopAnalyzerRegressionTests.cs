using System;
using System.Data.SQLite;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Axicultus.Util;
using System.Windows.Forms;
using Axicultus.Global;
using Xunit;

namespace Axicultus.Tests;

public class DesktopAnalyzerRegressionTests
{
    [Fact]
    public void SelectionHandlesMissingAndUnselectedTabs()
    {
        MonitorPresentationTests.RunOnSta(() =>
        {
            using var tabs = new TabControl();
            using var page = new TabPage { Name = "tabImages" };
            tabs.TabPages.Add(page);
            _ = tabs.Handle;
            tabs.SelectedIndex = -1;
            Assert.False(FrmMain.IsSelectedTab(null, page.Name));
            Assert.False(FrmMain.IsSelectedTab(tabs, page.Name));
            tabs.SelectedTab = page;
            Assert.True(FrmMain.IsSelectedTab(tabs, "TABIMAGES"));
        });
    }

    [Theory]
    [InlineData(-1, false)]
    [InlineData(0, true)]
    public void SelectionIsReadOnTheOwningUiThread(int selectedIndex, bool expected)
    {
        MonitorPresentationTests.RunOnSta(() =>
        {
            using var tabs = new TabControl();
            tabs.TabPages.Add(new TabPage { Name = "Files" });
            _ = tabs.Handle;
            tabs.SelectedIndex = selectedIndex;
            var lookup = Task.Run(() => FrmMain.IsSelectedTab(tabs, "FILES"));
            var elapsed = Stopwatch.StartNew();
            while (!lookup.IsCompleted && elapsed.Elapsed < TimeSpan.FromSeconds(5))
            {
                Application.DoEvents();
                Thread.Yield();
            }
            Assert.True(lookup.IsCompleted);
            Assert.Equal(expected, lookup.GetAwaiter().GetResult());
        });
    }

    [Fact]
    public void RestoredTabsFollowTheirAnchorAndMissingAnchorsAppend()
    {
        MonitorPresentationTests.RunOnSta(() =>
        {
            using var tabs = new TabControl();
            string[] order = ["Files", "PPT", "Bibles", "Images", "Media", "Default"];
            var pages = order.Select(name => new TabPage { Name = name }).ToArray();
            tabs.TabPages.AddRange(pages);
            foreach (bool show in new[] { false, true, true })
            {
                FrmMain.SetOneTab(tabs, pages[1], show, "FILES");
                FrmMain.SetOneTab(tabs, pages[4], show, "Images");
            }
            Assert.Equal(order, tabs.TabPages.Cast<TabPage>().Select(p => p.Name));
            FrmMain.SetOneTab(tabs, pages[1], false, "Missing");
            FrmMain.SetOneTab(tabs, pages[1], true, "Missing");
            Assert.Same(pages[1], tabs.TabPages[tabs.TabCount - 1]);
        });
    }

    [Theory]
    [InlineData("개역", true)]
    [InlineData("개역개정", false)]
    [InlineData("A", true)]
    [InlineData("a;b", false)]
    [InlineData("   ", false)]
    public void BibleRenameMatchesWholeNames(string proposed, bool allowed)
    {
        MonitorPresentationTests.RunOnSta(() =>
        {
            using var dialog = new FrmBibleRename("현재", ["개역개정", "A;B"]);
            Assert.Equal(allowed, dialog.CanUseName(proposed));
        });
    }

    [Fact]
    public void FolderRenameChecksTheLastSlotAndAllowsItsOwnName()
    {
        FolderSettings[] folders = [new(), new() { RenameName = "Other" }, new(), new() { RenameName = "Last" }];
        Assert.False(FrmUpdateFileName.TryRenameFolder(folders, 1, "last"));
        Assert.Equal("Other", folders[1].RenameName);
        Assert.True(FrmUpdateFileName.TryRenameFolder(folders, 3, "LAST"));
        Assert.Equal("LAST", folders[3].RenameName);
        Assert.False(FrmUpdateFileName.TryRenameFolder(folders, folders.Length, "Missing"));
    }

    [Fact]
    public void TimingMigrationAddsMscUnderTurkishCulture()
    {
        var before = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("tr-TR");
            using var db = new SQLiteConnection("Data Source=:memory:");
            db.Open();
            using var query = new SQLiteCommand("CREATE TABLE Songs(Id INTEGER)", db);
            query.ExecuteNonQuery();
            var connection = db;
            Axicultus.SQLite.DbController.CreateField(ref connection, "Songs", "Timing", 0);
            query.CommandText = "SELECT count(*) FROM pragma_table_info('Songs') WHERE name = 'MSC'";
            Assert.Equal(1L, query.ExecuteScalar());
        }
        finally { CultureInfo.CurrentCulture = before; }
    }

    [Theory]
    [InlineData("Timing TEXT")]
    [InlineData("MSC TEXT")]
    public void SongSchemaRepairsPartialMigrationAndPreservesExistingData(string existingColumn)
    {
        using var db = new SQLiteConnection("Data Source=:memory:");
        db.Open();
        using var query = new SQLiteCommand($"CREATE TABLE SONG (Id INTEGER, {existingColumn}); INSERT INTO SONG VALUES (1,'kept')", db);
        query.ExecuteNonQuery();
        Assert.True(Gf.ValidateSongTableFields(db));
        Assert.True(Gf.ValidateSongTableFields(db));
        query.CommandText = "SELECT count(*) FROM pragma_table_info('SONG') WHERE upper(name) IN ('TIMING','MSC')";
        Assert.Equal(2L, query.ExecuteScalar());
        query.CommandText = "SELECT " + existingColumn.Split(' ')[0] + " FROM SONG WHERE Id = 1";
        Assert.Equal("kept", query.ExecuteScalar());
    }

    [Fact]
    public void IncompleteReadOnlySchemaReportsFailure()
    {
        using var db = new SQLiteConnection("Data Source=:memory:");
        db.Open();
        using var query = new SQLiteCommand("CREATE TABLE SONG (Timing TEXT); PRAGMA query_only=ON", db);
        query.ExecuteNonQuery();
        Assert.False(Gf.ValidateSongTableFields(db));
    }

    [Fact]
    public void MultiStringDevicePropertiesKeepAllNamesAndFitAtExactCapacity()
    {
        var destination = new StringBuilder(3);
        Assert.True(DiNative.CopyDevicePropertyText(7, Encoding.Unicode.GetBytes("A\0B\0\0"), destination));
        Assert.Equal("A\0B", destination.ToString());
        Assert.False(DiNative.CopyDevicePropertyText(1, Encoding.Unicode.GetBytes("long\0"), destination));
        Assert.Equal("A\0B", destination.ToString());
    }

    [Fact]
    public void DevicePropertiesUseTheNativePointerSizeAndReleaseTheDeviceSet()
    {
        Assert.Equal(IntPtr.Size == 8 ? 32 : 28, Marshal.SizeOf<DiNative.SP_DEVINFO_DATA>());
        Guid classGuid = Guid.Empty;
        nint devices = DiNative.SetupDiGetClassDevs(ref classGuid, 0, 0, DiNative.DIGCF_ALLCLASSES | DiNative.DIGCF_PRESENT);
        Assert.NotEqual((nint)(-1), devices);
        try
        {
            var device = new DiNative.SP_DEVINFO_DATA { cbSize = Marshal.SizeOf<DiNative.SP_DEVINFO_DATA>() };
            Assert.True(DiNative.SetupDiEnumDeviceInfo(devices, 0, device));
            var name = new StringBuilder(1024);
            Assert.True(DiNative.SetupDiGetDeviceRegistryProperty(devices, device, DiNative.SPDRP_DEVICEDESC, 0, name, uint.MaxValue, 0));
            Assert.True(name.Length > 0);
            var exact = new StringBuilder(name.Length);
            Assert.True(DiNative.SetupDiGetDeviceRegistryProperty(devices, device, DiNative.SPDRP_DEVICEDESC, 0, exact, (uint)((name.Length + 1) * sizeof(char)), 0));
            Assert.Equal(name.ToString(), exact.ToString());
        }
        finally { Assert.NotEqual(0, DiNative.SetupDiDestroyDeviceInfoList(devices)); }
    }

    [DllImport("shell32.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
    private static extern nint FindExecutableW(string file, string directory, [Out] StringBuilder result);

    [Fact]
    public void PowerPointAssociationAgreesForAsciiAndUnicodePaths()
    {
        var directory = Directory.CreateTempSubdirectory("Slides-");
        try
        {
            string ascii = Path.Combine(directory.FullName, "ordinary.ppt");
            string unicode = Path.Combine(directory.FullName, "가사 🎵.ppt");
            File.WriteAllText(ascii, "");
            File.WriteAllText(unicode, "");
            bool expected = FindExecutableW(ascii, null, new StringBuilder(260)) > 32;
            Assert.Equal(expected, Gf.HasAssociatedExecutable(ascii));
            Assert.Equal(expected, Gf.HasAssociatedExecutable(unicode));
            Assert.False(Gf.HasAssociatedExecutable(Path.Combine(directory.FullName, "missing.ppt")));
        }
        finally { directory.Delete(true); }
    }
}
