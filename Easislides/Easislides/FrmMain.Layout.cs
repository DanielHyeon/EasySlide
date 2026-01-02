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
        private void ApplySplitterValues()
        {
            int num = 0;
            num = RegUtil.GetRegValue("settings", Splitter_FolderWidth, 230);
            if (num < 25 || num > 1500)
            {
                num = 230;
            }
            splitContainerMain.SplitterDistance = num;
            num = RegUtil.GetRegValue("settings", Splitter_OutputWidth, 250);
            if (num < 25 || num > 1500)
            {
                num = 250;
            }
            splitContainer2.SplitterDistance = num;
            num = RegUtil.GetRegValue("settings", Splitter_FolderHeight, 250);
            if (num < 25 || num > 1500)
            {
                num = 250;
            }
            splitContainer1.SplitterDistance = num;
            num = RegUtil.GetRegValue("settings", Splitter_PreviewLyricsHeight, 250);
            if (num < 25 || num > 1500)
            {
                num = 250;
            }
            splitContainerPreview.SplitterDistance = num;
            num = RegUtil.GetRegValue("settings", Splitter_outputLyricsHeight, 250);
            if (num < 25 || num > 1500)
            {
                num = 250;
            }
            splitContainerOutput.SplitterDistance = num;
        }

        /// <summary>
        /// daniel 
        /// ????????????? ????  ??.
        /// </summary>
        /// <returns></returns>
        private bool InitFormControls()
        {
            gf.isScreenWideMode = ((DataUtil.ObjToInt(RegUtil.GetRegValue("monitors", "IsMonitorWide", 0)) > 0) ? true : false);

            gf.ShowTaskBar();
            int num = RegUtil.GetRegValue("settings", "MainLeft", -1);
            int num2 = RegUtil.GetRegValue("settings", "MainTop", -1);
            int num3 = RegUtil.GetRegValue("settings", "MainWidth", 720);
            int num4 = RegUtil.GetRegValue("settings", "MainHeight", 560);
            if (num3 > Screen.PrimaryScreen.Bounds.Width)
            {
                num3 = Screen.PrimaryScreen.Bounds.Width - 20;
            }
            if (num4 > Screen.PrimaryScreen.Bounds.Height)
            {
                num4 = Screen.PrimaryScreen.Bounds.Height - 30;
            }
            if (num < 0)
            {
                num = 0;
            }
            if (num2 < 0)
            {
                num2 = 0;
            }
            if (num + num3 > Screen.PrimaryScreen.Bounds.Width)
            {
                num = Screen.PrimaryScreen.Bounds.Width - num3;
            }
            if (num2 + num4 > Screen.PrimaryScreen.Bounds.Height)
            {
                num2 = Screen.PrimaryScreen.Bounds.Height - num4;
            }
            base.Left = num;
            base.Top = num2;
            base.Height = num4;
            base.Width = num3;
            frmLeft = num;
            frmTop = num2;
            frmHeight = num4;
            frmWidth = num3;
            if (RegUtil.GetRegValue("settings", "MainMax", 0) > 0)
            {
                base.WindowState = FormWindowState.Maximized;
            }
            else
            {
                ApplySplitterValues();
            }
            DefPanel.Dock = DockStyle.Fill;
            IndPanel.Dock = DockStyle.Fill;
            PreviewInfo.Dock = DockStyle.Fill;
            IndradioButtonText.Checked = true;
            PreviewScreen.Parent = PreviewHolder;
            PreviewScreen.Dock = DockStyle.Fill;
            OutputScreen.Parent = OutputHolder;
            OutputScreen.Dock = DockStyle.Fill;
            panelPreviewSessionNotes2.Dock = DockStyle.Fill;
            PreviewNotes.Dock = DockStyle.Fill;
            panelPreviewSessionNotes2.Visible = false;
            flowLayoutExternalPowerPoint.Dock = DockStyle.Fill;
            flowLayoutImages.Dock = DockStyle.Fill;
            flowLayoutPreviewPowerPoint.Dock = DockStyle.Fill;
            flowLayoutOutputPowerPoint.Dock = DockStyle.Fill;
            flowLayoutPreviewLyrics.Dock = DockStyle.Fill;
            flowLayoutOutputLyrics.Dock = DockStyle.Fill;
            BuildPreviewScreenHandler();
            flowLayoutPreviewPowerPoint.Visible = false;
            flowLayoutOutputPowerPoint.Visible = false;
            flowLayoutPreviewLyrics.Visible = true;
            flowLayoutOutputLyrics.Visible = true;
            SingleMonIcon = (Bitmap)imageListSys.Images[12];
            DualMonIcon = (Bitmap)imageListSys.Images[13];
            Keyboard1Icon = (Bitmap)imageListSys.Images[14];
            SetPreviewAreas();
            gf.BuildFontsList(ref Ind_Reg1FontsList);
            gf.BuildFontsList(ref Ind_Reg2FontsList);
            gf.BuildFontsList(ref Def_PanelFontList);
            PreviewScreen.BuildTransitionsList(ref Def_TransItem);
            PreviewScreen.BuildTransitionsList(ref Def_TransSlides);
            PreviewScreen.BuildTransitionsList(ref Ind_TransItem);
            PreviewScreen.BuildTransitionsList(ref Ind_TransSlides);
            if (!gf.InitAppData())
            {
                return false;
            }
            ApplyUseSongNumbers(gf.UseSongNumbers);
            BuildFolderList();

            SetJumpToolTips();
            SetMainDefaultBackScreen();

            var task2 = Task.Factory.StartNew(() =>
            {
                PopulatePraiseBooksList();
            }, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Current);

            PraiseBook.Text = "";
            gf.NormalTextRegionBackColour = SongFolder.BackColor;

            PopulateWorshipList();

            ApplyWorshipMode();
            SetItemFontSettings(ref gf.PreviewItem);
            ResetMainPictureBox(ref gf.OutputItem);
            BuildPicturesFolderList();
            BuildInfoScreenFolderList();
            BuildMediaFolderList();
            SetPowerpointListingButton();
            SetTabsVisibility();
            gf.LoadBibleVersions(ref TabBibleVersions);

            TabBibleVersionsChanged();

            InitFormLoad = false;
            BuildVerseButtons(gf.PreviewItem, Reset: true);
            BuildVerseButtons(gf.OutputItem, Reset: true);
            TimerFlasher.Enabled = true;
            DefaultButtonForeColour = btnToOutput.ForeColor;
            PreviewScreen.AllowDrop = true;
            BuildEditHistoryMenuItems();
            EnableEditHistory();
            SetAutoRotateButtons();
            PreviewNotations(gf.PreviewArea_ShowNotations);
            SetNotesPreview(NotesChecked: false);
            AlertWindow.OnMessage += AlertWindow_OnMessage;
            popupHelper = new PopupWindowHelper();
            popupHelper.PopupClosed += popupHelper_PopupClosed;
            gf.SplashUp = false;
            Text = "EasiSlides" + ((gf.UserString != "") ? (" - " + gf.UserString) : "");

            return true;
        }

        private void ResizePreviewBottomPanel()
        {
            ResizeSampleScreen(panelPreviewBottom, PreviewHolder, PreviewBack, AdjustForIndicator: false);
        }

        private void ResizeOutputBottomPanel()
        {
            ResizeSampleScreen(panelOutputBottom, OutputHolder, OutputBack, gf.ShowLyricsMonitorAlertBox);
        }

        private void ResizeSampleScreen(Panel InPanelContainer, Panel InHolder, Panel InBack, bool AdjustForIndicator)
        {
            int num = InPanelContainer.Width;
            if (num < 0)
            {
                num = 0;
            }
            int num2 = 0;
            int num3 = AdjustForIndicator ? panelOutputLM1.Height : 0;
            int num4 = 15;
            int num5 = num * 18 / 20;

            //int num6 = ((num5 <= 0) ? 1 : num5) * 3 / 4;

            int num6 = 0;

            // Daniel Park ?? 2023??12??24??
            //int newHeight = 0;
            if (!gf.isScreenWideMode)
            {
                num6 = ((num5 <= 0) ? 1 : num5) * 3 / 4;
            }
            else
            {
                num6 = ((num5 <= 0) ? 1 : num5) * 3 / 4;
            }



            if (num6 > InPanelContainer.Height - (num2 + num3 + num4))
            {
                num6 = InPanelContainer.Height - (num2 + num3 + num4);
                num6 = ((num6 <= 0) ? 1 : num6);
                // daniel
                // ? ????????????
                // num5 = num6 * 4 / 3;

                //if (gf.isScreenWideMode)
                //    num5 = num6 * 5 / 3;
                //else
                num5 = num6 * 4 / 3;
            }
            InHolder.Left = (InPanelContainer.Width - num5 - num6 / 40) / 2;
            InHolder.Height = num6;
            InHolder.Width = num5;
            InHolder.Top = (InPanelContainer.Height - (num3 + num6)) / 2;
            if (InHolder.Top < num2 + 2 && num2 > 0)
            {
                InHolder.Top = num2 + 2;
            }
            if (num6 > 0)
            {
                InBack.Left = InHolder.Left + num6 / 40 + 1;
                InBack.Top = InHolder.Top + num6 / 40 + 1;
                InBack.Height = num6;
                InBack.Width = num5;
            }
            else
            {
                InBack.Height = 0;
                InBack.Width = 0;
            }
            SetStatusbarSize();
        }

        /// <summary>
        /// ????ResizeSampleScreen version 1
        /// </summary>
        /// <param name="InPanelContainer"></param>
        /// <param name="InHolder"></param>
        /// <param name="InBack"></param>
        /// <param name="AdjustForIndicator"></param>
        private void ResizeSampleScreen_v1(Panel InPanelContainer, Panel InHolder, Panel InBack, bool AdjustForIndicator)
        {
            int num = InPanelContainer.Width;
            if (num < 0)
            {
                num = 0;
            }
            int num2 = 0;
            int num3 = AdjustForIndicator ? panelOutputLM1.Height : 0;
            int num4 = 15;
            int num5 = num * 18 / 20;
            int num6 = ((num5 <= 0) ? 1 : num5) * 3 / 4;
            if (num6 > InPanelContainer.Height - (num2 + num3 + num4))
            {
                num6 = InPanelContainer.Height - (num2 + num3 + num4);
                num6 = ((num6 <= 0) ? 1 : num6);
                // daniel
                // ? ????????????
                // num5 = num6 * 4 / 3;

                //if (gf.isScreenWideMode)
                //    num5 = num6 * 5 / 3;
                //else
                num5 = num6 * 4 / 3;
            }
            InHolder.Left = (InPanelContainer.Width - num5 - num6 / 40) / 2;
            InHolder.Height = num6;
            InHolder.Width = num5;
            InHolder.Top = (InPanelContainer.Height - (num3 + num6)) / 2;
            if (InHolder.Top < num2 + 2 && num2 > 0)
            {
                InHolder.Top = num2 + 2;
            }
            if (num6 > 0)
            {
                InBack.Left = InHolder.Left + num6 / 40 + 1;
                InBack.Top = InHolder.Top + num6 / 40 + 1;
                InBack.Height = num6;
                InBack.Width = num5;
            }
            else
            {
                InBack.Height = 0;
                InBack.Width = 0;
            }
            SetStatusbarSize();
        }

        /// <summary>
        /// ?? ResizeSampleScreen
        /// </summary>
        /// <param name="InPanelContainer"></param>
        /// <param name="InHolder"></param>
        /// <param name="InBack"></param>
        /// <param name="AdjustForIndicator"></param>
        private void ResizeSampleScreen_v0(Panel InPanelContainer, Panel InHolder, Panel InBack, bool AdjustForIndicator)
        {
            int num = InPanelContainer.Height - (AdjustForIndicator ? labelBlackScreen.Height : 0);
            if (num < 0)
            {
                num = 0;
            }
            int num2 = num * 18 / 20;
            int num3 = ((num2 <= 0) ? 1 : num2) * 4 / 3;
            if (num3 > InPanelContainer.Width - 40)
            {
                num3 = InPanelContainer.Width - 40;
                num3 = ((num3 <= 0) ? 1 : num3);
                num2 = num3 * 3 / 4;
            }
            InHolder.Left = (InPanelContainer.Width - num3 - num2 / 40) / 2;
            InHolder.Height = num2;
            InHolder.Width = num3;
            InHolder.Top = (AdjustForIndicator ? labelBlackScreen.Height : 0) + (int)((double)(num - InHolder.Height) / 2.5) + num / 60;
            if (num2 > 0)
            {
                InBack.Left = InHolder.Left + num2 / 40 + 1;
                InBack.Top = InHolder.Top + num2 / 40 + 1;
                InBack.Height = num2;
                InBack.Width = num3;
            }
            else
            {
                InBack.Height = 0;
                InBack.Width = 0;
            }
            SetStatusbarSize();
        }

        private void SetPreviewAreas()
        {
            flowLayoutPreviewLyrics.Visible = IndradioButtonText.Checked;
            IndPanel.Visible = IndradioButtonFormat.Checked;
            PreviewInfo.Visible = IndradioButtonInfo.Checked;
            IndradioButtonText.ForeColor = (IndradioButtonText.Checked ? gf.ButtonPressedForeColour : gf.ButtonDefaultForeColour);
            IndradioButtonFormat.ForeColor = (IndradioButtonFormat.Checked ? gf.ButtonPressedForeColour : gf.ButtonDefaultForeColour);
            IndradioButtonInfo.ForeColor = (IndradioButtonInfo.Checked ? gf.ButtonPressedForeColour : gf.ButtonDefaultForeColour);
            if (IndradioButtonText.Checked)
            {
                ApplyPreviewArea_Setup(0);
            }
            FocusPreviewArea();
        }

        private void SetNotesPreview(bool NotesChecked)
        {
            panelPreviewSessionNotes2.Visible = NotesChecked;
        }

        private void FocusPreviewArea()
        {
            HighlightPreviewRichTextBox(OnEnter: true, ScrollToTop: true);
            PreviewInfo.Focus();
        }

        private void FocusOutputArea()
        {
            HighlightOutputRichTextBox(OnEnter: true, ScrollToTop: true);
            OutputInfo.Focus();
        }

        private void AllowIndividualFormat(bool AllowFormat)
        {
            AllowIndividualFormat(AllowFormat, BoxChecked: false);
        }

        private void AllowIndividualFormat(bool AllowFormat, bool BoxChecked)
        {
            if (AllowFormat)
            {
                Ind_checkBox.Enabled = true;
                gf.PreviewItem.UseDefaultFormat = !BoxChecked;
            }
            else
            {
                Ind_checkBox.Enabled = false;
                gf.PreviewItem.UseDefaultFormat = ((!(gf.PreviewItem.Type == "P")) ? true : false);
            }
            Ind_checkBox.Checked = BoxChecked;
            IndgroupBox1.Enabled = (BoxChecked & Ind_checkBox.Enabled);
            IndgroupBox2.Enabled = IndgroupBox1.Enabled;
            IndgroupBox3.Enabled = IndgroupBox1.Enabled;
            IndgroupBox4.Enabled = IndgroupBox1.Enabled;
            panelIndTemplate.Enabled = IndgroupBox1.Enabled;
        }

        /// <summary>
        /// daniel
        /// </summary>
        /// <param name="InCanvas"></param>
        /// <param name="InPanel"></param>
        /// <param name="InTotalScreens"></param>
        public void FormatPowerPointThumbContainers2(int index)
        {
            FormatPowerPointThumbContainers3(ref Powerpoint_OutputCanvas, ref flowLayoutOutputPowerPoint, index);
        }

        private void FormatPowerPointThumbContainers3(ref ImageCanvas[] InCanvas, ref FlowLayoutPanel InPanel, int index)
        {
            string text = "";
            if (index == 0)
            {
                gf.ThumbImagesPerRow = 3;
                if (InPanel.Name == "flowLayoutPreviewPowerPoint")
                {
                    text = "PP_Preview";
                    //flowLayoutPreviewPowerPoint.Controls.Clear();
                }
                else
                {
                    text = "PP_Output";
                    //flowLayoutOutputPowerPoint.Controls.Clear();
                }
            }

            //for (int i = 0; i < InTotalScreens; i++)
            {
                if (InCanvas[index] == null)
                {
                    InCanvas[index] = new ImageCanvas();
                    InCanvas[index].Name = text;
                    InCanvas[index].Tag = index.ToString();
                    InCanvas[index].SlideNumber = index + 1;
                    InCanvas[index].MouseUp += PowerPointImage_MouseUp;
                    InCanvas[index].DoubleClick += PowerPointImage_DoubleClick;
                    InCanvas[index].KeyUp += PowerPointImage_KeyUp;
                    InCanvas[index].PowerPoint = true;
                    InCanvas[index].Visible = true;
                }
                if (InPanel.Name == "flowLayoutPreviewPowerPoint")
                {
                    flowLayoutPreviewPowerPoint.Controls.Add(InCanvas[index]);
                }
                else
                {
                    flowLayoutOutputPowerPoint.Controls.Add(InCanvas[index]);
                }
            }

        }

        private void FormatPowerPointThumbContainers(ref ImageCanvas[] InCanvas, ref FlowLayoutPanel InPanel, int InTotalScreens)
        {
            Console.WriteLine($"[ThumbPreview] FormatPowerPointThumbContainers: Panel={InPanel?.Name}, TotalScreens={InTotalScreens}");
            string text = "";
            gf.ThumbImagesPerRow = 3;
            if (InPanel.Name == "flowLayoutPreviewPowerPoint")
            {
                text = "PP_Preview";
                flowLayoutPreviewPowerPoint.Controls.Clear();
            }
            else
            {
                text = "PP_Output";
                flowLayoutOutputPowerPoint.Controls.Clear();
            }

            for (int i = 0; i < InTotalScreens; i++)
            {
                if (InCanvas[i] == null)
                {
                    InCanvas[i] = new ImageCanvas();
                    InCanvas[i].Name = text;
                    InCanvas[i].Tag = i.ToString();
                    InCanvas[i].SlideNumber = i + 1;
                    InCanvas[i].MouseUp += PowerPointImage_MouseUp;
                    InCanvas[i].DoubleClick += PowerPointImage_DoubleClick;
                    InCanvas[i].KeyUp += PowerPointImage_KeyUp;
                    InCanvas[i].PowerPoint = true;
                }

                if (InPanel.Name == "flowLayoutPreviewPowerPoint")
                {
                    flowLayoutPreviewPowerPoint.Controls.Add(InCanvas[i]);
                }
                else
                {
                    flowLayoutOutputPowerPoint.Controls.Add(InCanvas[i]);
                }
            }

        }

        /// <summary>
        /// daniel SUN 11/24 2019
        /// </summary>
        /// <param name="InCanvas"></param>
        /// <param name="InPanel"></param>
        /// <param name="InTotalScreens"></param>
        private void FormatPowerPointThumbContainers1(ref ImageCanvas[] InCanvas, ref FlowLayoutPanel InPanel, int InTotalScreens)
        {
            string text = "";

            gf.ThumbImagesPerRow = 3;
            if (InPanel.Name == "flowLayoutPreviewPowerPoint")
            {
                text = "PP_Preview";
                flowLayoutPreviewPowerPoint.Controls.Clear();
            }
            else
            {
                text = "PP_Output";
                flowLayoutOutputPowerPoint.Controls.Clear();
            }

            int controlCount = 0;

            if (InPanel.Name == "flowLayoutPreviewPowerPoint")
            {
                controlCount = flowLayoutPreviewPowerPoint.Controls.Count;
            }
            else
            {
                controlCount = flowLayoutOutputPowerPoint.Controls.Count;
            }

            for (int i = 0; i < InTotalScreens; i++)
            {
                if (InCanvas[i] == null)
                {
                    InCanvas[i] = new ImageCanvas();
                    InCanvas[i].Name = text;
                    InCanvas[i].Tag = i.ToString();
                    InCanvas[i].SlideNumber = i + 1;
                    InCanvas[i].MouseUp += PowerPointImage_MouseUp;
                    InCanvas[i].DoubleClick += PowerPointImage_DoubleClick;
                    InCanvas[i].KeyUp += PowerPointImage_KeyUp;
                    InCanvas[i].PowerPoint = true;

                    ///daniel
                    if (InPanel.Name == "flowLayoutPreviewPowerPoint")
                    {
                        flowLayoutPreviewPowerPoint.Controls.Add(InCanvas[i]);
                    }
                    else
                    {
                        flowLayoutOutputPowerPoint.Controls.Add(InCanvas[i]);
                    }
                    ///
                }
                else
                {
                    InCanvas[i].Name = text;
                    InCanvas[i].Tag = i.ToString();
                    InCanvas[i].SlideNumber = i + 1;
                    //InCanvas[i].MouseUp += PowerPointImage_MouseUp;
                    InCanvas[i].PowerPoint = true;
                }

            }

            if (controlCount > InTotalScreens)
            {
                for (int k = InTotalScreens; k < controlCount; k++)
                {
                    if (InCanvas[k] != null)
                    {
                        InCanvas[k].Visible = false;
                    }
                }
            }
        }

        private void PowerPointImage_DoubleClick(object sender, EventArgs e)
        {
            try
            {
                // 더블클릭 플래그 설정
                pptDoubleClickInProgress = true;

                // KeyboardActionHandler를 통해 처리
                _keyboardHandler.HandlePowerPointThumbnailDoubleClick((Control)sender);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in PowerPointImage_DoubleClick: {ex.Message}");
            }
            finally
            {
                // 더블클릭 플래그 해제 (다음 클릭 허용)
                System.Threading.Tasks.Task.Delay(100).ContinueWith(_ =>
                {
                    this.BeginInvoke(new Action(() =>
                    {
                        pptDoubleClickInProgress = false;
                    }));
                });
            }
        }


        private void SetCanvasVisible(ref ImageCanvas[] InCanvas, bool MakeVisible)
        {
            //for (int i = 0; i < 1024; i++)
            for (int i = 0; i < InCanvas.Length; i++)
            {
                if (InCanvas[i] != null)
                {
                    InCanvas[i].Visible = MakeVisible;
                }
            }
        }

        private void BuildEditHistoryMenuItems()
        {
            Menu_EditHistoryList.DropDownItems.Clear();
            ToolStripMenuItem toolStripMenuItem = new ToolStripMenuItem();
            for (int i = 0; i < gf.AbsoluteMaxHitoryItems - 1; i++)
            {
                toolStripMenuItem = new ToolStripMenuItem();
                toolStripMenuItem.Name = "Menu_EditHistory" + i;
                toolStripMenuItem.Text = "";
                Menu_EditHistoryList.DropDownItems.Add(toolStripMenuItem);
                Menu_EditHistoryList.DropDownItems[i].Tag = i.ToString();
            }
            EventHandler value = new EventHandler(Menu_EditHistory_Click).Invoke;
            foreach (ToolStripMenuItem dropDownItem in Menu_EditHistoryList.DropDownItems)
            {
                dropDownItem.Click += value;
            }
        }

        private void SetSortButton(SortBy Inmode)
        {
            switch (Inmode)
            {
                case SortBy.Alpha:
                    Folders_WordCount.Checked = false;
                    CurStyle = SortBy.Alpha;
                    break;
                case SortBy.WordCount:
                    Folders_WordCount.Checked = true;
                    CurStyle = SortBy.WordCount;
                    break;
            }
        }

        private void SetPowerpointListingButton()
        {
            string text = "";
            gf.AssignDropDownItem(SelectedMenuItemName: (gf.PowerpointListingStyle != 0) ? PP_PreviewStyle.Name : PP_ListStyle.Name, SelectedBtn: ref PP_ListType, InMenuItem1: PP_ListStyle, InMenuItem2: PP_PreviewStyle);
            PowerpointStyle_Changed(RebuildPowerpointFolders: true);
        }

        private void ModeEnableItems(bool EnableWorshipMode)
        {
            ImagesFolder.Enabled = EnableWorshipMode;
            flowLayoutImages.Enabled = EnableWorshipMode;
            if (BackgroundImagesCanvas[0] != null)
            {
                for (int i = 0; i < BackgroundImagesCanvas.Length; i++)
                {
                    if (BackgroundImagesCanvas[i] != null)
                    {
                        BackgroundImagesCanvas[i].Enabled = EnableWorshipMode;
                    }
                }
            }
            BookLookup.Enabled = EnableWorshipMode;
            toolStripBible2.Enabled = EnableWorshipMode;
            BibleUserLookup.Enabled = EnableWorshipMode;
            TabBibleVersions.Enabled = EnableWorshipMode;
            BibleText.Enabled = EnableWorshipMode;
            InfoScreenList.Enabled = EnableWorshipMode;
            MediaList.Enabled = EnableWorshipMode;
            panelExternalFiles1.Enabled = EnableWorshipMode;
            flowLayoutPreviewPowerPoint.Enabled = EnableWorshipMode;
            DefPanel.Enabled = EnableWorshipMode;
            IndPanel.Enabled = EnableWorshipMode;
            btnToOutput.Enabled = EnableWorshipMode;
            btnToLive.Enabled = EnableWorshipMode;
            btnToOutputMoveNext.Enabled = EnableWorshipMode;
            IndradioButtonText.Checked = true;
            SetPreviewAreas();
            Menu_WorshipSessions.Enabled = ((EnableWorshipMode & !gf.ShowRunning) ? true : false);
            splitContainerPreview.Panel2Collapsed = !EnableWorshipMode;
            Menu_PraiseBookTemplates.Enabled = !EnableWorshipMode;
        }

        private void ClearLyrics(ref Panel InPanel)
        {
            ClearLyrics(ref InPanel, SetFocus: false);
        }

        private void ClearLyrics(ref Panel InPanel, bool SetFocus)
        {
            if (InPanel.Name == "flowLayoutPreviewLyrics")
            {
                FormatLyricsContainers(gf.PreviewItem, Reset: true, SetFocus);
            }
            else
            {
                FormatLyricsContainers(gf.OutputItem, Reset: true, SetFocus);
            }
        }

        internal void ShowStatusBarSummary()
        {
            int num = 0;
            string text = "";
            if (!IsSelectedTab(tabControlSource, "tabBibles"))
            {
                text = (IsSelectedTab(tabControlSource, "tabFiles") ? ("InfoScreen: " + InfoScreenList.Items.Count + " Listed. / ") : ((!IsSelectedTab(tabControlSource, "tabMedia")) ? ("Folder: " + SongsList.Items.Count + ((SongsList.Items.Count < 1) ? " item. /" : " items / ")) : ("Media: " + MediaList.Items.Count + " Listed. / ")));
            }
            else if (BibleText.Text != "")
            {
                text = "Verses listed: " + gf.HB_VersesLocation[0, 0] + " / ";
            }
            else if (gf.HB_TotalVersions > 0)
            {
                text = "Verses listed: 0 / ";
            }
            if (gf.EasiSlidesMode == UsageMode.Worship)
            {
                text = text + "Worship: " + WorshipListItems.Items.Count + ((WorshipListItems.Items.Count < 1) ? " item." : " items. ");
            }
            else if (gf.EasiSlidesMode == UsageMode.PraiseBook)
            {
                text = text + "PraiseBook: " + PraiseBookItems.Items.Count + ((PraiseBookItems.Items.Count < 1) ? " item." : " items. ");
            }
            statusStripMain.Items[0].Text = text;
            text = "";
            if (gf.PreviewItem.Source == ItemSource.SongsList)
            {
                if (gf.PreviewItem.Type == "D" && gf.PreviewItem.FolderNo > 0)
                {
                    text = text + " " + gf.FolderName[gf.PreviewItem.FolderNo];
                }
            }
            else if (gf.PreviewItem.Source == ItemSource.ExternalFileInfoScreen)
            {
                text += InfoScreenFolder.Text;
            }
            else if (gf.PreviewItem.Source == ItemSource.ExternalFileMedia)
            {
                text += MediaFolder.Text;
            }
            else if (gf.PreviewItem.Source == ItemSource.ExternalFilePowerpoint)
            {
                text += PowerpointFolder.Text;
            }
            else if (gf.PreviewItem.Source == ItemSource.HolyBible)
            {
                text += "Holy Bible";
            }
            else if (gf.PreviewItem.Source == ItemSource.WorshipList)
            {
                text += "Worship";
                if (gf.PreviewItem.Type == "D" && gf.PreviewItem.FolderNo > 0)
                {
                    text = text + " (" + gf.FolderName[gf.PreviewItem.FolderNo] + ")";
                }
            }
            else if (gf.PreviewItem.Source == ItemSource.PraiseBook)
            {
                text += "PraiseBook";
                if (gf.PreviewItem.Type == "D" && gf.PreviewItem.FolderNo > 0)
                {
                    text = text + " (" + gf.FolderName[gf.PreviewItem.FolderNo] + ")";
                }
            }
            else
            {
                text = (text ?? "");
            }
            statusStripMain.Items[1].Text = text + ((gf.PreviewItem.User_Reference != "") ? (" [" + gf.PreviewItem.User_Reference + "]") : "") + ((gf.PreviewItem.Book_Reference != "") ? (" [" + gf.PreviewItem.Book_Reference + "]") : "") + " " + KeyCapoText;
            statusStripMain.Items[1].ToolTipText = statusStripMain.Items[1].Text;
            StatusBarOutputPaneMess = ShowStatusBarOutputArea();
            SetStatusBarMediaTimings();
            SetStatusbarSize();
        }

        private string ShowStatusBarOutputArea()
        {
            string text = "";
            if (gf.OutputItem.CurItemNo < 1)
            {
                if (gf.OutputItem.ItemID != "")
                {
                    string text2 = text;
                    text = $"{text2}{gf.OutputItem.CurSlide}/{gf.OutputItem.TotalSlides} [Ad-hoc]";
                }
                else
                {
                    text = (text ?? "");
                }
            }
            else
            {
                // 22/04/01 03:39 daniel
                // ??  ? ?? ?????????????? ??
                string text2 = text;
                text = $"[{Convert.ToString(gf.OutputItem.CurItemNo)}/{WorshipListItems.Items.Count}] {gf.OutputItem.Title} {text2}{gf.OutputItem.CurSlide}/{gf.OutputItem.TotalSlides} ";
            }
            text += ((gf.GapItemOption > GapType.None) ? (((text == "") ? "" : " ") + "(" + gf.GapItemOption.ToString() + " Gap)") : "");
            statusStripMain.Items[2].Image = (gf.DualMonitorMode ? DualMonIcon : SingleMonIcon);
            statusStripMain.Items[2].Text = text;
            statusStripMain.Items[2].ToolTipText = gf.OutputMonitorText;
            if (gf.KeyBoardOption == 1)
            {
                statusStripMain.Items[3].Image = Keyboard1Icon;
                statusStripMain.Items[3].ToolTipText = "Use Alternate Keyboard keys";
            }
            else
            {
                statusStripMain.Items[3].Image = null;
                statusStripMain.Items[3].ToolTipText = "";
            }
            return text;
        }

        private void SetStatusBarMediaTimings()
        {
            if (gf.ShowRunning)
            {
                string text = RemoteControlLiveShow(LiveShowAction.Remote_GetMediaTimings);
                if (text != "")
                {
                    statusStripMain.Items[2].Text = StatusBarOutputPaneMess + " - " + text;
                }
                else
                {
                    statusStripMain.Items[2].Text = StatusBarOutputPaneMess;
                }
            }
        }

        private void SetStatusbarSize()
        {
            statusStripMain.Items[0].Width = splitContainerMain.Panel1.Width + 1;
            statusStripMain.Items[1].Width = splitContainer2.Panel1.Width + 2;
            if (gf.KeyBoardOption == 1)
            {
                statusStripMain.Items[3].Width = 40;
            }
            else
            {
                statusStripMain.Items[3].Width = 0;
            }
            int num = statusStripMain.Width - (statusStripMain.Items[0].Width + statusStripMain.Items[1].Width + 1 + statusStripMain.Items[3].Width) - 15;
            statusStripMain.Items[2].Width = ((num > 0) ? num : 0);
        }

        private int GetImagesPanelWidth()
        {
            return (SongsList.Width - 25 - 5 * (gf.ThumbImagesPerRow - 1)) / gf.ThumbImagesPerRow;
        }

        private void FormatBackgroundThumbContainers()
        {
            if (ImagesFolder.Text == "")
            {
                return;
            }
            gf.ThumbImagesPerRow = 3;
            int height = SongsList.Height;
            flowLayoutImages.Controls.Clear();
            for (int i = 0; i < BackgroundTotalImagesCount; i++)
            {
                if (BackgroundImagesCanvas[i] == null)
                {
                    BackgroundImagesCanvas[i] = new ImageCanvas();
                    BackgroundImagesCanvas[i].Tag = i.ToString();
                    BackgroundImagesCanvas[i].MouseUp += ThumbImage_MouseUp;
                }
                flowLayoutImages.Controls.Add(BackgroundImagesCanvas[i]);
                BackgroundImagesCanvas[i].Visible = false;
            }
            LoadBackgroundThumbImages();
        }

        /// <summary>
        /// daniel out ???? ?????ü???? ? ??
        /// </summary>
        /// <param name="InFlowPanel"></param>
        /// <param name="InCanvas"></param>
        /// <param name="ImageName"></param>
        /// <param name="TotalImagesCount"></param>
        /// <param name="PanelWidth"></param>
        /// <param name="InPrefix"></param>
        /// <param name="CurSelectedSlide"></param>
        /// <param name="InToolTip"></param>
        /// <param name="ExternalPP"></param>
        private void LoadThumbOutImages(FlowLayoutPanel InFlowPanel, ref ImageCanvas[] InCanvas, string[] ImageName, int TotalImagesCount, int PanelWidth, string InPrefix, int CurSelectedSlide, ToolTip InToolTip, bool ExternalPP)
        {
            if (InCanvas != null && CurSelectedSlide > 0)
            {
                Color backColor = InFlowPanel.BackColor;
                int num = (PanelWidth - 35) / 3;
                int newHeight = num * 3 / 4;

                try
                {
                    if (LoadThumbOutlockkey == 0)
                    {
                        for (int i = 0; i < TotalImagesCount; i++)
                        {
                            InCanvas[i].BuildNewImageThumbs(i, num, newHeight, ref ImageName, TotalImagesCount, InPrefix, CurSelectedSlide, backColor, InToolTip, ExternalPP);
                        }
                        Console.WriteLine($"[ThumbPreview] LoadThumbPreviewImages first-load done: Count={TotalImagesCount}");
                    }
                    else
                    {
                        if (previousOutSelectedSlide != CurSelectedSlide)
                        {
                            InCanvas[CurSelectedSlide - 1].BuildNewImageThumbs(CurSelectedSlide - 1, num, newHeight, ref ImageName, TotalImagesCount, InPrefix, CurSelectedSlide, backColor, InToolTip, ExternalPP);
                            InCanvas[previousOutSelectedSlide - 1].BuildNewImageThumbs(previousOutSelectedSlide - 1, num, newHeight, ref ImageName, TotalImagesCount, InPrefix, CurSelectedSlide, backColor, InToolTip, ExternalPP);
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);

                    Console.WriteLine(e.ToString());
                }

                InFlowPanel.ScrollControlIntoView(InCanvas[CurSelectedSlide - 1]);

                System.Threading.Interlocked.Increment(ref LoadThumbOutlockkey);
                previousOutSelectedSlide = CurSelectedSlide;
            }
        }

        /// <summary>
        /// daniel 4265 ?? ????
        /// </summary>
        /// <param name="InFlowPanel"></param>
        /// <param name="InCanvas"></param>
        /// <param name="ImageName"></param>
        /// <param name="TotalImagesCount"></param>
        /// <param name="PanelWidth"></param>
        /// <param name="InPrefix"></param>
        /// <param name="CurSelectedSlide"></param>
        /// <param name="InToolTip"></param>
        /// <param name="ExternalPP"></param>
        private void LoadThumbPreviewImages(FlowLayoutPanel InFlowPanel, ref ImageCanvas[] InCanvas, string[] ImageName, int TotalImagesCount, int PanelWidth, string InPrefix, int CurSelectedSlide, ToolTip InToolTip, bool ExternalPP)
        {
            Console.WriteLine($"[ThumbPreview] LoadThumbPreviewImages: Panel={InFlowPanel?.Name}, TotalImagesCount={TotalImagesCount}, CurSelectedSlide={CurSelectedSlide}, PanelWidth={PanelWidth}, CanvasNull={InCanvas == null}");
            if (InCanvas != null && TotalImagesCount > 0)
            {
                if (CurSelectedSlide <= 0)
                {
                    CurSelectedSlide = 1;
                }
                Color backColor = InFlowPanel.BackColor;
                int num = (PanelWidth - 35) / 3;
                int newHeight = num * 3 / 4;

                try
                {

                    if (LoadThumbPreviewlockkey == 0)
                    {
                        // ✅ 첫 로드: 모든 썸네일 로드
                        for (int i = 0; i < TotalImagesCount; i++)
                        {
                            InCanvas[i].BuildNewImageThumbs(i, num, newHeight, ref ImageName, TotalImagesCount, InPrefix, CurSelectedSlide, backColor, InToolTip, ExternalPP);
                        }
                    }
                    else
                    {
                        if (previousPreviewSelectedSlide != CurSelectedSlide)
                        {
                            // ✅ 최적화: 현재 + 이전 슬라이드만 업데이트
                            InCanvas[CurSelectedSlide - 1].BuildNewImageThumbs(CurSelectedSlide - 1, num, newHeight, ref ImageName, TotalImagesCount, InPrefix, CurSelectedSlide, backColor, InToolTip, ExternalPP);

                            if (previousPreviewSelectedSlide > 0 && previousPreviewSelectedSlide <= TotalImagesCount)
                            {
                                InCanvas[previousPreviewSelectedSlide - 1].BuildNewImageThumbs(previousPreviewSelectedSlide - 1, num, newHeight, ref ImageName, TotalImagesCount, InPrefix, CurSelectedSlide, backColor, InToolTip, ExternalPP);
                            }

                            // ✅ 추가 최적화: 주변 슬라이드도 미리 로드 (다음 선택 대비)
                            if (CurSelectedSlide < TotalImagesCount)
                            {
                                InCanvas[CurSelectedSlide].BuildNewImageThumbs(CurSelectedSlide, num, newHeight, ref ImageName, TotalImagesCount, InPrefix, CurSelectedSlide, backColor, InToolTip, ExternalPP);
                            }
                            if (CurSelectedSlide > 1)
                            {
                                InCanvas[CurSelectedSlide - 2].BuildNewImageThumbs(CurSelectedSlide - 2, num, newHeight, ref ImageName, TotalImagesCount, InPrefix, CurSelectedSlide, backColor, InToolTip, ExternalPP);
                            }
                            Console.WriteLine($"[ThumbPreview] LoadThumbPreviewImages incremental update: CurSelectedSlide={CurSelectedSlide}, PrevSelectedSlide={previousPreviewSelectedSlide}");
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);

                    Console.WriteLine(e.ToString());
                }

                if (CurSelectedSlide > 0 && CurSelectedSlide <= TotalImagesCount)
                {
                    InFlowPanel.ScrollControlIntoView(InCanvas[CurSelectedSlide - 1]);
                }

                System.Threading.Interlocked.Increment(ref LoadThumbPreviewlockkey);
                previousPreviewSelectedSlide = CurSelectedSlide;

            }
        }

        private void LoadThumbImages(FlowLayoutPanel InFlowPanel, ref ImageCanvas[] InCanvas, string[] ImageName, int TotalImagesCount, int PanelWidth, string InPrefix, int CurSelectedSlide, ToolTip InToolTip, bool ExternalPP)
        {
            if (InCanvas != null)
            {
                Color backColor = InFlowPanel.BackColor;

                int num = (PanelWidth - 35) / 3;
                int newHeight = num * 3 / 4;

                for (int i = 0; i < TotalImagesCount; i++)
                {
                    InCanvas[i].BuildNewImageThumbs(i, num, newHeight, ref ImageName, TotalImagesCount, InPrefix, CurSelectedSlide, backColor, InToolTip, ExternalPP);
                }
                try
                {
                    //InFlowPanel.ScrollControlIntoView(InCanvas[CurSelectedSlide - 1]);

                    InFlowPanel.ScrollControlIntoView(InCanvas[CurSelectedSlide <= 0 ? 0 : CurSelectedSlide - 1]);
                }
                catch
                {
                }
            }
        }

        private void FormatExternalPowerpointThumbContainers()
        {
            gf.ThumbImagesPerRow = 3;
            int height = SongsList.Height;
            flowLayoutExternalPowerPoint.Controls.Clear();
            PowerpointCurPreview = "";
            for (int i = 0; i < ExternalPPTotalImagesCount; i++)
            {
                if (Powerpoint_ExternalCanvas[i] == null)
                {
                    Powerpoint_ExternalCanvas[i] = new ImageCanvas();
                    Powerpoint_ExternalCanvas[i].Tag = i.ToString();
                    Powerpoint_ExternalCanvas[i].FileName = "";
                    Powerpoint_ExternalCanvas[i].MouseUp += ExternalFilesPP_MouseUp;
                    Powerpoint_ExternalCanvas[i].DoubleClick += ExternalFilesPP_DoubleClick;
                }
                else
                {
                    Powerpoint_ExternalCanvas[i].FileName = "";
                }
                flowLayoutExternalPowerPoint.Controls.Add(Powerpoint_ExternalCanvas[i]);
            }
            LoadExternalPowerpointThumbImages(0);
        }
        private void SetWorshipPraiseListColWidth()
        {
            if (WorshipListItems.Columns.Count > 0)
            {
                WorshipListItems.Columns[0].Width = ((WorshipListItems.Width - 25 >= 0) ? (WorshipListItems.Width - 25) : 0);
            }
            if (PraiseBookItems.Items.Count > 0)
            {
                PraiseBookItems.Columns[2].Width = ((PraiseBookItems.Width - PraiseBookItems.Columns[1].Width - 25 >= 0) ? (PraiseBookItems.Width - PraiseBookItems.Columns[1].Width - 25) : 0);
            }
        }

        private void BuildVerseButtons(SongSettings InItem)
        {
            BuildVerseButtons(InItem, Reset: false);
        }

        private void BuildVerseButtons(SongSettings InItem, bool Reset)
        {
            if (InItem.OutputStyleScreen)
            {
                BuildOutputVerseButtons(Reset);
            }
            else
            {
                BuildPreviewVerseButtons(Reset);
            }
        }

        private void BuildOutputVerseButtons()
        {
            BuildOutputVerseButtons(Reset: false);
        }

        private void BuildOutputVerseButtons(bool Reset)
        {
            OutputBtnVersePreChorus.Visible = false;
            OutputBtnVersePreChorus2.Visible = false;
            OutputBtnVerseChorus.Visible = false;
            OutputBtnVerseChorus2.Visible = false;
            OutputBtnVerseBridge.Visible = false;
            OutputBtnVerseBridge2.Visible = false;
            OutputBtnVerseEnding.Visible = false;
            if (Reset)
            {
                OutputBtnVerse1.Visible = false;
                OutputBtnVerse2.Visible = false;
                OutputBtnVerse3.Visible = false;
                OutputBtnVerse4.Visible = false;
                OutputBtnVerse5.Visible = false;
                OutputBtnVerse6.Visible = false;
                OutputBtnVerse7.Visible = false;
                OutputBtnVerse8.Visible = false;
                OutputBtnVerse9.Visible = false;
                return;
            }
            OutputBtnVerse1.Visible = ((gf.OutputItem.SongVerses[1] > 0) ? true : false);
            OutputBtnVerse2.Visible = ((gf.OutputItem.SongVerses[2] > 0) ? true : false);
            OutputBtnVerse3.Visible = ((gf.OutputItem.SongVerses[3] > 0) ? true : false);
            OutputBtnVerse4.Visible = ((gf.OutputItem.SongVerses[4] > 0) ? true : false);
            OutputBtnVerse5.Visible = ((gf.OutputItem.SongVerses[5] > 0) ? true : false);
            OutputBtnVerse6.Visible = ((gf.OutputItem.SongVerses[6] > 0) ? true : false);
            OutputBtnVerse7.Visible = ((gf.OutputItem.SongVerses[7] > 0) ? true : false);
            OutputBtnVerse8.Visible = ((gf.OutputItem.SongVerses[8] > 0) ? true : false);
            OutputBtnVerse9.Visible = ((gf.OutputItem.SongVerses[9] > 0) ? true : false);
            bool flag = false;
            bool flag2 = false;
            bool flag3 = false;
            bool flag4 = false;
            bool flag5 = false;
            bool flag6 = false;
            bool flag7 = false;
            for (int i = 1; i <= gf.OutputItem.TotalSlides; i++)
            {
                if (gf.OutputItem.Slide[i, 0] == 111)
                {
                    flag6 = true;
                }
                if (gf.OutputItem.Slide[i, 0] == 112)
                {
                    flag7 = true;
                }
                if (gf.OutputItem.Slide[i, 0] == 0)
                {
                    flag = true;
                }
                if (gf.OutputItem.Slide[i, 0] == 102)
                {
                    flag4 = true;
                }
                if (gf.OutputItem.Slide[i, 0] == 100)
                {
                    flag2 = true;
                }
                if (gf.OutputItem.Slide[i, 0] == 103)
                {
                    flag3 = true;
                }
                if (gf.OutputItem.Slide[i, 0] == 101)
                {
                    flag5 = true;
                }
            }
            if (flag6)
            {
                OutputBtnVersePreChorus.Visible = true;
            }
            if (flag7)
            {
                OutputBtnVersePreChorus2.Visible = true;
            }
            if (flag)
            {
                OutputBtnVerseChorus.Visible = true;
            }
            if (flag4)
            {
                OutputBtnVerseChorus2.Visible = true;
            }
            if (flag2)
            {
                OutputBtnVerseBridge.Visible = true;
            }
            if (flag3)
            {
                OutputBtnVerseBridge2.Visible = true;
            }
            if (flag5)
            {
                OutputBtnVerseEnding.Visible = true;
            }
        }

        private void BuildPreviewVerseButtons()
        {
            BuildPreviewVerseButtons(Reset: false);
        }

        private void BuildPreviewVerseButtons(bool Reset)
        {
            PreviewBtnVersePreChorus.Visible = false;
            PreviewBtnVersePreChorus2.Visible = false;
            PreviewBtnVerseChorus.Visible = false;
            PreviewBtnVerseChorus2.Visible = false;
            PreviewBtnVerseBridge.Visible = false;
            PreviewBtnVerseBridge2.Visible = false;
            PreviewBtnVerseEnding.Visible = false;
            if (Reset)
            {
                PreviewBtnVerse1.Visible = false;
                PreviewBtnVerse2.Visible = false;
                PreviewBtnVerse3.Visible = false;
                PreviewBtnVerse4.Visible = false;
                PreviewBtnVerse5.Visible = false;
                PreviewBtnVerse6.Visible = false;
                PreviewBtnVerse7.Visible = false;
                PreviewBtnVerse8.Visible = false;
                PreviewBtnVerse9.Visible = false;
                return;
            }
            PreviewBtnVerse1.Visible = ((gf.PreviewItem.SongVerses[1] > 0) ? true : false);
            PreviewBtnVerse2.Visible = ((gf.PreviewItem.SongVerses[2] > 0) ? true : false);
            PreviewBtnVerse3.Visible = ((gf.PreviewItem.SongVerses[3] > 0) ? true : false);
            PreviewBtnVerse4.Visible = ((gf.PreviewItem.SongVerses[4] > 0) ? true : false);
            PreviewBtnVerse5.Visible = ((gf.PreviewItem.SongVerses[5] > 0) ? true : false);
            PreviewBtnVerse6.Visible = ((gf.PreviewItem.SongVerses[6] > 0) ? true : false);
            PreviewBtnVerse7.Visible = ((gf.PreviewItem.SongVerses[7] > 0) ? true : false);
            PreviewBtnVerse8.Visible = ((gf.PreviewItem.SongVerses[8] > 0) ? true : false);
            PreviewBtnVerse9.Visible = ((gf.PreviewItem.SongVerses[9] > 0) ? true : false);
            bool flag = false;
            bool flag2 = false;
            bool flag3 = false;
            bool flag4 = false;
            bool flag5 = false;
            bool flag6 = false;
            bool flag7 = false;
            for (int i = 1; i <= gf.PreviewItem.TotalSlides; i++)
            {
                if (gf.PreviewItem.Slide[i, 0] == 111)
                {
                    flag6 = true;
                }
                if (gf.PreviewItem.Slide[i, 0] == 112)
                {
                    flag7 = true;
                }
                if (gf.PreviewItem.Slide[i, 0] == 0)
                {
                    flag = true;
                }
                if (gf.PreviewItem.Slide[i, 0] == 102)
                {
                    flag4 = true;
                }
                if (gf.PreviewItem.Slide[i, 0] == 100)
                {
                    flag2 = true;
                }
                if (gf.PreviewItem.Slide[i, 0] == 103)
                {
                    flag3 = true;
                }
                if (gf.PreviewItem.Slide[i, 0] == 101)
                {
                    flag5 = true;
                }
            }
            if (flag6)
            {
                PreviewBtnVersePreChorus.Visible = true;
            }
            if (flag7)
            {
                PreviewBtnVersePreChorus2.Visible = true;
            }
            if (flag)
            {
                PreviewBtnVerseChorus.Visible = true;
            }
            if (flag4)
            {
                PreviewBtnVerseChorus2.Visible = true;
            }
            if (flag2)
            {
                PreviewBtnVerseBridge.Visible = true;
            }
            if (flag3)
            {
                PreviewBtnVerseBridge2.Visible = true;
            }
            if (flag5)
            {
                PreviewBtnVerseEnding.Visible = true;
            }
        }

        private void UpdateFormatData()
        {
            UpdateFormatData(StartAtFirstSlide: true);
        }

        private void UpdateFormatData(bool StartAtFirstSlide)
        {
            UpdateFormatData(StartAtFirstSlide, ImageTransitionControl.TransitionAction.None);
        }

        private void UpdateFormatData(bool StartAtFirstSlide, ImageTransitionControl.TransitionAction TransitionAction)
        {
            gf.PreviewItem.Format.FormatString = ((!Ind_checkBox.Checked) ? "" : GetNewFormatString());
            if (gf.PreviewItem.Type == "I")
            {
                SaveInfoFilePreview(ReloadImageData: false);
            }
            if (gf.PreviewItem.Source == ItemSource.WorshipList)
            {
                int selectedIndex = gf.GetSelectedIndex(WorshipListItems);
                if (selectedIndex < 0)
                {
                    return;
                }
                WorshipListItems.Items[selectedIndex].SubItems[2].Text = gf.PreviewItem.Format.FormatString;
                //  ?? ???? 
                gf.SaveFormatStringToDatabase(gf.PreviewItem.ItemID, gf.PreviewItem.Format.FormatString);
                SaveWorshipList();
            }
            else if (gf.PreviewItem.Source == ItemSource.SongsList)
            {
                gf.SaveFormatStringToDatabase(gf.PreviewItem.ItemID, gf.PreviewItem.Format.FormatString);
            }
            else if (gf.PreviewItem.Source == ItemSource.HolyBible)
            {
                HB_CurSelectedFormat = gf.PreviewItem.Format.FormatString;
            }
            ShowSong(ref gf.PreviewItem, (!StartAtFirstSlide) ? gf.PreviewItem.CurSlide : 0, TransitionAction);
            FormatLyricsContainers(gf.PreviewItem);
        }

        private void AppyPreviewChangesToLive(int Selecteditem)
        {
            AppyPreviewChangesToLive(Selecteditem, StartAtFirstSlide: true);
        }

        private void AppyPreviewChangesToLive(int Selecteditem, bool StartAtFirstSlide)
        {
            if (gf.OutputItem.CurItemNo != Selecteditem + 1)
            {
                return;
            }
            gf.LoadIndividualFormatData(ref gf.OutputItem, gf.PreviewItem.Format.FormatString);
            gf.SetShowBackground(gf.OutputItem, ref OutputScreen);
            if (StartAtFirstSlide)
            {
                if (gf.ShowRunning)
                {
                    gf.MainAction_SongChanged_Transaction = ImageTransitionControl.TransitionAction.None;
                    RemoteControlLiveShow(LiveShowAction.Remote_SongChanged);
                }
            }
            else
            {
                gf.MainAction_SongChanged_Transaction = ImageTransitionControl.TransitionAction.None;
                if (gf.ShowRunning)
                {
                    RemoteControlLiveShow(LiveShowAction.Remote_BackgroundChanged);
                }
            }
            if (gf.OutputItem.ItemID != "")
            {
                RefreshSlidesFonts(ref gf.OutputItem);
                if (StartAtFirstSlide)
                {
                    ShowSong(ref gf.OutputItem);
                    FormatLyricsContainers(gf.OutputItem);
                }
            }
        }

        private string GetNewFormatString()
        {
            int num = gf.PreviewItem.Format.ShowFontBold[0] + gf.PreviewItem.Format.ShowFontItalic[0] * 2 + gf.PreviewItem.Format.ShowFontUnderline[0] * 4 + gf.PreviewItem.Format.ShowFontBold[2] * 8 + gf.PreviewItem.Format.ShowFontItalic[2] * 16 + gf.PreviewItem.Format.ShowFontUnderline[2] * 32;
            int num2 = gf.PreviewItem.Format.ShowFontBold[1] + gf.PreviewItem.Format.ShowFontItalic[1] * 2 + gf.PreviewItem.Format.ShowFontUnderline[1] * 4 + gf.PreviewItem.Format.ShowFontBold[3] * 8 + gf.PreviewItem.Format.ShowFontItalic[3] * 16 + gf.PreviewItem.Format.ShowFontUnderline[3] * 32;
            int num3 = gf.PreviewItem.Format.MediaMute + gf.PreviewItem.Format.MediaRepeat * 2 + gf.PreviewItem.Format.MediaWidescreen * 4;
            int num4 = gf.PreviewItem.Format.UseShadowFont * 2 + gf.PreviewItem.Format.ShowNotations * 4 + gf.PreviewItem.Format.ShowInterlace * 16 + gf.PreviewItem.Format.UseOutlineFont * 32 + gf.PreviewItem.Format.HideDisplayPanel * 64;
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(Convert.ToString(21) + "=" + Convert.ToString(gf.PreviewItem.Format.ShowSongHeadings) + '>');
            stringBuilder.Append(Convert.ToString(23) + "=" + Convert.ToString(gf.PreviewItem.Format.ShowSongHeadingsAlign) + '>');
            stringBuilder.Append(Convert.ToString(22) + "=" + num4.ToString() + '>');
            stringBuilder.Append(Convert.ToString(25) + "=" + Convert.ToString(gf.PreviewItem.Format.ShowLyrics) + '>');
            stringBuilder.Append(Convert.ToString(26) + "=" + Convert.ToString(gf.PreviewItem.Format.ShowScreenColour[0].ToArgb()) + '>');
            stringBuilder.Append(Convert.ToString(27) + "=" + Convert.ToString(gf.PreviewItem.Format.ShowScreenColour[1].ToArgb()) + '>');
            stringBuilder.Append(Convert.ToString(28) + "=" + gf.PreviewItem.Format.ShowScreenStyle.ToString() + '>');
            stringBuilder.Append(Convert.ToString(29) + "=" + Convert.ToString(gf.PreviewItem.Format.ShowFontColour[0].ToArgb()) + '>');
            stringBuilder.Append(Convert.ToString(30) + "=" + Convert.ToString(gf.PreviewItem.Format.ShowFontColour[1].ToArgb()) + '>');
            stringBuilder.Append(Convert.ToString(31) + "=" + Convert.ToString(gf.PreviewItem.Format.ShowFontAlign[0]) + '>');
            stringBuilder.Append(Convert.ToString(32) + "=" + Convert.ToString(gf.PreviewItem.Format.ShowFontAlign[1]) + '>');
            stringBuilder.Append(Convert.ToString(41) + "=" + num.ToString() + '>');
            stringBuilder.Append(Convert.ToString(42) + "=" + num2.ToString() + '>');
            stringBuilder.Append(Convert.ToString(43) + "=" + gf.PreviewItem.Format.ShowFontName[0] + '>');
            stringBuilder.Append(Convert.ToString(44) + "=" + gf.PreviewItem.Format.ShowFontName[1] + '>');
            stringBuilder.Append(Convert.ToString(45) + "=" + Convert.ToString(gf.PreviewItem.Format.ShowFontVPosition[0]) + '>');
            stringBuilder.Append(Convert.ToString(46) + "=" + Convert.ToString(gf.PreviewItem.Format.ShowFontVPosition[1]) + '>');
            stringBuilder.Append(Convert.ToString(47) + "=" + Convert.ToString(gf.PreviewItem.Format.ShowFontSize[0]) + '>');
            stringBuilder.Append(Convert.ToString(48) + "=" + Convert.ToString(gf.PreviewItem.Format.ShowFontSize[1]) + '>');
            stringBuilder.Append(Convert.ToString(50) + "=" + Convert.ToString(gf.PreviewItem.Format.MediaOption) + '>');
            stringBuilder.Append(Convert.ToString(51) + "=" + gf.PreviewItem.Format.MediaLocation + '>');
            stringBuilder.Append(Convert.ToString(52) + "=" + Convert.ToString(gf.PreviewItem.Format.MediaVolume) + '>');
            stringBuilder.Append(Convert.ToString(53) + "=" + Convert.ToString(gf.PreviewItem.Format.MediaBalance) + '>');
            stringBuilder.Append(Convert.ToString(54) + "=" + num3.ToString() + '>');
            stringBuilder.Append(Convert.ToString(55) + "=" + Convert.ToString(gf.PreviewItem.Format.MediaCaptureDeviceNumber) + '>');
            stringBuilder.Append(Convert.ToString(56) + "=" + gf.PreviewItem.Format.MediaOutputMonitorName + '>');
            stringBuilder.Append(Convert.ToString(61) + "=" + gf.PreviewItem.Format.BackgroundPicture + '>');
            stringBuilder.Append(Convert.ToString(62) + "=" + Convert.ToString((int)gf.PreviewItem.Format.BackgroundMode) + '>');
            stringBuilder.Append(Convert.ToString(63) + "=" + Convert.ToString(gf.PreviewItem.Format.ShowVerticalAlign) + '>');
            stringBuilder.Append(Convert.ToString(64) + "=" + Convert.ToString(gf.PreviewItem.Format.ShowLeftMargin) + '>');
            stringBuilder.Append(Convert.ToString(65) + "=" + Convert.ToString(gf.PreviewItem.Format.ShowRightMargin) + '>');
            stringBuilder.Append(Convert.ToString(66) + "=" + Convert.ToString(gf.PreviewItem.Format.ShowBottomMargin) + '>');
            stringBuilder.Append(Convert.ToString(71) + "=" + Convert.ToString(gf.PreviewItem.Format.TransposeOffset) + '>');
            stringBuilder.Append(Convert.ToString(72) + "=" + PreviewScreen.GetTransitionText(gf.PreviewItem.Format.ShowItemTransition) + '>');
            stringBuilder.Append(Convert.ToString(73) + "=" + PreviewScreen.GetTransitionText(gf.PreviewItem.Format.ShowSlideTransition) + '>');
            return stringBuilder.ToString();
        }

        private void UpdateFormatFields()
        {
            UpdatingFormatFields = true;
            string text = "";
            gf.AssignDropDownItem(SelectedMenuItemName: (gf.PreviewItem.Format.ShowSongHeadings == 0) ? Ind_HeadNoTitles.Name : ((gf.PreviewItem.Format.ShowSongHeadings != 1) ? Ind_HeadFirstScreen.Name : Ind_HeadAllTitles.Name), SelectedBtn: ref Ind_Head, InMenuItem1: Ind_HeadNoTitles, InMenuItem2: Ind_HeadAllTitles, InMenuItem3: Ind_HeadFirstScreen);
            gf.AssignDropDownItem(SelectedMenuItemName: (gf.PreviewItem.Format.ShowSongHeadingsAlign == 1) ? Ind_HeadAlignAsR2.Name : ((gf.PreviewItem.Format.ShowSongHeadingsAlign == 2) ? Ind_HeadAlignLeft.Name : ((gf.PreviewItem.Format.ShowSongHeadingsAlign == 3) ? Ind_HeadAlignCentre.Name : ((gf.PreviewItem.Format.ShowSongHeadingsAlign != 4) ? Ind_HeadAlignAsR1.Name : Ind_HeadAlignRight.Name))), SelectedBtn: ref Ind_HeadAlign, InMenuItem1: Ind_HeadAlignAsR1, InMenuItem2: Ind_HeadAlignAsR2, InMenuItem3: Ind_HeadAlignLeft, InMenuItem4: Ind_HeadAlignCentre, InMenuItem5: Ind_HeadAlignRight);
            Ind_Shadow.Checked = ((gf.PreviewItem.Format.UseShadowFont == 1) ? true : false);
            Ind_Outline.Checked = ((gf.PreviewItem.Format.UseOutlineFont == 1) ? true : false);
            gf.AssignDropDownItem(SelectedMenuItemName: (gf.PreviewItem.Format.ShowLyrics == 0) ? Ind_ShowRegion1.Name : ((gf.PreviewItem.Format.ShowLyrics != 1) ? Ind_ShowRegionBoth.Name : Ind_ShowRegion2.Name), SelectedBtn: ref Ind_Region, InMenuItem1: Ind_ShowRegion1, InMenuItem2: Ind_ShowRegion2, InMenuItem3: Ind_ShowRegionBoth);
            gf.AssignDropDownItem(SelectedMenuItemName: (gf.PreviewItem.Format.ShowVerticalAlign == 0) ? Ind_VAlignTop.Name : ((gf.PreviewItem.Format.ShowVerticalAlign != 1) ? Ind_VAlignBottom.Name : Ind_VAlignCentre.Name), SelectedBtn: ref Ind_VAlign, InMenuItem1: Ind_VAlignTop, InMenuItem2: Ind_VAlignCentre, InMenuItem3: Ind_VAlignBottom);
            Ind_Interlace.Checked = ((gf.PreviewItem.Format.ShowInterlace == 1) ? true : false);
            Ind_Notations.Checked = ((gf.PreviewItem.Format.ShowNotations == 1) ? true : false);
            Ind_HideDisplayPanel.Checked = ((gf.PreviewItem.Format.HideDisplayPanel == 1) ? true : false);
            Ind_LeftUpDown.Value = gf.PreviewItem.Format.ShowLeftMargin;
            Ind_RightUpDown.Value = gf.PreviewItem.Format.ShowRightMargin;
            gf.UpdatePosUpDowns(ref Ind_Reg1TopUpDown, ref Ind_Reg2TopUpDown, ref Ind_BottomUpDown, ref gf.PreviewItem.Format.ShowFontVPosition[0], ref gf.PreviewItem.Format.ShowFontVPosition[1], gf.PreviewItem.Format.ShowBottomMargin);
            gf.AssignDropDownItem(SelectedMenuItemName: (gf.PreviewItem.Format.BackgroundMode == ImageMode.Tile) ? Ind_ImageTile.Name : ((gf.PreviewItem.Format.BackgroundMode != ImageMode.Centre) ? Ind_ImageBestFit.Name : Ind_ImageCentre.Name), SelectedBtn: ref Ind_ImageMode, InMenuItem1: Ind_ImageTile, InMenuItem2: Ind_ImageCentre, InMenuItem3: Ind_ImageBestFit);
            Ind_NoImage.Enabled = ((gf.PreviewItem.Format.BackgroundPicture != "") ? true : false);
            Ind_BackColour.ForeColor = gf.PreviewItem.Format.ShowScreenColour[0];
            Ind_R1Bold.Checked = ((gf.PreviewItem.Format.ShowFontBold[0] > 0) ? true : false);
            gf.AssignDropDownItem(SelectedMenuItemName: (gf.PreviewItem.Format.ShowFontItalic[0] > 0 && gf.PreviewItem.Format.ShowFontItalic[2] > 0) ? Ind_R1Italics1.Name : ((gf.PreviewItem.Format.ShowFontItalic[2] <= 0) ? Ind_R1Italics0.Name : Ind_R1Italics2.Name), SelectedBtn: ref Ind_R1Italics, InMenuItem1: Ind_R1Italics0, InMenuItem2: Ind_R1Italics1, InMenuItem3: Ind_R1Italics2);
            Ind_R1Underline.Checked = ((gf.PreviewItem.Format.ShowFontUnderline[0] > 0) ? true : false);
            text = ((gf.PreviewItem.Format.ShowFontAlign[0] == 1) ? Ind_R1AlignLeft.Name : ((gf.PreviewItem.Format.ShowFontAlign[0] != 2) ? Ind_R1AlignRight.Name : Ind_R1AlignCentre.Name));
            Ind_R1Colour.ForeColor = gf.PreviewItem.Format.ShowFontColour[0];
            gf.AssignDropDownItem(ref Ind_R1Align, text, Ind_R1AlignLeft, Ind_R1AlignCentre, Ind_R1AlignRight);
            Ind_Reg1FontsList.Text = gf.PreviewItem.Format.ShowFontName[0];
            Ind_Reg1SizeUpDown.Value = gf.PreviewItem.Format.ShowFontSize[0];
            Ind_R2Bold.Checked = ((gf.PreviewItem.Format.ShowFontBold[1] > 0) ? true : false);
            gf.AssignDropDownItem(SelectedMenuItemName: (gf.PreviewItem.Format.ShowFontItalic[1] > 0 && gf.PreviewItem.Format.ShowFontItalic[3] > 0) ? Ind_R2Italics1.Name : ((gf.PreviewItem.Format.ShowFontItalic[3] <= 0) ? Ind_R2Italics0.Name : Ind_R2Italics2.Name), SelectedBtn: ref Ind_R2Italics, InMenuItem1: Ind_R2Italics0, InMenuItem2: Ind_R2Italics1, InMenuItem3: Ind_R2Italics2);
            Ind_R2Underline.Checked = ((gf.PreviewItem.Format.ShowFontUnderline[1] > 0) ? true : false);
            text = ((gf.PreviewItem.Format.ShowFontAlign[1] == 1) ? Ind_R2AlignLeft.Name : ((gf.PreviewItem.Format.ShowFontAlign[1] != 2) ? Ind_R2AlignRight.Name : Ind_R2AlignCentre.Name));
            Ind_R2Colour.ForeColor = gf.PreviewItem.Format.ShowFontColour[1];
            gf.AssignDropDownItem(ref Ind_R2Align, text, Ind_R2AlignLeft, Ind_R2AlignCentre, Ind_R2AlignRight);
            Ind_Reg2FontsList.Text = gf.PreviewItem.Format.ShowFontName[1];
            Ind_Reg2SizeUpDown.Value = gf.PreviewItem.Format.ShowFontSize[1];
            Ind_TransItem.SelectedIndex = gf.PreviewItem.Format.ShowItemTransition;
            Ind_TransSlides.SelectedIndex = gf.PreviewItem.Format.ShowSlideTransition;
            AssignMediaText(ref Ind_AssignMedia, gf.PreviewItem.Format.MediaOption);
            UpdatingFormatFields = false;
        }

        //private void UpdateDefaultNoImageButton()
        //{
        //}

        private void UpdateBackgroundImageButtons(int InMode, int BackgroundImageMode)
        {
            if (BackgroundImageMode < 0 || BackgroundImageMode > 2)
            {
                return;
            }
            string text = "";
            if (InMode == 1 || InMode == 2)
            {
                text = "";
                if (!Ind_checkBox.Checked)
                {
                    if (BackgroundImageMode.ToString() == Ind_ImageTile.Tag.ToString())
                    {
                        text = "Ind_ImageTile";
                    }
                    else if (BackgroundImageMode.ToString() == Ind_ImageCentre.Tag.ToString())
                    {
                        text = "Ind_ImageCentre";
                    }
                    else if (BackgroundImageMode.ToString() == Ind_ImageBestFit.Tag.ToString())
                    {
                        text = "Ind_ImageBestFit";
                    }
                    if (text != "")
                    {
                        gf.AssignDropDownItem(ref Ind_ImageMode, text, Ind_ImageTile, Ind_ImageCentre, Ind_ImageBestFit);
                    }
                }
            }
            if (InMode == 0 || InMode == 2)
            {
                text = "";
                if (BackgroundImageMode.ToString() == Def_ImageTile.Tag.ToString())
                {
                    text = "Def_ImageTile";
                }
                else if (BackgroundImageMode.ToString() == Def_ImageCentre.Tag.ToString())
                {
                    text = "Def_ImageCentre";
                }
                else if (BackgroundImageMode.ToString() == Def_ImageBestFit.Tag.ToString())
                {
                    text = "Def_ImageBestFit";
                }
                if (text != "")
                {
                    gf.AssignDropDownItem(ref Def_ImageMode, text, Def_ImageTile, Def_ImageCentre, Def_ImageBestFit);
                }
            }
            UpdateDefaultNoImageButton();
        }
    }
}
