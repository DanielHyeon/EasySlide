using Easislides.Properties;
using OfficeLib;
using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Linq;
using System.Data;
using System.Threading.Tasks;
using Easislides.Util;
using System.Threading;
using Easislides.SQLite;
using Easislides.Module;
using MethodInvoker = System.Windows.Forms.MethodInvoker;
using Type = System.Type;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Resources;
using System.Collections;
using System.Drawing.Imaging;

using System.ComponentModel.Design;




#if SQLite
using DbConnection = System.Data.SQLite.SQLiteConnection;
using DbCommand = System.Data.SQLite.SQLiteCommand;
using DbDataReader = System.Data.SQLite.SQLiteDataReader;
#elif MariaDB
using DbConnection = MySql.Data.MySqlClient.MySqlConnection;
using DbDataAdapter = MySql.Data.MySqlClient.MySqlDataAdapter;
using DbCommandBuilder = MySql.Data.MySqlClient.MySqlCommandBuilder;
using DbCommand = MySql.Data.MySqlClient.MySqlCommand;
using DbDataReader = MySql.Data.MySqlClient.MySqlDataReader;
using DbTransaction = MySql.Data.MySqlClient.MySqlTransaction;
#endif

namespace Easislides
{
    public partial class FrmMain : Form
    {
        // Fields, Constants, and Enums moved to FrmMain.Fields.cs

        public FrmMain()
        {
            InitializeComponent();
            FormClosed += Form1_FormClosed;
            WatchFirstShown();

            Init();

            frmMain = this;
            _keyboardHandler = new KeyboardActionHandler(this);

        }

        private void Init()
        {
            if (!Gf.InitEasiSlidesDir())
            {
                Gf.SplashScreenCanClose = true;
                Close();
            }
            else if (!InitFormControls())
            {
                Gf.SplashScreenCanClose = true;
                Close();
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

            Gf.formLoaded = true;
            // Keyboard hooking
            RemoveHookBlackScreen();
            AddHookBlackScreen();
            UpdateBlackScreenShortcut();

            RemoveHookSlideUpDown();
            AddHookSlideUpDown();

            Gf.InitFolderFiles(Gf.EasiSlidesTempDir);
            // The PowerPoint gate's state changes are logged and told (before any PowerPoint call of the main form).
            Gf.HookPowerPointGate();

            Gf.WMP_Present = DShowPlayerPresent();
            if (WorshipListItems.Items.Count > 0)
            {
                WorshipListIndexChanged(0, GetFirstItem: true);
            }
            LiveShow.OnMessage += LiveShow_OnMessage;

            MainPPT.Init();
            Gf.MainFormPPT = MainPPT;
            TimerToFront.Start();
            Gf.SplashScreenCanClose = true;

            // Test resource extraction (25.12.26)
            //string outputDir = @"E:\WorkSpace\wp - ms\dev\EasiSlides_v2.5.9\frmMainResource";
            //string resxPath = @"E:\WorkSpace\wp-ms\dev\EasiSlides_v2.5.9\Easislides\Easislides\FrmMain.resx";
            //Extract(resxPath, outputDir);

        }

        // Extract and SaveImage moved to FrmMain.Logic.cs
        // Form1_Resize moved to FrmMain.Events.cs
        // ApplySplitterValues moved to FrmMain.Layout.cs


        // LiveShow_OnMessage moved to FrmMain.Events.cs
        // InitFormControls moved to FrmMain.Layout.cs
        // AlertWindow_OnMessage moved to FrmMain.Events.cs
        // Panel Resize methods and Click handlers moved to FrmMain.Events.cs
        // ResizeSampleScreen methods and Focus/SetArea methods moved to FrmMain.Layout.cs
        // SaveFormStateToRegistry moved to FrmMain.Logic.cs

        private void tabControlSource_Resize(object sender, EventArgs e)
        {
            ResizetabControlSouceItems();
        }

        private void tabControlLists_Resize(object sender, EventArgs e)
        {
            ResizetabControlLists();
        }

        private void ResizetabControlSouceItems()
        {
            ResizeTabList(tabControlSource, SongsList, ToolBarMargin: false);
            // The find box above the song list (new look only) takes its height from the list (design D26).
            LayoutSongFind(listHeightIsBase: true);
            ResizeTabList(tabControlSource, InfoScreenList, ToolBarMargin: true);
            ResizeTabList(tabControlSource, PowerpointList, ToolBarMargin: true);
            ResizeTabList(tabControlSource, MediaList, ToolBarMargin: false);
            RelocateRightToolBar(InfoScreenList, ref panelInfoScreen2);
            RelocateRightToolBar(PowerpointList, ref panelPowerpoint2);
            SetSongListColWidth();
            SetInfoScreenListColWidth();
            SetMediaListColWidth();
            SetPowerpointListColWidth();
            ResizeComboAndToolBar(tabControlSource, ref SongFolder, ref panelFolders);
            ResizeComboAndToolBar(tabControlSource, ref InfoScreenFolder, ref panelInfoScreen1);
            ResizeComboAndToolBar(tabControlSource, ref MediaFolder, ref panelMedia1);
            ResizeComboAndToolBar(tabControlSource, ref PowerpointFolder, ref panelExternalFiles1);
            ResizeComboAndToolBar(tabControlSource, ref ImagesFolder, ref panelImage1);
            if (tabControlSource.SelectedTab != null && tabControlSource.SelectedTab.Name == "tabImages")
            {
                LoadBackgroundThumbImages();
            }
            else
            {
                Gf.TabSourceImagesChanged = true;
            }
            if (IsSelectedTab(tabControlSource, "tabPowerpoint") && Gf.PowerpointListingStyle > 0)
            {
                LoadExternalPowerpointThumbImages(0);
                Gf.TabSourceExternalFilesChanged = false;
            }
            if (IsSelectedTab(tabControlSource, "tabFiles"))
            {
                Gf.TabSourceExternalFilesChanged = true;
            }
            int lookupWidth = tabControlSource.Width * 7 / 12;
            lookupWidth = ((lookupWidth < 60) ? 60 : lookupWidth);
            BookLookup.Width = lookupWidth;
            BibleUserLookup.Left = BookLookup.Left + BookLookup.Width + 2;
            lookupWidth = tabControlSource.Width - (BibleUserLookup.Left + 12);
            if (lookupWidth > 0)
            {
                if (lookupWidth < 40)
                {
                    BibleUserLookup.Width = lookupWidth;
                }
                else if (lookupWidth < 40 + panelBible2.Width)
                {
                    BibleUserLookup.Width = 40;
                }
                else
                {
                    BibleUserLookup.Width = lookupWidth - panelBible2.Width;
                }
            }
            else
            {
                BibleUserLookup.Width = 0;
            }
            panelBible2.Left = BibleUserLookup.Left + BibleUserLookup.Width + 2;
            BibleText.Width = tabControlSource.Width - 15;
            BibleText.Height = tabControlSource.Height - 80;
            TabBibleVersions.Left = BibleText.Left;
            TabBibleVersions.Top = BibleText.Top + BibleText.Height;
            TabBibleVersions.Width = BibleText.Width;
            int groupBoxLeft = tabControlSource.Width - DefgroupBox1.Width;
            groupBoxLeft = ((groupBoxLeft > 0) ? (groupBoxLeft / 2) : 0);
            DefApplyDefaultsBtn.Left = groupBoxLeft;
            panelDefTemplate.Left = DefApplyDefaultsBtn.Left + DefApplyDefaultsBtn.Width;
            DefgroupBox1.Left = groupBoxLeft;
            DefgroupBox2.Left = groupBoxLeft;
            DefgroupBox3.Left = groupBoxLeft;
        }

        private void SetSongListColWidth()
        {
            if (SongsList.Columns.Count > 0)
            {
                SongsList.Columns[0].Width = ((SongsList.Width - SongsList.Columns[4].Width - 25 >= 0) ? (SongsList.Width - SongsList.Columns[4].Width - 25) : 0);
            }
        }

        private void SetInfoScreenListColWidth()
        {
            if (InfoScreenList.Columns.Count > 0)
            {
                InfoScreenList.Columns[0].Width = ((InfoScreenList.Width - 25 >= 0) ? (InfoScreenList.Width - 25) : 0);
            }
        }

        private void SetMediaListColWidth()
        {
            if (MediaList.Columns.Count > 0)
            {
                MediaList.Columns[0].Width = ((MediaList.Width - 25 >= 0) ? (MediaList.Width - 25) : 0);
            }
        }

        private void SetPowerpointListColWidth()
        {
            if (PowerpointList.Columns.Count > 0)
            {
                PowerpointList.Columns[0].Width = ((PowerpointList.Width - 25 >= 0) ? (PowerpointList.Width - 25) : 0);
            }
        }

        private void ResizetabControlLists()
        {
            ResizeTabList(tabControlLists, WorshipListItems, ToolBarMargin: true);
            ResizeComboAndToolBar(tabControlLists, ref SessionList, ref panelWorshipList1);
            RelocateRightToolBar(WorshipListItems, ref panelWorshipList2);
            ResizeTabList(tabControlLists, PraiseBookItems, ToolBarMargin: true);
            ResizeComboAndToolBar(tabControlLists, ref PraiseBook, ref panelPraiseBook1);
            RelocateRightToolBar(PraiseBookItems, ref panelPraiseBook2);
            SetWorshipPraiseListColWidth();
        }

        private void ResizeTabList(TabControl InTabControl, ListView InList, bool ToolBarMargin)
        {
            InList.Height = InTabControl.Height - 58;
            InList.Width = InTabControl.Width - (ToolBarMargin ? 42 : 15);
            InList.Width = ((InList.Width < 60) ? 60 : InList.Width);
        }

        private void ResizeComboAndToolBar(TabControl InTabControl, ref ComboBox InCombo)
        {
            Panel InToolBarPanel = new Panel();
            InToolBarPanel.Width = 0;
            InToolBarPanel.Height = 1;
            ResizeComboAndToolBar(InTabControl, ref InCombo, ref InToolBarPanel);
        }

        private void ResizeComboAndToolBar(TabControl InTabControl, ref ComboBox InCombo, ref Panel InToolBarPanel)
        {
            InCombo.Width = InTabControl.Width - (InToolBarPanel.Width + 15);
            InCombo.Width = ((InCombo.Width < 60) ? 60 : InCombo.Width);
            InToolBarPanel.Left = InCombo.Left + InCombo.Width + 1;
        }

        private void RelocateRightToolBar(ListView InListView, ref Panel InToolBarPanel)
        {
            InToolBarPanel.Left = InListView.Left + InListView.Width + 3;
        }

        private void SetAutoRotateButtons()
        {
            Gf.AssignDropDownItem(SelectedMenuItemName: (Gf.AutoRotateStyle == 0) ? Main_Rotate0.Name : ((Gf.AutoRotateStyle == 1) ? Main_Rotate1.Name : ((Gf.AutoRotateStyle != 2) ? Main_Rotate3.Name : Main_Rotate2.Name)), SelectedBtn: ref Main_RotateStyle, InMenuItem1: Main_Rotate0, InMenuItem2: Main_Rotate1, InMenuItem3: Main_Rotate2, InMenuItem4: Main_Rotate3);
            Main_NoRotate.Checked = !Gf.AutoRotateOn;
        }

        private void UpdateDefaultFields()
        {
            Gf.AssignDropDownItem(SelectedMenuItemName: (Gf.ShowSongHeadings == 0) ? Def_HeadNoTitles.Name : ((Gf.ShowSongHeadings != 1) ? Def_HeadFirstScreen.Name : Def_HeadAllTitles.Name), SelectedBtn: ref Def_Head, InMenuItem1: Def_HeadNoTitles, InMenuItem2: Def_HeadAllTitles, InMenuItem3: Def_HeadFirstScreen);
            Gf.AssignDropDownItem(SelectedMenuItemName: (Gf.ShowSongHeadingsAlign == 1) ? Def_HeadAlignAsR2.Name : ((Gf.ShowSongHeadingsAlign == 2) ? Def_HeadAlignLeft.Name : ((Gf.ShowSongHeadingsAlign == 3) ? Def_HeadAlignCentre.Name : ((Gf.ShowSongHeadingsAlign != 4) ? Def_HeadAlignAsR1.Name : Def_HeadAlignRight.Name))), SelectedBtn: ref Def_HeadAlign, InMenuItem1: Def_HeadAlignAsR1, InMenuItem2: Def_HeadAlignAsR2, InMenuItem3: Def_HeadAlignLeft, InMenuItem4: Def_HeadAlignCentre, InMenuItem5: Def_HeadAlignRight);
            Def_Shadow.Checked = ((Gf.UseShadowFont == 1) ? true : false);
            Def_Outline.Checked = ((Gf.UseOutlineFont == 1) ? true : false);
            Gf.AssignDropDownItem(SelectedMenuItemName: (Gf.ShowLyrics == 0) ? Def_ShowRegion1.Name : ((Gf.ShowLyrics != 1) ? Def_ShowRegionBoth.Name : Def_ShowRegion2.Name), SelectedBtn: ref Def_Region, InMenuItem1: Def_ShowRegion1, InMenuItem2: Def_ShowRegion2, InMenuItem3: Def_ShowRegionBoth);
            Gf.AssignDropDownItem(SelectedMenuItemName: (Gf.ShowVerticalAlign == 0) ? Def_VAlignTop.Name : ((Gf.ShowVerticalAlign != 1) ? Def_VAlignBottom.Name : Def_VAlignCentre.Name), SelectedBtn: ref Def_VAlign, InMenuItem1: Def_VAlignTop, InMenuItem2: Def_VAlignCentre, InMenuItem3: Def_VAlignBottom);
            Def_Interlace.Checked = ((Gf.ShowInterlace == 1) ? true : false);
            Def_Notations.Checked = ((Gf.ShowNotations == 1) ? true : false);
            Def_ToZero.Checked = ((Gf.ShowCapoZero == 1) ? true : false);
            Gf.AssignDropDownItem(SelectedMenuItemName: (Gf.BackgroundMode == ImageMode.Tile) ? Def_ImageTile.Name : ((Gf.BackgroundMode != ImageMode.Centre) ? Def_ImageBestFit.Name : Def_ImageCentre.Name), SelectedBtn: ref Def_ImageMode, InMenuItem1: Def_ImageTile, InMenuItem2: Def_ImageCentre, InMenuItem3: Def_ImageBestFit);
            UpdateDefaultNoImageButton();
            Gf.AssignDropDownItem(SelectedMenuItemName: (Gf.Folders[0].FontAlign[0] == 1) ? Def_R1AlignLeft.Name : ((Gf.Folders[0].FontAlign[0] != 2) ? Def_R1AlignRight.Name : Def_R1AlignCentre.Name), SelectedBtn: ref Def_R1Align, InMenuItem1: Def_R1AlignLeft, InMenuItem2: Def_R1AlignCentre, InMenuItem3: Def_R1AlignRight);
            Gf.AssignDropDownItem(SelectedMenuItemName: (Gf.Folders[0].FontAlign[1] == 1) ? Def_R2AlignLeft.Name : ((Gf.Folders[0].FontAlign[1] != 2) ? Def_R2AlignRight.Name : Def_R2AlignCentre.Name), SelectedBtn: ref Def_R2Align, InMenuItem1: Def_R2AlignLeft, InMenuItem2: Def_R2AlignCentre, InMenuItem3: Def_R2AlignRight);
            Def_BackColour.ForeColor = Gf.ShowScreenColour[0];
            Def_R1Colour.ForeColor = Gf.ShowFontColour[0];
            Def_R2Colour.ForeColor = Gf.ShowFontColour[1];
            UpdatingDefaultFields = true;
            Def_TransItem.SelectedIndex = Gf.ShowItemTransition;
            Def_TransSlides.SelectedIndex = Gf.ShowSlideTransition;
            AssignMediaText(ref Def_AssignMedia, Gf.MediaOption);
            UpdatingDefaultFields = false;
        }

        private void UpdateDefaultNoImageButton()
        {
            if (Gf.BackgroundPicture != "")
            {
                Def_NoImage.Enabled = true;
                Def_NoImage.ToolTipText = "Remove Default Background '" + Gf.BackgroundPicture + "'";
            }
            else
            {
                Def_NoImage.ToolTipText = "No Default Background";
                Def_NoImage.Enabled = false;
            }
        }

        private void UpdateDisplayPanelFields()
        {
            Def_PanelTransparent.Checked = ((Gf.PanelBackColourTransparent > 0) ? true : false);
            Def_PanelAsR1.Checked = ((Gf.PanelTextColourAsRegion1 > 0) ? true : false);
            Def_PanelShow.Checked = ((Gf.ShowDataDisplayMode > 0) ? true : false);
            Def_PanelSong.Checked = ((Gf.ShowDataDisplaySongs > 0) ? true : false);
            Def_PanelSlides.Checked = ((Gf.ShowDataDisplaySlides > 0) ? true : false);
            Def_PanelTitle.Checked = ((Gf.ShowDataDisplayTitle > 0) ? true : false);
            Def_PanelCopyright.Checked = ((Gf.ShowDataDisplayCopyright > 0) ? true : false);
            Def_PanelPrevNext.Checked = ((Gf.ShowDataDisplayPrevNext > 0) ? true : false);
            Def_PanelFontBold.Checked = ((Gf.ShowDataDisplayFontBold > 0) ? true : false);
            Def_PanelFontItalics.Checked = ((Gf.ShowDataDisplayFontItalic > 0) ? true : false);
            Def_PanelFontUnderline.Checked = ((Gf.ShowDataDisplayFontUnderline > 0) ? true : false);
            Def_PanelFontShadow.Checked = ((Gf.ShowDataDisplayFontShadow > 0) ? true : false);
            Def_PanelFontOutline.Checked = ((Gf.ShowDataDisplayFontOutline > 0) ? true : false);
            int heightPercent = (int)(Gf.BottomBorderFactor * 100.0);
            if (heightPercent < 6 || heightPercent > 20)
            {
                heightPercent = 6;
            }
            // The clamped value, kept within the control's own range (setting Value out of range throws).
            Def_PanelHeight.Value = Math.Max(Def_PanelHeight.Minimum, Math.Min(Def_PanelHeight.Maximum, heightPercent));
            Def_PanelFontOutline.Checked = ((Gf.ShowDataDisplayFontOutline > 0) ? true : false);
            Def_PanelTextColour.ForeColor = Gf.PanelTextColour;
            Def_PanelBackColour.ForeColor = Gf.PanelBackColour;
            Def_PanelFontList.Text = Gf.ShowDataDisplayFontName;
        }

        private void UpdateDisplayPanelData(bool RefreshSlides)
        {
            SaveWorshipList();
            if (Gf.PreviewItem.ItemID != "")
            {
                RefreshSlidesFonts(ref Gf.PreviewItem, ImageTransitionControl.TransitionAction.None);
            }
            if (Gf.OutputItem.ItemID != "")
            {
                RefreshSlidesFonts(ref Gf.OutputItem, ImageTransitionControl.TransitionAction.None);
            }
            if (Gf.ShowRunning)
            {
                RemoteControlLiveShow(LiveShowAction.Remote_FormatChanged);
                if (RefreshSlides)
                {
                    RemoteControlLiveShow(LiveShowAction.Remote_FormatChanged);
                }
                else
                {
                    RemoteControlLiveShow(LiveShowAction.Remote_PanelChanged);
                }
            }
        }

        private void DefApplyDefaultsBtn_Click(object sender, EventArgs e)
        {
            if (WorshipListItems.Items.Count > 0)
            {
                for (int i = 0; i < WorshipListItems.Items.Count; i++)
                {
                    if (DataUtil.Left(WorshipListItems.Items[i].SubItems[1].Text, 1) != "I")
                    {
                        WorshipListItems.Items[i].SubItems[2].Text = "";
                    }
                }
            }
            Ind_checkBox.Checked = false;
            ApplyDefaultData(StartAtFirstSlide: true);
        }

        private void ApplyDefaultData(bool StartAtFirstSlide)
        {
            ApplyDefaultData(StartAtFirstSlide, ImageTransitionControl.TransitionAction.None);
        }

        private void ApplyDefaultData(bool StartAtFirstSlide, ImageTransitionControl.TransitionAction TransitionAction)
        {
            if (Ind_checkBox.Checked)
            {
                ApplyIndividualFormat(ref Gf.PreviewItem);
            }
            else
            {
                NoIndividualFormat();
            }
            SaveWorshipList();
            Gf.SetShowBackground(Gf.PreviewItem, ref PreviewScreen);
            if (Gf.PreviewItem.ItemID != "")
            {
                if (StartAtFirstSlide)
                {
                    Gf.PreviewItem.CurSlide = 1;
                }
                RefreshSlidesFonts(ref Gf.PreviewItem, TransitionAction);
                ApplyLyricsRichTextBoxFont(0);
            }
            else
            {
                ResetMainPictureBox(ref Gf.PreviewItem);
            }
            if (!Gf.OutputItem.UseDefaultFormat || Gf.OutputItem.ItemID == "")
            {
                return;
            }
            if (StartAtFirstSlide)
            {
                Gf.OutputItem.CurSlide = 1;
            }
            if (Gf.ShowRunning)
            {
                if (StartAtFirstSlide)
                {
                    Gf.MainAction_SongChanged_Transaction = TransitionAction;
                    RemoteControlLiveShow(LiveShowAction.Remote_SongChanged);
                }
                else
                {
                    Gf.MainAction_SongChanged_Transaction = TransitionAction;
                    RemoteControlLiveShow(LiveShowAction.Remote_SongChanged);
                }
            }
            RefreshSlidesFonts(ref Gf.OutputItem, TransitionAction);
            ApplyLyricsRichTextBoxFont(1);
        }

        private bool RefreshSlidesFonts(ref SongSettings InItem)
        {
            return RefreshSlidesFonts(ref InItem, ImageTransitionControl.TransitionAction.AsStored);
        }

        private bool RefreshSlidesFonts(ref SongSettings InItem, ImageTransitionControl.TransitionAction TransitionAction)
        {
            SetItemFontSettings(ref InItem);
            Gf.FormatDisplayLyrics(ref InItem, PrepareSlides: true, UseStoredSequence: true);
            return ShowSlide(ref InItem, TransitionAction);
        }

        private void Def_Items_MouseUp(object sender, MouseEventArgs e)
        {
            ToolStripButton toolStripButton = (ToolStripButton)sender;
            string name = toolStripButton.Name;
            if (name == "Def_LoadTemplate")
            {
                LoadWorshipListTemplate();
            }
            else if (name == "Def_SaveTemplate")
            {
                SaveWorshipListTemplate(Gf.WorshipDir + Gf.CurSession + ".est", Gf.WorshipTemplatesDir);
            }
            else if (name == "Def_Shadow")
            {
                Gf.UseShadowFont = (toolStripButton.Checked ? 1 : 0);
                ApplyDefaultData(StartAtFirstSlide: false);
            }
            else if (name == "Def_Outline")
            {
                Gf.UseOutlineFont = (toolStripButton.Checked ? 1 : 0);
                ApplyDefaultData(StartAtFirstSlide: false);
            }
            else if (name == "Def_Interlace")
            {
                Gf.ShowInterlace = (toolStripButton.Checked ? 1 : 0);
                ApplyDefaultData(StartAtFirstSlide: false);
            }
            else if (name == "Def_Notations")
            {
                Gf.ShowNotations = (toolStripButton.Checked ? 1 : 0);
                ApplyDefaultData(StartAtFirstSlide: true);
            }
            else if (name == "Def_ToZero")
            {
                Gf.ShowCapoZero = (toolStripButton.Checked ? 1 : 0);
                ApplyDefaultData(StartAtFirstSlide: true);
            }
            else if (name == "Def_R1Colour")
            {
                if (Gf.SelectColorFromBtn(ref Def_R1Colour, ref Gf.ShowFontColour[0]))
                {
                    ApplyDefaultData(StartAtFirstSlide: false);
                }
            }
            else if (name == "Def_R2Colour")
            {
                if (Gf.SelectColorFromBtn(ref Def_R2Colour, ref Gf.ShowFontColour[1]))
                {
                    ApplyDefaultData(StartAtFirstSlide: false);
                }
            }
            else if (name == "Def_NoImage")
            {
                Def_SetNoImage();
            }
            else if (name == "Def_BackColour")
            {
                if (Gf.SelectBackgroundColors(ref Def_BackColour, ref Gf.ShowScreenColour[0], ref Gf.ShowScreenColour[1], ref Gf.ShowScreenStyle, IsDefault: true))
                {
                    SetMainDefaultBackScreen();
                    Def_SetNoImage();
                }
            }
            else if (name == "Def_AssignMedia")
            {
                Def_Media_Clicked();
            }
            else if (name == "Def_PanelAsR1")
            {
                Gf.PanelTextColourAsRegion1 = (toolStripButton.Checked ? 1 : 0);
                UpdateDisplayPanelData(RefreshSlides: false);
            }
            else if (name == "Def_PanelTextColour")
            {
                if (Gf.SelectColorFromBtn(ref Def_PanelTextColour, ref Gf.PanelTextColour))
                {
                    UpdateDisplayPanelData(RefreshSlides: false);
                }
            }
            else if (name == "Def_PanelTransparent")
            {
                Gf.PanelBackColourTransparent = (toolStripButton.Checked ? 1 : 0);
                UpdateDisplayPanelData(RefreshSlides: false);
            }
            else if (name == "Def_PanelBackColour")
            {
                if (Gf.SelectColorFromBtn(ref Def_PanelBackColour, ref Gf.PanelBackColour))
                {
                    UpdateDisplayPanelData(RefreshSlides: false);
                }
            }
            else if (name == "Def_PanelShow")
            {
                Gf.ShowDataDisplayMode = (toolStripButton.Checked ? 1 : 0);
                UpdateDisplayPanelData(RefreshSlides: false);
            }
            else if (name == "Def_PanelSong")
            {
                Gf.ShowDataDisplaySongs = (toolStripButton.Checked ? 1 : 0);
                UpdateDisplayPanelData(RefreshSlides: false);
            }
            else if (name == "Def_PanelSlides")
            {
                Gf.ShowDataDisplaySlides = (toolStripButton.Checked ? 1 : 0);
                UpdateDisplayPanelData(RefreshSlides: false);
            }
            else if (name == "Def_PanelTitle")
            {
                Gf.ShowDataDisplayTitle = (toolStripButton.Checked ? 1 : 0);
                UpdateDisplayPanelData(RefreshSlides: false);
            }
            else if (name == "Def_PanelCopyright")
            {
                Gf.ShowDataDisplayCopyright = (toolStripButton.Checked ? 1 : 0);
                UpdateDisplayPanelData(RefreshSlides: false);
            }
            else if (name == "Def_PanelPrevNext")
            {
                Gf.ShowDataDisplayPrevNext = (toolStripButton.Checked ? 1 : 0);
                UpdateDisplayPanelData(RefreshSlides: false);
            }
            else if (name == "Def_PanelFontBold")
            {
                Gf.ShowDataDisplayFontBold = (toolStripButton.Checked ? 1 : 0);
                UpdateDisplayPanelData(RefreshSlides: false);
            }
            else if (name == "Def_PanelFontItalics")
            {
                Gf.ShowDataDisplayFontItalic = (toolStripButton.Checked ? 1 : 0);
                UpdateDisplayPanelData(RefreshSlides: false);
            }
            else if (name == "Def_PanelFontUnderline")
            {
                Gf.ShowDataDisplayFontUnderline = (toolStripButton.Checked ? 1 : 0);
                UpdateDisplayPanelData(RefreshSlides: false);
            }
            else if (name == "Def_PanelFontShadow")
            {
                Gf.ShowDataDisplayFontShadow = (toolStripButton.Checked ? 1 : 0);
                UpdateDisplayPanelData(RefreshSlides: false);
            }
            else if (name == "Def_PanelFontOutline")
            {
                Gf.ShowDataDisplayFontOutline = (toolStripButton.Checked ? 1 : 0);
                UpdateDisplayPanelData(RefreshSlides: false);
            }
        }

        private void Def_PanelHeight_MouseUp(object sender, MouseEventArgs e)
        {
            Gf.BottomBorderFactor = (double)Def_PanelHeight.Value / 100.0;
                UpdateDisplayPanelData(RefreshSlides: true);
        }

        private void Def_PanelFontList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!InitFormLoad)
            {
                Gf.ShowDataDisplayFontName = Def_PanelFontList.Text;
                UpdateDisplayPanelData(RefreshSlides: false);
            }
        }

        // Methods moved to partial classes (Batch 2)

        // Batch 3 (Corrected) - Ind UpDowns, Layout buttons, Modes, StatusBar, PopulateLists moved to partial classes

        // Batch 3 (PraiseBook, PowerPoint, Media folders, SongFolder) moved to partial classes

        // Batch 5 - SongNumbering, FolderLists, Thumbnails, TabControl moved to partial classes

        // Batch 6 - Bible handling, Session loading, Index Parsing moved to partial classes











        private void SongsList_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.A)
            {
                SongsList_SelectAll();
            }
            else if (e.Control && e.KeyCode == Keys.C)
            {
                Copy_Song();
            }
            else if (e.KeyCode == Keys.Delete)
            {
                RemoveSongsListSong();
                SongsList.Focus();
            }
            else if (e.Control && e.Alt && e.KeyCode == Keys.V)
            {
                Gf.ShowDebugVideoMessages = !Gf.ShowDebugVideoMessages;
                MessageBox.Show("Video Debug Message Turned " + (Gf.ShowDebugVideoMessages ? "On" : "Off"));
            }
            else
            {
                SongsListIndexChanged(1, ScrollToCaret: false);
                SongsList.Focus();
            }
        }

        private void SongsList_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                SongsListIndexChanged();
            }
            else if (e.Button == MouseButtons.Right)
            {
                ListViewItem itemAt = SongsList.GetItemAt(e.X, e.Y);
                string OutString = "";
                if (itemAt != null)
                {
                    itemAt.Selected = true;
                    SongsListIndexChanged();
                }
                Gf.GetSelectedIndex(SongsList, ref OutString);
                Gf.SetMenuItem(ref CMenuSongs_Edit, OutString, "Edit Item", "Edit", DisableWhenBlank: true);
                CMenuSongs.Show(SongsList, e.X, e.Y);
            }
            SongsList.Focus();
        }


        private void tabControlLists_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (tabControlLists.SelectedIndex)
            {
                case 0:
                    ApplyWorshipMode();
                    break;
                case 1:
                    ApplyPraisebookMode();
                    break;
            }
        }


        private void Menu_EditHistory_Click(object sender, EventArgs e)
        {
            try
            {
                ToolStripMenuItem toolStripMenuItem = (ToolStripMenuItem)sender;
                int historyIndex = DataUtil.ObjToInt(toolStripMenuItem.Tag) + 1;
                Edit_Item(Gf.MainEditHistoryList[historyIndex].Id);
            }
            catch
            {
            }
        }

        private void SessionList_SelectedValueChanged(object sender, EventArgs e)
        {
            if (!InitFormLoad)
            {
                SessionList_Change();
            }
        }

        private void BuildPreviewScreenHandler()
        {
            PreviewScreen.DoubleClick += PreviewScreen_DoubleClick;
        }

        private void PreviewScreen_DoubleClick(object sender, EventArgs e)
        {
            if (Gf.PreviewItem.Type == "D")
            {
                string inIDString = "D" + Gf.PreviewItem.ItemID;
                Edit_Item(inIDString);
            }
            else if (Gf.PreviewItem.Type == "P")
            {
                string inIDString = "P" + Gf.PreviewItem.Path;
                Edit_Item(inIDString);
            }
            else if (Gf.PreviewItem.Type == "B")
            {
                string inIDString = "B" + Gf.PreviewItem.ItemID;
                string editedIdString = Edit_Item(inIDString, ref Gf.PreviewItem.Title);
                if (editedIdString != "")
                {
                    if (Gf.PreviewItem.Source == ItemSource.HolyBible)
                    {
                        HB_CurSelectedPassages = DataUtil.Right(editedIdString, editedIdString.Length - 1);
                        HB_CurSelectedTitle = Gf.PreviewItem.Title;
                        HB_SelectedPassagesChanged(HB_CurSelectedPassages, ref HB_CurSelectedTitle);
                    }
                    else if (Gf.PreviewItem.Source == ItemSource.WorshipList)
                    {
                        int selectedIndex = Gf.GetSelectedIndex(WorshipListItems);
                        WorshipListItems.Items[selectedIndex].SubItems[0].Text = Gf.PreviewItem.Title;
                        WorshipListItems.Items[selectedIndex].SubItems[1].Text = editedIdString;
                        WorshipListIndexChanged();
                    }
                }
            }
            else if (Gf.PreviewItem.Type == "T")
            {
                string inIDString = "T" + Gf.PreviewItem.Path;
                Edit_Item(inIDString);
            }
            else if (Gf.PreviewItem.Type == "W")
            {
                string inIDString = "W" + Gf.PreviewItem.Path;
                Edit_Item(inIDString);
            }
            else if (Gf.PreviewItem.Type == "I")
            {
                string inIDString = "I" + Gf.PreviewItem.Path;
                Edit_Item(inIDString);
            }
            else if (Gf.PreviewItem.Type == "M")
            {
                string inIDString = "M" + Gf.PreviewItem.Path;
                Edit_Item(inIDString);
            }
        }

        private void ThumbImage_MouseUp(object sender, MouseEventArgs e)
        {
            Control control = (Control)sender;
            ThumbImageClicked = Convert.ToInt32(control.Tag);
            if (e.Button == MouseButtons.Left)
            {
                ApplyBackground(ThumbImageClicked, 2);
            }
            else if (e.Button == MouseButtons.Right)
            {
                Point position = control.PointToClient(Cursor.Position);
                CMenuImages.Show(control, position);
            }
        }

        private void ExternalFilesPP_DoubleClick(object sender, EventArgs e)
        {
            AddFromPowerpointList();
        }

        private void ExternalFilesPP_MouseUp(object sender, MouseEventArgs e)
        {
            Control control = (Control)sender;
            ThumbImageClicked = Convert.ToInt32(control.Tag);
            if (e.Button == MouseButtons.Left)
            {
                Gf.PreviewItem.Source = ItemSource.ExternalFilePowerpoint;
                string itemIdString = PowerpointCurPreview = "P" + ExternalPPImagename[ThumbImageClicked];
                string InTitle = Path.GetFileNameWithoutExtension(ExternalPPImagename[ThumbImageClicked]);
                Gf.PreviewItem.InMainItemText = InTitle;
                Gf.PreviewItem.InSubItemItem1Text = itemIdString;
                Gf.PreviewItem.CurItemNo = 0;
                LoadItem(ref Gf.PreviewItem, itemIdString, "", 1, ref InTitle, ScrollToCaret: false);
                LoadExternalPowerpointThumbImages(ThumbImageClicked + 1);
                UpdateDisplayPanelFields();
            }
            else if (e.Button != MouseButtons.Right)
            {
            }
        }

        //private void UpdateBackgroundImageButtons(int InMode, int BackgroundImageMode)
        //{
        //    if (BackgroundImageMode < 0 || BackgroundImageMode > 2)
        //    {
        //        return;
        //    }
        //    string text = "";
        //    if (InMode == 1 || InMode == 2)
        //    {
        //        text = "";
        //        if (!Ind_checkBox.Checked)
        //        {
        //            if (BackgroundImageMode.ToString() == Ind_ImageTile.Tag.ToString())
        //            {
        //                text = "Ind_ImageTile";
        //            }
        //            else if (BackgroundImageMode.ToString() == Ind_ImageCentre.Tag.ToString())
        //            {
        //                text = "Ind_ImageCentre";
        //            }
        //            else if (BackgroundImageMode.ToString() == Ind_ImageBestFit.Tag.ToString())
        //            {
        //                text = "Ind_ImageBestFit";
        //            }
        //            if (text != "")
        //            {
        //                Gf.AssignDropDownItem(ref Ind_ImageMode, text, Ind_ImageTile, Ind_ImageCentre, Ind_ImageBestFit);
        //            }
        //        }
        //    }
        //    if (InMode == 0 || InMode == 2)
        //    {
        //        text = "";
        //        if (BackgroundImageMode.ToString() == Def_ImageTile.Tag.ToString())
        //        {
        //            text = "Def_ImageTile";
        //        }
        //        else if (BackgroundImageMode.ToString() == Def_ImageCentre.Tag.ToString())
        //        {
        //            text = "Def_ImageCentre";
        //        }
        //        else if (BackgroundImageMode.ToString() == Def_ImageBestFit.Tag.ToString())
        //        {
        //            text = "Def_ImageBestFit";
        //        }
        //        if (text != "")
        //        {
        //            Gf.AssignDropDownItem(ref Def_ImageMode, text, Def_ImageTile, Def_ImageCentre, Def_ImageBestFit);
        //        }
        //    }
        //    UpdateDefaultNoImageButton();
        //}







        private void btnToOutput_Click(object sender, EventArgs e)
        {
            if (Gf.PreviewItem.ItemID != "")
            {
                CopyPreviewToOutput();
            }
        }



        private void btnToOutputMoveNext_Click(object sender, EventArgs e)
        {
            if (Gf.PreviewItem.ItemID != "")
            {
                CopyPreviewToOutput();
            }
            ManualMoveToItem(Gf.PreviewItem, KeyDirection.NextOne);
            FocusOutputArea();
        }

        private void Ind_MarginUpDown_MouseUp(object sender, MouseEventArgs e)
        {
            if (!UpdatingFormatFields)
            {
                UpDownBase upDownBase = (UpDownBase)sender;
                string name = upDownBase.Name;
                UpdatingFormatFields = true;
                try
                {
                    if (name == "Ind_Reg1TopUpDown")
                    {
                        Gf.PreviewItem.Format.ShowFontVPosition[0] = (int)Ind_Reg1TopUpDown.Value;
                        Gf.UpdatePosUpDowns(ref Ind_Reg1TopUpDown, ref Ind_Reg2TopUpDown, ref Ind_BottomUpDown, ref Gf.PreviewItem.Format.ShowFontVPosition[0], ref Gf.PreviewItem.Format.ShowFontVPosition[1], Gf.PreviewItem.Format.ShowBottomMargin);
                    }
                    else if (name == "Ind_Reg2TopUpDown")
                    {
                        Gf.PreviewItem.Format.ShowFontVPosition[1] = (int)Ind_Reg2TopUpDown.Value;
                        Gf.UpdatePosUpDowns(ref Ind_Reg1TopUpDown, ref Ind_Reg2TopUpDown, ref Ind_BottomUpDown, ref Gf.PreviewItem.Format.ShowFontVPosition[0], ref Gf.PreviewItem.Format.ShowFontVPosition[1], Gf.PreviewItem.Format.ShowBottomMargin);
                    }
                    else if (name == "Ind_LeftUpDown")
                    {
                        Gf.PreviewItem.Format.ShowLeftMargin = (int)Ind_LeftUpDown.Value;
                    }
                    else if (name == "Ind_RightUpDown")
                    {
                        Gf.PreviewItem.Format.ShowRightMargin = (int)Ind_RightUpDown.Value;
                    }
                    else if (name == "Ind_BottomUpDown")
                    {
                        Gf.PreviewItem.Format.ShowBottomMargin = (int)Ind_BottomUpDown.Value;
                        Gf.UpdatePosUpDowns(ref Ind_Reg1TopUpDown, ref Ind_Reg2TopUpDown, ref Ind_BottomUpDown, ref Gf.PreviewItem.Format.ShowFontVPosition[0], ref Gf.PreviewItem.Format.ShowFontVPosition[1], Gf.PreviewItem.Format.ShowBottomMargin);
                    }
                }
                finally
                {
                    UpdatingFormatFields = false;
                }
                UpdateFormatData();
            }
        }

        private void Ind_FontsList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!(UpdatingFormatFields || InitFormLoad))
            {
                Gf.PreviewItem.Format.ShowFontName[0] = Ind_Reg1FontsList.Text;
                Gf.PreviewItem.Format.ShowFontName[1] = Ind_Reg2FontsList.Text;
                UpdateFormatData();
            }
        }

        private void Ind_TransSelectedIndexChanged(object sender, EventArgs e)
        {
            if (!(UpdatingFormatFields || InitFormLoad))
            {
                ToolStripComboBox toolStripComboBox = (ToolStripComboBox)sender;
                ImageTransitionControl.TransitionAction transitionAction = (toolStripComboBox.Name == "Ind_TransItem") ? ImageTransitionControl.TransitionAction.AsStoredItem : ImageTransitionControl.TransitionAction.AsStoredSlide;
                Gf.PreviewItem.Format.ShowItemTransition = Ind_TransItem.SelectedIndex;
                Gf.PreviewItem.Format.ShowSlideTransition = Ind_TransSlides.SelectedIndex;
                UpdateFormatData(StartAtFirstSlide: false, transitionAction);
            }
        }

        private void Ind_FontSizeUpDown_MouseUp(object sender, MouseEventArgs e)
        {
            if (!UpdatingFormatFields)
            {
                Gf.PreviewItem.Format.ShowFontSize[0] = (int)Ind_Reg1SizeUpDown.Value;
                Gf.PreviewItem.Format.ShowFontSize[1] = (int)Ind_Reg2SizeUpDown.Value;
                UpdateFormatData();
            }
        }

        //private void Ind_MarginUpDown_MouseUp(object sender, MouseEventArgs e)
        //{
        //    if (!UpdatingFormatFields)
        //    {
        //        UpDownBase upDownBase = (UpDownBase)sender;
        //        string name = upDownBase.Name;
        //        UpdatingFormatFields = true;
        //        if (name == "Ind_Reg1TopUpDown")
        //        {
        //            Gf.PreviewItem.Format.ShowFontVPosition[0] = (int)Ind_Reg1TopUpDown.Value;
        //            UpdatingFormatFields = true;
        //            Gf.UpdatePosUpDowns(ref Ind_Reg1TopUpDown, ref Ind_Reg2TopUpDown, ref Ind_BottomUpDown, ref Gf.PreviewItem.Format.ShowFontVPosition[0], ref Gf.PreviewItem.Format.ShowFontVPosition[1], Gf.PreviewItem.Format.ShowBottomMargin);
        //        }
        //        else if (name == "Ind_Reg2TopUpDown")
        //        {
        //            Gf.PreviewItem.Format.ShowFontVPosition[1] = (int)Ind_Reg2TopUpDown.Value;
        //            UpdatingFormatFields = true;
        //            Gf.UpdatePosUpDowns(ref Ind_Reg1TopUpDown, ref Ind_Reg2TopUpDown, ref Ind_BottomUpDown, ref Gf.PreviewItem.Format.ShowFontVPosition[0], ref Gf.PreviewItem.Format.ShowFontVPosition[1], Gf.PreviewItem.Format.ShowBottomMargin);
        //        }
        //        else if (name == "Ind_LeftUpDown")
        //        {
        //            Gf.PreviewItem.Format.ShowLeftMargin = (int)Ind_LeftUpDown.Value;
        //        }
        //        else if (name == "Ind_RightUpDown")
        //        {
        //            Gf.PreviewItem.Format.ShowRightMargin = (int)Ind_RightUpDown.Value;
        //        }
        //        else if (name == "Ind_BottomUpDown")
        //        {
        //            Gf.PreviewItem.Format.ShowBottomMargin = (int)Ind_BottomUpDown.Value;
        //            UpdatingFormatFields = true;
        //            Gf.UpdatePosUpDowns(ref Ind_Reg1TopUpDown, ref Ind_Reg2TopUpDown, ref Ind_BottomUpDown, ref Gf.PreviewItem.Format.ShowFontVPosition[0], ref Gf.PreviewItem.Format.ShowFontVPosition[1], Gf.PreviewItem.Format.ShowBottomMargin);
        //        }
        //        UpdatingFormatFields = false;
        //        UpdateFormatData();
        //    }
        //}

        private void Menu_useSongNumbering_Click(object sender, EventArgs e)
        {
            ApplyUseSongNumbers(Menu_UseSongNumbering.Checked);
            ImplementFolderChange = true;
            SongFolder_Change();
        }

        private void BookLookup_SelectedIndexChanged(object sender, EventArgs e)
        {
            BookLookupChanged();
        }

        private void TabBibleVersions_Click(object sender, EventArgs e)
        {
            if (TabBibleVersions.SelectedIndex != Gf.HB_CurVersionTabIndex)
            {
                TabBibleVersionsChanged();
            }
        }

        private void ImagesFolder_SelectedIndexChanged(object sender, EventArgs e)
        {
            ShowPicturesFolderThumbs();
        }

        private void SongFolder_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!InitFormLoad)
            {
                SongFolder_Change();
            }
        }

        private void tabControlSource_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (IsSelectedTab(tabControlSource, "tabImages"))
            {
                if (ImagesFolder.Items.Count > 0 && ImagesFolder.Text == "")
                {
                    ImagesFolder.SelectedIndex = 0;
                    Gf.TabSourceImagesChanged = false;
                }
                if (Gf.TabSourceImagesChanged)
                {
                    FormatBackgroundThumbContainers();
                    Gf.TabSourceImagesChanged = false;
                }
            }
            else if (IsSelectedTab(tabControlSource, "tabFiles"))
            {
                if (InfoScreenFolder.Items.Count > 0 && InfoScreenFolder.Text == "")
                {
                    InfoScreenFolder.SelectedIndex = 0;
                    Gf.TabSourceExternalFilesChanged = false;
                }
                if (!Gf.TabSourceExternalFilesChanged)
                {
                }
            }
            else if (IsSelectedTab(tabControlSource, "tabMedia"))
            {
                if (MediaFolder.Items.Count > 0 && MediaFolder.Text == "")
                {
                    MediaFolder.SelectedIndex = 0;
                    Gf.TabSourceMediaFolderFilesChanged = false;
                }
                if (!Gf.TabSourceMediaFolderFilesChanged)
                {
                }
            }
            else if (IsSelectedTab(tabControlSource, "tabPowerpoint"))
            {
                if (PowerpointFolder.Items.Count > 0 && PowerpointFolder.Text == "")
                {
                    PowerpointFolder.SelectedIndex = 0;
                    Gf.TabSourceExternalFilesChanged = false;
                }
                if (Gf.TabSourceExternalFilesChanged)
                {
                    FormatExternalPowerpointThumbContainers();
                    Gf.TabSourceExternalFilesChanged = false;
                }
            }
            ShowStatusBarSummary();
        }

        private void DisplaySettingsLabel(SongSettings Initem)
        {
            if (InvokeRequired)
            {
                this.Invoke(new MethodInvoker(delegate
                {
                    if (Initem != null)
                    {
                        ListViewItem listViewItem;
                        if (Initem.OutputStyleScreen)
                        {
                            OutputPanelDisplayName.Items.Clear();
                            listViewItem = OutputPanelDisplayName.Items.Add(Initem.Title);
                            toolTip1.SetToolTip(OutputPanelDisplayName, Initem.Title);
                        }
                        else
                        {
                            PreviewPanelDisplayName.Items.Clear();
                            listViewItem = PreviewPanelDisplayName.Items.Add(Initem.Title);
                            toolTip1.SetToolTip(PreviewPanelDisplayName, Initem.Title);
                        }
                        if (Initem.Type == "D")
                        {
                            listViewItem.ImageIndex = 0;
                        }
                        else if (Initem.Type == "P")
                        {
                            listViewItem.ImageIndex = 2;
                        }
                        else if (Initem.Type == "B")
                        {
                            listViewItem.ImageIndex = 4;
                        }
                        else if (Initem.Type == "T")
                        {
                            listViewItem.ImageIndex = 6;
                        }
                        else if (Initem.Type == "I")
                        {
                            listViewItem.ImageIndex = 8;
                        }
                        else if (Initem.Type == "W")
                        {
                            listViewItem.ImageIndex = 10;
                        }
                        else if (Initem.Type == "M")
                        {
                            listViewItem.ImageIndex = 28;
                        }
                        ApplyTitleListWordIcon(Initem.OutputStyleScreen ? OutputPanelDisplayName : PreviewPanelDisplayName, Initem);
                    }

                }));
            }
            else
            {
                if (Initem != null)
                {
                    ListViewItem listViewItem;
                    if (Initem.OutputStyleScreen)
                    {
                        OutputPanelDisplayName.Items.Clear();
                        listViewItem = OutputPanelDisplayName.Items.Add(Initem.Title);
                        toolTip1.SetToolTip(OutputPanelDisplayName, Initem.Title);
                    }
                    else
                    {
                        PreviewPanelDisplayName.Items.Clear();
                        listViewItem = PreviewPanelDisplayName.Items.Add(Initem.Title);
                        toolTip1.SetToolTip(PreviewPanelDisplayName, Initem.Title);
                    }
                    if (Initem.Type == "D")
                    {
                        listViewItem.ImageIndex = 0;
                    }
                    else if (Initem.Type == "P")
                    {
                        listViewItem.ImageIndex = 2;
                    }
                    else if (Initem.Type == "B")
                    {
                        listViewItem.ImageIndex = 4;
                    }
                    else if (Initem.Type == "T")
                    {
                        listViewItem.ImageIndex = 6;
                    }
                    else if (Initem.Type == "I")
                    {
                        listViewItem.ImageIndex = 8;
                    }
                    else if (Initem.Type == "W")
                    {
                        listViewItem.ImageIndex = 10;
                    }
                    else if (Initem.Type == "M")
                    {
                        listViewItem.ImageIndex = 28;
                    }
                    ApplyTitleListWordIcon(Initem.OutputStyleScreen ? OutputPanelDisplayName : PreviewPanelDisplayName, Initem);
                }
            }

        }

        private void SetSortButtonPB(SortBy Inmode)
        {
            switch (Inmode)
            {
                case SortBy.Alpha:
                    PB_WordCount.Checked = false;
                    Gf.PB_CJKGroupStyle = SortBy.Alpha;
                    break;
                case SortBy.WordCount:
                    PB_WordCount.Checked = true;
                    Gf.PB_CJKGroupStyle = SortBy.WordCount;
                    break;
            }
            string FirstCharSym = "";
            string songNumber = "000000";
            Cursor = Cursors.WaitCursor;
            if (Gf.UseSongNumbers)
            {
                for (int i = 0; i <= PraiseBookItems.Items.Count - 1; i++)
                {
                    string inTitle = Gf.RemoveMusicSym(PraiseBookItems.Items[i].SubItems[2].Text);
                    songNumber = DataUtil.Format6(PraiseBookItems.Items[i].SubItems[5].Text);
                    PraiseBookItems.Items[i].SubItems[0].Text = songNumber + DataUtil.GetCJKTitle(inTitle, Gf.PB_CJKGroupStyle, ref FirstCharSym);
                    PraiseBookItems.Items[i].SubItems[4].Text = FirstCharSym;
                }
            }
            else
            {
                for (int i = 0; i <= PraiseBookItems.Items.Count - 1; i++)
                {
                    string inTitle = Gf.RemoveMusicSym(PraiseBookItems.Items[i].SubItems[2].Text);
                    PraiseBookItems.Items[i].SubItems[0].Text = DataUtil.GetCJKTitle(inTitle, Gf.PB_CJKGroupStyle, ref FirstCharSym);
                    PraiseBookItems.Items[i].SubItems[4].Text = FirstCharSym;
                }
            }
            SortPraiseBookList();
            Cursor = Cursors.Default;
        }

        private void SavePraiseBook()
        {
            if (DataUtil.Trim(PraiseBook.Text) != "")
            {
                Gf.CurPraiseBook = PraiseBook.Text;
                Gf.PB_ShowWords[3] = 1;
                Gf.PB_ShowWords[4] = 1;
                for (int i = 1; i <= PraiseBookItems.Items.Count; i++)
                {
                    Gf.DocumentSongs[i].Title = Gf.RemoveMusicSym(DataUtil.Trim(PraiseBookItems.Items[i - 1].SubItems[2].Text));
                    Gf.DocumentSongs[i].IdString = DataUtil.Trim(PraiseBookItems.Items[i - 1].SubItems[3].Text);
                    Gf.DocumentSongs[i].NumberAndInfo = "";
                }
                FileUtil.CreateNewFile(Gf.PraiseBookDir + Gf.CurPraiseBook + ".esp");
                gfFileHelpers.SaveIndexFile(Gf.PraiseBookDir + Gf.CurPraiseBook + ".esp", ref PraiseBookItems, UsageMode.PraiseBook, SaveAllItems: true, "", Gf.CurPraiseBookNotes);
            }
        }

        private void SortPraiseBookList()
        {
            if (PraiseBookItems.Items.Count == 0)
            {
                return;
            }
            Cursor = Cursors.WaitCursor;
            PraiseBookItems.Sorting = SortOrder.Ascending;
            PraiseBookItems.Sort();
            if (Gf.UseSongNumbers)
            {
                for (int i = 0; i <= PraiseBookItems.Items.Count - 1; i++)
                {
                    PraiseBookItems.Items[i].SubItems[1].Text = PraiseBookItems.Items[i].SubItems[5].Text;
                }
            }
            else if (!Gf.UseSongNumbers)
            {
                for (int i = 0; i <= PraiseBookItems.Items.Count - 1; i++)
                {
                    PraiseBookItems.Items[i].SubItems[1].Text = Convert.ToString(i + 1);
                }
            }
            PraiseBookItems.Columns[1].Width = -1;
            if (PraiseBookItems.Columns.Count > 0)
            {
                PraiseBookItems.Columns[2].Width = ((PraiseBookItems.Width - PraiseBookItems.Columns[1].Width - 20 >= 0) ? (PraiseBookItems.Width - PraiseBookItems.Columns[1].Width - 20) : 0);
            }
            ShowStatusBarSummary();
            SavePraiseBook();
            Cursor = Cursors.Default;
        }

        private void WorshipList_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                ListViewItem itemAt = WorshipListItems.GetItemAt(e.X, e.Y);
                string OutString = "";
                if (itemAt != null)
                {
                    itemAt.Selected = true;
                    WorshipListIndexChanged();
                }
                Point point = default(Point);
                point = WorshipListItems.PointToClient(Cursor.Position);
                Gf.GetSelectedIndex(WorshipListItems, ref OutString);
                Gf.SetMenuItem(ref CMenuWorship_Edit, OutString, "Edit Item", "Edit", DisableWhenBlank: true);
                bool canPlayMedia = IsSelectedWorshipListMediaItem();
                CMenuWorship_Play.Enabled = canPlayMedia;
                CMenuWorship_PlayOnOutput.Enabled = canPlayMedia;
                CMenuWorship.Show(WorshipListItems, point);
            }
            else
            {
                WorshipListIndexChanged();
            }

            ResetListViewBackgroundColour(WorshipListItems);
            if (WorshipListDoubleClick)
            {
                WorshipListDoubleClick = false;
            }
            else
            {
                WorshipListItems.Focus();
            }
        }

        private bool IsSelectedWorshipListMediaItem()
        {
            if (WorshipListItems.SelectedItems.Count != 1)
            {
                return false;
            }
            ListViewItem selected = WorshipListItems.SelectedItems[0];
            if (selected.SubItems.Count < 2)
            {
                return false;
            }
            string typeAndPath = selected.SubItems[1].Text;
            if (string.IsNullOrEmpty(typeAndPath))
            {
                return false;
            }
            string itemType = DataUtil.Left(typeAndPath, 1);
            return itemType == "M";
        }

        private void ResetListViewBackgroundColour(ListView InListView)
        {
            for (int i = 0; i <= InListView.Items.Count - 1; i++)
            {
                InListView.Items[i].BackColor = SongFolder.BackColor;
            }
        }

        private void WorshipList_KeyUp(object sender, KeyEventArgs e)
        {
            HandleListViewKeyEvent(
                e,
                WorshipList_SelectAll,
                () => { RemoveWorshipListSong(); SaveWorshipList(); },
                WorshipListIndexChanged,
                WorshipListItems
            );
        }

        /// <summary>
        /// Shared key event handling for the list views.
        /// </summary>
        private void HandleListViewKeyEvent(KeyEventArgs e, Action selectAllAction, Action deleteAction, Action indexChangedAction, Control focusControl, Action copyAction = null)
        {
            if (e.Control && e.KeyCode == Keys.A)
            {
                selectAllAction?.Invoke();
                return;
            }

            if (e.Control && e.KeyCode == Keys.C && copyAction != null)
            {
                copyAction?.Invoke();
                return;
            }

            if (e.KeyCode == Keys.Delete)
            {
                deleteAction?.Invoke();
            }
            else
            {
                indexChangedAction?.Invoke();
            }

            focusControl?.Focus();
        }

        //static String prePreviewItemID = "";

        private void WorshipListItems_DoubleClick(object sender, EventArgs e)
        {
            if (Gf.PreviewItem.Type == "M")
            {
                string mediaPath = Gf.PreviewItem.Path;
                if (mediaPath == "")
                {
                    int selectedIndex = Gf.GetSelectedIndex(WorshipListItems);
                    if (selectedIndex >= 0)
                    {
                        string idString = WorshipListItems.Items[selectedIndex].SubItems[1].Text;
                        if (DataUtil.Left(idString, 1) == "M")
                        {
                            mediaPath = DataUtil.Mid(idString, 1);
                        }
                    }
                }
                if (mediaPath != "" && Gf.RunProcess(mediaPath))
                {
                    WorshipListDoubleClick = true;
                    FocusOutputArea();
                    return;
                }
            }
            if (Gf.ShowRunning)
            {
                if (Gf.PreviewItem.ItemID != "")
                {
                    //if(prePreviewItemID != Gf.PreviewItem.ItemID)
                    CopyPreviewToOutput();
                }
            }
            else if (Gf.PreviewItem.ItemID != "")
            {
                //if (prePreviewItemID != Gf.PreviewItem.ItemID)
                //{
                CopyPreviewToOutput();
                GoLive(InStatus: true);
                //}
                // A modal show (single-monitor mode) returns here only when it ends; the main form may have been
                // closed meanwhile.
                if (MainFormClosing)
                {
                    return;
                }
            }
            WorshipListDoubleClick = true;
            FocusOutputArea();

            //prePreviewItemID = Gf.PreviewItem.ItemID;
        }

        private void WorshipList_SelectAll()
        {
            if (WorshipListItems.Items.Count != 0)
            {
                Cursor = Cursors.WaitCursor;
                for (int i = 0; i <= WorshipListItems.Items.Count - 1; i++)
                {
                    WorshipListItems.Items[i].Selected = true;
                }
                Gf.DB_CurSongID = 0;
                ClearLyrics(ref flowLayoutPreviewLyrics);
                ResetMainPictureBox(ref Gf.PreviewItem);
                Cursor = Cursors.Default;
            }
        }

        private void WorshipList_UnselectAll()
        {
            if (WorshipListItems.Items.Count != 0)
            {
                Cursor = Cursors.WaitCursor;
                for (int i = 0; i <= WorshipListItems.Items.Count - 1; i++)
                {
                    WorshipListItems.Items[i].Selected = false;
                }
                Gf.DB_CurSongID = 0;
                ClearLyrics(ref flowLayoutPreviewLyrics);
                WorshipListIndexChanged();
                Cursor = Cursors.Default;
            }
        }

        private void PraiseBookItems_KeyUp(object sender, KeyEventArgs e)
        {
            HandleListViewKeyEvent(
                e,
                PraiseBookList_SelectAll,
                RemovePraiseBookListSong,
                PraiseBookListIndexChanged,
                PraiseBookItems
            );
        }

        private void PraiseBookList_SelectAll()
        {
            if (PraiseBookItems.Items.Count != 0)
            {
                Cursor = Cursors.WaitCursor;
                for (int i = 0; i <= PraiseBookItems.Items.Count - 1; i++)
                {
                    PraiseBookItems.Items[i].Selected = true;
                }
                Gf.DB_CurSongID = 0;
                ClearLyrics(ref flowLayoutPreviewLyrics);
                Cursor = Cursors.Default;
                DisplayLyrics(Gf.PreviewItem, 1, ImageTransitionControl.TransitionAction.None);
            }
        }

        private void PraiseBookList_UnselectAll()
        {
            if (PraiseBookItems.Items.Count != 0)
            {
                Cursor = Cursors.WaitCursor;
                for (int i = 0; i <= PraiseBookItems.Items.Count - 1; i++)
                {
                    PraiseBookItems.Items[i].Selected = false;
                }
                Gf.DB_CurSongID = 0;
                ClearLyrics(ref flowLayoutPreviewLyrics);
                PraiseBookListIndexChanged();
                Cursor = Cursors.Default;
            }
        }

        private void PraiseBookItems_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                ListViewItem itemAt = PraiseBookItems.GetItemAt(e.X, e.Y);
                string OutString = "";
                if (itemAt != null)
                {
                    itemAt.Selected = true;
                    DisplayLyrics(Gf.PreviewItem, 1, ImageTransitionControl.TransitionAction.None);
                }
                Point point = default(Point);
                point = PraiseBookItems.PointToClient(Cursor.Position);
                Gf.GetSelectedIndex(PraiseBookItems, ref OutString, "", 2);
                Gf.SetMenuItem(ref CMenuPraiseB_Edit, OutString, "Edit Item", "Edit", DisableWhenBlank: true);
                CMenuPraiseB.Show(PraiseBookItems, point);
            }
            else
            {
                PraiseBookListIndexChanged();
            }
            PraiseBookItems.Focus();
        }

        private void PraiseBookListIndexChanged()
        {
            int selectedIndex = Gf.GetSelectedIndex(PraiseBookItems);
            if (selectedIndex >= 0)
            {
                string idString = PraiseBookItems.Items[selectedIndex].SubItems[3].Text;
                Gf.PreviewItem.CurItemNo = selectedIndex + 1;
                Gf.PreviewItem.TotalItems = PraiseBookItems.Items.Count;
                Gf.PreviewItem.Source = ItemSource.PraiseBook;
                LoadItem(ref Gf.PreviewItem, idString);
            }
            else
            {
                Gf.PreviewItem.Type = "";
                Gf.PreviewItem.ItemID = "";
                ClearLyrics(ref flowLayoutPreviewLyrics);
            }
        }

        private void Edit_PraiseBookSong()
        {
            if (PraiseBookItems.Items.Count == 0)
            {
                return;
            }
            if (PraiseBookItems.SelectedItems.Count != 1)
            {
                MessageBox.Show("Please select ONE item from the PraiseBook List to edit");
                return;
            }
            string OutString = "PraiseBook List";
            int selectedIndex = Gf.GetSelectedIndex(PraiseBookItems, ref OutString);
            if (selectedIndex >= 0)
            {
                string idString = PraiseBookItems.Items[selectedIndex].SubItems[3].Text;
                Edit_Item(idString);
            }
        }

        private bool AddToWorshipList()
        {
            if (IsSelectedTab(tabControlSource, "tabFolders"))
            {
                return AddFromSongsList();
            }
            if (IsSelectedTab(tabControlSource, "tabFiles"))
            {
                return AddFromInfoScreensList();
            }
            if (IsSelectedTab(tabControlSource, "tabPowerpoint"))
            {
                return AddFromPowerpointList();
            }
            if (IsSelectedTab(tabControlSource, "tabBibles"))
            {
                return AddFromHolyBible();
            }
            if (IsSelectedTab(tabControlSource, "tabMedia"))
            {
                return AddFromMediaFilesList();
            }
            return AddFromPreview();
        }

        private bool AddFromPreview()
        {
            return AddFromPreview(GetWorshipListNextSelectedLoc());
        }

        private bool AddFromPreview(int AddToLocation)
        {
            if (Gf.PreviewItem.Source == ItemSource.SongsList)
            {
                return AddFromSongsList(AddToLocation);
            }
            if (Gf.PreviewItem.Source == ItemSource.ExternalFileInfoScreen)
            {
                return AddFromInfoScreensList(AddToLocation);
            }
            if (Gf.PreviewItem.Source == ItemSource.ExternalFileMedia)
            {
                return AddFromMediaFilesList(AddToLocation);
            }
            if (Gf.PreviewItem.Source == ItemSource.ExternalFilePowerpoint)
            {
                return AddFromPowerpointList();
            }
            if (Gf.PreviewItem.Source == ItemSource.HolyBible)
            {
                return AddFromHolyBible(AddToLocation);
            }
            return false;
        }

        private bool AddFromSongsList()
        {
            return AddFromSongsList(GetWorshipListNextSelectedLoc());
        }

        private bool AddFromSongsList(int AddToLocation)
        {
            if (SongsList.Items.Count == 0 || SongsList.SelectedItems.Count == 0)
            {
                return false;
            }
            ListViewItem listViewItem = new ListViewItem();
            string formatData = "";
            Cursor = Cursors.WaitCursor;
            if (AddToLocation < 0 || AddToLocation > WorshipListItems.Items.Count)
            {
                AddToLocation = WorshipListItems.Items.Count;
            }
            int addedCount = 0;
            Gf.TotalMusicFiles = -1;
            try
            {
                string searchString = "Select * from SONG";
                using DataTable dt = DbController.GetDataTable(Gf.ConnectStringMainDB, searchString);

                DataColumn[] primarykey = new DataColumn[] { dt.Columns["SONGID"] };
                dt.PrimaryKey = primarykey;

                for (int i = 0; i < SongsList.SelectedItems.Count; i++)
                {
                    string idString = SongsList.SelectedItems[i].SubItems[1].Text;
                    string songId = DataUtil.Mid(idString, 1, idString.Length - 1);
                    long songIdNumber = DataUtil.StringToInt(songId);
                    string idPrefix = DataUtil.Left(idString, 0);
                    try
                    {
                        DataRow dr = dt.Rows.Find(songId);
                        if (dr != null)
                        {
                            string title = DataUtil.GetDataString(dr, "Title_1");
                            string dataString = DataUtil.GetDataString(dr, "Title_2");
                            if (Gf.MusicFound(title, dataString))
                            {
                                title += " <#>";
                            }
                            formatData = DataUtil.GetDataString(dr, "FORMATDATA");
                            listViewItem = WorshipListItems.Items.Insert(AddToLocation, title);
                            listViewItem.ImageIndex = 0;
                            listViewItem.SubItems.Add(SongsList.SelectedItems[i].SubItems[1].Text);
                            listViewItem.SubItems.Add(formatData);
                            listViewItem.SubItems.Add(SongsList.SelectedItems[i].SubItems[4].Text);
                            listViewItem.SubItems.Add(SongsList.SelectedItems[i].SubItems[5].Text);
                            listViewItem.SubItems.Add(SongsList.SelectedItems[i].SubItems[6].Text);
                            listViewItem.SubItems.Add("");
                            // The item's own folder (not the browsing text, e.g. "Search Results:"): saved as <Folder>.
                            int folderNo = DataUtil.GetDataInt(dr, "FolderNo");
                            listViewItem.SubItems.Add(folderNo > 0 && folderNo < Gf.Folders.Length ? Gf.Folders[folderNo].Name : SongFolder.Text);
                            AddToLocation++;
                            addedCount++;
                        }
                    }
                    catch
                    {
                    }
                }
            }
            catch
            {
            }
            SetWorshipPraiseListColWidth();
            if (addedCount == 0)
            {
                Cursor = Cursors.Default;
                return false;
            }
            ShowStatusBarSummary();
            SelectWorshipListItem(AddToLocation - addedCount, addedCount);
            SaveWorshipList();
            UpdateOutputCurItemNo();
            Cursor = Cursors.Default;
            return true;
        }

        private int GetWorshipListNextSelectedLoc()
        {
            int selectedIndex = Gf.GetSelectedIndex(WorshipListItems);
            return (selectedIndex < 0) ? WorshipListItems.Items.Count : (selectedIndex + 1);
        }

        private void SelectWorshipListItem(int InIndex, int SelCount)
        {
            if (InIndex >= 0)
            {
                for (int i = 0; i < WorshipListItems.Items.Count; i++)
                {
                    if (i >= InIndex && i < InIndex + SelCount)
                    {
                        WorshipListItems.Items[i].Selected = true;
                    }
                    else
                    {
                        WorshipListItems.Items[i].Selected = false;
                    }
                }
            }
            WorshipListIndexChanged();
        }

        private bool AddFromInfoScreensList()
        {
            return AddFromInfoScreensList(GetWorshipListNextSelectedLoc());
        }

        private bool AddFromInfoScreensList(int AddToLocation)
        {
            if (InfoScreenList.SelectedItems.Count > 0)
            {
                if (AddToLocation < 0)
                {
                    AddToLocation = WorshipListItems.Items.Count;
                }
                int addedCount = 0;
                for (int i = 0; i < InfoScreenList.SelectedItems.Count; i++)
                {
                    AddExternalFileToWorshipList(DataUtil.Mid(InfoScreenList.SelectedItems[i].SubItems[1].Text, 1), AddToLocation + addedCount);
                    addedCount++;
                }
                if (InfoScreenList.SelectedItems.Count > 0)
                {
                    SelectWorshipListItem(AddToLocation, addedCount);
                }
                return true;
            }
            return false;
        }

        private bool AddFromMediaFilesList()
        {
            return AddFromMediaFilesList(GetWorshipListNextSelectedLoc());
        }

        private bool AddFromMediaFilesList(int AddToLocation)
        {
            if (MediaList.SelectedItems.Count > 0)
            {
                if (AddToLocation < 0)
                {
                    AddToLocation = WorshipListItems.Items.Count;
                }
                int addedCount = 0;
                for (int i = 0; i < MediaList.SelectedItems.Count; i++)
                {
                    AddExternalFileToWorshipList(DataUtil.Mid(MediaList.SelectedItems[i].SubItems[1].Text, 1), AddToLocation + addedCount);
                    addedCount++;
                }
                if (MediaList.SelectedItems.Count > 0)
                {
                    SelectWorshipListItem(AddToLocation, addedCount);
                }
                return true;
            }
            return false;
        }

        private bool AddFromPowerpointList()
        {
            return AddFromPowerpointList(GetWorshipListNextSelectedLoc());
        }

        private bool AddFromPowerpointList(int AddToLocation)
        {
            if (Gf.PowerpointListingStyle == 0)
            {
                if (PowerpointList.SelectedItems.Count > 0)
                {
                    if (AddToLocation < 0)
                    {
                        AddToLocation = WorshipListItems.Items.Count;
                    }
                    int addedCount = 0;
                    for (int i = 0; i < PowerpointList.SelectedItems.Count; i++)
                    {
                        if (!AddExternalFileToWorshipList(DataUtil.Mid(PowerpointList.SelectedItems[i].SubItems[1].Text, 1), AddToLocation + addedCount))
                        {
                            return false;
                        }
                        addedCount++;
                    }
                    if (PowerpointList.SelectedItems.Count > 0)
                    {
                        SelectWorshipListItem(AddToLocation, addedCount);
                    }
                    return true;
                }
                return false;
            }
            if (AddExternalFileToWorshipList(DataUtil.Mid(PowerpointCurPreview, 1), AddToLocation))
            {
                return true;
            }
            return false;
        }

        private void AddToPraiseBookList()
        {
            if (DataUtil.Trim(PraiseBook.Text) != "" && SongsList.SelectedItems.Count >= 1)
            {
                string FirstCharSym = "";
                ListViewItem listViewItem = new ListViewItem();
                string songNumber = "000000";
                Cursor = Cursors.WaitCursor;
                PraiseBookItems.Sorting = SortOrder.None;
                ListViewItem[] newItems = new ListViewItem[SongsList.SelectedItems.Count];
                PraiseBookItems.BeginUpdate();
                for (int i = 0; i < SongsList.SelectedItems.Count; i++)
                {
                    string title = Gf.RemoveMusicSym(SongsList.SelectedItems[i].Text);
                    songNumber = (Gf.UseSongNumbers ? DataUtil.Format6(SongsList.SelectedItems[i].SubItems[4].Text) : "");
                    newItems[i] = new ListViewItem(new string[6]
                    {
                        songNumber + DataUtil.GetCJKTitle(title, Gf.PB_CJKGroupStyle, ref FirstCharSym),
                        " ",
                        title,
                        SongsList.SelectedItems[i].SubItems[1].Text,
                        FirstCharSym,
                        SongsList.SelectedItems[i].SubItems[4].Text
                    });
                }
                PraiseBookItems.Items.AddRange(newItems);
                SortPraiseBookList();
                PraiseBookItems.EndUpdate();
                Cursor = Cursors.Default;
            }
        }

        private void OldAddToPraiseBookList()
        {
            if (DataUtil.Trim(PraiseBook.Text) == "")
            {
                return;
            }
            string FirstCharSym = "";
            ListViewItem listViewItem = new ListViewItem();
            string songNumber = "000000";
            Cursor = Cursors.WaitCursor;
            PraiseBookItems.Sorting = SortOrder.None;
            if (Gf.UseSongNumbers)
            {
                for (int i = 0; i < SongsList.SelectedItems.Count; i++)
                {
                    string title = Gf.RemoveMusicSym(SongsList.SelectedItems[i].Text);
                    songNumber = DataUtil.Format6(SongsList.SelectedItems[i].SubItems[4].Text);
                    listViewItem = PraiseBookItems.Items.Add(songNumber + DataUtil.GetCJKTitle(title, Gf.PB_CJKGroupStyle, ref FirstCharSym));
                    listViewItem.SubItems.Add(" ");
                    listViewItem.SubItems.Add(title);
                    listViewItem.SubItems.Add(SongsList.SelectedItems[i].SubItems[1].Text);
                    listViewItem.SubItems.Add(FirstCharSym);
                    listViewItem.SubItems.Add(SongsList.SelectedItems[i].SubItems[4].Text);
                }
            }
            else
            {
                for (int i = 0; i < SongsList.SelectedItems.Count; i++)
                {
                    string title = Gf.RemoveMusicSym(SongsList.SelectedItems[i].Text);
                    listViewItem = PraiseBookItems.Items.Add(DataUtil.GetCJKTitle(title, Gf.PB_CJKGroupStyle, ref FirstCharSym));
                    listViewItem.SubItems.Add(" ");
                    listViewItem.SubItems.Add(title);
                    listViewItem.SubItems.Add(SongsList.SelectedItems[i].SubItems[1].Text);
                    listViewItem.SubItems.Add(FirstCharSym);
                    listViewItem.SubItems.Add(SongsList.SelectedItems[i].SubItems[4].Text);
                }
            }
            SortPraiseBookList();
            Cursor = Cursors.Default;
        }

        private void SongsList_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (Gf.GetSelectedIndex(SongsList) >= 0)
            {
                if (Gf.EasiSlidesMode == UsageMode.Worship)
                {
                    AddToWorshipList();
                }
                else
                {
                    AddToPraiseBookList();
                }
            }
        }

        private void WL_Btn_MouseUp(object sender, MouseEventArgs e)
        {
            ToolStripButton toolStripButton = (ToolStripButton)sender;
            string name = toolStripButton.Name;
            if (name == "WL_Manage")
            {
                WorshipListsManage();
            }
            else if (name == "WL_Add")
            {
                AddToWorshipList();
            }
            else if (name == "WL_Open")
            {
                LocateFileWorshipList();
            }
            else if (name == "WL_Up")
            {
                MoveSongUp();
                SaveWorshipList();
            }
            else if (name == "WL_Down")
            {
                MoveSongDown();
                SaveWorshipList();
            }
            else if (name == "WL_Delete")
            {
                RemoveWorshipListSong();
                SaveWorshipList();
            }
            else if (name == "WL_Word")
            {
                SetDocLayout(PraiseBookLayout.WorshipList);
            }
            else if (name == "WL_Notes")
            {
                EditSessionNotes();
            }
        }

        private void EditSessionNotes()
        {
            Gf.EditNotes = Gf.CurSessionNotes;
            Gf.EditNotesHeading = "Edit Session Notes";
            FrmEditNotes frmEditNotes = new FrmEditNotes();
            if (frmEditNotes.ShowDialog() == DialogResult.OK)
            {
                Gf.CurSessionNotes = Gf.EditNotes;
                PreviewNotes.Text = Gf.CurSessionNotes;
                SaveWorshipList();
            }
        }

        private void PB_Btn_MouseUp(object sender, MouseEventArgs e)
        {
            ToolStripButton toolStripButton = (ToolStripButton)sender;
            string name = toolStripButton.Name;
            if (name == "PB_Manage")
            {
                PraiseBooksManage();
            }
            else if (name == "PB_Add")
            {
                AddToPraiseBookList();
            }
            else if (name == "PB_WordCount")
            {
                SetSortButtonPB(toolStripButton.Checked ? SortBy.WordCount : SortBy.Alpha);
            }
            else if (name == "PB_Delete")
            {
                RemovePraiseBookListSong();
            }
            else if (name == "PB_Word")
            {
                SetDocLayout(PraiseBookLayout.PraiseBook);
            }
            else if (name == "PB_Html")
            {
                GenerateHTMLPB();
            }
        }

        private void GenerateHTMLPB()
        {
            SavePraiseBook();
            if (PraiseBookItems.Items.Count > 0)
            {
                if (ValidatePraiseBookItems() < 0)
                {
                    FrmGenerateHtml frmGenerateHtml = new FrmGenerateHtml();
                    frmGenerateHtml.ShowDialog();
                }
            }
            else
            {
                MessageBox.Show("Can't Generate because the PraiseBook List is empty!");
            }
        }

        // ----- Folder sort menu (openspec change add-song-number-sort, design N3) -----

        /// <summary>The choices of the folder sort menu, in menu order.</summary>
        private static readonly (SortBy Mode, string Text)[] FolderSortChoices =
        {
            (SortBy.Alpha, "Sort by Title"),
            (SortBy.WordCount, "Sort by CJK Word Count"),
            (SortBy.Number, "Sort by Song Number"),
        };

        // Opened by Folders_WordCount; built once in InitFormControls (before ApplyModernUi, so the modern
        // look's renderer is given to it and taken back with the other context menus).
        private ContextMenuStrip folderSortMenu;

        // InMusicOnly of the last Find whose results are shown ("Search Results:"), to refill them in another order.
        private bool searchResultsMusicOnly;

        // The search statement (Gf.Find_SQLString at that time) of the shown "Search Results:".
        private string searchResultsSql;

        private static string FolderSortToolTip(SortBy mode)
        {
            foreach (var (choice, toolTip) in FolderSortChoices)
            {
                if (choice == mode)
                {
                    return toolTip;
                }
            }
            return FolderSortChoices[0].Text;
        }

        /// <summary>
        /// The menu of the folder sort button. The designer button stays a ToolStripButton (no designer type
        /// change, every existing reference keeps working); it opens this menu instead of toggling.
        /// </summary>
        private void BuildFolderSortMenu()
        {
            // The button has no text: without a name, accessibility tools list it as a nameless check box.
            Folders_WordCount.AccessibleName = "Song list sort order";
            folderSortMenu = new ContextMenuStrip(components) { Name = "CMenuFolderSort" };
            foreach (var (mode, label) in FolderSortChoices)
            {
                ToolStripMenuItem item = new ToolStripMenuItem(label)
                {
                    Name = "FolderSort_" + mode,
                    Tag = mode,
                };
                item.Click += FolderSortMenuItem_Click;
                folderSortMenu.Items.Add(item);
            }
        }

        /// <summary>
        /// Click (left mouse button, keyboard or accessibility Invoke) opens the sort menu below the button,
        /// the current order checked. Opening it writes nothing.
        /// </summary>
        private void Folders_WordCount_Click(object sender, EventArgs e)
        {
            if (!Folders_WordCount.Enabled || folderSortMenu == null)
            {
                return;
            }
            foreach (ToolStripItem entry in folderSortMenu.Items)
            {
                if (entry is ToolStripMenuItem item && item.Tag is SortBy mode)
                {
                    item.Checked = mode == CurStyle;
                }
            }
            // Bounds are relative to the strip that shows the button (the overflow drop-down when it overflows).
            ToolStrip owner = Folders_WordCount.GetCurrentParent() ?? toolStripFolders;
            Rectangle bounds = Folders_WordCount.Bounds;
            folderSortMenu.Show(owner, new Point(bounds.Left, bounds.Bottom));
        }

        private void FolderSortMenuItem_Click(object sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem { Tag: SortBy mode })
            {
                ChangeFolderSortStyle(mode);
            }
        }

        /// <summary>
        /// Makes mode the current folder's sort order: stored for that folder (FOLDER.GroupStyle) and the list
        /// refilled. Choosing the current order does nothing. On "Search Results:" there is no folder: the
        /// results are refilled in the new order and nothing is stored (before, folder 1 got the order).
        /// </summary>
        private void ChangeFolderSortStyle(SortBy mode)
        {
            if (mode == CurStyle)
            {
                return;
            }
            int folderNumber = Gf.GetFolderNumber(SongFolder.Text, ZeroIfInvalid: true);
            if (folderNumber <= 0)
            {
                // No folder: only the shown search results can be re-sorted, and only from the query that
                // produced them (Gf.Find_SQLString may have been cleared or replaced by the Find dialog since).
                // Without it the list and the button stay as they are.
                if (SongFolder.Text != SongSearchResultsFolder || string.IsNullOrEmpty(searchResultsSql))
                {
                    return;
                }
                SetSortButton(mode);
                string findSql = Gf.Find_SQLString;
                Gf.Find_SQLString = searchResultsSql;
                try
                {
                    SongsList.Items.Clear();
                    FillList(-1, "", searchResultsMusicOnly);
                }
                finally
                {
                    Gf.Find_SQLString = findSql;
                }
                return;
            }
            SetSortButton(mode);
            Gf.Folders[folderNumber].GroupStyle = CurStyle;
            Gf.SaveConfigSettings();
            SongFolder_Change();
        }

        //private void Images_Btn_MouseUp(object sender, MouseEventArgs e)
        //{
        //	ToolStripButton toolStripButton = (ToolStripButton)sender;
        //	string name = toolStripButton.Name;
        //	if (name == "Images_Left")
        //	{
        //		LoadBackgroundThumbImages();
        //	}
        //	else if (name == "Images_Right")
        //	{
        //		LoadBackgroundThumbImages();
        //	}
        //}

        private void Bibles_Btn_MouseUp(object sender, MouseEventArgs e)
        {
            ToolStripButton toolStripButton = (ToolStripButton)sender;
            string name = toolStripButton.Name;
            if (name == "Bibles_ShowVerses")
            {
                TabBibleVersionsChanged();
            }
            else if (name == "Bibles_Go")
            {
                BibleUserLookup_Submit();
            }
        }

        private void BibleUserLookup_Submit()
        {
            string inText = DataUtil.Trim(BibleUserLookup.Text);
            if (inText == "")
            {
                return;
            }
            int startChapterNo = 0;
            int startVerseNo = 0;
            int endChapterNo = 0;
            int endVerseNo = 0;
            bool isVerseLookup = BibleUserLookupValidation(ref inText, ref startChapterNo, ref startVerseNo, ref endChapterNo, ref endVerseNo);
            if (isVerseLookup)
            {
                BibleUserLookup.Text = inText;
                BibleUserLookup_ShowVerses();
                return;
            }
            bool hasLetters = false;
            foreach (char c in inText)
            {
                if (char.IsLetter(c))
                {
                    hasLetters = true;
                    break;
                }
            }
            if (hasLetters)
            {
                if (Gf.HB_TotalVersions < 1)
                {
                    return;
                }
                Gf.FindBibleVerses = true;
                Gf.FindFolderItems = false;
                Gf.HB_SQLString = Gf.BuildBibleSearchString(inText, TabBibleVersions.SelectedIndex);
                BibleVerseSearch();
            }
        }

        private void BibleUserLookup_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Return)
            {
                e.SuppressKeyPress = true;
                BibleUserLookup_Submit();
            }
        }

        private bool AddFromHolyBible()
        {
            return AddFromHolyBible(GetWorshipListNextSelectedLoc());
        }

        private bool AddFromHolyBible(int AddToLocation)
        {
            if (BibleText.Text == "" || BibleText.SelectionLength < 1)
            {
                return false;
            }
            if (Gf.PreviewItem.Type != "B" || Gf.PreviewItem.Source != ItemSource.HolyBible)
            {
                HB_SelectedPassagesChanged(HB_CurSelectedPassages, ref HB_CurSelectedTitle);
            }
            ListViewItem listViewItem = new ListViewItem();
            if (AddToLocation < 0 || AddToLocation > WorshipListItems.Items.Count)
            {
                AddToLocation = WorshipListItems.Items.Count;
            }
            listViewItem = WorshipListItems.Items.Insert(AddToLocation, HB_CurSelectedTitle);
            listViewItem.ImageIndex = 4;
            listViewItem.SubItems.Add("B" + HB_CurSelectedPassages);
            listViewItem.SubItems.Add(HB_CurSelectedFormat);
            listViewItem.SubItems.Add("");
            listViewItem.SubItems.Add("");
            listViewItem.SubItems.Add("");
            listViewItem.SubItems.Add("");
            listViewItem.SubItems.Add("");
            listViewItem.SubItems.Add("");
            ShowStatusBarSummary();
            SelectWorshipListItem(AddToLocation, 1);
            SaveWorshipList();
            UpdateOutputCurItemNo();
            return true;
        }

        private void UpdateOutputCurItemNo()
        {
            if (WorshipListItems.Items.Count == 0)
            {
                if (Gf.ShowRunning)
                {
                    GoLive(InStatus: false);
                }
                Gf.OutputItem.CurItemNo = 0;
                Gf.StartPresAt = 0;
                return;
            }
            int rowIndex = 0;
            while (true)
            {
                if (rowIndex <= WorshipListItems.Items.Count - 1)
                {
                    if (WorshipListItems.Items[rowIndex].SubItems[6].Text == "O")
                    {
                        Gf.StartPresAt = rowIndex + 1;
                        if (Gf.OutputItem.CurItemNo != 0)
                        {
                            Gf.OutputItem.CurItemNo = Gf.StartPresAt;
                        }
                        WorshipListItems.Items[rowIndex].EnsureVisible();
                        break;
                    }
                    rowIndex++;
                    continue;
                }
                if (WorshipListItems.Items.Count <= 0)
                {
                    break;
                }
                if (Gf.StartPresAt > WorshipListItems.Items.Count)
                {
                    Gf.StartPresAt = WorshipListItems.Items.Count;
                }
                if (Gf.OutputItem.CurItemNo != 0)
                {
                    if (Gf.StartPresAt == 0)
                    {
                        Gf.StartPresAt = 1;
                    }
                    WorshipListItems.Items[Gf.StartPresAt - 1].SubItems[6].Text = "O";
                    WorshipListItems.Items[Gf.StartPresAt - 1].EnsureVisible();
                    Gf.OutputItem.CurItemNo = Gf.StartPresAt;
                }
                break;
            }
            if (Gf.ShowRunning)
            {
                ValidateWorshipListItems(ShowErrorMessage: false);
                RemoteControlLiveShow(LiveShowAction.Remote_WorshipListChanged);
            }
        }

        private void HB_SelectedPassagesChanged(string InBibleString, ref string InTitle)
        {
            Cursor = Cursors.WaitCursor;
            Gf.PreviewItem.Source = ItemSource.HolyBible;
            if (InBibleString != "")
            {
                string idString = "B" + InBibleString;
                Gf.PreviewItem.InMainItemText = InTitle;
                Gf.PreviewItem.InSubItemItem1Text = idString;
                Gf.PreviewItem.CurItemNo = 0;
                Gf.PreviewItem.Type = "B";
                LoadItem(ref Gf.PreviewItem, idString, "", 1, ref InTitle, ScrollToCaret: true);
                UpdateDisplayPanelFields();
            }
            else
            {
                Gf.PreviewItem.Type = "";
                Gf.PreviewItem.Title = "";
                Gf.PreviewItem.ItemID = "";
                Gf.PreviewItem.CurItemNo = 0;
                Gf.LoadIndividualFormatData(ref Gf.PreviewItem, "");
                AllowIndividualFormat(AllowFormat: false);
                UpdateFormatFields();
                BuildVerseButtons(Gf.PreviewItem);
                DisplayLyrics(Gf.PreviewItem, 1);
                UpdateDisplayPanelFields();
            }
            Cursor = Cursors.Default;
        }

        private void BibleText_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                HB_StartBuildStringProcess();
            }
        }

        private void BibleText_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.A)
            {
                HB_SelectAll();
            }
            else if (e.Control && e.KeyCode == Keys.C)
            {
                HB_CopyText();
            }
            else if (e.KeyCode == Keys.ShiftKey)
            {
                HB_StartBuildStringProcess();
            }
        }

        private void BibleText_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (BibleText.Text != "")
                {
                    Point point = default(Point);
                    point = BibleText.PointToClient(Cursor.Position);
                    int charIndexFromPosition = BibleText.GetCharIndexFromPosition(point);
                    if (!(charIndexFromPosition < BibleText.SelectionStart || charIndexFromPosition > BibleText.SelectionStart + BibleText.SelectionLength - 1))
                    {
                        string data = "";
                        BibleText.DoDragDrop(new DataObject(DragDropSource.BiblePassage.ToString(), data), DragDropEffects.Link);
                    }
                }
            }
            else if (e.Button == MouseButtons.Right)
            {
                for (int i = 0; i <= CMenuBible.Items.Count - 1; i++)
                {
                    CMenuBible.Items[i].Enabled = base.Enabled;
                }
                if (BibleText.Text == "")
                {
                    CMenuBible.Items[0].Enabled = false;
                    CMenuBible.Items[1].Enabled = false;
                    CMenuBible.Items[2].Enabled = false;
                }
                if (BibleText.SelectedText != "")
                {
                    CMenuBible.Items[4].Enabled = true;
                    BuildBibleTextR2SubMenus();
                }
            }
        }

        private void BuildBibleTextR2SubMenus()
        {
            CMenuBible_AddRegion2.DropDownItems.Clear();
            ToolStripMenuItem toolStripMenuItem = new ToolStripMenuItem();
            for (int i = 0; i <= Gf.HB_TotalVersions - 1; i++)
            {
                toolStripMenuItem = new ToolStripMenuItem();
                toolStripMenuItem.Text = Gf.HB_Versions[i, 1];
                CMenuBible_AddRegion2.DropDownItems.Add(toolStripMenuItem);
                CMenuBible_AddRegion2.DropDownItems[i].Tag = i.ToString();
            }
            EventHandler value = new EventHandler(BibleR2MenuClickHandler).Invoke;
            foreach (ToolStripMenuItem dropDownItem in CMenuBible_AddRegion2.DropDownItems)
            {
                dropDownItem.Click += value;
            }
        }

        private void BibleUserLookup_ShowVerses()
        {
            Cursor = Cursors.WaitCursor;
            string InText = BibleUserLookup.Text;
            int StartChapterNo = 0;
            int StartVerseNo = 0;
            int EndChapterNo = 0;
            int EndVerseNo = 0;
            bool isValid = BibleUserLookupValidation(ref InText, ref StartChapterNo, ref StartVerseNo, ref EndChapterNo, ref EndVerseNo);
            BibleUserLookup.Text = InText;
            if (isValid)
            {
                int startIndex = 0;
                int endIndex = 0;
                for (int i = 1; i <= Gf.HB_VersesLocation[0].Version; i++)
                {
                    if (StartChapterNo == Gf.HB_VersesLocation[i].Chapter && StartVerseNo == Gf.HB_VersesLocation[i].Verse)
                    {
                        startIndex = i;
                    }
                }
                if (startIndex > 0)
                {
                    for (int i = startIndex; i <= Gf.HB_VersesLocation[0].Version; i++)
                    {
                        if (EndChapterNo >= Gf.HB_VersesLocation[i].Chapter && EndVerseNo >= Gf.HB_VersesLocation[i].Verse)
                        {
                            endIndex = i;
                        }
                    }
                    if (endIndex > 0)
                    {
                        EndVerseNo = Gf.HB_VersesLocation[endIndex].Verse;
                        BibleText.SelectionLength = 0;
                        BibleText.SelectionStart = Gf.HB_VersesLocation[startIndex].Start;
                        BibleText.ScrollToCaret();
                        if (EndChapterNo < 1)
                        {
                            if (StartVerseNo > 0)
                            {
                                HB_StartBuildStringProcess();
                            }
                            BibleUserLookup.Focus();
                        }
                        else if (EndVerseNo > 0)
                        {
                            BibleText.SelectionLength = Gf.HB_VersesLocation[endIndex].Start - Gf.HB_VersesLocation[startIndex].Start + Gf.HB_VersesLocation[endIndex].Length - 2;
                            HB_StartBuildStringProcess();
                            BibleUserLookup.Focus();
                        }
                    }
                }
            }
            Cursor = Cursors.Default;
        }

        private bool BibleUserLookupValidation(ref string InText, ref int StartChapterNo, ref int StartVerseNo, ref int EndChapterNo, ref int EndVerseNo)
        {
            if (BookLookup.Text == "" || BookLookup.SelectedIndex == 66 || DataUtil.Trim(InText) == "")
            {
                return false;
            }
            string reference = DataUtil.Trim(InText);
            if (reference.IndexOf("  ") >= 0)
            {
                return false;
            }
            reference = reference.Replace('.', ':');
            reference = reference.Replace('v', ':');
            reference = reference.Replace('V', ':');
            string startPart = reference;
            string endPart = "";
            bool chapterOnly = true;
            if (reference.IndexOf('-') < 0)
            {
                int spaceIndex = reference.IndexOf(' ');
                if (spaceIndex >= 0)
                {
                    int colonIndex = reference.IndexOf(':');
                    if (colonIndex >= 0 && colonIndex < spaceIndex)
                    {
                        reference = reference.Insert(spaceIndex, "-");
                        reference = reference.Remove(spaceIndex + 1, 1);
                    }
                    else
                    {
                        spaceIndex = reference.IndexOf(' ', spaceIndex + 1);
                        if (spaceIndex >= 0)
                        {
                            reference = reference.Insert(spaceIndex, "-");
                            reference = reference.Remove(spaceIndex + 1, 1);
                        }
                        else
                        {
                            reference = reference.Replace(" ", "-");
                        }
                    }
                }
            }
            if (reference.IndexOf('-') >= 0)
            {
                sArray = reference.Split('-');
                startPart = DataUtil.Trim(sArray[0]);
                endPart = DataUtil.Trim(sArray[1]);
            }
            else if (reference.IndexOf(' ') >= 0)
            {
                sArray = reference.Split('-');
                startPart = DataUtil.Trim(sArray[0]);
                endPart = DataUtil.Trim(sArray[1]);
            }
            startPart = startPart.Replace(' ', ':');
            endPart = endPart.Replace(' ', ':');
            InText = startPart + ((endPart != "") ? ("-" + endPart) : "");
            StartChapterNo = 1;
            StartVerseNo = 1;
            EndChapterNo = 1;
            EndVerseNo = 1;
            sArray = startPart.Split(':');
            StartChapterNo = DataUtil.StringToInt(sArray[0]);
            if (StartChapterNo <= 0)
            {
                return false;
            }
            if (sArray.GetUpperBound(0) < 1)
            {
                StartVerseNo = 1;
            }
            else
            {
                StartVerseNo = 1;
                int startVerse = DataUtil.StringToInt(sArray[1]);
                if (startVerse > 0)
                {
                    StartVerseNo = startVerse;
                }
                chapterOnly = false;
            }
            if (endPart == "")
            {
                EndChapterNo = StartChapterNo;
                if (chapterOnly)
                {
                    EndVerseNo = 200;
                }
                else
                {
                    EndVerseNo = StartVerseNo;
                }
            }
            else
            {
                sArray = endPart.Split(':');
                if (sArray.GetUpperBound(0) < 1)
                {
                    if (chapterOnly)
                    {
                        EndChapterNo = DataUtil.StringToInt(sArray[0]);
                        EndVerseNo = 200;
                    }
                    else
                    {
                        EndChapterNo = StartChapterNo;
                        EndVerseNo = DataUtil.StringToInt(sArray[0]);
                    }
                }
                else
                {
                    EndChapterNo = DataUtil.StringToInt(sArray[0]);
                    EndVerseNo = 1;
                    EndVerseNo = DataUtil.StringToInt(sArray[1]);
                }
            }
            return true;
        }

        private void MoveSongUp()
        {
            int count = WorshipListItems.Items.Count;
            if (count < 1)
            {
                return;
            }
            int selectedIndex = Gf.GetSelectedIndex(WorshipListItems);
            if (selectedIndex >= 1)
            {
                TimerFlasher.Stop();
                for (int i = 0; i <= 7; i++)
                {
                    string cellText = WorshipListItems.Items[selectedIndex].SubItems[i].Text;
                    WorshipListItems.Items[selectedIndex].SubItems[i].Text = WorshipListItems.Items[selectedIndex - 1].SubItems[i].Text;
                    WorshipListItems.Items[selectedIndex - 1].SubItems[i].Text = cellText;
                }
                int imageIndex = WorshipListItems.Items[selectedIndex].ImageIndex;
                WorshipListItems.Items[selectedIndex].ImageIndex = WorshipListItems.Items[selectedIndex - 1].ImageIndex;
                WorshipListItems.Items[selectedIndex - 1].ImageIndex = imageIndex;
                WorshipListItems.Items[selectedIndex].Selected = false;
                WorshipListItems.Items[selectedIndex - 1].Selected = true;
                Gf.PreviewItem.CurItemNo = selectedIndex;
                UpdateOutputCurItemNo();
                UpdateWorshipShowIcons();
                // Both calls above scroll to the output row; keep the moved row in view.
                WorshipListItems.EnsureVisible(selectedIndex - 1);
                TimerFlasher.Start();
            }
        }

        private void MoveSongDown()
        {
            int count = WorshipListItems.Items.Count;
            if (count <= 1)
            {
                return;
            }
            int selectedIndex = Gf.GetSelectedIndex(WorshipListItems);
            if (!(selectedIndex < 0 || selectedIndex == count - 1))
            {
                TimerFlasher.Stop();
                for (int i = 0; i <= 7; i++)
                {
                    string cellText = WorshipListItems.Items[selectedIndex].SubItems[i].Text;
                    WorshipListItems.Items[selectedIndex].SubItems[i].Text = WorshipListItems.Items[selectedIndex + 1].SubItems[i].Text;
                    WorshipListItems.Items[selectedIndex + 1].SubItems[i].Text = cellText;
                }
                int imageIndex = WorshipListItems.Items[selectedIndex].ImageIndex;
                WorshipListItems.Items[selectedIndex].ImageIndex = WorshipListItems.Items[selectedIndex + 1].ImageIndex;
                WorshipListItems.Items[selectedIndex + 1].ImageIndex = imageIndex;
                WorshipListItems.Items[selectedIndex].Selected = false;
                WorshipListItems.Items[selectedIndex + 1].Selected = true;
                Gf.PreviewItem.CurItemNo = selectedIndex + 2;
                UpdateOutputCurItemNo();
                UpdateWorshipShowIcons();
                // Both calls above scroll to the output row; keep the moved row in view.
                WorshipListItems.EnsureVisible(selectedIndex + 1);
                TimerFlasher.Start();
            }
        }

        private bool RemoveWorshipListSong()
        {
            return RemoveWorshipListSong(UpdateCurItemNo: true);
        }

        private bool RemoveWorshipListSong(bool UpdateCurItemNo)
        {
            WriteDebugLog($"RemoveWorshipListSong() called - Current count: {WorshipListItems.Items.Count}");
            bool removedOutputItem = false;
            int lastRemovedIndex = -1;
            if (WorshipListItems.Items.Count == 0)
            {
                WriteDebugLog("RemoveWorshipListSong() - No items to remove");
                return false;
            }
            int removedCount = 0;
            for (int i = WorshipListItems.Items.Count - 1; i >= 0; i--)
            {
                if (WorshipListItems.Items[i].Selected)
                {
                    WorshipListItems.Items[i].Remove();
                    lastRemovedIndex = i;
                    removedCount++;
                }
            }
            WriteDebugLog($"RemoveWorshipListSong() - Removed {removedCount} items, New count: {WorshipListItems.Items.Count}");
            if (!UpdateCurItemNo)
            {
                return false;
            }
            if (Gf.OutputItem.CurItemNo == lastRemovedIndex + 1)
            {
                removedOutputItem = true;
            }
            if (WorshipListItems.Items.Count > lastRemovedIndex)
            {
                if (lastRemovedIndex >= 0)
                {
                    WorshipListItems.Items[lastRemovedIndex].Selected = true;
                }
            }
            else if (WorshipListItems.Items.Count > 0)
            {
                if (lastRemovedIndex - 1 >= 0 && lastRemovedIndex <= WorshipListItems.Items.Count)
                {
                    WorshipListItems.Items[lastRemovedIndex - 1].Selected = true;
                }
                else
                {
                    WorshipListItems.Items[WorshipListItems.Items.Count - 1].Selected = true;
                }
            }
            ShowStatusBarSummary();
            WorshipListIndexChanged();
            UpdateOutputCurItemNo();
            if (removedOutputItem)
            {
                CopyPreviewToOutput();
            }
            return true;
        }

        private void SetDocLayout(PraiseBookLayout InLayout)
        {
            Gf.PB_FormatChanged = false;
            Gf.PB_Layout = InLayout;
            FrmGenerateDoc frmGenerateDoc = new FrmGenerateDoc();
            if (InLayout == PraiseBookLayout.WorshipList)
            {
                LoadWorshipList(1);
                Gf.PB_FullFileName = Gf.RootEasiSlidesDir + "Documents\\" + Gf.CurSession + ".rtf";
                Gf.PB_DocumentName = Gf.CurSession;
                if (PrepareWorshipListLyricsForRTF())
                {
                    frmGenerateDoc.ShowDialog();
                    if (Gf.PB_FormatChanged)
                    {
                        SaveWorshipList();
                    }
                }
                return;
            }
            LoadPraiseBook(1);
            Gf.PB_FullFileName = Gf.RootEasiSlidesDir + "Documents\\" + Gf.CurPraiseBook + ".rtf";
            Gf.PB_DocumentName = Gf.CurPraiseBook;
            if (PreparePraiseBookLyricsForRTF())
            {
                frmGenerateDoc.ShowDialog();
                if (Gf.PB_FormatChanged)
                {
                    SavePraiseBook();
                }
            }
        }

        private bool PrepareWorshipListLyricsForRTF()
        {
            if (WorshipListItems.Items.Count > 0)
            {
                int invalidIndex = ValidateWorshipListItems(ShowErrorMessage: true);
                if (invalidIndex >= 0)
                {
                    return false;
                }
                for (int i = 0; i <= WorshipListItems.Items.Count - 1; i++)
                {
                    Gf.DocumentSongs[i + 1].IdString = Gf.WorshipSongs[i + 1].IdString;
                    Gf.DocumentSongs[i + 1].Type = Gf.WorshipSongs[i + 1].Type;
                    Gf.DocumentSongs[i + 1].Title = Gf.RemoveMusicSym(DataUtil.Trim(Gf.WorshipSongs[i + 1].Title));
                    Gf.DocumentSongs[i + 1].IndexGroup = Gf.WorshipSongs[i + 1].Flag;
                    Gf.DocumentSongs[i + 1].NumberAndInfo = '>' + Gf.ExtractHeaderInfo(Gf.WorshipSongs[i + 1].FormatString, 71, '>');
                }
                Gf.TotalPraiseBookItems = WorshipListItems.Items.Count;
                return true;
            }
            MessageBox.Show("Can't produce lyrics document because the Worship List is empty!");
            return false;
        }

        private bool PreparePraiseBookLyricsForRTF()
        {
            if (PraiseBookItems.Items.Count > 0)
            {
                ValidatePraiseBookItems();
                return true;
            }
            MessageBox.Show("Can't produce lyrics document because the PraiseBook is empty!");
            return false;
        }

        private void RemovePraiseBookListSong()
        {
            int lastRemovedIndex = -1;
            if (PraiseBookItems.Items.Count == 0)
            {
                return;
            }
            for (int i = PraiseBookItems.Items.Count - 1; i >= 0; i--)
            {
                if (PraiseBookItems.Items[i].Selected)
                {
                    PraiseBookItems.Items[i].Remove();
                    lastRemovedIndex = i;
                }
            }
            SavePraiseBook();
            if (PraiseBookItems.Items.Count > lastRemovedIndex)
            {
                PraiseBookItems.Items[lastRemovedIndex].Selected = true;
            }
            else if (PraiseBookItems.Items.Count > 0)
            {
                if (lastRemovedIndex - 1 >= 0 && lastRemovedIndex <= PraiseBookItems.Items.Count)
                {
                    PraiseBookItems.Items[lastRemovedIndex - 1].Selected = true;
                }
                else
                {
                    PraiseBookItems.Items[PraiseBookItems.Items.Count - 1].Selected = true;
                }
            }
            PraiseBookListIndexChanged();
            if (Gf.UseSongNumbers)
            {
                for (int i = 0; i <= PraiseBookItems.Items.Count - 1; i++)
                {
                    PraiseBookItems.Items[i].SubItems[1].Text = PraiseBookItems.Items[i].SubItems[5].Text;
                }
            }
            else
            {
                for (int i = 0; i <= PraiseBookItems.Items.Count - 1; i++)
                {
                    PraiseBookItems.Items[i].SubItems[1].Text = Convert.ToString(i + 1);
                }
            }
            ShowStatusBarSummary();
        }

        private void TimerFlasher_Tick(object sender, EventArgs e)
        {
            Gf.TimerFlashOn = !Gf.TimerFlashOn;
            if (cbGoLive.Checked)
            {
                cbGoLive.ForeColor = (Gf.TimerFlashOn ? Gf.ButtonPressedForeColour : Gf.ButtonDefaultForeColour);
            }
            if (cbOutputBlack.Checked)
            {
                cbOutputBlack.ImageIndex = (Gf.TimerFlashOn ? 16 : 15);
            }
            if (cbOutputClear.Checked)
            {
                cbOutputClear.ImageIndex = (Gf.TimerFlashOn ? 18 : 17);
            }
            //if (cbOutputCam.Checked)
            //{
            //    cbOutputCam.ImageIndex = (Gf.TimerFlashOn ? 31 : 30);
            //}
            if (Gf.ShowRunning)
            {
                if (Gf.DualMonitorMode && Gf.LaunchShowUpdateDone)
                {
                    Focus();
                    Gf.LaunchShowUpdateDone = false;
                }
                if (Gf.OutputItem.CurItemNo > 0)
                {
                    try
                    {
                        if (WorshipListItems.Items[Gf.StartPresAt - 1].ImageIndex == 0 || WorshipListItems.Items[Gf.StartPresAt - 1].ImageIndex == 2 || WorshipListItems.Items[Gf.StartPresAt - 1].ImageIndex == 4 || WorshipListItems.Items[Gf.StartPresAt - 1].ImageIndex == 6 || WorshipListItems.Items[Gf.StartPresAt - 1].ImageIndex == 8 || WorshipListItems.Items[Gf.StartPresAt - 1].ImageIndex == 28 || WorshipListItems.Items[Gf.StartPresAt - 1].ImageIndex == 10)
                        {
                            WorshipListItems.Items[Gf.StartPresAt - 1].ImageIndex = WorshipListItems.Items[Gf.StartPresAt - 1].ImageIndex + (Gf.TimerFlashOn ? 1 : 0);
                        }
                        else
                        {
                            WorshipListItems.Items[Gf.StartPresAt - 1].ImageIndex = WorshipListItems.Items[Gf.StartPresAt - 1].ImageIndex - 1 + (Gf.TimerFlashOn ? 1 : 0);
                        }
                    }
                    catch
                    {
                    }
                }
                SetStatusBarMediaTimings();
            }
        }

        public void Remote_SongChanged()
        {
            if (Gf.DualMonitorMode)
            {
                LoadWorshipListItemToOutput(Gf.LiveItem.CurItemNo, Gf.LiveItem.CurSlide, NotifyLiveShow: false);
                FocusOutputArea();
                Focus();
            }
        }

        public void Remote_SlideChanged()
        {
            if (Gf.DualMonitorMode)
            {
                Gf.OutputItem.CurSlide = Gf.LiveItem.CurSlide;
                MoveToSlideOutputItem(Gf.OutputItem, KeyDirection.Refresh, NotifyLiveShow: false);
                FocusOutputArea();
                Focus();
            }
        }

        public void Remote_MovedToGapItem()
        {
            if (Gf.DualMonitorMode)
            {
                LoadItem(ref Gf.OutputItem, "G1", "", 0);
                FocusOutputArea();
                Focus();
            }
        }

        public void Remote_EndShow()
        {
            if (Gf.DualMonitorMode)
            {
                GoLive(InStatus: false);
            }
        }

        /// <summary>
        /// Ends a show whose start failed in the dual-monitor branch of Start_Presentation (called from
        /// GoLive(true)). Gf.ShowRunning is still true here, so GoLive(false) takes the Stop button's
        /// path: ShowRunning = false, LiveShow.StopShow() (FormClosingCleanup: screen saver restored,
        /// the presentations PreLoadPowerpointFiles opened closed, task bar shown; the window hidden),
        /// UpdateWorshipShowIcons(). The initialisation may have stopped anywhere, even after a slide
        /// show or media had started on the projector, so all of that is needed. Each step is guarded:
        /// whatever throws, this form ends in "show off" with ShowRunning false. Setting cbGoLive.Checked
        /// raises no event (its only handler is Click), so the nested GoLive(false) re-enters nothing.
        /// </summary>
        private void EndFailedShowStart()
        {
            try
            {
                GoLive(InStatus: false);
            }
            catch (Exception ex)
            {
                // StopShow stopped somewhere in its clean-up: repeat the clean-up steps that are safe to
                // repeat, each on its own.
                WriteDebugLogThrottled("Start_Presentation stop after failed start", ex);
                Gf.ShowRunning = false;
                TryStep("RestoreScreenSaverSettings", Gf.RestoreScreenSaverSettings);
                TryStep("ClearUpPowerpointWindows", Gf.ClearUpPowerpointWindows);
                TryStep("ShowTaskBar", Gf.ShowTaskBar);
                TryStep("Hide show window", () =>
                {
                    if (LiveShow != null && !LiveShow.IsDisposed && LiveShow.Visible)
                    {
                        LiveShow.Hide();
                    }
                });
            }
            // Button, menu, status row and worship list marks to "show off" (a no-op if GoLive(false)
            // above completed).
            TryStep("Remote_ShowStopped", Remote_ShowStopped);
            // Load runs only once per window: a window whose Load never completed lacks what only Load
            // does, so it is replaced by a new one at the next start (EnsureLiveShowWindow). A window
            // whose later initialisation (InitForm from VisibleChanged) failed is kept: the next start
            // shows it again and its VisibleChanged runs InitForm again.
            TryStep("Dispose show window", () =>
            {
                if (LiveShow != null && !LiveShow.IsDisposed && !LiveShow.FirstLoadCompleted)
                {
                    LiveShow.Dispose();
                    // LiveShow is never left a disposed instance: callers not gated on ShowRunning
                    // (QueryShowActive) would act on it and fail.
                    EnsureLiveShowWindow();
                }
            });
        }

        /// <summary>
        /// The main form closes while the show runs (any close reason: the operator, Task Manager's End task,
        /// or Windows ending the session, then from FormClosed; see Form1_FormClosing). The application then
        /// exits without closing the show window, so its FormClosing clean-up never ran: screen saver setting
        /// and task bar not restored, the show's slide shows not taken off the top. End the show first on the
        /// Stop button's path (GoLive(false) -> LiveShow.StopShow -> FormClosingCleanup), the same work and the
        /// same PowerPoint calls as when the operator stops the show and then closes the app (ShutdownPowerpoint
        /// afterwards finds nothing left to close). Never throws: a failure (e.g. the show window was already
        /// closed and disposed by the same shutdown) is logged, the steps that are safe to repeat are run one by
        /// one, and the closing goes on.
        /// </summary>
        private void EndShowBeforeClosing(CloseReason reason)
        {
            if (!Gf.ShowRunning)
            {
                return;
            }
            WriteDebugLog("Main form closing (" + reason + ") with the show running: ending the show first");
            try
            {
                GoLive(InStatus: false);
            }
            catch (Exception ex)
            {
                WriteDebugLogThrottled("Form closing: end show", ex);
                Gf.ShowRunning = false;
                TryStep("RestoreScreenSaverSettings", Gf.RestoreScreenSaverSettings);
                TryStep("ClearUpPowerpointWindows", Gf.ClearUpPowerpointWindows);
                TryStep("ShowTaskBar", Gf.ShowTaskBar);
                TryStep("Hide show window", () =>
                {
                    if (LiveShow != null && !LiveShow.IsDisposed && LiveShow.Visible)
                    {
                        LiveShow.Hide();
                    }
                });
            }
        }

        private static void TryStep(string what, Action step)
        {
            try
            {
                step();
            }
            catch (Exception ex)
            {
                WriteDebugLogThrottled("Start_Presentation clean-up: " + what, ex);
            }
        }

        /// <summary>
        /// The show window has ended a non-modal show itself (FrmLaunchShow.StopShowAndTellMainForm),
        /// or a failed show start has been cleaned up (EndFailedShowStart): bring this form to
        /// "show off". The show is already stopped (Gf.ShowRunning is false), so GoLive(false) only
        /// resets the show button, menu and status row and does not stop it again; the worship list
        /// marks are refreshed here because GoLive does that only when it stops the show.
        /// </summary>
        public void Remote_ShowStopped()
        {
            Gf.ShowRunning = false;
            GoLive(InStatus: false);
            UpdateWorshipShowIcons();
        }

        private void ItemKeyPressed(SongSettings InItem, Keys KeyCode, bool ShiftKey)
        {
            // Ignored in PraiseBook mode and for the Tab key
            if (Gf.EasiSlidesMode == UsageMode.PraiseBook || KeyCode == Keys.Tab)
            {
                return;
            }

            // Keyboard remapping
            Gf.ReMapKeyBoard(ref KeyCode);

            // Navigation keys
            if (KeyboardMapping.IsNavigationKey(KeyCode))
            {
                _keyboardHandler.HandleNavigationKey(InItem, KeyCode);
                return;
            }

            // Song verse jump keys
            if (KeyboardMapping.IsVerseJumpKey(KeyCode, ShiftKey))
            {
                _keyboardHandler.HandleVerseJumpKey(InItem, KeyCode, ShiftKey);
                return;
            }

            // Numeric keys (including C)
            if (KeyboardMapping.IsNumericKey(KeyCode) || KeyCode == Keys.C)
            {
                _keyboardHandler.HandleNumericKey(InItem, KeyCode);
                return;
            }

            // Function keys
            if (KeyboardMapping.IsFunctionKey(KeyCode))
            {
                _keyboardHandler.HandleFunctionKey(InItem, KeyCode);
            }
        }

        internal void JumpToVerseType(SongSettings InItem, int InOtherVerse)
        {
            int slideNo = 1;
            while (true)
            {
                if (slideNo <= InItem.TotalSlides)
                {
                    if (InItem.Slide[slideNo, 0] == InOtherVerse)
                    {
                        break;
                    }
                    slideNo++;
                    continue;
                }
                return;
            }
            InItem.CurSlide = slideNo;
            MoveToSlide(InItem, KeyDirection.Refresh);
        }

        internal void ManualMoveToItem(SongSettings InItem, KeyDirection InDirection)
        {
            MoveToItem(InItem, InDirection, 0, NotifyLiveShow: true);
        }

        private void ManualMoveToItem(SongSettings InItem, int GotoItemNumber)
        {
            if (GotoItemNumber <= WorshipListItems.Items.Count)
            {
                InItem.CurItemNo = GotoItemNumber;
                Gf.StartPresAt = GotoItemNumber;
                MoveToItem(InItem, KeyDirection.Refresh, 0, NotifyLiveShow: true);
            }
        }

        private void MoveToItem(SongSettings InItem, KeyDirection InDirection, int SlideNo, bool NotifyLiveShow)
        {
            if (InItem.OutputStyleScreen)
            {
                MoveToOutputItem(InItem, InDirection, SlideNo, NotifyLiveShow);
            }
            else if (!InItem.OutputStyleScreen)
            {
                MoveToPreviewItem(InItem, InDirection);
            }
        }

        private void MoveToOutputItem(SongSettings InItem, KeyDirection InDirection, int SlideNo, bool NotifyLiveShow)
        {
            if (InItem.Type == "G" && Gf.TotalWorshipListItems == 0)
            {
                return;
            }
            Gf.Launch_StartPresAt = Gf.StartPresAt;
            LoadThumbOutlockkey = 0;
            previousOutSelectedSlide = 1;

            switch (InDirection)
            {
                case KeyDirection.FirstOne:
                    LoadWorshipListItemToOutput(1, SlideNo, NotifyLiveShow: false);
                    break;
                case KeyDirection.PrevOne:
                    LoadWorshipListItemToOutput((InItem.CurItemNo > 0 && InItem.OutputStyleScreen) ? (Gf.StartPresAt - ((InItem.Type != "G") ? 1 : 0)) : Gf.StartPresAt, SlideNo, NotifyLiveShow: false);
                    break;
                case KeyDirection.NextOne:
                    if (Gf.GapItemOption == GapType.None)
                    {
                        LoadWorshipListItemToOutput(Gf.StartPresAt + 1, SlideNo, NotifyLiveShow: false);
                    }
                    else if (Gf.StartPresAt == 0 || (InItem.Type == "G" && Gf.Launch_StartPresAt != Gf.TotalWorshipListItems))
                    {
                        LoadWorshipListItemToOutput(Gf.StartPresAt + 1, SlideNo, NotifyLiveShow: false);
                    }
                    else
                    {
                        LoadItem(ref InItem, "G1", "", 0);
                    }
                    break;
                case KeyDirection.LastOne:
                    LoadWorshipListItemToOutput(WorshipListItems.Items.Count, SlideNo, NotifyLiveShow: false);
                    break;
                default:
                    LoadWorshipListItemToOutput(InItem.CurItemNo, SlideNo, NotifyLiveShow: false);
                    break;
            }
            if (Gf.ShowRunning && NotifyLiveShow)
            {
                Gf.MainAction_SongChanged_Transaction = ImageTransitionControl.TransitionAction.AsStored;
                Gf.MainAction_MoveToItemKeyDirection = InDirection;
                RemoteControlLiveShow(LiveShowAction.Remote_MoveToItemChanged);
            }
            FocusOutputArea();
        }

        private void MoveToPreviewItem(SongSettings InItem, KeyDirection InDirection)
        {
            if (WorshipListItems.Items.Count < 1)
            {
                return;
            }
            int selectedIndex = Gf.GetSelectedIndex(WorshipListItems);
            WorshipListItems.SelectedItems.Clear();
            switch (InDirection)
            {
                case KeyDirection.FirstOne:
                    WorshipListItems.Items[0].Selected = true;
                    WorshipListItems.EnsureVisible(0);
                    break;
                case KeyDirection.PrevOne:
                    if (selectedIndex > 0)
                    {
                        WorshipListItems.Items[selectedIndex - 1].Selected = true;
                        WorshipListItems.EnsureVisible(selectedIndex - 1);
                    }
                    else
                    {
                        WorshipListItems.Items[0].Selected = true;
                        WorshipListItems.EnsureVisible(0);
                    }
                    break;
                case KeyDirection.NextOne:
                    if (selectedIndex <= WorshipListItems.Items.Count - 2)
                    {
                        WorshipListItems.Items[selectedIndex + 1].Selected = true;
                        WorshipListItems.EnsureVisible(selectedIndex + 1);
                    }
                    else
                    {
                        WorshipListItems.Items[WorshipListItems.Items.Count - 1].Selected = true;
                        WorshipListItems.EnsureVisible(WorshipListItems.Items.Count - 1);
                    }
                    break;
                case KeyDirection.LastOne:
                    WorshipListItems.Items[WorshipListItems.Items.Count - 1].Selected = true;
                    WorshipListItems.EnsureVisible(WorshipListItems.Items.Count - 1);
                    break;
            }
            WorshipListIndexChanged();
            FocusPreviewArea();
        }

        internal void MoveToSlide(SongSettings InItem, KeyDirection InDirection)
        {
            if (InItem.OutputStyleScreen)
            {
                if (InItem.Type == "P" && InItem.TotalSlides <= 0)
                {
                    // A PowerPoint item with no slides on the main form: when the show window runs it, take that
                    // run now (an earlier try may have met a busy PowerPoint); otherwise "next" would do nothing.
                    ReconcileOutputWithLivePowerPoint();
                }
                if (InDirection != KeyDirection.NextOne || InItem.CurSlide < InItem.TotalSlides || Gf.AdvanceNextItem)
                {
                    MoveToSlideOutputItem(InItem, InDirection, NotifyLiveShow: true);
                    SetOutputPPThumbImages1(Gf.OutputItem.CurSlide);
                }
            }
            else
            {
                MoveToSlidePreviewItem(InItem, InDirection);
                SetPreviewPPThumbImages1(Gf.PreviewItem.CurSlide);
            }
        }

        private void MoveToSlideOutputItem(SongSettings InItem, KeyDirection InDirection, bool NotifyLiveShow)
        {
            if (Gf.AdvanceNextItem)
            {
                if (InDirection == KeyDirection.PrevOne)
                {
                    int lastSlideNo = (InItem.TotalSlides > 0) ? InItem.TotalSlides : 1;
                    if (InItem.Type == "G")
                    {
                        MoveToItem(InItem, KeyDirection.Refresh, lastSlideNo, NotifyLiveShow);
                        return;
                    }
                    if (InItem.CurItemNo > 1 && InItem.CurSlide < 2)
                    {
                        MoveToItem(InItem, KeyDirection.PrevOne, lastSlideNo, NotifyLiveShow);
                        return;
                    }
                }
                else if (InDirection == KeyDirection.NextOne && InItem.CurItemNo < WorshipListItems.Items.Count && InItem.CurSlide >= InItem.TotalSlides)
                {
                    if (Gf.ShowRunning && InItem.Type == "P")
                    {
                        int pptResult = MainPptFor(InItem).ImplementPowerpointSlideMovement(ref InItem.CurSlide, InItem.TotalSlides, (OfficeLibKeys)Gf.ReMapKeyDirectionToPowerpoint(InDirection));
                        // The end of the item, so the next item: PowerPoint reached the end of the show (-1, also
                        // when the show ended on this move), or there is no slide show to move (not started, or
                        // ended earlier). A navigation error or a busy PowerPoint is not an end: the item stays.
                        bool itemEnded = pptResult <= 0 || (MainPPT.LastCallNotRunning && MainPPT.LastNavigationError == null);
                        if (!itemEnded)
                        {
                            Gf.ReportPowerpointNavigation(MainPPT, InItem);
                            RemoteControlLiveShow(LiveShowAction.Remote_SlideChanged, InDirection);
                            ShowDualMonitorPP_Preview(ref InItem);
                            if (InItem.CurSlide < 1)
                            {
                                InItem.CurSlide = InItem.TotalSlides;
                            }
                            ShowStatusBarSummary();
                            return;
                        }
                    }
                    MoveToItem(InItem, KeyDirection.NextOne, 0, NotifyLiveShow);
                    return;
                }
            }
            if (Gf.ShowRunning && InItem.Type == "P")
            {
                MainPptFor(InItem).ImplementPowerpointSlideMovement(ref InItem.CurSlide, InItem.TotalSlides, (OfficeLibKeys)Gf.ReMapKeyDirectionToPowerpoint(InDirection));
                Gf.ReportPowerpointNavigation(MainPPT, InItem);
                if (InItem.CurSlide >= 0 && InItem.CurSlide <= InItem.TotalSlides)
                {
                    RemoteControlLiveShow(LiveShowAction.Remote_SlideChanged, InDirection);
                    ShowDualMonitorPP_Preview(ref InItem);
                }
                else if (InItem.CurSlide < 1)
                {
                    InItem.CurSlide = InItem.TotalSlides;
                }
            }
            else
            {
                switch (InDirection)
                {
                    case KeyDirection.FirstOne:
                        InItem.CurSlide = 1;
                        break;
                    case KeyDirection.PrevOne:
                        if (InItem.CurSlide > 2)
                        {
                            InItem.CurSlide--;
                        }
                        else
                        {
                            InItem.CurSlide = 1;
                        }
                        break;
                    case KeyDirection.NextOne:
                        if (InItem.CurSlide < InItem.TotalSlides)
                        {
                            InItem.CurSlide++;
                            break;
                        }
                        if (Gf.GapItemOption == GapType.None)
                        {
                            InItem.CurSlide = InItem.TotalSlides;
                            break;
                        }
                        MoveToItem(InItem, KeyDirection.NextOne, 0, NotifyLiveShow);
                        return;
                    case KeyDirection.LastOne:
                        InItem.CurSlide = InItem.TotalSlides;
                        break;
                }
                ImplementSlideMove(InItem, ScrollToTop: true);
                if (Gf.ShowRunning && NotifyLiveShow)
                {
                    RemoteControlLiveShow(LiveShowAction.Remote_SlideChanged);
                }
            }
            ShowStatusBarSummary();
        }

        private void MoveToSlidePreviewItem(SongSettings InItem, KeyDirection InDirection)
        {
            switch (InDirection)
            {
                case KeyDirection.FirstOne:
                    InItem.CurSlide = 1;
                    break;
                case KeyDirection.PrevOne:
                    if (InItem.CurSlide > 2)
                    {
                        InItem.CurSlide--;
                    }
                    else
                    {
                        InItem.CurSlide = 1;
                    }
                    break;
                case KeyDirection.NextOne:
                    if (InItem.CurSlide < InItem.TotalSlides)
                    {
                        InItem.CurSlide++;
                    }
                    else
                    {
                        InItem.CurSlide = InItem.TotalSlides;
                    }
                    break;
                case KeyDirection.LastOne:
                    InItem.CurSlide = InItem.TotalSlides;
                    break;
            }
            ImplementSlideMove(InItem, ScrollToTop: true);
        }

        internal void QueryShowActive()
        {
            if (OutputScreen.RefStatus())
            {
                RemoteControlLiveShow(LiveShowAction.Remote_ReferenceAlertHide);
                OutputScreen.StopRef();
            }
            else
            {
                RemoteControlLiveShow(LiveShowAction.Remote_ReferenceAlertShow);
                ShowSlide(ref Gf.OutputItem, ImageTransitionControl.TransitionAction.None, DoActiveIndicator: true);
            }
        }

        /// <summary>
        /// Triggers the next PowerPoint animation or video on the given slide.
        /// Called from KeyboardActionHandler.
        /// </summary>
        internal void PlayPowerPointAnimation(int slideNumber)
        {
            if (MainPPT?.prePowerPointApp != null)
            {
                MainPptFor(Gf.OutputItem).SafePlayNext(slideNumber);
                Gf.ReportPowerpointShowEnded(MainPPT, Gf.OutputItem);
            }
        }

        /// <summary>
        /// The main form's PowerPoint wrapper, set to drive the live item's slide show. In a dual monitor show
        /// the main form and the show window both run each PowerPoint item; at show start only the show window
        /// runs the item that is already live, so the main form follows the show window's run of it then (see
        /// PowerPoint.FollowRunOf). It never falls back to PowerPoint's active presentation.
        /// </summary>
        private PowerPoint MainPptFor(SongSettings InItem)
        {
            // A slide show the show window believes ended is asked about again first (one lookup), so an
            // earlier "ended" verdict does not stop the main form from following it.
            Gf.ReprobeEndedLivePowerPoint();
            MainPPT.FollowRunOf(Gf.LivePP, InItem.Path, Gf.PowerpointList);
            return MainPPT;
        }

        /// <summary>
        /// The live PowerPoint item's slide show was found gone: the show window stops being transparent over it.
        /// </summary>
        internal void LivePowerPointEnded()
        {
            LiveShow?.PaintEndedPowerPointItem();
        }

        /// <summary>
        /// The live PowerPoint item's slide show, believed ended, was found running: the show window becomes
        /// transparent over it again.
        /// </summary>
        internal void LivePowerPointResumed()
        {
            LiveShow?.ResumeStartedPowerPointItem();
        }

        // The output item path whose slide show the main form's own run (or an adopted run) drives, or null.
        private string mainPptRunItemPath;

        // Counts the main form's loads of a PowerPoint output item; with Gf.LiveLoadSerial and the show window's
        // run state it names the situation an adopt was last tried for (tried once per situation).
        private int outputPowerpointLoadSerial;
        private string adoptTriedFor;

        // Gf.LiveLoadSerial when the main form last looked at the show window's run state.
        private int liveLoadSerialSeen;

        /// <summary>
        /// Dual monitor show: makes the main form's output side agree with what the show window's run put on the
        /// projector (the truth). Called after the main form's run of a PowerPoint item (LoadItem) and after the
        /// show window has processed an item or slide change (RemoteControlLiveShow), so both orders of the two
        /// runs are covered. Does nothing (and asks PowerPoint nothing) when both runs agree.
        /// (a) The main form's run did not start, the show window's did: the main form takes the show window's
        ///     run for slide count, verse data and the output pane.
        /// (b) The show window's run did not start, the main form's did: the output pane shows the default
        ///     background and the main form stops driving a slide show that is not on the projector.
        /// </summary>
        private void ReconcileOutputWithLivePowerPoint()
        {
            if (!Gf.ShowRunning || !Gf.DualMonitorMode || Gf.OutputItem.Type != "P" || Gf.LiveItem.Type != "P"
                || !string.Equals(Gf.OutputItem.Path, Gf.LiveItem.Path, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }
            bool mainRuns = string.Equals(mainPptRunItemPath, Gf.OutputItem.Path, StringComparison.OrdinalIgnoreCase);
            if (Gf.LivePowerPointRunState == Gf.LivePowerPointRun.Started && !mainRuns)
            {
                // Tried once per item load and show-window state, not on every key press; again on a later
                // slide change or key only while the item still has no slides (a busy PowerPoint may have
                // refused the first try, and with 0 slides "next" does nothing). Once taken, mainRuns is true.
                string situation = outputPowerpointLoadSerial + "|" + Gf.LiveLoadSerial + "|" + Gf.LivePowerPointRunState;
                if (situation == adoptTriedFor && Gf.OutputItem.TotalSlides > 0)
                {
                    return;
                }
                adoptTriedFor = situation;
                WriteDebugLog("ReconcileOutputWithLivePowerPoint: adopting the show window's run of '" + Gf.OutputItem.Path + "'");
                int count = Gf.AdoptLivePowerpointRun(ref Gf.OutputItem, MainPPT);
                if (count <= 0)
                {
                    // Not taken: the item keeps its slide count and verse data.
                    WriteDebugLog("ReconcileOutputWithLivePowerPoint: the show window's run could not be taken");
                    return;
                }
                Gf.OutputItem.TotalSlides = count;
                mainPptRunItemPath = Gf.OutputItem.Path;
                Gf.OutputItem.CurSlide = Math.Max(1, Math.Min(Gf.LiveItem.CurSlide, Gf.OutputItem.TotalSlides));
                ShowDualMonitorPP_Preview(ref Gf.OutputItem);
                BuildVerseButtons(Gf.OutputItem);
                ShowStatusBarSummary();
                WriteDebugLog("ReconcileOutputWithLivePowerPoint: the main form follows the show window's run of '" + Gf.OutputItem.Path + "'");
            }
            else if (Gf.LivePowerPointRunState == Gf.LivePowerPointRun.NotStarted && mainRuns)
            {
                Gf.DropMainPowerpointRun(ref Gf.OutputItem, MainPPT);
                mainPptRunItemPath = null;
                Gf.ResetPictureBox(ref Gf.OutputItem, ref OutputScreen, GapType.Default, ImageTransitionControl.TransitionAction.None);
                BuildVerseButtons(Gf.OutputItem);
                ShowStatusBarSummary();
                WriteDebugLog("ReconcileOutputWithLivePowerPoint: the show window's run of '" + Gf.OutputItem.Path + "' did not start; the main form no longer drives its own run");
            }
        }

        private bool ImplementSlideMove(SongSettings InItem, bool ScrollToTop)
        {
            InItem.Show_LicAdim = false;
            if (InItem.OutputStyleScreen)
            {
                HighlightOutputRichTextBox(OnEnter: true, ScrollToTop);
            }
            else
            {
                HighlightPreviewRichTextBox(OnEnter: true, ScrollToTop);
            }
            return ShowSlide(ref InItem, ImageTransitionControl.TransitionAction.AsStored);
        }

        public bool LoadWorshipListItemToOutput(int Selecteditem, int SlideNo)
        {
            return LoadWorshipListItemToOutput(Selecteditem, SlideNo, NotifyLiveShow: true);
        }

        public bool LoadWorshipListItemToOutput(int Selecteditem, int SlideNo, bool NotifyLiveShow)
        {
            if (WorshipListItems.Items.Count == 0)
            {
                return false;
            }
            if (Selecteditem < 1)
            {
                Selecteditem = 1;
            }
            else if (Selecteditem > WorshipListItems.Items.Count)
            {
                Selecteditem = WorshipListItems.Items.Count;
            }
            Selecteditem--;
            string idString = WorshipListItems.Items[Selecteditem].SubItems[1].Text;
            string formatData = WorshipListItems.Items[Selecteditem].SubItems[2].Text;
            string InTitle = WorshipListItems.Items[Selecteditem].SubItems[0].Text;
            Gf.OutputItem.CurItemNo = Selecteditem + 1;
            Gf.StartPresAt = Gf.OutputItem.CurItemNo;
            Gf.OutputItem.Source = ItemSource.WorshipList;
            Gf.OutputItem.OutputStyleScreen = true;
            LoadItem(ref Gf.OutputItem, idString, formatData, SlideNo, ref InTitle, ScrollToCaret: true);
            if (Gf.ShowRunning && NotifyLiveShow)
            {
                Gf.MainAction_SongChanged_Transaction = ImageTransitionControl.TransitionAction.AsStored;
                RemoteControlLiveShow(LiveShowAction.Remote_SongChanged);
            }
            UpdateWorshipShowIcons();
            return true;
        }

        private void PreviewBtnUpDown_Click(object sender, EventArgs e)
        {
            Button button = (Button)sender;
            string name = button.Name;
            if (name == "PreviewBtnItemUp")
            {
                ManualMoveToItem(Gf.PreviewItem, KeyDirection.PrevOne);
            }
            else if (name == "PreviewBtnItemDown")
            {
                ManualMoveToItem(Gf.PreviewItem, KeyDirection.NextOne);
            }
            else if (name == "PreviewBtnSlideUp")
            {
                MoveToSlide(Gf.PreviewItem, KeyDirection.PrevOne);
            }
            else if (name == "PreviewBtnSlideDown")
            {
                MoveToSlide(Gf.PreviewItem, KeyDirection.NextOne);
            }
            FocusPreviewArea();
        }

        private void OutputBtnJumpToNonRotate_Click(object sender, EventArgs e)
        {
            GotoNextNonRotateItem(Gf.OutputItem);
            FocusOutputArea();
        }

        private void OutputBtnMedia_Click(object sender, EventArgs e)
        {
            if (Gf.OutputItem.OutputStyleScreen)
            {
                RemoteControlLiveShow(LiveShowAction.Remote_MediaItemPausePlay);
            }
            FocusOutputArea();
        }

        private void OutputBtnRefAlert_Click(object sender, EventArgs e)
        {
            QueryShowActive();
            FocusOutputArea();
        }

        private void OutputBtnUpDown_Click(object sender, EventArgs e)
        {
            Button button = (Button)sender;
            string name = button.Name;
            if (name == "OutputBtnItemUp")
            {
                ManualMoveToItem(Gf.OutputItem, KeyDirection.PrevOne);
            }
            else if (name == "OutputBtnItemDown")
            {
                ManualMoveToItem(Gf.OutputItem, KeyDirection.NextOne);
            }
            else if (name == "OutputBtnSlideUp")
            {
                MoveToSlide(Gf.OutputItem, KeyDirection.PrevOne);
            }
            else if (name == "OutputBtnSlideDown")
            {
                MoveToSlide(Gf.OutputItem, KeyDirection.NextOne);
            }
            FocusOutputArea();
        }

        private void PreviewBtnVerse_Click(object sender, EventArgs e)
        {
            Button button = (Button)sender;
            int verseCode = DataUtil.ObjToInt(button.Tag);
            if (verseCode < 10)
            {
                Gf.PreviewItem.CurSlide = Gf.PreviewItem.SongVerses[verseCode];
            }
            else
            {
                for (int i = 1; i <= Gf.PreviewItem.TotalSlides; i++)
                {
                    if (Gf.PreviewItem.Slide[i, 0] == verseCode)
                    {
                        Gf.PreviewItem.CurSlide = i;
                        i = Gf.PreviewItem.TotalSlides;
                    }
                }
            }
            MoveToSlide(Gf.PreviewItem, KeyDirection.Refresh);
            FocusPreviewArea();
        }

        private void OutputBtnVerse_Click(object sender, EventArgs e)
        {
            Button button = (Button)sender;
            int verseCode = DataUtil.ObjToInt(button.Tag);
            if (verseCode < 10)
            {
                Gf.OutputItem.CurSlide = Gf.OutputItem.SongVerses[verseCode];
            }
            else
            {
                for (int i = 1; i <= Gf.OutputItem.TotalSlides; i++)
                {
                    if (Gf.OutputItem.Slide[i, 0] == verseCode)
                    {
                        Gf.OutputItem.CurSlide = i;
                        i = Gf.OutputItem.TotalSlides;
                    }
                }
            }
            MoveToSlide(Gf.OutputItem, KeyDirection.Refresh);
            FocusOutputArea();
        }

        private void FormControl_Enter(object sender, EventArgs e)
        {
            Control inControl = (Control)sender;
            Gf.NormalTextRegionBackColour = SongFolder.BackColor;
            Gf.Control_Enter(inControl);
        }

        private void FormControl_Leave(object sender, EventArgs e)
        {
            Control inControl = (Control)sender;
            Gf.NormalTextRegionBackColour = SongFolder.BackColor;
            Gf.Control_Leave(inControl);
        }

        /// <summary>
        /// Lets the operator pick an external file (PowerPoint, Word, text, InfoScreen, worship list or
        /// media) and adds it to the worship list.
        /// </summary>
        private void LocateFileWorshipList()
        {
            int powerpointCount = 0;
            for (int i = 0; i <= WorshipListItems.Items.Count - 1; i++)
            {
                if (DataUtil.Left(WorshipListItems.Items[i].SubItems[1].Text, 1) == "P")
                {
                    powerpointCount++;
                }
            }
            string filter = "Valid External Files (*.ppt,*.pptx,*.doc,*.docx,*.txt,*.esi,*.esw)|*.ppt;*.pptx;*.doc;*.docx;*.txt;*.esi;*.esw|Powerpoint Files (*.ppt)|*.ppt|Powerpoint Files (*.pptx)|*.pptx|Word Documents (*.doc)|*.doc|Word Documents (*.docx)|*.docx|Text Files (*.txt)|*.txt|InfoScreens (*.esi)|*.esi|Worship Lists (*.esw)|*.esw";
            if (Gf.MediaFileExtensions.Count > 0)
            {
                string mediaLabel = "";
                string mediaPattern = "";
                for (int i = 0; i < Gf.MediaFileExtensions.Count; i++)
                {
                    string ext = Gf.MediaFileExtensions[i].Extension.ToLower();
                    if (i > 0)
                    {
                        mediaLabel += ",";
                        mediaPattern += ";";
                    }
                    mediaLabel += "*" + ext;
                    mediaPattern += "*" + ext;
                }
                filter += "|Media Files (" + mediaLabel + ")|" + mediaPattern;
            }
            openFileDialog1.Filter = filter;
            openFileDialog1.InitialDirectory = Gf.DocumentsDir;
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string fileName = openFileDialog1.FileName;
                AddExternalFileToWorshipList(fileName);
            }
        }

        private bool AddExternalFileToWorshipList(string FullPathFileName)
        {
            return AddExternalFileToWorshipList(FullPathFileName, GetWorshipListNextSelectedLoc());
        }

        /// <summary>
        /// Adds an external file (PowerPoint, Word, text, InfoScreen, media, or the items of a worship
        /// list file) to the worship list.
        /// </summary>
        /// <param name="FullPathFileName">Full path of the file to add.</param>
        /// <param name="AddToLocation">Worship list index to insert at (appended when out of range).</param>
        /// <returns>False only when the items of a worship list file could not be inserted.</returns>
        private bool AddExternalFileToWorshipList(string FullPathFileName, int AddToLocation)
        {
            ListViewItem listViewItem = new ListViewItem();
            string displayName = Gf.GetDisplayNameOnly(ref FullPathFileName, UpdateByRef: false);
            string musicTitle = displayName;
            string musicTitle2 = "";
            string typeSymbol = "";
            int imageIndex = 0;
            string extension = Path.GetExtension(FullPathFileName).ToLower();//daniel DataUtil.Right(FullPathFileName, 4);
            bool preloadPowerpoint = false;
            if (AddToLocation < 0 || AddToLocation > WorshipListItems.Items.Count)
            {
                AddToLocation = WorshipListItems.Items.Count;
            }
            if (extension == ".ppt" || extension == ".pptx")
            {
                typeSymbol = "P";
                imageIndex = 2;
                if (Gf.ShowRunning)
                {
                    preloadPowerpoint = true;
                }
            }
            else if (extension == ".txt")
            {
                typeSymbol = "T";
                imageIndex = 6;
            }
            else if (extension == ".doc" || extension == ".docx")
            {
                typeSymbol = "W";
                imageIndex = 10;
            }
            else if (extension == ".esi")
            {
                typeSymbol = "I";
                imageIndex = 8;
                string InTitle = "";
                Gf.LoadIndividualData(ref Gf.TempItem1, typeSymbol + FullPathFileName, "", 1, ref InTitle);
                musicTitle = Gf.TempItem1.Title;
                musicTitle2 = Gf.TempItem1.Title2;
            }
            else if (ValidMediaExt(extension))
            {
                typeSymbol = "M";
                imageIndex = 28;
            }
            else if (extension == ".esw")
            {
                bool inserted = InsertIndexFileItems(FullPathFileName, ref WorshipListItems, AddToLocation, ref Gf.CurSessionNotes);
                if (inserted)
                {
                    SaveWorshipList(preloadPowerpoint);
                }
                return inserted;
            }
            if (typeSymbol != "")
            {
                if (Gf.MusicFound(musicTitle, musicTitle2))
                {
                    displayName += " <#>";
                }
                listViewItem = WorshipListItems.Items.Insert(AddToLocation, displayName);
                listViewItem.ImageIndex = imageIndex;
                listViewItem.SubItems.Add(typeSymbol + FullPathFileName);
                listViewItem.SubItems.Add("");
                listViewItem.SubItems.Add("");
                listViewItem.SubItems.Add("");
                listViewItem.SubItems.Add("");
                listViewItem.SubItems.Add("");
                listViewItem.SubItems.Add("");
                AddToLocation++;
            }
            SaveWorshipList(preloadPowerpoint);
            return true;
        }

        private bool ValidMediaExt(string InExt)
        {
            for (int i = 0; i < Gf.MediaFileExtensions.Count; i++)
            {
                if (InExt.ToLower() == Gf.MediaFileExtensions[i].Extension.ToLower())
                {
                    return true;
                }
            }
            return false;
        }

        //private void ShowPreviewPPThumbs()
        //{
        //	ShowPreviewPPThumbs(0);
        //}

        private void ShowPreviewPPThumbs(int GotoSlide)
        {
            //PreviewPPTotalImagesCount = Gf.PreviewItem.TotalSlides;
            //if (PreviewPPTotalImagesCount > 1024)
            //	PreviewPPTotalImagesCount = 1024;

            //for (int i = 0; i < PreviewPPTotalImagesCount; i++)
            //{
            //	PreviewPPImagename[i] = "";
            //	PreviewPPTIHoldersFileName[i] = "";
            //	PreviewPPSlideNumber[i] = -1;
            //	PreviewPPImagename[i] = Gf.PREPPFullPath + Convert.ToString(i + 1) + ".jpg";
            //}

            PreviewPPTotalImagesCount = Gf.PreviewItem.TotalSlides;

            //for (int i = 0; i < 1024; i++)
            for (int i = 0; i < PreviewPPTotalImagesCount; i++)
            {
                PreviewPPImagename[i] = "";
            }
            //for (int i = 0; i < 1024; i++)
            for (int i = 0; i < PreviewPPTotalImagesCount; i++)
            {
                PreviewPPTIHoldersFileName[i] = "";
                PreviewPPSlideNumber[i] = -1;
            }

            for (int i = 0; i <= ((PreviewPPTotalImagesCount < 1024) ? (PreviewPPTotalImagesCount - 1) : 1023); i++)
            {
                PreviewPPImagename[i] = Gf.PREPPFullPath + Convert.ToString(i + 1) + ".jpg";
            }
            SetPreviewPPThumbImages1(GotoSlide);
        }

        /// <summary>
        /// Shows the slide thumbnails of the preview PowerPoint item.
        /// </summary>
        /// <param name="GotoSlide">The slide to scroll to; below 0 means the last slide.</param>
        private void SetPreviewPPThumbImages1(int GotoSlide)
        {
            if (Gf.PreviewItem.Type == "P")
            {
                if (GotoSlide < 0)
                {
                    GotoSlide = Gf.PreviewItem.TotalSlides;
                }
                LoadThumbPreviewImages(flowLayoutPreviewPowerPoint, ref Powerpoint_PreviewCanvas, PreviewPPImagename, Gf.PreviewItem.TotalSlides, flowLayoutPreviewPowerPoint.Width, Gf.OUTPPFullPath, GotoSlide, toolTip1, ExternalPP: true);
            }
        }

        //private void SetPreviewPPThumbImages(int GotoSlide)
        //{
        //	if (Gf.PreviewItem.Type == "P")
        //	{
        //		if (GotoSlide < 0)
        //		{
        //			GotoSlide = Gf.PreviewItem.TotalSlides;
        //		}
        //		LoadThumbImages(flowLayoutPreviewPowerPoint, ref Powerpoint_PreviewCanvas, PreviewPPImagename, Gf.PreviewItem.TotalSlides, flowLayoutPreviewPowerPoint.Width, Gf.OUTPPFullPath, GotoSlide, toolTip1, ExternalPP: true);
        //	}
        //}

        private void MakePowerpointPreviewVisible(SongSettings InItem, bool MakeVisible)
        {
            if (InItem.OutputStyleScreen)
            {
                flowLayoutOutputPowerPoint.Visible = MakeVisible;
            }
            else
            {
                flowLayoutPreviewPowerPoint.Visible = MakeVisible;
            }
        }

        private void ShowDualMonitorPP_Preview(ref SongSettings InItem)
        {
            if (InItem.TotalSlides != 0)
            {
                try
                {
                    int slideNo = (InItem.CurSlide < 0) ? InItem.TotalSlides : InItem.CurSlide;
                    InItem.Format.BackgroundPicture = (InItem.OutputStyleScreen ? Gf.OUTPPFullPath : Gf.PREPPFullPath) + slideNo + ".jpg";
                    InItem.UseDefaultFormat = false;
                    InItem.Format.BackgroundMode = ImageMode.BestFit;
                    Gf.DrawText(ref InItem, ref PreviewScreen, ref OutputScreen, InItem.LyricsAndNotationsList);
                    if (InItem.OutputStyleScreen)
                    {
                        Gf.SetShowBackground(InItem, ref OutputScreen);
                    }
                    else
                    {
                        Gf.SetShowBackground(InItem, ref PreviewScreen);
                    }
                    Gf.DrawText(ref InItem, ref PreviewScreen, ref OutputScreen, InItem.LyricsAndNotationsList);
                }
                catch
                {
                }
            }
        }

        private void ShowOutputPPThumbs()
        {
            ShowOutputPPThumbs(0);
        }

        string previousOutputPPImageName = "";

        /// <summary>
        /// Shows the slide thumbnails of the output PowerPoint item; the image name list is rebuilt only when
        /// the item's first image name changed.
        /// </summary>
        /// <param name="GotoSlide">The slide to scroll to; below 0 means the last slide.</param>
        private void ShowOutputPPThumbs(int GotoSlide)
        {
            bool isCurrentOutputPPImageSame = false;
            OutputPPTotalImagesCount = Gf.OutputItem.TotalSlides;

            if (!String.IsNullOrEmpty(previousOutputPPImageName))
            {
                if (previousOutputPPImageName != Gf.OUTPPFullPath + Convert.ToString(1) + ".jpg")
                {
                    isCurrentOutputPPImageSame = false;
                }
                else
                {
                    isCurrentOutputPPImageSame = true;
                }
            }

            if (!isCurrentOutputPPImageSame)
            {
                //for (int i = 0; i < 1024; i++)
                for (int i = 0; i < OutputPPTotalImagesCount; i++)
                {
                    OutputPPImagename[i] = "";
                }
                //for (int i = 0; i < 1024; i++)
                for (int i = 0; i < OutputPPTotalImagesCount; i++)
                {
                    OutputPPTIHoldersFileName[i] = "";
                    OutputPPSlideNumber[i] = -1;
                }
                for (int i = 0; i <= ((OutputPPTotalImagesCount < 1024) ? (OutputPPTotalImagesCount - 1) : 1023); i++)
                {
                    OutputPPImagename[i] = Gf.OUTPPFullPath + Convert.ToString(i + 1) + ".jpg";
                }
            }

            //OutputPPTotalImagesCount = Gf.PreviewItem.TotalSlides;
            //if (OutputPPTotalImagesCount > 1024)
            //	OutputPPTotalImagesCount = 1024;

            //for (int i = 0; i < OutputPPTotalImagesCount; i++)
            //{
            //	OutputPPImagename[i] = "";
            //	OutputPPTIHoldersFileName[i] = "";
            //	OutputPPSlideNumber[i] = -1;
            //	OutputPPImagename[i] = Gf.OUTPPFullPath + Convert.ToString(i + 1) + ".jpg"; ;
            //}

            SetOutputPPThumbImages1(GotoSlide);
            previousOutputPPImageName = OutputPPImagename[0];
        }

        //private void SetOutputPPThumbImages()
        //{
        //	SetOutputPPThumbImages(0);
        //}

        private void SetOutputPPThumbImages1(int GotoSlide)
        {
            if (Gf.OutputItem.Type == "P")
            {
                if (GotoSlide < 0)
                {
                    GotoSlide = Gf.OutputItem.TotalSlides;
                }
                LoadThumbOutImages(flowLayoutOutputPowerPoint, ref Powerpoint_OutputCanvas, OutputPPImagename, Gf.OutputItem.TotalSlides, flowLayoutOutputPowerPoint.Width, Gf.OUTPPFullPath, GotoSlide, toolTip1, ExternalPP: true);
                RefreshLiveIndicators();
            }
        }

        //private void SetOutputPPThumbImages2(int GotoSlide)
        //{
        //	if (Gf.OutputItem.Type == "P")
        //	{
        //		if (GotoSlide < 0)
        //		{
        //			GotoSlide = Gf.OutputItem.TotalSlides;
        //		}
        //		LoadThumbImages(flowLayoutOutputPowerPoint, ref Powerpoint_OutputCanvas, OutputPPImagename, Gf.OutputItem.TotalSlides, flowLayoutOutputPowerPoint.Width, Gf.OUTPPFullPath, GotoSlide, toolTip1, ExternalPP: true);
        //	}
        //}

        private void MakePowerpointOutputVisible(SongSettings InItem, bool MakeVisible)
        {
            if (InItem.OutputStyleScreen)
            {
                flowLayoutOutputPowerPoint.Visible = MakeVisible;
            }
            else
            {
                flowLayoutPreviewPowerPoint.Visible = MakeVisible;
            }
        }

        private void ShowDualMonitorPP_Output(ref SongSettings InItem)
        {
            if (InItem.TotalSlides != 0)
            {
                try
                {
                    InItem.UseDefaultFormat = false;
                    Gf.DrawText(ref InItem, ref OutputScreen, ref OutputScreen, InItem.LyricsAndNotationsList);
                    Gf.SetShowBackground(InItem, ref OutputScreen);
                    Gf.DrawText(ref InItem, ref OutputScreen, ref OutputScreen, InItem.LyricsAndNotationsList);
                }
                catch
                {
                }
            }
        }

        private string DragDropItemType(DragEventArgs e)
        {
            int lastFormatIndex = e.Data.GetFormats().Length - 1;
            for (int i = 0; i <= lastFormatIndex; i++)
            {
                if (e.Data.GetFormats()[i].Equals("System.Windows.Forms.ListView+SelectedListViewItemCollection"))
                {
                    if (DragListView == DragDropSource.WorshipList.ToString())
                    {
                        return DragDropSource.WorshipList.ToString();
                    }
                    if (DragListView == DragDropSource.SongsList.ToString())
                    {
                        return DragDropSource.SongsList.ToString();
                    }
                    if (DragListView == DragDropSource.InfoScreenList.ToString())
                    {
                        return DragDropSource.InfoScreenList.ToString();
                    }
                    if (DragListView == DragDropSource.PowerpointList.ToString())
                    {
                        return DragDropSource.PowerpointList.ToString();
                    }
                    if (DragListView == DragDropSource.MediaList.ToString())
                    {
                        return DragDropSource.MediaList.ToString();
                    }
                    return "";
                }
                if (e.Data.GetFormats()[i].Equals(DataFormats.FileDrop))
                {
                    return DataFormats.FileDrop;
                }
                if (e.Data.GetFormats()[i].Equals(DragDropSource.BiblePassage.ToString()))
                {
                    return DragDropSource.BiblePassage.ToString();
                }
            }
            return "";
        }

        // Each list passes its own selected items. DragListView names the source only while the drag runs
        // (DoDragDrop returns when it ends), so a later drag from outside this form is never taken for it.
        private void SongsList_ItemDrag(object sender, ItemDragEventArgs e)
        {
            DragListView = DragDropSource.SongsList.ToString();
            try
            {
                SongsList.DoDragDrop(SongsList.SelectedItems, DragDropEffects.Link);
            }
            finally
            {
                DragListView = "";
            }
        }

        private void InfoScreenList_ItemDrag(object sender, ItemDragEventArgs e)
        {
            DragListView = DragDropSource.InfoScreenList.ToString();
            try
            {
                InfoScreenList.DoDragDrop(InfoScreenList.SelectedItems, DragDropEffects.Link);
            }
            finally
            {
                DragListView = "";
            }
        }

        private void PowerpointList_ItemDrag(object sender, ItemDragEventArgs e)
        {
            DragListView = DragDropSource.PowerpointList.ToString();
            try
            {
                PowerpointList.DoDragDrop(PowerpointList.SelectedItems, DragDropEffects.Link);
            }
            finally
            {
                DragListView = "";
            }
        }

        private void MediaList_ItemDrag(object sender, ItemDragEventArgs e)
        {
            DragListView = DragDropSource.MediaList.ToString();
            try
            {
                MediaList.DoDragDrop(MediaList.SelectedItems, DragDropEffects.Link);
            }
            finally
            {
                DragListView = "";
            }
        }

        private void WorshipList_ItemDrag(object sender, ItemDragEventArgs e)
        {
            DragListView = DragDropSource.WorshipList.ToString();
            try
            {
                WorshipListItems.DoDragDrop(WorshipListItems.SelectedItems, DragDropEffects.Link);
            }
            finally
            {
                DragListView = "";
            }
        }

        /// <summary>
        /// A drag enters the worship list: no row is marked yet, and the drop effect is offered for the
        /// dragged data (list items, Bible passages and supported files, including .docx).
        /// </summary>
        /// <param name="sender">The worship list.</param>
        /// <param name="e">The drag data.</param>
        private void WorshipList_DragEnter(object sender, DragEventArgs e)
        {
            // A new drag over the list: no row is marked yet.
            WorshipListInsertAt = -1;
            e.Effect = WorshipListDropEffect(e);
        }

        /// <summary>
        /// The drop effect the worship list offers for the dragged data, wherever it is dropped (Link for
        /// list items, Bible passages and supported files; None for anything else).
        /// </summary>
        private DragDropEffects WorshipListDropEffect(DragEventArgs e)
        {
            string dragType = DragDropItemType(e);
            if (dragType == DragDropSource.WorshipList.ToString())
            {
                return DragDropEffects.Link;
            }
            else if (dragType == DragDropSource.SongsList.ToString())
            {
                return DragDropEffects.Link;
            }
            else if (dragType == DragDropSource.InfoScreenList.ToString())
            {
                return DragDropEffects.Link;
            }
            else if (dragType == DragDropSource.PowerpointList.ToString())
            {
                return DragDropEffects.Link;
            }
            else if (dragType == DragDropSource.MediaList.ToString())
            {
                return DragDropEffects.Link;
            }
            else if (dragType == DataFormats.FileDrop)
            {
                bool allSupported = true;
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (!Gf.SupportedImageFormat(files[0]))
                {
                    for (int i = 0; i <= files.Length - 1; i++)
                    {
                        string strExt = Path.GetExtension(files[i]).ToLower();
                        bool isPowerpoint = strExt == ".ppt" || strExt == ".pptx";
                        bool isDoc = strExt == ".doc" || strExt == ".docx";
                        bool isText = strExt == ".txt";
                        bool isInfo = strExt == ".esi";
                        bool isWorship = strExt == ".esw";
                        bool isMedia = ValidMediaExt(strExt);
                        if ((!isPowerpoint && !isDoc && !isText && !isInfo && !isWorship && !isMedia) || (isPowerpoint && Gf.ShowRunning))
                        {
                            allSupported = false;
                        }
                    }
                }
                return (allSupported ? DragDropEffects.Link : DragDropEffects.None);
            }
            else if (dragType == DragDropSource.BiblePassage.ToString())
            {
                return DragDropEffects.Link;
            }
            return DragDropEffects.None;
        }

        /// <summary>
        /// Marks the row under the cursor while something is dragged over the worship list, and shows
        /// "no drop" where no place can be told (WorshipListDropIndex -1). The mark only shows the place;
        /// the drop itself takes its place from the drop point.
        /// </summary>
        private void WorshipList_DragOver(object sender, DragEventArgs e)
        {
            int dropAt = WorshipListDropIndex(e.X, e.Y);
            e.Effect = (dropAt < 0) ? DragDropEffects.None : WorshipListDropEffect(e);
            ListViewItem itemAt = (dropAt >= 0 && dropAt < WorshipListItems.Items.Count) ? WorshipListItems.Items[dropAt] : null;
            if (itemAt == null)
            {
                // No row here (below the last row, or no place): drop the mark, so no earlier row stays marked.
                if (WorshipListInsertAt >= 0)
                {
                    ResetListViewBackgroundColour(WorshipListItems);
                }
                WorshipListInsertAt = -1;
                return;
            }
            // The whole row counts, its last pixel line included (the drop goes before this row).
            WorshipListInsertAt = itemAt.Index;
            itemAt.BackColor = Color.Yellow;
            if (WorshipListInsertAt > 0)
            {
                WorshipListItems.Items[WorshipListInsertAt - 1].BackColor = SongFolder.BackColor;
                WorshipListItems.Items[WorshipListInsertAt - 1].EnsureVisible();
            }
            if (WorshipListInsertAt != WorshipListItems.Items.Count - 1)
            {
                WorshipListItems.Items[WorshipListInsertAt + 1].BackColor = SongFolder.BackColor;
                WorshipListItems.Items[WorshipListInsertAt + 1].EnsureVisible();
            }
        }

        /// <summary>
        /// Where dropped items go in the worship list, from the drop point (screen coordinates): before
        /// the row level with the point; at the end when the point is below the last row or the list is empty;
        /// -1 anywhere else (no place can be told: nothing is inserted or moved). Only the height counts: the
        /// list shows column 0 alone (the other columns are 0 wide), and GetItemAt finds no row to the right
        /// of it, so X is taken into column 0 first. With no header, -1 is left only for the list's own top
        /// border, above the first visible row.
        /// </summary>
        private int WorshipListDropIndex(int screenX, int screenY)
        {
            Point point = WorshipListItems.PointToClient(new Point(screenX, screenY));
            int columnWidth = (WorshipListItems.Columns.Count > 0) ? WorshipListItems.Columns[0].Width : WorshipListItems.ClientSize.Width;
            int x = Math.Max(0, Math.Min(point.X, columnWidth - 1));
            ListViewItem itemAt = WorshipListItems.GetItemAt(x, point.Y);
            if (itemAt != null)
            {
                return itemAt.Index;
            }
            int count = WorshipListItems.Items.Count;
            if (count == 0 || point.Y >= WorshipListItems.Items[count - 1].Bounds.Bottom)
            {
                return count;
            }
            return -1;
        }

        /// <summary>
        /// Whether moving the selected worship list rows before row InsertAt changes the order of the list.
        /// The result order is: the other rows before InsertAt, the selected rows (in list order), the other
        /// rows from InsertAt on - the same order the move in WorshipList_DragDrop produces.
        /// </summary>
        private bool WorshipListMoveChangesOrder(int InsertAt)
        {
            var before = new System.Collections.Generic.List<ListViewItem>();
            var moved = new System.Collections.Generic.List<ListViewItem>();
            var after = new System.Collections.Generic.List<ListViewItem>();
            for (int i = 0; i < WorshipListItems.Items.Count; i++)
            {
                ListViewItem item = WorshipListItems.Items[i];
                if (item.Selected)
                {
                    moved.Add(item);
                }
                else if (i < InsertAt)
                {
                    before.Add(item);
                }
                else
                {
                    after.Add(item);
                }
            }
            if (moved.Count == 0)
            {
                return false;
            }
            before.AddRange(moved);
            before.AddRange(after);
            for (int i = 0; i < before.Count; i++)
            {
                if (!ReferenceEquals(before[i], WorshipListItems.Items[i]))
                {
                    return true;
                }
            }
            return false;
        }

        private void WorshipList_DragDrop(object sender, DragEventArgs e)
        {
            // The place comes from the drop point, never from the row DragOver marked last: DragOver may not
            // have run at all (a click the list control took for the start of a drag) or may be stale.
            int dropAt = WorshipListDropIndex(e.X, e.Y);
            WorshipListInsertAt = -1;
            string dragType = DragDropItemType(e);
            if (dragType == DragDropSource.WorshipList.ToString())
            {
                e.Effect = DragDropEffects.Link;
                // A drop that leaves the order as it is (on the dragged row itself, on the row right after
                // the dragged rows, or where no place can be told) changes nothing and saves nothing; the
                // row objects stay, so the output and preview marks stay too.
                if (dropAt >= 0 && WorshipListMoveChangesOrder(dropAt))
                {
                    MoveSelectedWorshipListRows(dropAt);
                }
            }
            // From the other lists, files and Bible passages: inserted before the row under the drop point
            // (at the end below the last row); where no place can be told (dropAt -1) nothing is inserted.
            else if (dragType == DragDropSource.SongsList.ToString())
            {
                e.Effect = DragDropEffects.Copy;
                if (dropAt >= 0)
                {
                    AddFromSongsList(dropAt);
                }
            }
            else if (dragType == DragDropSource.InfoScreenList.ToString())
            {
                e.Effect = DragDropEffects.Copy;
                if (dropAt >= 0)
                {
                    AddFromInfoScreensList(dropAt);
                }
            }
            else if (dragType == DragDropSource.PowerpointList.ToString())
            {
                e.Effect = DragDropEffects.Copy;
                if (dropAt >= 0)
                {
                    AddFromPowerpointList(dropAt);
                }
            }
            else if (dragType == DragDropSource.MediaList.ToString())
            {
                e.Effect = DragDropEffects.Copy;
                if (dropAt >= 0)
                {
                    AddFromMediaFilesList(dropAt);
                }
            }
            else if (dragType == DataFormats.FileDrop)
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (Gf.SupportedImageFormat(files[0]))
                {
                    // An image is not inserted: it becomes the background of the row it is dropped on (that
                    // row alone: ApplyBackground writes the preview item's format into every selected row),
                    // or, dropped below the last row, the default background (no row selected). Where no
                    // place can be told nothing happens: the default background would change the output.
                    if (dropAt >= WorshipListItems.Items.Count)
                    {
                        WorshipList_UnselectAll();
                        ApplyBackground(files[0]);
                    }
                    else if (dropAt >= 0)
                    {
                        SelectWorshipListItem(dropAt, 1);
                        ApplyBackground(files[0]);
                    }
                }
                else if (dropAt >= 0)
                {
                    int addedCount = 0;
                    for (int i = 0; i <= files.Length - 1; i++)
                    {
                        AddExternalFileToWorshipList(files[i], dropAt + addedCount);
                        addedCount++;
                    }
                    if (files.Length > 0)
                    {
                        SelectWorshipListItem(dropAt, addedCount);
                    }
                }
            }
            else if (dragType == DragDropSource.BiblePassage.ToString())
            {
                if (dropAt >= 0)
                {
                    AddFromHolyBible(dropAt);
                }
            }
            ResetListViewBackgroundColour(WorshipListItems);
            WorshipListIndexChanged();
            UpdateOutputCurItemNo();
        }

        /// <summary>
        /// Moves the selected worship list rows before row InsertAt (an index of the list before the move;
        /// Items.Count for the end), keeps them selected and saves the list. All or nothing: when a copy of a
        /// row cannot be inserted, the copies already inserted are taken out again, the list and its
        /// selection stay as they were, nothing is saved and the operator is told.
        /// </summary>
        private void MoveSelectedWorshipListRows(int InsertAt)
        {
            int insertAt = InsertAt;
            int selectedCount = WorshipListItems.SelectedItems.Count;
            int selectedBefore = 0;
            for (int i = 0; i < insertAt; i++)
            {
                if (WorshipListItems.Items[i].Selected)
                {
                    selectedBefore++;
                }
            }
            ListViewItem[] selectedRows = new ListViewItem[selectedCount];
            WorshipListItems.SelectedItems.CopyTo(selectedRows, 0);
            var inserted = new System.Collections.Generic.List<ListViewItem>();
            try
            {
                foreach (ListViewItem selectedItem in selectedRows)
                {
                    ListViewItem copy = (ListViewItem)selectedItem.Clone();
                    WorshipListItems.Items.Insert(insertAt + inserted.Count, copy);
                    inserted.Add(copy);
                }
            }
            catch (Exception ex)
            {
                foreach (ListViewItem copy in inserted)
                {
                    try
                    {
                        WorshipListItems.Items.Remove(copy);
                    }
                    catch
                    {
                    }
                }
                WriteDebugLogThrottled("MoveSelectedWorshipListRows", ex);
                ReportErrorNotice("The worship list item could not be moved; the list is unchanged");
                return;
            }
            RemoveWorshipListSong(UpdateCurItemNo: false);
            insertAt -= selectedBefore;
            // Rows moved to the top (insertAt 0) are selected as well.
            for (int i = insertAt; i < selectedCount + insertAt && i < WorshipListItems.Items.Count; i++)
            {
                WorshipListItems.Items[i].Selected = true;
            }
            SaveWorshipList();
        }

        private void WorshipList_DragLeave(object sender, EventArgs e)
        {
            ResetListViewBackgroundColour(WorshipListItems);
            WorshipListInsertAt = -1;
        }

        private void PraiseBookItems_DragEnter(object sender, DragEventArgs e)
        {
            string dragType = DragDropItemType(e);
            if (dragType == "SongsList")
            {
                e.Effect = DragDropEffects.Link;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void PraiseBookItems_DragDrop(object sender, DragEventArgs e)
        {
            string dragType = DragDropItemType(e);
            if (dragType == "SongsList")
            {
                e.Effect = DragDropEffects.Link;
                AddToPraiseBookList();
            }
        }

        private void cbGoLive_Click(object sender, EventArgs e)
        {
            GoLive(cbGoLive.Checked);
        }

        private void btnToLive_Click(object sender, EventArgs e)
        {
            PreviewItemToLive();
        }

        private void Menu_PreviewGoLiveNext_Click(object sender, EventArgs e)
        {
            if (Gf.PreviewItem.ItemID != "")
            {
                CopyPreviewToOutput();
            }
            ManualMoveToItem(Gf.PreviewItem, KeyDirection.NextOne);
            FocusOutputArea();
            GoLive(InStatus: true);
        }

        private void PreviewItemToLive()
        {
            if (Gf.ShowRunning)
            {
                if (Gf.PreviewItem.ItemID != "")
                {
                    CopyPreviewToOutput();
                }
            }
            else if (Gf.PreviewItem.ItemID != "")
            {
                CopyPreviewToOutput();
                GoLive(InStatus: true);
            }
        }

        private void GoLive(bool InStatus)
        {
            cbGoLive.Checked = InStatus;
            Menu_StartShow.Checked = InStatus;
            if (InStatus)
            {
                if (!Gf.ShowRunning)
                {
                    Menu_StartShow.Checked = InStatus;
                    Start_Presentation();
                    // A modal show returns only when it ends; the main form may have been closed meanwhile.
                    if (MainFormClosing)
                    {
                        return;
                    }
                }
            }
            else
            {
                cbGoLive.ForeColor = DefaultButtonForeColour;
                if (Gf.ShowRunning)
                {
                    Gf.ShowRunning = false;
                    LiveShow.StopShow();
                    UpdateWorshipShowIcons();
                }
            }
            if (Gf.DualMonitorMode)
            {
                FocusOutputArea();
                Focus();
            }
            RefreshLiveIndicators();
        }

        private void Start_Presentation()
        {
            if (Gf.OutputItem.ItemID == "")
            {
                Gf.StartPresAt = 1;
                LoadWorshipListItemToOutput(Gf.StartPresAt, 0);
                if (Gf.OutputItem.ItemID == "")
                {
                    GoLive(InStatus: false);
                    MessageBox.Show("Can't start the Show because the Worship List is empty!");
                    return;
                }
            }
            if (ValidateWorshipListItems(ShowErrorMessage: true) < 0)
            {
                Gf.ShowRunning = true;
                Gf.ResetShowRunningSettings();
                Gf.ResetPowerPointShowState();
                mainPptRunItemPath = null;
                UpdateWorshipShowIcons();
                Gf.SaveShowOptions = false;
                Gf.OutputItem.GapItemOnDisplay = false;
                DisplayInfo.SizeLaunchDisplay();
                // Slide shows a crashed or killed run left topmost (they are not closed).
                Gf.LowerLeftoverSlideShowWindows("Start_Presentation");
                // The clean-up an earlier show end skipped because PowerPoint did not answer, before this show's
                // pre-load opens its own copies (fix-ppt-show-end-timeout T3).
                Gf.RetryPowerPointCleanUp("Start_Presentation");
                FormWindowState formWindowState;
                if (Gf.DualMonitorMode)
                {
                    Gf.LaunchShowUpdateDone = false;
                    formWindowState = (base.WindowState = base.WindowState);

                    gfFileHelpers.PreLoadPowerpointFiles(ref Gf.LivePP, ref Gf.WorshipSongs);
                    bool started = false;
                    try
                    {
                        EnsureLiveShowWindow();
                        LiveShow.Show();
                        if (LiveShow.StartInitCompleted)
                        {
                            started = true;
                        }
                        else
                        {
                            // The show window's initialisation for this start (Load, or InitForm from
                            // its VisibleChanged) threw. That exception did not leave Show(): it went to
                            // Application.ThreadException, which logged it and showed the notice, so
                            // only the log line is written here.
                            WriteDebugLogThrottled("Start_Presentation Show", new InvalidOperationException("The show window did not finish initialising for this start (see the exception logged before)."));
                        }
                    }
                    catch (Exception ex)
                    {
                        // Show() itself threw: nothing else has reported it.
                        LogLiveShowFailure("Start_Presentation Show", ex);
                    }
                    if (!started)
                    {
                        EndFailedShowStart();
                        return;
                    }
                    // The show window's load at its start is of the item already on the output: nothing the main
                    // form's next item load has to reconcile with.
                    liveLoadSerialSeen = Gf.LiveLoadSerial;
                    FocusOutputArea();
                    return;
                }
                MinAllWindows();
                TimerReMax.Enabled = true;
                formWindowState = base.WindowState;
                base.WindowState = FormWindowState.Minimized;
                gfFileHelpers.PreLoadPowerpointFiles(ref Gf.LivePP, ref Gf.WorshipSongs);
                EnsureLiveShowWindow();
                try
                {
                    LiveShow.ShowDialog();
                }
                catch (Exception ex)
                {
                    // Only ShowDialog's own failures arrive here (e.g. the window is already visible);
                    // exceptions inside the show window's Load / InitForm or later go to
                    // Application.ThreadException instead. The code below restores the main window
                    // and must run; the show window did not run, so the show is not running.
                    LogLiveShowFailure("Start_Presentation ShowDialog", ex);
                    Gf.ShowRunning = false;
                }
                // The main form was closed while the modal show ran (Task Manager's End task, or the end of a Windows
                // session; a shutdown that is only being asked about does not end the modal show, see
                // FrmLaunchShow.WndProc): the show is ended (FormClosingCleanup / EndShowBeforeClosing) and the
                // form is closing. Nothing below may touch it.
                if (MainFormClosing || LiveShow == null || LiveShow.IsDisposed)
                {
                    return;
                }
                // A window whose Load failed lacks what only Load does (LiveScreen mouse handlers, the
                // hidden cursor) and Load never runs again for it: replace it for the next start.
                TryStep("Replace show window after a failed Load", () =>
                {
                    if (LiveShow != null && !LiveShow.IsDisposed && !LiveShow.FirstLoadCompleted)
                    {
                        LiveShow.Dispose();
                        EnsureLiveShowWindow();
                    }
                });
                Gf.ParentalAlertLive = false;
                Gf.MessageAlertLive = false;
                if (Gf.OutputItem.CurItemNo == 0)
                {
                    CopyPreviewToOutput();
                }
                else if (Gf.LiveItem.Type == "G")
                {
                    LoadItem(ref Gf.OutputItem, "G1", "", 0);
                }
                else
                {
                    LoadWorshipListItemToOutput(Gf.StartPresAt, Gf.OutputItem.CurSlide, NotifyLiveShow: false);
                }
                base.WindowState = formWindowState;
                UpdateWorshipShowIcons();
                Menu_StartShow.Checked = false;
                cbGoLive.Checked = false;
                cbGoLive.ForeColor = DefaultButtonForeColour;
                SetAutoRotateButtons();
                LiveBlack(Gf.ShowLiveBlack);
                LiveClear(Gf.ShowLiveClear);
                //LiveCam(Gf.ShowLiveCam);
                if (Gf.SaveShowOptions)
                {
                    SaveWorshipList();
                }
            }
            else
            {
                GoLive(InStatus: false);
            }
        }

        private void Start_Presentation_Old()
        {
            if (Gf.ShowRunning)
            {
                return;
            }
            if (Gf.OutputItem.ItemID == "")
            {
                Gf.StartPresAt = 1;
                LoadWorshipListItemToOutput(Gf.StartPresAt, 0);
                if (Gf.OutputItem.ItemID == "")
                {
                    GoLive(InStatus: false);
                    MessageBox.Show("Can't start the Show because the Worship List is empty!");
                    return;
                }
            }
            Cursor.Current = Cursors.WaitCursor;
            if (ValidateWorshipListItems(ShowErrorMessage: true) < 0)
            {
                Gf.ShowRunning = true;
                Gf.ResetShowRunningSettings();
                UpdateWorshipShowIcons();
                Gf.SaveShowOptions = false;
                DisplayInfo.SizeLaunchDisplay();
                try
                {
                    LiveShow.Close();
                    LiveShow = null;
                }
                catch
                {
                }
                try
                {
                    LiveShow = new FrmLaunchShow();
                    LiveShow.OnMessage += LiveShow_OnMessage;
                }
                catch
                {
                }
                FormWindowState formWindowState;
                if (Gf.DualMonitorMode)
                {
                    Gf.LaunchShowUpdateDone = false;
                    formWindowState = (base.WindowState = base.WindowState);
                    gfFileHelpers.PreLoadPowerpointFiles(ref Gf.LivePP, ref Gf.WorshipSongs);
                    try
                    {
                        LiveShow.Show();
                    }
                    catch
                    {
                    }
                    FocusOutputArea();
                    return;
                }
                MinAllWindows();
                TimerReMax.Enabled = true;
                formWindowState = base.WindowState;
                base.WindowState = FormWindowState.Minimized;
                Cursor.Current = Cursors.Default;
                Cursor.Hide();
                gfFileHelpers.PreLoadPowerpointFiles(ref Gf.LivePP, ref Gf.WorshipSongs);
                try
                {
                    LiveShow.ShowDialog();
                }
                catch
                {
                }
                Cursor.Current = Cursors.Default;
                Cursor.Hide();
                Cursor.Show();
                Gf.ParentalAlertLive = false;
                Gf.MessageAlertLive = false;
                if (Gf.OutputItem.CurItemNo == 0)
                {
                    CopyPreviewToOutput();
                }
                else if (Gf.LiveItem.Type == "G")
                {
                    LoadItem(ref Gf.OutputItem, "G1");
                }
                else
                {
                    LoadWorshipListItemToOutput(Gf.StartPresAt, Gf.OutputItem.CurSlide, NotifyLiveShow: false);
                }
                base.WindowState = formWindowState;
                UpdateWorshipShowIcons();
                Menu_StartShow.Checked = false;
                cbGoLive.Checked = false;
                cbGoLive.ForeColor = DefaultButtonForeColour;
                SetAutoRotateButtons();
                LiveBlack(Gf.ShowLiveBlack);
                LiveClear(Gf.ShowLiveClear);
                //LiveCam(Gf.ShowLiveCam);
                if (Gf.SaveShowOptions)
                {
                    SaveWorshipList();
                }
            }
            else
            {
                GoLive(InStatus: false);
            }
        }

        [SupportedOSPlatform("windows")]
        private void MinAllWindows()
        {
            Type typeFromProgID = Type.GetTypeFromProgID("Shell.Application");
            object target = Activator.CreateInstance(typeFromProgID);
            typeFromProgID.InvokeMember("MinimizeAll", BindingFlags.InvokeMethod, null, target, null);
        }

        internal void LiveBlack(bool InStatus)
        {
            try
            {
                Gf.ShowLiveBlack = InStatus;
                cbOutputBlack.Checked = Gf.ShowLiveBlack;
                Menu_BlackScreen.Checked = Gf.ShowLiveBlack;
                PostitionBlackClearGapLabels();
                RedrawOutputItemText();
                cbOutputBlack.ImageIndex = (cbOutputBlack.Checked ? 16 : 15);
                if (Gf.ShowRunning)
                {
                    RemoteControlLiveShow(LiveShowAction.Remote_LiveBlackClearChanged);
                }
                FocusOutputArea();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Debug.WriteLine(ex.StackTrace);
            }



        }

        private void LiveClear(bool InStatus)
        {
            Gf.ShowLiveClear = InStatus;
            cbOutputClear.Checked = Gf.ShowLiveClear;
            Menu_ClearScreen.Checked = Gf.ShowLiveClear;
            PostitionBlackClearGapLabels();
            RedrawOutputItemText();
            cbOutputClear.ImageIndex = (cbOutputClear.Checked ? 18 : 17);
            if (Gf.ShowRunning)
            {
                RemoteControlLiveShow(LiveShowAction.Remote_LiveBlackClearChanged);
            }
            FocusOutputArea();
        }

        private void PostitionBlackClearGapLabels()
        {
            if (Gf.ShowLiveBlack)
            {
                labelBlackScreen.Left = 0;
            }
            labelBlackScreen.Visible = (Gf.ShowLiveBlack ? true : false);
            if (Gf.ShowLiveClear)
            {
                labelHideText.Left = (Gf.ShowLiveBlack ? labelBlackScreen.Width : 0);
            }
            labelHideText.Visible = (Gf.ShowLiveClear ? true : false);
            if (Gf.OutputItem.Type == "G")
            {
                labelGapItem.Left = (Gf.ShowLiveBlack ? labelBlackScreen.Width : 0) + (Gf.ShowLiveClear ? labelHideText.Width : 0);
            }
            labelGapItem.Visible = ((Gf.OutputItem.Type == "G") ? true : false);
            ResizeOutputBottomPanel();
            RefreshLiveIndicators();
        }

        //private void LiveCam(bool InStatus)
        //{
        //    Gf.ShowLiveCam = InStatus;
        //    cbOutputCam.Checked = Gf.ShowLiveCam;
        //    Menu_LiveCam.Checked = Gf.ShowLiveCam;
        //    cbOutputCam.ImageIndex = (cbOutputCam.Checked ? 31 : 30);
        //    if (Gf.ShowRunning)
        //    {
        //        RemoteControlLiveShow(LiveShowAction.Remote_LiveCamStartStop);
        //    }
        //    FocusOutputArea();
        //}

        private void RedrawOutputItemText()
        {
            ShowSlide(ref Gf.OutputItem, Gf.GapItemUseFade ? ImageTransitionControl.TransitionAction.AsFade : ImageTransitionControl.TransitionAction.None);
        }

        /// <summary>
        /// Checks every worship list item (and an ad hoc output item) and fills Gf.WorshipSongs from them.
        /// </summary>
        /// <param name="ShowErrorMessage">Whether a problem is shown in a message box.</param>
        /// <returns>-1 when all items are valid; else the index of the first invalid item (1 when there are more PowerPoint files than permitted).</returns>
        private int ValidateWorshipListItems(bool ShowErrorMessage)
        {
            bool powerpointChecked = false;
            string typeSymbol = "";
            int powerpointCount = 0;

            using DbConnection connection = DbController.GetDbConnection(Gf.ConnectStringMainDB);

            Gf.AdHocItemPresent = ((Gf.OutputItem.Source != ItemSource.WorshipList) ? true : false);
            for (int i = 0; i <= WorshipListItems.Items.Count - ((!Gf.AdHocItemPresent) ? 1 : 0); i++)
            {
                string title;
                string idString;
                string inString;
                int songIndex;
                if (Gf.AdHocItemPresent && i == WorshipListItems.Items.Count)
                {
                    title = Gf.OutputItem.InMainItemText;
                    idString = Gf.OutputItem.InSubItemItem1Text;
                    inString = Gf.OutputItem.Format.FormatString;
                    songIndex = 0;
                }
                else
                {
                    title = WorshipListItems.Items[i].Text;
                    idString = WorshipListItems.Items[i].SubItems[1].Text;
                    inString = WorshipListItems.Items[i].SubItems[2].Text;
                    songIndex = i + 1;
                }
                typeSymbol = DataUtil.Left(idString, 1);
                string idValue = DataUtil.Right(idString, idString.Length - 1);
                if (typeSymbol == "D")
                {
                    bool songFound = false;
                    int folderNo = 0;
                    try
                    {
                        using DbCommand comm = new DbCommand($"Select FolderNo From SONG Where SONGID={idValue}", connection);

                        object objValue = comm.ExecuteScalar();

                        if (objValue != null)
                        {
                            Gf.WorshipSongs[songIndex].IdString = idString;
                            Gf.WorshipSongs[songIndex].Type = "D";
                            Gf.WorshipSongs[songIndex].Title = Gf.RemoveMusicSym(title);
                            Gf.WorshipSongs[songIndex].Flag = " ";
                            Gf.WorshipSongs[songIndex].FormatString = DataUtil.Trim(inString);
                            folderNo = DataUtil.ObjToInt(objValue);
                            songFound = true;
                        }
                    }
                    catch
                    {
                    }
                    if (!songFound)
                    {
                        if (ShowErrorMessage)
                        {
                            MessageBox.Show(this, "Sorry - Can't find the song '" + Gf.RemoveMusicSym(title) + "' in the Database!");
                        }
                        return i;
                    }
                    if (folderNo == 0)
                    {
                        if (ShowErrorMessage)
                        {
                            MessageBox.Show(this, "Sorry - Can't show the Database song '" + Gf.RemoveMusicSym(title) + "' because it has been deleted!");
                        }
                        return i;
                    }
                }
                else if (typeSymbol == "P")
                {
                    if (!powerpointChecked)
                    {
                        if (!Gf.PowerpointPresent())
                        {
                            if (ShowErrorMessage)
                            {
                                MessageBox.Show(this, "Error - You have a Powerpoint item but there is no Microsoft Powerpoint Software on your computer");
                            }
                            return i;
                        }
                        powerpointChecked = true;
                    }
                    string powerpointPath = idValue;
                    if (!File.Exists(powerpointPath))
                    {
                        if (ShowErrorMessage)
                        {
                            MessageBox.Show(this, "Sorry - Can't find the Powerpoint File '" + Gf.RemoveMusicSym(title) + "'");
                        }
                        return i;
                    }
                    powerpointCount++;
                    Gf.WorshipSongs[songIndex].IdString = "P" + powerpointPath;
                    Gf.WorshipSongs[songIndex].Type = "P";
                    Gf.WorshipSongs[songIndex].Title = Gf.RemoveMusicSym(title);
                    Gf.WorshipSongs[songIndex].Flag = " ";
                    Gf.WorshipSongs[songIndex].FormatString = "";
                }
                else if (typeSymbol == "B")
                {
                    Gf.WorshipSongs[songIndex].IdString = idString;
                    Gf.WorshipSongs[songIndex].Type = "B";
                    Gf.WorshipSongs[songIndex].Title = title;
                    Gf.WorshipSongs[songIndex].Flag = " ";
                    Gf.WorshipSongs[songIndex].FormatString = DataUtil.Trim(inString);
                }
                else if (typeSymbol == "T")
                {
                    Gf.WorshipSongs[songIndex].IdString = idString;
                    Gf.WorshipSongs[songIndex].Type = "T";
                    Gf.WorshipSongs[songIndex].Title = title;
                    Gf.WorshipSongs[songIndex].Flag = " ";
                    Gf.WorshipSongs[songIndex].FormatString = DataUtil.Trim(inString);
                }
                else if (typeSymbol == "I")
                {
                    Gf.WorshipSongs[songIndex].IdString = idString;
                    Gf.WorshipSongs[songIndex].Type = "I";
                    Gf.WorshipSongs[songIndex].Title = title;
                    Gf.WorshipSongs[songIndex].Flag = " ";
                    Gf.WorshipSongs[songIndex].FormatString = DataUtil.Trim(inString);
                }
                else if (typeSymbol == "W")
                {
                    Gf.WorshipSongs[songIndex].IdString = idString;
                    Gf.WorshipSongs[songIndex].Type = "W";
                    Gf.WorshipSongs[songIndex].Title = title;
                    Gf.WorshipSongs[songIndex].Flag = " ";
                    Gf.WorshipSongs[songIndex].FormatString = DataUtil.Trim(inString);
                }
                else if (typeSymbol == "M")
                {
                    Gf.WorshipSongs[songIndex].IdString = idString;
                    Gf.WorshipSongs[songIndex].Type = "M";
                    Gf.WorshipSongs[songIndex].Title = title;
                    Gf.WorshipSongs[songIndex].Flag = " ";
                    Gf.WorshipSongs[songIndex].FormatString = DataUtil.Trim(inString);
                }
            }

            Gf.TotalWorshipListItems = WorshipListItems.Items.Count;
            if (powerpointCount > Gf.PP_MaxFiles)
            {
                string maxFilesMessage = "Error - There are " + powerpointCount + " Powerpoint files on the Worship List but you are only permitted to have " + Gf.PP_MaxFiles + ".";
                if (ShowErrorMessage)
                {
                    MessageBox.Show(this, maxFilesMessage);
                }
                else if (Gf.FirstPowerpointMaxFilesReport())
                {
                    // During a show: a box would block the operator (and hide behind the show window in single
                    // monitor mode). Once per show.
                    WriteDebugLog("ValidateWorshipListItems: " + maxFilesMessage);
                    ReportErrorNotice(maxFilesMessage);
                }
                return 1;
            }

            return -1;
        }

        private void Menu_WorshipLists_Click(object sender, EventArgs e)
        {
            WorshipListsManage();
        }

        private void WorshipListsManage()
        {
            Gf.WorshipListsChanged = false;
            Gf.PraiseBooksListChanged = false;
            Gf.Def_FormatString = LoadWorshipList(2, Gf.WorshipTemplatesDir + "Default.est");
            if (Gf.Def_FormatString == "")
            {
                Gf.Def_FormatString = " ";
            }
            FrmManageItemLists frmManageItemLists = new FrmManageItemLists();
            frmManageItemLists.ShowDialog();
            if (Gf.PraiseBooksListChanged)
            {
                string curPraiseBook = Gf.CurPraiseBook;
                PopulatePraiseBooksList();
                Gf.CurPraiseBook = curPraiseBook;
                Gf.PraiseBooksListChanged = false;
            }
            if (Gf.CurSession != SessionList.Text)
            {
                WriteCurSession();
                PopulateWorshipList();
                LoadWorshipList(0);
                SessionList.Text = Gf.CurSession;
            }
            else if (Gf.WorshipListsChanged)
            {
                PopulateWorshipList();
            }
        }

        private void Menu_PraiseBooks_Click(object sender, EventArgs e)
        {
            PraiseBooksManage();
        }

        private void PraiseBooksManage()
        {
            Gf.PraiseBooksListChanged = false;
            Gf.WorshipListsChanged = false;
            Gf.CurPraiseBook = PraiseBook.Text;
            Gf.Def_FormatString = "";
            FrmManageItemLists frmManageItemLists = new FrmManageItemLists();
            frmManageItemLists.ShowDialog();
            if (Gf.WorshipListsChanged)
            {
                string curSession = Gf.CurSession;
                PopulateWorshipList();
                Gf.CurSession = curSession;
                Gf.WorshipListsChanged = false;
            }
            if (Gf.CurPraiseBook != PraiseBook.Text)
            {
                WriteCurPraiseBook();
                PopulatePraiseBooksList();
                LoadPraiseBook(0);
                PraiseBook.Text = Gf.CurPraiseBook;
            }
            else if (Gf.PraiseBooksListChanged)
            {
                PopulatePraiseBooksList();
            }
        }

        private void Menu_ListingOfSelectedFolder_Click(object sender, EventArgs e)
        {
            ListingOfSelectedFolder_Clicked();
        }

        private void ListingOfSelectedFolder_Clicked()
        {
            if (IsSelectedTab(tabControlSource, "tabFolders"))
            {
                // The listing is of the whole folder: a filter of the find box is dropped first (design D26).
                ShowWholeSongFolderForReport();
                if (SongsList.Items.Count > 0)
                {
                    GenerateIndexReport();
                }
                else
                {
                    MessageBox.Show("Please select the Song Folder for which you want an index of the database items.");
                }
            }
            else
            {
                MessageBox.Show("Index of Selected Folder are for Database Items only!");
            }
        }

        private void Menu_EasiSlidesFolder_Click(object sender, EventArgs e)
        {
            Gf.RunProcess(Gf.RootEasiSlidesDir);
        }

        private void Menu_StatusBar_Click(object sender, EventArgs e)
        {
            Menu_StatusBar.Checked = !Menu_StatusBar.Checked;
            // The operator's choice replaces a bar the error notice had shown (HideStatusStripShownForNotice).
            statusStripShownForNotice = false;
            statusStripMain.Visible = Menu_StatusBar.Checked;
        }

        private void Menu_PreviewNotations_Click(object sender, EventArgs e)
        {
            PreviewNotations(Menu_PreviewNotations.Checked);
        }

        private void PreviewNotations(bool ShowNotations)
        {
            Gf.PreviewArea_ShowNotations = ShowNotations;
            Menu_PreviewNotations.Checked = ShowNotations;
            ApplyPreviewArea_Setup(2);
        }

        private void Menu_Refresh_Click(object sender, EventArgs e)
        {
            RefreshItems();
        }

        private void RefreshItems()
        {
            InitFormLoad = true;
            BuildFolderList();
            PopulateWorshipList();
            BuildPicturesFolderList();
            BuildInfoScreenFolderList();
            BuildMediaFolderList();
            BuildPowerpointFolderList();
            ShowPicturesFolderThumbs();
            PopulatePraiseBooksList();
            InitFormLoad = false;
            SongFolder_Change();
        }

        private void Menu_StartShow_Click(object sender, EventArgs e)
        {
            GoLive(!Gf.ShowRunning);
        }

        private void Menu_ClearScreen_Click(object sender, EventArgs e)
        {
            LiveClear(!Gf.ShowLiveClear);
        }

        //private void Menu_LiveCam_Click(object sender, EventArgs e)
        //{
        //    //LiveCam(!Gf.ShowLiveCam);
        //}

        private void Menu_RestartCurrentItem_Click(object sender, EventArgs e)
        {
            SetRotateState(RotateOn: false);
            Gf.RestartItemActioned = false;
            MoveToItem(Gf.OutputItem, KeyDirection.Refresh, 1, NotifyLiveShow: true);
        }

        private void Menu_SwitchChinese_Click(object sender, EventArgs e)
        {
            OutputChineseSwitch();
        }

        private void OutputChineseSwitch()
        {
            if (Gf.OutputItem.CompleteLyrics != "")
            {
                int switchResult = Gf.SwitchChinese(ref Gf.OutputItem.CompleteLyrics);
                Gf.SwitchChineseLyricsNotationListView(ref Gf.OutputItem, switchResult);
                DisplayLyrics(Gf.OutputItem, Gf.OutputItem.CurSlide, ScrollToCaret: true, 2, ImageTransitionControl.TransitionAction.None);
                if (Gf.ShowRunning && switchResult >= 0)
                {
                    RemoteControlLiveShow(LiveShowAction.Remote_ChineseChanged);
                }
            }
        }

        private void cbOutputBlack_Click(object sender, EventArgs e)
        {
            LiveBlack(cbOutputBlack.Checked);
        }

        private void cbOutputClear_Click(object sender, EventArgs e)
        {
            LiveClear(cbOutputClear.Checked);
        }

        //private void cbOutputCam_Click(object sender, EventArgs e)
        //{
        //    LiveCam(cbOutputCam.Checked);
        //}

        private void Menu_SelectAll_Click(object sender, EventArgs e)
        {
            SongsList_SelectAll();
        }

        private void SongsList_SelectAll()
        {
            if (SongsList.Items.Count > 0)
            {
                Cursor.Current = Cursors.WaitCursor;
                for (int i = 0; i <= SongsList.Items.Count - 1; i++)
                {
                    SongsList.Items[i].Selected = true;
                }
                SongsList.Focus();
                Cursor.Current = Cursors.Default;
            }
        }

        private void Menu_Import_Click(object sender, EventArgs e)
        {
            FrmImport frmImport = new FrmImport();
            frmImport.ShowDialog();
        }

        private void Menu_ImportFolder_Click(object sender, EventArgs e)
        {
            FrmImportFolder frmImportFolder = new FrmImportFolder();
            frmImportFolder.ShowDialog();
        }

        private void Menu_Export_Click(object sender, EventArgs e)
        {
            FrmExport frmExport = new FrmExport();
            frmExport.ShowDialog();
        }

        private void Menu_Find_Click(object sender, EventArgs e)
        {
            Find_Items();
        }

        private void Menu_ReArrangeSongFolders_Click(object sender, EventArgs e)
        {
            FrmRearrangeFolderPositions frmRearrangeFolderPositions = new FrmRearrangeFolderPositions();
            if (frmRearrangeFolderPositions.ShowDialog() == DialogResult.OK)
            {
                BuildFolderList();
                SetJumpToolTips();
            }
        }

        private void Menu_Options_Click(object sender, EventArgs e)
        {
            ViewOptions();
        }

        private void Menu_AlertWindow_Click(object sender, EventArgs e)
        {
            ShowAlertForm();
        }

        private void Menu_StopAlert_Click(object sender, EventArgs e)
        {
            ShowStopAlert();
        }

        private void ShowStopAlert()
        {
            Gf.ParentalAlertLive = false;
            Gf.MessageAlertLive = false;
        }

        private void Menu_AddSong_Click(object sender, EventArgs e)
        {
            if (IsSelectedTab(tabControlSource, "tabFolders"))
            {
                AddNew();
            }
            else if (tabControlSource.SelectedTab.Name == "tabFiles")
            {
                EF_New_Clicked();
            }
        }

        private void Menu_EditSong_Click(object sender, EventArgs e)
        {
            if (IsSelectedTab(tabControlSource, "tabFolders"))
            {
                Edit_SongsListSong();
            }
            else if (IsSelectedTab(tabControlSource, "tabFiles"))
            {
                EF_Edit_Clicked();
            }
            else if (IsSelectedTab(tabControlSource, "tabPowerpoint"))
            {
                PP_Edit_Clicked();
            }
        }

        private void Menu_CopySong_Click(object sender, EventArgs e)
        {
            if (IsSelectedTab(tabControlSource, "tabFolders"))
            {
                Copy_Song();
            }
            else if (IsSelectedTab(tabControlSource, "tabFiles"))
            {
                EF_Copy_Clicked();
            }
            else if (IsSelectedTab(tabControlSource, "tabPowerpoint"))
            {
                PP_Copy_Clicked();
            }
        }

        private void Menu_MoveSong_Click(object sender, EventArgs e)
        {
            if (IsSelectedTab(tabControlSource, "tabFolders"))
            {
                Move_Song();
            }
            else if (IsSelectedTab(tabControlSource, "tabFiles"))
            {
                EF_Move_Clicked();
            }
            else if (IsSelectedTab(tabControlSource, "tabPowerpoint"))
            {
                PP_Move_Clicked();
            }
        }

        private void Menu_DeleteSong_Click(object sender, EventArgs e)
        {
            if (IsSelectedTab(tabControlSource, "tabFolders"))
            {
                RemoveSongsListSong();
            }
            else if (IsSelectedTab(tabControlSource, "tabFiles"))
            {
                EF_Delete_Clicked();
            }
            else if (IsSelectedTab(tabControlSource, "tabPowerpoint"))
            {
                PP_Delete_Clicked();
            }
        }

        private void Copy_Song()
        {
            Gf.SelectedItemsCount = SongsList.SelectedItems.Count;
            if (SongsList.SelectedItems.Count == 0 || Gf.SelectedItemsCount == 0)
            {
                return;
            }
            FrmCopy frmCopy = new FrmCopy();
            if (frmCopy.ShowDialog() != DialogResult.OK)
            {
                return;
            }
            int newSongId = 0;
            int[] RefileSongs = new int[30000];
            RefileSongs[0] = 0;
            string[] copiedFiles = new string[30000];
            copiedFiles[0] = "0";
            string[] inHeaderData = new string[255];
            int infoScreenGroupIndex = 0;
            string infoScreenDir = "";
            if (Gf.CopyToFolder < 0)
            {
                infoScreenGroupIndex = -Gf.CopyToFolder - 1;
                infoScreenDir = Gf.InfoScreenGroups[infoScreenGroupIndex].Path;
            }
            for (int i = 0; i <= SongsList.Items.Count - 1; i++)
            {
                if (SongsList.Items[i].Selected)
                {
                    try
                    {
                        string idString = SongsList.Items[i].SubItems[1].Text;
                        string songId = DataUtil.Right(idString, idString.Length - 1);
                        string fullSearchString = "select * from SONG where songid=" + songId;

                        using DataTable datatable = DbController.GetDataTable(Gf.ConnectStringMainDB, fullSearchString);

                        {
                            if (datatable.Rows.Count > 0)
                            {
                                DataRow dr = datatable.Rows[0];
                                if (Gf.CopyToFolder > 0)
                                {
                                    newSongId = Gf.InsertItemIntoDatabase(Gf.ConnectStringMainDB, dr["Title_1"], dr["Title_2"], dr["song_number"], Gf.CopyToFolder, dr["Lyrics"], dr["Sequence"], dr["writer"], dr["copyright"], dr["capo"], dr["Timing"], dr["Key"], dr["msc"], dr["CATEGORY"], dr["LICENCE_ADMIN1"], dr["LICENCE_ADMIN2"], dr["BOOK_REFERENCE"], dr["USER_REFERENCE"], dr["SETTINGS"], dr["FORMATDATA"]);
                                    if (newSongId > 0)
                                    {
                                        RefileSongs[0]++;
                                        RefileSongs[RefileSongs[0]] = newSongId;
                                    }
                                }
                                else
                                {
                                    Gf.TempItem1.Title = DataUtil.ObjToString(dr["Title_1"]);
                                    Gf.TempItem1.FolderNo = DataUtil.ObjToInt(dr["FolderNo"]);
                                    Gf.TempItem1.Title2 = DataUtil.ObjToString(dr["Title_2"]);
                                    Gf.TempItem1.SongNumber = DataUtil.ObjToInt(dr["song_number"]);
                                    Gf.TempItem1.CompleteLyrics = DataUtil.ObjToString(dr["Lyrics"]);
                                    Gf.TempItem1.Notations = DataUtil.ObjToString(dr["msc"]);
                                    Gf.TempItem1.SongSequence = DataUtil.ObjToString(dr["Sequence"]);
                                    Gf.TempItem1.Writer = DataUtil.ObjToString(dr["writer"]);
                                    Gf.TempItem1.Copyright = DataUtil.ObjToString(dr["copyright"]);
                                    Gf.TempItem1.Category = "";
                                    Gf.TempItem1.Timing = DataUtil.ObjToString(dr["Timing"]);
                                    Gf.TempItem1.MusicKey = DataUtil.ObjToString(dr["Key"]);
                                    Gf.TempItem1.Capo = DataUtil.ObjToInt(dr["capo"]);
                                    Gf.TempItem1.Show_LicAdminInfo1 = DataUtil.ObjToString(dr["LICENCE_ADMIN1"]);
                                    Gf.TempItem1.Show_LicAdminInfo2 = DataUtil.ObjToString(dr["LICENCE_ADMIN2"]);
                                    Gf.TempItem1.Book_Reference = DataUtil.ObjToString(dr["BOOK_REFERENCE"]);
                                    Gf.TempItem1.User_Reference = DataUtil.ObjToString(dr["USER_REFERENCE"]);
                                    Gf.TempItem1.RotateString = "";
                                    Gf.TempItem1.Settings = DataUtil.ObjToString(dr["SETTINGS"]);
                                    string InFileName = infoScreenDir + Gf.TempItem1.Title + ".esi";
                                    int copyNo = 0;
                                    while (File.Exists(InFileName))
                                    {
                                        copyNo++;
                                        InFileName = infoScreenDir + Gf.TempItem1.Title + " - Copy (" + copyNo + ").esi";
                                    }
                                    if (Gf.SaveXMLInfoScreen(Gf.TempItem1, InFileName, inHeaderData, ReloadImageData: false, UseOriginalNotations: false))
                                    {
                                        copiedFiles[0] = Convert.ToString(DataUtil.StringToInt(copiedFiles[0]) + 1);
                                        copiedFiles[DataUtil.StringToInt(copiedFiles[0])] = Gf.GetDisplayNameOnly(ref InFileName, UpdateByRef: false);
                                    }
                                }
                            }
                        }
                    }
                    catch
                    {
                    }
                }
            }
            if (Gf.CopyToFolder > 0)
            {
                HighlightCopyMoveItems(Gf.CopyToFolder, ref RefileSongs);
            }
            else
            {
                HighlightCopyMoveExternalItems(InfoScreenList, infoScreenGroupIndex, copiedFiles, "I");
            }
        }

        private void Move_Song()
        {
            Gf.SelectedItemsCount = SongsList.SelectedItems.Count;
            if (Gf.SelectedItemsCount == 0)
            {
                return;
            }
            FrmMove frmMove = new FrmMove();
            if (frmMove.ShowDialog() != DialogResult.OK)
            {
                return;
            }
            int folderNumber = Gf.GetFolderNumber(SongFolder.Text);
            int[] RefileSongs = new int[30000];
            if (Gf.MoveToFolder > 0)
            {
                RefileSongs[0] = 0;
                if (Gf.ReFileSelectedSongs(ref SongsList, folderNumber, Gf.MoveToFolder, ref RefileSongs, UpdateModifiedDate: false) > 0)
                {
                    HighlightCopyMoveItems(Gf.MoveToFolder, ref RefileSongs);
                }
            }
        }

        private void RemoveSongsListSong()
        {
            Gf.SelectedItemsCount = SongsList.SelectedItems.Count;
            string prompt = "";
            if (Gf.SelectedItemsCount == 1)
            {
                prompt = "Are you really sure you want to delete the selected song?";
            }
            else
            {
                if (Gf.SelectedItemsCount <= 1)
                {
                    return;
                }
                prompt = "Are you really sure you want to delete the " + Convert.ToString(Gf.SelectedItemsCount) + " songs you have selected?";
            }
            if (MessageBox.Show(prompt, "Delete Selected Song(s)", MessageBoxButtons.YesNo) != DialogResult.Yes)
            {
                return;
            }
            Cursor = Cursors.WaitCursor;
            int folderNumber = Gf.GetFolderNumber(SongFolder.Text);
            int[] RefileSongs = new int[30000];
            int firstSelectedIndex = 0;
            for (int i = SongsList.Items.Count - 1; i >= 0; i--)
            {
                if (SongsList.Items[i].Selected)
                {
                    firstSelectedIndex = i;
                }
            }
            Gf.ReFileSelectedSongs(ref SongsList, folderNumber, 0, ref RefileSongs, UpdateModifiedDate: true);
            if (SongsList.Items.Count > firstSelectedIndex)
            {
                if (firstSelectedIndex >= 0)
                {
                    SongsList.Items[firstSelectedIndex].Selected = true;
                }
            }
            else if (SongsList.Items.Count > 0 && firstSelectedIndex - 1 >= 0 && SongsList.Items.Count >= firstSelectedIndex - 1)
            {
                SongsList.Items[firstSelectedIndex - 1].Selected = true;
            }
            SongsListIndexChanged();
            ShowStatusBarSummary();
            UpdateMenuEditHistory();
            Cursor = Cursors.Default;
        }

        /// <summary>
        /// Whether the named tab page is selected. Safe to call from another thread (it invokes on the UI
        /// thread when needed, which fixed a cross-thread exception).
        /// </summary>
        /// <param name="InTab">The tab control.</param>
        /// <param name="InName">The tab page name (compared case-insensitively).</param>
        /// <returns>True when that tab page is selected.</returns>
        private bool IsSelectedTab(TabControl InTab, string InName)
        {
            bool result = false;

            if (InTab.InvokeRequired)
            {
                InTab.Invoke(new MethodInvoker(delegate
                {
                    if (InTab != null && InTab.TabCount > 0 && InTab.SelectedTab.Name.ToLower() == InName.ToLower())
                    {
                        result = true;
                    }
                }));
            }
            else
            {
                if (InTab != null && InTab.TabCount > 0 && InTab.SelectedTab.Name.ToLower() == InName.ToLower())
                {
                    result = true;
                }
            }

            return result;
        }

        private int GetTabIndex(TabControl InTab, string InName)
        {
            if (InTab != null && InTab.TabCount > 0)
            {
                for (int i = 0; i < InTab.TabCount; i++)
                {
                    if (InTab.TabPages[i].Name.ToLower() == InName.ToLower())
                    {
                        return i;
                    }
                }
            }
            return 0;
        }

        private void HighlightCopyMoveItems(int InFolderNo, ref int[] RefileSongs)
        {
            if (RefileSongs[0] == 0)
            {
                return;
            }
            if (tabControlSource.SelectedTab.Name != "tabFolders")
            {
                tabControlSource.SelectedIndex = 0;
            }
            if (SongFolder.Text == Gf.Folders[InFolderNo].Name)
            {
                SongFolder_Change();
            }
            else
            {
                SongFolder.Text = Gf.Folders[InFolderNo].Name;
            }
            int firstIndex = -1;
            int remainingCount = RefileSongs[0];
            for (int i = 0; i < SongsList.Items.Count; i++)
            {
                if (CompareRefileSongsList(DataUtil.StringToInt(DataUtil.Mid(SongsList.Items[i].SubItems[1].Text, 1)), ref RefileSongs))
                {
                    SongsList.Items[i].Selected = true;
                    if (firstIndex < 0)
                    {
                        remainingCount--;
                        firstIndex = i;
                    }
                    if (remainingCount == 0)
                    {
                        break;
                    }
                }
            }
            if (firstIndex >= 0)
            {
                SongsList.EnsureVisible(firstIndex);
            }
            SongsListIndexChanged();
        }

        private bool CompareRefileSongsList(int inID, ref int[] RefileSongs)
        {
            if (RefileSongs[0] == 0)
            {
                return false;
            }
            for (int i = 1; i <= RefileSongs[0]; i++)
            {
                if (inID == RefileSongs[i])
                {
                    return true;
                }
            }
            return false;
        }

        private bool CompareRefileSongsList(string inID, ref string[] RefileSongs)
        {
            if (DataUtil.StringToInt(RefileSongs[0]) == 0)
            {
                return false;
            }
            for (int i = 1; i <= DataUtil.StringToInt(RefileSongs[0]); i++)
            {
                if (inID == RefileSongs[i])
                {
                    return true;
                }
            }
            return false;
        }

        private void HighlightCopyMoveExternalItems(ListView InListView, int SelectedFolderIndex, string[] RefileSongs, string InFileSymbol)
        {
            int remainingCount = DataUtil.StringToInt(RefileSongs[0]);
            if (remainingCount < 1)
            {
                return;
            }
            if (InFileSymbol == "I")
            {
                tabControlSource.SelectedIndex = GetTabIndex(tabControlSource, "tabFiles");
                InfoScreenFolder.SelectedIndex = SelectedFolderIndex;
                ShowInfoScreenFolderContents();
            }
            else if (InFileSymbol == "P")
            {
                tabControlSource.SelectedIndex = GetTabIndex(tabControlSource, "tabPowerpoint");
                Gf.PowerpointListingStyle = 0;
                SetPowerpointListingButton();
                PowerpointFolder.SelectedIndex = SelectedFolderIndex;
                ShowPowerpointFolderContents(ShowThumbs: false);
            }
            int firstIndex = -1;
            for (int i = 0; i < InListView.Items.Count; i++)
            {
                if (CompareRefileSongsList(InListView.Items[i].SubItems[0].Text, ref RefileSongs))
                {
                    InListView.Items[i].Selected = true;
                    if (firstIndex < 0)
                    {
                        remainingCount--;
                        firstIndex = i;
                    }
                    if (remainingCount == 0)
                    {
                        break;
                    }
                }
            }
            if (firstIndex >= 0)
            {
                InListView.EnsureVisible(firstIndex);
            }
            if (InFileSymbol == "I")
            {
                InfoScreenListIndexChanged();
            }
            else
            {
                PowerpointListIndexChanged();
            }
        }

        private void ViewOptions()
        {
            Gf.CurMainSelectedFolder = Gf.GetFolderNumber(SongFolder.Text);
            Gf.Options_MediaExtChanged = false;
            Gf.Options_MediaDirChanged = false;
            Gf.Options_FolderListChanged = false;
            Gf.Options_FolderFormatChanged = false;
            Gf.Options_BibleListChanged = false;
            Gf.Options_MaxHistoryListChanged = false;
            Gf.Options_PreviewAreaChanged = false;
            Gf.Options_DMChanged = false;
            // A modal form is not disposed when it closes: dispose it here. Nothing below reads the dialog itself
            // (its results are in the Gf.Options_* flags), so only the result needs to leave the using block.
            DialogResult optionsResult;
            using (FrmOptions frmOptions = new FrmOptions())
            {
                optionsResult = frmOptions.ShowDialog();
            }
            if (optionsResult != DialogResult.OK)
            {
                return;
            }
            if (Gf.Options_FolderListChanged || Gf.Options_MediaDirChanged)
            {
                Refresh();
                BuildFolderList();
                if (Gf.Options_MediaDirChanged)
                {
                    BuildMediaFolderList();
                }
            }
            if (Gf.Options_FolderFormatChanged && Gf.PreviewItem.Type == "D")
            {
                if (Gf.PreviewItem.Source == ItemSource.SongsList)
                {
                    SongsListIndexChanged();
                }
                else if (Gf.PreviewItem.Source == ItemSource.WorshipList)
                {
                    WorshipListIndexChanged();
                }
            }
            if (Gf.Options_BibleListChanged)
            {
                Refresh();
                Gf.LoadBibleVersions(ref TabBibleVersions);
                Gf.LoadBibleBooksList(TabBibleVersions, ref BookLookup, ShowSearchResultsLine: false, BibleText);
                Gf.Options_BibleListChanged = false;
            }
            if (Gf.Options_MaxHistoryListChanged)
            {
                Gf.Options_MaxHistoryListChanged = false;
                Gf.SaveMainEditHistoryToRegistry();
                UpdateMenuEditHistory();
                Gf.Edit_HistoryMaxChanged = true;
            }
            if (Gf.Options_PreviewAreaChanged)
            {
                PreviewNotations(Gf.PreviewArea_ShowNotations);
            }
            if (Gf.Options_DMChanged)
            {
                SetMainDefaultBackScreen();
                LoadItem(ref Gf.PreviewItem, Gf.PreviewItem.Type + Gf.PreviewItem.ItemID, Gf.PreviewItem.Format.FormatString, Gf.PreviewItem.CurSlide);
                LoadItem(ref Gf.OutputItem, Gf.OutputItem.Type + Gf.OutputItem.ItemID, Gf.OutputItem.Format.FormatString, Gf.OutputItem.CurSlide);
            }
            //if (Gf.ShowRunning && Gf.ShowLiveCam)
            //{
            //    RemoteControlLiveShow(LiveShowAction.Remote_LiveCamUpdate);
            //}
            SetTabsVisibility();
            SetJumpToolTips();
            ShowStatusBarSummary();
        }

        private void SetJumpToolTips()
        {
            int labelLength = 15;
            string folderPrefix = "FOLDER ";
            string folderWord = "FOLDER";
            string toolTipA = Gf.Folders[Gf.JumpToA]?.Name ?? "";
            string toolTipB = Gf.Folders[Gf.JumpToB]?.Name ?? "";
            string toolTipC = Gf.Folders[Gf.JumpToC]?.Name ?? "";
            string labelA = DataUtil.Trim(DataUtil.Left(toolTipA, labelLength));
            string labelB = DataUtil.Trim(DataUtil.Left(toolTipB, labelLength));
            string labelC = DataUtil.Trim(DataUtil.Left(toolTipC, labelLength));
            if (toolTipA.StartsWith(folderPrefix, StringComparison.OrdinalIgnoreCase))
            {
                labelA = DataUtil.Mid(toolTipA, folderPrefix.Length, labelLength);
            }
            if (toolTipB.StartsWith(folderPrefix, StringComparison.OrdinalIgnoreCase))
            {
                labelB = DataUtil.Mid(toolTipB, folderPrefix.Length, labelLength);
            }
            if (toolTipC.StartsWith(folderPrefix, StringComparison.OrdinalIgnoreCase))
            {
                labelC = DataUtil.Mid(toolTipC, folderPrefix.Length, labelLength);
            }
            if (toolTipA.ToUpper().IndexOf(folderWord) < 0)
            {
                toolTipA = (Gf.Folders[Gf.JumpToA]?.Name ?? "") + " Folder";
            }
            if (toolTipB.ToUpper().IndexOf(folderWord) < 0)
            {
                toolTipB = (Gf.Folders[Gf.JumpToB]?.Name ?? "") + " Folder";
            }
            if (toolTipC.ToUpper().IndexOf(folderWord) < 0)
            {
                toolTipC = (Gf.Folders[Gf.JumpToC]?.Name ?? "") + " Folder";
            }
            Main_JumpA.Text = labelA;
            Main_JumpA.ToolTipText = toolTipA;
            Main_JumpB.Text = labelB;
            Main_JumpB.ToolTipText = toolTipB;
            Main_JumpC.Text = labelC;
            Main_JumpC.ToolTipText = toolTipC;
        }

        private void ShowAlertForm()
        {
            if (Gf.FormInUse("Show Alert"))
            {
                Gf.AlertRestoreWindow = true;
                return;
            }
            Gf.MessageAlertRequested = false;
            Gf.ParentalAlertRequested = false;
            Gf.LyricsAlertRequested = false;
            Gf.AlertFormOpen = true;
            try
            {
                AlertWindow.Show();
            }
            catch
            {
                try
                {
                    AlertWindow = new FrmShowAlert();
                    AlertWindow.OnMessage += AlertWindow_OnMessage;
                    AlertWindow.Show();
                }
                catch
                {
                }
            }
            TimerMessagingWindowOpen.Start();
        }

        private void Menu_Recover_Click(object sender, EventArgs e)
        {
            FrmRecoverDeleted frmRecoverDeleted = new FrmRecoverDeleted();
            if (frmRecoverDeleted.ShowDialog() == DialogResult.OK)
            {
                SongFolder_Change();
            }
        }

        private void Main_EditBtns_Click(object sender, EventArgs e)
        {
            ToolStripButton toolStripButton = (ToolStripButton)sender;
            string name = toolStripButton.Name;
            if (IsSelectedTab(tabControlSource, "tabFolders"))
            {
                if (name == "Main_New")
                {
                    AddNew();
                }
                else if (name == "Main_Edit")
                {
                    Edit_SongsListSong();
                }
                else if (name == "Main_Copy")
                {
                    Copy_Song();
                }
                else if (name == "Main_Move")
                {
                    Move_Song();
                }
                else if (name == "Main_Delete")
                {
                    RemoveSongsListSong();
                }
            }
            else if (IsSelectedTab(tabControlSource, "tabFiles"))
            {
                if (name == "Main_New")
                {
                    EF_New_Clicked();
                }
                else if (name == "Main_Edit")
                {
                    EF_Edit_Clicked();
                }
                else if (name == "Main_Copy")
                {
                    EF_Copy_Clicked();
                }
                else if (name == "Main_Move")
                {
                    EF_Move_Clicked();
                }
                else if (name == "Main_Delete")
                {
                    EF_Delete_Clicked();
                }
            }
            else if (IsSelectedTab(tabControlSource, "tabPowerpoint"))
            {
                if (name == "Main_Edit")
                {
                    PP_Edit_Clicked();
                }
                else if (name == "Main_Copy")
                {
                    PP_Copy_Clicked();
                }
                else if (name == "Main_Move")
                {
                    PP_Move_Clicked();
                }
                else if (name == "Main_Delete")
                {
                    PP_Delete_Clicked();
                }
            }
        }

        private void InfoScreen_EditBtns_Click(object sender, EventArgs e)
        {
            ToolStripButton toolStripButton = (ToolStripButton)sender;
            string name = toolStripButton.Name;
            if (name == "InfoScreen_New")
            {
                EF_New_Clicked();
            }
            else if (name == "InfoScreen_Edit")
            {
                EF_Edit_Clicked();
            }
            else if (name == "InfoScreen_Copy")
            {
                EF_Copy_Clicked();
            }
            else if (name == "InfoScreen_Move")
            {
                EF_Move_Clicked();
            }
            else if (name == "InfoScreen_Delete")
            {
                EF_Delete_Clicked();
            }
        }

        private void Powerpoint_EditBtns_Click(object sender, EventArgs e)
        {
            ToolStripButton toolStripButton = (ToolStripButton)sender;
            string name = toolStripButton.Name;
            if (name == "Powerpoint_Edit")
            {
                PP_Edit_Clicked();
            }
            else if (name == "Powerpoint_Copy")
            {
                PP_Copy_Clicked();
            }
            else if (name == "Powerpoint_Move")
            {
                PP_Move_Clicked();
            }
            else if (name == "Powerpoint_Delete")
            {
                PP_Delete_Clicked();
            }
        }

        private void Main_Media_Click(object sender, EventArgs e)
        {
            if (HasSelectedSongItem())
            {
                Ind_Media_Clicked();
                return;
            }
            Def_Media_Clicked();
        }

        private bool HasSelectedSongItem()
        {
            switch (tabControlSource.SelectedTab.Name)
            {
                case "tabFolders":
                    return SongsList.SelectedItems.Count == 1;
                case "tabFiles":
                    return InfoScreenList.SelectedItems.Count > 0;
                default:
                    return false;
            }
        }

        private void SongsListPlay()
        {
            string songTitle = "";
            string title = "";
            int selectedCount = 0;
            int index = 0;
            switch (tabControlSource.SelectedTab.Name)
            {
                case "tabFolders":
                    {
                        if (SongsList.Items.Count <= 0)
                        {
                            break;
                        }
                        for (int i = 0; i <= SongsList.Items.Count - 1; i++)
                        {
                            if (SongsList.Items[i].Selected)
                            {
                                selectedCount++;
                                index = i;
                                if (selectedCount > 1)
                                {
                                    i = SongsList.Items.Count - 1;
                                }
                            }
                        }
                        if (selectedCount != 1)
                        {
                            MessageBox.Show("Please select ONE item from the Songs List to play");
                            break;
                        }
                        songTitle = Gf.RemoveMusicSym(SongsList.Items[index].Text);
                        string songIdString = SongsList.Items[index].SubItems[1].Text;
                        string idValue = DataUtil.Right(songIdString, songIdString.Length - 1);
                        int songId = DataUtil.StringToInt(idValue);
                        if (DataUtil.Left(songIdString, 1) == "D")
                        {
                            title = Gf.LookupDBTitle2(songId);
                        }
                        Gf.Play_Media(songTitle, title);
                        break;
                    }
                case "tabFiles":
                    if (InfoScreenList.SelectedItems.Count > 0)
                    {
                        string idString = InfoScreenList.SelectedItems[0].SubItems[1].Text;
                        if (DataUtil.Left(idString, 1) == "I")
                        {
                            string InTitle = "";
                            Gf.LoadIndividualData(ref Gf.TempItem1, idString, "", 1, ref InTitle);
                            songTitle = Gf.TempItem1.Title;
                            title = Gf.TempItem1.Title2;
                            Gf.Play_Media(songTitle, title);
                        }
                    }
                    break;
            }
        }

        private void Main_Refresh_Click(object sender, EventArgs e)
        {
            RefreshItems();
        }

        private void Main_Options_Click(object sender, EventArgs e)
        {
            ViewOptions();
        }

        private void Main_Alerts_Click(object sender, EventArgs e)
        {
            ShowAlertForm();
        }

        private void Main_RotateStyle_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            Gf.AssignDropDownItem(ref Main_RotateStyle, e.ClickedItem.Name, Main_Rotate0, Main_Rotate1, Main_Rotate2, Main_Rotate3);
            Gf.AutoRotateStyle = DataUtil.ObjToInt(Main_RotateStyle.Tag);
        }

        private void Main_NoRotate_Click(object sender, EventArgs e)
        {
            SetRotateState(!Main_NoRotate.Checked);
        }

        internal void SetRotateState(bool RotateOn)
        {
            Main_NoRotate.Checked = !RotateOn;
            Gf.AutoRotateOn = RotateOn;
            Gf.RestartCurrentItem = false;
            if (Gf.ShowRunning)
            {
                RemoteControlLiveShow(LiveShowAction.Remote_RotateOnOffChanged);
            }
        }

        private void Main_Chinese_Click(object sender, EventArgs e)
        {
            OutputChineseSwitch();
        }

        private void Main_Find_Click(object sender, EventArgs e)
        {
            Find_Items();
        }

        private void Main_Jump_Click(object sender, EventArgs e)
        {
            ToolStripButton toolStripButton = (ToolStripButton)sender;
            string name = toolStripButton.Name;
            tabControlSource.SelectedIndex = 0;
            int folderNo = SongFolder.SelectedIndex;
            if (name == "Main_JumpA")
            {
                folderNo = Gf.JumpToA;
            }
            else if (name == "Main_JumpB")
            {
                folderNo = Gf.JumpToB;
            }
            else if (name == "Main_JumpC")
            {
                folderNo = Gf.JumpToC;
            }
            if (folderNo > 0 && folderNo < Gf.MAXSONGSFOLDERS && Gf.Folders[folderNo].Use > 0)
            {
                try
                {
                    SongFolder.Text = Gf.Folders[folderNo].Name;
                }
                catch
                {
                }
            }
        }

        private void Menu_Empty_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("This will permanently remove all the songs in the Deleted Folder - Are you really sure?", "Permanent Delete Song(s)", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                Cursor = Cursors.WaitCursor;
                if (!Gf.EmptyDelFolder())
                {
                    MessageBox.Show("Error encountered - Not all the songs were deleted. Please try again later.");
                }
                Cursor = Cursors.Default;
            }
        }

        private void Menu_Compact_Click(object sender, EventArgs e)
        {
            //Gf.CompactAllDB();
        }

        private void Menu_ClearAllFormatting_Click(object sender, EventArgs e)
        {
            if (Gf.ClearAllFormatting() && Gf.PreviewItem.Source == ItemSource.SongsList && Gf.PreviewItem.Type == "D")
            {
                SongsListIndexChanged();
            }
        }

        private void Menu_ClearRegistrySettings_Click(object sender, EventArgs e)
        {
            if (Gf.ClearRegistrySettings())
            {
                SaveToRegistryOnClosing = false;
                Close();
            }
        }

        private void Menu_Exit_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void Main_QuickFind_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Return)
            {
                return;
            }
            Main_QuickFind.Text = DataUtil.Trim(Main_QuickFind.Text);
            if (Main_QuickFind.Text == "")
            {
                return;
            }
            Cursor = Cursors.WaitCursor;
            if (IsSelectedTab(tabControlSource, "tabFolders"))
            {
                Gf.FindFolderItems = true;
                Gf.FindBibleVerses = false;
                Gf.Find_SQLString = Gf.BuildItemSearchString(Main_QuickFind.Text) + " and folderno > 0";
                Do_Find(InMusicOnly: false);
            }
            else if (IsSelectedTab(tabControlSource, "tabBibles"))
            {
                if (Gf.HB_TotalVersions < 1)
                {
                    Cursor = Cursors.Default;
                    return;
                }
                Gf.FindBibleVerses = true;
                Gf.FindFolderItems = false;
                Gf.HB_SQLString = Gf.BuildBibleSearchString(Main_QuickFind.Text, TabBibleVersions.SelectedIndex);
                BibleVerseSearch();
            }
            if (Main_QuickFind.Items.Count == 0 || Main_QuickFind.Text != Main_QuickFind.Items[0].ToString())
            {
                try
                {
                    Main_QuickFind.Items.Insert(0, Main_QuickFind.Text);
                    if (Main_QuickFind.Items.Count > 20)
                    {
                        // Keep the 20 most recent entries (indexes 0..19).
                        for (int index = Main_QuickFind.Items.Count - 1; index >= 20; index--)
                        {
                            Main_QuickFind.Items.RemoveAt(index);
                        }
                    }
                }
                catch
                {
                }
            }
            Cursor = Cursors.Default;
        }

        private void Do_Find(bool InMusicOnly)
        {
            if (Gf.FindFolderItems)
            {
                Gf.FindFolderItems = false;
                tabControlSource.SelectedIndex = GetTabIndex(tabControlSource, "tabFolders");
                ImplementFolderChange = false;
                if (SongFolder.Text != "Search Results:")
                {
                    SongFolder.Items.Add("Search Results:");
                }
                SongFolder.SelectedIndex = SongFolder.Items.Count - 1;
                ImplementFolderChange = true;
                SongsList.Items.Clear();
                SongsList.RightToLeft = RightToLeft.No;
                SongsList.RightToLeftLayout = false;
                statusStripMain.Items[0].Text = "";
                statusStripMain.Items[0].ToolTipText = "";
                searchResultsMusicOnly = InMusicOnly;
                searchResultsSql = Gf.Find_SQLString;
                FillList(-1, "", InMusicOnly);
            }
            else if (Gf.HB_SQLString != "")
            {
                BibleVerseSearch();
            }
        }

        private void BibleVerseSearch()
        {
            Cursor = Cursors.WaitCursor;
            HB_SearchInProgress = true;
            tabControlSource.SelectedIndex = GetTabIndex(tabControlSource, "tabBibles");
            TabBibleVersions.SelectedIndex = Gf.HB_CurVersionTabIndex;
            TabBibleVersionsChanged();
            BibleText.Text = "";
            HB_CurSelectedPassages = "";
            HB_CurSelectedTitle = "";
            Gf.SearchBiblePassages(TabBibleVersions.SelectedIndex, ref BookLookup, Gf.HB_SQLString, ref BibleText, Gf.HB_ShowVerses);
            Gf.HB_SQLString = "";
            Gf.HB_SequentialListing = false;
            HB_SearchInProgress = false;
            ShowStatusBarSummary();
            Cursor = Cursors.Default;
        }

        private void Main_QuickFind_Enter(object sender, EventArgs e)
        {
            if (Gf.UseFocusedTextRegionColour)
            {
                Color InBackground = Main_QuickFind.BackColor;
                Gf.SetEnterColour(ref InBackground);
                Main_QuickFind.BackColor = InBackground;
            }
            Main_QuickFind.SelectAll();
        }

        private void Main_QuickFind_Leave(object sender, EventArgs e)
        {
            Color InBackground = Main_QuickFind.BackColor;
            Gf.SetLeaveColor(ref InBackground);
            Main_QuickFind.BackColor = InBackground;
        }

        private void Find_Items()
        {
            Gf.FindFolderItems = false;
            Gf.FindBibleVerses = IsSelectedTab(tabControlSource, "tabBibles");
            if (Gf.FormInUse("Search for EasiSlides Items"))
            {
                Gf.FindItemRestoreWindow = true;
                return;
            }
            FrmFind frmFind = new FrmFind();
            Gf.FindItemsRequested = false;
            Gf.FindItemsFormOpen = true;
            frmFind.Show();
            TimerSearch.Start();
        }

        private void TimerSearch_Tick(object sender, EventArgs e)
        {
            if (Gf.FindItemsRequested)
            {
                Gf.FindItemsRequested = false;
                Do_Find(Gf.FindItemMediaOnly);
            }
            if (!Gf.FindItemsFormOpen)
            {
                TimerSearch.Stop();
            }
        }

        private void CMenuSongs_SelectAll_Click(object sender, EventArgs e)
        {
            SongsList_SelectAll();
        }

        private void CMenuSongs_UnselectAll_Click(object sender, EventArgs e)
        {
            SongsList_UnselectAll();
        }

        private void SongsList_UnselectAll()
        {
            if (SongsList.Items.Count > 0)
            {
                Cursor = Cursors.WaitCursor;
                for (int i = 0; i < SongsList.Items.Count; i++)
                {
                    SongsList.Items[i].Selected = false;
                }
                SongsListIndexChanged();
                Cursor = Cursors.Default;
            }
        }

        private void CMenuSongs_AddShow_Click(object sender, EventArgs e)
        {
            if (AddToWorshipList())
            {
                int selectedIndex = Gf.GetSelectedIndex(WorshipListItems);
                WorshipListItems.EnsureVisible(selectedIndex);
                Gf.StartPresAt = selectedIndex + 1;
                PreviewItemToLive();
            }
        }

        private void CMenuSongs_Edit_Click(object sender, EventArgs e)
        {
            Edit_SongsListSong();
        }

        private void CMenuSongs_Copy_Click(object sender, EventArgs e)
        {
            Copy_Song();
        }

        private void CMenuSongs_Refresh_Click(object sender, EventArgs e)
        {
        }

        private void CMenuImages_AddItem_Click(object sender, EventArgs e)
        {
            ApplyBackground(ThumbImageClicked, 1);
        }

        private void CMenuImages_AddDefault_Click(object sender, EventArgs e)
        {
            ApplyBackground(ThumbImageClicked, 0);
        }

        private void CMenuImages_Refresh_Click(object sender, EventArgs e)
        {
            BuildPicturesFolderList();
        }

        private void CMenuImages_Opening(object sender, CancelEventArgs e)
        {
            CMenuImages_AddItem.Enabled = ((Gf.PreviewItem.ItemID != "") ? true : false);
        }

        private void CMenuWorship_SelectAll_Click(object sender, EventArgs e)
        {
            WorshipList_SelectAll();
        }

        private void CMenuWorship_UnselectAll_Click(object sender, EventArgs e)
        {
            WorshipList_UnselectAll();
        }

        private void CMenuWorship_Clear_Click(object sender, EventArgs e)
        {
            if (WorshipListItems.Items.Count == 0)
            {
                MessageBox.Show("Worship list is already empty.");
                return;
            }

            string currentSession = Gf.CurSession;
            if (string.IsNullOrEmpty(currentSession))
            {
                MessageBox.Show("No worship list is currently loaded.");
                return;
            }

            DialogResult result = MessageBox.Show(
                "Really clear the worship list '" + currentSession + "'?\n\nThe current worship list will be moved to trash.",
                "Clear Worship List",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                try
                {
                    string sourceFile = Gf.WorshipDir + currentSession + ".esw";
                    string trashDir = Gf.WorshipDir + "Trash\\";

                    if (!Directory.Exists(trashDir))
                    {
                        Directory.CreateDirectory(trashDir);
                    }

                    string destFile = trashDir + currentSession + ".esw";

                    if (File.Exists(destFile))
                    {
                        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                        destFile = trashDir + currentSession + "_" + timestamp + ".esw";
                    }

                    if (File.Exists(sourceFile))
                    {
                        File.Move(sourceFile, destFile);
                    }

                    WorshipListItems.Items.Clear();
                    WorshipListIndexChanged();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error moving worship list to trash: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void CMenuWorship_Edit_Click(object sender, EventArgs e)
        {
            Edit_WorshipListSong();
        }

        private void Edit_WorshipListSong()
        {
            if (WorshipListItems.Items.Count == 0)
            {
                return;
            }
            if (WorshipListItems.SelectedItems.Count != 1)
            {
                MessageBox.Show("Please select ONE item from the Worship List to edit");
                return;
            }
            string OutString = "Worship List";
            int selectedIndex = Gf.GetSelectedIndex(WorshipListItems, ref OutString);
            if (selectedIndex < 0)
            {
                return;
            }
            string idString = WorshipListItems.Items[selectedIndex].SubItems[1].Text;
            if (DataUtil.Left(idString, 1) == "B")
            {
                string InTitle = WorshipListItems.Items[selectedIndex].SubItems[0].Text;
                string editedIdString = Edit_Item(idString, ref InTitle);
                WorshipListItems.Items[selectedIndex].SubItems[0].Text = InTitle;
                if (editedIdString != "")
                {
                    WorshipListItems.Items[selectedIndex].SubItems[1].Text = editedIdString;
                    WorshipListIndexChanged();
                }
            }
            else
            {
                Edit_Item(idString);
            }
        }

        private void CMenuWorship_Play_Click(object sender, EventArgs e)
        {
            WorshipListPlay();
        }

        private void WorshipListPlay()
        {
            string title = "";
            int selectedCount = 0;
            int index = 0;
            if (WorshipListItems.Items.Count <= 0)
            {
                return;
            }
            for (int i = 0; i <= WorshipListItems.Items.Count - 1; i++)
            {
                if (WorshipListItems.Items[i].Selected)
                {
                    selectedCount++;
                    index = i;
                    if (selectedCount > 1)
                    {
                        i = WorshipListItems.Items.Count - 1;
                    }
                }
            }
            if (selectedCount != 1)
            {
                MessageBox.Show("Please select ONE item from the Worship List to play");
                return;
            }
            string songTitle = Gf.RemoveMusicSym(WorshipListItems.Items[index].Text);
            string idString = WorshipListItems.Items[index].SubItems[1].Text;
            string itemType = DataUtil.Left(idString, 1);
            string idValue = DataUtil.Right(idString, idString.Length - 1);
            if (itemType == "M")
            {
                if (string.IsNullOrWhiteSpace(idValue) || !File.Exists(idValue))
                {
                    MessageBox.Show("Sorry, cannot find the media file '" + idValue + "'.");
                    return;
                }
                if (!Gf.RunProcess(idValue))
                {
                    MessageBox.Show("Sorry, cannot open the media file '" + idValue + "'.");
                }
                return;
            }
            int songId = DataUtil.StringToInt(idValue);
            if (itemType == "D")
            {
                title = Gf.LookupDBTitle2(songId);
            }
            Gf.Play_Media(songTitle, title);
        }

        private void CMenuWorship_PlayOnOutput_Click(object sender, EventArgs e)
        {
            WorshipListPlayOnOutputMonitor();
        }

        private void WorshipListPlayOnOutputMonitor()
        {
            string title = "";
            int selectedCount = 0;
            int index = 0;

            if (WorshipListItems.Items.Count <= 0)
            {
                return;
            }

            // Find the selected item
            for (int i = 0; i <= WorshipListItems.Items.Count - 1; i++)
            {
                if (WorshipListItems.Items[i].Selected)
                {
                    selectedCount++;
                    index = i;
                    if (selectedCount > 1)
                    {
                        i = WorshipListItems.Items.Count - 1;
                    }
                }
            }

            if (selectedCount != 1)
            {
                MessageBox.Show("Please select ONE item from the Worship List to play");
                return;
            }

            string songTitle = Gf.RemoveMusicSym(WorshipListItems.Items[index].Text);
            string idString = WorshipListItems.Items[index].SubItems[1].Text;
            string itemType = DataUtil.Left(idString, 1);
            string idValue = DataUtil.Right(idString, idString.Length - 1);

            if (itemType == "M")
            {
                if (string.IsNullOrWhiteSpace(idValue) || !File.Exists(idValue))
                {
                    MessageBox.Show("Sorry, cannot find the media file '" + idValue + "'.");
                    return;
                }

                // Play on the output monitor
                PlayMediaOnOutputMonitor(idValue);
                return;
            }

            int songId = DataUtil.StringToInt(idValue);
            if (itemType == "D")
            {
                title = Gf.LookupDBTitle2(songId);
            }

            // Look up the media file, then play it
            string mediaFile = Gf.GetMediaFileName(songTitle, title);
            if (!string.IsNullOrEmpty(mediaFile) && File.Exists(mediaFile))
            {
                PlayMediaOnOutputMonitor(mediaFile);
            }
            else
            {
                MessageBox.Show("Sorry, cannot find any media file for '" + songTitle + "'");
            }
        }

        private FrmLaunchMediaPlayer _mediaPlayerWindow = null;
        private bool _isMediaPlayerHookActive = false;
        private KeyboardActionHandler _keyboardHandler;

        private void PlayMediaOnOutputMonitor(string mediaFilePath)
        {
            // Check the output monitor setting
            string outputMonitorName = Gf.OutputMonitorName;

            if (string.IsNullOrEmpty(outputMonitorName) || outputMonitorName == "None")
            {
                MessageBox.Show("Please configure Output Monitor in Options first.");
                return;
            }

            try
            {
                // Create the FrmLaunchMediaPlayer instance, or reuse it
                if (_mediaPlayerWindow == null || _mediaPlayerWindow.IsDisposed)
                {
                    _mediaPlayerWindow = new FrmLaunchMediaPlayer();

                    // Double click closes it
                    _mediaPlayerWindow.DoubleClick += (s, e) =>
                    {
                        StopMediaPlayer();
                    };

                    // Remove the global keyboard hook when the window closes
                    _mediaPlayerWindow.FormClosed += (s, e) =>
                    {
                        RemoveHookMediaPlayer();
                    };
                }

                // Start the global keyboard hook for the media player
                AddHookMediaPlayer();

                // Apply the output monitor
                _mediaPlayerWindow.ApplyOutputMonitor(outputMonitorName);

                // Set the media file path
                Gf.LiveItem.Format.MediaOption = 2; // 2 = path given directly
                Gf.LiveItem.Format.MediaLocation = mediaFilePath;
                Gf.LiveItem.Format.MediaVolume = 100;
                Gf.LiveItem.Format.MediaMute = 0;
                Gf.LiveItem.Format.MediaWidescreen = 0;
                Gf.LiveItem.Format.MediaRepeat = 0;
                Gf.LiveItem.RotateTotal = 0;
                Gf.LiveItem.Type = "M"; // M = media type

                // Show the media player window
                if (!_mediaPlayerWindow.Visible)
                {
                    _mediaPlayerWindow.Show();
                }

                // Bring the window to the top (Win32 API)
                IntPtr handle = _mediaPlayerWindow.Handle;

                // Make it topmost
                SetWindowPos(handle, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW);

                // Bring it to the foreground
                SetForegroundWindow(handle);
                _mediaPlayerWindow.BringToFront();
                _mediaPlayerWindow.Activate();

                // Load and play in one go. Loading paused and then calling
                // Remote_ResumeItemFromStart tore the graph down and rebuilt it,
                // which blanked the output once before the video started.
                int result = _mediaPlayerWindow.Remote_PlayItem();
                if (result == 0)
                {
                    MessageBox.Show("Failed to load media file: " + mediaFilePath + "\n\nPlease check if DirectShow is installed and the file format is supported.");
                }
                else
                {
                    // Keep it on top after playback starts
                    SetWindowPos(handle, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW);
                    SetForegroundWindow(handle);
                    _mediaPlayerWindow.BringToFront();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error playing media: " + ex.Message);
            }
        }

        // Closes the media player window
        public void CloseMediaPlayer()
        {
            if (_mediaPlayerWindow != null && !_mediaPlayerWindow.IsDisposed)
            {
                _mediaPlayerWindow.Remote_StopItem();
                _mediaPlayerWindow.Hide();
            }
        }

        /// <summary>
        /// Adds the global keyboard hook for the media player.
        /// </summary>
        public void AddHookMediaPlayer()
        {
            try
            {
                if (!_isMediaPlayerHookActive)
                {
                    HookManager.KeyDown += HookManager_MediaPlayer_KeyDown;
                    _isMediaPlayerHookActive = true;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to add media player hook: {ex.Message}");
            }
        }

        /// <summary>
        /// Removes the global keyboard hook for the media player.
        /// </summary>
        public void RemoveHookMediaPlayer()
        {
            try
            {
                if (_isMediaPlayerHookActive)
                {
                    HookManager.KeyDown -= HookManager_MediaPlayer_KeyDown;
                    _isMediaPlayerHookActive = false;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to remove media player hook: {ex.Message}");
            }
        }

        /// <summary>
        /// Global keyboard event handler used only by the media player.
        /// </summary>
        private void HookManager_MediaPlayer_KeyDown(object sender, KeyEventArgs e)
        {
            // Acts only while the media player is shown
            if (_mediaPlayerWindow == null || _mediaPlayerWindow.IsDisposed || !_mediaPlayerWindow.Visible)
            {
                return;
            }

            // Make sure it runs on the UI thread
            frmMain.BeginInvoke(new Action(() =>
            {
                HandleMediaPlayerKey(e.KeyCode);
            }));
        }

        /// <summary>
        /// Handles a key for the media player.
        /// </summary>
        private void HandleMediaPlayerKey(Keys key)
        {
            if (_mediaPlayerWindow == null || _mediaPlayerWindow.IsDisposed)
                return;

            switch (key)
            {
                case Keys.Escape:
                    // ESC: stop the media and hide the window
                    StopMediaPlayer();
                    break;

                case Keys.Space:
                    // Space: toggle pause/play
                    _mediaPlayerWindow.Remote_PausePlayItem();
                    break;

                case Keys.Enter:
                    // Enter: restart playback
                    _mediaPlayerWindow.Remote_ResumeItemFromStart();
                    break;

                case Keys.S:
                    // S: stop
                    _mediaPlayerWindow.Remote_StopItem();
                    break;

                case Keys.M:
                    // M: toggle mute
                    ToggleMediaMute();
                    break;
            }
        }

        /// <summary>
        /// Stops the media player.
        /// </summary>
        private void StopMediaPlayer()
        {
            if (_mediaPlayerWindow != null && !_mediaPlayerWindow.IsDisposed)
            {
                _mediaPlayerWindow.Remote_StopItem();
                _mediaPlayerWindow.Hide();
                RemoveHookMediaPlayer();
            }
        }

        /// <summary>
        /// Toggles media mute.
        /// </summary>
        private void ToggleMediaMute()
        {
            if (Gf.LiveItem != null && Gf.LiveItem.Format != null)
            {
                Gf.LiveItem.Format.MediaMute = Gf.LiveItem.Format.MediaMute == 0 ? 1 : 0;
                // To apply the mute state, call Remote_LoadItem again if needed
            }
        }

        private void CMenuWorship_AddUsages_Click(object sender, EventArgs e)
        {
            AddToUsages();
        }

        private void CMenuPraiseB_SelectAll_Click(object sender, EventArgs e)
        {
            PraiseBookList_SelectAll();
        }

        private void CMenuPraiseB_UnselectAll_Click(object sender, EventArgs e)
        {
            PraiseBookList_UnselectAll();
        }

        private void CMenuPraiseB_Clear_Click(object sender, EventArgs e)
        {
            PraiseBookItems.Items.Clear();
            PraiseBookListIndexChanged();
            SavePraiseBook();
        }

        private void CMenuPraiseB_Edit_Click(object sender, EventArgs e)
        {
            Edit_PraiseBookSong();
        }

        private void Menu_AddToUsages_Click(object sender, EventArgs e)
        {
            AddToUsages();
        }

        private void Menu_ViewUsages_Click(object sender, EventArgs e)
        {
            FrmUsages frmUsages = new FrmUsages();
            frmUsages.ShowDialog();
        }

        private void AddToUsages()
        {
            if (WorshipListItems.Items.Count == 0)
            {
                MessageBox.Show("There are no items on the current Worship List");
                return;
            }
            int addedCount = 0;
            bool insertAttempted = false;
            int itemNo = 0;
            if (!Gf.ValidateDB(DatabaseType.Usages))
            {
                return;
            }
            try
            {
                using DbConnection connection = DbController.GetDbConnection(Gf.ConnectStringUsageDB);
               string sql = "";
                for (int i = 0; i < WorshipListItems.Items.Count; i++)
                {
                    if (DataUtil.Left(WorshipListItems.Items[i].SubItems[1].Text, 1) == "D")
                    {
                        itemNo = i + 1;
                        sql = "Insert into [USAGE] (WORSHIP_DATE,WORSHIP_LIST,SONG_TITLE,SONG_NUMBER,SONG_ID,ADMIN_1,ADMIN_2) Values (?,?,?,?,?,?,?)";
                        using DbCommand command = new DbCommand(sql, connection);
                        insertAttempted = true;
                        command.CommandText = sql;
                        command.Parameters.AddWithValue("@WORSHIP_DATE", DateTime.Now.Date);
                        command.Parameters.AddWithValue("@WORSHIP_LIST", DataUtil.Left(SessionList.Text, 50));
                        command.Parameters.AddWithValue("@SONG_TITLE", Gf.RemoveMusicSym(DataUtil.Left(WorshipListItems.Items[i].SubItems[0].Text, 100)));
                        command.Parameters.AddWithValue("@SONG_NUMBER", DataUtil.StringToInt(WorshipListItems.Items[i].SubItems[3].Text));
                        command.Parameters.AddWithValue("@SONG_ID", DataUtil.Mid(WorshipListItems.Items[i].SubItems[1].Text, 2));
                        command.Parameters.AddWithValue("@ADMIN_1", DataUtil.Left(WorshipListItems.Items[i].SubItems[4].Text, 50));
                        command.Parameters.AddWithValue("@ADMIN_2", DataUtil.Left(WorshipListItems.Items[i].SubItems[5].Text, 50));
                        command.ExecuteNonQuery();
                        addedCount++;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.ToString());

                insertAttempted = true;
                if (itemNo < 1)
                {
                    MessageBox.Show("Error writing usages details to the Usages Database. Please re-start EasiSlides and try again.");
                }
                else
                {
                    MessageBox.Show("Error writing Item " + itemNo + " to Usages Database.");
                }
            }
            switch (addedCount)
            {
                case 0:
                    if (!insertAttempted)
                    {
                        MessageBox.Show("There are no Database Items on the Worship List!");
                    }
                    break;
                case 1:
                    MessageBox.Show("Done. Detail of " + Convert.ToString(addedCount) + " Database Item was added to Usages record.");
                    break;
                default:
                    MessageBox.Show("Done. Details of " + Convert.ToString(addedCount) + " Database Items were added to the Usages record.");
                    break;
            }
        }

        private void Menu_SmartMerge_Click(object sender, EventArgs e)
        {
            FrmSmartMerge frmSmartMerge = new FrmSmartMerge();
            frmSmartMerge.ShowDialog();
        }

        private void Menu_Contents_Click(object sender, EventArgs e)
        {
            if (!Gf.RunProcess(Gf.HelpFile_Location))
            {
                MessageBox.Show("Cannot find the Help File - please Reinstall/Repair EasiSlides to install the Help Files.");
            }
        }

        private void Menu_HelpWeb_Click(object sender, EventArgs e)
        {
            if (!Gf.RunProcess("http://www.easislides.com/help"))
            {
                MessageBox.Show("Error - Cannot access www.EasiSlides.com. Please enable your internet connection and then try again.");
            }
        }

        private void Menu_Register_Click(object sender, EventArgs e)
        {
            FrmRegister frmRegister = new FrmRegister();
            frmRegister.ShowDialog();
        }

        private void Menu_About_Click(object sender, EventArgs e)
        {
            FrmAbout frmAbout = new FrmAbout();
            frmAbout.ShowDialog();
            Text = "EasiSlides" + ((Gf.UserString != "") ? (" - " + Gf.UserString) : "");
        }

        private void AddNew()
        {
            Gf.CurFolderName = SongFolder.Text;
            Gf.DB_CurSongID = 0;
            if (Gf.FormInUse("Edit Database Item"))
            {
                Gf.Edit_RequestReceived = true;
            }
            else
            {
                ShowEditItemForm();
            }
        }

        private void Edit_SongsListSong()
        {
            if (SongsList.Items.Count == 0)
            {
                return;
            }
            if (SongsList.SelectedItems.Count != 1)
            {
                MessageBox.Show("Please select ONE item from the Songs List to edit.");
                return;
            }
            string OutString = "Songs List";
            int selectedIndex = Gf.GetSelectedIndex(SongsList, ref OutString);
            if (selectedIndex >= 0)
            {
                string idString = SongsList.Items[selectedIndex].SubItems[1].Text;
                Edit_Item(idString);
            }
        }

        private string Edit_Item(string InIDString)
        {
            string InTitle = "";
            return Edit_Item(InIDString, ref InTitle);
        }

        private string Edit_Item(string InIDString, ref string InTitle)
        {
            Cursor = Cursors.WaitCursor;
            string result = UseCorrectEditor(InIDString, ref InTitle);
            Cursor = Cursors.Default;
            return result;
        }

        /// <summary>
        /// Opens the right editor for the item (database song, PowerPoint, Bible passage, text, InfoScreen,
        /// Word document or media).
        /// </summary>
        /// <param name="InIDString">Item ID: the type symbol followed by the song ID or the file path.</param>
        /// <param name="InTitle">The item title; updated when a Bible passage is edited.</param>
        /// <returns>The edited ID string of a Bible passage; otherwise an empty string.</returns>
        private string UseCorrectEditor(string InIDString, ref string InTitle)
        {
            AddToEditHistory(InIDString);
            string typeSymbol = DataUtil.Left(InIDString, 1);
            string idValue = DataUtil.Right(InIDString, InIDString.Length - 1);
            string strExt = Path.GetExtension(idValue).ToLower();
            if (typeSymbol == "D")
            {
                Gf.DB_CurSongID = DataUtil.StringToInt(idValue);
                if (Gf.ValidSongID(Gf.DB_CurSongID))
                {
                    if (Gf.FormInUse("Edit Database Item"))
                    {
                        Gf.Edit_RequestReceived = true;
                    }
                    else
                    {
                        ShowEditItemForm();
                    }
                }
                return "";
            }
            if (typeSymbol == "P")
            {
                if (File.Exists(idValue))
                {
                    if (!Gf.RunProcess(idValue))
                    {
                        MessageBox.Show("Cannot edit Powerpoint document!  It appears Powerpoint is not installed on this machine.");
                    }
                }
                else
                {
                    MessageBox.Show("The Powerpoint File is missing!");
                }
                return "";
            }
            if (typeSymbol == "B")
            {
                Gf.EditBible_IDString = InIDString;
                Gf.EditBible_Title = InTitle;
                FrmEditBibleItem frmEditBibleItem = new FrmEditBibleItem();
                if (frmEditBibleItem.ShowDialog() == DialogResult.OK)
                {
                    InTitle = Gf.EditBible_Title;
                }
                return Gf.EditBible_IDString;
            }
            if (typeSymbol == "T")
            {
                if (strExt != ".txt")
                {
                    return "";
                }
                if (File.Exists(idValue))
                {
                    if (!Gf.RunProcess(idValue))
                    {
                        MessageBox.Show("Cannot edit text file! Please set a default text editor and try again.");
                    }
                }
                else
                {
                    MessageBox.Show("The Text File is missing!");
                }
            }
            else if (typeSymbol == "I")
            {
                if (strExt != ".esi")
                {
                    return "";
                }
                ShowInfoScreen(idValue);
            }
            else if (typeSymbol == "W")
            {
                if (strExt != ".doc" && strExt != ".docx")
                {
                    return "";
                }
                if (File.Exists(idValue))
                {
                    if (!Gf.RunProcess(idValue))
                    {
                        MessageBox.Show("Cannot edit Word document!  It appears Microsoft Word is not installed on this machine.");
                    }
                }
                else
                {
                    MessageBox.Show("The Word Document is missing!");
                }
            }
            else if (typeSymbol == "M")
            {
                Ind_checkBox.Checked = true;
                Ind_checkBox_Action();
                Ind_Media_Clicked();
            }
            return "";
        }

        private void ShowEditItemForm()
        {
            FrmEditItem frmEditItem = new FrmEditItem();
            Gf.EditorFormOpen = true;
            TimerMessagingWindowOpen.Start();
            frmEditItem.Show();
        }

        private int ValidatePraiseBookItems()
        {
            for (int i = 0; i < PraiseBookItems.Items.Count; i++)
            {
                Gf.DocumentSongs[i + 1].IdString = PraiseBookItems.Items[i].SubItems[3].Text;
                Gf.DocumentSongs[i + 1].Type = "D";
                Gf.DocumentSongs[i + 1].Title = Gf.RemoveMusicSym(DataUtil.Trim(PraiseBookItems.Items[i].SubItems[2].Text));
                Gf.DocumentSongs[i + 1].IndexGroup = PraiseBookItems.Items[i].SubItems[4].Text;
                Gf.DocumentSongs[i + 1].NumberAndInfo = (Gf.UseSongNumbers ? PraiseBookItems.Items[i].SubItems[5].Text : "") + '>';
                Gf.TotalPraiseBookItems = PraiseBookItems.Items.Count;
            }
            return -1;
        }

        private void CMenuBible_SelectAll_Click(object sender, EventArgs e)
        {
            HB_SelectAll();
        }

        private void CMenuBible_UnselectAll_Click(object sender, EventArgs e)
        {
            HB_UnselectAll();
        }

        private void CMenuBible_AddShow_Click(object sender, EventArgs e)
        {
            if (HB_StartBuildStringProcess() && AddToWorshipList())
            {
                int selectedIndex = Gf.GetSelectedIndex(WorshipListItems);
                WorshipListItems.EnsureVisible(selectedIndex);
                Gf.StartPresAt = selectedIndex + 1;
                PreviewItemToLive();
            }
        }

        private void BibleR2MenuClickHandler(object sender, EventArgs e)
        {
            ToolStripMenuItem toolStripMenuItem = (ToolStripMenuItem)sender;
            AddRegion2ToBiblePassage(DataUtil.ObjToInt(toolStripMenuItem.Tag));
        }

        private void AddRegion2ToBiblePassage(int InVersionNo)
        {
            string arg = DataUtil.ExtractOneInfo(ref HB_CurSelectedPassages, ';') + ';';
            arg = arg + DataUtil.ExtractOneInfo(ref HB_CurSelectedPassages, ';') + ';';
            DataUtil.ExtractOneInfo(ref HB_CurSelectedPassages, ';');
            arg = arg + Gf.GetDisplayNameOnly(ref Gf.HB_Versions[InVersionNo, 4], UpdateByRef: false, KeepExt: true) + ';';
            HB_CurSelectedPassages = arg + HB_CurSelectedPassages;
            HB_SelectedPassagesChanged(HB_CurSelectedPassages, ref HB_CurSelectedTitle);
            HB_CurSelectedFormat = Gf.PreviewItem.Format.FormatString;
            BibleText.Focus();
        }

        private void CMenuBible_Copy_Click(object sender, EventArgs e)
        {
            HB_CopyText();
        }

        private void CMenuBible_CopyInfoScreen_Click(object sender, EventArgs e)
        {
            if (BibleText.Text != "")
            {
                string lyrics = Gf.PreviewItem.CompleteLyrics.Replace('\u0098'.ToString(), "");
                if (lyrics != "")
                {
                    Clipboard.SetDataObject(lyrics);
                    string infoScreenId = "";
                    try
                    {
                        infoScreenId = "*NEW*" + '>' + Gf.HB_Versions[TabBibleVersions.SelectedIndex, 5] + '>' + Gf.PreviewItem.Title;
                    }
                    catch
                    {
                        infoScreenId = "*NEW*" + '>' + Gf.PreviewItem.FolderNo.ToString() + '>' + Gf.PreviewItem.Title;
                    }
                    ShowInfoScreen(infoScreenId);
                }
            }
        }

        private void HB_SelectAll()
        {
            if (BibleText.Text != "")
            {
                BibleText.SelectionStart = 0;
                BibleText.SelectionLength = BibleText.Text.Length;
                HB_StartBuildStringProcess();
            }
        }

        private void HB_UnselectAll()
        {
            BibleText.SelectionLength = 0;
            HB_CurSelectedPassages = "";
            HB_CurSelectedTitle = "";
            HB_SelectedPassagesChanged(HB_CurSelectedPassages, ref HB_CurSelectedTitle);
        }

        private void HB_CopyText()
        {
            if (BibleText.Text != "")
            {
                Clipboard.SetDataObject(BibleText.SelectedText);
            }
        }

        private void ShowInfoScreen(string TitleString)
        {
            if (Gf.FormInUse("InfoScreen"))
            {
                Gf.InfoScreenFileName = TitleString;
                Gf.InfoScreen_RequestReceived = true;
                return;
            }
            FrmInfoScreen frmInfoScreen = new FrmInfoScreen();
            Gf.InfoScreen_RequestReceived = false;
            Gf.InfoScreenFormOpen = true;
            Gf.InfoScreenAction = InfoType.NoAction;
            Gf.InfoScreenFileName = TitleString;
            Gf.InfoScreenBackgroundPicture = "";
            Gf.InfoScreenLoadNewBackground = false;
            frmInfoScreen.Show();
            TimerMessagingWindowOpen.Start();
        }

        private void TimerMessagingWindowOpen_Tick(object sender, EventArgs e)
        {
            if (Gf.InfoScreenFormOpen || Gf.InfoScreenLoadItem || Gf.EditorFormOpen)
            {
                if (Gf.InfoScreenLoadItem)
                {
                    Gf.InfoScreenLoadItem = false;
                    AddToEditHistory("I" + Gf.InfoScreenFileName);
                    if (Gf.InfoScreenAction == InfoType.Save)
                    {
                        Gf.InfoScreenAction = InfoType.NoAction;
                        if (Gf.PreviewItem.Source == ItemSource.WorshipList && Gf.PreviewItem.ItemID == Gf.InfoScreenFileName)
                        {
                            WorshipListIndexChanged();
                        }
                        else
                        {
                            if (!IsSelectedTab(tabControlSource, "tabFiles"))
                            {
                                tabControlSource.SelectedIndex = GetTabIndex(tabControlSource, "tabFiles");
                            }
                            HighlightNewInfoScreenItem();
                        }
                    }
                    Gf.InfoScreenItemNew = false;
                }
                if (!Gf.EditorLoadItem)
                {
                    return;
                }
                Gf.EditorLoadItem = false;
                AddToEditHistory("D" + Gf.EditorItemID);
                if (Gf.EditorItemNew || Gf.EditorItemFolderChanged || Gf.EditorItemTitleChanged)
                {
                    HighlightNewFolderItem();
                }
                if (Gf.PreviewItem.ItemID == Gf.EditorItemID.ToString())
                {
                    if (Gf.PreviewItem.Source == ItemSource.SongsList)
                    {
                        SongsListIndexChanged();
                    }
                    else if (Gf.PreviewItem.Source == ItemSource.WorshipList)
                    {
                        WorshipListIndexChanged();
                    }
                    else if (Gf.PreviewItem.Source == ItemSource.PraiseBook)
                    {
                        PraiseBookListIndexChanged();
                    }
                }
            }
            else
            {
                TimerMessagingWindowOpen.Stop();
            }
        }

        private void HighlightNewInfoScreenItem()
        {
            BuildInfoScreenFolderList();
            int groupIndex = -1;
            string folderPath = Path.GetDirectoryName(Gf.InfoScreenFileName) + "\\";
            for (int i = 0; i < Gf.InfoScreenGroups.Count; i++)
            {
                if (folderPath == Gf.InfoScreenGroups[i].Path)
                {
                    groupIndex = i;
                    i = Gf.InfoScreenGroups.Count;
                }
            }
            if (groupIndex >= 0)
            {
                InfoScreenFolder.SelectedIndex = groupIndex;
                string displayNameOnly = Gf.GetDisplayNameOnly(ref Gf.InfoScreenFileName, UpdateByRef: false);
                try
                {
                    for (int i = 0; i < InfoScreenList.Items.Count; i++)
                    {
                        if (InfoScreenList.Items[i].SubItems[0].Text == displayNameOnly)
                        {
                            InfoScreenList.Items[i].Selected = true;
                            InfoScreenListIndexChanged();
                            InfoScreenList.EnsureVisible(i);
                            i = InfoScreenList.Items.Count;
                        }
                    }
                }
                catch
                {
                }
            }
            Gf.InfoScreenItemNew = false;
            Gf.InfoScreenLoadItem = false;
        }

        private void HighlightNewFolderItem()
        {
            if (Gf.EditorItemNew || Gf.EditorItemFolderChanged || SongFolder.Text != Gf.Folders[Gf.EditorItemNewFolder].Name)
            {
                ImplementFolderChange = true;
                if (SongFolder.Text == Gf.Folders[Gf.EditorItemNewFolder].Name)
                {
                    SongFolder_Change();
                }
                else
                {
                    SongFolder.Text = Gf.Folders[Gf.EditorItemNewFolder].Name;
                }
                Gf.EditorItemTitleChanged = false;
            }
            for (int i = 0; i < SongsList.Items.Count; i++)
            {
                if (DataUtil.Mid(SongsList.Items[i].SubItems[1].Text, 1) == Gf.EditorItemID.ToString())
                {
                    if (Gf.EditorItemTitleChanged)
                    {
                        SongsList.Items[i].SubItems[0].Text = Gf.EditorItemTitle;
                    }
                    SongsList.Items[i].Selected = true;
                    SongsList.EnsureVisible(i);
                    break;
                }
            }
            // A title rewritten in place under a find-box filter: the filter is applied again (design D26).
            ReapplySongFindAfterEdit();
            if (Gf.EditorItemTitleChanged)
            {
                for (int i = 0; i < WorshipListItems.Items.Count; i++)
                {
                    if (DataUtil.Mid(WorshipListItems.Items[i].SubItems[1].Text, 2) == Gf.EditorItemID.ToString())
                    {
                        WorshipListItems.Items[i].SubItems[0].Text = Gf.EditorItemTitle;
                        SaveWorshipList();
                        break;
                    }
                }
                for (int i = 0; i < PraiseBookItems.Items.Count; i++)
                {
                    if (DataUtil.Mid(PraiseBookItems.Items[i].SubItems[3].Text, 2) == Gf.EditorItemID.ToString())
                    {
                        PraiseBookList_Change();
                        break;
                    }
                }
            }
            Gf.EditorItemNew = false;
            Gf.EditorItemFolderChanged = false;
            Gf.EditorItemTitleChanged = false;
        }

        private void Ind_Head_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            Gf.AssignDropDownItem(ref Ind_Head, e.ClickedItem.Name, Ind_HeadNoTitles, Ind_HeadAllTitles, Ind_HeadFirstScreen);
            Gf.PreviewItem.Format.ShowSongHeadings = DataUtil.ObjToInt(Ind_Head.Tag);
            UpdateFormatData(StartAtFirstSlide: false);
        }

        private void Def_Head_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            Gf.AssignDropDownItem(ref Def_Head, e.ClickedItem.Name, Def_HeadNoTitles, Def_HeadAllTitles, Def_HeadFirstScreen);
            Gf.ShowSongHeadings = DataUtil.ObjToInt(Def_Head.Tag);
            ApplyDefaultData(StartAtFirstSlide: false);
        }

        private void Ind_HeadAlign_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            Gf.AssignDropDownItem(ref Ind_HeadAlign, e.ClickedItem.Name, Ind_HeadAlignAsR1, Ind_HeadAlignAsR2, Ind_HeadAlignLeft, Ind_HeadAlignCentre, Ind_HeadAlignRight);
            Gf.PreviewItem.Format.ShowSongHeadingsAlign = DataUtil.ObjToInt(Ind_HeadAlign.Tag);
            UpdateFormatData(StartAtFirstSlide: false);
        }

        private void Def_HeadAlign_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            Gf.AssignDropDownItem(ref Def_HeadAlign, e.ClickedItem.Name, Def_HeadAlignAsR1, Def_HeadAlignAsR2, Def_HeadAlignLeft, Def_HeadAlignCentre, Def_HeadAlignRight);
            Gf.ShowSongHeadingsAlign = DataUtil.ObjToInt(Def_HeadAlign.Tag);
            ApplyDefaultData(StartAtFirstSlide: false);
        }

        private bool DShowPlayerPresent()
        {
            // quartz.dll is the DirectShow runtime. The old check looked for
            // dpnhpast.dll (the legacy DirectPlay NAT helper), which is missing on
            // Windows installs without the optional DirectPlay feature and silently
            // disabled all video playback there.
            return File.Exists(Path.Combine(Environment.SystemDirectory, "quartz.dll"));
        }

        private void ApplyPreviewArea_Setup(int InMode)
        {
            try
            {
                ApplyLyricsRichTextBoxFont(InMode);
                SetPreviewAreaFont(BibleText);
                SetPreviewAreaFont(SongsList);
                SetPreviewAreaFont(InfoScreenList);
                SetPreviewAreaFont(WorshipListItems);
                SetPreviewAreaFont(PraiseBookItems);
                SetPreviewAreaFont(PreviewInfo);
            }
            catch
            {
            }
        }

        // Sets the preview-area font size on a control, keeping its font name and style.
        // The replaced font is disposed only when this form created it; designer fonts and fonts
        // inherited from a parent are never disposed.
        private void SetPreviewAreaFont(Control InControl)
        {
            Font font = new Font(InControl.Font.Name, Gf.PreviewArea_FontSize, InControl.Font.Style);
            InControl.Font = font;
            if (!ReferenceEquals(InControl.Font, font))
            {
                // The control did not keep this instance: either its current font is equal to it, or
                // (RichTextBox with a handle and text) it stores its own copy. Nothing references it.
                font.Dispose();
                return;
            }
            if (previewAreaOwnedFonts.TryGetValue(InControl, out Font previous))
            {
                previous.Dispose();
            }
            previewAreaOwnedFonts[InControl] = font;
        }

        // Returns the font shared by all lyrics boxes of one lyrics panel. A new font is created only when
        // the panel font or the preview font size changes; then every box of that panel is moved to the new
        // font, and the old font is disposed only if no box still uses it. (A RichTextBox that has a handle
        // and text stores its own copy of an assigned font, so a box may hold neither instance.)
        private Font GetLyricsFont(Panel InPanel, RichTextBox[] InRichTextBox)
        {
            bool isPreview = ReferenceEquals(InPanel, flowLayoutPreviewLyrics);
            (string Name, float Size, FontStyle Style) key = (InPanel.Font.Name, Gf.PreviewArea_FontSize, InPanel.Font.Style);
            Font current = isPreview ? previewLyricsFont : outputLyricsFont;
            if (current != null && key.Equals(isPreview ? previewLyricsFontKey : outputLyricsFontKey))
            {
                return current;
            }

            Font font = new Font(key.Name, key.Size, key.Style);
            if (isPreview)
            {
                previewLyricsFont = font;
                previewLyricsFontKey = key;
            }
            else
            {
                outputLyricsFont = font;
                outputLyricsFontKey = key;
            }

            if (current != null)
            {
                bool currentStillUsed = false;
                foreach (RichTextBox box in InRichTextBox)
                {
                    if (box == null)
                    {
                        continue;
                    }
                    box.Font = font;
                    if (ReferenceEquals(box.Font, current))
                    {
                        currentStillUsed = true;
                    }
                }
                if (!currentStillUsed)
                {
                    current.Dispose();
                }
            }
            return font;
        }

        private void ApplyLyricsRichTextBoxFont(int InMode)
        {
            if (InMode == 0 || InMode == 2)
            {
                Font previewFont = GetLyricsFont(flowLayoutPreviewLyrics, Lyrics_PreviewBox);
                for (int i = 1; i <= Gf.PreviewItem.TotalSlides; i++)
                {
                    if (Lyrics_PreviewBox[i] != null)
                    {
                        Lyrics_PreviewBox[i].Font = previewFont;
                        Lyrics_PreviewBox[i].BorderStyle = (Gf.PreviewArea_LineBetweenScreens ? BorderStyle.FixedSingle : BorderStyle.None);
                    }
                }
            }
            FormatLyricsContainers(Gf.PreviewItem);
            ResizePreviewRichTextBox();
            if (InMode != 1 && InMode != 2)
            {
                return;
            }
            Font outputFont = GetLyricsFont(flowLayoutOutputLyrics, Lyrics_OutputBox);
            for (int i = 1; i <= Gf.OutputItem.TotalSlides; i++)
            {
                if (Lyrics_OutputBox[i] != null)
                {
                    Lyrics_OutputBox[i].Font = outputFont;
                    Lyrics_OutputBox[i].BorderStyle = (Gf.PreviewArea_LineBetweenScreens ? BorderStyle.FixedSingle : BorderStyle.None);
                }
            }
            FormatLyricsContainers(Gf.OutputItem);
            ResizeOutputRichTextBox();
        }

        private void GenerateIndexReport()
        {
            if (GenerateListingFileName == "")
            {
                GenerateListingFileName = "Index Of Database Items.rtf";
            }
            if (GenerateListingDir == "")
            {
                GenerateListingDir = Gf.RootEasiSlidesDir + "Documents\\";
            }
            string fileName = SaveListingOfFolder(ref GenerateListingFileName, ref GenerateListingDir);
            if (Path.GetExtension(fileName) == ".rtf")
            {
                GenerateIndexReportRTF(fileName);
            }
            else if (Path.GetExtension(fileName) == ".txt")
            {
                GenerateIndexReportTabText(fileName);
            }
            else if (fileName != "")
            {
                MessageBox.Show("You haven't specified a supported Listing File Type.  Please retry and select a proper file type");
            }
        }

        private void GenerateIndexReportRTF(string OutputFileName)
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                using StreamWriter streamWriter = new StreamWriter(OutputFileName, append: false, FileUtil.Utf8WithBom);
                try
                {
                    streamWriter.AutoFlush = true;

                    Gf.RTFNewLine = "\\b0\\i0\\ulnone\\par ";
                    Gf.RTFIndent[0] = "\\pard\\tx1200\\tx3500\\tx8200\\tx9000 ";
                    string rtfHeader = "{\\rtf1\\ansi\\ansicpg1252\\deff0\\deflang1033{\\fonttbl{\\f0\\fnil\\fcharset0 Microsoft Sans Serif;}}\\viewkind1\\uc1\\pard\\f0\\fs24\\margr600\\margl1000\\margt900\\margb1000\\cols2\\colno1\\colw4650\\colsr750\\colno2\\colw4650 ";

                    //Recordset tableRecordSet = DbDaoController.GetTableRecordSet(Gf.ConnectStringMainDB, "SONG");
                    //tableRecordSet.Index = "PrimaryKey";

                    streamWriter.Write(rtfHeader);
                    streamWriter.Write(DataUtil.UnicodeToAscii_RTF("\\b1\\ul " + SongFolder.Text + "\\b0\\ulnone " + Gf.RTFNewLine));
                    streamWriter.Write(DataUtil.UnicodeToAscii_RTF(Gf.RTFNewLine));
                    for (int i = 0; i < SongsList.Items.Count; i++)
                    {
                        streamWriter.Write(DataUtil.UnicodeToAscii_RTF(Gf.RTFIndent[0] + Gf.RemoveMusicSym(SongsList.Items[i].SubItems[0].Text) + Gf.RTFNewLine));
                    }
                    streamWriter.Write(DataUtil.UnicodeToAscii_RTF(Gf.RTFNewLine));
                    streamWriter.Write(DataUtil.UnicodeToAscii_RTF("(Total: " + SongsList.Items.Count + " items)" + Gf.RTFNewLine));
                    streamWriter.Write("}");
                    //streamWriter.Flush();
                    //streamWriter.Close();
                    Gf.RunProcess(OutputFileName);
                }
                catch
                {
                    //streamWriter.Flush();
                    //streamWriter.Close();
                    MessageBox.Show("Error generating listing " + OutputFileName + ". Document might be in use.");
                }
            }
            catch
            {
            }
            Cursor = Cursors.Default;
        }


        private void GenerateIndexReportTabText(string OutputFileName)
        {
            Cursor = Cursors.WaitCursor;
            StringBuilder stringBuilder = new StringBuilder();
            try
            {
                string bookReference = "";
                string referenceNumber = "";
                string columns = "";
                string songId = "";
                string writer = "";
                int codeIndex = -1;

                using DbConnection connection = DbController.GetDbConnection(Gf.ConnectStringMainDB);
                stringBuilder.Append("Song Title\tWriter\tMP\tSF\tTS\r\n");

                for (int i = 0; i < SongsList.Items.Count; i++)
                {
                    songId = DataUtil.Mid(SongsList.Items[i].SubItems[1].Text, 1);
                    bookReference = "";
                    columns = "";
                    writer = "";

                    DataRow dataRow = DbController.GetDataRowScalar(connection, $"Select BOOK_REFERENCE, writer From SONG Where SONGID={songId}");

                    if (dataRow != null)
                    {
                        bookReference = DataUtil.GetDataString(dataRow, "BOOK_REFERENCE");
                        writer = DataUtil.GetDataString(dataRow, "Writer");
                        referenceNumber = "";
                        columns = "\t" + writer;
                        codeIndex = bookReference.IndexOf("MP");
                        if (codeIndex >= 0)
                        {
                            int numberStart = codeIndex + 2;
                            int numberEnd = bookReference.IndexOfAny(Gf.ReferenceAlertPickSeparator.ToCharArray(), numberStart);
                            numberEnd = ((numberEnd >= 0) ? numberEnd : bookReference.Length);
                            referenceNumber = DataUtil.Mid(bookReference, numberStart, numberEnd - numberStart);
                        }
                        columns = columns + "\t" + referenceNumber;
                        referenceNumber = "";
                        codeIndex = bookReference.IndexOf("SF");
                        if (codeIndex >= 0)
                        {
                            int numberStart = codeIndex + 2;
                            int numberEnd = bookReference.IndexOfAny(Gf.ReferenceAlertPickSeparator.ToCharArray(), numberStart);
                            numberEnd = ((numberEnd >= 0) ? numberEnd : bookReference.Length);
                            referenceNumber = DataUtil.Mid(bookReference, numberStart, numberEnd - numberStart);
                        }
                        columns = columns + "\t" + referenceNumber;
                        referenceNumber = "";
                        codeIndex = bookReference.IndexOf("TS");
                        if (codeIndex >= 0)
                        {
                            int numberStart = codeIndex + 2;
                            int numberEnd = bookReference.IndexOfAny(Gf.ReferenceAlertPickSeparator.ToCharArray(), numberStart);
                            numberEnd = ((numberEnd >= 0) ? numberEnd : bookReference.Length);
                            referenceNumber = DataUtil.Mid(bookReference, numberStart, numberEnd - numberStart);
                        }
                        columns = columns + "\t" + referenceNumber;
                    }
                    else
                    {
                        columns = "\t\t\t\t";
                    }
                    stringBuilder.Append(Gf.RemoveMusicSym(SongsList.Items[i].SubItems[0].Text) + columns + "\r\n");
                }
                FileUtil.CreateNewFile(OutputFileName, FileUtil.FileContentsType.DoubleByte, stringBuilder.ToString());
                Gf.RunProcess(OutputFileName);
            }
            catch
            {
            }
            Cursor = Cursors.Default;
        }


        private string SaveListingOfFolder(ref string InFileName, ref string InitDirectory)
        {
            saveFileDialog1.Filter = "Rich Text Document (*.rtf)|*.rtf|Tab delimted Text File (*.txt)|*.txt";
            saveFileDialog1.Title = "Listing of Folder";
            saveFileDialog1.InitialDirectory = InitDirectory;
            saveFileDialog1.OverwritePrompt = true;
            saveFileDialog1.AddExtension = true;
            string extension = Path.GetExtension(InFileName);
            saveFileDialog1.FileName = Gf.GetDisplayNameOnly(ref InFileName, UpdateByRef: false, KeepExt: false);
            saveFileDialog1.DefaultExt = ((extension.ToLower() == ".txt") ? ".txt" : ".rtf");
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                InFileName = Path.GetFileName(saveFileDialog1.FileName);
                InitDirectory = Path.GetDirectoryName(saveFileDialog1.FileName);
                return saveFileDialog1.FileName;
            }
            return "";
        }

        private void TimerToFront_Tick(object sender, EventArgs e)
        {
            if (LoadRepaintCount > 5)
            {
                TimerToFront.Stop();
                Focus();
                base.TopMost = false;
            }
            else
            {
                LoadRepaintCount++;
            }
        }

        internal string RemoteControlLiveShow(LiveShowAction InAction)
        {
            return RemoteControlLiveShow(InAction, KeyDirection.Refresh);
        }

        private string RemoteControlLiveShow(LiveShowAction InAction, KeyDirection InDirection)
        {
            try
            {
                switch (InAction)
                {
                    case LiveShowAction.Remote_SongChanged:
                        LiveShow.Remote_SongChanged(ReLoadIfCaptureDevice: false);
                        // The show window has just processed the current output item: its state is current.
                        ReconcileOutputWithLivePowerPoint();
                        liveLoadSerialSeen = Gf.LiveLoadSerial;
                        break;
                    case LiveShowAction.Remote_SlideChanged:
                        LiveShow.Remote_SlideChanged((int)InDirection);
                        ReconcileOutputWithLivePowerPoint();
                        liveLoadSerialSeen = Gf.LiveLoadSerial;
                        break;
                    case LiveShowAction.Remote_MessageAlertRequested:
                        LiveShow.Remote_MessageAlertRequested();
                        break;
                    case LiveShowAction.Remote_ParentalAlertRequested:
                        LiveShow.Remote_ParentalAlertRequested();
                        break;
                    case LiveShowAction.Remote_ReferenceAlertShow:
                        LiveShow.Remote_ReferenceAlertRequested(NewStatus: true);
                        break;
                    case LiveShowAction.Remote_LyricsAlertShow:
                        LiveShow.Remote_LyricsAlertRequested();
                        break;
                    case LiveShowAction.Remote_ReferenceAlertHide:
                        LiveShow.Remote_ReferenceAlertRequested(NewStatus: false);
                        break;
                    case LiveShowAction.Remote_FormatChanged:
                        LiveShow.Remote_FormatChanged();
                        break;
                    case LiveShowAction.Remote_PanelChanged:
                        LiveShow.Remote_PanelChanged();
                        break;
                    case LiveShowAction.Remote_DefaultBackgroundChanged:
                        LiveShow.Remote_DefaultBackgroundChanged();
                        break;
                    case LiveShowAction.Remote_BackgroundChanged:
                        LiveShow.Remote_BackgroundChanged();
                        break;
                    case LiveShowAction.Remote_MoveToItemChanged:
                        // Loads the next / previous item in the show window (item keys, end of a deck).
                        LiveShow.Remote_MoveToItemChanged();
                        ReconcileOutputWithLivePowerPoint();
                        liveLoadSerialSeen = Gf.LiveLoadSerial;
                        break;
                    case LiveShowAction.Remote_LiveBlackClearChanged:
                        LiveShow.Remote_LiveBlackClearChanged();
                        break;
                    case LiveShowAction.Remote_ChineseChanged:
                        LiveShow.Remote_ChineseChanged();
                        break;
                    case LiveShowAction.Remote_WorshipListChanged:
                        LiveShow.Remote_WorshipListChanged();
                        break;
                    //case LiveShowAction.Remote_LiveCamStartStop:
                    //    LiveShow.Remote_LiveCamStartStop();
                    //    break;
                    //case LiveShowAction.Remote_LiveCamUpdate:
                    //    LiveShow.Remote_LiveCamUpdate();
                    //    break;
                    case LiveShowAction.Remote_RefreshMediaWindow:
                        LiveShow.Remote_RefreshMediaWindow();
                        break;
                    case LiveShowAction.Remote_RotateOnOffChanged:
                        LiveShow.Remote_RotateOnOffChanged();
                        break;
                    case LiveShowAction.Remote_GetMediaTimings:
                        return LiveShow.Remote_GetMediaTimings();
                    //case LiveShowAction.Remote_MediaItemPausePlay:
                    //    return LiveShow.Remote_MediaItemPausePlay();
                    case LiveShowAction.Remote_SongJumpTo:
                        // Loads the jumped-to item (or a gap item) in the show window.
                        LiveShow.Remote_SongJumpTo();
                        ReconcileOutputWithLivePowerPoint();
                        liveLoadSerialSeen = Gf.LiveLoadSerial;
                        break;
                }
            }
            catch (Exception ex)
            {
                // The show window on the output monitor stays the one this form talks to: a failed
                // action never swaps it for a new, never shown window. Only a window that no longer
                // exists is replaced (for the next show); the action is not repeated on it.
                LogLiveShowFailure("RemoteControlLiveShow(" + InAction + ")", ex);
                EnsureLiveShowWindow();
            }
            return "";
        }

        /// <summary>
        /// Gives LiveShow a usable show window: a new one, wired like the first instance (OnMessage,
        /// see Form1_Load), when there is none or it has been disposed. A live window is kept.
        /// </summary>
        private void EnsureLiveShowWindow()
        {
            if (LiveShow == null || LiveShow.IsDisposed)
            {
                LiveShow = new FrmLaunchShow();
                LiveShow.OnMessage += LiveShow_OnMessage;
            }
        }

        private void LogLiveShowFailure(string where, Exception ex)
        {
            // Logged in full once, repeats only counted (Remote_GetMediaTimings runs every 600 ms).
            WriteDebugLogThrottled(where, ex);
            ShowErrorNotice("Show window: " + ex.Message);
        }

        private void InfoScreenFolder_SelectedIndexChanged(object sender, EventArgs e)
        {
            ShowInfoScreenFolderContents();
        }

        private void ShowInfoScreenFolderContents()
        {
            if (InfoScreenFolder.Items.Count > 0)
            {
                Cursor = Cursors.WaitCursor;
                ListBox listBox = new ListBox();
                listBox.Items.Clear();
                listBox.Sorted = false;
                ListViewItem listViewItem = new ListViewItem();
                try
                {
                    string path = Gf.InfoScreenGroups[InfoScreenFolder.SelectedIndex].Path;
                    string[] files = Directory.GetFiles(path, "*.esi");
                    string[] esiFiles = files;
                    foreach (string filePath in esiFiles)
                    {
                        if (filePath != "")
                        {
                            listBox.Items.Add(filePath);
                        }
                    }
                }
                catch
                {
                }
                listBox.Sorted = true;
                InfoScreenList.Items.Clear();
                string fileName = "";
                for (int j = 0; j < listBox.Items.Count; j++)
                {
                    fileName = listBox.Items[j].ToString();
                    listViewItem = InfoScreenList.Items.Add(Gf.GetDisplayNameOnly(ref fileName, UpdateByRef: false, KeepExt: false));
                    listViewItem.SubItems.Add("I" + listBox.Items[j]);
                }
                SetInfoScreenListColWidth();
                listBox.Dispose();
                Cursor = Cursors.Default;
            }
        }

        private void InfoScreenList_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.A)
            {
                ListViewSelectItems(InfoScreenList, SelectAll: true);
            }
            else if (e.Control && e.KeyCode == Keys.C)
            {
                EF_Copy_Clicked();
            }
            else if (e.KeyCode == Keys.Delete)
            {
                EF_Delete_Clicked();
                InfoScreenList.Focus();
            }
            else
            {
                InfoScreenListIndexChanged(1, ScrollToCaret: false);
                InfoScreenList.Focus();
            }
        }

        private void InfoScreenList_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (Gf.EasiSlidesMode == UsageMode.Worship && Gf.GetSelectedIndex(InfoScreenList) >= 0)
            {
                AddToWorshipList();
            }
        }

        private void InfoScreenList_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                InfoScreenListIndexChanged();
            }
            else if (e.Button == MouseButtons.Right)
            {
                ListViewItem itemAt = InfoScreenList.GetItemAt(e.X, e.Y);
                string OutString = "";
                if (itemAt != null)
                {
                    itemAt.Selected = true;
                    InfoScreenListIndexChanged();
                }
                Gf.GetSelectedIndex(InfoScreenList, ref OutString);
            }
            InfoScreenList.Focus();
        }

        private void InfoScreenListIndexChanged()
        {
            InfoScreenListIndexChanged(1);
        }

        private void InfoScreenListIndexChanged(int StartingSlide)
        {
            InfoScreenListIndexChanged(StartingSlide, ScrollToCaret: true);
        }

        private void InfoScreenListIndexChanged(int StartingSlide, bool ScrollToCaret)
        {
            int selectedIndex = Gf.GetSelectedIndex(InfoScreenList);
            if (selectedIndex >= 0)
            {
                Gf.PreviewItem.Source = ItemSource.ExternalFileInfoScreen;
                string idString = InfoScreenList.Items[selectedIndex].SubItems[1].Text;
                string InTitle = InfoScreenList.Items[selectedIndex].SubItems[0].Text;
                Gf.PreviewItem.InMainItemText = InTitle;
                Gf.PreviewItem.InSubItemItem1Text = idString;
                Gf.PreviewItem.CurItemNo = 0;
                LoadItem(ref Gf.PreviewItem, idString, "", StartingSlide, ref InTitle, ScrollToCaret);
                UpdateDisplayPanelFields();
            }
            else
            {
                Gf.PreviewItem.Type = "";
                Gf.PreviewItem.Title = "";
                Gf.PreviewItem.ItemID = "";
                Gf.PreviewItem.CurItemNo = 0;
                Gf.LoadIndividualFormatData(ref Gf.PreviewItem, "");
                AllowIndividualFormat(AllowFormat: false);
                UpdateFormatFields();
                BuildVerseButtons(Gf.PreviewItem);
                DisplayLyrics(Gf.PreviewItem, 0, ScrollToCaret: true);
                UpdateDisplayPanelFields();
            }
        }

        private void InfoScreen_OpenFolder_Click(object sender, EventArgs e)
        {
            string folderPath = Gf.InfoScreenGroups[InfoScreenFolder.SelectedIndex].Path;
            if (Gf.ValidateDir(folderPath, CreateDir: true))
            {
                Gf.RunProcess(folderPath);
            }
        }

        private void EF_New_Clicked()
        {
            ShowInfoScreen("");
        }

        private void EF_Edit_Clicked()
        {
            if (InfoScreenList.Items.Count > 0)
            {
                if (InfoScreenList.SelectedItems.Count > 0)
                {
                    Edit_Item(InfoScreenList.SelectedItems[0].SubItems[1].Text);
                }
                else
                {
                    MessageBox.Show("Please select ONE InfoScreen to edit");
                }
            }
        }

        private void EF_Copy_Clicked()
        {
            CopyExternalItems(InfoScreenList, "I");
        }

        private void EF_Move_Clicked()
        {
            MoveExternalItems(InfoScreenList, "I");
        }

        private void EF_Delete_Clicked()
        {
            RemoveExternalItems(InfoScreenList, "I");
        }

        private void CopyExternalItems(ListView InListView, string InFileSymbol)
        {
            if (InListView.SelectedItems.Count <= 0)
            {
                return;
            }
            Gf.SelectedItemsCount = InListView.SelectedItems.Count;
            Gf.ExternalMoveFolder = -1;
            Gf.ExternalCopyFolder = 1;
            Gf.ExternalMoveCopyType = InFileSymbol;
            FrmCopyMoveExternal frmCopyMoveExternal = new FrmCopyMoveExternal();
            if (frmCopyMoveExternal.ShowDialog() != DialogResult.OK)
            {
                return;
            }
            if (Gf.ExternalCopyFolder >= 0)
            {
                string[] copiedFiles = new string[30000];
                copiedFiles[0] = "0";
                ListView listView = new ListView();
                ListViewItem listViewItem = new ListViewItem();
                string InFileName = "";
                for (int i = 0; i < InListView.SelectedItems.Count; i++)
                {
                    switch (InFileSymbol)
                    {
                        case "I":
                            InFileName = Gf.CopyExternalFile(DataUtil.Mid(InListView.SelectedItems[i].SubItems[1].Text, 1), Gf.InfoScreenGroups[Gf.ExternalCopyFolder].Path);
                            break;
                        case "P":
                            InFileName = Gf.CopyExternalFile(DataUtil.Mid(InListView.SelectedItems[i].SubItems[1].Text, 1), Gf.PowerpointGroups[Gf.ExternalCopyFolder].Path);
                            break;
                    }
                    if (InFileName != "")
                    {
                        listViewItem = listView.Items.Add(Gf.GetDisplayNameOnly(ref InFileName, UpdateByRef: false));
                        copiedFiles[0] = Convert.ToString(DataUtil.StringToInt(copiedFiles[0]) + 1);
                        copiedFiles[DataUtil.StringToInt(copiedFiles[0])] = Gf.GetDisplayNameOnly(ref InFileName, UpdateByRef: false);
                    }
                }
                if (InFileSymbol == "I")
                {
                    HighlightCopyMoveExternalItems(InfoScreenList, Gf.ExternalCopyFolder, copiedFiles, InFileSymbol);
                }
                else
                {
                    HighlightCopyMoveExternalItems(PowerpointList, Gf.ExternalCopyFolder, copiedFiles, InFileSymbol);
                }
            }
            else
            {
                if (InFileSymbol != "I")
                {
                    return;
                }
                Gf.ExternalCopyFolder = -1 * Gf.ExternalCopyFolder;
                if (Gf.ExternalCopyFolder <= 0)
                {
                    Gf.ExternalCopyFolder = 1;
                }
                int[] RefileSongs = new int[30000];
                RefileSongs[0] = 0;
                ListView listView = new ListView();
                ListViewItem listViewItem = new ListViewItem();
                string InFileName = "";
                string[] ThisHeaderData = new string[255];
                for (int i = 0; i < InListView.SelectedItems.Count; i++)
                {
                    InFileName = DataUtil.Mid(InListView.SelectedItems[i].SubItems[1].Text, 1);
                    if (InFileName != "")
                    {
                        gfFileHelpers.LoadInfoFile(InFileName, ref Gf.TempItem1, ref ThisHeaderData);
                        if (DataUtil.Trim(Gf.TempItem1.Title) == "")
                        {
                            Gf.TempItem1.Title = Gf.GetDisplayNameOnly(ref InFileName, UpdateByRef: false);
                        }
                        Gf.TempItem1.FolderNo = Gf.ExternalCopyFolder;
                        int newSongId = Gf.InsertItemIntoDatabase(Gf.ConnectStringMainDB, Gf.TempItem1);
                        if (newSongId > 0)
                        {
                            RefileSongs[0]++;
                            RefileSongs[RefileSongs[0]] = newSongId;
                        }
                    }
                }
                if (RefileSongs[0] > 0)
                {
                    HighlightCopyMoveItems(Gf.ExternalCopyFolder, ref RefileSongs);
                }
            }
        }

        private void MoveExternalItems(ListView InListView, string InFileSymbol)
        {
            if (InListView.SelectedItems.Count <= 0)
            {
                return;
            }
            Gf.SelectedItemsCount = InListView.SelectedItems.Count;
            Gf.ExternalMoveFolder = 1;
            Gf.ExternalCopyFolder = -1;
            Gf.ExternalMoveCopyType = InFileSymbol;
            FrmCopyMoveExternal frmCopyMoveExternal = new FrmCopyMoveExternal();
            if (frmCopyMoveExternal.ShowDialog() != DialogResult.OK || Gf.ExternalMoveFolder == InfoScreenFolder.SelectedIndex || Gf.ExternalMoveFolder < 0 || Gf.ExternalMoveFolder == InfoScreenFolder.SelectedIndex)
            {
                return;
            }
            string[] movedFiles = new string[30000];
            movedFiles[0] = "0";
            ListView listView = new ListView();
            ListViewItem listViewItem = new ListViewItem();
            string InFileName = "";
            for (int i = 0; i < InListView.SelectedItems.Count; i++)
            {
                switch (InFileSymbol)
                {
                    case "I":
                        InFileName = Gf.MoveExternalFile(DataUtil.Mid(InListView.SelectedItems[i].SubItems[1].Text, 1), Gf.InfoScreenGroups[Gf.ExternalMoveFolder].Path);
                        break;
                    case "P":
                        InFileName = Gf.MoveExternalFile(DataUtil.Mid(InListView.SelectedItems[i].SubItems[1].Text, 1), Gf.PowerpointGroups[Gf.ExternalMoveFolder].Path);
                        break;
                }
                if (InFileName != "")
                {
                    listViewItem = listView.Items.Add(Gf.GetDisplayNameOnly(ref InFileName, UpdateByRef: false));
                    movedFiles[0] = Convert.ToString(DataUtil.StringToInt(movedFiles[0]) + 1);
                    movedFiles[DataUtil.StringToInt(movedFiles[0])] = Gf.GetDisplayNameOnly(ref InFileName, UpdateByRef: false);
                }
            }
            if (InFileSymbol == "I")
            {
                HighlightCopyMoveExternalItems(InfoScreenList, Gf.ExternalMoveFolder, movedFiles, InFileSymbol);
            }
            else
            {
                HighlightCopyMoveExternalItems(PowerpointList, Gf.ExternalMoveFolder, movedFiles, InFileSymbol);
            }
        }

        private void RemoveExternalItems(ListView InListView, string InFileSymbol)
        {
            if (InListView.SelectedItems.Count <= 0)
            {
                return;
            }
            string itemKind = "";
            if (InFileSymbol == "I")
            {
                itemKind = "InfoScreen(s)";
            }
            else
            {
                if (InFileSymbol != "P")
                {
                    return;
                }
                itemKind = "Powerpint File(s)";
            }
            if (MessageBox.Show("Delete Selected " + itemKind + " to Windows Recycle Bin?", "Delete " + itemKind, MessageBoxButtons.YesNo) != DialogResult.Yes)
            {
                return;
            }
            string InFileName = "";
            if (Gf.OutputItem.Type == InFileSymbol)
            {
                InFileName = Gf.OutputItem.ItemID;
                Gf.GetDisplayNameOnly(ref InFileName, UpdateByRef: true);
            }
            ListView.SelectedListViewItemCollection selectedItems = InListView.SelectedItems;
            int index = selectedItems[selectedItems.Count - 1].Index;
            for (int i = selectedItems.Count - 1; i >= 0; i--)
            {
                if (selectedItems[i].Text != InFileName)
                {
                    try
                    {
                        if (Gf.RecycleBin(DataUtil.Mid(selectedItems[i].SubItems[1].Text, 1)))
                        {
                            selectedItems[i].Remove();
                        }
                    }
                    catch
                    {
                    }
                }
            }
            if (InListView.Items.Count > index)
            {
                if (index >= 0)
                {
                    InListView.Items[index].Selected = true;
                }
            }
            else if (InListView.Items.Count > 0)
            {
                if (index - 1 >= 0 && index <= InListView.Items.Count)
                {
                    InListView.Items[index - 1].Selected = true;
                }
                else
                {
                    InListView.Items[InListView.Items.Count - 1].Selected = true;
                }
            }
            if (InFileSymbol == "I")
            {
                InfoScreenListIndexChanged();
            }
            else if (InFileSymbol == "P")
            {
                PowerpointListIndexChanged();
            }
        }

        private void SetTabsVisibility()
        {
            SetOneTab(ref tabControlSource, tabPowerpoint, Gf.UsePowerpointTab, tabFiles.Name);
            SetOneTab(ref tabControlSource, tabMedia, Gf.UseMediaTab, tabImages.Name);
            panelOutputLM1.Visible = Gf.ShowLyricsMonitorAlertBox;
            ResizePreviewBottomPanel();
            ResizeOutputBottomPanel();
        }

        private void SetOneTab(ref TabControl InTab, TabPage InTabPage, bool SetVisible, string InsertAfterTabName)
        {
            if (SetVisible)
            {
                if (InTab.TabPages.Contains(InTabPage))
                {
                    return;
                }
                int index = InTab.TabPages.Count;
                for (int i = 0; i < InTab.TabPages.Count; i++)
                {
                    if (InTab.TabPages[i].Name.ToLower() == InsertAfterTabName.ToLower())
                    {
                        index = i;
                    }
                }
                InTab.TabPages.Insert(index, InTabPage);
            }
            else if (InTab.TabPages.Contains(InTabPage))
            {
                InTab.TabPages.Remove(InTabPage);
            }
        }

        private void PowerpointListIndexChanged()
        {
            PowerpointListIndexChanged(1);
        }

        private void PowerpointListIndexChanged(int StartingSlide)
        {
            PowerpointListIndexChanged(StartingSlide, ScrollToCaret: true);
        }

        private void PowerpointListIndexChanged(int StartingSlide, bool ScrollToCaret)
        {
            int selectedIndex = Gf.GetSelectedIndex(PowerpointList);
            if (selectedIndex >= 0)
            {
                Gf.PreviewItem.Source = ItemSource.ExternalFilePowerpoint;
                string idString = PowerpointList.Items[selectedIndex].SubItems[1].Text;
                string InTitle = Path.GetFileNameWithoutExtension(DataUtil.Right(idString, idString.Length - 1));
                Gf.PreviewItem.InMainItemText = InTitle;
                Gf.PreviewItem.InSubItemItem1Text = idString;
                Gf.PreviewItem.CurItemNo = 0;
                LoadItem(ref Gf.PreviewItem, idString, "", 1, ref InTitle, ScrollToCaret: false);
                UpdateDisplayPanelFields();
            }
            else
            {
                Gf.PreviewItem.Type = "";
                Gf.PreviewItem.Title = "";
                Gf.PreviewItem.ItemID = "";
                Gf.PreviewItem.CurItemNo = 0;
                Gf.LoadIndividualFormatData(ref Gf.PreviewItem, "");
                AllowIndividualFormat(AllowFormat: false);
                UpdateFormatFields();
                BuildVerseButtons(Gf.PreviewItem);
                DisplayLyrics(Gf.PreviewItem, 0, ScrollToCaret: true);
                UpdateDisplayPanelFields();
            }
        }

        private void PP_Style_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            Gf.AssignDropDownItem(ref PP_ListType, e.ClickedItem.Name, PP_ListStyle, PP_PreviewStyle);
            Gf.PowerpointListingStyle = DataUtil.ObjToInt(PP_ListType.Tag);
            PowerpointStyle_Changed(RebuildPowerpointFolders: false);
        }

        private void PowerpointStyle_Changed(bool RebuildPowerpointFolders)
        {
            PP_OpenFolder.ToolTipText = "Open Powerpoint Folder";
            if (Gf.PowerpointListingStyle == 0)
            {
                PowerpointList.Visible = true;
                flowLayoutExternalPowerPoint.Visible = false;
            }
            else
            {
                PowerpointList.Visible = false;
                flowLayoutExternalPowerPoint.Visible = true;
            }
            if (RebuildPowerpointFolders)
            {
                BuildPowerpointFolderList();
            }
            else
            {
                ShowPowerpointFolderContents((Gf.PowerpointListingStyle == 1) ? true : false);
            }
        }

        private void PP_Edit_Clicked()
        {
            if (PowerpointList.Items.Count > 0)
            {
                if (PowerpointList.SelectedItems.Count > 0)
                {
                    Edit_Item(PowerpointList.SelectedItems[0].SubItems[1].Text);
                }
                else
                {
                    MessageBox.Show("Please select ONE Powerpoint file to edit");
                }
            }
        }

        private void PP_Copy_Clicked()
        {
            if (DataUtil.ObjToInt(PP_ListType.Tag) == 0)
            {
                CopyExternalItems(PowerpointList, "P");
            }
        }

        private void PP_Move_Clicked()
        {
            if (DataUtil.ObjToInt(PP_ListType.Tag) == 0)
            {
                MoveExternalItems(PowerpointList, "P");
            }
        }

        private void PP_Delete_Clicked()
        {
            if (DataUtil.ObjToInt(PP_ListType.Tag) == 0)
            {
                RemoveExternalItems(PowerpointList, "P");
            }
        }

        private void PP_Btn_Click(object sender, EventArgs e)
        {
            string folderPath = Gf.PowerpointGroups[PowerpointFolder.SelectedIndex].Path;
            if (Gf.ValidateDir(folderPath, CreateDir: true))
            {
                Gf.RunProcess(folderPath);
            }
        }

        private void CMenuFiles_SelectAll_Click(object sender, EventArgs e)
        {
            if (IsSelectedTab(tabControlSource, "tabFiles"))
            {
                ListViewSelectItems(InfoScreenList, SelectAll: true);
            }
            else if (IsSelectedTab(tabControlSource, "tabPowerpoint"))
            {
                ListViewSelectItems(PowerpointList, SelectAll: true);
            }
            else if (IsSelectedTab(tabControlSource, "tabMedia"))
            {
                ListViewSelectItems(MediaList, SelectAll: true);
            }
        }

        private void CMenuFiles_UnselectAll_Click(object sender, EventArgs e)
        {
            if (IsSelectedTab(tabControlSource, "tabFiles"))
            {
                ListViewSelectItems(InfoScreenList, SelectAll: false);
            }
            else if (IsSelectedTab(tabControlSource, "tabPowerpoint"))
            {
                ListViewSelectItems(PowerpointList, SelectAll: false);
            }
            else if (IsSelectedTab(tabControlSource, "tabMedia"))
            {
                ListViewSelectItems(MediaList, SelectAll: false);
            }
        }

        private void ListViewSelectItems(ListView InListView, bool SelectAll)
        {
            if (InListView.Items.Count > 0)
            {
                Cursor.Current = Cursors.WaitCursor;
                for (int i = 0; i <= InListView.Items.Count - 1; i++)
                {
                    InListView.Items[i].Selected = SelectAll;
                }
                InListView.Focus();
                Cursor.Current = Cursors.Default;
            }
        }

        private void CMenuFiles_AddShow_Click(object sender, EventArgs e)
        {
            if (AddToWorshipList())
            {
                int selectedIndex = Gf.GetSelectedIndex(WorshipListItems);
                WorshipListItems.EnsureVisible(selectedIndex);
                Gf.StartPresAt = selectedIndex + 1;
                PreviewItemToLive();
            }
        }

        private void CMenuFiles_Edit_Click(object sender, EventArgs e)
        {
            if (IsSelectedTab(tabControlSource, "tabFiles"))
            {
                EF_Edit_Clicked();
            }
            else if (IsSelectedTab(tabControlSource, "tabPowerpoint"))
            {
                PP_Edit_Clicked();
            }
        }

        private void CMenuFiles_Copy_Click(object sender, EventArgs e)
        {
            if (IsSelectedTab(tabControlSource, "tabFiles"))
            {
                EF_Copy_Clicked();
            }
            else if (IsSelectedTab(tabControlSource, "tabPowerpoint"))
            {
                PP_Copy_Clicked();
            }
        }

        private void CMenuFiles_Refresh_Click(object sender, EventArgs e)
        {
            RefreshItems();
        }

        private void PowerpointFolder_SelectedIndexChanged(object sender, EventArgs e)
        {
            ShowPowerpointFolderContents((Gf.PowerpointListingStyle == 1) ? true : false);
        }

        private void PowerpointList_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.A)
            {
                ListViewSelectItems(PowerpointList, SelectAll: true);
            }
            else if (e.Control && e.KeyCode == Keys.C)
            {
                PP_Copy_Clicked();
            }
            else if (e.KeyCode == Keys.Delete)
            {
                PP_Delete_Clicked();
                PowerpointList.Focus();
            }
            else
            {
                PowerpointListIndexChanged(1, ScrollToCaret: false);
                PowerpointList.Focus();
            }
        }

        private void PowerpointList_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (Gf.EasiSlidesMode == UsageMode.Worship && Gf.GetSelectedIndex(PowerpointList) >= 0)
            {
                AddToWorshipList();
            }
        }

        private void PowerpointList_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                PowerpointListIndexChanged();
            }
            else if (e.Button == MouseButtons.Right)
            {
                ListViewItem itemAt = PowerpointList.GetItemAt(e.X, e.Y);
                string OutString = "";
                if (itemAt != null)
                {
                    itemAt.Selected = true;
                    PowerpointListIndexChanged();
                }
                Gf.GetSelectedIndex(PowerpointList, ref OutString);
            }
            PowerpointList.Focus();
        }

        private void MediaFolder_SelectedIndexChanged(object sender, EventArgs e)
        {
            ShowMediaFolderContents();
        }

        private void ShowMediaFolderContents()
        {
            if (MediaFolder.Items.Count > 0)
            {
                Cursor = Cursors.WaitCursor;
                ListBox listBox = new ListBox();
                listBox.Items.Clear();
                listBox.Sorted = false;
                ListViewItem listViewItem = new ListViewItem();
                try
                {
                    string path = Gf.MediaGroups[MediaFolder.SelectedIndex].Path;
                    for (int i = 0; i < Gf.MediaFileExtensions.Count; i++)
                    {
                        try
                        {
                            string[] files = Directory.GetFiles(path, "*" + Gf.MediaFileExtensions[i].Extension);
                            string[] mediaFiles = files;
                            foreach (string filePath in mediaFiles)
                            {
                                if (filePath != "")
                                {
                                    listBox.Items.Add(filePath);
                                }
                            }
                        }
                        catch
                        {
                        }
                    }
                }
                catch
                {
                }
                listBox.Sorted = true;
                MediaList.Items.Clear();
                string fileName = "";
                for (int i = 0; i < listBox.Items.Count; i++)
                {
                    fileName = listBox.Items[i].ToString();
                    listViewItem = MediaList.Items.Add(Gf.GetDisplayNameOnly(ref fileName, UpdateByRef: false, KeepExt: true));
                    listViewItem.SubItems.Add("M" + listBox.Items[i]);
                }
                SetMediaListColWidth();
                listBox.Dispose();
                Cursor = Cursors.Default;
            }
        }

        private void MediaListIndexChanged()
        {
            int selectedIndex = Gf.GetSelectedIndex(MediaList);
            if (selectedIndex >= 0)
            {
                Gf.PreviewItem.Source = ItemSource.ExternalFileMedia;
                string idString = MediaList.Items[selectedIndex].SubItems[1].Text;
                string InFileName = MediaList.Items[selectedIndex].SubItems[0].Text;
                Gf.GetDisplayNameOnly(ref InFileName, UpdateByRef: true);
                Gf.PreviewItem.InMainItemText = InFileName;
                Gf.PreviewItem.InSubItemItem1Text = idString;
                Gf.PreviewItem.CurItemNo = 0;
                LoadItem(ref Gf.PreviewItem, idString, "", 1, ref InFileName, ScrollToCaret: false);
                UpdateDisplayPanelFields();
            }
            else
            {
                Gf.PreviewItem.Type = "";
                Gf.PreviewItem.Title = "";
                Gf.PreviewItem.ItemID = "";
                Gf.PreviewItem.CurItemNo = 0;
                Gf.LoadIndividualFormatData(ref Gf.PreviewItem, "");
                AllowIndividualFormat(AllowFormat: false);
                UpdateFormatFields();
                BuildVerseButtons(Gf.PreviewItem);
                DisplayLyrics(Gf.PreviewItem, 0, ScrollToCaret: true);
                UpdateDisplayPanelFields();
            }
        }

        private void MediaList_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.A)
            {
                ListViewSelectItems(MediaList, SelectAll: true);
            }
            else if (!e.Control || e.KeyCode != Keys.C)
            {
                if (e.KeyCode == Keys.Delete)
                {
                    MediaList.Focus();
                    return;
                }
                MediaListIndexChanged();
                MediaList.Focus();
            }
        }

        private void MediaList_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (Gf.EasiSlidesMode == UsageMode.Worship && Gf.GetSelectedIndex(MediaList) >= 0)
            {
                AddToWorshipList();
            }
        }

        private void MediaList_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                MediaListIndexChanged();
            }
            else if (e.Button == MouseButtons.Right)
            {
                ListViewItem itemAt = MediaList.GetItemAt(e.X, e.Y);
                string OutString = "";
                if (itemAt != null)
                {
                    itemAt.Selected = true;
                    MediaListIndexChanged();
                }
                Gf.GetSelectedIndex(MediaList, ref OutString);
            }
            MediaList.Focus();
        }

        private void Media_OpenFolder_Click(object sender, EventArgs e)
        {
            string folderPath = Gf.MediaGroups[MediaFolder.SelectedIndex].Path;
            if (Gf.ValidateDir(folderPath, CreateDir: true))
            {
                Gf.RunProcess(folderPath);
            }
        }

        private void Image_OpenFolder_Click(object sender, EventArgs e)
        {
            string folderPath = Gf.PictureGroups[ImagesFolder.SelectedIndex].Path;
            if (Gf.ValidateDir(folderPath, CreateDir: true))
            {
                Gf.RunProcess(folderPath);
            }
        }

        private void Image_Import_Click(object sender, EventArgs e)
        {
            string folderPath = Gf.PictureGroups[ImagesFolder.SelectedIndex].Path;
            openFileDialog1.Filter = "Images (*.jpg,*jpeg,*.bmp,*.gif,*.ico)|*.jpg;*jpeg;*.bmp;*.gif;*.ico";
            openFileDialog1.Title = "Import An Image into folder: " + ImagesFolder.Text;
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string fileName = openFileDialog1.FileName;
                try
                {
                    File.Copy(fileName, folderPath + Path.GetFileName(fileName));
                    ShowPicturesFolderThumbs();
                }
                catch
                {
                    MessageBox.Show("Image File could not be imported. This is possibly because there is already a file with the same name in the folder.");
                }
            }
        }

        private void InfoScreen_Import_Click(object sender, EventArgs e)
        {
            string folderPath = Gf.InfoScreenGroups[InfoScreenFolder.SelectedIndex].Path;
            openFileDialog1.Filter = "InfoScreens (*.esi)|*.esi";
            openFileDialog1.Title = "Import An InfoScreen into folder: " + InfoScreenFolder.Text;
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string fileName = openFileDialog1.FileName;
                try
                {
                    File.Copy(fileName, folderPath + Path.GetFileName(fileName));
                    ShowInfoScreenFolderContents();
                    int index = 0;
                    while (true)
                    {
                        if (index >= InfoScreenList.Items.Count)
                        {
                            return;
                        }
                        if (DataUtil.Mid(InfoScreenList.Items[index].SubItems[1].Text, 1) == folderPath + Path.GetFileName(fileName))
                        {
                            break;
                        }
                        index++;
                    }
                    InfoScreenList.Items[index].Selected = true;
                    InfoScreenList.Items[index].EnsureVisible();
                    InfoScreenListIndexChanged();
                }
                catch
                {
                    MessageBox.Show("InfoScreen could not be imported. This is possibly because there is already a file with the same name in the folder.");
                }
            }
        }

        private void PP_Import_Click(object sender, EventArgs e)
        {
            string folderPath = Gf.PowerpointGroups[PowerpointFolder.SelectedIndex].Path;
            //openFileDialog1.Filter = "Powerpoint Files (*.ppt)|*.ppt";

            openFileDialog1.Filter = "Powerpoint Files(*.ppt)| *.ppt | Powerpoint Files(*.pptx) | *.pptx";
            openFileDialog1.Title = "Import a Powerpoint File into folder: " + PowerpointFolder.Text;
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string fileName = openFileDialog1.FileName;
                try
                {
                    File.Copy(fileName, folderPath + Path.GetFileName(fileName));
                    ShowPowerpointFolderContents((Gf.PowerpointListingStyle == 1) ? true : false);
                    if (Gf.PowerpointListingStyle == 0)
                    {
                        ShowPowerpointFolderContents(ShowThumbs: false);
                        int index = 0;
                        while (true)
                        {
                            if (index >= PowerpointList.Items.Count)
                            {
                                return;
                            }
                            if (DataUtil.Mid(PowerpointList.Items[index].SubItems[1].Text, 1) == folderPath + Path.GetFileName(fileName))
                            {
                                break;
                            }
                            index++;
                        }
                        PowerpointList.Items[index].Selected = true;
                        PowerpointList.Items[index].EnsureVisible();
                        PowerpointListIndexChanged();
                    }
                }
                catch
                {
                    MessageBox.Show("Powerpoint File could not be imported. This is possibly because there is already a file with the same name in the folder.");
                }
            }
        }

        private void Media_Import_Click(object sender, EventArgs e)
        {
            string folderPath = Gf.MediaGroups[MediaFolder.SelectedIndex].Path;
            string filterLabel = (Gf.MediaFileExtensions.Count > 0) ? "Media Files (" : "";
            string filterPattern = (Gf.MediaFileExtensions.Count > 0) ? ")|" : "";
            for (int i = 0; i < Gf.MediaFileExtensions.Count; i++)
            {
                filterLabel = filterLabel + ((i > 0) ? "," : "") + "*" + Gf.MediaFileExtensions[i].Extension.ToLower();
                filterPattern = filterPattern + ((i > 0) ? ";" : "") + "*" + Gf.MediaFileExtensions[i].Extension.ToLower();
            }
            string filter = filterLabel + filterPattern;
            openFileDialog1.Filter = filter;
            openFileDialog1.Title = "Import A Media File into folder: " + MediaFolder.Text;
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string fileName = openFileDialog1.FileName;
                try
                {
                    File.Copy(fileName, folderPath + Path.GetFileName(fileName));
                    ShowMediaFolderContents();
                    int i = 0;
                    while (true)
                    {
                        if (i >= MediaList.Items.Count)
                        {
                            return;
                        }
                        if (DataUtil.Mid(MediaList.Items[i].SubItems[1].Text, 1) == folderPath + Path.GetFileName(fileName))
                        {
                            break;
                        }
                        i++;
                    }
                    MediaList.Items[i].Selected = true;
                    MediaList.Items[i].EnsureVisible();
                    MediaListIndexChanged();
                }
                catch
                {
                    MessageBox.Show("Media File could not be imported. Please check you have write access to the folder");
                }
            }
        }

        private void Btn_Popup_Click(object sender, EventArgs e)
        {
            Point popupPoint = new Point(0, 0);
            Button button = (Button)sender;
            PopupBtnPressed = button.Name;
            string popupBtnPressed = PopupBtnPressed;
            if (popupBtnPressed != null && popupBtnPressed == "BtnEditSessionNotes")
            {
                Gf.popUpText = PreviewNotes.Text;
                Gf.popUpTextMaxLength = PreviewNotes.MaxLength;
                popupPoint = new Point(splitContainerMain.Left + splitContainerMain.Panel1.Width + splitContainerMain.SplitterWidth, menuStripMain.Top + toolStripMain.Height + splitContainerPreview.Panel1.Height + panel1.Height + 20);
            }
            FrmPopupText popup = new FrmPopupText();
            Point location = PointToScreen(popupPoint);
            popupHelper.ShowPopup(this, popup, location);
        }

        private void popupHelper_PopupClosed(object sender, PopupClosedEventArgs e)
        {
            string popupBtnPressed = PopupBtnPressed;
            if (popupBtnPressed != null && popupBtnPressed == "BtnEditSessionNotes")
            {
                PreviewNotes.Text = Gf.popUpText;
            }
        }

        private void Menu_RefreshOutput_Click(object sender, EventArgs e)
        {
            Gf.RefreshWindowsDesktop();
            if (Gf.ShowRunning)
            {
                RemoteControlLiveShow(LiveShowAction.Remote_RefreshMediaWindow);
            }
        }

        private void FormatLyricsContainers(SongSettings InItem)
        {
            FormatLyricsContainers(InItem, Reset: false, SetFocus: false);
        }

        private void FormatLyricsContainers(SongSettings InItem, bool Reset, bool SetFocus)
        {
            if (InItem.OutputStyleScreen)
            {
                FormatLyricsContainers(ref Lyrics_OutputBox, ref flowLayoutOutputLyrics, InItem, Reset, SetFocus);
            }
            else
            {
                FormatLyricsContainers(ref Lyrics_PreviewBox, ref flowLayoutPreviewLyrics, InItem, Reset, SetFocus);
            }
        }

        private void FormatLyricsContainers(ref RichTextBox[] InRichTextBox, ref Panel InPanel, SongSettings InItem, bool Reset, bool SetFocus)
        {
            string name = (InPanel.Name == flowLayoutPreviewLyrics.Name) ? "Preview" : "Output";

            InPanel.Controls.Clear();

            InPanel.AutoScroll = true;
            int boxCount = Reset ? 1 : InItem.TotalSlides;
            Font lyricsFont = GetLyricsFont(InPanel, InRichTextBox);
            for (int i = 1; i <= boxCount; i++)
            {
                if (InRichTextBox[i] == null)
                {
                    InRichTextBox[i] = new RichTextBox();
                    InRichTextBox[i].Name = name;
                    InRichTextBox[i].Tag = i.ToString();
                    InRichTextBox[i].MouseUp += LyricsRichTextBox_MouseUp;
                    InRichTextBox[i].MouseWheel += LyricsRichTextBox_MouseWheel;
                    InRichTextBox[i].KeyUp += LyricsRichTextBox_KeyUp;
                    InRichTextBox[i].Dock = DockStyle.Top;
                    InRichTextBox[i].ScrollBars = RichTextBoxScrollBars.None;
                    InRichTextBox[i].ReadOnly = true;
                    InRichTextBox[i].BorderStyle = BorderStyle.None;
                    InRichTextBox[i].Font = lyricsFont;
                }
                else
                {
                    InRichTextBox[i].Font = lyricsFont;
                    InRichTextBox[i].BorderStyle = BorderStyle.None;
                }
                InPanel.Controls.Add(InRichTextBox[i]);
                InRichTextBox[i].BringToFront();
                if (Reset)
                {
                    InRichTextBox[i].Text = "";
                    InRichTextBox[i].Height = InRichTextBox[i].Font.Height;
                    if (SetFocus)
                    {
                        InRichTextBox[i].Focus();
                    }
                    return;
                }
            }
            GetLyricsSelectionColours(InPanel, out Color selectedText, out Color selectedBack);
            Gf.DisplayRichTextBoxSeries(ref InItem, ref InPanel, ref InRichTextBox, ScrollToCaret: true, Gf.PreviewArea_ShowNotations, selectedText, selectedBack);
            for (int i = 1; i <= boxCount; i++)
            {
                InRichTextBox[i].BorderStyle = (Gf.PreviewArea_LineBetweenScreens ? BorderStyle.FixedSingle : BorderStyle.None);
            }
            ResizePreviewRichTextBox();
            ResizeOutputRichTextBox();
        }

        private void LyricsRichTextBox_MouseUp(object sender, MouseEventArgs e)
        {
            RichTextBox richTextBox = (RichTextBox)sender;
            richTextBox.SelectionStart = 0;
            richTextBox.SelectionLength = 0;
            int newSlideNumber = DataUtil.StringToInt((string)richTextBox.Tag);
            if (richTextBox.Name == "Output")
            {
                LyricsRichTextBox_ShowSlide(Gf.OutputItem, newSlideNumber);
            }
            else
            {
                LyricsRichTextBox_ShowSlide(Gf.PreviewItem, newSlideNumber);
                flowLayoutPreviewLyrics.Focus();
            }
            Debug.WriteLine($"LyricsRichTextBox_MouseUp name={richTextBox.Name} focus={ActiveControl?.Name}");
        }

        private void LyricsRichTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            Keys keyCode = e.KeyCode;
            RichTextBox richTextBox = (RichTextBox)sender;
            if (richTextBox.Name == "Output")
            {
                ItemKeyPressed(Gf.OutputItem, keyCode, e.Shift);
                FocusOutputArea();
            }
            else
            {
                ItemKeyPressed(Gf.PreviewItem, keyCode, e.Shift);
            }
        }

        private void LyricsRichTextBox_MouseWheel(object sender, MouseEventArgs e)
        {
            RichTextBox richTextBox = (RichTextBox)sender;
            int scrollY = -(e.Delta / 3);
            if (richTextBox.Name == "Output")
            {
                scrollY -= flowLayoutOutputLyrics.AutoScrollPosition.Y;
                if (scrollY < 0)
                {
                    scrollY = 0;
                }
                flowLayoutOutputLyrics.AutoScrollPosition = new Point(0, scrollY);
            }
            else
            {
                scrollY -= flowLayoutPreviewLyrics.AutoScrollPosition.Y;
                if (scrollY < 0)
                {
                    scrollY = 0;
                }
                flowLayoutPreviewLyrics.AutoScrollPosition = new Point(0, scrollY);
            }
        }

        private void LyricsRichTextBox_ShowSlide(SongSettings InItem, int NewSlideNumber)
        {
            if (InItem.OutputStyleScreen)
            {
                OutputRichTextBox_ShowSlide(NewSlideNumber);
            }
            else
            {
                PreviewRichTextBox_ShowSlide(NewSlideNumber);
            }
        }

        private void OutputRichTextBox_ShowSlide(int NewSlideNumber)
        {
            Gf.OutputItem.CurSlide = NewSlideNumber;
            if (ImplementSlideMove(Gf.OutputItem, ScrollToTop: false) && Gf.ShowRunning)
            {
                RemoteControlLiveShow(LiveShowAction.Remote_SlideChanged);
            }
            FocusOutputArea();
        }

        private void PreviewRichTextBox_ShowSlide(int NewSlideNumber)
        {
            Gf.PreviewItem.CurSlide = NewSlideNumber;
            ImplementSlideMove(Gf.PreviewItem, ScrollToTop: false);
        }

        private void HighlightPreviewRichTextBox(bool OnEnter, bool ScrollToTop)
        {
            GetLyricsSelectionColours(flowLayoutPreviewLyrics, out Color selectedText, out Color selectedBack);
            Gf.HighlightRichTextBox(ref Lyrics_PreviewBox, ref flowLayoutPreviewLyrics, Gf.PreviewItem, OnEnter, ScrollToTop, selectedText, selectedBack);
        }

        private void HighlightOutputRichTextBox(bool OnEnter, bool ScrollToTop)
        {
            GetLyricsSelectionColours(flowLayoutOutputLyrics, out Color selectedText, out Color selectedBack);
            Gf.HighlightRichTextBox(ref Lyrics_OutputBox, ref flowLayoutOutputLyrics, Gf.OutputItem, OnEnter, ScrollToTop, selectedText, selectedBack);
            ShowStatusBarSummary();
        }

        private void ResizePreviewRichTextBox()
        {
            ResizeLyricsRichTextBox(ref Lyrics_PreviewBox, ref flowLayoutPreviewLyrics, Gf.PreviewItem);
        }

        private void ResizeOutputRichTextBox()
        {
            ResizeLyricsRichTextBox(ref Lyrics_OutputBox, ref flowLayoutOutputLyrics, Gf.OutputItem);
        }

        private void ResizeLyricsRichTextBox(ref RichTextBox[] InRichTextBox, ref Panel InPanel, SongSettings InItem)
        {
            for (int i = 1; i <= InItem.TotalSlides; i++)
            {
                if (InRichTextBox[i] != null)
                {
                    if (InRichTextBox[i].TextLength == 0)
                    {
                        InRichTextBox[i].Height = 5;
                        continue;
                    }
                    Point positionFromCharIndex = InRichTextBox[i].GetPositionFromCharIndex(InRichTextBox[i].TextLength - 1);
                    InRichTextBox[i].Height = positionFromCharIndex.Y + InRichTextBox[i].Font.Height * 2;
                }
            }
        }

        private void flowLayoutOutputLyrics_Click(object sender, EventArgs e)
        {
            if (Gf.OutputItem.TotalSlides > 0)
            {
                LyricsRichTextBox_ShowSlide(Gf.OutputItem, Gf.OutputItem.TotalSlides);
                HighlightOutputRichTextBox(OnEnter: true, ScrollToTop: false);
            }
        }

        private void flowLayoutPreviewLyrics_Click(object sender, EventArgs e)
        {
            if (Gf.PreviewItem.TotalSlides > 0)
            {
                LyricsRichTextBox_ShowSlide(Gf.PreviewItem, Gf.PreviewItem.TotalSlides);
                HighlightPreviewRichTextBox(OnEnter: true, ScrollToTop: false);
            }
            flowLayoutPreviewLyrics.Focus();
            Debug.WriteLine($"flowLayoutPreviewLyrics_Click focus={ActiveControl?.Name}");
        }

        private void flowLayoutPreviewLyrics_Leave(object sender, EventArgs e)
        {
            HighlightPreviewRichTextBox(OnEnter: false, ScrollToTop: false);
        }

        private void flowLayoutPreviewLyrics_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == Keys.Up || e.KeyCode == Keys.Down)
            {
                e.IsInputKey = true;
            }
            Debug.WriteLine($"flowLayoutPreviewLyrics_PreviewKeyDown key={e.KeyCode} input={e.IsInputKey} focus={ActiveControl?.Name}");
        }

        private void flowLayoutPreviewLyrics_KeyUp(object sender, KeyEventArgs e)
        {
            Keys keyCode = e.KeyCode;
            ItemKeyPressed(Gf.PreviewItem, keyCode, e.Shift);
            Debug.WriteLine($"flowLayoutPreviewLyrics_KeyUp key={e.KeyCode} shift={e.Shift} focus={ActiveControl?.Name}");
        }

        private void flowLayoutOutputLyrics_Leave(object sender, EventArgs e)
        {
            HighlightOutputRichTextBox(OnEnter: false, ScrollToTop: false);
        }

        private void OutputTextBoxLM_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                SendLyricsMonitorMessage();
            }
        }

        private void OutputTextBoxLM_Enter(object sender, EventArgs e)
        {
            if (Gf.UseFocusedTextRegionColour)
            {
                Color InBackground = OutputTextBoxLM.BackColor;
                Gf.SetEnterColour(ref InBackground);
                OutputTextBoxLM.BackColor = InBackground;
            }
            OutputTextBoxLM.SelectAll();
        }

        private void OutputTextBoxLM_Leave(object sender, EventArgs e)
        {
            Color InBackground = OutputTextBoxLM.BackColor;
            Gf.SetLeaveColor(ref InBackground);
            OutputTextBoxLM.BackColor = InBackground;
        }

        private void OutputBtnLMSend_Click(object sender, EventArgs e)
        {
            SendLyricsMonitorMessage();
        }

        private void OutputBtnLMClear_Click(object sender, EventArgs e)
        {
            OutputTextBoxLM.Text = "";
            SendLyricsMonitorMessage();
        }

        private void SendLyricsMonitorMessage()
        {
            Gf.LyricsAlertDetails = DataUtil.Trim(OutputTextBoxLM.Text);
            OutputTextBoxLM.Text = DataUtil.Trim(OutputTextBoxLM.Text);
            AlertWindow_OnMessage(2, "");
        }

        private void OutputInfo_KeyUp(object sender, KeyEventArgs e)
        {
            Keys keyCode = e.KeyCode;
            ItemKeyPressed(Gf.OutputItem, keyCode, e.Shift);
        }

        private void PreviewInfo_KeyUp(object sender, KeyEventArgs e)
        {
            Keys keyCode = e.KeyCode;
            ItemKeyPressed(Gf.PreviewItem, keyCode, e.Shift);
            Debug.WriteLine($"PreviewInfo_KeyUp key={e.KeyCode} shift={e.Shift} focus={ActiveControl?.Name}");
        }

        internal void GotoNextNonRotateItem(SongSettings InItem)
        {
            SaveWorshipList();
            int nextNonRotateItem = Gf.GetNextNonRotateItem((InItem.Type == "G") ? true : false);
            bool loadGapItem = false;
            if (Gf.GapItemOption == GapType.None)
            {
                if (nextNonRotateItem != Gf.StartPresAt)
                {
                    ManualMoveToItem(InItem, nextNonRotateItem);
                }
            }
            else if (nextNonRotateItem == Gf.StartPresAt)
            {
                if (InItem.Type != "G")
                {
                    loadGapItem = true;
                }
            }
            else if (nextNonRotateItem > 1)
            {
                loadGapItem = true;
            }
            if (loadGapItem)
            {
                Gf.StartPresAt = nextNonRotateItem;
                InItem.CurItemNo = Gf.StartPresAt;
                Gf.Launch_StartPresAt = Gf.StartPresAt;
                LoadItem(ref InItem, "G1", "", 0);
                if (Gf.ShowRunning)
                {
                    Gf.MainAction_SongChanged_Transaction = ImageTransitionControl.TransitionAction.AsStored;
                    Gf.MainAction_MoveToItemKeyDirection = KeyDirection.Refresh;
                    RemoteControlLiveShow(LiveShowAction.Remote_SongJumpTo);
                }
            }
            FocusOutputArea();
            UpdateWorshipShowIcons();
        }

        private void SaveInfoFilePreview(bool ReloadImageData)
        {
            if (Gf.PreviewItem.Type != "I")
            {
                return;
            }
            string songSequence = Gf.PreviewItem.SongSequence;
            Gf.PreviewItem.SongSequence = Gf.PreviewItem.SongOriginalLoadedSequence;
            if (Gf.SaveXMLInfoScreen(Gf.PreviewItem, Gf.PreviewItem.ItemID, null, ReloadImageData, UseOriginalNotations: true) && Gf.FormInUse("InfoScreen"))
            {
                Gf.InfoScreenAction = InfoType.FormatStringUpdate;
                Gf.InfoScreenFileName = Gf.PreviewItem.ItemID;
                Gf.InfoScreenNewFormatString = Gf.PreviewItem.Format.FormatString;
                Gf.InfoScreenBackgroundPicture = Gf.PreviewItem.Format.BackgroundPicture;
                if (ReloadImageData)
                {
                    Gf.InfoScreenLoadNewBackground = true;
                }
                Gf.InfoScreen_RequestReceived = true;
            }
            Gf.PreviewItem.SongSequence = songSequence;
        }

        private void Ind_checkBox_Click(object sender, EventArgs e)
        {
            Ind_checkBox_Action();
        }

        private void Ind_checkBox_Action()
        {
            if (Ind_checkBox.Checked)
            {
                Gf.PreviewItem.Format.BackgroundPicture = Gf.BackgroundPicture;
                Gf.PreviewItem.Format.BackgroundMode = Gf.BackgroundMode;
                Ind_NoImage.Enabled = ((Gf.PreviewItem.Format.BackgroundPicture != "") ? true : false);
                Gf.LoadIndividualFormatData(ref Gf.PreviewItem, GetNewFormatString());
                AllowIndividualFormat(AllowFormat: true, BoxChecked: true);
                if (Gf.PreviewItem.Source == ItemSource.WorshipList)
                {
                    int selectedIndex = Gf.GetSelectedIndex(WorshipListItems);
                    if (selectedIndex >= 0)
                    {
                        WorshipListItems.Items[selectedIndex].SubItems[2].Text = Gf.PreviewItem.Format.FormatString;
                    }
                    SaveWorshipList();
                }
                else if (Gf.PreviewItem.Source == ItemSource.SongsList)
                {
                    Gf.SaveFormatStringToDatabase(Gf.PreviewItem.ItemID, Gf.PreviewItem.Format.FormatString);
                }
            }
            else
            {
                string formatString = Gf.PreviewItem.Format.FormatString;
                ClearFormatPicture();
                NoIndividualFormat();
                if (Gf.PreviewItem.Source == ItemSource.WorshipList)
                {
                    if (formatString != Gf.PreviewItem.Format.FormatString)
                    {
                        WorshipListIndexChanged();
                        if (Gf.OutputItem.ItemID != "" && Gf.PreviewItem.ItemID == Gf.OutputItem.ItemID)
                        {
                            CopyPreviewToOutput();
                        }
                    }
                }
                else if (Gf.PreviewItem.Source == ItemSource.SongsList)
                {
                    Gf.SaveFormatStringToDatabase(Gf.PreviewItem.ItemID, "");
                }
            }
            SaveInfoFilePreview(ReloadImageData: true);
        }

        private void Menu_BlackScreen_Click(object sender, EventArgs e)
        {
            LiveBlack(!Gf.ShowLiveBlack);
        }

        /// <summary>
        /// OutPreview Slide Up Slide Down Global Hook Arrow Up Arrow Down
        /// </summary>
        public void AddHookSlideUpDown()
        {
            try
            {
                if (Gf.GlobalHookKey_Arrow || Gf.GlobalHookKey_CtrlArrow)
                {
                    HookManager.KeyUp += HookManager_KeyUp;
                }
            }
            catch { }
        }

        public void RemoveHookSlideUpDown()
        {
            try
            {
                HookManager.KeyUp -= HookManager_KeyUp;
            }
            catch { }
        }

        /// <summary>
        /// Balck Screen Global Hook F9 F10
        /// </summary>
        public void AddHookBlackScreen()
        {
            try
            {
                if (Gf.GlobalHookKey_F7 || Gf.GlobalHookKey_F8 || Gf.GlobalHookKey_F9 || Gf.GlobalHookKey_F10)
                {
                    HookManager.KeyDown += HookManager_KeyDown;
                }
            }
            catch { }
        }

        public void RemoveHookBlackScreen()
        {
            try
            {
                HookManager.KeyDown -= HookManager_KeyDown;
            }
            catch { }
        }

        public void UpdateBlackScreenShortcut()
        {
            // Prevent double-toggle when global black-screen hotkeys are enabled.
            Menu_BlackScreen.ShortcutKeys = (Gf.GlobalHookKey_F9 || Gf.GlobalHookKey_F10) ? Keys.None : Keys.F9;
            // The global key options may have changed: keep the button tooltips' keys in step.
            RefreshShortcutTooltips();
        }

        #region Event handlers of particular events. They will be activated when an appropriate checkbox is checked.
        private void HookManager_KeyDown(object sender, KeyEventArgs e)
        {
            frmMain.BeginInvoke(new Action(() =>
            {
                _keyboardHandler.HandleGlobalFunctionKey(e.KeyCode);
                if (e.KeyCode == Keys.F9 || e.KeyCode == Keys.F10)
                {
                    e.Handled = true;
                }
            }));
        }

        private void HookManager_KeyUp(object sender, KeyEventArgs e)
        {
            frmMain.BeginInvoke(new Action(() =>
            {
                _keyboardHandler.HandleGlobalArrowKey(e.KeyCode, HookManager.Control);
            }));
        }
        #endregion

        private void Main_QuickFind_Click(object sender, EventArgs e)
        {

        }

    }
}
