//using NetOffice.DAOApi;
using Easislides.Properties;
using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Data;
using System.Data.OleDb;
using Easislides.Util;
//using System.Data.SQLite;
using Easislides.SQLite;
using Easislides.Module;
using Easislides.Util;
using System.Runtime.InteropServices;

using System.Text;
using System.Collections.Generic;
using System.Web;




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
    public class FrmOptions : Form
    {
        private const int SampleSplitterTop = 6;

        private const int SampleSplitterBottom = 117;

        private string currentline;

        private int confile;

        private string result;

        private string tempString;

        private int CurFolder;

        private bool DirOK;

        private bool InitFormLoad = true;

        private float SampleSplitterVerticalIncrement = 0.96f;

        private float SampleSplitterHorizontalIncrement = 1.4f;

        private string[,] tempFolderLyricsHeading = new string[41, 4];

        private string[] tempFolderName = new string[41];

        private int[] tempFolderGroupStyle = new int[41];

        private bool[] tempFolderUse = new bool[41];

        private int[,] tempShowFontVPosition = new int[41, 2];

        private int[,] tempShowFontSize = new int[41, 2];

        private int[,] tempShowFontVPositionMax = new int[41, 2];

        private int[,] tempShowFontVPositionMin = new int[41, 2];

        private int[] tempLeftMargin = new int[41];

        private int[] tempRightMargin = new int[41];

        private int[] tempBottomMargin = new int[41];

        private string[,] tempShowFontName = new string[41, 2];

        private bool[,] tempShowFontBold = new bool[41, 4];

        private bool[,] tempShowFontItalic = new bool[41, 4];

        private bool[,] tempShowFontUnderline = new bool[41, 4];

        private bool[,] tempShowFontRTL = new bool[41, 2];

        public int[] tempFolderHeadingPercentSize = new int[41];

        public int[] tempFolderHeadingOption = new int[41];

        public bool[,] tempFolderHeadingFontBold = new bool[41, 2];

        public bool[,] tempFolderHeadingFontItalic = new bool[41, 2];

        public bool[,] tempFolderHeadingFontUnderline = new bool[41, 2];

        public double[,] tempShowLineSpacing = new double[41, 2];

        private bool LoadTempPos = false;

        private int tempHB_TotalVersions = 0;

        private IContainer components = null;

        private TabControl tabControl1;

        private TabPage tabPageMainWindow;

        private TabPage tabPageFolders;

        private Button BtnCancel;

        private Button BtnOK;

        private TabPage tabPageAlerts;

        private TabPage tabPageBibles;

        private TabPage tabPageLicence;

        private TabPage tabPageKeyboard;

        private GroupBox groupBox1;

        private GroupBox groupBox6;

        private GroupBox groupBoxDM;

        private GroupBox groupBox4;

        private GroupBox groupBox3;

        private GroupBox GroupBoxFolder;

        private GroupBox SelectedFolderGroupBox;

        private GroupBox groupBox14;

        private GroupBox groupBox13;

        private GroupBox GroupBoxFont1;

        private GroupBox GroupBoxFont0;

        private GroupBox groupBox10;

        private GroupBox GroupBoxHeadings;

        private GroupBox groupBox15;

        private GroupBox groupBox19;

        private GroupBox groupBox17;

        private GroupBox groupBox18;

        private GroupBox groupBox20;

        private Panel panel1;

        private NumericUpDown VersesMaxUpDown;

        private Label label1;

        private Panel panel4;

        private NumericUpDown EditHistoryMaxUpDown;

        private Label label4;

        private Panel panel3;

        private NumericUpDown PPMaxUpDown;

        private Label label3;

        private Panel panel2;

        private NumericUpDown AdhocVersesMaxUpDown;

        private Label label2;

        private CheckBox TextRegionUseColour;

        private Button btnTextRegionChangeColour;

        private CheckBox cbUseLargestFont;

        private CheckBox cbAutoTextOverflow;

        private RadioButton rbGapItemOption2;

        private RadioButton rbGapItemOption1;

        private RadioButton rbGapItemOption0;

        private CheckBox cbAdvanceNextItem;

        private CheckBox DM_AlwaysUseSecondaryMonitor;

        private TextBox tbMusicLocation;

        private Panel panel5;

        private ToolStrip toolStrip1;

        private ToolStripButton MusicLocationBtn;

        private Panel panelLinkTitle2Lookup;

        private ToolStrip toolStrip2;

        private ToolStripButton Monitor_Info;

        private CheckBox cbFolderUse;

        private ListView SongFolder;

        private ColumnHeader columnHeader3;

        private Panel panel6;

        private NumericUpDown ShowHeadingsPercentSizeUpDown;

        private Label label6;

        private Panel panelInd4;

        private ToolStrip HeadingsFontToolbar;

        private ToolStripButton HeadingsFont_Bold;

        private ToolStripButton HeadingsFont_Underline;

        private Label label8;

        private Panel panel7;

        private ToolStrip ToolBarFontBtn0;

        private ToolStripButton ToolBarFont_R1Bold;

        private ToolStripButton ToolBarFont_R1Underline;

        private Panel panelInd5;

        private ToolStrip toolStripInd5;

        private ToolStripComboBox ComboFontName0;

        private NumericUpDown FontSizeUpDown0;

        private Panel panel8;

        private ToolStrip toolStrip4;

        private ToolStripComboBox ComboFontName1;

        private Panel panel9;

        private NumericUpDown FontSizeUpDown1;

        private ToolStrip ToolBarFontBtn1;

        private ToolStripButton ToolBarFont_R2Bold;

        private ToolStripButton ToolBarFont_R2Underline;

        private Label label11;

        private NumericUpDown ShowLineSpacingMaxUpDown;

        private Label label13;

        private Label label12;

        private Label label15;

        private NumericUpDown RightMarginUpDown;

        private NumericUpDown LeftMarginUpDown;

        private NumericUpDown FontPositionUpDownBottom;

        private NumericUpDown FontPositionUpDown1;

        private NumericUpDown FontPositionUpDown0;

        private Panel Sample_PanelMain;

        private Label label17;

        private Label label16;

        private Label label18;

        private Label label19;

        private Panel panel13;

        private NumericUpDown MessageSizeUpDown;

        private Label label22;

        private Label label21;

        private Panel panel11;

        private NumericUpDown MessageAlertDurationUpDown;

        private Label label20;

        private Panel panel14;

        private ToolStrip toolStrip6;

        private ToolStripComboBox tbLyricsHeading0;

        private Panel panel16;

        private ToolStrip toolStrip8;

        private ToolStripComboBox tbLyricsHeading2;

        private Panel panel15;

        private ToolStrip toolStrip7;

        private ToolStripComboBox tbLyricsHeading1;

        private Panel panel17;

        private ToolStrip toolStrip9;

        private ToolStripComboBox ComboLyricsHeading;

        private Panel panelDM;

        private ToolStrip toolStripMonitorList;

        private ToolStripComboBox DualMonitorList;

        private Panel panel19;

        private ToolStrip toolStrip11;

        private ToolStripComboBox MessageComboFont;

        private Panel panel12;

        private ToolStrip ToolBarMessageFormat;

        private ToolStripButton Message_Bold;

        private ToolStripButton Message_Italics;

        private ToolStripButton Message_Underline;

        private ToolStripButton Message_Shadow;

        private ToolStripButton Message_Outline;

        private ToolStripDropDownButton Message_Align;

        private ToolStripMenuItem Message_AlignLeft;

        private ToolStripMenuItem Message_AlignCentre;

        private ToolStripMenuItem Message_AlignRight;

        private Panel panel20;

        private Button btnMessageChangeBackColour;

        private Button btnMessageChangeTextColour;

        private GroupBox groupBox16;

        private Button btnParentalChangeBackColour;

        private Panel panel23;

        private Button btnParentalChangeTextColour;

        private Panel panel24;

        private ToolStrip ToolBarParentalFormat;

        private ToolStripButton Parental_Bold;

        private ToolStripButton Parental_Italics;

        private ToolStripButton Parental_Underline;

        private ToolStripButton Parental_Shadow;

        private ToolStripButton Parental_Outline;

        private ToolStripDropDownButton Parental_Align;

        private ToolStripMenuItem Parental_AlignLeft;

        private ToolStripMenuItem Parental_AlignCentre;

        private ToolStripMenuItem Parental_AlignRight;

        private Panel panel25;

        private Label label27;

        private ToolStrip toolStrip14;

        private ToolStripComboBox ParentalComboFont;

        private Panel panel26;

        private NumericUpDown ParentalSizeUpDown;

        private Label label28;

        private Panel panel27;

        private NumericUpDown ParentalAlertUpDown;

        private Label label29;

        private TextBox ParentalAlert;

        private Label label30;

        private ListView BibleList;

        private ColumnHeader columnHeader4;

        private ColumnHeader columnHeader5;

        private ColumnHeader columnHeader6;

        private ColumnHeader columnHeader7;

        private ColumnHeader columnHeader8;

        private ColumnHeader columnHeader9;

        private Panel panel28;

        private ToolStrip ToolBarBibles;

        private ToolStripButton Bibles_Up;

        private ToolStripButton Bibles_Down;

        private ToolStripButton Bibles_Info;

        private Panel panel29;

        private ToolStrip toolStrip16;

        private ToolStripComboBox BibleAssociatedFolder;

        private NumericUpDown BibleFontSizeUpDown;

        private Label label31;

        private Button btnBibleRemove;

        private Button btnBibleNameChange;

        private Button btnBibleAdd;

        private Button btnBibleSearch;

        private ListView BibleSearchList;

        private ColumnHeader columnHeader12;

        private ColumnHeader columnHeader13;

        private ColumnHeader columnHeader14;

        private ColumnHeader columnHeader15;

        private Label label32;

        private TextBox AdminLic1;

        private TextBox tbNumberSymbol;

        private Panel panel30;

        private TextBox AdminLic8;

        private TextBox AdminLic7;

        private TextBox AdminLic6;

        private TextBox AdminLic5;

        private TextBox AdminLic4;

        private TextBox AdminLic3;

        private TextBox AdminLic2;

        private Label label33;

        private Label label34;

        private Panel panel32;

        private TextBox AdminLicPreview8;

        private TextBox AdminLicPreview7;

        private TextBox AdminLicPreview6;

        private TextBox AdminLicPreview5;

        private TextBox AdminLicPreview4;

        private TextBox AdminLicPreview3;

        private TextBox AdminLicPreview2;

        private Label label36;

        private TextBox AdminLicPreview1;

        private Panel panel31;

        private TextBox AdminLicNo8;

        private TextBox AdminLicNo7;

        private TextBox AdminLicNo6;

        private TextBox AdminLicNo5;

        private TextBox AdminLicNo4;

        private TextBox AdminLicNo3;

        private TextBox AdminLicNo2;

        private Label label35;

        private TextBox AdminLicNo1;

        private GroupBox groupBox21;

        private TextBox kbSelect17;

        private TextBox kbSelect16;

        private TextBox kbSelect15;

        private TextBox kbSelect14;

        private TextBox kbSelect13;

        private TextBox kbSelect12;

        private TextBox kbSelect11;

        private TextBox kbSelect10;

        private Panel panel34;

        private TextBox kbSelect07;

        private TextBox kbSelect06;

        private TextBox kbSelect05;

        private TextBox kbSelect04;

        private TextBox kbSelect03;

        private TextBox kbSelect02;

        private TextBox kbSelect01;

        private TextBox kbSelect00;

        private Panel panel35;

        private TextBox kbAction7;

        private TextBox kbAction6;

        private TextBox kbAction5;

        private TextBox kbAction4;

        private TextBox kbAction3;

        private TextBox kbAction2;

        private TextBox kbAction1;

        private Label label39;

        private TextBox kbAction0;

        private RadioButton rbKeyBoardOpt1;

        private RadioButton rbKeyBoardOpt0;

        private FolderBrowserDialog folderBrowserDialog1;

        private ColorDialog ColorDialog1;

        private Button SongFolder_Rename;

        private ImageList imageListSys;

        private GroupBox groupBox7;

        private CheckBox cbPreviewShowNotations;

        private NumericUpDown PreviewFontUpDown;

        private Label label37;

        private ToolStripButton Message_Flash;

        private ToolStripButton Message_Scroll;

        private ToolStripButton Parental_Flash;

        private ToolStripButton Parental_Scroll;

        private GroupBox groupBox8;

        private TextBox tbPick;

        private Label label24;

        private Panel panel10;

        private Button btnReferenceChangeBackColour;

        private Button btnReferenceChangeTextColour;

        private Panel panel21;

        private ToolStrip toolBarReferenceFormat;

        private ToolStripButton Reference_Flash;

        private ToolStripButton Reference_Scroll;

        private ToolStripDropDownButton Reference_Align;

        private ToolStripMenuItem Reference_AlignLeft;

        private ToolStripMenuItem Reference_AlignCentre;

        private ToolStripMenuItem Reference_AlignRight;

        private ToolStripButton Reference_Bold;

        private ToolStripButton Reference_Italics;

        private ToolStripButton Reference_Underline;

        private ToolStripButton Reference_Shadow;

        private ToolStripButton Reference_Outline;

        private Panel panel22;

        private Label label38;

        private ToolStrip toolStrip5;

        private ToolStripComboBox ReferenceComboFont;

        private Panel panel33;

        private NumericUpDown ReferenceSizeUpDown;

        private Label label40;

        private Panel panel36;

        private NumericUpDown ReferenceAlertDurationUpDown;

        private Label label41;

        private RadioButton Reference_Source3;

        private RadioButton Reference_Source0;

        private RadioButton Reference_Source4;

        private GroupBox groupBox11;

        private GroupBox groupBox9;

        private TextBox tbSubstitute;

        private Label label42;

        private CheckBox cbPick;

        private CheckBox cbPickBlank;

        private RadioButton Reference_Source2;

        private RadioButton Reference_Source1;

        private ToolStripDropDownButton ToolBarFont_R1Italics;

        private ToolStripMenuItem ToolBarFont_R1Italics0;

        private ToolStripMenuItem ToolBarFont_R1Italics1;

        private ToolStripMenuItem ToolBarFont_R1Italics2;

        private ToolStripDropDownButton ToolBarFont_R2Italics;

        private ToolStripMenuItem ToolBarFont_R2Italics0;

        private ToolStripMenuItem ToolBarFont_R2Italics1;

        private ToolStripMenuItem ToolBarFont_R2Italics2;

        private ToolStripDropDownButton HeadingsFont_Italics;

        private ToolStripMenuItem HeadingsFont_Italics0;

        private ToolStripMenuItem HeadingsFont_Italics1;

        private ToolStripMenuItem HeadingsFont_Italics2;

        private Panel SamplePanel_Right;

        private Panel SamplePanel_Left;

        private Panel SamplePanel_Top;

        private Panel panel38;

        private Panel panel41;

        private Panel panel40;

        private Panel panel37;

        private Panel SamplePanel_Region1;

        private Panel panel42;

        private Panel SamplePanel_Region2;

        private Label labelPreviewCentreTop;

        private Label labelPreviewCentreBottom;

        private RadioButton optDM1;

        private RadioButton optDM0;

        private NumericUpDown DM1UpDownTop;

        private Label label44;

        private NumericUpDown DM1UpDownWidth;

        private Label label43;

        private Label label45;

        private NumericUpDown DM1UpDownLeft;

        private NumericUpDown DM1UpDownHeight;

        private Label label46;

        private Panel panel18;

        private ToolStrip toolStrip3;

        private ToolStripComboBox tbLyricsHeading3;

        private Label label47;

        private RadioButton rbGapItemOption3;

        private Panel panel39;

        private ToolStrip toolStrip10;

        private ToolStripButton GapLogoLocationBtn;

        private TextBox tbGapLogoLocation;

        private OpenFileDialog OpenFileDialog1;

        private ToolStripButton Message_Transparent;

        private ToolStripButton Parental_Transparent;

        private ToolStripButton Reference_Transparent;

        private CheckBox cbDisableScreenSaver;

        private TabPage tabPageMonitors;

        private GroupBox groupBoxLM;

        private NumericUpDown LM1UpDownHeight;

        private NumericUpDown LM1UpDownLeft;

        private Label label23;

        private NumericUpDown LM1UpDownWidth;

        private NumericUpDown LM1UpDownTop;

        private RadioButton optLM1;

        private RadioButton optLM0;

        private Panel panel43;

        private ToolStrip toolStrip12;

        private ToolStripComboBox LyricsMonitorList;

        private Panel panel44;

        private ToolStrip toolStrip13;

        private ToolStripButton LyricsMonitor_Info;

        private Label label25;

        private Label label26;

        private Label label48;

        private NumericUpDown LMUpDownFontSize;

        private CheckBox cbLMShowNotations;

        private Label label49;

        private Button btnLMBackColour;

        private Button btnLMTextColour;

        private CheckBox LM_AlwaysUse;

        private GroupBox groupBox5;

        private Button btnLMHighlightColour;

        private CheckBox checkBoxPPTab;

        private NumericUpDown ShowLineSpacing2MaxUpDown;

        private GroupBox groupBox2;

        private Panel panelVideoHolder;

        private Panel panelVideoSize;

        private Label label50;

        private NumericUpDown VideoSizeUpDown1;

        private Label label14;

        private CheckBox checkBoxMediaTab;

        private Panel panel45;

        private ToolStrip toolStripVideo;

        private ToolStripDropDownButton Video_VAlign;

        private ToolStripMenuItem Video_VAlignTop;

        private ToolStripMenuItem Video_VAlignCentre;

        private ToolStripMenuItem Video_VAlignBottom;

        private Label label51;

        private ToolStripDropDownButton Message_VAlign;

        private ToolStripMenuItem Message_VAlignTop;

        private ToolStripMenuItem Message_VAlignBottom;

        private ToolStripDropDownButton Parental_VAlign;

        private ToolStripMenuItem Parental_VAlignTop;

        private ToolStripMenuItem Parental_VAlignBottom;

        private GroupBox groupBox22;

        private CheckBox checkBoxPPNoPanel;

        private CheckBox cbGapItemUseFade;

        private GroupBox groupBox12;

        private Panel panelJump;

        private ToolStrip toolStripJump;

        private ToolStripComboBox toolStripJumpA;

        private ToolStripComboBox toolStripJumpB;

        private ToolStripComboBox toolStripJumpC;

        private CheckBox checkBoxMediaNoPanel;

        private GroupBox groupBox23;

        private CheckBox cbMute;

        private CheckBox cbWidescreen;

        private Label label5;

        private Label label7;

        private Label label52;

        private TrackBar TrackBarBalance;

        private TrackBar TrackBarVolume;

        private Label label53;

        private CheckBox cbLineBetweenRegions;

        private CheckBox cbLineBetweenScreens;

        private Button btnTextRegionSlideTextColour;

        private Button btnTextRegionSlideBackColour;

        private TabPage tabPageShow;

        private Panel panel46;

        private NumericUpDown NotationFontFactorUpDown;

        private Label label54;

        private Panel panel47;

        private ToolStrip toolStripCaptureDevices;

        private ToolStripComboBox cbCaptureDevices;

        private CheckBox checkBoxLiveCamNoPanel;

        private CheckBox cbEnforceDisplay;

        private CheckBox cbWordWrapLeftAlignIndent;

        private ToolStripButton ToolBarFont_R1RTL;

        private ToolStripButton ToolBarFont_R2RTL;

        private NumericUpDown LMNotationsUpDownFontSize;

        private Panel panel48;

        private ToolStrip toolStripLyricsMonitor;

        private ToolStripButton LM_Bold;

        private ToolStripButton LM_Italic;

        private ToolStripButton LM_Underline;

        private Label label9;

        private ToolStripSeparator toolStripSeparator1;

        private ToolStripSeparator toolStripSeparator2;

        private Label label56;

        private Label label55;

        private Label label10;

        private GroupBox groupBox24;

        private CheckBox checkBoxLMBox;

        private CheckBox DM_CustomAsSingleMonitor;

        private ToolStripDropDownButton Reference_VAlign;

        private ToolStripMenuItem Reference_VAlignTop;

        private ToolStripMenuItem Reference_VAlignCentre;

        private ToolStripMenuItem Reference_VAlignBottom;

        private Label label58;
        private RadioButton optWide;
        private RadioButton optStandard;
        private Panel panel49;
        private Label label57;
        private CheckBox ChkGlobalHookF10;
        private CheckBox ChkGlobalHookF9;
        private CheckBox ChkGlobalHookCtrlArrow;
        private CheckBox ChkGlobalHookArrow;
        private Label label60;
        private Label label59;
        private CheckBox ChkGlobalHookF8;
        private CheckBox ChkGlobalHookF7;
        private CheckBox cbLMBroadcast;

        public FrmOptions()
        {
            InitializeComponent();
        }

        private void FrmOptions_Load(object sender, EventArgs e)
        {
            DirOK = true;
            for (int i = 1; i < 41; i++)
            {
                tempFolderName[i] = gf.FolderName[i];
                tempFolderGroupStyle[i] = (int)gf.FolderGroupStyle[i];
                tempFolderUse[i] = ((gf.FolderUse[i] > 0) ? true : false);
                tempLeftMargin[i] = gf.ShowLeftMargin[i];
                tempRightMargin[i] = gf.ShowRightMargin[i];
                tempBottomMargin[i] = gf.ShowBottomMargin[i];
                tempFolderHeadingPercentSize[i] = gf.FolderHeadingPercentSize[i];
                tempFolderHeadingOption[i] = gf.FolderHeadingOption[i];
                tempFolderHeadingFontBold[i, 0] = ((gf.FolderHeadingFontBold[i, 0] > 0) ? true : false);
                tempFolderHeadingFontItalic[i, 0] = ((gf.FolderHeadingFontItalic[i, 0] > 0) ? true : false);
                tempFolderHeadingFontUnderline[i, 0] = ((gf.FolderHeadingFontUnderline[i, 0] > 0) ? true : false);
                tempFolderHeadingFontBold[i, 1] = ((gf.FolderHeadingFontBold[i, 1] > 0) ? true : false);
                tempFolderHeadingFontItalic[i, 1] = ((gf.FolderHeadingFontItalic[i, 1] > 0) ? true : false);
                tempFolderHeadingFontUnderline[i, 1] = ((gf.FolderHeadingFontUnderline[i, 1] > 0) ? true : false);
                tempShowLineSpacing[i, 0] = gf.ShowLineSpacing[i, 0];
                tempShowLineSpacing[i, 1] = gf.ShowLineSpacing[i, 1];
                for (int j = 0; j < 4; j++)
                {
                    tempFolderLyricsHeading[i, j] = gf.FolderLyricsHeading[i, j];
                }
                for (int j = 0; j <= 1; j++)
                {
                    tempShowFontSize[i, j] = gf.ShowFontSize[i, j];
                    tempShowFontBold[i, j] = ((gf.ShowFontBold[i, j] > 0) ? true : false);
                    tempShowFontItalic[i, j] = ((gf.ShowFontItalic[i, j] > 0) ? true : false);
                    tempShowFontUnderline[i, j] = ((gf.ShowFontUnderline[i, j] > 0) ? true : false);
                    tempShowFontRTL[i, j] = ((gf.ShowFontRTL[i, j] > 0) ? true : false);
                    tempShowFontBold[i, j + 2] = ((gf.ShowFontBold[i, j + 2] > 0) ? true : false);
                    tempShowFontItalic[i, j + 2] = ((gf.ShowFontItalic[i, j + 2] > 0) ? true : false);
                    tempShowFontUnderline[i, j + 2] = ((gf.ShowFontUnderline[i, j + 2] > 0) ? true : false);
                    tempShowFontName[i, j] = gf.ShowFontName[i, j];
                    tempShowFontVPosition[i, j] = gf.ShowFontVPosition[i, j];
                }
                tempShowFontVPositionMax[i, 0] = gf.ShowFontVPosition[i, 1];
                tempShowFontVPositionMin[i, 1] = gf.ShowFontVPosition[i, 0];
            }
            char c = '副';
            char c2 = '歌';
            char c3 = '合';
            char c4 = '唱';
            char c5 = '중';
            char c6 = '창';
            tbLyricsHeading0.Items.Clear();
            tbLyricsHeading0.Items.Add("");
            tbLyricsHeading0.Items.Add("PreChorus:");
            tbLyricsHeading0.Items.Add(c + c2 + ":");
            tbLyricsHeading0.Items.Add(c3 + c4 + ":");
            tbLyricsHeading0.Items.Add(c5 + c6 + ":");
            tbLyricsHeading1.Items.Clear();
            tbLyricsHeading1.Items.Add("");
            tbLyricsHeading1.Items.Add("Chorus:");
            tbLyricsHeading1.Items.Add(c + c2 + ":");
            tbLyricsHeading1.Items.Add(c3 + c4 + ":");
            tbLyricsHeading1.Items.Add(c5 + c6 + ":");
            tbLyricsHeading2.Items.Clear();
            tbLyricsHeading2.Items.Add("");
            tbLyricsHeading2.Items.Add("Bridge:");
            tbLyricsHeading3.Items.Clear();
            tbLyricsHeading3.Items.Add("");
            tbLyricsHeading3.Items.Add("End...");
            gf.BuildFontsList(ref ComboFontName0);
            gf.BuildFontsList(ref ComboFontName1);
            gf.BuildFontsList(ref MessageComboFont);
            gf.BuildFontsList(ref ParentalComboFont);
            gf.BuildFontsList(ref ReferenceComboFont);
            CurFolder = gf.CurMainSelectedFolder;
            LoadGeneralSetting();
            BuildFolderList();
            SongFolder.Items[CurFolder - 1].Selected = true;
            SongFolderIndexChanged();
            Apply_FolderUse();
            SetJumpFolders();
            ApplySettings();
            BuildBibleAssociatedFolder();
            BuildBibleList();
            BibleListIndexChanged();
            BuildLicencesList();
            if (gf.Options_SelectedTabIndex <= tabControl1.TabCount)
            {
                tabControl1.SelectedIndex = gf.Options_SelectedTabIndex;
            }
            if (gf.ShowRunning)
            {
                DualMonitorList.Enabled = false;
                DM_AlwaysUseSecondaryMonitor.Enabled = false;
                LyricsMonitorList.Enabled = false;
                LM_AlwaysUse.Enabled = false;
            }
            KyTextBoxesColourBackground();
            UpdateOptFields(1);
            UpdateOptFields(2);
            InitFormLoad = false;
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            SaveVariables();
            if (gf.Options_BibleListChanged)
            {
                SaveBibleChanges();
            }
            //SaveLicenceChanges();
            gf.Options_SelectedTabIndex = tabControl1.SelectedIndex;
            base.DialogResult = DialogResult.OK;
            Close();
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            gf.Options_SelectedTabIndex = tabControl1.SelectedIndex;
            base.DialogResult = DialogResult.Cancel;
            Close();
        }

        private void SaveVariables()
        {
            for (int i = 1; i < 41; i++)
            {
                if ((gf.FolderName[i] != tempFolderName[i]) | (gf.FolderGroupStyle[i] != (SortBy)tempFolderGroupStyle[i]) | (gf.FolderUse[i] != (tempFolderUse[i] ? 1 : 0)))
                {
                    gf.Options_FolderListChanged = true;
                }
                gf.FolderName[i] = tempFolderName[i];
                gf.FolderGroupStyle[i] = (SortBy)tempFolderGroupStyle[i];
                gf.FolderUse[i] = (tempFolderUse[i] ? 1 : 0);
                if (gf.ShowLeftMargin[i] != tempLeftMargin[i] || gf.ShowRightMargin[i] != tempRightMargin[i] || gf.ShowBottomMargin[i] != tempBottomMargin[i] || gf.ShowLineSpacing[i, 0] != tempShowLineSpacing[i, 0] || gf.ShowLineSpacing[i, 1] != tempShowLineSpacing[i, 1] || gf.FolderHeadingPercentSize[i] != tempFolderHeadingPercentSize[i] || gf.FolderHeadingOption[i] != tempFolderHeadingOption[i] || gf.FolderHeadingFontBold[i, 0] != (tempFolderHeadingFontBold[i, 0] ? 1 : 0) || gf.FolderHeadingFontItalic[i, 0] != (tempFolderHeadingFontItalic[i, 0] ? 1 : 0) || gf.FolderHeadingFontUnderline[i, 0] != (tempFolderHeadingFontUnderline[i, 0] ? 1 : 0) || gf.FolderHeadingFontBold[i, 1] != (tempFolderHeadingFontBold[i, 1] ? 1 : 0) || gf.FolderHeadingFontItalic[i, 1] != (tempFolderHeadingFontItalic[i, 1] ? 1 : 0) || gf.FolderHeadingFontUnderline[i, 1] != (tempFolderHeadingFontUnderline[i, 1] ? 1 : 0))
                {
                    gf.ShowLeftMargin[i] = tempLeftMargin[i];
                    gf.ShowRightMargin[i] = tempRightMargin[i];
                    gf.ShowBottomMargin[i] = tempBottomMargin[i];
                    gf.ShowLineSpacing[i, 0] = tempShowLineSpacing[i, 0];
                    gf.ShowLineSpacing[i, 1] = tempShowLineSpacing[i, 1];
                    gf.FolderHeadingPercentSize[i] = tempFolderHeadingPercentSize[i];
                    gf.FolderHeadingOption[i] = tempFolderHeadingOption[i];
                    gf.FolderHeadingFontBold[i, 0] = (tempFolderHeadingFontBold[i, 0] ? 1 : 0);
                    gf.FolderHeadingFontItalic[i, 0] = (tempFolderHeadingFontItalic[i, 0] ? 1 : 0);
                    gf.FolderHeadingFontUnderline[i, 0] = (tempFolderHeadingFontUnderline[i, 0] ? 1 : 0);
                    gf.FolderHeadingFontBold[i, 1] = (tempFolderHeadingFontBold[i, 1] ? 1 : 0);
                    gf.FolderHeadingFontItalic[i, 1] = (tempFolderHeadingFontItalic[i, 1] ? 1 : 0);
                    gf.FolderHeadingFontUnderline[i, 1] = (tempFolderHeadingFontUnderline[i, 1] ? 1 : 0);
                    gf.Options_FolderFormatChanged = true;
                }
                for (int j = 0; j < 4; j++)
                {
                    if (gf.FolderLyricsHeading[i, j] != tempFolderLyricsHeading[i, j])
                    {
                        gf.Options_FolderFormatChanged = true;
                    }
                    gf.FolderLyricsHeading[i, j] = tempFolderLyricsHeading[i, j];
                }
                for (int j = 0; j <= 1; j++)
                {
                    if (gf.ShowFontSize[i, j] != tempShowFontSize[i, j] || gf.ShowFontBold[i, j] != (tempShowFontBold[i, j] ? 1 : 0) || gf.ShowFontItalic[i, j] != (tempShowFontItalic[i, j] ? 1 : 0) || gf.ShowFontUnderline[i, j] != (tempShowFontUnderline[i, j] ? 1 : 0) || gf.ShowFontRTL[i, j] != (tempShowFontRTL[i, j] ? 1 : 0) || gf.ShowFontBold[i, j + 2] != (tempShowFontBold[i, j + 2] ? 1 : 0) || gf.ShowFontItalic[i, j + 2] != (tempShowFontItalic[i, j + 2] ? 1 : 0) || gf.ShowFontUnderline[i, j + 2] != (tempShowFontUnderline[i, j + 2] ? 1 : 0) || gf.ShowFontName[i, j] != tempShowFontName[i, j] || gf.ShowFontVPosition[i, j] != tempShowFontVPosition[i, j])
                    {
                        gf.Options_FolderFormatChanged = true;
                    }
                    gf.ShowFontSize[i, j] = tempShowFontSize[i, j];
                    gf.ShowFontBold[i, j] = (tempShowFontBold[i, j] ? 1 : 0);
                    gf.ShowFontItalic[i, j] = (tempShowFontItalic[i, j] ? 1 : 0);
                    gf.ShowFontUnderline[i, j] = (tempShowFontUnderline[i, j] ? 1 : 0);
                    gf.ShowFontRTL[i, j] = (tempShowFontRTL[i, j] ? 1 : 0);
                    gf.ShowFontBold[i, j + 2] = (tempShowFontBold[i, j + 2] ? 1 : 0);
                    gf.ShowFontItalic[i, j + 2] = (tempShowFontItalic[i, j + 2] ? 1 : 0);
                    gf.ShowFontUnderline[i, j + 2] = (tempShowFontUnderline[i, j + 2] ? 1 : 0);
                    gf.ShowFontName[i, j] = tempShowFontName[i, j];
                    gf.ShowFontVPosition[i, j] = tempShowFontVPosition[i, j];
                }
            }
            gf.ComputeShowLineSpacing();
            gf.UsePowerpointTab = checkBoxPPTab.Checked;
            gf.NoPowerpointPanelOverlay = checkBoxPPNoPanel.Checked;
            gf.UseMediaTab = checkBoxMediaTab.Checked;
            gf.NoMediaPanelOverlay = checkBoxMediaNoPanel.Checked;
            gf.ShowLyricsMonitorAlertBox = checkBoxLMBox.Checked;
            if (gf.UseLargestFontSize != cbUseLargestFont.Checked || gf.AutoTextOverflow != cbAutoTextOverflow.Checked || gf.LineBetweenRegions != cbLineBetweenRegions.Checked || gf.WordWrapLeftAlignIndent != cbWordWrapLeftAlignIndent.Checked)
            {
                gf.UseLargestFontSize = cbUseLargestFont.Checked;
                gf.AutoTextOverflow = cbAutoTextOverflow.Checked;
                gf.LineBetweenRegions = cbLineBetweenRegions.Checked;
                gf.WordWrapLeftAlignIndent = cbWordWrapLeftAlignIndent.Checked;
                gf.Options_FolderFormatChanged = true;
            }
            gf.AdvanceNextItem = cbAdvanceNextItem.Checked;
            if (rbGapItemOption1.Checked)
            {
                gf.GapItemOption = GapType.Black;
            }
            else if (rbGapItemOption2.Checked)
            {
                gf.GapItemOption = GapType.Default;
            }
            else if (rbGapItemOption3.Checked)
            {
                gf.GapItemOption = GapType.User;
            }
            else
            {
                gf.GapItemOption = GapType.None;
            }
            gf.AltGapItemOption = GapType.None;
            gf.GapItemLogoFile = DataUtil.Trim(tbGapLogoLocation.Text);
            gf.GapItemUseFade = cbGapItemUseFade.Checked;
            gf.NotationFontFactor = (float)NotationFontFactorUpDown.Value / 100f;
            gf.HB_MaxVersesSelection = (int)VersesMaxUpDown.Value;
            gf.HB_MaxAdhocVersesSelection = (int)AdhocVersesMaxUpDown.Value;
            gf.PP_MaxFiles = (int)PPMaxUpDown.Value;
            if (EditHistoryMaxUpDown.Value != (decimal)gf.MaxUserEditHistory)
            {
                gf.Options_MaxHistoryListChanged = true;
            }
            gf.MaxUserEditHistory = (int)EditHistoryMaxUpDown.Value;
            if (gf.MaxUserEditHistory > gf.AbsoluteMaxHitoryItems)
            {
                gf.MaxUserEditHistory = gf.AbsoluteMaxHitoryItems;
            }
            if (gf.PreviewArea_ShowNotations != cbPreviewShowNotations.Checked || gf.PreviewArea_FontSize != (int)PreviewFontUpDown.Value || gf.PreviewArea_LineBetweenScreens != cbLineBetweenScreens.Checked)
            {
                gf.Options_PreviewAreaChanged = true;
            }
            gf.PreviewArea_ShowNotations = cbPreviewShowNotations.Checked;
            gf.PreviewArea_LineBetweenScreens = cbLineBetweenScreens.Checked;
            gf.PreviewArea_FontSize = (int)PreviewFontUpDown.Value;
            //gf.OutputMonitorNumber = DualMonitorList.SelectedIndex;
            if (DualMonitorList.SelectedItem != null)
            {
                gf.OutputMonitorName = DualMonitorList.SelectedItem.ToString();
            }

            gf.DMAlwaysUseSecondaryMonitor = DM_AlwaysUseSecondaryMonitor.Checked;

            //if (gf.DMAlwaysUseSecondaryMonitor & (gf.OutputMonitorNumber == 0) & (DualMonitorList.Items.Count > 0))
            //{
            //    gf.OutputMonitorNumber = 1;
            //    gf.GetScreenNumber(ref gf.OutputMonitorNumber, -1);
            //}

            // UseSecondaryMonitor 모드 인 경우
            if (gf.DMAlwaysUseSecondaryMonitor & (DualMonitorList.Items.Count > 0))
            {
                //gf.OutputMonitorNumber = gf.GetSecondryMonitorIndex();

                gf.OutputMonitorName = gf.GetSecondryMonitorName();
            }


            // daniel 추가 2024년 04월 07일 
            // 다중 모니터일 경우 파워포인트 슬라이드 쇼의 디스플레이를 설정

            new OfficeVersion().SetPowerPointDisplayMonitor(gf.OutputMonitorName);

            if (gf.DualMonitorSelectAutoOption != ((!optDM0.Checked) ? 1 : 0) || (int)DM1UpDownTop.Value != gf.DMOption1Top || (int)DM1UpDownLeft.Value != gf.DMOption1Left || (int)DM1UpDownWidth.Value != gf.DMOption1Width || DM_CustomAsSingleMonitor.Checked != gf.DMOption1AsSingleMonitor)
            {
                gf.Options_DMChanged = true;
            }

            // daniel
            // 스크린 비율 설정 추가
            gf.isScreenWideMode = optWide.Checked;

            gf.DualMonitorSelectAutoOption = ((!optDM0.Checked) ? 1 : 0);

            gf.DMOption1Top = (int)DM1UpDownTop.Value;
            gf.DMOption1Left = (int)DM1UpDownLeft.Value;
            gf.DMOption1Width = (int)DM1UpDownWidth.Value;
            //gf.LMOption1Height = (int)LM1UpDownWidth.Value * 3 / 4;
            //if (gf.LMOption1Height < 1)
            //{
            //	gf.LMOption1Height = 1;
            //}
            //wide mode가 아닐 경우

            if (!gf.isScreenWideMode)
                gf.DMOption1Height = (int)DM1UpDownWidth.Value * 3 / 4;
            else
                gf.DMOption1Height = (int)DM1UpDownWidth.Value;

            gf.DMOption1AsSingleMonitor = DM_CustomAsSingleMonitor.Checked;

            //gf.LyricsMonitorNumber = LyricsMonitorList.SelectedIndex;

            if (LyricsMonitorList?.SelectedItem?.ToString() != null)
                gf.LyricsMonitorName = LyricsMonitorList.SelectedItem.ToString();

            gf.LMAlwaysUseSecondaryMonitor = LM_AlwaysUse.Checked;
            //if (gf.LMAlwaysUseSecondaryMonitor & (gf.LyricsMonitorNumber == 0) & (DualMonitorList.Items.Count > 0))
            //{
            //    gf.LyricsMonitorNumber = 1;
            //    gf.GetScreenNumber(ref gf.LyricsMonitorNumber, (gf.DualMonitorSelectAutoOption == 0) ? gf.OutputMonitorNumber : (-1));
            //}

            if (gf.LMAlwaysUseSecondaryMonitor & (DualMonitorList.Items.Count > 0))
            {
                gf.GetScreenName(ref gf.LyricsMonitorName, (gf.DualMonitorSelectAutoOption == 0) ? gf.OutputMonitorName : "None");
            }

            if (gf.LMSelectAutoOption != ((!optLM0.Checked) ? 1 : 0) || (int)LM1UpDownTop.Value != gf.LMOption1Top || (int)LM1UpDownLeft.Value != gf.LMOption1Left || (int)LM1UpDownWidth.Value != gf.LMOption1Width)
            {
                gf.Options_DMChanged = true;
            }
            gf.LMSelectAutoOption = ((!optLM0.Checked) ? 1 : 0);
            gf.LMOption1Top = (int)LM1UpDownTop.Value;
            gf.LMOption1Left = (int)LM1UpDownLeft.Value;
            gf.LMOption1Width = (int)LM1UpDownWidth.Value;
            //gf.LMOption1Height = (int)LM1UpDownWidth.Value * 3 / 4;
            //if (gf.LMOption1Height < 1)
            //{
            //	gf.LMOption1Height = 1;
            //}
            // Daniel Park 수정 2023년 12월 24일
            if (!gf.isScreenWideMode)
                gf.DMOption1Height = (int)DM1UpDownWidth.Value * 3 / 4;
            else
            {
                gf.DMOption1Height = (int)DM1UpDownHeight.Value * 5 / 3;
            }
            //gf.DMOption1Height = (int)DM1UpDownHeight.Value;


            gf.LMTextColour = btnLMTextColour.ForeColor;
            gf.LMHighlightColour = btnLMHighlightColour.ForeColor;
            gf.LMBackColour = btnLMBackColour.ForeColor;
            gf.LMShowNotations = cbLMShowNotations.Checked;
            gf.LMMainFontSize = (int)LMUpDownFontSize.Value;
            gf.LMNotationsFontSize = (int)LMNotationsUpDownFontSize.Value;
            gf.LMFontBold = LM_Bold.Checked;
            gf.LMFontItalic = LM_Italic.Checked;
            gf.LMFontUnderline = LM_Underline.Checked;
            gf.LMFontFormat = (gf.LMFontBold ? 1 : 0) + (gf.LMFontItalic ? 1 : 0) * 2 + (gf.LMFontUnderline ? 1 : 0) * 4;
            gf.DisableSreenSaver = cbDisableScreenSaver.Checked;
            gf.VideoSize = (int)VideoSizeUpDown1.Value;
            gf.VideoVAlign = DataUtil.ObjToInt(Video_VAlign.Tag);
            if (gf.FocusedTextRegionColour != btnTextRegionChangeColour.ForeColor || gf.TextRegionSlideTextColour != btnTextRegionSlideTextColour.ForeColor || gf.TextRegionSlideBackColour != btnTextRegionSlideBackColour.ForeColor || gf.UseFocusedTextRegionColour != TextRegionUseColour.Checked)
            {
                gf.Options_PreviewAreaChanged = true;
            }
            gf.FocusedTextRegionColour = btnTextRegionChangeColour.ForeColor;
            gf.TextRegionSlideTextColour = btnTextRegionSlideTextColour.ForeColor;
            gf.TextRegionSlideBackColour = btnTextRegionSlideBackColour.ForeColor;
            gf.UseFocusedTextRegionColour = TextRegionUseColour.Checked;
            gf.AutoFocusTextRegion = false;
            gf.JumpToA = toolStripJumpA.SelectedIndex + 1;
            gf.JumpToB = toolStripJumpB.SelectedIndex + 1;
            gf.JumpToC = toolStripJumpC.SelectedIndex + 1;
            gf.LiveCamNumber = cbCaptureDevices.SelectedIndex + 1;
            gf.LiveCamVolume = TrackBarVolume.Value;
            gf.LiveCamBalance = TrackBarBalance.Value;
            gf.LiveCamMute = cbMute.Checked;
            gf.LiveCamWidescreen = cbWidescreen.Checked;
            gf.LiveCamNoPanelOverlay = checkBoxLiveCamNoPanel.Checked;
            gf.ParentalAlertDuration = (int)ParentalAlertUpDown.Value;
            gf.ParentalAlertScroll = Parental_Scroll.Checked;
            gf.ParentalAlertFlash = Parental_Flash.Checked;
            gf.ParentalAlertTransparent = Parental_Transparent.Checked;
            gf.ParentalAlertHeading = ParentalAlert.Text;
            gf.ParentalAlertFontName = ((ParentalComboFont.Text != "") ? ParentalComboFont.Text : "Microsoft Sans Serif");
            gf.ParentalAlertFontSize = (int)ParentalSizeUpDown.Value;
            gf.ParentalAlertBold = Parental_Bold.Checked;
            gf.ParentalAlertItalic = Parental_Italics.Checked;
            gf.ParentalAlertUnderline = Parental_Underline.Checked;
            gf.ParentalAlertShadow = Parental_Shadow.Checked;
            gf.ParentalAlertOutline = Parental_Outline.Checked;
            gf.ParentalAlertFontFormat = (gf.ParentalAlertBold ? 1 : 0) + (gf.ParentalAlertItalic ? 1 : 0) * 2 + (gf.ParentalAlertUnderline ? 1 : 0) * 4 + (gf.ParentalAlertShadow ? 1 : 0) * 8 + (gf.ParentalAlertOutline ? 1 : 0) * 16;
            gf.ParentalAlertTextColour = btnParentalChangeTextColour.ForeColor;
            gf.ParentalAlertBackColour = btnParentalChangeBackColour.ForeColor;
            gf.ParentalAlertTextAlign = DataUtil.ObjToInt(Parental_Align.Tag);
            gf.ParentalAlertVerticalAlign = DataUtil.ObjToInt(Parental_VAlign.Tag);
            gf.MessageAlertDuration = (int)MessageAlertDurationUpDown.Value;
            gf.MessageAlertScroll = Message_Scroll.Checked;
            gf.MessageAlertFlash = Message_Flash.Checked;
            gf.MessageAlertTransparent = Message_Transparent.Checked;
            gf.MessageAlertFontName = ((MessageComboFont.Text != "") ? MessageComboFont.Text : "Microsoft Sans Serif");
            gf.MessageAlertFontSize = (int)MessageSizeUpDown.Value;
            gf.MessageAlertBold = Message_Bold.Checked;
            gf.MessageAlertItalic = Message_Italics.Checked;
            gf.MessageAlertUnderline = Message_Underline.Checked;
            gf.MessageAlertShadow = Message_Shadow.Checked;
            gf.MessageAlertOutline = Message_Outline.Checked;
            gf.MessageAlertFontFormat = (gf.MessageAlertBold ? 1 : 0) + (gf.MessageAlertItalic ? 1 : 0) * 2 + (gf.MessageAlertUnderline ? 1 : 0) * 4 + (gf.MessageAlertShadow ? 1 : 0) * 8 + (gf.MessageAlertOutline ? 1 : 0) * 16;
            gf.MessageAlertTextColour = btnMessageChangeTextColour.ForeColor;
            gf.MessageAlertBackColour = btnMessageChangeBackColour.ForeColor;
            gf.MessageAlertTextAlign = DataUtil.ObjToInt(Message_Align.Tag);
            gf.MessageAlertVerticalAlign = DataUtil.ObjToInt(Message_VAlign.Tag);
            gf.ReferenceAlertDuration = (int)ReferenceAlertDurationUpDown.Value;
            gf.ReferenceAlertScroll = Reference_Scroll.Checked;
            gf.ReferenceAlertFlash = Reference_Flash.Checked;
            gf.ReferenceAlertTransparent = Reference_Transparent.Checked;
            gf.ReferenceAlertFontName = ((ReferenceComboFont.Text != "") ? ReferenceComboFont.Text : "Microsoft Sans Serif");
            gf.ReferenceAlertFontSize = (int)ReferenceSizeUpDown.Value;
            gf.ReferenceAlertBold = Reference_Bold.Checked;
            gf.ReferenceAlertItalic = Reference_Italics.Checked;
            gf.ReferenceAlertUnderline = Reference_Underline.Checked;
            gf.ReferenceAlertShadow = Reference_Shadow.Checked;
            gf.ReferenceAlertOutline = Reference_Outline.Checked;
            gf.ReferenceAlertFontFormat = (gf.ReferenceAlertBold ? 1 : 0) + (gf.ReferenceAlertItalic ? 1 : 0) * 2 + (gf.ReferenceAlertUnderline ? 1 : 0) * 4 + (gf.ReferenceAlertShadow ? 1 : 0) * 8 + (gf.ReferenceAlertOutline ? 1 : 0) * 16;
            gf.ReferenceAlertTextColour = btnReferenceChangeTextColour.ForeColor;
            gf.ReferenceAlertBackColour = btnReferenceChangeBackColour.ForeColor;
            gf.ReferenceAlertTextAlign = DataUtil.ObjToInt(Reference_Align.Tag);
            gf.ReferenceAlertVerticalAlign = DataUtil.ObjToInt(Reference_VAlign.Tag);
            gf.ReferenceAlertUsePick = cbPick.Checked;
            gf.ReferenceAlertBlankIfPickNotFound = cbPickBlank.Checked;
            gf.ReferenceAlertPickName = tbPick.Text;
            gf.ReferenceAlertPickSubstitute = tbSubstitute.Text;
            if (Reference_Source1.Checked)
            {
                gf.ReferenceAlertSource = 1;
            }
            else if (Reference_Source2.Checked)
            {
                gf.ReferenceAlertSource = 2;
            }
            else if (Reference_Source3.Checked)
            {
                gf.ReferenceAlertSource = 3;
            }
            else if (Reference_Source4.Checked)
            {
                gf.ReferenceAlertSource = 4;
            }
            else
            {
                gf.ReferenceAlertSource = 0;
            }
            if (gf.MediaDir != tbMusicLocation.Text)
            {
                gf.MediaDir = tbMusicLocation.Text;
                gf.Options_MediaDirChanged = true;
            }
            gf.KeyBoardOption = ((!rbKeyBoardOpt0.Checked) ? 1 : 0);

            //daniel
            //Global Keyboard F7, F8
            gf.GlobalHookKey_F7 = ChkGlobalHookF7.Checked;
            gf.GlobalHookKey_F8 = ChkGlobalHookF8.Checked;

            //daniel
            //Global Keyboard F9, F10
            gf.GlobalHookKey_F9 = ChkGlobalHookF9.Checked;
            gf.GlobalHookKey_F10 = ChkGlobalHookF10.Checked;

            gf.GlobalHookKey_Arrow = ChkGlobalHookArrow.Checked;
            gf.GlobalHookKey_CtrlArrow = ChkGlobalHookCtrlArrow.Checked;

            if (gf.GlobalHookKey_F7 || gf.GlobalHookKey_F8 || gf.GlobalHookKey_F9 || gf.GlobalHookKey_F10)
            {
                FrmMain.frmMain.RemoveHookBlackScreen();
                FrmMain.frmMain.AddHookBlackScreen();
            }
            else
            {
                FrmMain.frmMain.RemoveHookBlackScreen();
            }

            if (gf.GlobalHookKey_Arrow || gf.GlobalHookKey_CtrlArrow)
            {
                FrmMain.frmMain.RemoveHookSlideUpDown();
                FrmMain.frmMain.AddHookSlideUpDown();
            }
            else
            {
                FrmMain.frmMain.RemoveHookSlideUpDown();
            }

            gf.SaveConfigSettings();
            gf.SizeLaunchScreen();
        }

        private void BuildFolderList()
        {
            ListViewItem listViewItem = new ListViewItem();
            SongFolder.Items.Clear();
            toolStripJumpA.Items.Clear();
            toolStripJumpB.Items.Clear();
            toolStripJumpC.Items.Clear();
            for (int i = 1; i < 41; i++)
            {
                listViewItem = SongFolder.Items.Add(tempFolderName[i]);
                if (tempFolderUse[i])
                {
                    listViewItem.ImageIndex = 25;
                }
                else
                {
                    listViewItem.ImageIndex = 26;
                }
                toolStripJumpA.Items.Add(tempFolderName[i]);
                toolStripJumpB.Items.Add(tempFolderName[i]);
                toolStripJumpC.Items.Add(tempFolderName[i]);
            }
        }

        private void SetJumpFolders()
        {
            toolStripJumpA.SelectedIndex = 1;
            toolStripJumpB.SelectedIndex = 1;
            toolStripJumpC.SelectedIndex = 1;
            for (int i = 1; i < 41; i++)
            {
                if (gf.JumpToA == i)
                {
                    toolStripJumpA.SelectedIndex = i - 1;
                }
                if (gf.JumpToB == i)
                {
                    toolStripJumpB.SelectedIndex = i - 1;
                }
                if (gf.JumpToC == i)
                {
                    toolStripJumpC.SelectedIndex = i - 1;
                }
            }
        }

        private void SongFolder_SelectedIndexChanged(object sender, EventArgs e)
        {
            SongFolderIndexChanged();
        }

        private void SongFolderIndexChanged()
        {
            CurFolder = gf.GetSelectedIndex(SongFolder) + 1;
            cbFolderUse.Checked = tempFolderUse[CurFolder];
            ApplySettings();
        }

        private void Apply_FolderUse()
        {
            tempFolderUse[CurFolder] = cbFolderUse.Checked;
            ApplySettings();
        }

        private void ApplySettings()
        {
            if (CurFolder >= 1)
            {
                InitFormLoad = true;
                UpdateFolderDetailsGroupBoxName();
                UpdateRegionsPosFields();
                UpdateHeadingFormatting();
                UpdateFontsFields();
                UpdateFontDisplay();
                SongFolder.Items[CurFolder - 1].ImageIndex = (tempFolderUse[CurFolder] ? 25 : 26);
                ShowLineSpacingMaxUpDown.Value = (decimal)(((tempShowLineSpacing[CurFolder, 0] >= 0.5) & (tempShowLineSpacing[CurFolder, 0] <= 3.0)) ? tempShowLineSpacing[CurFolder, 0] : 1.0);
                ShowLineSpacing2MaxUpDown.Value = (decimal)(((tempShowLineSpacing[CurFolder, 1] >= 0.5) & (tempShowLineSpacing[CurFolder, 1] <= 3.0)) ? tempShowLineSpacing[CurFolder, 1] : 1.0);
                tbLyricsHeading0.Text = tempFolderLyricsHeading[CurFolder, 0];
                tbLyricsHeading1.Text = tempFolderLyricsHeading[CurFolder, 1];
                tbLyricsHeading2.Text = tempFolderLyricsHeading[CurFolder, 2];
                tbLyricsHeading3.Text = tempFolderLyricsHeading[CurFolder, 3];
                bool @checked = cbFolderUse.Checked;
                SelectedFolderGroupBox.Enabled = @checked;
                GroupBoxFont0.Enabled = @checked;
                GroupBoxFont1.Enabled = @checked;
                GroupBoxHeadings.Enabled = @checked;
                InitFormLoad = false;
            }
        }

        private void UpdateFolderDetailsGroupBoxName()
        {
            if (CurFolder > 0)
            {
                SelectedFolderGroupBox.Text = "Settings for the " + tempFolderName[CurFolder] + " Folder";
            }
            else
            {
                SelectedFolderGroupBox.Text = "Settings for the above selected folder Folder";
            }
        }

        private void UpdateRegionsPosFields()
        {
            try
            {
                LoadTempPos = true;
                gf.UpdatePosUpDowns(ref FontPositionUpDown0, ref FontPositionUpDown1, ref FontPositionUpDownBottom, ref tempShowFontVPosition[CurFolder, 0], ref tempShowFontVPosition[CurFolder, 1], tempBottomMargin[CurFolder]);
                LeftMarginUpDown.Value = tempLeftMargin[CurFolder];
                RightMarginUpDown.Value = tempRightMargin[CurFolder];
                SamplePanel_Top.Height = (int)((float)FontPositionUpDown0.Value * SampleSplitterVerticalIncrement);
                SamplePanel_Region1.Height = (int)(((float)FontPositionUpDown1.Value - (float)FontPositionUpDown0.Value) * SampleSplitterVerticalIncrement);
                SamplePanel_Region2.Height = (int)(((float)FontPositionUpDown1.Maximum - (float)FontPositionUpDown1.Value) * SampleSplitterVerticalIncrement);
                SamplePanel_Left.Width = (int)((float)LeftMarginUpDown.Value * SampleSplitterHorizontalIncrement);
                SamplePanel_Right.Width = (int)((float)RightMarginUpDown.Value * SampleSplitterHorizontalIncrement);
                LoadTempPos = false;
            }
            catch
            {
            }
        }

        private void UpdateHeadingFormatting()
        {
            ShowHeadingsPercentSizeUpDown.Value = (((tempFolderHeadingPercentSize[CurFolder] >= 0) & (tempFolderHeadingPercentSize[CurFolder] <= 150)) ? tempFolderHeadingPercentSize[CurFolder] : 100);
            if (tempFolderHeadingOption[CurFolder] == 2)
            {
                ComboLyricsHeading.SelectedIndex = 0;
            }
            else if (tempFolderHeadingOption[CurFolder] == 1)
            {
                ComboLyricsHeading.SelectedIndex = 1;
            }
            else
            {
                ComboLyricsHeading.SelectedIndex = 2;
            }
            HeadingsFont_Bold.Checked = tempFolderHeadingFontBold[CurFolder, 0];
            string text = "";
            gf.AssignDropDownItem(SelectedMenuItemName: (tempFolderHeadingFontItalic[CurFolder, 0] && tempFolderHeadingFontItalic[CurFolder, 1]) ? HeadingsFont_Italics1.Name : ((!tempFolderHeadingFontItalic[CurFolder, 1]) ? HeadingsFont_Italics0.Name : HeadingsFont_Italics2.Name), SelectedBtn: ref HeadingsFont_Italics, InMenuItem1: HeadingsFont_Italics0, InMenuItem2: HeadingsFont_Italics1, InMenuItem3: HeadingsFont_Italics2);
            HeadingsFont_Underline.Checked = tempFolderHeadingFontUnderline[CurFolder, 0];
        }

        private void UpdateFontsFields()
        {
            ComboFontName0.Text = tempShowFontName[CurFolder, 0];
            ComboFontName1.Text = tempShowFontName[CurFolder, 1];
            FontSizeUpDown0.Value = tempShowFontSize[CurFolder, 0];
            FontSizeUpDown1.Value = tempShowFontSize[CurFolder, 1];
            ToolBarFont_R1Bold.Checked = tempShowFontBold[CurFolder, 0];
            string text = "";
            gf.AssignDropDownItem(SelectedMenuItemName: (tempShowFontItalic[CurFolder, 0] && tempShowFontItalic[CurFolder, 2]) ? ToolBarFont_R1Italics1.Name : ((!tempShowFontItalic[CurFolder, 2]) ? ToolBarFont_R1Italics0.Name : ToolBarFont_R1Italics2.Name), SelectedBtn: ref ToolBarFont_R1Italics, InMenuItem1: ToolBarFont_R1Italics0, InMenuItem2: ToolBarFont_R1Italics1, InMenuItem3: ToolBarFont_R1Italics2);
            ToolBarFont_R1Underline.Checked = tempShowFontUnderline[CurFolder, 0];
            ToolBarFont_R1RTL.Checked = tempShowFontRTL[CurFolder, 0];
            ToolBarFont_R2Bold.Checked = tempShowFontBold[CurFolder, 1];
            gf.AssignDropDownItem(SelectedMenuItemName: (tempShowFontItalic[CurFolder, 1] && tempShowFontItalic[CurFolder, 3]) ? ToolBarFont_R2Italics1.Name : ((!tempShowFontItalic[CurFolder, 3]) ? ToolBarFont_R2Italics0.Name : ToolBarFont_R2Italics2.Name), SelectedBtn: ref ToolBarFont_R2Italics, InMenuItem1: ToolBarFont_R2Italics0, InMenuItem2: ToolBarFont_R2Italics1, InMenuItem3: ToolBarFont_R2Italics2);
            ToolBarFont_R2Underline.Checked = tempShowFontUnderline[CurFolder, 1];
            ToolBarFont_R2RTL.Checked = tempShowFontRTL[CurFolder, 1];
            UpdateFontDisplay();
        }

        private void HeadingsFont_Click(object sender, EventArgs e)
        {
            tempFolderHeadingFontBold[CurFolder, 0] = HeadingsFont_Bold.Checked;
            tempFolderHeadingFontUnderline[CurFolder, 0] = HeadingsFont_Underline.Checked;
        }

        private void HeadingsFont_Italics_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            gf.AssignDropDownItem(ref HeadingsFont_Italics, e.ClickedItem.Name, HeadingsFont_Italics0, HeadingsFont_Italics1, HeadingsFont_Italics2);
            int num = DataUtil.ObjToInt(HeadingsFont_Italics.Tag);
            int num2 = num;
            if (num2 == 2)
            {
                tempFolderHeadingFontItalic[CurFolder, 0] = false;
                tempFolderHeadingFontItalic[CurFolder, 1] = true;
            }
            else
            {
                tempFolderHeadingFontItalic[CurFolder, 0] = ((num > 0) ? true : false);
                tempFolderHeadingFontItalic[CurFolder, 1] = ((num > 0) ? true : false);
            }
        }

        private void ComboFontName0_SelectedIndexChanged(object sender, EventArgs e)
        {
            tempShowFontName[CurFolder, 0] = ComboFontName0.Text;
            UpdateFontDisplay();
        }

        private void ComboFontName1_SelectedIndexChanged(object sender, EventArgs e)
        {
            tempShowFontName[CurFolder, 1] = ComboFontName1.Text;
            UpdateFontDisplay();
        }

        private void tbLyricsHeading_TextChanged(object sender, EventArgs e)
        {
            if (!InitFormLoad)
            {
                tempFolderLyricsHeading[CurFolder, 0] = tbLyricsHeading0.Text;
                tempFolderLyricsHeading[CurFolder, 1] = tbLyricsHeading1.Text;
                tempFolderLyricsHeading[CurFolder, 2] = tbLyricsHeading2.Text;
                tempFolderLyricsHeading[CurFolder, 3] = tbLyricsHeading3.Text;
            }
        }

        private void ComboLyricsHeading_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!InitFormLoad)
            {
                if (ComboLyricsHeading.SelectedIndex == 0)
                {
                    tempFolderHeadingOption[CurFolder] = 0;
                }
                else if (ComboLyricsHeading.SelectedIndex == 1)
                {
                    tempFolderHeadingOption[CurFolder] = 1;
                }
                else if (ComboLyricsHeading.SelectedIndex == 2)
                {
                    tempFolderHeadingOption[CurFolder] = 2;
                }
            }
        }

        private void FontPositionUpDown0_ValueChanged(object sender, EventArgs e)
        {
            if (!LoadTempPos)
            {
                UpdateRegionsPosTempValues(0);
            }
        }

        private void FontPositionUpDown1_ValueChanged(object sender, EventArgs e)
        {
            if (!LoadTempPos)
            {
                UpdateRegionsPosTempValues(1);
            }
        }

        private void FontPositionUpDownBottom_ValueChanged(object sender, EventArgs e)
        {
            if (!LoadTempPos)
            {
                tempBottomMargin[CurFolder] = (int)FontPositionUpDownBottom.Value;
                ApplySettings();
            }
        }

        private void LeftRightMarginUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (!LoadTempPos)
            {
                tempLeftMargin[CurFolder] = (int)LeftMarginUpDown.Value;
                tempRightMargin[CurFolder] = (int)RightMarginUpDown.Value;
                ApplySettings();
            }
        }

        private void UpdateRegionsPosTempValues(int InArea)
        {
            if (InArea != 0 && InArea == 1 && FontPositionUpDown1.Value < FontPositionUpDown0.Value)
            {
                FontPositionUpDown1.Value = FontPositionUpDown0.Value;
            }
            tempShowFontVPosition[CurFolder, 0] = (int)FontPositionUpDown0.Value;
            tempShowFontVPositionMax[CurFolder, 0] = (int)FontPositionUpDown1.Value;
            tempShowFontVPosition[CurFolder, 1] = (int)FontPositionUpDown1.Value;
            tempShowFontVPositionMin[CurFolder, 0] = (int)FontPositionUpDown0.Value;
            ApplySettings();
        }

        private void cbFolderUse_CheckedChanged(object sender, EventArgs e)
        {
            Apply_FolderUse();
        }

        private void FontSizeUpDown0_ValueChanged(object sender, EventArgs e)
        {
            tempShowFontSize[CurFolder, 0] = (int)FontSizeUpDown0.Value;
        }

        private void FontSizeUpDown1_ValueChanged(object sender, EventArgs e)
        {
            tempShowFontSize[CurFolder, 1] = (int)FontSizeUpDown1.Value;
        }

        private void ToolBarFont_R1_Click(object sender, EventArgs e)
        {
            ToolStripButton toolStripButton = (ToolStripButton)sender;
            bool @checked = toolStripButton.Checked;
            string name = toolStripButton.Name;
            if (name == "ToolBarFont_R1Bold")
            {
                tempShowFontBold[CurFolder, 0] = @checked;
            }
            else if (name == "ToolBarFont_R1Underline")
            {
                tempShowFontUnderline[CurFolder, 0] = @checked;
            }
            else if (name == "ToolBarFont_R1RTL")
            {
                tempShowFontRTL[CurFolder, 0] = @checked;
            }
            UpdateFontDisplay();
        }

        private void ToolBarFont_R1Italics_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            gf.AssignDropDownItem(ref ToolBarFont_R1Italics, e.ClickedItem.Name, ToolBarFont_R1Italics0, ToolBarFont_R1Italics1, ToolBarFont_R1Italics2);
            int num = DataUtil.ObjToInt(ToolBarFont_R1Italics.Tag);
            int num2 = num;
            if (num2 == 2)
            {
                tempShowFontItalic[CurFolder, 0] = false;
                tempShowFontItalic[CurFolder, 2] = true;
            }
            else
            {
                tempShowFontItalic[CurFolder, 0] = ((num > 0) ? true : false);
                tempShowFontItalic[CurFolder, 2] = ((num > 0) ? true : false);
            }
            UpdateFontDisplay();
        }

        private void ToolBarFont_R2_Click(object sender, EventArgs e)
        {
            ToolStripButton toolStripButton = (ToolStripButton)sender;
            bool @checked = toolStripButton.Checked;
            string name = toolStripButton.Name;
            if (name == "ToolBarFont_R2Bold")
            {
                tempShowFontBold[CurFolder, 1] = @checked;
            }
            else if (name == "ToolBarFont_R2Underline")
            {
                tempShowFontUnderline[CurFolder, 1] = @checked;
            }
            else if (name == "ToolBarFont_R2RTL")
            {
                tempShowFontRTL[CurFolder, 1] = @checked;
            }
            UpdateFontDisplay();
        }

        private void ToolBarFont_R2Italics_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            gf.AssignDropDownItem(ref ToolBarFont_R2Italics, e.ClickedItem.Name, ToolBarFont_R2Italics0, ToolBarFont_R2Italics1, ToolBarFont_R2Italics2);
            int num = DataUtil.ObjToInt(ToolBarFont_R2Italics.Tag);
            int num2 = num;
            if (num2 == 2)
            {
                tempShowFontItalic[CurFolder, 1] = false;
                tempShowFontItalic[CurFolder, 3] = true;
            }
            else
            {
                tempShowFontItalic[CurFolder, 1] = ((num > 0) ? true : false);
                tempShowFontItalic[CurFolder, 3] = ((num > 0) ? true : false);
            }
            UpdateFontDisplay();
        }

        private void UpdateFontDisplay()
        {
            try
            {
                FontStyle fontStyle = FontStyle.Regular;
                if (tempShowFontBold[CurFolder, 0])
                {
                    fontStyle |= FontStyle.Bold;
                }
                if (tempShowFontItalic[CurFolder, 0] || tempShowFontItalic[CurFolder, 2])
                {
                    fontStyle |= FontStyle.Italic;
                }
                if (tempShowFontUnderline[CurFolder, 0])
                {
                    fontStyle |= FontStyle.Underline;
                }
                labelPreviewCentreTop.Font = new Font(tempShowFontName[CurFolder, 0], 11f, fontStyle);
                fontStyle = FontStyle.Regular;
                if (tempShowFontBold[CurFolder, 1])
                {
                    fontStyle |= FontStyle.Bold;
                }
                if (tempShowFontItalic[CurFolder, 1] || tempShowFontItalic[CurFolder, 3])
                {
                    fontStyle |= FontStyle.Italic;
                }
                if (tempShowFontUnderline[CurFolder, 1])
                {
                    fontStyle |= FontStyle.Underline;
                }
                labelPreviewCentreBottom.Font = new Font(tempShowFontName[CurFolder, 1], 11f, fontStyle);
            }
            catch
            {
            }
        }

        private void ShowLineSpacingMaxUpDown_ValueChanged(object sender, EventArgs e)
        {
            tempShowLineSpacing[CurFolder, 0] = (double)ShowLineSpacingMaxUpDown.Value;
        }

        private void ShowLineSpacing2MaxUpDown_ValueChanged(object sender, EventArgs e)
        {
            tempShowLineSpacing[CurFolder, 1] = (double)ShowLineSpacing2MaxUpDown.Value;
        }

        private bool ValidateFolderRename(string NewFolderName, int SelectedItem)
        {
            for (int i = 0; i <= SongFolder.Items.Count - 1; i++)
            {
                if (SelectedItem != i && SongFolder.Items[i].Text.ToLower() == NewFolderName.ToLower())
                {
                    return false;
                }
            }
            return true;
        }

        private void SongFolder_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F2)
            {
                EditSongFolderName();
            }
        }

        private void SongFolder_Rename_Click(object sender, EventArgs e)
        {
            EditSongFolderName();
        }

        private void EditSongFolderName()
        {
            if (!((SongFolder.Items.Count == 0) | (gf.GetSelectedIndex(SongFolder) < 0)))
            {
                SongFolder.Items[gf.GetSelectedIndex(SongFolder)].BeginEdit();
            }
        }

        private void SongFolder_AfterLabelEdit(object sender, LabelEditEventArgs e)
        {
            int selectedIndex = gf.GetSelectedIndex(SongFolder);
            if (e.Label == null)
            {
                return;
            }
            string label = e.Label;
            if (!((label == SongFolder.Items[selectedIndex].Text) | (label == SongFolder.Items[selectedIndex].Text)))
            {
                if (ValidateFolderRename(label, selectedIndex))
                {
                    tempFolderName[selectedIndex + 1] = label;
                    ApplySettings();
                }
                else
                {
                    MessageBox.Show("There is already another folder with the same name! Please try a different name.");
                    e.CancelEdit = true;
                    SongFolder.Items[selectedIndex].BeginEdit();
                }
            }
        }

        private void BuildBibleList()
        {
            tempHB_TotalVersions = 0;
            ListViewItem listViewItem = new ListViewItem();
            string fullSearchString = "select * from Biblefolder where name like \"*\" and displayorder >=0 order by displayorder, name";

            fullSearchString = fullSearchString.Replace("\"*\"", "\"%\"");

            try
            {
#if OleDb
			    using DataTable datatable = DbOleDbController.GetDataTable(gf.ConnectStringDef + gf.BiblesListFileName, fullSearchString);
#elif SQLite
                using DataTable datatable = DbController.GetDataTable(gf.ConnectSQLiteDef + gf.BiblesListFileName, fullSearchString);
#endif
                BibleList.Items.Clear();
                if (datatable.Rows.Count > 0)
                {

                    foreach (DataRow dr in datatable.Rows)
                    {
                        if (tempHB_TotalVersions <= 250)
                        {
                            listViewItem = BibleList.Items.Add(DataUtil.GetDataString(dr, "name"));
                            string InFileName = DataUtil.GetDataString(dr, "filename");
                            if (File.Exists(gf.BibleDir + InFileName))
                            {
                                listViewItem.ImageIndex = 4;
                                listViewItem.SubItems.Add(DataUtil.GetDataString(dr, "description"));
                            }
                            else
                            {
                                listViewItem.ImageIndex = 27;
                                listViewItem.SubItems.Add("** Cannot find Bible - please check Filename!");
                            }
                            listViewItem.SubItems.Add(gf.GetDisplayNameOnly(ref InFileName, UpdateByRef: false, KeepExt: true));
                            listViewItem.SubItems.Add(DataUtil.GetDataString(dr, "copyright"));
                            string text = DataUtil.GetDataString(dr, "songfolder");
                            if ((text == "") | (text == "0"))
                            {
                                text = "1";
                            }
                            listViewItem.SubItems.Add(text);
                            text = DataUtil.GetDataString(dr, "size");
                            if ((text == "") | (text == "0"))
                            {
                                text = "80";
                            }
                            listViewItem.SubItems.Add(text);
                        }
                    }
                    BibleList.Items[0].Selected = true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
        }

        private void BuildBibleAssociatedFolder()
        {
            BibleAssociatedFolder.Items.Clear();
            for (int i = 1; i < 41; i++)
            {
                BibleAssociatedFolder.Items.Add(tempFolderName[i]);
            }
            if (BibleAssociatedFolder.Items.Count > 0)
            {
                BibleAssociatedFolder.SelectedIndex = 0;
            }
        }

        private void btnBibleSearch_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            string text = "";
            string Name = "";
            string Description = "";
            string Copyright = "";
            string Info = "";
            ListViewItem listViewItem = new ListViewItem();
            BibleSearchList.Items.Clear();
            string[] files = Directory.GetFiles(gf.BibleDir, "*.mdb");
            try
            {
                string[] array = files;
                foreach (string text2 in array)
                {
                    text = text2;
                    if (gf.LookUpBibleName(text, ref Name, ref Description, ref Copyright, ref Info) && !BibleFileNameOnBibleList(text))
                    {
                        listViewItem = BibleSearchList.Items.Add(Name);
                        listViewItem.ImageIndex = 4;
                        listViewItem.SubItems.Add(Description);
                        listViewItem.SubItems.Add(gf.GetDisplayNameOnly(ref text, UpdateByRef: false, KeepExt: true));
                        listViewItem.SubItems.Add(Copyright);
                    }
                }
            }
            catch
            {
            }
            Cursor = Cursors.Default;
        }

        private bool BibleFileNameOnBibleList(string InFileName)
        {
            gf.GetDisplayNameOnly(ref InFileName, UpdateByRef: true, KeepExt: true);
            for (int i = 0; i <= BibleList.Items.Count - 1; i++)
            {
                if (BibleList.Items[i].SubItems[2].Text == InFileName)
                {
                    return true;
                }
            }
            return false;
        }

        private void btnBibleAdd_Click(object sender, EventArgs e)
        {
            AddBibleToList();
        }

        private void BibleSearchList_DoubleClick(object sender, EventArgs e)
        {
            AddBibleToList();
        }

        private void AddBibleToList()
        {
            if (BibleSearchList.Items.Count == 0)
            {
                MessageBox.Show("There are no Bibles to add - try clicking the 'Search' button first");
                return;
            }
            if (gf.GetSelectedIndex(BibleSearchList) < 0)
            {
                MessageBox.Show("Please select a Bible from the list to Add!");
                return;
            }
            ListViewItem listViewItem = new ListViewItem();
            for (int num = BibleSearchList.Items.Count - 1; num >= 0; num--)
            {
                if (BibleSearchList.Items[num].Selected)
                {
                    listViewItem = BibleList.Items.Add(BibleSearchList.Items[num].Text);
                    listViewItem.ImageIndex = 4;
                    listViewItem.SubItems.Add(BibleSearchList.Items[num].SubItems[1].Text);
                    listViewItem.SubItems.Add(BibleSearchList.Items[num].SubItems[2].Text);
                    listViewItem.SubItems.Add(BibleSearchList.Items[num].SubItems[3].Text);
                    listViewItem.SubItems.Add("1");
                    listViewItem.SubItems.Add("80");
                    BibleSearchList.Items[num].Remove();
                }
            }
            gf.Options_BibleListChanged = true;
        }

        private void btnBibleRemove_Click(object sender, EventArgs e)
        {
            if (BibleList.Items.Count == 0)
            {
                MessageBox.Show("There are no Bibles to remove!");
                return;
            }
            if (gf.GetSelectedIndex(BibleList) < 0)
            {
                MessageBox.Show("Please select a Bible from the list to Remove");
                return;
            }
            for (int num = BibleList.Items.Count - 1; num >= 0; num--)
            {
                if (BibleList.Items[num].Selected)
                {
                    BibleList.Items[num].Remove();
                }
            }
            gf.Options_BibleListChanged = true;
        }

        private void Bibles_Click(object sender, EventArgs e)
        {
            ToolStripButton toolStripButton = (ToolStripButton)sender;
            string name = toolStripButton.Name;
            if (name == "Bibles_Info")
            {
                DisplayBibleInfo();
            }
            else if (name == "Bibles_Up")
            {
                MoveBibleUp();
            }
            else if (name == "Bibles_Down")
            {
                MoveBibleDown();
            }
        }

        private void DisplayBibleInfo()
        {
            int count = BibleList.Items.Count;
            if (count < 1)
            {
                return;
            }
            int selectedIndex = gf.GetSelectedIndex(BibleList);
            if (selectedIndex >= 0)
            {
                string fileName = gf.BibleDir + BibleList.Items[selectedIndex].SubItems[2].Text;
                string Name = "";
                string Description = "";
                string Copyright = "";
                string Info = "";
                if (gf.LookUpBibleName(fileName, ref Name, ref Description, ref Copyright, ref Info) && Info != "")
                {
                    MessageBox.Show(Info, BibleList.Items[selectedIndex].SubItems[1].Text);
                }
            }
        }

        private void MoveBibleUp()
        {
            int count = BibleList.Items.Count;
            if (count < 1)
            {
                return;
            }
            int selectedIndex = gf.GetSelectedIndex(BibleList);
            if (selectedIndex >= 1)
            {
                for (int i = 0; i <= 5; i++)
                {
                    string text = BibleList.Items[selectedIndex].SubItems[i].Text;
                    BibleList.Items[selectedIndex].SubItems[i].Text = BibleList.Items[selectedIndex - 1].SubItems[i].Text;
                    BibleList.Items[selectedIndex - 1].SubItems[i].Text = text;
                }
                int imageIndex = BibleList.Items[selectedIndex].ImageIndex;
                BibleList.Items[selectedIndex].ImageIndex = BibleList.Items[selectedIndex - 1].ImageIndex;
                BibleList.Items[selectedIndex - 1].ImageIndex = imageIndex;
                BibleList.Items[selectedIndex].Selected = false;
                BibleList.Items[selectedIndex - 1].Selected = true;
                BibleList.EnsureVisible(selectedIndex - 1);
                gf.Options_BibleListChanged = true;
            }
        }

        private void MoveBibleDown()
        {
            int count = BibleList.Items.Count;
            if (count <= 1)
            {
                return;
            }
            int selectedIndex = gf.GetSelectedIndex(BibleList);
            if (!((selectedIndex < 0) | (selectedIndex == count - 1)))
            {
                for (int i = 0; i <= 5; i++)
                {
                    string text = BibleList.Items[selectedIndex].SubItems[i].Text;
                    BibleList.Items[selectedIndex].SubItems[i].Text = BibleList.Items[selectedIndex + 1].SubItems[i].Text;
                    BibleList.Items[selectedIndex + 1].SubItems[i].Text = text;
                }
                int imageIndex = BibleList.Items[selectedIndex].ImageIndex;
                BibleList.Items[selectedIndex].ImageIndex = BibleList.Items[selectedIndex + 1].ImageIndex;
                BibleList.Items[selectedIndex + 1].ImageIndex = imageIndex;
                BibleList.Items[selectedIndex].Selected = false;
                BibleList.Items[selectedIndex + 1].Selected = true;
                BibleList.EnsureVisible(selectedIndex + 1);
                gf.Options_BibleListChanged = true;
            }
        }

#if OleDb
        public void SaveBibleChanges()
        {
            string text = "";
            using (OleDbConnection daoDb = DbConnectionController.GetOleDbConnection(gf.ConnectStringDef + gf.BiblesListFileName))
            {
                OleDbDataAdapter da;
                DataSet ds;
                DataTable dt;
                try
                {
                    string query = "select * from Biblefolder where NAME like \"*\" ";
#if ODBC
                    query = query.Replace("\"*\"", "\"%\"");
#endif
                    (da, ds) = DbOleDbController.getDataAdapter(daoDb, query);
                    dt = ds.Tables[0];
                    if (dt.Rows.Count > 0)
                    {
                        foreach (DataRow dr in dt.Rows)
                        {
                            dr["displayorder"] = -1;
                        }
                        da.Update(dt);
                        dt.Dispose();
                        da.Dispose();
                    }


                    for (int i = 0; i < BibleList.Items.Count; i++)
                    {
                        (da, ds) = DbOleDbController.getDataAdapter(daoDb, "select * from Biblefolder where FILENAME = \"" + BibleList.Items[i].SubItems[2].Text + "\"");
                        dt = ds.Tables[0];
                        if (dt.Rows.Count > 0)
                        {
                            DataRow dr = dt.Rows[i];
                            //recordset.Edit();
                            dr["name"] = BibleList.Items[i].Text;
                            dr["description"] = BibleList.Items[i].SubItems[1].Text;
                            dr["filename"] = BibleList.Items[i].SubItems[2].Text;
                            dr["copyright"] = BibleList.Items[i].SubItems[3].Text;
                            dr["songfolder"] = BibleList.Items[i].SubItems[4].Text;
                            int num = (BibleList.Items[i].SubItems[5].Text == "") ? 1 : Convert.ToInt32(BibleList.Items[i].SubItems[5].Text);
                            dr["size"] = num;
                            dr["displayorder"] = i;
                        }
                        else
                        {
                            query = "select * from Biblefolder where NAME like \"*\" ";
#if ODBC
                            query = query.Replace("\"*\"", "\"%\"");
#endif
                            (da, ds) = DbOleDbController.getDataAdapter(daoDb, query);
                            dt = ds.Tables[0];
                            DataRow dr = dt.NewRow();
                            dr["name"] = BibleList.Items[i].Text;
                            dr["description"] = BibleList.Items[i].SubItems[1].Text;
                            dr["filename"] = BibleList.Items[i].SubItems[2].Text;
                            dr["copyright"] = BibleList.Items[i].SubItems[3].Text;
                            dr["songfolder"] = BibleList.Items[i].SubItems[4].Text;
                            int num = (BibleList.Items[i].SubItems[5].Text == "") ? 1 : Convert.ToInt32(BibleList.Items[i].SubItems[5].Text);
                            dr["size"] = num;
                            dr["displayorder"] = i;
                        }
                        da.Update(dt);
                        dt.Dispose();
                        da.Dispose();

                        gf.HB_Versions[i, 1] = BibleList.Items[i].Text;
                        gf.HB_Versions[i, 2] = BibleList.Items[i].SubItems[1].Text;
                        gf.HB_Versions[i, 4] = gf.BibleDir + BibleList.Items[i].SubItems[2].Text;
                        gf.HB_Versions[i, 3] = BibleList.Items[i].SubItems[3].Text;
                        gf.HB_Versions[i, 5] = BibleList.Items[i].SubItems[4].Text;
                        gf.HB_Versions[i, 6] = BibleList.Items[i].SubItems[5].Text;
                    }
                    gf.HB_TotalVersions = BibleList.Items.Count;
                    (da, ds) = DbOleDbController.getDataAdapter(daoDb, "select * from Biblefolder where displayorder < 0 ");
                    dt = ds.Tables[0];
                    if (dt.Rows.Count > 0)
                    {
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            dt.Rows[i].Delete();
                        }
                        da.Update(dt);
                    }

                    dt.Dispose();
                    da.Dispose();

                }
                catch
                {
                }
            }
        }

#elif SQLite
        public void SaveBibleChanges()
        {
            using DbConnection connection = DbController.GetDbConnection(gf.ConnectStringSQLiteDef + gf.BiblesListFileName);
            DbDataAdapter sQLiteDataAdapter;
            DataTable dataTable;
            try
            {
                string query = "select * from Biblefolder where NAME like \"%\" ";

                (sQLiteDataAdapter, dataTable) = DbController.getDataAdapter(connection, query);

                if (dataTable.Rows.Count > 0)
                {
                    foreach (DataRow dr in dataTable.Rows)
                    {
                        dr["displayorder"] = -1;
                    }
                    sQLiteDataAdapter.Update(dataTable);
                }


                for (int i = 0; i < BibleList.Items.Count; i++)
                {
                    (sQLiteDataAdapter, dataTable) = DbController.getDataAdapter(connection, "select * from Biblefolder where FILENAME = \"" + BibleList.Items[i].SubItems[2].Text + "\"");

                    if (dataTable.Rows.Count > 0)
                    {
                        DataRow dr = dataTable.Rows[i];
                        dr["name"] = BibleList.Items[i].Text;
                        dr["description"] = BibleList.Items[i].SubItems[1].Text;
                        dr["filename"] = BibleList.Items[i].SubItems[2].Text;
                        dr["copyright"] = BibleList.Items[i].SubItems[3].Text;
                        dr["songfolder"] = BibleList.Items[i].SubItems[4].Text;
                        int num = (BibleList.Items[i].SubItems[5].Text == "") ? 1 : Convert.ToInt32(BibleList.Items[i].SubItems[5].Text);
                        dr["size"] = num;
                        dr["displayorder"] = i;
                    }
                    else
                    {
                        query = "select * from Biblefolder where NAME like \"%\" ";
                        (sQLiteDataAdapter, dataTable) = DbController.getDataAdapter(connection, query);

                        DataRow dr = dataTable.NewRow();
                        dr["name"] = BibleList.Items[i].Text;
                        dr["description"] = BibleList.Items[i].SubItems[1].Text;
                        dr["filename"] = BibleList.Items[i].SubItems[2].Text;
                        dr["copyright"] = BibleList.Items[i].SubItems[3].Text;
                        dr["songfolder"] = BibleList.Items[i].SubItems[4].Text;
                        int num = (BibleList.Items[i].SubItems[5].Text == "") ? 1 : Convert.ToInt32(BibleList.Items[i].SubItems[5].Text);
                        dr["size"] = num;
                        dr["displayorder"] = i;
                    }
                    sQLiteDataAdapter.Update(dataTable);
                    dataTable.Dispose();


                    gf.HB_Versions[i, 1] = BibleList.Items[i].Text;
                    gf.HB_Versions[i, 2] = BibleList.Items[i].SubItems[1].Text;
                    gf.HB_Versions[i, 4] = gf.BibleDir + BibleList.Items[i].SubItems[2].Text;
                    gf.HB_Versions[i, 3] = BibleList.Items[i].SubItems[3].Text;
                    gf.HB_Versions[i, 5] = BibleList.Items[i].SubItems[4].Text;
                    gf.HB_Versions[i, 6] = BibleList.Items[i].SubItems[5].Text;
                }

                gf.HB_TotalVersions = BibleList.Items.Count;
                (sQLiteDataAdapter, dataTable) = DbController.getDataAdapter(connection, "select * from Biblefolder where displayorder < 0 ");

                if (dataTable.Rows.Count > 0)
                {
                    for (int i = 0; i < dataTable.Rows.Count; i++)
                    {
                        dataTable.Rows[i].Delete();
                    }
                    sQLiteDataAdapter.Update(dataTable);
                }

                dataTable.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }

        }
#endif
        private void btnBibleNameChange_Click(object sender, EventArgs e)
        {
            int selectedIndex = gf.GetSelectedIndex(BibleList);
            if (selectedIndex < 0)
            {
                return;
            }
            gf.Rename_String = BibleList.Items[selectedIndex].Text;
            gf.Rename_ExistingString = "";
            for (int i = 0; i < BibleList.Items.Count; i++)
            {
                if (selectedIndex != i)
                {
                    gf.Rename_ExistingString = gf.Rename_ExistingString + BibleList.Items[i].Text + ";";
                }
            }
            FrmBibleRename frmBibleRename = new FrmBibleRename();
            if (frmBibleRename.ShowDialog() == DialogResult.OK)
            {
                BibleList.Items[selectedIndex].Text = gf.Rename_String;
                gf.Options_BibleListChanged = true;
            }
        }

        private void BibleList_SelectedIndexChanged(object sender, EventArgs e)
        {
            BibleListIndexChanged();
        }

        private void BibleListIndexChanged()
        {
            int selectedIndex = gf.GetSelectedIndex(BibleList);
            if (selectedIndex >= 0)
            {
                InitFormLoad = true;
                BibleAssociatedFolder.Text = tempFolderName[Convert.ToInt32(BibleList.Items[selectedIndex].SubItems[4].Text)];
                int num = Convert.ToInt32(BibleList.Items[selectedIndex].SubItems[5].Text);
                BibleFontSizeUpDown.Value = (((num < 5) | (num > 200)) ? 80 : num);
                InitFormLoad = false;
            }
        }

        private void BibleAssociatedFolder_SelectedIndexChanged(object sender, EventArgs e)
        {
            int selectedIndex = gf.GetSelectedIndex(BibleList);
            if (selectedIndex >= 0)
            {
                BibleList.Items[selectedIndex].SubItems[4].Text = ((BibleAssociatedFolder.Text == "") ? "1" : Convert.ToString(BibleAssociatedFolder.SelectedIndex + 1));
                if (!InitFormLoad)
                {
                    gf.Options_BibleListChanged = true;
                }
            }
        }

        private void BibleFontSizeUpDown_ValueChanged(object sender, EventArgs e)
        {
            int selectedIndex = gf.GetSelectedIndex(BibleList);
            if (selectedIndex >= 0)
            {
                BibleList.Items[selectedIndex].SubItems[5].Text = BibleFontSizeUpDown.Value.ToString();
                if (!InitFormLoad)
                {
                    gf.Options_BibleListChanged = true;
                }
            }
        }

        private void BuildLicencesList()
        {
            AdminLic4.Text = gf.LicAdmin_List[4, 0];
            AdminLic5.Text = gf.LicAdmin_List[5, 0];
            AdminLic6.Text = gf.LicAdmin_List[6, 0];
            AdminLic7.Text = gf.LicAdmin_List[7, 0];
            AdminLic8.Text = gf.LicAdmin_List[8, 0];
            AdminLicNo3.Text = gf.LicAdmin_List[3, 1];
            AdminLicNo4.Text = gf.LicAdmin_List[4, 1];
            AdminLicNo5.Text = gf.LicAdmin_List[5, 1];
            AdminLicNo6.Text = gf.LicAdmin_List[6, 1];
            AdminLicNo7.Text = gf.LicAdmin_List[7, 1];
            AdminLicNo8.Text = gf.LicAdmin_List[8, 1];
            AdminLicPreview3.Text = gf.LicAdmin_List[3, 2];
            AdminLicPreview4.Text = gf.LicAdmin_List[4, 2];
            AdminLicPreview5.Text = gf.LicAdmin_List[5, 2];
            AdminLicPreview6.Text = gf.LicAdmin_List[6, 2];
            AdminLicPreview7.Text = gf.LicAdmin_List[7, 2];
            AdminLicPreview8.Text = gf.LicAdmin_List[8, 2];
            tbNumberSymbol.Text = gf.LicAdminNoSymbol;
            cbEnforceDisplay.Checked = gf.LicAdminEnforceDisplay;
        }

        private void AdminLic_TextChanged(object sender, EventArgs e)
        {
            LicencesPreview();
        }

        private void LicencesPreview()
        {
            AdminLicPreview3.Text = (((DataUtil.Trim(AdminLic3.Text) != "") & (DataUtil.Trim(AdminLicNo3.Text) != "")) ? (DataUtil.Trim(AdminLic3.Text) + tbNumberSymbol.Text + DataUtil.Trim(AdminLicNo3.Text)) : "");
            AdminLicPreview4.Text = (((DataUtil.Trim(AdminLic4.Text) != "") & (DataUtil.Trim(AdminLicNo4.Text) != "")) ? (DataUtil.Trim(AdminLic4.Text) + tbNumberSymbol.Text + DataUtil.Trim(AdminLicNo4.Text)) : "");
            AdminLicPreview5.Text = (((DataUtil.Trim(AdminLic5.Text) != "") & (DataUtil.Trim(AdminLicNo5.Text) != "")) ? (DataUtil.Trim(AdminLic5.Text) + tbNumberSymbol.Text + DataUtil.Trim(AdminLicNo5.Text)) : "");
            AdminLicPreview6.Text = (((DataUtil.Trim(AdminLic6.Text) != "") & (DataUtil.Trim(AdminLicNo6.Text) != "")) ? (DataUtil.Trim(AdminLic6.Text) + tbNumberSymbol.Text + DataUtil.Trim(AdminLicNo6.Text)) : "");
            AdminLicPreview7.Text = (((DataUtil.Trim(AdminLic7.Text) != "") & (DataUtil.Trim(AdminLicNo7.Text) != "")) ? (DataUtil.Trim(AdminLic7.Text) + tbNumberSymbol.Text + DataUtil.Trim(AdminLicNo7.Text)) : "");
            AdminLicPreview8.Text = (((DataUtil.Trim(AdminLic8.Text) != "") & (DataUtil.Trim(AdminLicNo8.Text) != "")) ? (DataUtil.Trim(AdminLic8.Text) + tbNumberSymbol.Text + DataUtil.Trim(AdminLicNo8.Text)) : "");
        }

        //		public void SaveLicenceChanges()
        //		{
        //			gf.LicAdmin_List[3, 0] = AdminLic3.Text;
        //			gf.LicAdmin_List[4, 0] = AdminLic4.Text;
        //			gf.LicAdmin_List[5, 0] = AdminLic5.Text;
        //			gf.LicAdmin_List[6, 0] = AdminLic6.Text;
        //			gf.LicAdmin_List[7, 0] = AdminLic7.Text;
        //			gf.LicAdmin_List[8, 0] = AdminLic8.Text;
        //			gf.LicAdmin_List[3, 1] = AdminLicNo3.Text;
        //			gf.LicAdmin_List[4, 1] = AdminLicNo4.Text;
        //			gf.LicAdmin_List[5, 1] = AdminLicNo5.Text;
        //			gf.LicAdmin_List[6, 1] = AdminLicNo6.Text;
        //			gf.LicAdmin_List[7, 1] = AdminLicNo7.Text;
        //			gf.LicAdmin_List[8, 1] = AdminLicNo8.Text;
        //			gf.LicAdmin_List[3, 2] = AdminLicPreview3.Text;
        //			gf.LicAdmin_List[4, 2] = AdminLicPreview4.Text;
        //			gf.LicAdmin_List[5, 2] = AdminLicPreview5.Text;
        //			gf.LicAdmin_List[6, 2] = AdminLicPreview6.Text;
        //			gf.LicAdmin_List[7, 2] = AdminLicPreview7.Text;
        //			gf.LicAdmin_List[8, 2] = AdminLicPreview8.Text;
        //			gf.LicAdminNoSymbol = tbNumberSymbol.Text;
        //			gf.LicAdminEnforceDisplay = cbEnforceDisplay.Checked;
        //			try
        //			{
        //				string text = "";
        //				using (OleDbConnection daoDb = DbConnectionController.GetOleDbConnection(gf.ConnectStringMainDB))
        //				{
        //					OleDbDataAdapter da = null;
        //					DataSet ds = null;
        //					DataTable dt = null;
        //					for (int i = 0; i < 9; i++)
        //					{
        //						if ((i == 0) | (i >= 4))
        //						{
        //							(da, ds) = DbOleDbController.getDataAdapter(daoDb, "select * from Folder where FolderNo=" + i);
        //							dt = ds.Tables[0];
        //							//recordset.Edit();
        //							dt.Rows[0]["ADMINISTRATOR"] = gf.LicAdmin_List[i, 0];
        //							da.Update(dt);
        //							dt.Dispose();
        //							da.Dispose();
        //							//recordset.Dispose();
        //						}
        //						else
        //						{
        //							string query = "select * from LICENCE where ADMINISTRATOR like \"*\" ";
        //#if ODBC
        //							query = query.Replace("\"*\"", "\"%\"");
        //#endif
        //							(da, ds) = DbOleDbController.getDataAdapter(daoDb, query);
        //							dt = ds.Tables[0];
        //							DataRow dr = dt.NewRow();
        //							dr["ADMINISTRATOR"] = gf.LicAdmin_List[i, 0];
        //							dr["REF"] = i;
        //							da.Update(dt);
        //							dt.Dispose();
        //							da.Dispose();

        //							//recordset.Dispose();
        //						}
        //					}
        //				}		
        //			}
        //			catch
        //			{
        //			}
        //			gf.SaveLicenceConfigSettings();
        //		}

        private void LoadGeneralSetting()
        {
            VersesMaxUpDown.Value = gf.HB_MaxVersesSelection;
            AdhocVersesMaxUpDown.Value = gf.HB_MaxAdhocVersesSelection;
            PPMaxUpDown.Value = gf.PP_MaxFiles;
            EditHistoryMaxUpDown.Value = gf.MaxUserEditHistory;
            BuildMonitorsList();
            cbDisableScreenSaver.Checked = gf.DisableSreenSaver;
            VideoSizeUpDown1.Value = gf.VideoSize;
            string text = "";
            gf.AssignDropDownItem(SelectedMenuItemName: (gf.VideoVAlign == 0) ? Video_VAlignTop.Name : ((gf.VideoVAlign != 1) ? Video_VAlignBottom.Name : Video_VAlignCentre.Name), SelectedBtn: ref Video_VAlign, InMenuItem1: Video_VAlignTop, InMenuItem2: Video_VAlignCentre, InMenuItem3: Video_VAlignBottom);
            UpdateVideoSizeSample();
            DM_AlwaysUseSecondaryMonitor.Checked = gf.DMAlwaysUseSecondaryMonitor;
            if (gf.DualMonitorSelectAutoOption == 0)
            {
                optDM0.Checked = true;
            }
            else
            {
                optDM1.Checked = true;
            }
            // daniel
            // 스크린 옵션 추가
            optWide.Checked = (gf.isScreenWideMode) ? true : false;

            DM1UpDownTop.Value = gf.DMOption1Top;
            DM1UpDownLeft.Value = gf.DMOption1Left;
            DM1UpDownWidth.Value = (gf.DMOption1Width < 1) ? 1 : gf.DMOption1Width;
            DM1UpDownHeight.Value = (gf.DMOption1Height < 1) ? 1 : gf.DMOption1Height;

            //DM1UpDownHeight.Value = gf.DMOption1Height;

            DM_CustomAsSingleMonitor.Checked = gf.DMOption1AsSingleMonitor;

            gf.GetScreenName(ref gf.OutputMonitorName, "None");

            int outputMonitorIndex = DualMonitorList.FindString(gf.OutputMonitorName);

            DualMonitorList.SelectedIndex = outputMonitorIndex;

            LM_AlwaysUse.Checked = gf.LMAlwaysUseSecondaryMonitor;
            if (gf.LMSelectAutoOption == 1)
            {
                optLM1.Checked = true;
            }
            else
            {
                optLM0.Checked = true;
            }
            LM1UpDownTop.Value = gf.LMOption1Top;
            LM1UpDownLeft.Value = gf.LMOption1Left;
            LM1UpDownWidth.Value = ((gf.LMOption1Width < 1) ? 1 : gf.LMOption1Width);
            LM1UpDownHeight.Value = ((gf.LMOption1Height < 1) ? 1 : gf.LMOption1Height);
            /// gf.DualMonitorSelectAutoOption :  
            //gf.GetScreenNumber(ref gf.LyricsMonitorNumber, (gf.DualMonitorSelectAutoOption == 0) ? gf.OutputMonitorNumber : (-1));

            if (gf.LyricsMonitorName != "None")
            {
                gf.GetScreenName(ref gf.LyricsMonitorName, (gf.DualMonitorSelectAutoOption == 0) ? gf.OutputMonitorName : "None");
            }

            int lyricsMonitorIndex = LyricsMonitorList.FindString(gf.LyricsMonitorName);

            LyricsMonitorList.SelectedIndex = lyricsMonitorIndex;

            btnLMTextColour.ForeColor = gf.LMTextColour;
            btnLMHighlightColour.ForeColor = gf.LMHighlightColour;
            btnLMBackColour.ForeColor = gf.LMBackColour;
            PreviewFontUpDown.Value = ((gf.PreviewArea_FontSize >= 8) ? gf.PreviewArea_FontSize : 8);
            LMUpDownFontSize.Value = ((gf.LMMainFontSize >= 8 && gf.LMMainFontSize <= 40) ? gf.LMMainFontSize : 20);
            LMNotationsUpDownFontSize.Value = ((gf.LMNotationsFontSize >= 8 && gf.LMNotationsFontSize <= 40) ? gf.LMNotationsFontSize : 20);
            cbLMShowNotations.Checked = gf.LMShowNotations;
            LM_Bold.Checked = gf.LMFontBold;
            LM_Italic.Checked = gf.LMFontItalic;
            LM_Underline.Checked = gf.LMFontUnderline;
            btnTextRegionChangeColour.ForeColor = gf.FocusedTextRegionColour;
            btnTextRegionSlideTextColour.ForeColor = gf.TextRegionSlideTextColour;
            btnTextRegionSlideBackColour.ForeColor = gf.TextRegionSlideBackColour;
            TextRegionUseColour.Checked = gf.UseFocusedTextRegionColour;
            checkBoxPPTab.Checked = gf.UsePowerpointTab;
            checkBoxPPNoPanel.Checked = gf.NoPowerpointPanelOverlay;
            checkBoxMediaTab.Checked = gf.UseMediaTab;
            checkBoxMediaNoPanel.Checked = gf.NoMediaPanelOverlay;
            checkBoxLMBox.Checked = gf.ShowLyricsMonitorAlertBox;
            cbUseLargestFont.Checked = gf.UseLargestFontSize;
            cbAutoTextOverflow.Checked = gf.AutoTextOverflow;
            cbAdvanceNextItem.Checked = gf.AdvanceNextItem;
            cbLineBetweenRegions.Checked = gf.LineBetweenRegions;
            cbWordWrapLeftAlignIndent.Checked = gf.WordWrapLeftAlignIndent;
            if (gf.GapItemOption == GapType.Black)
            {
                rbGapItemOption1.Checked = true;
            }
            else if (gf.GapItemOption == GapType.Default)
            {
                rbGapItemOption2.Checked = true;
            }
            else if (gf.GapItemOption == GapType.User)
            {
                rbGapItemOption3.Checked = true;
            }
            else
            {
                rbGapItemOption0.Checked = true;
            }
            tbGapLogoLocation.Text = gf.GapItemLogoFile;
            cbGapItemUseFade.Checked = gf.GapItemUseFade;
            NotationFontFactorUpDown.Value = (int)(gf.NotationFontFactor * 100.0);
            cbPreviewShowNotations.Checked = gf.PreviewArea_ShowNotations;
            cbLineBetweenScreens.Checked = gf.PreviewArea_LineBetweenScreens;
            PreviewFontUpDown.Value = ((gf.PreviewArea_FontSize >= 8) ? gf.PreviewArea_FontSize : 8);
            tbMusicLocation.Text = gf.MediaDir;
            tbMusicLocation.BackColor = tbGapLogoLocation.BackColor;
            gf.LoadBlankCaptureDevices(ref cbCaptureDevices);
            if (gf.WMP_Present)
            {
                try
                {
                    DShowLib dShowLib = new DShowLib();
                    dShowLib.ListCaptureDevices(ref cbCaptureDevices);
                }
                catch
                {
                }
            }
            cbCaptureDevices.SelectedIndex = gf.LiveCamNumber - 1;
            TrackBarVolume.Value = (((gf.LiveCamVolume >= 0) & (gf.LiveCamVolume <= 100)) ? gf.LiveCamVolume : 30);
            TrackBarBalance.Value = (((gf.LiveCamBalance >= -100) & (gf.LiveCamBalance <= 100)) ? gf.LiveCamBalance : 0);
            cbMute.Checked = gf.LiveCamMute;
            cbWidescreen.Checked = gf.LiveCamWidescreen;
            checkBoxLiveCamNoPanel.Checked = gf.LiveCamNoPanelOverlay;
            MessageAlertDurationUpDown.Value = gf.MessageAlertDuration;
            Message_Scroll.Checked = gf.MessageAlertScroll;
            Message_Flash.Checked = gf.MessageAlertFlash;
            Message_Transparent.Checked = gf.MessageAlertTransparent;
            MessageComboFont.Text = gf.MessageAlertFontName;
            MessageSizeUpDown.Value = gf.MessageAlertFontSize;
            btnMessageChangeTextColour.ForeColor = gf.MessageAlertTextColour;
            btnMessageChangeBackColour.ForeColor = gf.MessageAlertBackColour;
            gf.AssignDropDownItem(SelectedMenuItemName: (gf.MessageAlertTextAlign == 1) ? Message_AlignLeft.Name : ((gf.MessageAlertTextAlign != 2) ? Message_AlignRight.Name : Message_AlignCentre.Name), SelectedBtn: ref Message_Align, InMenuItem1: Message_AlignLeft, InMenuItem2: Message_AlignCentre, InMenuItem3: Message_AlignRight);
            gf.AssignDropDownItem(SelectedMenuItemName: (gf.MessageAlertVerticalAlign != 0) ? Message_VAlignBottom.Name : Message_VAlignTop.Name, SelectedBtn: ref Message_VAlign, InMenuItem1: Message_VAlignTop, InMenuItem2: Message_VAlignBottom);
            Message_Bold.Checked = gf.MessageAlertBold;
            Message_Italics.Checked = gf.MessageAlertItalic;
            Message_Underline.Checked = gf.MessageAlertUnderline;
            Message_Shadow.Checked = gf.MessageAlertShadow;
            Message_Outline.Checked = gf.MessageAlertOutline;
            ParentalAlertUpDown.Value = gf.ParentalAlertDuration;
            Parental_Scroll.Checked = gf.ParentalAlertScroll;
            Parental_Flash.Checked = gf.ParentalAlertFlash;
            Parental_Transparent.Checked = gf.ParentalAlertTransparent;
            ParentalComboFont.Text = gf.ParentalAlertFontName;
            ParentalSizeUpDown.Value = gf.ParentalAlertFontSize;
            btnParentalChangeTextColour.ForeColor = gf.ParentalAlertTextColour;
            btnParentalChangeBackColour.ForeColor = gf.ParentalAlertBackColour;
            ParentalAlert.Text = gf.ParentalAlertHeading;
            gf.AssignDropDownItem(SelectedMenuItemName: (gf.ParentalAlertTextAlign == 1) ? Parental_AlignLeft.Name : ((gf.ParentalAlertTextAlign != 2) ? Parental_AlignRight.Name : Parental_AlignCentre.Name), SelectedBtn: ref Parental_Align, InMenuItem1: Parental_AlignLeft, InMenuItem2: Parental_AlignCentre, InMenuItem3: Parental_AlignRight);
            gf.AssignDropDownItem(SelectedMenuItemName: (gf.ParentalAlertVerticalAlign != 0) ? Parental_VAlignBottom.Name : Parental_VAlignTop.Name, SelectedBtn: ref Parental_VAlign, InMenuItem1: Parental_VAlignTop, InMenuItem2: Parental_VAlignBottom);
            Parental_Bold.Checked = gf.ParentalAlertBold;
            Parental_Italics.Checked = gf.ParentalAlertItalic;
            Parental_Underline.Checked = gf.ParentalAlertUnderline;
            Parental_Shadow.Checked = gf.ParentalAlertShadow;
            Parental_Outline.Checked = gf.ParentalAlertOutline;
            ReferenceAlertDurationUpDown.Value = gf.ReferenceAlertDuration;
            Reference_Scroll.Checked = gf.ReferenceAlertScroll;
            Reference_Flash.Checked = gf.ReferenceAlertFlash;
            Reference_Transparent.Checked = gf.ReferenceAlertTransparent;
            ReferenceComboFont.Text = gf.ReferenceAlertFontName;
            ReferenceSizeUpDown.Value = gf.ReferenceAlertFontSize;
            btnReferenceChangeTextColour.ForeColor = gf.ReferenceAlertTextColour;
            btnReferenceChangeBackColour.ForeColor = gf.ReferenceAlertBackColour;
            gf.AssignDropDownItem(SelectedMenuItemName: (gf.ReferenceAlertTextAlign == 1) ? Reference_AlignLeft.Name : ((gf.ReferenceAlertTextAlign != 2) ? Reference_AlignRight.Name : Reference_AlignCentre.Name), SelectedBtn: ref Reference_Align, InMenuItem1: Reference_AlignLeft, InMenuItem2: Reference_AlignCentre, InMenuItem3: Reference_AlignRight);
            gf.AssignDropDownItem(SelectedMenuItemName: (gf.ReferenceAlertVerticalAlign == 0) ? Reference_VAlignTop.Name : ((gf.ReferenceAlertVerticalAlign != 2) ? Reference_VAlignCentre.Name : Reference_VAlignBottom.Name), SelectedBtn: ref Reference_VAlign, InMenuItem1: Reference_VAlignTop, InMenuItem2: Reference_VAlignCentre, InMenuItem3: Reference_VAlignBottom);
            Reference_Bold.Checked = gf.ReferenceAlertBold;
            Reference_Italics.Checked = gf.ReferenceAlertItalic;
            Reference_Underline.Checked = gf.ReferenceAlertUnderline;
            Reference_Shadow.Checked = gf.ReferenceAlertShadow;
            Reference_Outline.Checked = gf.ReferenceAlertOutline;
            cbPick.Checked = gf.ReferenceAlertUsePick;
            cbPickBlank.Checked = gf.ReferenceAlertBlankIfPickNotFound;
            tbPick.Text = gf.ReferenceAlertPickName;
            tbSubstitute.Text = gf.ReferenceAlertPickSubstitute;
            if (gf.ReferenceAlertSource == 1)
            {
                Reference_Source1.Checked = true;
            }
            else if (gf.ReferenceAlertSource == 2)
            {
                Reference_Source2.Checked = true;
            }
            else if (gf.ReferenceAlertSource == 3)
            {
                Reference_Source3.Checked = true;
            }
            else if (gf.ReferenceAlertSource == 4)
            {
                Reference_Source4.Checked = true;
            }
            else
            {
                Reference_Source0.Checked = true;
            }
            if (gf.KeyBoardOption == 0)
            {
                rbKeyBoardOpt0.Checked = true;
            }
            else
            {
                rbKeyBoardOpt1.Checked = true;
            }

            ///Global Hook F7 F8
            ChkGlobalHookF7.Checked = gf.GlobalHookKey_F7;
            ChkGlobalHookF8.Checked = gf.GlobalHookKey_F8;

            ///Global Hook F9 F10
            ChkGlobalHookF9.Checked = gf.GlobalHookKey_F9;
            ChkGlobalHookF10.Checked = gf.GlobalHookKey_F10;

            ChkGlobalHookArrow.Checked = gf.GlobalHookKey_Arrow;
            ChkGlobalHookCtrlArrow.Checked = gf.GlobalHookKey_CtrlArrow;



        }

        //private void LoadGeneralSetting()
        //{
        //	VersesMaxUpDown.Value = gf.HB_MaxVersesSelection;
        //	AdhocVersesMaxUpDown.Value = gf.HB_MaxAdhocVersesSelection;
        //	PPMaxUpDown.Value = gf.PP_MaxFiles;
        //	EditHistoryMaxUpDown.Value = gf.MaxUserEditHistory;
        //	BuildMonitorsList();
        //	cbDisableScreenSaver.Checked = gf.DisableSreenSaver;
        //	VideoSizeUpDown1.Value = gf.VideoSize;
        //	string text = "";
        //	gf.AssignDropDownItem(SelectedMenuItemName: (gf.VideoVAlign == 0) ? Video_VAlignTop.Name : ((gf.VideoVAlign != 1) ? Video_VAlignBottom.Name : Video_VAlignCentre.Name), SelectedBtn: ref Video_VAlign, InMenuItem1: Video_VAlignTop, InMenuItem2: Video_VAlignCentre, InMenuItem3: Video_VAlignBottom);
        //	UpdateVideoSizeSample();
        //	DM_AlwaysUseSecondaryMonitor.Checked = gf.DMAlwaysUseSecondaryMonitor;
        //	if (gf.DualMonitorSelectAutoOption == 0)
        //	{
        //		optDM0.Checked = true;
        //	}
        //	else
        //	{
        //		optDM1.Checked = true;
        //	}
        //          // daniel
        //          // 스크린 옵션 추가
        //          optWide.Checked = (gf.isScreenWideMode)?true:false;

        //          DM1UpDownTop.Value = gf.DMOption1Top;
        //	DM1UpDownLeft.Value = gf.DMOption1Left;
        //	DM1UpDownWidth.Value = ((gf.DMOption1Width < 1) ? 1 : gf.DMOption1Width);
        //          DM1UpDownHeight.Value = ((gf.DMOption1Height < 1) ? 1 : gf.DMOption1Height);

        //          //DM1UpDownHeight.Value = ((gf.DMOption1Height < 1) ? 1 : gf.DMOption1Height);

        //          DM_CustomAsSingleMonitor.Checked = gf.DMOption1AsSingleMonitor;

        //          gf.GetScreenNumber(ref gf.OutputMonitorNumber, -1);

        //          DualMonitorList.SelectedIndex = gf.OutputMonitorNumber;

        //          LM_AlwaysUse.Checked = gf.LMAlwaysUseSecondaryMonitor;
        //	if (gf.LMSelectAutoOption == 1)
        //	{
        //		optLM1.Checked = true;
        //	}
        //	else
        //	{
        //		optLM0.Checked = true;
        //	}
        //	LM1UpDownTop.Value = gf.LMOption1Top;
        //	LM1UpDownLeft.Value = gf.LMOption1Left;
        //	LM1UpDownWidth.Value = ((gf.LMOption1Width < 1) ? 1 : gf.LMOption1Width);
        //	LM1UpDownHeight.Value = ((gf.LMOption1Height < 1) ? 1 : gf.LMOption1Height);
        //          /// gf.DualMonitorSelectAutoOption :  
        //          gf.GetScreenNumber(ref gf.LyricsMonitorNumber, (gf.DualMonitorSelectAutoOption == 0) ? gf.OutputMonitorNumber : (-1));

        //	LyricsMonitorList.SelectedIndex = gf.LyricsMonitorNumber;

        //	btnLMTextColour.ForeColor = gf.LMTextColour;
        //	btnLMHighlightColour.ForeColor = gf.LMHighlightColour;
        //	btnLMBackColour.ForeColor = gf.LMBackColour;
        //	PreviewFontUpDown.Value = ((gf.PreviewArea_FontSize >= 8) ? gf.PreviewArea_FontSize : 8);
        //	LMUpDownFontSize.Value = ((gf.LMMainFontSize >= 8 && gf.LMMainFontSize <= 40) ? gf.LMMainFontSize : 20);
        //	LMNotationsUpDownFontSize.Value = ((gf.LMNotationsFontSize >= 8 && gf.LMNotationsFontSize <= 40) ? gf.LMNotationsFontSize : 20);
        //	cbLMShowNotations.Checked = gf.LMShowNotations;
        //	LM_Bold.Checked = gf.LMFontBold;
        //	LM_Italic.Checked = gf.LMFontItalic;
        //	LM_Underline.Checked = gf.LMFontUnderline;
        //	btnTextRegionChangeColour.ForeColor = gf.FocusedTextRegionColour;
        //	btnTextRegionSlideTextColour.ForeColor = gf.TextRegionSlideTextColour;
        //	btnTextRegionSlideBackColour.ForeColor = gf.TextRegionSlideBackColour;
        //	TextRegionUseColour.Checked = gf.UseFocusedTextRegionColour;
        //	checkBoxPPTab.Checked = gf.UsePowerpointTab;
        //	checkBoxPPNoPanel.Checked = gf.NoPowerpointPanelOverlay;
        //	checkBoxMediaTab.Checked = gf.UseMediaTab;
        //	checkBoxMediaNoPanel.Checked = gf.NoMediaPanelOverlay;
        //	checkBoxLMBox.Checked = gf.ShowLyricsMonitorAlertBox;
        //	cbUseLargestFont.Checked = gf.UseLargestFontSize;
        //	cbAutoTextOverflow.Checked = gf.AutoTextOverflow;
        //	cbAdvanceNextItem.Checked = gf.AdvanceNextItem;
        //	cbLineBetweenRegions.Checked = gf.LineBetweenRegions;
        //	cbWordWrapLeftAlignIndent.Checked = gf.WordWrapLeftAlignIndent;
        //	if (gf.GapItemOption == GapType.Black)
        //	{
        //		rbGapItemOption1.Checked = true;
        //	}
        //	else if (gf.GapItemOption == GapType.Default)
        //	{
        //		rbGapItemOption2.Checked = true;
        //	}
        //	else if (gf.GapItemOption == GapType.User)
        //	{
        //		rbGapItemOption3.Checked = true;
        //	}
        //	else
        //	{
        //		rbGapItemOption0.Checked = true;
        //	}
        //	tbGapLogoLocation.Text = gf.GapItemLogoFile;
        //	cbGapItemUseFade.Checked = gf.GapItemUseFade;
        //	NotationFontFactorUpDown.Value = (int)(gf.NotationFontFactor * 100.0);
        //	cbPreviewShowNotations.Checked = gf.PreviewArea_ShowNotations;
        //	cbLineBetweenScreens.Checked = gf.PreviewArea_LineBetweenScreens;
        //	PreviewFontUpDown.Value = ((gf.PreviewArea_FontSize >= 8) ? gf.PreviewArea_FontSize : 8);
        //	tbMusicLocation.Text = gf.MediaDir;
        //	tbMusicLocation.BackColor = tbGapLogoLocation.BackColor;
        //	gf.LoadBlankCaptureDevices(ref cbCaptureDevices);
        //	if (gf.WMP_Present)
        //	{
        //		try
        //		{
        //			DShowLib dShowLib = new DShowLib();
        //			dShowLib.ListCaptureDevices(ref cbCaptureDevices);
        //		}
        //		catch
        //		{
        //		}
        //	}
        //	cbCaptureDevices.SelectedIndex = gf.LiveCamNumber - 1;
        //	TrackBarVolume.Value = (((gf.LiveCamVolume >= 0) & (gf.LiveCamVolume <= 100)) ? gf.LiveCamVolume : 30);
        //	TrackBarBalance.Value = (((gf.LiveCamBalance >= -100) & (gf.LiveCamBalance <= 100)) ? gf.LiveCamBalance : 0);
        //	cbMute.Checked = gf.LiveCamMute;
        //	cbWidescreen.Checked = gf.LiveCamWidescreen;
        //	checkBoxLiveCamNoPanel.Checked = gf.LiveCamNoPanelOverlay;
        //	MessageAlertDurationUpDown.Value = gf.MessageAlertDuration;
        //	Message_Scroll.Checked = gf.MessageAlertScroll;
        //	Message_Flash.Checked = gf.MessageAlertFlash;
        //	Message_Transparent.Checked = gf.MessageAlertTransparent;
        //	MessageComboFont.Text = gf.MessageAlertFontName;
        //	MessageSizeUpDown.Value = gf.MessageAlertFontSize;
        //	btnMessageChangeTextColour.ForeColor = gf.MessageAlertTextColour;
        //	btnMessageChangeBackColour.ForeColor = gf.MessageAlertBackColour;
        //	gf.AssignDropDownItem(SelectedMenuItemName: (gf.MessageAlertTextAlign == 1) ? Message_AlignLeft.Name : ((gf.MessageAlertTextAlign != 2) ? Message_AlignRight.Name : Message_AlignCentre.Name), SelectedBtn: ref Message_Align, InMenuItem1: Message_AlignLeft, InMenuItem2: Message_AlignCentre, InMenuItem3: Message_AlignRight);
        //	gf.AssignDropDownItem(SelectedMenuItemName: (gf.MessageAlertVerticalAlign != 0) ? Message_VAlignBottom.Name : Message_VAlignTop.Name, SelectedBtn: ref Message_VAlign, InMenuItem1: Message_VAlignTop, InMenuItem2: Message_VAlignBottom);
        //	Message_Bold.Checked = gf.MessageAlertBold;
        //	Message_Italics.Checked = gf.MessageAlertItalic;
        //	Message_Underline.Checked = gf.MessageAlertUnderline;
        //	Message_Shadow.Checked = gf.MessageAlertShadow;
        //	Message_Outline.Checked = gf.MessageAlertOutline;
        //	ParentalAlertUpDown.Value = gf.ParentalAlertDuration;
        //	Parental_Scroll.Checked = gf.ParentalAlertScroll;
        //	Parental_Flash.Checked = gf.ParentalAlertFlash;
        //	Parental_Transparent.Checked = gf.ParentalAlertTransparent;
        //	ParentalComboFont.Text = gf.ParentalAlertFontName;
        //	ParentalSizeUpDown.Value = gf.ParentalAlertFontSize;
        //	btnParentalChangeTextColour.ForeColor = gf.ParentalAlertTextColour;
        //	btnParentalChangeBackColour.ForeColor = gf.ParentalAlertBackColour;
        //	ParentalAlert.Text = gf.ParentalAlertHeading;
        //	gf.AssignDropDownItem(SelectedMenuItemName: (gf.ParentalAlertTextAlign == 1) ? Parental_AlignLeft.Name : ((gf.ParentalAlertTextAlign != 2) ? Parental_AlignRight.Name : Parental_AlignCentre.Name), SelectedBtn: ref Parental_Align, InMenuItem1: Parental_AlignLeft, InMenuItem2: Parental_AlignCentre, InMenuItem3: Parental_AlignRight);
        //	gf.AssignDropDownItem(SelectedMenuItemName: (gf.ParentalAlertVerticalAlign != 0) ? Parental_VAlignBottom.Name : Parental_VAlignTop.Name, SelectedBtn: ref Parental_VAlign, InMenuItem1: Parental_VAlignTop, InMenuItem2: Parental_VAlignBottom);
        //	Parental_Bold.Checked = gf.ParentalAlertBold;
        //	Parental_Italics.Checked = gf.ParentalAlertItalic;
        //	Parental_Underline.Checked = gf.ParentalAlertUnderline;
        //	Parental_Shadow.Checked = gf.ParentalAlertShadow;
        //	Parental_Outline.Checked = gf.ParentalAlertOutline;
        //	ReferenceAlertDurationUpDown.Value = gf.ReferenceAlertDuration;
        //	Reference_Scroll.Checked = gf.ReferenceAlertScroll;
        //	Reference_Flash.Checked = gf.ReferenceAlertFlash;
        //	Reference_Transparent.Checked = gf.ReferenceAlertTransparent;
        //	ReferenceComboFont.Text = gf.ReferenceAlertFontName;
        //	ReferenceSizeUpDown.Value = gf.ReferenceAlertFontSize;
        //	btnReferenceChangeTextColour.ForeColor = gf.ReferenceAlertTextColour;
        //	btnReferenceChangeBackColour.ForeColor = gf.ReferenceAlertBackColour;
        //	gf.AssignDropDownItem(SelectedMenuItemName: (gf.ReferenceAlertTextAlign == 1) ? Reference_AlignLeft.Name : ((gf.ReferenceAlertTextAlign != 2) ? Reference_AlignRight.Name : Reference_AlignCentre.Name), SelectedBtn: ref Reference_Align, InMenuItem1: Reference_AlignLeft, InMenuItem2: Reference_AlignCentre, InMenuItem3: Reference_AlignRight);
        //	gf.AssignDropDownItem(SelectedMenuItemName: (gf.ReferenceAlertVerticalAlign == 0) ? Reference_VAlignTop.Name : ((gf.ReferenceAlertVerticalAlign != 2) ? Reference_VAlignCentre.Name : Reference_VAlignBottom.Name), SelectedBtn: ref Reference_VAlign, InMenuItem1: Reference_VAlignTop, InMenuItem2: Reference_VAlignCentre, InMenuItem3: Reference_VAlignBottom);
        //	Reference_Bold.Checked = gf.ReferenceAlertBold;
        //	Reference_Italics.Checked = gf.ReferenceAlertItalic;
        //	Reference_Underline.Checked = gf.ReferenceAlertUnderline;
        //	Reference_Shadow.Checked = gf.ReferenceAlertShadow;
        //	Reference_Outline.Checked = gf.ReferenceAlertOutline;
        //	cbPick.Checked = gf.ReferenceAlertUsePick;
        //	cbPickBlank.Checked = gf.ReferenceAlertBlankIfPickNotFound;
        //	tbPick.Text = gf.ReferenceAlertPickName;
        //	tbSubstitute.Text = gf.ReferenceAlertPickSubstitute;
        //	if (gf.ReferenceAlertSource == 1)
        //	{
        //		Reference_Source1.Checked = true;
        //	}
        //	else if (gf.ReferenceAlertSource == 2)
        //	{
        //		Reference_Source2.Checked = true;
        //	}
        //	else if (gf.ReferenceAlertSource == 3)
        //	{
        //		Reference_Source3.Checked = true;
        //	}
        //	else if (gf.ReferenceAlertSource == 4)
        //	{
        //		Reference_Source4.Checked = true;
        //	}
        //	else
        //	{
        //		Reference_Source0.Checked = true;
        //	}
        //	if (gf.KeyBoardOption == 0)
        //	{
        //		rbKeyBoardOpt0.Checked = true;
        //	}
        //	else
        //	{
        //		rbKeyBoardOpt1.Checked = true;
        //	}
        //}

        private void optDM_CheckedChanged(object sender, EventArgs e)
        {
            UpdateOptFields(1);
        }

        private void optLM_CheckedChanged(object sender, EventArgs e)
        {
            UpdateOptFields(2);
        }

        private void UpdateOptFields(int MonitorType)
        {
            if (MonitorType == 1)
            {
                DualMonitorList.Enabled = (optDM0.Checked ? true : false);
                DM_AlwaysUseSecondaryMonitor.Enabled = (optDM0.Checked ? true : false);
                DM1UpDownTop.Enabled = (optDM1.Checked ? true : false);
                DM1UpDownLeft.Enabled = (optDM1.Checked ? true : false);
                DM1UpDownWidth.Enabled = (optDM1.Checked ? true : false);
                DM_CustomAsSingleMonitor.Enabled = (optDM1.Checked ? true : false);
                groupBoxDM.Enabled = !gf.ShowRunning;
            }
            else
            {
                LyricsMonitorList.Enabled = (optLM0.Checked ? true : false);
                LM_AlwaysUse.Enabled = (optLM0.Checked ? true : false);
                LM1UpDownTop.Enabled = (optLM1.Checked ? true : false);
                LM1UpDownLeft.Enabled = (optLM1.Checked ? true : false);
                LM1UpDownWidth.Enabled = (optLM1.Checked ? true : false);
                groupBoxLM.Enabled = !gf.ShowRunning;
            }
        }

        private void DM1UpDownWidth_ValueChanged(object sender, EventArgs e)
        {
            //if (DM1UpDownWidth.Value > 1m)
            //{
            //	DM1UpDownHeight.Value = (int)DM1UpDownWidth.Value * 3 / 4;
            //}
            //else
            //{
            //	DM1UpDownHeight.Value = 1m;
            //}

            //wide mode가 아닐 경우
            if (!gf.isScreenWideMode)
                gf.DMOption1Height = (int)DM1UpDownWidth.Value * 3 / 4;
        }

        private void LM1UpDownWidth_ValueChanged(object sender, EventArgs e)
        {
            if (LM1UpDownWidth.Value > 1m)
            {
                LM1UpDownHeight.Value = (int)LM1UpDownWidth.Value * 3 / 4;
            }
            else
            {
                LM1UpDownHeight.Value = 1m;
            }
        }

        private void btnLMTextColour_Click(object sender, EventArgs e)
        {
            Color ColourSymbol = btnLMTextColour.ForeColor;
            gf.SelectColorFromBtn(ref btnLMTextColour, ref ColourSymbol);
        }

        private void btnLMHighlightColour_Click(object sender, EventArgs e)
        {
            Color ColourSymbol = btnLMHighlightColour.ForeColor;
            gf.SelectColorFromBtn(ref btnLMHighlightColour, ref ColourSymbol);
        }

        private void btnLMBackColour_Click(object sender, EventArgs e)
        {
            Color ColourSymbol = btnLMBackColour.ForeColor;
            gf.SelectColorFromBtn(ref btnLMBackColour, ref ColourSymbol);
        }

        public void BuildMonitorsList()
        {
            Screen[] allScreens = Screen.AllScreens;

            DualMonitorList.Items.Clear();
            LyricsMonitorList.Items.Clear();

            int nNumber = 0;

            foreach (Screen screen in allScreens)
            {
                if (screen != null)
                {
                    //DualMonitorList.Items.Add($"{screen.DeviceName}, {GetMonitor_Name(nNumber)}");
                    //if (!screen.Primary)
                    //{
                    //    LyricsMonitorList.Items.Add($"{screen.DeviceName}, {GetMonitor_Name(nNumber)}");
                    //}

                    DualMonitorList.Items.Add($"{screen.DeviceName}");
                    if (!screen.Primary)
                    {
                        LyricsMonitorList.Items.Add($"{screen.DeviceName}");
                    }
                }
                nNumber++;
            }
            LyricsMonitorList.Items.Add("None");
        }

        //public void BuildMonitorsList_Old()
        //{
        //    int maxScreen = gf.GetMaxScreen();

        //    DualMonitorList.Items.Clear();

        //    DualMonitorList.Items.Add("Primary Monitor (Single Monitor)");
        //    LyricsMonitorList.Items.Clear();
        //    LyricsMonitorList.Items.Add("None");
        //    for (int i = 1; i <= maxScreen - 1; i++)
        //    {
        //        DualMonitorList.Items.Add("Secondary Monitor " + i);
        //        LyricsMonitorList.Items.Add("Secondary Monitor " + i);
        //    }
        //}

        private void Monitor_Info_Click(object sender, EventArgs e)
        {
            if (DualMonitorList.Items.Count > 0)
            {
                int OutTop = 0;
                int OutLeft = 0;
                int OutWidth = 0;
                int OutHeight = 0;
                gf.GetScreenInfo(DualMonitorList.SelectedItem.ToString(), ref OutTop, ref OutLeft, ref OutWidth, ref OutHeight);
                //gf.GetScreenInfo(DualMonitorList.SelectedIndex, ref OutTop, ref OutLeft, ref OutWidth, ref OutHeight);
                MessageBox.Show(DualMonitorList.Text + " Setup: \nTop:\t" + Convert.ToString(OutTop) + "\nLeft:\t" + Convert.ToString(OutLeft) + "\nWidth:\t" + Convert.ToString(OutWidth) + "\nHeight:\t" + Convert.ToString(OutHeight));
            }
        }

        private string GetMonitor_Name(int nNumberOfDevicesPresent)
        {
            Guid GUID_DEVCLASS_MONITOR = new Guid("4D36E96E-E325-11CE-BFC1-08002BE10318");

            string strDeviceName = "";

            IntPtr hDeviceInfo = DiNative.SetupDiGetClassDevs(ref GUID_DEVCLASS_MONITOR, 0, IntPtr.Zero, DiNative.DIGCF_PRESENT);
            if (hDeviceInfo != IntPtr.Zero)
            {
                var DeviceInfoData = new DiNative.SP_DEVINFO_DATA() { cbSize = Marshal.SizeOf(typeof(DiNative.SP_DEVINFO_DATA)) };
                DiNative.SetupDiEnumDeviceInfo(hDeviceInfo, (uint)nNumberOfDevicesPresent, DeviceInfoData);

                StringBuilder sbDeviceName = new StringBuilder(260);
                sbDeviceName.Capacity = 260;

                bool bRet = DiNative.SetupDiGetDeviceRegistryProperty(hDeviceInfo, DeviceInfoData, DiNative.SPDRP_FRIENDLYNAME, 0, sbDeviceName, (uint)sbDeviceName.Capacity, IntPtr.Zero);
                if (bRet)
                {
                    strDeviceName = sbDeviceName.ToString();
                }
                else
                {
                    bRet = DiNative.SetupDiGetDeviceRegistryProperty(hDeviceInfo, DeviceInfoData, DiNative.SPDRP_DEVICEDESC, 0, sbDeviceName, (uint)sbDeviceName.Capacity, IntPtr.Zero);
                    if (bRet)
                    {
                        strDeviceName = sbDeviceName.ToString();
                    }
                }
            }

            return strDeviceName;
        }

        private void LyricsMonitor_Info_Click(object sender, EventArgs e)
        {
            if (LyricsMonitorList.Items.Count > 0)
            {
                int OutTop = 0;
                int OutLeft = 0;
                int OutWidth = 0;
                int OutHeight = 0;
                Screen[] screens = Screen.AllScreens;
                if (screens[LyricsMonitorList.SelectedIndex].Primary)
                {
                    MessageBox.Show("No Lyrics Monitor");
                    return;
                }
                gf.GetScreenInfo(LyricsMonitorList.SelectedItem.ToString(), ref OutTop, ref OutLeft, ref OutWidth, ref OutHeight);
                //gf.GetScreenInfo(LyricsMonitorList.SelectedIndex, ref OutTop, ref OutLeft, ref OutWidth, ref OutHeight);
                MessageBox.Show(LyricsMonitorList.Text + " Setup: \nTop:\t" + Convert.ToString(OutTop) + "\nLeft:\t" + Convert.ToString(OutLeft) + "\nWidth:\t" + Convert.ToString(OutWidth) + "\nHeight:\t" + Convert.ToString(OutHeight));
            }
        }

        private void btnTextRegionChangeColour_Click(object sender, EventArgs e)
        {
            Color ColourSymbol = btnTextRegionChangeColour.ForeColor;
            gf.SelectColorFromBtn(ref btnTextRegionChangeColour, ref ColourSymbol);
        }

        private void btnTextRegionSlideTextColour_Click(object sender, EventArgs e)
        {
            Color ColourSymbol = btnTextRegionSlideTextColour.ForeColor;
            gf.SelectColorFromBtn(ref btnTextRegionSlideTextColour, ref ColourSymbol);
        }

        private void btnTextRegionSlideBackColour_Click(object sender, EventArgs e)
        {
            Color ColourSymbol = btnTextRegionSlideBackColour.ForeColor;
            gf.SelectColorFromBtn(ref btnTextRegionSlideBackColour, ref ColourSymbol);
        }

        private void Message_Align_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            gf.AssignDropDownItem(ref Message_Align, e.ClickedItem.Name, Message_AlignLeft, Message_AlignCentre, Message_AlignRight);
        }

        private void Message_VAlign_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            gf.AssignDropDownItem(ref Message_VAlign, e.ClickedItem.Name, Message_VAlignTop, Message_VAlignBottom);
        }

        private void Parental_Align_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            gf.AssignDropDownItem(ref Parental_Align, e.ClickedItem.Name, Parental_AlignLeft, Parental_AlignCentre, Parental_AlignRight);
        }

        private void Parental_VAlign_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            gf.AssignDropDownItem(ref Parental_VAlign, e.ClickedItem.Name, Parental_VAlignTop, Parental_VAlignBottom);
        }

        private void Reference_Align_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            gf.AssignDropDownItem(ref Reference_Align, e.ClickedItem.Name, Reference_AlignLeft, Reference_AlignCentre, Reference_AlignRight);
        }

        private void Reference_VAlign_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            gf.AssignDropDownItem(ref Reference_VAlign, e.ClickedItem.Name, Reference_VAlignTop, Reference_VAlignCentre, Reference_VAlignBottom);
        }

        private void btnParentalChangeTextColour_Click(object sender, EventArgs e)
        {
            Color ColourSymbol = btnParentalChangeTextColour.ForeColor;
            gf.SelectColorFromBtn(ref btnParentalChangeTextColour, ref ColourSymbol);
        }

        private void btnParentalChangeBackColour_Click(object sender, EventArgs e)
        {
            Color ColourSymbol = btnParentalChangeBackColour.ForeColor;
            gf.SelectColorFromBtn(ref btnParentalChangeBackColour, ref ColourSymbol);
        }

        private void btnMessageChangeTextColour_Click(object sender, EventArgs e)
        {
            Color ColourSymbol = btnMessageChangeTextColour.ForeColor;
            gf.SelectColorFromBtn(ref btnMessageChangeTextColour, ref ColourSymbol);
        }

        private void btnMessageChangeBackColour_Click(object sender, EventArgs e)
        {
            Color ColourSymbol = btnMessageChangeBackColour.ForeColor;
            gf.SelectColorFromBtn(ref btnMessageChangeBackColour, ref ColourSymbol);
        }

        private void btnReferenceChangeTextColour_Click(object sender, EventArgs e)
        {
            Color ColourSymbol = btnReferenceChangeTextColour.ForeColor;
            gf.SelectColorFromBtn(ref btnReferenceChangeTextColour, ref ColourSymbol);
        }

        private void btnReferenceChangeBackColour_Click(object sender, EventArgs e)
        {
            Color ColourSymbol = btnReferenceChangeBackColour.ForeColor;
            gf.SelectColorFromBtn(ref btnReferenceChangeBackColour, ref ColourSymbol);
        }

        private void ShowHeadingsPercentSizeUpDown_ValueChanged(object sender, EventArgs e)
        {
            tempFolderHeadingPercentSize[CurFolder] = (int)ShowHeadingsPercentSizeUpDown.Value;
        }

        private void rbHeadingFontSettings_CheckedChanged(object sender, EventArgs e)
        {
            Control control = (Control)sender;
            if (Convert.ToInt32(control.Tag) == 0)
            {
                tempFolderHeadingOption[CurFolder] = 0;
            }
            else if (Convert.ToInt32(control.Tag) == 1)
            {
                tempFolderHeadingOption[CurFolder] = 1;
            }
            else if (Convert.ToInt32(control.Tag) == 2)
            {
                tempFolderHeadingOption[CurFolder] = 2;
            }
        }

        private void KyTextBoxesColourBackground()
        {
            Color backColor = AdminLicNo3.BackColor;
            Color backColor2 = BackColor;
            kbAction0.BackColor = backColor;
            kbAction1.BackColor = backColor;
            kbAction2.BackColor = backColor;
            kbAction3.BackColor = backColor;
            kbAction4.BackColor = backColor;
            kbAction5.BackColor = backColor;
            kbAction6.BackColor = backColor;
            kbAction7.BackColor = backColor;
            kbSelect00.BackColor = backColor2;
            kbSelect01.BackColor = backColor2;
            kbSelect02.BackColor = backColor2;
            kbSelect03.BackColor = backColor2;
            kbSelect04.BackColor = backColor2;
            kbSelect05.BackColor = backColor2;
            kbSelect06.BackColor = backColor2;
            kbSelect07.BackColor = backColor2;
            kbSelect10.BackColor = backColor2;
            kbSelect11.BackColor = backColor2;
            kbSelect12.BackColor = backColor2;
            kbSelect13.BackColor = backColor2;
            kbSelect14.BackColor = backColor2;
            kbSelect15.BackColor = backColor2;
            kbSelect16.BackColor = backColor2;
            kbSelect17.BackColor = backColor2;
            if (rbKeyBoardOpt0.Checked)
            {
                kbSelect00.BackColor = backColor;
                kbSelect01.BackColor = backColor;
                kbSelect02.BackColor = backColor;
                kbSelect03.BackColor = backColor;
                kbSelect04.BackColor = backColor;
                kbSelect05.BackColor = backColor;
                kbSelect06.BackColor = backColor;
                kbSelect07.BackColor = backColor;
            }
            else
            {
                kbSelect10.BackColor = backColor;
                kbSelect11.BackColor = backColor;
                kbSelect12.BackColor = backColor;
                kbSelect13.BackColor = backColor;
                kbSelect14.BackColor = backColor;
                kbSelect15.BackColor = backColor;
                kbSelect16.BackColor = backColor;
                kbSelect17.BackColor = backColor;
            }
        }

        private void rbKeyBoardOpt0_CheckedChanged(object sender, EventArgs e)
        {
            KyTextBoxesColourBackground();
        }

        private void MusicLocationBtn_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.SelectedPath = ((DataUtil.Trim(tbMusicLocation.Text) != "") ? DataUtil.Trim(tbMusicLocation.Text) : gf.MediaDir);
            folderBrowserDialog1.Description = "Select Folder where media files for the lyrics are held";
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                tbMusicLocation.Text = folderBrowserDialog1.SelectedPath;
                if (DataUtil.Right(tbMusicLocation.Text, 1) != "\\")
                {
                    tbMusicLocation.Text += "\\";
                }
            }
        }

        private void GapLogoLocationBtn_Click(object sender, EventArgs e)
        {
            tbGapLogoLocation.Text = DataUtil.Trim(tbGapLogoLocation.Text);
            string text = (tbGapLogoLocation.Text != "") ? Path.GetDirectoryName(tbGapLogoLocation.Text) : "";
            if (text == "")
            {
                text = gf.ImagesDir;
            }
            OpenFileDialog1.Filter = "Image Files (*.jpg,*.jpeg,*.bmp,*.gif,*.ico)|*.jpg;*.jpeg;*.bmp;*.gif;*.ico";
            OpenFileDialog1.InitialDirectory = text;
            OpenFileDialog1.AddExtension = true;
            OpenFileDialog1.FileName = "";
            if (OpenFileDialog1.ShowDialog() == DialogResult.OK)
            {
                tbGapLogoLocation.Text = OpenFileDialog1.FileName;
                rbGapItemOption3.Checked = true;
            }
        }

        private void tbGapLogoLocation_TextChanged(object sender, EventArgs e)
        {
            if (!InitFormLoad)
            {
                rbGapItemOption3.Checked = true;
            }
        }

        private void VideoSizeUpDown1_ValueChanged(object sender, EventArgs e)
        {
            UpdateVideoSizeSample();
        }

        private void UpdateVideoSizeSample()
        {
            int num = panelVideoHolder.Width * (int)VideoSizeUpDown1.Value / 100;
            int num2 = num * 3 / 4;
            int num3 = (panelVideoHolder.Width - num) / 2 - 1;
            num3 = ((num3 >= 0) ? num3 : 0);
            panelVideoSize.Left = num3;
            panelVideoSize.Width = num;
            panelVideoSize.Height = num2;
            int num4;
            switch (DataUtil.ObjToInt(Video_VAlign.Tag))
            {
                default:
                    num4 = 0;
                    break;
                case 2:
                    num4 = panelVideoHolder.Height - num2;
                    break;
                case 1:
                    num4 = (panelVideoHolder.Height - num2) / 2;
                    break;
            }
            int top = num4;
            panelVideoSize.Top = top;
        }

        private void Video_VAlign_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            gf.AssignDropDownItem(ref Video_VAlign, e.ClickedItem.Name, Video_VAlignTop, Video_VAlignCentre, Video_VAlignBottom);
            UpdateVideoSizeSample();
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
            components = new Container();
            ComponentResourceManager resources = new ComponentResourceManager(typeof(FrmOptions));
            tabControl1 = new TabControl();
            tabPageMainWindow = new TabPage();
            groupBox24 = new GroupBox();
            checkBoxLMBox = new CheckBox();
            groupBox12 = new GroupBox();
            panelJump = new Panel();
            label56 = new Label();
            label55 = new Label();
            label10 = new Label();
            toolStripJump = new ToolStrip();
            toolStripJumpA = new ToolStripComboBox();
            toolStripSeparator1 = new ToolStripSeparator();
            toolStripJumpB = new ToolStripComboBox();
            toolStripSeparator2 = new ToolStripSeparator();
            toolStripJumpC = new ToolStripComboBox();
            groupBox22 = new GroupBox();
            checkBoxPPTab = new CheckBox();
            panel3 = new Panel();
            PPMaxUpDown = new NumericUpDown();
            label3 = new Label();
            groupBox7 = new GroupBox();
            btnTextRegionSlideBackColour = new Button();
            btnTextRegionSlideTextColour = new Button();
            btnTextRegionChangeColour = new Button();
            PreviewFontUpDown = new NumericUpDown();
            label37 = new Label();
            TextRegionUseColour = new CheckBox();
            cbPreviewShowNotations = new CheckBox();
            cbLineBetweenScreens = new CheckBox();
            groupBox6 = new GroupBox();
            checkBoxMediaTab = new CheckBox();
            panel5 = new Panel();
            toolStrip1 = new ToolStrip();
            MusicLocationBtn = new ToolStripButton();
            tbMusicLocation = new TextBox();
            groupBox1 = new GroupBox();
            panel4 = new Panel();
            EditHistoryMaxUpDown = new NumericUpDown();
            label4 = new Label();
            panel2 = new Panel();
            AdhocVersesMaxUpDown = new NumericUpDown();
            label2 = new Label();
            panel1 = new Panel();
            VersesMaxUpDown = new NumericUpDown();
            label1 = new Label();
            tabPageShow = new TabPage();
            label58 = new Label();
            groupBox5 = new GroupBox();
            cbDisableScreenSaver = new CheckBox();
            cbAdvanceNextItem = new CheckBox();
            checkBoxMediaNoPanel = new CheckBox();
            checkBoxPPNoPanel = new CheckBox();
            groupBox3 = new GroupBox();
            cbGapItemUseFade = new CheckBox();
            panel39 = new Panel();
            toolStrip10 = new ToolStrip();
            GapLogoLocationBtn = new ToolStripButton();
            tbGapLogoLocation = new TextBox();
            rbGapItemOption3 = new RadioButton();
            rbGapItemOption1 = new RadioButton();
            rbGapItemOption0 = new RadioButton();
            rbGapItemOption2 = new RadioButton();
            groupBox2 = new GroupBox();
            label51 = new Label();
            panel45 = new Panel();
            toolStripVideo = new ToolStrip();
            Video_VAlign = new ToolStripDropDownButton();
            Video_VAlignTop = new ToolStripMenuItem();
            Video_VAlignCentre = new ToolStripMenuItem();
            Video_VAlignBottom = new ToolStripMenuItem();
            VideoSizeUpDown1 = new NumericUpDown();
            label14 = new Label();
            panelVideoHolder = new Panel();
            panelVideoSize = new Panel();
            label50 = new Label();
            groupBox4 = new GroupBox();
            panel46 = new Panel();
            NotationFontFactorUpDown = new NumericUpDown();
            label54 = new Label();
            cbWordWrapLeftAlignIndent = new CheckBox();
            cbLineBetweenRegions = new CheckBox();
            cbUseLargestFont = new CheckBox();
            cbAutoTextOverflow = new CheckBox();
            tabPageMonitors = new TabPage();
            groupBox23 = new GroupBox();
            checkBoxLiveCamNoPanel = new CheckBox();
            panel47 = new Panel();
            toolStripCaptureDevices = new ToolStrip();
            cbCaptureDevices = new ToolStripComboBox();
            label53 = new Label();
            label5 = new Label();
            label7 = new Label();
            TrackBarBalance = new TrackBar();
            TrackBarVolume = new TrackBar();
            cbWidescreen = new CheckBox();
            cbMute = new CheckBox();
            label52 = new Label();
            groupBoxLM = new GroupBox();
            cbLMBroadcast = new CheckBox();
            btnLMHighlightColour = new Button();
            btnLMBackColour = new Button();
            btnLMTextColour = new Button();
            LM1UpDownLeft = new NumericUpDown();
            label23 = new Label();
            LM1UpDownTop = new NumericUpDown();
            label26 = new Label();
            LMNotationsUpDownFontSize = new NumericUpDown();
            panel48 = new Panel();
            toolStripLyricsMonitor = new ToolStrip();
            LM_Bold = new ToolStripButton();
            LM_Italic = new ToolStripButton();
            LM_Underline = new ToolStripButton();
            LM_AlwaysUse = new CheckBox();
            LMUpDownFontSize = new NumericUpDown();
            optLM0 = new RadioButton();
            panel43 = new Panel();
            toolStrip12 = new ToolStrip();
            LyricsMonitorList = new ToolStripComboBox();
            panel44 = new Panel();
            toolStrip13 = new ToolStrip();
            LyricsMonitor_Info = new ToolStripButton();
            LM1UpDownHeight = new NumericUpDown();
            LM1UpDownWidth = new NumericUpDown();
            optLM1 = new RadioButton();
            label25 = new Label();
            label48 = new Label();
            label9 = new Label();
            cbLMShowNotations = new CheckBox();
            label49 = new Label();
            groupBoxDM = new GroupBox();
            panel49 = new Panel();
            optWide = new RadioButton();
            optStandard = new RadioButton();
            DM_CustomAsSingleMonitor = new CheckBox();
            DM1UpDownHeight = new NumericUpDown();
            DM1UpDownLeft = new NumericUpDown();
            label44 = new Label();
            DM1UpDownWidth = new NumericUpDown();
            DM1UpDownTop = new NumericUpDown();
            optDM1 = new RadioButton();
            optDM0 = new RadioButton();
            panelDM = new Panel();
            toolStripMonitorList = new ToolStrip();
            DualMonitorList = new ToolStripComboBox();
            panelLinkTitle2Lookup = new Panel();
            toolStrip2 = new ToolStrip();
            Monitor_Info = new ToolStripButton();
            DM_AlwaysUseSecondaryMonitor = new CheckBox();
            label46 = new Label();
            label43 = new Label();
            label45 = new Label();
            tabPageAlerts = new TabPage();
            groupBox8 = new GroupBox();
            groupBox11 = new GroupBox();
            cbPickBlank = new CheckBox();
            tbSubstitute = new TextBox();
            cbPick = new CheckBox();
            tbPick = new TextBox();
            label42 = new Label();
            label24 = new Label();
            groupBox9 = new GroupBox();
            Reference_Source1 = new RadioButton();
            Reference_Source2 = new RadioButton();
            Reference_Source0 = new RadioButton();
            Reference_Source3 = new RadioButton();
            Reference_Source4 = new RadioButton();
            panel10 = new Panel();
            btnReferenceChangeBackColour = new Button();
            btnReferenceChangeTextColour = new Button();
            panel21 = new Panel();
            toolBarReferenceFormat = new ToolStrip();
            Reference_Scroll = new ToolStripButton();
            Reference_Flash = new ToolStripButton();
            Reference_Transparent = new ToolStripButton();
            Reference_Align = new ToolStripDropDownButton();
            Reference_AlignLeft = new ToolStripMenuItem();
            Reference_AlignCentre = new ToolStripMenuItem();
            Reference_AlignRight = new ToolStripMenuItem();
            Reference_VAlign = new ToolStripDropDownButton();
            Reference_VAlignTop = new ToolStripMenuItem();
            Reference_VAlignCentre = new ToolStripMenuItem();
            Reference_VAlignBottom = new ToolStripMenuItem();
            Reference_Bold = new ToolStripButton();
            Reference_Italics = new ToolStripButton();
            Reference_Underline = new ToolStripButton();
            Reference_Shadow = new ToolStripButton();
            Reference_Outline = new ToolStripButton();
            panel22 = new Panel();
            label38 = new Label();
            toolStrip5 = new ToolStrip();
            ReferenceComboFont = new ToolStripComboBox();
            panel33 = new Panel();
            ReferenceSizeUpDown = new NumericUpDown();
            label40 = new Label();
            panel36 = new Panel();
            ReferenceAlertDurationUpDown = new NumericUpDown();
            label41 = new Label();
            groupBox16 = new GroupBox();
            ParentalAlert = new TextBox();
            panel23 = new Panel();
            btnParentalChangeBackColour = new Button();
            btnParentalChangeTextColour = new Button();
            label30 = new Label();
            panel24 = new Panel();
            ToolBarParentalFormat = new ToolStrip();
            Parental_Scroll = new ToolStripButton();
            Parental_Flash = new ToolStripButton();
            Parental_Transparent = new ToolStripButton();
            Parental_Align = new ToolStripDropDownButton();
            Parental_AlignLeft = new ToolStripMenuItem();
            Parental_AlignCentre = new ToolStripMenuItem();
            Parental_AlignRight = new ToolStripMenuItem();
            Parental_VAlign = new ToolStripDropDownButton();
            Parental_VAlignTop = new ToolStripMenuItem();
            Parental_VAlignBottom = new ToolStripMenuItem();
            Parental_Bold = new ToolStripButton();
            Parental_Italics = new ToolStripButton();
            Parental_Underline = new ToolStripButton();
            Parental_Shadow = new ToolStripButton();
            Parental_Outline = new ToolStripButton();
            panel25 = new Panel();
            label27 = new Label();
            toolStrip14 = new ToolStrip();
            ParentalComboFont = new ToolStripComboBox();
            panel26 = new Panel();
            ParentalSizeUpDown = new NumericUpDown();
            label28 = new Label();
            panel27 = new Panel();
            ParentalAlertUpDown = new NumericUpDown();
            label29 = new Label();
            groupBox15 = new GroupBox();
            panel20 = new Panel();
            btnMessageChangeBackColour = new Button();
            btnMessageChangeTextColour = new Button();
            panel12 = new Panel();
            ToolBarMessageFormat = new ToolStrip();
            Message_Scroll = new ToolStripButton();
            Message_Flash = new ToolStripButton();
            Message_Transparent = new ToolStripButton();
            Message_Align = new ToolStripDropDownButton();
            Message_AlignLeft = new ToolStripMenuItem();
            Message_AlignCentre = new ToolStripMenuItem();
            Message_AlignRight = new ToolStripMenuItem();
            Message_VAlign = new ToolStripDropDownButton();
            Message_VAlignTop = new ToolStripMenuItem();
            Message_VAlignBottom = new ToolStripMenuItem();
            Message_Bold = new ToolStripButton();
            Message_Italics = new ToolStripButton();
            Message_Underline = new ToolStripButton();
            Message_Shadow = new ToolStripButton();
            Message_Outline = new ToolStripButton();
            panel19 = new Panel();
            label21 = new Label();
            toolStrip11 = new ToolStrip();
            MessageComboFont = new ToolStripComboBox();
            panel13 = new Panel();
            MessageSizeUpDown = new NumericUpDown();
            label22 = new Label();
            panel11 = new Panel();
            MessageAlertDurationUpDown = new NumericUpDown();
            label20 = new Label();
            tabPageFolders = new TabPage();
            SelectedFolderGroupBox = new GroupBox();
            groupBox14 = new GroupBox();
            ShowLineSpacing2MaxUpDown = new NumericUpDown();
            ShowLineSpacingMaxUpDown = new NumericUpDown();
            groupBox13 = new GroupBox();
            panel18 = new Panel();
            toolStrip3 = new ToolStrip();
            tbLyricsHeading3 = new ToolStripComboBox();
            panel16 = new Panel();
            toolStrip8 = new ToolStrip();
            tbLyricsHeading2 = new ToolStripComboBox();
            panel15 = new Panel();
            toolStrip7 = new ToolStrip();
            tbLyricsHeading1 = new ToolStripComboBox();
            label47 = new Label();
            panel14 = new Panel();
            toolStrip6 = new ToolStrip();
            tbLyricsHeading0 = new ToolStripComboBox();
            label13 = new Label();
            label12 = new Label();
            label11 = new Label();
            GroupBoxFont1 = new GroupBox();
            panel8 = new Panel();
            toolStrip4 = new ToolStrip();
            ComboFontName1 = new ToolStripComboBox();
            panel9 = new Panel();
            FontSizeUpDown1 = new NumericUpDown();
            ToolBarFontBtn1 = new ToolStrip();
            ToolBarFont_R2Bold = new ToolStripButton();
            ToolBarFont_R2Italics = new ToolStripDropDownButton();
            ToolBarFont_R2Italics0 = new ToolStripMenuItem();
            ToolBarFont_R2Italics1 = new ToolStripMenuItem();
            ToolBarFont_R2Italics2 = new ToolStripMenuItem();
            ToolBarFont_R2Underline = new ToolStripButton();
            ToolBarFont_R2RTL = new ToolStripButton();
            GroupBoxFont0 = new GroupBox();
            panelInd5 = new Panel();
            toolStripInd5 = new ToolStrip();
            ComboFontName0 = new ToolStripComboBox();
            panel7 = new Panel();
            FontSizeUpDown0 = new NumericUpDown();
            ToolBarFontBtn0 = new ToolStrip();
            ToolBarFont_R1Bold = new ToolStripButton();
            ToolBarFont_R1Italics = new ToolStripDropDownButton();
            ToolBarFont_R1Italics0 = new ToolStripMenuItem();
            ToolBarFont_R1Italics1 = new ToolStripMenuItem();
            ToolBarFont_R1Italics2 = new ToolStripMenuItem();
            ToolBarFont_R1Underline = new ToolStripButton();
            ToolBarFont_R1RTL = new ToolStripButton();
            groupBox10 = new GroupBox();
            FontPositionUpDown0 = new NumericUpDown();
            label17 = new Label();
            RightMarginUpDown = new NumericUpDown();
            LeftMarginUpDown = new NumericUpDown();
            FontPositionUpDownBottom = new NumericUpDown();
            FontPositionUpDown1 = new NumericUpDown();
            Sample_PanelMain = new Panel();
            panel38 = new Panel();
            SamplePanel_Region2 = new Panel();
            labelPreviewCentreBottom = new Label();
            panel37 = new Panel();
            SamplePanel_Region1 = new Panel();
            labelPreviewCentreTop = new Label();
            panel42 = new Panel();
            SamplePanel_Top = new Panel();
            panel40 = new Panel();
            panel41 = new Panel();
            SamplePanel_Left = new Panel();
            SamplePanel_Right = new Panel();
            label15 = new Label();
            label16 = new Label();
            label18 = new Label();
            label19 = new Label();
            GroupBoxHeadings = new GroupBox();
            panel17 = new Panel();
            toolStrip9 = new ToolStrip();
            ComboLyricsHeading = new ToolStripComboBox();
            panelInd4 = new Panel();
            HeadingsFontToolbar = new ToolStrip();
            HeadingsFont_Bold = new ToolStripButton();
            HeadingsFont_Italics = new ToolStripDropDownButton();
            HeadingsFont_Italics0 = new ToolStripMenuItem();
            HeadingsFont_Italics1 = new ToolStripMenuItem();
            HeadingsFont_Italics2 = new ToolStripMenuItem();
            HeadingsFont_Underline = new ToolStripButton();
            panel6 = new Panel();
            ShowHeadingsPercentSizeUpDown = new NumericUpDown();
            label6 = new Label();
            label8 = new Label();
            GroupBoxFolder = new GroupBox();
            SongFolder_Rename = new Button();
            cbFolderUse = new CheckBox();
            SongFolder = new ListView();
            columnHeader3 = new ColumnHeader();
            imageListSys = new ImageList(components);
            tabPageBibles = new TabPage();
            groupBox19 = new GroupBox();
            label32 = new Label();
            btnBibleAdd = new Button();
            btnBibleSearch = new Button();
            BibleSearchList = new ListView();
            columnHeader12 = new ColumnHeader();
            columnHeader13 = new ColumnHeader();
            columnHeader14 = new ColumnHeader();
            columnHeader15 = new ColumnHeader();
            groupBox17 = new GroupBox();
            btnBibleRemove = new Button();
            btnBibleNameChange = new Button();
            panel28 = new Panel();
            ToolBarBibles = new ToolStrip();
            Bibles_Info = new ToolStripButton();
            Bibles_Up = new ToolStripButton();
            Bibles_Down = new ToolStripButton();
            groupBox18 = new GroupBox();
            BibleFontSizeUpDown = new NumericUpDown();
            label31 = new Label();
            panel29 = new Panel();
            toolStrip16 = new ToolStrip();
            BibleAssociatedFolder = new ToolStripComboBox();
            BibleList = new ListView();
            columnHeader4 = new ColumnHeader();
            columnHeader5 = new ColumnHeader();
            columnHeader6 = new ColumnHeader();
            columnHeader7 = new ColumnHeader();
            columnHeader8 = new ColumnHeader();
            columnHeader9 = new ColumnHeader();
            tabPageLicence = new TabPage();
            groupBox20 = new GroupBox();
            cbEnforceDisplay = new CheckBox();
            panel32 = new Panel();
            AdminLicPreview8 = new TextBox();
            AdminLicPreview7 = new TextBox();
            AdminLicPreview6 = new TextBox();
            AdminLicPreview5 = new TextBox();
            AdminLicPreview4 = new TextBox();
            AdminLicPreview3 = new TextBox();
            AdminLicPreview2 = new TextBox();
            AdminLicPreview1 = new TextBox();
            label36 = new Label();
            panel31 = new Panel();
            AdminLicNo8 = new TextBox();
            AdminLicNo7 = new TextBox();
            AdminLicNo6 = new TextBox();
            AdminLicNo5 = new TextBox();
            AdminLicNo4 = new TextBox();
            AdminLicNo3 = new TextBox();
            AdminLicNo2 = new TextBox();
            AdminLicNo1 = new TextBox();
            label35 = new Label();
            tbNumberSymbol = new TextBox();
            panel30 = new Panel();
            AdminLic8 = new TextBox();
            AdminLic7 = new TextBox();
            AdminLic6 = new TextBox();
            AdminLic5 = new TextBox();
            AdminLic4 = new TextBox();
            AdminLic3 = new TextBox();
            AdminLic2 = new TextBox();
            AdminLic1 = new TextBox();
            label33 = new Label();
            label34 = new Label();
            tabPageKeyboard = new TabPage();
            ChkGlobalHookF7 = new CheckBox();
            ChkGlobalHookF8 = new CheckBox();
            ChkGlobalHookCtrlArrow = new CheckBox();
            ChkGlobalHookArrow = new CheckBox();
            label60 = new Label();
            ChkGlobalHookF10 = new CheckBox();
            ChkGlobalHookF9 = new CheckBox();
            groupBox21 = new GroupBox();
            panel34 = new Panel();
            kbSelect17 = new TextBox();
            kbSelect16 = new TextBox();
            kbSelect07 = new TextBox();
            kbSelect15 = new TextBox();
            kbSelect06 = new TextBox();
            kbSelect14 = new TextBox();
            kbSelect05 = new TextBox();
            kbSelect13 = new TextBox();
            kbSelect04 = new TextBox();
            kbSelect12 = new TextBox();
            kbSelect03 = new TextBox();
            kbSelect11 = new TextBox();
            kbSelect02 = new TextBox();
            kbSelect01 = new TextBox();
            kbSelect10 = new TextBox();
            kbSelect00 = new TextBox();
            rbKeyBoardOpt1 = new RadioButton();
            rbKeyBoardOpt0 = new RadioButton();
            panel35 = new Panel();
            kbAction7 = new TextBox();
            kbAction6 = new TextBox();
            kbAction5 = new TextBox();
            kbAction4 = new TextBox();
            kbAction3 = new TextBox();
            kbAction2 = new TextBox();
            kbAction1 = new TextBox();
            kbAction0 = new TextBox();
            label39 = new Label();
            label59 = new Label();
            label57 = new Label();
            BtnCancel = new Button();
            BtnOK = new Button();
            folderBrowserDialog1 = new FolderBrowserDialog();
            ColorDialog1 = new ColorDialog();
            OpenFileDialog1 = new OpenFileDialog();
            tabControl1.SuspendLayout();
            tabPageMainWindow.SuspendLayout();
            groupBox24.SuspendLayout();
            groupBox12.SuspendLayout();
            panelJump.SuspendLayout();
            toolStripJump.SuspendLayout();
            groupBox22.SuspendLayout();
            panel3.SuspendLayout();
            ((ISupportInitialize)PPMaxUpDown).BeginInit();
            groupBox7.SuspendLayout();
            ((ISupportInitialize)PreviewFontUpDown).BeginInit();
            groupBox6.SuspendLayout();
            panel5.SuspendLayout();
            toolStrip1.SuspendLayout();
            groupBox1.SuspendLayout();
            panel4.SuspendLayout();
            ((ISupportInitialize)EditHistoryMaxUpDown).BeginInit();
            panel2.SuspendLayout();
            ((ISupportInitialize)AdhocVersesMaxUpDown).BeginInit();
            panel1.SuspendLayout();
            ((ISupportInitialize)VersesMaxUpDown).BeginInit();
            tabPageShow.SuspendLayout();
            groupBox5.SuspendLayout();
            groupBox3.SuspendLayout();
            panel39.SuspendLayout();
            toolStrip10.SuspendLayout();
            groupBox2.SuspendLayout();
            panel45.SuspendLayout();
            toolStripVideo.SuspendLayout();
            ((ISupportInitialize)VideoSizeUpDown1).BeginInit();
            panelVideoHolder.SuspendLayout();
            panelVideoSize.SuspendLayout();
            groupBox4.SuspendLayout();
            panel46.SuspendLayout();
            ((ISupportInitialize)NotationFontFactorUpDown).BeginInit();
            tabPageMonitors.SuspendLayout();
            groupBox23.SuspendLayout();
            panel47.SuspendLayout();
            toolStripCaptureDevices.SuspendLayout();
            ((ISupportInitialize)TrackBarBalance).BeginInit();
            ((ISupportInitialize)TrackBarVolume).BeginInit();
            groupBoxLM.SuspendLayout();
            ((ISupportInitialize)LM1UpDownLeft).BeginInit();
            ((ISupportInitialize)LM1UpDownTop).BeginInit();
            ((ISupportInitialize)LMNotationsUpDownFontSize).BeginInit();
            panel48.SuspendLayout();
            toolStripLyricsMonitor.SuspendLayout();
            ((ISupportInitialize)LMUpDownFontSize).BeginInit();
            panel43.SuspendLayout();
            toolStrip12.SuspendLayout();
            panel44.SuspendLayout();
            toolStrip13.SuspendLayout();
            ((ISupportInitialize)LM1UpDownHeight).BeginInit();
            ((ISupportInitialize)LM1UpDownWidth).BeginInit();
            groupBoxDM.SuspendLayout();
            panel49.SuspendLayout();
            ((ISupportInitialize)DM1UpDownHeight).BeginInit();
            ((ISupportInitialize)DM1UpDownLeft).BeginInit();
            ((ISupportInitialize)DM1UpDownWidth).BeginInit();
            ((ISupportInitialize)DM1UpDownTop).BeginInit();
            panelDM.SuspendLayout();
            toolStripMonitorList.SuspendLayout();
            panelLinkTitle2Lookup.SuspendLayout();
            toolStrip2.SuspendLayout();
            tabPageAlerts.SuspendLayout();
            groupBox8.SuspendLayout();
            groupBox11.SuspendLayout();
            groupBox9.SuspendLayout();
            panel10.SuspendLayout();
            panel21.SuspendLayout();
            toolBarReferenceFormat.SuspendLayout();
            panel22.SuspendLayout();
            toolStrip5.SuspendLayout();
            panel33.SuspendLayout();
            ((ISupportInitialize)ReferenceSizeUpDown).BeginInit();
            panel36.SuspendLayout();
            ((ISupportInitialize)ReferenceAlertDurationUpDown).BeginInit();
            groupBox16.SuspendLayout();
            panel23.SuspendLayout();
            panel24.SuspendLayout();
            ToolBarParentalFormat.SuspendLayout();
            panel25.SuspendLayout();
            toolStrip14.SuspendLayout();
            panel26.SuspendLayout();
            ((ISupportInitialize)ParentalSizeUpDown).BeginInit();
            panel27.SuspendLayout();
            ((ISupportInitialize)ParentalAlertUpDown).BeginInit();
            groupBox15.SuspendLayout();
            panel20.SuspendLayout();
            panel12.SuspendLayout();
            ToolBarMessageFormat.SuspendLayout();
            panel19.SuspendLayout();
            toolStrip11.SuspendLayout();
            panel13.SuspendLayout();
            ((ISupportInitialize)MessageSizeUpDown).BeginInit();
            panel11.SuspendLayout();
            ((ISupportInitialize)MessageAlertDurationUpDown).BeginInit();
            tabPageFolders.SuspendLayout();
            SelectedFolderGroupBox.SuspendLayout();
            groupBox14.SuspendLayout();
            ((ISupportInitialize)ShowLineSpacing2MaxUpDown).BeginInit();
            ((ISupportInitialize)ShowLineSpacingMaxUpDown).BeginInit();
            groupBox13.SuspendLayout();
            panel18.SuspendLayout();
            toolStrip3.SuspendLayout();
            panel16.SuspendLayout();
            toolStrip8.SuspendLayout();
            panel15.SuspendLayout();
            toolStrip7.SuspendLayout();
            panel14.SuspendLayout();
            toolStrip6.SuspendLayout();
            GroupBoxFont1.SuspendLayout();
            panel8.SuspendLayout();
            toolStrip4.SuspendLayout();
            panel9.SuspendLayout();
            ((ISupportInitialize)FontSizeUpDown1).BeginInit();
            ToolBarFontBtn1.SuspendLayout();
            GroupBoxFont0.SuspendLayout();
            panelInd5.SuspendLayout();
            toolStripInd5.SuspendLayout();
            panel7.SuspendLayout();
            ((ISupportInitialize)FontSizeUpDown0).BeginInit();
            ToolBarFontBtn0.SuspendLayout();
            groupBox10.SuspendLayout();
            ((ISupportInitialize)FontPositionUpDown0).BeginInit();
            ((ISupportInitialize)RightMarginUpDown).BeginInit();
            ((ISupportInitialize)LeftMarginUpDown).BeginInit();
            ((ISupportInitialize)FontPositionUpDownBottom).BeginInit();
            ((ISupportInitialize)FontPositionUpDown1).BeginInit();
            Sample_PanelMain.SuspendLayout();
            SamplePanel_Region2.SuspendLayout();
            SamplePanel_Region1.SuspendLayout();
            GroupBoxHeadings.SuspendLayout();
            panel17.SuspendLayout();
            toolStrip9.SuspendLayout();
            panelInd4.SuspendLayout();
            HeadingsFontToolbar.SuspendLayout();
            panel6.SuspendLayout();
            ((ISupportInitialize)ShowHeadingsPercentSizeUpDown).BeginInit();
            GroupBoxFolder.SuspendLayout();
            tabPageBibles.SuspendLayout();
            groupBox19.SuspendLayout();
            groupBox17.SuspendLayout();
            panel28.SuspendLayout();
            ToolBarBibles.SuspendLayout();
            groupBox18.SuspendLayout();
            ((ISupportInitialize)BibleFontSizeUpDown).BeginInit();
            panel29.SuspendLayout();
            toolStrip16.SuspendLayout();
            tabPageLicence.SuspendLayout();
            groupBox20.SuspendLayout();
            panel32.SuspendLayout();
            panel31.SuspendLayout();
            panel30.SuspendLayout();
            tabPageKeyboard.SuspendLayout();
            groupBox21.SuspendLayout();
            panel34.SuspendLayout();
            panel35.SuspendLayout();
            SuspendLayout();
            // 
            // tabControl1
            // 
            tabControl1.Controls.Add(tabPageMainWindow);
            tabControl1.Controls.Add(tabPageShow);
            tabControl1.Controls.Add(tabPageMonitors);
            tabControl1.Controls.Add(tabPageAlerts);
            tabControl1.Controls.Add(tabPageFolders);
            tabControl1.Controls.Add(tabPageBibles);
            tabControl1.Controls.Add(tabPageLicence);
            tabControl1.Controls.Add(tabPageKeyboard);
            tabControl1.Location = new Point(16, 19);
            tabControl1.Margin = new Padding(5, 4, 5, 4);
            tabControl1.Name = "tabControl1";
            tabControl1.Padding = new Point(10, 3);
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new Size(727, 571);
            tabControl1.TabIndex = 0;
            tabControl1.Tag = "";
            // 
            // tabPageMainWindow
            // 
            tabPageMainWindow.BackColor = SystemColors.Control;
            tabPageMainWindow.Controls.Add(groupBox24);
            tabPageMainWindow.Controls.Add(groupBox12);
            tabPageMainWindow.Controls.Add(groupBox22);
            tabPageMainWindow.Controls.Add(groupBox7);
            tabPageMainWindow.Controls.Add(groupBox6);
            tabPageMainWindow.Controls.Add(groupBox1);
            tabPageMainWindow.Location = new Point(4, 29);
            tabPageMainWindow.Margin = new Padding(5, 4, 5, 4);
            tabPageMainWindow.Name = "tabPageMainWindow";
            tabPageMainWindow.Padding = new Padding(5, 4, 5, 4);
            tabPageMainWindow.Size = new Size(719, 538);
            tabPageMainWindow.TabIndex = 0;
            tabPageMainWindow.Text = "Main Window";
            // 
            // groupBox24
            // 
            groupBox24.Controls.Add(checkBoxLMBox);
            groupBox24.Location = new Point(357, 207);
            groupBox24.Margin = new Padding(5, 4, 5, 4);
            groupBox24.Name = "groupBox24";
            groupBox24.Padding = new Padding(5, 4, 5, 4);
            groupBox24.Size = new Size(342, 72);
            groupBox24.TabIndex = 4;
            groupBox24.TabStop = false;
            groupBox24.Text = "Other Settings";
            // 
            // checkBoxLMBox
            // 
            checkBoxLMBox.AutoSize = true;
            checkBoxLMBox.Location = new Point(15, 32);
            checkBoxLMBox.Margin = new Padding(5, 4, 5, 4);
            checkBoxLMBox.Name = "checkBoxLMBox";
            checkBoxLMBox.Size = new Size(228, 24);
            checkBoxLMBox.TabIndex = 2;
            checkBoxLMBox.Text = "Show Lyrics Monitor Alert Box";
            // 
            // groupBox12
            // 
            groupBox12.Controls.Add(panelJump);
            groupBox12.Location = new Point(357, 287);
            groupBox12.Margin = new Padding(5, 4, 5, 4);
            groupBox12.Name = "groupBox12";
            groupBox12.Padding = new Padding(5, 4, 5, 4);
            groupBox12.Size = new Size(342, 160);
            groupBox12.TabIndex = 5;
            groupBox12.TabStop = false;
            groupBox12.Text = "Jump To Folder Icons";
            // 
            // panelJump
            // 
            panelJump.Controls.Add(label56);
            panelJump.Controls.Add(label55);
            panelJump.Controls.Add(label10);
            panelJump.Controls.Add(toolStripJump);
            panelJump.Location = new Point(16, 27);
            panelJump.Margin = new Padding(5, 4, 5, 4);
            panelJump.Name = "panelJump";
            panelJump.Size = new Size(264, 127);
            panelJump.TabIndex = 0;
            // 
            // label56
            // 
            label56.AutoSize = true;
            label56.Location = new Point(10, 93);
            label56.Margin = new Padding(5, 0, 5, 0);
            label56.Name = "label56";
            label56.Size = new Size(21, 20);
            label56.TabIndex = 11;
            label56.Text = "C.";
            // 
            // label55
            // 
            label55.AutoSize = true;
            label55.Location = new Point(10, 53);
            label55.Margin = new Padding(5, 0, 5, 0);
            label55.Name = "label55";
            label55.Size = new Size(21, 20);
            label55.TabIndex = 10;
            label55.Text = "B.";
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Location = new Point(10, 13);
            label10.Margin = new Padding(5, 0, 5, 0);
            label10.Name = "label10";
            label10.Size = new Size(22, 20);
            label10.TabIndex = 9;
            label10.Text = "A.";
            // 
            // toolStripJump
            // 
            toolStripJump.CanOverflow = false;
            toolStripJump.Dock = DockStyle.None;
            toolStripJump.GripStyle = ToolStripGripStyle.Hidden;
            toolStripJump.ImageScalingSize = new Size(24, 24);
            toolStripJump.Items.AddRange(new ToolStripItem[] { toolStripJumpA, toolStripSeparator1, toolStripJumpB, toolStripSeparator2, toolStripJumpC });
            toolStripJump.LayoutStyle = ToolStripLayoutStyle.VerticalStackWithOverflow;
            toolStripJump.Location = new Point(42, 7);
            toolStripJump.Name = "toolStripJump";
            toolStripJump.Padding = new Padding(0, 0, 2, 0);
            toolStripJump.RenderMode = ToolStripRenderMode.System;
            toolStripJump.Size = new Size(216, 98);
            toolStripJump.TabIndex = 5;
            // 
            // toolStripJumpA
            // 
            toolStripJumpA.AutoSize = false;
            toolStripJumpA.DropDownStyle = ComboBoxStyle.DropDownList;
            toolStripJumpA.MaxDropDownItems = 12;
            toolStripJumpA.Name = "toolStripJumpA";
            toolStripJumpA.Size = new Size(212, 28);
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(213, 6);
            // 
            // toolStripJumpB
            // 
            toolStripJumpB.DropDownStyle = ComboBoxStyle.DropDownList;
            toolStripJumpB.Name = "toolStripJumpB";
            toolStripJumpB.Size = new Size(211, 28);
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new Size(213, 6);
            // 
            // toolStripJumpC
            // 
            toolStripJumpC.DropDownStyle = ComboBoxStyle.DropDownList;
            toolStripJumpC.Name = "toolStripJumpC";
            toolStripJumpC.Size = new Size(211, 28);
            // 
            // groupBox22
            // 
            groupBox22.Controls.Add(checkBoxPPTab);
            groupBox22.Controls.Add(panel3);
            groupBox22.Location = new Point(8, 207);
            groupBox22.Margin = new Padding(5, 4, 5, 4);
            groupBox22.Name = "groupBox22";
            groupBox22.Padding = new Padding(5, 4, 5, 4);
            groupBox22.Size = new Size(336, 116);
            groupBox22.TabIndex = 1;
            groupBox22.TabStop = false;
            groupBox22.Text = "Powerpoint";
            // 
            // checkBoxPPTab
            // 
            checkBoxPPTab.AutoSize = true;
            checkBoxPPTab.Location = new Point(15, 76);
            checkBoxPPTab.Margin = new Padding(5, 4, 5, 4);
            checkBoxPPTab.Name = "checkBoxPPTab";
            checkBoxPPTab.Size = new Size(173, 24);
            checkBoxPPTab.TabIndex = 1;
            checkBoxPPTab.Text = "Show Powerpoint Tab";
            // 
            // panel3
            // 
            panel3.Controls.Add(PPMaxUpDown);
            panel3.Controls.Add(label3);
            panel3.Location = new Point(9, 28);
            panel3.Margin = new Padding(5, 4, 5, 4);
            panel3.Name = "panel3";
            panel3.Size = new Size(318, 39);
            panel3.TabIndex = 0;
            // 
            // PPMaxUpDown
            // 
            PPMaxUpDown.Location = new Point(239, 4);
            PPMaxUpDown.Margin = new Padding(5, 4, 5, 4);
            PPMaxUpDown.Name = "PPMaxUpDown";
            PPMaxUpDown.Size = new Size(72, 27);
            PPMaxUpDown.TabIndex = 0;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(5, 8);
            label3.Margin = new Padding(5, 0, 5, 0);
            label3.Name = "label3";
            label3.Size = new Size(209, 20);
            label3.TabIndex = 0;
            label3.Text = "Max Powerpoint Files allowed:";
            // 
            // groupBox7
            // 
            groupBox7.Controls.Add(btnTextRegionSlideBackColour);
            groupBox7.Controls.Add(btnTextRegionSlideTextColour);
            groupBox7.Controls.Add(btnTextRegionChangeColour);
            groupBox7.Controls.Add(PreviewFontUpDown);
            groupBox7.Controls.Add(label37);
            groupBox7.Controls.Add(TextRegionUseColour);
            groupBox7.Controls.Add(cbPreviewShowNotations);
            groupBox7.Controls.Add(cbLineBetweenScreens);
            groupBox7.Location = new Point(357, 21);
            groupBox7.Margin = new Padding(5, 4, 5, 4);
            groupBox7.Name = "groupBox7";
            groupBox7.Padding = new Padding(5, 4, 5, 4);
            groupBox7.Size = new Size(342, 176);
            groupBox7.TabIndex = 3;
            groupBox7.TabStop = false;
            groupBox7.Text = "Preview Text Areas";
            // 
            // btnTextRegionSlideBackColour
            // 
            btnTextRegionSlideBackColour.BackColor = Color.LightGray;
            btnTextRegionSlideBackColour.FlatStyle = FlatStyle.Flat;
            btnTextRegionSlideBackColour.Location = new Point(202, 123);
            btnTextRegionSlideBackColour.Margin = new Padding(5, 4, 5, 4);
            btnTextRegionSlideBackColour.Name = "btnTextRegionSlideBackColour";
            btnTextRegionSlideBackColour.Size = new Size(130, 39);
            btnTextRegionSlideBackColour.TabIndex = 7;
            btnTextRegionSlideBackColour.Text = "Selected Back";
            btnTextRegionSlideBackColour.UseVisualStyleBackColor = false;
            btnTextRegionSlideBackColour.Click += btnTextRegionSlideBackColour_Click;
            // 
            // btnTextRegionSlideTextColour
            // 
            btnTextRegionSlideTextColour.BackColor = Color.LightGray;
            btnTextRegionSlideTextColour.FlatStyle = FlatStyle.Flat;
            btnTextRegionSlideTextColour.Location = new Point(202, 83);
            btnTextRegionSlideTextColour.Margin = new Padding(5, 4, 5, 4);
            btnTextRegionSlideTextColour.Name = "btnTextRegionSlideTextColour";
            btnTextRegionSlideTextColour.Size = new Size(130, 39);
            btnTextRegionSlideTextColour.TabIndex = 6;
            btnTextRegionSlideTextColour.Text = "Selected Colour";
            btnTextRegionSlideTextColour.UseVisualStyleBackColor = false;
            btnTextRegionSlideTextColour.Click += btnTextRegionSlideTextColour_Click;
            // 
            // btnTextRegionChangeColour
            // 
            btnTextRegionChangeColour.BackColor = Color.LightGray;
            btnTextRegionChangeColour.FlatStyle = FlatStyle.Flat;
            btnTextRegionChangeColour.Location = new Point(202, 43);
            btnTextRegionChangeColour.Margin = new Padding(5, 4, 5, 4);
            btnTextRegionChangeColour.Name = "btnTextRegionChangeColour";
            btnTextRegionChangeColour.Size = new Size(130, 39);
            btnTextRegionChangeColour.TabIndex = 5;
            btnTextRegionChangeColour.Text = "Focused Colour";
            btnTextRegionChangeColour.UseVisualStyleBackColor = false;
            btnTextRegionChangeColour.Click += btnTextRegionChangeColour_Click;
            // 
            // PreviewFontUpDown
            // 
            PreviewFontUpDown.Location = new Point(88, 29);
            PreviewFontUpDown.Margin = new Padding(5, 4, 5, 4);
            PreviewFontUpDown.Maximum = new decimal(new int[] { 20, 0, 0, 0 });
            PreviewFontUpDown.Minimum = new decimal(new int[] { 8, 0, 0, 0 });
            PreviewFontUpDown.Name = "PreviewFontUpDown";
            PreviewFontUpDown.Size = new Size(72, 27);
            PreviewFontUpDown.TabIndex = 1;
            PreviewFontUpDown.Value = new decimal(new int[] { 8, 0, 0, 0 });
            // 
            // label37
            // 
            label37.AutoSize = true;
            label37.Location = new Point(11, 33);
            label37.Margin = new Padding(5, 0, 5, 0);
            label37.Name = "label37";
            label37.Size = new Size(69, 20);
            label37.TabIndex = 0;
            label37.Text = "Font Size";
            // 
            // TextRegionUseColour
            // 
            TextRegionUseColour.AutoSize = true;
            TextRegionUseColour.Location = new Point(16, 100);
            TextRegionUseColour.Margin = new Padding(5, 4, 5, 4);
            TextRegionUseColour.Name = "TextRegionUseColour";
            TextRegionUseColour.Size = new Size(161, 24);
            TextRegionUseColour.TabIndex = 3;
            TextRegionUseColour.Text = "Use Focused Colour";
            // 
            // cbPreviewShowNotations
            // 
            cbPreviewShowNotations.AutoSize = true;
            cbPreviewShowNotations.Location = new Point(16, 69);
            cbPreviewShowNotations.Margin = new Padding(5, 4, 5, 4);
            cbPreviewShowNotations.Name = "cbPreviewShowNotations";
            cbPreviewShowNotations.Size = new Size(136, 24);
            cbPreviewShowNotations.TabIndex = 2;
            cbPreviewShowNotations.Text = "Show Notations";
            // 
            // cbLineBetweenScreens
            // 
            cbLineBetweenScreens.AutoSize = true;
            cbLineBetweenScreens.Location = new Point(16, 131);
            cbLineBetweenScreens.Margin = new Padding(5, 4, 5, 4);
            cbLineBetweenScreens.Name = "cbLineBetweenScreens";
            cbLineBetweenScreens.Size = new Size(173, 24);
            cbLineBetweenScreens.TabIndex = 4;
            cbLineBetweenScreens.Text = "Line between Screens";
            // 
            // groupBox6
            // 
            groupBox6.Controls.Add(checkBoxMediaTab);
            groupBox6.Controls.Add(panel5);
            groupBox6.Controls.Add(tbMusicLocation);
            groupBox6.Location = new Point(8, 331);
            groupBox6.Margin = new Padding(5, 4, 5, 4);
            groupBox6.Name = "groupBox6";
            groupBox6.Padding = new Padding(5, 4, 5, 4);
            groupBox6.Size = new Size(336, 116);
            groupBox6.TabIndex = 2;
            groupBox6.TabStop = false;
            groupBox6.Text = "Media Files";
            // 
            // checkBoxMediaTab
            // 
            checkBoxMediaTab.AutoSize = true;
            checkBoxMediaTab.Location = new Point(16, 76);
            checkBoxMediaTab.Margin = new Padding(5, 4, 5, 4);
            checkBoxMediaTab.Name = "checkBoxMediaTab";
            checkBoxMediaTab.Size = new Size(140, 24);
            checkBoxMediaTab.TabIndex = 1;
            checkBoxMediaTab.Text = "Show Media Tab";
            // 
            // panel5
            // 
            panel5.Controls.Add(toolStrip1);
            panel5.Location = new Point(297, 33);
            panel5.Margin = new Padding(5, 4, 5, 4);
            panel5.Name = "panel5";
            panel5.Size = new Size(30, 32);
            panel5.TabIndex = 29;
            // 
            // toolStrip1
            // 
            toolStrip1.AutoSize = false;
            toolStrip1.CanOverflow = false;
            toolStrip1.Dock = DockStyle.None;
            toolStrip1.GripStyle = ToolStripGripStyle.Hidden;
            toolStrip1.ImageScalingSize = new Size(24, 24);
            toolStrip1.Items.AddRange(new ToolStripItem[] { MusicLocationBtn });
            toolStrip1.LayoutStyle = ToolStripLayoutStyle.Flow;
            toolStrip1.Location = new Point(0, 0);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.Padding = new Padding(0, 0, 2, 0);
            toolStrip1.RenderMode = ToolStripRenderMode.System;
            toolStrip1.Size = new Size(33, 37);
            toolStrip1.TabIndex = 0;
            // 
            // MusicLocationBtn
            // 
            MusicLocationBtn.AutoSize = false;
            MusicLocationBtn.DisplayStyle = ToolStripItemDisplayStyle.Image;
            MusicLocationBtn.Image = (Image)resources.GetObject("MusicLocationBtn.Image");
            MusicLocationBtn.ImageTransparentColor = Color.Magenta;
            MusicLocationBtn.Name = "MusicLocationBtn";
            MusicLocationBtn.Size = new Size(22, 22);
            MusicLocationBtn.Tag = "down";
            MusicLocationBtn.ToolTipText = "Change Location";
            MusicLocationBtn.Click += MusicLocationBtn_Click;
            // 
            // tbMusicLocation
            // 
            tbMusicLocation.Location = new Point(11, 33);
            tbMusicLocation.Margin = new Padding(5, 4, 5, 4);
            tbMusicLocation.MaxLength = 10;
            tbMusicLocation.Name = "tbMusicLocation";
            tbMusicLocation.Size = new Size(278, 27);
            tbMusicLocation.TabIndex = 0;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(panel4);
            groupBox1.Controls.Add(panel2);
            groupBox1.Controls.Add(panel1);
            groupBox1.Location = new Point(8, 21);
            groupBox1.Margin = new Padding(5, 4, 5, 4);
            groupBox1.Name = "groupBox1";
            groupBox1.Padding = new Padding(5, 4, 5, 4);
            groupBox1.Size = new Size(336, 176);
            groupBox1.TabIndex = 0;
            groupBox1.TabStop = false;
            groupBox1.Text = "Maximum Setting";
            // 
            // panel4
            // 
            panel4.Controls.Add(EditHistoryMaxUpDown);
            panel4.Controls.Add(label4);
            panel4.Location = new Point(8, 119);
            panel4.Margin = new Padding(5, 4, 5, 4);
            panel4.Name = "panel4";
            panel4.Size = new Size(318, 39);
            panel4.TabIndex = 4;
            // 
            // EditHistoryMaxUpDown
            // 
            EditHistoryMaxUpDown.Location = new Point(239, 4);
            EditHistoryMaxUpDown.Margin = new Padding(5, 4, 5, 4);
            EditHistoryMaxUpDown.Maximum = new decimal(new int[] { 20, 0, 0, 0 });
            EditHistoryMaxUpDown.Name = "EditHistoryMaxUpDown";
            EditHistoryMaxUpDown.Size = new Size(72, 27);
            EditHistoryMaxUpDown.TabIndex = 1;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(5, 8);
            label4.Margin = new Padding(5, 0, 5, 0);
            label4.Name = "label4";
            label4.Size = new Size(201, 20);
            label4.TabIndex = 0;
            label4.Text = "Max Edit History items listed:";
            // 
            // panel2
            // 
            panel2.Controls.Add(AdhocVersesMaxUpDown);
            panel2.Controls.Add(label2);
            panel2.Location = new Point(8, 73);
            panel2.Margin = new Padding(5, 4, 5, 4);
            panel2.Name = "panel2";
            panel2.Size = new Size(318, 39);
            panel2.TabIndex = 2;
            // 
            // AdhocVersesMaxUpDown
            // 
            AdhocVersesMaxUpDown.Location = new Point(239, 4);
            AdhocVersesMaxUpDown.Margin = new Padding(5, 4, 5, 4);
            AdhocVersesMaxUpDown.Maximum = new decimal(new int[] { 500, 0, 0, 0 });
            AdhocVersesMaxUpDown.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            AdhocVersesMaxUpDown.Name = "AdhocVersesMaxUpDown";
            AdhocVersesMaxUpDown.Size = new Size(72, 27);
            AdhocVersesMaxUpDown.TabIndex = 0;
            AdhocVersesMaxUpDown.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(5, 8);
            label2.Margin = new Padding(5, 0, 5, 0);
            label2.Name = "label2";
            label2.Size = new Size(191, 20);
            label2.TabIndex = 0;
            label2.Text = "Max Bible Search Selection:";
            // 
            // panel1
            // 
            panel1.Controls.Add(VersesMaxUpDown);
            panel1.Controls.Add(label1);
            panel1.Location = new Point(8, 29);
            panel1.Margin = new Padding(5, 4, 5, 4);
            panel1.Name = "panel1";
            panel1.Size = new Size(318, 39);
            panel1.TabIndex = 1;
            // 
            // VersesMaxUpDown
            // 
            VersesMaxUpDown.Location = new Point(239, 4);
            VersesMaxUpDown.Margin = new Padding(5, 4, 5, 4);
            VersesMaxUpDown.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
            VersesMaxUpDown.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            VersesMaxUpDown.Name = "VersesMaxUpDown";
            VersesMaxUpDown.Size = new Size(72, 27);
            VersesMaxUpDown.TabIndex = 1;
            VersesMaxUpDown.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(5, 8);
            label1.Margin = new Padding(5, 0, 5, 0);
            label1.Name = "label1";
            label1.Size = new Size(188, 20);
            label1.TabIndex = 0;
            label1.Text = "Max Bible Verses Selection:";
            // 
            // tabPageShow
            // 
            tabPageShow.BackColor = SystemColors.Control;
            tabPageShow.Controls.Add(label58);
            tabPageShow.Controls.Add(groupBox5);
            tabPageShow.Controls.Add(groupBox3);
            tabPageShow.Controls.Add(groupBox2);
            tabPageShow.Controls.Add(groupBox4);
            tabPageShow.Location = new Point(4, 29);
            tabPageShow.Margin = new Padding(5, 4, 5, 4);
            tabPageShow.Name = "tabPageShow";
            tabPageShow.Padding = new Padding(5, 4, 5, 4);
            tabPageShow.Size = new Size(719, 538);
            tabPageShow.TabIndex = 7;
            tabPageShow.Text = "Show";
            // 
            // label58
            // 
            label58.AutoSize = true;
            label58.Location = new Point(24, 424);
            label58.Margin = new Padding(5, 0, 5, 0);
            label58.Name = "label58";
            label58.Size = new Size(239, 20);
            label58.TabIndex = 4;
            label58.Text = "** Applicable in Dual Monitor Only";
            // 
            // groupBox5
            // 
            groupBox5.Controls.Add(cbDisableScreenSaver);
            groupBox5.Controls.Add(cbAdvanceNextItem);
            groupBox5.Controls.Add(checkBoxMediaNoPanel);
            groupBox5.Controls.Add(checkBoxPPNoPanel);
            groupBox5.Location = new Point(8, 229);
            groupBox5.Margin = new Padding(5, 4, 5, 4);
            groupBox5.Name = "groupBox5";
            groupBox5.Padding = new Padding(5, 4, 5, 4);
            groupBox5.Size = new Size(334, 177);
            groupBox5.TabIndex = 1;
            groupBox5.TabStop = false;
            groupBox5.Text = "Other Settings";
            // 
            // cbDisableScreenSaver
            // 
            cbDisableScreenSaver.AutoSize = true;
            cbDisableScreenSaver.Location = new Point(15, 132);
            cbDisableScreenSaver.Margin = new Padding(5, 4, 5, 4);
            cbDisableScreenSaver.Name = "cbDisableScreenSaver";
            cbDisableScreenSaver.Size = new Size(256, 24);
            cbDisableScreenSaver.TabIndex = 41;
            cbDisableScreenSaver.Text = "Disable Screen Saver during Show";
            // 
            // cbAdvanceNextItem
            // 
            cbAdvanceNextItem.AutoSize = true;
            cbAdvanceNextItem.Location = new Point(15, 32);
            cbAdvanceNextItem.Margin = new Padding(5, 4, 5, 4);
            cbAdvanceNextItem.Name = "cbAdvanceNextItem";
            cbAdvanceNextItem.Size = new Size(157, 24);
            cbAdvanceNextItem.TabIndex = 2;
            cbAdvanceNextItem.Text = "Advance Next Item";
            // 
            // checkBoxMediaNoPanel
            // 
            checkBoxMediaNoPanel.AutoSize = true;
            checkBoxMediaNoPanel.Location = new Point(15, 88);
            checkBoxMediaNoPanel.Margin = new Padding(5, 4, 5, 4);
            checkBoxMediaNoPanel.Name = "checkBoxMediaNoPanel";
            checkBoxMediaNoPanel.Size = new Size(231, 24);
            checkBoxMediaNoPanel.TabIndex = 30;
            checkBoxMediaNoPanel.Text = "No Panel Overlay For Media **";
            // 
            // checkBoxPPNoPanel
            // 
            checkBoxPPNoPanel.AutoSize = true;
            checkBoxPPNoPanel.Location = new Point(15, 60);
            checkBoxPPNoPanel.Margin = new Padding(5, 4, 5, 4);
            checkBoxPPNoPanel.Name = "checkBoxPPNoPanel";
            checkBoxPPNoPanel.Size = new Size(264, 24);
            checkBoxPPNoPanel.TabIndex = 4;
            checkBoxPPNoPanel.Text = "No Panel Overlay For Powerpoint **";
            // 
            // groupBox3
            // 
            groupBox3.Controls.Add(cbGapItemUseFade);
            groupBox3.Controls.Add(panel39);
            groupBox3.Controls.Add(tbGapLogoLocation);
            groupBox3.Controls.Add(rbGapItemOption3);
            groupBox3.Controls.Add(rbGapItemOption1);
            groupBox3.Controls.Add(rbGapItemOption0);
            groupBox3.Controls.Add(rbGapItemOption2);
            groupBox3.Location = new Point(358, 23);
            groupBox3.Margin = new Padding(5, 4, 5, 4);
            groupBox3.Name = "groupBox3";
            groupBox3.Padding = new Padding(5, 4, 5, 4);
            groupBox3.Size = new Size(342, 197);
            groupBox3.TabIndex = 2;
            groupBox3.TabStop = false;
            groupBox3.Text = "Gap between items";
            // 
            // cbGapItemUseFade
            // 
            cbGapItemUseFade.AutoSize = true;
            cbGapItemUseFade.Location = new Point(14, 149);
            cbGapItemUseFade.Margin = new Padding(5, 4, 5, 4);
            cbGapItemUseFade.Name = "cbGapItemUseFade";
            cbGapItemUseFade.Size = new Size(158, 24);
            cbGapItemUseFade.TabIndex = 5;
            cbGapItemUseFade.Text = "Use Fade Transition";
            // 
            // panel39
            // 
            panel39.Controls.Add(toolStrip10);
            panel39.Location = new Point(297, 107);
            panel39.Margin = new Padding(5, 4, 5, 4);
            panel39.Name = "panel39";
            panel39.Size = new Size(33, 32);
            panel39.TabIndex = 31;
            // 
            // toolStrip10
            // 
            toolStrip10.AutoSize = false;
            toolStrip10.CanOverflow = false;
            toolStrip10.Dock = DockStyle.None;
            toolStrip10.GripStyle = ToolStripGripStyle.Hidden;
            toolStrip10.ImageScalingSize = new Size(24, 24);
            toolStrip10.Items.AddRange(new ToolStripItem[] { GapLogoLocationBtn });
            toolStrip10.LayoutStyle = ToolStripLayoutStyle.Flow;
            toolStrip10.Location = new Point(0, -1);
            toolStrip10.Name = "toolStrip10";
            toolStrip10.Padding = new Padding(0, 0, 2, 0);
            toolStrip10.RenderMode = ToolStripRenderMode.System;
            toolStrip10.Size = new Size(33, 37);
            toolStrip10.TabIndex = 0;
            // 
            // GapLogoLocationBtn
            // 
            GapLogoLocationBtn.AutoSize = false;
            GapLogoLocationBtn.DisplayStyle = ToolStripItemDisplayStyle.Image;
            GapLogoLocationBtn.Image = (Image)resources.GetObject("GapLogoLocationBtn.Image");
            GapLogoLocationBtn.ImageTransparentColor = Color.Magenta;
            GapLogoLocationBtn.Name = "GapLogoLocationBtn";
            GapLogoLocationBtn.Size = new Size(22, 22);
            GapLogoLocationBtn.Tag = "";
            GapLogoLocationBtn.ToolTipText = "Lookup Logo File";
            GapLogoLocationBtn.Click += GapLogoLocationBtn_Click;
            // 
            // tbGapLogoLocation
            // 
            tbGapLogoLocation.Location = new Point(38, 107);
            tbGapLogoLocation.Margin = new Padding(5, 4, 5, 4);
            tbGapLogoLocation.MaxLength = 10;
            tbGapLogoLocation.Name = "tbGapLogoLocation";
            tbGapLogoLocation.Size = new Size(254, 27);
            tbGapLogoLocation.TabIndex = 4;
            tbGapLogoLocation.TextChanged += tbGapLogoLocation_TextChanged;
            // 
            // rbGapItemOption3
            // 
            rbGapItemOption3.AutoSize = true;
            rbGapItemOption3.Location = new Point(14, 112);
            rbGapItemOption3.Margin = new Padding(5, 4, 5, 4);
            rbGapItemOption3.Name = "rbGapItemOption3";
            rbGapItemOption3.Size = new Size(17, 16);
            rbGapItemOption3.TabIndex = 3;
            // 
            // rbGapItemOption1
            // 
            rbGapItemOption1.AutoSize = true;
            rbGapItemOption1.Location = new Point(14, 52);
            rbGapItemOption1.Margin = new Padding(5, 4, 5, 4);
            rbGapItemOption1.Name = "rbGapItemOption1";
            rbGapItemOption1.Size = new Size(113, 24);
            rbGapItemOption1.TabIndex = 1;
            rbGapItemOption1.Text = "Black Screen";
            // 
            // rbGapItemOption0
            // 
            rbGapItemOption0.AutoSize = true;
            rbGapItemOption0.Location = new Point(14, 24);
            rbGapItemOption0.Margin = new Padding(5, 4, 5, 4);
            rbGapItemOption0.Name = "rbGapItemOption0";
            rbGapItemOption0.Size = new Size(81, 24);
            rbGapItemOption0.TabIndex = 0;
            rbGapItemOption0.Text = "No Gap";
            // 
            // rbGapItemOption2
            // 
            rbGapItemOption2.AutoSize = true;
            rbGapItemOption2.Location = new Point(14, 80);
            rbGapItemOption2.Margin = new Padding(5, 4, 5, 4);
            rbGapItemOption2.Name = "rbGapItemOption2";
            rbGapItemOption2.Size = new Size(215, 24);
            rbGapItemOption2.TabIndex = 2;
            rbGapItemOption2.Text = "Default Session Background";
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(label51);
            groupBox2.Controls.Add(panel45);
            groupBox2.Controls.Add(VideoSizeUpDown1);
            groupBox2.Controls.Add(label14);
            groupBox2.Controls.Add(panelVideoHolder);
            groupBox2.Location = new Point(358, 229);
            groupBox2.Margin = new Padding(5, 4, 5, 4);
            groupBox2.Name = "groupBox2";
            groupBox2.Padding = new Padding(5, 4, 5, 4);
            groupBox2.Size = new Size(342, 177);
            groupBox2.TabIndex = 3;
            groupBox2.TabStop = false;
            groupBox2.Text = "Video Background";
            // 
            // label51
            // 
            label51.AutoSize = true;
            label51.Location = new Point(202, 100);
            label51.Margin = new Padding(5, 0, 5, 0);
            label51.Name = "label51";
            label51.Size = new Size(47, 20);
            label51.TabIndex = 2;
            label51.Text = "Align:";
            // 
            // panel45
            // 
            panel45.Controls.Add(toolStripVideo);
            panel45.Location = new Point(248, 93);
            panel45.Margin = new Padding(5, 4, 5, 4);
            panel45.Name = "panel45";
            panel45.Size = new Size(42, 33);
            panel45.TabIndex = 44;
            // 
            // toolStripVideo
            // 
            toolStripVideo.AutoSize = false;
            toolStripVideo.CanOverflow = false;
            toolStripVideo.Dock = DockStyle.None;
            toolStripVideo.GripStyle = ToolStripGripStyle.Hidden;
            toolStripVideo.ImageScalingSize = new Size(24, 24);
            toolStripVideo.Items.AddRange(new ToolStripItem[] { Video_VAlign });
            toolStripVideo.LayoutStyle = ToolStripLayoutStyle.HorizontalStackWithOverflow;
            toolStripVideo.Location = new Point(0, -1);
            toolStripVideo.Name = "toolStripVideo";
            toolStripVideo.Padding = new Padding(0, 0, 2, 0);
            toolStripVideo.RenderMode = ToolStripRenderMode.System;
            toolStripVideo.Size = new Size(42, 39);
            toolStripVideo.TabIndex = 0;
            // 
            // Video_VAlign
            // 
            Video_VAlign.DisplayStyle = ToolStripItemDisplayStyle.Image;
            Video_VAlign.DropDownItems.AddRange(new ToolStripItem[] { Video_VAlignTop, Video_VAlignCentre, Video_VAlignBottom });
            Video_VAlign.Image = (Image)resources.GetObject("Video_VAlign.Image");
            Video_VAlign.ImageTransparentColor = Color.Magenta;
            Video_VAlign.Name = "Video_VAlign";
            Video_VAlign.Size = new Size(38, 36);
            Video_VAlign.Tag = "0";
            Video_VAlign.ToolTipText = "Vertical Alignment";
            Video_VAlign.DropDownItemClicked += Video_VAlign_DropDownItemClicked;
            // 
            // Video_VAlignTop
            // 
            Video_VAlignTop.Image = (Image)resources.GetObject("Video_VAlignTop.Image");
            Video_VAlignTop.Name = "Video_VAlignTop";
            Video_VAlignTop.Size = new Size(181, 26);
            Video_VAlignTop.Tag = "0";
            Video_VAlignTop.Text = "Align Top";
            // 
            // Video_VAlignCentre
            // 
            Video_VAlignCentre.Image = (Image)resources.GetObject("Video_VAlignCentre.Image");
            Video_VAlignCentre.Name = "Video_VAlignCentre";
            Video_VAlignCentre.Size = new Size(181, 26);
            Video_VAlignCentre.Tag = "1";
            Video_VAlignCentre.Text = "Align Centre";
            // 
            // Video_VAlignBottom
            // 
            Video_VAlignBottom.Image = (Image)resources.GetObject("Video_VAlignBottom.Image");
            Video_VAlignBottom.Name = "Video_VAlignBottom";
            Video_VAlignBottom.Size = new Size(181, 26);
            Video_VAlignBottom.Tag = "2";
            Video_VAlignBottom.Text = "Align Bottom";
            // 
            // VideoSizeUpDown1
            // 
            VideoSizeUpDown1.Location = new Point(250, 53);
            VideoSizeUpDown1.Margin = new Padding(5, 4, 5, 4);
            VideoSizeUpDown1.Minimum = new decimal(new int[] { 25, 0, 0, 0 });
            VideoSizeUpDown1.Name = "VideoSizeUpDown1";
            VideoSizeUpDown1.Size = new Size(64, 27);
            VideoSizeUpDown1.TabIndex = 0;
            VideoSizeUpDown1.Value = new decimal(new int[] { 100, 0, 0, 0 });
            VideoSizeUpDown1.ValueChanged += VideoSizeUpDown1_ValueChanged;
            // 
            // label14
            // 
            label14.AutoSize = true;
            label14.Location = new Point(202, 60);
            label14.Margin = new Padding(5, 0, 5, 0);
            label14.Name = "label14";
            label14.Size = new Size(39, 20);
            label14.TabIndex = 1;
            label14.Text = "Size:";
            // 
            // panelVideoHolder
            // 
            panelVideoHolder.BackColor = Color.Navy;
            panelVideoHolder.BorderStyle = BorderStyle.Fixed3D;
            panelVideoHolder.Controls.Add(panelVideoSize);
            panelVideoHolder.Location = new Point(9, 31);
            panelVideoHolder.Margin = new Padding(5, 4, 5, 4);
            panelVideoHolder.Name = "panelVideoHolder";
            panelVideoHolder.Size = new Size(158, 136);
            panelVideoHolder.TabIndex = 15;
            // 
            // panelVideoSize
            // 
            panelVideoSize.BackColor = Color.FromArgb(128, 128, 255);
            panelVideoSize.Controls.Add(label50);
            panelVideoSize.Location = new Point(17, 0);
            panelVideoSize.Margin = new Padding(5, 4, 5, 4);
            panelVideoSize.Name = "panelVideoSize";
            panelVideoSize.Size = new Size(118, 103);
            panelVideoSize.TabIndex = 31;
            // 
            // label50
            // 
            label50.BackColor = Color.SlateBlue;
            label50.Dock = DockStyle.Fill;
            label50.ForeColor = Color.White;
            label50.Location = new Point(0, 0);
            label50.Margin = new Padding(5, 0, 5, 0);
            label50.Name = "label50";
            label50.Size = new Size(118, 103);
            label50.TabIndex = 21;
            label50.Text = "Video Size";
            label50.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // groupBox4
            // 
            groupBox4.Controls.Add(panel46);
            groupBox4.Controls.Add(cbWordWrapLeftAlignIndent);
            groupBox4.Controls.Add(cbLineBetweenRegions);
            groupBox4.Controls.Add(cbUseLargestFont);
            groupBox4.Controls.Add(cbAutoTextOverflow);
            groupBox4.Location = new Point(8, 23);
            groupBox4.Margin = new Padding(5, 4, 5, 4);
            groupBox4.Name = "groupBox4";
            groupBox4.Padding = new Padding(5, 4, 5, 4);
            groupBox4.Size = new Size(334, 197);
            groupBox4.TabIndex = 0;
            groupBox4.TabStop = false;
            groupBox4.Text = "Text Settings";
            // 
            // panel46
            // 
            panel46.Controls.Add(NotationFontFactorUpDown);
            panel46.Controls.Add(label54);
            panel46.Location = new Point(8, 141);
            panel46.Margin = new Padding(5, 4, 5, 4);
            panel46.Name = "panel46";
            panel46.Size = new Size(318, 39);
            panel46.TabIndex = 4;
            // 
            // NotationFontFactorUpDown
            // 
            NotationFontFactorUpDown.Location = new Point(247, 4);
            NotationFontFactorUpDown.Margin = new Padding(5, 4, 5, 4);
            NotationFontFactorUpDown.Maximum = new decimal(new int[] { 200, 0, 0, 0 });
            NotationFontFactorUpDown.Minimum = new decimal(new int[] { 20, 0, 0, 0 });
            NotationFontFactorUpDown.Name = "NotationFontFactorUpDown";
            NotationFontFactorUpDown.Size = new Size(64, 27);
            NotationFontFactorUpDown.TabIndex = 0;
            NotationFontFactorUpDown.Value = new decimal(new int[] { 75, 0, 0, 0 });
            // 
            // label54
            // 
            label54.AutoSize = true;
            label54.Location = new Point(5, 11);
            label54.Margin = new Padding(5, 0, 5, 0);
            label54.Name = "label54";
            label54.Size = new Size(249, 20);
            label54.TabIndex = 0;
            label54.Text = "Size of Notations to Folder Font (%):";
            // 
            // cbWordWrapLeftAlignIndent
            // 
            cbWordWrapLeftAlignIndent.AutoSize = true;
            cbWordWrapLeftAlignIndent.Location = new Point(15, 116);
            cbWordWrapLeftAlignIndent.Margin = new Padding(5, 4, 5, 4);
            cbWordWrapLeftAlignIndent.Name = "cbWordWrapLeftAlignIndent";
            cbWordWrapLeftAlignIndent.Size = new Size(210, 24);
            cbWordWrapLeftAlignIndent.TabIndex = 3;
            cbWordWrapLeftAlignIndent.Text = "Indent Word Wrapped Text";
            // 
            // cbLineBetweenRegions
            // 
            cbLineBetweenRegions.AutoSize = true;
            cbLineBetweenRegions.Location = new Point(15, 87);
            cbLineBetweenRegions.Margin = new Padding(5, 4, 5, 4);
            cbLineBetweenRegions.Name = "cbLineBetweenRegions";
            cbLineBetweenRegions.Size = new Size(176, 24);
            cbLineBetweenRegions.TabIndex = 2;
            cbLineBetweenRegions.Text = "Line between Regions";
            // 
            // cbUseLargestFont
            // 
            cbUseLargestFont.AutoSize = true;
            cbUseLargestFont.Location = new Point(15, 57);
            cbUseLargestFont.Margin = new Padding(5, 4, 5, 4);
            cbUseLargestFont.Name = "cbUseLargestFont";
            cbUseLargestFont.Size = new Size(140, 24);
            cbUseLargestFont.TabIndex = 1;
            cbUseLargestFont.Text = "Use Largest Font";
            // 
            // cbAutoTextOverflow
            // 
            cbAutoTextOverflow.AutoSize = true;
            cbAutoTextOverflow.Location = new Point(15, 29);
            cbAutoTextOverflow.Margin = new Padding(5, 4, 5, 4);
            cbAutoTextOverflow.Name = "cbAutoTextOverflow";
            cbAutoTextOverflow.Size = new Size(158, 24);
            cbAutoTextOverflow.TabIndex = 0;
            cbAutoTextOverflow.Text = "Auto Text Overflow";
            // 
            // tabPageMonitors
            // 
            tabPageMonitors.BackColor = SystemColors.Control;
            tabPageMonitors.Controls.Add(groupBox23);
            tabPageMonitors.Controls.Add(groupBoxLM);
            tabPageMonitors.Controls.Add(groupBoxDM);
            tabPageMonitors.Location = new Point(4, 29);
            tabPageMonitors.Margin = new Padding(5, 4, 5, 4);
            tabPageMonitors.Name = "tabPageMonitors";
            tabPageMonitors.Size = new Size(719, 538);
            tabPageMonitors.TabIndex = 6;
            tabPageMonitors.Text = "Monitors";
            // 
            // groupBox23
            // 
            groupBox23.Controls.Add(checkBoxLiveCamNoPanel);
            groupBox23.Controls.Add(panel47);
            groupBox23.Controls.Add(label53);
            groupBox23.Controls.Add(label5);
            groupBox23.Controls.Add(label7);
            groupBox23.Controls.Add(TrackBarBalance);
            groupBox23.Controls.Add(TrackBarVolume);
            groupBox23.Controls.Add(cbWidescreen);
            groupBox23.Controls.Add(cbMute);
            groupBox23.Controls.Add(label52);
            groupBox23.Location = new Point(9, 285);
            groupBox23.Margin = new Padding(5, 4, 5, 4);
            groupBox23.Name = "groupBox23";
            groupBox23.Padding = new Padding(5, 4, 5, 4);
            groupBox23.Size = new Size(335, 221);
            groupBox23.TabIndex = 1;
            groupBox23.TabStop = false;
            groupBox23.Text = "Live Cam Settings";
            // 
            // checkBoxLiveCamNoPanel
            // 
            checkBoxLiveCamNoPanel.AutoSize = true;
            checkBoxLiveCamNoPanel.Location = new Point(14, 187);
            checkBoxLiveCamNoPanel.Margin = new Padding(5, 4, 5, 4);
            checkBoxLiveCamNoPanel.Name = "checkBoxLiveCamNoPanel";
            checkBoxLiveCamNoPanel.Size = new Size(249, 24);
            checkBoxLiveCamNoPanel.TabIndex = 6;
            checkBoxLiveCamNoPanel.Text = "No Panel Overlay For Live Cam **";
            // 
            // panel47
            // 
            panel47.Controls.Add(toolStripCaptureDevices);
            panel47.Location = new Point(14, 32);
            panel47.Margin = new Padding(5, 4, 5, 4);
            panel47.Name = "panel47";
            panel47.Size = new Size(313, 33);
            panel47.TabIndex = 70;
            // 
            // toolStripCaptureDevices
            // 
            toolStripCaptureDevices.AutoSize = false;
            toolStripCaptureDevices.CanOverflow = false;
            toolStripCaptureDevices.Dock = DockStyle.None;
            toolStripCaptureDevices.GripStyle = ToolStripGripStyle.Hidden;
            toolStripCaptureDevices.ImageScalingSize = new Size(24, 24);
            toolStripCaptureDevices.Items.AddRange(new ToolStripItem[] { cbCaptureDevices });
            toolStripCaptureDevices.LayoutStyle = ToolStripLayoutStyle.HorizontalStackWithOverflow;
            toolStripCaptureDevices.Location = new Point(0, -1);
            toolStripCaptureDevices.Name = "toolStripCaptureDevices";
            toolStripCaptureDevices.Padding = new Padding(0, 0, 2, 0);
            toolStripCaptureDevices.RenderMode = ToolStripRenderMode.System;
            toolStripCaptureDevices.Size = new Size(313, 39);
            toolStripCaptureDevices.TabIndex = 0;
            // 
            // cbCaptureDevices
            // 
            cbCaptureDevices.AutoSize = false;
            cbCaptureDevices.DropDownStyle = ComboBoxStyle.DropDownList;
            cbCaptureDevices.Name = "cbCaptureDevices";
            cbCaptureDevices.Size = new Size(306, 28);
            // 
            // label53
            // 
            label53.AutoSize = true;
            label53.Location = new Point(111, 93);
            label53.Margin = new Padding(5, 0, 5, 0);
            label53.Name = "label53";
            label53.Size = new Size(34, 20);
            label53.TabIndex = 77;
            label53.Text = "Min";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(298, 149);
            label5.Margin = new Padding(5, 0, 5, 0);
            label5.Name = "label5";
            label5.Size = new Size(18, 20);
            label5.TabIndex = 76;
            label5.Text = "R";
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(119, 149);
            label7.Margin = new Padding(5, 0, 5, 0);
            label7.Name = "label7";
            label7.Size = new Size(16, 20);
            label7.TabIndex = 75;
            label7.Text = "L";
            // 
            // TrackBarBalance
            // 
            TrackBarBalance.AutoSize = false;
            TrackBarBalance.Location = new Point(139, 136);
            TrackBarBalance.Margin = new Padding(5, 4, 5, 4);
            TrackBarBalance.Maximum = 100;
            TrackBarBalance.Minimum = -100;
            TrackBarBalance.Name = "TrackBarBalance";
            TrackBarBalance.Size = new Size(154, 52);
            TrackBarBalance.TabIndex = 5;
            TrackBarBalance.TickFrequency = 20;
            // 
            // TrackBarVolume
            // 
            TrackBarVolume.AutoSize = false;
            TrackBarVolume.Location = new Point(139, 76);
            TrackBarVolume.Margin = new Padding(5, 4, 5, 4);
            TrackBarVolume.Maximum = 100;
            TrackBarVolume.Name = "TrackBarVolume";
            TrackBarVolume.Size = new Size(154, 53);
            TrackBarVolume.TabIndex = 4;
            TrackBarVolume.TickFrequency = 10;
            TrackBarVolume.TickStyle = TickStyle.Both;
            // 
            // cbWidescreen
            // 
            cbWidescreen.AutoSize = true;
            cbWidescreen.Location = new Point(14, 123);
            cbWidescreen.Margin = new Padding(5, 4, 5, 4);
            cbWidescreen.Name = "cbWidescreen";
            cbWidescreen.Size = new Size(110, 24);
            cbWidescreen.TabIndex = 3;
            cbWidescreen.Text = "WideScreen";
            // 
            // cbMute
            // 
            cbMute.AutoSize = true;
            cbMute.Location = new Point(14, 88);
            cbMute.Margin = new Padding(5, 4, 5, 4);
            cbMute.Name = "cbMute";
            cbMute.Size = new Size(65, 24);
            cbMute.TabIndex = 2;
            cbMute.Text = "Mute";
            // 
            // label52
            // 
            label52.AutoSize = true;
            label52.Location = new Point(291, 93);
            label52.Margin = new Padding(5, 0, 5, 0);
            label52.Name = "label52";
            label52.Size = new Size(37, 20);
            label52.TabIndex = 74;
            label52.Text = "Max";
            // 
            // groupBoxLM
            // 
            groupBoxLM.Controls.Add(cbLMBroadcast);
            groupBoxLM.Controls.Add(btnLMHighlightColour);
            groupBoxLM.Controls.Add(btnLMBackColour);
            groupBoxLM.Controls.Add(btnLMTextColour);
            groupBoxLM.Controls.Add(LM1UpDownLeft);
            groupBoxLM.Controls.Add(label23);
            groupBoxLM.Controls.Add(LM1UpDownTop);
            groupBoxLM.Controls.Add(label26);
            groupBoxLM.Controls.Add(LMNotationsUpDownFontSize);
            groupBoxLM.Controls.Add(panel48);
            groupBoxLM.Controls.Add(LM_AlwaysUse);
            groupBoxLM.Controls.Add(LMUpDownFontSize);
            groupBoxLM.Controls.Add(optLM0);
            groupBoxLM.Controls.Add(panel43);
            groupBoxLM.Controls.Add(panel44);
            groupBoxLM.Controls.Add(LM1UpDownHeight);
            groupBoxLM.Controls.Add(LM1UpDownWidth);
            groupBoxLM.Controls.Add(optLM1);
            groupBoxLM.Controls.Add(label25);
            groupBoxLM.Controls.Add(label48);
            groupBoxLM.Controls.Add(label9);
            groupBoxLM.Controls.Add(cbLMShowNotations);
            groupBoxLM.Controls.Add(label49);
            groupBoxLM.Location = new Point(360, 21);
            groupBoxLM.Margin = new Padding(5, 4, 5, 4);
            groupBoxLM.Name = "groupBoxLM";
            groupBoxLM.Padding = new Padding(5, 4, 5, 4);
            groupBoxLM.Size = new Size(335, 491);
            groupBoxLM.TabIndex = 2;
            groupBoxLM.TabStop = false;
            groupBoxLM.Text = "Lyrics Monitor";
            // 
            // cbLMBroadcast
            // 
            cbLMBroadcast.AutoSize = true;
            cbLMBroadcast.Location = new Point(25, 419);
            cbLMBroadcast.Margin = new Padding(5, 4, 5, 4);
            cbLMBroadcast.Name = "cbLMBroadcast";
            cbLMBroadcast.Size = new Size(136, 24);
            cbLMBroadcast.TabIndex = 45;
            cbLMBroadcast.Text = "Broadcast Lyrics";
            cbLMBroadcast.Visible = false;
            // 
            // btnLMHighlightColour
            // 
            btnLMHighlightColour.BackColor = Color.LightGray;
            btnLMHighlightColour.FlatStyle = FlatStyle.Flat;
            btnLMHighlightColour.Location = new Point(25, 323);
            btnLMHighlightColour.Margin = new Padding(5, 4, 5, 4);
            btnLMHighlightColour.Name = "btnLMHighlightColour";
            btnLMHighlightColour.Size = new Size(130, 39);
            btnLMHighlightColour.TabIndex = 17;
            btnLMHighlightColour.Text = "Selected Colour";
            btnLMHighlightColour.UseVisualStyleBackColor = false;
            btnLMHighlightColour.Click += btnLMHighlightColour_Click;
            // 
            // btnLMBackColour
            // 
            btnLMBackColour.BackColor = Color.LightGray;
            btnLMBackColour.FlatStyle = FlatStyle.Flat;
            btnLMBackColour.Location = new Point(25, 371);
            btnLMBackColour.Margin = new Padding(5, 4, 5, 4);
            btnLMBackColour.Name = "btnLMBackColour";
            btnLMBackColour.Size = new Size(130, 39);
            btnLMBackColour.TabIndex = 18;
            btnLMBackColour.Text = "Back Colour";
            btnLMBackColour.UseVisualStyleBackColor = false;
            btnLMBackColour.Click += btnLMBackColour_Click;
            // 
            // btnLMTextColour
            // 
            btnLMTextColour.BackColor = Color.LightGray;
            btnLMTextColour.FlatStyle = FlatStyle.Flat;
            btnLMTextColour.Location = new Point(25, 276);
            btnLMTextColour.Margin = new Padding(5, 4, 5, 4);
            btnLMTextColour.Name = "btnLMTextColour";
            btnLMTextColour.Size = new Size(130, 39);
            btnLMTextColour.TabIndex = 16;
            btnLMTextColour.Text = "Text Colour";
            btnLMTextColour.UseVisualStyleBackColor = false;
            btnLMTextColour.Click += btnLMTextColour_Click;
            // 
            // LM1UpDownLeft
            // 
            LM1UpDownLeft.Location = new Point(129, 152);
            LM1UpDownLeft.Margin = new Padding(5, 4, 5, 4);
            LM1UpDownLeft.Maximum = new decimal(new int[] { 9999, 0, 0, 0 });
            LM1UpDownLeft.Minimum = new decimal(new int[] { 9999, 0, 0, int.MinValue });
            LM1UpDownLeft.Name = "LM1UpDownLeft";
            LM1UpDownLeft.Size = new Size(65, 27);
            LM1UpDownLeft.TabIndex = 7;
            // 
            // label23
            // 
            label23.AutoSize = true;
            label23.Location = new Point(89, 117);
            label23.Margin = new Padding(5, 0, 5, 0);
            label23.Name = "label23";
            label23.Size = new Size(37, 20);
            label23.TabIndex = 2;
            label23.Text = "Top:";
            // 
            // LM1UpDownTop
            // 
            LM1UpDownTop.Location = new Point(129, 113);
            LM1UpDownTop.Margin = new Padding(5, 4, 5, 4);
            LM1UpDownTop.Maximum = new decimal(new int[] { 9999, 0, 0, 0 });
            LM1UpDownTop.Minimum = new decimal(new int[] { 9999, 0, 0, int.MinValue });
            LM1UpDownTop.Name = "LM1UpDownTop";
            LM1UpDownTop.Size = new Size(65, 27);
            LM1UpDownTop.TabIndex = 3;
            // 
            // label26
            // 
            label26.AutoSize = true;
            label26.Location = new Point(90, 156);
            label26.Margin = new Padding(5, 0, 5, 0);
            label26.Name = "label26";
            label26.Size = new Size(37, 20);
            label26.TabIndex = 6;
            label26.Text = "Left:";
            // 
            // LMNotationsUpDownFontSize
            // 
            LMNotationsUpDownFontSize.Location = new Point(255, 239);
            LMNotationsUpDownFontSize.Margin = new Padding(5, 4, 5, 4);
            LMNotationsUpDownFontSize.Maximum = new decimal(new int[] { 40, 0, 0, 0 });
            LMNotationsUpDownFontSize.Minimum = new decimal(new int[] { 8, 0, 0, 0 });
            LMNotationsUpDownFontSize.Name = "LMNotationsUpDownFontSize";
            LMNotationsUpDownFontSize.Size = new Size(65, 27);
            LMNotationsUpDownFontSize.TabIndex = 14;
            LMNotationsUpDownFontSize.Value = new decimal(new int[] { 8, 0, 0, 0 });
            LMNotationsUpDownFontSize.Visible = false;
            // 
            // panel48
            // 
            panel48.Controls.Add(toolStripLyricsMonitor);
            panel48.Location = new Point(208, 200);
            panel48.Margin = new Padding(5, 4, 5, 4);
            panel48.Name = "panel48";
            panel48.Size = new Size(96, 33);
            panel48.TabIndex = 44;
            panel48.Visible = false;
            // 
            // toolStripLyricsMonitor
            // 
            toolStripLyricsMonitor.AutoSize = false;
            toolStripLyricsMonitor.CanOverflow = false;
            toolStripLyricsMonitor.Dock = DockStyle.None;
            toolStripLyricsMonitor.GripStyle = ToolStripGripStyle.Hidden;
            toolStripLyricsMonitor.ImageScalingSize = new Size(24, 24);
            toolStripLyricsMonitor.Items.AddRange(new ToolStripItem[] { LM_Bold, LM_Italic, LM_Underline });
            toolStripLyricsMonitor.LayoutStyle = ToolStripLayoutStyle.HorizontalStackWithOverflow;
            toolStripLyricsMonitor.Location = new Point(0, -1);
            toolStripLyricsMonitor.Name = "toolStripLyricsMonitor";
            toolStripLyricsMonitor.Padding = new Padding(0, 0, 2, 0);
            toolStripLyricsMonitor.RenderMode = ToolStripRenderMode.System;
            toolStripLyricsMonitor.Size = new Size(97, 39);
            toolStripLyricsMonitor.TabIndex = 0;
            // 
            // LM_Bold
            // 
            LM_Bold.CheckOnClick = true;
            LM_Bold.DisplayStyle = ToolStripItemDisplayStyle.Image;
            LM_Bold.Image = (Image)resources.GetObject("LM_Bold.Image");
            LM_Bold.ImageTransparentColor = Color.Magenta;
            LM_Bold.Name = "LM_Bold";
            LM_Bold.Size = new Size(29, 36);
            // 
            // LM_Italic
            // 
            LM_Italic.CheckOnClick = true;
            LM_Italic.DisplayStyle = ToolStripItemDisplayStyle.Image;
            LM_Italic.Image = (Image)resources.GetObject("LM_Italic.Image");
            LM_Italic.ImageTransparentColor = Color.Magenta;
            LM_Italic.Name = "LM_Italic";
            LM_Italic.Size = new Size(29, 36);
            // 
            // LM_Underline
            // 
            LM_Underline.CheckOnClick = true;
            LM_Underline.DisplayStyle = ToolStripItemDisplayStyle.Image;
            LM_Underline.Image = (Image)resources.GetObject("LM_Underline.Image");
            LM_Underline.ImageTransparentColor = Color.Magenta;
            LM_Underline.Name = "LM_Underline";
            LM_Underline.Size = new Size(29, 36);
            // 
            // LM_AlwaysUse
            // 
            LM_AlwaysUse.AutoSize = true;
            LM_AlwaysUse.Location = new Point(41, 71);
            LM_AlwaysUse.Margin = new Padding(5, 4, 5, 4);
            LM_AlwaysUse.Name = "LM_AlwaysUse";
            LM_AlwaysUse.Size = new Size(264, 24);
            LM_AlwaysUse.TabIndex = 1;
            LM_AlwaysUse.Text = "Always attempt Secondary Monitor";
            // 
            // LMUpDownFontSize
            // 
            LMUpDownFontSize.Location = new Point(129, 200);
            LMUpDownFontSize.Margin = new Padding(5, 4, 5, 4);
            LMUpDownFontSize.Maximum = new decimal(new int[] { 40, 0, 0, 0 });
            LMUpDownFontSize.Minimum = new decimal(new int[] { 8, 0, 0, 0 });
            LMUpDownFontSize.Name = "LMUpDownFontSize";
            LMUpDownFontSize.Size = new Size(65, 27);
            LMUpDownFontSize.TabIndex = 12;
            LMUpDownFontSize.Value = new decimal(new int[] { 8, 0, 0, 0 });
            // 
            // optLM0
            // 
            optLM0.AutoSize = true;
            optLM0.Checked = true;
            optLM0.Location = new Point(14, 33);
            optLM0.Margin = new Padding(5, 4, 5, 4);
            optLM0.Name = "optLM0";
            optLM0.Size = new Size(17, 16);
            optLM0.TabIndex = 0;
            optLM0.TabStop = true;
            optLM0.UseVisualStyleBackColor = true;
            optLM0.CheckedChanged += optLM_CheckedChanged;
            // 
            // panel43
            // 
            panel43.Controls.Add(toolStrip12);
            panel43.Location = new Point(39, 29);
            panel43.Margin = new Padding(5, 4, 5, 4);
            panel43.Name = "panel43";
            panel43.Size = new Size(251, 33);
            panel43.TabIndex = 30;
            // 
            // toolStrip12
            // 
            toolStrip12.AutoSize = false;
            toolStrip12.CanOverflow = false;
            toolStrip12.Dock = DockStyle.None;
            toolStrip12.GripStyle = ToolStripGripStyle.Hidden;
            toolStrip12.ImageScalingSize = new Size(24, 24);
            toolStrip12.Items.AddRange(new ToolStripItem[] { LyricsMonitorList });
            toolStrip12.LayoutStyle = ToolStripLayoutStyle.HorizontalStackWithOverflow;
            toolStrip12.Location = new Point(0, -1);
            toolStrip12.Name = "toolStrip12";
            toolStrip12.Padding = new Padding(0, 0, 2, 0);
            toolStrip12.RenderMode = ToolStripRenderMode.System;
            toolStrip12.Size = new Size(255, 39);
            toolStrip12.TabIndex = 0;
            // 
            // LyricsMonitorList
            // 
            LyricsMonitorList.AutoSize = false;
            LyricsMonitorList.DropDownStyle = ComboBoxStyle.DropDownList;
            LyricsMonitorList.MaxDropDownItems = 12;
            LyricsMonitorList.Name = "LyricsMonitorList";
            LyricsMonitorList.Size = new Size(249, 28);
            // 
            // panel44
            // 
            panel44.Controls.Add(toolStrip13);
            panel44.Location = new Point(297, 32);
            panel44.Margin = new Padding(5, 4, 5, 4);
            panel44.Name = "panel44";
            panel44.Size = new Size(30, 32);
            panel44.TabIndex = 29;
            // 
            // toolStrip13
            // 
            toolStrip13.AutoSize = false;
            toolStrip13.CanOverflow = false;
            toolStrip13.Dock = DockStyle.None;
            toolStrip13.GripStyle = ToolStripGripStyle.Hidden;
            toolStrip13.ImageScalingSize = new Size(24, 24);
            toolStrip13.Items.AddRange(new ToolStripItem[] { LyricsMonitor_Info });
            toolStrip13.LayoutStyle = ToolStripLayoutStyle.Flow;
            toolStrip13.Location = new Point(0, -1);
            toolStrip13.Name = "toolStrip13";
            toolStrip13.Padding = new Padding(0, 0, 2, 0);
            toolStrip13.RenderMode = ToolStripRenderMode.System;
            toolStrip13.Size = new Size(38, 37);
            toolStrip13.TabIndex = 0;
            // 
            // LyricsMonitor_Info
            // 
            LyricsMonitor_Info.AutoSize = false;
            LyricsMonitor_Info.DisplayStyle = ToolStripItemDisplayStyle.Image;
            LyricsMonitor_Info.Image = (Image)resources.GetObject("LyricsMonitor_Info.Image");
            LyricsMonitor_Info.ImageTransparentColor = Color.Magenta;
            LyricsMonitor_Info.Name = "LyricsMonitor_Info";
            LyricsMonitor_Info.Size = new Size(22, 22);
            LyricsMonitor_Info.Tag = "";
            LyricsMonitor_Info.ToolTipText = "Monitor Info";
            LyricsMonitor_Info.Click += LyricsMonitor_Info_Click;
            // 
            // LM1UpDownHeight
            // 
            LM1UpDownHeight.Enabled = false;
            LM1UpDownHeight.Location = new Point(255, 152);
            LM1UpDownHeight.Margin = new Padding(5, 4, 5, 4);
            LM1UpDownHeight.Maximum = new decimal(new int[] { 9999, 0, 0, 0 });
            LM1UpDownHeight.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            LM1UpDownHeight.Name = "LM1UpDownHeight";
            LM1UpDownHeight.Size = new Size(65, 27);
            LM1UpDownHeight.TabIndex = 9;
            LM1UpDownHeight.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // LM1UpDownWidth
            // 
            LM1UpDownWidth.Location = new Point(255, 113);
            LM1UpDownWidth.Margin = new Padding(5, 4, 5, 4);
            LM1UpDownWidth.Maximum = new decimal(new int[] { 9999, 0, 0, 0 });
            LM1UpDownWidth.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            LM1UpDownWidth.Name = "LM1UpDownWidth";
            LM1UpDownWidth.Size = new Size(65, 27);
            LM1UpDownWidth.TabIndex = 5;
            LM1UpDownWidth.Value = new decimal(new int[] { 1, 0, 0, 0 });
            LM1UpDownWidth.ValueChanged += LM1UpDownWidth_ValueChanged;
            // 
            // optLM1
            // 
            optLM1.AutoSize = true;
            optLM1.Location = new Point(14, 113);
            optLM1.Margin = new Padding(5, 4, 5, 4);
            optLM1.Name = "optLM1";
            optLM1.Size = new Size(83, 24);
            optLM1.TabIndex = 1;
            optLM1.TabStop = true;
            optLM1.Text = "Custom:";
            optLM1.UseVisualStyleBackColor = true;
            optLM1.CheckedChanged += optLM_CheckedChanged;
            // 
            // label25
            // 
            label25.AutoSize = true;
            label25.Location = new Point(201, 159);
            label25.Margin = new Padding(5, 0, 5, 0);
            label25.Name = "label25";
            label25.Size = new Size(57, 20);
            label25.TabIndex = 8;
            label25.Text = "Height:";
            // 
            // label48
            // 
            label48.AutoSize = true;
            label48.Location = new Point(203, 117);
            label48.Margin = new Padding(5, 0, 5, 0);
            label48.Name = "label48";
            label48.Size = new Size(52, 20);
            label48.TabIndex = 4;
            label48.Text = "Width:";
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Location = new Point(215, 241);
            label9.Margin = new Padding(5, 0, 5, 0);
            label9.Name = "label9";
            label9.Size = new Size(39, 20);
            label9.TabIndex = 13;
            label9.Text = "Size:";
            label9.Visible = false;
            // 
            // cbLMShowNotations
            // 
            cbLMShowNotations.AutoSize = true;
            cbLMShowNotations.Location = new Point(25, 240);
            cbLMShowNotations.Margin = new Padding(5, 4, 5, 4);
            cbLMShowNotations.Name = "cbLMShowNotations";
            cbLMShowNotations.Size = new Size(136, 24);
            cbLMShowNotations.TabIndex = 15;
            cbLMShowNotations.Text = "Show Notations";
            // 
            // label49
            // 
            label49.AutoSize = true;
            label49.Location = new Point(22, 204);
            label49.Margin = new Padding(5, 0, 5, 0);
            label49.Name = "label49";
            label49.Size = new Size(109, 20);
            label49.TabIndex = 11;
            label49.Text = "Main Font Size:";
            // 
            // groupBoxDM
            // 
            groupBoxDM.Controls.Add(panel49);
            groupBoxDM.Controls.Add(DM_CustomAsSingleMonitor);
            groupBoxDM.Controls.Add(DM1UpDownHeight);
            groupBoxDM.Controls.Add(DM1UpDownLeft);
            groupBoxDM.Controls.Add(label44);
            groupBoxDM.Controls.Add(DM1UpDownWidth);
            groupBoxDM.Controls.Add(DM1UpDownTop);
            groupBoxDM.Controls.Add(optDM1);
            groupBoxDM.Controls.Add(optDM0);
            groupBoxDM.Controls.Add(panelDM);
            groupBoxDM.Controls.Add(panelLinkTitle2Lookup);
            groupBoxDM.Controls.Add(DM_AlwaysUseSecondaryMonitor);
            groupBoxDM.Controls.Add(label46);
            groupBoxDM.Controls.Add(label43);
            groupBoxDM.Controls.Add(label45);
            groupBoxDM.Location = new Point(9, 21);
            groupBoxDM.Margin = new Padding(5, 4, 5, 4);
            groupBoxDM.Name = "groupBoxDM";
            groupBoxDM.Padding = new Padding(5, 4, 5, 4);
            groupBoxDM.Size = new Size(335, 256);
            groupBoxDM.TabIndex = 0;
            groupBoxDM.TabStop = false;
            groupBoxDM.Text = "Live Output Monitor";
            // 
            // panel49
            // 
            panel49.Controls.Add(optWide);
            panel49.Controls.Add(optStandard);
            panel49.Location = new Point(14, 71);
            panel49.Margin = new Padding(3, 4, 3, 4);
            panel49.Name = "panel49";
            panel49.Size = new Size(306, 35);
            panel49.TabIndex = 44;
            // 
            // optWide
            // 
            optWide.AutoSize = true;
            optWide.Location = new Point(144, 5);
            optWide.Margin = new Padding(3, 4, 3, 4);
            optWide.Name = "optWide";
            optWide.Size = new Size(99, 24);
            optWide.TabIndex = 43;
            optWide.Text = "wide(16:9)";
            optWide.UseVisualStyleBackColor = true;
            optWide.CheckedChanged += optWide_CheckedChanged;
            // 
            // optStandard
            // 
            optStandard.AutoSize = true;
            optStandard.Checked = true;
            optStandard.Location = new Point(30, 5);
            optStandard.Margin = new Padding(3, 4, 3, 4);
            optStandard.Name = "optStandard";
            optStandard.Size = new Size(117, 24);
            optStandard.TabIndex = 42;
            optStandard.TabStop = true;
            optStandard.Text = "standard(4:3)";
            optStandard.UseVisualStyleBackColor = true;
            optStandard.CheckedChanged += optStandard_CheckedChanged;
            // 
            // DM_CustomAsSingleMonitor
            // 
            DM_CustomAsSingleMonitor.AutoSize = true;
            DM_CustomAsSingleMonitor.Location = new Point(41, 217);
            DM_CustomAsSingleMonitor.Margin = new Padding(5, 4, 5, 4);
            DM_CustomAsSingleMonitor.Name = "DM_CustomAsSingleMonitor";
            DM_CustomAsSingleMonitor.Size = new Size(216, 24);
            DM_CustomAsSingleMonitor.TabIndex = 41;
            DM_CustomAsSingleMonitor.Text = "Act as Single Monitor Mode";
            // 
            // DM1UpDownHeight
            // 
            DM1UpDownHeight.Location = new Point(255, 180);
            DM1UpDownHeight.Margin = new Padding(5, 4, 5, 4);
            DM1UpDownHeight.Maximum = new decimal(new int[] { 9999, 0, 0, 0 });
            DM1UpDownHeight.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            DM1UpDownHeight.Name = "DM1UpDownHeight";
            DM1UpDownHeight.Size = new Size(65, 27);
            DM1UpDownHeight.TabIndex = 3;
            DM1UpDownHeight.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // DM1UpDownLeft
            // 
            DM1UpDownLeft.Location = new Point(135, 180);
            DM1UpDownLeft.Margin = new Padding(5, 4, 5, 4);
            DM1UpDownLeft.Maximum = new decimal(new int[] { 9999, 0, 0, 0 });
            DM1UpDownLeft.Minimum = new decimal(new int[] { 9999, 0, 0, int.MinValue });
            DM1UpDownLeft.Name = "DM1UpDownLeft";
            DM1UpDownLeft.Size = new Size(65, 27);
            DM1UpDownLeft.TabIndex = 1;
            // 
            // label44
            // 
            label44.AutoSize = true;
            label44.Location = new Point(95, 145);
            label44.Margin = new Padding(5, 0, 5, 0);
            label44.Name = "label44";
            label44.Size = new Size(37, 20);
            label44.TabIndex = 36;
            label44.Text = "Top:";
            // 
            // DM1UpDownWidth
            // 
            DM1UpDownWidth.Location = new Point(255, 141);
            DM1UpDownWidth.Margin = new Padding(5, 4, 5, 4);
            DM1UpDownWidth.Maximum = new decimal(new int[] { 9999, 0, 0, 0 });
            DM1UpDownWidth.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            DM1UpDownWidth.Name = "DM1UpDownWidth";
            DM1UpDownWidth.Size = new Size(65, 27);
            DM1UpDownWidth.TabIndex = 2;
            DM1UpDownWidth.Value = new decimal(new int[] { 1, 0, 0, 0 });
            DM1UpDownWidth.ValueChanged += DM1UpDownWidth_ValueChanged;
            // 
            // DM1UpDownTop
            // 
            DM1UpDownTop.Location = new Point(135, 141);
            DM1UpDownTop.Margin = new Padding(5, 4, 5, 4);
            DM1UpDownTop.Maximum = new decimal(new int[] { 9999, 0, 0, 0 });
            DM1UpDownTop.Minimum = new decimal(new int[] { 9999, 0, 0, int.MinValue });
            DM1UpDownTop.Name = "DM1UpDownTop";
            DM1UpDownTop.Size = new Size(65, 27);
            DM1UpDownTop.TabIndex = 0;
            // 
            // optDM1
            // 
            optDM1.AutoSize = true;
            optDM1.Location = new Point(14, 141);
            optDM1.Margin = new Padding(5, 4, 5, 4);
            optDM1.Name = "optDM1";
            optDM1.Size = new Size(83, 24);
            optDM1.TabIndex = 32;
            optDM1.TabStop = true;
            optDM1.Text = "Custom:";
            optDM1.UseVisualStyleBackColor = true;
            // 
            // optDM0
            // 
            optDM0.AutoSize = true;
            optDM0.Checked = true;
            optDM0.Location = new Point(14, 33);
            optDM0.Margin = new Padding(5, 4, 5, 4);
            optDM0.Name = "optDM0";
            optDM0.Size = new Size(17, 16);
            optDM0.TabIndex = 31;
            optDM0.TabStop = true;
            optDM0.UseVisualStyleBackColor = true;
            optDM0.CheckedChanged += optDM_CheckedChanged;
            // 
            // panelDM
            // 
            panelDM.Controls.Add(toolStripMonitorList);
            panelDM.Location = new Point(39, 29);
            panelDM.Margin = new Padding(5, 4, 5, 4);
            panelDM.Name = "panelDM";
            panelDM.Size = new Size(251, 33);
            panelDM.TabIndex = 30;
            // 
            // toolStripMonitorList
            // 
            toolStripMonitorList.AutoSize = false;
            toolStripMonitorList.CanOverflow = false;
            toolStripMonitorList.Dock = DockStyle.None;
            toolStripMonitorList.GripStyle = ToolStripGripStyle.Hidden;
            toolStripMonitorList.ImageScalingSize = new Size(24, 24);
            toolStripMonitorList.Items.AddRange(new ToolStripItem[] { DualMonitorList });
            toolStripMonitorList.LayoutStyle = ToolStripLayoutStyle.HorizontalStackWithOverflow;
            toolStripMonitorList.Location = new Point(0, -1);
            toolStripMonitorList.Name = "toolStripMonitorList";
            toolStripMonitorList.Padding = new Padding(0, 0, 2, 0);
            toolStripMonitorList.RenderMode = ToolStripRenderMode.System;
            toolStripMonitorList.Size = new Size(255, 39);
            toolStripMonitorList.TabIndex = 0;
            // 
            // DualMonitorList
            // 
            DualMonitorList.AutoSize = false;
            DualMonitorList.DropDownStyle = ComboBoxStyle.DropDownList;
            DualMonitorList.MaxDropDownItems = 12;
            DualMonitorList.Name = "DualMonitorList";
            DualMonitorList.Size = new Size(249, 28);
            // 
            // panelLinkTitle2Lookup
            // 
            panelLinkTitle2Lookup.Controls.Add(toolStrip2);
            panelLinkTitle2Lookup.Location = new Point(297, 32);
            panelLinkTitle2Lookup.Margin = new Padding(5, 4, 5, 4);
            panelLinkTitle2Lookup.Name = "panelLinkTitle2Lookup";
            panelLinkTitle2Lookup.Size = new Size(30, 32);
            panelLinkTitle2Lookup.TabIndex = 29;
            // 
            // toolStrip2
            // 
            toolStrip2.AutoSize = false;
            toolStrip2.CanOverflow = false;
            toolStrip2.Dock = DockStyle.None;
            toolStrip2.GripStyle = ToolStripGripStyle.Hidden;
            toolStrip2.ImageScalingSize = new Size(24, 24);
            toolStrip2.Items.AddRange(new ToolStripItem[] { Monitor_Info });
            toolStrip2.LayoutStyle = ToolStripLayoutStyle.Flow;
            toolStrip2.Location = new Point(0, -1);
            toolStrip2.Name = "toolStrip2";
            toolStrip2.Padding = new Padding(0, 0, 2, 0);
            toolStrip2.RenderMode = ToolStripRenderMode.System;
            toolStrip2.Size = new Size(38, 37);
            toolStrip2.TabIndex = 5;
            // 
            // Monitor_Info
            // 
            Monitor_Info.AutoSize = false;
            Monitor_Info.DisplayStyle = ToolStripItemDisplayStyle.Image;
            Monitor_Info.Image = (Image)resources.GetObject("Monitor_Info.Image");
            Monitor_Info.ImageTransparentColor = Color.Magenta;
            Monitor_Info.Name = "Monitor_Info";
            Monitor_Info.Size = new Size(22, 22);
            Monitor_Info.Tag = "";
            Monitor_Info.ToolTipText = "Monitor Info";
            Monitor_Info.Click += Monitor_Info_Click;
            // 
            // DM_AlwaysUseSecondaryMonitor
            // 
            DM_AlwaysUseSecondaryMonitor.AutoSize = true;
            DM_AlwaysUseSecondaryMonitor.Location = new Point(14, 112);
            DM_AlwaysUseSecondaryMonitor.Margin = new Padding(5, 4, 5, 4);
            DM_AlwaysUseSecondaryMonitor.Name = "DM_AlwaysUseSecondaryMonitor";
            DM_AlwaysUseSecondaryMonitor.Size = new Size(264, 24);
            DM_AlwaysUseSecondaryMonitor.TabIndex = 0;
            DM_AlwaysUseSecondaryMonitor.Text = "Always attempt Secondary Monitor";
            // 
            // label46
            // 
            label46.AutoSize = true;
            label46.Location = new Point(201, 187);
            label46.Margin = new Padding(5, 0, 5, 0);
            label46.Name = "label46";
            label46.Size = new Size(57, 20);
            label46.TabIndex = 40;
            label46.Text = "Height:";
            // 
            // label43
            // 
            label43.AutoSize = true;
            label43.Location = new Point(96, 184);
            label43.Margin = new Padding(5, 0, 5, 0);
            label43.Name = "label43";
            label43.Size = new Size(37, 20);
            label43.TabIndex = 34;
            label43.Text = "Left:";
            // 
            // label45
            // 
            label45.AutoSize = true;
            label45.Location = new Point(203, 145);
            label45.Margin = new Padding(5, 0, 5, 0);
            label45.Name = "label45";
            label45.Size = new Size(52, 20);
            label45.TabIndex = 38;
            label45.Text = "Width:";
            // 
            // tabPageAlerts
            // 
            tabPageAlerts.BackColor = SystemColors.Control;
            tabPageAlerts.Controls.Add(groupBox8);
            tabPageAlerts.Controls.Add(groupBox16);
            tabPageAlerts.Controls.Add(groupBox15);
            tabPageAlerts.Location = new Point(4, 29);
            tabPageAlerts.Margin = new Padding(5, 4, 5, 4);
            tabPageAlerts.Name = "tabPageAlerts";
            tabPageAlerts.Padding = new Padding(5, 4, 5, 4);
            tabPageAlerts.Size = new Size(719, 538);
            tabPageAlerts.TabIndex = 2;
            tabPageAlerts.Text = "Alerts";
            // 
            // groupBox8
            // 
            groupBox8.Controls.Add(groupBox11);
            groupBox8.Controls.Add(groupBox9);
            groupBox8.Controls.Add(panel10);
            groupBox8.Controls.Add(panel21);
            groupBox8.Controls.Add(panel22);
            groupBox8.Controls.Add(panel33);
            groupBox8.Controls.Add(panel36);
            groupBox8.Location = new Point(363, 16);
            groupBox8.Margin = new Padding(5, 4, 5, 4);
            groupBox8.Name = "groupBox8";
            groupBox8.Padding = new Padding(5, 4, 5, 4);
            groupBox8.Size = new Size(342, 500);
            groupBox8.TabIndex = 2;
            groupBox8.TabStop = false;
            groupBox8.Text = "Display Reference On New Item";
            // 
            // groupBox11
            // 
            groupBox11.Controls.Add(cbPickBlank);
            groupBox11.Controls.Add(tbSubstitute);
            groupBox11.Controls.Add(cbPick);
            groupBox11.Controls.Add(tbPick);
            groupBox11.Controls.Add(label42);
            groupBox11.Controls.Add(label24);
            groupBox11.Location = new Point(170, 29);
            groupBox11.Margin = new Padding(5, 4, 5, 4);
            groupBox11.Name = "groupBox11";
            groupBox11.Padding = new Padding(5, 4, 5, 4);
            groupBox11.Size = new Size(161, 216);
            groupBox11.TabIndex = 1;
            groupBox11.TabStop = false;
            groupBox11.Text = "Pick && Substitute";
            // 
            // cbPickBlank
            // 
            cbPickBlank.AutoSize = true;
            cbPickBlank.Location = new Point(10, 63);
            cbPickBlank.Margin = new Padding(5, 4, 5, 4);
            cbPickBlank.Name = "cbPickBlank";
            cbPickBlank.Size = new Size(149, 24);
            cbPickBlank.TabIndex = 1;
            cbPickBlank.Text = "Blank if not found";
            // 
            // tbSubstitute
            // 
            tbSubstitute.Location = new Point(10, 176);
            tbSubstitute.Margin = new Padding(5, 4, 5, 4);
            tbSubstitute.MaxLength = 50;
            tbSubstitute.Name = "tbSubstitute";
            tbSubstitute.Size = new Size(139, 27);
            tbSubstitute.TabIndex = 5;
            // 
            // cbPick
            // 
            cbPick.AutoSize = true;
            cbPick.Location = new Point(10, 33);
            cbPick.Margin = new Padding(5, 4, 5, 4);
            cbPick.Name = "cbPick";
            cbPick.Size = new Size(76, 24);
            cbPick.TabIndex = 0;
            cbPick.Text = "Enable";
            // 
            // tbPick
            // 
            tbPick.Location = new Point(10, 116);
            tbPick.Margin = new Padding(5, 4, 5, 4);
            tbPick.MaxLength = 50;
            tbPick.Name = "tbPick";
            tbPick.Size = new Size(82, 27);
            tbPick.TabIndex = 3;
            // 
            // label42
            // 
            label42.AutoSize = true;
            label42.Location = new Point(7, 151);
            label42.Margin = new Padding(5, 0, 5, 0);
            label42.Name = "label42";
            label42.Size = new Size(78, 20);
            label42.TabIndex = 4;
            label42.Text = "Substitute:";
            label42.TextAlign = ContentAlignment.BottomLeft;
            // 
            // label24
            // 
            label24.AutoSize = true;
            label24.Location = new Point(8, 91);
            label24.Margin = new Padding(5, 0, 5, 0);
            label24.Name = "label24";
            label24.Size = new Size(38, 20);
            label24.TabIndex = 2;
            label24.Text = "Pick:";
            label24.TextAlign = ContentAlignment.BottomLeft;
            // 
            // groupBox9
            // 
            groupBox9.Controls.Add(Reference_Source1);
            groupBox9.Controls.Add(Reference_Source2);
            groupBox9.Controls.Add(Reference_Source0);
            groupBox9.Controls.Add(Reference_Source3);
            groupBox9.Controls.Add(Reference_Source4);
            groupBox9.Location = new Point(11, 29);
            groupBox9.Margin = new Padding(5, 4, 5, 4);
            groupBox9.Name = "groupBox9";
            groupBox9.Padding = new Padding(5, 4, 5, 4);
            groupBox9.Size = new Size(153, 216);
            groupBox9.TabIndex = 0;
            groupBox9.TabStop = false;
            groupBox9.Text = "Select Reference";
            // 
            // Reference_Source1
            // 
            Reference_Source1.AutoSize = true;
            Reference_Source1.Location = new Point(10, 68);
            Reference_Source1.Margin = new Padding(5, 4, 5, 4);
            Reference_Source1.Name = "Reference_Source1";
            Reference_Source1.Size = new Size(97, 24);
            Reference_Source1.TabIndex = 1;
            Reference_Source1.Text = "Song Title";
            // 
            // Reference_Source2
            // 
            Reference_Source2.AutoSize = true;
            Reference_Source2.Location = new Point(10, 101);
            Reference_Source2.Margin = new Padding(5, 4, 5, 4);
            Reference_Source2.Name = "Reference_Source2";
            Reference_Source2.Size = new Size(122, 24);
            Reference_Source2.TabIndex = 2;
            Reference_Source2.Text = "Song Number";
            // 
            // Reference_Source0
            // 
            Reference_Source0.AutoSize = true;
            Reference_Source0.Location = new Point(10, 33);
            Reference_Source0.Margin = new Padding(5, 4, 5, 4);
            Reference_Source0.Name = "Reference_Source0";
            Reference_Source0.Size = new Size(120, 24);
            Reference_Source0.TabIndex = 0;
            Reference_Source0.Text = "No Reference";
            // 
            // Reference_Source3
            // 
            Reference_Source3.AutoSize = true;
            Reference_Source3.Location = new Point(10, 136);
            Reference_Source3.Margin = new Padding(5, 4, 5, 4);
            Reference_Source3.Name = "Reference_Source3";
            Reference_Source3.Size = new Size(134, 24);
            Reference_Source3.TabIndex = 3;
            Reference_Source3.Text = "Book Reference";
            // 
            // Reference_Source4
            // 
            Reference_Source4.AutoSize = true;
            Reference_Source4.Location = new Point(10, 171);
            Reference_Source4.Margin = new Padding(5, 4, 5, 4);
            Reference_Source4.Name = "Reference_Source4";
            Reference_Source4.Size = new Size(129, 24);
            Reference_Source4.TabIndex = 4;
            Reference_Source4.Text = "User Reference";
            // 
            // panel10
            // 
            panel10.Controls.Add(btnReferenceChangeBackColour);
            panel10.Controls.Add(btnReferenceChangeTextColour);
            panel10.Location = new Point(14, 419);
            panel10.Margin = new Padding(5, 4, 5, 4);
            panel10.Name = "panel10";
            panel10.Size = new Size(312, 39);
            panel10.TabIndex = 5;
            // 
            // btnReferenceChangeBackColour
            // 
            btnReferenceChangeBackColour.BackColor = Color.LightGray;
            btnReferenceChangeBackColour.FlatStyle = FlatStyle.Flat;
            btnReferenceChangeBackColour.Location = new Point(158, 0);
            btnReferenceChangeBackColour.Margin = new Padding(5, 4, 5, 4);
            btnReferenceChangeBackColour.Name = "btnReferenceChangeBackColour";
            btnReferenceChangeBackColour.Size = new Size(151, 39);
            btnReferenceChangeBackColour.TabIndex = 1;
            btnReferenceChangeBackColour.Text = "Back Colour";
            btnReferenceChangeBackColour.UseVisualStyleBackColor = false;
            btnReferenceChangeBackColour.Click += btnReferenceChangeBackColour_Click;
            // 
            // btnReferenceChangeTextColour
            // 
            btnReferenceChangeTextColour.BackColor = Color.LightGray;
            btnReferenceChangeTextColour.FlatStyle = FlatStyle.Flat;
            btnReferenceChangeTextColour.Location = new Point(6, 0);
            btnReferenceChangeTextColour.Margin = new Padding(5, 4, 5, 4);
            btnReferenceChangeTextColour.Name = "btnReferenceChangeTextColour";
            btnReferenceChangeTextColour.Size = new Size(152, 39);
            btnReferenceChangeTextColour.TabIndex = 0;
            btnReferenceChangeTextColour.Text = "Text Colour";
            btnReferenceChangeTextColour.UseVisualStyleBackColor = false;
            btnReferenceChangeTextColour.Click += btnReferenceChangeTextColour_Click;
            // 
            // panel21
            // 
            panel21.Controls.Add(toolBarReferenceFormat);
            panel21.Location = new Point(14, 267);
            panel21.Margin = new Padding(5, 4, 5, 4);
            panel21.Name = "panel21";
            panel21.Size = new Size(322, 33);
            panel21.TabIndex = 2;
            // 
            // toolBarReferenceFormat
            // 
            toolBarReferenceFormat.AutoSize = false;
            toolBarReferenceFormat.CanOverflow = false;
            toolBarReferenceFormat.Dock = DockStyle.None;
            toolBarReferenceFormat.GripStyle = ToolStripGripStyle.Hidden;
            toolBarReferenceFormat.ImageScalingSize = new Size(24, 24);
            toolBarReferenceFormat.Items.AddRange(new ToolStripItem[] { Reference_Scroll, Reference_Flash, Reference_Transparent, Reference_Align, Reference_VAlign, Reference_Bold, Reference_Italics, Reference_Underline, Reference_Shadow, Reference_Outline });
            toolBarReferenceFormat.LayoutStyle = ToolStripLayoutStyle.HorizontalStackWithOverflow;
            toolBarReferenceFormat.Location = new Point(0, -1);
            toolBarReferenceFormat.Name = "toolBarReferenceFormat";
            toolBarReferenceFormat.Padding = new Padding(0, 0, 2, 0);
            toolBarReferenceFormat.RenderMode = ToolStripRenderMode.System;
            toolBarReferenceFormat.Size = new Size(327, 39);
            toolBarReferenceFormat.TabIndex = 0;
            // 
            // Reference_Scroll
            // 
            Reference_Scroll.CheckOnClick = true;
            Reference_Scroll.DisplayStyle = ToolStripItemDisplayStyle.Image;
            Reference_Scroll.Image = (Image)resources.GetObject("Reference_Scroll.Image");
            Reference_Scroll.ImageTransparentColor = Color.Magenta;
            Reference_Scroll.Name = "Reference_Scroll";
            Reference_Scroll.Size = new Size(29, 36);
            Reference_Scroll.ToolTipText = "Scroll";
            // 
            // Reference_Flash
            // 
            Reference_Flash.CheckOnClick = true;
            Reference_Flash.DisplayStyle = ToolStripItemDisplayStyle.Image;
            Reference_Flash.Image = (Image)resources.GetObject("Reference_Flash.Image");
            Reference_Flash.ImageTransparentColor = Color.Magenta;
            Reference_Flash.Name = "Reference_Flash";
            Reference_Flash.Size = new Size(29, 36);
            Reference_Flash.ToolTipText = "Flash";
            // 
            // Reference_Transparent
            // 
            Reference_Transparent.CheckOnClick = true;
            Reference_Transparent.DisplayStyle = ToolStripItemDisplayStyle.Image;
            Reference_Transparent.Image = (Image)resources.GetObject("Reference_Transparent.Image");
            Reference_Transparent.ImageTransparentColor = Color.Magenta;
            Reference_Transparent.Name = "Reference_Transparent";
            Reference_Transparent.Size = new Size(29, 36);
            Reference_Transparent.ToolTipText = "Transparent";
            // 
            // Reference_Align
            // 
            Reference_Align.DisplayStyle = ToolStripItemDisplayStyle.Image;
            Reference_Align.DropDownItems.AddRange(new ToolStripItem[] { Reference_AlignLeft, Reference_AlignCentre, Reference_AlignRight });
            Reference_Align.Image = (Image)resources.GetObject("Reference_Align.Image");
            Reference_Align.ImageTransparentColor = Color.Magenta;
            Reference_Align.Name = "Reference_Align";
            Reference_Align.Size = new Size(38, 36);
            Reference_Align.Tag = "2";
            Reference_Align.DropDownItemClicked += Reference_Align_DropDownItemClicked;
            // 
            // Reference_AlignLeft
            // 
            Reference_AlignLeft.Image = (Image)resources.GetObject("Reference_AlignLeft.Image");
            Reference_AlignLeft.Name = "Reference_AlignLeft";
            Reference_AlignLeft.Size = new Size(174, 26);
            Reference_AlignLeft.Tag = "1";
            Reference_AlignLeft.Text = "Align Left";
            // 
            // Reference_AlignCentre
            // 
            Reference_AlignCentre.Image = (Image)resources.GetObject("Reference_AlignCentre.Image");
            Reference_AlignCentre.Name = "Reference_AlignCentre";
            Reference_AlignCentre.Size = new Size(174, 26);
            Reference_AlignCentre.Tag = "2";
            Reference_AlignCentre.Text = "Align Centre";
            // 
            // Reference_AlignRight
            // 
            Reference_AlignRight.Image = (Image)resources.GetObject("Reference_AlignRight.Image");
            Reference_AlignRight.Name = "Reference_AlignRight";
            Reference_AlignRight.Size = new Size(174, 26);
            Reference_AlignRight.Tag = "3";
            Reference_AlignRight.Text = "Align Right";
            // 
            // Reference_VAlign
            // 
            Reference_VAlign.DisplayStyle = ToolStripItemDisplayStyle.Image;
            Reference_VAlign.DropDownItems.AddRange(new ToolStripItem[] { Reference_VAlignTop, Reference_VAlignCentre, Reference_VAlignBottom });
            Reference_VAlign.Image = (Image)resources.GetObject("Reference_VAlign.Image");
            Reference_VAlign.ImageTransparentColor = Color.Magenta;
            Reference_VAlign.Name = "Reference_VAlign";
            Reference_VAlign.Size = new Size(38, 36);
            Reference_VAlign.Tag = "1";
            Reference_VAlign.ToolTipText = "Vertical Alignment";
            Reference_VAlign.DropDownItemClicked += Reference_VAlign_DropDownItemClicked;
            // 
            // Reference_VAlignTop
            // 
            Reference_VAlignTop.Image = (Image)resources.GetObject("Reference_VAlignTop.Image");
            Reference_VAlignTop.Name = "Reference_VAlignTop";
            Reference_VAlignTop.Size = new Size(219, 26);
            Reference_VAlignTop.Tag = "0";
            Reference_VAlignTop.Text = "Align Top";
            // 
            // Reference_VAlignCentre
            // 
            Reference_VAlignCentre.Image = (Image)resources.GetObject("Reference_VAlignCentre.Image");
            Reference_VAlignCentre.Name = "Reference_VAlignCentre";
            Reference_VAlignCentre.Size = new Size(219, 26);
            Reference_VAlignCentre.Tag = "1";
            Reference_VAlignCentre.Text = "Align Near-Bottom";
            // 
            // Reference_VAlignBottom
            // 
            Reference_VAlignBottom.Image = (Image)resources.GetObject("Reference_VAlignBottom.Image");
            Reference_VAlignBottom.Name = "Reference_VAlignBottom";
            Reference_VAlignBottom.Size = new Size(219, 26);
            Reference_VAlignBottom.Tag = "2";
            Reference_VAlignBottom.Text = "Align Bottom";
            // 
            // Reference_Bold
            // 
            Reference_Bold.CheckOnClick = true;
            Reference_Bold.DisplayStyle = ToolStripItemDisplayStyle.Image;
            Reference_Bold.Image = (Image)resources.GetObject("Reference_Bold.Image");
            Reference_Bold.ImageTransparentColor = Color.Magenta;
            Reference_Bold.Name = "Reference_Bold";
            Reference_Bold.Size = new Size(29, 36);
            // 
            // Reference_Italics
            // 
            Reference_Italics.CheckOnClick = true;
            Reference_Italics.DisplayStyle = ToolStripItemDisplayStyle.Image;
            Reference_Italics.Image = (Image)resources.GetObject("Reference_Italics.Image");
            Reference_Italics.ImageTransparentColor = Color.Magenta;
            Reference_Italics.Name = "Reference_Italics";
            Reference_Italics.Size = new Size(29, 36);
            // 
            // Reference_Underline
            // 
            Reference_Underline.CheckOnClick = true;
            Reference_Underline.DisplayStyle = ToolStripItemDisplayStyle.Image;
            Reference_Underline.Image = (Image)resources.GetObject("Reference_Underline.Image");
            Reference_Underline.ImageTransparentColor = Color.Magenta;
            Reference_Underline.Name = "Reference_Underline";
            Reference_Underline.Size = new Size(29, 36);
            // 
            // Reference_Shadow
            // 
            Reference_Shadow.CheckOnClick = true;
            Reference_Shadow.DisplayStyle = ToolStripItemDisplayStyle.Image;
            Reference_Shadow.Image = (Image)resources.GetObject("Reference_Shadow.Image");
            Reference_Shadow.ImageTransparentColor = Color.Magenta;
            Reference_Shadow.Name = "Reference_Shadow";
            Reference_Shadow.Size = new Size(29, 36);
            // 
            // Reference_Outline
            // 
            Reference_Outline.CheckOnClick = true;
            Reference_Outline.DisplayStyle = ToolStripItemDisplayStyle.Image;
            Reference_Outline.Image = (Image)resources.GetObject("Reference_Outline.Image");
            Reference_Outline.ImageTransparentColor = Color.Magenta;
            Reference_Outline.Name = "Reference_Outline";
            Reference_Outline.Size = new Size(29, 36);
            // 
            // panel22
            // 
            panel22.Controls.Add(label38);
            panel22.Controls.Add(toolStrip5);
            panel22.Location = new Point(14, 303);
            panel22.Margin = new Padding(5, 4, 5, 4);
            panel22.Name = "panel22";
            panel22.Size = new Size(312, 33);
            panel22.TabIndex = 2;
            // 
            // label38
            // 
            label38.AutoSize = true;
            label38.Location = new Point(5, 7);
            label38.Margin = new Padding(5, 0, 5, 0);
            label38.Name = "label38";
            label38.Size = new Size(85, 20);
            label38.TabIndex = 0;
            label38.Text = "Font Name:";
            // 
            // toolStrip5
            // 
            toolStrip5.AutoSize = false;
            toolStrip5.CanOverflow = false;
            toolStrip5.Dock = DockStyle.None;
            toolStrip5.GripStyle = ToolStripGripStyle.Hidden;
            toolStrip5.ImageScalingSize = new Size(24, 24);
            toolStrip5.Items.AddRange(new ToolStripItem[] { ReferenceComboFont });
            toolStrip5.LayoutStyle = ToolStripLayoutStyle.HorizontalStackWithOverflow;
            toolStrip5.Location = new Point(106, -1);
            toolStrip5.Name = "toolStrip5";
            toolStrip5.Padding = new Padding(0, 0, 2, 0);
            toolStrip5.RenderMode = ToolStripRenderMode.System;
            toolStrip5.Size = new Size(202, 39);
            toolStrip5.TabIndex = 5;
            // 
            // ReferenceComboFont
            // 
            ReferenceComboFont.AutoSize = false;
            ReferenceComboFont.DropDownStyle = ComboBoxStyle.DropDownList;
            ReferenceComboFont.MaxDropDownItems = 12;
            ReferenceComboFont.Name = "ReferenceComboFont";
            ReferenceComboFont.Size = new Size(198, 28);
            // 
            // panel33
            // 
            panel33.Controls.Add(ReferenceSizeUpDown);
            panel33.Controls.Add(label40);
            panel33.Location = new Point(14, 377);
            panel33.Margin = new Padding(5, 4, 5, 4);
            panel33.Name = "panel33";
            panel33.Size = new Size(312, 39);
            panel33.TabIndex = 4;
            // 
            // ReferenceSizeUpDown
            // 
            ReferenceSizeUpDown.Location = new Point(235, 4);
            ReferenceSizeUpDown.Margin = new Padding(5, 4, 5, 4);
            ReferenceSizeUpDown.Maximum = new decimal(new int[] { 50, 0, 0, 0 });
            ReferenceSizeUpDown.Minimum = new decimal(new int[] { 15, 0, 0, 0 });
            ReferenceSizeUpDown.Name = "ReferenceSizeUpDown";
            ReferenceSizeUpDown.Size = new Size(72, 27);
            ReferenceSizeUpDown.TabIndex = 17;
            ReferenceSizeUpDown.Value = new decimal(new int[] { 20, 0, 0, 0 });
            // 
            // label40
            // 
            label40.AutoSize = true;
            label40.Location = new Point(5, 8);
            label40.Margin = new Padding(5, 0, 5, 0);
            label40.Name = "label40";
            label40.Size = new Size(104, 20);
            label40.TabIndex = 0;
            label40.Text = "Font Size Max:";
            // 
            // panel36
            // 
            panel36.Controls.Add(ReferenceAlertDurationUpDown);
            panel36.Controls.Add(label41);
            panel36.Location = new Point(14, 340);
            panel36.Margin = new Padding(5, 4, 5, 4);
            panel36.Name = "panel36";
            panel36.Size = new Size(312, 39);
            panel36.TabIndex = 3;
            // 
            // ReferenceAlertDurationUpDown
            // 
            ReferenceAlertDurationUpDown.Location = new Point(235, 4);
            ReferenceAlertDurationUpDown.Margin = new Padding(5, 4, 5, 4);
            ReferenceAlertDurationUpDown.Maximum = new decimal(new int[] { 60, 0, 0, 0 });
            ReferenceAlertDurationUpDown.Name = "ReferenceAlertDurationUpDown";
            ReferenceAlertDurationUpDown.Size = new Size(72, 27);
            ReferenceAlertDurationUpDown.TabIndex = 17;
            // 
            // label41
            // 
            label41.AutoSize = true;
            label41.Location = new Point(5, 8);
            label41.Margin = new Padding(5, 0, 5, 0);
            label41.Name = "label41";
            label41.Size = new Size(111, 20);
            label41.TabIndex = 0;
            label41.Text = "Duration (secs):";
            // 
            // groupBox16
            // 
            groupBox16.Controls.Add(ParentalAlert);
            groupBox16.Controls.Add(panel23);
            groupBox16.Controls.Add(label30);
            groupBox16.Controls.Add(panel24);
            groupBox16.Controls.Add(panel25);
            groupBox16.Controls.Add(panel26);
            groupBox16.Controls.Add(panel27);
            groupBox16.Location = new Point(10, 251);
            groupBox16.Margin = new Padding(5, 4, 5, 4);
            groupBox16.Name = "groupBox16";
            groupBox16.Padding = new Padding(5, 4, 5, 4);
            groupBox16.Size = new Size(343, 261);
            groupBox16.TabIndex = 1;
            groupBox16.TabStop = false;
            groupBox16.Text = "Parental Alert";
            // 
            // ParentalAlert
            // 
            ParentalAlert.Location = new Point(128, 227);
            ParentalAlert.Margin = new Padding(5, 4, 5, 4);
            ParentalAlert.MaxLength = 50;
            ParentalAlert.Name = "ParentalAlert";
            ParentalAlert.Size = new Size(195, 27);
            ParentalAlert.TabIndex = 6;
            // 
            // panel23
            // 
            panel23.Controls.Add(btnParentalChangeBackColour);
            panel23.Controls.Add(btnParentalChangeTextColour);
            panel23.Location = new Point(15, 183);
            panel23.Margin = new Padding(5, 4, 5, 4);
            panel23.Name = "panel23";
            panel23.Size = new Size(312, 39);
            panel23.TabIndex = 4;
            // 
            // btnParentalChangeBackColour
            // 
            btnParentalChangeBackColour.BackColor = Color.LightGray;
            btnParentalChangeBackColour.FlatStyle = FlatStyle.Flat;
            btnParentalChangeBackColour.Location = new Point(158, 0);
            btnParentalChangeBackColour.Margin = new Padding(5, 4, 5, 4);
            btnParentalChangeBackColour.Name = "btnParentalChangeBackColour";
            btnParentalChangeBackColour.Size = new Size(151, 39);
            btnParentalChangeBackColour.TabIndex = 1;
            btnParentalChangeBackColour.Text = "Back Colour";
            btnParentalChangeBackColour.UseVisualStyleBackColor = false;
            btnParentalChangeBackColour.Click += btnParentalChangeBackColour_Click;
            // 
            // btnParentalChangeTextColour
            // 
            btnParentalChangeTextColour.BackColor = Color.LightGray;
            btnParentalChangeTextColour.FlatStyle = FlatStyle.Flat;
            btnParentalChangeTextColour.Location = new Point(6, 0);
            btnParentalChangeTextColour.Margin = new Padding(5, 4, 5, 4);
            btnParentalChangeTextColour.Name = "btnParentalChangeTextColour";
            btnParentalChangeTextColour.Size = new Size(152, 39);
            btnParentalChangeTextColour.TabIndex = 0;
            btnParentalChangeTextColour.Text = "Text Colour";
            btnParentalChangeTextColour.UseVisualStyleBackColor = false;
            btnParentalChangeTextColour.Click += btnParentalChangeTextColour_Click;
            // 
            // label30
            // 
            label30.AutoSize = true;
            label30.Location = new Point(21, 229);
            label30.Margin = new Padding(5, 0, 5, 0);
            label30.Name = "label30";
            label30.Size = new Size(111, 20);
            label30.TabIndex = 5;
            label30.Text = "Optional Prefix:";
            label30.TextAlign = ContentAlignment.BottomLeft;
            // 
            // panel24
            // 
            panel24.Controls.Add(ToolBarParentalFormat);
            panel24.Location = new Point(15, 33);
            panel24.Margin = new Padding(5, 4, 5, 4);
            panel24.Name = "panel24";
            panel24.Size = new Size(327, 33);
            panel24.TabIndex = 21;
            // 
            // ToolBarParentalFormat
            // 
            ToolBarParentalFormat.AutoSize = false;
            ToolBarParentalFormat.CanOverflow = false;
            ToolBarParentalFormat.Dock = DockStyle.None;
            ToolBarParentalFormat.GripStyle = ToolStripGripStyle.Hidden;
            ToolBarParentalFormat.ImageScalingSize = new Size(24, 24);
            ToolBarParentalFormat.Items.AddRange(new ToolStripItem[] { Parental_Scroll, Parental_Flash, Parental_Transparent, Parental_Align, Parental_VAlign, Parental_Bold, Parental_Italics, Parental_Underline, Parental_Shadow, Parental_Outline });
            ToolBarParentalFormat.LayoutStyle = ToolStripLayoutStyle.HorizontalStackWithOverflow;
            ToolBarParentalFormat.Location = new Point(0, -1);
            ToolBarParentalFormat.Name = "ToolBarParentalFormat";
            ToolBarParentalFormat.Padding = new Padding(0, 0, 2, 0);
            ToolBarParentalFormat.RenderMode = ToolStripRenderMode.System;
            ToolBarParentalFormat.Size = new Size(327, 39);
            ToolBarParentalFormat.TabIndex = 0;
            // 
            // Parental_Scroll
            // 
            Parental_Scroll.CheckOnClick = true;
            Parental_Scroll.DisplayStyle = ToolStripItemDisplayStyle.Image;
            Parental_Scroll.Image = (Image)resources.GetObject("Parental_Scroll.Image");
            Parental_Scroll.ImageTransparentColor = Color.Magenta;
            Parental_Scroll.Name = "Parental_Scroll";
            Parental_Scroll.Size = new Size(29, 36);
            Parental_Scroll.ToolTipText = "Scroll";
            // 
            // Parental_Flash
            // 
            Parental_Flash.CheckOnClick = true;
            Parental_Flash.DisplayStyle = ToolStripItemDisplayStyle.Image;
            Parental_Flash.Image = (Image)resources.GetObject("Parental_Flash.Image");
            Parental_Flash.ImageTransparentColor = Color.Magenta;
            Parental_Flash.Name = "Parental_Flash";
            Parental_Flash.Size = new Size(29, 36);
            Parental_Flash.ToolTipText = "Flash";
            // 
            // Parental_Transparent
            // 
            Parental_Transparent.CheckOnClick = true;
            Parental_Transparent.DisplayStyle = ToolStripItemDisplayStyle.Image;
            Parental_Transparent.Image = (Image)resources.GetObject("Parental_Transparent.Image");
            Parental_Transparent.ImageTransparentColor = Color.Magenta;
            Parental_Transparent.Name = "Parental_Transparent";
            Parental_Transparent.Size = new Size(29, 36);
            Parental_Transparent.ToolTipText = "Transparent";
            // 
            // Parental_Align
            // 
            Parental_Align.DisplayStyle = ToolStripItemDisplayStyle.Image;
            Parental_Align.DropDownItems.AddRange(new ToolStripItem[] { Parental_AlignLeft, Parental_AlignCentre, Parental_AlignRight });
            Parental_Align.Image = (Image)resources.GetObject("Parental_Align.Image");
            Parental_Align.ImageTransparentColor = Color.Magenta;
            Parental_Align.Name = "Parental_Align";
            Parental_Align.Size = new Size(38, 36);
            Parental_Align.Tag = "2";
            Parental_Align.DropDownItemClicked += Parental_Align_DropDownItemClicked;
            // 
            // Parental_AlignLeft
            // 
            Parental_AlignLeft.Image = (Image)resources.GetObject("Parental_AlignLeft.Image");
            Parental_AlignLeft.Name = "Parental_AlignLeft";
            Parental_AlignLeft.Size = new Size(174, 26);
            Parental_AlignLeft.Tag = "1";
            Parental_AlignLeft.Text = "Align Left";
            // 
            // Parental_AlignCentre
            // 
            Parental_AlignCentre.Image = (Image)resources.GetObject("Parental_AlignCentre.Image");
            Parental_AlignCentre.Name = "Parental_AlignCentre";
            Parental_AlignCentre.Size = new Size(174, 26);
            Parental_AlignCentre.Tag = "2";
            Parental_AlignCentre.Text = "Align Centre";
            // 
            // Parental_AlignRight
            // 
            Parental_AlignRight.Image = (Image)resources.GetObject("Parental_AlignRight.Image");
            Parental_AlignRight.Name = "Parental_AlignRight";
            Parental_AlignRight.Size = new Size(174, 26);
            Parental_AlignRight.Tag = "3";
            Parental_AlignRight.Text = "Align Right";
            // 
            // Parental_VAlign
            // 
            Parental_VAlign.DisplayStyle = ToolStripItemDisplayStyle.Image;
            Parental_VAlign.DropDownItems.AddRange(new ToolStripItem[] { Parental_VAlignTop, Parental_VAlignBottom });
            Parental_VAlign.Image = (Image)resources.GetObject("Parental_VAlign.Image");
            Parental_VAlign.ImageTransparentColor = Color.Magenta;
            Parental_VAlign.Name = "Parental_VAlign";
            Parental_VAlign.Size = new Size(38, 36);
            Parental_VAlign.Tag = "2";
            Parental_VAlign.Text = "Align Bottom";
            Parental_VAlign.ToolTipText = "Alert At Bottom of Screen";
            Parental_VAlign.DropDownItemClicked += Parental_VAlign_DropDownItemClicked;
            // 
            // Parental_VAlignTop
            // 
            Parental_VAlignTop.Image = (Image)resources.GetObject("Parental_VAlignTop.Image");
            Parental_VAlignTop.Name = "Parental_VAlignTop";
            Parental_VAlignTop.Size = new Size(181, 26);
            Parental_VAlignTop.Tag = "0";
            Parental_VAlignTop.Text = "Align Top";
            // 
            // Parental_VAlignBottom
            // 
            Parental_VAlignBottom.Image = (Image)resources.GetObject("Parental_VAlignBottom.Image");
            Parental_VAlignBottom.Name = "Parental_VAlignBottom";
            Parental_VAlignBottom.Size = new Size(181, 26);
            Parental_VAlignBottom.Tag = "2";
            Parental_VAlignBottom.Text = "Align Bottom";
            // 
            // Parental_Bold
            // 
            Parental_Bold.CheckOnClick = true;
            Parental_Bold.DisplayStyle = ToolStripItemDisplayStyle.Image;
            Parental_Bold.Image = (Image)resources.GetObject("Parental_Bold.Image");
            Parental_Bold.ImageTransparentColor = Color.Magenta;
            Parental_Bold.Name = "Parental_Bold";
            Parental_Bold.Size = new Size(29, 36);
            // 
            // Parental_Italics
            // 
            Parental_Italics.CheckOnClick = true;
            Parental_Italics.DisplayStyle = ToolStripItemDisplayStyle.Image;
            Parental_Italics.Image = (Image)resources.GetObject("Parental_Italics.Image");
            Parental_Italics.ImageTransparentColor = Color.Magenta;
            Parental_Italics.Name = "Parental_Italics";
            Parental_Italics.Size = new Size(29, 36);
            // 
            // Parental_Underline
            // 
            Parental_Underline.CheckOnClick = true;
            Parental_Underline.DisplayStyle = ToolStripItemDisplayStyle.Image;
            Parental_Underline.Image = (Image)resources.GetObject("Parental_Underline.Image");
            Parental_Underline.ImageTransparentColor = Color.Magenta;
            Parental_Underline.Name = "Parental_Underline";
            Parental_Underline.Size = new Size(29, 36);
            // 
            // Parental_Shadow
            // 
            Parental_Shadow.CheckOnClick = true;
            Parental_Shadow.DisplayStyle = ToolStripItemDisplayStyle.Image;
            Parental_Shadow.Image = (Image)resources.GetObject("Parental_Shadow.Image");
            Parental_Shadow.ImageTransparentColor = Color.Magenta;
            Parental_Shadow.Name = "Parental_Shadow";
            Parental_Shadow.Size = new Size(29, 36);
            // 
            // Parental_Outline
            // 
            Parental_Outline.CheckOnClick = true;
            Parental_Outline.DisplayStyle = ToolStripItemDisplayStyle.Image;
            Parental_Outline.Image = (Image)resources.GetObject("Parental_Outline.Image");
            Parental_Outline.ImageTransparentColor = Color.Magenta;
            Parental_Outline.Name = "Parental_Outline";
            Parental_Outline.Size = new Size(29, 36);
            // 
            // panel25
            // 
            panel25.Controls.Add(label27);
            panel25.Controls.Add(toolStrip14);
            panel25.Location = new Point(15, 71);
            panel25.Margin = new Padding(5, 4, 5, 4);
            panel25.Name = "panel25";
            panel25.Size = new Size(312, 33);
            panel25.TabIndex = 1;
            // 
            // label27
            // 
            label27.AutoSize = true;
            label27.Location = new Point(5, 7);
            label27.Margin = new Padding(5, 0, 5, 0);
            label27.Name = "label27";
            label27.Size = new Size(85, 20);
            label27.TabIndex = 0;
            label27.Text = "Font Name:";
            // 
            // toolStrip14
            // 
            toolStrip14.AutoSize = false;
            toolStrip14.CanOverflow = false;
            toolStrip14.Dock = DockStyle.None;
            toolStrip14.GripStyle = ToolStripGripStyle.Hidden;
            toolStrip14.ImageScalingSize = new Size(24, 24);
            toolStrip14.Items.AddRange(new ToolStripItem[] { ParentalComboFont });
            toolStrip14.LayoutStyle = ToolStripLayoutStyle.HorizontalStackWithOverflow;
            toolStrip14.Location = new Point(106, -1);
            toolStrip14.Name = "toolStrip14";
            toolStrip14.Padding = new Padding(0, 0, 2, 0);
            toolStrip14.RenderMode = ToolStripRenderMode.System;
            toolStrip14.Size = new Size(202, 39);
            toolStrip14.TabIndex = 5;
            // 
            // ParentalComboFont
            // 
            ParentalComboFont.AutoSize = false;
            ParentalComboFont.DropDownStyle = ComboBoxStyle.DropDownList;
            ParentalComboFont.MaxDropDownItems = 12;
            ParentalComboFont.Name = "ParentalComboFont";
            ParentalComboFont.Size = new Size(198, 28);
            // 
            // panel26
            // 
            panel26.Controls.Add(ParentalSizeUpDown);
            panel26.Controls.Add(label28);
            panel26.Location = new Point(15, 144);
            panel26.Margin = new Padding(5, 4, 5, 4);
            panel26.Name = "panel26";
            panel26.Size = new Size(312, 39);
            panel26.TabIndex = 3;
            // 
            // ParentalSizeUpDown
            // 
            ParentalSizeUpDown.Location = new Point(235, 4);
            ParentalSizeUpDown.Margin = new Padding(5, 4, 5, 4);
            ParentalSizeUpDown.Maximum = new decimal(new int[] { 50, 0, 0, 0 });
            ParentalSizeUpDown.Minimum = new decimal(new int[] { 20, 0, 0, 0 });
            ParentalSizeUpDown.Name = "ParentalSizeUpDown";
            ParentalSizeUpDown.Size = new Size(72, 27);
            ParentalSizeUpDown.TabIndex = 17;
            ParentalSizeUpDown.Value = new decimal(new int[] { 20, 0, 0, 0 });
            // 
            // label28
            // 
            label28.AutoSize = true;
            label28.Location = new Point(5, 8);
            label28.Margin = new Padding(5, 0, 5, 0);
            label28.Name = "label28";
            label28.Size = new Size(104, 20);
            label28.TabIndex = 0;
            label28.Text = "Font Size Max:";
            // 
            // panel27
            // 
            panel27.Controls.Add(ParentalAlertUpDown);
            panel27.Controls.Add(label29);
            panel27.Location = new Point(15, 108);
            panel27.Margin = new Padding(5, 4, 5, 4);
            panel27.Name = "panel27";
            panel27.Size = new Size(312, 39);
            panel27.TabIndex = 2;
            // 
            // ParentalAlertUpDown
            // 
            ParentalAlertUpDown.Location = new Point(235, 4);
            ParentalAlertUpDown.Margin = new Padding(5, 4, 5, 4);
            ParentalAlertUpDown.Maximum = new decimal(new int[] { 60, 0, 0, 0 });
            ParentalAlertUpDown.Name = "ParentalAlertUpDown";
            ParentalAlertUpDown.Size = new Size(72, 27);
            ParentalAlertUpDown.TabIndex = 17;
            // 
            // label29
            // 
            label29.AutoSize = true;
            label29.Location = new Point(5, 8);
            label29.Margin = new Padding(5, 0, 5, 0);
            label29.Name = "label29";
            label29.Size = new Size(111, 20);
            label29.TabIndex = 0;
            label29.Text = "Duration (secs):";
            // 
            // groupBox15
            // 
            groupBox15.Controls.Add(panel20);
            groupBox15.Controls.Add(panel12);
            groupBox15.Controls.Add(panel19);
            groupBox15.Controls.Add(panel13);
            groupBox15.Controls.Add(panel11);
            groupBox15.Location = new Point(10, 16);
            groupBox15.Margin = new Padding(5, 4, 5, 4);
            groupBox15.Name = "groupBox15";
            groupBox15.Padding = new Padding(5, 4, 5, 4);
            groupBox15.Size = new Size(343, 228);
            groupBox15.TabIndex = 0;
            groupBox15.TabStop = false;
            groupBox15.Text = "Message Alert";
            // 
            // panel20
            // 
            panel20.Controls.Add(btnMessageChangeBackColour);
            panel20.Controls.Add(btnMessageChangeTextColour);
            panel20.Location = new Point(15, 181);
            panel20.Margin = new Padding(5, 4, 5, 4);
            panel20.Name = "panel20";
            panel20.Size = new Size(312, 39);
            panel20.TabIndex = 4;
            // 
            // btnMessageChangeBackColour
            // 
            btnMessageChangeBackColour.BackColor = Color.LightGray;
            btnMessageChangeBackColour.FlatStyle = FlatStyle.Flat;
            btnMessageChangeBackColour.Location = new Point(158, 0);
            btnMessageChangeBackColour.Margin = new Padding(5, 4, 5, 4);
            btnMessageChangeBackColour.Name = "btnMessageChangeBackColour";
            btnMessageChangeBackColour.Size = new Size(151, 39);
            btnMessageChangeBackColour.TabIndex = 1;
            btnMessageChangeBackColour.Text = "Back Colour";
            btnMessageChangeBackColour.UseVisualStyleBackColor = false;
            btnMessageChangeBackColour.Click += btnMessageChangeBackColour_Click;
            // 
            // btnMessageChangeTextColour
            // 
            btnMessageChangeTextColour.BackColor = Color.LightGray;
            btnMessageChangeTextColour.FlatStyle = FlatStyle.Flat;
            btnMessageChangeTextColour.Location = new Point(6, 0);
            btnMessageChangeTextColour.Margin = new Padding(5, 4, 5, 4);
            btnMessageChangeTextColour.Name = "btnMessageChangeTextColour";
            btnMessageChangeTextColour.Size = new Size(152, 39);
            btnMessageChangeTextColour.TabIndex = 0;
            btnMessageChangeTextColour.Text = "Text Colour";
            btnMessageChangeTextColour.UseVisualStyleBackColor = false;
            btnMessageChangeTextColour.Click += btnMessageChangeTextColour_Click;
            // 
            // panel12
            // 
            panel12.Controls.Add(ToolBarMessageFormat);
            panel12.Location = new Point(15, 29);
            panel12.Margin = new Padding(5, 4, 5, 4);
            panel12.Name = "panel12";
            panel12.Size = new Size(326, 33);
            panel12.TabIndex = 0;
            // 
            // ToolBarMessageFormat
            // 
            ToolBarMessageFormat.AutoSize = false;
            ToolBarMessageFormat.CanOverflow = false;
            ToolBarMessageFormat.Dock = DockStyle.None;
            ToolBarMessageFormat.GripStyle = ToolStripGripStyle.Hidden;
            ToolBarMessageFormat.ImageScalingSize = new Size(24, 24);
            ToolBarMessageFormat.Items.AddRange(new ToolStripItem[] { Message_Scroll, Message_Flash, Message_Transparent, Message_Align, Message_VAlign, Message_Bold, Message_Italics, Message_Underline, Message_Shadow, Message_Outline });
            ToolBarMessageFormat.LayoutStyle = ToolStripLayoutStyle.HorizontalStackWithOverflow;
            ToolBarMessageFormat.Location = new Point(0, -1);
            ToolBarMessageFormat.Name = "ToolBarMessageFormat";
            ToolBarMessageFormat.Padding = new Padding(0, 0, 2, 0);
            ToolBarMessageFormat.RenderMode = ToolStripRenderMode.System;
            ToolBarMessageFormat.Size = new Size(327, 39);
            ToolBarMessageFormat.TabIndex = 0;
            // 
            // Message_Scroll
            // 
            Message_Scroll.CheckOnClick = true;
            Message_Scroll.DisplayStyle = ToolStripItemDisplayStyle.Image;
            Message_Scroll.Image = (Image)resources.GetObject("Message_Scroll.Image");
            Message_Scroll.ImageTransparentColor = Color.Magenta;
            Message_Scroll.Name = "Message_Scroll";
            Message_Scroll.Size = new Size(29, 36);
            Message_Scroll.ToolTipText = "Scroll";
            // 
            // Message_Flash
            // 
            Message_Flash.CheckOnClick = true;
            Message_Flash.DisplayStyle = ToolStripItemDisplayStyle.Image;
            Message_Flash.Image = (Image)resources.GetObject("Message_Flash.Image");
            Message_Flash.ImageTransparentColor = Color.Magenta;
            Message_Flash.Name = "Message_Flash";
            Message_Flash.Size = new Size(29, 36);
            Message_Flash.ToolTipText = "Flash";
            // 
            // Message_Transparent
            // 
            Message_Transparent.CheckOnClick = true;
            Message_Transparent.DisplayStyle = ToolStripItemDisplayStyle.Image;
            Message_Transparent.Image = (Image)resources.GetObject("Message_Transparent.Image");
            Message_Transparent.ImageTransparentColor = Color.Magenta;
            Message_Transparent.Name = "Message_Transparent";
            Message_Transparent.Size = new Size(29, 36);
            Message_Transparent.ToolTipText = "Transparent";
            // 
            // Message_Align
            // 
            Message_Align.DisplayStyle = ToolStripItemDisplayStyle.Image;
            Message_Align.DropDownItems.AddRange(new ToolStripItem[] { Message_AlignLeft, Message_AlignCentre, Message_AlignRight });
            Message_Align.Image = (Image)resources.GetObject("Message_Align.Image");
            Message_Align.ImageTransparentColor = Color.Magenta;
            Message_Align.Name = "Message_Align";
            Message_Align.Size = new Size(38, 36);
            Message_Align.Tag = "2";
            Message_Align.DropDownItemClicked += Message_Align_DropDownItemClicked;
            // 
            // Message_AlignLeft
            // 
            Message_AlignLeft.Image = (Image)resources.GetObject("Message_AlignLeft.Image");
            Message_AlignLeft.Name = "Message_AlignLeft";
            Message_AlignLeft.Size = new Size(174, 26);
            Message_AlignLeft.Tag = "1";
            Message_AlignLeft.Text = "Align Left";
            // 
            // Message_AlignCentre
            // 
            Message_AlignCentre.Image = (Image)resources.GetObject("Message_AlignCentre.Image");
            Message_AlignCentre.Name = "Message_AlignCentre";
            Message_AlignCentre.Size = new Size(174, 26);
            Message_AlignCentre.Tag = "2";
            Message_AlignCentre.Text = "Align Centre";
            // 
            // Message_AlignRight
            // 
            Message_AlignRight.Image = (Image)resources.GetObject("Message_AlignRight.Image");
            Message_AlignRight.Name = "Message_AlignRight";
            Message_AlignRight.Size = new Size(174, 26);
            Message_AlignRight.Tag = "3";
            Message_AlignRight.Text = "Align Right";
            // 
            // Message_VAlign
            // 
            Message_VAlign.DisplayStyle = ToolStripItemDisplayStyle.Image;
            Message_VAlign.DropDownItems.AddRange(new ToolStripItem[] { Message_VAlignTop, Message_VAlignBottom });
            Message_VAlign.Image = (Image)resources.GetObject("Message_VAlign.Image");
            Message_VAlign.ImageTransparentColor = Color.Magenta;
            Message_VAlign.Name = "Message_VAlign";
            Message_VAlign.Size = new Size(38, 36);
            Message_VAlign.Tag = "2";
            Message_VAlign.Text = "Align Bottom";
            Message_VAlign.DropDownItemClicked += Message_VAlign_DropDownItemClicked;
            // 
            // Message_VAlignTop
            // 
            Message_VAlignTop.Image = (Image)resources.GetObject("Message_VAlignTop.Image");
            Message_VAlignTop.Name = "Message_VAlignTop";
            Message_VAlignTop.Size = new Size(181, 26);
            Message_VAlignTop.Tag = "0";
            Message_VAlignTop.Text = "Align Top";
            // 
            // Message_VAlignBottom
            // 
            Message_VAlignBottom.Image = (Image)resources.GetObject("Message_VAlignBottom.Image");
            Message_VAlignBottom.Name = "Message_VAlignBottom";
            Message_VAlignBottom.Size = new Size(181, 26);
            Message_VAlignBottom.Tag = "2";
            Message_VAlignBottom.Text = "Align Bottom";
            // 
            // Message_Bold
            // 
            Message_Bold.CheckOnClick = true;
            Message_Bold.DisplayStyle = ToolStripItemDisplayStyle.Image;
            Message_Bold.Image = (Image)resources.GetObject("Message_Bold.Image");
            Message_Bold.ImageTransparentColor = Color.Magenta;
            Message_Bold.Name = "Message_Bold";
            Message_Bold.Size = new Size(29, 36);
            // 
            // Message_Italics
            // 
            Message_Italics.CheckOnClick = true;
            Message_Italics.DisplayStyle = ToolStripItemDisplayStyle.Image;
            Message_Italics.Image = (Image)resources.GetObject("Message_Italics.Image");
            Message_Italics.ImageTransparentColor = Color.Magenta;
            Message_Italics.Name = "Message_Italics";
            Message_Italics.Size = new Size(29, 36);
            // 
            // Message_Underline
            // 
            Message_Underline.CheckOnClick = true;
            Message_Underline.DisplayStyle = ToolStripItemDisplayStyle.Image;
            Message_Underline.Image = (Image)resources.GetObject("Message_Underline.Image");
            Message_Underline.ImageTransparentColor = Color.Magenta;
            Message_Underline.Name = "Message_Underline";
            Message_Underline.Size = new Size(29, 36);
            // 
            // Message_Shadow
            // 
            Message_Shadow.CheckOnClick = true;
            Message_Shadow.DisplayStyle = ToolStripItemDisplayStyle.Image;
            Message_Shadow.Image = (Image)resources.GetObject("Message_Shadow.Image");
            Message_Shadow.ImageTransparentColor = Color.Magenta;
            Message_Shadow.Name = "Message_Shadow";
            Message_Shadow.Size = new Size(29, 36);
            // 
            // Message_Outline
            // 
            Message_Outline.CheckOnClick = true;
            Message_Outline.DisplayStyle = ToolStripItemDisplayStyle.Image;
            Message_Outline.Image = (Image)resources.GetObject("Message_Outline.Image");
            Message_Outline.ImageTransparentColor = Color.Magenta;
            Message_Outline.Name = "Message_Outline";
            Message_Outline.Size = new Size(29, 36);
            // 
            // panel19
            // 
            panel19.Controls.Add(label21);
            panel19.Controls.Add(toolStrip11);
            panel19.Location = new Point(15, 69);
            panel19.Margin = new Padding(5, 4, 5, 4);
            panel19.Name = "panel19";
            panel19.Size = new Size(312, 33);
            panel19.TabIndex = 1;
            // 
            // label21
            // 
            label21.AutoSize = true;
            label21.Location = new Point(5, 7);
            label21.Margin = new Padding(5, 0, 5, 0);
            label21.Name = "label21";
            label21.Size = new Size(85, 20);
            label21.TabIndex = 0;
            label21.Text = "Font Name:";
            // 
            // toolStrip11
            // 
            toolStrip11.AutoSize = false;
            toolStrip11.CanOverflow = false;
            toolStrip11.Dock = DockStyle.None;
            toolStrip11.GripStyle = ToolStripGripStyle.Hidden;
            toolStrip11.ImageScalingSize = new Size(24, 24);
            toolStrip11.Items.AddRange(new ToolStripItem[] { MessageComboFont });
            toolStrip11.LayoutStyle = ToolStripLayoutStyle.HorizontalStackWithOverflow;
            toolStrip11.Location = new Point(106, -1);
            toolStrip11.Name = "toolStrip11";
            toolStrip11.Padding = new Padding(0, 0, 2, 0);
            toolStrip11.RenderMode = ToolStripRenderMode.System;
            toolStrip11.Size = new Size(202, 39);
            toolStrip11.TabIndex = 5;
            // 
            // MessageComboFont
            // 
            MessageComboFont.AutoSize = false;
            MessageComboFont.DropDownStyle = ComboBoxStyle.DropDownList;
            MessageComboFont.MaxDropDownItems = 12;
            MessageComboFont.Name = "MessageComboFont";
            MessageComboFont.Size = new Size(198, 28);
            // 
            // panel13
            // 
            panel13.Controls.Add(MessageSizeUpDown);
            panel13.Controls.Add(label22);
            panel13.Location = new Point(15, 143);
            panel13.Margin = new Padding(5, 4, 5, 4);
            panel13.Name = "panel13";
            panel13.Size = new Size(312, 39);
            panel13.TabIndex = 3;
            // 
            // MessageSizeUpDown
            // 
            MessageSizeUpDown.Location = new Point(235, 4);
            MessageSizeUpDown.Margin = new Padding(5, 4, 5, 4);
            MessageSizeUpDown.Maximum = new decimal(new int[] { 50, 0, 0, 0 });
            MessageSizeUpDown.Minimum = new decimal(new int[] { 20, 0, 0, 0 });
            MessageSizeUpDown.Name = "MessageSizeUpDown";
            MessageSizeUpDown.Size = new Size(72, 27);
            MessageSizeUpDown.TabIndex = 17;
            MessageSizeUpDown.Value = new decimal(new int[] { 20, 0, 0, 0 });
            // 
            // label22
            // 
            label22.AutoSize = true;
            label22.Location = new Point(5, 8);
            label22.Margin = new Padding(5, 0, 5, 0);
            label22.Name = "label22";
            label22.Size = new Size(104, 20);
            label22.TabIndex = 0;
            label22.Text = "Font Size Max:";
            // 
            // panel11
            // 
            panel11.Controls.Add(MessageAlertDurationUpDown);
            panel11.Controls.Add(label20);
            panel11.Location = new Point(15, 107);
            panel11.Margin = new Padding(5, 4, 5, 4);
            panel11.Name = "panel11";
            panel11.Size = new Size(312, 39);
            panel11.TabIndex = 2;
            // 
            // MessageAlertDurationUpDown
            // 
            MessageAlertDurationUpDown.Location = new Point(235, 4);
            MessageAlertDurationUpDown.Margin = new Padding(5, 4, 5, 4);
            MessageAlertDurationUpDown.Maximum = new decimal(new int[] { 999, 0, 0, 0 });
            MessageAlertDurationUpDown.Name = "MessageAlertDurationUpDown";
            MessageAlertDurationUpDown.Size = new Size(72, 27);
            MessageAlertDurationUpDown.TabIndex = 17;
            // 
            // label20
            // 
            label20.AutoSize = true;
            label20.Location = new Point(5, 8);
            label20.Margin = new Padding(5, 0, 5, 0);
            label20.Name = "label20";
            label20.Size = new Size(111, 20);
            label20.TabIndex = 0;
            label20.Text = "Duration (secs):";
            // 
            // tabPageFolders
            // 
            tabPageFolders.BackColor = SystemColors.Control;
            tabPageFolders.Controls.Add(SelectedFolderGroupBox);
            tabPageFolders.Controls.Add(GroupBoxHeadings);
            tabPageFolders.Controls.Add(GroupBoxFolder);
            tabPageFolders.Location = new Point(4, 29);
            tabPageFolders.Margin = new Padding(5, 4, 5, 4);
            tabPageFolders.Name = "tabPageFolders";
            tabPageFolders.Padding = new Padding(5, 4, 5, 4);
            tabPageFolders.Size = new Size(719, 538);
            tabPageFolders.TabIndex = 1;
            tabPageFolders.Text = "Folders";
            // 
            // SelectedFolderGroupBox
            // 
            SelectedFolderGroupBox.Controls.Add(groupBox14);
            SelectedFolderGroupBox.Controls.Add(groupBox13);
            SelectedFolderGroupBox.Controls.Add(GroupBoxFont1);
            SelectedFolderGroupBox.Controls.Add(GroupBoxFont0);
            SelectedFolderGroupBox.Controls.Add(groupBox10);
            SelectedFolderGroupBox.Location = new Point(8, 213);
            SelectedFolderGroupBox.Margin = new Padding(5, 4, 5, 4);
            SelectedFolderGroupBox.Name = "SelectedFolderGroupBox";
            SelectedFolderGroupBox.Padding = new Padding(5, 4, 5, 4);
            SelectedFolderGroupBox.Size = new Size(699, 300);
            SelectedFolderGroupBox.TabIndex = 2;
            SelectedFolderGroupBox.TabStop = false;
            SelectedFolderGroupBox.Text = "Settings for the English Folder";
            // 
            // groupBox14
            // 
            groupBox14.Controls.Add(ShowLineSpacing2MaxUpDown);
            groupBox14.Controls.Add(ShowLineSpacingMaxUpDown);
            groupBox14.Location = new Point(519, 213);
            groupBox14.Margin = new Padding(5, 4, 5, 4);
            groupBox14.Name = "groupBox14";
            groupBox14.Padding = new Padding(5, 4, 5, 4);
            groupBox14.Size = new Size(174, 73);
            groupBox14.TabIndex = 4;
            groupBox14.TabStop = false;
            groupBox14.Text = "Reg. 1-2 Line Spacing";
            // 
            // ShowLineSpacing2MaxUpDown
            // 
            ShowLineSpacing2MaxUpDown.DecimalPlaces = 2;
            ShowLineSpacing2MaxUpDown.Increment = new decimal(new int[] { 5, 0, 0, 131072 });
            ShowLineSpacing2MaxUpDown.Location = new Point(95, 29);
            ShowLineSpacing2MaxUpDown.Margin = new Padding(5, 4, 5, 4);
            ShowLineSpacing2MaxUpDown.Maximum = new decimal(new int[] { 2, 0, 0, 0 });
            ShowLineSpacing2MaxUpDown.Minimum = new decimal(new int[] { 5, 0, 0, 65536 });
            ShowLineSpacing2MaxUpDown.Name = "ShowLineSpacing2MaxUpDown";
            ShowLineSpacing2MaxUpDown.Size = new Size(72, 27);
            ShowLineSpacing2MaxUpDown.TabIndex = 1;
            ShowLineSpacing2MaxUpDown.Value = new decimal(new int[] { 5, 0, 0, 65536 });
            ShowLineSpacing2MaxUpDown.ValueChanged += ShowLineSpacing2MaxUpDown_ValueChanged;
            // 
            // ShowLineSpacingMaxUpDown
            // 
            ShowLineSpacingMaxUpDown.DecimalPlaces = 2;
            ShowLineSpacingMaxUpDown.Increment = new decimal(new int[] { 5, 0, 0, 131072 });
            ShowLineSpacingMaxUpDown.Location = new Point(11, 29);
            ShowLineSpacingMaxUpDown.Margin = new Padding(5, 4, 5, 4);
            ShowLineSpacingMaxUpDown.Maximum = new decimal(new int[] { 2, 0, 0, 0 });
            ShowLineSpacingMaxUpDown.Minimum = new decimal(new int[] { 5, 0, 0, 65536 });
            ShowLineSpacingMaxUpDown.Name = "ShowLineSpacingMaxUpDown";
            ShowLineSpacingMaxUpDown.Size = new Size(72, 27);
            ShowLineSpacingMaxUpDown.TabIndex = 0;
            ShowLineSpacingMaxUpDown.Value = new decimal(new int[] { 5, 0, 0, 65536 });
            ShowLineSpacingMaxUpDown.ValueChanged += ShowLineSpacingMaxUpDown_ValueChanged;
            // 
            // groupBox13
            // 
            groupBox13.Controls.Add(panel18);
            groupBox13.Controls.Add(panel16);
            groupBox13.Controls.Add(panel15);
            groupBox13.Controls.Add(label47);
            groupBox13.Controls.Add(panel14);
            groupBox13.Controls.Add(label13);
            groupBox13.Controls.Add(label12);
            groupBox13.Controls.Add(label11);
            groupBox13.Location = new Point(519, 29);
            groupBox13.Margin = new Padding(5, 4, 5, 4);
            groupBox13.Name = "groupBox13";
            groupBox13.Padding = new Padding(5, 4, 5, 4);
            groupBox13.Size = new Size(174, 179);
            groupBox13.TabIndex = 3;
            groupBox13.TabStop = false;
            groupBox13.Text = "Headings Text";
            // 
            // panel18
            // 
            panel18.Controls.Add(toolStrip3);
            panel18.Location = new Point(67, 136);
            panel18.Margin = new Padding(5, 4, 5, 4);
            panel18.Name = "panel18";
            panel18.Size = new Size(102, 33);
            panel18.TabIndex = 23;
            // 
            // toolStrip3
            // 
            toolStrip3.AutoSize = false;
            toolStrip3.CanOverflow = false;
            toolStrip3.Dock = DockStyle.None;
            toolStrip3.GripStyle = ToolStripGripStyle.Hidden;
            toolStrip3.ImageScalingSize = new Size(24, 24);
            toolStrip3.Items.AddRange(new ToolStripItem[] { tbLyricsHeading3 });
            toolStrip3.LayoutStyle = ToolStripLayoutStyle.HorizontalStackWithOverflow;
            toolStrip3.Location = new Point(0, -1);
            toolStrip3.Name = "toolStrip3";
            toolStrip3.Padding = new Padding(0, 0, 2, 0);
            toolStrip3.RenderMode = ToolStripRenderMode.System;
            toolStrip3.Size = new Size(98, 39);
            toolStrip3.TabIndex = 5;
            // 
            // tbLyricsHeading3
            // 
            tbLyricsHeading3.AutoSize = false;
            tbLyricsHeading3.MaxDropDownItems = 12;
            tbLyricsHeading3.Name = "tbLyricsHeading3";
            tbLyricsHeading3.Size = new Size(92, 28);
            tbLyricsHeading3.TextChanged += tbLyricsHeading_TextChanged;
            // 
            // panel16
            // 
            panel16.Controls.Add(toolStrip8);
            panel16.Location = new Point(67, 100);
            panel16.Margin = new Padding(5, 4, 5, 4);
            panel16.Name = "panel16";
            panel16.Size = new Size(102, 33);
            panel16.TabIndex = 21;
            // 
            // toolStrip8
            // 
            toolStrip8.AutoSize = false;
            toolStrip8.CanOverflow = false;
            toolStrip8.Dock = DockStyle.None;
            toolStrip8.GripStyle = ToolStripGripStyle.Hidden;
            toolStrip8.ImageScalingSize = new Size(24, 24);
            toolStrip8.Items.AddRange(new ToolStripItem[] { tbLyricsHeading2 });
            toolStrip8.LayoutStyle = ToolStripLayoutStyle.HorizontalStackWithOverflow;
            toolStrip8.Location = new Point(0, -1);
            toolStrip8.Name = "toolStrip8";
            toolStrip8.Padding = new Padding(0, 0, 2, 0);
            toolStrip8.RenderMode = ToolStripRenderMode.System;
            toolStrip8.Size = new Size(98, 39);
            toolStrip8.TabIndex = 5;
            // 
            // tbLyricsHeading2
            // 
            tbLyricsHeading2.AutoSize = false;
            tbLyricsHeading2.MaxDropDownItems = 12;
            tbLyricsHeading2.Name = "tbLyricsHeading2";
            tbLyricsHeading2.Size = new Size(92, 28);
            tbLyricsHeading2.TextChanged += tbLyricsHeading_TextChanged;
            // 
            // panel15
            // 
            panel15.Controls.Add(toolStrip7);
            panel15.Location = new Point(67, 64);
            panel15.Margin = new Padding(5, 4, 5, 4);
            panel15.Name = "panel15";
            panel15.Size = new Size(102, 33);
            panel15.TabIndex = 20;
            // 
            // toolStrip7
            // 
            toolStrip7.AutoSize = false;
            toolStrip7.CanOverflow = false;
            toolStrip7.Dock = DockStyle.None;
            toolStrip7.GripStyle = ToolStripGripStyle.Hidden;
            toolStrip7.ImageScalingSize = new Size(24, 24);
            toolStrip7.Items.AddRange(new ToolStripItem[] { tbLyricsHeading1 });
            toolStrip7.LayoutStyle = ToolStripLayoutStyle.HorizontalStackWithOverflow;
            toolStrip7.Location = new Point(0, -1);
            toolStrip7.Name = "toolStrip7";
            toolStrip7.Padding = new Padding(0, 0, 2, 0);
            toolStrip7.RenderMode = ToolStripRenderMode.System;
            toolStrip7.Size = new Size(98, 39);
            toolStrip7.TabIndex = 5;
            // 
            // tbLyricsHeading1
            // 
            tbLyricsHeading1.AutoSize = false;
            tbLyricsHeading1.MaxDropDownItems = 12;
            tbLyricsHeading1.Name = "tbLyricsHeading1";
            tbLyricsHeading1.Size = new Size(92, 28);
            tbLyricsHeading1.TextChanged += tbLyricsHeading_TextChanged;
            // 
            // label47
            // 
            label47.AutoSize = true;
            label47.Location = new Point(8, 72);
            label47.Margin = new Padding(5, 0, 5, 0);
            label47.Name = "label47";
            label47.Size = new Size(57, 20);
            label47.TabIndex = 1;
            label47.Text = "Chorus:";
            // 
            // panel14
            // 
            panel14.Controls.Add(toolStrip6);
            panel14.Location = new Point(67, 29);
            panel14.Margin = new Padding(5, 4, 5, 4);
            panel14.Name = "panel14";
            panel14.Size = new Size(102, 33);
            panel14.TabIndex = 19;
            // 
            // toolStrip6
            // 
            toolStrip6.AutoSize = false;
            toolStrip6.CanOverflow = false;
            toolStrip6.Dock = DockStyle.None;
            toolStrip6.GripStyle = ToolStripGripStyle.Hidden;
            toolStrip6.ImageScalingSize = new Size(24, 24);
            toolStrip6.Items.AddRange(new ToolStripItem[] { tbLyricsHeading0 });
            toolStrip6.LayoutStyle = ToolStripLayoutStyle.HorizontalStackWithOverflow;
            toolStrip6.Location = new Point(0, -1);
            toolStrip6.Name = "toolStrip6";
            toolStrip6.Padding = new Padding(0, 0, 2, 0);
            toolStrip6.RenderMode = ToolStripRenderMode.System;
            toolStrip6.Size = new Size(98, 39);
            toolStrip6.TabIndex = 5;
            // 
            // tbLyricsHeading0
            // 
            tbLyricsHeading0.AutoSize = false;
            tbLyricsHeading0.MaxDropDownItems = 12;
            tbLyricsHeading0.Name = "tbLyricsHeading0";
            tbLyricsHeading0.Size = new Size(92, 28);
            tbLyricsHeading0.TextChanged += tbLyricsHeading_TextChanged;
            // 
            // label13
            // 
            label13.AutoSize = true;
            label13.Location = new Point(8, 141);
            label13.Margin = new Padding(5, 0, 5, 0);
            label13.Name = "label13";
            label13.Size = new Size(58, 20);
            label13.TabIndex = 3;
            label13.Text = "Ending:";
            // 
            // label12
            // 
            label12.AutoSize = true;
            label12.Location = new Point(8, 107);
            label12.Margin = new Padding(5, 0, 5, 0);
            label12.Name = "label12";
            label12.Size = new Size(56, 20);
            label12.TabIndex = 2;
            label12.Text = "Bridge:";
            // 
            // label11
            // 
            label11.AutoSize = true;
            label11.Location = new Point(8, 36);
            label11.Margin = new Padding(5, 0, 5, 0);
            label11.Name = "label11";
            label11.Size = new Size(55, 20);
            label11.TabIndex = 0;
            label11.Text = "PreChr:";
            // 
            // GroupBoxFont1
            // 
            GroupBoxFont1.Controls.Add(panel8);
            GroupBoxFont1.Controls.Add(panel9);
            GroupBoxFont1.Location = new Point(287, 163);
            GroupBoxFont1.Margin = new Padding(5, 4, 5, 4);
            GroupBoxFont1.Name = "GroupBoxFont1";
            GroupBoxFont1.Padding = new Padding(5, 4, 5, 4);
            GroupBoxFont1.Size = new Size(224, 124);
            GroupBoxFont1.TabIndex = 2;
            GroupBoxFont1.TabStop = false;
            GroupBoxFont1.Text = "Region 2 Font";
            // 
            // panel8
            // 
            panel8.Controls.Add(toolStrip4);
            panel8.Location = new Point(11, 29);
            panel8.Margin = new Padding(5, 4, 5, 4);
            panel8.Name = "panel8";
            panel8.Size = new Size(207, 33);
            panel8.TabIndex = 15;
            // 
            // toolStrip4
            // 
            toolStrip4.AutoSize = false;
            toolStrip4.CanOverflow = false;
            toolStrip4.Dock = DockStyle.None;
            toolStrip4.GripStyle = ToolStripGripStyle.Hidden;
            toolStrip4.ImageScalingSize = new Size(24, 24);
            toolStrip4.Items.AddRange(new ToolStripItem[] { ComboFontName1 });
            toolStrip4.LayoutStyle = ToolStripLayoutStyle.HorizontalStackWithOverflow;
            toolStrip4.Location = new Point(0, -1);
            toolStrip4.Name = "toolStrip4";
            toolStrip4.Padding = new Padding(0, 0, 2, 0);
            toolStrip4.RenderMode = ToolStripRenderMode.System;
            toolStrip4.Size = new Size(202, 39);
            toolStrip4.TabIndex = 0;
            // 
            // ComboFontName1
            // 
            ComboFontName1.AutoSize = false;
            ComboFontName1.DropDownStyle = ComboBoxStyle.DropDownList;
            ComboFontName1.MaxDropDownItems = 12;
            ComboFontName1.Name = "ComboFontName1";
            ComboFontName1.Size = new Size(198, 28);
            ComboFontName1.SelectedIndexChanged += ComboFontName1_SelectedIndexChanged;
            // 
            // panel9
            // 
            panel9.Controls.Add(FontSizeUpDown1);
            panel9.Controls.Add(ToolBarFontBtn1);
            panel9.Location = new Point(11, 72);
            panel9.Margin = new Padding(5, 4, 5, 4);
            panel9.Name = "panel9";
            panel9.Size = new Size(207, 33);
            panel9.TabIndex = 14;
            // 
            // FontSizeUpDown1
            // 
            FontSizeUpDown1.Location = new Point(145, 0);
            FontSizeUpDown1.Margin = new Padding(5, 4, 5, 4);
            FontSizeUpDown1.Minimum = new decimal(new int[] { 6, 0, 0, 0 });
            FontSizeUpDown1.Name = "FontSizeUpDown1";
            FontSizeUpDown1.Size = new Size(56, 27);
            FontSizeUpDown1.TabIndex = 2;
            FontSizeUpDown1.Value = new decimal(new int[] { 6, 0, 0, 0 });
            FontSizeUpDown1.ValueChanged += FontSizeUpDown1_ValueChanged;
            // 
            // ToolBarFontBtn1
            // 
            ToolBarFontBtn1.AutoSize = false;
            ToolBarFontBtn1.CanOverflow = false;
            ToolBarFontBtn1.Dock = DockStyle.None;
            ToolBarFontBtn1.GripStyle = ToolStripGripStyle.Hidden;
            ToolBarFontBtn1.ImageScalingSize = new Size(24, 24);
            ToolBarFontBtn1.Items.AddRange(new ToolStripItem[] { ToolBarFont_R2Bold, ToolBarFont_R2Italics, ToolBarFont_R2Underline, ToolBarFont_R2RTL });
            ToolBarFontBtn1.LayoutStyle = ToolStripLayoutStyle.HorizontalStackWithOverflow;
            ToolBarFontBtn1.Location = new Point(0, -1);
            ToolBarFontBtn1.Name = "ToolBarFontBtn1";
            ToolBarFontBtn1.Padding = new Padding(0, 0, 2, 0);
            ToolBarFontBtn1.RenderMode = ToolStripRenderMode.System;
            ToolBarFontBtn1.Size = new Size(142, 39);
            ToolBarFontBtn1.TabIndex = 0;
            // 
            // ToolBarFont_R2Bold
            // 
            ToolBarFont_R2Bold.CheckOnClick = true;
            ToolBarFont_R2Bold.DisplayStyle = ToolStripItemDisplayStyle.Image;
            ToolBarFont_R2Bold.Image = (Image)resources.GetObject("ToolBarFont_R2Bold.Image");
            ToolBarFont_R2Bold.ImageTransparentColor = Color.Magenta;
            ToolBarFont_R2Bold.Name = "ToolBarFont_R2Bold";
            ToolBarFont_R2Bold.Size = new Size(29, 36);
            ToolBarFont_R2Bold.Click += ToolBarFont_R2_Click;
            // 
            // ToolBarFont_R2Italics
            // 
            ToolBarFont_R2Italics.DisplayStyle = ToolStripItemDisplayStyle.Image;
            ToolBarFont_R2Italics.DropDownItems.AddRange(new ToolStripItem[] { ToolBarFont_R2Italics0, ToolBarFont_R2Italics1, ToolBarFont_R2Italics2 });
            ToolBarFont_R2Italics.Image = (Image)resources.GetObject("ToolBarFont_R2Italics.Image");
            ToolBarFont_R2Italics.ImageTransparentColor = Color.Magenta;
            ToolBarFont_R2Italics.Name = "ToolBarFont_R2Italics";
            ToolBarFont_R2Italics.Size = new Size(38, 36);
            ToolBarFont_R2Italics.DropDownItemClicked += ToolBarFont_R2Italics_DropDownItemClicked;
            // 
            // ToolBarFont_R2Italics0
            // 
            ToolBarFont_R2Italics0.Image = (Image)resources.GetObject("ToolBarFont_R2Italics0.Image");
            ToolBarFont_R2Italics0.Name = "ToolBarFont_R2Italics0";
            ToolBarFont_R2Italics0.Size = new Size(213, 26);
            ToolBarFont_R2Italics0.Tag = "0";
            ToolBarFont_R2Italics0.Text = "No Italics";
            // 
            // ToolBarFont_R2Italics1
            // 
            ToolBarFont_R2Italics1.Image = (Image)resources.GetObject("ToolBarFont_R2Italics1.Image");
            ToolBarFont_R2Italics1.Name = "ToolBarFont_R2Italics1";
            ToolBarFont_R2Italics1.Size = new Size(213, 26);
            ToolBarFont_R2Italics1.Tag = "1";
            ToolBarFont_R2Italics1.Text = "Italics";
            // 
            // ToolBarFont_R2Italics2
            // 
            ToolBarFont_R2Italics2.Image = (Image)resources.GetObject("ToolBarFont_R2Italics2.Image");
            ToolBarFont_R2Italics2.Name = "ToolBarFont_R2Italics2";
            ToolBarFont_R2Italics2.Size = new Size(213, 26);
            ToolBarFont_R2Italics2.Tag = "2";
            ToolBarFont_R2Italics2.Text = "Chorus Italics Only";
            // 
            // ToolBarFont_R2Underline
            // 
            ToolBarFont_R2Underline.CheckOnClick = true;
            ToolBarFont_R2Underline.DisplayStyle = ToolStripItemDisplayStyle.Image;
            ToolBarFont_R2Underline.Image = (Image)resources.GetObject("ToolBarFont_R2Underline.Image");
            ToolBarFont_R2Underline.ImageTransparentColor = Color.Magenta;
            ToolBarFont_R2Underline.Name = "ToolBarFont_R2Underline";
            ToolBarFont_R2Underline.Size = new Size(29, 36);
            ToolBarFont_R2Underline.Click += ToolBarFont_R2_Click;
            // 
            // ToolBarFont_R2RTL
            // 
            ToolBarFont_R2RTL.CheckOnClick = true;
            ToolBarFont_R2RTL.DisplayStyle = ToolStripItemDisplayStyle.Image;
            ToolBarFont_R2RTL.Image = (Image)resources.GetObject("ToolBarFont_R2RTL.Image");
            ToolBarFont_R2RTL.ImageTransparentColor = Color.Magenta;
            ToolBarFont_R2RTL.Name = "ToolBarFont_R2RTL";
            ToolBarFont_R2RTL.Size = new Size(29, 36);
            ToolBarFont_R2RTL.ToolTipText = "Right-To-Left";
            ToolBarFont_R2RTL.Click += ToolBarFont_R2_Click;
            // 
            // GroupBoxFont0
            // 
            GroupBoxFont0.Controls.Add(panelInd5);
            GroupBoxFont0.Controls.Add(panel7);
            GroupBoxFont0.Location = new Point(287, 29);
            GroupBoxFont0.Margin = new Padding(5, 4, 5, 4);
            GroupBoxFont0.Name = "GroupBoxFont0";
            GroupBoxFont0.Padding = new Padding(5, 4, 5, 4);
            GroupBoxFont0.Size = new Size(224, 124);
            GroupBoxFont0.TabIndex = 1;
            GroupBoxFont0.TabStop = false;
            GroupBoxFont0.Text = "Region 1 Font";
            // 
            // panelInd5
            // 
            panelInd5.Controls.Add(toolStripInd5);
            panelInd5.Location = new Point(11, 29);
            panelInd5.Margin = new Padding(5, 4, 5, 4);
            panelInd5.Name = "panelInd5";
            panelInd5.Size = new Size(207, 33);
            panelInd5.TabIndex = 13;
            // 
            // toolStripInd5
            // 
            toolStripInd5.AutoSize = false;
            toolStripInd5.CanOverflow = false;
            toolStripInd5.Dock = DockStyle.None;
            toolStripInd5.GripStyle = ToolStripGripStyle.Hidden;
            toolStripInd5.ImageScalingSize = new Size(24, 24);
            toolStripInd5.Items.AddRange(new ToolStripItem[] { ComboFontName0 });
            toolStripInd5.LayoutStyle = ToolStripLayoutStyle.HorizontalStackWithOverflow;
            toolStripInd5.Location = new Point(0, -1);
            toolStripInd5.Name = "toolStripInd5";
            toolStripInd5.Padding = new Padding(0, 0, 2, 0);
            toolStripInd5.RenderMode = ToolStripRenderMode.System;
            toolStripInd5.Size = new Size(202, 39);
            toolStripInd5.TabIndex = 5;
            // 
            // ComboFontName0
            // 
            ComboFontName0.AutoSize = false;
            ComboFontName0.DropDownStyle = ComboBoxStyle.DropDownList;
            ComboFontName0.MaxDropDownItems = 12;
            ComboFontName0.Name = "ComboFontName0";
            ComboFontName0.Size = new Size(198, 28);
            ComboFontName0.SelectedIndexChanged += ComboFontName0_SelectedIndexChanged;
            // 
            // panel7
            // 
            panel7.Controls.Add(FontSizeUpDown0);
            panel7.Controls.Add(ToolBarFontBtn0);
            panel7.Location = new Point(11, 72);
            panel7.Margin = new Padding(5, 4, 5, 4);
            panel7.Name = "panel7";
            panel7.Size = new Size(207, 33);
            panel7.TabIndex = 12;
            // 
            // FontSizeUpDown0
            // 
            FontSizeUpDown0.Location = new Point(145, 0);
            FontSizeUpDown0.Margin = new Padding(5, 4, 5, 4);
            FontSizeUpDown0.Minimum = new decimal(new int[] { 6, 0, 0, 0 });
            FontSizeUpDown0.Name = "FontSizeUpDown0";
            FontSizeUpDown0.Size = new Size(56, 27);
            FontSizeUpDown0.TabIndex = 2;
            FontSizeUpDown0.Value = new decimal(new int[] { 6, 0, 0, 0 });
            FontSizeUpDown0.ValueChanged += FontSizeUpDown0_ValueChanged;
            // 
            // ToolBarFontBtn0
            // 
            ToolBarFontBtn0.AutoSize = false;
            ToolBarFontBtn0.CanOverflow = false;
            ToolBarFontBtn0.Dock = DockStyle.None;
            ToolBarFontBtn0.GripStyle = ToolStripGripStyle.Hidden;
            ToolBarFontBtn0.ImageScalingSize = new Size(24, 24);
            ToolBarFontBtn0.Items.AddRange(new ToolStripItem[] { ToolBarFont_R1Bold, ToolBarFont_R1Italics, ToolBarFont_R1Underline, ToolBarFont_R1RTL });
            ToolBarFontBtn0.LayoutStyle = ToolStripLayoutStyle.HorizontalStackWithOverflow;
            ToolBarFontBtn0.Location = new Point(0, -1);
            ToolBarFontBtn0.Name = "ToolBarFontBtn0";
            ToolBarFontBtn0.Padding = new Padding(0, 0, 2, 0);
            ToolBarFontBtn0.RenderMode = ToolStripRenderMode.System;
            ToolBarFontBtn0.Size = new Size(142, 39);
            ToolBarFontBtn0.TabIndex = 0;
            // 
            // ToolBarFont_R1Bold
            // 
            ToolBarFont_R1Bold.CheckOnClick = true;
            ToolBarFont_R1Bold.DisplayStyle = ToolStripItemDisplayStyle.Image;
            ToolBarFont_R1Bold.Image = (Image)resources.GetObject("ToolBarFont_R1Bold.Image");
            ToolBarFont_R1Bold.ImageTransparentColor = Color.Magenta;
            ToolBarFont_R1Bold.Name = "ToolBarFont_R1Bold";
            ToolBarFont_R1Bold.Size = new Size(29, 36);
            ToolBarFont_R1Bold.Click += ToolBarFont_R1_Click;
            // 
            // ToolBarFont_R1Italics
            // 
            ToolBarFont_R1Italics.DisplayStyle = ToolStripItemDisplayStyle.Image;
            ToolBarFont_R1Italics.DropDownItems.AddRange(new ToolStripItem[] { ToolBarFont_R1Italics0, ToolBarFont_R1Italics1, ToolBarFont_R1Italics2 });
            ToolBarFont_R1Italics.Image = (Image)resources.GetObject("ToolBarFont_R1Italics.Image");
            ToolBarFont_R1Italics.ImageTransparentColor = Color.Magenta;
            ToolBarFont_R1Italics.Name = "ToolBarFont_R1Italics";
            ToolBarFont_R1Italics.Size = new Size(38, 36);
            ToolBarFont_R1Italics.Tag = "0";
            ToolBarFont_R1Italics.DropDownItemClicked += ToolBarFont_R1Italics_DropDownItemClicked;
            // 
            // ToolBarFont_R1Italics0
            // 
            ToolBarFont_R1Italics0.Image = (Image)resources.GetObject("ToolBarFont_R1Italics0.Image");
            ToolBarFont_R1Italics0.Name = "ToolBarFont_R1Italics0";
            ToolBarFont_R1Italics0.Size = new Size(213, 26);
            ToolBarFont_R1Italics0.Tag = "0";
            ToolBarFont_R1Italics0.Text = "No Italics";
            // 
            // ToolBarFont_R1Italics1
            // 
            ToolBarFont_R1Italics1.Image = (Image)resources.GetObject("ToolBarFont_R1Italics1.Image");
            ToolBarFont_R1Italics1.Name = "ToolBarFont_R1Italics1";
            ToolBarFont_R1Italics1.Size = new Size(213, 26);
            ToolBarFont_R1Italics1.Tag = "1";
            ToolBarFont_R1Italics1.Text = "Italics";
            // 
            // ToolBarFont_R1Italics2
            // 
            ToolBarFont_R1Italics2.Image = (Image)resources.GetObject("ToolBarFont_R1Italics2.Image");
            ToolBarFont_R1Italics2.Name = "ToolBarFont_R1Italics2";
            ToolBarFont_R1Italics2.Size = new Size(213, 26);
            ToolBarFont_R1Italics2.Tag = "2";
            ToolBarFont_R1Italics2.Text = "Chorus Italics Only";
            // 
            // ToolBarFont_R1Underline
            // 
            ToolBarFont_R1Underline.CheckOnClick = true;
            ToolBarFont_R1Underline.DisplayStyle = ToolStripItemDisplayStyle.Image;
            ToolBarFont_R1Underline.Image = (Image)resources.GetObject("ToolBarFont_R1Underline.Image");
            ToolBarFont_R1Underline.ImageTransparentColor = Color.Magenta;
            ToolBarFont_R1Underline.Name = "ToolBarFont_R1Underline";
            ToolBarFont_R1Underline.Size = new Size(29, 36);
            ToolBarFont_R1Underline.Click += ToolBarFont_R1_Click;
            // 
            // ToolBarFont_R1RTL
            // 
            ToolBarFont_R1RTL.CheckOnClick = true;
            ToolBarFont_R1RTL.DisplayStyle = ToolStripItemDisplayStyle.Image;
            ToolBarFont_R1RTL.Image = (Image)resources.GetObject("ToolBarFont_R1RTL.Image");
            ToolBarFont_R1RTL.ImageTransparentColor = Color.Magenta;
            ToolBarFont_R1RTL.Name = "ToolBarFont_R1RTL";
            ToolBarFont_R1RTL.Size = new Size(29, 36);
            ToolBarFont_R1RTL.ToolTipText = "Right-To-Left";
            ToolBarFont_R1RTL.Click += ToolBarFont_R1_Click;
            // 
            // groupBox10
            // 
            groupBox10.Controls.Add(FontPositionUpDown0);
            groupBox10.Controls.Add(label17);
            groupBox10.Controls.Add(RightMarginUpDown);
            groupBox10.Controls.Add(LeftMarginUpDown);
            groupBox10.Controls.Add(FontPositionUpDownBottom);
            groupBox10.Controls.Add(FontPositionUpDown1);
            groupBox10.Controls.Add(Sample_PanelMain);
            groupBox10.Controls.Add(label15);
            groupBox10.Controls.Add(label16);
            groupBox10.Controls.Add(label18);
            groupBox10.Controls.Add(label19);
            groupBox10.Location = new Point(8, 29);
            groupBox10.Margin = new Padding(5, 4, 5, 4);
            groupBox10.Name = "groupBox10";
            groupBox10.Padding = new Padding(5, 4, 5, 4);
            groupBox10.Size = new Size(271, 259);
            groupBox10.TabIndex = 0;
            groupBox10.TabStop = false;
            groupBox10.Text = "Region Positions";
            // 
            // FontPositionUpDown0
            // 
            FontPositionUpDown0.BackColor = Color.SlateBlue;
            FontPositionUpDown0.ForeColor = Color.White;
            FontPositionUpDown0.Location = new Point(203, 49);
            FontPositionUpDown0.Margin = new Padding(5, 4, 5, 4);
            FontPositionUpDown0.Maximum = new decimal(new int[] { 99, 0, 0, 0 });
            FontPositionUpDown0.Name = "FontPositionUpDown0";
            FontPositionUpDown0.Size = new Size(56, 27);
            FontPositionUpDown0.TabIndex = 0;
            FontPositionUpDown0.ValueChanged += FontPositionUpDown0_ValueChanged;
            // 
            // label17
            // 
            label17.AutoSize = true;
            label17.Location = new Point(206, 29);
            label17.Margin = new Padding(5, 0, 5, 0);
            label17.Name = "label17";
            label17.Size = new Size(17, 20);
            label17.TabIndex = 26;
            label17.Text = "0";
            // 
            // RightMarginUpDown
            // 
            RightMarginUpDown.BackColor = Color.Green;
            RightMarginUpDown.ForeColor = Color.White;
            RightMarginUpDown.Location = new Point(142, 204);
            RightMarginUpDown.Margin = new Padding(5, 4, 5, 4);
            RightMarginUpDown.Maximum = new decimal(new int[] { 99, 0, 0, 0 });
            RightMarginUpDown.Name = "RightMarginUpDown";
            RightMarginUpDown.Size = new Size(56, 27);
            RightMarginUpDown.TabIndex = 4;
            RightMarginUpDown.ValueChanged += LeftRightMarginUpDown_ValueChanged;
            // 
            // LeftMarginUpDown
            // 
            LeftMarginUpDown.BackColor = Color.Green;
            LeftMarginUpDown.ForeColor = Color.White;
            LeftMarginUpDown.Location = new Point(9, 204);
            LeftMarginUpDown.Margin = new Padding(5, 4, 5, 4);
            LeftMarginUpDown.Maximum = new decimal(new int[] { 99, 0, 0, 0 });
            LeftMarginUpDown.Name = "LeftMarginUpDown";
            LeftMarginUpDown.Size = new Size(56, 27);
            LeftMarginUpDown.TabIndex = 3;
            LeftMarginUpDown.ValueChanged += LeftRightMarginUpDown_ValueChanged;
            // 
            // FontPositionUpDownBottom
            // 
            FontPositionUpDownBottom.BackColor = Color.Navy;
            FontPositionUpDownBottom.ForeColor = Color.White;
            FontPositionUpDownBottom.Location = new Point(203, 148);
            FontPositionUpDownBottom.Margin = new Padding(5, 4, 5, 4);
            FontPositionUpDownBottom.Maximum = new decimal(new int[] { 99, 0, 0, 0 });
            FontPositionUpDownBottom.Name = "FontPositionUpDownBottom";
            FontPositionUpDownBottom.Size = new Size(56, 27);
            FontPositionUpDownBottom.TabIndex = 21;
            FontPositionUpDownBottom.ValueChanged += FontPositionUpDownBottom_ValueChanged;
            // 
            // FontPositionUpDown1
            // 
            FontPositionUpDown1.BackColor = Color.DarkViolet;
            FontPositionUpDown1.ForeColor = Color.White;
            FontPositionUpDown1.Location = new Point(203, 99);
            FontPositionUpDown1.Margin = new Padding(5, 4, 5, 4);
            FontPositionUpDown1.Maximum = new decimal(new int[] { 99, 0, 0, 0 });
            FontPositionUpDown1.Name = "FontPositionUpDown1";
            FontPositionUpDown1.Size = new Size(56, 27);
            FontPositionUpDown1.TabIndex = 1;
            FontPositionUpDown1.ValueChanged += FontPositionUpDown1_ValueChanged;
            // 
            // Sample_PanelMain
            // 
            Sample_PanelMain.BackColor = Color.Navy;
            Sample_PanelMain.BorderStyle = BorderStyle.Fixed3D;
            Sample_PanelMain.Controls.Add(panel38);
            Sample_PanelMain.Controls.Add(SamplePanel_Region2);
            Sample_PanelMain.Controls.Add(panel37);
            Sample_PanelMain.Controls.Add(SamplePanel_Region1);
            Sample_PanelMain.Controls.Add(panel42);
            Sample_PanelMain.Controls.Add(SamplePanel_Top);
            Sample_PanelMain.Controls.Add(panel40);
            Sample_PanelMain.Controls.Add(panel41);
            Sample_PanelMain.Controls.Add(SamplePanel_Left);
            Sample_PanelMain.Controls.Add(SamplePanel_Right);
            Sample_PanelMain.Location = new Point(8, 33);
            Sample_PanelMain.Margin = new Padding(5, 4, 5, 4);
            Sample_PanelMain.Name = "Sample_PanelMain";
            Sample_PanelMain.Size = new Size(190, 164);
            Sample_PanelMain.TabIndex = 14;
            // 
            // panel38
            // 
            panel38.BackColor = Color.FromArgb(255, 128, 0);
            panel38.Dock = DockStyle.Top;
            panel38.Location = new Point(19, 116);
            panel38.Margin = new Padding(5, 4, 5, 4);
            panel38.Name = "panel38";
            panel38.Size = new Size(148, 4);
            panel38.TabIndex = 27;
            // 
            // SamplePanel_Region2
            // 
            SamplePanel_Region2.BackColor = Color.MediumOrchid;
            SamplePanel_Region2.Controls.Add(labelPreviewCentreBottom);
            SamplePanel_Region2.Dock = DockStyle.Top;
            SamplePanel_Region2.Location = new Point(19, 72);
            SamplePanel_Region2.Margin = new Padding(5, 4, 5, 4);
            SamplePanel_Region2.Name = "SamplePanel_Region2";
            SamplePanel_Region2.Size = new Size(148, 44);
            SamplePanel_Region2.TabIndex = 33;
            // 
            // labelPreviewCentreBottom
            // 
            labelPreviewCentreBottom.BackColor = Color.DarkViolet;
            labelPreviewCentreBottom.Dock = DockStyle.Fill;
            labelPreviewCentreBottom.ForeColor = Color.White;
            labelPreviewCentreBottom.Location = new Point(0, 0);
            labelPreviewCentreBottom.Margin = new Padding(5, 0, 5, 0);
            labelPreviewCentreBottom.Name = "labelPreviewCentreBottom";
            labelPreviewCentreBottom.Size = new Size(148, 44);
            labelPreviewCentreBottom.TabIndex = 32;
            labelPreviewCentreBottom.Text = "Region 2 Top";
            labelPreviewCentreBottom.TextAlign = ContentAlignment.TopCenter;
            // 
            // panel37
            // 
            panel37.BackColor = Color.FromArgb(255, 128, 0);
            panel37.Dock = DockStyle.Top;
            panel37.Location = new Point(19, 68);
            panel37.Margin = new Padding(5, 4, 5, 4);
            panel37.Name = "panel37";
            panel37.Size = new Size(148, 4);
            panel37.TabIndex = 30;
            // 
            // SamplePanel_Region1
            // 
            SamplePanel_Region1.BackColor = Color.FromArgb(128, 128, 255);
            SamplePanel_Region1.Controls.Add(labelPreviewCentreTop);
            SamplePanel_Region1.Dock = DockStyle.Top;
            SamplePanel_Region1.Location = new Point(19, 20);
            SamplePanel_Region1.Margin = new Padding(5, 4, 5, 4);
            SamplePanel_Region1.Name = "SamplePanel_Region1";
            SamplePanel_Region1.Size = new Size(148, 48);
            SamplePanel_Region1.TabIndex = 31;
            // 
            // labelPreviewCentreTop
            // 
            labelPreviewCentreTop.BackColor = Color.SlateBlue;
            labelPreviewCentreTop.Dock = DockStyle.Fill;
            labelPreviewCentreTop.ForeColor = Color.White;
            labelPreviewCentreTop.Location = new Point(0, 0);
            labelPreviewCentreTop.Margin = new Padding(5, 0, 5, 0);
            labelPreviewCentreTop.Name = "labelPreviewCentreTop";
            labelPreviewCentreTop.Size = new Size(148, 48);
            labelPreviewCentreTop.TabIndex = 21;
            labelPreviewCentreTop.Text = "Region 1 Top";
            labelPreviewCentreTop.TextAlign = ContentAlignment.TopCenter;
            // 
            // panel42
            // 
            panel42.BackColor = Color.FromArgb(255, 128, 0);
            panel42.Dock = DockStyle.Top;
            panel42.Location = new Point(19, 16);
            panel42.Margin = new Padding(5, 4, 5, 4);
            panel42.Name = "panel42";
            panel42.Size = new Size(148, 4);
            panel42.TabIndex = 32;
            // 
            // SamplePanel_Top
            // 
            SamplePanel_Top.Dock = DockStyle.Top;
            SamplePanel_Top.Location = new Point(19, 0);
            SamplePanel_Top.Margin = new Padding(5, 4, 5, 4);
            SamplePanel_Top.Name = "SamplePanel_Top";
            SamplePanel_Top.Size = new Size(148, 16);
            SamplePanel_Top.TabIndex = 21;
            // 
            // panel40
            // 
            panel40.BackColor = Color.Green;
            panel40.Dock = DockStyle.Left;
            panel40.Location = new Point(14, 0);
            panel40.Margin = new Padding(5, 4, 5, 4);
            panel40.Name = "panel40";
            panel40.Size = new Size(5, 160);
            panel40.TabIndex = 28;
            // 
            // panel41
            // 
            panel41.BackColor = Color.Green;
            panel41.Dock = DockStyle.Right;
            panel41.Location = new Point(167, 0);
            panel41.Margin = new Padding(5, 4, 5, 4);
            panel41.Name = "panel41";
            panel41.Size = new Size(5, 160);
            panel41.TabIndex = 29;
            // 
            // SamplePanel_Left
            // 
            SamplePanel_Left.Dock = DockStyle.Left;
            SamplePanel_Left.Location = new Point(0, 0);
            SamplePanel_Left.Margin = new Padding(5, 4, 5, 4);
            SamplePanel_Left.Name = "SamplePanel_Left";
            SamplePanel_Left.Size = new Size(14, 160);
            SamplePanel_Left.TabIndex = 23;
            // 
            // SamplePanel_Right
            // 
            SamplePanel_Right.Dock = DockStyle.Right;
            SamplePanel_Right.Location = new Point(172, 0);
            SamplePanel_Right.Margin = new Padding(5, 4, 5, 4);
            SamplePanel_Right.Name = "SamplePanel_Right";
            SamplePanel_Right.Size = new Size(14, 160);
            SamplePanel_Right.TabIndex = 24;
            // 
            // label15
            // 
            label15.AutoSize = true;
            label15.Location = new Point(64, 209);
            label15.Margin = new Padding(5, 0, 5, 0);
            label15.Name = "label15";
            label15.Size = new Size(83, 20);
            label15.TabIndex = 24;
            label15.Text = "Left - Right";
            // 
            // label16
            // 
            label16.AutoSize = true;
            label16.Font = new Font("Microsoft Sans Serif", 15.75F, FontStyle.Bold);
            label16.Location = new Point(191, 17);
            label16.Margin = new Padding(5, 0, 5, 0);
            label16.Name = "label16";
            label16.Size = new Size(24, 31);
            label16.TabIndex = 25;
            label16.Text = "-";
            // 
            // label18
            // 
            label18.AutoSize = true;
            label18.Location = new Point(206, 180);
            label18.Margin = new Padding(5, 0, 5, 0);
            label18.Name = "label18";
            label18.Size = new Size(33, 20);
            label18.TabIndex = 28;
            label18.Text = "100";
            // 
            // label19
            // 
            label19.AutoSize = true;
            label19.Font = new Font("Microsoft Sans Serif", 15.75F, FontStyle.Bold);
            label19.Location = new Point(191, 168);
            label19.Margin = new Padding(5, 0, 5, 0);
            label19.Name = "label19";
            label19.Size = new Size(24, 31);
            label19.TabIndex = 2;
            label19.Text = "-";
            // 
            // GroupBoxHeadings
            // 
            GroupBoxHeadings.Controls.Add(panel17);
            GroupBoxHeadings.Controls.Add(panelInd4);
            GroupBoxHeadings.Controls.Add(panel6);
            GroupBoxHeadings.Controls.Add(label8);
            GroupBoxHeadings.Location = new Point(383, 27);
            GroupBoxHeadings.Margin = new Padding(5, 4, 5, 4);
            GroupBoxHeadings.Name = "GroupBoxHeadings";
            GroupBoxHeadings.Padding = new Padding(5, 4, 5, 4);
            GroupBoxHeadings.Size = new Size(326, 179);
            GroupBoxHeadings.TabIndex = 1;
            GroupBoxHeadings.TabStop = false;
            GroupBoxHeadings.Text = "Headings Font for the Selected Folder";
            // 
            // panel17
            // 
            panel17.Controls.Add(toolStrip9);
            panel17.Location = new Point(9, 73);
            panel17.Margin = new Padding(5, 4, 5, 4);
            panel17.Name = "panel17";
            panel17.Size = new Size(311, 33);
            panel17.TabIndex = 20;
            // 
            // toolStrip9
            // 
            toolStrip9.AutoSize = false;
            toolStrip9.CanOverflow = false;
            toolStrip9.Dock = DockStyle.None;
            toolStrip9.GripStyle = ToolStripGripStyle.Hidden;
            toolStrip9.ImageScalingSize = new Size(24, 24);
            toolStrip9.Items.AddRange(new ToolStripItem[] { ComboLyricsHeading });
            toolStrip9.LayoutStyle = ToolStripLayoutStyle.HorizontalStackWithOverflow;
            toolStrip9.Location = new Point(0, -1);
            toolStrip9.Name = "toolStrip9";
            toolStrip9.Padding = new Padding(0, 0, 2, 0);
            toolStrip9.RenderMode = ToolStripRenderMode.System;
            toolStrip9.Size = new Size(341, 39);
            toolStrip9.TabIndex = 5;
            // 
            // ComboLyricsHeading
            // 
            ComboLyricsHeading.AutoSize = false;
            ComboLyricsHeading.DropDownStyle = ComboBoxStyle.DropDownList;
            ComboLyricsHeading.Items.AddRange(new object[] { "Use Region 1 settings only", "Use Region 1 plus formatting below", "Use only formatting below" });
            ComboLyricsHeading.MaxDropDownItems = 12;
            ComboLyricsHeading.Name = "ComboLyricsHeading";
            ComboLyricsHeading.Size = new Size(306, 28);
            ComboLyricsHeading.SelectedIndexChanged += ComboLyricsHeading_SelectedIndexChanged;
            // 
            // panelInd4
            // 
            panelInd4.Controls.Add(HeadingsFontToolbar);
            panelInd4.Location = new Point(97, 113);
            panelInd4.Margin = new Padding(5, 4, 5, 4);
            panelInd4.Name = "panelInd4";
            panelInd4.Size = new Size(121, 33);
            panelInd4.TabIndex = 11;
            // 
            // HeadingsFontToolbar
            // 
            HeadingsFontToolbar.AutoSize = false;
            HeadingsFontToolbar.CanOverflow = false;
            HeadingsFontToolbar.Dock = DockStyle.None;
            HeadingsFontToolbar.GripStyle = ToolStripGripStyle.Hidden;
            HeadingsFontToolbar.ImageScalingSize = new Size(24, 24);
            HeadingsFontToolbar.Items.AddRange(new ToolStripItem[] { HeadingsFont_Bold, HeadingsFont_Italics, HeadingsFont_Underline });
            HeadingsFontToolbar.LayoutStyle = ToolStripLayoutStyle.HorizontalStackWithOverflow;
            HeadingsFontToolbar.Location = new Point(0, -1);
            HeadingsFontToolbar.Name = "HeadingsFontToolbar";
            HeadingsFontToolbar.Padding = new Padding(0, 0, 2, 0);
            HeadingsFontToolbar.RenderMode = ToolStripRenderMode.System;
            HeadingsFontToolbar.Size = new Size(105, 39);
            HeadingsFontToolbar.TabIndex = 0;
            // 
            // HeadingsFont_Bold
            // 
            HeadingsFont_Bold.CheckOnClick = true;
            HeadingsFont_Bold.DisplayStyle = ToolStripItemDisplayStyle.Image;
            HeadingsFont_Bold.Image = (Image)resources.GetObject("HeadingsFont_Bold.Image");
            HeadingsFont_Bold.ImageTransparentColor = Color.Magenta;
            HeadingsFont_Bold.Name = "HeadingsFont_Bold";
            HeadingsFont_Bold.Size = new Size(29, 36);
            HeadingsFont_Bold.Click += HeadingsFont_Click;
            // 
            // HeadingsFont_Italics
            // 
            HeadingsFont_Italics.DisplayStyle = ToolStripItemDisplayStyle.Image;
            HeadingsFont_Italics.DropDownItems.AddRange(new ToolStripItem[] { HeadingsFont_Italics0, HeadingsFont_Italics1, HeadingsFont_Italics2 });
            HeadingsFont_Italics.Image = (Image)resources.GetObject("HeadingsFont_Italics.Image");
            HeadingsFont_Italics.ImageTransparentColor = Color.Magenta;
            HeadingsFont_Italics.Name = "HeadingsFont_Italics";
            HeadingsFont_Italics.Size = new Size(38, 36);
            HeadingsFont_Italics.Tag = "0";
            HeadingsFont_Italics.DropDownItemClicked += HeadingsFont_Italics_DropDownItemClicked;
            // 
            // HeadingsFont_Italics0
            // 
            HeadingsFont_Italics0.Image = (Image)resources.GetObject("HeadingsFont_Italics0.Image");
            HeadingsFont_Italics0.Name = "HeadingsFont_Italics0";
            HeadingsFont_Italics0.Size = new Size(213, 26);
            HeadingsFont_Italics0.Tag = "0";
            HeadingsFont_Italics0.Text = "No Italics";
            // 
            // HeadingsFont_Italics1
            // 
            HeadingsFont_Italics1.Image = (Image)resources.GetObject("HeadingsFont_Italics1.Image");
            HeadingsFont_Italics1.Name = "HeadingsFont_Italics1";
            HeadingsFont_Italics1.Size = new Size(213, 26);
            HeadingsFont_Italics1.Tag = "1";
            HeadingsFont_Italics1.Text = "Italics";
            // 
            // HeadingsFont_Italics2
            // 
            HeadingsFont_Italics2.Image = (Image)resources.GetObject("HeadingsFont_Italics2.Image");
            HeadingsFont_Italics2.Name = "HeadingsFont_Italics2";
            HeadingsFont_Italics2.Size = new Size(213, 26);
            HeadingsFont_Italics2.Tag = "2";
            HeadingsFont_Italics2.Text = "Chorus Italics Only";
            // 
            // HeadingsFont_Underline
            // 
            HeadingsFont_Underline.CheckOnClick = true;
            HeadingsFont_Underline.DisplayStyle = ToolStripItemDisplayStyle.Image;
            HeadingsFont_Underline.Image = (Image)resources.GetObject("HeadingsFont_Underline.Image");
            HeadingsFont_Underline.ImageTransparentColor = Color.Magenta;
            HeadingsFont_Underline.Name = "HeadingsFont_Underline";
            HeadingsFont_Underline.Size = new Size(29, 36);
            HeadingsFont_Underline.Click += HeadingsFont_Click;
            // 
            // panel6
            // 
            panel6.Controls.Add(ShowHeadingsPercentSizeUpDown);
            panel6.Controls.Add(label6);
            panel6.Location = new Point(9, 29);
            panel6.Margin = new Padding(5, 4, 5, 4);
            panel6.Name = "panel6";
            panel6.Size = new Size(311, 39);
            panel6.TabIndex = 2;
            // 
            // ShowHeadingsPercentSizeUpDown
            // 
            ShowHeadingsPercentSizeUpDown.Location = new Point(234, 4);
            ShowHeadingsPercentSizeUpDown.Margin = new Padding(5, 4, 5, 4);
            ShowHeadingsPercentSizeUpDown.Maximum = new decimal(new int[] { 150, 0, 0, 0 });
            ShowHeadingsPercentSizeUpDown.Name = "ShowHeadingsPercentSizeUpDown";
            ShowHeadingsPercentSizeUpDown.Size = new Size(72, 27);
            ShowHeadingsPercentSizeUpDown.TabIndex = 1;
            ShowHeadingsPercentSizeUpDown.Value = new decimal(new int[] { 10, 0, 0, 0 });
            ShowHeadingsPercentSizeUpDown.ValueChanged += ShowHeadingsPercentSizeUpDown_ValueChanged;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(5, 8);
            label6.Margin = new Padding(5, 0, 5, 0);
            label6.Name = "label6";
            label6.Size = new Size(195, 20);
            label6.TabIndex = 0;
            label6.Text = "Headings Size (% Region 1):";
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new Point(14, 119);
            label8.Margin = new Padding(5, 0, 5, 0);
            label8.Name = "label8";
            label8.Size = new Size(85, 20);
            label8.TabIndex = 0;
            label8.Text = "Formatting:";
            // 
            // GroupBoxFolder
            // 
            GroupBoxFolder.Controls.Add(SongFolder_Rename);
            GroupBoxFolder.Controls.Add(cbFolderUse);
            GroupBoxFolder.Controls.Add(SongFolder);
            GroupBoxFolder.Location = new Point(8, 27);
            GroupBoxFolder.Margin = new Padding(5, 4, 5, 4);
            GroupBoxFolder.Name = "GroupBoxFolder";
            GroupBoxFolder.Padding = new Padding(5, 4, 5, 4);
            GroupBoxFolder.Size = new Size(367, 179);
            GroupBoxFolder.TabIndex = 0;
            GroupBoxFolder.TabStop = false;
            GroupBoxFolder.Text = "Select Folder to Update";
            // 
            // SongFolder_Rename
            // 
            SongFolder_Rename.FlatStyle = FlatStyle.Flat;
            SongFolder_Rename.Location = new Point(247, 59);
            SongFolder_Rename.Margin = new Padding(5, 4, 5, 4);
            SongFolder_Rename.Name = "SongFolder_Rename";
            SongFolder_Rename.Size = new Size(104, 39);
            SongFolder_Rename.TabIndex = 2;
            SongFolder_Rename.Text = "Rename";
            SongFolder_Rename.Click += SongFolder_Rename_Click;
            // 
            // cbFolderUse
            // 
            cbFolderUse.AutoSize = true;
            cbFolderUse.Location = new Point(247, 29);
            cbFolderUse.Margin = new Padding(5, 4, 5, 4);
            cbFolderUse.Name = "cbFolderUse";
            cbFolderUse.Size = new Size(101, 24);
            cbFolderUse.TabIndex = 1;
            cbFolderUse.Text = "Use Folder";
            cbFolderUse.CheckedChanged += cbFolderUse_CheckedChanged;
            // 
            // SongFolder
            // 
            SongFolder.Columns.AddRange(new ColumnHeader[] { columnHeader3 });
            SongFolder.FullRowSelect = true;
            SongFolder.HeaderStyle = ColumnHeaderStyle.None;
            SongFolder.LabelEdit = true;
            SongFolder.Location = new Point(8, 29);
            SongFolder.Margin = new Padding(5, 4, 5, 4);
            SongFolder.MultiSelect = false;
            SongFolder.Name = "SongFolder";
            SongFolder.ShowGroups = false;
            SongFolder.ShowItemToolTips = true;
            SongFolder.Size = new Size(229, 132);
            SongFolder.SmallImageList = imageListSys;
            SongFolder.TabIndex = 0;
            SongFolder.UseCompatibleStateImageBehavior = false;
            SongFolder.View = View.Details;
            SongFolder.AfterLabelEdit += SongFolder_AfterLabelEdit;
            SongFolder.SelectedIndexChanged += SongFolder_SelectedIndexChanged;
            SongFolder.KeyUp += SongFolder_KeyUp;
            // 
            // columnHeader3
            // 
            columnHeader3.Text = "";
            columnHeader3.Width = 143;
            // 
            // imageListSys
            // 
            imageListSys.ColorDepth = ColorDepth.Depth8Bit;
            imageListSys.ImageStream = (ImageListStreamer)resources.GetObject("imageListSys.ImageStream");
            imageListSys.TransparentColor = Color.Transparent;
            imageListSys.Images.SetKeyName(0, "ES Icon 32 Blue.ico");
            imageListSys.Images.SetKeyName(1, "ES Icon 32 Blue - Highlight.ico");
            imageListSys.Images.SetKeyName(2, "PPImg.gif");
            imageListSys.Images.SetKeyName(3, "PPImg - Highlight.gif");
            imageListSys.Images.SetKeyName(4, "Bible.gif");
            imageListSys.Images.SetKeyName(5, "Bible - Hightlight.gif");
            imageListSys.Images.SetKeyName(6, "notebook.gif");
            imageListSys.Images.SetKeyName(7, "notebook-highlight.gif");
            imageListSys.Images.SetKeyName(8, "Info_Sym.gif");
            imageListSys.Images.SetKeyName(9, "Info_Sym highlight.gif");
            imageListSys.Images.SetKeyName(10, "word.gif");
            imageListSys.Images.SetKeyName(11, "word-highlight.gif");
            imageListSys.Images.SetKeyName(12, "singlescreen.gif");
            imageListSys.Images.SetKeyName(13, "dualscreens.gif");
            imageListSys.Images.SetKeyName(14, "keyboard.gif");
            imageListSys.Images.SetKeyName(15, "BlackScreen-Pressed.gif");
            imageListSys.Images.SetKeyName(16, "BlackScreen-Red.gif");
            imageListSys.Images.SetKeyName(17, "BlueScreen-Pressed.gif");
            imageListSys.Images.SetKeyName(18, "BlueScreen-Red.gif");
            imageListSys.Images.SetKeyName(19, "folder.gif");
            imageListSys.Images.SetKeyName(20, "pic-bestfit.gif");
            imageListSys.Images.SetKeyName(21, "Bible.gif");
            imageListSys.Images.SetKeyName(22, "options.gif");
            imageListSys.Images.SetKeyName(23, "Info_Sym.gif");
            imageListSys.Images.SetKeyName(24, "PPImg.gif");
            imageListSys.Images.SetKeyName(25, "Tick.gif");
            imageListSys.Images.SetKeyName(26, "NumNewScreen.gif");
            imageListSys.Images.SetKeyName(27, "ques.gif");
            // 
            // tabPageBibles
            // 
            tabPageBibles.BackColor = SystemColors.Control;
            tabPageBibles.Controls.Add(groupBox19);
            tabPageBibles.Controls.Add(groupBox17);
            tabPageBibles.Location = new Point(4, 29);
            tabPageBibles.Margin = new Padding(5, 4, 5, 4);
            tabPageBibles.Name = "tabPageBibles";
            tabPageBibles.Padding = new Padding(5, 4, 5, 4);
            tabPageBibles.Size = new Size(719, 538);
            tabPageBibles.TabIndex = 3;
            tabPageBibles.Text = "Bibles";
            // 
            // groupBox19
            // 
            groupBox19.Controls.Add(label32);
            groupBox19.Controls.Add(btnBibleAdd);
            groupBox19.Controls.Add(btnBibleSearch);
            groupBox19.Controls.Add(BibleSearchList);
            groupBox19.Location = new Point(8, 319);
            groupBox19.Margin = new Padding(5, 4, 5, 4);
            groupBox19.Name = "groupBox19";
            groupBox19.Padding = new Padding(5, 4, 5, 4);
            groupBox19.Size = new Size(699, 191);
            groupBox19.TabIndex = 1;
            groupBox19.TabStop = false;
            groupBox19.Text = "Add Holy Bibles";
            // 
            // label32
            // 
            label32.Location = new Point(450, 87);
            label32.Margin = new Padding(5, 0, 5, 0);
            label32.Name = "label32";
            label32.Size = new Size(224, 96);
            label32.TabIndex = 3;
            label32.Text = "Click Search to list Bibles in the EasiSlides Holy Bible Folder.  If Bibles are listed, select and click Add to use them in EasiSlides.";
            // 
            // btnBibleAdd
            // 
            btnBibleAdd.FlatStyle = FlatStyle.Flat;
            btnBibleAdd.Location = new Point(563, 29);
            btnBibleAdd.Margin = new Padding(5, 4, 5, 4);
            btnBibleAdd.Name = "btnBibleAdd";
            btnBibleAdd.Size = new Size(104, 39);
            btnBibleAdd.TabIndex = 2;
            btnBibleAdd.Text = "Add";
            btnBibleAdd.Click += btnBibleAdd_Click;
            // 
            // btnBibleSearch
            // 
            btnBibleSearch.FlatStyle = FlatStyle.Flat;
            btnBibleSearch.Location = new Point(450, 29);
            btnBibleSearch.Margin = new Padding(5, 4, 5, 4);
            btnBibleSearch.Name = "btnBibleSearch";
            btnBibleSearch.Size = new Size(104, 39);
            btnBibleSearch.TabIndex = 1;
            btnBibleSearch.Text = "Search";
            btnBibleSearch.Click += btnBibleSearch_Click;
            // 
            // BibleSearchList
            // 
            BibleSearchList.Columns.AddRange(new ColumnHeader[] { columnHeader12, columnHeader13, columnHeader14, columnHeader15 });
            BibleSearchList.FullRowSelect = true;
            BibleSearchList.HeaderStyle = ColumnHeaderStyle.Nonclickable;
            BibleSearchList.Location = new Point(8, 29);
            BibleSearchList.Margin = new Padding(5, 4, 5, 4);
            BibleSearchList.Name = "BibleSearchList";
            BibleSearchList.Size = new Size(429, 151);
            BibleSearchList.TabIndex = 0;
            BibleSearchList.UseCompatibleStateImageBehavior = false;
            BibleSearchList.View = View.Details;
            BibleSearchList.DoubleClick += BibleSearchList_DoubleClick;
            // 
            // columnHeader12
            // 
            columnHeader12.Text = "Name";
            columnHeader12.Width = 70;
            // 
            // columnHeader13
            // 
            columnHeader13.Text = "Description";
            columnHeader13.Width = 250;
            // 
            // columnHeader14
            // 
            columnHeader14.Text = "File Name";
            columnHeader14.Width = 250;
            // 
            // columnHeader15
            // 
            columnHeader15.Text = "";
            columnHeader15.Width = 0;
            // 
            // groupBox17
            // 
            groupBox17.Controls.Add(btnBibleRemove);
            groupBox17.Controls.Add(btnBibleNameChange);
            groupBox17.Controls.Add(panel28);
            groupBox17.Controls.Add(groupBox18);
            groupBox17.Controls.Add(BibleList);
            groupBox17.Location = new Point(5, 24);
            groupBox17.Margin = new Padding(5, 4, 5, 4);
            groupBox17.Name = "groupBox17";
            groupBox17.Padding = new Padding(5, 4, 5, 4);
            groupBox17.Size = new Size(699, 284);
            groupBox17.TabIndex = 0;
            groupBox17.TabStop = false;
            groupBox17.Text = "Current Holy Bibles in EasiSlides";
            // 
            // btnBibleRemove
            // 
            btnBibleRemove.FlatStyle = FlatStyle.Flat;
            btnBibleRemove.Location = new Point(523, 77);
            btnBibleRemove.Margin = new Padding(5, 4, 5, 4);
            btnBibleRemove.Name = "btnBibleRemove";
            btnBibleRemove.Size = new Size(104, 39);
            btnBibleRemove.TabIndex = 3;
            btnBibleRemove.Text = "Remove";
            btnBibleRemove.Click += btnBibleRemove_Click;
            // 
            // btnBibleNameChange
            // 
            btnBibleNameChange.FlatStyle = FlatStyle.Flat;
            btnBibleNameChange.Location = new Point(523, 29);
            btnBibleNameChange.Margin = new Padding(5, 4, 5, 4);
            btnBibleNameChange.Name = "btnBibleNameChange";
            btnBibleNameChange.Size = new Size(104, 39);
            btnBibleNameChange.TabIndex = 2;
            btnBibleNameChange.Text = "Rename";
            btnBibleNameChange.Click += btnBibleNameChange_Click;
            // 
            // panel28
            // 
            panel28.Controls.Add(ToolBarBibles);
            panel28.Location = new Point(450, 29);
            panel28.Margin = new Padding(5, 4, 5, 4);
            panel28.Name = "panel28";
            panel28.Size = new Size(33, 121);
            panel28.TabIndex = 20;
            // 
            // ToolBarBibles
            // 
            ToolBarBibles.AutoSize = false;
            ToolBarBibles.CanOverflow = false;
            ToolBarBibles.Dock = DockStyle.None;
            ToolBarBibles.GripStyle = ToolStripGripStyle.Hidden;
            ToolBarBibles.ImageScalingSize = new Size(24, 24);
            ToolBarBibles.Items.AddRange(new ToolStripItem[] { Bibles_Info, Bibles_Up, Bibles_Down });
            ToolBarBibles.LayoutStyle = ToolStripLayoutStyle.VerticalStackWithOverflow;
            ToolBarBibles.Location = new Point(0, 0);
            ToolBarBibles.Name = "ToolBarBibles";
            ToolBarBibles.Padding = new Padding(0, 0, 2, 0);
            ToolBarBibles.RenderMode = ToolStripRenderMode.System;
            ToolBarBibles.Size = new Size(33, 137);
            ToolBarBibles.TabIndex = 0;
            // 
            // Bibles_Info
            // 
            Bibles_Info.AutoSize = false;
            Bibles_Info.DisplayStyle = ToolStripItemDisplayStyle.Image;
            Bibles_Info.Image = (Image)resources.GetObject("Bibles_Info.Image");
            Bibles_Info.ImageTransparentColor = Color.Magenta;
            Bibles_Info.Name = "Bibles_Info";
            Bibles_Info.Size = new Size(22, 22);
            Bibles_Info.Tag = "delete";
            Bibles_Info.ToolTipText = "Delete";
            Bibles_Info.Click += Bibles_Click;
            // 
            // Bibles_Up
            // 
            Bibles_Up.AutoSize = false;
            Bibles_Up.DisplayStyle = ToolStripItemDisplayStyle.Image;
            Bibles_Up.Image = (Image)resources.GetObject("Bibles_Up.Image");
            Bibles_Up.ImageTransparentColor = Color.Magenta;
            Bibles_Up.Name = "Bibles_Up";
            Bibles_Up.Size = new Size(22, 22);
            Bibles_Up.Tag = "up";
            Bibles_Up.ToolTipText = "Move Item Up";
            Bibles_Up.Click += Bibles_Click;
            // 
            // Bibles_Down
            // 
            Bibles_Down.AutoSize = false;
            Bibles_Down.DisplayStyle = ToolStripItemDisplayStyle.Image;
            Bibles_Down.Image = (Image)resources.GetObject("Bibles_Down.Image");
            Bibles_Down.ImageTransparentColor = Color.Magenta;
            Bibles_Down.Name = "Bibles_Down";
            Bibles_Down.Size = new Size(22, 22);
            Bibles_Down.Tag = "down";
            Bibles_Down.ToolTipText = "Move Item Down";
            Bibles_Down.Click += Bibles_Click;
            // 
            // groupBox18
            // 
            groupBox18.Controls.Add(BibleFontSizeUpDown);
            groupBox18.Controls.Add(label31);
            groupBox18.Controls.Add(panel29);
            groupBox18.Location = new Point(455, 149);
            groupBox18.Margin = new Padding(5, 4, 5, 4);
            groupBox18.Name = "groupBox18";
            groupBox18.Padding = new Padding(5, 4, 5, 4);
            groupBox18.Size = new Size(238, 124);
            groupBox18.TabIndex = 4;
            groupBox18.TabStop = false;
            groupBox18.Text = "Use Song Folder Settings";
            // 
            // BibleFontSizeUpDown
            // 
            BibleFontSizeUpDown.Location = new Point(155, 72);
            BibleFontSizeUpDown.Margin = new Padding(5, 4, 5, 4);
            BibleFontSizeUpDown.Maximum = new decimal(new int[] { 200, 0, 0, 0 });
            BibleFontSizeUpDown.Minimum = new decimal(new int[] { 5, 0, 0, 0 });
            BibleFontSizeUpDown.Name = "BibleFontSizeUpDown";
            BibleFontSizeUpDown.Size = new Size(62, 27);
            BibleFontSizeUpDown.TabIndex = 0;
            BibleFontSizeUpDown.Value = new decimal(new int[] { 5, 0, 0, 0 });
            BibleFontSizeUpDown.ValueChanged += BibleFontSizeUpDown_ValueChanged;
            // 
            // label31
            // 
            label31.AutoSize = true;
            label31.Location = new Point(15, 77);
            label31.Margin = new Padding(5, 0, 5, 0);
            label31.Name = "label31";
            label31.Size = new Size(98, 20);
            label31.TabIndex = 1;
            label31.Text = "Font Size (%):";
            // 
            // panel29
            // 
            panel29.Controls.Add(toolStrip16);
            panel29.Location = new Point(16, 29);
            panel29.Margin = new Padding(5, 4, 5, 4);
            panel29.Name = "panel29";
            panel29.Size = new Size(208, 33);
            panel29.TabIndex = 21;
            // 
            // toolStrip16
            // 
            toolStrip16.AutoSize = false;
            toolStrip16.CanOverflow = false;
            toolStrip16.Dock = DockStyle.None;
            toolStrip16.GripStyle = ToolStripGripStyle.Hidden;
            toolStrip16.ImageScalingSize = new Size(24, 24);
            toolStrip16.Items.AddRange(new ToolStripItem[] { BibleAssociatedFolder });
            toolStrip16.LayoutStyle = ToolStripLayoutStyle.HorizontalStackWithOverflow;
            toolStrip16.Location = new Point(0, -1);
            toolStrip16.Name = "toolStrip16";
            toolStrip16.Padding = new Padding(0, 0, 2, 0);
            toolStrip16.RenderMode = ToolStripRenderMode.System;
            toolStrip16.Size = new Size(202, 39);
            toolStrip16.TabIndex = 0;
            // 
            // BibleAssociatedFolder
            // 
            BibleAssociatedFolder.AutoSize = false;
            BibleAssociatedFolder.DropDownStyle = ComboBoxStyle.DropDownList;
            BibleAssociatedFolder.MaxDropDownItems = 12;
            BibleAssociatedFolder.Name = "BibleAssociatedFolder";
            BibleAssociatedFolder.Size = new Size(198, 28);
            BibleAssociatedFolder.SelectedIndexChanged += BibleAssociatedFolder_SelectedIndexChanged;
            // 
            // BibleList
            // 
            BibleList.Columns.AddRange(new ColumnHeader[] { columnHeader4, columnHeader5, columnHeader6, columnHeader7, columnHeader8, columnHeader9 });
            BibleList.FullRowSelect = true;
            BibleList.HeaderStyle = ColumnHeaderStyle.Nonclickable;
            BibleList.Location = new Point(8, 29);
            BibleList.Margin = new Padding(5, 4, 5, 4);
            BibleList.Name = "BibleList";
            BibleList.ShowItemToolTips = true;
            BibleList.Size = new Size(434, 243);
            BibleList.SmallImageList = imageListSys;
            BibleList.TabIndex = 0;
            BibleList.UseCompatibleStateImageBehavior = false;
            BibleList.View = View.Details;
            BibleList.SelectedIndexChanged += BibleList_SelectedIndexChanged;
            // 
            // columnHeader4
            // 
            columnHeader4.Text = "Name";
            columnHeader4.Width = 70;
            // 
            // columnHeader5
            // 
            columnHeader5.Text = "Description";
            columnHeader5.Width = 250;
            // 
            // columnHeader6
            // 
            columnHeader6.Text = "File Name";
            columnHeader6.Width = 250;
            // 
            // columnHeader7
            // 
            columnHeader7.Text = "Copyright";
            columnHeader7.Width = 0;
            // 
            // columnHeader8
            // 
            columnHeader8.Text = "Song Folder";
            columnHeader8.Width = 0;
            // 
            // columnHeader9
            // 
            columnHeader9.Text = "Size";
            columnHeader9.Width = 0;
            // 
            // tabPageLicence
            // 
            tabPageLicence.BackColor = SystemColors.Control;
            tabPageLicence.Controls.Add(groupBox20);
            tabPageLicence.Location = new Point(4, 29);
            tabPageLicence.Margin = new Padding(5, 4, 5, 4);
            tabPageLicence.Name = "tabPageLicence";
            tabPageLicence.Padding = new Padding(5, 4, 5, 4);
            tabPageLicence.Size = new Size(719, 538);
            tabPageLicence.TabIndex = 4;
            tabPageLicence.Text = "Licence";
            // 
            // groupBox20
            // 
            groupBox20.Controls.Add(cbEnforceDisplay);
            groupBox20.Controls.Add(panel32);
            groupBox20.Controls.Add(panel31);
            groupBox20.Controls.Add(tbNumberSymbol);
            groupBox20.Controls.Add(panel30);
            groupBox20.Controls.Add(label34);
            groupBox20.Location = new Point(8, 24);
            groupBox20.Margin = new Padding(5, 4, 5, 4);
            groupBox20.Name = "groupBox20";
            groupBox20.Padding = new Padding(5, 4, 5, 4);
            groupBox20.Size = new Size(699, 497);
            groupBox20.TabIndex = 4;
            groupBox20.TabStop = false;
            groupBox20.Text = "Licence Administration Details";
            // 
            // cbEnforceDisplay
            // 
            cbEnforceDisplay.AutoSize = true;
            cbEnforceDisplay.Location = new Point(11, 441);
            cbEnforceDisplay.Margin = new Padding(5, 4, 5, 4);
            cbEnforceDisplay.Name = "cbEnforceDisplay";
            cbEnforceDisplay.Size = new Size(334, 24);
            cbEnforceDisplay.TabIndex = 5;
            cbEnforceDisplay.Text = "Enforce Display of Copyright on First Showing";
            // 
            // panel32
            // 
            panel32.Controls.Add(AdminLicPreview8);
            panel32.Controls.Add(AdminLicPreview7);
            panel32.Controls.Add(AdminLicPreview6);
            panel32.Controls.Add(AdminLicPreview5);
            panel32.Controls.Add(AdminLicPreview4);
            panel32.Controls.Add(AdminLicPreview3);
            panel32.Controls.Add(AdminLicPreview2);
            panel32.Controls.Add(AdminLicPreview1);
            panel32.Controls.Add(label36);
            panel32.Location = new Point(375, 29);
            panel32.Margin = new Padding(5, 4, 5, 4);
            panel32.Name = "panel32";
            panel32.Size = new Size(265, 360);
            panel32.TabIndex = 2;
            // 
            // AdminLicPreview8
            // 
            AdminLicPreview8.Location = new Point(6, 309);
            AdminLicPreview8.Margin = new Padding(5, 4, 5, 4);
            AdminLicPreview8.MaxLength = 10;
            AdminLicPreview8.Name = "AdminLicPreview8";
            AdminLicPreview8.ReadOnly = true;
            AdminLicPreview8.Size = new Size(252, 27);
            AdminLicPreview8.TabIndex = 8;
            // 
            // AdminLicPreview7
            // 
            AdminLicPreview7.Location = new Point(6, 272);
            AdminLicPreview7.Margin = new Padding(5, 4, 5, 4);
            AdminLicPreview7.MaxLength = 10;
            AdminLicPreview7.Name = "AdminLicPreview7";
            AdminLicPreview7.ReadOnly = true;
            AdminLicPreview7.Size = new Size(252, 27);
            AdminLicPreview7.TabIndex = 7;
            // 
            // AdminLicPreview6
            // 
            AdminLicPreview6.Location = new Point(6, 236);
            AdminLicPreview6.Margin = new Padding(5, 4, 5, 4);
            AdminLicPreview6.MaxLength = 10;
            AdminLicPreview6.Name = "AdminLicPreview6";
            AdminLicPreview6.ReadOnly = true;
            AdminLicPreview6.Size = new Size(252, 27);
            AdminLicPreview6.TabIndex = 6;
            // 
            // AdminLicPreview5
            // 
            AdminLicPreview5.Location = new Point(6, 199);
            AdminLicPreview5.Margin = new Padding(5, 4, 5, 4);
            AdminLicPreview5.MaxLength = 10;
            AdminLicPreview5.Name = "AdminLicPreview5";
            AdminLicPreview5.ReadOnly = true;
            AdminLicPreview5.Size = new Size(252, 27);
            AdminLicPreview5.TabIndex = 5;
            // 
            // AdminLicPreview4
            // 
            AdminLicPreview4.Location = new Point(5, 161);
            AdminLicPreview4.Margin = new Padding(5, 4, 5, 4);
            AdminLicPreview4.MaxLength = 10;
            AdminLicPreview4.Name = "AdminLicPreview4";
            AdminLicPreview4.ReadOnly = true;
            AdminLicPreview4.Size = new Size(252, 27);
            AdminLicPreview4.TabIndex = 4;
            // 
            // AdminLicPreview3
            // 
            AdminLicPreview3.Location = new Point(5, 124);
            AdminLicPreview3.Margin = new Padding(5, 4, 5, 4);
            AdminLicPreview3.MaxLength = 10;
            AdminLicPreview3.Name = "AdminLicPreview3";
            AdminLicPreview3.ReadOnly = true;
            AdminLicPreview3.Size = new Size(252, 27);
            AdminLicPreview3.TabIndex = 3;
            // 
            // AdminLicPreview2
            // 
            AdminLicPreview2.Location = new Point(5, 72);
            AdminLicPreview2.Margin = new Padding(5, 4, 5, 4);
            AdminLicPreview2.MaxLength = 10;
            AdminLicPreview2.Name = "AdminLicPreview2";
            AdminLicPreview2.ReadOnly = true;
            AdminLicPreview2.Size = new Size(252, 27);
            AdminLicPreview2.TabIndex = 2;
            AdminLicPreview2.Text = "Public Domain";
            // 
            // AdminLicPreview1
            // 
            AdminLicPreview1.Location = new Point(5, 36);
            AdminLicPreview1.Margin = new Padding(5, 4, 5, 4);
            AdminLicPreview1.MaxLength = 10;
            AdminLicPreview1.Name = "AdminLicPreview1";
            AdminLicPreview1.ReadOnly = true;
            AdminLicPreview1.Size = new Size(252, 27);
            AdminLicPreview1.TabIndex = 1;
            // 
            // label36
            // 
            label36.AutoSize = true;
            label36.Location = new Point(5, 11);
            label36.Margin = new Padding(5, 0, 5, 0);
            label36.Name = "label36";
            label36.Size = new Size(200, 20);
            label36.TabIndex = 0;
            label36.Text = "How it will appear on Screen";
            // 
            // panel31
            // 
            panel31.Controls.Add(AdminLicNo8);
            panel31.Controls.Add(AdminLicNo7);
            panel31.Controls.Add(AdminLicNo6);
            panel31.Controls.Add(AdminLicNo5);
            panel31.Controls.Add(AdminLicNo4);
            panel31.Controls.Add(AdminLicNo3);
            panel31.Controls.Add(AdminLicNo2);
            panel31.Controls.Add(AdminLicNo1);
            panel31.Controls.Add(label35);
            panel31.Location = new Point(161, 29);
            panel31.Margin = new Padding(5, 4, 5, 4);
            panel31.Name = "panel31";
            panel31.Size = new Size(216, 360);
            panel31.TabIndex = 1;
            // 
            // AdminLicNo8
            // 
            AdminLicNo8.Location = new Point(6, 309);
            AdminLicNo8.Margin = new Padding(5, 4, 5, 4);
            AdminLicNo8.MaxLength = 10;
            AdminLicNo8.Name = "AdminLicNo8";
            AdminLicNo8.Size = new Size(198, 27);
            AdminLicNo8.TabIndex = 8;
            AdminLicNo8.TextChanged += AdminLic_TextChanged;
            // 
            // AdminLicNo7
            // 
            AdminLicNo7.Location = new Point(6, 272);
            AdminLicNo7.Margin = new Padding(5, 4, 5, 4);
            AdminLicNo7.MaxLength = 10;
            AdminLicNo7.Name = "AdminLicNo7";
            AdminLicNo7.Size = new Size(198, 27);
            AdminLicNo7.TabIndex = 7;
            AdminLicNo7.TextChanged += AdminLic_TextChanged;
            // 
            // AdminLicNo6
            // 
            AdminLicNo6.Location = new Point(6, 236);
            AdminLicNo6.Margin = new Padding(5, 4, 5, 4);
            AdminLicNo6.MaxLength = 10;
            AdminLicNo6.Name = "AdminLicNo6";
            AdminLicNo6.Size = new Size(198, 27);
            AdminLicNo6.TabIndex = 6;
            AdminLicNo6.TextChanged += AdminLic_TextChanged;
            // 
            // AdminLicNo5
            // 
            AdminLicNo5.Location = new Point(6, 199);
            AdminLicNo5.Margin = new Padding(5, 4, 5, 4);
            AdminLicNo5.MaxLength = 10;
            AdminLicNo5.Name = "AdminLicNo5";
            AdminLicNo5.Size = new Size(198, 27);
            AdminLicNo5.TabIndex = 5;
            AdminLicNo5.TextChanged += AdminLic_TextChanged;
            // 
            // AdminLicNo4
            // 
            AdminLicNo4.Location = new Point(5, 161);
            AdminLicNo4.Margin = new Padding(5, 4, 5, 4);
            AdminLicNo4.MaxLength = 10;
            AdminLicNo4.Name = "AdminLicNo4";
            AdminLicNo4.Size = new Size(198, 27);
            AdminLicNo4.TabIndex = 4;
            AdminLicNo4.TextChanged += AdminLic_TextChanged;
            // 
            // AdminLicNo3
            // 
            AdminLicNo3.Location = new Point(5, 124);
            AdminLicNo3.Margin = new Padding(5, 4, 5, 4);
            AdminLicNo3.MaxLength = 10;
            AdminLicNo3.Name = "AdminLicNo3";
            AdminLicNo3.Size = new Size(198, 27);
            AdminLicNo3.TabIndex = 3;
            AdminLicNo3.TextChanged += AdminLic_TextChanged;
            // 
            // AdminLicNo2
            // 
            AdminLicNo2.Location = new Point(5, 72);
            AdminLicNo2.Margin = new Padding(5, 4, 5, 4);
            AdminLicNo2.MaxLength = 10;
            AdminLicNo2.Name = "AdminLicNo2";
            AdminLicNo2.ReadOnly = true;
            AdminLicNo2.Size = new Size(198, 27);
            AdminLicNo2.TabIndex = 2;
            AdminLicNo2.Text = "Blank";
            // 
            // AdminLicNo1
            // 
            AdminLicNo1.Location = new Point(5, 36);
            AdminLicNo1.Margin = new Padding(5, 4, 5, 4);
            AdminLicNo1.MaxLength = 10;
            AdminLicNo1.Name = "AdminLicNo1";
            AdminLicNo1.ReadOnly = true;
            AdminLicNo1.Size = new Size(198, 27);
            AdminLicNo1.TabIndex = 1;
            AdminLicNo1.Text = "Blank";
            // 
            // label35
            // 
            label35.AutoSize = true;
            label35.Location = new Point(5, 11);
            label35.Margin = new Padding(5, 0, 5, 0);
            label35.Name = "label35";
            label35.Size = new Size(116, 20);
            label35.TabIndex = 0;
            label35.Text = "Licence Number";
            // 
            // tbNumberSymbol
            // 
            tbNumberSymbol.Location = new Point(167, 399);
            tbNumberSymbol.Margin = new Padding(5, 4, 5, 4);
            tbNumberSymbol.MaxLength = 10;
            tbNumberSymbol.Name = "tbNumberSymbol";
            tbNumberSymbol.Size = new Size(118, 27);
            tbNumberSymbol.TabIndex = 4;
            tbNumberSymbol.TextChanged += AdminLic_TextChanged;
            // 
            // panel30
            // 
            panel30.Controls.Add(AdminLic8);
            panel30.Controls.Add(AdminLic7);
            panel30.Controls.Add(AdminLic6);
            panel30.Controls.Add(AdminLic5);
            panel30.Controls.Add(AdminLic4);
            panel30.Controls.Add(AdminLic3);
            panel30.Controls.Add(AdminLic2);
            panel30.Controls.Add(AdminLic1);
            panel30.Controls.Add(label33);
            panel30.Location = new Point(8, 29);
            panel30.Margin = new Padding(5, 4, 5, 4);
            panel30.Name = "panel30";
            panel30.Size = new Size(154, 360);
            panel30.TabIndex = 0;
            // 
            // AdminLic8
            // 
            AdminLic8.Location = new Point(6, 309);
            AdminLic8.Margin = new Padding(5, 4, 5, 4);
            AdminLic8.MaxLength = 10;
            AdminLic8.Name = "AdminLic8";
            AdminLic8.Size = new Size(139, 27);
            AdminLic8.TabIndex = 8;
            // 
            // AdminLic7
            // 
            AdminLic7.Location = new Point(6, 272);
            AdminLic7.Margin = new Padding(5, 4, 5, 4);
            AdminLic7.MaxLength = 10;
            AdminLic7.Name = "AdminLic7";
            AdminLic7.Size = new Size(139, 27);
            AdminLic7.TabIndex = 7;
            // 
            // AdminLic6
            // 
            AdminLic6.Location = new Point(6, 236);
            AdminLic6.Margin = new Padding(5, 4, 5, 4);
            AdminLic6.MaxLength = 10;
            AdminLic6.Name = "AdminLic6";
            AdminLic6.Size = new Size(139, 27);
            AdminLic6.TabIndex = 6;
            // 
            // AdminLic5
            // 
            AdminLic5.Location = new Point(6, 199);
            AdminLic5.Margin = new Padding(5, 4, 5, 4);
            AdminLic5.MaxLength = 10;
            AdminLic5.Name = "AdminLic5";
            AdminLic5.Size = new Size(139, 27);
            AdminLic5.TabIndex = 5;
            // 
            // AdminLic4
            // 
            AdminLic4.Location = new Point(5, 161);
            AdminLic4.Margin = new Padding(5, 4, 5, 4);
            AdminLic4.MaxLength = 10;
            AdminLic4.Name = "AdminLic4";
            AdminLic4.Size = new Size(139, 27);
            AdminLic4.TabIndex = 4;
            // 
            // AdminLic3
            // 
            AdminLic3.Location = new Point(5, 124);
            AdminLic3.Margin = new Padding(5, 4, 5, 4);
            AdminLic3.MaxLength = 10;
            AdminLic3.Name = "AdminLic3";
            AdminLic3.ReadOnly = true;
            AdminLic3.Size = new Size(139, 27);
            AdminLic3.TabIndex = 3;
            AdminLic3.Text = "CCLI";
            // 
            // AdminLic2
            // 
            AdminLic2.Location = new Point(5, 72);
            AdminLic2.Margin = new Padding(5, 4, 5, 4);
            AdminLic2.MaxLength = 10;
            AdminLic2.Name = "AdminLic2";
            AdminLic2.ReadOnly = true;
            AdminLic2.Size = new Size(139, 27);
            AdminLic2.TabIndex = 2;
            AdminLic2.Text = "Public Domain";
            // 
            // AdminLic1
            // 
            AdminLic1.Location = new Point(5, 36);
            AdminLic1.Margin = new Padding(5, 4, 5, 4);
            AdminLic1.MaxLength = 10;
            AdminLic1.Name = "AdminLic1";
            AdminLic1.ReadOnly = true;
            AdminLic1.Size = new Size(139, 27);
            AdminLic1.TabIndex = 1;
            AdminLic1.Text = "None";
            // 
            // label33
            // 
            label33.AutoSize = true;
            label33.Location = new Point(5, 11);
            label33.Margin = new Padding(5, 0, 5, 0);
            label33.Name = "label33";
            label33.Size = new Size(100, 20);
            label33.TabIndex = 0;
            label33.Text = "Administrator";
            // 
            // label34
            // 
            label34.AutoSize = true;
            label34.Location = new Point(8, 403);
            label34.Margin = new Padding(5, 0, 5, 0);
            label34.Name = "label34";
            label34.Size = new Size(120, 20);
            label34.TabIndex = 3;
            label34.Text = "Number Symbol:";
            // 
            // tabPageKeyboard
            // 
            tabPageKeyboard.BackColor = SystemColors.Control;
            tabPageKeyboard.Controls.Add(ChkGlobalHookF7);
            tabPageKeyboard.Controls.Add(ChkGlobalHookF8);
            tabPageKeyboard.Controls.Add(ChkGlobalHookCtrlArrow);
            tabPageKeyboard.Controls.Add(ChkGlobalHookArrow);
            tabPageKeyboard.Controls.Add(label60);
            tabPageKeyboard.Controls.Add(ChkGlobalHookF10);
            tabPageKeyboard.Controls.Add(ChkGlobalHookF9);
            tabPageKeyboard.Controls.Add(groupBox21);
            tabPageKeyboard.Controls.Add(label59);
            tabPageKeyboard.Controls.Add(label57);
            tabPageKeyboard.Location = new Point(4, 29);
            tabPageKeyboard.Margin = new Padding(5, 4, 5, 4);
            tabPageKeyboard.Name = "tabPageKeyboard";
            tabPageKeyboard.Padding = new Padding(5, 4, 5, 4);
            tabPageKeyboard.Size = new Size(719, 538);
            tabPageKeyboard.TabIndex = 5;
            tabPageKeyboard.Text = "Keyboard";
            // 
            // ChkGlobalHookF7
            // 
            ChkGlobalHookF7.AutoSize = true;
            ChkGlobalHookF7.Location = new Point(207, 463);
            ChkGlobalHookF7.Margin = new Padding(2, 3, 2, 3);
            ChkGlobalHookF7.Name = "ChkGlobalHookF7";
            ChkGlobalHookF7.Size = new Size(198, 24);
            ChkGlobalHookF7.TabIndex = 14;
            ChkGlobalHookF7.Text = "F7 (turn off Black Output)";
            ChkGlobalHookF7.UseVisualStyleBackColor = true;
            // 
            // ChkGlobalHookF8
            // 
            ChkGlobalHookF8.AutoSize = true;
            ChkGlobalHookF8.Location = new Point(207, 492);
            ChkGlobalHookF8.Margin = new Padding(2, 3, 2, 3);
            ChkGlobalHookF8.Name = "ChkGlobalHookF8";
            ChkGlobalHookF8.Size = new Size(178, 24);
            ChkGlobalHookF8.TabIndex = 13;
            ChkGlobalHookF8.Text = "F8 (no control Output)";
            ChkGlobalHookF8.UseVisualStyleBackColor = true;
            // 
            // ChkGlobalHookCtrlArrow
            // 
            ChkGlobalHookCtrlArrow.AutoSize = true;
            ChkGlobalHookCtrlArrow.Location = new Point(418, 489);
            ChkGlobalHookCtrlArrow.Margin = new Padding(2, 3, 2, 3);
            ChkGlobalHookCtrlArrow.Name = "ChkGlobalHookCtrlArrow";
            ChkGlobalHookCtrlArrow.Size = new Size(208, 24);
            ChkGlobalHookCtrlArrow.TabIndex = 12;
            ChkGlobalHookCtrlArrow.Text = "OutputView_CtrlArrowKeys";
            ChkGlobalHookCtrlArrow.UseVisualStyleBackColor = true;
            ChkGlobalHookCtrlArrow.CheckedChanged += ChkGlobalHookCtrlArrow_CheckedChanged;
            // 
            // ChkGlobalHookArrow
            // 
            ChkGlobalHookArrow.AutoSize = true;
            ChkGlobalHookArrow.Location = new Point(418, 460);
            ChkGlobalHookArrow.Margin = new Padding(2, 3, 2, 3);
            ChkGlobalHookArrow.Name = "ChkGlobalHookArrow";
            ChkGlobalHookArrow.Size = new Size(185, 24);
            ChkGlobalHookArrow.TabIndex = 10;
            ChkGlobalHookArrow.Text = "OutputView_ArrowKeys";
            ChkGlobalHookArrow.UseVisualStyleBackColor = true;
            ChkGlobalHookArrow.CheckedChanged += ChkGlobalHookArrow_CheckedChanged;
            // 
            // label60
            // 
            label60.BorderStyle = BorderStyle.FixedSingle;
            label60.FlatStyle = FlatStyle.Flat;
            label60.Location = new Point(173, 435);
            label60.Margin = new Padding(2, 0, 2, 0);
            label60.Name = "label60";
            label60.Size = new Size(235, 87);
            label60.TabIndex = 8;
            label60.Text = "CopyPreviewToOutput";
            // 
            // ChkGlobalHookF10
            // 
            ChkGlobalHookF10.AutoSize = true;
            ChkGlobalHookF10.Location = new Point(95, 476);
            ChkGlobalHookF10.Margin = new Padding(2, 3, 2, 3);
            ChkGlobalHookF10.Name = "ChkGlobalHookF10";
            ChkGlobalHookF10.Size = new Size(54, 24);
            ChkGlobalHookF10.TabIndex = 7;
            ChkGlobalHookF10.Text = "F10";
            ChkGlobalHookF10.UseVisualStyleBackColor = true;
            // 
            // ChkGlobalHookF9
            // 
            ChkGlobalHookF9.AutoSize = true;
            ChkGlobalHookF9.Location = new Point(29, 476);
            ChkGlobalHookF9.Margin = new Padding(2, 3, 2, 3);
            ChkGlobalHookF9.Name = "ChkGlobalHookF9";
            ChkGlobalHookF9.Size = new Size(46, 24);
            ChkGlobalHookF9.TabIndex = 6;
            ChkGlobalHookF9.Text = "F9";
            ChkGlobalHookF9.UseVisualStyleBackColor = true;
            // 
            // groupBox21
            // 
            groupBox21.Controls.Add(panel34);
            groupBox21.Controls.Add(panel35);
            groupBox21.Location = new Point(8, 23);
            groupBox21.Margin = new Padding(5, 4, 5, 4);
            groupBox21.Name = "groupBox21";
            groupBox21.Padding = new Padding(5, 4, 5, 4);
            groupBox21.Size = new Size(699, 401);
            groupBox21.TabIndex = 5;
            groupBox21.TabStop = false;
            groupBox21.Text = "Keyboard Mapping during Show and in Preview.Output Areas";
            // 
            // panel34
            // 
            panel34.Controls.Add(kbSelect17);
            panel34.Controls.Add(kbSelect16);
            panel34.Controls.Add(kbSelect07);
            panel34.Controls.Add(kbSelect15);
            panel34.Controls.Add(kbSelect06);
            panel34.Controls.Add(kbSelect14);
            panel34.Controls.Add(kbSelect05);
            panel34.Controls.Add(kbSelect13);
            panel34.Controls.Add(kbSelect04);
            panel34.Controls.Add(kbSelect12);
            panel34.Controls.Add(kbSelect03);
            panel34.Controls.Add(kbSelect11);
            panel34.Controls.Add(kbSelect02);
            panel34.Controls.Add(kbSelect01);
            panel34.Controls.Add(kbSelect10);
            panel34.Controls.Add(kbSelect00);
            panel34.Controls.Add(rbKeyBoardOpt1);
            panel34.Controls.Add(rbKeyBoardOpt0);
            panel34.Location = new Point(161, 29);
            panel34.Margin = new Padding(5, 4, 5, 4);
            panel34.Name = "panel34";
            panel34.Size = new Size(504, 360);
            panel34.TabIndex = 37;
            // 
            // kbSelect17
            // 
            kbSelect17.BackColor = SystemColors.Window;
            kbSelect17.Location = new Point(246, 309);
            kbSelect17.Margin = new Padding(5, 4, 5, 4);
            kbSelect17.MaxLength = 10;
            kbSelect17.Name = "kbSelect17";
            kbSelect17.ReadOnly = true;
            kbSelect17.Size = new Size(226, 27);
            kbSelect17.TabIndex = 34;
            kbSelect17.Text = "End";
            kbSelect17.TextAlign = HorizontalAlignment.Center;
            // 
            // kbSelect16
            // 
            kbSelect16.BackColor = SystemColors.Window;
            kbSelect16.Location = new Point(247, 272);
            kbSelect16.Margin = new Padding(5, 4, 5, 4);
            kbSelect16.MaxLength = 10;
            kbSelect16.Name = "kbSelect16";
            kbSelect16.ReadOnly = true;
            kbSelect16.Size = new Size(226, 27);
            kbSelect16.TabIndex = 33;
            kbSelect16.Text = "Page Down / Space Bar";
            kbSelect16.TextAlign = HorizontalAlignment.Center;
            // 
            // kbSelect07
            // 
            kbSelect07.BackColor = SystemColors.Window;
            kbSelect07.Location = new Point(6, 309);
            kbSelect07.Margin = new Padding(5, 4, 5, 4);
            kbSelect07.MaxLength = 10;
            kbSelect07.Name = "kbSelect07";
            kbSelect07.ReadOnly = true;
            kbSelect07.Size = new Size(226, 27);
            kbSelect07.TabIndex = 34;
            kbSelect07.Text = "Right Arrow";
            kbSelect07.TextAlign = HorizontalAlignment.Center;
            // 
            // kbSelect15
            // 
            kbSelect15.BackColor = SystemColors.Window;
            kbSelect15.Location = new Point(247, 236);
            kbSelect15.Margin = new Padding(5, 4, 5, 4);
            kbSelect15.MaxLength = 10;
            kbSelect15.Name = "kbSelect15";
            kbSelect15.ReadOnly = true;
            kbSelect15.Size = new Size(226, 27);
            kbSelect15.TabIndex = 32;
            kbSelect15.Text = "Page Up";
            kbSelect15.TextAlign = HorizontalAlignment.Center;
            // 
            // kbSelect06
            // 
            kbSelect06.BackColor = SystemColors.Window;
            kbSelect06.Location = new Point(6, 272);
            kbSelect06.Margin = new Padding(5, 4, 5, 4);
            kbSelect06.MaxLength = 10;
            kbSelect06.Name = "kbSelect06";
            kbSelect06.ReadOnly = true;
            kbSelect06.Size = new Size(226, 27);
            kbSelect06.TabIndex = 33;
            kbSelect06.Text = "Down Arrow / Space Bar";
            kbSelect06.TextAlign = HorizontalAlignment.Center;
            // 
            // kbSelect14
            // 
            kbSelect14.BackColor = SystemColors.Window;
            kbSelect14.Location = new Point(247, 199);
            kbSelect14.Margin = new Padding(5, 4, 5, 4);
            kbSelect14.MaxLength = 10;
            kbSelect14.Name = "kbSelect14";
            kbSelect14.ReadOnly = true;
            kbSelect14.Size = new Size(226, 27);
            kbSelect14.TabIndex = 31;
            kbSelect14.Text = "Home";
            kbSelect14.TextAlign = HorizontalAlignment.Center;
            // 
            // kbSelect05
            // 
            kbSelect05.BackColor = SystemColors.Window;
            kbSelect05.Location = new Point(6, 236);
            kbSelect05.Margin = new Padding(5, 4, 5, 4);
            kbSelect05.MaxLength = 10;
            kbSelect05.Name = "kbSelect05";
            kbSelect05.ReadOnly = true;
            kbSelect05.Size = new Size(226, 27);
            kbSelect05.TabIndex = 32;
            kbSelect05.Text = "Up Arrow";
            kbSelect05.TextAlign = HorizontalAlignment.Center;
            // 
            // kbSelect13
            // 
            kbSelect13.BackColor = SystemColors.Window;
            kbSelect13.Location = new Point(246, 147);
            kbSelect13.Margin = new Padding(5, 4, 5, 4);
            kbSelect13.MaxLength = 10;
            kbSelect13.Name = "kbSelect13";
            kbSelect13.ReadOnly = true;
            kbSelect13.Size = new Size(226, 27);
            kbSelect13.TabIndex = 30;
            kbSelect13.Text = "Right Arrow";
            kbSelect13.TextAlign = HorizontalAlignment.Center;
            // 
            // kbSelect04
            // 
            kbSelect04.BackColor = SystemColors.Window;
            kbSelect04.Location = new Point(6, 199);
            kbSelect04.Margin = new Padding(5, 4, 5, 4);
            kbSelect04.MaxLength = 10;
            kbSelect04.Name = "kbSelect04";
            kbSelect04.ReadOnly = true;
            kbSelect04.Size = new Size(226, 27);
            kbSelect04.TabIndex = 31;
            kbSelect04.Text = "Left Arrow";
            kbSelect04.TextAlign = HorizontalAlignment.Center;
            // 
            // kbSelect12
            // 
            kbSelect12.BackColor = SystemColors.Window;
            kbSelect12.Location = new Point(246, 109);
            kbSelect12.Margin = new Padding(5, 4, 5, 4);
            kbSelect12.MaxLength = 10;
            kbSelect12.Name = "kbSelect12";
            kbSelect12.ReadOnly = true;
            kbSelect12.Size = new Size(226, 27);
            kbSelect12.TabIndex = 29;
            kbSelect12.Text = "Down Arrow";
            kbSelect12.TextAlign = HorizontalAlignment.Center;
            // 
            // kbSelect03
            // 
            kbSelect03.BackColor = SystemColors.Window;
            kbSelect03.Location = new Point(5, 147);
            kbSelect03.Margin = new Padding(5, 4, 5, 4);
            kbSelect03.MaxLength = 10;
            kbSelect03.Name = "kbSelect03";
            kbSelect03.ReadOnly = true;
            kbSelect03.Size = new Size(226, 27);
            kbSelect03.TabIndex = 30;
            kbSelect03.Text = "End";
            kbSelect03.TextAlign = HorizontalAlignment.Center;
            // 
            // kbSelect11
            // 
            kbSelect11.BackColor = SystemColors.Window;
            kbSelect11.Location = new Point(246, 72);
            kbSelect11.Margin = new Padding(5, 4, 5, 4);
            kbSelect11.MaxLength = 10;
            kbSelect11.Name = "kbSelect11";
            kbSelect11.ReadOnly = true;
            kbSelect11.Size = new Size(226, 27);
            kbSelect11.TabIndex = 28;
            kbSelect11.Text = "Up Arrow";
            kbSelect11.TextAlign = HorizontalAlignment.Center;
            // 
            // kbSelect02
            // 
            kbSelect02.BackColor = SystemColors.Window;
            kbSelect02.Location = new Point(5, 109);
            kbSelect02.Margin = new Padding(5, 4, 5, 4);
            kbSelect02.MaxLength = 10;
            kbSelect02.Name = "kbSelect02";
            kbSelect02.ReadOnly = true;
            kbSelect02.Size = new Size(226, 27);
            kbSelect02.TabIndex = 29;
            kbSelect02.Text = "Page down";
            kbSelect02.TextAlign = HorizontalAlignment.Center;
            // 
            // kbSelect01
            // 
            kbSelect01.BackColor = SystemColors.Window;
            kbSelect01.Location = new Point(5, 72);
            kbSelect01.Margin = new Padding(5, 4, 5, 4);
            kbSelect01.MaxLength = 10;
            kbSelect01.Name = "kbSelect01";
            kbSelect01.ReadOnly = true;
            kbSelect01.Size = new Size(226, 27);
            kbSelect01.TabIndex = 28;
            kbSelect01.Text = "Page Up";
            kbSelect01.TextAlign = HorizontalAlignment.Center;
            // 
            // kbSelect10
            // 
            kbSelect10.BackColor = SystemColors.Window;
            kbSelect10.Location = new Point(246, 36);
            kbSelect10.Margin = new Padding(5, 4, 5, 4);
            kbSelect10.MaxLength = 10;
            kbSelect10.Name = "kbSelect10";
            kbSelect10.ReadOnly = true;
            kbSelect10.Size = new Size(226, 27);
            kbSelect10.TabIndex = 22;
            kbSelect10.Text = "Left Arrow";
            kbSelect10.TextAlign = HorizontalAlignment.Center;
            // 
            // kbSelect00
            // 
            kbSelect00.BackColor = SystemColors.Window;
            kbSelect00.Location = new Point(5, 36);
            kbSelect00.Margin = new Padding(5, 4, 5, 4);
            kbSelect00.MaxLength = 10;
            kbSelect00.Name = "kbSelect00";
            kbSelect00.ReadOnly = true;
            kbSelect00.Size = new Size(226, 27);
            kbSelect00.TabIndex = 22;
            kbSelect00.Text = "Home";
            kbSelect00.TextAlign = HorizontalAlignment.Center;
            // 
            // rbKeyBoardOpt1
            // 
            rbKeyBoardOpt1.AutoSize = true;
            rbKeyBoardOpt1.Location = new Point(304, 8);
            rbKeyBoardOpt1.Margin = new Padding(5, 4, 5, 4);
            rbKeyBoardOpt1.Name = "rbKeyBoardOpt1";
            rbKeyBoardOpt1.Size = new Size(102, 24);
            rbKeyBoardOpt1.TabIndex = 1;
            rbKeyBoardOpt1.Text = "Alternative";
            // 
            // rbKeyBoardOpt0
            // 
            rbKeyBoardOpt0.AutoSize = true;
            rbKeyBoardOpt0.Location = new Point(67, 8);
            rbKeyBoardOpt0.Margin = new Padding(5, 4, 5, 4);
            rbKeyBoardOpt0.Name = "rbKeyBoardOpt0";
            rbKeyBoardOpt0.Size = new Size(79, 24);
            rbKeyBoardOpt0.TabIndex = 0;
            rbKeyBoardOpt0.Text = "Default";
            rbKeyBoardOpt0.CheckedChanged += rbKeyBoardOpt0_CheckedChanged;
            // 
            // panel35
            // 
            panel35.Controls.Add(kbAction7);
            panel35.Controls.Add(kbAction6);
            panel35.Controls.Add(kbAction5);
            panel35.Controls.Add(kbAction4);
            panel35.Controls.Add(kbAction3);
            panel35.Controls.Add(kbAction2);
            panel35.Controls.Add(kbAction1);
            panel35.Controls.Add(kbAction0);
            panel35.Controls.Add(label39);
            panel35.Location = new Point(8, 29);
            panel35.Margin = new Padding(5, 4, 5, 4);
            panel35.Name = "panel35";
            panel35.Size = new Size(154, 360);
            panel35.TabIndex = 23;
            // 
            // kbAction7
            // 
            kbAction7.BackColor = SystemColors.Window;
            kbAction7.Location = new Point(6, 309);
            kbAction7.Margin = new Padding(5, 4, 5, 4);
            kbAction7.MaxLength = 10;
            kbAction7.Name = "kbAction7";
            kbAction7.ReadOnly = true;
            kbAction7.Size = new Size(139, 27);
            kbAction7.TabIndex = 34;
            kbAction7.Text = "Last Slide";
            // 
            // kbAction6
            // 
            kbAction6.BackColor = SystemColors.Window;
            kbAction6.Location = new Point(6, 272);
            kbAction6.Margin = new Padding(5, 4, 5, 4);
            kbAction6.MaxLength = 10;
            kbAction6.Name = "kbAction6";
            kbAction6.ReadOnly = true;
            kbAction6.Size = new Size(139, 27);
            kbAction6.TabIndex = 33;
            kbAction6.Text = "Next Slide";
            // 
            // kbAction5
            // 
            kbAction5.BackColor = SystemColors.Window;
            kbAction5.Location = new Point(6, 236);
            kbAction5.Margin = new Padding(5, 4, 5, 4);
            kbAction5.MaxLength = 10;
            kbAction5.Name = "kbAction5";
            kbAction5.ReadOnly = true;
            kbAction5.Size = new Size(139, 27);
            kbAction5.TabIndex = 32;
            kbAction5.Text = "Previous Slide";
            // 
            // kbAction4
            // 
            kbAction4.BackColor = SystemColors.Window;
            kbAction4.Location = new Point(6, 199);
            kbAction4.Margin = new Padding(5, 4, 5, 4);
            kbAction4.MaxLength = 10;
            kbAction4.Name = "kbAction4";
            kbAction4.ReadOnly = true;
            kbAction4.Size = new Size(139, 27);
            kbAction4.TabIndex = 31;
            kbAction4.Text = "First Slide";
            // 
            // kbAction3
            // 
            kbAction3.BackColor = SystemColors.Window;
            kbAction3.Location = new Point(5, 147);
            kbAction3.Margin = new Padding(5, 4, 5, 4);
            kbAction3.MaxLength = 10;
            kbAction3.Name = "kbAction3";
            kbAction3.ReadOnly = true;
            kbAction3.Size = new Size(139, 27);
            kbAction3.TabIndex = 30;
            kbAction3.Text = "Last Item";
            // 
            // kbAction2
            // 
            kbAction2.BackColor = SystemColors.Window;
            kbAction2.Location = new Point(5, 109);
            kbAction2.Margin = new Padding(5, 4, 5, 4);
            kbAction2.MaxLength = 10;
            kbAction2.Name = "kbAction2";
            kbAction2.ReadOnly = true;
            kbAction2.Size = new Size(139, 27);
            kbAction2.TabIndex = 29;
            kbAction2.Text = "Next Item";
            // 
            // kbAction1
            // 
            kbAction1.BackColor = SystemColors.Window;
            kbAction1.Location = new Point(5, 72);
            kbAction1.Margin = new Padding(5, 4, 5, 4);
            kbAction1.MaxLength = 10;
            kbAction1.Name = "kbAction1";
            kbAction1.ReadOnly = true;
            kbAction1.Size = new Size(139, 27);
            kbAction1.TabIndex = 28;
            kbAction1.Text = "Previous Item";
            // 
            // kbAction0
            // 
            kbAction0.BackColor = SystemColors.Window;
            kbAction0.Location = new Point(5, 36);
            kbAction0.Margin = new Padding(5, 4, 5, 4);
            kbAction0.MaxLength = 10;
            kbAction0.Name = "kbAction0";
            kbAction0.ReadOnly = true;
            kbAction0.Size = new Size(139, 27);
            kbAction0.TabIndex = 22;
            kbAction0.Text = "First Item";
            // 
            // label39
            // 
            label39.AutoSize = true;
            label39.Location = new Point(5, 11);
            label39.Margin = new Padding(5, 0, 5, 0);
            label39.Name = "label39";
            label39.Size = new Size(55, 20);
            label39.TabIndex = 27;
            label39.Text = "Action:";
            // 
            // label59
            // 
            label59.BorderStyle = BorderStyle.FixedSingle;
            label59.FlatStyle = FlatStyle.Flat;
            label59.Location = new Point(414, 435);
            label59.Margin = new Padding(2, 0, 2, 0);
            label59.Name = "label59";
            label59.Size = new Size(235, 87);
            label59.TabIndex = 8;
            label59.Text = "SlideUpDown";
            // 
            // label57
            // 
            label57.BorderStyle = BorderStyle.FixedSingle;
            label57.FlatStyle = FlatStyle.Flat;
            label57.Location = new Point(8, 435);
            label57.Margin = new Padding(2, 0, 2, 0);
            label57.Name = "label57";
            label57.Size = new Size(159, 87);
            label57.TabIndex = 8;
            label57.Text = "Black Screen";
            // 
            // BtnCancel
            // 
            BtnCancel.DialogResult = DialogResult.Cancel;
            BtnCancel.Location = new Point(635, 599);
            BtnCancel.Margin = new Padding(5, 4, 5, 4);
            BtnCancel.Name = "BtnCancel";
            BtnCancel.Size = new Size(106, 37);
            BtnCancel.TabIndex = 1;
            BtnCancel.Text = "Cancel";
            BtnCancel.Click += BtnCancel_Click;
            // 
            // BtnOK
            // 
            BtnOK.Location = new Point(507, 599);
            BtnOK.Margin = new Padding(5, 4, 5, 4);
            BtnOK.Name = "BtnOK";
            BtnOK.Size = new Size(106, 37);
            BtnOK.TabIndex = 0;
            BtnOK.Text = "OK";
            BtnOK.Click += BtnOK_Click;
            // 
            // folderBrowserDialog1
            // 
            folderBrowserDialog1.SelectedPath = "folderBrowserDialog1";
            // 
            // FrmOptions
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(758, 652);
            Controls.Add(BtnCancel);
            Controls.Add(BtnOK);
            Controls.Add(tabControl1);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Icon = (Icon)resources.GetObject("$this.Icon");
            Margin = new Padding(5, 4, 5, 4);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FrmOptions";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Options";
            Load += FrmOptions_Load;
            tabControl1.ResumeLayout(false);
            tabPageMainWindow.ResumeLayout(false);
            groupBox24.ResumeLayout(false);
            groupBox24.PerformLayout();
            groupBox12.ResumeLayout(false);
            panelJump.ResumeLayout(false);
            panelJump.PerformLayout();
            toolStripJump.ResumeLayout(false);
            toolStripJump.PerformLayout();
            groupBox22.ResumeLayout(false);
            groupBox22.PerformLayout();
            panel3.ResumeLayout(false);
            panel3.PerformLayout();
            ((ISupportInitialize)PPMaxUpDown).EndInit();
            groupBox7.ResumeLayout(false);
            groupBox7.PerformLayout();
            ((ISupportInitialize)PreviewFontUpDown).EndInit();
            groupBox6.ResumeLayout(false);
            groupBox6.PerformLayout();
            panel5.ResumeLayout(false);
            toolStrip1.ResumeLayout(false);
            toolStrip1.PerformLayout();
            groupBox1.ResumeLayout(false);
            panel4.ResumeLayout(false);
            panel4.PerformLayout();
            ((ISupportInitialize)EditHistoryMaxUpDown).EndInit();
            panel2.ResumeLayout(false);
            panel2.PerformLayout();
            ((ISupportInitialize)AdhocVersesMaxUpDown).EndInit();
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ((ISupportInitialize)VersesMaxUpDown).EndInit();
            tabPageShow.ResumeLayout(false);
            tabPageShow.PerformLayout();
            groupBox5.ResumeLayout(false);
            groupBox5.PerformLayout();
            groupBox3.ResumeLayout(false);
            groupBox3.PerformLayout();
            panel39.ResumeLayout(false);
            toolStrip10.ResumeLayout(false);
            toolStrip10.PerformLayout();
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            panel45.ResumeLayout(false);
            toolStripVideo.ResumeLayout(false);
            toolStripVideo.PerformLayout();
            ((ISupportInitialize)VideoSizeUpDown1).EndInit();
            panelVideoHolder.ResumeLayout(false);
            panelVideoSize.ResumeLayout(false);
            groupBox4.ResumeLayout(false);
            groupBox4.PerformLayout();
            panel46.ResumeLayout(false);
            panel46.PerformLayout();
            ((ISupportInitialize)NotationFontFactorUpDown).EndInit();
            tabPageMonitors.ResumeLayout(false);
            groupBox23.ResumeLayout(false);
            groupBox23.PerformLayout();
            panel47.ResumeLayout(false);
            toolStripCaptureDevices.ResumeLayout(false);
            toolStripCaptureDevices.PerformLayout();
            ((ISupportInitialize)TrackBarBalance).EndInit();
            ((ISupportInitialize)TrackBarVolume).EndInit();
            groupBoxLM.ResumeLayout(false);
            groupBoxLM.PerformLayout();
            ((ISupportInitialize)LM1UpDownLeft).EndInit();
            ((ISupportInitialize)LM1UpDownTop).EndInit();
            ((ISupportInitialize)LMNotationsUpDownFontSize).EndInit();
            panel48.ResumeLayout(false);
            toolStripLyricsMonitor.ResumeLayout(false);
            toolStripLyricsMonitor.PerformLayout();
            ((ISupportInitialize)LMUpDownFontSize).EndInit();
            panel43.ResumeLayout(false);
            toolStrip12.ResumeLayout(false);
            toolStrip12.PerformLayout();
            panel44.ResumeLayout(false);
            toolStrip13.ResumeLayout(false);
            toolStrip13.PerformLayout();
            ((ISupportInitialize)LM1UpDownHeight).EndInit();
            ((ISupportInitialize)LM1UpDownWidth).EndInit();
            groupBoxDM.ResumeLayout(false);
            groupBoxDM.PerformLayout();
            panel49.ResumeLayout(false);
            panel49.PerformLayout();
            ((ISupportInitialize)DM1UpDownHeight).EndInit();
            ((ISupportInitialize)DM1UpDownLeft).EndInit();
            ((ISupportInitialize)DM1UpDownWidth).EndInit();
            ((ISupportInitialize)DM1UpDownTop).EndInit();
            panelDM.ResumeLayout(false);
            toolStripMonitorList.ResumeLayout(false);
            toolStripMonitorList.PerformLayout();
            panelLinkTitle2Lookup.ResumeLayout(false);
            toolStrip2.ResumeLayout(false);
            toolStrip2.PerformLayout();
            tabPageAlerts.ResumeLayout(false);
            groupBox8.ResumeLayout(false);
            groupBox11.ResumeLayout(false);
            groupBox11.PerformLayout();
            groupBox9.ResumeLayout(false);
            groupBox9.PerformLayout();
            panel10.ResumeLayout(false);
            panel21.ResumeLayout(false);
            toolBarReferenceFormat.ResumeLayout(false);
            toolBarReferenceFormat.PerformLayout();
            panel22.ResumeLayout(false);
            panel22.PerformLayout();
            toolStrip5.ResumeLayout(false);
            toolStrip5.PerformLayout();
            panel33.ResumeLayout(false);
            panel33.PerformLayout();
            ((ISupportInitialize)ReferenceSizeUpDown).EndInit();
            panel36.ResumeLayout(false);
            panel36.PerformLayout();
            ((ISupportInitialize)ReferenceAlertDurationUpDown).EndInit();
            groupBox16.ResumeLayout(false);
            groupBox16.PerformLayout();
            panel23.ResumeLayout(false);
            panel24.ResumeLayout(false);
            ToolBarParentalFormat.ResumeLayout(false);
            ToolBarParentalFormat.PerformLayout();
            panel25.ResumeLayout(false);
            panel25.PerformLayout();
            toolStrip14.ResumeLayout(false);
            toolStrip14.PerformLayout();
            panel26.ResumeLayout(false);
            panel26.PerformLayout();
            ((ISupportInitialize)ParentalSizeUpDown).EndInit();
            panel27.ResumeLayout(false);
            panel27.PerformLayout();
            ((ISupportInitialize)ParentalAlertUpDown).EndInit();
            groupBox15.ResumeLayout(false);
            panel20.ResumeLayout(false);
            panel12.ResumeLayout(false);
            ToolBarMessageFormat.ResumeLayout(false);
            ToolBarMessageFormat.PerformLayout();
            panel19.ResumeLayout(false);
            panel19.PerformLayout();
            toolStrip11.ResumeLayout(false);
            toolStrip11.PerformLayout();
            panel13.ResumeLayout(false);
            panel13.PerformLayout();
            ((ISupportInitialize)MessageSizeUpDown).EndInit();
            panel11.ResumeLayout(false);
            panel11.PerformLayout();
            ((ISupportInitialize)MessageAlertDurationUpDown).EndInit();
            tabPageFolders.ResumeLayout(false);
            SelectedFolderGroupBox.ResumeLayout(false);
            groupBox14.ResumeLayout(false);
            ((ISupportInitialize)ShowLineSpacing2MaxUpDown).EndInit();
            ((ISupportInitialize)ShowLineSpacingMaxUpDown).EndInit();
            groupBox13.ResumeLayout(false);
            groupBox13.PerformLayout();
            panel18.ResumeLayout(false);
            toolStrip3.ResumeLayout(false);
            toolStrip3.PerformLayout();
            panel16.ResumeLayout(false);
            toolStrip8.ResumeLayout(false);
            toolStrip8.PerformLayout();
            panel15.ResumeLayout(false);
            toolStrip7.ResumeLayout(false);
            toolStrip7.PerformLayout();
            panel14.ResumeLayout(false);
            toolStrip6.ResumeLayout(false);
            toolStrip6.PerformLayout();
            GroupBoxFont1.ResumeLayout(false);
            panel8.ResumeLayout(false);
            toolStrip4.ResumeLayout(false);
            toolStrip4.PerformLayout();
            panel9.ResumeLayout(false);
            ((ISupportInitialize)FontSizeUpDown1).EndInit();
            ToolBarFontBtn1.ResumeLayout(false);
            ToolBarFontBtn1.PerformLayout();
            GroupBoxFont0.ResumeLayout(false);
            panelInd5.ResumeLayout(false);
            toolStripInd5.ResumeLayout(false);
            toolStripInd5.PerformLayout();
            panel7.ResumeLayout(false);
            ((ISupportInitialize)FontSizeUpDown0).EndInit();
            ToolBarFontBtn0.ResumeLayout(false);
            ToolBarFontBtn0.PerformLayout();
            groupBox10.ResumeLayout(false);
            groupBox10.PerformLayout();
            ((ISupportInitialize)FontPositionUpDown0).EndInit();
            ((ISupportInitialize)RightMarginUpDown).EndInit();
            ((ISupportInitialize)LeftMarginUpDown).EndInit();
            ((ISupportInitialize)FontPositionUpDownBottom).EndInit();
            ((ISupportInitialize)FontPositionUpDown1).EndInit();
            Sample_PanelMain.ResumeLayout(false);
            SamplePanel_Region2.ResumeLayout(false);
            SamplePanel_Region1.ResumeLayout(false);
            GroupBoxHeadings.ResumeLayout(false);
            GroupBoxHeadings.PerformLayout();
            panel17.ResumeLayout(false);
            toolStrip9.ResumeLayout(false);
            toolStrip9.PerformLayout();
            panelInd4.ResumeLayout(false);
            HeadingsFontToolbar.ResumeLayout(false);
            HeadingsFontToolbar.PerformLayout();
            panel6.ResumeLayout(false);
            panel6.PerformLayout();
            ((ISupportInitialize)ShowHeadingsPercentSizeUpDown).EndInit();
            GroupBoxFolder.ResumeLayout(false);
            GroupBoxFolder.PerformLayout();
            tabPageBibles.ResumeLayout(false);
            groupBox19.ResumeLayout(false);
            groupBox17.ResumeLayout(false);
            panel28.ResumeLayout(false);
            ToolBarBibles.ResumeLayout(false);
            ToolBarBibles.PerformLayout();
            groupBox18.ResumeLayout(false);
            groupBox18.PerformLayout();
            ((ISupportInitialize)BibleFontSizeUpDown).EndInit();
            panel29.ResumeLayout(false);
            toolStrip16.ResumeLayout(false);
            toolStrip16.PerformLayout();
            tabPageLicence.ResumeLayout(false);
            groupBox20.ResumeLayout(false);
            groupBox20.PerformLayout();
            panel32.ResumeLayout(false);
            panel32.PerformLayout();
            panel31.ResumeLayout(false);
            panel31.PerformLayout();
            panel30.ResumeLayout(false);
            panel30.PerformLayout();
            tabPageKeyboard.ResumeLayout(false);
            tabPageKeyboard.PerformLayout();
            groupBox21.ResumeLayout(false);
            panel34.ResumeLayout(false);
            panel34.PerformLayout();
            panel35.ResumeLayout(false);
            panel35.PerformLayout();
            ResumeLayout(false);
        }

        private void optStandard_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void optWide_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void ChkGlobalHookArrow_CheckedChanged(object sender, EventArgs e)
        {
            if(ChkGlobalHookArrow.Checked)
            {
                ChkGlobalHookCtrlArrow.Checked = false;
            }
        }

        private void ChkGlobalHookCtrlArrow_CheckedChanged(object sender, EventArgs e)
        {
            if (ChkGlobalHookCtrlArrow.Checked)
            {
                ChkGlobalHookArrow.Checked = false;
            }
        }
    }
}
