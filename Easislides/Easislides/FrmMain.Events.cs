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
            if (e.Button == MouseButtons.Left)
            {
                Control control = (Control)sender;
                control.Focus();
                Debug.WriteLine($"PowerPointImage_MouseUp name={control.Name} slide={(DataUtil.ObjToInt(control.Tag) + 1)} focus={ActiveControl?.Name}");

                // ?îÎ∏î?¥Î¶≠ ÏßÑÌñâ Ï§ëÏù¥Î©??±Í? ?¥Î¶≠ Î¨¥Ïãú
                if (pptDoubleClickInProgress)
                {
                    return;
                }

        private void PowerPointImage_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == Keys.Up || e.KeyCode == Keys.Down)
            {
                e.IsInputKey = true;
            }
            Debug.WriteLine($"PowerPointImage_PreviewKeyDown key={e.KeyCode} input={e.IsInputKey} focus={ActiveControl?.Name}");
        }

        private void PowerPointImage_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Up && e.KeyCode != Keys.Down)
            {
                return;
            }

            Control control = (Control)sender;
            if (control.Name == "PP_Preview")
            {
                ItemKeyPressed(gf.PreviewItem, e.KeyCode, e.Shift);
            }
            else
            {
                ItemKeyPressed(gf.OutputItem, e.KeyCode, e.Shift);
            }
            Debug.WriteLine($"PowerPointImage_KeyUp name={control.Name} key={e.KeyCode} shift={e.Shift} focus={ActiveControl?.Name}");
        }
                int slideNumber = DataUtil.ObjToInt(control.Tag) + 1;

                // Ï¶âÏãú ?¨Îùº?¥Îìú ?¥Îèô (Delay ?ÜÏùå)
                if (control.Name == "PP_Preview")
                {
                    gf.PreviewItem.CurSlide = slideNumber;
                    MoveToSlide(gf.PreviewItem, KeyDirection.Refresh);
                }
                else
                {
                    gf.OutputItem.CurSlide = slideNumber;
                    MoveToSlide(gf.OutputItem, KeyDirection.Refresh);
                }

                // ÎßàÏ?Îß??¥Î¶≠ ?ïÎ≥¥ ?Ä??(?îÎ∏î ?¥Î¶≠ Í∞êÏ???
                pptLastClickedSlide = slideNumber;
                pptLastClickedControl = control;
            }
        }

        /// <summary>
        /// PowerPoint ?¥Î?ÏßÄ ?îÎ∏î?¥Î¶≠ Ï≤òÎ¶¨
        /// ?îÎ∏î?¥Î¶≠ ??PowerPoint ?¨Îùº?¥Îìú??Ï∞ΩÏùÑ ?úÏÑ±?îÌïòÍ≥??†ÎãàÎ©îÏù¥??ÎπÑÎîî?§Î? ?∏Î¶¨Í±∞Ìï©?àÎã§.
        /// </summary>
        private void PowerPointImage_DoubleClick(object sender, EventArgs e)
        {
            try
            {
                // ?îÎ∏î?¥Î¶≠ ?åÎûòÍ∑??§Ï†ï
                pptDoubleClickInProgress = true;

                Control control = (Control)sender;
                int slideNumber = DataUtil.ObjToInt(control.Tag) + 1;

                // ?¨Îùº?¥Îìú Î≤àÌò∏ ?§Ï†ï
                SongSettings InItem;
                if (control.Name == "PP_Preview")
                {
                    gf.PreviewItem.CurSlide = slideNumber;
                    InItem = gf.PreviewItem;
                }
                else
                {
                    gf.OutputItem.CurSlide = slideNumber;
                    InItem = gf.OutputItem;
                }

                // Trigger PowerPoint click to start animation/video
                if (MainPPT.prePowerPointApp != null)
                {
                    MainPPT.SafePlayNext(slideNumber);
                    //MainPPT.ActivateSlideShowAndTriggerClick(slideNumber);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in PowerPointImage_DoubleClick: {ex.Message}");
            }
            finally
            {
                // ?îÎ∏î?¥Î¶≠ ?åÎûòÍ∑??¥Ï†ú (?§Ïùå ?¥Î¶≠ ?àÏö©)
                System.Threading.Tasks.Task.Delay(100).ContinueWith(_ =>
                {
                    this.BeginInvoke(new Action(() =>
                    {
                        pptDoubleClickInProgress = false;
                    }));
                });
            }
        }

        private void BuildPreviewScreenHandler()
        {
            PreviewScreen.DoubleClick += PreviewScreen_DoubleClick;
        }

        private void PreviewScreen_DoubleClick(object sender, EventArgs e)
        {
            if (gf.PreviewItem.Type == "D")
            {
                string inIDString = "D" + gf.PreviewItem.ItemID;
                Edit_Item(inIDString);
            }
            else if (gf.PreviewItem.Type == "P")
            {
                string inIDString = "P" + gf.PreviewItem.Path;
                Edit_Item(inIDString);
            }
            else if (gf.PreviewItem.Type == "B")
            {
                string inIDString = "B" + gf.PreviewItem.ItemID;
                string text = Edit_Item(inIDString, ref gf.PreviewItem.Title);
                if (text != "")
                {
                    if (gf.PreviewItem.Source == ItemSource.HolyBible)
                    {
                        HB_CurSelectedPassages = DataUtil.Right(text, text.Length - 1);
                        HB_CurSelectedTitle = gf.PreviewItem.Title;
                        HB_SelectedPassagesChanged(HB_CurSelectedPassages, ref HB_CurSelectedTitle);
                    }
                    else if (gf.PreviewItem.Source == ItemSource.WorshipList)
                    {
                        int selectedIndex = gf.GetSelectedIndex(WorshipListItems);
                        WorshipListItems.Items[selectedIndex].SubItems[0].Text = gf.PreviewItem.Title;
                        WorshipListItems.Items[selectedIndex].SubItems[1].Text = text;
                        WorshipListIndexChanged();
                    }
                }
            }
            else if (gf.PreviewItem.Type == "T")
            {
                string inIDString = "T" + gf.PreviewItem.Path;
                Edit_Item(inIDString);
            }
            else if (gf.PreviewItem.Type == "W")
            {
                string inIDString = "W" + gf.PreviewItem.Path;
                Edit_Item(inIDString);
            }
            else if (gf.PreviewItem.Type == "I")
            {
                string inIDString = "I" + gf.PreviewItem.Path;
                Edit_Item(inIDString);
            }
            else if (gf.PreviewItem.Type == "M")
            {
                string inIDString = "M" + gf.PreviewItem.Path;
                Edit_Item(inIDString);
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
                gf.PreviewItem.Source = ItemSource.ExternalFilePowerpoint;
                string text = PowerpointCurPreview = "P" + ExternalPPImagename[ThumbImageClicked];
                string InTitle = Path.GetFileNameWithoutExtension(ExternalPPImagename[ThumbImageClicked]);
                gf.PreviewItem.InMainItemText = InTitle;
                gf.PreviewItem.InSubItemItem1Text = text;
                gf.PreviewItem.CurItemNo = 0;
                LoadItem(ref gf.PreviewItem, text, "", 1, ref InTitle, ScrollToCaret: false);
                LoadExternalPowerpointThumbImages(ThumbImageClicked + 1);
                UpdateDisplayPanelFields();
            }
            else if (e.Button != MouseButtons.Right)
            {
            }
        }

        private void Ind_FontSizeUpDown_MouseUp(object sender, MouseEventArgs e)
        {
            if (!UpdatingFormatFields)
            {
                gf.PreviewItem.Format.ShowFontSize[0] = (int)Ind_Reg1SizeUpDown.Value;
                gf.PreviewItem.Format.ShowFontSize[1] = (int)Ind_Reg2SizeUpDown.Value;
                UpdateFormatData();
            }
        }

        private void Ind_MarginUpDown_MouseUp(object sender, MouseEventArgs e)
        {
            if (!UpdatingFormatFields)
            {
                UpDownBase upDownBase = (UpDownBase)sender;
                string name = upDownBase.Name;
                UpdatingFormatFields = true;
                if (name == "Ind_Reg1TopUpDown")
                {
                    gf.PreviewItem.Format.ShowFontVPosition[0] = (int)Ind_Reg1TopUpDown.Value;
                    UpdatingFormatFields = true;
                    gf.UpdatePosUpDowns(ref Ind_Reg1TopUpDown, ref Ind_Reg2TopUpDown, ref Ind_BottomUpDown, ref gf.PreviewItem.Format.ShowFontVPosition[0], ref gf.PreviewItem.Format.ShowFontVPosition[1], gf.PreviewItem.Format.ShowBottomMargin);
                }
                else if (name == "Ind_Reg2TopUpDown")
                {
                    gf.PreviewItem.Format.ShowFontVPosition[1] = (int)Ind_Reg2TopUpDown.Value;
                    UpdatingFormatFields = true;
                    gf.UpdatePosUpDowns(ref Ind_Reg1TopUpDown, ref Ind_Reg2TopUpDown, ref Ind_BottomUpDown, ref gf.PreviewItem.Format.ShowFontVPosition[0], ref gf.PreviewItem.Format.ShowFontVPosition[1], gf.PreviewItem.Format.ShowBottomMargin);
                }
                else if (name == "Ind_LeftUpDown")
                {
                    gf.PreviewItem.Format.ShowLeftMargin = (int)Ind_LeftUpDown.Value;
                }
                else if (name == "Ind_RightUpDown")
                {
                    gf.PreviewItem.Format.ShowRightMargin = (int)Ind_RightUpDown.Value;
                }
                else if (name == "Ind_BottomUpDown")
                {
                    gf.PreviewItem.Format.ShowBottomMargin = (int)Ind_BottomUpDown.Value;
                    UpdatingFormatFields = true;
                    gf.UpdatePosUpDowns(ref Ind_Reg1TopUpDown, ref Ind_Reg2TopUpDown, ref Ind_BottomUpDown, ref gf.PreviewItem.Format.ShowFontVPosition[0], ref gf.PreviewItem.Format.ShowFontVPosition[1], gf.PreviewItem.Format.ShowBottomMargin);
                }
                UpdatingFormatFields = false;
                UpdateFormatData();
            }
        }

        private void Ind_FontsList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!(UpdatingFormatFields | InitFormLoad))
            {
                gf.PreviewItem.Format.ShowFontName[0] = Ind_Reg1FontsList.Text;
                gf.PreviewItem.Format.ShowFontName[1] = Ind_Reg2FontsList.Text;
                UpdateFormatData();
            }
        }

        private void Ind_TransSelectedIndexChanged(object sender, EventArgs e)
        {
            if (!(UpdatingFormatFields | InitFormLoad))
            {
                ToolStripComboBox toolStripComboBox = (ToolStripComboBox)sender;
                ImageTransitionControl.TransitionAction transitionAction = (toolStripComboBox.Name == "Ind_TransItem") ? ImageTransitionControl.TransitionAction.AsStoredItem : ImageTransitionControl.TransitionAction.AsStoredSlide;
                gf.PreviewItem.Format.ShowItemTransition = Ind_TransItem.SelectedIndex;
                gf.PreviewItem.Format.ShowSlideTransition = Ind_TransSlides.SelectedIndex;
                UpdateFormatData(StartAtFirstSlide: false, transitionAction);
            }
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

        private void Menu_useSongNumbering_Click(object sender, EventArgs e)
        {
            ApplyUseSongNumbers(Menu_UseSongNumbering.Checked);
            ImplementFolderChange = true;
            SongFolder_Change();
        }

        private void SongFolder_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!InitFormLoad)
            {
                SongFolder_Change();
            }
        }

        private void ImagesFolder_SelectedIndexChanged(object sender, EventArgs e)
        {
            ShowPicturesFolderThumbs();
        }

        private void toolStripImages1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
        }

        /// <summary>
        /// daniel ??? ????(?? ????)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tabControlSource_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (IsSelectedTab(tabControlSource, "tabImages"))
            {
                if ((ImagesFolder.Items.Count > 0) & (ImagesFolder.Text == ""))
                {
                    ImagesFolder.SelectedIndex = 0;
                    gf.TabSourceImagesChanged = false;
                }
                if (gf.TabSourceImagesChanged)
                {
                    FormatBackgroundThumbContainers();
                    gf.TabSourceImagesChanged = false;
                }
            }
            else if (IsSelectedTab(tabControlSource, "tabFiles"))
            {
                if ((InfoScreenFolder.Items.Count > 0) & (InfoScreenFolder.Text == ""))
                {
                    InfoScreenFolder.SelectedIndex = 0;
                    gf.TabSourceExternalFilesChanged = false;
                }
                if (!gf.TabSourceExternalFilesChanged)
                {
                }
            }
            else if (IsSelectedTab(tabControlSource, "tabMedia"))
            {
                if ((MediaFolder.Items.Count > 0) & (MediaFolder.Text == ""))
                {
                    MediaFolder.SelectedIndex = 0;
                    gf.TabSourceMediaFolderFilesChanged = false;
                }
                if (!gf.TabSourceMediaFolderFilesChanged)
                {
                }
            }
            else if (IsSelectedTab(tabControlSource, "tabPowerpoint"))
            {
                if ((PowerpointFolder.Items.Count > 0) & (PowerpointFolder.Text == ""))
                {
                    PowerpointFolder.SelectedIndex = 0;
                    gf.TabSourceExternalFilesChanged = false;
                }
                if (gf.TabSourceExternalFilesChanged)
                {
                    FormatExternalPowerpointThumbContainers();
                    gf.TabSourceExternalFilesChanged = false;
                }
            }
            ShowStatusBarSummary();
        }

        private void BookLookup_SelectedIndexChanged(object sender, EventArgs e)
        {
            BookLookupChanged();
        }

        private void Bibles_ShowVerses_Click(object sender, EventArgs e)
        {
        }

        private void TabBibleVersions_Click(object sender, EventArgs e)
        {
            if (TabBibleVersions.SelectedIndex != gf.HB_CurVersionTabIndex)
            {
                TabBibleVersionsChanged();
            }
            }


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
                gf.ShowDebugVideoMessages = !gf.ShowDebugVideoMessages;
                MessageBox.Show("Video Debug Message Turned " + (gf.ShowDebugVideoMessages ? "On" : "Off"));
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
                gf.GetSelectedIndex(SongsList, ref OutString);
                gf.SetMenuItem(ref CMenuSongs_Edit, OutString, "Edit Item", "Edit", DisableWhenBlank: true);
                CMenuSongs.Show(SongsList, e.X, e.Y);
            }
            SongsList.Focus();
        }

        private void Menu_EditHistory_Click(object sender, EventArgs e)
        {
            try
            {
                ToolStripMenuItem toolStripMenuItem = (ToolStripMenuItem)sender;
                int num = DataUtil.ObjToInt(toolStripMenuItem.Tag) + 1;
                Edit_Item(gf.MainEditHistoryList[num, 0]);
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

        private void btnToOutput_Click(object sender, EventArgs e)
        {
            if (gf.PreviewItem.ItemID != "")
            {
                CopyPreviewToOutput();
            }
        }

        private void btnToOutputMoveNext_Click(object sender, EventArgs e)
        {
            if (gf.PreviewItem.ItemID != "")
            {
                CopyPreviewToOutput();
            }
            ManualMoveToItem(gf.PreviewItem, KeyDirection.NextOne);
            FocusOutputArea();
        }
    }
}


