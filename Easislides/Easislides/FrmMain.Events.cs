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
        private void Form1_Resize(object sender, EventArgs e)
        {
            if (FormFirstLoad && base.WindowState == FormWindowState.Maximized)
            {
                ApplySplitterValues();
                FormFirstLoad = false;
            }
        }

        private void LiveShow_OnMessage(int MsgCode, string MsgString)
        {
            switch (MsgCode)
            {
                case 7:
                    Remote_SongChanged();
                    break;
                case 9:
                    Remote_SlideChanged();
                    break;
                case 8:
                    Remote_MovedToGapItem();
                    break;
                case 10:
                    Remote_EndShow();
                    break;
            }
        }

        private void AlertWindow_OnMessage(int MsgCode, string MsgString)
        {
            switch (MsgCode)
            {
                case 0:
                    Gf.MessageAlertRequested = true;
                    if (!Gf.ShowRunning)
                    {
                        GoLive(InStatus: true);
                    }
                    else
                    {
                        RemoteControlLiveShow(LiveShowAction.Remote_MessageAlertRequested);
                    }
                    break;
                case 1:
                    Gf.ParentalAlertRequested = true;
                    if (!Gf.ShowRunning)
                    {
                        GoLive(InStatus: true);
                    }
                    else
                    {
                        RemoteControlLiveShow(LiveShowAction.Remote_ParentalAlertRequested);
                    }
                    break;
                case 2:
                    Gf.LyricsAlertRequested = true;
                    if (Gf.ShowRunning)
                    {
                        RemoteControlLiveShow(LiveShowAction.Remote_LyricsAlertShow);
                    }
                    break;
            }
        }

        private void panelPreviewTop_Resize(object sender, EventArgs e)
        {
            PreviewPanelDisplayName.Columns[0].Width = PreviewPanelDisplayName.Width - 5;
            int num = panelPreviewTop.Width - IndgroupBox1.Width;
            num = ((num > 0) ? (num / 2) : 0);
            IndgroupBox1.Left = num;
            IndgroupBox2.Left = num;
            IndgroupBox3.Left = num;
            IndgroupBox4.Left = num;
            Ind_checkBox.Left = num;
            panelIndTemplate.Left = Ind_checkBox.Left + Ind_checkBox.Width + 4;
            ResizePreviewRichTextBox();
            if (flowLayoutPreviewPowerPoint.Visible)
            {
                SetPreviewPPThumbImages1(Gf.PreviewItem.CurSlide);
            }
        }

        private void panelPreviewBottom_Resize(object sender, EventArgs e)
        {
            ResizeSampleScreen(panelPreviewBottom, PreviewHolder, PreviewBack, AdjustForIndicator: false);
        }

        private void panelOutputTop_Resize(object sender, EventArgs e)
        {
            OutputPanelDisplayName.Columns[0].Width = OutputPanelDisplayName.Width - 5;
            ResizeOutputRichTextBox();
            if (flowLayoutOutputPowerPoint.Visible)
            {
                SetOutputPPThumbImages1(Gf.OutputItem.CurSlide);
            }
        }

        private void panelOutputBottom_Resize(object sender, EventArgs e)
        {
            ResizeOutputBottomPanel();
        }

        private void IndradioButtonTextFormatInfo_Click(object sender, EventArgs e)
        {
            SetPreviewAreas();
        }

        private void IndcbPreviewNotes_Click(object sender, EventArgs e)
        {
            SetNotesPreview(IndcbPreviewNotes.Checked);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!ForceFormClose)
            {
                try
                {
                    string filePath = Gf.WorshipDir + Gf.CurSession + ".esw";
                    int itemCountInFile = -1;
                    string lastWriteTime = "N/A";
                    try
                    {
                        if (System.IO.File.Exists(filePath))
                        {
                            string fileContent = System.IO.File.ReadAllText(filePath);
                            itemCountInFile = System.Text.RegularExpressions.Regex.Matches(fileContent, "<Item>").Count;
                            lastWriteTime = System.IO.File.GetLastWriteTime(filePath).ToString("yyyy-MM-dd HH:mm:ss.fff");
                        }
                    }
                    catch { }
                    WriteDebugLog($"=== FORM CLOSING === File: {filePath}, Items in file: {itemCountInFile}, Items in ListView: {WorshipListItems.Items.Count}, File last modified: {lastWriteTime}, UpdatingFormatFields: {UpdatingFormatFields}");

                    RemoveHookBlackScreen();
                    RemoveHookSlideUpDown();

                    if (SaveToRegistryOnClosing)
                    {
                        SaveFormStateToRegistry();
                    }
                    //daniel
                    //???? ??????????????
                    Gf.ClearUpPowerpointWindows();
                    gfFileHelpers.DeleteFolderFilesSafe(Gf.EasiSlidesTempDir);

                    WriteDebugLog("=== FORM CLOSED SUCCESSFULLY ===");
                }
                catch
                {
                }
            }
        }

        //private void tabControlSource_Resize(object sender, EventArgs e)
        //{
        //    if (Gf.formLoaded)
        //    {
        //        UpdateDisplayPanelData(RefreshSlides: false);
        //    }
        //}

        private void Def_Media_Clicked()
        {
            FrmMediaPlayerControl frmMediaPlayerControl = new FrmMediaPlayerControl();
            Gf.MPC_Type = MPCType.Session;
            Gf.Temp_MediaTitle1 = "";
            Gf.Temp_MediaTitle2 = "";
            Gf.Temp_MediaOption = Gf.MediaOption;
            Gf.Temp_MediaLocation = Gf.MediaLocation;
            Gf.Temp_MediaCaptureDeviceNumber = Gf.MediaCaptureDeviceNumber;
            Gf.Temp_MediaOutputMonitorName = Gf.MediaOutputMonitorName;
            Gf.Temp_MediaVolume = Gf.MediaVolume;
            Gf.Temp_MediaBalance = Gf.MediaBalance;
            Gf.Temp_MediaMute = Gf.MediaMute;
            Gf.Temp_MediaRepeat = Gf.MediaRepeat;
            Gf.Temp_MediaWidescreen = Gf.MediaWidescreen;
            if (frmMediaPlayerControl.ShowDialog() == DialogResult.OK)
            {
                Gf.MediaOption = Gf.Temp_MediaOption;
                Gf.MediaLocation = Gf.Temp_MediaLocation;
                Gf.MediaCaptureDeviceNumber = Gf.Temp_MediaCaptureDeviceNumber;
                Gf.MediaOutputMonitorName = Gf.Temp_MediaOutputMonitorName;
                Gf.MediaVolume = Gf.Temp_MediaVolume;
                Gf.MediaBalance = Gf.Temp_MediaBalance;
                Gf.MediaMute = Gf.Temp_MediaMute;
                Gf.MediaRepeat = Gf.Temp_MediaRepeat;
                Gf.MediaWidescreen = Gf.Temp_MediaWidescreen;
                AssignMediaText(ref Def_AssignMedia, Gf.MediaOption);
                ApplyDefaultData(StartAtFirstSlide: false);
            }
        }

        private void Def_Region_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            Gf.AssignDropDownItem(ref Def_Region, e.ClickedItem.Name, Def_ShowRegion1, Def_ShowRegion2, Def_ShowRegionBoth);
            Gf.ShowLyrics = DataUtil.ObjToInt(Def_Region.Tag);
            ApplyDefaultData(StartAtFirstSlide: false);
        }

        private void Def_VAlign_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            Gf.AssignDropDownItem(ref Def_VAlign, e.ClickedItem.Name, Def_VAlignTop, Def_VAlignCentre, Def_VAlignBottom);
            Gf.ShowVerticalAlign = DataUtil.ObjToInt(Def_VAlign.Tag);
            ApplyDefaultData(StartAtFirstSlide: false);
        }

        private void Def_R1Align_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            Gf.AssignDropDownItem(ref Def_R1Align, e.ClickedItem.Name, Def_R1AlignLeft, Def_R1AlignCentre, Def_R1AlignRight);
            Gf.ShowFontAlign[0, 0] = DataUtil.ObjToInt(Def_R1Align.Tag);
            ApplyDefaultData(StartAtFirstSlide: false);
        }

        private void Def_R2Align_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            Gf.AssignDropDownItem(ref Def_R2Align, e.ClickedItem.Name, Def_R2AlignLeft, Def_R2AlignCentre, Def_R2AlignRight);
            Gf.ShowFontAlign[0, 1] = DataUtil.ObjToInt(Def_R2Align.Tag);
            ApplyDefaultData(StartAtFirstSlide: false);
        }

        private void Def_ImageMode_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            Gf.AssignDropDownItem(ref Def_ImageMode, e.ClickedItem.Name, Def_ImageTile, Def_ImageCentre, Def_ImageBestFit);
            Gf.BackgroundMode = (ImageMode)DataUtil.ObjToInt(Def_ImageMode.Tag);
            ApplyDefaultData(StartAtFirstSlide: false);
        }

        private void Def_TransSelectedIndexChanged(object sender, EventArgs e)
        {
            if (!(UpdatingDefaultFields | InitFormLoad))
            {
                ToolStripComboBox toolStripComboBox = (ToolStripComboBox)sender;
                ImageTransitionControl.TransitionAction transitionAction = (toolStripComboBox.Name == "Def_TransItem") ? ImageTransitionControl.TransitionAction.AsStoredItem : ImageTransitionControl.TransitionAction.AsStoredSlide;
                Gf.ShowItemTransition = Def_TransItem.SelectedIndex;
                Gf.ShowSlideTransition = Def_TransSlides.SelectedIndex;
                ApplyDefaultData(StartAtFirstSlide: false, transitionAction);
            }
        }

        private void Ind_Items_MouseUp(object sender, MouseEventArgs e)
        {
            ToolStripButton toolStripButton = (ToolStripButton)sender;
            string name = toolStripButton.Name;
            if (name == "Ind_LoadTemplate")
            {
                if (Gf.PreviewItem.ItemID != "")
                {
                    LoadIndividualTemplate();
                }
            }
            else if (name == "Ind_SaveTemplate")
            {
                if (Gf.PreviewItem.ItemID != "")
                {
                    SaveIndividualTemplate(Gf.PreviewItem.Title);
                }
            }
            else if (name == "Ind_Shadow")
            {
                Gf.PreviewItem.Format.UseShadowFont = (toolStripButton.Checked ? 1 : 0);
                UpdateFormatData(StartAtFirstSlide: false);
            }
            else if (name == "Ind_Outline")
            {
                Gf.PreviewItem.Format.UseOutlineFont = (toolStripButton.Checked ? 1 : 0);
                UpdateFormatData(StartAtFirstSlide: false);
            }
            else if (name == "Ind_Interlace")
            {
                Gf.PreviewItem.Format.ShowInterlace = (toolStripButton.Checked ? 1 : 0);
                UpdateFormatData(StartAtFirstSlide: false);
            }
            else if (name == "Ind_Notations")
            {
                Gf.PreviewItem.Format.ShowNotations = (toolStripButton.Checked ? 1 : 0);
                UpdateFormatData(StartAtFirstSlide: true);
            }
            else if (name == "Ind_CapoDown")
            {
                Gf.PreviewItem.Format.PreviousTransposeOffset = Gf.PreviewItem.Format.TransposeOffset;
                Gf.PreviewItem.Format.TransposeOffset = Gf.IncrementChord(ref Gf.PreviewItem.Format.TransposeOffset, -1);
                UpdateFormatData(StartAtFirstSlide: false);
            }
            else if (name == "Ind_CapoUp")
            {
                Gf.PreviewItem.Format.PreviousTransposeOffset = Gf.PreviewItem.Format.TransposeOffset;
                Gf.PreviewItem.Format.TransposeOffset = Gf.IncrementChord(ref Gf.PreviewItem.Format.TransposeOffset, 1);
                UpdateFormatData(StartAtFirstSlide: false);
            }
            else if (name == "Ind_NoImage")
            {
                ClearFormatPicture();
            }
            else if (name == "Ind_BackColour")
            {
                // SongFormat 색상은 ARGB int 로 보관 → Color 로 풀어 대화상자에 넘기고, 성공 시 다시 int 로 저장.
                Color back0 = Color.FromArgb(Gf.PreviewItem.Format.ShowScreenColour[0]);
                Color back1 = Color.FromArgb(Gf.PreviewItem.Format.ShowScreenColour[1]);
                if (Gf.SelectBackgroundColors(ref Ind_BackColour, ref back0, ref back1, ref Gf.PreviewItem.Format.ShowScreenStyle, IsDefault: false))
                {
                    Gf.PreviewItem.Format.ShowScreenColour[0] = back0.ToArgb();
                    Gf.PreviewItem.Format.ShowScreenColour[1] = back1.ToArgb();
                    ClearFormatPicture();
                }
            }
            else if (name == "Ind_AssignMedia")
            {
                Ind_Media_Clicked();
            }
            else if (name == "Ind_HideDisplayPanel")
            {
                Gf.PreviewItem.Format.HideDisplayPanel = (toolStripButton.Checked ? 1 : 0);
                UpdateFormatData(StartAtFirstSlide: false);
            }
        }

        private void Ind_Media_Clicked()
        {
            FrmMediaPlayerControl frmMediaPlayerControl = new FrmMediaPlayerControl();
            Gf.MPC_Type = MPCType.Individual;
            Gf.Temp_MediaItemType = Gf.PreviewItem.Type;
            Gf.Temp_MediaTitle1 = Gf.PreviewItem.Title;
            Gf.Temp_MediaTitle2 = Gf.PreviewItem.Title2;
            Gf.Temp_MediaOption = Gf.PreviewItem.Format.MediaOption;
            Gf.Temp_MediaLocation = Gf.PreviewItem.Format.MediaLocation;
            Gf.Temp_MediaCaptureDeviceNumber = Gf.PreviewItem.Format.MediaCaptureDeviceNumber;
            Gf.Temp_MediaOutputMonitorName = Gf.PreviewItem.Format.MediaOutputMonitorName;
            Gf.Temp_MediaVolume = Gf.PreviewItem.Format.MediaVolume;
            Gf.Temp_MediaBalance = Gf.PreviewItem.Format.MediaBalance;
            Gf.Temp_MediaMute = Gf.PreviewItem.Format.MediaMute;
            Gf.Temp_MediaRepeat = Gf.PreviewItem.Format.MediaRepeat;
            Gf.Temp_MediaWidescreen = Gf.PreviewItem.Format.MediaWidescreen;
            if (frmMediaPlayerControl.ShowDialog() == DialogResult.OK)
            {
                Gf.PreviewItem.Format.MediaOption = Gf.Temp_MediaOption;
                Gf.PreviewItem.Format.MediaLocation = Gf.Temp_MediaLocation;
                Gf.PreviewItem.Format.MediaCaptureDeviceNumber = Gf.Temp_MediaCaptureDeviceNumber;
                Gf.PreviewItem.Format.MediaOutputMonitorName = Gf.Temp_MediaOutputMonitorName;
                Gf.PreviewItem.Format.MediaVolume = Gf.Temp_MediaVolume;
                Gf.PreviewItem.Format.MediaBalance = Gf.Temp_MediaBalance;
                Gf.PreviewItem.Format.MediaMute = Gf.Temp_MediaMute;
                Gf.PreviewItem.Format.MediaRepeat = Gf.Temp_MediaRepeat;
                Gf.PreviewItem.Format.MediaWidescreen = Gf.Temp_MediaWidescreen;
                AssignMediaText(ref Ind_AssignMedia, Gf.PreviewItem.Format.MediaOption);
                UpdateFormatData(StartAtFirstSlide: false);
            }
        }

        private void Ind_RegionsFormat_MouseUp(object sender, MouseEventArgs e)
        {
            ToolStripButton toolStripButton = (ToolStripButton)sender;
            bool @checked = toolStripButton.Checked;
            string name = toolStripButton.Name;
            if (name == "Ind_R1Bold")
            {
                Gf.PreviewItem.Format.ShowFontBold[0] = (@checked ? 1 : 0);
                UpdateFormatData();
            }
            else if (name == "Ind_R1Underline")
            {
                Gf.PreviewItem.Format.ShowFontUnderline[0] = (@checked ? 1 : 0);
                UpdateFormatData();
            }
            else if (name == "Ind_R1Colour")
            {
                // SongFormat 글자색은 ARGB int → Color 로 풀어 선택, 성공 시 다시 int 로 저장.
                Color r1Colour = Color.FromArgb(Gf.PreviewItem.Format.ShowFontColour[0]);
                if (Gf.SelectColorFromBtn(ref Ind_R1Colour, ref r1Colour))
                {
                    Gf.PreviewItem.Format.ShowFontColour[0] = r1Colour.ToArgb();
                    UpdateFormatData(StartAtFirstSlide: false);
                }
            }
            else if (name == "Ind_R2Bold")
            {
                Gf.PreviewItem.Format.ShowFontBold[1] = (@checked ? 1 : 0);
                UpdateFormatData();
            }
            else if (name == "Ind_R2Underline")
            {
                Gf.PreviewItem.Format.ShowFontUnderline[1] = (@checked ? 1 : 0);
                UpdateFormatData();
            }
            else if (name == "Ind_R2Colour")
            {
                // SongFormat 글자색은 ARGB int → Color 로 풀어 선택, 성공 시 다시 int 로 저장.
                Color r2Colour = Color.FromArgb(Gf.PreviewItem.Format.ShowFontColour[1]);
                if (Gf.SelectColorFromBtn(ref Ind_R2Colour, ref r2Colour))
                {
                    Gf.PreviewItem.Format.ShowFontColour[1] = r2Colour.ToArgb();
                    UpdateFormatData(StartAtFirstSlide: false);
                }
            }
        }

        private void Ind_R1Italics_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            Gf.AssignDropDownItem(ref Ind_R1Italics, e.ClickedItem.Name, Ind_R1Italics0, Ind_R1Italics1, Ind_R1Italics2);
            int num = DataUtil.ObjToInt(Ind_R1Italics.Tag);
            int num2 = num;
            if (num2 == 2)
            {
                Gf.PreviewItem.Format.ShowFontItalic[0] = 0;
                Gf.PreviewItem.Format.ShowFontItalic[2] = 1;
            }
            else
            {
                Gf.PreviewItem.Format.ShowFontItalic[0] = num;
                Gf.PreviewItem.Format.ShowFontItalic[2] = num;
            }
            UpdateFormatData();
        }

        private void Ind_R2Italics_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            Gf.AssignDropDownItem(ref Ind_R2Italics, e.ClickedItem.Name, Ind_R2Italics0, Ind_R2Italics1, Ind_R2Italics2);
            int num = DataUtil.ObjToInt(Ind_R2Italics.Tag);
            int num2 = num;
            if (num2 == 2)
            {
                Gf.PreviewItem.Format.ShowFontItalic[1] = 0;
                Gf.PreviewItem.Format.ShowFontItalic[3] = 1;
            }
            else
            {
                Gf.PreviewItem.Format.ShowFontItalic[1] = num;
                Gf.PreviewItem.Format.ShowFontItalic[3] = num;
            }
            UpdateFormatData();
        }

        private void Ind_Region_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            Gf.AssignDropDownItem(ref Ind_Region, e.ClickedItem.Name, Ind_ShowRegion1, Ind_ShowRegion2, Ind_ShowRegionBoth);
            Gf.PreviewItem.Format.ShowLyrics = DataUtil.ObjToInt(Ind_Region.Tag);
            UpdateFormatData(StartAtFirstSlide: false);
        }

        private void Ind_VAlign_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            Gf.AssignDropDownItem(ref Ind_VAlign, e.ClickedItem.Name, Ind_VAlignTop, Ind_VAlignCentre, Ind_VAlignBottom);
            Gf.PreviewItem.Format.ShowVerticalAlign = DataUtil.ObjToInt(Ind_VAlign.Tag);
            UpdateFormatData(StartAtFirstSlide: false);
        }

        private void Ind_ImageMode_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            Gf.AssignDropDownItem(ref Ind_ImageMode, e.ClickedItem.Name, Ind_ImageTile, Ind_ImageCentre, Ind_ImageBestFit);
            Gf.PreviewItem.Format.BackgroundMode = (ImageMode)DataUtil.ObjToInt(Ind_ImageMode.Tag);
            Gf.SetShowBackground(Gf.PreviewItem, ref PreviewScreen);
            UpdateFormatData(StartAtFirstSlide: false);
        }

        private void Ind_R1Align_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            Gf.AssignDropDownItem(ref Ind_R1Align, e.ClickedItem.Name, Ind_R1AlignLeft, Ind_R1AlignCentre, Ind_R1AlignRight);
            Gf.PreviewItem.Format.ShowFontAlign[0] = DataUtil.ObjToInt(Ind_R1Align.Tag);
            UpdateFormatData(StartAtFirstSlide: false);
        }

        private void Ind_R2Align_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            Gf.AssignDropDownItem(ref Ind_R2Align, e.ClickedItem.Name, Ind_R2AlignLeft, Ind_R2AlignCentre, Ind_R2AlignRight);
            Gf.PreviewItem.Format.ShowFontAlign[1] = DataUtil.ObjToInt(Ind_R2Align.Tag);
            UpdateFormatData(StartAtFirstSlide: false);
        }

        private void PraiseBook_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!InitFormLoad)
            {
                PraiseBookList_Change();
            }
        }

        private void PowerPointImage_MouseUp(object sender, MouseEventArgs e)
        {
            if (pptDoubleClickInProgress)
            {
                return;
            }

            // KeyboardActionHandler를 통해 처리
            _keyboardHandler.HandlePowerPointThumbnailClick((Control)sender, e);
        }

        /// <summary>
        /// PowerPoint ImageCanvas KeyUp 이벤트
        /// </summary>
        private void PowerPointImage_KeyUp(object sender, KeyEventArgs e)
        {
            Control control = (Control)sender;
            string panelName = control.Name == "PP_Preview"
                ? "flowLayoutPreviewPowerPoint"
                : "flowLayoutOutputPowerPoint";

            _keyboardHandler.HandlePowerPointContainerKeyUp(panelName, e.KeyCode, e.Shift);
        }

        /// <summary>
        /// PowerPoint Preview FlowLayoutPanel KeyUp 이벤트
        /// </summary>
        private void flowLayoutPreviewPowerPoint_KeyUp(object sender, KeyEventArgs e)
        {
            _keyboardHandler.HandlePowerPointContainerKeyUp("flowLayoutPreviewPowerPoint", e.KeyCode, e.Shift);
        }

        /// <summary>
        /// PowerPoint Output FlowLayoutPanel KeyUp 이벤트
        /// </summary>
        private void flowLayoutOutputPowerPoint_KeyUp(object sender, KeyEventArgs e)
        {
            _keyboardHandler.HandlePowerPointContainerKeyUp("flowLayoutOutputPowerPoint", e.KeyCode, e.Shift);
        }
    }
}
