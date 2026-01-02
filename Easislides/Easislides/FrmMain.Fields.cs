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
        // Win32 API for window management
        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        private const uint SWP_NOMOVE = 0x0002;
        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_SHOWWINDOW = 0x0040;

        internal enum LiveShowAction
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

        private string[,] tempFolderLyricsHeading = new string[gf.MAXSONGSFOLDERS, 2];

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

        // PowerPoint click handling state
        private int pptLastClickedSlide = -1;
        private Control pptLastClickedControl = null;
        private bool pptDoubleClickInProgress = false;

        // Thumbnail loading state
        static int previousOutSelectedSlide = 1;
        static int LoadThumbOutlockkey = 0;
        static int LoadThumbPreviewlockkey = 0;
        static int previousPreviewSelectedSlide = 1;

    }
}
