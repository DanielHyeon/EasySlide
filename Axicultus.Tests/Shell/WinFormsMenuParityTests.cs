using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Xunit;

namespace Axicultus.Wpf.Tests.Shell;

public class WinFormsMenuParityTests
{
    private static readonly string Root = FindRepositoryRoot();
    private static readonly string Designer = File.ReadAllText(Path.Combine(Root, "Axicultuss", "Forms", "FrmMain.Designer.cs"));
    private static readonly XElement MainMenu = XDocument.Load(Path.Combine(Root, "Axicultus", "MainWindow.xaml"))
        .Descendants().First(element => element.Name.LocalName == "Menu" && element.Elements().Any(item => (string?)item.Attribute("Header") == "File"));

    [Theory]
    [InlineData("File", "Menu_WorshipSessions", "Click", "OpenManageWorshipLists_Click")]
    [InlineData("File", "Menu_PraiseBookTemplates", "Click", "OpenPraiseBookIndex_Click")]
    [InlineData("File", "Menu_ListingOfSelectedFolder", "Click", "ExportFolderListing_Click")]
    [InlineData("File", "Menu_EditHistoryList", "SubmenuOpened", "RecentEdits_Open")]
    [InlineData("File", "Menu_Exit", "Click", "Exit_Click")]
    [InlineData("Edit", "Menu_AddSong", "Click", "Main_SourceEdit_Click", "Main_New")]
    [InlineData("Edit", "Menu_EditSong", "Click", "Main_SourceEdit_Click", "Main_Edit")]
    [InlineData("Edit", "Menu_CopySong", "Click", "Main_SourceEdit_Click", "Main_Copy")]
    [InlineData("Edit", "Menu_MoveSong", "Click", "Main_SourceEdit_Click", "Main_Move")]
    [InlineData("Edit", "Menu_DeleteSong", "Click", "Main_SourceEdit_Click", "Main_Delete")]
    [InlineData("Edit", "Menu_SelectAll", "Click", "CMenuSongs_SelectAll_Click")]
    [InlineData("Edit", "Menu_Find", "Click", "Main_Find_Click")]
    [InlineData("Edit", "Menu_UseSongNumbering", "IsChecked", "{Binding Library.UseSongNumbering, Mode=TwoWay}")]
    [InlineData("Edit", "Menu_ReArrangeSongFolders", "Click", "FolderEditor_Click")]
    [InlineData("View", "Menu_AxicultusFolder", "Click", "OpenAxicultusFolder_Click")]
    [InlineData("View", "Menu_Options", "Click", "OpenSettings_Click")]
    [InlineData("View", "Menu_Refresh", "Click", "Main_Refresh_Click")]
    [InlineData("View", "Menu_PreviewNotations", "IsChecked", "{Binding PreviewAreaShowNotations, Mode=TwoWay}")]
    [InlineData("View", "Menu_StatusBar", "Name", "ShowStatusMenu")]
    [InlineData("Output", "Menu_StartShow", "Command", "{Binding ToggleOutputLiveCommand}")]
    [InlineData("Output", "Menu_GoLiveWithPreview", "Command", "{Binding SendToOutputAndNextCommand}")]
    [InlineData("Output", "Menu_RefreshOutput", "Command", "{Binding RefreshOutputCommand}")]
    [InlineData("Output", "Menu_BlackScreen", "Command", "{Binding ToggleOutputBlackCommand}")]
    [InlineData("Output", "Menu_ClearScreen", "Command", "{Binding ToggleOutputClearCommand}")]
    [InlineData("Output", "Menu_RestartCurrentItem", "Command", "{Binding RestartCurrentItemCommand}")]
    [InlineData("Tools", "Menu_Import", "Click", "OpenImportExport_Click")]
    [InlineData("Tools", "Menu_ImportFolder", "Click", "OpenFolderImport_Click")]
    [InlineData("Tools", "Menu_Export", "Click", "OpenExport_Click")]
    [InlineData("Tools", "Menu_Recover", "Click", "SongRecover_Click")]
    [InlineData("Tools", "Menu_Empty", "Click", "EmptyDeleted_Click")]
    [InlineData("Tools", "Menu_AddToUsages", "Click", "AddListUsages_Click")]
    [InlineData("Tools", "Menu_ViewUsages", "Click", "OpenUsageReport_Click")]
    [InlineData("Tools", "Menu_SmartMerge", "Click", "SongMerge_Click")]
    [InlineData("Tools", "Menu_Compact", "Command", "{Binding Library.CompactDatabaseCommand}")]
    [InlineData("Tools", "Menu_ClearAllFormatting", "Click", "ClearDatabaseFormats_Click")]
    [InlineData("Tools", "Menu_ClearRegistrySettings", "Click", "ResetAllSettings_Click")]
    [InlineData("Help", "Menu_Contents", "Click", "HelpContents_Click")]
    [InlineData("Help", "Menu_HelpWeb", "Click", "WebHelp_Click")]
    [InlineData("Help", "Menu_Register", "Click", "OpenRegistration_Click")]
    [InlineData("Help", "Menu_About", "Click", "OpenAbout_Click")]
    public void LegacyMenuEntryHasAnActiveWpfRouteInTheSameMenu(string group, string legacy, string attribute, string target, string? tag = null)
    {
        var expectedGroup = Regex.Match(Designer, @"Menu_Main" + group + @"\.DropDownItems\.AddRange\(([^;]+)\);");
        Assert.Contains(legacy, Regex.Matches(expectedGroup.Groups[1].Value, @"\bMenu_\w+").Select(match => match.Value));
        var menu = MainMenu.Elements().Single(item => (string?)item.Attribute("Header") == group);
        Assert.Contains(menu.Elements(), item => item.Name.LocalName == "MenuItem"
            && item.Attributes().Any(a => a.Name.LocalName == attribute && a.Value == target)
            && (tag == null || (string?)item.Attribute("Tag") == tag));
    }

    [Fact]
    public void EveryWinFormsMenuActionIsInTheComparison()
    {
        var expected = Regex.Matches(Designer, @"Menu_Main\w+\.DropDownItems\.AddRange\(([^;]+)\);")
            .SelectMany(group => Regex.Matches(group.Groups[1].Value, @"\bMenu_\w+").Select(match => match.Value)).Order().ToArray();
        var cases = typeof(WinFormsMenuParityTests).GetMethod(nameof(LegacyMenuEntryHasAnActiveWpfRouteInTheSameMenu))!
            .GetCustomAttributes(typeof(InlineDataAttribute), false).Cast<InlineDataAttribute>()
            .Select(data => (string)data.GetData(null!).Single()[1]).Order().ToArray();
        Assert.Equal(expected, cases);
    }

    [Theory]
    [InlineData("File")]
    [InlineData("Edit")]
    [InlineData("View")]
    [InlineData("Output")]
    [InlineData("Tools")]
    [InlineData("Help")]
    public void NativeMenuOrderLabelsAndIconsMatch(string group)
    {
        var menu = MainMenu.Elements().Single(item => (string?)item.Attribute("Header") == group);
        var expected = Regex.Match(Designer, @"Menu_Main" + group + @"\.DropDownItems\.AddRange\(([^;]+)\);");
        var slots = Regex.Matches(expected.Groups[1].Value, @"\b(Menu_\w+|toolStripSeparator\w+)");
        var icons = File.ReadAllText(Path.Combine(Root, "Axicultuss", "Forms", "FrmMain.ModernUi.FormatIcons.cs"));
        var actual = menu.Elements().Take(slots.Count).ToList();
        Assert.Equal(slots.Count, actual.Count);
        for (int i = 0; i < slots.Count; i++)
        {
            var id = slots[i].Value;
            if (id.StartsWith("toolStripSeparator", StringComparison.Ordinal))
            {
                Assert.Equal("Separator", actual[i].Name.LocalName);
                continue;
            }
            var caption = Regex.Match(Designer, id + "\\.Text = \"([^\"]+)\";").Groups[1].Value.Replace("&", "");
            if (id is "Menu_Register" or "Menu_About") caption = caption.Replace("Axicultuss", "Axicultus", StringComparison.Ordinal);
            if (id == "Menu_ClearRegistrySettings") caption = "Reset WPF Settings...";
            Assert.Equal(caption, ((string?)actual[i].Attribute("Header"))?.Replace("_", "").Replace("…", "..."));
            var glyph = Regex.Match(icons, "\\(" + id + ", \"([^\"]+)\", ModernIconStyle\\.(\\w+)");
            var icon = actual[i].Elements().FirstOrDefault(e => e.Name.LocalName == "MenuItem.Icon")?.Elements().Single();
            Assert.Equal(glyph.Success ? glyph.Groups[1].Value : null, (string?)icon?.Attribute("Icon"));
            Assert.Equal(glyph.Groups[2].Value == "Filled", (string?)icon?.Attribute("Filled") == "True");
        }
    }

    private static string FindRepositoryRoot()
    {
        for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir != null; dir = dir.Parent)
            if (File.Exists(Path.Combine(dir.FullName, "Axicultus.sln"))) return dir.FullName;
        throw new DirectoryNotFoundException("Axicultus.sln");
    }
}
