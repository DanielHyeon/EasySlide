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

		public const string NotationSym = "»";

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
		/// daniel 성경이나 텍스트 파일을 1절씩 읽어와 RichEditBox에 보여줄때
		/// RichEditBox의 borderStyle을 FixedSingle:1로 할것인지 None:0 으로 할 것인지 결정
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

		public static string DashesStringSubstitute = "£!¬~¬";

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

		public static bool CurrentMediaIsVideo = false;

		public static int MediaOption = 0;

		public static int MediaVolume = 50;

		public static int MediaBalance = 0;

		public static int MediaMute = 0;

		public static int MediaRepeat = 0;

		public static int MediaWidescreen = 0;

		public static int MediaCaptureDeviceNumber = 1;

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

		//daniel  스크린 사이즈 4:3 , wide
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

	public static bool InitEasiSlidesDir()
		{
			RootEasiSlidesDir = RegUtil.GetRegValue("config", "root_directory", "C:\\EasiSlides\\");
			if (RootEasiSlidesDir == "")
			{
				RootEasiSlidesDir = "C:\\EasiSlides\\";
			}
			string regValue = RegUtil.GetRegValue("config", "version", "none");
			string regValue2 = RegUtil.GetRegValue("config", "database", "none");
			if (regValue != "4.0.5")
			{
				RegUtil.SaveRegValue("config", "version", "4.0.5");
			}
			if (regValue2 != "4.0B")
			{
				ApplicationFirstRun = true;
				RegUtil.SaveRegValue("config", "database", "4.0B");
			}
			if (!ValidateRootFolder())
			{
				return false;
			}
			RegUtil.SaveRegValue("config", "root_directory", RootEasiSlidesDir);
			ValidateID();
#if SQLite
			DBFileName = RootEasiSlidesDir + "Admin\\Database\\EasiSlidesDb.db";
#else
			DBFileName = RootEasiSlidesDir + "Admin\\Database\\EasiSlidesDb.mdb";
#endif

			bool flag = File.Exists(DBFileName);

			if (ApplicationFirstRun || !flag)
			{
				CovertItemsTov4();
				if (!RestoreSongsDatabase && ApplicationFirstRun)
				{
					SplashScreenBack = true;
					if (MessageBox.Show("Would you like to Install the EasiSlides Lyrics Database supplied with EasiSlides 4.0.5? Your existing Lyrics Database, if any, will be renamed and retained for backup.  Click Yes to Install, or No to continue using existing database.", "New EasiSlides Version 4.0.5 ... Replace existing Database with Supplied Database?", MessageBoxButtons.YesNo) == DialogResult.Yes)
					{
						RestoreSongsDatabase = true;
					}
				}
				else if (!RestoreSongsDatabase && !flag)
				{
					SplashScreenBack = true;
					if (MessageBox.Show("Cannot find the Lyrics Database.  Would you like to Install the EasiSlides Lyrics Database supplied with EasiSlides 4.0.5?  Click Yes to Install, or No to create a new blank database.", "Loading EasiSlides 4.0.5 ... Install Supplied Database?", MessageBoxButtons.YesNo) == DialogResult.Yes)
					{
						RestoreSongsDatabase = true;
					}
				}
			}
			if (RestoreSongsDatabase)
			{
				string text = RestoreOriginalSongsDatabase();
				if (text != "-1")
				{
					if (ApplicationFirstRun)
					{
						if (text != "")
						{
							MessageBox.Show("Lyrics Database installed successfully. " + ((text != "") ? ("Existing database has been renamed to: " + text) : ""));
						}
					}
					else
					{
						MessageBox.Show("Lyrics Database installed successfully. " + ((text != "") ? ("Existing database has been renamed to: " + text) : ""));
					}
				}
			}
			SplashScreenFront = true;
			return true;
		}

		public static void CovertItemsTov4()
		{
			WorshipDir = RootEasiSlidesDir + "Admin\\WorshipLists\\";
			RenameExtensions(WorshipDir, ".dat", ".esw");
			PraiseBookDir = RootEasiSlidesDir + "Admin\\PraiseBooks\\";
			RenameExtensions(PraiseBookDir, ".dat", ".esp");
			string name = "music_dir";
			string regValue = RegUtil.GetRegValue("config", name, RootEasiSlidesDir + "Music\\");
			if (regValue == RootEasiSlidesDir + "Music\\")
			{
				try
				{
					Directory.Move(regValue, RootEasiSlidesDir + "Media\\");
					RegUtil.SaveRegValue("config", "media_dir", RootEasiSlidesDir + "Media\\");
				}
				catch
				{
				}
			}
			else if (regValue != "")
			{
				RegUtil.SaveRegValue("config", "media_dir", regValue);
			}
			RegUtil.DeleletRegKey("config", name);
			string text = RootEasiSlidesDir + "Admin\\Database\\MusicExtensions.txt";
			string text2 = RootEasiSlidesDir + "Admin\\Database\\MediaExtensions.txt";
			if (File.Exists(text) && !File.Exists(text2))
			{
				try
				{
					File.Move(text, text2);
				}
				catch
				{
				}
			}
			string text3 = RootEasiSlidesDir + "User Images";
			string text4 = RootEasiSlidesDir + "Images\\";
			if (Directory.Exists(text3) && !Directory.Exists(text4))
			{
				try
				{
					Directory.Move(text3, text4);
				}
				catch
				{
				}
			}
		}

		public static void MessageOverSplashScreen(string InString)
		{
			SplashScreenBack = true;
			MessageBox.Show(InString);
			SplashScreenBack = false;
		}

		public static void RenameExtensions(string InDir, string InOldExt, string InNewExt)
		{
			try
			{
				string[] files = Directory.GetFiles(InDir, "*" + InOldExt);
				string[] array = files;
				foreach (string text in array)
				{
					try
					{
						File.Move(text, InDir + "\\" + Path.GetFileNameWithoutExtension(text) + InNewExt);
					}
					catch
					{
					}
				}
			}
			catch
			{
			}
		}

		public static bool InitAppData()
		{
			PerformanceStartTime = DateTime.Now;
			LoadInitData = true;
			LoadEulaText();
			//if (!LoadUnicodeStrokeCount())
			//{
			//	return false;
			//}
			HelpFile_Location = Application.StartupPath + "\\Sys\\EasiSlidesHelp.chm";
			MusicSymLen = " <#>".Length;
			MaxBackgroundStyleIndex = BackPattern.MaxStyleIndex;
			ShowScreenColour[0] = DefaultBackColour;
			ShowScreenColour[1] = DefaultBackColour;
			ShowScreenStyle = 0;
			ShowFontColour[0] = DefaultBackColour;
			ShowFontColour[1] = DefaultBackColour;
			//for (int i = 1; i < 160; i++)
			//{
			//	VerseTitle[i] = "";
			//	SequenceSymbol[i] = "";
			//}
			VerseTitle[0] = "chorus";
			for (int i = 1; i <= 99; i++)
			{
				VerseTitle[i] = DataUtil.ObjToString(i);
			}
			VerseTitle[100] = "bridge";
			VerseTitle[103] = VerseTitle[100] + " 2";
			VerseTitle[101] = "ending";
			VerseTitle[102] = VerseTitle[0] + " 2";
			VerseTitle[111] = "prechorus";
			VerseTitle[112] = VerseTitle[111] + " 2";
			VerseTitle[150] = "region 2";
			VerseTitle[151] = "\n";
			VerseTitle[152] = "note";
			SymbolsString = "";
			for (int i = 1; i <= 99; i++)
			{
				VerseSymbol[i] = "[" + VerseTitle[i] + "]";
				SequenceSymbol[i] = VerseTitle[i];
				SymbolsString = SymbolsString + VerseSymbol[i] + ",";
			}
			//for (int i = 100; i < 160; i++)
			//{
			//	VerseSymbol[i] = "";
			//	SequenceSymbol[i] = "";
			//}
			VerseSymbol[0] = "[" + VerseTitle[0] + "]";
			VerseSymbol[100] = "[" + VerseTitle[100] + "]";
			VerseSymbol[103] = "[" + VerseTitle[103] + "]";
			VerseSymbol[101] = "[" + VerseTitle[101] + "]";
			VerseSymbol[102] = "[" + VerseTitle[102] + "]";
			VerseSymbol[111] = "[" + VerseTitle[111] + "]";
			VerseSymbol[112] = "[" + VerseTitle[112] + "]";
			VerseSymbol[150] = "[" + VerseTitle[150] + "]";
			VerseSymbol[151] = VerseTitle[151];
			VerseSymbol[152] = "[" + VerseTitle[152] + "]";
			SequenceSymbol[0] = "c";
			SequenceSymbol[100] = "b";
			SequenceSymbol[103] = "w";
			SequenceSymbol[101] = "e";
			SequenceSymbol[102] = "t";
			SequenceSymbol[111] = "p";
			SequenceSymbol[112] = "q";
			string symbolsString = SymbolsString;
			SymbolsString = symbolsString + VerseSymbol[0] + "," + VerseSymbol[102] + "," + VerseSymbol[100] + "," + VerseSymbol[103] + "," + VerseSymbol[111] + "," + VerseSymbol[112] + "," + VerseSymbol[101] + "," + VerseSymbol[150];
			xArray = SymbolsString.Split(',');
			ShowLMargin = Screen.PrimaryScreen.Bounds.Width / 50;
			ShowRMargin = ShowLMargin;
			ShowLyricsWidth = Screen.PrimaryScreen.Bounds.Width - ShowLMargin - ShowRMargin;

#if SQLite
			UsageFileName = RootEasiSlidesDir + "Admin\\Database\\EsUsage.db";
			BiblesListFileName = RootEasiSlidesDir + "Admin\\Database\\EsBiblesList.db";
			tempDBFileName = RootEasiSlidesDir + "Admin\\Database\\~tempEasiSlidesDb.db";
			tempUsageFileName = RootEasiSlidesDir + "Admin\\Database\\~tempEsUsage.db";
			tempBiblesListFileName = RootEasiSlidesDir + "Admin\\Database\\~tempEsBiblesList.db";
#else
			UsageFileName = RootEasiSlidesDir + "Admin\\Database\\EsUsage.mdb";
			BiblesListFileName = RootEasiSlidesDir + "Admin\\Database\\EsBiblesList.mdb";
            tempDBFileName = RootEasiSlidesDir + "Admin\\Database\\~tempEasiSlidesDb.mdb";
            tempUsageFileName = RootEasiSlidesDir + "Admin\\Database\\~tempEsUsage.mdb";
            tempBiblesListFileName = RootEasiSlidesDir + "Admin\\Database\\~tempEsBiblesList.mdb";
#endif



			ConnectStringMainDB = ConnectStringDef + DBFileName;
			ConnectStringUsageDB = ConnectStringDef + UsageFileName;
			ConnectStringBibleDB = ConnectStringDef + BiblesListFileName;
			UserString = DataUtil.Trim(RegUtil.GetRegValue("config", "RegistrationUser", ""));

            //속도 개선 필요
            //var task2 = Task.Run<bool>(() =>
            //{
            if (!ValidateVer_3_4_Fields())
            {
                return false;
            }
            //    return Task.FromResult(true);
            //});

            LoadSavedData();

			GenerateMusicKeysList();
			SizeLaunchScreen();
			ResetShowRunningSettings();

			AlertsDataFile = RootEasiSlidesDir + "Admin\\Database\\Alerts.txt";
			ParentalDataFile = RootEasiSlidesDir + "Admin\\Database\\Parental.txt";
			string startupPath = Application.StartupPath;
			PreviewItem.Initialise();
			OutputItem.Initialise();
			LiveItem.Initialise();
			LyricsItem.Initialise();
			TempItem1.Initialise();
			EditItem1.Initialise();
			EditItem2.Initialise();
			InfoItem1.Initialise();
			InfoItem2.Initialise();
			OutputItem.OutputStyleScreen = true;
			SetListViewColumns(ListViewNotations, 5);
			var task1 = Task.Run(() =>
			{
				LivePP.Init();

				PreviewPPT.Init();
				OutputPPT.Init();
				ExternalPPT.Init();
			});

			SetPatternPeriod();

            //속도 개선 필요
            //var task2 = Task.Run<bool>(() =>
            //{
            //    if (!ValidateVer_3_4_Fields())
            //    {
            //        return Task.FromResult(false);
            //    }
            //    return Task.FromResult(true);
            //});

            return true;
		}

		public static void LoadEulaText()
		{
			EULA = "EasiSlides Software\n\nIMPORTANT: This software end user licence agreement ('EULA') is a legal agreement between you and EasiSlides. Read it carefully before completing the installation process and using the software. It provides a licence to use the software. By installing and using the software, you are confirming your acceptance of the software and agreeing to become bound by the terms of this agreement. If you do not agree with the terms of this licence you must remove EasiSlides Software files from your storage devices and cease to use the product.\n\nAll copyrights to 'EasiSlides Software', hereafter shall be referred to as 'the software', are exclusively owned by EasiSlides. Your licence confers no title or ownership in the software and should not be construed as a sale of any right in the software.\n\nYou MUST NOT use this software for purposes which are unlawful, including, but not limited to, the transmission of obscene or offensive content, or contents which may harass or cause distress to any person.\n\nYou may use this software for any length of time.\n\nYou are hereby licenced to make any number of backup copies of this software and documentation. You can give the copy of the software to anyone or distribute the software provided you abide by the following Licence restrictions:\n(a) You may not reproduce or distribute the software for the purpose of promoting other non-EasiSlides products or organisations unless specific permission to do so have been obtained from the EasiSlides Copyright holder.\n(b) You may not alter, merge, modify, adapt or translate the software, or decompile, reverse engineer, disassemble, or otherwise reduce the Software to a human-perceivable form.\n(c) You may not rent, lease, or sublicence the Software.\n(d) Where the software is placed on a network for distribution, you must place alongside the distributed software a fully functional and visible hyperlink to the official EasiSlides website at http://www.EasiSlides.com.\n(e) No fee is charged for the software.\n\nEASISLIDES SOFTWARE IS DISTRIBUTED 'AS IS'. NO WARRANTY OF ANY KIND IS EXPRESSED OR IMPLIED. YOU USE IT AT YOUR OWN RISK. EASISLIDES WILL NOT BE LIABLE FOR DATA LOSS, DAMAGES, LOSS OF PROFITS OR ANY OTHER KIND OF LOSS WHILE USING OR MISUSING THIS SOFTWARE.\n\nThis EULA  shall be governed by and construed in accordance with the laws of Northern Ireland. Any dispute arising under this EULA shall be subject to the exclusive jurisdiction of the courts of Northern Ireland.\n\nCopyright © 2007 EasiSlides, All rights reserved.\nInternet:  http://www.EasiSlides.com\n";
		}

		public static void ValidateID()
		{
			SystemID = RegUtil.GetRegValue("config", "ID", "");
			if (SystemID.Length < 12)
			{
				SystemID = DataUtil.Right(Guid.NewGuid().ToString(), 12);
				RegUtil.SaveRegValue("config", "ID", SystemID);
			}
			else if (SystemID.Length > 12)
			{
				SystemID = DataUtil.Left(SystemID, 12);
				RegUtil.SaveRegValue("config", "ID", SystemID);
			}
		}

		public static string GetUniqueID()
		{
			return DataUtil.Right(Guid.NewGuid().ToString(), 6);
		}

		public static void SetPatternPeriod()
		{
			TimeSpan timeSpan = DateTime.Now.Subtract(PerformanceStartTime);
			if (timeSpan.TotalSeconds < 12.0)
			{
				PatternTimerPeriod = 40;
			}
			else if (timeSpan.TotalSeconds < 25.0)
			{
				PatternTimerPeriod = 80;
			}
			else
			{
				PatternTimerPeriod = 200;
			}
		}

		public static int InsertItemIntoDatabase(string InConnectString, SongSettings InItem)
		{
			return InsertItemIntoDatabase(InConnectString, InItem.Title, InItem.Title2, InItem.SongNumber, InItem.FolderNo, InItem.CompleteLyrics, InItem.SongSequence, InItem.Writer, InItem.Copyright, InItem.Capo, InItem.Timing, InItem.MusicKey, InItem.Notations, InItem.Category, InItem.Show_LicAdminInfo1, InItem.Show_LicAdminInfo2, InItem.Book_Reference, InItem.User_Reference, InItem.Settings, InItem.Format.FormatString);
		}

		public static int InsertItemIntoDatabase(string InConnectString, object Title_1, object Title_2, object song_number, int FolderNo, object Lyrics, object Sequence, object writer, object copyright, object capo, object Timing, object Key, object msc, object CATEGORY, object LICENCE_ADMIN1, object LICENCE_ADMIN2, object BOOK_REFERENCE, object USER_REFERENCE, object Settings, object FormatData)
		{
			string title_ = Title_1.ToString();
			string title_2 = Title_2.ToString();
			int song_number2 = DataUtil.StringToInt(song_number.ToString());
			string lyrics = Lyrics.ToString();
			string sequence = Sequence.ToString();
			string writer2 = writer.ToString();
			string copyright2 = copyright.ToString();
			int capo2 = DataUtil.StringToInt(capo.ToString(), Minus1IfBlank: true);
			string timing = Timing.ToString();
			string inKey = Key.ToString();
			string msc2 = msc.ToString();
			string cATEGORY = CATEGORY.ToString();
			string lICENCE_ADMIN = LICENCE_ADMIN1.ToString();
			string lICENCE_ADMIN2 = LICENCE_ADMIN2.ToString();
			string bOOK_REFERENCE = BOOK_REFERENCE.ToString();
			string uSER_REFERENCE = USER_REFERENCE.ToString();
			string sETTINGS = Settings.ToString();
			string fORMATDATA = FormatData.ToString();
			return InsertItemIntoDatabase(InConnectString, title_, title_2, song_number2, FolderNo, lyrics, sequence, writer2, copyright2, capo2, timing, inKey, msc2, cATEGORY, lICENCE_ADMIN, lICENCE_ADMIN2, bOOK_REFERENCE, uSER_REFERENCE, sETTINGS, fORMATDATA);
		}

#if DAO
		public static int InsertItemIntoDatabase(string InConnectString, string Title_1, string Title_2, int song_number, int FolderNo, string Lyrics, string Sequence, string writer, string copyright, int capo, string Timing, string InKey, string msc, string CATEGORY, string LICENCE_ADMIN1, string LICENCE_ADMIN2, string BOOK_REFERENCE, string USER_REFERENCE, string SETTINGS, string FORMATDATA)
		{
			Title_1 = DataUtil.Left(Title_1, 100);
			Title_2 = DataUtil.Left(Title_2, 100);
			Sequence = DataUtil.Left(Sequence, 100);
			writer = DataUtil.Left(writer, 100);
			copyright = DataUtil.Left(copyright, 100);
			Timing = DataUtil.Left(Timing, 50);
			InKey = DataUtil.Left(InKey, 20);
			LICENCE_ADMIN1 = DataUtil.Left(LICENCE_ADMIN1, 50);
			LICENCE_ADMIN2 = DataUtil.Left(LICENCE_ADMIN2, 50);
			BOOK_REFERENCE = DataUtil.Left(BOOK_REFERENCE, 50);
			string value = DataUtil.CJK_WordCount(Title_1);
			string value2 = DataUtil.CJK_StrokeCount(Title_1);
			Recordset tableRecordSet = DbDaoController.GetTableRecordSet(InConnectString, "SONG");
			if (tableRecordSet != null)
			{
				try
				{
					tableRecordSet.AddNew();
					tableRecordSet.Fields["Title_1"].Value = Title_1;
					tableRecordSet.Fields["Title_2"].Value = Title_2;
					tableRecordSet.Fields["song_number"].Value = song_number;
					tableRecordSet.Fields["FolderNo"].Value = FolderNo;
					tableRecordSet.Fields["Lyrics"].Value = Lyrics;
					tableRecordSet.Fields["Sequence"].Value = Sequence;
					tableRecordSet.Fields["writer"].Value = writer;
					tableRecordSet.Fields["copyright"].Value = copyright;
					tableRecordSet.Fields["CJK_WordCount"].Value = value;
					tableRecordSet.Fields["CJK_StrokeCount"].Value = value2;
					tableRecordSet.Fields["capo"].Value = capo;
					tableRecordSet.Fields["Timing"].Value = Timing;
					tableRecordSet.Fields["Key"].Value = InKey;
					tableRecordSet.Fields["msc"].Value = msc;
					tableRecordSet.Fields["CATEGORY"].Value = CATEGORY;
					tableRecordSet.Fields["LICENCE_ADMIN1"].Value = LICENCE_ADMIN1;
					tableRecordSet.Fields["LICENCE_ADMIN2"].Value = LICENCE_ADMIN2;
					tableRecordSet.Fields["BOOK_REFERENCE"].Value = BOOK_REFERENCE;
					tableRecordSet.Fields["USER_REFERENCE"].Value = USER_REFERENCE;
					tableRecordSet.Fields["SETTINGS"].Value = SETTINGS;
					tableRecordSet.Fields["FORMATDATA"].Value = FORMATDATA;
					tableRecordSet.Fields["LastModified"].Value = DateTime.Now.Date;
					tableRecordSet.Update();
					Array ppsach = tableRecordSet.LastModified;
					tableRecordSet.set_Bookmark(ref ppsach);
					return DataUtil.ObjToInt(tableRecordSet.Fields["SongID"].Value);
				}
				catch
				{
					MessageBox.Show("Error encountered whilst adding item to database - item NOT added");
				}
			}
			return 0;
		}
#elif SQLite
		public static int InsertItemIntoDatabase(string InConnectString, string Title_1, string Title_2, int song_number, int FolderNo, string Lyrics, string Sequence, string writer, string copyright, int capo, string Timing, string InKey, string msc, string CATEGORY, string LICENCE_ADMIN1, string LICENCE_ADMIN2, string BOOK_REFERENCE, string USER_REFERENCE, string SETTINGS, string FORMATDATA)
		{
			Title_1 = DataUtil.Left(Title_1, 100);
			Title_2 = DataUtil.Left(Title_2, 100);
			Sequence = DataUtil.Left(Sequence, 100);
			writer = DataUtil.Left(writer, 100);
			copyright = DataUtil.Left(copyright, 100);
			Timing = DataUtil.Left(Timing, 50);
			InKey = DataUtil.Left(InKey, 20);
			LICENCE_ADMIN1 = DataUtil.Left(LICENCE_ADMIN1, 50);
			LICENCE_ADMIN2 = DataUtil.Left(LICENCE_ADMIN2, 50);
			BOOK_REFERENCE = DataUtil.Left(BOOK_REFERENCE, 50);
			string value = DataUtil.CJK_WordCount(Title_1);
			string value2 = DataUtil.CJK_StrokeCount(Title_1);

			

			(DbDataAdapter sQLiteDataAdapter, DataTable dt) = DbController.GetDataAdapter(InConnectString, "Select * from SONG");

			if (dt != null)
			{
				try
				{
					dt.AcceptChanges();

					DbCommandBuilder sqCB = new DbCommandBuilder(sQLiteDataAdapter);
					sQLiteDataAdapter.InsertCommand = sqCB.GetInsertCommand();

					DataRow dr = dt.NewRow();

					dr["Title_1"] = Title_1;
					dr["Title_2"] = Title_2;
					dr["song_number"] = song_number;
					dr["FolderNo"] = FolderNo;
					dr["Lyrics"] = Lyrics;
					dr["Sequence"] = Sequence;
					dr["writer"] = writer;
					dr["copyright"] = copyright;
					dr["CJK_WordCount"] = value;
					dr["CJK_StrokeCount"] = value2;
					dr["capo"] = capo;
					dr["Timing"] = Timing;
					dr["Key"] = InKey;
					dr["msc"] = msc;
					dr["CATEGORY"] = CATEGORY;
					dr["LICENCE_ADMIN1"] = LICENCE_ADMIN1;
					dr["LICENCE_ADMIN2"] = LICENCE_ADMIN2;
					dr["BOOK_REFERENCE"] = BOOK_REFERENCE;
					dr["USER_REFERENCE"] = USER_REFERENCE;
					dr["SETTINGS"] = SETTINGS;
					dr["FORMATDATA"] = FORMATDATA;
					dr["LastModified"] = DateTime.Now.Date;

					dt.Rows.Add(dr);					

					sQLiteDataAdapter.Update(dt);  // Its also update in database.	

					return DataUtil.ObjToInt(dr["SongID"]);
				}
				catch
				{
					MessageBox.Show("Error encountered whilst adding item to database - item NOT added");
				}
			}
			return 0;
		}
#endif

		public static bool UpdateDatabaseItem(string InConnectString, SongSettings InItem, int SongID)
		{
			return UpdateDatabaseItem(InConnectString, SongID, InItem.Title, InItem.Title2, InItem.SongNumber, InItem.FolderNo, InItem.CompleteLyrics, InItem.SongSequence, InItem.Writer, InItem.Copyright, InItem.Capo, InItem.Timing, InItem.MusicKey, InItem.Notations, InItem.Category, InItem.Show_LicAdminInfo1, InItem.Show_LicAdminInfo2, InItem.Book_Reference, InItem.User_Reference, InItem.Settings, InItem.Format.FormatString);
		}

		public static bool UpdateDatabaseItem(string InConnectString, int SongID, string Title_1, string Title_2, int song_number, int FolderNo, string Lyrics, string Sequence, string writer, string copyright, int capo, string Timing, string InKey, string msc, string CATEGORY, string LICENCE_ADMIN1, string LICENCE_ADMIN2, string BOOK_REFERENCE, string USER_REFERENCE, string SETTINGS, string FORMATDATA)
		{
			bool result = false;
			Title_1 = DataUtil.Left(Title_1, 100);
			Title_2 = DataUtil.Left(Title_2, 100);
			Sequence = DataUtil.Left(Sequence, 100);
			writer = DataUtil.Left(writer, 100);
			copyright = DataUtil.Left(copyright, 100);
			Timing = DataUtil.Left(Timing, 50);
			InKey = DataUtil.Left(InKey, 20);
			LICENCE_ADMIN1 = DataUtil.Left(LICENCE_ADMIN1, 50);
			LICENCE_ADMIN2 = DataUtil.Left(LICENCE_ADMIN2, 50);
			BOOK_REFERENCE = DataUtil.Left(BOOK_REFERENCE, 50);
			string value = DataUtil.CJK_WordCount(Title_1);
			string value2 = DataUtil.CJK_StrokeCount(Title_1);
			try
			{
#if OleDb
				using (OleDbConnection daoDb = DbConnectionController.GetOleDbConnection(ConnectStringMainDB))
				{
					try
					{
						string text = "Update SONG SET Title_1 =@Title_1,Title_2 =@Title_2,song_number =@song_number,FolderNo =@FolderNo,Lyrics =@Lyrics,Sequence =@Sequence,writer =@writer,copyright =@copyright,CJK_WordCount =@CJK_WordCount,CJK_StrokeCount =@CJK_StrokeCount,capo =@capo,Timing =@Timing,[Key] =@Key,msc =@msc,CATEGORY =@CATEGORY,LICENCE_ADMIN1 =@LICENCE_ADMIN1,LICENCE_ADMIN2 =@LICENCE_ADMIN2,BOOK_REFERENCE =@BOOK_REFERENCE,USER_REFERENCE =@USER_REFERENCE,SETTINGS =@SETTINGS,FORMATDATA =@FORMATDATA,LastModified =@LastModified where songid=" + SongID;
						OleDbCommand command = new OleDbCommand(text, daoDb);
						command.CommandText = text;
						command.Parameters.AddWithValue("@Title_1", Title_1);
						command.Parameters.AddWithValue("@Title_2", Title_2);
						command.Parameters.AddWithValue("@song_number", song_number);
						command.Parameters.AddWithValue("@FolderNo", FolderNo);
						command.Parameters.AddWithValue("@Lyrics", Lyrics);
						command.Parameters.AddWithValue("@Sequence", Sequence);
						command.Parameters.AddWithValue("@writer", writer);
						command.Parameters.AddWithValue("@copyright", copyright);
						command.Parameters.AddWithValue("@CJK_WordCount", value);
						command.Parameters.AddWithValue("@CJK_StrokeCount", value2);
						command.Parameters.AddWithValue("@capo", capo);
						command.Parameters.AddWithValue("@Timing", Timing);
						command.Parameters.AddWithValue("@Key", InKey);
						command.Parameters.AddWithValue("@msc", msc);
						command.Parameters.AddWithValue("@CATEGORY", CATEGORY);
						command.Parameters.AddWithValue("@LICENCE_ADMIN1", LICENCE_ADMIN1);
						command.Parameters.AddWithValue("@LICENCE_ADMIN2", LICENCE_ADMIN2);
						command.Parameters.AddWithValue("@BOOK_REFERENCE", BOOK_REFERENCE);
						command.Parameters.AddWithValue("@USER_REFERENCE", USER_REFERENCE);
						command.Parameters.AddWithValue("@SETTINGS", SETTINGS);
						command.Parameters.AddWithValue("@FORMATDATA", FORMATDATA);
						command.Parameters.AddWithValue("@LastModified", DateTime.Now.Date);
						command.ExecuteNonQuery();
						result = true;
					}
					catch (Exception ex)
					{
						MessageBox.Show("Error when writing to Database: \n" + ex.ToString());
					}
				}
#elif SQLite
				using DbConnection connection = DbController.GetDbConnection(ConnectStringMainDB);
				string text = "Update SONG SET Title_1 =@Title_1,Title_2 =@Title_2,song_number =@song_number,FolderNo =@FolderNo,Lyrics =@Lyrics,Sequence =@Sequence,writer =@writer,copyright =@copyright,CJK_WordCount =@CJK_WordCount,CJK_StrokeCount =@CJK_StrokeCount,capo =@capo,Timing =@Timing,[Key] =@Key,msc =@msc,CATEGORY =@CATEGORY,LICENCE_ADMIN1 =@LICENCE_ADMIN1,LICENCE_ADMIN2 =@LICENCE_ADMIN2,BOOK_REFERENCE =@BOOK_REFERENCE,USER_REFERENCE =@USER_REFERENCE,SETTINGS =@SETTINGS,FORMATDATA =@FORMATDATA,LastModified =@LastModified where songid=" + SongID;
				using DbCommand command = new DbCommand(text, connection);
				command.CommandText = text;
				command.Parameters.AddWithValue("@Title_1", Title_1);
				command.Parameters.AddWithValue("@Title_2", Title_2);
				command.Parameters.AddWithValue("@song_number", song_number);
				command.Parameters.AddWithValue("@FolderNo", FolderNo);
				command.Parameters.AddWithValue("@Lyrics", Lyrics);
				command.Parameters.AddWithValue("@Sequence", Sequence);
				command.Parameters.AddWithValue("@writer", writer);
				command.Parameters.AddWithValue("@copyright", copyright);
				command.Parameters.AddWithValue("@CJK_WordCount", value);
				command.Parameters.AddWithValue("@CJK_StrokeCount", value2);
				command.Parameters.AddWithValue("@capo", capo);
				command.Parameters.AddWithValue("@Timing", Timing);
				command.Parameters.AddWithValue("@Key", InKey);
				command.Parameters.AddWithValue("@msc", msc);
				command.Parameters.AddWithValue("@CATEGORY", CATEGORY);
				command.Parameters.AddWithValue("@LICENCE_ADMIN1", LICENCE_ADMIN1);
				command.Parameters.AddWithValue("@LICENCE_ADMIN2", LICENCE_ADMIN2);
				command.Parameters.AddWithValue("@BOOK_REFERENCE", BOOK_REFERENCE);
				command.Parameters.AddWithValue("@USER_REFERENCE", USER_REFERENCE);
				command.Parameters.AddWithValue("@SETTINGS", SETTINGS);
				command.Parameters.AddWithValue("@FORMATDATA", FORMATDATA);
				command.Parameters.AddWithValue("@LastModified", DateTime.Now.Date);
				command.ExecuteNonQuery();
				result = true;
#endif
			}
			catch (Exception ex)
			{
				MessageBox.Show("Error when writing to Database: \n" + ex.Message + ex.StackTrace);
			}
			return result;
		}

		public static void AssignDropDownItem(ref ToolStripDropDownButton SelectedBtn, string SelectedMenuItemName, ToolStripMenuItem InMenuItem1, ToolStripMenuItem InMenuItem2)
		{
			string text = "";
			string text2 = "";
			if (SelectedMenuItemName == InMenuItem2.Name)
			{
				SelectedBtn.Image = InMenuItem2.Image;
				text = (string)InMenuItem2.Tag;
				text2 = InMenuItem2.Text;
			}
			else
			{
				SelectedBtn.Image = InMenuItem1.Image;
				text = (string)InMenuItem1.Tag;
				text2 = InMenuItem1.Text;
			}
			SelectedBtn.Tag = text;
			SelectedBtn.ToolTipText = text2;
		}

		public static void AssignDropDownItem(ref ToolStripDropDownButton SelectedBtn, string SelectedMenuItemName, ToolStripMenuItem InMenuItem1, ToolStripMenuItem InMenuItem2, ToolStripMenuItem InMenuItem3)
		{
			string text = "";
			string text2 = "";
			if (SelectedMenuItemName == InMenuItem2.Name)
			{
				SelectedBtn.Image = InMenuItem2.Image;
				text = (string)InMenuItem2.Tag;
				text2 = InMenuItem2.Text;
			}
			else if (SelectedMenuItemName == InMenuItem3.Name)
			{
				SelectedBtn.Image = InMenuItem3.Image;
				text = (string)InMenuItem3.Tag;
				text2 = InMenuItem3.Text;
			}
			else
			{
				SelectedBtn.Image = InMenuItem1.Image;
				text = (string)InMenuItem1.Tag;
				text2 = InMenuItem1.Text;
			}
			SelectedBtn.Tag = text;
			SelectedBtn.ToolTipText = text2;
		}

		public static void AssignDropDownItem(ref ToolStripDropDownButton SelectedBtn, string SelectedMenuItemName, ToolStripMenuItem InMenuItem1, ToolStripMenuItem InMenuItem2, ToolStripMenuItem InMenuItem3, ToolStripMenuItem InMenuItem4)
		{
			string text = "";
			string text2 = "";
			if (SelectedMenuItemName == InMenuItem2.Name)
			{
				SelectedBtn.Image = InMenuItem2.Image;
				text = (string)InMenuItem2.Tag;
				text2 = InMenuItem2.Text;
			}
			else if (SelectedMenuItemName == InMenuItem3.Name)
			{
				SelectedBtn.Image = InMenuItem3.Image;
				text = (string)InMenuItem3.Tag;
				text2 = InMenuItem3.Text;
			}
			else if (SelectedMenuItemName == InMenuItem4.Name)
			{
				SelectedBtn.Image = InMenuItem4.Image;
				text = (string)InMenuItem4.Tag;
				text2 = InMenuItem4.Text;
			}
			else
			{
				SelectedBtn.Image = InMenuItem1.Image;
				text = (string)InMenuItem1.Tag;
				text2 = InMenuItem1.Text;
			}
			SelectedBtn.Tag = text;
			SelectedBtn.ToolTipText = text2;
		}

		public static void AssignDropDownItem(ref ToolStripDropDownButton SelectedBtn, string SelectedMenuItemName, ToolStripMenuItem InMenuItem1, ToolStripMenuItem InMenuItem2, ToolStripMenuItem InMenuItem3, ToolStripMenuItem InMenuItem4, ToolStripMenuItem InMenuItem5)
		{
			string text = "";
			string text2 = "";
			if (SelectedMenuItemName == InMenuItem2.Name)
			{
				SelectedBtn.Image = InMenuItem2.Image;
				text = (string)InMenuItem2.Tag;
				text2 = InMenuItem2.Text;
			}
			else if (SelectedMenuItemName == InMenuItem3.Name)
			{
				SelectedBtn.Image = InMenuItem3.Image;
				text = (string)InMenuItem3.Tag;
				text2 = InMenuItem3.Text;
			}
			else if (SelectedMenuItemName == InMenuItem4.Name)
			{
				SelectedBtn.Image = InMenuItem4.Image;
				text = (string)InMenuItem4.Tag;
				text2 = InMenuItem4.Text;
			}
			else if (SelectedMenuItemName == InMenuItem5.Name)
			{
				SelectedBtn.Image = InMenuItem5.Image;
				text = (string)InMenuItem5.Tag;
				text2 = InMenuItem5.Text;
			}
			else
			{
				SelectedBtn.Image = InMenuItem1.Image;
				text = (string)InMenuItem1.Tag;
				text2 = InMenuItem1.Text;
			}
			SelectedBtn.Tag = text;
			SelectedBtn.ToolTipText = text2;
		}

		public static Color SelectNewColour(Color CurColour)
		{
			Color InColour = CurColour;
			SelectColor(ref InColour);
			return InColour;
		}

		public static bool SelectColorFromBtn(ref Button InBtn, ref Color ColourSymbol)
		{
			Color InColour = InBtn.ForeColor;
			if (SelectColor(ref InColour))
			{
				InBtn.ForeColor = InColour;
				ColourSymbol = InColour;
				return true;
			}
			return false;
		}

		public static bool SelectColorFromBtn(ref ToolStripButton InBtn, ref Color ColourSymbol)
		{
			Color InColour = InBtn.ForeColor;
			if (SelectColor(ref InColour))
			{
				InBtn.ForeColor = InColour;
				ColourSymbol = InColour;
				return true;
			}
			return false;
		}

		public static bool SelectColor(ref Color InColour)
		{
			ColorDialog colorDialog = new ColorDialog();
			colorDialog.Color = InColour;
			if (colorDialog.ShowDialog() == DialogResult.OK)
			{
				InColour = colorDialog.Color;
				return true;
			}
			return false;
		}

		public static bool SelectBackgroundColors(ref ToolStripButton InBtn, ref Color InColour1, ref Color InColour2, ref int InStyle, bool IsDefault)
		{
			ChangedBackColour1 = InColour1;
			ChangedBackColour2 = InColour2;
			ChangedBackStyle = InStyle;
			ChangedIsDefault = IsDefault;
			FrmBackground frmBackground = new FrmBackground();
			if (frmBackground.ShowDialog() == DialogResult.OK)
			{
				InColour1 = ChangedBackColour1;
				InColour2 = ChangedBackColour2;
				InStyle = ChangedBackStyle;
				InBtn.ForeColor = InColour1;
				return true;
			}
			return false;
		}

		public static void BuildFontsList(ref ToolStripComboBox InTSComboBox)
		{
			if (FontsListMaxIndex < 0)
			{
				InstalledFontCollection installedFontCollection = new InstalledFontCollection();
				FontFamily[] families = installedFontCollection.Families;
				FontsListMaxIndex = -1;
				FontFamily[] array = families;
				foreach (FontFamily fontFamily in array)
				{
					FontsList[++FontsListMaxIndex] = fontFamily.Name;
				}
			}
			if (FontsListMaxIndex >= 0)
			{
				InTSComboBox.Items.Clear();
				InTSComboBox.Sorted = false;
				for (int j = 0; j <= FontsListMaxIndex; j++)
				{
					InTSComboBox.Items.Add(FontsList[j]);
				}
				InTSComboBox.Sorted = true;
				InTSComboBox.SelectedIndex = 0;
			}
		}

		public static void BuildFontSizeList(ref ToolStripComboBox InCombo)
		{
			InCombo.Items.Clear();
			InCombo.Items.Add("8");
			InCombo.Items.Add("9");
			InCombo.Items.Add("10");
			InCombo.Items.Add("11");
			InCombo.Items.Add("12");
			InCombo.Items.Add("13");
			InCombo.Items.Add("14");
			InCombo.Items.Add("15");
			InCombo.Items.Add("16");
			InCombo.Items.Add("17");
			InCombo.Items.Add("18");
			InCombo.Items.Add("19");
			InCombo.Items.Add("20");
		}

		public static string GetDisplayNameOnly(ref string InFileName, bool UpdateByRef)
		{
			return GetDisplayNameOnly(ref InFileName, UpdateByRef, KeepExt: false);
		}

		public static string GetDisplayNameOnly(ref string InFileName, bool UpdateByRef, bool KeepExt)
		{
			if ((InFileName == null) | (InFileName == ""))
			{
				return "";
			}
			string text = "";
			try
			{
				text = ((!KeepExt) ? Path.GetFileNameWithoutExtension(InFileName) : Path.GetFileName(InFileName));
				if (UpdateByRef)
				{
					InFileName = text;
				}
			}
			catch
			{
			}
			return text;
		}

		public static bool LoadUnicodeStrokeCount1()
		{
			string InString = "";
			if (LoadFileContents(Application.StartupPath + "\\Sys\\strokecount.dat", ref InString))
			{
				int i = 1;
				for (int num = InString.Length - 2; i <= num - 2; i += 3)
				{
					int num2 = DataUtil.ObjToInt(InString.Substring(i, 2));
					if (num2 > 0)
					{
						int num3 = InString[i - 1];
						if (num3 < 0)
						{
							num3 += 65536;
						}
						if (num3 >= 0)
						{
							StrokeCount[num3] = num2;
						}
					}
				}
				return true;
			}
			MessageOverSplashScreen("The EasiSlides system file strokecount.dat is missing. Please re-install EasiSlides Software.");
			return false;
		}

		public static bool LoadUnicodeStrokeCount()
		{
			string InString = "";
			if (LoadFileContents(Application.StartupPath + "\\Sys\\strokecount.dat", ref InString))
			{
				ReadOnlySpan<char> roString = InString.AsSpan();
				int i = 1;
				for (int num = roString.Length - 2; i <= num - 2; i += 3)
				{
					//int num2 = DataUtil.ObjToInt(InString.Substring(i, 2));
					int num2 = int.Parse(roString.Slice(i, 2));
					if (num2 > 0)
					{
						int num3 = roString[i - 1];
						if (num3 < 0)
						{
							num3 += 65536;
						}
						if (num3 >= 0)
						{
							StrokeCount[num3] = num2;
						}
					}
				}
				return true;
			}
			MessageOverSplashScreen("The EasiSlides system file strokecount.dat is missing. Please re-install EasiSlides Software.");
			return false;
		}

		public static bool LoadFileContents(string InFileName, ref string InString)
		{
			InString = "";
			if (File.Exists(InFileName))
			{
				using StreamReader streamReader = new StreamReader(InFileName, detectEncodingFromByteOrderMarks: true);
				InString = streamReader.ReadToEnd();
				//streamReader.Close();
				return true;
			}
			return false;
		}

		public static bool ValidateRootFolder()
		{
			if (DataUtil.Right(RootEasiSlidesDir, 1) != "\\")
			{
				RootEasiSlidesDir += "\\";
			}
			if (Directory.Exists(RootEasiSlidesDir))
			{
				return true;
			}
			try
			{
				if (ApplicationFirstRun)
				{
					if (FileUtil.MakeDir(RootEasiSlidesDir))
					{
						RestoreSongsDatabase = true;
						RestoreBackgroundImages();
						return true;
					}
					MessageOverSplashScreen("Error encountered whilst creating the EasiSlides Working folder: " + RootEasiSlidesDir + ". Make sure have write access to the area and try again");
					return false;
				}
			}
			catch
			{
			}
			SplashScreenBack = true;
			FrmGetWorkingFolder frmGetWorkingFolder = new FrmGetWorkingFolder();
			if (frmGetWorkingFolder.ShowDialog() == DialogResult.OK)
			{
				RestoreBackgroundImages();
				SplashScreenBack = false;
				return true;
			}
			SplashScreenBack = false;
			return false;
		}

		public static bool ValidateVer_3_4_Fields()
		{
			if (!ValidateDB(DatabaseType.Songs))
			{
				return false;
			}
			bool flag = false;
			bool flag2 = false;
			bool flag3 = false;
			bool flag4 = false;
			bool flag5 = false;
			bool flag6 = false;
			bool flag7 = false;
			bool flag8 = false;
			bool flag9 = false;
			bool flag10 = false;
			bool flag11 = false;
			bool flag12 = false;
			bool flag13 = false;
			bool flag14 = false;
			bool flag15 = false;
			bool flag16 = false;
			bool flag17 = false;
			bool flag18 = false;
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			bool flag19 = false;
			bool flag20 = false;
			bool flag21 = false;
			bool flag22 = false;
#if OleDb
			OleDbConnection connection = new OleDbConnection(ConnectStringMainDB);
			connection.Open();
#elif SQLite
			DbConnection connection = DbController.GetDbConnection(ConnectStringMainDB);
#endif
			try
			{
#if OleDb
				DataTable dbSchemaTable = connection.GetOleDbSchemaTable(OleDbSchemaGuid.Columns, new object[4]
				{
					null,
					null,
					"Folder",
					null
				});
#elif SQLite
				using DataTable dbSchemaTable = connection.GetSchema("Columns", new string[4]
				{
					null,
					null,
					"Folder",
					null
				});
#endif
				foreach (DataRow row in dbSchemaTable.Rows)
				{
					string a = DataUtil.ObjToString(row["COLUMN_NAME"]).ToUpper();
					if (a != "")
					{
						if (a == "BIU0".ToUpper())
						{
							flag = true;
						}
						if (a == "BIU1".ToUpper())
						{
							flag2 = true;
						}
						if (a == "ColA".ToUpper())
						{
							flag3 = true;
						}
						if (a == "ColB".ToUpper())
						{
							flag4 = true;
						}
						if (a == "LMargin".ToUpper())
						{
							flag6 = true;
						}
						if (a == "RMargin".ToUpper())
						{
							flag7 = true;
						}
						if (a == "BMargin".ToUpper())
						{
							flag8 = true;
						}
						if (a == "BIUHeading".ToUpper())
						{
							flag9 = true;
						}
						if (a == "HeadingSize".ToUpper())
						{
							flag10 = true;
						}
						if (a == "HeadingOption".ToUpper())
						{
							flag11 = true;
						}
						if (a == "LineSpacing".ToUpper())
						{
							flag12 = true;
						}
						if (a == "LineSpacing2".ToUpper())
						{
							flag13 = true;
						}
						if (a == "PreChorusHeading".ToUpper())
						{
							flag5 = true;
						}
					}
				}

#if DAO
				if (!flag)
				{
					DbDaoController.CreateField(ref connection, "Folder", "BIU0", 1);
				}
				if (!flag2)
				{
					DbDaoController.CreateField(ref connection, "Folder", "BIU1", 1);
				}
				if (!flag3)
				{
					DbDaoController.CreateField(ref connection, "Folder", "ColA", 0);
				}
				if (!flag4)
				{
					DbDaoController.CreateField(ref connection, "Folder", "ColB", 0);
				}
				if (!flag6)
				{
					DbDaoController.CreateField(ref connection, "Folder", "LMargin", 1);
				}
				if (!flag7)
				{
					DbDaoController.CreateField(ref connection, "Folder", "RMargin", 1);
				}
				if (!flag8)
				{
					DbDaoController.CreateField(ref connection, "Folder", "BMargin", 1);
				}
				if (!flag9)
				{
					DbDaoController.CreateField(ref connection, "Folder", "BIUHeading", 1);
				}
				if (!flag10)
				{
					DbDaoController.CreateField(ref connection, "Folder", "HeadingSize", 1);
				}
				if (!flag11)
				{
					DbDaoController.CreateField(ref connection, "Folder", "HeadingOption", 1);
				}
				if (!flag12)
				{
					DbDaoController.CreateField(ref connection, "Folder", "LineSpacing", 4);
				}
				if (!flag13)
				{
					DbDaoController.CreateField(ref connection, "Folder", "LineSpacing2", 4);
				}
				if (!flag5)
				{
					DbDaoController.CreateField(ref connection, "Folder", "PreChorusHeading", 0, 30);
				}
#elif SQLite
				if (!flag)
				{
					DbController.CreateField(ref connection, "Folder", "BIU0", 1);
				}
				if (!flag2)
				{
					DbController.CreateField(ref connection, "Folder", "BIU1", 1);
				}
				if (!flag3)
				{
					DbController.CreateField(ref connection, "Folder", "ColA", 0);
				}
				if (!flag4)
				{
					DbController.CreateField(ref connection, "Folder", "ColB", 0);
				}
				if (!flag5)
				{
					DbController.CreateField(ref connection, "Folder", "PreChorusHeading", 0, 30);
				}				
				if (!flag6)
				{
					DbController.CreateField(ref connection, "Folder", "LMargin", 1);
				}
				if (!flag7)
				{
					DbController.CreateField(ref connection, "Folder", "RMargin", 1);
				}
				if (!flag8)
				{
					DbController.CreateField(ref connection, "Folder", "BMargin", 1);
				}
				if (!flag9)
				{
					DbController.CreateField(ref connection, "Folder", "BIUHeading", 1);
				}
				if (!flag10)
				{
					DbController.CreateField(ref connection, "Folder", "HeadingSize", 1);
				}
				if (!flag11)
				{
					DbController.CreateField(ref connection, "Folder", "HeadingOption", 1);
				}
				if (!flag12)
				{
					DbController.CreateField(ref connection, "Folder", "LineSpacing", 4);
				}
				if (!flag13)
				{
					DbController.CreateField(ref connection, "Folder", "LineSpacing2", 4);
				}


#endif
			}
			catch
			{
			}
			finally
			{
				if (connection.State == ConnectionState.Open)
				{
					connection.Close();
				}
			}

			try
			{
				connection.Open();
#if OleDb
				DataTable dbSchemaTable = connection.GetOleDbSchemaTable(OleDbSchemaGuid.Columns, new object[4]
				{
					null,
					null,
					"Song",
					null
				});
#elif SQLite
				using DataTable dbSchemaTable = connection.GetSchema("Columns", new string[4]
				{
					null,
					null,
					"Song",
					null
				});
#endif

				foreach (DataRow row2 in dbSchemaTable.Rows)
				{
					string a = DataUtil.ObjToString(row2["COLUMN_NAME"]).ToUpper();
					if (a != "")
					{
						if (a == "CAPO".ToUpper())
						{
							flag14 = true;
						}
						if (a == "TIMING".ToUpper())
						{
							flag15 = true;
						}
						if (a == "SONG_NUMBER".ToUpper())
						{
							flag16 = true;
						}
						if (a == "BOOK_REFERENCE".ToUpper())
						{
							flag17 = true;
							num2 = DataUtil.ObjToInt(row2["CHARACTER_MAXIMUM_LENGTH"]);
						}
						if (a == "USER_REFERENCE".ToUpper())
						{
							flag18 = true;
							num = DataUtil.ObjToInt(row2["CHARACTER_MAXIMUM_LENGTH"]);
						}
						if (a == "LICENCE_ADMIN1".ToUpper())
						{
							flag19 = true;
						}
						if (a == "LICENCE_ADMIN2".ToUpper())
						{
							flag20 = true;
						}
						if (a == "SETTINGS".ToUpper())
						{
							flag21 = true;
						}
						if (a == "SEQUENCE".ToUpper())
						{
							num3 = DataUtil.ObjToInt(row2["CHARACTER_MAXIMUM_LENGTH"]);
						}
						if (a == "FORMATDATA".ToUpper())
						{
							flag22 = true;
						}
					}
				}
#if OleDb
				if (num2 > 1 && num2 < 100)
				{
					OleDbCommand command = new OleDbCommand("ALTER TABLE Song ALTER COLUMN BOOK_REFERENCE TEXT (100) ", connection);
					command.ExecuteNonQuery();
				}
				if (num3 > 1 && num3 < 255)
				{
					OleDbCommand command = new OleDbCommand("ALTER TABLE Song ALTER COLUMN SEQUENCE TEXT (255) ", connection);
					command.ExecuteNonQuery();
				}
				if (!flag14)
				{
					DbDaoController.CreateField(ref connection, "Song", "CAPO", 1);
				}
				if (!flag15)
				{
					DbDaoController.CreateField(ref connection, "Song", "TIMING", 0);
				}
				if (!flag16)
				{
					DbDaoController.CreateField(ref connection, "Song", "SONG_NUMBER", 1);
				}
				if (!flag17)
				{
					DbDaoController.CreateField(ref connection, "Song", "BOOK_REFERENCE", 0);
				}
				if (!flag18)
				{
					DbDaoController.CreateField(ref connection, "Song", "USER_REFERENCE", 5);
				}
				else if (num > 0)
				{
					OleDbCommand command = new OleDbCommand("ALTER TABLE Song ALTER COLUMN USER_REFERENCE MEMO ", connection);
					command.ExecuteNonQuery();
				}
				if (!flag19)
				{
					DbDaoController.CreateField(ref connection, "Song", "LICENCE_ADMIN1", 0);
				}
				if (!flag20)
				{
					DbDaoController.CreateField(ref connection, "Song", "LICENCE_ADMIN2", 0);
				}
				if (!flag21)
				{
					DbDaoController.CreateField(ref connection, "Song", "SETTINGS", 5);
				}
				if (!flag22)
				{
					DbDaoController.CreateField(ref connection, "Song", "FORMATDATA", 5);
				}
#elif SQLite
				if (num2 > 1 && num2 < 100)
				{
					try
					{
#if MariaDB
						DbCommand command = new DbCommand("ALTER TABLE Song MODIFY BOOK_REFERENCE varchar(100)", connection);

						command.ExecuteNonQuery();
					}
					catch { }
				}
				if (num3 > 1 && num3 < 255)
				{
					try
					{
						DbCommand command = new DbCommand("ALTER TABLE Song MODIFY SEQUENCE varchar(255)", connection);
						command.ExecuteNonQuery();
					}
					catch { }
				}
				if (!flag14)
				{
					DbController.CreateField(ref connection, "Song", "CAPO", 1);
				}
				if (!flag15)
				{
					DbController.CreateField(ref connection, "Song", "TIMING", 0);
				}
				if (!flag16)
				{
					DbController.CreateField(ref connection, "Song", "SONG_NUMBER", 1);
				}
				if (!flag17)
				{
					DbController.CreateField(ref connection, "Song", "BOOK_REFERENCE", 0);
				}
				if (!flag18)
				{
					DbController.CreateField(ref connection, "Song", "USER_REFERENCE", 5);
				}
				else if (num > 0)
				{
					try
					{
						DbCommand command = new DbCommand("ALTER TABLE Song MODIFY USER_REFERENCE varchar(255)", connection);
						command.ExecuteNonQuery();
					}
					catch { }
				}

				if (!flag19)
				{
					DbController.CreateField(ref connection, "Song", "LICENCE_ADMIN1", 0);
				}
				if (!flag20)
				{
					DbController.CreateField(ref connection, "Song", "LICENCE_ADMIN2", 0);
				}
				if (!flag21)
				{
					DbController.CreateField(ref connection, "Song", "SETTINGS", 5);
				}
				if (!flag22)
				{
					DbController.CreateField(ref connection, "Song", "FORMATDATA", 5);
				}
#else

						DbCommand command = new DbCommand("ALTER TABLE Song ALTER COLUMN BOOK_REFERENCE TEXT (100) ", connection);

						command.ExecuteNonQuery();
					}
					catch { }
				}
                if (num3 > 1 && num3 < 255)
                {
					try
					{
						DbCommand command = new DbCommand("ALTER TABLE Song ALTER COLUMN SEQUENCE TEXT (255) ", connection);
						command.ExecuteNonQuery();
					}
					catch { }
                }
                if (!flag14)
				{
					DbController.CreateField(ref connection, "Song", "CAPO", 1);
				}
				if (!flag15)
				{
					DbController.CreateField(ref connection, "Song", "TIMING", 0);
				}
				if (!flag16)
				{
					DbController.CreateField(ref connection, "Song", "SONG_NUMBER", 1);
				}
				if (!flag17)
				{
					DbController.CreateField(ref connection, "Song", "BOOK_REFERENCE", 0);
				}				
				if (!flag18)
				{
					DbController.CreateField(ref connection, "Song", "USER_REFERENCE", 5);
				}
                else if (num > 0)
                {
                    try
                    {
						DbController.SqliteAlterColumn(ref connection, "Song", "USER_REFERENCE MEMO", "USER_REFERENCE TEXT");
                        //DbCommand command = new DbCommand("ALTER TABLE Song ALTER COLUMN USER_REFERENCE MEMO ", connection);
                        //command.ExecuteNonQuery();
                    }
                    catch { }
                }

                if (!flag19)
				{
					DbController.CreateField(ref connection, "Song", "LICENCE_ADMIN1", 0);
				}
				if (!flag20)
				{
					DbController.CreateField(ref connection, "Song", "LICENCE_ADMIN2", 0);
				}
				if (!flag21)
				{
					DbController.CreateField(ref connection, "Song", "SETTINGS", 5);
				}
				if (!flag22)
				{
					DbController.CreateField(ref connection, "Song", "FORMATDATA", 5);
				}
#endif
#endif



			}
					catch
			{
			}
			finally
			{
				if (connection.State == ConnectionState.Open)
				{
					connection.Close();
				}
			}

			try
			{
				connection.Open();
				bool flag23 = false;
				string text = "LICENCE";

#if OleDb
				DataTable dbSchemaTable = connection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, new object[4]
				{
					null,
					null,
					text,
					"TABLE"
				});
#elif SQLite
				using DataTable dbSchemaTable = connection.GetSchema("Tables", new string[4]
				{
					null,
					null,
					text,
					"TABLE"
				});
#endif


				foreach (DataRow row3 in dbSchemaTable.Rows)
				{
					if (DataUtil.ObjToString(row3["TABLE_NAME"]).ToUpper() == text.ToUpper())
					{
						flag23 = true;
					}
				}

#if OleDb
				if (!flag23)
				{
					OleDbCommand command = new OleDbCommand("CREATE TABLE " + text, connection);
					command.ExecuteNonQuery();
					DbDaoController.CreateField(ref connection, text, "ADMINISTRATOR", 0);
					DbDaoController.CreateField(ref connection, text, "REF", 1);
				}
				DbDaoController.DropTable(ref connection, "USAGE");
				DbDaoController.DropTable(ref connection, "BIBLEFOLDER");
#elif SQLite
				if (!flag23)
				{
					DbCommand command = new DbCommand("CREATE TABLE " + text, connection);
					command.ExecuteNonQuery();

					DbController.CreateField(ref connection, text, "ADMINISTRATOR", 0);
					DbController.CreateField(ref connection, text, "REF", 1);
				}
				DbController.DropTable(ref connection, "USAGE");
				DbController.DropTable(ref connection, "BIBLEFOLDER");
#endif
			}
			catch (Exception ex)
			{
				Trace.WriteLine($"ERROR : {ex.Message}, {ex.StackTrace}");
			}
			finally
			{
				if (connection.State == ConnectionState.Open)
				{
					connection.Close();
				}
			}
			if (!ValidateDB(DatabaseType.Usages))
			{
				return false;
			}

#if OleDb
			connection = new OleDbConnection(ConnectStringUsageDB);
			connection.Open();
#elif SQLite
			connection = DbController.GetDbConnection(ConnectStringUsageDB);
#endif


			try
			{				
				bool flag23 = false;
				string text = "USAGE";
#if OleDb
				DataTable dbSchemaTable = connection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, new object[4]
				{
					null,
					null,
					text,
					"TABLE"
				});
#elif SQLite
				using DataTable dbSchemaTable = connection.GetSchema("Tables", new string[4]
				{
					null,
					null,
					text,
					"TABLE"
				});
#endif

				foreach (DataRow row4 in dbSchemaTable.Rows)
				{
					if (DataUtil.ObjToString(row4["TABLE_NAME"]).ToUpper() == text.ToUpper())
					{
						flag23 = true;
					}
				}

#if DAO
				if (!flag23)
				{
					Workspace workspace = DbDaoController.DAODBEngine_definst.Workspaces[0];
					Database database = workspace.OpenDatabase(connection.DataSource, false, false);
					database.Execute("CREATE TABLE USAGE(REC_ID  autoincrement)");
					DbDaoController.CreateField(ref connection, "USAGE", "WORSHIP_DATE", 2);
					DbDaoController.CreateField(ref connection, "USAGE", "WORSHIP_LIST", 0);
					DbDaoController.CreateField(ref connection, "USAGE", "SONG_TITLE", 0, 100);
					DbDaoController.CreateField(ref connection, "USAGE", "SONG_NUMBER", 1);
					DbDaoController.CreateField(ref connection, "USAGE", "SONG_ID", 0);
					DbDaoController.CreateField(ref connection, "USAGE", "ADMIN_1", 0);
					DbDaoController.CreateField(ref connection, "USAGE", "ADMIN_2", 0);
				}
				DbDaoController.DropTable(ref connection, "SONG");
				DbDaoController.DropTable(ref connection, "LICENCE");
				DbDaoController.DropTable(ref connection, "FOLDER");
				DbDaoController.DropTable(ref connection, "BIBLEFOLDER");
#elif SQLite
				if (!flag23)
				{
					//DbConnection connection = DbController.GetDbConnection(connection.DataSource);
					DbCommand command = new DbCommand("CREATE TABLE USAGE(REC_ID  autoincrement)", connection);
					command.ExecuteNonQuery();

					DbController.CreateField(ref connection, "USAGE", "WORSHIP_DATE", 2);
					DbController.CreateField(ref connection, "USAGE", "WORSHIP_LIST", 0);
					DbController.CreateField(ref connection, "USAGE", "SONG_TITLE", 0, 100);
					DbController.CreateField(ref connection, "USAGE", "SONG_NUMBER", 1);
					DbController.CreateField(ref connection, "USAGE", "SONG_ID", 0);
					DbController.CreateField(ref connection, "USAGE", "ADMIN_1", 0);
					DbController.CreateField(ref connection, "USAGE", "ADMIN_2", 0);
				}
				DbController.DropTable(ref connection, "SONG");
				DbController.DropTable(ref connection, "LICENCE");
				DbController.DropTable(ref connection, "FOLDER");
				DbController.DropTable(ref connection, "BIBLEFOLDER");
#endif


			}
			catch
			{
			}
			finally
			{
				if (connection.State == ConnectionState.Open)
				{
					connection.Close();
				}
			}
			if (!ValidateDB(DatabaseType.Bible))
			{
				return false;
			}
#if OleDb
			connection = new OleDbConnection(ConnectStringBibleDB);
			connection.Open();

#elif SQLite
			connection = DbController.GetDbConnection(ConnectStringBibleDB);
#endif
			try
			{
				bool flag23 = false;
				string text = "BIBLEFOLDER";
#if OleDb
				DataTable dbSchemaTable = connection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, new object[4]
				{
					null,
					null,
					text,
					"TABLE"
				});
#elif SQLite
				using DataTable dbSchemaTable = connection.GetSchema("Tables", new string[4]
				{
					null,
					null,
					text,
					"TABLE"
				});
#endif

				foreach (DataRow row5 in dbSchemaTable.Rows)
				{
					if (DataUtil.ObjToString(row5["TABLE_NAME"]).ToUpper() == text.ToUpper())
					{
						flag23 = true;
					}
				}

#if OleDb
				if (!flag23)
				{
					OleDbCommand command = new OleDbCommand("CREATE TABLE " + text, connection);
					command.ExecuteNonQuery();
					DbDaoController.CreateField(ref connection, "BIBLEFOLDER", "NAME", 0, 100);
					DbDaoController.CreateField(ref connection, "BIBLEFOLDER", "DESCRIPTION", 0, 100);
					DbDaoController.CreateField(ref connection, "BIBLEFOLDER", "FILENAME", 0, 100);
					DbDaoController.CreateField(ref connection, "BIBLEFOLDER", "COPYRIGHT", 0, 100);
					DbDaoController.CreateField(ref connection, "BIBLEFOLDER", "SONGFOLDER", 1);
					DbDaoController.CreateField(ref connection, "BIBLEFOLDER", "[SIZE]", 1);
					DbDaoController.CreateField(ref connection, "BIBLEFOLDER", "DISPLAYORDER", 1);
				}
				DbDaoController.DropTable(ref connection, "SONG");
				DbDaoController.DropTable(ref connection, "LICENCE");
				DbDaoController.DropTable(ref connection, "FOLDER");
				DbDaoController.DropTable(ref connection, "USAGE");
#elif SQLite
				if (!flag23)
				{
					DbCommand command = new DbCommand("CREATE TABLE " + text, connection);
					command.ExecuteNonQuery();

					DbController.CreateField(ref connection, "BIBLEFOLDER", "NAME", 0, 100);
					DbController.CreateField(ref connection, "BIBLEFOLDER", "DESCRIPTION", 0, 100);
					DbController.CreateField(ref connection, "BIBLEFOLDER", "FILENAME", 0, 100);
					DbController.CreateField(ref connection, "BIBLEFOLDER", "COPYRIGHT", 0, 100);
					DbController.CreateField(ref connection, "BIBLEFOLDER", "SONGFOLDER", 1);
					DbController.CreateField(ref connection, "BIBLEFOLDER", "[SIZE]", 1);
					DbController.CreateField(ref connection, "BIBLEFOLDER", "DISPLAYORDER", 1);
				}
				DbController.DropTable(ref connection, "SONG");
				DbController.DropTable(ref connection, "LICENCE");
				DbController.DropTable(ref connection, "FOLDER");
				DbController.DropTable(ref connection, "USAGE");
#endif

			}
			catch
			{
			}
			finally
			{
				if (connection.State == ConnectionState.Open)
				{
					connection.Close();
				}
			}
			return true;
		}

		public static string RestoreOriginalSongsDatabase()
		{
#if SQLite
			string text = $@"{Application.StartupPath}Sys\EasiSlidesDb.db";
#else
			string text = $@"{Application.StartupPath}Sys\EasiSlidesDb.mdb";
#endif
			RestoreSongsDatabase = false;
			if (File.Exists(text))
			{
				try
				{
					FileUtil.MakeDir(RootEasiSlidesDir + "Admin\\Database\\");
					int num = 1;
					string dBFileName = DBFileName;
					if (File.Exists(dBFileName))
					{
#if SQLite
						dBFileName = $@"{RootEasiSlidesDir}Admin\Database\{Path.GetFileNameWithoutExtension("EasiSlidesDb.mdb")}{DateTime.Now.ToString("-yyyy-MM-dd-")}{num}{Path.GetExtension("EasiSlidesDb.db")}";
						while (File.Exists(dBFileName))
						{
							num++;
							dBFileName = $@"{RootEasiSlidesDir}Admin\Database\{Path.GetFileNameWithoutExtension("EasiSlidesDb.mdb")}{DateTime.Now.ToString("-yyyy-MM-dd-")}{num}{Path.GetExtension("EasiSlidesDb.db")}";
						}
#else
						dBFileName = $@"{RootEasiSlidesDir}Admin\Database\{Path.GetFileNameWithoutExtension("EasiSlidesDb.mdb")}{DateTime.Now.ToString("-yyyy-MM-dd-")}{num}{Path.GetExtension("EasiSlidesDb.mdb")}";
						while (File.Exists(dBFileName))
						{
							num++;
							dBFileName = $@"{RootEasiSlidesDir}Admin\Database\{Path.GetFileNameWithoutExtension("EasiSlidesDb.mdb")}{DateTime.Now.ToString("-yyyy-MM-dd-")}{num}{Path.GetExtension("EasiSlidesDb.mdb")}";
						}
#endif
					}
					else
					{
						dBFileName = "";
					}
					try
					{
						if (dBFileName != "")
						{
							File.Move(DBFileName, dBFileName);
						}
						File.Copy(text, DBFileName);
						return dBFileName;
					}
					catch
					{
					}
					MessageBox.Show("Sorry, cannot install the new Lyrics Database. Please make sure the existing database is not in use and then try again.");
					return "-1";
				}
				catch
				{
					MessageBox.Show("Sorry, cannot install the new Lyrics Database. Please make sure the existing database is not in use and then try again.");
					return "-1";
				}
			}
			MessageBox.Show("Sorry, cannot install the new Lyrics Database. Please re-install EasiSlides Software.");
			return "-1";
		}

		public static void RestoreBackgroundImages()
		{
			string text = $@"{Application.StartupPath}\Backgrounds\Scenery\";
			string text2 = $@"{Application.StartupPath}\Backgrounds\Tiles\";
			string text3 = $@"{RootEasiSlidesDir}Images\Scenery\";
			string text4 = $@"{RootEasiSlidesDir}Images\Tiles\";
			if (FileUtil.MakeDir($@"{RootEasiSlidesDir}Images\"))
			{
				if (Directory.Exists(text) && FileUtil.MakeDir(text3))
				{
					FileUtil.CopyFiles(text, text3);
				}
				if (Directory.Exists(text2) && FileUtil.MakeDir(text4))
				{
					FileUtil.CopyFiles(text2, text4);
				}
			}
		}

		public static bool ValidateDB(DatabaseType InType)
		{
			string destFileName = "";
			string dbFileName = "";
			switch (InType)
			{
				case DatabaseType.Usages:
					destFileName = UsageFileName;
					dbFileName = "EsUsage";
					break;
				case DatabaseType.Bible:
					destFileName = BiblesListFileName;
					dbFileName = "EsBiblesList";
					break;
				default:
					destFileName = DBFileName;
					dbFileName = "EasiSlidesDb";
					break;
			}
			if (File.Exists(destFileName))
			{
				return true;
			}
#if SQLite
			string soucreFileName = $@"{Application.StartupPath}\Sys\{dbFileName}.db";
#else
			string soucreFileName = $@"{Application.StartupPath}\Sys\Defdb.dat";
#endif

			if (File.Exists(soucreFileName))
			{
				Directory.CreateDirectory($@"{RootEasiSlidesDir}Admin\Database\");
				File.Copy(soucreFileName, destFileName, overwrite: true);
				return true;
			}
			MessageOverSplashScreen(@$"Sorry, cannot create new {dbFileName} database. Please re-install EasiSlides Software.");
			return false;
		}

		public static bool DeleteAllFolders(string InConnectString)
		{
			if (InConnectString == "" || InConnectString == ConnectStringMainDB)
			{
				return false;
			}
			try
			{
#if OleDb
				using (OleDbConnection daoDb = DbConnectionController.GetOleDbConnection(InConnectString))
				{
					OleDbCommand command = new OleDbCommand("Delete * from Folder ", daoDb);
					command.ExecuteNonQuery();
				}
#elif SQLite
				using(DbConnection connection = DbController.GetDbConnection(InConnectString))
				{
					DbCommand command = new DbCommand("Delete * from Folder ", connection);
					command.ExecuteNonQuery();
				}
#endif
				return true;
			}
			catch
			{
			}
			return false;
		}

		public static void ResetFolder(int FNumber, string InFolderName, string InConnectString)
		{

#if OleDb
			using OleDbConnection connection = DbConnectionController.GetOleDbConnection(InConnectString);
#elif SQLite
			using DbConnection connection = DbController.GetDbConnection(InConnectString);
#endif
			try
			{
				bool flag = false;
				string cmdText = $@"select count(*) from FOLDER where FolderNo={DataUtil.ObjToString(FNumber)}";
#if OleDb
				OleDbCommand command = new OleDbCommand(cmdText, connection);
#elif SQLite
				DbCommand command = new DbCommand(cmdText, connection);				
#endif

				if (DataUtil.ObjToInt(command.ExecuteScalar()) == 0)
				{
					string value;
					if (InFolderName == "")
					{
						if (FNumber > 0)
						{
							value = $@"Folder {DataUtil.ObjToString(FNumber)}";
							flag = ((FNumber < 4) ? true : false);
						}
						else
						{
							value = "Recycle Folder";
							flag = false;
						}
					}
					else
					{
						value = InFolderName;
						flag = true;
					}
					cmdText = (command.CommandText = "Insert into Folder (FolderNo,name,Use,GroupStyle,PreChorusHeading,ChorusHeading, BridgeHeading,EndingHeading,BIUHeading,HeadingSize,HeadingOption,BIU0,Size0,Bold0,Align0,FontName0,Vpos0,BIU1,Size1,Bold1,Align1,FontName1,Vpos1)  Values (?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?)");
					command.Parameters.AddWithValue("@FolderNo", FNumber);
					command.Parameters.AddWithValue("@Name", value);
					command.Parameters.AddWithValue("@Use", flag);
					command.Parameters.AddWithValue("@GroupStyle", SortBy.Alpha);
					command.Parameters.AddWithValue("@PreChorusHeading", "");
					command.Parameters.AddWithValue("@ChorusHeading", "Chorus:");
					command.Parameters.AddWithValue("@BridgeHeading", "");
					command.Parameters.AddWithValue("@EndingHeading", "");
					command.Parameters.AddWithValue("@BIUHeading", 4);
					command.Parameters.AddWithValue("@HeadingSize", 100);
					command.Parameters.AddWithValue("@HeadingOption", HeadingFormat.AsRegion1Plus);
					command.Parameters.AddWithValue("@BIU0", 0);
					command.Parameters.AddWithValue("@Size0", 40);
					command.Parameters.AddWithValue("@Bold0", false);
					command.Parameters.AddWithValue("@Align0", 2);
					command.Parameters.AddWithValue("@FontName0", "Microsoft Sans Serif");
					command.Parameters.AddWithValue("@Vpos0", 0);
					command.Parameters.AddWithValue("@BIU1", 0);
					command.Parameters.AddWithValue("@Size1", 40);
					command.Parameters.AddWithValue("@Bold1", false);
					command.Parameters.AddWithValue("@Align1", 2);
					command.Parameters.AddWithValue("@FontName1", "Microsoft Sans Serif");
					command.Parameters.AddWithValue("@Vpos1", 50);
					command.ExecuteNonQuery();
				}
			}
			catch
			{
			}
			finally
			{
				if (connection.State == ConnectionState.Open)
				{
					connection.Close();
				}
			}
		}

		public static async Task ResetFolder1(int FNumber, string InFolderName, string InConnectString)
		{
			var task = Task.Run(() =>
			{
#if OleDb
				using OleDbConnection connection = new OleDbConnection(InConnectString);;
#elif SQLite
				using DbConnection connection = DbController.GetDbConnection(InConnectString);
#endif
				
				try
				{
					bool flag = false;
					string cmdText = $@"select count(*) from FOLDER where FolderNo={DataUtil.ObjToString(FNumber)}";
					connection.Open();
#if OleDb
					OleDbCommand command = new OleDbCommand(cmdText, connection);
#elif SQLite
					DbCommand command = new DbCommand(cmdText, connection);
#endif
					if ((int)command.ExecuteScalar() == 0)
					{
						string value;
						if (InFolderName == "")
						{
							if (FNumber > 0)
							{
								value = $@"Folder {DataUtil.ObjToString(FNumber)}";
								flag = ((FNumber < 4) ? true : false);
							}
							else
							{
								value = "Recycle Folder";
								flag = false;
							}
						}
						else
						{
							value = InFolderName;
							flag = true;
						}
						cmdText = (command.CommandText = "Insert into Folder (FolderNo,name,Use,GroupStyle,PreChorusHeading,ChorusHeading, BridgeHeading,EndingHeading,BIUHeading,HeadingSize,HeadingOption,BIU0,Size0,Bold0,Align0,FontName0,Vpos0,BIU1,Size1,Bold1,Align1,FontName1,Vpos1)  Values (?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?)");
						command.Parameters.AddWithValue("@FolderNo", FNumber);
						command.Parameters.AddWithValue("@Name", value);
						command.Parameters.AddWithValue("@Use", flag);
						command.Parameters.AddWithValue("@GroupStyle", SortBy.Alpha);
						command.Parameters.AddWithValue("@PreChorusHeading", "");
						command.Parameters.AddWithValue("@ChorusHeading", "Chorus:");
						command.Parameters.AddWithValue("@BridgeHeading", "");
						command.Parameters.AddWithValue("@EndingHeading", "");
						command.Parameters.AddWithValue("@BIUHeading", 4);
						command.Parameters.AddWithValue("@HeadingSize", 100);
						command.Parameters.AddWithValue("@HeadingOption", HeadingFormat.AsRegion1Plus);
						command.Parameters.AddWithValue("@BIU0", 0);
						command.Parameters.AddWithValue("@Size0", 40);
						command.Parameters.AddWithValue("@Bold0", false);
						command.Parameters.AddWithValue("@Align0", 2);
						command.Parameters.AddWithValue("@FontName0", "Microsoft Sans Serif");
						command.Parameters.AddWithValue("@Vpos0", 0);
						command.Parameters.AddWithValue("@BIU1", 0);
						command.Parameters.AddWithValue("@Size1", 40);
						command.Parameters.AddWithValue("@Bold1", false);
						command.Parameters.AddWithValue("@Align1", 2);
						command.Parameters.AddWithValue("@FontName1", "Microsoft Sans Serif");
						command.Parameters.AddWithValue("@Vpos1", 50);
						command.ExecuteNonQuery();
					}
				}
				catch
				{
				}
				finally
				{
					if (connection.State == ConnectionState.Open)
					{
						connection.Close();
					}
				}
			});
		}

		public static void LoadSavedData()
		{
			for (int i = 0; i < 41; i++)
			{
				FindSongsFolder[i] = true;
			}
			RootEasiSlidesDir = RegUtil.GetRegValue("config", "root_directory", "C:\\EasiSlides\\");
			WorshipDir = $@"{RootEasiSlidesDir}Admin\WorshipLists\";
			if (!Directory.Exists(WorshipDir))
			{
				FileUtil.MakeDir(WorshipDir);
			}
			InfoScreenDir = $@"{RootEasiSlidesDir}InfoScreens\";
			if (!Directory.Exists(InfoScreenDir))
			{
				FileUtil.MakeDir(InfoScreenDir);
			}
			PowerpointDir = $@"{RootEasiSlidesDir}Powerpoint\";
			if (!Directory.Exists(PowerpointDir))
			{
				FileUtil.MakeDir(PowerpointDir);
			}
			BibleDir = $@"{RootEasiSlidesDir}HolyBibles\";
			if (!Directory.Exists(BibleDir))
			{
				FileUtil.MakeDir(BibleDir);
			}
			PraiseBookDir = $@"{RootEasiSlidesDir}Admin\PraiseBooks\";
			if (!Directory.Exists(PraiseBookDir))
			{
				FileUtil.MakeDir(PraiseBookDir);
			}
			WorshipTemplatesDir = $@"{RootEasiSlidesDir}Admin\Templates\WorshipListsTemplates\";
			if (!Directory.Exists(WorshipTemplatesDir))
			{
				FileUtil.MakeDir(WorshipTemplatesDir);
			}
			SettingsTemplatesDir = $@"{RootEasiSlidesDir}Admin\Templates\SettingsTemplates\";
			if (!Directory.Exists(SettingsTemplatesDir))
			{
				FileUtil.MakeDir(SettingsTemplatesDir);
			}
			EasiSlidesTempDir = $@"{Path.GetTempPath()}EasiSlides Files\";
			if (!Directory.Exists(EasiSlidesTempDir))
			{
				FileUtil.MakeDir(EasiSlidesTempDir);
			}
			MediaDir = $@"{RootEasiSlidesDir}Media\";
			if (!Directory.Exists(MediaDir))
			{
				FileUtil.MakeDir(MediaDir);
			}
			DocumentsDir = $@"{RootEasiSlidesDir}Documents\";
			if (!Directory.Exists(DocumentsDir))
			{
				FileUtil.MakeDir(DocumentsDir);
			}
			ImagesDir = $@"{RootEasiSlidesDir}Images\";
			if (!Directory.Exists(ImagesDir))
			{
				RestoreBackgroundImages();
			}
			if (!Directory.Exists($@"{ImagesDir}Tiles\") || !Directory.Exists($@"{ImagesDir}Scenery\"))
			{
				RestoreBackgroundImages();
			}
			CurSession = RegUtil.GetRegValue("config", "current_session", "");
			CurPraiseBook = RegUtil.GetRegValue("config", "current_praisebook", "");
			string text = RegUtil.GetRegValue("config", "media_dir", MediaDir);
			if (DataUtil.Right(text, 1) != "\\")
			{
				text += "\\";
			}
			if (Directory.Exists(text))
			{
				MediaDir = text;
			}
			ExportToDir = RegUtil.GetRegValue("config", "export_dir", DocumentsDir);
			if (DataUtil.Right(ExportToDir, 1) != "\\")
			{
				ExportToDir += "\\";
			}
			ImportFromDir = RegUtil.GetRegValue("config", "import_dir", DocumentsDir);
			if (DataUtil.Right(ImportFromDir, 1) != "\\")
			{
				ImportFromDir += "\\";
			}
			PraiseOutputDir = RegUtil.GetRegValue("config", "praiseoutput_dir", DocumentsDir);
			if (DataUtil.Right(PraiseOutputDir, 1) != "\\")
			{
				PraiseOutputDir += "\\";
			}
			CurPraiseBook = RegUtil.GetRegValue("config", "current_praisebook", "");
			UseSongNumbers = ((DataUtil.ObjToInt(RegUtil.GetRegValue("options", "UseSongNumbers", 0)) > 0) ? true : false);
			PB_PrinterSpaces = ((DataUtil.ObjToInt(RegUtil.GetRegValue("options", "PrinterSpaces", 0)) > 0) ? 1 : 0);
			HB_MaxVersesSelection = DataUtil.ObjToInt(RegUtil.GetRegValue("options", "BibleMaxSelectVerses", 500));
			if (HB_MaxVersesSelection > 1000)
			{
				HB_MaxVersesSelection = 1000;
			}
			HB_MaxAdhocVersesSelection = DataUtil.ObjToInt(RegUtil.GetRegValue("options", "BibleMaxAdhocVersesSelection", 200));
			if (HB_MaxAdhocVersesSelection > 500)
			{
				HB_MaxAdhocVersesSelection = 200;
			}
			HB_ShowVerses = true;
			PP_MaxFiles = DataUtil.ObjToInt(RegUtil.GetRegValue("options", "PowerpointMaxFiles", 20));
			if (PP_MaxFiles > 100)
			{
				PP_MaxFiles = 100;
			}
			ShowRotateGap = DataUtil.ObjToInt(RegUtil.GetRegValue("options", "RotateGap", 0));
			if ((ShowRotateGap < 0) & (ShowRotateGap > 999))
			{
				ShowRotateGap = 0;
			}
			UsePowerpointTab = ((DataUtil.ObjToInt(RegUtil.GetRegValue("options", "UsePowerpointTab", 0)) > 0) ? true : false);
			NoPowerpointPanelOverlay = ((DataUtil.ObjToInt(RegUtil.GetRegValue("options", "NoPowerpointPanelOverlay", 0)) > 0) ? true : false);
			UseMediaTab = ((DataUtil.ObjToInt(RegUtil.GetRegValue("options", "UseMediaTab", 0)) > 0) ? true : false);
			ShowLyricsMonitorAlertBox = ((DataUtil.ObjToInt(RegUtil.GetRegValue("options", "ShowLyricsMonitorAlertBox", 0)) > 0) ? true : false);
			NoMediaPanelOverlay = ((DataUtil.ObjToInt(RegUtil.GetRegValue("options", "NoMediaPanelOverlay", 0)) > 0) ? true : false);
			AutoTextOverflow = ((DataUtil.ObjToInt(RegUtil.GetRegValue("options", "AutoTextOverflow", 1)) > 0) ? true : false);
			UseLargestFontSize = ((DataUtil.ObjToInt(RegUtil.GetRegValue("options", "UseLargestFontSize", 0)) > 0) ? true : false);
			AdvanceNextItem = ((DataUtil.ObjToInt(RegUtil.GetRegValue("options", "AdvanceNextItem", 0)) > 0) ? true : false);
			LineBetweenRegions = ((DataUtil.ObjToInt(RegUtil.GetRegValue("options", "LineBetweenRegions", 1)) > 0) ? true : false);
			GapItemOption = (GapType)DataUtil.ObjToInt(RegUtil.GetRegValue("options", "GapItemOption", 0));
			if ((GapItemOption < GapType.None) & (GapItemOption > GapType.User))
			{
				GapItemOption = GapType.None;
			}
			AltGapItemOption = GapType.None;
			GapItemLogoFile = RegUtil.GetRegValue("options", "GapItemLogoFile", "");
			GapItemUseFade = ((DataUtil.ObjToInt(RegUtil.GetRegValue("options", "GapItemUseFade", 1)) > 0) ? true : false);
			WordWrapLeftAlignIndent = ((DataUtil.ObjToInt(RegUtil.GetRegValue("options", "WordWrapLeftAlignIndent", 1)) > 0) ? true : false);
			WordWrapIgnoreStartSpaces = DataUtil.ObjToInt(RegUtil.GetRegValue("options", "WordWrapIgnoreStartSpaces", 8));
			if (WordWrapIgnoreStartSpaces < 1 || WordWrapIgnoreStartSpaces > 15)
			{
				WordWrapIgnoreStartSpaces = 9;
			}
			AutoRotateOn = ((DataUtil.ObjToInt(RegUtil.GetRegValue("options", "AutoRotateOn", 1)) > 0) ? true : false);
			AutoRotateStyle = DataUtil.ObjToInt(RegUtil.GetRegValue("options", "AutoRotateStyle", 3));
			if ((AutoRotateStyle < 1) & (AutoRotateStyle > 3))
			{
				AutoRotateStyle = 3;
			}
			int num = DataUtil.ObjToInt(RegUtil.GetRegValue("options", "NotationFontFactor", 75));
			if (num < 20 && num > 200)
			{
				num = 75;
			}
			NotationFontFactor = (float)num / 100f;
			PowerpointListingStyle = DataUtil.ObjToInt(RegUtil.GetRegValue("options", "ExternalListing", 0));
			if ((PowerpointListingStyle < 0) & (PowerpointListingStyle > 1))
			{
				PowerpointListingStyle = 0;
			}
			KeyBoardOption = DataUtil.ObjToInt(RegUtil.GetRegValue("options", "KeyBoardOption", 0));
			if ((KeyBoardOption < 0) & (KeyBoardOption > 1))
			{
				KeyBoardOption = 0;
			}

            //daniel
            //Global Keyboard Hook 가져오기
            GlobalHookKey_F7 = ((DataUtil.ObjToInt(RegUtil.GetRegValue("options", "GlobalHookKey_F7", 0)) > 0) ? true : false);
            GlobalHookKey_F8 = ((DataUtil.ObjToInt(RegUtil.GetRegValue("options", "GlobalHookKey_F8", 0)) > 0) ? true : false);

            //daniel
            //Global Keyboard Hook 가져오기
            GlobalHookKey_F9 = ((DataUtil.ObjToInt(RegUtil.GetRegValue("options", "GlobalHookKey_F9", 0)) > 0) ? true : false);
            GlobalHookKey_F10 = ((DataUtil.ObjToInt(RegUtil.GetRegValue("options", "GlobalHookKey_F10", 0)) > 0) ? true : false);

            GlobalHookKey_Arrow = ((DataUtil.ObjToInt(RegUtil.GetRegValue("options", "GlobalHookKey_Arrow", 0)) > 0) ? true : false);
            GlobalHookKey_CtrlArrow = ((DataUtil.ObjToInt(RegUtil.GetRegValue("options", "GlobalHookKey_CtrlArrow", 0)) > 0) ? true : false);

            EditMainFontName = RegUtil.GetRegValue("options", "EditMainFontName", "Microsoft Sans Serif");
			if (EditMainFontName == "")
			{
				EditMainFontName = "Microsoft Sans Serif";
			}
			EditMainFontSize = DataUtil.ObjToInt(RegUtil.GetRegValue("options", "EditMainFontSize", 12));
			if ((EditMainFontSize < 8) | (EditMainFontSize > 20))
			{
				EditMainFontSize = 12;
			}
			EditNotationFontSize = DataUtil.ObjToInt(RegUtil.GetRegValue("options", "EditNotationFontSize", 10));
			if ((EditNotationFontSize < 8) | (EditNotationFontSize > 20))
			{
				EditNotationFontSize = 10;
			}
			InfoMainFontName = RegUtil.GetRegValue("options", "InfoMainFontName", "Microsoft Sans Serif");
			if (InfoMainFontName == "")
			{
				InfoMainFontName = "Microsoft Sans Serif";
			}
			InfoMainFontSize = DataUtil.ObjToInt(RegUtil.GetRegValue("options", "InfoMainFontSize", 12));
			if ((InfoMainFontSize < 8) | (InfoMainFontSize > 20))
			{
				InfoMainFontSize = 12;
			}
			EditOpenDocumentDir = RegUtil.GetRegValue("options", "EditOpenDocumentDir", DocumentsDir);
			if (EditOpenDocumentDir == "")
			{
				EditOpenDocumentDir = DocumentsDir;
			}

			//daniel
			//OutputMonitorNumber = DataUtil.ObjToInt(RegUtil.GetRegValue("options", "OutputmonitorNumber", 1));
			//LyricsMonitorNumber = DataUtil.ObjToInt(RegUtil.GetRegValue("options", "LyricsMonitorNumber", 0));

			OutputMonitorName = RegUtil.GetRegValue("options", "OutputmonitorName", "None");
			LyricsMonitorName = RegUtil.GetRegValue("options", "LyricsMonitorName", "None");

			BibleText_FontSize = DataUtil.ObjToInt(RegUtil.GetRegValue("options", "BibleTextFontSize", 8));
			if ((BibleText_FontSize < 8) | (BibleText_FontSize > 20))
			{
				BibleText_FontSize = 8;
			}
			PreviewArea_FontSize = DataUtil.ObjToInt(RegUtil.GetRegValue("options", "PreviewAreaFontSize", 8));
			if ((PreviewArea_FontSize < 8) | (PreviewArea_FontSize > 20))
			{
				PreviewArea_FontSize = 8;
			}
			PreviewArea_ShowNotations = ((DataUtil.ObjToInt(RegUtil.GetRegValue("options", "PreviewAreaShowNotations", 0)) > 0) ? true : false);
			PreviewArea_LineBetweenScreens = ((DataUtil.ObjToInt(RegUtil.GetRegValue("options", "PreviewAreaLineBetweenScreens", 0)) > 0) ? true : false);
			ParentalAlertHeading = RegUtil.GetRegValue("options", "ParentalAlertHeading", "");
			ParentalAlertDuration = DataUtil.ObjToInt(RegUtil.GetRegValue("options", "ParentalAlertDuration", 20));
			if ((ParentalAlertDuration < 1) | (ParentalAlertDuration > 60))
			{
				ParentalAlertDuration = 30;
			}
			ParentalAlertTextColour = Color.FromArgb(DataUtil.ObjToInt(RegUtil.GetRegValue("options", "ParentalAlertTextColour", ParentalAlertTextColour.ToArgb())));
			ParentalAlertBackColour = Color.FromArgb(DataUtil.ObjToInt(RegUtil.GetRegValue("options", "ParentalAlertBackColour", ParentalAlertBackColour.ToArgb())));
			ParentalAlertTextAlign = DataUtil.ObjToInt(RegUtil.GetRegValue("options", "ParentalAlertTextAlign", 3));
			if ((ParentalAlertTextAlign < 1) | (ParentalAlertTextAlign > 3))
			{
				ParentalAlertTextAlign = 3;
			}
			ParentalAlertVerticalAlign = DataUtil.ObjToInt(RegUtil.GetRegValue("options", "ParentalAlertVerticalAlign", 2));
			if ((ParentalAlertVerticalAlign < 0) | (ParentalAlertVerticalAlign > 2))
			{
				ParentalAlertVerticalAlign = 2;
			}
			int inValue = DataUtil.ObjToInt(RegUtil.GetRegValue("options", "ParentalAlertStyle", 3));
			ParentalAlertScroll = DataUtil.GetBitBoolean(inValue, 1);
			ParentalAlertFlash = DataUtil.GetBitBoolean(inValue, 2);
			ParentalAlertTransparent = DataUtil.GetBitBoolean(inValue, 3);
			ParentalAlertFontName = RegUtil.GetRegValue("options", "ParentalAlertFontName", "Microsoft Sans Serif");
			if (ParentalAlertFontName == "")
			{
				ParentalAlertFontName = "Microsoft Sans Serif";
			}
			ParentalAlertFontSize = DataUtil.ObjToInt(RegUtil.GetRegValue("options", "ParentalAlertFontSize", 22));
			if ((ParentalAlertFontSize < 20) | (ParentalAlertFontSize > 50))
			{
				ParentalAlertFontSize = 25;
			}
			ParentalAlertFontFormat = DataUtil.ObjToInt(RegUtil.GetRegValue("options", "ParentalAlertFontFormat", 0));
			if ((ParentalAlertFontFormat < 0) | (ParentalAlertFontFormat > 100))
			{
				ParentalAlertFontFormat = 0;
			}
			ParentalAlertBold = DataUtil.GetBitBoolean(ParentalAlertFontFormat, 1);
			ParentalAlertItalic = DataUtil.GetBitBoolean(ParentalAlertFontFormat, 2);
			ParentalAlertUnderline = DataUtil.GetBitBoolean(ParentalAlertFontFormat, 3);
			ParentalAlertShadow = DataUtil.GetBitBoolean(ParentalAlertFontFormat, 4);
			ParentalAlertOutline = DataUtil.GetBitBoolean(ParentalAlertFontFormat, 5);
			MessageAlertDuration = DataUtil.ObjToInt(RegUtil.GetRegValue("options", "MessageAlertDuration", 20));
			if ((MessageAlertDuration < 1) | (MessageAlertDuration > 999))
			{
				MessageAlertDuration = 30;
			}
			MessageAlertTextColour = Color.FromArgb(DataUtil.ObjToInt(RegUtil.GetRegValue("options", "MessageAlertTextColour", MessageAlertTextColour.ToArgb())));
			MessageAlertBackColour = Color.FromArgb(DataUtil.ObjToInt(RegUtil.GetRegValue("options", "MessageAlertBackColour", MessageAlertBackColour.ToArgb())));
			MessageAlertTextAlign = DataUtil.ObjToInt(RegUtil.GetRegValue("options", "MessageAlertTextAlign", 2));
			if ((MessageAlertTextAlign < 1) | (MessageAlertTextAlign > 3))
			{
				MessageAlertTextAlign = 2;
			}
			MessageAlertVerticalAlign = DataUtil.ObjToInt(RegUtil.GetRegValue("options", "MessageAlertVerticalAlign", 2));
			if ((MessageAlertVerticalAlign < 0) | (MessageAlertVerticalAlign > 2))
			{
				MessageAlertVerticalAlign = 2;
			}
			int inValue2 = DataUtil.ObjToInt(RegUtil.GetRegValue("options", "MessageAlertStyle", 3));
			MessageAlertScroll = DataUtil.GetBitBoolean(inValue2, 1);
			MessageAlertFlash = DataUtil.GetBitBoolean(inValue2, 2);
			MessageAlertTransparent = DataUtil.GetBitBoolean(inValue2, 3);
			MessageAlertFontName = RegUtil.GetRegValue("options", "MessageAlertFontName", "Microsoft Sans Serif");
			if (MessageAlertFontName == "")
			{
				MessageAlertFontName = "Microsoft Sans Serif";
			}
			MessageAlertFontSize = DataUtil.ObjToInt(RegUtil.GetRegValue("options", "MessageAlertFontSize", 22));
			if ((MessageAlertFontSize < 20) | (MessageAlertFontSize > 50))
			{
				MessageAlertFontSize = 25;
			}
			MessageAlertFontFormat = DataUtil.ObjToInt(RegUtil.GetRegValue("options", "MessageAlertFontFormat", 0));
			if ((MessageAlertFontFormat < 0) | (MessageAlertFontFormat > 100))
			{
				MessageAlertFontFormat = 0;
			}
			MessageAlertBold = DataUtil.GetBitBoolean(MessageAlertFontFormat, 1);
			MessageAlertItalic = DataUtil.GetBitBoolean(MessageAlertFontFormat, 2);
			MessageAlertUnderline = DataUtil.GetBitBoolean(MessageAlertFontFormat, 3);
			MessageAlertShadow = DataUtil.GetBitBoolean(MessageAlertFontFormat, 4);
			MessageAlertOutline = DataUtil.GetBitBoolean(MessageAlertFontFormat, 5);
			ReferenceAlertDuration = DataUtil.ObjToInt(RegUtil.GetRegValue("options", "ReferenceAlertDuration", 20));
			if ((ReferenceAlertDuration < 1) | (ReferenceAlertDuration > 999))
			{
				ReferenceAlertDuration = 30;
			}
			ReferenceAlertTextColour = Color.FromArgb(DataUtil.ObjToInt(RegUtil.GetRegValue("options", "ReferenceAlertTextColour", BlackScreenColour.ToArgb())));
			ReferenceAlertBackColour = Color.FromArgb(DataUtil.ObjToInt(RegUtil.GetRegValue("options", "ReferenceAlertBackColour", Color.White.ToArgb())));
			ReferenceAlertTextAlign = DataUtil.ObjToInt(RegUtil.GetRegValue("options", "ReferenceAlertTextAlign", 3));
			if ((ReferenceAlertTextAlign < 1) | (ReferenceAlertTextAlign > 3))
			{
				ReferenceAlertTextAlign = 3;
			}
			ReferenceAlertVerticalAlign = DataUtil.ObjToInt(RegUtil.GetRegValue("options", "ReferenceAlertVerticalAlign", 1));
			if ((ReferenceAlertVerticalAlign < 0) | (ReferenceAlertVerticalAlign > 2))
			{
				ReferenceAlertVerticalAlign = 1;
			}
			int inValue3 = DataUtil.ObjToInt(RegUtil.GetRegValue("options", "ReferenceAlertStyle", 1));
			ReferenceAlertScroll = DataUtil.GetBitBoolean(inValue3, 1);
			ReferenceAlertFlash = DataUtil.GetBitBoolean(inValue3, 2);
			ReferenceAlertTransparent = DataUtil.GetBitBoolean(inValue3, 3);
			ReferenceAlertFontName = RegUtil.GetRegValue("options", "ReferenceAlertFontName", "Microsoft Sans Serif");
			if (ReferenceAlertFontName == "")
			{
				ReferenceAlertFontName = "Microsoft Sans Serif";
			}
			ReferenceAlertFontSize = DataUtil.ObjToInt(RegUtil.GetRegValue("options", "ReferenceAlertFontSize", 22));
			if ((ReferenceAlertFontSize < 20) | (ReferenceAlertFontSize > 50))
			{
				ReferenceAlertFontSize = 25;
			}
			ReferenceAlertFontFormat = DataUtil.ObjToInt(RegUtil.GetRegValue("options", "ReferenceAlertFontFormat", 0));
			if ((ReferenceAlertFontFormat < 0) | (ReferenceAlertFontFormat > 100))
			{
				ReferenceAlertFontFormat = 0;
			}
			ReferenceAlertBold = DataUtil.GetBitBoolean(ReferenceAlertFontFormat, 1);
			ReferenceAlertItalic = DataUtil.GetBitBoolean(ReferenceAlertFontFormat, 2);
			ReferenceAlertUnderline = DataUtil.GetBitBoolean(ReferenceAlertFontFormat, 3);
			ReferenceAlertShadow = DataUtil.GetBitBoolean(ReferenceAlertFontFormat, 4);
			ReferenceAlertOutline = DataUtil.GetBitBoolean(ReferenceAlertFontFormat, 5);
			ReferenceAlertUsePick = ((DataUtil.ObjToInt(RegUtil.GetRegValue("options", "ReferenceAlertUsePick", 0)) > 0) ? true : false);
			ReferenceAlertBlankIfPickNotFound = ((DataUtil.ObjToInt(RegUtil.GetRegValue("options", "ReferenceAlertBlankIfPickNotFound", 0)) > 0) ? true : false);
			ReferenceAlertSource = DataUtil.ObjToInt(RegUtil.GetRegValue("options", "ReferenceAlertSource", 0));
			ReferenceAlertPickName = RegUtil.GetRegValue("options", "ReferenceAlertPickName", "");
			ReferenceAlertPickSubstitute = RegUtil.GetRegValue("options", "ReferenceAlertPickSubstitute", "");
			ReferenceAlertPickSeparator = RegUtil.GetRegValue("options", "ReferenceAlertPickSeparator", ",");
			UpdateV4RegDM();
			DMAlwaysUseSecondaryMonitor = ((DataUtil.ObjToInt(RegUtil.GetRegValue("monitors", "AlwaysTryDualMonitor", 1)) > 0) ? true : false);
			//daniel
			//스크린 Mode
			isScreenWideMode = ((DataUtil.ObjToInt(RegUtil.GetRegValue("monitors", "IsMonitorWide", 0)) > 0) ? true : false);

			DualMonitorSelectAutoOption = DataUtil.ObjToInt(RegUtil.GetRegValue("monitors", "DualMonitorOption", 0));
			if ((DualMonitorSelectAutoOption < 0) | (DualMonitorSelectAutoOption > 1))
			{
				/// 모니터 선택 옵션
				/// 0인 경우 사용자가 리스트에서 선택하여 자동으로 PPT 위치를 잡는다
				/// 1인 경우 사용자가 커스텀으로 모니터 위치를 수동으로 입력한다.
				DualMonitorSelectAutoOption = 0;
			}
			DMOption1Left = DataUtil.ObjToInt(RegUtil.GetRegValue("monitors", "DualMonitorOptionCustomLeft", 0));
			if ((DMOption1Left < -9999) | (DMOption1Left > 9999))
			{
				DMOption1Left = 0;
			}
			DMOption1Top = DataUtil.ObjToInt(RegUtil.GetRegValue("monitors", "DualMonitorOptionCustomTop", 0));
			if ((DMOption1Top < -9999) | (DMOption1Top > 9999))
			{
				DMOption1Top = 0;
			}
			DMOption1Width = DataUtil.ObjToInt(RegUtil.GetRegValue("monitors", "DualMonitorOptionCustomWidth", 1));
			if ((DMOption1Width < 1) | (DMOption1Width > 9999))
			{
				DMOption1Width = 100;
			}

			//if (!gf.isScreenWideMode)
			//	DMOption1Height = DMOption1Width * 3 / 4;
			//else
			//	DMOption1Height = DMOption1Height;

			DMOption1Height = DMOption1Width * 3 / 4;
			if (DMOption1Height < 1)
			{
				DMOption1Height = 1;
			}
			DMOption1AsSingleMonitor = ((DataUtil.ObjToInt(RegUtil.GetRegValue("monitors", "DualMonitorOptionCustomAsSingle", 0)) > 0) ? true : false);
			DisableSreenSaver = ((DataUtil.ObjToInt(RegUtil.GetRegValue("monitors", "DisableSreenSaver", 1)) > 0) ? true : false);
			VideoSize = DataUtil.ObjToInt(RegUtil.GetRegValue("monitors", "VideoSize", 100));
			if ((VideoSize < 25) | (VideoSize > 100))
			{
				VideoSize = 100;
			}
			VideoVAlign = DataUtil.ObjToInt(RegUtil.GetRegValue("monitors", "VideoVAlign", 1));
			if ((VideoVAlign < 0) | (VideoVAlign > 2))
			{
				VideoVAlign = 1;
			}
			LMAlwaysUseSecondaryMonitor = ((DataUtil.ObjToInt(RegUtil.GetRegValue("monitors", "AlwaysTrySecondaryLyricsMonitor", 1)) > 0) ? true : false);
			LMSelectAutoOption = DataUtil.ObjToInt(RegUtil.GetRegValue("monitors", "LyricsMonitorOption", 0));
			if ((LMSelectAutoOption < 0) | (LMSelectAutoOption > 1))
			{
				LMSelectAutoOption = 0;
			}
			LMOption1Left = DataUtil.ObjToInt(RegUtil.GetRegValue("monitors", "LyricsMonitorOptionCustomLeft", 0));
			if ((LMOption1Left < -9999) | (LMOption1Left > 9999))
			{
				LMOption1Left = 0;
			}
			LMOption1Top = DataUtil.ObjToInt(RegUtil.GetRegValue("monitors", "LyricsMonitorOptionCustomTop", 0));
			if ((LMOption1Top < -9999) | (LMOption1Top > 9999))
			{
				LMOption1Top = 0;
			}
			LMOption1Width = DataUtil.ObjToInt(RegUtil.GetRegValue("monitors", "LyricsMonitorOptionCustomWidth", 1));
			if ((LMOption1Width < 1) | (LMOption1Width > 9999))
			{
				LMOption1Width = 100;
			}

			if (!gf.isScreenWideMode)
				LMOption1Height = LMOption1Width * 3 / 4;
			else
                LMOption1Height = LMOption1Height;

			if (LMOption1Height < 1)
			{
				LMOption1Height = 1;
			}
			LMTextColour = Color.FromArgb(DataUtil.ObjToInt(RegUtil.GetRegValue("monitors", "LyricsMonitorTextColour", LMTextColour.ToArgb())));
			LMHighlightColour = Color.FromArgb(DataUtil.ObjToInt(RegUtil.GetRegValue("monitors", "LyricsMonitorHighlightColour", LMHighlightColour.ToArgb())));
			LMBackColour = Color.FromArgb(DataUtil.ObjToInt(RegUtil.GetRegValue("monitors", "LyricsMonitorBackColour", LMBackColour.ToArgb())));
			LMShowNotations = ((DataUtil.ObjToInt(RegUtil.GetRegValue("monitors", "LyricsMonitorShowNotations", 1)) > 0) ? true : false);
			LMMainFontSize = DataUtil.ObjToInt(RegUtil.GetRegValue("monitors", "LyricsMonitorFontSize", 22));
			if ((LMMainFontSize < 8) | (LMMainFontSize > 40))
			{
				LMMainFontSize = 20;
			}
			LMNotationsFontSize = DataUtil.ObjToInt(RegUtil.GetRegValue("options", "LyricsMonitorNotationsFontSize", 22));
			if ((LMNotationsFontSize < 8) | (LMNotationsFontSize > 40))
			{
				LMNotationsFontSize = 20;
			}
			LMFontFormat = DataUtil.ObjToInt(RegUtil.GetRegValue("options", "LyricsMonitorFontFormat", 0));
			if ((LMFontFormat < 0) | (LMFontFormat > 7))
			{
				LMFontFormat = 0;
			}
			LMFontBold = DataUtil.GetBitBoolean(LMFontFormat, 1);
			LMFontItalic = DataUtil.GetBitBoolean(LMFontFormat, 2);
			LMFontUnderline = DataUtil.GetBitBoolean(LMFontFormat, 3);
			AutoFocusTextRegion = false;
			UseFocusedTextRegionColour = ((DataUtil.ObjToInt(RegUtil.GetRegValue("options", "UseFocusedBackColour", 1)) > 0) ? true : false);
			FocusedTextRegionColour = Color.FromArgb(DataUtil.ObjToInt(RegUtil.GetRegValue("options", "FocusedBackColour", FocusedTextRegionColour.ToArgb())));
			TextRegionSlideTextColour = Color.FromArgb(DataUtil.ObjToInt(RegUtil.GetRegValue("options", "SelectedSlideTextColour", TextRegionSlideTextColour.ToArgb())));
			TextRegionSlideBackColour = Color.FromArgb(DataUtil.ObjToInt(RegUtil.GetRegValue("options", "SelectedSlideBackColour", TextRegionSlideBackColour.ToArgb())));
			string text2 = "";
			TotalMediaFileExt = 0;
			MediaExtensionsDatafile = RootEasiSlidesDir + "Admin\\Database\\MediaExtensions.txt";
			AudioExtensionsDatafile = RootEasiSlidesDir + "Admin\\Database\\AudioExtensions.txt";
			VideoExtensionsDatafile = RootEasiSlidesDir + "Admin\\Database\\VideoExtensions.txt";
			LoadMusicExtArray();
			JumpToA = DataUtil.ObjToInt(RegUtil.GetRegValue("options", "JumpToA", 1));
			if ((JumpToA < 1) | (JumpToA > 41))
			{
				JumpToA = 1;
			}
			JumpToB = DataUtil.ObjToInt(RegUtil.GetRegValue("options", "JumpToB", 2));
			if ((JumpToB < 1) | (JumpToB > 41))
			{
				JumpToB = 2;
			}
			JumpToC = DataUtil.ObjToInt(RegUtil.GetRegValue("options", "JumpToC", 3));
			if ((JumpToC < 1) | (JumpToC > 41))
			{
				JumpToC = 3;
			}
			LiveCamNumber = DataUtil.ObjToInt(RegUtil.GetRegValue("monitors", "LiveCamNumber", 1));
			if ((LiveCamNumber < 1) | (LiveCamNumber > 5))
			{
				LiveCamNumber = 1;
			}
			LiveCamVolume = DataUtil.ObjToInt(RegUtil.GetRegValue("monitors", "LiveCamVolume", 50));
			if ((LiveCamVolume < 0) | (LiveCamVolume > 100))
			{
				LiveCamVolume = 50;
			}
			LiveCamBalance = DataUtil.ObjToInt(RegUtil.GetRegValue("monitors", "LiveCamBalance", 0));
			if ((LiveCamBalance < -100) | (LiveCamBalance > 100))
			{
				LiveCamBalance = 0;
			}
			LiveCamWidescreen = ((DataUtil.ObjToInt(RegUtil.GetRegValue("monitors", "LiveCamWidescreen", 0)) > 0) ? true : false);
			LiveCamMute = ((DataUtil.ObjToInt(RegUtil.GetRegValue("monitors", "LiveCamMute", 0)) > 0) ? true : false);
			LiveCamNoPanelOverlay = ((DataUtil.ObjToInt(RegUtil.GetRegValue("monitors", "LiveCamNoPanelOverlay", 0)) > 0) ? true : false);
			FindItemInTitle = ((DataUtil.ObjToInt(RegUtil.GetRegValue("options", "FindItemInTitle", 1)) > 0) ? true : false);
			FindItemInContents = ((DataUtil.ObjToInt(RegUtil.GetRegValue("options", "FindItemInContents", 1)) > 0) ? true : false);
			FindItemInSongNumber = ((DataUtil.ObjToInt(RegUtil.GetRegValue("options", "FindItemInSongNumber", 1)) > 0) ? true : false);
			FindItemInBookRef = ((DataUtil.ObjToInt(RegUtil.GetRegValue("options", "FindItemInBookRef", 1)) > 0) ? true : false);
			FindItemInUserRef = ((DataUtil.ObjToInt(RegUtil.GetRegValue("options", "FindItemInUserRef", 1)) > 0) ? true : false);
			FindItemInLicAdmin = ((DataUtil.ObjToInt(RegUtil.GetRegValue("options", "FindItemInLicAdmin", 1)) > 0) ? true : false);
			FindItemInWriter = ((DataUtil.ObjToInt(RegUtil.GetRegValue("options", "FindItemInWriter", 1)) > 0) ? true : false);
			FindItemInCopyright = ((DataUtil.ObjToInt(RegUtil.GetRegValue("options", "FindItemInCopyright", 1)) > 0) ? true : false);
			FindItemUseDates = ((DataUtil.ObjToInt(RegUtil.GetRegValue("options", "FindItemUseDates", 0)) > 0) ? true : false);
			DateTime findItemDateFrom = DateTime.Now.Subtract(TimeSpan.FromDays(91.0));
			string s = DataUtil.ObjToString(RegUtil.GetRegValue("options", "FindItemDateFrom", findItemDateFrom.ToString()));
			try
			{
				FindItemDateFrom = DateTime.Parse(s);
			}
			catch
			{
				FindItemDateFrom = findItemDateFrom;
			}
			s = DataUtil.ObjToString(RegUtil.GetRegValue("options", "FindItemDateTo", DateTime.Now.ToString()));
			try
			{
				FindItemDateTo = DateTime.Parse(s);
			}
			catch
			{
				FindItemDateTo = DateTime.Now;
			}
			OutlineFontSizeThreshold = DataUtil.ObjToInt(RegUtil.GetRegValue("options", "OutlineFontSizeThreshold", 55));
			if ((OutlineFontSizeThreshold < 25) | (OutlineFontSizeThreshold > 100))
			{
				OutlineFontSizeThreshold = 55;
			}
			OUTPPPrefix = EasiSlidesTempDir + "~OUTPPPreview";
			PREPPPrefix = EasiSlidesTempDir + "~PREPPPreview";
			ExtPPrefix = EasiSlidesTempDir + "ExtPPPreview";
			LoadFolderNamesArray();
			ComputeShowLineSpacing();
			LoadLicAdminDetails();

			SaveConfigSettings();
		}

		//public static void LoadSavedData()
		//{
		//	for (int i = 0; i < 41; i++)
		//	{
		//		FindSongsFolder[i] = true;
		//	}
		//	RootEasiSlidesDir = RegUtil.GetRegValue("config", "root_directory", "C:\\EasiSlides\\");
		//	WorshipDir = RootEasiSlidesDir + "Admin\\WorshipLists\\";
		//	if (!Directory.Exists(WorshipDir))
		//	{
		//		FileUtil.MakeDir(WorshipDir);
		//	}
		//	InfoScreenDir = RootEasiSlidesDir + "InfoScreens\\";
		//	if (!Directory.Exists(InfoScreenDir))
		//	{
		//		FileUtil.MakeDir(InfoScreenDir);
		//	}
		//	PowerpointDir = RootEasiSlidesDir + "Powerpoint\\";
		//	if (!Directory.Exists(PowerpointDir))
		//	{
		//		FileUtil.MakeDir(PowerpointDir);
		//	}
		//	BibleDir = RootEasiSlidesDir + "HolyBibles\\";
		//	if (!Directory.Exists(BibleDir))
		//	{
		//		FileUtil.MakeDir(BibleDir);
		//	}
		//	PraiseBookDir = RootEasiSlidesDir + "Admin\\PraiseBooks\\";
		//	if (!Directory.Exists(PraiseBookDir))
		//	{
		//		FileUtil.MakeDir(PraiseBookDir);
		//	}
		//	WorshipTemplatesDir = RootEasiSlidesDir + "Admin\\Templates\\WorshipListsTemplates\\";
		//	if (!Directory.Exists(WorshipTemplatesDir))
		//	{
		//		FileUtil.MakeDir(WorshipTemplatesDir);
		//	}
		//	SettingsTemplatesDir = RootEasiSlidesDir + "Admin\\Templates\\SettingsTemplates\\";
		//	if (!Directory.Exists(SettingsTemplatesDir))
		//	{
		//		FileUtil.MakeDir(SettingsTemplatesDir);
		//	}
		//	EasiSlidesTempDir = Path.GetTempPath() + "EasiSlides Files\\";
		//	if (!Directory.Exists(EasiSlidesTempDir))
		//	{
		//		FileUtil.MakeDir(EasiSlidesTempDir);
		//	}
		//	MediaDir = RootEasiSlidesDir + "Media\\";
		//	if (!Directory.Exists(MediaDir))
		//	{
		//		FileUtil.MakeDir(MediaDir);
		//	}
		//	DocumentsDir = RootEasiSlidesDir + "Documents\\";
		//	if (!Directory.Exists(DocumentsDir))
		//	{
		//		FileUtil.MakeDir(DocumentsDir);
		//	}
		//	ImagesDir = RootEasiSlidesDir + "Images\\";
		//	if (!Directory.Exists(ImagesDir))
		//	{
		//		RestoreBackgroundImages();
		//	}
		//	if (!Directory.Exists(ImagesDir + "Tiles\\") || !Directory.Exists(ImagesDir + "Scenery\\"))
		//	{
		//		RestoreBackgroundImages();
		//	}
		//	CurSession = RegUtil.GetRegValue("config", "current_session", "");
		//	CurPraiseBook = RegUtil.GetRegValue("config", "current_praisebook", "");
		//	string text = RegUtil.GetRegValue("config", "media_dir", MediaDir);
		//	if (DataUtil.Right(text, 1) != "\\")
		//	{
		//		text += "\\";
		//	}
		//	if (Directory.Exists(text))
		//	{
		//		MediaDir = text;
		//	}
		//	ExportToDir  = RegUtil.GetRegValue("config", "export_dir", DocumentsDir);
		//	if (DataUtil.Right(ExportToDir, 1) != "\\")
		//	{
		//		ExportToDir += "\\";
		//	}
		//	ImportFromDir = RegUtil.GetRegValue("config", "import_dir", DocumentsDir);
		//	if (DataUtil.Right(ImportFromDir, 1) != "\\")
		//	{
		//		ImportFromDir += "\\";
		//	}
		//	PraiseOutputDir = RegUtil.GetRegValue("config", "praiseoutput_dir", DocumentsDir);
		//	if (DataUtil.Right(PraiseOutputDir, 1) != "\\")
		//	{
		//		PraiseOutputDir += "\\";
		//	}
		//	CurPraiseBook = RegUtil.GetRegValue("config", "current_praisebook", "");
		//	UseSongNumbers = ((DataUtil.ObjToInt(RegUtil.GetRegValue("options", "UseSongNumbers", 0)) > 0) ? true : false);
		//	PB_PrinterSpaces = ((DataUtil.ObjToInt(RegUtil.GetRegValue("options", "PrinterSpaces", 0)) > 0) ? 1 : 0);
		//	HB_MaxVersesSelection = DataUtil.ObjToInt(RegUtil.GetRegValue("options", "BibleMaxSelectVerses", 500));
		//	if (HB_MaxVersesSelection > 1000)
		//	{
		//		HB_MaxVersesSelection = 1000;
		//	}
		//	HB_MaxAdhocVersesSelection = DataUtil.ObjToInt(RegUtil.GetRegValue("options", "BibleMaxAdhocVersesSelection", 200));
		//	if (HB_MaxAdhocVersesSelection > 500)
		//	{
		//		HB_MaxAdhocVersesSelection = 200;
		//	}
		//	HB_ShowVerses = true;
		//	PP_MaxFiles = DataUtil.ObjToInt(RegUtil.GetRegValue("options", "PowerpointMaxFiles", 20));
		//	if (PP_MaxFiles > 100)
		//	{
		//		PP_MaxFiles = 100;
		//	}
		//	ShowRotateGap = DataUtil.ObjToInt(RegUtil.GetRegValue("options", "RotateGap", 0));
		//	if ((ShowRotateGap < 0) & (ShowRotateGap > 999))
		//	{
		//		ShowRotateGap = 0;
		//	}
		//	UsePowerpointTab = ((DataUtil.ObjToInt(RegUtil.GetRegValue("options", "UsePowerpointTab", 0)) > 0) ? true : false);
		//	NoPowerpointPanelOverlay = ((DataUtil.ObjToInt(RegUtil.GetRegValue("options", "NoPowerpointPanelOverlay", 0)) > 0) ? true : false);
		//	UseMediaTab = ((DataUtil.ObjToInt(RegUtil.GetRegValue("options", "UseMediaTab", 0)) > 0) ? true : false);
		//	ShowLyricsMonitorAlertBox = ((DataUtil.ObjToInt(RegUtil.GetRegValue("options", "ShowLyricsMonitorAlertBox", 0)) > 0) ? true : false);
		//	NoMediaPanelOverlay = ((DataUtil.ObjToInt(RegUtil.GetRegValue("options", "NoMediaPanelOverlay", 0)) > 0) ? true : false);
		//	AutoTextOverflow = ((DataUtil.ObjToInt(RegUtil.GetRegValue("options", "AutoTextOverflow", 1)) > 0) ? true : false);
		//	UseLargestFontSize = ((DataUtil.ObjToInt(RegUtil.GetRegValue("options", "UseLargestFontSize", 0)) > 0) ? true : false);
		//	AdvanceNextItem = ((DataUtil.ObjToInt(RegUtil.GetRegValue("options", "AdvanceNextItem", 0)) > 0) ? true : false);
		//	LineBetweenRegions = ((DataUtil.ObjToInt(RegUtil.GetRegValue("options", "LineBetweenRegions", 1)) > 0) ? true : false);
		//	GapItemOption = (GapType)DataUtil.ObjToInt(RegUtil.GetRegValue("options", "GapItemOption", 0));
		//	if ((GapItemOption < GapType.None) & (GapItemOption > GapType.User))
		//	{
		//		GapItemOption = GapType.None;
		//	}
		//	AltGapItemOption = GapType.None;
		//	GapItemLogoFile = RegUtil.GetRegValue("options", "GapItemLogoFile", "");
		//	GapItemUseFade = ((DataUtil.ObjToInt(RegUtil.GetRegValue("options", "GapItemUseFade", 1)) > 0) ? true : false);
		//	WordWrapLeftAlignIndent = ((DataUtil.ObjToInt(RegUtil.GetRegValue("options", "WordWrapLeftAlignIndent", 1)) > 0) ? true : false);
		//	WordWrapIgnoreStartSpaces = DataUtil.ObjToInt(RegUtil.GetRegValue("options", "WordWrapIgnoreStartSpaces", 8));
		//	if (WordWrapIgnoreStartSpaces < 1 || WordWrapIgnoreStartSpaces > 15)
		//	{
		//		WordWrapIgnoreStartSpaces = 9;
		//	}
		//	AutoRotateOn = ((DataUtil.ObjToInt(RegUtil.GetRegValue("options", "AutoRotateOn", 1)) > 0) ? true : false);
		//	AutoRotateStyle = DataUtil.ObjToInt(RegUtil.GetRegValue("options", "AutoRotateStyle", 3));
		//	if ((AutoRotateStyle < 1) & (AutoRotateStyle > 3))
		//	{
		//		AutoRotateStyle = 3;
		//	}
		//	int num = DataUtil.ObjToInt(RegUtil.GetRegValue("options", "NotationFontFactor", 75));
		//	if (num < 20 && num > 200)
		//	{
		//		num = 75;
		//	}
		//	NotationFontFactor = (float)num / 100f;
		//	PowerpointListingStyle = DataUtil.ObjToInt(RegUtil.GetRegValue("options", "ExternalListing", 0));
		//	if ((PowerpointListingStyle < 0) & (PowerpointListingStyle > 1))
		//	{
		//		PowerpointListingStyle = 0;
		//	}
		//	KeyBoardOption = DataUtil.ObjToInt(RegUtil.GetRegValue("options", "KeyBoardOption", 0));
		//	if ((KeyBoardOption < 0) & (KeyBoardOption > 1))
		//	{
		//		KeyBoardOption = 0;
		//	}
		//	EditMainFontName = RegUtil.GetRegValue("options", "EditMainFontName", "Microsoft Sans Serif");
		//	if (EditMainFontName == "")
		//	{
		//		EditMainFontName = "Microsoft Sans Serif";
		//	}
		//	EditMainFontSize = DataUtil.ObjToInt(RegUtil.GetRegValue("options", "EditMainFontSize", 12));
		//	if ((EditMainFontSize < 8) | (EditMainFontSize > 20))
		//	{
		//		EditMainFontSize = 12;
		//	}
		//	EditNotationFontSize = DataUtil.ObjToInt(RegUtil.GetRegValue("options", "EditNotationFontSize", 10));
		//	if ((EditNotationFontSize < 8) | (EditNotationFontSize > 20))
		//	{
		//		EditNotationFontSize = 10;
		//	}
		//	InfoMainFontName = RegUtil.GetRegValue("options", "InfoMainFontName", "Microsoft Sans Serif");
		//	if (InfoMainFontName == "")
		//	{
		//		InfoMainFontName = "Microsoft Sans Serif";
		//	}
		//	InfoMainFontSize = DataUtil.ObjToInt(RegUtil.GetRegValue("options", "InfoMainFontSize", 12));
		//	if ((InfoMainFontSize < 8) | (InfoMainFontSize > 20))
		//	{
		//		InfoMainFontSize = 12;
		//	}
		//	EditOpenDocumentDir = RegUtil.GetRegValue("options", "EditOpenDocumentDir", DocumentsDir);
		//	if (EditOpenDocumentDir == "")
		//	{
		//		EditOpenDocumentDir = DocumentsDir;
		//	}

		//	//OutputMonitorNumber = DataUtil.ObjToInt(RegUtil.GetRegValue("options", "OutputmonitorNumber", 1));
		//	//LyricsMonitorNumber = DataUtil.ObjToInt(RegUtil.GetRegValue("options", "LyricsMonitorNumber", 0));

		//	OutputMonitorName = RegUtil.GetRegValue("options", "OutputmonitorName", "None");
		//	LyricsMonitorName = RegUtil.GetRegValue("options", "LyricsMonitorName", "None");

		//	BibleText_FontSize = DataUtil.ObjToInt(RegUtil.GetRegValue("options", "BibleTextFontSize", 8));
		//	if ((BibleText_FontSize < 8) | (BibleText_FontSize > 20))
		//	{
		//		BibleText_FontSize = 8;
		//	}
		//	PreviewArea_FontSize = DataUtil.ObjToInt(RegUtil.GetRegValue("options", "PreviewAreaFontSize", 8));
		//	if ((PreviewArea_FontSize < 8) | (PreviewArea_FontSize > 20))
		//	{
		//		PreviewArea_FontSize = 8;
		//	}
		//	PreviewArea_ShowNotations = ((DataUtil.ObjToInt(RegUtil.GetRegValue("options", "PreviewAreaShowNotations", 0)) > 0) ? true : false);
		//	PreviewArea_LineBetweenScreens = ((DataUtil.ObjToInt(RegUtil.GetRegValue("options", "PreviewAreaLineBetweenScreens", 0)) > 0) ? true : false);
		//	ParentalAlertHeading = RegUtil.GetRegValue("options", "ParentalAlertHeading", "");
		//	ParentalAlertDuration = DataUtil.ObjToInt(RegUtil.GetRegValue("options", "ParentalAlertDuration", 20));
		//	if ((ParentalAlertDuration < 1) | (ParentalAlertDuration > 60))
		//	{
		//		ParentalAlertDuration = 30;
		//	}
		//	ParentalAlertTextColour = Color.FromArgb(DataUtil.ObjToInt(RegUtil.GetRegValue("options", "ParentalAlertTextColour", ParentalAlertTextColour.ToArgb())));
		//	ParentalAlertBackColour = Color.FromArgb(DataUtil.ObjToInt(RegUtil.GetRegValue("options", "ParentalAlertBackColour", ParentalAlertBackColour.ToArgb())));
		//	ParentalAlertTextAlign = DataUtil.ObjToInt(RegUtil.GetRegValue("options", "ParentalAlertTextAlign", 3));
		//	if ((ParentalAlertTextAlign < 1) | (ParentalAlertTextAlign > 3))
		//	{
		//		ParentalAlertTextAlign = 3;
		//	}
		//	ParentalAlertVerticalAlign = DataUtil.ObjToInt(RegUtil.GetRegValue("options", "ParentalAlertVerticalAlign", 2));
		//	if ((ParentalAlertVerticalAlign < 0) | (ParentalAlertVerticalAlign > 2))
		//	{
		//		ParentalAlertVerticalAlign = 2;
		//	}
		//	int inValue = DataUtil.ObjToInt(RegUtil.GetRegValue("options", "ParentalAlertStyle", 3));
		//	ParentalAlertScroll = DataUtil.GetBitBoolean(inValue, 1);
		//	ParentalAlertFlash = DataUtil.GetBitBoolean(inValue, 2);
		//	ParentalAlertTransparent = DataUtil.GetBitBoolean(inValue, 3);
		//	ParentalAlertFontName = RegUtil.GetRegValue("options", "ParentalAlertFontName", "Microsoft Sans Serif");
		//	if (ParentalAlertFontName == "")
		//	{
		//		ParentalAlertFontName = "Microsoft Sans Serif";
		//	}
		//	ParentalAlertFontSize = DataUtil.ObjToInt(RegUtil.GetRegValue("options", "ParentalAlertFontSize", 22));
		//	if ((ParentalAlertFontSize < 20) | (ParentalAlertFontSize > 50))
		//	{
		//		ParentalAlertFontSize = 25;
		//	}
		//	ParentalAlertFontFormat = DataUtil.ObjToInt(RegUtil.GetRegValue("options", "ParentalAlertFontFormat", 0));
		//	if ((ParentalAlertFontFormat < 0) | (ParentalAlertFontFormat > 100))
		//	{
		//		ParentalAlertFontFormat = 0;
		//	}
		//	ParentalAlertBold = DataUtil.GetBitBoolean(ParentalAlertFontFormat, 1);
		//	ParentalAlertItalic = DataUtil.GetBitBoolean(ParentalAlertFontFormat, 2);
		//	ParentalAlertUnderline = DataUtil.GetBitBoolean(ParentalAlertFontFormat, 3);
		//	ParentalAlertShadow = DataUtil.GetBitBoolean(ParentalAlertFontFormat, 4);
		//	ParentalAlertOutline = DataUtil.GetBitBoolean(ParentalAlertFontFormat, 5);
		//	MessageAlertDuration = DataUtil.ObjToInt(RegUtil.GetRegValue("options", "MessageAlertDuration", 20));
		//	if ((MessageAlertDuration < 1) | (MessageAlertDuration > 999))
		//	{
		//		MessageAlertDuration = 30;
		//	}
		//	MessageAlertTextColour = Color.FromArgb(DataUtil.ObjToInt(RegUtil.GetRegValue("options", "MessageAlertTextColour", MessageAlertTextColour.ToArgb())));
		//	MessageAlertBackColour = Color.FromArgb(DataUtil.ObjToInt(RegUtil.GetRegValue("options", "MessageAlertBackColour", MessageAlertBackColour.ToArgb())));
		//	MessageAlertTextAlign = DataUtil.ObjToInt(RegUtil.GetRegValue("options", "MessageAlertTextAlign", 2));
		//	if ((MessageAlertTextAlign < 1) | (MessageAlertTextAlign > 3))
		//	{
		//		MessageAlertTextAlign = 2;
		//	}
		//	MessageAlertVerticalAlign = DataUtil.ObjToInt(RegUtil.GetRegValue("options", "MessageAlertVerticalAlign", 2));
		//	if ((MessageAlertVerticalAlign < 0) | (MessageAlertVerticalAlign > 2))
		//	{
		//		MessageAlertVerticalAlign = 2;
		//	}
		//	int inValue2 = DataUtil.ObjToInt(RegUtil.GetRegValue("options", "MessageAlertStyle", 3));
		//	MessageAlertScroll = DataUtil.GetBitBoolean(inValue2, 1);
		//	MessageAlertFlash = DataUtil.GetBitBoolean(inValue2, 2);
		//	MessageAlertTransparent = DataUtil.GetBitBoolean(inValue2, 3);
		//	MessageAlertFontName = RegUtil.GetRegValue("options", "MessageAlertFontName", "Microsoft Sans Serif");
		//	if (MessageAlertFontName == "")
		//	{
		//		MessageAlertFontName = "Microsoft Sans Serif";
		//	}
		//	MessageAlertFontSize = DataUtil.ObjToInt(RegUtil.GetRegValue("options", "MessageAlertFontSize", 22));
		//	if ((MessageAlertFontSize < 20) | (MessageAlertFontSize > 50))
		//	{
		//		MessageAlertFontSize = 25;
		//	}
		//	MessageAlertFontFormat = DataUtil.ObjToInt(RegUtil.GetRegValue("options", "MessageAlertFontFormat", 0));
		//	if ((MessageAlertFontFormat < 0) | (MessageAlertFontFormat > 100))
		//	{
		//		MessageAlertFontFormat = 0;
		//	}
		//	MessageAlertBold = DataUtil.GetBitBoolean(MessageAlertFontFormat, 1);
		//	MessageAlertItalic = DataUtil.GetBitBoolean(MessageAlertFontFormat, 2);
		//	MessageAlertUnderline = DataUtil.GetBitBoolean(MessageAlertFontFormat, 3);
		//	MessageAlertShadow = DataUtil.GetBitBoolean(MessageAlertFontFormat, 4);
		//	MessageAlertOutline = DataUtil.GetBitBoolean(MessageAlertFontFormat, 5);
		//	ReferenceAlertDuration = DataUtil.ObjToInt(RegUtil.GetRegValue("options", "ReferenceAlertDuration", 20));
		//	if ((ReferenceAlertDuration < 1) | (ReferenceAlertDuration > 999))
		//	{
		//		ReferenceAlertDuration = 30;
		//	}
		//	ReferenceAlertTextColour = Color.FromArgb(DataUtil.ObjToInt(RegUtil.GetRegValue("options", "ReferenceAlertTextColour", BlackScreenColour.ToArgb())));
		//	ReferenceAlertBackColour = Color.FromArgb(DataUtil.ObjToInt(RegUtil.GetRegValue("options", "ReferenceAlertBackColour", Color.White.ToArgb())));
		//	ReferenceAlertTextAlign = DataUtil.ObjToInt(RegUtil.GetRegValue("options", "ReferenceAlertTextAlign", 3));
		//	if ((ReferenceAlertTextAlign < 1) | (ReferenceAlertTextAlign > 3))
		//	{
		//		ReferenceAlertTextAlign = 3;
		//	}
		//	ReferenceAlertVerticalAlign = DataUtil.ObjToInt(RegUtil.GetRegValue("options", "ReferenceAlertVerticalAlign", 1));
		//	if ((ReferenceAlertVerticalAlign < 0) | (ReferenceAlertVerticalAlign > 2))
		//	{
		//		ReferenceAlertVerticalAlign = 1;
		//	}
		//	int inValue3 = DataUtil.ObjToInt(RegUtil.GetRegValue("options", "ReferenceAlertStyle", 1));
		//	ReferenceAlertScroll = DataUtil.GetBitBoolean(inValue3, 1);
		//	ReferenceAlertFlash = DataUtil.GetBitBoolean(inValue3, 2);
		//	ReferenceAlertTransparent = DataUtil.GetBitBoolean(inValue3, 3);
		//	ReferenceAlertFontName = RegUtil.GetRegValue("options", "ReferenceAlertFontName", "Microsoft Sans Serif");
		//	if (ReferenceAlertFontName == "")
		//	{
		//		ReferenceAlertFontName = "Microsoft Sans Serif";
		//	}
		//	ReferenceAlertFontSize = DataUtil.ObjToInt(RegUtil.GetRegValue("options", "ReferenceAlertFontSize", 22));
		//	if ((ReferenceAlertFontSize < 20) | (ReferenceAlertFontSize > 50))
		//	{
		//		ReferenceAlertFontSize = 25;
		//	}
		//	ReferenceAlertFontFormat = DataUtil.ObjToInt(RegUtil.GetRegValue("options", "ReferenceAlertFontFormat", 0));
		//	if ((ReferenceAlertFontFormat < 0) | (ReferenceAlertFontFormat > 100))
		//	{
		//		ReferenceAlertFontFormat = 0;
		//	}
		//	ReferenceAlertBold = DataUtil.GetBitBoolean(ReferenceAlertFontFormat, 1);
		//	ReferenceAlertItalic = DataUtil.GetBitBoolean(ReferenceAlertFontFormat, 2);
		//	ReferenceAlertUnderline = DataUtil.GetBitBoolean(ReferenceAlertFontFormat, 3);
		//	ReferenceAlertShadow = DataUtil.GetBitBoolean(ReferenceAlertFontFormat, 4);
		//	ReferenceAlertOutline = DataUtil.GetBitBoolean(ReferenceAlertFontFormat, 5);
		//	ReferenceAlertUsePick = ((DataUtil.ObjToInt(RegUtil.GetRegValue("options", "ReferenceAlertUsePick", 0)) > 0) ? true : false);
		//	ReferenceAlertBlankIfPickNotFound = ((DataUtil.ObjToInt(RegUtil.GetRegValue("options", "ReferenceAlertBlankIfPickNotFound", 0)) > 0) ? true : false);
		//	ReferenceAlertSource = DataUtil.ObjToInt(RegUtil.GetRegValue("options", "ReferenceAlertSource", 0));
		//	ReferenceAlertPickName = RegUtil.GetRegValue("options", "ReferenceAlertPickName", "");
		//	ReferenceAlertPickSubstitute = RegUtil.GetRegValue("options", "ReferenceAlertPickSubstitute", "");
		//	ReferenceAlertPickSeparator = RegUtil.GetRegValue("options", "ReferenceAlertPickSeparator", ",");
		//	UpdateV4RegDM();
		//	DMAlwaysUseSecondaryMonitor = ((DataUtil.ObjToInt(RegUtil.GetRegValue("monitors", "AlwaysTryDualMonitor", 1)) > 0) ? true : false);
		//	//daniel
		//	//스크린 Mode
		//	isScreenWideMode = ((DataUtil.ObjToInt(RegUtil.GetRegValue("monitors", "IsMonitorWide", 0)) > 0) ? true : false);

		//	DualMonitorSelectAutoOption = DataUtil.ObjToInt(RegUtil.GetRegValue("monitors", "DualMonitorOption", 0));
		//	if ((DualMonitorSelectAutoOption < 0) | (DualMonitorSelectAutoOption > 1))
		//	{
		//		/// 모니터 선택 옵션
		//		/// 0인 경우 사용자가 리스트에서 선택하여 자동으로 PPT 위치를 잡는다
		//		/// 1인 경우 사용자가 커스텀으로 모니터 위치를 수동으로 입력한다.
		//		DualMonitorSelectAutoOption = 0; 
		//	}
		//	DMOption1Left = DataUtil.ObjToInt(RegUtil.GetRegValue("monitors", "DualMonitorOptionCustomLeft", 0));
		//	if ((DMOption1Left < -9999) | (DMOption1Left > 9999))
		//	{
		//		DMOption1Left = 0;
		//	}
		//	DMOption1Top = DataUtil.ObjToInt(RegUtil.GetRegValue("monitors", "DualMonitorOptionCustomTop", 0));
		//	if ((DMOption1Top < -9999) | (DMOption1Top > 9999))
		//	{
		//		DMOption1Top = 0;
		//	}
		//	DMOption1Width = DataUtil.ObjToInt(RegUtil.GetRegValue("monitors", "DualMonitorOptionCustomWidth", 1));
		//	if ((DMOption1Width < 1) | (DMOption1Width > 9999))
		//	{
		//		DMOption1Width = 100;
		//	}
		//	DMOption1Height = DMOption1Width * 3 / 4;
		//	if (DMOption1Height < 1)
		//	{
		//		DMOption1Height = 1;
		//	}
		//	DMOption1AsSingleMonitor = ((DataUtil.ObjToInt(RegUtil.GetRegValue("monitors", "DualMonitorOptionCustomAsSingle", 0)) > 0) ? true : false);
		//	DisableSreenSaver = ((DataUtil.ObjToInt(RegUtil.GetRegValue("monitors", "DisableSreenSaver", 1)) > 0) ? true : false);
		//	VideoSize = DataUtil.ObjToInt(RegUtil.GetRegValue("monitors", "VideoSize", 100));
		//	if ((VideoSize < 25) | (VideoSize > 100))
		//	{
		//		VideoSize = 100;
		//	}
		//	VideoVAlign = DataUtil.ObjToInt(RegUtil.GetRegValue("monitors", "VideoVAlign", 1));
		//	if ((VideoVAlign < 0) | (VideoVAlign > 2))
		//	{
		//		VideoVAlign = 1;
		//	}
		//	LMAlwaysUseSecondaryMonitor = ((DataUtil.ObjToInt(RegUtil.GetRegValue("monitors", "AlwaysTrySecondaryLyricsMonitor", 1)) > 0) ? true : false);
		//	LMSelectAutoOption = DataUtil.ObjToInt(RegUtil.GetRegValue("monitors", "LyricsMonitorOption", 0));
		//	if ((LMSelectAutoOption < 0) | (LMSelectAutoOption > 1))
		//	{
		//		LMSelectAutoOption = 0;
		//	}
		//	LMOption1Left = DataUtil.ObjToInt(RegUtil.GetRegValue("monitors", "LyricsMonitorOptionCustomLeft", 0));
		//	if ((LMOption1Left < -9999) | (LMOption1Left > 9999))
		//	{
		//		LMOption1Left = 0;
		//	}
		//	LMOption1Top = DataUtil.ObjToInt(RegUtil.GetRegValue("monitors", "LyricsMonitorOptionCustomTop", 0));
		//	if ((LMOption1Top < -9999) | (LMOption1Top > 9999))
		//	{
		//		LMOption1Top = 0;
		//	}
		//	LMOption1Width = DataUtil.ObjToInt(RegUtil.GetRegValue("monitors", "LyricsMonitorOptionCustomWidth", 1));
		//	if ((LMOption1Width < 1) | (LMOption1Width > 9999))
		//	{
		//		LMOption1Width = 100;
		//	}
		//	LMOption1Height = LMOption1Width * 3 / 4;
		//	if (LMOption1Height < 1)
		//	{
		//		LMOption1Height = 1;
		//	}
		//	LMTextColour = Color.FromArgb(DataUtil.ObjToInt(RegUtil.GetRegValue("monitors", "LyricsMonitorTextColour", LMTextColour.ToArgb())));
		//	LMHighlightColour = Color.FromArgb(DataUtil.ObjToInt(RegUtil.GetRegValue("monitors", "LyricsMonitorHighlightColour", LMHighlightColour.ToArgb())));
		//	LMBackColour = Color.FromArgb(DataUtil.ObjToInt(RegUtil.GetRegValue("monitors", "LyricsMonitorBackColour", LMBackColour.ToArgb())));
		//	LMShowNotations = ((DataUtil.ObjToInt(RegUtil.GetRegValue("monitors", "LyricsMonitorShowNotations", 1)) > 0) ? true : false);
		//	LMMainFontSize = DataUtil.ObjToInt(RegUtil.GetRegValue("monitors", "LyricsMonitorFontSize", 22));
		//	if ((LMMainFontSize < 8) | (LMMainFontSize > 40))
		//	{
		//		LMMainFontSize = 20;
		//	}
		//	LMNotationsFontSize = DataUtil.ObjToInt(RegUtil.GetRegValue("options", "LyricsMonitorNotationsFontSize", 22));
		//	if ((LMNotationsFontSize < 8) | (LMNotationsFontSize > 40))
		//	{
		//		LMNotationsFontSize = 20;
		//	}
		//	LMFontFormat = DataUtil.ObjToInt(RegUtil.GetRegValue("options", "LyricsMonitorFontFormat", 0));
		//	if ((LMFontFormat < 0) | (LMFontFormat > 7))
		//	{
		//		LMFontFormat = 0;
		//	}
		//	LMFontBold = DataUtil.GetBitBoolean(LMFontFormat, 1);
		//	LMFontItalic = DataUtil.GetBitBoolean(LMFontFormat, 2);
		//	LMFontUnderline = DataUtil.GetBitBoolean(LMFontFormat, 3);
		//	AutoFocusTextRegion = false;
		//	UseFocusedTextRegionColour = ((DataUtil.ObjToInt(RegUtil.GetRegValue("options", "UseFocusedBackColour", 1)) > 0) ? true : false);
		//	FocusedTextRegionColour = Color.FromArgb(DataUtil.ObjToInt(RegUtil.GetRegValue("options", "FocusedBackColour", FocusedTextRegionColour.ToArgb())));
		//	TextRegionSlideTextColour = Color.FromArgb(DataUtil.ObjToInt(RegUtil.GetRegValue("options", "SelectedSlideTextColour", TextRegionSlideTextColour.ToArgb())));
		//	TextRegionSlideBackColour = Color.FromArgb(DataUtil.ObjToInt(RegUtil.GetRegValue("options", "SelectedSlideBackColour", TextRegionSlideBackColour.ToArgb())));
		//	string text2 = "";
		//	TotalMediaFileExt = 0;
		//	MediaExtensionsDatafile = RootEasiSlidesDir + "Admin\\Database\\MediaExtensions.txt";
		//	AudioExtensionsDatafile = RootEasiSlidesDir + "Admin\\Database\\AudioExtensions.txt";
		//	VideoExtensionsDatafile = RootEasiSlidesDir + "Admin\\Database\\VideoExtensions.txt";
		//	LoadMusicExtArray();
		//	JumpToA = DataUtil.ObjToInt(RegUtil.GetRegValue("options", "JumpToA", 1));
		//	if ((JumpToA < 1) | (JumpToA > 41))
		//	{
		//		JumpToA = 1;
		//	}
		//	JumpToB = DataUtil.ObjToInt(RegUtil.GetRegValue("options", "JumpToB", 2));
		//	if ((JumpToB < 1) | (JumpToB > 41))
		//	{
		//		JumpToB = 2;
		//	}
		//	JumpToC = DataUtil.ObjToInt(RegUtil.GetRegValue("options", "JumpToC", 3));
		//	if ((JumpToC < 1) | (JumpToC > 41))
		//	{
		//		JumpToC = 3;
		//	}
		//	LiveCamNumber = DataUtil.ObjToInt(RegUtil.GetRegValue("monitors", "LiveCamNumber", 1));
		//	if ((LiveCamNumber < 1) | (LiveCamNumber > 5))
		//	{
		//		LiveCamNumber = 1;
		//	}
		//	LiveCamVolume = DataUtil.ObjToInt(RegUtil.GetRegValue("monitors", "LiveCamVolume", 50));
		//	if ((LiveCamVolume < 0) | (LiveCamVolume > 100))
		//	{
		//		LiveCamVolume = 50;
		//	}
		//	LiveCamBalance = DataUtil.ObjToInt(RegUtil.GetRegValue("monitors", "LiveCamBalance", 0));
		//	if ((LiveCamBalance < -100) | (LiveCamBalance > 100))
		//	{
		//		LiveCamBalance = 0;
		//	}
		//	LiveCamWidescreen = ((DataUtil.ObjToInt(RegUtil.GetRegValue("monitors", "LiveCamWidescreen", 0)) > 0) ? true : false);
		//	LiveCamMute = ((DataUtil.ObjToInt(RegUtil.GetRegValue("monitors", "LiveCamMute", 0)) > 0) ? true : false);
		//	LiveCamNoPanelOverlay = ((DataUtil.ObjToInt(RegUtil.GetRegValue("monitors", "LiveCamNoPanelOverlay", 0)) > 0) ? true : false);
		//	FindItemInTitle = ((DataUtil.ObjToInt(RegUtil.GetRegValue("options", "FindItemInTitle", 1)) > 0) ? true : false);
		//	FindItemInContents = ((DataUtil.ObjToInt(RegUtil.GetRegValue("options", "FindItemInContents", 1)) > 0) ? true : false);
		//	FindItemInSongNumber = ((DataUtil.ObjToInt(RegUtil.GetRegValue("options", "FindItemInSongNumber", 1)) > 0) ? true : false);
		//	FindItemInBookRef = ((DataUtil.ObjToInt(RegUtil.GetRegValue("options", "FindItemInBookRef", 1)) > 0) ? true : false);
		//	FindItemInUserRef = ((DataUtil.ObjToInt(RegUtil.GetRegValue("options", "FindItemInUserRef", 1)) > 0) ? true : false);
		//	FindItemInLicAdmin = ((DataUtil.ObjToInt(RegUtil.GetRegValue("options", "FindItemInLicAdmin", 1)) > 0) ? true : false);
		//	FindItemInWriter = ((DataUtil.ObjToInt(RegUtil.GetRegValue("options", "FindItemInWriter", 1)) > 0) ? true : false);
		//	FindItemInCopyright = ((DataUtil.ObjToInt(RegUtil.GetRegValue("options", "FindItemInCopyright", 1)) > 0) ? true : false);
		//	FindItemUseDates = ((DataUtil.ObjToInt(RegUtil.GetRegValue("options", "FindItemUseDates", 0)) > 0) ? true : false);
		//	DateTime findItemDateFrom = DateTime.Now.Subtract(TimeSpan.FromDays(91.0));
		//	string s = DataUtil.ObjToString(RegUtil.GetRegValue("options", "FindItemDateFrom", findItemDateFrom.ToString()));
		//	try
		//	{
		//		FindItemDateFrom = DateTime.Parse(s);
		//	}
		//	catch
		//	{
		//		FindItemDateFrom = findItemDateFrom;
		//	}
		//	s = DataUtil.ObjToString(RegUtil.GetRegValue("options", "FindItemDateTo", DateTime.Now.ToString()));
		//	try
		//	{
		//		FindItemDateTo = DateTime.Parse(s);
		//	}
		//	catch
		//	{
		//		FindItemDateTo = DateTime.Now;
		//	}
		//	OutlineFontSizeThreshold = DataUtil.ObjToInt(RegUtil.GetRegValue("options", "OutlineFontSizeThreshold", 55));
		//	if ((OutlineFontSizeThreshold < 25) | (OutlineFontSizeThreshold > 100))
		//	{
		//		OutlineFontSizeThreshold = 55;
		//	}
		//	OUTPPPrefix = EasiSlidesTempDir + "~OUTPPPreview";
		//	PREPPPrefix = EasiSlidesTempDir + "~PREPPPreview";
		//	ExtPPrefix = EasiSlidesTempDir + "ExtPPPreview";
		//	LoadFolderNamesArray();
		//	ComputeShowLineSpacing();
		//	LoadLicAdminDetails();

		//	SaveConfigSettings();
		//}

		public static void UpdateV4RegDM()
		{
			if (DataUtil.ObjToInt(RegUtil.GetRegValue("options", "AlwaysTryDualMonitor", -1)) >= 0)
			{
				DMAlwaysUseSecondaryMonitor = ((DataUtil.ObjToInt(RegUtil.GetRegValue("options", "AlwaysTryDualMonitor", 1)) > 0) ? true : false);
				DualMonitorSelectAutoOption = DataUtil.ObjToInt(RegUtil.GetRegValue("options", "DualMonitorOption", 0));
				DMOption1Left = DataUtil.ObjToInt(RegUtil.GetRegValue("options", "DualMonitorOptionCustomLeft", 0));
				DMOption1Top = DataUtil.ObjToInt(RegUtil.GetRegValue("options", "DualMonitorOptionCustomTop", 0));
				DMOption1Width = DataUtil.ObjToInt(RegUtil.GetRegValue("options", "DualMonitorOptionCustomWidth", 1));
				RegUtil.SaveRegValue("monitors", "AlwaysTryDualMonitor", DMAlwaysUseSecondaryMonitor ? 1 : 0);
				RegUtil.SaveRegValue("monitors", "DualMonitorOption", DualMonitorSelectAutoOption);
				RegUtil.SaveRegValue("monitors", "DualMonitorOptionCustomLeft", DMOption1Left);
				RegUtil.SaveRegValue("monitors", "DualMonitorOptionCustomTop", DMOption1Top);
				RegUtil.SaveRegValue("monitors", "DualMonitorOptionCustomWidth", DMOption1Width);
				RegUtil.DeleletRegKey("options", "AlwaysTryDualMonitor");
				RegUtil.DeleletRegKey("options", "DualMonitorOption");
				RegUtil.DeleletRegKey("options", "DualMonitorOptionCustomLeft");
				RegUtil.DeleletRegKey("options", "DualMonitorOptionCustomTop");
				RegUtil.DeleletRegKey("options", "DualMonitorOptionCustomWidth");
			}
		}

		public static void LoadMusicExtArray()
		{
			TotalMediaFileExt = 0;
			LoadMusicExtArray("AudioExtensions.txt", MediaBackgroundStyle.Audio);
			LoadMusicExtArray("VideoExtensions.txt", MediaBackgroundStyle.Video);
		}

		public static void LoadMusicExtArray(string InFile, MediaBackgroundStyle InMediaType)
		{
			if (InMediaType != MediaBackgroundStyle.Video)
			{
				InMediaType = MediaBackgroundStyle.Audio;
			}
			string text = RootEasiSlidesDir + "Admin\\Database\\" + InFile;
			if (!File.Exists(text))
			{
				if (File.Exists(Application.StartupPath + "\\Sys\\" + InFile))
				{
					try
					{
						File.Copy(Application.StartupPath + "\\Sys\\" + InFile, text);
					}
					catch
					{
						FileUtil.CreateNewFile(text, FileUtil.FileContentsType.Ascii_Rtf);
					}
				}
				else
				{
					FileUtil.CreateNewFile(text, FileUtil.FileContentsType.Ascii_Rtf);
				}
			}
			using StreamReader streamReader = File.OpenText(text);
			string text2 = "";
			while ((text2 = streamReader.ReadLine()) != null)
			{
				text2 = DataUtil.TrimEnd(text2);
				if (text2 != "" && TotalMediaFileExt < 3000 && ValidateMusicExt(ref text2, ShowMessage: false))
				{
					MediaFileExtension[TotalMediaFileExt, 0] = text2.ToLower();
					MediaFileExtension[TotalMediaFileExt, 1] = InMediaType.ToString();
					TotalMediaFileExt++;
				}
			}
			//streamReader.Close();
		}

		public static bool ValidateMusicExt(ref string InExtension, bool ShowMessage)
		{
			if (!ValidateDirNameFormat(InExtension, ShowMessage ? "Music File Extension" : ""))
			{
				return false;
			}
			InExtension = DataUtil.Trim(InExtension);
			string text = "";
			for (int i = 0; i < InExtension.Length; i++)
			{
				if (InExtension[i] != '.')
				{
					text += DataUtil.Mid(InExtension, i, 1);
				}
			}
			if (text[0] != '.')
			{
				text = "." + text;
			}
			InExtension = text;
			return true;
		}

		public static string GetOpenFileDialogMediaString()
		{
			string openFileDialogMediaString = GetOpenFileDialogMediaString(MediaBackgroundStyle.None);
			string openFileDialogMediaString2 = GetOpenFileDialogMediaString(MediaBackgroundStyle.Audio);
			string openFileDialogMediaString3 = GetOpenFileDialogMediaString(MediaBackgroundStyle.Video);
			string str = "All Files (*.*)|*.*";
			openFileDialogMediaString = ((openFileDialogMediaString != "") ? (openFileDialogMediaString + "|") : "");
			openFileDialogMediaString2 = ((openFileDialogMediaString2 != "") ? (openFileDialogMediaString2 + "|") : "");
			openFileDialogMediaString3 = ((openFileDialogMediaString3 != "") ? (openFileDialogMediaString3 + "|") : "");
			return openFileDialogMediaString + openFileDialogMediaString2 + openFileDialogMediaString3 + str;
		}

		public static string GetOpenFileDialogMediaString(MediaBackgroundStyle InMediaType)
		{
			if (TotalMediaFileExt == 0)
			{
				return "";
			}
			string str = "";
			string text = "";
			int num = 0;
			switch (InMediaType)
			{
				case MediaBackgroundStyle.Audio:
					str = "Audio Files (";
					break;
				case MediaBackgroundStyle.Video:
					str = "Video Files (";
					break;
			}
			bool flag = true;
			for (int i = 0; i < TotalMediaFileExt; i++)
			{
				if (InMediaType == MediaBackgroundStyle.None || MediaFileExtension[i, 1] == InMediaType.ToString())
				{
					str = str + (flag ? "" : ",") + "*" + MediaFileExtension[i, 0];
					text = text + (flag ? "" : ";") + "*" + MediaFileExtension[i, 0];
					flag = false;
				}
			}
			if (flag)
			{
				return "";
			}
			str = ((InMediaType != 0) ? (str + ")") : "Media Files (all types)");
			return str + "|" + text;
		}

		public static bool ValidateDir(string FDir, bool CreateDir)
		{
			string text = DataUtil.Trim(FDir);
			if (text != "" && DataUtil.Right(text, 1) != "\\")
			{
				text += "\\";
			}
			if (Directory.Exists(text))
			{
				return true;
			}
			if (CreateDir)
			{
				return FileUtil.MakeDir(text);
			}
			return false;
		}

		public static bool ValidateDirNameFormat(string InString)
		{
			return ValidateDirNameFormat(InString, "");
		}

		public static bool ValidateDirNameFormat(string InString, string Heading)
		{
			if (!InString.Contains("\\") && !InString.Contains("/") && !InString.Contains(":") && !InString.Contains("*") && !InString.Contains("?") && !InString.Contains("\"") && !InString.Contains("<") && !InString.Contains(">") && !InString.Contains("|"))
			{
				return true;
			}
			if (Heading != "")
			{
				MessageBox.Show(Heading + " must not contain the characters: \\ / : * ? \" < > |");
			}
			return false;
		}

		public static string CorrectDirNameFormat(string InString)
		{
			InString = InString.Replace("\\", "");
			InString = InString.Replace("/", "");
			InString = InString.Replace(":", "");
			InString = InString.Replace("*", "");
			InString = InString.Replace("?", "");
			InString = InString.Replace("\"", "");
			InString = InString.Replace("<", "");
			InString = InString.Replace(">", "");
			InString = InString.Replace("|", "");
			return InString;
		}

#if DAO
		public static void LoadFolderNamesArray()
        {
            ValidateDefaultFolders();
            int num = 0;
            string text = "";
            string text2 = "";
            string fullSearchString = "select * from FOLDER where FolderNo >=0 and FolderNo < " + DataUtil.ObjToString(41);
            Recordset recordSet = DbDaoController.GetRecordSet(ConnectStringMainDB, fullSearchString);
            if (recordSet?.EOF ?? true)
            {
                return;
            }
            recordSet.MoveFirst();
            while (!recordSet.EOF)
            {
                num = DataUtil.GetDataInt(recordSet, "FolderNo");
                FolderName[num] = DataUtil.GetDataString(recordSet, "name");
                FolderUse[num] = DataUtil.GetDataInt(recordSet, "Use");
                FolderGroupStyle[num] = (SortBy)DataUtil.GetDataInt(recordSet, "GroupStyle");
                FolderLyricsHeading[num, 0] = DataUtil.GetDataString(recordSet, "PreChorusHeading");
                FolderLyricsHeading[num, 1] = DataUtil.GetDataString(recordSet, "ChorusHeading");
                FolderLyricsHeading[num, 2] = DataUtil.GetDataString(recordSet, "BridgeHeading");
                FolderLyricsHeading[num, 3] = DataUtil.GetDataString(recordSet, "EndingHeading");
                FolderHeadingPercentSize[num] = DataUtil.ObjToInt(DataUtil.GetDataString(recordSet, "HeadingSize"));
                if ((FolderHeadingPercentSize[num] < 0) | (FolderHeadingPercentSize[num] > 150))
                {
                    FolderHeadingPercentSize[num] = 100;
                }
                text2 = DataUtil.GetDataString(recordSet, "HeadingOption");
                if (text2 == "")
                {
                    text2 = DataUtil.ObjToString(HeadingFormat.AsRegion1Plus);
                }
                FolderHeadingOption[num] = DataUtil.ObjToInt(text2);
                if ((FolderHeadingOption[num] < 0) | (FolderHeadingOption[num] > 2))
                {
                    FolderHeadingOption[num] = DataUtil.ObjToInt(HeadingFormat.AsRegion1Plus);
                }
                text = DataUtil.GetDataString(recordSet, "BIUHeading");
                if (text == "")
                {
                    text = "4";
                }
                FolderHeadingFontBold[num, 0] = DataUtil.GetBitValue(DataUtil.ObjToInt(text), 1);
                FolderHeadingFontItalic[num, 0] = DataUtil.GetBitValue(DataUtil.ObjToInt(text), 2);
                FolderHeadingFontUnderline[num, 0] = DataUtil.GetBitValue(DataUtil.ObjToInt(text), 3);
                FolderHeadingFontBold[num, 1] = DataUtil.GetBitValue(DataUtil.ObjToInt(text), 4);
                FolderHeadingFontItalic[num, 1] = DataUtil.GetBitValue(DataUtil.ObjToInt(text), 5);
                FolderHeadingFontUnderline[num, 1] = DataUtil.GetBitValue(DataUtil.ObjToInt(text), 6);
                ShowLineSpacing[num, 0] = DataUtil.GetDataDouble(recordSet, "LineSpacing");
                if ((ShowLineSpacing[num, 0] < 0.5) | (ShowLineSpacing[num, 0] > 2.0))
                {
                    ShowLineSpacing[num, 0] = 1.0;
                }
                ShowLineSpacing[num, 1] = DataUtil.GetDataDouble(recordSet, "LineSpacing2");
                if ((ShowLineSpacing[num, 1] < 0.5) | (ShowLineSpacing[num, 1] > 2.0))
                {
                    ShowLineSpacing[num, 1] = 1.0;
                }
                ShowLeftMargin[num] = 2;
                ShowLeftMargin[num] = DataUtil.GetDataInt(recordSet, "LMargin");
                if ((ShowLeftMargin[num] < 0) | (ShowLeftMargin[num] > 40))
                {
                    ShowLeftMargin[num] = 2;
                }
                ShowRightMargin[num] = 2;
                ShowRightMargin[num] = DataUtil.GetDataInt(recordSet, "RMargin");
                if ((ShowRightMargin[num] < 0) | (ShowRightMargin[num] > 40))
                {
                    ShowRightMargin[num] = 2;
                }
                ShowBottomMargin[num] = 0;
                ShowBottomMargin[num] = DataUtil.GetDataInt(recordSet, "BMargin");
                if ((ShowBottomMargin[num] < 0) | (ShowBottomMargin[num] > 100))
                {
                    ShowBottomMargin[num] = 0;
                }
                ShowFontSize[num, 0] = DataUtil.ObjToInt(DataUtil.GetDataString(recordSet, "Size0"));
                if ((ShowFontSize[num, 0] < 1) | (ShowFontSize[num, 0] > 100))
                {
                    ShowFontSize[num, 0] = 37;
                }
                ShowFontBold[num, 0] = DataUtil.GetBitValue(DataUtil.GetDataInt(recordSet, "BIU0"), 1);
                ShowFontItalic[num, 0] = DataUtil.GetBitValue(DataUtil.GetDataInt(recordSet, "BIU0"), 2);
                ShowFontUnderline[num, 0] = DataUtil.GetBitValue(DataUtil.GetDataInt(recordSet, "BIU0"), 3);
                ShowFontBold[num, 2] = DataUtil.GetBitValue(DataUtil.GetDataInt(recordSet, "BIU0"), 4);
                ShowFontItalic[num, 2] = DataUtil.GetBitValue(DataUtil.GetDataInt(recordSet, "BIU0"), 5);
                ShowFontUnderline[num, 2] = DataUtil.GetBitValue(DataUtil.GetDataInt(recordSet, "BIU0"), 6);
                ShowFontRTL[num, 0] = DataUtil.GetBitValue(DataUtil.GetDataInt(recordSet, "BIU0"), 7);
                ShowFontName[num, 0] = DataUtil.GetDataString(recordSet, "FontName0");
                if (ShowFontName[num, 0] == "")
                {
                    ShowFontName[num, 0] = "Microsoft Sans Serif";
                }
                ShowFontVPosition[num, 0] = DataUtil.ObjToInt(DataUtil.GetDataString(recordSet, "Vpos0"));
                if ((ShowFontVPosition[num, 0] < 0) | (ShowFontVPosition[num, 0] > 100))
                {
                    ShowFontVPosition[num, 0] = 0;
                }
                ShowFontSize[num, 1] = DataUtil.ObjToInt(DataUtil.GetDataString(recordSet, "Size1"));
                if ((ShowFontSize[num, 1] < 1) | (ShowFontSize[num, 1] > 100))
                {
                    ShowFontSize[num, 1] = 37;
                }
                ShowFontBold[num, 1] = DataUtil.GetBitValue(DataUtil.GetDataInt(recordSet, "BIU1"), 1);
                ShowFontItalic[num, 1] = DataUtil.GetBitValue(DataUtil.GetDataInt(recordSet, "BIU1"), 2);
                ShowFontUnderline[num, 1] = DataUtil.GetBitValue(DataUtil.GetDataInt(recordSet, "BIU1"), 3);
                ShowFontBold[num, 3] = DataUtil.GetBitValue(DataUtil.GetDataInt(recordSet, "BIU1"), 4);
                ShowFontItalic[num, 3] = DataUtil.GetBitValue(DataUtil.GetDataInt(recordSet, "BIU1"), 5);
                ShowFontUnderline[num, 3] = DataUtil.GetBitValue(DataUtil.GetDataInt(recordSet, "BIU1"), 6);
                ShowFontRTL[num, 1] = DataUtil.GetBitValue(DataUtil.GetDataInt(recordSet, "BIU1"), 7);
                ShowFontName[num, 1] = DataUtil.GetDataString(recordSet, "FontName1");
                if (ShowFontName[num, 1] == "")
                {
                    ShowFontName[num, 1] = "Microsoft Sans Serif";
                }
                ShowFontVPosition[num, 1] = DataUtil.ObjToInt(DataUtil.GetDataString(recordSet, "Vpos1"));
                if ((ShowFontVPosition[num, 1] < ShowFontVPosition[num, 0]) | (ShowFontVPosition[num, 1] > 100))
                {
                    ShowFontVPosition[num, 1] = ShowFontVPosition[num, 0] + (100 - ShowFontVPosition[num, 0]) / 2;
                }
                recordSet.MoveNext();
            }
        }
#elif SQLite
		public static void LoadFolderNamesArray()
		{
			ValidateDefaultFolders();
			int num = 0;
			string text = "";
			string text2 = "";
			string fullSearchString = "select * from FOLDER where FolderNo >=0 and FolderNo < " + DataUtil.ObjToString(41);

			DbConnection connection = null;
			DbDataReader dataReader = null;

			(connection, dataReader) = DbController.GetDataReader(ConnectStringMainDB, fullSearchString);

			using (connection)
			{
				using (dataReader)
				{
					if (dataReader == null || !dataReader.HasRows)
					{
						return;
					}

					while (dataReader.Read())
					{
						num = DataUtil.GetDataInt(dataReader, "FolderNo");
						FolderName[num] = DataUtil.GetDataString(dataReader, "name");
						FolderUse[num] = DataUtil.GetDataInt(dataReader, "Use");
						FolderGroupStyle[num] = (SortBy)DataUtil.GetDataInt(dataReader, "GroupStyle");
						FolderLyricsHeading[num, 0] = DataUtil.GetDataString(dataReader, "PreChorusHeading");
						FolderLyricsHeading[num, 1] = DataUtil.GetDataString(dataReader, "ChorusHeading");
						FolderLyricsHeading[num, 2] = DataUtil.GetDataString(dataReader, "BridgeHeading");
						FolderLyricsHeading[num, 3] = DataUtil.GetDataString(dataReader, "EndingHeading");
						FolderHeadingPercentSize[num] = DataUtil.ObjToInt(DataUtil.GetDataString(dataReader, "HeadingSize"));
						if ((FolderHeadingPercentSize[num] < 0) | (FolderHeadingPercentSize[num] > 150))
						{
							FolderHeadingPercentSize[num] = 100;
						}
						text2 = DataUtil.GetDataString(dataReader, "HeadingOption");
						if (text2 == "")
						{
							text2 = DataUtil.ObjToString(HeadingFormat.AsRegion1Plus);
						}
						FolderHeadingOption[num] = DataUtil.ObjToInt(text2);
						if ((FolderHeadingOption[num] < 0) | (FolderHeadingOption[num] > 2))
						{
							FolderHeadingOption[num] = DataUtil.ObjToInt(HeadingFormat.AsRegion1Plus);
						}
						text = DataUtil.GetDataString(dataReader, "BIUHeading");
						if (text == "")
						{
							text = "4";
						}
						FolderHeadingFontBold[num, 0] = DataUtil.GetBitValue(DataUtil.ObjToInt(text), 1);
						FolderHeadingFontItalic[num, 0] = DataUtil.GetBitValue(DataUtil.ObjToInt(text), 2);
						FolderHeadingFontUnderline[num, 0] = DataUtil.GetBitValue(DataUtil.ObjToInt(text), 3);
						FolderHeadingFontBold[num, 1] = DataUtil.GetBitValue(DataUtil.ObjToInt(text), 4);
						FolderHeadingFontItalic[num, 1] = DataUtil.GetBitValue(DataUtil.ObjToInt(text), 5);
						FolderHeadingFontUnderline[num, 1] = DataUtil.GetBitValue(DataUtil.ObjToInt(text), 6);
						ShowLineSpacing[num, 0] = DataUtil.GetDataDouble(dataReader, "LineSpacing");
						if ((ShowLineSpacing[num, 0] < 0.5) | (ShowLineSpacing[num, 0] > 2.0))
						{
							ShowLineSpacing[num, 0] = 1.0;
						}
						ShowLineSpacing[num, 1] = DataUtil.GetDataDouble(dataReader, "LineSpacing2");
						if ((ShowLineSpacing[num, 1] < 0.5) | (ShowLineSpacing[num, 1] > 2.0))
						{
							ShowLineSpacing[num, 1] = 1.0;
						}
						ShowLeftMargin[num] = 2;
						ShowLeftMargin[num] = DataUtil.GetDataInt(dataReader, "LMargin");
						if ((ShowLeftMargin[num] < 0) | (ShowLeftMargin[num] > 40))
						{
							ShowLeftMargin[num] = 2;
						}
						ShowRightMargin[num] = 2;
						ShowRightMargin[num] = DataUtil.GetDataInt(dataReader, "RMargin");
						if ((ShowRightMargin[num] < 0) | (ShowRightMargin[num] > 40))
						{
							ShowRightMargin[num] = 2;
						}
						ShowBottomMargin[num] = 0;
						ShowBottomMargin[num] = DataUtil.GetDataInt(dataReader, "BMargin");
						if ((ShowBottomMargin[num] < 0) | (ShowBottomMargin[num] > 100))
						{
							ShowBottomMargin[num] = 0;
						}
						ShowFontSize[num, 0] = DataUtil.ObjToInt(DataUtil.GetDataString(dataReader, "Size0"));
						if ((ShowFontSize[num, 0] < 1) | (ShowFontSize[num, 0] > 100))
						{
							ShowFontSize[num, 0] = 37;
						}
						ShowFontBold[num, 0] = DataUtil.GetBitValue(DataUtil.GetDataInt(dataReader, "BIU0"), 1);
						ShowFontItalic[num, 0] = DataUtil.GetBitValue(DataUtil.GetDataInt(dataReader, "BIU0"), 2);
						ShowFontUnderline[num, 0] = DataUtil.GetBitValue(DataUtil.GetDataInt(dataReader, "BIU0"), 3);
						ShowFontBold[num, 2] = DataUtil.GetBitValue(DataUtil.GetDataInt(dataReader, "BIU0"), 4);
						ShowFontItalic[num, 2] = DataUtil.GetBitValue(DataUtil.GetDataInt(dataReader, "BIU0"), 5);
						ShowFontUnderline[num, 2] = DataUtil.GetBitValue(DataUtil.GetDataInt(dataReader, "BIU0"), 6);
						ShowFontRTL[num, 0] = DataUtil.GetBitValue(DataUtil.GetDataInt(dataReader, "BIU0"), 7);
						ShowFontName[num, 0] = DataUtil.GetDataString(dataReader, "FontName0");
						if (ShowFontName[num, 0] == "")
						{
							ShowFontName[num, 0] = "Microsoft Sans Serif";
						}
						ShowFontVPosition[num, 0] = DataUtil.ObjToInt(DataUtil.GetDataString(dataReader, "Vpos0"));
						if ((ShowFontVPosition[num, 0] < 0) | (ShowFontVPosition[num, 0] > 100))
						{
							ShowFontVPosition[num, 0] = 0;
						}
						ShowFontSize[num, 1] = DataUtil.ObjToInt(DataUtil.GetDataString(dataReader, "Size1"));
						if ((ShowFontSize[num, 1] < 1) | (ShowFontSize[num, 1] > 100))
						{
							ShowFontSize[num, 1] = 37;
						}
						ShowFontBold[num, 1] = DataUtil.GetBitValue(DataUtil.GetDataInt(dataReader, "BIU1"), 1);
						ShowFontItalic[num, 1] = DataUtil.GetBitValue(DataUtil.GetDataInt(dataReader, "BIU1"), 2);
						ShowFontUnderline[num, 1] = DataUtil.GetBitValue(DataUtil.GetDataInt(dataReader, "BIU1"), 3);
						ShowFontBold[num, 3] = DataUtil.GetBitValue(DataUtil.GetDataInt(dataReader, "BIU1"), 4);
						ShowFontItalic[num, 3] = DataUtil.GetBitValue(DataUtil.GetDataInt(dataReader, "BIU1"), 5);
						ShowFontUnderline[num, 3] = DataUtil.GetBitValue(DataUtil.GetDataInt(dataReader, "BIU1"), 6);
						ShowFontRTL[num, 1] = DataUtil.GetBitValue(DataUtil.GetDataInt(dataReader, "BIU1"), 7);
						ShowFontName[num, 1] = DataUtil.GetDataString(dataReader, "FontName1");
						if (ShowFontName[num, 1] == "")
						{
							ShowFontName[num, 1] = "Microsoft Sans Serif";
						}
						ShowFontVPosition[num, 1] = DataUtil.ObjToInt(DataUtil.GetDataString(dataReader, "Vpos1"));
						if ((ShowFontVPosition[num, 1] < ShowFontVPosition[num, 0]) | (ShowFontVPosition[num, 1] > 100))
						{
							ShowFontVPosition[num, 1] = ShowFontVPosition[num, 0] + (100 - ShowFontVPosition[num, 0]) / 2;
						}
					}
				}
			}
		}
#endif

		public static void ComputeShowLineSpacing()
		{
			for (int i = 0; i < 41; i++)
			{
				MainFontSpacingFactor[i, 0] = ShowLineSpacing[i, 0] + 0.05;
				MainFontSpacingFactor[i, 1] = ShowLineSpacing[i, 1] + 0.05;
			}
		}

		public static void LoadLicAdminDetails()
		{
			LicAdminEnforceDisplay = ((DataUtil.ObjToInt(RegUtil.GetRegValue("config", "licEnforceDisplay", 1)) > 0) ? true : false);
			LicAdminNoSymbol = RegUtil.GetRegValue("config", "licNoSym", "#");
			LicAdmin_List[0, 0] = "1";
			LicAdmin_List[1, 0] = "None";
			LicAdmin_List[2, 0] = "Public Domain";
			LicAdmin_List[3, 0] = "CCLI";
			int num = 0;
			for (num = 4; num < 9; num++)
			{
				LicAdmin_List[num, 0] = "";
			}
			string fullSearchString = "select * from LICENCE where Ref >=4 and Ref < " + DataUtil.ObjToString(9);
#if OleDb
			DataTable datatable = DbOleDbController.GetDataTable(ConnectStringMainDB, fullSearchString);
#elif SQLite
			using DataTable datatable = DbController.GetDataTable(ConnectStringMainDB, fullSearchString);
#endif
			if (datatable.Rows.Count > 0)
			{
				//recordSet.MoveFirst();
				//while (!recordSet.EOF)
				foreach (DataRow dr in datatable.Rows)
				{
					num = DataUtil.GetDataInt(dr, "Ref");
					LicAdmin_List[num, 0] = DataUtil.Trim(DataUtil.GetDataString(dr, "ADMINISTRATOR"));
					//recordSet.MoveNext();
				}
			}
			LicAdmin_List[1, 1] = "";
			LicAdmin_List[2, 1] = "Public Domain";
			LicAdmin_List[3, 1] = DataUtil.Trim(RegUtil.GetRegValue("config", "licCCLI_no", ""));
			LicAdmin_List[4, 1] = DataUtil.Trim(RegUtil.GetRegValue("config", "lic4_no", ""));
			LicAdmin_List[5, 1] = DataUtil.Trim(RegUtil.GetRegValue("config", "lic5_no", ""));
			LicAdmin_List[6, 1] = DataUtil.Trim(RegUtil.GetRegValue("config", "lic6_no", ""));
			LicAdmin_List[7, 1] = DataUtil.Trim(RegUtil.GetRegValue("config", "lic7_no", ""));
			LicAdmin_List[8, 1] = DataUtil.Trim(RegUtil.GetRegValue("config", "lic8_no", ""));
			LicAdmin_List[1, 2] = "";
			LicAdmin_List[2, 2] = "Public Domain";
			for (num = 3; num < 9; num++)
			{
				if ((LicAdmin_List[num, 0] == "") | (LicAdmin_List[num, 1] == ""))
				{
					LicAdmin_List[num, 2] = "";
				}
				else
				{
					LicAdmin_List[num, 2] = LicAdmin_List[num, 0] + LicAdminNoSymbol + LicAdmin_List[num, 1];
				}
			}
			LicAdmin_List[0, 1] = LicAdmin_List[1, 1];
			LicAdmin_List[0, 2] = LicAdmin_List[1, 2];
		}

		/// <summary>
		/// daniel
		/// </summary>
		public static void SaveConfigSettings()
		{

			RegUtil.SaveRegValue("config", "root_directory", RootEasiSlidesDir);
			RegUtil.SaveRegValue("config", "current_session", CurSession);
			RegUtil.SaveRegValue("config", "current_praisebook", CurPraiseBook);
			RegUtil.SaveRegValue("config", "export_dir", ExportToDir);
			RegUtil.SaveRegValue("config", "import_dir", ImportFromDir);
			RegUtil.SaveRegValue("config", "praiseoutput_dir", PraiseOutputDir);
			RegUtil.SaveRegValue("config", "media_dir", MediaDir);

			SaveOptionsData();
			SaveLicenceConfigSettings();
			SaveFoldersSettings();
		}

		/// <summary>
		/// daniel
		/// </summary>
#if OleDb
		public static void SaveFoldersSettings4()
        {
            int num = 0;
            if (ValidateDefaultFolders(0))
            {
                try
                {
                    string text = "";
                    Database daoDb = DbDaoController.GetDaoDb(ConnectStringMainDB);
                    Recordset recordset = null;
					for (int i = 1; i < 41; i++)
                    {
                        num = i;
                        if (FolderName[i] != "")
                        {
                            text = "select * from Folder where FolderNo=" + i;
                            recordset = DbDaoController.GetRecordSet(daoDb, text);
                            recordset.Edit();
                            recordset.Fields["name"].Value = FolderName[i];
                            recordset.Fields["Use"].Value = FolderUse[i];
                            recordset.Fields["GroupStyle"].Value = FolderGroupStyle[i];
                            recordset.Fields["PreChorusHeading"].Value = FolderLyricsHeading[i, 0];
                            recordset.Fields["ChorusHeading"].Value = FolderLyricsHeading[i, 1];
                            recordset.Fields["BridgeHeading"].Value = FolderLyricsHeading[i, 2];
                            recordset.Fields["EndingHeading"].Value = FolderLyricsHeading[i, 3];
                            recordset.Fields["BIUHeading"].Value = FolderHeadingFontBold[i, 0] + FolderHeadingFontItalic[i, 0] * 2 + FolderHeadingFontUnderline[i, 0] * 4 + FolderHeadingFontBold[i, 1] * 8 + FolderHeadingFontItalic[i, 1] * 16 + FolderHeadingFontUnderline[i, 1] * 32;
                            recordset.Fields["HeadingSize"].Value = FolderHeadingPercentSize[i];
                            recordset.Fields["HeadingOption"].Value = FolderHeadingOption[i];
                            recordset.Fields["LineSpacing"].Value = ShowLineSpacing[i, 0];
                            recordset.Fields["LineSpacing2"].Value = ShowLineSpacing[i, 1];
                            recordset.Fields["BIU0"].Value = ShowFontBold[i, 0] + ShowFontItalic[i, 0] * 2 + ShowFontUnderline[i, 0] * 4 + ShowFontBold[i, 2] * 8 + ShowFontItalic[i, 2] * 16 + ShowFontUnderline[i, 2] * 32 + ShowFontRTL[i, 0] * 64;
                            recordset.Fields["Size0"].Value = ShowFontSize[i, 0];
                            recordset.Fields["FontName0"].Value = ShowFontName[i, 0];
                            recordset.Fields["Vpos0"].Value = ShowFontVPosition[i, 0];
                            recordset.Fields["BIU1"].Value = ShowFontBold[i, 1] + ShowFontItalic[i, 1] * 2 + ShowFontUnderline[i, 1] * 4 + ShowFontBold[i, 3] * 8 + ShowFontItalic[i, 3] * 16 + ShowFontUnderline[i, 3] * 32 + ShowFontRTL[i, 1] * 64;
                            recordset.Fields["Size1"].Value = ShowFontSize[i, 1];
                            recordset.Fields["FontName1"].Value = ShowFontName[i, 1];
                            recordset.Fields["Vpos1"].Value = ShowFontVPosition[i, 1];
                            recordset.Fields["LMargin"].Value = ShowLeftMargin[i];
                            recordset.Fields["RMargin"].Value = ShowRightMargin[i];
                            recordset.Fields["BMargin"].Value = ShowBottomMargin[i];
                            recordset.Update();
                            recordset.Close();
                        }
                    }
                    if (recordset != null)
                    {
                        recordset = null;
                    }
					if (daoDb != null)
					{
						daoDb.Close();
						daoDb = null;
					}

				}
                catch
                {
                    MessageBox.Show("Error: Cannot Save Folder Settings for Folder Index: " + num);
                }
            }
        }
#elif SQLite
		public static void SaveFoldersSettings4()
		{
			int num = 0;
			if (ValidateDefaultFolders(0))
			{
				try
				{
					string sQuery = "select * from Folder";

					using DbConnection connection = DbController.GetDbConnection(ConnectStringMainDB);
					using DataTable dataTable = DbController.GetDataTable(ConnectStringMainDB, sQuery);

					DataColumn[] primarykey = new DataColumn[] { dataTable.Columns["FolderNo"] };
					dataTable.PrimaryKey = primarykey;

					for (int i = 1; i < 41; i++)
					{
						num = i;
						if (FolderName[i] != "")
						{
							DataRow dr = dataTable.Rows.Find($"{i}");
							if (dr != null)
							{
								dr["name"] = FolderName[i];
								dr["Use"] = FolderUse[i];
								dr["GroupStyle"] = FolderGroupStyle[i];
								dr["PreChorusHeading"] = FolderLyricsHeading[i, 0];
								dr["ChorusHeading"] = FolderLyricsHeading[i, 1];
								dr["BridgeHeading"] = FolderLyricsHeading[i, 2];
								dr["EndingHeading"] = FolderLyricsHeading[i, 3];
								dr["BIUHeading"] = FolderHeadingFontBold[i, 0] + FolderHeadingFontItalic[i, 0] * 2 + FolderHeadingFontUnderline[i, 0] * 4 + FolderHeadingFontBold[i, 1] * 8 + FolderHeadingFontItalic[i, 1] * 16 + FolderHeadingFontUnderline[i, 1] * 32;
								dr["HeadingSize"] = FolderHeadingPercentSize[i];
								dr["HeadingOption"] = FolderHeadingOption[i];
								dr["LineSpacing"] = ShowLineSpacing[i, 0];
								dr["LineSpacing2"] = ShowLineSpacing[i, 1];
								dr["BIU0"] = ShowFontBold[i, 0] + ShowFontItalic[i, 0] * 2 + ShowFontUnderline[i, 0] * 4 + ShowFontBold[i, 2] * 8 + ShowFontItalic[i, 2] * 16 + ShowFontUnderline[i, 2] * 32 + ShowFontRTL[i, 0] * 64;
								dr["Size0"] = ShowFontSize[i, 0];
								dr["FontName0"] = ShowFontName[i, 0];
								dr["Vpos0"] = ShowFontVPosition[i, 0];
								dr["BIU1"] = ShowFontBold[i, 1] + ShowFontItalic[i, 1] * 2 + ShowFontUnderline[i, 1] * 4 + ShowFontBold[i, 3] * 8 + ShowFontItalic[i, 3] * 16 + ShowFontUnderline[i, 3] * 32 + ShowFontRTL[i, 1] * 64;
								dr["Size1"] = ShowFontSize[i, 1];
								dr["FontName1"] = ShowFontName[i, 1];
								dr["Vpos1"] = ShowFontVPosition[i, 1];
								dr["LMargin"] = ShowLeftMargin[i];
								dr["RMargin"] = ShowRightMargin[i];
								dr["BMargin"] = ShowBottomMargin[i];
							}

						}
					}

					DbController.UpdateTable(connection, sQuery, dataTable);
				}
				catch
				{
					MessageBox.Show("Error: Cannot Save Folder Settings for Folder Index: " + num);
				}
			}
		}
#endif

#if OleDb
		public static void SaveFoldersSettings()
		{
			int num = 0;
			int rowIndex = 0;

			if (ValidateDefaultFolders(0))
			{
				try
				{
					string text = "";
					using (OleDbConnection daoDb = DbConnectionController.GetOleDbConnection(ConnectStringMainDB))
					{
						OleDbDataAdapter da = null;
						DataSet ds = null;
						DataTable dt = null;

						text = "select * from Folder where FolderNo > 0 and FolderNo <= 41";

						(da, ds) = DbOleDbController.getDataAdapter(daoDb, text);
						dt = ds.Tables[0];
						if (dt.Rows.Count > 0)
						{
							for (int i = 1; i < 41; i++)
							{
								num = i;
								rowIndex = i - 1;
								if (FolderName[i] != "")
								{
									dt.Rows[rowIndex]["name"] = FolderName[i];
									dt.Rows[rowIndex]["Use"] = FolderUse[i];
									dt.Rows[rowIndex]["GroupStyle"] = FolderGroupStyle[i];
									dt.Rows[rowIndex]["PreChorusHeading"] = FolderLyricsHeading[i, 0];
									dt.Rows[rowIndex]["ChorusHeading"] = FolderLyricsHeading[i, 1];
									dt.Rows[rowIndex]["BridgeHeading"] = FolderLyricsHeading[i, 2];
									dt.Rows[rowIndex]["EndingHeading"] = FolderLyricsHeading[i, 3];
									dt.Rows[rowIndex]["BIUHeading"] = FolderHeadingFontBold[i, 0] + FolderHeadingFontItalic[i, 0] * 2 + FolderHeadingFontUnderline[i, 0] * 4 + FolderHeadingFontBold[i, 1] * 8 + FolderHeadingFontItalic[i, 1] * 16 + FolderHeadingFontUnderline[i, 1] * 32;
									dt.Rows[rowIndex]["HeadingSize"] = FolderHeadingPercentSize[i];
									dt.Rows[rowIndex]["HeadingOption"] = FolderHeadingOption[i];
									dt.Rows[rowIndex]["LineSpacing"] = ShowLineSpacing[i, 0];
									dt.Rows[rowIndex]["LineSpacing2"] = ShowLineSpacing[i, 1];
									dt.Rows[rowIndex]["BIU0"] = ShowFontBold[i, 0] + ShowFontItalic[i, 0] * 2 + ShowFontUnderline[i, 0] * 4 + ShowFontBold[i, 2] * 8 + ShowFontItalic[i, 2] * 16 + ShowFontUnderline[i, 2] * 32 + ShowFontRTL[i, 0] * 64;
									dt.Rows[rowIndex]["Size0"] = ShowFontSize[i, 0];
									dt.Rows[rowIndex]["FontName0"] = ShowFontName[i, 0];
									dt.Rows[rowIndex]["Vpos0"] = ShowFontVPosition[i, 0];
									dt.Rows[rowIndex]["BIU1"] = ShowFontBold[i, 1] + ShowFontItalic[i, 1] * 2 + ShowFontUnderline[i, 1] * 4 + ShowFontBold[i, 3] * 8 + ShowFontItalic[i, 3] * 16 + ShowFontUnderline[i, 3] * 32 + ShowFontRTL[i, 1] * 64;
									dt.Rows[rowIndex]["Size1"] = ShowFontSize[i, 1];
									dt.Rows[rowIndex]["FontName1"] = ShowFontName[i, 1];
									dt.Rows[rowIndex]["Vpos1"] = ShowFontVPosition[i, 1];
									dt.Rows[rowIndex]["LMargin"] = ShowLeftMargin[i];
									dt.Rows[rowIndex]["RMargin"] = ShowRightMargin[i];
									dt.Rows[rowIndex]["BMargin"] = ShowBottomMargin[i];									
								}
							}
							da.Update(dt);
							dt.Dispose();
							da.Dispose();
						}
					}
				}
				catch (Exception e)
				{
					Console.WriteLine(e.Message);
					Console.WriteLine(e.StackTrace);
				}
			}
		}
#elif SQLite
		public static void SaveFoldersSettings()
		{
			int num = 0;
			int rowIndex = 0;

			if (ValidateDefaultFolders(0))
			{
				try
				{
					string text = "";
					using DbConnection connection = DbController.GetDbConnection(ConnectStringMainDB);

					DbDataAdapter sQLiteDataAdapter = null;
					DataTable dataTable = null;

					text = "select * from Folder where FolderNo > 0 and FolderNo <= 41";
					(sQLiteDataAdapter, dataTable) = DbController.getDataAdapter(connection, text);

					if (dataTable.Rows.Count > 0)
					{
						for (int i = 1; i < 41; i++)
						{
							num = i;
							rowIndex = i - 1;
							if (FolderName[i] != "")
							{
								dataTable.Rows[rowIndex]["name"] = FolderName[i];
								dataTable.Rows[rowIndex]["Use"] = FolderUse[i];
								dataTable.Rows[rowIndex]["GroupStyle"] = FolderGroupStyle[i];
								dataTable.Rows[rowIndex]["PreChorusHeading"] = FolderLyricsHeading[i, 0];
								dataTable.Rows[rowIndex]["ChorusHeading"] = FolderLyricsHeading[i, 1];
								dataTable.Rows[rowIndex]["BridgeHeading"] = FolderLyricsHeading[i, 2];
								dataTable.Rows[rowIndex]["EndingHeading"] = FolderLyricsHeading[i, 3];
								dataTable.Rows[rowIndex]["BIUHeading"] = FolderHeadingFontBold[i, 0] + FolderHeadingFontItalic[i, 0] * 2 + FolderHeadingFontUnderline[i, 0] * 4 + FolderHeadingFontBold[i, 1] * 8 + FolderHeadingFontItalic[i, 1] * 16 + FolderHeadingFontUnderline[i, 1] * 32;
								dataTable.Rows[rowIndex]["HeadingSize"] = FolderHeadingPercentSize[i];
								dataTable.Rows[rowIndex]["HeadingOption"] = FolderHeadingOption[i];
								dataTable.Rows[rowIndex]["LineSpacing"] = ShowLineSpacing[i, 0];
								dataTable.Rows[rowIndex]["LineSpacing2"] = ShowLineSpacing[i, 1];
								dataTable.Rows[rowIndex]["BIU0"] = ShowFontBold[i, 0] + ShowFontItalic[i, 0] * 2 + ShowFontUnderline[i, 0] * 4 + ShowFontBold[i, 2] * 8 + ShowFontItalic[i, 2] * 16 + ShowFontUnderline[i, 2] * 32 + ShowFontRTL[i, 0] * 64;
								dataTable.Rows[rowIndex]["Size0"] = ShowFontSize[i, 0];
								dataTable.Rows[rowIndex]["FontName0"] = ShowFontName[i, 0];
								dataTable.Rows[rowIndex]["Vpos0"] = ShowFontVPosition[i, 0];
								dataTable.Rows[rowIndex]["BIU1"] = ShowFontBold[i, 1] + ShowFontItalic[i, 1] * 2 + ShowFontUnderline[i, 1] * 4 + ShowFontBold[i, 3] * 8 + ShowFontItalic[i, 3] * 16 + ShowFontUnderline[i, 3] * 32 + ShowFontRTL[i, 1] * 64;
								dataTable.Rows[rowIndex]["Size1"] = ShowFontSize[i, 1];
								dataTable.Rows[rowIndex]["FontName1"] = ShowFontName[i, 1];
								dataTable.Rows[rowIndex]["Vpos1"] = ShowFontVPosition[i, 1];
								dataTable.Rows[rowIndex]["LMargin"] = ShowLeftMargin[i];
								dataTable.Rows[rowIndex]["RMargin"] = ShowRightMargin[i];
								dataTable.Rows[rowIndex]["BMargin"] = ShowBottomMargin[i];
							}
						}
						DbController.UpdateTable(connection, text, dataTable);
					}

					if (dataTable != null)
						dataTable.Dispose();
				}
				catch (Exception e)
				{
					Console.WriteLine(e.Message);
					Console.WriteLine(e.StackTrace);
				}
			}
		}
#endif

		public static void SaveLicenceConfigSettings()
		{
			RegUtil.SaveRegValue("config", "licCCLI_no", LicAdmin_List[3, 1]);
			RegUtil.SaveRegValue("config", "lic4_no", LicAdmin_List[4, 1]);
			RegUtil.SaveRegValue("config", "lic5_no", LicAdmin_List[5, 1]);
			RegUtil.SaveRegValue("config", "lic6_no", LicAdmin_List[6, 1]);
			RegUtil.SaveRegValue("config", "lic7_no", LicAdmin_List[7, 1]);
			RegUtil.SaveRegValue("config", "lic8_no", LicAdmin_List[8, 1]);
			RegUtil.SaveRegValue("config", "licNoSym", LicAdminNoSymbol);
			RegUtil.SaveRegValue("config", "licEnforceDisplay", LicAdminEnforceDisplay ? 1 : 0);
		}

		public static void SaveOptionsData()
		{
			RegUtil.SaveRegValue("options", "PrinterSpaces", PB_PrinterSpaces);
			RegUtil.SaveRegValue("options", "UseSongNumbers", UseSongNumbers ? 1 : 0);
			RegUtil.SaveRegValue("options", "BibleMaxSelectVerses", HB_MaxVersesSelection);
			RegUtil.SaveRegValue("options", "BibleMaxAdhocVersesSelection", HB_MaxAdhocVersesSelection);
			RegUtil.SaveRegValue("options", "BibleShowVerses", 1);
			RegUtil.SaveRegValue("options", "PowerpointMaxFiles", PP_MaxFiles);
			RegUtil.SaveRegValue("options", "AutoTextOverflow", AutoTextOverflow ? 1 : 0);
			RegUtil.SaveRegValue("options", "UseLargestFontSize", UseLargestFontSize ? 1 : 0);
			RegUtil.SaveRegValue("options", "UsePowerpointTab", UsePowerpointTab ? 1 : 0);
			RegUtil.SaveRegValue("options", "NoPowerpointPanelOverlay", NoPowerpointPanelOverlay ? 1 : 0);
			RegUtil.SaveRegValue("options", "UseMediaTab", UseMediaTab ? 1 : 0);
			RegUtil.SaveRegValue("options", "ShowLyricsMonitorAlertBox", ShowLyricsMonitorAlertBox ? 1 : 0);
			RegUtil.SaveRegValue("options", "NoMediaPanelOverlay", NoMediaPanelOverlay ? 1 : 0);
			RegUtil.SaveRegValue("options", "RotateGap", ShowRotateGap);
			RegUtil.SaveRegValue("options", "AdvanceNextItem", AdvanceNextItem ? 1 : 0);
			RegUtil.SaveRegValue("options", "LineBetweenRegions", LineBetweenRegions ? 1 : 0);
			RegUtil.SaveRegValue("options", "GapItemOption", (int)GapItemOption);
			RegUtil.SaveRegValue("options", "GapItemLogoFile", GapItemLogoFile);
			RegUtil.SaveRegValue("options", "GapItemUseFade", GapItemUseFade ? 1 : 0);
			RegUtil.SaveRegValue("options", "WordWrapLeftAlignIndent", WordWrapLeftAlignIndent ? 1 : 0);
			RegUtil.SaveRegValue("options", "WordWrapIgnoreStartSpaces", WordWrapIgnoreStartSpaces);
			RegUtil.SaveRegValue("options", "AutoRotateOn", AutoRotateOn ? 1 : 0);
			RegUtil.SaveRegValue("options", "AutoRotateStyle", AutoRotateStyle);
			RegUtil.SaveRegValue("options", "NotationFontFactor", (int)(NotationFontFactor * 100.0));
			RegUtil.SaveRegValue("options", "ExternalListing", PowerpointListingStyle);
			RegUtil.SaveRegValue("options", "KeyBoardOption", KeyBoardOption);
            // Global Hook F7, F8 (panel black) 여부
            RegUtil.SaveRegValue("options", "GlobalHookKey_F7", GlobalHookKey_F7 ? 1 : 0);
            RegUtil.SaveRegValue("options", "GlobalHookKey_F8", GlobalHookKey_F8 ? 1 : 0);

            // Global Hook F9, F10 (panel black) 여부
            RegUtil.SaveRegValue("options", "GlobalHookKey_F9", GlobalHookKey_F9 ? 1 : 0);
            RegUtil.SaveRegValue("options", "GlobalHookKey_F10", GlobalHookKey_F10 ? 1 : 0);

            // Global Hook Arrow, CtrlArrow (panel black) 여부
            RegUtil.SaveRegValue("options", "GlobalHookKey_Arrow", GlobalHookKey_Arrow ? 1 : 0);
            RegUtil.SaveRegValue("options", "GlobalHookKey_CtrlArrow", GlobalHookKey_CtrlArrow ? 1 : 0);

            RegUtil.SaveRegValue("options", "OutputmonitorName", OutputMonitorName);
			RegUtil.SaveRegValue("options", "LyricsMonitorName", LyricsMonitorName);

			RegUtil.SaveRegValue("options", "PreviewAreaShowNotations", PreviewArea_ShowNotations ? 1 : 0);
			RegUtil.SaveRegValue("options", "PreviewAreaLineBetweenScreens", PreviewArea_LineBetweenScreens ? 1 : 0);
			RegUtil.SaveRegValue("options", "PreviewAreaFontSize", PreviewArea_FontSize);
			RegUtil.SaveRegValue("options", "ParentalAlertHeading", ParentalAlertHeading);
			RegUtil.SaveRegValue("options", "ParentalAlertDuration", ParentalAlertDuration);
			RegUtil.SaveRegValue("options", "ParentalAlertTextColour", ParentalAlertTextColour.ToArgb());
			RegUtil.SaveRegValue("options", "ParentalAlertBackColour", ParentalAlertBackColour.ToArgb());
			RegUtil.SaveRegValue("options", "ParentalAlertTextAlign", ParentalAlertTextAlign);
			RegUtil.SaveRegValue("options", "ParentalAlertVerticalAlign", ParentalAlertVerticalAlign);
			int num = 0;
			num = (ParentalAlertScroll ? 1 : 0) + (ParentalAlertFlash ? 1 : 0) * 2 + (ParentalAlertTransparent ? 1 : 0) * 4;
			RegUtil.SaveRegValue("options", "ParentalAlertStyle", num);
			RegUtil.SaveRegValue("options", "ParentalAlertFontName", ParentalAlertFontName);
			RegUtil.SaveRegValue("options", "ParentalAlertFontSize", ParentalAlertFontSize);
			RegUtil.SaveRegValue("options", "ParentalAlertFontFormat", ParentalAlertFontFormat);
			RegUtil.SaveRegValue("options", "MessageAlertDuration", MessageAlertDuration);
			RegUtil.SaveRegValue("options", "MessageAlertTextAlign", MessageAlertTextAlign);
			RegUtil.SaveRegValue("options", "MessageAlertVerticalAlign", MessageAlertVerticalAlign);
			RegUtil.SaveRegValue("options", "MessageAlertTextColour", MessageAlertTextColour.ToArgb());
			RegUtil.SaveRegValue("options", "MessageAlertBackColour", MessageAlertBackColour.ToArgb());
			num = (MessageAlertScroll ? 1 : 0) + (MessageAlertFlash ? 1 : 0) * 2 + (MessageAlertTransparent ? 1 : 0) * 4;
			RegUtil.SaveRegValue("options", "MessageAlertStyle", num);
			RegUtil.SaveRegValue("options", "MessageAlertFontName", MessageAlertFontName);
			RegUtil.SaveRegValue("options", "MessageAlertFontSize", MessageAlertFontSize);
			RegUtil.SaveRegValue("options", "MessageAlertFontFormat", MessageAlertFontFormat);
			RegUtil.SaveRegValue("options", "ReferenceAlertDuration", ReferenceAlertDuration);
			RegUtil.SaveRegValue("options", "ReferenceAlertTextAlign", ReferenceAlertTextAlign);
			RegUtil.SaveRegValue("options", "ReferenceAlertVerticalAlign", ReferenceAlertVerticalAlign);
			RegUtil.SaveRegValue("options", "ReferenceAlertTextColour", ReferenceAlertTextColour.ToArgb());
			RegUtil.SaveRegValue("options", "ReferenceAlertBackColour", ReferenceAlertBackColour.ToArgb());
			num = (ReferenceAlertScroll ? 1 : 0) + (ReferenceAlertFlash ? 1 : 0) * 2 + (ReferenceAlertTransparent ? 1 : 0) * 4;
			RegUtil.SaveRegValue("options", "ReferenceAlertStyle", num);
			RegUtil.SaveRegValue("options", "ReferenceAlertFontName", ReferenceAlertFontName);
			RegUtil.SaveRegValue("options", "ReferenceAlertFontSize", ReferenceAlertFontSize);
			RegUtil.SaveRegValue("options", "ReferenceAlertFontFormat", ReferenceAlertFontFormat);
			RegUtil.SaveRegValue("options", "ReferenceAlertUsePick", ReferenceAlertUsePick ? 1 : 0);
			RegUtil.SaveRegValue("options", "ReferenceAlertBlankIfPickNotFound", ReferenceAlertBlankIfPickNotFound ? 1 : 0);
			RegUtil.SaveRegValue("options", "ReferenceAlertSource", ReferenceAlertSource);
			RegUtil.SaveRegValue("options", "ReferenceAlertPickName", ReferenceAlertPickName);
			RegUtil.SaveRegValue("options", "ReferenceAlertPickSubstitute", ReferenceAlertPickSubstitute);
			RegUtil.SaveRegValue("options", "ReferenceAlertPickSeparator", ReferenceAlertPickSeparator);
			RegUtil.SaveRegValue("options", "FocusedBackColour", FocusedTextRegionColour.ToArgb());
			RegUtil.SaveRegValue("options", "SelectedSlideTextColour", TextRegionSlideTextColour.ToArgb());
			RegUtil.SaveRegValue("options", "SelectedSlideBackColour", TextRegionSlideBackColour.ToArgb());
			RegUtil.SaveRegValue("options", "UseFocusedBackColour", UseFocusedTextRegionColour ? 1 : 0);
			RegUtil.SaveRegValue("options", "JumpToA", JumpToA);
			RegUtil.SaveRegValue("options", "JumpToB", JumpToB);
			RegUtil.SaveRegValue("options", "JumpToC", JumpToC);
			RegUtil.SaveRegValue("options", "FindItemInTitle", FindItemInTitle ? 1 : 0);
			RegUtil.SaveRegValue("options", "FindItemInContents", FindItemInContents ? 1 : 0);
			RegUtil.SaveRegValue("options", "FindItemInSongNumber", FindItemInSongNumber ? 1 : 0);
			RegUtil.SaveRegValue("options", "FindItemInBookRef", FindItemInBookRef ? 1 : 0);
			RegUtil.SaveRegValue("options", "FindItemInUserRef", FindItemInUserRef ? 1 : 0);
			RegUtil.SaveRegValue("options", "FindItemInLicAdmin", FindItemInLicAdmin ? 1 : 0);
			RegUtil.SaveRegValue("options", "FindItemInWriter", FindItemInWriter ? 1 : 0);
			RegUtil.SaveRegValue("options", "FindItemInCopyright", FindItemInCopyright ? 1 : 0);
			RegUtil.SaveRegValue("options", "FindItemInTitle", FindItemInTitle ? 1 : 0);
			RegUtil.SaveRegValue("options", "FindItemUseDates", FindItemUseDates ? 1 : 0);
			RegUtil.SaveRegValue("options", "FindItemDateFrom", FindItemDateFrom.ToString());
			RegUtil.SaveRegValue("options", "FindItemDateTo", FindItemDateTo.ToString());
			RegUtil.SaveRegValue("options", "OutlineFontSizeThreshold", OutlineFontSizeThreshold);

            RegUtil.SaveRegValue("monitors", "AlwaysTryDualMonitor", DMAlwaysUseSecondaryMonitor ? 1 : 0);

            // daniel
            // 모니터 Wide 스크린 Mode
            RegUtil.SaveRegValue("monitors", "IsMonitorWide", isScreenWideMode ? 1: 0);
			RegUtil.SaveRegValue("monitors", "DualMonitorOption", DualMonitorSelectAutoOption);
			RegUtil.SaveRegValue("monitors", "DualMonitorOptionCustomLeft", DMOption1Left);
			RegUtil.SaveRegValue("monitors", "DualMonitorOptionCustomTop", DMOption1Top);
			RegUtil.SaveRegValue("monitors", "DualMonitorOptionCustomWidth", DMOption1Width);
			RegUtil.SaveRegValue("monitors", "DualMonitorOptionCustomAsSingle", DMOption1AsSingleMonitor ? 1 : 0);
			RegUtil.SaveRegValue("monitors", "DisableSreenSaver", DisableSreenSaver ? 1 : 0);
			RegUtil.SaveRegValue("monitors", "VideoSize", VideoSize);
			RegUtil.SaveRegValue("monitors", "VideoVAlign", VideoVAlign);
			RegUtil.SaveRegValue("monitors", "AlwaysTrySecondaryLyricsMonitor", LMAlwaysUseSecondaryMonitor ? 1 : 0);
			RegUtil.SaveRegValue("monitors", "LyricsMonitorOption", LMSelectAutoOption);
			RegUtil.SaveRegValue("monitors", "LyricsMonitorOptionCustomLeft", LMOption1Left);
			RegUtil.SaveRegValue("monitors", "LyricsMonitorOptionCustomTop", LMOption1Top);
			RegUtil.SaveRegValue("monitors", "LyricsMonitorOptionCustomWidth", LMOption1Width);
			RegUtil.SaveRegValue("monitors", "LyricsMonitorTextColour", LMTextColour.ToArgb());
			RegUtil.SaveRegValue("monitors", "LyricsMonitorHighlightColour", LMHighlightColour.ToArgb());
			RegUtil.SaveRegValue("monitors", "LyricsMonitorBackColour", LMBackColour.ToArgb());
			RegUtil.SaveRegValue("monitors", "LyricsMonitorShowNotations", LMShowNotations ? 1 : 0);
			RegUtil.SaveRegValue("monitors", "LyricsMonitorFontSize", LMMainFontSize);
			RegUtil.SaveRegValue("monitors", "LyricsMonitorNotationsFontSize", LMNotationsFontSize);
			RegUtil.SaveRegValue("options", "LyricsMonitorFontFormat", LMFontFormat);
			RegUtil.SaveRegValue("monitors", "LiveCamNumber", LiveCamNumber);
			RegUtil.SaveRegValue("monitors", "LiveCamVolume", LiveCamVolume);
			RegUtil.SaveRegValue("monitors", "LiveCamBalance", LiveCamBalance);
			RegUtil.SaveRegValue("monitors", "LiveCamWidescreen", LiveCamWidescreen ? 1 : 0);
			RegUtil.SaveRegValue("monitors", "LiveCamNoPanelOverlay", LiveCamNoPanelOverlay ? 1 : 0);
			RegUtil.SaveRegValue("monitors", "LiveCamMute", LiveCamMute ? 1 : 0);


        }

		public static bool ValidateDefaultFolders()
		{
			return ValidateDefaultFolders(0);
		}

		/// <summary>
		/// daniel
		/// 로딩 속도를 빠르게 하기 위해 수정 필요
		/// </summary>
		/// <param name="CheckColumns"></param>
		/// <returns></returns>
		public static bool ValidateDefaultFolders(int CheckColumns)
		{
			if (!ValidateDB(DatabaseType.Songs))
			{
				return false;
			}
			for (int i = 0; i < 41; i++)
			{
				ResetFolder(i, "", ConnectStringMainDB);
			}
			return true;
		}

		public static void LoadSongKeyCapoTiming(ref ComboBox SongCapo, ref ComboBox SongKey, ref ComboBox SongTiming)
		{
			SongCapo.Items.Clear();
			SongCapo.Items.Add("");
			SongCapo.Items.Add("Capo 0");
			SongCapo.Items.Add("Capo 1");
			SongCapo.Items.Add("Capo 2");
			SongCapo.Items.Add("Capo 3");
			SongCapo.Items.Add("Capo 4");
			SongCapo.Items.Add("Capo 5");
			SongCapo.Items.Add("Capo 6");
			SongCapo.Items.Add("Capo 7");
			SongCapo.Items.Add("Capo 8");
			SongCapo.Items.Add("Capo 9");
			SongCapo.Items.Add("Capo 10");
			SongCapo.Items.Add("Capo 11");
			SongKey.Items.Clear();
			SongKey.Items.Add("");
			SongKey.Items.Add("A");
			SongKey.Items.Add("B");
			SongKey.Items.Add("C");
			SongKey.Items.Add("D");
			SongKey.Items.Add("E");
			SongKey.Items.Add("F");
			SongKey.Items.Add("G");
			SongKey.Items.Add("Am");
			SongKey.Items.Add("Bm");
			SongKey.Items.Add("Cm");
			SongKey.Items.Add("Dm");
			SongKey.Items.Add("Em");
			SongKey.Items.Add("Fm");
			SongKey.Items.Add("Gm");
			SongKey.Items.Add("Ab");
			SongKey.Items.Add("Bb");
			SongKey.Items.Add("Db");
			SongKey.Items.Add("Eb");
			SongKey.Items.Add("F#");
			SongKey.Items.Add("Bbm");
			SongKey.Items.Add("C#m");
			SongKey.Items.Add("D#m");
			SongKey.Items.Add("F#m");
			SongKey.Items.Add("G#m");
			SongTiming.Items.Clear();
			SongTiming.Items.Add("");
			SongTiming.Items.Add("3/4");
			SongTiming.Items.Add("4/4");
		}

		public static void GenerateMusicKeysList()
		{
			MusicMajorKeys[0] = "G";
			MusicMajorKeys[1] = "Ab";
			MusicMajorKeys[2] = "A";
			MusicMajorKeys[3] = "Bb";
			MusicMajorKeys[4] = "B";
			MusicMajorKeys[5] = "C";
			MusicMajorKeys[6] = "Db";
			MusicMajorKeys[7] = "D";
			MusicMajorKeys[8] = "Eb";
			MusicMajorKeys[9] = "E";
			MusicMajorKeys[10] = "F";
			MusicMajorKeys[11] = "F#";
			MusicMajorKeysFlatSharp[0] = 1;
			MusicMajorKeysFlatSharp[1] = 0;
			MusicMajorKeysFlatSharp[2] = 1;
			MusicMajorKeysFlatSharp[3] = 0;
			MusicMajorKeysFlatSharp[4] = 1;
			MusicMajorKeysFlatSharp[5] = 1;
			MusicMajorKeysFlatSharp[6] = 0;
			MusicMajorKeysFlatSharp[7] = 1;
			MusicMajorKeysFlatSharp[8] = 0;
			MusicMajorKeysFlatSharp[9] = 1;
			MusicMajorKeysFlatSharp[10] = 0;
			MusicMajorKeysFlatSharp[11] = 1;
			MusicMinorKeys[0] = "Gm";
			MusicMinorKeys[1] = "G#m";
			MusicMinorKeys[2] = "Am";
			MusicMinorKeys[3] = "Bbm";
			MusicMinorKeys[4] = "Bm";
			MusicMinorKeys[5] = "Cm";
			MusicMinorKeys[6] = "C#m";
			MusicMinorKeys[7] = "Dm";
			MusicMinorKeys[8] = "D#m";
			MusicMinorKeys[9] = "Em";
			MusicMinorKeys[10] = "Fm";
			MusicMinorKeys[11] = "F#m";
			MusicMinorKeysFlatSharp[0] = 0;
			MusicMinorKeysFlatSharp[1] = 1;
			MusicMinorKeysFlatSharp[2] = 1;
			MusicMinorKeysFlatSharp[3] = 0;
			MusicMinorKeysFlatSharp[4] = 1;
			MusicMinorKeysFlatSharp[5] = 0;
			MusicMinorKeysFlatSharp[6] = 1;
			MusicMinorKeysFlatSharp[7] = 0;
			MusicMinorKeysFlatSharp[8] = 1;
			MusicMinorKeysFlatSharp[9] = 1;
			MusicMinorKeysFlatSharp[10] = 0;
			MusicMinorKeysFlatSharp[11] = 1;
			MusicMajorChords[0, 0] = "G";
			MusicMajorChords[1, 0] = "Ab";
			MusicMajorChords[2, 0] = "A";
			MusicMajorChords[3, 0] = "Bb";
			MusicMajorChords[4, 0] = "B";
			MusicMajorChords[5, 0] = "C";
			MusicMajorChords[6, 0] = "Db";
			MusicMajorChords[7, 0] = "D";
			MusicMajorChords[8, 0] = "Eb";
			MusicMajorChords[9, 0] = "E";
			MusicMajorChords[10, 0] = "F";
			MusicMajorChords[11, 0] = "Gb";
			MusicMajorChords[0, 1] = "G";
			MusicMajorChords[1, 1] = "G#";
			MusicMajorChords[2, 1] = "A";
			MusicMajorChords[3, 1] = "A#";
			MusicMajorChords[4, 1] = "B";
			MusicMajorChords[5, 1] = "C";
			MusicMajorChords[6, 1] = "C#";
			MusicMajorChords[7, 1] = "D";
			MusicMajorChords[8, 1] = "D#";
			MusicMajorChords[9, 1] = "E";
			MusicMajorChords[10, 1] = "F";
			MusicMajorChords[11, 1] = "F#";
			MusicMinorChords[0, 0] = "Gm";
			MusicMinorChords[1, 0] = "Abm";
			MusicMinorChords[2, 0] = "Am";
			MusicMinorChords[3, 0] = "Bbm";
			MusicMinorChords[4, 0] = "Bm";
			MusicMinorChords[5, 0] = "Cm";
			MusicMinorChords[6, 0] = "Dbm";
			MusicMinorChords[7, 0] = "Dm";
			MusicMinorChords[8, 0] = "Ebm";
			MusicMinorChords[9, 0] = "Em";
			MusicMinorChords[10, 0] = "Fm";
			MusicMinorChords[11, 0] = "Gbm";
			MusicMinorChords[0, 1] = "Gm";
			MusicMinorChords[1, 1] = "G#m";
			MusicMinorChords[2, 1] = "Am";
			MusicMinorChords[3, 1] = "A#m";
			MusicMinorChords[4, 1] = "Bm";
			MusicMinorChords[5, 1] = "Cm";
			MusicMinorChords[6, 1] = "C#m";
			MusicMinorChords[7, 1] = "Dm";
			MusicMinorChords[8, 1] = "D#m";
			MusicMinorChords[9, 1] = "Em";
			MusicMinorChords[10, 1] = "Fm";
			MusicMinorChords[11, 1] = "F#m";
		}

		public static void OldGenerateMusicKeysList()
		{
			MusicMajorKeys[0] = "G";
			MusicMajorKeys[1] = "Ab";
			MusicMajorKeys[2] = "A";
			MusicMajorKeys[3] = "Bb";
			MusicMajorKeys[4] = "B";
			MusicMajorKeys[5] = "C";
			MusicMajorKeys[6] = "Db";
			MusicMajorKeys[7] = "D";
			MusicMajorKeys[8] = "Eb";
			MusicMajorKeys[9] = "E";
			MusicMajorKeys[10] = "F";
			MusicMajorKeys[11] = "Gb";
			MusicMinorKeys[0] = "Gm";
			MusicMinorKeys[1] = "Abm";
			MusicMinorKeys[2] = "Am";
			MusicMinorKeys[3] = "Bbm";
			MusicMinorKeys[4] = "Bm";
			MusicMinorKeys[5] = "Cm";
			MusicMinorKeys[6] = "Dbm";
			MusicMinorKeys[7] = "Dm";
			MusicMinorKeys[8] = "Ebm";
			MusicMinorKeys[9] = "Em";
			MusicMinorKeys[10] = "Fm";
			MusicMinorKeys[11] = "Gbm";
			MusicMajorChords[0, 0] = "G";
			MusicMajorChords[1, 0] = "Ab";
			MusicMajorChords[2, 0] = "A";
			MusicMajorChords[3, 0] = "Bb";
			MusicMajorChords[4, 0] = "B";
			MusicMajorChords[5, 0] = "C";
			MusicMajorChords[6, 0] = "Db";
			MusicMajorChords[7, 0] = "D";
			MusicMajorChords[8, 0] = "Eb";
			MusicMajorChords[9, 0] = "E";
			MusicMajorChords[10, 0] = "F";
			MusicMajorChords[11, 0] = "Gb";
			MusicMajorChords[0, 1] = "G";
			MusicMajorChords[1, 1] = "G#";
			MusicMajorChords[2, 1] = "A";
			MusicMajorChords[3, 1] = "A#";
			MusicMajorChords[4, 1] = "B";
			MusicMajorChords[5, 1] = "C";
			MusicMajorChords[6, 1] = "C#";
			MusicMajorChords[7, 1] = "D";
			MusicMajorChords[8, 1] = "D#";
			MusicMajorChords[9, 1] = "E";
			MusicMajorChords[10, 1] = "F";
			MusicMajorChords[11, 1] = "F#";
			MusicMinorChords[0, 0] = "Gm";
			MusicMinorChords[1, 0] = "Abm";
			MusicMinorChords[2, 0] = "Am";
			MusicMinorChords[3, 0] = "Bbm";
			MusicMinorChords[4, 0] = "Bm";
			MusicMinorChords[5, 0] = "Cm";
			MusicMinorChords[6, 0] = "Dbm";
			MusicMinorChords[7, 0] = "Dm";
			MusicMinorChords[8, 0] = "Ebm";
			MusicMinorChords[9, 0] = "Em";
			MusicMinorChords[10, 0] = "Fm";
			MusicMinorChords[11, 0] = "Gbm";
			MusicMinorChords[0, 1] = "Gm";
			MusicMinorChords[1, 1] = "G#m";
			MusicMinorChords[2, 1] = "Am";
			MusicMinorChords[3, 1] = "A#m";
			MusicMinorChords[4, 1] = "Bm";
			MusicMinorChords[5, 1] = "Cm";
			MusicMinorChords[6, 1] = "C#m";
			MusicMinorChords[7, 1] = "Dm";
			MusicMinorChords[8, 1] = "D#m";
			MusicMinorChords[9, 1] = "Em";
			MusicMinorChords[10, 1] = "Fm";
			MusicMinorChords[11, 1] = "F#m";
		}

		//public static int GetDataInt(Recordset rs, string Fieldname)
		//{
		//	try
		//	{
		//		return ObjToInt(rs.Fields[Fieldname], Minus1IfBlank: false);
		//	}
		//	catch
		//	{
		//		return 0;
		//	}
		//}

		//public static int GetDataInt(DataRow rs, string Fieldname)
		//{
		//	try
		//	{
		//		return ObjToInt(rs[Fieldname], Minus1IfBlank: false);
		//	}
		//	catch
		//	{
		//		return 0;
		//	}
		//}

		//public static int GetDataInt(Recordset rs, string Fieldname, bool Minus1IfBlank)
		//{
		//	try
		//	{
		//		return ObjToInt(rs.Fields[Fieldname], Minus1IfBlank);
		//	}
		//	catch
		//	{
		//		return Minus1IfBlank ? (-1) : 0;
		//	}
		//}



		//public static double GetDataDouble(Recordset rs, string Fieldname)
		//{
		//	try
		//	{
		//		return ObjToDouble(rs.Fields[Fieldname]);
		//	}
		//	catch
		//	{
		//		return 0.0;
		//	}
		//}


		//public static string GetDataString(Recordset rs, string Fieldname)
		//{
		//	try
		//	{
		//		return ObjToString(rs.Fields[Fieldname]);
		//	}
		//	catch
		//	{
		//		return "";
		//	}
		//}

		public static void SingleArraySort(string[] InArray)
		{
			SingleArraySort(InArray, SortAscending: true);
		}

		public static void SingleArraySort(string[] InArray, bool SortAscending)
		{
			Array.Sort(InArray);
			if (!SortAscending)
			{
				Array.Reverse(InArray);
			}
		}

		public static int GetFolderNumber(string InName)
		{
			return GetFolderNumber(InName, ZeroIfInvalid: false);
		}

		public static int GetFolderNumber(string InName, bool ZeroIfInvalid)
		{
			for (int i = 0; i < 41; i++)
			{
				if (InName == FolderName[i])
				{
					return i;
				}
			}
			if (ZeroIfInvalid)
			{
				return 0;
			}
			return 1;
		}

		public static bool MusicFound(string MusicTitle1)
		{
			return MusicFound(MusicTitle1, "");
		}

		public static bool MusicFound(string MusicTitle1, string MusicTitle2)
		{
			return MusicFound(MusicTitle1, MusicTitle2, StoreDirPath: false);
		}

		public static bool MusicFound(string MusicTitle1, string MusicTitle2, bool StoreDirPath)
		{
			if (TotalMusicFiles < 0)
			{
				TotalMusicFiles = 0;
				MusicBuildContinue = true;
				MusicBuildStartTime = DateTime.Now;
				MusicBuildLapseTime = new TimeSpan(0L);
				BuildMusicFilesListArray(MediaDir, StoreDirPath);
			}
			if (TotalMusicFiles < 1)
			{
				return false;
			}
			for (int i = 0; i < TotalMusicFiles; i++)
			{
				if (MediaFilesList[i, 0].ToLower() == MusicTitle1.ToLower())
				{
					return true;
				}
				if (MusicTitle2 != "" && MediaFilesList[i, 0].ToLower() == MusicTitle2.ToLower())
				{
					return true;
				}
			}
			return false;
		}

		public static void BuildMusicFilesListArray(string FolderPath, bool StoreDirPath)
		{
			if (FolderPath == "" || !MusicBuildContinue || (!Directory.Exists(FolderPath) | (DataUtil.Mid(FolderPath, 1) == ":\\System Volume Information\\")))
			{
				return;
			}
			MusicBuildLapseTime = DateTime.Now.Subtract(MusicBuildStartTime);
			if (MusicBuildLapseTime.Seconds > 10)
			{
				MusicBuildContinue = false;
				return;
			}
			string[] array;
			for (int i = 0; i < TotalMediaFileExt; i++)
			{
				try
				{
					string[] files = Directory.GetFiles(FolderPath, "*" + MediaFileExtension[i, 0]);
					array = files;
					foreach (string text in array)
					{
						string InFileName = text;
						MediaFilesList[TotalMusicFiles, 0] = GetDisplayNameOnly(ref InFileName, UpdateByRef: false);
						MediaFilesList[TotalMusicFiles, 1] = MediaFileExtension[i, 0];
						int iLen = InFileName.Length - (MediaFilesList[TotalMusicFiles, 0].Length + MediaFilesList[TotalMusicFiles, 1].Length);
						MediaFilesList[TotalMusicFiles, 2] = DataUtil.Left(InFileName, iLen);
						TotalMusicFiles++;
					}
				}
				catch
				{
				}
			}
			// Daniel
			// C:\EasiSlides\Media\
			if (!FolderPath.EndsWith(@"\Media\"))
			{
				gf.MediaDir = @"C:\EasiSlides\Media\";
                FolderPath = gf.MediaDir;
			}

            string[] directories = Directory.GetDirectories(FolderPath);
			if (directories.Length > 0)
			{
				SingleArraySort(directories, SortAscending: true);
			}
			array = directories;
			foreach (string str in array)
			{
				BuildMusicFilesListArray(str + "\\", StoreDirPath);
			}
		}

		public static string GetMediaFileName(string MusicTitle1, string MusicTitle2)
		{
			string DirPath = "";
			string FileName = "";
			return GetMediaFileName(MusicTitle1, MusicTitle2, ref DirPath, ref FileName, StoreDirPath: false);
		}

		public static string GetMediaFileName(string MusicTitle1, string MusicTitle2, ref string DirPath)
		{
			string FileName = "";
			return GetMediaFileName(MusicTitle1, MusicTitle2, ref DirPath, ref FileName, StoreDirPath: false);
		}

		public static string GetMediaFileName(string MusicTitle1, string MusicTitle2, ref string DirPath, ref string FileName)
		{
			return GetMediaFileName(MusicTitle1, MusicTitle2, ref DirPath, ref FileName, StoreDirPath: false);
		}

		public static string GetMediaFileName(string MusicTitle1, string MusicTitle2, ref string DirPath, ref string FileName, bool StoreDirPath)
		{
			if (StoreDirPath & (TotalMusicFiles < 1))
			{
				return "";
			}
			string text = "";
			for (int i = 0; i <= 1; i++)
			{
				text = ((i == 0) ? MusicTitle1 : MusicTitle2);
				for (int j = 0; j < TotalMediaFileExt; j++)
				{
					if (StoreDirPath)
					{
						for (int k = 0; k < TotalMusicFiles; k++)
						{
							if (MediaFilesList[k, 0] == text && MediaFilesList[k, 1] == MediaFileExtension[j, 0])
							{
								DirPath = MediaFilesList[k, 2];
								FileName = MediaFilesList[k, 0] + MediaFilesList[k, 1];
								return DirPath + FileName;
							}
						}
					}
					else
					{
						string mediaFileNameFromDir = GetMediaFileNameFromDir(MediaDir, MediaFileExtension[j, 0], text, ref DirPath, ref FileName);
						if (mediaFileNameFromDir != "")
						{
							return mediaFileNameFromDir;
						}
					}
				}
			}
			return "";
		}

		public static string GetMediaFileNameFromDir(string FolderPath, string MusicExtension, string MusicTitle1)
		{
			string DirPath = "";
			string FileName = "";
			return GetMediaFileNameFromDir(FolderPath, MusicExtension, MusicTitle1, ref DirPath, ref FileName);
		}

		public static string GetMediaFileNameFromDir(string FolderPath, string MusicExtension, string MusicTitle1, ref string DirPath)
		{
			string FileName = "";
			return GetMediaFileNameFromDir(FolderPath, MusicExtension, MusicTitle1, ref DirPath, ref FileName);
		}

		public static string GetMediaFileNameFromDir(string FolderPath, string MusicExtension, string MusicTitle1, ref string DirPath, ref string FileName)
		{
			if ((FolderPath == "") | !Directory.Exists(FolderPath) | (DataUtil.Mid(FolderPath, 1) == ":\\System Volume Information\\"))
			{
				return "";
			}
			if (File.Exists(FolderPath + MusicTitle1 + MusicExtension))
			{
				DirPath = FolderPath;
				FileName = MusicTitle1 + MusicExtension;
				return DirPath + FileName;
			}
			string[] directories = Directory.GetDirectories(FolderPath);
			if (directories.Length > 0)
			{
				SingleArraySort(directories, SortAscending: true);
			}
			string[] array = directories;
			foreach (string str in array)
			{
				string mediaFileNameFromDir = GetMediaFileNameFromDir(str + "\\", MusicExtension, MusicTitle1, ref DirPath, ref FileName);
				if (mediaFileNameFromDir != "")
				{
					return mediaFileNameFromDir;
				}
			}
			return "";
		}

		public static int FormatImageContainers(ref ImageCanvas[] InCanvas, int MaxCanvas, int InImagesCount, int w, int h, int LeftOffset, int TopOffset)
		{
			int result = 0;
			for (int i = 0; i < MaxCanvas; i++)
			{
				InCanvas[i].PosWidth = w;
				InCanvas[i].PosHeight = h;
				InCanvas[i].PosLeft = (w + 5) * (i % ThumbImagesPerRow) + LeftOffset;
				InCanvas[i].PosTop = TopOffset + (h + 5) * (i / ThumbImagesPerRow);
				if (i < InImagesCount)
				{
					InCanvas[i].Visible = true;
					result = i + 1;
				}
				else
				{
					InCanvas[i].Visible = false;
				}
			}
			return result;
		}

		public static void ShowThumbImage(ref ImageCanvas InCanvas, int InCanvasIndex, int InWidth, int InHeight, ref string[] FileArray, string[] ImageArray, int FirstRef, int MaxDisplay, ToolTip InToolTip, string InPrefix, int CurSelectedSlide, bool ExternalPP)
		{
			if (InCanvasIndex >= MaxDisplay)
			{
				InToolTip.SetToolTip(InCanvas, "");
				FileArray[InCanvasIndex] = "";
				InCanvas.FileName = "";
				InCanvas.Visible = false;
				return;
			}
			int num = FirstRef + InCanvasIndex + 1;
			string text = "";
			int posWidth = InCanvas.PosWidth;
			int posHeight = InCanvas.PosHeight;
			string text2 = ImageArray[FirstRef + InCanvasIndex];
			if (InCanvas.PowerPoint)
			{
				text = num.ToString();
				InCanvas.SlideNumber = num;
			}
			else
			{
				text = GetDisplayNameOnly(ref ImageArray[FirstRef + InCanvasIndex], UpdateByRef: false, KeepExt: true);
				InCanvas.SlideNumber = 0;
				if (DataUtil.Right(text2, 4) == ".ppt")
				{
					text2 = ExtPPrefix + Path.GetFileNameWithoutExtension(text2) + ".jpg";
				}
				if (InCanvas.FileName == text2 && !ExternalPP)
				{
					int NewImageWidth = 0;
					int NewImageHeight = 0;
					CalcImageToFit(InCanvas.ImageRatio, InCanvas.PosWidth, InCanvas.PosHeight, ref NewImageWidth, ref NewImageHeight);
					InCanvas.Width = NewImageWidth;
					InCanvas.Height = NewImageHeight;
					InCanvas.Left = InCanvas.PosLeft + (int)((float)(InCanvas.PosWidth - NewImageWidth) / 2f);
					InCanvas.Top = InCanvas.PosTop + (int)((float)(InCanvas.PosHeight - NewImageHeight) / 2f);
					return;
				}
			}
			int NewImageWidth2 = 0;
			int NewImageHeight2 = 0;
			int num2 = 0;
			int num3 = 0;
			int StoredImageWidth = 0;
			int StoredImageHeight = 0;
			try
			{
				using (Image image = Image.FromFile(text2))
				{
					InCanvas.SetImageRatio(image.Width, image.Height);
					CalcImageToFit(InCanvas.ImageRatio, posWidth, posHeight, ref NewImageWidth2, ref NewImageHeight2, ComputeStoredImage: true, ref StoredImageWidth, ref StoredImageHeight);
					if (!((NewImageWidth2 <= 0) | (NewImageHeight2 <= 0)))
					{
						using (Image image2 = new Bitmap(StoredImageWidth, StoredImageHeight, PixelFormat.Format24bppRgb))
						{
							Graphics graphics = Graphics.FromImage(image2);
							Rectangle rect = new Rectangle(0, 0, StoredImageWidth, StoredImageHeight);
							if (InCanvas.PowerPoint)
							{
								Font font = new Font("Microsoft Sans Serif", (float)StoredImageWidth / 12f);
								int num4 = (int)((StoredImageWidth >= StoredImageHeight) ? ((float)StoredImageWidth * 0.04f) : ((float)StoredImageHeight * 0.04f));
								Rectangle rect2 = new Rectangle(num4, num4, StoredImageWidth - num4 * 2, StoredImageHeight - num4 * 2);
								graphics.DrawImage(image, rect2);
								string text3 = num.ToString();
								PointF pointF = new PointF(num4 + 2, num4 + 2);
								SizeF sizeF = graphics.MeasureString(text3, font);
								graphics.FillRectangle(new SolidBrush(BlackScreenColour), pointF.X + 3f, pointF.Y + 3f, sizeF.Width, sizeF.Height);
								graphics.FillRectangle(new SolidBrush(Color.Yellow), pointF.X, pointF.Y, sizeF.Width, sizeF.Height);
								graphics.DrawString(text3, font, new SolidBrush(BlackScreenColour), pointF.X, pointF.Y);
								if (CurSelectedSlide == num)
								{
									graphics.DrawRectangle(new Pen(new SolidBrush(Color.Red), (int)font.Size), rect);
								}
								else
								{
									graphics.DrawRectangle(new Pen(new SolidBrush(InCanvas.Parent.BackColor), (int)font.Size), rect);
								}
							}
							else if (ExternalPP)
							{
								Font font = new Font("Microsoft Sans Serif", (float)StoredImageWidth / 12f);
								int num4 = (int)((StoredImageWidth >= StoredImageHeight) ? ((float)StoredImageWidth * 0.04f) : ((float)StoredImageHeight * 0.04f));
								Rectangle rect2 = new Rectangle(num4, num4, StoredImageWidth - num4 * 2, StoredImageHeight - num4 * 2);
								graphics.DrawImage(image, rect2);
								if (CurSelectedSlide == num)
								{
									graphics.DrawRectangle(new Pen(new SolidBrush(Color.Red), (int)font.Size), rect);
								}
								else
								{
									graphics.DrawRectangle(new Pen(new SolidBrush(InCanvas.Parent.BackColor), (int)font.Size), rect);
								}
							}
							else
							{
								graphics.DrawImage(image, rect);
							}
							num2 = (posWidth - NewImageWidth2) / 2;
							num3 = (posHeight - NewImageHeight2) / 2;
							InCanvas.Left = InCanvas.PosLeft + num2;
							InCanvas.Top = InCanvas.PosTop + num3;
							InCanvas.Width = NewImageWidth2;
							InCanvas.Height = NewImageHeight2;
							InToolTip.SetToolTip(InCanvas, text);
							FileArray[InCanvasIndex] = ImageArray[FirstRef + InCanvasIndex];
							InCanvas.FileName = text2;
							InCanvas.image = image2;
							InCanvas.Visible = true;
							InCanvas.Invalidate();
							//graphics.Dispose();
						}
					}
				}
			}
			catch
			{
			}
		}

		public static float SetImageRatio(int InImageWidth, int InImageHeight)
		{
			try
			{
				return (float)InImageWidth / (float)InImageHeight;
			}
			catch
			{
				return 1f;
			}
		}

		public static void CalcImageToFit(float InImageRatio, int InContainerWidth, int InContainerHeight, ref int NewImageWidth, ref int NewImageHeight)
		{
			int StoredImageWidth = 0;
			int StoredImageHeight = 0;
			CalcImageToFit(InImageRatio, InContainerWidth, InContainerHeight, ref NewImageWidth, ref NewImageHeight, ComputeStoredImage: false, ref StoredImageWidth, ref StoredImageHeight);
		}

		public static void CalcImageToFit(float InImageRatio, int InContainerWidth, int InContainerHeight, ref int NewImageWidth, ref int NewImageHeight, bool ComputeStoredImage, ref int StoredImageWidth, ref int StoredImageHeight)
		{
			float num = (float)InContainerWidth / (float)InContainerHeight;
			if (num < InImageRatio)
			{
				NewImageWidth = InContainerWidth;
				NewImageHeight = (int)((float)NewImageWidth / InImageRatio);
				if (ComputeStoredImage)
				{
					StoredImageWidth = 200;
					StoredImageHeight = (int)((float)StoredImageWidth / InImageRatio);
				}
			}
			else
			{
				NewImageHeight = InContainerHeight;
				NewImageWidth = (int)((float)NewImageHeight * InImageRatio);
				if (ComputeStoredImage)
				{
					StoredImageHeight = 150;
					StoredImageWidth = (int)((float)StoredImageHeight * InImageRatio);
				}
			}
		}

		public static void LoadBibleVersions(ref TabControl InTab)
		{
			try
			{
				InTab.TabPages.Clear();
				InTab.ShowToolTips = true;
				HB_TotalVersions = 0;
				string fullSearchString = "select * from Biblefolder where NAME like \"*\" and displayorder >=0 order by displayorder, NAME";
#if ODBC
				fullSearchString = fullSearchString.Replace("\"*\"", "\"%\"");
#endif
#if OleDb

				DataTable dataTable = DbOleDbController.GetDataTable(ConnectStringBibleDB, fullSearchString);
#elif SQLite
				//Provider = Microsoft.ACE.OLEDB.12.0;
				//string SQLiteConnectStringBibleDB = ConnectStringBibleDB.Replace("Provider=Microsoft.ACE.OLEDB.12.0;", "");
				using DataTable dataTable = DbController.GetDataTable(ConnectStringBibleDB, fullSearchString);
#endif
				if (dataTable.Rows.Count>0)
				{
					//recordSet.MoveFirst();
					//while (!recordSet.EOF)
					foreach(DataRow dr in dataTable.Rows)
					{
						if (HB_TotalVersions <= 250)
						{
							TabPage tabPage = new TabPage();
							InTab.Controls.Add(tabPage);
							HB_Versions[HB_TotalVersions, 1] = DataUtil.GetDataString(dr, "NAME");
							HB_Versions[HB_TotalVersions, 4] = BibleDir + DataUtil.GetDataString(dr, "FILENAME");
							HB_Versions[HB_TotalVersions, 2] = DataUtil.GetDataString(dr, "DESCRIPTION");
							HB_Versions[HB_TotalVersions, 3] = DataUtil.GetDataString(dr, "COPYRIGHT");
							HB_Versions[HB_TotalVersions, 5] = "1";
							HB_Versions[HB_TotalVersions, 5] = DataUtil.GetDataString(dr, "SONGFOLDER");
							HB_Versions[HB_TotalVersions, 6] = "80";
							HB_Versions[HB_TotalVersions, 6] = DataUtil.GetDataString(dr, "SIZE");
							tabPage.Text = HB_Versions[HB_TotalVersions, 1];
							tabPage.ToolTipText = HB_Versions[HB_TotalVersions, 2];
							if (DataUtil.StringToInt(HB_Versions[HB_TotalVersions, 5]) < 1)
							{
								HB_Versions[HB_TotalVersions, 5] = "1";
							}
							if ((DataUtil.StringToInt(HB_Versions[HB_TotalVersions, 6]) < 5) | (DataUtil.StringToInt(HB_Versions[HB_TotalVersions, 6]) > 200))
							{
								HB_Versions[HB_TotalVersions, 6] = "80";
							}
							HB_TotalVersions++;
						}						
					}
				}
				if (HB_TotalVersions > 0)
				{
					InTab.Enabled = true;
				}
				else
				{
					TabPage tabPage = new TabPage();
					tabPage.Text = "No Bible";
					InTab.Controls.Add(tabPage);
					InTab.Enabled = false;
				}
			}
			catch
			{
			}
		}

		public static bool LoadBibleBooksList(TabControl InTab, ref ComboBox InChapterList, bool ShowSearchResultsLine, RichTextBox OutputTextBox)
		{
			HB_CurVersionTabIndex = InTab.SelectedIndex;
			if (LoadBibleBooksList(HB_CurVersionTabIndex, ref InChapterList, ShowAllBooksLine: false, ShowSearchResultsLine))
			{
				return true;
			}

			if (OutputTextBox != null)
			{
				OutputTextBox.Text = "";
			}
			return false;
		}

		/// <summary>
		/// daniel
		/// 성경책을 읽어 오는 부분
		/// </summary>
		/// <param name="InBibleVersion"></param>
		/// <param name="InChapterList"></param>
		/// <param name="ShowAllBooksLine"></param>
		/// <param name="ShowSearchResultsLine"></param>
		/// <returns></returns>
		public static bool LoadBibleBooksList(int InBibleVersion, ref ComboBox InChapterList, bool ShowAllBooksLine, bool ShowSearchResultsLine)
		{

			HBFilename = GetBibleFileName(InBibleVersion);
			int num = (InChapterList.SelectedIndex >= 0) ? InChapterList.SelectedIndex : 0;
			InChapterList.Items.Clear();
			if (ShowAllBooksLine)
			{
				InChapterList.Items.Add("All Books");
			}

			int recordSetRowsCount = 0;

			string fullSearchString = "select * from Bible where book=0 and chapter=10 and (verse >0 and verse <=" + 66 + ") order by verse";

#if OleDb
			using (OleDbConnection connection = DbConnectionController.GetOleDbConnection(ConnectStringDef + HBFilename))
			{
				DataTable datatable = DbOleDbController.getDataTable(connection, fullSearchString);
				recordSetRowsCount = datatable.Rows.Count;
				if (recordSetRowsCount > 0)
				{
					//recordSet.MoveFirst();
					//while (!recordSet.EOF)
					InChapterList.BeginUpdate();
					foreach (DataRow dr in datatable.Rows)
					{
						HBVersionBookName[DataUtil.GetDataInt(dr, "verse")] = DataUtil.GetDataString(dr, "bibletext");
						InChapterList.Items.Add(DataUtil.GetDataString(dr, "bibletext"));
						//recordSet.MoveNext();
					}
					InChapterList.EndUpdate();
				}
			}

#elif SQLite
			using (DbConnection connection = DbController.GetDbConnection(ConnectSQLiteDef + HBFilename))
			{
				DataTable datatable = DbController.GetDataTable(connection, fullSearchString);
				recordSetRowsCount = datatable.Rows.Count;
				if (recordSetRowsCount > 0)
				{
					//recordSet.MoveFirst();
					//while (!recordSet.EOF)
					InChapterList.BeginUpdate();
					foreach (DataRow dr in datatable.Rows)
					{
						HBVersionBookName[DataUtil.GetDataInt(dr, "verse")] = DataUtil.GetDataString(dr, "bibletext");
						InChapterList.Items.Add(DataUtil.GetDataString(dr, "bibletext"));
						//recordSet.MoveNext();
					}
					InChapterList.EndUpdate();
				}
			}
#endif

			/// daniel
			/// OleDbConnection 연결이 꼬이는 것을 방지 하기 위해
			/// 아래 로직 분리
			/// InChapterList.SelectedIndex가 호출 될 경우 LoadBiblePassages가 호출 됨
			if (recordSetRowsCount > 0)
			{
				if (ShowSearchResultsLine)
				{
					InChapterList.Items.Add("Search Results:");
					InChapterList.SelectedIndex = InChapterList.Items.Count - 1;
				}
				else
				{
					InChapterList.SelectedIndex = ((num < 66) ? num : 0);
				}
				return true;
			}
			
			return false;
		}

		public static string GetBibleFileName(int SelectedVersion)
		{
			string fullSearchString = "select * from Biblefolder where DISPLAYORDER = " + SelectedVersion;
#if OleDb
			DataTable datatable = DbOleDbController.GetDataTable(ConnectStringBibleDB, fullSearchString);
#elif SQLite
			using DataTable datatable = DbController.GetDataTable(ConnectStringBibleDB, fullSearchString);
#endif
			if (datatable.Rows.Count>0)
			{
				//recordSet.MoveFirst();
				if (HB_TotalVersions <= 250)
				{
					return BibleDir + DataUtil.GetDataString(datatable.Rows[0], "filename");
				}
			}
			return "";
		}

		public static bool LoadBiblePassagesFromTabIndex(int InBiBleVersion, ComboBox InBookList, ref RichTextBox InTextBox, bool InShowVerses)
		{
			InTextBox.Clear();
			StringBuilder InTextString = new StringBuilder();
			bool result = LoadBiblePassages(InBiBleVersion, InBookList.SelectedIndex + 1, ref InTextString, InShowVerses, DoCompleteBook: true, TrackOutput: true, 1, 1, 0, 0, AdHocListing: false, NoneSpacingText: false, ShowRepeatingChapters: true, ShowFormatTags: false);
			InTextBox.Text = InTextString.ToString();
			return result;
		}

		public static bool LoadBiblePassages(int InBiBleVersion, int InBookNumber, ref StringBuilder InTextString, bool InShowVerses, bool DoCompleteBook, bool TrackOutput, int ChapterStart, int VerseStart, int ChapterEnd, int VerseEnd, bool AdHocListing, bool NoneSpacingText, bool ShowRepeatingChapters, bool ShowFormatTags)
		{
			try
			{
				string connectString = ConnectStringDef + HB_Versions[InBiBleVersion, 4];
				string text = "";
				string text2 = "";
#if OleDb
				using (OleDbConnection connection = DbConnectionController.GetOleDbConnection(connectString))
#elif SQLite
				using (DbConnection connection = DbController.GetDbConnection(connectString))
#endif
				{
					
					DataTable recordset = null;
					if (AdHocListing)
					{
						text2 = "select * from Bible where book=0 and chapter=10 and verse=" + InBookNumber;
#if OleDb
						recordset = DbOleDbController.getDataTable(connection, text2);
#elif SQLite
						recordset = DbController.GetDataTable(connection, text2);
#endif
						if (recordset.Rows.Count > 0)
						{
							//recordset.MoveFirst();
							text = DataUtil.GetDataString(recordset.Rows[0], "bibletext");
						}
					}
					if (DoCompleteBook)
					{
						text2 = "select * from Bible where book=" + InBookNumber + " order by chapter, verse";
					}
					else
					{
						string text3 = "";
						string text4 = "";
						if (ChapterEnd > 0)
						{
							text3 = " and chapter <=" + ChapterEnd + " ";
						}
						if (VerseEnd > 0)
						{
							text4 = " and verse <=" + VerseEnd + " ";
						}
						text2 = "select * from Bible where book=" + InBookNumber + " and chapter >=" + ChapterStart + " " + text3 + " and verse >=" + VerseStart + " " + text4 + " order by book, chapter, verse";
					}
					StringBuilder stringBuilder = new StringBuilder();
					string text5 = "";
					string text6 = "";
					char c = '\u3000';
					int num = 0;
					int num2 = 0;
					string str = (ShowFormatTags && !AdHocListing) ? '\u0098'.ToString() : " ";
#if OleDb
					recordset = DbOleDbController.getDataTable(connection, text2);
#elif SQLite
					recordset = DbController.GetDataTable(connection, text2);
#endif
					if (recordset.Rows.Count > 0)
					{
						//recordset.MoveFirst();
						//while (!recordset.EOF)
						foreach (DataRow dr in recordset.Rows)
						{
							num2++;
							int dataInt = DataUtil.GetDataInt(dr, "chapter");
							int dataInt2 = DataUtil.GetDataInt(dr, "verse");
							text5 = ((ShowRepeatingChapters || dataInt != num) ? (dataInt + ":") : "") + dataInt2 + str;
							num = dataInt;
							text6 = (InShowVerses ? (DataUtil.GetDataString(dr, "bibletext") ?? "") : "");
							if (TrackOutput)
							{
								HB_VersesLocation[num2, 0] = InBiBleVersion;
								HB_VersesLocation[num2, 1] = InBookNumber;
								HB_VersesLocation[num2, 2] = dataInt;
								HB_VersesLocation[num2, 3] = dataInt2;
								HB_VersesLocation[num2, 4] = stringBuilder.Length;
								stringBuilder.Append(text + text5 + " " + text6 + "\n\n");
								HB_VersesLocation[num2, 5] = stringBuilder.Length + 1 - HB_VersesLocation[num2, 4];
							}
							else if (AdHocListing)
							{
								stringBuilder.Append(text6 + " (" + text + text5 + ")\n");
							}
							else
							{
								stringBuilder.Append(text5 + (NoneSpacingText ? c.ToString() : " ") + text6 + "\n");
							}
							//recordset.MoveNext();
						}
					}
					if (TrackOutput)
					{
						HB_VersesLocation[0, 0] = num2;
					}
					InTextString.Append(DataUtil.TrimEnd(stringBuilder.ToString()));
					if (recordset != null)
					{
						recordset = null;
					}
				}
				return true;
			}
			catch
			{
				return false;
			}
		}

		public static bool RefreshBiblePassages(int InBibleVersion, ComboBox InBookList, ref RichTextBox InTextContainer)
		{
			return RefreshBiblePassages(InBibleVersion, InBookList, ref InTextContainer, InShowVerses: true);
		}

		public static bool RefreshBiblePassages(int InBibleVersion, ComboBox InBookList, ref RichTextBox InTextContainer, bool InShowVerses)
		{
			HBConvertVersion(InBibleVersion);
			StringBuilder stringBuilder = new StringBuilder();
			try
			{
				string connectString = ConnectStringDef + HB_Versions[InBibleVersion, 4];
				string text = "";
#if OleDb
				using (OleDbConnection connection = DbConnectionController.GetOleDbConnection(connectString))
#elif SQLite
				using (DbConnection connection = DbController.GetDbConnection(connectString))
#endif
				{
					
					DataTable recordset = null;
					for (int i = 1; i <= HB_VersesLocation[0, 0]; i++)
					{
						try
						{
							text = "select * from bible where book =" + HB_VersesLocation[i, 1] + " and chapter=" + HB_VersesLocation[i, 2] + " and verse=" + HB_VersesLocation[i, 3] + " order by book, chapter, verse";
#if OleDb
							recordset = DbOleDbController.getDataTable(connection, text);
#elif SQLite
							recordset = DbController.GetDataTable(connection, text);			
#endif
							if (recordset.Rows.Count>0)
							{
								HB_VersesLocation[i, 4] = stringBuilder.Length;
								stringBuilder.Append(string.Concat(InBookList.Items[HB_VersesLocation[i, 1] - 1], " ", HB_VersesLocation[i, 2], ":", HB_VersesLocation[i, 3], " ", InShowVerses ? DataUtil.GetDataString(recordset.Rows[0], "bibletext") : "", "\n\n"));
								HB_VersesLocation[i, 5] = stringBuilder.Length + 1 - HB_VersesLocation[i, 4];
								recordset.Dispose();
							}
						}
						catch
						{
							HB_VersesLocation[i, 4] = stringBuilder.Length;
							HB_VersesLocation[i, 5] = 0;
						}
					}
					//InTextContainer.Text = DataUtil.TrimEnd(stringBuilder.ToString());
					if (recordset != null)
					{
						recordset = null;
					}
				}
				return true;
			}
			catch
			{
				InTextContainer.Text = DataUtil.TrimEnd(stringBuilder.ToString());
				return false;
			}
		}

		public static bool HBConvertVersion(int InBibleVersion)
		{
			if (InBibleVersion < HB_TotalVersions - 1)
			{
				return false;
			}
			for (int i = 1; i <= HB_VersesLocation[0, 0]; i++)
			{
				HB_VersesLocation[i, 0] = InBibleVersion;
			}
			return true;
		}

		public static bool LookUpBibleName(string FileName, ref string Name, ref string Description, ref string Copyright, ref string Info)
		{
			Name = "";
			Description = "";
			Copyright = "";
			Info = "";
			string connectString = ConnectStringDef + FileName;
			try
			{
#if OleDb
				DataTable dataTable = DbOleDbController.GetDataTable(connectString, "select * from Bible where book=0 and chapter=0 and verse=0");
				Description = DataUtil.GetDataString(dataTable.Rows[0], "bibletext");
				dataTable = DbOleDbController.GetDataTable(connectString, "select * from Bible where book=0 and chapter=0 and verse=1");
				Name = DataUtil.GetDataString(dataTable.Rows[0], "bibletext");
				dataTable = DbOleDbController.GetDataTable(connectString, "select * from Bible where book=0 and chapter=0 and verse=3");
				Copyright = DataUtil.GetDataString(dataTable.Rows[0], "bibletext");
				dataTable = DbOleDbController.GetDataTable(connectString, "select * from Bible where book=0 and chapter=0 and verse=4");
				Info = DataUtil.GetDataString(dataTable.Rows[0], "bibletext");
#elif SQLite
				DataTable dataTable = DbController.GetDataTable(connectString, "select * from Bible where book=0 and chapter=0 and verse=0");
				Description = DataUtil.GetDataString(dataTable.Rows[0], "bibletext");
				if (dataTable != null) dataTable.Dispose();

				dataTable = DbController.GetDataTable(connectString, "select * from Bible where book=0 and chapter=0 and verse=1");
				Name = DataUtil.GetDataString(dataTable.Rows[0], "bibletext");
				if (dataTable != null) dataTable.Dispose();
				dataTable = DbController.GetDataTable(connectString, "select * from Bible where book=0 and chapter=0 and verse=3");
				Copyright = DataUtil.GetDataString(dataTable.Rows[0], "bibletext");
				if (dataTable != null) dataTable.Dispose();
				dataTable = DbController.GetDataTable(connectString, "select * from Bible where book=0 and chapter=0 and verse=4");
				Info = DataUtil.GetDataString(dataTable.Rows[0], "bibletext");
				if (dataTable != null) dataTable.Dispose();
#endif

			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				Console.WriteLine(ex.StackTrace);
			}
			if (Name == "" || Description == "")
			{
				return false;
			}
			return true;
		}

		public static string LookUpBookName(int InBibleVersion, int InBookNumber)
		{
			try
			{
				string connectString = ConnectStringDef + HB_Versions[InBibleVersion, 4];
				string fullSearchString = "select * from Bible where book=0 and chapter=10 and verse=" + InBookNumber;
#if OleDb
				DataTable datatable = DbOleDbController.getDataTable(connectString, fullSearchString);
#elif SQLite
				using DataTable datatable = DbController.GetDataTable(connectString, fullSearchString);
#endif
				if (datatable.Rows.Count>0)
				{
					return DataUtil.GetDataString(datatable.Rows[0], "bibletext");
				}
			}
			catch
			{
				return "";
			}
			return "";
		}

		public void FormatText(ref SongSettings InItem, Color PanelBackColour, int PanelBackColorAsScreen, Color PanelTextColor, int PaneltextColourAsRegion1)
		{
			FormatText(ref InItem, PanelBackColour, PanelBackColorAsScreen, PanelTextColor, PaneltextColourAsRegion1, UseDefault: true);
		}

		public static void FormatText(ref SongSettings InItem, Color PanelBackColour, int PanelBackColorAsScreen, Color PanelTextColor, int PaneltextColourAsRegion1, bool UseDefault)
		{
			Color[] array = new Color[2];
			int[] array2 = new int[2];
			int[] array3 = new int[4];
			int[] array4 = new int[4];
			int[] array5 = new int[4];
			int[] array6 = new int[2];
			string[] array7 = new string[2];
			int[] array8 = new int[2];
			int[] array9 = new int[2];
			int buffer_LS_Width = Buffer_LS_Width;
			int buffer_LS_Height = Buffer_LS_Height;
			double num = PreviewSampleFactor;
			int num2 = 0;
			int num3 = 0;
			int num4 = 0;
			int num5 = 0;
			if (UseDefault)
			{
				array[0] = ShowFontColour[0];
				array[1] = ShowFontColour[1];
				array2[0] = ShowFontAlign[0, 0];
				array2[1] = ShowFontAlign[0, 1];
				array3[0] = ShowFontBold[InItem.FolderNo, 0];
				array3[1] = ShowFontBold[InItem.FolderNo, 1];
				array4[0] = ShowFontItalic[InItem.FolderNo, 0];
				array4[1] = ShowFontItalic[InItem.FolderNo, 1];
				array4[2] = ShowFontItalic[InItem.FolderNo, 2];
				array4[3] = ShowFontItalic[InItem.FolderNo, 3];
				array5[0] = ShowFontUnderline[InItem.FolderNo, 0];
				array5[1] = ShowFontUnderline[InItem.FolderNo, 1];
				array6[0] = ShowFontRTL[InItem.FolderNo, 0];
				array6[1] = ShowFontRTL[InItem.FolderNo, 1];
				array7[0] = ShowFontName[InItem.FolderNo, 0];
				array7[1] = ShowFontName[InItem.FolderNo, 1];
				if (InItem.Type == "B")
				{
					array8[0] = InItem.Format.ShowFontSize[0];
					array8[1] = InItem.Format.ShowFontSize[1];
				}
				else
				{
					array8[0] = ShowFontSize[InItem.FolderNo, 0];
					array8[1] = ShowFontSize[InItem.FolderNo, 1];
				}
				array9[0] = ShowFontVPosition[InItem.FolderNo, 0];
				array9[1] = ShowFontVPosition[InItem.FolderNo, 1];
				num2 = ShowLeftMargin[InItem.FolderNo];
				num3 = ShowRightMargin[InItem.FolderNo];
				num4 = ShowBottomMargin[InItem.FolderNo];
				num5 = ((ShowSongHeadingsAlign == 1) ? array2[1] : ((ShowSongHeadingsAlign == 2) ? 1 : ((ShowSongHeadingsAlign == 3) ? 2 : ((ShowSongHeadingsAlign != 4) ? array2[0] : 3))));
			}
			else if (!UseDefault)
			{
				array[0] = InItem.Format.ShowFontColour[0];
				array[1] = InItem.Format.ShowFontColour[1];
				array2[0] = InItem.Format.ShowFontAlign[0];
				array2[1] = InItem.Format.ShowFontAlign[1];
				array3[0] = InItem.Format.ShowFontBold[0];
				array3[1] = InItem.Format.ShowFontBold[1];
				array4[0] = InItem.Format.ShowFontItalic[0];
				array4[1] = InItem.Format.ShowFontItalic[1];
				array4[2] = InItem.Format.ShowFontItalic[2];
				array4[3] = InItem.Format.ShowFontItalic[3];
				array5[0] = InItem.Format.ShowFontUnderline[0];
				array5[1] = InItem.Format.ShowFontUnderline[1];
				array7[0] = InItem.Format.ShowFontName[0];
				array7[1] = InItem.Format.ShowFontName[1];
				array8[0] = InItem.Format.ShowFontSize[0];
				array8[1] = InItem.Format.ShowFontSize[1];
				array9[0] = InItem.Format.ShowFontVPosition[0];
				array9[1] = InItem.Format.ShowFontVPosition[1];
				num2 = InItem.Format.ShowLeftMargin;
				num3 = InItem.Format.ShowRightMargin;
				num4 = InItem.Format.ShowBottomMargin;
				num5 = ((InItem.Format.ShowSongHeadingsAlign == 1) ? array2[1] : ((InItem.Format.ShowSongHeadingsAlign == 2) ? 1 : ((InItem.Format.ShowSongHeadingsAlign == 3) ? 2 : ((InItem.Format.ShowSongHeadingsAlign != 4) ? array2[0] : 3))));
			}
			int num6 = FolderHeadingOption[InItem.FolderNo];
			bool flag = (FolderHeadingFontBold[InItem.FolderNo, 0] > 0) ? true : false;
			bool flag2 = (FolderHeadingFontItalic[InItem.FolderNo, 0] > 0) ? true : false;
			bool flag3 = (FolderHeadingFontUnderline[InItem.FolderNo, 0] > 0) ? true : false;
			bool flag4 = (FolderHeadingFontItalic[InItem.FolderNo, 1] > 0) ? true : false;
			int num7 = num2 * buffer_LS_Width / 100;
			int num8 = buffer_LS_Width - (num7 + num3 * buffer_LS_Width / 100);
			int num9 = buffer_LS_Height - (int)(TopBorderFactor * (double)buffer_LS_Height + (BottomBorderFactor + 0.029999999329447746) * (double)buffer_LS_Height + (double)(num4 * buffer_LS_Height / 100));
			ShowTopBorderSize = (int)((double)buffer_LS_Height * TopBorderFactor);
			
			Image image = new Bitmap(num8, 1, PixelFormat.Format24bppRgb);
			Graphics graphics = Graphics.FromImage(image);
			SizeF layoutArea = new SizeF(num8, 32000f);
			int i;
			FontStyle fontStyle;
			int num11;
			double num12;
			for (i = 0; i <= 2; i++)
			{
				int num10 = (i < 2) ? i : 0;
				fontStyle = FontStyle.Regular;
				FontStyle fontStyle2 = FontStyle.Regular;
				if (i == 2 && num6 != 0)
				{
					if (num6 == 2)
					{
						if (flag)
						{
							fontStyle |= FontStyle.Bold;
							fontStyle2 |= FontStyle.Bold;
						}
						if (flag2)
						{
							fontStyle |= FontStyle.Italic;
						}
						if (flag4)
						{
							fontStyle2 |= FontStyle.Italic;
						}
						if (flag3)
						{
							fontStyle |= FontStyle.Underline;
							fontStyle2 |= FontStyle.Underline;
						}
					}
					else
					{
						if ((array3[num10] > 0) | flag)
						{
							fontStyle |= FontStyle.Bold;
							fontStyle2 |= FontStyle.Bold;
						}
						if ((array4[num10] > 0) | flag2)
						{
							fontStyle |= FontStyle.Italic;
						}
						if ((array4[num10] > 0) | flag4)
						{
							fontStyle2 |= FontStyle.Italic;
						}
						if ((array5[num10] > 0) | flag3)
						{
							fontStyle |= FontStyle.Underline;
							fontStyle2 |= FontStyle.Underline;
						}
					}
				}
				else
				{
					if (array3[num10] > 0)
					{
						fontStyle |= FontStyle.Bold;
						fontStyle2 |= FontStyle.Bold;
					}
					if (array4[num10] > 0)
					{
						fontStyle |= FontStyle.Italic;
					}
					if (array5[num10] > 0)
					{
						fontStyle |= FontStyle.Underline;
						fontStyle2 |= FontStyle.Bold;
					}
					if (array4[num10] > 0 || array4[num10 + 2] > 0)
					{
						fontStyle2 |= FontStyle.Italic;
					}
				}
				InItem.Lyrics[i].ForeColour = array[num10];
				switch ((i == 2) ? num5 : array2[num10])
				{
					case 3:
						InItem.Lyrics[i].TextAlign = StringAlignment.Far;
						break;
					case 1:
						InItem.Lyrics[i].TextAlign = StringAlignment.Near;
						break;
					default:
						InItem.Lyrics[i].TextAlign = StringAlignment.Center;
						break;
				}
				num11 = DisplayFontSize(array8[num10], buffer_LS_Width, i, InItem.FolderNo);
				num12 = (double)num11 / num;
				try
				{
					InItem.Lyrics[i].FS_Font = new Font(array7[num10], num11, fontStyle);
					InItem.Lyrics[i].FS_ChorusFont = new Font(array7[num10], num11, fontStyle2);
				}
				catch
				{
					InItem.Lyrics[i].FS_Font = new Font("Microsoft Sans Serif", num11, fontStyle);
					InItem.Lyrics[i].FS_ChorusFont = new Font("Microsoft Sans Serif", num11, fontStyle2);
				}
				try
				{
					InItem.Lyrics[i].Font = new Font(array7[num10], (float)((num12 > 0.0) ? num12 : 1.0), fontStyle);
					InItem.Lyrics[i].ChorusFont = new Font(array7[num10], (float)((num12 > 0.0) ? num12 : 1.0), fontStyle2);
				}
				catch
				{
					InItem.Lyrics[i].Font = new Font("Microsoft Sans Serif", (float)((num12 > 0.0) ? num12 : 1.0), fontStyle);
					InItem.Lyrics[i].ChorusFont = new Font("Microsoft Sans Serif", (float)((num12 > 0.0) ? num12 : 1.0), fontStyle2);
				}
			}
			double num13 = (double)graphics.MeasureString("A", InItem.Lyrics[2].FS_Font, layoutArea).Height * 1.1;
			if (num13 > (double)num9 / 4.0)
			{
				num13 = (double)num9 / 4.0;
			}
			for (i = 0; i <= 2; i++)
			{
				InItem.Lyrics[i].Visible = false;
				InItem.Lyrics[i].FS_Width = num8;
				InItem.Lyrics[i].FS_Left = num7;
				InItem.Lyrics[i].FS_Top = SetLyricsTopPos(array9[(i < 2) ? i : 0], buffer_LS_Height) + ((i == 0) ? ((int)num13) : 0);
				if (i == 2)
				{
					InItem.Lyrics[i].FS_Height = (int)num13;
					InItem.Lyrics[i].FS_Height_R2Bound = InItem.Lyrics[i].FS_Height;
					InItem.Lyrics[0].FS_Height_R2Bound = InItem.Lyrics[1].FS_Top - InItem.Lyrics[0].FS_Top;
					InItem.Lyrics[0].Height_R2Bound = Convert.ToInt32((double)InItem.Lyrics[0].FS_Height_R2Bound / num);
				}
				else
				{
					InItem.Lyrics[i].FS_Height = num9 - InItem.Lyrics[i].FS_Top;
					InItem.Lyrics[i].FS_Height_R2Bound = InItem.Lyrics[i].FS_Height;
				}
				InItem.Lyrics[i].Width = Convert.ToInt32((double)InItem.Lyrics[i].FS_Width / num);
				InItem.Lyrics[i].Left = Convert.ToInt32((double)InItem.Lyrics[i].FS_Left / num);
				InItem.Lyrics[i].Top = Convert.ToInt32((double)InItem.Lyrics[i].FS_Top / num);
				InItem.Lyrics[i].Height = Convert.ToInt32((double)InItem.Lyrics[i].FS_Height / num);
				InItem.Lyrics[i].Height_R2Bound = Convert.ToInt32((double)InItem.Lyrics[i].FS_Height_R2Bound / num);
			}
			i = 3;
			InItem.Lyrics[i].BackColour = PanelBackColour;
			InItem.Lyrics[i].Transparent = ((PanelBackColorAsScreen > 0) ? true : false);
			InItem.Lyrics[i].ForeColour = ((PaneltextColourAsRegion1 > 0) ? InItem.Lyrics[0].ForeColour : PanelTextColour);
			InItem.Lyrics[i].FS_Width = buffer_LS_Width - buffer_LS_Width / 50;
			InItem.Lyrics[i].FS_Left = (buffer_LS_Width - InItem.Lyrics[i].FS_Width) / 2;
			fontStyle = FontStyle.Regular;
			if (ShowDataDisplayFontBold > 0)
			{
				fontStyle |= FontStyle.Bold;
			}
			if (ShowDataDisplayFontItalic > 0)
			{
				fontStyle |= FontStyle.Italic;
			}
			if (ShowDataDisplayFontUnderline > 0)
			{
				fontStyle |= FontStyle.Underline;
			}
			InItem.Lyrics[i].TextAlign = StringAlignment.Near;
			double num14 = BottomBorderFactor * (double)buffer_LS_Height;
			InItem.Lyrics[i].FS_Height = (int)(num14 * 0.8);
			InItem.Lyrics[i].FS_Top = buffer_LS_Height - InItem.Lyrics[i].FS_Height;
			num11 = 40;
			Font font;
			try
			{
				font = new Font(ShowDataDisplayFontName, num11, fontStyle);
			}
			catch
			{
				font = new Font("Microsoft Sans Serif", num11, fontStyle);
			}
			while ((graphics.MeasureString("A", font, layoutArea).Height > (float)(InItem.Lyrics[i].FS_Height * 9 / 20)) & (font.Size > 1f))
			{
				num11--;
				font = new Font(font.Name, num11, fontStyle);
			}
			num11 = (int)font.Size + 1;
			num12 = (double)font.Size / num;
			InItem.Lyrics[i].FS_Height_R2Bound = InItem.Lyrics[i].FS_Height;
			InItem.Lyrics[i].Width = (int)((double)InItem.Lyrics[i].FS_Width / num);
			InItem.Lyrics[i].Left = (int)((double)InItem.Lyrics[i].FS_Left / num);
			InItem.Lyrics[i].Top = (int)((double)InItem.Lyrics[i].FS_Top / num);
			InItem.Lyrics[i].Height = (int)((double)InItem.Lyrics[i].FS_Height / num);
			InItem.Lyrics[i].Height_R2Bound = (int)((double)InItem.Lyrics[i].FS_Height_R2Bound / num);
			InItem.Lyrics[i].FS_Font = new Font(font.Name, num11, fontStyle);
			InItem.Lyrics[i].Font = new Font(font.Name, (!(num12 > 0.0)) ? 1 : ((int)num12), fontStyle);
			//graphics.Dispose();
			//image.Dispose();
		}

		public static int SetLyricsTopPos(int TopSetting, int ScreenHeight)
		{
			return ScreenHeight * TopSetting / 100;
		}

		public static int DisplayFontSize(int InFontSize, int InLyricsWidth, int InNum, int Folderno)
		{
			if (InNum == 2)
			{
				try
				{
					InFontSize = InFontSize * FolderHeadingPercentSize[Folderno] / 100;
				}
				catch
				{
				}
			}
			int num = InFontSize * InLyricsWidth / 960;
			return (num < 1) ? 1 : num;
		}

		public static int Load32HeaderData(string InFileName, string InContents, ref string[] ThisHeaderData)
		{
			return Load32HeaderData(InFileName, InContents, ref ThisHeaderData, '>');
		}

		public static int Load32HeaderData(string InFileName, string InContents, ref string[] ThisHeaderData, char SeparatorSym)
		{
			for (int i = 0; i < 255; i++)
			{
				ThisHeaderData[i] = "";
			}
			int num = InContents.IndexOf("[");
			int num2 = InContents.IndexOf("]", num + 1);
			if (num == 0 && num2 > 1)
			{
				num++;
				int num3 = num2 + 1;
				string InFormatString = DataUtil.Mid(InContents, num, num2 - num + 1);
				if (DataUtil.Convertv32FormatString(ref InFormatString, SeparatorSym) == 320)
				{
					LoadHeaderData(InFormatString, ref ThisHeaderData, '>');
					return num2;
				}
			}
			num = 0;
			num2 = 0;
			InContents = "";
			return num2;
		}

		public static string ExtractHeaderInfo(string InString, int Index, char Separator)
		{
			int num = InString.IndexOf(Index + "=");
			if (num >= 0)
			{
				num += Index.ToString().Length + 1;
				int num2 = InString.IndexOf(Separator, num);
				if (num2 <= num)
				{
					return "";
				}
				return DataUtil.Mid(InString, num, num2 - num);
			}
			return "";
		}

		public static int LoadHeaderData(string InFormatString, ref string[] ThisHeaderData, char SeparatorSym)
		{
			for (int i = 0; i < 255; i++)
			{
				ThisHeaderData[i] = "";
			}
			if (InFormatString == "" || InFormatString[0] == '[')
			{
				return -1;
			}
			string text = "";
			string text2 = "";
			int num = -1;
			string[] array = InFormatString.Split('>');
			for (int i = 0; i <= array.GetUpperBound(0); i++)
			{
				text2 = DataUtil.ExtractOneInfo(ref array[i], '=', RemoveExtract: true, MinusOneIfBlank: false);
				if (text2 != "")
				{
					num = DataUtil.StringToInt(text2);
					if (num > 0 && num < 255)
					{
						ThisHeaderData[num] = array[i];
					}
				}
			}
			return 1;
		}

		public static void ApplyHeaderData()
		{
			ApplyHeaderData(0);
		}

		public static void ApplyHeaderData(int InMode)
		{
			int num = IndexFileVersion = ExtractNumericData(HeaderData[1]);
			if (InMode != 1)
			{
				num = ExtractNumericData(HeaderData[11]);
				PanelBackColour = (((HeaderData[11] == "") | !ValidateColour(num)) ? DefaultBackColour : Color.FromArgb(Convert.ToInt32(num)));
				num = ExtractNumericData(HeaderData[12]);
				PanelBackColourTransparent = (((num < 0) | (num > 1)) ? 1 : num);
				num = ExtractNumericData(HeaderData[13]);
				PanelTextColour = (((HeaderData[13] == "") | !ValidateColour(num)) ? DefaultForeColour : Color.FromArgb(Convert.ToInt32(num)));
				num = ExtractNumericData(HeaderData[14]);
				PanelTextColourAsRegion1 = (((num < 0) | (num > 1)) ? 1 : num);
				num = ExtractNumericData(HeaderData[15]);
				num = ((num < 0) ? 31 : num);
				ShowDataDisplayMode = DataUtil.GetBitValue(num, 1);
				ShowDataDisplaySlides = DataUtil.GetBitValue(num, 2);
				ShowDataDisplaySongs = DataUtil.GetBitValue(num, 3);
				ShowDataDisplayTitle = DataUtil.GetBitValue(num, 4);
				ShowDataDisplayCopyright = DataUtil.GetBitValue(num, 5);
				ShowDataDisplayPrevNext = DataUtil.GetBitValue(num, 6);
				num = ExtractNumericData(HeaderData[16]);
				num = ((num < 6 || num > 20) ? 8 : num);
				BottomBorderFactor = (double)num / 100.0;
				ShowDataDisplayFontName = HeaderData[17];
				if (ShowDataDisplayFontName == "")
				{
					ShowDataDisplayFontName = "Microsoft Sans Serif";
				}
				num = ExtractNumericData(HeaderData[18]);
				num = ((num >= 0) ? num : 0);
				ShowDataDisplayFontBold = DataUtil.GetBitValue(num, 1);
				ShowDataDisplayFontItalic = DataUtil.GetBitValue(num, 2);
				ShowDataDisplayFontUnderline = DataUtil.GetBitValue(num, 3);
				ShowDataDisplayFontShadow = DataUtil.GetBitValue(num, 4);
				ShowDataDisplayFontOutline = DataUtil.GetBitValue(num, 5);
				num = ExtractNumericData(HeaderData[19]);
				ShowDataDisplayIndicatorsFontSize = ((num < 8 || num > 20) ? 8 : num);
				num = ExtractNumericData(HeaderData[21]);
				ShowSongHeadings = (((num < 0) | (num > 3)) ? 1 : num);
				num = ExtractNumericData(HeaderData[22]);
				num = ((num < 0) ? 2 : num);
				UseShadowFont = DataUtil.GetBitValue(num, 2);
				ShowNotations = DataUtil.GetBitValue(num, 3);
				ShowCapoZero = DataUtil.GetBitValue(num, 4);
				ShowInterlace = DataUtil.GetBitValue(num, 5);
				UseOutlineFont = DataUtil.GetBitValue(num, 6);
				num = ExtractNumericData(HeaderData[23]);
				ShowSongHeadingsAlign = ((!((num < 0) | (num > 4))) ? num : 0);
				num = ExtractNumericData(HeaderData[25]);
				ShowLyrics = (((num < 0) | (num > 2)) ? 2 : num);
				num = ExtractNumericData(HeaderData[26]);
				ShowScreenColour[0] = (((HeaderData[26] == "") | !ValidateColour(num)) ? DefaultBackColour : Color.FromArgb(Convert.ToInt32(num)));
				num = ExtractNumericData(HeaderData[27]);
				ShowScreenColour[1] = (((HeaderData[27] == "") | !ValidateColour(num)) ? ShowScreenColour[0] : Color.FromArgb(Convert.ToInt32(num)));
				num = ExtractNumericData(HeaderData[28]);
				ShowScreenStyle = ((!((num < 0) | (num > MaxBackgroundStyleIndex))) ? num : 0);
				num = ExtractNumericData(HeaderData[29]);
				ShowFontColour[0] = (((HeaderData[29] == "") | !ValidateColour(num)) ? DefaultForeColour : Color.FromArgb(Convert.ToInt32(num)));
				num = ExtractNumericData(HeaderData[30]);
				ShowFontColour[1] = (((HeaderData[30] == "") | !ValidateColour(num)) ? DefaultForeColour : Color.FromArgb(Convert.ToInt32(num)));
				num = ExtractNumericData(HeaderData[31]);
				ShowFontAlign[0, 0] = (((num < 0) | (num > 3)) ? 2 : num);
				num = ExtractNumericData(HeaderData[32]);
				ShowFontAlign[0, 1] = (((num < 0) | (num > 3)) ? 2 : num);
				num = ExtractNumericData(HeaderData[50]);
				MediaOption = ((!((num < 0) | (num > 3))) ? num : 0);
				MediaLocation = HeaderData[51];
				num = ExtractNumericData(HeaderData[52]);
				MediaVolume = (((num < 0) | (num > 100)) ? 50 : num);
				num = ExtractNumericData(HeaderData[53]);
				MediaBalance = ((!((num < -100) | (num > 100))) ? num : 0);
				num = ExtractNumericData(HeaderData[54]);
				MediaMute = DataUtil.GetBitValue(num, 1);
				MediaRepeat = DataUtil.GetBitValue(num, 2);
				MediaWidescreen = DataUtil.GetBitValue(num, 3);
				num = ExtractNumericData(HeaderData[55]);
				MediaCaptureDeviceNumber = (((num < 1) | (num > 5)) ? 1 : num);
				BackgroundPicture = HeaderData[61];
				num = ExtractNumericData(HeaderData[62]);
				BackgroundMode = (ImageMode)(((num < 0) | (num > 2)) ? 2 : num);
				num = ExtractNumericData(HeaderData[63]);
				ShowVerticalAlign = (((num < 0) | (num > 2)) ? 1 : num);
				num = ExtractNumericData(HeaderData[64]);
				ShowLeftMargin[0] = (((num < 0) | (num > 40)) ? 2 : num);
				num = ExtractNumericData(HeaderData[65]);
				ShowRightMargin[0] = (((num < 0) | (num > 40)) ? 2 : num);
				num = ExtractNumericData(HeaderData[66]);
				ShowBottomMargin[0] = ((!((num < 0) | (num > 100))) ? num : 0);
				ShowItemTransition = GlobalImageCanvas.GetTransitionType(HeaderData[72]);
				ShowSlideTransition = GlobalImageCanvas.GetTransitionType(HeaderData[73]);
			}
			for (int i = 0; i < 8; i++)
			{
				PB_ShowWords[i] = 1;
				PB_WordsBold[i] = 0;
				PB_WordsItalic[i] = 0;
				PB_WordsUnderline[i] = 0;
				PB_WordsSize[i] = 11;
				PB_WordsColour[i] = BlackScreenColour;
			}
			PB_WordsSize[0] = 13;
			PB_WordsSize[5] = 11;
			PB_WordsSize[2] = 6;
			PB_WordsBold[0] = 1;
			PB_WordsBold[1] = 1;
			PB_WordsItalic[4] = 1;
			PB_WordsUnderline[0] = 1;
			PB_ShowHeadings[0] = 1;
			PB_ShowHeadings[1] = 1;
			PB_ShowHeadings[2] = 0;
			PB_ShowHeadings[3] = 0;
			PB_LyricsPattern = 1;
			PB_ShowSection = 2;
			PB_ShowColumns = 2;
			PB_PageSize = 0;
			PB_Spacing[0] = 0;
			PB_Spacing[1] = 2;
			PB_ShowScreenBreaks = 1;
			PB_OneSongPerPage = 0;
			PB_CJKGroupStyle = SortBy.Alpha;
			for (int i = 0; i < 8; i++)
			{
				int num2 = 101 + i * 5;
				num = ExtractNumericData(HeaderData[num2]);
				PB_ShowWords[i] = ((num < 0) ? 1 : DataUtil.GetBitValue(num, 1));
				if (num < 0)
				{
					num = 0;
				}
				if (i < 6)
				{
					PB_WordsBold[i] = DataUtil.GetBitValue(num, 2);
					PB_WordsItalic[i] = DataUtil.GetBitValue(num, 3);
					PB_WordsUnderline[i] = DataUtil.GetBitValue(num, 4);
					num = ExtractNumericData(HeaderData[num2 + 1]);
					PB_WordsSize[i] = (((num < 4) | (num > 72)) ? PB_WordsSize[i] : num);
					num = ExtractNumericData(HeaderData[num2 + 2]);
					PB_WordsColour[i] = (((HeaderData[num2 + 2] == "") | !ValidateColour(num)) ? BlackScreenColour : Color.FromArgb(Convert.ToInt32(num)));
				}
				else
				{
					PB_WordsBold[i] = PB_WordsBold[2];
					PB_WordsItalic[i] = PB_WordsItalic[2];
					PB_WordsUnderline[i] = PB_WordsUnderline[2];
					PB_WordsSize[i] = PB_WordsSize[2];
					PB_WordsColour[i] = PB_WordsColour[2];
				}
			}
			num = ExtractNumericData(HeaderData[151]);
			PB_ShowHeadings[0] = DataUtil.GetBitValue(num, 1);
			PB_ShowHeadings[1] = DataUtil.GetBitValue(num, 2);
			PB_ShowHeadings[2] = DataUtil.GetBitValue(num, 3);
			PB_ShowHeadings[3] = DataUtil.GetBitValue(num, 4);
			num = ExtractNumericData(HeaderData[153]);
			PB_LyricsPattern = (((num < 0) | (num > 1)) ? 1 : num);
			num = ExtractNumericData(HeaderData[154]);
			PB_ShowSection = (((num < 0) | (num > 2)) ? 2 : num);
			num = ExtractNumericData(HeaderData[155]);
			PB_ShowColumns = (((num < 1) | (num > 2)) ? 2 : num);
			num = ExtractNumericData(HeaderData[156]);
			PB_PageSize = ((!((num < 0) | (num > 1))) ? num : 0);
			num = ExtractNumericData(HeaderData[170]);
			PB_Spacing[0] = ((!((num < 0) | (num > 5))) ? num : 0);
			num = ExtractNumericData(HeaderData[171]);
			PB_Spacing[1] = (((num < 1) | (num > 20)) ? 2 : num);
			num = ExtractNumericData(HeaderData[172]);
			PB_ShowScreenBreaks = (((num < 0) | (num > 1)) ? 1 : num);
			num = ExtractNumericData(HeaderData[173]);
			PB_OneSongPerPage = ((!((num < 0) | (num > 1))) ? num : 0);
			num = ExtractNumericData(HeaderData[174]);
			PB_CJKGroupStyle = (SortBy)((!((num < 0) | (num > 1))) ? num : 0);
			num = ExtractNumericData(HeaderData[180]);
			PB_ShowNotations = DataUtil.GetBitValue(num, 1);
			PB_ShowTiming = DataUtil.GetBitValue(num, 2);
			PB_ShowKey = DataUtil.GetBitValue(num, 3);
			PB_ShowCapo = DataUtil.GetBitValue(num, 4);
			PB_CapoZero = ((num >= 0) ? DataUtil.GetBitValue(num, 5) : 0);
		}

		public static int ExtractNumericData(string InString)
		{
			if (string.IsNullOrEmpty(InString))
			{
				return -1;
			}

			int chknum = -1;			
			try
			{
				bool isnum = int.TryParse(InString, out chknum);
				return chknum;
			}
			catch
			{
				return -1;
			}
		}

		public static bool ValidateColour(int InNumber)
		{
			try
			{
				Color color = Color.FromArgb(Convert.ToInt32(InNumber));
				return true;
			}
			catch
			{
				return false;
			}
		}

		public static int GetSelectedIndex(ListView InListView)
		{
			string OutString = null;
			return GetSelectedIndex(InListView, ref OutString, "");
		}

		public static int GetSelectedIndex(ListView InListView, ref string OutString)
		{
			return GetSelectedIndex(InListView, ref OutString, "");
		}

		public static int GetSelectedIndex(ListView InListView, ref string OutString, string InString)
		{
			return GetSelectedIndex(InListView, ref OutString, InString, 0);
		}

		public static int GetSelectedIndex(ListView InListView, ref string OutString, string InString, int ColumnText)
		{
			if (InListView.SelectedItems.Count == 0)
			{
				if (InListView.Items.Count > 0 && InListView.Items[0].Selected)
				{
					return 0;
				}
				if (InString != "")
				{
					MessageBox.Show("Please select a song from the " + InString + " to edit");
				}
				return -1;
			}

			IEnumerator enumerator = InListView.SelectedItems.GetEnumerator();

			try
			{
				if (enumerator.MoveNext())
				{
					ListViewItem listViewItem = (ListViewItem)enumerator.Current;
					if (OutString != null)
					{
						OutString = listViewItem.SubItems[ColumnText].Text;
					}
					return InListView.Items.IndexOf(listViewItem);
				}
			}
			finally
			{
				IDisposable disposable = enumerator as IDisposable;
				if (disposable != null)
				{
					disposable.Dispose();
				}
			}
			return -1;
		}

		public static string RemoveMusicSym(string InString)
		{
			if (DataUtil.Right(InString, MusicSymLen) == " <#>")
			{
				return DataUtil.Left(InString, InString.Length - MusicSymLen);
			}
			return InString;
		}

		public static void SizeLaunchScreen()
		{
			if ((OutputMonitorName == GetPrimaryMonitorName()) & DMAlwaysUseSecondaryMonitor)
			{
				OutputMonitorName = GetSecondryMonitorName();
			}

			GetScreenName(ref OutputMonitorName, ref LS_Top, ref LS_Left, ref LS_Width, ref LS_Height, "None");

			if (DualMonitorSelectAutoOption == 0)
			{
				if (OutputMonitorName != "None")
				{
					DualMonitorMode = true;
					OutputMonitorText = "Dual Monitor Mode";
				}
				else
				{
					DualMonitorMode = false;
					OutputMonitorText = "Single Monitor Mode";
				}
			}
			else
			{
				LS_Top = DMOption1Top;
				LS_Left = DMOption1Left;
				LS_Width = DMOption1Width;
				LS_Height = DMOption1Height;
				DualMonitorMode = ((!DMOption1AsSingleMonitor) ? true : false);
				OutputMonitorText = "Custom Monitor Mode";
			}

			if (LS_Height >= 768)
			{
				Buffer_LS_Height = LS_Height;
				Buffer_LS_Width = LS_Width;
			}
			else
			{
				float num = 768f / (float)LS_Height;
				Buffer_LS_Height = 768;
				Buffer_LS_Width = (int)((float)LS_Width * num);
			}
			ShowTopBorderSize = (int)((double)Buffer_LS_Height * TopBorderFactor);
			ShowBottomBorderSize = (int)((double)Buffer_LS_Height * BottomBorderFactor);
			AdjustedOutlineThreshold = OutlineFontSizeThreshold * Buffer_LS_Width / 960;

			if (LMAlwaysUseSecondaryMonitor & (LyricsMonitorName == GetPrimaryMonitorName()))
			{
				LyricsMonitorName = GetSecondryMonitorName();
			}

			GetScreenName(ref LyricsMonitorName, ref LM_Top, ref LM_Left, ref LM_Width, ref LM_Height, (DualMonitorSelectAutoOption == 0) ? OutputMonitorName : "None", LMAlwaysUseSecondaryMonitor);

			if (LMSelectAutoOption == 1)
			{
				LM_Top = LMOption1Top;
				LM_Left = LMOption1Left;
				LM_Width = LMOption1Width;
				LM_Height = LMOption1Height;
			}
		}

		//public static void SizeLaunchScreen()
		//{
		//	if ((OutputMonitorNumber == GetPrimaryMonitorIndex()) & DMAlwaysUseSecondaryMonitor)
		//	{
		//		OutputMonitorNumber = GetSecondryMonitorIndex();
		//	}

		//	GetScreenNumber(ref OutputMonitorNumber, ref LS_Top, ref LS_Left, ref LS_Width, ref LS_Height, -1);

		//	if (DualMonitorSelectAutoOption == 0)
		//	{
		//		if (OutputMonitorNumber > 0)
		//		{
		//			DualMonitorMode = true;
		//			OutputMonitorText = "Dual Monitor Mode";
		//		}
		//		else
		//		{
		//			DualMonitorMode = false;
		//			OutputMonitorText = "Single Monitor Mode";
		//		}
		//	}
		//	else
		//	{
		//		LS_Top = DMOption1Top;
		//		LS_Left = DMOption1Left;
		//		LS_Width = DMOption1Width;
		//		LS_Height = DMOption1Height;
		//		DualMonitorMode = ((!DMOption1AsSingleMonitor) ? true : false);
		//		OutputMonitorText = "Custom Monitor Mode";
		//	}

		//	if (LS_Height >= 768)
		//	{
		//		Buffer_LS_Height = LS_Height;
		//		Buffer_LS_Width = LS_Width;
		//	}
		//	else
		//	{
		//		float num = 768f / (float)LS_Height;
		//		Buffer_LS_Height = 768;
		//		Buffer_LS_Width = (int)((float)LS_Width * num);
		//	}
		//	ShowTopBorderSize = (int)((double)Buffer_LS_Height * TopBorderFactor);
		//	ShowBottomBorderSize = (int)((double)Buffer_LS_Height * BottomBorderFactor);
		//	AdjustedOutlineThreshold = OutlineFontSizeThreshold * Buffer_LS_Width / 960;

		//	if (LMAlwaysUseSecondaryMonitor & (LyricsMonitorNumber == GetPrimaryMonitorIndex()) )
		//	{
		//		LyricsMonitorNumber = GetSecondryMonitorIndex();
		//	}

		//	GetScreenNumber(ref LyricsMonitorNumber, ref LM_Top, ref LM_Left, ref LM_Width, ref LM_Height, (DualMonitorSelectAutoOption == 0) ? OutputMonitorNumber : (-1));

		//	if (LMSelectAutoOption == 1)
		//	{
		//		LM_Top = LMOption1Top;
		//		LM_Left = LMOption1Left;
		//		LM_Width = LMOption1Width;
		//		LM_Height = LMOption1Height;
		//	}
		//}

		//public static void GetScreenInfo(string monitorName, ref int OutTop, ref int OutLeft, ref int OutWidth, ref int OutHeight)
		//{
		//	GetScreenInfo(ref monitorName, ref OutTop, ref OutLeft, ref OutWidth, ref OutHeight);
		//}

		public static void GetScreenInfo(int InMonitor, ref int OutTop, ref int OutLeft, ref int OutWidth, ref int OutHeight)
		{
			GetScreenNumber(ref InMonitor, ref OutTop, ref OutLeft, ref OutWidth, ref OutHeight, -1);
		}

		public static Screen GetPrimaryMonitor()
		{
			Screen primaryScreen = null;
			Screen[] allScreens = Screen.AllScreens;
			foreach (Screen screen in allScreens)
			{
				if (screen.Primary)
				{
					primaryScreen = screen;
					break;
				}
			}

			return primaryScreen;
		}

		public static int GetPrimaryMonitorIndex()
		{
			int monitorIndex = 0;

			Screen[] allScreens = Screen.AllScreens;

			for (int i = 0; i< allScreens.Length; i++)
			{
				if (allScreens[i] != null)
				{
					if (allScreens[i].Primary)
					{
						monitorIndex = i;
						break;
					}
				}
			}

			return monitorIndex;
		}

		public static int GetSecondryMonitorIndex(int skipMonitorIndex = -1)
		{
			int monitorIndex = 0;

			Screen[] allScreens = Screen.AllScreens;

			for (int i = 0; i < allScreens.Length; i++)
			{
				if (!allScreens[i].Primary)
				{
					if (skipMonitorIndex == i)
						continue;

					monitorIndex = i;
					break;
				}
			}

			return monitorIndex;
		}

		public static string GetPrimaryMonitorName()
		{
			string primaryMonitorName = "None";

			Screen[] allScreens = Screen.AllScreens;
			foreach (Screen screen in allScreens)
			{
				primaryMonitorName = screen.DeviceName;
				if (screen.Primary)
					break;
			}

			return primaryMonitorName;
		}

		public static string GetSecondryMonitorName()
		{
			string secondryMonitorName = "None";

			Screen[] allScreens = Screen.AllScreens;
			foreach(Screen screen in allScreens)
            {
				secondryMonitorName = screen.DeviceName;
				if (!screen.Primary)
					break;
			}

			return secondryMonitorName;
		}

		public static (string, Screen) GetSecondryMonitor()
		{
			string secondryMonitorName = "None";
			Screen secondryScreen = null;

			Screen[] allScreens = Screen.AllScreens;
			foreach (Screen screen in allScreens)
			{
				secondryMonitorName = screen.DeviceName;
				secondryScreen = screen;
				if (!screen.Primary)
					break;
			}

			return (secondryMonitorName, secondryScreen);
		}

		//public static int GetMaxScreen()
		//{
		//	Screen[] allScreens = Screen.AllScreens;
		//	int num = allScreens.Length;
		//	int num2 = 20000;
		//	int num3 = 20000;
		//	int num4 = 0;
		//	for (int i = 0; i < num; i++)
		//	{
		//		if ((allScreens[i].Bounds.Height < num3) & (allScreens[i].Bounds.Width < num3))
		//		{
		//			num4++;
		//		}
		//	}
		//	return num4;
		//}

		public static void GetScreenName(ref string inMonitorName, string skipMonitor, bool isAllwaysSecondryMonitor = false)
		{
			Screen[] allScreens = Screen.AllScreens;

			if (allScreens.Length == 1)
			{
				inMonitorName = allScreens[0].DeviceName;
				return;
			}

			try
			{
				foreach (Screen screen in allScreens)
				{
					if (screen.DeviceName != skipMonitor && !screen.Primary && isAllwaysSecondryMonitor)
					{
						inMonitorName = screen.DeviceName;
						return;
					}
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
				Console.WriteLine(e.StackTrace);
			}
		}

		public static void GetScreenNumber(ref int inMonitor, int skipMonitor)
		{
			Screen[] allScreens = Screen.AllScreens;

			try
			{
				if (allScreens[inMonitor] == null)
				{
					//None 으로 설정
					inMonitor = GetPrimaryMonitorIndex();
					return;
				}

				if (skipMonitor != -1)
				{
					inMonitor = GetSecondryMonitorIndex(skipMonitor);
				}
			}
			catch (Exception e)
			{
				inMonitor = 0;
				Console.WriteLine(e.Message);
				Console.WriteLine(e.StackTrace);
			}
		}

		public static void GetScreenInfo(string inMonitorName, ref int t, ref int l, ref int w, ref int h)
		{

			Screen[] allScreens = Screen.AllScreens;

			Screen selectScreen = null;

			foreach (Screen screen in allScreens)
			{
				if (screen.DeviceName == inMonitorName)
				{
					selectScreen = screen;
					break;
				}
			}

			try
			{
				if (selectScreen != null)
				{
					Size screenSize = selectScreen.Bounds.Size;

					t = selectScreen.Bounds.Y;
					l = selectScreen.Bounds.X;
					h = screenSize.Height;
					w = screenSize.Width;
				}
				else
                {
					Screen primaryScreen = GetPrimaryMonitor();
					Size screenSize = selectScreen.Bounds.Size;

					t = selectScreen.Bounds.Y;
					l = selectScreen.Bounds.X;
					h = screenSize.Height;
					w = screenSize.Width;
				}
			}
			catch (Exception e)
			{
				Screen primaryScreen = GetPrimaryMonitor();
				Size screenSize = selectScreen.Bounds.Size;

				t = selectScreen.Bounds.Y;
				l = selectScreen.Bounds.X;
				h = screenSize.Height;
				w = screenSize.Width;
				
				Console.WriteLine(e.Message);
				Console.WriteLine(e.StackTrace);
			}
		}

		public static void GetScreenName(ref string inMonitorName, ref int t, ref int l, ref int w, ref int h, string skipMonitor, bool isAllwaysSecondryMonitor = false)
		{

			Screen[] allScreens = Screen.AllScreens;

			Screen selectScreen = null;

			foreach (Screen screen in allScreens)
			{
				if (screen.DeviceName == inMonitorName && screen.DeviceName != skipMonitor)
				{
					selectScreen = screen;
					break;
				}
			}

			try
			{
				if ((skipMonitor != "None") & (skipMonitor == GetPrimaryMonitorName()) & isAllwaysSecondryMonitor)
				{
					(inMonitorName, selectScreen) = GetSecondryMonitor();
				}

				if (selectScreen != null)
				{
                    //Rectangle screenRectangle = selectScreen.WorkingArea;

                    //t = screenRectangle.Top;
                    //l = screenRectangle.Left;
                    //h = selectScreen.Bounds.Height;
                    //// daniel
                    //// 프리젠테이션 사이즈 와이드로 변경
                    //// //w = h * 4 / 3;
                    //if (gf.isScreenWideMode)
                    //    w = selectScreen.Bounds.Width;
                    //else
                    //    w = h * 4 / 3;

                    Size screenSize = selectScreen.Bounds.Size;

                    t = selectScreen.Bounds.Y;
                    l = selectScreen.Bounds.X;
                    h = screenSize.Height;
                    // daniel
                    // 프리젠테이션 사이즈 와이드로 변경
                    // //w = h * 4 / 3;
                    if (gf.isScreenWideMode)
                        w = screenSize.Width;
                    else
                        w = h * 4 / 3;
                }
			}
			catch (Exception e)
			{
				Screen primaryScreen = GetPrimaryMonitor();
				Size screenSize = selectScreen.Bounds.Size;

				t = selectScreen.Bounds.Y;
				l = selectScreen.Bounds.X;
				h = screenSize.Height;
				// daniel
				// 프리젠테이션 사이즈 와이드로 변경
				// //w = h * 4 / 3;
				if (gf.isScreenWideMode)
					w = screenSize.Width;
				else
					w = h * 4 / 3;

				//Rectangle screenRectangle = System.Windows.Forms.Screen.AllScreens[0].WorkingArea;

				//t = screenRectangle.Top;
				//l = screenRectangle.Left;
				//h = allScreens[0].Bounds.Height;
				//// daniel
				//// 프리젠테이션 사이즈 와이드로 변경
				//// //w = h * 4 / 3;
				//if (gf.isScreenWideMode)
				//	w = allScreens[0].Bounds.Width;
				//else
				//	w = h * 4 / 3;

				Console.WriteLine(e.Message);
				Console.WriteLine(e.StackTrace);
			}
		}

		public static void GetScreenNumber(ref int inMonitor, ref int t, ref int l, ref int w, ref int h, int skipMonitor)
		{

			Screen[] allScreens = Screen.AllScreens;
			
			int monitorIndex = inMonitor;

			try
			{
				if ((skipMonitor != -1) & (skipMonitor == GetPrimaryMonitorIndex()))
				{
					monitorIndex = GetSecondryMonitorIndex();
				}

				Screen selectScreen = Screen.AllScreens[monitorIndex];

				Size screenSize = selectScreen.Bounds.Size;

				t = selectScreen.Bounds.X;
				l = selectScreen.Bounds.Y;
				h = screenSize.Height;
				// daniel
				// 프리젠테이션 사이즈 와이드로 변경
				// //w = h * 4 / 3;
				if (gf.isScreenWideMode)
					w = screenSize.Width;
				else
					w = h * 4 / 3;

				//Rectangle screenRectangle = System.Windows.Forms.Screen.AllScreens[monitorIndex].WorkingArea;

				//t = screenRectangle.Top;
				//l = screenRectangle.Left;
				//h = allScreens[monitorIndex].Bounds.Height;
				//// daniel
				//// 프리젠테이션 사이즈 와이드로 변경
				//// //w = h * 4 / 3;
				//if (gf.isScreenWideMode)
				//	w = allScreens[monitorIndex].Bounds.Width;
				//else
				//	w = h * 4 / 3;
			}
			catch (Exception e)
			{
				Screen primaryScreen = GetPrimaryMonitor();
				Size screenSize = primaryScreen.Bounds.Size;

				t = primaryScreen.Bounds.X;
				l = primaryScreen.Bounds.Y;
				h = screenSize.Height;
				// daniel
				// 프리젠테이션 사이즈 와이드로 변경
				// //w = h * 4 / 3;
				if (gf.isScreenWideMode)
					w = screenSize.Width;
				else
					w = h * 4 / 3;

				//Rectangle screenRectangle = System.Windows.Forms.Screen.AllScreens[0].WorkingArea;

				//t = screenRectangle.Top;
				//l = screenRectangle.Left;
				//h = allScreens[0].Bounds.Height;
				//// daniel
				//// 프리젠테이션 사이즈 와이드로 변경
				//// //w = h * 4 / 3;
				//if (gf.isScreenWideMode)
				//	w = allScreens[0].Bounds.Width;
				//else
				//	w = h * 4 / 3;

				Console.WriteLine(e.Message);
				Console.WriteLine(e.StackTrace);
			}
		}

		//public static int GetScreenNumber(ref int InMonitor, ref int t, ref int l, ref int w, ref int h, int SkipMonitor)
		//{
		//	Screen[] allScreens = Screen.AllScreens;
		//	int num = allScreens.Length;
		//	int num2 = 20000;
		//	int num3 = 20000;
		//	int num4 = -1;
		//	for (int i = 0; i < num; i++)
		//	{
		//		if (allScreens[i].Primary)
		//		{
		//			num4 = i; //0이 secondory 1이 primary
		//			i = num;
		//		}
		//	}

		//	// 스크린이 없을 경우 0으로 셋팅
		//	if (num4 < 0)
		//	{
		//		num4 = 0;
		//	}

		//	int num5;
		//	//Primary Monitor 일 경우
		//	if (InMonitor == 0)
		//	{
		//		num5 = num4; //0
		//	}
		//	else if (((InMonitor < num) & (InMonitor != SkipMonitor)) && ((allScreens[InMonitor].Bounds.Height < num3) & (allScreens[InMonitor].Bounds.Width < num3)))
		//	{
		//		num5 = InMonitor;
		//	}
		//	else
		//	{
		//		int i = 0;
		//		while (true)
		//		{
		//			if (i < num)
		//			{
		//				if ((i != num4) & (allScreens[i].Bounds.Height < num3) & (allScreens[i].Bounds.Width < num3) & (InMonitor != SkipMonitor))
		//				{
		//					InMonitor = i;
		//					num5 = i;
		//					break;
		//				}
		//				i++;
		//				continue;
		//			}
		//			InMonitor = 0;
		//			num5 = 0;
		//			InMonitor = 0;
		//			break;
		//		}
		//	}

		//	if (num5 >= num)
		//	{
		//		num5 = num4;
		//	}

		//	try
		//	{
		//		t = allScreens[num5].Bounds.Top;
		//		l = allScreens[num5].Bounds.Left;
		//		h = allScreens[num5].Bounds.Height;
		//		// daniel
		//		// 프리젠테이션 사이즈 와이드로 변경
		//		// //w = h * 4 / 3;
		//		if (gf.isScreenWideMode)
		//			w = allScreens[num5].Bounds.Width;
		//		else
		//			w = h * 4 / 3;


		//	}
		//	catch
		//	{
		//		num5 = 0;
		//		t = allScreens[num5].Bounds.Top;
		//		l = allScreens[num5].Bounds.Left;
		//		h = allScreens[num5].Bounds.Height;
		//		// daniel
		//		// 프리젠테이션 사이즈 와이드로 변경
		//		// //w = h * 4 / 3;
		//		if (gf.isScreenWideMode)
		//			w = allScreens[num5].Bounds.Width;
		//		else
		//			w = h * 4 / 3;
		//	}
		//	return num5;
		//}

		public static void ResetShowRunningSettings()
		{
			ShowRunning_ShowDataDisplayMode = 0;
			ShowRunning_ShowSongHeadings = 0;
			ShowRunning_ShowLyrics = 0;
			ShowRunning_UseShadowFont = 0;
			ShowRunning_UseOutlineFont = 0;
			ShowRunning_ShowNotations = 0;
			ShowRunning_ShowInterlace = 0;
			ShowRunning_ShowVerticalAlign = 0;
		}

		public static void SetListViewColumns(ListView InListView, int NumberofCol)
		{
			InListView.Clear();
			InListView.View = View.Details;
			for (int i = 0; i < NumberofCol; i++)
			{
				InListView.Columns.Add(i.ToString());
			}
		}

		public static void LoadDBFormatString(ref SongSettings InItem)
		{
			try
			{
				string fullSearchString = "select * from SONG where songid=" + InItem.ItemID;
#if OleDb
				DataTable datatable = DbOleDbController.GetDataTable(ConnectStringMainDB, fullSearchString);
#elif SQLite
				using DataTable datatable = DbController.GetDataTable(ConnectStringMainDB, fullSearchString);
#endif
				if (datatable.Rows.Count>0)
				{
					InItem.Format.DBStoredFormat = DataUtil.GetDataString(datatable.Rows[0], "FORMATDATA");
				}
			}
			catch
			{
			}
		}

		public static void LoadIndividualData(ref SongSettings InItem, string InIDString, string InFormatString, int StartingSlide)
		{
			string InTitle = "";
			LoadIndividualData(ref InItem, InIDString, InFormatString, StartingSlide, ref InTitle);
		}

		public static void LoadIndividualData(ref SongSettings InItem, string InIDString, string InFormatString, int StartingSlide, ref string InTitle)
		{
			string a = DataUtil.Left(InIDString, 1);
			InItem.ItemID = DataUtil.Mid(InIDString, 1);
			string InFileName = InItem.ItemID;
			if (a == "D")
			{
				InItem.Type = "D";
				try
				{
					string fullSearchString = "select * from SONG where songid=" + InItem.ItemID;
#if OleDb
					DataTable datatable = DbOleDbController.GetDataTable(ConnectStringMainDB, fullSearchString);
#elif SQLite
					using DataTable datatable = DbController.GetDataTable(ConnectStringMainDB, fullSearchString);
#endif
					
					if (datatable.Rows.Count>0)
					{
						DataRow dr = datatable.Rows[0];
						InItem.Title = DataUtil.GetDataString(dr, "Title_1");
						InItem.Title2 = DataUtil.GetDataString(dr, "Title_2");
						InItem.SongNumber = DataUtil.GetDataInt(dr, "song_number");
						InItem.CompleteLyrics = DataUtil.GetDataString(dr, "Lyrics");
						InItem.FolderNo = DataUtil.GetDataInt(dr, "FolderNo");
						InItem.FontSizeFactor = 100;
						InItem.Writer = DataUtil.GetDataString(dr, "Writer");
						InItem.Copyright = DataUtil.GetDataString(dr, "Copyright");
						InItem.Format.FormatString = InFormatString;
						InItem.Format.DBStoredFormat = DataUtil.GetDataString(dr, "FORMATDATA");
						InItem.Show_LicAdminInfo1 = DataUtil.GetDataString(dr, "LICENCE_ADMIN1");
						InItem.Show_LicAdminInfo2 = DataUtil.GetDataString(dr, "LICENCE_ADMIN2");
						InItem.In_LicAdminInfo1 = InItem.Show_LicAdminInfo1;
						InItem.In_LicAdminInfo2 = InItem.Show_LicAdminInfo2;
						LoadLicAdminDisplayInfo(ref InItem.Show_LicAdminInfo1, ref InItem.Show_LicAdminInfo2);
						InItem.Notations = DataUtil.GetDataString(dr, "msc");
						InItem.Capo = DataUtil.GetDataInt(dr, "capo", Minus1IfBlank: true);
						InItem.CurSlide = StartingSlide;
						InItem.SongSequence = DataUtil.GetDataString(dr, "Sequence");
						InItem.SongOriginalLoadedSequence = InItem.SongSequence;
						InItem.Book_Reference = DataUtil.GetDataString(dr, "Book_Reference");
						InItem.User_Reference = DataUtil.GetDataString(dr, "User_Reference");
						InItem.Timing = DataUtil.GetDataString(dr, "timing");
						InItem.MusicKey = DataUtil.GetDataString(dr, "key");
						InItem.Category = DataUtil.GetDataString(dr, "category");
						InItem.RotateString = ExtractSettings(DataUtil.GetDataString(dr, "SETTINGS"), SettingsCategory.RotateString);
						GetRotationStyle(ref InItem);
						if ((ListViewNotations == null) | (InItem.LyricsAndNotationsList == null))
						{
							InItem.CompleteLyrics = "";
						}
					}
				}
				catch
				{
				}
			}
			else if (a == "P")
			{
				InItem.Type = "P";
				InItem.Title = GetDisplayNameOnly(ref InFileName, UpdateByRef: false);
				InItem.CurSlide = StartingSlide;
				InItem.Path = DataUtil.Mid(InIDString, 1);
			}
			else if (a == "B")
			{
				string InString = InIDString;
				DataUtil.ExtractOneInfo(ref InString, ';');
				int num = LookUpBibleVersionNumber(DataUtil.ExtractOneInfo(ref InString, ';'));
				int num2 = LookUpBibleVersionNumber(DataUtil.ExtractOneInfo(ref InString, ';'));
				InItem.Type = "B";
				if (num >= 0)
				{
					InItem.Type = "B";
					InItem.FolderNo = DataUtil.StringToInt(HB_Versions[num, 5]);
					InItem.FontSizeFactor = DataUtil.StringToInt(HB_Versions[num, 6]);
					InItem.CompleteLyrics = LoadSelectedBibleVerses(InItem.ItemID);
					InItem.Copyright = HB_Versions[num, 3];
					InItem.Show_BookName = ((DataUtil.Left(InItem.ItemID, 1) == "1") ? true : false);
					int num3 = InTitle.IndexOf("(");
					if (num3 > 0)
					{
						InTitle = DataUtil.Trim(DataUtil.Left(InTitle, num3 - 1));
					}
					if (num2 < 0)
					{
						InItem.Title = InTitle + " (" + HB_Versions[num, 1] + ")";
						InItem.HBR2_FolderNo = InItem.FolderNo;
						InItem.HBR2_FontSizeFactor = InItem.FontSizeFactor;
					}
					else
					{
						InItem.Title = InTitle + " (" + HB_Versions[num, 1] + "/" + HB_Versions[num2, 1] + ")";
						InItem.HBR2_FolderNo = DataUtil.StringToInt(HB_Versions[num2, 5]);
						InItem.HBR2_FontSizeFactor = DataUtil.StringToInt(HB_Versions[num2, 6]);
						SongSettings obj2 = InItem;
						obj2.Copyright = obj2.Copyright + "/" + HB_Versions[num2, 3];
					}
					InItem.Format.FormatString = InFormatString;
					LoadLicAdminDisplayInfo(ref InItem.Show_LicAdminInfo1, ref InItem.Show_LicAdminInfo2);
					InItem.Capo = -1;
					InItem.CurSlide = StartingSlide;
					InTitle = InItem.Title;
				}
			}
			else if (a == "T")
			{
				InItem.Type = "T";
				InItem.Title = GetDisplayNameOnly(ref InFileName, UpdateByRef: false);
				InItem.Type = "T";
				InItem.CompleteLyrics = LoadTextFile(InFileName);
				InItem.Format.FormatString = InFormatString;
				InItem.Capo = -1;
				InItem.CurSlide = StartingSlide;
				InItem.Path = DataUtil.Mid(InIDString, 1);
			}
			else if (a == "I")
			{
				InItem.Type = "I";
				string[] ThisHeaderData = new string[255];
				LoadInfoFile(InFileName, ref InItem, ref ThisHeaderData);
				InItem.CurSlide = StartingSlide;
				InItem.Path = DataUtil.Mid(InIDString, 1);
			}
			else if (a == "W")
			{
				InItem.Type = "W";
				InItem.Title = GetDisplayNameOnly(ref InFileName, UpdateByRef: false);
				InItem.Type = "W";
				InItem.CompleteLyrics = GetOfficeDocContents(InFileName);
				InItem.Format.FormatString = InFormatString;
				InItem.Capo = -1;
				InItem.CurSlide = StartingSlide;
				InItem.Path = DataUtil.Mid(InIDString, 1);
			}
			else if (a == "M")
			{
				InItem.Type = "M";
				InItem.Title = GetDisplayNameOnly(ref InFileName, UpdateByRef: false);
				InItem.Type = "M";
				InItem.CompleteLyrics = " ";
				InItem.Format.FormatString = InFormatString;
				InItem.Capo = -1;
				InItem.CurSlide = 1;
				InItem.Path = DataUtil.Mid(InIDString, 1);
			}
			else if (a == "G")
			{
				InItem.Type = "G";
				InItem.Title = "";
				InItem.Type = "G";
				InItem.CompleteLyrics = " ";
				InItem.Format.FormatString = GapFormatString(ref InItem);
				InItem.Capo = -1;
				InItem.CurSlide = StartingSlide;
			}
			InItem.OriginalNotations = InItem.Notations;
			if (InItem.CompleteLyrics == "")
			{
				InItem.CompleteLyrics = " ";
			}
		}

		private static string GapFormatString(ref SongSettings InItem)
		{
			InItem.UseDefaultFormat = false;
			switch (GapItemOption)
			{
				case GapType.Black:
					InItem.Format.ShowScreenColour[0] = BlackScreenColour;
					InItem.Format.ShowScreenColour[1] = BlackScreenColour;
					InItem.Format.ShowScreenStyle = 11;
					InItem.Format.BackgroundPicture = "";
					break;
				case GapType.User:
					{
						InItem.Format.BackgroundPicture = GapItemLogoFile;
						string directoryName = Path.GetDirectoryName(InItem.Format.BackgroundPicture);
						if (directoryName == RootEasiSlidesDir + "Images\\Tiles")
						{
							InItem.Format.BackgroundMode = ImageMode.Tile;
						}
						else
						{
							InItem.Format.BackgroundMode = ImageMode.BestFit;
						}
						break;
					}
				case GapType.Default:
					InItem.Format.ShowScreenColour[0] = ShowScreenColour[0];
					InItem.Format.ShowScreenColour[1] = ShowScreenColour[1];
					InItem.Format.ShowScreenStyle = ShowScreenStyle;
					InItem.Format.BackgroundPicture = BackgroundPicture;
					InItem.Format.BackgroundMode = BackgroundMode;
					break;
				default:
					return "";
			}
			InItem.Format.ShowItemTransition = (GapItemUseFade ? 15 : 0);
			InItem.Format.ShowSlideTransition = InItem.Format.ShowItemTransition;
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(Convert.ToString(26) + "=" + Convert.ToString(InItem.Format.ShowScreenColour[0].ToArgb()) + '>');
			stringBuilder.Append(Convert.ToString(27) + "=" + Convert.ToString(InItem.Format.ShowScreenColour[1].ToArgb()) + '>');
			stringBuilder.Append(Convert.ToString(28) + "=" + InItem.Format.ShowScreenStyle.ToString() + '>');
			stringBuilder.Append(Convert.ToString(50) + "=" + Convert.ToString(InItem.Format.MediaOption) + '>');
			stringBuilder.Append(Convert.ToString(51) + "=" + InItem.Format.MediaLocation + '>');
			stringBuilder.Append(Convert.ToString(52) + "=" + Convert.ToString(InItem.Format.MediaVolume) + '>');
			stringBuilder.Append(Convert.ToString(53) + "=" + Convert.ToString(InItem.Format.MediaBalance) + '>');
			int num = InItem.Format.MediaMute + InItem.Format.MediaRepeat * 2 + InItem.Format.MediaWidescreen * 4;
			stringBuilder.Append(Convert.ToString(54) + "=" + num.ToString() + '>');
			stringBuilder.Append(Convert.ToString(55) + "=" + Convert.ToString(InItem.Format.MediaCaptureDeviceNumber) + '>');
			stringBuilder.Append(Convert.ToString(61) + "=" + InItem.Format.BackgroundPicture + '>');
			stringBuilder.Append(Convert.ToString(62) + "=" + Convert.ToString((int)InItem.Format.BackgroundMode) + '>');
			stringBuilder.Append(Convert.ToString(72) + "=" + tempScreen.GetTransitionText(InItem.Format.ShowItemTransition) + '>');
			stringBuilder.Append(Convert.ToString(73) + "=" + tempScreen.GetTransitionText(InItem.Format.ShowSlideTransition) + '>');
			return stringBuilder.ToString();
		}

		public static bool IsNewR2Format(string InLyrics)
		{
			InLyrics = InLyrics.Replace("\r\n", "\n");
			string[] array = InLyrics.Split(new string[1]
			{
				VerseSymbol[150]
			}, StringSplitOptions.RemoveEmptyEntries);
			if ((InLyrics == "") | (array.GetUpperBound(0) == 0) | (array.GetUpperBound(0) > 1))
			{
				return false;
			}
			if (array[1][0] == "\n"[0])
			{
				array[1] = DataUtil.Mid(array[1], 1);
			}
			for (int i = 1; i <= 99; i++)
			{
				if (DataUtil.Left(array[1], VerseSymbol[i].Length) == VerseSymbol[i])
				{
					return true;
				}
			}
			if (DataUtil.Left(array[1], VerseSymbol[0].Length) == VerseSymbol[0])
			{
				return true;
			}
			if (DataUtil.Left(array[1], VerseSymbol[100].Length) == VerseSymbol[100])
			{
				return true;
			}
			if (DataUtil.Left(array[1], VerseSymbol[103].Length) == VerseSymbol[103])
			{
				return true;
			}
			if (DataUtil.Left(array[1], VerseSymbol[101].Length) == VerseSymbol[101])
			{
				return true;
			}
			if (DataUtil.Left(array[1], VerseSymbol[102].Length) == VerseSymbol[102])
			{
				return true;
			}
			if (DataUtil.Left(array[1], VerseSymbol[111].Length) == VerseSymbol[111])
			{
				return true;
			}
			if (DataUtil.Left(array[1], VerseSymbol[112].Length) == VerseSymbol[112])
			{
				return true;
			}
			return false;
		}

		public static void InitialiseIndividualData(ref SongSettings InItem)
		{
			InitialiseIndividualData(ref InItem, GapMedia.None, "");
		}

		public static void InitialiseIndividualData(ref SongSettings InItem, GapMedia InGapMedia, string InType)
		{
			InItem.ItemID = "";
			InItem.PrevItemPP = ((InItem.Type == "P") ? true : false);
			InItem.Type = "";
			InItem.SongNumber = 0;
			InItem.FolderNo = 0;
			InItem.CompleteLyrics = "";
			InItem.SongSequence = "";
			InItem.SongBasicSequence = "";
			InItem.SongOriginalLoadedSequence = "";
			InItem.Writer = "";
			InItem.Copyright = "";
			InItem.Capo = -1;
			InItem.Timing = "";
			InItem.MusicKey = "";
			InItem.OriginalNotations = "";
			InItem.Notations = "";
			InItem.Category = "";
			InItem.Show_LicAdminInfo1 = "";
			InItem.Show_LicAdminInfo2 = "";
			InItem.In_LicAdminInfo1 = "";
			InItem.In_LicAdminInfo2 = "";
			InItem.Book_Reference = "";
			InItem.User_Reference = "";
			InItem.HBR2_FolderNo = 0;
			InItem.HBR2_FontSizeFactor = 100;
			InItem.FontSizeFactor = 100;
			InItem.CurSlide = 0;
			InItem.TotalSlides = 0;
			InItem.Path = "";
			InItem.RotateString = "";
			InItem.RotateStyle = 1;
			InItem.RotateGap = 0;
			InItem.RotateTotal = 0;
			InItem.RotateTimings = "";
			InItem.RotateSequence = "";
			InItem.Format.ImageString = "";
			InItem.Format.TempImageFileName = "";
			InItem.FirstShowing = true;
			InItem.FolderName = "";
			InItem.PrevTitle = "";
			InItem.NextTitle = "";
			InItem.Format.FormatString = "";
			InItem.Format.DBStoredFormat = "";
			if (InGapMedia == GapMedia.SessionMedia && MediaOption == 1)
			{
				InGapMedia = GapMedia.None;
			}
			switch (InGapMedia)
			{
				case GapMedia.SameAsPrevious:
					InItem.Format.MediaOption = (InItem.UseDefaultFormat ? MediaOption : InItem.Format.MediaOption);
					InItem.Format.MediaLocation = (InItem.UseDefaultFormat ? MediaLocation : InItem.Format.MediaLocation);
					InItem.Format.MediaVolume = (InItem.UseDefaultFormat ? MediaVolume : InItem.Format.MediaVolume);
					InItem.Format.MediaBalance = (InItem.UseDefaultFormat ? MediaBalance : InItem.Format.MediaBalance);
					InItem.Format.MediaCaptureDeviceNumber = (InItem.UseDefaultFormat ? MediaCaptureDeviceNumber : InItem.Format.MediaCaptureDeviceNumber);
					break;
				case GapMedia.SessionMedia:
					InItem.Format.MediaOption = MediaOption;
					InItem.Format.MediaLocation = MediaLocation;
					InItem.Format.MediaVolume = MediaVolume;
					InItem.Format.MediaBalance = MediaBalance;
					InItem.Format.MediaCaptureDeviceNumber = MediaCaptureDeviceNumber;
					break;
				default:
					InItem.Title = "";
					InItem.Title2 = "";
					InItem.Format.MediaOption = 0;
					InItem.Format.MediaLocation = "";
					InItem.Format.MediaVolume = 0;
					InItem.Format.MediaBalance = 0;
					InItem.Format.MediaCaptureDeviceNumber = 0;
					break;
			}
			InItem.UseDefaultFormat = true;
		}

		public static int LookUpBibleVersionNumber(string InFileName)
		{
			if (HB_TotalVersions < 1)
			{
				return -1;
			}
			for (int i = 0; i < HB_TotalVersions; i++)
			{
				if (HB_Versions[i, 4] == BibleDir + InFileName)
				{
					return i;
				}
			}
			return -1;
		}

		public static void LoadLicAdminDisplayInfo(ref string AdminInfo1, ref string AdminInfo2)
		{
			string text = "";
			string text2 = "";
			if (AdminInfo1 == "")
			{
				text = LicAdmin_List[0, 2];
			}
			else
			{
				for (int i = 2; i <= 9; i++)
				{
					if (LicAdmin_List[i, 0] != "" && AdminInfo1 == LicAdmin_List[i, 0])
					{
						text = LicAdmin_List[i, 2];
						i = 9;
					}
				}
			}
			if (AdminInfo2 == "")
			{
				text2 = LicAdmin_List[0, 2];
			}
			else
			{
				for (int i = 2; i <= 9; i++)
				{
					if (LicAdmin_List[i, 0] != "" && AdminInfo2 == LicAdmin_List[i, 0])
					{
						text2 = LicAdmin_List[i, 2];
						i = 9;
					}
				}
			}
			if (text != "")
			{
				AdminInfo1 = text;
			}
			if (text != text2)
			{
				AdminInfo2 = text2;
			}
		}

		public static int IncrementChord(ref int CurPos, int TransposeTo)
		{
			int num = CurPos + TransposeTo;
			if (num >= 12)
			{
				num %= 12;
			}
			else if (num < 0)
			{
				num += 12 * (1 - num / 12);
			}
			return num;
		}

		public void LoadIndividualFormatData(ref SongSettings InItem)
		{
			LoadIndividualFormatData(ref InItem, "");
		}

        /// <summary>
        /// daniel
        /// 사용자가 BibleTab을 선택하고 성경 특정권을 선택후 특정절을 선택시
        /// 아래 메소드가 호출됨
        /// </summary>
        /// <param name="InFullBibleString"></param>
        /// <returns></returns>
        public static string LoadSelectedBibleVerses(string InFullBibleString)
        {
            try
            {
                // 첫 번째 값이 0보다 크면 flag = true
                bool flag = DataUtil.StringToInt(DataUtil.ExtractOneInfo(ref InFullBibleString, ';')) > 0;

                // 성경 버전 정보 저장
                string[] bibleVersions = new string[2];
                int[] versionNumbers = new int[2];

                for (int i = 0; i < 2; i++)
                {
                    bibleVersions[i] = DataUtil.ExtractOneInfo(ref InFullBibleString, ';');
                    versionNumbers[i] = LookUpBibleVersionNumber(bibleVersions[i]);
                }

                string[] verseData = { InFullBibleString, InFullBibleString };

                // 결과 문자열
                StringBuilder InTextString = new StringBuilder();

                bool hasSecondVersion = versionNumbers[1] >= 0;

                for (int i = 0; i <= (hasSecondVersion ? 1 : 0); i++)
                {
                    bool flag2 = PartialWordSearch(versionNumbers[1]);

                    if (i == 1)
                    {
                        InTextString.Append(VerseSymbol[150]).Append("\n");
                    }

                    while (!string.IsNullOrEmpty(verseData[i]))
                    {
                        int inBookNumber = DataUtil.StringToInt(DataUtil.ExtractOneInfo(ref verseData[i], ';'));
                        int chapterStart = DataUtil.StringToInt(DataUtil.ExtractOneInfo(ref verseData[i], ';'));
                        int verseStart = DataUtil.StringToInt(DataUtil.ExtractOneInfo(ref verseData[i], ';'));
                        int chapterEnd = DataUtil.StringToInt(DataUtil.ExtractOneInfo(ref verseData[i], ';'));
                        int verseEnd = DataUtil.StringToInt(DataUtil.ExtractOneInfo(ref verseData[i], ';'));

                        LoadBiblePassages(versionNumbers[i], inBookNumber, ref InTextString,
                            InShowVerses: true, DoCompleteBook: false, TrackOutput: false,
                            chapterStart, verseStart, chapterEnd, verseEnd,
                            flag, flag2, flag, ShowFormatTags: true);

                        InTextString.Append("\n");
                    }
                }

                return InTextString.ToString();
            }
            catch (Exception ex)
            {
                // 예외 발생 시 디버깅을 위해 로그 출력
                Console.WriteLine($"Error: {ex.Message}");
                return "";
            }
        }

        /// <summary>
        /// daniel
        /// 사용자가 BibleTab을 선택하고 성경 특정권을 선택후 특정절을 선택시
        /// 아래 메소드가 호출됨
        /// </summary>
        /// <param name="InFullBibleString"></param>
        /// <returns></returns>
        public static string LoadSelectedBibleVerses_Old(string InFullBibleString)
        {
            try
            {
                bool flag = (DataUtil.StringToInt(DataUtil.ExtractOneInfo(ref InFullBibleString, ';')) > 0) ? true : false;
                string[] array = new string[2];
                int[] array2 = new int[2];
                string[] array3 = new string[2];
                array[0] = DataUtil.ExtractOneInfo(ref InFullBibleString, ';');
                array2[0] = LookUpBibleVersionNumber(array[0]);
                array[1] = DataUtil.ExtractOneInfo(ref InFullBibleString, ';');
                array2[1] = LookUpBibleVersionNumber(array[1]);
                array3[0] = InFullBibleString;
                array3[1] = InFullBibleString;
                StringBuilder InTextString = new StringBuilder();
                string text = "";
                for (int i = 0; i <= ((array2[1] >= 0) ? 1 : 0); i++)
                {
                    bool flag2 = false;
                    string text2 = ConnectStringDef + HB_Versions[array2[i], 4];
                    flag2 = PartialWordSearch(array2[1]);
                    if (i == 1)
                    {
                        InTextString.Append(VerseSymbol[150] + "\n");
                    }
                    while (array3[i] != "")
                    {
                        int inBookNumber = DataUtil.StringToInt(DataUtil.ExtractOneInfo(ref array3[i], ';'));
                        int chapterStart = DataUtil.StringToInt(DataUtil.ExtractOneInfo(ref array3[i], ';'));
                        int verseStart = DataUtil.StringToInt(DataUtil.ExtractOneInfo(ref array3[i], ';'));
                        int chapterEnd = DataUtil.StringToInt(DataUtil.ExtractOneInfo(ref array3[i], ';'));
                        int verseEnd = DataUtil.StringToInt(DataUtil.ExtractOneInfo(ref array3[i], ';'));
                        LoadBiblePassages(array2[i], inBookNumber, ref InTextString, InShowVerses: true, DoCompleteBook: false, TrackOutput: false, chapterStart, verseStart, chapterEnd, verseEnd, flag, flag2, flag ? true : false, ShowFormatTags: true);
                        InTextString.Append("\n");
                    }
                }
                return InTextString.ToString();
            }
            catch
            {
                return "";
            }
        }

		public static void UpdatePosUpDowns(ref NumericUpDown Reg1_UpDown, ref NumericUpDown Reg2_UpDown, ref NumericUpDown RegBottom_UpDown, ref int Reg1_Value, ref int Reg2_Value, int RegBottom_Value)
		{
			if (Reg1_Value < 0)
			{
				Reg1_Value = 0;
			}
			if (Reg1_Value > 100)
			{
				Reg1_Value = 100;
			}
			if (Reg2_Value < Reg1_Value)
			{
				Reg2_Value = Reg1_Value;
			}
			if (Reg2_Value > 100)
			{
				Reg2_Value = 100;
			}
			if (RegBottom_Value + Reg1_Value > 100)
			{
				RegBottom_Value = 100 - Reg1_Value;
				Reg2_Value = Reg1_Value;
			}
			if (RegBottom_Value + Reg2_Value > 100)
			{
				Reg2_Value = 100 - RegBottom_Value;
			}
			Reg1_UpDown.Minimum = 0m;
			Reg1_UpDown.Maximum = 100m;
			Reg1_UpDown.Value = Reg1_Value;
			Reg1_UpDown.Maximum = 100 - RegBottom_Value;
			Reg2_UpDown.Minimum = Reg1_Value;
			Reg2_UpDown.Maximum = 100m;
			Reg2_UpDown.Value = Reg2_Value;
			Reg2_UpDown.Maximum = 100 - RegBottom_Value;
			RegBottom_UpDown.Minimum = 0m;
			RegBottom_UpDown.Maximum = 100m;
			RegBottom_UpDown.Value = RegBottom_Value;
		}

		public static void SetShowBackground(SongSettings InItem, ref ImageTransitionControl InPic)
		{
			SetShowBackground(InItem, ref InPic, FallBackToDefault: true);
		}

		public static void SetShowBackground(SongSettings InItem, ref ImageTransitionControl InPic, bool FallBackToDefault)
		{
			ImageMode picMode = ImageMode.Tile;
			string text = "";
			if (InItem.UseDefaultFormat)
			{
				if (File.Exists(BackgroundPicture))
				{
					text = BackgroundPicture;
				}
				picMode = BackgroundMode;
			}
			else
			{
				string text2 = (InItem.Type == "I") ? InItem.Format.TempImageFileName : InItem.Format.BackgroundPicture;
				if (File.Exists(text2))
				{
					text = text2;
					picMode = InItem.Format.BackgroundMode;
				}
				else if (InItem.Type == "I" && InItem.Format.TempImageFileName == "")
				{
					InItem.Format.ShowScreenColour[0] = ShowScreenColour[0];
					InItem.Format.ShowScreenColour[1] = ShowScreenColour[1];
					InItem.Format.ShowScreenStyle = ShowScreenStyle;
				}
			}
			if (text == "")
			{
				SetColoursFormat(InItem, ref InPic);
			}
			else
			{
				SetImageFormat(ref InPic, text, picMode, SetTransparent: false);
			}
		}

		public static void SetColoursFormat(SongSettings InItem, ref ImageTransitionControl InPic)
		{
			SetColoursFormat(InItem, ref InPic, SetTransparent: false);
		}

		public static void SetColoursFormat(SongSettings InItem, ref ImageTransitionControl InPic, bool SetTransparent)
		{
			if (SetTransparent)
			{
				string text = "";
				Image image = new Bitmap(Buffer_LS_Width, Buffer_LS_Height);
				Graphics g = Graphics.FromImage(image);
				BackPattern.Clear(ref g, TransparentColour);
				InPic.NewBackgroundPicture = image;
				InPic.NewTextImage = InPic.NewBackgroundPicture;
				InPic.TransitBackPictureAction = ImageTransitionControl.BackPicturesTransition.None;
				InPic.CurrentBackgroundPicture = (Image)InPic.NewBackgroundPicture.Clone();
				//image.Dispose();
				//g.Dispose();
			}
			else if (InItem.UseDefaultFormat)
			{
				if (InPic.NewBackgroundPicture != null)
				{
					InPic.CurrentBackgroundPicture = (Image)InPic.NewBackgroundPicture.Clone();
				}
				else
				{
					InPic.CurrentBackgroundPicture = InPic.BackgroundImage;
					InPic.CurrentTextImage = InPic.BackgroundImage;
					InPic.CurrentCombinedImage = InPic.BackgroundImage;
					InPic.NewTextImage = InPic.BackgroundImage;
				}
				InPic.NewBackgroundPicture = InPic.BackgroundImage;
				if (InPic.BackgroundID == DefaultBackgroundID)
				{
					InPic.TransitBackPictureAction = ImageTransitionControl.BackPicturesTransition.None;
				}
				else
				{
					InPic.TransitBackPictureAction = ImageTransitionControl.BackPicturesTransition.CurrentOnly;
				}
				InPic.BackgroundID = DefaultBackgroundID;
			}
			else
			{
				string text = "";
				Image image = new Bitmap(Buffer_LS_Width, Buffer_LS_Height);
				Graphics g = Graphics.FromImage(image);
				BackPattern.Fill(ref g, InItem.Format.ShowScreenColour[0], InItem.Format.ShowScreenColour[1], InItem.Format.ShowScreenStyle, Buffer_LS_Width, Buffer_LS_Height, ref text);
				if (InPic.NewBackgroundPicture != null)
				{
					InPic.CurrentBackgroundPicture = (Image)InPic.NewBackgroundPicture.Clone();
				}
				else
				{
					InPic.CurrentBackgroundPicture = InPic.BackgroundImage;
					InPic.CurrentTextImage = InPic.BackgroundImage;
					InPic.CurrentCombinedImage = InPic.BackgroundImage;
					InPic.NewTextImage = InPic.BackgroundImage;
				}
				InPic.NewBackgroundPicture = image;
				if (InPic.BackgroundID == text)
				{
					InPic.TransitBackPictureAction = ImageTransitionControl.BackPicturesTransition.None;
				}
				else if (InPic.BackgroundID == DefaultBackgroundID)
				{
					InPic.TransitBackPictureAction = ImageTransitionControl.BackPicturesTransition.NewOnly;
				}
				else if (text == DefaultBackgroundID)
				{
					InPic.TransitBackPictureAction = ImageTransitionControl.BackPicturesTransition.CurrentOnly;
				}
				else
				{
					InPic.TransitBackPictureAction = ImageTransitionControl.BackPicturesTransition.BothBackgrounds;
				}
				InPic.BackgroundID = text;
				//image.Dispose();
				//g.Dispose();
			}
			try
			{
				if (InPic.CurrentBackgroundPicture == null)
				{
					InPic.CurrentBackgroundPicture = (Image)InPic.NewBackgroundPicture.Clone();
				}
				if (InPic.CurrentTextImage == null)
				{
					InPic.CurrentTextImage = (Image)InPic.NewBackgroundPicture.Clone();
				}
				if (InPic.CurrentCombinedImage == null)
				{
					InPic.CurrentCombinedImage = (Image)InPic.NewBackgroundPicture.Clone();
				}
				if (InPic.NewBackgroundPicture == null)
				{
					InPic.NewBackgroundPicture = (Image)InPic.NewBackgroundPicture.Clone();
				}
				if (InPic.NewTextImage == null)
				{
					InPic.NewTextImage = (Image)InPic.NewBackgroundPicture.Clone();
				}
				if (InPic.NewCombinedImage == null)
				{
					InPic.NewCombinedImage = (Image)InPic.NewBackgroundPicture.Clone();
				}
			}
			catch
			{
			}
		}

		public static void SetImageFormat(ref ImageTransitionControl InPic, string File_Name, ImageMode PicMode, bool SetTransparent)
		{
			Image image = new Bitmap(Buffer_LS_Width, Buffer_LS_Height);
			Graphics graphics = Graphics.FromImage(image);
			if (InPic.BackgroundID != "")
			{
				if (InPic.BackgroundID == DefaultBackgroundID)
				{
					InPic.TransitBackPictureAction = ImageTransitionControl.BackPicturesTransition.NewOnly;
				}
				else
				{
					InPic.TransitBackPictureAction = ImageTransitionControl.BackPicturesTransition.BothBackgrounds;
				}
			}
			else if ((InPic.ImageFileName == File_Name) & (InPic.PicMode == (int)PicMode))
			{
				InPic.TransitBackPictureAction = ImageTransitionControl.BackPicturesTransition.None;
			}
			else
			{
				InPic.TransitBackPictureAction = ImageTransitionControl.BackPicturesTransition.BothBackgrounds;
			}
			InPic.BackgroundID = "";
			Image image2 = Image.FromFile(File_Name);
			InPic.ImageFileName = File_Name;
			InPic.PicMode = (int)PicMode;
			double num = (double)image2.Width / (double)image2.Height;
			if ((InPic.Width <= 0) | (InPic.Height <= 0))
			{
				return;
			}
			double num2 = (double)InPic.Width / (double)InPic.Height;
			switch (PicMode)
			{
				case ImageMode.Centre:
					{
						int x2 = 0;
						int y2 = 0;
						int num4 = image2.Width;
						int num3 = image2.Height;
						int x;
						int width;
						int height;
						int y;
						if (num2 < num)
						{
							x = 0;
							width = image.Width;
							height = (int)((double)width / num);
							y = (image.Height - height) / 2;
						}
						else
						{
							y = 0;
							height = image.Height;
							width = (int)((double)height * num);
							x = (image.Width - width) / 2;
						}
						graphics.DrawImage(image2, new Rectangle(x, y, width, height), new Rectangle(x2, y2, num4, num3), GraphicsUnit.Pixel);
						break;
					}
				case ImageMode.Tile:
					{
						int num4 = image2.Width;
						int num3 = image2.Height;
						for (int x2 = 0; x2 <= image.Width / num4; x2++)
						{
							for (int y2 = 0; y2 <= image.Height / num3; y2++)
							{
								int x = x2 * num4;
								int y = y2 * num3;
								graphics.DrawImage(image2, new Rectangle(x, y, num4, num3), new Rectangle(0, 0, num4, num3), GraphicsUnit.Pixel);
							}
						}
						break;
					}
				default:
					{
						int x = 0;
						int y = 0;
						int width = image.Width;
						int height = image.Height;
						int y2;
						int num3;
						int num4;
						int x2;
						if (num2 < num)
						{
							y2 = 0;
							num3 = image2.Height;
							num4 = (int)((double)num3 * num2);
							x2 = (image2.Width - num4) / 2;
						}
						else
						{
							x2 = 0;
							num4 = image2.Width;
							num3 = (int)((double)num4 / num2);
							y2 = (image2.Height - num3) / 2;
						}
						graphics.DrawImage(image2, new Rectangle(x, y, width, height), new Rectangle(x2, y2, num4, num3), GraphicsUnit.Pixel);
						break;
					}
			}
			if (InPic.NewBackgroundPicture != null)
			{
				InPic.CurrentBackgroundPicture = (Image)InPic.NewBackgroundPicture.Clone();
			}
			else
			{
				InPic.CurrentBackgroundPicture = image;
			}
			InPic.NewBackgroundPicture = image;
            //image2.Dispose();
            //image.Dispose();
            //graphics.Dispose();
        }

		public static string GetSlideContents(SongSettings InItem, int CurSlide, int RegionNumber, Font InFont, bool PreviewNotations)
		{
			int[,] slide = InItem.Slide;
			int num = (RegionNumber == 0) ? 1 : 3;
			int num2 = (RegionNumber == 0) ? 2 : 4;
			string text = "";
			string text2 = "";
			int num3 = 0;
			tbWorkspace.WordWrap = false;
			tbTempSpace.WordWrap = false;

			int itemCount = InItem.LyricsAndNotationsList.Items.Count;

			if (PreviewNotations)
			{
				if (slide[CurSlide, num] <= slide[CurSlide, num2])
				{
					bool flag = false;
					
					for (int i = slide[CurSlide, num]; i <= slide[CurSlide, num2]; i++)
					{
						if (i >= itemCount) continue;
						if (InItem.LyricsAndNotationsList.Items[i].SubItems[3].Text != "")
						{
							flag = true;
							i = slide[CurSlide, num2] + 1;
						}
					}
					string text3 = "";
					for (int i = slide[CurSlide, num]; i <= slide[CurSlide, num2]; i++)
					{
						if (i >= itemCount) continue;
						text = text + InItem.LyricsAndNotationsList.Items[i].SubItems[2].Text + "\n";
						if (flag)
						{
							object obj = text3;
							text3 = string.Concat(obj, "(", Convert.ToInt32(num3), ';', InItem.LyricsAndNotationsList.Items[i].SubItems[3].Text, ")");
						}
						num3++;
					}
					text = CombineLyricsAndNotations(text, text3, InFont, InFont, ref tbWorkspace, ref tbTempSpace) + "\n";
				}
			}
			else if (slide[CurSlide, num] <= slide[CurSlide, num2])
			{
				for (int i = slide[CurSlide, num]; i <= slide[CurSlide, num2]; i++)
				{
					if (i >= itemCount) continue;
					text2 = InItem.LyricsAndNotationsList.Items[i].SubItems[2].Text + "\n";
					SubstituteDashes(ref text2, 0);
					text += text2;
				}
			}
			if (InItem.Type == "B")
			{
				text = text.Replace('\u0098'.ToString(), "");
			}
			return DataUtil.TrimEnd(text) + "\n";
		}

		public static string OldGetSlideContents(SongSettings InItem, int CurSlide, int RegionNumber, Font InFont, bool PreviewNotations)
		{
			int[,] slide = InItem.Slide;
			int num = (RegionNumber == 0) ? 1 : 3;
			int num2 = (RegionNumber == 0) ? 2 : 4;
			string text = "";
			if (PreviewNotations)
			{
				if (slide[CurSlide, num] <= slide[CurSlide, num2])
				{
					bool flag = false;
					for (int i = slide[CurSlide, num]; i <= slide[CurSlide, num2]; i++)
					{
						if (InItem.LyricsAndNotationsList.Items[i].SubItems[3].Text != "")
						{
							flag = true;
							i = slide[CurSlide, num2] + 1;
						}
					}
					for (int i = slide[CurSlide, num]; i <= slide[CurSlide, num2]; i++)
					{
						if (flag)
						{
							text = text + CombineLyricsAndNotations(InItem.LyricsAndNotationsList.Items[i].SubItems[2].Text, InItem.LyricsAndNotationsList.Items[i].SubItems[3].Text, InFont, InFont, ref tbWorkspace, ref tbTempSpace) + "\n";
						}
						text = text + InItem.LyricsAndNotationsList.Items[i].SubItems[2].Text + "\n";
					}
				}
			}
			else if (slide[CurSlide, num] <= slide[CurSlide, num2])
			{
				for (int i = slide[CurSlide, num]; i <= slide[CurSlide, num2]; i++)
				{
					text = text + InItem.LyricsAndNotationsList.Items[i].SubItems[2].Text + "\n";
				}
			}
			if (InItem.Type == "B")
			{
				text = text.Replace('\u0098'.ToString(), "");
			}
			return text;
		}

		public static string FormatNotationString(ListView InListView, string InString, string InNotation, Font MainFont, Font NotationsFont)
		{
			Graphics graphics = InListView.CreateGraphics();
			int num = 0;
			string text = "";
			int num2 = 0;
			string text2 = "i";
			int num3 = (int)graphics.MeasureString(text2, NotationsFont, 1000, StringFormat.GenericTypographic).Width;
			string text3 = text2;
			string text4 = "";
			int num4 = 0;
			string text5 = DataUtil.ExtractOneInfo(ref InNotation, ';');
			string text6 = DataUtil.ExtractOneInfo(ref InNotation, ';');
			while ((text5 != "-1") & (text6 != "-1"))
			{
				text = DataUtil.Left(InString, Convert.ToInt32(text6));
				if (DataUtil.Right(text, 1) == " ")
				{
					text = DataUtil.Left(text, text.Length - 1) + text2;
				}
				num2 = (int)graphics.MeasureString(text, MainFont, 32000, StringFormat.GenericDefault).Width;
				while (graphics.MeasureString(text3, NotationsFont, 32000, StringFormat.GenericDefault).Width < (float)(num2 + num3))
				{
					text3 = DataUtil.Left(text3, text3.Length - 1) + " " + text2;
					num4++;
				}
				text3 = ((text3.Length - 2 < 0 || !(text3[text3.Length - 2].ToString() != " ")) ? (DataUtil.Left(text3, text3.Length - 1) + text5 + text2) : (DataUtil.Left(text3, text3.Length - 1) + " " + text5 + text2));
				if (PB_PrinterSpaces > 0)
				{
					int num5 = num4 / 12;
					for (int i = 1; i <= num4 + num5; i++)
					{
						text4 += " ";
					}
					text4 += text5;
				}
				num4 = 0;
				text5 = DataUtil.ExtractOneInfo(ref InNotation, ';');
				text6 = DataUtil.ExtractOneInfo(ref InNotation, ';');
			}
			if (DataUtil.Right(text3, 1) == text2)
			{
				text3 = DataUtil.Left(text3, text3.Length - 1);
			}
			if (text3 != "")
			{
				if (PB_PrinterSpaces > 0)
				{
					return text4;
				}
				return text3;
			}
			return " ";
		}

		public static string RTFFormatNotationString(string InText, string InNotationString, Font MainFont, Font NotationFont)
		{
			string text = "";
			string text2 = "";
			int num = 0;
			int num2 = 0;
			string text3 = "";
			tbWorkspace.Font = MainFont;
			tbWorkspace.WordWrap = false;
			tbWorkspace.Text = InText;
			tbTempSpace.Font = NotationFont;
			tbTempSpace.WordWrap = false;
			tbTempSpace.Text = "";
			while (InNotationString != "")
			{
				text = InNotationString;
				text2 = DataUtil.ExtractOneInfo(ref text, ';');
				num = Convert.ToInt32(DataUtil.ExtractOneInfo(ref text, ';'));
				InNotationString = text;
				num2 = GetAssociatedLyricsLineCurPosX(ref tbWorkspace, num, 0);
				while (GetAssociatedLyricsLineCurPosX(ref tbTempSpace, tbTempSpace.Text.Length - 1) < num2 - 1)
				{
					text3 += " ";
					tbTempSpace.Text = text3;
					MarkSelectedRTB(ref tbTempSpace, 0, tbTempSpace.Text.Length, 2, MainFont, NotationFont);
				}
				text3 += (((text3.Length > 1) & (DataUtil.Right(text3, 1) != " ")) ? (" " + text2) : text2);
				tbTempSpace.Text = text3;
				MarkSelectedRTB(ref tbTempSpace, 0, tbTempSpace.Text.Length, 2, MainFont, NotationFont);
			}
			return text3;
		}

		public static int GetAssociatedLyricsLineCurPosX(ref RichTextBox IntextBox, int InCurPos)
		{
			return GetAssociatedLyricsLineCurPosX(ref IntextBox, InCurPos, 0);
		}

		public static int GetAssociatedLyricsLineCurPosX(ref RichTextBox IntextBox, int InCurPos, int LyricsCurPosMin)
		{
			return GetAssociatedLyricsLineCurPosX(ref IntextBox, InCurPos, LyricsCurPosMin, 64000);
		}

		public static int GetAssociatedLyricsLineCurPosX(ref RichTextBox IntextBox, int InCurPos, int LyricsCurPosMin, int LyricsCurPosMax)
		{
			if (InCurPos < 0)
			{
				InCurPos = 0;
			}
			if (InCurPos > LyricsCurPosMax - LyricsCurPosMin + 1)
			{
				Point positionFromCharIndex = IntextBox.GetPositionFromCharIndex(LyricsCurPosMax - LyricsCurPosMin + 1);
				Point positionFromCharIndex2 = IntextBox.GetPositionFromCharIndex(LyricsCurPosMax - LyricsCurPosMin);
				return positionFromCharIndex.X + (positionFromCharIndex.X - positionFromCharIndex2.X);
			}
			return IntextBox.GetPositionFromCharIndex(InCurPos + LyricsCurPosMin).X;
		}

		public static string CombineLyricsAndNotations(string InText, string InNotations, Font MainFont, Font NotationFont, ref RichTextBox InWorkspace, ref RichTextBox InTempSpace)
		{
			if ((InNotations == "") | (InText == ""))
			{
				return InText;
			}
			StringBuilder stringBuilder = new StringBuilder();
			InWorkspace.Text = InText;
			MarkSelectedRTB(ref InWorkspace, 0, InWorkspace.Text.Length, 0, MainFont, NotationFont);
			int num = DataUtil.CountLf(InWorkspace.Text);
			int InMin = 0;
			int InMax = 0;
			string text = "";
			int num2 = ListNotationData(InNotations, ref NotationsArray, num);
			for (int i = 0; i < num; i++)
			{
				if (num2 > 0 && NotationsArray[i] != "")
				{
					GetMinMaxfromTextBox(InWorkspace, i, ref InMin, ref InMax);
					InTempSpace.Text = "";
					string text2 = "";
					while (NotationsArray[i].Length > 0)
					{
						text = NotationsArray[i];
						string text3 = DataUtil.ExtractOneInfo(ref text, ';');
						int inCurPos = Convert.ToInt32(DataUtil.ExtractOneInfo(ref text, ';'));
						NotationsArray[i] = text;
						int associatedLyricsLineCurPosX = GetAssociatedLyricsLineCurPosX(ref InWorkspace, inCurPos, InMin, InMax);
						while (GetAssociatedLyricsLineCurPosX(ref InTempSpace, InTempSpace.Text.Length - 1) < associatedLyricsLineCurPosX - 4)
						{
							text2 += " ";
							InTempSpace.Text = text2;
							MarkSelectedRTB(ref InTempSpace, 0, InTempSpace.Text.Length, 2, MainFont, NotationFont);
						}
						text2 += (((text2.Length > 1) & (DataUtil.Right(text2, 1) != " ")) ? (" " + text3) : text3);
						InTempSpace.Text = text2;
						MarkSelectedRTB(ref InTempSpace, 0, InTempSpace.Text.Length, 2, MainFont, NotationFont);
					}
					stringBuilder.Append(InTempSpace.Text + " »\n");
				}
				stringBuilder.Append(InWorkspace.Lines[i] + "\n");
			}
			if (DataUtil.Right(stringBuilder.ToString(), 1) == "\n")
			{
				return DataUtil.Left(stringBuilder.ToString(), stringBuilder.Length - 1);
			}
			return stringBuilder.ToString();
		}

		public static void GetMinMaxfromTextBox(RichTextBox InBox, int InLineNumber, ref int InMin, ref int InMax)
		{
			int num = 0;
			string text = InBox.Text + "\n";
			InMax = -1;
			for (int i = 0; i <= InLineNumber; i++)
			{
				InMin = InMax + 1;
				InMax = text.IndexOf("\n", InMin);
				if (InMax < 0)
				{
					i = InLineNumber;
				}
			}
			InMax--;
		}

		public static void FormatDisplayLyrics(ref SongSettings InItem, bool PrepareSlides, bool UseStoredSequence)
		{
			int num = InItem.UseDefaultFormat ? ShowNotations : InItem.Format.ShowNotations;
			int num2 = (!InItem.UseDefaultFormat) ? InItem.Format.TransposeOffset : ((ShowCapoZero == 1) ? IncrementChord(ref InItem.Capo, 0) : 0);
			int num3 = -1;
			ListViewItem listViewItem = new ListViewItem();
			int num4 = 0;
			InItem.CompleteLyrics = InItem.CompleteLyrics.Replace("\r\n", "\n");
			num4 = DataUtil.CountLf(InItem.CompleteLyrics);
			if (InItem != null && InItem.Capo < 0)
			{
				InItem.Capo = -1;
			}
			if (ShowRunning_ShowNotations == 1)
			{
				num = ((num <= 0) ? 1 : 0);
			}
			if (num == 1)
			{
				if (num2 > 0)
				{
					InItem.Notations = TransposeNotations(InItem.OriginalNotations, InItem.Format.PreviousTransposeOffset, num2, InItem.MusicKey);
				}
				else
				{
					InItem.Notations = InItem.OriginalNotations;
				}
			}
			for (int i = 0; i < 160; i++)
			{
				InItem.VersePresent[i] = false;
				InItem.VerseLineLoc[i, 0] = -1;
				InItem.VerseLineLoc[i, 1] = -1;
				InItem.VerseLineLoc[i, 2] = -1;
				InItem.VerseLineLoc[i, 3] = -1;
				InItem.VerseLineLoc[i, 4] = -1;
			}
			ListView listView = ExtractLyrics(InItem.CompleteLyrics, InItem.Notations);
			InItem.LyricsAndNotationsList.Items.Clear();
			int num5 = -1;
			num3 = -1;
			if (InItem.RotateStyle == 2)
			{
				InItem.SongSequence = InItem.RotateSequence;
			}
			InItem.SongBasicSequence = "";
			for (int i = 0; i < listView.Items.Count; i++)
			{
				num3 = DataUtil.StringToInt(listView.Items[i].Text);
				if (!InItem.VersePresent[num3])
				{
					InItem.VersePresent[num3] = true;
					if (num5 != num3)
					{
						InItem.SongBasicSequence += (char)num3;
						num5 = num3;
					}
				}
			}
			for (int i = 0; i < 160; i++)
			{
				if (!InItem.VersePresent[i])
				{
					continue;
				}
				for (int j = 0; j < listView.Items.Count; j++)
				{
					if (listView.Items[j].SubItems[0].Text == i.ToString() && listView.Items[j].SubItems[1].Text == "1")
					{
						string text = i.ToString();
						listViewItem = InItem.LyricsAndNotationsList.Items.Add(text);
						listViewItem.SubItems.Add("1");
						listViewItem.SubItems.Add(listView.Items[j].SubItems[2].Text);
						listViewItem.SubItems.Add(listView.Items[j].SubItems[3].Text);
						listViewItem.SubItems.Add("");
					}
				}
				for (int j = 0; j < listView.Items.Count; j++)
				{
					if (listView.Items[j].SubItems[0].Text == i.ToString() && listView.Items[j].SubItems[1].Text == "2")
					{
						string text = i.ToString();
						listViewItem = InItem.LyricsAndNotationsList.Items.Add(text);
						listViewItem.SubItems.Add("2");
						listViewItem.SubItems.Add(listView.Items[j].SubItems[2].Text);
						listViewItem.SubItems.Add(listView.Items[j].SubItems[3].Text);
						listViewItem.SubItems.Add("");
						InItem.VersePresent[150] = true;
					}
				}
			}
			for (int i = InItem.LyricsAndNotationsList.Items.Count - 1; i >= 0; i--)
			{
				if (InItem.LyricsAndNotationsList.Items[i].SubItems[2].Text == "")
				{
					InItem.LyricsAndNotationsList.Items[i].Remove();
				}
				else
				{
					i = 0;
				}
			}
			for (int i = InItem.LyricsAndNotationsList.Items.Count - 1; i >= 1; i--)
			{
				if (InItem.LyricsAndNotationsList.Items[i].SubItems[2].Text == "" && InItem.LyricsAndNotationsList.Items[i - 1].SubItems[2].Text == "")
				{
					InItem.LyricsAndNotationsList.Items[i].Remove();
				}
			}
			if (InItem.LyricsAndNotationsList.Items.Count > 0 && InItem.LyricsAndNotationsList.Items[0].SubItems[2].Text == "")
			{
				InItem.LyricsAndNotationsList.Items[0].Remove();
			}
			int num6 = -1;
			num5 = -1;
			for (num3 = 0; num3 < 160; num3++)
			{
				if (!InItem.VersePresent[num3])
				{
					continue;
				}
				for (int i = 0; i < InItem.LyricsAndNotationsList.Items.Count; i++)
				{
					num5 = DataUtil.StringToInt(InItem.LyricsAndNotationsList.Items[i].SubItems[0].Text);
					num6 = DataUtil.StringToInt(InItem.LyricsAndNotationsList.Items[i].SubItems[1].Text);
					if (num3 == num5)
					{
						if (InItem.VerseLineLoc[num3, num6 * 2 - 1] < 0)
						{
							InItem.VerseLineLoc[num3, num6 * 2 - 1] = i;
						}
						InItem.VerseLineLoc[num3, num6 * 2] = i;
					}
				}
			}
			if (PrepareSlides)
			{
				if (UseStoredSequence)
				{
					ValidateSequence(ref InItem);
				}
				else
				{
					InItem.SongSequence = InItem.SongBasicSequence;
				}
				BuildSlides(InItem, InItem.LyricsAndNotationsList, ref InItem.SongSequence, ref InItem.TotalSlides, ref InItem.SongVerses, ref InItem.ChorusSlides, ref InItem.Slide, num);
				if (InItem == null)
				{
				}
			}
			else
			{
				InItem.SongSequence = InItem.SongBasicSequence;
			}
		}

		public static void ValidateSequence(ref SongSettings InItem)
		{
			if (InItem.SongSequence == null)
			{
				InItem.SongSequence = "";
			}
			string text = "";
			for (int i = 0; i < InItem.SongSequence.Length; i++)
			{
				if (InItem.VersePresent[InItem.SongSequence[i]])
				{
					text += DataUtil.Mid(InItem.SongSequence, i, 1);
				}
			}
			if (text.Length > 0)
			{
				InItem.SongSequence = text;
			}
			else
			{
				InItem.SongSequence = InItem.SongBasicSequence;
			}
		}

		public static void GetMinMaxfromTextString(string InString, int InLineNumber, ref int InMin, ref int InMax)
		{
			if (InString == "")
			{
				InMin = -1;
				InMax = -1;
			}
			int num = 0;
			int num2 = 0;
			string text = InString + "\n";
			InMax = -1;
			for (num = 0; num <= InLineNumber; num++)
			{
				InMin = InMax + 1;
				InMax = text.IndexOf("\n", InMin);
				if (InMax < 0)
				{
					InMin = -1;
					InMax = -1;
					num = InLineNumber;
				}
			}
			InMax--;
		}

		public static int GetVerseIndicator(string InString)
		{
			for (int i = 0; i <= 150; i++)
			{
				if (((i < 13) | (i >= 100 && i <= 112) | (i == 150)) && InString.IndexOf(VerseSymbol[i]) >= 0)
				{
					return i;
				}
			}
			return -1;
		}

		public static string TransposeNotations(string InNotationsData, int PreviousTransposeTo, int TransposeTo, string StoredMusicKey)
		{
			string ResultString = "";
			string text = "";
			int num = ExtractOneNotationsLine(ref InNotationsData, ref ResultString);
			if (PreviousTransposeTo > TransposeTo)
			{
				TransposeTo -= 12;
			}
			int flatSharpKey = TransposeKey(ref StoredMusicKey, TransposeTo);
			while (num >= 0)
			{
				object obj = text;
				text = string.Concat(obj, "(", Convert.ToString(num), ';');
				text = text + TransposeOneNotationString(ResultString, TransposeTo, flatSharpKey) + ")";
				num = Convert.ToInt32(ExtractOneNotationsLine(ref InNotationsData, ref ResultString));
			}
			return text;
		}

		public static string TransposeOneNotationString(string NotationString, int TransposeTo, int FlatSharpKey)
		{
			string text = "";
			while (NotationString != "")
			{
				string text2 = TransposeChord(DataUtil.ExtractOneInfo(ref NotationString, ';'), TransposeTo, FlatSharpKey);
				string text3 = DataUtil.ExtractOneInfo(ref NotationString, ';');
				object obj = text;
				text = string.Concat(obj, text2, ';', text3, ';');
			}
			return text;
		}

		public static int ListNotationData(string InString, ref string[] NotationsArray, int MaxLineCount)
		{
			string ResultString = "";
			int num = 0;
			for (int i = 0; i <= MaxLineCount; i++)
			{
				NotationsArray[i] = "";
			}
			int num2 = ExtractOneNotationsLine(ref InString, ref ResultString);
			while (num2 >= 0)
			{
				NotationsArray[num2] = ResultString;
				num2 = ExtractOneNotationsLine(ref InString, ref ResultString);
				num++;
			}
			return num;
		}

		public static int ExtractOneNotationsLine(ref string InString, ref string ResultString)
		{
			if ((InString == null) | (InString == ""))
			{
				ResultString = "";
				return -1;
			}
			int num = 0;
			int num2 = 0;
			string str = "";
			int num3 = -1;
			num = InString.IndexOf("(");
			if (num < 0)
			{
				return -1;
			}
			num2 = InString.IndexOf(")", num);
			if (num2 < num)
			{
				return -1;
			}
			num++;
			str += DataUtil.Mid(InString, num, num2 - num);
			InString = DataUtil.Mid(InString, num2 + ")".Length);
			num3 = DataUtil.StringToInt(DataUtil.ExtractOneInfo(ref str, ';'));
			ResultString = str;
			return num3;
		}

		public static void ChangeNotationLineNumber(ref string InOneNotationLine, int InNewLineNumber)
		{
			if (InOneNotationLine.Length > 0)
			{
				string InString = InOneNotationLine;
				string text = DataUtil.ExtractOneInfo(ref InString, ';');
				int num = Convert.ToInt32(DataUtil.ExtractOneInfo(ref InString, ';'));
				if (text != "-1" && num >= 0)
				{
					InOneNotationLine = text + ';' + InNewLineNumber + ';' + InString;
				}
			}
		}

		public static void BuildSlides(SongSettings InItem, ListView LyricsLists, ref string SongSequence, ref int CurSongMaxSlides, ref int[] SongVerses, ref int[] ChorusSlides, ref int[,] Slide, int InShowNotations)
		{
			int[,] Reg1SubLoc = new int[10000, 4];
			int folderNo = 1;
			for (int i = 0; i <= 9; i++)
			{
				SongVerses[i] = 0;
				ChorusSlides[i] = 0;
			}
			CurSongMaxSlides = 0;
			if (InItem != null)
			{
				InItem.Verse2Present = InItem.VersePresent[2];
				folderNo = InItem.FolderNo;
			}
			Slide[0, 3] = -1;
			foreach (int num in SongSequence)
			{
				bool lastSubScreen = false;
				if (InItem.VersePresent[num])
				{
					int verseScreenCount = GetVerseScreenCount(LyricsLists, InItem.VerseLineLoc[num, 1], InItem.VerseLineLoc[num, 2]);
					if (verseScreenCount > 0)
					{
						try
						{
							for (int j = 1; j <= verseScreenCount; j++)
							{
								int verseScreenLoc = GetVerseScreenLoc(LyricsLists, InItem.VerseLineLoc[num, 1], InItem.VerseLineLoc[num, 2], j);
								if (verseScreenLoc >= 0)
								{
									CurSongMaxSlides++;
									if (j == 1)
									{
										Slide[CurSongMaxSlides, 0] = num;
									}
									else
									{
										Slide[CurSongMaxSlides, 0] = -1;
									}
									Slide[CurSongMaxSlides, 1] = verseScreenLoc;
									Slide[CurSongMaxSlides, 2] = GetVerseScreenEndLoc(LyricsLists, verseScreenLoc, InItem.VerseLineLoc[num, 2]);
									Slide[CurSongMaxSlides, 3] = -1;
									Slide[CurSongMaxSlides, 4] = -1;
									Reg1SubLoc[0, 0] = 1;
									Reg1SubLoc[1, 1] = Slide[CurSongMaxSlides, 1];
									Reg1SubLoc[1, 2] = Slide[CurSongMaxSlides, 2];
									if (InItem != null && Slide[CurSongMaxSlides, 2] >= 0 && InItem.SplitScreens && GetScreensRequired(InItem, InItem.Lyrics[0], ref LyricsLists, Slide[CurSongMaxSlides, 1], Slide[CurSongMaxSlides, 2], ref Reg1SubLoc, InItem.VersePresent[150], 0, folderNo, InShowNotations) > 1)
									{
										CurSongMaxSlides--;
										for (int k = 1; k <= Reg1SubLoc[0, 0]; k++)
										{
											CurSongMaxSlides++;
											if (j == 1 && k == 1)
											{
												Slide[CurSongMaxSlides, 0] = num;
											}
											else
											{
												Slide[CurSongMaxSlides, 0] = -1;
											}
											Slide[CurSongMaxSlides, 1] = Reg1SubLoc[k, 1];
											Slide[CurSongMaxSlides, 2] = Reg1SubLoc[k, 2];
											Slide[CurSongMaxSlides, 3] = -1;
											Slide[CurSongMaxSlides, 4] = -1;
										}
									}
									if ((InItem.VerseLineLoc[num, 3] >= 0) & (InItem.VerseLineLoc[num, 4] >= InItem.VerseLineLoc[num, 3]))
									{
										Slide[0, 3] = 1;
										if (j == verseScreenCount)
										{
											lastSubScreen = true;
										}
										BuildSlidesReg2(LyricsLists, InItem.VerseLineLoc[num, 3], InItem.VerseLineLoc[num, 4], ref Slide, ref CurSongMaxSlides, Reg1SubLoc, j, lastSubScreen);
									}
								}
							}
						}
						catch
						{
						}
					}
					else
					{
						try
						{
							Slide[0, 3] = 2;
							verseScreenCount = GetVerseScreenCount(LyricsLists, InItem.VerseLineLoc[num, 3], InItem.VerseLineLoc[num, 4]);
							for (int j = 1; j <= verseScreenCount; j++)
							{
								int verseScreenLoc = GetVerseScreenLoc(LyricsLists, InItem.VerseLineLoc[num, 3], InItem.VerseLineLoc[num, 4], j);
								if (verseScreenLoc >= 0)
								{
									CurSongMaxSlides++;
									if (j == 1)
									{
										Slide[CurSongMaxSlides, 0] = num;
									}
									else
									{
										Slide[CurSongMaxSlides, 0] = -1;
									}
									Slide[CurSongMaxSlides, 1] = -1;
									Slide[CurSongMaxSlides, 2] = -1;
									Slide[CurSongMaxSlides, 3] = verseScreenLoc;
									Slide[CurSongMaxSlides, 4] = GetVerseScreenEndLoc(LyricsLists, verseScreenLoc, InItem.VerseLineLoc[num, 4]);
									Reg1SubLoc[0, 0] = 1;
									Reg1SubLoc[1, 1] = Slide[CurSongMaxSlides, 3];
									Reg1SubLoc[1, 2] = Slide[CurSongMaxSlides, 4];
									if (InItem != null && Slide[CurSongMaxSlides, 4] >= 0 && InItem.SplitScreens && GetScreensRequired(InItem, InItem.Lyrics[1], ref LyricsLists, Slide[CurSongMaxSlides, 3], Slide[CurSongMaxSlides, 4], ref Reg1SubLoc, Region2Present: true, 1, folderNo, InShowNotations) > 1)
									{
										CurSongMaxSlides--;
										for (int k = 1; k <= Reg1SubLoc[0, 0]; k++)
										{
											CurSongMaxSlides++;
											if (k == 1)
											{
												Slide[CurSongMaxSlides, 0] = num;
											}
											else
											{
												Slide[CurSongMaxSlides, 0] = -1;
											}
											Slide[CurSongMaxSlides, 1] = -1;
											Slide[CurSongMaxSlides, 2] = -1;
											Slide[CurSongMaxSlides, 3] = Reg1SubLoc[k, 1];
											Slide[CurSongMaxSlides, 4] = Reg1SubLoc[k, 2];
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
			}
			try
			{
				int num2 = 1;
				for (int i = CurSongMaxSlides; i >= 1; i--)
				{
					if ((Slide[i, 0] > 0) & (Slide[i, 0] <= 9))
					{
						SongVerses[Slide[i, 0]] = i;
					}
					else if (Slide[i, 0] == 0 && num2 <= 9)
					{
						SongVerses[0] = i;
						ChorusSlides[num2] = i;
						num2++;
					}
				}
			}
			catch
			{
			}
		}

		public static int GetVerseScreenCount(ListView LyricsLists, int StartLoc, int EndLoc)
		{
			if ((StartLoc > EndLoc) | (StartLoc < 0) | (EndLoc < 0))
			{
				return -1;
			}
			int num = 0;
			for (int i = StartLoc; i <= EndLoc; i++)
			{
				if ((LyricsLists.Items[i].SubItems[2].Text == "") | (i == EndLoc))
				{
					num++;
				}
			}
			return num;
		}

		public static int GetVerseScreenLoc(ListView LyricsLists, int StartLoc, int EndLoc, int InScreenNumber)
		{
			if (StartLoc > EndLoc)
			{
				return -1;
			}
			if (StartLoc + 1 <= EndLoc && LyricsLists.Items[StartLoc].SubItems[2].Text == "")
			{
				StartLoc++;
			}
			int num = 1;
			for (int i = StartLoc; i <= EndLoc; i++)
			{
				if (LyricsLists.Items[i].SubItems[2].Text == "")
				{
					num++;
				}
				if (InScreenNumber == num)
				{
					if (LyricsLists.Items[i].SubItems[2].Text != "")
					{
						return i;
					}
					if (i < EndLoc)
					{
						return i + 1;
					}
					return -1;
				}
			}
			return -1;
		}

		public static int GetVerseScreenEndLoc(ListView LyricsLists, int StartLoc, int EndLoc)
		{
			if (StartLoc > EndLoc)
			{
				return -1;
			}
			if (StartLoc == EndLoc)
			{
				return EndLoc;
			}
			if (LyricsLists.Items[StartLoc].SubItems[2].Text == "")
			{
				StartLoc++;
			}
			for (int i = StartLoc; i < EndLoc; i++)
			{
				if (LyricsLists.Items[i].SubItems[2].Text == "")
				{
					return i - 1;
				}
			}
			return EndLoc;
		}

		public static void BuildSlidesReg2(ListView LyricsLists, int StartLoc, int EndLoc, ref int[,] Slide, ref int CurSlideNumber, int[,] Reg1SubLoc, int InScreenNumber, bool LastSubScreen)
		{
			if ((StartLoc > EndLoc) | (Reg1SubLoc[0, 0] < 1))
			{
				return;
			}
			int num = -1;
			int num2 = EndLoc;
			num = GetVerseScreenLoc(LyricsLists, StartLoc, EndLoc, InScreenNumber);
			if (num < 0)
			{
				return;
			}
			num2 = GetVerseScreenEndLoc(LyricsLists, num, EndLoc);
			int num3 = num;
			for (int i = 1; i <= Reg1SubLoc[0, 0]; i++)
			{
				int num4 = CurSlideNumber - Reg1SubLoc[0, 0] + i;
				int num5 = Slide[num4, 2] - Slide[num4, 1] + 1;
				if (num5 <= 0 || num3 < 0)
				{
					continue;
				}
				Slide[num4, 3] = num3;
				if (num3 < 0)
				{
					continue;
				}
				if (num2 - num3 + 1 >= num5)
				{
					if (i == Reg1SubLoc[0, 0])
					{
						Slide[num4, 4] = num2;
						num3 = -1;
					}
					else
					{
						Slide[num4, 4] = num3 + num5 - 1;
						num3 += num5;
					}
				}
				else
				{
					Slide[num4, 4] = num2;
					num3 = -1;
				}
			}
			if (EndLoc <= num2 || !LastSubScreen)
			{
				return;
			}
			num = num2 + 1;
			for (int j = 1; j <= GetVerseScreenCount(LyricsLists, num, EndLoc); j++)
			{
				int verseScreenLoc = GetVerseScreenLoc(LyricsLists, num, EndLoc, j);
				if (verseScreenLoc >= 0)
				{
					CurSlideNumber++;
					Slide[CurSlideNumber, 0] = -1;
					Slide[CurSlideNumber, 1] = -1;
					Slide[CurSlideNumber, 2] = -1;
					Slide[CurSlideNumber, 3] = verseScreenLoc;
					Slide[CurSlideNumber, 4] = GetVerseScreenEndLoc(LyricsLists, verseScreenLoc, EndLoc);
				}
			}
		}

		public static int GetScreensRequired(SongSettings InItem, SongLyrics InLyricsFormat, ref ListView LyricsLists, int StartLoc, int EndLoc, ref int[,] Reg1SubLoc, bool Region2Present, int RegionNumber, int FolderNo, int InShowNotations)
		{
			Graphics g = LyricsLists.CreateGraphics();
			int num = 0;
			Reg1SubLoc[0, 0] = num;
			ListViewItem listViewItem = new ListViewItem();
			int num2 = StartLoc;
			int endline = EndLoc;
			while (GetOneScreen(InItem, InLyricsFormat, ref LyricsLists, g, num2, ref endline, EndLoc, Region2Present, RegionNumber, FolderNo, InShowNotations) > 0)
			{
				num++;
				Reg1SubLoc[num, 1] = num2;
				Reg1SubLoc[num, 2] = endline;
				Reg1SubLoc[num, 3] = endline - num2 + 1;
				Reg1SubLoc[0, 0] = num;
				num2 = endline + 1;
				endline = EndLoc;
			}
			return num;
		}

		public static int GetOneScreen(SongSettings InItem, SongLyrics InLyricsFormat, ref ListView LyricsLists, Graphics g, int startline, ref int endline, int EndLoc, bool Region2Present, int RegionNumber, int FolderNo, int InShowNotations)
		{
			return GetOneScreen(InItem, InLyricsFormat, ref LyricsLists, g, startline, ref endline, EndLoc, Region2Present, RegionNumber, FolderNo, InShowNotations, FitAllIntoOneScreen: false, UseLargestFontSize: false);
		}

		public static int GetOneScreen(SongSettings InItem, SongLyrics InLyricsFormat, ref ListView LyricsLists, Graphics g, int startline, ref int endline, int EndLoc, bool Region2Present, int RegionNumber, int FolderNo, int InShowNotations, bool FitAllIntoOneScreen, bool UseLargestFontSize)
		{
			if (endline < startline)
			{
				return 0;
			}
			Font MainFont = new Font(InLyricsFormat.Font.Name, InLyricsFormat.Font.Size, InLyricsFormat.Font.Style);
			SizeF layoutArea = new SizeF(InLyricsFormat.FS_Width, 32000f);
			string text = (RegionNumber == 0) ? "\n" : "";
			double num = MainFontSpacingFactor[FolderNo, 0] + ((InShowNotations > 0) ? NotationFontFactor : 0.0);
			int num2 = (int)((double)(Region2Present ? InLyricsFormat.FS_Height_R2Bound : InLyricsFormat.FS_Height) / num);
			string text2 = "";
			for (int i = startline; i <= endline; i++)
			{
				text2 = text2 + LyricsLists.Items[i].SubItems[2].Text + "\n";
			}
			if (text2.Length > 1)
			{
				text2 = DataUtil.Left(text2, text2.Length - 1);
			}
			if ((!AutoTextOverflow || InItem.RotateStyle == 2) && InItem.Type != "B")
			{
				ReduceFontToFit(g, text2, ref MainFont, InLyricsFormat.FS_Width, num2, MultiLine: true);
			}
			int num3 = (int)g.MeasureString("A", InLyricsFormat.FS_Font, layoutArea).Height;
			int num4 = 0;
			bool flag = true;
			for (int i = startline; i <= EndLoc; i++)
			{
				if (i == startline)
				{
					ReduceFontToFit(g, LyricsLists.Items[i].SubItems[2].Text, ref MainFont, InLyricsFormat.FS_Width, num2, MultiLine: true);
				}
				num4 += GetLinesRequiredAndAddBreakPlusFont(ref LyricsLists, MainFont, g, i, layoutArea.Width) * num3;
				if ((num4 > num2) & ((AutoTextOverflow & (InItem.RotateStyle != 2)) || InItem.Type == "B"))
				{
					endline = ((i > startline) ? (i - 1) : startline);
					return 1;
				}
			}
			endline = EndLoc;
			return 1;
		}

		public static string ActionWordWrapSpacesAtStart(ref string InString)
		{
			string text = "";
			if (WordWrapIgnoreStartSpaces > 0 && InString.Length > WordWrapIgnoreStartSpaces)
			{
				string text2 = DataUtil.Left(InString, WordWrapIgnoreStartSpaces);
				for (int i = 0; i < text2.Length; i++)
				{
					if (text2[i].ToString() == " ")
					{
						text = text + i.ToString() + ';';
					}
				}
				text2 = text2.Replace(" ", "_");
				InString = text2 + DataUtil.Mid(InString, WordWrapIgnoreStartSpaces);
			}
			return text;
		}

		public static void ActionUndoWordWrapSpacesAtStart(ref string ExtractedText, ref string ReplacedLog)
		{
			int num = DataUtil.StringToInt(DataUtil.ExtractOneInfo(ref ReplacedLog, ';'));
			string text = ExtractedText;
			while (num < ExtractedText.Length && num >= 0)
			{
				text = text.Remove(num, 1);
				text = text.Insert(num, " ");
				num = DataUtil.StringToInt(DataUtil.ExtractOneInfo(ref ReplacedLog, ';'));
			}
			ExtractedText = text;
			if (num > ExtractedText.Length)
			{
				ReplacedLog = num.ToString() + ';' + ReplacedLog;
			}
			int num2 = num;
			text = "";
			while (ReplacedLog != "")
			{
				num = DataUtil.StringToInt(DataUtil.ExtractOneInfo(ref ReplacedLog, ';'));
				text = text + Convert.ToString(num - num2) + ';';
			}
			ReplacedLog = text;
		}

		public static int ReduceFontToFit(Graphics g, string InText, ref Font MainFont, int InWidth, int InHeight)
		{
			return ReduceFontToFit(g, InText, ref MainFont, InWidth, InHeight, MultiLine: false);
		}

		public static int ReduceFontToFit(Graphics g, string InText, ref Font MainFont, int InWidth, int InHeight, bool MultiLine)
		{
			SizeF layoutArea = new SizeF(InWidth, 32000f);
			if (MultiLine)
			{
				while ((g.MeasureString(InText, MainFont, layoutArea).Height > (float)InHeight) & (MainFont.Size > 1f))
				{
					MainFont = new Font(MainFont.Name, MainFont.Size - 1f, MainFont.Style);
				}
			}
			else
			{
				while (MainFont.Size > 1f && (g.MeasureString(InText, MainFont).Width > (float)InWidth || g.MeasureString(InText, MainFont).Height > (float)InHeight))
				{
					MainFont = new Font(MainFont.Name, MainFont.Size - 1f, MainFont.Style);
				}
			}
			return (int)g.MeasureString(InText, MainFont).Height;
		}

		public static int IncreaseFontToLargest(Graphics g, string InText, ref Font MainFont, int InWidth, int InHeight)
		{
			bool OnlyOneDisplayLine = false;
			return IncreaseFontToLargest(g, InText, ref MainFont, InWidth, InHeight, ref OnlyOneDisplayLine);
		}

		public static int IncreaseFontToLargest(Graphics g, string InText, ref Font MainFont, int InWidth, int InHeight, ref bool OnlyOneDisplayLine)
		{
			ReduceFontToFit(g, InText, ref MainFont, InWidth, InHeight);
			SizeF layoutArea = new SizeF(InWidth, 32000f);
			while ((g.MeasureString(InText, MainFont, layoutArea).Height < (float)InHeight) & (MainFont.Size <= 100f))
			{
				MainFont = new Font(MainFont.Name, MainFont.Size + 1f, MainFont.Style);
			}
			MainFont = new Font(MainFont.Name, MainFont.Size - 4f, MainFont.Style);
			int result = (int)g.MeasureString(InText, MainFont, layoutArea).Height;
			if (g.MeasureString(InText, MainFont).Width < (float)InWidth)
			{
				OnlyOneDisplayLine = true;
			}
			return result;
		}

		public static int GetLinesRequiredAndAddBreakPlusFont(ref ListView LyricsLists, Font MainFont, Graphics g, int LyricsIndex, float InWidth)
		{
			string InString = LyricsLists.Items[LyricsIndex].SubItems[2].Text;
			ActionWordWrapSpacesAtStart(ref InString);
			int length = InString.Length;
			if (length == 0)
			{
				return 0;
			}
			int num = 1;
			int num2 = 0;
			bool flag = false;
			int num3 = 1;
			int num4 = 0;
			LyricsLists.Items[LyricsIndex].SubItems[4].Text = "";
			for (int i = 1; i <= length; i++)
			{
				if (DataUtil.Mid(InString, i, 1) == " ")
				{
					num2 = i;
					flag = true;
				}
				else if (g.MeasureString(DataUtil.Mid(InString, num, i - num + 1), MainFont).Width > InWidth)
				{
					if (flag)
					{
						num4++;
						num = num2 + 1;
						ListViewItem.ListViewSubItem listViewSubItem = LyricsLists.Items[LyricsIndex].SubItems[4];
						listViewSubItem.Text = listViewSubItem.Text + Convert.ToString(num - num3) + '>';
						num3 = num;
						flag = false;
					}
					else
					{
						num4++;
						num = i;
						ListViewItem.ListViewSubItem listViewSubItem2 = LyricsLists.Items[LyricsIndex].SubItems[4];
						listViewSubItem2.Text = listViewSubItem2.Text + Convert.ToString(num - num3) + '>';
						num3 = num;
					}
				}
			}
			ListViewItem.ListViewSubItem listViewSubItem3 = LyricsLists.Items[LyricsIndex].SubItems[4];
			listViewSubItem3.Text = listViewSubItem3.Text + Convert.ToString(length - num3 + 1) + '>';
			LyricsLists.Items[LyricsIndex].SubItems[4].Text = Convert.ToString(num4 + 1) + '>' + Convert.ToString(MainFont.Size) + '>' + LyricsLists.Items[LyricsIndex].SubItems[4].Text;
			LyricsArray[LyricsIndex, 2] = LyricsLists.Items[LyricsIndex].SubItems[4].Text;
			return num4 + 1;
		}

		public static bool ShowDBSlide(ref SongSettings InItem, ref ImageTransitionControl PInPictureBox, ref ImageTransitionControl OInPictureBox, bool DoActiveIndicator, ImageTransitionControl.TransitionAction TransitionAction)
		{
			if (InItem.OutputStyleScreen)
			{
				return ShowDBSlide(ref InItem, ref OInPictureBox, DoActiveIndicator, TransitionAction, RedoBackground: false);
			}
			return ShowDBSlide(ref InItem, ref PInPictureBox, DoActiveIndicator, TransitionAction, RedoBackground: false);
		}

		public static bool ShowDBSlide(ref SongSettings InItem, ref ImageTransitionControl InPictureBox, bool DoActiveIndicator, ImageTransitionControl.TransitionAction TransitionAction, bool RedoBackground)
		{
			int num = InItem.UseDefaultFormat ? ShowSongHeadingsAlign : InItem.Format.ShowSongHeadingsAlign;
			int num2 = InItem.UseDefaultFormat ? ShowNotations : InItem.Format.ShowNotations;
			int inShowInterlace = InItem.UseDefaultFormat ? ShowInterlace : InItem.Format.ShowInterlace;
			int num3 = InItem.UseDefaultFormat ? ShowVerticalAlign : InItem.Format.ShowVerticalAlign;
			int transitionType = InItem.UseDefaultFormat ? ShowItemTransition : InItem.Format.ShowItemTransition;
			int transitionType2 = InItem.UseDefaultFormat ? ShowSlideTransition : InItem.Format.ShowSlideTransition;
			InItem.Format.ShowLyrics = (InItem.UseDefaultFormat ? ShowLyrics : InItem.Format.ShowLyrics);
			InItem.Format.ShowSongHeadings = (InItem.UseDefaultFormat ? ShowSongHeadings : InItem.Format.ShowSongHeadings);
			InItem.Format.ShowSongHeadingsAlign = (InItem.UseDefaultFormat ? ShowSongHeadingsAlign : InItem.Format.ShowSongHeadingsAlign);
			int inUseShadowFont = InItem.UseDefaultFormat ? UseShadowFont : InItem.Format.UseShadowFont;
			int inUseOutlineFont = InItem.UseDefaultFormat ? UseOutlineFont : InItem.Format.UseOutlineFont;
			int inHideDisplayPanel = (!InItem.UseDefaultFormat) ? InItem.Format.HideDisplayPanel : ((ShowDataDisplayMode <= 0) ? 1 : 0);
			InItem.CurSlide = ((InItem.CurSlide <= 0) ? 1 : ((InItem.CurSlide > InItem.TotalSlides) ? InItem.TotalSlides : InItem.CurSlide));
			if (InItem.CurSlide > 0)
			{
				if (InItem.Slide[InItem.CurSlide, 0] >= 0)
				{
					if (InItem.CurSlide > 1)
					{
						if (InItem.Slide[InItem.CurSlide, 0] == 0)
						{
							InItem.Lyrics[2].Text = FolderLyricsHeading[InItem.FolderNo, 1];
						}
						else if (InItem.Slide[InItem.CurSlide, 0] == 102)
						{
							InItem.Lyrics[2].Text = FolderLyricsHeading[InItem.FolderNo, 1] + ((FolderLyricsHeading[InItem.FolderNo, 1] != "") ? " (2)" : "");
						}
						else if (InItem.Slide[InItem.CurSlide, 0] == 111)
						{
							InItem.Lyrics[2].Text = FolderLyricsHeading[InItem.FolderNo, 0];
						}
						else if (InItem.Slide[InItem.CurSlide, 0] == 112)
						{
							InItem.Lyrics[2].Text = FolderLyricsHeading[InItem.FolderNo, 0] + ((FolderLyricsHeading[InItem.FolderNo, 0] != "") ? " (2)" : "");
						}
						else if (InItem.Slide[InItem.CurSlide, 0] == 100)
						{
							InItem.Lyrics[2].Text = FolderLyricsHeading[InItem.FolderNo, 2];
						}
						else if (InItem.Slide[InItem.CurSlide, 0] == 103)
						{
							InItem.Lyrics[2].Text = FolderLyricsHeading[InItem.FolderNo, 2] + ((FolderLyricsHeading[InItem.FolderNo, 2] != "") ? " (2)" : "");
						}
						else if (InItem.Slide[InItem.CurSlide, 0] == 101)
						{
							InItem.Lyrics[2].Text = FolderLyricsHeading[InItem.FolderNo, 3];
						}
						else if (InItem.Verse2Present || (InItem.CurSlide > 1 && InItem.Slide[InItem.CurSlide, 0] == 1))
						{
							InItem.Lyrics[2].Text = VerseTitle[InItem.Slide[InItem.CurSlide, 0]];
						}
						else
						{
							InItem.Lyrics[2].Text = "";
						}
					}
					else
					{
						InItem.Lyrics[2].Text = InItem.Title;
					}
				}
				else
				{
					InItem.Lyrics[2].Text = "";
				}
			}
			if (ShowRunning_ShowNotations == 1)
			{
				num2 = ((num2 <= 0) ? 1 : 0);
			}
			num3 = (num3 + ShowRunning_ShowVerticalAlign) % 3;
			if (InItem.FirstShowing)
			{
				InPictureBox.TransitionType = (ImageTransitionControl.TransitionTypes)transitionType;
				if (LicAdminEnforceDisplay)
				{
					InItem.Show_LicAdim = true;
				}
			}
			else
			{
				InPictureBox.TransitionType = (ImageTransitionControl.TransitionTypes)transitionType2;
			}
			if (InItem.FirstShowing || RedoBackground || ComputeTransition(InItem, ref InPictureBox, TransitionAction) != 0)
			{
				if (InItem.Format.MediaTransparent || (ShowLiveCam && InItem.AtLiveScreen))
				{
					SetTransparentBackground(InItem, ref InPictureBox);
				}
				else
				{
					SetShowBackground(InItem, ref InPictureBox, (!(InItem.Type == "G")) ? true : false);
				}
			}
			else if (ShowLiveCam && InItem.AtLiveScreen)
			{
				SetTransparentBackground(InItem, ref InPictureBox);
			}
			if (InItem.Type != "")
			{
				DrawText(ref InItem, ref InPictureBox, InItem.LyricsAndNotationsList, inUseShadowFont, inUseOutlineFont, num2, inShowInterlace, num3, inHideDisplayPanel, TransitionAction, DoActiveIndicator, ClearAll: false);
			}
			return true;
		}

		public static void DrawText(ref SongSettings InItem, ref ImageTransitionControl PPictureBox, ref ImageTransitionControl OPictureBox, ListView LyricsAndNotationsList)
		{
			if (InItem.OutputStyleScreen)
			{
				DrawText(ref InItem, ref OPictureBox, LyricsAndNotationsList, InItem.Format.UseShadowFont, InItem.Format.UseOutlineFont, InItem.Format.ShowNotations, InItem.Format.ShowInterlace, InItem.Format.ShowVerticalAlign, InItem.Format.HideDisplayPanel, (ImageTransitionControl.TransitionAction)InItem.Format.ShowSlideTransition, DoActiveIndicator: false, ClearAll: false);
			}
			else
			{
				DrawText(ref InItem, ref PPictureBox, LyricsAndNotationsList, InItem.Format.UseShadowFont, InItem.Format.UseOutlineFont, InItem.Format.ShowNotations, InItem.Format.ShowInterlace, InItem.Format.ShowVerticalAlign, InItem.Format.HideDisplayPanel, (ImageTransitionControl.TransitionAction)InItem.Format.ShowSlideTransition, DoActiveIndicator: false, ClearAll: false);
			}
		}

		public static void DrawText(ref SongSettings InItem, ref ImageTransitionControl InPictureBox, ListView LyricsAndNotationsList, bool DoActiveIndicator, bool ClearAll)
		{
			DrawText(ref InItem, ref InPictureBox, LyricsAndNotationsList, InItem.Format.UseShadowFont, InItem.Format.UseOutlineFont, InItem.Format.ShowNotations, InItem.Format.ShowInterlace, InItem.Format.ShowVerticalAlign, InItem.Format.HideDisplayPanel, (ImageTransitionControl.TransitionAction)InItem.Format.ShowSlideTransition, DoActiveIndicator, ClearAll);
		}

		public static void DrawText(ref SongSettings InItem, ref ImageTransitionControl InPictureBox, ListView LyricsAndNotationsList, int InUseShadowFont, int InUseOutlineFont, int InShowNotations, int InShowInterlace, int InShowVerticalAlign, int InHideDisplayPanel, ImageTransitionControl.TransitionAction TransitionAction, bool DoActiveIndicator, bool ClearAll)
		{
			bool flag = (InShowInterlace == 1) ? true : false;
			bool liveCamOnShow = false;
			bool fitAllIntoOneScreen = ((!AutoTextOverflow || InItem.RotateStyle == 2) && InItem.Type != "B") ? true : false;
			int num = 0;
			int num2 = 0;
			if ((InPictureBox.Width <= 0) | (InPictureBox.Height <= 0))
			{
				return;
			}
			if (InPictureBox.NewBackgroundPicture == null)
			{
				SetShowBackground(InItem, ref InPictureBox);
			}
			int width = InPictureBox.NewBackgroundPicture.Width;
			int height = InPictureBox.NewBackgroundPicture.Height;
			for (int i = 0; i <= 2; i++)
			{
				InItem.Lyrics[i].FS_TopOffset = 0;
				InItem.Lyrics[i].FS_OneLyricAndNotationHeight = 0;
				InItem.Lyrics[i].FS_InterlaceGapHeight = 0;
				InItem.Lyrics[i].FS_InterlaceLinePattern = "";
			}
			Image image = new Bitmap(width, height, PixelFormat.Format32bppPArgb);
			Graphics g = Graphics.FromImage(image);
			g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
			g.Clear(Color.Transparent);
			//g.Dispose();
			//image.Dispose();

			Image image2 = new Bitmap(width, height, PixelFormat.Format32bppPArgb);
			Graphics graphics = Graphics.FromImage(image2);
			graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
			graphics.Clear(Color.Transparent);
			//graphics.Dispose();
			//image2.Dispose();

			if (InPictureBox.CurrentCombinedImage == null)
			{
				InPictureBox.CurrentBackgroundPicture = (Image)InPictureBox.BackgroundImage.Clone();
				InPictureBox.CurrentTextImage = (Image)InPictureBox.BackgroundImage.Clone();
				InPictureBox.CurrentCombinedImage = (Image)InPictureBox.BackgroundImage.Clone();
			}
			if (InPictureBox.CurrentPanelImage == null)
			{
				InPictureBox.CurrentPanelImage = (Image)InPictureBox.BackgroundImage.Clone();
			}
			if (InPictureBox.NewTextImage == null)
			{
				InPictureBox.NewTextImage = (Image)InPictureBox.BackgroundImage.Clone();
			}
			if (InPictureBox.NewPanelImage == null)
			{
				InPictureBox.NewPanelImage = (Image)InPictureBox.BackgroundImage.Clone();
			}
			ComputeTransition(InItem, ref InPictureBox, TransitionAction);
			if (InPictureBox.TransitionType != 0)
			{
				InPictureBox.CurrentTextImage = (Image)InPictureBox.NewTextImage.Clone();
				InPictureBox.CurrentPanelImage = (Image)InPictureBox.NewPanelImage.Clone();
			}
			if (ShowRunning_UseShadowFont == 1)
			{
				InUseShadowFont = ((InUseShadowFont <= 0) ? 1 : 0);
			}
			if (ShowRunning_UseOutlineFont == 1)
			{
				InUseOutlineFont = ((InUseOutlineFont <= 0) ? 1 : 0);
			}
			if (ShowRunning_ShowInterlace == 1)
			{
				flag = !flag;
			}
			if (ShowLiveCam & InItem.AtLiveScreen)
			{
				ClearAll = true;
				liveCamOnShow = true;
			}
			else if (InItem.OutputStyleScreen && ShowLiveClear)
			{
				ClearAll = true;
			}
			if (InItem.OutputStyleScreen && ShowLiveBlack)
			{
				ClearAll = true;
				g.Clear(BlackScreenColour);
				graphics.Clear(BlackScreenColour);
			}
			if (ClearAll)
			{
				InPictureBox.NewPanelImage = image2;
			}
			else if (InItem.Lyrics[0].Font == null)
			{
				InPictureBox.NewPanelImage = image2;
			}
			else
			{
				if (((ShowDataDisplayMode == 1 && InHideDisplayPanel == 0) | InItem.Show_LicAdim) && InItem.Type != "G")
				{
					DrawDisplayPanel(InItem, InHideDisplayPanel, ref InPictureBox, graphics);
				}
				InPictureBox.NewPanelImage = image2;
				int num3 = (int)InItem.Lyrics[1].FS_Font.Size;
				if ((InItem.Type == "D") | (InItem.Type == "B") | (InItem.Type == "T") | (InItem.Type == "I") | (InItem.Type == "W") | (InItem.Type == "M"))
				{
					int num4 = (InItem.Format.ShowLyrics + ShowRunning_ShowLyrics) % 3;
					int num5 = (InItem.Format.ShowSongHeadings == 1 || (InItem.Format.ShowSongHeadings == 2 && InItem.FirstShowing)) ? 1 : 0;
					num5 = (num5 + ShowRunning_ShowSongHeadings) % 2;
					if (InItem.Type == "M")
					{
						num5 = 0;
					}
					InItem.CurSlideIsVerse = false;
					int num6 = InItem.CurSlide;
					if (InItem.Slide[num6, 0] < 0)
					{
						num6--;
						while (num6 >= 0 && InItem.Slide[num6, 0] < 0)
						{
							num6--;
						}
						if (num6 < 0)
						{
							num6 = 0;
						}
					}
					InItem.CurSlideIsVerse = (InItem.Slide[num6, 0] > 0 && InItem.Slide[num6, 0] < 99);
					bool flag2 = false;
					if ((num4 < 2) | (InItem.Slide[InItem.CurSlide, 1] < 0) | (InItem.Slide[InItem.CurSlide, 3] < 0))
					{
						flag2 = true;
						flag = false;
					}
					int num7 = 0;
					int num8 = 0;
					int num9 = 0;
					Font MainFont = new Font("Microsoft Sans Serif", 30f);
					Font MainFont2 = new Font("Microsoft Sans Serif", 30f);
					Font MainFont3 = new Font("Microsoft Sans Serif", 30f);
					Font NotationsFont = new Font("Microsoft Sans Serif", 30f);
					Font NotationsFont2 = new Font("Microsoft Sans Serif", 30f);
					int num10 = 0;
					int num11 = 0;
					num7 = GetOneRegionHeight(ref InItem, ref InPictureBox, 2, LyricsAndNotationsList, ref g, InUseShadowFont, InUseOutlineFont, InShowNotations, flag2, ref MainFont, ref NotationsFont, InShowInterlace, fitAllIntoOneScreen, UseLargestFontSize);
					if ((num4 == 0) | (num4 == 2))
					{
						num8 = GetOneRegionHeight(ref InItem, ref InPictureBox, 0, LyricsAndNotationsList, ref g, InUseShadowFont, InUseOutlineFont, InShowNotations, flag2, ref MainFont2, ref NotationsFont, InShowInterlace, fitAllIntoOneScreen, UseLargestFontSize);
					}
					if ((num4 == 1) | (num4 == 2))
					{
						num9 = GetOneRegionHeight(ref InItem, ref InPictureBox, 1, LyricsAndNotationsList, ref g, InUseShadowFont, InUseOutlineFont, InShowNotations, flag2, ref MainFont3, ref NotationsFont2, InShowInterlace, fitAllIntoOneScreen, UseLargestFontSize);
						if (flag2)
						{
							InItem.Lyrics[1].FS_Top = InItem.Lyrics[0].FS_Top;
						}
					}
					switch (InShowVerticalAlign)
					{
						case 0:
							num10 = ((num5 <= 0) ? (-num7) : 0);
							break;
						case 1:
							num10 = (InItem.Lyrics[0].FS_Height - (num8 + num9 + num11 + ((num5 <= 0) ? num7 : ((!(InItem.Lyrics[2].Text != "")) ? num7 : 0)))) / 2;
							break;
						case 2:
							num10 = InItem.Lyrics[0].FS_Height - (num8 + num9 + num11);
							break;
					}
					num10 += ShowTopBorderSize;
					if ((num4 == 0) | (num4 == 2))
					{
						DrawOneRegion(ref InItem, ref InPictureBox, 0, LyricsAndNotationsList, ref g, InUseShadowFont, InUseOutlineFont, InShowNotations, flag2, ref MainFont2, ref NotationsFont, num10, flag, InShowVerticalAlign, 0, fitAllIntoOneScreen, UseLargestFontSize);
					}
					if ((num4 == 1) | (num4 == 2))
					{
						DrawOneRegion(ref InItem, ref InPictureBox, 1, LyricsAndNotationsList, ref g, InUseShadowFont, InUseOutlineFont, InShowNotations, flag2, ref MainFont3, ref NotationsFont2, num10, flag, InShowVerticalAlign, num8 + num11, fitAllIntoOneScreen, UseLargestFontSize);
					}
					if (num5 > 0)
					{
						DrawOneRegion(ref InItem, ref InPictureBox, 2, LyricsAndNotationsList, ref g, InUseShadowFont, InUseOutlineFont, InShowNotations, flag2, ref MainFont, ref NotationsFont2, num10, flag, InShowVerticalAlign, 0, fitAllIntoOneScreen, UseLargestFontSize);
					}
				}
				if ((InShowNotations == 1) & (InItem.Capo > 0))
				{
					DrawCapoSettings(InItem, g);
				}
			}
			bool firstShowing = InItem.FirstShowing;
			if (InItem.FirstShowing)
			{
				InPictureBox.TransitionTime = ((ShowRunning && InItem.OutputStyleScreen && !InItem.AtLiveScreen) ? 0.7f : 1.5f);
				InPictureBox.ItemChanged = true;
			}
			else
			{
				InPictureBox.TransitionTime = ((ShowRunning && InItem.OutputStyleScreen && !InItem.AtLiveScreen) ? 0.1f : 0.6f);
				InPictureBox.ItemChanged = false;
			}
			if (!ClearAll)
			{
				InItem.FirstShowing = false;
			}
			if (InPictureBox.TransitionType != 0)
			{
				InPictureBox.CurrentTextImage = (Image)InPictureBox.NewTextImage.Clone();
			}
			InPictureBox.NewTextImage = image;
			if (firstShowing || DoActiveIndicator)
			{
				LoadReferenceAlert(ref InPictureBox, InItem, ClearAll, DoActiveIndicator);
			}
			InPictureBox.Go(TransitionAction, firstShowing, ClearAll, DoActiveIndicator, liveCamOnShow);
			//GC.Collect();
		}

		public static ImageTransitionControl.TransitionTypes ComputeTransition(SongSettings InItem, ref ImageTransitionControl InPictureBox, ImageTransitionControl.TransitionAction TransitionAction)
		{
			bool flag = false;
			if ((ShowLiveCam & InItem.AtLiveScreen) || (InItem.OutputStyleScreen && ShowLiveBlack && TransitionAction != ImageTransitionControl.TransitionAction.AsFade))
			{
				flag = true;
			}
			if (InItem.PrevItemPP || flag)
			{
				InPictureBox.TransitionType = ImageTransitionControl.TransitionTypes.None;
			}
			else if (TransitionAction != ImageTransitionControl.TransitionAction.AsStored)
			{
				if (InItem.FirstShowing)
				{
					InPictureBox.TransitionType = (ImageTransitionControl.TransitionTypes)InItem.Format.ShowItemTransition;
				}
				else
				{
					switch (TransitionAction)
					{
						case ImageTransitionControl.TransitionAction.AsStoredItem:
							InPictureBox.TransitionType = (ImageTransitionControl.TransitionTypes)InItem.Format.ShowItemTransition;
							break;
						case ImageTransitionControl.TransitionAction.None:
							InPictureBox.TransitionType = ImageTransitionControl.TransitionTypes.None;
							break;
						default:
							InPictureBox.TransitionType = (ImageTransitionControl.TransitionTypes)InItem.Format.ShowSlideTransition;
							break;
					}
				}
			}
			return InPictureBox.TransitionType;
		}

		public static int GetOneRegionHeight(ref SongSettings InItem, ref ImageTransitionControl InPictureBox, int RegNum, ListView LyricsAndNotationsList, ref Graphics g, int InUseShadowFont, int InUseOutlineFont, int InShowNotations, bool OnlyOneRegionShown, ref Font MainFont, ref Font NotationsFont, int InterlaceOption, bool FitAllIntoOneScreen, bool UseLargestFontSize)
		{
			if (InItem.LyricsAndNotationsList.Items.Count == 0)
			{
				return 0;
			}
			int num = (int)InItem.Lyrics[RegNum].FS_Font.Size;
			int num2 = (int)((double)num * NotationFontFactor);
			string text = "";
			string text2 = "";
			int num3 = 0;
			int fS_Left = InItem.Lyrics[RegNum].FS_Left;
			int fS_Top = InItem.Lyrics[RegNum].FS_Top;
			int fS_Width = InItem.Lyrics[RegNum].FS_Width;
			int num4 = (InItem.Slide[0, 3] > 0 && !OnlyOneRegionShown) ? InItem.Lyrics[RegNum].FS_Height_R2Bound : InItem.Lyrics[RegNum].FS_Height;
			num3 = InItem.Lyrics[RegNum].FS_Top + InItem.Lyrics[RegNum].FS_Height;
			int fS_Height_R2Bound = InItem.Lyrics[1].FS_Height_R2Bound;
			SizeF layoutArea = new SizeF(fS_Width, 32000f);
			int num5 = 0;
			int num6 = 0;
			double num7 = MainFontSpacingFactor[InItem.FolderNo, (RegNum != 0) ? 1 : 0] + ((InShowNotations > 0) ? NotationFontFactor : 0.0);
			int num8;
			int num9;
			switch (RegNum)
			{
				case 0:
					num8 = InItem.Slide[InItem.CurSlide, 1];
					num9 = InItem.Slide[InItem.CurSlide, 2];
					break;
				case 1:
					num8 = InItem.Slide[InItem.CurSlide, 3];
					num9 = InItem.Slide[InItem.CurSlide, 4];
					break;
				default:
					text = InItem.Lyrics[RegNum].Text;
					MainFont = new Font(InItem.Lyrics[RegNum].Font.Name, num, InItem.CurSlideIsVerse ? InItem.Lyrics[RegNum].Font.Style : InItem.Lyrics[RegNum].ChorusFont.Style);
					if (UseLargestFontSize)
					{
						ActionWordWrapSpacesAtStart(ref text);
					}
					ReduceFontToFit(g, text, ref MainFont, fS_Width, num4, MultiLine: true);
					return num4;
			}
			if ((num8 < 0) | (num9 < 0))
			{
				return 0;
			}
			if (num8 <= num9)
			{
				string text3 = "";
				for (int i = num8; i <= num9; i++)
				{
					text2 = InItem.LyricsAndNotationsList.Items[i].SubItems[2].Text;
					if (UseLargestFontSize)
					{
						ActionWordWrapSpacesAtStart(ref text2);
					}
					text = text + text2 + "\n";
					text3 = InItem.LyricsAndNotationsList.Items[i].SubItems[4].Text;
					num5 += DataUtil.StringToInt(DataUtil.ExtractOneInfo(ref text3, '>', RemoveExtract: false));
					num6++;
				}
				if (text.Length > 1)
				{
					text = DataUtil.Left(text, text.Length - 1);
				}
			}
			else
			{
				num6 = 1;
			}
			num4 = (int)((double)num4 / num7);
			int num10;
			if (UseLargestFontSize)
			{
				MainFont = new Font(InItem.Lyrics[RegNum].Font.Name, num, InItem.CurSlideIsVerse ? InItem.Lyrics[RegNum].Font.Style : InItem.Lyrics[RegNum].ChorusFont.Style);
				bool OnlyOneDisplayLine = false;
				num10 = IncreaseFontToLargest(g, text, ref MainFont, fS_Width, num4, ref OnlyOneDisplayLine);
				NotationsFont = new Font(InItem.Lyrics[RegNum].Font.Name, (!(MainFont.Size >= 2f)) ? 1 : Convert.ToInt32((double)MainFont.Size * NotationFontFactor), InItem.Lyrics[RegNum].Font.Style);
				InItem.Lyrics[RegNum].FS_OneLyricAndNotationHeight = (int)((double)g.MeasureString("A", MainFont, layoutArea).Height * num7);
				return (int)((double)num10 * num7);
			}
			if (num5 > 0)
			{
				string InString = InItem.LyricsAndNotationsList.Items[num8].SubItems[4].Text;
				DataUtil.ExtractOneInfo(ref InString, '>');
				num = Convert.ToInt32(DataUtil.ExtractOneInfo(ref InString, '>'));
				if (num < 1)
				{
					num = (int)InItem.Lyrics[RegNum].Font.Size;
				}
				MainFont = new Font(InItem.Lyrics[RegNum].Font.Name, num, InItem.CurSlideIsVerse ? InItem.Lyrics[RegNum].Font.Style : InItem.Lyrics[RegNum].ChorusFont.Style);
				NotationsFont = new Font(InItem.Lyrics[RegNum].Font.Name, (!(MainFont.Size >= 2f)) ? 1 : Convert.ToInt32((double)MainFont.Size * NotationFontFactor), InItem.Lyrics[RegNum].Font.Style);
				InItem.Lyrics[RegNum].FS_OneLyricAndNotationHeight = (int)((double)g.MeasureString("A", MainFont, layoutArea).Height * num7);
				return InItem.Lyrics[RegNum].FS_OneLyricAndNotationHeight * num5;
			}
			MainFont = new Font(InItem.Lyrics[RegNum].Font.Name, num, InItem.CurSlideIsVerse ? InItem.Lyrics[RegNum].Font.Style : InItem.Lyrics[RegNum].ChorusFont.Style);
			ReduceFontToFit(g, text, ref MainFont, fS_Width, num4, MultiLine: true);
			num10 = num4;
			NotationsFont = new Font(InItem.Lyrics[RegNum].Font.Name, (!(MainFont.Size >= 2f)) ? 1 : Convert.ToInt32((double)MainFont.Size * NotationFontFactor), InItem.Lyrics[RegNum].Font.Style);
			InItem.Lyrics[RegNum].FS_OneLyricAndNotationHeight = (int)((double)g.MeasureString("A", MainFont, layoutArea).Height * num7);
			return (int)((double)num10 * num7);
		}

		public static int DrawOneRegion(ref SongSettings InItem, ref ImageTransitionControl InPictureBox, int RegNum, ListView LyricsAndNotationsList, ref Graphics g, int InUseShadowFont, int InUseOutlineFont, int InShowNotations, bool OnlyOneRegionShown, ref Font MainFont, ref Font NotationsFont, int OffsetAfterAlignment, bool InterlaceOption, int InShowVerticalAlign, int Region1Height, bool FitAllIntoOneScreen, bool UseLargestFontSize)
		{
			if (InItem.LyricsAndNotationsList.Items.Count == 0)
			{
				return 0;
			}
			int HeightOffset = 0;
			int num = -1;
			int num2 = -1;
			string lyricsText = "";
			int fS_Left = InItem.Lyrics[RegNum].FS_Left;
			int num3 = InItem.Lyrics[RegNum].FS_Top;
			int fS_Width = InItem.Lyrics[RegNum].FS_Width;
			int num4 = (InItem.Slide[0, 3] > 0 && !OnlyOneRegionShown) ? InItem.Lyrics[RegNum].FS_Height_R2Bound : InItem.Lyrics[RegNum].FS_Height;
			SizeF layoutArea = new SizeF(fS_Width, 32000f);
			int num5 = (int)((double)g.MeasureString("A", MainFont, layoutArea).Height * MainFontSpacingFactor[InItem.FolderNo, (RegNum != 0) ? 1 : 0]);
			int notationsLineHeight = (int)((double)num5 * ((MainFont.Size >= 2f) ? NotationFontFactor : 1.0));
			int notationsLineTextVOffset = 0;
			string interlaceLinePattern = (RegNum == 1) ? InItem.Lyrics[RegNum].FS_InterlaceLinePattern : "";
			int num6 = 0;
			switch (RegNum)
			{
				case 0:
					num = InItem.Slide[InItem.CurSlide, 1];
					num2 = InItem.Slide[InItem.CurSlide, 2];
					break;
				case 1:
					num = InItem.Slide[InItem.CurSlide, 3];
					num2 = InItem.Slide[InItem.CurSlide, 4];
					break;
				case 2:
					lyricsText = InItem.Lyrics[RegNum].Text;
					num3 += OffsetAfterAlignment;
					DrawOneLine(rect_normal: new RectangleF(fS_Left, num3 + InItem.Lyrics[RegNum].FS_TopOffset, fS_Width, num4), InItem: ref InItem, InPictureBox: ref InPictureBox, InLyrics: InItem.Lyrics[2], RegionNumber: 2, Slide: InItem.Slide, LyricsAndNotationsList: LyricsAndNotationsList, g: ref g, MainFont: MainFont, NotationsFont: NotationsFont, OneLineHeight: num5, NotationsLineHeight: 0, NotationsLineTextVOffset: 0, InHeight: num4, LyricsText: lyricsText, InUseShadowFont: InUseShadowFont, InUseOutlineFont: InUseOutlineFont, InShowNotations: 0);
					return 0;
			}
			if ((num < 0) | (num2 < 0))
			{
				return 0;
			}
			if (OnlyOneRegionShown)
			{
				num3 += OffsetAfterAlignment;
			}
			else
			{
				switch (RegNum)
				{
					case 0:
						num3 += OffsetAfterAlignment;
						if (InterlaceOption)
						{
							InItem.Lyrics[0].FS_InterlaceGapHeight = InItem.Lyrics[1].FS_OneLyricAndNotationHeight;
						}
						break;
					case 1:
						if (InterlaceOption)
						{
							num3 = InItem.Lyrics[0].FS_Top + OffsetAfterAlignment + (int)((double)InItem.Lyrics[0].FS_OneLyricAndNotationHeight * 0.9);
							InItem.Lyrics[1].FS_InterlaceGapHeight = InItem.Lyrics[0].FS_OneLyricAndNotationHeight;
							break;
						}
						num3 = InItem.Lyrics[0].FS_Top + OffsetAfterAlignment + Region1Height + Buffer_LS_Height / 30;
						if (LineBetweenRegions)
						{
							OutputOneLineToScreen(InItem, "<<DrawLine>>", MainFont, g, InItem.Lyrics[RegNum].ForeColour, StringAlignment.Center, InUseShadowFont, InUseOutlineFont, fS_Left, num3 - Buffer_LS_Height / 40, fS_Width, 0);
						}
						break;
				}
			}
			InItem.Lyrics[RegNum].FS_TopOffset = num3;
			RectangleF rect_normal2 = new RectangleF(fS_Left, num3, fS_Width, num4);
			RectangleF rectangleF = new RectangleF(rect_normal2.Left + MainFont.Size / 30f + 1f, rect_normal2.Top + MainFont.Size / 30f + 1f, rect_normal2.Width, rect_normal2.Height);
			if (num <= num2)
			{
				for (int i = num; i <= num2; i++)
				{
					num6 = 0;
					DrawOneLine(ref InItem, ref InPictureBox, InItem.Lyrics[RegNum], RegNum, InItem.Slide, LyricsAndNotationsList, ref g, MainFont, NotationsFont, num5, notationsLineHeight, notationsLineTextVOffset, rect_normal2, num4, lyricsText, InUseShadowFont, InUseOutlineFont, InShowNotations, i, ref HeightOffset, ref num6, InterlaceOption, interlaceLinePattern);
					if (RegNum == 0)
					{
						SongLyrics obj = InItem.Lyrics[1];
						obj.FS_InterlaceLinePattern = obj.FS_InterlaceLinePattern + Convert.ToString(num6) + '>';
					}
				}
			}
			return (int)MainFont.Size;
		}

		public static void DrawOneLine(ref SongSettings InItem, ref ImageTransitionControl InPictureBox, SongLyrics InLyrics, int RegionNumber, int[,] Slide, ListView LyricsAndNotationsList, ref Graphics g, Font MainFont, Font NotationsFont, int OneLineHeight, int NotationsLineHeight, int NotationsLineTextVOffset, RectangleF rect_normal, int InHeight, string LyricsText, int InUseShadowFont, int InUseOutlineFont, int InShowNotations)
		{
			DrawOneLine(ref InItem, ref InPictureBox, InLyrics, RegionNumber, Slide, LyricsAndNotationsList, ref g, MainFont, NotationsFont, OneLineHeight, NotationsLineHeight, NotationsLineTextVOffset, rect_normal, InHeight, LyricsText, InUseShadowFont, InUseOutlineFont, InShowNotations, -1);
		}

		public static void DrawOneLine(ref SongSettings InItem, ref ImageTransitionControl InPictureBox, SongLyrics InLyrics, int RegionNumber, int[,] Slide, ListView LyricsAndNotationsList, ref Graphics g, Font MainFont, Font NotationsFont, int OneLineHeight, int NotationsLineHeight, int NotationsLineTextVOffset, RectangleF rect_normal, int InHeight, string LyricsText, int InUseShadowFont, int InUseOutlineFont, int InShowNotations, int CurLine)
		{
			int HeightOffset = 0;
			int LinesRequired = 0;
			DrawOneLine(ref InItem, ref InPictureBox, InLyrics, RegionNumber, Slide, LyricsAndNotationsList, ref g, MainFont, NotationsFont, OneLineHeight, NotationsLineHeight, NotationsLineTextVOffset, rect_normal, InHeight, LyricsText, InUseShadowFont, InUseOutlineFont, InShowNotations, CurLine, ref HeightOffset, ref LinesRequired, InterlaceOption: false);
		}

		public static void DrawOneLine(ref SongSettings InItem, ref ImageTransitionControl InPictureBox, SongLyrics InLyrics, int RegionNumber, int[,] Slide, ListView LyricsAndNotationsList, ref Graphics g, Font MainFont, Font NotationsFont, int OneLineHeight, int NotationsLineHeight, int NotationsLineTextVOffset, RectangleF rect_normal, int InHeight, string LyricsText, int InUseShadowFont, int InUseOutlineFont, int InShowNotations, int CurLine, ref int HeightOffset)
		{
			int LinesRequired = 0;
			DrawOneLine(ref InItem, ref InPictureBox, InLyrics, RegionNumber, Slide, LyricsAndNotationsList, ref g, MainFont, NotationsFont, OneLineHeight, NotationsLineHeight, NotationsLineTextVOffset, rect_normal, InHeight, LyricsText, InUseShadowFont, InUseOutlineFont, InShowNotations, CurLine, ref HeightOffset, ref LinesRequired, InterlaceOption: false);
		}

		public static void DrawOneLine(ref SongSettings InItem, ref ImageTransitionControl InPictureBox, SongLyrics InLyrics, int RegionNumber, int[,] Slide, ListView LyricsAndNotationsList, ref Graphics g, Font MainFont, Font NotationsFont, int OneLineHeight, int NotationsLineHeight, int NotationsLineTextVOffset, RectangleF rect_normal, int InHeight, string LyricsText, int InUseShadowFont, int InUseOutlineFont, int InShowNotations, int CurLine, ref int HeightOffset, ref int LinesRequired)
		{
			DrawOneLine(ref InItem, ref InPictureBox, InLyrics, RegionNumber, Slide, LyricsAndNotationsList, ref g, MainFont, NotationsFont, OneLineHeight, NotationsLineHeight, NotationsLineTextVOffset, rect_normal, InHeight, LyricsText, InUseShadowFont, InUseOutlineFont, InShowNotations, CurLine, ref HeightOffset, ref LinesRequired, InterlaceOption: false);
		}

		public static void DrawOneLine(ref SongSettings InItem, ref ImageTransitionControl InPictureBox, SongLyrics InLyrics, int RegionNumber, int[,] Slide, ListView LyricsAndNotationsList, ref Graphics g, Font MainFont, Font NotationsFont, int OneLineHeight, int NotationsLineHeight, int NotationsLineTextVOffset, RectangleF rect_normal, int InHeight, string LyricsText, int InUseShadowFont, int InUseOutlineFont, int InShowNotations, int CurLine, ref int HeightOffset, ref int LinesRequired, bool InterlaceOption)
		{
			DrawOneLine(ref InItem, ref InPictureBox, InLyrics, RegionNumber, Slide, LyricsAndNotationsList, ref g, MainFont, NotationsFont, OneLineHeight, NotationsLineHeight, NotationsLineTextVOffset, rect_normal, InHeight, LyricsText, InUseShadowFont, InUseOutlineFont, InShowNotations, CurLine, ref HeightOffset, ref LinesRequired, InterlaceOption, "");
		}

		public static void DrawOneLine(ref SongSettings InItem, ref ImageTransitionControl InPictureBox, SongLyrics InLyrics, int RegionNumber, int[,] Slide, ListView LyricsAndNotationsList, ref Graphics g, Font MainFont, Font NotationsFont, int OneLineHeight, int NotationsLineHeight, int NotationsLineTextVOffset, RectangleF rect_normal, int InHeight, string LyricsText, int InUseShadowFont, int InUseOutlineFont, int InShowNotations, int CurLine, ref int HeightOffset, ref int LinesRequired, bool InterlaceOption, string InterlaceLinePattern)
		{
			StringFormat stringFormat = new StringFormat();
			SizeF sizeF = new SizeF(rect_normal.Width, 32000f);
			int startPos = 0;
			int num = 0;
			int EndExtractedTextPos = -1;
			int R2_MaxLinesPermitted = Convert.ToInt32(DataUtil.ExtractOneInfo(ref InterlaceLinePattern, '>'));
			if (R2_MaxLinesPermitted < 0)
			{
				R2_MaxLinesPermitted = 0;
			}
			string notationsString;
			string InString;
			if (CurLine < 0)
			{
				if (LyricsText == "")
				{
					return;
				}
				notationsString = "";
				InString = "";
			}
			else
			{
				LyricsText = InItem.LyricsAndNotationsList.Items[CurLine].SubItems[2].Text;
				notationsString = InItem.LyricsAndNotationsList.Items[CurLine].SubItems[3].Text;
				InString = InItem.LyricsAndNotationsList.Items[CurLine].SubItems[4].Text;
			}
			if (InterlaceOption && RegionNumber == 1)
			{
				while ((g.MeasureString(LyricsText, MainFont, 100000).Width + 10f > rect_normal.Width * (float)R2_MaxLinesPermitted) & (MainFont.Size > 1f))
				{
					MainFont = new Font(InLyrics.Font.Name, MainFont.Size - 1f, InLyrics.Font.Style);
					NotationsFont = new Font(InLyrics.Font.Name, Convert.ToInt32((double)MainFont.Size * NotationFontFactor), InLyrics.Font.Style);
				}
			}
			LinesRequired++;
			string ReplacedLog = "";
			if (UseLargestFontSize)
			{
				InString = "";
				ReplacedLog = ActionWordWrapSpacesAtStart(ref LyricsText);
			}
			int num2 = Convert.ToInt32(DataUtil.ExtractOneInfo(ref InString, '>'));
			if (num2 > 0)
			{
				if (num2 == 1)
				{
					num2 = 0;
				}
				else
				{
					DataUtil.ExtractOneInfo(ref InString, '>');
					num2 = Convert.ToInt32(DataUtil.ExtractOneInfo(ref InString, '>'));
				}
			}
			string a = DisplayTextByWidthOneLine(InItem, LyricsText, InLyrics, RegionNumber, MainFont, NotationsFont, g, (int)rect_normal.Left, (int)rect_normal.Top, (int)rect_normal.Width, ref HeightOffset, OneLineHeight, NotationsLineHeight, NotationsLineTextVOffset, startPos, InUseShadowFont, InUseOutlineFont, InShowNotations, notationsString, ref EndExtractedTextPos, ref R2_MaxLinesPermitted, InterlaceOption, num2, IsWrappedText: false, ref ReplacedLog);
			if (num2 > 0)
			{
				num2 = Convert.ToInt32(DataUtil.ExtractOneInfo(ref InString, '>'));
			}
			while (a != "" && EndExtractedTextPos > 0)
			{
				startPos = EndExtractedTextPos + 1;
				LinesRequired++;
				a = DisplayTextByWidthOneLine(InItem, LyricsText, InLyrics, RegionNumber, MainFont, NotationsFont, g, (int)rect_normal.Left, (int)rect_normal.Top, (int)rect_normal.Width, ref HeightOffset, OneLineHeight, NotationsLineHeight, NotationsLineTextVOffset, startPos, InUseShadowFont, InUseOutlineFont, InShowNotations, notationsString, ref EndExtractedTextPos, ref R2_MaxLinesPermitted, InterlaceOption, num2, IsWrappedText: true, ref ReplacedLog);
				if (num2 > 0)
				{
					num2 = Convert.ToInt32(DataUtil.ExtractOneInfo(ref InString, '>'));
				}
			}
			if (R2_MaxLinesPermitted > 0 && InterlaceOption)
			{
				HeightOffset += (InLyrics.FS_OneLyricAndNotationHeight + InLyrics.FS_InterlaceGapHeight) * R2_MaxLinesPermitted;
			}
		}

		public static string DisplayTextByWidthOneLine(SongSettings InItem, string InText, SongLyrics InLyrics, int RegionNumber, Font MainFont, Font NotationsFont, Graphics g, int InLeft, int InTop, int InWidth, ref int HeightOffset, int OneLineHeight, int NotationsLineHeight, int NotationsLineTextVOffset, int StartPos, int InUseShadowFont, int InUseOutlineFont, int InShowNotations, string NotationsString, ref int EndExtractedTextPos, ref int R2_MaxLinesPermitted, bool InterlaceOption, int InSetLength, bool IsWrappedText, ref string ReplacedLog)
		{
			int length = InText.Length;
			if (length == 0)
			{
				EndExtractedTextPos = -1;
				return "";
			}
			string ExtractedText = "";
			bool flag = false;
			R2_MaxLinesPermitted--;
			SubDivideOneOutputText(InText, MainFont, NotationsFont, g, InWidth, InShowNotations, NotationsString, length, InSetLength, StartPos, ref EndExtractedTextPos, ref ExtractedText);
			int num = (int)(double)g.MeasureString(ExtractedText, MainFont).Width;
			HeightOffset += ((InShowNotations == 1) ? NotationsLineHeight : 0);
			ActionUndoWordWrapSpacesAtStart(ref ExtractedText, ref ReplacedLog);
			SubstituteDashes(ref ExtractedText, InShowNotations);
			int num2 = OutputOneLineToScreen(InItem, ExtractedText, MainFont, g, InLyrics.ForeColour, InLyrics.TextAlign, InUseShadowFont, InUseOutlineFont, InLeft, InTop + HeightOffset, InWidth, 0, (IsWrappedText && WordWrapLeftAlignIndent && InItem.Type != "I" && RegionNumber != 2) ? true : false);
			if (InShowNotations == 1)
			{
				int num3 = (int)MainFont.Size - 2;
				if (num3 < 1)
				{
					num3 = 1;
				}
				Font font = new Font(MainFont.Name, num3);
				HeightOffset -= NotationsLineHeight;
				int num4 = 0;
				while (NotationsString != "")
				{
					string text = DataUtil.ExtractOneInfo(ref NotationsString, ';');
					int num5 = Convert.ToInt32(DataUtil.ExtractOneInfo(ref NotationsString, ';'));
					if (!(text != "-1") || num5 < 0 || num5 < StartPos)
					{
						continue;
					}
					for (int i = StartPos; i <= EndExtractedTextPos; i++)
					{
						if ((((i == num5) ? 1 : 0) & ((i != EndExtractedTextPos) ? 1 : ((EndExtractedTextPos == length) ? 1 : 0))) != 0)
						{
							int iLen = i - StartPos;
							string text2 = DataUtil.Mid(InText, StartPos, iLen);
							int num6 = (int)g.MeasureString(text2, font).Width;
							OutputOneLineToScreen(InItem, text, NotationsFont, g, InLyrics.ForeColour, StringAlignment.Near, InUseShadowFont, InUseOutlineFont, num2 + num6 + num4, InTop + HeightOffset + NotationsLineTextVOffset, InWidth, 0, (IsWrappedText && WordWrapLeftAlignIndent && InItem.Type != "I") ? true : false);
							num4 += (int)((i == EndExtractedTextPos) ? g.MeasureString(text + "S", NotationsFont).Width : 0f);
							i = EndExtractedTextPos;
						}
					}
				}
				HeightOffset += NotationsLineHeight;
			}
			HeightOffset += (InterlaceOption ? (InLyrics.FS_OneLyricAndNotationHeight + InLyrics.FS_InterlaceGapHeight) : OneLineHeight);
			return DataUtil.Right(InText, length - EndExtractedTextPos - 1);
		}

		public static void SubstituteDashes(ref string ExtractedText, int InShowNotations)
		{
			if (InShowNotations < 1)
			{
				ExtractedText = ExtractedText.Replace(DashesString, DashesStringSubstitute);
				ExtractedText = ExtractedText.Replace(DashesStringSubstitute + "---", "");
				ExtractedText = ExtractedText.Replace(DashesStringSubstitute + "--", "");
				ExtractedText = ExtractedText.Replace(DashesStringSubstitute + "-", "");
				ExtractedText = ExtractedText.Replace(DashesStringSubstitute, "");
			}
		}

		public static void SubDivideOneOutputText(string InText, Font MainFont, Font NotationsFont, Graphics g, int InWidth, int InShowNotations, string NotationsString, int TextLength, int InSetLength, int StartPos, ref int EndExtractedTextPos, ref string ExtractedText)
		{
			int num = -1;
			bool flag = false;
			int num2 = 0;
			if (InSetLength == 0)
			{
				ExtractedText = InText;
				EndExtractedTextPos = TextLength;
				return;
			}
			if (InSetLength > 0)
			{
				for (int i = StartPos; i <= StartPos + InSetLength - 1; i++)
				{
					if (DataUtil.Mid(InText, i, 1) == " ")
					{
						StartPos++;
						InSetLength--;
					}
					else
					{
						i = StartPos + InSetLength;
					}
				}
				ExtractedText = DataUtil.Mid(InText, StartPos, InSetLength);
				EndExtractedTextPos = StartPos + InSetLength - 1;
				return;
			}
			for (int i = StartPos; i <= TextLength; i++)
			{
				if (DataUtil.Mid(InText, i, 1) == " ")
				{
					num = i;
					flag = true;
					if (i == TextLength)
					{
						ExtractedText = DataUtil.Mid(InText, StartPos, TextLength - StartPos + 1);
						EndExtractedTextPos = TextLength;
					}
				}
				else if (g.MeasureString(DataUtil.Mid(InText, StartPos, i - StartPos + 1), MainFont).Width > (float)InWidth)
				{
					if (flag)
					{
						ExtractedText = DataUtil.Mid(InText, StartPos, num - StartPos);
						num2 = num + 1;
						EndExtractedTextPos = num2 - 1;
						i = TextLength;
					}
					else
					{
						ExtractedText = DataUtil.Mid(InText, StartPos, i - StartPos);
						num2 = i;
						EndExtractedTextPos = num2 - 1;
						i = TextLength;
					}
				}
				else if (i == TextLength)
				{
					ExtractedText = DataUtil.Mid(InText, StartPos, TextLength - StartPos + 1);
					EndExtractedTextPos = TextLength;
				}
			}
			if (num2 <= 0)
			{
				return;
			}
			for (int j = EndExtractedTextPos + 1; j <= TextLength; j++)
			{
				if (DataUtil.Mid(InText, j, 1) == " ")
				{
					EndExtractedTextPos++;
				}
				else
				{
					j = TextLength + 1;
				}
			}
		}

		public static int OutputOneLineToScreen(SongSettings InItem, string ExtractedText, Font InFont, Graphics g, Color InColour, StringAlignment alignformat, int InUseShadowFont, int InUseOutlineFont, int x, int y, int w, int h)
		{
			return OutputOneLineToScreen(InItem, ExtractedText, InFont, g, InColour, alignformat, InUseShadowFont, InUseOutlineFont, x, y, w, h, IndentLeftAligned: false);
		}

		public static int OutputOneLineToScreen(SongSettings InItem, string ExtractedText, Font InFont, Graphics g, Color InColour, StringAlignment alignformat, int InUseShadowFont, int InUseOutlineFont, int x, int y, int w, int h, bool IndentLeftAligned)
		{
			GraphicsPath graphicsPath = new GraphicsPath();
			GraphicsPath graphicsPath2 = new GraphicsPath();
			float num = InFont.Size / (float)AdjustedOutlineThreshold;
			string text = "";
			if (InItem.Type == "B" && ExtractedText.IndexOf('\u0098') >= 0)
			{
				string[] array = ExtractedText.Split('\u0098');
				if (array.GetUpperBound(0) > 0)
				{
					text = DataUtil.Trim(array[0]);
					ExtractedText = DataUtil.Trim(array[1]);
				}
			}
			if (num < 1f)
			{
				num = 1f;
			}
			Pen pen = new Pen(BlackScreenColour, num);
			int num2 = (int)((double)InFont.Size * 1.2999999523162842);
			int x2 = x;
			StringFormat stringFormat = new StringFormat();
			switch (alignformat)
			{
				case StringAlignment.Center:
					x += w / 2;
					break;
				case StringAlignment.Far:
					x += w;
					break;
				default:
					if (IndentLeftAligned)
					{
						ExtractedText = "  " + ExtractedText;
					}
					break;
			}
			if (InFont.Size <= 1f)
			{
				return x;
			}
			stringFormat.Alignment = alignformat;
			g.SmoothingMode = SmoothingMode.AntiAlias;
			int num3 = 0;
			int num4 = 0;
			int num5 = num2 / 10;
			if (num5 < 1)
			{
				num5 = 1;
			}
			int num6 = y;
			if (h > 0)
			{
				graphicsPath2.AddString(ExtractedText, new FontFamily(InFont.Name), (int)InFont.Style, num2, new Rectangle(num3 + x, y, w, h), stringFormat);
				int num7 = (int)graphicsPath2.GetBounds().Height + 10;
				num6 += h - num7;
			}
			if (InUseShadowFont > 0)
			{
				int num8 = (int)(InFont.Size / 22f) + 1;
				if (text != "" && alignformat == StringAlignment.Near)
				{
					num3 = DrawSuperScript(graphicsPath, text, InFont, num2, new Rectangle(x + num8, y + num8, (h != 0) ? w : 0, h), AlignLeft: true);
					g.FillPath(new SolidBrush(BlackScreenColour), graphicsPath);
					graphicsPath.Reset();
				}
				if (ExtractedText == "<<DrawLine>>")
				{
					graphicsPath.AddRectangle(new Rectangle(x2, y, w, num5));
				}
				else if (h == 0)
				{
					graphicsPath.AddString(ExtractedText, new FontFamily(InFont.Name), (int)InFont.Style, num2, new Point(num3 + x + num8, y + num8), stringFormat);
				}
				else
				{
					graphicsPath.AddString(ExtractedText, new FontFamily(InFont.Name), (int)InFont.Style, num2, new Rectangle(num3 + x + num8, num6 + num8, w, h), stringFormat);
				}
				g.FillPath(new SolidBrush(BlackScreenColour), graphicsPath);
				num4 = (int)graphicsPath.GetBounds().Left;
				graphicsPath.Reset();
				if (text != "" && alignformat != 0)
				{
					DrawSuperScript(graphicsPath, text, InFont, num2, new Rectangle(num4, y + num8, (h != 0) ? w : 0, h), AlignLeft: false);
					g.FillPath(new SolidBrush(BlackScreenColour), graphicsPath);
					graphicsPath.Reset();
				}
			}
			if (text != "" && alignformat == StringAlignment.Near)
			{
				num3 = DrawSuperScript(graphicsPath, text, InFont, num2, new Rectangle(x, y, (h != 0) ? w : 0, h), AlignLeft: true);
				g.FillPath(new SolidBrush(InColour), graphicsPath);
				if (InUseOutlineFont > 0)
				{
					g.DrawPath(pen, graphicsPath);
				}
				graphicsPath.Reset();
			}
			if (ExtractedText == "<<DrawLine>>")
			{
				graphicsPath.AddRectangle(new Rectangle(x2, y, w, num5));
			}
			else if (h == 0)
			{
				graphicsPath.AddString(ExtractedText, new FontFamily(InFont.Name), (int)InFont.Style, num2, new Point(num3 + x, y), stringFormat);
			}
			else
			{
				graphicsPath.AddString(ExtractedText, new FontFamily(InFont.Name), (int)InFont.Style, num2, new Rectangle(num3 + x, num6, w, h), stringFormat);
			}
			g.FillPath(new SolidBrush(InColour), graphicsPath);
			if (InUseOutlineFont > 0)
			{
				g.DrawPath(pen, graphicsPath);
			}
			num4 = (int)graphicsPath.GetBounds().Left;
			graphicsPath.Reset();
			if (text != "" && alignformat != 0)
			{
				DrawSuperScript(graphicsPath, text, InFont, num2, new Rectangle(num4, y, (h != 0) ? w : 0, h), AlignLeft: false);
				g.FillPath(new SolidBrush(InColour), graphicsPath);
				if (InUseOutlineFont > 0)
				{
					g.DrawPath(pen, graphicsPath);
				}
				graphicsPath.Reset();
			}
			if (InUseOutlineFont > 0)
			{
				g.DrawPath(pen, graphicsPath);
			}
			if (h == 0)
			{
				return num4;
			}
			return num6;
		}

		public static int DrawSuperScript(GraphicsPath pth, string InText, Font InFont, int InFontSize, Rectangle InRectangle, bool AlignLeft)
		{
			StringFormat stringFormat = new StringFormat();
			if (AlignLeft)
			{
				stringFormat.Alignment = StringAlignment.Near;
			}
			else
			{
				stringFormat.Alignment = StringAlignment.Far;
			}
			InFontSize = InFontSize * 7 / 10;
			if (InRectangle.Height == 0)
			{
				pth.AddString(InText, new FontFamily(InFont.Name), (int)InFont.Style, InFontSize, new Point(InRectangle.Left, InRectangle.Top), stringFormat);
			}
			else
			{
				pth.AddString(InText, new FontFamily(InFont.Name), (int)InFont.Style, InFontSize, InRectangle, stringFormat);
			}
			int num = (int)pth.GetBounds().Width;
			int num2 = num / InText.Length * 2 / 3;
			return num + num2;
		}

		public static void ReverseString(ref string InString)
		{
			if (InString.Length != 0)
			{
				string text = "";
				for (int num = InString.Length - 1; num >= 0; num--)
				{
					text += InString[num];
				}
				InString = text;
			}
		}

		public static void DrawCapoSettings(SongSettings InItem, Graphics g)
		{
			SongLyrics songLyrics = InItem.Lyrics[3];
			StringFormat stringFormat = new StringFormat();
			int value = (int)songLyrics.FS_Font.Size;
			int fS_Left = songLyrics.FS_Left;
			int fS_Top = songLyrics.FS_Top;
			int fS_Width = songLyrics.FS_Width;
			int fS_Height = songLyrics.FS_Height;
			stringFormat.Alignment = StringAlignment.Far;
			Font font = new Font(songLyrics.FS_Font.Name, Convert.ToInt32(value), songLyrics.FS_Font.Style);
			int num = fS_Top + fS_Height / 2;
			g.DrawString(layoutRectangle: new RectangleF(fS_Left, num, fS_Width, fS_Height), s: "Capo " + Convert.ToString(InItem.Capo), font: font, brush: new SolidBrush(songLyrics.ForeColour), format: stringFormat);
		}

		public static void DrawDisplayPanel(SongSettings InItem, int InHideDisplayPanel, ref ImageTransitionControl InPictureBox, Graphics g)
		{
			if (InItem.ItemID == "")
			{
				return;
			}
			SongLyrics songLyrics = InItem.Lyrics[3];
			StringFormat stringFormat = new StringFormat();
			int num = (int)songLyrics.FS_Font.Size;
			int num2 = 0;
			int num3 = 0;
			int num4 = 1;
			int num5 = 0;
			Color inColour = (PanelTextColourAsRegion1 > 0) ? songLyrics.ForeColour : PanelTextColour;
			int fS_Left = songLyrics.FS_Left;
			int fS_Top = songLyrics.FS_Top;
			int fS_Width = songLyrics.FS_Width;
			int fS_Height = songLyrics.FS_Height;
			if (PanelBackColourTransparent < 1)
			{
				g.FillRectangle(new SolidBrush(PanelBackColour), 0, fS_Top - 5, Buffer_LS_Width, fS_Height + 5);
			}
			Font inFont = new Font(songLyrics.FS_Font.Name, num, songLyrics.FS_Font.Style);
			num2 = fS_Top;
			string text = InItem.Writer + (((InItem.Writer != "") & (InItem.Copyright != "")) ? "; " : "") + InItem.Copyright;
			string text2 = text;
			text = text2 + ((text == "") ? "" : " ") + InItem.Show_LicAdminInfo1 + ((InItem.Show_LicAdminInfo1 == "") ? "" : " ") + InItem.Show_LicAdminInfo2;
			string text3 = InItem.PrevTitle;
			string text4 = InItem.NextTitle;
			if (ShowDataDisplayMode > 0 && InHideDisplayPanel == 0)
			{
				int num6 = 0;
				int num7 = 0;
				int num8 = 0;
				int num9 = 0;
				int num10 = 0;
				float num11 = 1.18f;
				int num12 = (int)((float)fS_Width * 0.15f);
				int num13 = (int)((float)fS_Width * ((InItem.TotalSlides > 20) ? 0.15f : 0.25f));
				int num14 = (int)((float)fS_Width * (0.34f + (float)(ShowDataDisplayIndicatorsFontSize - 8) * 0.032f));
				int num15 = fS_Width;
				num4 = DisplayFontSize(11, Buffer_LS_Width, 3, 1);
				inFont = new Font(songLyrics.FS_Font.Name, num4, songLyrics.FS_Font.Style);
				int num16 = num4 * fS_Height / num;
				int num17 = fS_Top + (fS_Height - num16);
				if (ShowDataDisplayPrevNext > 0)
				{
					num15 -= num12;
					string text5 = "<<";
					string text6 = ">>";
					if (text3 == "")
					{
						text5 = "...  ";
					}
					if (text4 == "")
					{
						text6 = "...  ";
					}
					num9 = (int)(g.MeasureString(text5, inFont, 10000).Width * num11);
					num10 = (int)(g.MeasureString(text6, inFont, 10000).Width * num11);
					num7 = (int)(g.MeasureString(text3, inFont, 10000).Width * num11);
					num8 = (int)(g.MeasureString(text4, inFont, 10000).Width * num11);
					if (num13 > 0)
					{
						float num18 = 0f;
						if (num7 > 0 && num7 > num13 - num9)
						{
							num18 = (float)(num13 - num9) / (float)num7;
							text3 = DataUtil.Left(text3, (int)(num18 * (float)(text3.Length - 3))) + "...";
						}
						if (num8 > 0 && num8 > num13 - num10)
						{
							num18 = (float)(num13 - num10) / (float)num8;
							text4 = DataUtil.Left(text4, (int)(num18 * (float)(text4.Length - 3))) + "...";
						}
					}
					text3 += text5;
					text4 += text6;
					num7 = (int)(g.MeasureString(text3, inFont, 10000).Width * num11);
					num8 = (int)(g.MeasureString(text4, inFont, 10000).Width * num11);
					num6 = ((num7 > num8) ? num7 : num8);
					stringFormat.Alignment = StringAlignment.Far;
					RectangleF rectangleF = new RectangleF(fS_Width - num7, num17 + 1, num7, num16);
					OutputOneLineToScreen(InItem, text3, inFont, g, inColour, stringFormat.Alignment, ShowDataDisplayFontShadow, ShowDataDisplayFontOutline, (int)rectangleF.Left, (int)rectangleF.Top, (int)rectangleF.Width, 0);
					rectangleF = new RectangleF(fS_Width - num8, num17 + num16 / 2, num8, num16);
					OutputOneLineToScreen(InItem, text4, inFont, g, inColour, stringFormat.Alignment, ShowDataDisplayFontShadow, ShowDataDisplayFontOutline, (int)rectangleF.Left, (int)rectangleF.Top, (int)rectangleF.Width, 0);
					num6 = ((num7 > num8) ? num7 : num8);
				}
				stringFormat.Alignment = StringAlignment.Near;
				int num19 = 4;
				RectangleF rect_slidesinfo = new RectangleF(0f, num17, fS_Width - (num6 + num19), num16);
				num15 = DP_SetSlideIndicators(InItem, ref InPictureBox, ref g, inFont, rect_slidesinfo) - num19;
				num2 = fS_Top + fS_Height / 2;
				num = DisplayFontSize(num, Buffer_LS_Width, 3, 1);
				num4 = ((ShowDataDisplayTitle > 0) ? (num * 7 / 8) : num);
				if (num4 < 1)
				{
					num4 = 1;
				}
				inFont = new Font(songLyrics.FS_Font.Name, num4, songLyrics.FS_Font.Style);
				bool flag = false;
				int num20 = fS_Height;
				if ((ShowDataDisplayCopyright > 0 || InItem.Show_LicAdim) && text != "")
				{
					int num21 = 0;
					int num22 = (int)(g.MeasureString(text, inFont, 10000).Width * num11);
					if (num22 > num15 - num3)
					{
						ReduceFontToFit(g, text, ref inFont, num15 - num3, fS_Height / 2, MultiLine: true);
					}
					RectangleF rectangleF2 = new RectangleF(num3 + num19, num2, num15 - num3, fS_Height / 2 + 2);
					num21 = OutputOneLineToScreen(InItem, text, inFont, g, inColour, stringFormat.Alignment, ShowDataDisplayFontShadow, ShowDataDisplayFontOutline, (int)rectangleF2.Left, (int)rectangleF2.Top, (int)rectangleF2.Width, fS_Height / 2);
					num20 = num21 - fS_Top;
					num2 = num21 - num20;
					flag = true;
				}
				else
				{
					num2 = fS_Top;
				}
				num5 = fS_Height * 10 / 100;
				num20 -= num5;
				num2 += num5;
				inFont = new Font(songLyrics.FS_Font.Name, num * 12 / 10, songLyrics.FS_Font.Style);
				if (ShowDataDisplayTitle > 0)
				{
					string title = InItem.Title;
					int num23 = (int)(g.MeasureString(title, inFont, 10000).Width * num11);
					num20 = ((num20 > 2) ? (num20 - 2) : 2);
					if (num23 > num15)
					{
						ReduceFontToFit(g, title, ref inFont, num15 - num3, num20, (!flag) ? true : false);
					}
					if (inFont.Size >= 25f)
					{
						num2 -= 3;
					}
					else if (inFont.Size <= 21f)
					{
						num2 += 2;
					}
					RectangleF rectangleF3 = new RectangleF(num3 + num19, num2 + (flag ? 3 : (-2)), num15 - num3, num20);
					OutputOneLineToScreen(InItem, title, inFont, g, inColour, stringFormat.Alignment, ShowDataDisplayFontShadow, ShowDataDisplayFontOutline, (int)rectangleF3.Left, (int)rectangleF3.Top, (int)rectangleF3.Width, num20);
				}
				InItem.Show_LicAdim = false;
			}
			else if (InItem.Show_LicAdim)
			{
				num2 = fS_Top + fS_Height / 2;
				RectangleF rectangleF2 = new RectangleF(num3, num2, fS_Width, fS_Height / 2);
				OutputOneLineToScreen(InItem, text, inFont, g, inColour, stringFormat.Alignment, ShowDataDisplayFontShadow, ShowDataDisplayFontOutline, (int)rectangleF2.Left, (int)rectangleF2.Top, (int)rectangleF2.Width, 0);
				InItem.Show_LicAdim = false;
			}
		}

		public static void LoadRegistryMainEditHistory()
		{
			MaxUserEditHistory = RegUtil.GetRegValue("options", "MaxEditHistory", 10);
			if ((MaxUserEditHistory < 0) | (MaxUserEditHistory > AbsoluteMaxHitoryItems))
			{
				MaxUserEditHistory = AbsoluteMaxHitoryItems;
			}
			for (int i = 1; i <= AbsoluteMaxHitoryItems; i++)
			{
				MainEditHistoryList[i, 0] = "";
			}
			TotalMainEditHistory = AbsoluteMaxHitoryItems;
			for (int i = AbsoluteMaxHitoryItems; i >= 1; i--)
			{
				MainEditHistoryList[i, 0] = RegUtil.GetRegValue("maineditlist", i.ToString(), "");
				if (MainEditHistoryList[i, 0] == "")
				{
					TotalMainEditHistory = i - 1;
				}
			}
			ValidateMainHistoryItems();
		}

		public static void ValidateMainHistoryItems()
		{
			int num = 0;
			if ((TotalMainEditHistory < 0) | (TotalMainEditHistory > AbsoluteMaxHitoryItems))
			{
				TotalMainEditHistory = AbsoluteMaxHitoryItems;
			}
			for (int i = 1; i <= TotalMainEditHistory; i++)
			{
				if (GetItemTitle(MainEditHistoryList[i, 0]) != "")
				{
					num++;
					MainEditHistoryList[num, 0] = MainEditHistoryList[i, 0];
				}
			}
			TotalMainEditHistory = num;
			RemoveDuplicateEditorHistoryItems(ref MainEditHistoryList, ref TotalMainEditHistory);
		}

		public static void LoadRegistryEditorEditHistory()
		{
			MaxUserEditHistory = RegUtil.GetRegValue("options", "MaxEditHistory", 10);
			if ((MaxUserEditHistory < 0) | (MaxUserEditHistory > AbsoluteMaxHitoryItems))
			{
				MaxUserEditHistory = AbsoluteMaxHitoryItems;
			}
			for (int i = 1; i <= AbsoluteMaxHitoryItems; i++)
			{
				EditorEditHistoryList[i, 0] = "";
			}
			TotalEditorEditHistory = AbsoluteMaxHitoryItems;
			for (int i = AbsoluteMaxHitoryItems; i >= 1; i--)
			{
				EditorEditHistoryList[i, 0] = RegUtil.GetRegValue("editoreditlist", i.ToString(), "");
				if (EditorEditHistoryList[i, 0] == "")
				{
					TotalEditorEditHistory = i - 1;
				}
			}
			ValidateEditorHistoryItems();
		}

		public static void ValidateEditorHistoryItems()
		{
			int num = 0;
			for (int i = 1; i <= TotalEditorEditHistory; i++)
			{
				if (GetItemTitle(EditorEditHistoryList[i, 0]) != "")
				{
					num++;
					EditorEditHistoryList[num, 0] = EditorEditHistoryList[i, 0];
				}
			}
			TotalEditorEditHistory = num;
			RemoveDuplicateEditorHistoryItems(ref EditorEditHistoryList, ref TotalEditorEditHistory);
		}

		public static void LoadRegistryInfoScreenEditHistory()
		{
			MaxUserEditHistory = RegUtil.GetRegValue("options", "MaxEditHistory", 10);
			if ((MaxUserEditHistory < 0) | (MaxUserEditHistory > AbsoluteMaxHitoryItems))
			{
				MaxUserEditHistory = AbsoluteMaxHitoryItems;
			}
			for (int i = 1; i <= AbsoluteMaxHitoryItems; i++)
			{
				InfoScreenEditHistoryList[i, 0] = "";
			}
			TotalInfoScreenEditHistory = MaxUserEditHistory;
			for (int i = AbsoluteMaxHitoryItems; i >= 1; i--)
			{
				InfoScreenEditHistoryList[i, 0] = RegUtil.GetRegValue("infoscreeneditlist", i.ToString(), "");
				if (InfoScreenEditHistoryList[i, 0] == "")
				{
					TotalInfoScreenEditHistory = i - 1;
				}
			}
			ValidateInfoScreenHistoryItems();
		}

		public static void ValidateInfoScreenHistoryItems()
		{
			int num = 0;
			for (int i = 1; i <= TotalInfoScreenEditHistory; i++)
			{
				if (GetItemTitle(InfoScreenEditHistoryList[i, 0]) != "")
				{
					num++;
					InfoScreenEditHistoryList[num, 0] = InfoScreenEditHistoryList[i, 0];
				}
			}
			TotalInfoScreenEditHistory = num;
			RemoveDuplicateEditorHistoryItems(ref InfoScreenEditHistoryList, ref TotalInfoScreenEditHistory);
		}

		public static string GetItemTitle(string InIDString)
		{
			InIDString = DataUtil.Trim(InIDString);
			if (InIDString == "")
			{
				return "";
			}
			string a = DataUtil.Left(InIDString, 1);
			string text = DataUtil.Mid(InIDString, 1);
			string InFileName = "";
			if (a == "D")
			{
				try
				{
					string fullSearchString = "select * from SONG where songid=" + text;

#if OleDb
					DataTable datatable = DbOleDbController.GetDataTable(ConnectStringMainDB, fullSearchString);
#elif SQLite
					using DataTable datatable = DbController.GetDataTable(ConnectStringMainDB, fullSearchString);
#endif

					if (datatable.Rows.Count>0 && DataUtil.GetDataInt(datatable.Rows[0], "FolderNo") > 0 && FolderUse[DataUtil.GetDataInt(datatable.Rows[0], "FolderNo")] > 0)
					{
						InFileName = DataUtil.GetDataString(datatable.Rows[0], "Title_1");
					}
				}
				catch
				{
				}
			}
			else if (a == "P")
			{
				InFileName = text;
			}
			else if (!(a == "B"))
			{
				if (a == "T")
				{
					InFileName = text;
				}
				else if (a == "I")
				{
					InFileName = text;
					GetDisplayNameOnly(ref InFileName, UpdateByRef: true);
				}
				else if (a == "W")
				{
					InFileName = text;
				}
				else if (a == "M")
				{
					InFileName = text;
				}
			}
			return InFileName;
		}

		public static void SaveMainEditHistoryToRegistry()
		{
			RegUtil.SaveRegValue("options", "MaxEditHistory", MaxUserEditHistory);
			if (TotalMainEditHistory > MaxUserEditHistory)
			{
				TotalMainEditHistory = MaxUserEditHistory;
			}
			for (int i = 1; i <= AbsoluteMaxHitoryItems; i++)
			{
				if (i <= TotalMainEditHistory)
				{
					RegUtil.SaveRegValue("maineditlist", i.ToString(), MainEditHistoryList[i, 0]);
					continue;
				}
				RegUtil.SaveRegValue("maineditlist", i.ToString(), "");
				MainEditHistoryList[i, 0] = "";
			}
		}

		public static void RemoveDuplicateEditorHistoryItems(ref string[,] InEditHistoryList, ref int InTotalEditorEditHistory)
		{
			int num = (InTotalEditorEditHistory <= AbsoluteMaxHitoryItems) ? InTotalEditorEditHistory : AbsoluteMaxHitoryItems;
			bool flag = false;
			int num2 = 0;
			int num3 = 0;
			int num4 = 0;
			for (num3 = 1; num3 <= AbsoluteMaxHitoryItems; num3++)
			{
				TempEditHistoryList[num3, 0] = "";
				TempEditHistoryList[num3, 1] = "";
			}
			for (num3 = 1; num3 <= num; num3++)
			{
				if (num3 == 1)
				{
					num2 = 1;
					TempEditHistoryList[1, 0] = InEditHistoryList[1, 0];
					TempEditHistoryList[1, 1] = InEditHistoryList[1, 1];
					continue;
				}
				flag = false;
				for (num4 = 1; num4 <= num2; num4++)
				{
					if (InEditHistoryList[num3, 0] == TempEditHistoryList[num4, 0])
					{
						flag = true;
					}
				}
				if (!flag)
				{
					num2++;
					TempEditHistoryList[num2, 0] = InEditHistoryList[num3, 0];
					TempEditHistoryList[num2, 1] = InEditHistoryList[num3, 1];
				}
			}
			for (num3 = 1; num3 <= AbsoluteMaxHitoryItems; num3++)
			{
				InEditHistoryList[num3, 0] = TempEditHistoryList[num3, 0];
				InEditHistoryList[num3, 1] = TempEditHistoryList[num3, 1];
			}
			InTotalEditorEditHistory = num2;
		}

		public static void SaveEditorEditHistoryToRegistry()
		{
			RegUtil.SaveRegValue("options", "MaxEditHistory", MaxUserEditHistory);
			if (TotalEditorEditHistory > MaxUserEditHistory)
			{
				TotalEditorEditHistory = MaxUserEditHistory;
			}
			for (int i = 1; i <= AbsoluteMaxHitoryItems; i++)
			{
				if (i <= TotalEditorEditHistory)
				{
					RegUtil.SaveRegValue("editoreditlist", i.ToString(), EditorEditHistoryList[i, 0]);
					continue;
				}
				RegUtil.SaveRegValue("editoreditlist", i.ToString(), "");
				EditorEditHistoryList[i, 0] = "";
			}
		}

		public static void SaveInfoScreenEditHistoryToRegistry()
		{
			RegUtil.SaveRegValue("options", "MaxEditHistory", MaxUserEditHistory);
			if (TotalInfoScreenEditHistory > MaxUserEditHistory)
			{
				TotalInfoScreenEditHistory = MaxUserEditHistory;
			}
			for (int i = 1; i <= AbsoluteMaxHitoryItems; i++)
			{
				if (i <= TotalInfoScreenEditHistory)
				{
					RegUtil.SaveRegValue("infoscreeneditlist", i.ToString(), InfoScreenEditHistoryList[i, 0]);
					continue;
				}
				RegUtil.SaveRegValue("infoscreeneditlist", i.ToString(), "");
				InfoScreenEditHistoryList[i, 0] = "";
			}
		}

		public static int DP_SetSlideIndicators(SongSettings InItem, ref ImageTransitionControl InPic, ref Graphics g, Font tempFont, RectangleF rect_slidesinfo)
		{
			int num = (InItem.Source == ItemSource.WorshipList) ? InItem.CurItemNo : 0;
			int totalWorshipListItems = TotalWorshipListItems;
			Color color = (PanelTextColourAsRegion1 > 0) ? InItem.Lyrics[3].ForeColour : PanelTextColour;
			StringFormat stringFormat = new StringFormat();
			string text = "";
			string text2 = "";
			FontStyle fontStyle = FontStyle.Regular;
			if (ShowDataDisplayFontBold > 0)
			{
				fontStyle |= FontStyle.Bold;
			}
			if (ShowDataDisplayFontItalic > 0)
			{
				fontStyle |= FontStyle.Italic;
			}
			tempFont = new Font(tempFont.Name, tempFont.Size, fontStyle);
			int num2 = (int)g.MeasureString("1", tempFont, 10000).Height;
			int num3 = (int)(rect_slidesinfo.Top + rect_slidesinfo.Height / 20f);
			int num4 = (int)(rect_slidesinfo.Top + rect_slidesinfo.Height / 2f);
			int num5 = num4 - num2 / 20;
			int num6 = 0;
			if (ShowDataDisplaySlides > 0)
			{
				int num7 = DataDisplaySlides(InItem, ref g, tempFont, color, rect_slidesinfo, num3, num4, num5, 0, DisplayIndicators: false);
				num6 = (int)rect_slidesinfo.Width - num7;
				DataDisplaySlides(InItem, ref g, tempFont, color, rect_slidesinfo, num3, num4, num5, num6, DisplayIndicators: true);
				num6 -= (int)g.MeasureString("1", tempFont, 10000).Width;
			}
			else
			{
				num6 = (int)rect_slidesinfo.Width;
			}
			if (ShowDataDisplaySongs > 0)
			{
				text2 = ((num > 0) ? num.ToString() : "A");
				int num8 = (int)g.MeasureString(text2, tempFont, 10000).Width;
				text = totalWorshipListItems.ToString();
				int num9 = (int)g.MeasureString(text, tempFont, 10000).Width;
				num6 -= num9;
				OutputOneLineToScreen(InItem, text2, tempFont, g, color, stringFormat.Alignment, ShowDataDisplayFontShadow, ShowDataDisplayFontOutline, num6 + (num9 - num8) / 2, num3, num6 + num9, 0);
				OutputOneLineToScreen(InItem, text, tempFont, g, color, stringFormat.Alignment, ShowDataDisplayFontShadow, ShowDataDisplayFontOutline, num6, num4, num6 + num9, 0);
				g.DrawLine(new Pen(color), num6, num5, num6 + num9, num5);
			}
			return num6;
		}

		public static int DataDisplaySlides(SongSettings InItem, ref Graphics g, Font tempFont, Color In_TextColour, RectangleF rect_slidesinfo, int VersesSymOffsetTop, int SlidesSymOffsetTop, int RectOffsetTop, int OffsetLeft, bool DisplayIndicators)
		{
			int curSlide = InItem.CurSlide;
			int totalSlides = InItem.TotalSlides;
			string text = "";
			string text2 = "";
			Color color = In_TextColour;
			StringFormat stringFormat = new StringFormat();
			int num = (totalSlides <= 10) ? totalSlides : 11;
			int num2 = (curSlide - 1) / 10 * 10 + 1;
			int num3 = (totalSlides > num2 + 9) ? (num2 + 9) : totalSlides;
			int num4 = num3 - num2 + 1;
			int num5 = OffsetLeft;
			for (int i = 1; i <= 11; i++)
			{
				text2 = "";
				text = "";
				bool flag = false;
				if (i <= num4)
				{
					int num6 = num2 + i - 1;
					text = ((num6 < 10) ? " " : "") + num6.ToString("0") + " ";
					flag = ((curSlide == num6) ? true : false);
					if (((InItem.Slide[num6, 0] >= 0) & (InItem.Slide[num6, 0] <= 10)) | ((InItem.Slide[num6, 0] >= 100) & (InItem.Slide[num6, 0] <= 112)))
					{
						text2 = " " + ConvertSequenceSymbol(SequenceSymbol[InItem.Slide[num6, 0]]);
					}
				}
				else if (i == 11 && totalSlides > num3)
				{
					text = ".. " + totalSlides.ToString("00");
				}
				int num7 = (int)g.MeasureString(text, tempFont, 10000).Width;
				if (DisplayIndicators)
				{
					if (flag)
					{
						g.DrawRectangle(new Pen(In_TextColour), OffsetLeft + 1, RectOffsetTop, num7 + 1, rect_slidesinfo.Height * 2f / 5f);
					}
					else
					{
						color = Color.White;
					}
					color = (flag ? Color.Red : In_TextColour);
					OutputOneLineToScreen(InItem, text2, tempFont, g, color, stringFormat.Alignment, ShowDataDisplayFontShadow, ShowDataDisplayFontOutline, OffsetLeft, VersesSymOffsetTop, OffsetLeft + num7, 0);
					OutputOneLineToScreen(InItem, text, tempFont, g, color, stringFormat.Alignment, ShowDataDisplayFontShadow, ShowDataDisplayFontOutline, OffsetLeft, SlidesSymOffsetTop, OffsetLeft + num7, 0);
				}
				OffsetLeft += num7;
			}
			num5 = OffsetLeft - num5;
			OffsetLeft += (int)g.MeasureString("A", tempFont, 10000).Width;
			return num5;
		}

		public static string ConvertSequenceSymbol(string InSymbol)
		{
			if (InSymbol == SequenceSymbol[103])
			{
				return SequenceSymbol[100].ToUpper();
			}
			if (InSymbol == SequenceSymbol[102])
			{
				return SequenceSymbol[0].ToUpper();
			}
			if (InSymbol == SequenceSymbol[112])
			{
				return SequenceSymbol[111].ToUpper();
			}
			return InSymbol;
		}

		public static bool SaveIndexFile(string InFileName, ref ListView InList, UsageMode InMode, bool SaveAllItems, string InFormatString, string InNotes)
		{
			StringBuilder stringBuilder = new StringBuilder();
			XmlTextWriter xtw = null;

            try
			{
                xtw = new XmlTextWriter(InFileName, Encoding.UTF8);

				xtw.Formatting = Formatting.Indented;
				xtw.WriteStartDocument();
				xtw.WriteStartElement("EasiSlides");
				xtw.WriteStartElement("ListItem");
				WriteXMLSessionHeader(ref xtw, InFormatString, InNotes);
				string text = "";
				string text2 = "";
				string value = "";
				string text3 = "";
				int num = SaveAllItems ? InList.Items.Count : 0;
				for (int i = 1; i <= num; i++)
				{
					text3 = "";
					switch (InMode)
					{
						case UsageMode.Worship:
							{
								text = DataUtil.Trim(InList.Items[i - 1].SubItems[1].Text);
								string text4 = DataUtil.Left(text, 1);
								switch (text4)
								{
									case "D":
										text2 = RemoveMusicSym(DataUtil.Trim(InList.Items[i - 1].Text));
										if (DataUtil.Right(text2, " <Error - Item Not Found>".Length) == " <Error - Item Not Found>")
										{
											text2 = DataUtil.Left(text2, text2.Length - " <Error - Item Not Found>".Length);
										}
										text3 = InList.Items[i - 1].SubItems[7].Text;
										break;
									case "B":
										text2 = DataUtil.Trim(InList.Items[i - 1].Text);
										text = "B" + DataUtil.Mid(text, 1);
										break;
									default:
										text2 = DataUtil.Mid(text, 1);
										text = text4 + "1";
										break;
								}
								value = DataUtil.Trim(InList.Items[i - 1].SubItems[2].Text);
								break;
							}
						case UsageMode.PraiseBook:
							text = DataUtil.Trim(InList.Items[i - 1].SubItems[3].Text);
							text2 = RemoveMusicSym(DataUtil.Trim(InList.Items[i - 1].SubItems[2].Text));
							value = InList.Items[i - 1].SubItems[5].Text;
							break;
					}
					xtw.WriteStartElement("Item");
					xtw.WriteElementString("ItemID", text);
					xtw.WriteElementString("Title1", text2);
					xtw.WriteElementString("Folder", text3);
					xtw.WriteElementString("FormatData", value);
					xtw.WriteEndElement();
				}
				xtw.WriteEndDocument();
				xtw.Flush();
                xtw.Dispose();


                return true;
			}
			catch(Exception ex)
			{
				Console.WriteLine(ex.Message);
				Console.WriteLine(ex);

				if (xtw!= null)
                    xtw.Dispose();

                return false;
			}
		}

		public static void WriteXMLSessionHeader(ref XmlTextWriter xtw, string InFormatString, string InNotes)
		{
			int num = MediaMute + MediaRepeat * 2 + MediaWidescreen * 4;
			StringBuilder stringBuilder = new StringBuilder();
			if (InFormatString != "")
			{
				stringBuilder.Append(InFormatString);
			}
			else
			{
				stringBuilder.Append(Convert.ToString(11) + "=" + Convert.ToString(PanelBackColour.ToArgb()) + '>');
				stringBuilder.Append(Convert.ToString(12) + "=" + Convert.ToString(PanelBackColourTransparent) + '>');
				stringBuilder.Append(Convert.ToString(13) + "=" + Convert.ToString(PanelTextColour.ToArgb()) + '>');
				stringBuilder.Append(Convert.ToString(14) + "=" + Convert.ToString(PanelTextColourAsRegion1) + '>');
				stringBuilder.Append(Convert.ToString(15) + "=" + Convert.ToString(ShowDataDisplayMode + ShowDataDisplaySlides * 2 + ShowDataDisplaySongs * 4 + ShowDataDisplayTitle * 8 + ShowDataDisplayCopyright * 16 + ShowDataDisplayPrevNext * 32) + '>');
				stringBuilder.Append(Convert.ToString(16) + "=" + Convert.ToString((int)(BottomBorderFactor * 100.0)) + '>');
				stringBuilder.Append(Convert.ToString(17) + "=" + ShowDataDisplayFontName + '>');
				stringBuilder.Append(Convert.ToString(18) + "=" + Convert.ToString(ShowDataDisplayFontBold + ShowDataDisplayFontItalic * 2 + ShowDataDisplayFontUnderline * 4 + ShowDataDisplayFontShadow * 8 + ShowDataDisplayFontOutline * 16) + '>');
				stringBuilder.Append(Convert.ToString(19) + "=" + Convert.ToString(ShowDataDisplayIndicatorsFontSize) + '>');
				stringBuilder.Append(Convert.ToString(21) + "=" + Convert.ToString(ShowSongHeadings) + '>');
				stringBuilder.Append(Convert.ToString(23) + "=" + Convert.ToString(ShowSongHeadingsAlign) + '>');
				stringBuilder.Append(Convert.ToString(22) + "=" + Convert.ToString(UseShadowFont * 2 + ShowNotations * 4 + ShowCapoZero * 8 + ShowInterlace * 16 + UseOutlineFont * 32) + '>');
				stringBuilder.Append(Convert.ToString(25) + "=" + Convert.ToString(ShowLyrics) + '>');
				stringBuilder.Append(Convert.ToString(26) + "=" + Convert.ToString(ShowScreenColour[0].ToArgb()) + '>');
				stringBuilder.Append(Convert.ToString(27) + "=" + Convert.ToString(ShowScreenColour[1].ToArgb()) + '>');
				stringBuilder.Append(Convert.ToString(28) + "=" + ShowScreenStyle.ToString() + '>');
				stringBuilder.Append(Convert.ToString(29) + "=" + Convert.ToString(ShowFontColour[0].ToArgb()) + '>');
				stringBuilder.Append(Convert.ToString(30) + "=" + Convert.ToString(ShowFontColour[1].ToArgb()) + '>');
				stringBuilder.Append(Convert.ToString(31) + "=" + Convert.ToString(ShowFontAlign[0, 0]) + '>');
				stringBuilder.Append(Convert.ToString(32) + "=" + Convert.ToString(ShowFontAlign[0, 1]) + '>');
				stringBuilder.Append(Convert.ToString(50) + "=" + Convert.ToString(MediaOption) + '>');
				stringBuilder.Append(Convert.ToString(51) + "=" + MediaLocation + '>');
				stringBuilder.Append(Convert.ToString(52) + "=" + Convert.ToString(MediaVolume) + '>');
				stringBuilder.Append(Convert.ToString(53) + "=" + Convert.ToString(MediaBalance) + '>');
				stringBuilder.Append(Convert.ToString(54) + "=" + num.ToString() + '>');
				stringBuilder.Append(Convert.ToString(55) + "=" + MediaCaptureDeviceNumber.ToString() + '>');
				stringBuilder.Append(Convert.ToString(61) + "=" + BackgroundPicture + '>');
				stringBuilder.Append(Convert.ToString(62) + "=" + Convert.ToString((int)BackgroundMode) + '>');
				stringBuilder.Append(Convert.ToString(63) + "=" + Convert.ToString(ShowVerticalAlign) + '>');
				stringBuilder.Append(Convert.ToString(64) + "=" + Convert.ToString(ShowLeftMargin[0]) + '>');
				stringBuilder.Append(Convert.ToString(65) + "=" + Convert.ToString(ShowRightMargin[0]) + '>');
				stringBuilder.Append(Convert.ToString(66) + "=" + Convert.ToString(ShowBottomMargin[0]) + '>');
				stringBuilder.Append(Convert.ToString(72) + "=" + GlobalImageCanvas.GetTransitionText(ShowItemTransition) + '>');
				stringBuilder.Append(Convert.ToString(73) + "=" + GlobalImageCanvas.GetTransitionText(ShowSlideTransition) + '>');
				for (int i = 0; i < 8; i++)
				{
					int num2 = 101 + i * 5;
					stringBuilder.Append(num2.ToString() + "=" + Convert.ToString(PB_ShowWords[i] + PB_WordsBold[i] * 2 + PB_WordsItalic[i] * 4 + PB_WordsUnderline[i] * 8) + '>');
					stringBuilder.Append(Convert.ToString(num2 + 1) + "=" + PB_WordsSize[i].ToString() + '>');
					stringBuilder.Append(Convert.ToString(num2 + 2) + "=" + Convert.ToString(PB_WordsColour[i].ToArgb()) + '>');
				}
				stringBuilder.Append(Convert.ToString(151) + "=" + Convert.ToString(PB_ShowHeadings[0] + PB_ShowHeadings[1] * 2 + PB_ShowHeadings[2] * 4 + PB_ShowHeadings[3] * 8) + '>');
				stringBuilder.Append(Convert.ToString(153) + "=" + Convert.ToString(PB_LyricsPattern) + '>');
				stringBuilder.Append(Convert.ToString(154) + "=" + Convert.ToString(PB_ShowSection) + '>');
				stringBuilder.Append(Convert.ToString(155) + "=" + Convert.ToString(PB_ShowColumns) + '>');
				stringBuilder.Append(Convert.ToString(156) + "=" + Convert.ToString(PB_PageSize) + '>');
				stringBuilder.Append(Convert.ToString(170) + "=" + Convert.ToString(PB_Spacing[0]) + '>');
				stringBuilder.Append(Convert.ToString(171) + "=" + Convert.ToString(PB_Spacing[1]) + '>');
				stringBuilder.Append(Convert.ToString(172) + "=" + Convert.ToString(PB_ShowScreenBreaks) + '>');
				stringBuilder.Append(Convert.ToString(173) + "=" + Convert.ToString(PB_OneSongPerPage) + '>');
				stringBuilder.Append(Convert.ToString(174) + "=" + Convert.ToString(PB_CJKGroupStyle) + '>');
				stringBuilder.Append(Convert.ToString(180) + "=" + Convert.ToString(PB_ShowNotations + PB_ShowTiming * 2 + PB_ShowKey * 4 + PB_ShowCapo * 8 + PB_CapoZero * 16) + '>');
			}
			xtw.WriteStartElement("ListHeader");
			xtw.WriteElementString("SystemID", SystemID);
			xtw.WriteElementString("FormatData", stringBuilder.ToString());
			xtw.WriteElementString("Notes", InNotes);
			xtw.WriteEndElement();
		}

		public static void SaveIndexFileOld(string OutFileName, ref ListView InList, UsageMode InMode)
		{
			BinaryWriter w = null;

            try
			{
				using FileStream fileStream = new FileStream(OutFileName, FileMode.Create);
				
				w = new BinaryWriter(fileStream);
				
				w.Write(byte.MaxValue);
				w.Write((byte)254);

				string inString = "[";
				FileUtil.WriteStringToBinaryFile(ref w, inString);
				inString = '\u0001' + "=" + Convert.ToString(320) + '>';
				FileUtil.WriteStringToBinaryFile(ref w, inString);
				inString = '\u0002' + "=" + Convert.ToString(PanelBackColour.ToArgb()) + '>';
				FileUtil.WriteStringToBinaryFile(ref w, inString);
				inString = '\u0003' + "=" + Convert.ToString(PanelBackColourTransparent) + '>';
				FileUtil.WriteStringToBinaryFile(ref w, inString);
				inString = '\u0004' + "=" + Convert.ToString(PanelTextColour.ToArgb()) + '>';
				FileUtil.WriteStringToBinaryFile(ref w, inString);
				inString = '\u0005' + "=" + Convert.ToString(PanelTextColourAsRegion1) + '>';
				FileUtil.WriteStringToBinaryFile(ref w, inString);
				inString = '\u0006' + "=" + Convert.ToString(ShowDataDisplayMode + ShowDataDisplaySlides * 2 + ShowDataDisplaySongs * 4 + ShowDataDisplayTitle * 8 + ShowDataDisplayCopyright * 16 + ShowDataDisplayPrevNext * 32) + '>';
				FileUtil.WriteStringToBinaryFile(ref w, inString);
				inString = '\a' + "=" + SystemID + ":" + '>';
				FileUtil.WriteStringToBinaryFile(ref w, inString);
				inString = '\b' + "=" + Convert.ToString(ShowSongHeadings + UseShadowFont * 2 + ShowNotations * 4 + ShowCapoZero * 8 + ShowInterlace * 16 + UseOutlineFont * 32) + '>';
				FileUtil.WriteStringToBinaryFile(ref w, inString);
				inString = '\t' + "=" + Convert.ToString(ShowLyrics) + '>';
				FileUtil.WriteStringToBinaryFile(ref w, inString);
				inString = '\n' + "=" + Convert.ToString(ShowScreenColour[0].ToArgb()) + ":" + Convert.ToString(ShowScreenColour[1].ToArgb()) + ":" + ShowScreenStyle.ToString() + '>';
				FileUtil.WriteStringToBinaryFile(ref w, inString);
				inString = '\f' + "=" + Convert.ToString(ShowFontColour[0].ToArgb()) + '>';
				FileUtil.WriteStringToBinaryFile(ref w, inString);
				inString = '\r' + "=" + Convert.ToString(ShowFontColour[1].ToArgb()) + '>';
				FileUtil.WriteStringToBinaryFile(ref w, inString);
				inString = '\u000e' + "=" + Convert.ToString(ShowFontAlign[0, 0]) + '>';
				FileUtil.WriteStringToBinaryFile(ref w, inString);
				inString = '\u000f' + "=" + Convert.ToString(ShowFontAlign[0, 1]) + '>';
				FileUtil.WriteStringToBinaryFile(ref w, inString);
				inString = '\u001a' + "=" + BackgroundPicture + '>';
				FileUtil.WriteStringToBinaryFile(ref w, inString);
				inString = '\u001b' + "=" + Convert.ToString((int)BackgroundMode) + '>';
				FileUtil.WriteStringToBinaryFile(ref w, inString);
				inString = '\u001c' + "=" + Convert.ToString(ShowVerticalAlign) + '>';
				FileUtil.WriteStringToBinaryFile(ref w, inString);
				inString = '\u001d' + "=" + Convert.ToString(ShowLeftMargin[0]) + '>';
				FileUtil.WriteStringToBinaryFile(ref w, inString);
				inString = '\u001e' + "=" + Convert.ToString(ShowRightMargin[0]) + '>';
				FileUtil.WriteStringToBinaryFile(ref w, inString);
				inString = '\u001f' + "=" + Convert.ToString(ShowBottomMargin[0]) + '>';
				FileUtil.WriteStringToBinaryFile(ref w, inString);
				inString = '!' + "=" + GlobalImageCanvas.GetTransitionText(ShowItemTransition) + '>';
				FileUtil.WriteStringToBinaryFile(ref w, inString);
				inString = '"' + "=" + GlobalImageCanvas.GetTransitionText(ShowSlideTransition) + '>';
				FileUtil.WriteStringToBinaryFile(ref w, inString);
				for (int i = 0; i <= 6; i++)
				{
					int num = 141 + i * 5;
					inString = (char)num + "=" + Convert.ToString(PB_ShowWords[i] + PB_WordsBold[i] * 2 + PB_WordsItalic[i] * 4 + PB_WordsUnderline[i] * 8) + '>';
					FileUtil.WriteStringToBinaryFile(ref w, inString);
					inString = (char)(num + 1) + "=" + Convert.ToString(PB_WordsSize[i]) + '>';
					FileUtil.WriteStringToBinaryFile(ref w, inString);
				}
				inString = 'µ' + "=" + Convert.ToString(PB_ShowHeadings[0] + PB_ShowHeadings[1] * 2 + PB_ShowHeadings[2] * 4 + PB_ShowHeadings[3] * 8) + '>';
				FileUtil.WriteStringToBinaryFile(ref w, inString);
				inString = '¶' + "=" + Convert.ToString(PB_LyricsPattern) + '>';
				FileUtil.WriteStringToBinaryFile(ref w, inString);
				inString = '·' + "=" + Convert.ToString(PB_ShowSection) + '>';
				FileUtil.WriteStringToBinaryFile(ref w, inString);
				inString = '\u00b8' + "=" + Convert.ToString(PB_ShowColumns) + '>';
				FileUtil.WriteStringToBinaryFile(ref w, inString);
				inString = '¹' + "=" + Convert.ToString(PB_Spacing[0]) + '>';
				FileUtil.WriteStringToBinaryFile(ref w, inString);
				inString = 'º' + "=" + Convert.ToString(PB_Spacing[1]) + '>';
				FileUtil.WriteStringToBinaryFile(ref w, inString);
				inString = '»' + "=" + Convert.ToString(PB_ShowScreenBreaks) + '>';
				FileUtil.WriteStringToBinaryFile(ref w, inString);
				inString = 'Ã' + "=" + Convert.ToString(PB_CJKGroupStyle) + '>';
				FileUtil.WriteStringToBinaryFile(ref w, inString);
				inString = 'Ä' + "=" + Convert.ToString(PB_ShowNotations + PB_ShowTiming * 2 + PB_ShowKey * 4 + PB_ShowCapo * 8 + PB_CapoZero * 16) + '>';
				FileUtil.WriteStringToBinaryFile(ref w, inString);
				inString = "]";
				FileUtil.WriteStringToBinaryFile(ref w, inString);
				string text = "";
				string text2 = "";
				string text3 = "";
				for (int i = 1; i <= InList.Items.Count; i++)
				{
					if (InMode == UsageMode.Worship)
					{
						text = DataUtil.Trim(InList.Items[i - 1].SubItems[1].Text);
						string a = DataUtil.Left(text, 1);
						if (a == "D")
						{
							text2 = RemoveMusicSym(DataUtil.Trim(InList.Items[i - 1].Text));
							if (DataUtil.Right(text2, " <Error - Item Not Found>".Length) == " <Error - Item Not Found>")
							{
								text2 = DataUtil.Left(text2, text2.Length - " <Error - Item Not Found>".Length);
							}
							text2 += ((InList.Items[i - 1].SubItems[7].Text == "") ? "" : (":" + InList.Items[i - 1].SubItems[7].Text));
						}
						else if (a == "P")
						{
							text2 = DataUtil.Mid(text, 1);
							text = "P1";
						}
						else if (a == "B")
						{
							text2 = DataUtil.Trim(InList.Items[i - 1].Text);
							text = "B" + DataUtil.Mid(text, 1);
						}
						else if (a == "T")
						{
							text2 = DataUtil.Mid(text, 1);
							text = "T1";
						}
						else if (a == "I")
						{
							text2 = DataUtil.Mid(text, 1);
							text = "I1";
						}
						else if (a == "W")
						{
							text2 = DataUtil.Mid(text, 1);
							text = "W1";
						}
						else if (a == "M")
						{
							text2 = DataUtil.Mid(text, 1);
							text = "M1";
						}
						text3 = DataUtil.Trim(InList.Items[i - 1].SubItems[2].Text);
					}
					else
					{
						text = DataUtil.Trim(InList.Items[i - 1].SubItems[3].Text);
						text2 = RemoveMusicSym(DataUtil.Trim(InList.Items[i - 1].SubItems[2].Text));
						text3 = InList.Items[i - 1].SubItems[5].Text;
					}
					inString = text + "\\" + text2 + "\\" + '*' + text3 + '>';
					FileUtil.WriteStringToBinaryFile(ref w, inString);
				}
				w.Flush();
				w.Dispose();
				//fileStream.Close();
			}
			catch
			{
				if(w != null)
					w.Dispose();

            }
		}

		public static void ReMapKeyBoard(ref Keys InKey)
		{
			if (InKey == Keys.Home)
			{
				if (KeyBoardOption == 1)
				{
					InKey = Keys.Left;
				}
			}
			else if (InKey == Keys.Prior)
			{
				if (KeyBoardOption == 1)
				{
					InKey = Keys.Up;
				}
			}
			else if (InKey == Keys.Next)
			{
				if (KeyBoardOption == 1)
				{
					InKey = Keys.Down;
				}
			}
			else if (InKey == Keys.End)
			{
				if (KeyBoardOption == 1)
				{
					InKey = Keys.Right;
				}
			}
			else if (InKey == Keys.Left)
			{
				if (KeyBoardOption == 1)
				{
					InKey = Keys.Home;
				}
			}
			else if (InKey == Keys.Up)
			{
				if (KeyBoardOption == 1)
				{
					InKey = Keys.Prior;
				}
			}
			else if (InKey == Keys.Down)
			{
				if (KeyBoardOption == 1)
				{
					InKey = Keys.Next;
				}
			}
			else if (InKey == Keys.Right && KeyBoardOption == 1)
			{
				InKey = Keys.End;
			}
		}

		public static int ImplementSlideMovement(ref int InCurSlide, int InCurMaxSlide, Keys InKey, int InSlideNo)
		{
			switch (InKey)
			{
				case Keys.Left:
					InCurSlide = 1;
					break;
				case Keys.Up:
					InCurSlide = ((InCurSlide <= 2) ? 1 : (InCurSlide - 1));
					break;
				case Keys.Down:
					InCurSlide = ((InCurSlide < InCurMaxSlide) ? (InCurSlide + 1) : InCurMaxSlide);
					break;
				case Keys.Right:
					InCurSlide = InCurMaxSlide;
					break;
				case Keys.None:
					InCurSlide = InSlideNo;
					break;
			}
			return InCurSlide;
		}

		public static Keys ReMapKeyDirectionToPowerpoint(KeyDirection InDirection)
		{
			switch (InDirection)
			{
				case KeyDirection.FirstOne:
					return Keys.Left;
				case KeyDirection.PrevOne:
					return Keys.Up;
				case KeyDirection.NextOne:
					return Keys.Down;
				case KeyDirection.LastOne:
					return Keys.Right;
                case KeyDirection.SpaceOne:
                    return Keys.Space;
                default:
					return Keys.F5;
			}
		}

		public static string LoadTextFile(string InFileName)
		{
			return LoadTextFile(InFileName, ShowErrorMsg: false);
		}

		/// <summary>
		///  daniel change Encoding.GetEncoding(1252)
		/// </summary>
		/// <param name="InFileName"></param>
		/// <param name="ShowErrorMsg"></param>
		/// <returns></returns>
		public static string LoadTextFile(string InFileName, bool ShowErrorMsg)
		{
			try
			{
				string text = "";

      			//StreamReader streamReader = new StreamReader(InFileName, Encoding.GetEncoding(1252));
				using StreamReader streamReader = new StreamReader(InFileName);
				text = streamReader.ReadToEnd();
				//streamReader.Close();
				return text.Replace("\r\n", "\n").Replace("\v", "\n");
			}
			catch(Exception e)
			{
				Console.WriteLine(e.Message);
				Console.WriteLine(e.StackTrace);

				if (ShowErrorMsg)
				{
					MessageBox.Show("Cannot open the selected text file. The text file might be corrupted");
				}
				return "";
			}
		}

		public static void Load32InfoFile(string InFileName, ref SongSettings InItem, ref string[] ThisHeaderData)
		{
			try
			{
				string text = "";
				using StreamReader streamReader = File.OpenText(InFileName);
				text = streamReader.ReadToEnd();
				//streamReader.Close();
				string InfoFolder = "";
				string InfoHeading = "";
				string InfoRotate = "";
				int num = text.IndexOf("[");
				int num2 = text.IndexOf("]", num + 1);
				if (num == 0 && num2 > 1)
				{
					num++;
					string InFormatString = DataUtil.Mid(text, num, num2 - num);
					if (DataUtil.Convertv32FormatString(ref InFormatString, '*', ref InfoHeading, ref InfoFolder, ref InfoRotate) == 320)
					{
						InItem.CompleteLyrics = DataUtil.Mid(text, num2 + 1, text.Length - num2);
						InItem.Title = InfoHeading;
						InItem.FolderNo = DataUtil.StringToInt(InfoFolder);
						if (InItem.FolderNo < 1)
						{
							InItem.FolderNo = 1;
						}
						InItem.RotateString = InfoRotate;
					}
				}
			}
			catch
			{
			}
		}

		public static void LoadInfoFile(string InFileName, ref SongSettings InItem, ref string[] ThisHeaderData)
		{
			try
			{
				XmlTextReader reader = new XmlTextReader(InFileName);
				try
				{
					bool flag = false;
					if (ValidateEasiSlidesXML(ref reader))
					{
						string itemID = InItem.ItemID;
						ExtractEasiSlidesXMLItem(ref reader, ref InItem);
						InItem.ItemID = itemID;
					}
					else
					{
						Load32InfoFile(InFileName, ref InItem, ref ThisHeaderData);
					}
				}
				catch
				{
				}
				reader?.Close();
			}
			catch
			{
			}
		}

		public static bool ValidateEasiSlidesXML(ref XmlTextReader reader)
		{
			try
			{
				reader.Read();
				while (reader.Read())
				{
					if ((reader.NodeType == XmlNodeType.Element) & (reader.Name == "EasiSlides"))
					{
						return true;
					}
				}
			}
			catch
			{
			}
			return false;
		}

		public static bool MoveToXMLItemElement(ref XmlTextReader reader)
		{
			while (reader.Read())
			{
				if ((reader.NodeType == XmlNodeType.Element) & (reader.Name == "Item"))
				{
					return true;
				}
			}
			return false;
		}

		public static bool ExtractEasiSlidesXMLItem(ref XmlTextReader reader, ref SongSettings InItem)
		{
			string text = "";
			string text2 = "";
			bool flag = false;
			InItem.Type = "I";
			if (MoveToXMLItemElement(ref reader))
			{
				while (reader.Read())
				{
					text2 = "";
					switch (reader.NodeType)
					{
						case XmlNodeType.Element:
							text = reader.Name;
							text2 = reader.ReadElementContentAsObject().ToString();
							AssignElementToItem(ref InItem, text, text2);
							flag = true;
							break;
						case XmlNodeType.EndElement:
							text = reader.Name;
							if (reader.Name == "Item")
							{
								InItem.In_LicAdminInfo1 = InItem.Show_LicAdminInfo1;
								InItem.In_LicAdminInfo2 = InItem.Show_LicAdminInfo2;
								LoadLicAdminDisplayInfo(ref InItem.Show_LicAdminInfo1, ref InItem.Show_LicAdminInfo2);
								return flag ? true : false;
							}
							break;
					}
				}
			}
			return false;
		}

		public static void GetTitle2AndFormatFromInfoFile(string InFileName, ref string Title2, ref string FormatString)
		{
			InitialiseIndividualData(ref TempItem1);
			LoadInfoFile(InFileName, ref TempItem1, ref tempHeaderData);
			Title2 = TempItem1.Title2;
			FormatString = TempItem1.Format.FormatString;
		}

		public static void AssignElementToItem(ref SongSettings InItem, string ElementName, string ElementValue)
		{
			switch (ElementName)
			{
				case "Title1":
					InItem.Title = ElementValue;
					break;
				case "Title2":
					InItem.Title2 = ElementValue;
					break;
				case "Folder":
					InItem.FolderName = ElementValue;
					InItem.FolderNo = GetFolderNumber(ElementValue);
					break;
				case "SongNumber":
					InItem.SongNumber = DataUtil.StringToInt(ElementValue);
					break;
				case "Contents":
					InItem.CompleteLyrics = ElementValue;
					break;
				case "Notations":
					InItem.Notations = ElementValue;
					break;
				case "Sequence":
					InItem.SongSequence = ConvertTextStringToSequence(ElementValue);
					InItem.SongOriginalLoadedSequence = InItem.SongSequence;
					break;
				case "Writer":
					InItem.Writer = ElementValue;
					break;
				case "Copyright":
					InItem.Copyright = ElementValue;
					break;
				case "Category":
					InItem.Category = ElementValue;
					break;
				case "Timing":
					InItem.Timing = ElementValue;
					break;
				case "MusicKey":
					InItem.MusicKey = ElementValue;
					break;
				case "Capo":
					InItem.Capo = DataUtil.StringToInt(ElementValue, Minus1IfBlank: true);
					break;
				case "LicenceAdmin1":
					InItem.Show_LicAdminInfo1 = ElementValue;
					break;
				case "LicenceAdmin2":
					InItem.Show_LicAdminInfo2 = ElementValue;
					break;
				case "BookReference":
					InItem.Book_Reference = ElementValue;
					break;
				case "UserReference":
					InItem.User_Reference = ElementValue;
					break;
				case "FormatData":
					InItem.Format.FormatString = ElementValue;
					break;
				case "Settings":
					InItem.Settings = ElementValue;
					InItem.RotateString = ExtractSettings(ElementValue, SettingsCategory.RotateString);
					GetRotationStyle(ref InItem);
					break;
				case "Image":
					InItem.Format.ImageString = ElementValue;
					break;
			}
		}

		public static string ExtractSettings(string InSettingsString, SettingsCategory settingsCategory)
		{
			if (InSettingsString == "")
			{
				return "";
			}
			string text = "";
			string text2 = "";
			int num = -1;
			string[] array = InSettingsString.Split('>');
			for (int i = 0; i <= array.GetUpperBound(0); i++)
			{
				text2 = DataUtil.ExtractOneInfo(ref array[i], '=', RemoveExtract: true, MinusOneIfBlank: false);
				if (text2 != "")
				{
					num = DataUtil.StringToInt(text2);
					if (num == (int)settingsCategory)
					{
						return array[i];
					}
				}
			}
			return "";
		}

		public static string CombineSettings(SongSettings InItem)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(Convert.ToString(10) + "=" + InItem.RotateString + '>');
			return stringBuilder.ToString();
		}

		public static string dumpImageToFile(byte[] img, string InFileName)
		{
			try
			{
				string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(InFileName);
				string extension = Path.GetExtension(InFileName);
				string text = EasiSlidesTempDir + fileNameWithoutExtension + GetUniqueID() + extension;
				using FileStream fileStream = new FileStream(text, FileMode.OpenOrCreate, FileAccess.Write);
				int num = 0;
				fileStream.Write(img, num, img.Length - num);
				//fileStream.Close();
				//fileStream = null;
				return text;
			}
			catch
			{
				return "";
			}
		}

		public static void StartElement(object strURI, string strName, string strName_3, object attributes)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		public static void OldSaveInfoFile(string InFullFileName, string InText, string[] ThisHeaderData)
		{
			try
			{
				using FileStream fileStream = new FileStream(InFullFileName, FileMode.Create);
				BinaryWriter w = new BinaryWriter(fileStream);
				w.Write(byte.MaxValue);
				w.Write((byte)254);
				string inString = "[";
				FileUtil.WriteStringToBinaryFile(ref w, inString);
				inString = '\u0001' + "=" + 320.ToString() + '*'.ToString();
				FileUtil.WriteStringToBinaryFile(ref w, inString);
				for (int i = 2; i <= 254; i++)
				{
					if (ThisHeaderData[i] != "" && ThisHeaderData[i] != null)
					{
						inString = (char)i + "=" + ThisHeaderData[i] + '*'.ToString();
						FileUtil.WriteStringToBinaryFile(ref w, inString);
					}
				}
				inString = "]";
				FileUtil.WriteStringToBinaryFile(ref w, inString);
				inString = InText;
				FileUtil.WriteStringToBinaryFile(ref w, inString);
				w.Close();
				//fileStream.Close();
			}
			catch
			{
			}
		}

		public static string SetPowerpointPreviewPrefix(SongSettings InItem)
		{
			if (InItem.Type != "P")
			{
				return "";
			}
			if (InItem.OutputStyleScreen)
			{
				if ((OUTPPSequence < 0) | (OUTPPSequence >= 49))
				{
					OUTPPSequence = 0;
				}
				OUTPPSequence++;
				OUTPPFullPath = OUTPPPrefix + OUTPPSequence;
				return OUTPPFullPath;
			}
			if ((PREPPSequence < 0) | (PREPPSequence >= 49))
			{
				PREPPSequence = 0;
			}
			PREPPSequence++;
			PREPPFullPath = PREPPPrefix + PREPPSequence;
			return PREPPFullPath;
		}

		public static string SetPowerpointPreviewPrefix1(SongSettings InItem)
		{
			if (InItem.Type != "P")
			{
				return "";
			}
			Regex regex = new Regex(string.Format("[{0}]", Regex.Escape(new string(Path.GetInvalidFileNameChars()))));
			string prefixItemName = regex.Replace(InItem.Title, "");

			if (InItem.OutputStyleScreen)
			{
				OUTPPFullPath = OUTPPPrefix + "$" + prefixItemName + "$";
				return OUTPPFullPath;
			}
			PREPPFullPath = PREPPPrefix + "$" + prefixItemName + "$";
			return PREPPFullPath;
		}

		public static void MinimizePowerPointWindows(ref PowerPoint InPPT)
		{
			InPPT.ResSetAllShowWindows();
		}

		public static int RunPowerpointSong(ref SongSettings InItem, ref PowerPoint InPPT, int StartingSlide)
		{
			return RunPowerpointSong(ref InItem, ref InPPT, StartingSlide, ShowResult: false);
		}

		public static int RunPowerpointSong(ref SongSettings InItem, ref PowerPoint InPPT, int StartingSlide, bool ShowResult)
		{
			for (int i = 1; i <= 1000; i++)
			{
				InItem.Slide[i, 0] = -1;
			}
			for (int i = 0; i <= 9; i++)
			{
				InItem.SongVerses[i] = 0;
			}
			InPPT.displayName = OutputMonitorName;

			string text = InPPT.Run(InItem.Path, ref PowerpointList, ref TotalPowerpointItems);
			if (StartingSlide < 2)
			{
				InPPT.First();
				InItem.CurSlide = 1;
			}
			else if (StartingSlide > InPPT.Count())
			{
				InPPT.Last();
				InItem.CurSlide = InPPT.Count();
			}
			else if (InPPT.Count() > 0)
			{
				InPPT.GotoSlide(StartingSlide);
				InItem.CurSlide = StartingSlide;
			}

			if (!ShowLiveCam && gf.DualMonitorSelectAutoOption == 1)
            {
				float scalef = 0.75f;
                // 파워포인트 파일이 표시되는 모니터를 설정하기 위해 설정

                if (DualMonitorMode)
                {
                    InPPT.SetShowWindow((float)LS_Left * scalef, (float)LS_Top * scalef, (float)LS_Width * scalef, (float)LS_Height * scalef);
                }
                else
                {
                    InPPT.SetShowWindow(LS_Left, LS_Top, (float)LS_Width * scalef, (float)LS_Height * scalef);
                }
            }

            InPPT.LoadVersesAndSlides(ref InItem.SongVerses, ref InItem.Slide, SequenceSymbol);
			return InPPT.Count();
		}

		//public static int RunPowerpointSong(ref SongSettings InItem, ref PowerPoint InPPT, int StartingSlide, bool ShowResult)
		//{
		//	for (int i = 1; i <= 1000; i++)
		//	{
		//		InItem.Slide[i, 0] = -1;
		//	}
		//	for (int i = 0; i <= 9; i++)
		//	{
		//		InItem.SongVerses[i] = 0;
		//	}
		//	string text = InPPT.Run(InItem.Path, ref PowerpointList, ref TotalPowerpointItems);
		//	if (StartingSlide < 2)
		//	{
		//		InPPT.First();
		//		InItem.CurSlide = 1;
		//	}
		//	else if (StartingSlide > InPPT.Count())
		//	{
		//		InPPT.Last();
		//		InItem.CurSlide = InPPT.Count();
		//	}
		//	else if (InPPT.Count() > 0)
		//	{
		//		InPPT.GotoSlide(StartingSlide);
		//		InItem.CurSlide = StartingSlide;
		//	}
  //          if (!ShowLiveCam)
  //          {
  //              if (DualMonitorMode)
  //              {
  //                  InPPT.SetShowWindow((float)LS_Left * 0.75f, (float)LS_Top * 0.75f, (float)LS_Width * 0.75f, (float)LS_Height * 0.75f);
  //              }
  //              else
  //              {
  //                  InPPT.SetShowWindow(LS_Left, LS_Top, (float)LS_Width * 0.75f, (float)LS_Height * 0.75f);
  //              }
  //          }

  //          InPPT.LoadVersesAndSlides(ref InItem.SongVerses, ref InItem.Slide, SequenceSymbol);
		//	return InPPT.Count();
		//}

		public static string GetOfficeDocContents(string InFileName)
		{
			try
			{
				WordDoc wordDoc = new WordDoc();
				return wordDoc.GetContents(InFileName).Replace("\v", "\n");
			}
			catch (Exception)
			{
			}
			return "";
		}

		public static bool SupportedImageFormat(string InFileName)
		{
			string a = DataUtil.Right(InFileName, 4).ToLower();
			if ((a == ".bmp") | (a == ".jpg") | (a == ".ico") | (a == ".gif") | (DataUtil.Right(InFileName, 5).ToLower() == ".jpeg"))
			{
				return true;
			}
			return false;
		}

		/// <summary>
		/// daniel
		/// 확장자 docx 추가
		/// </summary>
		/// <param name="InFileName"></param>
		/// <returns></returns>
		public static bool SupportedOpenDocFormat(string InFileName)
		{
			//string a = DataUtil.Right(InFileName, 4).ToLower();
			string strExt = Path.GetExtension(InFileName).ToLower();
			if ((strExt == ".doc") | (strExt == ".docx") | (strExt == ".txt"))
			{
				return true;
			}
			return false;
		}

		public static int ValidateSongMovement(ref int InCurItem, int InCurMaxItems, Keys InKey, int InItemNo)
		{
			switch (InKey)
			{
				case Keys.Home:
					InCurItem = 1;
					break;
				case Keys.End:
					InCurItem = InCurMaxItems;
					break;
				case Keys.Prior:
					InCurItem = ((InCurItem <= 2) ? 1 : (InCurItem - 1));
					break;
				case Keys.Next:
					InCurItem = ((InCurItem < InCurMaxItems) ? (InCurItem + 1) : InCurMaxItems);
					break;
				case Keys.None:
					InCurItem = ((InItemNo <= 0) ? InCurItem : ((InItemNo <= InCurMaxItems) ? InItemNo : InCurMaxItems));
					break;
			}
			return InCurItem;
		}

		public static void SetTransparentBackground(SongSettings InItem, ref ImageTransitionControl InPic)
		{
			SetColoursFormat(InItem, ref InPic, SetTransparent: true);
		}

		public static void OldSwitchChineseLyricsNotationListView(ref ListView InLyricsAndNotationsList, int ChangeTo)
		{
			if (InLyricsAndNotationsList.Items.Count > 0)
			{
				for (int i = 0; i <= InLyricsAndNotationsList.Items.Count - 1; i++)
				{
					string InString = InLyricsAndNotationsList.Items[i].SubItems[2].Text;
					SwitchChinese(ref InString, ChangeTo);
					InLyricsAndNotationsList.Items[i].SubItems[2].Text = InString;
				}
			}
		}

		public static void SwitchChineseLyricsNotationListView(ref SongSettings InItem, int ChangeTo)
		{
			if (InItem.LyricsAndNotationsList.Items.Count > 0)
			{
				for (int i = 0; i <= InItem.LyricsAndNotationsList.Items.Count - 1; i++)
				{
					string InString = InItem.LyricsAndNotationsList.Items[i].SubItems[2].Text;
					SwitchChinese(ref InString, ChangeTo);
					InItem.LyricsAndNotationsList.Items[i].SubItems[2].Text = InString;
					SwitchChinese(ref InItem.Title, ChangeTo);
					SwitchChinese(ref InItem.Title2, ChangeTo);
					SwitchChinese(ref InItem.Copyright, ChangeTo);
				}
			}
		}

		public static int SwitchChinese(ref RichTextBox InTextBox)
		{
			string InString = InTextBox.Text;
			int num = SwitchChinese(ref InString);
			string[] array = InString.Split("\n"[0]);
			int num2 = 0;
			int num3 = -1;
			int num4 = 0;
			string text = "";
			num2 = 0;
			SendMessage(InTextBox.Handle, 11u, 0u, 0u);
			for (int i = 0; i <= array.GetUpperBound(0); i++)
			{
				num3 = InString.IndexOf("\n"[0], num2);
				SwitchChinese(ref array[i], num);
				text = array[i];
				num4 = num3 - num2;
				if (num3 >= 0)
				{
					InTextBox.SelectionStart = num2;
					InTextBox.SelectionLength = num4;
					InTextBox.SelectedText = text;
					num2 = num3 + 1;
				}
				else if (num2 <= InString.Length)
				{
					InTextBox.SelectionStart = num2;
					InTextBox.SelectionLength = InString.Length - num2;
					InTextBox.SelectedText = text;
				}
			}
			SendMessage(InTextBox.Handle, 11u, 1u, 0u);
			return num;
		}

		public static int SwitchChinese(ref string InString)
		{
			return SwitchChinese(ref InString, -1);
		}

		/// <summary>
		/// daniel
		/// </summary>
		/// <param name="InString"></param>
		/// <param name="SelectedType"></param>
		/// <returns></returns>
		public static int SwitchChinese(ref string InString, int SelectedType)
		{
			try
			{
				string text = "»#«";
				char c = '祢';
				string str = InString;
				//str = Strings.StrConv(str, VbStrConv.SimplifiedChinese, CultureInfo.CurrentCulture.LCID);
				
				str = getStrConv(str, "utf-8");

				switch (SelectedType)
				{
					case 1:
						if (!(str == InString))
						{
							InString = str;
						}
						return 1;
					case 0:
						if (str == InString)
						{
							str = str.Replace(c.ToString(), text);
							str = getStrConv(str, "utf-8");
							//str = Strings.StrConv(str, VbStrConv.TraditionalChinese, CultureInfo.CurrentCulture.LCID);
							str = (InString = str.Replace(text, c.ToString()));
						}
						return 0;
					default:
						if (str == InString)
						{
							str = str.Replace(c.ToString(), text);
							str = getStrConv(str, "utf-8");
							//str = Strings.StrConv(str, VbStrConv.TraditionalChinese, CultureInfo.CurrentCulture.LCID);
							str = (InString = str.Replace(text, c.ToString()));
							return 0;
						}
						InString = str;
						return 1;
				}
            }
            catch (Exception e)
			{
				Console.WriteLine(e.Message);
				Console.WriteLine(e.StackTrace);
			}

			return 1;
		}

		/// <summary>
		/// daniel
		/// </summary>
		/// <param name="sDat"></param>
		/// <param name="codepage"></param>
		/// <returns></returns>
		static string getStrConv(string sDat, string codepage)
		{
			try
			{
				//x-cp20936 중국 codepage
				System.Text.Encoding myEncoding = Encoding.GetEncoding(codepage);

				byte[] buf = myEncoding.GetBytes(sDat);

				return myEncoding.GetString(buf);
			}
			catch (Exception e)
            {
				Console.WriteLine(e.Message);
				Console.WriteLine(e.StackTrace);
			}

			return sDat; 
		}

		public static string ExtractFolderFromListTitle(ref string InListTitle)
		{
			string result = "1";
			int num = InListTitle.IndexOf(":", 0);
			if (num > 0)
			{
				string inName = DataUtil.Trim(DataUtil.Right(InListTitle, InListTitle.Length - num - 1));
				result = GetFolderNumber(inName, ZeroIfInvalid: true).ToString();
				InListTitle = DataUtil.Left(InListTitle, num);
			}
			return result;
		}

		/// <summary>
		/// daniel
		/// </summary>
		/// <param name="InPP"></param>
		/// <param name="InSongsArray"></param>
		public static void PreLoadPowerpointFiles(ref PowerPoint InPP, ref string[,] InSongsArray)
		{
			bool flag = false;
			InPP.isLive = true;
			InPP.isEditable = false;
			for (int i = 0; i <= TotalWorshipListItems; i++)
			{
				try
				{
					if (InSongsArray[i, 1] == "P")
					{
						if (!flag)
						{
							InPP.NewApp();
							//InPP.newPowerPointSlideApp();
							flag = true;
						}
						InPP.Open(DataUtil.Right(InSongsArray[i, 0], InSongsArray[i, 0].Length - 1), ref PowerpointList, ref TotalPowerpointItems);
						// 이렇게 해주지 않으면 프리젠테이션창이 Nomal로 뜬다
						InPP.prePowerPointApp.Activate();
						InPP.prePowerPointApp.WindowState = NetOffice.PowerPointApi.Enums.PpWindowState.ppWindowMinimized;
						//InPP.newOpen(DataUtil.Right(InSongsArray[i, 0], InSongsArray[i, 0].Length - 1), ref PowerpointList, ref TotalPowerpointItems);
					}
				}
				catch
				{
				}
			}
		}

		/// <summary>
		/// daniel
		/// </summary>
		public static void ClearUpPowerpointWindows()
		{
			string text = LivePP.ClearUpPowerpointWindows(ref PowerpointList, ref TotalPowerpointItems);
			if( text == "")
            {
				LivePP.QuitPowerPointApp(LivePP.prePowerPointApp);
			}
		}

		public static bool PowerpointPresent()
		{
			string text = Application.StartupPath + "\\Sys\\~temp.ppt";
			FileUtil.MakeDir(Application.StartupPath + "\\Sys");
			if (!File.Exists(text))
			{
				FileUtil.CreateNewFile(text);
			}
			text = '"' + text + '"';
			string lpResult = new string(' ', 260);
			if (FindExecutable(text, "", lpResult) > 32)
			{
				return true;
			}
			return false;
		}
        /// <summary>
        /// daniel park
        /// Buffer_LS_Width 오류 발생으로 코드 삽입
        /// </summary>
        /// <param name="InScreen"></param>
        public static void SetDefaultBackScreen(ref ImageTransitionControl InScreen)
		{
			try
			{
                switch (Buffer_LS_Width)
                {
                    case <= 0 when Buffer_LS_Height == 768:
                        Buffer_LS_Width = 1024;
                        break;
                    case <= 0 when Buffer_LS_Height == 800:
                        Buffer_LS_Width = 1280;
                        break;
                    case <= 0 when Buffer_LS_Height == 1024:
                        Buffer_LS_Width = 1280;
                        break;
                    case <= 0 when Buffer_LS_Height == 900:
                        Buffer_LS_Width = 1440;
                        break;
                    case <= 0 when Buffer_LS_Height == 1050:
                        Buffer_LS_Width = 1680;
                        break;
                    case <= 0 when Buffer_LS_Height == 1200:
                        Buffer_LS_Width = 1600;
                        break;
                    case <= 0 when Buffer_LS_Height == 1536:
                        Buffer_LS_Width = 2048;
                        break;
                    case <= 0 when Buffer_LS_Height == 1920:
                        Buffer_LS_Width = 1080;
                        break;
                    case <= 0 when Buffer_LS_Height == 2560:
                        Buffer_LS_Width = 1440;
                        break;
                    case <= 0 when Buffer_LS_Height == 3840:
                        Buffer_LS_Width = 2160;
                        break;
                    case <= 0 when Buffer_LS_Height == 7680:
                        Buffer_LS_Width = 4320;
                        break;
                }

                Image image = new Bitmap(Buffer_LS_Width, Buffer_LS_Height);
				Graphics g = Graphics.FromImage(image);
				BackPattern.Fill(ref g, ShowScreenColour[0], ShowScreenColour[1], ShowScreenStyle, Buffer_LS_Width, Buffer_LS_Height, ref DefaultBackgroundID);
				InScreen.SetDefaultBackgroundPicture(image);
				//g.Dispose();
				//image.Dispose();
			}
			catch (Exception e)
			{
				Console.WriteLine(e.StackTrace);
				Console.WriteLine(e.Message);
			}
		}

		public static bool EmptyDelFolder()
		{
			try
			{
#if OleDb
				using (OleDbConnection daoDb = DbConnectionController.GetOleDbConnection(ConnectStringMainDB))
				{
					OleDbCommand command = new OleDbCommand("Delete * from Song where FolderNo =0", daoDb);
					command.ExecuteNonQuery();
				}
#elif SQLite
				using (DbConnection connection = DbController.GetDbConnection(ConnectStringMainDB))
				{
					DbCommand command = new DbCommand("Delete * from Song where FolderNo = 0", connection);
					command.ExecuteNonQuery();
				}
#endif



				return true;
			}
			catch
			{
			}
			return false;
		}
#if DAO
		public static int ReFileSelectedSongs(ref ListView InListView, int CurFolder, int NewFolder, ref int[] RefileSongs, bool UpdateModifiedDate)
		{
			RefileSongs[0] = 0;
			string text = "";
			try
			{
				Recordset tableRecordSet = DbDaoController.GetTableRecordSet(ConnectStringMainDB, "SONG");
				tableRecordSet.Index = "PrimaryKey";
				for (int num = InListView.Items.Count - 1; num >= 0; num--)
				{
					if (InListView.Items[num].Selected)
					{
						string text2;
						if (CurFolder == 0)
						{
							text2 = InListView.Items[num].SubItems[3].Text;
							NewFolder = DataUtil.StringToInt(InListView.Items[num].SubItems[1].Text);
						}
						else
						{
							string text3 = InListView.Items[num].SubItems[1].Text;
							text2 = DataUtil.Right(text3, text3.Length - 1);
						}
						try
						{
							tableRecordSet.Seek("=", text2, def, def, def, def, def, def, def, def, def, def, def, def);
							if (!tableRecordSet.NoMatch)
							{
								tableRecordSet.Edit();
								tableRecordSet.Fields["OldFolder"].Value = CurFolder;
								tableRecordSet.Fields["FolderNo"].Value = NewFolder;
								if (UpdateModifiedDate)
								{
									tableRecordSet.Fields["LastModified"].Value = DateTime.Now.Date;
								}
								tableRecordSet.Update();
								InListView.Items[num].Remove();
								RefileSongs[0]++;
								RefileSongs[RefileSongs[0]] = DataUtil.StringToInt(text2);
							}
						}
						catch
						{
						}
					}
				}
				if (tableRecordSet != null)
				{
					tableRecordSet.Close();
					tableRecordSet = null;
				}
			}
			catch
			{
			}
			return RefileSongs[0];
		}
#elif SQLite
		public static int ReFileSelectedSongs(ref ListView InListView, int CurFolder, int NewFolder, ref int[] RefileSongs, bool UpdateModifiedDate)
		{
			RefileSongs[0] = 0;
			string sQuery = "select * from SONG";
			try
			{
				using DbConnection connection = DbController.GetDbConnection(ConnectStringMainDB);
				using DataTable dataTable = DbController.GetDataTable(ConnectStringMainDB, sQuery);

				DataColumn[] primarykey = new DataColumn[] { dataTable.Columns["SONGID"] };
				dataTable.PrimaryKey = primarykey;

				for (int num = InListView.Items.Count - 1; num >= 0; num--)
				{
					if (InListView.Items[num].Selected)
					{
						string text2;
						if (CurFolder == 0)
						{
							text2 = InListView.Items[num].SubItems[3].Text;
							NewFolder = DataUtil.StringToInt(InListView.Items[num].SubItems[1].Text);
						}
						else
						{
							string text3 = InListView.Items[num].SubItems[1].Text;
							text2 = DataUtil.Right(text3, text3.Length - 1);
						}
						try
						{
							DataRow dr = dataTable.Rows.Find($"{text2}");
							if (dr != null)
							{
								dr["OldFolder"] = CurFolder;
								dr["FolderNo"] = NewFolder;
								if (UpdateModifiedDate)
								{
									dr["LastModified"] = DateTime.Now.Date;
								}
								InListView.Items[num].Remove();
								RefileSongs[0]++;
								RefileSongs[RefileSongs[0]] = DataUtil.StringToInt(text2);
							}
						}
						catch
						{
						}
					}
				}

				DbController.UpdateTable(connection, sQuery, dataTable);
			}
			catch
			{
			}
			return RefileSongs[0];
		}
#endif

#if DAO
		public static int ReFileSelectedSongs(ref ListView InListView)
		{
			string text = "";
			try
			{
				Recordset tableRecordSet = DbDaoController.GetTableRecordSet(ConnectStringMainDB, "SONG");
				tableRecordSet.Index = "PrimaryKey";
				for (int num = InListView.Items.Count - 1; num >= 0; num--)
				{
					if (InListView.Items[num].Checked)
					{
						string text2 = InListView.Items[num].SubItems[3].Text;
						try
						{
							tableRecordSet.Seek("=", text2, def, def, def, def, def, def, def, def, def, def, def, def);
							if (!tableRecordSet.NoMatch)
							{
								int num2 = Convert.ToInt32(InListView.Items[num].SubItems[4].Text);
								tableRecordSet.Edit();
								tableRecordSet.Fields["OldFolder"].Value = 0;
								tableRecordSet.Fields["FolderNo"].Value = num2;
								tableRecordSet.Fields["LastModified"].Value = DateTime.Now.Date;
								tableRecordSet.Update();
								InListView.Items[num].Remove();
							}
						}
						catch
						{
						}
					}
				}
				if (tableRecordSet != null)
				{
					tableRecordSet.Close();
					tableRecordSet = null;
				}
			}
			catch
			{
			}
			return InListView.CheckedItems.Count;
		}
#elif SQLite
		public static int ReFileSelectedSongs(ref ListView InListView)
		{
			string sQuery = "select * from SONG";
			try
			{
				using DbConnection connection = DbController.GetDbConnection(ConnectStringMainDB);
				using DataTable dataTable = DbController.GetDataTable(ConnectStringMainDB, sQuery);

				DataColumn[] primarykey = new DataColumn[] { dataTable.Columns["SONGID"] };
				dataTable.PrimaryKey = primarykey;

				for (int num = InListView.Items.Count - 1; num >= 0; num--)
				{
					if (InListView.Items[num].Checked)
					{
						string text2 = InListView.Items[num].SubItems[3].Text;
						try
						{
							DataRow dr = dataTable.Rows.Find($"{text2}");
							if (dr != null)
							{
								int num2 = Convert.ToInt32(InListView.Items[num].SubItems[4].Text);
								dr["OldFolder"] = 0;
								dr["FolderNo"] = num2;
								dr["LastModified"] = DateTime.Now.Date;
								InListView.Items[num].Remove();
							}
						}
						catch
						{
						}
					}
				}
				DbController.UpdateTable(connection, sQuery, dataTable);
			}
			catch
			{
			}
			return InListView.CheckedItems.Count;
		}
#endif

		public static int ReFileSelectedSongsADO(ref ListView.SelectedListViewItemCollection SongItems, int CurFolder, int NewFolder, ref int[] RefileSongs, bool UpdateModifiedDate)
		{
			RefileSongs[0] = 0;
			try
			{
#if OleDb
				using (OleDbConnection daoDb = DbConnectionController.GetOleDbConnection(ConnectStringMainDB))
				{
					string text = "";
					for (int num = SongItems.Count - 1; num >= 0; num--)
					{
						try
						{
							string text2;
							if (CurFolder == 0)
							{
								text2 = SongItems[num].SubItems[3].Text;
								NewFolder = DataUtil.StringToInt(SongItems[num].SubItems[1].Text);
							}
							else
							{
								string text3 = SongItems[num].SubItems[1].Text;
								text2 = DataUtil.Right(text3, text3.Length - 1);
							}
							text = "Update SONG SET OldFolder = @OldFolder, FolderNo =@FolderNo" + (UpdateModifiedDate ? ", LastModified =@LastModified" : "") + " where songid=" + text2.ToString();
							OleDbCommand command = new OleDbCommand(text, daoDb);
							command.CommandText = text;
							command.Parameters.AddWithValue("@OldFolder", CurFolder);
							command.Parameters.AddWithValue("@FolderNo", NewFolder);
							command.Parameters.AddWithValue("@LastModified", DateTime.Now.Date);
							command.ExecuteNonQuery();
							command.Dispose();
							SongItems[num].Remove();
							RefileSongs[0]++;
							RefileSongs[RefileSongs[0]] = DataUtil.StringToInt(text2);
						}
						catch (Exception)
						{
						}
					}
				}
#elif SQLite
				using (DbConnection connection = DbController.GetDbConnection(ConnectStringMainDB))
				{
					string text = "";
					for (int num = SongItems.Count - 1; num >= 0; num--)
					{
						try
						{
							string text2;
							if (CurFolder == 0)
							{
								text2 = SongItems[num].SubItems[3].Text;
								NewFolder = DataUtil.StringToInt(SongItems[num].SubItems[1].Text);
							}
							else
							{
								string text3 = SongItems[num].SubItems[1].Text;
								text2 = DataUtil.Right(text3, text3.Length - 1);
							}
							text = "Update SONG SET OldFolder = @OldFolder, FolderNo =@FolderNo" + (UpdateModifiedDate ? ", LastModified =@LastModified" : "") + " where songid=" + text2.ToString();
							using DbCommand command = new DbCommand(text, connection);
							command.CommandText = text;
							command.Parameters.AddWithValue("@OldFolder", CurFolder);
							command.Parameters.AddWithValue("@FolderNo", NewFolder);
							command.Parameters.AddWithValue("@LastModified", DateTime.Now.Date);
							command.ExecuteNonQuery();
							command.Dispose();
							SongItems[num].Remove();
							RefileSongs[0]++;
							RefileSongs[RefileSongs[0]] = DataUtil.StringToInt(text2);
						}
						catch (Exception e)
						{
							Console.WriteLine(e.Message);
							Console.WriteLine(e.StackTrace);
						}
					}
				}
#endif
				
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				Console.WriteLine(ex.StackTrace);
			}
			return RefileSongs[0];
		}

		//public static int ReFileSelectedSongsADO(ref ListView.CheckedListViewItemCollection SongItems)
		//{
		//	try
		//	{
		//		OleDbConnection connection = new OleDbConnection(ConnectStringMainDB);
		//		connection.Open();
		//		string text = "";
		//		for (int num = SongItems.Count - 1; num >= 0; num--)
		//		{
		//			try
		//			{
		//				string text2 = SongItems[num].SubItems[3].Text;
		//				int folderNumber = GetFolderNumber(SongItems[num].SubItems[1].Text);
		//				text = "Update SONG SET OldFolder = @OldFolder,FolderNo =@FolderNo,LastModified =@LastModified where songid=" + text2.ToString();
		//				OleDbCommand command = new OleDbCommand(text, connection);
		//				command.CommandText = text;
		//				command.Parameters.AddWithValue("@OldFolder", 0);
		//				command.Parameters.AddWithValue("@FolderNo", folderNumber);
		//				command.Parameters.AddWithValue("@LastModified", DateTime.Now.Date);
		//				command.ExecuteNonQuery();
		//				SongItems[num].Remove();
		//			}
		//			catch (Exception)
		//			{
		//			}
		//		}
		//		connection.Close();
		//	}
		//	catch
		//	{
		//	}
		//	return SongItems.Count;
		//}
#if OleDb
		public static bool ClearAllFormatting()
		{
			if (MessageBox.Show("This will clear all individual formatting held in the Lyrics Database. Click Yes to proceed.", "Compact EasiSlides Databases", MessageBoxButtons.YesNo) == DialogResult.Yes)
			{
				try
				{
					string fullSearchString = "select * from SONG where FORMATDATA <> ''";
					using (OleDbConnection daoDb = DbConnectionController.GetOleDbConnection(ConnectStringMainDB))
					{
						OleDbDataAdapter da = null;
						DataSet ds = null;
						DataTable dt = null;
						(da, ds) = DbOleDbController.getDataAdapter(daoDb, fullSearchString);
						dt = ds.Tables[0];
						if (dt.Rows.Count > 0)
						{
							//recordset.MoveFirst();
							//while (!recordset.EOF)
							foreach (DataRow dr in dt.Rows)
							{
								//recordset.Edit();
								dr["FORMATDATA"] = "";
								//recordset.MoveNext();
							}
							da.Update(dt);
							dt.Dispose();
							da.Dispose();
						}
					}
					return true;
				}
				catch
				{
				}
			}
			return false;
		}
#elif SQLite
		public static bool ClearAllFormatting()
		{
			if (MessageBox.Show("This will clear all individual formatting held in the Lyrics Database. Click Yes to proceed.", "Compact EasiSlides Databases", MessageBoxButtons.YesNo) == DialogResult.Yes)
			{
				try
				{
					string fullSearchString = "select * from SONG where FORMATDATA <> ''";

					using DbConnection connection = DbController.GetDbConnection(ConnectStringMainDB);

					DbDataAdapter sQLiteDataAdapter = null;

					DataTable dataTable = null;
					(sQLiteDataAdapter, dataTable) = DbController.getDataAdapter(connection, fullSearchString);

					if (dataTable.Rows.Count > 0)
					{
						foreach (DataRow dr in dataTable.Rows)
						{
							dr["FORMATDATA"] = "";
						}
						DbController.UpdateTable(connection, fullSearchString, dataTable);
					}

					return true;
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.Message);
					Console.WriteLine(ex.StackTrace);
				}
			}
			return false;
		}
#endif
		public static bool ClearRegistrySettings()
		{
			if (MessageBox.Show("Warning: This will clear all EasiSlides Registry Settings and EasiSlides will then Close. Please note that the next time you restart EasiSlides, the EasiSlides Working Folder will be set to C:\\EasiSlides. Click Yes if you wish to proceed.", "Clear EasiSlides Registry Settings", MessageBoxButtons.YesNo) == DialogResult.Yes)
			{
				try
				{
					RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("Software", writable: true);
					registryKey.DeleteSubKeyTree("EasiSlides");
					registryKey.Close();
					return true;
				}
				catch
				{
				}
			}
			return false;
		}

#if OleDb
		public static bool SwapFolderNumbers(ListView InFolderOrder)
		{
			bool flag = false;
			try
			{
				int[] array = new int[41];
				string text = "";
				for (int i = 1; i < 41; i++)
				{
					if (InFolderOrder.Items.Count >= 41)
					{
						break;
					}
					array[GetFolderNumber(InFolderOrder.Items[i - 1].SubItems[0].Text)] = i;
				}
				string fullSearchString = "select * from SONG WHERE FolderNo > 0 ";
				using (OleDbConnection daoDb = DbConnectionController.GetOleDbConnection(ConnectStringMainDB))
				{

					int num = 0;
					OleDbDataAdapter da;
					DataSet ds = null;
					DataTable dt = null;
					(da, ds) = DbOleDbController.getDataAdapter(daoDb, fullSearchString);
					dt = ds.Tables[0];

					if (dt.Rows.Count > 0)
					{
						foreach (DataRow dr in dt.Rows)
						{
							num = DataUtil.ObjToInt(dr["FolderNo"]);
							if (num != array[num])
							{
								dr["FolderNo"] = array[num];
							}
						}
						da.Update(dt);
						dt.Dispose();
						da.Dispose();
					}

					fullSearchString = "select * from FOLDER  where FolderNo > 0 ORDER BY FolderNo";
					(da, ds) = DbOleDbController.getDataAdapter(daoDb, fullSearchString);
					dt = ds.Tables[0];

					string text2 = "";
					if (dt.Rows.Count > 0)
					{
						foreach (DataRow dr in dt.Rows)
						{
							num = DataUtil.ObjToInt(dr["FolderNo"]);
							text2 = DataUtil.ObjToString(dr["Name"]);
							dr["FolderNo"] = -array[num];
						}
						da.Update(dt);
						dt.Dispose();
						da.Dispose();
					}

					fullSearchString = "select * from FOLDER  where FolderNo < 0 ORDER BY FolderNo";
					(da, ds) = DbOleDbController.getDataAdapter(daoDb, fullSearchString);
					dt = ds.Tables[0];

					if (dt.Rows.Count > 0)
					{
						foreach (DataRow dr in dt.Rows)
						{
							num = DataUtil.ObjToInt(dr["FolderNo"]);
							dr["FolderNo"] = -num;
						}
						da.Update(dt);
						dt.Dispose();
						da.Dispose();
					}
				}
				return true;
			}
			catch
			{
			}
			return false;
		}
#elif SQLite
		public static bool SwapFolderNumbers(ListView InFolderOrder)
		{
			bool flag = false;
			try
			{
				int[] array = new int[41];
				string text = "";
				for (int i = 1; i < 41; i++)
				{
					if (InFolderOrder.Items.Count >= 41)
					{
						break;
					}
					array[GetFolderNumber(InFolderOrder.Items[i - 1].SubItems[0].Text)] = i;
				}
				string fullSearchString = "select * from SONG WHERE FolderNo > 0 ";
				using (DbConnection connection = new DbConnection(ConnectStringMainDB))
				{

					int num = 0;
					DbDataAdapter sQLiteDataAdapter;
					
					DataTable dataTable = null;
					(sQLiteDataAdapter, dataTable) = DbController.getDataAdapter(connection, fullSearchString);

					if (dataTable.Rows.Count > 0)
					{
						dataTable.AcceptChanges();

						DbCommandBuilder sqCB = new DbCommandBuilder(sQLiteDataAdapter);
						sQLiteDataAdapter.UpdateCommand = sqCB.GetUpdateCommand();

						foreach (DataRow dr in dataTable.Rows)
						{
							num = DataUtil.ObjToInt(dr["FolderNo"]);
							if (num != array[num])
							{
								dr["FolderNo"] = array[num];
							}
						}
						sQLiteDataAdapter.Update(dataTable);
						dataTable.Dispose();
						sQLiteDataAdapter.Dispose();
					}
					
					fullSearchString = "select * from FOLDER  where FolderNo > 0 ORDER BY FolderNo";
					(sQLiteDataAdapter, dataTable) = DbController.getDataAdapter(connection, fullSearchString);

					string text2 = "";
					if (dataTable.Rows.Count > 0)
					{
						dataTable.AcceptChanges();

						DbCommandBuilder sqCB = new DbCommandBuilder(sQLiteDataAdapter);
						sQLiteDataAdapter.UpdateCommand = sqCB.GetUpdateCommand();

						foreach (DataRow dr in dataTable.Rows)
						{
							num = DataUtil.ObjToInt(dr["FolderNo"]);
							text2 = DataUtil.ObjToString(dr["Name"]);
							dr["FolderNo"] = -array[num];
						}
						sQLiteDataAdapter.Update(dataTable);
						dataTable.Dispose();
						sQLiteDataAdapter.Dispose();
					}

					fullSearchString = "select * from FOLDER  where FolderNo < 0 ORDER BY FolderNo";
					(sQLiteDataAdapter, dataTable) = DbController.getDataAdapter(connection, fullSearchString);

					if (dataTable.Rows.Count > 0)
					{
						dataTable.AcceptChanges();

						DbCommandBuilder sqCB = new DbCommandBuilder(sQLiteDataAdapter);
						sQLiteDataAdapter.UpdateCommand = sqCB.GetUpdateCommand();

						foreach (DataRow dr in dataTable.Rows)
						{
							num = DataUtil.ObjToInt(dr["FolderNo"]);
							dr["FolderNo"] = -num;
						}
						sQLiteDataAdapter.Update(dataTable);
						dataTable.Dispose();
						sQLiteDataAdapter.Dispose();
					}					
				}
				return true;
			}
			catch(Exception ex)
			{
				Console.WriteLine(ex.Message);
				Console.WriteLine(ex.StackTrace);
			}
			return false;
		}
#endif


#if DAO
		public static void SaveFormatStringToDatabase(string SongID, string FormatString)
		{
			try
			{
				Recordset tableRecordSet = DbDaoController.GetTableRecordSet(ConnectStringMainDB, "SONG");
				tableRecordSet.Index = "PrimaryKey";
				tableRecordSet.Seek("=", SongID, def, def, def, def, def, def, def, def, def, def, def, def);
				if (!tableRecordSet.NoMatch)
				{
					tableRecordSet.Edit();
					tableRecordSet.Fields["FormatData"].Value = FormatString;
					tableRecordSet.Update();
				}
				if (tableRecordSet != null)
				{
					tableRecordSet.Close();
					tableRecordSet = null;
				}
			}
			catch
			{
			}
		}
#elif SQLite
		public static void SaveFormatStringToDatabase(string SongID, string FormatString)
		{
            int intSongID = 0;
            bool result = int.TryParse(SongID, out intSongID);

			if (!result) return;
           
			string sQuery = "select * from SONG";
			try
			{
				using DbConnection connection = DbController.GetDbConnection(ConnectStringMainDB);
				using DataTable dataTable = DbController.GetDataTable(ConnectStringMainDB, sQuery);

				DataRow dr = dataTable.Rows.Find($"{SongID}");
				if (dr != null)
				{
					dr["FormatData"] = FormatString;
				}
				DbController.UpdateTable(connection, sQuery, dataTable);
			}
			catch
			{
			}
		}
#endif

		public static string BuildItemSearchString(string InString)
		{
			return BuildItemSearchString(InString, SearchTitle: true, SearchContents: true, SearchSongNumber: true, SearchBookRef: true, SearchUserRef: true, SearchLicAdmin: true, SearchWriter: true, SearchCopyright: true, SearchNotationsOnly: false, "", "", SearchDates: false, DateTime.Now, DateTime.Now, null);
		}

#if DAO
		public static string BuildItemSearchString(string InString, bool SearchTitle, bool SearchContents, bool SearchSongNumber, bool SearchBookRef, bool SearchUserRef, bool SearchLicAdmin, bool SearchWriter, bool SearchCopyright, bool SearchNotationsOnly, string SearchSongKey, string SearchSongTiming, bool SearchDates, DateTime DateFrom, DateTime DateTo, CheckedListBox InFolderList)
		{
			string text = "";
			string text2 = "";
			string text3 = "";
			string text4 = "";
			string text5 = "";
			string text6 = "";
			string text7 = "";
			string text8 = "";
			string text9 = "";
			string text10 = "";
			string text11 = "";
			string text12 = "";
			string text13 = "";
			string text14 = "";
			int num = 0;
			string text15 = "*";
			bool flag = false;
			for (int i = 0; i <= InString.Length; i++)
			{
				text = ((!(DataUtil.Mid(InString, i, 1) != "*")) ? (text + text15) : (text + DataUtil.Mid(InString, i, 1)));
			}
			if (DataUtil.Left(text, 1) != text15)
			{
				text = text15 + text;
			}
			if (DataUtil.Right(text, 1) != text15)
			{
				text += text15;
			}
			for (int i = 1; i < 41; i++)
			{
				FindSongsFolder[i] = false;
			}
			if (InFolderList != null)
			{
				for (int i = 0; i <= InFolderList.CheckedItems.Count - 1; i++)
				{
					FindSongsFolder[GetFolderNumber(InFolderList.CheckedItems[i].ToString())] = true;
					text2 = ((!(text2 == "")) ? (text2 + " or FolderNo=" + GetFolderNumber(InFolderList.CheckedItems[i].ToString())) : (" and (FolderNo=" + GetFolderNumber(InFolderList.CheckedItems[i].ToString())));
				}
				if (text2 != "")
				{
					text2 += ")";
				}
			}
			else
			{
				for (int i = 1; i < 41; i++)
				{
					if (FolderUse[i] > 0)
					{
						FindSongsFolder[i] = true;
						text2 = ((!(text2 == "")) ? (text2 + " or FolderNo=" + i) : (" and (FolderNo=" + i));
					}
				}
				if (text2 != "")
				{
					text2 += ")";
				}
			}
			if (SearchTitle)
			{
				text3 = " (LCase(Title_1) like \"" + text.ToLower() + "\" " + text2 + ") or (LCase(Title_2) like \"" + text.ToLower() + "\" " + text2 + ")";
				flag = true;
			}
			if (SearchContents)
			{
				text7 = (flag ? " OR " : "") + " (LCase(lyrics) like \"" + text.ToLower() + "\" " + text2 + ")";
				flag = true;
			}
			int num2 = DataUtil.StringToInt(text, Minus1IfBlank: true);
			if (SearchSongNumber)
			{
				if (num2 == 0)
				{
					text4 = (flag ? " OR " : "") + " ((song_number < 1 or song_number = NULL ) " + text2 + ")";
					flag = true;
				}
				else if (num2 < 0)
				{
					text4 = (flag ? " OR " : "") + " ((song_number >= " + num2 + "  and song_number <= " + num2 + ") " + text2 + ")";
					flag = true;
				}
			}
			if (SearchBookRef)
			{
				text5 = (flag ? " OR " : "") + " (LCase(BOOK_REFERENCE) like \"" + text.ToLower() + "\" " + text2 + ")";
				flag = true;
			}
			if (SearchUserRef)
			{
				text6 = (flag ? " OR " : "") + " (LCase(USER_REFERENCE) like \"" + text.ToLower() + "\" " + text2 + ")";
				flag = true;
			}
			if (SearchLicAdmin)
			{
				text10 = (flag ? " OR " : "") + " (LCase(licence_admin1) like \"" + text.ToLower() + "\" " + text2 + ") or (LCase(licence_admin2) like \"" + text.ToLower() + "\" " + text2 + ")";
				flag = true;
			}
			if (SearchWriter)
			{
				text8 = (flag ? " OR " : "") + " (LCase(writer) like \"" + text.ToLower() + "\" " + text2 + ")";
				flag = true;
			}
			if (SearchCopyright)
			{
				text9 = (flag ? " OR " : "") + " (LCase(copyright) like \"" + text.ToLower() + "\" " + text2 + ")";
				flag = true;
			}
			if (SearchNotationsOnly)
			{
				text11 = (flag ? " AND " : "") + " (msc <> \"\")";
			}
			if (SearchSongKey != "")
			{
				text12 = (flag ? " AND " : "") + " (key = \"" + SearchSongKey + "\")";
			}
			if (SearchSongTiming != "")
			{
				text13 = (flag ? " AND " : "") + " (timing = \"" + SearchSongTiming + "\")";
			}
			if (SearchDates)
			{
				text14 = (flag ? " AND " : "") + " LastModified >=#" + DateFrom.ToString("MM-dd-yyyy") + "# and LastModified <=#" + DateTo.ToString("MM-dd-yyyy") + "# ";
			}
			string text16 = text3 + text7 + text4 + text5 + text6 + text10 + text8 + text9;
			if (text16 != "")
			{
				text16 = "(" + text16 + ")";
			}
			if ((text16 == "") & !FindItemNotationsOnly)
			{
				text16 = " title_1 = \"@!@~!~\"";
			}
			return "select * from SONG where " + text16 + text12 + text13 + text11 + text14;
		}
#elif SQLite
		public static string BuildItemSearchString(string InString, bool SearchTitle, bool SearchContents, bool SearchSongNumber, bool SearchBookRef, bool SearchUserRef, bool SearchLicAdmin, bool SearchWriter, bool SearchCopyright, bool SearchNotationsOnly, string SearchSongKey, string SearchSongTiming, bool SearchDates, DateTime DateFrom, DateTime DateTo, CheckedListBox InFolderList)
		{
			string text = "";
			string text2 = "";
			string text3 = "";
			string text4 = "";
			string text5 = "";
			string text6 = "";
			string text7 = "";
			string text8 = "";
			string text9 = "";
			string text10 = "";
			string text11 = "";
			string text12 = "";
			string text13 = "";
			string text14 = "";
			int num = 0;
			string text15 = "%";
			bool flag = false;
			for (int i = 0; i <= InString.Length; i++)
			{
				text = ((!(DataUtil.Mid(InString, i, 1) != "%")) ? (text + text15) : (text + DataUtil.Mid(InString, i, 1)));
			}
			if (DataUtil.Left(text, 1) != text15)
			{
				text = text15 + text;
			}
			if (DataUtil.Right(text, 1) != text15)
			{
				text += text15;
			}
			for (int i = 1; i < 41; i++)
			{
				FindSongsFolder[i] = false;
			}
			if (InFolderList != null)
			{
				for (int i = 0; i <= InFolderList.CheckedItems.Count - 1; i++)
				{
					FindSongsFolder[GetFolderNumber(InFolderList.CheckedItems[i].ToString())] = true;
					text2 = ((!(text2 == "")) ? (text2 + " or FolderNo=" + GetFolderNumber(InFolderList.CheckedItems[i].ToString())) : (" and (FolderNo=" + GetFolderNumber(InFolderList.CheckedItems[i].ToString())));
				}
				if (text2 != "")
				{
					text2 += ")";
				}
			}
			else
			{
				for (int i = 1; i < 41; i++)
				{
					if (FolderUse[i] > 0)
					{
						FindSongsFolder[i] = true;
						text2 = ((!(text2 == "")) ? (text2 + " or FolderNo=" + i) : (" and (FolderNo=" + i));
					}
				}
				if (text2 != "")
				{
					text2 += ")";
				}
			}
			if (SearchTitle)
			{
				text3 = " (lower(Title_1) like \"" + text.ToLower() + "\" " + text2 + ") or (lower(Title_2) like \"" + text.ToLower() + "\" " + text2 + ")";
				flag = true;
			}
			if (SearchContents)
			{
				text7 = (flag ? " OR " : "") + " (lower(lyrics) like \"" + text.ToLower() + "\" " + text2 + ")";
				flag = true;
			}
			int num2 = DataUtil.StringToInt(text, Minus1IfBlank: true);
			if (SearchSongNumber)
			{
				if (num2 == 0)
				{
					text4 = (flag ? " OR " : "") + " ((song_number < 1 or song_number = NULL ) " + text2 + ")";
					flag = true;
				}
				else if (num2 < 0)
				{
					text4 = (flag ? " OR " : "") + " ((song_number >= " + num2 + "  and song_number <= " + num2 + ") " + text2 + ")";
					flag = true;
				}
			}
			if (SearchBookRef)
			{
				text5 = (flag ? " OR " : "") + " (lower(BOOK_REFERENCE) like \"" + text.ToLower() + "\" " + text2 + ")";
				flag = true;
			}
			if (SearchUserRef)
			{
				text6 = (flag ? " OR " : "") + " (lower(USER_REFERENCE) like \"" + text.ToLower() + "\" " + text2 + ")";
				flag = true;
			}
			if (SearchLicAdmin)
			{
				text10 = (flag ? " OR " : "") + " (lower(licence_admin1) like \"" + text.ToLower() + "\" " + text2 + ") or (lower(licence_admin2) like \"" + text.ToLower() + "\" " + text2 + ")";
				flag = true;
			}
			if (SearchWriter)
			{
				text8 = (flag ? " OR " : "") + " (lower(writer) like \"" + text.ToLower() + "\" " + text2 + ")";
				flag = true;
			}
			if (SearchCopyright)
			{
				text9 = (flag ? " OR " : "") + " (lower(copyright) like \"" + text.ToLower() + "\" " + text2 + ")";
				flag = true;
			}
			if (SearchNotationsOnly)
			{
				text11 = (flag ? " AND " : "") + " (msc <> \"\")";
			}
			if (SearchSongKey != "")
			{
				text12 = (flag ? " AND " : "") + " (key = \"" + SearchSongKey + "\")";
			}
			if (SearchSongTiming != "")
			{
				text13 = (flag ? " AND " : "") + " (timing = \"" + SearchSongTiming + "\")";
			}
			if (SearchDates)
			{
				text14 = (flag ? " AND " : "") + " LastModified >=#" + DateFrom.ToString("MM-dd-yyyy") + "# and LastModified <=#" + DateTo.ToString("MM-dd-yyyy") + "# ";
			}
			string text16 = text3 + text7 + text4 + text5 + text6 + text10 + text8 + text9;
			if (text16 != "")
			{
				text16 = "(" + text16 + ")";
			}
			if ((text16 == "") & !FindItemNotationsOnly)
			{
				text16 = " title_1 = \"@!@~!~\"";
			}
			return "select * from SONG where " + text16 + text12 + text13 + text11 + text14;
		}
#endif

		public static string BuildBibleSearchString(string InSearchPassage, int VersionIndex)
		{
			return BuildBibleSearchString(InSearchPassage, VersionIndex, 0, 2);
		}

#if DAO
		public static string BuildBibleSearchString(string InSearchPassage, int VersionIndex, int BookIndex, int MatchSelected)
		{
			string text = "\"*[ -/:-@]";
			string text2 = "[ -/:-@]*\"";
			if (PartialWordSearch(VersionIndex))
			{
				text = "\"*";
				text2 = "*\"";
			}
			if (DataUtil.Trim(InSearchPassage).Length > 0)
			{
				InSearchPassage = DataUtil.Trim(InSearchPassage.ToLower());
				sArray = InSearchPassage.Split(' ');
				string text3 = "";
				string text4 = "";
				string text5 = "";
				string text6 = "";
				string text7 = "";
				text3 = "select * from Bible where book";
				text3 = ((BookIndex >= 1) ? (text3 + "=" + BookIndex) : (text3 + ">0 "));
				switch (MatchSelected)
				{
					case 1:
						{
							for (int i = 0; i <= sArray.GetUpperBound(0); i++)
							{
								if (i > 0)
								{
									text4 += " or ";
									text5 += " or ";
									text6 += " or ";
									text7 += " or ";
								}
								string text8 = text4;
								text4 = text8 + " LCase(bibletext) like " + text + DataUtil.Trim(sArray[i]) + text2;
								text5 = text5 + " LCase(bibletext) like \"" + DataUtil.Trim(sArray[i]) + text2;
								text8 = text6;
								text6 = text8 + " LCase(bibletext) like " + text + DataUtil.Trim(sArray[i]) + "\"";
								text7 = text7 + " LCase(bibletext) like \"" + DataUtil.Trim(sArray[i]) + "\"";
							}
							break;
						}
					case 0:
						{
							for (int i = 0; i <= sArray.GetUpperBound(0); i++)
							{
								if (i > 0)
								{
									text4 += " and ";
									text5 += " and ";
									text6 += " and ";
									text7 += " and ";
								}
								string text8 = text4;
								text4 = text8 + " LCase(bibletext) like " + text + sArray[i] + text2;
								text5 = text5 + " LCase(bibletext) like \"" + sArray[i] + text2;
								text8 = text6;
								text6 = text8 + " LCase(bibletext) like " + text + sArray[i] + "\"";
								text7 = text7 + " LCase(bibletext) like \"" + sArray[i] + "\"";
							}
							break;
						}
					default:
						text4 = " LCase(bibletext) like " + text + DataUtil.Trim(InSearchPassage).ToLower() + text2;
						text5 = " LCase(bibletext) like \"" + DataUtil.Trim(InSearchPassage).ToLower() + text2;
						text6 = " LCase(bibletext) like " + text + DataUtil.Trim(InSearchPassage).ToLower() + "\"";
						text7 = " LCase(bibletext) like \"" + DataUtil.Trim(InSearchPassage).ToLower() + "\"";
						break;
				}
				text4 = DataUtil.Trim(text4);
				text5 = DataUtil.Trim(text5);
				text6 = DataUtil.Trim(text6);
				text7 = DataUtil.Trim(text7);
				if (text4 != "")
				{
					text3 = text3 + " AND ( (" + text4 + ")";
					if (!PartialWordSearch(VersionIndex))
					{
						string text8 = text3;
						text3 = text8 + " OR (" + text5 + ") OR (" + text6 + ") OR (" + text7 + ")";
					}
					return text3 + " ) order by Book, chapter, verse ";
				}
			}
			return "";
		}
#elif SQLite
		public static string BuildBibleSearchString(string InSearchPassage, int VersionIndex, int BookIndex, int MatchSelected)
		{
			string text = "'*[ -/:-@]";
			string text2 = "[ -/:-@]*'";
			if (PartialWordSearch(VersionIndex))
			{
				text = "'%";
				text2 = "%'";
			}
			if (DataUtil.Trim(InSearchPassage).Length > 0)
			{
				InSearchPassage = DataUtil.Trim(InSearchPassage.ToLower());
				sArray = InSearchPassage.Split(' ');
				string text3 = "";
				string text4 = "";
				string text5 = "";
				string text6 = "";
				string text7 = "";
				text3 = "select * from Bible where book";
				text3 = ((BookIndex >= 1) ? (text3 + "=" + BookIndex) : (text3 + ">0 "));
				switch (MatchSelected)
				{
					case 1:
						{
							for (int i = 0; i <= sArray.GetUpperBound(0); i++)
							{
								string term = DataUtil.Trim(sArray[i]).Replace("'", "''");
								if (i > 0)
								{
									text4 += " or ";
									text5 += " or ";
									text6 += " or ";
									text7 += " or ";
								}
								string text8 = text4;
								text4 = text8 + " lower(bibletext) like " + text + term + text2;
								text5 = text5 + " lower(bibletext) like '" + term + text2;
								text8 = text6;
								text6 = text8 + " lower(bibletext) like " + text + term + "'";
								text7 = text7 + " lower(bibletext) like '" + term + "'";
							}
							break;
						}
					case 0:
						{
							for (int i = 0; i <= sArray.GetUpperBound(0); i++)
							{
								string term = DataUtil.Trim(sArray[i]).Replace("'", "''");
								if (i > 0)
								{
									text4 += " and ";
									text5 += " and ";
									text6 += " and ";
									text7 += " and ";
								}
								string text8 = text4;
								text4 = text8 + " lower(bibletext) like " + text + term + text2;
								text5 = text5 + " lower(bibletext) like '" + term + text2;
								text8 = text6;
								text6 = text8 + " lower(bibletext) like " + text + term + "'";
								text7 = text7 + " lower(bibletext) like '" + term + "'";
							}
							break;
						}
					default:
						string passage = DataUtil.Trim(InSearchPassage).ToLower().Replace("'", "''");
						text4 = " lower(bibletext) like " + text + passage + text2;
						text5 = " lower(bibletext) like '" + passage + text2;
						text6 = " lower(bibletext) like " + text + passage + "'";
						text7 = " lower(bibletext) like '" + passage + "'";
						break;
				}
				text4 = DataUtil.Trim(text4);
				text5 = DataUtil.Trim(text5);
				text6 = DataUtil.Trim(text6);
				text7 = DataUtil.Trim(text7);
				if (text4 != "")
				{
					text3 = text3 + " AND ( (" + text4 + ")";
					if (!PartialWordSearch(VersionIndex))
					{
						string text8 = text3;
						text3 = text8 + " OR (" + text5 + ") OR (" + text6 + ") OR (" + text7 + ")";
					}
					return text3 + " ) order by Book, chapter, verse ";
				}
			}
			return "";
		}
#endif

		public static bool PartialWordSearch(int VersionIndex)
		{
			if (VersionIndex < 0) 
				return false;

			try
			{
				string connectString = ConnectStringDef + HB_Versions[VersionIndex, 4];
				string fullSearchString = "select * from Bible where book=0 and chapter=0 and verse=20";
#if OleDb
				if (DbOleDbController.GetDataTable(connectString, fullSearchString).Rows.Count > 0)
				{
					return true;
				}
#elif SQLite
				if (DbController.GetDataTable(connectString, fullSearchString).Rows.Count > 0)
				{
					return true;
				}
#endif

			}
			catch(Exception ex)
			{
				Console.WriteLine(ex.Message);
				Console.WriteLine(ex.StackTrace);
			}
			return false;
		}


#if DAO
		public static bool SearchBiblePassages(int InBibleVersion, ref ComboBox InBookList, string InSelectString, ref RichTextBox InTextContainer, bool InShowVerses)
		{
			int num = 0;
			string text = "";
			string connectString = ConnectStringDef + HB_Versions[InBibleVersion, 4];
			StringBuilder stringBuilder = new StringBuilder();
			Recordset recordSet = DbDaoController.GetRecordSet(connectString, InSelectString);
			if (!(recordSet?.EOF ?? true))
			{
				recordSet.MoveFirst();
				while (!recordSet.EOF && num < 3000)
				{
					num++;
					int dataInt = DataUtil.GetDataInt(recordSet, "book");
					int dataInt2 = DataUtil.GetDataInt(recordSet, "chapter");
					int dataInt3 = DataUtil.GetDataInt(recordSet, "verse");
					text = string.Concat(InBookList.Items[dataInt - 1], " ", dataInt2, ":", dataInt3, " ");
					HB_VersesLocation[num, 0] = InBibleVersion;
					HB_VersesLocation[num, 1] = dataInt;
					HB_VersesLocation[num, 2] = dataInt2;
					HB_VersesLocation[num, 3] = dataInt3;
					HB_VersesLocation[num, 4] = stringBuilder.Length;
					stringBuilder.Append(text + (InShowVerses ? DataUtil.GetDataString(recordSet, "bibletext") : "") + "\n\n");
					HB_VersesLocation[num, 5] = stringBuilder.Length + 1 - HB_VersesLocation[num, 4];
					recordSet.MoveNext();
				}
				HB_VersesLocation[0, 0] = num;
				InTextContainer.Text = DataUtil.TrimEnd(stringBuilder.ToString());
				if (num >= 3000)
				{
					MessageBox.Show("The number of search results has been limited to " + Convert.ToString(3000) + ".");
				}
				return true;
			}
			return false;
		}
#elif SQLite
		public static bool SearchBiblePassages(int InBibleVersion, ref ComboBox InBookList, string InSelectString, ref RichTextBox InTextContainer, bool InShowVerses)
		{
			int num = 0;
			string text = "";
			string connectString = ConnectStringDef + HB_Versions[InBibleVersion, 4];
			StringBuilder stringBuilder = new StringBuilder();

			DbConnection connection = null;
			DbDataReader dataReader = null;

			(connection, dataReader) = DbController.GetDataReader(connectString, InSelectString);

			//Recordset recordSet = DbDaoController.GetRecordSet(connectString, InSelectString);

			using (connection)
			{
				using (dataReader)
				{
					if (dataReader != null && dataReader.HasRows)
					{
						//recordSet.MoveFirst();
						while (dataReader.Read() && num < 3000)
						{
							num++;
							int dataInt = DataUtil.GetDataInt(dataReader, "book");
							int dataInt2 = DataUtil.GetDataInt(dataReader, "chapter");
							int dataInt3 = DataUtil.GetDataInt(dataReader, "verse");
							text = string.Concat(InBookList.Items[dataInt - 1], " ", dataInt2, ":", dataInt3, " ");
							HB_VersesLocation[num, 0] = InBibleVersion;
							HB_VersesLocation[num, 1] = dataInt;
							HB_VersesLocation[num, 2] = dataInt2;
							HB_VersesLocation[num, 3] = dataInt3;
							HB_VersesLocation[num, 4] = stringBuilder.Length;
							stringBuilder.Append(text + (InShowVerses ? DataUtil.GetDataString(dataReader, "bibletext") : "") + "\n\n");
							HB_VersesLocation[num, 5] = stringBuilder.Length + 1 - HB_VersesLocation[num, 4];
						}
						HB_VersesLocation[0, 0] = num;
						InTextContainer.Text = DataUtil.TrimEnd(stringBuilder.ToString());
						if (num >= 3000)
						{
							MessageBox.Show("The number of search results has been limited to " + Convert.ToString(3000) + ".");
						}
						return true;
					}
				}
			}
			return false;
		}
#endif

		public static bool FormInUse(string InFormName)
		{
			return (FindWindow(null, InFormName).ToInt32() > 0) ? true : false;
		}

		public static void SetMenuItem(ref ToolStripMenuItem InMenuItem, string ItemName, string DefaultText, string MergedText, bool DisableWhenBlank)
		{
			if (ItemName == "")
			{
				InMenuItem.Text = DefaultText;
				InMenuItem.Enabled = ((!DisableWhenBlank) ? true : false);
				return;
			}
			int num = 20;
			string str = (ItemName.Length <= num) ? "'" : "...'";
			InMenuItem.Text = MergedText + " '" + DataUtil.Left(ItemName, num) + str;
			InMenuItem.Enabled = true;
		}

		public static void Play_Media(string title1, string title2)
		{
			string mediaFileName = GetMediaFileName(title1, title2);
			if (!RunProcess(mediaFileName))
			{
				MessageBox.Show("Sorry, cannot find any media file for '" + title1 + "'" + ((title2 != "") ? (" or '" + title2 + "'") : ""));
			}
		}

		/// <summary>
		/// daniel 2020년 5월 6일 수정
		/// </summary>
		/// <param name="InProcessString"></param>
		/// <returns></returns>
		public static bool RunProcess(string InProcessString)
		{
			try
			{
				ProcessStartInfo psi = new ProcessStartInfo
				{
					WindowStyle = ProcessWindowStyle.Normal,
					FileName = InProcessString,
					UseShellExecute = true
				};
				Process.Start(psi);

				//.net core에서는 아래와 같이 하면 동작을 하지 않음(파워포인트를 열수 없음)
				//Process process = Process.Start(InProcessString);
			}
			catch(Exception e)
			{
				Console.WriteLine(e.Message);
				Console.WriteLine(e.StackTrace);
				return false;
			}
			return true;
		}

		public static string LookupDBTitle2(int InKey)
		{
			try
			{
				string fullSearchString = "select * from SONG where songid=" + Convert.ToString(InKey);
#if OleDb
				DataTable datatable = DbOleDbController.GetDataTable(ConnectStringMainDB, fullSearchString);
#elif SQLite
				using DataTable datatable = DbController.GetDataTable(ConnectStringMainDB, fullSearchString);
#endif
				if (datatable.Rows.Count>0)
				{
					return DataUtil.ObjToString(datatable.Rows[0]["Title_2"]);
				}
			}
			catch
			{
			}
			return "";
		}

		public static Font GetNewFont(string InFontName, int InFontSize, bool InBold, bool InItalic, bool InUnderline)
		{
			return GetNewFont(InFontName, InFontSize, InBold, InItalic, InUnderline, ShowErrorMsg: true);
		}

		public static Font GetNewFont(string InFontName, int InFontSize, bool InBold, bool InItalic, bool InUnderline, bool ShowErrorMsg)
		{
			FontStyle fontStyle = FontStyle.Regular;
			if (InBold)
			{
				fontStyle |= FontStyle.Bold;
			}
			if (InItalic)
			{
				fontStyle |= FontStyle.Italic;
			}
			if (InUnderline)
			{
				fontStyle |= FontStyle.Underline;
			}
			try
			{
				return new Font(InFontName, InFontSize, fontStyle);
			}
			catch
			{
				if (ShowErrorMsg)
				{
				}
				return new Font("Microsoft Sans Serif", InFontSize, fontStyle);
			}
		}

		public static string TransposeChord(string InNotation, int TransposeStep, int FlatSharpKey)
		{
			if (InNotation == "")
			{
				return "";
			}
			if (TransposeStep == 0)
			{
				return InNotation;
			}
			string[] array = InNotation.Split('/');
			string str = TransposeOneChord(array[0], TransposeStep, accidentals: false, FlatSharpKey);
			string str2 = "";
			if (array.GetUpperBound(0) > 0)
			{
				str2 = "/" + TransposeOneChord(array[1], TransposeStep, accidentals: true, FlatSharpKey);
			}
			return str + str2;
		}

		public static string TransposeOneChord(string InChord, int TransposeStep, bool accidentals, int FlatSharpKey)
		{
			int start = 0;
			bool flag = false;
			int num = -1;
			int num2 = 0;
			string text = DataUtil.Left(InChord, 1);
			if (text == "[" || text == "{")
			{
				InChord = DataUtil.Mid(InChord, 1);
			}
			else
			{
				text = "";
			}
			string text2 = DataUtil.Left(InChord, 2);
			while (text2.Length > 0 && !flag)
			{
				start = text2.Length;
				for (int i = 0; i <= 11; i++)
				{
					if ((MusicMajorChords[i, 0] == text2) | (MusicMajorChords[i, 1] == text2))
					{
						flag = true;
						num = i;
						num2 = 0;
						i = 12;
					}
				}
				if (!flag)
				{
					for (int i = 0; i <= 11; i++)
					{
						if ((MusicMinorChords[i, 0] == text2) | (MusicMinorChords[i, 1] == text2))
						{
							flag = true;
							num = i;
							num2 = 1;
							i = 12;
						}
					}
				}
				if (!flag)
				{
					text2 = DataUtil.Left(text2, text2.Length - 1);
				}
			}
			if (flag)
			{
				string text3 = "";
				if (FlatSharpKey < 0 || FlatSharpKey > 1)
				{
					FlatSharpKey = ((TransposeStep > 0) ? 1 : 0);
				}
				int num3 = num + TransposeStep;
				if (num3 < 0)
				{
					num3 = 11 + num3 + 1;
				}
				if (num3 > 11)
				{
					num3 = num3 - 11 - 1;
				}
				text3 = ((num2 != 0) ? MusicMinorChords[num3, (FlatSharpKey > 0) ? 1 : 0] : MusicMajorChords[num3, (FlatSharpKey > 0) ? 1 : 0]);
				return text + text3 + DataUtil.Mid(InChord, start);
			}
			return text + InChord;
		}

		public static int TransposeKey(ref string InKey, int TransposeStep)
		{
			if (InKey == "")
			{
				return (TransposeStep > 0) ? 1 : 0;
			}
			bool flag = false;
			int num = -1;
			int num2 = 0;
			for (int i = 0; i <= 11; i++)
			{
				if (MusicMajorKeys[i] == InKey)
				{
					flag = true;
					num = i;
					num2 = 0;
					i = 12;
				}
			}
			if (!flag)
			{
				for (int i = 0; i <= 11; i++)
				{
					if (MusicMinorKeys[i] == InKey)
					{
						flag = true;
						num = i;
						num2 = 1;
						i = 12;
					}
				}
			}
			if (flag)
			{
				string text = "";
				int num3 = -1;
				int num4 = num + TransposeStep;
				if (num4 < 0)
				{
					num4 = 11 + num4 + 1;
				}
				if (num4 > 11)
				{
					num4 = num4 - 11 - 1;
				}
				if (num2 == 0)
				{
					text = MusicMajorKeys[num4];
					num3 = MusicMajorKeysFlatSharp[num4];
				}
				else
				{
					text = MusicMinorKeys[num4];
					num3 = MusicMinorKeysFlatSharp[num4];
				}
				InKey = text;
				return num3;
			}
			return -1;
		}

		public static bool ValidSongID(int InSongID)
		{
			try
			{
				string fullSearchString = "select * from SONG where songid=" + InSongID;
#if OleDb
				DataTable datatable = DbOleDbController.GetDataTable(ConnectStringMainDB, fullSearchString);
#elif SQLite
				using DataTable datatable = DbController.GetDataTable(ConnectStringMainDB, fullSearchString);
#endif
				
				if (datatable.Rows.Count > 0)
				{
					return (FolderUse[DataUtil.ObjToInt(datatable.Rows[0]["FolderNo"])] > 0) ? true : false;
				}
			}
			catch
			{
			}
			return false;
		}

		public static void GetCurPosInLine(string instring, ref int CurPos)
		{
			MoveToPosInLine(instring, ref CurPos, 0);
		}

		public static void MoveToPosInLine(string instring, ref int CurPos, int InMode)
		{
			if (instring.Length == 0)
			{
				return;
			}
			bool flag = false;
			int num = CurPos + ((InMode == 0) ? (-1) : 0);
			string text = "";
			while (num >= 0 && num < instring.Length && !flag)
			{
				text = DataUtil.Mid(instring, num, 1);
				if ((text == "\r") | (text == "\n"))
				{
					flag = true;
					CurPos = num + ((InMode == 0) ? 1 : 0);
					num = 0;
				}
				else
				{
					num += ((InMode != 0) ? 1 : (-1));
				}
			}
			if (!flag)
			{
				CurPos = ((InMode != 0) ? instring.Length : 0);
			}
		}

		public static void InsertChordAboveCurrentLine(ref RichTextBox InTextBox, string InChordString)
		{
			bool flag = (InChordString[0].ToString() == "/") ? true : false;
			int i = InTextBox.SelectionStart;
			int curPos = i;
			int num = i;
			int length = InTextBox.Text.Length;
			Point positionFromCharIndex = InTextBox.GetPositionFromCharIndex(i);
			int lineFromCharIndex = InTextBox.GetLineFromCharIndex(i);
			bool flag2 = false;
			int num2 = InTextBox.Text.IndexOf("»", i);
			if (InTextBox.GetLineFromCharIndex(num2) != lineFromCharIndex)
			{
				num2 = -1;
			}
			int num3 = -1;
			if (num2 >= 0)
			{
				num3 = i;
			}
			else
			{
				InChordString += " ";
				if (lineFromCharIndex < 1)
				{
					flag2 = true;
				}
				else
				{
					string textFromPreviousLine = GetTextFromPreviousLine(InTextBox.Text, curPos);
					if (textFromPreviousLine.IndexOf("»") < 0)
					{
						flag2 = true;
					}
				}
				if (flag2)
				{
					InsertIndicator(ref InTextBox, 151);
					InTextBox.SelectionStart -= 1;
					InsertIndicator(ref InTextBox, 152);
					i = InTextBox.SelectionStart;
				}
				else
				{
					GetCurPosInLine(InTextBox.Text, ref i);
					i--;
				}
			}
			int CurPos = -1;
			MoveToPosInLine(InTextBox.Text, ref CurPos, 1);
			GetCurPosInLine(InTextBox.Text, ref i);
			int num4 = i;
			InTextBox.SelectionStart = i;
			num2 = InTextBox.Text.IndexOf("»", i);
			if (num2 < 0)
			{
				return;
			}
			if (num3 >= 0)
			{
				i = num3;
			}
			InTextBox.SelectionStart = num2;
			Point positionFromCharIndex2 = InTextBox.GetPositionFromCharIndex(InTextBox.SelectionStart);
			while (positionFromCharIndex2.X < positionFromCharIndex.X)
			{
				InTextBox.SelectedText = " ";
				positionFromCharIndex2 = InTextBox.GetPositionFromCharIndex(InTextBox.SelectionStart);
			}
			GetCurPosInLine(InTextBox.Text, ref i);
			for (; InTextBox.GetPositionFromCharIndex(i).X < positionFromCharIndex.X; i++)
			{
			}
			bool flag3 = false;
			while (i > num4 && i < num2 && !flag3)
			{
				if (InTextBox.Text[i - 1].ToString() == " ")
				{
					flag3 = true;
				}
				else if (InTextBox.Text[i].ToString() == " ")
				{
					i++;
					flag3 = true;
				}
				else
				{
					i++;
				}
			}
			Point point = new Point(-1, -1);
			int num5 = -1;
			for (int j = i; j <= num2; j++)
			{
				if (InTextBox.Text[j].ToString() != " ")
				{
					point = InTextBox.GetPositionFromCharIndex(j);
					num5 = j;
					j = num2 + 1;
				}
			}
			InTextBox.SelectionStart = i;
			InTextBox.SelectedText = InChordString;
			InTextBox.SelectedText = "";
			if (num5 >= 0)
			{
				num5 += InChordString.Length - 1;
				int x = InTextBox.GetPositionFromCharIndex(num5).X;
				while (x >= point.X && InTextBox.Text[num5 - 1].ToString() == " " && InTextBox.Text[num5 - 2].ToString() == " ")
				{
					InTextBox.SelectionStart = num5;
					InTextBox.SelectionLength = 1;
					InTextBox.SelectedText = "";
					num5--;
					x = InTextBox.GetPositionFromCharIndex(num5).X;
				}
			}
			InTextBox.SelectionStart = ((num3 >= 0) ? (num3 + InChordString.Length) : (num + (InTextBox.Text.Length - length)));
			InTextBox.SelectedText = "";
		}

		public static string GetTextFromPreviousLine(string InString, int CurPos)
		{
			GetCurPosInLine(InString, ref CurPos);
			if (CurPos > 0)
			{
				CurPos--;
			}
			int num = CurPos;
			GetCurPosInLine(InString, ref CurPos);
			int num2 = num - CurPos;
			string inString = "";
			if (num2 > 0)
			{
				inString = DataUtil.Mid(InString, CurPos, num2);
			}
			return DataUtil.Trim(inString);
		}

		public static void OldGetCurPosInLine(string instring, ref int CurPos, int InPos)
		{
			if (instring.Length == 0)
			{
				return;
			}
			bool flag = false;
			int num = CurPos + 1 + ((InPos == 0) ? (-1) : 0);
			while ((num > 1) & (num < instring.Length + 1))
			{
				if ((DataUtil.Mid(instring, num, 1) == "\r") | (DataUtil.Mid(instring, num, 1) == "\n"))
				{
					flag = true;
					CurPos = num + ((InPos != 0) ? (-1) : 0);
					num = 1;
				}
				else
				{
					num += ((InPos != 0) ? 1 : (-1));
				}
			}
			if (!flag)
			{
				CurPos = ((InPos != 0) ? instring.Length : 0);
			}
		}

		public static string MakeTitleValidFileName(string InString)
		{
			char[] array = new char[9]
			{
				'\\',
				'/',
				':',
				'*',
				'?',
				'"',
				'<',
				'>',
				'|'
			};
			for (int i = 0; i < array.Length; i++)
			{
				InString = InString.Replace(array[i], '_');
			}
			InString = DataUtil.Left(InString, 255);
			return InString;
		}

		public static bool ValidateTitleDetails(string InString, string Heading)
		{
			char[] anyOf = new char[6]
			{
				'[',
				']',
				'*',
				'"',
				'<',
				'>'
			};
			if (InString.IndexOfAny(anyOf) >= 0)
			{
				MessageBox.Show(Heading + " must not contain the characters: ] [ * \" < >");
				return false;
			}
			if (InString.Length > 100)
			{
				MessageBox.Show(Heading + " is too long (" + Convert.ToString(InString.Length) + "), maximum length is 100 characters including spaces");
				return false;
			}
			return true;
		}

		public static void FormatPlainLyrics(ref RichTextBox InTextBox)
		{
			string text = "";
			string text2 = InTextBox.Text;
			text = text2.Replace("\r\n", "\n");
			text = text.Replace("\r", "");
			for (int num = 99; num > 0; num--)
			{
				text = text.Replace("Verse   " + num, num.ToString());
				text = text.Replace("verse   " + num, num.ToString());
				text = text.Replace("VERSE   " + num, num.ToString());
				text = text.Replace("Verse  " + num, num.ToString());
				text = text.Replace("verse  " + num, num.ToString());
				text = text.Replace("VERSE  " + num, num.ToString());
				text = text.Replace("Verse   " + num, num.ToString());
				text = text.Replace("verse   " + num, num.ToString());
				text = text.Replace("VERSE   " + num, num.ToString());
				text = text.Replace("Verse  " + num, num.ToString());
				text = text.Replace("verse  " + num, num.ToString());
				text = text.Replace("VERSE  " + num, num.ToString());
				text = text.Replace("Verse " + num, num.ToString());
				text = text.Replace("verse " + num, num.ToString());
				text = text.Replace("VERSE " + num, num.ToString());
				text = text.Replace("Verse" + num, num.ToString());
				text = text.Replace("verse" + num, num.ToString());
				text = text.Replace("VERSE" + num, num.ToString());
				text = text.Replace("Ver   " + num, num.ToString());
				text = text.Replace("ver   " + num, num.ToString());
				text = text.Replace("VER   " + num, num.ToString());
				text = text.Replace("Ver  " + num, num.ToString());
				text = text.Replace("ver  " + num, num.ToString());
				text = text.Replace("VER  " + num, num.ToString());
				text = text.Replace("Ver " + num, num.ToString());
				text = text.Replace("ver " + num, num.ToString());
				text = text.Replace("VER " + num, num.ToString());
				text = text.Replace("Ver" + num, num.ToString());
				text = text.Replace("ver" + num, num.ToString());
				text = text.Replace("VER" + num, num.ToString());
				text = text.Replace("V   " + num, num.ToString());
				text = text.Replace("v   " + num, num.ToString());
				text = text.Replace("V  " + num, num.ToString());
				text = text.Replace("v  " + num, num.ToString());
				text = text.Replace("V " + num, num.ToString());
				text = text.Replace("v " + num, num.ToString());
				text = text.Replace("V" + num, num.ToString());
				text = text.Replace("v" + num, num.ToString());
			}
			if (text.IndexOf("1") >= 0)
			{
				if (DataUtil.Left(text, 7) == "1.     ")
				{
					text = "[1]\n" + DataUtil.Mid(text, 8);
				}
				else if (DataUtil.Left(text, 6) == "1.    ")
				{
					text = "[1]\n" + DataUtil.Mid(text, 7);
				}
				else if (DataUtil.Left(text, 5) == "1.   ")
				{
					text = "[1]\n" + DataUtil.Mid(text, 6);
				}
				else if (DataUtil.Left(text, 4) == "1.  ")
				{
					text = "[1]\n" + DataUtil.Mid(text, 5);
				}
				else if (DataUtil.Left(text, 3) == "1. ")
				{
					text = "[1]\n" + DataUtil.Mid(text, 4);
				}
				else if (DataUtil.Left(text, 2) == "1.")
				{
					text = "[1]\n" + DataUtil.Mid(text, 3);
				}
				else if (DataUtil.Left(text, 7) == "1:     ")
				{
					text = "[1]\n" + DataUtil.Mid(text, 8);
				}
				else if (DataUtil.Left(text, 6) == "1:    ")
				{
					text = "[1]\n" + DataUtil.Mid(text, 7);
				}
				else if (DataUtil.Left(text, 5) == "1:   ")
				{
					text = "[1]\n" + DataUtil.Mid(text, 6);
				}
				else if (DataUtil.Left(text, 4) == "1:  ")
				{
					text = "[1]\n" + DataUtil.Mid(text, 5);
				}
				else if (DataUtil.Left(text, 3) == "1: ")
				{
					text = "[1]\n" + DataUtil.Mid(text, 4);
				}
				else if (DataUtil.Left(text, 2) == "1:")
				{
					text = "[1]\n" + DataUtil.Mid(text, 3);
				}
				else if (DataUtil.Left(text, 7) == "1      ")
				{
					text = "[1]\n" + DataUtil.Mid(text, 8);
				}
				else if (DataUtil.Left(text, 6) == "1     ")
				{
					text = "[1]\n" + DataUtil.Mid(text, 7);
				}
				else if (DataUtil.Left(text, 5) == "1    ")
				{
					text = "[1]\n" + DataUtil.Mid(text, 6);
				}
				else if (DataUtil.Left(text, 4) == "1   ")
				{
					text = "[1]\n" + DataUtil.Mid(text, 5);
				}
				else if (DataUtil.Left(text, 3) == "1  ")
				{
					text = "[1]\n" + DataUtil.Mid(text, 4);
				}
				else if (DataUtil.Left(text, 2) == "1 ")
				{
					text = "[1]\n" + DataUtil.Mid(text, 3);
				}
				else if (DataUtil.Left(text, 2) == "1\n")
				{
					text = "[1]\n" + DataUtil.Mid(text, 3);
				}
			}
			for (int num = 99; num > 0; num--)
			{
				if (text.IndexOf(num.ToString()) >= 0)
				{
					text = text.Replace("\n" + num + ".     ", "\n[" + num + "]\n");
					text = text.Replace("\n" + num + ".    ", "\n[" + num + "]\n");
					text = text.Replace("\n" + num + ".   ", "\n[" + num + "]\n");
					text = text.Replace("\n" + num + ".  ", "\n[" + num + "]\n");
					text = text.Replace("\n" + num + ". ", "\n[" + num + "]\n");
					text = text.Replace("\n" + num + ".", "\n[" + num + "]\n");
					text = text.Replace("\n" + num + ":     ", "\n[" + num + "]\n");
					text = text.Replace("\n" + num + ":    ", "\n[" + num + "]\n");
					text = text.Replace("\n" + num + ":   ", "\n[" + num + "]\n");
					text = text.Replace("\n" + num + ":  ", "\n[" + num + "]\n");
					text = text.Replace("\n" + num + ": ", "\n[" + num + "]\n");
					text = text.Replace("\n" + num + ":", "\n[" + num + "]\n");
					text = text.Replace("\n" + num + "     ", "\n[" + num + "]\n");
					text = text.Replace("\n" + num + "    ", "\n[" + num + "]\n");
					text = text.Replace("\n" + num + "   ", "\n[" + num + "]\n");
					text = text.Replace("\n" + num + "  ", "\n[" + num + "]\n");
					text = text.Replace("\n" + num + " ", "\n[" + num + "]\n");
					text = text.Replace("\n" + num + " ", "\n[" + num + "]\n");
					text = text.Replace("\n" + num + "\n", "\n[" + num + "]\n");
					text = text.Replace("\n" + num + "\t", "\n[" + num + "]\n");
					text = text.Replace("\n\n[" + num + "]", "\n[" + num + "]\n");
					text = text.Replace("[" + num + "]\n\n", "[" + num + "]\n");
				}
			}
			string text3 = "ChoruS";
			string text4 = "[chorus]\n";
			text = text.Replace("\nchorus", "\n" + text3);
			text = text.Replace("\nChorus", "\n" + text3);
			text = text.Replace("\nCHORUS", "\n" + text3);
			if (DataUtil.Left(text, 6) == "chorus")
			{
				text = text3 + DataUtil.Mid(text, 6);
			}
			if (DataUtil.Left(text, 6) == "Chorus")
			{
				text = text3 + DataUtil.Mid(text, 6);
			}
			if (DataUtil.Left(text, 6) == "CHORUS")
			{
				text = text3 + DataUtil.Mid(text, 6);
			}
			if (text.IndexOf(text3) >= 0)
			{
				text = text.Replace(text3 + ".     ", text4);
				text = text.Replace(text3 + ".    ", text4);
				text = text.Replace(text3 + ".   ", text4);
				text = text.Replace(text3 + ".  ", text4);
				text = text.Replace(text3 + ". ", text4);
				text = text.Replace(text3 + ".", text4);
				text = text.Replace(text3 + ":     ", text4);
				text = text.Replace(text3 + ":    ", text4);
				text = text.Replace(text3 + ":   ", text4);
				text = text.Replace(text3 + ":  ", text4);
				text = text.Replace(text3 + ": ", text4);
				text = text.Replace(text3 + ":", text4);
				text = text.Replace(text3 + "     ", text4);
				text = text.Replace(text3 + "    ", text4);
				text = text.Replace(text3 + "   ", text4);
				text = text.Replace(text3 + "  ", text4);
				text = text.Replace(text3 + " ", text4);
				text = text.Replace(text3 + "\t", text4);
				text = text.Replace(text3, text4);
				text = text.Replace("\n\n" + text4, "\n" + text4);
				text = text.Replace(text4 + "\n", text4);
			}
			if (text.IndexOf("[2]") >= 0 && text.IndexOf("[1]") < 0)
			{
				text = "[1]\n" + text;
			}
			text = text.Replace("\t\n", "\n");
			text = text.Replace("\t", "");
			text = text.Replace("\n\n\n", "\n\n");
			text = text.Replace("\n\n[", "\n[");
			text = DataUtil.TrimEnd(text);
			InTextBox.Text = text;
		}

		public static void Merge_Songs(SongSettings InItem1, SongSettings InItem2, ref string CombinedLyrics, ref string CombinedNotations)
		{
			FormatDisplayLyrics(ref InItem1, PrepareSlides: false, UseStoredSequence: true);
			FormatDisplayLyrics(ref InItem2, PrepareSlides: false, UseStoredSequence: true);
			int[] array = new int[160];
			ListViewItem listViewItem = new ListViewItem();
			TempItem1.LyricsAndNotationsList.Items.Clear();
			bool flag = false;
			for (int i = 0; i < InItem1.SongSequence.Length; i++)
			{
				int num = InItem1.SongSequence[i];
				if (i == 0 && (InItem1.CompleteLyrics.IndexOf(VerseSymbol[num]) >= 0 || InItem2.CompleteLyrics.IndexOf(VerseSymbol[num]) >= 0))
				{
					flag = true;
				}
				listViewItem = TempItem1.LyricsAndNotationsList.Items.Add(VerseSymbol[num]);
				listViewItem.SubItems.Add("");
				for (int j = InItem1.VerseLineLoc[num, 1]; (j >= 0) & (j <= InItem1.VerseLineLoc[num, 2]); j++)
				{
					listViewItem = TempItem1.LyricsAndNotationsList.Items.Add(InItem1.LyricsAndNotationsList.Items[j].SubItems[2].Text);
					listViewItem.SubItems.Add(InItem1.LyricsAndNotationsList.Items[j].SubItems[3].Text);
				}
				if (InItem2.VersePresent[num] & (InItem2.CompleteLyrics != ""))
				{
					listViewItem = TempItem1.LyricsAndNotationsList.Items.Add(VerseSymbol[150]);
					listViewItem.SubItems.Add("");
					for (int k = InItem2.VerseLineLoc[num, 1]; (k >= 0) & (k <= InItem2.VerseLineLoc[num, 2]); k++)
					{
						listViewItem = TempItem1.LyricsAndNotationsList.Items.Add(InItem2.LyricsAndNotationsList.Items[k].SubItems[2].Text);
						listViewItem.SubItems.Add(InItem2.LyricsAndNotationsList.Items[k].SubItems[3].Text);
					}
					InItem2.VersePresent[num] = false;
				}
			}
			for (int i = 0; i <= 112; i++)
			{
				if (InItem2.VersePresent[i] & (InItem2.CompleteLyrics != ""))
				{
					int num = i;
					listViewItem = TempItem1.LyricsAndNotationsList.Items.Add(VerseSymbol[num]);
					listViewItem.SubItems.Add("");
					listViewItem = TempItem1.LyricsAndNotationsList.Items.Add(VerseSymbol[150]);
					listViewItem.SubItems.Add("");
					for (int k = InItem2.VerseLineLoc[num, 1]; (k >= 0) & (k <= InItem2.VerseLineLoc[num, 2]); k++)
					{
						listViewItem = TempItem1.LyricsAndNotationsList.Items.Add(InItem2.LyricsAndNotationsList.Items[k].SubItems[2].Text);
						listViewItem.SubItems.Add(InItem2.LyricsAndNotationsList.Items[k].SubItems[3].Text);
					}
				}
			}
			if (!flag && TempItem1.LyricsAndNotationsList.Items.Count > 0)
			{
				TempItem1.LyricsAndNotationsList.Items[0].Remove();
			}
			CombinedLyrics = "";
			for (int i = 0; i < TempItem1.LyricsAndNotationsList.Items.Count; i++)
			{
				CombinedLyrics = CombinedLyrics + "\n" + TempItem1.LyricsAndNotationsList.Items[i].SubItems[0].Text;
			}
			CombinedLyrics = DataUtil.Mid(CombinedLyrics, 1);
			CombinedNotations = "";
			for (int i = 0; i < TempItem1.LyricsAndNotationsList.Items.Count; i++)
			{
				if (TempItem1.LyricsAndNotationsList.Items[i].SubItems[1].Text != "")
				{
					object obj = CombinedNotations;
					CombinedNotations = string.Concat(obj, "(", Convert.ToString(i), ';', TempItem1.LyricsAndNotationsList.Items[i].SubItems[1].Text, ")");
				}
			}
			CombinedLyrics = DataUtil.TrimEnd(CombinedLyrics);
		}

		public static bool UpdateRefString(string Instring, string InStringDelim, ref TextBox StoredTextBox, string StoredStringDelim)
		{
			string text = "";
			bool flag = false;
			bool flag2 = false;
			string text2 = "";
			string text3 = "";
			string[] array = Instring.Split(InStringDelim[0]);
			string[] array2 = StoredTextBox.Text.Split(StoredStringDelim[0]);
			for (int i = 0; i <= array.GetUpperBound(0); i++)
			{
				text2 = DataUtil.Trim(array[i]).ToLower();
				flag = false;
				if (!(text2 != ""))
				{
					continue;
				}
				for (int j = 0; j <= array2.GetUpperBound(0); j++)
				{
					text3 = DataUtil.Trim(array2[j]).ToLower();
					if (text2 == text3)
					{
						flag = true;
						j = array2.GetUpperBound(0) + 1;
					}
				}
				if (!flag)
				{
					text = text + ((text != "") ? (StoredStringDelim + " ") : "") + DataUtil.Trim(array[i]);
				}
			}
			if (text != "")
			{
				TextBox obj = StoredTextBox;
				obj.Text = obj.Text + ((StoredTextBox.Text != "") ? (StoredStringDelim + " ") : "") + text;
				return true;
			}
			return false;
		}

		public static ListView ExtractLyrics(string InLyrics, string InNotations)
		{
			string OutText = "";
			string OutNotationString = "";
			string OutText2 = "";
			string OutNotationString2 = "";
			return ExtractLyrics(InLyrics, InNotations, ref OutText, ref OutNotationString, ref OutText2, ref OutNotationString2);
		}

		public static ListView ExtractLyrics(string InLyrics, string InNotations, ref string OutText1, ref string OutNotationString1, ref string OutText2, ref string OutNotationString2)
		{
			InLyrics = InLyrics.Replace("\r\n", "\n");
			if (IsNewR2Format(InLyrics))
			{
				return ExtractNewFormatLyrics(InLyrics, InNotations, ref OutText1, ref OutNotationString1, ref OutText2, ref OutNotationString2);
			}
			return ExtractDefaultFormatLyrics(InLyrics, InNotations, ref OutText1, ref OutNotationString1, ref OutText2, ref OutNotationString2);
		}

		public static ListView ExtractNewFormatLyrics(string InLyrics, string InNotations, ref string OutText1, ref string OutNotationString1, ref string OutText2, ref string OutNotationString2)
		{
			string text = "";
			StringBuilder stringBuilder = new StringBuilder();
			StringBuilder stringBuilder2 = new StringBuilder();
			StringBuilder stringBuilder3 = new StringBuilder();
			StringBuilder stringBuilder4 = new StringBuilder();
			string ResultString = "";
			string text2 = "";
			int num = -1;
			int num2 = -1;
			int num3 = -1;
			string text3 = "";
			int num4 = -1;
			int num5 = 1;
			bool flag = false;
			string[] array = InLyrics.Split("\n"[0]);
			ListView listView = new ListView();
			SetListViewColumns(listView, 4);
			listView.Items.Clear();
			ListViewItem listViewItem = new ListViewItem();
			for (int i = 0; i <= array.GetUpperBound(0); i++)
			{
				for (int j = 0; j <= xArray.GetUpperBound(0); j++)
				{
					if (array[i] == xArray[j])
					{
						if (array[i] != VerseSymbol[150])
						{
							text3 = array[i];
							num4 = GetVerseNumeric(text3);
							flag = true;
							j = xArray.GetUpperBound(0) + 1;
						}
						else
						{
							num5 = 2;
						}
					}
				}
				if ((num < i) & (InNotations.Length > 0))
				{
					num = ExtractOneNotationsLine(ref InNotations, ref ResultString);
				}
				if (num5 == 1)
				{
					num2++;
					stringBuilder.Append(array[i] + "\n");
					if (i == num)
					{
						text2 = AddNotationLineNumber(ResultString, num2);
						stringBuilder3.Append(text2);
						text2 = ResultString;
					}
				}
				else if (array[i] == VerseSymbol[150])
				{
					flag = true;
				}
				else
				{
					num3++;
					stringBuilder2.Append(array[i] + "\n");
					if (i == num)
					{
						text2 = AddNotationLineNumber(ResultString, num3);
						stringBuilder4.Append(text2);
						text2 = ResultString;
					}
				}
				if (!flag)
				{
					if (num4 < 0)
					{
						num4 = 1;
					}
					listViewItem = listView.Items.Add(num4.ToString());
					listViewItem.SubItems.Add(num5.ToString());
					listViewItem.SubItems.Add(array[i]);
					listViewItem.SubItems.Add(text2);
				}
				flag = false;
				text2 = "";
			}
			OutText1 = DataUtil.TrimEnd(stringBuilder.ToString());
			OutText2 = DataUtil.TrimEnd(stringBuilder2.ToString());
			OutNotationString1 = stringBuilder3.ToString();
			OutNotationString2 = stringBuilder4.ToString();
			return listView;
		}

		public static ListView ExtractDefaultFormatLyrics(string InLyrics, string InNotations, ref string OutText1, ref string OutNotationString1, ref string OutText2, ref string OutNotationString2)
		{
			string text = "";
			StringBuilder stringBuilder = new StringBuilder();
			StringBuilder stringBuilder2 = new StringBuilder();
			StringBuilder stringBuilder3 = new StringBuilder();
			StringBuilder stringBuilder4 = new StringBuilder();
			string ResultString = "";
			string text2 = "";
			int num = -1;
			int num2 = -1;
			int num3 = -1;
			string text3 = "";
			int num4 = -1;
			int num5 = 1;
			bool flag = false;
			string[] array = InLyrics.Split("\n"[0]);
			ListView listView = new ListView();
			SetListViewColumns(listView, 4);
			listView.Items.Clear();
			ListViewItem listViewItem = new ListViewItem();
			for (int i = 0; i <= array.GetUpperBound(0); i++)
			{
				for (int j = 0; j <= xArray.GetUpperBound(0); j++)
				{
					if (array[i] == xArray[j])
					{
						flag = true;
						if (array[i] != VerseSymbol[150])
						{
							num5 = 1;
							text3 = array[i];
							num4 = GetVerseNumeric(text3);
							j = xArray.GetUpperBound(0) + 1;
						}
						else
						{
							num5 = 2;
						}
					}
				}
				if ((num < i) & (InNotations.Length > 0))
				{
					num = ExtractOneNotationsLine(ref InNotations, ref ResultString);
				}
				if (num5 == 1)
				{
					num2++;
					stringBuilder.Append(array[i] + "\n");
					if (i == num)
					{
						text2 = AddNotationLineNumber(ResultString, num2);
						stringBuilder3.Append(text2);
						text2 = ResultString;
					}
				}
				else if (array[i] == VerseSymbol[150])
				{
					if (text3 != "")
					{
						num3++;
						stringBuilder2.Append(text3 + "\n");
					}
				}
				else
				{
					num3++;
					stringBuilder2.Append(array[i] + "\n");
					if (i == num)
					{
						text2 = AddNotationLineNumber(ResultString, num3);
						stringBuilder4.Append(text2);
						text2 = ResultString;
					}
				}
				if (!flag)
				{
					if (num4 < 0)
					{
						num4 = 1;
					}
					listViewItem = listView.Items.Add(num4.ToString());
					listViewItem.SubItems.Add(num5.ToString());
					listViewItem.SubItems.Add(array[i]);
					listViewItem.SubItems.Add(text2);
				}
				flag = false;
				text2 = "";
			}
			OutText1 = DataUtil.TrimEnd(stringBuilder.ToString());
			OutText2 = DataUtil.TrimEnd(stringBuilder2.ToString());
			OutNotationString1 = stringBuilder3.ToString();
			OutNotationString2 = stringBuilder4.ToString();
			return listView;
		}

		private static int GetVerseNumeric(string CurVerseSymbol)
		{
			for (int i = 0; i < 160; i++)
			{
				if (CurVerseSymbol == VerseSymbol[i])
				{
					return i;
				}
			}
			return -1;
		}

		public static string AddNotationLineNumber(string InOneNotationLine, int InNewLineNumber)
		{
			if (InOneNotationLine != "")
			{
				return "(" + InNewLineNumber + ';' + InOneNotationLine + ")";
			}
			return "";
		}

		public static void RemoveInvalidDirNameChars(ref string InString)
		{
			InString = InString.Replace("\\", "_");
			InString = InString.Replace("/", "_");
			InString = InString.Replace(":", "_");
			InString = InString.Replace("*", "_");
			InString = InString.Replace("?", "_");
			InString = InString.Replace("\"", "_");
			InString = InString.Replace("<", "_");
			InString = InString.Replace(">", "_");
			InString = InString.Replace("|", "_");
		}

		public static string RTFCheck(string InString)
		{
			InString = InString.Replace("{", "\\{");
			return InString.Replace("}", "\\}");
		}

		public static string Html_MusicDisplayName(string title1, string title2, string HTMLIndexDir)
		{
			string DirPath = "";
			string FileName = "";
			if (GetMusicFileName(title1, title2, ref DirPath, ref FileName, StoreDirPath: true) == "")
			{
				return "";
			}
			int num = 0;
			if (DirPath == HTMLIndexDir)
			{
				return FileName;
			}
			for (int i = 0; i < HTMLIndexDir.Length; i++)
			{
				if (DataUtil.Mid(HTMLIndexDir, i, 1) != DataUtil.Mid(DirPath, i, 1))
				{
					num = i - 1;
					i = HTMLIndexDir.Length;
				}
			}
			if (num < 1)
			{
				return DirPath + FileName;
			}
			string str = "";
			for (int i = num + 1; i <= HTMLIndexDir.Length; i++)
			{
				if (DataUtil.Mid(HTMLIndexDir, i, 1) == "\\")
				{
					str += "..\\";
				}
			}
			str += DataUtil.Mid(DirPath, num + 1);
			return str + FileName;
		}

		public static string GetMusicFileName(string MusicTitle1, string MusicTitle2)
		{
			string DirPath = "";
			string FileName = "";
			return GetMusicFileName(MusicTitle1, MusicTitle2, ref DirPath, ref FileName, StoreDirPath: false);
		}

		public static string GetMusicFileName(string MusicTitle1, string MusicTitle2, ref string DirPath)
		{
			string FileName = "";
			return GetMusicFileName(MusicTitle1, MusicTitle2, ref DirPath, ref FileName, StoreDirPath: false);
		}

		public static string GetMusicFileName(string MusicTitle1, string MusicTitle2, ref string DirPath, ref string FileName)
		{
			return GetMusicFileName(MusicTitle1, MusicTitle2, ref DirPath, ref FileName, StoreDirPath: false);
		}

		public static string GetMusicFileName(string MusicTitle1, string MusicTitle2, ref string DirPath, ref string FileName, bool StoreDirPath)
		{
			if (StoreDirPath & (TotalMusicFiles < 1))
			{
				return "";
			}
			string text = "";
			for (int i = 0; i <= 1; i++)
			{
				text = ((i == 0) ? MusicTitle1 : MusicTitle2);
				for (int j = 0; j <= TotalMediaFileExt - 1; j++)
				{
					if (StoreDirPath)
					{
						for (int k = 0; k <= TotalMusicFiles - 1; k++)
						{
							if (MediaFilesList[k, 0] == text && MediaFilesList[k, 1] == MediaFileExtension[j, 0])
							{
								DirPath = MediaFilesList[k, 2];
								FileName = MediaFilesList[k, 0] + MediaFilesList[k, 1];
								return DirPath + FileName;
							}
						}
					}
					else
					{
						string musicFileNameFromDir = GetMusicFileNameFromDir(MediaDir, MediaFileExtension[j, 0], text, ref DirPath, ref FileName);
						if (musicFileNameFromDir != "")
						{
							return musicFileNameFromDir;
						}
					}
				}
			}
			return "";
		}

		public static string GetMusicFileNameFromDir(string FolderPath, string MusicExtension, string MusicTitle1)
		{
			string DirPath = "";
			string FileName = "";
			return GetMusicFileNameFromDir(FolderPath, MusicExtension, MusicTitle1, ref DirPath, ref FileName);
		}

		public static string GetMusicFileNameFromDir(string FolderPath, string MusicExtension, string MusicTitle1, ref string DirPath)
		{
			string FileName = "";
			return GetMusicFileNameFromDir(FolderPath, MusicExtension, MusicTitle1, ref DirPath, ref FileName);
		}

		public static string GetMusicFileNameFromDir(string FolderPath, string MusicExtension, string MusicTitle1, ref string DirPath, ref string FileName)
		{
			if ((FolderPath == "") | !Directory.Exists(FolderPath) | (DataUtil.Mid(FolderPath, 2) == ":\\System Volume Information\\"))
			{
				return "";
			}
			if (File.Exists(FolderPath + MusicTitle1 + MusicExtension))
			{
				DirPath = FolderPath;
				FileName = MusicTitle1 + MusicExtension;
				return DirPath + FileName;
			}
			string[] directories = Directory.GetDirectories(FolderPath);
			if (directories.Length > 0)
			{
				SingleArraySort(directories, SortAscending: true);
			}
			string[] array = directories;
			foreach (string str in array)
			{
				string musicFileNameFromDir = GetMusicFileNameFromDir(str + "\\", MusicExtension, MusicTitle1, ref DirPath, ref FileName);
				if (musicFileNameFromDir != "")
				{
					return musicFileNameFromDir;
				}
			}
			return "";
		}

		public static void AlertSettings(AlertType InAlertType)
		{
			ParentalAlertLive = false;
			MessageAlertLive = false;
			AlertTimeRemaining = 0;
			Alert_FormattedMessage = "";
			Alert_MessageHeight = 0;
			Alert_MessageDisplayed = false;
			switch (InAlertType)
			{
				case AlertType.Parental:
					Alert_OriginalMessage = DataUtil.Trim(ParentalAlertHeading) + " " + ParentalAlertDetails;
					Alert_TextAlign = ParentalAlertTextAlign;
					Alert_VerticalAlign = ParentalAlertVerticalAlign;
					Alert_TextColour = ParentalAlertTextColour;
					Alert_BackColour = ParentalAlertBackColour;
					Alert_Flash = ParentalAlertFlash;
					Alert_Scroll = ParentalAlertScroll;
					Alert_Transparent = ParentalAlertTransparent;
					AlertTimeRemaining = ParentalAlertDuration;
					Alert_UserFont = GetNewFont(ParentalAlertFontName, ParentalAlertFontSize, ParentalAlertBold, ParentalAlertItalic, ParentalAlertUnderline, ShowErrorMsg: false);
					Alert_UserFontShadow = ParentalAlertShadow;
					Alert_UserFontOutline = ParentalAlertOutline;
					BuildAlertSequence();
					Alert_NewMessage = true;
					ParentalAlertLive = true;
					break;
				case AlertType.Message:
					Alert_OriginalMessage = MessageAlertDetails;
					Alert_TextAlign = MessageAlertTextAlign;
					Alert_VerticalAlign = MessageAlertVerticalAlign;
					Alert_TextColour = MessageAlertTextColour;
					Alert_BackColour = MessageAlertBackColour;
					Alert_Flash = MessageAlertFlash;
					Alert_Scroll = MessageAlertScroll;
					Alert_Transparent = MessageAlertTransparent;
					AlertTimeRemaining = MessageAlertDuration;
					Alert_UserFont = GetNewFont(MessageAlertFontName, MessageAlertFontSize, MessageAlertBold, MessageAlertItalic, MessageAlertUnderline, ShowErrorMsg: false);
					Alert_UserFontShadow = MessageAlertShadow;
					Alert_UserFontOutline = MessageAlertOutline;
					BuildAlertSequence();
					Alert_NewMessage = true;
					MessageAlertLive = true;
					break;
			}
		}

		public static void BuildAlertSequence()
		{
			string text = "";
			if (Alert_Flash)
			{
				text = "R" + '>' + "R" + '>' + "R" + '>' + "R" + '>';
			}
			Alert_FormatSequence = text;
			bool flag = Alert_Scroll;
			if ((Alert_OriginalMessage.Length + 30) * AlertGap > AlertTimeRemaining)
			{
				flag = false;
			}
			if (flag)
			{
				for (int i = 0; i < Alert_OriginalMessage.Length; i++)
				{
					Alert_FormatSequence = Alert_FormatSequence + "S" + '>';
				}
				Alert_FormattedMessage = "";
				Alert_MessageLength = 0;
			}
			else if (!flag)
			{
				for (int i = 0; i < Alert_OriginalMessage.Length; i++)
				{
					Alert_FormatSequence = Alert_FormatSequence + "A" + '>';
				}
				Alert_FormattedMessage = "";
				Alert_MessageLength = Alert_OriginalMessage.Length;
			}
			Alert_FormatSequence += text;
			for (int i = 1; i <= 30; i++)
			{
				Alert_FormatSequence = Alert_FormatSequence + "A" + '>';
			}
			if (Alert_Scroll)
			{
				Alert_FormatSequence = Alert_FormatSequence + "C" + '>';
			}
			Alert_FormatOriginalSequence = Alert_FormatSequence;
		}

		public static void OldBuildAlertSequence()
		{
			string text = "";
			if (Alert_Flash)
			{
				text = "R" + '>' + "R" + '>' + "R" + '>' + "R" + '>';
			}
			Alert_FormatSequence = text;
			bool flag = Alert_Scroll;
			if ((Alert_OriginalMessage.Length + 30) * AlertGap > AlertTimeRemaining)
			{
				flag = false;
			}
			if (flag)
			{
				for (int i = 0; i < Alert_OriginalMessage.Length; i++)
				{
					Alert_FormatSequence = Alert_FormatSequence + "S" + '>';
				}
				Alert_FormattedMessage = "";
				Alert_MessageLength = 0;
			}
			else if (!flag)
			{
				for (int i = 0; i < Alert_OriginalMessage.Length; i++)
				{
					Alert_FormatSequence = Alert_FormatSequence + "A" + '>';
				}
				Alert_FormattedMessage = "";
				Alert_MessageLength = Alert_OriginalMessage.Length;
			}
			Alert_FormatSequence += text;
			for (int i = 1; i <= 30; i++)
			{
				Alert_FormatSequence = Alert_FormatSequence + "A" + '>';
			}
			if (Alert_Scroll)
			{
				Alert_FormatSequence = Alert_FormatSequence + "C" + '>';
			}
			Alert_FormatOriginalSequence = Alert_FormatSequence;
		}

		public static void LoadComboBoxFromTextFile(ref ComboBox InComboBox, string InTextFile)
		{
			InComboBox.Items.Clear();
			string InString = "";
			if (LoadFileContents(InTextFile, ref InString))
			{
				InString = InString.Replace("\r\n", "\n");
				try
				{
					string[] array = InString.Split("\n"[0]);
					for (int i = 0; i <= array.GetUpperBound(0) && i < 20; i++)
					{
						if (DataUtil.Trim(array[i]) != "")
						{
							InComboBox.Items.Add(DataUtil.Trim(array[i]));
						}
					}
				}
				catch
				{
				}
			}
			InComboBox.Text = "";
		}

		public static void SaveComboBoxToTextFile(ref ComboBox InComboBox, string InTextFile)
		{
			string text = "";
			for (int i = 0; i < InComboBox.Items.Count && i < 20; i++)
			{
				if (DataUtil.Trim(InComboBox.Items[i].ToString()) != "")
				{
					text = text + DataUtil.TrimEnd(DataUtil.Trim(InComboBox.Items[i].ToString())) + "\r\n";
				}
			}
			FileUtil.CreateNewFile(InTextFile, FileUtil.FileContentsType.DoubleByte, text);
		}

		public static void LoadListViewFromTextFile(ref ListView InListView, string InTextFile)
		{
			InListView.Items.Clear();
			string InString = "";
			if (LoadFileContents(InTextFile, ref InString))
			{
				InString = InString.Replace("\r\n", "\n");
				try
				{
					string[] array = InString.Split("\n"[0]);
					for (int i = 0; i <= array.GetUpperBound(0); i++)
					{
						if (DataUtil.Trim(array[i]) != "")
						{
							InListView.Items.Add(DataUtil.Trim(array[i]));
						}
					}
				}
				catch
				{
				}
			}
			InListView.Text = "";
		}

		public static void SaveListViewToTextFile(ref ListView InListView, string InTextFile)
		{
			string text = "";
			for (int i = 0; i < InListView.Items.Count; i++)
			{
				if (DataUtil.Trim(InListView.Items[i].Text) != "")
				{
					text = text + DataUtil.TrimEnd(DataUtil.Trim(InListView.Items[i].Text)) + "\r\n";
				}
			}
			FileUtil.CreateNewFile(InTextFile, FileUtil.FileContentsType.DoubleByte, text);
		}

		public static void MapLyricsBreak(ref ListView InScreenBreak, ref RichTextBox InLyrics, ref bool ScreenBreakListDone)
		{
			int num = 0;
			int num2 = -1;
			int num3 = -1;
			int num4 = 0;
			string text = "";
			string text2 = "";
			bool flag = false;
			InLyrics.Text.Replace("\r\n", "\n");
			InLyrics.Text = DataUtil.TrimStart(InLyrics.Text);
			InScreenBreak.Items.Clear();
			InLyrics.Focus();
			while (num >= 0)
			{
				num2 = InLyrics.Text.IndexOf("[", (num < InLyrics.Text.Length) ? num : InLyrics.Text.Length);
				num3 = InLyrics.Text.IndexOf("\n\n", (num < InLyrics.Text.Length) ? num : InLyrics.Text.Length);
				if (!flag && num2 != 0 && num3 != 0)
				{
					num2 = -1;
					num3 = 0;
				}
				if (num2 >= 0 && num3 >= 0)
				{
					if (num2 < num3)
					{
						text = ValidateVerseIndicator(InLyrics.Text, num2);
						if (text != "")
						{
							if (text2 != text)
							{
								num4 = 0;
							}
							text2 = text;
							AddItemToScreenBreak(ref InScreenBreak, num2, text2, num4);
							num4++;
							num = text2.Length + num2;
						}
					}
					else
					{
						AddItemToScreenBreak(ref InScreenBreak, num3 + (flag ? 2 : 0), text2, num4);
						num4++;
						num = 2 + num3;
					}
				}
				else if (num2 >= 0)
				{
					text = ValidateVerseIndicator(InLyrics.Text, num2);
					if (text != "")
					{
						if (text2 != text)
						{
							num4 = 0;
						}
						text2 = text;
						AddItemToScreenBreak(ref InScreenBreak, num2, text2, num4);
						num4++;
						num = text2.Length + num2;
					}
				}
				else if (num3 >= 0)
				{
					AddItemToScreenBreak(ref InScreenBreak, num3 + (flag ? 2 : 0), text2, num4);
					num4++;
					num = 2 + num3;
				}
				else
				{
					num = -1;
				}
				flag = true;
			}
			ScreenBreakListDone = true;
		}

		public static string ValidateVerseIndicator(string InText, int StartPosition)
		{
			for (int i = 0; i <= 99; i++)
			{
				if (DataUtil.Mid(InText, StartPosition, VerseSymbol[i].Length) == VerseSymbol[i])
				{
					return VerseSymbol[i];
				}
			}
			for (int i = 100; i <= 112; i++)
			{
				if (DataUtil.Mid(InText, StartPosition, VerseSymbol[i].Length) == VerseSymbol[i])
				{
					return VerseSymbol[i];
				}
			}
			return "";
		}

		public static void AddItemToScreenBreak(ref ListView InListView, int NewPosition, string VerseSym, int ScreenBreakCount)
		{
			ListViewItem listViewItem = new ListViewItem();
			listViewItem = InListView.Items.Add(NewPosition.ToString());
			listViewItem.SubItems.Add(VerseSym);
			listViewItem.SubItems.Add(ScreenBreakCount.ToString());
		}

		public static void GetBreakPosition(ListView InListView, int CurPosition, int Direction, ref int NewPosition, ref int NewPositionLength, ref string LookupVerseSym, ref int LookupScreenCount)
		{
			if (InListView.Items.Count == 0)
			{
				LookupVerseSym = "";
				NewPosition = 0;
				NewPositionLength = -1;
				LookupScreenCount = 0;
				return;
			}
			int num = -1;
			if (Direction == 0)
			{
				int num2 = InListView.Items.Count - 1;
				while (true)
				{
					if (num2 >= 0)
					{
						if (DataUtil.StringToInt(InListView.Items[num2].SubItems[0].Text) < CurPosition)
						{
							num = num2;
							break;
						}
						num2--;
						continue;
					}
					num = 0;
					break;
				}
			}
			else
			{
				int num2 = 0;
				while (true)
				{
					if (num2 < InListView.Items.Count)
					{
						if (DataUtil.StringToInt(InListView.Items[num2].SubItems[0].Text) > CurPosition)
						{
							if (NewPositionLength == 0 && num2 > 0)
							{
								num2--;
							}
							num = num2;
							break;
						}
						num2++;
						continue;
					}
					num = InListView.Items.Count - 1;
					break;
				}
			}
			NewPosition = DataUtil.StringToInt(InListView.Items[num].SubItems[0].Text);
			LookupVerseSym = InListView.Items[num].SubItems[1].Text;
			LookupScreenCount = DataUtil.StringToInt(InListView.Items[num].SubItems[2].Text);
			if (num < InListView.Items.Count - 1)
			{
				int num3 = DataUtil.StringToInt(InListView.Items[num + 1].SubItems[0].Text);
				NewPositionLength = num3 - NewPosition;
			}
			else
			{
				NewPositionLength = -1;
			}
		}

		public static void GetBreakPosition(ListView InListView, ref int NewPosition, ref int NewPositionLength, string LookupVerseSym, int LookupScreenCount)
		{
			if (InListView.Items.Count == 0)
			{
				LookupVerseSym = "";
				NewPosition = 0;
				NewPositionLength = 0;
				LookupScreenCount = 0;
				return;
			}
			int num = -1;
			int num2 = 0;
			while (true)
			{
				if (num2 < InListView.Items.Count)
				{
					if (InListView.Items[num2].SubItems[1].Text == LookupVerseSym && DataUtil.StringToInt(InListView.Items[num2].SubItems[2].Text) == LookupScreenCount)
					{
						num = num2;
						break;
					}
					num2++;
					continue;
				}
				if (num >= 0)
				{
					break;
				}
				LookupVerseSym = "";
				NewPosition = 0;
				NewPositionLength = 0;
				LookupScreenCount = 0;
				return;
			}
			NewPosition = DataUtil.StringToInt(InListView.Items[num].SubItems[0].Text);
			if (num < InListView.Items.Count - 1)
			{
				int num3 = DataUtil.StringToInt(InListView.Items[num + 1].SubItems[0].Text);
				NewPositionLength = num3 - NewPosition;
			}
			else
			{
				NewPositionLength = -1;
			}
		}

		public static bool RecycleBin(string FullFileName)
		{
			try
			{
				SHFILEOPSTRUCT lpFileOp = default(SHFILEOPSTRUCT);
				lpFileOp.hwnd = IntPtr.Zero;
				lpFileOp.wFunc = 3u;
				lpFileOp.fFlags = 80;
				lpFileOp.pFrom = FullFileName + '\0' + '\0';
				lpFileOp.fAnyOperationsAborted = 0;
				lpFileOp.hNameMappings = IntPtr.Zero;
				SHFileOperation(ref lpFileOp);
				return !File.Exists(FullFileName);
			}
			catch
			{
				return false;
			}
		}

		public static string CopyExternalFile(string SourceFileName, string CopyToFolder)
		{
			if (File.Exists(SourceFileName))
			{
				int num = 0;
				string extension = Path.GetExtension(SourceFileName);
				string displayNameOnly = GetDisplayNameOnly(ref SourceFileName, UpdateByRef: false);
				string text = Path.GetDirectoryName(SourceFileName) + "\\";
				if (!Directory.Exists(CopyToFolder) && !FileUtil.MakeDir(CopyToFolder))
				{
					return "";
				}
				string text2 = CopyToFolder + displayNameOnly + extension;
				while (File.Exists(text2))
				{
					num++;
					text2 = CopyToFolder + displayNameOnly + " - Copy (" + num + ")" + extension;
				}
				try
				{
					File.Copy(SourceFileName, text2);
					return text2;
				}
				catch
				{
				}
			}
			return "";
		}

		public static string MoveExternalFile(string SourceFileName, string MoveToFolder)
		{
			if (File.Exists(SourceFileName))
			{
				int num = 0;
				string extension = Path.GetExtension(SourceFileName);
				string displayNameOnly = GetDisplayNameOnly(ref SourceFileName, UpdateByRef: false);
				string text = Path.GetDirectoryName(SourceFileName) + "\\";
				if (!Directory.Exists(MoveToFolder) && !FileUtil.MakeDir(MoveToFolder))
				{
					return "";
				}
				string text2 = MoveToFolder + displayNameOnly + extension;
				while (File.Exists(text2))
				{
					num++;
					text2 = MoveToFolder + displayNameOnly + " - Copy (" + num + ")" + extension;
				}
				try
				{
					File.Move(SourceFileName, text2);
					return text2;
				}
				catch
				{
				}
			}
			return "";
		}

		public static void ScanSelectedRTB(ref RichTextBox InTextBox, bool[] VersePresent, bool DoAll, int StartPos, int EndPos, string[] InsArray, Font MainFont, Font NotationFont, bool DoNotations)
		{
			if (InTextBox.Text == "")
			{
				return;
			}
			int selectionStart = EndPos;
			if (DoAll)
			{
				StartPos = 0;
				EndPos = InTextBox.Text.Length - 1;
			}
			else
			{
				if (StartPos > EndPos)
				{
					int num = EndPos;
					StartPos = EndPos;
					EndPos = StartPos;
				}
				MoveToPosInLine(InTextBox.Text, ref StartPos, 0);
				MoveToPosInLine(InTextBox.Text, ref EndPos, 1);
			}
			int num2 = 0;
			MarkSelectedRTB(ref InTextBox, StartPos, EndPos - StartPos + 1, 0, MainFont, NotationFont);
			for (num2 = 0; num2 <= InsArray.GetUpperBound(0); num2++)
			{
				if (StartPos < 0)
				{
					StartPos = 0;
				}
				if (InsArray[num2] == VerseSymbol[150])
				{
					int num3;
					try
					{
						num3 = InTextBox.Text.IndexOf(InsArray[num2], StartPos);
					}
					catch
					{
						num3 = -1;
					}
					while (num3 >= 0)
					{
						MarkSelectedRTB(ref InTextBox, num3, InsArray[num2].Length, 1, MainFont, NotationFont);
						num3 = InTextBox.Text.IndexOf(InsArray[num2], num3 + 1);
					}
				}
				else
				{
					int num3;
					try
					{
						num3 = InTextBox.Text.IndexOf(InsArray[num2], StartPos);
					}
					catch
					{
						num3 = -1;
					}
					if (num3 >= 0 && num3 <= EndPos)
					{
						MarkSelectedRTB(ref InTextBox, num3, InsArray[num2].Length, 1, MainFont, NotationFont);
					}
				}
			}
			if (DoNotations)
			{
				int num3;
				try
				{
					num3 = InTextBox.Text.IndexOf("»", StartPos);
				}
				catch
				{
					num3 = -1;
				}
				while (num3 >= 0 && num3 <= EndPos)
				{
					int CurPos = num3;
					int CurPos2 = num3;
					MoveToPosInLine(InTextBox.Text, ref CurPos, 0);
					MoveToPosInLine(InTextBox.Text, ref CurPos2, 1);
					MarkSelectedRTB(ref InTextBox, CurPos, CurPos2 - CurPos + 1, 2, MainFont, NotationFont);
					num3 = InTextBox.Text.IndexOf("»", num3 + 1);
				}
			}
			InTextBox.SelectionStart = selectionStart;
		}

		public static void MarkSelectedRTB(ref RichTextBox InTextBox, int SelStartPos, int SelLen, int InMode, Font MainFont, Font NotationFont)
		{
			SendMessage(InTextBox.Handle, 11u, 0u, 0u);
			InTextBox.SelectionStart = SelStartPos;
			InTextBox.SelectionLength = SelLen;
			switch (InMode)
			{
				case 0:
					InTextBox.SelectionCharOffset = 0;
					InTextBox.SelectionFont = MainFont;
					InTextBox.SelectionColor = NormalTextColour;
					break;
				case 1:
					InTextBox.SelectionCharOffset = 0;
					InTextBox.SelectionFont = MainFont;
					InTextBox.SelectionColor = SelectedTextColour;
					break;
				case 2:
					InTextBox.SelectionCharOffset = 0;
					InTextBox.SelectionFont = NotationFont;
					InTextBox.SelectionColor = NotationColour;
					break;
			}
			InTextBox.SelectionStart = SelStartPos + SelLen;
			InTextBox.SelectionLength = 0;
			InTextBox.SelectionColor = NormalTextColour;
			SendMessage(InTextBox.Handle, 11u, 1u, 0u);
			InTextBox.Refresh();
		}

		public static bool SaveXMLInfoScreen(SongSettings InItem, string InFileName, string[] InHeaderData, bool ReloadImageData, bool UseOriginalNotations)
		{
            XmlTextWriter xtw = null;

            try
			{
				xtw = new XmlTextWriter(InFileName, Encoding.UTF8);
				xtw.Formatting = Formatting.Indented;
				xtw.WriteStartDocument();
				xtw.WriteStartElement("EasiSlides");
				WriteXMLOneItem(ref xtw, InItem, InHeaderData, ReloadImageData, UseOriginalNotations);
				xtw.WriteEndDocument();
				xtw.Flush();
				xtw.Dispose();
				return true;
			}
			catch
			{
				if(xtw!= null)	
					xtw.Dispose();
				return false;
			}
		}

		public static void WriteXMLOneItem(ref XmlTextWriter xtw, SongSettings InItem, string[] InHeaderData, bool ReloadImageData)
		{
			WriteXMLOneItem(ref xtw, InItem, InHeaderData, ReloadImageData, UseOriginalNotations: true);
		}

		public static void WriteXMLOneItem(ref XmlTextWriter xtw, SongSettings InItem, string[] InHeaderData, bool ReloadImageData, bool UseOriginalNotations)
		{
			string text = InItem.Format.FormatString;
			if (InHeaderData != null)
			{
				for (int i = 2; i <= 254; i++)
				{
					if (InHeaderData[i] != "" && InHeaderData[i] != null)
					{
						object obj = text;
						text = string.Concat(obj, (char)i, "=", InHeaderData[i], '>'.ToString());
					}
				}
			}
			try
			{
				xtw.WriteStartElement("Item");
				xtw.WriteElementString("Title1", InItem.Title);
				xtw.WriteElementString("Title2", InItem.Title2);
				xtw.WriteElementString("Folder", FolderName[InItem.FolderNo]);
				xtw.WriteElementString("SongNumber", InItem.SongNumber.ToString());
				InItem.CompleteLyrics = InItem.CompleteLyrics.Replace("\r\n", "\n");
				InItem.CompleteLyrics = InItem.CompleteLyrics.Replace("\n", "\r\n");
				xtw.WriteElementString("Contents", InItem.CompleteLyrics);
				xtw.WriteElementString("Notations", UseOriginalNotations ? InItem.OriginalNotations : InItem.Notations);
				xtw.WriteElementString("Sequence", ConvertSequenceToTextString(InItem.SongSequence));
				xtw.WriteElementString("Writer", InItem.Writer);
				xtw.WriteElementString("Copyright", InItem.Copyright);
				xtw.WriteElementString("Category", InItem.Category);
				xtw.WriteElementString("Timing", InItem.Timing);
				xtw.WriteElementString("MusicKey", InItem.MusicKey);
				xtw.WriteElementString("Capo", InItem.Capo.ToString());
				xtw.WriteElementString("LicenceAdmin1", InItem.Show_LicAdminInfo1);
				xtw.WriteElementString("LicenceAdmin2", InItem.Show_LicAdminInfo2);
				xtw.WriteElementString("BookReference", InItem.Book_Reference);
				xtw.WriteElementString("UserReference", InItem.User_Reference);
				xtw.WriteElementString("FormatData", text);
				xtw.WriteElementString("Settings", InItem.Settings);
				if (ReloadImageData && InItem.Format.BackgroundPicture != null && InItem.Format.BackgroundPicture != "")
				{
					Base64EncodeImageFile(ref xtw, "Image", InItem.Format.BackgroundPicture);
				}
				else if (InItem.Format.ImageString != "")
				{
					xtw.WriteElementString("Image", InItem.Format.ImageString);
				}
				xtw.WriteEndElement();
			}
			catch
			{
			}
		}

		public static void Base64EncodeImageFile(ref XmlTextWriter xtw, string InElementString, string InFileName)
		{
			FileInfo fileInfo = new FileInfo(InFileName);
			using FileStream fileStream = fileInfo.OpenRead();
			byte[] array = new byte[fileInfo.Length];
			fileStream.Read(array, 0, array.Length);
			//fileStream.Close();
			xtw.WriteStartElement(InElementString);
			xtw.WriteBase64(array, 0, array.Length);
		}

		public static string ConvertSequenceToTextString(string InSequence)
		{
			if (InSequence.Length > 0)
			{
				string text = "";
				for (int i = 0; i < InSequence.Length; i++)
				{
					int num = InSequence[i];
					text = ((num <= 0 || num >= 13) ? (text + SequenceSymbol[num]) : (text + num));
					if (i < InSequence.Length - 1)
					{
						text += ",";
					}
				}
				return text;
			}
			return "";
		}

		public static string ConvertTextStringToSequence(string InSequence)
		{
			if (InSequence.Length > 0)
			{
				InSequence = InSequence.ToLower();
				string text = "";
				int num = -1;
				string text2 = "";
				while (InSequence != "")
				{
					text = DataUtil.ExtractOneInfo(ref InSequence, ',');
					num = -1;
					if (text != "")
					{
						switch (text)
						{
							case "c":
								num = 0;
								break;
							case "b":
								num = 100;
								break;
							case "w":
								num = 103;
								break;
							case "e":
								num = 101;
								break;
							case "t":
								num = 102;
								break;
							case "p":
								num = 111;
								break;
							case "q":
								num = 112;
								break;
							default:
								num = DataUtil.StringToInt(text, Minus1IfBlank: true);
								if (num > 9 || num < 0)
								{
									num = -1;
								}
								break;
						}
					}
					if (num >= 0)
					{
						text2 += (char)num;
					}
				}
				return text2;
			}
			return "";
		}

		public static string FormatMode(int InPart)
		{
			int num = PB_WordsBold[InPart];
			int num2 = PB_WordsItalic[InPart];
			int num3 = PB_WordsUnderline[InPart];
			string str = "\\cf" + Convert.ToString(InPart + 1);
			string str2 = (num > 0) ? ((num2 > 0) ? ((num3 <= 0) ? "\\b\\i\\ulnone " : "\\b\\i\\ul ") : ((num3 <= 0) ? "\\b\\i0\\ulnone " : "\\b\\i0\\ul ")) : ((num2 > 0) ? ((num3 <= 0) ? "\\b0\\i\\ulnone " : "\\b0\\i\\ul ") : ((num3 <= 0) ? "\\b0\\i0\\ulnone " : "\\b0\\i0\\ul "));
			return "\\fs" + Convert.ToString(PB_WordsSize[InPart] * 2) + str + str2;
		}

#if DAO
		public static bool LoadDataIntoItem(ref SongSettings InItem, Recordset rs, string InID)
		{
			try
			{
				rs.Seek("=", InID, def, def, def, def, def, def, def, def, def, def, def, def);
				if (!rs.NoMatch)
				{
					return LoadDataIntoItem(ref InItem, rs);
				}

				InitialiseIndividualData(ref InItem);
			}
			catch
			{
			}
			return false;
		}
#elif SQLite
		public static bool LoadDataIntoItem(ref SongSettings InItem, DataTable dt, string InID)
		{
			try
			{
				DataRow[] arrRows = null;
				arrRows = dt.Select("SONGID='"+ InID + "'");

				if (arrRows != null && arrRows.Length > 0)
				{
					return LoadDataIntoItem(ref InItem, arrRows[0]);
				}

				InitialiseIndividualData(ref InItem);
			}
			catch
			{
			}
			return false;
		}
#endif

#if DAO
		public static bool LoadDataIntoItem(ref SongSettings InItem, Recordset rs)
		{
			InitialiseIndividualData(ref InItem);
			try
			{
				InItem.Title = DataUtil.GetDataString(rs, "Title_1");
				InItem.Title2 = DataUtil.GetDataString(rs, "Title_2");
				InItem.SongNumber = DataUtil.GetDataInt(rs, "song_number");
				InItem.CompleteLyrics = DataUtil.GetDataString(rs, "Lyrics");
				InItem.FolderNo = DataUtil.GetDataInt(rs, "FolderNo");
				InItem.Copyright = DataUtil.GetDataString(rs, "Copyright");
				InItem.Show_LicAdminInfo1 = DataUtil.GetDataString(rs, "LICENCE_ADMIN1");
				InItem.Show_LicAdminInfo2 = DataUtil.GetDataString(rs, "LICENCE_ADMIN2");
				InItem.Format.FormatString = DataUtil.GetDataString(rs, "FORMATDATA");
				InItem.Notations = DataUtil.GetDataString(rs, "msc");
				InItem.Capo = DataUtil.GetDataInt(rs, "capo", Minus1IfBlank: true);
				InItem.SongSequence = DataUtil.GetDataString(rs, "Sequence");
				InItem.Writer = DataUtil.GetDataString(rs, "Writer");
				InItem.Book_Reference = DataUtil.GetDataString(rs, "Book_Reference");
				InItem.User_Reference = DataUtil.GetDataString(rs, "User_Reference");
				InItem.Timing = DataUtil.GetDataString(rs, "timing");
				InItem.MusicKey = DataUtil.GetDataString(rs, "key");
				InItem.Category = DataUtil.GetDataString(rs, "category");
				InItem.Settings = DataUtil.GetDataString(rs, "SETTINGS");
				return true;
			}
			catch
			{
			}
			return false;
		}
#elif SQLite
		public static bool LoadDataIntoItem(ref SongSettings InItem, DataRow dr)
		{
			InitialiseIndividualData(ref InItem);
			try
			{
				InItem.Title = DataUtil.GetDataString(dr, "Title_1");
				InItem.Title2 = DataUtil.GetDataString(dr, "Title_2");
				InItem.SongNumber = DataUtil.GetDataInt(dr, "song_number");
				InItem.CompleteLyrics = DataUtil.GetDataString(dr, "Lyrics");
				InItem.FolderNo = DataUtil.GetDataInt(dr, "FolderNo");
				InItem.Copyright = DataUtil.GetDataString(dr, "Copyright");
				InItem.Show_LicAdminInfo1 = DataUtil.GetDataString(dr, "LICENCE_ADMIN1");
				InItem.Show_LicAdminInfo2 = DataUtil.GetDataString(dr, "LICENCE_ADMIN2");
				InItem.Format.FormatString = DataUtil.GetDataString(dr, "FORMATDATA");
				InItem.Notations = DataUtil.GetDataString(dr, "msc");
				InItem.Capo = DataUtil.GetDataInt(dr, "capo", Minus1IfBlank: true);
				InItem.SongSequence = DataUtil.GetDataString(dr, "Sequence");
				InItem.Writer = DataUtil.GetDataString(dr, "Writer");
				InItem.Book_Reference = DataUtil.GetDataString(dr, "Book_Reference");
				InItem.User_Reference = DataUtil.GetDataString(dr, "User_Reference");
				InItem.Timing = DataUtil.GetDataString(dr, "timing");
				InItem.MusicKey = DataUtil.GetDataString(dr, "key");
				InItem.Category = DataUtil.GetDataString(dr, "category");
				InItem.Settings = DataUtil.GetDataString(dr, "SETTINGS");
				return true;
			}
			catch
			{
			}
			return false;
		}
#endif

		public static void ResetPictureBox(ref SongSettings InItem, ref ImageTransitionControl InScreen, GapType GapItemBackground, ImageTransitionControl.TransitionAction InTransAction)
		{
			Color color = InItem.Format.ShowScreenColour[0];
			Color color2 = InItem.Format.ShowScreenColour[1];
			int showScreenStyle = InItem.Format.ShowScreenStyle;
			string tempImageFileName = InItem.Format.TempImageFileName;
			string backgroundPicture = InItem.Format.BackgroundPicture;
			int backgroundMode = (int)InItem.Format.BackgroundMode;
			bool useDefaultFormat = InItem.UseDefaultFormat;
			string text = InItem.Lyrics[0].Text;
			string text2 = InItem.Lyrics[1].Text;
			string text3 = InItem.Lyrics[2].Text;
			string songSequence = InItem.SongSequence;
			InItem.SongSequence = "";
			InItem.Lyrics[0].Text = "";
			InItem.Lyrics[1].Text = "";
			InItem.Lyrics[2].Text = "";
			InItem.Format.BackgroundPicture = "";
			InItem.Format.MediaLocation = "";
			if (InTransAction != ImageTransitionControl.TransitionAction.AsStored)
			{
				InItem.Format.ShowSlideTransition = 0;
			}
			if (InItem.OutputStyleScreen)
			{
				switch (GapItemBackground)
				{
					case GapType.Black:
						InItem.UseDefaultFormat = false;
						InItem.Format.ShowScreenColour[0] = BlackScreenColour;
						InItem.Format.ShowScreenColour[1] = BlackScreenColour;
						InItem.Format.ShowScreenStyle = 11;
						InItem.Format.TempImageFileName = "";
						InItem.Format.BackgroundPicture = "";
						InItem.Format.ShowSlideTransition = (GapItemUseFade ? 15 : 0);
						break;
					case GapType.User:
						{
							InItem.UseDefaultFormat = false;
							InItem.Format.TempImageFileName = GapItemLogoFile;
							InItem.Format.BackgroundPicture = GapItemLogoFile;
							string directoryName = Path.GetDirectoryName(InItem.Format.BackgroundPicture);
							if (directoryName == RootEasiSlidesDir + "Images\\Tiles")
							{
								InItem.Format.BackgroundMode = ImageMode.Tile;
							}
							else
							{
								InItem.Format.BackgroundMode = ImageMode.BestFit;
							}
							InItem.Format.ShowSlideTransition = (GapItemUseFade ? 15 : 0);
							break;
						}
					case GapType.Default:
						InItem.UseDefaultFormat = false;
						InItem.Format.ShowScreenColour[0] = ShowScreenColour[0];
						InItem.Format.ShowScreenColour[1] = ShowScreenColour[1];
						InItem.Format.ShowScreenStyle = ShowScreenStyle;
						InItem.Format.TempImageFileName = BackgroundPicture;
						InItem.Format.BackgroundPicture = BackgroundPicture;
						InItem.Format.BackgroundMode = BackgroundMode;
						InItem.Format.ShowSlideTransition = (GapItemUseFade ? 15 : 0);
						break;
				}
			}
			SetShowBackground(InItem, ref InScreen, FallBackToDefault: false);
			InItem.UseDefaultFormat = useDefaultFormat;
			InItem.Format.ShowScreenColour[0] = color;
			InItem.Format.ShowScreenColour[1] = color2;
			InItem.Format.ShowScreenStyle = showScreenStyle;
			InItem.Format.BackgroundPicture = backgroundPicture;
			InItem.Format.TempImageFileName = tempImageFileName;
			InItem.Format.BackgroundMode = (ImageMode)backgroundMode;
			DrawText(ref InItem, ref InScreen, InItem.LyricsAndNotationsList, DoActiveIndicator: false, ClearAll: true);
		}

		public static string LookUpMediaString(DShowLib Player)
		{
			switch (LookUpMediaInteger(Player))
			{
				case 1:
					return "Audio Only";
				case 2:
					return "Video";
				default:
					return "No Media Playing";
			}
		}

		public static string LookUpMediaString(DShowLib Player, int InCode, int WaitCount)
		{
			switch (InCode)
			{
				case -1:
					return "Selected Source Not Playable";
				case 0:
					{
						WaitCount /= 3;
						string text = "";
						for (int i = 0; i < WaitCount; i++)
						{
							text += ".";
						}
						return "Connecting to media..." + text;
					}
				default:
					return "No Media Playing";
			}
		}

		public static int LookUpMediaInteger(DShowLib Player)
		{
			try
			{
				if (Player.newFilename != "")
				{
					if (Player.isVideo)
					{
						return 1;
					}
					return 2;
				}
				return 3;
			}
			catch
			{
				return 3;
			}
		}

		public static string GetMediaLocation(SongSettings InItem)
		{
			return GetMediaLocation(InItem.Format.MediaOption, InItem.Title, InItem.Title2, InItem.UseDefaultFormat, InItem.Type, InItem.Format.MediaLocation, InItem.Format.MediaCaptureDeviceNumber);
		}

		public static string GetMediaLocation(int InMediaOption, string InTitle1, string InTitle2, bool InUseDefaultFormat, string InType, string InMediaLocation, int InMediaCaptureDeviceNumber)
		{
			string text = "";
			InMediaOption = ((InUseDefaultFormat && InType != "M") ? MediaOption : InMediaOption);
			switch (InMediaOption)
			{
				case 1:
					return GetMediaFileName(InTitle1, InTitle2);
				case 2:
					return (InUseDefaultFormat && InType != "M") ? MediaLocation : InMediaLocation;
				case 3:
					return "<<Capture>>" + (InUseDefaultFormat ? MediaCaptureDeviceNumber.ToString() : InMediaCaptureDeviceNumber.ToString());
				default:
					return "";
			}
		}

		public static string OldGetMediaLocation(SongSettings InItem)
		{
			string text = "";
			switch (InItem.Format.MediaOption)
			{
				case 1:
					return GetMediaFileName(InItem.Title, InItem.Title2);
				case 2:
					return (InItem.UseDefaultFormat && InItem.Type != "M") ? MediaLocation : InItem.Format.MediaLocation;
				case 3:
					return "<<Capture>>" + InItem.Format.MediaCaptureDeviceNumber;
				default:
					return "";
			}
		}

		public static void GetRotationStyle(ref SongSettings InItem)
		{
			string InString = InItem.RotateString;
			string InString2 = "";
			string text = "";
			string[] array = InString.Split("»"[0]);
			if (array.GetUpperBound(0) >= 0)
			{
				InString = array[0];
				if (array.GetUpperBound(0) >= 1)
				{
					InString2 = array[1];
				}
			}
			int num = DataUtil.StringToInt(DataUtil.ExtractOneInfo(ref InString, ';', RemoveExtract: true, MinusOneIfBlank: false));
			if (num < 1 || num > 2)
			{
				num = 1;
			}
			InItem.RotateStyle = num;
			int num2 = DataUtil.StringToInt(DataUtil.ExtractOneInfo(ref InString, ';', RemoveExtract: true, MinusOneIfBlank: false));
			if (num2 < 0 || num2 > 99999)
			{
				num2 = 0;
			}
			InItem.RotateGap = num2;
			int num3 = DataUtil.StringToInt(DataUtil.ExtractOneInfo(ref InString, ';', RemoveExtract: true, MinusOneIfBlank: false));
			if (num3 < 0 || num3 > 3599)
			{
				num3 = 0;
			}
			InItem.RotateTotal = num3;
			text = InString;
			InItem.RotateTimings = "";
			InItem.RotateSequence = "";
			while (text != "")
			{
				num2 = DataUtil.StringToInt(DataUtil.ExtractOneInfo(ref text, ';', RemoveExtract: true, MinusOneIfBlank: true));
				if ((num2 >= 0 && num2 < 99) || (num2 >= 100 && num2 < 112))
				{
					InItem.RotateSequence += (char)num2;
				}
			}
			int num4 = num;
			if (num4 != 2)
			{
				return;
			}
			ListBox listBox = new ListBox();
			listBox.Sorted = false;
			while (InString2 != "")
			{
				num2 = DataUtil.StringToInt(DataUtil.ExtractOneInfo(ref InString2, ';', RemoveExtract: true, MinusOneIfBlank: true));
				if (num2 > 0 && num2 <= 99999)
				{
					listBox.Items.Add(num2.ToString("00000"));
				}
			}
			if (listBox.Items.Count > 0)
			{
				listBox.Sorted = true;
				string text2 = "";
				for (int i = 0; i < listBox.Items.Count; i++)
				{
					text2 = DataUtil.StringToInt(listBox.Items[i].ToString()).ToString();
					SongSettings obj = InItem;
					obj.RotateTimings = obj.RotateTimings + text2 + ';';
				}
			}
		}

		public static void LoadReferenceAlert(ref ImageTransitionControl InScreen, SongSettings InItem, bool ClearAll, bool DoActiveIndicator)
		{
			InScreen.StopRef();
			string text = "";
			if (!ClearAll)
			{
				switch (ReferenceAlertSource)
				{
					case 1:
						text = InItem.Title;
						break;
					case 2:
						text = InItem.SongNumber.ToString();
						break;
					case 3:
						text = InItem.Book_Reference;
						break;
					case 4:
						text = InItem.User_Reference;
						break;
					default:
						text = "";
						break;
				}
			}
			if (ReferenceAlertUsePick & (text != "") & (ReferenceAlertPickName != ""))
			{
				int num = text.IndexOf(ReferenceAlertPickName);
				if (num >= 0)
				{
					int num2 = num + ReferenceAlertPickName.Length;
					if (text.Length == num2)
					{
						text = ReferenceAlertPickSubstitute;
					}
					else if (ReferenceAlertPickSeparator != "")
					{
						int num3 = text.IndexOfAny(ReferenceAlertPickSeparator.ToCharArray(), num2);
						num3 = ((num3 >= 0) ? num3 : text.Length);
						text = ((ReferenceAlertPickSubstitute == "") ? ReferenceAlertPickName : ReferenceAlertPickSubstitute) + DataUtil.Mid(text, num2, num3 - num2);
					}
					else
					{
						text = ((ReferenceAlertPickSubstitute == "") ? ReferenceAlertPickName : ReferenceAlertPickSubstitute) + DataUtil.Mid(text, num2, text.Length - num2);
					}
				}
				else if (ReferenceAlertBlankIfPickNotFound)
				{
					text = "";
				}
			}
			if (DoActiveIndicator && text == "")
			{
				text = " ";
			}
			if (text == "")
			{
				InScreen.StopRef();
				InScreen.RefDisplayString = "";
			}
			else
			{
				ReferenceAlertFont = GetNewFont(ReferenceAlertFontName, ReferenceAlertFontSize, ReferenceAlertBold, ReferenceAlertItalic, ReferenceAlertUnderline, ShowErrorMsg: false);
				InScreen.LoadRef(InItem, DataUtil.Left(text, 50), ReferenceAlertDuration, ReferenceAlertFont, ReferenceAlertScroll, ReferenceAlertFlash, ReferenceAlertTransparent, ReferenceAlertShadow, ReferenceAlertOutline, ReferenceAlertTextColour, ReferenceAlertBackColour, ReferenceAlertTextAlign, ReferenceAlertVerticalAlign, BottomBorderFactor);
			}
		}

        public static void InitFolderFiles(string InFolder)
        {
			try
			{
				string[] files = Directory.GetFiles(InFolder);
				string[] array = files;
				foreach (string path in array)
				{
					try
					{
						File.Delete(path);
					}
					catch (Exception ex)
					{
						Trace.WriteLine($"ERROR : {ex.Message}, {ex.StackTrace}");
					}
				}
				string[] directories = Directory.GetDirectories(InFolder);
				array = directories;
				foreach (string text in array)
				{
					try
					{
                        InitFolderFiles(text);
						Directory.Delete(text);
					}
					catch (Exception ex)
					{
						Trace.WriteLine($"ERROR : {ex.Message}, {ex.StackTrace}");
					}
				}
			}
			catch (Exception ex)
			{
				Trace.WriteLine($"ERROR : {ex.Message}, {ex.StackTrace}");
			}
        }

        public static void DeleteFolderFiles(string InFolder)
		{
			try
			{
				if (CommonUtil.ProcessKill("POWERPNT"))
				{
					Thread.Sleep(2000);

					string[] files = Directory.GetFiles(InFolder);
					string[] array = files;
					foreach (string path in array)
					{
						try
						{
							File.Delete(path);
						}
						catch (Exception ex)
						{
							Trace.WriteLine($"ERROR : {ex.Message}, {ex.StackTrace}");
						}
					}
					string[] directories = Directory.GetDirectories(InFolder);
					array = directories;
					foreach (string text in array)
					{
						try
						{
							DeleteFolderFiles(text);
							Directory.Delete(text);
						}
						catch (Exception ex)
						{
							Trace.WriteLine($"ERROR : {ex.Message}, {ex.StackTrace}");
						}
					}
				}
			}
			catch (Exception ex)
			{
				Trace.WriteLine($"ERROR : {ex.Message}, {ex.StackTrace}");
			}
		}

		internal static int GetNextNonRotateItem(bool CurrentItemIsGapItem)
		{
			int startPresAt = StartPresAt;
			int startPresAt2 = StartPresAt;
			int num = -1;
			int num2 = -1;
			if (TotalWorshipListItems > 0)
			{
				int num3 = startPresAt2 + 1;
				for (num3 = startPresAt2 + 1; num3 <= TotalWorshipListItems; num3++)
				{
					int itemRotateResult = GetItemRotateResult(TempItem2, WorshipSongs[num3, 0]);
					if (itemRotateResult < 1)
					{
						if (num < 0)
						{
							num = num3;
						}
						else if (num2 < 0)
						{
							num2 = num3;
						}
						if (num2 > 0)
						{
							num3 = TotalWorshipListItems;
						}
					}
				}
				if (num > 0)
				{
					if (GapItemOption == GapType.None)
					{
						return num;
					}
					if (!CurrentItemIsGapItem)
					{
						return num - 1;
					}
					if (num == startPresAt2 + 1)
					{
						return num;
					}
					return num - 1;
				}
			}
			return startPresAt;
		}

		internal static int GetItemRotateResult(string InIDString)
		{
			return GetItemRotateResult(TempItem1, InIDString);
		}

		internal static int GetItemRotateResult(SongSettings InItem, string InIDString)
		{
			string a = DataUtil.Left(InIDString, 1);
			if ((a == "I") | (a == "D"))
			{
				string InTitle = "";
				LoadIndividualData(ref InItem, InIDString, "", 1, ref InTitle);
				MediaBackgroundStyle mediaBackgroundType = GetMediaBackgroundType(InItem, UpdateVariables: false);
				switch (InItem.RotateStyle)
				{
					case 1:
						if (InItem.RotateGap > 0)
						{
							return (mediaBackgroundType != MediaBackgroundStyle.Video && mediaBackgroundType != MediaBackgroundStyle.SameAsPrevious) ? 1 : 2;
						}
						break;
					case 2:
						if (InItem.RotateTimings != "" || InItem.RotateTotal > 0)
						{
							return (mediaBackgroundType != MediaBackgroundStyle.Video && mediaBackgroundType != MediaBackgroundStyle.SameAsPrevious) ? 1 : 2;
						}
						break;
				}
			}
			return 0;
		}

		public static MediaBackgroundStyle GetMediaBackgroundType(SongSettings InItem, bool UpdateVariables)
		{
			string mediaLocation = GetMediaLocation(InItem);
			if (mediaLocation == "")
			{
				if (UpdateVariables)
				{
					CurrentMediaLocation = "";
				}
				return MediaBackgroundStyle.None;
			}
			if (mediaLocation == CurrentMediaLocation)
			{
				return MediaBackgroundStyle.SameAsPrevious;
			}
			MediaBackgroundStyle mediaType = GetMediaType(mediaLocation);
			if (UpdateVariables)
			{
				CurrentMediaLocation = mediaLocation;
				CurrentMediaIsVideo = ((mediaType == MediaBackgroundStyle.Video) ? true : false);
			}
			return mediaType;
		}

		public static MediaBackgroundStyle GetMediaType(string InLocation)
		{
			if (DataUtil.Left(InLocation, "<<Capture>>".Length) == "<<Capture>>")
			{
				return MediaBackgroundStyle.Video;
			}
			string text = "";
			try
			{
				text = Path.GetExtension(InLocation).ToLower();
			}
			catch
			{
				return MediaBackgroundStyle.Audio;
			}
			for (int i = 0; i < TotalMediaFileExt; i++)
			{
				if (MediaFileExtension[i, 0] == text)
				{
					if (MediaFileExtension[i, 1] == MediaBackgroundStyle.Video.ToString())
					{
						return MediaBackgroundStyle.Video;
					}
					return MediaBackgroundStyle.Audio;
				}
			}
			return MediaBackgroundStyle.Audio;
		}

		public static void SubDivideTextAndNotations(string InString, string InNotation, Font MainFont, Font NotationsFont, ref ListView TextNotationsList, int InWidth)
		{
			InWidth /= 15;
			Graphics graphics = TextNotationsList.CreateGraphics();
			int num = -1;
			ListViewItem listViewItem = new ListViewItem();
			int num2 = 0;
			TextNotationsList.Items.Clear();
			while (InString != "")
			{
				int length = InString.Length;
				for (int num3 = length; num3 >= 1; num3--)
				{
					string text = DataUtil.Left(InString, num3);
					if (((graphics.MeasureString(text, MainFont, 32000, StringFormat.GenericDefault).Width <= (float)InWidth) | (text.IndexOf(" ") < 0)) && ((DataUtil.Right(text, 1) == " ") | (num3 == length) | (text.IndexOf(" ") < 0)))
					{
						listViewItem = TextNotationsList.Items.Add(text);
						listViewItem.SubItems.Add("");
						string InString2 = InNotation;
						string text2 = "";
						while (InString2 != "")
						{
							string text3 = DataUtil.ExtractOneInfo(ref InString2, ';');
							string text4 = DataUtil.ExtractOneInfo(ref InString2, ';');
							if (((text3 != "-1") & (text4 != "-1")) && Convert.ToInt32(text4) >= num2)
							{
								if (Convert.ToInt32(text4) >= num2 + num3 && num3 < length)
								{
									InString2 = "";
									continue;
								}
								object obj = text2;
								text2 = string.Concat(obj, text3, ';', Convert.ToString(Convert.ToInt32(text4) - num2), ';');
							}
						}
						listViewItem.SubItems.Add((text2 != "") ? text2 : " ");
						listViewItem.SubItems.Add(Convert.ToString(num2));
						num2 += num3;
						InString = DataUtil.Mid(InString, num3);
						num3 = 0;
					}
				}
			}
			for (int num3 = 0; num3 < TextNotationsList.Items.Count; num3++)
			{
				TextNotationsList.Items[num3].SubItems[1].Text = FormatNotationString(TextNotationsList, TextNotationsList.Items[num3].SubItems[0].Text, TextNotationsList.Items[num3].SubItems[2].Text, MainFont, NotationsFont);
			}
		}

		public static void OldSubDivideTextAndNotations(string InString, string InNotation, Font MainFont, Font NotationsFont, ref ListView TextNotationsList, int InWidth)
		{
			InWidth /= 15;
			Graphics graphics = TextNotationsList.CreateGraphics();
			int num = -1;
			ListViewItem listViewItem = new ListViewItem();
			int num2 = 0;
			TextNotationsList.Items.Clear();
			while (InString != "")
			{
				int length = InString.Length;
				for (int num3 = length; num3 >= 1; num3--)
				{
					string text = DataUtil.Left(InString, num3);
					if (((graphics.MeasureString(text, MainFont, 32000, StringFormat.GenericDefault).Width <= (float)InWidth) | (text.IndexOf(" ") < 0)) && ((DataUtil.Right(text, 1) == " ") | (num3 == length) | (text.IndexOf(" ") < 0)))
					{
						listViewItem = TextNotationsList.Items.Add(text);
						listViewItem.SubItems.Add("");
						string InString2 = InNotation;
						string text2 = "";
						while (InString2 != "")
						{
							string text3 = DataUtil.ExtractOneInfo(ref InString2, ';');
							string text4 = DataUtil.ExtractOneInfo(ref InString2, ';');
							if (((text3 != "-1") & (text4 != "-1")) && Convert.ToInt32(text4) >= num2)
							{
								if (Convert.ToInt32(text4) >= num2 + num3 && num3 < length)
								{
									InString2 = "";
									continue;
								}
								object obj = text2;
								text2 = string.Concat(obj, text3, ';', Convert.ToString(Convert.ToInt32(text4) - num2), ';');
							}
						}
						listViewItem.SubItems.Add((text2 != "") ? text2 : " ");
						listViewItem.SubItems.Add(Convert.ToString(num2));
						num2 += num3;
						InString = DataUtil.Mid(InString, num3);
						num3 = 0;
					}
				}
			}
			for (int num3 = 0; num3 < TextNotationsList.Items.Count; num3++)
			{
			}
		}

		public static void SetLiveShowScreenSaverSettings()
		{
			SystemParametersInfo(16, 0, ref PriorScreenSaverState, 0);
			if (PriorScreenSaverState && DisableSreenSaver)
			{
				SetScreenSaverActive(SetOn: false);
			}
		}

		public static void RestoreScreenSaverSettings()
		{
			if (PriorScreenSaverState)
			{
				SetScreenSaverActive(SetOn: true);
			}
		}

		public static void SetScreenSaverActive(bool SetOn)
		{
			SystemParametersInfo(17, SetOn ? 1 : 0, ref SetOn, 0);
		}

		public static void HighlightDisplaySlidesText(SongSettings InItem, ref RichTextBox InTextBox)
		{
			HighlightDisplaySlidesText(InItem, ref InTextBox, ScrollToCaret: true);
		}

		public static void HighlightDisplaySlidesText(SongSettings InItem, ref RichTextBox InTextBox, bool ScrollToCaret)
		{
			HighlightDisplaySlidesText(InItem, ref InTextBox, ScrollToCaret, BlackScreenColour, Color.Red);
		}

		public static void HighlightDisplaySlidesText(SongSettings InItem, ref RichTextBox InTextBox, bool ScrollToCaret, Color TextColour, Color HighlightColour)
		{
			InItem.CurSlide = ((InItem.CurSlide < 1) ? 1 : ((InItem.CurSlide > InItem.TotalSlides) ? InItem.TotalSlides : InItem.CurSlide));
			InTextBox.Select(0, InTextBox.Text.Length);
			InTextBox.SelectionColor = TextColour;
			InTextBox.Select(InItem.Slide[InItem.CurSlide, 5], InItem.Slide[InItem.CurSlide, 6]);
			InTextBox.SelectionColor = HighlightColour;
			InTextBox.SelectionLength = 0;
			if (ScrollToCaret)
			{
				InTextBox.Select(InItem.Slide[InItem.CurSlide, 5] + InItem.Slide[InItem.CurSlide, 6] + 90, 0);
				InTextBox.ScrollToCaret();
				InTextBox.Select(InItem.Slide[InItem.CurSlide, 5], 0);
				InTextBox.ScrollToCaret();
			}
		}

		public static void DisplaySlidesFormattedLyrics(ref SongSettings InItem, ref RichTextBox PInTextBox, ref RichTextBox OInTextBox, bool ScrollToCaret, bool PreviewNotations)
		{
			if (InItem.OutputStyleScreen)
			{
				DisplaySlidesFormattedLyrics(ref InItem, ref OInTextBox, ScrollToCaret, PreviewNotations);
			}
			else
			{
				DisplaySlidesFormattedLyrics(ref InItem, ref PInTextBox, ScrollToCaret, PreviewNotations);
			}
		}

		public static void DisplaySlidesFormattedLyrics(ref SongSettings InItem, ref RichTextBox InTextBox, bool ScrollToCaret, bool PreviewNotations)
		{
			InItem.CurSlide = ((InItem.CurSlide < 1) ? 1 : ((InItem.CurSlide > InItem.TotalSlides) ? InItem.TotalSlides : InItem.CurSlide));
			InItem.FolderNo = ((InItem.FolderNo <= 0) ? 1 : InItem.FolderNo);
			int num = 0;
			InTextBox.Text = "";
			if (InItem.Type == "P")
			{
				return;
			}
			int num2 = 0;
			int num3 = 0;
			for (int i = 1; i <= InItem.TotalSlides; i++)
			{
				num2 = InTextBox.Text.Length;
				int num4 = 0;
				try
				{
					if (InItem.Slide[i, 0] >= 0)
					{
						if (i > 1)
						{
							InTextBox.Text += "\n";
						}
						if (InItem.Slide[i, 0] == 0)
						{
							InTextBox.Text += ((FolderLyricsHeading[InItem.FolderNo, 1] != "") ? FolderLyricsHeading[InItem.FolderNo, 1] : "Chorus:");
						}
						else if (InItem.Slide[i, 0] == 102)
						{
							InTextBox.Text += ((FolderLyricsHeading[InItem.FolderNo, 1] != "") ? (FolderLyricsHeading[InItem.FolderNo, 1] + " (2)") : "Chorus 2:");
						}
						else if (InItem.Slide[i, 0] == 111)
						{
							InTextBox.Text += ((FolderLyricsHeading[InItem.FolderNo, 0] != "") ? FolderLyricsHeading[InItem.FolderNo, 0] : "Prechorus:");
						}
						else if (InItem.Slide[i, 0] == 112)
						{
							InTextBox.Text += ((FolderLyricsHeading[InItem.FolderNo, 0] != "") ? (FolderLyricsHeading[InItem.FolderNo, 0] + " (2)") : "Prechorus 2:");
						}
						else if (InItem.Slide[i, 0] == 100)
						{
							InTextBox.Text += ((FolderLyricsHeading[InItem.FolderNo, 2] != "") ? FolderLyricsHeading[InItem.FolderNo, 2] : "Bridge:");
						}
						else if (InItem.Slide[i, 0] == 103)
						{
							InTextBox.Text += ((FolderLyricsHeading[InItem.FolderNo, 2] != "") ? (FolderLyricsHeading[InItem.FolderNo, 2] + " (2)") : "Bridge 2:");
						}
						else if (InItem.Slide[i, 0] == 101)
						{
							InTextBox.Text += ((FolderLyricsHeading[InItem.FolderNo, 3] != "") ? FolderLyricsHeading[InItem.FolderNo, 3] : "Ending:");
						}
						else if (InItem.Verse2Present || (i > 1 && InItem.Slide[i, 0] == 1))
						{
							RichTextBox obj = InTextBox;
							obj.Text = obj.Text + VerseTitle[InItem.Slide[i, 0]] + ".";
							int num5 = InItem.Slide[i, 0];
						}
						num4 = InTextBox.Text.Length - num2;
					}
				}
				catch
				{
					MessageBox.Show("Error");
				}
				InTextBox.Text += "\n";
				if (InItem.Slide[i, 2] >= 0)
				{
					InTextBox.Text += GetSlideContents(InItem, i, 0, InTextBox.Font, PreviewNotations);
				}
				if (InItem.Slide[i, 4] >= 0)
				{
					InTextBox.Text += GetSlideContents(InItem, i, 1, InTextBox.Font, PreviewNotations);
				}
				num3 = InTextBox.Text.Length - num2 + 1;
				InItem.Slide[i, 5] = num2;
				InItem.Slide[i, 6] = num3;
				InItem.Slide[i, 7] = num4;
			}
			for (int i = 1; i <= InItem.TotalSlides; i++)
			{
				if (InItem.Slide[InItem.CurSlide, 7] > 0)
				{
					InTextBox.Select(InItem.Slide[i, 5], InItem.Slide[i, 7]);
					InTextBox.SelectionFont = new Font(InTextBox.Font, FontStyle.Regular);
					InTextBox.SelectionLength = 0;
				}
			}
			HighlightDisplaySlidesText(InItem, ref InTextBox, ScrollToCaret);
		}

		public static void DisplayRichTextBoxSeries(ref SongSettings InItem, ref Panel InPanel, ref RichTextBox[] InRichTextBox, bool ScrollToCaret, bool PreviewNotations)
		{
			InItem.CurSlide = ((InItem.CurSlide < 1) ? 1 : ((InItem.CurSlide > InItem.TotalSlides) ? InItem.TotalSlides : InItem.CurSlide));
			InItem.FolderNo = ((InItem.FolderNo <= 0) ? 1 : InItem.FolderNo);
			int num = 0;
			if (InRichTextBox[1] == null || InItem.Type == "P")
			{
				return;
			}
			int num2 = 0;
			int num3 = 0;
			string text = "";
			for (int i = 1; i <= InItem.TotalSlides; i++)
			{
				text = "";
				num2 = text.Length;
				int num4 = 0;
				try
				{
					if (InItem.Slide[i, 0] >= 0)
					{
						if (i > 1)
						{
							text += "\n";
						}
						if (InItem.Slide[i, 0] == 0)
						{
							text += ((FolderLyricsHeading[InItem.FolderNo, 1] != "") ? FolderLyricsHeading[InItem.FolderNo, 1] : "Chorus:");
						}
						else if (InItem.Slide[i, 0] == 102)
						{
							text += ((FolderLyricsHeading[InItem.FolderNo, 1] != "") ? (FolderLyricsHeading[InItem.FolderNo, 1] + " (2)") : "Chorus 2:");
						}
						else if (InItem.Slide[i, 0] == 111)
						{
							text += ((FolderLyricsHeading[InItem.FolderNo, 0] != "") ? FolderLyricsHeading[InItem.FolderNo, 0] : "Prechorus:");
						}
						else if (InItem.Slide[i, 0] == 112)
						{
							text += ((FolderLyricsHeading[InItem.FolderNo, 0] != "") ? (FolderLyricsHeading[InItem.FolderNo, 0] + " (2)") : "Prechorus 2:");
						}
						else if (InItem.Slide[i, 0] == 100)
						{
							text += ((FolderLyricsHeading[InItem.FolderNo, 2] != "") ? FolderLyricsHeading[InItem.FolderNo, 2] : "Bridge:");
						}
						else if (InItem.Slide[i, 0] == 103)
						{
							text += ((FolderLyricsHeading[InItem.FolderNo, 2] != "") ? (FolderLyricsHeading[InItem.FolderNo, 2] + " (2)") : "Bridge 2:");
						}
						else if (InItem.Slide[i, 0] == 101)
						{
							text += ((FolderLyricsHeading[InItem.FolderNo, 3] != "") ? FolderLyricsHeading[InItem.FolderNo, 3] : "Ending:");
						}
						else if (InItem.Verse2Present || (i > 1 && InItem.Slide[i, 0] == 1))
						{
							text = text + VerseTitle[InItem.Slide[i, 0]] + ".";
							int num5 = InItem.Slide[i, 0];
						}
						num4 = text.Length - num2;
					}
				}
				catch
				{
					MessageBox.Show("Error");
				}
				text += "\n";
				if (InItem.Slide[i, 2] >= 0)
				{
					text += GetSlideContents(InItem, i, 0, InRichTextBox[1].Font, PreviewNotations);
				}
				if (InItem.Slide[i, 4] >= 0)
				{
					text += GetSlideContents(InItem, i, 1, InRichTextBox[1].Font, PreviewNotations);
				}
				text = DataUtil.TrimStart(text);
				text = DataUtil.TrimEnd(text);
				if (InRichTextBox[i] != null)
				{
					InRichTextBox[i].Text = text;
					InRichTextBox[i].SelectAll();
					InRichTextBox[i].SelectionFont = new Font("Microsoft Sans Serif", InRichTextBox[i].Font.Size, InRichTextBox[i].Font.Style);
					InRichTextBox[i].SelectionStart = 0;
					InRichTextBox[i].SelectionLength = 0;
				}
				num3 = text.Length - num2 + 1;
				InItem.Slide[i, 5] = num2;
				InItem.Slide[i, 6] = num3;
				InItem.Slide[i, 7] = num4;
			}
			HighlightRichTextBox(ref InRichTextBox, ref InPanel, InItem, OnEnterPanel: false, ScrollToCaret);
		}

		public static void RefreshWindowsDesktop()
		{
			InvalidateRect(IntPtr.Zero, IntPtr.Zero, bErase: true);
		}

		public static void BuildSubFolderList(string InDir, string RemovePrefix, ref string[,] InGroup, ref int InTotal)
		{
			string[] directories = Directory.GetDirectories(InDir);
			if (directories.Length > 0)
			{
				SingleArraySort(directories);
			}
			string[] array = directories;
			foreach (string text in array)
			{
				if ((!(RemovePrefix == ImagesDir) || !(text == RootEasiSlidesDir + "Images\\Scenery")) && !(text == RootEasiSlidesDir + "Images\\Tiles") && InTotal < 255)
				{
					InGroup[InTotal, 1] = text + "\\";
					InGroup[InTotal, 0] = "\\" + text.Replace(RemovePrefix, "");
					InTotal++;
					BuildSubFolderList(text, RemovePrefix, ref InGroup, ref InTotal);
				}
			}
		}

		public static void ClipboardCopyTextBox(RichTextBox InTextBox)
		{
			if (InTextBox.SelectedText != "")
			{
				Clipboard.SetDataObject(InTextBox.SelectedText);
			}
			else
			{
				Clipboard.SetDataObject("");
			}
		}

		public static void ClipboardPasteTextBox(RichTextBox InTextBox, int Location)
		{
			InTextBox.SelectionLength = 0;
			InTextBox.SelectionStart = Location;
			InTextBox.Focus();
			InTextBox.Paste();
		}

		public static void ClipboardPasteTextBox(RichTextBox InTextBox, int Location, string InPasteString)
		{
			Clipboard.SetDataObject(InPasteString.Replace("\r\n", "\n").Replace("\r", "\n").Replace("\v", "\n"));
			InTextBox.SelectionLength = 0;
			InTextBox.SelectionStart = Location;
			InTextBox.Focus();
			InTextBox.Paste();
		}

		public static void InsertIndicator(ref RichTextBox InTextBox, int InNum)
		{
			int CurPos = InTextBox.SelectionStart;
			string selectedText = (InNum == 152) ? " »" : VerseSymbol[InNum];
			string text = "";
			switch (InNum)
			{
				case 151:
					GetCurPosInLine(InTextBox.Text, ref CurPos);
					InTextBox.SelectionStart = CurPos;
					break;
				case 152:
					MoveToPosInLine(InTextBox.Text, ref CurPos, 1);
					InTextBox.SelectionStart = CurPos;
					break;
				default:
					GetCurPosInLine(InTextBox.Text, ref CurPos);
					InTextBox.SelectionStart = CurPos;
					text = (((DataUtil.Mid(InTextBox.Text, CurPos, 1) == "\r") | (DataUtil.Mid(InTextBox.Text, CurPos, 1) == "\n")) ? "" : "\r\n");
					break;
			}
			InTextBox.SelectedText = selectedText;
			if (text != "")
			{
				InTextBox.SelectedText = text;
			}
			if (InNum == 152)
			{
				InTextBox.SelectionStart -= 1;
			}
		}

		public static void LoadBlankCaptureDevices(ref ToolStripComboBox InComboBoxDevice, ref ToolStripComboBox InComboBoxMedium)
		{
			InComboBoxDevice.Items.Clear();
			for (int i = 1; i <= 10; i++)
			{
				InComboBoxDevice.Items.Add(i + ".");
			}
			InComboBoxMedium.Items.Clear();
			InComboBoxMedium.Items.Add("Video");
		}

		public static void LoadBlankCaptureDevices(ref ToolStripComboBox InComboBoxDevice)
		{
			InComboBoxDevice.Items.Clear();
			for (int i = 1; i <= 10; i++)
			{
				InComboBoxDevice.Items.Add(i + ".");
			}
		}

		public static void HighlightRichTextBox(ref RichTextBox[] InRichTextBox, ref Panel InPanel, SongSettings InItem, bool OnEnterPanel, bool ScrollToTop)
		{
			if (OnEnterPanel)
			{
				Control_Enter(InPanel);
			}
			else
			{
				Control_Leave(InPanel);
			}
			for (int i = 1; i <= InItem.TotalSlides; i++)
			{
				if (InRichTextBox[i] == null)
				{
					continue;
				}
				if (OnEnterPanel)
				{
					Control_Enter(InRichTextBox[i]);
					if ((string)InRichTextBox[i].Tag == InItem.CurSlide.ToString() && !InItem.GapItemOnDisplay)
					{
						InRichTextBox[i].ForeColor = TextRegionSlideTextColour;
						InRichTextBox[i].BackColor = TextRegionSlideBackColour;
						int top = InRichTextBox[i].Top;
						int num = top;
						if (ScrollToTop)
						{
							bool flag = (top <= 0) ? true : false;
							while (!flag)
							{
								SendMessage(InPanel.Handle, 277u, 3u, 0u);
								top = InRichTextBox[i].Top;
								if (top < num && top > 0)
								{
									num = top;
								}
								else
								{
									flag = true;
								}
							}
						}
						InRichTextBox[i].Focus();
						InPanel.ScrollControlIntoView(InRichTextBox[i]);
						if (!ScrollToTop && i < InItem.TotalSlides)
						{
							top = InRichTextBox[i].Top;
							int num2 = 0;
							while (top > 5 && num2 < 5)
							{
								SendMessage(InPanel.Handle, 277u, 1u, 0u);
								top = InRichTextBox[i].Top;
								num2++;
							}
						}
					}
					else
					{
						InRichTextBox[i].ForeColor = NormalTextColour;
					}
				}
				else
				{
					Control_Leave(InRichTextBox[i]);
					if ((string)InRichTextBox[i].Tag == InItem.CurSlide.ToString())
					{
						InRichTextBox[i].ForeColor = TextRegionSlideTextColour;
						InRichTextBox[i].BackColor = TextRegionSlideBackColour;
					}
					else
					{
						InRichTextBox[i].ForeColor = NormalTextColour;
					}
				}
			}
		}

		public static void Control_Enter(Control InControl)
		{
			Color InBackground = InControl.BackColor;
			SetEnterColour(ref InBackground);
			InControl.BackColor = InBackground;
			if (InControl.Name == "Main_QuickFind")
			{
				((TextBox)InControl).SelectAll();
			}
		}

		public static void Control_Leave(Control InControl)
		{
			Color InBackground = InControl.BackColor;
			SetLeaveColor(ref InBackground);
			InControl.BackColor = InBackground;
		}

		public static void SetEnterColour(ref Color InBackground)
		{
			if (UseFocusedTextRegionColour)
			{
				InBackground = FocusedTextRegionColour;
			}
			else
			{
				InBackground = NormalTextRegionBackColour;
			}
		}

		public static void SetLeaveColor(ref Color InBackground)
		{
			if (InBackground != NormalTextRegionBackColour)
			{
				InBackground = NormalTextRegionBackColour;
			}
		}

		/// <summary>
		/// daniel 
		/// 확장자 docx 추가
		/// </summary>
		/// <param name="InFileName"></param>
		/// <returns></returns>
		public static string ExtractDocTextContents(string InFileName)
		{
			string result = "";
			if (File.Exists(InFileName))
			{
				switch (Path.GetExtension(InFileName).ToLower())
				{
					case ".doc":
					case ".docx":
						result = GetOfficeDocContents(InFileName);
						break;
					case ".txt":
						result = LoadTextFile(InFileName);
						break;
				}
			}
			return result;
		}

		public static void LoadIndividualFormatData(ref SongSettings InItem, string InFormatString)
		{
			for (int i = 0; i < 255; i++)
			{
				InItem.Format.HeaderData[i] = ExtractHeaderInfo(InFormatString, i, '>');
			}
			int folderNo = InItem.FolderNo;
			int num = folderNo;
			int num2 = 1;
			if (InItem.Type == "B")
			{
				num = InItem.HBR2_FolderNo;
				num2 = 0;
			}
			if (folderNo >= 0)
			{
				InItem.FolderNo = folderNo;
				ShowFontBold[0, 0] = ShowFontBold[folderNo, 0];
				ShowFontBold[0, 1] = ShowFontBold[num, num2];
				ShowFontItalic[0, 0] = ShowFontItalic[folderNo, 0];
				ShowFontItalic[0, 1] = ShowFontItalic[num, num2];
				ShowFontUnderline[0, 0] = ShowFontUnderline[folderNo, 0];
				ShowFontUnderline[0, 1] = ShowFontUnderline[num, num2];
				ShowFontRTL[0, 0] = ShowFontRTL[folderNo, 0];
				ShowFontRTL[0, 1] = ShowFontRTL[num, num2];
				ShowFontBold[0, 2] = ShowFontBold[folderNo, 0];
				ShowFontBold[0, 3] = ShowFontBold[num, num2];
				ShowFontItalic[0, 2] = ShowFontItalic[folderNo, 2];
				ShowFontItalic[0, 3] = ShowFontItalic[num, 3];
				ShowFontUnderline[0, 2] = ShowFontUnderline[folderNo, 0];
				ShowFontUnderline[0, 3] = ShowFontUnderline[num, num2];
				ShowFontName[0, 0] = ShowFontName[folderNo, 0];
				ShowFontName[0, 1] = ShowFontName[num, num2];
				ShowFontSize[0, 0] = ShowFontSize[folderNo, 0] * InItem.FontSizeFactor / 100;
				ShowFontSize[0, 1] = ShowFontSize[num, num2] * ((InItem.Type == "B") ? InItem.HBR2_FontSizeFactor : InItem.FontSizeFactor) / 100;
				if (ShowFontSize[0, 0] <= 0)
				{
					ShowFontSize[0, 0] = 6;
				}
				if (ShowFontSize[0, 1] <= 0)
				{
					ShowFontSize[0, 1] = 6;
				}
				ShowBottomMargin[0] = ShowBottomMargin[folderNo];
				ShowLeftMargin[0] = ShowLeftMargin[folderNo];
				ShowRightMargin[0] = ShowRightMargin[folderNo];
				ShowFontVPosition[0, 0] = ShowFontVPosition[folderNo, 0];
				ShowFontVPosition[0, 1] = ShowFontVPosition[folderNo, 1];
				FolderHeadingFontBold[0, 0] = FolderHeadingFontBold[folderNo, 0];
				FolderHeadingFontItalic[0, 0] = FolderHeadingFontItalic[folderNo, 0];
				FolderHeadingFontUnderline[0, 0] = FolderHeadingFontUnderline[folderNo, 0];
				FolderHeadingFontBold[0, 1] = FolderHeadingFontBold[folderNo, 1];
				FolderHeadingFontItalic[0, 1] = FolderHeadingFontItalic[folderNo, 1];
				FolderHeadingFontUnderline[0, 1] = FolderHeadingFontUnderline[folderNo, 1];
				FolderHeadingPercentSize[0] = FolderHeadingPercentSize[folderNo];
				FolderHeadingOption[0] = FolderHeadingOption[folderNo];
				ShowLineSpacing[0, 0] = ShowLineSpacing[folderNo, 0];
				ShowLineSpacing[0, 1] = ShowLineSpacing[folderNo, 1];
			}
			int num3 = ExtractNumericData(InItem.Format.HeaderData[21]);
			InItem.Format.ShowSongHeadings = (((num3 < 0) | (num3 > 3)) ? ShowSongHeadings : num3);
			num3 = ExtractNumericData(InItem.Format.HeaderData[22]);
			InItem.Format.UseShadowFont = ((num3 < 0) ? UseShadowFont : DataUtil.GetBitValue(num3, 2));
			InItem.Format.ShowNotations = ((num3 < 0) ? ShowNotations : DataUtil.GetBitValue(num3, 3));
			InItem.Format.ShowInterlace = ((num3 < 0) ? ShowInterlace : DataUtil.GetBitValue(num3, 5));
			InItem.Format.UseOutlineFont = ((num3 < 0) ? UseOutlineFont : DataUtil.GetBitValue(num3, 6));
			InItem.Format.HideDisplayPanel = ((num3 >= 0) ? DataUtil.GetBitValue(num3, 7) : 0);
			num3 = ExtractNumericData(InItem.Format.HeaderData[23]);
			InItem.Format.ShowSongHeadingsAlign = (((num3 < 0) | (num3 > 4)) ? ShowSongHeadingsAlign : num3);
			num3 = ExtractNumericData(InItem.Format.HeaderData[25]);
			InItem.Format.ShowLyrics = (((num3 < 0) | (num3 > 2)) ? ShowLyrics : num3);
			num3 = ExtractNumericData(InItem.Format.HeaderData[26]);
			InItem.Format.ShowScreenColour[0] = (((InItem.Format.HeaderData[26] == "") | !ValidateColour(num3)) ? ShowScreenColour[0] : Color.FromArgb(Convert.ToInt32(num3)));
			num3 = ExtractNumericData(InItem.Format.HeaderData[27]);
			InItem.Format.ShowScreenColour[1] = (((InItem.Format.HeaderData[27] == "") | !ValidateColour(num3)) ? ShowScreenColour[0] : Color.FromArgb(Convert.ToInt32(num3)));
			num3 = ExtractNumericData(InItem.Format.HeaderData[28]);
			InItem.Format.ShowScreenStyle = (((num3 < 0) | (num3 > MaxBackgroundStyleIndex)) ? ShowScreenStyle : num3);
			num3 = ExtractNumericData(InItem.Format.HeaderData[29]);
			InItem.Format.ShowFontColour[0] = (((InItem.Format.HeaderData[29] == "") | !ValidateColour(num3)) ? ShowFontColour[0] : Color.FromArgb(Convert.ToInt32(num3)));
			num3 = ExtractNumericData(InItem.Format.HeaderData[30]);
			InItem.Format.ShowFontColour[1] = (((InItem.Format.HeaderData[30] == "") | !ValidateColour(num3)) ? ShowFontColour[1] : Color.FromArgb(Convert.ToInt32(num3)));
			num3 = ExtractNumericData(InItem.Format.HeaderData[31]);
			InItem.Format.ShowFontAlign[0] = (((num3 < 1) | (num3 > 3)) ? ShowFontAlign[0, 0] : num3);
			num3 = ExtractNumericData(InItem.Format.HeaderData[32]);
			InItem.Format.ShowFontAlign[1] = (((num3 < 1) | (num3 > 3)) ? ShowFontAlign[0, 1] : num3);
			num3 = ExtractNumericData(InItem.Format.HeaderData[41]);
			InItem.Format.ShowFontBold[0] = ((num3 < 0 || num3 > 127) ? ShowFontBold[0, 0] : DataUtil.GetBitValue(num3, 1));
			InItem.Format.ShowFontItalic[0] = ((num3 < 0 || num3 > 127) ? ShowFontItalic[0, 0] : DataUtil.GetBitValue(num3, 2));
			InItem.Format.ShowFontUnderline[0] = ((num3 < 0 || num3 > 127) ? ShowFontUnderline[0, 0] : DataUtil.GetBitValue(num3, 3));
			InItem.Format.ShowFontBold[2] = ((num3 < 0 || num3 > 127) ? ShowFontBold[0, 1] : DataUtil.GetBitValue(num3, 4));
			InItem.Format.ShowFontItalic[2] = ((num3 < 0 || num3 > 127) ? ShowFontItalic[0, 2] : DataUtil.GetBitValue(num3, 5));
			InItem.Format.ShowFontUnderline[2] = ((num3 < 0 || num3 > 127) ? ShowFontUnderline[0, 1] : DataUtil.GetBitValue(num3, 6));
			num3 = ExtractNumericData(InItem.Format.HeaderData[42]);
			InItem.Format.ShowFontBold[1] = ((num3 < 0 || num3 > 127) ? ShowFontBold[0, 1] : DataUtil.GetBitValue(num3, 1));
			InItem.Format.ShowFontItalic[1] = ((num3 < 0 || num3 > 127) ? ShowFontItalic[0, 1] : DataUtil.GetBitValue(num3, 2));
			InItem.Format.ShowFontUnderline[1] = ((num3 < 0 || num3 > 127) ? ShowFontUnderline[0, 1] : DataUtil.GetBitValue(num3, 3));
			InItem.Format.ShowFontBold[3] = ((num3 < 0 || num3 > 127) ? ShowFontBold[0, 3] : DataUtil.GetBitValue(num3, 4));
			InItem.Format.ShowFontItalic[3] = ((num3 < 0 || num3 > 127) ? ShowFontItalic[0, 3] : DataUtil.GetBitValue(num3, 5));
			InItem.Format.ShowFontUnderline[3] = ((num3 < 0 || num3 > 127) ? ShowFontUnderline[0, 3] : DataUtil.GetBitValue(num3, 6));
			InItem.Format.ShowFontName[0] = ((InItem.Format.HeaderData[43] == "") ? ShowFontName[0, 0] : InItem.Format.HeaderData[43]);
			InItem.Format.ShowFontName[1] = ((InItem.Format.HeaderData[44] == "") ? ShowFontName[0, 1] : InItem.Format.HeaderData[44]);
			num3 = ExtractNumericData(InItem.Format.HeaderData[45]);
			InItem.Format.ShowFontVPosition[0] = (((num3 < 0) | (num3 > 100)) ? ShowFontVPosition[0, 0] : num3);
			num3 = ExtractNumericData(InItem.Format.HeaderData[46]);
			InItem.Format.ShowFontVPosition[1] = (((num3 < InItem.Format.ShowFontVPosition[0]) | (num3 > 100)) ? ShowFontVPosition[0, 1] : num3);
			num3 = ExtractNumericData(InItem.Format.HeaderData[47]);
			InItem.Format.ShowFontSize[0] = (((num3 < 6) | (num3 > 100)) ? ShowFontSize[0, 0] : num3);
			num3 = ExtractNumericData(InItem.Format.HeaderData[48]);
			InItem.Format.ShowFontSize[1] = (((num3 < 6) | (num3 > 100)) ? ShowFontSize[0, 1] : num3);
			num3 = ExtractNumericData(InItem.Format.HeaderData[71]);
			InItem.Format.TransposeOffset = ((!((num3 < 0) | (num3 > 11))) ? num3 : 0);
			InItem.Format.PreviousTransposeOffset = InItem.Format.TransposeOffset;
			InItem.Format.BackgroundPicture = InItem.Format.HeaderData[61];
			num3 = ExtractNumericData(InItem.Format.HeaderData[50]);
			InItem.Format.MediaOption = (((num3 < 0) | (num3 > 3)) ? MediaOption : num3);
			InItem.Format.MediaLocation = InItem.Format.HeaderData[51];
			num3 = ExtractNumericData(InItem.Format.HeaderData[52]);
			InItem.Format.MediaVolume = (((num3 < 0) | (num3 > 100)) ? MediaVolume : num3);
			num3 = ExtractNumericData(InItem.Format.HeaderData[53]);
			InItem.Format.MediaBalance = (((num3 < -100) | (num3 > 100)) ? MediaBalance : num3);
			num3 = ExtractNumericData(InItem.Format.HeaderData[54]);
			InItem.Format.MediaMute = ((num3 < 0) ? MediaMute : DataUtil.GetBitValue(num3, 1));
			InItem.Format.MediaRepeat = ((num3 < 0) ? MediaRepeat : DataUtil.GetBitValue(num3, 2));
			InItem.Format.MediaWidescreen = ((num3 < 0) ? MediaWidescreen : DataUtil.GetBitValue(num3, 3));
			if (InItem.Type == "M")
			{
				InItem.Format.ShowSongHeadings = 0;
				InItem.Format.MediaOption = 2;
				InItem.Format.MediaLocation = InItem.ItemID;
			}
			num3 = ExtractNumericData(InItem.Format.HeaderData[55]);
			InItem.Format.MediaCaptureDeviceNumber = (((num3 < 1) | (num3 > 5)) ? 1 : num3);
			num3 = ExtractNumericData(InItem.Format.HeaderData[62]);
			InItem.Format.BackgroundMode = (ImageMode)(((num3 < 0) | (num3 > 2)) ? ((int)BackgroundMode) : num3);
			num3 = ExtractNumericData(InItem.Format.HeaderData[63]);
			InItem.Format.ShowVerticalAlign = (((num3 < 0) | (num3 > 2)) ? ShowVerticalAlign : num3);
			num3 = ExtractNumericData(InItem.Format.HeaderData[64]);
			InItem.Format.ShowLeftMargin = (((num3 < 0) | (num3 > 40)) ? ShowLeftMargin[0] : num3);
			num3 = ExtractNumericData(InItem.Format.HeaderData[65]);
			InItem.Format.ShowRightMargin = (((num3 < 0) | (num3 > 40)) ? ShowRightMargin[0] : num3);
			num3 = ExtractNumericData(InItem.Format.HeaderData[66]);
			InItem.Format.ShowBottomMargin = (((num3 < 0) | (num3 > 100)) ? ShowBottomMargin[0] : num3);
			if (InItem.Format.HeaderData[72] != "")
			{
				InItem.Format.ShowItemTransition = GlobalImageCanvas.GetTransitionType(InItem.Format.HeaderData[72]);
			}
			else
			{
				InItem.Format.ShowItemTransition = ShowItemTransition;
			}
			if (InItem.Format.HeaderData[73] != "")
			{
				InItem.Format.ShowSlideTransition = GlobalImageCanvas.GetTransitionType(InItem.Format.HeaderData[73]);
			}
			else
			{
				InItem.Format.ShowSlideTransition = ShowSlideTransition;
			}
			InItem.Format.FormatString = InFormatString;
			InItem.UseDefaultFormat = ((InFormatString == "") ? true : false);
			if (InItem.Format.ImageString != "" && InItem.Format.BackgroundPicture != "")
			{
				InItem.Format.TempImageFileName = dumpImageToFile(Convert.FromBase64String(InItem.Format.ImageString), InItem.Format.BackgroundPicture);
			}
		}
	}
}
