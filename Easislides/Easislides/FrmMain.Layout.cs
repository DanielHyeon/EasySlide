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
            int splitterDistance = 0;
            splitterDistance = RegUtil.GetRegValue("settings", Splitter_FolderWidth, 230);
            if (splitterDistance < 25 || splitterDistance > 1500)
            {
                splitterDistance = 230;
            }
            splitContainerMain.SplitterDistance = splitterDistance;
            splitterDistance = RegUtil.GetRegValue("settings", Splitter_OutputWidth, 250);
            if (splitterDistance < 25 || splitterDistance > 1500)
            {
                splitterDistance = 250;
            }
            splitContainer2.SplitterDistance = splitterDistance;
            splitterDistance = RegUtil.GetRegValue("settings", Splitter_FolderHeight, 250);
            if (splitterDistance < 25 || splitterDistance > 1500)
            {
                splitterDistance = 250;
            }
            splitContainer1.SplitterDistance = splitterDistance;
            splitterDistance = RegUtil.GetRegValue("settings", Splitter_PreviewLyricsHeight, 250);
            if (splitterDistance < 25 || splitterDistance > 1500)
            {
                splitterDistance = 250;
            }
            splitContainerPreview.SplitterDistance = splitterDistance;
            splitterDistance = RegUtil.GetRegValue("settings", Splitter_outputLyricsHeight, 250);
            if (splitterDistance < 25 || splitterDistance > 1500)
            {
                splitterDistance = 250;
            }
            splitContainerOutput.SplitterDistance = splitterDistance;
        }

        /// <summary>
        /// Restores the main window position and splitters, sets up the panels and loads the lists.
        /// Returns false when the application data could not be initialised.
        /// </summary>
        private bool InitFormControls()
        {
            Gf.isScreenWideMode = ((DataUtil.ObjToInt(RegUtil.GetRegValue("monitors", "IsMonitorWide", 0)) > 0) ? true : false);

            Gf.ShowTaskBar();
            int mainLeft = RegUtil.GetRegValue("settings", "MainLeft", -1);
            int mainTop = RegUtil.GetRegValue("settings", "MainTop", -1);
            int mainWidth = RegUtil.GetRegValue("settings", "MainWidth", 720);
            int mainHeight = RegUtil.GetRegValue("settings", "MainHeight", 560);
            if (mainWidth > Screen.PrimaryScreen.Bounds.Width)
            {
                mainWidth = Screen.PrimaryScreen.Bounds.Width - 20;
            }
            if (mainHeight > Screen.PrimaryScreen.Bounds.Height)
            {
                mainHeight = Screen.PrimaryScreen.Bounds.Height - 30;
            }
            if (mainLeft < 0)
            {
                mainLeft = 0;
            }
            if (mainTop < 0)
            {
                mainTop = 0;
            }
            if (mainLeft + mainWidth > Screen.PrimaryScreen.Bounds.Width)
            {
                mainLeft = Screen.PrimaryScreen.Bounds.Width - mainWidth;
            }
            if (mainTop + mainHeight > Screen.PrimaryScreen.Bounds.Height)
            {
                mainTop = Screen.PrimaryScreen.Bounds.Height - mainHeight;
            }
            base.Left = mainLeft;
            base.Top = mainTop;
            base.Height = mainHeight;
            base.Width = mainWidth;
            frmLeft = mainLeft;
            frmTop = mainTop;
            frmHeight = mainHeight;
            frmWidth = mainWidth;
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
            Gf.BuildFontsList(ref Ind_Reg1FontsList);
            Gf.BuildFontsList(ref Ind_Reg2FontsList);
            Gf.BuildFontsList(ref Def_PanelFontList);
            PreviewScreen.BuildTransitionsList(ref Def_TransItem);
            PreviewScreen.BuildTransitionsList(ref Def_TransSlides);
            PreviewScreen.BuildTransitionsList(ref Ind_TransItem);
            PreviewScreen.BuildTransitionsList(ref Ind_TransSlides);
            if (!Gf.InitAppData())
            {
                return false;
            }
            BuildFolderSortMenu();
            ApplyModernUi();
            ApplyUseSongNumbers(Gf.UseSongNumbers);
            BuildFolderList();

            SetJumpToolTips();
            SetMainDefaultBackScreen();

            var task2 = Task.Factory.StartNew(() =>
            {
                PopulatePraiseBooksList();
            }, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Current);

            PraiseBook.Text = "";
            Gf.NormalTextRegionBackColour = SongFolder.BackColor;

            PopulateWorshipList();

            ApplyWorshipMode();
            SetItemFontSettings(ref Gf.PreviewItem);
            ResetMainPictureBox(ref Gf.OutputItem);
            BuildPicturesFolderList();
            BuildInfoScreenFolderList();
            BuildMediaFolderList();
            SetPowerpointListingButton();
            SetTabsVisibility();
            Gf.LoadBibleVersions(ref TabBibleVersions);

            TabBibleVersionsChanged();

            InitFormLoad = false;
            BuildVerseButtons(Gf.PreviewItem, Reset: true);
            BuildVerseButtons(Gf.OutputItem, Reset: true);
            TimerFlasher.Enabled = true;
            DefaultButtonForeColour = btnToOutput.ForeColor;
            PreviewScreen.AllowDrop = true;
            BuildEditHistoryMenuItems();
            EnableEditHistory();
            SetAutoRotateButtons();
            PreviewNotations(Gf.PreviewArea_ShowNotations);
            SetNotesPreview(NotesChecked: false);
            AlertWindow.OnMessage += AlertWindow_OnMessage;
            popupHelper = new PopupWindowHelper();
            popupHelper.PopupClosed += popupHelper_PopupClosed;
            Gf.SplashUp = false;
            Text = "EasiSlides" + ((Gf.UserString != "") ? (" - " + Gf.UserString) : "");

            return true;
        }

        private void ResizePreviewBottomPanel()
        {
            ResizeSampleScreen(panelPreviewBottom, PreviewHolder, PreviewBack, AdjustForIndicator: false);
        }

        private void ResizeOutputBottomPanel()
        {
            ResizeSampleScreen(panelOutputBottom, OutputHolder, OutputBack, Gf.ShowLyricsMonitorAlertBox);
        }

        private void ResizeSampleScreen(Panel InPanelContainer, Panel InHolder, Panel InBack, bool AdjustForIndicator)
        {
            int indicatorHeight = AdjustForIndicator ? panelOutputLM1.Height : 0;
            var available = new Rectangle(0, 0, InPanelContainer.Width * 18 / 20,
                Math.Max(0, InPanelContainer.Height - indicatorHeight - 15));
            Size size = MonitorLayout.Fit(available, Gf.isScreenWideMode).Size;
            int shade = size.Height / 40;
            InHolder.SetBounds((InPanelContainer.Width - size.Width - shade) / 2,
                (InPanelContainer.Height - indicatorHeight - size.Height) / 2, size.Width, size.Height);
            InBack.SetBounds(InHolder.Left + shade + 1, InHolder.Top + shade + 1, size.Width, size.Height);
            SetStatusbarSize();
        }

        /// <summary>
        /// Earlier ResizeSampleScreen (version 1, no wide-screen branch).
        /// </summary>
        private void ResizeSampleScreen_v1(Panel InPanelContainer, Panel InHolder, Panel InBack, bool AdjustForIndicator)
        {
            int containerWidth = InPanelContainer.Width;
            if (containerWidth < 0)
            {
                containerWidth = 0;
            }
            int topOffset = 0;
            int indicatorHeight = AdjustForIndicator ? panelOutputLM1.Height : 0;
            int bottomMargin = 15;
            int holderWidth = containerWidth * 18 / 20;
            int holderHeight = ((holderWidth <= 0) ? 1 : holderWidth) * 3 / 4;
            if (holderHeight > InPanelContainer.Height - (topOffset + indicatorHeight + bottomMargin))
            {
                holderHeight = InPanelContainer.Height - (topOffset + indicatorHeight + bottomMargin);
                holderHeight = ((holderHeight <= 0) ? 1 : holderHeight);
                // daniel
                // holderWidth = holderHeight * 4 / 3;

                //if (Gf.isScreenWideMode)
                //    holderWidth = holderHeight * 5 / 3;
                //else
                holderWidth = holderHeight * 4 / 3;
            }
            InHolder.Left = (InPanelContainer.Width - holderWidth - holderHeight / 40) / 2;
            InHolder.Height = holderHeight;
            InHolder.Width = holderWidth;
            InHolder.Top = (InPanelContainer.Height - (indicatorHeight + holderHeight)) / 2;
            if (InHolder.Top < topOffset + 2 && topOffset > 0)
            {
                InHolder.Top = topOffset + 2;
            }
            if (holderHeight > 0)
            {
                InBack.Left = InHolder.Left + holderHeight / 40 + 1;
                InBack.Top = InHolder.Top + holderHeight / 40 + 1;
                InBack.Height = holderHeight;
                InBack.Width = holderWidth;
            }
            else
            {
                InBack.Height = 0;
                InBack.Width = 0;
            }
            SetStatusbarSize();
        }

        /// <summary>
        /// Original ResizeSampleScreen (version 0, sized from the panel height).
        /// </summary>
        private void ResizeSampleScreen_v0(Panel InPanelContainer, Panel InHolder, Panel InBack, bool AdjustForIndicator)
        {
            int availableHeight = InPanelContainer.Height - (AdjustForIndicator ? labelBlackScreen.Height : 0);
            if (availableHeight < 0)
            {
                availableHeight = 0;
            }
            int holderHeight = availableHeight * 18 / 20;
            int holderWidth = ((holderHeight <= 0) ? 1 : holderHeight) * 4 / 3;
            if (holderWidth > InPanelContainer.Width - 40)
            {
                holderWidth = InPanelContainer.Width - 40;
                holderWidth = ((holderWidth <= 0) ? 1 : holderWidth);
                holderHeight = holderWidth * 3 / 4;
            }
            InHolder.Left = (InPanelContainer.Width - holderWidth - holderHeight / 40) / 2;
            InHolder.Height = holderHeight;
            InHolder.Width = holderWidth;
            InHolder.Top = (AdjustForIndicator ? labelBlackScreen.Height : 0) + (int)((double)(availableHeight - InHolder.Height) / 2.5) + availableHeight / 60;
            if (holderHeight > 0)
            {
                InBack.Left = InHolder.Left + holderHeight / 40 + 1;
                InBack.Top = InHolder.Top + holderHeight / 40 + 1;
                InBack.Height = holderHeight;
                InBack.Width = holderWidth;
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
            IndradioButtonText.ForeColor = (IndradioButtonText.Checked ? Gf.ButtonPressedForeColour : Gf.ButtonDefaultForeColour);
            IndradioButtonFormat.ForeColor = (IndradioButtonFormat.Checked ? Gf.ButtonPressedForeColour : Gf.ButtonDefaultForeColour);
            IndradioButtonInfo.ForeColor = (IndradioButtonInfo.Checked ? Gf.ButtonPressedForeColour : Gf.ButtonDefaultForeColour);
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
                Gf.PreviewItem.UseDefaultFormat = !BoxChecked;
            }
            else
            {
                Ind_checkBox.Enabled = false;
                Gf.PreviewItem.UseDefaultFormat = Gf.PreviewItem.Type != "P";
            }
            Ind_checkBox.Checked = BoxChecked;
            IndgroupBox1.Enabled = BoxChecked && Ind_checkBox.Enabled;
            IndgroupBox2.Enabled = IndgroupBox1.Enabled;
            IndgroupBox3.Enabled = IndgroupBox1.Enabled;
            IndgroupBox4.Enabled = IndgroupBox1.Enabled;
            panelIndTemplate.Enabled = IndgroupBox1.Enabled;
        }

        /// <summary>
        /// Adds output PowerPoint thumbnail <paramref name="index"/> to its panel, creating it when needed.
        /// </summary>
        public void FormatPowerPointThumbContainers2(int index)
        {
            if (index >= flowLayoutOutputPowerPoint.Controls.Count)
                FormatPowerPointThumbContainers(flowLayoutOutputPowerPoint, checked(index + 1));
        }

        private void FormatPowerPointThumbContainers(PowerPointThumbnailPanel panel, int count)
        {
            Gf.ThumbImagesPerRow = 3;
            panel.SetSlideCount(count, toolTip1, index =>
            {
                var canvas = new ImageCanvas
                {
                    Name = panel == flowLayoutPreviewPowerPoint ? "PP_Preview" : "PP_Output",
                    Tag = index.ToString(),
                    SlideNumber = index + 1,
                    PowerPoint = true
                };
                canvas.MouseUp += PowerPointImage_MouseUp;
                canvas.DoubleClick += PowerPointImage_DoubleClick;
                canvas.KeyUp += PowerPointImage_KeyUp;
                ApplyModernThumbStyle(canvas, panel);
                return canvas;
            });
        }

        private void DisplayPowerPointThumbnails(PowerPointThumbnailPanel panel, int count, string prefix, int selectedSlide)
        {
            if (panel.Controls.Count != count)
                FormatPowerPointThumbContainers(panel, count);
            panel.Display(prefix, selectedSlide, toolTip1);
        }

        private void PowerPointImage_DoubleClick(object sender, EventArgs e)
        {
            try
            {
                // Set the double-click flag
                pptDoubleClickInProgress = true;

                // Handled through KeyboardActionHandler
                _keyboardHandler.HandlePowerPointThumbnailDoubleClick((Control)sender);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in PowerPointImage_DoubleClick: {ex.Message}");
            }
            finally
            {
                // Clear the double-click flag (allows the next click)
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
            for (int i = 0; i < Gf.AbsoluteMaxHitoryItems - 1; i++)
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

        /// <summary>
        /// Makes Inmode the current folder sort order and shows it on the sort button: checked for any
        /// order other than title, the tooltip names the order (openspec change add-song-number-sort, N3).
        /// An unknown value is title order.
        /// </summary>
        private void SetSortButton(SortBy Inmode)
        {
            CurStyle = Inmode == SortBy.WordCount || Inmode == SortBy.Number ? Inmode : SortBy.Alpha;
            Folders_WordCount.Checked = CurStyle != SortBy.Alpha;
            Folders_WordCount.ToolTipText = FolderSortToolTip(CurStyle);
        }

        private void SetPowerpointListingButton()
        {
            Gf.AssignDropDownItem(SelectedMenuItemName: (Gf.PowerpointListingStyle != 0) ? PP_PreviewStyle.Name : PP_ListStyle.Name, SelectedBtn: ref PP_ListType, InMenuItem1: PP_ListStyle, InMenuItem2: PP_PreviewStyle);
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
            Menu_WorshipSessions.Enabled = EnableWorshipMode && !Gf.ShowRunning;
            splitContainerPreview.Panel2Collapsed = !EnableWorshipMode;
            Menu_PraiseBookTemplates.Enabled = !EnableWorshipMode;
        }

        private void ClearLyrics(LyricsSlidePanel panel)
        {
            FormatLyricsContainers(panel == flowLayoutPreviewLyrics ? Gf.PreviewItem : Gf.OutputItem, Reset: true, SetFocus: false);
        }

        // Error notice of the status strip: created on the first error, written by nothing else,
        // hidden again by a click on it (which also restarts its count).
        private ToolStripStatusLabel errorNoticeLabel;
        private int errorNoticeCount;
        // Message part of the notice shown now (count, prefix and suffix are added when it is laid out).
        private string errorNoticeMessage = "";

        private const string ErrorNoticeSuffix = " — see EasiSlides_Debug.log";
        // While a notice is shown, the output monitor pane keeps at least this width (logical pixels),
        // and the summary and preview panes give up width down to PaneMinWidth before the notice is cut.
        private const int OutputPaneMinWidth = 120;
        private const int PaneMinWidth = 40;
        private const TextFormatFlags NoticeTextFlags = TextFormatFlags.SingleLine | TextFormatFlags.NoPrefix;

        /// <summary>
        /// True once this form has been shown, whether or not Form1_Load ran to the end. From then on
        /// an unhandled exception is reported in the error notice; before that
        /// Program.OnUiThreadException shows a message box.
        /// </summary>
        internal bool ReadyForErrorNotices { get; private set; }

        private void WatchFirstShown()
        {
            Shown += (sender, e) => ReadyForErrorNotices = true;
        }

        protected override void OnLoad(EventArgs e)
        {
            try
            {
                base.OnLoad(e);
            }
            finally
            {
                // WinForms raises Shown only when the Load handlers completed (measured: a Form1_Load
                // that throws means no Shown), and Activated is no substitute (it comes before Load).
                // The window is shown either way, so the flag is set here as well. It is posted, so it
                // runs when this thread next handles its message queue, after Load has been abandoned and
                // the exception has been reported. Nothing depends on whether the window is already
                // visible at that moment. Measured with the start-up message box Program shows
                // (DefaultDesktopOnly): it did not run while that box was open (15 s).
                if (!ReadyForErrorNotices && IsHandleCreated)
                {
                    BeginInvoke((MethodInvoker)delegate { ReadyForErrorNotices = true; });
                }
            }
        }

        /// <summary>
        /// Shows <paramref name="message"/> in the main form's error notice, from any code and any
        /// thread (a screen's paint, the show window). Does nothing while the main form has not been
        /// shown yet (the caller logs the error itself). Never throws.
        /// </summary>
        internal static void ReportErrorNotice(string message)
        {
            try
            {
                FrmMain form = frmMain;
                if (form != null && form.ReadyForErrorNotices)
                {
                    form.ShowErrorNotice(message);
                }
            }
            catch
            {
            }
        }

        /// <summary>
        /// Tells the operator in the status strip that an error occurred (an unhandled UI-thread
        /// exception, a failed paint, or a failed action on the show window; details in the debug
        /// log). Posted, so it never blocks the caller and is safe from any thread; does nothing
        /// when this form is not usable. Never throws.
        /// </summary>
        internal void ShowErrorNotice(string message)
        {
            try
            {
                if (IsDisposed || Disposing || !IsHandleCreated)
                {
                    return;
                }
                BeginInvoke((MethodInvoker)delegate { ShowErrorNoticeNow(message); });
            }
            catch
            {
            }
        }

        private void ShowErrorNoticeNow(string message)
        {
            try
            {
                if (IsDisposed || statusStripMain.IsDisposed)
                {
                    return;
                }
                if (errorNoticeLabel == null)
                {
                    errorNoticeLabel = new ToolStripStatusLabel
                    {
                        Name = "StatusBarErrorNotice",
                        AutoSize = false,
                        AutoToolTip = false,
                        ForeColor = Color.Red,
                        TextAlign = ContentAlignment.MiddleLeft,
                        Visible = false
                    };
                    errorNoticeLabel.Click += (sender, e) =>
                    {
                        // Dismissed: the next error starts counting from 1 again.
                        errorNoticeLabel.Visible = false;
                        statusStripMain.ShowItemToolTips = false;
                        errorNoticeCount = 0;
                        errorNoticeMessage = "";
                        HideStatusStripShownForNotice();
                        SetStatusbarSize();
                    };
                    // The full text of a shortened notice is in its tool tip. The strip shows item tool
                    // tips only while the pointer is on the notice. The other panes have AutoToolTip = true
                    // (designer), and ShowStatusBarSummary / ShowStatusBarOutputArea also give panes 2-4 a
                    // ToolTipText, but none was ever shown because ShowItemToolTips was off; that stays so.
                    errorNoticeLabel.MouseEnter += (sender, e) => statusStripMain.ShowItemToolTips = true;
                    errorNoticeLabel.MouseLeave += (sender, e) => statusStripMain.ShowItemToolTips = false;
                    // Appended after the four panes, which other code addresses by index.
                    statusStripMain.Items.Add(errorNoticeLabel);
                }
                if (!Menu_StatusBar.Checked && !statusStripMain.Visible)
                {
                    // The operator has hidden the status bar (View > Status Bar), so the notice would not
                    // be seen: show the bar while the notice is up. The menu check stays as the operator
                    // set it, and the bar is hidden again when the notice is dismissed.
                    statusStripShownForNotice = true;
                    statusStripMain.Visible = true;
                }
                errorNoticeCount++;
                string shortMessage = (message ?? "").Replace("\r", " ").Replace("\n", " ").Trim();
                if (shortMessage.Length > 500)
                {
                    shortMessage = shortMessage.Substring(0, 500);
                }
                // The newest message is shown: a failure that repeats (a timer, a paint) leaves the text as
                // it is and only raises the count.
                errorNoticeMessage = shortMessage;
                errorNoticeLabel.ToolTipText = ErrorNoticeText(errorNoticeMessage) + " (click to hide)";
                errorNoticeLabel.Visible = true;
                SetStatusbarSize();
            }
            catch
            {
            }
        }

        // True while the status bar is shown only because of the error notice (the operator hid it).
        private bool statusStripShownForNotice;

        /// <summary>
        /// Hides the status bar again if the notice had shown it. Setting Visible makes the form lay
        /// out its docked panels again, as View > Status Bar does.
        /// </summary>
        private void HideStatusStripShownForNotice()
        {
            if (statusStripShownForNotice)
            {
                statusStripShownForNotice = false;
                statusStripMain.Visible = Menu_StatusBar.Checked;
            }
        }

        private string ErrorNoticeText(string shownMessage)
        {
            return "Error (" + errorNoticeCount + "): " + shownMessage + ErrorNoticeSuffix;
        }

        private int NoticeTextWidth(string text)
        {
            // Label padding plus a few pixels for the text renderer's own margins.
            return TextRenderer.MeasureText(text, errorNoticeLabel.Font, Size.Empty, NoticeTextFlags).Width + errorNoticeLabel.Padding.Horizontal + 6;
        }

        /// <summary>
        /// Notice text that fits <paramref name="width"/>: the full text, or the message cut and ended
        /// with an ellipsis, or (when even the count and suffix do not fit) the whole text cut.
        /// </summary>
        private string FitErrorNoticeText(int width)
        {
            string full = ErrorNoticeText(errorNoticeMessage);
            if (NoticeTextWidth(full) <= width)
            {
                return full;
            }
            const string Ellipsis = "…";
            if (NoticeTextWidth(ErrorNoticeText(Ellipsis)) <= width)
            {
                return ErrorNoticeText(errorNoticeMessage.Substring(0, LongestFittingLength(errorNoticeMessage.Length, n => ErrorNoticeText(errorNoticeMessage.Substring(0, n).TrimEnd() + Ellipsis), width)).TrimEnd() + Ellipsis);
            }
            return full.Substring(0, LongestFittingLength(full.Length, n => full.Substring(0, n) + Ellipsis, width)) + Ellipsis;
        }

        private int LongestFittingLength(int maxLength, Func<int, string> textOfLength, int width)
        {
            // Binary search for the longest n whose text fits; 0 when none does.
            int low = 0;
            int high = maxLength;
            while (low < high)
            {
                int mid = (low + high + 1) / 2;
                if (NoticeTextWidth(textOfLength(mid)) <= width)
                {
                    low = mid;
                }
                else
                {
                    high = mid - 1;
                }
            }
            return low;
        }

        internal void ShowStatusBarSummary()
        {
            string paneText = "";
            if (!IsSelectedTab(tabControlSource, "tabBibles"))
            {
                paneText = (IsSelectedTab(tabControlSource, "tabFiles") ? ("InfoScreen: " + InfoScreenList.Items.Count + " Listed. / ") : ((!IsSelectedTab(tabControlSource, "tabMedia")) ? ("Folder: " + SongFolderCountText()) : ("Media: " + MediaList.Items.Count + " Listed. / ")));
            }
            else if (BibleText.Text != "")
            {
                paneText = "Verses listed: " + Gf.HB_VersesLocation[0].Version + " / ";
            }
            else if (Gf.HB_TotalVersions > 0)
            {
                paneText = "Verses listed: 0 / ";
            }
            if (Gf.EasiSlidesMode == UsageMode.Worship)
            {
                paneText = paneText + "Worship: " + WorshipListItems.Items.Count + ((WorshipListItems.Items.Count < 1) ? " item." : " items. ");
            }
            else if (Gf.EasiSlidesMode == UsageMode.PraiseBook)
            {
                paneText = paneText + "PraiseBook: " + PraiseBookItems.Items.Count + ((PraiseBookItems.Items.Count < 1) ? " item." : " items. ");
            }
            statusStripMain.Items[0].Text = paneText;
            paneText = "";
            if (Gf.PreviewItem.Source == ItemSource.SongsList)
            {
                if (Gf.PreviewItem.Type == "D" && Gf.PreviewItem.FolderNo > 0)
                {
                    paneText = paneText + " " + Gf.Folders[Gf.PreviewItem.FolderNo].Name;
                }
            }
            else if (Gf.PreviewItem.Source == ItemSource.ExternalFileInfoScreen)
            {
                paneText += InfoScreenFolder.Text;
            }
            else if (Gf.PreviewItem.Source == ItemSource.ExternalFileMedia)
            {
                paneText += MediaFolder.Text;
            }
            else if (Gf.PreviewItem.Source == ItemSource.ExternalFilePowerpoint)
            {
                paneText += PowerpointFolder.Text;
            }
            else if (Gf.PreviewItem.Source == ItemSource.HolyBible)
            {
                paneText += "Holy Bible";
            }
            else if (Gf.PreviewItem.Source == ItemSource.WorshipList)
            {
                paneText += "Worship";
                if (Gf.PreviewItem.Type == "D" && Gf.PreviewItem.FolderNo > 0)
                {
                    paneText = paneText + " (" + Gf.Folders[Gf.PreviewItem.FolderNo].Name + ")";
                }
            }
            else if (Gf.PreviewItem.Source == ItemSource.PraiseBook)
            {
                paneText += "PraiseBook";
                if (Gf.PreviewItem.Type == "D" && Gf.PreviewItem.FolderNo > 0)
                {
                    paneText = paneText + " (" + Gf.Folders[Gf.PreviewItem.FolderNo].Name + ")";
                }
            }
            else
            {
                paneText = (paneText ?? "");
            }
            statusStripMain.Items[1].Text = paneText + ((Gf.PreviewItem.User_Reference != "") ? (" [" + Gf.PreviewItem.User_Reference + "]") : "") + ((Gf.PreviewItem.Book_Reference != "") ? (" [" + Gf.PreviewItem.Book_Reference + "]") : "") + " " + KeyCapoText;
            statusStripMain.Items[1].ToolTipText = statusStripMain.Items[1].Text;
            StatusBarOutputPaneMess = ShowStatusBarOutputArea();
            SetStatusBarMediaTimings();
            SetStatusbarSize();
        }

        private string ShowStatusBarOutputArea()
        {
            string outputText = "";
            if (Gf.OutputItem.CurItemNo < 1)
            {
                if (Gf.OutputItem.ItemID != "")
                {
                    string previousText = outputText;
                    outputText = $"{previousText}{Gf.OutputItem.CurSlide}/{Gf.OutputItem.TotalSlides} [Ad-hoc]";
                }
                else
                {
                    outputText = (outputText ?? "");
                }
            }
            else
            {
                // Worship list item: its position in the list, its title and the slide (daniel, 2022-04-01).
                string previousText = outputText;
                outputText = $"[{Convert.ToString(Gf.OutputItem.CurItemNo)}/{WorshipListItems.Items.Count}] {Gf.OutputItem.Title} {previousText}{Gf.OutputItem.CurSlide}/{Gf.OutputItem.TotalSlides} ";
            }
            outputText += ((Gf.GapItemOption > GapType.None) ? (((outputText == "") ? "" : " ") + "(" + Gf.GapItemOption.ToString() + " Gap)") : "");
            statusStripMain.Items[2].Image = (Gf.DualMonitorMode ? DualMonIcon : SingleMonIcon);
            statusStripMain.Items[2].Text = outputText;
            statusStripMain.Items[2].ToolTipText = Gf.OutputMonitorText;
            if (Gf.KeyBoardOption == 1)
            {
                statusStripMain.Items[3].Image = Keyboard1Icon;
                statusStripMain.Items[3].ToolTipText = "Use Alternate Keyboard keys";
            }
            else
            {
                statusStripMain.Items[3].Image = null;
                statusStripMain.Items[3].ToolTipText = "";
            }
            return outputText;
        }

        private void SetStatusBarMediaTimings()
        {
            if (Gf.ShowRunning)
            {
                string mediaTimings = RemoteControlLiveShow(LiveShowAction.Remote_GetMediaTimings);
                if (mediaTimings != "")
                {
                    statusStripMain.Items[2].Text = StatusBarOutputPaneMess + " - " + mediaTimings;
                }
                else
                {
                    statusStripMain.Items[2].Text = StatusBarOutputPaneMess;
                }
            }
        }

        private void SetStatusbarSize()
        {
            // Room the strip lays its items out in: the client width less the strip's padding (its
            // right padding holds the sizing grip) and the items' margins, less 1 px: the strip's table
            // layout does not place an item that ends exactly at the edge (measured). The old fixed
            // "- 16" was 3 px short of the designer padding (1 + 18), so the output monitor pane and the
            // keyboard pane after it were not placed at all.
            int room = statusStripMain.DisplayRectangle.Width - 1;
            foreach (ToolStripItem item in statusStripMain.Items)
            {
                if (item.Available)
                {
                    room -= item.Margin.Horizontal;
                }
            }
            int summaryWidth = splitContainerMain.Panel1.Width + 1;
            int previewWidth = splitContainer2.Panel1.Width + 2;
            int keyboardWidth = (Gf.KeyBoardOption == 1) ? 40 : 0;
            int noticeWidth = 0;
            // Available, not Visible: Visible reads false while the strip has no room to place the label.
            if (errorNoticeLabel != null && errorNoticeLabel.Available)
            {
                // The notice takes at most half the strip and never more than is left once the output
                // pane has its minimum; the summary and preview panes shrink to make room for it first.
                int wanted = Math.Max(0, Math.Min(NoticeTextWidth(ErrorNoticeText(errorNoticeMessage)), room / 2));
                int paneMin = LogicalToDeviceUnits(PaneMinWidth);
                int shortage = wanted - (room - summaryWidth - previewWidth - keyboardWidth - LogicalToDeviceUnits(OutputPaneMinWidth));
                if (shortage > 0)
                {
                    int cut = Math.Min(shortage, Math.Max(previewWidth - paneMin, 0));
                    previewWidth -= cut;
                    shortage -= cut;
                }
                if (shortage > 0)
                {
                    int cut = Math.Min(shortage, Math.Max(summaryWidth - paneMin, 0));
                    summaryWidth -= cut;
                    shortage -= cut;
                }
                noticeWidth = Math.Max(0, wanted - Math.Max(shortage, 0));
                errorNoticeLabel.Width = noticeWidth;
                string noticeText = FitErrorNoticeText(noticeWidth);
                if (errorNoticeLabel.Text != noticeText)
                {
                    errorNoticeLabel.Text = noticeText;
                }
            }
            statusStripMain.Items[0].Width = summaryWidth;
            statusStripMain.Items[1].Width = previewWidth;
            statusStripMain.Items[3].Width = keyboardWidth;
            int outputWidth = room - (summaryWidth + previewWidth + keyboardWidth + noticeWidth);
            statusStripMain.Items[2].Width = ((outputWidth > 0) ? outputWidth : 0);
        }

        private int GetImagesPanelWidth()
        {
            return (SongsList.Width - 25 - 5 * (Gf.ThumbImagesPerRow - 1)) / Gf.ThumbImagesPerRow;
        }

        private void FormatBackgroundThumbContainers()
        {
            if (ImagesFolder.Text == "")
            {
                return;
            }
            Gf.ThumbImagesPerRow = 3;
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

        private void LoadThumbImages(FlowLayoutPanel InFlowPanel, ref ImageCanvas[] InCanvas, string[] ImageName, int TotalImagesCount, int PanelWidth, string InPrefix, int CurSelectedSlide, ToolTip InToolTip, bool ExternalPP)
        {
            if (InCanvas != null)
            {
                Color backColor = InFlowPanel.BackColor;

                int thumbWidth = (PanelWidth - 35) / 3;
                int newHeight = thumbWidth * 3 / 4;

                for (int i = 0; i < TotalImagesCount; i++)
                {
                    InCanvas[i].BuildNewImageThumbs(i, thumbWidth, newHeight, ref ImageName, TotalImagesCount, InPrefix, CurSelectedSlide, backColor, InToolTip, ExternalPP);
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
            Gf.ThumbImagesPerRow = 3;
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
            OutputBtnVerse1.Visible = ((Gf.OutputItem.SongVerses[1] > 0) ? true : false);
            OutputBtnVerse2.Visible = ((Gf.OutputItem.SongVerses[2] > 0) ? true : false);
            OutputBtnVerse3.Visible = ((Gf.OutputItem.SongVerses[3] > 0) ? true : false);
            OutputBtnVerse4.Visible = ((Gf.OutputItem.SongVerses[4] > 0) ? true : false);
            OutputBtnVerse5.Visible = ((Gf.OutputItem.SongVerses[5] > 0) ? true : false);
            OutputBtnVerse6.Visible = ((Gf.OutputItem.SongVerses[6] > 0) ? true : false);
            OutputBtnVerse7.Visible = ((Gf.OutputItem.SongVerses[7] > 0) ? true : false);
            OutputBtnVerse8.Visible = ((Gf.OutputItem.SongVerses[8] > 0) ? true : false);
            OutputBtnVerse9.Visible = ((Gf.OutputItem.SongVerses[9] > 0) ? true : false);
            bool hasChorus = false;
            bool hasBridge = false;
            bool hasBridge2 = false;
            bool hasChorus2 = false;
            bool hasEnding = false;
            bool hasPreChorus = false;
            bool hasPreChorus2 = false;
            for (int i = 1; i <= Gf.OutputItem.TotalSlides; i++)
            {
                if (Gf.OutputItem.Slide[i, 0] == 111)
                {
                    hasPreChorus = true;
                }
                if (Gf.OutputItem.Slide[i, 0] == 112)
                {
                    hasPreChorus2 = true;
                }
                if (Gf.OutputItem.Slide[i, 0] == 0)
                {
                    hasChorus = true;
                }
                if (Gf.OutputItem.Slide[i, 0] == 102)
                {
                    hasChorus2 = true;
                }
                if (Gf.OutputItem.Slide[i, 0] == 100)
                {
                    hasBridge = true;
                }
                if (Gf.OutputItem.Slide[i, 0] == 103)
                {
                    hasBridge2 = true;
                }
                if (Gf.OutputItem.Slide[i, 0] == 101)
                {
                    hasEnding = true;
                }
            }
            if (hasPreChorus)
            {
                OutputBtnVersePreChorus.Visible = true;
            }
            if (hasPreChorus2)
            {
                OutputBtnVersePreChorus2.Visible = true;
            }
            if (hasChorus)
            {
                OutputBtnVerseChorus.Visible = true;
            }
            if (hasChorus2)
            {
                OutputBtnVerseChorus2.Visible = true;
            }
            if (hasBridge)
            {
                OutputBtnVerseBridge.Visible = true;
            }
            if (hasBridge2)
            {
                OutputBtnVerseBridge2.Visible = true;
            }
            if (hasEnding)
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
            PreviewBtnVerse1.Visible = ((Gf.PreviewItem.SongVerses[1] > 0) ? true : false);
            PreviewBtnVerse2.Visible = ((Gf.PreviewItem.SongVerses[2] > 0) ? true : false);
            PreviewBtnVerse3.Visible = ((Gf.PreviewItem.SongVerses[3] > 0) ? true : false);
            PreviewBtnVerse4.Visible = ((Gf.PreviewItem.SongVerses[4] > 0) ? true : false);
            PreviewBtnVerse5.Visible = ((Gf.PreviewItem.SongVerses[5] > 0) ? true : false);
            PreviewBtnVerse6.Visible = ((Gf.PreviewItem.SongVerses[6] > 0) ? true : false);
            PreviewBtnVerse7.Visible = ((Gf.PreviewItem.SongVerses[7] > 0) ? true : false);
            PreviewBtnVerse8.Visible = ((Gf.PreviewItem.SongVerses[8] > 0) ? true : false);
            PreviewBtnVerse9.Visible = ((Gf.PreviewItem.SongVerses[9] > 0) ? true : false);
            bool hasChorus = false;
            bool hasBridge = false;
            bool hasBridge2 = false;
            bool hasChorus2 = false;
            bool hasEnding = false;
            bool hasPreChorus = false;
            bool hasPreChorus2 = false;
            for (int i = 1; i <= Gf.PreviewItem.TotalSlides; i++)
            {
                if (Gf.PreviewItem.Slide[i, 0] == 111)
                {
                    hasPreChorus = true;
                }
                if (Gf.PreviewItem.Slide[i, 0] == 112)
                {
                    hasPreChorus2 = true;
                }
                if (Gf.PreviewItem.Slide[i, 0] == 0)
                {
                    hasChorus = true;
                }
                if (Gf.PreviewItem.Slide[i, 0] == 102)
                {
                    hasChorus2 = true;
                }
                if (Gf.PreviewItem.Slide[i, 0] == 100)
                {
                    hasBridge = true;
                }
                if (Gf.PreviewItem.Slide[i, 0] == 103)
                {
                    hasBridge2 = true;
                }
                if (Gf.PreviewItem.Slide[i, 0] == 101)
                {
                    hasEnding = true;
                }
            }
            if (hasPreChorus)
            {
                PreviewBtnVersePreChorus.Visible = true;
            }
            if (hasPreChorus2)
            {
                PreviewBtnVersePreChorus2.Visible = true;
            }
            if (hasChorus)
            {
                PreviewBtnVerseChorus.Visible = true;
            }
            if (hasChorus2)
            {
                PreviewBtnVerseChorus2.Visible = true;
            }
            if (hasBridge)
            {
                PreviewBtnVerseBridge.Visible = true;
            }
            if (hasBridge2)
            {
                PreviewBtnVerseBridge2.Visible = true;
            }
            if (hasEnding)
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
            Gf.PreviewItem.Format.FormatString = ((!Ind_checkBox.Checked) ? "" : GetNewFormatString());
            if (Gf.PreviewItem.Type == "I")
            {
                SaveInfoFilePreview(ReloadImageData: false);
            }
            if (Gf.PreviewItem.Source == ItemSource.WorshipList)
            {
                int selectedIndex = Gf.GetSelectedIndex(WorshipListItems);
                if (selectedIndex < 0)
                {
                    return;
                }
                WorshipListItems.Items[selectedIndex].SubItems[2].Text = Gf.PreviewItem.Format.FormatString;
                Gf.SaveFormatStringToDatabase(Gf.PreviewItem.ItemID, Gf.PreviewItem.Format.FormatString);
                SaveWorshipList();
            }
            else if (Gf.PreviewItem.Source == ItemSource.SongsList)
            {
                Gf.SaveFormatStringToDatabase(Gf.PreviewItem.ItemID, Gf.PreviewItem.Format.FormatString);
            }
            else if (Gf.PreviewItem.Source == ItemSource.HolyBible)
            {
                HB_CurSelectedFormat = Gf.PreviewItem.Format.FormatString;
            }
            ShowSong(ref Gf.PreviewItem, (!StartAtFirstSlide) ? Gf.PreviewItem.CurSlide : 0, TransitionAction);
            FormatLyricsContainers(Gf.PreviewItem);
        }

        private void AppyPreviewChangesToLive(int Selecteditem)
        {
            AppyPreviewChangesToLive(Selecteditem, StartAtFirstSlide: true);
        }

        private void AppyPreviewChangesToLive(int Selecteditem, bool StartAtFirstSlide)
        {
            if (Gf.OutputItem.CurItemNo != Selecteditem + 1)
            {
                return;
            }
            Gf.LoadIndividualFormatData(ref Gf.OutputItem, Gf.PreviewItem.Format.FormatString);
            Gf.SetShowBackground(Gf.OutputItem, ref OutputScreen);
            if (StartAtFirstSlide)
            {
                if (Gf.ShowRunning)
                {
                    Gf.MainAction_SongChanged_Transaction = ImageTransitionControl.TransitionAction.None;
                    RemoteControlLiveShow(LiveShowAction.Remote_SongChanged);
                }
            }
            else
            {
                Gf.MainAction_SongChanged_Transaction = ImageTransitionControl.TransitionAction.None;
                if (Gf.ShowRunning)
                {
                    RemoteControlLiveShow(LiveShowAction.Remote_BackgroundChanged);
                }
            }
            if (Gf.OutputItem.ItemID != "")
            {
                RefreshSlidesFonts(ref Gf.OutputItem);
                if (StartAtFirstSlide)
                {
                    ShowSong(ref Gf.OutputItem);
                    FormatLyricsContainers(Gf.OutputItem);
                }
            }
        }

        private string GetNewFormatString()
        {
            int region1FontStyle = Gf.PreviewItem.Format.ShowFontBold[0] + Gf.PreviewItem.Format.ShowFontItalic[0] * 2 + Gf.PreviewItem.Format.ShowFontUnderline[0] * 4 + Gf.PreviewItem.Format.ShowFontBold[2] * 8 + Gf.PreviewItem.Format.ShowFontItalic[2] * 16 + Gf.PreviewItem.Format.ShowFontUnderline[2] * 32;
            int region2FontStyle = Gf.PreviewItem.Format.ShowFontBold[1] + Gf.PreviewItem.Format.ShowFontItalic[1] * 2 + Gf.PreviewItem.Format.ShowFontUnderline[1] * 4 + Gf.PreviewItem.Format.ShowFontBold[3] * 8 + Gf.PreviewItem.Format.ShowFontItalic[3] * 16 + Gf.PreviewItem.Format.ShowFontUnderline[3] * 32;
            int mediaFlags = Gf.PreviewItem.Format.MediaMute + Gf.PreviewItem.Format.MediaRepeat * 2 + Gf.PreviewItem.Format.MediaWidescreen * 4;
            int displayFlags = Gf.PreviewItem.Format.UseShadowFont * 2 + Gf.PreviewItem.Format.ShowNotations * 4 + Gf.PreviewItem.Format.ShowInterlace * 16 + Gf.PreviewItem.Format.UseOutlineFont * 32 + Gf.PreviewItem.Format.HideDisplayPanel * 64;
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(Convert.ToString(21) + "=" + Convert.ToString(Gf.PreviewItem.Format.ShowSongHeadings) + '>');
            stringBuilder.Append(Convert.ToString(23) + "=" + Convert.ToString(Gf.PreviewItem.Format.ShowSongHeadingsAlign) + '>');
            stringBuilder.Append(Convert.ToString(22) + "=" + displayFlags.ToString() + '>');
            stringBuilder.Append(Convert.ToString(25) + "=" + Convert.ToString(Gf.PreviewItem.Format.ShowLyrics) + '>');
            // SongFormat colours are already ARGB ints and are written as they are (the same values .ToArgb() gave before).
            stringBuilder.Append(Convert.ToString(26) + "=" + Convert.ToString(Gf.PreviewItem.Format.ShowScreenColour[0]) + '>');
            stringBuilder.Append(Convert.ToString(27) + "=" + Convert.ToString(Gf.PreviewItem.Format.ShowScreenColour[1]) + '>');
            stringBuilder.Append(Convert.ToString(28) + "=" + Gf.PreviewItem.Format.ShowScreenStyle.ToString() + '>');
            stringBuilder.Append(Convert.ToString(29) + "=" + Convert.ToString(Gf.PreviewItem.Format.ShowFontColour[0]) + '>');
            stringBuilder.Append(Convert.ToString(30) + "=" + Convert.ToString(Gf.PreviewItem.Format.ShowFontColour[1]) + '>');
            stringBuilder.Append(Convert.ToString(31) + "=" + Convert.ToString(Gf.PreviewItem.Format.ShowFontAlign[0]) + '>');
            stringBuilder.Append(Convert.ToString(32) + "=" + Convert.ToString(Gf.PreviewItem.Format.ShowFontAlign[1]) + '>');
            stringBuilder.Append(Convert.ToString(41) + "=" + region1FontStyle.ToString() + '>');
            stringBuilder.Append(Convert.ToString(42) + "=" + region2FontStyle.ToString() + '>');
            stringBuilder.Append(Convert.ToString(43) + "=" + Gf.PreviewItem.Format.ShowFontName[0] + '>');
            stringBuilder.Append(Convert.ToString(44) + "=" + Gf.PreviewItem.Format.ShowFontName[1] + '>');
            stringBuilder.Append(Convert.ToString(45) + "=" + Convert.ToString(Gf.PreviewItem.Format.ShowFontVPosition[0]) + '>');
            stringBuilder.Append(Convert.ToString(46) + "=" + Convert.ToString(Gf.PreviewItem.Format.ShowFontVPosition[1]) + '>');
            stringBuilder.Append(Convert.ToString(47) + "=" + Convert.ToString(Gf.PreviewItem.Format.ShowFontSize[0]) + '>');
            stringBuilder.Append(Convert.ToString(48) + "=" + Convert.ToString(Gf.PreviewItem.Format.ShowFontSize[1]) + '>');
            stringBuilder.Append(Convert.ToString(50) + "=" + Convert.ToString(Gf.PreviewItem.Format.MediaOption) + '>');
            stringBuilder.Append(Convert.ToString(51) + "=" + Gf.PreviewItem.Format.MediaLocation + '>');
            stringBuilder.Append(Convert.ToString(52) + "=" + Convert.ToString(Gf.PreviewItem.Format.MediaVolume) + '>');
            stringBuilder.Append(Convert.ToString(53) + "=" + Convert.ToString(Gf.PreviewItem.Format.MediaBalance) + '>');
            stringBuilder.Append(Convert.ToString(54) + "=" + mediaFlags.ToString() + '>');
            stringBuilder.Append(Convert.ToString(55) + "=" + Convert.ToString(Gf.PreviewItem.Format.MediaCaptureDeviceNumber) + '>');
            stringBuilder.Append(Convert.ToString(56) + "=" + Gf.PreviewItem.Format.MediaOutputMonitorName + '>');
            stringBuilder.Append(Convert.ToString(61) + "=" + Gf.PreviewItem.Format.BackgroundPicture + '>');
            stringBuilder.Append(Convert.ToString(62) + "=" + Convert.ToString((int)Gf.PreviewItem.Format.BackgroundMode) + '>');
            stringBuilder.Append(Convert.ToString(63) + "=" + Convert.ToString(Gf.PreviewItem.Format.ShowVerticalAlign) + '>');
            stringBuilder.Append(Convert.ToString(64) + "=" + Convert.ToString(Gf.PreviewItem.Format.ShowLeftMargin) + '>');
            stringBuilder.Append(Convert.ToString(65) + "=" + Convert.ToString(Gf.PreviewItem.Format.ShowRightMargin) + '>');
            stringBuilder.Append(Convert.ToString(66) + "=" + Convert.ToString(Gf.PreviewItem.Format.ShowBottomMargin) + '>');
            stringBuilder.Append(Convert.ToString(71) + "=" + Convert.ToString(Gf.PreviewItem.Format.TransposeOffset) + '>');
            stringBuilder.Append(Convert.ToString(72) + "=" + PreviewScreen.GetTransitionText(Gf.PreviewItem.Format.ShowItemTransition) + '>');
            stringBuilder.Append(Convert.ToString(73) + "=" + PreviewScreen.GetTransitionText(Gf.PreviewItem.Format.ShowSlideTransition) + '>');
            return stringBuilder.ToString();
        }

        private void UpdateFormatFields()
        {
            WriteDebugLog("UpdateFormatFields() called - Setting UpdatingFormatFields = true");
            UpdatingFormatFields = true;
            try
            {
                string alignItemName = "";
                Gf.AssignDropDownItem(SelectedMenuItemName: (Gf.PreviewItem.Format.ShowSongHeadings == 0) ? Ind_HeadNoTitles.Name : ((Gf.PreviewItem.Format.ShowSongHeadings != 1) ? Ind_HeadFirstScreen.Name : Ind_HeadAllTitles.Name), SelectedBtn: ref Ind_Head, InMenuItem1: Ind_HeadNoTitles, InMenuItem2: Ind_HeadAllTitles, InMenuItem3: Ind_HeadFirstScreen);
                Gf.AssignDropDownItem(SelectedMenuItemName: (Gf.PreviewItem.Format.ShowSongHeadingsAlign == 1) ? Ind_HeadAlignAsR2.Name : ((Gf.PreviewItem.Format.ShowSongHeadingsAlign == 2) ? Ind_HeadAlignLeft.Name : ((Gf.PreviewItem.Format.ShowSongHeadingsAlign == 3) ? Ind_HeadAlignCentre.Name : ((Gf.PreviewItem.Format.ShowSongHeadingsAlign != 4) ? Ind_HeadAlignAsR1.Name : Ind_HeadAlignRight.Name))), SelectedBtn: ref Ind_HeadAlign, InMenuItem1: Ind_HeadAlignAsR1, InMenuItem2: Ind_HeadAlignAsR2, InMenuItem3: Ind_HeadAlignLeft, InMenuItem4: Ind_HeadAlignCentre, InMenuItem5: Ind_HeadAlignRight);
                Ind_Shadow.Checked = ((Gf.PreviewItem.Format.UseShadowFont == 1) ? true : false);
                Ind_Outline.Checked = ((Gf.PreviewItem.Format.UseOutlineFont == 1) ? true : false);
                Gf.AssignDropDownItem(SelectedMenuItemName: (Gf.PreviewItem.Format.ShowLyrics == 0) ? Ind_ShowRegion1.Name : ((Gf.PreviewItem.Format.ShowLyrics != 1) ? Ind_ShowRegionBoth.Name : Ind_ShowRegion2.Name), SelectedBtn: ref Ind_Region, InMenuItem1: Ind_ShowRegion1, InMenuItem2: Ind_ShowRegion2, InMenuItem3: Ind_ShowRegionBoth);
                Gf.AssignDropDownItem(SelectedMenuItemName: (Gf.PreviewItem.Format.ShowVerticalAlign == 0) ? Ind_VAlignTop.Name : ((Gf.PreviewItem.Format.ShowVerticalAlign != 1) ? Ind_VAlignBottom.Name : Ind_VAlignCentre.Name), SelectedBtn: ref Ind_VAlign, InMenuItem1: Ind_VAlignTop, InMenuItem2: Ind_VAlignCentre, InMenuItem3: Ind_VAlignBottom);
                Ind_Interlace.Checked = ((Gf.PreviewItem.Format.ShowInterlace == 1) ? true : false);
                Ind_Notations.Checked = ((Gf.PreviewItem.Format.ShowNotations == 1) ? true : false);
                Ind_HideDisplayPanel.Checked = ((Gf.PreviewItem.Format.HideDisplayPanel == 1) ? true : false);
                Ind_LeftUpDown.Value = Gf.PreviewItem.Format.ShowLeftMargin;
                Ind_RightUpDown.Value = Gf.PreviewItem.Format.ShowRightMargin;
                Gf.UpdatePosUpDowns(ref Ind_Reg1TopUpDown, ref Ind_Reg2TopUpDown, ref Ind_BottomUpDown, ref Gf.PreviewItem.Format.ShowFontVPosition[0], ref Gf.PreviewItem.Format.ShowFontVPosition[1], Gf.PreviewItem.Format.ShowBottomMargin);
                Gf.AssignDropDownItem(SelectedMenuItemName: (Gf.PreviewItem.Format.BackgroundMode == ImageMode.Tile) ? Ind_ImageTile.Name : ((Gf.PreviewItem.Format.BackgroundMode != ImageMode.Centre) ? Ind_ImageBestFit.Name : Ind_ImageCentre.Name), SelectedBtn: ref Ind_ImageMode, InMenuItem1: Ind_ImageTile, InMenuItem2: Ind_ImageCentre, InMenuItem3: Ind_ImageBestFit);
                Ind_NoImage.Enabled = ((Gf.PreviewItem.Format.BackgroundPicture != "") ? true : false);
                Ind_BackColour.ForeColor = Color.FromArgb(Gf.PreviewItem.Format.ShowScreenColour[0]);
                Ind_R1Bold.Checked = ((Gf.PreviewItem.Format.ShowFontBold[0] > 0) ? true : false);
                Gf.AssignDropDownItem(SelectedMenuItemName: (Gf.PreviewItem.Format.ShowFontItalic[0] > 0 && Gf.PreviewItem.Format.ShowFontItalic[2] > 0) ? Ind_R1Italics1.Name : ((Gf.PreviewItem.Format.ShowFontItalic[2] <= 0) ? Ind_R1Italics0.Name : Ind_R1Italics2.Name), SelectedBtn: ref Ind_R1Italics, InMenuItem1: Ind_R1Italics0, InMenuItem2: Ind_R1Italics1, InMenuItem3: Ind_R1Italics2);
                Ind_R1Underline.Checked = ((Gf.PreviewItem.Format.ShowFontUnderline[0] > 0) ? true : false);
                alignItemName = ((Gf.PreviewItem.Format.ShowFontAlign[0] == 1) ? Ind_R1AlignLeft.Name : ((Gf.PreviewItem.Format.ShowFontAlign[0] != 2) ? Ind_R1AlignRight.Name : Ind_R1AlignCentre.Name));
                Ind_R1Colour.ForeColor = Color.FromArgb(Gf.PreviewItem.Format.ShowFontColour[0]);
                Gf.AssignDropDownItem(ref Ind_R1Align, alignItemName, Ind_R1AlignLeft, Ind_R1AlignCentre, Ind_R1AlignRight);
                Ind_Reg1FontsList.Text = Gf.PreviewItem.Format.ShowFontName[0];
                Ind_Reg1SizeUpDown.Value = Gf.PreviewItem.Format.ShowFontSize[0];
                Ind_R2Bold.Checked = ((Gf.PreviewItem.Format.ShowFontBold[1] > 0) ? true : false);
                Gf.AssignDropDownItem(SelectedMenuItemName: (Gf.PreviewItem.Format.ShowFontItalic[1] > 0 && Gf.PreviewItem.Format.ShowFontItalic[3] > 0) ? Ind_R2Italics1.Name : ((Gf.PreviewItem.Format.ShowFontItalic[3] <= 0) ? Ind_R2Italics0.Name : Ind_R2Italics2.Name), SelectedBtn: ref Ind_R2Italics, InMenuItem1: Ind_R2Italics0, InMenuItem2: Ind_R2Italics1, InMenuItem3: Ind_R2Italics2);
                Ind_R2Underline.Checked = ((Gf.PreviewItem.Format.ShowFontUnderline[1] > 0) ? true : false);
                alignItemName = ((Gf.PreviewItem.Format.ShowFontAlign[1] == 1) ? Ind_R2AlignLeft.Name : ((Gf.PreviewItem.Format.ShowFontAlign[1] != 2) ? Ind_R2AlignRight.Name : Ind_R2AlignCentre.Name));
                Ind_R2Colour.ForeColor = Color.FromArgb(Gf.PreviewItem.Format.ShowFontColour[1]);
                Gf.AssignDropDownItem(ref Ind_R2Align, alignItemName, Ind_R2AlignLeft, Ind_R2AlignCentre, Ind_R2AlignRight);
                Ind_Reg2FontsList.Text = Gf.PreviewItem.Format.ShowFontName[1];
                Ind_Reg2SizeUpDown.Value = Gf.PreviewItem.Format.ShowFontSize[1];
                Ind_TransItem.SelectedIndex = Gf.PreviewItem.Format.ShowItemTransition;
                Ind_TransSlides.SelectedIndex = Gf.PreviewItem.Format.ShowSlideTransition;
                AssignMediaText(ref Ind_AssignMedia, Gf.PreviewItem.Format.MediaOption);
            }
            catch (Exception ex)
            {
                WriteDebugLog($"ERROR in UpdateFormatFields(): {ex.Message}");
                throw;
            }
            finally
            {
                WriteDebugLog("UpdateFormatFields() completed - Setting UpdatingFormatFields = false");
                UpdatingFormatFields = false;
            }
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
            string imageModeItemName = "";
            if (InMode == 1 || InMode == 2)
            {
                imageModeItemName = "";
                if (!Ind_checkBox.Checked)
                {
                    if (BackgroundImageMode.ToString() == Ind_ImageTile.Tag.ToString())
                    {
                        imageModeItemName = "Ind_ImageTile";
                    }
                    else if (BackgroundImageMode.ToString() == Ind_ImageCentre.Tag.ToString())
                    {
                        imageModeItemName = "Ind_ImageCentre";
                    }
                    else if (BackgroundImageMode.ToString() == Ind_ImageBestFit.Tag.ToString())
                    {
                        imageModeItemName = "Ind_ImageBestFit";
                    }
                    if (imageModeItemName != "")
                    {
                        Gf.AssignDropDownItem(ref Ind_ImageMode, imageModeItemName, Ind_ImageTile, Ind_ImageCentre, Ind_ImageBestFit);
                    }
                }
            }
            if (InMode == 0 || InMode == 2)
            {
                imageModeItemName = "";
                if (BackgroundImageMode.ToString() == Def_ImageTile.Tag.ToString())
                {
                    imageModeItemName = "Def_ImageTile";
                }
                else if (BackgroundImageMode.ToString() == Def_ImageCentre.Tag.ToString())
                {
                    imageModeItemName = "Def_ImageCentre";
                }
                else if (BackgroundImageMode.ToString() == Def_ImageBestFit.Tag.ToString())
                {
                    imageModeItemName = "Def_ImageBestFit";
                }
                if (imageModeItemName != "")
                {
                    Gf.AssignDropDownItem(ref Def_ImageMode, imageModeItemName, Def_ImageTile, Def_ImageCentre, Def_ImageBestFit);
                }
            }
            UpdateDefaultNoImageButton();
        }
    }
}
