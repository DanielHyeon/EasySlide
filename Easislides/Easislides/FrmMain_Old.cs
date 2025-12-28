//using NetOffice.DAOApi;
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
	public class FrmMain : Form
	{
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
			Remote_LiveCamStartStop,
			Remote_LiveCamUpdate,
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

		private IContainer components = null;

		private ToolStripContainer toolStripContainerMain;

		private MenuStrip menuStripMain;

		private ToolStrip toolStripMain;

		private SplitContainer splitContainerMain;

		private SplitContainer splitContainer1;

		private StatusStrip statusStripMain;

		private SplitContainer splitContainer2;

		private SplitContainer splitContainerPreview;

		private SplitContainer splitContainerOutput;

		private TabControl tabControlSource;

		private TabPage tabFolders;

		private TabPage tabImages;

		private TabPage tabBibles;

		private TabPage tabDefault;

		private TabControl tabControlLists;

		private TabPage tabWorshipList;

		private TabPage tabPraiseBook;

		private Panel panel1;

		private Panel panel2;

		private Panel panel3;

		private Button PreviewBtnSlideDown;

		private Button PreviewBtnSlideUp;

		private Panel panel6;

		private Button OutputBtnItemDown;

		private Button OutputBtnItemUp;

		private Panel panel7;

		private Panel panel8;

		private Panel PreviewHolder;

		private Panel PreviewBack;

		private Panel OutputHolder;

		private Panel OutputBack;

		private Button OutputBtnSlideDown;

		private Button OutputBtnSlideUp;

		private Panel panel10;

		private Panel panel9;

		private CheckBox cbOutputBlack;

		private CheckBox cbOutputClear;

		private CheckBox cbGoLive;

		private Button btnToOutput;

		private Panel panelPreviewTop;

		private Panel panelOutputTop;

		private Panel panelPreviewBottom;

		private Panel panelOutputBottom;

		private RadioButton IndradioButtonText;

		private RadioButton IndradioButtonFormat;

		private ToolStripButton Main_New;

		private ToolStripButton Main_Edit;

		private ToolStripButton Main_Copy;

		private ToolStripButton Main_Move;

		private ToolStripButton Main_Delete;

		private ToolStripSeparator toolStripSeparator1;

		private ToolStripButton Main_Media;

		private ToolStripButton Main_Refresh;

		private ToolStripSeparator toolStripSeparator2;

		private ToolStripButton Main_Options;

		private ToolStripSeparator toolStripSeparator3;

		private ToolStripButton Main_Alerts;

		private ToolStripButton Main_Chinese;

		private ToolStripSeparator toolStripSeparator4;

		private ToolStripButton Main_Find;

		private ToolStripComboBox Main_QuickFind;

		private Panel IndPanel;

		private CheckBox Ind_checkBox;

		private GroupBox IndgroupBox1;

		private GroupBox IndgroupBox3;

		private ListView SongsList;

		private ComboBox SongFolder;

		private ComboBox ImagesFolder;

		private ComboBox BookLookup;

		private Panel panelBible2;

		private ToolStrip toolStripBible2;

		private ToolStripButton Bibles_Go;

		private TextBox BibleUserLookup;

		private RichTextBox BibleText;

		private Panel panelFolders;

		private ToolStrip toolStripFolders;

		private ToolStripButton Folders_WordCount;

		private ListView WorshipListItems;

		private ComboBox SessionList;

		private Panel panelPraiseBook1;

		private ListView PraiseBookItems;

		private ComboBox PraiseBook;

		private Panel panelWorshipList2;

		private ToolStrip toolStripWorshipList2;

		private ToolStripButton WL_Up;

		private ToolStripButton WL_Down;

		private Panel panelPraiseBook2;

		private ToolStrip toolStripPraiseBook2;

		private ToolStripButton PB_Delete;

		private Panel panelWorshipList1;

		private ToolStrip toolStripWorshipList1;

		private ToolStripButton WL_Manage;

		private ToolStripButton WL_Add;

		private ToolStripButton WL_Open;

		private ToolStrip toolStripPraiseBook1;

		private ToolStripButton PB_Manage;

		private ToolStripButton PB_Add;

		private ToolStripButton WL_Delete;

		private ToolStripSeparator toolStripSeparator6;

		private ToolStripButton WL_Word;

		private ToolStripSeparator toolStripSeparator7;

		private ToolStripButton PB_Word;

		private ToolStripButton PB_Html;

		private ToolStripButton PB_WordCount;

		private Panel DefPanel;

		private GroupBox DefgroupBox2;

		private GroupBox DefgroupBox3;

		private GroupBox DefgroupBox1;

		private Panel panelDef1;

		private ToolStrip toolStripDef1;

		private ToolStripButton Def_Outline;

		private ToolStripButton Def_Shadow;

		private ToolStripDropDownButton Def_Region;

		private ToolStripMenuItem Def_ShowRegion1;

		private ToolStripMenuItem Def_ShowRegion2;

		private ToolStripMenuItem Def_ShowRegionBoth;

		private ToolStripButton Def_Interlace;

		private ToolStripButton Def_Notations;

		private ToolStripButton Def_ToZero;

		private ToolStripDropDownButton Def_VAlign;

		private ToolStripMenuItem Def_VAlignTop;

		private ToolStripMenuItem Def_VAlignCentre;

		private ToolStripMenuItem Def_VAlignBottom;

		private Panel panelDef2;

		private ToolStrip toolStripDef2;

		private ToolStripDropDownButton Def_R1Align;

		private ToolStripMenuItem Def_R1AlignLeft;

		private ToolStripMenuItem Def_R1AlignCentre;

		private ToolStripMenuItem Def_R1AlignRight;

		private ToolStripDropDownButton Def_R2Align;

		private ToolStripMenuItem Def_R2AlignLeft;

		private ToolStripMenuItem Def_R2AlignCentre;

		private ToolStripMenuItem Def_R2AlignRight;

		private ToolStripButton Def_R1Colour;

		private ToolStripButton Def_R2Colour;

		private Button DefApplyDefaultsBtn;

		private Panel panelDef3;

		private ToolStrip toolStripDef3;

		private ToolStripDropDownButton Def_ImageMode;

		private ToolStripMenuItem Def_ImageTile;

		private ToolStripMenuItem Def_ImageCentre;

		private ToolStripMenuItem Def_ImageBestFit;

		private ToolStripButton Def_BackColour;

		private ToolStripButton Def_NoImage;

		private Panel panelDef4;

		private ToolStrip toolStripDef4;

		private ToolStripComboBox Def_TransItem;

		private ToolStripComboBox Def_TransSlides;

		private Panel panelDef6;

		private ToolStrip toolStripDef6;

		private ToolStripButton Def_PanelShow;

		private ToolStripButton Def_PanelSong;

		private ToolStripButton Def_PanelSlides;

		private ToolStripButton Def_PanelTitle;

		private ToolStripButton Def_PanelCopyright;

		private Panel panelDef5;

		private ToolStrip toolStripDef5;

		private ToolStripButton Def_PanelTextColour;

		private ToolStripSeparator toolStripSeparator14;

		private ToolStripButton Def_PanelBackColour;

		private ToolStripButton Def_PanelAsR1;

		private ToolStripButton Def_PanelTransparent;

		private Panel panelInd1;

		private ToolStrip toolStripInd1;

		private ToolStripButton Ind_Outline;

		private ToolStripButton Ind_Shadow;

		private ToolStripDropDownButton Ind_Region;

		private ToolStripMenuItem Ind_ShowRegion1;

		private ToolStripMenuItem Ind_ShowRegion2;

		private ToolStripMenuItem Ind_ShowRegionBoth;

		private ToolStripDropDownButton Ind_VAlign;

		private ToolStripMenuItem Ind_VAlignTop;

		private ToolStripMenuItem Ind_VAlignCentre;

		private ToolStripMenuItem Ind_VAlignBottom;

		private ToolStripButton Ind_Interlace;

		private ToolStripButton Ind_Notations;

		private NumericUpDown Ind_BottomUpDown;

		private NumericUpDown Ind_RightUpDown;

		private NumericUpDown Ind_LeftUpDown;

		private Label label3;

		private Label label2;

		private Label label1;

		private GroupBox IndgroupBox2;

		private Panel panelInd3;

		private ToolStrip toolStripInd3;

		private ToolStripComboBox Ind_TransItem;

		private ToolStripComboBox Ind_TransSlides;

		private Panel panelInd2;

		private ToolStrip toolStripInd2;

		private ToolStripDropDownButton Ind_ImageMode;

		private ToolStripMenuItem Ind_ImageTile;

		private ToolStripMenuItem Ind_ImageCentre;

		private ToolStripMenuItem Ind_ImageBestFit;

		private ToolStripButton Ind_NoImage;

		private ToolStripButton Ind_BackColour;

		private ToolStripButton Ind_AssignMedia;

		private Panel panelInd4;

		private ToolStrip toolStripInd4;

		private ToolStripSeparator toolStripSeparator13;

		private ToolStripDropDownButton Ind_R1Align;

		private ToolStripMenuItem Ind_R1AlignLeft;

		private ToolStripMenuItem Ind_R1AlignCentre;

		private ToolStripMenuItem Ind_R1AlignRight;

		private ToolStripButton Ind_R1Colour;

		private ToolStripButton Ind_R1Bold;

		private ToolStripButton Ind_R1Underline;

		private NumericUpDown Ind_Reg1TopUpDown;

		private Label label4;

		private NumericUpDown Ind_Reg1SizeUpDown;

		private Label labelBlackScreenOn;

		private Panel panelInd5;

		private ToolStrip toolStripInd5;

		private ToolStripComboBox Ind_Reg1FontsList;

		private GroupBox IndgroupBox4;

		private Panel panelInd7;

		private ToolStrip toolStripInd7;

		private ToolStripComboBox Ind_Reg2FontsList;

		private NumericUpDown Ind_Reg2SizeUpDown;

		private Label label6;

		private NumericUpDown Ind_Reg2TopUpDown;

		private Label label7;

		private Panel panelInd6;

		private ToolStrip toolStripInd6;

		private ToolStripButton Ind_R2Bold;

		private ToolStripButton Ind_R2Underline;

		private ToolStripSeparator toolStripSeparator15;

		private ToolStripDropDownButton Ind_R2Align;

		private ToolStripMenuItem Ind_R2AlignLeft;

		private ToolStripMenuItem Ind_R2AlignCentre;

		private ToolStripMenuItem Ind_R2AlignRight;

		private ToolStripButton Ind_R2Colour;

		private ImageList imageListSys;

		private ColumnHeader columnHeader1;

		private ColumnHeader columnHeader2;

		private ColumnHeader columnHeader3;

		private ColumnHeader columnHeader4;

		private ColumnHeader columnHeader5;

		private ColumnHeader columnHeader6;

		private ColumnHeader columnHeader7;

		private ColumnHeader columnHeader8;

		private ColumnHeader columnHeader9;

		private ColumnHeader columnHeader10;

		private ColumnHeader columnHeader11;

		private ColumnHeader columnHeader12;

		private ColumnHeader columnHeader13;

		private ColumnHeader columnHeader14;

		private ToolStripMenuItem Menu_MainFile;

		private ToolStripMenuItem Menu_MainEdit;

		private ToolStripMenuItem Menu_MainView;

		private ToolStripMenuItem Menu_MainOutput;

		private ToolStripMenuItem Menu_MainTools;

		private ToolStripMenuItem Menu_MainHelp;

		private ToolStripMenuItem Menu_WorshipSessions;

		private ToolStripMenuItem Menu_PraiseBookTemplates;

		private ToolStripSeparator toolStripSeparator18;

		private ToolStripMenuItem Menu_Exit;

		private ToolStripMenuItem Menu_AddSong;

		private ToolStripSeparator toolStripSeparator19;

		private ToolStripMenuItem Menu_EditSong;

		private ToolStripMenuItem Menu_CopySong;

		private ToolStripMenuItem Menu_MoveSong;

		private ToolStripMenuItem Menu_DeleteSong;

		private ToolStripMenuItem Menu_SelectAll;

		private ToolStripMenuItem Menu_Find;

		private ToolStripSeparator toolStripSeparator21;

		private ToolStripMenuItem Menu_ReArrangeSongFolders;

		private ToolStripMenuItem Menu_EasiSlidesFolder;

		private ToolStripMenuItem Menu_Refresh;

		private ToolStripMenuItem Menu_StatusBar;

		private ToolStripSeparator toolStripSeparator23;

		private ToolStripMenuItem Menu_Options;

		private ToolTip toolTip1;

		private TabControl TabBibleVersions;

		private TabPage tabPage1;

		private System.Windows.Forms.Timer TimerFlasher;

		private ListView PreviewPanelDisplayName;

		private ListView OutputPanelDisplayName;

		private ColumnHeader columnHeader15;

		private ColumnHeader columnHeader16;

		private ContextMenuStrip CMenuBible;

		private ToolStripMenuItem CMenuBible_SelectAll;

		private ToolStripMenuItem CMenuBible_UnselectAll;

		private ToolStripMenuItem CMenuBible_AddShow;

		private ToolStripMenuItem CMenuBible_AddRegion2;

		private ToolStripSeparator toolStripSeparator17;

		private ToolStripSeparator toolStripSeparator24;

		private ToolStripMenuItem CMenuBible_Copy;

		private ToolStripMenuItem CMenuBible_CopyInfoScreen;

		private ColumnHeader columnHeader17;

		private ColumnHeader columnHeader18;

		private ColumnHeader columnHeader19;

		private ColumnHeader columnHeader20;

		private ColumnHeader columnHeader21;

		private ColumnHeader columnHeader22;

		private Button PreviewBtnItemDown;

		private Button PreviewBtnItemUp;

		private OpenFileDialog openFileDialog1;

		private ToolStripStatusLabel StatusBarPanel1;

		private ToolStripStatusLabel StatusBarPanel2;

		private ToolStripStatusLabel StatusBarPanel3;

		private ToolStripStatusLabel StatusBarPanel4;

		private ToolStripButton Def_AssignMedia;

		private ToolStripSeparator toolStripSeparator27;

		private ToolStripMenuItem Menu_StartShow;

		private ToolStripSeparator toolStripSeparator28;

		private ToolStripMenuItem Menu_BlackScreen;

		private ToolStripMenuItem Menu_ClearScreen;

		private ToolStripSeparator toolStripSeparator29;

		private ToolStripMenuItem Menu_AlertWindow;

		private ToolStripMenuItem Menu_StopAlert;

		private ToolStripSeparator toolStripSeparator30;

		private ToolStripMenuItem Menu_SwitchChinese;

		private System.Windows.Forms.Timer TimerReMax;

		private ToolStripMenuItem Menu_Contents;

		private ToolStripMenuItem Menu_HelpWeb;

		private ToolStripSeparator toolStripSeparator31;

		private ToolStripMenuItem Menu_Register;

		private ToolStripMenuItem Menu_About;

		private ToolStripMenuItem Menu_Import;

		private ToolStripMenuItem Menu_Export;

		private ToolStripSeparator toolStripSeparator32;

		private ToolStripMenuItem Menu_Recover;

		private ToolStripMenuItem Menu_Empty;

		private ToolStripSeparator toolStripSeparator33;

		private ToolStripMenuItem Menu_AddToUsages;

		private ToolStripMenuItem Menu_ViewUsages;

		private ToolStripSeparator toolStripSeparator34;

		private ToolStripMenuItem Menu_SmartMerge;

		private ToolStripMenuItem Menu_Compact;

		private System.Windows.Forms.Timer TimerSearch;

		private ContextMenuStrip CMenuSongs;

		private ToolStripMenuItem CMenuSongs_SelectAll;

		private ToolStripMenuItem CMenuSongs_UnselectAll;

		private ToolStripMenuItem CMenuSongs_AddShow;

		private ToolStripMenuItem CMenuSongs_Edit;

		private ToolStripMenuItem CMenuSongs_Refresh;

		private ContextMenuStrip CMenuImages;

		private ToolStripMenuItem CMenuImages_AddItem;

		private ToolStripMenuItem CMenuImages_AddDefault;

		private ToolStripSeparator toolStripSeparator35;

		private ToolStripMenuItem CMenuImages_Refresh;

		private ContextMenuStrip CMenuPraiseB;

		private ToolStripMenuItem CMenuPraiseB_SelectAll;

		private ToolStripMenuItem CMenuPraiseB_UnselectAll;

		private ToolStripMenuItem CMenuPraiseB_Clear;

		private ToolStripMenuItem CMenuPraiseB_Edit;

		private ToolStripSeparator toolStripSeparator36;

		private ContextMenuStrip CMenuWorship;

		private ToolStripMenuItem CMenuWorship_SelectAll;

		private ToolStripMenuItem CMenuWorship_UnselectAll;

		private ToolStripMenuItem CMenuWorship_Clear;

		private ToolStripMenuItem CMenuWorship_Edit;

		private ToolStripMenuItem CMenuWorship_Play;

		private ToolStripMenuItem CMenuWorship_AddUsages;

		private ToolStripSeparator toolStripSeparator37;

		private ToolStripSeparator toolStripSeparator38;

		private ToolStripSeparator toolStripSeparator39;

		private System.Windows.Forms.Timer TimerMessagingWindowOpen;

		private ToolStripMenuItem Menu_EditHistoryList;

		private ToolStripSeparator toolStripSeparator41;

		private ToolStripMenuItem Menu_UseSongNumbering;

		private ToolStripSeparator toolStripSeparator20;

		private TabPage tabFiles;

		private ListView InfoScreenList;

		private ColumnHeader columnHeader23;

		private ColumnHeader columnHeader24;

		private ColumnHeader columnHeader25;

		private ColumnHeader columnHeader26;

		private ColumnHeader columnHeader27;

		private ColumnHeader columnHeader28;

		private ColumnHeader columnHeader29;

		private Panel panelExternalFiles1;

		private ToolStrip toolStripPowerpoint1;

		private ToolStripButton PP_OpenFolder;

		private ContextMenuStrip CMenuFiles;

		private ToolStripMenuItem CMenuFiles_SelectAll;

		private ToolStripMenuItem CMenuFiles_UnselectAll;

		private ToolStripMenuItem CMenuFiles_AddShow;

		private ToolStripMenuItem CMenuFiles_Edit;

		private ToolStripMenuItem CMenuFiles_Refresh;

		private ToolStripSeparator toolStripSeparator22;

		private RadioButton IndradioButtonInfo;

		private RichTextBox PreviewInfo;

		private ToolStripDropDownButton Def_Head;

		private ToolStripMenuItem Def_HeadNoTitles;

		private ToolStripMenuItem Def_HeadAllTitles;

		private ToolStripMenuItem Def_HeadFirstScreen;

		private ToolStripDropDownButton Ind_Head;

		private ToolStripMenuItem Ind_HeadNoTitles;

		private ToolStripMenuItem Ind_HeadAllTitles;

		private ToolStripMenuItem Ind_HeadFirstScreen;

		private ComboBox InfoScreenFolder;

		private Panel panelExternalFiles;

		private Panel panelImagesTop;

		private FlowLayoutPanel flowLayoutImages;

		private FlowLayoutPanel flowLayoutPreviewPowerPoint;

		private FlowLayoutPanel flowLayoutOutputPowerPoint;

		private FlowLayoutPanel flowLayoutExternalPowerPoint;

		private ToolStripButton Def_PanelPrevNext;

		private ToolStripDropDownButton Main_RotateStyle;

		private ToolStripMenuItem Main_Rotate1;

		private ToolStripMenuItem Main_Rotate2;

		private ToolStripDropDownButton Ind_R2Italics;

		private ToolStripDropDownButton Ind_R1Italics;

		private ToolStripMenuItem Ind_R1Italics0;

		private ToolStripMenuItem Ind_R1Italics1;

		private ToolStripMenuItem Ind_R1Italics2;

		private ToolStripMenuItem Ind_R2Italics0;

		private ToolStripMenuItem Ind_R2Italics1;

		private ToolStripMenuItem Ind_R2Italics2;

		private Button btnToLive;

		private FlowLayoutPanel flowLayoutPanel1;

		private Button PreviewBtnVerse1;

		private Button PreviewBtnVersePreChorus2;

		private Button PreviewBtnVersePreChorus;

		private Button PreviewBtnVerse9;

		private Button PreviewBtnVerse8;

		private Button PreviewBtnVerse7;

		private Button PreviewBtnVerse6;

		private Button PreviewBtnVerse5;

		private Button PreviewBtnVerse4;

		private Button PreviewBtnVerse3;

		private Button PreviewBtnVerse2;

		private Button PreviewBtnVerseChorus;

		private Button PreviewBtnVerseChorus2;

		private Button PreviewBtnVerseBridge;

		private Button PreviewBtnVerseEnding;

		private FlowLayoutPanel flowLayoutPanel2;

		private Button OutputBtnVerse1;

		private Button OutputBtnVerse2;

		private Button OutputBtnVerse3;

		private Button OutputBtnVerse4;

		private Button OutputBtnVerse5;

		private Button OutputBtnVerse6;

		private Button OutputBtnVerse7;

		private Button OutputBtnVerse8;

		private Button OutputBtnVerse9;

		private Button OutputBtnVersePreChorus;

		private Button OutputBtnVersePreChorus2;

		private Button OutputBtnVerseChorus;

		private Button OutputBtnVerseChorus2;

		private Button OutputBtnVerseBridge;

		private Button OutputBtnVerseEnding;

		private ToolStripMenuItem CMenuSongs_Copy;

		private ToolStripSeparator toolStripSeparator10;

		private ToolStripDropDownButton PP_ListType;

		private ToolStripMenuItem PP_ListStyle;

		private ToolStripMenuItem PP_PreviewStyle;

		private ToolStripMenuItem Menu_ListingOfSelectedFolder;

		private ToolStripSeparator toolStripSeparator16;

		private ToolStripSeparator toolStripSeparator12;

		private ToolStripMenuItem CMenuFiles_Copy;

		private ToolStripSeparator toolStripSeparator25;

		private System.Windows.Forms.Timer TimerToFront;

		private Button PreviewBtnVerseBridge2;

		private Button OutputBtnVerseBridge2;

		private ToolStripMenuItem Menu_GoLiveWithPreview;

		private ToolStripDropDownButton Def_HeadAlign;

		private ToolStripSeparator toolStripSeparator26;

		private ToolStripMenuItem Def_HeadAlignAsR1;

		private ToolStripMenuItem Def_HeadAlignAsR2;

		private ToolStripMenuItem Def_HeadAlignLeft;

		private ToolStripMenuItem Def_HeadAlignCentre;

		private ToolStripMenuItem Def_HeadAlignRight;

		private Panel panel4;

		private ToolStrip toolStrip1;

		private ToolStripDropDownButton Ind_HeadAlign;

		private ToolStripMenuItem Ind_HeadAlignAsR1;

		private ToolStripMenuItem Ind_HeadAlignAsR2;

		private ToolStripMenuItem Ind_HeadAlignLeft;

		private ToolStripButton Ind_CapoDown;

		private ToolStripButton Ind_CapoUp;

		private ToolStripMenuItem Ind_HeadAlignCentre;

		private ToolStripMenuItem Ind_HeadAlignRight;

		private Button btnToOutputMoveNext;

		private Panel panelDefTemplate;

		private ToolStrip toolStripDefTemplates;

		private ToolStripButton Def_SaveTemplate;

		private ToolStripButton Def_LoadTemplate;

		private SaveFileDialog saveFileDialog1;

		private Panel panelIndTemplate;

		private ToolStrip toolStripIndTemplates;

		private ToolStripButton Ind_LoadTemplate;

		private ToolStripButton Ind_SaveTemplate;

		private NumericUpDown Def_PanelHeight;

		private Panel panel21;

		private ToolStrip toolStripDef7;

		private ToolStripButton Def_PanelFontBold;

		private ToolStripButton Def_PanelFontItalics;

		private ToolStripButton Def_PanelFontUnderline;

		private ToolStripButton Def_PanelFontShadow;

		private ToolStripButton Def_PanelFontOutline;

		private ToolStripComboBox Def_PanelFontList;

		private TabPage tabPowerpoint;

		private ListView PowerpointList;

		private ColumnHeader columnHeader30;

		private ColumnHeader columnHeader31;

		private ColumnHeader columnHeader32;

		private ColumnHeader columnHeader33;

		private ColumnHeader columnHeader34;

		private ColumnHeader columnHeader35;

		private ColumnHeader columnHeader36;

		private Panel panelPowerpoint1;

		private ComboBox PowerpointFolder;

		private Panel panelInfoScreen1;

		private ToolStrip InfoScreentoolstrip1;

		private ToolStripButton InfoScreen_OpenFolder;

		private TabPage tabMedia;

		private Panel panel11;

		private Panel panelMedia1;

		private ToolStrip toolStripMedia1;

		private ToolStripButton Media_OpenFolder;

		private ComboBox MediaFolder;

		private ListView MediaList;

		private ColumnHeader columnHeader37;

		private ColumnHeader columnHeader38;

		private ColumnHeader columnHeader39;

		private ColumnHeader columnHeader40;

		private ColumnHeader columnHeader41;

		private ColumnHeader columnHeader42;

		private ColumnHeader columnHeader43;

		private Panel panelImage1;

		private ToolStrip toolStripImage1;

		private ToolStripButton Image_OpenFolder;

		private ToolStripButton Image_Import;

		private ToolStripButton PP_Import;

		private ToolStripButton Media_Import;

		private ToolStripButton InfoScreen_Import;

		private Panel panelInfoScreen2;

		private ToolStrip InfoScreentoolstrip2;

		private ToolStripButton InfoScreen_New;

		private ToolStripButton InfoScreen_Edit;

		private ToolStripButton InfoScreen_Copy;

		private ToolStripButton InfoScreen_Move;

		private ToolStripButton InfoScreen_Delete;

		private Panel panelPowerpoint2;

		private ToolStrip toolStripPowerpoint2;

		private ToolStripButton Powerpoint_Edit;

		private ToolStripButton Powerpoint_Copy;

		private ToolStripButton Powerpoint_Move;

		private ToolStripButton Powerpoint_Delete;

		private ToolStripButton Main_JumpA;

		private ToolStripButton Main_JumpB;

		private ToolStripButton Main_JumpC;

		private ToolStripMenuItem Menu_ClearAllFormatting;

		private RichTextBox PreviewNotes;

		private CheckBox IndcbPreviewNotes;

		private Panel panelPreviewSessionNotes2;

		private ToolStripButton WL_Notes;

		private Label labelHideText;

		private Label labelBlackScreen;

		private ToolStripMenuItem Menu_RefreshOutput;

		private CheckBox cbOutputCam;

		private ToolStripMenuItem Menu_LiveCam;

		private Label label5;

		private Panel flowLayoutPreviewLyrics;

		private Panel flowLayoutOutputLyrics;

		private Label labelGapItem;

		private Button OutputBtnRefAlert;

		private ToolStripSeparator toolStripSeparator5;

		private ToolStripButton Ind_HideDisplayPanel;

		private Panel panelOutputLM1;

		private Button OutputBtnLMSend;

		private Button OutputBtnLMClear;

		private Panel panelOutputLM3;

		private Panel panelOutputLM2;

		private ToolStripMenuItem Menu_PreviewNotations;

		private TextBox OutputTextBoxLM;

		private ToolStripSeparator toolStripSeparator8;

		private TextBox OutputInfo;

		private ToolStripButton Main_NoRotate;

		private ToolStripMenuItem Menu_ClearRegistrySettings;

		private ToolStripSeparator toolStripSeparator9;

		private ToolStripMenuItem Menu_ImportFolder;

		private ToolStripMenuItem Main_Rotate0;

		private ToolStripMenuItem Main_Rotate3;

		private Button OutputBtnJumpToNonRotate;

		private Button OutputBtnMedia;
		private ToolStripMenuItem Menu_RestartCurrentItem;

		public FrmMain()
		{
			InitializeComponent();

			FrmMain_Init();
		}

		private void FrmMain_Init()
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

		private void FrmMain_Load(object sender, EventArgs e)
		{
			gf.WMP_Present = DShowPlayerPresent();
			if (WorshipListItems.Items.Count > 0)
			{
				WorshipListIndexChanged(0, GetFirstItem: true);
			}
			LiveShow.OnMessage += LiveShow_OnMessage;

			MainPPT.Init();
			TimerToFront.Start();
			gf.SplashScreenCanClose = true;
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

		private void FrmMain_Resize(object sender, EventArgs e)
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
		/// ������Ʈ������ �ػ� ���� ������ ���� �߰�.
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

			int num6 = ((num5 <= 0) ? 1 : num5) * 3 / 4;

			if (num6 > InPanelContainer.Height - (num2 + num3 + num4))
			{
				num6 = InPanelContainer.Height - (num2 + num3 + num4);
				num6 = ((num6 <= 0) ? 1 : num6);
				// daniel
				// �̸����� ������ ���̵�� ����
				// num5 = num6 * 4 / 3;

				if (gf.isScreenWideMode)
					num5 = num6 * 5 / 3;
				else
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
		/// ������ ResizeSampleScreen version 1
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
				// �̸����� ������ ���̵�� ����
				// num5 = num6 * 4 / 3;

				if (gf.isScreenWideMode)
					num5 = num6 * 5 / 3;
				else
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
		/// ���� ResizeSampleScreen
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

		private void FrmMain_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (!ForceFormClose)
			{
				try
				{
					if (SaveToRegistryOnClosing)
					{
						SaveFormStateToRegistry();
					}
					GfFileHelpers.DeleteFolderFilesSafe(gf.EasiSlidesTempDir);
					//daniel
					//���α׷� ����� �Ŀ�����Ʈ ������ ����
					gf.ClearUpPowerpointWindows();
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
\t\t\tgf.Temp_MediaOutputMonitorName = gf.MediaOutputMonitorName;
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
\t\t\tgf.MediaOutputMonitorName = gf.Temp_MediaOutputMonitorName;
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
\t\t\tgf.Temp_MediaOutputMonitorName = gf.PreviewItem.Format.MediaOutputMonitorName;
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
\t\t\tgf.PreviewItem.Format.MediaOutputMonitorName = gf.Temp_MediaOutputMonitorName;
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
				// ���� ���� �ְ� �ִ� �����̵��� Ÿ��Ʋ�� ���� �ش�
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

		private void PowerPointImage_MouseUp(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				Control control = (Control)sender;
				if (control.Name == "PP_Preview")
				{
					gf.PreviewItem.CurSlide = DataUtil.ObjToInt(control.Tag) + 1;
					MoveToSlide(gf.PreviewItem, KeyDirection.Refresh);
				}
				else
				{
					gf.OutputItem.CurSlide = DataUtil.ObjToInt(control.Tag) + 1;
					MoveToSlide(gf.OutputItem, KeyDirection.Refresh);
				}
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
			///MDB Access������ Like �˻����� "*" �� ���
			//string text10 = "*";
			///SQLite������ Like �˻����� "%" �� ���
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
		/// daniel out �̹��� �ε�� ��ü�� �ٽ� �׸��� ����
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
			if (InCanvas != null)
			{
				Color backColor = InFlowPanel.BackColor;
				int num = (PanelWidth - 35) / 3;
				int newHeight = num * 3 / 4;
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
						try
						{
							InCanvas[CurSelectedSlide - 1].BuildNewImageThumbs(CurSelectedSlide - 1, num, newHeight, ref ImageName, TotalImagesCount, InPrefix, CurSelectedSlide, backColor, InToolTip, ExternalPP);
						}
						catch
						{ }
						try
						{
							InCanvas[previousOutSelectedSlide - 1].BuildNewImageThumbs(previousOutSelectedSlide - 1, num, newHeight, ref ImageName, TotalImagesCount, InPrefix, CurSelectedSlide, backColor, InToolTip, ExternalPP);
						}
						catch
						{ }

					}
				}
				try
				{
					InFlowPanel.ScrollControlIntoView(InCanvas[CurSelectedSlide - 1]);
				}
				catch
				{
				}

				System.Threading.Interlocked.Increment(ref LoadThumbOutlockkey);
				previousOutSelectedSlide = CurSelectedSlide;
			}
		}

		static int LoadThumbPreviewlockkey = 0;
		static int previousPreviewSelectedSlide = 1;
		/// <summary>
		/// daniel 4265 ���� ������
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
			if (InCanvas != null)
			{
				Color backColor = InFlowPanel.BackColor;
				int num = (PanelWidth - 35) / 3;
				int newHeight = num * 3 / 4;

				if (LoadThumbPreviewlockkey == 0)
				{
					for (int i = 0; i < TotalImagesCount; i++)
					{
						InCanvas[i].BuildNewImageThumbs(i, num, newHeight, ref ImageName, TotalImagesCount, InPrefix, CurSelectedSlide, backColor, InToolTip, ExternalPP);
					}
				}
				else
				{
					if (previousPreviewSelectedSlide != CurSelectedSlide)
					{
						InCanvas[CurSelectedSlide - 1].BuildNewImageThumbs(CurSelectedSlide - 1, num, newHeight, ref ImageName, TotalImagesCount, InPrefix, CurSelectedSlide, backColor, InToolTip, ExternalPP);
						InCanvas[previousPreviewSelectedSlide - 1].BuildNewImageThumbs(previousPreviewSelectedSlide - 1, num, newHeight, ref ImageName, TotalImagesCount, InPrefix, CurSelectedSlide, backColor, InToolTip, ExternalPP);
					}
				}
				try
				{
					InFlowPanel.ScrollControlIntoView(InCanvas[CurSelectedSlide - 1]);
				}
				catch
				{
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
		/// daniel �� ��ư ���ý� (���� �ǵ�)
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
					//������ �޺��ڽ����� ���� ���ý� ����Ǿ� ���� ������ ǥ�� ��
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
				catch(Exception e)
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
					//SQLite LCase()  -> lower() �� ���� UCase() -> upper()
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
				//���� ���� ���θ� ���� üũ �ؾ���
				string InTitle = WorshipListItems.Items[num].SubItems[0].Text;
				string text = WorshipListItems.Items[num].SubItems[1].Text;
				gf.PreviewItem.InMainItemText = InTitle;
				gf.PreviewItem.InSubItemItem1Text = text;
				gf.PreviewItem.CurItemNo = num + 1;
				gf.PreviewItem.TotalItems = WorshipListItems.Items.Count;

				string filePrefix = gf.SetPowerpointPreviewPrefix1(gf.PreviewItem);

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

				if (OutputItem != InItem.ItemID)
				{
					OutputItem = InItem.ItemID;
				}
				else
				{
					if (gf.OutputPPT.IsBuildedFileCheck(InItem.Path, filePrefix, ref InItem.TotalSlides))
						return;
				}

			}
			else
			{
				gf.PreviewPPT.preViewEvent = new OfficeLib.PreviewEvent(FormatPowerPointThumbContainers2);

				if (previwItem != InItem.ItemID)
				{
					previwItem = InItem.ItemID;
				}
				else
				{
					if (gf.PreviewPPT.IsBuildedFileCheck(InItem.Path, filePrefix, ref InItem.TotalSlides))
						return;
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
		//���� ���� ���θ� ���� üũ

		private void WorshipListItems_DoubleClick(object sender, EventArgs e)
		{
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
				BibleUserLookup_ShowVerses();
			}
		}

		private void BibleUserLookup_KeyUp(object sender, KeyEventArgs e)
		{
			if ((e.KeyCode == Keys.Return) | (e.KeyCode == Keys.Return))
			{
				BibleUserLookup_ShowVerses();
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
			if (cbOutputCam.Checked)
			{
				cbOutputCam.ImageIndex = (gf.TimerFlashOn ? 31 : 30);
			}
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
					MoveToSlide(InItem, KeyDirection.PrevOne);
					return;
				case Keys.Left:
					MoveToSlide(InItem, KeyDirection.FirstOne);
					return;
				case Keys.Right:
					MoveToSlide(InItem, KeyDirection.LastOne);
					return;
				case Keys.Down:
					MoveToSlide(InItem, KeyDirection.NextOne);
					return;
				case Keys.Space:
					MoveToSlide(InItem, KeyDirection.NextOne);
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
		/// Ȯ���� docx �߰�
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
			openFileDialog1.Filter = "Valid External Files (*.ppt,*.pptx,*.doc,*.docx,*.txt,*.esi,*.esw)|*.ppt;*.pptx;*.doc;*.docx;*.txt;*.esi;*.esw|Powerpoint Files (*.ppt)|*.ppt|Powerpoint Files (*.pptx)|*.pptx|Word Documents (*.doc)|*.doc|Word Documents (*.docx)|*.docx|Text Files (*.txt)|*.txt|InfoScreens (*.esi)|*.esi|Worship Lists (*.esw)|*.esw";
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
		/// Ȯ���� docx �߰�
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
				if(previousOutputPPImageName != gf.OUTPPFullPath + Convert.ToString(1) + ".jpg")
                {
					isCurrentOutputPPImageSame = false;
				}
				else
                {
					isCurrentOutputPPImageSame = true;
				}
			}

			if(!isCurrentOutputPPImageSame)
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
		/// Ȯ���� docx �߰�
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

						if (((strExt != ".ppt") & (strExt != ".doc") & (strExt != ".txt")) | ((strExt == ".ppt") & gf.ShowRunning))
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
				gf.SizeLaunchScreen();
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
				LiveCam(gf.ShowLiveCam);
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
				gf.SizeLaunchScreen();
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
				LiveCam(gf.ShowLiveCam);
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

		private void MinAllWindows()
		{
			Type typeFromProgID = Type.GetTypeFromProgID("Shell.Application");
			object target = Activator.CreateInstance(typeFromProgID);
			typeFromProgID.InvokeMember("MinimizeAll", BindingFlags.InvokeMethod, null, target, null);
		}

		private void LiveBlack(bool InStatus)
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

		private void LiveCam(bool InStatus)
		{
			gf.ShowLiveCam = InStatus;
			cbOutputCam.Checked = gf.ShowLiveCam;
			Menu_LiveCam.Checked = gf.ShowLiveCam;
			cbOutputCam.ImageIndex = (cbOutputCam.Checked ? 31 : 30);
			if (gf.ShowRunning)
			{
				RemoteControlLiveShow(LiveShowAction.Remote_LiveCamStartStop);
			}
			FocusOutputArea();
		}

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

		private void Menu_BlackScreen_Click(object sender, EventArgs e)
		{
			LiveBlack(!gf.ShowLiveBlack);
		}

		private void Menu_ClearScreen_Click(object sender, EventArgs e)
		{
			LiveClear(!gf.ShowLiveClear);
		}

		private void Menu_LiveCam_Click(object sender, EventArgs e)
		{
			LiveCam(!gf.ShowLiveCam);
		}

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

		private void cbOutputCam_Click(object sender, EventArgs e)
		{
			LiveCam(cbOutputCam.Checked);
		}

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
			if (gf.ShowRunning && gf.ShowLiveCam)
			{
				RemoteControlLiveShow(LiveShowAction.Remote_LiveCamUpdate);
			}
			SetTabsVisibility();
			SetJumpToolTips();
			ShowStatusBarSummary();
		}

		private void SetJumpToolTips()
		{
			int iLen = 15;
			string text = "FOLDER ";
			string value = "FOLDER";
			string text2 = gf.FolderName[gf.JumpToA]??="";
			string text3 = gf.FolderName[gf.JumpToB]??="";
			string text4 = gf.FolderName[gf.JumpToC]??="";
			string text5 = DataUtil.Trim(DataUtil.Left(text2, iLen));
			string text6 = DataUtil.Trim(DataUtil.Left(text3, iLen));
			string text7 = DataUtil.Trim(DataUtil.Left(text4, iLen));
			if (DataUtil.Left(gf.FolderName[gf.JumpToA]??="".ToUpper(), text.Length) == text)
			{
				text5 = DataUtil.Mid(text2, text.Length, iLen);
			}
			if (DataUtil.Left(gf.FolderName[gf.JumpToB]??="".ToUpper(), text.Length) == text)
			{
				text6 = DataUtil.Mid(text3, text.Length, iLen);
			}
			if (DataUtil.Left(gf.FolderName[gf.JumpToC]??="".ToUpper(), text.Length) == text)
			{
				text7 = DataUtil.Mid(text4, text.Length, iLen);
			}
			if (text2.ToUpper().IndexOf(value) < 0)
			{
				text2 = gf.FolderName[gf.JumpToA]??="" + " Folder";
			}
			if (text3.ToUpper().IndexOf(value) < 0)
			{
				text3 = gf.FolderName[gf.JumpToB]??="" + " Folder";
			}
			if (text4.ToUpper().IndexOf(value) < 0)
			{
				text4 = gf.FolderName[gf.JumpToC]??="" + " Folder";
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
			SongsListPlay();
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
			WorshipListItems.Items.Clear();
			WorshipListIndexChanged();
			SaveWorshipList();
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
			string inString = DataUtil.Right(text, text.Length - 1);
			int inKey = DataUtil.StringToInt(inString);
			if (DataUtil.Left(text, 1) == "D")
			{
				title = gf.LookupDBTitle2(inKey);
			}
			gf.Play_Media(title2, title);
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
			bool flag2 = false;
			if (!gf.ValidateDB(DatabaseType.Usages))
			{
				return;
			}
            try
            {
#if OleDb
				using OleDbConnection connection = DbConnectionController.GetOleDbConnection(gf.ConnectStringMainDB);
#elif SQLite
                using DbConnection connection  = DbController.GetDbConnection(gf.ConnectStringMainDB);
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
			catch
			{
				flag2 = true;
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
					if (!flag2)
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
		/// Ȯ���� .docx �߰�
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
				using StreamWriter streamWriter = new StreamWriter(OutputFileName, append: false, Encoding.Default);
				try
				{
					streamWriter.AutoFlush = true;

					gf.RTFNewLine = "\\b0\\i0\\ulnone\\par ";
					gf.RTFIndent[0] = "\\pard\\tx1200\\tx3500\\tx8200\\tx9000 ";
					string value = "{\\rtf1\\ansi\\ansicpg1252\\deff0\\deflang1033{\\fonttbl{\\f0\\fnil\\fcharset0 Microsoft Sans Serif;}}\\viewkind1\\uc1\\pard\\f0\\fs24\\margr600\\margl1000\\margt900\\margb1000\\cols2\\colno1\\colw4650\\colsr750\\colno2\\colw4650 ";
					string text = "";
					int num = -1;

					/// daniel  ����ϴ� ���� ��� ����
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
					case LiveShowAction.Remote_LiveCamStartStop:
						LiveShow.Remote_LiveCamStartStop();
						break;
					case LiveShowAction.Remote_LiveCamUpdate:
						LiveShow.Remote_LiveCamUpdate();
						break;
					case LiveShowAction.Remote_RefreshMediaWindow:
						LiveShow.Remote_RefreshMediaWindow();
						break;
					case LiveShowAction.Remote_RotateOnOffChanged:
						LiveShow.Remote_RotateOnOffChanged();
						break;
					case LiveShowAction.Remote_GetMediaTimings:
						return LiveShow.Remote_GetMediaTimings();
					case LiveShowAction.Remote_MediaItemPausePlay:
						return LiveShow.Remote_MediaItemPausePlay();
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

		protected override void Dispose(bool disposing)
		{
			if (disposing && components != null)
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		private void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmMain));
            this.toolStripContainerMain = new System.Windows.Forms.ToolStripContainer();
            this.splitContainerMain = new System.Windows.Forms.SplitContainer();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.tabControlSource = new System.Windows.Forms.TabControl();
            this.tabFolders = new System.Windows.Forms.TabPage();
            this.panelFolders = new System.Windows.Forms.Panel();
            this.toolStripFolders = new System.Windows.Forms.ToolStrip();
            this.Folders_WordCount = new System.Windows.Forms.ToolStripButton();
            this.SongsList = new System.Windows.Forms.ListView();
            this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader3 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader4 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader5 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader6 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader7 = new System.Windows.Forms.ColumnHeader();
            this.CMenuSongs = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.CMenuSongs_SelectAll = new System.Windows.Forms.ToolStripMenuItem();
            this.CMenuSongs_UnselectAll = new System.Windows.Forms.ToolStripMenuItem();
            this.CMenuSongs_AddShow = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator38 = new System.Windows.Forms.ToolStripSeparator();
            this.CMenuSongs_Edit = new System.Windows.Forms.ToolStripMenuItem();
            this.CMenuSongs_Copy = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator10 = new System.Windows.Forms.ToolStripSeparator();
            this.CMenuSongs_Refresh = new System.Windows.Forms.ToolStripMenuItem();
            this.SongFolder = new System.Windows.Forms.ComboBox();
            this.tabFiles = new System.Windows.Forms.TabPage();
            this.panelInfoScreen2 = new System.Windows.Forms.Panel();
            this.InfoScreentoolstrip2 = new System.Windows.Forms.ToolStrip();
            this.InfoScreen_New = new System.Windows.Forms.ToolStripButton();
            this.InfoScreen_Edit = new System.Windows.Forms.ToolStripButton();
            this.InfoScreen_Copy = new System.Windows.Forms.ToolStripButton();
            this.InfoScreen_Move = new System.Windows.Forms.ToolStripButton();
            this.InfoScreen_Delete = new System.Windows.Forms.ToolStripButton();
            this.panelExternalFiles = new System.Windows.Forms.Panel();
            this.panelInfoScreen1 = new System.Windows.Forms.Panel();
            this.InfoScreentoolstrip1 = new System.Windows.Forms.ToolStrip();
            this.InfoScreen_OpenFolder = new System.Windows.Forms.ToolStripButton();
            this.InfoScreen_Import = new System.Windows.Forms.ToolStripButton();
            this.InfoScreenFolder = new System.Windows.Forms.ComboBox();
            this.InfoScreenList = new System.Windows.Forms.ListView();
            this.columnHeader23 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader24 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader25 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader26 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader27 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader28 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader29 = new System.Windows.Forms.ColumnHeader();
            this.CMenuFiles = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.CMenuFiles_SelectAll = new System.Windows.Forms.ToolStripMenuItem();
            this.CMenuFiles_UnselectAll = new System.Windows.Forms.ToolStripMenuItem();
            this.CMenuFiles_AddShow = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator12 = new System.Windows.Forms.ToolStripSeparator();
            this.CMenuFiles_Edit = new System.Windows.Forms.ToolStripMenuItem();
            this.CMenuFiles_Copy = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator25 = new System.Windows.Forms.ToolStripSeparator();
            this.CMenuFiles_Refresh = new System.Windows.Forms.ToolStripMenuItem();
            this.tabPowerpoint = new System.Windows.Forms.TabPage();
            this.flowLayoutExternalPowerPoint = new System.Windows.Forms.FlowLayoutPanel();
            this.panelPowerpoint2 = new System.Windows.Forms.Panel();
            this.toolStripPowerpoint2 = new System.Windows.Forms.ToolStrip();
            this.Powerpoint_Edit = new System.Windows.Forms.ToolStripButton();
            this.Powerpoint_Copy = new System.Windows.Forms.ToolStripButton();
            this.Powerpoint_Move = new System.Windows.Forms.ToolStripButton();
            this.Powerpoint_Delete = new System.Windows.Forms.ToolStripButton();
            this.PowerpointList = new System.Windows.Forms.ListView();
            this.columnHeader30 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader31 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader32 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader33 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader34 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader35 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader36 = new System.Windows.Forms.ColumnHeader();
            this.panelPowerpoint1 = new System.Windows.Forms.Panel();
            this.PowerpointFolder = new System.Windows.Forms.ComboBox();
            this.panelExternalFiles1 = new System.Windows.Forms.Panel();
            this.toolStripPowerpoint1 = new System.Windows.Forms.ToolStrip();
            this.PP_ListType = new System.Windows.Forms.ToolStripDropDownButton();
            this.PP_ListStyle = new System.Windows.Forms.ToolStripMenuItem();
            this.PP_PreviewStyle = new System.Windows.Forms.ToolStripMenuItem();
            this.PP_OpenFolder = new System.Windows.Forms.ToolStripButton();
            this.PP_Import = new System.Windows.Forms.ToolStripButton();
            this.tabBibles = new System.Windows.Forms.TabPage();
            this.BibleText = new System.Windows.Forms.RichTextBox();
            this.CMenuBible = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.CMenuBible_SelectAll = new System.Windows.Forms.ToolStripMenuItem();
            this.CMenuBible_UnselectAll = new System.Windows.Forms.ToolStripMenuItem();
            this.CMenuBible_AddShow = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator17 = new System.Windows.Forms.ToolStripSeparator();
            this.CMenuBible_AddRegion2 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator24 = new System.Windows.Forms.ToolStripSeparator();
            this.CMenuBible_Copy = new System.Windows.Forms.ToolStripMenuItem();
            this.CMenuBible_CopyInfoScreen = new System.Windows.Forms.ToolStripMenuItem();
            this.BibleUserLookup = new System.Windows.Forms.TextBox();
            this.panelBible2 = new System.Windows.Forms.Panel();
            this.toolStripBible2 = new System.Windows.Forms.ToolStrip();
            this.Bibles_Go = new System.Windows.Forms.ToolStripButton();
            this.BookLookup = new System.Windows.Forms.ComboBox();
            this.TabBibleVersions = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tabImages = new System.Windows.Forms.TabPage();
            this.flowLayoutImages = new System.Windows.Forms.FlowLayoutPanel();
            this.panelImagesTop = new System.Windows.Forms.Panel();
            this.panelImage1 = new System.Windows.Forms.Panel();
            this.toolStripImage1 = new System.Windows.Forms.ToolStrip();
            this.Image_OpenFolder = new System.Windows.Forms.ToolStripButton();
            this.Image_Import = new System.Windows.Forms.ToolStripButton();
            this.ImagesFolder = new System.Windows.Forms.ComboBox();
            this.tabMedia = new System.Windows.Forms.TabPage();
            this.panel11 = new System.Windows.Forms.Panel();
            this.panelMedia1 = new System.Windows.Forms.Panel();
            this.toolStripMedia1 = new System.Windows.Forms.ToolStrip();
            this.Media_OpenFolder = new System.Windows.Forms.ToolStripButton();
            this.Media_Import = new System.Windows.Forms.ToolStripButton();
            this.MediaFolder = new System.Windows.Forms.ComboBox();
            this.MediaList = new System.Windows.Forms.ListView();
            this.columnHeader37 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader38 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader39 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader40 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader41 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader42 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader43 = new System.Windows.Forms.ColumnHeader();
            this.tabDefault = new System.Windows.Forms.TabPage();
            this.DefPanel = new System.Windows.Forms.Panel();
            this.panelDefTemplate = new System.Windows.Forms.Panel();
            this.toolStripDefTemplates = new System.Windows.Forms.ToolStrip();
            this.Def_LoadTemplate = new System.Windows.Forms.ToolStripButton();
            this.Def_SaveTemplate = new System.Windows.Forms.ToolStripButton();
            this.DefApplyDefaultsBtn = new System.Windows.Forms.Button();
            this.DefgroupBox2 = new System.Windows.Forms.GroupBox();
            this.panelDef4 = new System.Windows.Forms.Panel();
            this.toolStripDef4 = new System.Windows.Forms.ToolStrip();
            this.Def_TransItem = new System.Windows.Forms.ToolStripComboBox();
            this.Def_TransSlides = new System.Windows.Forms.ToolStripComboBox();
            this.panelDef3 = new System.Windows.Forms.Panel();
            this.toolStripDef3 = new System.Windows.Forms.ToolStrip();
            this.Def_ImageMode = new System.Windows.Forms.ToolStripDropDownButton();
            this.Def_ImageTile = new System.Windows.Forms.ToolStripMenuItem();
            this.Def_ImageCentre = new System.Windows.Forms.ToolStripMenuItem();
            this.Def_ImageBestFit = new System.Windows.Forms.ToolStripMenuItem();
            this.Def_NoImage = new System.Windows.Forms.ToolStripButton();
            this.Def_BackColour = new System.Windows.Forms.ToolStripButton();
            this.Def_AssignMedia = new System.Windows.Forms.ToolStripButton();
            this.DefgroupBox3 = new System.Windows.Forms.GroupBox();
            this.panel21 = new System.Windows.Forms.Panel();
            this.toolStripDef7 = new System.Windows.Forms.ToolStrip();
            this.Def_PanelFontBold = new System.Windows.Forms.ToolStripButton();
            this.Def_PanelFontItalics = new System.Windows.Forms.ToolStripButton();
            this.Def_PanelFontUnderline = new System.Windows.Forms.ToolStripButton();
            this.Def_PanelFontShadow = new System.Windows.Forms.ToolStripButton();
            this.Def_PanelFontOutline = new System.Windows.Forms.ToolStripButton();
            this.Def_PanelFontList = new System.Windows.Forms.ToolStripComboBox();
            this.Def_PanelHeight = new System.Windows.Forms.NumericUpDown();
            this.panelDef5 = new System.Windows.Forms.Panel();
            this.toolStripDef5 = new System.Windows.Forms.ToolStrip();
            this.Def_PanelAsR1 = new System.Windows.Forms.ToolStripButton();
            this.Def_PanelTextColour = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator14 = new System.Windows.Forms.ToolStripSeparator();
            this.Def_PanelTransparent = new System.Windows.Forms.ToolStripButton();
            this.Def_PanelBackColour = new System.Windows.Forms.ToolStripButton();
            this.panelDef6 = new System.Windows.Forms.Panel();
            this.toolStripDef6 = new System.Windows.Forms.ToolStrip();
            this.Def_PanelShow = new System.Windows.Forms.ToolStripButton();
            this.Def_PanelTitle = new System.Windows.Forms.ToolStripButton();
            this.Def_PanelCopyright = new System.Windows.Forms.ToolStripButton();
            this.Def_PanelSong = new System.Windows.Forms.ToolStripButton();
            this.Def_PanelSlides = new System.Windows.Forms.ToolStripButton();
            this.Def_PanelPrevNext = new System.Windows.Forms.ToolStripButton();
            this.label5 = new System.Windows.Forms.Label();
            this.DefgroupBox1 = new System.Windows.Forms.GroupBox();
            this.panelDef2 = new System.Windows.Forms.Panel();
            this.toolStripDef2 = new System.Windows.Forms.ToolStrip();
            this.Def_HeadAlign = new System.Windows.Forms.ToolStripDropDownButton();
            this.Def_HeadAlignAsR1 = new System.Windows.Forms.ToolStripMenuItem();
            this.Def_HeadAlignAsR2 = new System.Windows.Forms.ToolStripMenuItem();
            this.Def_HeadAlignLeft = new System.Windows.Forms.ToolStripMenuItem();
            this.Def_HeadAlignCentre = new System.Windows.Forms.ToolStripMenuItem();
            this.Def_HeadAlignRight = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator26 = new System.Windows.Forms.ToolStripSeparator();
            this.Def_R1Align = new System.Windows.Forms.ToolStripDropDownButton();
            this.Def_R1AlignLeft = new System.Windows.Forms.ToolStripMenuItem();
            this.Def_R1AlignCentre = new System.Windows.Forms.ToolStripMenuItem();
            this.Def_R1AlignRight = new System.Windows.Forms.ToolStripMenuItem();
            this.Def_R1Colour = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator8 = new System.Windows.Forms.ToolStripSeparator();
            this.Def_R2Align = new System.Windows.Forms.ToolStripDropDownButton();
            this.Def_R2AlignLeft = new System.Windows.Forms.ToolStripMenuItem();
            this.Def_R2AlignCentre = new System.Windows.Forms.ToolStripMenuItem();
            this.Def_R2AlignRight = new System.Windows.Forms.ToolStripMenuItem();
            this.Def_R2Colour = new System.Windows.Forms.ToolStripButton();
            this.panelDef1 = new System.Windows.Forms.Panel();
            this.toolStripDef1 = new System.Windows.Forms.ToolStrip();
            this.Def_Head = new System.Windows.Forms.ToolStripDropDownButton();
            this.Def_HeadNoTitles = new System.Windows.Forms.ToolStripMenuItem();
            this.Def_HeadAllTitles = new System.Windows.Forms.ToolStripMenuItem();
            this.Def_HeadFirstScreen = new System.Windows.Forms.ToolStripMenuItem();
            this.Def_Region = new System.Windows.Forms.ToolStripDropDownButton();
            this.Def_ShowRegion1 = new System.Windows.Forms.ToolStripMenuItem();
            this.Def_ShowRegion2 = new System.Windows.Forms.ToolStripMenuItem();
            this.Def_ShowRegionBoth = new System.Windows.Forms.ToolStripMenuItem();
            this.Def_VAlign = new System.Windows.Forms.ToolStripDropDownButton();
            this.Def_VAlignTop = new System.Windows.Forms.ToolStripMenuItem();
            this.Def_VAlignCentre = new System.Windows.Forms.ToolStripMenuItem();
            this.Def_VAlignBottom = new System.Windows.Forms.ToolStripMenuItem();
            this.Def_Shadow = new System.Windows.Forms.ToolStripButton();
            this.Def_Outline = new System.Windows.Forms.ToolStripButton();
            this.Def_Interlace = new System.Windows.Forms.ToolStripButton();
            this.Def_Notations = new System.Windows.Forms.ToolStripButton();
            this.Def_ToZero = new System.Windows.Forms.ToolStripButton();
            this.tabControlLists = new System.Windows.Forms.TabControl();
            this.tabWorshipList = new System.Windows.Forms.TabPage();
            this.panelWorshipList2 = new System.Windows.Forms.Panel();
            this.toolStripWorshipList2 = new System.Windows.Forms.ToolStrip();
            this.WL_Up = new System.Windows.Forms.ToolStripButton();
            this.WL_Down = new System.Windows.Forms.ToolStripButton();
            this.WL_Delete = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            this.WL_Word = new System.Windows.Forms.ToolStripButton();
            this.WL_Notes = new System.Windows.Forms.ToolStripButton();
            this.panelWorshipList1 = new System.Windows.Forms.Panel();
            this.toolStripWorshipList1 = new System.Windows.Forms.ToolStrip();
            this.WL_Manage = new System.Windows.Forms.ToolStripButton();
            this.WL_Add = new System.Windows.Forms.ToolStripButton();
            this.WL_Open = new System.Windows.Forms.ToolStripButton();
            this.WorshipListItems = new System.Windows.Forms.ListView();
            this.columnHeader8 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader9 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader10 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader11 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader12 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader13 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader14 = new System.Windows.Forms.ColumnHeader();
            this.CMenuWorship = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.CMenuWorship_SelectAll = new System.Windows.Forms.ToolStripMenuItem();
            this.CMenuWorship_UnselectAll = new System.Windows.Forms.ToolStripMenuItem();
            this.CMenuWorship_Clear = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator39 = new System.Windows.Forms.ToolStripSeparator();
            this.CMenuWorship_Edit = new System.Windows.Forms.ToolStripMenuItem();
            this.CMenuWorship_Play = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator37 = new System.Windows.Forms.ToolStripSeparator();
            this.CMenuWorship_AddUsages = new System.Windows.Forms.ToolStripMenuItem();
            this.imageListSys = new System.Windows.Forms.ImageList(this.components);
            this.SessionList = new System.Windows.Forms.ComboBox();
            this.tabPraiseBook = new System.Windows.Forms.TabPage();
            this.panelPraiseBook2 = new System.Windows.Forms.Panel();
            this.toolStripPraiseBook2 = new System.Windows.Forms.ToolStrip();
            this.toolStripSeparator22 = new System.Windows.Forms.ToolStripSeparator();
            this.PB_Delete = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
            this.PB_Word = new System.Windows.Forms.ToolStripButton();
            this.PB_Html = new System.Windows.Forms.ToolStripButton();
            this.panelPraiseBook1 = new System.Windows.Forms.Panel();
            this.toolStripPraiseBook1 = new System.Windows.Forms.ToolStrip();
            this.PB_Manage = new System.Windows.Forms.ToolStripButton();
            this.PB_Add = new System.Windows.Forms.ToolStripButton();
            this.PB_WordCount = new System.Windows.Forms.ToolStripButton();
            this.PraiseBookItems = new System.Windows.Forms.ListView();
            this.columnHeader17 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader18 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader19 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader20 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader21 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader22 = new System.Windows.Forms.ColumnHeader();
            this.CMenuPraiseB = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.CMenuPraiseB_SelectAll = new System.Windows.Forms.ToolStripMenuItem();
            this.CMenuPraiseB_UnselectAll = new System.Windows.Forms.ToolStripMenuItem();
            this.CMenuPraiseB_Clear = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator36 = new System.Windows.Forms.ToolStripSeparator();
            this.CMenuPraiseB_Edit = new System.Windows.Forms.ToolStripMenuItem();
            this.PraiseBook = new System.Windows.Forms.ComboBox();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.splitContainerPreview = new System.Windows.Forms.SplitContainer();
            this.panelPreviewTop = new System.Windows.Forms.Panel();
            this.flowLayoutPreviewPowerPoint = new System.Windows.Forms.FlowLayoutPanel();
            this.IndPanel = new System.Windows.Forms.Panel();
            this.panelIndTemplate = new System.Windows.Forms.Panel();
            this.toolStripIndTemplates = new System.Windows.Forms.ToolStrip();
            this.Ind_LoadTemplate = new System.Windows.Forms.ToolStripButton();
            this.Ind_SaveTemplate = new System.Windows.Forms.ToolStripButton();
            this.IndgroupBox4 = new System.Windows.Forms.GroupBox();
            this.panelInd7 = new System.Windows.Forms.Panel();
            this.toolStripInd7 = new System.Windows.Forms.ToolStrip();
            this.Ind_Reg2FontsList = new System.Windows.Forms.ToolStripComboBox();
            this.Ind_Reg2SizeUpDown = new System.Windows.Forms.NumericUpDown();
            this.label6 = new System.Windows.Forms.Label();
            this.Ind_Reg2TopUpDown = new System.Windows.Forms.NumericUpDown();
            this.panelInd6 = new System.Windows.Forms.Panel();
            this.toolStripInd6 = new System.Windows.Forms.ToolStrip();
            this.Ind_R2Bold = new System.Windows.Forms.ToolStripButton();
            this.Ind_R2Italics = new System.Windows.Forms.ToolStripDropDownButton();
            this.Ind_R2Italics0 = new System.Windows.Forms.ToolStripMenuItem();
            this.Ind_R2Italics1 = new System.Windows.Forms.ToolStripMenuItem();
            this.Ind_R2Italics2 = new System.Windows.Forms.ToolStripMenuItem();
            this.Ind_R2Underline = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator15 = new System.Windows.Forms.ToolStripSeparator();
            this.Ind_R2Align = new System.Windows.Forms.ToolStripDropDownButton();
            this.Ind_R2AlignLeft = new System.Windows.Forms.ToolStripMenuItem();
            this.Ind_R2AlignCentre = new System.Windows.Forms.ToolStripMenuItem();
            this.Ind_R2AlignRight = new System.Windows.Forms.ToolStripMenuItem();
            this.Ind_R2Colour = new System.Windows.Forms.ToolStripButton();
            this.label7 = new System.Windows.Forms.Label();
            this.IndgroupBox3 = new System.Windows.Forms.GroupBox();
            this.panelInd5 = new System.Windows.Forms.Panel();
            this.toolStripInd5 = new System.Windows.Forms.ToolStrip();
            this.Ind_Reg1FontsList = new System.Windows.Forms.ToolStripComboBox();
            this.Ind_Reg1SizeUpDown = new System.Windows.Forms.NumericUpDown();
            this.labelBlackScreenOn = new System.Windows.Forms.Label();
            this.Ind_Reg1TopUpDown = new System.Windows.Forms.NumericUpDown();
            this.panelInd4 = new System.Windows.Forms.Panel();
            this.toolStripInd4 = new System.Windows.Forms.ToolStrip();
            this.Ind_R1Bold = new System.Windows.Forms.ToolStripButton();
            this.Ind_R1Italics = new System.Windows.Forms.ToolStripDropDownButton();
            this.Ind_R1Italics0 = new System.Windows.Forms.ToolStripMenuItem();
            this.Ind_R1Italics1 = new System.Windows.Forms.ToolStripMenuItem();
            this.Ind_R1Italics2 = new System.Windows.Forms.ToolStripMenuItem();
            this.Ind_R1Underline = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator13 = new System.Windows.Forms.ToolStripSeparator();
            this.Ind_R1Align = new System.Windows.Forms.ToolStripDropDownButton();
            this.Ind_R1AlignLeft = new System.Windows.Forms.ToolStripMenuItem();
            this.Ind_R1AlignCentre = new System.Windows.Forms.ToolStripMenuItem();
            this.Ind_R1AlignRight = new System.Windows.Forms.ToolStripMenuItem();
            this.Ind_R1Colour = new System.Windows.Forms.ToolStripButton();
            this.label4 = new System.Windows.Forms.Label();
            this.IndgroupBox2 = new System.Windows.Forms.GroupBox();
            this.Ind_BottomUpDown = new System.Windows.Forms.NumericUpDown();
            this.panelInd3 = new System.Windows.Forms.Panel();
            this.toolStripInd3 = new System.Windows.Forms.ToolStrip();
            this.Ind_TransItem = new System.Windows.Forms.ToolStripComboBox();
            this.Ind_TransSlides = new System.Windows.Forms.ToolStripComboBox();
            this.Ind_RightUpDown = new System.Windows.Forms.NumericUpDown();
            this.panelInd2 = new System.Windows.Forms.Panel();
            this.toolStripInd2 = new System.Windows.Forms.ToolStrip();
            this.Ind_ImageMode = new System.Windows.Forms.ToolStripDropDownButton();
            this.Ind_ImageTile = new System.Windows.Forms.ToolStripMenuItem();
            this.Ind_ImageCentre = new System.Windows.Forms.ToolStripMenuItem();
            this.Ind_ImageBestFit = new System.Windows.Forms.ToolStripMenuItem();
            this.Ind_NoImage = new System.Windows.Forms.ToolStripButton();
            this.Ind_BackColour = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator27 = new System.Windows.Forms.ToolStripSeparator();
            this.Ind_AssignMedia = new System.Windows.Forms.ToolStripButton();
            this.label3 = new System.Windows.Forms.Label();
            this.Ind_LeftUpDown = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.IndgroupBox1 = new System.Windows.Forms.GroupBox();
            this.panel4 = new System.Windows.Forms.Panel();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.Ind_HeadAlign = new System.Windows.Forms.ToolStripDropDownButton();
            this.Ind_HeadAlignAsR1 = new System.Windows.Forms.ToolStripMenuItem();
            this.Ind_HeadAlignAsR2 = new System.Windows.Forms.ToolStripMenuItem();
            this.Ind_HeadAlignLeft = new System.Windows.Forms.ToolStripMenuItem();
            this.Ind_HeadAlignCentre = new System.Windows.Forms.ToolStripMenuItem();
            this.Ind_HeadAlignRight = new System.Windows.Forms.ToolStripMenuItem();
            this.Ind_CapoDown = new System.Windows.Forms.ToolStripButton();
            this.Ind_CapoUp = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.Ind_HideDisplayPanel = new System.Windows.Forms.ToolStripButton();
            this.panelInd1 = new System.Windows.Forms.Panel();
            this.toolStripInd1 = new System.Windows.Forms.ToolStrip();
            this.Ind_Head = new System.Windows.Forms.ToolStripDropDownButton();
            this.Ind_HeadNoTitles = new System.Windows.Forms.ToolStripMenuItem();
            this.Ind_HeadAllTitles = new System.Windows.Forms.ToolStripMenuItem();
            this.Ind_HeadFirstScreen = new System.Windows.Forms.ToolStripMenuItem();
            this.Ind_Region = new System.Windows.Forms.ToolStripDropDownButton();
            this.Ind_ShowRegion1 = new System.Windows.Forms.ToolStripMenuItem();
            this.Ind_ShowRegion2 = new System.Windows.Forms.ToolStripMenuItem();
            this.Ind_ShowRegionBoth = new System.Windows.Forms.ToolStripMenuItem();
            this.Ind_VAlign = new System.Windows.Forms.ToolStripDropDownButton();
            this.Ind_VAlignTop = new System.Windows.Forms.ToolStripMenuItem();
            this.Ind_VAlignCentre = new System.Windows.Forms.ToolStripMenuItem();
            this.Ind_VAlignBottom = new System.Windows.Forms.ToolStripMenuItem();
            this.Ind_Shadow = new System.Windows.Forms.ToolStripButton();
            this.Ind_Outline = new System.Windows.Forms.ToolStripButton();
            this.Ind_Interlace = new System.Windows.Forms.ToolStripButton();
            this.Ind_Notations = new System.Windows.Forms.ToolStripButton();
            this.Ind_checkBox = new System.Windows.Forms.CheckBox();
            this.PreviewInfo = new System.Windows.Forms.RichTextBox();
            this.flowLayoutPreviewLyrics = new System.Windows.Forms.Panel();
            this.panel9 = new System.Windows.Forms.Panel();
            this.btnToLive = new System.Windows.Forms.Button();
            this.btnToOutputMoveNext = new System.Windows.Forms.Button();
            this.PreviewPanelDisplayName = new System.Windows.Forms.ListView();
            this.columnHeader15 = new System.Windows.Forms.ColumnHeader();
            this.btnToOutput = new System.Windows.Forms.Button();
            this.panelPreviewBottom = new System.Windows.Forms.Panel();
            this.panelPreviewSessionNotes2 = new System.Windows.Forms.Panel();
            this.PreviewNotes = new System.Windows.Forms.RichTextBox();
            this.PreviewHolder = new System.Windows.Forms.Panel();
            this.PreviewBack = new System.Windows.Forms.Panel();
            this.panel7 = new System.Windows.Forms.Panel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.PreviewBtnVerse1 = new System.Windows.Forms.Button();
            this.PreviewBtnVerse2 = new System.Windows.Forms.Button();
            this.PreviewBtnVerse3 = new System.Windows.Forms.Button();
            this.PreviewBtnVerse4 = new System.Windows.Forms.Button();
            this.PreviewBtnVerse5 = new System.Windows.Forms.Button();
            this.PreviewBtnVerse6 = new System.Windows.Forms.Button();
            this.PreviewBtnVerse7 = new System.Windows.Forms.Button();
            this.PreviewBtnVerse8 = new System.Windows.Forms.Button();
            this.PreviewBtnVerse9 = new System.Windows.Forms.Button();
            this.PreviewBtnVersePreChorus = new System.Windows.Forms.Button();
            this.PreviewBtnVersePreChorus2 = new System.Windows.Forms.Button();
            this.PreviewBtnVerseChorus = new System.Windows.Forms.Button();
            this.PreviewBtnVerseChorus2 = new System.Windows.Forms.Button();
            this.PreviewBtnVerseBridge = new System.Windows.Forms.Button();
            this.PreviewBtnVerseBridge2 = new System.Windows.Forms.Button();
            this.PreviewBtnVerseEnding = new System.Windows.Forms.Button();
            this.panel3 = new System.Windows.Forms.Panel();
            this.PreviewBtnSlideDown = new System.Windows.Forms.Button();
            this.PreviewBtnSlideUp = new System.Windows.Forms.Button();
            this.PreviewBtnItemDown = new System.Windows.Forms.Button();
            this.PreviewBtnItemUp = new System.Windows.Forms.Button();
            this.IndradioButtonInfo = new System.Windows.Forms.RadioButton();
            this.IndradioButtonFormat = new System.Windows.Forms.RadioButton();
            this.IndradioButtonText = new System.Windows.Forms.RadioButton();
            this.IndcbPreviewNotes = new System.Windows.Forms.CheckBox();
            this.splitContainerOutput = new System.Windows.Forms.SplitContainer();
            this.panelOutputTop = new System.Windows.Forms.Panel();
            this.flowLayoutOutputPowerPoint = new System.Windows.Forms.FlowLayoutPanel();
            this.flowLayoutOutputLyrics = new System.Windows.Forms.Panel();
            this.OutputInfo = new System.Windows.Forms.TextBox();
            this.panel10 = new System.Windows.Forms.Panel();
            this.cbOutputBlack = new System.Windows.Forms.CheckBox();
            this.cbOutputClear = new System.Windows.Forms.CheckBox();
            this.cbOutputCam = new System.Windows.Forms.CheckBox();
            this.OutputPanelDisplayName = new System.Windows.Forms.ListView();
            this.columnHeader16 = new System.Windows.Forms.ColumnHeader();
            this.cbGoLive = new System.Windows.Forms.CheckBox();
            this.panelOutputBottom = new System.Windows.Forms.Panel();
            this.panelOutputLM1 = new System.Windows.Forms.Panel();
            this.OutputTextBoxLM = new System.Windows.Forms.TextBox();
            this.panelOutputLM2 = new System.Windows.Forms.Panel();
            this.panelOutputLM3 = new System.Windows.Forms.Panel();
            this.OutputBtnLMSend = new System.Windows.Forms.Button();
            this.OutputBtnLMClear = new System.Windows.Forms.Button();
            this.OutputHolder = new System.Windows.Forms.Panel();
            this.OutputBack = new System.Windows.Forms.Panel();
            this.panel8 = new System.Windows.Forms.Panel();
            this.labelGapItem = new System.Windows.Forms.Label();
            this.labelHideText = new System.Windows.Forms.Label();
            this.labelBlackScreen = new System.Windows.Forms.Label();
            this.panel2 = new System.Windows.Forms.Panel();
            this.flowLayoutPanel2 = new System.Windows.Forms.FlowLayoutPanel();
            this.OutputBtnVerse1 = new System.Windows.Forms.Button();
            this.OutputBtnVerse2 = new System.Windows.Forms.Button();
            this.OutputBtnVerse3 = new System.Windows.Forms.Button();
            this.OutputBtnVerse4 = new System.Windows.Forms.Button();
            this.OutputBtnVerse5 = new System.Windows.Forms.Button();
            this.OutputBtnVerse6 = new System.Windows.Forms.Button();
            this.OutputBtnVerse7 = new System.Windows.Forms.Button();
            this.OutputBtnVerse8 = new System.Windows.Forms.Button();
            this.OutputBtnVerse9 = new System.Windows.Forms.Button();
            this.OutputBtnVersePreChorus = new System.Windows.Forms.Button();
            this.OutputBtnVersePreChorus2 = new System.Windows.Forms.Button();
            this.OutputBtnVerseChorus = new System.Windows.Forms.Button();
            this.OutputBtnVerseChorus2 = new System.Windows.Forms.Button();
            this.OutputBtnVerseBridge = new System.Windows.Forms.Button();
            this.OutputBtnVerseBridge2 = new System.Windows.Forms.Button();
            this.OutputBtnVerseEnding = new System.Windows.Forms.Button();
            this.panel6 = new System.Windows.Forms.Panel();
            this.OutputBtnSlideDown = new System.Windows.Forms.Button();
            this.OutputBtnSlideUp = new System.Windows.Forms.Button();
            this.OutputBtnItemDown = new System.Windows.Forms.Button();
            this.OutputBtnItemUp = new System.Windows.Forms.Button();
            this.OutputBtnRefAlert = new System.Windows.Forms.Button();
            this.OutputBtnMedia = new System.Windows.Forms.Button();
            this.OutputBtnJumpToNonRotate = new System.Windows.Forms.Button();
            this.toolStripMain = new System.Windows.Forms.ToolStrip();
            this.Main_New = new System.Windows.Forms.ToolStripButton();
            this.Main_Edit = new System.Windows.Forms.ToolStripButton();
            this.Main_Copy = new System.Windows.Forms.ToolStripButton();
            this.Main_Move = new System.Windows.Forms.ToolStripButton();
            this.Main_Delete = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.Main_Media = new System.Windows.Forms.ToolStripButton();
            this.Main_Refresh = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.Main_Options = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.Main_NoRotate = new System.Windows.Forms.ToolStripButton();
            this.Main_RotateStyle = new System.Windows.Forms.ToolStripDropDownButton();
            this.Main_Rotate0 = new System.Windows.Forms.ToolStripMenuItem();
            this.Main_Rotate1 = new System.Windows.Forms.ToolStripMenuItem();
            this.Main_Rotate2 = new System.Windows.Forms.ToolStripMenuItem();
            this.Main_Rotate3 = new System.Windows.Forms.ToolStripMenuItem();
            this.Main_Alerts = new System.Windows.Forms.ToolStripButton();
            this.Main_Chinese = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.Main_Find = new System.Windows.Forms.ToolStripButton();
            this.Main_QuickFind = new System.Windows.Forms.ToolStripComboBox();
            this.Main_JumpA = new System.Windows.Forms.ToolStripButton();
            this.Main_JumpB = new System.Windows.Forms.ToolStripButton();
            this.Main_JumpC = new System.Windows.Forms.ToolStripButton();
            this.menuStripMain = new System.Windows.Forms.MenuStrip();
            this.Menu_MainFile = new System.Windows.Forms.ToolStripMenuItem();
            this.Menu_WorshipSessions = new System.Windows.Forms.ToolStripMenuItem();
            this.Menu_PraiseBookTemplates = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator20 = new System.Windows.Forms.ToolStripSeparator();
            this.Menu_ListingOfSelectedFolder = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator16 = new System.Windows.Forms.ToolStripSeparator();
            this.Menu_EditHistoryList = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator18 = new System.Windows.Forms.ToolStripSeparator();
            this.Menu_Exit = new System.Windows.Forms.ToolStripMenuItem();
            this.Menu_MainEdit = new System.Windows.Forms.ToolStripMenuItem();
            this.Menu_AddSong = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator19 = new System.Windows.Forms.ToolStripSeparator();
            this.Menu_EditSong = new System.Windows.Forms.ToolStripMenuItem();
            this.Menu_CopySong = new System.Windows.Forms.ToolStripMenuItem();
            this.Menu_MoveSong = new System.Windows.Forms.ToolStripMenuItem();
            this.Menu_DeleteSong = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator41 = new System.Windows.Forms.ToolStripSeparator();
            this.Menu_SelectAll = new System.Windows.Forms.ToolStripMenuItem();
            this.Menu_Find = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator21 = new System.Windows.Forms.ToolStripSeparator();
            this.Menu_UseSongNumbering = new System.Windows.Forms.ToolStripMenuItem();
            this.Menu_ReArrangeSongFolders = new System.Windows.Forms.ToolStripMenuItem();
            this.Menu_MainView = new System.Windows.Forms.ToolStripMenuItem();
            this.Menu_EasiSlidesFolder = new System.Windows.Forms.ToolStripMenuItem();
            this.Menu_Options = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator23 = new System.Windows.Forms.ToolStripSeparator();
            this.Menu_Refresh = new System.Windows.Forms.ToolStripMenuItem();
            this.Menu_PreviewNotations = new System.Windows.Forms.ToolStripMenuItem();
            this.Menu_StatusBar = new System.Windows.Forms.ToolStripMenuItem();
            this.Menu_MainOutput = new System.Windows.Forms.ToolStripMenuItem();
            this.Menu_StartShow = new System.Windows.Forms.ToolStripMenuItem();
            this.Menu_GoLiveWithPreview = new System.Windows.Forms.ToolStripMenuItem();
            this.Menu_RefreshOutput = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator28 = new System.Windows.Forms.ToolStripSeparator();
            this.Menu_BlackScreen = new System.Windows.Forms.ToolStripMenuItem();
            this.Menu_ClearScreen = new System.Windows.Forms.ToolStripMenuItem();
            this.Menu_LiveCam = new System.Windows.Forms.ToolStripMenuItem();
            this.Menu_RestartCurrentItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator29 = new System.Windows.Forms.ToolStripSeparator();
            this.Menu_AlertWindow = new System.Windows.Forms.ToolStripMenuItem();
            this.Menu_StopAlert = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator30 = new System.Windows.Forms.ToolStripSeparator();
            this.Menu_SwitchChinese = new System.Windows.Forms.ToolStripMenuItem();
            this.Menu_MainTools = new System.Windows.Forms.ToolStripMenuItem();
            this.Menu_Import = new System.Windows.Forms.ToolStripMenuItem();
            this.Menu_ImportFolder = new System.Windows.Forms.ToolStripMenuItem();
            this.Menu_Export = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator32 = new System.Windows.Forms.ToolStripSeparator();
            this.Menu_Recover = new System.Windows.Forms.ToolStripMenuItem();
            this.Menu_Empty = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator33 = new System.Windows.Forms.ToolStripSeparator();
            this.Menu_AddToUsages = new System.Windows.Forms.ToolStripMenuItem();
            this.Menu_ViewUsages = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator34 = new System.Windows.Forms.ToolStripSeparator();
            this.Menu_SmartMerge = new System.Windows.Forms.ToolStripMenuItem();
            this.Menu_Compact = new System.Windows.Forms.ToolStripMenuItem();
            this.Menu_ClearAllFormatting = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator9 = new System.Windows.Forms.ToolStripSeparator();
            this.Menu_ClearRegistrySettings = new System.Windows.Forms.ToolStripMenuItem();
            this.Menu_MainHelp = new System.Windows.Forms.ToolStripMenuItem();
            this.Menu_Contents = new System.Windows.Forms.ToolStripMenuItem();
            this.Menu_HelpWeb = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator31 = new System.Windows.Forms.ToolStripSeparator();
            this.Menu_Register = new System.Windows.Forms.ToolStripMenuItem();
            this.Menu_About = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStripMain = new System.Windows.Forms.StatusStrip();
            this.StatusBarPanel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.StatusBarPanel2 = new System.Windows.Forms.ToolStripStatusLabel();
            this.StatusBarPanel3 = new System.Windows.Forms.ToolStripStatusLabel();
            this.StatusBarPanel4 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.TimerFlasher = new System.Windows.Forms.Timer(this.components);
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.TimerReMax = new System.Windows.Forms.Timer(this.components);
            this.TimerSearch = new System.Windows.Forms.Timer(this.components);
            this.CMenuImages = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.CMenuImages_AddItem = new System.Windows.Forms.ToolStripMenuItem();
            this.CMenuImages_AddDefault = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator35 = new System.Windows.Forms.ToolStripSeparator();
            this.CMenuImages_Refresh = new System.Windows.Forms.ToolStripMenuItem();
            this.TimerMessagingWindowOpen = new System.Windows.Forms.Timer(this.components);
            this.TimerToFront = new System.Windows.Forms.Timer(this.components);
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.toolStripContainerMain.ContentPanel.SuspendLayout();
            this.toolStripContainerMain.TopToolStripPanel.SuspendLayout();
            this.toolStripContainerMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerMain)).BeginInit();
            this.splitContainerMain.Panel1.SuspendLayout();
            this.splitContainerMain.Panel2.SuspendLayout();
            this.splitContainerMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.tabControlSource.SuspendLayout();
            this.tabFolders.SuspendLayout();
            this.panelFolders.SuspendLayout();
            this.toolStripFolders.SuspendLayout();
            this.CMenuSongs.SuspendLayout();
            this.tabFiles.SuspendLayout();
            this.panelInfoScreen2.SuspendLayout();
            this.InfoScreentoolstrip2.SuspendLayout();
            this.panelExternalFiles.SuspendLayout();
            this.panelInfoScreen1.SuspendLayout();
            this.InfoScreentoolstrip1.SuspendLayout();
            this.CMenuFiles.SuspendLayout();
            this.tabPowerpoint.SuspendLayout();
            this.panelPowerpoint2.SuspendLayout();
            this.toolStripPowerpoint2.SuspendLayout();
            this.panelPowerpoint1.SuspendLayout();
            this.panelExternalFiles1.SuspendLayout();
            this.toolStripPowerpoint1.SuspendLayout();
            this.tabBibles.SuspendLayout();
            this.CMenuBible.SuspendLayout();
            this.panelBible2.SuspendLayout();
            this.toolStripBible2.SuspendLayout();
            this.TabBibleVersions.SuspendLayout();
            this.tabImages.SuspendLayout();
            this.panelImagesTop.SuspendLayout();
            this.panelImage1.SuspendLayout();
            this.toolStripImage1.SuspendLayout();
            this.tabMedia.SuspendLayout();
            this.panel11.SuspendLayout();
            this.panelMedia1.SuspendLayout();
            this.toolStripMedia1.SuspendLayout();
            this.tabDefault.SuspendLayout();
            this.DefPanel.SuspendLayout();
            this.panelDefTemplate.SuspendLayout();
            this.toolStripDefTemplates.SuspendLayout();
            this.DefgroupBox2.SuspendLayout();
            this.panelDef4.SuspendLayout();
            this.toolStripDef4.SuspendLayout();
            this.panelDef3.SuspendLayout();
            this.toolStripDef3.SuspendLayout();
            this.DefgroupBox3.SuspendLayout();
            this.panel21.SuspendLayout();
            this.toolStripDef7.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Def_PanelHeight)).BeginInit();
            this.panelDef5.SuspendLayout();
            this.toolStripDef5.SuspendLayout();
            this.panelDef6.SuspendLayout();
            this.toolStripDef6.SuspendLayout();
            this.DefgroupBox1.SuspendLayout();
            this.panelDef2.SuspendLayout();
            this.toolStripDef2.SuspendLayout();
            this.panelDef1.SuspendLayout();
            this.toolStripDef1.SuspendLayout();
            this.tabControlLists.SuspendLayout();
            this.tabWorshipList.SuspendLayout();
            this.panelWorshipList2.SuspendLayout();
            this.toolStripWorshipList2.SuspendLayout();
            this.panelWorshipList1.SuspendLayout();
            this.toolStripWorshipList1.SuspendLayout();
            this.CMenuWorship.SuspendLayout();
            this.tabPraiseBook.SuspendLayout();
            this.panelPraiseBook2.SuspendLayout();
            this.toolStripPraiseBook2.SuspendLayout();
            this.panelPraiseBook1.SuspendLayout();
            this.toolStripPraiseBook1.SuspendLayout();
            this.CMenuPraiseB.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerPreview)).BeginInit();
            this.splitContainerPreview.Panel1.SuspendLayout();
            this.splitContainerPreview.Panel2.SuspendLayout();
            this.splitContainerPreview.SuspendLayout();
            this.panelPreviewTop.SuspendLayout();
            this.IndPanel.SuspendLayout();
            this.panelIndTemplate.SuspendLayout();
            this.toolStripIndTemplates.SuspendLayout();
            this.IndgroupBox4.SuspendLayout();
            this.panelInd7.SuspendLayout();
            this.toolStripInd7.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Ind_Reg2SizeUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.Ind_Reg2TopUpDown)).BeginInit();
            this.panelInd6.SuspendLayout();
            this.toolStripInd6.SuspendLayout();
            this.IndgroupBox3.SuspendLayout();
            this.panelInd5.SuspendLayout();
            this.toolStripInd5.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Ind_Reg1SizeUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.Ind_Reg1TopUpDown)).BeginInit();
            this.panelInd4.SuspendLayout();
            this.toolStripInd4.SuspendLayout();
            this.IndgroupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Ind_BottomUpDown)).BeginInit();
            this.panelInd3.SuspendLayout();
            this.toolStripInd3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Ind_RightUpDown)).BeginInit();
            this.panelInd2.SuspendLayout();
            this.toolStripInd2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Ind_LeftUpDown)).BeginInit();
            this.IndgroupBox1.SuspendLayout();
            this.panel4.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.panelInd1.SuspendLayout();
            this.toolStripInd1.SuspendLayout();
            this.panel9.SuspendLayout();
            this.panelPreviewBottom.SuspendLayout();
            this.panelPreviewSessionNotes2.SuspendLayout();
            this.panel1.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.panel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerOutput)).BeginInit();
            this.splitContainerOutput.Panel1.SuspendLayout();
            this.splitContainerOutput.Panel2.SuspendLayout();
            this.splitContainerOutput.SuspendLayout();
            this.panelOutputTop.SuspendLayout();
            this.panel10.SuspendLayout();
            this.panelOutputBottom.SuspendLayout();
            this.panelOutputLM1.SuspendLayout();
            this.panelOutputLM3.SuspendLayout();
            this.panel8.SuspendLayout();
            this.panel2.SuspendLayout();
            this.flowLayoutPanel2.SuspendLayout();
            this.panel6.SuspendLayout();
            this.toolStripMain.SuspendLayout();
            this.menuStripMain.SuspendLayout();
            this.statusStripMain.SuspendLayout();
            this.CMenuImages.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStripContainerMain
            // 
            // 
            // toolStripContainerMain.ContentPanel
            // 
            this.toolStripContainerMain.ContentPanel.Controls.Add(this.splitContainerMain);
            this.toolStripContainerMain.ContentPanel.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.toolStripContainerMain.ContentPanel.Size = new System.Drawing.Size(857, 490);
            this.toolStripContainerMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.toolStripContainerMain.Location = new System.Drawing.Point(0, 24);
            this.toolStripContainerMain.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.toolStripContainerMain.Name = "toolStripContainerMain";
            this.toolStripContainerMain.Size = new System.Drawing.Size(857, 515);
            this.toolStripContainerMain.TabIndex = 1;
            this.toolStripContainerMain.Text = "toolStripContainer1";
            // 
            // toolStripContainerMain.TopToolStripPanel
            // 
            this.toolStripContainerMain.TopToolStripPanel.Controls.Add(this.toolStripMain);
            // 
            // splitContainerMain
            // 
            this.splitContainerMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerMain.Location = new System.Drawing.Point(0, 0);
            this.splitContainerMain.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.splitContainerMain.Name = "splitContainerMain";
            // 
            // splitContainerMain.Panel1
            // 
            this.splitContainerMain.Panel1.Controls.Add(this.splitContainer1);
            // 
            // splitContainerMain.Panel2
            // 
            this.splitContainerMain.Panel2.Controls.Add(this.splitContainer2);
            this.splitContainerMain.Panel2MinSize = 50;
            this.splitContainerMain.Size = new System.Drawing.Size(857, 490);
            this.splitContainerMain.SplitterDistance = 307;
            this.splitContainerMain.SplitterWidth = 3;
            this.splitContainerMain.TabIndex = 0;
            this.splitContainerMain.Text = "splitContainer1";
            // 
            // splitContainer1
            // 
            this.splitContainer1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.tabControlSource);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.tabControlLists);
            this.splitContainer1.Size = new System.Drawing.Size(307, 490);
            this.splitContainer1.SplitterDistance = 386;
            this.splitContainer1.TabIndex = 0;
            this.splitContainer1.Text = "splitContainer2";
            // 
            // tabControlSource
            // 
            this.tabControlSource.Alignment = System.Windows.Forms.TabAlignment.Bottom;
            this.tabControlSource.Controls.Add(this.tabFolders);
            this.tabControlSource.Controls.Add(this.tabFiles);
            this.tabControlSource.Controls.Add(this.tabPowerpoint);
            this.tabControlSource.Controls.Add(this.tabBibles);
            this.tabControlSource.Controls.Add(this.tabImages);
            this.tabControlSource.Controls.Add(this.tabMedia);
            this.tabControlSource.Controls.Add(this.tabDefault);
            this.tabControlSource.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControlSource.Location = new System.Drawing.Point(0, 0);
            this.tabControlSource.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.tabControlSource.Name = "tabControlSource";
            this.tabControlSource.Padding = new System.Drawing.Point(5, 3);
            this.tabControlSource.SelectedIndex = 0;
            this.tabControlSource.Size = new System.Drawing.Size(303, 382);
            this.tabControlSource.TabIndex = 0;
            this.tabControlSource.SelectedIndexChanged += new System.EventHandler(this.tabControlSource_SelectedIndexChanged);
            this.tabControlSource.Resize += new System.EventHandler(this.tabControlSource_Resize);
            // 
            // tabFolders
            // 
            this.tabFolders.BackColor = System.Drawing.SystemColors.Control;
            this.tabFolders.Controls.Add(this.panelFolders);
            this.tabFolders.Controls.Add(this.SongsList);
            this.tabFolders.Controls.Add(this.SongFolder);
            this.tabFolders.Location = new System.Drawing.Point(4, 4);
            this.tabFolders.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.tabFolders.Name = "tabFolders";
            this.tabFolders.Padding = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.tabFolders.Size = new System.Drawing.Size(295, 354);
            this.tabFolders.TabIndex = 0;
            this.tabFolders.Text = "Folders";
            // 
            // panelFolders
            // 
            this.panelFolders.Controls.Add(this.toolStripFolders);
            this.panelFolders.Location = new System.Drawing.Point(77, 4);
            this.panelFolders.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.panelFolders.Name = "panelFolders";
            this.panelFolders.Size = new System.Drawing.Size(28, 25);
            this.panelFolders.TabIndex = 7;
            // 
            // toolStripFolders
            // 
            this.toolStripFolders.AutoSize = false;
            this.toolStripFolders.CanOverflow = false;
            this.toolStripFolders.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStripFolders.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStripFolders.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.Folders_WordCount});
            this.toolStripFolders.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
            this.toolStripFolders.Location = new System.Drawing.Point(0, -1);
            this.toolStripFolders.Name = "toolStripFolders";
            this.toolStripFolders.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.toolStripFolders.Size = new System.Drawing.Size(28, 29);
            this.toolStripFolders.TabIndex = 0;
            // 
            // Folders_WordCount
            // 
            this.Folders_WordCount.CheckOnClick = true;
            this.Folders_WordCount.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.Folders_WordCount.Image = ((System.Drawing.Image)(resources.GetObject("Folders_WordCount.Image")));
            this.Folders_WordCount.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.Folders_WordCount.Name = "Folders_WordCount";
            this.Folders_WordCount.Size = new System.Drawing.Size(23, 26);
            this.Folders_WordCount.Tag = "wordcount";
            this.Folders_WordCount.ToolTipText = "Sort by CJK Word Count";
            this.Folders_WordCount.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Folders_WordCount_MouseUp);
            // 
            // SongsList
            // 
            this.SongsList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3,
            this.columnHeader4,
            this.columnHeader5,
            this.columnHeader6,
            this.columnHeader7});
            this.SongsList.ContextMenuStrip = this.CMenuSongs;
            this.SongsList.FullRowSelect = true;
            this.SongsList.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.SongsList.LabelWrap = false;
            this.SongsList.Location = new System.Drawing.Point(3, 31);
            this.SongsList.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.SongsList.Name = "SongsList";
            this.SongsList.ShowItemToolTips = true;
            this.SongsList.Size = new System.Drawing.Size(40, 85);
            this.SongsList.TabIndex = 1;
            this.SongsList.UseCompatibleStateImageBehavior = false;
            this.SongsList.View = System.Windows.Forms.View.Details;
            this.SongsList.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.SongsList_ItemDrag);
            this.SongsList.Enter += new System.EventHandler(this.FormControl_Enter);
            this.SongsList.KeyUp += new System.Windows.Forms.KeyEventHandler(this.SongsList_KeyUp);
            this.SongsList.Leave += new System.EventHandler(this.FormControl_Leave);
            this.SongsList.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.SongsList_MouseDoubleClick);
            this.SongsList.MouseUp += new System.Windows.Forms.MouseEventHandler(this.SongsList_MouseUp);
            // 
            // columnHeader2
            // 
            this.columnHeader2.Width = 0;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Width = 0;
            // 
            // columnHeader4
            // 
            this.columnHeader4.Width = 0;
            // 
            // columnHeader5
            // 
            this.columnHeader5.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.columnHeader5.Width = 0;
            // 
            // columnHeader6
            // 
            this.columnHeader6.Width = 0;
            // 
            // columnHeader7
            // 
            this.columnHeader7.Width = 0;
            // 
            // CMenuSongs
            // 
            this.CMenuSongs.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.CMenuSongs_SelectAll,
            this.CMenuSongs_UnselectAll,
            this.CMenuSongs_AddShow,
            this.toolStripSeparator38,
            this.CMenuSongs_Edit,
            this.CMenuSongs_Copy,
            this.toolStripSeparator10,
            this.CMenuSongs_Refresh});
            this.CMenuSongs.Name = "ContextMenuBibleText";
            this.CMenuSongs.Size = new System.Drawing.Size(142, 148);
            // 
            // CMenuSongs_SelectAll
            // 
            this.CMenuSongs_SelectAll.Name = "CMenuSongs_SelectAll";
            this.CMenuSongs_SelectAll.Size = new System.Drawing.Size(141, 22);
            this.CMenuSongs_SelectAll.Text = "Select &All";
            this.CMenuSongs_SelectAll.Click += new System.EventHandler(this.CMenuSongs_SelectAll_Click);
            // 
            // CMenuSongs_UnselectAll
            // 
            this.CMenuSongs_UnselectAll.Name = "CMenuSongs_UnselectAll";
            this.CMenuSongs_UnselectAll.Size = new System.Drawing.Size(141, 22);
            this.CMenuSongs_UnselectAll.Text = "&Unselect All";
            this.CMenuSongs_UnselectAll.Click += new System.EventHandler(this.CMenuSongs_UnselectAll_Click);
            // 
            // CMenuSongs_AddShow
            // 
            this.CMenuSongs_AddShow.Name = "CMenuSongs_AddShow";
            this.CMenuSongs_AddShow.Size = new System.Drawing.Size(141, 22);
            this.CMenuSongs_AddShow.Text = "Add && &Show";
            this.CMenuSongs_AddShow.Click += new System.EventHandler(this.CMenuSongs_AddShow_Click);
            // 
            // toolStripSeparator38
            // 
            this.toolStripSeparator38.Name = "toolStripSeparator38";
            this.toolStripSeparator38.Size = new System.Drawing.Size(138, 6);
            // 
            // CMenuSongs_Edit
            // 
            this.CMenuSongs_Edit.Name = "CMenuSongs_Edit";
            this.CMenuSongs_Edit.Size = new System.Drawing.Size(141, 22);
            this.CMenuSongs_Edit.Text = "Edit item";
            this.CMenuSongs_Edit.Click += new System.EventHandler(this.CMenuSongs_Edit_Click);
            // 
            // CMenuSongs_Copy
            // 
            this.CMenuSongs_Copy.Name = "CMenuSongs_Copy";
            this.CMenuSongs_Copy.Size = new System.Drawing.Size(141, 22);
            this.CMenuSongs_Copy.Text = "Copy item";
            this.CMenuSongs_Copy.Click += new System.EventHandler(this.CMenuSongs_Copy_Click);
            // 
            // toolStripSeparator10
            // 
            this.toolStripSeparator10.Name = "toolStripSeparator10";
            this.toolStripSeparator10.Size = new System.Drawing.Size(138, 6);
            // 
            // CMenuSongs_Refresh
            // 
            this.CMenuSongs_Refresh.Name = "CMenuSongs_Refresh";
            this.CMenuSongs_Refresh.Size = new System.Drawing.Size(141, 22);
            this.CMenuSongs_Refresh.Text = "Refresh";
            this.CMenuSongs_Refresh.Click += new System.EventHandler(this.CMenuSongs_Refresh_Click);
            // 
            // SongFolder
            // 
            this.SongFolder.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.SongFolder.FormattingEnabled = true;
            this.SongFolder.Location = new System.Drawing.Point(3, 4);
            this.SongFolder.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.SongFolder.MaxDropDownItems = 12;
            this.SongFolder.Name = "SongFolder";
            this.SongFolder.Size = new System.Drawing.Size(66, 23);
            this.SongFolder.TabIndex = 0;
            this.SongFolder.SelectedIndexChanged += new System.EventHandler(this.SongFolder_SelectedIndexChanged);
            // 
            // tabFiles
            // 
            this.tabFiles.BackColor = System.Drawing.SystemColors.Control;
            this.tabFiles.Controls.Add(this.panelInfoScreen2);
            this.tabFiles.Controls.Add(this.panelExternalFiles);
            this.tabFiles.Controls.Add(this.InfoScreenList);
            this.tabFiles.Location = new System.Drawing.Point(4, 4);
            this.tabFiles.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.tabFiles.Name = "tabFiles";
            this.tabFiles.Size = new System.Drawing.Size(295, 354);
            this.tabFiles.TabIndex = 4;
            this.tabFiles.Text = "InfoScr";
            // 
            // panelInfoScreen2
            // 
            this.panelInfoScreen2.Controls.Add(this.InfoScreentoolstrip2);
            this.panelInfoScreen2.Location = new System.Drawing.Point(77, 31);
            this.panelInfoScreen2.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.panelInfoScreen2.Name = "panelInfoScreen2";
            this.panelInfoScreen2.Size = new System.Drawing.Size(29, 141);
            this.panelInfoScreen2.TabIndex = 17;
            // 
            // InfoScreentoolstrip2
            // 
            this.InfoScreentoolstrip2.AutoSize = false;
            this.InfoScreentoolstrip2.CanOverflow = false;
            this.InfoScreentoolstrip2.Dock = System.Windows.Forms.DockStyle.None;
            this.InfoScreentoolstrip2.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.InfoScreentoolstrip2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.InfoScreen_New,
            this.InfoScreen_Edit,
            this.InfoScreen_Copy,
            this.InfoScreen_Move,
            this.InfoScreen_Delete});
            this.InfoScreentoolstrip2.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.VerticalStackWithOverflow;
            this.InfoScreentoolstrip2.Location = new System.Drawing.Point(0, -1);
            this.InfoScreentoolstrip2.Name = "InfoScreentoolstrip2";
            this.InfoScreentoolstrip2.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.InfoScreentoolstrip2.Size = new System.Drawing.Size(29, 154);
            this.InfoScreentoolstrip2.TabIndex = 0;
            // 
            // InfoScreen_New
            // 
            this.InfoScreen_New.AutoSize = false;
            this.InfoScreen_New.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.InfoScreen_New.Image = ((System.Drawing.Image)(resources.GetObject("InfoScreen_New.Image")));
            this.InfoScreen_New.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.InfoScreen_New.Name = "InfoScreen_New";
            this.InfoScreen_New.Size = new System.Drawing.Size(23, 22);
            this.InfoScreen_New.Tag = "new";
            this.InfoScreen_New.ToolTipText = "New";
            this.InfoScreen_New.Click += new System.EventHandler(this.InfoScreen_EditBtns_Click);
            // 
            // InfoScreen_Edit
            // 
            this.InfoScreen_Edit.AutoSize = false;
            this.InfoScreen_Edit.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.InfoScreen_Edit.Image = ((System.Drawing.Image)(resources.GetObject("InfoScreen_Edit.Image")));
            this.InfoScreen_Edit.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.InfoScreen_Edit.Name = "InfoScreen_Edit";
            this.InfoScreen_Edit.Size = new System.Drawing.Size(22, 22);
            this.InfoScreen_Edit.Tag = "edit";
            this.InfoScreen_Edit.ToolTipText = "Edit";
            this.InfoScreen_Edit.Click += new System.EventHandler(this.InfoScreen_EditBtns_Click);
            // 
            // InfoScreen_Copy
            // 
            this.InfoScreen_Copy.AutoSize = false;
            this.InfoScreen_Copy.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.InfoScreen_Copy.Image = ((System.Drawing.Image)(resources.GetObject("InfoScreen_Copy.Image")));
            this.InfoScreen_Copy.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.InfoScreen_Copy.Name = "InfoScreen_Copy";
            this.InfoScreen_Copy.Size = new System.Drawing.Size(22, 22);
            this.InfoScreen_Copy.Tag = "copy";
            this.InfoScreen_Copy.ToolTipText = "Copy";
            this.InfoScreen_Copy.Click += new System.EventHandler(this.InfoScreen_EditBtns_Click);
            // 
            // InfoScreen_Move
            // 
            this.InfoScreen_Move.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.InfoScreen_Move.Image = ((System.Drawing.Image)(resources.GetObject("InfoScreen_Move.Image")));
            this.InfoScreen_Move.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.InfoScreen_Move.Name = "InfoScreen_Move";
            this.InfoScreen_Move.Size = new System.Drawing.Size(27, 20);
            this.InfoScreen_Move.Tag = "move";
            this.InfoScreen_Move.ToolTipText = "Move";
            this.InfoScreen_Move.Click += new System.EventHandler(this.InfoScreen_EditBtns_Click);
            // 
            // InfoScreen_Delete
            // 
            this.InfoScreen_Delete.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.InfoScreen_Delete.Image = ((System.Drawing.Image)(resources.GetObject("InfoScreen_Delete.Image")));
            this.InfoScreen_Delete.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.InfoScreen_Delete.Name = "InfoScreen_Delete";
            this.InfoScreen_Delete.Size = new System.Drawing.Size(27, 20);
            this.InfoScreen_Delete.Tag = "delete";
            this.InfoScreen_Delete.ToolTipText = "Delete";
            this.InfoScreen_Delete.Click += new System.EventHandler(this.InfoScreen_EditBtns_Click);
            // 
            // panelExternalFiles
            // 
            this.panelExternalFiles.Controls.Add(this.panelInfoScreen1);
            this.panelExternalFiles.Controls.Add(this.InfoScreenFolder);
            this.panelExternalFiles.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelExternalFiles.Location = new System.Drawing.Point(0, 0);
            this.panelExternalFiles.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.panelExternalFiles.Name = "panelExternalFiles";
            this.panelExternalFiles.Size = new System.Drawing.Size(295, 30);
            this.panelExternalFiles.TabIndex = 16;
            // 
            // panelInfoScreen1
            // 
            this.panelInfoScreen1.Controls.Add(this.InfoScreentoolstrip1);
            this.panelInfoScreen1.Location = new System.Drawing.Point(134, 4);
            this.panelInfoScreen1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.panelInfoScreen1.Name = "panelInfoScreen1";
            this.panelInfoScreen1.Size = new System.Drawing.Size(58, 22);
            this.panelInfoScreen1.TabIndex = 18;
            // 
            // InfoScreentoolstrip1
            // 
            this.InfoScreentoolstrip1.AutoSize = false;
            this.InfoScreentoolstrip1.CanOverflow = false;
            this.InfoScreentoolstrip1.Dock = System.Windows.Forms.DockStyle.None;
            this.InfoScreentoolstrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.InfoScreentoolstrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.InfoScreen_OpenFolder,
            this.InfoScreen_Import});
            this.InfoScreentoolstrip1.Location = new System.Drawing.Point(0, -1);
            this.InfoScreentoolstrip1.Name = "InfoScreentoolstrip1";
            this.InfoScreentoolstrip1.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.InfoScreentoolstrip1.Size = new System.Drawing.Size(63, 28);
            this.InfoScreentoolstrip1.TabIndex = 5;
            // 
            // InfoScreen_OpenFolder
            // 
            this.InfoScreen_OpenFolder.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.InfoScreen_OpenFolder.Image = ((System.Drawing.Image)(resources.GetObject("InfoScreen_OpenFolder.Image")));
            this.InfoScreen_OpenFolder.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.InfoScreen_OpenFolder.Name = "InfoScreen_OpenFolder";
            this.InfoScreen_OpenFolder.Size = new System.Drawing.Size(23, 25);
            this.InfoScreen_OpenFolder.Tag = "";
            this.InfoScreen_OpenFolder.ToolTipText = "Open Folder";
            this.InfoScreen_OpenFolder.Click += new System.EventHandler(this.InfoScreen_OpenFolder_Click);
            // 
            // InfoScreen_Import
            // 
            this.InfoScreen_Import.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.InfoScreen_Import.Image = ((System.Drawing.Image)(resources.GetObject("InfoScreen_Import.Image")));
            this.InfoScreen_Import.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.InfoScreen_Import.Name = "InfoScreen_Import";
            this.InfoScreen_Import.Size = new System.Drawing.Size(23, 25);
            this.InfoScreen_Import.ToolTipText = "Import an InfoScreen Into Folder";
            this.InfoScreen_Import.Click += new System.EventHandler(this.InfoScreen_Import_Click);
            // 
            // InfoScreenFolder
            // 
            this.InfoScreenFolder.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.InfoScreenFolder.FormattingEnabled = true;
            this.InfoScreenFolder.Location = new System.Drawing.Point(3, 4);
            this.InfoScreenFolder.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.InfoScreenFolder.MaxDropDownItems = 12;
            this.InfoScreenFolder.Name = "InfoScreenFolder";
            this.InfoScreenFolder.Size = new System.Drawing.Size(123, 23);
            this.InfoScreenFolder.TabIndex = 17;
            this.InfoScreenFolder.SelectedIndexChanged += new System.EventHandler(this.InfoScreenFolder_SelectedIndexChanged);
            // 
            // InfoScreenList
            // 
            this.InfoScreenList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader23,
            this.columnHeader24,
            this.columnHeader25,
            this.columnHeader26,
            this.columnHeader27,
            this.columnHeader28,
            this.columnHeader29});
            this.InfoScreenList.ContextMenuStrip = this.CMenuFiles;
            this.InfoScreenList.FullRowSelect = true;
            this.InfoScreenList.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.InfoScreenList.LabelWrap = false;
            this.InfoScreenList.Location = new System.Drawing.Point(3, 31);
            this.InfoScreenList.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.InfoScreenList.Name = "InfoScreenList";
            this.InfoScreenList.ShowItemToolTips = true;
            this.InfoScreenList.Size = new System.Drawing.Size(40, 85);
            this.InfoScreenList.TabIndex = 5;
            this.InfoScreenList.UseCompatibleStateImageBehavior = false;
            this.InfoScreenList.View = System.Windows.Forms.View.Details;
            this.InfoScreenList.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.InfoScreenList_ItemDrag);
            this.InfoScreenList.Enter += new System.EventHandler(this.FormControl_Enter);
            this.InfoScreenList.KeyUp += new System.Windows.Forms.KeyEventHandler(this.InfoScreenList_KeyUp);
            this.InfoScreenList.Leave += new System.EventHandler(this.FormControl_Leave);
            this.InfoScreenList.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.InfoScreenList_MouseDoubleClick);
            this.InfoScreenList.MouseUp += new System.Windows.Forms.MouseEventHandler(this.InfoScreenList_MouseUp);
            // 
            // columnHeader24
            // 
            this.columnHeader24.Width = 0;
            // 
            // columnHeader25
            // 
            this.columnHeader25.Width = 0;
            // 
            // columnHeader26
            // 
            this.columnHeader26.Width = 0;
            // 
            // columnHeader27
            // 
            this.columnHeader27.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.columnHeader27.Width = 0;
            // 
            // columnHeader28
            // 
            this.columnHeader28.Width = 0;
            // 
            // columnHeader29
            // 
            this.columnHeader29.Width = 0;
            // 
            // CMenuFiles
            // 
            this.CMenuFiles.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.CMenuFiles_SelectAll,
            this.CMenuFiles_UnselectAll,
            this.CMenuFiles_AddShow,
            this.toolStripSeparator12,
            this.CMenuFiles_Edit,
            this.CMenuFiles_Copy,
            this.toolStripSeparator25,
            this.CMenuFiles_Refresh});
            this.CMenuFiles.Name = "ContextMenuBibleText";
            this.CMenuFiles.Size = new System.Drawing.Size(142, 148);
            // 
            // CMenuFiles_SelectAll
            // 
            this.CMenuFiles_SelectAll.Name = "CMenuFiles_SelectAll";
            this.CMenuFiles_SelectAll.Size = new System.Drawing.Size(141, 22);
            this.CMenuFiles_SelectAll.Text = "Select &All";
            this.CMenuFiles_SelectAll.Click += new System.EventHandler(this.CMenuFiles_SelectAll_Click);
            // 
            // CMenuFiles_UnselectAll
            // 
            this.CMenuFiles_UnselectAll.Name = "CMenuFiles_UnselectAll";
            this.CMenuFiles_UnselectAll.Size = new System.Drawing.Size(141, 22);
            this.CMenuFiles_UnselectAll.Text = "&Unselect All";
            this.CMenuFiles_UnselectAll.Click += new System.EventHandler(this.CMenuFiles_UnselectAll_Click);
            // 
            // CMenuFiles_AddShow
            // 
            this.CMenuFiles_AddShow.Name = "CMenuFiles_AddShow";
            this.CMenuFiles_AddShow.Size = new System.Drawing.Size(141, 22);
            this.CMenuFiles_AddShow.Text = "Add && &Show";
            this.CMenuFiles_AddShow.Click += new System.EventHandler(this.CMenuFiles_AddShow_Click);
            // 
            // toolStripSeparator12
            // 
            this.toolStripSeparator12.Name = "toolStripSeparator12";
            this.toolStripSeparator12.Size = new System.Drawing.Size(138, 6);
            // 
            // CMenuFiles_Edit
            // 
            this.CMenuFiles_Edit.Name = "CMenuFiles_Edit";
            this.CMenuFiles_Edit.Size = new System.Drawing.Size(141, 22);
            this.CMenuFiles_Edit.Text = "Edit";
            this.CMenuFiles_Edit.Click += new System.EventHandler(this.CMenuFiles_Edit_Click);
            // 
            // CMenuFiles_Copy
            // 
            this.CMenuFiles_Copy.Name = "CMenuFiles_Copy";
            this.CMenuFiles_Copy.Size = new System.Drawing.Size(141, 22);
            this.CMenuFiles_Copy.Text = "Copy";
            this.CMenuFiles_Copy.Click += new System.EventHandler(this.CMenuFiles_Copy_Click);
            // 
            // toolStripSeparator25
            // 
            this.toolStripSeparator25.Name = "toolStripSeparator25";
            this.toolStripSeparator25.Size = new System.Drawing.Size(138, 6);
            // 
            // CMenuFiles_Refresh
            // 
            this.CMenuFiles_Refresh.Name = "CMenuFiles_Refresh";
            this.CMenuFiles_Refresh.Size = new System.Drawing.Size(141, 22);
            this.CMenuFiles_Refresh.Text = "Refresh";
            this.CMenuFiles_Refresh.Click += new System.EventHandler(this.CMenuFiles_Refresh_Click);
            // 
            // tabPowerpoint
            // 
            this.tabPowerpoint.BackColor = System.Drawing.SystemColors.Control;
            this.tabPowerpoint.Controls.Add(this.flowLayoutExternalPowerPoint);
            this.tabPowerpoint.Controls.Add(this.panelPowerpoint2);
            this.tabPowerpoint.Controls.Add(this.PowerpointList);
            this.tabPowerpoint.Controls.Add(this.panelPowerpoint1);
            this.tabPowerpoint.Location = new System.Drawing.Point(4, 4);
            this.tabPowerpoint.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.tabPowerpoint.Name = "tabPowerpoint";
            this.tabPowerpoint.Size = new System.Drawing.Size(295, 354);
            this.tabPowerpoint.TabIndex = 5;
            this.tabPowerpoint.Text = "PowerP";
            // 
            // flowLayoutExternalPowerPoint
            // 
            this.flowLayoutExternalPowerPoint.AutoScroll = true;
            this.flowLayoutExternalPowerPoint.Location = new System.Drawing.Point(105, 51);
            this.flowLayoutExternalPowerPoint.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.flowLayoutExternalPowerPoint.Name = "flowLayoutExternalPowerPoint";
            this.flowLayoutExternalPowerPoint.Size = new System.Drawing.Size(69, 40);
            this.flowLayoutExternalPowerPoint.TabIndex = 18;
            // 
            // panelPowerpoint2
            // 
            this.panelPowerpoint2.Controls.Add(this.toolStripPowerpoint2);
            this.panelPowerpoint2.Location = new System.Drawing.Point(69, 31);
            this.panelPowerpoint2.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.panelPowerpoint2.Name = "panelPowerpoint2";
            this.panelPowerpoint2.Size = new System.Drawing.Size(29, 112);
            this.panelPowerpoint2.TabIndex = 20;
            // 
            // toolStripPowerpoint2
            // 
            this.toolStripPowerpoint2.AutoSize = false;
            this.toolStripPowerpoint2.CanOverflow = false;
            this.toolStripPowerpoint2.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStripPowerpoint2.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStripPowerpoint2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.Powerpoint_Edit,
            this.Powerpoint_Copy,
            this.Powerpoint_Move,
            this.Powerpoint_Delete});
            this.toolStripPowerpoint2.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.VerticalStackWithOverflow;
            this.toolStripPowerpoint2.Location = new System.Drawing.Point(0, -1);
            this.toolStripPowerpoint2.Name = "toolStripPowerpoint2";
            this.toolStripPowerpoint2.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.toolStripPowerpoint2.Size = new System.Drawing.Size(29, 125);
            this.toolStripPowerpoint2.TabIndex = 0;
            // 
            // Powerpoint_Edit
            // 
            this.Powerpoint_Edit.AutoSize = false;
            this.Powerpoint_Edit.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.Powerpoint_Edit.Image = ((System.Drawing.Image)(resources.GetObject("Powerpoint_Edit.Image")));
            this.Powerpoint_Edit.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.Powerpoint_Edit.Name = "Powerpoint_Edit";
            this.Powerpoint_Edit.Size = new System.Drawing.Size(22, 22);
            this.Powerpoint_Edit.Tag = "edit";
            this.Powerpoint_Edit.ToolTipText = "Edit";
            this.Powerpoint_Edit.Click += new System.EventHandler(this.Powerpoint_EditBtns_Click);
            // 
            // Powerpoint_Copy
            // 
            this.Powerpoint_Copy.AutoSize = false;
            this.Powerpoint_Copy.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.Powerpoint_Copy.Image = ((System.Drawing.Image)(resources.GetObject("Powerpoint_Copy.Image")));
            this.Powerpoint_Copy.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.Powerpoint_Copy.Name = "Powerpoint_Copy";
            this.Powerpoint_Copy.Size = new System.Drawing.Size(22, 22);
            this.Powerpoint_Copy.Tag = "copy";
            this.Powerpoint_Copy.ToolTipText = "Copy";
            this.Powerpoint_Copy.Click += new System.EventHandler(this.Powerpoint_EditBtns_Click);
            // 
            // Powerpoint_Move
            // 
            this.Powerpoint_Move.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.Powerpoint_Move.Image = ((System.Drawing.Image)(resources.GetObject("Powerpoint_Move.Image")));
            this.Powerpoint_Move.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.Powerpoint_Move.Name = "Powerpoint_Move";
            this.Powerpoint_Move.Size = new System.Drawing.Size(27, 20);
            this.Powerpoint_Move.Tag = "move";
            this.Powerpoint_Move.ToolTipText = "Move";
            this.Powerpoint_Move.Click += new System.EventHandler(this.Powerpoint_EditBtns_Click);
            // 
            // Powerpoint_Delete
            // 
            this.Powerpoint_Delete.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.Powerpoint_Delete.Image = ((System.Drawing.Image)(resources.GetObject("Powerpoint_Delete.Image")));
            this.Powerpoint_Delete.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.Powerpoint_Delete.Name = "Powerpoint_Delete";
            this.Powerpoint_Delete.Size = new System.Drawing.Size(27, 20);
            this.Powerpoint_Delete.Tag = "delete";
            this.Powerpoint_Delete.ToolTipText = "Delete";
            this.Powerpoint_Delete.Click += new System.EventHandler(this.Powerpoint_EditBtns_Click);
            // 
            // PowerpointList
            // 
            this.PowerpointList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader30,
            this.columnHeader31,
            this.columnHeader32,
            this.columnHeader33,
            this.columnHeader34,
            this.columnHeader35,
            this.columnHeader36});
            this.PowerpointList.ContextMenuStrip = this.CMenuFiles;
            this.PowerpointList.FullRowSelect = true;
            this.PowerpointList.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.PowerpointList.LabelWrap = false;
            this.PowerpointList.Location = new System.Drawing.Point(3, 31);
            this.PowerpointList.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.PowerpointList.Name = "PowerpointList";
            this.PowerpointList.ShowItemToolTips = true;
            this.PowerpointList.Size = new System.Drawing.Size(40, 85);
            this.PowerpointList.TabIndex = 19;
            this.PowerpointList.UseCompatibleStateImageBehavior = false;
            this.PowerpointList.View = System.Windows.Forms.View.Details;
            this.PowerpointList.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.PowerpointList_ItemDrag);
            this.PowerpointList.Enter += new System.EventHandler(this.FormControl_Enter);
            this.PowerpointList.KeyUp += new System.Windows.Forms.KeyEventHandler(this.PowerpointList_KeyUp);
            this.PowerpointList.Leave += new System.EventHandler(this.FormControl_Leave);
            this.PowerpointList.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.PowerpointList_MouseDoubleClick);
            this.PowerpointList.MouseUp += new System.Windows.Forms.MouseEventHandler(this.PowerpointList_MouseUp);
            // 
            // columnHeader31
            // 
            this.columnHeader31.Width = 0;
            // 
            // columnHeader32
            // 
            this.columnHeader32.Width = 0;
            // 
            // columnHeader33
            // 
            this.columnHeader33.Width = 0;
            // 
            // columnHeader34
            // 
            this.columnHeader34.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.columnHeader34.Width = 0;
            // 
            // columnHeader35
            // 
            this.columnHeader35.Width = 0;
            // 
            // columnHeader36
            // 
            this.columnHeader36.Width = 0;
            // 
            // panelPowerpoint1
            // 
            this.panelPowerpoint1.Controls.Add(this.PowerpointFolder);
            this.panelPowerpoint1.Controls.Add(this.panelExternalFiles1);
            this.panelPowerpoint1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelPowerpoint1.Location = new System.Drawing.Point(0, 0);
            this.panelPowerpoint1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.panelPowerpoint1.Name = "panelPowerpoint1";
            this.panelPowerpoint1.Size = new System.Drawing.Size(295, 30);
            this.panelPowerpoint1.TabIndex = 17;
            // 
            // PowerpointFolder
            // 
            this.PowerpointFolder.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.PowerpointFolder.FormattingEnabled = true;
            this.PowerpointFolder.Location = new System.Drawing.Point(3, 4);
            this.PowerpointFolder.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.PowerpointFolder.MaxDropDownItems = 12;
            this.PowerpointFolder.Name = "PowerpointFolder";
            this.PowerpointFolder.Size = new System.Drawing.Size(123, 23);
            this.PowerpointFolder.TabIndex = 17;
            this.PowerpointFolder.SelectedIndexChanged += new System.EventHandler(this.PowerpointFolder_SelectedIndexChanged);
            // 
            // panelExternalFiles1
            // 
            this.panelExternalFiles1.Controls.Add(this.toolStripPowerpoint1);
            this.panelExternalFiles1.Location = new System.Drawing.Point(133, 4);
            this.panelExternalFiles1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.panelExternalFiles1.Name = "panelExternalFiles1";
            this.panelExternalFiles1.Size = new System.Drawing.Size(91, 25);
            this.panelExternalFiles1.TabIndex = 13;
            // 
            // toolStripPowerpoint1
            // 
            this.toolStripPowerpoint1.AutoSize = false;
            this.toolStripPowerpoint1.CanOverflow = false;
            this.toolStripPowerpoint1.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStripPowerpoint1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStripPowerpoint1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.PP_ListType,
            this.PP_OpenFolder,
            this.PP_Import});
            this.toolStripPowerpoint1.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
            this.toolStripPowerpoint1.Location = new System.Drawing.Point(0, -1);
            this.toolStripPowerpoint1.Name = "toolStripPowerpoint1";
            this.toolStripPowerpoint1.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.toolStripPowerpoint1.Size = new System.Drawing.Size(96, 29);
            this.toolStripPowerpoint1.TabIndex = 5;
            // 
            // PP_ListType
            // 
            this.PP_ListType.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.PP_ListType.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.PP_ListStyle,
            this.PP_PreviewStyle});
            this.PP_ListType.Image = ((System.Drawing.Image)(resources.GetObject("PP_ListType.Image")));
            this.PP_ListType.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.PP_ListType.Name = "PP_ListType";
            this.PP_ListType.Size = new System.Drawing.Size(29, 26);
            this.PP_ListType.Tag = "0";
            this.PP_ListType.DropDownItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.PP_Style_DropDownItemClicked);
            // 
            // PP_ListStyle
            // 
            this.PP_ListStyle.Image = ((System.Drawing.Image)(resources.GetObject("PP_ListStyle.Image")));
            this.PP_ListStyle.Name = "PP_ListStyle";
            this.PP_ListStyle.Size = new System.Drawing.Size(179, 22);
            this.PP_ListStyle.Tag = "0";
            this.PP_ListStyle.Text = "Powerpoint Listing";
            // 
            // PP_PreviewStyle
            // 
            this.PP_PreviewStyle.Image = ((System.Drawing.Image)(resources.GetObject("PP_PreviewStyle.Image")));
            this.PP_PreviewStyle.Name = "PP_PreviewStyle";
            this.PP_PreviewStyle.Size = new System.Drawing.Size(179, 22);
            this.PP_PreviewStyle.Tag = "1";
            this.PP_PreviewStyle.Text = "Powerpoint Preview";
            // 
            // PP_OpenFolder
            // 
            this.PP_OpenFolder.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.PP_OpenFolder.Image = ((System.Drawing.Image)(resources.GetObject("PP_OpenFolder.Image")));
            this.PP_OpenFolder.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.PP_OpenFolder.Name = "PP_OpenFolder";
            this.PP_OpenFolder.Size = new System.Drawing.Size(23, 26);
            this.PP_OpenFolder.Tag = "";
            this.PP_OpenFolder.ToolTipText = "Open Powerpoint Folder";
            this.PP_OpenFolder.Click += new System.EventHandler(this.PP_Btn_Click);
            // 
            // PP_Import
            // 
            this.PP_Import.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.PP_Import.Image = ((System.Drawing.Image)(resources.GetObject("PP_Import.Image")));
            this.PP_Import.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.PP_Import.Name = "PP_Import";
            this.PP_Import.Size = new System.Drawing.Size(23, 26);
            this.PP_Import.ToolTipText = "Import a Powerpoint File into Folder";
            this.PP_Import.Click += new System.EventHandler(this.PP_Import_Click);
            // 
            // tabBibles
            // 
            this.tabBibles.BackColor = System.Drawing.SystemColors.Control;
            this.tabBibles.Controls.Add(this.BibleText);
            this.tabBibles.Controls.Add(this.BibleUserLookup);
            this.tabBibles.Controls.Add(this.panelBible2);
            this.tabBibles.Controls.Add(this.BookLookup);
            this.tabBibles.Controls.Add(this.TabBibleVersions);
            this.tabBibles.Location = new System.Drawing.Point(4, 4);
            this.tabBibles.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.tabBibles.Name = "tabBibles";
            this.tabBibles.Size = new System.Drawing.Size(295, 354);
            this.tabBibles.TabIndex = 2;
            this.tabBibles.Text = "Bibles";
            // 
            // BibleText
            // 
            this.BibleText.BackColor = System.Drawing.SystemColors.Window;
            this.BibleText.ContextMenuStrip = this.CMenuBible;
            this.BibleText.EnableAutoDragDrop = true;
            this.BibleText.HideSelection = false;
            this.BibleText.Location = new System.Drawing.Point(3, 31);
            this.BibleText.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.BibleText.Name = "BibleText";
            this.BibleText.ReadOnly = true;
            this.BibleText.Size = new System.Drawing.Size(34, 48);
            this.BibleText.TabIndex = 2;
            this.BibleText.Text = "";
            this.BibleText.Enter += new System.EventHandler(this.FormControl_Enter);
            this.BibleText.KeyUp += new System.Windows.Forms.KeyEventHandler(this.BibleText_KeyUp);
            this.BibleText.Leave += new System.EventHandler(this.FormControl_Leave);
            this.BibleText.MouseDown += new System.Windows.Forms.MouseEventHandler(this.BibleText_MouseDown);
            this.BibleText.MouseUp += new System.Windows.Forms.MouseEventHandler(this.BibleText_MouseUp);
            // 
            // CMenuBible
            // 
            this.CMenuBible.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.CMenuBible_SelectAll,
            this.CMenuBible_UnselectAll,
            this.CMenuBible_AddShow,
            this.toolStripSeparator17,
            this.CMenuBible_AddRegion2,
            this.toolStripSeparator24,
            this.CMenuBible_Copy,
            this.CMenuBible_CopyInfoScreen});
            this.CMenuBible.Name = "ContextMenuBibleText";
            this.CMenuBible.Size = new System.Drawing.Size(176, 148);
            // 
            // CMenuBible_SelectAll
            // 
            this.CMenuBible_SelectAll.Name = "CMenuBible_SelectAll";
            this.CMenuBible_SelectAll.Size = new System.Drawing.Size(175, 22);
            this.CMenuBible_SelectAll.Text = "Select &All";
            this.CMenuBible_SelectAll.Click += new System.EventHandler(this.CMenuBible_SelectAll_Click);
            // 
            // CMenuBible_UnselectAll
            // 
            this.CMenuBible_UnselectAll.Name = "CMenuBible_UnselectAll";
            this.CMenuBible_UnselectAll.Size = new System.Drawing.Size(175, 22);
            this.CMenuBible_UnselectAll.Text = "&Unselect All";
            this.CMenuBible_UnselectAll.Click += new System.EventHandler(this.CMenuBible_UnselectAll_Click);
            // 
            // CMenuBible_AddShow
            // 
            this.CMenuBible_AddShow.Name = "CMenuBible_AddShow";
            this.CMenuBible_AddShow.Size = new System.Drawing.Size(175, 22);
            this.CMenuBible_AddShow.Text = "Add && &Show";
            this.CMenuBible_AddShow.Click += new System.EventHandler(this.CMenuBible_AddShow_Click);
            // 
            // toolStripSeparator17
            // 
            this.toolStripSeparator17.Name = "toolStripSeparator17";
            this.toolStripSeparator17.Size = new System.Drawing.Size(172, 6);
            // 
            // CMenuBible_AddRegion2
            // 
            this.CMenuBible_AddRegion2.Name = "CMenuBible_AddRegion2";
            this.CMenuBible_AddRegion2.Size = new System.Drawing.Size(175, 22);
            this.CMenuBible_AddRegion2.Text = "Add &Region 2";
            // 
            // toolStripSeparator24
            // 
            this.toolStripSeparator24.Name = "toolStripSeparator24";
            this.toolStripSeparator24.Size = new System.Drawing.Size(172, 6);
            // 
            // CMenuBible_Copy
            // 
            this.CMenuBible_Copy.Name = "CMenuBible_Copy";
            this.CMenuBible_Copy.Size = new System.Drawing.Size(175, 22);
            this.CMenuBible_Copy.Text = "&Copy";
            this.CMenuBible_Copy.Click += new System.EventHandler(this.CMenuBible_Copy_Click);
            // 
            // CMenuBible_CopyInfoScreen
            // 
            this.CMenuBible_CopyInfoScreen.Name = "CMenuBible_CopyInfoScreen";
            this.CMenuBible_CopyInfoScreen.Size = new System.Drawing.Size(175, 22);
            this.CMenuBible_CopyInfoScreen.Text = "Copy to &InfoScreen";
            this.CMenuBible_CopyInfoScreen.Click += new System.EventHandler(this.CMenuBible_CopyInfoScreen_Click);
            // 
            // BibleUserLookup
            // 
            this.BibleUserLookup.Location = new System.Drawing.Point(75, 5);
            this.BibleUserLookup.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.BibleUserLookup.Name = "BibleUserLookup";
            this.BibleUserLookup.Size = new System.Drawing.Size(52, 23);
            this.BibleUserLookup.TabIndex = 1;
            this.BibleUserLookup.Enter += new System.EventHandler(this.FormControl_Enter);
            this.BibleUserLookup.KeyUp += new System.Windows.Forms.KeyEventHandler(this.BibleUserLookup_KeyUp);
            this.BibleUserLookup.Leave += new System.EventHandler(this.FormControl_Leave);
            // 
            // panelBible2
            // 
            this.panelBible2.Controls.Add(this.toolStripBible2);
            this.panelBible2.Location = new System.Drawing.Point(131, 4);
            this.panelBible2.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.panelBible2.Name = "panelBible2";
            this.panelBible2.Size = new System.Drawing.Size(28, 25);
            this.panelBible2.TabIndex = 7;
            // 
            // toolStripBible2
            // 
            this.toolStripBible2.AutoSize = false;
            this.toolStripBible2.CanOverflow = false;
            this.toolStripBible2.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStripBible2.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStripBible2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.Bibles_Go});
            this.toolStripBible2.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
            this.toolStripBible2.Location = new System.Drawing.Point(0, -1);
            this.toolStripBible2.Name = "toolStripBible2";
            this.toolStripBible2.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.toolStripBible2.Size = new System.Drawing.Size(28, 29);
            this.toolStripBible2.TabIndex = 0;
            // 
            // Bibles_Go
            // 
            this.Bibles_Go.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.Bibles_Go.Image = ((System.Drawing.Image)(resources.GetObject("Bibles_Go.Image")));
            this.Bibles_Go.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.Bibles_Go.Name = "Bibles_Go";
            this.Bibles_Go.Size = new System.Drawing.Size(23, 26);
            this.Bibles_Go.Tag = "tick";
            this.Bibles_Go.ToolTipText = "Select typed-in reference";
            this.Bibles_Go.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Bibles_Btn_MouseUp);
            // 
            // BookLookup
            // 
            this.BookLookup.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.BookLookup.FormattingEnabled = true;
            this.BookLookup.Location = new System.Drawing.Point(3, 4);
            this.BookLookup.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.BookLookup.MaxDropDownItems = 40;
            this.BookLookup.Name = "BookLookup";
            this.BookLookup.Size = new System.Drawing.Size(66, 23);
            this.BookLookup.TabIndex = 0;
            this.BookLookup.SelectedIndexChanged += new System.EventHandler(this.BookLookup_SelectedIndexChanged);
            // 
            // TabBibleVersions
            // 
            this.TabBibleVersions.Alignment = System.Windows.Forms.TabAlignment.Bottom;
            this.TabBibleVersions.Appearance = System.Windows.Forms.TabAppearance.Buttons;
            this.TabBibleVersions.Controls.Add(this.tabPage1);
            this.TabBibleVersions.Location = new System.Drawing.Point(3, 108);
            this.TabBibleVersions.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.TabBibleVersions.Name = "TabBibleVersions";
            this.TabBibleVersions.SelectedIndex = 0;
            this.TabBibleVersions.Size = new System.Drawing.Size(98, 22);
            this.TabBibleVersions.TabIndex = 3;
            this.TabBibleVersions.Click += new System.EventHandler(this.TabBibleVersions_Click);
            // 
            // tabPage1
            // 
            this.tabPage1.Location = new System.Drawing.Point(4, 4);
            this.tabPage1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.tabPage1.Size = new System.Drawing.Size(90, 0);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "No Bible";
            // 
            // tabImages
            // 
            this.tabImages.BackColor = System.Drawing.SystemColors.Control;
            this.tabImages.Controls.Add(this.flowLayoutImages);
            this.tabImages.Controls.Add(this.panelImagesTop);
            this.tabImages.Location = new System.Drawing.Point(4, 4);
            this.tabImages.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.tabImages.Name = "tabImages";
            this.tabImages.Padding = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.tabImages.Size = new System.Drawing.Size(295, 354);
            this.tabImages.TabIndex = 1;
            this.tabImages.Text = "Images";
            // 
            // flowLayoutImages
            // 
            this.flowLayoutImages.AutoScroll = true;
            this.flowLayoutImages.Location = new System.Drawing.Point(3, 40);
            this.flowLayoutImages.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.flowLayoutImages.Name = "flowLayoutImages";
            this.flowLayoutImages.Size = new System.Drawing.Size(69, 40);
            this.flowLayoutImages.TabIndex = 2;
            // 
            // panelImagesTop
            // 
            this.panelImagesTop.Controls.Add(this.panelImage1);
            this.panelImagesTop.Controls.Add(this.ImagesFolder);
            this.panelImagesTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelImagesTop.Location = new System.Drawing.Point(3, 4);
            this.panelImagesTop.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.panelImagesTop.Name = "panelImagesTop";
            this.panelImagesTop.Size = new System.Drawing.Size(289, 30);
            this.panelImagesTop.TabIndex = 2;
            // 
            // panelImage1
            // 
            this.panelImage1.Controls.Add(this.toolStripImage1);
            this.panelImage1.Location = new System.Drawing.Point(135, 1);
            this.panelImage1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.panelImage1.Name = "panelImage1";
            this.panelImage1.Size = new System.Drawing.Size(57, 22);
            this.panelImage1.TabIndex = 19;
            // 
            // toolStripImage1
            // 
            this.toolStripImage1.AutoSize = false;
            this.toolStripImage1.CanOverflow = false;
            this.toolStripImage1.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStripImage1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStripImage1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.Image_OpenFolder,
            this.Image_Import});
            this.toolStripImage1.Location = new System.Drawing.Point(0, -1);
            this.toolStripImage1.Name = "toolStripImage1";
            this.toolStripImage1.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.toolStripImage1.Size = new System.Drawing.Size(63, 28);
            this.toolStripImage1.TabIndex = 5;
            // 
            // Image_OpenFolder
            // 
            this.Image_OpenFolder.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.Image_OpenFolder.Image = ((System.Drawing.Image)(resources.GetObject("Image_OpenFolder.Image")));
            this.Image_OpenFolder.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.Image_OpenFolder.Name = "Image_OpenFolder";
            this.Image_OpenFolder.Size = new System.Drawing.Size(23, 25);
            this.Image_OpenFolder.Tag = "";
            this.Image_OpenFolder.ToolTipText = "Open Folder";
            this.Image_OpenFolder.Click += new System.EventHandler(this.Image_OpenFolder_Click);
            // 
            // Image_Import
            // 
            this.Image_Import.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.Image_Import.Image = ((System.Drawing.Image)(resources.GetObject("Image_Import.Image")));
            this.Image_Import.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.Image_Import.Name = "Image_Import";
            this.Image_Import.Size = new System.Drawing.Size(23, 25);
            this.Image_Import.Text = "Import An Image";
            this.Image_Import.Click += new System.EventHandler(this.Image_Import_Click);
            // 
            // ImagesFolder
            // 
            this.ImagesFolder.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ImagesFolder.FormattingEnabled = true;
            this.ImagesFolder.Location = new System.Drawing.Point(0, 0);
            this.ImagesFolder.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.ImagesFolder.MaxDropDownItems = 12;
            this.ImagesFolder.Name = "ImagesFolder";
            this.ImagesFolder.Size = new System.Drawing.Size(128, 23);
            this.ImagesFolder.TabIndex = 5;
            this.ImagesFolder.SelectedIndexChanged += new System.EventHandler(this.ImagesFolder_SelectedIndexChanged);
            // 
            // tabMedia
            // 
            this.tabMedia.BackColor = System.Drawing.SystemColors.Control;
            this.tabMedia.Controls.Add(this.panel11);
            this.tabMedia.Controls.Add(this.MediaList);
            this.tabMedia.Location = new System.Drawing.Point(4, 4);
            this.tabMedia.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.tabMedia.Name = "tabMedia";
            this.tabMedia.Size = new System.Drawing.Size(295, 354);
            this.tabMedia.TabIndex = 6;
            this.tabMedia.Text = "Media";
            // 
            // panel11
            // 
            this.panel11.Controls.Add(this.panelMedia1);
            this.panel11.Controls.Add(this.MediaFolder);
            this.panel11.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel11.Location = new System.Drawing.Point(0, 0);
            this.panel11.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.panel11.Name = "panel11";
            this.panel11.Size = new System.Drawing.Size(295, 30);
            this.panel11.TabIndex = 18;
            // 
            // panelMedia1
            // 
            this.panelMedia1.Controls.Add(this.toolStripMedia1);
            this.panelMedia1.Location = new System.Drawing.Point(134, 4);
            this.panelMedia1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.panelMedia1.Name = "panelMedia1";
            this.panelMedia1.Size = new System.Drawing.Size(58, 22);
            this.panelMedia1.TabIndex = 18;
            // 
            // toolStripMedia1
            // 
            this.toolStripMedia1.AutoSize = false;
            this.toolStripMedia1.CanOverflow = false;
            this.toolStripMedia1.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStripMedia1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStripMedia1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.Media_OpenFolder,
            this.Media_Import});
            this.toolStripMedia1.Location = new System.Drawing.Point(0, -1);
            this.toolStripMedia1.Name = "toolStripMedia1";
            this.toolStripMedia1.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.toolStripMedia1.Size = new System.Drawing.Size(62, 28);
            this.toolStripMedia1.TabIndex = 5;
            // 
            // Media_OpenFolder
            // 
            this.Media_OpenFolder.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.Media_OpenFolder.Image = ((System.Drawing.Image)(resources.GetObject("Media_OpenFolder.Image")));
            this.Media_OpenFolder.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.Media_OpenFolder.Name = "Media_OpenFolder";
            this.Media_OpenFolder.Size = new System.Drawing.Size(23, 25);
            this.Media_OpenFolder.Tag = "";
            this.Media_OpenFolder.ToolTipText = "Open Folder";
            this.Media_OpenFolder.Click += new System.EventHandler(this.Media_OpenFolder_Click);
            // 
            // Media_Import
            // 
            this.Media_Import.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.Media_Import.Image = ((System.Drawing.Image)(resources.GetObject("Media_Import.Image")));
            this.Media_Import.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.Media_Import.Name = "Media_Import";
            this.Media_Import.Size = new System.Drawing.Size(23, 25);
            this.Media_Import.ToolTipText = "Import A Media File into Folder";
            this.Media_Import.Click += new System.EventHandler(this.Media_Import_Click);
            // 
            // MediaFolder
            // 
            this.MediaFolder.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.MediaFolder.FormattingEnabled = true;
            this.MediaFolder.Location = new System.Drawing.Point(3, 4);
            this.MediaFolder.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MediaFolder.MaxDropDownItems = 12;
            this.MediaFolder.Name = "MediaFolder";
            this.MediaFolder.Size = new System.Drawing.Size(123, 23);
            this.MediaFolder.TabIndex = 17;
            this.MediaFolder.SelectedIndexChanged += new System.EventHandler(this.MediaFolder_SelectedIndexChanged);
            // 
            // MediaList
            // 
            this.MediaList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader37,
            this.columnHeader38,
            this.columnHeader39,
            this.columnHeader40,
            this.columnHeader41,
            this.columnHeader42,
            this.columnHeader43});
            this.MediaList.ContextMenuStrip = this.CMenuFiles;
            this.MediaList.FullRowSelect = true;
            this.MediaList.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.MediaList.LabelWrap = false;
            this.MediaList.Location = new System.Drawing.Point(3, 31);
            this.MediaList.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MediaList.Name = "MediaList";
            this.MediaList.ShowItemToolTips = true;
            this.MediaList.Size = new System.Drawing.Size(40, 85);
            this.MediaList.TabIndex = 17;
            this.MediaList.UseCompatibleStateImageBehavior = false;
            this.MediaList.View = System.Windows.Forms.View.Details;
            this.MediaList.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.MediaList_ItemDrag);
            this.MediaList.Enter += new System.EventHandler(this.FormControl_Enter);
            this.MediaList.KeyUp += new System.Windows.Forms.KeyEventHandler(this.MediaList_KeyUp);
            this.MediaList.Leave += new System.EventHandler(this.FormControl_Leave);
            this.MediaList.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.MediaList_MouseDoubleClick);
            this.MediaList.MouseUp += new System.Windows.Forms.MouseEventHandler(this.MediaList_MouseUp);
            // 
            // columnHeader38
            // 
            this.columnHeader38.Width = 0;
            // 
            // columnHeader39
            // 
            this.columnHeader39.Width = 0;
            // 
            // columnHeader40
            // 
            this.columnHeader40.Width = 0;
            // 
            // columnHeader41
            // 
            this.columnHeader41.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.columnHeader41.Width = 0;
            // 
            // columnHeader42
            // 
            this.columnHeader42.Width = 0;
            // 
            // columnHeader43
            // 
            this.columnHeader43.Width = 0;
            // 
            // tabDefault
            // 
            this.tabDefault.BackColor = System.Drawing.SystemColors.Control;
            this.tabDefault.Controls.Add(this.DefPanel);
            this.tabDefault.Location = new System.Drawing.Point(4, 4);
            this.tabDefault.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.tabDefault.Name = "tabDefault";
            this.tabDefault.Size = new System.Drawing.Size(295, 354);
            this.tabDefault.TabIndex = 3;
            this.tabDefault.Text = "Default";
            // 
            // DefPanel
            // 
            this.DefPanel.AutoScroll = true;
            this.DefPanel.Controls.Add(this.panelDefTemplate);
            this.DefPanel.Controls.Add(this.DefApplyDefaultsBtn);
            this.DefPanel.Controls.Add(this.DefgroupBox2);
            this.DefPanel.Controls.Add(this.DefgroupBox3);
            this.DefPanel.Controls.Add(this.DefgroupBox1);
            this.DefPanel.Location = new System.Drawing.Point(3, 4);
            this.DefPanel.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.DefPanel.Name = "DefPanel";
            this.DefPanel.Size = new System.Drawing.Size(268, 320);
            this.DefPanel.TabIndex = 3;
            // 
            // panelDefTemplate
            // 
            this.panelDefTemplate.Controls.Add(this.toolStripDefTemplates);
            this.panelDefTemplate.Location = new System.Drawing.Point(204, 4);
            this.panelDefTemplate.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.panelDefTemplate.Name = "panelDefTemplate";
            this.panelDefTemplate.Size = new System.Drawing.Size(56, 25);
            this.panelDefTemplate.TabIndex = 11;
            // 
            // toolStripDefTemplates
            // 
            this.toolStripDefTemplates.AutoSize = false;
            this.toolStripDefTemplates.CanOverflow = false;
            this.toolStripDefTemplates.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStripDefTemplates.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStripDefTemplates.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.Def_LoadTemplate,
            this.Def_SaveTemplate});
            this.toolStripDefTemplates.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
            this.toolStripDefTemplates.Location = new System.Drawing.Point(0, -1);
            this.toolStripDefTemplates.Name = "toolStripDefTemplates";
            this.toolStripDefTemplates.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.toolStripDefTemplates.Size = new System.Drawing.Size(58, 29);
            this.toolStripDefTemplates.TabIndex = 0;
            // 
            // Def_LoadTemplate
            // 
            this.Def_LoadTemplate.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.Def_LoadTemplate.Image = ((System.Drawing.Image)(resources.GetObject("Def_LoadTemplate.Image")));
            this.Def_LoadTemplate.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.Def_LoadTemplate.Name = "Def_LoadTemplate";
            this.Def_LoadTemplate.Size = new System.Drawing.Size(23, 26);
            this.Def_LoadTemplate.ToolTipText = "Load Settings Template";
            this.Def_LoadTemplate.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Def_Items_MouseUp);
            // 
            // Def_SaveTemplate
            // 
            this.Def_SaveTemplate.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.Def_SaveTemplate.Image = ((System.Drawing.Image)(resources.GetObject("Def_SaveTemplate.Image")));
            this.Def_SaveTemplate.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.Def_SaveTemplate.Name = "Def_SaveTemplate";
            this.Def_SaveTemplate.Size = new System.Drawing.Size(23, 26);
            this.Def_SaveTemplate.ToolTipText = "Save Settings as a Template";
            this.Def_SaveTemplate.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Def_Items_MouseUp);
            // 
            // DefApplyDefaultsBtn
            // 
            this.DefApplyDefaultsBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.DefApplyDefaultsBtn.Location = new System.Drawing.Point(3, 4);
            this.DefApplyDefaultsBtn.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.DefApplyDefaultsBtn.Name = "DefApplyDefaultsBtn";
            this.DefApplyDefaultsBtn.Size = new System.Drawing.Size(201, 26);
            this.DefApplyDefaultsBtn.TabIndex = 0;
            this.DefApplyDefaultsBtn.Text = "Apply to All Except InfoScreens";
            this.toolTip1.SetToolTip(this.DefApplyDefaultsBtn, "Apply Defaults to all on Worship List except InfoScreen Items");
            this.DefApplyDefaultsBtn.Click += new System.EventHandler(this.DefApplyDefaultsBtn_Click);
            // 
            // DefgroupBox2
            // 
            this.DefgroupBox2.Controls.Add(this.panelDef4);
            this.DefgroupBox2.Controls.Add(this.panelDef3);
            this.DefgroupBox2.Location = new System.Drawing.Point(3, 119);
            this.DefgroupBox2.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.DefgroupBox2.Name = "DefgroupBox2";
            this.DefgroupBox2.Padding = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.DefgroupBox2.Size = new System.Drawing.Size(259, 82);
            this.DefgroupBox2.TabIndex = 2;
            this.DefgroupBox2.TabStop = false;
            this.DefgroupBox2.Text = "Default Background";
            // 
            // panelDef4
            // 
            this.panelDef4.Controls.Add(this.toolStripDef4);
            this.panelDef4.Location = new System.Drawing.Point(7, 51);
            this.panelDef4.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.panelDef4.Name = "panelDef4";
            this.panelDef4.Size = new System.Drawing.Size(238, 25);
            this.panelDef4.TabIndex = 11;
            // 
            // toolStripDef4
            // 
            this.toolStripDef4.AutoSize = false;
            this.toolStripDef4.CanOverflow = false;
            this.toolStripDef4.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStripDef4.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStripDef4.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.Def_TransItem,
            this.Def_TransSlides});
            this.toolStripDef4.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
            this.toolStripDef4.Location = new System.Drawing.Point(0, -1);
            this.toolStripDef4.Name = "toolStripDef4";
            this.toolStripDef4.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.toolStripDef4.Size = new System.Drawing.Size(245, 29);
            this.toolStripDef4.TabIndex = 5;
            // 
            // Def_TransItem
            // 
            this.Def_TransItem.AutoSize = false;
            this.Def_TransItem.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.Def_TransItem.MaxDropDownItems = 24;
            this.Def_TransItem.Name = "Def_TransItem";
            this.Def_TransItem.Size = new System.Drawing.Size(116, 23);
            this.Def_TransItem.ToolTipText = "Item Transition";
            this.Def_TransItem.SelectedIndexChanged += new System.EventHandler(this.Def_TransSelectedIndexChanged);
            // 
            // Def_TransSlides
            // 
            this.Def_TransSlides.AutoSize = false;
            this.Def_TransSlides.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.Def_TransSlides.MaxDropDownItems = 24;
            this.Def_TransSlides.Name = "Def_TransSlides";
            this.Def_TransSlides.Size = new System.Drawing.Size(116, 23);
            this.Def_TransSlides.ToolTipText = "Slide Transition";
            this.Def_TransSlides.SelectedIndexChanged += new System.EventHandler(this.Def_TransSelectedIndexChanged);
            // 
            // panelDef3
            // 
            this.panelDef3.Controls.Add(this.toolStripDef3);
            this.panelDef3.Location = new System.Drawing.Point(7, 21);
            this.panelDef3.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.panelDef3.Name = "panelDef3";
            this.panelDef3.Size = new System.Drawing.Size(238, 25);
            this.panelDef3.TabIndex = 10;
            // 
            // toolStripDef3
            // 
            this.toolStripDef3.AutoSize = false;
            this.toolStripDef3.CanOverflow = false;
            this.toolStripDef3.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStripDef3.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStripDef3.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.Def_ImageMode,
            this.Def_NoImage,
            this.Def_BackColour,
            this.Def_AssignMedia});
            this.toolStripDef3.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
            this.toolStripDef3.Location = new System.Drawing.Point(0, -1);
            this.toolStripDef3.Name = "toolStripDef3";
            this.toolStripDef3.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.toolStripDef3.Size = new System.Drawing.Size(245, 29);
            this.toolStripDef3.TabIndex = 0;
            // 
            // Def_ImageMode
            // 
            this.Def_ImageMode.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.Def_ImageMode.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.Def_ImageTile,
            this.Def_ImageCentre,
            this.Def_ImageBestFit});
            this.Def_ImageMode.Image = ((System.Drawing.Image)(resources.GetObject("Def_ImageMode.Image")));
            this.Def_ImageMode.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.Def_ImageMode.Name = "Def_ImageMode";
            this.Def_ImageMode.Size = new System.Drawing.Size(29, 26);
            this.Def_ImageMode.Tag = "2";
            this.Def_ImageMode.ToolTipText = "Background Picture Format";
            this.Def_ImageMode.DropDownItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.Def_ImageMode_DropDownItemClicked);
            // 
            // Def_ImageTile
            // 
            this.Def_ImageTile.Image = ((System.Drawing.Image)(resources.GetObject("Def_ImageTile.Image")));
            this.Def_ImageTile.Name = "Def_ImageTile";
            this.Def_ImageTile.Size = new System.Drawing.Size(148, 22);
            this.Def_ImageTile.Tag = "0";
            this.Def_ImageTile.Text = "Tile Image";
            // 
            // Def_ImageCentre
            // 
            this.Def_ImageCentre.Image = ((System.Drawing.Image)(resources.GetObject("Def_ImageCentre.Image")));
            this.Def_ImageCentre.Name = "Def_ImageCentre";
            this.Def_ImageCentre.Size = new System.Drawing.Size(148, 22);
            this.Def_ImageCentre.Tag = "1";
            this.Def_ImageCentre.Text = "Centre Image";
            // 
            // Def_ImageBestFit
            // 
            this.Def_ImageBestFit.Image = ((System.Drawing.Image)(resources.GetObject("Def_ImageBestFit.Image")));
            this.Def_ImageBestFit.Name = "Def_ImageBestFit";
            this.Def_ImageBestFit.Size = new System.Drawing.Size(148, 22);
            this.Def_ImageBestFit.Tag = "2";
            this.Def_ImageBestFit.Text = "Best Fit Image";
            // 
            // Def_NoImage
            // 
            this.Def_NoImage.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.Def_NoImage.Image = ((System.Drawing.Image)(resources.GetObject("Def_NoImage.Image")));
            this.Def_NoImage.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.Def_NoImage.Name = "Def_NoImage";
            this.Def_NoImage.Size = new System.Drawing.Size(23, 26);
            this.Def_NoImage.ToolTipText = "No Background Picture";
            this.Def_NoImage.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Def_Items_MouseUp);
            // 
            // Def_BackColour
            // 
            this.Def_BackColour.AutoSize = false;
            this.Def_BackColour.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.Def_BackColour.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.Def_BackColour.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.Def_BackColour.Name = "Def_BackColour";
            this.Def_BackColour.Size = new System.Drawing.Size(46, 22);
            this.Def_BackColour.Text = "Colours";
            this.Def_BackColour.ToolTipText = "Background Colours and Patterns";
            this.Def_BackColour.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Def_Items_MouseUp);
            // 
            // Def_AssignMedia
            // 
            this.Def_AssignMedia.AutoSize = false;
            this.Def_AssignMedia.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.Def_AssignMedia.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.Def_AssignMedia.Name = "Def_AssignMedia";
            this.Def_AssignMedia.Size = new System.Drawing.Size(106, 22);
            this.Def_AssignMedia.Text = "Media";
            this.Def_AssignMedia.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Def_Items_MouseUp);
            // 
            // DefgroupBox3
            // 
            this.DefgroupBox3.Controls.Add(this.panel21);
            this.DefgroupBox3.Controls.Add(this.Def_PanelHeight);
            this.DefgroupBox3.Controls.Add(this.panelDef5);
            this.DefgroupBox3.Controls.Add(this.panelDef6);
            this.DefgroupBox3.Controls.Add(this.label5);
            this.DefgroupBox3.Location = new System.Drawing.Point(3, 204);
            this.DefgroupBox3.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.DefgroupBox3.Name = "DefgroupBox3";
            this.DefgroupBox3.Padding = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.DefgroupBox3.Size = new System.Drawing.Size(259, 110);
            this.DefgroupBox3.TabIndex = 3;
            this.DefgroupBox3.TabStop = false;
            this.DefgroupBox3.Text = "Display Panel";
            // 
            // panel21
            // 
            this.panel21.Controls.Add(this.toolStripDef7);
            this.panel21.Location = new System.Drawing.Point(7, 80);
            this.panel21.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.panel21.Name = "panel21";
            this.panel21.Size = new System.Drawing.Size(244, 25);
            this.panel21.TabIndex = 13;
            // 
            // toolStripDef7
            // 
            this.toolStripDef7.AutoSize = false;
            this.toolStripDef7.CanOverflow = false;
            this.toolStripDef7.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStripDef7.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStripDef7.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.Def_PanelFontBold,
            this.Def_PanelFontItalics,
            this.Def_PanelFontUnderline,
            this.Def_PanelFontShadow,
            this.Def_PanelFontOutline,
            this.Def_PanelFontList});
            this.toolStripDef7.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
            this.toolStripDef7.Location = new System.Drawing.Point(0, -1);
            this.toolStripDef7.Name = "toolStripDef7";
            this.toolStripDef7.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.toolStripDef7.Size = new System.Drawing.Size(244, 29);
            this.toolStripDef7.TabIndex = 0;
            // 
            // Def_PanelFontBold
            // 
            this.Def_PanelFontBold.CheckOnClick = true;
            this.Def_PanelFontBold.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.Def_PanelFontBold.Image = ((System.Drawing.Image)(resources.GetObject("Def_PanelFontBold.Image")));
            this.Def_PanelFontBold.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.Def_PanelFontBold.Name = "Def_PanelFontBold";
            this.Def_PanelFontBold.Size = new System.Drawing.Size(23, 26);
            this.Def_PanelFontBold.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Def_Items_MouseUp);
            // 
            // Def_PanelFontItalics
            // 
            this.Def_PanelFontItalics.CheckOnClick = true;
            this.Def_PanelFontItalics.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.Def_PanelFontItalics.Image = ((System.Drawing.Image)(resources.GetObject("Def_PanelFontItalics.Image")));
            this.Def_PanelFontItalics.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.Def_PanelFontItalics.Name = "Def_PanelFontItalics";
            this.Def_PanelFontItalics.Size = new System.Drawing.Size(23, 26);
            this.Def_PanelFontItalics.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Def_Items_MouseUp);
            // 
            // Def_PanelFontUnderline
            // 
            this.Def_PanelFontUnderline.CheckOnClick = true;
            this.Def_PanelFontUnderline.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.Def_PanelFontUnderline.Image = ((System.Drawing.Image)(resources.GetObject("Def_PanelFontUnderline.Image")));
            this.Def_PanelFontUnderline.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.Def_PanelFontUnderline.Name = "Def_PanelFontUnderline";
            this.Def_PanelFontUnderline.Size = new System.Drawing.Size(23, 26);
            this.Def_PanelFontUnderline.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Def_Items_MouseUp);
            // 
            // Def_PanelFontShadow
            // 
            this.Def_PanelFontShadow.CheckOnClick = true;
            this.Def_PanelFontShadow.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.Def_PanelFontShadow.Image = ((System.Drawing.Image)(resources.GetObject("Def_PanelFontShadow.Image")));
            this.Def_PanelFontShadow.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.Def_PanelFontShadow.Name = "Def_PanelFontShadow";
            this.Def_PanelFontShadow.Size = new System.Drawing.Size(23, 26);
            this.Def_PanelFontShadow.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Def_Items_MouseUp);
            // 
            // Def_PanelFontOutline
            // 
            this.Def_PanelFontOutline.CheckOnClick = true;
            this.Def_PanelFontOutline.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.Def_PanelFontOutline.Image = ((System.Drawing.Image)(resources.GetObject("Def_PanelFontOutline.Image")));
            this.Def_PanelFontOutline.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.Def_PanelFontOutline.Name = "Def_PanelFontOutline";
            this.Def_PanelFontOutline.Size = new System.Drawing.Size(23, 26);
            this.Def_PanelFontOutline.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Def_Items_MouseUp);
            // 
            // Def_PanelFontList
            // 
            this.Def_PanelFontList.DropDownWidth = 150;
            this.Def_PanelFontList.Name = "Def_PanelFontList";
            this.Def_PanelFontList.Size = new System.Drawing.Size(107, 29);
            this.Def_PanelFontList.SelectedIndexChanged += new System.EventHandler(this.Def_PanelFontList_SelectedIndexChanged);
            // 
            // Def_PanelHeight
            // 
            this.Def_PanelHeight.Location = new System.Drawing.Point(208, 52);
            this.Def_PanelHeight.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Def_PanelHeight.Maximum = new decimal(new int[] {
            20,
            0,
            0,
            0});
            this.Def_PanelHeight.Minimum = new decimal(new int[] {
            6,
            0,
            0,
            0});
            this.Def_PanelHeight.Name = "Def_PanelHeight";
            this.Def_PanelHeight.Size = new System.Drawing.Size(43, 23);
            this.Def_PanelHeight.TabIndex = 12;
            this.Def_PanelHeight.Value = new decimal(new int[] {
            8,
            0,
            0,
            0});
            this.Def_PanelHeight.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Def_PanelHeight_MouseUp);
            // 
            // panelDef5
            // 
            this.panelDef5.Controls.Add(this.toolStripDef5);
            this.panelDef5.Location = new System.Drawing.Point(7, 21);
            this.panelDef5.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.panelDef5.Name = "panelDef5";
            this.panelDef5.Size = new System.Drawing.Size(238, 25);
            this.panelDef5.TabIndex = 10;
            // 
            // toolStripDef5
            // 
            this.toolStripDef5.AutoSize = false;
            this.toolStripDef5.CanOverflow = false;
            this.toolStripDef5.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStripDef5.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStripDef5.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.Def_PanelAsR1,
            this.Def_PanelTextColour,
            this.toolStripSeparator14,
            this.Def_PanelTransparent,
            this.Def_PanelBackColour});
            this.toolStripDef5.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
            this.toolStripDef5.Location = new System.Drawing.Point(0, -1);
            this.toolStripDef5.Name = "toolStripDef5";
            this.toolStripDef5.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.toolStripDef5.Size = new System.Drawing.Size(238, 29);
            this.toolStripDef5.TabIndex = 0;
            // 
            // Def_PanelAsR1
            // 
            this.Def_PanelAsR1.CheckOnClick = true;
            this.Def_PanelAsR1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.Def_PanelAsR1.Image = ((System.Drawing.Image)(resources.GetObject("Def_PanelAsR1.Image")));
            this.Def_PanelAsR1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.Def_PanelAsR1.Name = "Def_PanelAsR1";
            this.Def_PanelAsR1.Size = new System.Drawing.Size(23, 26);
            this.Def_PanelAsR1.ToolTipText = "Text Colour As Region 1";
            this.Def_PanelAsR1.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Def_Items_MouseUp);
            // 
            // Def_PanelTextColour
            // 
            this.Def_PanelTextColour.AutoSize = false;
            this.Def_PanelTextColour.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.Def_PanelTextColour.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.Def_PanelTextColour.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.Def_PanelTextColour.Name = "Def_PanelTextColour";
            this.Def_PanelTextColour.Size = new System.Drawing.Size(75, 22);
            this.Def_PanelTextColour.Text = "Text Colour";
            this.Def_PanelTextColour.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Def_Items_MouseUp);
            // 
            // toolStripSeparator14
            // 
            this.toolStripSeparator14.Name = "toolStripSeparator14";
            this.toolStripSeparator14.Size = new System.Drawing.Size(6, 29);
            // 
            // Def_PanelTransparent
            // 
            this.Def_PanelTransparent.CheckOnClick = true;
            this.Def_PanelTransparent.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.Def_PanelTransparent.Image = ((System.Drawing.Image)(resources.GetObject("Def_PanelTransparent.Image")));
            this.Def_PanelTransparent.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.Def_PanelTransparent.Name = "Def_PanelTransparent";
            this.Def_PanelTransparent.Size = new System.Drawing.Size(23, 26);
            this.Def_PanelTransparent.ToolTipText = "Transparent Background";
            this.Def_PanelTransparent.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Def_Items_MouseUp);
            // 
            // Def_PanelBackColour
            // 
            this.Def_PanelBackColour.AutoSize = false;
            this.Def_PanelBackColour.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.Def_PanelBackColour.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.Def_PanelBackColour.Image = ((System.Drawing.Image)(resources.GetObject("Def_PanelBackColour.Image")));
            this.Def_PanelBackColour.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.Def_PanelBackColour.Name = "Def_PanelBackColour";
            this.Def_PanelBackColour.Size = new System.Drawing.Size(75, 22);
            this.Def_PanelBackColour.Text = "Back Colour";
            this.Def_PanelBackColour.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Def_Items_MouseUp);
            // 
            // panelDef6
            // 
            this.panelDef6.Controls.Add(this.toolStripDef6);
            this.panelDef6.Location = new System.Drawing.Point(7, 50);
            this.panelDef6.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.panelDef6.Name = "panelDef6";
            this.panelDef6.Size = new System.Drawing.Size(162, 25);
            this.panelDef6.TabIndex = 9;
            // 
            // toolStripDef6
            // 
            this.toolStripDef6.AutoSize = false;
            this.toolStripDef6.CanOverflow = false;
            this.toolStripDef6.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStripDef6.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStripDef6.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.Def_PanelShow,
            this.Def_PanelTitle,
            this.Def_PanelCopyright,
            this.Def_PanelSong,
            this.Def_PanelSlides,
            this.Def_PanelPrevNext});
            this.toolStripDef6.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
            this.toolStripDef6.Location = new System.Drawing.Point(0, -1);
            this.toolStripDef6.Name = "toolStripDef6";
            this.toolStripDef6.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.toolStripDef6.Size = new System.Drawing.Size(170, 29);
            this.toolStripDef6.TabIndex = 0;
            // 
            // Def_PanelShow
            // 
            this.Def_PanelShow.CheckOnClick = true;
            this.Def_PanelShow.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.Def_PanelShow.Image = ((System.Drawing.Image)(resources.GetObject("Def_PanelShow.Image")));
            this.Def_PanelShow.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.Def_PanelShow.Name = "Def_PanelShow";
            this.Def_PanelShow.Size = new System.Drawing.Size(23, 26);
            this.Def_PanelShow.Tag = "list";
            this.Def_PanelShow.ToolTipText = "Show Display Panel";
            this.Def_PanelShow.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Def_Items_MouseUp);
            // 
            // Def_PanelTitle
            // 
            this.Def_PanelTitle.CheckOnClick = true;
            this.Def_PanelTitle.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.Def_PanelTitle.Image = ((System.Drawing.Image)(resources.GetObject("Def_PanelTitle.Image")));
            this.Def_PanelTitle.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.Def_PanelTitle.Name = "Def_PanelTitle";
            this.Def_PanelTitle.Size = new System.Drawing.Size(23, 26);
            this.Def_PanelTitle.ToolTipText = "Show Title";
            this.Def_PanelTitle.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Def_Items_MouseUp);
            // 
            // Def_PanelCopyright
            // 
            this.Def_PanelCopyright.CheckOnClick = true;
            this.Def_PanelCopyright.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.Def_PanelCopyright.Image = ((System.Drawing.Image)(resources.GetObject("Def_PanelCopyright.Image")));
            this.Def_PanelCopyright.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.Def_PanelCopyright.Name = "Def_PanelCopyright";
            this.Def_PanelCopyright.Size = new System.Drawing.Size(23, 26);
            this.Def_PanelCopyright.ToolTipText = "Show Copyright Information";
            this.Def_PanelCopyright.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Def_Items_MouseUp);
            // 
            // Def_PanelSong
            // 
            this.Def_PanelSong.CheckOnClick = true;
            this.Def_PanelSong.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.Def_PanelSong.Image = ((System.Drawing.Image)(resources.GetObject("Def_PanelSong.Image")));
            this.Def_PanelSong.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.Def_PanelSong.Name = "Def_PanelSong";
            this.Def_PanelSong.Size = new System.Drawing.Size(23, 26);
            this.Def_PanelSong.Tag = "add";
            this.Def_PanelSong.ToolTipText = "Show Item Number";
            this.Def_PanelSong.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Def_Items_MouseUp);
            // 
            // Def_PanelSlides
            // 
            this.Def_PanelSlides.CheckOnClick = true;
            this.Def_PanelSlides.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.Def_PanelSlides.Image = ((System.Drawing.Image)(resources.GetObject("Def_PanelSlides.Image")));
            this.Def_PanelSlides.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.Def_PanelSlides.Name = "Def_PanelSlides";
            this.Def_PanelSlides.Size = new System.Drawing.Size(23, 26);
            this.Def_PanelSlides.Tag = "open";
            this.Def_PanelSlides.ToolTipText = "Show Verse/Slide Indicators";
            this.Def_PanelSlides.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Def_Items_MouseUp);
            // 
            // Def_PanelPrevNext
            // 
            this.Def_PanelPrevNext.CheckOnClick = true;
            this.Def_PanelPrevNext.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.Def_PanelPrevNext.Image = ((System.Drawing.Image)(resources.GetObject("Def_PanelPrevNext.Image")));
            this.Def_PanelPrevNext.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.Def_PanelPrevNext.Name = "Def_PanelPrevNext";
            this.Def_PanelPrevNext.Size = new System.Drawing.Size(23, 26);
            this.Def_PanelPrevNext.ToolTipText = "Show Previous/Next Item";
            this.Def_PanelPrevNext.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Def_Items_MouseUp);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(166, 55);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(43, 15);
            this.label5.TabIndex = 7;
            this.label5.Text = "Height";
            // 
            // DefgroupBox1
            // 
            this.DefgroupBox1.Controls.Add(this.panelDef2);
            this.DefgroupBox1.Controls.Add(this.panelDef1);
            this.DefgroupBox1.Location = new System.Drawing.Point(3, 34);
            this.DefgroupBox1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.DefgroupBox1.Name = "DefgroupBox1";
            this.DefgroupBox1.Padding = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.DefgroupBox1.Size = new System.Drawing.Size(259, 82);
            this.DefgroupBox1.TabIndex = 1;
            this.DefgroupBox1.TabStop = false;
            this.DefgroupBox1.Text = "Default Layout";
            // 
            // panelDef2
            // 
            this.panelDef2.Controls.Add(this.toolStripDef2);
            this.panelDef2.Location = new System.Drawing.Point(7, 50);
            this.panelDef2.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.panelDef2.Name = "panelDef2";
            this.panelDef2.Size = new System.Drawing.Size(243, 25);
            this.panelDef2.TabIndex = 9;
            // 
            // toolStripDef2
            // 
            this.toolStripDef2.AutoSize = false;
            this.toolStripDef2.CanOverflow = false;
            this.toolStripDef2.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStripDef2.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStripDef2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.Def_HeadAlign,
            this.toolStripSeparator26,
            this.Def_R1Align,
            this.Def_R1Colour,
            this.toolStripSeparator8,
            this.Def_R2Align,
            this.Def_R2Colour});
            this.toolStripDef2.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
            this.toolStripDef2.Location = new System.Drawing.Point(0, -1);
            this.toolStripDef2.Name = "toolStripDef2";
            this.toolStripDef2.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.toolStripDef2.Size = new System.Drawing.Size(244, 29);
            this.toolStripDef2.TabIndex = 0;
            // 
            // Def_HeadAlign
            // 
            this.Def_HeadAlign.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.Def_HeadAlign.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.Def_HeadAlignAsR1,
            this.Def_HeadAlignAsR2,
            this.Def_HeadAlignLeft,
            this.Def_HeadAlignCentre,
            this.Def_HeadAlignRight});
            this.Def_HeadAlign.Image = ((System.Drawing.Image)(resources.GetObject("Def_HeadAlign.Image")));
            this.Def_HeadAlign.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.Def_HeadAlign.Name = "Def_HeadAlign";
            this.Def_HeadAlign.Size = new System.Drawing.Size(29, 26);
            this.Def_HeadAlign.Tag = "0";
            this.Def_HeadAlign.Text = "toolStripDropDownButton1";
            this.Def_HeadAlign.DropDownItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.Def_HeadAlign_DropDownItemClicked);
            // 
            // Def_HeadAlignAsR1
            // 
            this.Def_HeadAlignAsR1.Image = ((System.Drawing.Image)(resources.GetObject("Def_HeadAlignAsR1.Image")));
            this.Def_HeadAlignAsR1.Name = "Def_HeadAlignAsR1";
            this.Def_HeadAlignAsR1.Size = new System.Drawing.Size(220, 22);
            this.Def_HeadAlignAsR1.Tag = "0";
            this.Def_HeadAlignAsR1.Text = "Headings Align As Region 1";
            // 
            // Def_HeadAlignAsR2
            // 
            this.Def_HeadAlignAsR2.Image = ((System.Drawing.Image)(resources.GetObject("Def_HeadAlignAsR2.Image")));
            this.Def_HeadAlignAsR2.Name = "Def_HeadAlignAsR2";
            this.Def_HeadAlignAsR2.Size = new System.Drawing.Size(220, 22);
            this.Def_HeadAlignAsR2.Tag = "1";
            this.Def_HeadAlignAsR2.Text = "Headings Align As Region 2";
            // 
            // Def_HeadAlignLeft
            // 
            this.Def_HeadAlignLeft.Image = ((System.Drawing.Image)(resources.GetObject("Def_HeadAlignLeft.Image")));
            this.Def_HeadAlignLeft.Name = "Def_HeadAlignLeft";
            this.Def_HeadAlignLeft.Size = new System.Drawing.Size(220, 22);
            this.Def_HeadAlignLeft.Tag = "2";
            this.Def_HeadAlignLeft.Text = "Headings Align Left";
            // 
            // Def_HeadAlignCentre
            // 
            this.Def_HeadAlignCentre.Image = ((System.Drawing.Image)(resources.GetObject("Def_HeadAlignCentre.Image")));
            this.Def_HeadAlignCentre.Name = "Def_HeadAlignCentre";
            this.Def_HeadAlignCentre.Size = new System.Drawing.Size(220, 22);
            this.Def_HeadAlignCentre.Tag = "3";
            this.Def_HeadAlignCentre.Text = "Headings Align Centre";
            // 
            // Def_HeadAlignRight
            // 
            this.Def_HeadAlignRight.Image = ((System.Drawing.Image)(resources.GetObject("Def_HeadAlignRight.Image")));
            this.Def_HeadAlignRight.Name = "Def_HeadAlignRight";
            this.Def_HeadAlignRight.Size = new System.Drawing.Size(220, 22);
            this.Def_HeadAlignRight.Tag = "4";
            this.Def_HeadAlignRight.Text = "Headings Align Right";
            // 
            // toolStripSeparator26
            // 
            this.toolStripSeparator26.Name = "toolStripSeparator26";
            this.toolStripSeparator26.Size = new System.Drawing.Size(6, 29);
            // 
            // Def_R1Align
            // 
            this.Def_R1Align.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.Def_R1Align.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.Def_R1AlignLeft,
            this.Def_R1AlignCentre,
            this.Def_R1AlignRight});
            this.Def_R1Align.Image = ((System.Drawing.Image)(resources.GetObject("Def_R1Align.Image")));
            this.Def_R1Align.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.Def_R1Align.Name = "Def_R1Align";
            this.Def_R1Align.Size = new System.Drawing.Size(29, 26);
            this.Def_R1Align.Tag = "2";
            this.Def_R1Align.DropDownItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.Def_R1Align_DropDownItemClicked);
            // 
            // Def_R1AlignLeft
            // 
            this.Def_R1AlignLeft.Image = ((System.Drawing.Image)(resources.GetObject("Def_R1AlignLeft.Image")));
            this.Def_R1AlignLeft.Name = "Def_R1AlignLeft";
            this.Def_R1AlignLeft.Size = new System.Drawing.Size(189, 22);
            this.Def_R1AlignLeft.Tag = "1";
            this.Def_R1AlignLeft.Text = "Region 1 Align Left";
            // 
            // Def_R1AlignCentre
            // 
            this.Def_R1AlignCentre.Image = ((System.Drawing.Image)(resources.GetObject("Def_R1AlignCentre.Image")));
            this.Def_R1AlignCentre.Name = "Def_R1AlignCentre";
            this.Def_R1AlignCentre.Size = new System.Drawing.Size(189, 22);
            this.Def_R1AlignCentre.Tag = "2";
            this.Def_R1AlignCentre.Text = "Region 1 Align Centre";
            // 
            // Def_R1AlignRight
            // 
            this.Def_R1AlignRight.Image = ((System.Drawing.Image)(resources.GetObject("Def_R1AlignRight.Image")));
            this.Def_R1AlignRight.Name = "Def_R1AlignRight";
            this.Def_R1AlignRight.Size = new System.Drawing.Size(189, 22);
            this.Def_R1AlignRight.Tag = "3";
            this.Def_R1AlignRight.Text = "Region 1 Align Right";
            // 
            // Def_R1Colour
            // 
            this.Def_R1Colour.AutoSize = false;
            this.Def_R1Colour.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.Def_R1Colour.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.Def_R1Colour.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.Def_R1Colour.Name = "Def_R1Colour";
            this.Def_R1Colour.Size = new System.Drawing.Size(54, 22);
            this.Def_R1Colour.Text = "R1 Col";
            this.Def_R1Colour.ToolTipText = "Region 1 Text Colour";
            this.Def_R1Colour.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Def_Items_MouseUp);
            // 
            // toolStripSeparator8
            // 
            this.toolStripSeparator8.Name = "toolStripSeparator8";
            this.toolStripSeparator8.Size = new System.Drawing.Size(6, 29);
            // 
            // Def_R2Align
            // 
            this.Def_R2Align.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.Def_R2Align.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.Def_R2AlignLeft,
            this.Def_R2AlignCentre,
            this.Def_R2AlignRight});
            this.Def_R2Align.Image = ((System.Drawing.Image)(resources.GetObject("Def_R2Align.Image")));
            this.Def_R2Align.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.Def_R2Align.Name = "Def_R2Align";
            this.Def_R2Align.Size = new System.Drawing.Size(29, 26);
            this.Def_R2Align.Tag = "2";
            this.Def_R2Align.DropDownItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.Def_R2Align_DropDownItemClicked);
            // 
            // Def_R2AlignLeft
            // 
            this.Def_R2AlignLeft.Image = ((System.Drawing.Image)(resources.GetObject("Def_R2AlignLeft.Image")));
            this.Def_R2AlignLeft.Name = "Def_R2AlignLeft";
            this.Def_R2AlignLeft.Size = new System.Drawing.Size(189, 22);
            this.Def_R2AlignLeft.Tag = "1";
            this.Def_R2AlignLeft.Text = "Region 2 Align Left";
            // 
            // Def_R2AlignCentre
            // 
            this.Def_R2AlignCentre.Image = ((System.Drawing.Image)(resources.GetObject("Def_R2AlignCentre.Image")));
            this.Def_R2AlignCentre.Name = "Def_R2AlignCentre";
            this.Def_R2AlignCentre.Size = new System.Drawing.Size(189, 22);
            this.Def_R2AlignCentre.Tag = "2";
            this.Def_R2AlignCentre.Text = "Region 2 Align Centre";
            // 
            // Def_R2AlignRight
            // 
            this.Def_R2AlignRight.Image = ((System.Drawing.Image)(resources.GetObject("Def_R2AlignRight.Image")));
            this.Def_R2AlignRight.Name = "Def_R2AlignRight";
            this.Def_R2AlignRight.Size = new System.Drawing.Size(189, 22);
            this.Def_R2AlignRight.Tag = "3";
            this.Def_R2AlignRight.Text = "Region 2 Align Right";
            // 
            // Def_R2Colour
            // 
            this.Def_R2Colour.AutoSize = false;
            this.Def_R2Colour.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.Def_R2Colour.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.Def_R2Colour.Image = ((System.Drawing.Image)(resources.GetObject("Def_R2Colour.Image")));
            this.Def_R2Colour.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.Def_R2Colour.Name = "Def_R2Colour";
            this.Def_R2Colour.Size = new System.Drawing.Size(54, 22);
            this.Def_R2Colour.Text = "R2 Col";
            this.Def_R2Colour.ToolTipText = "Region 2 Text Colour";
            this.Def_R2Colour.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Def_Items_MouseUp);
            // 
            // panelDef1
            // 
            this.panelDef1.Controls.Add(this.toolStripDef1);
            this.panelDef1.Location = new System.Drawing.Point(7, 21);
            this.panelDef1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.panelDef1.Name = "panelDef1";
            this.panelDef1.Size = new System.Drawing.Size(245, 25);
            this.panelDef1.TabIndex = 8;
            // 
            // toolStripDef1
            // 
            this.toolStripDef1.AutoSize = false;
            this.toolStripDef1.CanOverflow = false;
            this.toolStripDef1.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStripDef1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStripDef1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.Def_Head,
            this.Def_Region,
            this.Def_VAlign,
            this.Def_Shadow,
            this.Def_Outline,
            this.Def_Interlace,
            this.Def_Notations,
            this.Def_ToZero});
            this.toolStripDef1.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
            this.toolStripDef1.Location = new System.Drawing.Point(0, -1);
            this.toolStripDef1.Name = "toolStripDef1";
            this.toolStripDef1.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.toolStripDef1.Size = new System.Drawing.Size(245, 29);
            this.toolStripDef1.TabIndex = 0;
            // 
            // Def_Head
            // 
            this.Def_Head.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.Def_Head.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.Def_HeadNoTitles,
            this.Def_HeadAllTitles,
            this.Def_HeadFirstScreen});
            this.Def_Head.Image = ((System.Drawing.Image)(resources.GetObject("Def_Head.Image")));
            this.Def_Head.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.Def_Head.Name = "Def_Head";
            this.Def_Head.Size = new System.Drawing.Size(29, 26);
            this.Def_Head.Tag = "1";
            this.Def_Head.ToolTipText = "Display Title/Verse Headings";
            this.Def_Head.DropDownItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.Def_Head_DropDownItemClicked);
            // 
            // Def_HeadNoTitles
            // 
            this.Def_HeadNoTitles.Image = ((System.Drawing.Image)(resources.GetObject("Def_HeadNoTitles.Image")));
            this.Def_HeadNoTitles.Name = "Def_HeadNoTitles";
            this.Def_HeadNoTitles.Size = new System.Drawing.Size(225, 22);
            this.Def_HeadNoTitles.Tag = "0";
            this.Def_HeadNoTitles.Text = "No Headings";
            // 
            // Def_HeadAllTitles
            // 
            this.Def_HeadAllTitles.Image = ((System.Drawing.Image)(resources.GetObject("Def_HeadAllTitles.Image")));
            this.Def_HeadAllTitles.Name = "Def_HeadAllTitles";
            this.Def_HeadAllTitles.Size = new System.Drawing.Size(225, 22);
            this.Def_HeadAllTitles.Tag = "1";
            this.Def_HeadAllTitles.Text = "Show All Headings";
            // 
            // Def_HeadFirstScreen
            // 
            this.Def_HeadFirstScreen.Image = ((System.Drawing.Image)(resources.GetObject("Def_HeadFirstScreen.Image")));
            this.Def_HeadFirstScreen.Name = "Def_HeadFirstScreen";
            this.Def_HeadFirstScreen.Size = new System.Drawing.Size(225, 22);
            this.Def_HeadFirstScreen.Tag = "2";
            this.Def_HeadFirstScreen.Text = "Heading At First Screen Only";
            // 
            // Def_Region
            // 
            this.Def_Region.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.Def_Region.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.Def_ShowRegion1,
            this.Def_ShowRegion2,
            this.Def_ShowRegionBoth});
            this.Def_Region.Image = ((System.Drawing.Image)(resources.GetObject("Def_Region.Image")));
            this.Def_Region.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.Def_Region.Name = "Def_Region";
            this.Def_Region.Size = new System.Drawing.Size(29, 26);
            this.Def_Region.Tag = "2";
            this.Def_Region.ToolTipText = "Show Region Text";
            this.Def_Region.DropDownItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.Def_Region_DropDownItemClicked);
            // 
            // Def_ShowRegion1
            // 
            this.Def_ShowRegion1.Image = ((System.Drawing.Image)(resources.GetObject("Def_ShowRegion1.Image")));
            this.Def_ShowRegion1.Name = "Def_ShowRegion1";
            this.Def_ShowRegion1.Size = new System.Drawing.Size(157, 22);
            this.Def_ShowRegion1.Tag = "0";
            this.Def_ShowRegion1.Text = "Region 1 Only";
            // 
            // Def_ShowRegion2
            // 
            this.Def_ShowRegion2.Image = ((System.Drawing.Image)(resources.GetObject("Def_ShowRegion2.Image")));
            this.Def_ShowRegion2.Name = "Def_ShowRegion2";
            this.Def_ShowRegion2.Size = new System.Drawing.Size(157, 22);
            this.Def_ShowRegion2.Tag = "1";
            this.Def_ShowRegion2.Text = "Region 2 Only";
            // 
            // Def_ShowRegionBoth
            // 
            this.Def_ShowRegionBoth.Image = ((System.Drawing.Image)(resources.GetObject("Def_ShowRegionBoth.Image")));
            this.Def_ShowRegionBoth.Name = "Def_ShowRegionBoth";
            this.Def_ShowRegionBoth.Size = new System.Drawing.Size(157, 22);
            this.Def_ShowRegionBoth.Tag = "2";
            this.Def_ShowRegionBoth.Text = "Regions 1 and 2";
            // 
            // Def_VAlign
            // 
            this.Def_VAlign.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.Def_VAlign.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.Def_VAlignTop,
            this.Def_VAlignCentre,
            this.Def_VAlignBottom});
            this.Def_VAlign.Image = ((System.Drawing.Image)(resources.GetObject("Def_VAlign.Image")));
            this.Def_VAlign.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.Def_VAlign.Name = "Def_VAlign";
            this.Def_VAlign.Size = new System.Drawing.Size(29, 26);
            this.Def_VAlign.Tag = "1";
            this.Def_VAlign.ToolTipText = "Vertical Alignment";
            this.Def_VAlign.DropDownItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.Def_VAlign_DropDownItemClicked);
            // 
            // Def_VAlignTop
            // 
            this.Def_VAlignTop.Image = ((System.Drawing.Image)(resources.GetObject("Def_VAlignTop.Image")));
            this.Def_VAlignTop.Name = "Def_VAlignTop";
            this.Def_VAlignTop.Size = new System.Drawing.Size(145, 22);
            this.Def_VAlignTop.Tag = "0";
            this.Def_VAlignTop.Text = "Align Top";
            // 
            // Def_VAlignCentre
            // 
            this.Def_VAlignCentre.Image = ((System.Drawing.Image)(resources.GetObject("Def_VAlignCentre.Image")));
            this.Def_VAlignCentre.Name = "Def_VAlignCentre";
            this.Def_VAlignCentre.Size = new System.Drawing.Size(145, 22);
            this.Def_VAlignCentre.Tag = "1";
            this.Def_VAlignCentre.Text = "Align Centre";
            // 
            // Def_VAlignBottom
            // 
            this.Def_VAlignBottom.Image = ((System.Drawing.Image)(resources.GetObject("Def_VAlignBottom.Image")));
            this.Def_VAlignBottom.Name = "Def_VAlignBottom";
            this.Def_VAlignBottom.Size = new System.Drawing.Size(145, 22);
            this.Def_VAlignBottom.Tag = "2";
            this.Def_VAlignBottom.Text = "Align Bottom";
            // 
            // Def_Shadow
            // 
            this.Def_Shadow.CheckOnClick = true;
            this.Def_Shadow.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.Def_Shadow.Image = ((System.Drawing.Image)(resources.GetObject("Def_Shadow.Image")));
            this.Def_Shadow.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.Def_Shadow.Name = "Def_Shadow";
            this.Def_Shadow.Size = new System.Drawing.Size(23, 26);
            this.Def_Shadow.Tag = "open";
            this.Def_Shadow.ToolTipText = "Shadow Font";
            this.Def_Shadow.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Def_Items_MouseUp);
            // 
            // Def_Outline
            // 
            this.Def_Outline.CheckOnClick = true;
            this.Def_Outline.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.Def_Outline.Image = ((System.Drawing.Image)(resources.GetObject("Def_Outline.Image")));
            this.Def_Outline.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.Def_Outline.Name = "Def_Outline";
            this.Def_Outline.Size = new System.Drawing.Size(23, 26);
            this.Def_Outline.Tag = "add";
            this.Def_Outline.ToolTipText = "Outline Font";
            this.Def_Outline.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Def_Items_MouseUp);
            // 
            // Def_Interlace
            // 
            this.Def_Interlace.CheckOnClick = true;
            this.Def_Interlace.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.Def_Interlace.Image = ((System.Drawing.Image)(resources.GetObject("Def_Interlace.Image")));
            this.Def_Interlace.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.Def_Interlace.Name = "Def_Interlace";
            this.Def_Interlace.Size = new System.Drawing.Size(23, 26);
            this.Def_Interlace.ToolTipText = "Interlace Region1/Regions2";
            this.Def_Interlace.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Def_Items_MouseUp);
            // 
            // Def_Notations
            // 
            this.Def_Notations.CheckOnClick = true;
            this.Def_Notations.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.Def_Notations.Image = ((System.Drawing.Image)(resources.GetObject("Def_Notations.Image")));
            this.Def_Notations.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.Def_Notations.Name = "Def_Notations";
            this.Def_Notations.Size = new System.Drawing.Size(23, 26);
            this.Def_Notations.ToolTipText = "Show Notations";
            this.Def_Notations.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Def_Items_MouseUp);
            // 
            // Def_ToZero
            // 
            this.Def_ToZero.CheckOnClick = true;
            this.Def_ToZero.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.Def_ToZero.Image = ((System.Drawing.Image)(resources.GetObject("Def_ToZero.Image")));
            this.Def_ToZero.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.Def_ToZero.Name = "Def_ToZero";
            this.Def_ToZero.Size = new System.Drawing.Size(23, 26);
            this.Def_ToZero.ToolTipText = "To Capo 0";
            this.Def_ToZero.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Def_Items_MouseUp);
            // 
            // tabControlLists
            // 
            this.tabControlLists.Alignment = System.Windows.Forms.TabAlignment.Bottom;
            this.tabControlLists.Controls.Add(this.tabWorshipList);
            this.tabControlLists.Controls.Add(this.tabPraiseBook);
            this.tabControlLists.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControlLists.Location = new System.Drawing.Point(0, 0);
            this.tabControlLists.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.tabControlLists.Name = "tabControlLists";
            this.tabControlLists.Padding = new System.Drawing.Point(5, 3);
            this.tabControlLists.SelectedIndex = 0;
            this.tabControlLists.Size = new System.Drawing.Size(303, 96);
            this.tabControlLists.TabIndex = 0;
            this.tabControlLists.SelectedIndexChanged += new System.EventHandler(this.tabControlLists_SelectedIndexChanged);
            this.tabControlLists.Resize += new System.EventHandler(this.tabControlLists_Resize);
            // 
            // tabWorshipList
            // 
            this.tabWorshipList.BackColor = System.Drawing.SystemColors.Control;
            this.tabWorshipList.Controls.Add(this.panelWorshipList2);
            this.tabWorshipList.Controls.Add(this.panelWorshipList1);
            this.tabWorshipList.Controls.Add(this.WorshipListItems);
            this.tabWorshipList.Controls.Add(this.SessionList);
            this.tabWorshipList.Location = new System.Drawing.Point(4, 4);
            this.tabWorshipList.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.tabWorshipList.Name = "tabWorshipList";
            this.tabWorshipList.Padding = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.tabWorshipList.Size = new System.Drawing.Size(295, 68);
            this.tabWorshipList.TabIndex = 0;
            this.tabWorshipList.Text = "Worship List";
            // 
            // panelWorshipList2
            // 
            this.panelWorshipList2.Controls.Add(this.toolStripWorshipList2);
            this.panelWorshipList2.Location = new System.Drawing.Point(51, 31);
            this.panelWorshipList2.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.panelWorshipList2.Name = "panelWorshipList2";
            this.panelWorshipList2.Size = new System.Drawing.Size(29, 158);
            this.panelWorshipList2.TabIndex = 11;
            // 
            // toolStripWorshipList2
            // 
            this.toolStripWorshipList2.AutoSize = false;
            this.toolStripWorshipList2.CanOverflow = false;
            this.toolStripWorshipList2.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStripWorshipList2.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStripWorshipList2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.WL_Up,
            this.WL_Down,
            this.WL_Delete,
            this.toolStripSeparator6,
            this.WL_Word,
            this.WL_Notes});
            this.toolStripWorshipList2.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.VerticalStackWithOverflow;
            this.toolStripWorshipList2.Location = new System.Drawing.Point(0, 0);
            this.toolStripWorshipList2.Name = "toolStripWorshipList2";
            this.toolStripWorshipList2.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.toolStripWorshipList2.Size = new System.Drawing.Size(29, 159);
            this.toolStripWorshipList2.TabIndex = 0;
            // 
            // WL_Up
            // 
            this.WL_Up.AutoSize = false;
            this.WL_Up.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.WL_Up.Image = ((System.Drawing.Image)(resources.GetObject("WL_Up.Image")));
            this.WL_Up.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.WL_Up.Name = "WL_Up";
            this.WL_Up.Size = new System.Drawing.Size(22, 22);
            this.WL_Up.Tag = "up";
            this.WL_Up.ToolTipText = "Move Item Up";
            this.WL_Up.MouseUp += new System.Windows.Forms.MouseEventHandler(this.WL_Btn_MouseUp);
            // 
            // WL_Down
            // 
            this.WL_Down.AutoSize = false;
            this.WL_Down.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.WL_Down.Image = ((System.Drawing.Image)(resources.GetObject("WL_Down.Image")));
            this.WL_Down.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.WL_Down.Name = "WL_Down";
            this.WL_Down.Size = new System.Drawing.Size(22, 22);
            this.WL_Down.Tag = "down";
            this.WL_Down.ToolTipText = "Move Item Down";
            this.WL_Down.MouseUp += new System.Windows.Forms.MouseEventHandler(this.WL_Btn_MouseUp);
            // 
            // WL_Delete
            // 
            this.WL_Delete.AutoSize = false;
            this.WL_Delete.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.WL_Delete.Image = ((System.Drawing.Image)(resources.GetObject("WL_Delete.Image")));
            this.WL_Delete.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.WL_Delete.Name = "WL_Delete";
            this.WL_Delete.Size = new System.Drawing.Size(22, 22);
            this.WL_Delete.Tag = "delete";
            this.WL_Delete.ToolTipText = "Delete";
            this.WL_Delete.MouseUp += new System.Windows.Forms.MouseEventHandler(this.WL_Btn_MouseUp);
            // 
            // toolStripSeparator6
            // 
            this.toolStripSeparator6.Name = "toolStripSeparator6";
            this.toolStripSeparator6.Size = new System.Drawing.Size(27, 6);
            // 
            // WL_Word
            // 
            this.WL_Word.AutoSize = false;
            this.WL_Word.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.WL_Word.Image = ((System.Drawing.Image)(resources.GetObject("WL_Word.Image")));
            this.WL_Word.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.WL_Word.Name = "WL_Word";
            this.WL_Word.Size = new System.Drawing.Size(22, 22);
            this.WL_Word.Tag = "word";
            this.WL_Word.ToolTipText = "Generate RTF Document";
            this.WL_Word.MouseUp += new System.Windows.Forms.MouseEventHandler(this.WL_Btn_MouseUp);
            // 
            // WL_Notes
            // 
            this.WL_Notes.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.WL_Notes.Image = ((System.Drawing.Image)(resources.GetObject("WL_Notes.Image")));
            this.WL_Notes.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.WL_Notes.Name = "WL_Notes";
            this.WL_Notes.Size = new System.Drawing.Size(27, 20);
            this.WL_Notes.ToolTipText = "Edit Session Notes";
            this.WL_Notes.MouseUp += new System.Windows.Forms.MouseEventHandler(this.WL_Btn_MouseUp);
            // 
            // panelWorshipList1
            // 
            this.panelWorshipList1.Controls.Add(this.toolStripWorshipList1);
            this.panelWorshipList1.Location = new System.Drawing.Point(77, 4);
            this.panelWorshipList1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.panelWorshipList1.Name = "panelWorshipList1";
            this.panelWorshipList1.Size = new System.Drawing.Size(82, 25);
            this.panelWorshipList1.TabIndex = 7;
            // 
            // toolStripWorshipList1
            // 
            this.toolStripWorshipList1.AutoSize = false;
            this.toolStripWorshipList1.CanOverflow = false;
            this.toolStripWorshipList1.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStripWorshipList1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStripWorshipList1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.WL_Manage,
            this.WL_Add,
            this.WL_Open});
            this.toolStripWorshipList1.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
            this.toolStripWorshipList1.Location = new System.Drawing.Point(0, -1);
            this.toolStripWorshipList1.Name = "toolStripWorshipList1";
            this.toolStripWorshipList1.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.toolStripWorshipList1.Size = new System.Drawing.Size(97, 29);
            this.toolStripWorshipList1.TabIndex = 0;
            // 
            // WL_Manage
            // 
            this.WL_Manage.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.WL_Manage.Image = ((System.Drawing.Image)(resources.GetObject("WL_Manage.Image")));
            this.WL_Manage.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.WL_Manage.Name = "WL_Manage";
            this.WL_Manage.Size = new System.Drawing.Size(23, 26);
            this.WL_Manage.Tag = "list";
            this.WL_Manage.ToolTipText = "Manage Worship Lists";
            this.WL_Manage.MouseUp += new System.Windows.Forms.MouseEventHandler(this.WL_Btn_MouseUp);
            // 
            // WL_Add
            // 
            this.WL_Add.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.WL_Add.Image = ((System.Drawing.Image)(resources.GetObject("WL_Add.Image")));
            this.WL_Add.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.WL_Add.Name = "WL_Add";
            this.WL_Add.Size = new System.Drawing.Size(23, 26);
            this.WL_Add.Tag = "add";
            this.WL_Add.ToolTipText = "Add to Worship List";
            this.WL_Add.MouseUp += new System.Windows.Forms.MouseEventHandler(this.WL_Btn_MouseUp);
            // 
            // WL_Open
            // 
            this.WL_Open.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.WL_Open.Image = ((System.Drawing.Image)(resources.GetObject("WL_Open.Image")));
            this.WL_Open.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.WL_Open.Name = "WL_Open";
            this.WL_Open.Size = new System.Drawing.Size(23, 26);
            this.WL_Open.Tag = "open";
            this.WL_Open.ToolTipText = "Add External Document to Worship List";
            this.WL_Open.MouseUp += new System.Windows.Forms.MouseEventHandler(this.WL_Btn_MouseUp);
            // 
            // WorshipListItems
            // 
            this.WorshipListItems.AllowDrop = true;
            this.WorshipListItems.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader8,
            this.columnHeader9,
            this.columnHeader10,
            this.columnHeader11,
            this.columnHeader12,
            this.columnHeader13,
            this.columnHeader14});
            this.WorshipListItems.ContextMenuStrip = this.CMenuWorship;
            this.WorshipListItems.FullRowSelect = true;
            this.WorshipListItems.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.WorshipListItems.LabelWrap = false;
            this.WorshipListItems.Location = new System.Drawing.Point(3, 31);
            this.WorshipListItems.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.WorshipListItems.Name = "WorshipListItems";
            this.WorshipListItems.ShowItemToolTips = true;
            this.WorshipListItems.Size = new System.Drawing.Size(40, 85);
            this.WorshipListItems.SmallImageList = this.imageListSys;
            this.WorshipListItems.TabIndex = 1;
            this.WorshipListItems.UseCompatibleStateImageBehavior = false;
            this.WorshipListItems.View = System.Windows.Forms.View.Details;
            this.WorshipListItems.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.WorshipList_ItemDrag);
            this.WorshipListItems.DragDrop += new System.Windows.Forms.DragEventHandler(this.WorshipList_DragDrop);
            this.WorshipListItems.DragEnter += new System.Windows.Forms.DragEventHandler(this.WorshipList_DragEnter);
            this.WorshipListItems.DragOver += new System.Windows.Forms.DragEventHandler(this.WorshipList_DragOver);
            this.WorshipListItems.DragLeave += new System.EventHandler(this.WorshipList_DragLeave);
            this.WorshipListItems.DoubleClick += new System.EventHandler(this.WorshipListItems_DoubleClick);
            this.WorshipListItems.Enter += new System.EventHandler(this.FormControl_Enter);
            this.WorshipListItems.KeyUp += new System.Windows.Forms.KeyEventHandler(this.WorshipList_KeyUp);
            this.WorshipListItems.Leave += new System.EventHandler(this.FormControl_Leave);
            this.WorshipListItems.MouseUp += new System.Windows.Forms.MouseEventHandler(this.WorshipList_MouseUp);
            // 
            // columnHeader9
            // 
            this.columnHeader9.Width = 0;
            // 
            // columnHeader10
            // 
            this.columnHeader10.Width = 0;
            // 
            // columnHeader11
            // 
            this.columnHeader11.Width = 0;
            // 
            // columnHeader12
            // 
            this.columnHeader12.Width = 0;
            // 
            // columnHeader13
            // 
            this.columnHeader13.Width = 0;
            // 
            // columnHeader14
            // 
            this.columnHeader14.Width = 0;
            // 
            // CMenuWorship
            // 
            this.CMenuWorship.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.CMenuWorship_SelectAll,
            this.CMenuWorship_UnselectAll,
            this.CMenuWorship_Clear,
            this.toolStripSeparator39,
            this.CMenuWorship_Edit,
            this.CMenuWorship_Play,
            this.toolStripSeparator37,
            this.CMenuWorship_AddUsages});
            this.CMenuWorship.Name = "ContextMenuBibleText";
            this.CMenuWorship.Size = new System.Drawing.Size(186, 148);
            // 
            // CMenuWorship_SelectAll
            // 
            this.CMenuWorship_SelectAll.Name = "CMenuWorship_SelectAll";
            this.CMenuWorship_SelectAll.Size = new System.Drawing.Size(185, 22);
            this.CMenuWorship_SelectAll.Text = "Select &All";
            this.CMenuWorship_SelectAll.Click += new System.EventHandler(this.CMenuWorship_SelectAll_Click);
            // 
            // CMenuWorship_UnselectAll
            // 
            this.CMenuWorship_UnselectAll.Name = "CMenuWorship_UnselectAll";
            this.CMenuWorship_UnselectAll.Size = new System.Drawing.Size(185, 22);
            this.CMenuWorship_UnselectAll.Text = "&Unselect All";
            this.CMenuWorship_UnselectAll.Click += new System.EventHandler(this.CMenuWorship_UnselectAll_Click);
            // 
            // CMenuWorship_Clear
            // 
            this.CMenuWorship_Clear.Name = "CMenuWorship_Clear";
            this.CMenuWorship_Clear.Size = new System.Drawing.Size(185, 22);
            this.CMenuWorship_Clear.Text = "Clear Worship List";
            this.CMenuWorship_Clear.Click += new System.EventHandler(this.CMenuWorship_Clear_Click);
            // 
            // toolStripSeparator39
            // 
            this.toolStripSeparator39.Name = "toolStripSeparator39";
            this.toolStripSeparator39.Size = new System.Drawing.Size(182, 6);
            // 
            // CMenuWorship_Edit
            // 
            this.CMenuWorship_Edit.Name = "CMenuWorship_Edit";
            this.CMenuWorship_Edit.Size = new System.Drawing.Size(185, 22);
            this.CMenuWorship_Edit.Text = "Edit item";
            this.CMenuWorship_Edit.Click += new System.EventHandler(this.CMenuWorship_Edit_Click);
            // 
            // CMenuWorship_Play
            // 
            this.CMenuWorship_Play.Name = "CMenuWorship_Play";
            this.CMenuWorship_Play.Size = new System.Drawing.Size(185, 22);
            this.CMenuWorship_Play.Text = "Play Media";
            this.CMenuWorship_Play.Click += new System.EventHandler(this.CMenuWorship_Play_Click);
            // 
            // toolStripSeparator37
            // 
            this.toolStripSeparator37.Name = "toolStripSeparator37";
            this.toolStripSeparator37.Size = new System.Drawing.Size(182, 6);
            // 
            // CMenuWorship_AddUsages
            // 
            this.CMenuWorship_AddUsages.Name = "CMenuWorship_AddUsages";
            this.CMenuWorship_AddUsages.Size = new System.Drawing.Size(185, 22);
            this.CMenuWorship_AddUsages.Text = "Add Songs to Usages";
            this.CMenuWorship_AddUsages.Click += new System.EventHandler(this.CMenuWorship_AddUsages_Click);
            // 
            // imageListSys
            // 
            this.imageListSys.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            this.imageListSys.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageListSys.ImageStream")));
            this.imageListSys.TransparentColor = System.Drawing.Color.Transparent;
            this.imageListSys.Images.SetKeyName(0, "ES Icon 32 Blue.ico");
            this.imageListSys.Images.SetKeyName(1, "ES Icon 32 Blue - Highlight.ico");
            this.imageListSys.Images.SetKeyName(2, "PPImg.gif");
            this.imageListSys.Images.SetKeyName(3, "PPImg - Highlight.gif");
            this.imageListSys.Images.SetKeyName(4, "Bible.gif");
            this.imageListSys.Images.SetKeyName(5, "Bible - Hightlight.gif");
            this.imageListSys.Images.SetKeyName(6, "notebook.gif");
            this.imageListSys.Images.SetKeyName(7, "notebook-highlight.gif");
            this.imageListSys.Images.SetKeyName(8, "Info_Sym.gif");
            this.imageListSys.Images.SetKeyName(9, "Info_Sym highlight.gif");
            this.imageListSys.Images.SetKeyName(10, "word.gif");
            this.imageListSys.Images.SetKeyName(11, "word-highlight.gif");
            this.imageListSys.Images.SetKeyName(12, "singlescreen.gif");
            this.imageListSys.Images.SetKeyName(13, "dualscreens.gif");
            this.imageListSys.Images.SetKeyName(14, "keyboard.gif");
            this.imageListSys.Images.SetKeyName(15, "BlackScreen-Pressed.gif");
            this.imageListSys.Images.SetKeyName(16, "BlackScreen-Red.gif");
            this.imageListSys.Images.SetKeyName(17, "BlueScreen-Pressed.gif");
            this.imageListSys.Images.SetKeyName(18, "BlueScreen-Red.gif");
            this.imageListSys.Images.SetKeyName(19, "folder.gif");
            this.imageListSys.Images.SetKeyName(20, "pic-bestfit.gif");
            this.imageListSys.Images.SetKeyName(21, "Bible.gif");
            this.imageListSys.Images.SetKeyName(22, "options.gif");
            this.imageListSys.Images.SetKeyName(23, "Info_Sym.gif");
            this.imageListSys.Images.SetKeyName(24, "PPImg.gif");
            this.imageListSys.Images.SetKeyName(25, "Tick.gif");
            this.imageListSys.Images.SetKeyName(26, "NumNewScreen.gif");
            this.imageListSys.Images.SetKeyName(27, "ques.gif");
            this.imageListSys.Images.SetKeyName(28, "Media.gif");
            this.imageListSys.Images.SetKeyName(29, "Media-highlight.gif");
            this.imageListSys.Images.SetKeyName(30, "camcorder.gif");
            this.imageListSys.Images.SetKeyName(31, "camcorder-Red.gif");
            // 
            // SessionList
            // 
            this.SessionList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.SessionList.FormattingEnabled = true;
            this.SessionList.Location = new System.Drawing.Point(3, 4);
            this.SessionList.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.SessionList.MaxDropDownItems = 12;
            this.SessionList.Name = "SessionList";
            this.SessionList.Size = new System.Drawing.Size(66, 23);
            this.SessionList.TabIndex = 0;
            this.SessionList.SelectedValueChanged += new System.EventHandler(this.SessionList_SelectedValueChanged);
            // 
            // tabPraiseBook
            // 
            this.tabPraiseBook.BackColor = System.Drawing.SystemColors.Control;
            this.tabPraiseBook.Controls.Add(this.panelPraiseBook2);
            this.tabPraiseBook.Controls.Add(this.panelPraiseBook1);
            this.tabPraiseBook.Controls.Add(this.PraiseBookItems);
            this.tabPraiseBook.Controls.Add(this.PraiseBook);
            this.tabPraiseBook.Location = new System.Drawing.Point(4, 4);
            this.tabPraiseBook.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.tabPraiseBook.Name = "tabPraiseBook";
            this.tabPraiseBook.Padding = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.tabPraiseBook.Size = new System.Drawing.Size(295, 68);
            this.tabPraiseBook.TabIndex = 1;
            this.tabPraiseBook.Text = "Praise Book";
            // 
            // panelPraiseBook2
            // 
            this.panelPraiseBook2.Controls.Add(this.toolStripPraiseBook2);
            this.panelPraiseBook2.Location = new System.Drawing.Point(51, 31);
            this.panelPraiseBook2.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.panelPraiseBook2.Name = "panelPraiseBook2";
            this.panelPraiseBook2.Size = new System.Drawing.Size(29, 99);
            this.panelPraiseBook2.TabIndex = 12;
            // 
            // toolStripPraiseBook2
            // 
            this.toolStripPraiseBook2.AutoSize = false;
            this.toolStripPraiseBook2.CanOverflow = false;
            this.toolStripPraiseBook2.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStripPraiseBook2.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStripPraiseBook2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripSeparator22,
            this.PB_Delete,
            this.toolStripSeparator7,
            this.PB_Word,
            this.PB_Html});
            this.toolStripPraiseBook2.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.VerticalStackWithOverflow;
            this.toolStripPraiseBook2.Location = new System.Drawing.Point(0, 0);
            this.toolStripPraiseBook2.Name = "toolStripPraiseBook2";
            this.toolStripPraiseBook2.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.toolStripPraiseBook2.Size = new System.Drawing.Size(29, 101);
            this.toolStripPraiseBook2.TabIndex = 0;
            // 
            // toolStripSeparator22
            // 
            this.toolStripSeparator22.Name = "toolStripSeparator22";
            this.toolStripSeparator22.Size = new System.Drawing.Size(27, 6);
            // 
            // PB_Delete
            // 
            this.PB_Delete.AutoSize = false;
            this.PB_Delete.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.PB_Delete.Image = ((System.Drawing.Image)(resources.GetObject("PB_Delete.Image")));
            this.PB_Delete.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.PB_Delete.Name = "PB_Delete";
            this.PB_Delete.Size = new System.Drawing.Size(23, 22);
            this.PB_Delete.Tag = "delete";
            this.PB_Delete.ToolTipText = "Delete";
            this.PB_Delete.MouseUp += new System.Windows.Forms.MouseEventHandler(this.PB_Btn_MouseUp);
            // 
            // toolStripSeparator7
            // 
            this.toolStripSeparator7.Name = "toolStripSeparator7";
            this.toolStripSeparator7.Size = new System.Drawing.Size(27, 6);
            // 
            // PB_Word
            // 
            this.PB_Word.AutoSize = false;
            this.PB_Word.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.PB_Word.Image = ((System.Drawing.Image)(resources.GetObject("PB_Word.Image")));
            this.PB_Word.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.PB_Word.Name = "PB_Word";
            this.PB_Word.Size = new System.Drawing.Size(22, 22);
            this.PB_Word.Tag = "word";
            this.PB_Word.ToolTipText = "Generate RTF Document";
            this.PB_Word.MouseUp += new System.Windows.Forms.MouseEventHandler(this.PB_Btn_MouseUp);
            // 
            // PB_Html
            // 
            this.PB_Html.AutoSize = false;
            this.PB_Html.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.PB_Html.Image = ((System.Drawing.Image)(resources.GetObject("PB_Html.Image")));
            this.PB_Html.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.PB_Html.Name = "PB_Html";
            this.PB_Html.Size = new System.Drawing.Size(22, 22);
            this.PB_Html.Tag = "ie";
            this.PB_Html.ToolTipText = "Generate HTML Document";
            this.PB_Html.MouseUp += new System.Windows.Forms.MouseEventHandler(this.PB_Btn_MouseUp);
            // 
            // panelPraiseBook1
            // 
            this.panelPraiseBook1.Controls.Add(this.toolStripPraiseBook1);
            this.panelPraiseBook1.Location = new System.Drawing.Point(77, 4);
            this.panelPraiseBook1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.panelPraiseBook1.Name = "panelPraiseBook1";
            this.panelPraiseBook1.Size = new System.Drawing.Size(82, 25);
            this.panelPraiseBook1.TabIndex = 10;
            // 
            // toolStripPraiseBook1
            // 
            this.toolStripPraiseBook1.AutoSize = false;
            this.toolStripPraiseBook1.CanOverflow = false;
            this.toolStripPraiseBook1.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStripPraiseBook1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStripPraiseBook1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.PB_Manage,
            this.PB_Add,
            this.PB_WordCount});
            this.toolStripPraiseBook1.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
            this.toolStripPraiseBook1.Location = new System.Drawing.Point(0, -1);
            this.toolStripPraiseBook1.Name = "toolStripPraiseBook1";
            this.toolStripPraiseBook1.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.toolStripPraiseBook1.Size = new System.Drawing.Size(97, 29);
            this.toolStripPraiseBook1.TabIndex = 0;
            // 
            // PB_Manage
            // 
            this.PB_Manage.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.PB_Manage.Image = ((System.Drawing.Image)(resources.GetObject("PB_Manage.Image")));
            this.PB_Manage.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.PB_Manage.Name = "PB_Manage";
            this.PB_Manage.Size = new System.Drawing.Size(23, 26);
            this.PB_Manage.Tag = "list";
            this.PB_Manage.ToolTipText = "Manage PraiseBooks";
            this.PB_Manage.MouseUp += new System.Windows.Forms.MouseEventHandler(this.PB_Btn_MouseUp);
            // 
            // PB_Add
            // 
            this.PB_Add.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.PB_Add.Image = ((System.Drawing.Image)(resources.GetObject("PB_Add.Image")));
            this.PB_Add.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.PB_Add.Name = "PB_Add";
            this.PB_Add.Size = new System.Drawing.Size(23, 26);
            this.PB_Add.Tag = "add";
            this.PB_Add.ToolTipText = "Add to PraiseBook";
            this.PB_Add.MouseUp += new System.Windows.Forms.MouseEventHandler(this.PB_Btn_MouseUp);
            // 
            // PB_WordCount
            // 
            this.PB_WordCount.CheckOnClick = true;
            this.PB_WordCount.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.PB_WordCount.Image = ((System.Drawing.Image)(resources.GetObject("PB_WordCount.Image")));
            this.PB_WordCount.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.PB_WordCount.Name = "PB_WordCount";
            this.PB_WordCount.Size = new System.Drawing.Size(23, 26);
            this.PB_WordCount.Tag = "wordcount";
            this.PB_WordCount.ToolTipText = "Sort by CJK Word Count";
            this.PB_WordCount.MouseUp += new System.Windows.Forms.MouseEventHandler(this.PB_Btn_MouseUp);
            // 
            // PraiseBookItems
            // 
            this.PraiseBookItems.AllowDrop = true;
            this.PraiseBookItems.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader17,
            this.columnHeader18,
            this.columnHeader19,
            this.columnHeader20,
            this.columnHeader21,
            this.columnHeader22});
            this.PraiseBookItems.ContextMenuStrip = this.CMenuPraiseB;
            this.PraiseBookItems.FullRowSelect = true;
            this.PraiseBookItems.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.PraiseBookItems.LabelWrap = false;
            this.PraiseBookItems.Location = new System.Drawing.Point(3, 31);
            this.PraiseBookItems.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.PraiseBookItems.Name = "PraiseBookItems";
            this.PraiseBookItems.ShowItemToolTips = true;
            this.PraiseBookItems.Size = new System.Drawing.Size(40, 85);
            this.PraiseBookItems.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.PraiseBookItems.TabIndex = 1;
            this.PraiseBookItems.UseCompatibleStateImageBehavior = false;
            this.PraiseBookItems.View = System.Windows.Forms.View.Details;
            this.PraiseBookItems.DragDrop += new System.Windows.Forms.DragEventHandler(this.PraiseBookItems_DragDrop);
            this.PraiseBookItems.DragEnter += new System.Windows.Forms.DragEventHandler(this.PraiseBookItems_DragEnter);
            this.PraiseBookItems.Enter += new System.EventHandler(this.FormControl_Enter);
            this.PraiseBookItems.KeyUp += new System.Windows.Forms.KeyEventHandler(this.PraiseBookItems_KeyUp);
            this.PraiseBookItems.Leave += new System.EventHandler(this.FormControl_Leave);
            this.PraiseBookItems.MouseUp += new System.Windows.Forms.MouseEventHandler(this.PraiseBookItems_MouseUp);
            // 
            // columnHeader17
            // 
            this.columnHeader17.Width = 0;
            // 
            // columnHeader18
            // 
            this.columnHeader18.Width = 0;
            // 
            // columnHeader19
            // 
            this.columnHeader19.Width = 0;
            // 
            // columnHeader20
            // 
            this.columnHeader20.Width = 0;
            // 
            // columnHeader21
            // 
            this.columnHeader21.Width = 0;
            // 
            // columnHeader22
            // 
            this.columnHeader22.Width = 0;
            // 
            // CMenuPraiseB
            // 
            this.CMenuPraiseB.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.CMenuPraiseB_SelectAll,
            this.CMenuPraiseB_UnselectAll,
            this.CMenuPraiseB_Clear,
            this.toolStripSeparator36,
            this.CMenuPraiseB_Edit});
            this.CMenuPraiseB.Name = "ContextMenuBibleText";
            this.CMenuPraiseB.Size = new System.Drawing.Size(184, 98);
            // 
            // CMenuPraiseB_SelectAll
            // 
            this.CMenuPraiseB_SelectAll.Name = "CMenuPraiseB_SelectAll";
            this.CMenuPraiseB_SelectAll.Size = new System.Drawing.Size(183, 22);
            this.CMenuPraiseB_SelectAll.Text = "Select &All";
            this.CMenuPraiseB_SelectAll.Click += new System.EventHandler(this.CMenuPraiseB_SelectAll_Click);
            // 
            // CMenuPraiseB_UnselectAll
            // 
            this.CMenuPraiseB_UnselectAll.Name = "CMenuPraiseB_UnselectAll";
            this.CMenuPraiseB_UnselectAll.Size = new System.Drawing.Size(183, 22);
            this.CMenuPraiseB_UnselectAll.Text = "&Unselect All";
            this.CMenuPraiseB_UnselectAll.Click += new System.EventHandler(this.CMenuPraiseB_UnselectAll_Click);
            // 
            // CMenuPraiseB_Clear
            // 
            this.CMenuPraiseB_Clear.Name = "CMenuPraiseB_Clear";
            this.CMenuPraiseB_Clear.Size = new System.Drawing.Size(183, 22);
            this.CMenuPraiseB_Clear.Text = "Clear PraiseBook List";
            this.CMenuPraiseB_Clear.Click += new System.EventHandler(this.CMenuPraiseB_Clear_Click);
            // 
            // toolStripSeparator36
            // 
            this.toolStripSeparator36.Name = "toolStripSeparator36";
            this.toolStripSeparator36.Size = new System.Drawing.Size(180, 6);
            // 
            // CMenuPraiseB_Edit
            // 
            this.CMenuPraiseB_Edit.Name = "CMenuPraiseB_Edit";
            this.CMenuPraiseB_Edit.Size = new System.Drawing.Size(183, 22);
            this.CMenuPraiseB_Edit.Text = "Edit item";
            this.CMenuPraiseB_Edit.Click += new System.EventHandler(this.CMenuPraiseB_Edit_Click);
            // 
            // PraiseBook
            // 
            this.PraiseBook.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.PraiseBook.FormattingEnabled = true;
            this.PraiseBook.Location = new System.Drawing.Point(3, 4);
            this.PraiseBook.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.PraiseBook.MaxDropDownItems = 12;
            this.PraiseBook.Name = "PraiseBook";
            this.PraiseBook.Size = new System.Drawing.Size(66, 23);
            this.PraiseBook.TabIndex = 0;
            this.PraiseBook.SelectedIndexChanged += new System.EventHandler(this.PraiseBook_SelectedIndexChanged);
            // 
            // splitContainer2
            // 
            this.splitContainer2.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.splitContainer2.Name = "splitContainer2";
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.splitContainerPreview);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.splitContainerOutput);
            this.splitContainer2.Size = new System.Drawing.Size(547, 490);
            this.splitContainer2.SplitterDistance = 326;
            this.splitContainer2.SplitterWidth = 3;
            this.splitContainer2.TabIndex = 0;
            this.splitContainer2.Text = "splitContainer2";
            // 
            // splitContainerPreview
            // 
            this.splitContainerPreview.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerPreview.Location = new System.Drawing.Point(0, 0);
            this.splitContainerPreview.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.splitContainerPreview.Name = "splitContainerPreview";
            this.splitContainerPreview.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainerPreview.Panel1
            // 
            this.splitContainerPreview.Panel1.Controls.Add(this.panelPreviewTop);
            this.splitContainerPreview.Panel1.Controls.Add(this.panel9);
            this.splitContainerPreview.Panel1MinSize = 50;
            // 
            // splitContainerPreview.Panel2
            // 
            this.splitContainerPreview.Panel2.BackColor = System.Drawing.SystemColors.Control;
            this.splitContainerPreview.Panel2.Controls.Add(this.panelPreviewBottom);
            this.splitContainerPreview.Panel2.Controls.Add(this.panel7);
            this.splitContainerPreview.Panel2.Controls.Add(this.panel1);
            this.splitContainerPreview.Size = new System.Drawing.Size(322, 486);
            this.splitContainerPreview.SplitterDistance = 429;
            this.splitContainerPreview.TabIndex = 0;
            this.splitContainerPreview.Text = "splitContainer3";
            // 
            // panelPreviewTop
            // 
            this.panelPreviewTop.Controls.Add(this.flowLayoutPreviewPowerPoint);
            this.panelPreviewTop.Controls.Add(this.IndPanel);
            this.panelPreviewTop.Controls.Add(this.PreviewInfo);
            this.panelPreviewTop.Controls.Add(this.flowLayoutPreviewLyrics);
            this.panelPreviewTop.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelPreviewTop.Location = new System.Drawing.Point(0, 25);
            this.panelPreviewTop.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.panelPreviewTop.Name = "panelPreviewTop";
            this.panelPreviewTop.Size = new System.Drawing.Size(322, 404);
            this.panelPreviewTop.TabIndex = 1;
            this.panelPreviewTop.Resize += new System.EventHandler(this.panelPreviewTop_Resize);
            // 
            // flowLayoutPreviewPowerPoint
            // 
            this.flowLayoutPreviewPowerPoint.AutoScroll = true;
            this.flowLayoutPreviewPowerPoint.Location = new System.Drawing.Point(3, 151);
            this.flowLayoutPreviewPowerPoint.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.flowLayoutPreviewPowerPoint.Name = "flowLayoutPreviewPowerPoint";
            this.flowLayoutPreviewPowerPoint.Size = new System.Drawing.Size(69, 40);
            this.flowLayoutPreviewPowerPoint.TabIndex = 5;
            // 
            // IndPanel
            // 
            this.IndPanel.AutoScroll = true;
            this.IndPanel.Controls.Add(this.panelIndTemplate);
            this.IndPanel.Controls.Add(this.IndgroupBox4);
            this.IndPanel.Controls.Add(this.IndgroupBox3);
            this.IndPanel.Controls.Add(this.IndgroupBox2);
            this.IndPanel.Controls.Add(this.IndgroupBox1);
            this.IndPanel.Controls.Add(this.Ind_checkBox);
            this.IndPanel.Location = new System.Drawing.Point(21, 8);
            this.IndPanel.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.IndPanel.Name = "IndPanel";
            this.IndPanel.Size = new System.Drawing.Size(296, 390);
            this.IndPanel.TabIndex = 2;
            // 
            // panelIndTemplate
            // 
            this.panelIndTemplate.Controls.Add(this.toolStripIndTemplates);
            this.panelIndTemplate.Location = new System.Drawing.Point(175, 4);
            this.panelIndTemplate.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.panelIndTemplate.Name = "panelIndTemplate";
            this.panelIndTemplate.Size = new System.Drawing.Size(56, 25);
            this.panelIndTemplate.TabIndex = 12;
            // 
            // toolStripIndTemplates
            // 
            this.toolStripIndTemplates.AutoSize = false;
            this.toolStripIndTemplates.CanOverflow = false;
            this.toolStripIndTemplates.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStripIndTemplates.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStripIndTemplates.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.Ind_LoadTemplate,
            this.Ind_SaveTemplate});
            this.toolStripIndTemplates.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
            this.toolStripIndTemplates.Location = new System.Drawing.Point(0, -1);
            this.toolStripIndTemplates.Name = "toolStripIndTemplates";
            this.toolStripIndTemplates.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.toolStripIndTemplates.Size = new System.Drawing.Size(58, 29);
            this.toolStripIndTemplates.TabIndex = 0;
            // 
            // Ind_LoadTemplate
            // 
            this.Ind_LoadTemplate.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.Ind_LoadTemplate.Image = ((System.Drawing.Image)(resources.GetObject("Ind_LoadTemplate.Image")));
            this.Ind_LoadTemplate.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.Ind_LoadTemplate.Name = "Ind_LoadTemplate";
            this.Ind_LoadTemplate.Size = new System.Drawing.Size(23, 26);
            this.Ind_LoadTemplate.ToolTipText = "Load Settings Template";
            this.Ind_LoadTemplate.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Ind_Items_MouseUp);
            // 
            // Ind_SaveTemplate
            // 
            this.Ind_SaveTemplate.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.Ind_SaveTemplate.Image = ((System.Drawing.Image)(resources.GetObject("Ind_SaveTemplate.Image")));
            this.Ind_SaveTemplate.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.Ind_SaveTemplate.Name = "Ind_SaveTemplate";
            this.Ind_SaveTemplate.Size = new System.Drawing.Size(23, 26);
            this.Ind_SaveTemplate.ToolTipText = "Save Settings as a Template";
            this.Ind_SaveTemplate.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Ind_Items_MouseUp);
            // 
            // IndgroupBox4
            // 
            this.IndgroupBox4.Controls.Add(this.panelInd7);
            this.IndgroupBox4.Controls.Add(this.Ind_Reg2SizeUpDown);
            this.IndgroupBox4.Controls.Add(this.label6);
            this.IndgroupBox4.Controls.Add(this.Ind_Reg2TopUpDown);
            this.IndgroupBox4.Controls.Add(this.panelInd6);
            this.IndgroupBox4.Controls.Add(this.label7);
            this.IndgroupBox4.Location = new System.Drawing.Point(7, 305);
            this.IndgroupBox4.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.IndgroupBox4.Name = "IndgroupBox4";
            this.IndgroupBox4.Padding = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.IndgroupBox4.Size = new System.Drawing.Size(280, 82);
            this.IndgroupBox4.TabIndex = 3;
            this.IndgroupBox4.TabStop = false;
            this.IndgroupBox4.Text = "Region 2";
            // 
            // panelInd7
            // 
            this.panelInd7.Controls.Add(this.toolStripInd7);
            this.panelInd7.Location = new System.Drawing.Point(8, 50);
            this.panelInd7.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.panelInd7.Name = "panelInd7";
            this.panelInd7.Size = new System.Drawing.Size(176, 25);
            this.panelInd7.TabIndex = 12;
            // 
            // toolStripInd7
            // 
            this.toolStripInd7.AutoSize = false;
            this.toolStripInd7.CanOverflow = false;
            this.toolStripInd7.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStripInd7.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStripInd7.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.Ind_Reg2FontsList});
            this.toolStripInd7.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
            this.toolStripInd7.Location = new System.Drawing.Point(0, -1);
            this.toolStripInd7.Name = "toolStripInd7";
            this.toolStripInd7.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.toolStripInd7.Size = new System.Drawing.Size(181, 29);
            this.toolStripInd7.TabIndex = 5;
            // 
            // Ind_Reg2FontsList
            // 
            this.Ind_Reg2FontsList.AutoSize = false;
            this.Ind_Reg2FontsList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.Ind_Reg2FontsList.Items.AddRange(new object[] {
            "No Media",
            "Show Media",
            "Hide Media"});
            this.Ind_Reg2FontsList.MaxDropDownItems = 12;
            this.Ind_Reg2FontsList.Name = "Ind_Reg2FontsList";
            this.Ind_Reg2FontsList.Size = new System.Drawing.Size(171, 23);
            this.Ind_Reg2FontsList.SelectedIndexChanged += new System.EventHandler(this.Ind_FontsList_SelectedIndexChanged);
            // 
            // Ind_Reg2SizeUpDown
            // 
            this.Ind_Reg2SizeUpDown.Location = new System.Drawing.Point(219, 51);
            this.Ind_Reg2SizeUpDown.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Ind_Reg2SizeUpDown.Minimum = new decimal(new int[] {
            6,
            0,
            0,
            0});
            this.Ind_Reg2SizeUpDown.Name = "Ind_Reg2SizeUpDown";
            this.Ind_Reg2SizeUpDown.Size = new System.Drawing.Size(52, 23);
            this.Ind_Reg2SizeUpDown.TabIndex = 3;
            this.Ind_Reg2SizeUpDown.Value = new decimal(new int[] {
            6,
            0,
            0,
            0});
            this.Ind_Reg2SizeUpDown.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Ind_FontSizeUpDown_MouseUp);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(189, 52);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(30, 15);
            this.label6.TabIndex = 2;
            this.label6.Text = "Size:";
            // 
            // Ind_Reg2TopUpDown
            // 
            this.Ind_Reg2TopUpDown.Location = new System.Drawing.Point(219, 22);
            this.Ind_Reg2TopUpDown.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Ind_Reg2TopUpDown.Name = "Ind_Reg2TopUpDown";
            this.Ind_Reg2TopUpDown.Size = new System.Drawing.Size(52, 23);
            this.Ind_Reg2TopUpDown.TabIndex = 1;
            this.Ind_Reg2TopUpDown.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Ind_MarginUpDown_MouseUp);
            // 
            // panelInd6
            // 
            this.panelInd6.Controls.Add(this.toolStripInd6);
            this.panelInd6.Location = new System.Drawing.Point(8, 21);
            this.panelInd6.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.panelInd6.Name = "panelInd6";
            this.panelInd6.Size = new System.Drawing.Size(181, 25);
            this.panelInd6.TabIndex = 10;
            // 
            // toolStripInd6
            // 
            this.toolStripInd6.AutoSize = false;
            this.toolStripInd6.CanOverflow = false;
            this.toolStripInd6.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStripInd6.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStripInd6.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.Ind_R2Bold,
            this.Ind_R2Italics,
            this.Ind_R2Underline,
            this.toolStripSeparator15,
            this.Ind_R2Align,
            this.Ind_R2Colour});
            this.toolStripInd6.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
            this.toolStripInd6.Location = new System.Drawing.Point(0, -1);
            this.toolStripInd6.Name = "toolStripInd6";
            this.toolStripInd6.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.toolStripInd6.Size = new System.Drawing.Size(181, 29);
            this.toolStripInd6.TabIndex = 0;
            // 
            // Ind_R2Bold
            // 
            this.Ind_R2Bold.CheckOnClick = true;
            this.Ind_R2Bold.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.Ind_R2Bold.Image = ((System.Drawing.Image)(resources.GetObject("Ind_R2Bold.Image")));
            this.Ind_R2Bold.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.Ind_R2Bold.Name = "Ind_R2Bold";
            this.Ind_R2Bold.Size = new System.Drawing.Size(23, 26);
            this.Ind_R2Bold.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Ind_RegionsFormat_MouseUp);
            // 
            // Ind_R2Italics
            // 
            this.Ind_R2Italics.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.Ind_R2Italics.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.Ind_R2Italics0,
            this.Ind_R2Italics1,
            this.Ind_R2Italics2});
            this.Ind_R2Italics.Image = ((System.Drawing.Image)(resources.GetObject("Ind_R2Italics.Image")));
            this.Ind_R2Italics.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.Ind_R2Italics.Name = "Ind_R2Italics";
            this.Ind_R2Italics.Size = new System.Drawing.Size(29, 26);
            this.Ind_R2Italics.Tag = "0";
            this.Ind_R2Italics.Text = "toolStripDropDownButton1";
            this.Ind_R2Italics.DropDownItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.Ind_R2Italics_DropDownItemClicked);
            // 
            // Ind_R2Italics0
            // 
            this.Ind_R2Italics0.Image = ((System.Drawing.Image)(resources.GetObject("Ind_R2Italics0.Image")));
            this.Ind_R2Italics0.Name = "Ind_R2Italics0";
            this.Ind_R2Italics0.Size = new System.Drawing.Size(173, 22);
            this.Ind_R2Italics0.Tag = "0";
            this.Ind_R2Italics0.Text = "No Italics";
            // 
            // Ind_R2Italics1
            // 
            this.Ind_R2Italics1.Image = ((System.Drawing.Image)(resources.GetObject("Ind_R2Italics1.Image")));
            this.Ind_R2Italics1.Name = "Ind_R2Italics1";
            this.Ind_R2Italics1.Size = new System.Drawing.Size(173, 22);
            this.Ind_R2Italics1.Tag = "1";
            this.Ind_R2Italics1.Text = "Italics";
            // 
            // Ind_R2Italics2
            // 
            this.Ind_R2Italics2.Image = ((System.Drawing.Image)(resources.GetObject("Ind_R2Italics2.Image")));
            this.Ind_R2Italics2.Name = "Ind_R2Italics2";
            this.Ind_R2Italics2.Size = new System.Drawing.Size(173, 22);
            this.Ind_R2Italics2.Tag = "2";
            this.Ind_R2Italics2.Text = "Chorus Italics Only";
            // 
            // Ind_R2Underline
            // 
            this.Ind_R2Underline.CheckOnClick = true;
            this.Ind_R2Underline.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.Ind_R2Underline.Image = ((System.Drawing.Image)(resources.GetObject("Ind_R2Underline.Image")));
            this.Ind_R2Underline.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.Ind_R2Underline.Name = "Ind_R2Underline";
            this.Ind_R2Underline.Size = new System.Drawing.Size(23, 26);
            this.Ind_R2Underline.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Ind_RegionsFormat_MouseUp);
            // 
            // toolStripSeparator15
            // 
            this.toolStripSeparator15.Name = "toolStripSeparator15";
            this.toolStripSeparator15.Size = new System.Drawing.Size(6, 29);
            // 
            // Ind_R2Align
            // 
            this.Ind_R2Align.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.Ind_R2Align.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.Ind_R2AlignLeft,
            this.Ind_R2AlignCentre,
            this.Ind_R2AlignRight});
            this.Ind_R2Align.Image = ((System.Drawing.Image)(resources.GetObject("Ind_R2Align.Image")));
            this.Ind_R2Align.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.Ind_R2Align.Name = "Ind_R2Align";
            this.Ind_R2Align.Size = new System.Drawing.Size(29, 26);
            this.Ind_R2Align.Tag = "1";
            this.Ind_R2Align.DropDownItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.Ind_R2Align_DropDownItemClicked);
            // 
            // Ind_R2AlignLeft
            // 
            this.Ind_R2AlignLeft.Image = ((System.Drawing.Image)(resources.GetObject("Ind_R2AlignLeft.Image")));
            this.Ind_R2AlignLeft.Name = "Ind_R2AlignLeft";
            this.Ind_R2AlignLeft.Size = new System.Drawing.Size(140, 22);
            this.Ind_R2AlignLeft.Tag = "1";
            this.Ind_R2AlignLeft.Text = "Align Left";
            // 
            // Ind_R2AlignCentre
            // 
            this.Ind_R2AlignCentre.Image = ((System.Drawing.Image)(resources.GetObject("Ind_R2AlignCentre.Image")));
            this.Ind_R2AlignCentre.Name = "Ind_R2AlignCentre";
            this.Ind_R2AlignCentre.Size = new System.Drawing.Size(140, 22);
            this.Ind_R2AlignCentre.Tag = "2";
            this.Ind_R2AlignCentre.Text = "Align Centre";
            // 
            // Ind_R2AlignRight
            // 
            this.Ind_R2AlignRight.Image = ((System.Drawing.Image)(resources.GetObject("Ind_R2AlignRight.Image")));
            this.Ind_R2AlignRight.Name = "Ind_R2AlignRight";
            this.Ind_R2AlignRight.Size = new System.Drawing.Size(140, 22);
            this.Ind_R2AlignRight.Tag = "3";
            this.Ind_R2AlignRight.Text = "Align Right";
            // 
            // Ind_R2Colour
            // 
            this.Ind_R2Colour.AutoSize = false;
            this.Ind_R2Colour.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.Ind_R2Colour.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.Ind_R2Colour.Image = ((System.Drawing.Image)(resources.GetObject("Ind_R2Colour.Image")));
            this.Ind_R2Colour.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.Ind_R2Colour.Name = "Ind_R2Colour";
            this.Ind_R2Colour.Size = new System.Drawing.Size(44, 22);
            this.Ind_R2Colour.Text = "Colour";
            this.Ind_R2Colour.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Ind_RegionsFormat_MouseUp);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(190, 24);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(29, 15);
            this.label7.TabIndex = 0;
            this.label7.Text = "Top:";
            // 
            // IndgroupBox3
            // 
            this.IndgroupBox3.Controls.Add(this.panelInd5);
            this.IndgroupBox3.Controls.Add(this.Ind_Reg1SizeUpDown);
            this.IndgroupBox3.Controls.Add(this.labelBlackScreenOn);
            this.IndgroupBox3.Controls.Add(this.Ind_Reg1TopUpDown);
            this.IndgroupBox3.Controls.Add(this.panelInd4);
            this.IndgroupBox3.Controls.Add(this.label4);
            this.IndgroupBox3.Location = new System.Drawing.Point(7, 221);
            this.IndgroupBox3.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.IndgroupBox3.Name = "IndgroupBox3";
            this.IndgroupBox3.Padding = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.IndgroupBox3.Size = new System.Drawing.Size(280, 82);
            this.IndgroupBox3.TabIndex = 2;
            this.IndgroupBox3.TabStop = false;
            this.IndgroupBox3.Text = "Region 1";
            // 
            // panelInd5
            // 
            this.panelInd5.Controls.Add(this.toolStripInd5);
            this.panelInd5.Location = new System.Drawing.Point(8, 50);
            this.panelInd5.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.panelInd5.Name = "panelInd5";
            this.panelInd5.Size = new System.Drawing.Size(176, 25);
            this.panelInd5.TabIndex = 12;
            // 
            // toolStripInd5
            // 
            this.toolStripInd5.AutoSize = false;
            this.toolStripInd5.CanOverflow = false;
            this.toolStripInd5.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStripInd5.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStripInd5.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.Ind_Reg1FontsList});
            this.toolStripInd5.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
            this.toolStripInd5.Location = new System.Drawing.Point(0, -1);
            this.toolStripInd5.Name = "toolStripInd5";
            this.toolStripInd5.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.toolStripInd5.Size = new System.Drawing.Size(181, 29);
            this.toolStripInd5.TabIndex = 5;
            // 
            // Ind_Reg1FontsList
            // 
            this.Ind_Reg1FontsList.AutoSize = false;
            this.Ind_Reg1FontsList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.Ind_Reg1FontsList.Items.AddRange(new object[] {
            "No Media",
            "Show Media",
            "Hide Media"});
            this.Ind_Reg1FontsList.MaxDropDownItems = 12;
            this.Ind_Reg1FontsList.Name = "Ind_Reg1FontsList";
            this.Ind_Reg1FontsList.Size = new System.Drawing.Size(171, 23);
            this.Ind_Reg1FontsList.SelectedIndexChanged += new System.EventHandler(this.Ind_FontsList_SelectedIndexChanged);
            // 
            // Ind_Reg1SizeUpDown
            // 
            this.Ind_Reg1SizeUpDown.Location = new System.Drawing.Point(219, 51);
            this.Ind_Reg1SizeUpDown.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Ind_Reg1SizeUpDown.Minimum = new decimal(new int[] {
            6,
            0,
            0,
            0});
            this.Ind_Reg1SizeUpDown.Name = "Ind_Reg1SizeUpDown";
            this.Ind_Reg1SizeUpDown.Size = new System.Drawing.Size(52, 23);
            this.Ind_Reg1SizeUpDown.TabIndex = 3;
            this.Ind_Reg1SizeUpDown.Value = new decimal(new int[] {
            6,
            0,
            0,
            0});
            this.Ind_Reg1SizeUpDown.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Ind_FontSizeUpDown_MouseUp);
            // 
            // labelBlackScreenOn
            // 
            this.labelBlackScreenOn.AutoSize = true;
            this.labelBlackScreenOn.BackColor = System.Drawing.Color.Transparent;
            this.labelBlackScreenOn.Location = new System.Drawing.Point(189, 52);
            this.labelBlackScreenOn.Name = "labelBlackScreenOn";
            this.labelBlackScreenOn.Size = new System.Drawing.Size(30, 15);
            this.labelBlackScreenOn.TabIndex = 2;
            this.labelBlackScreenOn.Text = "Size:";
            // 
            // Ind_Reg1TopUpDown
            // 
            this.Ind_Reg1TopUpDown.Location = new System.Drawing.Point(219, 22);
            this.Ind_Reg1TopUpDown.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Ind_Reg1TopUpDown.Name = "Ind_Reg1TopUpDown";
            this.Ind_Reg1TopUpDown.Size = new System.Drawing.Size(52, 23);
            this.Ind_Reg1TopUpDown.TabIndex = 1;
            this.Ind_Reg1TopUpDown.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Ind_MarginUpDown_MouseUp);
            // 
            // panelInd4
            // 
            this.panelInd4.Controls.Add(this.toolStripInd4);
            this.panelInd4.Location = new System.Drawing.Point(8, 21);
            this.panelInd4.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.panelInd4.Name = "panelInd4";
            this.panelInd4.Size = new System.Drawing.Size(181, 25);
            this.panelInd4.TabIndex = 10;
            // 
            // toolStripInd4
            // 
            this.toolStripInd4.AutoSize = false;
            this.toolStripInd4.CanOverflow = false;
            this.toolStripInd4.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStripInd4.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStripInd4.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.Ind_R1Bold,
            this.Ind_R1Italics,
            this.Ind_R1Underline,
            this.toolStripSeparator13,
            this.Ind_R1Align,
            this.Ind_R1Colour});
            this.toolStripInd4.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
            this.toolStripInd4.Location = new System.Drawing.Point(0, -1);
            this.toolStripInd4.Name = "toolStripInd4";
            this.toolStripInd4.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.toolStripInd4.Size = new System.Drawing.Size(181, 29);
            this.toolStripInd4.TabIndex = 0;
            // 
            // Ind_R1Bold
            // 
            this.Ind_R1Bold.CheckOnClick = true;
            this.Ind_R1Bold.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.Ind_R1Bold.Image = ((System.Drawing.Image)(resources.GetObject("Ind_R1Bold.Image")));
            this.Ind_R1Bold.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.Ind_R1Bold.Name = "Ind_R1Bold";
            this.Ind_R1Bold.Size = new System.Drawing.Size(23, 26);
            this.Ind_R1Bold.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Ind_RegionsFormat_MouseUp);
            // 
            // Ind_R1Italics
            // 
            this.Ind_R1Italics.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.Ind_R1Italics.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.Ind_R1Italics0,
            this.Ind_R1Italics1,
            this.Ind_R1Italics2});
            this.Ind_R1Italics.Image = ((System.Drawing.Image)(resources.GetObject("Ind_R1Italics.Image")));
            this.Ind_R1Italics.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.Ind_R1Italics.Name = "Ind_R1Italics";
            this.Ind_R1Italics.Size = new System.Drawing.Size(29, 26);
            this.Ind_R1Italics.Tag = "0";
            this.Ind_R1Italics.DropDownItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.Ind_R1Italics_DropDownItemClicked);
            // 
            // Ind_R1Italics0
            // 
            this.Ind_R1Italics0.Image = ((System.Drawing.Image)(resources.GetObject("Ind_R1Italics0.Image")));
            this.Ind_R1Italics0.Name = "Ind_R1Italics0";
            this.Ind_R1Italics0.Size = new System.Drawing.Size(173, 22);
            this.Ind_R1Italics0.Tag = "0";
            this.Ind_R1Italics0.Text = "No Italics";
            // 
            // Ind_R1Italics1
            // 
            this.Ind_R1Italics1.Image = ((System.Drawing.Image)(resources.GetObject("Ind_R1Italics1.Image")));
            this.Ind_R1Italics1.Name = "Ind_R1Italics1";
            this.Ind_R1Italics1.Size = new System.Drawing.Size(173, 22);
            this.Ind_R1Italics1.Tag = "1";
            this.Ind_R1Italics1.Text = "Italics";
            // 
            // Ind_R1Italics2
            // 
            this.Ind_R1Italics2.Image = ((System.Drawing.Image)(resources.GetObject("Ind_R1Italics2.Image")));
            this.Ind_R1Italics2.Name = "Ind_R1Italics2";
            this.Ind_R1Italics2.Size = new System.Drawing.Size(173, 22);
            this.Ind_R1Italics2.Tag = "2";
            this.Ind_R1Italics2.Text = "Chorus Italics Only";
            // 
            // Ind_R1Underline
            // 
            this.Ind_R1Underline.CheckOnClick = true;
            this.Ind_R1Underline.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.Ind_R1Underline.Image = ((System.Drawing.Image)(resources.GetObject("Ind_R1Underline.Image")));
            this.Ind_R1Underline.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.Ind_R1Underline.Name = "Ind_R1Underline";
            this.Ind_R1Underline.Size = new System.Drawing.Size(23, 26);
            this.Ind_R1Underline.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Ind_RegionsFormat_MouseUp);
            // 
            // toolStripSeparator13
            // 
            this.toolStripSeparator13.Name = "toolStripSeparator13";
            this.toolStripSeparator13.Size = new System.Drawing.Size(6, 29);
            // 
            // Ind_R1Align
            // 
            this.Ind_R1Align.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.Ind_R1Align.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.Ind_R1AlignLeft,
            this.Ind_R1AlignCentre,
            this.Ind_R1AlignRight});
            this.Ind_R1Align.Image = ((System.Drawing.Image)(resources.GetObject("Ind_R1Align.Image")));
            this.Ind_R1Align.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.Ind_R1Align.Name = "Ind_R1Align";
            this.Ind_R1Align.Size = new System.Drawing.Size(29, 26);
            this.Ind_R1Align.Tag = "2";
            this.Ind_R1Align.DropDownItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.Ind_R1Align_DropDownItemClicked);
            // 
            // Ind_R1AlignLeft
            // 
            this.Ind_R1AlignLeft.Image = ((System.Drawing.Image)(resources.GetObject("Ind_R1AlignLeft.Image")));
            this.Ind_R1AlignLeft.Name = "Ind_R1AlignLeft";
            this.Ind_R1AlignLeft.Size = new System.Drawing.Size(140, 22);
            this.Ind_R1AlignLeft.Tag = "1";
            this.Ind_R1AlignLeft.Text = "Align Left";
            // 
            // Ind_R1AlignCentre
            // 
            this.Ind_R1AlignCentre.Image = ((System.Drawing.Image)(resources.GetObject("Ind_R1AlignCentre.Image")));
            this.Ind_R1AlignCentre.Name = "Ind_R1AlignCentre";
            this.Ind_R1AlignCentre.Size = new System.Drawing.Size(140, 22);
            this.Ind_R1AlignCentre.Tag = "2";
            this.Ind_R1AlignCentre.Text = "Align Centre";
            // 
            // Ind_R1AlignRight
            // 
            this.Ind_R1AlignRight.Image = ((System.Drawing.Image)(resources.GetObject("Ind_R1AlignRight.Image")));
            this.Ind_R1AlignRight.Name = "Ind_R1AlignRight";
            this.Ind_R1AlignRight.Size = new System.Drawing.Size(140, 22);
            this.Ind_R1AlignRight.Tag = "3";
            this.Ind_R1AlignRight.Text = "Align Right";
            // 
            // Ind_R1Colour
            // 
            this.Ind_R1Colour.AutoSize = false;
            this.Ind_R1Colour.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.Ind_R1Colour.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.Ind_R1Colour.Image = ((System.Drawing.Image)(resources.GetObject("Ind_R1Colour.Image")));
            this.Ind_R1Colour.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.Ind_R1Colour.Name = "Ind_R1Colour";
            this.Ind_R1Colour.Size = new System.Drawing.Size(44, 22);
            this.Ind_R1Colour.Text = "Colour";
            this.Ind_R1Colour.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Ind_RegionsFormat_MouseUp);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(190, 24);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(29, 15);
            this.label4.TabIndex = 0;
            this.label4.Text = "Top:";
            // 
            // IndgroupBox2
            // 
            this.IndgroupBox2.Controls.Add(this.Ind_BottomUpDown);
            this.IndgroupBox2.Controls.Add(this.panelInd3);
            this.IndgroupBox2.Controls.Add(this.Ind_RightUpDown);
            this.IndgroupBox2.Controls.Add(this.panelInd2);
            this.IndgroupBox2.Controls.Add(this.label3);
            this.IndgroupBox2.Controls.Add(this.Ind_LeftUpDown);
            this.IndgroupBox2.Controls.Add(this.label2);
            this.IndgroupBox2.Controls.Add(this.label1);
            this.IndgroupBox2.Location = new System.Drawing.Point(7, 108);
            this.IndgroupBox2.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.IndgroupBox2.Name = "IndgroupBox2";
            this.IndgroupBox2.Padding = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.IndgroupBox2.Size = new System.Drawing.Size(280, 112);
            this.IndgroupBox2.TabIndex = 1;
            this.IndgroupBox2.TabStop = false;
            this.IndgroupBox2.Text = "Background";
            // 
            // Ind_BottomUpDown
            // 
            this.Ind_BottomUpDown.Location = new System.Drawing.Point(218, 82);
            this.Ind_BottomUpDown.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Ind_BottomUpDown.Name = "Ind_BottomUpDown";
            this.Ind_BottomUpDown.Size = new System.Drawing.Size(52, 23);
            this.Ind_BottomUpDown.TabIndex = 2;
            this.Ind_BottomUpDown.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Ind_MarginUpDown_MouseUp);
            // 
            // panelInd3
            // 
            this.panelInd3.Controls.Add(this.toolStripInd3);
            this.panelInd3.Location = new System.Drawing.Point(7, 50);
            this.panelInd3.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.panelInd3.Name = "panelInd3";
            this.panelInd3.Size = new System.Drawing.Size(266, 25);
            this.panelInd3.TabIndex = 11;
            // 
            // toolStripInd3
            // 
            this.toolStripInd3.AutoSize = false;
            this.toolStripInd3.CanOverflow = false;
            this.toolStripInd3.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStripInd3.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStripInd3.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.Ind_TransItem,
            this.Ind_TransSlides});
            this.toolStripInd3.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
            this.toolStripInd3.Location = new System.Drawing.Point(0, -1);
            this.toolStripInd3.Name = "toolStripInd3";
            this.toolStripInd3.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.toolStripInd3.Size = new System.Drawing.Size(269, 29);
            this.toolStripInd3.TabIndex = 0;
            // 
            // Ind_TransItem
            // 
            this.Ind_TransItem.AutoSize = false;
            this.Ind_TransItem.AutoToolTip = true;
            this.Ind_TransItem.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.Ind_TransItem.MaxDropDownItems = 24;
            this.Ind_TransItem.Name = "Ind_TransItem";
            this.Ind_TransItem.Size = new System.Drawing.Size(129, 23);
            this.Ind_TransItem.ToolTipText = "Item Transition";
            this.Ind_TransItem.SelectedIndexChanged += new System.EventHandler(this.Ind_TransSelectedIndexChanged);
            // 
            // Ind_TransSlides
            // 
            this.Ind_TransSlides.AutoSize = false;
            this.Ind_TransSlides.AutoToolTip = true;
            this.Ind_TransSlides.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.Ind_TransSlides.MaxDropDownItems = 24;
            this.Ind_TransSlides.Name = "Ind_TransSlides";
            this.Ind_TransSlides.Size = new System.Drawing.Size(129, 23);
            this.Ind_TransSlides.ToolTipText = "Slide Transition";
            this.Ind_TransSlides.SelectedIndexChanged += new System.EventHandler(this.Ind_TransSelectedIndexChanged);
            // 
            // Ind_RightUpDown
            // 
            this.Ind_RightUpDown.Location = new System.Drawing.Point(121, 82);
            this.Ind_RightUpDown.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Ind_RightUpDown.Maximum = new decimal(new int[] {
            40,
            0,
            0,
            0});
            this.Ind_RightUpDown.Name = "Ind_RightUpDown";
            this.Ind_RightUpDown.Size = new System.Drawing.Size(48, 23);
            this.Ind_RightUpDown.TabIndex = 1;
            this.Ind_RightUpDown.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Ind_MarginUpDown_MouseUp);
            // 
            // panelInd2
            // 
            this.panelInd2.Controls.Add(this.toolStripInd2);
            this.panelInd2.Location = new System.Drawing.Point(7, 21);
            this.panelInd2.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.panelInd2.Name = "panelInd2";
            this.panelInd2.Size = new System.Drawing.Size(266, 25);
            this.panelInd2.TabIndex = 10;
            // 
            // toolStripInd2
            // 
            this.toolStripInd2.AutoSize = false;
            this.toolStripInd2.CanOverflow = false;
            this.toolStripInd2.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStripInd2.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStripInd2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.Ind_ImageMode,
            this.Ind_NoImage,
            this.Ind_BackColour,
            this.toolStripSeparator27,
            this.Ind_AssignMedia});
            this.toolStripInd2.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
            this.toolStripInd2.Location = new System.Drawing.Point(0, -1);
            this.toolStripInd2.Name = "toolStripInd2";
            this.toolStripInd2.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.toolStripInd2.Size = new System.Drawing.Size(269, 29);
            this.toolStripInd2.TabIndex = 0;
            // 
            // Ind_ImageMode
            // 
            this.Ind_ImageMode.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.Ind_ImageMode.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.Ind_ImageTile,
            this.Ind_ImageCentre,
            this.Ind_ImageBestFit});
            this.Ind_ImageMode.Image = ((System.Drawing.Image)(resources.GetObject("Ind_ImageMode.Image")));
            this.Ind_ImageMode.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.Ind_ImageMode.Name = "Ind_ImageMode";
            this.Ind_ImageMode.Size = new System.Drawing.Size(29, 26);
            this.Ind_ImageMode.Tag = "2";
            this.Ind_ImageMode.ToolTipText = "Background Picture Format";
            this.Ind_ImageMode.DropDownItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.Ind_ImageMode_DropDownItemClicked);
            // 
            // Ind_ImageTile
            // 
            this.Ind_ImageTile.Image = ((System.Drawing.Image)(resources.GetObject("Ind_ImageTile.Image")));
            this.Ind_ImageTile.Name = "Ind_ImageTile";
            this.Ind_ImageTile.Size = new System.Drawing.Size(148, 22);
            this.Ind_ImageTile.Tag = "0";
            this.Ind_ImageTile.Text = "Tile Image";
            // 
            // Ind_ImageCentre
            // 
            this.Ind_ImageCentre.Image = ((System.Drawing.Image)(resources.GetObject("Ind_ImageCentre.Image")));
            this.Ind_ImageCentre.Name = "Ind_ImageCentre";
            this.Ind_ImageCentre.Size = new System.Drawing.Size(148, 22);
            this.Ind_ImageCentre.Tag = "1";
            this.Ind_ImageCentre.Text = "Centre Image";
            // 
            // Ind_ImageBestFit
            // 
            this.Ind_ImageBestFit.Image = ((System.Drawing.Image)(resources.GetObject("Ind_ImageBestFit.Image")));
            this.Ind_ImageBestFit.Name = "Ind_ImageBestFit";
            this.Ind_ImageBestFit.Size = new System.Drawing.Size(148, 22);
            this.Ind_ImageBestFit.Tag = "2";
            this.Ind_ImageBestFit.Text = "Best Fit Image";
            // 
            // Ind_NoImage
            // 
            this.Ind_NoImage.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.Ind_NoImage.Image = ((System.Drawing.Image)(resources.GetObject("Ind_NoImage.Image")));
            this.Ind_NoImage.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.Ind_NoImage.Name = "Ind_NoImage";
            this.Ind_NoImage.Size = new System.Drawing.Size(23, 26);
            this.Ind_NoImage.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Ind_Items_MouseUp);
            // 
            // Ind_BackColour
            // 
            this.Ind_BackColour.AutoSize = false;
            this.Ind_BackColour.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.Ind_BackColour.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.Ind_BackColour.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.Ind_BackColour.Name = "Ind_BackColour";
            this.Ind_BackColour.Size = new System.Drawing.Size(59, 22);
            this.Ind_BackColour.Text = "Colours";
            this.Ind_BackColour.ToolTipText = "Background Colours and Patterns";
            this.Ind_BackColour.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Ind_Items_MouseUp);
            // 
            // toolStripSeparator27
            // 
            this.toolStripSeparator27.Name = "toolStripSeparator27";
            this.toolStripSeparator27.Size = new System.Drawing.Size(6, 29);
            // 
            // Ind_AssignMedia
            // 
            this.Ind_AssignMedia.AutoSize = false;
            this.Ind_AssignMedia.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.Ind_AssignMedia.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.Ind_AssignMedia.Name = "Ind_AssignMedia";
            this.Ind_AssignMedia.Size = new System.Drawing.Size(110, 22);
            this.Ind_AssignMedia.Text = "Media";
            this.Ind_AssignMedia.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Ind_Items_MouseUp);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(173, 84);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(50, 15);
            this.label3.TabIndex = 15;
            this.label3.Text = "Bottom:";
            // 
            // Ind_LeftUpDown
            // 
            this.Ind_LeftUpDown.Location = new System.Drawing.Point(33, 82);
            this.Ind_LeftUpDown.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Ind_LeftUpDown.Maximum = new decimal(new int[] {
            40,
            0,
            0,
            0});
            this.Ind_LeftUpDown.Name = "Ind_LeftUpDown";
            this.Ind_LeftUpDown.Size = new System.Drawing.Size(48, 23);
            this.Ind_LeftUpDown.TabIndex = 10;
            this.Ind_LeftUpDown.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Ind_MarginUpDown_MouseUp);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(85, 84);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(38, 15);
            this.label2.TabIndex = 14;
            this.label2.Text = "Right:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(5, 84);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(30, 15);
            this.label1.TabIndex = 0;
            this.label1.Text = "Left:";
            // 
            // IndgroupBox1
            // 
            this.IndgroupBox1.Controls.Add(this.panel4);
            this.IndgroupBox1.Controls.Add(this.panelInd1);
            this.IndgroupBox1.Location = new System.Drawing.Point(7, 22);
            this.IndgroupBox1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.IndgroupBox1.Name = "IndgroupBox1";
            this.IndgroupBox1.Padding = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.IndgroupBox1.Size = new System.Drawing.Size(280, 84);
            this.IndgroupBox1.TabIndex = 0;
            this.IndgroupBox1.TabStop = false;
            this.IndgroupBox1.Text = "Layout";
            // 
            // panel4
            // 
            this.panel4.Controls.Add(this.toolStrip1);
            this.panel4.Location = new System.Drawing.Point(7, 51);
            this.panel4.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(122, 25);
            this.panel4.TabIndex = 10;
            // 
            // toolStrip1
            // 
            this.toolStrip1.AutoSize = false;
            this.toolStrip1.CanOverflow = false;
            this.toolStrip1.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.Ind_HeadAlign,
            this.Ind_CapoDown,
            this.Ind_CapoUp,
            this.toolStripSeparator5,
            this.Ind_HideDisplayPanel});
            this.toolStrip1.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
            this.toolStrip1.Location = new System.Drawing.Point(0, -1);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.toolStrip1.Size = new System.Drawing.Size(127, 29);
            this.toolStrip1.TabIndex = 0;
            // 
            // Ind_HeadAlign
            // 
            this.Ind_HeadAlign.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.Ind_HeadAlign.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.Ind_HeadAlignAsR1,
            this.Ind_HeadAlignAsR2,
            this.Ind_HeadAlignLeft,
            this.Ind_HeadAlignCentre,
            this.Ind_HeadAlignRight});
            this.Ind_HeadAlign.Image = ((System.Drawing.Image)(resources.GetObject("Ind_HeadAlign.Image")));
            this.Ind_HeadAlign.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.Ind_HeadAlign.Name = "Ind_HeadAlign";
            this.Ind_HeadAlign.Size = new System.Drawing.Size(29, 26);
            this.Ind_HeadAlign.Tag = "0";
            this.Ind_HeadAlign.ToolTipText = "Display Title/Verse Headings";
            this.Ind_HeadAlign.DropDownItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.Ind_HeadAlign_DropDownItemClicked);
            // 
            // Ind_HeadAlignAsR1
            // 
            this.Ind_HeadAlignAsR1.Image = ((System.Drawing.Image)(resources.GetObject("Ind_HeadAlignAsR1.Image")));
            this.Ind_HeadAlignAsR1.Name = "Ind_HeadAlignAsR1";
            this.Ind_HeadAlignAsR1.Size = new System.Drawing.Size(220, 22);
            this.Ind_HeadAlignAsR1.Tag = "0";
            this.Ind_HeadAlignAsR1.Text = "Headings Align As Region 1";
            // 
            // Ind_HeadAlignAsR2
            // 
            this.Ind_HeadAlignAsR2.Image = ((System.Drawing.Image)(resources.GetObject("Ind_HeadAlignAsR2.Image")));
            this.Ind_HeadAlignAsR2.Name = "Ind_HeadAlignAsR2";
            this.Ind_HeadAlignAsR2.Size = new System.Drawing.Size(220, 22);
            this.Ind_HeadAlignAsR2.Tag = "1";
            this.Ind_HeadAlignAsR2.Text = "Headings Align As region 2";
            // 
            // Ind_HeadAlignLeft
            // 
            this.Ind_HeadAlignLeft.Image = ((System.Drawing.Image)(resources.GetObject("Ind_HeadAlignLeft.Image")));
            this.Ind_HeadAlignLeft.Name = "Ind_HeadAlignLeft";
            this.Ind_HeadAlignLeft.Size = new System.Drawing.Size(220, 22);
            this.Ind_HeadAlignLeft.Tag = "2";
            this.Ind_HeadAlignLeft.Text = "Headings Align Left";
            // 
            // Ind_HeadAlignCentre
            // 
            this.Ind_HeadAlignCentre.Image = ((System.Drawing.Image)(resources.GetObject("Ind_HeadAlignCentre.Image")));
            this.Ind_HeadAlignCentre.Name = "Ind_HeadAlignCentre";
            this.Ind_HeadAlignCentre.Size = new System.Drawing.Size(220, 22);
            this.Ind_HeadAlignCentre.Tag = "3";
            this.Ind_HeadAlignCentre.Text = "Headings Align Centre";
            // 
            // Ind_HeadAlignRight
            // 
            this.Ind_HeadAlignRight.Image = ((System.Drawing.Image)(resources.GetObject("Ind_HeadAlignRight.Image")));
            this.Ind_HeadAlignRight.Name = "Ind_HeadAlignRight";
            this.Ind_HeadAlignRight.Size = new System.Drawing.Size(220, 22);
            this.Ind_HeadAlignRight.Tag = "4";
            this.Ind_HeadAlignRight.Text = "Headings Align Right";
            // 
            // Ind_CapoDown
            // 
            this.Ind_CapoDown.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.Ind_CapoDown.Image = ((System.Drawing.Image)(resources.GetObject("Ind_CapoDown.Image")));
            this.Ind_CapoDown.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.Ind_CapoDown.Name = "Ind_CapoDown";
            this.Ind_CapoDown.Size = new System.Drawing.Size(23, 26);
            this.Ind_CapoDown.ToolTipText = "Transpose Down 1 Semi-Tone";
            this.Ind_CapoDown.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Ind_Items_MouseUp);
            // 
            // Ind_CapoUp
            // 
            this.Ind_CapoUp.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.Ind_CapoUp.Image = ((System.Drawing.Image)(resources.GetObject("Ind_CapoUp.Image")));
            this.Ind_CapoUp.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.Ind_CapoUp.Name = "Ind_CapoUp";
            this.Ind_CapoUp.Size = new System.Drawing.Size(23, 26);
            this.Ind_CapoUp.ToolTipText = "Transpose Up 1 Semi-Tone";
            this.Ind_CapoUp.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Ind_Items_MouseUp);
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(6, 29);
            // 
            // Ind_HideDisplayPanel
            // 
            this.Ind_HideDisplayPanel.CheckOnClick = true;
            this.Ind_HideDisplayPanel.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.Ind_HideDisplayPanel.Image = ((System.Drawing.Image)(resources.GetObject("Ind_HideDisplayPanel.Image")));
            this.Ind_HideDisplayPanel.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.Ind_HideDisplayPanel.Name = "Ind_HideDisplayPanel";
            this.Ind_HideDisplayPanel.Size = new System.Drawing.Size(23, 26);
            this.Ind_HideDisplayPanel.ToolTipText = "Do not show Display Panel Data";
            this.Ind_HideDisplayPanel.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Ind_Items_MouseUp);
            // 
            // panelInd1
            // 
            this.panelInd1.Controls.Add(this.toolStripInd1);
            this.panelInd1.Location = new System.Drawing.Point(7, 21);
            this.panelInd1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.panelInd1.Name = "panelInd1";
            this.panelInd1.Size = new System.Drawing.Size(213, 25);
            this.panelInd1.TabIndex = 9;
            // 
            // toolStripInd1
            // 
            this.toolStripInd1.AutoSize = false;
            this.toolStripInd1.CanOverflow = false;
            this.toolStripInd1.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStripInd1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStripInd1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.Ind_Head,
            this.Ind_Region,
            this.Ind_VAlign,
            this.Ind_Shadow,
            this.Ind_Outline,
            this.Ind_Interlace,
            this.Ind_Notations});
            this.toolStripInd1.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
            this.toolStripInd1.Location = new System.Drawing.Point(0, -1);
            this.toolStripInd1.Name = "toolStripInd1";
            this.toolStripInd1.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.toolStripInd1.Size = new System.Drawing.Size(217, 29);
            this.toolStripInd1.TabIndex = 0;
            // 
            // Ind_Head
            // 
            this.Ind_Head.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.Ind_Head.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.Ind_HeadNoTitles,
            this.Ind_HeadAllTitles,
            this.Ind_HeadFirstScreen});
            this.Ind_Head.Image = ((System.Drawing.Image)(resources.GetObject("Ind_Head.Image")));
            this.Ind_Head.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.Ind_Head.Name = "Ind_Head";
            this.Ind_Head.Size = new System.Drawing.Size(29, 26);
            this.Ind_Head.Tag = "1";
            this.Ind_Head.ToolTipText = "Display Title/Verse Headings";
            this.Ind_Head.DropDownItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.Ind_Head_DropDownItemClicked);
            // 
            // Ind_HeadNoTitles
            // 
            this.Ind_HeadNoTitles.Image = ((System.Drawing.Image)(resources.GetObject("Ind_HeadNoTitles.Image")));
            this.Ind_HeadNoTitles.Name = "Ind_HeadNoTitles";
            this.Ind_HeadNoTitles.Size = new System.Drawing.Size(225, 22);
            this.Ind_HeadNoTitles.Tag = "0";
            this.Ind_HeadNoTitles.Text = "No Headings";
            // 
            // Ind_HeadAllTitles
            // 
            this.Ind_HeadAllTitles.Image = ((System.Drawing.Image)(resources.GetObject("Ind_HeadAllTitles.Image")));
            this.Ind_HeadAllTitles.Name = "Ind_HeadAllTitles";
            this.Ind_HeadAllTitles.Size = new System.Drawing.Size(225, 22);
            this.Ind_HeadAllTitles.Tag = "1";
            this.Ind_HeadAllTitles.Text = "Show All Headings";
            // 
            // Ind_HeadFirstScreen
            // 
            this.Ind_HeadFirstScreen.Image = ((System.Drawing.Image)(resources.GetObject("Ind_HeadFirstScreen.Image")));
            this.Ind_HeadFirstScreen.Name = "Ind_HeadFirstScreen";
            this.Ind_HeadFirstScreen.Size = new System.Drawing.Size(225, 22);
            this.Ind_HeadFirstScreen.Tag = "2";
            this.Ind_HeadFirstScreen.Text = "Heading At First Screen Only";
            // 
            // Ind_Region
            // 
            this.Ind_Region.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.Ind_Region.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.Ind_ShowRegion1,
            this.Ind_ShowRegion2,
            this.Ind_ShowRegionBoth});
            this.Ind_Region.Image = ((System.Drawing.Image)(resources.GetObject("Ind_Region.Image")));
            this.Ind_Region.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.Ind_Region.Name = "Ind_Region";
            this.Ind_Region.Size = new System.Drawing.Size(29, 26);
            this.Ind_Region.Tag = "2";
            this.Ind_Region.ToolTipText = "Show Region Text";
            this.Ind_Region.DropDownItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.Ind_Region_DropDownItemClicked);
            // 
            // Ind_ShowRegion1
            // 
            this.Ind_ShowRegion1.Image = ((System.Drawing.Image)(resources.GetObject("Ind_ShowRegion1.Image")));
            this.Ind_ShowRegion1.Name = "Ind_ShowRegion1";
            this.Ind_ShowRegion1.Size = new System.Drawing.Size(148, 22);
            this.Ind_ShowRegion1.Tag = "0";
            this.Ind_ShowRegion1.Text = "Region 1 Only";
            // 
            // Ind_ShowRegion2
            // 
            this.Ind_ShowRegion2.Image = ((System.Drawing.Image)(resources.GetObject("Ind_ShowRegion2.Image")));
            this.Ind_ShowRegion2.Name = "Ind_ShowRegion2";
            this.Ind_ShowRegion2.Size = new System.Drawing.Size(148, 22);
            this.Ind_ShowRegion2.Tag = "1";
            this.Ind_ShowRegion2.Text = "Region 2 Only";
            // 
            // Ind_ShowRegionBoth
            // 
            this.Ind_ShowRegionBoth.Image = ((System.Drawing.Image)(resources.GetObject("Ind_ShowRegionBoth.Image")));
            this.Ind_ShowRegionBoth.Name = "Ind_ShowRegionBoth";
            this.Ind_ShowRegionBoth.Size = new System.Drawing.Size(148, 22);
            this.Ind_ShowRegionBoth.Tag = "2";
            this.Ind_ShowRegionBoth.Text = "Regions 1 && 2";
            // 
            // Ind_VAlign
            // 
            this.Ind_VAlign.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.Ind_VAlign.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.Ind_VAlignTop,
            this.Ind_VAlignCentre,
            this.Ind_VAlignBottom});
            this.Ind_VAlign.Image = ((System.Drawing.Image)(resources.GetObject("Ind_VAlign.Image")));
            this.Ind_VAlign.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.Ind_VAlign.Name = "Ind_VAlign";
            this.Ind_VAlign.Size = new System.Drawing.Size(29, 26);
            this.Ind_VAlign.Tag = "1";
            this.Ind_VAlign.ToolTipText = "Vertical Alignment";
            this.Ind_VAlign.DropDownItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.Ind_VAlign_DropDownItemClicked);
            // 
            // Ind_VAlignTop
            // 
            this.Ind_VAlignTop.Image = ((System.Drawing.Image)(resources.GetObject("Ind_VAlignTop.Image")));
            this.Ind_VAlignTop.Name = "Ind_VAlignTop";
            this.Ind_VAlignTop.Size = new System.Drawing.Size(145, 22);
            this.Ind_VAlignTop.Tag = "0";
            this.Ind_VAlignTop.Text = "Align Top";
            // 
            // Ind_VAlignCentre
            // 
            this.Ind_VAlignCentre.Image = ((System.Drawing.Image)(resources.GetObject("Ind_VAlignCentre.Image")));
            this.Ind_VAlignCentre.Name = "Ind_VAlignCentre";
            this.Ind_VAlignCentre.Size = new System.Drawing.Size(145, 22);
            this.Ind_VAlignCentre.Tag = "1";
            this.Ind_VAlignCentre.Text = "Align Centre";
            // 
            // Ind_VAlignBottom
            // 
            this.Ind_VAlignBottom.Image = ((System.Drawing.Image)(resources.GetObject("Ind_VAlignBottom.Image")));
            this.Ind_VAlignBottom.Name = "Ind_VAlignBottom";
            this.Ind_VAlignBottom.Size = new System.Drawing.Size(145, 22);
            this.Ind_VAlignBottom.Tag = "2";
            this.Ind_VAlignBottom.Text = "Align Bottom";
            // 
            // Ind_Shadow
            // 
            this.Ind_Shadow.CheckOnClick = true;
            this.Ind_Shadow.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.Ind_Shadow.Image = ((System.Drawing.Image)(resources.GetObject("Ind_Shadow.Image")));
            this.Ind_Shadow.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.Ind_Shadow.Name = "Ind_Shadow";
            this.Ind_Shadow.Size = new System.Drawing.Size(23, 26);
            this.Ind_Shadow.Tag = "open";
            this.Ind_Shadow.ToolTipText = "Shadow Font";
            this.Ind_Shadow.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Ind_Items_MouseUp);
            // 
            // Ind_Outline
            // 
            this.Ind_Outline.CheckOnClick = true;
            this.Ind_Outline.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.Ind_Outline.Image = ((System.Drawing.Image)(resources.GetObject("Ind_Outline.Image")));
            this.Ind_Outline.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.Ind_Outline.Name = "Ind_Outline";
            this.Ind_Outline.Size = new System.Drawing.Size(23, 26);
            this.Ind_Outline.Tag = "add";
            this.Ind_Outline.ToolTipText = "Outline Font";
            this.Ind_Outline.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Ind_Items_MouseUp);
            // 
            // Ind_Interlace
            // 
            this.Ind_Interlace.CheckOnClick = true;
            this.Ind_Interlace.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.Ind_Interlace.Image = ((System.Drawing.Image)(resources.GetObject("Ind_Interlace.Image")));
            this.Ind_Interlace.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.Ind_Interlace.Name = "Ind_Interlace";
            this.Ind_Interlace.Size = new System.Drawing.Size(23, 26);
            this.Ind_Interlace.ToolTipText = "Interlace Region1/Region2";
            this.Ind_Interlace.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Ind_Items_MouseUp);
            // 
            // Ind_Notations
            // 
            this.Ind_Notations.CheckOnClick = true;
            this.Ind_Notations.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.Ind_Notations.Image = ((System.Drawing.Image)(resources.GetObject("Ind_Notations.Image")));
            this.Ind_Notations.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.Ind_Notations.Name = "Ind_Notations";
            this.Ind_Notations.Size = new System.Drawing.Size(23, 26);
            this.Ind_Notations.ToolTipText = "Show Notations";
            this.Ind_Notations.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Ind_Items_MouseUp);
            // 
            // Ind_checkBox
            // 
            this.Ind_checkBox.AutoSize = true;
            this.Ind_checkBox.BackColor = System.Drawing.Color.Transparent;
            this.Ind_checkBox.Location = new System.Drawing.Point(7, 4);
            this.Ind_checkBox.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Ind_checkBox.Name = "Ind_checkBox";
            this.Ind_checkBox.Size = new System.Drawing.Size(145, 19);
            this.Ind_checkBox.TabIndex = 0;
            this.Ind_checkBox.Text = "Use Individual Settings";
            this.Ind_checkBox.UseVisualStyleBackColor = false;
            this.Ind_checkBox.Click += new System.EventHandler(this.Ind_checkBox_Click);
            // 
            // PreviewInfo
            // 
            this.PreviewInfo.BackColor = System.Drawing.SystemColors.Window;
            this.PreviewInfo.Location = new System.Drawing.Point(3, 61);
            this.PreviewInfo.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.PreviewInfo.Name = "PreviewInfo";
            this.PreviewInfo.ReadOnly = true;
            this.PreviewInfo.Size = new System.Drawing.Size(34, 48);
            this.PreviewInfo.TabIndex = 4;
            this.PreviewInfo.Text = "";
            this.PreviewInfo.Enter += new System.EventHandler(this.FormControl_Enter);
            this.PreviewInfo.Leave += new System.EventHandler(this.FormControl_Leave);
            // 
            // flowLayoutPreviewLyrics
            // 
            this.flowLayoutPreviewLyrics.AutoScroll = true;
            this.flowLayoutPreviewLyrics.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowLayoutPreviewLyrics.BackColor = System.Drawing.SystemColors.Window;
            this.flowLayoutPreviewLyrics.Location = new System.Drawing.Point(3, 22);
            this.flowLayoutPreviewLyrics.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.flowLayoutPreviewLyrics.Name = "flowLayoutPreviewLyrics";
            this.flowLayoutPreviewLyrics.Size = new System.Drawing.Size(73, 51);
            this.flowLayoutPreviewLyrics.TabIndex = 6;
            this.flowLayoutPreviewLyrics.Click += new System.EventHandler(this.flowLayoutPreviewLyrics_Click);
            this.flowLayoutPreviewLyrics.Leave += new System.EventHandler(this.flowLayoutPreviewLyrics_Leave);
            // 
            // panel9
            // 
            this.panel9.Controls.Add(this.btnToLive);
            this.panel9.Controls.Add(this.btnToOutputMoveNext);
            this.panel9.Controls.Add(this.PreviewPanelDisplayName);
            this.panel9.Controls.Add(this.btnToOutput);
            this.panel9.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel9.Location = new System.Drawing.Point(0, 0);
            this.panel9.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.panel9.Name = "panel9";
            this.panel9.Size = new System.Drawing.Size(322, 25);
            this.panel9.TabIndex = 0;
            // 
            // btnToLive
            // 
            this.btnToLive.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnToLive.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btnToLive.Location = new System.Drawing.Point(199, 0);
            this.btnToLive.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnToLive.Name = "btnToLive";
            this.btnToLive.Size = new System.Drawing.Size(49, 25);
            this.btnToLive.TabIndex = 8;
            this.btnToLive.Text = "LIVE";
            this.toolTip1.SetToolTip(this.btnToLive, "Copy to Output and Start Show");
            this.btnToLive.Click += new System.EventHandler(this.btnToLive_Click);
            // 
            // btnToOutputMoveNext
            // 
            this.btnToOutputMoveNext.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnToOutputMoveNext.Image = ((System.Drawing.Image)(resources.GetObject("btnToOutputMoveNext.Image")));
            this.btnToOutputMoveNext.Location = new System.Drawing.Point(248, 0);
            this.btnToOutputMoveNext.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnToOutputMoveNext.Name = "btnToOutputMoveNext";
            this.btnToOutputMoveNext.Size = new System.Drawing.Size(37, 25);
            this.btnToOutputMoveNext.TabIndex = 9;
            this.toolTip1.SetToolTip(this.btnToOutputMoveNext, "Copy to Output and Preview Next Worship List Item");
            this.btnToOutputMoveNext.Click += new System.EventHandler(this.btnToOutputMoveNext_Click);
            // 
            // PreviewPanelDisplayName
            // 
            this.PreviewPanelDisplayName.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader15});
            this.PreviewPanelDisplayName.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PreviewPanelDisplayName.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.PreviewPanelDisplayName.LabelWrap = false;
            this.PreviewPanelDisplayName.Location = new System.Drawing.Point(0, 0);
            this.PreviewPanelDisplayName.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.PreviewPanelDisplayName.MultiSelect = false;
            this.PreviewPanelDisplayName.Name = "PreviewPanelDisplayName";
            this.PreviewPanelDisplayName.Scrollable = false;
            this.PreviewPanelDisplayName.ShowItemToolTips = true;
            this.PreviewPanelDisplayName.Size = new System.Drawing.Size(285, 25);
            this.PreviewPanelDisplayName.SmallImageList = this.imageListSys;
            this.PreviewPanelDisplayName.TabIndex = 7;
            this.PreviewPanelDisplayName.TabStop = false;
            this.PreviewPanelDisplayName.UseCompatibleStateImageBehavior = false;
            this.PreviewPanelDisplayName.View = System.Windows.Forms.View.Details;
            // 
            // btnToOutput
            // 
            this.btnToOutput.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnToOutput.Image = ((System.Drawing.Image)(resources.GetObject("btnToOutput.Image")));
            this.btnToOutput.Location = new System.Drawing.Point(285, 0);
            this.btnToOutput.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnToOutput.Name = "btnToOutput";
            this.btnToOutput.Size = new System.Drawing.Size(37, 25);
            this.btnToOutput.TabIndex = 6;
            this.toolTip1.SetToolTip(this.btnToOutput, "Copy to Output");
            this.btnToOutput.Click += new System.EventHandler(this.btnToOutput_Click);
            // 
            // panelPreviewBottom
            // 
            this.panelPreviewBottom.BackColor = System.Drawing.Color.Gray;
            this.panelPreviewBottom.Controls.Add(this.panelPreviewSessionNotes2);
            this.panelPreviewBottom.Controls.Add(this.PreviewHolder);
            this.panelPreviewBottom.Controls.Add(this.PreviewBack);
            this.panelPreviewBottom.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelPreviewBottom.Location = new System.Drawing.Point(0, 26);
            this.panelPreviewBottom.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.panelPreviewBottom.Name = "panelPreviewBottom";
            this.panelPreviewBottom.Size = new System.Drawing.Size(322, 27);
            this.panelPreviewBottom.TabIndex = 2;
            this.panelPreviewBottom.Resize += new System.EventHandler(this.panelPreviewBottom_Resize);
            // 
            // panelPreviewSessionNotes2
            // 
            this.panelPreviewSessionNotes2.BackColor = System.Drawing.SystemColors.Window;
            this.panelPreviewSessionNotes2.Controls.Add(this.PreviewNotes);
            this.panelPreviewSessionNotes2.Location = new System.Drawing.Point(108, 5);
            this.panelPreviewSessionNotes2.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.panelPreviewSessionNotes2.Name = "panelPreviewSessionNotes2";
            this.panelPreviewSessionNotes2.Size = new System.Drawing.Size(124, 19);
            this.panelPreviewSessionNotes2.TabIndex = 5;
            // 
            // PreviewNotes
            // 
            this.PreviewNotes.BackColor = System.Drawing.SystemColors.Window;
            this.PreviewNotes.Location = new System.Drawing.Point(55, 4);
            this.PreviewNotes.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.PreviewNotes.Name = "PreviewNotes";
            this.PreviewNotes.ReadOnly = true;
            this.PreviewNotes.Size = new System.Drawing.Size(34, 13);
            this.PreviewNotes.TabIndex = 4;
            this.PreviewNotes.Text = "";
            // 
            // PreviewHolder
            // 
            this.PreviewHolder.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(20)))), ((int)(((byte)(20)))));
            this.PreviewHolder.Location = new System.Drawing.Point(8, 5);
            this.PreviewHolder.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.PreviewHolder.Name = "PreviewHolder";
            this.PreviewHolder.Size = new System.Drawing.Size(35, 15);
            this.PreviewHolder.TabIndex = 3;
            // 
            // PreviewBack
            // 
            this.PreviewBack.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(20)))), ((int)(((byte)(20)))));
            this.PreviewBack.Location = new System.Drawing.Point(61, 5);
            this.PreviewBack.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.PreviewBack.Name = "PreviewBack";
            this.PreviewBack.Size = new System.Drawing.Size(35, 15);
            this.PreviewBack.TabIndex = 2;
            // 
            // panel7
            // 
            this.panel7.BackColor = System.Drawing.SystemColors.Control;
            this.panel7.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel7.Location = new System.Drawing.Point(0, 25);
            this.panel7.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.panel7.Name = "panel7";
            this.panel7.Size = new System.Drawing.Size(322, 1);
            this.panel7.TabIndex = 1;
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.SystemColors.Control;
            this.panel1.Controls.Add(this.flowLayoutPanel1);
            this.panel1.Controls.Add(this.panel3);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(322, 25);
            this.panel1.TabIndex = 0;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Controls.Add(this.PreviewBtnVerse1);
            this.flowLayoutPanel1.Controls.Add(this.PreviewBtnVerse2);
            this.flowLayoutPanel1.Controls.Add(this.PreviewBtnVerse3);
            this.flowLayoutPanel1.Controls.Add(this.PreviewBtnVerse4);
            this.flowLayoutPanel1.Controls.Add(this.PreviewBtnVerse5);
            this.flowLayoutPanel1.Controls.Add(this.PreviewBtnVerse6);
            this.flowLayoutPanel1.Controls.Add(this.PreviewBtnVerse7);
            this.flowLayoutPanel1.Controls.Add(this.PreviewBtnVerse8);
            this.flowLayoutPanel1.Controls.Add(this.PreviewBtnVerse9);
            this.flowLayoutPanel1.Controls.Add(this.PreviewBtnVersePreChorus);
            this.flowLayoutPanel1.Controls.Add(this.PreviewBtnVersePreChorus2);
            this.flowLayoutPanel1.Controls.Add(this.PreviewBtnVerseChorus);
            this.flowLayoutPanel1.Controls.Add(this.PreviewBtnVerseChorus2);
            this.flowLayoutPanel1.Controls.Add(this.PreviewBtnVerseBridge);
            this.flowLayoutPanel1.Controls.Add(this.PreviewBtnVerseBridge2);
            this.flowLayoutPanel1.Controls.Add(this.PreviewBtnVerseEnding);
            this.flowLayoutPanel1.Location = new System.Drawing.Point(259, 0);
            this.flowLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(289, 25);
            this.flowLayoutPanel1.TabIndex = 1;
            // 
            // PreviewBtnVerse1
            // 
            this.PreviewBtnVerse1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.PreviewBtnVerse1.Location = new System.Drawing.Point(0, 0);
            this.PreviewBtnVerse1.Margin = new System.Windows.Forms.Padding(0);
            this.PreviewBtnVerse1.Name = "PreviewBtnVerse1";
            this.PreviewBtnVerse1.Size = new System.Drawing.Size(17, 25);
            this.PreviewBtnVerse1.TabIndex = 4;
            this.PreviewBtnVerse1.Tag = "1";
            this.PreviewBtnVerse1.Text = "1";
            this.PreviewBtnVerse1.Click += new System.EventHandler(this.PreviewBtnVerse_Click);
            // 
            // PreviewBtnVerse2
            // 
            this.PreviewBtnVerse2.Dock = System.Windows.Forms.DockStyle.Left;
            this.PreviewBtnVerse2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.PreviewBtnVerse2.Location = new System.Drawing.Point(17, 0);
            this.PreviewBtnVerse2.Margin = new System.Windows.Forms.Padding(0);
            this.PreviewBtnVerse2.Name = "PreviewBtnVerse2";
            this.PreviewBtnVerse2.Size = new System.Drawing.Size(17, 25);
            this.PreviewBtnVerse2.TabIndex = 18;
            this.PreviewBtnVerse2.Tag = "2";
            this.PreviewBtnVerse2.Text = "2";
            this.PreviewBtnVerse2.Click += new System.EventHandler(this.PreviewBtnVerse_Click);
            // 
            // PreviewBtnVerse3
            // 
            this.PreviewBtnVerse3.Dock = System.Windows.Forms.DockStyle.Left;
            this.PreviewBtnVerse3.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.PreviewBtnVerse3.Location = new System.Drawing.Point(34, 0);
            this.PreviewBtnVerse3.Margin = new System.Windows.Forms.Padding(0);
            this.PreviewBtnVerse3.Name = "PreviewBtnVerse3";
            this.PreviewBtnVerse3.Size = new System.Drawing.Size(17, 25);
            this.PreviewBtnVerse3.TabIndex = 19;
            this.PreviewBtnVerse3.Tag = "3";
            this.PreviewBtnVerse3.Text = "3";
            this.PreviewBtnVerse3.Click += new System.EventHandler(this.PreviewBtnVerse_Click);
            // 
            // PreviewBtnVerse4
            // 
            this.PreviewBtnVerse4.Dock = System.Windows.Forms.DockStyle.Left;
            this.PreviewBtnVerse4.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.PreviewBtnVerse4.Location = new System.Drawing.Point(51, 0);
            this.PreviewBtnVerse4.Margin = new System.Windows.Forms.Padding(0);
            this.PreviewBtnVerse4.Name = "PreviewBtnVerse4";
            this.PreviewBtnVerse4.Size = new System.Drawing.Size(17, 25);
            this.PreviewBtnVerse4.TabIndex = 20;
            this.PreviewBtnVerse4.Tag = "4";
            this.PreviewBtnVerse4.Text = "4";
            this.PreviewBtnVerse4.Click += new System.EventHandler(this.PreviewBtnVerse_Click);
            // 
            // PreviewBtnVerse5
            // 
            this.PreviewBtnVerse5.Dock = System.Windows.Forms.DockStyle.Left;
            this.PreviewBtnVerse5.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.PreviewBtnVerse5.Location = new System.Drawing.Point(68, 0);
            this.PreviewBtnVerse5.Margin = new System.Windows.Forms.Padding(0);
            this.PreviewBtnVerse5.Name = "PreviewBtnVerse5";
            this.PreviewBtnVerse5.Size = new System.Drawing.Size(17, 25);
            this.PreviewBtnVerse5.TabIndex = 21;
            this.PreviewBtnVerse5.Tag = "5";
            this.PreviewBtnVerse5.Text = "5";
            this.PreviewBtnVerse5.Click += new System.EventHandler(this.PreviewBtnVerse_Click);
            // 
            // PreviewBtnVerse6
            // 
            this.PreviewBtnVerse6.Dock = System.Windows.Forms.DockStyle.Left;
            this.PreviewBtnVerse6.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.PreviewBtnVerse6.Location = new System.Drawing.Point(85, 0);
            this.PreviewBtnVerse6.Margin = new System.Windows.Forms.Padding(0);
            this.PreviewBtnVerse6.Name = "PreviewBtnVerse6";
            this.PreviewBtnVerse6.Size = new System.Drawing.Size(17, 25);
            this.PreviewBtnVerse6.TabIndex = 22;
            this.PreviewBtnVerse6.Tag = "6";
            this.PreviewBtnVerse6.Text = "6";
            this.PreviewBtnVerse6.Click += new System.EventHandler(this.PreviewBtnVerse_Click);
            // 
            // PreviewBtnVerse7
            // 
            this.PreviewBtnVerse7.Dock = System.Windows.Forms.DockStyle.Left;
            this.PreviewBtnVerse7.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.PreviewBtnVerse7.Location = new System.Drawing.Point(102, 0);
            this.PreviewBtnVerse7.Margin = new System.Windows.Forms.Padding(0);
            this.PreviewBtnVerse7.Name = "PreviewBtnVerse7";
            this.PreviewBtnVerse7.Size = new System.Drawing.Size(17, 25);
            this.PreviewBtnVerse7.TabIndex = 23;
            this.PreviewBtnVerse7.Tag = "7";
            this.PreviewBtnVerse7.Text = "7";
            this.PreviewBtnVerse7.Click += new System.EventHandler(this.PreviewBtnVerse_Click);
            // 
            // PreviewBtnVerse8
            // 
            this.PreviewBtnVerse8.Dock = System.Windows.Forms.DockStyle.Left;
            this.PreviewBtnVerse8.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.PreviewBtnVerse8.Location = new System.Drawing.Point(119, 0);
            this.PreviewBtnVerse8.Margin = new System.Windows.Forms.Padding(0);
            this.PreviewBtnVerse8.Name = "PreviewBtnVerse8";
            this.PreviewBtnVerse8.Size = new System.Drawing.Size(17, 25);
            this.PreviewBtnVerse8.TabIndex = 24;
            this.PreviewBtnVerse8.Tag = "8";
            this.PreviewBtnVerse8.Text = "8";
            this.PreviewBtnVerse8.Click += new System.EventHandler(this.PreviewBtnVerse_Click);
            // 
            // PreviewBtnVerse9
            // 
            this.PreviewBtnVerse9.Dock = System.Windows.Forms.DockStyle.Left;
            this.PreviewBtnVerse9.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.PreviewBtnVerse9.Location = new System.Drawing.Point(136, 0);
            this.PreviewBtnVerse9.Margin = new System.Windows.Forms.Padding(0);
            this.PreviewBtnVerse9.Name = "PreviewBtnVerse9";
            this.PreviewBtnVerse9.Size = new System.Drawing.Size(17, 25);
            this.PreviewBtnVerse9.TabIndex = 25;
            this.PreviewBtnVerse9.Tag = "9";
            this.PreviewBtnVerse9.Text = "9";
            this.PreviewBtnVerse9.Click += new System.EventHandler(this.PreviewBtnVerse_Click);
            // 
            // PreviewBtnVersePreChorus
            // 
            this.PreviewBtnVersePreChorus.Dock = System.Windows.Forms.DockStyle.Left;
            this.PreviewBtnVersePreChorus.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.PreviewBtnVersePreChorus.Location = new System.Drawing.Point(153, 0);
            this.PreviewBtnVersePreChorus.Margin = new System.Windows.Forms.Padding(0);
            this.PreviewBtnVersePreChorus.Name = "PreviewBtnVersePreChorus";
            this.PreviewBtnVersePreChorus.Size = new System.Drawing.Size(17, 25);
            this.PreviewBtnVersePreChorus.TabIndex = 26;
            this.PreviewBtnVersePreChorus.Tag = "111";
            this.PreviewBtnVersePreChorus.Text = "p";
            this.PreviewBtnVersePreChorus.Click += new System.EventHandler(this.PreviewBtnVerse_Click);
            // 
            // PreviewBtnVersePreChorus2
            // 
            this.PreviewBtnVersePreChorus2.Dock = System.Windows.Forms.DockStyle.Left;
            this.PreviewBtnVersePreChorus2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.PreviewBtnVersePreChorus2.Location = new System.Drawing.Point(170, 0);
            this.PreviewBtnVersePreChorus2.Margin = new System.Windows.Forms.Padding(0);
            this.PreviewBtnVersePreChorus2.Name = "PreviewBtnVersePreChorus2";
            this.PreviewBtnVersePreChorus2.Size = new System.Drawing.Size(17, 25);
            this.PreviewBtnVersePreChorus2.TabIndex = 27;
            this.PreviewBtnVersePreChorus2.Tag = "112";
            this.PreviewBtnVersePreChorus2.Text = "q";
            this.PreviewBtnVersePreChorus2.Click += new System.EventHandler(this.PreviewBtnVerse_Click);
            // 
            // PreviewBtnVerseChorus
            // 
            this.PreviewBtnVerseChorus.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.PreviewBtnVerseChorus.Location = new System.Drawing.Point(187, 0);
            this.PreviewBtnVerseChorus.Margin = new System.Windows.Forms.Padding(0);
            this.PreviewBtnVerseChorus.Name = "PreviewBtnVerseChorus";
            this.PreviewBtnVerseChorus.Size = new System.Drawing.Size(17, 25);
            this.PreviewBtnVerseChorus.TabIndex = 28;
            this.PreviewBtnVerseChorus.Tag = "0";
            this.PreviewBtnVerseChorus.Text = "c";
            this.PreviewBtnVerseChorus.Click += new System.EventHandler(this.PreviewBtnVerse_Click);
            // 
            // PreviewBtnVerseChorus2
            // 
            this.PreviewBtnVerseChorus2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.PreviewBtnVerseChorus2.Location = new System.Drawing.Point(204, 0);
            this.PreviewBtnVerseChorus2.Margin = new System.Windows.Forms.Padding(0);
            this.PreviewBtnVerseChorus2.Name = "PreviewBtnVerseChorus2";
            this.PreviewBtnVerseChorus2.Size = new System.Drawing.Size(17, 25);
            this.PreviewBtnVerseChorus2.TabIndex = 30;
            this.PreviewBtnVerseChorus2.Tag = "102";
            this.PreviewBtnVerseChorus2.Text = "t";
            this.PreviewBtnVerseChorus2.Click += new System.EventHandler(this.PreviewBtnVerse_Click);
            // 
            // PreviewBtnVerseBridge
            // 
            this.PreviewBtnVerseBridge.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.PreviewBtnVerseBridge.Location = new System.Drawing.Point(221, 0);
            this.PreviewBtnVerseBridge.Margin = new System.Windows.Forms.Padding(0);
            this.PreviewBtnVerseBridge.Name = "PreviewBtnVerseBridge";
            this.PreviewBtnVerseBridge.Size = new System.Drawing.Size(17, 25);
            this.PreviewBtnVerseBridge.TabIndex = 29;
            this.PreviewBtnVerseBridge.Tag = "100";
            this.PreviewBtnVerseBridge.Text = "b";
            this.PreviewBtnVerseBridge.Click += new System.EventHandler(this.PreviewBtnVerse_Click);
            // 
            // PreviewBtnVerseBridge2
            // 
            this.PreviewBtnVerseBridge2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.PreviewBtnVerseBridge2.Location = new System.Drawing.Point(238, 0);
            this.PreviewBtnVerseBridge2.Margin = new System.Windows.Forms.Padding(0);
            this.PreviewBtnVerseBridge2.Name = "PreviewBtnVerseBridge2";
            this.PreviewBtnVerseBridge2.Size = new System.Drawing.Size(20, 25);
            this.PreviewBtnVerseBridge2.TabIndex = 32;
            this.PreviewBtnVerseBridge2.Tag = "103";
            this.PreviewBtnVerseBridge2.Text = "w";
            this.PreviewBtnVerseBridge2.Click += new System.EventHandler(this.PreviewBtnVerse_Click);
            // 
            // PreviewBtnVerseEnding
            // 
            this.PreviewBtnVerseEnding.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.PreviewBtnVerseEnding.Location = new System.Drawing.Point(258, 0);
            this.PreviewBtnVerseEnding.Margin = new System.Windows.Forms.Padding(0);
            this.PreviewBtnVerseEnding.Name = "PreviewBtnVerseEnding";
            this.PreviewBtnVerseEnding.Size = new System.Drawing.Size(17, 25);
            this.PreviewBtnVerseEnding.TabIndex = 31;
            this.PreviewBtnVerseEnding.Tag = "101";
            this.PreviewBtnVerseEnding.Text = "e";
            this.PreviewBtnVerseEnding.Click += new System.EventHandler(this.PreviewBtnVerse_Click);
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.PreviewBtnSlideDown);
            this.panel3.Controls.Add(this.PreviewBtnSlideUp);
            this.panel3.Controls.Add(this.PreviewBtnItemDown);
            this.panel3.Controls.Add(this.PreviewBtnItemUp);
            this.panel3.Controls.Add(this.IndradioButtonInfo);
            this.panel3.Controls.Add(this.IndradioButtonFormat);
            this.panel3.Controls.Add(this.IndradioButtonText);
            this.panel3.Controls.Add(this.IndcbPreviewNotes);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Left;
            this.panel3.Location = new System.Drawing.Point(0, 0);
            this.panel3.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(257, 25);
            this.panel3.TabIndex = 0;
            // 
            // PreviewBtnSlideDown
            // 
            this.PreviewBtnSlideDown.Dock = System.Windows.Forms.DockStyle.Left;
            this.PreviewBtnSlideDown.Image = ((System.Drawing.Image)(resources.GetObject("PreviewBtnSlideDown.Image")));
            this.PreviewBtnSlideDown.Location = new System.Drawing.Point(209, 0);
            this.PreviewBtnSlideDown.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.PreviewBtnSlideDown.Name = "PreviewBtnSlideDown";
            this.PreviewBtnSlideDown.Size = new System.Drawing.Size(26, 25);
            this.PreviewBtnSlideDown.TabIndex = 3;
            this.toolTip1.SetToolTip(this.PreviewBtnSlideDown, "Next Slide");
            this.PreviewBtnSlideDown.Click += new System.EventHandler(this.PreviewBtnUpDown_Click);
            // 
            // PreviewBtnSlideUp
            // 
            this.PreviewBtnSlideUp.Dock = System.Windows.Forms.DockStyle.Left;
            this.PreviewBtnSlideUp.Image = ((System.Drawing.Image)(resources.GetObject("PreviewBtnSlideUp.Image")));
            this.PreviewBtnSlideUp.Location = new System.Drawing.Point(183, 0);
            this.PreviewBtnSlideUp.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.PreviewBtnSlideUp.Name = "PreviewBtnSlideUp";
            this.PreviewBtnSlideUp.Size = new System.Drawing.Size(26, 25);
            this.PreviewBtnSlideUp.TabIndex = 2;
            this.toolTip1.SetToolTip(this.PreviewBtnSlideUp, "Previous Slide");
            this.PreviewBtnSlideUp.Click += new System.EventHandler(this.PreviewBtnUpDown_Click);
            // 
            // PreviewBtnItemDown
            // 
            this.PreviewBtnItemDown.Dock = System.Windows.Forms.DockStyle.Left;
            this.PreviewBtnItemDown.Image = ((System.Drawing.Image)(resources.GetObject("PreviewBtnItemDown.Image")));
            this.PreviewBtnItemDown.Location = new System.Drawing.Point(157, 0);
            this.PreviewBtnItemDown.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.PreviewBtnItemDown.Name = "PreviewBtnItemDown";
            this.PreviewBtnItemDown.Size = new System.Drawing.Size(26, 25);
            this.PreviewBtnItemDown.TabIndex = 7;
            this.toolTip1.SetToolTip(this.PreviewBtnItemDown, "Next Item");
            this.PreviewBtnItemDown.Click += new System.EventHandler(this.PreviewBtnUpDown_Click);
            // 
            // PreviewBtnItemUp
            // 
            this.PreviewBtnItemUp.Dock = System.Windows.Forms.DockStyle.Left;
            this.PreviewBtnItemUp.Image = ((System.Drawing.Image)(resources.GetObject("PreviewBtnItemUp.Image")));
            this.PreviewBtnItemUp.Location = new System.Drawing.Point(131, 0);
            this.PreviewBtnItemUp.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.PreviewBtnItemUp.Name = "PreviewBtnItemUp";
            this.PreviewBtnItemUp.Size = new System.Drawing.Size(26, 25);
            this.PreviewBtnItemUp.TabIndex = 6;
            this.toolTip1.SetToolTip(this.PreviewBtnItemUp, "Previous Item");
            this.PreviewBtnItemUp.Click += new System.EventHandler(this.PreviewBtnUpDown_Click);
            // 
            // IndradioButtonInfo
            // 
            this.IndradioButtonInfo.Appearance = System.Windows.Forms.Appearance.Button;
            this.IndradioButtonInfo.AutoSize = true;
            this.IndradioButtonInfo.Dock = System.Windows.Forms.DockStyle.Left;
            this.IndradioButtonInfo.Location = new System.Drawing.Point(93, 0);
            this.IndradioButtonInfo.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.IndradioButtonInfo.Name = "IndradioButtonInfo";
            this.IndradioButtonInfo.Size = new System.Drawing.Size(38, 25);
            this.IndradioButtonInfo.TabIndex = 8;
            this.IndradioButtonInfo.Text = "Info";
            this.IndradioButtonInfo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.IndradioButtonInfo.Click += new System.EventHandler(this.IndradioButtonTextFormatInfo_Click);
            // 
            // IndradioButtonFormat
            // 
            this.IndradioButtonFormat.Appearance = System.Windows.Forms.Appearance.Button;
            this.IndradioButtonFormat.AutoSize = true;
            this.IndradioButtonFormat.Dock = System.Windows.Forms.DockStyle.Left;
            this.IndradioButtonFormat.Location = new System.Drawing.Point(60, 0);
            this.IndradioButtonFormat.Margin = new System.Windows.Forms.Padding(0, 4, 0, 4);
            this.IndradioButtonFormat.Name = "IndradioButtonFormat";
            this.IndradioButtonFormat.Size = new System.Drawing.Size(33, 25);
            this.IndradioButtonFormat.TabIndex = 5;
            this.IndradioButtonFormat.Text = "Set";
            this.IndradioButtonFormat.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.toolTip1.SetToolTip(this.IndradioButtonFormat, "Format Text");
            this.IndradioButtonFormat.Click += new System.EventHandler(this.IndradioButtonTextFormatInfo_Click);
            // 
            // IndradioButtonText
            // 
            this.IndradioButtonText.Appearance = System.Windows.Forms.Appearance.Button;
            this.IndradioButtonText.AutoSize = true;
            this.IndradioButtonText.Dock = System.Windows.Forms.DockStyle.Left;
            this.IndradioButtonText.Location = new System.Drawing.Point(22, 0);
            this.IndradioButtonText.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.IndradioButtonText.Name = "IndradioButtonText";
            this.IndradioButtonText.Size = new System.Drawing.Size(38, 25);
            this.IndradioButtonText.TabIndex = 4;
            this.IndradioButtonText.Text = "Text";
            this.IndradioButtonText.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.IndradioButtonText.Click += new System.EventHandler(this.IndradioButtonTextFormatInfo_Click);
            // 
            // IndcbPreviewNotes
            // 
            this.IndcbPreviewNotes.Appearance = System.Windows.Forms.Appearance.Button;
            this.IndcbPreviewNotes.AutoSize = true;
            this.IndcbPreviewNotes.Dock = System.Windows.Forms.DockStyle.Left;
            this.IndcbPreviewNotes.Image = ((System.Drawing.Image)(resources.GetObject("IndcbPreviewNotes.Image")));
            this.IndcbPreviewNotes.Location = new System.Drawing.Point(0, 0);
            this.IndcbPreviewNotes.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.IndcbPreviewNotes.Name = "IndcbPreviewNotes";
            this.IndcbPreviewNotes.Size = new System.Drawing.Size(22, 25);
            this.IndcbPreviewNotes.TabIndex = 11;
            this.toolTip1.SetToolTip(this.IndcbPreviewNotes, "Show Session Notes");
            this.IndcbPreviewNotes.UseVisualStyleBackColor = true;
            this.IndcbPreviewNotes.Click += new System.EventHandler(this.IndcbPreviewNotes_Click);
            // 
            // splitContainerOutput
            // 
            this.splitContainerOutput.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerOutput.Location = new System.Drawing.Point(0, 0);
            this.splitContainerOutput.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.splitContainerOutput.Name = "splitContainerOutput";
            this.splitContainerOutput.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainerOutput.Panel1
            // 
            this.splitContainerOutput.Panel1.Controls.Add(this.panelOutputTop);
            this.splitContainerOutput.Panel1.Controls.Add(this.panel10);
            this.splitContainerOutput.Panel1MinSize = 50;
            // 
            // splitContainerOutput.Panel2
            // 
            this.splitContainerOutput.Panel2.BackColor = System.Drawing.SystemColors.Control;
            this.splitContainerOutput.Panel2.Controls.Add(this.panelOutputBottom);
            this.splitContainerOutput.Panel2.Controls.Add(this.panel8);
            this.splitContainerOutput.Panel2.Controls.Add(this.panel2);
            this.splitContainerOutput.Size = new System.Drawing.Size(214, 486);
            this.splitContainerOutput.SplitterDistance = 358;
            this.splitContainerOutput.TabIndex = 0;
            this.splitContainerOutput.Text = "splitContainer3";
            // 
            // panelOutputTop
            // 
            this.panelOutputTop.Controls.Add(this.flowLayoutOutputPowerPoint);
            this.panelOutputTop.Controls.Add(this.flowLayoutOutputLyrics);
            this.panelOutputTop.Controls.Add(this.OutputInfo);
            this.panelOutputTop.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelOutputTop.Location = new System.Drawing.Point(0, 25);
            this.panelOutputTop.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.panelOutputTop.Name = "panelOutputTop";
            this.panelOutputTop.Size = new System.Drawing.Size(214, 333);
            this.panelOutputTop.TabIndex = 0;
            this.panelOutputTop.Resize += new System.EventHandler(this.panelOutputTop_Resize);
            // 
            // flowLayoutOutputPowerPoint
            // 
            this.flowLayoutOutputPowerPoint.AutoScroll = true;
            this.flowLayoutOutputPowerPoint.Location = new System.Drawing.Point(13, 165);
            this.flowLayoutOutputPowerPoint.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.flowLayoutOutputPowerPoint.Name = "flowLayoutOutputPowerPoint";
            this.flowLayoutOutputPowerPoint.Size = new System.Drawing.Size(69, 40);
            this.flowLayoutOutputPowerPoint.TabIndex = 6;
            // 
            // flowLayoutOutputLyrics
            // 
            this.flowLayoutOutputLyrics.AutoScroll = true;
            this.flowLayoutOutputLyrics.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowLayoutOutputLyrics.BackColor = System.Drawing.SystemColors.Window;
            this.flowLayoutOutputLyrics.Location = new System.Drawing.Point(13, 25);
            this.flowLayoutOutputLyrics.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.flowLayoutOutputLyrics.Name = "flowLayoutOutputLyrics";
            this.flowLayoutOutputLyrics.Size = new System.Drawing.Size(73, 51);
            this.flowLayoutOutputLyrics.TabIndex = 7;
            this.flowLayoutOutputLyrics.Click += new System.EventHandler(this.flowLayoutOutputLyrics_Click);
            this.flowLayoutOutputLyrics.Leave += new System.EventHandler(this.flowLayoutOutputLyrics_Leave);
            // 
            // OutputInfo
            // 
            this.OutputInfo.Location = new System.Drawing.Point(13, 8);
            this.OutputInfo.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.OutputInfo.Name = "OutputInfo";
            this.OutputInfo.Size = new System.Drawing.Size(18, 23);
            this.OutputInfo.TabIndex = 9;
            this.OutputInfo.KeyUp += new System.Windows.Forms.KeyEventHandler(this.OutputInfo_KeyUp);
            // 
            // panel10
            // 
            this.panel10.Controls.Add(this.cbOutputBlack);
            this.panel10.Controls.Add(this.cbOutputClear);
            this.panel10.Controls.Add(this.cbOutputCam);
            this.panel10.Controls.Add(this.OutputPanelDisplayName);
            this.panel10.Controls.Add(this.cbGoLive);
            this.panel10.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel10.Location = new System.Drawing.Point(0, 0);
            this.panel10.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.panel10.Name = "panel10";
            this.panel10.Size = new System.Drawing.Size(214, 25);
            this.panel10.TabIndex = 1;
            // 
            // cbOutputBlack
            // 
            this.cbOutputBlack.Appearance = System.Windows.Forms.Appearance.Button;
            this.cbOutputBlack.Dock = System.Windows.Forms.DockStyle.Right;
            this.cbOutputBlack.ImageIndex = 15;
            this.cbOutputBlack.ImageList = this.imageListSys;
            this.cbOutputBlack.Location = new System.Drawing.Point(60, 0);
            this.cbOutputBlack.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.cbOutputBlack.Name = "cbOutputBlack";
            this.cbOutputBlack.Size = new System.Drawing.Size(35, 25);
            this.cbOutputBlack.TabIndex = 2;
            this.cbOutputBlack.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.toolTip1.SetToolTip(this.cbOutputBlack, "Black Screen");
            this.cbOutputBlack.Click += new System.EventHandler(this.cbOutputBlack_Click);
            // 
            // cbOutputClear
            // 
            this.cbOutputClear.Appearance = System.Windows.Forms.Appearance.Button;
            this.cbOutputClear.Dock = System.Windows.Forms.DockStyle.Right;
            this.cbOutputClear.ImageIndex = 17;
            this.cbOutputClear.ImageList = this.imageListSys;
            this.cbOutputClear.Location = new System.Drawing.Point(95, 0);
            this.cbOutputClear.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.cbOutputClear.Name = "cbOutputClear";
            this.cbOutputClear.Size = new System.Drawing.Size(35, 25);
            this.cbOutputClear.TabIndex = 1;
            this.cbOutputClear.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.toolTip1.SetToolTip(this.cbOutputClear, "Hide Text");
            this.cbOutputClear.Click += new System.EventHandler(this.cbOutputClear_Click);
            // 
            // cbOutputCam
            // 
            this.cbOutputCam.Appearance = System.Windows.Forms.Appearance.Button;
            this.cbOutputCam.Dock = System.Windows.Forms.DockStyle.Right;
            this.cbOutputCam.ImageIndex = 30;
            this.cbOutputCam.ImageList = this.imageListSys;
            this.cbOutputCam.Location = new System.Drawing.Point(130, 0);
            this.cbOutputCam.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.cbOutputCam.Name = "cbOutputCam";
            this.cbOutputCam.Size = new System.Drawing.Size(35, 25);
            this.cbOutputCam.TabIndex = 9;
            this.cbOutputCam.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.toolTip1.SetToolTip(this.cbOutputCam, "Live Cam");
            this.cbOutputCam.Click += new System.EventHandler(this.cbOutputCam_Click);
            // 
            // OutputPanelDisplayName
            // 
            this.OutputPanelDisplayName.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader16});
            this.OutputPanelDisplayName.Dock = System.Windows.Forms.DockStyle.Fill;
            this.OutputPanelDisplayName.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.OutputPanelDisplayName.LabelWrap = false;
            this.OutputPanelDisplayName.Location = new System.Drawing.Point(0, 0);
            this.OutputPanelDisplayName.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.OutputPanelDisplayName.MultiSelect = false;
            this.OutputPanelDisplayName.Name = "OutputPanelDisplayName";
            this.OutputPanelDisplayName.Scrollable = false;
            this.OutputPanelDisplayName.ShowItemToolTips = true;
            this.OutputPanelDisplayName.Size = new System.Drawing.Size(165, 25);
            this.OutputPanelDisplayName.SmallImageList = this.imageListSys;
            this.OutputPanelDisplayName.TabIndex = 8;
            this.OutputPanelDisplayName.TabStop = false;
            this.OutputPanelDisplayName.UseCompatibleStateImageBehavior = false;
            this.OutputPanelDisplayName.View = System.Windows.Forms.View.Details;
            // 
            // cbGoLive
            // 
            this.cbGoLive.Appearance = System.Windows.Forms.Appearance.Button;
            this.cbGoLive.Dock = System.Windows.Forms.DockStyle.Right;
            this.cbGoLive.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.cbGoLive.Location = new System.Drawing.Point(165, 0);
            this.cbGoLive.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.cbGoLive.Name = "cbGoLive";
            this.cbGoLive.Size = new System.Drawing.Size(49, 25);
            this.cbGoLive.TabIndex = 3;
            this.cbGoLive.Text = "LIVE";
            this.cbGoLive.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.toolTip1.SetToolTip(this.cbGoLive, "Start Show");
            this.cbGoLive.Click += new System.EventHandler(this.cbGoLive_Click);
            // 
            // panelOutputBottom
            // 
            this.panelOutputBottom.BackColor = System.Drawing.Color.Gray;
            this.panelOutputBottom.Controls.Add(this.panelOutputLM1);
            this.panelOutputBottom.Controls.Add(this.OutputHolder);
            this.panelOutputBottom.Controls.Add(this.OutputBack);
            this.panelOutputBottom.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelOutputBottom.Location = new System.Drawing.Point(0, 46);
            this.panelOutputBottom.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.panelOutputBottom.Name = "panelOutputBottom";
            this.panelOutputBottom.Size = new System.Drawing.Size(214, 78);
            this.panelOutputBottom.TabIndex = 3;
            this.panelOutputBottom.Resize += new System.EventHandler(this.panelOutputBottom_Resize);
            // 
            // panelOutputLM1
            // 
            this.panelOutputLM1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.panelOutputLM1.Controls.Add(this.OutputTextBoxLM);
            this.panelOutputLM1.Controls.Add(this.panelOutputLM2);
            this.panelOutputLM1.Controls.Add(this.panelOutputLM3);
            this.panelOutputLM1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelOutputLM1.Location = new System.Drawing.Point(0, 52);
            this.panelOutputLM1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.panelOutputLM1.Name = "panelOutputLM1";
            this.panelOutputLM1.Size = new System.Drawing.Size(214, 26);
            this.panelOutputLM1.TabIndex = 7;
            // 
            // OutputTextBoxLM
            // 
            this.OutputTextBoxLM.Dock = System.Windows.Forms.DockStyle.Fill;
            this.OutputTextBoxLM.Location = new System.Drawing.Point(0, 2);
            this.OutputTextBoxLM.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.OutputTextBoxLM.Name = "OutputTextBoxLM";
            this.OutputTextBoxLM.Size = new System.Drawing.Size(156, 23);
            this.OutputTextBoxLM.TabIndex = 15;
            this.OutputTextBoxLM.WordWrap = false;
            this.OutputTextBoxLM.KeyUp += new System.Windows.Forms.KeyEventHandler(this.OutputTextBoxLM_KeyUp);
            // 
            // panelOutputLM2
            // 
            this.panelOutputLM2.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelOutputLM2.Location = new System.Drawing.Point(0, 0);
            this.panelOutputLM2.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.panelOutputLM2.Name = "panelOutputLM2";
            this.panelOutputLM2.Size = new System.Drawing.Size(156, 2);
            this.panelOutputLM2.TabIndex = 14;
            // 
            // panelOutputLM3
            // 
            this.panelOutputLM3.Controls.Add(this.OutputBtnLMSend);
            this.panelOutputLM3.Controls.Add(this.OutputBtnLMClear);
            this.panelOutputLM3.Dock = System.Windows.Forms.DockStyle.Right;
            this.panelOutputLM3.Location = new System.Drawing.Point(156, 0);
            this.panelOutputLM3.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.panelOutputLM3.Name = "panelOutputLM3";
            this.panelOutputLM3.Size = new System.Drawing.Size(54, 22);
            this.panelOutputLM3.TabIndex = 13;
            // 
            // OutputBtnLMSend
            // 
            this.OutputBtnLMSend.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.OutputBtnLMSend.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.OutputBtnLMSend.Image = ((System.Drawing.Image)(resources.GetObject("OutputBtnLMSend.Image")));
            this.OutputBtnLMSend.Location = new System.Drawing.Point(1, -1);
            this.OutputBtnLMSend.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.OutputBtnLMSend.Name = "OutputBtnLMSend";
            this.OutputBtnLMSend.Size = new System.Drawing.Size(27, 25);
            this.OutputBtnLMSend.TabIndex = 9;
            this.toolTip1.SetToolTip(this.OutputBtnLMSend, "Send Message to Lyrics Monitor");
            this.OutputBtnLMSend.UseVisualStyleBackColor = false;
            this.OutputBtnLMSend.Click += new System.EventHandler(this.OutputBtnLMSend_Click);
            // 
            // OutputBtnLMClear
            // 
            this.OutputBtnLMClear.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.OutputBtnLMClear.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.OutputBtnLMClear.Image = ((System.Drawing.Image)(resources.GetObject("OutputBtnLMClear.Image")));
            this.OutputBtnLMClear.Location = new System.Drawing.Point(27, -1);
            this.OutputBtnLMClear.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.OutputBtnLMClear.Name = "OutputBtnLMClear";
            this.OutputBtnLMClear.Size = new System.Drawing.Size(27, 25);
            this.OutputBtnLMClear.TabIndex = 11;
            this.toolTip1.SetToolTip(this.OutputBtnLMClear, "Clear Lyrics Monitor Message");
            this.OutputBtnLMClear.UseVisualStyleBackColor = false;
            this.OutputBtnLMClear.Click += new System.EventHandler(this.OutputBtnLMClear_Click);
            // 
            // OutputHolder
            // 
            this.OutputHolder.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(20)))), ((int)(((byte)(20)))));
            this.OutputHolder.Location = new System.Drawing.Point(3, 44);
            this.OutputHolder.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.OutputHolder.Name = "OutputHolder";
            this.OutputHolder.Size = new System.Drawing.Size(35, 15);
            this.OutputHolder.TabIndex = 5;
            // 
            // OutputBack
            // 
            this.OutputBack.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(20)))), ((int)(((byte)(20)))));
            this.OutputBack.Location = new System.Drawing.Point(56, 44);
            this.OutputBack.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.OutputBack.Name = "OutputBack";
            this.OutputBack.Size = new System.Drawing.Size(35, 15);
            this.OutputBack.TabIndex = 4;
            // 
            // panel8
            // 
            this.panel8.BackColor = System.Drawing.SystemColors.Control;
            this.panel8.Controls.Add(this.labelGapItem);
            this.panel8.Controls.Add(this.labelHideText);
            this.panel8.Controls.Add(this.labelBlackScreen);
            this.panel8.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel8.Location = new System.Drawing.Point(0, 25);
            this.panel8.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.panel8.Name = "panel8";
            this.panel8.Size = new System.Drawing.Size(214, 21);
            this.panel8.TabIndex = 2;
            // 
            // labelGapItem
            // 
            this.labelGapItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.labelGapItem.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.labelGapItem.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.labelGapItem.Location = new System.Drawing.Point(127, 2);
            this.labelGapItem.Name = "labelGapItem";
            this.labelGapItem.Size = new System.Drawing.Size(77, 18);
            this.labelGapItem.TabIndex = 6;
            this.labelGapItem.Text = "Gap Item";
            this.labelGapItem.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.labelGapItem.Visible = false;
            // 
            // labelHideText
            // 
            this.labelHideText.BackColor = System.Drawing.Color.PowderBlue;
            this.labelHideText.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.labelHideText.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.labelHideText.Location = new System.Drawing.Point(73, 2);
            this.labelHideText.Name = "labelHideText";
            this.labelHideText.Size = new System.Drawing.Size(77, 18);
            this.labelHideText.TabIndex = 1;
            this.labelHideText.Text = "Hide Text";
            this.labelHideText.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.labelHideText.Visible = false;
            // 
            // labelBlackScreen
            // 
            this.labelBlackScreen.BackColor = System.Drawing.Color.White;
            this.labelBlackScreen.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.labelBlackScreen.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.labelBlackScreen.ForeColor = System.Drawing.Color.Black;
            this.labelBlackScreen.Location = new System.Drawing.Point(0, 2);
            this.labelBlackScreen.Name = "labelBlackScreen";
            this.labelBlackScreen.Size = new System.Drawing.Size(98, 18);
            this.labelBlackScreen.TabIndex = 0;
            this.labelBlackScreen.Text = "Black Screen";
            this.labelBlackScreen.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.labelBlackScreen.Visible = false;
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.SystemColors.Control;
            this.panel2.Controls.Add(this.flowLayoutPanel2);
            this.panel2.Controls.Add(this.panel6);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel2.Location = new System.Drawing.Point(0, 0);
            this.panel2.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(214, 25);
            this.panel2.TabIndex = 1;
            // 
            // flowLayoutPanel2
            // 
            this.flowLayoutPanel2.Controls.Add(this.OutputBtnVerse1);
            this.flowLayoutPanel2.Controls.Add(this.OutputBtnVerse2);
            this.flowLayoutPanel2.Controls.Add(this.OutputBtnVerse3);
            this.flowLayoutPanel2.Controls.Add(this.OutputBtnVerse4);
            this.flowLayoutPanel2.Controls.Add(this.OutputBtnVerse5);
            this.flowLayoutPanel2.Controls.Add(this.OutputBtnVerse6);
            this.flowLayoutPanel2.Controls.Add(this.OutputBtnVerse7);
            this.flowLayoutPanel2.Controls.Add(this.OutputBtnVerse8);
            this.flowLayoutPanel2.Controls.Add(this.OutputBtnVerse9);
            this.flowLayoutPanel2.Controls.Add(this.OutputBtnVersePreChorus);
            this.flowLayoutPanel2.Controls.Add(this.OutputBtnVersePreChorus2);
            this.flowLayoutPanel2.Controls.Add(this.OutputBtnVerseChorus);
            this.flowLayoutPanel2.Controls.Add(this.OutputBtnVerseChorus2);
            this.flowLayoutPanel2.Controls.Add(this.OutputBtnVerseBridge);
            this.flowLayoutPanel2.Controls.Add(this.OutputBtnVerseBridge2);
            this.flowLayoutPanel2.Controls.Add(this.OutputBtnVerseEnding);
            this.flowLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Left;
            this.flowLayoutPanel2.Location = new System.Drawing.Point(189, 0);
            this.flowLayoutPanel2.Margin = new System.Windows.Forms.Padding(0);
            this.flowLayoutPanel2.Name = "flowLayoutPanel2";
            this.flowLayoutPanel2.Size = new System.Drawing.Size(293, 25);
            this.flowLayoutPanel2.TabIndex = 8;
            // 
            // OutputBtnVerse1
            // 
            this.OutputBtnVerse1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.OutputBtnVerse1.Location = new System.Drawing.Point(0, 0);
            this.OutputBtnVerse1.Margin = new System.Windows.Forms.Padding(0);
            this.OutputBtnVerse1.Name = "OutputBtnVerse1";
            this.OutputBtnVerse1.Size = new System.Drawing.Size(17, 25);
            this.OutputBtnVerse1.TabIndex = 20;
            this.OutputBtnVerse1.Tag = "1";
            this.OutputBtnVerse1.Text = "1";
            this.OutputBtnVerse1.Click += new System.EventHandler(this.OutputBtnVerse_Click);
            // 
            // OutputBtnVerse2
            // 
            this.OutputBtnVerse2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.OutputBtnVerse2.Location = new System.Drawing.Point(17, 0);
            this.OutputBtnVerse2.Margin = new System.Windows.Forms.Padding(0);
            this.OutputBtnVerse2.Name = "OutputBtnVerse2";
            this.OutputBtnVerse2.Size = new System.Drawing.Size(17, 25);
            this.OutputBtnVerse2.TabIndex = 21;
            this.OutputBtnVerse2.Tag = "2";
            this.OutputBtnVerse2.Text = "2";
            this.OutputBtnVerse2.Click += new System.EventHandler(this.OutputBtnVerse_Click);
            // 
            // OutputBtnVerse3
            // 
            this.OutputBtnVerse3.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.OutputBtnVerse3.Location = new System.Drawing.Point(34, 0);
            this.OutputBtnVerse3.Margin = new System.Windows.Forms.Padding(0);
            this.OutputBtnVerse3.Name = "OutputBtnVerse3";
            this.OutputBtnVerse3.Size = new System.Drawing.Size(17, 25);
            this.OutputBtnVerse3.TabIndex = 22;
            this.OutputBtnVerse3.Tag = "3";
            this.OutputBtnVerse3.Text = "3";
            this.OutputBtnVerse3.Click += new System.EventHandler(this.OutputBtnVerse_Click);
            // 
            // OutputBtnVerse4
            // 
            this.OutputBtnVerse4.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.OutputBtnVerse4.Location = new System.Drawing.Point(51, 0);
            this.OutputBtnVerse4.Margin = new System.Windows.Forms.Padding(0);
            this.OutputBtnVerse4.Name = "OutputBtnVerse4";
            this.OutputBtnVerse4.Size = new System.Drawing.Size(17, 25);
            this.OutputBtnVerse4.TabIndex = 23;
            this.OutputBtnVerse4.Tag = "4";
            this.OutputBtnVerse4.Text = "4";
            this.OutputBtnVerse4.Click += new System.EventHandler(this.OutputBtnVerse_Click);
            // 
            // OutputBtnVerse5
            // 
            this.OutputBtnVerse5.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.OutputBtnVerse5.Location = new System.Drawing.Point(68, 0);
            this.OutputBtnVerse5.Margin = new System.Windows.Forms.Padding(0);
            this.OutputBtnVerse5.Name = "OutputBtnVerse5";
            this.OutputBtnVerse5.Size = new System.Drawing.Size(17, 25);
            this.OutputBtnVerse5.TabIndex = 24;
            this.OutputBtnVerse5.Tag = "5";
            this.OutputBtnVerse5.Text = "5";
            this.OutputBtnVerse5.Click += new System.EventHandler(this.OutputBtnVerse_Click);
            // 
            // OutputBtnVerse6
            // 
            this.OutputBtnVerse6.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.OutputBtnVerse6.Location = new System.Drawing.Point(85, 0);
            this.OutputBtnVerse6.Margin = new System.Windows.Forms.Padding(0);
            this.OutputBtnVerse6.Name = "OutputBtnVerse6";
            this.OutputBtnVerse6.Size = new System.Drawing.Size(17, 25);
            this.OutputBtnVerse6.TabIndex = 25;
            this.OutputBtnVerse6.Tag = "6";
            this.OutputBtnVerse6.Text = "6";
            this.OutputBtnVerse6.Click += new System.EventHandler(this.OutputBtnVerse_Click);
            // 
            // OutputBtnVerse7
            // 
            this.OutputBtnVerse7.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.OutputBtnVerse7.Location = new System.Drawing.Point(102, 0);
            this.OutputBtnVerse7.Margin = new System.Windows.Forms.Padding(0);
            this.OutputBtnVerse7.Name = "OutputBtnVerse7";
            this.OutputBtnVerse7.Size = new System.Drawing.Size(17, 25);
            this.OutputBtnVerse7.TabIndex = 26;
            this.OutputBtnVerse7.Tag = "7";
            this.OutputBtnVerse7.Text = "7";
            this.OutputBtnVerse7.Click += new System.EventHandler(this.OutputBtnVerse_Click);
            // 
            // OutputBtnVerse8
            // 
            this.OutputBtnVerse8.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.OutputBtnVerse8.Location = new System.Drawing.Point(119, 0);
            this.OutputBtnVerse8.Margin = new System.Windows.Forms.Padding(0);
            this.OutputBtnVerse8.Name = "OutputBtnVerse8";
            this.OutputBtnVerse8.Size = new System.Drawing.Size(17, 25);
            this.OutputBtnVerse8.TabIndex = 27;
            this.OutputBtnVerse8.Tag = "8";
            this.OutputBtnVerse8.Text = "8";
            this.OutputBtnVerse8.Click += new System.EventHandler(this.OutputBtnVerse_Click);
            // 
            // OutputBtnVerse9
            // 
            this.OutputBtnVerse9.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.OutputBtnVerse9.Location = new System.Drawing.Point(136, 0);
            this.OutputBtnVerse9.Margin = new System.Windows.Forms.Padding(0);
            this.OutputBtnVerse9.Name = "OutputBtnVerse9";
            this.OutputBtnVerse9.Size = new System.Drawing.Size(17, 25);
            this.OutputBtnVerse9.TabIndex = 28;
            this.OutputBtnVerse9.Tag = "9";
            this.OutputBtnVerse9.Text = "9";
            this.OutputBtnVerse9.Click += new System.EventHandler(this.OutputBtnVerse_Click);
            // 
            // OutputBtnVersePreChorus
            // 
            this.OutputBtnVersePreChorus.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.OutputBtnVersePreChorus.Location = new System.Drawing.Point(153, 0);
            this.OutputBtnVersePreChorus.Margin = new System.Windows.Forms.Padding(0);
            this.OutputBtnVersePreChorus.Name = "OutputBtnVersePreChorus";
            this.OutputBtnVersePreChorus.Size = new System.Drawing.Size(17, 25);
            this.OutputBtnVersePreChorus.TabIndex = 33;
            this.OutputBtnVersePreChorus.Tag = "111";
            this.OutputBtnVersePreChorus.Text = "p";
            this.OutputBtnVersePreChorus.Click += new System.EventHandler(this.OutputBtnVerse_Click);
            // 
            // OutputBtnVersePreChorus2
            // 
            this.OutputBtnVersePreChorus2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.OutputBtnVersePreChorus2.Location = new System.Drawing.Point(170, 0);
            this.OutputBtnVersePreChorus2.Margin = new System.Windows.Forms.Padding(0);
            this.OutputBtnVersePreChorus2.Name = "OutputBtnVersePreChorus2";
            this.OutputBtnVersePreChorus2.Size = new System.Drawing.Size(17, 25);
            this.OutputBtnVersePreChorus2.TabIndex = 34;
            this.OutputBtnVersePreChorus2.Tag = "112";
            this.OutputBtnVersePreChorus2.Text = "q";
            this.OutputBtnVersePreChorus2.Click += new System.EventHandler(this.OutputBtnVerse_Click);
            // 
            // OutputBtnVerseChorus
            // 
            this.OutputBtnVerseChorus.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.OutputBtnVerseChorus.Location = new System.Drawing.Point(187, 0);
            this.OutputBtnVerseChorus.Margin = new System.Windows.Forms.Padding(0);
            this.OutputBtnVerseChorus.Name = "OutputBtnVerseChorus";
            this.OutputBtnVerseChorus.Size = new System.Drawing.Size(17, 25);
            this.OutputBtnVerseChorus.TabIndex = 29;
            this.OutputBtnVerseChorus.Tag = "0";
            this.OutputBtnVerseChorus.Text = "c";
            this.OutputBtnVerseChorus.Click += new System.EventHandler(this.OutputBtnVerse_Click);
            // 
            // OutputBtnVerseChorus2
            // 
            this.OutputBtnVerseChorus2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.OutputBtnVerseChorus2.Location = new System.Drawing.Point(204, 0);
            this.OutputBtnVerseChorus2.Margin = new System.Windows.Forms.Padding(0);
            this.OutputBtnVerseChorus2.Name = "OutputBtnVerseChorus2";
            this.OutputBtnVerseChorus2.Size = new System.Drawing.Size(17, 25);
            this.OutputBtnVerseChorus2.TabIndex = 31;
            this.OutputBtnVerseChorus2.Tag = "102";
            this.OutputBtnVerseChorus2.Text = "t";
            this.OutputBtnVerseChorus2.Click += new System.EventHandler(this.OutputBtnVerse_Click);
            // 
            // OutputBtnVerseBridge
            // 
            this.OutputBtnVerseBridge.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.OutputBtnVerseBridge.Location = new System.Drawing.Point(221, 0);
            this.OutputBtnVerseBridge.Margin = new System.Windows.Forms.Padding(0);
            this.OutputBtnVerseBridge.Name = "OutputBtnVerseBridge";
            this.OutputBtnVerseBridge.Size = new System.Drawing.Size(17, 25);
            this.OutputBtnVerseBridge.TabIndex = 30;
            this.OutputBtnVerseBridge.Tag = "100";
            this.OutputBtnVerseBridge.Text = "b";
            this.OutputBtnVerseBridge.Click += new System.EventHandler(this.OutputBtnVerse_Click);
            // 
            // OutputBtnVerseBridge2
            // 
            this.OutputBtnVerseBridge2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.OutputBtnVerseBridge2.Location = new System.Drawing.Point(238, 0);
            this.OutputBtnVerseBridge2.Margin = new System.Windows.Forms.Padding(0);
            this.OutputBtnVerseBridge2.Name = "OutputBtnVerseBridge2";
            this.OutputBtnVerseBridge2.Size = new System.Drawing.Size(20, 25);
            this.OutputBtnVerseBridge2.TabIndex = 35;
            this.OutputBtnVerseBridge2.Tag = "103";
            this.OutputBtnVerseBridge2.Text = "w";
            this.OutputBtnVerseBridge2.Click += new System.EventHandler(this.OutputBtnVerse_Click);
            // 
            // OutputBtnVerseEnding
            // 
            this.OutputBtnVerseEnding.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.OutputBtnVerseEnding.Location = new System.Drawing.Point(258, 0);
            this.OutputBtnVerseEnding.Margin = new System.Windows.Forms.Padding(0);
            this.OutputBtnVerseEnding.Name = "OutputBtnVerseEnding";
            this.OutputBtnVerseEnding.Size = new System.Drawing.Size(17, 25);
            this.OutputBtnVerseEnding.TabIndex = 32;
            this.OutputBtnVerseEnding.Tag = "101";
            this.OutputBtnVerseEnding.Text = "e";
            this.OutputBtnVerseEnding.Click += new System.EventHandler(this.OutputBtnVerse_Click);
            // 
            // panel6
            // 
            this.panel6.Controls.Add(this.OutputBtnSlideDown);
            this.panel6.Controls.Add(this.OutputBtnSlideUp);
            this.panel6.Controls.Add(this.OutputBtnItemDown);
            this.panel6.Controls.Add(this.OutputBtnItemUp);
            this.panel6.Controls.Add(this.OutputBtnRefAlert);
            this.panel6.Controls.Add(this.OutputBtnMedia);
            this.panel6.Controls.Add(this.OutputBtnJumpToNonRotate);
            this.panel6.Dock = System.Windows.Forms.DockStyle.Left;
            this.panel6.Location = new System.Drawing.Point(0, 0);
            this.panel6.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.panel6.Name = "panel6";
            this.panel6.Size = new System.Drawing.Size(189, 25);
            this.panel6.TabIndex = 1;
            // 
            // OutputBtnSlideDown
            // 
            this.OutputBtnSlideDown.Dock = System.Windows.Forms.DockStyle.Left;
            this.OutputBtnSlideDown.Image = ((System.Drawing.Image)(resources.GetObject("OutputBtnSlideDown.Image")));
            this.OutputBtnSlideDown.Location = new System.Drawing.Point(159, 0);
            this.OutputBtnSlideDown.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.OutputBtnSlideDown.Name = "OutputBtnSlideDown";
            this.OutputBtnSlideDown.Size = new System.Drawing.Size(26, 25);
            this.OutputBtnSlideDown.TabIndex = 5;
            this.toolTip1.SetToolTip(this.OutputBtnSlideDown, "Next Slide");
            this.OutputBtnSlideDown.Click += new System.EventHandler(this.OutputBtnUpDown_Click);
            // 
            // OutputBtnSlideUp
            // 
            this.OutputBtnSlideUp.Dock = System.Windows.Forms.DockStyle.Left;
            this.OutputBtnSlideUp.Image = ((System.Drawing.Image)(resources.GetObject("OutputBtnSlideUp.Image")));
            this.OutputBtnSlideUp.Location = new System.Drawing.Point(133, 0);
            this.OutputBtnSlideUp.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.OutputBtnSlideUp.Name = "OutputBtnSlideUp";
            this.OutputBtnSlideUp.Size = new System.Drawing.Size(26, 25);
            this.OutputBtnSlideUp.TabIndex = 4;
            this.toolTip1.SetToolTip(this.OutputBtnSlideUp, "Previous Slide");
            this.OutputBtnSlideUp.Click += new System.EventHandler(this.OutputBtnUpDown_Click);
            // 
            // OutputBtnItemDown
            // 
            this.OutputBtnItemDown.Dock = System.Windows.Forms.DockStyle.Left;
            this.OutputBtnItemDown.Image = ((System.Drawing.Image)(resources.GetObject("OutputBtnItemDown.Image")));
            this.OutputBtnItemDown.Location = new System.Drawing.Point(107, 0);
            this.OutputBtnItemDown.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.OutputBtnItemDown.Name = "OutputBtnItemDown";
            this.OutputBtnItemDown.Size = new System.Drawing.Size(26, 25);
            this.OutputBtnItemDown.TabIndex = 3;
            this.toolTip1.SetToolTip(this.OutputBtnItemDown, "Next Item");
            this.OutputBtnItemDown.Click += new System.EventHandler(this.OutputBtnUpDown_Click);
            // 
            // OutputBtnItemUp
            // 
            this.OutputBtnItemUp.Dock = System.Windows.Forms.DockStyle.Left;
            this.OutputBtnItemUp.Image = ((System.Drawing.Image)(resources.GetObject("OutputBtnItemUp.Image")));
            this.OutputBtnItemUp.Location = new System.Drawing.Point(81, 0);
            this.OutputBtnItemUp.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.OutputBtnItemUp.Name = "OutputBtnItemUp";
            this.OutputBtnItemUp.Size = new System.Drawing.Size(26, 25);
            this.OutputBtnItemUp.TabIndex = 2;
            this.toolTip1.SetToolTip(this.OutputBtnItemUp, "Previous Item");
            this.OutputBtnItemUp.Click += new System.EventHandler(this.OutputBtnUpDown_Click);
            // 
            // OutputBtnRefAlert
            // 
            this.OutputBtnRefAlert.Dock = System.Windows.Forms.DockStyle.Left;
            this.OutputBtnRefAlert.Image = ((System.Drawing.Image)(resources.GetObject("OutputBtnRefAlert.Image")));
            this.OutputBtnRefAlert.Location = new System.Drawing.Point(54, 0);
            this.OutputBtnRefAlert.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.OutputBtnRefAlert.Name = "OutputBtnRefAlert";
            this.OutputBtnRefAlert.Size = new System.Drawing.Size(27, 25);
            this.OutputBtnRefAlert.TabIndex = 6;
            this.toolTip1.SetToolTip(this.OutputBtnRefAlert, "Show/Stop Reference Alert");
            this.OutputBtnRefAlert.Click += new System.EventHandler(this.OutputBtnRefAlert_Click);
            // 
            // OutputBtnMedia
            // 
            this.OutputBtnMedia.Dock = System.Windows.Forms.DockStyle.Left;
            this.OutputBtnMedia.Image = ((System.Drawing.Image)(resources.GetObject("OutputBtnMedia.Image")));
            this.OutputBtnMedia.Location = new System.Drawing.Point(27, 0);
            this.OutputBtnMedia.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.OutputBtnMedia.Name = "OutputBtnMedia";
            this.OutputBtnMedia.Size = new System.Drawing.Size(27, 25);
            this.OutputBtnMedia.TabIndex = 7;
            this.toolTip1.SetToolTip(this.OutputBtnMedia, "Media Pause/Resume");
            this.OutputBtnMedia.Click += new System.EventHandler(this.OutputBtnMedia_Click);
            // 
            // OutputBtnJumpToNonRotate
            // 
            this.OutputBtnJumpToNonRotate.Dock = System.Windows.Forms.DockStyle.Left;
            this.OutputBtnJumpToNonRotate.Image = ((System.Drawing.Image)(resources.GetObject("OutputBtnJumpToNonRotate.Image")));
            this.OutputBtnJumpToNonRotate.Location = new System.Drawing.Point(0, 0);
            this.OutputBtnJumpToNonRotate.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.OutputBtnJumpToNonRotate.Name = "OutputBtnJumpToNonRotate";
            this.OutputBtnJumpToNonRotate.Size = new System.Drawing.Size(27, 25);
            this.OutputBtnJumpToNonRotate.TabIndex = 8;
            this.toolTip1.SetToolTip(this.OutputBtnJumpToNonRotate, "Jump To Non-Rotating Item");
            this.OutputBtnJumpToNonRotate.Click += new System.EventHandler(this.OutputBtnJumpToNonRotate_Click);
            // 
            // toolStripMain
            // 
            this.toolStripMain.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStripMain.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStripMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.Main_New,
            this.Main_Edit,
            this.Main_Copy,
            this.Main_Move,
            this.Main_Delete,
            this.toolStripSeparator1,
            this.Main_Media,
            this.Main_Refresh,
            this.toolStripSeparator2,
            this.Main_Options,
            this.toolStripSeparator3,
            this.Main_NoRotate,
            this.Main_RotateStyle,
            this.Main_Alerts,
            this.Main_Chinese,
            this.toolStripSeparator4,
            this.Main_Find,
            this.Main_QuickFind,
            this.Main_JumpA,
            this.Main_JumpB,
            this.Main_JumpC});
            this.toolStripMain.Location = new System.Drawing.Point(3, 0);
            this.toolStripMain.Name = "toolStripMain";
            this.toolStripMain.Size = new System.Drawing.Size(533, 25);
            this.toolStripMain.TabIndex = 0;
            this.toolStripMain.Text = "toolStrip1";
            // 
            // Main_New
            // 
            this.Main_New.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.Main_New.Image = ((System.Drawing.Image)(resources.GetObject("Main_New.Image")));
            this.Main_New.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.Main_New.Name = "Main_New";
            this.Main_New.Size = new System.Drawing.Size(23, 22);
            this.Main_New.Tag = "";
            this.Main_New.ToolTipText = "New";
            this.Main_New.Click += new System.EventHandler(this.Main_EditBtns_Click);
            // 
            // Main_Edit
            // 
            this.Main_Edit.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.Main_Edit.Image = ((System.Drawing.Image)(resources.GetObject("Main_Edit.Image")));
            this.Main_Edit.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.Main_Edit.Name = "Main_Edit";
            this.Main_Edit.Size = new System.Drawing.Size(23, 22);
            this.Main_Edit.Tag = "";
            this.Main_Edit.ToolTipText = "Edit";
            this.Main_Edit.Click += new System.EventHandler(this.Main_EditBtns_Click);
            // 
            // Main_Copy
            // 
            this.Main_Copy.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.Main_Copy.Image = ((System.Drawing.Image)(resources.GetObject("Main_Copy.Image")));
            this.Main_Copy.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.Main_Copy.Name = "Main_Copy";
            this.Main_Copy.Size = new System.Drawing.Size(23, 22);
            this.Main_Copy.Tag = "";
            this.Main_Copy.ToolTipText = "Copy";
            this.Main_Copy.Click += new System.EventHandler(this.Main_EditBtns_Click);
            // 
            // Main_Move
            // 
            this.Main_Move.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.Main_Move.Image = ((System.Drawing.Image)(resources.GetObject("Main_Move.Image")));
            this.Main_Move.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.Main_Move.Name = "Main_Move";
            this.Main_Move.Size = new System.Drawing.Size(23, 22);
            this.Main_Move.Tag = "";
            this.Main_Move.ToolTipText = "Move";
            this.Main_Move.Click += new System.EventHandler(this.Main_EditBtns_Click);
            // 
            // Main_Delete
            // 
            this.Main_Delete.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.Main_Delete.Image = ((System.Drawing.Image)(resources.GetObject("Main_Delete.Image")));
            this.Main_Delete.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.Main_Delete.Name = "Main_Delete";
            this.Main_Delete.Size = new System.Drawing.Size(23, 22);
            this.Main_Delete.Tag = "";
            this.Main_Delete.ToolTipText = "Delete";
            this.Main_Delete.Click += new System.EventHandler(this.Main_EditBtns_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // Main_Media
            // 
            this.Main_Media.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.Main_Media.Image = ((System.Drawing.Image)(resources.GetObject("Main_Media.Image")));
            this.Main_Media.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.Main_Media.Name = "Main_Media";
            this.Main_Media.Size = new System.Drawing.Size(23, 22);
            this.Main_Media.Tag = "";
            this.Main_Media.ToolTipText = "Play Media";
            this.Main_Media.Click += new System.EventHandler(this.Main_Media_Click);
            // 
            // Main_Refresh
            // 
            this.Main_Refresh.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.Main_Refresh.Image = ((System.Drawing.Image)(resources.GetObject("Main_Refresh.Image")));
            this.Main_Refresh.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.Main_Refresh.Name = "Main_Refresh";
            this.Main_Refresh.Size = new System.Drawing.Size(23, 22);
            this.Main_Refresh.Tag = "";
            this.Main_Refresh.ToolTipText = "Refresh";
            this.Main_Refresh.Click += new System.EventHandler(this.Main_Refresh_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
            // 
            // Main_Options
            // 
            this.Main_Options.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.Main_Options.Image = ((System.Drawing.Image)(resources.GetObject("Main_Options.Image")));
            this.Main_Options.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.Main_Options.Name = "Main_Options";
            this.Main_Options.Size = new System.Drawing.Size(23, 22);
            this.Main_Options.Tag = "";
            this.Main_Options.ToolTipText = "Options";
            this.Main_Options.Click += new System.EventHandler(this.Main_Options_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(6, 25);
            // 
            // Main_NoRotate
            // 
            this.Main_NoRotate.CheckOnClick = true;
            this.Main_NoRotate.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.Main_NoRotate.Image = ((System.Drawing.Image)(resources.GetObject("Main_NoRotate.Image")));
            this.Main_NoRotate.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.Main_NoRotate.Name = "Main_NoRotate";
            this.Main_NoRotate.Size = new System.Drawing.Size(23, 22);
            this.Main_NoRotate.Tag = "";
            this.Main_NoRotate.ToolTipText = "Stop Auto Rotate ";
            this.Main_NoRotate.Click += new System.EventHandler(this.Main_NoRotate_Click);
            // 
            // Main_RotateStyle
            // 
            this.Main_RotateStyle.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.Main_RotateStyle.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.Main_Rotate0,
            this.Main_Rotate1,
            this.Main_Rotate2,
            this.Main_Rotate3});
            this.Main_RotateStyle.Image = ((System.Drawing.Image)(resources.GetObject("Main_RotateStyle.Image")));
            this.Main_RotateStyle.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.Main_RotateStyle.Name = "Main_RotateStyle";
            this.Main_RotateStyle.Size = new System.Drawing.Size(29, 22);
            this.Main_RotateStyle.ToolTipText = "Rotate Style";
            this.Main_RotateStyle.DropDownItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.Main_RotateStyle_DropDownItemClicked);
            // 
            // Main_Rotate0
            // 
            this.Main_Rotate0.Image = ((System.Drawing.Image)(resources.GetObject("Main_Rotate0.Image")));
            this.Main_Rotate0.Name = "Main_Rotate0";
            this.Main_Rotate0.Size = new System.Drawing.Size(236, 22);
            this.Main_Rotate0.Tag = "0";
            this.Main_Rotate0.Text = "Auto Rotate One Item ";
            // 
            // Main_Rotate1
            // 
            this.Main_Rotate1.Image = ((System.Drawing.Image)(resources.GetObject("Main_Rotate1.Image")));
            this.Main_Rotate1.Name = "Main_Rotate1";
            this.Main_Rotate1.Size = new System.Drawing.Size(236, 22);
            this.Main_Rotate1.Tag = "1";
            this.Main_Rotate1.Text = "Auto Rotate One Item - Repeat";
            // 
            // Main_Rotate2
            // 
            this.Main_Rotate2.Image = ((System.Drawing.Image)(resources.GetObject("Main_Rotate2.Image")));
            this.Main_Rotate2.Name = "Main_Rotate2";
            this.Main_Rotate2.Size = new System.Drawing.Size(236, 22);
            this.Main_Rotate2.Tag = "2";
            this.Main_Rotate2.Text = "Auto Rotate Group";
            // 
            // Main_Rotate3
            // 
            this.Main_Rotate3.Image = ((System.Drawing.Image)(resources.GetObject("Main_Rotate3.Image")));
            this.Main_Rotate3.Name = "Main_Rotate3";
            this.Main_Rotate3.Size = new System.Drawing.Size(236, 22);
            this.Main_Rotate3.Tag = "3";
            this.Main_Rotate3.Text = "Auto Rotate Group - Repeat";
            // 
            // Main_Alerts
            // 
            this.Main_Alerts.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.Main_Alerts.Image = ((System.Drawing.Image)(resources.GetObject("Main_Alerts.Image")));
            this.Main_Alerts.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.Main_Alerts.Name = "Main_Alerts";
            this.Main_Alerts.Size = new System.Drawing.Size(23, 22);
            this.Main_Alerts.Tag = "";
            this.Main_Alerts.ToolTipText = "Alerts";
            this.Main_Alerts.Click += new System.EventHandler(this.Main_Alerts_Click);
            // 
            // Main_Chinese
            // 
            this.Main_Chinese.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.Main_Chinese.Image = ((System.Drawing.Image)(resources.GetObject("Main_Chinese.Image")));
            this.Main_Chinese.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.Main_Chinese.Name = "Main_Chinese";
            this.Main_Chinese.Size = new System.Drawing.Size(23, 22);
            this.Main_Chinese.Tag = "";
            this.Main_Chinese.ToolTipText = "Trad/Simp Chinese";
            this.Main_Chinese.Click += new System.EventHandler(this.Main_Chinese_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(6, 25);
            // 
            // Main_Find
            // 
            this.Main_Find.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.Main_Find.Image = ((System.Drawing.Image)(resources.GetObject("Main_Find.Image")));
            this.Main_Find.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.Main_Find.Name = "Main_Find";
            this.Main_Find.Size = new System.Drawing.Size(23, 22);
            this.Main_Find.Tag = "";
            this.Main_Find.ToolTipText = "Find";
            this.Main_Find.Click += new System.EventHandler(this.Main_Find_Click);
            // 
            // Main_QuickFind
            // 
            this.Main_QuickFind.MaxDropDownItems = 12;
            this.Main_QuickFind.Name = "Main_QuickFind";
            this.Main_QuickFind.Size = new System.Drawing.Size(130, 25);
            this.Main_QuickFind.Tag = "";
            this.Main_QuickFind.Text = "Search Phrase";
            this.Main_QuickFind.ToolTipText = "Enter phrase and  press Keyboard Enter key";
            this.Main_QuickFind.Enter += new System.EventHandler(this.Main_QuickFind_Enter);
            this.Main_QuickFind.Leave += new System.EventHandler(this.Main_QuickFind_Leave);
            this.Main_QuickFind.KeyUp += new System.Windows.Forms.KeyEventHandler(this.Main_QuickFind_KeyUp);
            // 
            // Main_JumpA
            // 
            this.Main_JumpA.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.Main_JumpA.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.Main_JumpA.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.Main_JumpA.Name = "Main_JumpA";
            this.Main_JumpA.Overflow = System.Windows.Forms.ToolStripItemOverflow.Never;
            this.Main_JumpA.Size = new System.Drawing.Size(23, 22);
            this.Main_JumpA.Text = "A";
            this.Main_JumpA.Click += new System.EventHandler(this.Main_Jump_Click);
            // 
            // Main_JumpB
            // 
            this.Main_JumpB.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.Main_JumpB.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.Main_JumpB.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.Main_JumpB.Name = "Main_JumpB";
            this.Main_JumpB.Overflow = System.Windows.Forms.ToolStripItemOverflow.Never;
            this.Main_JumpB.Size = new System.Drawing.Size(23, 22);
            this.Main_JumpB.Text = "B";
            this.Main_JumpB.Click += new System.EventHandler(this.Main_Jump_Click);
            // 
            // Main_JumpC
            // 
            this.Main_JumpC.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.Main_JumpC.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.Main_JumpC.Name = "Main_JumpC";
            this.Main_JumpC.Overflow = System.Windows.Forms.ToolStripItemOverflow.Never;
            this.Main_JumpC.Size = new System.Drawing.Size(23, 22);
            this.Main_JumpC.Text = "C";
            this.Main_JumpC.Click += new System.EventHandler(this.Main_Jump_Click);
            // 
            // menuStripMain
            // 
            this.menuStripMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.Menu_MainFile,
            this.Menu_MainEdit,
            this.Menu_MainView,
            this.Menu_MainOutput,
            this.Menu_MainTools,
            this.Menu_MainHelp});
            this.menuStripMain.Location = new System.Drawing.Point(0, 0);
            this.menuStripMain.Name = "menuStripMain";
            this.menuStripMain.Padding = new System.Windows.Forms.Padding(7, 2, 0, 2);
            this.menuStripMain.Size = new System.Drawing.Size(857, 24);
            this.menuStripMain.TabIndex = 0;
            this.menuStripMain.Text = "menuStrip1";
            // 
            // Menu_MainFile
            // 
            this.Menu_MainFile.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.Menu_WorshipSessions,
            this.Menu_PraiseBookTemplates,
            this.toolStripSeparator20,
            this.Menu_ListingOfSelectedFolder,
            this.toolStripSeparator16,
            this.Menu_EditHistoryList,
            this.toolStripSeparator18,
            this.Menu_Exit});
            this.Menu_MainFile.Name = "Menu_MainFile";
            this.Menu_MainFile.Size = new System.Drawing.Size(37, 20);
            this.Menu_MainFile.Text = "&File";
            // 
            // Menu_WorshipSessions
            // 
            this.Menu_WorshipSessions.Image = ((System.Drawing.Image)(resources.GetObject("Menu_WorshipSessions.Image")));
            this.Menu_WorshipSessions.Name = "Menu_WorshipSessions";
            this.Menu_WorshipSessions.Size = new System.Drawing.Size(206, 22);
            this.Menu_WorshipSessions.Text = "Worship Sessions...";
            this.Menu_WorshipSessions.Click += new System.EventHandler(this.Menu_WorshipLists_Click);
            // 
            // Menu_PraiseBookTemplates
            // 
            this.Menu_PraiseBookTemplates.Image = ((System.Drawing.Image)(resources.GetObject("Menu_PraiseBookTemplates.Image")));
            this.Menu_PraiseBookTemplates.Name = "Menu_PraiseBookTemplates";
            this.Menu_PraiseBookTemplates.Size = new System.Drawing.Size(206, 22);
            this.Menu_PraiseBookTemplates.Text = "PraiseBooks...";
            this.Menu_PraiseBookTemplates.Click += new System.EventHandler(this.Menu_PraiseBooks_Click);
            // 
            // toolStripSeparator20
            // 
            this.toolStripSeparator20.Name = "toolStripSeparator20";
            this.toolStripSeparator20.Size = new System.Drawing.Size(203, 6);
            // 
            // Menu_ListingOfSelectedFolder
            // 
            this.Menu_ListingOfSelectedFolder.Name = "Menu_ListingOfSelectedFolder";
            this.Menu_ListingOfSelectedFolder.Size = new System.Drawing.Size(206, 22);
            this.Menu_ListingOfSelectedFolder.Text = "Listing of Selected Folder";
            this.Menu_ListingOfSelectedFolder.Click += new System.EventHandler(this.Menu_ListingOfSelectedFolder_Click);
            // 
            // toolStripSeparator16
            // 
            this.toolStripSeparator16.Name = "toolStripSeparator16";
            this.toolStripSeparator16.Size = new System.Drawing.Size(203, 6);
            // 
            // Menu_EditHistoryList
            // 
            this.Menu_EditHistoryList.Name = "Menu_EditHistoryList";
            this.Menu_EditHistoryList.Size = new System.Drawing.Size(206, 22);
            this.Menu_EditHistoryList.Text = "Recent Edits";
            // 
            // toolStripSeparator18
            // 
            this.toolStripSeparator18.Name = "toolStripSeparator18";
            this.toolStripSeparator18.Size = new System.Drawing.Size(203, 6);
            // 
            // Menu_Exit
            // 
            this.Menu_Exit.Name = "Menu_Exit";
            this.Menu_Exit.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.F4)));
            this.Menu_Exit.Size = new System.Drawing.Size(206, 22);
            this.Menu_Exit.Text = "Exit";
            this.Menu_Exit.Click += new System.EventHandler(this.Menu_Exit_Click);
            // 
            // Menu_MainEdit
            // 
            this.Menu_MainEdit.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.Menu_AddSong,
            this.toolStripSeparator19,
            this.Menu_EditSong,
            this.Menu_CopySong,
            this.Menu_MoveSong,
            this.Menu_DeleteSong,
            this.toolStripSeparator41,
            this.Menu_SelectAll,
            this.Menu_Find,
            this.toolStripSeparator21,
            this.Menu_UseSongNumbering,
            this.Menu_ReArrangeSongFolders});
            this.Menu_MainEdit.Name = "Menu_MainEdit";
            this.Menu_MainEdit.Size = new System.Drawing.Size(39, 20);
            this.Menu_MainEdit.Text = "&Edit";
            // 
            // Menu_AddSong
            // 
            this.Menu_AddSong.Image = ((System.Drawing.Image)(resources.GetObject("Menu_AddSong.Image")));
            this.Menu_AddSong.Name = "Menu_AddSong";
            this.Menu_AddSong.Size = new System.Drawing.Size(205, 22);
            this.Menu_AddSong.Text = "Add New Song...";
            this.Menu_AddSong.Click += new System.EventHandler(this.Menu_AddSong_Click);
            // 
            // toolStripSeparator19
            // 
            this.toolStripSeparator19.Name = "toolStripSeparator19";
            this.toolStripSeparator19.Size = new System.Drawing.Size(202, 6);
            // 
            // Menu_EditSong
            // 
            this.Menu_EditSong.Image = ((System.Drawing.Image)(resources.GetObject("Menu_EditSong.Image")));
            this.Menu_EditSong.Name = "Menu_EditSong";
            this.Menu_EditSong.Size = new System.Drawing.Size(205, 22);
            this.Menu_EditSong.Text = "Edit";
            this.Menu_EditSong.Click += new System.EventHandler(this.Menu_EditSong_Click);
            // 
            // Menu_CopySong
            // 
            this.Menu_CopySong.Image = ((System.Drawing.Image)(resources.GetObject("Menu_CopySong.Image")));
            this.Menu_CopySong.Name = "Menu_CopySong";
            this.Menu_CopySong.Size = new System.Drawing.Size(205, 22);
            this.Menu_CopySong.Text = "Copy";
            this.Menu_CopySong.Click += new System.EventHandler(this.Menu_CopySong_Click);
            // 
            // Menu_MoveSong
            // 
            this.Menu_MoveSong.Image = ((System.Drawing.Image)(resources.GetObject("Menu_MoveSong.Image")));
            this.Menu_MoveSong.Name = "Menu_MoveSong";
            this.Menu_MoveSong.Size = new System.Drawing.Size(205, 22);
            this.Menu_MoveSong.Text = "Move";
            this.Menu_MoveSong.Click += new System.EventHandler(this.Menu_MoveSong_Click);
            // 
            // Menu_DeleteSong
            // 
            this.Menu_DeleteSong.Image = ((System.Drawing.Image)(resources.GetObject("Menu_DeleteSong.Image")));
            this.Menu_DeleteSong.Name = "Menu_DeleteSong";
            this.Menu_DeleteSong.Size = new System.Drawing.Size(205, 22);
            this.Menu_DeleteSong.Text = "Delete...";
            this.Menu_DeleteSong.Click += new System.EventHandler(this.Menu_DeleteSong_Click);
            // 
            // toolStripSeparator41
            // 
            this.toolStripSeparator41.Name = "toolStripSeparator41";
            this.toolStripSeparator41.Size = new System.Drawing.Size(202, 6);
            // 
            // Menu_SelectAll
            // 
            this.Menu_SelectAll.Name = "Menu_SelectAll";
            this.Menu_SelectAll.Size = new System.Drawing.Size(205, 22);
            this.Menu_SelectAll.Text = "Select All";
            this.Menu_SelectAll.Click += new System.EventHandler(this.Menu_SelectAll_Click);
            // 
            // Menu_Find
            // 
            this.Menu_Find.Image = ((System.Drawing.Image)(resources.GetObject("Menu_Find.Image")));
            this.Menu_Find.Name = "Menu_Find";
            this.Menu_Find.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.F)));
            this.Menu_Find.Size = new System.Drawing.Size(205, 22);
            this.Menu_Find.Text = "Find";
            this.Menu_Find.Click += new System.EventHandler(this.Menu_Find_Click);
            // 
            // toolStripSeparator21
            // 
            this.toolStripSeparator21.Name = "toolStripSeparator21";
            this.toolStripSeparator21.Size = new System.Drawing.Size(202, 6);
            // 
            // Menu_UseSongNumbering
            // 
            this.Menu_UseSongNumbering.CheckOnClick = true;
            this.Menu_UseSongNumbering.Name = "Menu_UseSongNumbering";
            this.Menu_UseSongNumbering.Size = new System.Drawing.Size(205, 22);
            this.Menu_UseSongNumbering.Text = "Use Song Numbering";
            this.Menu_UseSongNumbering.Click += new System.EventHandler(this.Menu_useSongNumbering_Click);
            // 
            // Menu_ReArrangeSongFolders
            // 
            this.Menu_ReArrangeSongFolders.Name = "Menu_ReArrangeSongFolders";
            this.Menu_ReArrangeSongFolders.Size = new System.Drawing.Size(205, 22);
            this.Menu_ReArrangeSongFolders.Text = "Re-Arrange Song Folders";
            this.Menu_ReArrangeSongFolders.Click += new System.EventHandler(this.Menu_ReArrangeSongFolders_Click);
            // 
            // Menu_MainView
            // 
            this.Menu_MainView.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.Menu_EasiSlidesFolder,
            this.Menu_Options,
            this.toolStripSeparator23,
            this.Menu_Refresh,
            this.Menu_PreviewNotations,
            this.Menu_StatusBar});
            this.Menu_MainView.Name = "Menu_MainView";
            this.Menu_MainView.Size = new System.Drawing.Size(44, 20);
            this.Menu_MainView.Text = "&View";
            // 
            // Menu_EasiSlidesFolder
            // 
            this.Menu_EasiSlidesFolder.Image = ((System.Drawing.Image)(resources.GetObject("Menu_EasiSlidesFolder.Image")));
            this.Menu_EasiSlidesFolder.Name = "Menu_EasiSlidesFolder";
            this.Menu_EasiSlidesFolder.Size = new System.Drawing.Size(215, 22);
            this.Menu_EasiSlidesFolder.Text = "EasiSlides Folder";
            this.Menu_EasiSlidesFolder.Click += new System.EventHandler(this.Menu_EasiSlidesFolder_Click);
            // 
            // Menu_Options
            // 
            this.Menu_Options.Image = ((System.Drawing.Image)(resources.GetObject("Menu_Options.Image")));
            this.Menu_Options.Name = "Menu_Options";
            this.Menu_Options.Size = new System.Drawing.Size(215, 22);
            this.Menu_Options.Text = "Options";
            this.Menu_Options.Click += new System.EventHandler(this.Menu_Options_Click);
            // 
            // toolStripSeparator23
            // 
            this.toolStripSeparator23.Name = "toolStripSeparator23";
            this.toolStripSeparator23.Size = new System.Drawing.Size(212, 6);
            // 
            // Menu_Refresh
            // 
            this.Menu_Refresh.Image = ((System.Drawing.Image)(resources.GetObject("Menu_Refresh.Image")));
            this.Menu_Refresh.Name = "Menu_Refresh";
            this.Menu_Refresh.Size = new System.Drawing.Size(215, 22);
            this.Menu_Refresh.Text = "Refresh";
            this.Menu_Refresh.Click += new System.EventHandler(this.Menu_Refresh_Click);
            // 
            // Menu_PreviewNotations
            // 
            this.Menu_PreviewNotations.CheckOnClick = true;
            this.Menu_PreviewNotations.Name = "Menu_PreviewNotations";
            this.Menu_PreviewNotations.Size = new System.Drawing.Size(215, 22);
            this.Menu_PreviewNotations.Text = "Show Notations in Preview";
            this.Menu_PreviewNotations.Click += new System.EventHandler(this.Menu_PreviewNotations_Click);
            // 
            // Menu_StatusBar
            // 
            this.Menu_StatusBar.Checked = true;
            this.Menu_StatusBar.CheckState = System.Windows.Forms.CheckState.Checked;
            this.Menu_StatusBar.Name = "Menu_StatusBar";
            this.Menu_StatusBar.Size = new System.Drawing.Size(215, 22);
            this.Menu_StatusBar.Text = "Status Bar";
            this.Menu_StatusBar.Click += new System.EventHandler(this.Menu_StatusBar_Click);
            // 
            // Menu_MainOutput
            // 
            this.Menu_MainOutput.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.Menu_StartShow,
            this.Menu_GoLiveWithPreview,
            this.Menu_RefreshOutput,
            this.toolStripSeparator28,
            this.Menu_BlackScreen,
            this.Menu_ClearScreen,
            this.Menu_LiveCam,
            this.Menu_RestartCurrentItem,
            this.toolStripSeparator29,
            this.Menu_AlertWindow,
            this.Menu_StopAlert,
            this.toolStripSeparator30,
            this.Menu_SwitchChinese});
            this.Menu_MainOutput.Name = "Menu_MainOutput";
            this.Menu_MainOutput.Size = new System.Drawing.Size(57, 20);
            this.Menu_MainOutput.Text = "&Output";
            // 
            // Menu_StartShow
            // 
            this.Menu_StartShow.Name = "Menu_StartShow";
            this.Menu_StartShow.ShortcutKeys = System.Windows.Forms.Keys.F12;
            this.Menu_StartShow.Size = new System.Drawing.Size(249, 22);
            this.Menu_StartShow.Text = "Start Show - Go LIVE";
            this.Menu_StartShow.Click += new System.EventHandler(this.Menu_StartShow_Click);
            // 
            // Menu_GoLiveWithPreview
            // 
            this.Menu_GoLiveWithPreview.Name = "Menu_GoLiveWithPreview";
            this.Menu_GoLiveWithPreview.ShortcutKeys = System.Windows.Forms.Keys.F11;
            this.Menu_GoLiveWithPreview.Size = new System.Drawing.Size(249, 22);
            this.Menu_GoLiveWithPreview.Text = "Preview: Go Live, Move Next";
            this.Menu_GoLiveWithPreview.Click += new System.EventHandler(this.Menu_PreviewGoLiveNext_Click);
            // 
            // Menu_RefreshOutput
            // 
            this.Menu_RefreshOutput.Name = "Menu_RefreshOutput";
            this.Menu_RefreshOutput.Size = new System.Drawing.Size(249, 22);
            this.Menu_RefreshOutput.Text = "Refresh Output";
            this.Menu_RefreshOutput.Click += new System.EventHandler(this.Menu_RefreshOutput_Click);
            // 
            // toolStripSeparator28
            // 
            this.toolStripSeparator28.Name = "toolStripSeparator28";
            this.toolStripSeparator28.Size = new System.Drawing.Size(246, 6);
            // 
            // Menu_BlackScreen
            // 
            this.Menu_BlackScreen.Image = ((System.Drawing.Image)(resources.GetObject("Menu_BlackScreen.Image")));
            this.Menu_BlackScreen.Name = "Menu_BlackScreen";
            this.Menu_BlackScreen.ShortcutKeys = System.Windows.Forms.Keys.F2;
            this.Menu_BlackScreen.Size = new System.Drawing.Size(249, 22);
            this.Menu_BlackScreen.Text = "Black Screen";
            this.Menu_BlackScreen.Click += new System.EventHandler(this.Menu_BlackScreen_Click);
            // 
            // Menu_ClearScreen
            // 
            this.Menu_ClearScreen.Image = ((System.Drawing.Image)(resources.GetObject("Menu_ClearScreen.Image")));
            this.Menu_ClearScreen.Name = "Menu_ClearScreen";
            this.Menu_ClearScreen.ShortcutKeys = System.Windows.Forms.Keys.F3;
            this.Menu_ClearScreen.Size = new System.Drawing.Size(249, 22);
            this.Menu_ClearScreen.Text = "Clear Screen";
            this.Menu_ClearScreen.Click += new System.EventHandler(this.Menu_ClearScreen_Click);
            // 
            // Menu_LiveCam
            // 
            this.Menu_LiveCam.Image = ((System.Drawing.Image)(resources.GetObject("Menu_LiveCam.Image")));
            this.Menu_LiveCam.Name = "Menu_LiveCam";
            this.Menu_LiveCam.ShortcutKeys = System.Windows.Forms.Keys.F4;
            this.Menu_LiveCam.Size = new System.Drawing.Size(249, 22);
            this.Menu_LiveCam.Text = "Live Cam";
            this.Menu_LiveCam.Click += new System.EventHandler(this.Menu_LiveCam_Click);
            // 
            // Menu_RestartCurrentItem
            // 
            this.Menu_RestartCurrentItem.Name = "Menu_RestartCurrentItem";
            this.Menu_RestartCurrentItem.ShortcutKeys = System.Windows.Forms.Keys.F5;
            this.Menu_RestartCurrentItem.Size = new System.Drawing.Size(249, 22);
            this.Menu_RestartCurrentItem.Text = "Restart Current Item";
            this.Menu_RestartCurrentItem.Click += new System.EventHandler(this.Menu_RestartCurrentItem_Click);
            // 
            // toolStripSeparator29
            // 
            this.toolStripSeparator29.Name = "toolStripSeparator29";
            this.toolStripSeparator29.Size = new System.Drawing.Size(246, 6);
            // 
            // Menu_AlertWindow
            // 
            this.Menu_AlertWindow.Image = ((System.Drawing.Image)(resources.GetObject("Menu_AlertWindow.Image")));
            this.Menu_AlertWindow.Name = "Menu_AlertWindow";
            this.Menu_AlertWindow.ShortcutKeys = System.Windows.Forms.Keys.F9;
            this.Menu_AlertWindow.Size = new System.Drawing.Size(249, 22);
            this.Menu_AlertWindow.Text = "Show Alert Window";
            this.Menu_AlertWindow.Click += new System.EventHandler(this.Menu_AlertWindow_Click);
            // 
            // Menu_StopAlert
            // 
            this.Menu_StopAlert.Name = "Menu_StopAlert";
            this.Menu_StopAlert.Size = new System.Drawing.Size(249, 22);
            this.Menu_StopAlert.Text = "Stop Alert";
            this.Menu_StopAlert.Click += new System.EventHandler(this.Menu_StopAlert_Click);
            // 
            // toolStripSeparator30
            // 
            this.toolStripSeparator30.Name = "toolStripSeparator30";
            this.toolStripSeparator30.Size = new System.Drawing.Size(246, 6);
            // 
            // Menu_SwitchChinese
            // 
            this.Menu_SwitchChinese.Image = ((System.Drawing.Image)(resources.GetObject("Menu_SwitchChinese.Image")));
            this.Menu_SwitchChinese.Name = "Menu_SwitchChinese";
            this.Menu_SwitchChinese.ShortcutKeys = System.Windows.Forms.Keys.F6;
            this.Menu_SwitchChinese.Size = new System.Drawing.Size(249, 22);
            this.Menu_SwitchChinese.Text = "Switch Trad/Simp Chinese";
            this.Menu_SwitchChinese.Click += new System.EventHandler(this.Menu_SwitchChinese_Click);
            // 
            // Menu_MainTools
            // 
            this.Menu_MainTools.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.Menu_Import,
            this.Menu_ImportFolder,
            this.Menu_Export,
            this.toolStripSeparator32,
            this.Menu_Recover,
            this.Menu_Empty,
            this.toolStripSeparator33,
            this.Menu_AddToUsages,
            this.Menu_ViewUsages,
            this.toolStripSeparator34,
            this.Menu_SmartMerge,
            this.Menu_Compact,
            this.Menu_ClearAllFormatting,
            this.toolStripSeparator9,
            this.Menu_ClearRegistrySettings});
            this.Menu_MainTools.Name = "Menu_MainTools";
            this.Menu_MainTools.Size = new System.Drawing.Size(46, 20);
            this.Menu_MainTools.Text = "&Tools";
            // 
            // Menu_Import
            // 
            this.Menu_Import.Name = "Menu_Import";
            this.Menu_Import.Size = new System.Drawing.Size(289, 22);
            this.Menu_Import.Text = "&Import";
            this.Menu_Import.Click += new System.EventHandler(this.Menu_Import_Click);
            // 
            // Menu_ImportFolder
            // 
            this.Menu_ImportFolder.Name = "Menu_ImportFolder";
            this.Menu_ImportFolder.Size = new System.Drawing.Size(289, 22);
            this.Menu_ImportFolder.Text = "I&mport Folder";
            this.Menu_ImportFolder.Click += new System.EventHandler(this.Menu_ImportFolder_Click);
            // 
            // Menu_Export
            // 
            this.Menu_Export.Name = "Menu_Export";
            this.Menu_Export.Size = new System.Drawing.Size(289, 22);
            this.Menu_Export.Text = "&Export";
            this.Menu_Export.Click += new System.EventHandler(this.Menu_Export_Click);
            // 
            // toolStripSeparator32
            // 
            this.toolStripSeparator32.Name = "toolStripSeparator32";
            this.toolStripSeparator32.Size = new System.Drawing.Size(286, 6);
            // 
            // Menu_Recover
            // 
            this.Menu_Recover.Name = "Menu_Recover";
            this.Menu_Recover.Size = new System.Drawing.Size(289, 22);
            this.Menu_Recover.Text = "&Recover Deleted Items";
            this.Menu_Recover.Click += new System.EventHandler(this.Menu_Recover_Click);
            // 
            // Menu_Empty
            // 
            this.Menu_Empty.Name = "Menu_Empty";
            this.Menu_Empty.Size = new System.Drawing.Size(289, 22);
            this.Menu_Empty.Text = "&Empty Deleted Folder...";
            this.Menu_Empty.Click += new System.EventHandler(this.Menu_Empty_Click);
            // 
            // toolStripSeparator33
            // 
            this.toolStripSeparator33.Name = "toolStripSeparator33";
            this.toolStripSeparator33.Size = new System.Drawing.Size(286, 6);
            // 
            // Menu_AddToUsages
            // 
            this.Menu_AddToUsages.Name = "Menu_AddToUsages";
            this.Menu_AddToUsages.Size = new System.Drawing.Size(289, 22);
            this.Menu_AddToUsages.Text = "&Add Worship List to Usages";
            this.Menu_AddToUsages.Click += new System.EventHandler(this.Menu_AddToUsages_Click);
            // 
            // Menu_ViewUsages
            // 
            this.Menu_ViewUsages.Name = "Menu_ViewUsages";
            this.Menu_ViewUsages.Size = new System.Drawing.Size(289, 22);
            this.Menu_ViewUsages.Text = "&View usages";
            this.Menu_ViewUsages.Click += new System.EventHandler(this.Menu_ViewUsages_Click);
            // 
            // toolStripSeparator34
            // 
            this.toolStripSeparator34.Name = "toolStripSeparator34";
            this.toolStripSeparator34.Size = new System.Drawing.Size(286, 6);
            // 
            // Menu_SmartMerge
            // 
            this.Menu_SmartMerge.Name = "Menu_SmartMerge";
            this.Menu_SmartMerge.Size = new System.Drawing.Size(289, 22);
            this.Menu_SmartMerge.Text = "&Smart Merge";
            this.Menu_SmartMerge.Click += new System.EventHandler(this.Menu_SmartMerge_Click);
            // 
            // Menu_Compact
            // 
            this.Menu_Compact.Name = "Menu_Compact";
            this.Menu_Compact.Size = new System.Drawing.Size(289, 22);
            this.Menu_Compact.Text = "&Compact and Repair Databases";
            this.Menu_Compact.Click += new System.EventHandler(this.Menu_Compact_Click);
            // 
            // Menu_ClearAllFormatting
            // 
            this.Menu_ClearAllFormatting.Name = "Menu_ClearAllFormatting";
            this.Menu_ClearAllFormatting.Size = new System.Drawing.Size(289, 22);
            this.Menu_ClearAllFormatting.Text = "Clear All &Formatting in Database.";
            this.Menu_ClearAllFormatting.Click += new System.EventHandler(this.Menu_ClearAllFormatting_Click);
            // 
            // toolStripSeparator9
            // 
            this.toolStripSeparator9.Name = "toolStripSeparator9";
            this.toolStripSeparator9.Size = new System.Drawing.Size(286, 6);
            // 
            // Menu_ClearRegistrySettings
            // 
            this.Menu_ClearRegistrySettings.Name = "Menu_ClearRegistrySettings";
            this.Menu_ClearRegistrySettings.Size = new System.Drawing.Size(289, 22);
            this.Menu_ClearRegistrySettings.Text = "Clear EasiSlides Registry Settings and Exit";
            this.Menu_ClearRegistrySettings.Click += new System.EventHandler(this.Menu_ClearRegistrySettings_Click);
            // 
            // Menu_MainHelp
            // 
            this.Menu_MainHelp.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.Menu_Contents,
            this.Menu_HelpWeb,
            this.toolStripSeparator31,
            this.Menu_Register,
            this.Menu_About});
            this.Menu_MainHelp.Name = "Menu_MainHelp";
            this.Menu_MainHelp.Size = new System.Drawing.Size(44, 20);
            this.Menu_MainHelp.Text = "&Help";
            // 
            // Menu_Contents
            // 
            this.Menu_Contents.Image = ((System.Drawing.Image)(resources.GetObject("Menu_Contents.Image")));
            this.Menu_Contents.Name = "Menu_Contents";
            this.Menu_Contents.ShortcutKeys = System.Windows.Forms.Keys.F1;
            this.Menu_Contents.Size = new System.Drawing.Size(205, 22);
            this.Menu_Contents.Text = "Contents";
            this.Menu_Contents.Click += new System.EventHandler(this.Menu_Contents_Click);
            // 
            // Menu_HelpWeb
            // 
            this.Menu_HelpWeb.Image = ((System.Drawing.Image)(resources.GetObject("Menu_HelpWeb.Image")));
            this.Menu_HelpWeb.Name = "Menu_HelpWeb";
            this.Menu_HelpWeb.Size = new System.Drawing.Size(205, 22);
            this.Menu_HelpWeb.Text = "Help on the Web";
            this.Menu_HelpWeb.Click += new System.EventHandler(this.Menu_HelpWeb_Click);
            // 
            // toolStripSeparator31
            // 
            this.toolStripSeparator31.Name = "toolStripSeparator31";
            this.toolStripSeparator31.Size = new System.Drawing.Size(202, 6);
            // 
            // Menu_Register
            // 
            this.Menu_Register.Name = "Menu_Register";
            this.Menu_Register.Size = new System.Drawing.Size(205, 22);
            this.Menu_Register.Text = "Register Use of EasiSlides";
            this.Menu_Register.Click += new System.EventHandler(this.Menu_Register_Click);
            // 
            // Menu_About
            // 
            this.Menu_About.Name = "Menu_About";
            this.Menu_About.Size = new System.Drawing.Size(205, 22);
            this.Menu_About.Text = "About EasiSlides";
            this.Menu_About.Click += new System.EventHandler(this.Menu_About_Click);
            // 
            // statusStripMain
            // 
            this.statusStripMain.AutoSize = false;
            this.statusStripMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.StatusBarPanel1,
            this.StatusBarPanel2,
            this.StatusBarPanel3,
            this.StatusBarPanel4});
            this.statusStripMain.Location = new System.Drawing.Point(0, 539);
            this.statusStripMain.Name = "statusStripMain";
            this.statusStripMain.Padding = new System.Windows.Forms.Padding(1, 0, 16, 0);
            this.statusStripMain.Size = new System.Drawing.Size(857, 25);
            this.statusStripMain.TabIndex = 1;
            // 
            // StatusBarPanel1
            // 
            this.StatusBarPanel1.AutoSize = false;
            this.StatusBarPanel1.AutoToolTip = true;
            this.StatusBarPanel1.BorderSides = ((System.Windows.Forms.ToolStripStatusLabelBorderSides)((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left | System.Windows.Forms.ToolStripStatusLabelBorderSides.Right)));
            this.StatusBarPanel1.BorderStyle = System.Windows.Forms.Border3DStyle.RaisedInner;
            this.StatusBarPanel1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.StatusBarPanel1.ImageAlign = System.Drawing.ContentAlignment.TopLeft;
            this.StatusBarPanel1.Name = "StatusBarPanel1";
            this.StatusBarPanel1.Padding = new System.Windows.Forms.Padding(3, 0, 4, 0);
            this.StatusBarPanel1.Size = new System.Drawing.Size(10, 20);
            this.StatusBarPanel1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // StatusBarPanel2
            // 
            this.StatusBarPanel2.AutoSize = false;
            this.StatusBarPanel2.AutoToolTip = true;
            this.StatusBarPanel2.BorderSides = ((System.Windows.Forms.ToolStripStatusLabelBorderSides)((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left | System.Windows.Forms.ToolStripStatusLabelBorderSides.Right)));
            this.StatusBarPanel2.BorderStyle = System.Windows.Forms.Border3DStyle.RaisedInner;
            this.StatusBarPanel2.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.StatusBarPanel2.ImageAlign = System.Drawing.ContentAlignment.TopLeft;
            this.StatusBarPanel2.Name = "StatusBarPanel2";
            this.StatusBarPanel2.Padding = new System.Windows.Forms.Padding(3, 0, 4, 0);
            this.StatusBarPanel2.Size = new System.Drawing.Size(10, 20);
            this.StatusBarPanel2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // StatusBarPanel3
            // 
            this.StatusBarPanel3.AutoSize = false;
            this.StatusBarPanel3.AutoToolTip = true;
            this.StatusBarPanel3.BorderSides = ((System.Windows.Forms.ToolStripStatusLabelBorderSides)((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left | System.Windows.Forms.ToolStripStatusLabelBorderSides.Right)));
            this.StatusBarPanel3.BorderStyle = System.Windows.Forms.Border3DStyle.RaisedInner;
            this.StatusBarPanel3.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.StatusBarPanel3.Name = "StatusBarPanel3";
            this.StatusBarPanel3.Padding = new System.Windows.Forms.Padding(3, 0, 4, 0);
            this.StatusBarPanel3.Size = new System.Drawing.Size(10, 20);
            this.StatusBarPanel3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // StatusBarPanel4
            // 
            this.StatusBarPanel4.AutoSize = false;
            this.StatusBarPanel4.AutoToolTip = true;
            this.StatusBarPanel4.BorderSides = ((System.Windows.Forms.ToolStripStatusLabelBorderSides)((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left | System.Windows.Forms.ToolStripStatusLabelBorderSides.Right)));
            this.StatusBarPanel4.BorderStyle = System.Windows.Forms.Border3DStyle.RaisedInner;
            this.StatusBarPanel4.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.StatusBarPanel4.Name = "StatusBarPanel4";
            this.StatusBarPanel4.Padding = new System.Windows.Forms.Padding(3, 0, 4, 0);
            this.StatusBarPanel4.Size = new System.Drawing.Size(10, 20);
            this.StatusBarPanel4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // TimerFlasher
            // 
            this.TimerFlasher.Interval = 600;
            this.TimerFlasher.Tick += new System.EventHandler(this.TimerFlasher_Tick);
            // 
            // TimerReMax
            // 
            this.TimerReMax.Interval = 1000;
            // 
            // TimerSearch
            // 
            this.TimerSearch.Interval = 500;
            this.TimerSearch.Tick += new System.EventHandler(this.TimerSearch_Tick);
            // 
            // CMenuImages
            // 
            this.CMenuImages.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.CMenuImages_AddItem,
            this.CMenuImages_AddDefault,
            this.toolStripSeparator35,
            this.CMenuImages_Refresh});
            this.CMenuImages.Name = "CMenuImages";
            this.CMenuImages.Size = new System.Drawing.Size(181, 76);
            this.CMenuImages.Opening += new System.ComponentModel.CancelEventHandler(this.CMenuImages_Opening);
            // 
            // CMenuImages_AddItem
            // 
            this.CMenuImages_AddItem.Name = "CMenuImages_AddItem";
            this.CMenuImages_AddItem.Size = new System.Drawing.Size(180, 22);
            this.CMenuImages_AddItem.Text = "Add to Item";
            this.CMenuImages_AddItem.Click += new System.EventHandler(this.CMenuImages_AddItem_Click);
            // 
            // CMenuImages_AddDefault
            // 
            this.CMenuImages_AddDefault.Name = "CMenuImages_AddDefault";
            this.CMenuImages_AddDefault.Size = new System.Drawing.Size(180, 22);
            this.CMenuImages_AddDefault.Text = "Add to Default";
            this.CMenuImages_AddDefault.Click += new System.EventHandler(this.CMenuImages_AddDefault_Click);
            // 
            // toolStripSeparator35
            // 
            this.toolStripSeparator35.Name = "toolStripSeparator35";
            this.toolStripSeparator35.Size = new System.Drawing.Size(177, 6);
            // 
            // CMenuImages_Refresh
            // 
            this.CMenuImages_Refresh.Name = "CMenuImages_Refresh";
            this.CMenuImages_Refresh.Size = new System.Drawing.Size(180, 22);
            this.CMenuImages_Refresh.Text = "Refresh Images Lists";
            this.CMenuImages_Refresh.Click += new System.EventHandler(this.CMenuImages_Refresh_Click);
            // 
            // TimerMessagingWindowOpen
            // 
            this.TimerMessagingWindowOpen.Interval = 1000;
            this.TimerMessagingWindowOpen.Tick += new System.EventHandler(this.TimerMessagingWindowOpen_Tick);
            // 
            // TimerToFront
            // 
            this.TimerToFront.Enabled = true;
            this.TimerToFront.Tick += new System.EventHandler(this.TimerToFront_Tick);
            // 
            // FrmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(857, 564);
            this.Controls.Add(this.toolStripContainerMain);
            this.Controls.Add(this.menuStripMain);
            this.Controls.Add(this.statusStripMain);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStripMain;
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "FrmMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "EasiSlides";
            this.TopMost = true;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FrmMain_FormClosing);
            this.Load += new System.EventHandler(this.FrmMain_Load);
            this.Resize += new System.EventHandler(this.FrmMain_Resize);
            this.toolStripContainerMain.ContentPanel.ResumeLayout(false);
            this.toolStripContainerMain.TopToolStripPanel.ResumeLayout(false);
            this.toolStripContainerMain.TopToolStripPanel.PerformLayout();
            this.toolStripContainerMain.ResumeLayout(false);
            this.toolStripContainerMain.PerformLayout();
            this.splitContainerMain.Panel1.ResumeLayout(false);
            this.splitContainerMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerMain)).EndInit();
            this.splitContainerMain.ResumeLayout(false);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.tabControlSource.ResumeLayout(false);
            this.tabFolders.ResumeLayout(false);
            this.panelFolders.ResumeLayout(false);
            this.toolStripFolders.ResumeLayout(false);
            this.toolStripFolders.PerformLayout();
            this.CMenuSongs.ResumeLayout(false);
            this.tabFiles.ResumeLayout(false);
            this.panelInfoScreen2.ResumeLayout(false);
            this.InfoScreentoolstrip2.ResumeLayout(false);
            this.InfoScreentoolstrip2.PerformLayout();
            this.panelExternalFiles.ResumeLayout(false);
            this.panelInfoScreen1.ResumeLayout(false);
            this.InfoScreentoolstrip1.ResumeLayout(false);
            this.InfoScreentoolstrip1.PerformLayout();
            this.CMenuFiles.ResumeLayout(false);
            this.tabPowerpoint.ResumeLayout(false);
            this.panelPowerpoint2.ResumeLayout(false);
            this.toolStripPowerpoint2.ResumeLayout(false);
            this.toolStripPowerpoint2.PerformLayout();
            this.panelPowerpoint1.ResumeLayout(false);
            this.panelExternalFiles1.ResumeLayout(false);
            this.toolStripPowerpoint1.ResumeLayout(false);
            this.toolStripPowerpoint1.PerformLayout();
            this.tabBibles.ResumeLayout(false);
            this.tabBibles.PerformLayout();
            this.CMenuBible.ResumeLayout(false);
            this.panelBible2.ResumeLayout(false);
            this.toolStripBible2.ResumeLayout(false);
            this.toolStripBible2.PerformLayout();
            this.TabBibleVersions.ResumeLayout(false);
            this.tabImages.ResumeLayout(false);
            this.panelImagesTop.ResumeLayout(false);
            this.panelImage1.ResumeLayout(false);
            this.toolStripImage1.ResumeLayout(false);
            this.toolStripImage1.PerformLayout();
            this.tabMedia.ResumeLayout(false);
            this.panel11.ResumeLayout(false);
            this.panelMedia1.ResumeLayout(false);
            this.toolStripMedia1.ResumeLayout(false);
            this.toolStripMedia1.PerformLayout();
            this.tabDefault.ResumeLayout(false);
            this.DefPanel.ResumeLayout(false);
            this.panelDefTemplate.ResumeLayout(false);
            this.toolStripDefTemplates.ResumeLayout(false);
            this.toolStripDefTemplates.PerformLayout();
            this.DefgroupBox2.ResumeLayout(false);
            this.panelDef4.ResumeLayout(false);
            this.toolStripDef4.ResumeLayout(false);
            this.toolStripDef4.PerformLayout();
            this.panelDef3.ResumeLayout(false);
            this.toolStripDef3.ResumeLayout(false);
            this.toolStripDef3.PerformLayout();
            this.DefgroupBox3.ResumeLayout(false);
            this.DefgroupBox3.PerformLayout();
            this.panel21.ResumeLayout(false);
            this.toolStripDef7.ResumeLayout(false);
            this.toolStripDef7.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Def_PanelHeight)).EndInit();
            this.panelDef5.ResumeLayout(false);
            this.toolStripDef5.ResumeLayout(false);
            this.toolStripDef5.PerformLayout();
            this.panelDef6.ResumeLayout(false);
            this.toolStripDef6.ResumeLayout(false);
            this.toolStripDef6.PerformLayout();
            this.DefgroupBox1.ResumeLayout(false);
            this.panelDef2.ResumeLayout(false);
            this.toolStripDef2.ResumeLayout(false);
            this.toolStripDef2.PerformLayout();
            this.panelDef1.ResumeLayout(false);
            this.toolStripDef1.ResumeLayout(false);
            this.toolStripDef1.PerformLayout();
            this.tabControlLists.ResumeLayout(false);
            this.tabWorshipList.ResumeLayout(false);
            this.panelWorshipList2.ResumeLayout(false);
            this.toolStripWorshipList2.ResumeLayout(false);
            this.toolStripWorshipList2.PerformLayout();
            this.panelWorshipList1.ResumeLayout(false);
            this.toolStripWorshipList1.ResumeLayout(false);
            this.toolStripWorshipList1.PerformLayout();
            this.CMenuWorship.ResumeLayout(false);
            this.tabPraiseBook.ResumeLayout(false);
            this.panelPraiseBook2.ResumeLayout(false);
            this.toolStripPraiseBook2.ResumeLayout(false);
            this.toolStripPraiseBook2.PerformLayout();
            this.panelPraiseBook1.ResumeLayout(false);
            this.toolStripPraiseBook1.ResumeLayout(false);
            this.toolStripPraiseBook1.PerformLayout();
            this.CMenuPraiseB.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.splitContainerPreview.Panel1.ResumeLayout(false);
            this.splitContainerPreview.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerPreview)).EndInit();
            this.splitContainerPreview.ResumeLayout(false);
            this.panelPreviewTop.ResumeLayout(false);
            this.IndPanel.ResumeLayout(false);
            this.IndPanel.PerformLayout();
            this.panelIndTemplate.ResumeLayout(false);
            this.toolStripIndTemplates.ResumeLayout(false);
            this.toolStripIndTemplates.PerformLayout();
            this.IndgroupBox4.ResumeLayout(false);
            this.IndgroupBox4.PerformLayout();
            this.panelInd7.ResumeLayout(false);
            this.toolStripInd7.ResumeLayout(false);
            this.toolStripInd7.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Ind_Reg2SizeUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.Ind_Reg2TopUpDown)).EndInit();
            this.panelInd6.ResumeLayout(false);
            this.toolStripInd6.ResumeLayout(false);
            this.toolStripInd6.PerformLayout();
            this.IndgroupBox3.ResumeLayout(false);
            this.IndgroupBox3.PerformLayout();
            this.panelInd5.ResumeLayout(false);
            this.toolStripInd5.ResumeLayout(false);
            this.toolStripInd5.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Ind_Reg1SizeUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.Ind_Reg1TopUpDown)).EndInit();
            this.panelInd4.ResumeLayout(false);
            this.toolStripInd4.ResumeLayout(false);
            this.toolStripInd4.PerformLayout();
            this.IndgroupBox2.ResumeLayout(false);
            this.IndgroupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Ind_BottomUpDown)).EndInit();
            this.panelInd3.ResumeLayout(false);
            this.toolStripInd3.ResumeLayout(false);
            this.toolStripInd3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Ind_RightUpDown)).EndInit();
            this.panelInd2.ResumeLayout(false);
            this.toolStripInd2.ResumeLayout(false);
            this.toolStripInd2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Ind_LeftUpDown)).EndInit();
            this.IndgroupBox1.ResumeLayout(false);
            this.panel4.ResumeLayout(false);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.panelInd1.ResumeLayout(false);
            this.toolStripInd1.ResumeLayout(false);
            this.toolStripInd1.PerformLayout();
            this.panel9.ResumeLayout(false);
            this.panelPreviewBottom.ResumeLayout(false);
            this.panelPreviewSessionNotes2.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.flowLayoutPanel1.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            this.splitContainerOutput.Panel1.ResumeLayout(false);
            this.splitContainerOutput.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerOutput)).EndInit();
            this.splitContainerOutput.ResumeLayout(false);
            this.panelOutputTop.ResumeLayout(false);
            this.panelOutputTop.PerformLayout();
            this.panel10.ResumeLayout(false);
            this.panelOutputBottom.ResumeLayout(false);
            this.panelOutputLM1.ResumeLayout(false);
            this.panelOutputLM1.PerformLayout();
            this.panelOutputLM3.ResumeLayout(false);
            this.panel8.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.flowLayoutPanel2.ResumeLayout(false);
            this.panel6.ResumeLayout(false);
            this.toolStripMain.ResumeLayout(false);
            this.toolStripMain.PerformLayout();
            this.menuStripMain.ResumeLayout(false);
            this.menuStripMain.PerformLayout();
            this.statusStripMain.ResumeLayout(false);
            this.statusStripMain.PerformLayout();
            this.CMenuImages.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

		}
	}

	//public class CustomFlowLayoutPanel : FlowLayoutPanel
	//{
	//    public CustomFlowLayoutPanel()
	//        : base()
	//    {
	//        this.SetStyle(ControlStyles.DoubleBuffer, true);
	//        this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
	//        this.SetStyle(ControlStyles.UserPaint, true);
	//    }

	//    protected override void OnScroll(ScrollEventArgs se)
	//    {
	//        this.Invalidate();

	//        base.OnScroll(se);
	//    }

	//    protected override CreateParams CreateParams
	//    {
	//        get
	//        {
	//            CreateParams cp = base.CreateParams;
	//            cp.ExStyle |= 0x02000000; // WS_CLIPCHILDREN
	//            return cp;
	//        }
	//    }
	//}
}


