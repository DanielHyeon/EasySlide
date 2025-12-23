//using NetOffice.DAOApi;
using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Data;
using Easislides.Util;
using Easislides.SQLite;
using Easislides.Module;
using Easislides.Properties;

namespace Easislides
{
	public class FrmInfoScreen : Form
	{
		private enum ControlsBtn
		{
			PlayPausebtn,
			Stopbtn,
			FFbtn,
			FRbtn,
			Closebtn
		}

		private const int MaxRotateContainers = 1024;

		private IContainer components = null;

		private ToolStrip toolStrip1;

		private StatusStrip statusStrip1;

		private SplitContainer splitContainer1;

		private Panel panelR1Top;

		private Panel panelR1Left;

		private RichTextBox tbLyrics1;

		private Label LabeltbLyrics;

		private Panel panelR1LeftMiddle;

		private Button R1Chinese;

		private Button R1BtnNotations;

		private Button R1BtnEnding;

		private Button R1BtnBridge;

		private Button R1BtnChorus2;

		private Button R1BtnChorus;

		private Button R1Btn10;

		private Button R1Btn9;

		private Button R1Btn8;

		private Button R1Btn7;

		private Button R1Btn6;

		private Button R1Btn5;

		private Button R1Btn4;

		private Button R1Btn3;

		private Button R1Btn2;

		private Button R1Btn1;

		private Panel panel6;

		private ToolStripButton Main_New;

		private ToolStripButton Main_Save;

		private ToolStripButton Main_Import;

		private ToolStripSeparator toolStripSeparator1;

		private ToolStripButton Main_WordWrap;

		private ToolStripSeparator toolStripSeparator2;

		private ToolStripButton Main_TransposeDown;

		private ToolStripButton Main_TransposeUp;

		private ToolStripSeparator toolStripSeparator4;

		private ToolStripComboBox ComboFontName;

		private ToolStripComboBox ComboMainFontSize;

		private ToolStripComboBox ComboNotationFontSize;

		private Button R1Redo;

		private Button R1Undo;

		private MenuStrip menuStripMain;

		private ToolStripMenuItem Menu_MainFile;

		private ToolStripMenuItem Menu_New;

		private ToolStripMenuItem Menu_Save;

		private ToolStripSeparator toolStripSeparator16;

		private ToolStripMenuItem Menu_EditHistoryList;

		private ToolStripSeparator toolStripSeparator18;

		private ToolStripMenuItem Menu_Exit;

		private ToolStripMenuItem Menu_MainTools;

		private ToolStripMenuItem Menu_TransposeDown;

		private ToolStripMenuItem Menu_TransposeUp;

		private Timer TimerEditRequest;

		private OpenFileDialog OpenFileDialog1;

		private SplitContainer splitContainerMain;

		private ToolTip toolTip1;

		private Button R1VerseFormat;

		private Button R1BtnNewScreen;

		private ToolStripMenuItem Menu_Import;

		private ToolStripMenuItem Menu_WordWrap;

		private ToolStripSeparator Menu_EditHistorySeparator;

		private GroupBox groupBox2;

		private Panel panelVerses;

		private ListView VersesList;

		private ColumnHeader columnHeader1;

		private ColumnHeader columnHeader2;

		private Panel panel2;

		private Label label16;

		private Panel panelOrderList;

		private ListView OrderList;

		private ColumnHeader columnHeader3;

		private ColumnHeader columnHeader4;

		private Panel panel4;

		private Label label17;

		private Panel panelSeqUpDown;

		private ToolStrip toolStripSeqUpDown;

		private ToolStripButton OrderList_Up;

		private ToolStripButton OrderList_Down;

		private ToolStripSeparator toolStripSeparator5;

		private ToolStripButton OrderList_Delete;

		private Panel panelSeqSet;

		private ToolStrip toolStripSeqSet;

		private ToolStripButton Verses_Add;

		private ToolStripButton Verses_SmartAdd;

		private GroupBox groupBox1;

		private Panel panel7;

		private Panel panelLinkTitle2Lookup;

		private ToolStrip toolStrip2;

		private ToolStripButton Title2_LookUp;

		private Panel LinkTitle2Pic;

		private ComboBox SongFolder;

		private TextBox CopyrightInfo;

		private Label label2;

		private TextBox WriterInfo;

		private Label label3;

		private TextBox SongTitle2;

		private Label label4;

		private TextBox SongTitle;

		private Label label5;

		private Panel panel8;

		private Label label6;

		private TextBox UserReference;

		private Label label8;

		private TextBox BookReference;

		private Label label9;

		private Label label10;

		private ComboBox LicAdminInfo2;

		private Label label7;

		private ComboBox LicAdminInfo1;

		private ComboBox SongTiming;

		private Label label13;

		private ComboBox SongKey;

		private TextBox SongNumber;

		private ComboBox SongCapo;

		private Label label11;

		private Label label12;

		private Label labelFormat;

		private Button Btn_BookRef;

		private Button Btn_UserRef;

		private Button Btn_Title;

		private Button Btn_Copyright;

		private Button Btn_Writer;

		private Button Btn_Title2;

		private Button R1BtnPreChorus2;

		private Button R1BtnPreChorus;

		private TabControl tabRightPane;

		private TabPage tabRight_Region2;

		private Panel panelR2All;

		private RichTextBox tbLyrics2;

		private Panel panelR2Top;

		private Label LabeltbLyrics2;

		private Panel panelR2Left;

		private Button SyncBtnDown;

		private Button SyncBtnUp;

		private Panel panelR2LeftMiddle;

		private Button R2BtnPreChorus2;

		private Button R2BtnPreChorus;

		private Button R2VerseFormat;

		private Button R2BtnNewScreen;

		private Button R2Redo;

		private Button R2Undo;

		private Button R2Chinese;

		private Button R2BtnNotations;

		private Button R2BtnEnding;

		private Button R2BtnBridge;

		private Button R2BtnChorus2;

		private Button R2BtnChorus;

		private Button R2Btn10;

		private Button R2Btn9;

		private Button R2Btn8;

		private Button R2Btn7;

		private Button R2Btn6;

		private Button R2Btn5;

		private Button R2Btn4;

		private Button R2Btn3;

		private Button R2Btn2;

		private Button R2Btn1;

		private TabPage tabRight_Rotate;

		private Panel panelRotate;

		private SplitContainer splitContainerRotate;

		private GroupBox groupBoxRotateVerses;

		private Panel panelRotate_Verses;

		private ListView Rotate_VersesList;

		private ColumnHeader columnHeader6;

		private ColumnHeader columnHeader7;

		private ColumnHeader columnHeader8;

		private Panel panel10;

		private Label label23;

		private Panel panelRotate_OrderList;

		private ListView Rotate_OrderList;

		private ColumnHeader columnHeader9;

		private ColumnHeader columnHeader10;

		private Panel panel12;

		private Label label24;

		private Panel panel13;

		private ToolStrip toolStripRotate_SeqSet;

		private ToolStripButton Rotate_Verses_Add;

		private ToolStripButton Rotate_Verses_SmartAdd;

		private Panel panel14;

		private ToolStrip toolStripRotate_SeqUpDown;

		private ToolStripButton Rotate_OrderList_Up;

		private ToolStripButton Rotate_OrderList_Down;

		private ToolStripSeparator toolStripSeparator3;

		private ToolStripButton Rotate_OrderList_Delete;

		private Panel panelRotate_Sample;

		private Button btnAddPosition;

		private Label LabelMediaType;

		private Label labelMed;

		private Panel panelRotate_Media;

		private Panel panelNoPlayer;

		private Label label14;

		private Label labelNoPlayer1;

		private Label labelNoPlayer2;

		private Label labelPos;

		private Label LabelPosition;

		private Panel panelLoc;

		private ToolStrip toolStrip3;

		private ToolStripButton Rotate_LocationBtn;

		private Label LabelDuration;

		private Button btnDuration;

		private TextBox Rotate_tbSourceLocation;

		private TrackBar TrackBarVolume;

		private Panel panelPlayBtns;

		private TrackBar TrackBarDuration;

		private Button StopBtn;

		private Button PlayPauseBtn;

		private Button FastReverseBtn;

		private Button FastForwardBtn;

		private Label labelVol;

		private Panel panelRotateLeft;

		private FlowLayoutPanel flowLayoutRotate;

		private Panel panelRotateLeftTop2;

		private DateTimePicker Rotate_TimeTotal;

		private Button btnClearMediaPositions;

		private Label label21;

		private Label label19;

		private Panel panelRotateLeftTop1;

		private GroupBox groupBox3;

		private NumericUpDown Rotate_SlidesGapUpDown;

		private RadioButton Rotate_Equal;

		private RadioButton Rotate_Multiple;

		private ToolStripStatusLabel toolStripStatusLabel1;

		private Timer TimerFast;

		private Timer TimerAttemptConnect;

		private Timer TimerTrack;

		private Button R1BtnBridge2;

		private Button R2BtnBridge2;

		private ContextMenuStrip CMRegion1;

		private ToolStripMenuItem CMRegion1_Copy;

		private ToolStripMenuItem CMRegion1_Paste;

		private ContextMenuStrip CMRegion2;

		private ToolStripMenuItem CMRegion2_Copy;

		private ToolStripMenuItem CMRegion2_Paste;

		private ToolStripSeparator toolStripSeparator6;

		private ToolStripMenuItem Menu_ShowAllButtons;

		private Panel panel3;

		private Panel panelR1LeftTop;

		private Panel panelR1LeftBottom;

		private Panel panel1;

		private Panel panelR2LeftTop;

		private Panel panelR2LeftBottom;

		private ToolStripMenuItem Menu_SaveAs;

		private SaveFileDialog saveFileDialog1;

		private ToolStripButton Main_SaveExit;

		private ToolStripMenuItem Menu_SaveExit;

		private CheckBox R1RightToLeft;

		private CheckBox R2RightToLeft;

		private ToolStripMenuItem Menu_ChordsMenu;

		private ToolStripButton Main_ChordsMenu;

		private Label labelDur;

		private bool InitLoad = true;

		private PopupWindowHelper popupHelper = null;

		private string PopupBtnPressed = "";

		private string Lyrics1SavedCopy = "";

		private string Lyrics2SavedCopy = "";

		private string wArray = "";

		private int[] VerseListIndex = new int[160];

		private bool LyricsVisited;

		private bool VerseSymbolChanged;

		private bool[] prevVersePresent = new bool[160];

		private bool[] VersePresent = new bool[160];

		private int[] VersePresentNewScreenCount = new int[160];

		private string SongLyrics;

		private bool InsertAction;

		private int LeftMargin;

		private byte[] VerseArray;

		private string SQLFolderLookUp;

		private string SavedFileName;

		private string SavedWriterInfo;

		private string SavedCopyrightInfo;

		private string SavedSequence;

		private string SavedFolder;

		private string SavedUserReference;

		private string SavedBookReference;

		private string SavedSongNumber;

		private string SavedTitle2;

		private string SavedTitle;

		private string OrderListSequence;

		private string SavedMusicNotations;

		private string SavedSongTiming;

		private string SavedSongKey;

		private string SavedCapo;

		private string SavedLicAdminInfo2;

		private string SavedLicAdminInfo1;

		private string SavedRotateString;

		private string ThisItemFileName = "";

		private int countc;

		private int counta;

		private bool Title2IgnoreChange;

		private long countb;

		private string InitSongTitle2;

		private int FormState;

		private string[] sArray;

		private bool LoadingSong = false;

		private int LastCurPos = 0;

		private Font MainFont;

		private Font NotationFont;

		private bool CurFolderFound = false;

		private bool FormCanClose = false;

		private bool IgnoreChange = false;

		private bool InsertingPresetItem = false;

		private int StackArrayMaxPoint = 100;

		private bool SetWordWrap = true;

		private bool SetChordsMenu = true;

		private bool SetRightToLeft1 = true;

		private bool SetRightToLeft2 = true;

		private int[,] StackTrackPos = new int[3, 2];

		private int[] StackMaxRedo = new int[3];

		private int[] StackIndex = new int[3];

		private int[] StackStartPoint = new int[3];

		private string[,] sStack = new string[3, 1000];

		private int[,] iCursorPosition = new int[3, 1000];

		private string[] Info_HeaderData = new string[255];

		private string CurFileName = "";

		private string PreviousFileDir = "";

		private string CombinedLyrics = "";

		private string CombinedNotations = "";

		private bool InitFontsLists = true;

		private int PrevSplitterDistance = 0;

		private bool TopPanelLocked = true;

		private RichTextBox tbWorkspace = new RichTextBox();

		private RichTextBox tbTempSpace = new RichTextBox();

		private string Lyrics1SavedNotations = "";

		private string Lyrics1Only = "";

		private string Lyrics2SavedNotations = "";

		private string Lyrics2Only = "";

		private string Reg_FormLeft = "InfoScreenDBLeft";

		private string Reg_FormTop = "InfoScreenDBTop";

		private string Reg_FormWidth = "InfoScreenDBWidth";

		private string Reg_FormHeight = "InfoScreenDBHeight";

		private string Reg_FormMax = "InfoScreenDBMax";

		private string Reg_FormWordWrap = "InfoScreenDB_WordWrap";

		private string Reg_FormSetChordsMenu = "InfoScreenDB_ChordsMenu";

		private string Reg_FormLyricsSplit = "InfoScreenDB_LyricsSplit";

		private string Reg_FormRegion2Tab = "InfoScreenDB_Region2Tab";

		private ListView tempSequenceCopied = new ListView();

		private ListView ListViewNotations = new ListView();

		private ListView ListViewNotationLog = new ListView();

		private ListView ScreenBreaks1 = new ListView();

		private ListView ScreenBreaks2 = new ListView();

		private bool ScreenBreak1Available = false;

		private bool ScreenBreak2Available = false;

		private string RotateString = "";

		private DateTime InitDateTime = new DateTime(2005, 1, 1, 0, 0, 0);

		private int RotateTotalScreensIndex = -1;

		private double TimeIncrement = 1.0;

		private DateTimePicker[] RotateTimePosition = new DateTimePicker[1024];

		private Label[] RotateTimeLabel = new Label[1024];

		private int AttemptConnectCount = 0;

		private int MaxAttemptConnectCount = 60;

		private double CurMediaPosition = 0.0;

		private double CurMediaLength = 0.0;

		private int[] VerseScreenCount = new int[160];

		private DShowLib DShowPlayer = new DShowLib();

		private bool PlayerOK = false;

		private int tbLyrics1MouseUpPos = 0;

		private int tbLyrics2MouseUpPos = 0;

		private bool SplitterReAdjust = false;

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
			components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmInfoScreen));
			toolStrip1 = new System.Windows.Forms.ToolStrip();
			Main_New = new System.Windows.Forms.ToolStripButton();
			Main_Save = new System.Windows.Forms.ToolStripButton();
			Main_SaveExit = new System.Windows.Forms.ToolStripButton();
			toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			Main_Import = new System.Windows.Forms.ToolStripButton();
			Main_WordWrap = new System.Windows.Forms.ToolStripButton();
			Main_ChordsMenu = new System.Windows.Forms.ToolStripButton();
			toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
			Main_TransposeDown = new System.Windows.Forms.ToolStripButton();
			Main_TransposeUp = new System.Windows.Forms.ToolStripButton();
			toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
			ComboFontName = new System.Windows.Forms.ToolStripComboBox();
			ComboMainFontSize = new System.Windows.Forms.ToolStripComboBox();
			ComboNotationFontSize = new System.Windows.Forms.ToolStripComboBox();
			statusStrip1 = new System.Windows.Forms.StatusStrip();
			toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
			splitContainer1 = new System.Windows.Forms.SplitContainer();
			tbLyrics1 = new System.Windows.Forms.RichTextBox();
			CMRegion1 = new System.Windows.Forms.ContextMenuStrip(components);
			CMRegion1_Copy = new System.Windows.Forms.ToolStripMenuItem();
			CMRegion1_Paste = new System.Windows.Forms.ToolStripMenuItem();
			panelR1Top = new System.Windows.Forms.Panel();
			LabeltbLyrics = new System.Windows.Forms.Label();
			panelR1Left = new System.Windows.Forms.Panel();
			panel3 = new System.Windows.Forms.Panel();
			panelR1LeftBottom = new System.Windows.Forms.Panel();
			R1RightToLeft = new System.Windows.Forms.CheckBox();
			R1Chinese = new System.Windows.Forms.Button();
			panelR1LeftMiddle = new System.Windows.Forms.Panel();
			R1BtnBridge2 = new System.Windows.Forms.Button();
			R1BtnPreChorus2 = new System.Windows.Forms.Button();
			R1BtnChorus2 = new System.Windows.Forms.Button();
			R1BtnPreChorus = new System.Windows.Forms.Button();
			R1BtnChorus = new System.Windows.Forms.Button();
			R1VerseFormat = new System.Windows.Forms.Button();
			R1BtnNewScreen = new System.Windows.Forms.Button();
			R1BtnNotations = new System.Windows.Forms.Button();
			R1BtnEnding = new System.Windows.Forms.Button();
			R1BtnBridge = new System.Windows.Forms.Button();
			R1Btn10 = new System.Windows.Forms.Button();
			R1Btn9 = new System.Windows.Forms.Button();
			R1Btn8 = new System.Windows.Forms.Button();
			R1Btn7 = new System.Windows.Forms.Button();
			R1Btn6 = new System.Windows.Forms.Button();
			R1Btn5 = new System.Windows.Forms.Button();
			R1Btn4 = new System.Windows.Forms.Button();
			R1Btn3 = new System.Windows.Forms.Button();
			R1Btn2 = new System.Windows.Forms.Button();
			R1Btn1 = new System.Windows.Forms.Button();
			panelR1LeftTop = new System.Windows.Forms.Panel();
			R1Undo = new System.Windows.Forms.Button();
			R1Redo = new System.Windows.Forms.Button();
			tabRightPane = new System.Windows.Forms.TabControl();
			tabRight_Region2 = new System.Windows.Forms.TabPage();
			panelR2All = new System.Windows.Forms.Panel();
			tbLyrics2 = new System.Windows.Forms.RichTextBox();
			CMRegion2 = new System.Windows.Forms.ContextMenuStrip(components);
			CMRegion2_Copy = new System.Windows.Forms.ToolStripMenuItem();
			CMRegion2_Paste = new System.Windows.Forms.ToolStripMenuItem();
			panelR2Top = new System.Windows.Forms.Panel();
			LabeltbLyrics2 = new System.Windows.Forms.Label();
			panelR2Left = new System.Windows.Forms.Panel();
			panel1 = new System.Windows.Forms.Panel();
			panelR2LeftBottom = new System.Windows.Forms.Panel();
			R2RightToLeft = new System.Windows.Forms.CheckBox();
			R2Chinese = new System.Windows.Forms.Button();
			panelR2LeftMiddle = new System.Windows.Forms.Panel();
			R2BtnBridge2 = new System.Windows.Forms.Button();
			R2BtnPreChorus2 = new System.Windows.Forms.Button();
			R2BtnPreChorus = new System.Windows.Forms.Button();
			R2VerseFormat = new System.Windows.Forms.Button();
			R2BtnChorus = new System.Windows.Forms.Button();
			R2BtnChorus2 = new System.Windows.Forms.Button();
			R2BtnNewScreen = new System.Windows.Forms.Button();
			R2BtnNotations = new System.Windows.Forms.Button();
			R2BtnEnding = new System.Windows.Forms.Button();
			R2BtnBridge = new System.Windows.Forms.Button();
			R2Btn10 = new System.Windows.Forms.Button();
			R2Btn9 = new System.Windows.Forms.Button();
			R2Btn8 = new System.Windows.Forms.Button();
			R2Btn7 = new System.Windows.Forms.Button();
			R2Btn6 = new System.Windows.Forms.Button();
			R2Btn5 = new System.Windows.Forms.Button();
			R2Btn4 = new System.Windows.Forms.Button();
			R2Btn3 = new System.Windows.Forms.Button();
			R2Btn2 = new System.Windows.Forms.Button();
			R2Btn1 = new System.Windows.Forms.Button();
			panelR2LeftTop = new System.Windows.Forms.Panel();
			R2Undo = new System.Windows.Forms.Button();
			R2Redo = new System.Windows.Forms.Button();
			SyncBtnDown = new System.Windows.Forms.Button();
			SyncBtnUp = new System.Windows.Forms.Button();
			tabRight_Rotate = new System.Windows.Forms.TabPage();
			panelRotate = new System.Windows.Forms.Panel();
			splitContainerRotate = new System.Windows.Forms.SplitContainer();
			groupBoxRotateVerses = new System.Windows.Forms.GroupBox();
			panelRotate_Verses = new System.Windows.Forms.Panel();
			Rotate_VersesList = new System.Windows.Forms.ListView();
			columnHeader6 = new System.Windows.Forms.ColumnHeader();
			columnHeader7 = new System.Windows.Forms.ColumnHeader();
			columnHeader8 = new System.Windows.Forms.ColumnHeader();
			panel10 = new System.Windows.Forms.Panel();
			label23 = new System.Windows.Forms.Label();
			panelRotate_OrderList = new System.Windows.Forms.Panel();
			Rotate_OrderList = new System.Windows.Forms.ListView();
			columnHeader9 = new System.Windows.Forms.ColumnHeader();
			columnHeader10 = new System.Windows.Forms.ColumnHeader();
			panel12 = new System.Windows.Forms.Panel();
			label24 = new System.Windows.Forms.Label();
			panel13 = new System.Windows.Forms.Panel();
			toolStripRotate_SeqSet = new System.Windows.Forms.ToolStrip();
			Rotate_Verses_Add = new System.Windows.Forms.ToolStripButton();
			Rotate_Verses_SmartAdd = new System.Windows.Forms.ToolStripButton();
			panel14 = new System.Windows.Forms.Panel();
			toolStripRotate_SeqUpDown = new System.Windows.Forms.ToolStrip();
			Rotate_OrderList_Up = new System.Windows.Forms.ToolStripButton();
			Rotate_OrderList_Down = new System.Windows.Forms.ToolStripButton();
			toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
			Rotate_OrderList_Delete = new System.Windows.Forms.ToolStripButton();
			panelRotate_Sample = new System.Windows.Forms.Panel();
			labelDur = new System.Windows.Forms.Label();
			btnAddPosition = new System.Windows.Forms.Button();
			btnDuration = new System.Windows.Forms.Button();
			LabelMediaType = new System.Windows.Forms.Label();
			labelMed = new System.Windows.Forms.Label();
			panelRotate_Media = new System.Windows.Forms.Panel();
			panelNoPlayer = new System.Windows.Forms.Panel();
			label14 = new System.Windows.Forms.Label();
			labelNoPlayer1 = new System.Windows.Forms.Label();
			labelNoPlayer2 = new System.Windows.Forms.Label();
			labelPos = new System.Windows.Forms.Label();
			LabelPosition = new System.Windows.Forms.Label();
			panelLoc = new System.Windows.Forms.Panel();
			toolStrip3 = new System.Windows.Forms.ToolStrip();
			Rotate_LocationBtn = new System.Windows.Forms.ToolStripButton();
			LabelDuration = new System.Windows.Forms.Label();
			Rotate_tbSourceLocation = new System.Windows.Forms.TextBox();
			TrackBarVolume = new System.Windows.Forms.TrackBar();
			panelPlayBtns = new System.Windows.Forms.Panel();
			TrackBarDuration = new System.Windows.Forms.TrackBar();
			StopBtn = new System.Windows.Forms.Button();
			PlayPauseBtn = new System.Windows.Forms.Button();
			FastReverseBtn = new System.Windows.Forms.Button();
			FastForwardBtn = new System.Windows.Forms.Button();
			labelVol = new System.Windows.Forms.Label();
			panelRotateLeft = new System.Windows.Forms.Panel();
			flowLayoutRotate = new System.Windows.Forms.FlowLayoutPanel();
			panelRotateLeftTop2 = new System.Windows.Forms.Panel();
			Rotate_TimeTotal = new System.Windows.Forms.DateTimePicker();
			btnClearMediaPositions = new System.Windows.Forms.Button();
			label21 = new System.Windows.Forms.Label();
			label19 = new System.Windows.Forms.Label();
			panelRotateLeftTop1 = new System.Windows.Forms.Panel();
			groupBox3 = new System.Windows.Forms.GroupBox();
			Rotate_SlidesGapUpDown = new System.Windows.Forms.NumericUpDown();
			Rotate_Equal = new System.Windows.Forms.RadioButton();
			Rotate_Multiple = new System.Windows.Forms.RadioButton();
			panel6 = new System.Windows.Forms.Panel();
			menuStripMain = new System.Windows.Forms.MenuStrip();
			Menu_MainFile = new System.Windows.Forms.ToolStripMenuItem();
			Menu_New = new System.Windows.Forms.ToolStripMenuItem();
			Menu_Save = new System.Windows.Forms.ToolStripMenuItem();
			Menu_SaveAs = new System.Windows.Forms.ToolStripMenuItem();
			Menu_SaveExit = new System.Windows.Forms.ToolStripMenuItem();
			toolStripSeparator16 = new System.Windows.Forms.ToolStripSeparator();
			Menu_EditHistoryList = new System.Windows.Forms.ToolStripMenuItem();
			toolStripSeparator18 = new System.Windows.Forms.ToolStripSeparator();
			Menu_Exit = new System.Windows.Forms.ToolStripMenuItem();
			Menu_MainTools = new System.Windows.Forms.ToolStripMenuItem();
			Menu_Import = new System.Windows.Forms.ToolStripMenuItem();
			Menu_WordWrap = new System.Windows.Forms.ToolStripMenuItem();
			Menu_ChordsMenu = new System.Windows.Forms.ToolStripMenuItem();
			Menu_EditHistorySeparator = new System.Windows.Forms.ToolStripSeparator();
			Menu_TransposeDown = new System.Windows.Forms.ToolStripMenuItem();
			Menu_TransposeUp = new System.Windows.Forms.ToolStripMenuItem();
			toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
			Menu_ShowAllButtons = new System.Windows.Forms.ToolStripMenuItem();
			TimerEditRequest = new System.Windows.Forms.Timer(components);
			OpenFileDialog1 = new System.Windows.Forms.OpenFileDialog();
			splitContainerMain = new System.Windows.Forms.SplitContainer();
			groupBox2 = new System.Windows.Forms.GroupBox();
			panelVerses = new System.Windows.Forms.Panel();
			VersesList = new System.Windows.Forms.ListView();
			columnHeader1 = new System.Windows.Forms.ColumnHeader();
			columnHeader2 = new System.Windows.Forms.ColumnHeader();
			panel2 = new System.Windows.Forms.Panel();
			label16 = new System.Windows.Forms.Label();
			panelOrderList = new System.Windows.Forms.Panel();
			OrderList = new System.Windows.Forms.ListView();
			columnHeader3 = new System.Windows.Forms.ColumnHeader();
			columnHeader4 = new System.Windows.Forms.ColumnHeader();
			panel4 = new System.Windows.Forms.Panel();
			label17 = new System.Windows.Forms.Label();
			panelSeqSet = new System.Windows.Forms.Panel();
			toolStripSeqSet = new System.Windows.Forms.ToolStrip();
			Verses_Add = new System.Windows.Forms.ToolStripButton();
			Verses_SmartAdd = new System.Windows.Forms.ToolStripButton();
			panelSeqUpDown = new System.Windows.Forms.Panel();
			toolStripSeqUpDown = new System.Windows.Forms.ToolStrip();
			OrderList_Up = new System.Windows.Forms.ToolStripButton();
			OrderList_Down = new System.Windows.Forms.ToolStripButton();
			toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
			OrderList_Delete = new System.Windows.Forms.ToolStripButton();
			groupBox1 = new System.Windows.Forms.GroupBox();
			panel7 = new System.Windows.Forms.Panel();
			Btn_Title2 = new System.Windows.Forms.Button();
			Btn_Title = new System.Windows.Forms.Button();
			Btn_Copyright = new System.Windows.Forms.Button();
			Btn_Writer = new System.Windows.Forms.Button();
			SongFolder = new System.Windows.Forms.ComboBox();
			panelLinkTitle2Lookup = new System.Windows.Forms.Panel();
			toolStrip2 = new System.Windows.Forms.ToolStrip();
			Title2_LookUp = new System.Windows.Forms.ToolStripButton();
			LinkTitle2Pic = new System.Windows.Forms.Panel();
			CopyrightInfo = new System.Windows.Forms.TextBox();
			label2 = new System.Windows.Forms.Label();
			WriterInfo = new System.Windows.Forms.TextBox();
			label3 = new System.Windows.Forms.Label();
			SongTitle2 = new System.Windows.Forms.TextBox();
			label4 = new System.Windows.Forms.Label();
			SongTitle = new System.Windows.Forms.TextBox();
			label5 = new System.Windows.Forms.Label();
			labelFormat = new System.Windows.Forms.Label();
			panel8 = new System.Windows.Forms.Panel();
			Btn_BookRef = new System.Windows.Forms.Button();
			Btn_UserRef = new System.Windows.Forms.Button();
			label6 = new System.Windows.Forms.Label();
			UserReference = new System.Windows.Forms.TextBox();
			BookReference = new System.Windows.Forms.TextBox();
			label9 = new System.Windows.Forms.Label();
			label10 = new System.Windows.Forms.Label();
			LicAdminInfo2 = new System.Windows.Forms.ComboBox();
			LicAdminInfo1 = new System.Windows.Forms.ComboBox();
			SongTiming = new System.Windows.Forms.ComboBox();
			label13 = new System.Windows.Forms.Label();
			SongKey = new System.Windows.Forms.ComboBox();
			SongNumber = new System.Windows.Forms.TextBox();
			SongCapo = new System.Windows.Forms.ComboBox();
			label11 = new System.Windows.Forms.Label();
			label12 = new System.Windows.Forms.Label();
			label8 = new System.Windows.Forms.Label();
			label7 = new System.Windows.Forms.Label();
			toolTip1 = new System.Windows.Forms.ToolTip(components);
			TimerFast = new System.Windows.Forms.Timer(components);
			TimerAttemptConnect = new System.Windows.Forms.Timer(components);
			TimerTrack = new System.Windows.Forms.Timer(components);
			saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
			toolStrip1.SuspendLayout();
			statusStrip1.SuspendLayout();
			splitContainer1.Panel1.SuspendLayout();
			splitContainer1.Panel2.SuspendLayout();
			splitContainer1.SuspendLayout();
			CMRegion1.SuspendLayout();
			panelR1Top.SuspendLayout();
			panelR1Left.SuspendLayout();
			panel3.SuspendLayout();
			panelR1LeftBottom.SuspendLayout();
			panelR1LeftMiddle.SuspendLayout();
			panelR1LeftTop.SuspendLayout();
			tabRightPane.SuspendLayout();
			tabRight_Region2.SuspendLayout();
			panelR2All.SuspendLayout();
			CMRegion2.SuspendLayout();
			panelR2Top.SuspendLayout();
			panelR2Left.SuspendLayout();
			panel1.SuspendLayout();
			panelR2LeftBottom.SuspendLayout();
			panelR2LeftMiddle.SuspendLayout();
			panelR2LeftTop.SuspendLayout();
			tabRight_Rotate.SuspendLayout();
			panelRotate.SuspendLayout();
			splitContainerRotate.Panel1.SuspendLayout();
			splitContainerRotate.Panel2.SuspendLayout();
			splitContainerRotate.SuspendLayout();
			groupBoxRotateVerses.SuspendLayout();
			panelRotate_Verses.SuspendLayout();
			panel10.SuspendLayout();
			panelRotate_OrderList.SuspendLayout();
			panel12.SuspendLayout();
			panel13.SuspendLayout();
			toolStripRotate_SeqSet.SuspendLayout();
			panel14.SuspendLayout();
			toolStripRotate_SeqUpDown.SuspendLayout();
			panelRotate_Sample.SuspendLayout();
			panelRotate_Media.SuspendLayout();
			panelNoPlayer.SuspendLayout();
			panelLoc.SuspendLayout();
			toolStrip3.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)TrackBarVolume).BeginInit();
			panelPlayBtns.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)TrackBarDuration).BeginInit();
			panelRotateLeft.SuspendLayout();
			panelRotateLeftTop2.SuspendLayout();
			panelRotateLeftTop1.SuspendLayout();
			groupBox3.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)Rotate_SlidesGapUpDown).BeginInit();
			menuStripMain.SuspendLayout();
			splitContainerMain.Panel1.SuspendLayout();
			splitContainerMain.Panel2.SuspendLayout();
			splitContainerMain.SuspendLayout();
			groupBox2.SuspendLayout();
			panelVerses.SuspendLayout();
			panel2.SuspendLayout();
			panelOrderList.SuspendLayout();
			panel4.SuspendLayout();
			panelSeqSet.SuspendLayout();
			toolStripSeqSet.SuspendLayout();
			panelSeqUpDown.SuspendLayout();
			toolStripSeqUpDown.SuspendLayout();
			groupBox1.SuspendLayout();
			panel7.SuspendLayout();
			panelLinkTitle2Lookup.SuspendLayout();
			toolStrip2.SuspendLayout();
			panel8.SuspendLayout();
			SuspendLayout();
			toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[14]
			{
				Main_New,
				Main_Save,
				Main_SaveExit,
				toolStripSeparator1,
				Main_Import,
				Main_WordWrap,
				Main_ChordsMenu,
				toolStripSeparator2,
				Main_TransposeDown,
				Main_TransposeUp,
				toolStripSeparator4,
				ComboFontName,
				ComboMainFontSize,
				ComboNotationFontSize
			});
			toolStrip1.Location = new System.Drawing.Point(0, 24);
			toolStrip1.Name = "toolStrip1";
			toolStrip1.Size = new System.Drawing.Size(775, 25);
			toolStrip1.TabIndex = 0;
			toolStrip1.Text = "toolStrip1";
			Main_New.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			Main_New.Image = Resources.New;
			Main_New.ImageTransparentColor = System.Drawing.Color.Magenta;
			Main_New.Name = "Main_New";
			Main_New.Size = new System.Drawing.Size(23, 22);
			Main_New.ToolTipText = "New Item";
			Main_New.Click += new System.EventHandler(Main_New_Click);
			Main_Save.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			Main_Save.Image = Resources.Save;
			Main_Save.ImageTransparentColor = System.Drawing.Color.Magenta;
			Main_Save.Name = "Main_Save";
			Main_Save.Size = new System.Drawing.Size(23, 22);
			Main_Save.ToolTipText = "Save Item";
			Main_Save.Click += new System.EventHandler(Main_Save_Click);
			Main_SaveExit.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			Main_SaveExit.Image = Resources.SaveClose;
			Main_SaveExit.ImageTransparentColor = System.Drawing.Color.Magenta;
			Main_SaveExit.Name = "Main_SaveExit";
			Main_SaveExit.Size = new System.Drawing.Size(23, 22);
			Main_SaveExit.ToolTipText = "Save and Exit";
			Main_SaveExit.Click += new System.EventHandler(Main_SaveExit_Click);
			toolStripSeparator1.Name = "toolStripSeparator1";
			toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
			Main_Import.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			Main_Import.Image = Resources.open;
			Main_Import.ImageTransparentColor = System.Drawing.Color.Magenta;
			Main_Import.Name = "Main_Import";
			Main_Import.Size = new System.Drawing.Size(23, 22);
			Main_Import.Text = "toolStripButton5";
			Main_Import.ToolTipText = "Import Word/Text File";
			Main_Import.Click += new System.EventHandler(Main_Import_Click);
			Main_WordWrap.CheckOnClick = true;
			Main_WordWrap.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			Main_WordWrap.Image = Resources.wordwrap;
			Main_WordWrap.ImageTransparentColor = System.Drawing.Color.Magenta;
			Main_WordWrap.Name = "Main_WordWrap";
			Main_WordWrap.Size = new System.Drawing.Size(23, 22);
			Main_WordWrap.ToolTipText = "Word Wrap";
			Main_WordWrap.Click += new System.EventHandler(Main_WordWrap_Click);
			Main_ChordsMenu.CheckOnClick = true;
			Main_ChordsMenu.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			Main_ChordsMenu.Image = Resources.PopUpChords;
			Main_ChordsMenu.ImageTransparentColor = System.Drawing.Color.Magenta;
			Main_ChordsMenu.Name = "Main_ChordsMenu";
			Main_ChordsMenu.Size = new System.Drawing.Size(23, 22);
			Main_ChordsMenu.ToolTipText = "Right Click Chords Menu";
			Main_ChordsMenu.Click += new System.EventHandler(Main_ChordsMenu_Click);
			toolStripSeparator2.Name = "toolStripSeparator2";
			toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
			Main_TransposeDown.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			Main_TransposeDown.Image = Resources.arrowGL;
			Main_TransposeDown.ImageTransparentColor = System.Drawing.Color.Magenta;
			Main_TransposeDown.Name = "Main_TransposeDown";
			Main_TransposeDown.Size = new System.Drawing.Size(23, 22);
			Main_TransposeDown.ToolTipText = "Transpose Chord Down";
			Main_TransposeDown.Click += new System.EventHandler(Main_TransposeDown_Click);
			Main_TransposeUp.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			Main_TransposeUp.Image = Resources.arrowGR;
			Main_TransposeUp.ImageTransparentColor = System.Drawing.Color.Magenta;
			Main_TransposeUp.Name = "Main_TransposeUp";
			Main_TransposeUp.Size = new System.Drawing.Size(23, 22);
			Main_TransposeUp.ToolTipText = "Transpose Chord Up";
			Main_TransposeUp.Click += new System.EventHandler(Main_TransposeUp_Click);
			toolStripSeparator4.Name = "toolStripSeparator4";
			toolStripSeparator4.Size = new System.Drawing.Size(6, 25);
			ComboFontName.AutoSize = false;
			ComboFontName.Name = "ComboFontName";
			ComboFontName.Size = new System.Drawing.Size(121, 21);
			ComboFontName.Text = "Font Name";
			ComboFontName.ToolTipText = "Font Name";
			ComboFontName.SelectedIndexChanged += new System.EventHandler(ComboFonts_SelectedIndexChanged);
			ComboMainFontSize.AutoSize = false;
			ComboMainFontSize.Name = "ComboMainFontSize";
			ComboMainFontSize.Size = new System.Drawing.Size(40, 21);
			ComboMainFontSize.Text = "12";
			ComboMainFontSize.ToolTipText = "Font Size";
			ComboMainFontSize.SelectedIndexChanged += new System.EventHandler(ComboFonts_SelectedIndexChanged);
			ComboNotationFontSize.AutoSize = false;
			ComboNotationFontSize.Name = "ComboNotationFontSize";
			ComboNotationFontSize.Size = new System.Drawing.Size(40, 21);
			ComboNotationFontSize.Text = "10";
			ComboNotationFontSize.ToolTipText = "Notation Size";
			ComboNotationFontSize.SelectedIndexChanged += new System.EventHandler(ComboFonts_SelectedIndexChanged);
			statusStrip1.BackColor = System.Drawing.Color.LimeGreen;
			statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[1]
			{
				toolStripStatusLabel1
			});
			statusStrip1.Location = new System.Drawing.Point(0, 539);
			statusStrip1.Name = "statusStrip1";
			statusStrip1.Size = new System.Drawing.Size(775, 22);
			statusStrip1.TabIndex = 2;
			toolStripStatusLabel1.BackColor = System.Drawing.Color.FromArgb(0, 192, 0);
			toolStripStatusLabel1.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
			toolStripStatusLabel1.ForeColor = System.Drawing.Color.Black;
			toolStripStatusLabel1.Name = "toolStripStatusLabel1";
			toolStripStatusLabel1.Size = new System.Drawing.Size(0, 17);
			splitContainer1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			splitContainer1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
			splitContainer1.Location = new System.Drawing.Point(0, 0);
			splitContainer1.Name = "splitContainer1";
			splitContainer1.Panel1.Controls.Add(tbLyrics1);
			splitContainer1.Panel1.Controls.Add(panelR1Top);
			splitContainer1.Panel1.Controls.Add(panelR1Left);
			splitContainer1.Panel2.Controls.Add(tabRightPane);
			splitContainer1.Panel2.Controls.Add(panel6);
			splitContainer1.Panel2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
			splitContainer1.Size = new System.Drawing.Size(775, 355);
			splitContainer1.SplitterDistance = 351;
			splitContainer1.TabIndex = 0;
			splitContainer1.Text = "splitContainer1";
			tbLyrics1.AutoWordSelection = true;
			tbLyrics1.ContextMenuStrip = CMRegion1;
			tbLyrics1.DetectUrls = false;
			tbLyrics1.Dock = System.Windows.Forms.DockStyle.Fill;
			tbLyrics1.EnableAutoDragDrop = true;
			tbLyrics1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
			tbLyrics1.HideSelection = false;
			tbLyrics1.Location = new System.Drawing.Point(55, 16);
			tbLyrics1.Name = "tbLyrics1";
			tbLyrics1.Size = new System.Drawing.Size(292, 335);
			tbLyrics1.TabIndex = 0;
			tbLyrics1.Text = "";
			tbLyrics1.SelectionChanged += new System.EventHandler(tbLyrics_SelectionChanged);
			tbLyrics1.MouseUp += new System.Windows.Forms.MouseEventHandler(tbLyrics1_MouseUp);
			tbLyrics1.KeyDown += new System.Windows.Forms.KeyEventHandler(tbLyrics1_KeyDown);
			tbLyrics1.KeyUp += new System.Windows.Forms.KeyEventHandler(tbLyrics1_KeyUp);
			tbLyrics1.TextChanged += new System.EventHandler(tbLyrics1_TextChanged);
			CMRegion1.Items.AddRange(new System.Windows.Forms.ToolStripItem[2]
			{
				CMRegion1_Copy,
				CMRegion1_Paste
			});
			CMRegion1.Name = "CMRegion1";
			CMRegion1.Size = new System.Drawing.Size(102, 48);
			CMRegion1_Copy.Name = "CMRegion1_Copy";
			CMRegion1_Copy.Size = new System.Drawing.Size(101, 22);
			CMRegion1_Copy.Text = "Copy";
			CMRegion1_Copy.Click += new System.EventHandler(CMRegion1_Copy_Click);
			CMRegion1_Paste.Name = "CMRegion1_Paste";
			CMRegion1_Paste.Size = new System.Drawing.Size(101, 22);
			CMRegion1_Paste.Text = "Paste";
			CMRegion1_Paste.Click += new System.EventHandler(CMRegion1_Paste_Click);
			panelR1Top.Controls.Add(LabeltbLyrics);
			panelR1Top.Dock = System.Windows.Forms.DockStyle.Top;
			panelR1Top.Location = new System.Drawing.Point(55, 0);
			panelR1Top.Name = "panelR1Top";
			panelR1Top.Size = new System.Drawing.Size(292, 16);
			panelR1Top.TabIndex = 0;
			LabeltbLyrics.Location = new System.Drawing.Point(3, 1);
			LabeltbLyrics.Name = "LabeltbLyrics";
			LabeltbLyrics.Size = new System.Drawing.Size(346, 13);
			LabeltbLyrics.TabIndex = 0;
			LabeltbLyrics.Text = "Region 1";
			panelR1Left.Controls.Add(panel3);
			panelR1Left.Dock = System.Windows.Forms.DockStyle.Left;
			panelR1Left.Location = new System.Drawing.Point(0, 0);
			panelR1Left.Name = "panelR1Left";
			panelR1Left.Size = new System.Drawing.Size(55, 351);
			panelR1Left.TabIndex = 1;
			panel3.Controls.Add(panelR1LeftBottom);
			panel3.Controls.Add(panelR1LeftMiddle);
			panel3.Controls.Add(panelR1LeftTop);
			panel3.Location = new System.Drawing.Point(4, 16);
			panel3.Name = "panel3";
			panel3.Size = new System.Drawing.Size(48, 279);
			panel3.TabIndex = 2;
			panelR1LeftBottom.Controls.Add(R1RightToLeft);
			panelR1LeftBottom.Controls.Add(R1Chinese);
			panelR1LeftBottom.Dock = System.Windows.Forms.DockStyle.Top;
			panelR1LeftBottom.Location = new System.Drawing.Point(0, 254);
			panelR1LeftBottom.Name = "panelR1LeftBottom";
			panelR1LeftBottom.Size = new System.Drawing.Size(48, 24);
			panelR1LeftBottom.TabIndex = 2;
			R1RightToLeft.Appearance = System.Windows.Forms.Appearance.Button;
			R1RightToLeft.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			R1RightToLeft.Image = Resources.LangRightLeft;
			R1RightToLeft.Location = new System.Drawing.Point(22, 1);
			R1RightToLeft.Name = "R1RightToLeft";
			R1RightToLeft.Size = new System.Drawing.Size(23, 23);
			R1RightToLeft.TabIndex = 3;
			toolTip1.SetToolTip(R1RightToLeft, "Right-To-Left Text");
			R1RightToLeft.UseVisualStyleBackColor = true;
			R1RightToLeft.Visible = false;
			R1RightToLeft.Click += new System.EventHandler(RightToLeft_Click);
			R1Chinese.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			R1Chinese.Image = Resources.Chinese;
			R1Chinese.Location = new System.Drawing.Point(0, 1);
			R1Chinese.Name = "R1Chinese";
			R1Chinese.Size = new System.Drawing.Size(23, 23);
			R1Chinese.TabIndex = 22;
			R1Chinese.Tag = "";
			toolTip1.SetToolTip(R1Chinese, "Siwtch Trad/Simp Chinese");
			R1Chinese.Click += new System.EventHandler(R1Chinese_Click);
			panelR1LeftMiddle.Controls.Add(R1BtnBridge2);
			panelR1LeftMiddle.Controls.Add(R1BtnPreChorus2);
			panelR1LeftMiddle.Controls.Add(R1BtnChorus2);
			panelR1LeftMiddle.Controls.Add(R1BtnPreChorus);
			panelR1LeftMiddle.Controls.Add(R1BtnChorus);
			panelR1LeftMiddle.Controls.Add(R1VerseFormat);
			panelR1LeftMiddle.Controls.Add(R1BtnNewScreen);
			panelR1LeftMiddle.Controls.Add(R1BtnNotations);
			panelR1LeftMiddle.Controls.Add(R1BtnEnding);
			panelR1LeftMiddle.Controls.Add(R1BtnBridge);
			panelR1LeftMiddle.Controls.Add(R1Btn10);
			panelR1LeftMiddle.Controls.Add(R1Btn9);
			panelR1LeftMiddle.Controls.Add(R1Btn8);
			panelR1LeftMiddle.Controls.Add(R1Btn7);
			panelR1LeftMiddle.Controls.Add(R1Btn6);
			panelR1LeftMiddle.Controls.Add(R1Btn5);
			panelR1LeftMiddle.Controls.Add(R1Btn4);
			panelR1LeftMiddle.Controls.Add(R1Btn3);
			panelR1LeftMiddle.Controls.Add(R1Btn2);
			panelR1LeftMiddle.Controls.Add(R1Btn1);
			panelR1LeftMiddle.Dock = System.Windows.Forms.DockStyle.Top;
			panelR1LeftMiddle.Location = new System.Drawing.Point(0, 24);
			panelR1LeftMiddle.Name = "panelR1LeftMiddle";
			panelR1LeftMiddle.Size = new System.Drawing.Size(48, 230);
			panelR1LeftMiddle.TabIndex = 0;
			R1BtnBridge2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			R1BtnBridge2.Image = Resources.NumBridge2;
			R1BtnBridge2.Location = new System.Drawing.Point(23, 161);
			R1BtnBridge2.Name = "R1BtnBridge2";
			R1BtnBridge2.Size = new System.Drawing.Size(23, 23);
			R1BtnBridge2.TabIndex = 17;
			R1BtnBridge2.Tag = "103";
			toolTip1.SetToolTip(R1BtnBridge2, "Bridge 2 Indicator");
			R1BtnBridge2.Click += new System.EventHandler(R1Btn_Click);
			R1BtnPreChorus2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			R1BtnPreChorus2.Image = Resources.NumPreChorus2;
			R1BtnPreChorus2.Location = new System.Drawing.Point(23, 115);
			R1BtnPreChorus2.Name = "R1BtnPreChorus2";
			R1BtnPreChorus2.Size = new System.Drawing.Size(23, 23);
			R1BtnPreChorus2.TabIndex = 13;
			R1BtnPreChorus2.Tag = "112";
			toolTip1.SetToolTip(R1BtnPreChorus2, "Prechorus2 Indicator");
			R1BtnPreChorus2.Click += new System.EventHandler(R1Btn_Click);
			R1BtnChorus2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			R1BtnChorus2.Image = Resources.NumChorus2;
			R1BtnChorus2.Location = new System.Drawing.Point(23, 138);
			R1BtnChorus2.Name = "R1BtnChorus2";
			R1BtnChorus2.Size = new System.Drawing.Size(23, 23);
			R1BtnChorus2.TabIndex = 15;
			R1BtnChorus2.Tag = "102";
			toolTip1.SetToolTip(R1BtnChorus2, "Chrous 2 Indicator");
			R1BtnChorus2.Click += new System.EventHandler(R1Btn_Click);
			R1BtnPreChorus.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			R1BtnPreChorus.Image = Resources.NumPreChorus;
			R1BtnPreChorus.Location = new System.Drawing.Point(0, 115);
			R1BtnPreChorus.Name = "R1BtnPreChorus";
			R1BtnPreChorus.Size = new System.Drawing.Size(23, 23);
			R1BtnPreChorus.TabIndex = 12;
			R1BtnPreChorus.Tag = "111";
			toolTip1.SetToolTip(R1BtnPreChorus, "Prechorus Indicator");
			R1BtnPreChorus.Click += new System.EventHandler(R1Btn_Click);
			R1BtnChorus.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			R1BtnChorus.Image = Resources.NumChorus;
			R1BtnChorus.Location = new System.Drawing.Point(0, 138);
			R1BtnChorus.Name = "R1BtnChorus";
			R1BtnChorus.Size = new System.Drawing.Size(23, 23);
			R1BtnChorus.TabIndex = 14;
			R1BtnChorus.Tag = "0";
			toolTip1.SetToolTip(R1BtnChorus, "Chorus Indicator");
			R1BtnChorus.Click += new System.EventHandler(R1Btn_Click);
			R1VerseFormat.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			R1VerseFormat.Image = Resources.VerseFormat;
			R1VerseFormat.Location = new System.Drawing.Point(23, 207);
			R1VerseFormat.Name = "R1VerseFormat";
			R1VerseFormat.Size = new System.Drawing.Size(23, 23);
			R1VerseFormat.TabIndex = 21;
			R1VerseFormat.Tag = "";
			toolTip1.SetToolTip(R1VerseFormat, "Verses Format");
			R1VerseFormat.Click += new System.EventHandler(R1VerseFormat_Click);
			R1BtnNewScreen.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			R1BtnNewScreen.Image = Resources.NumNewScreen;
			R1BtnNewScreen.Location = new System.Drawing.Point(23, 184);
			R1BtnNewScreen.Name = "R1BtnNewScreen";
			R1BtnNewScreen.Size = new System.Drawing.Size(23, 23);
			R1BtnNewScreen.TabIndex = 19;
			R1BtnNewScreen.Tag = "151";
			toolTip1.SetToolTip(R1BtnNewScreen, "New Screen Indicator");
			R1BtnNewScreen.Click += new System.EventHandler(R1Btn_Click);
			R1BtnNotations.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			R1BtnNotations.Image = Resources.NotationSym;
			R1BtnNotations.Location = new System.Drawing.Point(0, 207);
			R1BtnNotations.Name = "R1BtnNotations";
			R1BtnNotations.Size = new System.Drawing.Size(23, 23);
			R1BtnNotations.TabIndex = 20;
			R1BtnNotations.Tag = "152";
			toolTip1.SetToolTip(R1BtnNotations, "Notations Indicator (F8)");
			R1BtnNotations.Click += new System.EventHandler(R1Btn_Click);
			R1BtnEnding.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			R1BtnEnding.Image = Resources.NumEnding;
			R1BtnEnding.Location = new System.Drawing.Point(0, 184);
			R1BtnEnding.Name = "R1BtnEnding";
			R1BtnEnding.Size = new System.Drawing.Size(23, 23);
			R1BtnEnding.TabIndex = 18;
			R1BtnEnding.Tag = "101";
			toolTip1.SetToolTip(R1BtnEnding, "Ending Indicator");
			R1BtnEnding.Click += new System.EventHandler(R1Btn_Click);
			R1BtnBridge.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			R1BtnBridge.Image = Resources.NumBridge;
			R1BtnBridge.Location = new System.Drawing.Point(0, 161);
			R1BtnBridge.Name = "R1BtnBridge";
			R1BtnBridge.Size = new System.Drawing.Size(23, 23);
			R1BtnBridge.TabIndex = 16;
			R1BtnBridge.Tag = "100";
			toolTip1.SetToolTip(R1BtnBridge, "Bridge Indicator");
			R1BtnBridge.Click += new System.EventHandler(R1Btn_Click);
			R1Btn10.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			R1Btn10.Image = Resources.Num10;
			R1Btn10.Location = new System.Drawing.Point(23, 92);
			R1Btn10.Name = "R1Btn10";
			R1Btn10.Size = new System.Drawing.Size(23, 23);
			R1Btn10.TabIndex = 11;
			R1Btn10.Tag = "10";
			R1Btn10.Click += new System.EventHandler(R1Btn_Click);
			R1Btn9.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			R1Btn9.Image = Resources.Num9;
			R1Btn9.Location = new System.Drawing.Point(0, 92);
			R1Btn9.Name = "R1Btn9";
			R1Btn9.Size = new System.Drawing.Size(23, 23);
			R1Btn9.TabIndex = 10;
			R1Btn9.Tag = "9";
			R1Btn9.Click += new System.EventHandler(R1Btn_Click);
			R1Btn8.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			R1Btn8.Image = Resources.Num8;
			R1Btn8.Location = new System.Drawing.Point(23, 69);
			R1Btn8.Name = "R1Btn8";
			R1Btn8.Size = new System.Drawing.Size(23, 23);
			R1Btn8.TabIndex = 9;
			R1Btn8.Tag = "8";
			R1Btn8.Click += new System.EventHandler(R1Btn_Click);
			R1Btn7.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			R1Btn7.Image = Resources.Num7;
			R1Btn7.Location = new System.Drawing.Point(0, 69);
			R1Btn7.Name = "R1Btn7";
			R1Btn7.Size = new System.Drawing.Size(23, 23);
			R1Btn7.TabIndex = 8;
			R1Btn7.Tag = "7";
			R1Btn7.Click += new System.EventHandler(R1Btn_Click);
			R1Btn6.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			R1Btn6.Image = Resources.Num6;
			R1Btn6.Location = new System.Drawing.Point(23, 46);
			R1Btn6.Name = "R1Btn6";
			R1Btn6.Size = new System.Drawing.Size(23, 23);
			R1Btn6.TabIndex = 7;
			R1Btn6.Tag = "6";
			R1Btn6.Click += new System.EventHandler(R1Btn_Click);
			R1Btn5.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			R1Btn5.Image = Resources.Num5;
			R1Btn5.Location = new System.Drawing.Point(0, 46);
			R1Btn5.Name = "R1Btn5";
			R1Btn5.Size = new System.Drawing.Size(23, 23);
			R1Btn5.TabIndex = 6;
			R1Btn5.Tag = "5";
			R1Btn5.Click += new System.EventHandler(R1Btn_Click);
			R1Btn4.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			R1Btn4.Image = Resources.Num4;
			R1Btn4.Location = new System.Drawing.Point(23, 23);
			R1Btn4.Name = "R1Btn4";
			R1Btn4.Size = new System.Drawing.Size(23, 23);
			R1Btn4.TabIndex = 5;
			R1Btn4.Tag = "4";
			R1Btn4.Click += new System.EventHandler(R1Btn_Click);
			R1Btn3.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			R1Btn3.Image = Resources.Num3;
			R1Btn3.Location = new System.Drawing.Point(0, 23);
			R1Btn3.Name = "R1Btn3";
			R1Btn3.Size = new System.Drawing.Size(23, 23);
			R1Btn3.TabIndex = 4;
			R1Btn3.Tag = "3";
			R1Btn3.Click += new System.EventHandler(R1Btn_Click);
			R1Btn2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			R1Btn2.Image = Resources.Num2;
			R1Btn2.Location = new System.Drawing.Point(23, 0);
			R1Btn2.Name = "R1Btn2";
			R1Btn2.Size = new System.Drawing.Size(23, 23);
			R1Btn2.TabIndex = 3;
			R1Btn2.Tag = "2";
			R1Btn2.Click += new System.EventHandler(R1Btn_Click);
			R1Btn1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			R1Btn1.Image = Resources.Num1;
			R1Btn1.Location = new System.Drawing.Point(0, 0);
			R1Btn1.Name = "R1Btn1";
			R1Btn1.Size = new System.Drawing.Size(23, 23);
			R1Btn1.TabIndex = 2;
			R1Btn1.Tag = "1";
			toolTip1.SetToolTip(R1Btn1, "Verse Indicator");
			R1Btn1.Click += new System.EventHandler(R1Btn_Click);
			panelR1LeftTop.Controls.Add(R1Undo);
			panelR1LeftTop.Controls.Add(R1Redo);
			panelR1LeftTop.Dock = System.Windows.Forms.DockStyle.Top;
			panelR1LeftTop.Location = new System.Drawing.Point(0, 0);
			panelR1LeftTop.Name = "panelR1LeftTop";
			panelR1LeftTop.Size = new System.Drawing.Size(48, 24);
			panelR1LeftTop.TabIndex = 2;
			R1Undo.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			R1Undo.Image = Resources.undo;
			R1Undo.Location = new System.Drawing.Point(0, 1);
			R1Undo.Name = "R1Undo";
			R1Undo.Size = new System.Drawing.Size(23, 23);
			R1Undo.TabIndex = 0;
			R1Undo.Tag = "0";
			toolTip1.SetToolTip(R1Undo, "Undo");
			R1Undo.Click += new System.EventHandler(R1UndoRedo_Click);
			R1Redo.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			R1Redo.Image = Resources.redo;
			R1Redo.Location = new System.Drawing.Point(23, 1);
			R1Redo.Name = "R1Redo";
			R1Redo.Size = new System.Drawing.Size(23, 23);
			R1Redo.TabIndex = 1;
			R1Redo.Tag = "1";
			toolTip1.SetToolTip(R1Redo, "Redo");
			R1Redo.Click += new System.EventHandler(R1UndoRedo_Click);
			tabRightPane.Controls.Add(tabRight_Region2);
			tabRightPane.Controls.Add(tabRight_Rotate);
			tabRightPane.Dock = System.Windows.Forms.DockStyle.Fill;
			tabRightPane.Location = new System.Drawing.Point(0, 0);
			tabRightPane.Multiline = true;
			tabRightPane.Name = "tabRightPane";
			tabRightPane.SelectedIndex = 0;
			tabRightPane.Size = new System.Drawing.Size(413, 351);
			tabRightPane.TabIndex = 0;
			tabRight_Region2.BackColor = System.Drawing.SystemColors.Control;
			tabRight_Region2.Controls.Add(panelR2All);
			tabRight_Region2.Location = new System.Drawing.Point(4, 22);
			tabRight_Region2.Name = "tabRight_Region2";
			tabRight_Region2.Padding = new System.Windows.Forms.Padding(3);
			tabRight_Region2.Size = new System.Drawing.Size(405, 325);
			tabRight_Region2.TabIndex = 0;
			tabRight_Region2.Text = "Region 2";
			panelR2All.Controls.Add(tbLyrics2);
			panelR2All.Controls.Add(panelR2Top);
			panelR2All.Controls.Add(panelR2Left);
			panelR2All.Dock = System.Windows.Forms.DockStyle.Fill;
			panelR2All.Location = new System.Drawing.Point(3, 3);
			panelR2All.Name = "panelR2All";
			panelR2All.Size = new System.Drawing.Size(399, 319);
			panelR2All.TabIndex = 7;
			tbLyrics2.ContextMenuStrip = CMRegion2;
			tbLyrics2.DetectUrls = false;
			tbLyrics2.Dock = System.Windows.Forms.DockStyle.Fill;
			tbLyrics2.EnableAutoDragDrop = true;
			tbLyrics2.HideSelection = false;
			tbLyrics2.Location = new System.Drawing.Point(72, 16);
			tbLyrics2.Name = "tbLyrics2";
			tbLyrics2.Size = new System.Drawing.Size(327, 303);
			tbLyrics2.TabIndex = 5;
			tbLyrics2.Text = "";
			tbLyrics2.SelectionChanged += new System.EventHandler(tbLyrics2_SelectionChanged);
			tbLyrics2.MouseUp += new System.Windows.Forms.MouseEventHandler(tbLyrics2_MouseUp);
			tbLyrics2.KeyDown += new System.Windows.Forms.KeyEventHandler(tbLyrics2_KeyDown);
			tbLyrics2.KeyUp += new System.Windows.Forms.KeyEventHandler(tbLyrics2_KeyUp);
			tbLyrics2.TextChanged += new System.EventHandler(tbLyrics2_TextChanged);
			CMRegion2.Items.AddRange(new System.Windows.Forms.ToolStripItem[2]
			{
				CMRegion2_Copy,
				CMRegion2_Paste
			});
			CMRegion2.Name = "CMRegion1";
			CMRegion2.Size = new System.Drawing.Size(102, 48);
			CMRegion2_Copy.Name = "CMRegion2_Copy";
			CMRegion2_Copy.Size = new System.Drawing.Size(101, 22);
			CMRegion2_Copy.Text = "Copy";
			CMRegion2_Copy.Click += new System.EventHandler(CMRegion2_Copy_Click);
			CMRegion2_Paste.Name = "CMRegion2_Paste";
			CMRegion2_Paste.Size = new System.Drawing.Size(101, 22);
			CMRegion2_Paste.Text = "Paste";
			CMRegion2_Paste.Click += new System.EventHandler(CMRegion2_Paste_Click);
			panelR2Top.Controls.Add(LabeltbLyrics2);
			panelR2Top.Dock = System.Windows.Forms.DockStyle.Top;
			panelR2Top.Location = new System.Drawing.Point(72, 0);
			panelR2Top.Name = "panelR2Top";
			panelR2Top.Size = new System.Drawing.Size(327, 16);
			panelR2Top.TabIndex = 1;
			LabeltbLyrics2.Location = new System.Drawing.Point(-1, 1);
			LabeltbLyrics2.Name = "LabeltbLyrics2";
			LabeltbLyrics2.Size = new System.Drawing.Size(377, 13);
			LabeltbLyrics2.TabIndex = 1;
			LabeltbLyrics2.Text = "Region 2";
			panelR2Left.Controls.Add(panel1);
			panelR2Left.Controls.Add(SyncBtnDown);
			panelR2Left.Controls.Add(SyncBtnUp);
			panelR2Left.Dock = System.Windows.Forms.DockStyle.Left;
			panelR2Left.Location = new System.Drawing.Point(0, 0);
			panelR2Left.Name = "panelR2Left";
			panelR2Left.Size = new System.Drawing.Size(72, 319);
			panelR2Left.TabIndex = 0;
			panel1.Controls.Add(panelR2LeftBottom);
			panel1.Controls.Add(panelR2LeftMiddle);
			panel1.Controls.Add(panelR2LeftTop);
			panel1.Location = new System.Drawing.Point(21, 15);
			panel1.Name = "panel1";
			panel1.Size = new System.Drawing.Size(48, 280);
			panel1.TabIndex = 2;
			panelR2LeftBottom.Controls.Add(R2RightToLeft);
			panelR2LeftBottom.Controls.Add(R2Chinese);
			panelR2LeftBottom.Dock = System.Windows.Forms.DockStyle.Top;
			panelR2LeftBottom.Location = new System.Drawing.Point(0, 254);
			panelR2LeftBottom.Name = "panelR2LeftBottom";
			panelR2LeftBottom.Size = new System.Drawing.Size(48, 24);
			panelR2LeftBottom.TabIndex = 3;
			R2RightToLeft.Appearance = System.Windows.Forms.Appearance.Button;
			R2RightToLeft.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			R2RightToLeft.Image = Resources.LangRightLeft;
			R2RightToLeft.Location = new System.Drawing.Point(23, 0);
			R2RightToLeft.Name = "R2RightToLeft";
			R2RightToLeft.Size = new System.Drawing.Size(23, 23);
			R2RightToLeft.TabIndex = 24;
			toolTip1.SetToolTip(R2RightToLeft, "Right-To-Left Text");
			R2RightToLeft.UseVisualStyleBackColor = true;
			R2RightToLeft.Visible = false;
			R2RightToLeft.Click += new System.EventHandler(RightToLeft_Click);
			R2Chinese.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			R2Chinese.Image = Resources.Chinese;
			R2Chinese.Location = new System.Drawing.Point(0, 0);
			R2Chinese.Name = "R2Chinese";
			R2Chinese.Size = new System.Drawing.Size(23, 23);
			R2Chinese.TabIndex = 22;
			R2Chinese.Tag = "";
			toolTip1.SetToolTip(R2Chinese, "Siwtch Trad/Simp Chinese");
			R2Chinese.Click += new System.EventHandler(R2Chinese_Click);
			panelR2LeftMiddle.Controls.Add(R2BtnBridge2);
			panelR2LeftMiddle.Controls.Add(R2BtnPreChorus2);
			panelR2LeftMiddle.Controls.Add(R2BtnPreChorus);
			panelR2LeftMiddle.Controls.Add(R2VerseFormat);
			panelR2LeftMiddle.Controls.Add(R2BtnChorus);
			panelR2LeftMiddle.Controls.Add(R2BtnChorus2);
			panelR2LeftMiddle.Controls.Add(R2BtnNewScreen);
			panelR2LeftMiddle.Controls.Add(R2BtnNotations);
			panelR2LeftMiddle.Controls.Add(R2BtnEnding);
			panelR2LeftMiddle.Controls.Add(R2BtnBridge);
			panelR2LeftMiddle.Controls.Add(R2Btn10);
			panelR2LeftMiddle.Controls.Add(R2Btn9);
			panelR2LeftMiddle.Controls.Add(R2Btn8);
			panelR2LeftMiddle.Controls.Add(R2Btn7);
			panelR2LeftMiddle.Controls.Add(R2Btn6);
			panelR2LeftMiddle.Controls.Add(R2Btn5);
			panelR2LeftMiddle.Controls.Add(R2Btn4);
			panelR2LeftMiddle.Controls.Add(R2Btn3);
			panelR2LeftMiddle.Controls.Add(R2Btn2);
			panelR2LeftMiddle.Controls.Add(R2Btn1);
			panelR2LeftMiddle.Dock = System.Windows.Forms.DockStyle.Top;
			panelR2LeftMiddle.Location = new System.Drawing.Point(0, 24);
			panelR2LeftMiddle.Name = "panelR2LeftMiddle";
			panelR2LeftMiddle.Size = new System.Drawing.Size(48, 230);
			panelR2LeftMiddle.TabIndex = 1;
			R2BtnBridge2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			R2BtnBridge2.Image = Resources.NumBridge2;
			R2BtnBridge2.Location = new System.Drawing.Point(23, 161);
			R2BtnBridge2.Name = "R2BtnBridge2";
			R2BtnBridge2.Size = new System.Drawing.Size(23, 23);
			R2BtnBridge2.TabIndex = 17;
			R2BtnBridge2.Tag = "103";
			toolTip1.SetToolTip(R2BtnBridge2, "Bridge 2 Indicator");
			R2BtnBridge2.Click += new System.EventHandler(R2Btn_Click);
			R2BtnPreChorus2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			R2BtnPreChorus2.Image = Resources.NumPreChorus2;
			R2BtnPreChorus2.Location = new System.Drawing.Point(23, 115);
			R2BtnPreChorus2.Name = "R2BtnPreChorus2";
			R2BtnPreChorus2.Size = new System.Drawing.Size(23, 23);
			R2BtnPreChorus2.TabIndex = 13;
			R2BtnPreChorus2.Tag = "112";
			toolTip1.SetToolTip(R2BtnPreChorus2, "Prechorus2 Indicator");
			R2BtnPreChorus2.Click += new System.EventHandler(R2Btn_Click);
			R2BtnPreChorus.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			R2BtnPreChorus.Image = Resources.NumPreChorus;
			R2BtnPreChorus.Location = new System.Drawing.Point(0, 115);
			R2BtnPreChorus.Name = "R2BtnPreChorus";
			R2BtnPreChorus.Size = new System.Drawing.Size(23, 23);
			R2BtnPreChorus.TabIndex = 12;
			R2BtnPreChorus.Tag = "111";
			toolTip1.SetToolTip(R2BtnPreChorus, "Prechorus Indicator");
			R2BtnPreChorus.Click += new System.EventHandler(R2Btn_Click);
			R2VerseFormat.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			R2VerseFormat.Image = Resources.VerseFormat;
			R2VerseFormat.Location = new System.Drawing.Point(23, 207);
			R2VerseFormat.Name = "R2VerseFormat";
			R2VerseFormat.Size = new System.Drawing.Size(23, 23);
			R2VerseFormat.TabIndex = 21;
			R2VerseFormat.Tag = "";
			toolTip1.SetToolTip(R2VerseFormat, "Verses Format");
			R2VerseFormat.Click += new System.EventHandler(R2VerseFormat_Click);
			R2BtnChorus.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			R2BtnChorus.Image = Resources.NumChorus;
			R2BtnChorus.Location = new System.Drawing.Point(0, 138);
			R2BtnChorus.Name = "R2BtnChorus";
			R2BtnChorus.Size = new System.Drawing.Size(23, 23);
			R2BtnChorus.TabIndex = 14;
			R2BtnChorus.Tag = "0";
			toolTip1.SetToolTip(R2BtnChorus, "Chorus Indicator");
			R2BtnChorus.Click += new System.EventHandler(R2Btn_Click);
			R2BtnChorus2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			R2BtnChorus2.Image = Resources.NumChorus2;
			R2BtnChorus2.Location = new System.Drawing.Point(23, 138);
			R2BtnChorus2.Name = "R2BtnChorus2";
			R2BtnChorus2.Size = new System.Drawing.Size(23, 23);
			R2BtnChorus2.TabIndex = 15;
			R2BtnChorus2.Tag = "102";
			toolTip1.SetToolTip(R2BtnChorus2, "Chrous 2 Indicator");
			R2BtnChorus2.Click += new System.EventHandler(R2Btn_Click);
			R2BtnNewScreen.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			R2BtnNewScreen.Image = Resources.NumNewScreen;
			R2BtnNewScreen.Location = new System.Drawing.Point(23, 184);
			R2BtnNewScreen.Name = "R2BtnNewScreen";
			R2BtnNewScreen.Size = new System.Drawing.Size(23, 23);
			R2BtnNewScreen.TabIndex = 19;
			R2BtnNewScreen.Tag = "151";
			toolTip1.SetToolTip(R2BtnNewScreen, "New Screen Indicator");
			R2BtnNewScreen.Click += new System.EventHandler(R2Btn_Click);
			R2BtnNotations.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			R2BtnNotations.Image = Resources.NotationSym;
			R2BtnNotations.Location = new System.Drawing.Point(0, 207);
			R2BtnNotations.Name = "R2BtnNotations";
			R2BtnNotations.Size = new System.Drawing.Size(23, 23);
			R2BtnNotations.TabIndex = 20;
			R2BtnNotations.Tag = "152";
			toolTip1.SetToolTip(R2BtnNotations, "Notations Indicator (F8)");
			R2BtnNotations.Click += new System.EventHandler(R2Btn_Click);
			R2BtnEnding.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			R2BtnEnding.Image = Resources.NumEnding;
			R2BtnEnding.Location = new System.Drawing.Point(0, 184);
			R2BtnEnding.Name = "R2BtnEnding";
			R2BtnEnding.Size = new System.Drawing.Size(23, 23);
			R2BtnEnding.TabIndex = 18;
			R2BtnEnding.Tag = "101";
			toolTip1.SetToolTip(R2BtnEnding, "Ending Indicator");
			R2BtnEnding.Click += new System.EventHandler(R2Btn_Click);
			R2BtnBridge.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			R2BtnBridge.Image = Resources.NumBridge;
			R2BtnBridge.Location = new System.Drawing.Point(0, 161);
			R2BtnBridge.Name = "R2BtnBridge";
			R2BtnBridge.Size = new System.Drawing.Size(23, 23);
			R2BtnBridge.TabIndex = 16;
			R2BtnBridge.Tag = "100";
			toolTip1.SetToolTip(R2BtnBridge, "Bridge Indicator");
			R2BtnBridge.Click += new System.EventHandler(R2Btn_Click);
			R2Btn10.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			R2Btn10.Image = Resources.Num10;
			R2Btn10.Location = new System.Drawing.Point(23, 92);
			R2Btn10.Name = "R2Btn10";
			R2Btn10.Size = new System.Drawing.Size(23, 23);
			R2Btn10.TabIndex = 11;
			R2Btn10.Tag = "10";
			R2Btn10.Click += new System.EventHandler(R2Btn_Click);
			R2Btn9.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			R2Btn9.Image = Resources.Num9;
			R2Btn9.Location = new System.Drawing.Point(0, 92);
			R2Btn9.Name = "R2Btn9";
			R2Btn9.Size = new System.Drawing.Size(23, 23);
			R2Btn9.TabIndex = 10;
			R2Btn9.Tag = "9";
			R2Btn9.Click += new System.EventHandler(R2Btn_Click);
			R2Btn8.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			R2Btn8.Image = Resources.Num8;
			R2Btn8.Location = new System.Drawing.Point(23, 69);
			R2Btn8.Name = "R2Btn8";
			R2Btn8.Size = new System.Drawing.Size(23, 23);
			R2Btn8.TabIndex = 9;
			R2Btn8.Tag = "8";
			R2Btn8.Click += new System.EventHandler(R2Btn_Click);
			R2Btn7.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			R2Btn7.Image = Resources.Num7;
			R2Btn7.Location = new System.Drawing.Point(0, 69);
			R2Btn7.Name = "R2Btn7";
			R2Btn7.Size = new System.Drawing.Size(23, 23);
			R2Btn7.TabIndex = 8;
			R2Btn7.Tag = "7";
			R2Btn7.Click += new System.EventHandler(R2Btn_Click);
			R2Btn6.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			R2Btn6.Image = Resources.Num6;
			R2Btn6.Location = new System.Drawing.Point(23, 46);
			R2Btn6.Name = "R2Btn6";
			R2Btn6.Size = new System.Drawing.Size(23, 23);
			R2Btn6.TabIndex = 7;
			R2Btn6.Tag = "6";
			R2Btn6.Click += new System.EventHandler(R2Btn_Click);
			R2Btn5.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			R2Btn5.Image = Resources.Num5;
			R2Btn5.Location = new System.Drawing.Point(0, 46);
			R2Btn5.Name = "R2Btn5";
			R2Btn5.Size = new System.Drawing.Size(23, 23);
			R2Btn5.TabIndex = 6;
			R2Btn5.Tag = "5";
			R2Btn5.Click += new System.EventHandler(R2Btn_Click);
			R2Btn4.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			R2Btn4.Image = Resources.Num4;
			R2Btn4.Location = new System.Drawing.Point(23, 23);
			R2Btn4.Name = "R2Btn4";
			R2Btn4.Size = new System.Drawing.Size(23, 23);
			R2Btn4.TabIndex = 5;
			R2Btn4.Tag = "4";
			R2Btn4.Click += new System.EventHandler(R2Btn_Click);
			R2Btn3.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			R2Btn3.Image = Resources.Num3;
			R2Btn3.Location = new System.Drawing.Point(0, 23);
			R2Btn3.Name = "R2Btn3";
			R2Btn3.Size = new System.Drawing.Size(23, 23);
			R2Btn3.TabIndex = 4;
			R2Btn3.Tag = "3";
			R2Btn3.Click += new System.EventHandler(R2Btn_Click);
			R2Btn2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			R2Btn2.Image = Resources.Num2;
			R2Btn2.Location = new System.Drawing.Point(23, 0);
			R2Btn2.Name = "R2Btn2";
			R2Btn2.Size = new System.Drawing.Size(23, 23);
			R2Btn2.TabIndex = 3;
			R2Btn2.Tag = "2";
			R2Btn2.Click += new System.EventHandler(R2Btn_Click);
			R2Btn1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			R2Btn1.Image = Resources.Num1;
			R2Btn1.Location = new System.Drawing.Point(0, 0);
			R2Btn1.Name = "R2Btn1";
			R2Btn1.Size = new System.Drawing.Size(23, 23);
			R2Btn1.TabIndex = 2;
			R2Btn1.Tag = "1";
			toolTip1.SetToolTip(R2Btn1, "Verse Indicator");
			R2Btn1.Click += new System.EventHandler(R2Btn_Click);
			panelR2LeftTop.Controls.Add(R2Undo);
			panelR2LeftTop.Controls.Add(R2Redo);
			panelR2LeftTop.Dock = System.Windows.Forms.DockStyle.Top;
			panelR2LeftTop.Location = new System.Drawing.Point(0, 0);
			panelR2LeftTop.Name = "panelR2LeftTop";
			panelR2LeftTop.Size = new System.Drawing.Size(48, 24);
			panelR2LeftTop.TabIndex = 2;
			R2Undo.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			R2Undo.Image = Resources.undo;
			R2Undo.Location = new System.Drawing.Point(0, 1);
			R2Undo.Name = "R2Undo";
			R2Undo.Size = new System.Drawing.Size(23, 23);
			R2Undo.TabIndex = 0;
			R2Undo.Tag = "0";
			toolTip1.SetToolTip(R2Undo, "Undo");
			R2Undo.Click += new System.EventHandler(R2UndoRedo_Click);
			R2Redo.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			R2Redo.Image = Resources.redo;
			R2Redo.Location = new System.Drawing.Point(23, 1);
			R2Redo.Name = "R2Redo";
			R2Redo.Size = new System.Drawing.Size(23, 23);
			R2Redo.TabIndex = 1;
			R2Redo.Tag = "1";
			toolTip1.SetToolTip(R2Redo, "Redo");
			R2Redo.Click += new System.EventHandler(R2UndoRedo_Click);
			SyncBtnDown.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			SyncBtnDown.Image = Resources.green_down;
			SyncBtnDown.Location = new System.Drawing.Point(0, 40);
			SyncBtnDown.Name = "SyncBtnDown";
			SyncBtnDown.Size = new System.Drawing.Size(20, 22);
			SyncBtnDown.TabIndex = 1;
			SyncBtnDown.Tag = "1";
			toolTip1.SetToolTip(SyncBtnDown, "Highlight Next Slide");
			SyncBtnDown.Click += new System.EventHandler(SyncBtnUpDown_Click);
			SyncBtnUp.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			SyncBtnUp.Image = Resources.green_up;
			SyncBtnUp.Location = new System.Drawing.Point(0, 18);
			SyncBtnUp.Name = "SyncBtnUp";
			SyncBtnUp.Size = new System.Drawing.Size(20, 22);
			SyncBtnUp.TabIndex = 0;
			SyncBtnUp.Tag = "0";
			toolTip1.SetToolTip(SyncBtnUp, "Highlight Previous Slide");
			SyncBtnUp.Click += new System.EventHandler(SyncBtnUpDown_Click);
			tabRight_Rotate.BackColor = System.Drawing.SystemColors.Control;
			tabRight_Rotate.Controls.Add(panelRotate);
			tabRight_Rotate.Location = new System.Drawing.Point(4, 22);
			tabRight_Rotate.Name = "tabRight_Rotate";
			tabRight_Rotate.Padding = new System.Windows.Forms.Padding(3);
			tabRight_Rotate.Size = new System.Drawing.Size(405, 325);
			tabRight_Rotate.TabIndex = 1;
			tabRight_Rotate.Text = "Rotate Style";
			panelRotate.Controls.Add(splitContainerRotate);
			panelRotate.Controls.Add(panelRotateLeft);
			panelRotate.Dock = System.Windows.Forms.DockStyle.Fill;
			panelRotate.Location = new System.Drawing.Point(3, 3);
			panelRotate.Name = "panelRotate";
			panelRotate.Size = new System.Drawing.Size(399, 319);
			panelRotate.TabIndex = 0;
			splitContainerRotate.Dock = System.Windows.Forms.DockStyle.Fill;
			splitContainerRotate.Location = new System.Drawing.Point(123, 0);
			splitContainerRotate.Name = "splitContainerRotate";
			splitContainerRotate.Orientation = System.Windows.Forms.Orientation.Horizontal;
			splitContainerRotate.Panel1.Controls.Add(groupBoxRotateVerses);
			splitContainerRotate.Panel2.Controls.Add(panelRotate_Sample);
			splitContainerRotate.Size = new System.Drawing.Size(276, 319);
			splitContainerRotate.SplitterDistance = 108;
			splitContainerRotate.TabIndex = 33;
			splitContainerRotate.Text = "splitContainer2";
			splitContainerRotate.SplitterMoved += new System.Windows.Forms.SplitterEventHandler(splitContainerRotate_SplitterMoved);
			groupBoxRotateVerses.Controls.Add(panelRotate_Verses);
			groupBoxRotateVerses.Controls.Add(panelRotate_OrderList);
			groupBoxRotateVerses.Controls.Add(panel13);
			groupBoxRotateVerses.Controls.Add(panel14);
			groupBoxRotateVerses.Dock = System.Windows.Forms.DockStyle.Left;
			groupBoxRotateVerses.Enabled = false;
			groupBoxRotateVerses.Location = new System.Drawing.Point(0, 0);
			groupBoxRotateVerses.Name = "groupBoxRotateVerses";
			groupBoxRotateVerses.Padding = new System.Windows.Forms.Padding(0);
			groupBoxRotateVerses.Size = new System.Drawing.Size(240, 108);
			groupBoxRotateVerses.TabIndex = 33;
			groupBoxRotateVerses.TabStop = false;
			panelRotate_Verses.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			panelRotate_Verses.Controls.Add(Rotate_VersesList);
			panelRotate_Verses.Controls.Add(panel10);
			panelRotate_Verses.Location = new System.Drawing.Point(6, 10);
			panelRotate_Verses.Name = "panelRotate_Verses";
			panelRotate_Verses.Size = new System.Drawing.Size(90, 97);
			panelRotate_Verses.TabIndex = 1;
			Rotate_VersesList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[3]
			{
				columnHeader6,
				columnHeader7,
				columnHeader8
			});
			Rotate_VersesList.Dock = System.Windows.Forms.DockStyle.Fill;
			Rotate_VersesList.FullRowSelect = true;
			Rotate_VersesList.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
			Rotate_VersesList.Location = new System.Drawing.Point(0, 14);
			Rotate_VersesList.Margin = new System.Windows.Forms.Padding(1);
			Rotate_VersesList.Name = "Rotate_VersesList";
			Rotate_VersesList.ShowItemToolTips = true;
			Rotate_VersesList.Size = new System.Drawing.Size(86, 79);
			Rotate_VersesList.TabIndex = 0;
			Rotate_VersesList.UseCompatibleStateImageBehavior = false;
			Rotate_VersesList.View = System.Windows.Forms.View.Details;
			Rotate_VersesList.DoubleClick += new System.EventHandler(Rotate_VersesList_DoubleClick);
			columnHeader6.Width = 65;
			columnHeader7.Width = 0;
			columnHeader8.Width = 0;
			panel10.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			panel10.Controls.Add(label23);
			panel10.Dock = System.Windows.Forms.DockStyle.Top;
			panel10.Location = new System.Drawing.Point(0, 0);
			panel10.Name = "panel10";
			panel10.Size = new System.Drawing.Size(86, 14);
			panel10.TabIndex = 0;
			label23.AutoSize = true;
			label23.Location = new System.Drawing.Point(12, -1);
			label23.Name = "label23";
			label23.Size = new System.Drawing.Size(39, 13);
			label23.TabIndex = 0;
			label23.Text = "Verses";
			panelRotate_OrderList.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			panelRotate_OrderList.Controls.Add(Rotate_OrderList);
			panelRotate_OrderList.Controls.Add(panel12);
			panelRotate_OrderList.Location = new System.Drawing.Point(122, 10);
			panelRotate_OrderList.Name = "panelRotate_OrderList";
			panelRotate_OrderList.Size = new System.Drawing.Size(90, 97);
			panelRotate_OrderList.TabIndex = 2;
			Rotate_OrderList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[2]
			{
				columnHeader9,
				columnHeader10
			});
			Rotate_OrderList.Dock = System.Windows.Forms.DockStyle.Fill;
			Rotate_OrderList.FullRowSelect = true;
			Rotate_OrderList.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
			Rotate_OrderList.Location = new System.Drawing.Point(0, 14);
			Rotate_OrderList.Name = "Rotate_OrderList";
			Rotate_OrderList.Size = new System.Drawing.Size(86, 79);
			Rotate_OrderList.TabIndex = 0;
			Rotate_OrderList.UseCompatibleStateImageBehavior = false;
			Rotate_OrderList.View = System.Windows.Forms.View.Details;
			Rotate_OrderList.KeyUp += new System.Windows.Forms.KeyEventHandler(Rotate_OrderList_KeyUp);
			columnHeader9.Width = 65;
			columnHeader10.Width = 0;
			panel12.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			panel12.Controls.Add(label24);
			panel12.Dock = System.Windows.Forms.DockStyle.Top;
			panel12.Location = new System.Drawing.Point(0, 0);
			panel12.Name = "panel12";
			panel12.Size = new System.Drawing.Size(86, 14);
			panel12.TabIndex = 0;
			label24.AutoSize = true;
			label24.Location = new System.Drawing.Point(10, -1);
			label24.Name = "label24";
			label24.Size = new System.Drawing.Size(56, 13);
			label24.TabIndex = 0;
			label24.Text = "Sequence";
			panel13.Controls.Add(toolStripRotate_SeqSet);
			panel13.Location = new System.Drawing.Point(96, 27);
			panel13.Name = "panel13";
			panel13.Size = new System.Drawing.Size(25, 52);
			panel13.TabIndex = 13;
			toolStripRotate_SeqSet.AutoSize = false;
			toolStripRotate_SeqSet.CanOverflow = false;
			toolStripRotate_SeqSet.Dock = System.Windows.Forms.DockStyle.None;
			toolStripRotate_SeqSet.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			toolStripRotate_SeqSet.Items.AddRange(new System.Windows.Forms.ToolStripItem[2]
			{
				Rotate_Verses_Add,
				Rotate_Verses_SmartAdd
			});
			toolStripRotate_SeqSet.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.VerticalStackWithOverflow;
			toolStripRotate_SeqSet.Location = new System.Drawing.Point(0, 1);
			toolStripRotate_SeqSet.Name = "toolStripRotate_SeqSet";
			toolStripRotate_SeqSet.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
			toolStripRotate_SeqSet.Size = new System.Drawing.Size(25, 62);
			toolStripRotate_SeqSet.TabIndex = 5;
			Rotate_Verses_Add.AutoSize = false;
			Rotate_Verses_Add.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			Rotate_Verses_Add.Image = Resources.arrowR;
			Rotate_Verses_Add.ImageTransparentColor = System.Drawing.Color.Magenta;
			Rotate_Verses_Add.Name = "Rotate_Verses_Add";
			Rotate_Verses_Add.Size = new System.Drawing.Size(22, 22);
			Rotate_Verses_Add.Tag = "";
			Rotate_Verses_Add.ToolTipText = "Add";
			Rotate_Verses_Add.Click += new System.EventHandler(Rotate_Verses_Add_Click);
			Rotate_Verses_SmartAdd.AutoSize = false;
			Rotate_Verses_SmartAdd.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			Rotate_Verses_SmartAdd.Image = Resources.multi_arrowr;
			Rotate_Verses_SmartAdd.ImageTransparentColor = System.Drawing.Color.Magenta;
			Rotate_Verses_SmartAdd.Name = "Rotate_Verses_SmartAdd";
			Rotate_Verses_SmartAdd.Size = new System.Drawing.Size(22, 22);
			Rotate_Verses_SmartAdd.Tag = "";
			Rotate_Verses_SmartAdd.ToolTipText = "Smart Add";
			Rotate_Verses_SmartAdd.Click += new System.EventHandler(Rotate_Verses_Add_Click);
			panel14.Controls.Add(toolStripRotate_SeqUpDown);
			panel14.Location = new System.Drawing.Point(211, 28);
			panel14.Name = "panel14";
			panel14.Size = new System.Drawing.Size(25, 79);
			panel14.TabIndex = 12;
			toolStripRotate_SeqUpDown.AutoSize = false;
			toolStripRotate_SeqUpDown.CanOverflow = false;
			toolStripRotate_SeqUpDown.Dock = System.Windows.Forms.DockStyle.None;
			toolStripRotate_SeqUpDown.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			toolStripRotate_SeqUpDown.Items.AddRange(new System.Windows.Forms.ToolStripItem[4]
			{
				Rotate_OrderList_Up,
				Rotate_OrderList_Down,
				toolStripSeparator3,
				Rotate_OrderList_Delete
			});
			toolStripRotate_SeqUpDown.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.VerticalStackWithOverflow;
			toolStripRotate_SeqUpDown.Location = new System.Drawing.Point(0, -1);
			toolStripRotate_SeqUpDown.Name = "toolStripRotate_SeqUpDown";
			toolStripRotate_SeqUpDown.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
			toolStripRotate_SeqUpDown.Size = new System.Drawing.Size(25, 89);
			toolStripRotate_SeqUpDown.TabIndex = 5;
			Rotate_OrderList_Up.AutoSize = false;
			Rotate_OrderList_Up.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			Rotate_OrderList_Up.Image = Resources.handup;
			Rotate_OrderList_Up.ImageTransparentColor = System.Drawing.Color.Magenta;
			Rotate_OrderList_Up.Name = "Rotate_OrderList_Up";
			Rotate_OrderList_Up.Size = new System.Drawing.Size(22, 22);
			Rotate_OrderList_Up.Tag = "up";
			Rotate_OrderList_Up.ToolTipText = "Move Item Up";
			Rotate_OrderList_Up.Click += new System.EventHandler(Rotate_OrderList_Btn_Click);
			Rotate_OrderList_Down.AutoSize = false;
			Rotate_OrderList_Down.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			Rotate_OrderList_Down.Image = Resources.handdown;
			Rotate_OrderList_Down.ImageTransparentColor = System.Drawing.Color.Magenta;
			Rotate_OrderList_Down.Name = "Rotate_OrderList_Down";
			Rotate_OrderList_Down.Size = new System.Drawing.Size(22, 22);
			Rotate_OrderList_Down.Tag = "down";
			Rotate_OrderList_Down.ToolTipText = "Move Item Down";
			Rotate_OrderList_Down.Click += new System.EventHandler(Rotate_OrderList_Btn_Click);
			toolStripSeparator3.Name = "toolStripSeparator3";
			toolStripSeparator3.Size = new System.Drawing.Size(23, 6);
			Rotate_OrderList_Delete.AutoSize = false;
			Rotate_OrderList_Delete.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			Rotate_OrderList_Delete.Image = Resources.Delete;
			Rotate_OrderList_Delete.ImageTransparentColor = System.Drawing.Color.Magenta;
			Rotate_OrderList_Delete.Name = "Rotate_OrderList_Delete";
			Rotate_OrderList_Delete.Size = new System.Drawing.Size(22, 22);
			Rotate_OrderList_Delete.Tag = "delete";
			Rotate_OrderList_Delete.ToolTipText = "Delete";
			Rotate_OrderList_Delete.Click += new System.EventHandler(Rotate_OrderList_Btn_Click);
			panelRotate_Sample.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			panelRotate_Sample.Controls.Add(labelDur);
			panelRotate_Sample.Controls.Add(btnAddPosition);
			panelRotate_Sample.Controls.Add(btnDuration);
			panelRotate_Sample.Controls.Add(LabelMediaType);
			panelRotate_Sample.Controls.Add(labelMed);
			panelRotate_Sample.Controls.Add(panelRotate_Media);
			panelRotate_Sample.Controls.Add(labelPos);
			panelRotate_Sample.Controls.Add(LabelPosition);
			panelRotate_Sample.Controls.Add(panelLoc);
			panelRotate_Sample.Controls.Add(LabelDuration);
			panelRotate_Sample.Controls.Add(Rotate_tbSourceLocation);
			panelRotate_Sample.Controls.Add(TrackBarVolume);
			panelRotate_Sample.Controls.Add(panelPlayBtns);
			panelRotate_Sample.Controls.Add(labelVol);
			panelRotate_Sample.Dock = System.Windows.Forms.DockStyle.Fill;
			panelRotate_Sample.Location = new System.Drawing.Point(0, 0);
			panelRotate_Sample.Name = "panelRotate_Sample";
			panelRotate_Sample.Size = new System.Drawing.Size(276, 207);
			panelRotate_Sample.TabIndex = 31;
			panelRotate_Sample.Resize += new System.EventHandler(panelRotate_Sample_Resize);
			labelDur.AutoSize = true;
			labelDur.Location = new System.Drawing.Point(6, 28);
			labelDur.Name = "labelDur";
			labelDur.Size = new System.Drawing.Size(43, 13);
			labelDur.TabIndex = 68;
			labelDur.Text = "Length:";
			btnAddPosition.Image = Resources.arrowL;
			btnAddPosition.Location = new System.Drawing.Point(7, 118);
			btnAddPosition.Name = "btnAddPosition";
			btnAddPosition.Size = new System.Drawing.Size(41, 22);
			btnAddPosition.TabIndex = 1;
			toolTip1.SetToolTip(btnAddPosition, "Copy current position to next blank timing");
			btnAddPosition.Click += new System.EventHandler(btnAddPosition_Click);
			btnDuration.Image = Resources.arrowBL;
			btnDuration.Location = new System.Drawing.Point(7, 58);
			btnDuration.Name = "btnDuration";
			btnDuration.Size = new System.Drawing.Size(41, 23);
			btnDuration.TabIndex = 0;
			btnDuration.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			toolTip1.SetToolTip(btnDuration, "Click to Store length");
			btnDuration.Click += new System.EventHandler(btnDuration_Click);
			LabelMediaType.AutoSize = true;
			LabelMediaType.ForeColor = System.Drawing.Color.Red;
			LabelMediaType.Location = new System.Drawing.Point(93, 190);
			LabelMediaType.Name = "LabelMediaType";
			LabelMediaType.Size = new System.Drawing.Size(33, 13);
			LabelMediaType.TabIndex = 30;
			LabelMediaType.Text = "None";
			labelMed.AutoSize = true;
			labelMed.Location = new System.Drawing.Point(56, 190);
			labelMed.Name = "labelMed";
			labelMed.Size = new System.Drawing.Size(39, 13);
			labelMed.TabIndex = 29;
			labelMed.Text = "Media:";
			panelRotate_Media.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			panelRotate_Media.Controls.Add(panelNoPlayer);
			panelRotate_Media.Location = new System.Drawing.Point(55, 26);
			panelRotate_Media.Name = "panelRotate_Media";
			panelRotate_Media.Size = new System.Drawing.Size(160, 120);
			panelRotate_Media.TabIndex = 66;
			panelNoPlayer.BackColor = System.Drawing.Color.MidnightBlue;
			panelNoPlayer.Controls.Add(label14);
			panelNoPlayer.Controls.Add(labelNoPlayer1);
			panelNoPlayer.Controls.Add(labelNoPlayer2);
			panelNoPlayer.ForeColor = System.Drawing.Color.White;
			panelNoPlayer.Location = new System.Drawing.Point(0, 0);
			panelNoPlayer.Name = "panelNoPlayer";
			panelNoPlayer.Size = new System.Drawing.Size(156, 116);
			panelNoPlayer.TabIndex = 1;
			panelNoPlayer.Visible = false;
			label14.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
			label14.Location = new System.Drawing.Point(-1, 60);
			label14.Name = "label14";
			label14.Size = new System.Drawing.Size(154, 32);
			label14.TabIndex = 25;
			label14.Text = "to view / listen to Media Backgrounds.";
			label14.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			labelNoPlayer1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
			labelNoPlayer1.Location = new System.Drawing.Point(20, 16);
			labelNoPlayer1.Name = "labelNoPlayer1";
			labelNoPlayer1.Size = new System.Drawing.Size(111, 20);
			labelNoPlayer1.TabIndex = 0;
			labelNoPlayer1.Text = "Please install";
			labelNoPlayer1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			labelNoPlayer2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
			labelNoPlayer2.Location = new System.Drawing.Point(1, 33);
			labelNoPlayer2.Name = "labelNoPlayer2";
			labelNoPlayer2.Size = new System.Drawing.Size(154, 31);
			labelNoPlayer2.TabIndex = 0;
			labelNoPlayer2.Text = "Windows Media Player 10 or DirectX 9";
			labelNoPlayer2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			labelPos.AutoSize = true;
			labelPos.Location = new System.Drawing.Point(6, 87);
			labelPos.Name = "labelPos";
			labelPos.Size = new System.Drawing.Size(47, 13);
			labelPos.TabIndex = 26;
			labelPos.Text = "Position:";
			LabelPosition.AutoSize = true;
			LabelPosition.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
			LabelPosition.ForeColor = System.Drawing.Color.Red;
			LabelPosition.Location = new System.Drawing.Point(6, 102);
			LabelPosition.Name = "LabelPosition";
			LabelPosition.Size = new System.Drawing.Size(32, 13);
			LabelPosition.TabIndex = 28;
			LabelPosition.Text = "0:00";
			panelLoc.Controls.Add(toolStrip3);
			panelLoc.Location = new System.Drawing.Point(176, 1);
			panelLoc.Name = "panelLoc";
			panelLoc.Size = new System.Drawing.Size(23, 23);
			panelLoc.TabIndex = 63;
			toolStrip3.AutoSize = false;
			toolStrip3.CanOverflow = false;
			toolStrip3.Dock = System.Windows.Forms.DockStyle.None;
			toolStrip3.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			toolStrip3.Items.AddRange(new System.Windows.Forms.ToolStripItem[1]
			{
				Rotate_LocationBtn
			});
			toolStrip3.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.Flow;
			toolStrip3.Location = new System.Drawing.Point(1, 0);
			toolStrip3.Name = "toolStrip3";
			toolStrip3.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
			toolStrip3.Size = new System.Drawing.Size(25, 30);
			toolStrip3.TabIndex = 0;
			Rotate_LocationBtn.AutoSize = false;
			Rotate_LocationBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			Rotate_LocationBtn.Image = Resources.folder;
			Rotate_LocationBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
			Rotate_LocationBtn.Name = "Rotate_LocationBtn";
			Rotate_LocationBtn.Size = new System.Drawing.Size(22, 22);
			Rotate_LocationBtn.Tag = "";
			Rotate_LocationBtn.ToolTipText = "Media File Location";
			Rotate_LocationBtn.Click += new System.EventHandler(Rotate_LocationBtn_Click);
			LabelDuration.AutoSize = true;
			LabelDuration.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
			LabelDuration.ForeColor = System.Drawing.SystemColors.MenuHighlight;
			LabelDuration.Location = new System.Drawing.Point(6, 43);
			LabelDuration.Name = "LabelDuration";
			LabelDuration.Size = new System.Drawing.Size(32, 13);
			LabelDuration.TabIndex = 27;
			LabelDuration.Text = "0:00";
			Rotate_tbSourceLocation.Location = new System.Drawing.Point(3, 3);
			Rotate_tbSourceLocation.Name = "Rotate_tbSourceLocation";
			Rotate_tbSourceLocation.Size = new System.Drawing.Size(166, 20);
			Rotate_tbSourceLocation.TabIndex = 0;
			toolTip1.SetToolTip(Rotate_tbSourceLocation, "Optional Media File for Reference (Filename will not be saved)");
			TrackBarVolume.AutoSize = false;
			TrackBarVolume.Location = new System.Drawing.Point(216, 42);
			TrackBarVolume.Maximum = 100;
			TrackBarVolume.Name = "TrackBarVolume";
			TrackBarVolume.Orientation = System.Windows.Forms.Orientation.Vertical;
			TrackBarVolume.Size = new System.Drawing.Size(37, 113);
			TrackBarVolume.TabIndex = 8;
			TrackBarVolume.TickFrequency = 10;
			TrackBarVolume.TickStyle = System.Windows.Forms.TickStyle.TopLeft;
			TrackBarVolume.ValueChanged += new System.EventHandler(TrackBarVolume_ValueChanged);
			panelPlayBtns.Controls.Add(TrackBarDuration);
			panelPlayBtns.Controls.Add(StopBtn);
			panelPlayBtns.Controls.Add(PlayPauseBtn);
			panelPlayBtns.Controls.Add(FastReverseBtn);
			panelPlayBtns.Controls.Add(FastForwardBtn);
			panelPlayBtns.Location = new System.Drawing.Point(49, 147);
			panelPlayBtns.Name = "panelPlayBtns";
			panelPlayBtns.Size = new System.Drawing.Size(167, 41);
			panelPlayBtns.TabIndex = 24;
			TrackBarDuration.AutoSize = false;
			TrackBarDuration.Location = new System.Drawing.Point(-4, 0);
			TrackBarDuration.Maximum = 1000;
			TrackBarDuration.Name = "TrackBarDuration";
			TrackBarDuration.Size = new System.Drawing.Size(172, 18);
			TrackBarDuration.TabIndex = 11;
			TrackBarDuration.TickFrequency = 0;
			TrackBarDuration.TickStyle = System.Windows.Forms.TickStyle.None;
			TrackBarDuration.Scroll += new System.EventHandler(TrackBarDuration_Scroll);
			StopBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			StopBtn.Location = new System.Drawing.Point(124, 18);
			StopBtn.Name = "StopBtn";
			StopBtn.Size = new System.Drawing.Size(40, 23);
			StopBtn.TabIndex = 16;
			StopBtn.Text = "Stop";
			StopBtn.Click += new System.EventHandler(StopBtn_Click);
			PlayPauseBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			PlayPauseBtn.Location = new System.Drawing.Point(73, 18);
			PlayPauseBtn.Name = "PlayPauseBtn";
			PlayPauseBtn.Size = new System.Drawing.Size(49, 23);
			PlayPauseBtn.TabIndex = 17;
			PlayPauseBtn.Text = "Play";
			PlayPauseBtn.Click += new System.EventHandler(PlayPauseBtn_Click);
			FastReverseBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			FastReverseBtn.Location = new System.Drawing.Point(8, 18);
			FastReverseBtn.Name = "FastReverseBtn";
			FastReverseBtn.Size = new System.Drawing.Size(30, 23);
			FastReverseBtn.TabIndex = 0;
			FastReverseBtn.Text = "<<";
			FastReverseBtn.MouseDown += new System.Windows.Forms.MouseEventHandler(FastReverseBtn_MouseDown);
			FastReverseBtn.MouseUp += new System.Windows.Forms.MouseEventHandler(FastReverseBtn_MouseUp);
			FastForwardBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			FastForwardBtn.Location = new System.Drawing.Point(40, 18);
			FastForwardBtn.Name = "FastForwardBtn";
			FastForwardBtn.Size = new System.Drawing.Size(30, 23);
			FastForwardBtn.TabIndex = 1;
			FastForwardBtn.Text = ">>";
			FastForwardBtn.MouseDown += new System.Windows.Forms.MouseEventHandler(FastForwardBtn_MouseDown);
			FastForwardBtn.MouseUp += new System.Windows.Forms.MouseEventHandler(FastForwardBtn_MouseUp);
			labelVol.AutoSize = true;
			labelVol.Location = new System.Drawing.Point(216, 26);
			labelVol.Name = "labelVol";
			labelVol.Size = new System.Drawing.Size(42, 13);
			labelVol.TabIndex = 9;
			labelVol.Text = "Volume";
			panelRotateLeft.Controls.Add(flowLayoutRotate);
			panelRotateLeft.Controls.Add(panelRotateLeftTop2);
			panelRotateLeft.Controls.Add(panelRotateLeftTop1);
			panelRotateLeft.Dock = System.Windows.Forms.DockStyle.Left;
			panelRotateLeft.Location = new System.Drawing.Point(0, 0);
			panelRotateLeft.Name = "panelRotateLeft";
			panelRotateLeft.Size = new System.Drawing.Size(123, 319);
			panelRotateLeft.TabIndex = 32;
			flowLayoutRotate.AutoScroll = true;
			flowLayoutRotate.Dock = System.Windows.Forms.DockStyle.Fill;
			flowLayoutRotate.Location = new System.Drawing.Point(0, 138);
			flowLayoutRotate.Name = "flowLayoutRotate";
			flowLayoutRotate.Size = new System.Drawing.Size(123, 181);
			flowLayoutRotate.TabIndex = 7;
			panelRotateLeftTop2.Controls.Add(Rotate_TimeTotal);
			panelRotateLeftTop2.Controls.Add(btnClearMediaPositions);
			panelRotateLeftTop2.Controls.Add(label21);
			panelRotateLeftTop2.Controls.Add(label19);
			panelRotateLeftTop2.Dock = System.Windows.Forms.DockStyle.Top;
			panelRotateLeftTop2.Location = new System.Drawing.Point(0, 66);
			panelRotateLeftTop2.Name = "panelRotateLeftTop2";
			panelRotateLeftTop2.Size = new System.Drawing.Size(123, 72);
			panelRotateLeftTop2.TabIndex = 0;
			Rotate_TimeTotal.CustomFormat = "mm:ss";
			Rotate_TimeTotal.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
			Rotate_TimeTotal.Location = new System.Drawing.Point(56, 30);
			Rotate_TimeTotal.Name = "Rotate_TimeTotal";
			Rotate_TimeTotal.ShowUpDown = true;
			Rotate_TimeTotal.Size = new System.Drawing.Size(52, 20);
			Rotate_TimeTotal.TabIndex = 1;
			btnClearMediaPositions.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			btnClearMediaPositions.Location = new System.Drawing.Point(4, 2);
			btnClearMediaPositions.Name = "btnClearMediaPositions";
			btnClearMediaPositions.Size = new System.Drawing.Size(112, 24);
			btnClearMediaPositions.TabIndex = 0;
			btnClearMediaPositions.Text = "Clear All Timings";
			toolTip1.SetToolTip(btnClearMediaPositions, "Copy to next blank Position");
			btnClearMediaPositions.Click += new System.EventHandler(btnClearMediaPositions_Click);
			label21.AutoSize = true;
			label21.Location = new System.Drawing.Point(3, 54);
			label21.Name = "label21";
			label21.Size = new System.Drawing.Size(115, 13);
			label21.TabIndex = 1;
			label21.Text = "Sequence/Start mm:ss";
			label21.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
			label19.AutoSize = true;
			label19.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
			label19.Location = new System.Drawing.Point(4, 34);
			label19.Name = "label19";
			label19.Size = new System.Drawing.Size(50, 13);
			label19.TabIndex = 25;
			label19.Text = "Length:";
			panelRotateLeftTop1.Controls.Add(groupBox3);
			panelRotateLeftTop1.Dock = System.Windows.Forms.DockStyle.Top;
			panelRotateLeftTop1.Location = new System.Drawing.Point(0, 0);
			panelRotateLeftTop1.Name = "panelRotateLeftTop1";
			panelRotateLeftTop1.Size = new System.Drawing.Size(123, 66);
			panelRotateLeftTop1.TabIndex = 68;
			groupBox3.Controls.Add(Rotate_SlidesGapUpDown);
			groupBox3.Controls.Add(Rotate_Equal);
			groupBox3.Controls.Add(Rotate_Multiple);
			groupBox3.Location = new System.Drawing.Point(4, 0);
			groupBox3.Name = "groupBox3";
			groupBox3.Size = new System.Drawing.Size(112, 60);
			groupBox3.TabIndex = 9;
			groupBox3.TabStop = false;
			Rotate_SlidesGapUpDown.Location = new System.Drawing.Point(63, 13);
			Rotate_SlidesGapUpDown.Maximum = new decimal(new int[4]
			{
				999,
				0,
				0,
				0
			});
			Rotate_SlidesGapUpDown.Name = "Rotate_SlidesGapUpDown";
			Rotate_SlidesGapUpDown.Size = new System.Drawing.Size(43, 20);
			Rotate_SlidesGapUpDown.TabIndex = 2;
			toolTip1.SetToolTip(Rotate_SlidesGapUpDown, "Timing in seconds");
			Rotate_Equal.AutoSize = true;
			Rotate_Equal.Location = new System.Drawing.Point(7, 15);
			Rotate_Equal.Name = "Rotate_Equal";
			Rotate_Equal.Size = new System.Drawing.Size(59, 17);
			Rotate_Equal.TabIndex = 0;
			Rotate_Equal.Tag = "1";
			Rotate_Equal.Text = "Simple:";
			toolTip1.SetToolTip(Rotate_Equal, "Rotate each slide in equal seconds");
			Rotate_Equal.CheckedChanged += new System.EventHandler(Rotate_Option_CheckedChanged);
			Rotate_Multiple.AutoSize = true;
			Rotate_Multiple.Location = new System.Drawing.Point(7, 37);
			Rotate_Multiple.Name = "Rotate_Multiple";
			Rotate_Multiple.Size = new System.Drawing.Size(77, 17);
			Rotate_Multiple.TabIndex = 1;
			Rotate_Multiple.Tag = "2";
			Rotate_Multiple.Text = "Sequence:";
			panel6.Dock = System.Windows.Forms.DockStyle.Right;
			panel6.Location = new System.Drawing.Point(413, 0);
			panel6.Name = "panel6";
			panel6.Size = new System.Drawing.Size(3, 351);
			panel6.TabIndex = 6;
			menuStripMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[2]
			{
				Menu_MainFile,
				Menu_MainTools
			});
			menuStripMain.Location = new System.Drawing.Point(0, 0);
			menuStripMain.Name = "menuStripMain";
			menuStripMain.Size = new System.Drawing.Size(775, 24);
			menuStripMain.TabIndex = 6;
			menuStripMain.Text = "menuStrip1";
			Menu_MainFile.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[8]
			{
				Menu_New,
				Menu_Save,
				Menu_SaveAs,
				Menu_SaveExit,
				toolStripSeparator16,
				Menu_EditHistoryList,
				toolStripSeparator18,
				Menu_Exit
			});
			Menu_MainFile.Name = "Menu_MainFile";
			Menu_MainFile.Size = new System.Drawing.Size(35, 20);
			Menu_MainFile.Text = "&File";
			Menu_New.Image = Resources.New;
			Menu_New.Name = "Menu_New";
			Menu_New.Size = new System.Drawing.Size(134, 22);
			Menu_New.Text = "&New";
			Menu_New.Click += new System.EventHandler(Menu_New_Click);
			Menu_Save.Image = Resources.Save;
			Menu_Save.Name = "Menu_Save";
			Menu_Save.Size = new System.Drawing.Size(134, 22);
			Menu_Save.Text = "&Save";
			Menu_Save.Click += new System.EventHandler(Menu_Save_Click);
			Menu_SaveAs.Image = Resources.Save;
			Menu_SaveAs.Name = "Menu_SaveAs";
			Menu_SaveAs.Size = new System.Drawing.Size(134, 22);
			Menu_SaveAs.Text = "Save &As...";
			Menu_SaveAs.Click += new System.EventHandler(Menu_SaveAs_Click);
			Menu_SaveExit.Image = Resources.SaveClose;
			Menu_SaveExit.Name = "Menu_SaveExit";
			Menu_SaveExit.Size = new System.Drawing.Size(134, 22);
			Menu_SaveExit.Text = "Save && &Exit";
			Menu_SaveExit.Click += new System.EventHandler(Menu_SaveExit_Click);
			toolStripSeparator16.Name = "toolStripSeparator16";
			toolStripSeparator16.Size = new System.Drawing.Size(131, 6);
			Menu_EditHistoryList.Name = "Menu_EditHistoryList";
			Menu_EditHistoryList.Size = new System.Drawing.Size(134, 22);
			Menu_EditHistoryList.Text = "&Recent Edits";
			toolStripSeparator18.Name = "toolStripSeparator18";
			toolStripSeparator18.Size = new System.Drawing.Size(131, 6);
			Menu_Exit.Name = "Menu_Exit";
			Menu_Exit.ShortcutKeys = (System.Windows.Forms.Keys)262259;
			Menu_Exit.Size = new System.Drawing.Size(134, 22);
			Menu_Exit.Text = "E&xit";
			Menu_Exit.Click += new System.EventHandler(Menu_Exit_Click);
			Menu_MainTools.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[8]
			{
				Menu_Import,
				Menu_WordWrap,
				Menu_ChordsMenu,
				Menu_EditHistorySeparator,
				Menu_TransposeDown,
				Menu_TransposeUp,
				toolStripSeparator6,
				Menu_ShowAllButtons
			});
			Menu_MainTools.Name = "Menu_MainTools";
			Menu_MainTools.Size = new System.Drawing.Size(44, 20);
			Menu_MainTools.Text = "&Tools";
			Menu_Import.Image = Resources.open;
			Menu_Import.Name = "Menu_Import";
			Menu_Import.Size = new System.Drawing.Size(190, 22);
			Menu_Import.Text = "&Import...";
			Menu_Import.Click += new System.EventHandler(Menu_Import_Click);
			Menu_WordWrap.CheckOnClick = true;
			Menu_WordWrap.Image = Resources.wordwrap;
			Menu_WordWrap.Name = "Menu_WordWrap";
			Menu_WordWrap.Size = new System.Drawing.Size(190, 22);
			Menu_WordWrap.Text = "&Word Wrap";
			Menu_WordWrap.Click += new System.EventHandler(Main_WordWrap_Click);
			Menu_ChordsMenu.CheckOnClick = true;
			Menu_ChordsMenu.Image = Resources.PopUpChords;
			Menu_ChordsMenu.Name = "Menu_ChordsMenu";
			Menu_ChordsMenu.Size = new System.Drawing.Size(190, 22);
			Menu_ChordsMenu.Text = "Right-Click Chords Menu";
			Menu_ChordsMenu.Click += new System.EventHandler(Menu_ChordsMenu_Click);
			Menu_EditHistorySeparator.Name = "Menu_EditHistorySeparator";
			Menu_EditHistorySeparator.Size = new System.Drawing.Size(187, 6);
			Menu_TransposeDown.Image = Resources.arrowGL;
			Menu_TransposeDown.Name = "Menu_TransposeDown";
			Menu_TransposeDown.Size = new System.Drawing.Size(190, 22);
			Menu_TransposeDown.Text = "Transpose Chord &Down";
			Menu_TransposeDown.Click += new System.EventHandler(Menu_TransposeDown_Click);
			Menu_TransposeUp.Image = Resources.arrowGR;
			Menu_TransposeUp.Name = "Menu_TransposeUp";
			Menu_TransposeUp.Size = new System.Drawing.Size(190, 22);
			Menu_TransposeUp.Text = "Transpose Chord &Up";
			Menu_TransposeUp.Click += new System.EventHandler(Menu_TransposeUp_Click);
			toolStripSeparator6.Name = "toolStripSeparator6";
			toolStripSeparator6.Size = new System.Drawing.Size(187, 6);
			Menu_ShowAllButtons.CheckOnClick = true;
			Menu_ShowAllButtons.Name = "Menu_ShowAllButtons";
			Menu_ShowAllButtons.Size = new System.Drawing.Size(190, 22);
			Menu_ShowAllButtons.Text = "Show All &Buttons";
			Menu_ShowAllButtons.Click += new System.EventHandler(Menu_ShowAllButtons_Click);
			TimerEditRequest.Interval = 1000;
			TimerEditRequest.Tick += new System.EventHandler(TimerEditRequest_Tick);
			OpenFileDialog1.FileName = "openFileDialog1";
			splitContainerMain.Dock = System.Windows.Forms.DockStyle.Fill;
			splitContainerMain.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
			splitContainerMain.Location = new System.Drawing.Point(0, 49);
			splitContainerMain.Name = "splitContainerMain";
			splitContainerMain.Orientation = System.Windows.Forms.Orientation.Horizontal;
			splitContainerMain.Panel1.BackColor = System.Drawing.SystemColors.Control;
			splitContainerMain.Panel1.Controls.Add(groupBox2);
			splitContainerMain.Panel1.Controls.Add(groupBox1);
			splitContainerMain.Panel2.Controls.Add(splitContainer1);
			splitContainerMain.Size = new System.Drawing.Size(775, 490);
			splitContainerMain.SplitterDistance = 131;
			splitContainerMain.TabIndex = 0;
			splitContainerMain.Text = "splitContainer2";
			splitContainerMain.SplitterMoved += new System.Windows.Forms.SplitterEventHandler(splitContainerMain_SplitterMoved);
			groupBox2.Controls.Add(panelVerses);
			groupBox2.Controls.Add(panelOrderList);
			groupBox2.Controls.Add(panelSeqSet);
			groupBox2.Controls.Add(panelSeqUpDown);
			groupBox2.Location = new System.Drawing.Point(447, 1);
			groupBox2.Name = "groupBox2";
			groupBox2.Padding = new System.Windows.Forms.Padding(0);
			groupBox2.Size = new System.Drawing.Size(239, 129);
			groupBox2.TabIndex = 1;
			groupBox2.TabStop = false;
			panelVerses.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			panelVerses.Controls.Add(VersesList);
			panelVerses.Controls.Add(panel2);
			panelVerses.Location = new System.Drawing.Point(6, 10);
			panelVerses.Name = "panelVerses";
			panelVerses.Size = new System.Drawing.Size(90, 114);
			panelVerses.TabIndex = 1;
			VersesList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[2]
			{
				columnHeader1,
				columnHeader2
			});
			VersesList.Dock = System.Windows.Forms.DockStyle.Fill;
			VersesList.FullRowSelect = true;
			VersesList.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
			VersesList.Location = new System.Drawing.Point(0, 14);
			VersesList.Name = "VersesList";
			VersesList.ShowItemToolTips = true;
			VersesList.Size = new System.Drawing.Size(86, 96);
			VersesList.TabIndex = 0;
			VersesList.UseCompatibleStateImageBehavior = false;
			VersesList.View = System.Windows.Forms.View.Details;
			VersesList.DoubleClick += new System.EventHandler(VersesList_DoubleClick);
			columnHeader1.Width = 65;
			columnHeader2.Width = 0;
			panel2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			panel2.Controls.Add(label16);
			panel2.Dock = System.Windows.Forms.DockStyle.Top;
			panel2.Location = new System.Drawing.Point(0, 0);
			panel2.Name = "panel2";
			panel2.Size = new System.Drawing.Size(86, 14);
			panel2.TabIndex = 0;
			label16.AutoSize = true;
			label16.Location = new System.Drawing.Point(12, -1);
			label16.Name = "label16";
			label16.Size = new System.Drawing.Size(39, 13);
			label16.TabIndex = 0;
			label16.Text = "Verses";
			panelOrderList.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			panelOrderList.Controls.Add(OrderList);
			panelOrderList.Controls.Add(panel4);
			panelOrderList.Location = new System.Drawing.Point(122, 10);
			panelOrderList.Name = "panelOrderList";
			panelOrderList.Size = new System.Drawing.Size(90, 114);
			panelOrderList.TabIndex = 2;
			OrderList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[2]
			{
				columnHeader3,
				columnHeader4
			});
			OrderList.Dock = System.Windows.Forms.DockStyle.Fill;
			OrderList.FullRowSelect = true;
			OrderList.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
			OrderList.Location = new System.Drawing.Point(0, 14);
			OrderList.Name = "OrderList";
			OrderList.Size = new System.Drawing.Size(86, 96);
			OrderList.TabIndex = 0;
			OrderList.UseCompatibleStateImageBehavior = false;
			OrderList.View = System.Windows.Forms.View.Details;
			OrderList.KeyUp += new System.Windows.Forms.KeyEventHandler(OrderList_KeyUp);
			columnHeader3.Width = 65;
			columnHeader4.Width = 0;
			panel4.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			panel4.Controls.Add(label17);
			panel4.Dock = System.Windows.Forms.DockStyle.Top;
			panel4.Location = new System.Drawing.Point(0, 0);
			panel4.Name = "panel4";
			panel4.Size = new System.Drawing.Size(86, 14);
			panel4.TabIndex = 0;
			label17.AutoSize = true;
			label17.Location = new System.Drawing.Point(10, -1);
			label17.Name = "label17";
			label17.Size = new System.Drawing.Size(56, 13);
			label17.TabIndex = 0;
			label17.Text = "Sequence";
			panelSeqSet.Controls.Add(toolStripSeqSet);
			panelSeqSet.Location = new System.Drawing.Point(95, 28);
			panelSeqSet.Name = "panelSeqSet";
			panelSeqSet.Size = new System.Drawing.Size(25, 52);
			panelSeqSet.TabIndex = 13;
			toolStripSeqSet.AutoSize = false;
			toolStripSeqSet.CanOverflow = false;
			toolStripSeqSet.Dock = System.Windows.Forms.DockStyle.None;
			toolStripSeqSet.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			toolStripSeqSet.Items.AddRange(new System.Windows.Forms.ToolStripItem[2]
			{
				Verses_Add,
				Verses_SmartAdd
			});
			toolStripSeqSet.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.VerticalStackWithOverflow;
			toolStripSeqSet.Location = new System.Drawing.Point(0, 1);
			toolStripSeqSet.Name = "toolStripSeqSet";
			toolStripSeqSet.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
			toolStripSeqSet.Size = new System.Drawing.Size(25, 62);
			toolStripSeqSet.TabIndex = 5;
			Verses_Add.AutoSize = false;
			Verses_Add.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			Verses_Add.Image = Resources.arrowR;
			Verses_Add.ImageTransparentColor = System.Drawing.Color.Magenta;
			Verses_Add.Name = "Verses_Add";
			Verses_Add.Size = new System.Drawing.Size(22, 22);
			Verses_Add.Tag = "";
			Verses_Add.ToolTipText = "Move Item Up";
			Verses_Add.Click += new System.EventHandler(Verses_Add_Click);
			Verses_SmartAdd.AutoSize = false;
			Verses_SmartAdd.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			Verses_SmartAdd.Image = Resources.multi_arrowr;
			Verses_SmartAdd.ImageTransparentColor = System.Drawing.Color.Magenta;
			Verses_SmartAdd.Name = "Verses_SmartAdd";
			Verses_SmartAdd.Size = new System.Drawing.Size(22, 22);
			Verses_SmartAdd.Tag = "";
			Verses_SmartAdd.ToolTipText = "Move Item Down";
			Verses_SmartAdd.Click += new System.EventHandler(Verses_Add_Click);
			panelSeqUpDown.Controls.Add(toolStripSeqUpDown);
			panelSeqUpDown.Location = new System.Drawing.Point(211, 26);
			panelSeqUpDown.Name = "panelSeqUpDown";
			panelSeqUpDown.Size = new System.Drawing.Size(25, 79);
			panelSeqUpDown.TabIndex = 12;
			toolStripSeqUpDown.AutoSize = false;
			toolStripSeqUpDown.CanOverflow = false;
			toolStripSeqUpDown.Dock = System.Windows.Forms.DockStyle.None;
			toolStripSeqUpDown.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			toolStripSeqUpDown.Items.AddRange(new System.Windows.Forms.ToolStripItem[4]
			{
				OrderList_Up,
				OrderList_Down,
				toolStripSeparator5,
				OrderList_Delete
			});
			toolStripSeqUpDown.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.VerticalStackWithOverflow;
			toolStripSeqUpDown.Location = new System.Drawing.Point(0, 0);
			toolStripSeqUpDown.Name = "toolStripSeqUpDown";
			toolStripSeqUpDown.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
			toolStripSeqUpDown.Size = new System.Drawing.Size(25, 83);
			toolStripSeqUpDown.TabIndex = 5;
			OrderList_Up.AutoSize = false;
			OrderList_Up.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			OrderList_Up.Image = Resources.handup;
			OrderList_Up.ImageTransparentColor = System.Drawing.Color.Magenta;
			OrderList_Up.Name = "OrderList_Up";
			OrderList_Up.Size = new System.Drawing.Size(22, 22);
			OrderList_Up.Tag = "up";
			OrderList_Up.ToolTipText = "Move Item Up";
			OrderList_Up.Click += new System.EventHandler(OrderList_Btn_Click);
			OrderList_Down.AutoSize = false;
			OrderList_Down.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			OrderList_Down.Image = Resources.handdown;
			OrderList_Down.ImageTransparentColor = System.Drawing.Color.Magenta;
			OrderList_Down.Name = "OrderList_Down";
			OrderList_Down.Size = new System.Drawing.Size(22, 22);
			OrderList_Down.Tag = "down";
			OrderList_Down.ToolTipText = "Move Item Down";
			OrderList_Down.Click += new System.EventHandler(OrderList_Btn_Click);
			toolStripSeparator5.Name = "toolStripSeparator5";
			toolStripSeparator5.Size = new System.Drawing.Size(23, 6);
			OrderList_Delete.AutoSize = false;
			OrderList_Delete.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			OrderList_Delete.Image = Resources.Delete;
			OrderList_Delete.ImageTransparentColor = System.Drawing.Color.Magenta;
			OrderList_Delete.Name = "OrderList_Delete";
			OrderList_Delete.Size = new System.Drawing.Size(22, 22);
			OrderList_Delete.Tag = "delete";
			OrderList_Delete.ToolTipText = "Delete";
			OrderList_Delete.Click += new System.EventHandler(OrderList_Btn_Click);
			groupBox1.Controls.Add(panel7);
			groupBox1.Controls.Add(panel8);
			groupBox1.Location = new System.Drawing.Point(3, 1);
			groupBox1.Margin = new System.Windows.Forms.Padding(3, 3, 6, 6);
			groupBox1.Name = "groupBox1";
			groupBox1.Padding = new System.Windows.Forms.Padding(0);
			groupBox1.Size = new System.Drawing.Size(449, 129);
			groupBox1.TabIndex = 0;
			groupBox1.TabStop = false;
			panel7.Controls.Add(Btn_Title2);
			panel7.Controls.Add(Btn_Title);
			panel7.Controls.Add(Btn_Copyright);
			panel7.Controls.Add(Btn_Writer);
			panel7.Controls.Add(SongFolder);
			panel7.Controls.Add(panelLinkTitle2Lookup);
			panel7.Controls.Add(LinkTitle2Pic);
			panel7.Controls.Add(CopyrightInfo);
			panel7.Controls.Add(label2);
			panel7.Controls.Add(WriterInfo);
			panel7.Controls.Add(label3);
			panel7.Controls.Add(SongTitle2);
			panel7.Controls.Add(label4);
			panel7.Controls.Add(SongTitle);
			panel7.Controls.Add(label5);
			panel7.Controls.Add(labelFormat);
			panel7.Location = new System.Drawing.Point(3, 10);
			panel7.Name = "panel7";
			panel7.Size = new System.Drawing.Size(216, 114);
			panel7.TabIndex = 0;
			Btn_Title2.BackColor = System.Drawing.Color.Aqua;
			Btn_Title2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			Btn_Title2.Location = new System.Drawing.Point(140, 48);
			Btn_Title2.Name = "Btn_Title2";
			Btn_Title2.Size = new System.Drawing.Size(18, 20);
			Btn_Title2.TabIndex = 36;
			Btn_Title2.TabStop = false;
			Btn_Title2.Text = "...";
			Btn_Title2.UseVisualStyleBackColor = false;
			Btn_Title2.Visible = false;
			Btn_Title2.Enter += new System.EventHandler(Btn_Enter);
			Btn_Title2.Click += new System.EventHandler(Btn_Click);
			Btn_Title2.Leave += new System.EventHandler(Btn_Enter);
			Btn_Title.BackColor = System.Drawing.Color.Aqua;
			Btn_Title.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			Btn_Title.Location = new System.Drawing.Point(193, 26);
			Btn_Title.Name = "Btn_Title";
			Btn_Title.Size = new System.Drawing.Size(18, 20);
			Btn_Title.TabIndex = 35;
			Btn_Title.TabStop = false;
			Btn_Title.Text = "...";
			Btn_Title.UseVisualStyleBackColor = false;
			Btn_Title.Visible = false;
			Btn_Title.Enter += new System.EventHandler(Btn_Enter);
			Btn_Title.Click += new System.EventHandler(Btn_Click);
			Btn_Title.Leave += new System.EventHandler(Btn_Enter);
			Btn_Copyright.BackColor = System.Drawing.Color.Aqua;
			Btn_Copyright.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			Btn_Copyright.Location = new System.Drawing.Point(193, 92);
			Btn_Copyright.Name = "Btn_Copyright";
			Btn_Copyright.Size = new System.Drawing.Size(18, 20);
			Btn_Copyright.TabIndex = 34;
			Btn_Copyright.TabStop = false;
			Btn_Copyright.Text = "...";
			Btn_Copyright.UseVisualStyleBackColor = false;
			Btn_Copyright.Visible = false;
			Btn_Copyright.Enter += new System.EventHandler(Btn_Enter);
			Btn_Copyright.Click += new System.EventHandler(Btn_Click);
			Btn_Copyright.Leave += new System.EventHandler(Btn_Enter);
			Btn_Writer.BackColor = System.Drawing.Color.Aqua;
			Btn_Writer.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			Btn_Writer.Location = new System.Drawing.Point(193, 70);
			Btn_Writer.Name = "Btn_Writer";
			Btn_Writer.Size = new System.Drawing.Size(18, 20);
			Btn_Writer.TabIndex = 33;
			Btn_Writer.TabStop = false;
			Btn_Writer.Text = "...";
			Btn_Writer.UseVisualStyleBackColor = false;
			Btn_Writer.Visible = false;
			Btn_Writer.Enter += new System.EventHandler(Btn_Enter);
			Btn_Writer.Click += new System.EventHandler(Btn_Click);
			Btn_Writer.Leave += new System.EventHandler(Btn_Enter);
			SongFolder.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			SongFolder.FormattingEnabled = true;
			SongFolder.Location = new System.Drawing.Point(93, 3);
			SongFolder.MaxDropDownItems = 12;
			SongFolder.Name = "SongFolder";
			SongFolder.Size = new System.Drawing.Size(118, 21);
			SongFolder.TabIndex = 1;
			SongFolder.SelectedIndexChanged += new System.EventHandler(SongFolder_SelectedIndexChanged);
			panelLinkTitle2Lookup.Controls.Add(toolStrip2);
			panelLinkTitle2Lookup.Location = new System.Drawing.Point(184, 47);
			panelLinkTitle2Lookup.Name = "panelLinkTitle2Lookup";
			panelLinkTitle2Lookup.Size = new System.Drawing.Size(22, 21);
			panelLinkTitle2Lookup.TabIndex = 28;
			toolStrip2.AutoSize = false;
			toolStrip2.CanOverflow = false;
			toolStrip2.Dock = System.Windows.Forms.DockStyle.None;
			toolStrip2.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			toolStrip2.Items.AddRange(new System.Windows.Forms.ToolStripItem[1]
			{
				Title2_LookUp
			});
			toolStrip2.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.Flow;
			toolStrip2.Location = new System.Drawing.Point(0, -1);
			toolStrip2.Name = "toolStrip2";
			toolStrip2.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
			toolStrip2.Size = new System.Drawing.Size(28, 24);
			toolStrip2.TabIndex = 4;
			Title2_LookUp.AutoSize = false;
			Title2_LookUp.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			Title2_LookUp.Image = Resources.folder;
			Title2_LookUp.ImageTransparentColor = System.Drawing.Color.Magenta;
			Title2_LookUp.Name = "Title2_LookUp";
			Title2_LookUp.Size = new System.Drawing.Size(22, 22);
			Title2_LookUp.Tag = "down";
			Title2_LookUp.ToolTipText = "Look Up Title";
			Title2_LookUp.Click += new System.EventHandler(Title2_LookUp_Click);
			LinkTitle2Pic.BackgroundImage = Resources.Tick;
			LinkTitle2Pic.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
			LinkTitle2Pic.Location = new System.Drawing.Point(162, 48);
			LinkTitle2Pic.Name = "LinkTitle2Pic";
			LinkTitle2Pic.Size = new System.Drawing.Size(21, 21);
			LinkTitle2Pic.TabIndex = 6;
			CopyrightInfo.Location = new System.Drawing.Point(56, 92);
			CopyrightInfo.MaxLength = 100;
			CopyrightInfo.Name = "CopyrightInfo";
			CopyrightInfo.Size = new System.Drawing.Size(156, 20);
			CopyrightInfo.TabIndex = 6;
			CopyrightInfo.Enter += new System.EventHandler(TextBox_Enter);
			CopyrightInfo.Leave += new System.EventHandler(TextBox_Leave);
			label2.AutoSize = true;
			label2.Location = new System.Drawing.Point(3, 29);
			label2.Name = "label2";
			label2.Size = new System.Drawing.Size(30, 13);
			label2.TabIndex = 4;
			label2.Text = "Title:";
			label2.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
			WriterInfo.Location = new System.Drawing.Point(56, 70);
			WriterInfo.MaxLength = 100;
			WriterInfo.Name = "WriterInfo";
			WriterInfo.Size = new System.Drawing.Size(156, 20);
			WriterInfo.TabIndex = 5;
			WriterInfo.Enter += new System.EventHandler(TextBox_Enter);
			WriterInfo.Leave += new System.EventHandler(TextBox_Leave);
			label3.AutoSize = true;
			label3.Location = new System.Drawing.Point(3, 51);
			label3.Name = "label3";
			label3.Size = new System.Drawing.Size(36, 13);
			label3.TabIndex = 5;
			label3.Text = "Title2:";
			label3.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
			SongTitle2.Location = new System.Drawing.Point(56, 48);
			SongTitle2.MaxLength = 100;
			SongTitle2.Name = "SongTitle2";
			SongTitle2.Size = new System.Drawing.Size(103, 20);
			SongTitle2.TabIndex = 3;
			SongTitle2.Enter += new System.EventHandler(TextBox_Enter);
			SongTitle2.Leave += new System.EventHandler(TextBox_Leave);
			label4.AutoSize = true;
			label4.Location = new System.Drawing.Point(3, 73);
			label4.Name = "label4";
			label4.Size = new System.Drawing.Size(38, 13);
			label4.TabIndex = 6;
			label4.Text = "Writer:";
			label4.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
			SongTitle.Location = new System.Drawing.Point(56, 26);
			SongTitle.MaxLength = 100;
			SongTitle.Name = "SongTitle";
			SongTitle.Size = new System.Drawing.Size(141, 20);
			SongTitle.TabIndex = 2;
			SongTitle.Enter += new System.EventHandler(TextBox_Enter);
			SongTitle.Leave += new System.EventHandler(TextBox_Leave);
			label5.AutoSize = true;
			label5.Location = new System.Drawing.Point(3, 94);
			label5.Name = "label5";
			label5.Size = new System.Drawing.Size(54, 13);
			label5.TabIndex = 7;
			label5.Text = "Copyright:";
			label5.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
			labelFormat.AutoSize = true;
			labelFormat.Location = new System.Drawing.Point(3, 6);
			labelFormat.Name = "labelFormat";
			labelFormat.Size = new System.Drawing.Size(89, 13);
			labelFormat.TabIndex = 29;
			labelFormat.Text = "Format As Folder:";
			labelFormat.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
			panel8.Controls.Add(Btn_BookRef);
			panel8.Controls.Add(Btn_UserRef);
			panel8.Controls.Add(label6);
			panel8.Controls.Add(UserReference);
			panel8.Controls.Add(BookReference);
			panel8.Controls.Add(label9);
			panel8.Controls.Add(label10);
			panel8.Controls.Add(LicAdminInfo2);
			panel8.Controls.Add(LicAdminInfo1);
			panel8.Controls.Add(SongTiming);
			panel8.Controls.Add(label13);
			panel8.Controls.Add(SongKey);
			panel8.Controls.Add(SongNumber);
			panel8.Controls.Add(SongCapo);
			panel8.Controls.Add(label11);
			panel8.Controls.Add(label12);
			panel8.Controls.Add(label8);
			panel8.Controls.Add(label7);
			panel8.Location = new System.Drawing.Point(220, 10);
			panel8.Name = "panel8";
			panel8.Size = new System.Drawing.Size(221, 114);
			panel8.TabIndex = 1;
			Btn_BookRef.BackColor = System.Drawing.Color.Aqua;
			Btn_BookRef.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			Btn_BookRef.Location = new System.Drawing.Point(200, 47);
			Btn_BookRef.Name = "Btn_BookRef";
			Btn_BookRef.Size = new System.Drawing.Size(18, 20);
			Btn_BookRef.TabIndex = 31;
			Btn_BookRef.TabStop = false;
			Btn_BookRef.Text = "...";
			Btn_BookRef.UseVisualStyleBackColor = false;
			Btn_BookRef.Visible = false;
			Btn_BookRef.Enter += new System.EventHandler(Btn_Enter);
			Btn_BookRef.Click += new System.EventHandler(Btn_Click);
			Btn_BookRef.Leave += new System.EventHandler(Btn_Enter);
			Btn_UserRef.BackColor = System.Drawing.Color.Aqua;
			Btn_UserRef.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			Btn_UserRef.Location = new System.Drawing.Point(200, 69);
			Btn_UserRef.Name = "Btn_UserRef";
			Btn_UserRef.Size = new System.Drawing.Size(18, 20);
			Btn_UserRef.TabIndex = 30;
			Btn_UserRef.TabStop = false;
			Btn_UserRef.Text = "...";
			Btn_UserRef.UseVisualStyleBackColor = false;
			Btn_UserRef.Visible = false;
			Btn_UserRef.Enter += new System.EventHandler(Btn_Enter);
			Btn_UserRef.Click += new System.EventHandler(Btn_Click);
			Btn_UserRef.Leave += new System.EventHandler(Btn_Enter);
			label6.AutoSize = true;
			label6.Location = new System.Drawing.Point(3, 6);
			label6.Name = "label6";
			label6.Size = new System.Drawing.Size(35, 13);
			label6.TabIndex = 12;
			label6.Text = "Capo:";
			label6.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
			UserReference.Location = new System.Drawing.Point(56, 69);
			UserReference.Name = "UserReference";
			UserReference.Size = new System.Drawing.Size(144, 20);
			UserReference.TabIndex = 5;
			UserReference.Enter += new System.EventHandler(TextBox_Enter);
			UserReference.Leave += new System.EventHandler(TextBox_Leave);
			BookReference.Location = new System.Drawing.Point(56, 47);
			BookReference.MaxLength = 100;
			BookReference.Name = "BookReference";
			BookReference.Size = new System.Drawing.Size(144, 20);
			BookReference.TabIndex = 4;
			BookReference.Enter += new System.EventHandler(TextBox_Enter);
			BookReference.Leave += new System.EventHandler(TextBox_Leave);
			label9.AutoSize = true;
			label9.Location = new System.Drawing.Point(3, 73);
			label9.Name = "label9";
			label9.Size = new System.Drawing.Size(52, 13);
			label9.TabIndex = 19;
			label9.Text = "User Ref:";
			label9.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
			label10.AutoSize = true;
			label10.Location = new System.Drawing.Point(3, 94);
			label10.Name = "label10";
			label10.Size = new System.Drawing.Size(48, 13);
			label10.TabIndex = 20;
			label10.Text = "Admin 1:";
			label10.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
			LicAdminInfo2.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			LicAdminInfo2.FormattingEnabled = true;
			LicAdminInfo2.Location = new System.Drawing.Point(146, 91);
			LicAdminInfo2.MaxDropDownItems = 12;
			LicAdminInfo2.Name = "LicAdminInfo2";
			LicAdminInfo2.Size = new System.Drawing.Size(72, 21);
			LicAdminInfo2.TabIndex = 7;
			LicAdminInfo1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			LicAdminInfo1.FormattingEnabled = true;
			LicAdminInfo1.Location = new System.Drawing.Point(56, 91);
			LicAdminInfo1.MaxDropDownItems = 12;
			LicAdminInfo1.Name = "LicAdminInfo1";
			LicAdminInfo1.Size = new System.Drawing.Size(72, 21);
			LicAdminInfo1.TabIndex = 6;
			SongTiming.FormattingEnabled = true;
			SongTiming.Location = new System.Drawing.Point(166, 24);
			SongTiming.MaxDropDownItems = 13;
			SongTiming.Name = "SongTiming";
			SongTiming.Size = new System.Drawing.Size(52, 21);
			SongTiming.TabIndex = 3;
			label13.AutoSize = true;
			label13.Location = new System.Drawing.Point(130, 94);
			label13.Name = "label13";
			label13.Size = new System.Drawing.Size(16, 13);
			label13.TabIndex = 23;
			label13.Text = "2:";
			label13.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
			SongKey.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			SongKey.FormattingEnabled = true;
			SongKey.Location = new System.Drawing.Point(166, 2);
			SongKey.MaxDropDownItems = 13;
			SongKey.Name = "SongKey";
			SongKey.Size = new System.Drawing.Size(52, 21);
			SongKey.TabIndex = 1;
			SongNumber.Location = new System.Drawing.Point(56, 25);
			SongNumber.MaxLength = 10;
			SongNumber.Name = "SongNumber";
			SongNumber.Size = new System.Drawing.Size(70, 20);
			SongNumber.TabIndex = 2;
			SongCapo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			SongCapo.FormattingEnabled = true;
			SongCapo.Location = new System.Drawing.Point(56, 2);
			SongCapo.MaxDropDownItems = 13;
			SongCapo.Name = "SongCapo";
			SongCapo.Size = new System.Drawing.Size(70, 21);
			SongCapo.TabIndex = 0;
			label11.AutoSize = true;
			label11.Location = new System.Drawing.Point(129, 5);
			label11.Name = "label11";
			label11.Size = new System.Drawing.Size(28, 13);
			label11.TabIndex = 21;
			label11.Text = "Key:";
			label11.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
			label12.AutoSize = true;
			label12.Location = new System.Drawing.Point(128, 28);
			label12.Name = "label12";
			label12.Size = new System.Drawing.Size(41, 13);
			label12.TabIndex = 22;
			label12.Text = "Timing:";
			label12.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
			label8.AutoSize = true;
			label8.Location = new System.Drawing.Point(3, 51);
			label8.Name = "label8";
			label8.Size = new System.Drawing.Size(55, 13);
			label8.TabIndex = 18;
			label8.Text = "Book Ref:";
			label8.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
			label7.AutoSize = true;
			label7.Location = new System.Drawing.Point(3, 29);
			label7.Name = "label7";
			label7.Size = new System.Drawing.Size(55, 13);
			label7.TabIndex = 17;
			label7.Text = "Song No.:";
			label7.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
			TimerFast.Interval = 500;
			TimerFast.Tick += new System.EventHandler(TimerFast_Tick);
			TimerAttemptConnect.Interval = 500;
			TimerAttemptConnect.Tick += new System.EventHandler(TimerAttemptConnect_Tick);
			TimerTrack.Interval = 1000;
			TimerTrack.Tick += new System.EventHandler(TimerTrack_Tick);
			base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
			base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			base.ClientSize = new System.Drawing.Size(775, 561);
			base.Controls.Add(splitContainerMain);
			base.Controls.Add(toolStrip1);
			base.Controls.Add(statusStrip1);
			base.Controls.Add(menuStripMain);
			base.Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
			MinimumSize = new System.Drawing.Size(245, 238);
			base.Name = "FrmInfoScreen";
			base.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
			Text = "InfoScreen";
			base.Resize += new System.EventHandler(FrmInfoScreen_Resize);
			base.FormClosing += new System.Windows.Forms.FormClosingEventHandler(FrmInfoScreen_FormClosing);
			base.Load += new System.EventHandler(FrmInfoScreen_Load);
			toolStrip1.ResumeLayout(false);
			toolStrip1.PerformLayout();
			statusStrip1.ResumeLayout(false);
			statusStrip1.PerformLayout();
			splitContainer1.Panel1.ResumeLayout(false);
			splitContainer1.Panel2.ResumeLayout(false);
			splitContainer1.ResumeLayout(false);
			CMRegion1.ResumeLayout(false);
			panelR1Top.ResumeLayout(false);
			panelR1Left.ResumeLayout(false);
			panel3.ResumeLayout(false);
			panelR1LeftBottom.ResumeLayout(false);
			panelR1LeftMiddle.ResumeLayout(false);
			panelR1LeftTop.ResumeLayout(false);
			tabRightPane.ResumeLayout(false);
			tabRight_Region2.ResumeLayout(false);
			panelR2All.ResumeLayout(false);
			CMRegion2.ResumeLayout(false);
			panelR2Top.ResumeLayout(false);
			panelR2Left.ResumeLayout(false);
			panel1.ResumeLayout(false);
			panelR2LeftBottom.ResumeLayout(false);
			panelR2LeftMiddle.ResumeLayout(false);
			panelR2LeftTop.ResumeLayout(false);
			tabRight_Rotate.ResumeLayout(false);
			panelRotate.ResumeLayout(false);
			splitContainerRotate.Panel1.ResumeLayout(false);
			splitContainerRotate.Panel2.ResumeLayout(false);
			splitContainerRotate.ResumeLayout(false);
			groupBoxRotateVerses.ResumeLayout(false);
			panelRotate_Verses.ResumeLayout(false);
			panel10.ResumeLayout(false);
			panel10.PerformLayout();
			panelRotate_OrderList.ResumeLayout(false);
			panel12.ResumeLayout(false);
			panel12.PerformLayout();
			panel13.ResumeLayout(false);
			toolStripRotate_SeqSet.ResumeLayout(false);
			toolStripRotate_SeqSet.PerformLayout();
			panel14.ResumeLayout(false);
			toolStripRotate_SeqUpDown.ResumeLayout(false);
			toolStripRotate_SeqUpDown.PerformLayout();
			panelRotate_Sample.ResumeLayout(false);
			panelRotate_Sample.PerformLayout();
			panelRotate_Media.ResumeLayout(false);
			panelNoPlayer.ResumeLayout(false);
			panelLoc.ResumeLayout(false);
			toolStrip3.ResumeLayout(false);
			toolStrip3.PerformLayout();
			((System.ComponentModel.ISupportInitialize)TrackBarVolume).EndInit();
			panelPlayBtns.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)TrackBarDuration).EndInit();
			panelRotateLeft.ResumeLayout(false);
			panelRotateLeftTop2.ResumeLayout(false);
			panelRotateLeftTop2.PerformLayout();
			panelRotateLeftTop1.ResumeLayout(false);
			groupBox3.ResumeLayout(false);
			groupBox3.PerformLayout();
			((System.ComponentModel.ISupportInitialize)Rotate_SlidesGapUpDown).EndInit();
			menuStripMain.ResumeLayout(false);
			menuStripMain.PerformLayout();
			splitContainerMain.Panel1.ResumeLayout(false);
			splitContainerMain.Panel2.ResumeLayout(false);
			splitContainerMain.ResumeLayout(false);
			groupBox2.ResumeLayout(false);
			panelVerses.ResumeLayout(false);
			panel2.ResumeLayout(false);
			panel2.PerformLayout();
			panelOrderList.ResumeLayout(false);
			panel4.ResumeLayout(false);
			panel4.PerformLayout();
			panelSeqSet.ResumeLayout(false);
			toolStripSeqSet.ResumeLayout(false);
			toolStripSeqSet.PerformLayout();
			panelSeqUpDown.ResumeLayout(false);
			toolStripSeqUpDown.ResumeLayout(false);
			toolStripSeqUpDown.PerformLayout();
			groupBox1.ResumeLayout(false);
			panel7.ResumeLayout(false);
			panel7.PerformLayout();
			panelLinkTitle2Lookup.ResumeLayout(false);
			toolStrip2.ResumeLayout(false);
			toolStrip2.PerformLayout();
			panel8.ResumeLayout(false);
			panel8.PerformLayout();
			ResumeLayout(false);
			PerformLayout();
		}

		public FrmInfoScreen()
		{
			InitializeComponent();
			popupHelper = new PopupWindowHelper();
			popupHelper.PopupClosed += popupHelper_PopupClosed;
			tbLyrics1.Focus();
		}

		protected override void OnHandleCreated(EventArgs e)
		{
			popupHelper.AssignHandle(base.Handle);
		}

		private void Btn_Click(object sender, EventArgs e)
		{
			Point p = new Point(0, 0);
			Button button = (Button)sender;
			PopupBtnPressed = button.Name;
			switch (PopupBtnPressed)
			{
			case "Btn_Title":
				gf.popUpText = SongTitle.Text;
				gf.popUpTextMaxLength = SongTitle.MaxLength;
				p = new Point(splitContainerMain.Left + groupBox1.Left + panel7.Left + SongTitle.Left, splitContainerMain.Top + groupBox1.Top + panel7.Top + SongTitle.Top + SongTitle.Height);
				break;
			case "Btn_Title2":
				gf.popUpText = SongTitle2.Text;
				gf.popUpTextMaxLength = SongTitle2.MaxLength;
				p = new Point(splitContainerMain.Left + groupBox1.Left + panel7.Left + SongTitle2.Left, splitContainerMain.Top + groupBox1.Top + panel7.Top + SongTitle2.Top + SongTitle2.Height);
				break;
			case "Btn_Writer":
				gf.popUpText = WriterInfo.Text;
				gf.popUpTextMaxLength = WriterInfo.MaxLength;
				p = new Point(splitContainerMain.Left + groupBox1.Left + panel7.Left + WriterInfo.Left, splitContainerMain.Top + groupBox1.Top + panel7.Top + WriterInfo.Top + WriterInfo.Height);
				break;
			case "Btn_Copyright":
				gf.popUpText = CopyrightInfo.Text;
				gf.popUpTextMaxLength = CopyrightInfo.MaxLength;
				p = new Point(splitContainerMain.Left + groupBox1.Left + panel7.Left + CopyrightInfo.Left, splitContainerMain.Top + groupBox1.Top + panel7.Top + CopyrightInfo.Top + CopyrightInfo.Height);
				break;
			case "Btn_BookRef":
				gf.popUpText = BookReference.Text;
				gf.popUpTextMaxLength = BookReference.MaxLength;
				p = new Point(splitContainerMain.Left + groupBox1.Left + panel8.Left + BookReference.Left, splitContainerMain.Top + groupBox1.Top + panel8.Top + BookReference.Top + BookReference.Height);
				break;
			case "Btn_UserRef":
				gf.popUpText = UserReference.Text;
				gf.popUpTextMaxLength = UserReference.MaxLength;
				p = new Point(splitContainerMain.Left + groupBox1.Left + panel8.Left + UserReference.Left, splitContainerMain.Top + groupBox1.Top + panel8.Top + UserReference.Top + UserReference.Height);
				break;
			}
			FrmPopupText popup = new FrmPopupText();
			Point location = PointToScreen(p);
			popupHelper.ShowPopup(this, popup, location);
		}

		private void popupHelper_PopupClosed(object sender, PopupClosedEventArgs e)
		{
			switch (PopupBtnPressed)
			{
			case "Btn_Title":
				SongTitle.Text = gf.popUpText;
				break;
			case "Btn_Title2":
				SongTitle2.Text = gf.popUpText;
				break;
			case "Btn_Writer":
				WriterInfo.Text = gf.popUpText;
				break;
			case "Btn_Copyright":
				CopyrightInfo.Text = gf.popUpText;
				break;
			case "Btn_BookRef":
				BookReference.Text = gf.popUpText;
				break;
			case "Btn_UserRef":
				UserReference.Text = gf.popUpText;
				break;
			}
		}

		private void FrmInfoScreen_Load(object sender, EventArgs e)
		{
			int num = DataUtil.ObjToInt(RegUtil.GetRegValue("settings", Reg_FormLeft, -1));
			int num2 = DataUtil.ObjToInt(RegUtil.GetRegValue("settings", Reg_FormTop, -1));
			int num3 = DataUtil.ObjToInt(RegUtil.GetRegValue("settings", Reg_FormWidth, 620));
			int num4 = DataUtil.ObjToInt(RegUtil.GetRegValue("settings", Reg_FormHeight, 450));
			FormState = DataUtil.ObjToInt(RegUtil.GetRegValue("settings", Reg_FormMax, 0));
			int num5 = DataUtil.ObjToInt(RegUtil.GetRegValue("settings", Reg_FormLyricsSplit, 520));
			int num6 = DataUtil.ObjToInt(RegUtil.GetRegValue("settings", Reg_FormRegion2Tab, 0));
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
				num = (Screen.PrimaryScreen.Bounds.Width - num3) / 2;
			}
			if (num2 < 0)
			{
				num2 = (Screen.PrimaryScreen.Bounds.Height - num4) / 2;
			}
			if (num2 > 0)
			{
				num2 = num2 * 2 / 3;
			}
			if (num + num3 > Screen.PrimaryScreen.Bounds.Width)
			{
				num = (Screen.PrimaryScreen.Bounds.Width - num3) / 2;
			}
			if (num2 + num4 > Screen.PrimaryScreen.Bounds.Height)
			{
				num2 = (Screen.PrimaryScreen.Bounds.Height - num4) / 2;
			}
			base.Top = num2;
			base.Left = num;
			base.Width = num3;
			base.Height = num4;
			if (FormState > 0)
			{
				base.WindowState = FormWindowState.Maximized;
			}
			splitContainerMain.SplitterDistance = splitContainerMain.Panel1MinSize;
			num5 = ((num5 < 0 || num5 > 1000) ? 520 : num5);
			int num7 = splitContainer1.Width * num5 / 1000;
			if (num7 < splitContainer1.Panel1MinSize)
			{
				num7 = splitContainer1.Panel1MinSize;
			}
			splitContainer1.SplitterDistance = num7;
			num6 = ((num6 >= 0 && num6 <= 1) ? num6 : 0);
			tabRightPane.SelectedIndex = num6;
			BuildTempItems();
			sArray = gf.SymbolsString.Split(',');
			LyricsVisited = false;
			InitMediaPlayer();
			BuildFolderList();
			ResetAll();
			SetWordWrap = ((RegUtil.GetRegValue("settings", Reg_FormWordWrap, 0) > 0) ? true : false);
			SetLyricsWordWrap(SetWordWrap);
			SetChordsMenu = ((RegUtil.GetRegValue("settings", Reg_FormSetChordsMenu, 0) > 0) ? true : false);
			SetMenuChordsMenu(SetChordsMenu);
			gf.InfoMainShowAllButtons = ((DataUtil.ObjToInt(RegUtil.GetRegValue("settings", "InfoMainShowAllButtons", 0)) > 0) ? true : false);
			ShowAllButtons(gf.InfoMainShowAllButtons);
			gf.BuildFontsList(ref ComboFontName);
			gf.BuildFontSizeList(ref ComboMainFontSize);
			gf.BuildFontSizeList(ref ComboNotationFontSize);
			ComboFontName.Text = gf.InfoMainFontName;
			ComboMainFontSize.Text = gf.InfoMainFontSize.ToString();
			ComboNotationFontSize.Text = gf.InfoNotationFontSize.ToString();
			InitFontsLists = false;
			ApplyFonts();
			EnableFontNameSize(EnableState: true);
			LoadLists();
			BuildEditHistoryMenuItems();
			EnableEditHistory();
			if (gf.InfoScreenFileName != "")
			{
				CurFileName = gf.InfoScreenFileName;
				if (DataUtil.Left(gf.InfoScreenFileName, "*NEW*".Length) == "*NEW*")
				{
					string InString = CurFileName;
					if (NewItem())
					{
						DataUtil.ExtractOneInfo(ref InString, '>');
						string inString = DataUtil.ExtractOneInfo(ref InString, '>');
						if (DataUtil.StringToInt(inString) < 1)
						{
							inString = "1";
						}
						SongFolder.Text = gf.FolderName[DataUtil.StringToInt(inString)];
						SongTitle.Text = DataUtil.ExtractOneInfo(ref InString, '>');
						tbLyrics1.Paste();
					}
				}
				else
				{
					LoadInfoScreen(CurFileName);
					AddToEditHistory("I" + CurFileName);
				}
			}
			tbLyrics1.DragEnter += tbLyrics_DragEnter;
			tbLyrics1.DragDrop += tbLyrics1_DragDrop;
			tbLyrics2.DragEnter += tbLyrics_DragEnter;
			tbLyrics2.DragDrop += tbLyrics2_DragDrop;
			TrackBarVolume.Value = (((gf.MediaVolume >= 0) & (gf.MediaVolume <= 100)) ? gf.MediaVolume : 20);
			TimerTrack.Start();
			TimerEditRequest.Start();
			BeginInvoke(new MethodInvoker(focustbLyrics1));
			InitLoad = false;
		}

		private void focustbLyrics1()
		{
			tbLyrics1.Focus();
		}

		private void BuildFolderList()
		{
			SongFolder.Items.Clear();
			for (int i = 1; i < 41; i++)
			{
				if (gf.FolderUse[i] > 0)
				{
					SongFolder.Items.Add(gf.FolderName[i]);
				}
			}
			if (SongFolder.Items.Count == 0)
			{
				SongFolder.Items.Add(gf.FolderName[1]);
			}
			SongFolder.Text = SongFolder.Items[0].ToString();
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

		/// <summary>
		/// daniel
		/// 확장자 docx 파일 추가
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void tbLyrics_DragEnter(object sender, DragEventArgs e)
		{
			string a = DragDropItemType(e);
			if (a == DataFormats.FileDrop)
			{
				bool flag = true;
				string[] array = (string[])e.Data.GetData(DataFormats.FileDrop);
				if (array == null)
				{
					flag = false;
				}
				else 
				{
					string strExt = Path.GetExtension(array[0]).ToLower();
					if ((strExt != ".doc") & (strExt != ".rtf") & (strExt != ".txt"))
					{
						flag = false;
					}
				}
				e.Effect = flag ? DragDropEffects.Copy : DragDropEffects.None;
			}
		}

		private string DragDropItemType(DragEventArgs e)
		{
			int num = e.Data.GetFormats().Length - 1;
			for (int i = 0; i <= num; i++)
			{
				if (e.Data.GetFormats()[i].Equals(DataFormats.FileDrop))
				{
					return DataFormats.FileDrop;
				}
			}
			return "";
		}

		private void tbLyrics1_DragDrop(object sender, DragEventArgs e)
		{
			if (e.Data.GetDataPresent(DataFormats.FileDrop))
			{
				string[] array = (string[])e.Data.GetData(DataFormats.FileDrop);
				GetExternalDocumentContents(array[0], 1);
				e.Effect = DragDropEffects.None;
			}
		}

		private void tbLyrics2_DragDrop(object sender, DragEventArgs e)
		{
			if (e.Data.GetDataPresent(DataFormats.FileDrop))
			{
				string[] array = (string[])e.Data.GetData(DataFormats.FileDrop);
				GetExternalDocumentContents(array[0], 2);
				e.Effect = DragDropEffects.None;
			}
		}

		private void R1Btn_Click(object sender, EventArgs e)
		{
			InsertingPresetItem = true;
			Button button = (Button)sender;
			gf.InsertIndicator(ref tbLyrics1, DataUtil.ObjToInt(button.Tag));
			InsertingPresetItem = false;
			Lyrics_TextChanged(1);
		}

		private void R2Btn_Click(object sender, EventArgs e)
		{
			InsertingPresetItem = true;
			Button button = (Button)sender;
			gf.InsertIndicator(ref tbLyrics2, DataUtil.ObjToInt(button.Tag));
			InsertingPresetItem = false;
			Lyrics_TextChanged(2);
		}

		private void EnableFontNameSize(bool EnableState)
		{
			ComboFontName.Enabled = EnableState;
			ComboMainFontSize.Enabled = EnableState;
			ComboNotationFontSize.Enabled = EnableState;
		}

		private void BuildTempItems()
		{
			tempSequenceCopied.Clear();
			tempSequenceCopied.Columns.Add("tempSequenceCopied1");
			tempSequenceCopied.Columns.Add("tempSequenceCopied2");
			ListViewNotations.Columns.Add("ListViewNotations1");
			ListViewNotations.Columns.Add("ListViewNotations2");
			ListViewNotations.Columns.Add("ListViewNotations3");
			ListViewNotations.Columns.Add("ListViewNotations4");
			ListViewNotations.Columns.Add("ListViewNotations5");
			ListViewNotations.View = View.Details;
			ListViewNotationLog.Columns.Add("ListViewNotationLog1");
			ListViewNotationLog.Columns.Add("ListViewNotationLog2");
			ListViewNotationLog.Columns.Add("ListViewNotationLog3");
			ListViewNotationLog.Columns.Add("ListViewNotationLog4");
			ListViewNotationLog.Columns.Add("ListViewNotationLog5");
			ListViewNotationLog.View = View.Details;
			ScreenBreaks1.Columns.Add("ScreenBreak1");
			ScreenBreaks1.Columns.Add("ScreenBreak2");
			tbWorkspace.WordWrap = false;
			tbTempSpace.WordWrap = false;
		}

		private void Menu_New_Click(object sender, EventArgs e)
		{
			NewItem();
		}

		private void Menu_Save_Click(object sender, EventArgs e)
		{
			SaveBtn_Click(UseDialog: false);
		}

		private void Menu_SaveAs_Click(object sender, EventArgs e)
		{
			SaveBtn_Click(UseDialog: true);
		}

		private void Menu_SaveExit_Click(object sender, EventArgs e)
		{
			SaveExitBtn_Click(UseDialog: false);
		}

		private void Menu_Import_Click(object sender, EventArgs e)
		{
			ExternalDocumentBtnPressed();
		}

		private void Menu_WordWrap_Click(object sender, EventArgs e)
		{
			SetLyricsWordWrap(Menu_WordWrap.Checked);
		}

		private void SetLyricsWordWrap(bool IsChecked)
		{
			SetWordWrap = IsChecked;
			Main_WordWrap.Checked = SetWordWrap;
			Menu_WordWrap.Checked = SetWordWrap;
			tbLyrics1.WordWrap = SetWordWrap;
			tbLyrics2.WordWrap = SetWordWrap;
		}

		private void Menu_Exit_Click(object sender, EventArgs e)
		{
			QuitEditor();
		}

		private void Menu_TransposeDown_Click(object sender, EventArgs e)
		{
			BtnChordsClick(-1);
		}

		private void Menu_TransposeUp_Click(object sender, EventArgs e)
		{
			BtnChordsClick(1);
		}

		private void Menu_ShowAllButtons_Click(object sender, EventArgs e)
		{
			ShowAllButtons(Menu_ShowAllButtons.Checked);
		}

		private void Main_New_Click(object sender, EventArgs e)
		{
			NewItem();
		}

		private void Main_Save_Click(object sender, EventArgs e)
		{
			SaveBtn_Click(UseDialog: false);
		}

		private void Main_SaveExit_Click(object sender, EventArgs e)
		{
			SaveExitBtn_Click(UseDialog: false);
		}

		private void Main_Import_Click(object sender, EventArgs e)
		{
			ExternalDocumentBtnPressed();
		}

		private void Main_WordWrap_Click(object sender, EventArgs e)
		{
			SetLyricsWordWrap(Main_WordWrap.Checked);
		}

		private void Main_TransposeDown_Click(object sender, EventArgs e)
		{
			BtnChordsClick(-1);
		}

		private void Main_TransposeUp_Click(object sender, EventArgs e)
		{
			BtnChordsClick(1);
		}

		private void LoadLists()
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
			SongKey.Items.Add("Gb");
			SongKey.Items.Add("Abm");
			SongKey.Items.Add("Bbm");
			SongKey.Items.Add("Dbm");
			SongKey.Items.Add("Ebm");
			SongKey.Items.Add("Gbm");
			SongKey.Items.Add("F#");
			SongKey.Items.Add("F#m");
			SongTiming.Items.Clear();
			SongTiming.Items.Add("");
			SongTiming.Items.Add("3/4");
			SongTiming.Items.Add("4/4");
			LicAdminInfo1.Items.Clear();
			LicAdminInfo1.Items.Add("");
			LicAdminInfo1.Items.Add(gf.LicAdmin_List[2, 0]);
			LicAdminInfo1.Items.Add(gf.LicAdmin_List[3, 0]);
			LicAdminInfo2.Items.Clear();
			LicAdminInfo2.Items.Add("");
			LicAdminInfo2.Items.Add(gf.LicAdmin_List[2, 0]);
			LicAdminInfo2.Items.Add(gf.LicAdmin_List[3, 0]);
			if (gf.LicAdmin_List[4, 0] != "")
			{
				LicAdminInfo1.Items.Add(gf.LicAdmin_List[4, 0]);
				LicAdminInfo2.Items.Add(gf.LicAdmin_List[4, 0]);
			}
			if (gf.LicAdmin_List[5, 0] != "")
			{
				LicAdminInfo1.Items.Add(gf.LicAdmin_List[5, 0]);
				LicAdminInfo2.Items.Add(gf.LicAdmin_List[5, 0]);
			}
			if (gf.LicAdmin_List[6, 0] != "")
			{
				LicAdminInfo1.Items.Add(gf.LicAdmin_List[6, 0]);
				LicAdminInfo2.Items.Add(gf.LicAdmin_List[6, 0]);
			}
		}

		private void ResetAll()
		{
			gf.InitialiseIndividualData(ref gf.InfoItem1);
			gf.InitialiseIndividualData(ref gf.InfoItem2);
			ThisItemFileName = "";
			CurFileName = "";
			SongTitle.Text = "";
			SongTitle2.Text = "";
			InitSongTitle2 = "";
			WriterInfo.Text = "";
			BookReference.Text = "";
			UserReference.Text = "";
			CopyrightInfo.Text = "";
			LicAdminInfo1.Text = "";
			LicAdminInfo2.Text = "";
			tbLyrics1.Text = "";
			tbLyrics2.Text = "";
			SongCapo.Text = "";
			SongKey.Text = "";
			SongTiming.Text = "";
			Lyrics1SavedNotations = "";
			Lyrics2SavedNotations = "";
			CombinedLyrics = "";
			CombinedNotations = "";
			VersesList.Items.Clear();
			OrderList.Items.Clear();
			Rotate_VersesList.Items.Clear();
			Rotate_OrderList.Items.Clear();
			Rotate_TimeTotal.MinDate = InitDateTime;
			Rotate_TimeTotal.Value = InitDateTime;
			CurMediaLength = 0.0;
			LabelDuration.Text = "00:00";
			if (PlayerOK)
			{
				DShowPlayer.newFilename = "";
			}
			OrderListSequence = "";
			LinkTitle2Pic.Visible = false;
			Title2IgnoreChange = false;
			for (int i = 0; i < 160; i++)
			{
				prevVersePresent[i] = false;
				VersePresent[i] = false;
			}
			tempSequenceCopied.Items.Clear();
			RotateString = "";
			Rotate_tbSourceLocation.Text = "";
			if (!Rotate_Equal.Checked)
			{
				Rotate_Equal.Checked = true;
			}
			else
			{
				MediaOption_Changed();
			}
			UpdateRotateTimePositions(1, 0, 0, "", UseRotateTimings: true, ResetAll: true);
			UpdateSavedStrings();
			ClearStack(0);
			toolStripStatusLabel1.Text = "";
		}

		private void UpdateSavedStrings()
		{
			Lyrics1SavedCopy = tbLyrics1.Text;
			Lyrics2SavedCopy = tbLyrics2.Text;
			SavedFileName = ThisItemFileName;
			SavedTitle = SongTitle.Text;
			SavedTitle2 = SongTitle2.Text;
			SavedSongNumber = SongNumber.Text;
			SavedBookReference = BookReference.Text;
			SavedUserReference = UserReference.Text;
			SavedWriterInfo = WriterInfo.Text;
			SavedCopyrightInfo = CopyrightInfo.Text;
			SavedLicAdminInfo1 = LicAdminInfo1.Text;
			SavedLicAdminInfo2 = LicAdminInfo2.Text;
			SavedCapo = SongCapo.Text;
			SavedSongKey = SongKey.Text;
			SavedSongTiming = SongTiming.Text;
			SavedFolder = gf.GetFolderNumber(SongFolder.Items[SongFolder.SelectedIndex].ToString()).ToString();
			SavedSequence = OrderListSequence;
			SavedRotateString = GenerateRotateString();
		}

		private void rtfMain_Change(int Region)
		{
			if (!IgnoreChange)
			{
				StackIndex[Region]++;
				if ((StackIndex[Region] == StackStartPoint[Region]) & (StackStartPoint[Region] > 0))
				{
					StackStartPoint[Region]++;
				}
				if (StackStartPoint[Region] > StackArrayMaxPoint)
				{
					StackStartPoint[Region] = 1;
				}
				if (StackIndex[Region] > StackArrayMaxPoint)
				{
					StackIndex[Region] = 0;
					StackStartPoint[Region] = 1;
				}
				StackMaxRedo[Region] = StackIndex[Region];
				iCursorPosition[Region, StackIndex[Region]] = ((Region == 1) ? tbLyrics1.SelectionStart : tbLyrics2.SelectionStart);
				sStack[Region, StackIndex[Region]] = ((Region == 1) ? tbLyrics1.Rtf : tbLyrics2.Rtf);
			}
		}

		private void ApplyFonts()
		{
			if (!InitFontsLists)
			{
				gf.InfoMainFontSize = Convert.ToInt32(ComboMainFontSize.Text);
				if ((gf.InfoMainFontSize < 6) | (gf.InfoMainFontSize > 20))
				{
					gf.InfoMainFontSize = 12;
				}
				gf.InfoNotationFontSize = Convert.ToInt32(ComboNotationFontSize.Text);
				if ((gf.InfoNotationFontSize < 6) | (gf.InfoNotationFontSize > 20))
				{
					gf.InfoNotationFontSize = 10;
				}
				MainFont = gf.GetNewFont(ComboFontName.Text, gf.InfoMainFontSize, InBold: false, InItalic: false, InUnderline: false, ShowErrorMsg: false);
				gf.InfoMainFontName = MainFont.Name;
				tbLyrics1.Font = MainFont;
				tbLyrics2.Font = MainFont;
				tbWorkspace.Font = MainFont;
				tbTempSpace.Font = MainFont;
				NotationFont = gf.GetNewFont(gf.InfoMainFontName, gf.InfoNotationFontSize, InBold: false, InItalic: false, InUnderline: false, ShowErrorMsg: false);
				RegUtil.SaveRegValue("options", "EditMainFontName", gf.InfoMainFontName);
				RegUtil.SaveRegValue("options", "EditMainFontSize", gf.InfoMainFontSize);
				RegUtil.SaveRegValue("options", "EditNotationFontSize", gf.InfoNotationFontSize);
			}
		}

		/// <summary>
		/// daniel
		/// 확장자 docx 추가
		/// </summary>
		private void ExternalDocumentBtnPressed()
		{
			OpenFileDialog1.Filter = "Word Documents and Text Files (*.doc;*.docx;*.txt) | *.doc;*.docx;*.txt|Word Documents (*.doc;*.docx)|*.doc;*.docx|Text Files (*.txt)|*.txt";
			OpenFileDialog1.InitialDirectory = gf.EditOpenDocumentDir;
			OpenFileDialog1.FileName = "";
			if (OpenFileDialog1.ShowDialog() == DialogResult.OK)
			{
				gf.EditOpenDocumentDir = Path.GetDirectoryName(OpenFileDialog1.FileName);
				RegUtil.SaveRegValue("options", "EditOpenDocumentDir", gf.EditOpenDocumentDir);
				string fileName = OpenFileDialog1.FileName;
				GetExternalDocumentContents(fileName, 3);
			}
		}

		/// <summary>
		/// daniel
		/// 확장자 docx 추가
		/// </summary>
		/// <param name="InFileName"></param>
		/// <param name="RegNum"></param>
		private void GetExternalDocumentContents(string InFileName, int RegNum)
		{
			Cursor = Cursors.WaitCursor;
			string OutText = "";
			string OutText2 = "";
			string inLyrics = "";
			string a = "";
			bool flag = false;

			string strExt = Path.GetExtension(InFileName).ToLower();
			switch (strExt)
			{
				case ".doc":
				case ".docx":
					inLyrics = gf.GetOfficeDocContents(InFileName);
					flag = true;
					break;
				case ".txt":
					inLyrics = gf.LoadTextFile(InFileName, ShowErrorMsg: true);
					flag = true;
					break;
			}

			if (flag)
			{
				gf.ExtractLyrics(inLyrics, "", ref OutText, ref Lyrics1SavedNotations, ref OutText2, ref Lyrics2SavedNotations);
				string text = "";
				string text2 = "";
				if (RegNum == 1 || RegNum == 3)
				{
					a = gf.GetDisplayNameOnly(ref InFileName, UpdateByRef: false);
					text = OutText;
					text2 = OutText2;
				}
				else if (OutText2 != "")
				{
					text = OutText;
					text2 = OutText2;
				}
				else
				{
					text2 = OutText;
				}
				if (RegNum == 3 || (tbLyrics1.Text != "" && text != "") || (tbLyrics2.Text != "" && text2 != ""))
				{
					if (!NewItem())
					{
						Cursor = Cursors.Default;
						return;
					}
					SongTitle.Text = gf.GetDisplayNameOnly(ref InFileName, UpdateByRef: false);
				}
				if (a != "")
				{
					SongTitle.Text = gf.GetDisplayNameOnly(ref InFileName, UpdateByRef: false);
				}
				if (text != "")
				{
					tbLyrics1.Text = gf.CombineLyricsAndNotations(text, Lyrics1SavedNotations, MainFont, NotationFont, ref tbWorkspace, ref tbTempSpace);
				}
				if (text2 != "")
				{
					tbLyrics2.Text = gf.CombineLyricsAndNotations(text2, Lyrics2SavedNotations, MainFont, NotationFont, ref tbWorkspace, ref tbTempSpace);
				}
			}
			Cursor = Cursors.Default;
		}

		private void SwitchChinese(int Region)
		{
			int num = -1;
			switch (Region)
			{
			case 1:
			{
				num = gf.SwitchChinese(ref tbLyrics1);
				string InString = SongTitle.Text;
				gf.SwitchChinese(ref InString, num);
				SongTitle.Text = InString;
				InString = SongTitle2.Text;
				gf.SwitchChinese(ref InString, num);
				SongTitle2.Text = InString;
				InString = WriterInfo.Text;
				gf.SwitchChinese(ref InString, num);
				WriterInfo.Text = InString;
				InString = CopyrightInfo.Text;
				gf.SwitchChinese(ref InString, num);
				CopyrightInfo.Text = InString;
				break;
			}
			case 2:
				gf.SwitchChinese(ref tbLyrics2);
				break;
			}
		}

		private void BtnChordsClick(int TransposeStep)
		{
			int flatSharpKey = -1;
			if (SongKey.Text != "")
			{
				string InKey = SongKey.Text;
				flatSharpKey = gf.TransposeKey(ref InKey, TransposeStep);
				SongKey.Text = InKey;
			}
			if (SongCapo.Text != "")
			{
				int selectedIndex = SongCapo.SelectedIndex;
				selectedIndex -= TransposeStep;
				if (selectedIndex > 12)
				{
					selectedIndex = (selectedIndex - 1) % 12 + 1;
				}
				else if (selectedIndex < 1)
				{
					selectedIndex = 13 - (selectedIndex + 1);
				}
				SongCapo.SelectedIndex = selectedIndex;
			}
			IgnoreChange = true;
			BtnChordsClick(TransposeStep, ref tbLyrics1, ref Lyrics1SavedNotations, ref Lyrics1Only, flatSharpKey);
			BtnChordsClick(TransposeStep, ref tbLyrics2, ref Lyrics2SavedNotations, ref Lyrics2Only, flatSharpKey);
			gf.ScanSelectedRTB(ref tbLyrics1, VersePresent, DoAll: true, 0, 0, sArray, MainFont, NotationFont, DoNotations: true);
			gf.ScanSelectedRTB(ref tbLyrics2, VersePresent, DoAll: true, 0, 0, sArray, MainFont, NotationFont, DoNotations: true);
			IgnoreChange = false;
		}

		private void BtnChordsClick(int TransposeStep, ref RichTextBox InTextBox, ref string SavedNotations, ref string LyricsOnly, int FlatSharpKey)
		{
			ValidateMusicNotations(ref InTextBox, ref SavedNotations, ref LyricsOnly);
			string text = SavedNotations;
			bool flag = false;
			string text2 = "";
			string text3 = "";
			for (int i = 0; i < text.Length; i++)
			{
				string text4 = DataUtil.Mid(text, i, 1);
				if ((text4 == "(") | (text4 == ")"))
				{
					text2 += text4;
				}
				else if (text4 == ';'.ToString())
				{
					text3 = gf.TransposeChord(text3, TransposeStep, FlatSharpKey);
					text2 = text2 + text3 + text4;
					text3 = "";
					flag = false;
				}
				else
				{
					text3 += text4;
				}
			}
			InTextBox.Text = gf.CombineLyricsAndNotations(LyricsOnly, text2, MainFont, NotationFont, ref tbWorkspace, ref tbTempSpace);
		}

		private bool ValidateMusicNotations(ref RichTextBox InTextBox, ref string InSavedNotations, ref string InLyricsOnly)
		{
			InSavedNotations = "";
			InLyricsOnly = InTextBox.Text;
			if (InTextBox.Text.IndexOf("»") < 0)
			{
				return true;
			}
			int num = DataUtil.CountLf(InTextBox.Text);
			if (num < 1)
			{
				return true;
			}
			int num2 = 0;
			ListViewItem listViewItem = new ListViewItem();
			BuildListofNotationLines(ref InTextBox, ref ListViewNotations, ref InLyricsOnly, num);
			bool flag = false;
			int num3 = num - 1;
			bool flag2 = false;
			for (int num4 = num - 1; num4 >= 0; num4--)
			{
				if (ListViewNotations.Items[num4].SubItems[1].Text == "" && num4 > 0)
				{
					if (ListViewNotations.Items[num4 - 1].SubItems[1].Text == "»")
					{
						ListViewNotations.Items[num4].SubItems[2].Text = Convert.ToString(num4 - 1);
					}
					else
					{
						flag2 = true;
					}
				}
				else
				{
					flag2 = true;
				}
				if (flag2)
				{
					ListViewNotations.Items[num4].Remove();
				}
				flag2 = false;
			}
			int InMin = 0;
			int InMax = 0;
			int InMin2 = 0;
			int InMax2 = 0;
			int num5 = 0;
			int num6 = 0;
			string MusicNotationName = "";
			Point MusicNotationCoOrd = new Point(0, 0);
			InSavedNotations = "";
			for (int num4 = 0; num4 <= ListViewNotations.Items.Count - 1; num4++)
			{
				ListViewNotationLog.Items.Clear();
				gf.GetMinMaxfromTextBox(InTextBox, Convert.ToInt32(ListViewNotations.Items[num4].SubItems[2].Text), ref InMin, ref InMax);
				gf.GetMinMaxfromTextBox(InTextBox, Convert.ToInt32(ListViewNotations.Items[num4].Text), ref InMin2, ref InMax2);
				num2 = Convert.ToInt32(ListViewNotations.Items[num4].SubItems[2].Text) - num4;
				num6 = 0;
				while (FindNextNotation(InTextBox, ref num6, ref MusicNotationName, ref MusicNotationCoOrd, InMin, InMax))
				{
					int associatedLyricsLineCurPos = GetAssociatedLyricsLineCurPos(ref InTextBox, MusicNotationCoOrd.X, InMin2, InMax2);
					listViewItem = ListViewNotationLog.Items.Add(MusicNotationName);
					listViewItem.SubItems.Add(Convert.ToString(associatedLyricsLineCurPos));
				}
				object obj = InSavedNotations;
				InSavedNotations = string.Concat(obj, "(", Convert.ToString(num2), ';');
				for (int i = 0; i <= ListViewNotationLog.Items.Count - 1; i++)
				{
					obj = InSavedNotations;
					InSavedNotations = string.Concat(obj, ListViewNotationLog.Items[i].Text, ';', ListViewNotationLog.Items[i].SubItems[1].Text, ';');
				}
				InSavedNotations += ")";
			}
			return true;
		}

		public void BuildListofNotationLines(ref RichTextBox InTextBox, ref ListView StoreNotationsList, ref string InLyricsOnly, int TotalLines)
		{
			ListViewItem listViewItem = new ListViewItem();
			int InMin = 0;
			int InMax = 0;
			InLyricsOnly = "";
			StoreNotationsList.Items.Clear();
			for (int i = 0; i <= TotalLines - 1; i++)
			{
				string text = InTextBox.Lines[i];
				bool flag = (text.IndexOf("»") >= 0) ? true : false;
				if (flag)
				{
					if (i < TotalLines - 1)
					{
						if (InTextBox.Lines[i + 1].IndexOf("»") >= 0)
						{
							flag = false;
						}
						else if (DataUtil.TrimEnd(InTextBox.Lines[i + 1]) == "")
						{
							flag = false;
						}
					}
					else
					{
						flag = false;
					}
				}
				listViewItem = StoreNotationsList.Items.Add(Convert.ToString(i));
				if (flag)
				{
					listViewItem.SubItems.Add("»");
					listViewItem.SubItems.Add("");
					continue;
				}
				listViewItem.SubItems.Add("");
				listViewItem.SubItems.Add("");
				gf.GetMinMaxfromTextBox(InTextBox, Convert.ToInt32(i), ref InMin, ref InMax);
				text = DataUtil.Mid(InTextBox.Text, InMin, InMax - InMin + 1);
				InLyricsOnly = InLyricsOnly + text + ((i < TotalLines - 1) ? "\n" : "");
			}
			InLyricsOnly = InLyricsOnly.Replace(" »", "");
			InLyricsOnly = InLyricsOnly.Replace(" »", "");
			InLyricsOnly = InLyricsOnly.Replace("»", "");
		}

		public void oldGetMinMaxfromTextBox(RichTextBox InBox, int InLineNumber, ref int InMin, ref int InMax)
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

		public bool FindNextNotation(RichTextBox IntextBox, ref int StartMusicCurPos, ref string MusicNotationName, ref Point MusicNotationCoOrd, int MusicCurPosMin, int MusicCurPosMax)
		{
			bool flag = false;
			IntextBox.SelectionLength = 0;
			MusicNotationName = "";
			if (StartMusicCurPos > MusicCurPosMax - MusicCurPosMin + 1)
			{
				return false;
			}
			for (int i = StartMusicCurPos; i <= MusicCurPosMax - MusicCurPosMin + 1; i++)
			{
				string text = DataUtil.Mid(IntextBox.Text, MusicCurPosMin + i, 1);
				if ((text != " ") & (text != "»"))
				{
					if (!flag)
					{
						flag = true;
						int index = IntextBox.SelectionStart = MusicCurPosMin + i;
						MusicNotationCoOrd = IntextBox.GetPositionFromCharIndex(index);
						int x = MusicNotationCoOrd.X;
						text = text;
					}
					MusicNotationName += text;
				}
				else if (flag)
				{
					StartMusicCurPos = i;
					return true;
				}
			}
			return false;
		}

		public int GetAssociatedLyricsLineCurPos(ref RichTextBox IntextBox, int MusicNotationCoOrdX, int LyricsCurPosMin, int LyricsCurPosMax)
		{
			int num = 0;
			int num2 = num;
			for (num2 = num; num2 <= LyricsCurPosMax - LyricsCurPosMin + 1; num2++)
			{
				if (IntextBox.GetPositionFromCharIndex(num2 + LyricsCurPosMin).X <= MusicNotationCoOrdX)
				{
					num = num2;
					continue;
				}
				return num;
			}
			return num2 - 1;
		}

		private string OldCombineLyricsAndNotations(string InLyrics, string InNotations)
		{
			if ((InNotations == "") | (InLyrics == ""))
			{
				return InLyrics;
			}
			StringBuilder stringBuilder = new StringBuilder();
			tbWorkspace.Text = InLyrics;
			gf.MarkSelectedRTB(ref tbWorkspace, 0, tbWorkspace.Text.Length, 0, MainFont, NotationFont);
			int num = DataUtil.CountLf(tbWorkspace.Text);
			int InMin = 0;
			int InMax = 0;
			string text = "";
			int num2 = gf.ListNotationData(InNotations, ref gf.NotationsArray, num);
			for (int i = 0; i < num; i++)
			{
				if (num2 > 0 && gf.NotationsArray[i] != "")
				{
					gf.GetMinMaxfromTextBox(tbWorkspace, i, ref InMin, ref InMax);
					tbTempSpace.Text = "";
					string text2 = "";
					while (gf.NotationsArray[i].Length > 0)
					{
						text = gf.NotationsArray[i];
						string text3 = DataUtil.ExtractOneInfo(ref text, ';');
						int inCurPos = Convert.ToInt32(DataUtil.ExtractOneInfo(ref text, ';'));
						gf.NotationsArray[i] = text;
						int associatedLyricsLineCurPosX = gf.GetAssociatedLyricsLineCurPosX(ref tbWorkspace, inCurPos, InMin, InMax);
						while (gf.GetAssociatedLyricsLineCurPosX(ref tbTempSpace, tbTempSpace.Text.Length - 1) < associatedLyricsLineCurPosX - 1)
						{
							text2 += " ";
							tbTempSpace.Text = text2;
							gf.MarkSelectedRTB(ref tbTempSpace, 0, tbTempSpace.Text.Length, 2, MainFont, NotationFont);
						}
						text2 += (((text2.Length > 1) & (DataUtil.Right(text2, 1) != " ")) ? (" " + text3) : text3);
						tbTempSpace.Text = text2;
						gf.MarkSelectedRTB(ref tbTempSpace, 0, tbTempSpace.Text.Length, 2, MainFont, NotationFont);
					}
					stringBuilder.Append(tbTempSpace.Text + " »\n");
				}
				stringBuilder.Append(tbWorkspace.Lines[i] + "\n");
			}
			if (DataUtil.Right(stringBuilder.ToString(), 1) == "\n")
			{
				return DataUtil.Left(stringBuilder.ToString(), stringBuilder.Length - 1);
			}
			return stringBuilder.ToString();
		}

		private void EnableEditHistory()
		{
			gf.LoadRegistryEditorEditHistory();
			UpdateMenu_EditHistory();
		}

		private void AddToEditHistory(string InItemID)
		{
			if (!((gf.GetItemTitle(InItemID) == "") | (gf.InfoScreenEditHistoryList[1, 0] == InItemID)))
			{
				if (gf.TotalInfoScreenEditHistory < gf.MaxUserEditHistory)
				{
					gf.TotalInfoScreenEditHistory++;
				}
				else
				{
					gf.TotalInfoScreenEditHistory = gf.MaxUserEditHistory;
				}
				for (int num = gf.TotalInfoScreenEditHistory; num >= 2; num--)
				{
					gf.InfoScreenEditHistoryList[num, 0] = gf.InfoScreenEditHistoryList[num - 1, 0];
				}
				gf.InfoScreenEditHistoryList[1, 0] = InItemID;
				gf.RemoveDuplicateEditorHistoryItems(ref gf.InfoScreenEditHistoryList, ref gf.TotalInfoScreenEditHistory);
				UpdateMenu_EditHistory();
				gf.SaveEditorEditHistoryToRegistry();
			}
		}

		private void UpdateMenu_EditHistory()
		{
			try
			{
				int num = 0;
				string text = "";
				if ((gf.TotalInfoScreenEditHistory < 0) | (gf.TotalInfoScreenEditHistory > gf.AbsoluteMaxHitoryItems))
				{
					gf.TotalInfoScreenEditHistory = gf.AbsoluteMaxHitoryItems;
				}
				for (int i = 1; i <= gf.TotalInfoScreenEditHistory; i++)
				{
					text = gf.GetItemTitle(gf.InfoScreenEditHistoryList[i, 0]);
					if (text != "" && gf.InfoScreenEditHistoryList[num, 0] != gf.InfoScreenEditHistoryList[i, 0])
					{
						num++;
						gf.InfoScreenEditHistoryList[num, 0] = gf.InfoScreenEditHistoryList[i, 0];
						gf.InfoScreenEditHistoryList[num, 1] = text;
					}
				}
				gf.TotalInfoScreenEditHistory = num;
				for (int i = gf.TotalInfoScreenEditHistory + 1; i <= gf.AbsoluteMaxHitoryItems; i++)
				{
					gf.InfoScreenEditHistoryList[i, 0] = "";
					gf.InfoScreenEditHistoryList[i, 1] = "";
				}
				for (int i = 1; i <= gf.AbsoluteMaxHitoryItems; i++)
				{
					Menu_EditHistoryList.DropDownItems[i - 1].Text = i + " " + gf.InfoScreenEditHistoryList[i, 1];
					Menu_EditHistoryList.DropDownItems[i - 1].Visible = ((i <= gf.TotalInfoScreenEditHistory) ? true : false);
				}
			}
			catch
			{
			}
		}

		private bool NewItem()
		{
			ClearErrorMessage(0);
			if (ActionBeforeNextEvent() == DialogResult.Cancel)
			{
				return false;
			}
			ResetAll();
			gf.InfoScreenFileName = "";
			return true;
		}

		private void QuitEditor()
		{
			ClearErrorMessage(0);
			Save_FormPos_To_Registry();
			if (ActionBeforeNextEvent() != DialogResult.Cancel)
			{
				FormCanClose = true;
				Close();
			}
		}

		private void ShowMessage(int Region, string MsgText)
		{
			if ((Region == 0) | (Region == 1))
			{
				LabeltbLyrics.Font = new Font(LabeltbLyrics.Font, FontStyle.Bold);
				LabeltbLyrics.BackColor = Color.Yellow;
				LabeltbLyrics.Text = MsgText;
			}
			if ((Region == 0) | (Region == 2))
			{
				LabeltbLyrics2.Font = new Font(LabeltbLyrics2.Font, FontStyle.Bold);
				LabeltbLyrics2.BackColor = Color.Yellow;
				LabeltbLyrics2.Text = MsgText;
			}
		}

		private void ClearErrorMessage(int Region)
		{
			if ((Region == 0) | (Region == 1))
			{
				LabeltbLyrics.Font = new Font(LabeltbLyrics.Font, FontStyle.Regular);
				LabeltbLyrics.BackColor = label2.BackColor;
				LabeltbLyrics.Text = "Region 1" + (SetRightToLeft1 ? " : Right-To-Left Text On" : "");
				ScreenBreak1Available = false;
			}
			if ((Region == 0) | (Region == 2))
			{
				LabeltbLyrics2.Font = new Font(LabeltbLyrics2.Font, FontStyle.Regular);
				LabeltbLyrics2.BackColor = label2.BackColor;
				LabeltbLyrics2.Text = "Region 2" + (SetRightToLeft2 ? " : Right-To-Left Text On" : "");
				ScreenBreak2Available = false;
			}
		}

		private void Save_FormPos_To_Registry()
		{
			if (base.WindowState == FormWindowState.Maximized)
			{
				RegUtil.SaveRegValue("settings", Reg_FormMax, 1);
				RegUtil.SaveRegValue("settings", Reg_FormWordWrap, tbLyrics1.WordWrap ? 1 : 0);
				RegUtil.SaveRegValue("settings", Reg_FormSetChordsMenu, SetChordsMenu ? 1 : 0);
				RegUtil.SaveRegValue("settings", Reg_FormRegion2Tab, tabRightPane.SelectedIndex);
			}
			else if (base.WindowState != FormWindowState.Maximized)
			{
				RegUtil.SaveRegValue("settings", Reg_FormMax, 0);
				RegUtil.SaveRegValue("settings", Reg_FormLeft, base.Left);
				RegUtil.SaveRegValue("settings", Reg_FormTop, base.Top);
				RegUtil.SaveRegValue("settings", Reg_FormWidth, base.Width);
				RegUtil.SaveRegValue("settings", Reg_FormHeight, base.Height);
				RegUtil.SaveRegValue("settings", Reg_FormWordWrap, tbLyrics1.WordWrap ? 1 : 0);
				RegUtil.SaveRegValue("settings", Reg_FormSetChordsMenu, SetChordsMenu ? 1 : 0);
				RegUtil.SaveRegValue("settings", Reg_FormLyricsSplit, splitContainer1.SplitterDistance * 1000 / splitContainer1.Width);
				RegUtil.SaveRegValue("settings", Reg_FormRegion2Tab, tabRightPane.SelectedIndex);
			}
		}

		private DialogResult ActionBeforeNextEvent()
		{
			if (!ChangesMade())
			{
				return DialogResult.Yes;
			}
			string text = "Do you wish to save the current InfoScreen?";
			switch (MessageBox.Show(text, "", MessageBoxButtons.YesNoCancel))
			{
			case DialogResult.Yes:
				SaveBtn_Click(UseDialog: false);
				if (!ChangesMade())
				{
					return DialogResult.Yes;
				}
				return DialogResult.Cancel;
			case DialogResult.No:
				return DialogResult.No;
			default:
				return DialogResult.Cancel;
			}
		}

		private bool ChangesMade()
		{
			if (tbLyrics1.Text != Lyrics1SavedCopy || tbLyrics2.Text != Lyrics2SavedCopy || ThisItemFileName != SavedFileName || SongTitle.Text != SavedTitle || SongTitle2.Text != SavedTitle2 || SongNumber.Text != SavedSongNumber || BookReference.Text != SavedBookReference || UserReference.Text != SavedUserReference || WriterInfo.Text != SavedWriterInfo || CopyrightInfo.Text != SavedCopyrightInfo || LicAdminInfo1.Text != SavedLicAdminInfo1 || LicAdminInfo2.Text != SavedLicAdminInfo2 || SongKey.Text != SavedSongKey || SongTiming.Text != SavedSongTiming || SongCapo.Text != SavedCapo || GenerateRotateString() != SavedRotateString || gf.GetFolderNumber(SongFolder.Items[SongFolder.SelectedIndex].ToString()) != DataUtil.StringToInt(SavedFolder) || OrderListSequence != SavedSequence)
			{
				return true;
			}
			return false;
		}

		private bool SaveBtn_Click(bool UseDialog)
		{
			ClearErrorMessage(0);
			Cursor = Cursors.WaitCursor;
			if (ValidateAllDetails())
			{
				if (SaveDataToFile(UseDialog))
				{
					UpdateSavedStrings();
					gf.InfoScreenItemNew = ((!(gf.InfoScreenFileName == CurFileName)) ? true : false);
					gf.InfoScreenFileName = CurFileName;
					AddToEditHistory("I" + gf.InfoScreenFileName);
					gf.InfoScreenAction = InfoType.Save;
					gf.InfoScreenLoadItem = true;
					toolStripStatusLabel1.Text = " " + gf.InfoScreenFileName;
					Cursor = Cursors.Default;
					return true;
				}
				Cursor = Cursors.Default;
				return false;
			}
			Cursor = Cursors.Default;
			return false;
		}

		private void SaveExitBtn_Click(bool UseDialog)
		{
			if (SaveBtn_Click(UseDialog: false))
			{
				QuitEditor();
			}
		}

		private bool SaveDataToFile(bool UseDialog)
		{
			gf.InfoItem1.Title = SongTitle.Text;
			gf.InfoItem1.FolderNo = gf.GetFolderNumber(SongFolder.Text);
			gf.InfoItem1.Title2 = SongTitle2.Text;
			gf.InfoItem1.SongNumber = DataUtil.StringToInt(SongNumber.Text);
			gf.InfoItem1.CompleteLyrics = CombinedLyrics;
			gf.InfoItem1.Notations = CombinedNotations;
			gf.InfoItem1.SongSequence = OrderListSequence;
			gf.InfoItem1.Writer = WriterInfo.Text;
			gf.InfoItem1.Copyright = CopyrightInfo.Text;
			gf.InfoItem1.Category = "";
			gf.InfoItem1.Timing = SongTiming.Text;
			gf.InfoItem1.MusicKey = SongKey.Text;
			gf.InfoItem1.Capo = SongCapo.SelectedIndex - 1;
			gf.InfoItem1.Show_LicAdminInfo1 = LicAdminInfo1.Text;
			gf.InfoItem1.Show_LicAdminInfo2 = LicAdminInfo2.Text;
			gf.InfoItem1.Book_Reference = BookReference.Text;
			gf.InfoItem1.User_Reference = UserReference.Text;
			gf.InfoItem1.RotateString = GenerateRotateString();
			gf.InfoItem1.Settings = gf.CombineSettings(gf.InfoItem1);
			return SaveInfoScreenFileName(UseDialog);
		}

		private bool SaveInfoScreenFileName(bool UseDialog)
		{
			string text = CurFileName;
			string initialDirectory = "";
			try
			{
				initialDirectory = Path.GetDirectoryName(CurFileName);
			}
			catch
			{
				if (PreviousFileDir == "")
				{
					initialDirectory = gf.InfoScreenDir;
				}
			}
			try
			{
				if (CurFileName == "" || UseDialog)
				{
					saveFileDialog1.Filter = "EasiSlides InfoScreens (*.esi)|*.esi";
					saveFileDialog1.Title = "Save InfoScreen";
					saveFileDialog1.InitialDirectory = initialDirectory;
					saveFileDialog1.FileName = ThisItemFileName;
					saveFileDialog1.OverwritePrompt = true;
					saveFileDialog1.AddExtension = true;
					saveFileDialog1.DefaultExt = ".esi";
					if (saveFileDialog1.ShowDialog() != DialogResult.OK)
					{
						return false;
					}
					text = saveFileDialog1.FileName;
					if (text != "")
					{
						PreviousFileDir = Path.GetDirectoryName(text);
					}
				}
			}
			catch
			{
				ShowMessage(1, "Error encountered - InfoScreen NOT saved. Please make sure you have write access.");
				return false;
			}
			try
			{
				gf.SaveXMLInfoScreen(gf.InfoItem1, text, Info_HeaderData, gf.InfoScreenLoadNewBackground, UseOriginalNotations: false);
				ShowMessage(1, "InfoScreen saved");
				CurFileName = text;
				ThisItemFileName = gf.GetDisplayNameOnly(ref CurFileName, UpdateByRef: false);
				return true;
			}
			catch
			{
				ShowMessage(1, "Error encountered - InfoScreen NOT saved. Please make sure you have write access.");
				return false;
			}
		}

		private void LoadInfoScreen(string InFileName)
		{
			ResetAll();
			SongLyrics = "";
			string OutText = "";
			string OutText2 = "";
			Title2IgnoreChange = true;
			gf.LoadInfoFile(InFileName, ref gf.InfoItem1, ref Info_HeaderData);
			gf.InfoItem1.Show_LicAdminInfo1 = gf.InfoItem1.In_LicAdminInfo1;
			gf.InfoItem1.Show_LicAdminInfo2 = gf.InfoItem1.In_LicAdminInfo2;
			ThisItemFileName = gf.GetDisplayNameOnly(ref InFileName, UpdateByRef: false);
			int num = DataUtil.StringToInt(DataUtil.ExtractOneInfo(ref Info_HeaderData[31], '>', RemoveExtract: false, MinusOneIfBlank: false));
			SongTitle.Text = gf.InfoItem1.Title;
			SongFolder.Text = gf.FolderName[gf.InfoItem1.FolderNo];
			Title2IgnoreChange = true;
			SongTitle2.Text = gf.InfoItem1.Title2;
			SongNumber.Text = gf.InfoItem1.SongNumber.ToString();
			BookReference.Text = gf.InfoItem1.Book_Reference;
			UserReference.Text = gf.InfoItem1.User_Reference;
			InitSongTitle2 = SongTitle.Text;
			CopyrightInfo.Text = gf.InfoItem1.Copyright;
			LicAdminInfo1.Text = gf.InfoItem1.Show_LicAdminInfo1;
			LicAdminInfo2.Text = gf.InfoItem1.Show_LicAdminInfo2;
			WriterInfo.Text = gf.InfoItem1.Writer;
			OrderListSequence = gf.InfoItem1.SongSequence;
			string text = gf.InfoItem1.Capo.ToString();
			SongCapo.Text = (((text == "") | (text == "-1")) ? "" : ("Capo " + text));
			if (SongCapo.Text == "")
			{
				SongCapo.SelectedIndex = 0;
			}
			SongTiming.Text = gf.InfoItem1.Timing;
			SongKey.Text = gf.InfoItem1.MusicKey;
			Lyrics1SavedNotations = gf.InfoItem1.Notations;
			string completeLyrics = gf.InfoItem1.CompleteLyrics;
			gf.ExtractLyrics(completeLyrics, Lyrics1SavedNotations, ref OutText, ref Lyrics1SavedNotations, ref OutText2, ref Lyrics2SavedNotations);
			InitLoad = true;
			tbLyrics1.Text = gf.CombineLyricsAndNotations(OutText, Lyrics1SavedNotations, MainFont, NotationFont, ref tbWorkspace, ref tbTempSpace);
			tbLyrics2.Text = gf.CombineLyricsAndNotations(OutText2, Lyrics2SavedNotations, MainFont, NotationFont, ref tbWorkspace, ref tbTempSpace);
			InitLoad = false;
			UpdateVersesList();
			IgnoreChange = true;
			gf.ScanSelectedRTB(ref tbLyrics1, VersePresent, DoAll: true, 0, 0, sArray, MainFont, NotationFont, DoNotations: true);
			gf.ScanSelectedRTB(ref tbLyrics2, VersePresent, DoAll: true, 0, 0, sArray, MainFont, NotationFont, DoNotations: true);
			IgnoreChange = false;
			RotateString = gf.InfoItem1.RotateString;
			OrderList.Items.Clear();
			Rotate_OrderList.Items.Clear();
			int num2 = 1;
			int num3 = 0;
			Rotate_tbSourceLocation.Text = gf.GetMediaFileName(ThisItemFileName, SongTitle2.Text);
			ListViewItem listViewItem = new ListViewItem();
			if (OrderListSequence.Length > 0)
			{
				for (int i = 0; i < OrderListSequence.Length; i++)
				{
					int num4 = OrderListSequence[i];
					listViewItem = OrderList.Items.Add(gf.VerseTitle[num4]);
					listViewItem.SubItems.Add(num4.ToString());
				}
			}
			if (gf.InfoItem1.RotateSequence.Length > 0)
			{
				try
				{
					for (int i = 0; i < gf.InfoItem1.RotateSequence.Length; i++)
					{
						int num4 = gf.InfoItem1.RotateSequence[i];
						listViewItem = Rotate_OrderList.Items.Add(gf.VerseTitle[num4]);
						listViewItem.SubItems.Add(num4.ToString());
					}
				}
				catch
				{
				}
			}
			UpdateRotateTimePositions(gf.InfoItem1.RotateStyle, gf.InfoItem1.RotateGap, gf.InfoItem1.RotateTotal, gf.InfoItem1.RotateTimings, UseRotateTimings: true, ResetAll: false);
			UpdateSavedStrings();
			Title2IgnoreChange = false;
			SongTitle2_Change();
			tbLyrics1.Focus();
			tbLyrics1.SelectionLength = 0;
			StackTrackPos[1, 0] = 0;
			StackTrackPos[1, 1] = 0;
			StackTrackPos[2, 0] = 0;
			StackTrackPos[2, 1] = 0;
			ClearStack(0);
			CurFileName = InFileName;
			toolStripStatusLabel1.Text = " " + CurFileName;
		}

		private void SongTitle2_Change()
		{
			if (Title2IgnoreChange)
			{
				return;
			}
			if (DataUtil.Trim(SongTitle2.Text) == "")
			{
				LinkTitle2Pic.Visible = false;
			}
			else
			{
				bool flag = false;
				try
				{
#if DAO
					string fullSearchString = "select * from SONG where LCase(Title_1) like \"" + SongTitle2.Text.ToLower() + "\"" + SQLFolderLookUp;
					using DataTable datatable = DbOleDbController.GetDataTable(gf.ConnectStringMainDB, fullSearchString);
#elif SQLite
					string fullSearchString = "select * from SONG where lower(Title_1) like \"" + SongTitle2.Text.ToLower() + "\"" + SQLFolderLookUp;
					using DataTable datatable = DbController.GetDataTable(gf.ConnectStringMainDB, fullSearchString);
#endif

					if (datatable.Rows.Count>0)
					{
						//recordSet.MoveFirst();
						LinkTitle2Pic.Visible = true;
						Title2IgnoreChange = true;
						SongTitle2.Text = DataUtil.ObjToString(datatable.Rows[0]["Title_1"]);
						Title2IgnoreChange = false;
						flag = true;
					}
				}
				catch
				{
				}
				if (!flag)
				{
					LinkTitle2Pic.Visible = false;
				}
			}
			ClearErrorMessage(0);
		}

		private bool ValidateAllDetails()
		{
			if (!ValidateTitles())
			{
				return false;
			}
			if (!ValidateSequence())
			{
				return false;
			}
			int selectionStart = tbLyrics1.SelectionStart;
			int selectionStart2 = tbLyrics2.SelectionStart;
			if (!ValidateContents())
			{
				return false;
			}
			if (!ValidateMusicNotations(ref tbLyrics1, ref Lyrics1SavedNotations, ref Lyrics1Only))
			{
				return false;
			}
			if (!ValidateMusicNotations(ref tbLyrics2, ref Lyrics2SavedNotations, ref Lyrics2Only))
			{
				return false;
			}
			tbLyrics1.SelectionStart = selectionStart;
			tbLyrics1.SelectionLength = 0;
			tbLyrics1.ScrollToCaret();
			tbLyrics2.SelectionStart = selectionStart2;
			tbLyrics2.SelectionLength = 0;
			tbLyrics2.ScrollToCaret();
			gf.InfoItem1.CompleteLyrics = Lyrics1Only;
			gf.InfoItem1.Notations = Lyrics1SavedNotations;
			gf.InfoItem2.CompleteLyrics = Lyrics2Only;
			gf.InfoItem2.Notations = Lyrics2SavedNotations;
			gf.Merge_Songs(gf.InfoItem1, gf.InfoItem2, ref CombinedLyrics, ref CombinedNotations);
			return true;
		}

		private bool ValidateTitles()
		{
			string text = "";
			if (!gf.ValidateTitleDetails(SongTitle.Text, "Title"))
			{
				return false;
			}
			if (!gf.ValidateTitleDetails(SongTitle2.Text, "Link Title"))
			{
				return false;
			}
			if (!gf.ValidateTitleDetails(CopyrightInfo.Text, "Copyright Info"))
			{
				return false;
			}
			if (!gf.ValidateTitleDetails(BookReference.Text, "Book Reference Info"))
			{
				return false;
			}
			if (!gf.ValidateTitleDetails(WriterInfo.Text, "Writer Info"))
			{
				return false;
			}
			if (SongNumber.Text != "")
			{
				int num = DataUtil.StringToInt(SongNumber.Text, Minus1IfBlank: true);
				if (num < 0)
				{
					MessageBox.Show("Song Number must be numeric and without spaces");
					return false;
				}
			}
			if ((tbLyrics1.TextLength < 1) & (tbLyrics2.TextLength < 1))
			{
				ShowMessage(1, "There are no contents to save!");
				return false;
			}
			return true;
		}

		private bool ValidateSequence()
		{
			for (int i = 0; i <= OrderList.Items.Count - 1; i++)
			{
				int num = DataUtil.StringToInt(OrderList.Items[i].SubItems[1].Text);
				if (!VersePresent[num])
				{
					if (num > 0 && num < 13)
					{
						ShowMessage(1, "Sequence contains Verse " + gf.VerseTitle[num] + " which is not in the lyrics!");
					}
					else
					{
						ShowMessage(1, "Sequence contains a " + gf.VerseTitle[num] + " which is not in the lyrics!");
					}
					i = OrderList.Items.Count - 1;
					return false;
				}
			}
			return true;
		}

		private bool ValidateContents()
		{
			if (!ValidateContents(ref tbLyrics1, 1))
			{
				return false;
			}
			if (!ValidateContents(ref tbLyrics2, 2))
			{
				return false;
			}
			return true;
		}

		private bool ValidateContents(ref RichTextBox InTextBox, int Region)
		{
			IgnoreChange = true;
			InTextBox.Text = InTextBox.Text.Replace("\r\n", "\n");
			if (Region == 1)
			{
				gf.ScanSelectedRTB(ref tbLyrics1, VersePresent, DoAll: true, 0, 0, sArray, MainFont, NotationFont, DoNotations: true);
			}
			else
			{
				gf.ScanSelectedRTB(ref tbLyrics2, VersePresent, DoAll: true, 0, 0, sArray, MainFont, NotationFont, DoNotations: true);
			}
			IgnoreChange = false;
			bool flag = true;
			bool flag2 = false;
			int num = 0;
			while (flag)
			{
				if (VersePresent[num])
				{
					int num2 = InTextBox.Text.IndexOf(gf.VerseSymbol[num]);
					if (num2 >= 0 && num2 != 0)
					{
						if (DataUtil.Mid(InTextBox.Text, num2 - 1, 1) != "\n")
						{
							gf.ClipboardPasteTextBox(InTextBox, num2 - 1, "\r\n\r\n");
							num2 = InTextBox.SelectionStart + 1;
						}
						num2 += gf.VerseSymbol[num].Length;
						if (DataUtil.Mid(InTextBox.Text, num2, 1) != "\n")
						{
							gf.ClipboardPasteTextBox(InTextBox, num2, "\r\n\r\n");
							num2 = InTextBox.SelectionStart + 1;
						}
					}
				}
				num++;
				if (num > 99 && num < 100)
				{
					num = 100;
				}
				if (num > 150)
				{
					flag = false;
				}
			}
			tbWorkspace.Text = InTextBox.Text.Replace("\r\n", "\n");
			int num3 = tbWorkspace.Text.IndexOf(gf.VerseSymbol[150]);
			if (num3 >= 0)
			{
				ShowMessage(Region, "REGION 2 indicator is not permitted - please remove");
				InTextBox.Focus();
				InTextBox.SelectionStart = num3;
				InTextBox.SelectionLength = gf.VerseSymbol[150].Length;
				return false;
			}
			flag = true;
			flag2 = false;
			num = 0;
			int num4 = -1;
			int num5 = -1;
			int num6 = 30000;
			int num7 = 0;
			while (num < 160 && flag)
			{
				if (VersePresent[num])
				{
					int num2 = tbWorkspace.Text.IndexOf(gf.VerseSymbol[num]);
					if (num2 >= 0)
					{
						if (num2 < num6)
						{
							num6 = num2;
							num5 = num;
						}
						if (num >= 0 && num <= 112)
						{
							int num8 = tbWorkspace.Text.IndexOf(gf.VerseSymbol[num], num2 + gf.VerseSymbol[num].Length);
							if (num8 >= 0)
							{
								flag2 = true;
							}
							if (num > 1 && num < 100)
							{
								if (!VersePresent[num - 1] && num4 < 0)
								{
									num4 = num - 1;
								}
								if (num7 < 1)
								{
									num7 = 1;
								}
							}
							else
							{
								num7 = num;
							}
						}
					}
				}
				if (flag2)
				{
					flag = false;
					continue;
				}
				num++;
				if (num > 99 && num < 100)
				{
					num = 100;
				}
			}
			if (flag2)
			{
				int num2 = -1;
				int num8 = -1;
				for (int i = 0; i <= tbWorkspace.TextLength; i++)
				{
					tbWorkspace.SelectionStart = i;
					tbWorkspace.SelectionLength = gf.VerseSymbol[num].Length;
					if (tbWorkspace.SelectedText == gf.VerseSymbol[num])
					{
						if (num2 < 0)
						{
							num2 = i;
							continue;
						}
						num8 = i;
						i = tbWorkspace.TextLength;
					}
				}
				if (num > 0 && num < 13)
				{
					ShowMessage(Region, "Duplicate Verse " + gf.VerseTitle[num] + " indicator found - please amend.");
				}
				else
				{
					ShowMessage(Region, "Duplicate " + gf.VerseTitle[num] + " indicator found - please amend.");
				}
				InTextBox.Focus();
				InTextBox.SelectionStart = num8;
				InTextBox.SelectionLength = gf.VerseSymbol[num].Length;
				return false;
			}
			if (num4 > 0)
			{
				ShowMessage(Region, "Verse " + num4 + " indicator is missing.");
				return false;
			}
			if (num7 > 0 && num6 != 0)
			{
				if (num5 > 0 && num5 < 13)
				{
					ShowMessage(Region, "Indicator is required for the lyrics before Verse " + num5);
				}
				else
				{
					ShowMessage(Region, "Indicator is required for the lyrics before the " + gf.VerseTitle[num5]);
				}
				return false;
			}
			ClearErrorMessage(Region);
			return true;
		}

		private void UpdateVersesList()
		{
			if (InitLoad)
			{
				return;
			}
			ListViewItem listViewItem = new ListViewItem();
			wArray = "";
			countb = -1L;
			VerseSymbolChanged = false;
			bool flag = false;
			int num = -1;
			for (int i = 0; i < 160; i++)
			{
				if (!((i <= 99) | (i >= 100 && i <= 112) | (i >= 150 && i < 152)))
				{
					continue;
				}
				num = tbLyrics1.Text.IndexOf(gf.VerseSymbol[i], 0);
				if (gf.VerseSymbol[i] != "" && num >= 0)
				{
					VersePresent[i] = true;
					VersePresentNewScreenCount[i] = CountVerseScreens(tbLyrics1.Text, num + 1);
					flag = true;
				}
				else
				{
					num = tbLyrics2.Text.IndexOf(gf.VerseSymbol[i], 0);
					if (gf.VerseSymbol[i] != "" && num >= 0)
					{
						VersePresent[i] = true;
						VersePresentNewScreenCount[i] = CountVerseScreens(tbLyrics2.Text, num + 1);
						flag = true;
					}
					else
					{
						VersePresent[i] = false;
						VersePresentNewScreenCount[i] = 0;
					}
				}
				if (VersePresent[i] != prevVersePresent[i])
				{
					VerseSymbolChanged = true;
				}
				wArray += (VersePresent[i] ? ("," + gf.VerseSymbol[i]) : "");
			}
			if (wArray != "")
			{
				wArray = DataUtil.Mid(wArray, 1);
			}
			sArray = wArray.Split(',');
			if (VerseSymbolChanged)
			{
				VersesList.Items.Clear();
				for (int i = 1; i <= 99; i++)
				{
					if (VersePresent[i])
					{
						listViewItem = VersesList.Items.Add(gf.VerseTitle[i]);
						listViewItem.SubItems.Add(i.ToString());
						listViewItem.SubItems.Add(VersePresentNewScreenCount[i].ToString());
					}
					prevVersePresent[i] = VersePresent[i];
				}
				if (VersePresent[0])
				{
					listViewItem = VersesList.Items.Add(gf.VerseTitle[0]);
					listViewItem.SubItems.Add(0.ToString());
					listViewItem.SubItems.Add(VersePresentNewScreenCount[0].ToString());
				}
				prevVersePresent[0] = VersePresent[0];
				for (int i = 100; i <= 112; i++)
				{
					if (VersePresent[i])
					{
						listViewItem = VersesList.Items.Add(gf.VerseTitle[i]);
						listViewItem.SubItems.Add(i.ToString());
						listViewItem.SubItems.Add(VersePresentNewScreenCount[0].ToString());
					}
					prevVersePresent[i] = VersePresent[i];
				}
			}
			if ((VersesList.Items.Count == 0 && (tbLyrics1.Text != "" || tbLyrics2.Text != "")) || (VersesList.Items.Count > 0 && (!VersePresent[1] || tbLyrics1.Text == "" || tbLyrics1.Text[0] != '[')))
			{
				VersePresentNewScreenCount[1] = CountVerseScreens(tbLyrics1.Text, 0);
				if (VersePresentNewScreenCount[1] < 1)
				{
					VersePresentNewScreenCount[1] = 1;
				}
				VersePresent[1] = true;
				listViewItem = VersesList.Items.Add(gf.VerseTitle[1]);
				listViewItem.SubItems.Add("1");
				listViewItem.SubItems.Add(VersePresentNewScreenCount[1].ToString());
				prevVersePresent[1] = VersePresent[1];
				flag = true;
			}
			CopyVerseListToRotateVerseList();
			if (flag || VerseSymbolChanged)
			{
				UpdateRotateTimePositions();
			}
		}

		private int CountVerseScreens(string InText, int StartIndex)
		{
			int num = InText.IndexOf('[', StartIndex);
			if (num < 0)
			{
				num = InText.Length;
			}
			int num2 = 1;
			int num3 = InText.IndexOf("\n\n", StartIndex);
			if (num3 >= 0)
			{
				while (num3 >= 0 && num3 < num)
				{
					num2++;
					num3 = ((num3 + 2 >= InText.Length) ? (-1) : InText.IndexOf("\n\n", num3 + 2));
				}
			}
			return num2;
		}

		private void tbLyrics_SelectionChanged(object sender, EventArgs e)
		{
			if (!IgnoreChange)
			{
				Lyrics_SelectionChanged(1);
			}
		}

		private void tbLyrics2_SelectionChanged(object sender, EventArgs e)
		{
			if (!IgnoreChange)
			{
				Lyrics_SelectionChanged(2);
			}
		}

		private void Lyrics_SelectionChanged(int Region)
		{
			StackTrackPos[Region, 0] = StackTrackPos[Region, 1];
			StackTrackPos[Region, 1] = ((Region == 1) ? tbLyrics1.SelectionStart : tbLyrics2.SelectionStart);
		}

		private void FrmInfoScreen_Resize(object sender, EventArgs e)
		{
			SetTopPanel();
			groupBox2.Left = base.Width - groupBox2.Width - 11;
			int num = groupBox2.Left - groupBox1.Left - 3;
			num = ((num > 0) ? num : 0);
			groupBox1.Width = num;
			if (groupBox1.Width < 443)
			{
				panel7.Width = 216;
			}
			else
			{
				panel7.Width = groupBox1.Width - 227;
			}
			panel8.Left = panel7.Left + panel7.Width;
			ResizeTitleFields();
			SetRotatePanel();
		}

		private void ResizeTitleFields()
		{
			SongTitle.Width = panel8.Left - SongTitle.Left - 7;
			WriterInfo.Width = SongTitle.Width;
			CopyrightInfo.Width = SongTitle.Width;
			SongTitle2.Width = SongTitle.Width - (panelLinkTitle2Lookup.Width + LinkTitle2Pic.Width) - 3;
			LinkTitle2Pic.Left = SongTitle.Left + SongTitle2.Width + 3;
			panelLinkTitle2Lookup.Left = LinkTitle2Pic.Left + LinkTitle2Pic.Width;
			int num = WriterInfo.Width - (SongFolder.Left - WriterInfo.Left);
			SongFolder.Width = ((num < 20) ? 20 : num);
			Btn_Title.Left = SongTitle.Left + SongTitle.Width - Btn_Title.Width;
			Btn_Title2.Left = SongTitle2.Left + SongTitle2.Width - Btn_Title2.Width;
			Btn_Writer.Left = WriterInfo.Left + WriterInfo.Width - Btn_Writer.Width;
			Btn_Copyright.Left = CopyrightInfo.Left + CopyrightInfo.Width - Btn_Copyright.Width;
			UserReference.Width = (Btn_UserRef.Visible ? (162 - Btn_UserRef.Width) : 162);
			BookReference.Width = (Btn_BookRef.Visible ? (162 - Btn_BookRef.Width) : 162);
			SongTitle.Width -= (Btn_Title.Visible ? Btn_Title.Width : 0);
			SongTitle2.Width -= (Btn_Title2.Visible ? Btn_Title2.Width : 0);
			WriterInfo.Width -= (Btn_Writer.Visible ? Btn_Writer.Width : 0);
			CopyrightInfo.Width -= (Btn_Copyright.Visible ? Btn_Copyright.Width : 0);
		}

		private void ComboFonts_SelectedIndexChanged(object sender, EventArgs e)
		{
			ApplyFonts();
		}

		private void tbLyrics1_TextChanged(object sender, EventArgs e)
		{
			if (!IgnoreChange)
			{
				Lyrics_TextChanged(1);
			}
		}

		private void tbLyrics2_TextChanged(object sender, EventArgs e)
		{
			if (!IgnoreChange)
			{
				Lyrics_TextChanged(2);
			}
		}

		private void Lyrics_TextChanged(int Region)
		{
			IgnoreChange = true;
			if ((tbLyrics1.Text != "") | (tbLyrics2.Text != ""))
			{
				if (ComboFontName.Enabled)
				{
					EnableFontNameSize(EnableState: false);
				}
			}
			else
			{
				EnableFontNameSize(EnableState: true);
			}
			ClearErrorMessage(Region);
			UpdateVersesList();
			IgnoreChange = false;
			if (!InsertingPresetItem)
			{
				rtfMain_Change(Region);
			}
			IgnoreChange = true;
			if (Region == 1)
			{
				gf.ScanSelectedRTB(ref tbLyrics1, VersePresent, DoAll: false, StackTrackPos[Region, 1], StackTrackPos[Region, 1], sArray, MainFont, NotationFont, DoNotations: true);
				tbLyrics1.Focus();
			}
			else
			{
				gf.ScanSelectedRTB(ref tbLyrics2, VersePresent, DoAll: false, StackTrackPos[Region, 1], StackTrackPos[Region, 1], sArray, MainFont, NotationFont, DoNotations: true);
				tbLyrics2.Focus();
			}
			IgnoreChange = false;
		}

		private void SongFolder_SelectedIndexChanged(object sender, EventArgs e)
		{
			int folderNumber = gf.GetFolderNumber(SongFolder.Items[SongFolder.SelectedIndex].ToString());
			try
			{
				SetRightToLeft1 = ((gf.ShowFontRTL[folderNumber, 0] > 0) ? true : false);
				SetRightToLeft2 = ((gf.ShowFontRTL[folderNumber, 1] > 0) ? true : false);
			}
			catch
			{
				SetRightToLeft1 = false;
				SetRightToLeft2 = false;
			}
			SetRightToLeftAtRegion(1, SetRightToLeft1);
			SetRightToLeftAtRegion(2, SetRightToLeft2);
			ClearErrorMessage(0);
		}

		private void FileName_TextChanged(object sender, EventArgs e)
		{
			ClearErrorMessage(0);
		}

		private void FrmInfoScreen_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (!FormCanClose)
			{
				ClearErrorMessage(0);
				Save_FormPos_To_Registry();
				if (ActionBeforeNextEvent() == DialogResult.Cancel)
				{
					e.Cancel = true;
					return;
				}
				gf.InfoScreenFormOpen = false;
			}
			try
			{
				DShowPlayer.TidyUp();
				TimerEditRequest.Stop();
				ApplyPlayControls(ControlsBtn.Closebtn);
				TimerTrack.Stop();
				TimerFast.Stop();
			}
			catch
			{
			}
		}

		private void splitContainerMain_SplitterMoved(object sender, SplitterEventArgs e)
		{
			if (!SplitterReAdjust)
			{
				SetTopPanel();
				if (splitContainerMain.SplitterDistance < 130)
				{
					SplitterReAdjust = true;
					splitContainerMain.SplitterDistance = 130;
					SplitterReAdjust = false;
				}
				groupBox2.Height = splitContainerMain.Panel1.Height - 1;
				panelVerses.Height = groupBox2.Height - 15;
				panelOrderList.Height = panelVerses.Height;
			}
		}

		private void SetTopPanel()
		{
			if (base.WindowState == FormWindowState.Normal)
			{
				splitContainerMain.FixedPanel = FixedPanel.Panel1;
			}
			else if (base.WindowState == FormWindowState.Maximized)
			{
				splitContainerMain.FixedPanel = FixedPanel.None;
			}
		}

		private void Menu_EditHistory_Click(object sender, EventArgs e)
		{
			try
			{
				ToolStripMenuItem toolStripMenuItem = (ToolStripMenuItem)sender;
				int num = DataUtil.ObjToInt(toolStripMenuItem.Tag) + 1;
				if (ActionBeforeNextEvent() != DialogResult.Cancel)
				{
					CurFileName = DataUtil.Mid(gf.InfoScreenEditHistoryList[num, 0], 1);
					LoadInfoScreen(CurFileName);
					AddToEditHistory("I" + CurFileName);
					gf.InfoScreenFileName = CurFileName;
					gf.InfoScreenLoadItem = true;
				}
			}
			catch
			{
			}
		}

		private void R1Chinese_Click(object sender, EventArgs e)
		{
			SwitchChinese(1);
		}

		private void R2Chinese_Click(object sender, EventArgs e)
		{
			SwitchChinese(2);
		}

		private void R1UndoRedo_Click(object sender, EventArgs e)
		{
			Button button = (Button)sender;
			switch (DataUtil.ObjToInt(button.Tag))
			{
			case 0:
				cmdUndo_Click(1);
				break;
			case 1:
				cmdRedo_Click(1);
				break;
			}
		}

		private void R2UndoRedo_Click(object sender, EventArgs e)
		{
			Button button = (Button)sender;
			switch (DataUtil.ObjToInt(button.Tag))
			{
			case 0:
				cmdUndo_Click(2);
				break;
			case 1:
				cmdRedo_Click(2);
				break;
			}
		}

		private void ClearStack(int Region)
		{
			if ((Region == 0) | (Region == 1))
			{
				StackIndex[1] = -1;
				StackMaxRedo[1] = -1;
				StackStartPoint[1] = 0;
				rtfMain_Change(1);
			}
			if ((Region == 0) | (Region == 2))
			{
				StackIndex[2] = -1;
				StackMaxRedo[2] = -1;
				StackStartPoint[2] = 0;
				rtfMain_Change(2);
			}
		}

		private void cmdUndo_Click(int Region)
		{
			if (StackIndex[Region] != StackStartPoint[Region])
			{
				IgnoreChange = true;
				StackIndex[Region]--;
				if (StackIndex[Region] >= 0)
				{
					switch (Region)
					{
					case 1:
						AssignStackToLyrics(ref tbLyrics1, Region);
						break;
					case 2:
						AssignStackToLyrics(ref tbLyrics2, Region);
						break;
					}
				}
				else if (StackStartPoint[Region] > 0)
				{
					StackIndex[Region] = StackArrayMaxPoint;
				}
				IgnoreChange = false;
			}
			if (Region == 1)
			{
				tbLyrics1.Focus();
			}
			else
			{
				tbLyrics2.Focus();
			}
		}

		private void cmdRedo_Click(int Region)
		{
			if (StackIndex[Region] != StackMaxRedo[Region])
			{
				IgnoreChange = true;
				StackIndex[Region]++;
				if (StackIndex[Region] > StackArrayMaxPoint)
				{
					StackIndex[Region] = 0;
				}
				switch (Region)
				{
				case 1:
					AssignStackToLyrics(ref tbLyrics1, Region);
					break;
				case 2:
					AssignStackToLyrics(ref tbLyrics2, Region);
					break;
				}
				IgnoreChange = false;
			}
			if (Region == 1)
			{
				tbLyrics1.Focus();
			}
			else
			{
				tbLyrics2.Focus();
			}
		}

		private void AssignStackToLyrics(ref RichTextBox InLyrics, int Region)
		{
			InLyrics.Rtf = sStack[Region, StackIndex[Region]];
			InLyrics.SelectionStart = iCursorPosition[Region, StackIndex[Region]];
			UpdateVersesList();
		}

		private void Verses_Add_Click(object sender, EventArgs e)
		{
			ToolStripButton toolStripButton = (ToolStripButton)sender;
			if (toolStripButton.Name == "Verses_Add")
			{
				AddBtn_Click(ref VersesList, ref OrderList, ref OrderListSequence);
			}
			else
			{
				SmartAddBtn_Click(ref OrderList, ref OrderListSequence);
			}
		}

		private void AddBtn_Click(ref ListView InVersesList, ref ListView InOrderList, ref string InSequence)
		{
			ListViewItem listViewItem = new ListViewItem();
			ClearErrorMessage(0);
			for (int i = 0; i <= InVersesList.Items.Count - 1; i++)
			{
				if (InVersesList.Items[i].Selected)
				{
					listViewItem = InOrderList.Items.Add(InVersesList.Items[i].Text);
					listViewItem.SubItems.Add(InVersesList.Items[i].SubItems[1].Text);
				}
			}
			UpdateSequence(ref InOrderList, ref InSequence);
		}

		private void UpdateSequence(ref ListView InOrderList, ref string InSequence)
		{
			InSequence = "";
			if (InOrderList.Items.Count > 0)
			{
				for (int i = 0; i < InOrderList.Items.Count; i++)
				{
					InSequence += (char)DataUtil.StringToInt(InOrderList.Items[i].SubItems[1].Text);
				}
			}
			if (InOrderList.Name == "Rotate_OrderList")
			{
				UpdateRotateTimePositions();
			}
		}

		private void SmartAddBtn_Click(ref ListView InOrderList, ref string InSequence)
		{
			ListViewItem listViewItem = new ListViewItem();
			ClearErrorMessage(0);
			InOrderList.Items.Clear();
			tbWorkspace.Text = tbLyrics1.Text;
			int num = tbWorkspace.Text.IndexOf("[");
			if (tbWorkspace.Text.IndexOf(gf.VerseSymbol[0]) == num && num >= 0)
			{
				listViewItem = InOrderList.Items.Add(gf.VerseTitle[0]);
				listViewItem.SubItems.Add(0.ToString());
			}
			for (int i = 1; i < 99; i++)
			{
				if (!VersePresent[i])
				{
					continue;
				}
				listViewItem = InOrderList.Items.Add(gf.VerseTitle[i]);
				listViewItem.SubItems.Add(i.ToString());
				if (VersePresent[111])
				{
					if (!VersePresent[i + 1] & VersePresent[112])
					{
						listViewItem = InOrderList.Items.Add(gf.VerseTitle[112]);
						listViewItem.SubItems.Add(112.ToString());
					}
					else
					{
						listViewItem = InOrderList.Items.Add(gf.VerseTitle[111]);
						listViewItem.SubItems.Add(111.ToString());
					}
				}
				if (VersePresent[0])
				{
					if (!VersePresent[i + 1] & VersePresent[102])
					{
						listViewItem = InOrderList.Items.Add(gf.VerseTitle[102]);
						listViewItem.SubItems.Add(102.ToString());
					}
					else
					{
						listViewItem = InOrderList.Items.Add(gf.VerseTitle[0]);
						listViewItem.SubItems.Add(0.ToString());
					}
				}
				if ((i == 1) & VersePresent[100])
				{
					listViewItem = InOrderList.Items.Add(gf.VerseTitle[100]);
					listViewItem.SubItems.Add(100.ToString());
				}
			}
			if (VersePresent[101])
			{
				listViewItem = InOrderList.Items.Add(gf.VerseTitle[101]);
				listViewItem.SubItems.Add(101.ToString());
			}
			UpdateSequence(ref InOrderList, ref InSequence);
		}

		private void OrderList_Btn_Click(object sender, EventArgs e)
		{
			ToolStripButton toolStripButton = (ToolStripButton)sender;
			if (toolStripButton.Name == "OrderList_Up")
			{
				MoveUPBtn_Click(ref OrderList, ref OrderListSequence);
			}
			else if (toolStripButton.Name == "OrderList_Down")
			{
				MoveDownBtn_Click(ref OrderList, ref OrderListSequence);
			}
			else
			{
				DelBtn_Click(ref OrderList, ref OrderListSequence);
			}
		}

		private void MoveUPBtn_Click(ref ListView InOrderList, ref string InSequence)
		{
			ClearErrorMessage(0);
			int count = InOrderList.Items.Count;
			if (count < 1)
			{
				return;
			}
			int num = 0;
			for (int i = 0; i < count; i++)
			{
				if (InOrderList.Items[i].Selected)
				{
					if (num < 1)
					{
						num = i;
						continue;
					}
					i = count;
					num = 0;
				}
			}
			if (num >= 1)
			{
				string text = InOrderList.Items[num].Text;
				InOrderList.Items[num].Text = InOrderList.Items[num - 1].Text;
				InOrderList.Items[num - 1].Text = text;
				text = InOrderList.Items[num].SubItems[1].Text;
				InOrderList.Items[num].SubItems[1].Text = InOrderList.Items[num - 1].SubItems[1].Text;
				InOrderList.Items[num - 1].SubItems[1].Text = text;
				InOrderList.Items[num].Selected = false;
				InOrderList.Items[num - 1].Selected = true;
				UpdateSequence(ref InOrderList, ref InSequence);
			}
		}

		private void MoveDownBtn_Click(ref ListView InOrderList, ref string InSequence)
		{
			ClearErrorMessage(0);
			int count = InOrderList.Items.Count;
			if (count <= 1)
			{
				return;
			}
			int num = 0;
			for (int i = 0; i <= count - 1; i++)
			{
				if (InOrderList.Items[i].Selected)
				{
					if (num < 1)
					{
						num = i;
						continue;
					}
					i = count;
					num = -1;
				}
			}
			if (!((num < 0) | (num == count - 1)))
			{
				string text = InOrderList.Items[num].Text;
				InOrderList.Items[num].Text = InOrderList.Items[num + 1].Text;
				InOrderList.Items[num + 1].Text = text;
				text = InOrderList.Items[num].SubItems[1].Text;
				InOrderList.Items[num].SubItems[1].Text = InOrderList.Items[num + 1].SubItems[1].Text;
				InOrderList.Items[num + 1].SubItems[1].Text = text;
				InOrderList.Items[num].Selected = false;
				InOrderList.Items[num + 1].Selected = true;
				UpdateSequence(ref InOrderList, ref InSequence);
			}
		}

		private void DelBtn_Click(ref ListView InOrderList, ref string InSequence)
		{
			ClearErrorMessage(0);
			if (InOrderList.Items.Count == 0)
			{
				return;
			}
			int num = 0;
			for (int num2 = InOrderList.Items.Count - 1; num2 >= 0; num2--)
			{
				if (InOrderList.Items[num2].Selected)
				{
					InOrderList.Items[num2].Remove();
					num = num2;
				}
			}
			if (num > 0)
			{
				num--;
			}
			if (InOrderList.Items.Count > 0)
			{
				InOrderList.Items[num].Selected = true;
			}
			UpdateSequence(ref InOrderList, ref InSequence);
		}

		private void OrderList_KeyUp(object sender, KeyEventArgs e)
		{
			Action_OrderList_KeyUpEvent(ref OrderList, e, ref OrderListSequence);
		}

		private void Action_OrderList_KeyUpEvent(ref ListView InOrderList, KeyEventArgs e, ref string InSequence)
		{
			if (e.Control && e.KeyCode == Keys.A)
			{
				for (int i = 0; i <= InOrderList.Items.Count - 1; i++)
				{
					InOrderList.Items[i].Selected = true;
				}
			}
			else if (e.Control && e.KeyCode == Keys.C)
			{
				tempSequenceCopied.Items.Clear();
				ListViewItem listViewItem = new ListViewItem();
				for (int i = 0; i <= InOrderList.Items.Count - 1; i++)
				{
					if (InOrderList.Items[i].Selected)
					{
						listViewItem = tempSequenceCopied.Items.Add(InOrderList.Items[i].Text);
						listViewItem.SubItems.Add(InOrderList.Items[i].SubItems[1].Text);
					}
				}
			}
			else if (e.Control && e.KeyCode == Keys.V)
			{
				ListViewItem listViewItem = new ListViewItem();
				for (int i = 0; i <= tempSequenceCopied.Items.Count - 1; i++)
				{
					listViewItem = InOrderList.Items.Add(tempSequenceCopied.Items[i].Text);
					listViewItem.SubItems.Add(tempSequenceCopied.Items[i].SubItems[1].Text);
				}
				UpdateSequence(ref InOrderList, ref InSequence);
			}
			else if (e.KeyCode == Keys.Delete)
			{
				DelBtn_Click(ref InOrderList, ref InSequence);
			}
		}

		private void VersesList_DoubleClick(object sender, EventArgs e)
		{
			if (VersesList.SelectedItems.Count > 0)
			{
				AddBtn_Click(ref VersesList, ref OrderList, ref OrderListSequence);
			}
		}

		private void SongTitle2_TextChanged(object sender, EventArgs e)
		{
			SongTitle2_Change();
		}

		private void R1VerseFormat_Click(object sender, EventArgs e)
		{
			gf.FormatPlainLyrics(ref tbLyrics1);
		}

		private void R2VerseFormat_Click(object sender, EventArgs e)
		{
			gf.FormatPlainLyrics(ref tbLyrics2);
		}

		private void TimerEditRequest_Tick(object sender, EventArgs e)
		{
			if (gf.InfoScreen_RequestReceived)
			{
				gf.InfoScreen_RequestReceived = false;
				if (gf.InfoScreenAction == InfoType.FormatStringUpdate)
				{
					if (gf.InfoScreenFileName != "" && gf.InfoScreenFileName == gf.InfoScreenDir + ThisItemFileName + ".esi")
					{
						gf.InfoItem1.Format.FormatString = gf.InfoScreenNewFormatString;
						gf.InfoScreenNewFormatString = "";
						if (gf.InfoScreenLoadNewBackground)
						{
							gf.InfoItem1.Format.BackgroundPicture = gf.InfoScreenBackgroundPicture;
						}
					}
				}
				else
				{
					ClearErrorMessage(0);
					if (base.WindowState == FormWindowState.Minimized)
					{
						base.WindowState = FormWindowState.Normal;
					}
					base.TopMost = true;
					Focus();
					base.TopMost = false;
					if (gf.InfoScreenFileName != "")
					{
						if (ActionBeforeNextEvent() == DialogResult.Cancel)
						{
							return;
						}
						LoadInfoScreen(gf.InfoScreenFileName);
						AddToEditHistory("I" + gf.InfoScreenFileName);
					}
					else
					{
						NewItem();
					}
				}
				gf.InfoScreenAction = InfoType.NoAction;
			}
			if (gf.Edit_HistoryMaxChanged)
			{
				gf.SaveInfoScreenEditHistoryToRegistry();
				UpdateMenu_EditHistory();
			}
		}

		private void Title2_LookUp_Click(object sender, EventArgs e)
		{
			if (SongTitle2.Text == "")
			{
				gf.Lookup_NameSelected = "*";
			}
			else
			{
				gf.Lookup_NameSelected = "*" + DataUtil.Trim(SongTitle2.Text) + "*";
			}
			FrmLookupTitles frmLookupTitles = new FrmLookupTitles();
			if (frmLookupTitles.ShowDialog() == DialogResult.OK && gf.Lookup_NameSelected != "")
			{
				SongTitle2.Text = gf.Lookup_NameSelected;
				if (gf.Lookup_NameBookRef != "")
				{
					gf.UpdateRefString(gf.Lookup_NameBookRef, ",", ref BookReference, ",");
				}
				if (gf.Lookup_NameUserRef != "")
				{
					gf.UpdateRefString(gf.Lookup_NameUserRef, ",", ref UserReference, ",");
				}
			}
		}

		private void SyncBtnUpDown_Click(object sender, EventArgs e)
		{
			Button button = (Button)sender;
			int direction = DataUtil.ObjToInt(button.Tag);
			ScrollBothLyrics(direction);
		}

		private void ScrollBothLyrics(int Direction)
		{
			int num = tbLyrics1.SelectionStart;
			int selectionLength = tbLyrics1.SelectionLength;
			int NewPosition = 0;
			int NewPositionLength = selectionLength;
			if (!ScreenBreak1Available)
			{
				gf.MapLyricsBreak(ref ScreenBreaks1, ref tbLyrics1, ref ScreenBreak1Available);
			}
			if (!ScreenBreak2Available)
			{
				gf.MapLyricsBreak(ref ScreenBreaks2, ref tbLyrics2, ref ScreenBreak2Available);
			}
			string LookupVerseSym = "";
			int LookupScreenCount = 0;
			if (num == 0 && selectionLength == 0)
			{
				num = -1;
			}
			gf.GetBreakPosition(ScreenBreaks1, num, Direction, ref NewPosition, ref NewPositionLength, ref LookupVerseSym, ref LookupScreenCount);
			int NewPosition2 = 0;
			int NewPositionLength2 = 0;
			gf.GetBreakPosition(ScreenBreaks2, ref NewPosition2, ref NewPositionLength2, LookupVerseSym, LookupScreenCount);
			tbLyrics1.SelectionStart = NewPosition;
			tbLyrics1.SelectionLength = ((NewPositionLength >= 0) ? NewPositionLength : (tbLyrics1.Text.Length - NewPositionLength));
			tbLyrics1.ScrollToCaret();
			tbLyrics2.SelectionStart = NewPosition2;
			tbLyrics2.SelectionLength = ((NewPositionLength2 >= 0) ? NewPositionLength2 : (tbLyrics2.Text.Length - NewPositionLength2));
			tbLyrics2.ScrollToCaret();
			tbLyrics1.Focus();
		}

		private void TextBox_Leave(object sender, EventArgs e)
		{
			TextBox textBox = (TextBox)sender;
			TextBoxBtnMovement(textBox.Name);
		}

		private void SetTextBoxInvisible()
		{
			Btn_Title.Visible = false;
			Btn_Title2.Visible = false;
			Btn_Writer.Visible = false;
			Btn_Copyright.Visible = false;
			Btn_BookRef.Visible = false;
			Btn_UserRef.Visible = false;
		}

		private void TextBox_Enter(object sender, EventArgs e)
		{
			TextBox textBox = (TextBox)sender;
			TextBoxBtnMovement(textBox.Name);
		}

		private void Btn_Enter(object sender, EventArgs e)
		{
			Button button = (Button)sender;
			TextBoxBtnMovement(button.Name);
		}

		private void TextBoxBtnMovement(string InName)
		{
			SetTextBoxInvisible();
			switch (InName)
			{
			case "SongTitle":
			case "Btn_Title":
				Btn_Title.Visible = true;
				break;
			case "SongTitle2":
			case "Btn_Title2":
				Btn_Title2.Visible = true;
				break;
			case "WriterInfo":
			case "Btn_Writer":
				Btn_Writer.Visible = true;
				break;
			case "CopyrightInfo":
			case "Btn_Copyright":
				Btn_Copyright.Visible = true;
				break;
			case "BookReference":
			case "Btn_BookRef":
				Btn_BookRef.Visible = true;
				break;
			case "UserReference":
			case "Btn_UserRef":
				Btn_UserRef.Visible = true;
				break;
			}
			ResizeTitleFields();
		}

		private void tbLyrics1_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Control && e.KeyCode == Keys.Z)
			{
				cmdUndo_Click(1);
			}
			else if (e.Control && e.KeyCode == Keys.Y)
			{
				cmdRedo_Click(1);
			}
		}

		private void tbLyrics1_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.F8)
			{
				InsertingPresetItem = true;
				gf.InsertIndicator(ref tbLyrics1, 152);
				InsertingPresetItem = false;
				Lyrics_TextChanged(1);
			}
		}

		private void tbLyrics2_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Control && e.KeyCode == Keys.Z)
			{
				cmdUndo_Click(2);
			}
			else if (e.Control && e.KeyCode == Keys.Y)
			{
				cmdRedo_Click(2);
			}
		}

		private void tbLyrics2_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.F8)
			{
				InsertingPresetItem = true;
				gf.InsertIndicator(ref tbLyrics2, 152);
				InsertingPresetItem = false;
				Lyrics_TextChanged(2);
			}
		}

		private void splitContainerRotate_SplitterMoved(object sender, SplitterEventArgs e)
		{
			SetRotatePanel();
			if (splitContainerRotate.SplitterDistance < 112)
			{
				splitContainerRotate.SplitterDistance = 112;
			}
			groupBoxRotateVerses.Height = splitContainerRotate.Panel1.Height - 2;
			panelRotate_Verses.Height = groupBoxRotateVerses.Height - 15;
			panelRotate_OrderList.Height = panelRotate_Verses.Height;
		}

		private void SetRotatePanel()
		{
			if (base.WindowState == FormWindowState.Normal)
			{
				splitContainerRotate.FixedPanel = FixedPanel.Panel1;
			}
			else if (base.WindowState == FormWindowState.Maximized)
			{
				splitContainerRotate.FixedPanel = FixedPanel.None;
			}
		}

		private void UpdateRotateTimePositions()
		{
			TimeSpan timeSpan = new TimeSpan(0, 0, 0);
			UpdateRotateTimePositions(InTotalRotateTime: (int)Rotate_TimeTotal.Value.Subtract(InitDateTime).TotalSeconds, InStyle: (!Rotate_Multiple.Checked) ? 1 : 2, InGapRotateTime: (int)Rotate_SlidesGapUpDown.Value, InRotateTimings: "", UseRotateTimings: false, ResetAll: false);
		}

		private void UpdateRotateTimePositions(int InStyle, int InGapRotateTime, int InTotalRotateTime, string InRotateTimings, bool UseRotateTimings, bool ResetAll)
		{
			flowLayoutRotate.Controls.Clear();
			if (InStyle == 2)
			{
				Rotate_Multiple.Checked = true;
			}
			else
			{
				Rotate_Equal.Checked = true;
			}
			Rotate_SlidesGapUpDown.Value = InGapRotateTime;
			Rotate_TimeTotal.Value = InitDateTime.Add(new TimeSpan(0, 0, InTotalRotateTime));
			RotateTotalScreensIndex = -1;
			int num = -1;
			string text = "";
			for (int i = 0; i < Rotate_OrderList.Items.Count; i++)
			{
				num = VersePresentNewScreenCount[DataUtil.StringToInt(Rotate_OrderList.Items[i].SubItems[1].Text)];
				for (int j = 0; j < num; j++)
				{
					RotateTotalScreensIndex++;
					if (RotateTimeLabel[RotateTotalScreensIndex] == null)
					{
						RotateTimeLabel[RotateTotalScreensIndex] = new Label();
						RotateTimeLabel[RotateTotalScreensIndex].AutoSize = false;
						RotateTimeLabel[RotateTotalScreensIndex].Width = 48;
						RotateTimeLabel[RotateTotalScreensIndex].TextAlign = ContentAlignment.MiddleLeft;
					}
					if (RotateTimePosition[RotateTotalScreensIndex] == null)
					{
						RotateTimePosition[RotateTotalScreensIndex] = new DateTimePicker();
						RotateTimePosition[RotateTotalScreensIndex].Width = 52;
						RotateTimePosition[RotateTotalScreensIndex].ShowUpDown = true;
						RotateTimePosition[RotateTotalScreensIndex].MinDate = InitDateTime;
						RotateTimePosition[RotateTotalScreensIndex].CustomFormat = "mm:ss";
						RotateTimePosition[RotateTotalScreensIndex].Format = DateTimePickerFormat.Custom;
						RotateTimePosition[RotateTotalScreensIndex].Value = InitDateTime;
						if (i == 0 && j == 0)
						{
							RotateTimePosition[RotateTotalScreensIndex].Enabled = false;
						}
					}
					RotateTimeLabel[RotateTotalScreensIndex].Text = ((j < 1) ? Rotate_OrderList.Items[i].Text : " - ");
					RotateTimePosition[RotateTotalScreensIndex].Tag = ((j < 1) ? Rotate_OrderList.Items[i].SubItems[1].Text : 151.ToString());
					if (UseRotateTimings && RotateTotalScreensIndex > 0)
					{
						text = DataUtil.ExtractOneInfo(ref InRotateTimings, ';', RemoveExtract: true, MinusOneIfBlank: false);
						RotateTimePosition[RotateTotalScreensIndex].Value = InitDateTime.Add(new TimeSpan(0, 0, DataUtil.StringToInt(text)));
					}
					flowLayoutRotate.Controls.Add(RotateTimeLabel[RotateTotalScreensIndex]);
					flowLayoutRotate.Controls.Add(RotateTimePosition[RotateTotalScreensIndex]);
				}
			}
			if (!ResetAll)
			{
				return;
			}
			for (int i = 0; i < RotateTimePosition.Length; i++)
			{
				if (RotateTimePosition[i] != null)
				{
					RotateTimePosition[i].Value = InitDateTime;
				}
			}
		}

		private void InitMediaPlayer()
		{
			if (gf.WMP_Present)
			{
				try
				{
					DShowPlayer.Parent = this;
					DShowPlayer.Parent = panelRotate_Media;
					DShowPlayer.Location = new Point(0, 0);
					DShowPlayer.SetDefaultSize(0, 0, panelRotate_Media.Width, panelRotate_Media.Height, (VAlign)gf.VideoVAlign);
					DShowPlayer.ForeColorChanged += DShowPlayer_ForeColorChanged;
					PlayerOK = true;
				}
				catch
				{
					PlayerOK = false;
				}
			}
			if (PlayerOK)
			{
				DShowPlayer.Dock = DockStyle.Fill;
				DShowPlayer.newFilename = Rotate_tbSourceLocation.Text;
				panelNoPlayer.Visible = false;
				EnableMediaControls(MediaOn: true);
				DShowPlayer.Visible = true;
			}
			else
			{
				EnableMediaControls(MediaOn: false);
				panelNoPlayer.Dock = DockStyle.Fill;
				panelNoPlayer.Visible = true;
			}
		}

		private void DShowPlayer_ForeColorChanged(object sender, EventArgs e)
		{
			switch (DShowPlayer.currentState)
			{
			case PlayState.Running:
				PlayPauseBtn.Enabled = true;
				StopBtn.Enabled = true;
				PlayPauseBtn.Text = "Pause";
				LabelMediaType.Text = DShowPlayer.GetStatusText();
				Cursor = Cursors.Default;
				break;
			case PlayState.Paused:
				PlayPauseBtn.Text = "Play";
				Cursor = Cursors.Default;
				break;
			case PlayState.Stopped:
				StopBtn.Enabled = false;
				PlayPauseBtn.Text = "Play";
				Cursor = Cursors.Default;
				break;
			default:
				StopBtn.Enabled = false;
				PlayPauseBtn.Text = "Play";
				break;
			}
		}

		private void EnableMediaControls(bool MediaOn)
		{
			bool enabled = (MediaOn & PlayerOK) ? true : false;
			btnDuration.Enabled = enabled;
			btnAddPosition.Enabled = enabled;
			panelPlayBtns.Enabled = enabled;
			TrackBarVolume.Enabled = enabled;
			labelVol.Enabled = enabled;
			labelPos.Enabled = enabled;
			LabelDuration.Enabled = enabled;
			LabelPosition.Enabled = enabled;
			LabelMediaType.Enabled = enabled;
			labelMed.Enabled = enabled;
		}

		private void Rotate_LocationBtn_Click(object sender, EventArgs e)
		{
			Rotate_tbSourceLocation.Text = DataUtil.Trim(Rotate_tbSourceLocation.Text);
			string text = "";
			try
			{
				text = Path.GetDirectoryName(Rotate_tbSourceLocation.Text);
			}
			catch
			{
			}
			text = ((text != "") ? text : gf.MediaDir);
			OpenFileDialog1.Filter = "All Files (*.*) | *.*";
			OpenFileDialog1.InitialDirectory = text;
			OpenFileDialog1.AddExtension = true;
			OpenFileDialog1.FileName = Rotate_tbSourceLocation.Text;
			bool flag = false;
			try
			{
				if (OpenFileDialog1.ShowDialog() == DialogResult.OK)
				{
					ApplyPlayControls(ControlsBtn.Stopbtn);
					Rotate_tbSourceLocation.Text = OpenFileDialog1.FileName;
					if (PlayerOK)
					{
						DShowPlayer.newFilename = Rotate_tbSourceLocation.Text;
					}
					ApplySoundControls(ApplyMute: false);
				}
			}
			catch
			{
				flag = true;
			}
			if (flag)
			{
				try
				{
					OpenFileDialog1.FileName = "";
					if (OpenFileDialog1.ShowDialog() == DialogResult.OK)
					{
						ApplyPlayControls(ControlsBtn.Stopbtn);
						Rotate_tbSourceLocation.Text = OpenFileDialog1.FileName;
						if (PlayerOK)
						{
							DShowPlayer.newFilename = Rotate_tbSourceLocation.Text;
						}
					}
				}
				catch
				{
				}
			}
		}

		private void ApplyPlayControls(ControlsBtn InAction)
		{
			if (!PlayerOK)
			{
				return;
			}
			TimerFast.Stop();
			switch (InAction)
			{
			case ControlsBtn.PlayPausebtn:
				if (DShowPlayer.currentState == PlayState.Running || DShowPlayer.currentState == PlayState.Paused)
				{
					DShowPlayer.PausePlayClip();
					break;
				}
				Rotate_tbSourceLocation.Text = DataUtil.Trim(Rotate_tbSourceLocation.Text);
				try
				{
					DShowPlayer.newFilename = Rotate_tbSourceLocation.Text;
					if (DShowPlayer.newFilename != "")
					{
						DShowPlayer.OpenClip();
						AttemptConnectCount = 0;
						LabelMediaType.Text = DShowPlayer.GetStatusText();
					}
					else
					{
						ResetMediaMessages();
					}
				}
				catch
				{
					DShowPlayer.newFilename = "";
					ResetMediaMessages();
				}
				return;
			case ControlsBtn.Stopbtn:
				DShowPlayer.StopClip();
				break;
			case ControlsBtn.FFbtn:
				ApplySoundControls(ApplyMute: true);
				IncrementCurrentPosition(1.0);
				TimeIncrement = 5.0;
				TimerFast.Start();
				break;
			case ControlsBtn.FRbtn:
				ApplySoundControls(ApplyMute: true);
				IncrementCurrentPosition(-1.0);
				TimeIncrement = -5.0;
				TimerFast.Start();
				break;
			case ControlsBtn.Closebtn:
				DShowPlayer.StopClip();
				break;
			}
			Cursor = Cursors.Default;
		}

		private void ResetMediaMessages()
		{
			if (PlayerOK)
			{
				LabelMediaType.Text = DShowPlayer.GetStatusText();
			}
			else
			{
				LabelMediaType.Text = "";
			}
			Cursor = Cursors.Default;
		}

		private void SetDurationSettings()
		{
			if (PlayerOK && DShowPlayer.GetClipDuration() > 0)
			{
				SetDurationSettings(ResetAll: false);
			}
			else
			{
				SetDurationSettings(ResetAll: true);
			}
		}

		private void SetDurationSettings(bool ResetAll)
		{
			if (ResetAll)
			{
				if (LabelMediaType.Text != "" && ((LabelMediaType.Text[0] == 'A') | (LabelMediaType.Text[0] == 'V')))
				{
					LabelDuration.Text = "Stream";
				}
				else
				{
					LabelDuration.Text = "00:00";
				}
				LabelPosition.Text = "00:00";
				TrackBarDuration.Maximum = 0;
				TrackBarDuration.Value = 0;
			}
			else if (PlayerOK)
			{
				LabelDuration.Text = ((DShowPlayer.newFilename != "") ? DShowPlayer.GetClipDurationString() : "00:00");
				CurMediaPosition = DShowPlayer.GetCurrentPosition();
				CurMediaLength = DShowPlayer.GetClipDuration();
				LabelPosition.Text = DShowPlayer.GetCurrentPositionString();
				TrackBarDuration.Maximum = DShowPlayer.GetClipDuration();
				TrackBarDuration.Value = ((DShowPlayer.GetCurrentPosition() > TrackBarDuration.Maximum) ? TrackBarDuration.Maximum : DShowPlayer.GetCurrentPosition());
			}
			else
			{
				LabelDuration.Text = "00:00";
				LabelPosition.Text = "00:00";
				TrackBarDuration.Maximum = 1000;
				TrackBarDuration.Value = 0;
			}
		}

		private void ApplySoundControls(bool ApplyMute)
		{
			if (PlayerOK)
			{
				DShowPlayer.SetVolume(TrackBarVolume.Value);
				gf.MediaVolume = TrackBarVolume.Value;
			}
		}

		private void PlayPauseBtn_Click(object sender, EventArgs e)
		{
			ApplyPlayControls(ControlsBtn.PlayPausebtn);
		}

		private void StopBtn_Click(object sender, EventArgs e)
		{
			ApplyPlayControls(ControlsBtn.Stopbtn);
		}

		private void FastReverseBtn_MouseDown(object sender, MouseEventArgs e)
		{
			ApplyPlayControls(ControlsBtn.FRbtn);
		}

		private void FastReverseBtn_MouseUp(object sender, MouseEventArgs e)
		{
			ReturnToPreviousState();
		}

		private void FastForwardBtn_MouseDown(object sender, MouseEventArgs e)
		{
			ApplyPlayControls(ControlsBtn.FFbtn);
		}

		private void FastForwardBtn_MouseUp(object sender, MouseEventArgs e)
		{
			ReturnToPreviousState();
		}

		private void ReturnToPreviousState()
		{
			TimerFast.Stop();
			ApplySoundControls(ApplyMute: false);
		}

		private void TimerTrack_Tick(object sender, EventArgs e)
		{
			SetDurationSettings();
		}

		private void TrackBarDuration_Scroll(object sender, EventArgs e)
		{
			if (PlayerOK)
			{
				DShowPlayer.SetCurrentPosition(TrackBarDuration.Value);
			}
		}

		private void TrackBarVolume_ValueChanged(object sender, EventArgs e)
		{
			if (!InitLoad)
			{
				ApplySoundControls(ApplyMute: false);
			}
		}

		private void btnAddPosition_Click(object sender, EventArgs e)
		{
			if (RotateTotalScreensIndex <= 0)
			{
				return;
			}
			DateTime value = InitDateTime.Add(new TimeSpan(0, 0, (int)CurMediaPosition));
			int num = 1;
			while (true)
			{
				if (num < RotateTotalScreensIndex + 1)
				{
					if (RotateTimePosition[num].Value <= InitDateTime)
					{
						break;
					}
					num++;
					continue;
				}
				return;
			}
			RotateTimePosition[num].Value = value;
		}

		private void Rotate_Option_CheckedChanged(object sender, EventArgs e)
		{
			MediaOption_Changed();
		}

		private void MediaOption_Changed()
		{
			EnableMediaControls((Rotate_Multiple.Checked & PlayerOK) ? true : false);
			panelRotateLeftTop2.Enabled = Rotate_Multiple.Checked;
			flowLayoutRotate.Enabled = Rotate_Multiple.Checked;
			groupBoxRotateVerses.Enabled = Rotate_Multiple.Checked;
			Rotate_tbSourceLocation.Enabled = Rotate_Multiple.Checked;
			panelLoc.Enabled = Rotate_Multiple.Checked;
			if (Rotate_Multiple.Checked)
			{
				ApplyPlayControls(ControlsBtn.Closebtn);
			}
		}

		private void btnClearMediaPositions_Click(object sender, EventArgs e)
		{
			if (RotateTotalScreensIndex >= 0)
			{
				for (int i = 0; i <= RotateTotalScreensIndex; i++)
				{
					if (RotateTimePosition[i] != null)
					{
						RotateTimePosition[i].Value = InitDateTime;
					}
				}
			}
			Rotate_TimeTotal.Value = InitDateTime;
		}

		private void TimerFast_Tick(object sender, EventArgs e)
		{
			IncrementCurrentPosition(TimeIncrement);
		}

		private void IncrementCurrentPosition(double InIncrement)
		{
			if (PlayerOK)
			{
				DShowPlayer.SetCurrentPosition((double)DShowPlayer.GetCurrentPosition() + InIncrement);
				SetDurationSettings();
			}
		}

		private void panelRotate_Sample_Resize(object sender, EventArgs e)
		{
			Rotate_tbSourceLocation.Width = panelRotate_Sample.Width - Rotate_tbSourceLocation.Left - panelLoc.Width - 6;
			panelLoc.Left = Rotate_tbSourceLocation.Left + Rotate_tbSourceLocation.Width + 2;
		}

		private string GenerateRotateString()
		{
			TimeSpan timeSpan = new TimeSpan(0, 0, 0);
			return GenerateRotateString(InTotalRotateTime: (int)Rotate_TimeTotal.Value.Subtract(InitDateTime).TotalSeconds, InStyle: Rotate_Equal.Checked ? 1 : 2, InGapRotateTime: (int)Rotate_SlidesGapUpDown.Value, InOrderList: Rotate_OrderList, InPosition: RotateTimePosition, InTotalScreens: RotateTotalScreensIndex + 1);
		}

		private string GenerateRotateString(int InStyle, int InGapRotateTime, int InTotalRotateTime, ListView InOrderList, DateTimePicker[] InPosition, int InTotalScreens)
		{
			if (InStyle == 1 && InGapRotateTime == 0 && InTotalRotateTime == 0 && InOrderList.Items.Count == 0)
			{
				return "";
			}
			string text = InStyle.ToString() + ';' + InGapRotateTime.ToString() + ';' + InTotalRotateTime.ToString() + ';';
			int num = 0;
			for (int i = 0; i < InOrderList.Items.Count; i++)
			{
				text = text + DataUtil.StringToInt(InOrderList.Items[i].SubItems[1].Text).ToString() + ';';
			}
			if (InTotalScreens > 0)
			{
				ListBox listBox = new ListBox();
				listBox.Sorted = false;
				string text2 = "";
				TimeSpan timeSpan = new TimeSpan(0, 0, 0);
				for (int i = 0; i < InTotalScreens; i++)
				{
					if (InPosition[i] != null)
					{
						timeSpan = InPosition[i].Value.Subtract(InitDateTime);
						if ((int)timeSpan.TotalSeconds > 0)
						{
							listBox.Items.Add(timeSpan.TotalSeconds.ToString("00000"));
						}
					}
				}
				if (listBox.Items.Count > 0)
				{
					listBox.Sorted = true;
					string text3 = "";
					for (int i = 0; i < listBox.Items.Count; i++)
					{
						text3 = text3 + DataUtil.StringToInt(listBox.Items[i].ToString()).ToString() + ';';
					}
					text = text + "»" + text3;
				}
			}
			return text;
		}

		private void TimerAttemptConnect_Tick(object sender, EventArgs e)
		{
			if (PlayerOK)
			{
				if (AttemptConnectCount >= MaxAttemptConnectCount)
				{
					TimerAttemptConnect.Stop();
					LabelMediaType.Text = DShowPlayer.GetStatusText();
					DShowPlayer.StopClip();
					Cursor = Cursors.Default;
				}
				else
				{
					Cursor = Cursors.AppStarting;
					AttemptConnectCount++;
					LabelMediaType.Text = DShowPlayer.GetStatusText();
				}
			}
			else
			{
				TimerAttemptConnect.Stop();
				SetDurationSettings(ResetAll: true);
			}
		}

		private void CopyVerseListToRotateVerseList()
		{
			if (VersesList.Items.Count > 0)
			{
				ListViewItem listViewItem = new ListViewItem();
				Rotate_VersesList.Items.Clear();
				for (int i = 0; i < VersesList.Items.Count; i++)
				{
					listViewItem = (ListViewItem)VersesList.Items[i].Clone();
					Rotate_VersesList.Items.Add(listViewItem);
				}
			}
			else
			{
				Rotate_VersesList.Items.Clear();
			}
		}

		private void Rotate_Verses_Add_Click(object sender, EventArgs e)
		{
			ToolStripButton toolStripButton = (ToolStripButton)sender;
			string InSequence = "";
			if (toolStripButton.Name == "Rotate_Verses_Add")
			{
				AddBtn_Click(ref Rotate_VersesList, ref Rotate_OrderList, ref InSequence);
			}
			else
			{
				SmartAddBtn_Click(ref Rotate_OrderList, ref InSequence);
			}
		}

		private void Rotate_VersesList_DoubleClick(object sender, EventArgs e)
		{
			string InSequence = "";
			if (Rotate_VersesList.SelectedItems.Count > 0)
			{
				AddBtn_Click(ref Rotate_VersesList, ref Rotate_OrderList, ref InSequence);
			}
		}

		private void Rotate_OrderList_Btn_Click(object sender, EventArgs e)
		{
			ToolStripButton toolStripButton = (ToolStripButton)sender;
			string InSequence = "";
			if (toolStripButton.Name == "Rotate_OrderList_Up")
			{
				MoveUPBtn_Click(ref Rotate_OrderList, ref InSequence);
			}
			else if (toolStripButton.Name == "Rotate_OrderList_Down")
			{
				MoveDownBtn_Click(ref Rotate_OrderList, ref InSequence);
			}
			else
			{
				DelBtn_Click(ref Rotate_OrderList, ref InSequence);
			}
		}

		private void Rotate_OrderList_KeyUp(object sender, KeyEventArgs e)
		{
			string InSequence = "";
			Action_OrderList_KeyUpEvent(ref Rotate_OrderList, e, ref InSequence);
		}

		private void btnDuration_Click(object sender, EventArgs e)
		{
			DateTime value = InitDateTime.Add(new TimeSpan(0, 0, (int)CurMediaLength));
			Rotate_TimeTotal.Value = value;
		}

		private void CMRegion1_Copy_Click(object sender, EventArgs e)
		{
			gf.ClipboardCopyTextBox(tbLyrics1);
		}

		private void CMRegion1_Paste_Click(object sender, EventArgs e)
		{
			gf.ClipboardPasteTextBox(tbLyrics1, tbLyrics1.SelectionStart);
		}

		private void CMRegion2_Copy_Click(object sender, EventArgs e)
		{
			gf.ClipboardCopyTextBox(tbLyrics2);
		}

		private void CMRegion2_Paste_Click(object sender, EventArgs e)
		{
			gf.ClipboardPasteTextBox(tbLyrics2, tbLyrics2.SelectionStart);
		}

		private void ShowAllButtons(bool ShowAllBtns)
		{
			Menu_ShowAllButtons.Checked = ShowAllBtns;
			gf.InfoMainShowAllButtons = ShowAllBtns;
			RegUtil.SaveRegValue("settings", "InfoMainShowAllButtons", gf.InfoMainShowAllButtons ? 1 : 0);
			panelR1LeftMiddle.Visible = ShowAllBtns;
			panelR2LeftMiddle.Visible = ShowAllBtns;
		}

		private void BuildLyricsContextMenu(int InRegion)
		{
			if (InRegion == 1)
			{
				BuildContextMenuItems(ref CMRegion1, InRegion, SetChordsMenu);
			}
			else
			{
				BuildContextMenuItems(ref CMRegion2, InRegion, SetChordsMenu);
			}
		}

		private void BuildContextMenuItems(ref ContextMenuStrip InContextMenu, int InRegion, bool InSetChordsMenu)
		{
			InContextMenu.Items.Clear();
			ToolStripMenuItem toolStripMenuItem = new ToolStripMenuItem();
			toolStripMenuItem = new ToolStripMenuItem();
			toolStripMenuItem.Name = string.Concat(InContextMenu, "_Copy");
			toolStripMenuItem.Text = "Copy";
			InContextMenu.Items.Add(toolStripMenuItem);
			if (InRegion == 1)
			{
				toolStripMenuItem.Click += new EventHandler(CMRegion1_Copy_Click).Invoke;
			}
			else
			{
				toolStripMenuItem.Click += new EventHandler(CMRegion2_Copy_Click).Invoke;
			}
			toolStripMenuItem = new ToolStripMenuItem();
			toolStripMenuItem.Name = string.Concat(InContextMenu, "_Paste");
			toolStripMenuItem.Text = "Paste";
			InContextMenu.Items.Add(toolStripMenuItem);
			if (InRegion == 1)
			{
				toolStripMenuItem.Click += new EventHandler(CMRegion1_Paste_Click).Invoke;
			}
			else
			{
				toolStripMenuItem.Click += new EventHandler(CMRegion2_Paste_Click).Invoke;
			}
			if (!InSetChordsMenu)
			{
				return;
			}
			ToolStripSeparator toolStripSeparator = new ToolStripSeparator();
			toolStripSeparator = new ToolStripSeparator();
			InContextMenu.Items.Add(toolStripSeparator);
			ToolStripMenuItem toolStripMenuItem2 = new ToolStripMenuItem();
			toolStripMenuItem2 = new ToolStripMenuItem();
			toolStripMenuItem2.Text = "Minor";
			InContextMenu.Items.Add(toolStripMenuItem2);
			for (int i = 0; i < 12; i++)
			{
				toolStripMenuItem = new ToolStripMenuItem();
				toolStripMenuItem.Text = gf.MusicMinorChords[i, 0];
				toolStripMenuItem.Tag = InRegion.ToString();
				toolStripMenuItem2.DropDownItems.Add(toolStripMenuItem);
				toolStripMenuItem.Click += ContextMenuChords_Click;
				if (i == 1 || i == 3 || i == 6 || i == 8 || i == 11)
				{
					toolStripMenuItem = new ToolStripMenuItem();
					toolStripMenuItem.Text = gf.MusicMinorChords[i, 1];
					toolStripMenuItem.Tag = InRegion.ToString();
					toolStripMenuItem2.DropDownItems.Add(toolStripMenuItem);
					toolStripMenuItem.Click += ContextMenuChords_Click;
				}
			}
			toolStripMenuItem2 = new ToolStripMenuItem();
			toolStripMenuItem2.Text = "Minor 7th";
			InContextMenu.Items.Add(toolStripMenuItem2);
			for (int i = 0; i < 12; i++)
			{
				toolStripMenuItem = new ToolStripMenuItem();
				toolStripMenuItem.Text = gf.MusicMinorChords[i, 0] + "7";
				toolStripMenuItem.Tag = InRegion.ToString();
				toolStripMenuItem2.DropDownItems.Add(toolStripMenuItem);
				toolStripMenuItem.Click += ContextMenuChords_Click;
				if (i == 1 || i == 3 || i == 6 || i == 8 || i == 11)
				{
					toolStripMenuItem = new ToolStripMenuItem();
					toolStripMenuItem.Text = gf.MusicMinorChords[i, 1] + "7";
					toolStripMenuItem.Tag = InRegion.ToString();
					toolStripMenuItem2.DropDownItems.Add(toolStripMenuItem);
					toolStripMenuItem.Click += ContextMenuChords_Click;
				}
			}
			toolStripMenuItem2 = new ToolStripMenuItem();
			toolStripMenuItem2.Text = "Major 7th";
			InContextMenu.Items.Add(toolStripMenuItem2);
			for (int i = 0; i < 12; i++)
			{
				toolStripMenuItem = new ToolStripMenuItem();
				toolStripMenuItem.Text = gf.MusicMajorChords[i, 1] + "7";
				toolStripMenuItem.Tag = InRegion.ToString();
				toolStripMenuItem2.DropDownItems.Add(toolStripMenuItem);
				toolStripMenuItem.Click += ContextMenuChords_Click;
				if (i == 1 || i == 3 || i == 6 || i == 8 || i == 11)
				{
					toolStripMenuItem = new ToolStripMenuItem();
					toolStripMenuItem.Text = gf.MusicMajorChords[i, 0] + "7";
					toolStripMenuItem.Tag = InRegion.ToString();
					toolStripMenuItem2.DropDownItems.Add(toolStripMenuItem);
					toolStripMenuItem.Click += ContextMenuChords_Click;
				}
			}
			toolStripMenuItem2 = new ToolStripMenuItem();
			toolStripMenuItem2.Text = "Bass";
			InContextMenu.Items.Add(toolStripMenuItem2);
			for (int i = 0; i < 12; i++)
			{
				toolStripMenuItem = new ToolStripMenuItem();
				toolStripMenuItem.Text = "/" + gf.MusicMajorChords[i, 1];
				toolStripMenuItem.Tag = InRegion.ToString();
				toolStripMenuItem2.DropDownItems.Add(toolStripMenuItem);
				toolStripMenuItem.Click += ContextMenuChords_Click;
			}
			for (int i = 0; i < 12; i++)
			{
				toolStripMenuItem = new ToolStripMenuItem();
				toolStripMenuItem.Text = gf.MusicMajorChords[i, 1];
				toolStripMenuItem.Tag = InRegion.ToString();
				InContextMenu.Items.Add(toolStripMenuItem);
				toolStripMenuItem.Click += ContextMenuChords_Click;
				if (i == 1 || i == 3 || i == 6 || i == 8 || i == 11)
				{
					toolStripMenuItem = new ToolStripMenuItem();
					toolStripMenuItem.Text = gf.MusicMajorChords[i, 0];
					toolStripMenuItem.Tag = InRegion.ToString();
					InContextMenu.Items.Add(toolStripMenuItem);
					toolStripMenuItem.Click += ContextMenuChords_Click;
				}
			}
		}

		private void ContextMenuChords_Click(object sender, EventArgs e)
		{
			try
			{
				ToolStripMenuItem toolStripMenuItem = (ToolStripMenuItem)sender;
				if (DataUtil.ObjToInt(toolStripMenuItem.Tag) == 1)
				{
					tbLyrics1.SelectionStart = tbLyrics1MouseUpPos;
					tbLyrics1.SelectionLength = 0;
					InsertingPresetItem = true;
					gf.InsertChordAboveCurrentLine(ref tbLyrics1, toolStripMenuItem.Text);
					InsertingPresetItem = false;
					Lyrics_TextChanged(1);
				}
				else
				{
					tbLyrics2.SelectionStart = tbLyrics2MouseUpPos;
					tbLyrics2.SelectionLength = 0;
					InsertingPresetItem = true;
					gf.InsertChordAboveCurrentLine(ref tbLyrics2, toolStripMenuItem.Text);
					InsertingPresetItem = false;
					Lyrics_TextChanged(2);
				}
			}
			catch
			{
			}
		}

		private void RightToLeft_Click(object sender, EventArgs e)
		{
			CheckBox checkBox = (CheckBox)sender;
			string name = checkBox.Name;
			if (name == "R1RightToLeft")
			{
				SetRightToLeftAtRegion(1, checkBox.Checked);
			}
			else if (name == "R2RightToLeft")
			{
				SetRightToLeftAtRegion(2, checkBox.Checked);
			}
		}

		private void SetRightToLeftAtRegion(int InRegion, bool IsTrue)
		{
			if (InRegion == 1)
			{
				SetRightToLeft1 = IsTrue;
				tbLyrics1.RightToLeft = (IsTrue ? RightToLeft.Yes : RightToLeft.No);
				ClearErrorMessage(1);
				tbLyrics1.Invalidate();
			}
			else
			{
				SetRightToLeft2 = IsTrue;
				tbLyrics2.RightToLeft = (IsTrue ? RightToLeft.Yes : RightToLeft.No);
				ClearErrorMessage(2);
				tbLyrics2.Invalidate();
			}
		}

		private void tbLyrics1_MouseUp(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Right)
			{
				int num = tbLyrics1MouseUpPos = tbLyrics1.GetCharIndexFromPosition(new Point(e.X, e.Y));
			}
		}

		private void tbLyrics2_MouseUp(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Right)
			{
				int num = tbLyrics2MouseUpPos = tbLyrics2.GetCharIndexFromPosition(new Point(e.X, e.Y));
			}
		}

		private void Main_ChordsMenu_Click(object sender, EventArgs e)
		{
			SetMenuChordsMenu(Main_ChordsMenu.Checked);
		}

		private void Menu_ChordsMenu_Click(object sender, EventArgs e)
		{
			SetMenuChordsMenu(Menu_ChordsMenu.Checked);
		}

		private void SetMenuChordsMenu(bool IsChecked)
		{
			SetChordsMenu = IsChecked;
			Main_ChordsMenu.Checked = SetChordsMenu;
			Menu_ChordsMenu.Checked = SetChordsMenu;
			BuildLyricsContextMenu(1);
			BuildLyricsContextMenu(2);
		}
	}
}
