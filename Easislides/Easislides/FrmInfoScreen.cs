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
            components = new Container();
            ComponentResourceManager resources = new ComponentResourceManager(typeof(FrmInfoScreen));
            toolStrip1 = new ToolStrip();
            Main_New = new ToolStripButton();
            Main_Save = new ToolStripButton();
            Main_SaveExit = new ToolStripButton();
            toolStripSeparator1 = new ToolStripSeparator();
            Main_Import = new ToolStripButton();
            Main_WordWrap = new ToolStripButton();
            Main_ChordsMenu = new ToolStripButton();
            toolStripSeparator2 = new ToolStripSeparator();
            Main_TransposeDown = new ToolStripButton();
            Main_TransposeUp = new ToolStripButton();
            toolStripSeparator4 = new ToolStripSeparator();
            ComboFontName = new ToolStripComboBox();
            ComboMainFontSize = new ToolStripComboBox();
            ComboNotationFontSize = new ToolStripComboBox();
            statusStrip1 = new StatusStrip();
            toolStripStatusLabel1 = new ToolStripStatusLabel();
            splitContainer1 = new SplitContainer();
            tbLyrics1 = new RichTextBox();
            CMRegion1 = new ContextMenuStrip(components);
            CMRegion1_Copy = new ToolStripMenuItem();
            CMRegion1_Paste = new ToolStripMenuItem();
            panelR1Top = new Panel();
            LabeltbLyrics = new Label();
            panelR1Left = new Panel();
            panel3 = new Panel();
            panelR1LeftBottom = new Panel();
            R1RightToLeft = new CheckBox();
            R1Chinese = new Button();
            panelR1LeftMiddle = new Panel();
            R1BtnBridge2 = new Button();
            R1BtnPreChorus2 = new Button();
            R1BtnChorus2 = new Button();
            R1BtnPreChorus = new Button();
            R1BtnChorus = new Button();
            R1VerseFormat = new Button();
            R1BtnNewScreen = new Button();
            R1BtnNotations = new Button();
            R1BtnEnding = new Button();
            R1BtnBridge = new Button();
            R1Btn10 = new Button();
            R1Btn9 = new Button();
            R1Btn8 = new Button();
            R1Btn7 = new Button();
            R1Btn6 = new Button();
            R1Btn5 = new Button();
            R1Btn4 = new Button();
            R1Btn3 = new Button();
            R1Btn2 = new Button();
            R1Btn1 = new Button();
            panelR1LeftTop = new Panel();
            R1Undo = new Button();
            R1Redo = new Button();
            tabRightPane = new TabControl();
            tabRight_Region2 = new TabPage();
            panelR2All = new Panel();
            tbLyrics2 = new RichTextBox();
            CMRegion2 = new ContextMenuStrip(components);
            CMRegion2_Copy = new ToolStripMenuItem();
            CMRegion2_Paste = new ToolStripMenuItem();
            panelR2Top = new Panel();
            LabeltbLyrics2 = new Label();
            panelR2Left = new Panel();
            panel1 = new Panel();
            panelR2LeftBottom = new Panel();
            R2RightToLeft = new CheckBox();
            R2Chinese = new Button();
            panelR2LeftMiddle = new Panel();
            R2BtnBridge2 = new Button();
            R2BtnPreChorus2 = new Button();
            R2BtnPreChorus = new Button();
            R2VerseFormat = new Button();
            R2BtnChorus = new Button();
            R2BtnChorus2 = new Button();
            R2BtnNewScreen = new Button();
            R2BtnNotations = new Button();
            R2BtnEnding = new Button();
            R2BtnBridge = new Button();
            R2Btn10 = new Button();
            R2Btn9 = new Button();
            R2Btn8 = new Button();
            R2Btn7 = new Button();
            R2Btn6 = new Button();
            R2Btn5 = new Button();
            R2Btn4 = new Button();
            R2Btn3 = new Button();
            R2Btn2 = new Button();
            R2Btn1 = new Button();
            panelR2LeftTop = new Panel();
            R2Undo = new Button();
            R2Redo = new Button();
            SyncBtnDown = new Button();
            SyncBtnUp = new Button();
            tabRight_Rotate = new TabPage();
            panelRotate = new Panel();
            splitContainerRotate = new SplitContainer();
            groupBoxRotateVerses = new GroupBox();
            panelRotate_Verses = new Panel();
            Rotate_VersesList = new ListView();
            columnHeader6 = new ColumnHeader();
            columnHeader7 = new ColumnHeader();
            columnHeader8 = new ColumnHeader();
            panel10 = new Panel();
            label23 = new Label();
            panelRotate_OrderList = new Panel();
            Rotate_OrderList = new ListView();
            columnHeader9 = new ColumnHeader();
            columnHeader10 = new ColumnHeader();
            panel12 = new Panel();
            label24 = new Label();
            panel13 = new Panel();
            toolStripRotate_SeqSet = new ToolStrip();
            Rotate_Verses_Add = new ToolStripButton();
            Rotate_Verses_SmartAdd = new ToolStripButton();
            panel14 = new Panel();
            toolStripRotate_SeqUpDown = new ToolStrip();
            Rotate_OrderList_Up = new ToolStripButton();
            Rotate_OrderList_Down = new ToolStripButton();
            toolStripSeparator3 = new ToolStripSeparator();
            Rotate_OrderList_Delete = new ToolStripButton();
            panelRotate_Sample = new Panel();
            labelDur = new Label();
            btnAddPosition = new Button();
            btnDuration = new Button();
            LabelMediaType = new Label();
            labelMed = new Label();
            panelRotate_Media = new Panel();
            panelNoPlayer = new Panel();
            label14 = new Label();
            labelNoPlayer1 = new Label();
            labelNoPlayer2 = new Label();
            labelPos = new Label();
            LabelPosition = new Label();
            panelLoc = new Panel();
            toolStrip3 = new ToolStrip();
            Rotate_LocationBtn = new ToolStripButton();
            LabelDuration = new Label();
            Rotate_tbSourceLocation = new TextBox();
            TrackBarVolume = new TrackBar();
            panelPlayBtns = new Panel();
            TrackBarDuration = new TrackBar();
            StopBtn = new Button();
            PlayPauseBtn = new Button();
            FastReverseBtn = new Button();
            FastForwardBtn = new Button();
            labelVol = new Label();
            panelRotateLeft = new Panel();
            flowLayoutRotate = new FlowLayoutPanel();
            panelRotateLeftTop2 = new Panel();
            Rotate_TimeTotal = new DateTimePicker();
            btnClearMediaPositions = new Button();
            label21 = new Label();
            label19 = new Label();
            panelRotateLeftTop1 = new Panel();
            groupBox3 = new GroupBox();
            Rotate_SlidesGapUpDown = new NumericUpDown();
            Rotate_Equal = new RadioButton();
            Rotate_Multiple = new RadioButton();
            panel6 = new Panel();
            menuStripMain = new MenuStrip();
            Menu_MainFile = new ToolStripMenuItem();
            Menu_New = new ToolStripMenuItem();
            Menu_Save = new ToolStripMenuItem();
            Menu_SaveAs = new ToolStripMenuItem();
            Menu_SaveExit = new ToolStripMenuItem();
            toolStripSeparator16 = new ToolStripSeparator();
            Menu_EditHistoryList = new ToolStripMenuItem();
            toolStripSeparator18 = new ToolStripSeparator();
            Menu_Exit = new ToolStripMenuItem();
            Menu_MainTools = new ToolStripMenuItem();
            Menu_Import = new ToolStripMenuItem();
            Menu_WordWrap = new ToolStripMenuItem();
            Menu_ChordsMenu = new ToolStripMenuItem();
            Menu_EditHistorySeparator = new ToolStripSeparator();
            Menu_TransposeDown = new ToolStripMenuItem();
            Menu_TransposeUp = new ToolStripMenuItem();
            toolStripSeparator6 = new ToolStripSeparator();
            Menu_ShowAllButtons = new ToolStripMenuItem();
            TimerEditRequest = new Timer(components);
            OpenFileDialog1 = new OpenFileDialog();
            splitContainerMain = new SplitContainer();
            groupBox2 = new GroupBox();
            panelVerses = new Panel();
            VersesList = new ListView();
            columnHeader1 = new ColumnHeader();
            columnHeader2 = new ColumnHeader();
            panel2 = new Panel();
            label16 = new Label();
            panelOrderList = new Panel();
            OrderList = new ListView();
            columnHeader3 = new ColumnHeader();
            columnHeader4 = new ColumnHeader();
            panel4 = new Panel();
            label17 = new Label();
            panelSeqSet = new Panel();
            toolStripSeqSet = new ToolStrip();
            Verses_Add = new ToolStripButton();
            Verses_SmartAdd = new ToolStripButton();
            panelSeqUpDown = new Panel();
            toolStripSeqUpDown = new ToolStrip();
            OrderList_Up = new ToolStripButton();
            OrderList_Down = new ToolStripButton();
            toolStripSeparator5 = new ToolStripSeparator();
            OrderList_Delete = new ToolStripButton();
            groupBox1 = new GroupBox();
            panel7 = new Panel();
            Btn_Title2 = new Button();
            Btn_Title = new Button();
            Btn_Copyright = new Button();
            Btn_Writer = new Button();
            SongFolder = new ComboBox();
            panelLinkTitle2Lookup = new Panel();
            toolStrip2 = new ToolStrip();
            Title2_LookUp = new ToolStripButton();
            LinkTitle2Pic = new Panel();
            CopyrightInfo = new TextBox();
            label2 = new Label();
            WriterInfo = new TextBox();
            label3 = new Label();
            SongTitle2 = new TextBox();
            label4 = new Label();
            SongTitle = new TextBox();
            label5 = new Label();
            labelFormat = new Label();
            panel8 = new Panel();
            Btn_BookRef = new Button();
            Btn_UserRef = new Button();
            label6 = new Label();
            UserReference = new TextBox();
            BookReference = new TextBox();
            label9 = new Label();
            label10 = new Label();
            LicAdminInfo2 = new ComboBox();
            LicAdminInfo1 = new ComboBox();
            SongTiming = new ComboBox();
            label13 = new Label();
            SongKey = new ComboBox();
            SongNumber = new TextBox();
            SongCapo = new ComboBox();
            label11 = new Label();
            label12 = new Label();
            label8 = new Label();
            label7 = new Label();
            toolTip1 = new ToolTip(components);
            TimerFast = new Timer(components);
            TimerAttemptConnect = new Timer(components);
            TimerTrack = new Timer(components);
            saveFileDialog1 = new SaveFileDialog();
            toolStrip1.SuspendLayout();
            statusStrip1.SuspendLayout();
            ((ISupportInitialize)splitContainer1).BeginInit();
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
            ((ISupportInitialize)splitContainerRotate).BeginInit();
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
            ((ISupportInitialize)TrackBarVolume).BeginInit();
            panelPlayBtns.SuspendLayout();
            ((ISupportInitialize)TrackBarDuration).BeginInit();
            panelRotateLeft.SuspendLayout();
            panelRotateLeftTop2.SuspendLayout();
            panelRotateLeftTop1.SuspendLayout();
            groupBox3.SuspendLayout();
            ((ISupportInitialize)Rotate_SlidesGapUpDown).BeginInit();
            menuStripMain.SuspendLayout();
            ((ISupportInitialize)splitContainerMain).BeginInit();
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
            // 
            // toolStrip1
            // 
            toolStrip1.ImageScalingSize = new Size(20, 20);
            toolStrip1.Items.AddRange(new ToolStripItem[] { Main_New, Main_Save, Main_SaveExit, toolStripSeparator1, Main_Import, Main_WordWrap, Main_ChordsMenu, toolStripSeparator2, Main_TransposeDown, Main_TransposeUp, toolStripSeparator4, ComboFontName, ComboMainFontSize, ComboNotationFontSize });
            toolStrip1.Location = new Point(0, 30);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.Size = new Size(1033, 28);
            toolStrip1.TabIndex = 0;
            toolStrip1.Text = "toolStrip1";
            // 
            // Main_New
            // 
            Main_New.DisplayStyle = ToolStripItemDisplayStyle.Image;
            Main_New.Image = Resources.New;
            Main_New.ImageTransparentColor = Color.Magenta;
            Main_New.Name = "Main_New";
            Main_New.Size = new Size(29, 25);
            Main_New.ToolTipText = "New Item";
            Main_New.Click += Main_New_Click;
            // 
            // Main_Save
            // 
            Main_Save.DisplayStyle = ToolStripItemDisplayStyle.Image;
            Main_Save.Image = Resources.Save;
            Main_Save.ImageTransparentColor = Color.Magenta;
            Main_Save.Name = "Main_Save";
            Main_Save.Size = new Size(29, 25);
            Main_Save.ToolTipText = "Save Item";
            Main_Save.Click += Main_Save_Click;
            // 
            // Main_SaveExit
            // 
            Main_SaveExit.DisplayStyle = ToolStripItemDisplayStyle.Image;
            Main_SaveExit.Image = Resources.SaveClose;
            Main_SaveExit.ImageTransparentColor = Color.Magenta;
            Main_SaveExit.Name = "Main_SaveExit";
            Main_SaveExit.Size = new Size(29, 25);
            Main_SaveExit.ToolTipText = "Save and Exit";
            Main_SaveExit.Click += Main_SaveExit_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(6, 28);
            // 
            // Main_Import
            // 
            Main_Import.DisplayStyle = ToolStripItemDisplayStyle.Image;
            Main_Import.Image = Resources.open;
            Main_Import.ImageTransparentColor = Color.Magenta;
            Main_Import.Name = "Main_Import";
            Main_Import.Size = new Size(29, 25);
            Main_Import.Text = "toolStripButton5";
            Main_Import.ToolTipText = "Import Word/Text File";
            Main_Import.Click += Main_Import_Click;
            // 
            // Main_WordWrap
            // 
            Main_WordWrap.CheckOnClick = true;
            Main_WordWrap.DisplayStyle = ToolStripItemDisplayStyle.Image;
            Main_WordWrap.Image = Resources.wordwrap;
            Main_WordWrap.ImageTransparentColor = Color.Magenta;
            Main_WordWrap.Name = "Main_WordWrap";
            Main_WordWrap.Size = new Size(29, 25);
            Main_WordWrap.ToolTipText = "Word Wrap";
            Main_WordWrap.Click += Main_WordWrap_Click;
            // 
            // Main_ChordsMenu
            // 
            Main_ChordsMenu.CheckOnClick = true;
            Main_ChordsMenu.DisplayStyle = ToolStripItemDisplayStyle.Image;
            Main_ChordsMenu.Image = Resources.PopUpChords;
            Main_ChordsMenu.ImageTransparentColor = Color.Magenta;
            Main_ChordsMenu.Name = "Main_ChordsMenu";
            Main_ChordsMenu.Size = new Size(29, 25);
            Main_ChordsMenu.ToolTipText = "Right Click Chords Menu";
            Main_ChordsMenu.Click += Main_ChordsMenu_Click;
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new Size(6, 28);
            // 
            // Main_TransposeDown
            // 
            Main_TransposeDown.DisplayStyle = ToolStripItemDisplayStyle.Image;
            Main_TransposeDown.Image = Resources.arrowGL;
            Main_TransposeDown.ImageTransparentColor = Color.Magenta;
            Main_TransposeDown.Name = "Main_TransposeDown";
            Main_TransposeDown.Size = new Size(29, 25);
            Main_TransposeDown.ToolTipText = "Transpose Chord Down";
            Main_TransposeDown.Click += Main_TransposeDown_Click;
            // 
            // Main_TransposeUp
            // 
            Main_TransposeUp.DisplayStyle = ToolStripItemDisplayStyle.Image;
            Main_TransposeUp.Image = Resources.arrowGR;
            Main_TransposeUp.ImageTransparentColor = Color.Magenta;
            Main_TransposeUp.Name = "Main_TransposeUp";
            Main_TransposeUp.Size = new Size(29, 25);
            Main_TransposeUp.ToolTipText = "Transpose Chord Up";
            Main_TransposeUp.Click += Main_TransposeUp_Click;
            // 
            // toolStripSeparator4
            // 
            toolStripSeparator4.Name = "toolStripSeparator4";
            toolStripSeparator4.Size = new Size(6, 28);
            // 
            // ComboFontName
            // 
            ComboFontName.AutoSize = false;
            ComboFontName.Name = "ComboFontName";
            ComboFontName.Size = new Size(160, 28);
            ComboFontName.Text = "Font Name";
            ComboFontName.ToolTipText = "Font Name";
            ComboFontName.SelectedIndexChanged += ComboFonts_SelectedIndexChanged;
            // 
            // ComboMainFontSize
            // 
            ComboMainFontSize.AutoSize = false;
            ComboMainFontSize.Name = "ComboMainFontSize";
            ComboMainFontSize.Size = new Size(52, 28);
            ComboMainFontSize.Text = "12";
            ComboMainFontSize.ToolTipText = "Font Size";
            ComboMainFontSize.SelectedIndexChanged += ComboFonts_SelectedIndexChanged;
            // 
            // ComboNotationFontSize
            // 
            ComboNotationFontSize.AutoSize = false;
            ComboNotationFontSize.Name = "ComboNotationFontSize";
            ComboNotationFontSize.Size = new Size(52, 28);
            ComboNotationFontSize.Text = "10";
            ComboNotationFontSize.ToolTipText = "Notation Size";
            ComboNotationFontSize.SelectedIndexChanged += ComboFonts_SelectedIndexChanged;
            // 
            // statusStrip1
            // 
            statusStrip1.BackColor = Color.LimeGreen;
            statusStrip1.ImageScalingSize = new Size(20, 20);
            statusStrip1.Items.AddRange(new ToolStripItem[] { toolStripStatusLabel1 });
            statusStrip1.Location = new Point(0, 841);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Padding = new Padding(1, 0, 19, 0);
            statusStrip1.Size = new Size(1033, 22);
            statusStrip1.TabIndex = 2;
            // 
            // toolStripStatusLabel1
            // 
            toolStripStatusLabel1.BackColor = Color.FromArgb(0, 192, 0);
            toolStripStatusLabel1.Font = new Font("Microsoft Sans Serif", 11.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            toolStripStatusLabel1.ForeColor = Color.Black;
            toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            toolStripStatusLabel1.Size = new Size(0, 16);
            // 
            // splitContainer1
            // 
            splitContainer1.BorderStyle = BorderStyle.Fixed3D;
            splitContainer1.Dock = DockStyle.Fill;
            splitContainer1.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            splitContainer1.Location = new Point(0, 0);
            splitContainer1.Margin = new Padding(4, 5, 4, 5);
            splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(tbLyrics1);
            splitContainer1.Panel1.Controls.Add(panelR1Top);
            splitContainer1.Panel1.Controls.Add(panelR1Left);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(tabRightPane);
            splitContainer1.Panel2.Controls.Add(panel6);
            splitContainer1.Panel2.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            splitContainer1.Size = new Size(1033, 575);
            splitContainer1.SplitterDistance = 467;
            splitContainer1.SplitterWidth = 5;
            splitContainer1.TabIndex = 0;
            splitContainer1.Text = "splitContainer1";
            // 
            // tbLyrics1
            // 
            tbLyrics1.AutoWordSelection = true;
            tbLyrics1.ContextMenuStrip = CMRegion1;
            tbLyrics1.DetectUrls = false;
            tbLyrics1.Dock = DockStyle.Fill;
            tbLyrics1.EnableAutoDragDrop = true;
            tbLyrics1.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            tbLyrics1.HideSelection = false;
            tbLyrics1.Location = new Point(73, 25);
            tbLyrics1.Margin = new Padding(4, 5, 4, 5);
            tbLyrics1.Name = "tbLyrics1";
            tbLyrics1.Size = new Size(390, 546);
            tbLyrics1.TabIndex = 0;
            tbLyrics1.Text = "";
            tbLyrics1.SelectionChanged += tbLyrics_SelectionChanged;
            tbLyrics1.TextChanged += tbLyrics1_TextChanged;
            tbLyrics1.KeyDown += tbLyrics1_KeyDown;
            tbLyrics1.KeyUp += tbLyrics1_KeyUp;
            tbLyrics1.MouseUp += tbLyrics1_MouseUp;
            // 
            // CMRegion1
            // 
            CMRegion1.ImageScalingSize = new Size(20, 20);
            CMRegion1.Items.AddRange(new ToolStripItem[] { CMRegion1_Copy, CMRegion1_Paste });
            CMRegion1.Name = "CMRegion1";
            CMRegion1.Size = new Size(113, 52);
            // 
            // CMRegion1_Copy
            // 
            CMRegion1_Copy.Name = "CMRegion1_Copy";
            CMRegion1_Copy.Size = new Size(112, 24);
            CMRegion1_Copy.Text = "Copy";
            CMRegion1_Copy.Click += CMRegion1_Copy_Click;
            // 
            // CMRegion1_Paste
            // 
            CMRegion1_Paste.Name = "CMRegion1_Paste";
            CMRegion1_Paste.Size = new Size(112, 24);
            CMRegion1_Paste.Text = "Paste";
            CMRegion1_Paste.Click += CMRegion1_Paste_Click;
            // 
            // panelR1Top
            // 
            panelR1Top.Controls.Add(LabeltbLyrics);
            panelR1Top.Dock = DockStyle.Top;
            panelR1Top.Location = new Point(73, 0);
            panelR1Top.Margin = new Padding(4, 5, 4, 5);
            panelR1Top.Name = "panelR1Top";
            panelR1Top.Size = new Size(390, 25);
            panelR1Top.TabIndex = 0;
            // 
            // LabeltbLyrics
            // 
            LabeltbLyrics.Location = new Point(4, 2);
            LabeltbLyrics.Margin = new Padding(4, 0, 4, 0);
            LabeltbLyrics.Name = "LabeltbLyrics";
            LabeltbLyrics.Size = new Size(461, 20);
            LabeltbLyrics.TabIndex = 0;
            LabeltbLyrics.Text = "Region 1";
            // 
            // panelR1Left
            // 
            panelR1Left.Controls.Add(panel3);
            panelR1Left.Dock = DockStyle.Left;
            panelR1Left.Location = new Point(0, 0);
            panelR1Left.Margin = new Padding(4, 5, 4, 5);
            panelR1Left.Name = "panelR1Left";
            panelR1Left.Size = new Size(73, 571);
            panelR1Left.TabIndex = 1;
            // 
            // panel3
            // 
            panel3.Controls.Add(panelR1LeftBottom);
            panel3.Controls.Add(panelR1LeftMiddle);
            panel3.Controls.Add(panelR1LeftTop);
            panel3.Location = new Point(5, 25);
            panel3.Margin = new Padding(4, 5, 4, 5);
            panel3.Name = "panel3";
            panel3.Size = new Size(64, 429);
            panel3.TabIndex = 2;
            // 
            // panelR1LeftBottom
            // 
            panelR1LeftBottom.Controls.Add(R1RightToLeft);
            panelR1LeftBottom.Controls.Add(R1Chinese);
            panelR1LeftBottom.Dock = DockStyle.Top;
            panelR1LeftBottom.Location = new Point(0, 391);
            panelR1LeftBottom.Margin = new Padding(4, 5, 4, 5);
            panelR1LeftBottom.Name = "panelR1LeftBottom";
            panelR1LeftBottom.Size = new Size(64, 37);
            panelR1LeftBottom.TabIndex = 2;
            // 
            // R1RightToLeft
            // 
            R1RightToLeft.Appearance = Appearance.Button;
            R1RightToLeft.FlatStyle = FlatStyle.Flat;
            R1RightToLeft.Image = Resources.LangRightLeft;
            R1RightToLeft.Location = new Point(29, 2);
            R1RightToLeft.Margin = new Padding(4, 5, 4, 5);
            R1RightToLeft.Name = "R1RightToLeft";
            R1RightToLeft.Size = new Size(31, 35);
            R1RightToLeft.TabIndex = 3;
            toolTip1.SetToolTip(R1RightToLeft, "Right-To-Left Text");
            R1RightToLeft.UseVisualStyleBackColor = true;
            R1RightToLeft.Visible = false;
            R1RightToLeft.Click += RightToLeft_Click;
            // 
            // R1Chinese
            // 
            R1Chinese.FlatStyle = FlatStyle.Flat;
            R1Chinese.Image = Resources.Chinese;
            R1Chinese.Location = new Point(0, 2);
            R1Chinese.Margin = new Padding(4, 5, 4, 5);
            R1Chinese.Name = "R1Chinese";
            R1Chinese.Size = new Size(31, 35);
            R1Chinese.TabIndex = 22;
            R1Chinese.Tag = "";
            toolTip1.SetToolTip(R1Chinese, "Siwtch Trad/Simp Chinese");
            R1Chinese.Click += R1Chinese_Click;
            // 
            // panelR1LeftMiddle
            // 
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
            panelR1LeftMiddle.Dock = DockStyle.Top;
            panelR1LeftMiddle.Location = new Point(0, 37);
            panelR1LeftMiddle.Margin = new Padding(4, 5, 4, 5);
            panelR1LeftMiddle.Name = "panelR1LeftMiddle";
            panelR1LeftMiddle.Size = new Size(64, 354);
            panelR1LeftMiddle.TabIndex = 0;
            // 
            // R1BtnBridge2
            // 
            R1BtnBridge2.FlatStyle = FlatStyle.Flat;
            R1BtnBridge2.Image = Resources.NumBridge2;
            R1BtnBridge2.Location = new Point(31, 248);
            R1BtnBridge2.Margin = new Padding(4, 5, 4, 5);
            R1BtnBridge2.Name = "R1BtnBridge2";
            R1BtnBridge2.Size = new Size(31, 35);
            R1BtnBridge2.TabIndex = 17;
            R1BtnBridge2.Tag = "103";
            toolTip1.SetToolTip(R1BtnBridge2, "Bridge 2 Indicator");
            R1BtnBridge2.Click += R1Btn_Click;
            // 
            // R1BtnPreChorus2
            // 
            R1BtnPreChorus2.FlatStyle = FlatStyle.Flat;
            R1BtnPreChorus2.Image = Resources.NumPreChorus2;
            R1BtnPreChorus2.Location = new Point(31, 177);
            R1BtnPreChorus2.Margin = new Padding(4, 5, 4, 5);
            R1BtnPreChorus2.Name = "R1BtnPreChorus2";
            R1BtnPreChorus2.Size = new Size(31, 35);
            R1BtnPreChorus2.TabIndex = 13;
            R1BtnPreChorus2.Tag = "112";
            toolTip1.SetToolTip(R1BtnPreChorus2, "Prechorus2 Indicator");
            R1BtnPreChorus2.Click += R1Btn_Click;
            // 
            // R1BtnChorus2
            // 
            R1BtnChorus2.FlatStyle = FlatStyle.Flat;
            R1BtnChorus2.Image = Resources.NumChorus2;
            R1BtnChorus2.Location = new Point(31, 212);
            R1BtnChorus2.Margin = new Padding(4, 5, 4, 5);
            R1BtnChorus2.Name = "R1BtnChorus2";
            R1BtnChorus2.Size = new Size(31, 35);
            R1BtnChorus2.TabIndex = 15;
            R1BtnChorus2.Tag = "102";
            toolTip1.SetToolTip(R1BtnChorus2, "Chrous 2 Indicator");
            R1BtnChorus2.Click += R1Btn_Click;
            // 
            // R1BtnPreChorus
            // 
            R1BtnPreChorus.FlatStyle = FlatStyle.Flat;
            R1BtnPreChorus.Image = Resources.NumPreChorus;
            R1BtnPreChorus.Location = new Point(0, 177);
            R1BtnPreChorus.Margin = new Padding(4, 5, 4, 5);
            R1BtnPreChorus.Name = "R1BtnPreChorus";
            R1BtnPreChorus.Size = new Size(31, 35);
            R1BtnPreChorus.TabIndex = 12;
            R1BtnPreChorus.Tag = "111";
            toolTip1.SetToolTip(R1BtnPreChorus, "Prechorus Indicator");
            R1BtnPreChorus.Click += R1Btn_Click;
            // 
            // R1BtnChorus
            // 
            R1BtnChorus.FlatStyle = FlatStyle.Flat;
            R1BtnChorus.Image = Resources.NumChorus;
            R1BtnChorus.Location = new Point(0, 212);
            R1BtnChorus.Margin = new Padding(4, 5, 4, 5);
            R1BtnChorus.Name = "R1BtnChorus";
            R1BtnChorus.Size = new Size(31, 35);
            R1BtnChorus.TabIndex = 14;
            R1BtnChorus.Tag = "0";
            toolTip1.SetToolTip(R1BtnChorus, "Chorus Indicator");
            R1BtnChorus.Click += R1Btn_Click;
            // 
            // R1VerseFormat
            // 
            R1VerseFormat.FlatStyle = FlatStyle.Flat;
            R1VerseFormat.Image = Resources.VerseFormat;
            R1VerseFormat.Location = new Point(31, 318);
            R1VerseFormat.Margin = new Padding(4, 5, 4, 5);
            R1VerseFormat.Name = "R1VerseFormat";
            R1VerseFormat.Size = new Size(31, 35);
            R1VerseFormat.TabIndex = 21;
            R1VerseFormat.Tag = "";
            toolTip1.SetToolTip(R1VerseFormat, "Verses Format");
            R1VerseFormat.Click += R1VerseFormat_Click;
            // 
            // R1BtnNewScreen
            // 
            R1BtnNewScreen.FlatStyle = FlatStyle.Flat;
            R1BtnNewScreen.Image = Resources.NumNewScreen;
            R1BtnNewScreen.Location = new Point(31, 283);
            R1BtnNewScreen.Margin = new Padding(4, 5, 4, 5);
            R1BtnNewScreen.Name = "R1BtnNewScreen";
            R1BtnNewScreen.Size = new Size(31, 35);
            R1BtnNewScreen.TabIndex = 19;
            R1BtnNewScreen.Tag = "151";
            toolTip1.SetToolTip(R1BtnNewScreen, "New Screen Indicator");
            R1BtnNewScreen.Click += R1Btn_Click;
            // 
            // R1BtnNotations
            // 
            R1BtnNotations.FlatStyle = FlatStyle.Flat;
            R1BtnNotations.Image = Resources.NotationSym;
            R1BtnNotations.Location = new Point(0, 318);
            R1BtnNotations.Margin = new Padding(4, 5, 4, 5);
            R1BtnNotations.Name = "R1BtnNotations";
            R1BtnNotations.Size = new Size(31, 35);
            R1BtnNotations.TabIndex = 20;
            R1BtnNotations.Tag = "152";
            toolTip1.SetToolTip(R1BtnNotations, "Notations Indicator (F8)");
            R1BtnNotations.Click += R1Btn_Click;
            // 
            // R1BtnEnding
            // 
            R1BtnEnding.FlatStyle = FlatStyle.Flat;
            R1BtnEnding.Image = Resources.NumEnding;
            R1BtnEnding.Location = new Point(0, 283);
            R1BtnEnding.Margin = new Padding(4, 5, 4, 5);
            R1BtnEnding.Name = "R1BtnEnding";
            R1BtnEnding.Size = new Size(31, 35);
            R1BtnEnding.TabIndex = 18;
            R1BtnEnding.Tag = "101";
            toolTip1.SetToolTip(R1BtnEnding, "Ending Indicator");
            R1BtnEnding.Click += R1Btn_Click;
            // 
            // R1BtnBridge
            // 
            R1BtnBridge.FlatStyle = FlatStyle.Flat;
            R1BtnBridge.Image = Resources.NumBridge;
            R1BtnBridge.Location = new Point(0, 248);
            R1BtnBridge.Margin = new Padding(4, 5, 4, 5);
            R1BtnBridge.Name = "R1BtnBridge";
            R1BtnBridge.Size = new Size(31, 35);
            R1BtnBridge.TabIndex = 16;
            R1BtnBridge.Tag = "100";
            toolTip1.SetToolTip(R1BtnBridge, "Bridge Indicator");
            R1BtnBridge.Click += R1Btn_Click;
            // 
            // R1Btn10
            // 
            R1Btn10.FlatStyle = FlatStyle.Flat;
            R1Btn10.Image = Resources.Num10;
            R1Btn10.Location = new Point(31, 142);
            R1Btn10.Margin = new Padding(4, 5, 4, 5);
            R1Btn10.Name = "R1Btn10";
            R1Btn10.Size = new Size(31, 35);
            R1Btn10.TabIndex = 11;
            R1Btn10.Tag = "10";
            R1Btn10.Click += R1Btn_Click;
            // 
            // R1Btn9
            // 
            R1Btn9.FlatStyle = FlatStyle.Flat;
            R1Btn9.Image = Resources.Num9;
            R1Btn9.Location = new Point(0, 142);
            R1Btn9.Margin = new Padding(4, 5, 4, 5);
            R1Btn9.Name = "R1Btn9";
            R1Btn9.Size = new Size(31, 35);
            R1Btn9.TabIndex = 10;
            R1Btn9.Tag = "9";
            R1Btn9.Click += R1Btn_Click;
            // 
            // R1Btn8
            // 
            R1Btn8.FlatStyle = FlatStyle.Flat;
            R1Btn8.Image = Resources.Num8;
            R1Btn8.Location = new Point(31, 106);
            R1Btn8.Margin = new Padding(4, 5, 4, 5);
            R1Btn8.Name = "R1Btn8";
            R1Btn8.Size = new Size(31, 35);
            R1Btn8.TabIndex = 9;
            R1Btn8.Tag = "8";
            R1Btn8.Click += R1Btn_Click;
            // 
            // R1Btn7
            // 
            R1Btn7.FlatStyle = FlatStyle.Flat;
            R1Btn7.Image = Resources.Num7;
            R1Btn7.Location = new Point(0, 106);
            R1Btn7.Margin = new Padding(4, 5, 4, 5);
            R1Btn7.Name = "R1Btn7";
            R1Btn7.Size = new Size(31, 35);
            R1Btn7.TabIndex = 8;
            R1Btn7.Tag = "7";
            R1Btn7.Click += R1Btn_Click;
            // 
            // R1Btn6
            // 
            R1Btn6.FlatStyle = FlatStyle.Flat;
            R1Btn6.Image = Resources.Num6;
            R1Btn6.Location = new Point(31, 71);
            R1Btn6.Margin = new Padding(4, 5, 4, 5);
            R1Btn6.Name = "R1Btn6";
            R1Btn6.Size = new Size(31, 35);
            R1Btn6.TabIndex = 7;
            R1Btn6.Tag = "6";
            R1Btn6.Click += R1Btn_Click;
            // 
            // R1Btn5
            // 
            R1Btn5.FlatStyle = FlatStyle.Flat;
            R1Btn5.Image = Resources.Num5;
            R1Btn5.Location = new Point(0, 71);
            R1Btn5.Margin = new Padding(4, 5, 4, 5);
            R1Btn5.Name = "R1Btn5";
            R1Btn5.Size = new Size(31, 35);
            R1Btn5.TabIndex = 6;
            R1Btn5.Tag = "5";
            R1Btn5.Click += R1Btn_Click;
            // 
            // R1Btn4
            // 
            R1Btn4.FlatStyle = FlatStyle.Flat;
            R1Btn4.Image = Resources.Num4;
            R1Btn4.Location = new Point(31, 35);
            R1Btn4.Margin = new Padding(4, 5, 4, 5);
            R1Btn4.Name = "R1Btn4";
            R1Btn4.Size = new Size(31, 35);
            R1Btn4.TabIndex = 5;
            R1Btn4.Tag = "4";
            R1Btn4.Click += R1Btn_Click;
            // 
            // R1Btn3
            // 
            R1Btn3.FlatStyle = FlatStyle.Flat;
            R1Btn3.Image = Resources.Num3;
            R1Btn3.Location = new Point(0, 35);
            R1Btn3.Margin = new Padding(4, 5, 4, 5);
            R1Btn3.Name = "R1Btn3";
            R1Btn3.Size = new Size(31, 35);
            R1Btn3.TabIndex = 4;
            R1Btn3.Tag = "3";
            R1Btn3.Click += R1Btn_Click;
            // 
            // R1Btn2
            // 
            R1Btn2.FlatStyle = FlatStyle.Flat;
            R1Btn2.Image = Resources.Num2;
            R1Btn2.Location = new Point(31, 0);
            R1Btn2.Margin = new Padding(4, 5, 4, 5);
            R1Btn2.Name = "R1Btn2";
            R1Btn2.Size = new Size(31, 35);
            R1Btn2.TabIndex = 3;
            R1Btn2.Tag = "2";
            R1Btn2.Click += R1Btn_Click;
            // 
            // R1Btn1
            // 
            R1Btn1.FlatStyle = FlatStyle.Flat;
            R1Btn1.Image = Resources.Num1;
            R1Btn1.Location = new Point(0, 0);
            R1Btn1.Margin = new Padding(4, 5, 4, 5);
            R1Btn1.Name = "R1Btn1";
            R1Btn1.Size = new Size(31, 35);
            R1Btn1.TabIndex = 2;
            R1Btn1.Tag = "1";
            toolTip1.SetToolTip(R1Btn1, "Verse Indicator");
            R1Btn1.Click += R1Btn_Click;
            // 
            // panelR1LeftTop
            // 
            panelR1LeftTop.Controls.Add(R1Undo);
            panelR1LeftTop.Controls.Add(R1Redo);
            panelR1LeftTop.Dock = DockStyle.Top;
            panelR1LeftTop.Location = new Point(0, 0);
            panelR1LeftTop.Margin = new Padding(4, 5, 4, 5);
            panelR1LeftTop.Name = "panelR1LeftTop";
            panelR1LeftTop.Size = new Size(64, 37);
            panelR1LeftTop.TabIndex = 2;
            // 
            // R1Undo
            // 
            R1Undo.FlatStyle = FlatStyle.Flat;
            R1Undo.Image = Resources.undo;
            R1Undo.Location = new Point(0, 2);
            R1Undo.Margin = new Padding(4, 5, 4, 5);
            R1Undo.Name = "R1Undo";
            R1Undo.Size = new Size(31, 35);
            R1Undo.TabIndex = 0;
            R1Undo.Tag = "0";
            toolTip1.SetToolTip(R1Undo, "Undo");
            R1Undo.Click += R1UndoRedo_Click;
            // 
            // R1Redo
            // 
            R1Redo.FlatStyle = FlatStyle.Flat;
            R1Redo.Image = Resources.redo;
            R1Redo.Location = new Point(31, 2);
            R1Redo.Margin = new Padding(4, 5, 4, 5);
            R1Redo.Name = "R1Redo";
            R1Redo.Size = new Size(31, 35);
            R1Redo.TabIndex = 1;
            R1Redo.Tag = "1";
            toolTip1.SetToolTip(R1Redo, "Redo");
            R1Redo.Click += R1UndoRedo_Click;
            // 
            // tabRightPane
            // 
            tabRightPane.Controls.Add(tabRight_Region2);
            tabRightPane.Controls.Add(tabRight_Rotate);
            tabRightPane.Dock = DockStyle.Fill;
            tabRightPane.Location = new Point(0, 0);
            tabRightPane.Margin = new Padding(4, 5, 4, 5);
            tabRightPane.Multiline = true;
            tabRightPane.Name = "tabRightPane";
            tabRightPane.SelectedIndex = 0;
            tabRightPane.Size = new Size(553, 571);
            tabRightPane.TabIndex = 0;
            // 
            // tabRight_Region2
            // 
            tabRight_Region2.BackColor = SystemColors.Control;
            tabRight_Region2.Controls.Add(panelR2All);
            tabRight_Region2.Location = new Point(4, 26);
            tabRight_Region2.Margin = new Padding(4, 5, 4, 5);
            tabRight_Region2.Name = "tabRight_Region2";
            tabRight_Region2.Padding = new Padding(4, 5, 4, 5);
            tabRight_Region2.Size = new Size(545, 541);
            tabRight_Region2.TabIndex = 0;
            tabRight_Region2.Text = "Region 2";
            // 
            // panelR2All
            // 
            panelR2All.Controls.Add(tbLyrics2);
            panelR2All.Controls.Add(panelR2Top);
            panelR2All.Controls.Add(panelR2Left);
            panelR2All.Dock = DockStyle.Fill;
            panelR2All.Location = new Point(4, 5);
            panelR2All.Margin = new Padding(4, 5, 4, 5);
            panelR2All.Name = "panelR2All";
            panelR2All.Size = new Size(537, 531);
            panelR2All.TabIndex = 7;
            // 
            // tbLyrics2
            // 
            tbLyrics2.ContextMenuStrip = CMRegion2;
            tbLyrics2.DetectUrls = false;
            tbLyrics2.Dock = DockStyle.Fill;
            tbLyrics2.EnableAutoDragDrop = true;
            tbLyrics2.HideSelection = false;
            tbLyrics2.Location = new Point(96, 25);
            tbLyrics2.Margin = new Padding(4, 5, 4, 5);
            tbLyrics2.Name = "tbLyrics2";
            tbLyrics2.Size = new Size(441, 506);
            tbLyrics2.TabIndex = 5;
            tbLyrics2.Text = "";
            tbLyrics2.SelectionChanged += tbLyrics2_SelectionChanged;
            tbLyrics2.TextChanged += tbLyrics2_TextChanged;
            tbLyrics2.KeyDown += tbLyrics2_KeyDown;
            tbLyrics2.KeyUp += tbLyrics2_KeyUp;
            tbLyrics2.MouseUp += tbLyrics2_MouseUp;
            // 
            // CMRegion2
            // 
            CMRegion2.ImageScalingSize = new Size(20, 20);
            CMRegion2.Items.AddRange(new ToolStripItem[] { CMRegion2_Copy, CMRegion2_Paste });
            CMRegion2.Name = "CMRegion1";
            CMRegion2.Size = new Size(113, 52);
            // 
            // CMRegion2_Copy
            // 
            CMRegion2_Copy.Name = "CMRegion2_Copy";
            CMRegion2_Copy.Size = new Size(112, 24);
            CMRegion2_Copy.Text = "Copy";
            CMRegion2_Copy.Click += CMRegion2_Copy_Click;
            // 
            // CMRegion2_Paste
            // 
            CMRegion2_Paste.Name = "CMRegion2_Paste";
            CMRegion2_Paste.Size = new Size(112, 24);
            CMRegion2_Paste.Text = "Paste";
            CMRegion2_Paste.Click += CMRegion2_Paste_Click;
            // 
            // panelR2Top
            // 
            panelR2Top.Controls.Add(LabeltbLyrics2);
            panelR2Top.Dock = DockStyle.Top;
            panelR2Top.Location = new Point(96, 0);
            panelR2Top.Margin = new Padding(4, 5, 4, 5);
            panelR2Top.Name = "panelR2Top";
            panelR2Top.Size = new Size(441, 25);
            panelR2Top.TabIndex = 1;
            // 
            // LabeltbLyrics2
            // 
            LabeltbLyrics2.Location = new Point(-1, 2);
            LabeltbLyrics2.Margin = new Padding(4, 0, 4, 0);
            LabeltbLyrics2.Name = "LabeltbLyrics2";
            LabeltbLyrics2.Size = new Size(503, 20);
            LabeltbLyrics2.TabIndex = 1;
            LabeltbLyrics2.Text = "Region 2";
            // 
            // panelR2Left
            // 
            panelR2Left.Controls.Add(panel1);
            panelR2Left.Controls.Add(SyncBtnDown);
            panelR2Left.Controls.Add(SyncBtnUp);
            panelR2Left.Dock = DockStyle.Left;
            panelR2Left.Location = new Point(0, 0);
            panelR2Left.Margin = new Padding(4, 5, 4, 5);
            panelR2Left.Name = "panelR2Left";
            panelR2Left.Size = new Size(96, 531);
            panelR2Left.TabIndex = 0;
            // 
            // panel1
            // 
            panel1.Controls.Add(panelR2LeftBottom);
            panel1.Controls.Add(panelR2LeftMiddle);
            panel1.Controls.Add(panelR2LeftTop);
            panel1.Location = new Point(28, 23);
            panel1.Margin = new Padding(4, 5, 4, 5);
            panel1.Name = "panel1";
            panel1.Size = new Size(64, 431);
            panel1.TabIndex = 2;
            // 
            // panelR2LeftBottom
            // 
            panelR2LeftBottom.Controls.Add(R2RightToLeft);
            panelR2LeftBottom.Controls.Add(R2Chinese);
            panelR2LeftBottom.Dock = DockStyle.Top;
            panelR2LeftBottom.Location = new Point(0, 391);
            panelR2LeftBottom.Margin = new Padding(4, 5, 4, 5);
            panelR2LeftBottom.Name = "panelR2LeftBottom";
            panelR2LeftBottom.Size = new Size(64, 37);
            panelR2LeftBottom.TabIndex = 3;
            // 
            // R2RightToLeft
            // 
            R2RightToLeft.Appearance = Appearance.Button;
            R2RightToLeft.FlatStyle = FlatStyle.Flat;
            R2RightToLeft.Image = Resources.LangRightLeft;
            R2RightToLeft.Location = new Point(31, 0);
            R2RightToLeft.Margin = new Padding(4, 5, 4, 5);
            R2RightToLeft.Name = "R2RightToLeft";
            R2RightToLeft.Size = new Size(31, 35);
            R2RightToLeft.TabIndex = 24;
            toolTip1.SetToolTip(R2RightToLeft, "Right-To-Left Text");
            R2RightToLeft.UseVisualStyleBackColor = true;
            R2RightToLeft.Visible = false;
            R2RightToLeft.Click += RightToLeft_Click;
            // 
            // R2Chinese
            // 
            R2Chinese.FlatStyle = FlatStyle.Flat;
            R2Chinese.Image = Resources.Chinese;
            R2Chinese.Location = new Point(0, 0);
            R2Chinese.Margin = new Padding(4, 5, 4, 5);
            R2Chinese.Name = "R2Chinese";
            R2Chinese.Size = new Size(31, 35);
            R2Chinese.TabIndex = 22;
            R2Chinese.Tag = "";
            toolTip1.SetToolTip(R2Chinese, "Siwtch Trad/Simp Chinese");
            R2Chinese.Click += R2Chinese_Click;
            // 
            // panelR2LeftMiddle
            // 
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
            panelR2LeftMiddle.Dock = DockStyle.Top;
            panelR2LeftMiddle.Location = new Point(0, 37);
            panelR2LeftMiddle.Margin = new Padding(4, 5, 4, 5);
            panelR2LeftMiddle.Name = "panelR2LeftMiddle";
            panelR2LeftMiddle.Size = new Size(64, 354);
            panelR2LeftMiddle.TabIndex = 1;
            // 
            // R2BtnBridge2
            // 
            R2BtnBridge2.FlatStyle = FlatStyle.Flat;
            R2BtnBridge2.Image = Resources.NumBridge2;
            R2BtnBridge2.Location = new Point(31, 248);
            R2BtnBridge2.Margin = new Padding(4, 5, 4, 5);
            R2BtnBridge2.Name = "R2BtnBridge2";
            R2BtnBridge2.Size = new Size(31, 35);
            R2BtnBridge2.TabIndex = 17;
            R2BtnBridge2.Tag = "103";
            toolTip1.SetToolTip(R2BtnBridge2, "Bridge 2 Indicator");
            R2BtnBridge2.Click += R2Btn_Click;
            // 
            // R2BtnPreChorus2
            // 
            R2BtnPreChorus2.FlatStyle = FlatStyle.Flat;
            R2BtnPreChorus2.Image = Resources.NumPreChorus2;
            R2BtnPreChorus2.Location = new Point(31, 177);
            R2BtnPreChorus2.Margin = new Padding(4, 5, 4, 5);
            R2BtnPreChorus2.Name = "R2BtnPreChorus2";
            R2BtnPreChorus2.Size = new Size(31, 35);
            R2BtnPreChorus2.TabIndex = 13;
            R2BtnPreChorus2.Tag = "112";
            toolTip1.SetToolTip(R2BtnPreChorus2, "Prechorus2 Indicator");
            R2BtnPreChorus2.Click += R2Btn_Click;
            // 
            // R2BtnPreChorus
            // 
            R2BtnPreChorus.FlatStyle = FlatStyle.Flat;
            R2BtnPreChorus.Image = Resources.NumPreChorus;
            R2BtnPreChorus.Location = new Point(0, 177);
            R2BtnPreChorus.Margin = new Padding(4, 5, 4, 5);
            R2BtnPreChorus.Name = "R2BtnPreChorus";
            R2BtnPreChorus.Size = new Size(31, 35);
            R2BtnPreChorus.TabIndex = 12;
            R2BtnPreChorus.Tag = "111";
            toolTip1.SetToolTip(R2BtnPreChorus, "Prechorus Indicator");
            R2BtnPreChorus.Click += R2Btn_Click;
            // 
            // R2VerseFormat
            // 
            R2VerseFormat.FlatStyle = FlatStyle.Flat;
            R2VerseFormat.Image = Resources.VerseFormat;
            R2VerseFormat.Location = new Point(31, 318);
            R2VerseFormat.Margin = new Padding(4, 5, 4, 5);
            R2VerseFormat.Name = "R2VerseFormat";
            R2VerseFormat.Size = new Size(31, 35);
            R2VerseFormat.TabIndex = 21;
            R2VerseFormat.Tag = "";
            toolTip1.SetToolTip(R2VerseFormat, "Verses Format");
            R2VerseFormat.Click += R2VerseFormat_Click;
            // 
            // R2BtnChorus
            // 
            R2BtnChorus.FlatStyle = FlatStyle.Flat;
            R2BtnChorus.Image = Resources.NumChorus;
            R2BtnChorus.Location = new Point(0, 212);
            R2BtnChorus.Margin = new Padding(4, 5, 4, 5);
            R2BtnChorus.Name = "R2BtnChorus";
            R2BtnChorus.Size = new Size(31, 35);
            R2BtnChorus.TabIndex = 14;
            R2BtnChorus.Tag = "0";
            toolTip1.SetToolTip(R2BtnChorus, "Chorus Indicator");
            R2BtnChorus.Click += R2Btn_Click;
            // 
            // R2BtnChorus2
            // 
            R2BtnChorus2.FlatStyle = FlatStyle.Flat;
            R2BtnChorus2.Image = Resources.NumChorus2;
            R2BtnChorus2.Location = new Point(31, 212);
            R2BtnChorus2.Margin = new Padding(4, 5, 4, 5);
            R2BtnChorus2.Name = "R2BtnChorus2";
            R2BtnChorus2.Size = new Size(31, 35);
            R2BtnChorus2.TabIndex = 15;
            R2BtnChorus2.Tag = "102";
            toolTip1.SetToolTip(R2BtnChorus2, "Chrous 2 Indicator");
            R2BtnChorus2.Click += R2Btn_Click;
            // 
            // R2BtnNewScreen
            // 
            R2BtnNewScreen.FlatStyle = FlatStyle.Flat;
            R2BtnNewScreen.Image = Resources.NumNewScreen;
            R2BtnNewScreen.Location = new Point(31, 283);
            R2BtnNewScreen.Margin = new Padding(4, 5, 4, 5);
            R2BtnNewScreen.Name = "R2BtnNewScreen";
            R2BtnNewScreen.Size = new Size(31, 35);
            R2BtnNewScreen.TabIndex = 19;
            R2BtnNewScreen.Tag = "151";
            toolTip1.SetToolTip(R2BtnNewScreen, "New Screen Indicator");
            R2BtnNewScreen.Click += R2Btn_Click;
            // 
            // R2BtnNotations
            // 
            R2BtnNotations.FlatStyle = FlatStyle.Flat;
            R2BtnNotations.Image = Resources.NotationSym;
            R2BtnNotations.Location = new Point(0, 318);
            R2BtnNotations.Margin = new Padding(4, 5, 4, 5);
            R2BtnNotations.Name = "R2BtnNotations";
            R2BtnNotations.Size = new Size(31, 35);
            R2BtnNotations.TabIndex = 20;
            R2BtnNotations.Tag = "152";
            toolTip1.SetToolTip(R2BtnNotations, "Notations Indicator (F8)");
            R2BtnNotations.Click += R2Btn_Click;
            // 
            // R2BtnEnding
            // 
            R2BtnEnding.FlatStyle = FlatStyle.Flat;
            R2BtnEnding.Image = Resources.NumEnding;
            R2BtnEnding.Location = new Point(0, 283);
            R2BtnEnding.Margin = new Padding(4, 5, 4, 5);
            R2BtnEnding.Name = "R2BtnEnding";
            R2BtnEnding.Size = new Size(31, 35);
            R2BtnEnding.TabIndex = 18;
            R2BtnEnding.Tag = "101";
            toolTip1.SetToolTip(R2BtnEnding, "Ending Indicator");
            R2BtnEnding.Click += R2Btn_Click;
            // 
            // R2BtnBridge
            // 
            R2BtnBridge.FlatStyle = FlatStyle.Flat;
            R2BtnBridge.Image = Resources.NumBridge;
            R2BtnBridge.Location = new Point(0, 248);
            R2BtnBridge.Margin = new Padding(4, 5, 4, 5);
            R2BtnBridge.Name = "R2BtnBridge";
            R2BtnBridge.Size = new Size(31, 35);
            R2BtnBridge.TabIndex = 16;
            R2BtnBridge.Tag = "100";
            toolTip1.SetToolTip(R2BtnBridge, "Bridge Indicator");
            R2BtnBridge.Click += R2Btn_Click;
            // 
            // R2Btn10
            // 
            R2Btn10.FlatStyle = FlatStyle.Flat;
            R2Btn10.Image = Resources.Num10;
            R2Btn10.Location = new Point(31, 142);
            R2Btn10.Margin = new Padding(4, 5, 4, 5);
            R2Btn10.Name = "R2Btn10";
            R2Btn10.Size = new Size(31, 35);
            R2Btn10.TabIndex = 11;
            R2Btn10.Tag = "10";
            R2Btn10.Click += R2Btn_Click;
            // 
            // R2Btn9
            // 
            R2Btn9.FlatStyle = FlatStyle.Flat;
            R2Btn9.Image = Resources.Num9;
            R2Btn9.Location = new Point(0, 142);
            R2Btn9.Margin = new Padding(4, 5, 4, 5);
            R2Btn9.Name = "R2Btn9";
            R2Btn9.Size = new Size(31, 35);
            R2Btn9.TabIndex = 10;
            R2Btn9.Tag = "9";
            R2Btn9.Click += R2Btn_Click;
            // 
            // R2Btn8
            // 
            R2Btn8.FlatStyle = FlatStyle.Flat;
            R2Btn8.Image = Resources.Num8;
            R2Btn8.Location = new Point(31, 106);
            R2Btn8.Margin = new Padding(4, 5, 4, 5);
            R2Btn8.Name = "R2Btn8";
            R2Btn8.Size = new Size(31, 35);
            R2Btn8.TabIndex = 9;
            R2Btn8.Tag = "8";
            R2Btn8.Click += R2Btn_Click;
            // 
            // R2Btn7
            // 
            R2Btn7.FlatStyle = FlatStyle.Flat;
            R2Btn7.Image = Resources.Num7;
            R2Btn7.Location = new Point(0, 106);
            R2Btn7.Margin = new Padding(4, 5, 4, 5);
            R2Btn7.Name = "R2Btn7";
            R2Btn7.Size = new Size(31, 35);
            R2Btn7.TabIndex = 8;
            R2Btn7.Tag = "7";
            R2Btn7.Click += R2Btn_Click;
            // 
            // R2Btn6
            // 
            R2Btn6.FlatStyle = FlatStyle.Flat;
            R2Btn6.Image = Resources.Num6;
            R2Btn6.Location = new Point(31, 71);
            R2Btn6.Margin = new Padding(4, 5, 4, 5);
            R2Btn6.Name = "R2Btn6";
            R2Btn6.Size = new Size(31, 35);
            R2Btn6.TabIndex = 7;
            R2Btn6.Tag = "6";
            R2Btn6.Click += R2Btn_Click;
            // 
            // R2Btn5
            // 
            R2Btn5.FlatStyle = FlatStyle.Flat;
            R2Btn5.Image = Resources.Num5;
            R2Btn5.Location = new Point(0, 71);
            R2Btn5.Margin = new Padding(4, 5, 4, 5);
            R2Btn5.Name = "R2Btn5";
            R2Btn5.Size = new Size(31, 35);
            R2Btn5.TabIndex = 6;
            R2Btn5.Tag = "5";
            R2Btn5.Click += R2Btn_Click;
            // 
            // R2Btn4
            // 
            R2Btn4.FlatStyle = FlatStyle.Flat;
            R2Btn4.Image = Resources.Num4;
            R2Btn4.Location = new Point(31, 35);
            R2Btn4.Margin = new Padding(4, 5, 4, 5);
            R2Btn4.Name = "R2Btn4";
            R2Btn4.Size = new Size(31, 35);
            R2Btn4.TabIndex = 5;
            R2Btn4.Tag = "4";
            R2Btn4.Click += R2Btn_Click;
            // 
            // R2Btn3
            // 
            R2Btn3.FlatStyle = FlatStyle.Flat;
            R2Btn3.Image = Resources.Num3;
            R2Btn3.Location = new Point(0, 35);
            R2Btn3.Margin = new Padding(4, 5, 4, 5);
            R2Btn3.Name = "R2Btn3";
            R2Btn3.Size = new Size(31, 35);
            R2Btn3.TabIndex = 4;
            R2Btn3.Tag = "3";
            R2Btn3.Click += R2Btn_Click;
            // 
            // R2Btn2
            // 
            R2Btn2.FlatStyle = FlatStyle.Flat;
            R2Btn2.Image = Resources.Num2;
            R2Btn2.Location = new Point(31, 0);
            R2Btn2.Margin = new Padding(4, 5, 4, 5);
            R2Btn2.Name = "R2Btn2";
            R2Btn2.Size = new Size(31, 35);
            R2Btn2.TabIndex = 3;
            R2Btn2.Tag = "2";
            R2Btn2.Click += R2Btn_Click;
            // 
            // R2Btn1
            // 
            R2Btn1.FlatStyle = FlatStyle.Flat;
            R2Btn1.Image = Resources.Num1;
            R2Btn1.Location = new Point(0, 0);
            R2Btn1.Margin = new Padding(4, 5, 4, 5);
            R2Btn1.Name = "R2Btn1";
            R2Btn1.Size = new Size(31, 35);
            R2Btn1.TabIndex = 2;
            R2Btn1.Tag = "1";
            toolTip1.SetToolTip(R2Btn1, "Verse Indicator");
            R2Btn1.Click += R2Btn_Click;
            // 
            // panelR2LeftTop
            // 
            panelR2LeftTop.Controls.Add(R2Undo);
            panelR2LeftTop.Controls.Add(R2Redo);
            panelR2LeftTop.Dock = DockStyle.Top;
            panelR2LeftTop.Location = new Point(0, 0);
            panelR2LeftTop.Margin = new Padding(4, 5, 4, 5);
            panelR2LeftTop.Name = "panelR2LeftTop";
            panelR2LeftTop.Size = new Size(64, 37);
            panelR2LeftTop.TabIndex = 2;
            // 
            // R2Undo
            // 
            R2Undo.FlatStyle = FlatStyle.Flat;
            R2Undo.Image = Resources.undo;
            R2Undo.Location = new Point(0, 2);
            R2Undo.Margin = new Padding(4, 5, 4, 5);
            R2Undo.Name = "R2Undo";
            R2Undo.Size = new Size(31, 35);
            R2Undo.TabIndex = 0;
            R2Undo.Tag = "0";
            toolTip1.SetToolTip(R2Undo, "Undo");
            R2Undo.Click += R2UndoRedo_Click;
            // 
            // R2Redo
            // 
            R2Redo.FlatStyle = FlatStyle.Flat;
            R2Redo.Image = Resources.redo;
            R2Redo.Location = new Point(31, 2);
            R2Redo.Margin = new Padding(4, 5, 4, 5);
            R2Redo.Name = "R2Redo";
            R2Redo.Size = new Size(31, 35);
            R2Redo.TabIndex = 1;
            R2Redo.Tag = "1";
            toolTip1.SetToolTip(R2Redo, "Redo");
            R2Redo.Click += R2UndoRedo_Click;
            // 
            // SyncBtnDown
            // 
            SyncBtnDown.FlatStyle = FlatStyle.Flat;
            SyncBtnDown.Image = Resources.green_down;
            SyncBtnDown.Location = new Point(0, 62);
            SyncBtnDown.Margin = new Padding(4, 5, 4, 5);
            SyncBtnDown.Name = "SyncBtnDown";
            SyncBtnDown.Size = new Size(27, 34);
            SyncBtnDown.TabIndex = 1;
            SyncBtnDown.Tag = "1";
            toolTip1.SetToolTip(SyncBtnDown, "Highlight Next Slide");
            SyncBtnDown.Click += SyncBtnUpDown_Click;
            // 
            // SyncBtnUp
            // 
            SyncBtnUp.FlatStyle = FlatStyle.Flat;
            SyncBtnUp.Image = Resources.green_up;
            SyncBtnUp.Location = new Point(0, 28);
            SyncBtnUp.Margin = new Padding(4, 5, 4, 5);
            SyncBtnUp.Name = "SyncBtnUp";
            SyncBtnUp.Size = new Size(27, 34);
            SyncBtnUp.TabIndex = 0;
            SyncBtnUp.Tag = "0";
            toolTip1.SetToolTip(SyncBtnUp, "Highlight Previous Slide");
            SyncBtnUp.Click += SyncBtnUpDown_Click;
            // 
            // tabRight_Rotate
            // 
            tabRight_Rotate.BackColor = SystemColors.Control;
            tabRight_Rotate.Controls.Add(panelRotate);
            tabRight_Rotate.Location = new Point(4, 26);
            tabRight_Rotate.Margin = new Padding(4, 5, 4, 5);
            tabRight_Rotate.Name = "tabRight_Rotate";
            tabRight_Rotate.Padding = new Padding(4, 5, 4, 5);
            tabRight_Rotate.Size = new Size(543, 510);
            tabRight_Rotate.TabIndex = 1;
            tabRight_Rotate.Text = "Rotate Style";
            // 
            // panelRotate
            // 
            panelRotate.Controls.Add(splitContainerRotate);
            panelRotate.Controls.Add(panelRotateLeft);
            panelRotate.Dock = DockStyle.Fill;
            panelRotate.Location = new Point(4, 5);
            panelRotate.Margin = new Padding(4, 5, 4, 5);
            panelRotate.Name = "panelRotate";
            panelRotate.Size = new Size(535, 500);
            panelRotate.TabIndex = 0;
            // 
            // splitContainerRotate
            // 
            splitContainerRotate.Dock = DockStyle.Fill;
            splitContainerRotate.Location = new Point(164, 0);
            splitContainerRotate.Margin = new Padding(4, 5, 4, 5);
            splitContainerRotate.Name = "splitContainerRotate";
            splitContainerRotate.Orientation = Orientation.Horizontal;
            // 
            // splitContainerRotate.Panel1
            // 
            splitContainerRotate.Panel1.Controls.Add(groupBoxRotateVerses);
            // 
            // splitContainerRotate.Panel2
            // 
            splitContainerRotate.Panel2.Controls.Add(panelRotate_Sample);
            splitContainerRotate.Size = new Size(371, 500);
            splitContainerRotate.SplitterDistance = 169;
            splitContainerRotate.SplitterWidth = 6;
            splitContainerRotate.TabIndex = 33;
            splitContainerRotate.Text = "splitContainer2";
            splitContainerRotate.SplitterMoved += splitContainerRotate_SplitterMoved;
            // 
            // groupBoxRotateVerses
            // 
            groupBoxRotateVerses.Controls.Add(panelRotate_Verses);
            groupBoxRotateVerses.Controls.Add(panelRotate_OrderList);
            groupBoxRotateVerses.Controls.Add(panel13);
            groupBoxRotateVerses.Controls.Add(panel14);
            groupBoxRotateVerses.Dock = DockStyle.Left;
            groupBoxRotateVerses.Enabled = false;
            groupBoxRotateVerses.Location = new Point(0, 0);
            groupBoxRotateVerses.Margin = new Padding(4, 5, 4, 5);
            groupBoxRotateVerses.Name = "groupBoxRotateVerses";
            groupBoxRotateVerses.Padding = new Padding(0);
            groupBoxRotateVerses.Size = new Size(320, 169);
            groupBoxRotateVerses.TabIndex = 33;
            groupBoxRotateVerses.TabStop = false;
            // 
            // panelRotate_Verses
            // 
            panelRotate_Verses.BorderStyle = BorderStyle.Fixed3D;
            panelRotate_Verses.Controls.Add(Rotate_VersesList);
            panelRotate_Verses.Controls.Add(panel10);
            panelRotate_Verses.Location = new Point(8, 15);
            panelRotate_Verses.Margin = new Padding(4, 5, 4, 5);
            panelRotate_Verses.Name = "panelRotate_Verses";
            panelRotate_Verses.Size = new Size(119, 147);
            panelRotate_Verses.TabIndex = 1;
            // 
            // Rotate_VersesList
            // 
            Rotate_VersesList.Columns.AddRange(new ColumnHeader[] { columnHeader6, columnHeader7, columnHeader8 });
            Rotate_VersesList.Dock = DockStyle.Fill;
            Rotate_VersesList.FullRowSelect = true;
            Rotate_VersesList.HeaderStyle = ColumnHeaderStyle.None;
            Rotate_VersesList.Location = new Point(0, 20);
            Rotate_VersesList.Margin = new Padding(1, 2, 1, 2);
            Rotate_VersesList.Name = "Rotate_VersesList";
            Rotate_VersesList.ShowItemToolTips = true;
            Rotate_VersesList.Size = new Size(115, 123);
            Rotate_VersesList.TabIndex = 0;
            Rotate_VersesList.UseCompatibleStateImageBehavior = false;
            Rotate_VersesList.View = View.Details;
            Rotate_VersesList.DoubleClick += Rotate_VersesList_DoubleClick;
            // 
            // columnHeader6
            // 
            columnHeader6.Width = 65;
            // 
            // columnHeader7
            // 
            columnHeader7.Width = 0;
            // 
            // columnHeader8
            // 
            columnHeader8.Width = 0;
            // 
            // panel10
            // 
            panel10.BorderStyle = BorderStyle.FixedSingle;
            panel10.Controls.Add(label23);
            panel10.Dock = DockStyle.Top;
            panel10.Location = new Point(0, 0);
            panel10.Margin = new Padding(4, 5, 4, 5);
            panel10.Name = "panel10";
            panel10.Size = new Size(115, 20);
            panel10.TabIndex = 0;
            // 
            // label23
            // 
            label23.AutoSize = true;
            label23.Location = new Point(16, -2);
            label23.Margin = new Padding(4, 0, 4, 0);
            label23.Name = "label23";
            label23.Size = new Size(52, 17);
            label23.TabIndex = 0;
            label23.Text = "Verses";
            // 
            // panelRotate_OrderList
            // 
            panelRotate_OrderList.BorderStyle = BorderStyle.Fixed3D;
            panelRotate_OrderList.Controls.Add(Rotate_OrderList);
            panelRotate_OrderList.Controls.Add(panel12);
            panelRotate_OrderList.Location = new Point(163, 15);
            panelRotate_OrderList.Margin = new Padding(4, 5, 4, 5);
            panelRotate_OrderList.Name = "panelRotate_OrderList";
            panelRotate_OrderList.Size = new Size(119, 147);
            panelRotate_OrderList.TabIndex = 2;
            // 
            // Rotate_OrderList
            // 
            Rotate_OrderList.Columns.AddRange(new ColumnHeader[] { columnHeader9, columnHeader10 });
            Rotate_OrderList.Dock = DockStyle.Fill;
            Rotate_OrderList.FullRowSelect = true;
            Rotate_OrderList.HeaderStyle = ColumnHeaderStyle.None;
            Rotate_OrderList.Location = new Point(0, 20);
            Rotate_OrderList.Margin = new Padding(4, 5, 4, 5);
            Rotate_OrderList.Name = "Rotate_OrderList";
            Rotate_OrderList.Size = new Size(115, 123);
            Rotate_OrderList.TabIndex = 0;
            Rotate_OrderList.UseCompatibleStateImageBehavior = false;
            Rotate_OrderList.View = View.Details;
            Rotate_OrderList.KeyUp += Rotate_OrderList_KeyUp;
            // 
            // columnHeader9
            // 
            columnHeader9.Width = 65;
            // 
            // columnHeader10
            // 
            columnHeader10.Width = 0;
            // 
            // panel12
            // 
            panel12.BorderStyle = BorderStyle.FixedSingle;
            panel12.Controls.Add(label24);
            panel12.Dock = DockStyle.Top;
            panel12.Location = new Point(0, 0);
            panel12.Margin = new Padding(4, 5, 4, 5);
            panel12.Name = "panel12";
            panel12.Size = new Size(115, 20);
            panel12.TabIndex = 0;
            // 
            // label24
            // 
            label24.AutoSize = true;
            label24.Location = new Point(13, -2);
            label24.Margin = new Padding(4, 0, 4, 0);
            label24.Name = "label24";
            label24.Size = new Size(72, 17);
            label24.TabIndex = 0;
            label24.Text = "Sequence";
            // 
            // panel13
            // 
            panel13.Controls.Add(toolStripRotate_SeqSet);
            panel13.Location = new Point(128, 42);
            panel13.Margin = new Padding(4, 5, 4, 5);
            panel13.Name = "panel13";
            panel13.Size = new Size(33, 80);
            panel13.TabIndex = 13;
            // 
            // toolStripRotate_SeqSet
            // 
            toolStripRotate_SeqSet.AutoSize = false;
            toolStripRotate_SeqSet.CanOverflow = false;
            toolStripRotate_SeqSet.Dock = DockStyle.None;
            toolStripRotate_SeqSet.GripStyle = ToolStripGripStyle.Hidden;
            toolStripRotate_SeqSet.ImageScalingSize = new Size(20, 20);
            toolStripRotate_SeqSet.Items.AddRange(new ToolStripItem[] { Rotate_Verses_Add, Rotate_Verses_SmartAdd });
            toolStripRotate_SeqSet.LayoutStyle = ToolStripLayoutStyle.VerticalStackWithOverflow;
            toolStripRotate_SeqSet.Location = new Point(0, 2);
            toolStripRotate_SeqSet.Name = "toolStripRotate_SeqSet";
            toolStripRotate_SeqSet.RenderMode = ToolStripRenderMode.System;
            toolStripRotate_SeqSet.Size = new Size(33, 95);
            toolStripRotate_SeqSet.TabIndex = 5;
            // 
            // Rotate_Verses_Add
            // 
            Rotate_Verses_Add.AutoSize = false;
            Rotate_Verses_Add.DisplayStyle = ToolStripItemDisplayStyle.Image;
            Rotate_Verses_Add.Image = Resources.arrowR;
            Rotate_Verses_Add.ImageTransparentColor = Color.Magenta;
            Rotate_Verses_Add.Name = "Rotate_Verses_Add";
            Rotate_Verses_Add.Size = new Size(22, 22);
            Rotate_Verses_Add.Tag = "";
            Rotate_Verses_Add.ToolTipText = "Add";
            Rotate_Verses_Add.Click += Rotate_Verses_Add_Click;
            // 
            // Rotate_Verses_SmartAdd
            // 
            Rotate_Verses_SmartAdd.AutoSize = false;
            Rotate_Verses_SmartAdd.DisplayStyle = ToolStripItemDisplayStyle.Image;
            Rotate_Verses_SmartAdd.Image = Resources.multi_arrowr;
            Rotate_Verses_SmartAdd.ImageTransparentColor = Color.Magenta;
            Rotate_Verses_SmartAdd.Name = "Rotate_Verses_SmartAdd";
            Rotate_Verses_SmartAdd.Size = new Size(22, 22);
            Rotate_Verses_SmartAdd.Tag = "";
            Rotate_Verses_SmartAdd.ToolTipText = "Smart Add";
            Rotate_Verses_SmartAdd.Click += Rotate_Verses_Add_Click;
            // 
            // panel14
            // 
            panel14.Controls.Add(toolStripRotate_SeqUpDown);
            panel14.Location = new Point(281, 43);
            panel14.Margin = new Padding(4, 5, 4, 5);
            panel14.Name = "panel14";
            panel14.Size = new Size(33, 122);
            panel14.TabIndex = 12;
            // 
            // toolStripRotate_SeqUpDown
            // 
            toolStripRotate_SeqUpDown.AutoSize = false;
            toolStripRotate_SeqUpDown.CanOverflow = false;
            toolStripRotate_SeqUpDown.Dock = DockStyle.None;
            toolStripRotate_SeqUpDown.GripStyle = ToolStripGripStyle.Hidden;
            toolStripRotate_SeqUpDown.ImageScalingSize = new Size(20, 20);
            toolStripRotate_SeqUpDown.Items.AddRange(new ToolStripItem[] { Rotate_OrderList_Up, Rotate_OrderList_Down, toolStripSeparator3, Rotate_OrderList_Delete });
            toolStripRotate_SeqUpDown.LayoutStyle = ToolStripLayoutStyle.VerticalStackWithOverflow;
            toolStripRotate_SeqUpDown.Location = new Point(0, -2);
            toolStripRotate_SeqUpDown.Name = "toolStripRotate_SeqUpDown";
            toolStripRotate_SeqUpDown.RenderMode = ToolStripRenderMode.System;
            toolStripRotate_SeqUpDown.Size = new Size(33, 137);
            toolStripRotate_SeqUpDown.TabIndex = 5;
            // 
            // Rotate_OrderList_Up
            // 
            Rotate_OrderList_Up.AutoSize = false;
            Rotate_OrderList_Up.DisplayStyle = ToolStripItemDisplayStyle.Image;
            Rotate_OrderList_Up.Image = Resources.handup;
            Rotate_OrderList_Up.ImageTransparentColor = Color.Magenta;
            Rotate_OrderList_Up.Name = "Rotate_OrderList_Up";
            Rotate_OrderList_Up.Size = new Size(22, 22);
            Rotate_OrderList_Up.Tag = "up";
            Rotate_OrderList_Up.ToolTipText = "Move Item Up";
            Rotate_OrderList_Up.Click += Rotate_OrderList_Btn_Click;
            // 
            // Rotate_OrderList_Down
            // 
            Rotate_OrderList_Down.AutoSize = false;
            Rotate_OrderList_Down.DisplayStyle = ToolStripItemDisplayStyle.Image;
            Rotate_OrderList_Down.Image = Resources.handdown;
            Rotate_OrderList_Down.ImageTransparentColor = Color.Magenta;
            Rotate_OrderList_Down.Name = "Rotate_OrderList_Down";
            Rotate_OrderList_Down.Size = new Size(22, 22);
            Rotate_OrderList_Down.Tag = "down";
            Rotate_OrderList_Down.ToolTipText = "Move Item Down";
            Rotate_OrderList_Down.Click += Rotate_OrderList_Btn_Click;
            // 
            // toolStripSeparator3
            // 
            toolStripSeparator3.Name = "toolStripSeparator3";
            toolStripSeparator3.Size = new Size(31, 6);
            // 
            // Rotate_OrderList_Delete
            // 
            Rotate_OrderList_Delete.AutoSize = false;
            Rotate_OrderList_Delete.DisplayStyle = ToolStripItemDisplayStyle.Image;
            Rotate_OrderList_Delete.Image = Resources.Delete;
            Rotate_OrderList_Delete.ImageTransparentColor = Color.Magenta;
            Rotate_OrderList_Delete.Name = "Rotate_OrderList_Delete";
            Rotate_OrderList_Delete.Size = new Size(22, 22);
            Rotate_OrderList_Delete.Tag = "delete";
            Rotate_OrderList_Delete.ToolTipText = "Delete";
            Rotate_OrderList_Delete.Click += Rotate_OrderList_Btn_Click;
            // 
            // panelRotate_Sample
            // 
            panelRotate_Sample.BorderStyle = BorderStyle.Fixed3D;
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
            panelRotate_Sample.Dock = DockStyle.Fill;
            panelRotate_Sample.Location = new Point(0, 0);
            panelRotate_Sample.Margin = new Padding(4, 5, 4, 5);
            panelRotate_Sample.Name = "panelRotate_Sample";
            panelRotate_Sample.Size = new Size(371, 325);
            panelRotate_Sample.TabIndex = 31;
            panelRotate_Sample.Resize += panelRotate_Sample_Resize;
            // 
            // labelDur
            // 
            labelDur.AutoSize = true;
            labelDur.Location = new Point(8, 43);
            labelDur.Margin = new Padding(4, 0, 4, 0);
            labelDur.Name = "labelDur";
            labelDur.Size = new Size(56, 17);
            labelDur.TabIndex = 68;
            labelDur.Text = "Length:";
            // 
            // btnAddPosition
            // 
            btnAddPosition.Image = Resources.arrowL;
            btnAddPosition.Location = new Point(9, 182);
            btnAddPosition.Margin = new Padding(4, 5, 4, 5);
            btnAddPosition.Name = "btnAddPosition";
            btnAddPosition.Size = new Size(55, 34);
            btnAddPosition.TabIndex = 1;
            toolTip1.SetToolTip(btnAddPosition, "Copy current position to next blank timing");
            btnAddPosition.Click += btnAddPosition_Click;
            // 
            // btnDuration
            // 
            btnDuration.Image = Resources.arrowBL;
            btnDuration.Location = new Point(9, 89);
            btnDuration.Margin = new Padding(4, 5, 4, 5);
            btnDuration.Name = "btnDuration";
            btnDuration.Size = new Size(55, 35);
            btnDuration.TabIndex = 0;
            btnDuration.TextAlign = ContentAlignment.MiddleLeft;
            toolTip1.SetToolTip(btnDuration, "Click to Store length");
            btnDuration.Click += btnDuration_Click;
            // 
            // LabelMediaType
            // 
            LabelMediaType.AutoSize = true;
            LabelMediaType.ForeColor = Color.Red;
            LabelMediaType.Location = new Point(124, 292);
            LabelMediaType.Margin = new Padding(4, 0, 4, 0);
            LabelMediaType.Name = "LabelMediaType";
            LabelMediaType.Size = new Size(42, 17);
            LabelMediaType.TabIndex = 30;
            LabelMediaType.Text = "None";
            // 
            // labelMed
            // 
            labelMed.AutoSize = true;
            labelMed.Location = new Point(75, 292);
            labelMed.Margin = new Padding(4, 0, 4, 0);
            labelMed.Name = "labelMed";
            labelMed.Size = new Size(50, 17);
            labelMed.TabIndex = 29;
            labelMed.Text = "Media:";
            // 
            // panelRotate_Media
            // 
            panelRotate_Media.BorderStyle = BorderStyle.Fixed3D;
            panelRotate_Media.Controls.Add(panelNoPlayer);
            panelRotate_Media.Location = new Point(73, 40);
            panelRotate_Media.Margin = new Padding(4, 5, 4, 5);
            panelRotate_Media.Name = "panelRotate_Media";
            panelRotate_Media.Size = new Size(212, 182);
            panelRotate_Media.TabIndex = 66;
            // 
            // panelNoPlayer
            // 
            panelNoPlayer.BackColor = Color.MidnightBlue;
            panelNoPlayer.Controls.Add(label14);
            panelNoPlayer.Controls.Add(labelNoPlayer1);
            panelNoPlayer.Controls.Add(labelNoPlayer2);
            panelNoPlayer.ForeColor = Color.White;
            panelNoPlayer.Location = new Point(0, 0);
            panelNoPlayer.Margin = new Padding(4, 5, 4, 5);
            panelNoPlayer.Name = "panelNoPlayer";
            panelNoPlayer.Size = new Size(208, 178);
            panelNoPlayer.TabIndex = 1;
            panelNoPlayer.Visible = false;
            // 
            // label14
            // 
            label14.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label14.Location = new Point(-1, 92);
            label14.Margin = new Padding(4, 0, 4, 0);
            label14.Name = "label14";
            label14.Size = new Size(205, 49);
            label14.TabIndex = 25;
            label14.Text = "to view / listen to Media Backgrounds.";
            label14.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // labelNoPlayer1
            // 
            labelNoPlayer1.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            labelNoPlayer1.Location = new Point(27, 25);
            labelNoPlayer1.Margin = new Padding(4, 0, 4, 0);
            labelNoPlayer1.Name = "labelNoPlayer1";
            labelNoPlayer1.Size = new Size(148, 31);
            labelNoPlayer1.TabIndex = 0;
            labelNoPlayer1.Text = "Please install";
            labelNoPlayer1.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // labelNoPlayer2
            // 
            labelNoPlayer2.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            labelNoPlayer2.Location = new Point(1, 51);
            labelNoPlayer2.Margin = new Padding(4, 0, 4, 0);
            labelNoPlayer2.Name = "labelNoPlayer2";
            labelNoPlayer2.Size = new Size(205, 48);
            labelNoPlayer2.TabIndex = 0;
            labelNoPlayer2.Text = "Windows Media Player 10 or DirectX 9";
            labelNoPlayer2.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // labelPos
            // 
            labelPos.AutoSize = true;
            labelPos.Location = new Point(8, 134);
            labelPos.Margin = new Padding(4, 0, 4, 0);
            labelPos.Name = "labelPos";
            labelPos.Size = new Size(62, 17);
            labelPos.TabIndex = 26;
            labelPos.Text = "Position:";
            // 
            // LabelPosition
            // 
            LabelPosition.AutoSize = true;
            LabelPosition.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            LabelPosition.ForeColor = Color.Red;
            LabelPosition.Location = new Point(8, 157);
            LabelPosition.Margin = new Padding(4, 0, 4, 0);
            LabelPosition.Name = "LabelPosition";
            LabelPosition.Size = new Size(40, 17);
            LabelPosition.TabIndex = 28;
            LabelPosition.Text = "0:00";
            // 
            // panelLoc
            // 
            panelLoc.Controls.Add(toolStrip3);
            panelLoc.Location = new Point(235, 2);
            panelLoc.Margin = new Padding(4, 5, 4, 5);
            panelLoc.Name = "panelLoc";
            panelLoc.Size = new Size(31, 35);
            panelLoc.TabIndex = 63;
            // 
            // toolStrip3
            // 
            toolStrip3.AutoSize = false;
            toolStrip3.CanOverflow = false;
            toolStrip3.Dock = DockStyle.None;
            toolStrip3.GripStyle = ToolStripGripStyle.Hidden;
            toolStrip3.ImageScalingSize = new Size(20, 20);
            toolStrip3.Items.AddRange(new ToolStripItem[] { Rotate_LocationBtn });
            toolStrip3.LayoutStyle = ToolStripLayoutStyle.Flow;
            toolStrip3.Location = new Point(1, 0);
            toolStrip3.Name = "toolStrip3";
            toolStrip3.RenderMode = ToolStripRenderMode.System;
            toolStrip3.Size = new Size(33, 46);
            toolStrip3.TabIndex = 0;
            // 
            // Rotate_LocationBtn
            // 
            Rotate_LocationBtn.AutoSize = false;
            Rotate_LocationBtn.DisplayStyle = ToolStripItemDisplayStyle.Image;
            Rotate_LocationBtn.Image = Resources.folder;
            Rotate_LocationBtn.ImageTransparentColor = Color.Magenta;
            Rotate_LocationBtn.Name = "Rotate_LocationBtn";
            Rotate_LocationBtn.Size = new Size(22, 22);
            Rotate_LocationBtn.Tag = "";
            Rotate_LocationBtn.ToolTipText = "Media File Location";
            Rotate_LocationBtn.Click += Rotate_LocationBtn_Click;
            // 
            // LabelDuration
            // 
            LabelDuration.AutoSize = true;
            LabelDuration.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            LabelDuration.ForeColor = SystemColors.MenuHighlight;
            LabelDuration.Location = new Point(8, 66);
            LabelDuration.Margin = new Padding(4, 0, 4, 0);
            LabelDuration.Name = "LabelDuration";
            LabelDuration.Size = new Size(40, 17);
            LabelDuration.TabIndex = 27;
            LabelDuration.Text = "0:00";
            // 
            // Rotate_tbSourceLocation
            // 
            Rotate_tbSourceLocation.Location = new Point(4, 5);
            Rotate_tbSourceLocation.Margin = new Padding(4, 5, 4, 5);
            Rotate_tbSourceLocation.Name = "Rotate_tbSourceLocation";
            Rotate_tbSourceLocation.Size = new Size(220, 23);
            Rotate_tbSourceLocation.TabIndex = 0;
            toolTip1.SetToolTip(Rotate_tbSourceLocation, "Optional Media File for Reference (Filename will not be saved)");
            // 
            // TrackBarVolume
            // 
            TrackBarVolume.AutoSize = false;
            TrackBarVolume.Location = new Point(288, 65);
            TrackBarVolume.Margin = new Padding(4, 5, 4, 5);
            TrackBarVolume.Maximum = 100;
            TrackBarVolume.Name = "TrackBarVolume";
            TrackBarVolume.Orientation = Orientation.Vertical;
            TrackBarVolume.Size = new Size(49, 174);
            TrackBarVolume.TabIndex = 8;
            TrackBarVolume.TickFrequency = 10;
            TrackBarVolume.TickStyle = TickStyle.TopLeft;
            TrackBarVolume.ValueChanged += TrackBarVolume_ValueChanged;
            // 
            // panelPlayBtns
            // 
            panelPlayBtns.Controls.Add(TrackBarDuration);
            panelPlayBtns.Controls.Add(StopBtn);
            panelPlayBtns.Controls.Add(PlayPauseBtn);
            panelPlayBtns.Controls.Add(FastReverseBtn);
            panelPlayBtns.Controls.Add(FastForwardBtn);
            panelPlayBtns.Location = new Point(65, 226);
            panelPlayBtns.Margin = new Padding(4, 5, 4, 5);
            panelPlayBtns.Name = "panelPlayBtns";
            panelPlayBtns.Size = new Size(223, 63);
            panelPlayBtns.TabIndex = 24;
            // 
            // TrackBarDuration
            // 
            TrackBarDuration.AutoSize = false;
            TrackBarDuration.Location = new Point(-5, 0);
            TrackBarDuration.Margin = new Padding(4, 5, 4, 5);
            TrackBarDuration.Maximum = 1000;
            TrackBarDuration.Name = "TrackBarDuration";
            TrackBarDuration.Size = new Size(229, 28);
            TrackBarDuration.TabIndex = 11;
            TrackBarDuration.TickFrequency = 0;
            TrackBarDuration.TickStyle = TickStyle.None;
            TrackBarDuration.Scroll += TrackBarDuration_Scroll;
            // 
            // StopBtn
            // 
            StopBtn.FlatStyle = FlatStyle.Flat;
            StopBtn.Location = new Point(165, 28);
            StopBtn.Margin = new Padding(4, 5, 4, 5);
            StopBtn.Name = "StopBtn";
            StopBtn.Size = new Size(53, 35);
            StopBtn.TabIndex = 16;
            StopBtn.Text = "Stop";
            StopBtn.Click += StopBtn_Click;
            // 
            // PlayPauseBtn
            // 
            PlayPauseBtn.FlatStyle = FlatStyle.Flat;
            PlayPauseBtn.Location = new Point(97, 28);
            PlayPauseBtn.Margin = new Padding(4, 5, 4, 5);
            PlayPauseBtn.Name = "PlayPauseBtn";
            PlayPauseBtn.Size = new Size(65, 35);
            PlayPauseBtn.TabIndex = 17;
            PlayPauseBtn.Text = "Play";
            PlayPauseBtn.Click += PlayPauseBtn_Click;
            // 
            // FastReverseBtn
            // 
            FastReverseBtn.FlatStyle = FlatStyle.Flat;
            FastReverseBtn.Location = new Point(11, 28);
            FastReverseBtn.Margin = new Padding(4, 5, 4, 5);
            FastReverseBtn.Name = "FastReverseBtn";
            FastReverseBtn.Size = new Size(40, 35);
            FastReverseBtn.TabIndex = 0;
            FastReverseBtn.Text = "<<";
            FastReverseBtn.MouseDown += FastReverseBtn_MouseDown;
            FastReverseBtn.MouseUp += FastReverseBtn_MouseUp;
            // 
            // FastForwardBtn
            // 
            FastForwardBtn.FlatStyle = FlatStyle.Flat;
            FastForwardBtn.Location = new Point(53, 28);
            FastForwardBtn.Margin = new Padding(4, 5, 4, 5);
            FastForwardBtn.Name = "FastForwardBtn";
            FastForwardBtn.Size = new Size(40, 35);
            FastForwardBtn.TabIndex = 1;
            FastForwardBtn.Text = ">>";
            FastForwardBtn.MouseDown += FastForwardBtn_MouseDown;
            FastForwardBtn.MouseUp += FastForwardBtn_MouseUp;
            // 
            // labelVol
            // 
            labelVol.AutoSize = true;
            labelVol.Location = new Point(288, 40);
            labelVol.Margin = new Padding(4, 0, 4, 0);
            labelVol.Name = "labelVol";
            labelVol.Size = new Size(55, 17);
            labelVol.TabIndex = 9;
            labelVol.Text = "Volume";
            // 
            // panelRotateLeft
            // 
            panelRotateLeft.Controls.Add(flowLayoutRotate);
            panelRotateLeft.Controls.Add(panelRotateLeftTop2);
            panelRotateLeft.Controls.Add(panelRotateLeftTop1);
            panelRotateLeft.Dock = DockStyle.Left;
            panelRotateLeft.Location = new Point(0, 0);
            panelRotateLeft.Margin = new Padding(4, 5, 4, 5);
            panelRotateLeft.Name = "panelRotateLeft";
            panelRotateLeft.Size = new Size(164, 500);
            panelRotateLeft.TabIndex = 32;
            // 
            // flowLayoutRotate
            // 
            flowLayoutRotate.AutoScroll = true;
            flowLayoutRotate.Dock = DockStyle.Fill;
            flowLayoutRotate.Location = new Point(0, 213);
            flowLayoutRotate.Margin = new Padding(4, 5, 4, 5);
            flowLayoutRotate.Name = "flowLayoutRotate";
            flowLayoutRotate.Size = new Size(164, 287);
            flowLayoutRotate.TabIndex = 7;
            // 
            // panelRotateLeftTop2
            // 
            panelRotateLeftTop2.Controls.Add(Rotate_TimeTotal);
            panelRotateLeftTop2.Controls.Add(btnClearMediaPositions);
            panelRotateLeftTop2.Controls.Add(label21);
            panelRotateLeftTop2.Controls.Add(label19);
            panelRotateLeftTop2.Dock = DockStyle.Top;
            panelRotateLeftTop2.Location = new Point(0, 102);
            panelRotateLeftTop2.Margin = new Padding(4, 5, 4, 5);
            panelRotateLeftTop2.Name = "panelRotateLeftTop2";
            panelRotateLeftTop2.Size = new Size(164, 111);
            panelRotateLeftTop2.TabIndex = 0;
            // 
            // Rotate_TimeTotal
            // 
            Rotate_TimeTotal.CustomFormat = "mm:ss";
            Rotate_TimeTotal.Format = DateTimePickerFormat.Custom;
            Rotate_TimeTotal.Location = new Point(75, 46);
            Rotate_TimeTotal.Margin = new Padding(4, 5, 4, 5);
            Rotate_TimeTotal.Name = "Rotate_TimeTotal";
            Rotate_TimeTotal.ShowUpDown = true;
            Rotate_TimeTotal.Size = new Size(68, 23);
            Rotate_TimeTotal.TabIndex = 1;
            // 
            // btnClearMediaPositions
            // 
            btnClearMediaPositions.FlatStyle = FlatStyle.Flat;
            btnClearMediaPositions.Location = new Point(5, 3);
            btnClearMediaPositions.Margin = new Padding(4, 5, 4, 5);
            btnClearMediaPositions.Name = "btnClearMediaPositions";
            btnClearMediaPositions.Size = new Size(149, 37);
            btnClearMediaPositions.TabIndex = 0;
            btnClearMediaPositions.Text = "Clear All Timings";
            toolTip1.SetToolTip(btnClearMediaPositions, "Copy to next blank Position");
            btnClearMediaPositions.Click += btnClearMediaPositions_Click;
            // 
            // label21
            // 
            label21.AutoSize = true;
            label21.Location = new Point(4, 83);
            label21.Margin = new Padding(4, 0, 4, 0);
            label21.Name = "label21";
            label21.Size = new Size(150, 17);
            label21.TabIndex = 1;
            label21.Text = "Sequence/Start mm:ss";
            label21.TextAlign = ContentAlignment.BottomLeft;
            // 
            // label19
            // 
            label19.AutoSize = true;
            label19.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label19.Location = new Point(5, 52);
            label19.Margin = new Padding(4, 0, 4, 0);
            label19.Name = "label19";
            label19.Size = new Size(63, 17);
            label19.TabIndex = 25;
            label19.Text = "Length:";
            // 
            // panelRotateLeftTop1
            // 
            panelRotateLeftTop1.Controls.Add(groupBox3);
            panelRotateLeftTop1.Dock = DockStyle.Top;
            panelRotateLeftTop1.Location = new Point(0, 0);
            panelRotateLeftTop1.Margin = new Padding(4, 5, 4, 5);
            panelRotateLeftTop1.Name = "panelRotateLeftTop1";
            panelRotateLeftTop1.Size = new Size(164, 102);
            panelRotateLeftTop1.TabIndex = 68;
            // 
            // groupBox3
            // 
            groupBox3.Controls.Add(Rotate_SlidesGapUpDown);
            groupBox3.Controls.Add(Rotate_Equal);
            groupBox3.Controls.Add(Rotate_Multiple);
            groupBox3.Location = new Point(5, 0);
            groupBox3.Margin = new Padding(4, 5, 4, 5);
            groupBox3.Name = "groupBox3";
            groupBox3.Padding = new Padding(4, 5, 4, 5);
            groupBox3.Size = new Size(149, 92);
            groupBox3.TabIndex = 9;
            groupBox3.TabStop = false;
            // 
            // Rotate_SlidesGapUpDown
            // 
            Rotate_SlidesGapUpDown.Location = new Point(84, 20);
            Rotate_SlidesGapUpDown.Margin = new Padding(4, 5, 4, 5);
            Rotate_SlidesGapUpDown.Maximum = new decimal(new int[] { 999, 0, 0, 0 });
            Rotate_SlidesGapUpDown.Name = "Rotate_SlidesGapUpDown";
            Rotate_SlidesGapUpDown.Size = new Size(57, 23);
            Rotate_SlidesGapUpDown.TabIndex = 2;
            toolTip1.SetToolTip(Rotate_SlidesGapUpDown, "Timing in seconds");
            // 
            // Rotate_Equal
            // 
            Rotate_Equal.AutoSize = true;
            Rotate_Equal.Location = new Point(9, 23);
            Rotate_Equal.Margin = new Padding(4, 5, 4, 5);
            Rotate_Equal.Name = "Rotate_Equal";
            Rotate_Equal.Size = new Size(75, 21);
            Rotate_Equal.TabIndex = 0;
            Rotate_Equal.Tag = "1";
            Rotate_Equal.Text = "Simple:";
            toolTip1.SetToolTip(Rotate_Equal, "Rotate each slide in equal seconds");
            Rotate_Equal.CheckedChanged += Rotate_Option_CheckedChanged;
            // 
            // Rotate_Multiple
            // 
            Rotate_Multiple.AutoSize = true;
            Rotate_Multiple.Location = new Point(9, 57);
            Rotate_Multiple.Margin = new Padding(4, 5, 4, 5);
            Rotate_Multiple.Name = "Rotate_Multiple";
            Rotate_Multiple.Size = new Size(97, 21);
            Rotate_Multiple.TabIndex = 1;
            Rotate_Multiple.Tag = "2";
            Rotate_Multiple.Text = "Sequence:";
            // 
            // panel6
            // 
            panel6.Dock = DockStyle.Right;
            panel6.Location = new Point(553, 0);
            panel6.Margin = new Padding(4, 5, 4, 5);
            panel6.Name = "panel6";
            panel6.Size = new Size(4, 571);
            panel6.TabIndex = 6;
            // 
            // menuStripMain
            // 
            menuStripMain.ImageScalingSize = new Size(20, 20);
            menuStripMain.Items.AddRange(new ToolStripItem[] { Menu_MainFile, Menu_MainTools });
            menuStripMain.Location = new Point(0, 0);
            menuStripMain.Name = "menuStripMain";
            menuStripMain.Padding = new Padding(8, 3, 0, 3);
            menuStripMain.Size = new Size(1033, 30);
            menuStripMain.TabIndex = 6;
            menuStripMain.Text = "menuStrip1";
            // 
            // Menu_MainFile
            // 
            Menu_MainFile.DropDownItems.AddRange(new ToolStripItem[] { Menu_New, Menu_Save, Menu_SaveAs, Menu_SaveExit, toolStripSeparator16, Menu_EditHistoryList, toolStripSeparator18, Menu_Exit });
            Menu_MainFile.Name = "Menu_MainFile";
            Menu_MainFile.Size = new Size(46, 24);
            Menu_MainFile.Text = "&File";
            // 
            // Menu_New
            // 
            Menu_New.Image = Resources.New;
            Menu_New.Name = "Menu_New";
            Menu_New.Size = new Size(173, 26);
            Menu_New.Text = "&New";
            Menu_New.Click += Menu_New_Click;
            // 
            // Menu_Save
            // 
            Menu_Save.Image = Resources.Save;
            Menu_Save.Name = "Menu_Save";
            Menu_Save.Size = new Size(173, 26);
            Menu_Save.Text = "&Save";
            Menu_Save.Click += Menu_Save_Click;
            // 
            // Menu_SaveAs
            // 
            Menu_SaveAs.Image = Resources.Save;
            Menu_SaveAs.Name = "Menu_SaveAs";
            Menu_SaveAs.Size = new Size(173, 26);
            Menu_SaveAs.Text = "Save &As...";
            Menu_SaveAs.Click += Menu_SaveAs_Click;
            // 
            // Menu_SaveExit
            // 
            Menu_SaveExit.Image = Resources.SaveClose;
            Menu_SaveExit.Name = "Menu_SaveExit";
            Menu_SaveExit.Size = new Size(173, 26);
            Menu_SaveExit.Text = "Save && &Exit";
            Menu_SaveExit.Click += Menu_SaveExit_Click;
            // 
            // toolStripSeparator16
            // 
            toolStripSeparator16.Name = "toolStripSeparator16";
            toolStripSeparator16.Size = new Size(170, 6);
            // 
            // Menu_EditHistoryList
            // 
            Menu_EditHistoryList.Name = "Menu_EditHistoryList";
            Menu_EditHistoryList.Size = new Size(173, 26);
            Menu_EditHistoryList.Text = "&Recent Edits";
            // 
            // toolStripSeparator18
            // 
            toolStripSeparator18.Name = "toolStripSeparator18";
            toolStripSeparator18.Size = new Size(170, 6);
            // 
            // Menu_Exit
            // 
            Menu_Exit.Name = "Menu_Exit";
            Menu_Exit.Size = new Size(173, 26);
            Menu_Exit.Text = "E&xit";
            Menu_Exit.Click += Menu_Exit_Click;
            // 
            // Menu_MainTools
            // 
            Menu_MainTools.DropDownItems.AddRange(new ToolStripItem[] { Menu_Import, Menu_WordWrap, Menu_ChordsMenu, Menu_EditHistorySeparator, Menu_TransposeDown, Menu_TransposeUp, toolStripSeparator6, Menu_ShowAllButtons });
            Menu_MainTools.Name = "Menu_MainTools";
            Menu_MainTools.Size = new Size(58, 24);
            Menu_MainTools.Text = "&Tools";
            // 
            // Menu_Import
            // 
            Menu_Import.Image = Resources.open;
            Menu_Import.Name = "Menu_Import";
            Menu_Import.Size = new Size(255, 26);
            Menu_Import.Text = "&Import...";
            Menu_Import.Click += Menu_Import_Click;
            // 
            // Menu_WordWrap
            // 
            Menu_WordWrap.CheckOnClick = true;
            Menu_WordWrap.Image = Resources.wordwrap;
            Menu_WordWrap.Name = "Menu_WordWrap";
            Menu_WordWrap.Size = new Size(255, 26);
            Menu_WordWrap.Text = "&Word Wrap";
            Menu_WordWrap.Click += Main_WordWrap_Click;
            // 
            // Menu_ChordsMenu
            // 
            Menu_ChordsMenu.CheckOnClick = true;
            Menu_ChordsMenu.Image = Resources.PopUpChords;
            Menu_ChordsMenu.Name = "Menu_ChordsMenu";
            Menu_ChordsMenu.Size = new Size(255, 26);
            Menu_ChordsMenu.Text = "Right-Click Chords Menu";
            Menu_ChordsMenu.Click += Menu_ChordsMenu_Click;
            // 
            // Menu_EditHistorySeparator
            // 
            Menu_EditHistorySeparator.Name = "Menu_EditHistorySeparator";
            Menu_EditHistorySeparator.Size = new Size(252, 6);
            // 
            // Menu_TransposeDown
            // 
            Menu_TransposeDown.Image = Resources.arrowGL;
            Menu_TransposeDown.Name = "Menu_TransposeDown";
            Menu_TransposeDown.Size = new Size(255, 26);
            Menu_TransposeDown.Text = "Transpose Chord &Down";
            Menu_TransposeDown.Click += Menu_TransposeDown_Click;
            // 
            // Menu_TransposeUp
            // 
            Menu_TransposeUp.Image = Resources.arrowGR;
            Menu_TransposeUp.Name = "Menu_TransposeUp";
            Menu_TransposeUp.Size = new Size(255, 26);
            Menu_TransposeUp.Text = "Transpose Chord &Up";
            Menu_TransposeUp.Click += Menu_TransposeUp_Click;
            // 
            // toolStripSeparator6
            // 
            toolStripSeparator6.Name = "toolStripSeparator6";
            toolStripSeparator6.Size = new Size(252, 6);
            // 
            // Menu_ShowAllButtons
            // 
            Menu_ShowAllButtons.CheckOnClick = true;
            Menu_ShowAllButtons.Name = "Menu_ShowAllButtons";
            Menu_ShowAllButtons.Size = new Size(255, 26);
            Menu_ShowAllButtons.Text = "Show All &Buttons";
            Menu_ShowAllButtons.Click += Menu_ShowAllButtons_Click;
            // 
            // TimerEditRequest
            // 
            TimerEditRequest.Interval = 1000;
            TimerEditRequest.Tick += TimerEditRequest_Tick;
            // 
            // OpenFileDialog1
            // 
            OpenFileDialog1.FileName = "openFileDialog1";
            // 
            // splitContainerMain
            // 
            splitContainerMain.Dock = DockStyle.Fill;
            splitContainerMain.FixedPanel = FixedPanel.Panel1;
            splitContainerMain.Location = new Point(0, 58);
            splitContainerMain.Margin = new Padding(4, 5, 4, 5);
            splitContainerMain.Name = "splitContainerMain";
            splitContainerMain.Orientation = Orientation.Horizontal;
            // 
            // splitContainerMain.Panel1
            // 
            splitContainerMain.Panel1.BackColor = SystemColors.Control;
            splitContainerMain.Panel1.Controls.Add(groupBox2);
            splitContainerMain.Panel1.Controls.Add(groupBox1);
            // 
            // splitContainerMain.Panel2
            // 
            splitContainerMain.Panel2.Controls.Add(splitContainer1);
            splitContainerMain.Size = new Size(1033, 783);
            splitContainerMain.SplitterDistance = 202;
            splitContainerMain.SplitterWidth = 6;
            splitContainerMain.TabIndex = 0;
            splitContainerMain.Text = "splitContainer2";
            splitContainerMain.SplitterMoved += splitContainerMain_SplitterMoved;
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(panelVerses);
            groupBox2.Controls.Add(panelOrderList);
            groupBox2.Controls.Add(panelSeqSet);
            groupBox2.Controls.Add(panelSeqUpDown);
            groupBox2.Location = new Point(596, 2);
            groupBox2.Margin = new Padding(4, 5, 4, 5);
            groupBox2.Name = "groupBox2";
            groupBox2.Padding = new Padding(0);
            groupBox2.Size = new Size(319, 198);
            groupBox2.TabIndex = 1;
            groupBox2.TabStop = false;
            // 
            // panelVerses
            // 
            panelVerses.BorderStyle = BorderStyle.Fixed3D;
            panelVerses.Controls.Add(VersesList);
            panelVerses.Controls.Add(panel2);
            panelVerses.Location = new Point(8, 15);
            panelVerses.Margin = new Padding(4, 5, 4, 5);
            panelVerses.Name = "panelVerses";
            panelVerses.Size = new Size(119, 173);
            panelVerses.TabIndex = 1;
            // 
            // VersesList
            // 
            VersesList.Columns.AddRange(new ColumnHeader[] { columnHeader1, columnHeader2 });
            VersesList.Dock = DockStyle.Fill;
            VersesList.FullRowSelect = true;
            VersesList.HeaderStyle = ColumnHeaderStyle.None;
            VersesList.Location = new Point(0, 20);
            VersesList.Margin = new Padding(4, 5, 4, 5);
            VersesList.Name = "VersesList";
            VersesList.ShowItemToolTips = true;
            VersesList.Size = new Size(115, 149);
            VersesList.TabIndex = 0;
            VersesList.UseCompatibleStateImageBehavior = false;
            VersesList.View = View.Details;
            VersesList.DoubleClick += VersesList_DoubleClick;
            // 
            // columnHeader1
            // 
            columnHeader1.Width = 65;
            // 
            // columnHeader2
            // 
            columnHeader2.Width = 0;
            // 
            // panel2
            // 
            panel2.BorderStyle = BorderStyle.FixedSingle;
            panel2.Controls.Add(label16);
            panel2.Dock = DockStyle.Top;
            panel2.Location = new Point(0, 0);
            panel2.Margin = new Padding(4, 5, 4, 5);
            panel2.Name = "panel2";
            panel2.Size = new Size(115, 20);
            panel2.TabIndex = 0;
            // 
            // label16
            // 
            label16.AutoSize = true;
            label16.Location = new Point(16, -2);
            label16.Margin = new Padding(4, 0, 4, 0);
            label16.Name = "label16";
            label16.Size = new Size(50, 20);
            label16.TabIndex = 0;
            label16.Text = "Verses";
            // 
            // panelOrderList
            // 
            panelOrderList.BorderStyle = BorderStyle.Fixed3D;
            panelOrderList.Controls.Add(OrderList);
            panelOrderList.Controls.Add(panel4);
            panelOrderList.Location = new Point(163, 15);
            panelOrderList.Margin = new Padding(4, 5, 4, 5);
            panelOrderList.Name = "panelOrderList";
            panelOrderList.Size = new Size(119, 173);
            panelOrderList.TabIndex = 2;
            // 
            // OrderList
            // 
            OrderList.Columns.AddRange(new ColumnHeader[] { columnHeader3, columnHeader4 });
            OrderList.Dock = DockStyle.Fill;
            OrderList.FullRowSelect = true;
            OrderList.HeaderStyle = ColumnHeaderStyle.None;
            OrderList.Location = new Point(0, 20);
            OrderList.Margin = new Padding(4, 5, 4, 5);
            OrderList.Name = "OrderList";
            OrderList.Size = new Size(115, 149);
            OrderList.TabIndex = 0;
            OrderList.UseCompatibleStateImageBehavior = false;
            OrderList.View = View.Details;
            OrderList.KeyUp += OrderList_KeyUp;
            // 
            // columnHeader3
            // 
            columnHeader3.Width = 65;
            // 
            // columnHeader4
            // 
            columnHeader4.Width = 0;
            // 
            // panel4
            // 
            panel4.BorderStyle = BorderStyle.FixedSingle;
            panel4.Controls.Add(label17);
            panel4.Dock = DockStyle.Top;
            panel4.Location = new Point(0, 0);
            panel4.Margin = new Padding(4, 5, 4, 5);
            panel4.Name = "panel4";
            panel4.Size = new Size(115, 20);
            panel4.TabIndex = 0;
            // 
            // label17
            // 
            label17.AutoSize = true;
            label17.Location = new Point(13, -2);
            label17.Margin = new Padding(4, 0, 4, 0);
            label17.Name = "label17";
            label17.Size = new Size(73, 20);
            label17.TabIndex = 0;
            label17.Text = "Sequence";
            // 
            // panelSeqSet
            // 
            panelSeqSet.Controls.Add(toolStripSeqSet);
            panelSeqSet.Location = new Point(127, 43);
            panelSeqSet.Margin = new Padding(4, 5, 4, 5);
            panelSeqSet.Name = "panelSeqSet";
            panelSeqSet.Size = new Size(33, 80);
            panelSeqSet.TabIndex = 13;
            // 
            // toolStripSeqSet
            // 
            toolStripSeqSet.AutoSize = false;
            toolStripSeqSet.CanOverflow = false;
            toolStripSeqSet.Dock = DockStyle.None;
            toolStripSeqSet.GripStyle = ToolStripGripStyle.Hidden;
            toolStripSeqSet.ImageScalingSize = new Size(20, 20);
            toolStripSeqSet.Items.AddRange(new ToolStripItem[] { Verses_Add, Verses_SmartAdd });
            toolStripSeqSet.LayoutStyle = ToolStripLayoutStyle.VerticalStackWithOverflow;
            toolStripSeqSet.Location = new Point(0, 2);
            toolStripSeqSet.Name = "toolStripSeqSet";
            toolStripSeqSet.RenderMode = ToolStripRenderMode.System;
            toolStripSeqSet.Size = new Size(33, 95);
            toolStripSeqSet.TabIndex = 5;
            // 
            // Verses_Add
            // 
            Verses_Add.AutoSize = false;
            Verses_Add.DisplayStyle = ToolStripItemDisplayStyle.Image;
            Verses_Add.Image = Resources.arrowR;
            Verses_Add.ImageTransparentColor = Color.Magenta;
            Verses_Add.Name = "Verses_Add";
            Verses_Add.Size = new Size(22, 22);
            Verses_Add.Tag = "";
            Verses_Add.ToolTipText = "Move Item Up";
            Verses_Add.Click += Verses_Add_Click;
            // 
            // Verses_SmartAdd
            // 
            Verses_SmartAdd.AutoSize = false;
            Verses_SmartAdd.DisplayStyle = ToolStripItemDisplayStyle.Image;
            Verses_SmartAdd.Image = Resources.multi_arrowr;
            Verses_SmartAdd.ImageTransparentColor = Color.Magenta;
            Verses_SmartAdd.Name = "Verses_SmartAdd";
            Verses_SmartAdd.Size = new Size(22, 22);
            Verses_SmartAdd.Tag = "";
            Verses_SmartAdd.ToolTipText = "Move Item Down";
            Verses_SmartAdd.Click += Verses_Add_Click;
            // 
            // panelSeqUpDown
            // 
            panelSeqUpDown.Controls.Add(toolStripSeqUpDown);
            panelSeqUpDown.Location = new Point(281, 40);
            panelSeqUpDown.Margin = new Padding(4, 5, 4, 5);
            panelSeqUpDown.Name = "panelSeqUpDown";
            panelSeqUpDown.Size = new Size(33, 122);
            panelSeqUpDown.TabIndex = 12;
            // 
            // toolStripSeqUpDown
            // 
            toolStripSeqUpDown.AutoSize = false;
            toolStripSeqUpDown.CanOverflow = false;
            toolStripSeqUpDown.Dock = DockStyle.None;
            toolStripSeqUpDown.GripStyle = ToolStripGripStyle.Hidden;
            toolStripSeqUpDown.ImageScalingSize = new Size(20, 20);
            toolStripSeqUpDown.Items.AddRange(new ToolStripItem[] { OrderList_Up, OrderList_Down, toolStripSeparator5, OrderList_Delete });
            toolStripSeqUpDown.LayoutStyle = ToolStripLayoutStyle.VerticalStackWithOverflow;
            toolStripSeqUpDown.Location = new Point(0, 0);
            toolStripSeqUpDown.Name = "toolStripSeqUpDown";
            toolStripSeqUpDown.RenderMode = ToolStripRenderMode.System;
            toolStripSeqUpDown.Size = new Size(33, 128);
            toolStripSeqUpDown.TabIndex = 5;
            // 
            // OrderList_Up
            // 
            OrderList_Up.AutoSize = false;
            OrderList_Up.DisplayStyle = ToolStripItemDisplayStyle.Image;
            OrderList_Up.Image = Resources.handup;
            OrderList_Up.ImageTransparentColor = Color.Magenta;
            OrderList_Up.Name = "OrderList_Up";
            OrderList_Up.Size = new Size(22, 22);
            OrderList_Up.Tag = "up";
            OrderList_Up.ToolTipText = "Move Item Up";
            OrderList_Up.Click += OrderList_Btn_Click;
            // 
            // OrderList_Down
            // 
            OrderList_Down.AutoSize = false;
            OrderList_Down.DisplayStyle = ToolStripItemDisplayStyle.Image;
            OrderList_Down.Image = Resources.handdown;
            OrderList_Down.ImageTransparentColor = Color.Magenta;
            OrderList_Down.Name = "OrderList_Down";
            OrderList_Down.Size = new Size(22, 22);
            OrderList_Down.Tag = "down";
            OrderList_Down.ToolTipText = "Move Item Down";
            OrderList_Down.Click += OrderList_Btn_Click;
            // 
            // toolStripSeparator5
            // 
            toolStripSeparator5.Name = "toolStripSeparator5";
            toolStripSeparator5.Size = new Size(31, 6);
            // 
            // OrderList_Delete
            // 
            OrderList_Delete.AutoSize = false;
            OrderList_Delete.DisplayStyle = ToolStripItemDisplayStyle.Image;
            OrderList_Delete.Image = Resources.Delete;
            OrderList_Delete.ImageTransparentColor = Color.Magenta;
            OrderList_Delete.Name = "OrderList_Delete";
            OrderList_Delete.Size = new Size(22, 22);
            OrderList_Delete.Tag = "delete";
            OrderList_Delete.ToolTipText = "Delete";
            OrderList_Delete.Click += OrderList_Btn_Click;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(panel7);
            groupBox1.Controls.Add(panel8);
            groupBox1.Location = new Point(4, 2);
            groupBox1.Margin = new Padding(4, 5, 8, 9);
            groupBox1.Name = "groupBox1";
            groupBox1.Padding = new Padding(0);
            groupBox1.Size = new Size(599, 198);
            groupBox1.TabIndex = 0;
            groupBox1.TabStop = false;
            // 
            // panel7
            // 
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
            panel7.Location = new Point(4, 15);
            panel7.Margin = new Padding(4, 5, 4, 5);
            panel7.Name = "panel7";
            panel7.Size = new Size(288, 175);
            panel7.TabIndex = 0;
            // 
            // Btn_Title2
            // 
            Btn_Title2.BackColor = Color.Aqua;
            Btn_Title2.FlatStyle = FlatStyle.Flat;
            Btn_Title2.Location = new Point(187, 74);
            Btn_Title2.Margin = new Padding(4, 5, 4, 5);
            Btn_Title2.Name = "Btn_Title2";
            Btn_Title2.Size = new Size(24, 31);
            Btn_Title2.TabIndex = 36;
            Btn_Title2.TabStop = false;
            Btn_Title2.Text = "...";
            Btn_Title2.UseVisualStyleBackColor = false;
            Btn_Title2.Visible = false;
            Btn_Title2.Click += Btn_Click;
            Btn_Title2.Enter += Btn_Enter;
            Btn_Title2.Leave += Btn_Enter;
            // 
            // Btn_Title
            // 
            Btn_Title.BackColor = Color.Aqua;
            Btn_Title.FlatStyle = FlatStyle.Flat;
            Btn_Title.Location = new Point(257, 40);
            Btn_Title.Margin = new Padding(4, 5, 4, 5);
            Btn_Title.Name = "Btn_Title";
            Btn_Title.Size = new Size(24, 31);
            Btn_Title.TabIndex = 35;
            Btn_Title.TabStop = false;
            Btn_Title.Text = "...";
            Btn_Title.UseVisualStyleBackColor = false;
            Btn_Title.Visible = false;
            Btn_Title.Click += Btn_Click;
            Btn_Title.Enter += Btn_Enter;
            Btn_Title.Leave += Btn_Enter;
            // 
            // Btn_Copyright
            // 
            Btn_Copyright.BackColor = Color.Aqua;
            Btn_Copyright.FlatStyle = FlatStyle.Flat;
            Btn_Copyright.Location = new Point(257, 142);
            Btn_Copyright.Margin = new Padding(4, 5, 4, 5);
            Btn_Copyright.Name = "Btn_Copyright";
            Btn_Copyright.Size = new Size(24, 31);
            Btn_Copyright.TabIndex = 34;
            Btn_Copyright.TabStop = false;
            Btn_Copyright.Text = "...";
            Btn_Copyright.UseVisualStyleBackColor = false;
            Btn_Copyright.Visible = false;
            Btn_Copyright.Click += Btn_Click;
            Btn_Copyright.Enter += Btn_Enter;
            Btn_Copyright.Leave += Btn_Enter;
            // 
            // Btn_Writer
            // 
            Btn_Writer.BackColor = Color.Aqua;
            Btn_Writer.FlatStyle = FlatStyle.Flat;
            Btn_Writer.Location = new Point(257, 108);
            Btn_Writer.Margin = new Padding(4, 5, 4, 5);
            Btn_Writer.Name = "Btn_Writer";
            Btn_Writer.Size = new Size(24, 31);
            Btn_Writer.TabIndex = 33;
            Btn_Writer.TabStop = false;
            Btn_Writer.Text = "...";
            Btn_Writer.UseVisualStyleBackColor = false;
            Btn_Writer.Visible = false;
            Btn_Writer.Click += Btn_Click;
            Btn_Writer.Enter += Btn_Enter;
            Btn_Writer.Leave += Btn_Enter;
            // 
            // SongFolder
            // 
            SongFolder.DropDownStyle = ComboBoxStyle.DropDownList;
            SongFolder.FormattingEnabled = true;
            SongFolder.Location = new Point(124, 5);
            SongFolder.Margin = new Padding(4, 5, 4, 5);
            SongFolder.MaxDropDownItems = 12;
            SongFolder.Name = "SongFolder";
            SongFolder.Size = new Size(156, 28);
            SongFolder.TabIndex = 1;
            SongFolder.SelectedIndexChanged += SongFolder_SelectedIndexChanged;
            // 
            // panelLinkTitle2Lookup
            // 
            panelLinkTitle2Lookup.Controls.Add(toolStrip2);
            panelLinkTitle2Lookup.Location = new Point(245, 72);
            panelLinkTitle2Lookup.Margin = new Padding(4, 5, 4, 5);
            panelLinkTitle2Lookup.Name = "panelLinkTitle2Lookup";
            panelLinkTitle2Lookup.Size = new Size(29, 32);
            panelLinkTitle2Lookup.TabIndex = 28;
            // 
            // toolStrip2
            // 
            toolStrip2.AutoSize = false;
            toolStrip2.CanOverflow = false;
            toolStrip2.Dock = DockStyle.None;
            toolStrip2.GripStyle = ToolStripGripStyle.Hidden;
            toolStrip2.ImageScalingSize = new Size(20, 20);
            toolStrip2.Items.AddRange(new ToolStripItem[] { Title2_LookUp });
            toolStrip2.LayoutStyle = ToolStripLayoutStyle.Flow;
            toolStrip2.Location = new Point(0, -2);
            toolStrip2.Name = "toolStrip2";
            toolStrip2.RenderMode = ToolStripRenderMode.System;
            toolStrip2.Size = new Size(37, 37);
            toolStrip2.TabIndex = 4;
            // 
            // Title2_LookUp
            // 
            Title2_LookUp.AutoSize = false;
            Title2_LookUp.DisplayStyle = ToolStripItemDisplayStyle.Image;
            Title2_LookUp.Image = Resources.folder;
            Title2_LookUp.ImageTransparentColor = Color.Magenta;
            Title2_LookUp.Name = "Title2_LookUp";
            Title2_LookUp.Size = new Size(22, 22);
            Title2_LookUp.Tag = "down";
            Title2_LookUp.ToolTipText = "Look Up Title";
            Title2_LookUp.Click += Title2_LookUp_Click;
            // 
            // LinkTitle2Pic
            // 
            LinkTitle2Pic.BackgroundImage = Resources.Tick;
            LinkTitle2Pic.BackgroundImageLayout = ImageLayout.Center;
            LinkTitle2Pic.Location = new Point(216, 74);
            LinkTitle2Pic.Margin = new Padding(4, 5, 4, 5);
            LinkTitle2Pic.Name = "LinkTitle2Pic";
            LinkTitle2Pic.Size = new Size(28, 32);
            LinkTitle2Pic.TabIndex = 6;
            // 
            // CopyrightInfo
            // 
            CopyrightInfo.Location = new Point(75, 142);
            CopyrightInfo.Margin = new Padding(4, 5, 4, 5);
            CopyrightInfo.MaxLength = 100;
            CopyrightInfo.Name = "CopyrightInfo";
            CopyrightInfo.Size = new Size(207, 27);
            CopyrightInfo.TabIndex = 6;
            CopyrightInfo.Enter += TextBox_Enter;
            CopyrightInfo.Leave += TextBox_Leave;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(4, 45);
            label2.Margin = new Padding(4, 0, 4, 0);
            label2.Name = "label2";
            label2.Size = new Size(41, 20);
            label2.TabIndex = 4;
            label2.Text = "Title:";
            label2.TextAlign = ContentAlignment.BottomLeft;
            // 
            // WriterInfo
            // 
            WriterInfo.Location = new Point(75, 108);
            WriterInfo.Margin = new Padding(4, 5, 4, 5);
            WriterInfo.MaxLength = 100;
            WriterInfo.Name = "WriterInfo";
            WriterInfo.Size = new Size(207, 27);
            WriterInfo.TabIndex = 5;
            WriterInfo.Enter += TextBox_Enter;
            WriterInfo.Leave += TextBox_Leave;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(4, 78);
            label3.Margin = new Padding(4, 0, 4, 0);
            label3.Name = "label3";
            label3.Size = new Size(49, 20);
            label3.TabIndex = 5;
            label3.Text = "Title2:";
            label3.TextAlign = ContentAlignment.BottomLeft;
            // 
            // SongTitle2
            // 
            SongTitle2.Location = new Point(75, 74);
            SongTitle2.Margin = new Padding(4, 5, 4, 5);
            SongTitle2.MaxLength = 100;
            SongTitle2.Name = "SongTitle2";
            SongTitle2.Size = new Size(136, 27);
            SongTitle2.TabIndex = 3;
            SongTitle2.Enter += TextBox_Enter;
            SongTitle2.Leave += TextBox_Leave;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(4, 112);
            label4.Margin = new Padding(4, 0, 4, 0);
            label4.Name = "label4";
            label4.Size = new Size(53, 20);
            label4.TabIndex = 6;
            label4.Text = "Writer:";
            label4.TextAlign = ContentAlignment.BottomLeft;
            // 
            // SongTitle
            // 
            SongTitle.Location = new Point(75, 40);
            SongTitle.Margin = new Padding(4, 5, 4, 5);
            SongTitle.MaxLength = 100;
            SongTitle.Name = "SongTitle";
            SongTitle.Size = new Size(187, 27);
            SongTitle.TabIndex = 2;
            SongTitle.Enter += TextBox_Enter;
            SongTitle.Leave += TextBox_Leave;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(4, 145);
            label5.Margin = new Padding(4, 0, 4, 0);
            label5.Name = "label5";
            label5.Size = new Size(77, 20);
            label5.TabIndex = 7;
            label5.Text = "Copyright:";
            label5.TextAlign = ContentAlignment.BottomLeft;
            // 
            // labelFormat
            // 
            labelFormat.AutoSize = true;
            labelFormat.Location = new Point(4, 9);
            labelFormat.Margin = new Padding(4, 0, 4, 0);
            labelFormat.Name = "labelFormat";
            labelFormat.Size = new Size(125, 20);
            labelFormat.TabIndex = 29;
            labelFormat.Text = "Format As Folder:";
            labelFormat.TextAlign = ContentAlignment.BottomLeft;
            // 
            // panel8
            // 
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
            panel8.Location = new Point(293, 15);
            panel8.Margin = new Padding(4, 5, 4, 5);
            panel8.Name = "panel8";
            panel8.Size = new Size(295, 175);
            panel8.TabIndex = 1;
            // 
            // Btn_BookRef
            // 
            Btn_BookRef.BackColor = Color.Aqua;
            Btn_BookRef.FlatStyle = FlatStyle.Flat;
            Btn_BookRef.Location = new Point(267, 72);
            Btn_BookRef.Margin = new Padding(4, 5, 4, 5);
            Btn_BookRef.Name = "Btn_BookRef";
            Btn_BookRef.Size = new Size(24, 31);
            Btn_BookRef.TabIndex = 31;
            Btn_BookRef.TabStop = false;
            Btn_BookRef.Text = "...";
            Btn_BookRef.UseVisualStyleBackColor = false;
            Btn_BookRef.Visible = false;
            Btn_BookRef.Click += Btn_Click;
            Btn_BookRef.Enter += Btn_Enter;
            Btn_BookRef.Leave += Btn_Enter;
            // 
            // Btn_UserRef
            // 
            Btn_UserRef.BackColor = Color.Aqua;
            Btn_UserRef.FlatStyle = FlatStyle.Flat;
            Btn_UserRef.Location = new Point(267, 106);
            Btn_UserRef.Margin = new Padding(4, 5, 4, 5);
            Btn_UserRef.Name = "Btn_UserRef";
            Btn_UserRef.Size = new Size(24, 31);
            Btn_UserRef.TabIndex = 30;
            Btn_UserRef.TabStop = false;
            Btn_UserRef.Text = "...";
            Btn_UserRef.UseVisualStyleBackColor = false;
            Btn_UserRef.Visible = false;
            Btn_UserRef.Click += Btn_Click;
            Btn_UserRef.Enter += Btn_Enter;
            Btn_UserRef.Leave += Btn_Enter;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(4, 9);
            label6.Margin = new Padding(4, 0, 4, 0);
            label6.Name = "label6";
            label6.Size = new Size(47, 20);
            label6.TabIndex = 12;
            label6.Text = "Capo:";
            label6.TextAlign = ContentAlignment.BottomLeft;
            // 
            // UserReference
            // 
            UserReference.Location = new Point(75, 106);
            UserReference.Margin = new Padding(4, 5, 4, 5);
            UserReference.Name = "UserReference";
            UserReference.Size = new Size(191, 27);
            UserReference.TabIndex = 5;
            UserReference.Enter += TextBox_Enter;
            UserReference.Leave += TextBox_Leave;
            // 
            // BookReference
            // 
            BookReference.Location = new Point(75, 72);
            BookReference.Margin = new Padding(4, 5, 4, 5);
            BookReference.MaxLength = 100;
            BookReference.Name = "BookReference";
            BookReference.Size = new Size(191, 27);
            BookReference.TabIndex = 4;
            BookReference.Enter += TextBox_Enter;
            BookReference.Leave += TextBox_Leave;
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Location = new Point(4, 112);
            label9.Margin = new Padding(4, 0, 4, 0);
            label9.Name = "label9";
            label9.Size = new Size(67, 20);
            label9.TabIndex = 19;
            label9.Text = "User Ref:";
            label9.TextAlign = ContentAlignment.BottomLeft;
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Location = new Point(4, 145);
            label10.Margin = new Padding(4, 0, 4, 0);
            label10.Name = "label10";
            label10.Size = new Size(68, 20);
            label10.TabIndex = 20;
            label10.Text = "Admin 1:";
            label10.TextAlign = ContentAlignment.BottomLeft;
            // 
            // LicAdminInfo2
            // 
            LicAdminInfo2.DropDownStyle = ComboBoxStyle.DropDownList;
            LicAdminInfo2.FormattingEnabled = true;
            LicAdminInfo2.Location = new Point(195, 140);
            LicAdminInfo2.Margin = new Padding(4, 5, 4, 5);
            LicAdminInfo2.MaxDropDownItems = 12;
            LicAdminInfo2.Name = "LicAdminInfo2";
            LicAdminInfo2.Size = new Size(95, 28);
            LicAdminInfo2.TabIndex = 7;
            // 
            // LicAdminInfo1
            // 
            LicAdminInfo1.DropDownStyle = ComboBoxStyle.DropDownList;
            LicAdminInfo1.FormattingEnabled = true;
            LicAdminInfo1.Location = new Point(75, 140);
            LicAdminInfo1.Margin = new Padding(4, 5, 4, 5);
            LicAdminInfo1.MaxDropDownItems = 12;
            LicAdminInfo1.Name = "LicAdminInfo1";
            LicAdminInfo1.Size = new Size(95, 28);
            LicAdminInfo1.TabIndex = 6;
            // 
            // SongTiming
            // 
            SongTiming.FormattingEnabled = true;
            SongTiming.Location = new Point(221, 37);
            SongTiming.Margin = new Padding(4, 5, 4, 5);
            SongTiming.MaxDropDownItems = 13;
            SongTiming.Name = "SongTiming";
            SongTiming.Size = new Size(68, 28);
            SongTiming.TabIndex = 3;
            // 
            // label13
            // 
            label13.AutoSize = true;
            label13.Location = new Point(173, 145);
            label13.Margin = new Padding(4, 0, 4, 0);
            label13.Name = "label13";
            label13.Size = new Size(20, 20);
            label13.TabIndex = 23;
            label13.Text = "2:";
            label13.TextAlign = ContentAlignment.BottomLeft;
            // 
            // SongKey
            // 
            SongKey.DropDownStyle = ComboBoxStyle.DropDownList;
            SongKey.FormattingEnabled = true;
            SongKey.Location = new Point(221, 3);
            SongKey.Margin = new Padding(4, 5, 4, 5);
            SongKey.MaxDropDownItems = 13;
            SongKey.Name = "SongKey";
            SongKey.Size = new Size(68, 28);
            SongKey.TabIndex = 1;
            // 
            // SongNumber
            // 
            SongNumber.Location = new Point(75, 38);
            SongNumber.Margin = new Padding(4, 5, 4, 5);
            SongNumber.MaxLength = 10;
            SongNumber.Name = "SongNumber";
            SongNumber.Size = new Size(92, 27);
            SongNumber.TabIndex = 2;
            // 
            // SongCapo
            // 
            SongCapo.DropDownStyle = ComboBoxStyle.DropDownList;
            SongCapo.FormattingEnabled = true;
            SongCapo.Location = new Point(75, 3);
            SongCapo.Margin = new Padding(4, 5, 4, 5);
            SongCapo.MaxDropDownItems = 13;
            SongCapo.Name = "SongCapo";
            SongCapo.Size = new Size(92, 28);
            SongCapo.TabIndex = 0;
            // 
            // label11
            // 
            label11.AutoSize = true;
            label11.Location = new Point(172, 8);
            label11.Margin = new Padding(4, 0, 4, 0);
            label11.Name = "label11";
            label11.Size = new Size(36, 20);
            label11.TabIndex = 21;
            label11.Text = "Key:";
            label11.TextAlign = ContentAlignment.BottomLeft;
            // 
            // label12
            // 
            label12.AutoSize = true;
            label12.Location = new Point(171, 43);
            label12.Margin = new Padding(4, 0, 4, 0);
            label12.Name = "label12";
            label12.Size = new Size(58, 20);
            label12.TabIndex = 22;
            label12.Text = "Timing:";
            label12.TextAlign = ContentAlignment.BottomLeft;
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new Point(4, 78);
            label8.Margin = new Padding(4, 0, 4, 0);
            label8.Name = "label8";
            label8.Size = new Size(72, 20);
            label8.TabIndex = 18;
            label8.Text = "Book Ref:";
            label8.TextAlign = ContentAlignment.BottomLeft;
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(4, 45);
            label7.Margin = new Padding(4, 0, 4, 0);
            label7.Name = "label7";
            label7.Size = new Size(73, 20);
            label7.TabIndex = 17;
            label7.Text = "Song No.:";
            label7.TextAlign = ContentAlignment.BottomLeft;
            // 
            // TimerFast
            // 
            TimerFast.Interval = 500;
            TimerFast.Tick += TimerFast_Tick;
            // 
            // TimerAttemptConnect
            // 
            TimerAttemptConnect.Interval = 500;
            TimerAttemptConnect.Tick += TimerAttemptConnect_Tick;
            // 
            // TimerTrack
            // 
            TimerTrack.Interval = 1000;
            TimerTrack.Tick += TimerTrack_Tick;
            // 
            // FrmInfoScreen
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1033, 863);
            Controls.Add(splitContainerMain);
            Controls.Add(toolStrip1);
            Controls.Add(statusStrip1);
            Controls.Add(menuStripMain);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Margin = new Padding(4, 5, 4, 5);
            MinimumSize = new Size(321, 341);
            Name = "FrmInfoScreen";
            SizeGripStyle = SizeGripStyle.Show;
            Text = "InfoScreen";
            FormClosing += FrmInfoScreen_FormClosing;
            Load += FrmInfoScreen_Load;
            Resize += FrmInfoScreen_Resize;
            toolStrip1.ResumeLayout(false);
            toolStrip1.PerformLayout();
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            ((ISupportInitialize)splitContainer1).EndInit();
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
            ((ISupportInitialize)splitContainerRotate).EndInit();
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
            ((ISupportInitialize)TrackBarVolume).EndInit();
            panelPlayBtns.ResumeLayout(false);
            ((ISupportInitialize)TrackBarDuration).EndInit();
            panelRotateLeft.ResumeLayout(false);
            panelRotateLeftTop2.ResumeLayout(false);
            panelRotateLeftTop2.PerformLayout();
            panelRotateLeftTop1.ResumeLayout(false);
            groupBox3.ResumeLayout(false);
            groupBox3.PerformLayout();
            ((ISupportInitialize)Rotate_SlidesGapUpDown).EndInit();
            menuStripMain.ResumeLayout(false);
            menuStripMain.PerformLayout();
            splitContainerMain.Panel1.ResumeLayout(false);
            splitContainerMain.Panel2.ResumeLayout(false);
            ((ISupportInitialize)splitContainerMain).EndInit();
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
