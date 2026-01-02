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
                    gf.MessageAlertRequested = true;
                    if (!gf.ShowRunning)
                    {
                        GoLive(InStatus: true);
                    }
                    else
                    {
                        RemoteControlLiveShow(LiveShowAction.Remote_MessageAlertRequested);
                    }
                    break;
                case 1:
                    gf.ParentalAlertRequested = true;
                    if (!gf.ShowRunning)
                    {
                        GoLive(InStatus: true);
                    }
                    else
                    {
                        RemoteControlLiveShow(LiveShowAction.Remote_ParentalAlertRequested);
                    }
                    break;
                case 2:
                    gf.LyricsAlertRequested = true;
                    if (gf.ShowRunning)
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
                SetPreviewPPThumbImages1(gf.PreviewItem.CurSlide);
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
                SetOutputPPThumbImages1(gf.OutputItem.CurSlide);
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
                    RemoveHookBlackScreen();
                    RemoveHookSlideUpDown();

                    if (SaveToRegistryOnClosing)
                    {
                        SaveFormStateToRegistry();
                    }
                    //daniel
                    //???? ??????????????
                    gf.ClearUpPowerpointWindows();
                    gfFileHelpers.DeleteFolderFilesSafe(gf.EasiSlidesTempDir);

                }
                catch
                {
                }
            }
        }

        //private void tabControlSource_Resize(object sender, EventArgs e)
        //{
        //    if (gf.formLoaded)
        //    {
        //        UpdateDisplayPanelData(RefreshSlides: false);
        //    }
        //}

        private void Def_Media_Clicked()
        {
            FrmMediaPlayerControl frmMediaPlayerControl = new FrmMediaPlayerControl();
            gf.MPC_Type = MPCType.Session;
            gf.Temp_MediaTitle1 = "";
            gf.Temp_MediaTitle2 = "";
            gf.Temp_MediaOption = gf.MediaOption;
            gf.Temp_MediaLocation = gf.MediaLocation;
            gf.Temp_MediaCaptureDeviceNumber = gf.MediaCaptureDeviceNumber;
            gf.Temp_MediaOutputMonitorName = gf.MediaOutputMonitorName;
            gf.Temp_MediaVolume = gf.MediaVolume;
            gf.Temp_MediaBalance = gf.MediaBalance;
            gf.Temp_MediaMute = gf.MediaMute;
            gf.Temp_MediaRepeat = gf.MediaRepeat;
            gf.Temp_MediaWidescreen = gf.MediaWidescreen;
            if (frmMediaPlayerControl.ShowDialog() == DialogResult.OK)
            {
                gf.MediaOption = gf.Temp_MediaOption;
                gf.MediaLocation = gf.Temp_MediaLocation;
                gf.MediaCaptureDeviceNumber = gf.Temp_MediaCaptureDeviceNumber;
                gf.MediaOutputMonitorName = gf.Temp_MediaOutputMonitorName;
                gf.MediaVolume = gf.Temp_MediaVolume;
                gf.MediaBalance = gf.Temp_MediaBalance;
                gf.MediaMute = gf.Temp_MediaMute;
                gf.MediaRepeat = gf.Temp_MediaRepeat;
                gf.MediaWidescreen = gf.Temp_MediaWidescreen;
                AssignMediaText(ref Def_AssignMedia, gf.MediaOption);
                ApplyDefaultData(StartAtFirstSlide: false);
            }
        }

        private void Def_Region_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            gf.AssignDropDownItem(ref Def_Region, e.ClickedItem.Name, Def_ShowRegion1, Def_ShowRegion2, Def_ShowRegionBoth);
            gf.ShowLyrics = DataUtil.ObjToInt(Def_Region.Tag);
            ApplyDefaultData(StartAtFirstSlide: false);
        }

        private void Def_VAlign_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            gf.AssignDropDownItem(ref Def_VAlign, e.ClickedItem.Name, Def_VAlignTop, Def_VAlignCentre, Def_VAlignBottom);
            gf.ShowVerticalAlign = DataUtil.ObjToInt(Def_VAlign.Tag);
            ApplyDefaultData(StartAtFirstSlide: false);
        }

        private void Def_R1Align_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            gf.AssignDropDownItem(ref Def_R1Align, e.ClickedItem.Name, Def_R1AlignLeft, Def_R1AlignCentre, Def_R1AlignRight);
            gf.ShowFontAlign[0, 0] = DataUtil.ObjToInt(Def_R1Align.Tag);
            ApplyDefaultData(StartAtFirstSlide: false);
        }

        private void Def_R2Align_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            gf.AssignDropDownItem(ref Def_R2Align, e.ClickedItem.Name, Def_R2AlignLeft, Def_R2AlignCentre, Def_R2AlignRight);
            gf.ShowFontAlign[0, 1] = DataUtil.ObjToInt(Def_R2Align.Tag);
            ApplyDefaultData(StartAtFirstSlide: false);
        }

        private void Def_ImageMode_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            gf.AssignDropDownItem(ref Def_ImageMode, e.ClickedItem.Name, Def_ImageTile, Def_ImageCentre, Def_ImageBestFit);
            gf.BackgroundMode = (ImageMode)DataUtil.ObjToInt(Def_ImageMode.Tag);
            ApplyDefaultData(StartAtFirstSlide: false);
        }

        private void Def_TransSelectedIndexChanged(object sender, EventArgs e)
        {
            if (!(UpdatingDefaultFields | InitFormLoad))
            {
                ToolStripComboBox toolStripComboBox = (ToolStripComboBox)sender;
                ImageTransitionControl.TransitionAction transitionAction = (toolStripComboBox.Name == "Def_TransItem") ? ImageTransitionControl.TransitionAction.AsStoredItem : ImageTransitionControl.TransitionAction.AsStoredSlide;
                gf.ShowItemTransition = Def_TransItem.SelectedIndex;
                gf.ShowSlideTransition = Def_TransSlides.SelectedIndex;
                ApplyDefaultData(StartAtFirstSlide: false, transitionAction);
            }
        }

        private void Ind_Items_MouseUp(object sender, MouseEventArgs e)
        {
            ToolStripButton toolStripButton = (ToolStripButton)sender;
            string name = toolStripButton.Name;
            if (name == "Ind_LoadTemplate")
            {
                if (gf.PreviewItem.ItemID != "")
                {
                    LoadIndividualTemplate();
                }
            }
            else if (name == "Ind_SaveTemplate")
            {
                if (gf.PreviewItem.ItemID != "")
                {
                    SaveIndividualTemplate(gf.PreviewItem.Title);
                }
            }
            else if (name == "Ind_Shadow")
            {
                gf.PreviewItem.Format.UseShadowFont = (toolStripButton.Checked ? 1 : 0);
                UpdateFormatData(StartAtFirstSlide: false);
            }
            else if (name == "Ind_Outline")
            {
                gf.PreviewItem.Format.UseOutlineFont = (toolStripButton.Checked ? 1 : 0);
                UpdateFormatData(StartAtFirstSlide: false);
            }
            else if (name == "Ind_Interlace")
            {
                gf.PreviewItem.Format.ShowInterlace = (toolStripButton.Checked ? 1 : 0);
                UpdateFormatData(StartAtFirstSlide: false);
            }
            else if (name == "Ind_Notations")
            {
                gf.PreviewItem.Format.ShowNotations = (toolStripButton.Checked ? 1 : 0);
                UpdateFormatData(StartAtFirstSlide: true);
            }
            else if (name == "Ind_CapoDown")
            {
                gf.PreviewItem.Format.PreviousTransposeOffset = gf.PreviewItem.Format.TransposeOffset;
                gf.PreviewItem.Format.TransposeOffset = gf.IncrementChord(ref gf.PreviewItem.Format.TransposeOffset, -1);
                UpdateFormatData(StartAtFirstSlide: false);
            }
            else if (name == "Ind_CapoUp")
            {
                gf.PreviewItem.Format.PreviousTransposeOffset = gf.PreviewItem.Format.TransposeOffset;
                gf.PreviewItem.Format.TransposeOffset = gf.IncrementChord(ref gf.PreviewItem.Format.TransposeOffset, 1);
                UpdateFormatData(StartAtFirstSlide: false);
            }
            else if (name == "Ind_NoImage")
            {
                ClearFormatPicture();
            }
            else if (name == "Ind_BackColour")
            {
                if (gf.SelectBackgroundColors(ref Ind_BackColour, ref gf.PreviewItem.Format.ShowScreenColour[0], ref gf.PreviewItem.Format.ShowScreenColour[1], ref gf.PreviewItem.Format.ShowScreenStyle, IsDefault: false))
                {
                    ClearFormatPicture();
                }
            }
            else if (name == "Ind_AssignMedia")
            {
                Ind_Media_Clicked();
            }
            else if (name == "Ind_HideDisplayPanel")
            {
                gf.PreviewItem.Format.HideDisplayPanel = (toolStripButton.Checked ? 1 : 0);
                UpdateFormatData(StartAtFirstSlide: false);
            }
        }

        private void Ind_Media_Clicked()
        {
            FrmMediaPlayerControl frmMediaPlayerControl = new FrmMediaPlayerControl();
            gf.MPC_Type = MPCType.Individual;
            gf.Temp_MediaItemType = gf.PreviewItem.Type;
            gf.Temp_MediaTitle1 = gf.PreviewItem.Title;
            gf.Temp_MediaTitle2 = gf.PreviewItem.Title2;
            gf.Temp_MediaOption = gf.PreviewItem.Format.MediaOption;
            gf.Temp_MediaLocation = gf.PreviewItem.Format.MediaLocation;
            gf.Temp_MediaCaptureDeviceNumber = gf.PreviewItem.Format.MediaCaptureDeviceNumber;
            gf.Temp_MediaOutputMonitorName = gf.PreviewItem.Format.MediaOutputMonitorName;
            gf.Temp_MediaVolume = gf.PreviewItem.Format.MediaVolume;
            gf.Temp_MediaBalance = gf.PreviewItem.Format.MediaBalance;
            gf.Temp_MediaMute = gf.PreviewItem.Format.MediaMute;
            gf.Temp_MediaRepeat = gf.PreviewItem.Format.MediaRepeat;
            gf.Temp_MediaWidescreen = gf.PreviewItem.Format.MediaWidescreen;
            if (frmMediaPlayerControl.ShowDialog() == DialogResult.OK)
            {
                gf.PreviewItem.Format.MediaOption = gf.Temp_MediaOption;
                gf.PreviewItem.Format.MediaLocation = gf.Temp_MediaLocation;
                gf.PreviewItem.Format.MediaCaptureDeviceNumber = gf.Temp_MediaCaptureDeviceNumber;
                gf.PreviewItem.Format.MediaOutputMonitorName = gf.Temp_MediaOutputMonitorName;
                gf.PreviewItem.Format.MediaVolume = gf.Temp_MediaVolume;
                gf.PreviewItem.Format.MediaBalance = gf.Temp_MediaBalance;
                gf.PreviewItem.Format.MediaMute = gf.Temp_MediaMute;
                gf.PreviewItem.Format.MediaRepeat = gf.Temp_MediaRepeat;
                gf.PreviewItem.Format.MediaWidescreen = gf.Temp_MediaWidescreen;
                AssignMediaText(ref Ind_AssignMedia, gf.PreviewItem.Format.MediaOption);
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
                gf.PreviewItem.Format.ShowFontBold[0] = (@checked ? 1 : 0);
                UpdateFormatData();
            }
            else if (name == "Ind_R1Underline")
            {
                gf.PreviewItem.Format.ShowFontUnderline[0] = (@checked ? 1 : 0);
                UpdateFormatData();
            }
            else if (name == "Ind_R1Colour")
            {
                if (gf.SelectColorFromBtn(ref Ind_R1Colour, ref gf.PreviewItem.Format.ShowFontColour[0]))
                {
                    UpdateFormatData(StartAtFirstSlide: false);
                }
            }
            else if (name == "Ind_R2Bold")
            {
                gf.PreviewItem.Format.ShowFontBold[1] = (@checked ? 1 : 0);
                UpdateFormatData();
            }
            else if (name == "Ind_R2Underline")
            {
                gf.PreviewItem.Format.ShowFontUnderline[1] = (@checked ? 1 : 0);
                UpdateFormatData();
            }
            else if (name == "Ind_R2Colour" && gf.SelectColorFromBtn(ref Ind_R2Colour, ref gf.PreviewItem.Format.ShowFontColour[1]))
            {
                UpdateFormatData(StartAtFirstSlide: false);
            }
        }

        private void Ind_R1Italics_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            gf.AssignDropDownItem(ref Ind_R1Italics, e.ClickedItem.Name, Ind_R1Italics0, Ind_R1Italics1, Ind_R1Italics2);
            int num = DataUtil.ObjToInt(Ind_R1Italics.Tag);
            int num2 = num;
            if (num2 == 2)
            {
                gf.PreviewItem.Format.ShowFontItalic[0] = 0;
                gf.PreviewItem.Format.ShowFontItalic[2] = 1;
            }
            else
            {
                gf.PreviewItem.Format.ShowFontItalic[0] = num;
                gf.PreviewItem.Format.ShowFontItalic[2] = num;
            }
            UpdateFormatData();
        }

        private void Ind_R2Italics_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            gf.AssignDropDownItem(ref Ind_R2Italics, e.ClickedItem.Name, Ind_R2Italics0, Ind_R2Italics1, Ind_R2Italics2);
            int num = DataUtil.ObjToInt(Ind_R2Italics.Tag);
            int num2 = num;
            if (num2 == 2)
            {
                gf.PreviewItem.Format.ShowFontItalic[1] = 0;
                gf.PreviewItem.Format.ShowFontItalic[3] = 1;
            }
            else
            {
                gf.PreviewItem.Format.ShowFontItalic[1] = num;
                gf.PreviewItem.Format.ShowFontItalic[3] = num;
            }
            UpdateFormatData();
        }

        private void Ind_Region_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            gf.AssignDropDownItem(ref Ind_Region, e.ClickedItem.Name, Ind_ShowRegion1, Ind_ShowRegion2, Ind_ShowRegionBoth);
            gf.PreviewItem.Format.ShowLyrics = DataUtil.ObjToInt(Ind_Region.Tag);
            UpdateFormatData(StartAtFirstSlide: false);
        }

        private void Ind_VAlign_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            gf.AssignDropDownItem(ref Ind_VAlign, e.ClickedItem.Name, Ind_VAlignTop, Ind_VAlignCentre, Ind_VAlignBottom);
            gf.PreviewItem.Format.ShowVerticalAlign = DataUtil.ObjToInt(Ind_VAlign.Tag);
            UpdateFormatData(StartAtFirstSlide: false);
        }

        private void Ind_ImageMode_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            gf.AssignDropDownItem(ref Ind_ImageMode, e.ClickedItem.Name, Ind_ImageTile, Ind_ImageCentre, Ind_ImageBestFit);
            gf.PreviewItem.Format.BackgroundMode = (ImageMode)DataUtil.ObjToInt(Ind_ImageMode.Tag);
            gf.SetShowBackground(gf.PreviewItem, ref PreviewScreen);
            UpdateFormatData(StartAtFirstSlide: false);
        }

        private void Ind_R1Align_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            gf.AssignDropDownItem(ref Ind_R1Align, e.ClickedItem.Name, Ind_R1AlignLeft, Ind_R1AlignCentre, Ind_R1AlignRight);
            gf.PreviewItem.Format.ShowFontAlign[0] = DataUtil.ObjToInt(Ind_R1Align.Tag);
            UpdateFormatData(StartAtFirstSlide: false);
        }

        private void Ind_R2Align_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            gf.AssignDropDownItem(ref Ind_R2Align, e.ClickedItem.Name, Ind_R2AlignLeft, Ind_R2AlignCentre, Ind_R2AlignRight);
            gf.PreviewItem.Format.ShowFontAlign[1] = DataUtil.ObjToInt(Ind_R2Align.Tag);
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
