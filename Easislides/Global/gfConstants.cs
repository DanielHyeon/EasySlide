//using JRO;
using Easislides.SQLite;
//using Easislides.Model.EasiSlidesDbDataSetTableAdapters;
using Easislides.Util;
//using Microsoft.Office.Interop.Access.Dao;
using Microsoft.Win32;
//using NetOffice.PowerPointApi;
using OfficeLib;
using System;
using System.Collections;
using System.Data;
using System.Data.OleDb;
using System.Data.SQLite;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using Easislides.Module;
using System.Threading;

//using NetOffice.DAOApi;

#if SQLite
using DbConnection = System.Data.SQLite.SQLiteConnection;
using DbDataAdapter = System.Data.SQLite.SQLiteDataAdapter;
using DbCommandBuilder = System.Data.SQLite.SQLiteCommandBuilder;
using DbCommand = System.Data.SQLite.SQLiteCommand;
using DbDataReader = System.Data.SQLite.SQLiteDataReader;
using DbTransaction = System.Data.SQLite.SQLiteTransaction;
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
    internal unsafe partial class gf
	{

		public const string EasiSlides_Version = "4.0.5";

		public const string Database_Version = "4.0B";

		public const string MainRelease = "";

		public const string EasiSlides_Copyright = "2003-2010 Wai Kuen Mo";

		public const string ESFormatSymEnd = "]";

		public const string ESFormatSymStart = "[";

		public const string WDFileSym = "W";

		public const string INFileSym = "I";

		public const string TXFileSym = "T";

		public const string BIFileSym = "B";

		public const string PPFileSym = "P";

		public const string DBFileSym = "D";

		public const string MDFileSym = "M";

		public const string GAFileSym = "G";

		public const string MusicSym = " <#>";

		public const string WorshipListOutputIndicator = "O";

		public const char NotationsSeparator = ';';

		public const char FieldSeparator = '>';

		public const string FieldSeparatorString = ">";

		public const char HBSeparator = ';';

		public const char BibleverseSym0 = ':';

		public const char BibleverseSym1 = '.';

		public const char BibleverseSym2 = 'v';

		public const char BibleverseSym3 = 'V';

		public const string SQLWildCard = "%";

		public const string DAOWildCard = "*";

		public const char ESSongFormatSym = '*';

		public const char ESSequenceSeparator = ',';

		public const string ESFormatNotation = "[~";

		public const string ESFormatSeq = "[@";

		public const string ESFormatTitle = "[>";

		public const string ESFormatSubNotationStart = "(";

		public const string ESFormatSubNotationEnd = ")";

		public const string RTF_Ext = ".rtf";

		public const string TEXT_Ext = ".txt";

		public const string Info_Ext = ".esi";

		public const string WORD_Ext = ".doc";

		public const string PP_Ext = ".ppt";

		public const string ESText32_Ext = ".est";

		public const string ESText_Ext = ".esn";

		public const string ESFT_Ext = ".esft";

		public const string ESF_Ext = ".esf";

		public const string XML_Ext = ".xml";

		public const string OldList_Ext = ".dat";

		public const string EST_Ext = ".est";

		public const string ESW_Ext = ".esw";

		public const string ESP_Ext = ".esp";

		public const string Access_Ext = ".mdb";

		public const string WebPage_Ext = ".htm";

		public const string NotationSym = "쨩";

		public const string MusicFlat = "b";

		public const string MusicSharp = "#";

		public const string vbLf = "\n";

		public const string vbCr = "\r";

		public const string vbTab = "\t";

		public const string vbvTab = "\v";

		public const string vbCrLf = "\r\n";

		public const string est31SongTitle = "[>";

		public const string est31SongTitle2 = ">>";

		public const string est31SongFolder = ">f";

		public const string est31SongCopyright = ">c";

		public const string est31BookReference = ">r";

		public const string est31UserReference = ">u";

		public const string est31SongWriterInfo = ">w";

		public const string est31SongKey = ">k";

		public const string est31SongTiming = ">t";

		public const string est31SongCapo = ">0";

		public const string est31SongNumber = ">n";

		public const string est31SongAdmin1 = ">a";

		public const string est31SongAdmin2 = ">b";

		public const string est31Sequence = ">@";

		public const string est31SongFormat = ">q";			

		public const string SansSerifString = "Microsoft Sans Serif";

		public const string MediaCaptureSymbol = "<<Capture>>";

		public const string DrawLineSymbol = "<<DrawLine>>";

		public const int BIBLE_MODE = 3;

		public const int INVALID_HANDLE_VALUE = -1;

		public const int MAX_SONGS = 20000;

		public const int MAX_VERSES = 9;

		public const int MIN_SCREEN_WIDTH = 9600;

		public const int SongOptNoMusic = 2;

		public const int SongOptMusic = 1;

		public const int SongOptAll = 0;

		public const int DB_MAXSLIDES = 1000;

		public const int MAXSONGSFOLDERS = 41;

		public const int DB_MAXVERSESADD = 3;

		public const int DB_MAXVERSES = 99;

		public const int DB_MAXHEADINGTYPES = 4;

		public const int NOTE = 152;

		public const int NEWSCREEN = 151;

		public const int REGION2 = 150;

		public const int PRECHORUS2 = 112;

		public const int PRECHORUS = 111;

		public const int CHORUS2 = 102;

		public const int ENDING = 101;

		public const int BRIDGE = 100;

		public const int BRIDGE2 = 103;

		public const int CHORUS = 0;

		public const int DB_MAXVERSESARRAY = 160;

		public const int DB_DELFOLDER = 0;

		public const int LIST_ADD = 1;

		public const int LIST_RENAME = 2;

		public const int FOLDER_RENAME = 3;

		public const int PRAISEBOOK_ADD = 4;

		public const int PRAISEBOOK_RENAME = 5;

		public const int INFOSCREEN_NAME = 6;

		public const int LIST_COPY_AS_PRAISEBOOK = 7;

		public const int LIST_COPY_AS_WORSHIPLIST = 8;

		public const int SONG_MOVE = 3;

		public const int SONG_RECOVER = 2;

		public const int SONG_DELETE = 1;

		public const int RTB_IU = 7;

		public const int RTB_BIU = 6;

		public const int RTB_BU = 5;

		public const int RTB_BI = 4;

		public const int RTB_U = 3;

		public const int RTB_I = 2;

		public const int RTB_B = 1;

		public const int RTB_NORMAL = 0;

		public const int MAX_FILENAME_LEN = 260;

		public const string DelimitSym = ">";

		public const string ShowSettingSym = ">";

		public const int SongNumberMaxLength = 6;

		public const string TextfileExt = ".txt";

		public const int MAX_PATH = 260;

		public const long ERROR_FILE_NO_ASSOCIATION = 31L;

		public const long ERROR_FILE_NOT_FOUND = 2L;

		public const long ERROR_PATH_NOT_FOUND = 3L;

		public const long ERROR_FILE_SUCCESS = 32L;

		public const long ERROR_BAD_FORMAT = 11L;

		public const int ShowInputMax = 19;

		public const int PraiseInputMax = 60;

		public const string HeaderDataSym = "=";

		public const int HeaderDataMax = 255;

		public const int UniPosOffset = 65536;

		public const int RTFIndentAmount = 400;

		public const int KEYEVENTF_KEYUP = 2;

		public const int VK_LWIN = 91;

		public const int MAXPictureGroups = 255;

		public const int ThumbDefaultHSpacing = 5;

		public const int ThumbDefaultVSpacing = 5;

		public const int ThumbRootTop = 30;

		public const string HeaderLine = "\n";

		public const int IND_MAX = 10;

		public const int MusicKeyMaxIndex = 11;

		public const int HB_MaxBooks = 66;

		public const int HB_MaxVersions = 250;

		public const int LicAdmin_Max = 9;

		public const int HB_PassagesMax = 3001;

		public const int MaxMusicFileExtensions = 3000;

		public const int MaxRefileSongs = 30000;

		public const int DefaultShowLeftMargin = 2;

		public const int DefaultShowRightMargin = 2;

		public const int DefaultShowBottomMargin = 0;

		public const int DefaultShowRotateGap = 0;

		public const double MainFontHeightFactor = 1.05;

		public const string NewCopyInfoScreenSym = "*NEW*";

		public const string DirSym = "\\";

		public const string DefaultEasiSlidesDir = "C:\\EasiSlides\\";

		public const string DefaultSysDir = "Admin\\";

		public const string DefaultWorshipDir = "Admin\\WorshipLists\\";

		public const string DefaultInfoScreenDirName = "InfoScreen Items";

		public const string DefaultInfoScreenDir = "InfoScreens\\";

		public const string DefaultPowerpointDirName = "Powerpoint Items";

		public const string DefaultPowerpointDir = "Powerpoint\\";

		public const string DefaultDBDir = "Admin\\Database\\";

		public const string DefaultHBDir = "HolyBibles\\";

		public const string DefaultPraiseBookDir = "Admin\\PraiseBooks\\";

		public const string DefaultTemplatesDir = "Admin\\Templates\\";

		public const string DefaultWorshipTemplatesDir = "Admin\\Templates\\WorshipListsTemplates\\";

		public const string DefaultSettingsTemplatesDir = "Admin\\Templates\\SettingsTemplates\\";

		public const string DefaultMediaDirName = "Media Files";

		public const string DefaultMediaDir = "Media\\";

		public const string DefaultImagesDirName = "Images";

		public const string DefaultImagesDir = "Images\\";

		public const string DefaultDocumentsDir = "Documents\\";

		public const string DefaultHTMLDir = "Html\\";

		public const string DefaultDBFilename = "EasiSlidesDb.mdb";

		public const string DefaultUsageFilename = "EsUsage.mdb";

		public const string DefaultBibleDBFilename = "EsBiblesList.mdb";

		public const string DefaultAlertsFilename = "Alerts.txt";

		public const string DefaultParentalFilename = "Parental.txt";

		public const string DefaultMediaExtensionFilename = "MediaExtensions.txt";

		public const string DefaultAudioExtensionFilename = "AudioExtensions.txt";

		public const string DefaultVideoExtensionFilename = "VideoExtensions.txt";

		public const float BorderWidthFactor = 0.04f;

		public const string MissingDBItemTitle = " <Error - Item Not Found>";

		public const int WM_SETREDRAW = 11;

		public const string XMLNode_EasiSlides = "EasiSlides";

		public const string XMLNode_SystemID = "SystemID";

		public const string XMLNode_Item = "Item";

		public const string XMLNode_ItemID = "ItemID";

		public const string XMLNode_ListItem = "ListItem";

		public const string XMLNode_ListHeader = "ListHeader";

		public const string XMLNode_Title1 = "Title1";

		public const string XMLNode_Title2 = "Title2";

		public const string XMLNode_Folder = "Folder";

		public const string XMLNode_SongNumber = "SongNumber";

		public const string XMLNode_Contents = "Contents";

		public const string XMLNode_Notations = "Notations";

		public const string XMLNode_Sequence = "Sequence";

		public const string XMLNode_Writer = "Writer";

		public const string XMLNode_Copyright = "Copyright";

		public const string XMLNode_Category = "Category";

		public const string XMLNode_CJKWordCount = "CJKWordCount";

		public const string XMLNode_CJKStrokeCount = "CJKStrokeCount";

		public const string XMLNode_Timing = "Timing";

		public const string XMLNode_MusicKey = "MusicKey";

		public const string XMLNode_Capo = "Capo";

		public const string XMLNode_LicenceAdmin1 = "LicenceAdmin1";

		public const string XMLNode_LicenceAdmin2 = "LicenceAdmin2";

		public const string XMLNode_BookReference = "BookReference";

		public const string XMLNode_UserReference = "UserReference";

		public const string XMLNode_FormatData = "FormatData";

		public const string XMLNode_Image = "Image";

		public const string XMLNode_Notes = "Notes";

		public const string XMLNode_Settings = "Settings";

		public const string ImageSubFolderScenery = "Scenery";

		public const string ImageSubFolderTiles = "Tiles";

		public const char SuperscriptTagEnd = '\u0098';

		public const int LiveCamMaxDevices = 5;

		private const int SWP_HIDEWINDOW = 128;

		private const int SWP_SHOWWINDOW = 64;

		private const int FO_DELETE = 3;

		private const int FOF_ALLOWUNDO = 64;

		private const int FOF_NOCONFIRMATION = 16;

		public const int SPI_GETSCREENSAVERRUNNING = 114;

		public const int SPI_GETSCREENSAVEACTIVE = 16;

		public const int SPI_SETSCREENSAVEACTIVE = 17;

		public const int SRCCOPY = 13369376;

		public const int SBS_HORZ = 0;

		public const int SBS_VERT = 1;

		public const int SBS_CTL = 2;

		public const int SBS_BOTH = 3;

		public const int WM_VSCROLL = 277;

		public const int WM_HSCROLL = 276;

		public const int SB_LINEUP = 0;

		public const int SB_LINEDOWN = 1;

		public const int SB_PAGEUP = 2;

		public const int SB_PAGEDOWN = 3;

		public const int SB_THUMBPOSITION = 4;

		public const int SB_THUMBTRACK = 5;

		public const int SB_TOP = 6;

		public const int SB_BOTTOM = 7;

		public const int SB_ENDSCROLL = 8;

		public const int EM_GETFIRSTVISIBLELINE = 206;

		public static string SystemID = "";

		public static bool ApplicationFirstRun = false;

		public static bool RestoreSongsDatabase = false;

		public static DateTime PerformanceStartTime;

		public static DateTime MusicBuildStartTime;

		public static TimeSpan MusicBuildLapseTime = new TimeSpan(0L);

		public static bool MusicBuildContinue = true;

		public static bool WorshipListIDOK = false;

		public static Patterns BackPattern = new Patterns();

		public static bool Def_Headings = false;

		public static bool Def_OutlineFont = false;

		public static bool Def_ShadowFont = false;

		public static bool Def_Interleave = false;

		public static bool Def_Notations = false;

		public static bool Def_CapoZero = false;

		public static int Def_Regions = 0;

		public static int Def_VerticalAlign = 0;

		public static int Def_BackroundImage = 0;

		public static int Def_R1Align = 0;

		public static int Def_R2Align = 0;

		public static int Def_MultiMediaMode = 0;

		public static bool Panel_ShowPanel = false;

		public static bool Panel_Totals = false;

		public static bool Panel_Slides = false;

		public static bool Panel_Title = false;

		public static bool Panel_Copyright = false;

		public static bool Panel_TextAsR1 = false;

		public static bool Panel_BackTransparent = false;

		public static bool SplashScreenCanClose = false;

		public static bool SplashScreenBack = false;

		public static bool SplashScreenFront = false;

		public static bool PreviewArea_ShowNotations = true;

		/// <summary>
		/// daniel ?占쎄꼍?占쎈굹 ?占쎌뒪???占쎌씪??1?占쎌뵫 ?占쎌뼱?占?RichEditBox??蹂댁뿬以꾨븣
		/// RichEditBox??borderStyle??FixedSingle:1占??占쎄쾬?占쏙옙? None:0 ?占쎈줈 ??寃껋씤吏 寃곗젙
		/// </summary>
		public static bool PreviewArea_LineBetweenScreens = true;

		public static int PreviewArea_FontSize = 8;

		public static int BibleText_FontSize = 8;

		public static string Def_FormatString = "";

		public static string ImportFolder_StartDir = "";

		public static string ImportFolder_FolderName = "";

		public static Color Def_BackColour1 = BlackScreenColour;

		public static Color Def_BackColour2 = BlackScreenColour;

		public static Color Def_BackColourPattern = BlackScreenColour;

		public static Color Def_R1Colour = BlackScreenColour;

		public static Color Def_R2Color = BlackScreenColour;

		public static Color Panel_TextColour = BlackScreenColour;

		public static Color Panel_BackColour = BlackScreenColour;

		public static Color SelectedTextColour = Color.Red;

		public static Color NormalTextColour = BlackScreenColour;

		public static Color SelectedTextBackgroundColour = BlackScreenColour;

		public static Color NotationColour = Color.Blue;

		public static ImageTransitionControl GlobalImageCanvas = new ImageTransitionControl();

		public static bool RotateSlides = true;

		public static Color ButtonDefaultForeColour = BlackScreenColour;

		public static Color ButtonPressedForeColour = Color.Red;

		public static string DashesString = "----";

		public static string DashesStringSubstitute = "짙!짭~짭";

		public static bool DoRTBOnEnterEvent = false;

		public static ImageTransitionControl tempScreen = new ImageTransitionControl();

		public static int PatternTimerPeriod = 80;

		public static bool DisableTransitions = true;

		public static ListBox tempList = new ListBox();

		public static ListView ListViewNotations = new ListView();

		public static bool UseSongNumbers = false;

		public static int AssignAdminCount = 0;

		public static string AssignAdmin1 = "";

		public static string AssignAdmin2 = "";

		public static string[,] LyricsArray = new string[10000, 3];

		public static int LyricsArrayMax = 0;

		public static bool AutoFocusTextRegion = false;

		public static bool UseFocusedTextRegionColour = true;

		public static Color TransparentColour = Color.FromArgb(255, 16, 0, 16);

		public static Color CaptureTransparentColour = Color.FromArgb(255, 0, 0, 0);

		public static Color BlackScreenColour = Color.FromArgb(255, 0, 0, 0);

		public static Color FocusedTextRegionColour = Color.FromArgb(-2097921);

		public static Color NormalTextRegionBackColour = Color.Black;

		public static Color TextRegionSlideTextColour = Color.Red;

		public static Color TextRegionSlideBackColour = Color.Yellow;

		public static string[,] MediaFileExtension = new string[3000, 2];

		public static int TotalMediaFileExt = 0;

		public static int IndexFileVersion = 0;

		public static int JumpToA = 1;

		public static int JumpToB = 2;

		public static int JumpToC = 3;

		public static int SystemClientMode = 0;

		public static string SystemClientServerName = "";

		public static bool SystemClientServerConnected = false;

		public static int[,] ShowFontVPosition = new int[41, 2];

		public static int[,] ShowFontAlign = new int[41, 2];

		public static int[,] ShowFontSize = new int[41, 2];

		public static int[,] ShowFontBold = new int[41, 4];

		public static int[,] ShowFontItalic = new int[41, 4];

		public static int[,] ShowFontUnderline = new int[41, 4];

		public static int[,] ShowFontRTL = new int[41, 2];

		public static string[,] ShowFontName = new string[41, 2];

		public static int[] ShowLeftMargin = new int[41];

		public static int[] ShowRightMargin = new int[41];

		public static int[] ShowBottomMargin = new int[41];

		public static int ShowRotateGap = 0;

		public static int ShowDataDisplayPrevNext = 0;

		public static int ShowDataDisplayCopyright = 0;

		public static int ShowDataDisplayTitle = 0;

		public static int ShowDataDisplaySongs = 0;

		public static int ShowDataDisplaySlides = 0;

		public static string ShowDataDisplayFontName = "Microsoft Sans Serif";

		public static int ShowDataDisplayFontBold = 0;

		public static int ShowDataDisplayFontItalic = 0;

		public static int ShowDataDisplayFontUnderline = 0;

		public static int ShowDataDisplayFontShadow = 0;

		public static int ShowDataDisplayFontOutline = 0;

		public static int ShowDataDisplayIndicatorsFontSize = 11;

		public static int ShowVerticalAlign = 1;

		public static int ShowLyrics = 0;

		public static int ShowSongHeadings = 0;

		public static int ShowSongHeadingsAlign = 0;

		public static int ShowInterlace = 0;

		public static int ShowCapoZero = 0;

		public static int ShowNotations = 0;

		public static string OUTPPPrefix;

		public static string PREPPPrefix;

		public static string ExtPPrefix;

		public static int ExtPPrefix_Num = 0;

		public static int OUTPPSequence = 0;

		public static int PREPPSequence = 0;

		public static int ExtPPSequence = 0;

		public static string OUTPPFullPath = "";

		public static string PREPPFullPath = "";

		public static string ExtPPFullPath = "";

		public static bool UsePowerpointTab = false;

		public static bool NoPowerpointPanelOverlay = false;

		public static bool UseMediaTab = false;

		public static bool NoMediaPanelOverlay = false;

		public static bool ShowLyricsMonitorAlertBox = false;

		public static bool AutoTextOverflow = true;

		public static bool UseLargestFontSize = true;

		public static bool AdvanceNextItem = false;

		public static bool LineBetweenRegions = true;

		public static GapType GapItemOption = GapType.None;

		public static GapType AltGapItemOption = GapType.None;

		public static bool GapItemUseFade = true;

		public static string GapItemLogoFile = "";

		public static int KeyBoardOption = 0;

		public static int TotalMainEditHistory = 0;

		public static int MaxUserEditHistory = 10;

		public static int AbsoluteMaxHitoryItems = 20;

		public static string[,] TempEditHistoryList = new string[AbsoluteMaxHitoryItems + 1, 2];

		public static string[,] MainEditHistoryList = new string[AbsoluteMaxHitoryItems + 1, 2];

		public static int TotalEditorEditHistory = 0;

		public static string[,] EditorEditHistoryList = new string[AbsoluteMaxHitoryItems + 1, 2];

		public static int TotalInfoScreenEditHistory = 0;

		public static string[,] InfoScreenEditHistoryList = new string[AbsoluteMaxHitoryItems + 1, 2];

		public static string AlertsDataFile = "";

		public static string ParentalDataFile = "";

		public static int AlertGap = 200;

		public static string MediaExtensionsDatafile = "";

		public static string AudioExtensionsDatafile = "";

		public static string VideoExtensionsDatafile = "";

		public static bool FindItemsRequested = false;

		public static bool FindItemsFormOpen = false;

		public static int Options_SelectedTabIndex = 0;

		public static string CurFolderName = "";

		public static int HB_MaxVersesSelection = 1000;

		public static int HB_MaxAdhocVersesSelection = 200;

		public static bool HB_ShowVerses = true;

		public static int PP_MaxFiles = 50;

        //public static int OutputMonitorNumber = 1;

        //public static int LyricsMonitorNumber = 0;
        
		public static bool GlobalHookKey_F7 = false;

        public static bool GlobalHookKey_F8 = false;

        public static bool GlobalHookKey_F9 = false;

        public static bool GlobalHookKey_F10 = false;

        public static bool GlobalHookKey_Arrow = false;

        public static bool GlobalHookKey_CtrlArrow = false;

        public static string OutputMonitorName = "None";

		public static string LyricsMonitorName = "None";

		public static string EditMainFontName;

		public static int EditMainFontSize = 12;

		public static int EditNotationFontSize = 10;

		public static string InfoMainFontName;

		public static int InfoMainFontSize = 12;

		public static int InfoNotationFontSize = 10;

		public static bool InfoMainShowAllButtons = false;

		public static string LicAdminNoSymbol = "#";

		public static bool LicAdminEnforceDisplay = true;

		public static string UserString = "";

		public static bool AlertFormOpen = false;

		public static bool AlertRestoreWindow = false;

		public static string ParentalAlertHeading = "Parental Alert:";

		public static int ParentalAlertDuration = 20;

		public static string ParentalAlertDetails = "";

		public static bool ParentalAlertLive = false;

		public static bool ParentalAlertRequested = false;

		public static int ParentalAlertTextAlign = 3;

		public static int ParentalAlertVerticalAlign = 2;

		public static Color ParentalAlertBackColour = Color.White;

		public static Color ParentalAlertTextColour = Color.Magenta;

		public static bool ParentalAlertScroll = true;

		public static bool ParentalAlertFlash = true;

		public static bool ParentalAlertTransparent = false;

		public static string ParentalAlertFontName = "";

		public static int ParentalAlertFontSize = 25;

		public static int ParentalAlertFontFormat = 0;

		public static bool ParentalAlertBold = false;

		public static bool ParentalAlertItalic = false;

		public static bool ParentalAlertUnderline = false;

		public static bool ParentalAlertShadow = false;

		public static bool ParentalAlertOutline = false;

		public static int MessageAlertDuration = 20;

		public static string MessageAlertDetails = "";

		public static bool MessageAlertLive = false;

		public static bool MessageAlertRequested = false;

		public static int MessageAlertTextAlign = 2;

		public static int MessageAlertVerticalAlign = 2;

		public static Color MessageAlertBackColour = Color.White;

		public static Color MessageAlertTextColour = BlackScreenColour;

		public static bool MessageAlertScroll = true;

		public static bool MessageAlertFlash = true;

		public static bool MessageAlertTransparent = false;

		public static string MessageAlertFontName = "";

		public static int MessageAlertFontSize = 25;

		public static int MessageAlertFontFormat = 0;

		public static bool MessageAlertBold = false;

		public static bool MessageAlertItalic = false;

		public static bool MessageAlertUnderline = false;

		public static bool MessageAlertShadow = false;

		public static bool MessageAlertOutline = false;

		public static int ReferenceAlertDuration = 20;

		public static string ReferenceAlertDetails = "";

		public static bool ReferenceAlertLive = false;

		public static bool ReferenceAlertRequested = false;

		public static int ReferenceAlertTextAlign = 3;

		public static int ReferenceAlertVerticalAlign = 1;

		public static Color ReferenceAlertBackColour = Color.White;

		public static Color ReferenceAlertTextColour = BlackScreenColour;

		public static bool ReferenceAlertScroll = true;

		public static bool ReferenceAlertFlash = false;

		public static bool ReferenceAlertTransparent = false;

		public static string ReferenceAlertFontName = "";

		public static int ReferenceAlertFontSize = 25;

		public static int ReferenceAlertFontFormat = 0;

		public static bool ReferenceAlertBold = false;

		public static bool ReferenceAlertItalic = false;

		public static bool ReferenceAlertUnderline = false;

		public static bool ReferenceAlertShadow = false;

		public static bool ReferenceAlertOutline = false;

		public static int ReferenceAlertSource = 0;

		public static bool ReferenceAlertUsePick = false;

		public static bool ReferenceAlertBlankIfPickNotFound = false;

		public static string ReferenceAlertPickName = "";

		public static string ReferenceAlertPickSubstitute = "";

		public static string ReferenceAlertPickSeparator = ",";

		public static Font ReferenceAlertFont = new Font("Microsoft Sans Serif", 30f);

		public static bool LyricsAlertRequested = false;

		public static string LyricsAlertDetails = "";

		public static int TotalMusicFiles = -1;

		public static string[,] MediaFilesList = new string[32000, 3];

		public static TimeSpan MediaPlayedLapseTime = new TimeSpan(0L);

		public static int OutlineFontSizeThreshold = 55;

		public static int AdjustedOutlineThreshold = 58;

		public static bool WMP_Present = false;

		public static string SymbolsString = "";

		public static int LiveCamNumber = 1;

		public static int LiveCamBalance = 0;

		public static int LiveCamVolume = 50;

		public static bool LiveCamMute = false;

		public static bool LiveCamWidescreen = false;

		public static bool LiveCamNoPanelOverlay = false;

		public static int AlertTimeRemaining = 0;

		public static bool DMAlwaysUseSecondaryMonitor = true;

		public static int DualMonitorSelectAutoOption = 0;

		public static int DMOption1Left = 0;

		public static int DMOption1Top = 0;

		public static int DMOption1Width = 1;

		public static int DMOption1Height = 1;

		public static bool DMOption1AsSingleMonitor = false;

		public static bool DisableSreenSaver = true;

		public static bool LMAlwaysUseSecondaryMonitor = true;

		public static int LMSelectAutoOption = 0;

		public static int LMOption1Left = 0;

		public static int LMOption1Top = 0;

		public static int LMOption1Width = 1;

		public static int LMOption1Height = 1;

		public static Color LMTextColour = BlackScreenColour;

		public static Color LMHighlightColour = Color.Red;

		public static Color LMBackColour = Color.White;

		public static bool LMShowNotations = true;

		public static int LMMainFontSize = 20;

		public static int LMNotationsFontSize = 20;

		public static bool LMFontBold = false;

		public static bool LMFontUnderline = false;

		public static bool LMFontItalic = false;

		public static int LMFontFormat = 0;

		public static int ThumbImagesPerRow = 3;

		public static bool InfoScreen_RequestReceived = false;

		public static bool InfoScreenFormOpen = false;

		public static InfoType InfoScreenAction = InfoType.NoAction;

		public static string InfoScreenFileName = "";

		public static string InfoScreenEditTitle = "";

		public static string InfoScreenNewFormatString = "";

		public static string InfoScreenBackgroundPicture = "";

		public static bool InfoScreenLoadNewBackground = true;

		public static bool InfoScreenLoadItem = false;

		public static bool InfoScreenItemNew = false;

		public static bool EditorLoadItem = false;

		public static bool EditorFormOpen = false;

		public static int EditorItemID = 0;

		public static bool EditorItemNew = false;

		public static bool EditorItemFolderChanged = false;

		public static bool EditorItemTitleChanged = false;

		public static int EditorItemNewFolder = 0;

		public static string EditorItemTitle = "";

		public static string Import_Admin2ColumnName = "";

		public static string Import_Admin1ColumnName = "";

		public static string Import_SongTimingColumnName = "";

		public static string Import_SongNumberColumnName = "";

		public static string Import_SongKeyColumnName = "";

		public static string Import_UserReferenceColumnName = "";

		public static string Import_BookReferenceColumnName = "";

		public static string Import_SongCopyrightColumnName = "";

		public static string Import_SongWriterInfoColumnName = "";

		public static string Import_SongLyricsColumnName = "";

		public static string Import_SongTitle2ColumnName = "";

		public static string Import_SongTitleColumnName = "";

		public static string Import_TableName = "";

		public static string Import_AccessFileName = "";

		public static int SelectedItemsCount = 0;

		public static SongSettings PreviewItem = new SongSettings();

		public static SongSettings OutputItem = new SongSettings();

		public static SongSettings LiveItem = new SongSettings();

		public static SongSettings LyricsItem = new SongSettings();

		public static SongSettings TempItem1 = new SongSettings();

		public static SongSettings TempItem2 = new SongSettings();

		public static SongSettings EditItem1 = new SongSettings();

		public static SongSettings EditItem2 = new SongSettings();

		public static SongSettings InfoItem1 = new SongSettings();

		public static SongSettings InfoItem2 = new SongSettings();

		public static int LS_Height = 0;

		public static int LS_Width = 0;

		public static int LS_Top = 0;

		public static int LS_Left = 0;

		public static int Buffer_LS_Height = 0;

		public static int Buffer_LS_Width = 0;

		public static int LM_Height = 0;

		public static int LM_Width = 0;

		public static int LM_Top = 0;

		public static int LM_Left = 0;

		public static int PreviewSampleFactor = 1;

		public static int PP_TempNum = 0;

		public static int PP_SlidesCount = 0;

		public static int PP_SlidesNum = 0;

		public static Color DefaultBackColour = Color.FromArgb(DataUtil.ObjToInt(-16777056));

		public static Color DefaultForeColour = Color.FromArgb(DataUtil.ObjToInt(-1));

		public static int AutoRotateStyle = 3;

		public static bool AutoRotateOn = true;

		public static bool RestartCurrentItem = false;

		public static bool RestartItemActioned = false;

		public static int PowerpointListingStyle = 0;

		public static bool Alert_Scroll = true;

		public static bool Alert_Flash = true;

		public static bool Alert_tempFlash = true;

		public static bool Alert_Transparent = true;

		public static int Alert_RestCount = 0;

		public static string Alert_OriginalMessage = "";

		public static string Alert_FormattedMessage = "";

		public static string Alert_FormatOriginalSequence = "";

		public static string Alert_FormatSequence = "";

		public static int Alert_MessageLength = 0;

		public static int Alert_FlashCount = 0;

		public static bool Alert_MessageDisplayed = false;

		public static Color Alert_TextColour = Color.Red;

		public static Color Alert_BackColour = Color.White;

		public static Color Alert_TempColour = Color.Red;

		public static int Alert_TextAlign = 3;

		public static int Alert_VerticalAlign = 2;

		public static bool Alert_NewMessage = true;

		public static Font Alert_UserFont = new Font("Microsoft Sans Serif", 30f);

		public static bool Alert_UserFontShadow = true;

		public static bool Alert_UserFontOutline = true;

		public static Font Alert_AdjustedFont = new Font("Microsoft Sans Serif", 30f);

		public static int Alert_MessageHeight = 0;

		public static int Alert_FontSize = 12;

		public static string Alert_Fontname = "";

		public static bool Alert_FontBold = false;

		public static bool Alert_FontItalic = false;

		public static bool Alert_FontUnderline = false;

		public static int ShowRunning_ShowVerticalAlign = 0;

		public static int ShowRunning_ShowInterlace = 0;

		public static int ShowRunning_ShowNotations = 0;

		public static int ShowRunning_ShowLyrics = 0;

		public static int ShowRunning_ShowSongHeadings = 0;

		public static int ShowRunning_UseOutlineFont = 0;

		public static int ShowRunning_UseShadowFont = 0;

		public static int ShowRunning_ShowDataDisplayMode = 0;

		public static bool LaunchShowUpdateDone = false;

		public static int LaunchShowLastItem = 0;

		public static int LaunchShowCurSlide = 0;

		public static bool TabSourceImagesChanged = false;

		public static bool TabSourceExternalFilesChanged = false;

		public static bool TabSourceMediaFolderFilesChanged = false;

		public static Color ChangedBackColour1 = BlackScreenColour;

		public static Color ChangedBackColour2 = BlackScreenColour;

		public static int ChangedBackStyle = 0;

		public static bool ChangedIsDefault = true;

		public static string popUpText = "";

		public static int popUpTextMaxLength = 100;

		public static string[] sArray;

		public static string[] xArray;

		public static RichTextBox tbWorkspace = new RichTextBox();

		public static RichTextBox tbTempSpace = new RichTextBox();

		public static RichTextBox tbLyricsMonitorSpace = new RichTextBox();

		public static RichTextBox tbLyricsNextItemSpace = new RichTextBox();

		public static int ShowBMargin = 0;

		public static int ShowRMargin = 0;

		public static int ShowLMargin = 0;

		public static int ShowLyricsWidth = 0;

		public static int HB_TotalVersions = 0;

		public static string[,] HB_Versions = new string[250, 7];

		public static int HB_CurVersionTabIndex = -1;

		public static int[,] HB_VersesLocation = new int[3001, 7];

		public static string Find_SQLString = "";

		public static string HB_SQLString = "";

		public static bool HB_UsingSearch = false;

		public static bool HB_SequentialListing = true;

		public static string HelpFile_Location;

		//public static DBEngine DAODBEngine_definst = new DBEngine();//"DAO.DBEngine.120");

		public static bool LoadInitData = true;

		public static int MaxBackgroundStyleIndex;

		public static string HTMLPublisher;

		public static string HTMLWeb;

		public static string HTMLInfo1;

		public static bool HTMLMusicLink;

		public static bool HTMLShowIndex;

		public static bool HTMLShowChorusItalics;

		public static bool HTMLShowNumbers;

		public static bool HTMLShowBookRef;

		public static bool HTMLShowUserRef;

		public static bool HTMLDoubleByte;

		public static int HTMLDocumentOption;

		public static int HTMLRTFFontSize = 12;

		public static string FindSearchPhrase = "";

		public static string FindBibleSearchPhrase = "";

		public static int CurMainSelectedFolder = 1;

		public static int MusicSymLen = " <#>".Length;

		public static double TopBorderFactor = 0.022000000476837159;

		public static double MinBottomBorderFactor = 0.08;

		public static double BottomBorderFactor = 0.08;

		public static double RegionsGapFactor = 0.02;

		public static int ShowTopBorderSize;

		public static int ShowBottomBorderSize;

		public static int SearchOption = 0;

		public static string[] SequenceSymbol = Enumerable.Repeat(string.Empty, 160).ToArray();// new string[160] 

		public static int[] Reg_NumArray = new int[9];

		public static string RootEasiSlidesDir;

		public static int[] PB_WordsUnderline = new int[8];

		public static int[] PB_WordsItalic = new int[8];

		public static int[] PB_WordsBold = new int[8];

		public static int[] PB_WordsSize = new int[8];

		public static int[] PB_ShowWords = new int[8];

		public static Color[] PB_WordsColour = new Color[8];

		public static SortBy PB_CJKGroupStyle = SortBy.Alpha;

		public static int PB_ShowScreenBreaks = 0;

		public static int PB_OneSongPerPage = 0;

		public static int[] PB_Spacing = new int[2];

		public static int PB_ShowColumns = 0;

		public static int PB_ShowSection = 0;

		public static int PB_LyricsPattern = 0;

		public static int PB_PageSize = 0;

		public static int[] PB_ShowHeadings = new int[4];

		public static string PB_DocumentName;

		public static string PB_FullFileName;

		public static PraiseBookLayout PB_Layout = PraiseBookLayout.WorshipList;

		public static int PB_PrinterSpaces = 0;

		public static int PB_CapoZero = 0;

		public static int PB_ShowCapo = 0;

		public static int PB_ShowKey = 0;

		public static int PB_ShowTiming = 0;

		public static int PB_ShowNotations = 0;

		public static int DB_CurSongID;

		public static string Edit_CurFolderTitle = "";

		public static bool Edit_RequestReceived = false;

		public static bool Edit_HistoryMaxChanged = false;

		public static string EditBible_Title;

		public static string EditBible_IDString;

		public static string[] VerseSymbol = Enumerable.Repeat(string.Empty, 160).ToArray();// new string[160];

		public static string[] VerseTitle = Enumerable.Repeat(string.Empty, 160).ToArray();  //new string[160];

		public static string[] VerseFormatSymbol = new string[6];

		public static string EULA;

		public static bool PraiseBooksListChanged;

		public static bool WorshipListsChanged;

		public static bool NameChangeSucceeded;

		public static int FolderRenameNo = 0;

		public static int NameChangeAction = 0;

		public static string CurPraiseBook;

		public static string CurPraiseBookNotes = "";

		public static string[] FolderRenameName = new string[41];

		public static string SelectedListName;

		public static string InfoScreenDir;

		public static string PowerpointDir;

		public static string WorshipTemplatesDir;

		public static string SettingsTemplatesDir;

		public static string BibleDir;

		public static string ImagesDir;

		public static string PraiseOutputDir;

		public static string ImportFromDir;

		public static string ExportToDir;

		public static string DocumentsDir;

		public static string PraiseBookDir;

		public static string MediaDir;

		public static string EasiSlidesTempDir;

		public static string WorshipDir;

		public static string EditOpenDocumentDir;

		public static string CurSession;

		public static string CurSessionNotes = "";

		public static string EditNotes = "";

		public static string EditNotesHeading = "";

		public static string[,] FolderLyricsHeading = new string[41, 4];

		public static string[] FolderName = new string[41];

		public static int[] FolderHeadingPercentSize = new int[41];

		public static int[,] FolderHeadingFontBold = new int[41, 2];

		public static int[,] FolderHeadingFontItalic = new int[41, 2];

		public static int[,] FolderHeadingFontUnderline = new int[41, 2];

		public static int[] FolderHeadingOption = new int[41];

		public static double[,] ShowLineSpacing = new double[41, 2];

		public static int[] FolderUse = new int[41];

		public static SortBy[] FolderGroupStyle = new SortBy[41];

		public static int UseOutlineFont = 0;

		public static int UseShadowFont = 0;

		public static ImageMode BackgroundMode = ImageMode.Tile;

		public static int ShowSlideTransition = 0;

		public static int ShowItemTransition = 0;

		public static Color[] ShowFontColour = new Color[2];

		public static Color[] ShowScreenColour = new Color[2];

		public static int ShowScreenStyle = 0;

		public static string BackgroundPicture = "";

		public static int VideoSize = 100;

		public static int VideoVAlign = 1;

		public static string MediaLocation = "";

		public static string CurrentMediaLocation = "";

		public static string CurrentMediaOutputMonitorName = "";

		public static bool CurrentMediaIsVideo = false;

		public static int MediaOption = 0;

		public static int MediaVolume = 50;

		public static int MediaBalance = 0;

		public static int MediaMute = 0;

		public static int MediaRepeat = 0;

		public static int MediaWidescreen = 0;

		public static int MediaCaptureDeviceNumber = 1;

		public static string MediaOutputMonitorName = "";

		public static bool MediaNotifyRepeatItem = false;

		public static bool MediaCurrentItemIsVideo = false;

		public static bool MediaNotifyItemConnecting = false;

		public static bool MediaNotifyItemCannotPlay = false;

		public static bool MediaDoRotate = false;

		public static bool MediaResetStartTime = false;

		public static bool MediaLengthAsRotateLength = false;

		public static DateTime MediaLiveItemStartTime = default(DateTime);

		public static string Temp_MediaItemType = "";

		public static string Temp_MediaTitle1 = "";

		public static string Temp_MediaTitle2 = "";

		public static string Temp_MediaLocation = "";

		public static string Temp_MediaItemTitleLocation = "";

		public static int Temp_MediaOption = 0;

		public static int Temp_MediaVolume = 50;

		public static int Temp_MediaBalance = 0;

		public static int Temp_MediaMute = 0;

		public static int Temp_MediaRepeat = 0;

		public static int Temp_MediaWidescreen = 0;

		public static int Temp_MediaCaptureDeviceNumber = 1;

		public static string Temp_MediaOutputMonitorName = "";

		public static double[,] MainFontSpacingFactor = new double[41, 2];

		public static double NotationFontFactor = 0.75;

		public static string Lookup_NameSelected;

		public static string Lookup_NameBookRef;

		public static string Lookup_NameUserRef;

		public static bool SplashUp = true;

		public static bool FindBibleVerses;

		public static bool FindFolderItems;

		public static bool FindItemRestoreWindow = false;

		public static bool FindItemInTitle = true;

		public static bool FindItemInSongNumber = true;

		public static bool FindItemInBookRef = true;

		public static bool FindItemInUserRef = true;

		public static bool FindItemInContents = true;

		public static bool FindItemInWriter = false;

		public static bool FindItemInCopyright = false;

		public static bool FindItemInLicAdmin = false;

		public static bool FindItemMediaOnly = false;

		public static bool FindItemNotationsOnly = false;

		public static string FindItemWithKey = "";

		public static string FindItemWithTiming = "";

		public static bool FindItemUseDates = false;

		public static DateTime FindItemDateFrom;

		public static DateTime FindItemDateTo;

		public static int FindBibleBookIndex = 0;

		public static string tempDBFileName = "";

		public static string tempBiblesListFileName = "";

		public static string tempUsageFileName = "";

		public static string BiblesListFileName = "";

		public static string UsageFileName = "";

		public static string DBFileName = "";

#if SQLite
		public static string ConnectStringDef = "Data Source=";
#else
		public static string ConnectStringDef = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=";
#endif

		public static string ConnectStringSQLiteDef = "Data Source=";

		public static string ConnectSQLiteDef = "Data Source=";

		public static string ConnectStringMainDB = "";

		public static string ConnectStringUsageDB = "";

		public static string ConnectStringBibleDB = "";

		public static string ExportFileName = "";

		public static string HBFilename = "";

		public static string ListingDirPath = "";

		public static string Listing_File_Name = "";

		public static int StartPresAt = 0;

		public static int Launch_StartPresAt = 0;

		public static bool AdHocItemPresent = false;

		public long MainFormHwnd = 0L;

		public static string[,] LicAdmin_List = new string[9, 3];

		public static string SFMPTSData = "";

		public static string BookRefData = "";

		public static bool[] FindSongsFolder = new bool[41];

		public static bool AllDirOK = false;

		public static bool AppCloseDown = false;

		public static string[,] WorshipSongs = new string[20000, 5];

		public static string[,] DocumentSongs = new string[20000, 5];

		public static int TotalWorshipListItems = 0;

		public static int TotalPraiseBookItems = 0;

		public static int TitlePosTop = 0;

		public static int TitleFontSize = 0;

		public static int ByteSize = 0;

		public static string Rename_String = "";

		public static string Rename_ExistingString = "";

		public static bool Options_MediaExtChanged = false;

		public static bool Options_MediaDirChanged = false;

		public static bool Options_MaxHistoryListChanged = false;

		public static bool Options_FolderListChanged = false;

		public static bool Options_FolderFormatChanged = false;

		public static bool Options_BibleListChanged = false;

		public static bool Options_PreviewAreaChanged = false;

		public static bool Options_DMChanged = false;

		public static int ShowDataDisplayMode;

		public static bool ShowRunning = false;

		public static Color PanelTextColour;

		public static Color PanelBackColour;

		public static int PanelTextColourAsRegion1 = 1;

		public static int PanelBackColourTransparent = 1;

		public static string SongFormatData = "";

		public static string[] FontsList = new string[2000];

		public static int FontsListMaxIndex = -1;

		public static string[] ShowInput = new string[60];

		public static string[] HeaderData = new string[255];

		public static string[] tempHeaderData = new string[255];

		public static bool SaveShowOptions;

		public static UsageMode EasiSlidesMode = UsageMode.Worship;

		public static int[] StrokeCount = new int[65536];

		public static bool PB_FormatChanged;

		public static string[] RTFIndent = new string[6];

		public static string RTFTab = "\\tab ";

		public static string RTFLineSpacing;

		public static string RTFNewLine;

		public static int[] RTFTabValue = new int[9];

		public static int MoveToFolder;

		public static int CopyToFolder;

		public static string ExternalMoveCopyType = "";

		public static int ExternalMoveFolder;

		public static int ExternalCopyFolder;

		public static string GenHTMLDir;

		public static string[,] PictureGroups = new string[255, 3];

		public static string[,] InfoScreenGroups = new string[255, 3];

		public static string[,] MediaGroups = new string[255, 3];

		public static string[,] PowerpointGroups = new string[255, 3];

		public static int InfoScreenFolderTotal = 0;

		public static int MediaFolderTotal = 0;

		public static int PowerpointFolderTotal = 0;

		public static int PicFolderTotal = 0;

		public static PowerPoint LivePP = new PowerPoint();

		public static PowerPoint PreviewPPT = new PowerPoint();

		public static PowerPoint OutputPPT = new PowerPoint();

		public static PowerPoint ExternalPPT = new PowerPoint();

		public static string[,] PowerpointList = new string[100000, 2];

		public static int TotalPowerpointItems = 0;

		public static bool ShowLiveBlack = false;

		public static bool ShowLiveClear = false;

		public static bool ShowLiveCam = false;

		public static bool WordWrapLeftAlignIndent = true;

		public static int WordWrapIgnoreStartSpaces = 8;

		public static bool DualMonitorMode = false;

		public static bool ShowDebugVideoMessages = false;

		public static string OutputMonitorText = "";

		public static string[] HBBookName = new string[67];

		public static string[] HBVersionBookName = new string[67];

		public UnicodeEncoding UnicodeEncoder = new UnicodeEncoding();

		public static string[] MusicMajorKeys = new string[14];

		public static string[] MusicMinorKeys = new string[14];

		public static string[,] MusicMajorChords = new string[13, 2];

		public static string[,] MusicMinorChords = new string[13, 2];

		public static int[] MusicMajorKeysFlatSharp = new int[14];

		public static int[] MusicMinorKeysFlatSharp = new int[14];

		public static bool TimerFlashOn = false;

		public static bool PreviewMouseHeldDown = false;

		public static bool OutputMouseHeldDown = false;

		public static bool LyricsMonitor_LyricsChanged = false;

		public static bool LyricsMonitor_WorshipListChanged = false;

		public static KeyDirection MainAction_MoveToItemKeyDirection = KeyDirection.Refresh;

		public static ImageTransitionControl.TransitionAction MainAction_SongChanged_Transaction = ImageTransitionControl.TransitionAction.AsStored;

		public static MPCType MPC_Type = MPCType.Session;

		public static string DefaultBackgroundID = "";

		public static object missing = Missing.Value;

		public static string[] NotationsArray = new string[32000];

		public static readonly object def = Missing.Value;

		public static bool PriorScreenSaverState = false;

		//daniel  ?占쏀겕占??占쎌씠占?4:3 , wide
		public static bool isScreenWideMode = false;

		private gf()
		{
		}

		[DllImport("user32.dll")]
		private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

		[DllImport("user32.dll", SetLastError = true)]
		private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

		[DllImport("shell32.dll", EntryPoint = "FindExecutableA")]
		public static extern int FindExecutable(string lpFile, string lpDirectory, string lpResult);

		public static void HideTaskBar()
		{
			IntPtr hWnd = FindWindow("Shell_TrayWnd", "");
			SetWindowPos(hWnd, IntPtr.Zero, 0, 0, 0, 0, 128u);
		}

		public static void ShowTaskBar()
		{
			IntPtr hWnd = FindWindow("Shell_TrayWnd", "");
			SetWindowPos(hWnd, IntPtr.Zero, 0, 0, 0, 0, 64u);
		}

		[DllImport("shell32.dll", CharSet = CharSet.Auto)]
		public static extern int SHFileOperation(ref SHFILEOPSTRUCT lpFileOp);

		[DllImport("user32.dll")]
		public static extern int SendMessageW(IntPtr hwnd, uint msg, uint wParam, uint lParam);

		[DllImport("user32", CharSet = CharSet.Auto)]
		public static extern bool SystemParametersInfo(int uAction, int uParam, ref bool lpvParam, int fuWinIni);

		[DllImport("user32.dll")]
		public static extern bool InvalidateRect(IntPtr hwnd, IntPtr lpRect, bool bErase);

		[DllImport("Gdi32.dll")]
		public static extern IntPtr CreateCompatibleDC(IntPtr hdc);

		[DllImport("Gdi32.dll")]
		public static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int width, int height);

		[DllImport("Gdi32.dll")]
		public static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);

		[DllImport("gdi32.dll")]
		public static extern IntPtr DeleteDC(IntPtr hDc);

		[DllImport("gdi32.dll")]
		public static extern IntPtr DeleteObject(IntPtr hDc);

		[DllImport("gdi32.dll")]
		public static extern bool BitBlt(IntPtr hdcDest, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hdcSrc, int nXSrc, int nYSrc, int dwRop);

		public static delegate* unmanaged[Stdcall]<IntPtr, uint, uint, uint, int> SendMessage = (delegate* unmanaged[Stdcall]<IntPtr, uint, uint, uint, int>)(delegate*<IntPtr, uint, uint, uint, int>)&gf.SendMessageW;


		// Win32 API ?좎뼵 - ShowWindow, SetForegroundWindow 異붽?
		[DllImport("user32.dll")]
		private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

		[DllImport("user32.dll")]
		private static extern bool SetForegroundWindow(IntPtr hWnd);

		[DllImport("user32.dll")]
		private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

		[DllImport("user32.dll")]
		private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

		[DllImport("user32.dll")]
		private static extern bool IsWindowVisible(IntPtr hWnd);

		[DllImport("user32.dll")]
		private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

		[StructLayout(LayoutKind.Sequential)]
		private struct RECT
		{
			public int Left;
			public int Top;
			public int Right;
			public int Bottom;
		}

		private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

		private static readonly IntPtr HWND_TOP = new IntPtr(0);
		private const int SW_MAXIMIZE = 3;
	}
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
public struct SHFILEOPSTRUCT
{
	public IntPtr hwnd;
	public uint wFunc;
	public string pFrom;
	public string pTo;
	public ushort fFlags;
	public bool fAnyOperationsAborted;
	public IntPtr hNameMappings;
	public string lpszProgressTitle;
}
