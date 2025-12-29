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
//using Microsoft.Office.Interop.Access.Dao;
using Easislides.Util;
//using Microsoft.Office.Interop.Access.Dao;
using System.Threading;
//using System.Data.SQLite;
using Easislides.SQLite;
using Easislides.Module;
using MethodInvoker = System.Windows.Forms.MethodInvoker;
using Google.Protobuf.WellKnownTypes;
using Type = System.Type;
using System.Diagnostics;
using static System.Runtime.CompilerServices.RuntimeHelpers;
using DirectShowLib.BDA;
//using static Easislides.Util.HookController;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using HookManager = Easislides.HookManager;
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
        // Win32 API for window management
        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        private const uint SWP_NOMOVE = 0x0002;
        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_SHOWWINDOW = 0x0040;

        private enum LiveShowAction
        {
            Remote_SongChanged,
            Remote_SlideChanged,
            Remote_SongJumpTo,
            Remote_MessageAlertRequested,
            Remote_ParentalAlertRequested,
            Remote_ReferenceAlertShow,
            Remote_ReferenceAlertHide,
            Remote_LyricsAlertShow,
            Remote_FormatChanged,
            Remote_PanelChanged,
            Remote_DefaultBackgroundChanged,
            Remote_BackgroundChanged,
            Remote_MoveToItemChanged,
            Remote_LiveBlackClearChanged,
            Remote_ChineseChanged,
            Remote_WorshipListChanged,
            //Remote_LiveCamStartStop,
            //Remote_LiveCamUpdate,
            Remote_RefreshMediaWindow,
            Remote_RotateOnOffChanged,
            Remote_GetMediaTimings,
            Remote_MediaItemPausePlay
        }

        private enum DragDropSource
        {
            WorshipList,
            SongsList,
            InfoScreenList,
            PowerpointList,
            MediaList,
            BiblePassage
        }

        private const int MinListVisibleWidth = 60;

        private const int MaxImageContainers = 1024;

        private const int MaxLyricsContainers = 30000;

        private const int MaxTIHolders = 1024;

        private const int MaxImagesLookup = 1024;

        private bool ForceFormClose = false;

        private PopupWindowHelper popupHelper = null;

        private string PopupBtnPressed = "";

        private string Splitter_FolderWidth = "FolderWidthRatio";

        private string Splitter_FolderHeight = "FolderHeightRatio";

        private string Splitter_OutputWidth = "OutputWidthRatio";

        private string Splitter_PreviewLyricsHeight = "PreviewLyricsHeightRatio";

        private string Splitter_outputLyricsHeight = "outputLyricsHeightRatio";

        private string DragListView = "";

        private string PowerpointCurPreview = "";

        private Color Pic_BorderColour = gf.BlackScreenColour;

        private Color Pic_HighlightColour = Color.Red;

        private Color DefaultButtonForeColour = Color.Gray;

        private bool InitFormLoad = true;

        private string InContents = "";

        private bool ImplementFolderChange = false;

        private SortBy CurStyle = SortBy.Alpha;

        private string[] sArray;

        private string[,] tempFolderLyricsHeading = new string[41, 2];

        private string[] Verse = new string[160];

        private string KeyCapoText = "";

        private string[] BackgroundImagename = new string[1024];

        private string BackgroundCurImagePath = "";

        private int BackgroundTotalImagesCount = 0;

        private string[] ExternalPPTIHoldersFileName = new string[1024];

        private string[] ExternalPPImagename = new string[1024];

        private string ExternalPPCurImagePath = "";

        private int ExternalPPTotalImagesCount = 0;

        private string[] OutputPPTIHoldersFileName = new string[1024];

        private string[] OutputPPImagename = new string[1024];

        private int[] OutputPPSlideNumber = new int[1024];

        private int OutputPPTotalImagesCount = 0;

        private string[] PreviewPPTIHoldersFileName = new string[1024];

        private string[] PreviewPPImagename = new string[1024];

        private int[] PreviewPPSlideNumber = new int[1024];

        private int PreviewPPTotalImagesCount = 0;

        private bool UpdatingFormatFields = false;

        private bool UpdatingDefaultFields = false;

        private string[] LastSelectedSongsListItem = new string[2];

        private bool HB_SearchInProgress = false;

        private string HB_CurSelectedTitle = "";

        private string HB_CurSelectedPassages = "";

        private string HB_CurSelectedFormat = "";

        private int WorshipListInsertAt = -1;

        private string GenerateListingDir = "";

        private string GenerateListingFileName = "";

        private bool SaveToRegistryOnClosing = true;

        private string StatusBarOutputPaneMess = "";

        private Bitmap SingleMonIcon;

        private Bitmap DualMonIcon;

        private Bitmap Keyboard1Icon;

        private PowerPoint MainPPT = new PowerPoint();

        private int frmHeight;

        private int frmWidth;

        private int frmTop;

        private int frmLeft;

        private ImageCanvas[] BackgroundImagesCanvas = new ImageCanvas[1024];

        private ImageCanvas[] Powerpoint_PreviewCanvas = new ImageCanvas[1024];

        private ImageCanvas[] Powerpoint_OutputCanvas = new ImageCanvas[1024];

        private ImageCanvas[] Powerpoint_ExternalCanvas = new ImageCanvas[1024];

        private ImageTransitionControl PreviewScreen = new ImageTransitionControl();

        private ImageTransitionControl OutputScreen = new ImageTransitionControl();

        private int ThumbImageClicked = 0;

        private RichTextBox[] Lyrics_PreviewBox = new RichTextBox[30000];

        private RichTextBox[] Lyrics_OutputBox = new RichTextBox[30000];

        private bool WorshipListDoubleClick = false;

        private int LoadRepaintCount = 0;

        private bool FormFirstLoad = true;

        private FrmLaunchShow LiveShow = new FrmLaunchShow();

        private FrmShowAlert AlertWindow = new FrmShowAlert();

        public static FrmMain frmMain = null;

        public FrmMain()
        {
            InitializeComponent();

            Init();

            frmMain = this;

        }

        private void Init()
        {
            if (!gf.InitEasiSlidesDir())
            {
                gf.SplashScreenCanClose = true;
                Close();
            }
            else if (!InitFormControls())
            {
                gf.SplashScreenCanClose = true;
                Close();
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            /// Keyboard Hooking ??��
            RemoveHookBlackScreen();
            AddHookBlackScreen();

            RemoveHookSlideUpDown();
            AddHookSlideUpDown();

            gf.WMP_Present = DShowPlayerPresent();
            if (WorshipListItems.Items.Count > 0)
            {
                WorshipListIndexChanged(0, GetFirstItem: true);
            }
            LiveShow.OnMessage += LiveShow_OnMessage;

            MainPPT.Init();
            MainPPT.preViewToOutputEvent = new OfficeLib.PreViewToOutputEvent(PreViewToOutput);
            TimerToFront.Start();
            gf.SplashScreenCanClose = true;

            gf.InitFolderFiles(gf.EasiSlidesTempDir);

            /// Daniel 25.12.26
            /// Test Resource Extraction
            //string outputDir = @"E:\WorkSpace\wp - ms\dev\EasiSlides_v2.5.9\frmMainResource";
            //string resxPath = @"E:\WorkSpace\wp-ms\dev\EasiSlides_v2.5.9\Easislides\Easislides\FrmMain.resx";
            //Extract(resxPath, outputDir);

        }

        public static void Extract(string resxPath, string outputDir)
        {
            Console.WriteLine($"Processing: {resxPath}");
            if (!Directory.Exists(outputDir)) Directory.CreateDirectory(outputDir);

            using (ResXResourceReader reader = new ResXResourceReader(resxPath))
            {
                reader.UseResXDataNodes = true; // ??��????��????��????????�� ��??

                foreach (DictionaryEntry entry in reader)
                {
                    string key = entry.Key.ToString();
                    ResXDataNode node = (ResXDataNode)entry.Value;

                    try
                    {
                        // ??�� ??????��????�� ??�?�
                        object value = node.GetValue((ITypeResolutionService)null);

                        if (value == null) continue;

                        // 1. ??�� ????�� (Bitmap) ó��
                        if (value is Bitmap bmp)
                        {
                            SaveImage(bmp, outputDir, key, ImageFormat.Png);
                        }
                        // 2. ??��??(Icon) ó�� - $this.Icon ??
                        else if (value is Icon icon)
                        {
                            // ??����?? Bitmap??�� ��??��??????�??� .ico??????
                            using (FileStream fs = new FileStream(Path.Combine(outputDir, $"{key}.ico"), FileMode.Create))
                            {
                                icon.Save(fs);
                            }
                            Console.WriteLine($"[Icon] Extracted: {key}.ico");
                        }
                        // 3. ????�� ����???????(ImageStream) ó�� - imageListSys.ImageStream ??
                        else if (value is ImageListStreamer streamer)
                        {
                            using (ImageList imgList = new ImageList())
                            {
                                imgList.ImageStream = streamer;
                                for (int i = 0; i < imgList.Images.Count; i++)
                                {
                                    string subName = $"{key}_{i}";
                                    SaveImage(imgList.Images[i], outputDir, subName, ImageFormat.Png);
                                }
                                Console.WriteLine($"[ImageList] Extracted {imgList.Images.Count} images from {key}");
                            }
                        }
                        // 4. ����??????�� (TrayLocation ??
                        else
                        {
                            // Point, Size ??�� ?????? �?�????��?????
                            // Console.WriteLine($"[Skip] {key} is type {value.GetType().Name}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[Error] Failed to extract {key}: {ex.Message}");
                    }
                }
            }
            Console.WriteLine("Extraction Complete.");
        }

        private static void SaveImage(Image img, string dir, string name, ImageFormat format)
        {
            // ??�?��� ??????? ???? ���� ??��
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                name = name.Replace(c, '_');
            }

            string ext = format == ImageFormat.Png ? "png" : "jpg";
            string path = Path.Combine(dir, $"{name}.{ext}");

            img.Save(path, format);
            Console.WriteLine($"[Image] Extracted: {name}.{ext}");
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

        private void Form1_Resize(object sender, EventArgs e)
        {
            if (FormFirstLoad && base.WindowState == FormWindowState.Maximized)
            {
                ApplySplitterValues();
                FormFirstLoad = false;
            }
        }

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
        /// ???????����????��??���� ??��??�� ���� ��??.
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

        private void ResizePreviewBottomPanel()
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

            // Daniel Park ??�� 2023??12??24??
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
                // �?����� ??��??????��??��??
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
        /// ??��??ResizeSampleScreen version 1
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
                // �?����� ??��??????��??��??
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
        /// ??�� ResizeSampleScreen
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

        private void IndradioButtonTextFormatInfo_Click(object sender, EventArgs e)
        {
            SetPreviewAreas();
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

        private void IndcbPreviewNotes_Click(object sender, EventArgs e)
        {
            SetNotesPreview(IndcbPreviewNotes.Checked);
        }

        private void SetNotesPreview(bool NotesChecked)
        {
            panelPreviewSessionNotes2.Visible = NotesChecked;
        }

        private void FocusPreviewArea()
        {
            HighlightPreviewRichTextBox(OnEnter: true, ScrollToTop: true);
            Focus();
        }

        private void FocusOutputArea()
        {
            OutputInfo.Focus();
            HighlightOutputRichTextBox(OnEnter: true, ScrollToTop: true);
            Focus();
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
                    //??�??� ����????��??��????��????��
                    gf.ClearUpPowerpointWindows();
                    GfFileHelpers.DeleteFolderFilesSafe(gf.EasiSlidesTempDir);

                }
                catch
                {
                }
            }
        }

        private void SaveFormStateToRegistry()
        {
            switch (base.WindowState)
            {
                case FormWindowState.Maximized:
                    RegUtil.SaveRegValue("settings", "MainMax", 1);
                    RegUtil.SaveRegValue("settings", "MainLeft", base.RestoreBounds.Left);
                    RegUtil.SaveRegValue("settings", "MainTop", base.RestoreBounds.Top);
                    RegUtil.SaveRegValue("settings", "MainWidth", base.RestoreBounds.Width);
                    RegUtil.SaveRegValue("settings", "MainHeight", base.RestoreBounds.Height);
                    break;
                case FormWindowState.Normal:
                    RegUtil.SaveRegValue("settings", "MainMax", 0);
                    RegUtil.SaveRegValue("settings", "MainLeft", base.Left);
                    RegUtil.SaveRegValue("settings", "MainTop", base.Top);
                    RegUtil.SaveRegValue("settings", "MainWidth", base.Width);
                    RegUtil.SaveRegValue("settings", "MainHeight", base.Height);
                    break;
                case FormWindowState.Minimized:
                    gf.SaveConfigSettings();
                    return;
            }
            RegUtil.SaveRegValue("settings", Splitter_FolderWidth, splitContainerMain.SplitterDistance);
            RegUtil.SaveRegValue("settings", Splitter_OutputWidth, splitContainer2.SplitterDistance);
            RegUtil.SaveRegValue("settings", Splitter_FolderHeight, splitContainer1.SplitterDistance);
            RegUtil.SaveRegValue("settings", Splitter_PreviewLyricsHeight, splitContainerPreview.SplitterDistance);
            RegUtil.SaveRegValue("settings", Splitter_outputLyricsHeight, splitContainerOutput.SplitterDistance);
            gf.SaveConfigSettings();
        }

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
                gf.TabSourceImagesChanged = true;
            }
            if (IsSelectedTab(tabControlSource, "tabPowerpoint") && gf.PowerpointListingStyle > 0)
            {
                LoadExternalPowerpointThumbImages(0);
                gf.TabSourceExternalFilesChanged = false;
            }
            if (IsSelectedTab(tabControlSource, "tabFiles"))
            {
                gf.TabSourceExternalFilesChanged = true;
            }
            int num = tabControlSource.Width * 7 / 12;
            num = ((num < 60) ? 60 : num);
            BookLookup.Width = num;
            BibleUserLookup.Left = BookLookup.Left + BookLookup.Width + 2;
            num = tabControlSource.Width - (BibleUserLookup.Left + 12);
            if (num > 0)
            {
                if (num < 40)
                {
                    BibleUserLookup.Width = num;
                }
                else if (num < 40 + panelBible2.Width)
                {
                    BibleUserLookup.Width = 40;
                }
                else
                {
                    BibleUserLookup.Width = num - panelBible2.Width;
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
            int num2 = tabControlSource.Width - DefgroupBox1.Width;
            num2 = ((num2 > 0) ? (num2 / 2) : 0);
            DefApplyDefaultsBtn.Left = num2;
            panelDefTemplate.Left = DefApplyDefaultsBtn.Left + DefApplyDefaultsBtn.Width;
            DefgroupBox1.Left = num2;
            DefgroupBox2.Left = num2;
            DefgroupBox3.Left = num2;
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
            string text = "";
            gf.AssignDropDownItem(SelectedMenuItemName: (gf.AutoRotateStyle == 0) ? Main_Rotate0.Name : ((gf.AutoRotateStyle == 1) ? Main_Rotate1.Name : ((gf.AutoRotateStyle != 2) ? Main_Rotate3.Name : Main_Rotate2.Name)), SelectedBtn: ref Main_RotateStyle, InMenuItem1: Main_Rotate0, InMenuItem2: Main_Rotate1, InMenuItem3: Main_Rotate2, InMenuItem4: Main_Rotate3);
            Main_NoRotate.Checked = !gf.AutoRotateOn;
        }

        private void UpdateDefaultFields()
        {
            string text = "";
            gf.AssignDropDownItem(SelectedMenuItemName: (gf.ShowSongHeadings == 0) ? Def_HeadNoTitles.Name : ((gf.ShowSongHeadings != 1) ? Def_HeadFirstScreen.Name : Def_HeadAllTitles.Name), SelectedBtn: ref Def_Head, InMenuItem1: Def_HeadNoTitles, InMenuItem2: Def_HeadAllTitles, InMenuItem3: Def_HeadFirstScreen);
            gf.AssignDropDownItem(SelectedMenuItemName: (gf.ShowSongHeadingsAlign == 1) ? Def_HeadAlignAsR2.Name : ((gf.ShowSongHeadingsAlign == 2) ? Def_HeadAlignLeft.Name : ((gf.ShowSongHeadingsAlign == 3) ? Def_HeadAlignCentre.Name : ((gf.ShowSongHeadingsAlign != 4) ? Def_HeadAlignAsR1.Name : Def_HeadAlignRight.Name))), SelectedBtn: ref Def_HeadAlign, InMenuItem1: Def_HeadAlignAsR1, InMenuItem2: Def_HeadAlignAsR2, InMenuItem3: Def_HeadAlignLeft, InMenuItem4: Def_HeadAlignCentre, InMenuItem5: Def_HeadAlignRight);
            Def_Shadow.Checked = ((gf.UseShadowFont == 1) ? true : false);
            Def_Outline.Checked = ((gf.UseOutlineFont == 1) ? true : false);
            gf.AssignDropDownItem(SelectedMenuItemName: (gf.ShowLyrics == 0) ? Def_ShowRegion1.Name : ((gf.ShowLyrics != 1) ? Def_ShowRegionBoth.Name : Def_ShowRegion2.Name), SelectedBtn: ref Def_Region, InMenuItem1: Def_ShowRegion1, InMenuItem2: Def_ShowRegion2, InMenuItem3: Def_ShowRegionBoth);
            gf.AssignDropDownItem(SelectedMenuItemName: (gf.ShowVerticalAlign == 0) ? Def_VAlignTop.Name : ((gf.ShowVerticalAlign != 1) ? Def_VAlignBottom.Name : Def_VAlignCentre.Name), SelectedBtn: ref Def_VAlign, InMenuItem1: Def_VAlignTop, InMenuItem2: Def_VAlignCentre, InMenuItem3: Def_VAlignBottom);
            Def_Interlace.Checked = ((gf.ShowInterlace == 1) ? true : false);
            Def_Notations.Checked = ((gf.ShowNotations == 1) ? true : false);
            Def_ToZero.Checked = ((gf.ShowCapoZero == 1) ? true : false);
            gf.AssignDropDownItem(SelectedMenuItemName: (gf.BackgroundMode == ImageMode.Tile) ? Def_ImageTile.Name : ((gf.BackgroundMode != ImageMode.Centre) ? Def_ImageBestFit.Name : Def_ImageCentre.Name), SelectedBtn: ref Def_ImageMode, InMenuItem1: Def_ImageTile, InMenuItem2: Def_ImageCentre, InMenuItem3: Def_ImageBestFit);
            UpdateDefaultNoImageButton();
            gf.AssignDropDownItem(SelectedMenuItemName: (gf.ShowFontAlign[0, 0] == 1) ? Def_R1AlignLeft.Name : ((gf.ShowFontAlign[0, 0] != 2) ? Def_R1AlignRight.Name : Def_R1AlignCentre.Name), SelectedBtn: ref Def_R1Align, InMenuItem1: Def_R1AlignLeft, InMenuItem2: Def_R1AlignCentre, InMenuItem3: Def_R1AlignRight);
            gf.AssignDropDownItem(SelectedMenuItemName: (gf.ShowFontAlign[0, 1] == 1) ? Def_R2AlignLeft.Name : ((gf.ShowFontAlign[0, 1] != 2) ? Def_R2AlignRight.Name : Def_R2AlignCentre.Name), SelectedBtn: ref Def_R2Align, InMenuItem1: Def_R2AlignLeft, InMenuItem2: Def_R2AlignCentre, InMenuItem3: Def_R2AlignRight);
            Def_BackColour.ForeColor = gf.ShowScreenColour[0];
            Def_R1Colour.ForeColor = gf.ShowFontColour[0];
            Def_R2Colour.ForeColor = gf.ShowFontColour[1];
            UpdatingDefaultFields = true;
            Def_TransItem.SelectedIndex = gf.ShowItemTransition;
            Def_TransSlides.SelectedIndex = gf.ShowSlideTransition;
            AssignMediaText(ref Def_AssignMedia, gf.MediaOption);
            UpdatingDefaultFields = false;
        }

        private void UpdateDefaultNoImageButton()
        {
            if (gf.BackgroundPicture != "")
            {
                Def_NoImage.Enabled = true;
                Def_NoImage.ToolTipText = "Remove Default Background '" + gf.BackgroundPicture + "'";
            }
            else
            {
                Def_NoImage.ToolTipText = "No Default Background";
                Def_NoImage.Enabled = false;
            }
        }

        private void UpdateDisplayPanelFields()
        {
            string text = "";
            Def_PanelTransparent.Checked = ((gf.PanelBackColourTransparent > 0) ? true : false);
            Def_PanelAsR1.Checked = ((gf.PanelTextColourAsRegion1 > 0) ? true : false);
            Def_PanelShow.Checked = ((gf.ShowDataDisplayMode > 0) ? true : false);
            Def_PanelSong.Checked = ((gf.ShowDataDisplaySongs > 0) ? true : false);
            Def_PanelSlides.Checked = ((gf.ShowDataDisplaySlides > 0) ? true : false);
            Def_PanelTitle.Checked = ((gf.ShowDataDisplayTitle > 0) ? true : false);
            Def_PanelCopyright.Checked = ((gf.ShowDataDisplayCopyright > 0) ? true : false);
            Def_PanelPrevNext.Checked = ((gf.ShowDataDisplayPrevNext > 0) ? true : false);
            Def_PanelFontBold.Checked = ((gf.ShowDataDisplayFontBold > 0) ? true : false);
            Def_PanelFontItalics.Checked = ((gf.ShowDataDisplayFontItalic > 0) ? true : false);
            Def_PanelFontUnderline.Checked = ((gf.ShowDataDisplayFontUnderline > 0) ? true : false);
            Def_PanelFontShadow.Checked = ((gf.ShowDataDisplayFontShadow > 0) ? true : false);
            Def_PanelFontOutline.Checked = ((gf.ShowDataDisplayFontOutline > 0) ? true : false);
            int num = (int)(gf.BottomBorderFactor * 100.0);
            if (num < 6 || num > 20)
            {
                num = 6;
            }
            Def_PanelHeight.Value = (int)(gf.BottomBorderFactor * 100.0);
            Def_PanelFontOutline.Checked = ((gf.ShowDataDisplayFontOutline > 0) ? true : false);
            Def_PanelTextColour.ForeColor = gf.PanelTextColour;
            Def_PanelBackColour.ForeColor = gf.PanelBackColour;
            Def_PanelFontList.Text = gf.ShowDataDisplayFontName;
        }

        private void UpdateDisplayPanelData(bool RefreshSlides)
        {
            SaveWorshipList();
            if (gf.PreviewItem.ItemID != "")
            {
                RefreshSlidesFonts(ref gf.PreviewItem, ImageTransitionControl.TransitionAction.None);
            }
            if (gf.OutputItem.ItemID != "")
            {
                RefreshSlidesFonts(ref gf.OutputItem, ImageTransitionControl.TransitionAction.None);
            }
            if (gf.ShowRunning)
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
                ApplyIndividualFormat(ref gf.PreviewItem);
            }
            else
            {
                NoIndividualFormat();
            }
            SaveWorshipList();
            gf.SetShowBackground(gf.PreviewItem, ref PreviewScreen);
            if (gf.PreviewItem.ItemID != "")
            {
                if (StartAtFirstSlide)
                {
                    gf.PreviewItem.CurSlide = 1;
                }
                RefreshSlidesFonts(ref gf.PreviewItem, TransitionAction);
                ApplyLyricsRichTextBoxFont(0);
            }
            else
            {
                ResetMainPictureBox(ref gf.PreviewItem);
            }
            if (!gf.OutputItem.UseDefaultFormat || !(gf.OutputItem.ItemID != ""))
            {
                return;
            }
            if (StartAtFirstSlide)
            {
                gf.OutputItem.CurSlide = 1;
            }
            if (gf.ShowRunning)
            {
                if (StartAtFirstSlide)
                {
                    gf.MainAction_SongChanged_Transaction = TransitionAction;
                    RemoteControlLiveShow(LiveShowAction.Remote_SongChanged);
                }
                else
                {
                    gf.MainAction_SongChanged_Transaction = TransitionAction;
                    RemoteControlLiveShow(LiveShowAction.Remote_SongChanged);
                }
            }
            RefreshSlidesFonts(ref gf.OutputItem, TransitionAction);
            ApplyLyricsRichTextBoxFont(1);
        }

        private bool RefreshSlidesFonts(ref SongSettings InItem)
        {
            return RefreshSlidesFonts(ref InItem, ImageTransitionControl.TransitionAction.AsStored);
        }

        private bool RefreshSlidesFonts(ref SongSettings InItem, ImageTransitionControl.TransitionAction TransitionAction)
        {
            SetItemFontSettings(ref InItem);
            gf.FormatDisplayLyrics(ref InItem, PrepareSlides: true, UseStoredSequence: true);
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
                SaveWorshipListTemplate(gf.WorshipDir + gf.CurSession + ".est", gf.WorshipTemplatesDir);
            }
            else if (name == "Def_Shadow")
            {
                gf.UseShadowFont = (toolStripButton.Checked ? 1 : 0);
                ApplyDefaultData(StartAtFirstSlide: false);
            }
            else if (name == "Def_Outline")
            {
                gf.UseOutlineFont = (toolStripButton.Checked ? 1 : 0);
                ApplyDefaultData(StartAtFirstSlide: false);
            }
            else if (name == "Def_Interlace")
            {
                gf.ShowInterlace = (toolStripButton.Checked ? 1 : 0);
                ApplyDefaultData(StartAtFirstSlide: false);
            }
            else if (name == "Def_Notations")
            {
                gf.ShowNotations = (toolStripButton.Checked ? 1 : 0);
                ApplyDefaultData(StartAtFirstSlide: true);
            }
            else if (name == "Def_ToZero")
            {
                gf.ShowCapoZero = (toolStripButton.Checked ? 1 : 0);
                ApplyDefaultData(StartAtFirstSlide: true);
            }
            else if (name == "Def_R1Colour")
            {
                if (gf.SelectColorFromBtn(ref Def_R1Colour, ref gf.ShowFontColour[0]))
                {
                    ApplyDefaultData(StartAtFirstSlide: false);
                }
            }
            else if (name == "Def_R2Colour")
            {
                if (gf.SelectColorFromBtn(ref Def_R2Colour, ref gf.ShowFontColour[1]))
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
                if (gf.SelectBackgroundColors(ref Def_BackColour, ref gf.ShowScreenColour[0], ref gf.ShowScreenColour[1], ref gf.ShowScreenStyle, IsDefault: true))
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
                gf.PanelTextColourAsRegion1 = (toolStripButton.Checked ? 1 : 0);
                UpdateDisplayPanelData(RefreshSlides: false);
            }
            else if (name == "Def_PanelTextColour")
            {
                if (gf.SelectColorFromBtn(ref Def_PanelTextColour, ref gf.PanelTextColour))
                {
                    UpdateDisplayPanelData(RefreshSlides: false);
                }
            }
            else if (name == "Def_PanelTransparent")
            {
                gf.PanelBackColourTransparent = (toolStripButton.Checked ? 1 : 0);
                UpdateDisplayPanelData(RefreshSlides: false);
            }
            else if (name == "Def_PanelBackColour")
            {
                if (gf.SelectColorFromBtn(ref Def_PanelBackColour, ref gf.PanelBackColour))
                {
                    UpdateDisplayPanelData(RefreshSlides: false);
                }
            }
            else if (name == "Def_PanelShow")
            {
                gf.ShowDataDisplayMode = (toolStripButton.Checked ? 1 : 0);
                UpdateDisplayPanelData(RefreshSlides: false);
            }
            else if (name == "Def_PanelSong")
            {
                gf.ShowDataDisplaySongs = (toolStripButton.Checked ? 1 : 0);
                UpdateDisplayPanelData(RefreshSlides: false);
            }
            else if (name == "Def_PanelSlides")
            {
                gf.ShowDataDisplaySlides = (toolStripButton.Checked ? 1 : 0);
                UpdateDisplayPanelData(RefreshSlides: false);
            }
            else if (name == "Def_PanelTitle")
            {
                gf.ShowDataDisplayTitle = (toolStripButton.Checked ? 1 : 0);
                UpdateDisplayPanelData(RefreshSlides: false);
            }
            else if (name == "Def_PanelCopyright")
            {
                gf.ShowDataDisplayCopyright = (toolStripButton.Checked ? 1 : 0);
                UpdateDisplayPanelData(RefreshSlides: false);
            }
            else if (name == "Def_PanelPrevNext")
            {
                gf.ShowDataDisplayPrevNext = (toolStripButton.Checked ? 1 : 0);
                UpdateDisplayPanelData(RefreshSlides: false);
            }
            else if (name == "Def_PanelFontBold")
            {
                gf.ShowDataDisplayFontBold = (toolStripButton.Checked ? 1 : 0);
                UpdateDisplayPanelData(RefreshSlides: false);
            }
            else if (name == "Def_PanelFontItalics")
            {
                gf.ShowDataDisplayFontItalic = (toolStripButton.Checked ? 1 : 0);
                UpdateDisplayPanelData(RefreshSlides: false);
            }
            else if (name == "Def_PanelFontUnderline")
            {
                gf.ShowDataDisplayFontUnderline = (toolStripButton.Checked ? 1 : 0);
                UpdateDisplayPanelData(RefreshSlides: false);
            }
            else if (name == "Def_PanelFontShadow")
            {
                gf.ShowDataDisplayFontShadow = (toolStripButton.Checked ? 1 : 0);
                UpdateDisplayPanelData(RefreshSlides: false);
            }
            else if (name == "Def_PanelFontOutline")
            {
                gf.ShowDataDisplayFontOutline = (toolStripButton.Checked ? 1 : 0);
                UpdateDisplayPanelData(RefreshSlides: false);
            }
        }

        private void Def_PanelHeight_MouseUp(object sender, MouseEventArgs e)
        {
            gf.BottomBorderFactor = (double)Def_PanelHeight.Value / 100.0;
            UpdateDisplayPanelData(RefreshSlides: true);
        }

        private void Def_PanelFontList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!InitFormLoad)
            {
                gf.ShowDataDisplayFontName = Def_PanelFontList.Text;
                UpdateDisplayPanelData(RefreshSlides: false);
            }
        }

        private void Def_SetNoImage()
        {
            gf.BackgroundPicture = "";
            ApplyDefaultData(StartAtFirstSlide: false);
            gf.SetShowBackground(gf.OutputItem, ref OutputScreen);
            RefreshSlidesFonts(ref gf.OutputItem);
            UpdateDefaultNoImageButton();
        }

        private void LoadWorshipListTemplate()
        {
            openFileDialog1.Filter = "EasiSlides Template File (*.est)|*.est";
            openFileDialog1.Title = "Load Session Settings from a Template";
            openFileDialog1.InitialDirectory = gf.WorshipTemplatesDir;
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string fileName = openFileDialog1.FileName;
                LoadWorshipList(1, fileName);
                ApplyDefaultData(StartAtFirstSlide: true);
                LoadIndexFilePostAction(UsageMode.Worship);
            }
        }

        private void SaveWorshipListTemplate(string InFileName, string InitDirectory)
        {
            saveFileDialog1.Filter = "EasiSlides Template File (*.est)|*.est";
            saveFileDialog1.Title = "Save Current Session Settings to a Template";
            saveFileDialog1.InitialDirectory = InitDirectory;
            saveFileDialog1.FileName = gf.GetDisplayNameOnly(ref InFileName, UpdateByRef: false, KeepExt: true);
            saveFileDialog1.OverwritePrompt = true;
            saveFileDialog1.AddExtension = true;
            saveFileDialog1.DefaultExt = ".est";
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string fileName = saveFileDialog1.FileName;
                try
                {
                    gf.SaveIndexFile(fileName, ref WorshipListItems, UsageMode.Worship, SaveAllItems: false, "", "");
                }
                catch
                {
                    MessageBox.Show("Error Saving File, please make sure you have write access and try again");
                }
            }
        }

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

        private void ClearFormatPicture()
        {
            gf.PreviewItem.Format.BackgroundPicture = "";
            gf.PreviewItem.Format.TempImageFileName = "";
            gf.SetShowBackground(gf.PreviewItem, ref PreviewScreen, FallBackToDefault: false);
            UpdateFormatData(StartAtFirstSlide: false);
            Ind_NoImage.Enabled = false;
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

        private void ApplyIndividualFormat(ref SongSettings InItem)
        {
            ApplyIndividualFormat(ref InItem, "D");
        }

        private void ApplyIndividualFormat(ref SongSettings InItem, string InItemSym)
        {
            ApplyIndividualFormat(ref InItem, InItemSym, 0);
        }

        private void ApplyIndividualFormat(ref SongSettings InItem, string InItemSym, int FNum)
        {
            Ind_checkBox.Checked = true;
            if (gf.PreviewItem.Source == ItemSource.WorshipList)
            {
                int selectedIndex = gf.GetSelectedIndex(WorshipListItems);
                if (selectedIndex < 0)
                {
                    Ind_checkBox.Checked = false;
                    return;
                }
                bool flag = false;
                gf.SongFormatData = "";
                gf.PreviewItem.UseDefaultFormat = false;
                if (gf.PreviewItem.Type == "I")
                {
                    WorshipListItems.Items[selectedIndex].SubItems[2].Text = gf.PreviewItem.Format.FormatString;
                }
                if (WorshipListItems.Items[selectedIndex].SubItems[2].Text == "")
                {
                    flag = true;
                }
                gf.SongFormatData = WorshipListItems.Items[selectedIndex].SubItems[2].Text;
                gf.LoadIndividualFormatData(ref gf.PreviewItem, gf.SongFormatData);
                UpdateFormatFields();
                if (flag)
                {
                    UpdateFormatData();
                }
            }
            else if (gf.PreviewItem.Source == ItemSource.SongsList)
            {
                gf.PreviewItem.UseDefaultFormat = false;
            }
        }

        private void NoIndividualFormat()
        {
            Ind_checkBox.Checked = false;
            gf.LoadIndividualFormatData(ref gf.PreviewItem, "");
            RefreshSlidesFonts(ref gf.PreviewItem, ImageTransitionControl.TransitionAction.None);
            UpdateFormatFields();
            AllowIndividualFormat(AllowFormat: true, BoxChecked: false);
            if (gf.PreviewItem.Source == ItemSource.WorshipList)
            {
                int selectedIndex = gf.GetSelectedIndex(WorshipListItems);
                gf.PreviewItem.UseDefaultFormat = true;
                bool flag = false;
                if (selectedIndex >= 0)
                {
                    if (WorshipListItems.Items[selectedIndex].SubItems[2].Text != "")
                    {
                        flag = true;
                    }
                    WorshipListItems.Items[selectedIndex].SubItems[2].Text = "";
                }
                if (flag)
                {
                    DisplayLyrics(gf.PreviewItem, 1);
                    SaveWorshipList();
                }
            }
            else if (gf.PreviewItem.Source == ItemSource.SongsList)
            {
                gf.PreviewItem.UseDefaultFormat = true;
                DisplayLyrics(gf.PreviewItem, 1);
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

        private void LoadIndividualTemplate()
        {
            openFileDialog1.Filter = "EasiSlides Template File (*.est)|*.est";
            openFileDialog1.Title = "Load Individual Settings from a Template";
            openFileDialog1.InitialDirectory = gf.SettingsTemplatesDir;
            if (openFileDialog1.ShowDialog() != DialogResult.OK)
            {
                return;
            }
            string fileName = openFileDialog1.FileName;
            string text = LoadWorshipList(2, fileName);
            gf.PreviewItem.Format.FormatString = text;
            string type = gf.PreviewItem.Type;
            int num;
            switch (type)
            {
                default:
                    num = ((!(type == "M")) ? 1 : 0);
                    break;
                case "D":
                case "B":
                case "T":
                case "I":
                case "W":
                    num = 0;
                    break;
            }
            if (num != 0)
            {
                return;
            }
            if (gf.PreviewItem.Source == ItemSource.SongsList)
            {
                gf.SaveFormatStringToDatabase(gf.PreviewItem.ItemID, text);
            }
            else if (gf.PreviewItem.Source == ItemSource.WorshipList)
            {
                int selectedIndex = gf.GetSelectedIndex(WorshipListItems);
                if (selectedIndex < 0)
                {
                    return;
                }
                WorshipListItems.Items[selectedIndex].SubItems[2].Text = gf.PreviewItem.Format.FormatString;
                SaveWorshipList();
            }
            gf.LoadIndividualFormatData(ref gf.PreviewItem, text);
            if (type == "I")
            {
                gf.PreviewItem.Format.TempImageFileName = gf.PreviewItem.Format.BackgroundPicture;
                SaveInfoFilePreview(ReloadImageData: true);
            }
            SetItemFontSettings(ref gf.PreviewItem);
            gf.FormatDisplayLyrics(ref gf.PreviewItem, PrepareSlides: true, UseStoredSequence: true);
            AllowIndividualFormat(AllowFormat: true, BoxChecked: true);
            UpdateFormatFields();
            BuildVerseButtons(gf.PreviewItem);
            DisplayLyrics(gf.PreviewItem, 0, ScrollToCaret: true);
            DisplayItemInfo(gf.PreviewItem, ref PreviewInfo);
        }

        private void SaveIndividualTemplate(string InTitle)
        {
            InTitle = gf.MakeTitleValidFileName(InTitle);
            saveFileDialog1.Filter = "EasiSlides Template File (*.est)|*.est";
            saveFileDialog1.Title = "Save Current Individual Settings to a Template";
            saveFileDialog1.InitialDirectory = gf.SettingsTemplatesDir;
            saveFileDialog1.FileName = InTitle;
            saveFileDialog1.OverwritePrompt = true;
            saveFileDialog1.AddExtension = true;
            saveFileDialog1.DefaultExt = ".est";
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string fileName = saveFileDialog1.FileName;
                try
                {
                    gf.SaveIndexFile(fileName, ref WorshipListItems, UsageMode.Worship, SaveAllItems: false, gf.PreviewItem.Format.FormatString, "");
                }
                catch
                {
                    MessageBox.Show("Error Saving File, please make sure you have write access and try again");
                }
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

        private void AssignMediaText(ref ToolStripButton InButton, int InMediaOption)
        {
            switch (InMediaOption)
            {
                case 1:
                    InButton.Text = "Media: As Title";
                    break;
                case 2:
                    InButton.Text = "Media: Specific";
                    break;
                case 3:
                    InButton.Text = "Media: Live Feed";
                    break;
                default:
                    InButton.Text = "Media: None";
                    break;
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

        private void SetItemFontSettings(ref SongSettings InItem)
        {
            gf.FormatText(ref InItem, gf.PanelBackColour, gf.PanelBackColourTransparent, gf.PanelTextColour, gf.PanelTextColourAsRegion1, InItem.UseDefaultFormat);
        }

        private void ApplyWorshipMode()
        {
            gf.EasiSlidesMode = UsageMode.Worship;
            ModeEnableItems(EnableWorshipMode: true);
            LoadWorshipList(1);
            WorshipListIndexChanged();
            ShowStatusBarSummary();
            DisplayLyrics(gf.PreviewItem, 0);
        }

        private void ApplyPraisebookMode()
        {
            Cursor = Cursors.WaitCursor;
            gf.EasiSlidesMode = UsageMode.PraiseBook;
            ModeEnableItems(EnableWorshipMode: false);
            if (PraiseBook.Text == "")
            {
                if (PraiseBook.Items.Count > 0)
                {
                    PraiseBook.SelectedIndex = 0;
                }
            }
            else
            {
                LoadPraiseBook((PraiseBookItems.Items.Count != 0) ? 1 : 0);
            }
            PraiseBookIndexChanged();
            ShowStatusBarSummary();
            DisplayLyrics(gf.PreviewItem, 0);
            Cursor = Cursors.Default;
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

        private void PraiseBookIndexChanged()
        {
            int selectedIndex = gf.GetSelectedIndex(PraiseBookItems);
            if (selectedIndex >= 0)
            {
                string text = PraiseBookItems.Items[selectedIndex].SubItems[3].Text;
                gf.PreviewItem.CurItemNo = selectedIndex + 1;
                gf.PreviewItem.TotalItems = PraiseBook.Items.Count;
                gf.PreviewItem.Source = ItemSource.PraiseBook;
                LoadItem(ref gf.PreviewItem, text);
            }
            else
            {
                gf.PreviewItem.Type = "";
                gf.PreviewItem.ItemID = "";
                ClearLyrics(ref flowLayoutPreviewLyrics);
            }
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

#if DAO
		public void Load32PraiseBook(int DataType)
		{
			if (!(DataUtil.Trim(PraiseBook.Text) == ""))
			{
				ListViewItem listViewItem = new ListViewItem();
				string text = "";
				string inFileName = gf.PraiseBookDir + gf.CurPraiseBook + ".esp";
				gf.LoadFileContents(inFileName, ref InContents);
				int num = gf.Load32HeaderData(inFileName, InContents, ref gf.HeaderData);
				if (num < 1)
				{
					gf.ApplyHeaderData(1);
					InContents = "";
				}
				if (DataType == 1)
				{
					gf.ApplyHeaderData(1);
					return;
				}
				PraiseBookItems.Items.Clear();
				gf.ApplyHeaderData(1);
				InContents = DataUtil.Mid(InContents, num + 1, InContents.Length - num);
				int num2 = 0;
				int num3 = InContents.IndexOf(">");
				try
				{
					Database daoDb = DbDaoController.GetDaoDb(gf.ConnectStringMainDB);
					Recordset recordset = null;
					while (num3 >= 0)
					{
						string text2 = DataUtil.Trim(DataUtil.Mid(InContents, num2, num3 - num2));
						if (text2 != "")
						{
							int num4 = text2.IndexOf("\\");
							string fNum_ID = DataUtil.Mid(text2, 1, num4 - 1);
							int num5 = text2.IndexOf("\\", num4 + 1);
							if (num5 < 0)
							{
								num5 = text2.Length + 1;
							}
							string displayName = DataUtil.Mid(text2, num4 + 1, num5 - (num4 + 1));
							gf.WorshipListIDOK = true;
							WriteItemtoPraiseBook(daoDb, recordset, DataUtil.Left(text2, 1), fNum_ID, displayName, "");
						}
						text2 = "";
						num2 = num3 + 1;
						num3 = InContents.IndexOf(">", num2);
					}
					if (recordset != null)
					{
						recordset.Close();
						recordset = null;
					}
				}
				catch
				{
				}
				LoadIndexFilePostAction(UsageMode.PraiseBook);
			}
		}
#elif SQLite
        public void Load32PraiseBook(int DataType)
        {
            if (!(DataUtil.Trim(PraiseBook.Text) == ""))
            {
                ListViewItem listViewItem = new ListViewItem();
                string text = "";
                string inFileName = gf.PraiseBookDir + gf.CurPraiseBook + ".esp";
                gf.LoadFileContents(inFileName, ref InContents);
                int num = gf.Load32HeaderData(inFileName, InContents, ref gf.HeaderData);
                if (num < 1)
                {
                    gf.ApplyHeaderData(1);
                    InContents = "";
                }
                if (DataType == 1)
                {
                    gf.ApplyHeaderData(1);
                    return;
                }
                PraiseBookItems.Items.Clear();
                gf.ApplyHeaderData(1);
                InContents = DataUtil.Mid(InContents, num + 1, InContents.Length - num);
                int num2 = 0;
                int num3 = InContents.IndexOf(">");
                try
                {
                    DbConnection connection = DbController.GetDbConnection(gf.ConnectStringMainDB);

                    while (num3 >= 0)
                    {
                        string text2 = DataUtil.Trim(DataUtil.Mid(InContents, num2, num3 - num2));
                        if (text2 != "")
                        {
                            int num4 = text2.IndexOf("\\");
                            string fNum_ID = DataUtil.Mid(text2, 1, num4 - 1);
                            int num5 = text2.IndexOf("\\", num4 + 1);
                            if (num5 < 0)
                            {
                                num5 = text2.Length + 1;
                            }
                            string displayName = DataUtil.Mid(text2, num4 + 1, num5 - (num4 + 1));
                            gf.WorshipListIDOK = true;
                            WriteItemtoPraiseBook(connection, DataUtil.Left(text2, 1), fNum_ID, displayName, "");
                        }
                        text2 = "";
                        num2 = num3 + 1;
                        num3 = InContents.IndexOf(">", num2);
                    }
                }
                catch
                {
                }
                LoadIndexFilePostAction(UsageMode.PraiseBook);
            }
        }
#endif

#if DAO
		private void WriteItemtoPraiseBook(Database db, Recordset rs, string InSym, string FNum_ID, string DisplayName1, string FolderName)
		{
			if (!(FNum_ID == ""))
			{
				ListViewItem listViewItem = new ListViewItem();
				string text = "";
				string text2 = "";
				string text3 = "";
				string text4 = "0";
				string FirstCharSym = "";
				if (InSym == "D")
				{
					bool flag = false;
					try
					{
						string fullSearchString = (!gf.WorshipListIDOK) ? ("select * from SONG where LCase(Title_1) like \"" + DisplayName1.ToLower() + "\"  AND FolderNo = " + gf.GetFolderNumber(FolderName)) : ("select * from SONG where songid = " + FNum_ID + " AND FolderNo > 0 ");
						rs = DbDaoController.GetRecordSet(db, fullSearchString);
						if (!(rs?.EOF ?? true))
						{
							rs.MoveFirst();
							if (DataUtil.GetDataInt(rs, "FolderNo") > 0 && gf.FolderUse[DataUtil.GetDataInt(rs, "FolderNo")] > 0)
							{
								DisplayName1 = DataUtil.GetDataString(rs, "Title_1");
								FolderName = gf.FolderName[DataUtil.GetDataInt(rs, "FolderNo")];
								FNum_ID = "D" + DataUtil.GetDataInt(rs, "songid");
								text4 = ((DataUtil.GetDataString(rs, "song_number") != "") ? DataUtil.GetDataString(rs, "song_number") : "0");
								flag = true;
							}
						}
						rs?.Close();
						if (!flag)
						{
							FNum_ID = "D0";
							DisplayName1 += " <Error - Item Not Found>";
						}
					}
					catch
					{
						FNum_ID = "D0";
						DisplayName1 += " <Error - Item Not Found>";
					}
				}
				else
				{
					FNum_ID = "D0";
					gf.GetDisplayNameOnly(ref DisplayName1, UpdateByRef: true);
				}
				listViewItem = PraiseBookItems.Items.Add(DataUtil.GetCJKTitle(DisplayName1, gf.PB_CJKGroupStyle, ref FirstCharSym));
				listViewItem.SubItems.Add("");
				listViewItem.SubItems.Add(DisplayName1);
				listViewItem.SubItems.Add(FNum_ID);
				listViewItem.SubItems.Add(FirstCharSym);
				listViewItem.SubItems.Add(text4);
			}
		}
#elif SQLite
        private void WriteItemtoPraiseBook(DbConnection connection, string InSym, string FNum_ID, string DisplayName1, string FolderName)
        {
            if (!(FNum_ID == ""))
            {
                ListViewItem listViewItem = new ListViewItem();
                //string text = "";
                //string text2 = "";
                //string text3 = "";
                string text4 = "0";
                string FirstCharSym = "";
                if (InSym == "D")
                {
                    bool flag = false;
                    try
                    {
                        //string fullSearchString = (!gf.WorshipListIDOK) ? ("select * from SONG where lower(Title_1) like \"" + DisplayName1.ToLower() + "\"  AND FolderNo = " + gf.GetFolderNumber(FolderName)) : ("select * from SONG where songid = " + FNum_ID + " AND FolderNo > 0 ");
                        string fullSearchString = (!gf.WorshipListIDOK) ? ($"select * from SONG where lower(Title_1) like \"{DisplayName1.ToLower()}\"  AND FolderNo = {gf.GetFolderNumber(FolderName)}") : ($"select * from SONG where songid = {FNum_ID} AND FolderNo > 0 ");

                        DataRow dr = DbController.GetDataRowScalar(connection, fullSearchString);
                        if (dr != null)
                        {
                            if (DataUtil.GetDataInt(dr, "FolderNo") > 0 && gf.FolderUse[DataUtil.GetDataInt(dr, "FolderNo")] > 0)
                            {
                                DisplayName1 = DataUtil.GetDataString(dr, "Title_1");
                                FolderName = gf.FolderName[DataUtil.GetDataInt(dr, "FolderNo")];
                                FNum_ID = "D" + DataUtil.GetDataInt(dr, "songid");
                                text4 = ((DataUtil.GetDataString(dr, "song_number") != "") ? DataUtil.GetDataString(dr, "song_number") : "0");
                                flag = true;
                            }
                        }

                        if (!flag)
                        {
                            FNum_ID = "D0";
                            DisplayName1 += " <Error - Item Not Found>";
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        Console.WriteLine(ex.StackTrace);
                        FNum_ID = "D0";
                        DisplayName1 += " <Error - Item Not Found>";
                    }
                }
                else
                {
                    FNum_ID = "D0";
                    gf.GetDisplayNameOnly(ref DisplayName1, UpdateByRef: true);
                }
                listViewItem = PraiseBookItems.Items.Add(DataUtil.GetCJKTitle(DisplayName1, gf.PB_CJKGroupStyle, ref FirstCharSym));
                listViewItem.SubItems.Add("");
                listViewItem.SubItems.Add(DisplayName1);
                listViewItem.SubItems.Add(FNum_ID);
                listViewItem.SubItems.Add(FirstCharSym);
                listViewItem.SubItems.Add(text4);
            }
        }
#endif

        private void ShowStatusBarSummary()
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
                // ??�� ���� �?� ??�� ??��??��??????????���� ��??
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

        private void PopulateWorshipList()
        {
            bool flag = false;
            SessionList.Items.Clear();
            if (!Directory.Exists(gf.WorshipDir))
            {
                FileUtil.MakeDir(gf.WorshipDir);
            }
            DirectoryInfo directoryInfo = new DirectoryInfo(gf.WorshipDir);
            FileInfo[] files = directoryInfo.GetFiles("*.esw");
            foreach (FileInfo fileInfo in files)
            {
                string InFileName = fileInfo.Name;
                InFileName = gf.GetDisplayNameOnly(ref InFileName, UpdateByRef: true);
                if (InFileName != "")
                {
                    SessionList.Items.Add(InFileName);
                    if (gf.CurSession == InFileName)
                    {
                        flag = true;
                    }
                }
            }
            if (flag)
            {
                SessionList.Text = gf.CurSession;
            }
            else if (SessionList.Items.Count > 0)
            {
                SessionList.SelectedIndex = 0;
                gf.CurSession = SessionList.Items[0].ToString();
            }
            else
            {
                gf.CurSession = "Worship Service";
                FileUtil.CreateNewFile(gf.WorshipDir + gf.CurSession + ".esw");
                SessionList.Items.Add(gf.CurSession);
                SessionList.SelectedIndex = 0;
            }
            SessionList_Change();
        }

        private void PopulatePraiseBooksList()
        {
            bool flag = false;
            PraiseBook.Items.Clear();
            if (!Directory.Exists(gf.PraiseBookDir))
            {
                FileUtil.MakeDir(gf.PraiseBookDir);
            }
            DirectoryInfo directoryInfo = new DirectoryInfo(gf.PraiseBookDir);
            FileInfo[] files = directoryInfo.GetFiles("*.esp");
            foreach (FileInfo fileInfo in files)
            {
                string InFileName = fileInfo.Name;
                InFileName = gf.GetDisplayNameOnly(ref InFileName, UpdateByRef: true);
                if (InFileName != "")
                {
                    PraiseBook.Items.Add(InFileName);
                    if (gf.CurPraiseBook == InFileName)
                    {
                        flag = true;
                    }
                }
            }
            if (flag)
            {
                PraiseBook.Text = gf.CurPraiseBook;
            }
            else if (PraiseBook.Items.Count > 0)
            {
                PraiseBook.SelectedIndex = 0;
                gf.CurPraiseBook = PraiseBook.Items[0].ToString();
            }
            else
            {
                gf.CurPraiseBook = "PraiseBook 1";
                FileUtil.CreateNewFile(gf.PraiseBookDir + gf.CurPraiseBook + ".esp");
                PraiseBook.Items.Add(gf.CurPraiseBook);
                PraiseBook.SelectedIndex = 0;
            }
            PraiseBook.Text = gf.CurPraiseBook;
        }

        private void PraiseBook_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!InitFormLoad)
            {
                PraiseBookList_Change();
            }
        }

        private void PraiseBookList_Change()
        {
            if (DataUtil.Trim(PraiseBook.Text) == "")
            {
                toolStripPraiseBook1.Items[1].Enabled = false;
                toolStripPraiseBook1.Items[2].Enabled = false;
                toolStripPraiseBook2.Items[0].Enabled = false;
                toolStripPraiseBook2.Items[2].Enabled = false;
                toolStripPraiseBook2.Items[3].Enabled = false;
                PraiseBook.Items.Clear();
                return;
            }
            toolStripPraiseBook1.Items[1].Enabled = IsSelectedTab(tabControlSource, "tabFolders");
            toolStripPraiseBook1.Items[2].Enabled = true;
            toolStripPraiseBook2.Items[0].Enabled = true;
            toolStripPraiseBook2.Items[2].Enabled = true;
            toolStripPraiseBook2.Items[3].Enabled = true;
            if (!InitFormLoad)
            {
                Cursor = Cursors.WaitCursor;
                gf.CurPraiseBook = DataUtil.Trim(PraiseBook.Text);
                LoadPraiseBook(0);
                PraiseBookIndexChanged();
                ShowStatusBarSummary();
                DisplayLyrics(gf.PreviewItem, 0);
                Cursor = Cursors.Default;
            }
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
                //flowLayoutPreviewPowerPoint.Controls.Clear();
            }
            else
            {
                text = "PP_Output";
                //flowLayoutOutputPowerPoint.Controls.Clear();
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

                //if (InPanel.Name == "flowLayoutPreviewPowerPoint")
                //{
                //    flowLayoutPreviewPowerPoint.Controls.Add(InCanvas[i]);
                //}
                //else
                //{
                //    flowLayoutOutputPowerPoint.Controls.Add(InCanvas[i]);
                //}
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

        // PowerPoint 싱글/더블 클릭 처리
        private int pptLastClickedSlide = -1;
        private Control pptLastClickedControl = null;
        private bool pptDoubleClickInProgress = false;

        private void PowerPointImage_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                Control control = (Control)sender;

                // 더블클릭 진행 중이면 싱글 클릭 무시
                if (pptDoubleClickInProgress)
                {
                    return;
                }

                int slideNumber = DataUtil.ObjToInt(control.Tag) + 1;

                // 즉시 슬라이드 이동 (Delay 없음)
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

                // 마지막 클릭 정보 저장 (더블 클릭 감지용)
                pptLastClickedSlide = slideNumber;
                pptLastClickedControl = control;
            }
        }

        /// <summary>
        /// PowerPoint 이미지 더블클릭 처리
        /// 더블클릭 시 PowerPoint 슬라이드쇼 창을 활성화하고 애니메이션/비디오를 트리거합니다.
        /// </summary>
        private void PowerPointImage_DoubleClick(object sender, EventArgs e)
        {
            try
            {
                // 더블클릭 플래그 설정
                pptDoubleClickInProgress = true;

                Control control = (Control)sender;
                int slideNumber = DataUtil.ObjToInt(control.Tag) + 1;

                // 슬라이드 번호 설정
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
                    MainPPT.ActivateSlideShowAndTriggerClick(slideNumber);
                }
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

        private void BuildPicturesFolderList()
        {
            gf.PictureGroups[0, 0] = "Scenery";
            gf.PictureGroups[0, 1] = gf.RootEasiSlidesDir + "Images\\" + gf.PictureGroups[0, 0] + "\\";
            gf.PictureGroups[1, 0] = "Tiles";
            gf.PictureGroups[1, 1] = gf.RootEasiSlidesDir + "Images\\" + gf.PictureGroups[1, 0] + "\\";
            gf.PictureGroups[2, 0] = "Images";
            gf.PictureGroups[2, 1] = gf.ImagesDir;
            gf.PicFolderTotal = 3;
            string[] directories = Directory.GetDirectories(gf.ImagesDir);
            if (directories.Length > 0)
            {
                gf.SingleArraySort(directories);
            }
            string text = gf.RootEasiSlidesDir + "Images\\Scenery";
            string text2 = gf.RootEasiSlidesDir + "Images\\Tiles";
            gf.BuildSubFolderList(gf.ImagesDir, gf.ImagesDir, ref gf.PictureGroups, ref gf.PicFolderTotal);
            ImagesFolder.Items.Clear();
            for (int i = 0; i < gf.PicFolderTotal; i++)
            {
                ImagesFolder.Items.Add(gf.PictureGroups[i, 0]);
            }
        }

        private void BuildInfoScreenFolderList()
        {
            gf.InfoScreenGroups[0, 0] = "InfoScreen Items";
            gf.InfoScreenGroups[0, 1] = gf.InfoScreenDir;
            gf.InfoScreenFolderTotal = 1;
            gf.BuildSubFolderList(gf.InfoScreenDir, gf.InfoScreenDir, ref gf.InfoScreenGroups, ref gf.InfoScreenFolderTotal);
            InfoScreenFolder.Items.Clear();
            for (int i = 0; i < gf.InfoScreenFolderTotal; i++)
            {
                InfoScreenFolder.Items.Add(gf.InfoScreenGroups[i, 0]);
            }
            if (InfoScreenFolder.Items.Count > 0)
            {
                InfoScreenFolder.SelectedIndex = 0;
            }
        }

        private void BuildPowerpointFolderList()
        {
            gf.PowerpointGroups[0, 0] = "Powerpoint Items";
            gf.PowerpointGroups[0, 1] = gf.PowerpointDir;
            gf.PowerpointFolderTotal = 1;
            gf.BuildSubFolderList(gf.PowerpointDir, gf.PowerpointDir, ref gf.PowerpointGroups, ref gf.PowerpointFolderTotal);
            PowerpointFolder.Items.Clear();
            for (int i = 0; i < gf.PowerpointFolderTotal; i++)
            {
                PowerpointFolder.Items.Add(gf.PowerpointGroups[i, 0]);
            }
            if (PowerpointFolder.Items.Count > 0)
            {
                PowerpointFolder.SelectedIndex = 0;
            }
        }

        private void BuildMediaFolderList()
        {
            gf.MediaGroups[0, 0] = "Media Files";
            gf.MediaGroups[0, 1] = gf.MediaDir;
            gf.MediaFolderTotal = 1;
            gf.BuildSubFolderList(gf.MediaDir, gf.MediaDir, ref gf.MediaGroups, ref gf.MediaFolderTotal);
            MediaFolder.Items.Clear();
            for (int i = 0; i < gf.MediaFolderTotal; i++)
            {
                MediaFolder.Items.Add(gf.MediaGroups[i, 0]);
            }
            if (MediaFolder.Items.Count > 0)
            {
                MediaFolder.SelectedIndex = 0;
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

        private void SongFolder_Change()
        {
            if (!(!ImplementFolderChange | (SongFolder.Text == "")))
            {
                if (SongFolder.Items[SongFolder.Items.Count - 1].ToString() == "Search Results:")
                {
                    SongFolder.Items.RemoveAt(SongFolder.Items.Count - 1);
                }
                SongFolder.ForeColor = SongsList.ForeColor;
                bool findItemMediaOnly = gf.FindItemMediaOnly;
                gf.FindItemMediaOnly = false;
                bool findItemInContents = gf.FindItemInContents;
                gf.FindItemInContents = false;
                SongsList.Items.Clear();
                SetSongListColWidth();
                if (SongFolder.Items.Count >= 1)
                {
                    int folderNumber = gf.GetFolderNumber(SongFolder.Text);
                    CurStyle = gf.FolderGroupStyle[folderNumber];
                    Cursor = Cursors.WaitCursor;
                    SetSortButton(CurStyle);
                    SongsList.RightToLeft = ((gf.ShowFontRTL[folderNumber, 0] > 0) ? RightToLeft.Yes : RightToLeft.No);
                    SongsList.RightToLeftLayout = ((gf.ShowFontRTL[folderNumber, 0] > 0) ? true : false);
                    FillList(folderNumber, "", gf.FindItemMediaOnly);
                    gf.FindItemMediaOnly = findItemMediaOnly;
                    gf.FindItemInContents = findItemInContents;
                    ShowStatusBarSummary();
                    Cursor = Cursors.Default;
                }
            }
        }

        private void Menu_useSongNumbering_Click(object sender, EventArgs e)
        {
            ApplyUseSongNumbers(Menu_UseSongNumbering.Checked);
            ImplementFolderChange = true;
            SongFolder_Change();
        }

        private void ApplyUseSongNumbers(bool InUseSongNumbers)
        {
            gf.UseSongNumbers = InUseSongNumbers;
            Menu_UseSongNumbering.Checked = gf.UseSongNumbers;
            if (InUseSongNumbers)
            {
                SongsList.Columns[4].Width = 60;
                Folders_WordCount.Enabled = false;
                PB_WordCount.Enabled = false;
            }
            else
            {
                SongsList.Columns[4].Width = 0;
                Folders_WordCount.Enabled = true;
                PB_WordCount.Enabled = true;
            }
            SetSortButtonPB(gf.PB_CJKGroupStyle);
        }

        private void BuildFolderList()
        {
            ImplementFolderChange = false;
            int num = 0;
            int num2 = 0;
            string text = "";
            string text2 = "";
            int num3;
            if (SongFolder.Items.Count > 0)
            {
                if (SongFolder.SelectedIndex < 0)
                {
                    SongFolder.SelectedIndex = 0;
                }
                num3 = gf.GetFolderNumber(SongFolder.SelectedText);
            }
            else
            {
                num3 = 0;
            }
            SongFolder.Items.Clear();
            for (int i = 1; i < 41; i++)
            {
                if (gf.FolderUse[i] > 0)
                {
                    SongFolder.Items.Add(gf.FolderName[i]);
                    if (gf.GetFolderNumber(gf.FolderName[i]) == num3)
                    {
                        num = i;
                        text = gf.FolderName[i];
                    }
                    if (num2 == 0)
                    {
                        num2 = i;
                        text2 = gf.FolderName[i];
                    }
                }
            }
            if (text == "")
            {
                text = text2;
            }
            SongFolder.Text = text;
            ImplementFolderChange = true;
            SongFolder_Change();
        }

#if DAO
		/// <summary>
		/// daniel find
		/// </summary>
		/// <param name="FNumber"></param>
		/// <param name="ListString"></param>
		/// <param name="InItemMusicOnly"></param>
		private void FillList(int FNumber, string ListString, bool InItemMusicOnly)
		{
			int num = 0;
			string text = "";
			string text2 = "";
			string text3 = "";
			string text4 = "";
			string text5 = "";
			string text6 = "";
			string text7 = "";
			string text8 = "";
			string text9 = "";
			int num2 = 0;
			string text10 = "*";
			gf.TotalMusicFiles = -1;
			bool flag = false;
			string text11 = "";
			if (FNumber == 0)
			{
				for (int i = 1; i < 41; i++)
				{
					if (gf.FindSongsFolder[i])
					{
						text11 = ((!(text11 == "")) ? (text11 + " or FolderNo=" + Convert.ToString(i)) : (" and (FolderNo=" + Convert.ToString(i)));
					}
				}
				text11 += ")";
			}
			else
			{
				text11 = " and FolderNo=" + Convert.ToString(FNumber);
			}
			string str = (FNumber < 0) ? gf.Find_SQLString : ("select * from SONG where (LCase(Title_1) like \"" + text10.ToLower() + "\" " + text11 + ") or (LCase(Title_2) like \"" + text10.ToLower() + "\" " + text11 + ")");
			string str2 = gf.UseSongNumbers ? " order by song_number, cjk_strokecount" : ((CurStyle != SortBy.WordCount) ? " order by cjk_strokecount" : " order by cjk_wordcount, cjk_strokecount");
			str += str2;
			ListViewItem listViewItem = new ListViewItem();
			SongsList.BeginUpdate();
			try
			{
				Recordset recordSet = DbDaoController.GetRecordSet(gf.ConnectStringMainDB, str);
				if (!(recordSet?.EOF ?? true))
				{
					ListViewItem[] array = new ListViewItem[recordSet.RecordCount];
					int num3 = 0;
					recordSet.MoveFirst();
					while (!recordSet.EOF)
					{
						string musicTitle = DataUtil.ObjToString(recordSet.Fields["Title_2"].Value);
						num2 = DataUtil.ObjToInt(recordSet.Fields["song_number"].Value);
						bool flag2 = gf.MusicFound(DataUtil.ObjToString(recordSet.Fields["Title_1"].Value), musicTitle);
						text8 = DataUtil.ObjToString(recordSet.Fields["LICENCE_ADMIN1"].Value);
						text9 = DataUtil.ObjToString(recordSet.Fields["LICENCE_ADMIN2"].Value);
						if ((InItemMusicOnly && flag2) || !InItemMusicOnly)
						{
							array[num3] = new ListViewItem(new string[7]
							{
								DataUtil.ObjToString(recordSet.Fields["Title_1"].Value) + (flag2 ? " <#>" : ""),
								"D" + DataUtil.ObjToString(recordSet.Fields["SongID"].Value),
								"",
								"",
								num2.ToString(),
								text8,
								text9
							});
							num3++;
						}
						recordSet.MoveNext();
					}
					SongsList.Items.AddRange(array);
				}
			}
			catch
			{
			}
			SongsList.EndUpdate();
			ShowStatusBarSummary();
		}
#elif SQLite
        /// <summary>
        /// daniel find
        /// </summary>
        /// <param name="FNumber"></param>
        /// <param name="ListString"></param>
        /// <param name="InItemMusicOnly"></param>
        private void FillList(int FNumber, string ListString, bool InItemMusicOnly)
        {
            int num = 0;
            //string text = "";
            //string text2 = "";
            //string text3 = "";
            //string text4 = "";
            //string text5 = "";
            //string text6 = "";
            //string text7 = "";
            string text8 = "";
            string text9 = "";
            int num2 = 0;
            ///MDB Access??��??Like ��??��??"*" ????��
            //string text10 = "*";
            ///SQLite??��??Like ��??��??"%" ????��
            string text10 = "%";

            gf.TotalMusicFiles = -1;
            bool flag = false;
            string text11 = "";
            if (FNumber == 0)
            {
                for (int i = 1; i < 41; i++)
                {
                    if (gf.FindSongsFolder[i])
                    {
                        text11 = ((!(text11 == "")) ? ("{text11} or FolderNo = {Convert.ToString(i)}") : ($" and (FolderNo = {Convert.ToString(i)}"));
                    }
                }
                text11 += ")";
            }
            else
            {
                text11 = " and FolderNo=" + Convert.ToString(FNumber);
            }
            //string str = (FNumber < 0) ? gf.Find_SQLString : ("select * from SONG where (lower(Title_1) like \"" + text10.ToLower() + "\" " + text11 + ") or (lower(Title_2) like \"" + text10.ToLower() + "\" " + text11 + ")");
            string str = (FNumber < 0) ? gf.Find_SQLString : ($"select * from SONG where (lower(Title_1) like \"{text10.ToLower()}\" {text11}) or (lower(Title_2) like \"{text10.ToLower()}\" {text11})");

            string str2 = gf.UseSongNumbers ? " order by song_number, cjk_strokecount" : ((CurStyle != SortBy.WordCount) ? " order by cjk_strokecount" : " order by cjk_wordcount, cjk_strokecount");
            str += str2;

            string sQuery = str;
            string sQueryCount = str.Replace("*", "Count(*)");

            string sMultiQuery = $"{sQueryCount};{sQuery}";

            ListViewItem listViewItem = new ListViewItem();
            SongsList.BeginUpdate();
            try
            {
                DbConnection connection = null;
                DbDataReader dataReader = null;

                (connection, dataReader) = DbController.GetDataReader(gf.ConnectStringMainDB, sMultiQuery);

                using (connection)
                {
                    using (dataReader)
                    {
                        if (dataReader != null && dataReader.HasRows)
                        {
                            dataReader.Read();

                            ListViewItem[] array = new ListViewItem[DataUtil.ObjToInt(dataReader[0])];
                            int num3 = 0;
                            dataReader.NextResult();

                            while (dataReader.Read())
                            {
                                string musicTitle = DataUtil.ObjToString(dataReader["Title_2"]);
                                num2 = DataUtil.ObjToInt(dataReader["song_number"]);
                                bool flag2 = gf.MusicFound(DataUtil.ObjToString(dataReader["Title_1"]), musicTitle);
                                text8 = DataUtil.ObjToString(dataReader["LICENCE_ADMIN1"]);
                                text9 = DataUtil.ObjToString(dataReader["LICENCE_ADMIN2"]);
                                if ((InItemMusicOnly && flag2) || !InItemMusicOnly)
                                {
                                    array[num3] = new ListViewItem(new string[7]
                                    {
                                DataUtil.ObjToString(dataReader["Title_1"]) + (flag2 ? " <#>" : ""),
                                "D" + DataUtil.ObjToString(dataReader["SongID"]),
                                "",
                                "",
                                num2.ToString(),
                                text8,
                                text9
                                    });
                                    num3++;
                                }
                            }
                            SongsList.Items.AddRange(array);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
            SongsList.EndUpdate();
            ShowStatusBarSummary();
        }
#endif


        private int GetImagesPanelWidth()
        {
            return (SongsList.Width - 25 - 5 * (gf.ThumbImagesPerRow - 1)) / gf.ThumbImagesPerRow;
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

        private void ShowPicturesFolderThumbs()
        {
            if (InitFormLoad)
            {
                return;
            }
            BackgroundCurImagePath = "";
            BackgroundTotalImagesCount = 0;
            if (ImagesFolder.Items.Count <= 0)
            {
                return;
            }
            BackgroundCurImagePath = gf.PictureGroups[ImagesFolder.SelectedIndex, 1];
            string[] array = new string[5]
            {
                ".jpg",
                ".jpeg",
                ".bmp",
                ".gif",
                ".ico"
            };
            for (int i = 0; i < BackgroundImagename.Length; i++)
            {
                BackgroundImagename[i] = "";
            }
            ListBox listBox = new ListBox();
            listBox.Items.Clear();
            listBox.Sorted = false;
            BackgroundTotalImagesCount = 0;
            if (BackgroundCurImagePath != "")
            {
                for (int j = 0; j <= 4; j++)
                {
                    try
                    {
                        string[] files = Directory.GetFiles(BackgroundCurImagePath, "*" + array[j]);
                        string[] array2 = files;
                        foreach (string text in array2)
                        {
                            string text2 = text;
                            listBox.Items.Add(text);
                        }
                    }
                    catch
                    {
                    }
                }
            }
            listBox.Sorted = true;
            BackgroundTotalImagesCount = listBox.Items.Count;
            for (int i = 0; i < ((BackgroundTotalImagesCount < 1024) ? BackgroundTotalImagesCount : 1023); i++)
            {
                BackgroundImagename[i] = listBox.Items[i].ToString();
            }
            FormatBackgroundThumbContainers();
            listBox.Dispose();
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

        private void LoadBackgroundThumbImages()
        {
            LoadThumbImages(flowLayoutImages, ref BackgroundImagesCanvas, BackgroundImagename, BackgroundTotalImagesCount, tabControlSource.Width - 15, "", 0, toolTip1, ExternalPP: false);
        }

        static int previousOutSelectedSlide = 1;

        static int LoadThumbOutlockkey = 0;
        /// <summary>
        /// daniel out ????�� �?�????ü????�� �?��� ??��
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

        static int LoadThumbPreviewlockkey = 0;
        static int previousPreviewSelectedSlide = 1;
        /// <summary>
        /// daniel 4265 ??�� ??��??
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
            if (InCanvas != null && CurSelectedSlide > 0)
            {
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
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);

                    Console.WriteLine(e.ToString());
                }


                InFlowPanel.ScrollControlIntoView(InCanvas[CurSelectedSlide - 1]);

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

        private void ShowPowerpointFolderContents(bool ShowThumbs)
        {
            Cursor = Cursors.WaitCursor;
            ExternalPPCurImagePath = "";
            ExternalPPTotalImagesCount = 0;
            PowerpointList.Items.Clear();
            if (PowerpointFolder.Items.Count <= 0)
            {
                return;
            }
            ExternalPPCurImagePath = gf.PowerpointGroups[PowerpointFolder.SelectedIndex, 1];
            for (int i = 0; i < ExternalPPImagename.Length; i++)
            {
                ExternalPPImagename[i] = "";
            }
            ListBox listBox = new ListBox();
            ListViewItem listViewItem = new ListViewItem();
            listBox.Items.Clear();
            listBox.Sorted = false;
            ExternalPPTotalImagesCount = 0;
            if (ExternalPPCurImagePath != "")
            {
                try
                {
                    // daniel
                    //string[] files1 = Directory.GetFiles(ExternalPPCurImagePath, "*.ppt");
                    var files = Directory.GetFiles(ExternalPPCurImagePath, "*", SearchOption.AllDirectories)
                        .Where(s => s.EndsWith(".ppt") || s.EndsWith(".pptx"));
                    string[] array = files.ToArray();
                    foreach (string text in array)
                    {
                        string text2 = text;
                        listBox.Items.Add(text);
                    }
                }
                catch
                {
                }
            }
            listBox.Sorted = true;
            ExternalPPTotalImagesCount = listBox.Items.Count;
            string text3 = "";
            for (int i = 0; i < ((ExternalPPTotalImagesCount < 1024) ? ExternalPPTotalImagesCount : 1023); i++)
            {
                if (gf.PowerpointListingStyle == 0)
                {
                    text3 = listBox.Items[i].ToString();
                    listViewItem = PowerpointList.Items.Add(gf.GetDisplayNameOnly(ref text3, UpdateByRef: false, KeepExt: false));
                    listViewItem.SubItems.Add("P" + listBox.Items[i]);
                }
                else
                {
                    ExternalPPImagename[i] = listBox.Items[i].ToString();
                }
            }
            SetPowerpointListColWidth();
            if (gf.PowerpointListingStyle == 1)
            {
                gf.ExtPPrefix_Num++;
                if (!Directory.Exists(gf.ExtPPrefix + gf.ExtPPrefix_Num + "\\"))
                {
                    FileUtil.MakeDir(gf.ExtPPrefix + gf.ExtPPrefix_Num + "\\");
                }
                gf.ExternalPPT.BuildFirstScreenDump(ExternalPPImagename, listBox.Items.Count, gf.ExtPPrefix + gf.ExtPPrefix_Num + "\\");
                FormatExternalPowerpointThumbContainers();
            }
            SetPowerpointListColWidth();
            listBox.Dispose();
            Cursor = Cursors.Default;
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

        private void LoadExternalPowerpointThumbImages(int GotoSlide)
        {
            LoadThumbImages(flowLayoutExternalPowerPoint, ref Powerpoint_ExternalCanvas, ExternalPPImagename, ExternalPPTotalImagesCount, tabControlSource.Width - 10, "", GotoSlide, toolTip1, ExternalPP: true);
        }

        private void toolStripImages1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
        }

        /// <summary>
        /// daniel ??��? ??��??(??�� ????)
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

        private void BookLookupChanged()
        {
            if (InvokeRequired)
            {
                this.Invoke(new MethodInvoker(delegate
                {
                    if (!HB_SearchInProgress)
                    {
                        Cursor = Cursors.WaitCursor;
                        if (BookLookup.Items.Count > 66)
                        {
                            BookLookup.Items.RemoveAt(66);
                        }
                        BibleText.Text = "";
                        BibleUserLookup.Text = "";
                        gf.LoadBiblePassagesFromTabIndex(TabBibleVersions.SelectedIndex, BookLookup, ref BibleText, gf.HB_ShowVerses);
                        gf.HB_SequentialListing = true;
                        ShowStatusBarSummary();
                        Cursor = Cursors.Default;
                    }

                }));
            }
            else
            {
                if (!HB_SearchInProgress)
                {
                    Cursor = Cursors.WaitCursor;
                    if (BookLookup.Items.Count > 66)
                    {
                        BookLookup.Items.RemoveAt(66);
                    }
                    BibleText.Text = "";
                    BibleUserLookup.Text = "";
                    //??������ �?��?�??�� ??�� ??��????��??�� ??�� ����????�� ??
                    gf.LoadBiblePassagesFromTabIndex(TabBibleVersions.SelectedIndex, BookLookup, ref BibleText, gf.HB_ShowVerses);
                    gf.HB_SequentialListing = true;
                    ShowStatusBarSummary();
                    Cursor = Cursors.Default;
                }
            }
        }

        private void Bibles_ShowVerses_Click(object sender, EventArgs e)
        {
        }

        private void TabBibleVersionsChanged()
        {
            if (!TabBibleVersions.Enabled)
            {
                return;
            }
            if (BookLookup.SelectedIndex == 66)
            {
                HB_SearchInProgress = true;
            }
            if (gf.LoadBibleBooksList(TabBibleVersions, ref BookLookup, HB_SearchInProgress, null))
            {
                gf.HB_CurVersionTabIndex = TabBibleVersions.SelectedIndex;
                if ((BookLookup.SelectedIndex == 66) & (gf.HB_SQLString == ""))
                {
                    Cursor = Cursors.WaitCursor;
                    gf.RefreshBiblePassages(gf.HB_CurVersionTabIndex, BookLookup, ref BibleText, gf.HB_ShowVerses);
                    Cursor = Cursors.Default;
                }
                HB_ReselectSame();
            }
            else
            {
                BibleText.Text = "";
            }
            HB_SearchInProgress = false;
            ShowStatusBarSummary();
        }

        private void HB_ReselectSame()
        {
            if (!((BibleText.Text == "") | (HB_CurSelectedPassages == "")))
            {
                try
                {
                    string InString = HB_CurSelectedPassages;
                    DataUtil.ExtractOneInfo(ref InString, ';');
                    DataUtil.ExtractOneInfo(ref InString, ';');
                    DataUtil.ExtractOneInfo(ref InString, ';');
                    int num = DataUtil.StringToInt(DataUtil.ExtractOneInfo(ref InString, ';'));
                    int num2 = DataUtil.StringToInt(DataUtil.ExtractOneInfo(ref InString, ';'));
                    int num3 = DataUtil.StringToInt(DataUtil.ExtractOneInfo(ref InString, ';'));
                    int num4 = DataUtil.StringToInt(DataUtil.ExtractOneInfo(ref InString, ';'));
                    int num5 = DataUtil.StringToInt(DataUtil.ExtractOneInfo(ref InString, ';'));
                    int num6 = num;
                    while (InString != "")
                    {
                        num6 = Convert.ToInt32(DataUtil.ExtractOneInfo(ref InString, ';'));
                        DataUtil.ExtractOneInfo(ref InString, ';');
                        DataUtil.ExtractOneInfo(ref InString, ';');
                        num4 = DataUtil.StringToInt(DataUtil.ExtractOneInfo(ref InString, ';'));
                        num5 = DataUtil.StringToInt(DataUtil.ExtractOneInfo(ref InString, ';'));
                    }
                    BibleText.Focus();
                    for (int i = 1; i <= gf.HB_VersesLocation[0, 0]; i++)
                    {
                        if ((gf.HB_VersesLocation[i, 1] == num) & (gf.HB_VersesLocation[i, 2] == num2) & (gf.HB_VersesLocation[i, 3] == num3))
                        {
                            BibleText.SelectionStart = gf.HB_VersesLocation[i, 4];
                            for (int j = i; j <= gf.HB_VersesLocation[0, 0]; j++)
                            {
                                if ((gf.HB_VersesLocation[j, 1] == num6) & (gf.HB_VersesLocation[j, 2] == num4) & (gf.HB_VersesLocation[j, 3] == num5))
                                {
                                    BibleText.SelectionLength = gf.HB_VersesLocation[j, 4] - gf.HB_VersesLocation[i, 4] + gf.HB_VersesLocation[j, 5] - 2;
                                    BibleText.ScrollToCaret();
                                    j = gf.HB_VersesLocation[0, 0];
                                }
                            }
                            i = gf.HB_VersesLocation[0, 0];
                        }
                    }
                    HB_StartBuildStringProcess();
                }
                catch
                {
                }
            }
        }

        private bool HB_StartBuildStringProcess()
        {
            if (gf.HB_TotalVersions >= 1)
            {
                HB_CurSelectedTitle = "";
                HB_CurSelectedPassages = HB_BuildSelectionString(TabBibleVersions.SelectedIndex, ref HB_CurSelectedTitle);
                HB_SelectedPassagesChanged(HB_CurSelectedPassages, ref HB_CurSelectedTitle);
                HB_CurSelectedFormat = "";
                BibleText.Focus();
                return true;
            }
            HB_CurSelectedPassages = "";
            HB_CurSelectedTitle = "";
            BibleText.Focus();
            return false;
        }

        private string HB_BuildSelectionString(int InBibleVersion, ref string OutputTitle)
        {
            if (BibleText.Text != "")
            {
                string displayNameOnly = gf.GetDisplayNameOnly(ref gf.HB_Versions[InBibleVersion, 4], UpdateByRef: false, KeepExt: true);
                int num = 0;
                int num2 = 0;
                string text = "";
                int num3 = BibleText.SelectionStart + 2;
                int num4 = num3 + BibleText.SelectionLength;
                if (num3 >= 0)
                {
                    for (int i = 1; i <= gf.HB_VersesLocation[0, 0]; i++)
                    {
                        if (!((num3 >= gf.HB_VersesLocation[i, 4]) & (num3 <= gf.HB_VersesLocation[i, 4] + gf.HB_VersesLocation[i, 5])))
                        {
                            continue;
                        }
                        num = i;
                        for (int j = i; j <= gf.HB_VersesLocation[0, 0]; j++)
                        {
                            if ((num4 >= gf.HB_VersesLocation[j, 4]) & (num4 <= gf.HB_VersesLocation[j, 4] + gf.HB_VersesLocation[j, 5]))
                            {
                                num2 = j;
                                j = 3002;
                            }
                        }
                        i = 3002;
                    }
                }
                int num5 = num2 - num + 1;
                if (gf.HB_SequentialListing)
                {
                    if (num5 > gf.HB_MaxVersesSelection)
                    {
                        num2 = num + gf.HB_MaxVersesSelection - 1;
                    }
                }
                else if (num5 > gf.HB_MaxAdhocVersesSelection)
                {
                    num2 = num + gf.HB_MaxAdhocVersesSelection - 1;
                }
                if (num >= 0)
                {
                    BibleText.SelectionStart = gf.HB_VersesLocation[num, 4];
                    if (num2 < 0)
                    {
                        num2 = num;
                    }
                    BibleText.SelectionLength = gf.HB_VersesLocation[num2, 4] - gf.HB_VersesLocation[num, 4] + gf.HB_VersesLocation[num2, 5] - 2;
                }
                BibleText.ScrollToCaret();
                if (gf.HB_SequentialListing)
                {
                    string text2 = Convert.ToString(gf.HB_VersesLocation[num, 2]) + ":" + Convert.ToString(gf.HB_VersesLocation[num, 3]);
                    string text3 = " - " + Convert.ToString(gf.HB_VersesLocation[num2, 2]) + ":" + Convert.ToString(gf.HB_VersesLocation[num2, 3]);
                    if (gf.HB_VersesLocation[num, 2] == gf.HB_VersesLocation[num2, 2])
                    {
                        text3 = ((gf.HB_VersesLocation[num, 3] != gf.HB_VersesLocation[num2, 3]) ? ("-" + Convert.ToString(gf.HB_VersesLocation[num2, 3])) : "");
                    }
                    OutputTitle = gf.LookUpBookName(InBibleVersion, gf.HB_VersesLocation[num, 1]) + " " + text2 + text3 + " (" + gf.HB_Versions[InBibleVersion, 1] + ")";
                    text = "0" + ';' + displayNameOnly + ';' + ';';
                    int i;
                    for (i = num; i <= num2; i++)
                    {
                        int num6;
                        for (num6 = i; gf.HB_VersesLocation[i, 2] == gf.HB_VersesLocation[num6, 2] && i <= num2; i++)
                        {
                        }
                        int j = i - 1;
                        object obj = text;
                        text = string.Concat(obj, Convert.ToString(gf.HB_VersesLocation[num6, 1]), ';', Convert.ToString(gf.HB_VersesLocation[num6, 2]), ';', Convert.ToString(gf.HB_VersesLocation[num6, 3]), ';', Convert.ToString(gf.HB_VersesLocation[j, 2]), ';', Convert.ToString(gf.HB_VersesLocation[j, 3]), ';');
                        i = j;
                    }
                }
                else
                {
                    text = "1" + ';' + displayNameOnly + ';' + ';';
                    for (int i = num; i <= num2; i++)
                    {
                        string text4 = OutputTitle;
                        OutputTitle = text4 + DataUtil.Trim(DataUtil.Left(gf.LookUpBookName(InBibleVersion, gf.HB_VersesLocation[i, 1]), 4)) + " " + Convert.ToString(gf.HB_VersesLocation[i, 2]) + ":" + Convert.ToString(gf.HB_VersesLocation[i, 3]) + ",";
                        object obj = text;
                        text = string.Concat(obj, Convert.ToString(gf.HB_VersesLocation[i, 1]), ';', Convert.ToString(gf.HB_VersesLocation[i, 2]), ';', Convert.ToString(gf.HB_VersesLocation[i, 3]), ';', Convert.ToString(gf.HB_VersesLocation[i, 2]), ';', Convert.ToString(gf.HB_VersesLocation[i, 3]), ';');
                    }
                    OutputTitle = DataUtil.Left(OutputTitle, OutputTitle.Length - 1);
                    if (OutputTitle.Length > 60)
                    {
                        OutputTitle = DataUtil.Left(OutputTitle, 60) + " .. ";
                    }
                    OutputTitle = OutputTitle + " (" + gf.HB_Versions[InBibleVersion, 1] + ")";
                }
                return text;
            }
            return "";
        }

        private void TabBibleVersions_Click(object sender, EventArgs e)
        {
            if (TabBibleVersions.SelectedIndex != gf.HB_CurVersionTabIndex)
            {
                TabBibleVersionsChanged();
            }
        }

        private void SessionList_Change()
        {
            gf.CurSession = SessionList.Text;
            Cursor = Cursors.WaitCursor;
            LoadWorshipList(0);
            WriteCurSession();
            if (gf.ShowRunning)
            {
                ValidateWorshipListItems(ShowErrorMessage: false);
                gf.PreLoadPowerpointFiles(ref gf.LivePP, ref gf.WorshipSongs);
            }
            Cursor = Cursors.Default;
        }

        private void WriteCurSession()
        {
            RegUtil.SaveRegValue("config", "current_session", gf.CurSession);
        }

        private void WriteCurPraiseBook()
        {
            RegUtil.SaveRegValue("config", "current_praisebook", gf.CurPraiseBook);
        }

        private void LoadWorshipList(int DataType)
        {
            string inFileName = gf.WorshipDir + gf.CurSession + ".esw";
            LoadWorshipList(DataType, inFileName);
        }

        private string LoadWorshipList(int DataType, string InFileName)
        {
            gf.StartPresAt = 0;
            string result = LoadIndexFile(DataType, InFileName, ref WorshipListItems, UsageMode.Worship, ref gf.CurSessionNotes);
            PreviewNotes.Text = gf.CurSessionNotes;
            return result;
        }

        private void LoadPraiseBook(int DataType)
        {
            string inFileName = gf.PraiseBookDir + gf.CurPraiseBook + ".esp";
            LoadIndexFile(DataType, inFileName, ref PraiseBookItems, UsageMode.PraiseBook, ref gf.CurPraiseBookNotes);
        }

        /// <summary>
        /// daniel
        /// </summary>
        /// <param name="DataType"></param>
        /// <param name="InFileName"></param>
        /// <param name="InList"></param>
        /// <param name="InMode"></param>
        /// <param name="InNotes"></param>
        /// <returns></returns>
        private string LoadIndexFile(int DataType, string InFileName, ref ListView InList, UsageMode InMode, ref string InNotes)
        {
            string text = "";
            try
            {
                XmlTextReader xmlTextReader = new XmlTextReader(InFileName);
                try
                {
                    bool flag = false;
                    bool flag2 = false;
                    bool flag3 = false;
                    xmlTextReader.Read();
                    while (xmlTextReader.Read() && !flag)
                    {
                        if ((xmlTextReader.NodeType == XmlNodeType.Element) & (xmlTextReader.Name == "EasiSlides"))
                        {
                            flag = true;
                        }
                    }
                    if (flag)
                    {
                        while (xmlTextReader.Read() && !flag2)
                        {
                            if ((xmlTextReader.NodeType == XmlNodeType.Element) & (xmlTextReader.Name == "ListItem"))
                            {
                                flag2 = true;
                            }
                        }
                        if (!flag2)
                        {
                            xmlTextReader?.Close();
                        }
                        else
                        {
                            while (xmlTextReader.Read() && !flag3)
                            {
                                if ((xmlTextReader.NodeType == XmlNodeType.Element) & (xmlTextReader.Name == "ListHeader"))
                                {
                                    flag3 = true;
                                }
                            }
                            if (!flag3)
                            {
                                xmlTextReader?.Close();
                            }
                            else
                            {
                                string text2 = "";
                                string displayName = "";
                                string folderName = "";
                                string formatString = "";
                                xmlTextReader.Read();
                                gf.WorshipListIDOK = false;
                                if ((xmlTextReader.NodeType == XmlNodeType.Element) & (xmlTextReader.Name == "SystemID"))
                                {
                                    if (gf.SystemID == xmlTextReader.ReadElementContentAsString())
                                    {
                                        gf.WorshipListIDOK = true;
                                    }
                                    xmlTextReader.Read();
                                    if ((xmlTextReader.NodeType == XmlNodeType.Element) & (xmlTextReader.Name == "FormatData"))
                                    {
                                        text = xmlTextReader.ReadElementContentAsString();
                                        if (DataType == 2)
                                        {
                                            return text;
                                        }
                                        gf.LoadHeaderData(text, ref gf.HeaderData, '>');
                                        if (DataType == 1)
                                        {
                                            gf.ApplyHeaderData();
                                            if (InMode == UsageMode.Worship)
                                            {
                                                UpdateDefaultFields();
                                                UpdateDisplayPanelFields();
                                            }
                                            return text;
                                        }
                                        gf.ApplyHeaderData();
                                        InList.Items.Clear();
                                        InNotes = "";
                                        xmlTextReader.Read();
                                        if ((xmlTextReader.NodeType == XmlNodeType.Element) & (xmlTextReader.Name == "Notes"))
                                        {
                                            InNotes = xmlTextReader.ReadElementContentAsString();
                                        }
#if DAO
										Database daoDb = DbDaoController.GetDaoDb(gf.ConnectStringMainDB);
										Recordset rs = null;
#elif SQLite
                                        DbConnection connection = DbController.GetDbConnection(gf.ConnectStringMainDB);
#endif
                                        while (xmlTextReader.Read())
                                        {
                                            switch (xmlTextReader.NodeType)
                                            {
                                                case XmlNodeType.Element:
                                                    switch (xmlTextReader.Name)
                                                    {
                                                        case "ItemID":
                                                            text2 = xmlTextReader.ReadElementContentAsString();
                                                            break;
                                                        case "Title1":
                                                            displayName = xmlTextReader.ReadElementContentAsString();
                                                            break;
                                                        case "Folder":
                                                            folderName = xmlTextReader.ReadElementContentAsString();
                                                            break;
                                                        case "FormatData":
                                                            formatString = xmlTextReader.ReadElementContentAsString();
                                                            break;
                                                    }
                                                    break;
                                                case XmlNodeType.EndElement:
                                                    if (text2 != "")
                                                    {
                                                        if (InMode == UsageMode.Worship)
                                                        {
#if DAO
															WriteItemtoWorshipList(daoDb, rs, DataUtil.Left(text2, 1), DataUtil.Right(text2, text2.Length - 1), displayName, folderName, formatString, -1);
#elif SQLite
                                                            WriteItemtoWorshipList(connection, DataUtil.Left(text2, 1), DataUtil.Right(text2, text2.Length - 1), displayName, folderName, formatString, -1);
#endif
                                                        }
                                                        else
                                                        {
#if DAO
															WriteItemtoPraiseBook(daoDb, rs, DataUtil.Left(text2, 1), DataUtil.Right(text2, text2.Length - 1), displayName, folderName);
#elif SQLite
                                                            WriteItemtoPraiseBook(connection, DataUtil.Left(text2, 1), DataUtil.Right(text2, text2.Length - 1), displayName, folderName);
#endif
                                                        }
                                                        text2 = "";
                                                        displayName = "";
                                                        folderName = "";
                                                        formatString = "";
                                                    }
                                                    break;
                                            }
                                        }
                                        xmlTextReader?.Close();
                                        goto IL_0417;
                                    }
                                    xmlTextReader?.Close();
                                }
                                else
                                {
                                    xmlTextReader?.Close();
                                }
                            }
                        }
                        goto IL_0454;
                    }
                }
                catch (Exception e)
                {
                    xmlTextReader?.Close();
                }
            }
            catch (Exception ex)
            {
            }
            if (InMode == UsageMode.Worship)
            {
                Load32WorshipList(DataType);
            }
            else
            {
                Load32PraiseBook(DataType);
            }
            return "";
        IL_0417:
            LoadIndexFilePostAction(InMode);
            return text;
        IL_0454:
            LoadIndexFilePostAction(InMode);
            return "";
        }

        private void Load32WorshipList(int DataType)
        {
            gf.TotalMusicFiles = -1;
            string inFileName = gf.WorshipDir + gf.CurSession + ".esw";
            gf.LoadFileContents(inFileName, ref InContents);
            int num = gf.Load32HeaderData(inFileName, InContents, ref gf.HeaderData);
            if (num < 1)
            {
                InContents = "";
            }
            switch (DataType)
            {
                case 2:
                    return;
                case 1:
                    gf.ApplyHeaderData();
                    UpdateDefaultFields();
                    return;
            }
            gf.ApplyHeaderData();
            WorshipListItems.Items.Clear();
            InContents = DataUtil.Mid(InContents, num + 1, InContents.Length - num);
            int num2 = 0;
            int num3 = InContents.IndexOf(">");
            try
            {

#if DAO
				Database daoDb = DbDaoController.GetDaoDb(gf.ConnectStringMainDB);
				Recordset recordset = null;
#elif SQLite
                DbConnection connection = DbController.GetDbConnection(gf.ConnectStringMainDB);
#endif
                while (num3 >= 0)
                {
                    string text = DataUtil.Trim(DataUtil.Mid(InContents, num2, num3 - num2));
                    int num4 = 0;
                    gf.SongFormatData = "";
                    num4 = text.IndexOf('*');
                    if (num4 >= 0)
                    {
                        gf.SongFormatData = DataUtil.Trim(DataUtil.Right(text, text.Length - num4 - 1));
                        text = DataUtil.Trim(DataUtil.Left(text, num4));
                    }
                    if (text != "")
                    {
                        int num5 = text.IndexOf("\\");
                        string fNum_ID = DataUtil.Mid(text, 1, num5 - 1);
                        int num6 = text.IndexOf("\\", num5 + 1);
                        int num7 = num6;
                        while (num7 >= 0)
                        {
                            num7 = text.IndexOf("\\", num7 + 1);
                            if (num7 >= 0)
                            {
                                num6 = num7;
                            }
                        }
                        if (num6 < 0)
                        {
                            num6 = text.Length + 1;
                        }
                        string displayName = DataUtil.Mid(text, num5 + 1, num6 - (num5 + 1));
                        string inSym = DataUtil.Left(text, 1);
                        gf.WorshipListIDOK = true;
                        DataUtil.Convertv32FormatString(ref gf.SongFormatData, '*');
#if DAO
						WriteItemtoWorshipList(daoDb, recordset, inSym, fNum_ID, displayName, "", gf.SongFormatData, -1);
#elif SQLite
                        WriteItemtoWorshipList(connection, inSym, fNum_ID, displayName, "", gf.SongFormatData, -1);
#endif

                    }
                    text = "";
                    num2 = num3 + 1;
                    num3 = InContents.IndexOf(">", num2);
                }
#if DAO
				if (recordset != null)
				{
					recordset.Close();
					recordset = null;
				}
#endif
            }
            catch
            {
            }
            LoadIndexFilePostAction(UsageMode.Worship);
        }

        private void LoadIndexFilePostAction(UsageMode InMode)
        {
            if (InMode == UsageMode.Worship)
            {
                SetMainDefaultBackScreen();
                SetWorshipPraiseListColWidth();
                UpdateDefaultFields();
                UpdateDisplayPanelFields();
                UpdateDisplayPanelData(RefreshSlides: true);
                if ((WorshipListItems.Items.Count > 0) & (gf.PreviewItem.Source == ItemSource.WorshipList))
                {
                    WorshipListItems.Items[0].Selected = true;
                    WorshipListIndexChanged();
                }
                else
                {
                    WorshipListIndexChanged();
                }
            }
            else
            {
                SetSortButtonPB(gf.PB_CJKGroupStyle);
                if (PraiseBookItems.Items.Count > 0)
                {
                    PraiseBookItems.Items[0].Selected = true;
                }
                SavePraiseBook();
            }
            Cursor = Cursors.Default;
        }

        private bool InsertIndexFileItems(string InFileName, ref ListView InList, int AddToLocation, ref string InNotes)
        {
            try
            {
                XmlTextReader xmlTextReader = new XmlTextReader(InFileName);
                try
                {
                    bool flag = false;
                    bool flag2 = false;
                    bool flag3 = false;
                    xmlTextReader.Read();
                    while (xmlTextReader.Read() && !flag)
                    {
                        if ((xmlTextReader.NodeType == XmlNodeType.Element) & (xmlTextReader.Name == "EasiSlides"))
                        {
                            flag = true;
                        }
                    }
                    if (flag)
                    {
                        while (xmlTextReader.Read() && !flag2)
                        {
                            if ((xmlTextReader.NodeType == XmlNodeType.Element) & (xmlTextReader.Name == "ListItem"))
                            {
                                flag2 = true;
                            }
                        }
                        if (!flag2)
                        {
                            xmlTextReader?.Close();
                        }
                        else
                        {
                            while (xmlTextReader.Read() && !flag3)
                            {
                                if ((xmlTextReader.NodeType == XmlNodeType.Element) & (xmlTextReader.Name == "ListHeader"))
                                {
                                    flag3 = true;
                                }
                            }
                            if (!flag3)
                            {
                                xmlTextReader?.Close();
                            }
                            else
                            {
                                string text = "";
                                string displayName = "";
                                string folderName = "";
                                string formatString = "";
                                xmlTextReader.Read();
                                gf.WorshipListIDOK = false;
                                if ((xmlTextReader.NodeType == XmlNodeType.Element) & (xmlTextReader.Name == "SystemID"))
                                {
                                    if (gf.SystemID == xmlTextReader.ReadElementContentAsString())
                                    {
                                        gf.WorshipListIDOK = true;
                                    }
                                    xmlTextReader.Read();
                                    if ((xmlTextReader.NodeType == XmlNodeType.Element) & (xmlTextReader.Name == "FormatData"))
                                    {
                                        string text2 = xmlTextReader.ReadElementContentAsString();
                                        xmlTextReader.Read();
                                        if ((xmlTextReader.NodeType == XmlNodeType.Element) & (xmlTextReader.Name == "Notes"))
                                        {
                                            InNotes += xmlTextReader.ReadElementContentAsString();
                                        }
#if DAO
										Database daoDb = DbDaoController.GetDaoDb(gf.ConnectStringMainDB);
										Recordset recordset = null;
#elif SQLite
                                        DbConnection connection = DbController.GetDbConnection(gf.ConnectStringMainDB);
#endif
                                        while (xmlTextReader.Read())
                                        {
                                            switch (xmlTextReader.NodeType)
                                            {
                                                case XmlNodeType.Element:
                                                    switch (xmlTextReader.Name)
                                                    {
                                                        case "ItemID":
                                                            text = xmlTextReader.ReadElementContentAsString();
                                                            break;
                                                        case "Title1":
                                                            displayName = xmlTextReader.ReadElementContentAsString();
                                                            break;
                                                        case "Folder":
                                                            folderName = xmlTextReader.ReadElementContentAsString();
                                                            break;
                                                        case "FormatData":
                                                            formatString = xmlTextReader.ReadElementContentAsString();
                                                            break;
                                                    }
                                                    break;
                                                case XmlNodeType.EndElement:
                                                    if (text != "")
                                                    {
#if DAO
														WriteItemtoWorshipList(daoDb, rs, DataUtil.Left(text, 1), DataUtil.Right(text, text.Length - 1), displayName, folderName, formatString, AddToLocation);
#elif SQLite
                                                        WriteItemtoWorshipList(connection, DataUtil.Left(text, 1), DataUtil.Right(text, text.Length - 1), displayName, folderName, formatString, AddToLocation);
#endif
                                                        text = "";
                                                        displayName = "";
                                                        folderName = "";
                                                        formatString = "";
                                                        AddToLocation++;
                                                    }
                                                    break;
                                            }
                                        }
                                        xmlTextReader?.Close();
                                        goto IL_0360;
                                    }
                                    xmlTextReader?.Close();
                                }
                                else
                                {
                                    xmlTextReader?.Close();
                                }
                            }
                        }
                    }
                }
                catch
                {
                    xmlTextReader?.Close();
                }
            }
            catch
            {
            }
            return false;
        IL_0360:
            return true;
        }

#if DAO
		private void WriteItemtoWorshipList(Database db, Recordset rs, string InSym, string FNum_ID, string DisplayName1, string FolderName, string FormatString, int AddToLocation)
		{
			if (FNum_ID == "")
			{
				return;
			}
			ListViewItem listViewItem = new ListViewItem();
			string musicTitle = "";
			string musicTitle2 = gf.GetDisplayNameOnly(ref DisplayName1, UpdateByRef: false);
			string text = "";
			string text2 = "";
			string text3 = "0";
			if (InSym == "D")
			{
				bool flag = false;
				try
				{
					string fullSearchString = (!gf.WorshipListIDOK) ? ("select * from SONG where LCase(Title_1) like \"" + DisplayName1.ToLower() + "\"  AND FolderNo = " + gf.GetFolderNumber(FolderName)) : ("select * from SONG where songid = " + FNum_ID + " AND FolderNo > 0 ");
					rs = DbDaoController.GetRecordSet(db, fullSearchString);
					if (!(rs?.EOF ?? true))
					{
						rs.MoveFirst();
						if (DataUtil.GetDataInt(rs, "FolderNo") > 0 && gf.FolderUse[DataUtil.GetDataInt(rs, "FolderNo")] > 0)
						{
							DisplayName1 = DataUtil.GetDataString(rs, "Title_1");
							musicTitle = DataUtil.GetDataString(rs, "Title_2");
							text = DataUtil.GetDataString(rs, "LICENCE_ADMIN1");
							text2 = DataUtil.GetDataString(rs, "LICENCE_ADMIN2");
							FolderName = gf.FolderName[DataUtil.GetDataInt(rs, "FolderNo")];
							FNum_ID = "D" + DataUtil.GetDataInt(rs, "songid");
							text3 = ((DataUtil.GetDataString(rs, "song_number") != "") ? DataUtil.GetDataString(rs, "song_number") : "0");
							flag = true;
						}
					}
					rs?.Close();
					if (!flag)
					{
						DisplayName1 += " <Error - Item Not Found>";
						FNum_ID = "D0";
					}
				}
				catch
				{
					FNum_ID = "D0";
					DisplayName1 += " <Error - Item Not Found>";
				}
			}
			else if (InSym == "P")
			{
				FNum_ID = "P" + DisplayName1;
				gf.GetDisplayNameOnly(ref DisplayName1, UpdateByRef: true);
			}
			else if (InSym == "B")
			{
				FNum_ID = "B" + FNum_ID;
			}
			else if (InSym == "T")
			{
				FNum_ID = "T" + DisplayName1;
				gf.GetDisplayNameOnly(ref DisplayName1, UpdateByRef: true);
			}
			else if (InSym == "I")
			{
				FNum_ID = "I" + DisplayName1;
				string InTitle = "";
				gf.LoadIndividualData(ref gf.TempItem1, FNum_ID, "", 1, ref InTitle);
				musicTitle2 = gf.TempItem1.Title;
				musicTitle = gf.TempItem1.Title2;
				gf.GetDisplayNameOnly(ref DisplayName1, UpdateByRef: true);
			}
			else if (InSym == "W")
			{
				FNum_ID = "W" + DisplayName1;
				gf.GetDisplayNameOnly(ref DisplayName1, UpdateByRef: true);
			}
			else if (InSym == "M")
			{
				FNum_ID = "M" + DisplayName1;
				gf.GetDisplayNameOnly(ref DisplayName1, UpdateByRef: true);
			}
			if (DisplayName1 != "")
			{
				if (gf.MusicFound(musicTitle2, musicTitle))
				{
					DisplayName1 += " <#>";
				}
				if (AddToLocation < 0)
				{
					listViewItem = WorshipListItems.Items.Add(DisplayName1);
				}
				else
				{
					try
					{
						listViewItem = WorshipListItems.Items.Insert(AddToLocation, DisplayName1);
					}
					catch
					{
						listViewItem = WorshipListItems.Items.Add(DisplayName1);
					}
				}
				if (InSym == "D")
				{
					listViewItem.ImageIndex = 0;
				}
				else if (InSym == "P")
				{
					listViewItem.ImageIndex = 2;
				}
				else if (InSym == "B")
				{
					listViewItem.ImageIndex = 4;
				}
				else if (InSym == "T")
				{
					listViewItem.ImageIndex = 6;
				}
				else if (InSym == "I")
				{
					listViewItem.ImageIndex = 8;
				}
				else if (InSym == "W")
				{
					listViewItem.ImageIndex = 10;
				}
				else if (InSym == "M")
				{
					listViewItem.ImageIndex = 28;
				}
				listViewItem.SubItems.Add(FNum_ID);
				listViewItem.SubItems.Add(FormatString);
				listViewItem.SubItems.Add(text3);
				listViewItem.SubItems.Add(text);
				listViewItem.SubItems.Add(text2);
				listViewItem.SubItems.Add("");
				listViewItem.SubItems.Add(FolderName);
			}
		}
#elif SQLite
        private void WriteItemtoWorshipList(DbConnection connection, string InSym, string FNum_ID, string DisplayName1, string FolderName, string FormatString, int AddToLocation)
        {
            if (FNum_ID == "")
            {
                return;
            }
            ListViewItem listViewItem = new ListViewItem();
            string musicTitle = "";
            string musicTitle2 = gf.GetDisplayNameOnly(ref DisplayName1, UpdateByRef: false);
            string text = "";
            string text2 = "";
            string text3 = "0";
            if (InSym == "D")
            {
                bool flag = false;
                try
                {
                    //SQLite LCase()  -> lower() ??��??UCase() -> upper()
                    //string fullSearchString = (!gf.WorshipListIDOK) ? ("select * from SONG where LCase(Title_1) like \"" + DisplayName1.ToLower() + "\"  AND FolderNo = " + gf.GetFolderNumber(FolderName)) : ("select * from SONG where songid = " + FNum_ID + " AND FolderNo > 0 ");
                    string fullSearchString = (!gf.WorshipListIDOK) ? ("select * from SONG where lower(Title_1) like \"" + DisplayName1.ToLower() + "\"  AND FolderNo = " + gf.GetFolderNumber(FolderName)) : ("select * from SONG where songid = " + FNum_ID + " AND FolderNo > 0 ");

                    DataRow dr = DbController.GetDataRowScalar(connection, fullSearchString);
                    if (dr != null)
                    {
                        if (DataUtil.GetDataInt(dr, "FolderNo") > 0 && gf.FolderUse[DataUtil.GetDataInt(dr, "FolderNo")] > 0)
                        {
                            DisplayName1 = DataUtil.GetDataString(dr, "Title_1");
                            musicTitle = DataUtil.GetDataString(dr, "Title_2");
                            text = DataUtil.GetDataString(dr, "LICENCE_ADMIN1");
                            text2 = DataUtil.GetDataString(dr, "LICENCE_ADMIN2");
                            FolderName = gf.FolderName[DataUtil.GetDataInt(dr, "FolderNo")];
                            FNum_ID = "D" + DataUtil.GetDataInt(dr, "songid");
                            text3 = ((DataUtil.GetDataString(dr, "song_number") != "") ? DataUtil.GetDataString(dr, "song_number") : "0");
                            flag = true;
                        }
                    }

                    if (!flag)
                    {
                        DisplayName1 += " <Error - Item Not Found>";
                        FNum_ID = "D0";
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Console.WriteLine(ex.StackTrace);
                    FNum_ID = "D0";
                    DisplayName1 += " <Error - Item Not Found>";
                }
            }
            else if (InSym == "P")
            {
                FNum_ID = "P" + DisplayName1;
                gf.GetDisplayNameOnly(ref DisplayName1, UpdateByRef: true);
            }
            else if (InSym == "B")
            {
                FNum_ID = "B" + FNum_ID;
            }
            else if (InSym == "T")
            {
                FNum_ID = "T" + DisplayName1;
                gf.GetDisplayNameOnly(ref DisplayName1, UpdateByRef: true);
            }
            else if (InSym == "I")
            {
                FNum_ID = "I" + DisplayName1;
                string InTitle = "";
                gf.LoadIndividualData(ref gf.TempItem1, FNum_ID, "", 1, ref InTitle);
                musicTitle2 = gf.TempItem1.Title;
                musicTitle = gf.TempItem1.Title2;
                gf.GetDisplayNameOnly(ref DisplayName1, UpdateByRef: true);
            }
            else if (InSym == "W")
            {
                FNum_ID = "W" + DisplayName1;
                gf.GetDisplayNameOnly(ref DisplayName1, UpdateByRef: true);
            }
            else if (InSym == "M")
            {
                FNum_ID = "M" + DisplayName1;
                gf.GetDisplayNameOnly(ref DisplayName1, UpdateByRef: true);
            }
            if (DisplayName1 != "")
            {
                if (gf.MusicFound(musicTitle2, musicTitle))
                {
                    DisplayName1 += " <#>";
                }
                if (AddToLocation < 0)
                {
                    listViewItem = WorshipListItems.Items.Add(DisplayName1);
                }
                else
                {
                    try
                    {
                        listViewItem = WorshipListItems.Items.Insert(AddToLocation, DisplayName1);
                    }
                    catch
                    {
                        listViewItem = WorshipListItems.Items.Add(DisplayName1);
                    }
                }
                if (InSym == "D")
                {
                    listViewItem.ImageIndex = 0;
                }
                else if (InSym == "P")
                {
                    listViewItem.ImageIndex = 2;
                }
                else if (InSym == "B")
                {
                    listViewItem.ImageIndex = 4;
                }
                else if (InSym == "T")
                {
                    listViewItem.ImageIndex = 6;
                }
                else if (InSym == "I")
                {
                    listViewItem.ImageIndex = 8;
                }
                else if (InSym == "W")
                {
                    listViewItem.ImageIndex = 10;
                }
                else if (InSym == "M")
                {
                    listViewItem.ImageIndex = 28;
                }
                listViewItem.SubItems.Add(FNum_ID);
                listViewItem.SubItems.Add(FormatString);
                listViewItem.SubItems.Add(text3);
                listViewItem.SubItems.Add(text);
                listViewItem.SubItems.Add(text2);
                listViewItem.SubItems.Add("");
                listViewItem.SubItems.Add(FolderName);
            }
        }
#endif

        private void SetMainDefaultBackScreen()
        {
            gf.SetDefaultBackScreen(ref PreviewScreen);
            gf.SetDefaultBackScreen(ref OutputScreen);
            if (gf.ShowRunning)
            {
                RemoteControlLiveShow(LiveShowAction.Remote_DefaultBackgroundChanged);
            }
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

        /// <summary>
        /// daniel
        /// </summary>
        private void WorshipListIndexChanged()
        {
            LoadThumbPreviewlockkey = 0;
            WorshipListIndexChanged(0);
        }

        private void WorshipListIndexChanged(int StartingSlide)
        {
            WorshipListIndexChanged(StartingSlide, GetFirstItem: false);
        }

        static int preSelectedItemNum = -1;

        private void WorshipListIndexChanged(int StartingSlide, bool GetFirstItem)
        {
            gf.PreviewItem.Source = ItemSource.WorshipList;
            gf.TotalWorshipListItems = WorshipListItems.Items.Count;
            int num;
            if (GetFirstItem & (gf.TotalWorshipListItems > 0))
            {
                WorshipListItems.Items[0].Selected = true;
                num = 0;
            }
            else
            {
                num = gf.GetSelectedIndex(WorshipListItems);
            }

            if (num >= 0)
            {
                //??�� ��????????���� ü? ??��??
                string InTitle = WorshipListItems.Items[num].SubItems[0].Text;
                string text = WorshipListItems.Items[num].SubItems[1].Text;
                gf.PreviewItem.InMainItemText = InTitle;
                gf.PreviewItem.InSubItemItem1Text = text;
                gf.PreviewItem.CurItemNo = num + 1;
                gf.PreviewItem.TotalItems = WorshipListItems.Items.Count;

                string filePrefix = gf.SetPowerpointPreviewPrefix1(gf.PreviewItem);

                // ✅ 캐싱 최적화: 파일이 빌드되지 않았거나 다른 항목으로 변경된 경우에만 로드
                if (!gf.PreviewPPT.IsBuildedFileCheck(gf.PreviewItem.Path, filePrefix, ref gf.PreviewItem.TotalSlides) || preSelectedItemNum != num)
                {
                    LoadItem(ref gf.PreviewItem, text, WorshipListItems.Items[num].SubItems[2].Text, StartingSlide, ref InTitle, ScrollToCaret: true);
                    UpdateDisplayPanelFields();
                }

                preSelectedItemNum = num;
            }
            else
            {
                gf.InitialiseIndividualData(ref gf.PreviewItem);
                gf.LoadIndividualFormatData(ref gf.PreviewItem, "");
                AllowIndividualFormat(AllowFormat: false);
                NoIndividualFormat();
                BuildVerseButtons(gf.PreviewItem, Reset: true);
                gf.PreviewItem.Format.ShowSlideTransition = 0;
                gf.PreviewItem.Format.ShowItemTransition = 0;

                ResetMainPictureBox(ref gf.PreviewItem);
                ClearLyrics(ref flowLayoutPreviewLyrics);
                UpdateDisplayPanelFields();
            }
        }

        private void SaveWorshipList()
        {
            SaveWorshipList(PreloadPowerpoint: false);
        }

        private void SaveWorshipList(bool PreloadPowerpoint)
        {
            if (!UpdatingFormatFields)
            {
                gf.CurSession = SessionList.Text;
                for (int i = 1; i <= WorshipListItems.Items.Count; i++)
                {
                    gf.WorshipSongs[i, 2] = gf.RemoveMusicSym(DataUtil.Trim(WorshipListItems.Items[i - 1].Text));
                    gf.WorshipSongs[i, 0] = DataUtil.Trim(WorshipListItems.Items[i - 1].SubItems[1].Text);
                    gf.WorshipSongs[i, 1] = DataUtil.Left(gf.WorshipSongs[i, 0], 1);
                    gf.WorshipSongs[i, 4] = DataUtil.Trim(WorshipListItems.Items[i - 1].SubItems[2].Text);
                }
                gf.TotalWorshipListItems = WorshipListItems.Items.Count;
                gf.SaveIndexFile(gf.WorshipDir + gf.CurSession + ".esw", ref WorshipListItems, UsageMode.Worship, SaveAllItems: true, "", gf.CurSessionNotes);
                if (PreloadPowerpoint)
                {
                    gf.PreLoadPowerpointFiles(ref gf.LivePP, ref gf.WorshipSongs);
                }
            }
        }

        private void LoadItem(ref SongSettings InItem, string InIDString)
        {
            LoadItem(ref InItem, InIDString, "", 1);
        }

        private void LoadItem(ref SongSettings InItem, string InIDString, string InFormatString, int StartingSlide)
        {
            string InTitle = "";
            LoadItem(ref InItem, InIDString, InFormatString, StartingSlide, ref InTitle, ScrollToCaret: true);
        }

        /// <summary>
        /// daniel
        /// </summary>
        /// <param name="InItem"></param>
        /// <param name="InIDString"></param>
        /// <param name="InFormatString"></param>
        /// <param name="StartingSlide"></param>
        /// <param name="InTitle"></param>
        /// <param name="ScrollToCaret"></param>
        private void LoadItem(ref SongSettings InItem, string InIDString, string InFormatString, int StartingSlide, ref string InTitle, bool ScrollToCaret)
        {
            string prevTitle = "";
            string nextTitle = "";
            int num = gf.StartPresAt;
            if (WorshipListItems.Items.Count > 0)
            {
                int num2 = -1;
                int num3 = -1;
                if (!InItem.OutputStyleScreen)
                {
                    num = gf.GetSelectedIndex(WorshipListItems) + 1;
                }
                if (InItem.CurItemNo == 0)
                {
                    num2 = num - 1;
                    num3 = num;
                }
                else
                {
                    num2 = num - 2;
                    num3 = num;
                }
                if (num2 < 0 && InItem.CurItemNo == 0)
                {
                    num2 = 0;
                }
                if (num3 >= WorshipListItems.Items.Count)
                {
                    num3 = ((InItem.CurItemNo != 0) ? (-1) : (WorshipListItems.Items.Count - 1));
                }
                if (num2 == num3 && num2 == 0)
                {
                    num2 = -1;
                }
                prevTitle = ((num2 >= 0) ? gf.RemoveMusicSym(WorshipListItems.Items[num2].SubItems[0].Text) : "");
                nextTitle = ((num3 >= 0) ? gf.RemoveMusicSym(WorshipListItems.Items[num3].SubItems[0].Text) : "");
            }
            string text = DataUtil.Left(InIDString, 1);
            bool flag = false;
            gf.InitialiseIndividualData(ref InItem, GapMedia.None, text);
            InItem.PrevTitle = prevTitle;
            InItem.NextTitle = nextTitle;
            MakePowerpointPreviewVisible(InItem, (text == "P") ? true : false);
            if (text == "P")
            {
                gf.LoadIndividualData(ref InItem, InIDString, "", StartingSlide);
                ValidatePowerpointItem(InItem);
                gf.LoadIndividualFormatData(ref InItem, "");
                AllowIndividualFormat(AllowFormat: false);
                SetItemFontSettings(ref InItem);
                if (!InItem.OutputStyleScreen)
                {
                    UpdateFormatFields();
                }
                BuildAllPowerpointScreenDumps(ref InItem);
                InItem.CurSlide = StartingSlide;
                InItem.CurSlide = ((InItem.CurSlide < 1) ? 1 : ((InItem.CurSlide > InItem.TotalSlides) ? InItem.TotalSlides : InItem.CurSlide));
                if (InItem.OutputStyleScreen)
                {
                    ShowOutputPPThumbs(InItem.CurSlide);
                }
                else
                {
                    ShowPreviewPPThumbs(InItem.CurSlide);
                }
                InItem.Format.ShowItemTransition = 0;
                InItem.Format.ShowSlideTransition = 0;
                if (gf.ShowRunning & InItem.OutputStyleScreen)
                {
                    InItem.TotalSlides = gf.RunPowerpointSong(ref InItem, ref MainPPT, StartingSlide, ShowResult: true);
                    if (gf.DualMonitorMode)
                    {
                        ShowDualMonitorPP_Preview(ref InItem);
                    }
                }
                else
                {
                    PP_Preview(ref InItem);
                }
                ShowStatusBarSummary();
                BuildVerseButtons(InItem);
                DisplaySettingsLabel(InItem);
            }
            else
            {
                int num4;
                switch (text)
                {
                    default:
                        num4 = ((!(text == "G")) ? 1 : 0);
                        break;
                    case "D":
                    case "B":
                    case "T":
                    case "I":
                    case "W":
                    case "M":
                        num4 = 0;
                        break;
                }
                if (num4 == 0)
                {
                    Cursor.Current = Cursors.WaitCursor;
                    gf.LoadIndividualData(ref InItem, InIDString, "", StartingSlide, ref InTitle);
                    if (InItem.Source == ItemSource.SongsList)
                    {
                        if (!InItem.OutputStyleScreen)
                        {
                            InFormatString = InItem.Format.DBStoredFormat;
                        }
                    }
                    else if (text == "I" || text == "G")
                    {
                        InFormatString = InItem.Format.FormatString;
                    }
                    gf.LoadIndividualFormatData(ref InItem, InFormatString);
                    SetItemFontSettings(ref InItem);
                    gf.FormatDisplayLyrics(ref InItem, PrepareSlides: true, UseStoredSequence: true);
                    if (InItem.OutputStyleScreen)
                    {
                        BuildVerseButtons(gf.OutputItem);
                        DisplayLyrics(gf.OutputItem, StartingSlide);
                    }
                    else
                    {
                        AllowIndividualFormat(AllowFormat: true, (!(InFormatString == "")) ? true : false);
                        UpdateFormatFields();
                        BuildVerseButtons(InItem);
                        DisplayLyrics(InItem, StartingSlide, ScrollToCaret);
                    }
                }
                else
                {
                    gf.LoadIndividualData(ref InItem, InIDString, "", StartingSlide, ref InTitle);
                    AllowIndividualFormat(AllowFormat: true, (!(InFormatString == "")) ? true : false);
                    if (InFormatString != "")
                    {
                        ApplyIndividualFormat(ref InItem);
                    }
                    UpdateFormatFields();
                    BuildVerseButtons(InItem);
                    DisplayLyrics(InItem, StartingSlide);
                }
            }
            if (InItem.OutputStyleScreen)
            {
                PostitionBlackClearGapLabels();
                return;
            }
            DisplayItemInfo(InItem, ref PreviewInfo);
            ShowStatusBarSummary();
        }

        private void DisplayItemInfo(SongSettings InItem, ref RichTextBox InTextBox)
        {
            InTextBox.Text = "";
            InTextBox.SelectionStart = 0;
            string text = "";
            if (InItem.Type == "P" || InItem.Type == "W" || InItem.Type == "T" || InItem.Type == "I")
            {
                string itemID = InItem.ItemID;
                RichTextBox obj = InTextBox;
                obj.Text = obj.Text + InItem.ItemID + "\n";
            }
            if (InItem.Title2 != "")
            {
                RichTextBox obj2 = InTextBox;
                obj2.Text = obj2.Text + "Title2: " + InItem.Title2 + "\n";
            }
            if (InItem.Format.BackgroundPicture != "" && InItem.Type != "P")
            {
                if (InTextBox.Text != "")
                {
                    InTextBox.Text += "\n";
                }
                RichTextBox obj3 = InTextBox;
                obj3.Text = obj3.Text + "(Image: " + InItem.Format.BackgroundPicture + ")\n";
            }
            text = ((InItem.Format.MediaOption == 1 || InItem.Format.MediaOption == 2) ? gf.GetMediaLocation(InItem) : "");
            if (text != "")
            {
                RichTextBox obj4 = InTextBox;
                obj4.Text = obj4.Text + "(Media: " + text + ")\n";
            }
            if (InItem.Writer != "")
            {
                RichTextBox obj5 = InTextBox;
                obj5.Text = obj5.Text + "Writer: " + InItem.Writer + "\n";
            }
            if (InItem.Copyright != "")
            {
                RichTextBox obj6 = InTextBox;
                obj6.Text = obj6.Text + "Copyright: " + InItem.Copyright + "\n";
            }
            if (InItem.Capo > 0 || InItem.MusicKey != "" || InItem.Timing != "")
            {
                KeyCapoText = ((InItem.MusicKey != "") ? ("Key: " + InItem.MusicKey + " ") : "") + ((InItem.Capo > 0) ? (" Capo " + Convert.ToString(InItem.Capo) + " ") : "") + ((InItem.Timing != "") ? (" (" + InItem.Timing + ")") : "");
                RichTextBox obj7 = InTextBox;
                obj7.Text = obj7.Text + KeyCapoText + "\n";
            }
            else
            {
                KeyCapoText = "";
            }
            if (InItem.Book_Reference != "")
            {
                RichTextBox obj8 = InTextBox;
                obj8.Text = obj8.Text + "Book Ref: " + InItem.Book_Reference + "\n";
            }
            if (InItem.User_Reference != "")
            {
                RichTextBox obj9 = InTextBox;
                obj9.Text = obj9.Text + "User Ref: " + InItem.User_Reference + "\n";
            }
        }

        private bool ValidatePowerpointItem(SongSettings InItem)
        {
            if (File.Exists(InItem.Path))
            {
                return true;
            }
            MessageBox.Show("Sorry - Can't find the Powerpoint File '" + InItem.Path + "'");
            InItem.Path = "";
            return false;
        }

        static string previwItem = "";
        static string OutputItem = "";

        private void BuildAllPowerpointScreenDumps(ref SongSettings InItem)
        {
            string filePrefix = gf.SetPowerpointPreviewPrefix1(InItem);

            if (InItem.OutputStyleScreen)
            {
                gf.OutputPPT.preViewEvent = new OfficeLib.PreviewEvent(FormatPowerPointThumbContainers2);

                // ✅ 중복 호출 방지: 같은 ItemID면 캐시 확인
                if (OutputItem != InItem.ItemID)
                {
                    OutputItem = InItem.ItemID;
                }
                else
                {
                    if (gf.OutputPPT.IsBuildedFileCheck(InItem.Path, filePrefix, ref InItem.TotalSlides))
                    {
                        Console.WriteLine($"[Cache Hit] Output PPT already built: {InItem.ItemID}");
                        return;
                    }
                }
            }
            else
            {
                gf.PreviewPPT.preViewEvent = new OfficeLib.PreviewEvent(FormatPowerPointThumbContainers2);

                // ✅ 중복 호출 방지: 같은 ItemID면 캐시 확인
                if (previwItem != InItem.ItemID)
                {
                    previwItem = InItem.ItemID;
                }
                else
                {
                    if (gf.PreviewPPT.IsBuildedFileCheck(InItem.Path, filePrefix, ref InItem.TotalSlides))
                    {
                        Console.WriteLine($"[Cache Hit] Preview PPT already built: {InItem.ItemID}");
                        return;
                    }
                }
            }

            if (InItem.OutputStyleScreen)
            {
                if (gf.OutputPPT.BuildScreenOutDumps(InItem.Path, filePrefix, ref InItem.TotalSlides, 9, 1000, ref InItem.SongVerses, ref InItem.Slide, gf.SequenceSymbol))
                {
                    FormatPowerPointThumbContainers1(ref Powerpoint_OutputCanvas, ref flowLayoutOutputPowerPoint, InItem.TotalSlides);
                }
            }
            else
            {
                // ✅ 동기 방식 유지 (PowerPoint COM 객체는 STA 스레드에서만 안전)
                // 캐싱 및 이미지 최적화로 성능 개선
                if (gf.PreviewPPT.BuildScreenPreDumps(InItem.Path, filePrefix, ref InItem.TotalSlides, 9, 1000, ref InItem.SongVerses, ref InItem.Slide, gf.SequenceSymbol))
                {
                    FormatPowerPointThumbContainers1(ref Powerpoint_PreviewCanvas, ref flowLayoutPreviewPowerPoint, InItem.TotalSlides);
                }
            }
        }

        private void PP_Preview(ref SongSettings InItem)
        {
            Cursor = Cursors.WaitCursor;
            InItem.Format.BackgroundPicture = (InItem.OutputStyleScreen ? gf.OUTPPFullPath : gf.PREPPFullPath) + InItem.CurSlide + ".jpg";
            InItem.Format.BackgroundMode = ImageMode.BestFit;
            InItem.UseDefaultFormat = false;
            if (InItem.OutputStyleScreen)
            {
                gf.SetShowBackground(InItem, ref OutputScreen);
            }
            else
            {
                gf.SetShowBackground(InItem, ref PreviewScreen);
            }
            SetItemFontSettings(ref InItem);
            gf.DrawText(ref InItem, ref PreviewScreen, ref OutputScreen, InItem.LyricsAndNotationsList);
            ShowStatusBarSummary();
            DisplaySettingsLabel(InItem);
            Cursor = Cursors.Default;
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
                // ���� ??�� ????�� ����
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

        private void DisplayLyrics(SongSettings InItem, int StartingSlide)
        {
            DisplayLyrics(InItem, StartingSlide, ImageTransitionControl.TransitionAction.AsStored);
        }

        private void DisplayLyrics(SongSettings InItem, int StartingSlide, ImageTransitionControl.TransitionAction TransitionAction)
        {
            DisplayLyrics(InItem, StartingSlide, ScrollToCaret: true, 2, TransitionAction);
        }

        private void DisplayLyrics(SongSettings InItem, int StartingSlide, bool ScrollToCaret)
        {
            DisplayLyrics(InItem, StartingSlide, ScrollToCaret, 2, ImageTransitionControl.TransitionAction.AsStored);
        }

        private void DisplayLyrics(SongSettings InItem, int StartingSlide, bool ScrollToCaret, int GapItemBackground)
        {
            DisplayLyrics(InItem, StartingSlide, ScrollToCaret, GapItemBackground, ImageTransitionControl.TransitionAction.AsStored);
        }

        private void DisplayLyrics(SongSettings InItem, int StartingSlide, bool ScrollToCaret, int GapItemBackground, ImageTransitionControl.TransitionAction TransitionAction)
        {
            if (InitFormLoad)
            {
                return;
            }
            bool flag = true;
            DisplaySettingsLabel(InItem);
            if (StartingSlide > 0)
            {
                TransitionAction = ImageTransitionControl.TransitionAction.None;
            }
            if (InItem.Type == "P")
            {
                ResetMainPictureBox(ref InItem);
            }
            else if (InItem.Type == "D")
            {
                if (gf.EasiSlidesMode == UsageMode.Worship)
                {
                    InItem.CurSlide = StartingSlide;
                    ShowSlide(ref InItem, TransitionAction);
                }
                else
                {
                    gf.FormatDisplayLyrics(ref InItem, PrepareSlides: true, UseStoredSequence: true);
                }
            }
            else if (InItem.Type == "B" || InItem.Type == "T" || InItem.Type == "I" || InItem.Type == "W" || InItem.Type == "G")
            {
                if (gf.EasiSlidesMode == UsageMode.Worship)
                {
                    InItem.CurSlide = StartingSlide;
                    ShowSlide(ref InItem, TransitionAction);
                }
            }
            else if (InItem.Type == "M" || InItem.Type == "G")
            {
                if (gf.EasiSlidesMode == UsageMode.Worship)
                {
                    InItem.CurSlide = 1;
                    ShowSlide(ref InItem, TransitionAction);
                }
            }
            else
            {
                ResetMainPictureBox(ref InItem, GapType.Default, ImageTransitionControl.TransitionAction.None);
                if (InItem.OutputStyleScreen)
                {
                    ClearLyrics(ref flowLayoutOutputLyrics);
                }
                else
                {
                    ClearLyrics(ref flowLayoutPreviewLyrics);
                }
                flag = false;
            }
            if (flag)
            {
                FormatLyricsContainers(InItem);
            }
            ShowStatusBarSummary();
        }

        private void ResetMainPictureBox(ref SongSettings InItem)
        {
            GapType gapItemBackground = GapType.Default;
            ResetMainPictureBox(ref InItem, gapItemBackground, ImageTransitionControl.TransitionAction.None);
        }

        private void ResetMainPictureBox(ref SongSettings InItem, GapType GapItemBackground, ImageTransitionControl.TransitionAction TransitionAction)
        {
            if (InItem.OutputStyleScreen)
            {
                gf.ResetPictureBox(ref InItem, ref OutputScreen, GapItemBackground, TransitionAction);
            }
            else
            {
                gf.ResetPictureBox(ref InItem, ref PreviewScreen, GapItemBackground, TransitionAction);
            }
            DisplaySettingsLabel(InItem);
            ShowStatusBarSummary();
        }

        private bool ShowSlide(ref SongSettings InItem, ImageTransitionControl.TransitionAction TransitionAction)
        {
            return ShowSlide(ref InItem, TransitionAction, DoActiveIndicator: false);
        }

        private bool ShowSlide(ref SongSettings InItem, ImageTransitionControl.TransitionAction TransitionAction, bool DoActiveIndicator)
        {
            if (InItem.Type == "P")
            {
                if (InItem.OutputStyleScreen)
                {
                    if (gf.ShowRunning)
                    {
                        MainPPT.ImplementPowerpointSlideMovement(ref InItem.CurSlide, InItem.TotalSlides, OfficeLibKeys.None, InItem.CurSlide);
                        ShowDualMonitorPP_Preview(ref InItem);
                        if (InItem.CurSlide < 1)
                        {
                            InItem.CurSlide = InItem.TotalSlides;
                        }
                        ShowStatusBarSummary();
                    }
                    else
                    {
                        PP_Preview(ref InItem);
                    }
                }
                else
                {
                    PP_Preview(ref InItem);
                }
                return true;
            }
            InItem.TotalItems = WorshipListItems.Items.Count;
            if (TransitionAction == ImageTransitionControl.TransitionAction.AsStored)
            {
            }
            if (gf.ShowDBSlide(ref InItem, ref PreviewScreen, ref OutputScreen, DoActiveIndicator, TransitionAction))
            {
                return true;
            }
            ResetMainPictureBox(ref InItem);
            return false;
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

        private void SongsListIndexChanged()
        {
            SongsListIndexChanged(1);
        }

        private void SongsListIndexChanged(int StartingSlide)
        {
            SongsListIndexChanged(StartingSlide, ScrollToCaret: true);
        }

        private void SongsListIndexChanged(int StartingSlide, bool ScrollToCaret)
        {
            if (LastSelectedSongsListItem[0] == null)
            {
                LastSelectedSongsListItem[0] = "";
            }
            if (LastSelectedSongsListItem[1] == null)
            {
                LastSelectedSongsListItem[1] = "";
            }
            gf.PreviewItem.Source = ItemSource.SongsList;
            int selectedIndex = gf.GetSelectedIndex(SongsList);
            if (selectedIndex >= 0)
            {
                string text = SongsList.Items[selectedIndex].SubItems[1].Text;
                string InTitle = SongsList.Items[selectedIndex].SubItems[0].Text;
                gf.PreviewItem.InMainItemText = InTitle;
                gf.PreviewItem.InSubItemItem1Text = text;
                gf.PreviewItem.CurItemNo = 0;
                LoadItem(ref gf.PreviewItem, text, "", StartingSlide, ref InTitle, ScrollToCaret);
                UpdateDisplayPanelFields();
            }
            else
            {
                gf.PreviewItem.Type = "";
                gf.PreviewItem.Title = "";
                gf.PreviewItem.ItemID = "";
                gf.PreviewItem.CurItemNo = 0;
                gf.LoadIndividualFormatData(ref gf.PreviewItem, "");
                AllowIndividualFormat(AllowFormat: false);
                UpdateFormatFields();
                BuildVerseButtons(gf.PreviewItem);
                DisplayLyrics(gf.PreviewItem, 0, ScrollToCaret: true);
                UpdateDisplayPanelFields();
            }
        }

        private void EnableEditHistory()
        {
            gf.LoadRegistryMainEditHistory();
            UpdateMenuEditHistory();
        }

        private void AddToEditHistory(string InItemID)
        {
            if (!((gf.GetItemTitle(InItemID) == "") | (gf.MainEditHistoryList[1, 0] == InItemID)))
            {
                if (gf.TotalMainEditHistory < gf.MaxUserEditHistory)
                {
                    gf.TotalMainEditHistory++;
                }
                else
                {
                    gf.TotalMainEditHistory = gf.MaxUserEditHistory;
                }
                for (int num = gf.TotalMainEditHistory; num >= 2; num--)
                {
                    gf.MainEditHistoryList[num, 0] = gf.MainEditHistoryList[num - 1, 0];
                }
                gf.MainEditHistoryList[1, 0] = InItemID;
                gf.RemoveDuplicateEditorHistoryItems(ref gf.MainEditHistoryList, ref gf.TotalMainEditHistory);
                UpdateMenuEditHistory();
                gf.SaveMainEditHistoryToRegistry();
            }
        }

        private void UpdateMenuEditHistory()
        {
            try
            {
                int num = 0;
                string text = "";
                string text2 = "";
                for (int i = 1; i <= gf.TotalMainEditHistory; i++)
                {
                    text2 = gf.GetItemTitle(gf.MainEditHistoryList[i, 0]);
                    if (text2 != "" && gf.MainEditHistoryList[num, 0] != gf.MainEditHistoryList[i, 0])
                    {
                        num++;
                        gf.MainEditHistoryList[num, 0] = gf.MainEditHistoryList[i, 0];
                        gf.MainEditHistoryList[num, 1] = text2;
                    }
                }
                gf.TotalMainEditHistory = num;
                for (int i = gf.TotalMainEditHistory + 1; i <= gf.AbsoluteMaxHitoryItems; i++)
                {
                    gf.MainEditHistoryList[i, 0] = "";
                    gf.MainEditHistoryList[i, 1] = "";
                }
                for (int i = 1; i < gf.AbsoluteMaxHitoryItems; i++)
                {
                    Menu_EditHistoryList.DropDownItems[i - 1].Text = i + " " + gf.MainEditHistoryList[i, 1];
                    if (i > gf.TotalMainEditHistory)
                    {
                        Menu_EditHistoryList.DropDownItems[i - 1].Visible = false;
                    }
                    else
                    {
                        Menu_EditHistoryList.DropDownItems[i - 1].Visible = true;
                        text = DataUtil.Left(gf.MainEditHistoryList[i, 0], 1);
                        if (text == "D")
                        {
                            Menu_EditHistoryList.DropDownItems[i - 1].Image = Resources.ES_16;
                        }
                        else if (text == "P")
                        {
                            Menu_EditHistoryList.DropDownItems[i - 1].Image = Resources.PPImg;
                        }
                        else if (text == "B")
                        {
                            Menu_EditHistoryList.DropDownItems[i - 1].Image = Resources.Bible;
                        }
                        else if (text == "T")
                        {
                            Menu_EditHistoryList.DropDownItems[i - 1].Image = Resources.notebook;
                        }
                        else if (text == "I")
                        {
                            Menu_EditHistoryList.DropDownItems[i - 1].Image = Resources.Info_Icon;
                        }
                        else if (text == "W")
                        {
                            Menu_EditHistoryList.DropDownItems[i - 1].Image = Resources.word;
                        }
                        else
                        {
                            Menu_EditHistoryList.DropDownItems[i - 1].Image = null;
                        }
                    }
                }
            }
            catch
            {
            }
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

        private void ApplyBackground(int ImageTag, int InMode)
        {
            string directoryName = Path.GetDirectoryName(BackgroundImagename[ImageTag]);
            int backgroundImageMode = -1;
            if (directoryName == gf.RootEasiSlidesDir + "Images\\Scenery")
            {
                backgroundImageMode = 2;
            }
            else if (directoryName == gf.RootEasiSlidesDir + "Images\\Tiles")
            {
                backgroundImageMode = 0;
            }
            ApplyBackground(BackgroundImagename[ImageTag], InMode, backgroundImageMode);
            UpdateBackgroundImageButtons(InMode, backgroundImageMode);
        }

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

        private void ApplyBackground(string InImageFileName)
        {
            ApplyBackground(InImageFileName, 2, -1);
        }

        private void ApplyBackground(string InImageFileName, int InMode, int BackgroundImageMode)
        {
            if (InMode == 0)
            {
                gf.BackgroundPicture = InImageFileName;
                if (BackgroundImageMode >= 0 && BackgroundImageMode < 3)
                {
                    gf.BackgroundMode = (ImageMode)BackgroundImageMode;
                    gf.PreviewItem.Format.BackgroundMode = (ImageMode)BackgroundImageMode;
                    gf.OutputItem.Format.BackgroundMode = (ImageMode)BackgroundImageMode;
                }
                SaveWorshipList();
                WorshipListIndexChanged(gf.PreviewItem.CurSlide);
                gf.SetShowBackground(gf.OutputItem, ref OutputScreen);
                DisplayLyrics(gf.OutputItem, gf.OutputItem.CurSlide);
                if (gf.ShowRunning)
                {
                    RemoteControlLiveShow(LiveShowAction.Remote_BackgroundChanged);
                }
            }
            else if ((InMode == 1) | (InMode == 2))
            {
                if (gf.PreviewItem.Source == ItemSource.WorshipList)
                {
                    int selectedIndex = gf.GetSelectedIndex(WorshipListItems);
                    if (selectedIndex >= 0)
                    {
                        for (int i = 0; i <= WorshipListItems.Items.Count - 1; i++)
                        {
                            if (!WorshipListItems.Items[i].Selected)
                            {
                                continue;
                            }
                            if ((DataUtil.Left(WorshipListItems.Items[i].SubItems[1].Text, 1) == "D") | (DataUtil.Left(WorshipListItems.Items[i].SubItems[1].Text, 1) == "B") | (DataUtil.Left(WorshipListItems.Items[i].SubItems[1].Text, 1) == "T") | (DataUtil.Left(WorshipListItems.Items[i].SubItems[1].Text, 1) == "M") | (DataUtil.Left(WorshipListItems.Items[i].SubItems[1].Text, 1) == "W"))
                            {
                                gf.PreviewItem.Format.BackgroundPicture = InImageFileName;
                                if (BackgroundImageMode >= 0 && BackgroundImageMode < 3)
                                {
                                    gf.PreviewItem.Format.BackgroundMode = (ImageMode)BackgroundImageMode;
                                }
                                gf.PreviewItem.Format.FormatString = GetNewFormatString();
                                WorshipListItems.Items[i].SubItems[2].Text = gf.PreviewItem.Format.FormatString;
                            }
                            else if (DataUtil.Left(WorshipListItems.Items[i].SubItems[1].Text, 1) == "I")
                            {
                                gf.PreviewItem.Format.BackgroundPicture = InImageFileName;
                                if (BackgroundImageMode >= 0 && BackgroundImageMode < 3)
                                {
                                    gf.PreviewItem.Format.BackgroundMode = (ImageMode)BackgroundImageMode;
                                }
                                gf.PreviewItem.UseDefaultFormat = false;
                                gf.PreviewItem.Format.FormatString = GetNewFormatString();
                                gf.SetShowBackground(gf.PreviewItem, ref PreviewScreen);
                                gf.DrawText(ref gf.PreviewItem, ref PreviewScreen, gf.PreviewItem.LyricsAndNotationsList, DoActiveIndicator: false, ClearAll: false);
                                AllowIndividualFormat(AllowFormat: true, BoxChecked: true);
                                UpdateFormatFields();
                                SaveInfoFilePreview(ReloadImageData: true);
                            }
                        }
                        SaveWorshipList();
                        WorshipListIndexChanged(gf.PreviewItem.CurSlide);
                    }
                    else if (InMode == 2)
                    {
                        gf.BackgroundPicture = InImageFileName;
                        if (BackgroundImageMode >= 0 && BackgroundImageMode < 3)
                        {
                            gf.BackgroundMode = (ImageMode)BackgroundImageMode;
                            gf.OutputItem.Format.BackgroundMode = (ImageMode)BackgroundImageMode;
                        }
                        SaveWorshipList();
                        WorshipListIndexChanged(gf.PreviewItem.CurSlide);
                        gf.SetShowBackground(gf.OutputItem, ref OutputScreen);
                        DisplayLyrics(gf.OutputItem, gf.OutputItem.CurSlide, ImageTransitionControl.TransitionAction.None);
                        if (gf.ShowRunning)
                        {
                            RemoteControlLiveShow(LiveShowAction.Remote_BackgroundChanged);
                        }
                    }
                }
                else if (gf.PreviewItem.Source == ItemSource.SongsList)
                {
                    int selectedIndex = gf.GetSelectedIndex(SongsList);
                    if (selectedIndex >= 0)
                    {
                        gf.PreviewItem.Format.BackgroundPicture = InImageFileName;
                        if (BackgroundImageMode >= 0 && BackgroundImageMode < 3)
                        {
                            gf.PreviewItem.Format.BackgroundMode = (ImageMode)BackgroundImageMode;
                        }
                        gf.SaveFormatStringToDatabase(gf.PreviewItem.ItemID, GetNewFormatString());
                        SongsListIndexChanged(gf.PreviewItem.CurSlide);
                    }
                }
                else if (gf.PreviewItem.Source == ItemSource.HolyBible)
                {
                    if (gf.PreviewItem.CompleteLyrics != "")
                    {
                        gf.PreviewItem.Format.BackgroundPicture = InImageFileName;
                        if (BackgroundImageMode >= 0 && BackgroundImageMode < 3)
                        {
                            gf.PreviewItem.Format.BackgroundMode = (ImageMode)BackgroundImageMode;
                        }
                        gf.PreviewItem.UseDefaultFormat = false;
                        gf.PreviewItem.Format.FormatString = GetNewFormatString();
                        HB_CurSelectedFormat = gf.PreviewItem.Format.FormatString;
                        gf.SetShowBackground(gf.PreviewItem, ref PreviewScreen);
                        gf.DrawText(ref gf.PreviewItem, ref PreviewScreen, gf.PreviewItem.LyricsAndNotationsList, DoActiveIndicator: false, ClearAll: false);
                        AllowIndividualFormat(AllowFormat: true, BoxChecked: true);
                        UpdateFormatFields();
                    }
                }
                else if (gf.PreviewItem.Source == ItemSource.ExternalFileInfoScreen && gf.PreviewItem.CompleteLyrics != "")
                {
                    gf.PreviewItem.Format.BackgroundPicture = InImageFileName;
                    if (BackgroundImageMode >= 0 && BackgroundImageMode < 3)
                    {
                        gf.PreviewItem.Format.BackgroundMode = (ImageMode)BackgroundImageMode;
                    }
                    gf.PreviewItem.UseDefaultFormat = false;
                    gf.PreviewItem.Format.FormatString = GetNewFormatString();
                    gf.PreviewItem.Format.TempImageFileName = gf.PreviewItem.Format.BackgroundPicture;
                    gf.SetShowBackground(gf.PreviewItem, ref PreviewScreen);
                    gf.DrawText(ref gf.PreviewItem, ref PreviewScreen, gf.PreviewItem.LyricsAndNotationsList, DoActiveIndicator: false, ClearAll: false);
                    AllowIndividualFormat(AllowFormat: true, BoxChecked: true);
                    UpdateFormatFields();
                    SaveInfoFilePreview(ReloadImageData: true);
                }
            }
            Def_NoImage.Enabled = ((gf.BackgroundPicture != "") ? true : false);
            Ind_NoImage.Enabled = ((gf.PreviewItem.Format.BackgroundPicture != "") ? true : false);
            UpdateWorshipShowIcons();
            UpdateDefaultNoImageButton();
            if (gf.PreviewItem.Type == "D")
            {
                gf.SaveFormatStringToDatabase(gf.PreviewItem.ItemID, gf.PreviewItem.Format.FormatString);
            }
        }

        private void UpdateWorshipShowIcons()
        {
            if (ResetWorshipShowIcons())
            {
                if (gf.OutputItem.CurItemNo > 0)
                {
                    try
                    {
                        if (WorshipListItems.Items[gf.StartPresAt - 1].ImageIndex == 0 || WorshipListItems.Items[gf.StartPresAt - 1].ImageIndex == 2 || WorshipListItems.Items[gf.StartPresAt - 1].ImageIndex == 4 || WorshipListItems.Items[gf.StartPresAt - 1].ImageIndex == 6 || WorshipListItems.Items[gf.StartPresAt - 1].ImageIndex == 8 || WorshipListItems.Items[gf.StartPresAt - 1].ImageIndex == 28 || WorshipListItems.Items[gf.StartPresAt - 1].ImageIndex == 10)
                        {
                            WorshipListItems.Items[gf.StartPresAt - 1].ImageIndex = WorshipListItems.Items[gf.StartPresAt - 1].ImageIndex + 1;
                            WorshipListItems.Items[gf.StartPresAt - 1].SubItems[6].Text = "O";
                        }
                        WorshipListItems.Items[gf.StartPresAt - 1].ForeColor = Color.Red;
                        WorshipListItems.Items[gf.StartPresAt - 1].EnsureVisible();
                    }
                    catch
                    {
                    }
                }
                else if ((gf.StartPresAt > 0) & (gf.StartPresAt <= WorshipListItems.Items.Count))
                {
                    try
                    {
                        if (DataUtil.Left(WorshipListItems.Items[gf.StartPresAt - 1].SubItems[1].Text, 1) == "D")
                        {
                            WorshipListItems.Items[gf.StartPresAt - 1].ImageIndex = 0;
                        }
                        else if (DataUtil.Left(WorshipListItems.Items[gf.StartPresAt - 1].SubItems[1].Text, 1) == "P")
                        {
                            WorshipListItems.Items[gf.StartPresAt - 1].ImageIndex = 2;
                        }
                        else if (DataUtil.Left(WorshipListItems.Items[gf.StartPresAt - 1].SubItems[1].Text, 1) == "B")
                        {
                            WorshipListItems.Items[gf.StartPresAt - 1].ImageIndex = 4;
                        }
                        else if (DataUtil.Left(WorshipListItems.Items[gf.StartPresAt - 1].SubItems[1].Text, 1) == "T")
                        {
                            WorshipListItems.Items[gf.StartPresAt - 1].ImageIndex = 6;
                        }
                        else if (DataUtil.Left(WorshipListItems.Items[gf.StartPresAt - 1].SubItems[1].Text, 1) == "I")
                        {
                            WorshipListItems.Items[gf.StartPresAt - 1].ImageIndex = 8;
                        }
                        else if (DataUtil.Left(WorshipListItems.Items[gf.StartPresAt - 1].SubItems[1].Text, 1) == "W")
                        {
                            WorshipListItems.Items[gf.StartPresAt - 1].ImageIndex = 10;
                        }
                        else if (DataUtil.Left(WorshipListItems.Items[gf.StartPresAt - 1].SubItems[1].Text, 1) == "M")
                        {
                            WorshipListItems.Items[gf.StartPresAt - 1].ImageIndex = 28;
                        }
                        else
                        {
                            WorshipListItems.Items[gf.StartPresAt - 1].SubItems[6].Text = "";
                        }
                    }
                    catch
                    {
                    }
                }
            }
        }

        private bool ResetWorshipShowIcons()
        {
            if (WorshipListItems.Items.Count > 0)
            {
                for (int i = 0; i < WorshipListItems.Items.Count; i++)
                {
                    if (WorshipListItems.Items[i].ImageIndex == 1 || WorshipListItems.Items[i].ImageIndex == 3 || WorshipListItems.Items[i].ImageIndex == 5 || WorshipListItems.Items[i].ImageIndex == 7 || WorshipListItems.Items[i].ImageIndex == 9 || WorshipListItems.Items[i].ImageIndex == 29 || WorshipListItems.Items[i].ImageIndex == 11)
                    {
                        WorshipListItems.Items[i].ImageIndex = WorshipListItems.Items[i].ImageIndex - 1;
                    }
                    WorshipListItems.Items[i].SubItems[6].Text = "";
                    if (WorshipListItems.Items[i].ForeColor != SongsList.ForeColor)
                    {
                        WorshipListItems.Items[i].ForeColor = SongsList.ForeColor;
                    }
                }
                return true;
            }
            return false;
        }

        private void CopyPreviewToOutput()
        {
            gf.OutputItem.InMainItemText = gf.PreviewItem.InMainItemText;
            gf.OutputItem.InSubItemItem1Text = gf.PreviewItem.InSubItemItem1Text;
            gf.OutputItem.Source = gf.PreviewItem.Source;
            gf.OutputItem.CurItemNo = gf.PreviewItem.CurItemNo;
            gf.StartPresAt = ((gf.OutputItem.CurItemNo > 0) ? gf.OutputItem.CurItemNo : gf.StartPresAt);
            gf.OutputItem.OutputStyleScreen = true;
            if (gf.ShowRunning)
            {
                if (gf.OutputItem.CurItemNo == 0)
                {
                    gf.WorshipSongs[0, 2] = gf.OutputItem.InMainItemText;
                    gf.WorshipSongs[0, 0] = gf.OutputItem.InSubItemItem1Text;
                    gf.WorshipSongs[0, 1] = DataUtil.Left(gf.WorshipSongs[0, 0], 1);
                    gf.WorshipSongs[0, 4] = gf.PreviewItem.Format.FormatString;
                    gf.AdHocItemPresent = true;
                }
                if (gf.PreviewItem.Type == "P" && gf.PreviewItem.Source != ItemSource.WorshipList)
                {
                    gf.PreLoadPowerpointFiles(ref gf.LivePP, ref gf.WorshipSongs);
                }
            }
            LoadThumbOutlockkey = 0;
            LoadItem(ref gf.OutputItem, gf.PreviewItem.Type + gf.PreviewItem.ItemID, gf.PreviewItem.Format.FormatString, gf.PreviewItem.CurSlide, ref gf.PreviewItem.Title, ScrollToCaret: true);
            UpdateWorshipShowIcons();
            if (gf.ShowRunning)
            {
                gf.MainAction_SongChanged_Transaction = ImageTransitionControl.TransitionAction.AsStoredItem;
                RemoteControlLiveShow(LiveShowAction.Remote_SongChanged);
            }
            FocusOutputArea();
        }

        private void btnToOutput_Click(object sender, EventArgs e)
        {
            if (gf.PreviewItem.ItemID != "")
            {
                CopyPreviewToOutput();
            }
        }

        private void PreViewToOutput()
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

        private void ShowSong(ref SongSettings InItem)
        {
            ShowSong(ref InItem, 1);
        }

        private void ShowSong(ref SongSettings InItem, int StartingSlide)
        {
            ShowSong(ref InItem, StartingSlide, ImageTransitionControl.TransitionAction.AsStored);
        }

        private void ShowSong(ref SongSettings InItem, int StartingSlide, ImageTransitionControl.TransitionAction TransitionAction)
        {
            if (TransitionAction == ImageTransitionControl.TransitionAction.AsStored)
            {
                TransitionAction = (ImageTransitionControl.TransitionAction)InItem.Format.ShowItemTransition;
            }
            InItem.CurSlide = StartingSlide;
            RefreshSlidesFonts(ref InItem, TransitionAction);
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
                }
            }

        }

        private void SetSortButtonPB(SortBy Inmode)
        {
            switch (Inmode)
            {
                case SortBy.Alpha:
                    PB_WordCount.Checked = false;
                    gf.PB_CJKGroupStyle = SortBy.Alpha;
                    break;
                case SortBy.WordCount:
                    PB_WordCount.Checked = true;
                    gf.PB_CJKGroupStyle = SortBy.WordCount;
                    break;
            }
            string FirstCharSym = "";
            string text = "";
            string text2 = "000000";
            Cursor = Cursors.WaitCursor;
            if (gf.UseSongNumbers)
            {
                for (int i = 0; i <= PraiseBookItems.Items.Count - 1; i++)
                {
                    string inTitle = gf.RemoveMusicSym(PraiseBookItems.Items[i].SubItems[2].Text);
                    text2 = DataUtil.Format6(PraiseBookItems.Items[i].SubItems[5].Text);
                    PraiseBookItems.Items[i].SubItems[0].Text = text2 + DataUtil.GetCJKTitle(inTitle, gf.PB_CJKGroupStyle, ref FirstCharSym);
                    PraiseBookItems.Items[i].SubItems[4].Text = FirstCharSym;
                }
            }
            else
            {
                for (int i = 0; i <= PraiseBookItems.Items.Count - 1; i++)
                {
                    string inTitle = gf.RemoveMusicSym(PraiseBookItems.Items[i].SubItems[2].Text);
                    PraiseBookItems.Items[i].SubItems[0].Text = DataUtil.GetCJKTitle(inTitle, gf.PB_CJKGroupStyle, ref FirstCharSym);
                    PraiseBookItems.Items[i].SubItems[4].Text = FirstCharSym;
                }
            }
            SortPraiseBookList();
            Cursor = Cursors.Default;
        }

        private void SavePraiseBook()
        {
            if (!(DataUtil.Trim(PraiseBook.Text) == ""))
            {
                gf.CurPraiseBook = PraiseBook.Text;
                gf.PB_ShowWords[3] = 1;
                gf.PB_ShowWords[4] = 1;
                for (int i = 1; i <= PraiseBookItems.Items.Count; i++)
                {
                    gf.DocumentSongs[i, 2] = gf.RemoveMusicSym(DataUtil.Trim(PraiseBookItems.Items[i - 1].SubItems[2].Text));
                    gf.DocumentSongs[i, 0] = DataUtil.Trim(PraiseBookItems.Items[i - 1].SubItems[3].Text);
                    gf.DocumentSongs[i, 4] = "";
                }
                FileUtil.CreateNewFile(gf.PraiseBookDir + gf.CurPraiseBook + ".esp");
                gf.SaveIndexFile(gf.PraiseBookDir + gf.CurPraiseBook + ".esp", ref PraiseBookItems, UsageMode.PraiseBook, SaveAllItems: true, "", gf.CurPraiseBookNotes);
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
            if (gf.UseSongNumbers)
            {
                for (int i = 0; i <= PraiseBookItems.Items.Count - 1; i++)
                {
                    PraiseBookItems.Items[i].SubItems[1].Text = PraiseBookItems.Items[i].SubItems[5].Text;
                }
            }
            else if (!gf.UseSongNumbers)
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
                gf.GetSelectedIndex(WorshipListItems, ref OutString);
                gf.SetMenuItem(ref CMenuWorship_Edit, OutString, "Edit Item", "Edit", DisableWhenBlank: true);
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

        private void ResetListViewBackgroundColour(ListView InListView)
        {
            for (int i = 0; i <= InListView.Items.Count - 1; i++)
            {
                InListView.Items[i].BackColor = SongFolder.BackColor;
            }
        }

        private void WorshipList_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.A)
            {
                WorshipList_SelectAll();
                return;
            }
            if (e.KeyCode == Keys.Delete)
            {
                RemoveWorshipListSong();
                SaveWorshipList();
            }
            else
            {
                WorshipListIndexChanged();
            }
            WorshipListItems.Focus();
        }

        //static String prePreviewItemID = "";
        //??�� ��????????���� ü?

        private void WorshipListItems_DoubleClick(object sender, EventArgs e)
        {
            if (gf.PreviewItem.Type == "M")
            {
                string mediaPath = gf.PreviewItem.Path;
                if (mediaPath == "")
                {
                    int selectedIndex = gf.GetSelectedIndex(WorshipListItems);
                    if (selectedIndex >= 0)
                    {
                        string idString = WorshipListItems.Items[selectedIndex].SubItems[1].Text;
                        if (DataUtil.Left(idString, 1) == "M")
                        {
                            mediaPath = DataUtil.Mid(idString, 1);
                        }
                    }
                }
                if (mediaPath != "" && gf.RunProcess(mediaPath))
                {
                    WorshipListDoubleClick = true;
                    FocusOutputArea();
                    return;
                }
            }
            if (gf.ShowRunning)
            {
                if (gf.PreviewItem.ItemID != "")
                {
                    //if(prePreviewItemID != gf.PreviewItem.ItemID)
                    CopyPreviewToOutput();
                }
            }
            else if (gf.PreviewItem.ItemID != "")
            {
                //if (prePreviewItemID != gf.PreviewItem.ItemID)
                //{
                CopyPreviewToOutput();
                GoLive(InStatus: true);
                //}
            }
            WorshipListDoubleClick = true;
            FocusOutputArea();

            //prePreviewItemID = gf.PreviewItem.ItemID;
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
                gf.DB_CurSongID = 0;
                ClearLyrics(ref flowLayoutPreviewLyrics);
                ResetMainPictureBox(ref gf.PreviewItem);
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
                gf.DB_CurSongID = 0;
                ClearLyrics(ref flowLayoutPreviewLyrics);
                WorshipListIndexChanged();
                Cursor = Cursors.Default;
            }
        }

        private void PraiseBookItems_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.A)
            {
                PraiseBookList_SelectAll();
                return;
            }
            if (e.KeyCode == Keys.Delete)
            {
                RemovePraiseBookListSong();
            }
            else
            {
                PraiseBookListIndexChanged();
            }
            PraiseBookItems.Focus();
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
                gf.DB_CurSongID = 0;
                ClearLyrics(ref flowLayoutPreviewLyrics);
                Cursor = Cursors.Default;
                DisplayLyrics(gf.PreviewItem, 1, ImageTransitionControl.TransitionAction.None);
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
                gf.DB_CurSongID = 0;
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
                    DisplayLyrics(gf.PreviewItem, 1, ImageTransitionControl.TransitionAction.None);
                }
                Point point = default(Point);
                point = PraiseBookItems.PointToClient(Cursor.Position);
                gf.GetSelectedIndex(PraiseBookItems, ref OutString, "", 2);
                gf.SetMenuItem(ref CMenuPraiseB_Edit, OutString, "Edit Item", "Edit", DisableWhenBlank: true);
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
            int selectedIndex = gf.GetSelectedIndex(PraiseBookItems);
            if (selectedIndex >= 0)
            {
                string text = PraiseBookItems.Items[selectedIndex].SubItems[3].Text;
                gf.PreviewItem.CurItemNo = selectedIndex + 1;
                gf.PreviewItem.TotalItems = PraiseBookItems.Items.Count;
                gf.PreviewItem.Source = ItemSource.PraiseBook;
                LoadItem(ref gf.PreviewItem, text);
            }
            else
            {
                gf.PreviewItem.Type = "";
                gf.PreviewItem.ItemID = "";
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
            int selectedIndex = gf.GetSelectedIndex(PraiseBookItems, ref OutString);
            if (selectedIndex >= 0)
            {
                string text = PraiseBookItems.Items[selectedIndex].SubItems[3].Text;
                Edit_Item(text);
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
            if (gf.PreviewItem.Source == ItemSource.SongsList)
            {
                return AddFromSongsList(AddToLocation);
            }
            if (gf.PreviewItem.Source == ItemSource.ExternalFileInfoScreen)
            {
                return AddFromInfoScreensList(AddToLocation);
            }
            if (gf.PreviewItem.Source == ItemSource.ExternalFileInfoScreen)
            {
                return AddFromMediaFilesList(AddToLocation);
            }
            if (gf.PreviewItem.Source == ItemSource.ExternalFilePowerpoint)
            {
                return AddFromPowerpointList();
            }
            if (gf.PreviewItem.Source == ItemSource.HolyBible)
            {
                return AddFromHolyBible(AddToLocation);
            }
            return false;
        }

        private bool AddFromSongsList()
        {
            return AddFromSongsList(GetWorshipListNextSelectedLoc());
        }
#if DAO
		private bool AddFromSongsList(int AddToLocation)
		{
			if (SongsList.Items.Count == 0 || SongsList.SelectedItems.Count == 0)
			{
				return false;
			}
			ListViewItem listViewItem = new ListViewItem();
			string text = "";
			Cursor = Cursors.WaitCursor;
			if (AddToLocation < 0 || AddToLocation > WorshipListItems.Items.Count)
			{
				AddToLocation = WorshipListItems.Items.Count;
			}
			int num = 0;
			gf.TotalMusicFiles = -1;
			try
			{
				Recordset tableRecordSet = DbDaoController.GetTableRecordSet(gf.ConnectStringMainDB, "SONG");
				tableRecordSet.Index = "PrimaryKey";
				for (int i = 0; i < SongsList.SelectedItems.Count; i++)
				{
					string text2 = SongsList.SelectedItems[i].SubItems[1].Text;
					string text3 = DataUtil.Mid(text2, 1, text2.Length - 1);
					long num2 = DataUtil.StringToInt(text3);
					string text4 = DataUtil.Left(text2, 0);
					try
					{
						tableRecordSet.Seek("=", text3, gf.def, gf.def, gf.def, gf.def, gf.def, gf.def, gf.def, gf.def, gf.def, gf.def, gf.def, gf.def);
						if (!tableRecordSet.NoMatch)
						{
							string text5 = DataUtil.GetDataString(tableRecordSet, "Title_1");
							string dataString = DataUtil.GetDataString(tableRecordSet, "Title_2");
							if (gf.MusicFound(text5, dataString))
							{
								text5 += " <#>";
							}
							text = DataUtil.GetDataString(tableRecordSet, "FORMATDATA");
							listViewItem = WorshipListItems.Items.Insert(AddToLocation, text5);
							listViewItem.ImageIndex = 0;
							listViewItem.SubItems.Add(SongsList.SelectedItems[i].SubItems[1].Text);
							listViewItem.SubItems.Add(text);
							listViewItem.SubItems.Add(SongsList.SelectedItems[i].SubItems[4].Text);
							listViewItem.SubItems.Add(SongsList.SelectedItems[i].SubItems[5].Text);
							listViewItem.SubItems.Add(SongsList.SelectedItems[i].SubItems[6].Text);
							listViewItem.SubItems.Add("");
							listViewItem.SubItems.Add(SongFolder.Text);
							AddToLocation++;
							num++;
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
			if (num == 0)
			{
				Cursor = Cursors.Default;
				return false;
			}
			ShowStatusBarSummary();
			SelectWorshipListItem(AddToLocation - num, num);
			SaveWorshipList();
			UpdateOutputCurItemNo();
			Cursor = Cursors.Default;
			return true;
		}
#elif SQLite
        private bool AddFromSongsList(int AddToLocation)
        {
            if (SongsList.Items.Count == 0 || SongsList.SelectedItems.Count == 0)
            {
                return false;
            }
            ListViewItem listViewItem = new ListViewItem();
            string text = "";
            Cursor = Cursors.WaitCursor;
            if (AddToLocation < 0 || AddToLocation > WorshipListItems.Items.Count)
            {
                AddToLocation = WorshipListItems.Items.Count;
            }
            int num = 0;
            gf.TotalMusicFiles = -1;
            try
            {
                string searchString = "Select * from SONG";
                using DataTable dt = DbController.GetDataTable(gf.ConnectStringMainDB, searchString);

                DataColumn[] primarykey = new DataColumn[] { dt.Columns["SONGID"] };
                dt.PrimaryKey = primarykey;

                for (int i = 0; i < SongsList.SelectedItems.Count; i++)
                {
                    string text2 = SongsList.SelectedItems[i].SubItems[1].Text;
                    string text3 = DataUtil.Mid(text2, 1, text2.Length - 1);
                    long num2 = DataUtil.StringToInt(text3);
                    string text4 = DataUtil.Left(text2, 0);
                    try
                    {
                        DataRow dr = dt.Rows.Find(text3);
                        if (dr != null)
                        {
                            string text5 = DataUtil.GetDataString(dr, "Title_1");
                            string dataString = DataUtil.GetDataString(dr, "Title_2");
                            if (gf.MusicFound(text5, dataString))
                            {
                                text5 += " <#>";
                            }
                            text = DataUtil.GetDataString(dr, "FORMATDATA");
                            listViewItem = WorshipListItems.Items.Insert(AddToLocation, text5);
                            listViewItem.ImageIndex = 0;
                            listViewItem.SubItems.Add(SongsList.SelectedItems[i].SubItems[1].Text);
                            listViewItem.SubItems.Add(text);
                            listViewItem.SubItems.Add(SongsList.SelectedItems[i].SubItems[4].Text);
                            listViewItem.SubItems.Add(SongsList.SelectedItems[i].SubItems[5].Text);
                            listViewItem.SubItems.Add(SongsList.SelectedItems[i].SubItems[6].Text);
                            listViewItem.SubItems.Add("");
                            listViewItem.SubItems.Add(SongFolder.Text);
                            AddToLocation++;
                            num++;
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
            if (num == 0)
            {
                Cursor = Cursors.Default;
                return false;
            }
            ShowStatusBarSummary();
            SelectWorshipListItem(AddToLocation - num, num);
            SaveWorshipList();
            UpdateOutputCurItemNo();
            Cursor = Cursors.Default;
            return true;
        }
#endif

        private int GetWorshipListNextSelectedLoc()
        {
            int selectedIndex = gf.GetSelectedIndex(WorshipListItems);
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
                int num = 0;
                for (int i = 0; i < InfoScreenList.SelectedItems.Count; i++)
                {
                    AddExternalFileToWorshipList(DataUtil.Mid(InfoScreenList.SelectedItems[i].SubItems[1].Text, 1), AddToLocation + num);
                    num++;
                }
                if (InfoScreenList.SelectedItems.Count > 0)
                {
                    SelectWorshipListItem(AddToLocation, num);
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
                int num = 0;
                for (int i = 0; i < MediaList.SelectedItems.Count; i++)
                {
                    AddExternalFileToWorshipList(DataUtil.Mid(MediaList.SelectedItems[i].SubItems[1].Text, 1), AddToLocation + num);
                    num++;
                }
                if (MediaList.SelectedItems.Count > 0)
                {
                    SelectWorshipListItem(AddToLocation, num);
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
            if (gf.PowerpointListingStyle == 0)
            {
                if (PowerpointList.SelectedItems.Count > 0)
                {
                    if (AddToLocation < 0)
                    {
                        AddToLocation = WorshipListItems.Items.Count;
                    }
                    int num = 0;
                    for (int i = 0; i < PowerpointList.SelectedItems.Count; i++)
                    {
                        if (!AddExternalFileToWorshipList(DataUtil.Mid(PowerpointList.SelectedItems[i].SubItems[1].Text, 1), AddToLocation + num))
                        {
                            return false;
                        }
                        num++;
                    }
                    if (PowerpointList.SelectedItems.Count > 0)
                    {
                        SelectWorshipListItem(AddToLocation, num);
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
            if (!(DataUtil.Trim(PraiseBook.Text) == "") && SongsList.SelectedItems.Count >= 1)
            {
                string FirstCharSym = "";
                ListViewItem listViewItem = new ListViewItem();
                string text = "";
                string text2 = "000000";
                Cursor = Cursors.WaitCursor;
                PraiseBookItems.Sorting = SortOrder.None;
                ListViewItem[] array = new ListViewItem[SongsList.SelectedItems.Count];
                PraiseBookItems.BeginUpdate();
                for (int i = 0; i < SongsList.SelectedItems.Count; i++)
                {
                    string text3 = gf.RemoveMusicSym(SongsList.SelectedItems[i].Text);
                    text2 = (gf.UseSongNumbers ? DataUtil.Format6(SongsList.SelectedItems[i].SubItems[4].Text) : "");
                    array[i] = new ListViewItem(new string[6]
                    {
                        text2 + DataUtil.GetCJKTitle(text3, gf.PB_CJKGroupStyle, ref FirstCharSym),
                        " ",
                        text3,
                        SongsList.SelectedItems[i].SubItems[1].Text,
                        FirstCharSym,
                        SongsList.SelectedItems[i].SubItems[4].Text
                    });
                }
                PraiseBookItems.Items.AddRange(array);
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
            string text = "";
            string text2 = "000000";
            Cursor = Cursors.WaitCursor;
            PraiseBookItems.Sorting = SortOrder.None;
            if (gf.UseSongNumbers)
            {
                for (int i = 0; i < SongsList.SelectedItems.Count; i++)
                {
                    string text3 = gf.RemoveMusicSym(SongsList.SelectedItems[i].Text);
                    text2 = DataUtil.Format6(SongsList.SelectedItems[i].SubItems[4].Text);
                    listViewItem = PraiseBookItems.Items.Add(text2 + DataUtil.GetCJKTitle(text3, gf.PB_CJKGroupStyle, ref FirstCharSym));
                    listViewItem.SubItems.Add(" ");
                    listViewItem.SubItems.Add(text3);
                    listViewItem.SubItems.Add(SongsList.SelectedItems[i].SubItems[1].Text);
                    listViewItem.SubItems.Add(FirstCharSym);
                    listViewItem.SubItems.Add(SongsList.SelectedItems[i].SubItems[4].Text);
                }
            }
            else
            {
                for (int i = 0; i < SongsList.SelectedItems.Count; i++)
                {
                    string text3 = gf.RemoveMusicSym(SongsList.SelectedItems[i].Text);
                    listViewItem = PraiseBookItems.Items.Add(DataUtil.GetCJKTitle(text3, gf.PB_CJKGroupStyle, ref FirstCharSym));
                    listViewItem.SubItems.Add(" ");
                    listViewItem.SubItems.Add(text3);
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
            if (gf.GetSelectedIndex(SongsList) >= 0)
            {
                if (gf.EasiSlidesMode == UsageMode.Worship)
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
            gf.EditNotes = gf.CurSessionNotes;
            gf.EditNotesHeading = "Edit Session Notes";
            FrmEditNotes frmEditNotes = new FrmEditNotes();
            if (frmEditNotes.ShowDialog() == DialogResult.OK)
            {
                gf.CurSessionNotes = gf.EditNotes;
                PreviewNotes.Text = gf.CurSessionNotes;
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

        private void Folders_WordCount_MouseUp(object sender, MouseEventArgs e)
        {
            SortBy curStyle = CurStyle;
            SetSortButton(Folders_WordCount.Checked ? SortBy.WordCount : SortBy.Alpha);
            gf.FolderGroupStyle[gf.GetFolderNumber(SongFolder.Text)] = CurStyle;
            gf.SaveConfigSettings();
            if (CurStyle != curStyle)
            {
                SongFolder_Change();
            }
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
                if (gf.HB_TotalVersions < 1)
                {
                    return;
                }
                gf.FindBibleVerses = true;
                gf.FindFolderItems = false;
                gf.HB_SQLString = gf.BuildBibleSearchString(inText, TabBibleVersions.SelectedIndex);
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
            if ((BibleText.Text == "") | (BibleText.SelectionLength < 1))
            {
                return false;
            }
            if ((gf.PreviewItem.Type != "B") | (gf.PreviewItem.Source != ItemSource.HolyBible))
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
                if (gf.ShowRunning)
                {
                    GoLive(InStatus: false);
                }
                gf.OutputItem.CurItemNo = 0;
                gf.StartPresAt = 0;
                return;
            }
            int num = 0;
            while (true)
            {
                if (num <= WorshipListItems.Items.Count - 1)
                {
                    if (WorshipListItems.Items[num].SubItems[6].Text == "O")
                    {
                        gf.StartPresAt = num + 1;
                        if (gf.OutputItem.CurItemNo != 0)
                        {
                            gf.OutputItem.CurItemNo = gf.StartPresAt;
                        }
                        WorshipListItems.Items[num].EnsureVisible();
                        break;
                    }
                    num++;
                    continue;
                }
                if (WorshipListItems.Items.Count <= 0)
                {
                    break;
                }
                if (gf.StartPresAt > WorshipListItems.Items.Count)
                {
                    gf.StartPresAt = WorshipListItems.Items.Count;
                }
                if (gf.OutputItem.CurItemNo != 0)
                {
                    if (gf.StartPresAt == 0)
                    {
                        gf.StartPresAt = 1;
                    }
                    WorshipListItems.Items[gf.StartPresAt - 1].SubItems[6].Text = "O";
                    WorshipListItems.Items[gf.StartPresAt - 1].EnsureVisible();
                    gf.OutputItem.CurItemNo = gf.StartPresAt;
                }
                break;
            }
            if (gf.ShowRunning)
            {
                ValidateWorshipListItems(ShowErrorMessage: false);
                RemoteControlLiveShow(LiveShowAction.Remote_WorshipListChanged);
            }
        }

        private void HB_SelectedPassagesChanged(string InBibleString, ref string InTitle)
        {
            Cursor = Cursors.WaitCursor;
            gf.PreviewItem.Source = ItemSource.HolyBible;
            if (InBibleString != "")
            {
                string text = "B" + InBibleString;
                gf.PreviewItem.InMainItemText = InTitle;
                gf.PreviewItem.InSubItemItem1Text = text;
                gf.PreviewItem.CurItemNo = 0;
                gf.PreviewItem.Type = "B";
                LoadItem(ref gf.PreviewItem, text, "", 1, ref InTitle, ScrollToCaret: true);
                UpdateDisplayPanelFields();
            }
            else
            {
                gf.PreviewItem.Type = "";
                gf.PreviewItem.Title = "";
                gf.PreviewItem.ItemID = "";
                gf.PreviewItem.CurItemNo = 0;
                gf.LoadIndividualFormatData(ref gf.PreviewItem, "");
                AllowIndividualFormat(AllowFormat: false);
                UpdateFormatFields();
                BuildVerseButtons(gf.PreviewItem);
                DisplayLyrics(gf.PreviewItem, 1);
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
                    if (!((charIndexFromPosition < BibleText.SelectionStart) | (charIndexFromPosition > BibleText.SelectionStart + BibleText.SelectionLength - 1)))
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
            for (int i = 0; i <= gf.HB_TotalVersions - 1; i++)
            {
                toolStripMenuItem = new ToolStripMenuItem();
                toolStripMenuItem.Text = gf.HB_Versions[i, 1];
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
            bool flag = BibleUserLookupValidation(ref InText, ref StartChapterNo, ref StartVerseNo, ref EndChapterNo, ref EndVerseNo);
            BibleUserLookup.Text = InText;
            if (flag)
            {
                int num = 0;
                int num2 = 0;
                for (int i = 1; i <= gf.HB_VersesLocation[0, 0]; i++)
                {
                    if ((StartChapterNo == gf.HB_VersesLocation[i, 2]) & (StartVerseNo == gf.HB_VersesLocation[i, 3]))
                    {
                        num = i;
                    }
                }
                if (num > 0)
                {
                    for (int i = num; i <= gf.HB_VersesLocation[0, 0]; i++)
                    {
                        if ((EndChapterNo >= gf.HB_VersesLocation[i, 2]) & (EndVerseNo >= gf.HB_VersesLocation[i, 3]))
                        {
                            num2 = i;
                        }
                    }
                    if (num2 > 0)
                    {
                        EndVerseNo = gf.HB_VersesLocation[num2, 3];
                        BibleText.SelectionLength = 0;
                        BibleText.SelectionStart = gf.HB_VersesLocation[num, 4];
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
                            BibleText.SelectionLength = gf.HB_VersesLocation[num2, 4] - gf.HB_VersesLocation[num, 4] + gf.HB_VersesLocation[num2, 5] - 2;
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
            if ((BookLookup.Text == "") | (BookLookup.SelectedIndex == 66) | (DataUtil.Trim(InText) == ""))
            {
                return false;
            }
            string text = DataUtil.Trim(InText);
            if (text.IndexOf("  ") >= 0)
            {
                return false;
            }
            text = text.Replace('.', ':');
            text = text.Replace('v', ':');
            text = text.Replace('V', ':');
            string text2 = text;
            string text3 = "";
            bool flag = true;
            if (text.IndexOf('-') < 0)
            {
                int num = text.IndexOf(' ');
                if (num >= 0)
                {
                    int num2 = text.IndexOf(':');
                    if (num2 >= 0 && num2 < num)
                    {
                        text = text.Insert(num, "-");
                        text = text.Remove(num + 1, 1);
                    }
                    else
                    {
                        num = text.IndexOf(' ', num + 1);
                        if (num >= 0)
                        {
                            text = text.Insert(num, "-");
                            text = text.Remove(num + 1, 1);
                        }
                        else
                        {
                            text = text.Replace(" ", "-");
                        }
                    }
                }
            }
            if (text.IndexOf('-') >= 0)
            {
                sArray = text.Split('-');
                text2 = DataUtil.Trim(sArray[0]);
                text3 = DataUtil.Trim(sArray[1]);
            }
            else if (text.IndexOf(' ') >= 0)
            {
                sArray = text.Split('-');
                text2 = DataUtil.Trim(sArray[0]);
                text3 = DataUtil.Trim(sArray[1]);
            }
            text2 = text2.Replace(' ', ':');
            text3 = text3.Replace(' ', ':');
            InText = text2 + ((text3 != "") ? ("-" + text3) : "");
            StartChapterNo = 1;
            StartVerseNo = 1;
            EndChapterNo = 1;
            EndVerseNo = 1;
            sArray = text2.Split(':');
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
                int num3 = DataUtil.StringToInt(sArray[1]);
                if (num3 > 0)
                {
                    StartVerseNo = num3;
                }
                flag = false;
            }
            if (text3 == "")
            {
                EndChapterNo = StartChapterNo;
                if (flag)
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
                sArray = text3.Split(':');
                if (sArray.GetUpperBound(0) < 1)
                {
                    if (flag)
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
            int selectedIndex = gf.GetSelectedIndex(WorshipListItems);
            if (selectedIndex >= 1)
            {
                TimerFlasher.Stop();
                for (int i = 0; i <= 7; i++)
                {
                    string text = WorshipListItems.Items[selectedIndex].SubItems[i].Text;
                    WorshipListItems.Items[selectedIndex].SubItems[i].Text = WorshipListItems.Items[selectedIndex - 1].SubItems[i].Text;
                    WorshipListItems.Items[selectedIndex - 1].SubItems[i].Text = text;
                }
                int imageIndex = WorshipListItems.Items[selectedIndex].ImageIndex;
                WorshipListItems.Items[selectedIndex].ImageIndex = WorshipListItems.Items[selectedIndex - 1].ImageIndex;
                WorshipListItems.Items[selectedIndex - 1].ImageIndex = imageIndex;
                WorshipListItems.Items[selectedIndex].Selected = false;
                WorshipListItems.Items[selectedIndex - 1].Selected = true;
                WorshipListItems.EnsureVisible(selectedIndex - 1);
                gf.PreviewItem.CurItemNo = selectedIndex;
                UpdateOutputCurItemNo();
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
            int selectedIndex = gf.GetSelectedIndex(WorshipListItems);
            if (!((selectedIndex < 0) | (selectedIndex == count - 1)))
            {
                TimerFlasher.Stop();
                for (int i = 0; i <= 7; i++)
                {
                    string text = WorshipListItems.Items[selectedIndex].SubItems[i].Text;
                    WorshipListItems.Items[selectedIndex].SubItems[i].Text = WorshipListItems.Items[selectedIndex + 1].SubItems[i].Text;
                    WorshipListItems.Items[selectedIndex + 1].SubItems[i].Text = text;
                }
                int imageIndex = WorshipListItems.Items[selectedIndex].ImageIndex;
                WorshipListItems.Items[selectedIndex].ImageIndex = WorshipListItems.Items[selectedIndex + 1].ImageIndex;
                WorshipListItems.Items[selectedIndex + 1].ImageIndex = imageIndex;
                WorshipListItems.Items[selectedIndex].Selected = false;
                WorshipListItems.Items[selectedIndex + 1].Selected = true;
                WorshipListItems.EnsureVisible(selectedIndex + 1);
                gf.PreviewItem.CurItemNo = selectedIndex + 2;
                UpdateOutputCurItemNo();
                UpdateWorshipShowIcons();
                TimerFlasher.Start();
            }
        }

        private bool RemoveWorshipListSong()
        {
            return RemoveWorshipListSong(UpdateCurItemNo: true);
        }

        private bool RemoveWorshipListSong(bool UpdateCurItemNo)
        {
            bool flag = false;
            int num = -1;
            if (WorshipListItems.Items.Count == 0)
            {
                return false;
            }
            for (int num2 = WorshipListItems.Items.Count - 1; num2 >= 0; num2--)
            {
                if (WorshipListItems.Items[num2].Selected)
                {
                    WorshipListItems.Items[num2].Remove();
                    num = num2;
                }
            }
            if (!UpdateCurItemNo)
            {
                return false;
            }
            if (gf.OutputItem.CurItemNo == num + 1)
            {
                flag = true;
            }
            if (WorshipListItems.Items.Count > num)
            {
                if (num >= 0)
                {
                    WorshipListItems.Items[num].Selected = true;
                }
            }
            else if (WorshipListItems.Items.Count > 0)
            {
                if (num - 1 >= 0 && num <= WorshipListItems.Items.Count)
                {
                    WorshipListItems.Items[num - 1].Selected = true;
                }
                else
                {
                    WorshipListItems.Items[WorshipListItems.Items.Count - 1].Selected = true;
                }
            }
            ShowStatusBarSummary();
            WorshipListIndexChanged();
            UpdateOutputCurItemNo();
            if (flag)
            {
                CopyPreviewToOutput();
            }
            return true;
        }

        private void SetDocLayout(PraiseBookLayout InLayout)
        {
            gf.PB_FormatChanged = false;
            gf.PB_Layout = InLayout;
            FrmGenerateDoc frmGenerateDoc = new FrmGenerateDoc();
            bool flag = false;
            if (InLayout == PraiseBookLayout.WorshipList)
            {
                LoadWorshipList(1);
                gf.PB_FullFileName = gf.RootEasiSlidesDir + "Documents\\" + gf.CurSession + ".rtf";
                gf.PB_DocumentName = gf.CurSession;
                if (PrepareWorshipListLyricsForRTF())
                {
                    frmGenerateDoc.ShowDialog();
                    if (gf.PB_FormatChanged)
                    {
                        SaveWorshipList();
                    }
                }
                return;
            }
            LoadPraiseBook(1);
            gf.PB_FullFileName = gf.RootEasiSlidesDir + "Documents\\" + gf.CurPraiseBook + ".rtf";
            gf.PB_DocumentName = gf.CurPraiseBook;
            if (PreparePraiseBookLyricsForRTF())
            {
                frmGenerateDoc.ShowDialog();
                if (gf.PB_FormatChanged)
                {
                    SavePraiseBook();
                }
            }
        }

        private bool PrepareWorshipListLyricsForRTF()
        {
            if (WorshipListItems.Items.Count > 0)
            {
                int num = ValidateWorshipListItems(ShowErrorMessage: true);
                if (num >= 0)
                {
                    return false;
                }
                for (int i = 0; i <= WorshipListItems.Items.Count - 1; i++)
                {
                    gf.DocumentSongs[i + 1, 0] = gf.WorshipSongs[i + 1, 0];
                    gf.DocumentSongs[i + 1, 1] = gf.WorshipSongs[i + 1, 1];
                    gf.DocumentSongs[i + 1, 2] = gf.RemoveMusicSym(DataUtil.Trim(gf.WorshipSongs[i + 1, 2]));
                    gf.DocumentSongs[i + 1, 3] = gf.WorshipSongs[i + 1, 3];
                    gf.DocumentSongs[i + 1, 4] = '>' + gf.ExtractHeaderInfo(gf.WorshipSongs[i + 1, 4], 71, '>');
                }
                gf.TotalPraiseBookItems = WorshipListItems.Items.Count;
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
            int num = -1;
            if (PraiseBookItems.Items.Count == 0)
            {
                return;
            }
            for (int num2 = PraiseBookItems.Items.Count - 1; num2 >= 0; num2--)
            {
                if (PraiseBookItems.Items[num2].Selected)
                {
                    PraiseBookItems.Items[num2].Remove();
                    num = num2;
                }
            }
            SavePraiseBook();
            if (PraiseBookItems.Items.Count > num)
            {
                PraiseBookItems.Items[num].Selected = true;
            }
            else if (PraiseBookItems.Items.Count > 0)
            {
                if (num - 1 >= 0 && num <= PraiseBookItems.Items.Count)
                {
                    PraiseBookItems.Items[num - 1].Selected = true;
                }
                else
                {
                    PraiseBookItems.Items[PraiseBookItems.Items.Count - 1].Selected = true;
                }
            }
            PraiseBookListIndexChanged();
            if (gf.UseSongNumbers)
            {
                for (int num2 = 0; num2 <= PraiseBookItems.Items.Count - 1; num2++)
                {
                    PraiseBookItems.Items[num2].SubItems[1].Text = PraiseBookItems.Items[num2].SubItems[5].Text;
                }
            }
            else
            {
                for (int num2 = 0; num2 <= PraiseBookItems.Items.Count - 1; num2++)
                {
                    PraiseBookItems.Items[num2].SubItems[1].Text = Convert.ToString(num2 + 1);
                }
            }
            ShowStatusBarSummary();
        }

        private void TimerFlasher_Tick(object sender, EventArgs e)
        {
            gf.TimerFlashOn = !gf.TimerFlashOn;
            if (cbGoLive.Checked)
            {
                cbGoLive.ForeColor = (gf.TimerFlashOn ? gf.ButtonPressedForeColour : gf.ButtonDefaultForeColour);
            }
            if (cbOutputBlack.Checked)
            {
                cbOutputBlack.ImageIndex = (gf.TimerFlashOn ? 16 : 15);
            }
            if (cbOutputClear.Checked)
            {
                cbOutputClear.ImageIndex = (gf.TimerFlashOn ? 18 : 17);
            }
            //if (cbOutputCam.Checked)
            //{
            //    cbOutputCam.ImageIndex = (gf.TimerFlashOn ? 31 : 30);
            //}
            if (gf.ShowRunning)
            {
                if (gf.DualMonitorMode & gf.LaunchShowUpdateDone)
                {
                    Focus();
                    gf.LaunchShowUpdateDone = false;
                }
                if (gf.OutputItem.CurItemNo > 0)
                {
                    try
                    {
                        if (WorshipListItems.Items[gf.StartPresAt - 1].ImageIndex == 0 || WorshipListItems.Items[gf.StartPresAt - 1].ImageIndex == 2 || WorshipListItems.Items[gf.StartPresAt - 1].ImageIndex == 4 || WorshipListItems.Items[gf.StartPresAt - 1].ImageIndex == 6 || WorshipListItems.Items[gf.StartPresAt - 1].ImageIndex == 8 || WorshipListItems.Items[gf.StartPresAt - 1].ImageIndex == 28 || WorshipListItems.Items[gf.StartPresAt - 1].ImageIndex == 10)
                        {
                            WorshipListItems.Items[gf.StartPresAt - 1].ImageIndex = WorshipListItems.Items[gf.StartPresAt - 1].ImageIndex + (gf.TimerFlashOn ? 1 : 0);
                        }
                        else
                        {
                            WorshipListItems.Items[gf.StartPresAt - 1].ImageIndex = WorshipListItems.Items[gf.StartPresAt - 1].ImageIndex - 1 + (gf.TimerFlashOn ? 1 : 0);
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
            if (gf.DualMonitorMode)
            {
                LoadWorshipListItemToOutput(gf.LiveItem.CurItemNo, gf.LiveItem.CurSlide, NotifyLiveShow: false);
                FocusOutputArea();
                Focus();
            }
        }

        public void Remote_SlideChanged()
        {
            if (gf.DualMonitorMode)
            {
                gf.OutputItem.CurSlide = gf.LiveItem.CurSlide;
                MoveToSlideOutputItem(gf.OutputItem, KeyDirection.Refresh, NotifyLiveShow: false);
                FocusOutputArea();
                Focus();
            }
        }

        public void Remote_MovedToGapItem()
        {
            if (gf.DualMonitorMode)
            {
                LoadItem(ref gf.OutputItem, "G1", "", 0);
                FocusOutputArea();
                Focus();
            }
        }

        public void Remote_EndShow()
        {
            if (gf.DualMonitorMode)
            {
                GoLive(InStatus: false);
            }
        }

        private void ItemKeyPressed(SongSettings InItem, Keys KeyCode, bool ShiftKey)
        {
            if (gf.EasiSlidesMode == UsageMode.PraiseBook || KeyCode == Keys.Tab)
            {
                return;
            }
            gf.ReMapKeyBoard(ref KeyCode);
            int num;
            switch (KeyCode)
            {
                case Keys.Home:
                    ManualMoveToItem(InItem, KeyDirection.FirstOne);
                    return;
                case Keys.Prior:
                    ManualMoveToItem(InItem, KeyDirection.PrevOne);
                    return;
                case Keys.Next:
                    ManualMoveToItem(InItem, KeyDirection.NextOne);
                    return;
                case Keys.End:
                    ManualMoveToItem(InItem, KeyDirection.LastOne);
                    return;
                case Keys.Tab:
                    ManualMoveToItem(InItem, KeyDirection.NextOne);
                    return;
                case Keys.Up:
                    if (!gf.GlobalHookKey_CtrlArrow && !gf.GlobalHookKey_Arrow)
                    {
                        MoveToSlide(InItem, KeyDirection.PrevOne);
                    }
                    return;
                case Keys.Left:
                    MoveToSlide(InItem, KeyDirection.FirstOne);
                    return;
                case Keys.Right:
                    MoveToSlide(InItem, KeyDirection.LastOne);
                    return;
                case Keys.Down:
                    if (!gf.GlobalHookKey_CtrlArrow && !gf.GlobalHookKey_Arrow)
                    {
                        MoveToSlide(InItem, KeyDirection.NextOne);
                    }
                    return;
                case Keys.Space:
                    MoveToSlide(InItem, KeyDirection.SpaceOne);
                    return;
                default:
                    num = ((!ShiftKey || KeyCode != Keys.B) ? 1 : 0);
                    break;
                case Keys.W:
                    num = 0;
                    break;
            }
            if (num == 0)
            {
                JumpToVerseType(InItem, 103);
                return;
            }
            int num2;
            switch (KeyCode)
            {
                case Keys.B:
                    JumpToVerseType(InItem, 100);
                    return;
                default:
                    num2 = ((!ShiftKey || KeyCode != Keys.P) ? 1 : 0);
                    break;
                case Keys.Q:
                    num2 = 0;
                    break;
            }
            if (num2 == 0)
            {
                JumpToVerseType(InItem, 112);
                return;
            }
            int num3;
            switch (KeyCode)
            {
                case Keys.P:
                    JumpToVerseType(InItem, 111);
                    return;
                case Keys.E:
                    JumpToVerseType(InItem, 101);
                    return;
                case Keys.G:
                    if (gf.GapItemOption == GapType.None)
                    {
                        gf.GapItemOption = gf.AltGapItemOption;
                        gf.AltGapItemOption = GapType.None;
                    }
                    else
                    {
                        gf.AltGapItemOption = gf.GapItemOption;
                        gf.GapItemOption = GapType.None;
                    }
                    ShowStatusBarSummary();
                    return;
                case Keys.Z:
                    if (InItem.OutputStyleScreen)
                    {
                        QueryShowActive();
                    }
                    return;
                case Keys.A:
                    SetRotateState(!gf.AutoRotateOn);
                    return;
                case Keys.J:
                    GotoNextNonRotateItem(InItem);
                    return;
                case Keys.M:
                    if (InItem.OutputStyleScreen)
                    {
                        RemoteControlLiveShow(LiveShowAction.Remote_MediaItemPausePlay);
                    }
                    return;
                default:
                    num3 = ((!ShiftKey || KeyCode != Keys.C) ? 1 : 0);
                    break;
                case Keys.T:
                    num3 = 0;
                    break;
            }
            if (num3 == 0)
            {
                JumpToVerseType(InItem, 102);
                return;
            }
            if (KeyCode == Keys.C)
            {
                KeyCode = Keys.D0;
            }
            else if (KeyCode == Keys.D0 || KeyCode == Keys.NumPad0)
            {
                KeyCode = Keys.D0;
            }
            else if (KeyCode == Keys.D1 || KeyCode == Keys.NumPad1)
            {
                KeyCode = Keys.D1;
            }
            else if (KeyCode == Keys.D2 || KeyCode == Keys.NumPad2)
            {
                KeyCode = Keys.D2;
            }
            else if (KeyCode == Keys.D3 || KeyCode == Keys.NumPad3)
            {
                KeyCode = Keys.D3;
            }
            else if (KeyCode == Keys.D4 || KeyCode == Keys.NumPad4)
            {
                KeyCode = Keys.D4;
            }
            else if (KeyCode == Keys.D5 || KeyCode == Keys.NumPad5)
            {
                KeyCode = Keys.D5;
            }
            else if (KeyCode == Keys.D6 || KeyCode == Keys.NumPad6)
            {
                KeyCode = Keys.D6;
            }
            else if (KeyCode == Keys.D7 || KeyCode == Keys.NumPad7)
            {
                KeyCode = Keys.D7;
            }
            else if (KeyCode == Keys.D8 || KeyCode == Keys.NumPad8)
            {
                KeyCode = Keys.D8;
            }
            else
            {
                if (KeyCode != Keys.D9 && KeyCode != Keys.NumPad9)
                {
                    return;
                }
                KeyCode = Keys.D9;
            }
            if (InItem.OutputStyleScreen)
            {
                if (InItem.SongVerses[(int)(KeyCode - 48)] > 0)
                {
                    InItem.CurSlide = InItem.SongVerses[(int)(KeyCode - 48)];
                    MoveToSlide(InItem, KeyDirection.Refresh);
                    KeyCode = Keys.None;
                }
            }
            else if (InItem.SongVerses[(int)(KeyCode - 48)] > 0)
            {
                InItem.CurSlide = InItem.SongVerses[(int)(KeyCode - 48)];
                MoveToSlide(InItem, KeyDirection.Refresh);
                KeyCode = Keys.None;
            }
        }

        private void JumpToVerseType(SongSettings InItem, int InOtherVerse)
        {
            int num = 1;
            while (true)
            {
                if (num <= InItem.TotalSlides)
                {
                    if (InItem.Slide[num, 0] == InOtherVerse)
                    {
                        break;
                    }
                    num++;
                    continue;
                }
                return;
            }
            InItem.CurSlide = num;
            MoveToSlide(InItem, KeyDirection.Refresh);
        }

        private void ManualMoveToItem(SongSettings InItem, KeyDirection InDirection)
        {
            MoveToItem(InItem, InDirection, 0, NotifyLiveShow: true);
        }

        private void ManualMoveToItem(SongSettings InItem, int GotoItemNumber)
        {
            if (GotoItemNumber <= WorshipListItems.Items.Count)
            {
                InItem.CurItemNo = GotoItemNumber;
                gf.StartPresAt = GotoItemNumber;
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
            if (InItem.Type == "G" && gf.TotalWorshipListItems == 0)
            {
                return;
            }
            gf.Launch_StartPresAt = gf.StartPresAt;
            LoadThumbOutlockkey = 0;

            switch (InDirection)
            {
                case KeyDirection.FirstOne:
                    LoadWorshipListItemToOutput(1, SlideNo, NotifyLiveShow: false);
                    break;
                case KeyDirection.PrevOne:
                    LoadWorshipListItemToOutput(((InItem.CurItemNo > 0) & InItem.OutputStyleScreen) ? (gf.StartPresAt - ((!(InItem.Type == "G")) ? 1 : 0)) : gf.StartPresAt, SlideNo, NotifyLiveShow: false);
                    break;
                case KeyDirection.NextOne:
                    if (gf.GapItemOption == GapType.None)
                    {
                        LoadWorshipListItemToOutput(gf.StartPresAt + 1, SlideNo, NotifyLiveShow: false);
                    }
                    else if (gf.StartPresAt == 0 || (InItem.Type == "G" && gf.Launch_StartPresAt != gf.TotalWorshipListItems))
                    {
                        LoadWorshipListItemToOutput(gf.StartPresAt + 1, SlideNo, NotifyLiveShow: false);
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
            if (gf.ShowRunning && NotifyLiveShow)
            {
                gf.MainAction_SongChanged_Transaction = ImageTransitionControl.TransitionAction.AsStored;
                gf.MainAction_MoveToItemKeyDirection = InDirection;
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
            int selectedIndex = gf.GetSelectedIndex(WorshipListItems);
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

        private void MoveToSlide(SongSettings InItem, KeyDirection InDirection)
        {
            if (InItem.OutputStyleScreen)
            {
                if (InDirection != KeyDirection.NextOne || InItem.CurSlide < InItem.TotalSlides || gf.AdvanceNextItem)
                {
                    MoveToSlideOutputItem(InItem, InDirection, NotifyLiveShow: true);
                    SetOutputPPThumbImages1(gf.OutputItem.CurSlide);
                }
            }
            else
            {
                MoveToSlidePreviewItem(InItem, InDirection);
                SetPreviewPPThumbImages1(gf.PreviewItem.CurSlide);
            }
        }

        private void MoveToSlideOutputItem(SongSettings InItem, KeyDirection InDirection, bool NotifyLiveShow)
        {
            if (gf.AdvanceNextItem)
            {
                if (InDirection == KeyDirection.PrevOne)
                {
                    if (InItem.Type == "G")
                    {
                        MoveToItem(InItem, KeyDirection.Refresh, 30000, NotifyLiveShow);
                        return;
                    }
                    if (InItem.CurItemNo > 1 && InItem.CurSlide < 2)
                    {
                        MoveToItem(InItem, KeyDirection.PrevOne, 30000, NotifyLiveShow);
                        return;
                    }
                }
                else if (InDirection == KeyDirection.NextOne && InItem.CurItemNo < WorshipListItems.Items.Count && InItem.CurSlide >= InItem.TotalSlides)
                {
                    if (gf.ShowRunning & (InItem.Type == "P"))
                    {
                        int num = MainPPT.ImplementPowerpointSlideMovement(ref InItem.CurSlide, InItem.TotalSlides, (OfficeLibKeys)gf.ReMapKeyDirectionToPowerpoint(InDirection));
                        if (num > 0)
                        {
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
            if (gf.ShowRunning & (InItem.Type == "P"))
            {
                MainPPT.ImplementPowerpointSlideMovement(ref InItem.CurSlide, InItem.TotalSlides, (OfficeLibKeys)gf.ReMapKeyDirectionToPowerpoint(InDirection));
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
                        if (gf.GapItemOption == GapType.None)
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
                if (gf.ShowRunning && NotifyLiveShow)
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

        private void QueryShowActive()
        {
            if (OutputScreen.RefStatus())
            {
                RemoteControlLiveShow(LiveShowAction.Remote_ReferenceAlertHide);
                OutputScreen.StopRef();
            }
            else
            {
                RemoteControlLiveShow(LiveShowAction.Remote_ReferenceAlertShow);
                ShowSlide(ref gf.OutputItem, ImageTransitionControl.TransitionAction.None, DoActiveIndicator: true);
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
            string text = WorshipListItems.Items[Selecteditem].SubItems[1].Text;
            string text2 = WorshipListItems.Items[Selecteditem].SubItems[2].Text;
            string InTitle = WorshipListItems.Items[Selecteditem].SubItems[0].Text;
            gf.OutputItem.CurItemNo = Selecteditem + 1;
            gf.StartPresAt = gf.OutputItem.CurItemNo;
            gf.OutputItem.Source = ItemSource.WorshipList;
            gf.OutputItem.OutputStyleScreen = true;
            LoadItem(ref gf.OutputItem, text, text2, SlideNo, ref InTitle, ScrollToCaret: true);
            if (gf.ShowRunning && NotifyLiveShow)
            {
                gf.MainAction_SongChanged_Transaction = ImageTransitionControl.TransitionAction.AsStored;
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
                ManualMoveToItem(gf.PreviewItem, KeyDirection.PrevOne);
            }
            else if (name == "PreviewBtnItemDown")
            {
                ManualMoveToItem(gf.PreviewItem, KeyDirection.NextOne);
            }
            else if (name == "PreviewBtnSlideUp")
            {
                MoveToSlide(gf.PreviewItem, KeyDirection.PrevOne);
            }
            else if (name == "PreviewBtnSlideDown")
            {
                MoveToSlide(gf.PreviewItem, KeyDirection.NextOne);
            }
            FocusPreviewArea();
        }

        private void OutputBtnJumpToNonRotate_Click(object sender, EventArgs e)
        {
            GotoNextNonRotateItem(gf.OutputItem);
            FocusOutputArea();
        }

        private void OutputBtnMedia_Click(object sender, EventArgs e)
        {
            if (gf.OutputItem.OutputStyleScreen)
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
                ManualMoveToItem(gf.OutputItem, KeyDirection.PrevOne);
            }
            else if (name == "OutputBtnItemDown")
            {
                ManualMoveToItem(gf.OutputItem, KeyDirection.NextOne);
            }
            else if (name == "OutputBtnSlideUp")
            {
                MoveToSlide(gf.OutputItem, KeyDirection.PrevOne);
            }
            else if (name == "OutputBtnSlideDown")
            {
                MoveToSlide(gf.OutputItem, KeyDirection.NextOne);
            }
            FocusOutputArea();
        }

        private void PreviewBtnVerse_Click(object sender, EventArgs e)
        {
            Button button = (Button)sender;
            int num = DataUtil.ObjToInt(button.Tag);
            if (num < 10)
            {
                gf.PreviewItem.CurSlide = gf.PreviewItem.SongVerses[num];
            }
            else
            {
                for (int i = 1; i <= gf.PreviewItem.TotalSlides; i++)
                {
                    if (gf.PreviewItem.Slide[i, 0] == num)
                    {
                        gf.PreviewItem.CurSlide = i;
                        i = gf.PreviewItem.TotalSlides;
                    }
                }
            }
            MoveToSlide(gf.PreviewItem, KeyDirection.Refresh);
            FocusPreviewArea();
        }

        private void OutputBtnVerse_Click(object sender, EventArgs e)
        {
            Button button = (Button)sender;
            int num = DataUtil.ObjToInt(button.Tag);
            if (num < 10)
            {
                gf.OutputItem.CurSlide = gf.OutputItem.SongVerses[num];
            }
            else
            {
                for (int i = 1; i <= gf.OutputItem.TotalSlides; i++)
                {
                    if (gf.OutputItem.Slide[i, 0] == num)
                    {
                        gf.OutputItem.CurSlide = i;
                        i = gf.OutputItem.TotalSlides;
                    }
                }
            }
            MoveToSlide(gf.OutputItem, KeyDirection.Refresh);
            FocusOutputArea();
        }

        private void FormControl_Enter(object sender, EventArgs e)
        {
            Control inControl = (Control)sender;
            gf.NormalTextRegionBackColour = SongFolder.BackColor;
            gf.Control_Enter(inControl);
        }

        private void FormControl_Leave(object sender, EventArgs e)
        {
            Control inControl = (Control)sender;
            gf.NormalTextRegionBackColour = SongFolder.BackColor;
            gf.Control_Leave(inControl);
        }

        /// <summary>
        /// daniel
        /// ??��??docx ��??
        /// </summary>
        private void LocateFileWorshipList()
        {
            int num = 0;
            for (int i = 0; i <= WorshipListItems.Items.Count - 1; i++)
            {
                if (DataUtil.Left(WorshipListItems.Items[i].SubItems[1].Text, 1) == "P")
                {
                    num++;
                }
            }
            string filter = "Valid External Files (*.ppt,*.pptx,*.doc,*.docx,*.txt,*.esi,*.esw)|*.ppt;*.pptx;*.doc;*.docx;*.txt;*.esi;*.esw|Powerpoint Files (*.ppt)|*.ppt|Powerpoint Files (*.pptx)|*.pptx|Word Documents (*.doc)|*.doc|Word Documents (*.docx)|*.docx|Text Files (*.txt)|*.txt|InfoScreens (*.esi)|*.esi|Worship Lists (*.esw)|*.esw";
            if (gf.TotalMediaFileExt > 0)
            {
                string mediaLabel = "";
                string mediaPattern = "";
                for (int i = 0; i < gf.TotalMediaFileExt; i++)
                {
                    string ext = gf.MediaFileExtension[i, 0].ToLower();
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
            openFileDialog1.InitialDirectory = gf.DocumentsDir;
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
        /// daniel
        /// ??��??docx ��??
        /// </summary>
        /// <param name="FullPathFileName"></param>
        /// <param name="AddToLocation"></param>
        /// <returns></returns>
        private bool AddExternalFileToWorshipList(string FullPathFileName, int AddToLocation)
        {
            ListViewItem listViewItem = new ListViewItem();
            string text = gf.GetDisplayNameOnly(ref FullPathFileName, UpdateByRef: false);
            string musicTitle = text;
            string musicTitle2 = "";
            string text2 = "";
            int imageIndex = 0;
            string text3 = Path.GetExtension(FullPathFileName).ToLower();//daniel DataUtil.Right(FullPathFileName, 4);
            bool preloadPowerpoint = false;
            if (AddToLocation < 0 || AddToLocation > WorshipListItems.Items.Count)
            {
                AddToLocation = WorshipListItems.Items.Count;
            }
            if (text3 == ".ppt" || text3 == ".pptx")
            {
                text2 = "P";
                imageIndex = 2;
                if (gf.ShowRunning)
                {
                    preloadPowerpoint = true;
                }
            }
            else if (text3 == ".txt")
            {
                text2 = "T";
                imageIndex = 6;
            }
            else if (text3 == ".doc" || text3 == ".docx")
            {
                text2 = "W";
                imageIndex = 10;
            }
            else if (text3 == ".esi")
            {
                text2 = "I";
                imageIndex = 8;
                string InTitle = "";
                gf.LoadIndividualData(ref gf.TempItem1, text2 + FullPathFileName, "", 1, ref InTitle);
                musicTitle = gf.TempItem1.Title;
                musicTitle2 = gf.TempItem1.Title2;
            }
            else if (ValidMediaExt(text3))
            {
                text2 = "M";
                imageIndex = 28;
            }
            else if (text3 == ".esw")
            {
                bool flag = InsertIndexFileItems(FullPathFileName, ref WorshipListItems, AddToLocation, ref gf.CurSessionNotes);
                if (flag)
                {
                    SaveWorshipList(preloadPowerpoint);
                }
                return flag;
            }
            if (text2 != "")
            {
                if (gf.MusicFound(musicTitle, musicTitle2))
                {
                    text += " <#>";
                }
                listViewItem = WorshipListItems.Items.Insert(AddToLocation, text);
                listViewItem.ImageIndex = imageIndex;
                listViewItem.SubItems.Add(text2 + FullPathFileName);
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
            for (int i = 0; i < gf.TotalMediaFileExt; i++)
            {
                if (InExt.ToLower() == gf.MediaFileExtension[i, 0].ToLower())
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
            //PreviewPPTotalImagesCount = gf.PreviewItem.TotalSlides;
            //if (PreviewPPTotalImagesCount > 1024)
            //	PreviewPPTotalImagesCount = 1024;

            //for (int i = 0; i < PreviewPPTotalImagesCount; i++)
            //{
            //	PreviewPPImagename[i] = "";
            //	PreviewPPTIHoldersFileName[i] = "";
            //	PreviewPPSlideNumber[i] = -1;
            //	PreviewPPImagename[i] = gf.PREPPFullPath + Convert.ToString(i + 1) + ".jpg";
            //}

            PreviewPPTotalImagesCount = gf.PreviewItem.TotalSlides;

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
                PreviewPPImagename[i] = gf.PREPPFullPath + Convert.ToString(i + 1) + ".jpg";
            }
            SetPreviewPPThumbImages1(GotoSlide);
        }

        /// <summary>
        /// daniel
        /// </summary>
        /// <param name="GotoSlide"></param>
        private void SetPreviewPPThumbImages1(int GotoSlide)
        {
            if (gf.PreviewItem.Type == "P")
            {
                if (GotoSlide < 0)
                {
                    GotoSlide = gf.PreviewItem.TotalSlides;
                }
                LoadThumbPreviewImages(flowLayoutPreviewPowerPoint, ref Powerpoint_PreviewCanvas, PreviewPPImagename, gf.PreviewItem.TotalSlides, flowLayoutPreviewPowerPoint.Width, gf.OUTPPFullPath, GotoSlide, toolTip1, ExternalPP: true);
            }
        }

        //private void SetPreviewPPThumbImages(int GotoSlide)
        //{
        //	if (gf.PreviewItem.Type == "P")
        //	{
        //		if (GotoSlide < 0)
        //		{
        //			GotoSlide = gf.PreviewItem.TotalSlides;
        //		}
        //		LoadThumbImages(flowLayoutPreviewPowerPoint, ref Powerpoint_PreviewCanvas, PreviewPPImagename, gf.PreviewItem.TotalSlides, flowLayoutPreviewPowerPoint.Width, gf.OUTPPFullPath, GotoSlide, toolTip1, ExternalPP: true);
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
                    int num = (InItem.CurSlide < 0) ? InItem.TotalSlides : InItem.CurSlide;
                    InItem.Format.BackgroundPicture = (InItem.OutputStyleScreen ? gf.OUTPPFullPath : gf.PREPPFullPath) + num + ".jpg";
                    InItem.UseDefaultFormat = false;
                    InItem.Format.BackgroundMode = ImageMode.BestFit;
                    gf.DrawText(ref InItem, ref PreviewScreen, ref OutputScreen, InItem.LyricsAndNotationsList);
                    if (InItem.OutputStyleScreen)
                    {
                        gf.SetShowBackground(InItem, ref OutputScreen);
                    }
                    else
                    {
                        gf.SetShowBackground(InItem, ref PreviewScreen);
                    }
                    gf.DrawText(ref InItem, ref PreviewScreen, ref OutputScreen, InItem.LyricsAndNotationsList);
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
        private bool isActivated;

        /// <summary>
        /// daniel
        /// </summary>
        /// <param name="GotoSlide"></param>
        private void ShowOutputPPThumbs(int GotoSlide)
        {
            bool isCurrentOutputPPImageSame = false;
            OutputPPTotalImagesCount = gf.OutputItem.TotalSlides;

            if (!String.IsNullOrEmpty(previousOutputPPImageName))
            {
                if (previousOutputPPImageName != gf.OUTPPFullPath + Convert.ToString(1) + ".jpg")
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
                    OutputPPImagename[i] = gf.OUTPPFullPath + Convert.ToString(i + 1) + ".jpg";
                }
            }

            //OutputPPTotalImagesCount = gf.PreviewItem.TotalSlides;
            //if (OutputPPTotalImagesCount > 1024)
            //	OutputPPTotalImagesCount = 1024;

            //for (int i = 0; i < OutputPPTotalImagesCount; i++)
            //{
            //	OutputPPImagename[i] = "";
            //	OutputPPTIHoldersFileName[i] = "";
            //	OutputPPSlideNumber[i] = -1;
            //	OutputPPImagename[i] = gf.OUTPPFullPath + Convert.ToString(i + 1) + ".jpg"; ;
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
            if (gf.OutputItem.Type == "P")
            {
                if (GotoSlide < 0)
                {
                    GotoSlide = gf.OutputItem.TotalSlides;
                }
                LoadThumbOutImages(flowLayoutOutputPowerPoint, ref Powerpoint_OutputCanvas, OutputPPImagename, gf.OutputItem.TotalSlides, flowLayoutOutputPowerPoint.Width, gf.OUTPPFullPath, GotoSlide, toolTip1, ExternalPP: true);
            }
        }

        //private void SetOutputPPThumbImages2(int GotoSlide)
        //{
        //	if (gf.OutputItem.Type == "P")
        //	{
        //		if (GotoSlide < 0)
        //		{
        //			GotoSlide = gf.OutputItem.TotalSlides;
        //		}
        //		LoadThumbImages(flowLayoutOutputPowerPoint, ref Powerpoint_OutputCanvas, OutputPPImagename, gf.OutputItem.TotalSlides, flowLayoutOutputPowerPoint.Width, gf.OUTPPFullPath, GotoSlide, toolTip1, ExternalPP: true);
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
                    gf.DrawText(ref InItem, ref OutputScreen, ref OutputScreen, InItem.LyricsAndNotationsList);
                    gf.SetShowBackground(InItem, ref OutputScreen);
                    gf.DrawText(ref InItem, ref OutputScreen, ref OutputScreen, InItem.LyricsAndNotationsList);
                }
                catch
                {
                }
            }
        }

        private string DragDropItemType(DragEventArgs e)
        {
            int num = e.Data.GetFormats().Length - 1;
            for (int i = 0; i <= num; i++)
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

        private void SongsList_ItemDrag(object sender, ItemDragEventArgs e)
        {
            DragListView = DragDropSource.SongsList.ToString();
            SongsList.DoDragDrop(SongsList.SelectedItems, DragDropEffects.Link);
        }

        private void InfoScreenList_ItemDrag(object sender, ItemDragEventArgs e)
        {
            DragListView = DragDropSource.InfoScreenList.ToString();
            InfoScreenList.DoDragDrop(SongsList.SelectedItems, DragDropEffects.Link);
        }

        private void PowerpointList_ItemDrag(object sender, ItemDragEventArgs e)
        {
            DragListView = DragDropSource.PowerpointList.ToString();
            PowerpointList.DoDragDrop(SongsList.SelectedItems, DragDropEffects.Link);
        }

        private void MediaList_ItemDrag(object sender, ItemDragEventArgs e)
        {
            DragListView = DragDropSource.MediaList.ToString();
            MediaList.DoDragDrop(SongsList.SelectedItems, DragDropEffects.Link);
        }

        private void WorshipList_ItemDrag(object sender, ItemDragEventArgs e)
        {
            DragListView = DragDropSource.WorshipList.ToString();
            WorshipListItems.DoDragDrop(WorshipListItems.SelectedItems, DragDropEffects.Link);
        }

        /// <summary>
        /// daniel
        /// ??��??docx ��??
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WorshipList_DragEnter(object sender, DragEventArgs e)
        {
            string a = DragDropItemType(e);
            if (a == DragDropSource.WorshipList.ToString())
            {
                e.Effect = DragDropEffects.Link;
            }
            else if (a == DragDropSource.SongsList.ToString())
            {
                e.Effect = DragDropEffects.Link;
            }
            else if (a == DragDropSource.InfoScreenList.ToString())
            {
                e.Effect = DragDropEffects.Link;
            }
            else if (a == DragDropSource.PowerpointList.ToString())
            {
                e.Effect = DragDropEffects.Link;
            }
            else if (a == DragDropSource.MediaList.ToString())
            {
                e.Effect = DragDropEffects.Link;
            }
            else if (a == DataFormats.FileDrop)
            {
                bool flag = true;
                string[] array = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (!gf.SupportedImageFormat(array[0]))
                {
                    for (int i = 0; i <= array.Length - 1; i++)
                    {
                        string strExt = Path.GetExtension(array[i]).ToLower();
                        bool isPowerpoint = strExt == ".ppt" || strExt == ".pptx";
                        bool isDoc = strExt == ".doc" || strExt == ".docx";
                        bool isText = strExt == ".txt";
                        bool isInfo = strExt == ".esi";
                        bool isWorship = strExt == ".esw";
                        bool isMedia = ValidMediaExt(strExt);
                        if ((!isPowerpoint && !isDoc && !isText && !isInfo && !isWorship && !isMedia) || (isPowerpoint && gf.ShowRunning))
                        {
                            flag = false;
                        }
                    }
                }
                e.Effect = (flag ? DragDropEffects.Link : DragDropEffects.None);
            }
            else if (a == DragDropSource.BiblePassage.ToString())
            {
                e.Effect = DragDropEffects.Link;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void WorshipList_DragOver(object sender, DragEventArgs e)
        {
            Point point = default(Point);
            point = WorshipListItems.PointToClient(Cursor.Position);
            ListViewItem itemAt = WorshipListItems.GetItemAt(point.X, point.Y);
            if (itemAt == null)
            {
                return;
            }
            WorshipListInsertAt = -1;
            if (point.Y >= itemAt.Bounds.Height + itemAt.Bounds.Y - 1)
            {
                if (WorshipListItems.Items.Count > 0)
                {
                    WorshipListItems.Items[WorshipListItems.Items.Count - 1].BackColor = SongFolder.BackColor;
                }
            }
            else if (WorshipListItems.Items.Count > 0)
            {
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
        }

        private void WorshipList_DragDrop(object sender, DragEventArgs e)
        {
            int num = -1;
            int num2 = 0;
            string a = DragDropItemType(e);
            if (a == DragDropSource.WorshipList.ToString())
            {
                e.Effect = DragDropEffects.Link;
                num = ((WorshipListInsertAt >= 0) ? WorshipListInsertAt : WorshipListItems.Items.Count);
                num2 = WorshipListItems.SelectedItems.Count;
                int num3 = 0;
                if (num > 0)
                {
                    for (int i = 0; i < num; i++)
                    {
                        if (WorshipListItems.Items[i].Selected)
                        {
                            num3++;
                        }
                    }
                }
                int num4 = 0;
                foreach (ListViewItem selectedItem in WorshipListItems.SelectedItems)
                {
                    try
                    {
                        WorshipListItems.Items.Insert(num + num4, (ListViewItem)selectedItem.Clone());
                        num4++;
                    }
                    catch
                    {
                    }
                }
                RemoveWorshipListSong(UpdateCurItemNo: false);
                num -= num3;
                if (num > 0)
                {
                    for (int i = num; i < num2 + num; i++)
                    {
                        WorshipListItems.Items[i].Selected = true;
                    }
                }
                SaveWorshipList();
            }
            else if (a == DragDropSource.SongsList.ToString())
            {
                e.Effect = DragDropEffects.Copy;
                num = WorshipListInsertAt;
                num2 = SongsList.SelectedItems.Count;
                AddFromSongsList(WorshipListInsertAt);
            }
            else if (a == DragDropSource.InfoScreenList.ToString())
            {
                e.Effect = DragDropEffects.Copy;
                num = WorshipListInsertAt;
                num2 = SongsList.SelectedItems.Count;
                AddFromInfoScreensList(WorshipListInsertAt);
            }
            else if (a == DragDropSource.PowerpointList.ToString())
            {
                e.Effect = DragDropEffects.Copy;
                num = WorshipListInsertAt;
                num2 = SongsList.SelectedItems.Count;
                AddFromPowerpointList(WorshipListInsertAt);
            }
            else if (a == DragDropSource.MediaList.ToString())
            {
                e.Effect = DragDropEffects.Copy;
                num = WorshipListInsertAt;
                num2 = SongsList.SelectedItems.Count;
                AddFromMediaFilesList(WorshipListInsertAt);
            }
            else if (a == DataFormats.FileDrop)
            {
                bool flag = true;
                string[] array = (string[])e.Data.GetData(DataFormats.FileDrop);
                num = WorshipListInsertAt;
                num2 = array.Length;
                if (gf.SupportedImageFormat(array[0]))
                {
                    if (WorshipListInsertAt < 0)
                    {
                        WorshipList_UnselectAll();
                    }
                    else
                    {
                        WorshipListItems.Items[WorshipListInsertAt].Selected = true;
                        WorshipListIndexChanged();
                    }
                    ApplyBackground(array[0]);
                    num2 = 1;
                }
                else
                {
                    int num4 = 0;
                    for (int i = 0; i <= array.Length - 1; i++)
                    {
                        AddExternalFileToWorshipList(array[i], WorshipListInsertAt + num4);
                        num4++;
                    }
                    if (array.Length > 0)
                    {
                        SelectWorshipListItem(WorshipListInsertAt, num4);
                    }
                }
            }
            else if (a == DragDropSource.BiblePassage.ToString())
            {
                AddFromHolyBible(WorshipListInsertAt);
            }
            ResetListViewBackgroundColour(WorshipListItems);
            WorshipListIndexChanged();
            UpdateOutputCurItemNo();
        }

        private void WorshipList_DragLeave(object sender, EventArgs e)
        {
            ResetListViewBackgroundColour(WorshipListItems);
            WorshipListInsertAt = -1;
        }

        private void PraiseBookItems_DragEnter(object sender, DragEventArgs e)
        {
            string a = DragDropItemType(e);
            if (a == "SongsList")
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
            string a = DragDropItemType(e);
            if (a == "SongsList")
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
            if (gf.PreviewItem.ItemID != "")
            {
                CopyPreviewToOutput();
            }
            ManualMoveToItem(gf.PreviewItem, KeyDirection.NextOne);
            FocusOutputArea();
            GoLive(InStatus: true);
        }

        private void PreviewItemToLive()
        {
            if (gf.ShowRunning)
            {
                if (gf.PreviewItem.ItemID != "")
                {
                    CopyPreviewToOutput();
                }
            }
            else if (gf.PreviewItem.ItemID != "")
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
                if (!gf.ShowRunning)
                {
                    Menu_StartShow.Checked = InStatus;
                    Start_Presentation();
                }
            }
            else
            {
                cbGoLive.ForeColor = DefaultButtonForeColour;
                if (gf.ShowRunning)
                {
                    gf.ShowRunning = false;
                    LiveShow.StopShow();
                    UpdateWorshipShowIcons();
                }
            }
            if (gf.DualMonitorMode)
            {
                FocusOutputArea();
                Focus();
            }
        }

        private void Start_Presentation()
        {
            if (gf.OutputItem.ItemID == "")
            {
                gf.StartPresAt = 1;
                LoadWorshipListItemToOutput(gf.StartPresAt, 0);
                if (gf.OutputItem.ItemID == "")
                {
                    GoLive(InStatus: false);
                    MessageBox.Show("Can't start the Show because the Worship List is empty!");
                    return;
                }
            }
            if (ValidateWorshipListItems(ShowErrorMessage: true) < 0)
            {
                gf.ShowRunning = true;
                gf.ResetShowRunningSettings();
                UpdateWorshipShowIcons();
                gf.SaveShowOptions = false;
                gf.OutputItem.GapItemOnDisplay = false;
                DisplayInfo.SizeLaunchDisplay();
                FormWindowState formWindowState;
                if (gf.DualMonitorMode)
                {
                    gf.LaunchShowUpdateDone = false;
                    formWindowState = (base.WindowState = base.WindowState);
                    gf.PreLoadPowerpointFiles(ref gf.LivePP, ref gf.WorshipSongs);
                    try
                    {
                        LiveShow.Show();
                    }
                    catch
                    {
                        try
                        {
                            LiveShow = new FrmLaunchShow();
                            LiveShow.OnMessage += LiveShow_OnMessage;
                            LiveShow.Show();
                        }
                        catch
                        {
                        }
                    }
                    FocusOutputArea();
                    return;
                }
                MinAllWindows();
                TimerReMax.Enabled = true;
                formWindowState = base.WindowState;
                base.WindowState = FormWindowState.Minimized;
                gf.PreLoadPowerpointFiles(ref gf.LivePP, ref gf.WorshipSongs);
                try
                {
                    LiveShow.ShowDialog();
                }
                catch
                {
                    try
                    {
                        LiveShow = new FrmLaunchShow();
                        LiveShow.OnMessage += LiveShow_OnMessage;
                        LiveShow.ShowDialog();
                    }
                    catch
                    {
                    }
                }
                gf.ParentalAlertLive = false;
                gf.MessageAlertLive = false;
                if (gf.OutputItem.CurItemNo == 0)
                {
                    CopyPreviewToOutput();
                }
                else if (gf.LiveItem.Type == "G")
                {
                    LoadItem(ref gf.OutputItem, "G1", "", 0);
                }
                else
                {
                    LoadWorshipListItemToOutput(gf.StartPresAt, gf.OutputItem.CurSlide, NotifyLiveShow: false);
                }
                base.WindowState = formWindowState;
                UpdateWorshipShowIcons();
                Menu_StartShow.Checked = false;
                cbGoLive.Checked = false;
                cbGoLive.ForeColor = DefaultButtonForeColour;
                SetAutoRotateButtons();
                LiveBlack(gf.ShowLiveBlack);
                LiveClear(gf.ShowLiveClear);
                //LiveCam(gf.ShowLiveCam);
                if (gf.SaveShowOptions)
                {
                    SaveWorshipList();
                }
            }
            else
            {
                GoLive(InStatus: false);
            }
        }

        private void OldStart_Presentation()
        {
            if (gf.ShowRunning)
            {
                return;
            }
            if (gf.OutputItem.ItemID == "")
            {
                gf.StartPresAt = 1;
                LoadWorshipListItemToOutput(gf.StartPresAt, 0);
                if (gf.OutputItem.ItemID == "")
                {
                    GoLive(InStatus: false);
                    MessageBox.Show("Can't start the Show because the Worship List is empty!");
                    return;
                }
            }
            Cursor.Current = Cursors.WaitCursor;
            if (ValidateWorshipListItems(ShowErrorMessage: true) < 0)
            {
                gf.ShowRunning = true;
                gf.ResetShowRunningSettings();
                UpdateWorshipShowIcons();
                gf.SaveShowOptions = false;
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
                if (gf.DualMonitorMode)
                {
                    gf.LaunchShowUpdateDone = false;
                    formWindowState = (base.WindowState = base.WindowState);
                    gf.PreLoadPowerpointFiles(ref gf.LivePP, ref gf.WorshipSongs);
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
                gf.PreLoadPowerpointFiles(ref gf.LivePP, ref gf.WorshipSongs);
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
                gf.ParentalAlertLive = false;
                gf.MessageAlertLive = false;
                if (gf.OutputItem.CurItemNo == 0)
                {
                    CopyPreviewToOutput();
                }
                else if (gf.LiveItem.Type == "G")
                {
                    LoadItem(ref gf.OutputItem, "G1");
                }
                else
                {
                    LoadWorshipListItemToOutput(gf.StartPresAt, gf.OutputItem.CurSlide, NotifyLiveShow: false);
                }
                base.WindowState = formWindowState;
                UpdateWorshipShowIcons();
                Menu_StartShow.Checked = false;
                cbGoLive.Checked = false;
                cbGoLive.ForeColor = DefaultButtonForeColour;
                SetAutoRotateButtons();
                LiveBlack(gf.ShowLiveBlack);
                LiveClear(gf.ShowLiveClear);
                //LiveCam(gf.ShowLiveCam);
                if (gf.SaveShowOptions)
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

        private void LiveBlack(bool InStatus)
        {
            try
            {
                gf.ShowLiveBlack = InStatus;
                cbOutputBlack.Checked = gf.ShowLiveBlack;
                Menu_BlackScreen.Checked = gf.ShowLiveBlack;
                PostitionBlackClearGapLabels();
                RedrawOutputItemText();
                cbOutputBlack.ImageIndex = (cbOutputBlack.Checked ? 16 : 15);
                if (gf.ShowRunning)
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
            gf.ShowLiveClear = InStatus;
            cbOutputClear.Checked = gf.ShowLiveClear;
            Menu_ClearScreen.Checked = gf.ShowLiveClear;
            PostitionBlackClearGapLabels();
            RedrawOutputItemText();
            cbOutputClear.ImageIndex = (cbOutputClear.Checked ? 18 : 17);
            if (gf.ShowRunning)
            {
                RemoteControlLiveShow(LiveShowAction.Remote_LiveBlackClearChanged);
            }
            FocusOutputArea();
        }

        private void PostitionBlackClearGapLabels()
        {
            if (gf.ShowLiveBlack)
            {
                labelBlackScreen.Left = 0;
            }
            labelBlackScreen.Visible = (gf.ShowLiveBlack ? true : false);
            if (gf.ShowLiveClear)
            {
                labelHideText.Left = (gf.ShowLiveBlack ? labelBlackScreen.Width : 0);
            }
            labelHideText.Visible = (gf.ShowLiveClear ? true : false);
            if (gf.OutputItem.Type == "G")
            {
                labelGapItem.Left = (gf.ShowLiveBlack ? labelBlackScreen.Width : 0) + (gf.ShowLiveClear ? labelHideText.Width : 0);
            }
            labelGapItem.Visible = ((gf.OutputItem.Type == "G") ? true : false);
            ResizeOutputBottomPanel();
        }

        //private void LiveCam(bool InStatus)
        //{
        //    gf.ShowLiveCam = InStatus;
        //    cbOutputCam.Checked = gf.ShowLiveCam;
        //    Menu_LiveCam.Checked = gf.ShowLiveCam;
        //    cbOutputCam.ImageIndex = (cbOutputCam.Checked ? 31 : 30);
        //    if (gf.ShowRunning)
        //    {
        //        RemoteControlLiveShow(LiveShowAction.Remote_LiveCamStartStop);
        //    }
        //    FocusOutputArea();
        //}

        private void RedrawOutputItemText()
        {
            ShowSlide(ref gf.OutputItem, gf.GapItemUseFade ? ImageTransitionControl.TransitionAction.AsFade : ImageTransitionControl.TransitionAction.None);
        }

        /// <summary>
        /// daniel
        /// </summary>
        /// <param name="ShowErrorMessage"></param>
        /// <returns></returns>
        private int ValidateWorshipListItems(bool ShowErrorMessage)
        {
            bool flag = false;
            bool flag2 = false;
            string text = "";
            int num = 0;

#if OleDb
			using OleDbConnection connection = DbConnectionController.GetOleDbConnection(gf.ConnectStringMainDB);
#elif SQLite
            using DbConnection connection = DbController.GetDbConnection(gf.ConnectStringMainDB);
#endif

            gf.AdHocItemPresent = ((gf.OutputItem.Source != ItemSource.WorshipList) ? true : false);
            for (int i = 0; i <= WorshipListItems.Items.Count - ((!gf.AdHocItemPresent) ? 1 : 0); i++)
            {
                string text2;
                string text3;
                string inString;
                int num2;
                if (gf.AdHocItemPresent & (i == WorshipListItems.Items.Count))
                {
                    text2 = gf.OutputItem.InMainItemText;
                    text3 = gf.OutputItem.InSubItemItem1Text;
                    inString = gf.OutputItem.Format.FormatString;
                    num2 = 0;
                }
                else
                {
                    text2 = WorshipListItems.Items[i].Text;
                    text3 = WorshipListItems.Items[i].SubItems[1].Text;
                    inString = WorshipListItems.Items[i].SubItems[2].Text;
                    num2 = i + 1;
                }
                text = DataUtil.Left(text3, 1);
                string text4 = DataUtil.Right(text3, text3.Length - 1);
                if (text == "D")
                {
                    bool flag3 = false;
                    int num3 = 0;
                    try
                    {
#if OleDb
						using OleDbCommand comm = new OleDbCommand($"Select FolderNo From SONG Where SONGID={text4}", connection);
#elif SQLite
                        using DbCommand comm = new DbCommand($"Select FolderNo From SONG Where SONGID={text4}", connection);
#endif

                        object objValue = comm.ExecuteScalar();

                        if (objValue != null)
                        {
                            gf.WorshipSongs[num2, 0] = text3;
                            gf.WorshipSongs[num2, 1] = "D";
                            gf.WorshipSongs[num2, 2] = gf.RemoveMusicSym(text2);
                            gf.WorshipSongs[num2, 3] = " ";
                            gf.WorshipSongs[num2, 4] = DataUtil.Trim(inString);
                            num3 = DataUtil.ObjToInt(objValue);
                            flag3 = true;
                        }
                    }
                    catch
                    {
                    }
                    if (!flag3)
                    {
                        if (ShowErrorMessage)
                        {
                            MessageBox.Show("Sorry - Can't find the song '" + gf.RemoveMusicSym(text2) + "' in the Database!");
                        }
                        return i;
                    }
                    if (num3 == 0)
                    {
                        if (ShowErrorMessage)
                        {
                            MessageBox.Show("Sorry - Can't show the Database song '" + gf.RemoveMusicSym(text2) + "' because it has been deleted!");
                        }
                        return i;
                    }
                }
                else if (text == "P")
                {
                    if (!flag)
                    {
                        if (!gf.PowerpointPresent())
                        {
                            if (ShowErrorMessage)
                            {
                                MessageBox.Show("Error - You have a Powerpoint item but there is no Microsoft Powerpoint Software on your computer");
                            }
                            return i;
                        }
                        flag = true;
                    }
                    string text5 = text4;
                    if (!File.Exists(text5))
                    {
                        if (ShowErrorMessage)
                        {
                            MessageBox.Show("Sorry - Can't find the Powerpoint File '" + gf.RemoveMusicSym(text2) + "'");
                        }
                        return i;
                    }
                    flag2 = true;
                    num++;
                    gf.WorshipSongs[num2, 0] = "P" + text5;
                    gf.WorshipSongs[num2, 1] = "P";
                    gf.WorshipSongs[num2, 2] = gf.RemoveMusicSym(text2);
                    gf.WorshipSongs[num2, 3] = " ";
                    gf.WorshipSongs[num2, 4] = "";
                }
                else if (text == "B")
                {
                    gf.WorshipSongs[num2, 0] = text3;
                    gf.WorshipSongs[num2, 1] = "B";
                    gf.WorshipSongs[num2, 2] = text2;
                    gf.WorshipSongs[num2, 3] = " ";
                    gf.WorshipSongs[num2, 4] = DataUtil.Trim(inString);
                }
                else if (text == "T")
                {
                    gf.WorshipSongs[num2, 0] = text3;
                    gf.WorshipSongs[num2, 1] = "T";
                    gf.WorshipSongs[num2, 2] = text2;
                    gf.WorshipSongs[num2, 3] = " ";
                    gf.WorshipSongs[num2, 4] = DataUtil.Trim(inString);
                }
                else if (text == "I")
                {
                    gf.WorshipSongs[num2, 0] = text3;
                    gf.WorshipSongs[num2, 1] = "I";
                    gf.WorshipSongs[num2, 2] = text2;
                    gf.WorshipSongs[num2, 3] = " ";
                    gf.WorshipSongs[num2, 4] = DataUtil.Trim(inString);
                }
                else if (text == "W")
                {
                    gf.WorshipSongs[num2, 0] = text3;
                    gf.WorshipSongs[num2, 1] = "W";
                    gf.WorshipSongs[num2, 2] = text2;
                    gf.WorshipSongs[num2, 3] = " ";
                    gf.WorshipSongs[num2, 4] = DataUtil.Trim(inString);
                }
                else if (text == "M")
                {
                    gf.WorshipSongs[num2, 0] = text3;
                    gf.WorshipSongs[num2, 1] = "M";
                    gf.WorshipSongs[num2, 2] = text2;
                    gf.WorshipSongs[num2, 3] = " ";
                    gf.WorshipSongs[num2, 4] = DataUtil.Trim(inString);
                }
            }

            gf.TotalWorshipListItems = WorshipListItems.Items.Count;
            if (num > gf.PP_MaxFiles)
            {
                MessageBox.Show("Error - There are " + num + " Powerpoint files on the Worship List but you are only permitted to have " + gf.PP_MaxFiles + ".");
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
            gf.WorshipListsChanged = false;
            gf.PraiseBooksListChanged = false;
            gf.Def_FormatString = LoadWorshipList(2, gf.WorshipTemplatesDir + "Default.est");
            if (gf.Def_FormatString == "")
            {
                gf.Def_FormatString = " ";
            }
            FrmManageItemLists frmManageItemLists = new FrmManageItemLists();
            frmManageItemLists.ShowDialog();
            if (gf.PraiseBooksListChanged)
            {
                string curPraiseBook = gf.CurPraiseBook;
                PopulatePraiseBooksList();
                gf.CurPraiseBook = curPraiseBook;
                gf.PraiseBooksListChanged = false;
            }
            if (gf.CurSession != SessionList.Text)
            {
                WriteCurSession();
                PopulateWorshipList();
                LoadWorshipList(0);
                SessionList.Text = gf.CurSession;
            }
            else if (gf.WorshipListsChanged)
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
            gf.PraiseBooksListChanged = false;
            gf.WorshipListsChanged = false;
            gf.CurPraiseBook = PraiseBook.Text;
            gf.Def_FormatString = "";
            FrmManageItemLists frmManageItemLists = new FrmManageItemLists();
            frmManageItemLists.ShowDialog();
            if (gf.WorshipListsChanged)
            {
                string curSession = gf.CurSession;
                PopulateWorshipList();
                gf.CurSession = curSession;
                gf.WorshipListsChanged = false;
            }
            if (gf.CurPraiseBook != PraiseBook.Text)
            {
                WriteCurPraiseBook();
                PopulatePraiseBooksList();
                LoadPraiseBook(0);
                PraiseBook.Text = gf.CurPraiseBook;
            }
            else if (gf.PraiseBooksListChanged)
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
            gf.RunProcess(gf.RootEasiSlidesDir);
        }

        private void Menu_StatusBar_Click(object sender, EventArgs e)
        {
            Menu_StatusBar.Checked = !Menu_StatusBar.Checked;
            statusStripMain.Visible = Menu_StatusBar.Checked;
        }

        private void Menu_PreviewNotations_Click(object sender, EventArgs e)
        {
            PreviewNotations(Menu_PreviewNotations.Checked);
        }

        private void PreviewNotations(bool ShowNotations)
        {
            gf.PreviewArea_ShowNotations = ShowNotations;
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
            GoLive(!gf.ShowRunning);
        }

        private void Menu_ClearScreen_Click(object sender, EventArgs e)
        {
            LiveClear(!gf.ShowLiveClear);
        }

        //private void Menu_LiveCam_Click(object sender, EventArgs e)
        //{
        //    //LiveCam(!gf.ShowLiveCam);
        //}

        private void Menu_RestartCurrentItem_Click(object sender, EventArgs e)
        {
            SetRotateState(RotateOn: false);
            gf.RestartItemActioned = false;
            MoveToItem(gf.OutputItem, KeyDirection.Refresh, 1, NotifyLiveShow: true);
        }

        private void Menu_SwitchChinese_Click(object sender, EventArgs e)
        {
            OutputChineseSwitch();
        }

        private void OutputChineseSwitch()
        {
            if (!(gf.OutputItem.CompleteLyrics == ""))
            {
                int num = gf.SwitchChinese(ref gf.OutputItem.CompleteLyrics);
                gf.SwitchChineseLyricsNotationListView(ref gf.OutputItem, num);
                DisplayLyrics(gf.OutputItem, gf.OutputItem.CurSlide, ScrollToCaret: true, 2, ImageTransitionControl.TransitionAction.None);
                if (gf.ShowRunning && num >= 0)
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
            gf.ParentalAlertLive = false;
            gf.MessageAlertLive = false;
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
            gf.SelectedItemsCount = SongsList.SelectedItems.Count;
            if (SongsList.SelectedItems.Count == 0 || gf.SelectedItemsCount == 0)
            {
                return;
            }
            FrmCopy frmCopy = new FrmCopy();
            if (frmCopy.ShowDialog() != DialogResult.OK)
            {
                return;
            }
            int num = 0;
            int[] RefileSongs = new int[30000];
            RefileSongs[0] = 0;
            string[] array = new string[30000];
            array[0] = "0";
            string[] inHeaderData = new string[255];
            int num2 = 0;
            string text = "";
            if (gf.CopyToFolder < 0)
            {
                num2 = -gf.CopyToFolder - 1;
                text = gf.InfoScreenGroups[num2, 1];
            }
            for (int i = 0; i <= SongsList.Items.Count - 1; i++)
            {
                if (SongsList.Items[i].Selected)
                {
                    try
                    {
                        string text2 = SongsList.Items[i].SubItems[1].Text;
                        string str = DataUtil.Right(text2, text2.Length - 1);
                        string fullSearchString = "select * from SONG where songid=" + str;
#if OleDb
						using DataTable datatable = DbOleDbController.GetDataTable(gf.ConnectStringMainDB, fullSearchString);
#elif SQLite
                        using DataTable datatable = DbController.GetDataTable(gf.ConnectStringMainDB, fullSearchString);
#endif

                        {
                            if (datatable.Rows.Count > 0)
                            {
                                DataRow dr = datatable.Rows[0];
                                if (gf.CopyToFolder > 0)
                                {
                                    num = gf.InsertItemIntoDatabase(gf.ConnectStringMainDB, dr["Title_1"], dr["Title_2"], dr["song_number"], gf.CopyToFolder, dr["Lyrics"], dr["Sequence"], dr["writer"], dr["copyright"], dr["capo"], dr["Timing"], dr["Key"], dr["msc"], dr["CATEGORY"], dr["LICENCE_ADMIN1"], dr["LICENCE_ADMIN2"], dr["BOOK_REFERENCE"], dr["USER_REFERENCE"], dr["SETTINGS"], dr["FORMATDATA"]);
                                    if (num > 0)
                                    {
                                        RefileSongs[0]++;
                                        RefileSongs[RefileSongs[0]] = num;
                                    }
                                }
                                else
                                {
                                    gf.TempItem1.Title = DataUtil.ObjToString(dr["Title_1"]);
                                    gf.TempItem1.FolderNo = DataUtil.ObjToInt(dr["FolderNo"]);
                                    gf.TempItem1.Title2 = DataUtil.ObjToString(dr["Title_2"]);
                                    gf.TempItem1.SongNumber = DataUtil.ObjToInt(dr["song_number"]);
                                    gf.TempItem1.CompleteLyrics = DataUtil.ObjToString(dr["Lyrics"]);
                                    gf.TempItem1.Notations = DataUtil.ObjToString(dr["msc"]);
                                    gf.TempItem1.SongSequence = DataUtil.ObjToString(dr["Sequence"]);
                                    gf.TempItem1.Writer = DataUtil.ObjToString(dr["writer"]);
                                    gf.TempItem1.Copyright = DataUtil.ObjToString(dr["copyright"]);
                                    gf.TempItem1.Category = "";
                                    gf.TempItem1.Timing = DataUtil.ObjToString(dr["Timing"]);
                                    gf.TempItem1.MusicKey = DataUtil.ObjToString(dr["Key"]);
                                    gf.TempItem1.Capo = DataUtil.ObjToInt(dr["capo"]);
                                    gf.TempItem1.Show_LicAdminInfo1 = DataUtil.ObjToString(dr["LICENCE_ADMIN1"]);
                                    gf.TempItem1.Show_LicAdminInfo2 = DataUtil.ObjToString(dr["LICENCE_ADMIN2"]);
                                    gf.TempItem1.Book_Reference = DataUtil.ObjToString(dr["BOOK_REFERENCE"]);
                                    gf.TempItem1.User_Reference = DataUtil.ObjToString(dr["USER_REFERENCE"]);
                                    gf.TempItem1.RotateString = "";
                                    gf.TempItem1.Settings = DataUtil.ObjToString(dr["SETTINGS"]);
                                    string InFileName = text + gf.TempItem1.Title + ".esi";
                                    int num3 = 0;
                                    while (File.Exists(InFileName))
                                    {
                                        num3++;
                                        InFileName = text + gf.TempItem1.Title + " - Copy (" + num3 + ").esi";
                                    }
                                    if (gf.SaveXMLInfoScreen(gf.TempItem1, InFileName, inHeaderData, ReloadImageData: false, UseOriginalNotations: false))
                                    {
                                        array[0] = Convert.ToString(DataUtil.StringToInt(array[0]) + 1);
                                        array[DataUtil.StringToInt(array[0])] = gf.GetDisplayNameOnly(ref InFileName, UpdateByRef: false);
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
            if (gf.CopyToFolder > 0)
            {
                HighlightCopyMoveItems(gf.CopyToFolder, ref RefileSongs);
            }
            else
            {
                HighlightCopyMoveExternalItems(InfoScreenList, num2, array, "I");
            }
        }

        private void Move_Song()
        {
            gf.SelectedItemsCount = SongsList.SelectedItems.Count;
            if (gf.SelectedItemsCount == 0)
            {
                return;
            }
            FrmMove frmMove = new FrmMove();
            if (frmMove.ShowDialog() != DialogResult.OK)
            {
                return;
            }
            int folderNumber = gf.GetFolderNumber(SongFolder.Text);
            int[] RefileSongs = new int[30000];
            if (gf.MoveToFolder > 0)
            {
                RefileSongs[0] = 0;
                if (gf.ReFileSelectedSongs(ref SongsList, folderNumber, gf.MoveToFolder, ref RefileSongs, UpdateModifiedDate: false) > 0)
                {
                    HighlightCopyMoveItems(gf.MoveToFolder, ref RefileSongs);
                }
            }
        }

        private void RemoveSongsListSong()
        {
            gf.SelectedItemsCount = SongsList.SelectedItems.Count;
            string text = "";
            if (gf.SelectedItemsCount == 1)
            {
                text = "Are you really sure you want to delete the selected song?";
            }
            else
            {
                if (gf.SelectedItemsCount <= 1)
                {
                    return;
                }
                text = "Are you really sure you want to delete the " + Convert.ToString(gf.SelectedItemsCount) + " songs you have selected?";
            }
            if (MessageBox.Show(text, "Delete Selected Song(s)", MessageBoxButtons.YesNo) != DialogResult.Yes)
            {
                return;
            }
            Cursor = Cursors.WaitCursor;
            int folderNumber = gf.GetFolderNumber(SongFolder.Text);
            int[] RefileSongs = new int[30000];
            int num = 0;
            for (int num2 = SongsList.Items.Count - 1; num2 >= 0; num2--)
            {
                if (SongsList.Items[num2].Selected)
                {
                    num = num2;
                }
            }
            gf.ReFileSelectedSongs(ref SongsList, folderNumber, 0, ref RefileSongs, UpdateModifiedDate: true);
            if (SongsList.Items.Count > num)
            {
                if (num >= 0)
                {
                    SongsList.Items[num].Selected = true;
                }
            }
            else if (SongsList.Items.Count > 0 && ((num - 1 >= 0) & (SongsList.Items.Count >= num - 1)))
            {
                SongsList.Items[num - 1].Selected = true;
            }
            SongsListIndexChanged();
            ShowStatusBarSummary();
            UpdateMenuEditHistory();
            Cursor = Cursors.Default;
        }

        /// <summary>
        /// daniel 
        /// cross thread execption error fix
        /// </summary>
        /// <param name="InTab"></param>
        /// <param name="InName"></param>
        /// <returns></returns>
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
            if (SongFolder.Text == gf.FolderName[InFolderNo])
            {
                SongFolder_Change();
            }
            else
            {
                SongFolder.Text = gf.FolderName[InFolderNo];
            }
            int num = -1;
            int num2 = RefileSongs[0];
            for (int i = 0; i < SongsList.Items.Count; i++)
            {
                if (CompareRefileSongsList(DataUtil.StringToInt(DataUtil.Mid(SongsList.Items[i].SubItems[1].Text, 1)), ref RefileSongs))
                {
                    SongsList.Items[i].Selected = true;
                    if (num < 0)
                    {
                        num2--;
                        num = i;
                    }
                    if (num2 == 0)
                    {
                        break;
                    }
                }
            }
            if (num >= 0)
            {
                SongsList.EnsureVisible(num);
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
            int num = DataUtil.StringToInt(RefileSongs[0]);
            if (num < 1)
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
                gf.PowerpointListingStyle = 0;
                SetPowerpointListingButton();
                PowerpointFolder.SelectedIndex = SelectedFolderIndex;
                ShowPowerpointFolderContents(ShowThumbs: false);
            }
            int num2 = -1;
            for (int i = 0; i < InListView.Items.Count; i++)
            {
                if (CompareRefileSongsList(InListView.Items[i].SubItems[0].Text, ref RefileSongs))
                {
                    InListView.Items[i].Selected = true;
                    if (num2 < 0)
                    {
                        num--;
                        num2 = i;
                    }
                    if (num == 0)
                    {
                        break;
                    }
                }
            }
            if (num2 >= 0)
            {
                InListView.EnsureVisible(num2);
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
            gf.CurMainSelectedFolder = gf.GetFolderNumber(SongFolder.Text);
            gf.Options_MediaExtChanged = false;
            gf.Options_MediaDirChanged = false;
            gf.Options_FolderListChanged = false;
            gf.Options_FolderFormatChanged = false;
            gf.Options_BibleListChanged = false;
            gf.Options_MaxHistoryListChanged = false;
            gf.Options_PreviewAreaChanged = false;
            gf.Options_DMChanged = false;
            FrmOptions frmOptions = new FrmOptions();
            if (frmOptions.ShowDialog() != DialogResult.OK)
            {
                return;
            }
            if (gf.Options_FolderListChanged | gf.Options_MediaDirChanged)
            {
                Refresh();
                BuildFolderList();
                if (gf.Options_MediaDirChanged)
                {
                    BuildMediaFolderList();
                }
            }
            if (gf.Options_FolderFormatChanged && gf.PreviewItem.Type == "D")
            {
                if (gf.PreviewItem.Source == ItemSource.SongsList)
                {
                    SongsListIndexChanged();
                }
                else if (gf.PreviewItem.Source == ItemSource.WorshipList)
                {
                    WorshipListIndexChanged();
                }
            }
            if (gf.Options_BibleListChanged)
            {
                Refresh();
                gf.LoadBibleVersions(ref TabBibleVersions);
                gf.LoadBibleBooksList(TabBibleVersions, ref BookLookup, ShowSearchResultsLine: false, BibleText);
                gf.Options_BibleListChanged = false;
            }
            if (gf.Options_MaxHistoryListChanged)
            {
                gf.Options_MaxHistoryListChanged = false;
                gf.SaveMainEditHistoryToRegistry();
                UpdateMenuEditHistory();
                gf.Edit_HistoryMaxChanged = true;
            }
            if (gf.Options_PreviewAreaChanged)
            {
                PreviewNotations(gf.PreviewArea_ShowNotations);
            }
            if (gf.Options_DMChanged)
            {
                SetMainDefaultBackScreen();
                LoadItem(ref gf.PreviewItem, gf.PreviewItem.Type + gf.PreviewItem.ItemID, gf.PreviewItem.Format.FormatString, gf.PreviewItem.CurSlide);
                LoadItem(ref gf.OutputItem, gf.OutputItem.Type + gf.OutputItem.ItemID, gf.OutputItem.Format.FormatString, gf.OutputItem.CurSlide);
            }
            //if (gf.ShowRunning && gf.ShowLiveCam)
            //{
            //    RemoteControlLiveShow(LiveShowAction.Remote_LiveCamUpdate);
            //}
            SetTabsVisibility();
            SetJumpToolTips();
            ShowStatusBarSummary();
        }

        private void SetJumpToolTips()
        {
            int iLen = 15;
            string text = "FOLDER ";
            string value = "FOLDER";
            string text2 = gf.FolderName[gf.JumpToA] ??= "";
            string text3 = gf.FolderName[gf.JumpToB] ??= "";
            string text4 = gf.FolderName[gf.JumpToC] ??= "";
            string text5 = DataUtil.Trim(DataUtil.Left(text2, iLen));
            string text6 = DataUtil.Trim(DataUtil.Left(text3, iLen));
            string text7 = DataUtil.Trim(DataUtil.Left(text4, iLen));
            if (DataUtil.Left(gf.FolderName[gf.JumpToA] ??= "".ToUpper(), text.Length) == text)
            {
                text5 = DataUtil.Mid(text2, text.Length, iLen);
            }
            if (DataUtil.Left(gf.FolderName[gf.JumpToB] ??= "".ToUpper(), text.Length) == text)
            {
                text6 = DataUtil.Mid(text3, text.Length, iLen);
            }
            if (DataUtil.Left(gf.FolderName[gf.JumpToC] ??= "".ToUpper(), text.Length) == text)
            {
                text7 = DataUtil.Mid(text4, text.Length, iLen);
            }
            if (text2.ToUpper().IndexOf(value) < 0)
            {
                text2 = gf.FolderName[gf.JumpToA] ??= "" + " Folder";
            }
            if (text3.ToUpper().IndexOf(value) < 0)
            {
                text3 = gf.FolderName[gf.JumpToB] ??= "" + " Folder";
            }
            if (text4.ToUpper().IndexOf(value) < 0)
            {
                text4 = gf.FolderName[gf.JumpToC] ??= "" + " Folder";
            }
            Main_JumpA.Text = text5;
            Main_JumpA.ToolTipText = text2;
            Main_JumpB.Text = text6;
            Main_JumpB.ToolTipText = text3;
            Main_JumpC.Text = text7;
            Main_JumpC.ToolTipText = text4;
        }

        private void ShowAlertForm()
        {
            if (gf.FormInUse("Show Alert"))
            {
                gf.AlertRestoreWindow = true;
                return;
            }
            gf.MessageAlertRequested = false;
            gf.ParentalAlertRequested = false;
            gf.LyricsAlertRequested = false;
            gf.AlertFormOpen = true;
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
            string text = "";
            string title = "";
            int num = 0;
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
                                num++;
                                index = i;
                                if (num > 1)
                                {
                                    i = SongsList.Items.Count - 1;
                                }
                            }
                        }
                        if (num != 1)
                        {
                            MessageBox.Show("Please select ONE item from the Songs List to play");
                            break;
                        }
                        text = gf.RemoveMusicSym(SongsList.Items[index].Text);
                        string text3 = SongsList.Items[index].SubItems[1].Text;
                        string inString = DataUtil.Right(text3, text3.Length - 1);
                        int inKey = DataUtil.StringToInt(inString);
                        if (DataUtil.Left(text3, 1) == "D")
                        {
                            title = gf.LookupDBTitle2(inKey);
                        }
                        gf.Play_Media(text, title);
                        break;
                    }
                case "tabFiles":
                    if (InfoScreenList.SelectedItems.Count > 0)
                    {
                        string text2 = InfoScreenList.SelectedItems[0].SubItems[1].Text;
                        if (DataUtil.Left(text2, 1) == "I")
                        {
                            string InTitle = "";
                            gf.LoadIndividualData(ref gf.TempItem1, text2, "", 1, ref InTitle);
                            text = gf.TempItem1.Title;
                            title = gf.TempItem1.Title2;
                            gf.Play_Media(text, title);
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
            gf.AssignDropDownItem(ref Main_RotateStyle, e.ClickedItem.Name, Main_Rotate0, Main_Rotate1, Main_Rotate2, Main_Rotate3);
            gf.AutoRotateStyle = DataUtil.ObjToInt(Main_RotateStyle.Tag);
        }

        private void Main_NoRotate_Click(object sender, EventArgs e)
        {
            SetRotateState(!Main_NoRotate.Checked);
        }

        private void SetRotateState(bool RotateOn)
        {
            Main_NoRotate.Checked = !RotateOn;
            gf.AutoRotateOn = RotateOn;
            gf.RestartCurrentItem = false;
            if (gf.ShowRunning)
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
            int num = SongFolder.SelectedIndex;
            if (name == "Main_JumpA")
            {
                num = gf.JumpToA;
            }
            else if (name == "Main_JumpB")
            {
                num = gf.JumpToB;
            }
            else if (name == "Main_JumpC")
            {
                num = gf.JumpToC;
            }
            if (num > 0 && num < 41 && gf.FolderUse[num] > 0)
            {
                try
                {
                    SongFolder.Text = gf.FolderName[num];
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
                if (!gf.EmptyDelFolder())
                {
                    MessageBox.Show("Error encountered - Not all the songs were deleted. Please try again later.");
                }
                Cursor = Cursors.Default;
            }
        }

        private void Menu_Compact_Click(object sender, EventArgs e)
        {
            //gf.CompactAllDB();
        }

        private void Menu_ClearAllFormatting_Click(object sender, EventArgs e)
        {
            if (gf.ClearAllFormatting() && gf.PreviewItem.Source == ItemSource.SongsList && gf.PreviewItem.Type == "D")
            {
                SongsListIndexChanged();
            }
        }

        private void Menu_ClearRegistrySettings_Click(object sender, EventArgs e)
        {
            if (gf.ClearRegistrySettings())
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
            if (!((e.KeyCode == Keys.Return) | (e.KeyCode == Keys.Return)))
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
                gf.FindFolderItems = true;
                gf.FindBibleVerses = false;
                gf.Find_SQLString = gf.BuildItemSearchString(Main_QuickFind.Text) + " and folderno > 0";
                Do_Find(InMusicOnly: false);
            }
            else if (IsSelectedTab(tabControlSource, "tabBibles"))
            {
                if (gf.HB_TotalVersions < 1)
                {
                    return;
                }
                gf.FindBibleVerses = true;
                gf.FindFolderItems = false;
                gf.HB_SQLString = gf.BuildBibleSearchString(Main_QuickFind.Text, TabBibleVersions.SelectedIndex);
                BibleVerseSearch();
            }
            if (Main_QuickFind.Items.Count == 0 || Main_QuickFind.Text != Main_QuickFind.Items[0].ToString())
            {
                try
                {
                    Main_QuickFind.Items.Insert(0, Main_QuickFind.Text);
                    if (Main_QuickFind.Items.Count > 20)
                    {
                        for (int num = Main_QuickFind.Items.Count; num >= 21; num--)
                        {
                            Main_QuickFind.Items.RemoveAt(num);
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
            if (gf.FindFolderItems)
            {
                gf.FindFolderItems = false;
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
                FillList(-1, "", InMusicOnly);
            }
            else if (gf.HB_SQLString != "")
            {
                BibleVerseSearch();
            }
        }

        private void BibleVerseSearch()
        {
            Cursor = Cursors.WaitCursor;
            HB_SearchInProgress = true;
            tabControlSource.SelectedIndex = GetTabIndex(tabControlSource, "tabBibles");
            TabBibleVersions.SelectedIndex = gf.HB_CurVersionTabIndex;
            TabBibleVersionsChanged();
            BibleText.Text = "";
            HB_CurSelectedPassages = "";
            HB_CurSelectedTitle = "";
            gf.SearchBiblePassages(TabBibleVersions.SelectedIndex, ref BookLookup, gf.HB_SQLString, ref BibleText, gf.HB_ShowVerses);
            gf.HB_SQLString = "";
            gf.HB_SequentialListing = false;
            HB_SearchInProgress = false;
            ShowStatusBarSummary();
            Cursor = Cursors.Default;
        }

        private void Main_QuickFind_Enter(object sender, EventArgs e)
        {
            if (gf.UseFocusedTextRegionColour)
            {
                Color InBackground = Main_QuickFind.BackColor;
                gf.SetEnterColour(ref InBackground);
                Main_QuickFind.BackColor = InBackground;
            }
            Main_QuickFind.SelectAll();
        }

        private void Main_QuickFind_Leave(object sender, EventArgs e)
        {
            Color InBackground = Main_QuickFind.BackColor;
            gf.SetLeaveColor(ref InBackground);
            Main_QuickFind.BackColor = InBackground;
        }

        private void Find_Items()
        {
            gf.FindFolderItems = false;
            gf.FindBibleVerses = IsSelectedTab(tabControlSource, "tabBibles");
            if (gf.FormInUse("Search for EasiSlides Items"))
            {
                gf.FindItemRestoreWindow = true;
                return;
            }
            FrmFind frmFind = new FrmFind();
            gf.FindItemsRequested = false;
            gf.FindItemsFormOpen = true;
            frmFind.Show();
            TimerSearch.Start();
        }

        private void TimerSearch_Tick(object sender, EventArgs e)
        {
            if (gf.FindItemsRequested)
            {
                gf.FindItemsRequested = false;
                Do_Find(gf.FindItemMediaOnly);
            }
            if (!gf.FindItemsFormOpen)
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
                int selectedIndex = gf.GetSelectedIndex(WorshipListItems);
                WorshipListItems.EnsureVisible(selectedIndex);
                gf.StartPresAt = selectedIndex + 1;
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
            CMenuImages_AddItem.Enabled = ((!(gf.PreviewItem.ItemID == "")) ? true : false);
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

            string currentSession = gf.CurSession;
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
                    string sourceFile = gf.WorshipDir + currentSession + ".esw";
                    string trashDir = gf.WorshipDir + "Trash\\";

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
            int selectedIndex = gf.GetSelectedIndex(WorshipListItems, ref OutString);
            if (selectedIndex < 0)
            {
                return;
            }
            string text = WorshipListItems.Items[selectedIndex].SubItems[1].Text;
            if (DataUtil.Left(text, 1) == "B")
            {
                string InTitle = WorshipListItems.Items[selectedIndex].SubItems[0].Text;
                string text2 = Edit_Item(text, ref InTitle);
                WorshipListItems.Items[selectedIndex].SubItems[0].Text = InTitle;
                if (text2 != "")
                {
                    WorshipListItems.Items[selectedIndex].SubItems[1].Text = text2;
                    WorshipListIndexChanged();
                }
            }
            else
            {
                Edit_Item(text);
            }
        }

        private void CMenuWorship_Play_Click(object sender, EventArgs e)
        {
            WorshipListPlay();
        }

        private void WorshipListPlay()
        {
            string title = "";
            int num = 0;
            int index = 0;
            if (WorshipListItems.Items.Count <= 0)
            {
                return;
            }
            for (int i = 0; i <= WorshipListItems.Items.Count - 1; i++)
            {
                if (WorshipListItems.Items[i].Selected)
                {
                    num++;
                    index = i;
                    if (num > 1)
                    {
                        i = WorshipListItems.Items.Count - 1;
                    }
                }
            }
            if (num != 1)
            {
                MessageBox.Show("Please select ONE item from the Worship List to play");
                return;
            }
            string title2 = gf.RemoveMusicSym(WorshipListItems.Items[index].Text);
            string text = WorshipListItems.Items[index].SubItems[1].Text;
            string itemType = DataUtil.Left(text, 1);
            string inString = DataUtil.Right(text, text.Length - 1);
            if (itemType == "M")
            {
                if (string.IsNullOrWhiteSpace(inString) || !File.Exists(inString))
                {
                    MessageBox.Show("Sorry, cannot find the media file '" + inString + "'.");
                    return;
                }
                if (!gf.RunProcess(inString))
                {
                    MessageBox.Show("Sorry, cannot open the media file '" + inString + "'.");
                }
                return;
            }
            int inKey = DataUtil.StringToInt(inString);
            if (itemType == "D")
            {
                title = gf.LookupDBTitle2(inKey);
            }
            gf.Play_Media(title2, title);
        }

        private void CMenuWorship_PlayOnOutput_Click(object sender, EventArgs e)
        {
            WorshipListPlayOnOutputMonitor();
        }

        private void WorshipListPlayOnOutputMonitor()
        {
            string title = "";
            int num = 0;
            int index = 0;

            if (WorshipListItems.Items.Count <= 0)
            {
                return;
            }

            // 선택된 항목 확인
            for (int i = 0; i <= WorshipListItems.Items.Count - 1; i++)
            {
                if (WorshipListItems.Items[i].Selected)
                {
                    num++;
                    index = i;
                    if (num > 1)
                    {
                        i = WorshipListItems.Items.Count - 1;
                    }
                }
            }

            if (num != 1)
            {
                MessageBox.Show("Please select ONE item from the Worship List to play");
                return;
            }

            string title2 = gf.RemoveMusicSym(WorshipListItems.Items[index].Text);
            string text = WorshipListItems.Items[index].SubItems[1].Text;
            string itemType = DataUtil.Left(text, 1);
            string inString = DataUtil.Right(text, text.Length - 1);

            if (itemType == "M")
            {
                if (string.IsNullOrWhiteSpace(inString) || !File.Exists(inString))
                {
                    MessageBox.Show("Sorry, cannot find the media file '" + inString + "'.");
                    return;
                }

                // Output Monitor에서 재생
                PlayMediaOnOutputMonitor(inString);
                return;
            }

            int inKey = DataUtil.StringToInt(inString);
            if (itemType == "D")
            {
                title = gf.LookupDBTitle2(inKey);
            }

            // 미디어 파일 검색 후 재생
            string mediaFile = gf.GetMediaFileName(title2, title);
            if (!string.IsNullOrEmpty(mediaFile) && File.Exists(mediaFile))
            {
                PlayMediaOnOutputMonitor(mediaFile);
            }
            else
            {
                MessageBox.Show("Sorry, cannot find any media file for '" + title2 + "'");
            }
        }

        private FrmLaunchMediaPlayer _mediaPlayerWindow = null;
        private bool _isMediaPlayerHookActive = false;

        private void PlayMediaOnOutputMonitor(string mediaFilePath)
        {
            // Output Monitor 설정 확인
            string outputMonitorName = gf.OutputMonitorName;

            if (string.IsNullOrEmpty(outputMonitorName) || outputMonitorName == "None")
            {
                MessageBox.Show("Please configure Output Monitor in Options first.");
                return;
            }

            try
            {
                // FrmLaunchMediaPlayer 인스턴스 생성 또는 재사용
                if (_mediaPlayerWindow == null || _mediaPlayerWindow.IsDisposed)
                {
                    _mediaPlayerWindow = new FrmLaunchMediaPlayer();

                    // 더블클릭으로 닫기 기능 추가
                    _mediaPlayerWindow.DoubleClick += (s, e) =>
                    {
                        StopMediaPlayer();
                    };

                    // 창이 닫힐 때 글로벌 후킹 해제
                    _mediaPlayerWindow.FormClosed += (s, e) =>
                    {
                        RemoveHookMediaPlayer();
                    };
                }

                // 미디어 플레이어용 글로벌 키보드 후킹 시작
                AddHookMediaPlayer();

                // Output Monitor 적용
                _mediaPlayerWindow.ApplyOutputMonitor(outputMonitorName);

                // 미디어 파일 경로 설정
                gf.LiveItem.Format.MediaOption = 2; // 2 = 직접 경로 지정
                gf.LiveItem.Format.MediaLocation = mediaFilePath;
                gf.LiveItem.Format.MediaVolume = 100;
                gf.LiveItem.Format.MediaMute = 0;
                gf.LiveItem.Format.MediaWidescreen = 0;
                gf.LiveItem.Format.MediaRepeat = 0;
                gf.LiveItem.RotateTotal = 0;
                gf.LiveItem.Type = "M"; // M = Media 타입

                // 미디어 플레이어 창 표시
                if (!_mediaPlayerWindow.Visible)
                {
                    _mediaPlayerWindow.Show();
                }

                // 창을 최상위로 가져오기 (Win32 API 사용)
                IntPtr handle = _mediaPlayerWindow.Handle;

                // TopMost 설정
                SetWindowPos(handle, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW);

                // Foreground로 가져오기
                SetForegroundWindow(handle);
                _mediaPlayerWindow.BringToFront();
                _mediaPlayerWindow.Activate();

                // 미디어 로드 및 재생
                int result = _mediaPlayerWindow.Remote_LoadItem();
                if (result == 0)
                {
                    MessageBox.Show("Failed to load media file: " + mediaFilePath + "\n\nPlease check if DirectShow is installed and the file format is supported.");
                }
                else
                {
                    // 재생 시작
                    System.Threading.Thread.Sleep(500);
                    _mediaPlayerWindow.Remote_ResumeItemFromStart();

                    // 재생 시작 후에도 최상위 유지
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

        // 미디어 플레이어 창 닫기 메서드
        public void CloseMediaPlayer()
        {
            if (_mediaPlayerWindow != null && !_mediaPlayerWindow.IsDisposed)
            {
                _mediaPlayerWindow.Remote_StopItem();
                _mediaPlayerWindow.Hide();
            }
        }

        /// <summary>
        /// 미디어 플레이어용 글로벌 키보드 후킹 추가
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
        /// 미디어 플레이어용 글로벌 키보드 후킹 제거
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
        /// 미디어 플레이어 전용 글로벌 키보드 이벤트 핸들러
        /// </summary>
        private void HookManager_MediaPlayer_KeyDown(object sender, KeyEventArgs e)
        {
            // 미디어 플레이어가 활성 상태일 때만 동작
            if (_mediaPlayerWindow == null || _mediaPlayerWindow.IsDisposed || !_mediaPlayerWindow.Visible)
            {
                return;
            }

            // UI 스레드에서 실행되도록 보장
            frmMain.BeginInvoke(new Action(() =>
            {
                HandleMediaPlayerKey(e.KeyCode);
            }));
        }

        /// <summary>
        /// 미디어 플레이어 키 처리
        /// </summary>
        private void HandleMediaPlayerKey(Keys key)
        {
            if (_mediaPlayerWindow == null || _mediaPlayerWindow.IsDisposed)
                return;

            switch (key)
            {
                case Keys.Escape:
                    // ESC: 미디어 중지 및 숨김
                    StopMediaPlayer();
                    break;

                case Keys.Space:
                    // Space: 일시정지/재생 토글
                    _mediaPlayerWindow.Remote_PausePlayItem();
                    break;

                case Keys.Enter:
                    // Enter: 재생 재시작
                    _mediaPlayerWindow.Remote_ResumeItemFromStart();
                    break;

                case Keys.S:
                    // S: 중지
                    _mediaPlayerWindow.Remote_StopItem();
                    break;

                case Keys.M:
                    // M: 음소거 토글
                    ToggleMediaMute();
                    break;
            }
        }

        /// <summary>
        /// 미디어 플레이어 중지
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
        /// 미디어 음소거 토글
        /// </summary>
        private void ToggleMediaMute()
        {
            if (gf.LiveItem != null && gf.LiveItem.Format != null)
            {
                gf.LiveItem.Format.MediaMute = gf.LiveItem.Format.MediaMute == 0 ? 1 : 0;
                // 음소거 상태 적용 (필요시 Remote_LoadItem 재호출)
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
            int num = 0;
            bool flag = false;
            int num2 = 0;
            if (!gf.ValidateDB(DatabaseType.Usages))
            {
                return;
            }
            try
            {
#if OleDb
				using OleDbConnection connection = DbConnectionController.GetOleDbConnection(gf.ConnectStringUsageDB);
#elif SQLite
                using DbConnection connection = DbController.GetDbConnection(gf.ConnectStringUsageDB);
#endif

                string text = "";
                for (int i = 0; i < WorshipListItems.Items.Count; i++)
                {
                    if (DataUtil.Left(WorshipListItems.Items[i].SubItems[1].Text, 1) == "D")
                    {
                        num2 = i + 1;
                        text = "Insert into [USAGE] (WORSHIP_DATE,WORSHIP_LIST,SONG_TITLE,SONG_NUMBER,SONG_ID,ADMIN_1,ADMIN_2) Values (?,?,?,?,?,?,?)";
#if OleDb
						using OleDbCommand command = new OleDbCommand(text, connection);
#elif SQLite
                        using DbCommand command = new DbCommand(text, connection);
#endif

                        flag = true;
                        command.CommandText = text;
                        command.Parameters.AddWithValue("@WORSHIP_DATE", DateTime.Now.Date);
                        command.Parameters.AddWithValue("@WORSHIP_LIST", DataUtil.Left(SessionList.Text, 50));
                        command.Parameters.AddWithValue("@SONG_TITLE", gf.RemoveMusicSym(DataUtil.Left(WorshipListItems.Items[i].SubItems[0].Text, 100)));
                        command.Parameters.AddWithValue("@SONG_NUMBER", DataUtil.StringToInt(WorshipListItems.Items[i].SubItems[3].Text));
                        command.Parameters.AddWithValue("@SONG_ID", DataUtil.Mid(WorshipListItems.Items[i].SubItems[1].Text, 2));
                        command.Parameters.AddWithValue("@ADMIN_1", DataUtil.Left(WorshipListItems.Items[i].SubItems[4].Text, 50));
                        command.Parameters.AddWithValue("@ADMIN_2", DataUtil.Left(WorshipListItems.Items[i].SubItems[5].Text, 50));
                        command.ExecuteNonQuery();
                        num++;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.ToString());

                flag = true;
                if (num2 < 1)
                {
                    MessageBox.Show("Error writing usages details to the Usages Database. Please re-start EasiSlides and try again.");
                }
                else
                {
                    MessageBox.Show("Error writing Item " + num2 + " to Usages Database.");
                }
            }
            switch (num)
            {
                case 0:
                    if (!flag)
                    {
                        MessageBox.Show("There are no Database Items on the Worship List!");
                    }
                    break;
                case 1:
                    MessageBox.Show("Done. Detail of " + Convert.ToString(num) + " Database Item was added to Usages record.");
                    break;
                default:
                    MessageBox.Show("Done. Details of " + Convert.ToString(num) + " Database Items were added to the Usages record.");
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
            if (!gf.RunProcess(gf.HelpFile_Location))
            {
                MessageBox.Show("Cannot find the Help File - please Reinstall/Repair EasiSlides to install the Help Files.");
            }
        }

        private void Menu_HelpWeb_Click(object sender, EventArgs e)
        {
            if (!gf.RunProcess("http://www.easislides.com/help"))
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
            Text = "EasiSlides" + ((gf.UserString != "") ? (" - " + gf.UserString) : "");
        }

        private void AddNew()
        {
            gf.CurFolderName = SongFolder.Text;
            gf.DB_CurSongID = 0;
            if (gf.FormInUse("Edit Database Item"))
            {
                gf.Edit_RequestReceived = true;
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
            int selectedIndex = gf.GetSelectedIndex(SongsList, ref OutString);
            if (selectedIndex >= 0)
            {
                string text = SongsList.Items[selectedIndex].SubItems[1].Text;
                Edit_Item(text);
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
        /// daniel
        /// ??��??.docx ��??
        /// </summary>
        /// <param name="InIDString"></param>
        /// <param name="InTitle"></param>
        /// <returns></returns>
        private string UseCorrectEditor(string InIDString, ref string InTitle)
        {
            AddToEditHistory(InIDString);
            string a = DataUtil.Left(InIDString, 1);
            string text = DataUtil.Right(InIDString, InIDString.Length - 1);
            string strExt = Path.GetExtension(text).ToLower();
            if (a == "D")
            {
                gf.DB_CurSongID = DataUtil.StringToInt(text);
                if (gf.ValidSongID(gf.DB_CurSongID))
                {
                    if (gf.FormInUse("Edit Database Item"))
                    {
                        gf.Edit_RequestReceived = true;
                    }
                    else
                    {
                        ShowEditItemForm();
                    }
                }
                return "";
            }
            if (a == "P")
            {
                if (File.Exists(text))
                {

                    gf.LivePP.isLive = false;
                    gf.LivePP.isEditable = true;
                    if (!gf.RunProcess(text))
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
            if (a == "B")
            {
                gf.EditBible_IDString = InIDString;
                gf.EditBible_Title = InTitle;
                FrmEditBibleItem frmEditBibleItem = new FrmEditBibleItem();
                if (frmEditBibleItem.ShowDialog() == DialogResult.OK)
                {
                    InTitle = gf.EditBible_Title;
                }
                return gf.EditBible_IDString;
            }
            if (a == "T")
            {
                if (strExt != ".txt")
                {
                    return "";
                }
                if (File.Exists(text))
                {
                    if (!gf.RunProcess(text))
                    {
                        MessageBox.Show("Cannot edit text file! Please set a default text editor and try again.");
                    }
                }
                else
                {
                    MessageBox.Show("The Text File is missing!");
                }
            }
            else if (a == "I")
            {
                if (strExt != ".esi")
                {
                    return "";
                }
                ShowInfoScreen(text);
            }
            else if (a == "W")
            {
                if (strExt != ".doc" || strExt != ".docx")
                {
                    return "";
                }
                if (File.Exists(text))
                {
                    if (!gf.RunProcess(text))
                    {
                        MessageBox.Show("Cannot edit Word document!  It appears Microsoft Word is not installed on this machine.");
                    }
                }
                else
                {
                    MessageBox.Show("The Word Document is missing!");
                }
            }
            else if (a == "M")
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
            gf.EditorFormOpen = true;
            TimerMessagingWindowOpen.Start();
            frmEditItem.Show();
        }

        private int ValidatePraiseBookItems()
        {
            for (int i = 0; i < PraiseBookItems.Items.Count; i++)
            {
                gf.DocumentSongs[i + 1, 0] = PraiseBookItems.Items[i].SubItems[3].Text;
                gf.DocumentSongs[i + 1, 1] = "D";
                gf.DocumentSongs[i + 1, 2] = gf.RemoveMusicSym(DataUtil.Trim(PraiseBookItems.Items[i].SubItems[2].Text));
                gf.DocumentSongs[i + 1, 3] = PraiseBookItems.Items[i].SubItems[4].Text;
                gf.DocumentSongs[i + 1, 4] = (gf.UseSongNumbers ? PraiseBookItems.Items[i].SubItems[5].Text : "") + '>';
                gf.TotalPraiseBookItems = PraiseBookItems.Items.Count;
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
                int selectedIndex = gf.GetSelectedIndex(WorshipListItems);
                WorshipListItems.EnsureVisible(selectedIndex);
                gf.StartPresAt = selectedIndex + 1;
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
            arg = arg + gf.GetDisplayNameOnly(ref gf.HB_Versions[InVersionNo, 4], UpdateByRef: false, KeepExt: true) + ';';
            HB_CurSelectedPassages = arg + HB_CurSelectedPassages;
            HB_SelectedPassagesChanged(HB_CurSelectedPassages, ref HB_CurSelectedTitle);
            HB_CurSelectedFormat = gf.PreviewItem.Format.FormatString;
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
                string text = gf.PreviewItem.CompleteLyrics.Replace('\u0098'.ToString(), "");
                if (text != "")
                {
                    Clipboard.SetDataObject(text);
                    string text2 = "";
                    try
                    {
                        text2 = "*NEW*" + '>' + gf.HB_Versions[TabBibleVersions.SelectedIndex, 5] + '>' + gf.PreviewItem.Title;
                    }
                    catch
                    {
                        text2 = "*NEW*" + '>' + gf.PreviewItem.FolderNo.ToString() + '>' + gf.PreviewItem.Title;
                    }
                    ShowInfoScreen(text2);
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
            if (gf.FormInUse("InfoScreen"))
            {
                gf.InfoScreenFileName = TitleString;
                gf.InfoScreen_RequestReceived = true;
                return;
            }
            FrmInfoScreen frmInfoScreen = new FrmInfoScreen();
            gf.InfoScreen_RequestReceived = false;
            gf.InfoScreenFormOpen = true;
            gf.InfoScreenAction = InfoType.NoAction;
            gf.InfoScreenFileName = TitleString;
            gf.InfoScreenBackgroundPicture = "";
            gf.InfoScreenLoadNewBackground = false;
            frmInfoScreen.Show();
            TimerMessagingWindowOpen.Start();
        }

        private void TimerMessagingWindowOpen_Tick(object sender, EventArgs e)
        {
            if (gf.InfoScreenFormOpen || gf.InfoScreenLoadItem || gf.EditorFormOpen)
            {
                if (gf.InfoScreenLoadItem)
                {
                    gf.InfoScreenLoadItem = false;
                    AddToEditHistory("I" + gf.InfoScreenFileName);
                    if (gf.InfoScreenAction == InfoType.Save)
                    {
                        gf.InfoScreenAction = InfoType.NoAction;
                        if (gf.PreviewItem.Source == ItemSource.WorshipList && gf.PreviewItem.ItemID == gf.InfoScreenFileName)
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
                    gf.InfoScreenItemNew = false;
                }
                if (!gf.EditorLoadItem)
                {
                    return;
                }
                gf.EditorLoadItem = false;
                AddToEditHistory("D" + gf.EditorItemID);
                if (gf.EditorItemNew | gf.EditorItemFolderChanged | gf.EditorItemTitleChanged)
                {
                    HighlightNewFolderItem();
                }
                if (gf.PreviewItem.ItemID == gf.EditorItemID.ToString())
                {
                    if (gf.PreviewItem.Source == ItemSource.SongsList)
                    {
                        SongsListIndexChanged();
                    }
                    else if (gf.PreviewItem.Source == ItemSource.WorshipList)
                    {
                        WorshipListIndexChanged();
                    }
                    else if (gf.PreviewItem.Source == ItemSource.PraiseBook)
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
            int num = -1;
            string a = Path.GetDirectoryName(gf.InfoScreenFileName) + "\\";
            for (int i = 0; i < gf.InfoScreenFolderTotal; i++)
            {
                if (a == gf.InfoScreenGroups[i, 1])
                {
                    num = i;
                    i = gf.InfoScreenFolderTotal;
                }
            }
            if (num >= 0)
            {
                InfoScreenFolder.SelectedIndex = num;
                string displayNameOnly = gf.GetDisplayNameOnly(ref gf.InfoScreenFileName, UpdateByRef: false);
                try
                {
                    for (int i = 0; i < InfoScreenList.Items.Count; i++)
                    {
                        if (InfoScreenList.Items[i].SubItems[0].Text == displayNameOnly)
                        {
                            InfoScreenList.Items[i].Selected = true;
                            InfoScreenListIndexChanged();
                            SongsList.EnsureVisible(i);
                            i = InfoScreenList.Items.Count;
                        }
                    }
                }
                catch
                {
                }
            }
            gf.InfoScreenItemNew = false;
            gf.InfoScreenLoadItem = false;
        }

        private void HighlightNewFolderItem()
        {
            if (gf.EditorItemNew | gf.EditorItemFolderChanged | (SongFolder.Text != gf.FolderName[gf.EditorItemNewFolder]))
            {
                ImplementFolderChange = true;
                if (SongFolder.Text == gf.FolderName[gf.EditorItemNewFolder])
                {
                    SongFolder_Change();
                }
                else
                {
                    SongFolder.Text = gf.FolderName[gf.EditorItemNewFolder];
                }
                gf.EditorItemTitleChanged = false;
            }
            for (int i = 0; i < SongsList.Items.Count; i++)
            {
                if (DataUtil.Mid(SongsList.Items[i].SubItems[1].Text, 1) == gf.EditorItemID.ToString())
                {
                    if (gf.EditorItemTitleChanged)
                    {
                        SongsList.Items[i].SubItems[0].Text = gf.EditorItemTitle;
                    }
                    SongsList.Items[i].Selected = true;
                    SongsList.EnsureVisible(i);
                    break;
                }
            }
            if (gf.EditorItemTitleChanged)
            {
                for (int i = 0; i < WorshipListItems.Items.Count; i++)
                {
                    if (DataUtil.Mid(WorshipListItems.Items[i].SubItems[1].Text, 2) == gf.EditorItemID.ToString())
                    {
                        WorshipListItems.Items[i].SubItems[0].Text = gf.EditorItemTitle;
                        SaveWorshipList();
                        break;
                    }
                }
                for (int i = 0; i < PraiseBookItems.Items.Count; i++)
                {
                    if (DataUtil.Mid(PraiseBookItems.Items[i].SubItems[3].Text, 2) == gf.EditorItemID.ToString())
                    {
                        PraiseBookList_Change();
                        break;
                    }
                }
            }
            gf.EditorItemNew = false;
            gf.EditorItemFolderChanged = false;
            gf.EditorItemTitleChanged = false;
        }

        private void Ind_Head_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            gf.AssignDropDownItem(ref Ind_Head, e.ClickedItem.Name, Ind_HeadNoTitles, Ind_HeadAllTitles, Ind_HeadFirstScreen);
            gf.PreviewItem.Format.ShowSongHeadings = DataUtil.ObjToInt(Ind_Head.Tag);
            UpdateFormatData(StartAtFirstSlide: false);
        }

        private void Def_Head_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            gf.AssignDropDownItem(ref Def_Head, e.ClickedItem.Name, Def_HeadNoTitles, Def_HeadAllTitles, Def_HeadFirstScreen);
            gf.ShowSongHeadings = DataUtil.ObjToInt(Def_Head.Tag);
            ApplyDefaultData(StartAtFirstSlide: false);
        }

        private void Ind_HeadAlign_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            gf.AssignDropDownItem(ref Ind_HeadAlign, e.ClickedItem.Name, Ind_HeadAlignAsR1, Ind_HeadAlignAsR2, Ind_HeadAlignLeft, Ind_HeadAlignCentre, Ind_HeadAlignRight);
            gf.PreviewItem.Format.ShowSongHeadingsAlign = DataUtil.ObjToInt(Ind_HeadAlign.Tag);
            UpdateFormatData(StartAtFirstSlide: false);
        }

        private void Def_HeadAlign_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            gf.AssignDropDownItem(ref Def_HeadAlign, e.ClickedItem.Name, Def_HeadAlignAsR1, Def_HeadAlignAsR2, Def_HeadAlignLeft, Def_HeadAlignCentre, Def_HeadAlignRight);
            gf.ShowSongHeadingsAlign = DataUtil.ObjToInt(Def_HeadAlign.Tag);
            ApplyDefaultData(StartAtFirstSlide: false);
        }

        private bool DShowPlayerPresent()
        {
            return File.Exists(Path.Combine(Environment.SystemDirectory, "dpnhpast.dll"));
        }

        private void ApplyPreviewArea_Setup(int InMode)
        {
            try
            {
                ApplyLyricsRichTextBoxFont(InMode);
                BibleText.Font = new Font(BibleText.Font.Name, gf.PreviewArea_FontSize, BibleText.Font.Style);
                SongsList.Font = new Font(SongsList.Font.Name, gf.PreviewArea_FontSize, SongsList.Font.Style);
                InfoScreenList.Font = new Font(InfoScreenList.Font.Name, gf.PreviewArea_FontSize, InfoScreenList.Font.Style);
                WorshipListItems.Font = new Font(WorshipListItems.Font.Name, gf.PreviewArea_FontSize, WorshipListItems.Font.Style);
                PraiseBookItems.Font = new Font(PraiseBookItems.Font.Name, gf.PreviewArea_FontSize, PraiseBookItems.Font.Style);
                PreviewInfo.Font = new Font(PreviewInfo.Font.Name, gf.PreviewArea_FontSize, PreviewInfo.Font.Style);
            }
            catch
            {
            }
        }

        private void ApplyLyricsRichTextBoxFont(int InMode)
        {
            if (InMode == 0 || InMode == 2)
            {
                for (int i = 1; i <= gf.PreviewItem.TotalSlides; i++)
                {
                    if (Lyrics_PreviewBox[i] != null)
                    {
                        Lyrics_PreviewBox[i].Font = new Font(flowLayoutPreviewLyrics.Font.Name, gf.PreviewArea_FontSize, flowLayoutPreviewLyrics.Font.Style);
                        Lyrics_PreviewBox[i].BorderStyle = (gf.PreviewArea_LineBetweenScreens ? BorderStyle.FixedSingle : BorderStyle.None);
                    }
                }
            }
            FormatLyricsContainers(gf.PreviewItem);
            ResizePreviewRichTextBox();
            if (InMode != 1 && InMode != 2)
            {
                return;
            }
            for (int i = 1; i <= gf.OutputItem.TotalSlides; i++)
            {
                if (Lyrics_OutputBox[i] != null)
                {
                    Lyrics_OutputBox[i].Font = new Font(flowLayoutOutputLyrics.Font.Name, gf.PreviewArea_FontSize, flowLayoutOutputLyrics.Font.Style);
                    Lyrics_OutputBox[i].BorderStyle = (gf.PreviewArea_LineBetweenScreens ? BorderStyle.FixedSingle : BorderStyle.None);
                }
            }
            FormatLyricsContainers(gf.OutputItem);
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
                GenerateListingDir = gf.RootEasiSlidesDir + "Documents\\";
            }
            string text = SaveListingOfFolder(ref GenerateListingFileName, ref GenerateListingDir);
            if (Path.GetExtension(text) == ".rtf")
            {
                GenerateIndexReportRTF(text);
            }
            else if (Path.GetExtension(text) == ".txt")
            {
                GenerateIndexReportTabText(text);
            }
            else if (text != "")
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

                    gf.RTFNewLine = "\\b0\\i0\\ulnone\\par ";
                    gf.RTFIndent[0] = "\\pard\\tx1200\\tx3500\\tx8200\\tx9000 ";
                    string value = "{\\rtf1\\ansi\\ansicpg1252\\deff0\\deflang1033{\\fonttbl{\\f0\\fnil\\fcharset0 Microsoft Sans Serif;}}\\viewkind1\\uc1\\pard\\f0\\fs24\\margr600\\margl1000\\margt900\\margb1000\\cols2\\colno1\\colw4650\\colsr750\\colno2\\colw4650 ";
                    string text = "";
                    int num = -1;

                    /// daniel  ??��??�� ���� ??��??????
                    //Recordset tableRecordSet = DbDaoController.GetTableRecordSet(gf.ConnectStringMainDB, "SONG");
                    //tableRecordSet.Index = "PrimaryKey";

                    streamWriter.Write(value);
                    streamWriter.Write(DataUtil.UnicodeToAscii_RTF("\\b1\\ul " + SongFolder.Text + "\\b0\\ulnone " + gf.RTFNewLine));
                    streamWriter.Write(DataUtil.UnicodeToAscii_RTF(gf.RTFNewLine));
                    for (int i = 0; i < SongsList.Items.Count; i++)
                    {
                        streamWriter.Write(DataUtil.UnicodeToAscii_RTF(gf.RTFIndent[0] + gf.RemoveMusicSym(SongsList.Items[i].SubItems[0].Text) + gf.RTFNewLine));
                    }
                    streamWriter.Write(DataUtil.UnicodeToAscii_RTF(gf.RTFNewLine));
                    streamWriter.Write(DataUtil.UnicodeToAscii_RTF("(Total: " + SongsList.Items.Count + " items)" + gf.RTFNewLine));
                    streamWriter.Write("}");
                    //streamWriter.Flush();
                    //streamWriter.Close();
                    gf.RunProcess(OutputFileName);
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

#if DAO
		private void GenerateIndexReportTabText(string OutputFileName)
		{
			Cursor = Cursors.WaitCursor;
			StringBuilder stringBuilder = new StringBuilder();
			try
			{
				string text = "";
				string text2 = "";
				string text3 = "";
				string text4 = "";
				string text5 = "";
				int num = -1;
				
				Recordset tableRecordSet = DbDaoController.GetTableRecordSet(gf.ConnectStringMainDB, "SONG");
				tableRecordSet.Index = "PrimaryKey";

				stringBuilder.Append("Song Title\tWriter\tMP\tSF\tTS\r\n");
				for (int i = 0; i < SongsList.Items.Count; i++)
				{
					text4 = DataUtil.Mid(SongsList.Items[i].SubItems[1].Text, 1);
					text = "";
					text3 = "";
					text5 = "";
					tableRecordSet.Seek("=", text4, gf.def, gf.def, gf.def, gf.def, gf.def, gf.def, gf.def, gf.def, gf.def, gf.def, gf.def, gf.def);
					if (!tableRecordSet.NoMatch)
					{
						text = DataUtil.GetDataString(tableRecordSet, "BOOK_REFERENCE");
						text5 = DataUtil.GetDataString(tableRecordSet, "Writer");
						text2 = "";
						text3 = "\t" + text5;
						num = text.IndexOf("MP");
						if (num >= 0)
						{
							int num2 = num + 2;
							int num3 = text.IndexOfAny(gf.ReferenceAlertPickSeparator.ToCharArray(), num2);
							num3 = ((num3 >= 0) ? num3 : text.Length);
							text2 = DataUtil.Mid(text, num2, num3 - num2);
						}
						text3 = text3 + "\t" + text2;
						text2 = "";
						num = text.IndexOf("SF");
						if (num >= 0)
						{
							int num2 = num + 2;
							int num3 = text.IndexOfAny(gf.ReferenceAlertPickSeparator.ToCharArray(), num2);
							num3 = ((num3 >= 0) ? num3 : text.Length);
							text2 = DataUtil.Mid(text, num2, num3 - num2);
						}
						text3 = text3 + "\t" + text2;
						text2 = "";
						num = text.IndexOf("TS");
						if (num >= 0)
						{
							int num2 = num + 2;
							int num3 = text.IndexOfAny(gf.ReferenceAlertPickSeparator.ToCharArray(), num2);
							num3 = ((num3 >= 0) ? num3 : text.Length);
							text2 = DataUtil.Mid(text, num2, num3 - num2);
						}
						text3 = text3 + "\t" + text2;
					}
					else
					{
						text3 = "\t\t\t\t";
					}
					stringBuilder.Append(gf.RemoveMusicSym(SongsList.Items[i].SubItems[0].Text) + text3 + "\r\n");
				}
				FileUtil.CreateNewFile(OutputFileName, FileUtil.FileContentsType.DoubleByte, stringBuilder.ToString());
				gf.RunProcess(OutputFileName);
			}
			catch
			{
			}
			Cursor = Cursors.Default;
		}
#elif SQLite
        private void GenerateIndexReportTabText(string OutputFileName)
        {
            Cursor = Cursors.WaitCursor;
            StringBuilder stringBuilder = new StringBuilder();
            try
            {
                string text = "";
                string text2 = "";
                string text3 = "";
                string text4 = "";
                string text5 = "";
                int num = -1;

                using DbConnection connection = DbController.GetDbConnection(gf.ConnectStringMainDB);
                stringBuilder.Append("Song Title\tWriter\tMP\tSF\tTS\r\n");

                for (int i = 0; i < SongsList.Items.Count; i++)
                {
                    text4 = DataUtil.Mid(SongsList.Items[i].SubItems[1].Text, 1);
                    text = "";
                    text3 = "";
                    text5 = "";

                    DataRow dataRow = DbController.GetDataRowScalar(connection, $"Select FolderNo From SONG Where SONGID={text4}");

                    if (dataRow != null)
                    {
                        text = DataUtil.GetDataString(dataRow, "BOOK_REFERENCE");
                        text5 = DataUtil.GetDataString(dataRow, "Writer");
                        text2 = "";
                        text3 = "\t" + text5;
                        num = text.IndexOf("MP");
                        if (num >= 0)
                        {
                            int num2 = num + 2;
                            int num3 = text.IndexOfAny(gf.ReferenceAlertPickSeparator.ToCharArray(), num2);
                            num3 = ((num3 >= 0) ? num3 : text.Length);
                            text2 = DataUtil.Mid(text, num2, num3 - num2);
                        }
                        text3 = text3 + "\t" + text2;
                        text2 = "";
                        num = text.IndexOf("SF");
                        if (num >= 0)
                        {
                            int num2 = num + 2;
                            int num3 = text.IndexOfAny(gf.ReferenceAlertPickSeparator.ToCharArray(), num2);
                            num3 = ((num3 >= 0) ? num3 : text.Length);
                            text2 = DataUtil.Mid(text, num2, num3 - num2);
                        }
                        text3 = text3 + "\t" + text2;
                        text2 = "";
                        num = text.IndexOf("TS");
                        if (num >= 0)
                        {
                            int num2 = num + 2;
                            int num3 = text.IndexOfAny(gf.ReferenceAlertPickSeparator.ToCharArray(), num2);
                            num3 = ((num3 >= 0) ? num3 : text.Length);
                            text2 = DataUtil.Mid(text, num2, num3 - num2);
                        }
                        text3 = text3 + "\t" + text2;
                    }
                    else
                    {
                        text3 = "\t\t\t\t";
                    }
                    stringBuilder.Append(gf.RemoveMusicSym(SongsList.Items[i].SubItems[0].Text) + text3 + "\r\n");
                }
                FileUtil.CreateNewFile(OutputFileName, FileUtil.FileContentsType.DoubleByte, stringBuilder.ToString());
                gf.RunProcess(OutputFileName);
            }
            catch
            {
            }
            Cursor = Cursors.Default;
        }
#endif

        private string SaveListingOfFolder(ref string InFileName, ref string InitDirectory)
        {
            saveFileDialog1.Filter = "Rich Text Document (*.rtf)|*.rtf|Tab delimted Text File (*.txt)|*.txt";
            saveFileDialog1.Title = "Listing of Folder";
            saveFileDialog1.InitialDirectory = InitDirectory;
            saveFileDialog1.OverwritePrompt = true;
            saveFileDialog1.AddExtension = true;
            string extension = Path.GetExtension(InFileName);
            saveFileDialog1.FileName = gf.GetDisplayNameOnly(ref InFileName, UpdateByRef: false, KeepExt: false);
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

        private string RemoteControlLiveShow(LiveShowAction InAction)
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
                        break;
                    case LiveShowAction.Remote_SlideChanged:
                        LiveShow.Remote_SlideChanged((int)InDirection);
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
                        LiveShow.Remote_MoveToItemChanged();
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
                        LiveShow.Remote_SongJumpTo();
                        break;
                }
            }
            catch
            {
                try
                {
                    LiveShow = new FrmLaunchShow();
                    LiveShow.OnMessage += LiveShow_OnMessage;
                    RemoteControlLiveShow(InAction);
                }
                catch
                {
                }
            }
            return "";
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
                    string path = gf.InfoScreenGroups[InfoScreenFolder.SelectedIndex, 1];
                    string[] files = Directory.GetFiles(path, "*.esi");
                    string[] array = files;
                    foreach (string text in array)
                    {
                        if (text != "")
                        {
                            listBox.Items.Add(text);
                        }
                    }
                }
                catch
                {
                }
                listBox.Sorted = true;
                InfoScreenList.Items.Clear();
                string text2 = "";
                for (int j = 0; j < listBox.Items.Count; j++)
                {
                    text2 = listBox.Items[j].ToString();
                    listViewItem = InfoScreenList.Items.Add(gf.GetDisplayNameOnly(ref text2, UpdateByRef: false, KeepExt: false));
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
            if (gf.EasiSlidesMode == UsageMode.Worship && gf.GetSelectedIndex(InfoScreenList) >= 0)
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
                gf.GetSelectedIndex(InfoScreenList, ref OutString);
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
            int selectedIndex = gf.GetSelectedIndex(InfoScreenList);
            if (selectedIndex >= 0)
            {
                gf.PreviewItem.Source = ItemSource.ExternalFileInfoScreen;
                string text = InfoScreenList.Items[selectedIndex].SubItems[1].Text;
                string InTitle = InfoScreenList.Items[selectedIndex].SubItems[0].Text;
                gf.PreviewItem.InMainItemText = InTitle;
                gf.PreviewItem.InSubItemItem1Text = text;
                gf.PreviewItem.CurItemNo = 0;
                LoadItem(ref gf.PreviewItem, text, "", StartingSlide, ref InTitle, ScrollToCaret);
                UpdateDisplayPanelFields();
            }
            else
            {
                gf.PreviewItem.Type = "";
                gf.PreviewItem.Title = "";
                gf.PreviewItem.ItemID = "";
                gf.PreviewItem.CurItemNo = 0;
                gf.LoadIndividualFormatData(ref gf.PreviewItem, "");
                AllowIndividualFormat(AllowFormat: false);
                UpdateFormatFields();
                BuildVerseButtons(gf.PreviewItem);
                DisplayLyrics(gf.PreviewItem, 0, ScrollToCaret: true);
                UpdateDisplayPanelFields();
            }
        }

        private void InfoScreen_OpenFolder_Click(object sender, EventArgs e)
        {
            string text = gf.InfoScreenGroups[InfoScreenFolder.SelectedIndex, 1];
            if (gf.ValidateDir(text, CreateDir: true))
            {
                gf.RunProcess(text);
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
            gf.SelectedItemsCount = InListView.SelectedItems.Count;
            gf.ExternalMoveFolder = -1;
            gf.ExternalCopyFolder = 1;
            gf.ExternalMoveCopyType = InFileSymbol;
            FrmCopyMoveExternal frmCopyMoveExternal = new FrmCopyMoveExternal();
            if (frmCopyMoveExternal.ShowDialog() != DialogResult.OK)
            {
                return;
            }
            if (gf.ExternalCopyFolder >= 0)
            {
                string[] array = new string[30000];
                array[0] = "0";
                ListView listView = new ListView();
                ListViewItem listViewItem = new ListViewItem();
                string InFileName = "";
                for (int i = 0; i < InListView.SelectedItems.Count; i++)
                {
                    switch (InFileSymbol)
                    {
                        case "I":
                            InFileName = gf.CopyExternalFile(DataUtil.Mid(InListView.SelectedItems[i].SubItems[1].Text, 1), gf.InfoScreenGroups[gf.ExternalCopyFolder, 1]);
                            break;
                        case "P":
                            InFileName = gf.CopyExternalFile(DataUtil.Mid(InListView.SelectedItems[i].SubItems[1].Text, 1), gf.PowerpointGroups[gf.ExternalCopyFolder, 1]);
                            break;
                    }
                    if (InFileName != "")
                    {
                        listViewItem = listView.Items.Add(gf.GetDisplayNameOnly(ref InFileName, UpdateByRef: false));
                        array[0] = Convert.ToString(DataUtil.StringToInt(array[0]) + 1);
                        array[DataUtil.StringToInt(array[0])] = gf.GetDisplayNameOnly(ref InFileName, UpdateByRef: false);
                    }
                }
                if (InFileSymbol == "I")
                {
                    HighlightCopyMoveExternalItems(InfoScreenList, gf.ExternalCopyFolder, array, InFileSymbol);
                }
                else
                {
                    HighlightCopyMoveExternalItems(PowerpointList, gf.ExternalCopyFolder, array, InFileSymbol);
                }
            }
            else
            {
                if (!(InFileSymbol == "I"))
                {
                    return;
                }
                gf.ExternalCopyFolder = -1 * gf.ExternalCopyFolder;
                if (gf.ExternalCopyFolder <= 0)
                {
                    gf.ExternalCopyFolder = 1;
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
                        gf.LoadInfoFile(InFileName, ref gf.TempItem1, ref ThisHeaderData);
                        if (DataUtil.Trim(gf.TempItem1.Title) == "")
                        {
                            gf.TempItem1.Title = gf.GetDisplayNameOnly(ref InFileName, UpdateByRef: false);
                        }
                        gf.TempItem1.FolderNo = gf.ExternalCopyFolder;
                        int num = gf.InsertItemIntoDatabase(gf.ConnectStringMainDB, gf.TempItem1);
                        if (num > 0)
                        {
                            RefileSongs[0]++;
                            RefileSongs[RefileSongs[0]] = num;
                        }
                    }
                }
                if (RefileSongs[0] > 0)
                {
                    HighlightCopyMoveItems(gf.ExternalCopyFolder, ref RefileSongs);
                }
            }
        }

        private void MoveExternalItems(ListView InListView, string InFileSymbol)
        {
            if (InListView.SelectedItems.Count <= 0)
            {
                return;
            }
            gf.SelectedItemsCount = InListView.SelectedItems.Count;
            gf.ExternalMoveFolder = 1;
            gf.ExternalCopyFolder = -1;
            gf.ExternalMoveCopyType = InFileSymbol;
            FrmCopyMoveExternal frmCopyMoveExternal = new FrmCopyMoveExternal();
            if (frmCopyMoveExternal.ShowDialog() != DialogResult.OK || gf.ExternalMoveFolder == InfoScreenFolder.SelectedIndex || gf.ExternalMoveFolder < 0 || gf.ExternalMoveFolder == InfoScreenFolder.SelectedIndex)
            {
                return;
            }
            string[] array = new string[30000];
            array[0] = "0";
            ListView listView = new ListView();
            ListViewItem listViewItem = new ListViewItem();
            string InFileName = "";
            for (int i = 0; i < InListView.SelectedItems.Count; i++)
            {
                switch (InFileSymbol)
                {
                    case "I":
                        InFileName = gf.MoveExternalFile(DataUtil.Mid(InListView.SelectedItems[i].SubItems[1].Text, 1), gf.InfoScreenGroups[gf.ExternalMoveFolder, 1]);
                        break;
                    case "P":
                        InFileName = gf.MoveExternalFile(DataUtil.Mid(InListView.SelectedItems[i].SubItems[1].Text, 1), gf.PowerpointGroups[gf.ExternalMoveFolder, 1]);
                        break;
                }
                if (InFileName != "")
                {
                    listViewItem = listView.Items.Add(gf.GetDisplayNameOnly(ref InFileName, UpdateByRef: false));
                    array[0] = Convert.ToString(DataUtil.StringToInt(array[0]) + 1);
                    array[DataUtil.StringToInt(array[0])] = gf.GetDisplayNameOnly(ref InFileName, UpdateByRef: false);
                }
            }
            if (InFileSymbol == "I")
            {
                HighlightCopyMoveExternalItems(InfoScreenList, gf.ExternalMoveFolder, array, InFileSymbol);
            }
            else
            {
                HighlightCopyMoveExternalItems(PowerpointList, gf.ExternalMoveFolder, array, InFileSymbol);
            }
        }

        private void RemoveExternalItems(ListView InListView, string InFileSymbol)
        {
            if (InListView.SelectedItems.Count <= 0)
            {
                return;
            }
            string text = "";
            if (InFileSymbol == "I")
            {
                text = "InfoScreen(s)";
            }
            else
            {
                if (!(InFileSymbol == "P"))
                {
                    return;
                }
                text = "Powerpint File(s)";
            }
            if (MessageBox.Show("Delete Selected " + text + " to Windows Recycle Bin?", "Delete " + text, MessageBoxButtons.YesNo) != DialogResult.Yes)
            {
                return;
            }
            string InFileName = "";
            if (gf.OutputItem.Type == InFileSymbol)
            {
                InFileName = gf.OutputItem.ItemID;
                gf.GetDisplayNameOnly(ref InFileName, UpdateByRef: true);
            }
            ListView.SelectedListViewItemCollection selectedItems = InListView.SelectedItems;
            int index = selectedItems[selectedItems.Count - 1].Index;
            for (int num = selectedItems.Count - 1; num >= 0; num--)
            {
                if (selectedItems[num].Text != InFileName)
                {
                    try
                    {
                        if (gf.RecycleBin(DataUtil.Mid(selectedItems[num].SubItems[1].Text, 1)))
                        {
                            selectedItems[num].Remove();
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
            SetOneTab(ref tabControlSource, tabPowerpoint, gf.UsePowerpointTab, tabFiles.Name);
            SetOneTab(ref tabControlSource, tabMedia, gf.UseMediaTab, tabImages.Name);
            panelOutputLM1.Visible = gf.ShowLyricsMonitorAlertBox;
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
            int selectedIndex = gf.GetSelectedIndex(PowerpointList);
            if (selectedIndex >= 0)
            {
                gf.PreviewItem.Source = ItemSource.ExternalFilePowerpoint;
                string text = PowerpointList.Items[selectedIndex].SubItems[1].Text;
                string InTitle = Path.GetFileNameWithoutExtension(DataUtil.Right(text, text.Length - 1));
                gf.PreviewItem.InMainItemText = InTitle;
                gf.PreviewItem.InSubItemItem1Text = text;
                gf.PreviewItem.CurItemNo = 0;
                LoadItem(ref gf.PreviewItem, text, "", 1, ref InTitle, ScrollToCaret: false);
                UpdateDisplayPanelFields();
            }
            else
            {
                gf.PreviewItem.Type = "";
                gf.PreviewItem.Title = "";
                gf.PreviewItem.ItemID = "";
                gf.PreviewItem.CurItemNo = 0;
                gf.LoadIndividualFormatData(ref gf.PreviewItem, "");
                AllowIndividualFormat(AllowFormat: false);
                UpdateFormatFields();
                BuildVerseButtons(gf.PreviewItem);
                DisplayLyrics(gf.PreviewItem, 0, ScrollToCaret: true);
                UpdateDisplayPanelFields();
            }
        }

        private void PP_Style_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            gf.AssignDropDownItem(ref PP_ListType, e.ClickedItem.Name, PP_ListStyle, PP_PreviewStyle);
            gf.PowerpointListingStyle = DataUtil.ObjToInt(PP_ListType.Tag);
            PowerpointStyle_Changed(RebuildPowerpointFolders: false);
        }

        private void PowerpointStyle_Changed(bool RebuildPowerpointFolders)
        {
            PP_OpenFolder.ToolTipText = "Open Powerpoint Folder";
            if (gf.PowerpointListingStyle == 0)
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
                ShowPowerpointFolderContents((gf.PowerpointListingStyle == 1) ? true : false);
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
            string text = gf.PowerpointGroups[PowerpointFolder.SelectedIndex, 1];
            if (gf.ValidateDir(text, CreateDir: true))
            {
                gf.RunProcess(text);
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
                int selectedIndex = gf.GetSelectedIndex(WorshipListItems);
                WorshipListItems.EnsureVisible(selectedIndex);
                gf.StartPresAt = selectedIndex + 1;
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
            ShowPowerpointFolderContents((gf.PowerpointListingStyle == 1) ? true : false);
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
            if (gf.EasiSlidesMode == UsageMode.Worship && gf.GetSelectedIndex(PowerpointList) >= 0)
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
                gf.GetSelectedIndex(PowerpointList, ref OutString);
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
                    string path = gf.MediaGroups[MediaFolder.SelectedIndex, 1];
                    for (int i = 0; i < gf.TotalMediaFileExt; i++)
                    {
                        try
                        {
                            string[] files = Directory.GetFiles(path, "*" + gf.MediaFileExtension[i, 0]);
                            string[] array = files;
                            foreach (string text in array)
                            {
                                if (text != "")
                                {
                                    listBox.Items.Add(text);
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
                string text2 = "";
                for (int i = 0; i < listBox.Items.Count; i++)
                {
                    text2 = listBox.Items[i].ToString();
                    listViewItem = MediaList.Items.Add(gf.GetDisplayNameOnly(ref text2, UpdateByRef: false, KeepExt: true));
                    listViewItem.SubItems.Add("M" + listBox.Items[i]);
                }
                SetMediaListColWidth();
                listBox.Dispose();
                Cursor = Cursors.Default;
            }
        }

        private void MediaListIndexChanged()
        {
            int selectedIndex = gf.GetSelectedIndex(MediaList);
            if (selectedIndex >= 0)
            {
                gf.PreviewItem.Source = ItemSource.ExternalFileMedia;
                string text = MediaList.Items[selectedIndex].SubItems[1].Text;
                string InFileName = MediaList.Items[selectedIndex].SubItems[0].Text;
                gf.GetDisplayNameOnly(ref InFileName, UpdateByRef: true);
                gf.PreviewItem.InMainItemText = InFileName;
                gf.PreviewItem.InSubItemItem1Text = text;
                gf.PreviewItem.CurItemNo = 0;
                LoadItem(ref gf.PreviewItem, text, "", 1, ref InFileName, ScrollToCaret: false);
                UpdateDisplayPanelFields();
            }
            else
            {
                gf.PreviewItem.Type = "";
                gf.PreviewItem.Title = "";
                gf.PreviewItem.ItemID = "";
                gf.PreviewItem.CurItemNo = 0;
                gf.LoadIndividualFormatData(ref gf.PreviewItem, "");
                AllowIndividualFormat(AllowFormat: false);
                UpdateFormatFields();
                BuildVerseButtons(gf.PreviewItem);
                DisplayLyrics(gf.PreviewItem, 0, ScrollToCaret: true);
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
            if (gf.EasiSlidesMode == UsageMode.Worship && gf.GetSelectedIndex(MediaList) >= 0)
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
                gf.GetSelectedIndex(MediaList, ref OutString);
            }
            MediaList.Focus();
        }

        private void Media_OpenFolder_Click(object sender, EventArgs e)
        {
            string text = gf.MediaGroups[MediaFolder.SelectedIndex, 1];
            if (gf.ValidateDir(text, CreateDir: true))
            {
                gf.RunProcess(text);
            }
        }

        private void Image_OpenFolder_Click(object sender, EventArgs e)
        {
            string text = gf.PictureGroups[ImagesFolder.SelectedIndex, 1];
            if (gf.ValidateDir(text, CreateDir: true))
            {
                gf.RunProcess(text);
            }
        }

        private void Image_Import_Click(object sender, EventArgs e)
        {
            string str = gf.PictureGroups[ImagesFolder.SelectedIndex, 1];
            openFileDialog1.Filter = "Images (*.jpg,*jpeg,*.bmp,*.gif,*.ico)|*.jpg;*jpeg;*.bmp;*.gif;*.ico";
            openFileDialog1.Title = "Import An Image into folder: " + ImagesFolder.Text;
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string fileName = openFileDialog1.FileName;
                try
                {
                    File.Copy(fileName, str + Path.GetFileName(fileName));
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
            string str = gf.InfoScreenGroups[InfoScreenFolder.SelectedIndex, 1];
            openFileDialog1.Filter = "InfoScreens (*.esi)|*.esi";
            openFileDialog1.Title = "Import An InfoScreen into folder: " + InfoScreenFolder.Text;
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string fileName = openFileDialog1.FileName;
                try
                {
                    File.Copy(fileName, str + Path.GetFileName(fileName));
                    ShowInfoScreenFolderContents();
                    int num = 0;
                    while (true)
                    {
                        if (num >= InfoScreenList.Items.Count)
                        {
                            return;
                        }
                        if (DataUtil.Mid(InfoScreenList.Items[num].SubItems[1].Text, 1) == str + Path.GetFileName(fileName))
                        {
                            break;
                        }
                        num++;
                    }
                    InfoScreenList.Items[num].Selected = true;
                    InfoScreenList.Items[num].EnsureVisible();
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
            string str = gf.PowerpointGroups[PowerpointFolder.SelectedIndex, 1];
            //openFileDialog1.Filter = "Powerpoint Files (*.ppt)|*.ppt";

            openFileDialog1.Filter = "Powerpoint Files(*.ppt)| *.ppt | Powerpoint Files(*.pptx) | *.pptx";
            openFileDialog1.Title = "Import a Powerpoint File into folder: " + ImagesFolder.Text;
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string fileName = openFileDialog1.FileName;
                try
                {
                    File.Copy(fileName, str + Path.GetFileName(fileName));
                    ShowPowerpointFolderContents((gf.PowerpointListingStyle == 1) ? true : false);
                    if (gf.PowerpointListingStyle == 0)
                    {
                        ShowPowerpointFolderContents(ShowThumbs: false);
                        int num = 0;
                        while (true)
                        {
                            if (num >= PowerpointList.Items.Count)
                            {
                                return;
                            }
                            if (DataUtil.Mid(PowerpointList.Items[num].SubItems[1].Text, 1) == str + Path.GetFileName(fileName))
                            {
                                break;
                            }
                            num++;
                        }
                        PowerpointList.Items[num].Selected = true;
                        PowerpointList.Items[num].EnsureVisible();
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
            string str = gf.MediaGroups[MediaFolder.SelectedIndex, 1];
            string str2 = (gf.TotalMediaFileExt > 0) ? "Media Files (" : "";
            string text = (gf.TotalMediaFileExt > 0) ? ")|" : "";
            for (int i = 0; i < gf.TotalMediaFileExt; i++)
            {
                str2 = str2 + ((i > 0) ? "," : "") + "*" + gf.MediaFileExtension[i, 0].ToLower();
                text = text + ((i > 0) ? ";" : "") + "*" + gf.MediaFileExtension[i, 0].ToLower();
            }
            string filter = str2 + text;
            openFileDialog1.Filter = filter;
            openFileDialog1.Title = "Import A Media File into folder: " + ImagesFolder.Text;
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string fileName = openFileDialog1.FileName;
                try
                {
                    File.Copy(fileName, str + Path.GetFileName(fileName));
                    ShowMediaFolderContents();
                    int i = 0;
                    while (true)
                    {
                        if (i >= MediaList.Items.Count)
                        {
                            return;
                        }
                        if (DataUtil.Mid(MediaList.Items[i].SubItems[1].Text, 1) == str + Path.GetFileName(fileName))
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
            Point p = new Point(0, 0);
            Button button = (Button)sender;
            PopupBtnPressed = button.Name;
            string popupBtnPressed = PopupBtnPressed;
            if (popupBtnPressed != null && popupBtnPressed == "BtnEditSessionNotes")
            {
                gf.popUpText = PreviewNotes.Text;
                gf.popUpTextMaxLength = PreviewNotes.MaxLength;
                p = new Point(splitContainerMain.Left + splitContainerMain.Panel1.Width + splitContainerMain.SplitterWidth, menuStripMain.Top + toolStripMain.Height + splitContainerPreview.Panel1.Height + panel1.Height + 20);
            }
            FrmPopupText popup = new FrmPopupText();
            Point location = PointToScreen(p);
            popupHelper.ShowPopup(this, popup, location);
        }

        private void popupHelper_PopupClosed(object sender, PopupClosedEventArgs e)
        {
            string popupBtnPressed = PopupBtnPressed;
            if (popupBtnPressed != null && popupBtnPressed == "BtnEditSessionNotes")
            {
                PreviewNotes.Text = gf.popUpText;
            }
        }

        private void Menu_RefreshOutput_Click(object sender, EventArgs e)
        {
            gf.RefreshWindowsDesktop();
            if (gf.ShowRunning)
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
            int num = Reset ? 1 : InItem.TotalSlides;
            for (int i = 1; i <= num; i++)
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
                    InRichTextBox[i].Font = new Font(InPanel.Font.Name, gf.PreviewArea_FontSize, InPanel.Font.Style);
                }
                else
                {
                    InRichTextBox[i].Font = new Font(InPanel.Font.Name, gf.PreviewArea_FontSize, InPanel.Font.Style);
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
            gf.DisplayRichTextBoxSeries(ref InItem, ref InPanel, ref InRichTextBox, ScrollToCaret: true, gf.PreviewArea_ShowNotations);
            for (int i = 1; i <= num; i++)
            {
                InRichTextBox[i].BorderStyle = (gf.PreviewArea_LineBetweenScreens ? BorderStyle.FixedSingle : BorderStyle.None);
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
                LyricsRichTextBox_ShowSlide(gf.OutputItem, newSlideNumber);
            }
            else
            {
                LyricsRichTextBox_ShowSlide(gf.PreviewItem, newSlideNumber);
            }
        }

        private void LyricsRichTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            Keys keyCode = e.KeyCode;
            RichTextBox richTextBox = (RichTextBox)sender;
            if (richTextBox.Name == "Output")
            {
                ItemKeyPressed(gf.OutputItem, keyCode, e.Shift);
                FocusOutputArea();
            }
            else
            {
                ItemKeyPressed(gf.PreviewItem, keyCode, e.Shift);
            }
        }

        private void LyricsRichTextBox_MouseWheel(object sender, MouseEventArgs e)
        {
            RichTextBox richTextBox = (RichTextBox)sender;
            int num = -(e.Delta / 3);
            if (richTextBox.Name == "Output")
            {
                num -= flowLayoutOutputLyrics.AutoScrollPosition.Y;
                if (num < 0)
                {
                    num = 0;
                }
                flowLayoutOutputLyrics.AutoScrollPosition = new Point(0, num);
            }
            else
            {
                num -= flowLayoutPreviewLyrics.AutoScrollPosition.Y;
                if (num < 0)
                {
                    num = 0;
                }
                flowLayoutPreviewLyrics.AutoScrollPosition = new Point(0, num);
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
            gf.OutputItem.CurSlide = NewSlideNumber;
            if (ImplementSlideMove(gf.OutputItem, ScrollToTop: false) && gf.ShowRunning)
            {
                RemoteControlLiveShow(LiveShowAction.Remote_SlideChanged);
            }
            FocusOutputArea();
        }

        private void PreviewRichTextBox_ShowSlide(int NewSlideNumber)
        {
            gf.PreviewItem.CurSlide = NewSlideNumber;
            ImplementSlideMove(gf.PreviewItem, ScrollToTop: false);
        }

        private void HighlightPreviewRichTextBox(bool OnEnter, bool ScrollToTop)
        {
            gf.HighlightRichTextBox(ref Lyrics_PreviewBox, ref flowLayoutPreviewLyrics, gf.PreviewItem, OnEnter, ScrollToTop);
        }

        private void HighlightOutputRichTextBox(bool OnEnter, bool ScrollToTop)
        {
            gf.HighlightRichTextBox(ref Lyrics_OutputBox, ref flowLayoutOutputLyrics, gf.OutputItem, OnEnter, ScrollToTop);
            ShowStatusBarSummary();
        }

        private void ResizePreviewRichTextBox()
        {
            ResizeLyricsRichTextBox(ref Lyrics_PreviewBox, ref flowLayoutPreviewLyrics, gf.PreviewItem);
        }

        private void ResizeOutputRichTextBox()
        {
            ResizeLyricsRichTextBox(ref Lyrics_OutputBox, ref flowLayoutOutputLyrics, gf.OutputItem);
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
            if (gf.OutputItem.TotalSlides > 0)
            {
                LyricsRichTextBox_ShowSlide(gf.OutputItem, gf.OutputItem.TotalSlides);
                HighlightOutputRichTextBox(OnEnter: true, ScrollToTop: false);
            }
        }

        private void flowLayoutPreviewLyrics_Click(object sender, EventArgs e)
        {
            if (gf.PreviewItem.TotalSlides > 0)
            {
                LyricsRichTextBox_ShowSlide(gf.PreviewItem, gf.PreviewItem.TotalSlides);
                HighlightPreviewRichTextBox(OnEnter: true, ScrollToTop: false);
            }
        }

        private void flowLayoutPreviewLyrics_Leave(object sender, EventArgs e)
        {
            HighlightPreviewRichTextBox(OnEnter: false, ScrollToTop: false);
        }

        private void flowLayoutOutputLyrics_Leave(object sender, EventArgs e)
        {
            HighlightOutputRichTextBox(OnEnter: false, ScrollToTop: false);
        }

        private void OutputTextBoxLM_KeyUp(object sender, KeyEventArgs e)
        {
            if ((e.KeyCode == Keys.Return) | (e.KeyCode == Keys.Return))
            {
                SendLyricsMonitorMessage();
            }
        }

        private void OutputTextBoxLM_Enter(object sender, EventArgs e)
        {
            if (gf.UseFocusedTextRegionColour)
            {
                Color InBackground = OutputTextBoxLM.BackColor;
                gf.SetEnterColour(ref InBackground);
                OutputTextBoxLM.BackColor = InBackground;
            }
            OutputTextBoxLM.SelectAll();
        }

        private void OutputTextBoxLM_Leave(object sender, EventArgs e)
        {
            Color InBackground = OutputTextBoxLM.BackColor;
            gf.SetLeaveColor(ref InBackground);
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
            gf.LyricsAlertDetails = DataUtil.Trim(OutputTextBoxLM.Text);
            OutputTextBoxLM.Text = DataUtil.Trim(OutputTextBoxLM.Text);
            AlertWindow_OnMessage(2, "");
        }

        private void OutputInfo_KeyUp(object sender, KeyEventArgs e)
        {
            Keys keyCode = e.KeyCode;
            ItemKeyPressed(gf.OutputItem, keyCode, e.Shift);
        }

        private void GotoNextNonRotateItem(SongSettings InItem)
        {
            SaveWorshipList();
            int nextNonRotateItem = gf.GetNextNonRotateItem((InItem.Type == "G") ? true : false);
            bool flag = false;
            if (gf.GapItemOption == GapType.None)
            {
                if (nextNonRotateItem != gf.StartPresAt)
                {
                    ManualMoveToItem(InItem, nextNonRotateItem);
                }
            }
            else if (nextNonRotateItem == gf.StartPresAt)
            {
                if (InItem.Type != "G")
                {
                    flag = true;
                }
            }
            else if (nextNonRotateItem > 1)
            {
                flag = true;
            }
            if (flag)
            {
                gf.StartPresAt = nextNonRotateItem;
                InItem.CurItemNo = gf.StartPresAt;
                gf.Launch_StartPresAt = gf.StartPresAt;
                LoadItem(ref InItem, "G1", "", 0);
                if (gf.ShowRunning)
                {
                    gf.MainAction_SongChanged_Transaction = ImageTransitionControl.TransitionAction.AsStored;
                    gf.MainAction_MoveToItemKeyDirection = KeyDirection.Refresh;
                    RemoteControlLiveShow(LiveShowAction.Remote_SongJumpTo);
                }
            }
            FocusOutputArea();
            UpdateWorshipShowIcons();
        }

        private void SaveInfoFilePreview(bool ReloadImageData)
        {
            if (!(gf.PreviewItem.Type == "I"))
            {
                return;
            }
            string songSequence = gf.PreviewItem.SongSequence;
            gf.PreviewItem.SongSequence = gf.PreviewItem.SongOriginalLoadedSequence;
            if (gf.SaveXMLInfoScreen(gf.PreviewItem, gf.PreviewItem.ItemID, null, ReloadImageData, UseOriginalNotations: true) && gf.FormInUse("InfoScreen"))
            {
                gf.InfoScreenAction = InfoType.FormatStringUpdate;
                gf.InfoScreenFileName = gf.PreviewItem.ItemID;
                gf.InfoScreenNewFormatString = gf.PreviewItem.Format.FormatString;
                gf.InfoScreenBackgroundPicture = gf.PreviewItem.Format.BackgroundPicture;
                if (ReloadImageData)
                {
                    gf.InfoScreenLoadNewBackground = true;
                }
                gf.InfoScreen_RequestReceived = true;
            }
            gf.PreviewItem.SongSequence = songSequence;
        }

        private void Ind_checkBox_Click(object sender, EventArgs e)
        {
            Ind_checkBox_Action();
        }

        private void Ind_checkBox_Action()
        {
            if (Ind_checkBox.Checked)
            {
                gf.PreviewItem.Format.BackgroundPicture = gf.BackgroundPicture;
                gf.PreviewItem.Format.BackgroundMode = gf.BackgroundMode;
                Ind_NoImage.Enabled = ((gf.PreviewItem.Format.BackgroundPicture != "") ? true : false);
                gf.LoadIndividualFormatData(ref gf.PreviewItem, GetNewFormatString());
                AllowIndividualFormat(AllowFormat: true, BoxChecked: true);
                if (gf.PreviewItem.Source == ItemSource.WorshipList)
                {
                    int selectedIndex = gf.GetSelectedIndex(WorshipListItems);
                    if (selectedIndex >= 0)
                    {
                        WorshipListItems.Items[selectedIndex].SubItems[2].Text = gf.PreviewItem.Format.FormatString;
                    }
                    SaveWorshipList();
                }
                else if (gf.PreviewItem.Source == ItemSource.SongsList)
                {
                    gf.SaveFormatStringToDatabase(gf.PreviewItem.ItemID, gf.PreviewItem.Format.FormatString);
                }
            }
            else
            {
                string formatString = gf.PreviewItem.Format.FormatString;
                ClearFormatPicture();
                NoIndividualFormat();
                if (gf.PreviewItem.Source == ItemSource.WorshipList)
                {
                    if (formatString != gf.PreviewItem.Format.FormatString)
                    {
                        WorshipListIndexChanged();
                        if (gf.OutputItem.ItemID != "" && gf.PreviewItem.ItemID == gf.OutputItem.ItemID)
                        {
                            CopyPreviewToOutput();
                        }
                    }
                }
                else if (gf.PreviewItem.Source == ItemSource.SongsList)
                {
                    gf.SaveFormatStringToDatabase(gf.PreviewItem.ItemID, "");
                }
            }
            SaveInfoFilePreview(ReloadImageData: true);
        }

        private void Menu_BlackScreen_Click(object sender, EventArgs e)
        {
            if (!gf.GlobalHookKey_F9 && !gf.GlobalHookKey_F10)
            {
                LiveBlack(!gf.ShowLiveBlack);
            }
        }

        /// <summary>
        /// OutPreview Slide Up Slide Down Global Hook Arrow Up Arrow Down
        /// </summary>
        public void AddHookSlideUpDown()
        {
            try
            {
                if (gf.GlobalHookKey_Arrow || gf.GlobalHookKey_CtrlArrow)
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
                if (gf.GlobalHookKey_F7 || gf.GlobalHookKey_F8 || gf.GlobalHookKey_F9 || gf.GlobalHookKey_F10)
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

        #region Event handlers of particular events. They will be activated when an appropriate checkbox is checked.
        //daniel ??�� 2024??
        private void HookManager_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.F7 when gf.GlobalHookKey_F7:
                    frmMain.BeginInvoke(new Action(() =>
                    {
                        if (gf.PreviewItem.ItemID != "")
                        {
                            CopyPreviewToOutput();
                            frmMain.BeginInvoke(new Action(() => { LiveBlack(gf.ShowLiveBlack = false); }));
                        }
                    }));
                    break;
                case Keys.F8 when gf.GlobalHookKey_F8:
                    frmMain.BeginInvoke(new Action(() =>
                    {
                        if (gf.PreviewItem.ItemID != "")
                        {
                            CopyPreviewToOutput();
                        }
                    }));
                    break;
                case Keys.F9 when gf.GlobalHookKey_F9:
                    frmMain.BeginInvoke(new Action(() => { LiveBlack(!gf.ShowLiveBlack); }));
                    break;
                case Keys.F10 when gf.GlobalHookKey_F10:
                    frmMain.BeginInvoke(new Action(() => { LiveBlack(!gf.ShowLiveBlack); }));
                    break;
            }
        }

        private void HookManager_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                //case Keys.Left when HookManager.Control && gf.GlobalHookKey_CtrlArrow:
                //case Keys.Right when HookManager.Control && gf.GlobalHookKey_CtrlArrow:
                case Keys.Up when HookManager.Control && gf.GlobalHookKey_CtrlArrow:
                    frmMain.BeginInvoke(new Action(() =>
                    {
                        MoveToSlide(gf.OutputItem, KeyDirection.PrevOne);
                    }));
                    break;
                case Keys.Down when HookManager.Control && gf.GlobalHookKey_CtrlArrow:
                    frmMain.BeginInvoke(new Action(() =>
                    {
                        MoveToSlide(gf.OutputItem, KeyDirection.NextOne);
                    }));
                    break;
                case Keys.Up when gf.GlobalHookKey_Arrow:
                    frmMain.BeginInvoke(new Action(() =>
                    {
                        MoveToSlide(gf.OutputItem, KeyDirection.PrevOne);
                    }));
                    break;
                case Keys.Down when gf.GlobalHookKey_Arrow:
                    //���� ??�� ����?????????�� ??��????��??
                    frmMain.BeginInvoke(new Action(() =>
                    {
                        MoveToSlide(gf.OutputItem, KeyDirection.NextOne);
                    }));
                    break;
            }
        }
        #endregion

        private void Main_QuickFind_Click(object sender, EventArgs e)
        {

        }
    }
}

