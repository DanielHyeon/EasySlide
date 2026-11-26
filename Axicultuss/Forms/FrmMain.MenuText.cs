using System;
using System.IO;
using System.Windows.Forms;
using Axicultus.Localization;
using Axicultus.Util;

namespace Axicultus;

// The main menu's texts in the UI language (Axicultus.Core's MenuText resources, shared with the WPF app) and
// View > Language. The designer keeps its English texts for design time; these replace them at run time.
public partial class FrmMain
{
    /// <summary>The settings value holding the menus' language (ko-KR or en-US); read by Program.Main.</summary>
    internal const string Reg_UiLanguage = "UiLanguage";

    private const string AppName = "Axicultuss";
    private const string FolderName = "Axicultus"; // the data folder the View menu opens

    private ToolStripMenuItem Menu_LanguageKorean, Menu_LanguageEnglish;

    private void ApplyMenuText()
    {
        Menu_MainFile.Text = MenuText.Menu_File;
        Menu_MainEdit.Text = MenuText.Menu_Edit;
        Menu_MainView.Text = MenuText.Menu_View;
        Menu_MainOutput.Text = MenuText.Menu_Output;
        Menu_MainTools.Text = MenuText.Menu_Tools;
        Menu_MainHelp.Text = MenuText.Menu_Help;

        Menu_WorshipSessions.Text = MenuText.File_WorshipSessions;
        Menu_PraiseBookTemplates.Text = MenuText.File_PraiseBooks;
        Menu_ListingOfSelectedFolder.Text = MenuText.File_ListingOfSelectedFolder;
        Menu_EditHistoryList.Text = MenuText.File_RecentEdits;
        Menu_Exit.Text = MenuText.File_Exit;

        Menu_AddSong.Text = MenuText.Edit_AddNewSong;
        Menu_EditSong.Text = MenuText.Edit_Edit;
        Menu_CopySong.Text = MenuText.Edit_Copy;
        Menu_MoveSong.Text = MenuText.Edit_Move;
        Menu_DeleteSong.Text = MenuText.Edit_Delete;
        Menu_SelectAll.Text = MenuText.Edit_SelectAll;
        Menu_Find.Text = MenuText.Edit_Find;
        Menu_UseSongNumbering.Text = MenuText.Edit_UseSongNumbering;
        Menu_ReArrangeSongFolders.Text = MenuText.Edit_RearrangeSongFolders;

        Menu_AxicultusFolder.Text = string.Format(MenuText.View_AppFolder, FolderName);
        Menu_Options.Text = MenuText.View_Options;
        Menu_Refresh.Text = MenuText.View_Refresh;
        Menu_PreviewNotations.Text = MenuText.View_ShowNotations;
        Menu_StatusBar.Text = MenuText.View_StatusBar;

        Menu_StartShow.Text = MenuText.Output_StartShow;
        Menu_GoLiveWithPreview.Text = MenuText.Output_GoLiveMoveNext;
        Menu_RefreshOutput.Text = MenuText.Output_RefreshOutput;
        Menu_BlackScreen.Text = MenuText.Output_BlackScreen;
        Menu_ClearScreen.Text = MenuText.Output_ClearScreen;
        Menu_RestartCurrentItem.Text = MenuText.Output_RestartCurrentItem;

        Menu_Import.Text = MenuText.Tools_Import;
        Menu_ImportFolder.Text = MenuText.Tools_ImportFolder;
        Menu_Export.Text = MenuText.Tools_Export;
        Menu_Recover.Text = MenuText.Tools_RecoverDeleted;
        Menu_Empty.Text = MenuText.Tools_EmptyDeleted;
        Menu_AddToUsages.Text = MenuText.Tools_AddWorshipListToUsages;
        Menu_ViewUsages.Text = MenuText.Tools_ViewUsages;
        Menu_SmartMerge.Text = MenuText.Tools_SmartMerge;
        Menu_Compact.Text = MenuText.Tools_CompactDatabases;
        Menu_ClearAllFormatting.Text = MenuText.Tools_ClearAllFormatting;
        Menu_ClearRegistrySettings.Text = string.Format(MenuText.Tools_ClearSettingsAndExit, AppName);

        Menu_Contents.Text = MenuText.Help_Contents;
        Menu_HelpWeb.Text = MenuText.Help_Web;
        Menu_Register.Text = string.Format(MenuText.Help_Register, AppName);
        Menu_About.Text = string.Format(MenuText.Help_About, AppName);

        AddLanguageMenu();
    }

    // View > Language: each language named in itself; the choice shows after a restart.
    private void AddLanguageMenu()
    {
        Menu_LanguageKorean = new ToolStripMenuItem("한국어") { Tag = UiLanguage.Korean };
        Menu_LanguageEnglish = new ToolStripMenuItem("English") { Tag = UiLanguage.English };
        Menu_LanguageKorean.Click += Menu_Language_Click;
        Menu_LanguageEnglish.Click += Menu_Language_Click;
        var language = new ToolStripMenuItem(MenuText.View_Language) { Name = "Menu_Language" };
        language.DropDownItems.AddRange([Menu_LanguageKorean, Menu_LanguageEnglish]);
        language.DropDownOpening += (_, _) =>
        {
            string kept = KeptUiLanguage();
            Menu_LanguageKorean.Checked = kept == UiLanguage.Korean;
            Menu_LanguageEnglish.Checked = kept == UiLanguage.English;
        };
        Menu_MainView.DropDownItems.Add(new ToolStripSeparator());
        Menu_MainView.DropDownItems.Add(language);
    }

    internal static string KeptUiLanguage() => UiLanguage.Resolve(RegUtil.GetRegValue("settings", Reg_UiLanguage, UiLanguage.Korean));

    private void Menu_Language_Click(object sender, EventArgs e)
    {
        if (sender is not ToolStripMenuItem { Tag: string language } || language == KeptUiLanguage()) return;
        string title = MenuText.Language_Title;
        try
        {
            RegUtil.SaveRegValue("settings", Reg_UiLanguage, language);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or System.Security.SecurityException)
        {
            MessageBox.Show(this, string.Format(MenuText.Language_SaveFailed, ex.Message), title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        MessageBox.Show(this, MenuText.Language_Restart, title, MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
}
