using Axicultus.Properties;
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
using Axicultus.Util;
using System.Threading;
using Axicultus.SQLite;
using Axicultus.Module;
using MethodInvoker = System.Windows.Forms.MethodInvoker;
using Type = System.Type;
using System.Diagnostics;
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

namespace Axicultus;

public partial class FrmMain : Form
{
    // Win32 API for window management

    private static readonly IntPtr HWND_TOPMOST = new(-1);
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

    private Color Pic_BorderColour = Gf.BlackScreenColour;

    private Color Pic_HighlightColour = Color.Red;

    private Color DefaultButtonForeColour = Color.Gray;

    private bool InitFormLoad = true;

    private string InContents = "";

    private bool ImplementFolderChange = false;

    private SortBy CurStyle = SortBy.Alpha;

    private string[] sArray;

    private string[,] tempFolderLyricsHeading = new string[Gf.MAXSONGSFOLDERS, 2];

    private string[] Verse = new string[160];

    private string KeyCapoText = "";

    private string[] BackgroundImagename = [];

    private string BackgroundCurImagePath = "";

    private string[] ExternalPPImagename = [];

    private string ExternalPPCurImagePath = "";

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

    private PowerPoint MainPPT = new();

    private readonly System.Collections.Generic.Dictionary<Control, Font> previewAreaOwnedFonts = [];

    private int frmHeight;

    private int frmWidth;

    private int frmTop;

    private int frmLeft;

    // Named so that a paint failure in the log and the error notice says which screen failed.
    private ImageTransitionControl PreviewScreen = new() { Name = "PreviewScreen" };

    private ImageTransitionControl OutputScreen = new() { Name = "OutputScreen" };

    private int ThumbImageClicked = 0;

    private bool WorshipListDoubleClick = false;

    private int LoadRepaintCount = 0;

    private bool FormFirstLoad = true;

    private FrmLaunchShow LiveShow = new();

    private FrmShowAlert AlertWindow = new();

    public static FrmMain frmMain { get; private set; }

    // PowerPoint click handling state
    private bool pptDoubleClickInProgress = false;

    // Thumbnail loading state

}
