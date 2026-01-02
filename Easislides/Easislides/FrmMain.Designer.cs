using System.Windows.Forms;

namespace Easislides
{
    partial class FrmMain
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;
        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // 미디어 플레이어 후킹 정리
                RemoveHookMediaPlayer();

                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmMain));
            imageListSys = new ImageList(components);
            Ind_HeadFirstScreen = new ToolStripMenuItem();
            PreviewBtnVerse1 = new Button();
            PreviewBtnVerse2 = new Button();
            PreviewBtnVerse3 = new Button();
            flowLayoutPanel1 = new FlowLayoutPanel();
            PreviewBtnVerse4 = new Button();
            PreviewBtnVerse5 = new Button();
            PreviewBtnVerse6 = new Button();
            PreviewBtnVerse7 = new Button();
            PreviewBtnVerse8 = new Button();
            PreviewBtnVerse9 = new Button();
            PreviewBtnVersePreChorus = new Button();
            PreviewBtnVersePreChorus2 = new Button();
            PreviewBtnVerseChorus = new Button();
            PreviewBtnVerseChorus2 = new Button();
            PreviewBtnVerseBridge = new Button();
            PreviewBtnVerseBridge2 = new Button();
            PreviewBtnVerseEnding = new Button();
            panel7 = new Panel();
            panel1 = new Panel();
            panel3 = new Panel();
            PreviewBtnSlideDown = new Button();
            PreviewBtnSlideUp = new Button();
            PreviewBtnItemDown = new Button();
            PreviewBtnItemUp = new Button();
            IndradioButtonInfo = new RadioButton();
            IndradioButtonFormat = new RadioButton();
            IndradioButtonText = new RadioButton();
            IndcbPreviewNotes = new CheckBox();
            PreviewHolder = new Panel();
            PreviewBack = new Panel();
            PreviewNotes = new RichTextBox();
            panelPreviewSessionNotes2 = new Panel();
            panelPreviewBottom = new Panel();
            columnHeader15 = new ColumnHeader();
            panel9 = new Panel();
            btnToLive = new Button();
            btnToOutputMoveNext = new Button();
            PreviewPanelDisplayName = new ListView();
            btnToOutput = new Button();
            Ind_HeadAllTitles = new ToolStripMenuItem();
            Ind_Region = new ToolStripDropDownButton();
            Ind_ShowRegion1 = new ToolStripMenuItem();
            Ind_ShowRegion2 = new ToolStripMenuItem();
            Ind_ShowRegionBoth = new ToolStripMenuItem();
            Ind_VAlign = new ToolStripDropDownButton();
            Ind_VAlignTop = new ToolStripMenuItem();
            Ind_VAlignCentre = new ToolStripMenuItem();
            Ind_VAlignBottom = new ToolStripMenuItem();
            flowLayoutPreviewLyrics = new Panel();
            Ind_Outline = new ToolStripButton();
            Ind_Interlace = new ToolStripButton();
            Ind_Notations = new ToolStripButton();
            Ind_checkBox = new CheckBox();
            Ind_Shadow = new ToolStripButton();
            PreviewInfo = new RichTextBox();
            panelOutputTop = new Panel();
            flowLayoutOutputPowerPoint = new FlowLayoutPanel();
            flowLayoutOutputLyrics = new Panel();
            OutputInfo = new TextBox();
            panel10 = new Panel();
            cbOutputBlack = new CheckBox();
            cbOutputClear = new CheckBox();
            OutputPanelDisplayName = new ListView();
            columnHeader16 = new ColumnHeader();
            cbGoLive = new CheckBox();
            panelOutputBottom = new Panel();
            panelOutputLM1 = new Panel();
            OutputTextBoxLM = new TextBox();
            panelOutputLM2 = new Panel();
            panelOutputLM3 = new Panel();
            OutputBtnLMSend = new Button();
            OutputBtnLMClear = new Button();
            OutputHolder = new Panel();
            OutputBack = new Panel();
            Ind_R2Italics = new ToolStripDropDownButton();
            Ind_R2Italics0 = new ToolStripMenuItem();
            Ind_R2Italics1 = new ToolStripMenuItem();
            Ind_R2Italics2 = new ToolStripMenuItem();
            splitContainerOutput = new SplitContainer();
            panel8 = new Panel();
            labelGapItem = new Label();
            labelHideText = new Label();
            labelBlackScreen = new Label();
            panel2 = new Panel();
            flowLayoutPanel2 = new FlowLayoutPanel();
            OutputBtnVerse1 = new Button();
            OutputBtnVerse2 = new Button();
            OutputBtnVerse3 = new Button();
            OutputBtnVerse4 = new Button();
            OutputBtnVerse5 = new Button();
            OutputBtnVerse6 = new Button();
            OutputBtnVerse7 = new Button();
            OutputBtnVerse8 = new Button();
            OutputBtnVerse9 = new Button();
            OutputBtnVersePreChorus = new Button();
            OutputBtnVersePreChorus2 = new Button();
            OutputBtnVerseChorus = new Button();
            OutputBtnVerseChorus2 = new Button();
            OutputBtnVerseBridge = new Button();
            OutputBtnVerseBridge2 = new Button();
            OutputBtnVerseEnding = new Button();
            panel6 = new Panel();
            OutputBtnSlideDown = new Button();
            OutputBtnSlideUp = new Button();
            OutputBtnItemDown = new Button();
            OutputBtnItemUp = new Button();
            OutputBtnRefAlert = new Button();
            OutputBtnMedia = new Button();
            OutputBtnJumpToNonRotate = new Button();
            Ind_HeadNoTitles = new ToolStripMenuItem();
            IndgroupBox2 = new GroupBox();
            Ind_BottomUpDown = new NumericUpDown();
            panelInd3 = new Panel();
            toolStripInd3 = new ToolStrip();
            Ind_TransItem = new ToolStripComboBox();
            Ind_TransSlides = new ToolStripComboBox();
            Ind_RightUpDown = new NumericUpDown();
            panelInd2 = new Panel();
            toolStripInd2 = new ToolStrip();
            Ind_ImageMode = new ToolStripDropDownButton();
            Ind_ImageTile = new ToolStripMenuItem();
            Ind_ImageCentre = new ToolStripMenuItem();
            Ind_ImageBestFit = new ToolStripMenuItem();
            Ind_NoImage = new ToolStripButton();
            Ind_BackColour = new ToolStripButton();
            toolStripSeparator27 = new ToolStripSeparator();
            Ind_AssignMedia = new ToolStripButton();
            label3 = new Label();
            Ind_LeftUpDown = new NumericUpDown();
            label2 = new Label();
            label1 = new Label();
            Ind_R1Colour = new ToolStripButton();
            Ind_R1AlignRight = new ToolStripMenuItem();
            Ind_R1AlignCentre = new ToolStripMenuItem();
            Ind_R1AlignLeft = new ToolStripMenuItem();
            Ind_R1Align = new ToolStripDropDownButton();
            toolStripSeparator13 = new ToolStripSeparator();
            Ind_R1Underline = new ToolStripButton();
            Ind_R1Italics2 = new ToolStripMenuItem();
            Ind_R1Italics1 = new ToolStripMenuItem();
            Ind_R1Italics0 = new ToolStripMenuItem();
            Ind_R1Italics = new ToolStripDropDownButton();
            Ind_R1Bold = new ToolStripButton();
            panelInd5 = new Panel();
            toolStripInd5 = new ToolStrip();
            Ind_Reg1FontsList = new ToolStripComboBox();
            toolStripInd4 = new ToolStrip();
            Ind_Reg1SizeUpDown = new NumericUpDown();
            labelBlackScreenOn = new Label();
            TimerToFront = new Timer(components);
            Ind_R2Underline = new ToolStripButton();
            toolStripSeparator15 = new ToolStripSeparator();
            Ind_R2Align = new ToolStripDropDownButton();
            Ind_R2AlignLeft = new ToolStripMenuItem();
            Ind_R2AlignCentre = new ToolStripMenuItem();
            Ind_R2AlignRight = new ToolStripMenuItem();
            Ind_R2Colour = new ToolStripButton();
            label7 = new Label();
            IndgroupBox3 = new GroupBox();
            Ind_Reg1TopUpDown = new NumericUpDown();
            panelInd4 = new Panel();
            label4 = new Label();
            panel4 = new Panel();
            toolStrip1 = new ToolStrip();
            Ind_HeadAlign = new ToolStripDropDownButton();
            Ind_HeadAlignAsR1 = new ToolStripMenuItem();
            Ind_HeadAlignAsR2 = new ToolStripMenuItem();
            Ind_HeadAlignLeft = new ToolStripMenuItem();
            Ind_HeadAlignCentre = new ToolStripMenuItem();
            Ind_HeadAlignRight = new ToolStripMenuItem();
            Ind_CapoDown = new ToolStripButton();
            Ind_CapoUp = new ToolStripButton();
            toolStripSeparator5 = new ToolStripSeparator();
            Ind_HideDisplayPanel = new ToolStripButton();
            IndgroupBox1 = new GroupBox();
            panelInd1 = new Panel();
            toolStripInd1 = new ToolStrip();
            Ind_Head = new ToolStripDropDownButton();
            Menu_ImportFolder = new ToolStripMenuItem();
            Menu_GoLiveWithPreview = new ToolStripMenuItem();
            Menu_RefreshOutput = new ToolStripMenuItem();
            toolStripSeparator28 = new ToolStripSeparator();
            Menu_BlackScreen = new ToolStripMenuItem();
            Menu_ClearScreen = new ToolStripMenuItem();
            Menu_StartShow = new ToolStripMenuItem();
            Menu_RestartCurrentItem = new ToolStripMenuItem();
            Menu_MainTools = new ToolStripMenuItem();
            Menu_Import = new ToolStripMenuItem();
            Menu_Export = new ToolStripMenuItem();
            toolStripSeparator32 = new ToolStripSeparator();
            Menu_Recover = new ToolStripMenuItem();
            Menu_Empty = new ToolStripMenuItem();
            toolStripSeparator33 = new ToolStripSeparator();
            Menu_AddToUsages = new ToolStripMenuItem();
            Menu_ViewUsages = new ToolStripMenuItem();
            toolStripSeparator34 = new ToolStripSeparator();
            Menu_SmartMerge = new ToolStripMenuItem();
            Menu_Compact = new ToolStripMenuItem();
            Menu_ClearAllFormatting = new ToolStripMenuItem();
            toolStripSeparator9 = new ToolStripSeparator();
            Menu_ClearRegistrySettings = new ToolStripMenuItem();
            Menu_EditSong = new ToolStripMenuItem();
            Menu_MainOutput = new ToolStripMenuItem();
            toolStripSeparator19 = new ToolStripSeparator();
            Menu_CopySong = new ToolStripMenuItem();
            Menu_MoveSong = new ToolStripMenuItem();
            Menu_DeleteSong = new ToolStripMenuItem();
            toolStripSeparator41 = new ToolStripSeparator();
            Menu_SelectAll = new ToolStripMenuItem();
            Menu_Find = new ToolStripMenuItem();
            Menu_StatusBar = new ToolStripMenuItem();
            toolStripSeparator21 = new ToolStripSeparator();
            Menu_ReArrangeSongFolders = new ToolStripMenuItem();
            Menu_MainView = new ToolStripMenuItem();
            Menu_EasiSlidesFolder = new ToolStripMenuItem();
            Menu_Options = new ToolStripMenuItem();
            toolStripSeparator23 = new ToolStripSeparator();
            Menu_Refresh = new ToolStripMenuItem();
            Menu_PreviewNotations = new ToolStripMenuItem();
            Menu_UseSongNumbering = new ToolStripMenuItem();
            StatusBarPanel2 = new ToolStripStatusLabel();
            StatusBarPanel3 = new ToolStripStatusLabel();
            StatusBarPanel4 = new ToolStripStatusLabel();
            toolTip1 = new ToolTip(components);
            DefApplyDefaultsBtn = new Button();
            TimerFlasher = new Timer(components);
            openFileDialog1 = new OpenFileDialog();
            StatusBarPanel1 = new ToolStripStatusLabel();
            TimerReMax = new Timer(components);
            CMenuImages = new ContextMenuStrip(components);
            CMenuImages_AddItem = new ToolStripMenuItem();
            CMenuImages_AddDefault = new ToolStripMenuItem();
            toolStripSeparator35 = new ToolStripSeparator();
            CMenuImages_Refresh = new ToolStripMenuItem();
            TimerMessagingWindowOpen = new Timer(components);
            TimerSearch = new Timer(components);
            saveFileDialog1 = new SaveFileDialog();
            statusStripMain = new StatusStrip();
            Menu_About = new ToolStripMenuItem();
            Menu_MainHelp = new ToolStripMenuItem();
            Menu_Contents = new ToolStripMenuItem();
            Menu_HelpWeb = new ToolStripMenuItem();
            toolStripSeparator31 = new ToolStripSeparator();
            Menu_Register = new ToolStripMenuItem();
            Menu_AddSong = new ToolStripMenuItem();
            toolStripMain = new ToolStrip();
            Main_New = new ToolStripButton();
            Main_Edit = new ToolStripButton();
            Main_Copy = new ToolStripButton();
            Main_Move = new ToolStripButton();
            Main_Delete = new ToolStripButton();
            toolStripSeparator1 = new ToolStripSeparator();
            Main_Media = new ToolStripButton();
            Main_Refresh = new ToolStripButton();
            toolStripSeparator2 = new ToolStripSeparator();
            Main_Options = new ToolStripButton();
            toolStripSeparator3 = new ToolStripSeparator();
            Main_NoRotate = new ToolStripButton();
            Main_RotateStyle = new ToolStripDropDownButton();
            Main_Rotate0 = new ToolStripMenuItem();
            Main_Rotate1 = new ToolStripMenuItem();
            Main_Rotate2 = new ToolStripMenuItem();
            Main_Rotate3 = new ToolStripMenuItem();
            Main_Alerts = new ToolStripButton();
            Main_Chinese = new ToolStripButton();
            toolStripSeparator4 = new ToolStripSeparator();
            Main_Find = new ToolStripButton();
            Main_QuickFind = new ToolStripComboBox();
            Main_JumpA = new ToolStripButton();
            Main_JumpB = new ToolStripButton();
            Main_JumpC = new ToolStripButton();
            menuStripMain = new MenuStrip();
            Menu_MainFile = new ToolStripMenuItem();
            Menu_WorshipSessions = new ToolStripMenuItem();
            Menu_PraiseBookTemplates = new ToolStripMenuItem();
            toolStripSeparator20 = new ToolStripSeparator();
            Menu_ListingOfSelectedFolder = new ToolStripMenuItem();
            toolStripSeparator16 = new ToolStripSeparator();
            Menu_EditHistoryList = new ToolStripMenuItem();
            toolStripSeparator18 = new ToolStripSeparator();
            Menu_Exit = new ToolStripMenuItem();
            Menu_MainEdit = new ToolStripMenuItem();
            Ind_R2Bold = new ToolStripButton();
            Image_Import = new ToolStripButton();
            CMenuBible_CopyInfoScreen = new ToolStripMenuItem();
            BibleUserLookup = new TextBox();
            panelBible2 = new Panel();
            toolStripBible2 = new ToolStrip();
            Bibles_Go = new ToolStripButton();
            BookLookup = new ComboBox();
            CMenuBible_Copy = new ToolStripMenuItem();
            TabBibleVersions = new TabControl();
            tabPage1 = new TabPage();
            tabImages = new TabPage();
            flowLayoutImages = new FlowLayoutPanel();
            panelImagesTop = new Panel();
            panelImage1 = new Panel();
            toolStripImage1 = new ToolStrip();
            Image_OpenFolder = new ToolStripButton();
            ImagesFolder = new ComboBox();
            panelPowerpoint1 = new Panel();
            PowerpointFolder = new ComboBox();
            panelExternalFiles1 = new Panel();
            toolStripPowerpoint1 = new ToolStrip();
            PP_ListType = new ToolStripDropDownButton();
            PP_ListStyle = new ToolStripMenuItem();
            PP_PreviewStyle = new ToolStripMenuItem();
            PP_OpenFolder = new ToolStripButton();
            PP_Import = new ToolStripButton();
            toolStripSeparator24 = new ToolStripSeparator();
            columnHeader36 = new ColumnHeader();
            CMenuBible_AddRegion2 = new ToolStripMenuItem();
            tabBibles = new TabPage();
            BibleText = new RichTextBox();
            CMenuBible = new ContextMenuStrip(components);
            CMenuBible_SelectAll = new ToolStripMenuItem();
            CMenuBible_UnselectAll = new ToolStripMenuItem();
            CMenuBible_AddShow = new ToolStripMenuItem();
            toolStripSeparator17 = new ToolStripSeparator();
            tabMedia = new TabPage();
            panel11 = new Panel();
            panelMedia1 = new Panel();
            toolStripMedia1 = new ToolStrip();
            Media_OpenFolder = new ToolStripButton();
            Media_Import = new ToolStripButton();
            MediaFolder = new ComboBox();
            MediaList = new ListView();
            columnHeader37 = new ColumnHeader();
            columnHeader38 = new ColumnHeader();
            columnHeader39 = new ColumnHeader();
            columnHeader40 = new ColumnHeader();
            columnHeader41 = new ColumnHeader();
            columnHeader42 = new ColumnHeader();
            columnHeader43 = new ColumnHeader();
            CMenuFiles = new ContextMenuStrip(components);
            CMenuFiles_SelectAll = new ToolStripMenuItem();
            CMenuFiles_UnselectAll = new ToolStripMenuItem();
            CMenuFiles_AddShow = new ToolStripMenuItem();
            toolStripSeparator12 = new ToolStripSeparator();
            CMenuFiles_Edit = new ToolStripMenuItem();
            CMenuFiles_Copy = new ToolStripMenuItem();
            toolStripSeparator25 = new ToolStripSeparator();
            CMenuFiles_Refresh = new ToolStripMenuItem();
            Def_SaveTemplate = new ToolStripButton();
            DefgroupBox2 = new GroupBox();
            panelDef4 = new Panel();
            toolStripDef4 = new ToolStrip();
            Def_TransItem = new ToolStripComboBox();
            Def_TransSlides = new ToolStripComboBox();
            panelDef3 = new Panel();
            toolStripDef3 = new ToolStrip();
            Def_ImageMode = new ToolStripDropDownButton();
            Def_ImageTile = new ToolStripMenuItem();
            Def_ImageCentre = new ToolStripMenuItem();
            Def_ImageBestFit = new ToolStripMenuItem();
            Def_NoImage = new ToolStripButton();
            Def_BackColour = new ToolStripButton();
            Def_AssignMedia = new ToolStripButton();
            Def_LoadTemplate = new ToolStripButton();
            toolStripInd6 = new ToolStrip();
            toolStripDefTemplates = new ToolStrip();
            panelDefTemplate = new Panel();
            tabDefault = new TabPage();
            DefPanel = new Panel();
            DefgroupBox3 = new GroupBox();
            panel21 = new Panel();
            toolStripDef7 = new ToolStrip();
            Def_PanelFontBold = new ToolStripButton();
            Def_PanelFontItalics = new ToolStripButton();
            Def_PanelFontUnderline = new ToolStripButton();
            Def_PanelFontShadow = new ToolStripButton();
            Def_PanelFontOutline = new ToolStripButton();
            Def_PanelFontList = new ToolStripComboBox();
            Def_PanelHeight = new NumericUpDown();
            panelDef5 = new Panel();
            toolStripDef5 = new ToolStrip();
            Def_PanelAsR1 = new ToolStripButton();
            Def_PanelTextColour = new ToolStripButton();
            toolStripSeparator14 = new ToolStripSeparator();
            Def_PanelTransparent = new ToolStripButton();
            Def_PanelBackColour = new ToolStripButton();
            panelDef6 = new Panel();
            toolStripDef6 = new ToolStrip();
            Def_PanelShow = new ToolStripButton();
            Def_PanelTitle = new ToolStripButton();
            Def_PanelCopyright = new ToolStripButton();
            Def_PanelSong = new ToolStripButton();
            Def_PanelSlides = new ToolStripButton();
            Def_PanelPrevNext = new ToolStripButton();
            label5 = new Label();
            DefgroupBox1 = new GroupBox();
            panelDef2 = new Panel();
            toolStripDef2 = new ToolStrip();
            Def_HeadAlign = new ToolStripDropDownButton();
            Def_HeadAlignAsR1 = new ToolStripMenuItem();
            Def_HeadAlignAsR2 = new ToolStripMenuItem();
            Def_HeadAlignLeft = new ToolStripMenuItem();
            Def_HeadAlignCentre = new ToolStripMenuItem();
            Def_HeadAlignRight = new ToolStripMenuItem();
            toolStripSeparator26 = new ToolStripSeparator();
            Def_R1Align = new ToolStripDropDownButton();
            Def_R1AlignLeft = new ToolStripMenuItem();
            Def_R1AlignCentre = new ToolStripMenuItem();
            Def_R1AlignRight = new ToolStripMenuItem();
            Def_R1Colour = new ToolStripButton();
            toolStripSeparator8 = new ToolStripSeparator();
            Def_R2Align = new ToolStripDropDownButton();
            Def_R2AlignLeft = new ToolStripMenuItem();
            Def_R2AlignCentre = new ToolStripMenuItem();
            Def_R2AlignRight = new ToolStripMenuItem();
            Def_R2Colour = new ToolStripButton();
            panelDef1 = new Panel();
            toolStripDef1 = new ToolStrip();
            Def_Head = new ToolStripDropDownButton();
            Def_HeadNoTitles = new ToolStripMenuItem();
            Def_HeadAllTitles = new ToolStripMenuItem();
            Def_HeadFirstScreen = new ToolStripMenuItem();
            Def_Region = new ToolStripDropDownButton();
            Def_ShowRegion1 = new ToolStripMenuItem();
            Def_ShowRegion2 = new ToolStripMenuItem();
            Def_ShowRegionBoth = new ToolStripMenuItem();
            Def_VAlign = new ToolStripDropDownButton();
            Def_VAlignTop = new ToolStripMenuItem();
            Def_VAlignCentre = new ToolStripMenuItem();
            Def_VAlignBottom = new ToolStripMenuItem();
            Def_Shadow = new ToolStripButton();
            Def_Outline = new ToolStripButton();
            Def_Interlace = new ToolStripButton();
            Def_Notations = new ToolStripButton();
            Def_ToZero = new ToolStripButton();
            columnHeader35 = new ColumnHeader();
            InfoScreen_Delete = new ToolStripButton();
            CMenuSongs_AddShow = new ToolStripMenuItem();
            toolStripSeparator38 = new ToolStripSeparator();
            CMenuSongs_Edit = new ToolStripMenuItem();
            CMenuSongs_Copy = new ToolStripMenuItem();
            toolStripSeparator10 = new ToolStripSeparator();
            CMenuSongs_Refresh = new ToolStripMenuItem();
            CMenuSongs_UnselectAll = new ToolStripMenuItem();
            SongFolder = new ComboBox();
            panelInfoScreen2 = new Panel();
            InfoScreentoolstrip2 = new ToolStrip();
            InfoScreen_New = new ToolStripButton();
            InfoScreen_Edit = new ToolStripButton();
            InfoScreen_Copy = new ToolStripButton();
            InfoScreen_Move = new ToolStripButton();
            tabFiles = new TabPage();
            panelExternalFiles = new Panel();
            panelInfoScreen1 = new Panel();
            InfoScreentoolstrip1 = new ToolStrip();
            InfoScreen_OpenFolder = new ToolStripButton();
            InfoScreen_Import = new ToolStripButton();
            InfoScreenFolder = new ComboBox();
            InfoScreenList = new ListView();
            columnHeader23 = new ColumnHeader();
            columnHeader24 = new ColumnHeader();
            columnHeader25 = new ColumnHeader();
            columnHeader26 = new ColumnHeader();
            columnHeader27 = new ColumnHeader();
            columnHeader28 = new ColumnHeader();
            columnHeader29 = new ColumnHeader();
            CMenuSongs_SelectAll = new ToolStripMenuItem();
            toolStripContainerMain = new ToolStripContainer();
            splitContainerMain = new SplitContainer();
            splitContainer1 = new SplitContainer();
            tabControlSource = new TabControl();
            tabFolders = new TabPage();
            panelFolders = new Panel();
            toolStripFolders = new ToolStrip();
            Folders_WordCount = new ToolStripButton();
            SongsList = new ListView();
            columnHeader1 = new ColumnHeader();
            columnHeader2 = new ColumnHeader();
            columnHeader3 = new ColumnHeader();
            columnHeader4 = new ColumnHeader();
            columnHeader5 = new ColumnHeader();
            columnHeader6 = new ColumnHeader();
            columnHeader7 = new ColumnHeader();
            CMenuSongs = new ContextMenuStrip(components);
            tabPowerpoint = new TabPage();
            flowLayoutExternalPowerPoint = new FlowLayoutPanel();
            panelPowerpoint2 = new Panel();
            toolStripPowerpoint2 = new ToolStrip();
            Powerpoint_Edit = new ToolStripButton();
            Powerpoint_Copy = new ToolStripButton();
            Powerpoint_Move = new ToolStripButton();
            Powerpoint_Delete = new ToolStripButton();
            PowerpointList = new ListView();
            columnHeader30 = new ColumnHeader();
            columnHeader31 = new ColumnHeader();
            columnHeader32 = new ColumnHeader();
            columnHeader33 = new ColumnHeader();
            columnHeader34 = new ColumnHeader();
            tabControlLists = new TabControl();
            tabWorshipList = new TabPage();
            panelWorshipList2 = new Panel();
            toolStripWorshipList2 = new ToolStrip();
            WL_Up = new ToolStripButton();
            WL_Down = new ToolStripButton();
            WL_Delete = new ToolStripButton();
            toolStripSeparator6 = new ToolStripSeparator();
            WL_Word = new ToolStripButton();
            WL_Notes = new ToolStripButton();
            panelWorshipList1 = new Panel();
            toolStripWorshipList1 = new ToolStrip();
            WL_Manage = new ToolStripButton();
            WL_Add = new ToolStripButton();
            WL_Open = new ToolStripButton();
            WorshipListItems = new ListView();
            columnHeader8 = new ColumnHeader();
            columnHeader9 = new ColumnHeader();
            columnHeader10 = new ColumnHeader();
            columnHeader11 = new ColumnHeader();
            columnHeader12 = new ColumnHeader();
            columnHeader13 = new ColumnHeader();
            columnHeader14 = new ColumnHeader();
            CMenuWorship = new ContextMenuStrip(components);
            CMenuWorship_SelectAll = new ToolStripMenuItem();
            CMenuWorship_UnselectAll = new ToolStripMenuItem();
            CMenuWorship_Clear = new ToolStripMenuItem();
            toolStripSeparator39 = new ToolStripSeparator();
            CMenuWorship_Edit = new ToolStripMenuItem();
            CMenuWorship_Play = new ToolStripMenuItem();
            CMenuWorship_PlayOnOutput = new ToolStripMenuItem();
            toolStripSeparator37 = new ToolStripSeparator();
            CMenuWorship_AddUsages = new ToolStripMenuItem();
            SessionList = new ComboBox();
            tabPraiseBook = new TabPage();
            panelPraiseBook2 = new Panel();
            toolStripPraiseBook2 = new ToolStrip();
            toolStripSeparator22 = new ToolStripSeparator();
            PB_Delete = new ToolStripButton();
            toolStripSeparator7 = new ToolStripSeparator();
            PB_Word = new ToolStripButton();
            PB_Html = new ToolStripButton();
            panelPraiseBook1 = new Panel();
            toolStripPraiseBook1 = new ToolStrip();
            PB_Manage = new ToolStripButton();
            PB_Add = new ToolStripButton();
            PB_WordCount = new ToolStripButton();
            PraiseBookItems = new ListView();
            columnHeader17 = new ColumnHeader();
            columnHeader18 = new ColumnHeader();
            columnHeader19 = new ColumnHeader();
            columnHeader20 = new ColumnHeader();
            columnHeader21 = new ColumnHeader();
            columnHeader22 = new ColumnHeader();
            CMenuPraiseB = new ContextMenuStrip(components);
            CMenuPraiseB_SelectAll = new ToolStripMenuItem();
            CMenuPraiseB_UnselectAll = new ToolStripMenuItem();
            CMenuPraiseB_Clear = new ToolStripMenuItem();
            toolStripSeparator36 = new ToolStripSeparator();
            CMenuPraiseB_Edit = new ToolStripMenuItem();
            PraiseBook = new ComboBox();
            splitContainer2 = new SplitContainer();
            splitContainerPreview = new SplitContainer();
            panelPreviewTop = new Panel();
            flowLayoutPreviewPowerPoint = new FlowLayoutPanel();
            IndPanel = new Panel();
            panelIndTemplate = new Panel();
            toolStripIndTemplates = new ToolStrip();
            Ind_LoadTemplate = new ToolStripButton();
            Ind_SaveTemplate = new ToolStripButton();
            IndgroupBox4 = new GroupBox();
            panelInd7 = new Panel();
            toolStripInd7 = new ToolStrip();
            Ind_Reg2FontsList = new ToolStripComboBox();
            Ind_Reg2SizeUpDown = new NumericUpDown();
            label6 = new Label();
            Ind_Reg2TopUpDown = new NumericUpDown();
            panelInd6 = new Panel();
            flowLayoutPanel1.SuspendLayout();
            panel1.SuspendLayout();
            panel3.SuspendLayout();
            panelPreviewSessionNotes2.SuspendLayout();
            panelPreviewBottom.SuspendLayout();
            panel9.SuspendLayout();
            panelOutputTop.SuspendLayout();
            panel10.SuspendLayout();
            panelOutputBottom.SuspendLayout();
            panelOutputLM1.SuspendLayout();
            panelOutputLM3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainerOutput).BeginInit();
            splitContainerOutput.Panel1.SuspendLayout();
            splitContainerOutput.Panel2.SuspendLayout();
            splitContainerOutput.SuspendLayout();
            panel8.SuspendLayout();
            panel2.SuspendLayout();
            flowLayoutPanel2.SuspendLayout();
            panel6.SuspendLayout();
            IndgroupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)Ind_BottomUpDown).BeginInit();
            panelInd3.SuspendLayout();
            toolStripInd3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)Ind_RightUpDown).BeginInit();
            panelInd2.SuspendLayout();
            toolStripInd2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)Ind_LeftUpDown).BeginInit();
            panelInd5.SuspendLayout();
            toolStripInd5.SuspendLayout();
            toolStripInd4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)Ind_Reg1SizeUpDown).BeginInit();
            IndgroupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)Ind_Reg1TopUpDown).BeginInit();
            panelInd4.SuspendLayout();
            panel4.SuspendLayout();
            toolStrip1.SuspendLayout();
            IndgroupBox1.SuspendLayout();
            panelInd1.SuspendLayout();
            toolStripInd1.SuspendLayout();
            CMenuImages.SuspendLayout();
            statusStripMain.SuspendLayout();
            toolStripMain.SuspendLayout();
            menuStripMain.SuspendLayout();
            panelBible2.SuspendLayout();
            toolStripBible2.SuspendLayout();
            TabBibleVersions.SuspendLayout();
            tabImages.SuspendLayout();
            panelImagesTop.SuspendLayout();
            panelImage1.SuspendLayout();
            toolStripImage1.SuspendLayout();
            panelPowerpoint1.SuspendLayout();
            panelExternalFiles1.SuspendLayout();
            toolStripPowerpoint1.SuspendLayout();
            tabBibles.SuspendLayout();
            CMenuBible.SuspendLayout();
            tabMedia.SuspendLayout();
            panel11.SuspendLayout();
            panelMedia1.SuspendLayout();
            toolStripMedia1.SuspendLayout();
            CMenuFiles.SuspendLayout();
            DefgroupBox2.SuspendLayout();
            panelDef4.SuspendLayout();
            toolStripDef4.SuspendLayout();
            panelDef3.SuspendLayout();
            toolStripDef3.SuspendLayout();
            toolStripInd6.SuspendLayout();
            toolStripDefTemplates.SuspendLayout();
            panelDefTemplate.SuspendLayout();
            tabDefault.SuspendLayout();
            DefPanel.SuspendLayout();
            DefgroupBox3.SuspendLayout();
            panel21.SuspendLayout();
            toolStripDef7.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)Def_PanelHeight).BeginInit();
            panelDef5.SuspendLayout();
            toolStripDef5.SuspendLayout();
            panelDef6.SuspendLayout();
            toolStripDef6.SuspendLayout();
            DefgroupBox1.SuspendLayout();
            panelDef2.SuspendLayout();
            toolStripDef2.SuspendLayout();
            panelDef1.SuspendLayout();
            toolStripDef1.SuspendLayout();
            panelInfoScreen2.SuspendLayout();
            InfoScreentoolstrip2.SuspendLayout();
            tabFiles.SuspendLayout();
            panelExternalFiles.SuspendLayout();
            panelInfoScreen1.SuspendLayout();
            InfoScreentoolstrip1.SuspendLayout();
            toolStripContainerMain.ContentPanel.SuspendLayout();
            toolStripContainerMain.TopToolStripPanel.SuspendLayout();
            toolStripContainerMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainerMain).BeginInit();
            splitContainerMain.Panel1.SuspendLayout();
            splitContainerMain.Panel2.SuspendLayout();
            splitContainerMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            tabControlSource.SuspendLayout();
            tabFolders.SuspendLayout();
            panelFolders.SuspendLayout();
            toolStripFolders.SuspendLayout();
            CMenuSongs.SuspendLayout();
            tabPowerpoint.SuspendLayout();
            panelPowerpoint2.SuspendLayout();
            toolStripPowerpoint2.SuspendLayout();
            tabControlLists.SuspendLayout();
            tabWorshipList.SuspendLayout();
            panelWorshipList2.SuspendLayout();
            toolStripWorshipList2.SuspendLayout();
            panelWorshipList1.SuspendLayout();
            toolStripWorshipList1.SuspendLayout();
            CMenuWorship.SuspendLayout();
            tabPraiseBook.SuspendLayout();
            panelPraiseBook2.SuspendLayout();
            toolStripPraiseBook2.SuspendLayout();
            panelPraiseBook1.SuspendLayout();
            toolStripPraiseBook1.SuspendLayout();
            CMenuPraiseB.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer2).BeginInit();
            splitContainer2.Panel1.SuspendLayout();
            splitContainer2.Panel2.SuspendLayout();
            splitContainer2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainerPreview).BeginInit();
            splitContainerPreview.Panel1.SuspendLayout();
            splitContainerPreview.Panel2.SuspendLayout();
            splitContainerPreview.SuspendLayout();
            panelPreviewTop.SuspendLayout();
            IndPanel.SuspendLayout();
            panelIndTemplate.SuspendLayout();
            toolStripIndTemplates.SuspendLayout();
            IndgroupBox4.SuspendLayout();
            panelInd7.SuspendLayout();
            toolStripInd7.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)Ind_Reg2SizeUpDown).BeginInit();
            ((System.ComponentModel.ISupportInitialize)Ind_Reg2TopUpDown).BeginInit();
            panelInd6.SuspendLayout();
            SuspendLayout();
            // 
            // imageListSys
            // 
            imageListSys.ColorDepth = ColorDepth.Depth16Bit;
            imageListSys.ImageStream = (ImageListStreamer)resources.GetObject("imageListSys.ImageStream");
            imageListSys.TransparentColor = System.Drawing.Color.Transparent;
            imageListSys.Images.SetKeyName(0, "fivicon.jpg");
            imageListSys.Images.SetKeyName(1, "favicon-Highlight.png");
            imageListSys.Images.SetKeyName(2, "pptType.png");
            imageListSys.Images.SetKeyName(3, "PPTType-Highlight.png");
            imageListSys.Images.SetKeyName(4, "BibleType.gif");
            imageListSys.Images.SetKeyName(5, "BibleType-Highlight.gif");
            imageListSys.Images.SetKeyName(6, "NotebookType.gif");
            imageListSys.Images.SetKeyName(7, "NotebookType-Highlight.gif");
            imageListSys.Images.SetKeyName(8, "infoType.gif");
            imageListSys.Images.SetKeyName(9, "infoType-Highlight.gif");
            imageListSys.Images.SetKeyName(10, "WordType.gif");
            imageListSys.Images.SetKeyName(11, "WordType-Highlight.gif");
            imageListSys.Images.SetKeyName(12, "singleScreen.png");
            imageListSys.Images.SetKeyName(13, "dualScreen.png");
            imageListSys.Images.SetKeyName(14, "keyboard.png");
            imageListSys.Images.SetKeyName(15, "BlackScreen.gif");
            imageListSys.Images.SetKeyName(16, "BlackScreen-red.gif");
            imageListSys.Images.SetKeyName(17, "hideText.gif");
            imageListSys.Images.SetKeyName(18, "hideText-red.gif");
            imageListSys.Images.SetKeyName(19, "folderOpen.gif");
            imageListSys.Images.SetKeyName(20, "pic-bestfit.gif");
            imageListSys.Images.SetKeyName(21, "Bibles_Go.gif");
            imageListSys.Images.SetKeyName(22, "Option.gif");
            imageListSys.Images.SetKeyName(23, "infoType.gif");
            imageListSys.Images.SetKeyName(24, "PPTListType.gif");
            imageListSys.Images.SetKeyName(25, "tick.gif");
            imageListSys.Images.SetKeyName(26, "singleScreen.png");
            imageListSys.Images.SetKeyName(27, "question-mark.gif");
            imageListSys.Images.SetKeyName(28, "mediaType.gif");
            imageListSys.Images.SetKeyName(29, "mediaType-Highlight.gif");
            imageListSys.Images.SetKeyName(30, "LiveCam.gif");
            imageListSys.Images.SetKeyName(31, "LiveCam-red.gif");
            imageListSys.Images.SetKeyName(32, "LiveCam.gif");
            imageListSys.Images.SetKeyName(33, "btnLive.png");
            imageListSys.Images.SetKeyName(34, "Move Up.png");
            imageListSys.Images.SetKeyName(35, "Move Down.png");
            imageListSys.Images.SetKeyName(36, "EditSessionNote.png");
            imageListSys.Images.SetKeyName(37, "EditSessionNote.png");
            imageListSys.Images.SetKeyName(38, "btnLive.png");
            imageListSys.Images.SetKeyName(39, "btnToOutputMove.png");
            imageListSys.Images.SetKeyName(40, "btnToOutput.png");
            imageListSys.Images.SetKeyName(41, "SetPreviewNote.png");
            imageListSys.Images.SetKeyName(42, "MoveToUpList.png");
            imageListSys.Images.SetKeyName(43, "MoveToDownList.png");
            imageListSys.Images.SetKeyName(44, "MoveToUpContent.png");
            imageListSys.Images.SetKeyName(45, "MoveToDownContent.png");
            imageListSys.Images.SetKeyName(46, "MediaFile.png");
            imageListSys.Images.SetKeyName(47, "Alert.png");
            imageListSys.Images.SetKeyName(48, "MoveToUpList.png");
            imageListSys.Images.SetKeyName(49, "MoveToDownList.png");
            imageListSys.Images.SetKeyName(50, "MoveToUpContent.png");
            imageListSys.Images.SetKeyName(51, "MoveToDownContent.png");
            // 
            // Ind_HeadFirstScreen
            // 
            Ind_HeadFirstScreen.Image = (System.Drawing.Image)resources.GetObject("Ind_HeadFirstScreen.Image");
            Ind_HeadFirstScreen.Name = "Ind_HeadFirstScreen";
            Ind_HeadFirstScreen.Size = new System.Drawing.Size(281, 26);
            Ind_HeadFirstScreen.Tag = "2";
            Ind_HeadFirstScreen.Text = "Heading At First Screen Only";
            // 
            // PreviewBtnVerse1
            // 
            PreviewBtnVerse1.FlatStyle = FlatStyle.Flat;
            PreviewBtnVerse1.Location = new System.Drawing.Point(0, 0);
            PreviewBtnVerse1.Margin = new Padding(0);
            PreviewBtnVerse1.Name = "PreviewBtnVerse1";
            PreviewBtnVerse1.Size = new System.Drawing.Size(19, 33);
            PreviewBtnVerse1.TabIndex = 4;
            PreviewBtnVerse1.Tag = "1";
            PreviewBtnVerse1.Text = "1";
            PreviewBtnVerse1.Click += PreviewBtnVerse_Click;
            // 
            // PreviewBtnVerse2
            // 
            PreviewBtnVerse2.Dock = DockStyle.Left;
            PreviewBtnVerse2.FlatStyle = FlatStyle.Flat;
            PreviewBtnVerse2.Location = new System.Drawing.Point(19, 0);
            PreviewBtnVerse2.Margin = new Padding(0);
            PreviewBtnVerse2.Name = "PreviewBtnVerse2";
            PreviewBtnVerse2.Size = new System.Drawing.Size(19, 33);
            PreviewBtnVerse2.TabIndex = 18;
            PreviewBtnVerse2.Tag = "2";
            PreviewBtnVerse2.Text = "2";
            PreviewBtnVerse2.Click += PreviewBtnVerse_Click;
            // 
            // PreviewBtnVerse3
            // 
            PreviewBtnVerse3.Dock = DockStyle.Left;
            PreviewBtnVerse3.FlatStyle = FlatStyle.Flat;
            PreviewBtnVerse3.Location = new System.Drawing.Point(38, 0);
            PreviewBtnVerse3.Margin = new Padding(0);
            PreviewBtnVerse3.Name = "PreviewBtnVerse3";
            PreviewBtnVerse3.Size = new System.Drawing.Size(19, 33);
            PreviewBtnVerse3.TabIndex = 19;
            PreviewBtnVerse3.Tag = "3";
            PreviewBtnVerse3.Text = "3";
            PreviewBtnVerse3.Click += PreviewBtnVerse_Click;
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.Controls.Add(PreviewBtnVerse1);
            flowLayoutPanel1.Controls.Add(PreviewBtnVerse2);
            flowLayoutPanel1.Controls.Add(PreviewBtnVerse3);
            flowLayoutPanel1.Controls.Add(PreviewBtnVerse4);
            flowLayoutPanel1.Controls.Add(PreviewBtnVerse5);
            flowLayoutPanel1.Controls.Add(PreviewBtnVerse6);
            flowLayoutPanel1.Controls.Add(PreviewBtnVerse7);
            flowLayoutPanel1.Controls.Add(PreviewBtnVerse8);
            flowLayoutPanel1.Controls.Add(PreviewBtnVerse9);
            flowLayoutPanel1.Controls.Add(PreviewBtnVersePreChorus);
            flowLayoutPanel1.Controls.Add(PreviewBtnVersePreChorus2);
            flowLayoutPanel1.Controls.Add(PreviewBtnVerseChorus);
            flowLayoutPanel1.Controls.Add(PreviewBtnVerseChorus2);
            flowLayoutPanel1.Controls.Add(PreviewBtnVerseBridge);
            flowLayoutPanel1.Controls.Add(PreviewBtnVerseBridge2);
            flowLayoutPanel1.Controls.Add(PreviewBtnVerseEnding);
            flowLayoutPanel1.Location = new System.Drawing.Point(296, 0);
            flowLayoutPanel1.Margin = new Padding(0);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Size = new System.Drawing.Size(330, 33);
            flowLayoutPanel1.TabIndex = 1;
            // 
            // PreviewBtnVerse4
            // 
            PreviewBtnVerse4.Dock = DockStyle.Left;
            PreviewBtnVerse4.FlatStyle = FlatStyle.Flat;
            PreviewBtnVerse4.Location = new System.Drawing.Point(57, 0);
            PreviewBtnVerse4.Margin = new Padding(0);
            PreviewBtnVerse4.Name = "PreviewBtnVerse4";
            PreviewBtnVerse4.Size = new System.Drawing.Size(19, 33);
            PreviewBtnVerse4.TabIndex = 20;
            PreviewBtnVerse4.Tag = "4";
            PreviewBtnVerse4.Text = "4";
            PreviewBtnVerse4.Click += PreviewBtnVerse_Click;
            // 
            // PreviewBtnVerse5
            // 
            PreviewBtnVerse5.Dock = DockStyle.Left;
            PreviewBtnVerse5.FlatStyle = FlatStyle.Flat;
            PreviewBtnVerse5.Location = new System.Drawing.Point(76, 0);
            PreviewBtnVerse5.Margin = new Padding(0);
            PreviewBtnVerse5.Name = "PreviewBtnVerse5";
            PreviewBtnVerse5.Size = new System.Drawing.Size(19, 33);
            PreviewBtnVerse5.TabIndex = 21;
            PreviewBtnVerse5.Tag = "5";
            PreviewBtnVerse5.Text = "5";
            PreviewBtnVerse5.Click += PreviewBtnVerse_Click;
            // 
            // PreviewBtnVerse6
            // 
            PreviewBtnVerse6.Dock = DockStyle.Left;
            PreviewBtnVerse6.FlatStyle = FlatStyle.Flat;
            PreviewBtnVerse6.Location = new System.Drawing.Point(95, 0);
            PreviewBtnVerse6.Margin = new Padding(0);
            PreviewBtnVerse6.Name = "PreviewBtnVerse6";
            PreviewBtnVerse6.Size = new System.Drawing.Size(19, 33);
            PreviewBtnVerse6.TabIndex = 22;
            PreviewBtnVerse6.Tag = "6";
            PreviewBtnVerse6.Text = "6";
            PreviewBtnVerse6.Click += PreviewBtnVerse_Click;
            // 
            // PreviewBtnVerse7
            // 
            PreviewBtnVerse7.Dock = DockStyle.Left;
            PreviewBtnVerse7.FlatStyle = FlatStyle.Flat;
            PreviewBtnVerse7.Location = new System.Drawing.Point(114, 0);
            PreviewBtnVerse7.Margin = new Padding(0);
            PreviewBtnVerse7.Name = "PreviewBtnVerse7";
            PreviewBtnVerse7.Size = new System.Drawing.Size(19, 33);
            PreviewBtnVerse7.TabIndex = 23;
            PreviewBtnVerse7.Tag = "7";
            PreviewBtnVerse7.Text = "7";
            PreviewBtnVerse7.Click += PreviewBtnVerse_Click;
            // 
            // PreviewBtnVerse8
            // 
            PreviewBtnVerse8.Dock = DockStyle.Left;
            PreviewBtnVerse8.FlatStyle = FlatStyle.Flat;
            PreviewBtnVerse8.Location = new System.Drawing.Point(133, 0);
            PreviewBtnVerse8.Margin = new Padding(0);
            PreviewBtnVerse8.Name = "PreviewBtnVerse8";
            PreviewBtnVerse8.Size = new System.Drawing.Size(19, 33);
            PreviewBtnVerse8.TabIndex = 24;
            PreviewBtnVerse8.Tag = "8";
            PreviewBtnVerse8.Text = "8";
            PreviewBtnVerse8.Click += PreviewBtnVerse_Click;
            // 
            // PreviewBtnVerse9
            // 
            PreviewBtnVerse9.Dock = DockStyle.Left;
            PreviewBtnVerse9.FlatStyle = FlatStyle.Flat;
            PreviewBtnVerse9.Location = new System.Drawing.Point(152, 0);
            PreviewBtnVerse9.Margin = new Padding(0);
            PreviewBtnVerse9.Name = "PreviewBtnVerse9";
            PreviewBtnVerse9.Size = new System.Drawing.Size(19, 33);
            PreviewBtnVerse9.TabIndex = 25;
            PreviewBtnVerse9.Tag = "9";
            PreviewBtnVerse9.Text = "9";
            PreviewBtnVerse9.Click += PreviewBtnVerse_Click;
            // 
            // PreviewBtnVersePreChorus
            // 
            PreviewBtnVersePreChorus.Dock = DockStyle.Left;
            PreviewBtnVersePreChorus.FlatStyle = FlatStyle.Flat;
            PreviewBtnVersePreChorus.Location = new System.Drawing.Point(171, 0);
            PreviewBtnVersePreChorus.Margin = new Padding(0);
            PreviewBtnVersePreChorus.Name = "PreviewBtnVersePreChorus";
            PreviewBtnVersePreChorus.Size = new System.Drawing.Size(19, 33);
            PreviewBtnVersePreChorus.TabIndex = 26;
            PreviewBtnVersePreChorus.Tag = "111";
            PreviewBtnVersePreChorus.Text = "p";
            PreviewBtnVersePreChorus.Click += PreviewBtnVerse_Click;
            // 
            // PreviewBtnVersePreChorus2
            // 
            PreviewBtnVersePreChorus2.Dock = DockStyle.Left;
            PreviewBtnVersePreChorus2.FlatStyle = FlatStyle.Flat;
            PreviewBtnVersePreChorus2.Location = new System.Drawing.Point(190, 0);
            PreviewBtnVersePreChorus2.Margin = new Padding(0);
            PreviewBtnVersePreChorus2.Name = "PreviewBtnVersePreChorus2";
            PreviewBtnVersePreChorus2.Size = new System.Drawing.Size(19, 33);
            PreviewBtnVersePreChorus2.TabIndex = 27;
            PreviewBtnVersePreChorus2.Tag = "112";
            PreviewBtnVersePreChorus2.Text = "q";
            PreviewBtnVersePreChorus2.Click += PreviewBtnVerse_Click;
            // 
            // PreviewBtnVerseChorus
            // 
            PreviewBtnVerseChorus.FlatStyle = FlatStyle.Flat;
            PreviewBtnVerseChorus.Location = new System.Drawing.Point(209, 0);
            PreviewBtnVerseChorus.Margin = new Padding(0);
            PreviewBtnVerseChorus.Name = "PreviewBtnVerseChorus";
            PreviewBtnVerseChorus.Size = new System.Drawing.Size(19, 33);
            PreviewBtnVerseChorus.TabIndex = 28;
            PreviewBtnVerseChorus.Tag = "0";
            PreviewBtnVerseChorus.Text = "c";
            PreviewBtnVerseChorus.Click += PreviewBtnVerse_Click;
            // 
            // PreviewBtnVerseChorus2
            // 
            PreviewBtnVerseChorus2.FlatStyle = FlatStyle.Flat;
            PreviewBtnVerseChorus2.Location = new System.Drawing.Point(228, 0);
            PreviewBtnVerseChorus2.Margin = new Padding(0);
            PreviewBtnVerseChorus2.Name = "PreviewBtnVerseChorus2";
            PreviewBtnVerseChorus2.Size = new System.Drawing.Size(19, 33);
            PreviewBtnVerseChorus2.TabIndex = 30;
            PreviewBtnVerseChorus2.Tag = "102";
            PreviewBtnVerseChorus2.Text = "t";
            PreviewBtnVerseChorus2.Click += PreviewBtnVerse_Click;
            // 
            // PreviewBtnVerseBridge
            // 
            PreviewBtnVerseBridge.FlatStyle = FlatStyle.Flat;
            PreviewBtnVerseBridge.Location = new System.Drawing.Point(247, 0);
            PreviewBtnVerseBridge.Margin = new Padding(0);
            PreviewBtnVerseBridge.Name = "PreviewBtnVerseBridge";
            PreviewBtnVerseBridge.Size = new System.Drawing.Size(19, 33);
            PreviewBtnVerseBridge.TabIndex = 29;
            PreviewBtnVerseBridge.Tag = "100";
            PreviewBtnVerseBridge.Text = "b";
            PreviewBtnVerseBridge.Click += PreviewBtnVerse_Click;
            // 
            // PreviewBtnVerseBridge2
            // 
            PreviewBtnVerseBridge2.FlatStyle = FlatStyle.Flat;
            PreviewBtnVerseBridge2.Location = new System.Drawing.Point(266, 0);
            PreviewBtnVerseBridge2.Margin = new Padding(0);
            PreviewBtnVerseBridge2.Name = "PreviewBtnVerseBridge2";
            PreviewBtnVerseBridge2.Size = new System.Drawing.Size(23, 33);
            PreviewBtnVerseBridge2.TabIndex = 32;
            PreviewBtnVerseBridge2.Tag = "103";
            PreviewBtnVerseBridge2.Text = "w";
            PreviewBtnVerseBridge2.Click += PreviewBtnVerse_Click;
            // 
            // PreviewBtnVerseEnding
            // 
            PreviewBtnVerseEnding.FlatStyle = FlatStyle.Flat;
            PreviewBtnVerseEnding.Location = new System.Drawing.Point(289, 0);
            PreviewBtnVerseEnding.Margin = new Padding(0);
            PreviewBtnVerseEnding.Name = "PreviewBtnVerseEnding";
            PreviewBtnVerseEnding.Size = new System.Drawing.Size(19, 33);
            PreviewBtnVerseEnding.TabIndex = 31;
            PreviewBtnVerseEnding.Tag = "101";
            PreviewBtnVerseEnding.Text = "e";
            PreviewBtnVerseEnding.Click += PreviewBtnVerse_Click;
            // 
            // panel7
            // 
            panel7.BackColor = System.Drawing.SystemColors.Control;
            panel7.Dock = DockStyle.Top;
            panel7.Location = new System.Drawing.Point(0, 33);
            panel7.Margin = new Padding(3, 5, 3, 5);
            panel7.Name = "panel7";
            panel7.Size = new System.Drawing.Size(368, 1);
            panel7.TabIndex = 1;
            // 
            // panel1
            // 
            panel1.BackColor = System.Drawing.SystemColors.Control;
            panel1.Controls.Add(flowLayoutPanel1);
            panel1.Controls.Add(panel3);
            panel1.Dock = DockStyle.Top;
            panel1.Location = new System.Drawing.Point(0, 0);
            panel1.Margin = new Padding(3, 5, 3, 5);
            panel1.Name = "panel1";
            panel1.Size = new System.Drawing.Size(368, 33);
            panel1.TabIndex = 0;
            // 
            // panel3
            // 
            panel3.Controls.Add(PreviewBtnSlideDown);
            panel3.Controls.Add(PreviewBtnSlideUp);
            panel3.Controls.Add(PreviewBtnItemDown);
            panel3.Controls.Add(PreviewBtnItemUp);
            panel3.Controls.Add(IndradioButtonInfo);
            panel3.Controls.Add(IndradioButtonFormat);
            panel3.Controls.Add(IndradioButtonText);
            panel3.Controls.Add(IndcbPreviewNotes);
            panel3.Dock = DockStyle.Left;
            panel3.Location = new System.Drawing.Point(0, 0);
            panel3.Margin = new Padding(3, 5, 3, 5);
            panel3.Name = "panel3";
            panel3.Size = new System.Drawing.Size(294, 33);
            panel3.TabIndex = 0;
            // 
            // PreviewBtnSlideDown
            // 
            PreviewBtnSlideDown.Dock = DockStyle.Left;
            PreviewBtnSlideDown.ImageIndex = 45;
            PreviewBtnSlideDown.ImageList = imageListSys;
            PreviewBtnSlideDown.Location = new System.Drawing.Point(254, 0);
            PreviewBtnSlideDown.Margin = new Padding(3, 5, 3, 5);
            PreviewBtnSlideDown.Name = "PreviewBtnSlideDown";
            PreviewBtnSlideDown.Size = new System.Drawing.Size(30, 33);
            PreviewBtnSlideDown.TabIndex = 3;
            toolTip1.SetToolTip(PreviewBtnSlideDown, "Next Slide");
            PreviewBtnSlideDown.Click += PreviewBtnUpDown_Click;
            // 
            // PreviewBtnSlideUp
            // 
            PreviewBtnSlideUp.Dock = DockStyle.Left;
            PreviewBtnSlideUp.ImageIndex = 44;
            PreviewBtnSlideUp.ImageList = imageListSys;
            PreviewBtnSlideUp.Location = new System.Drawing.Point(224, 0);
            PreviewBtnSlideUp.Margin = new Padding(3, 5, 3, 5);
            PreviewBtnSlideUp.Name = "PreviewBtnSlideUp";
            PreviewBtnSlideUp.Size = new System.Drawing.Size(30, 33);
            PreviewBtnSlideUp.TabIndex = 2;
            toolTip1.SetToolTip(PreviewBtnSlideUp, "Previous Slide");
            PreviewBtnSlideUp.Click += PreviewBtnUpDown_Click;
            // 
            // PreviewBtnItemDown
            // 
            PreviewBtnItemDown.Dock = DockStyle.Left;
            PreviewBtnItemDown.ImageIndex = 43;
            PreviewBtnItemDown.ImageList = imageListSys;
            PreviewBtnItemDown.Location = new System.Drawing.Point(194, 0);
            PreviewBtnItemDown.Margin = new Padding(3, 5, 3, 5);
            PreviewBtnItemDown.Name = "PreviewBtnItemDown";
            PreviewBtnItemDown.Size = new System.Drawing.Size(30, 33);
            PreviewBtnItemDown.TabIndex = 7;
            toolTip1.SetToolTip(PreviewBtnItemDown, "Next Item");
            PreviewBtnItemDown.Click += PreviewBtnUpDown_Click;
            // 
            // PreviewBtnItemUp
            // 
            PreviewBtnItemUp.Dock = DockStyle.Left;
            PreviewBtnItemUp.ImageIndex = 42;
            PreviewBtnItemUp.ImageList = imageListSys;
            PreviewBtnItemUp.Location = new System.Drawing.Point(164, 0);
            PreviewBtnItemUp.Margin = new Padding(3, 5, 3, 5);
            PreviewBtnItemUp.Name = "PreviewBtnItemUp";
            PreviewBtnItemUp.Size = new System.Drawing.Size(30, 33);
            PreviewBtnItemUp.TabIndex = 6;
            toolTip1.SetToolTip(PreviewBtnItemUp, "Previous Item");
            PreviewBtnItemUp.Click += PreviewBtnUpDown_Click;
            // 
            // IndradioButtonInfo
            // 
            IndradioButtonInfo.Appearance = Appearance.Button;
            IndradioButtonInfo.AutoSize = true;
            IndradioButtonInfo.Dock = DockStyle.Left;
            IndradioButtonInfo.Location = new System.Drawing.Point(119, 0);
            IndradioButtonInfo.Margin = new Padding(3, 5, 3, 5);
            IndradioButtonInfo.Name = "IndradioButtonInfo";
            IndradioButtonInfo.Size = new System.Drawing.Size(45, 33);
            IndradioButtonInfo.TabIndex = 8;
            IndradioButtonInfo.Text = "Info";
            IndradioButtonInfo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            IndradioButtonInfo.Click += IndradioButtonTextFormatInfo_Click;
            // 
            // IndradioButtonFormat
            // 
            IndradioButtonFormat.Appearance = Appearance.Button;
            IndradioButtonFormat.AutoSize = true;
            IndradioButtonFormat.Dock = DockStyle.Left;
            IndradioButtonFormat.Location = new System.Drawing.Point(79, 0);
            IndradioButtonFormat.Margin = new Padding(0, 5, 0, 5);
            IndradioButtonFormat.Name = "IndradioButtonFormat";
            IndradioButtonFormat.Size = new System.Drawing.Size(40, 33);
            IndradioButtonFormat.TabIndex = 5;
            IndradioButtonFormat.Text = "Set";
            IndradioButtonFormat.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            toolTip1.SetToolTip(IndradioButtonFormat, "Format Text");
            IndradioButtonFormat.Click += IndradioButtonTextFormatInfo_Click;
            // 
            // IndradioButtonText
            // 
            IndradioButtonText.Appearance = Appearance.Button;
            IndradioButtonText.AutoSize = true;
            IndradioButtonText.Dock = DockStyle.Left;
            IndradioButtonText.Location = new System.Drawing.Point(33, 0);
            IndradioButtonText.Margin = new Padding(3, 5, 3, 5);
            IndradioButtonText.Name = "IndradioButtonText";
            IndradioButtonText.Size = new System.Drawing.Size(46, 33);
            IndradioButtonText.TabIndex = 4;
            IndradioButtonText.Text = "Text";
            IndradioButtonText.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            IndradioButtonText.Click += IndradioButtonTextFormatInfo_Click;
            // 
            // IndcbPreviewNotes
            // 
            IndcbPreviewNotes.Appearance = Appearance.Button;
            IndcbPreviewNotes.BackgroundImageLayout = ImageLayout.Stretch;
            IndcbPreviewNotes.Dock = DockStyle.Left;
            IndcbPreviewNotes.ImageIndex = 41;
            IndcbPreviewNotes.ImageList = imageListSys;
            IndcbPreviewNotes.Location = new System.Drawing.Point(0, 0);
            IndcbPreviewNotes.Margin = new Padding(3, 5, 3, 5);
            IndcbPreviewNotes.Name = "IndcbPreviewNotes";
            IndcbPreviewNotes.Size = new System.Drawing.Size(33, 33);
            IndcbPreviewNotes.TabIndex = 11;
            toolTip1.SetToolTip(IndcbPreviewNotes, "Show Session Notes");
            IndcbPreviewNotes.UseVisualStyleBackColor = true;
            IndcbPreviewNotes.Click += IndcbPreviewNotes_Click;
            // 
            // PreviewHolder
            // 
            PreviewHolder.BackColor = System.Drawing.Color.FromArgb(20, 20, 20);
            PreviewHolder.Location = new System.Drawing.Point(9, 7);
            PreviewHolder.Margin = new Padding(3, 5, 3, 5);
            PreviewHolder.Name = "PreviewHolder";
            PreviewHolder.Size = new System.Drawing.Size(40, 20);
            PreviewHolder.TabIndex = 3;
            // 
            // PreviewBack
            // 
            PreviewBack.BackColor = System.Drawing.Color.FromArgb(20, 20, 20);
            PreviewBack.Location = new System.Drawing.Point(70, 7);
            PreviewBack.Margin = new Padding(3, 5, 3, 5);
            PreviewBack.Name = "PreviewBack";
            PreviewBack.Size = new System.Drawing.Size(40, 20);
            PreviewBack.TabIndex = 2;
            // 
            // PreviewNotes
            // 
            PreviewNotes.BackColor = System.Drawing.SystemColors.Window;
            PreviewNotes.Location = new System.Drawing.Point(63, 5);
            PreviewNotes.Margin = new Padding(3, 5, 3, 5);
            PreviewNotes.Name = "PreviewNotes";
            PreviewNotes.ReadOnly = true;
            PreviewNotes.Size = new System.Drawing.Size(38, 16);
            PreviewNotes.TabIndex = 4;
            PreviewNotes.Text = "";
            // 
            // panelPreviewSessionNotes2
            // 
            panelPreviewSessionNotes2.BackColor = System.Drawing.SystemColors.Window;
            panelPreviewSessionNotes2.Controls.Add(PreviewNotes);
            panelPreviewSessionNotes2.Location = new System.Drawing.Point(123, 7);
            panelPreviewSessionNotes2.Margin = new Padding(3, 5, 3, 5);
            panelPreviewSessionNotes2.Name = "panelPreviewSessionNotes2";
            panelPreviewSessionNotes2.Size = new System.Drawing.Size(142, 25);
            panelPreviewSessionNotes2.TabIndex = 5;
            // 
            // panelPreviewBottom
            // 
            panelPreviewBottom.BackColor = System.Drawing.Color.Gray;
            panelPreviewBottom.Controls.Add(panelPreviewSessionNotes2);
            panelPreviewBottom.Controls.Add(PreviewHolder);
            panelPreviewBottom.Controls.Add(PreviewBack);
            panelPreviewBottom.Dock = DockStyle.Fill;
            panelPreviewBottom.Location = new System.Drawing.Point(0, 34);
            panelPreviewBottom.Margin = new Padding(3, 5, 3, 5);
            panelPreviewBottom.Name = "panelPreviewBottom";
            panelPreviewBottom.Size = new System.Drawing.Size(368, 42);
            panelPreviewBottom.TabIndex = 2;
            panelPreviewBottom.Resize += panelPreviewBottom_Resize;
            // 
            // panel9
            // 
            panel9.Controls.Add(btnToLive);
            panel9.Controls.Add(btnToOutputMoveNext);
            panel9.Controls.Add(PreviewPanelDisplayName);
            panel9.Controls.Add(btnToOutput);
            panel9.Dock = DockStyle.Top;
            panel9.Location = new System.Drawing.Point(0, 0);
            panel9.Margin = new Padding(3, 5, 3, 5);
            panel9.Name = "panel9";
            panel9.Size = new System.Drawing.Size(368, 33);
            panel9.TabIndex = 0;
            // 
            // btnToLive
            // 
            btnToLive.Dock = DockStyle.Right;
            btnToLive.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold);
            btnToLive.ImageIndex = 38;
            btnToLive.ImageList = imageListSys;
            btnToLive.Location = new System.Drawing.Point(228, 0);
            btnToLive.Margin = new Padding(3, 5, 3, 5);
            btnToLive.Name = "btnToLive";
            btnToLive.Size = new System.Drawing.Size(56, 33);
            btnToLive.TabIndex = 8;
            toolTip1.SetToolTip(btnToLive, "Copy to Output and Start Show");
            btnToLive.Click += btnToLive_Click;
            // 
            // btnToOutputMoveNext
            // 
            btnToOutputMoveNext.BackgroundImageLayout = ImageLayout.Stretch;
            btnToOutputMoveNext.Dock = DockStyle.Right;
            btnToOutputMoveNext.ImageIndex = 39;
            btnToOutputMoveNext.ImageList = imageListSys;
            btnToOutputMoveNext.Location = new System.Drawing.Point(284, 0);
            btnToOutputMoveNext.Margin = new Padding(3, 5, 3, 5);
            btnToOutputMoveNext.Name = "btnToOutputMoveNext";
            btnToOutputMoveNext.Size = new System.Drawing.Size(42, 33);
            btnToOutputMoveNext.TabIndex = 9;
            toolTip1.SetToolTip(btnToOutputMoveNext, "Copy to Output and Preview Next Worship List Item");
            btnToOutputMoveNext.Click += btnToOutputMoveNext_Click;
            // 
            // PreviewPanelDisplayName
            // 
            PreviewPanelDisplayName.Columns.AddRange(new ColumnHeader[] { columnHeader15 });
            PreviewPanelDisplayName.Dock = DockStyle.Fill;
            PreviewPanelDisplayName.HeaderStyle = ColumnHeaderStyle.None;
            PreviewPanelDisplayName.LabelWrap = false;
            PreviewPanelDisplayName.Location = new System.Drawing.Point(0, 0);
            PreviewPanelDisplayName.Margin = new Padding(3, 5, 3, 5);
            PreviewPanelDisplayName.MultiSelect = false;
            PreviewPanelDisplayName.Name = "PreviewPanelDisplayName";
            PreviewPanelDisplayName.Scrollable = false;
            PreviewPanelDisplayName.ShowItemToolTips = true;
            PreviewPanelDisplayName.Size = new System.Drawing.Size(326, 33);
            PreviewPanelDisplayName.SmallImageList = imageListSys;
            PreviewPanelDisplayName.TabIndex = 7;
            PreviewPanelDisplayName.TabStop = false;
            PreviewPanelDisplayName.UseCompatibleStateImageBehavior = false;
            PreviewPanelDisplayName.View = View.Details;
            // 
            // btnToOutput
            // 
            btnToOutput.Dock = DockStyle.Right;
            btnToOutput.ImageIndex = 40;
            btnToOutput.ImageList = imageListSys;
            btnToOutput.Location = new System.Drawing.Point(326, 0);
            btnToOutput.Margin = new Padding(3, 5, 3, 5);
            btnToOutput.Name = "btnToOutput";
            btnToOutput.Size = new System.Drawing.Size(42, 33);
            btnToOutput.TabIndex = 6;
            toolTip1.SetToolTip(btnToOutput, "Copy to Output");
            btnToOutput.Click += btnToOutput_Click;
            // 
            // Ind_HeadAllTitles
            // 
            Ind_HeadAllTitles.Image = (System.Drawing.Image)resources.GetObject("Ind_HeadAllTitles.Image");
            Ind_HeadAllTitles.Name = "Ind_HeadAllTitles";
            Ind_HeadAllTitles.Size = new System.Drawing.Size(281, 26);
            Ind_HeadAllTitles.Tag = "1";
            Ind_HeadAllTitles.Text = "Show All Headings";
            // 
            // Ind_Region
            // 
            Ind_Region.DisplayStyle = ToolStripItemDisplayStyle.Image;
            Ind_Region.DropDownItems.AddRange(new ToolStripItem[] { Ind_ShowRegion1, Ind_ShowRegion2, Ind_ShowRegionBoth });
            Ind_Region.Image = (System.Drawing.Image)resources.GetObject("Ind_Region.Image");
            Ind_Region.ImageTransparentColor = System.Drawing.Color.Magenta;
            Ind_Region.Name = "Ind_Region";
            Ind_Region.Size = new System.Drawing.Size(38, 36);
            Ind_Region.Tag = "2";
            Ind_Region.ToolTipText = "Show Region Text";
            Ind_Region.DropDownItemClicked += Ind_Region_DropDownItemClicked;
            // 
            // Ind_ShowRegion1
            // 
            Ind_ShowRegion1.Image = (System.Drawing.Image)resources.GetObject("Ind_ShowRegion1.Image");
            Ind_ShowRegion1.Name = "Ind_ShowRegion1";
            Ind_ShowRegion1.Size = new System.Drawing.Size(185, 26);
            Ind_ShowRegion1.Tag = "0";
            Ind_ShowRegion1.Text = "Region 1 Only";
            // 
            // Ind_ShowRegion2
            // 
            Ind_ShowRegion2.Image = (System.Drawing.Image)resources.GetObject("Ind_ShowRegion2.Image");
            Ind_ShowRegion2.Name = "Ind_ShowRegion2";
            Ind_ShowRegion2.Size = new System.Drawing.Size(185, 26);
            Ind_ShowRegion2.Tag = "1";
            Ind_ShowRegion2.Text = "Region 2 Only";
            // 
            // Ind_ShowRegionBoth
            // 
            Ind_ShowRegionBoth.Image = (System.Drawing.Image)resources.GetObject("Ind_ShowRegionBoth.Image");
            Ind_ShowRegionBoth.Name = "Ind_ShowRegionBoth";
            Ind_ShowRegionBoth.Size = new System.Drawing.Size(185, 26);
            Ind_ShowRegionBoth.Tag = "2";
            Ind_ShowRegionBoth.Text = "Regions 1 && 2";
            // 
            // Ind_VAlign
            // 
            Ind_VAlign.DisplayStyle = ToolStripItemDisplayStyle.Image;
            Ind_VAlign.DropDownItems.AddRange(new ToolStripItem[] { Ind_VAlignTop, Ind_VAlignCentre, Ind_VAlignBottom });
            Ind_VAlign.Image = (System.Drawing.Image)resources.GetObject("Ind_VAlign.Image");
            Ind_VAlign.ImageTransparentColor = System.Drawing.Color.Magenta;
            Ind_VAlign.Name = "Ind_VAlign";
            Ind_VAlign.Size = new System.Drawing.Size(38, 36);
            Ind_VAlign.Tag = "1";
            Ind_VAlign.ToolTipText = "Vertical Alignment";
            Ind_VAlign.DropDownItemClicked += Ind_VAlign_DropDownItemClicked;
            // 
            // Ind_VAlignTop
            // 
            Ind_VAlignTop.Image = (System.Drawing.Image)resources.GetObject("Ind_VAlignTop.Image");
            Ind_VAlignTop.Name = "Ind_VAlignTop";
            Ind_VAlignTop.Size = new System.Drawing.Size(181, 26);
            Ind_VAlignTop.Tag = "0";
            Ind_VAlignTop.Text = "Align Top";
            // 
            // Ind_VAlignCentre
            // 
            Ind_VAlignCentre.Image = (System.Drawing.Image)resources.GetObject("Ind_VAlignCentre.Image");
            Ind_VAlignCentre.Name = "Ind_VAlignCentre";
            Ind_VAlignCentre.Size = new System.Drawing.Size(181, 26);
            Ind_VAlignCentre.Tag = "1";
            Ind_VAlignCentre.Text = "Align Centre";
            // 
            // Ind_VAlignBottom
            // 
            Ind_VAlignBottom.Image = (System.Drawing.Image)resources.GetObject("Ind_VAlignBottom.Image");
            Ind_VAlignBottom.Name = "Ind_VAlignBottom";
            Ind_VAlignBottom.Size = new System.Drawing.Size(181, 26);
            Ind_VAlignBottom.Tag = "2";
            Ind_VAlignBottom.Text = "Align Bottom";
            // 
            // flowLayoutPreviewLyrics
            // 
            flowLayoutPreviewLyrics.AutoScroll = true;
            flowLayoutPreviewLyrics.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            flowLayoutPreviewLyrics.BackColor = System.Drawing.SystemColors.Window;
            flowLayoutPreviewLyrics.Location = new System.Drawing.Point(3, 29);
            flowLayoutPreviewLyrics.Margin = new Padding(3, 5, 3, 5);
            flowLayoutPreviewLyrics.Name = "flowLayoutPreviewLyrics";
            flowLayoutPreviewLyrics.Size = new System.Drawing.Size(83, 68);
            flowLayoutPreviewLyrics.TabIndex = 6;
            flowLayoutPreviewLyrics.TabStop = true;
            flowLayoutPreviewLyrics.Click += flowLayoutPreviewLyrics_Click;
            flowLayoutPreviewLyrics.KeyUp += flowLayoutPreviewLyrics_KeyUp;
            flowLayoutPreviewLyrics.PreviewKeyDown += flowLayoutPreviewLyrics_PreviewKeyDown;
            flowLayoutPreviewLyrics.Leave += flowLayoutPreviewLyrics_Leave;
            // 
            // Ind_Outline
            // 
            Ind_Outline.CheckOnClick = true;
            Ind_Outline.DisplayStyle = ToolStripItemDisplayStyle.Image;
            Ind_Outline.Image = (System.Drawing.Image)resources.GetObject("Ind_Outline.Image");
            Ind_Outline.ImageTransparentColor = System.Drawing.Color.Magenta;
            Ind_Outline.Name = "Ind_Outline";
            Ind_Outline.Size = new System.Drawing.Size(29, 36);
            Ind_Outline.Tag = "add";
            Ind_Outline.ToolTipText = "Outline Font";
            Ind_Outline.MouseUp += Ind_Items_MouseUp;
            // 
            // Ind_Interlace
            // 
            Ind_Interlace.CheckOnClick = true;
            Ind_Interlace.DisplayStyle = ToolStripItemDisplayStyle.Image;
            Ind_Interlace.Image = (System.Drawing.Image)resources.GetObject("Ind_Interlace.Image");
            Ind_Interlace.ImageTransparentColor = System.Drawing.Color.Magenta;
            Ind_Interlace.Name = "Ind_Interlace";
            Ind_Interlace.Size = new System.Drawing.Size(29, 36);
            Ind_Interlace.ToolTipText = "Interlace Region1/Region2";
            Ind_Interlace.MouseUp += Ind_Items_MouseUp;
            // 
            // Ind_Notations
            // 
            Ind_Notations.CheckOnClick = true;
            Ind_Notations.DisplayStyle = ToolStripItemDisplayStyle.Image;
            Ind_Notations.Image = (System.Drawing.Image)resources.GetObject("Ind_Notations.Image");
            Ind_Notations.ImageTransparentColor = System.Drawing.Color.Magenta;
            Ind_Notations.Name = "Ind_Notations";
            Ind_Notations.Size = new System.Drawing.Size(29, 36);
            Ind_Notations.ToolTipText = "Show Notations";
            Ind_Notations.MouseUp += Ind_Items_MouseUp;
            // 
            // Ind_checkBox
            // 
            Ind_checkBox.AutoSize = true;
            Ind_checkBox.BackColor = System.Drawing.Color.Transparent;
            Ind_checkBox.Location = new System.Drawing.Point(8, 5);
            Ind_checkBox.Margin = new Padding(3, 5, 3, 5);
            Ind_checkBox.Name = "Ind_checkBox";
            Ind_checkBox.Size = new System.Drawing.Size(181, 24);
            Ind_checkBox.TabIndex = 0;
            Ind_checkBox.Text = "Use Individual Settings";
            Ind_checkBox.UseVisualStyleBackColor = false;
            Ind_checkBox.Click += Ind_checkBox_Click;
            // 
            // Ind_Shadow
            // 
            Ind_Shadow.CheckOnClick = true;
            Ind_Shadow.DisplayStyle = ToolStripItemDisplayStyle.Image;
            Ind_Shadow.Image = (System.Drawing.Image)resources.GetObject("Ind_Shadow.Image");
            Ind_Shadow.ImageTransparentColor = System.Drawing.Color.Magenta;
            Ind_Shadow.Name = "Ind_Shadow";
            Ind_Shadow.Size = new System.Drawing.Size(29, 36);
            Ind_Shadow.Tag = "open";
            Ind_Shadow.ToolTipText = "Shadow Font";
            Ind_Shadow.MouseUp += Ind_Items_MouseUp;
            // 
            // PreviewInfo
            // 
            PreviewInfo.BackColor = System.Drawing.SystemColors.Window;
            PreviewInfo.Location = new System.Drawing.Point(3, 81);
            PreviewInfo.Margin = new Padding(3, 5, 3, 5);
            PreviewInfo.Name = "PreviewInfo";
            PreviewInfo.ReadOnly = true;
            PreviewInfo.Size = new System.Drawing.Size(38, 63);
            PreviewInfo.TabIndex = 4;
            PreviewInfo.Text = "";
            PreviewInfo.KeyUp += PreviewInfo_KeyUp;
            PreviewInfo.Enter += FormControl_Enter;
            PreviewInfo.Leave += FormControl_Leave;
            // 
            // panelOutputTop
            // 
            panelOutputTop.Controls.Add(flowLayoutOutputPowerPoint);
            panelOutputTop.Controls.Add(flowLayoutOutputLyrics);
            panelOutputTop.Controls.Add(OutputInfo);
            panelOutputTop.Dock = DockStyle.Fill;
            panelOutputTop.Location = new System.Drawing.Point(0, 33);
            panelOutputTop.Margin = new Padding(3, 5, 3, 5);
            panelOutputTop.Name = "panelOutputTop";
            panelOutputTop.Size = new System.Drawing.Size(249, 446);
            panelOutputTop.TabIndex = 0;
            panelOutputTop.Resize += panelOutputTop_Resize;
            // 
            // flowLayoutOutputPowerPoint
            // 
            flowLayoutOutputPowerPoint.AutoScroll = true;
            flowLayoutOutputPowerPoint.Location = new System.Drawing.Point(15, 220);
            flowLayoutOutputPowerPoint.Margin = new Padding(3, 5, 3, 5);
            flowLayoutOutputPowerPoint.Name = "flowLayoutOutputPowerPoint";
            flowLayoutOutputPowerPoint.Size = new System.Drawing.Size(79, 53);
            flowLayoutOutputPowerPoint.TabIndex = 6;
            flowLayoutOutputPowerPoint.KeyUp += new KeyEventHandler(flowLayoutOutputPowerPoint_KeyUp);
            // 
            // flowLayoutOutputLyrics
            // 
            flowLayoutOutputLyrics.AutoScroll = true;
            flowLayoutOutputLyrics.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            flowLayoutOutputLyrics.BackColor = System.Drawing.SystemColors.Window;
            flowLayoutOutputLyrics.Location = new System.Drawing.Point(15, 33);
            flowLayoutOutputLyrics.Margin = new Padding(3, 5, 3, 5);
            flowLayoutOutputLyrics.Name = "flowLayoutOutputLyrics";
            flowLayoutOutputLyrics.Size = new System.Drawing.Size(83, 68);
            flowLayoutOutputLyrics.TabIndex = 7;
            flowLayoutOutputLyrics.Click += flowLayoutOutputLyrics_Click;
            flowLayoutOutputLyrics.Leave += flowLayoutOutputLyrics_Leave;
            // 
            // OutputInfo
            // 
            OutputInfo.Location = new System.Drawing.Point(15, 11);
            OutputInfo.Margin = new Padding(3, 5, 3, 5);
            OutputInfo.Name = "OutputInfo";
            OutputInfo.Size = new System.Drawing.Size(20, 27);
            OutputInfo.TabIndex = 9;
            OutputInfo.KeyUp += OutputInfo_KeyUp;
            // 
            // panel10
            // 
            panel10.Controls.Add(cbOutputBlack);
            panel10.Controls.Add(cbOutputClear);
            panel10.Controls.Add(OutputPanelDisplayName);
            panel10.Controls.Add(cbGoLive);
            panel10.Dock = DockStyle.Top;
            panel10.Location = new System.Drawing.Point(0, 0);
            panel10.Margin = new Padding(3, 5, 3, 5);
            panel10.Name = "panel10";
            panel10.Size = new System.Drawing.Size(249, 33);
            panel10.TabIndex = 1;
            // 
            // cbOutputBlack
            // 
            cbOutputBlack.Appearance = Appearance.Button;
            cbOutputBlack.Dock = DockStyle.Right;
            cbOutputBlack.ImageIndex = 15;
            cbOutputBlack.ImageList = imageListSys;
            cbOutputBlack.Location = new System.Drawing.Point(113, 0);
            cbOutputBlack.Margin = new Padding(3, 5, 3, 5);
            cbOutputBlack.Name = "cbOutputBlack";
            cbOutputBlack.Size = new System.Drawing.Size(40, 33);
            cbOutputBlack.TabIndex = 2;
            cbOutputBlack.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            toolTip1.SetToolTip(cbOutputBlack, "Black Screen");
            cbOutputBlack.Click += cbOutputBlack_Click;
            // 
            // cbOutputClear
            // 
            cbOutputClear.Appearance = Appearance.Button;
            cbOutputClear.Dock = DockStyle.Right;
            cbOutputClear.ImageIndex = 17;
            cbOutputClear.ImageList = imageListSys;
            cbOutputClear.Location = new System.Drawing.Point(153, 0);
            cbOutputClear.Margin = new Padding(3, 5, 3, 5);
            cbOutputClear.Name = "cbOutputClear";
            cbOutputClear.Size = new System.Drawing.Size(40, 33);
            cbOutputClear.TabIndex = 1;
            cbOutputClear.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            toolTip1.SetToolTip(cbOutputClear, "Hide Text");
            cbOutputClear.Click += cbOutputClear_Click;
            // 
            // OutputPanelDisplayName
            // 
            OutputPanelDisplayName.Columns.AddRange(new ColumnHeader[] { columnHeader16 });
            OutputPanelDisplayName.Dock = DockStyle.Fill;
            OutputPanelDisplayName.HeaderStyle = ColumnHeaderStyle.None;
            OutputPanelDisplayName.LabelWrap = false;
            OutputPanelDisplayName.Location = new System.Drawing.Point(0, 0);
            OutputPanelDisplayName.Margin = new Padding(3, 5, 3, 5);
            OutputPanelDisplayName.MultiSelect = false;
            OutputPanelDisplayName.Name = "OutputPanelDisplayName";
            OutputPanelDisplayName.Scrollable = false;
            OutputPanelDisplayName.ShowItemToolTips = true;
            OutputPanelDisplayName.Size = new System.Drawing.Size(193, 33);
            OutputPanelDisplayName.SmallImageList = imageListSys;
            OutputPanelDisplayName.TabIndex = 8;
            OutputPanelDisplayName.TabStop = false;
            OutputPanelDisplayName.UseCompatibleStateImageBehavior = false;
            OutputPanelDisplayName.View = View.Details;
            // 
            // cbGoLive
            // 
            cbGoLive.Appearance = Appearance.Button;
            cbGoLive.Dock = DockStyle.Right;
            cbGoLive.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold);
            cbGoLive.ImageIndex = 38;
            cbGoLive.ImageList = imageListSys;
            cbGoLive.Location = new System.Drawing.Point(193, 0);
            cbGoLive.Margin = new Padding(3, 5, 3, 5);
            cbGoLive.Name = "cbGoLive";
            cbGoLive.Size = new System.Drawing.Size(56, 33);
            cbGoLive.TabIndex = 3;
            cbGoLive.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            toolTip1.SetToolTip(cbGoLive, "Start Show");
            cbGoLive.Click += cbGoLive_Click;
            // 
            // panelOutputBottom
            // 
            panelOutputBottom.BackColor = System.Drawing.Color.Gray;
            panelOutputBottom.Controls.Add(panelOutputLM1);
            panelOutputBottom.Controls.Add(OutputHolder);
            panelOutputBottom.Controls.Add(OutputBack);
            panelOutputBottom.Dock = DockStyle.Fill;
            panelOutputBottom.Location = new System.Drawing.Point(0, 61);
            panelOutputBottom.Margin = new Padding(3, 5, 3, 5);
            panelOutputBottom.Name = "panelOutputBottom";
            panelOutputBottom.Size = new System.Drawing.Size(249, 109);
            panelOutputBottom.TabIndex = 3;
            panelOutputBottom.Resize += panelOutputBottom_Resize;
            // 
            // panelOutputLM1
            // 
            panelOutputLM1.BorderStyle = BorderStyle.Fixed3D;
            panelOutputLM1.Controls.Add(OutputTextBoxLM);
            panelOutputLM1.Controls.Add(panelOutputLM2);
            panelOutputLM1.Controls.Add(panelOutputLM3);
            panelOutputLM1.Dock = DockStyle.Bottom;
            panelOutputLM1.Location = new System.Drawing.Point(0, 76);
            panelOutputLM1.Margin = new Padding(3, 5, 3, 5);
            panelOutputLM1.Name = "panelOutputLM1";
            panelOutputLM1.Size = new System.Drawing.Size(249, 33);
            panelOutputLM1.TabIndex = 7;
            // 
            // OutputTextBoxLM
            // 
            OutputTextBoxLM.Dock = DockStyle.Fill;
            OutputTextBoxLM.Location = new System.Drawing.Point(0, 3);
            OutputTextBoxLM.Margin = new Padding(3, 5, 3, 5);
            OutputTextBoxLM.Name = "OutputTextBoxLM";
            OutputTextBoxLM.Size = new System.Drawing.Size(183, 27);
            OutputTextBoxLM.TabIndex = 15;
            OutputTextBoxLM.WordWrap = false;
            OutputTextBoxLM.KeyUp += OutputTextBoxLM_KeyUp;
            // 
            // panelOutputLM2
            // 
            panelOutputLM2.Dock = DockStyle.Top;
            panelOutputLM2.Location = new System.Drawing.Point(0, 0);
            panelOutputLM2.Margin = new Padding(3, 5, 3, 5);
            panelOutputLM2.Name = "panelOutputLM2";
            panelOutputLM2.Size = new System.Drawing.Size(183, 3);
            panelOutputLM2.TabIndex = 14;
            // 
            // panelOutputLM3
            // 
            panelOutputLM3.Controls.Add(OutputBtnLMSend);
            panelOutputLM3.Controls.Add(OutputBtnLMClear);
            panelOutputLM3.Dock = DockStyle.Right;
            panelOutputLM3.Location = new System.Drawing.Point(183, 0);
            panelOutputLM3.Margin = new Padding(3, 5, 3, 5);
            panelOutputLM3.Name = "panelOutputLM3";
            panelOutputLM3.Size = new System.Drawing.Size(62, 29);
            panelOutputLM3.TabIndex = 13;
            // 
            // OutputBtnLMSend
            // 
            OutputBtnLMSend.BackColor = System.Drawing.SystemColors.ButtonFace;
            OutputBtnLMSend.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold);
            OutputBtnLMSend.Image = (System.Drawing.Image)resources.GetObject("OutputBtnLMSend.Image");
            OutputBtnLMSend.Location = new System.Drawing.Point(1, -1);
            OutputBtnLMSend.Margin = new Padding(3, 5, 3, 5);
            OutputBtnLMSend.Name = "OutputBtnLMSend";
            OutputBtnLMSend.Size = new System.Drawing.Size(31, 33);
            OutputBtnLMSend.TabIndex = 9;
            toolTip1.SetToolTip(OutputBtnLMSend, "Send Message to Lyrics Monitor");
            OutputBtnLMSend.UseVisualStyleBackColor = false;
            OutputBtnLMSend.Click += OutputBtnLMSend_Click;
            // 
            // OutputBtnLMClear
            // 
            OutputBtnLMClear.BackColor = System.Drawing.SystemColors.ButtonFace;
            OutputBtnLMClear.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold);
            OutputBtnLMClear.Image = (System.Drawing.Image)resources.GetObject("OutputBtnLMClear.Image");
            OutputBtnLMClear.Location = new System.Drawing.Point(31, -1);
            OutputBtnLMClear.Margin = new Padding(3, 5, 3, 5);
            OutputBtnLMClear.Name = "OutputBtnLMClear";
            OutputBtnLMClear.Size = new System.Drawing.Size(31, 33);
            OutputBtnLMClear.TabIndex = 11;
            toolTip1.SetToolTip(OutputBtnLMClear, "Clear Lyrics Monitor Message");
            OutputBtnLMClear.UseVisualStyleBackColor = false;
            OutputBtnLMClear.Click += OutputBtnLMClear_Click;
            // 
            // OutputHolder
            // 
            OutputHolder.BackColor = System.Drawing.Color.FromArgb(20, 20, 20);
            OutputHolder.Location = new System.Drawing.Point(3, 59);
            OutputHolder.Margin = new Padding(3, 5, 3, 5);
            OutputHolder.Name = "OutputHolder";
            OutputHolder.Size = new System.Drawing.Size(40, 20);
            OutputHolder.TabIndex = 5;
            // 
            // OutputBack
            // 
            OutputBack.BackColor = System.Drawing.Color.FromArgb(20, 20, 20);
            OutputBack.Location = new System.Drawing.Point(64, 59);
            OutputBack.Margin = new Padding(3, 5, 3, 5);
            OutputBack.Name = "OutputBack";
            OutputBack.Size = new System.Drawing.Size(40, 20);
            OutputBack.TabIndex = 4;
            // 
            // Ind_R2Italics
            // 
            Ind_R2Italics.DisplayStyle = ToolStripItemDisplayStyle.Image;
            Ind_R2Italics.DropDownItems.AddRange(new ToolStripItem[] { Ind_R2Italics0, Ind_R2Italics1, Ind_R2Italics2 });
            Ind_R2Italics.Image = (System.Drawing.Image)resources.GetObject("Ind_R2Italics.Image");
            Ind_R2Italics.ImageTransparentColor = System.Drawing.Color.Magenta;
            Ind_R2Italics.Name = "Ind_R2Italics";
            Ind_R2Italics.Size = new System.Drawing.Size(38, 36);
            Ind_R2Italics.Tag = "0";
            Ind_R2Italics.Text = "toolStripDropDownButton1";
            Ind_R2Italics.DropDownItemClicked += Ind_R2Italics_DropDownItemClicked;
            // 
            // Ind_R2Italics0
            // 
            Ind_R2Italics0.Image = (System.Drawing.Image)resources.GetObject("Ind_R2Italics0.Image");
            Ind_R2Italics0.Name = "Ind_R2Italics0";
            Ind_R2Italics0.Size = new System.Drawing.Size(213, 26);
            Ind_R2Italics0.Tag = "0";
            Ind_R2Italics0.Text = "No Italics";
            // 
            // Ind_R2Italics1
            // 
            Ind_R2Italics1.Image = (System.Drawing.Image)resources.GetObject("Ind_R2Italics1.Image");
            Ind_R2Italics1.Name = "Ind_R2Italics1";
            Ind_R2Italics1.Size = new System.Drawing.Size(213, 26);
            Ind_R2Italics1.Tag = "1";
            Ind_R2Italics1.Text = "Italics";
            // 
            // Ind_R2Italics2
            // 
            Ind_R2Italics2.Image = (System.Drawing.Image)resources.GetObject("Ind_R2Italics2.Image");
            Ind_R2Italics2.Name = "Ind_R2Italics2";
            Ind_R2Italics2.Size = new System.Drawing.Size(213, 26);
            Ind_R2Italics2.Tag = "2";
            Ind_R2Italics2.Text = "Chorus Italics Only";
            // 
            // splitContainerOutput
            // 
            splitContainerOutput.Dock = DockStyle.Fill;
            splitContainerOutput.Location = new System.Drawing.Point(0, 0);
            splitContainerOutput.Margin = new Padding(3, 5, 3, 5);
            splitContainerOutput.Name = "splitContainerOutput";
            splitContainerOutput.Orientation = Orientation.Horizontal;
            // 
            // splitContainerOutput.Panel1
            // 
            splitContainerOutput.Panel1.Controls.Add(panelOutputTop);
            splitContainerOutput.Panel1.Controls.Add(panel10);
            splitContainerOutput.Panel1MinSize = 50;
            // 
            // splitContainerOutput.Panel2
            // 
            splitContainerOutput.Panel2.BackColor = System.Drawing.SystemColors.Control;
            splitContainerOutput.Panel2.Controls.Add(panelOutputBottom);
            splitContainerOutput.Panel2.Controls.Add(panel8);
            splitContainerOutput.Panel2.Controls.Add(panel2);
            splitContainerOutput.Size = new System.Drawing.Size(249, 654);
            splitContainerOutput.SplitterDistance = 479;
            splitContainerOutput.SplitterWidth = 5;
            splitContainerOutput.TabIndex = 0;
            splitContainerOutput.Text = "splitContainer3";
            // 
            // panel8
            // 
            panel8.BackColor = System.Drawing.SystemColors.Control;
            panel8.Controls.Add(labelGapItem);
            panel8.Controls.Add(labelHideText);
            panel8.Controls.Add(labelBlackScreen);
            panel8.Dock = DockStyle.Top;
            panel8.Location = new System.Drawing.Point(0, 33);
            panel8.Margin = new Padding(3, 5, 3, 5);
            panel8.Name = "panel8";
            panel8.Size = new System.Drawing.Size(249, 28);
            panel8.TabIndex = 2;
            // 
            // labelGapItem
            // 
            labelGapItem.BackColor = System.Drawing.Color.FromArgb(192, 255, 192);
            labelGapItem.BorderStyle = BorderStyle.FixedSingle;
            labelGapItem.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold);
            labelGapItem.Location = new System.Drawing.Point(145, 3);
            labelGapItem.Name = "labelGapItem";
            labelGapItem.Size = new System.Drawing.Size(88, 23);
            labelGapItem.TabIndex = 6;
            labelGapItem.Text = "Gap Item";
            labelGapItem.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            labelGapItem.Visible = false;
            // 
            // labelHideText
            // 
            labelHideText.BackColor = System.Drawing.Color.PowderBlue;
            labelHideText.BorderStyle = BorderStyle.FixedSingle;
            labelHideText.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold);
            labelHideText.Location = new System.Drawing.Point(83, 3);
            labelHideText.Name = "labelHideText";
            labelHideText.Size = new System.Drawing.Size(88, 23);
            labelHideText.TabIndex = 1;
            labelHideText.Text = "Hide Text";
            labelHideText.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            labelHideText.Visible = false;
            // 
            // labelBlackScreen
            // 
            labelBlackScreen.BackColor = System.Drawing.Color.White;
            labelBlackScreen.BorderStyle = BorderStyle.FixedSingle;
            labelBlackScreen.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold);
            labelBlackScreen.ForeColor = System.Drawing.Color.Black;
            labelBlackScreen.Location = new System.Drawing.Point(0, 3);
            labelBlackScreen.Name = "labelBlackScreen";
            labelBlackScreen.Size = new System.Drawing.Size(112, 23);
            labelBlackScreen.TabIndex = 0;
            labelBlackScreen.Text = "Black Screen";
            labelBlackScreen.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            labelBlackScreen.Visible = false;
            // 
            // panel2
            // 
            panel2.BackColor = System.Drawing.SystemColors.Control;
            panel2.Controls.Add(flowLayoutPanel2);
            panel2.Controls.Add(panel6);
            panel2.Dock = DockStyle.Top;
            panel2.Location = new System.Drawing.Point(0, 0);
            panel2.Margin = new Padding(3, 5, 3, 5);
            panel2.Name = "panel2";
            panel2.Size = new System.Drawing.Size(249, 33);
            panel2.TabIndex = 1;
            // 
            // flowLayoutPanel2
            // 
            flowLayoutPanel2.Controls.Add(OutputBtnVerse1);
            flowLayoutPanel2.Controls.Add(OutputBtnVerse2);
            flowLayoutPanel2.Controls.Add(OutputBtnVerse3);
            flowLayoutPanel2.Controls.Add(OutputBtnVerse4);
            flowLayoutPanel2.Controls.Add(OutputBtnVerse5);
            flowLayoutPanel2.Controls.Add(OutputBtnVerse6);
            flowLayoutPanel2.Controls.Add(OutputBtnVerse7);
            flowLayoutPanel2.Controls.Add(OutputBtnVerse8);
            flowLayoutPanel2.Controls.Add(OutputBtnVerse9);
            flowLayoutPanel2.Controls.Add(OutputBtnVersePreChorus);
            flowLayoutPanel2.Controls.Add(OutputBtnVersePreChorus2);
            flowLayoutPanel2.Controls.Add(OutputBtnVerseChorus);
            flowLayoutPanel2.Controls.Add(OutputBtnVerseChorus2);
            flowLayoutPanel2.Controls.Add(OutputBtnVerseBridge);
            flowLayoutPanel2.Controls.Add(OutputBtnVerseBridge2);
            flowLayoutPanel2.Controls.Add(OutputBtnVerseEnding);
            flowLayoutPanel2.Dock = DockStyle.Left;
            flowLayoutPanel2.Location = new System.Drawing.Point(216, 0);
            flowLayoutPanel2.Margin = new Padding(0);
            flowLayoutPanel2.Name = "flowLayoutPanel2";
            flowLayoutPanel2.Size = new System.Drawing.Size(335, 33);
            flowLayoutPanel2.TabIndex = 8;
            // 
            // OutputBtnVerse1
            // 
            OutputBtnVerse1.FlatStyle = FlatStyle.Flat;
            OutputBtnVerse1.Location = new System.Drawing.Point(0, 0);
            OutputBtnVerse1.Margin = new Padding(0);
            OutputBtnVerse1.Name = "OutputBtnVerse1";
            OutputBtnVerse1.Size = new System.Drawing.Size(19, 33);
            OutputBtnVerse1.TabIndex = 20;
            OutputBtnVerse1.Tag = "1";
            OutputBtnVerse1.Text = "1";
            OutputBtnVerse1.Click += OutputBtnVerse_Click;
            // 
            // OutputBtnVerse2
            // 
            OutputBtnVerse2.FlatStyle = FlatStyle.Flat;
            OutputBtnVerse2.Location = new System.Drawing.Point(19, 0);
            OutputBtnVerse2.Margin = new Padding(0);
            OutputBtnVerse2.Name = "OutputBtnVerse2";
            OutputBtnVerse2.Size = new System.Drawing.Size(19, 33);
            OutputBtnVerse2.TabIndex = 21;
            OutputBtnVerse2.Tag = "2";
            OutputBtnVerse2.Text = "2";
            OutputBtnVerse2.Click += OutputBtnVerse_Click;
            // 
            // OutputBtnVerse3
            // 
            OutputBtnVerse3.FlatStyle = FlatStyle.Flat;
            OutputBtnVerse3.Location = new System.Drawing.Point(38, 0);
            OutputBtnVerse3.Margin = new Padding(0);
            OutputBtnVerse3.Name = "OutputBtnVerse3";
            OutputBtnVerse3.Size = new System.Drawing.Size(19, 33);
            OutputBtnVerse3.TabIndex = 22;
            OutputBtnVerse3.Tag = "3";
            OutputBtnVerse3.Text = "3";
            OutputBtnVerse3.Click += OutputBtnVerse_Click;
            // 
            // OutputBtnVerse4
            // 
            OutputBtnVerse4.FlatStyle = FlatStyle.Flat;
            OutputBtnVerse4.Location = new System.Drawing.Point(57, 0);
            OutputBtnVerse4.Margin = new Padding(0);
            OutputBtnVerse4.Name = "OutputBtnVerse4";
            OutputBtnVerse4.Size = new System.Drawing.Size(19, 33);
            OutputBtnVerse4.TabIndex = 23;
            OutputBtnVerse4.Tag = "4";
            OutputBtnVerse4.Text = "4";
            OutputBtnVerse4.Click += OutputBtnVerse_Click;
            // 
            // OutputBtnVerse5
            // 
            OutputBtnVerse5.FlatStyle = FlatStyle.Flat;
            OutputBtnVerse5.Location = new System.Drawing.Point(76, 0);
            OutputBtnVerse5.Margin = new Padding(0);
            OutputBtnVerse5.Name = "OutputBtnVerse5";
            OutputBtnVerse5.Size = new System.Drawing.Size(19, 33);
            OutputBtnVerse5.TabIndex = 24;
            OutputBtnVerse5.Tag = "5";
            OutputBtnVerse5.Text = "5";
            OutputBtnVerse5.Click += OutputBtnVerse_Click;
            // 
            // OutputBtnVerse6
            // 
            OutputBtnVerse6.FlatStyle = FlatStyle.Flat;
            OutputBtnVerse6.Location = new System.Drawing.Point(95, 0);
            OutputBtnVerse6.Margin = new Padding(0);
            OutputBtnVerse6.Name = "OutputBtnVerse6";
            OutputBtnVerse6.Size = new System.Drawing.Size(19, 33);
            OutputBtnVerse6.TabIndex = 25;
            OutputBtnVerse6.Tag = "6";
            OutputBtnVerse6.Text = "6";
            OutputBtnVerse6.Click += OutputBtnVerse_Click;
            // 
            // OutputBtnVerse7
            // 
            OutputBtnVerse7.FlatStyle = FlatStyle.Flat;
            OutputBtnVerse7.Location = new System.Drawing.Point(114, 0);
            OutputBtnVerse7.Margin = new Padding(0);
            OutputBtnVerse7.Name = "OutputBtnVerse7";
            OutputBtnVerse7.Size = new System.Drawing.Size(19, 33);
            OutputBtnVerse7.TabIndex = 26;
            OutputBtnVerse7.Tag = "7";
            OutputBtnVerse7.Text = "7";
            OutputBtnVerse7.Click += OutputBtnVerse_Click;
            // 
            // OutputBtnVerse8
            // 
            OutputBtnVerse8.FlatStyle = FlatStyle.Flat;
            OutputBtnVerse8.Location = new System.Drawing.Point(133, 0);
            OutputBtnVerse8.Margin = new Padding(0);
            OutputBtnVerse8.Name = "OutputBtnVerse8";
            OutputBtnVerse8.Size = new System.Drawing.Size(19, 33);
            OutputBtnVerse8.TabIndex = 27;
            OutputBtnVerse8.Tag = "8";
            OutputBtnVerse8.Text = "8";
            OutputBtnVerse8.Click += OutputBtnVerse_Click;
            // 
            // OutputBtnVerse9
            // 
            OutputBtnVerse9.FlatStyle = FlatStyle.Flat;
            OutputBtnVerse9.Location = new System.Drawing.Point(152, 0);
            OutputBtnVerse9.Margin = new Padding(0);
            OutputBtnVerse9.Name = "OutputBtnVerse9";
            OutputBtnVerse9.Size = new System.Drawing.Size(19, 33);
            OutputBtnVerse9.TabIndex = 28;
            OutputBtnVerse9.Tag = "9";
            OutputBtnVerse9.Text = "9";
            OutputBtnVerse9.Click += OutputBtnVerse_Click;
            // 
            // OutputBtnVersePreChorus
            // 
            OutputBtnVersePreChorus.FlatStyle = FlatStyle.Flat;
            OutputBtnVersePreChorus.Location = new System.Drawing.Point(171, 0);
            OutputBtnVersePreChorus.Margin = new Padding(0);
            OutputBtnVersePreChorus.Name = "OutputBtnVersePreChorus";
            OutputBtnVersePreChorus.Size = new System.Drawing.Size(19, 33);
            OutputBtnVersePreChorus.TabIndex = 33;
            OutputBtnVersePreChorus.Tag = "111";
            OutputBtnVersePreChorus.Text = "p";
            OutputBtnVersePreChorus.Click += OutputBtnVerse_Click;
            // 
            // OutputBtnVersePreChorus2
            // 
            OutputBtnVersePreChorus2.FlatStyle = FlatStyle.Flat;
            OutputBtnVersePreChorus2.Location = new System.Drawing.Point(190, 0);
            OutputBtnVersePreChorus2.Margin = new Padding(0);
            OutputBtnVersePreChorus2.Name = "OutputBtnVersePreChorus2";
            OutputBtnVersePreChorus2.Size = new System.Drawing.Size(19, 33);
            OutputBtnVersePreChorus2.TabIndex = 34;
            OutputBtnVersePreChorus2.Tag = "112";
            OutputBtnVersePreChorus2.Text = "q";
            OutputBtnVersePreChorus2.Click += OutputBtnVerse_Click;
            // 
            // OutputBtnVerseChorus
            // 
            OutputBtnVerseChorus.FlatStyle = FlatStyle.Flat;
            OutputBtnVerseChorus.Location = new System.Drawing.Point(209, 0);
            OutputBtnVerseChorus.Margin = new Padding(0);
            OutputBtnVerseChorus.Name = "OutputBtnVerseChorus";
            OutputBtnVerseChorus.Size = new System.Drawing.Size(19, 33);
            OutputBtnVerseChorus.TabIndex = 29;
            OutputBtnVerseChorus.Tag = "0";
            OutputBtnVerseChorus.Text = "c";
            OutputBtnVerseChorus.Click += OutputBtnVerse_Click;
            // 
            // OutputBtnVerseChorus2
            // 
            OutputBtnVerseChorus2.FlatStyle = FlatStyle.Flat;
            OutputBtnVerseChorus2.Location = new System.Drawing.Point(228, 0);
            OutputBtnVerseChorus2.Margin = new Padding(0);
            OutputBtnVerseChorus2.Name = "OutputBtnVerseChorus2";
            OutputBtnVerseChorus2.Size = new System.Drawing.Size(19, 33);
            OutputBtnVerseChorus2.TabIndex = 31;
            OutputBtnVerseChorus2.Tag = "102";
            OutputBtnVerseChorus2.Text = "t";
            OutputBtnVerseChorus2.Click += OutputBtnVerse_Click;
            // 
            // OutputBtnVerseBridge
            // 
            OutputBtnVerseBridge.FlatStyle = FlatStyle.Flat;
            OutputBtnVerseBridge.Location = new System.Drawing.Point(247, 0);
            OutputBtnVerseBridge.Margin = new Padding(0);
            OutputBtnVerseBridge.Name = "OutputBtnVerseBridge";
            OutputBtnVerseBridge.Size = new System.Drawing.Size(19, 33);
            OutputBtnVerseBridge.TabIndex = 30;
            OutputBtnVerseBridge.Tag = "100";
            OutputBtnVerseBridge.Text = "b";
            OutputBtnVerseBridge.Click += OutputBtnVerse_Click;
            // 
            // OutputBtnVerseBridge2
            // 
            OutputBtnVerseBridge2.FlatStyle = FlatStyle.Flat;
            OutputBtnVerseBridge2.Location = new System.Drawing.Point(266, 0);
            OutputBtnVerseBridge2.Margin = new Padding(0);
            OutputBtnVerseBridge2.Name = "OutputBtnVerseBridge2";
            OutputBtnVerseBridge2.Size = new System.Drawing.Size(23, 33);
            OutputBtnVerseBridge2.TabIndex = 35;
            OutputBtnVerseBridge2.Tag = "103";
            OutputBtnVerseBridge2.Text = "w";
            OutputBtnVerseBridge2.Click += OutputBtnVerse_Click;
            // 
            // OutputBtnVerseEnding
            // 
            OutputBtnVerseEnding.FlatStyle = FlatStyle.Flat;
            OutputBtnVerseEnding.Location = new System.Drawing.Point(289, 0);
            OutputBtnVerseEnding.Margin = new Padding(0);
            OutputBtnVerseEnding.Name = "OutputBtnVerseEnding";
            OutputBtnVerseEnding.Size = new System.Drawing.Size(19, 33);
            OutputBtnVerseEnding.TabIndex = 32;
            OutputBtnVerseEnding.Tag = "101";
            OutputBtnVerseEnding.Text = "e";
            OutputBtnVerseEnding.Click += OutputBtnVerse_Click;
            // 
            // panel6
            // 
            panel6.Controls.Add(OutputBtnSlideDown);
            panel6.Controls.Add(OutputBtnSlideUp);
            panel6.Controls.Add(OutputBtnItemDown);
            panel6.Controls.Add(OutputBtnItemUp);
            panel6.Controls.Add(OutputBtnRefAlert);
            panel6.Controls.Add(OutputBtnMedia);
            panel6.Controls.Add(OutputBtnJumpToNonRotate);
            panel6.Dock = DockStyle.Left;
            panel6.Location = new System.Drawing.Point(0, 0);
            panel6.Margin = new Padding(3, 5, 3, 5);
            panel6.Name = "panel6";
            panel6.Size = new System.Drawing.Size(216, 33);
            panel6.TabIndex = 1;
            // 
            // OutputBtnSlideDown
            // 
            OutputBtnSlideDown.Dock = DockStyle.Left;
            OutputBtnSlideDown.ImageIndex = 51;
            OutputBtnSlideDown.ImageList = imageListSys;
            OutputBtnSlideDown.Location = new System.Drawing.Point(183, 0);
            OutputBtnSlideDown.Margin = new Padding(3, 5, 3, 5);
            OutputBtnSlideDown.Name = "OutputBtnSlideDown";
            OutputBtnSlideDown.Size = new System.Drawing.Size(30, 33);
            OutputBtnSlideDown.TabIndex = 5;
            toolTip1.SetToolTip(OutputBtnSlideDown, "Next Slide");
            OutputBtnSlideDown.Click += OutputBtnUpDown_Click;
            // 
            // OutputBtnSlideUp
            // 
            OutputBtnSlideUp.Dock = DockStyle.Left;
            OutputBtnSlideUp.ImageIndex = 50;
            OutputBtnSlideUp.ImageList = imageListSys;
            OutputBtnSlideUp.Location = new System.Drawing.Point(153, 0);
            OutputBtnSlideUp.Margin = new Padding(3, 5, 3, 5);
            OutputBtnSlideUp.Name = "OutputBtnSlideUp";
            OutputBtnSlideUp.Size = new System.Drawing.Size(30, 33);
            OutputBtnSlideUp.TabIndex = 4;
            toolTip1.SetToolTip(OutputBtnSlideUp, "Previous Slide");
            OutputBtnSlideUp.Click += OutputBtnUpDown_Click;
            // 
            // OutputBtnItemDown
            // 
            OutputBtnItemDown.Dock = DockStyle.Left;
            OutputBtnItemDown.ImageIndex = 49;
            OutputBtnItemDown.ImageList = imageListSys;
            OutputBtnItemDown.Location = new System.Drawing.Point(123, 0);
            OutputBtnItemDown.Margin = new Padding(3, 5, 3, 5);
            OutputBtnItemDown.Name = "OutputBtnItemDown";
            OutputBtnItemDown.Size = new System.Drawing.Size(30, 33);
            OutputBtnItemDown.TabIndex = 3;
            toolTip1.SetToolTip(OutputBtnItemDown, "Next Item");
            OutputBtnItemDown.Click += OutputBtnUpDown_Click;
            // 
            // OutputBtnItemUp
            // 
            OutputBtnItemUp.Dock = DockStyle.Left;
            OutputBtnItemUp.ImageIndex = 48;
            OutputBtnItemUp.ImageList = imageListSys;
            OutputBtnItemUp.Location = new System.Drawing.Point(93, 0);
            OutputBtnItemUp.Margin = new Padding(3, 5, 3, 5);
            OutputBtnItemUp.Name = "OutputBtnItemUp";
            OutputBtnItemUp.Size = new System.Drawing.Size(30, 33);
            OutputBtnItemUp.TabIndex = 2;
            toolTip1.SetToolTip(OutputBtnItemUp, "Previous Item");
            OutputBtnItemUp.Click += OutputBtnUpDown_Click;
            // 
            // OutputBtnRefAlert
            // 
            OutputBtnRefAlert.Dock = DockStyle.Left;
            OutputBtnRefAlert.ImageIndex = 47;
            OutputBtnRefAlert.ImageList = imageListSys;
            OutputBtnRefAlert.Location = new System.Drawing.Point(62, 0);
            OutputBtnRefAlert.Margin = new Padding(3, 5, 3, 5);
            OutputBtnRefAlert.Name = "OutputBtnRefAlert";
            OutputBtnRefAlert.Size = new System.Drawing.Size(31, 33);
            OutputBtnRefAlert.TabIndex = 6;
            toolTip1.SetToolTip(OutputBtnRefAlert, "Show/Stop Reference Alert");
            OutputBtnRefAlert.Click += OutputBtnRefAlert_Click;
            // 
            // OutputBtnMedia
            // 
            OutputBtnMedia.Dock = DockStyle.Left;
            OutputBtnMedia.ImageIndex = 46;
            OutputBtnMedia.ImageList = imageListSys;
            OutputBtnMedia.Location = new System.Drawing.Point(31, 0);
            OutputBtnMedia.Margin = new Padding(3, 5, 3, 5);
            OutputBtnMedia.Name = "OutputBtnMedia";
            OutputBtnMedia.Size = new System.Drawing.Size(31, 33);
            OutputBtnMedia.TabIndex = 7;
            toolTip1.SetToolTip(OutputBtnMedia, "Media Pause/Resume");
            OutputBtnMedia.Click += OutputBtnMedia_Click;
            // 
            // OutputBtnJumpToNonRotate
            // 
            OutputBtnJumpToNonRotate.Dock = DockStyle.Left;
            OutputBtnJumpToNonRotate.Image = (System.Drawing.Image)resources.GetObject("OutputBtnJumpToNonRotate.Image");
            OutputBtnJumpToNonRotate.Location = new System.Drawing.Point(0, 0);
            OutputBtnJumpToNonRotate.Margin = new Padding(3, 5, 3, 5);
            OutputBtnJumpToNonRotate.Name = "OutputBtnJumpToNonRotate";
            OutputBtnJumpToNonRotate.Size = new System.Drawing.Size(31, 33);
            OutputBtnJumpToNonRotate.TabIndex = 8;
            toolTip1.SetToolTip(OutputBtnJumpToNonRotate, "Jump To Non-Rotating Item");
            OutputBtnJumpToNonRotate.Click += OutputBtnJumpToNonRotate_Click;
            // 
            // Ind_HeadNoTitles
            // 
            Ind_HeadNoTitles.Image = (System.Drawing.Image)resources.GetObject("Ind_HeadNoTitles.Image");
            Ind_HeadNoTitles.Name = "Ind_HeadNoTitles";
            Ind_HeadNoTitles.Size = new System.Drawing.Size(281, 26);
            Ind_HeadNoTitles.Tag = "0";
            Ind_HeadNoTitles.Text = "No Headings";
            // 
            // IndgroupBox2
            // 
            IndgroupBox2.Controls.Add(Ind_BottomUpDown);
            IndgroupBox2.Controls.Add(panelInd3);
            IndgroupBox2.Controls.Add(Ind_RightUpDown);
            IndgroupBox2.Controls.Add(panelInd2);
            IndgroupBox2.Controls.Add(label3);
            IndgroupBox2.Controls.Add(Ind_LeftUpDown);
            IndgroupBox2.Controls.Add(label2);
            IndgroupBox2.Controls.Add(label1);
            IndgroupBox2.Location = new System.Drawing.Point(8, 144);
            IndgroupBox2.Margin = new Padding(3, 5, 3, 5);
            IndgroupBox2.Name = "IndgroupBox2";
            IndgroupBox2.Padding = new Padding(3, 5, 3, 5);
            IndgroupBox2.Size = new System.Drawing.Size(320, 149);
            IndgroupBox2.TabIndex = 1;
            IndgroupBox2.TabStop = false;
            IndgroupBox2.Text = "Background";
            // 
            // Ind_BottomUpDown
            // 
            Ind_BottomUpDown.Location = new System.Drawing.Point(249, 109);
            Ind_BottomUpDown.Margin = new Padding(3, 5, 3, 5);
            Ind_BottomUpDown.Name = "Ind_BottomUpDown";
            Ind_BottomUpDown.Size = new System.Drawing.Size(59, 27);
            Ind_BottomUpDown.TabIndex = 2;
            Ind_BottomUpDown.MouseUp += Ind_MarginUpDown_MouseUp;
            // 
            // panelInd3
            // 
            panelInd3.Controls.Add(toolStripInd3);
            panelInd3.Location = new System.Drawing.Point(8, 67);
            panelInd3.Margin = new Padding(3, 5, 3, 5);
            panelInd3.Name = "panelInd3";
            panelInd3.Size = new System.Drawing.Size(304, 33);
            panelInd3.TabIndex = 11;
            // 
            // toolStripInd3
            // 
            toolStripInd3.AutoSize = false;
            toolStripInd3.CanOverflow = false;
            toolStripInd3.Dock = DockStyle.None;
            toolStripInd3.GripStyle = ToolStripGripStyle.Hidden;
            toolStripInd3.ImageScalingSize = new System.Drawing.Size(24, 24);
            toolStripInd3.Items.AddRange(new ToolStripItem[] { Ind_TransItem, Ind_TransSlides });
            toolStripInd3.LayoutStyle = ToolStripLayoutStyle.HorizontalStackWithOverflow;
            toolStripInd3.Location = new System.Drawing.Point(0, -1);
            toolStripInd3.Name = "toolStripInd3";
            toolStripInd3.Padding = new Padding(0, 0, 2, 0);
            toolStripInd3.RenderMode = ToolStripRenderMode.System;
            toolStripInd3.Size = new System.Drawing.Size(307, 39);
            toolStripInd3.TabIndex = 0;
            // 
            // Ind_TransItem
            // 
            Ind_TransItem.AutoSize = false;
            Ind_TransItem.AutoToolTip = true;
            Ind_TransItem.DropDownStyle = ComboBoxStyle.DropDownList;
            Ind_TransItem.MaxDropDownItems = 24;
            Ind_TransItem.Name = "Ind_TransItem";
            Ind_TransItem.Size = new System.Drawing.Size(147, 28);
            Ind_TransItem.ToolTipText = "Item Transition";
            Ind_TransItem.SelectedIndexChanged += Ind_TransSelectedIndexChanged;
            // 
            // Ind_TransSlides
            // 
            Ind_TransSlides.AutoSize = false;
            Ind_TransSlides.AutoToolTip = true;
            Ind_TransSlides.DropDownStyle = ComboBoxStyle.DropDownList;
            Ind_TransSlides.MaxDropDownItems = 24;
            Ind_TransSlides.Name = "Ind_TransSlides";
            Ind_TransSlides.Size = new System.Drawing.Size(147, 28);
            Ind_TransSlides.ToolTipText = "Slide Transition";
            Ind_TransSlides.SelectedIndexChanged += Ind_TransSelectedIndexChanged;
            // 
            // Ind_RightUpDown
            // 
            Ind_RightUpDown.Location = new System.Drawing.Point(138, 109);
            Ind_RightUpDown.Margin = new Padding(3, 5, 3, 5);
            Ind_RightUpDown.Maximum = new decimal(new int[] { 40, 0, 0, 0 });
            Ind_RightUpDown.Name = "Ind_RightUpDown";
            Ind_RightUpDown.Size = new System.Drawing.Size(55, 27);
            Ind_RightUpDown.TabIndex = 1;
            Ind_RightUpDown.MouseUp += Ind_MarginUpDown_MouseUp;
            // 
            // panelInd2
            // 
            panelInd2.Controls.Add(toolStripInd2);
            panelInd2.Location = new System.Drawing.Point(8, 28);
            panelInd2.Margin = new Padding(3, 5, 3, 5);
            panelInd2.Name = "panelInd2";
            panelInd2.Size = new System.Drawing.Size(304, 33);
            panelInd2.TabIndex = 10;
            // 
            // toolStripInd2
            // 
            toolStripInd2.AutoSize = false;
            toolStripInd2.CanOverflow = false;
            toolStripInd2.Dock = DockStyle.None;
            toolStripInd2.GripStyle = ToolStripGripStyle.Hidden;
            toolStripInd2.ImageScalingSize = new System.Drawing.Size(24, 24);
            toolStripInd2.Items.AddRange(new ToolStripItem[] { Ind_ImageMode, Ind_NoImage, Ind_BackColour, toolStripSeparator27, Ind_AssignMedia });
            toolStripInd2.LayoutStyle = ToolStripLayoutStyle.HorizontalStackWithOverflow;
            toolStripInd2.Location = new System.Drawing.Point(0, -1);
            toolStripInd2.Name = "toolStripInd2";
            toolStripInd2.Padding = new Padding(0, 0, 2, 0);
            toolStripInd2.RenderMode = ToolStripRenderMode.System;
            toolStripInd2.Size = new System.Drawing.Size(307, 39);
            toolStripInd2.TabIndex = 0;
            // 
            // Ind_ImageMode
            // 
            Ind_ImageMode.DisplayStyle = ToolStripItemDisplayStyle.Image;
            Ind_ImageMode.DropDownItems.AddRange(new ToolStripItem[] { Ind_ImageTile, Ind_ImageCentre, Ind_ImageBestFit });
            Ind_ImageMode.Image = (System.Drawing.Image)resources.GetObject("Ind_ImageMode.Image");
            Ind_ImageMode.ImageTransparentColor = System.Drawing.Color.Magenta;
            Ind_ImageMode.Name = "Ind_ImageMode";
            Ind_ImageMode.Size = new System.Drawing.Size(38, 36);
            Ind_ImageMode.Tag = "2";
            Ind_ImageMode.ToolTipText = "Background Picture Format";
            Ind_ImageMode.DropDownItemClicked += Ind_ImageMode_DropDownItemClicked;
            // 
            // Ind_ImageTile
            // 
            Ind_ImageTile.Image = (System.Drawing.Image)resources.GetObject("Ind_ImageTile.Image");
            Ind_ImageTile.Name = "Ind_ImageTile";
            Ind_ImageTile.Size = new System.Drawing.Size(186, 26);
            Ind_ImageTile.Tag = "0";
            Ind_ImageTile.Text = "Tile Image";
            // 
            // Ind_ImageCentre
            // 
            Ind_ImageCentre.Image = (System.Drawing.Image)resources.GetObject("Ind_ImageCentre.Image");
            Ind_ImageCentre.Name = "Ind_ImageCentre";
            Ind_ImageCentre.Size = new System.Drawing.Size(186, 26);
            Ind_ImageCentre.Tag = "1";
            Ind_ImageCentre.Text = "Centre Image";
            // 
            // Ind_ImageBestFit
            // 
            Ind_ImageBestFit.Image = (System.Drawing.Image)resources.GetObject("Ind_ImageBestFit.Image");
            Ind_ImageBestFit.Name = "Ind_ImageBestFit";
            Ind_ImageBestFit.Size = new System.Drawing.Size(186, 26);
            Ind_ImageBestFit.Tag = "2";
            Ind_ImageBestFit.Text = "Best Fit Image";
            // 
            // Ind_NoImage
            // 
            Ind_NoImage.DisplayStyle = ToolStripItemDisplayStyle.Image;
            Ind_NoImage.Image = (System.Drawing.Image)resources.GetObject("Ind_NoImage.Image");
            Ind_NoImage.ImageTransparentColor = System.Drawing.Color.Magenta;
            Ind_NoImage.Name = "Ind_NoImage";
            Ind_NoImage.Size = new System.Drawing.Size(29, 36);
            Ind_NoImage.MouseUp += Ind_Items_MouseUp;
            // 
            // Ind_BackColour
            // 
            Ind_BackColour.AutoSize = false;
            Ind_BackColour.BackColor = System.Drawing.SystemColors.ActiveBorder;
            Ind_BackColour.DisplayStyle = ToolStripItemDisplayStyle.Text;
            Ind_BackColour.ImageTransparentColor = System.Drawing.Color.Magenta;
            Ind_BackColour.Name = "Ind_BackColour";
            Ind_BackColour.Size = new System.Drawing.Size(59, 22);
            Ind_BackColour.Text = "Colours";
            Ind_BackColour.ToolTipText = "Background Colours and Patterns";
            Ind_BackColour.MouseUp += Ind_Items_MouseUp;
            // 
            // toolStripSeparator27
            // 
            toolStripSeparator27.Name = "toolStripSeparator27";
            toolStripSeparator27.Size = new System.Drawing.Size(6, 39);
            // 
            // Ind_AssignMedia
            // 
            Ind_AssignMedia.AutoSize = false;
            Ind_AssignMedia.DisplayStyle = ToolStripItemDisplayStyle.Text;
            Ind_AssignMedia.ImageTransparentColor = System.Drawing.Color.Magenta;
            Ind_AssignMedia.Name = "Ind_AssignMedia";
            Ind_AssignMedia.Size = new System.Drawing.Size(110, 22);
            Ind_AssignMedia.Text = "Media";
            Ind_AssignMedia.MouseUp += Ind_Items_MouseUp;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(198, 112);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(62, 20);
            label3.TabIndex = 15;
            label3.Text = "Bottom:";
            // 
            // Ind_LeftUpDown
            // 
            Ind_LeftUpDown.Location = new System.Drawing.Point(38, 109);
            Ind_LeftUpDown.Margin = new Padding(3, 5, 3, 5);
            Ind_LeftUpDown.Maximum = new decimal(new int[] { 40, 0, 0, 0 });
            Ind_LeftUpDown.Name = "Ind_LeftUpDown";
            Ind_LeftUpDown.Size = new System.Drawing.Size(55, 27);
            Ind_LeftUpDown.TabIndex = 10;
            Ind_LeftUpDown.MouseUp += Ind_MarginUpDown_MouseUp;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(97, 112);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(47, 20);
            label2.TabIndex = 14;
            label2.Text = "Right:";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(6, 112);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(37, 20);
            label1.TabIndex = 0;
            label1.Text = "Left:";
            // 
            // Ind_R1Colour
            // 
            Ind_R1Colour.AutoSize = false;
            Ind_R1Colour.BackColor = System.Drawing.SystemColors.ActiveBorder;
            Ind_R1Colour.DisplayStyle = ToolStripItemDisplayStyle.Text;
            Ind_R1Colour.Image = (System.Drawing.Image)resources.GetObject("Ind_R1Colour.Image");
            Ind_R1Colour.ImageTransparentColor = System.Drawing.Color.Magenta;
            Ind_R1Colour.Name = "Ind_R1Colour";
            Ind_R1Colour.Size = new System.Drawing.Size(44, 22);
            Ind_R1Colour.Text = "Colour";
            Ind_R1Colour.MouseUp += Ind_RegionsFormat_MouseUp;
            // 
            // Ind_R1AlignRight
            // 
            Ind_R1AlignRight.Image = (System.Drawing.Image)resources.GetObject("Ind_R1AlignRight.Image");
            Ind_R1AlignRight.Name = "Ind_R1AlignRight";
            Ind_R1AlignRight.Size = new System.Drawing.Size(174, 26);
            Ind_R1AlignRight.Tag = "3";
            Ind_R1AlignRight.Text = "Align Right";
            // 
            // Ind_R1AlignCentre
            // 
            Ind_R1AlignCentre.Image = (System.Drawing.Image)resources.GetObject("Ind_R1AlignCentre.Image");
            Ind_R1AlignCentre.Name = "Ind_R1AlignCentre";
            Ind_R1AlignCentre.Size = new System.Drawing.Size(174, 26);
            Ind_R1AlignCentre.Tag = "2";
            Ind_R1AlignCentre.Text = "Align Centre";
            // 
            // Ind_R1AlignLeft
            // 
            Ind_R1AlignLeft.Image = (System.Drawing.Image)resources.GetObject("Ind_R1AlignLeft.Image");
            Ind_R1AlignLeft.Name = "Ind_R1AlignLeft";
            Ind_R1AlignLeft.Size = new System.Drawing.Size(174, 26);
            Ind_R1AlignLeft.Tag = "1";
            Ind_R1AlignLeft.Text = "Align Left";
            // 
            // Ind_R1Align
            // 
            Ind_R1Align.DisplayStyle = ToolStripItemDisplayStyle.Image;
            Ind_R1Align.DropDownItems.AddRange(new ToolStripItem[] { Ind_R1AlignLeft, Ind_R1AlignCentre, Ind_R1AlignRight });
            Ind_R1Align.Image = (System.Drawing.Image)resources.GetObject("Ind_R1Align.Image");
            Ind_R1Align.ImageTransparentColor = System.Drawing.Color.Magenta;
            Ind_R1Align.Name = "Ind_R1Align";
            Ind_R1Align.Size = new System.Drawing.Size(38, 36);
            Ind_R1Align.Tag = "2";
            Ind_R1Align.DropDownItemClicked += Ind_R1Align_DropDownItemClicked;
            // 
            // toolStripSeparator13
            // 
            toolStripSeparator13.Name = "toolStripSeparator13";
            toolStripSeparator13.Size = new System.Drawing.Size(6, 39);
            // 
            // Ind_R1Underline
            // 
            Ind_R1Underline.CheckOnClick = true;
            Ind_R1Underline.DisplayStyle = ToolStripItemDisplayStyle.Image;
            Ind_R1Underline.Image = (System.Drawing.Image)resources.GetObject("Ind_R1Underline.Image");
            Ind_R1Underline.ImageTransparentColor = System.Drawing.Color.Magenta;
            Ind_R1Underline.Name = "Ind_R1Underline";
            Ind_R1Underline.Size = new System.Drawing.Size(29, 36);
            Ind_R1Underline.MouseUp += Ind_RegionsFormat_MouseUp;
            // 
            // Ind_R1Italics2
            // 
            Ind_R1Italics2.Image = (System.Drawing.Image)resources.GetObject("Ind_R1Italics2.Image");
            Ind_R1Italics2.Name = "Ind_R1Italics2";
            Ind_R1Italics2.Size = new System.Drawing.Size(213, 26);
            Ind_R1Italics2.Tag = "2";
            Ind_R1Italics2.Text = "Chorus Italics Only";
            // 
            // Ind_R1Italics1
            // 
            Ind_R1Italics1.Image = (System.Drawing.Image)resources.GetObject("Ind_R1Italics1.Image");
            Ind_R1Italics1.Name = "Ind_R1Italics1";
            Ind_R1Italics1.Size = new System.Drawing.Size(213, 26);
            Ind_R1Italics1.Tag = "1";
            Ind_R1Italics1.Text = "Italics";
            // 
            // Ind_R1Italics0
            // 
            Ind_R1Italics0.Image = (System.Drawing.Image)resources.GetObject("Ind_R1Italics0.Image");
            Ind_R1Italics0.Name = "Ind_R1Italics0";
            Ind_R1Italics0.Size = new System.Drawing.Size(213, 26);
            Ind_R1Italics0.Tag = "0";
            Ind_R1Italics0.Text = "No Italics";
            // 
            // Ind_R1Italics
            // 
            Ind_R1Italics.DisplayStyle = ToolStripItemDisplayStyle.Image;
            Ind_R1Italics.DropDownItems.AddRange(new ToolStripItem[] { Ind_R1Italics0, Ind_R1Italics1, Ind_R1Italics2 });
            Ind_R1Italics.Image = (System.Drawing.Image)resources.GetObject("Ind_R1Italics.Image");
            Ind_R1Italics.ImageTransparentColor = System.Drawing.Color.Magenta;
            Ind_R1Italics.Name = "Ind_R1Italics";
            Ind_R1Italics.Size = new System.Drawing.Size(38, 36);
            Ind_R1Italics.Tag = "0";
            Ind_R1Italics.DropDownItemClicked += Ind_R1Italics_DropDownItemClicked;
            // 
            // Ind_R1Bold
            // 
            Ind_R1Bold.CheckOnClick = true;
            Ind_R1Bold.DisplayStyle = ToolStripItemDisplayStyle.Image;
            Ind_R1Bold.Image = (System.Drawing.Image)resources.GetObject("Ind_R1Bold.Image");
            Ind_R1Bold.ImageTransparentColor = System.Drawing.Color.Magenta;
            Ind_R1Bold.Name = "Ind_R1Bold";
            Ind_R1Bold.Size = new System.Drawing.Size(29, 36);
            Ind_R1Bold.MouseUp += Ind_RegionsFormat_MouseUp;
            // 
            // panelInd5
            // 
            panelInd5.Controls.Add(toolStripInd5);
            panelInd5.Location = new System.Drawing.Point(9, 67);
            panelInd5.Margin = new Padding(3, 5, 3, 5);
            panelInd5.Name = "panelInd5";
            panelInd5.Size = new System.Drawing.Size(201, 33);
            panelInd5.TabIndex = 12;
            // 
            // toolStripInd5
            // 
            toolStripInd5.AutoSize = false;
            toolStripInd5.CanOverflow = false;
            toolStripInd5.Dock = DockStyle.None;
            toolStripInd5.GripStyle = ToolStripGripStyle.Hidden;
            toolStripInd5.ImageScalingSize = new System.Drawing.Size(24, 24);
            toolStripInd5.Items.AddRange(new ToolStripItem[] { Ind_Reg1FontsList });
            toolStripInd5.LayoutStyle = ToolStripLayoutStyle.HorizontalStackWithOverflow;
            toolStripInd5.Location = new System.Drawing.Point(0, -1);
            toolStripInd5.Name = "toolStripInd5";
            toolStripInd5.Padding = new Padding(0, 0, 2, 0);
            toolStripInd5.RenderMode = ToolStripRenderMode.System;
            toolStripInd5.Size = new System.Drawing.Size(207, 39);
            toolStripInd5.TabIndex = 5;
            // 
            // Ind_Reg1FontsList
            // 
            Ind_Reg1FontsList.AutoSize = false;
            Ind_Reg1FontsList.DropDownStyle = ComboBoxStyle.DropDownList;
            Ind_Reg1FontsList.Items.AddRange(new object[] { "No Media", "Show Media", "Hide Media" });
            Ind_Reg1FontsList.MaxDropDownItems = 12;
            Ind_Reg1FontsList.Name = "Ind_Reg1FontsList";
            Ind_Reg1FontsList.Size = new System.Drawing.Size(195, 28);
            Ind_Reg1FontsList.SelectedIndexChanged += Ind_FontsList_SelectedIndexChanged;
            // 
            // toolStripInd4
            // 
            toolStripInd4.AutoSize = false;
            toolStripInd4.CanOverflow = false;
            toolStripInd4.Dock = DockStyle.None;
            toolStripInd4.GripStyle = ToolStripGripStyle.Hidden;
            toolStripInd4.ImageScalingSize = new System.Drawing.Size(24, 24);
            toolStripInd4.Items.AddRange(new ToolStripItem[] { Ind_R1Bold, Ind_R1Italics, Ind_R1Underline, toolStripSeparator13, Ind_R1Align, Ind_R1Colour });
            toolStripInd4.LayoutStyle = ToolStripLayoutStyle.HorizontalStackWithOverflow;
            toolStripInd4.Location = new System.Drawing.Point(0, -1);
            toolStripInd4.Name = "toolStripInd4";
            toolStripInd4.Padding = new Padding(0, 0, 2, 0);
            toolStripInd4.RenderMode = ToolStripRenderMode.System;
            toolStripInd4.Size = new System.Drawing.Size(207, 39);
            toolStripInd4.TabIndex = 0;
            // 
            // Ind_Reg1SizeUpDown
            // 
            Ind_Reg1SizeUpDown.Location = new System.Drawing.Point(250, 68);
            Ind_Reg1SizeUpDown.Margin = new Padding(3, 5, 3, 5);
            Ind_Reg1SizeUpDown.Minimum = new decimal(new int[] { 6, 0, 0, 0 });
            Ind_Reg1SizeUpDown.Name = "Ind_Reg1SizeUpDown";
            Ind_Reg1SizeUpDown.Size = new System.Drawing.Size(59, 27);
            Ind_Reg1SizeUpDown.TabIndex = 3;
            Ind_Reg1SizeUpDown.Value = new decimal(new int[] { 6, 0, 0, 0 });
            Ind_Reg1SizeUpDown.MouseUp += Ind_FontSizeUpDown_MouseUp;
            // 
            // labelBlackScreenOn
            // 
            labelBlackScreenOn.AutoSize = true;
            labelBlackScreenOn.BackColor = System.Drawing.Color.Transparent;
            labelBlackScreenOn.Location = new System.Drawing.Point(216, 69);
            labelBlackScreenOn.Name = "labelBlackScreenOn";
            labelBlackScreenOn.Size = new System.Drawing.Size(39, 20);
            labelBlackScreenOn.TabIndex = 2;
            labelBlackScreenOn.Text = "Size:";
            // 
            // TimerToFront
            // 
            TimerToFront.Enabled = true;
            TimerToFront.Tick += TimerToFront_Tick;
            // 
            // Ind_R2Underline
            // 
            Ind_R2Underline.CheckOnClick = true;
            Ind_R2Underline.DisplayStyle = ToolStripItemDisplayStyle.Image;
            Ind_R2Underline.Image = (System.Drawing.Image)resources.GetObject("Ind_R2Underline.Image");
            Ind_R2Underline.ImageTransparentColor = System.Drawing.Color.Magenta;
            Ind_R2Underline.Name = "Ind_R2Underline";
            Ind_R2Underline.Size = new System.Drawing.Size(29, 36);
            Ind_R2Underline.MouseUp += Ind_RegionsFormat_MouseUp;
            // 
            // toolStripSeparator15
            // 
            toolStripSeparator15.Name = "toolStripSeparator15";
            toolStripSeparator15.Size = new System.Drawing.Size(6, 39);
            // 
            // Ind_R2Align
            // 
            Ind_R2Align.DisplayStyle = ToolStripItemDisplayStyle.Image;
            Ind_R2Align.DropDownItems.AddRange(new ToolStripItem[] { Ind_R2AlignLeft, Ind_R2AlignCentre, Ind_R2AlignRight });
            Ind_R2Align.Image = (System.Drawing.Image)resources.GetObject("Ind_R2Align.Image");
            Ind_R2Align.ImageTransparentColor = System.Drawing.Color.Magenta;
            Ind_R2Align.Name = "Ind_R2Align";
            Ind_R2Align.Size = new System.Drawing.Size(38, 36);
            Ind_R2Align.Tag = "1";
            Ind_R2Align.DropDownItemClicked += Ind_R2Align_DropDownItemClicked;
            // 
            // Ind_R2AlignLeft
            // 
            Ind_R2AlignLeft.Image = (System.Drawing.Image)resources.GetObject("Ind_R2AlignLeft.Image");
            Ind_R2AlignLeft.Name = "Ind_R2AlignLeft";
            Ind_R2AlignLeft.Size = new System.Drawing.Size(174, 26);
            Ind_R2AlignLeft.Tag = "1";
            Ind_R2AlignLeft.Text = "Align Left";
            // 
            // Ind_R2AlignCentre
            // 
            Ind_R2AlignCentre.Image = (System.Drawing.Image)resources.GetObject("Ind_R2AlignCentre.Image");
            Ind_R2AlignCentre.Name = "Ind_R2AlignCentre";
            Ind_R2AlignCentre.Size = new System.Drawing.Size(174, 26);
            Ind_R2AlignCentre.Tag = "2";
            Ind_R2AlignCentre.Text = "Align Centre";
            // 
            // Ind_R2AlignRight
            // 
            Ind_R2AlignRight.Image = (System.Drawing.Image)resources.GetObject("Ind_R2AlignRight.Image");
            Ind_R2AlignRight.Name = "Ind_R2AlignRight";
            Ind_R2AlignRight.Size = new System.Drawing.Size(174, 26);
            Ind_R2AlignRight.Tag = "3";
            Ind_R2AlignRight.Text = "Align Right";
            // 
            // Ind_R2Colour
            // 
            Ind_R2Colour.AutoSize = false;
            Ind_R2Colour.BackColor = System.Drawing.SystemColors.ActiveBorder;
            Ind_R2Colour.DisplayStyle = ToolStripItemDisplayStyle.Text;
            Ind_R2Colour.Image = (System.Drawing.Image)resources.GetObject("Ind_R2Colour.Image");
            Ind_R2Colour.ImageTransparentColor = System.Drawing.Color.Magenta;
            Ind_R2Colour.Name = "Ind_R2Colour";
            Ind_R2Colour.Size = new System.Drawing.Size(44, 22);
            Ind_R2Colour.Text = "Colour";
            Ind_R2Colour.MouseUp += Ind_RegionsFormat_MouseUp;
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new System.Drawing.Point(217, 32);
            label7.Name = "label7";
            label7.Size = new System.Drawing.Size(37, 20);
            label7.TabIndex = 0;
            label7.Text = "Top:";
            // 
            // IndgroupBox3
            // 
            IndgroupBox3.Controls.Add(panelInd5);
            IndgroupBox3.Controls.Add(Ind_Reg1SizeUpDown);
            IndgroupBox3.Controls.Add(labelBlackScreenOn);
            IndgroupBox3.Controls.Add(Ind_Reg1TopUpDown);
            IndgroupBox3.Controls.Add(panelInd4);
            IndgroupBox3.Controls.Add(label4);
            IndgroupBox3.Location = new System.Drawing.Point(8, 295);
            IndgroupBox3.Margin = new Padding(3, 5, 3, 5);
            IndgroupBox3.Name = "IndgroupBox3";
            IndgroupBox3.Padding = new Padding(3, 5, 3, 5);
            IndgroupBox3.Size = new System.Drawing.Size(320, 109);
            IndgroupBox3.TabIndex = 2;
            IndgroupBox3.TabStop = false;
            IndgroupBox3.Text = "Region 1";
            // 
            // Ind_Reg1TopUpDown
            // 
            Ind_Reg1TopUpDown.Location = new System.Drawing.Point(250, 29);
            Ind_Reg1TopUpDown.Margin = new Padding(3, 5, 3, 5);
            Ind_Reg1TopUpDown.Name = "Ind_Reg1TopUpDown";
            Ind_Reg1TopUpDown.Size = new System.Drawing.Size(59, 27);
            Ind_Reg1TopUpDown.TabIndex = 1;
            Ind_Reg1TopUpDown.MouseUp += Ind_MarginUpDown_MouseUp;
            // 
            // panelInd4
            // 
            panelInd4.Controls.Add(toolStripInd4);
            panelInd4.Location = new System.Drawing.Point(9, 28);
            panelInd4.Margin = new Padding(3, 5, 3, 5);
            panelInd4.Name = "panelInd4";
            panelInd4.Size = new System.Drawing.Size(207, 33);
            panelInd4.TabIndex = 10;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new System.Drawing.Point(217, 32);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(37, 20);
            label4.TabIndex = 0;
            label4.Text = "Top:";
            // 
            // panel4
            // 
            panel4.Controls.Add(toolStrip1);
            panel4.Location = new System.Drawing.Point(8, 68);
            panel4.Margin = new Padding(3, 5, 3, 5);
            panel4.Name = "panel4";
            panel4.Size = new System.Drawing.Size(139, 33);
            panel4.TabIndex = 10;
            // 
            // toolStrip1
            // 
            toolStrip1.AutoSize = false;
            toolStrip1.CanOverflow = false;
            toolStrip1.Dock = DockStyle.None;
            toolStrip1.GripStyle = ToolStripGripStyle.Hidden;
            toolStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            toolStrip1.Items.AddRange(new ToolStripItem[] { Ind_HeadAlign, Ind_CapoDown, Ind_CapoUp, toolStripSeparator5, Ind_HideDisplayPanel });
            toolStrip1.LayoutStyle = ToolStripLayoutStyle.HorizontalStackWithOverflow;
            toolStrip1.Location = new System.Drawing.Point(0, -1);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.Padding = new Padding(0, 0, 2, 0);
            toolStrip1.RenderMode = ToolStripRenderMode.System;
            toolStrip1.Size = new System.Drawing.Size(145, 39);
            toolStrip1.TabIndex = 0;
            // 
            // Ind_HeadAlign
            // 
            Ind_HeadAlign.DisplayStyle = ToolStripItemDisplayStyle.Image;
            Ind_HeadAlign.DropDownItems.AddRange(new ToolStripItem[] { Ind_HeadAlignAsR1, Ind_HeadAlignAsR2, Ind_HeadAlignLeft, Ind_HeadAlignCentre, Ind_HeadAlignRight });
            Ind_HeadAlign.Image = (System.Drawing.Image)resources.GetObject("Ind_HeadAlign.Image");
            Ind_HeadAlign.ImageTransparentColor = System.Drawing.Color.Magenta;
            Ind_HeadAlign.Name = "Ind_HeadAlign";
            Ind_HeadAlign.Size = new System.Drawing.Size(38, 36);
            Ind_HeadAlign.Tag = "0";
            Ind_HeadAlign.ToolTipText = "Display Title/Verse Headings";
            Ind_HeadAlign.DropDownItemClicked += Ind_HeadAlign_DropDownItemClicked;
            // 
            // Ind_HeadAlignAsR1
            // 
            Ind_HeadAlignAsR1.Image = (System.Drawing.Image)resources.GetObject("Ind_HeadAlignAsR1.Image");
            Ind_HeadAlignAsR1.Name = "Ind_HeadAlignAsR1";
            Ind_HeadAlignAsR1.Size = new System.Drawing.Size(277, 26);
            Ind_HeadAlignAsR1.Tag = "0";
            Ind_HeadAlignAsR1.Text = "Headings Align As Region 1";
            // 
            // Ind_HeadAlignAsR2
            // 
            Ind_HeadAlignAsR2.Image = (System.Drawing.Image)resources.GetObject("Ind_HeadAlignAsR2.Image");
            Ind_HeadAlignAsR2.Name = "Ind_HeadAlignAsR2";
            Ind_HeadAlignAsR2.Size = new System.Drawing.Size(277, 26);
            Ind_HeadAlignAsR2.Tag = "1";
            Ind_HeadAlignAsR2.Text = "Headings Align As region 2";
            // 
            // Ind_HeadAlignLeft
            // 
            Ind_HeadAlignLeft.Image = (System.Drawing.Image)resources.GetObject("Ind_HeadAlignLeft.Image");
            Ind_HeadAlignLeft.Name = "Ind_HeadAlignLeft";
            Ind_HeadAlignLeft.Size = new System.Drawing.Size(277, 26);
            Ind_HeadAlignLeft.Tag = "2";
            Ind_HeadAlignLeft.Text = "Headings Align Left";
            // 
            // Ind_HeadAlignCentre
            // 
            Ind_HeadAlignCentre.Image = (System.Drawing.Image)resources.GetObject("Ind_HeadAlignCentre.Image");
            Ind_HeadAlignCentre.Name = "Ind_HeadAlignCentre";
            Ind_HeadAlignCentre.Size = new System.Drawing.Size(277, 26);
            Ind_HeadAlignCentre.Tag = "3";
            Ind_HeadAlignCentre.Text = "Headings Align Centre";
            // 
            // Ind_HeadAlignRight
            // 
            Ind_HeadAlignRight.Image = (System.Drawing.Image)resources.GetObject("Ind_HeadAlignRight.Image");
            Ind_HeadAlignRight.Name = "Ind_HeadAlignRight";
            Ind_HeadAlignRight.Size = new System.Drawing.Size(277, 26);
            Ind_HeadAlignRight.Tag = "4";
            Ind_HeadAlignRight.Text = "Headings Align Right";
            // 
            // Ind_CapoDown
            // 
            Ind_CapoDown.DisplayStyle = ToolStripItemDisplayStyle.Image;
            Ind_CapoDown.Image = (System.Drawing.Image)resources.GetObject("Ind_CapoDown.Image");
            Ind_CapoDown.ImageTransparentColor = System.Drawing.Color.Magenta;
            Ind_CapoDown.Name = "Ind_CapoDown";
            Ind_CapoDown.Size = new System.Drawing.Size(29, 36);
            Ind_CapoDown.ToolTipText = "Transpose Down 1 Semi-Tone";
            Ind_CapoDown.MouseUp += Ind_Items_MouseUp;
            // 
            // Ind_CapoUp
            // 
            Ind_CapoUp.DisplayStyle = ToolStripItemDisplayStyle.Image;
            Ind_CapoUp.Image = (System.Drawing.Image)resources.GetObject("Ind_CapoUp.Image");
            Ind_CapoUp.ImageTransparentColor = System.Drawing.Color.Magenta;
            Ind_CapoUp.Name = "Ind_CapoUp";
            Ind_CapoUp.Size = new System.Drawing.Size(29, 36);
            Ind_CapoUp.ToolTipText = "Transpose Up 1 Semi-Tone";
            Ind_CapoUp.MouseUp += Ind_Items_MouseUp;
            // 
            // toolStripSeparator5
            // 
            toolStripSeparator5.Name = "toolStripSeparator5";
            toolStripSeparator5.Size = new System.Drawing.Size(6, 39);
            // 
            // Ind_HideDisplayPanel
            // 
            Ind_HideDisplayPanel.CheckOnClick = true;
            Ind_HideDisplayPanel.DisplayStyle = ToolStripItemDisplayStyle.Image;
            Ind_HideDisplayPanel.Image = (System.Drawing.Image)resources.GetObject("Ind_HideDisplayPanel.Image");
            Ind_HideDisplayPanel.ImageTransparentColor = System.Drawing.Color.Magenta;
            Ind_HideDisplayPanel.Name = "Ind_HideDisplayPanel";
            Ind_HideDisplayPanel.Size = new System.Drawing.Size(29, 36);
            Ind_HideDisplayPanel.ToolTipText = "Do not show Display Panel Data";
            Ind_HideDisplayPanel.MouseUp += Ind_Items_MouseUp;
            // 
            // IndgroupBox1
            // 
            IndgroupBox1.Controls.Add(panel4);
            IndgroupBox1.Controls.Add(panelInd1);
            IndgroupBox1.Location = new System.Drawing.Point(8, 29);
            IndgroupBox1.Margin = new Padding(3, 5, 3, 5);
            IndgroupBox1.Name = "IndgroupBox1";
            IndgroupBox1.Padding = new Padding(3, 5, 3, 5);
            IndgroupBox1.Size = new System.Drawing.Size(320, 112);
            IndgroupBox1.TabIndex = 0;
            IndgroupBox1.TabStop = false;
            IndgroupBox1.Text = "Layout";
            // 
            // panelInd1
            // 
            panelInd1.Controls.Add(toolStripInd1);
            panelInd1.Location = new System.Drawing.Point(8, 28);
            panelInd1.Margin = new Padding(3, 5, 3, 5);
            panelInd1.Name = "panelInd1";
            panelInd1.Size = new System.Drawing.Size(243, 33);
            panelInd1.TabIndex = 9;
            // 
            // toolStripInd1
            // 
            toolStripInd1.AutoSize = false;
            toolStripInd1.CanOverflow = false;
            toolStripInd1.Dock = DockStyle.None;
            toolStripInd1.GripStyle = ToolStripGripStyle.Hidden;
            toolStripInd1.ImageScalingSize = new System.Drawing.Size(24, 24);
            toolStripInd1.Items.AddRange(new ToolStripItem[] { Ind_Head, Ind_Region, Ind_VAlign, Ind_Shadow, Ind_Outline, Ind_Interlace, Ind_Notations });
            toolStripInd1.LayoutStyle = ToolStripLayoutStyle.HorizontalStackWithOverflow;
            toolStripInd1.Location = new System.Drawing.Point(0, -1);
            toolStripInd1.Name = "toolStripInd1";
            toolStripInd1.Padding = new Padding(0, 0, 2, 0);
            toolStripInd1.RenderMode = ToolStripRenderMode.System;
            toolStripInd1.Size = new System.Drawing.Size(248, 39);
            toolStripInd1.TabIndex = 0;
            // 
            // Ind_Head
            // 
            Ind_Head.DisplayStyle = ToolStripItemDisplayStyle.Image;
            Ind_Head.DropDownItems.AddRange(new ToolStripItem[] { Ind_HeadNoTitles, Ind_HeadAllTitles, Ind_HeadFirstScreen });
            Ind_Head.Image = (System.Drawing.Image)resources.GetObject("Ind_Head.Image");
            Ind_Head.ImageTransparentColor = System.Drawing.Color.Magenta;
            Ind_Head.Name = "Ind_Head";
            Ind_Head.Size = new System.Drawing.Size(38, 36);
            Ind_Head.Tag = "1";
            Ind_Head.ToolTipText = "Display Title/Verse Headings";
            Ind_Head.DropDownItemClicked += Ind_Head_DropDownItemClicked;
            // 
            // Menu_ImportFolder
            // 
            Menu_ImportFolder.Name = "Menu_ImportFolder";
            Menu_ImportFolder.Size = new System.Drawing.Size(366, 26);
            Menu_ImportFolder.Text = "I&mport Folder";
            Menu_ImportFolder.Click += Menu_ImportFolder_Click;
            // 
            // Menu_GoLiveWithPreview
            // 
            Menu_GoLiveWithPreview.Name = "Menu_GoLiveWithPreview";
            Menu_GoLiveWithPreview.ShortcutKeys = Keys.F11;
            Menu_GoLiveWithPreview.Size = new System.Drawing.Size(310, 26);
            Menu_GoLiveWithPreview.Text = "Preview: Go Live, Move Next";
            Menu_GoLiveWithPreview.Click += Menu_PreviewGoLiveNext_Click;
            // 
            // Menu_RefreshOutput
            // 
            Menu_RefreshOutput.Name = "Menu_RefreshOutput";
            Menu_RefreshOutput.Size = new System.Drawing.Size(310, 26);
            Menu_RefreshOutput.Text = "Refresh Output";
            Menu_RefreshOutput.Click += Menu_RefreshOutput_Click;
            // 
            // toolStripSeparator28
            // 
            toolStripSeparator28.Name = "toolStripSeparator28";
            toolStripSeparator28.Size = new System.Drawing.Size(307, 6);
            // 
            // Menu_BlackScreen
            // 
            Menu_BlackScreen.Image = (System.Drawing.Image)resources.GetObject("Menu_BlackScreen.Image");
            Menu_BlackScreen.Name = "Menu_BlackScreen";
            Menu_BlackScreen.ShortcutKeys = Keys.F9;
            Menu_BlackScreen.Size = new System.Drawing.Size(310, 26);
            Menu_BlackScreen.Text = "Black Screen";
            Menu_BlackScreen.Click += Menu_BlackScreen_Click;
            // 
            // Menu_ClearScreen
            // 
            Menu_ClearScreen.Image = (System.Drawing.Image)resources.GetObject("Menu_ClearScreen.Image");
            Menu_ClearScreen.Name = "Menu_ClearScreen";
            Menu_ClearScreen.ShortcutKeys = Keys.F3;
            Menu_ClearScreen.Size = new System.Drawing.Size(310, 26);
            Menu_ClearScreen.Text = "Clear Screen";
            Menu_ClearScreen.Click += Menu_ClearScreen_Click;
            // 
            // Menu_StartShow
            // 
            Menu_StartShow.Name = "Menu_StartShow";
            Menu_StartShow.ShortcutKeys = Keys.F12;
            Menu_StartShow.Size = new System.Drawing.Size(310, 26);
            Menu_StartShow.Text = "Start Show - Go LIVE";
            Menu_StartShow.Click += Menu_StartShow_Click;
            // 
            // Menu_RestartCurrentItem
            // 
            Menu_RestartCurrentItem.Name = "Menu_RestartCurrentItem";
            Menu_RestartCurrentItem.ShortcutKeys = Keys.F5;
            Menu_RestartCurrentItem.Size = new System.Drawing.Size(310, 26);
            Menu_RestartCurrentItem.Text = "Restart Current Item";
            Menu_RestartCurrentItem.Click += Menu_RestartCurrentItem_Click;
            // 
            // Menu_MainTools
            // 
            Menu_MainTools.DropDownItems.AddRange(new ToolStripItem[] { Menu_Import, Menu_ImportFolder, Menu_Export, toolStripSeparator32, Menu_Recover, Menu_Empty, toolStripSeparator33, Menu_AddToUsages, Menu_ViewUsages, toolStripSeparator34, Menu_SmartMerge, Menu_Compact, Menu_ClearAllFormatting, toolStripSeparator9, Menu_ClearRegistrySettings });
            Menu_MainTools.Name = "Menu_MainTools";
            Menu_MainTools.Size = new System.Drawing.Size(58, 24);
            Menu_MainTools.Text = "&Tools";
            // 
            // Menu_Import
            // 
            Menu_Import.Name = "Menu_Import";
            Menu_Import.Size = new System.Drawing.Size(366, 26);
            Menu_Import.Text = "&Import";
            Menu_Import.Click += Menu_Import_Click;
            // 
            // Menu_Export
            // 
            Menu_Export.Name = "Menu_Export";
            Menu_Export.Size = new System.Drawing.Size(366, 26);
            Menu_Export.Text = "&Export";
            Menu_Export.Click += Menu_Export_Click;
            // 
            // toolStripSeparator32
            // 
            toolStripSeparator32.Name = "toolStripSeparator32";
            toolStripSeparator32.Size = new System.Drawing.Size(363, 6);
            // 
            // Menu_Recover
            // 
            Menu_Recover.Name = "Menu_Recover";
            Menu_Recover.Size = new System.Drawing.Size(366, 26);
            Menu_Recover.Text = "&Recover Deleted Items";
            Menu_Recover.Click += Menu_Recover_Click;
            // 
            // Menu_Empty
            // 
            Menu_Empty.Name = "Menu_Empty";
            Menu_Empty.Size = new System.Drawing.Size(366, 26);
            Menu_Empty.Text = "&Empty Deleted Folder...";
            Menu_Empty.Click += Menu_Empty_Click;
            // 
            // toolStripSeparator33
            // 
            toolStripSeparator33.Name = "toolStripSeparator33";
            toolStripSeparator33.Size = new System.Drawing.Size(363, 6);
            // 
            // Menu_AddToUsages
            // 
            Menu_AddToUsages.Name = "Menu_AddToUsages";
            Menu_AddToUsages.Size = new System.Drawing.Size(366, 26);
            Menu_AddToUsages.Text = "&Add Worship List to Usages";
            Menu_AddToUsages.Click += Menu_AddToUsages_Click;
            // 
            // Menu_ViewUsages
            // 
            Menu_ViewUsages.Name = "Menu_ViewUsages";
            Menu_ViewUsages.Size = new System.Drawing.Size(366, 26);
            Menu_ViewUsages.Text = "&View usages";
            Menu_ViewUsages.Click += Menu_ViewUsages_Click;
            // 
            // toolStripSeparator34
            // 
            toolStripSeparator34.Name = "toolStripSeparator34";
            toolStripSeparator34.Size = new System.Drawing.Size(363, 6);
            // 
            // Menu_SmartMerge
            // 
            Menu_SmartMerge.Name = "Menu_SmartMerge";
            Menu_SmartMerge.Size = new System.Drawing.Size(366, 26);
            Menu_SmartMerge.Text = "&Smart Merge";
            Menu_SmartMerge.Click += Menu_SmartMerge_Click;
            // 
            // Menu_Compact
            // 
            Menu_Compact.Name = "Menu_Compact";
            Menu_Compact.Size = new System.Drawing.Size(366, 26);
            Menu_Compact.Text = "&Compact and Repair Databases";
            Menu_Compact.Click += Menu_Compact_Click;
            // 
            // Menu_ClearAllFormatting
            // 
            Menu_ClearAllFormatting.Name = "Menu_ClearAllFormatting";
            Menu_ClearAllFormatting.Size = new System.Drawing.Size(366, 26);
            Menu_ClearAllFormatting.Text = "Clear All &Formatting in Database.";
            Menu_ClearAllFormatting.Click += Menu_ClearAllFormatting_Click;
            // 
            // toolStripSeparator9
            // 
            toolStripSeparator9.Name = "toolStripSeparator9";
            toolStripSeparator9.Size = new System.Drawing.Size(363, 6);
            // 
            // Menu_ClearRegistrySettings
            // 
            Menu_ClearRegistrySettings.Name = "Menu_ClearRegistrySettings";
            Menu_ClearRegistrySettings.Size = new System.Drawing.Size(366, 26);
            Menu_ClearRegistrySettings.Text = "Clear EasiSlides Registry Settings and Exit";
            Menu_ClearRegistrySettings.Click += Menu_ClearRegistrySettings_Click;
            // 
            // Menu_EditSong
            // 
            Menu_EditSong.Image = (System.Drawing.Image)resources.GetObject("Menu_EditSong.Image");
            Menu_EditSong.Name = "Menu_EditSong";
            Menu_EditSong.Size = new System.Drawing.Size(258, 26);
            Menu_EditSong.Text = "Edit";
            Menu_EditSong.Click += Menu_EditSong_Click;
            // 
            // Menu_MainOutput
            // 
            Menu_MainOutput.DropDownItems.AddRange(new ToolStripItem[] { Menu_StartShow, Menu_GoLiveWithPreview, Menu_RefreshOutput, toolStripSeparator28, Menu_BlackScreen, Menu_ClearScreen, Menu_RestartCurrentItem });
            Menu_MainOutput.Name = "Menu_MainOutput";
            Menu_MainOutput.Size = new System.Drawing.Size(69, 24);
            Menu_MainOutput.Text = "&Output";
            // 
            // toolStripSeparator19
            // 
            toolStripSeparator19.Name = "toolStripSeparator19";
            toolStripSeparator19.Size = new System.Drawing.Size(255, 6);
            // 
            // Menu_CopySong
            // 
            Menu_CopySong.Image = (System.Drawing.Image)resources.GetObject("Menu_CopySong.Image");
            Menu_CopySong.Name = "Menu_CopySong";
            Menu_CopySong.Size = new System.Drawing.Size(258, 26);
            Menu_CopySong.Text = "Copy";
            Menu_CopySong.Click += Menu_CopySong_Click;
            // 
            // Menu_MoveSong
            // 
            Menu_MoveSong.Image = (System.Drawing.Image)resources.GetObject("Menu_MoveSong.Image");
            Menu_MoveSong.Name = "Menu_MoveSong";
            Menu_MoveSong.Size = new System.Drawing.Size(258, 26);
            Menu_MoveSong.Text = "Move";
            Menu_MoveSong.Click += Menu_MoveSong_Click;
            // 
            // Menu_DeleteSong
            // 
            Menu_DeleteSong.Image = (System.Drawing.Image)resources.GetObject("Menu_DeleteSong.Image");
            Menu_DeleteSong.Name = "Menu_DeleteSong";
            Menu_DeleteSong.Size = new System.Drawing.Size(258, 26);
            Menu_DeleteSong.Text = "Delete...";
            Menu_DeleteSong.Click += Menu_DeleteSong_Click;
            // 
            // toolStripSeparator41
            // 
            toolStripSeparator41.Name = "toolStripSeparator41";
            toolStripSeparator41.Size = new System.Drawing.Size(255, 6);
            // 
            // Menu_SelectAll
            // 
            Menu_SelectAll.Name = "Menu_SelectAll";
            Menu_SelectAll.Size = new System.Drawing.Size(258, 26);
            Menu_SelectAll.Text = "Select All";
            Menu_SelectAll.Click += Menu_SelectAll_Click;
            // 
            // Menu_Find
            // 
            Menu_Find.Image = (System.Drawing.Image)resources.GetObject("Menu_Find.Image");
            Menu_Find.Name = "Menu_Find";
            Menu_Find.ShortcutKeys = Keys.Control | Keys.F;
            Menu_Find.Size = new System.Drawing.Size(258, 26);
            Menu_Find.Text = "Find";
            Menu_Find.Click += Menu_Find_Click;
            // 
            // Menu_StatusBar
            // 
            Menu_StatusBar.Checked = true;
            Menu_StatusBar.CheckState = CheckState.Checked;
            Menu_StatusBar.Name = "Menu_StatusBar";
            Menu_StatusBar.Size = new System.Drawing.Size(268, 26);
            Menu_StatusBar.Text = "Status Bar";
            Menu_StatusBar.Click += Menu_StatusBar_Click;
            // 
            // toolStripSeparator21
            // 
            toolStripSeparator21.Name = "toolStripSeparator21";
            toolStripSeparator21.Size = new System.Drawing.Size(255, 6);
            // 
            // Menu_ReArrangeSongFolders
            // 
            Menu_ReArrangeSongFolders.Name = "Menu_ReArrangeSongFolders";
            Menu_ReArrangeSongFolders.Size = new System.Drawing.Size(258, 26);
            Menu_ReArrangeSongFolders.Text = "Re-Arrange Song Folders";
            Menu_ReArrangeSongFolders.Click += Menu_ReArrangeSongFolders_Click;
            // 
            // Menu_MainView
            // 
            Menu_MainView.DropDownItems.AddRange(new ToolStripItem[] { Menu_EasiSlidesFolder, Menu_Options, toolStripSeparator23, Menu_Refresh, Menu_PreviewNotations, Menu_StatusBar });
            Menu_MainView.Name = "Menu_MainView";
            Menu_MainView.Size = new System.Drawing.Size(55, 24);
            Menu_MainView.Text = "&View";
            // 
            // Menu_EasiSlidesFolder
            // 
            Menu_EasiSlidesFolder.Image = (System.Drawing.Image)resources.GetObject("Menu_EasiSlidesFolder.Image");
            Menu_EasiSlidesFolder.Name = "Menu_EasiSlidesFolder";
            Menu_EasiSlidesFolder.Size = new System.Drawing.Size(268, 26);
            Menu_EasiSlidesFolder.Text = "EasiSlides Folder";
            Menu_EasiSlidesFolder.Click += Menu_EasiSlidesFolder_Click;
            // 
            // Menu_Options
            // 
            Menu_Options.Image = (System.Drawing.Image)resources.GetObject("Menu_Options.Image");
            Menu_Options.Name = "Menu_Options";
            Menu_Options.Size = new System.Drawing.Size(268, 26);
            Menu_Options.Text = "Options";
            Menu_Options.Click += Menu_Options_Click;
            // 
            // toolStripSeparator23
            // 
            toolStripSeparator23.Name = "toolStripSeparator23";
            toolStripSeparator23.Size = new System.Drawing.Size(265, 6);
            // 
            // Menu_Refresh
            // 
            Menu_Refresh.Image = (System.Drawing.Image)resources.GetObject("Menu_Refresh.Image");
            Menu_Refresh.Name = "Menu_Refresh";
            Menu_Refresh.Size = new System.Drawing.Size(268, 26);
            Menu_Refresh.Text = "Refresh";
            Menu_Refresh.Click += Menu_Refresh_Click;
            // 
            // Menu_PreviewNotations
            // 
            Menu_PreviewNotations.CheckOnClick = true;
            Menu_PreviewNotations.Name = "Menu_PreviewNotations";
            Menu_PreviewNotations.Size = new System.Drawing.Size(268, 26);
            Menu_PreviewNotations.Text = "Show Notations in Preview";
            Menu_PreviewNotations.Click += Menu_PreviewNotations_Click;
            // 
            // Menu_UseSongNumbering
            // 
            Menu_UseSongNumbering.CheckOnClick = true;
            Menu_UseSongNumbering.Name = "Menu_UseSongNumbering";
            Menu_UseSongNumbering.Size = new System.Drawing.Size(258, 26);
            Menu_UseSongNumbering.Text = "Use Song Numbering";
            Menu_UseSongNumbering.Click += Menu_useSongNumbering_Click;
            // 
            // StatusBarPanel2
            // 
            StatusBarPanel2.AutoSize = false;
            StatusBarPanel2.AutoToolTip = true;
            StatusBarPanel2.BorderSides = ToolStripStatusLabelBorderSides.Left | ToolStripStatusLabelBorderSides.Right;
            StatusBarPanel2.BorderStyle = Border3DStyle.RaisedInner;
            StatusBarPanel2.DisplayStyle = ToolStripItemDisplayStyle.Text;
            StatusBarPanel2.ImageAlign = System.Drawing.ContentAlignment.TopLeft;
            StatusBarPanel2.Name = "StatusBarPanel2";
            StatusBarPanel2.Padding = new Padding(3, 0, 4, 0);
            StatusBarPanel2.Size = new System.Drawing.Size(10, 27);
            StatusBarPanel2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // StatusBarPanel3
            // 
            StatusBarPanel3.AutoSize = false;
            StatusBarPanel3.AutoToolTip = true;
            StatusBarPanel3.BorderSides = ToolStripStatusLabelBorderSides.Left | ToolStripStatusLabelBorderSides.Right;
            StatusBarPanel3.BorderStyle = Border3DStyle.RaisedInner;
            StatusBarPanel3.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            StatusBarPanel3.Name = "StatusBarPanel3";
            StatusBarPanel3.Padding = new Padding(3, 0, 4, 0);
            StatusBarPanel3.Size = new System.Drawing.Size(10, 27);
            StatusBarPanel3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // StatusBarPanel4
            // 
            StatusBarPanel4.AutoSize = false;
            StatusBarPanel4.AutoToolTip = true;
            StatusBarPanel4.BorderSides = ToolStripStatusLabelBorderSides.Left | ToolStripStatusLabelBorderSides.Right;
            StatusBarPanel4.BorderStyle = Border3DStyle.RaisedInner;
            StatusBarPanel4.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            StatusBarPanel4.Name = "StatusBarPanel4";
            StatusBarPanel4.Padding = new Padding(3, 0, 4, 0);
            StatusBarPanel4.Size = new System.Drawing.Size(10, 27);
            StatusBarPanel4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // DefApplyDefaultsBtn
            // 
            DefApplyDefaultsBtn.FlatStyle = FlatStyle.Flat;
            DefApplyDefaultsBtn.Location = new System.Drawing.Point(3, 5);
            DefApplyDefaultsBtn.Margin = new Padding(3, 5, 3, 5);
            DefApplyDefaultsBtn.Name = "DefApplyDefaultsBtn";
            DefApplyDefaultsBtn.Size = new System.Drawing.Size(230, 35);
            DefApplyDefaultsBtn.TabIndex = 0;
            DefApplyDefaultsBtn.Text = "Apply to All Except InfoScreens";
            toolTip1.SetToolTip(DefApplyDefaultsBtn, "Apply Defaults to all on Worship List except InfoScreen Items");
            DefApplyDefaultsBtn.Click += DefApplyDefaultsBtn_Click;
            // 
            // TimerFlasher
            // 
            TimerFlasher.Interval = 600;
            TimerFlasher.Tick += TimerFlasher_Tick;
            // 
            // StatusBarPanel1
            // 
            StatusBarPanel1.AutoSize = false;
            StatusBarPanel1.AutoToolTip = true;
            StatusBarPanel1.BorderSides = ToolStripStatusLabelBorderSides.Left | ToolStripStatusLabelBorderSides.Right;
            StatusBarPanel1.BorderStyle = Border3DStyle.RaisedInner;
            StatusBarPanel1.DisplayStyle = ToolStripItemDisplayStyle.Text;
            StatusBarPanel1.ImageAlign = System.Drawing.ContentAlignment.TopLeft;
            StatusBarPanel1.Name = "StatusBarPanel1";
            StatusBarPanel1.Padding = new Padding(3, 0, 4, 0);
            StatusBarPanel1.Size = new System.Drawing.Size(10, 27);
            StatusBarPanel1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // TimerReMax
            // 
            TimerReMax.Interval = 1000;
            // 
            // CMenuImages
            // 
            CMenuImages.ImageScalingSize = new System.Drawing.Size(24, 24);
            CMenuImages.Items.AddRange(new ToolStripItem[] { CMenuImages_AddItem, CMenuImages_AddDefault, toolStripSeparator35, CMenuImages_Refresh });
            CMenuImages.Name = "CMenuImages";
            CMenuImages.Size = new System.Drawing.Size(212, 82);
            CMenuImages.Opening += CMenuImages_Opening;
            // 
            // CMenuImages_AddItem
            // 
            CMenuImages_AddItem.Name = "CMenuImages_AddItem";
            CMenuImages_AddItem.Size = new System.Drawing.Size(211, 24);
            CMenuImages_AddItem.Text = "Add to Item";
            CMenuImages_AddItem.Click += CMenuImages_AddItem_Click;
            // 
            // CMenuImages_AddDefault
            // 
            CMenuImages_AddDefault.Name = "CMenuImages_AddDefault";
            CMenuImages_AddDefault.Size = new System.Drawing.Size(211, 24);
            CMenuImages_AddDefault.Text = "Add to Default";
            CMenuImages_AddDefault.Click += CMenuImages_AddDefault_Click;
            // 
            // toolStripSeparator35
            // 
            toolStripSeparator35.Name = "toolStripSeparator35";
            toolStripSeparator35.Size = new System.Drawing.Size(208, 6);
            // 
            // CMenuImages_Refresh
            // 
            CMenuImages_Refresh.Name = "CMenuImages_Refresh";
            CMenuImages_Refresh.Size = new System.Drawing.Size(211, 24);
            CMenuImages_Refresh.Text = "Refresh Images Lists";
            CMenuImages_Refresh.Click += CMenuImages_Refresh_Click;
            // 
            // TimerMessagingWindowOpen
            // 
            TimerMessagingWindowOpen.Interval = 1000;
            TimerMessagingWindowOpen.Tick += TimerMessagingWindowOpen_Tick;
            // 
            // TimerSearch
            // 
            TimerSearch.Interval = 500;
            TimerSearch.Tick += TimerSearch_Tick;
            // 
            // statusStripMain
            // 
            statusStripMain.AutoSize = false;
            statusStripMain.ImageScalingSize = new System.Drawing.Size(24, 24);
            statusStripMain.Items.AddRange(new ToolStripItem[] { StatusBarPanel1, StatusBarPanel2, StatusBarPanel3, StatusBarPanel4 });
            statusStripMain.Location = new System.Drawing.Point(0, 719);
            statusStripMain.Name = "statusStripMain";
            statusStripMain.Padding = new Padding(1, 0, 18, 0);
            statusStripMain.Size = new System.Drawing.Size(979, 33);
            statusStripMain.TabIndex = 1;
            // 
            // Menu_About
            // 
            Menu_About.Name = "Menu_About";
            Menu_About.Size = new System.Drawing.Size(261, 26);
            Menu_About.Text = "About EasiSlides";
            Menu_About.Click += Menu_About_Click;
            // 
            // Menu_MainHelp
            // 
            Menu_MainHelp.DropDownItems.AddRange(new ToolStripItem[] { Menu_Contents, Menu_HelpWeb, toolStripSeparator31, Menu_Register, Menu_About });
            Menu_MainHelp.Name = "Menu_MainHelp";
            Menu_MainHelp.Size = new System.Drawing.Size(55, 24);
            Menu_MainHelp.Text = "&Help";
            // 
            // Menu_Contents
            // 
            Menu_Contents.Image = (System.Drawing.Image)resources.GetObject("Menu_Contents.Image");
            Menu_Contents.Name = "Menu_Contents";
            Menu_Contents.ShortcutKeys = Keys.F1;
            Menu_Contents.Size = new System.Drawing.Size(261, 26);
            Menu_Contents.Text = "Contents";
            Menu_Contents.Click += Menu_Contents_Click;
            // 
            // Menu_HelpWeb
            // 
            Menu_HelpWeb.Image = (System.Drawing.Image)resources.GetObject("Menu_HelpWeb.Image");
            Menu_HelpWeb.Name = "Menu_HelpWeb";
            Menu_HelpWeb.Size = new System.Drawing.Size(261, 26);
            Menu_HelpWeb.Text = "Help on the Web";
            Menu_HelpWeb.Click += Menu_HelpWeb_Click;
            // 
            // toolStripSeparator31
            // 
            toolStripSeparator31.Name = "toolStripSeparator31";
            toolStripSeparator31.Size = new System.Drawing.Size(258, 6);
            // 
            // Menu_Register
            // 
            Menu_Register.Name = "Menu_Register";
            Menu_Register.Size = new System.Drawing.Size(261, 26);
            Menu_Register.Text = "Register Use of EasiSlides";
            Menu_Register.Click += Menu_Register_Click;
            // 
            // Menu_AddSong
            // 
            Menu_AddSong.Image = (System.Drawing.Image)resources.GetObject("Menu_AddSong.Image");
            Menu_AddSong.Name = "Menu_AddSong";
            Menu_AddSong.Size = new System.Drawing.Size(258, 26);
            Menu_AddSong.Text = "Add New Song...";
            Menu_AddSong.Click += Menu_AddSong_Click;
            // 
            // toolStripMain
            // 
            toolStripMain.Dock = DockStyle.None;
            toolStripMain.GripStyle = ToolStripGripStyle.Hidden;
            toolStripMain.ImageScalingSize = new System.Drawing.Size(24, 24);
            toolStripMain.Items.AddRange(new ToolStripItem[] { Main_New, Main_Edit, Main_Copy, Main_Move, Main_Delete, toolStripSeparator1, Main_Media, Main_Refresh, toolStripSeparator2, Main_Options, toolStripSeparator3, Main_NoRotate, Main_RotateStyle, Main_Alerts, Main_Chinese, toolStripSeparator4, Main_Find, Main_QuickFind, Main_JumpA, Main_JumpB, Main_JumpC });
            toolStripMain.Location = new System.Drawing.Point(4, 0);
            toolStripMain.Name = "toolStripMain";
            toolStripMain.Size = new System.Drawing.Size(632, 31);
            toolStripMain.TabIndex = 0;
            toolStripMain.Text = "toolStrip1";
            // 
            // Main_New
            // 
            Main_New.AutoSize = false;
            Main_New.BackgroundImageLayout = ImageLayout.Stretch;
            Main_New.DisplayStyle = ToolStripItemDisplayStyle.Image;
            Main_New.Image = (System.Drawing.Image)resources.GetObject("Main_New.Image");
            Main_New.ImageTransparentColor = System.Drawing.Color.Magenta;
            Main_New.Name = "Main_New";
            Main_New.Size = new System.Drawing.Size(29, 28);
            Main_New.Tag = "";
            Main_New.ToolTipText = "New";
            Main_New.Click += Main_EditBtns_Click;
            // 
            // Main_Edit
            // 
            Main_Edit.AutoSize = false;
            Main_Edit.BackgroundImageLayout = ImageLayout.Stretch;
            Main_Edit.DisplayStyle = ToolStripItemDisplayStyle.Image;
            Main_Edit.Image = (System.Drawing.Image)resources.GetObject("Main_Edit.Image");
            Main_Edit.ImageTransparentColor = System.Drawing.Color.Magenta;
            Main_Edit.Name = "Main_Edit";
            Main_Edit.Size = new System.Drawing.Size(29, 28);
            Main_Edit.Tag = "";
            Main_Edit.ToolTipText = "Edit";
            Main_Edit.Click += Main_EditBtns_Click;
            // 
            // Main_Copy
            // 
            Main_Copy.AutoSize = false;
            Main_Copy.BackgroundImageLayout = ImageLayout.Stretch;
            Main_Copy.DisplayStyle = ToolStripItemDisplayStyle.Image;
            Main_Copy.Image = (System.Drawing.Image)resources.GetObject("Main_Copy.Image");
            Main_Copy.ImageTransparentColor = System.Drawing.Color.Magenta;
            Main_Copy.Name = "Main_Copy";
            Main_Copy.Size = new System.Drawing.Size(29, 28);
            Main_Copy.Tag = "";
            Main_Copy.ToolTipText = "Copy";
            Main_Copy.Click += Main_EditBtns_Click;
            // 
            // Main_Move
            // 
            Main_Move.AutoSize = false;
            Main_Move.BackgroundImageLayout = ImageLayout.Stretch;
            Main_Move.DisplayStyle = ToolStripItemDisplayStyle.Image;
            Main_Move.Image = (System.Drawing.Image)resources.GetObject("Main_Move.Image");
            Main_Move.ImageTransparentColor = System.Drawing.Color.Magenta;
            Main_Move.Name = "Main_Move";
            Main_Move.Padding = new Padding(2);
            Main_Move.Size = new System.Drawing.Size(29, 28);
            Main_Move.Tag = "";
            Main_Move.ToolTipText = "Move";
            Main_Move.Click += Main_EditBtns_Click;
            // 
            // Main_Delete
            // 
            Main_Delete.AutoSize = false;
            Main_Delete.BackgroundImageLayout = ImageLayout.Stretch;
            Main_Delete.DisplayStyle = ToolStripItemDisplayStyle.Image;
            Main_Delete.Image = (System.Drawing.Image)resources.GetObject("Main_Delete.Image");
            Main_Delete.ImageTransparentColor = System.Drawing.Color.Magenta;
            Main_Delete.Name = "Main_Delete";
            Main_Delete.Size = new System.Drawing.Size(29, 28);
            Main_Delete.Tag = "";
            Main_Delete.ToolTipText = "Delete";
            Main_Delete.Click += Main_EditBtns_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new System.Drawing.Size(6, 31);
            // 
            // Main_Media
            // 
            Main_Media.AutoSize = false;
            Main_Media.BackgroundImageLayout = ImageLayout.Stretch;
            Main_Media.DisplayStyle = ToolStripItemDisplayStyle.Image;
            Main_Media.Image = (System.Drawing.Image)resources.GetObject("Main_Media.Image");
            Main_Media.ImageTransparentColor = System.Drawing.Color.Magenta;
            Main_Media.Name = "Main_Media";
            Main_Media.Size = new System.Drawing.Size(29, 28);
            Main_Media.Tag = "";
            Main_Media.ToolTipText = "Play Media";
            Main_Media.Click += Main_Media_Click;
            // 
            // Main_Refresh
            // 
            Main_Refresh.AutoSize = false;
            Main_Refresh.BackgroundImageLayout = ImageLayout.Stretch;
            Main_Refresh.DisplayStyle = ToolStripItemDisplayStyle.Image;
            Main_Refresh.Image = (System.Drawing.Image)resources.GetObject("Main_Refresh.Image");
            Main_Refresh.ImageTransparentColor = System.Drawing.Color.Magenta;
            Main_Refresh.Name = "Main_Refresh";
            Main_Refresh.Size = new System.Drawing.Size(29, 28);
            Main_Refresh.Tag = "";
            Main_Refresh.ToolTipText = "Refresh";
            Main_Refresh.Click += Main_Refresh_Click;
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new System.Drawing.Size(6, 31);
            // 
            // Main_Options
            // 
            Main_Options.AutoSize = false;
            Main_Options.BackgroundImageLayout = ImageLayout.Stretch;
            Main_Options.DisplayStyle = ToolStripItemDisplayStyle.Image;
            Main_Options.Image = (System.Drawing.Image)resources.GetObject("Main_Options.Image");
            Main_Options.ImageTransparentColor = System.Drawing.Color.Magenta;
            Main_Options.Name = "Main_Options";
            Main_Options.Size = new System.Drawing.Size(29, 28);
            Main_Options.Tag = "";
            Main_Options.ToolTipText = "Options";
            Main_Options.Click += Main_Options_Click;
            // 
            // toolStripSeparator3
            // 
            toolStripSeparator3.Name = "toolStripSeparator3";
            toolStripSeparator3.Size = new System.Drawing.Size(6, 31);
            // 
            // Main_NoRotate
            // 
            Main_NoRotate.AutoSize = false;
            Main_NoRotate.BackgroundImageLayout = ImageLayout.Stretch;
            Main_NoRotate.CheckOnClick = true;
            Main_NoRotate.DisplayStyle = ToolStripItemDisplayStyle.Image;
            Main_NoRotate.Image = (System.Drawing.Image)resources.GetObject("Main_NoRotate.Image");
            Main_NoRotate.ImageTransparentColor = System.Drawing.Color.Magenta;
            Main_NoRotate.Name = "Main_NoRotate";
            Main_NoRotate.Size = new System.Drawing.Size(29, 28);
            Main_NoRotate.Tag = "";
            Main_NoRotate.ToolTipText = "Stop Auto Rotate ";
            Main_NoRotate.Click += Main_NoRotate_Click;
            // 
            // Main_RotateStyle
            // 
            Main_RotateStyle.DisplayStyle = ToolStripItemDisplayStyle.Image;
            Main_RotateStyle.DropDownItems.AddRange(new ToolStripItem[] { Main_Rotate0, Main_Rotate1, Main_Rotate2, Main_Rotate3 });
            Main_RotateStyle.Image = (System.Drawing.Image)resources.GetObject("Main_RotateStyle.Image");
            Main_RotateStyle.ImageTransparentColor = System.Drawing.Color.Magenta;
            Main_RotateStyle.Name = "Main_RotateStyle";
            Main_RotateStyle.Size = new System.Drawing.Size(38, 28);
            Main_RotateStyle.ToolTipText = "Rotate Style";
            Main_RotateStyle.DropDownItemClicked += Main_RotateStyle_DropDownItemClicked;
            // 
            // Main_Rotate0
            // 
            Main_Rotate0.Image = (System.Drawing.Image)resources.GetObject("Main_Rotate0.Image");
            Main_Rotate0.Name = "Main_Rotate0";
            Main_Rotate0.Size = new System.Drawing.Size(298, 26);
            Main_Rotate0.Tag = "0";
            Main_Rotate0.Text = "Auto Rotate One Item ";
            // 
            // Main_Rotate1
            // 
            Main_Rotate1.Image = (System.Drawing.Image)resources.GetObject("Main_Rotate1.Image");
            Main_Rotate1.Name = "Main_Rotate1";
            Main_Rotate1.Size = new System.Drawing.Size(298, 26);
            Main_Rotate1.Tag = "1";
            Main_Rotate1.Text = "Auto Rotate One Item - Repeat";
            // 
            // Main_Rotate2
            // 
            Main_Rotate2.Image = (System.Drawing.Image)resources.GetObject("Main_Rotate2.Image");
            Main_Rotate2.Name = "Main_Rotate2";
            Main_Rotate2.Size = new System.Drawing.Size(298, 26);
            Main_Rotate2.Tag = "2";
            Main_Rotate2.Text = "Auto Rotate Group";
            // 
            // Main_Rotate3
            // 
            Main_Rotate3.Image = (System.Drawing.Image)resources.GetObject("Main_Rotate3.Image");
            Main_Rotate3.Name = "Main_Rotate3";
            Main_Rotate3.Size = new System.Drawing.Size(298, 26);
            Main_Rotate3.Tag = "3";
            Main_Rotate3.Text = "Auto Rotate Group - Repeat";
            // 
            // Main_Alerts
            // 
            Main_Alerts.AutoSize = false;
            Main_Alerts.BackgroundImageLayout = ImageLayout.Stretch;
            Main_Alerts.DisplayStyle = ToolStripItemDisplayStyle.Image;
            Main_Alerts.Image = (System.Drawing.Image)resources.GetObject("Main_Alerts.Image");
            Main_Alerts.ImageTransparentColor = System.Drawing.Color.Magenta;
            Main_Alerts.Name = "Main_Alerts";
            Main_Alerts.Size = new System.Drawing.Size(29, 28);
            Main_Alerts.Tag = "";
            Main_Alerts.ToolTipText = "Alerts";
            Main_Alerts.Click += Main_Alerts_Click;
            // 
            // Main_Chinese
            // 
            Main_Chinese.AutoSize = false;
            Main_Chinese.BackgroundImageLayout = ImageLayout.Stretch;
            Main_Chinese.DisplayStyle = ToolStripItemDisplayStyle.Image;
            Main_Chinese.Image = (System.Drawing.Image)resources.GetObject("Main_Chinese.Image");
            Main_Chinese.ImageTransparentColor = System.Drawing.Color.Magenta;
            Main_Chinese.Name = "Main_Chinese";
            Main_Chinese.Size = new System.Drawing.Size(29, 28);
            Main_Chinese.Tag = "";
            Main_Chinese.ToolTipText = "Trad/Simp Chinese";
            Main_Chinese.Click += Main_Chinese_Click;
            // 
            // toolStripSeparator4
            // 
            toolStripSeparator4.Name = "toolStripSeparator4";
            toolStripSeparator4.Size = new System.Drawing.Size(6, 31);
            // 
            // Main_Find
            // 
            Main_Find.AutoSize = false;
            Main_Find.BackgroundImageLayout = ImageLayout.Stretch;
            Main_Find.DisplayStyle = ToolStripItemDisplayStyle.Image;
            Main_Find.Image = (System.Drawing.Image)resources.GetObject("Main_Find.Image");
            Main_Find.ImageTransparentColor = System.Drawing.Color.Magenta;
            Main_Find.Name = "Main_Find";
            Main_Find.Size = new System.Drawing.Size(29, 28);
            Main_Find.Tag = "";
            Main_Find.ToolTipText = "Find";
            Main_Find.Click += Main_Find_Click;
            // 
            // Main_QuickFind
            // 
            Main_QuickFind.MaxDropDownItems = 12;
            Main_QuickFind.Name = "Main_QuickFind";
            Main_QuickFind.Size = new System.Drawing.Size(130, 31);
            Main_QuickFind.Tag = "";
            Main_QuickFind.Text = "Search Phrase";
            Main_QuickFind.ToolTipText = "Enter phrase and  press Keyboard Enter key";
            Main_QuickFind.Enter += Main_QuickFind_Enter;
            Main_QuickFind.Leave += Main_QuickFind_Leave;
            Main_QuickFind.KeyUp += Main_QuickFind_KeyUp;
            // 
            // Main_JumpA
            // 
            Main_JumpA.DisplayStyle = ToolStripItemDisplayStyle.Text;
            Main_JumpA.Font = new System.Drawing.Font("Tahoma", 8.25F);
            Main_JumpA.ImageTransparentColor = System.Drawing.Color.Magenta;
            Main_JumpA.Name = "Main_JumpA";
            Main_JumpA.Overflow = ToolStripItemOverflow.Never;
            Main_JumpA.Size = new System.Drawing.Size(29, 28);
            Main_JumpA.Text = "A";
            Main_JumpA.Click += Main_Jump_Click;
            // 
            // Main_JumpB
            // 
            Main_JumpB.DisplayStyle = ToolStripItemDisplayStyle.Text;
            Main_JumpB.Font = new System.Drawing.Font("Tahoma", 8.25F);
            Main_JumpB.ImageTransparentColor = System.Drawing.Color.Magenta;
            Main_JumpB.Name = "Main_JumpB";
            Main_JumpB.Overflow = ToolStripItemOverflow.Never;
            Main_JumpB.Size = new System.Drawing.Size(29, 28);
            Main_JumpB.Text = "B";
            Main_JumpB.Click += Main_Jump_Click;
            // 
            // Main_JumpC
            // 
            Main_JumpC.Font = new System.Drawing.Font("Tahoma", 8.25F);
            Main_JumpC.ImageTransparentColor = System.Drawing.Color.Magenta;
            Main_JumpC.Name = "Main_JumpC";
            Main_JumpC.Overflow = ToolStripItemOverflow.Never;
            Main_JumpC.Size = new System.Drawing.Size(29, 28);
            Main_JumpC.Text = "C";
            Main_JumpC.Click += Main_Jump_Click;
            // 
            // menuStripMain
            // 
            menuStripMain.ImageScalingSize = new System.Drawing.Size(24, 24);
            menuStripMain.Items.AddRange(new ToolStripItem[] { Menu_MainFile, Menu_MainEdit, Menu_MainView, Menu_MainOutput, Menu_MainTools, Menu_MainHelp });
            menuStripMain.Location = new System.Drawing.Point(0, 0);
            menuStripMain.Name = "menuStripMain";
            menuStripMain.Padding = new Padding(8, 3, 0, 3);
            menuStripMain.Size = new System.Drawing.Size(979, 30);
            menuStripMain.TabIndex = 0;
            menuStripMain.Text = "menuStrip1";
            // 
            // Menu_MainFile
            // 
            Menu_MainFile.DropDownItems.AddRange(new ToolStripItem[] { Menu_WorshipSessions, Menu_PraiseBookTemplates, toolStripSeparator20, Menu_ListingOfSelectedFolder, toolStripSeparator16, Menu_EditHistoryList, toolStripSeparator18, Menu_Exit });
            Menu_MainFile.Name = "Menu_MainFile";
            Menu_MainFile.Size = new System.Drawing.Size(46, 24);
            Menu_MainFile.Text = "&File";
            // 
            // Menu_WorshipSessions
            // 
            Menu_WorshipSessions.Image = (System.Drawing.Image)resources.GetObject("Menu_WorshipSessions.Image");
            Menu_WorshipSessions.Name = "Menu_WorshipSessions";
            Menu_WorshipSessions.Size = new System.Drawing.Size(260, 26);
            Menu_WorshipSessions.Text = "Worship Sessions...";
            Menu_WorshipSessions.Click += Menu_WorshipLists_Click;
            // 
            // Menu_PraiseBookTemplates
            // 
            Menu_PraiseBookTemplates.Image = (System.Drawing.Image)resources.GetObject("Menu_PraiseBookTemplates.Image");
            Menu_PraiseBookTemplates.Name = "Menu_PraiseBookTemplates";
            Menu_PraiseBookTemplates.Size = new System.Drawing.Size(260, 26);
            Menu_PraiseBookTemplates.Text = "PraiseBooks...";
            Menu_PraiseBookTemplates.Click += Menu_PraiseBooks_Click;
            // 
            // toolStripSeparator20
            // 
            toolStripSeparator20.Name = "toolStripSeparator20";
            toolStripSeparator20.Size = new System.Drawing.Size(257, 6);
            // 
            // Menu_ListingOfSelectedFolder
            // 
            Menu_ListingOfSelectedFolder.Name = "Menu_ListingOfSelectedFolder";
            Menu_ListingOfSelectedFolder.Size = new System.Drawing.Size(260, 26);
            Menu_ListingOfSelectedFolder.Text = "Listing of Selected Folder";
            Menu_ListingOfSelectedFolder.Click += Menu_ListingOfSelectedFolder_Click;
            // 
            // toolStripSeparator16
            // 
            toolStripSeparator16.Name = "toolStripSeparator16";
            toolStripSeparator16.Size = new System.Drawing.Size(257, 6);
            // 
            // Menu_EditHistoryList
            // 
            Menu_EditHistoryList.Name = "Menu_EditHistoryList";
            Menu_EditHistoryList.Size = new System.Drawing.Size(260, 26);
            Menu_EditHistoryList.Text = "Recent Edits";
            // 
            // toolStripSeparator18
            // 
            toolStripSeparator18.Name = "toolStripSeparator18";
            toolStripSeparator18.Size = new System.Drawing.Size(257, 6);
            // 
            // Menu_Exit
            // 
            Menu_Exit.Name = "Menu_Exit";
            Menu_Exit.ShortcutKeys = Keys.Alt | Keys.F4;
            Menu_Exit.Size = new System.Drawing.Size(260, 26);
            Menu_Exit.Text = "Exit";
            Menu_Exit.Click += Menu_Exit_Click;
            // 
            // Menu_MainEdit
            // 
            Menu_MainEdit.DropDownItems.AddRange(new ToolStripItem[] { Menu_AddSong, toolStripSeparator19, Menu_EditSong, Menu_CopySong, Menu_MoveSong, Menu_DeleteSong, toolStripSeparator41, Menu_SelectAll, Menu_Find, toolStripSeparator21, Menu_UseSongNumbering, Menu_ReArrangeSongFolders });
            Menu_MainEdit.Name = "Menu_MainEdit";
            Menu_MainEdit.Size = new System.Drawing.Size(49, 24);
            Menu_MainEdit.Text = "&Edit";
            // 
            // Ind_R2Bold
            // 
            Ind_R2Bold.CheckOnClick = true;
            Ind_R2Bold.DisplayStyle = ToolStripItemDisplayStyle.Image;
            Ind_R2Bold.Image = (System.Drawing.Image)resources.GetObject("Ind_R2Bold.Image");
            Ind_R2Bold.ImageTransparentColor = System.Drawing.Color.Magenta;
            Ind_R2Bold.Name = "Ind_R2Bold";
            Ind_R2Bold.Size = new System.Drawing.Size(29, 36);
            Ind_R2Bold.MouseUp += Ind_RegionsFormat_MouseUp;
            // 
            // Image_Import
            // 
            Image_Import.DisplayStyle = ToolStripItemDisplayStyle.Image;
            Image_Import.Image = (System.Drawing.Image)resources.GetObject("Image_Import.Image");
            Image_Import.ImageTransparentColor = System.Drawing.Color.Magenta;
            Image_Import.Name = "Image_Import";
            Image_Import.Size = new System.Drawing.Size(29, 34);
            Image_Import.Text = "Import An Image";
            Image_Import.Click += Image_Import_Click;
            // 
            // CMenuBible_CopyInfoScreen
            // 
            CMenuBible_CopyInfoScreen.Name = "CMenuBible_CopyInfoScreen";
            CMenuBible_CopyInfoScreen.Size = new System.Drawing.Size(204, 24);
            CMenuBible_CopyInfoScreen.Text = "Copy to &InfoScreen";
            CMenuBible_CopyInfoScreen.Click += CMenuBible_CopyInfoScreen_Click;
            // 
            // BibleUserLookup
            // 
            BibleUserLookup.Location = new System.Drawing.Point(86, 7);
            BibleUserLookup.Margin = new Padding(3, 5, 3, 5);
            BibleUserLookup.Name = "BibleUserLookup";
            BibleUserLookup.Size = new System.Drawing.Size(59, 27);
            BibleUserLookup.TabIndex = 1;
            BibleUserLookup.Enter += FormControl_Enter;
            BibleUserLookup.KeyDown += BibleUserLookup_KeyDown;
            BibleUserLookup.Leave += FormControl_Leave;
            // 
            // panelBible2
            // 
            panelBible2.Controls.Add(toolStripBible2);
            panelBible2.Location = new System.Drawing.Point(150, 5);
            panelBible2.Margin = new Padding(3, 5, 3, 5);
            panelBible2.Name = "panelBible2";
            panelBible2.Size = new System.Drawing.Size(32, 33);
            panelBible2.TabIndex = 7;
            // 
            // toolStripBible2
            // 
            toolStripBible2.AutoSize = false;
            toolStripBible2.CanOverflow = false;
            toolStripBible2.Dock = DockStyle.None;
            toolStripBible2.GripStyle = ToolStripGripStyle.Hidden;
            toolStripBible2.ImageScalingSize = new System.Drawing.Size(24, 24);
            toolStripBible2.Items.AddRange(new ToolStripItem[] { Bibles_Go });
            toolStripBible2.LayoutStyle = ToolStripLayoutStyle.HorizontalStackWithOverflow;
            toolStripBible2.Location = new System.Drawing.Point(0, -1);
            toolStripBible2.Name = "toolStripBible2";
            toolStripBible2.Padding = new Padding(0, 0, 2, 0);
            toolStripBible2.RenderMode = ToolStripRenderMode.System;
            toolStripBible2.Size = new System.Drawing.Size(32, 39);
            toolStripBible2.TabIndex = 0;
            // 
            // Bibles_Go
            // 
            Bibles_Go.DisplayStyle = ToolStripItemDisplayStyle.Image;
            Bibles_Go.Image = (System.Drawing.Image)resources.GetObject("Bibles_Go.Image");
            Bibles_Go.ImageTransparentColor = System.Drawing.Color.Magenta;
            Bibles_Go.Name = "Bibles_Go";
            Bibles_Go.Size = new System.Drawing.Size(29, 36);
            Bibles_Go.Tag = "tick";
            Bibles_Go.ToolTipText = "Select typed-in reference";
            Bibles_Go.MouseUp += Bibles_Btn_MouseUp;
            // 
            // BookLookup
            // 
            BookLookup.DropDownStyle = ComboBoxStyle.DropDownList;
            BookLookup.FormattingEnabled = true;
            BookLookup.Location = new System.Drawing.Point(3, 5);
            BookLookup.Margin = new Padding(3, 5, 3, 5);
            BookLookup.MaxDropDownItems = 40;
            BookLookup.Name = "BookLookup";
            BookLookup.Size = new System.Drawing.Size(75, 28);
            BookLookup.TabIndex = 0;
            BookLookup.SelectedIndexChanged += BookLookup_SelectedIndexChanged;
            // 
            // CMenuBible_Copy
            // 
            CMenuBible_Copy.Name = "CMenuBible_Copy";
            CMenuBible_Copy.Size = new System.Drawing.Size(204, 24);
            CMenuBible_Copy.Text = "&Copy";
            CMenuBible_Copy.Click += CMenuBible_Copy_Click;
            // 
            // TabBibleVersions
            // 
            TabBibleVersions.Alignment = TabAlignment.Bottom;
            TabBibleVersions.Appearance = TabAppearance.Buttons;
            TabBibleVersions.Controls.Add(tabPage1);
            TabBibleVersions.Location = new System.Drawing.Point(3, 144);
            TabBibleVersions.Margin = new Padding(3, 5, 3, 5);
            TabBibleVersions.Name = "TabBibleVersions";
            TabBibleVersions.SelectedIndex = 0;
            TabBibleVersions.Size = new System.Drawing.Size(112, 29);
            TabBibleVersions.TabIndex = 3;
            TabBibleVersions.Click += TabBibleVersions_Click;
            // 
            // tabPage1
            // 
            tabPage1.Location = new System.Drawing.Point(4, 4);
            tabPage1.Margin = new Padding(3, 5, 3, 5);
            tabPage1.Name = "tabPage1";
            tabPage1.Padding = new Padding(3, 5, 3, 5);
            tabPage1.Size = new System.Drawing.Size(104, 0);
            tabPage1.TabIndex = 0;
            tabPage1.Text = "No Bible";
            // 
            // tabImages
            // 
            tabImages.BackColor = System.Drawing.SystemColors.Control;
            tabImages.Controls.Add(flowLayoutImages);
            tabImages.Controls.Add(panelImagesTop);
            tabImages.Location = new System.Drawing.Point(4, 4);
            tabImages.Margin = new Padding(3, 5, 3, 5);
            tabImages.Name = "tabImages";
            tabImages.Padding = new Padding(3, 5, 3, 5);
            tabImages.Size = new System.Drawing.Size(336, 478);
            tabImages.TabIndex = 1;
            tabImages.Text = "Images";
            // 
            // flowLayoutImages
            // 
            flowLayoutImages.AutoScroll = true;
            flowLayoutImages.Location = new System.Drawing.Point(3, 53);
            flowLayoutImages.Margin = new Padding(3, 5, 3, 5);
            flowLayoutImages.Name = "flowLayoutImages";
            flowLayoutImages.Size = new System.Drawing.Size(79, 53);
            flowLayoutImages.TabIndex = 2;
            // 
            // panelImagesTop
            // 
            panelImagesTop.Controls.Add(panelImage1);
            panelImagesTop.Controls.Add(ImagesFolder);
            panelImagesTop.Dock = DockStyle.Top;
            panelImagesTop.Location = new System.Drawing.Point(3, 5);
            panelImagesTop.Margin = new Padding(3, 5, 3, 5);
            panelImagesTop.Name = "panelImagesTop";
            panelImagesTop.Size = new System.Drawing.Size(330, 40);
            panelImagesTop.TabIndex = 2;
            // 
            // panelImage1
            // 
            panelImage1.Controls.Add(toolStripImage1);
            panelImage1.Location = new System.Drawing.Point(154, 1);
            panelImage1.Margin = new Padding(3, 5, 3, 5);
            panelImage1.Name = "panelImage1";
            panelImage1.Size = new System.Drawing.Size(65, 29);
            panelImage1.TabIndex = 19;
            // 
            // toolStripImage1
            // 
            toolStripImage1.AutoSize = false;
            toolStripImage1.CanOverflow = false;
            toolStripImage1.Dock = DockStyle.None;
            toolStripImage1.GripStyle = ToolStripGripStyle.Hidden;
            toolStripImage1.ImageScalingSize = new System.Drawing.Size(24, 24);
            toolStripImage1.Items.AddRange(new ToolStripItem[] { Image_OpenFolder, Image_Import });
            toolStripImage1.Location = new System.Drawing.Point(0, -1);
            toolStripImage1.Name = "toolStripImage1";
            toolStripImage1.Padding = new Padding(0, 0, 2, 0);
            toolStripImage1.RenderMode = ToolStripRenderMode.System;
            toolStripImage1.Size = new System.Drawing.Size(72, 37);
            toolStripImage1.TabIndex = 5;
            // 
            // Image_OpenFolder
            // 
            Image_OpenFolder.DisplayStyle = ToolStripItemDisplayStyle.Image;
            Image_OpenFolder.Image = (System.Drawing.Image)resources.GetObject("Image_OpenFolder.Image");
            Image_OpenFolder.ImageTransparentColor = System.Drawing.Color.Magenta;
            Image_OpenFolder.Name = "Image_OpenFolder";
            Image_OpenFolder.Size = new System.Drawing.Size(29, 34);
            Image_OpenFolder.Tag = "";
            Image_OpenFolder.ToolTipText = "Open Folder";
            Image_OpenFolder.Click += Image_OpenFolder_Click;
            // 
            // ImagesFolder
            // 
            ImagesFolder.DropDownStyle = ComboBoxStyle.DropDownList;
            ImagesFolder.FormattingEnabled = true;
            ImagesFolder.Location = new System.Drawing.Point(0, 0);
            ImagesFolder.Margin = new Padding(3, 5, 3, 5);
            ImagesFolder.MaxDropDownItems = 12;
            ImagesFolder.Name = "ImagesFolder";
            ImagesFolder.Size = new System.Drawing.Size(146, 28);
            ImagesFolder.TabIndex = 5;
            ImagesFolder.SelectedIndexChanged += ImagesFolder_SelectedIndexChanged;
            // 
            // panelPowerpoint1
            // 
            panelPowerpoint1.Controls.Add(PowerpointFolder);
            panelPowerpoint1.Controls.Add(panelExternalFiles1);
            panelPowerpoint1.Dock = DockStyle.Top;
            panelPowerpoint1.Location = new System.Drawing.Point(0, 0);
            panelPowerpoint1.Margin = new Padding(3, 5, 3, 5);
            panelPowerpoint1.Name = "panelPowerpoint1";
            panelPowerpoint1.Size = new System.Drawing.Size(336, 40);
            panelPowerpoint1.TabIndex = 17;
            // 
            // PowerpointFolder
            // 
            PowerpointFolder.DropDownStyle = ComboBoxStyle.DropDownList;
            PowerpointFolder.FormattingEnabled = true;
            PowerpointFolder.Location = new System.Drawing.Point(3, 5);
            PowerpointFolder.Margin = new Padding(3, 5, 3, 5);
            PowerpointFolder.MaxDropDownItems = 12;
            PowerpointFolder.Name = "PowerpointFolder";
            PowerpointFolder.Size = new System.Drawing.Size(140, 28);
            PowerpointFolder.TabIndex = 17;
            PowerpointFolder.SelectedIndexChanged += PowerpointFolder_SelectedIndexChanged;
            // 
            // panelExternalFiles1
            // 
            panelExternalFiles1.Controls.Add(toolStripPowerpoint1);
            panelExternalFiles1.Location = new System.Drawing.Point(152, 5);
            panelExternalFiles1.Margin = new Padding(3, 5, 3, 5);
            panelExternalFiles1.Name = "panelExternalFiles1";
            panelExternalFiles1.Size = new System.Drawing.Size(104, 33);
            panelExternalFiles1.TabIndex = 13;
            // 
            // toolStripPowerpoint1
            // 
            toolStripPowerpoint1.AutoSize = false;
            toolStripPowerpoint1.CanOverflow = false;
            toolStripPowerpoint1.Dock = DockStyle.None;
            toolStripPowerpoint1.GripStyle = ToolStripGripStyle.Hidden;
            toolStripPowerpoint1.ImageScalingSize = new System.Drawing.Size(24, 24);
            toolStripPowerpoint1.Items.AddRange(new ToolStripItem[] { PP_ListType, PP_OpenFolder, PP_Import });
            toolStripPowerpoint1.LayoutStyle = ToolStripLayoutStyle.HorizontalStackWithOverflow;
            toolStripPowerpoint1.Location = new System.Drawing.Point(0, -1);
            toolStripPowerpoint1.Name = "toolStripPowerpoint1";
            toolStripPowerpoint1.Padding = new Padding(0, 0, 2, 0);
            toolStripPowerpoint1.RenderMode = ToolStripRenderMode.System;
            toolStripPowerpoint1.Size = new System.Drawing.Size(110, 39);
            toolStripPowerpoint1.TabIndex = 5;
            // 
            // PP_ListType
            // 
            PP_ListType.DisplayStyle = ToolStripItemDisplayStyle.Image;
            PP_ListType.DropDownItems.AddRange(new ToolStripItem[] { PP_ListStyle, PP_PreviewStyle });
            PP_ListType.Image = (System.Drawing.Image)resources.GetObject("PP_ListType.Image");
            PP_ListType.ImageTransparentColor = System.Drawing.Color.Magenta;
            PP_ListType.Name = "PP_ListType";
            PP_ListType.Size = new System.Drawing.Size(38, 36);
            PP_ListType.Tag = "0";
            PP_ListType.DropDownItemClicked += PP_Style_DropDownItemClicked;
            // 
            // PP_ListStyle
            // 
            PP_ListStyle.Image = (System.Drawing.Image)resources.GetObject("PP_ListStyle.Image");
            PP_ListStyle.Name = "PP_ListStyle";
            PP_ListStyle.Size = new System.Drawing.Size(222, 26);
            PP_ListStyle.Tag = "0";
            PP_ListStyle.Text = "Powerpoint Listing";
            // 
            // PP_PreviewStyle
            // 
            PP_PreviewStyle.Image = (System.Drawing.Image)resources.GetObject("PP_PreviewStyle.Image");
            PP_PreviewStyle.Name = "PP_PreviewStyle";
            PP_PreviewStyle.Size = new System.Drawing.Size(222, 26);
            PP_PreviewStyle.Tag = "1";
            PP_PreviewStyle.Text = "Powerpoint Preview";
            // 
            // PP_OpenFolder
            // 
            PP_OpenFolder.DisplayStyle = ToolStripItemDisplayStyle.Image;
            PP_OpenFolder.Image = (System.Drawing.Image)resources.GetObject("PP_OpenFolder.Image");
            PP_OpenFolder.ImageTransparentColor = System.Drawing.Color.Magenta;
            PP_OpenFolder.Name = "PP_OpenFolder";
            PP_OpenFolder.Size = new System.Drawing.Size(29, 36);
            PP_OpenFolder.Tag = "";
            PP_OpenFolder.ToolTipText = "Open Powerpoint Folder";
            PP_OpenFolder.Click += PP_Btn_Click;
            // 
            // PP_Import
            // 
            PP_Import.DisplayStyle = ToolStripItemDisplayStyle.Image;
            PP_Import.Image = (System.Drawing.Image)resources.GetObject("PP_Import.Image");
            PP_Import.ImageTransparentColor = System.Drawing.Color.Magenta;
            PP_Import.Name = "PP_Import";
            PP_Import.Size = new System.Drawing.Size(29, 36);
            PP_Import.ToolTipText = "Import a Powerpoint File into Folder";
            PP_Import.Click += PP_Import_Click;
            // 
            // toolStripSeparator24
            // 
            toolStripSeparator24.Name = "toolStripSeparator24";
            toolStripSeparator24.Size = new System.Drawing.Size(201, 6);
            // 
            // columnHeader36
            // 
            columnHeader36.Width = 0;
            // 
            // CMenuBible_AddRegion2
            // 
            CMenuBible_AddRegion2.Name = "CMenuBible_AddRegion2";
            CMenuBible_AddRegion2.Size = new System.Drawing.Size(204, 24);
            CMenuBible_AddRegion2.Text = "Add &Region 2";
            // 
            // tabBibles
            // 
            tabBibles.BackColor = System.Drawing.SystemColors.Control;
            tabBibles.Controls.Add(BibleText);
            tabBibles.Controls.Add(BibleUserLookup);
            tabBibles.Controls.Add(panelBible2);
            tabBibles.Controls.Add(BookLookup);
            tabBibles.Controls.Add(TabBibleVersions);
            tabBibles.Location = new System.Drawing.Point(4, 4);
            tabBibles.Margin = new Padding(3, 5, 3, 5);
            tabBibles.Name = "tabBibles";
            tabBibles.Size = new System.Drawing.Size(336, 478);
            tabBibles.TabIndex = 2;
            tabBibles.Text = "Bibles";
            // 
            // BibleText
            // 
            BibleText.BackColor = System.Drawing.SystemColors.Window;
            BibleText.ContextMenuStrip = CMenuBible;
            BibleText.EnableAutoDragDrop = true;
            BibleText.HideSelection = false;
            BibleText.Location = new System.Drawing.Point(3, 41);
            BibleText.Margin = new Padding(3, 5, 3, 5);
            BibleText.Name = "BibleText";
            BibleText.ReadOnly = true;
            BibleText.Size = new System.Drawing.Size(38, 63);
            BibleText.TabIndex = 2;
            BibleText.Text = "";
            BibleText.Enter += FormControl_Enter;
            BibleText.KeyUp += BibleText_KeyUp;
            BibleText.Leave += FormControl_Leave;
            BibleText.MouseDown += BibleText_MouseDown;
            BibleText.MouseUp += BibleText_MouseUp;
            // 
            // CMenuBible
            // 
            CMenuBible.ImageScalingSize = new System.Drawing.Size(24, 24);
            CMenuBible.Items.AddRange(new ToolStripItem[] { CMenuBible_SelectAll, CMenuBible_UnselectAll, CMenuBible_AddShow, toolStripSeparator17, CMenuBible_AddRegion2, toolStripSeparator24, CMenuBible_Copy, CMenuBible_CopyInfoScreen });
            CMenuBible.Name = "ContextMenuBibleText";
            CMenuBible.Size = new System.Drawing.Size(205, 160);
            // 
            // CMenuBible_SelectAll
            // 
            CMenuBible_SelectAll.Name = "CMenuBible_SelectAll";
            CMenuBible_SelectAll.Size = new System.Drawing.Size(204, 24);
            CMenuBible_SelectAll.Text = "Select &All";
            CMenuBible_SelectAll.Click += CMenuBible_SelectAll_Click;
            // 
            // CMenuBible_UnselectAll
            // 
            CMenuBible_UnselectAll.Name = "CMenuBible_UnselectAll";
            CMenuBible_UnselectAll.Size = new System.Drawing.Size(204, 24);
            CMenuBible_UnselectAll.Text = "&Unselect All";
            CMenuBible_UnselectAll.Click += CMenuBible_UnselectAll_Click;
            // 
            // CMenuBible_AddShow
            // 
            CMenuBible_AddShow.Name = "CMenuBible_AddShow";
            CMenuBible_AddShow.Size = new System.Drawing.Size(204, 24);
            CMenuBible_AddShow.Text = "Add && &Show";
            CMenuBible_AddShow.Click += CMenuBible_AddShow_Click;
            // 
            // toolStripSeparator17
            // 
            toolStripSeparator17.Name = "toolStripSeparator17";
            toolStripSeparator17.Size = new System.Drawing.Size(201, 6);
            // 
            // tabMedia
            // 
            tabMedia.BackColor = System.Drawing.SystemColors.Control;
            tabMedia.Controls.Add(panel11);
            tabMedia.Controls.Add(MediaList);
            tabMedia.Location = new System.Drawing.Point(4, 4);
            tabMedia.Margin = new Padding(3, 5, 3, 5);
            tabMedia.Name = "tabMedia";
            tabMedia.Size = new System.Drawing.Size(336, 478);
            tabMedia.TabIndex = 6;
            tabMedia.Text = "Media";
            // 
            // panel11
            // 
            panel11.Controls.Add(panelMedia1);
            panel11.Controls.Add(MediaFolder);
            panel11.Dock = DockStyle.Top;
            panel11.Location = new System.Drawing.Point(0, 0);
            panel11.Margin = new Padding(3, 5, 3, 5);
            panel11.Name = "panel11";
            panel11.Size = new System.Drawing.Size(336, 40);
            panel11.TabIndex = 18;
            // 
            // panelMedia1
            // 
            panelMedia1.Controls.Add(toolStripMedia1);
            panelMedia1.Location = new System.Drawing.Point(153, 5);
            panelMedia1.Margin = new Padding(3, 5, 3, 5);
            panelMedia1.Name = "panelMedia1";
            panelMedia1.Size = new System.Drawing.Size(66, 29);
            panelMedia1.TabIndex = 18;
            // 
            // toolStripMedia1
            // 
            toolStripMedia1.AutoSize = false;
            toolStripMedia1.CanOverflow = false;
            toolStripMedia1.Dock = DockStyle.None;
            toolStripMedia1.GripStyle = ToolStripGripStyle.Hidden;
            toolStripMedia1.ImageScalingSize = new System.Drawing.Size(24, 24);
            toolStripMedia1.Items.AddRange(new ToolStripItem[] { Media_OpenFolder, Media_Import });
            toolStripMedia1.Location = new System.Drawing.Point(0, -1);
            toolStripMedia1.Name = "toolStripMedia1";
            toolStripMedia1.Padding = new Padding(0, 0, 2, 0);
            toolStripMedia1.RenderMode = ToolStripRenderMode.System;
            toolStripMedia1.Size = new System.Drawing.Size(71, 37);
            toolStripMedia1.TabIndex = 5;
            // 
            // Media_OpenFolder
            // 
            Media_OpenFolder.DisplayStyle = ToolStripItemDisplayStyle.Image;
            Media_OpenFolder.Image = (System.Drawing.Image)resources.GetObject("Media_OpenFolder.Image");
            Media_OpenFolder.ImageTransparentColor = System.Drawing.Color.Magenta;
            Media_OpenFolder.Name = "Media_OpenFolder";
            Media_OpenFolder.Size = new System.Drawing.Size(29, 34);
            Media_OpenFolder.Tag = "";
            Media_OpenFolder.ToolTipText = "Open Folder";
            Media_OpenFolder.Click += Media_OpenFolder_Click;
            // 
            // Media_Import
            // 
            Media_Import.DisplayStyle = ToolStripItemDisplayStyle.Image;
            Media_Import.Image = (System.Drawing.Image)resources.GetObject("Media_Import.Image");
            Media_Import.ImageTransparentColor = System.Drawing.Color.Magenta;
            Media_Import.Name = "Media_Import";
            Media_Import.Size = new System.Drawing.Size(29, 34);
            Media_Import.ToolTipText = "Import A Media File into Folder";
            Media_Import.Click += Media_Import_Click;
            // 
            // MediaFolder
            // 
            MediaFolder.DropDownStyle = ComboBoxStyle.DropDownList;
            MediaFolder.FormattingEnabled = true;
            MediaFolder.Location = new System.Drawing.Point(3, 5);
            MediaFolder.Margin = new Padding(3, 5, 3, 5);
            MediaFolder.MaxDropDownItems = 12;
            MediaFolder.Name = "MediaFolder";
            MediaFolder.Size = new System.Drawing.Size(140, 28);
            MediaFolder.TabIndex = 17;
            MediaFolder.SelectedIndexChanged += MediaFolder_SelectedIndexChanged;
            // 
            // MediaList
            // 
            MediaList.Columns.AddRange(new ColumnHeader[] { columnHeader37, columnHeader38, columnHeader39, columnHeader40, columnHeader41, columnHeader42, columnHeader43 });
            MediaList.ContextMenuStrip = CMenuFiles;
            MediaList.FullRowSelect = true;
            MediaList.HeaderStyle = ColumnHeaderStyle.None;
            MediaList.LabelWrap = false;
            MediaList.Location = new System.Drawing.Point(3, 41);
            MediaList.Margin = new Padding(3, 5, 3, 5);
            MediaList.Name = "MediaList";
            MediaList.ShowItemToolTips = true;
            MediaList.Size = new System.Drawing.Size(45, 112);
            MediaList.TabIndex = 17;
            MediaList.UseCompatibleStateImageBehavior = false;
            MediaList.View = View.Details;
            MediaList.ItemDrag += MediaList_ItemDrag;
            MediaList.Enter += FormControl_Enter;
            MediaList.KeyUp += MediaList_KeyUp;
            MediaList.Leave += FormControl_Leave;
            MediaList.MouseDoubleClick += MediaList_MouseDoubleClick;
            MediaList.MouseUp += MediaList_MouseUp;
            // 
            // columnHeader38
            // 
            columnHeader38.Width = 0;
            // 
            // columnHeader39
            // 
            columnHeader39.Width = 0;
            // 
            // columnHeader40
            // 
            columnHeader40.Width = 0;
            // 
            // columnHeader41
            // 
            columnHeader41.TextAlign = HorizontalAlignment.Center;
            columnHeader41.Width = 0;
            // 
            // columnHeader42
            // 
            columnHeader42.Width = 0;
            // 
            // columnHeader43
            // 
            columnHeader43.Width = 0;
            // 
            // CMenuFiles
            // 
            CMenuFiles.ImageScalingSize = new System.Drawing.Size(24, 24);
            CMenuFiles.Items.AddRange(new ToolStripItem[] { CMenuFiles_SelectAll, CMenuFiles_UnselectAll, CMenuFiles_AddShow, toolStripSeparator12, CMenuFiles_Edit, CMenuFiles_Copy, toolStripSeparator25, CMenuFiles_Refresh });
            CMenuFiles.Name = "ContextMenuBibleText";
            CMenuFiles.Size = new System.Drawing.Size(163, 160);
            // 
            // CMenuFiles_SelectAll
            // 
            CMenuFiles_SelectAll.Name = "CMenuFiles_SelectAll";
            CMenuFiles_SelectAll.Size = new System.Drawing.Size(162, 24);
            CMenuFiles_SelectAll.Text = "Select &All";
            CMenuFiles_SelectAll.Click += CMenuFiles_SelectAll_Click;
            // 
            // CMenuFiles_UnselectAll
            // 
            CMenuFiles_UnselectAll.Name = "CMenuFiles_UnselectAll";
            CMenuFiles_UnselectAll.Size = new System.Drawing.Size(162, 24);
            CMenuFiles_UnselectAll.Text = "&Unselect All";
            CMenuFiles_UnselectAll.Click += CMenuFiles_UnselectAll_Click;
            // 
            // CMenuFiles_AddShow
            // 
            CMenuFiles_AddShow.Name = "CMenuFiles_AddShow";
            CMenuFiles_AddShow.Size = new System.Drawing.Size(162, 24);
            CMenuFiles_AddShow.Text = "Add && &Show";
            CMenuFiles_AddShow.Click += CMenuFiles_AddShow_Click;
            // 
            // toolStripSeparator12
            // 
            toolStripSeparator12.Name = "toolStripSeparator12";
            toolStripSeparator12.Size = new System.Drawing.Size(159, 6);
            // 
            // CMenuFiles_Edit
            // 
            CMenuFiles_Edit.Name = "CMenuFiles_Edit";
            CMenuFiles_Edit.Size = new System.Drawing.Size(162, 24);
            CMenuFiles_Edit.Text = "Edit";
            CMenuFiles_Edit.Click += CMenuFiles_Edit_Click;
            // 
            // CMenuFiles_Copy
            // 
            CMenuFiles_Copy.Name = "CMenuFiles_Copy";
            CMenuFiles_Copy.Size = new System.Drawing.Size(162, 24);
            CMenuFiles_Copy.Text = "Copy";
            CMenuFiles_Copy.Click += CMenuFiles_Copy_Click;
            // 
            // toolStripSeparator25
            // 
            toolStripSeparator25.Name = "toolStripSeparator25";
            toolStripSeparator25.Size = new System.Drawing.Size(159, 6);
            // 
            // CMenuFiles_Refresh
            // 
            CMenuFiles_Refresh.Name = "CMenuFiles_Refresh";
            CMenuFiles_Refresh.Size = new System.Drawing.Size(162, 24);
            CMenuFiles_Refresh.Text = "Refresh";
            CMenuFiles_Refresh.Click += CMenuFiles_Refresh_Click;
            // 
            // Def_SaveTemplate
            // 
            Def_SaveTemplate.DisplayStyle = ToolStripItemDisplayStyle.Image;
            Def_SaveTemplate.Image = (System.Drawing.Image)resources.GetObject("Def_SaveTemplate.Image");
            Def_SaveTemplate.ImageTransparentColor = System.Drawing.Color.Magenta;
            Def_SaveTemplate.Name = "Def_SaveTemplate";
            Def_SaveTemplate.Size = new System.Drawing.Size(29, 36);
            Def_SaveTemplate.ToolTipText = "Save Settings as a Template";
            Def_SaveTemplate.MouseUp += Def_Items_MouseUp;
            // 
            // DefgroupBox2
            // 
            DefgroupBox2.Controls.Add(panelDef4);
            DefgroupBox2.Controls.Add(panelDef3);
            DefgroupBox2.Location = new System.Drawing.Point(3, 159);
            DefgroupBox2.Margin = new Padding(3, 5, 3, 5);
            DefgroupBox2.Name = "DefgroupBox2";
            DefgroupBox2.Padding = new Padding(3, 5, 3, 5);
            DefgroupBox2.Size = new System.Drawing.Size(296, 109);
            DefgroupBox2.TabIndex = 2;
            DefgroupBox2.TabStop = false;
            DefgroupBox2.Text = "Default Background";
            // 
            // panelDef4
            // 
            panelDef4.Controls.Add(toolStripDef4);
            panelDef4.Location = new System.Drawing.Point(8, 68);
            panelDef4.Margin = new Padding(3, 5, 3, 5);
            panelDef4.Name = "panelDef4";
            panelDef4.Size = new System.Drawing.Size(272, 33);
            panelDef4.TabIndex = 11;
            // 
            // toolStripDef4
            // 
            toolStripDef4.AutoSize = false;
            toolStripDef4.CanOverflow = false;
            toolStripDef4.Dock = DockStyle.None;
            toolStripDef4.GripStyle = ToolStripGripStyle.Hidden;
            toolStripDef4.ImageScalingSize = new System.Drawing.Size(24, 24);
            toolStripDef4.Items.AddRange(new ToolStripItem[] { Def_TransItem, Def_TransSlides });
            toolStripDef4.LayoutStyle = ToolStripLayoutStyle.HorizontalStackWithOverflow;
            toolStripDef4.Location = new System.Drawing.Point(0, -1);
            toolStripDef4.Name = "toolStripDef4";
            toolStripDef4.Padding = new Padding(0, 0, 2, 0);
            toolStripDef4.RenderMode = ToolStripRenderMode.System;
            toolStripDef4.Size = new System.Drawing.Size(280, 39);
            toolStripDef4.TabIndex = 5;
            // 
            // Def_TransItem
            // 
            Def_TransItem.AutoSize = false;
            Def_TransItem.DropDownStyle = ComboBoxStyle.DropDownList;
            Def_TransItem.MaxDropDownItems = 24;
            Def_TransItem.Name = "Def_TransItem";
            Def_TransItem.Size = new System.Drawing.Size(132, 28);
            Def_TransItem.ToolTipText = "Item Transition";
            Def_TransItem.SelectedIndexChanged += Def_TransSelectedIndexChanged;
            // 
            // Def_TransSlides
            // 
            Def_TransSlides.AutoSize = false;
            Def_TransSlides.DropDownStyle = ComboBoxStyle.DropDownList;
            Def_TransSlides.MaxDropDownItems = 24;
            Def_TransSlides.Name = "Def_TransSlides";
            Def_TransSlides.Size = new System.Drawing.Size(132, 28);
            Def_TransSlides.ToolTipText = "Slide Transition";
            Def_TransSlides.SelectedIndexChanged += Def_TransSelectedIndexChanged;
            // 
            // panelDef3
            // 
            panelDef3.Controls.Add(toolStripDef3);
            panelDef3.Location = new System.Drawing.Point(8, 28);
            panelDef3.Margin = new Padding(3, 5, 3, 5);
            panelDef3.Name = "panelDef3";
            panelDef3.Size = new System.Drawing.Size(272, 33);
            panelDef3.TabIndex = 10;
            // 
            // toolStripDef3
            // 
            toolStripDef3.AutoSize = false;
            toolStripDef3.CanOverflow = false;
            toolStripDef3.Dock = DockStyle.None;
            toolStripDef3.GripStyle = ToolStripGripStyle.Hidden;
            toolStripDef3.ImageScalingSize = new System.Drawing.Size(24, 24);
            toolStripDef3.Items.AddRange(new ToolStripItem[] { Def_ImageMode, Def_NoImage, Def_BackColour, Def_AssignMedia });
            toolStripDef3.LayoutStyle = ToolStripLayoutStyle.HorizontalStackWithOverflow;
            toolStripDef3.Location = new System.Drawing.Point(0, -1);
            toolStripDef3.Name = "toolStripDef3";
            toolStripDef3.Padding = new Padding(0, 0, 2, 0);
            toolStripDef3.RenderMode = ToolStripRenderMode.System;
            toolStripDef3.Size = new System.Drawing.Size(280, 39);
            toolStripDef3.TabIndex = 0;
            // 
            // Def_ImageMode
            // 
            Def_ImageMode.DisplayStyle = ToolStripItemDisplayStyle.Image;
            Def_ImageMode.DropDownItems.AddRange(new ToolStripItem[] { Def_ImageTile, Def_ImageCentre, Def_ImageBestFit });
            Def_ImageMode.Image = (System.Drawing.Image)resources.GetObject("Def_ImageMode.Image");
            Def_ImageMode.ImageTransparentColor = System.Drawing.Color.Magenta;
            Def_ImageMode.Name = "Def_ImageMode";
            Def_ImageMode.Size = new System.Drawing.Size(38, 36);
            Def_ImageMode.Tag = "2";
            Def_ImageMode.ToolTipText = "Background Picture Format";
            Def_ImageMode.DropDownItemClicked += Def_ImageMode_DropDownItemClicked;
            // 
            // Def_ImageTile
            // 
            Def_ImageTile.Image = (System.Drawing.Image)resources.GetObject("Def_ImageTile.Image");
            Def_ImageTile.Name = "Def_ImageTile";
            Def_ImageTile.Size = new System.Drawing.Size(186, 26);
            Def_ImageTile.Tag = "0";
            Def_ImageTile.Text = "Tile Image";
            // 
            // Def_ImageCentre
            // 
            Def_ImageCentre.Image = (System.Drawing.Image)resources.GetObject("Def_ImageCentre.Image");
            Def_ImageCentre.Name = "Def_ImageCentre";
            Def_ImageCentre.Size = new System.Drawing.Size(186, 26);
            Def_ImageCentre.Tag = "1";
            Def_ImageCentre.Text = "Centre Image";
            // 
            // Def_ImageBestFit
            // 
            Def_ImageBestFit.Image = (System.Drawing.Image)resources.GetObject("Def_ImageBestFit.Image");
            Def_ImageBestFit.Name = "Def_ImageBestFit";
            Def_ImageBestFit.Size = new System.Drawing.Size(186, 26);
            Def_ImageBestFit.Tag = "2";
            Def_ImageBestFit.Text = "Best Fit Image";
            // 
            // Def_NoImage
            // 
            Def_NoImage.DisplayStyle = ToolStripItemDisplayStyle.Image;
            Def_NoImage.Image = (System.Drawing.Image)resources.GetObject("Def_NoImage.Image");
            Def_NoImage.ImageTransparentColor = System.Drawing.Color.Magenta;
            Def_NoImage.Name = "Def_NoImage";
            Def_NoImage.Size = new System.Drawing.Size(29, 36);
            Def_NoImage.ToolTipText = "No Background Picture";
            Def_NoImage.MouseUp += Def_Items_MouseUp;
            // 
            // Def_BackColour
            // 
            Def_BackColour.AutoSize = false;
            Def_BackColour.BackColor = System.Drawing.SystemColors.ActiveBorder;
            Def_BackColour.DisplayStyle = ToolStripItemDisplayStyle.Text;
            Def_BackColour.ImageTransparentColor = System.Drawing.Color.Magenta;
            Def_BackColour.Name = "Def_BackColour";
            Def_BackColour.Size = new System.Drawing.Size(46, 22);
            Def_BackColour.Text = "Colours";
            Def_BackColour.ToolTipText = "Background Colours and Patterns";
            Def_BackColour.MouseUp += Def_Items_MouseUp;
            // 
            // Def_AssignMedia
            // 
            Def_AssignMedia.AutoSize = false;
            Def_AssignMedia.DisplayStyle = ToolStripItemDisplayStyle.Text;
            Def_AssignMedia.ImageTransparentColor = System.Drawing.Color.Magenta;
            Def_AssignMedia.Name = "Def_AssignMedia";
            Def_AssignMedia.Size = new System.Drawing.Size(106, 22);
            Def_AssignMedia.Text = "Media";
            Def_AssignMedia.MouseUp += Def_Items_MouseUp;
            // 
            // Def_LoadTemplate
            // 
            Def_LoadTemplate.DisplayStyle = ToolStripItemDisplayStyle.Image;
            Def_LoadTemplate.Image = (System.Drawing.Image)resources.GetObject("Def_LoadTemplate.Image");
            Def_LoadTemplate.ImageTransparentColor = System.Drawing.Color.Magenta;
            Def_LoadTemplate.Name = "Def_LoadTemplate";
            Def_LoadTemplate.Size = new System.Drawing.Size(29, 36);
            Def_LoadTemplate.ToolTipText = "Load Settings Template";
            Def_LoadTemplate.MouseUp += Def_Items_MouseUp;
            // 
            // toolStripInd6
            // 
            toolStripInd6.AutoSize = false;
            toolStripInd6.CanOverflow = false;
            toolStripInd6.Dock = DockStyle.None;
            toolStripInd6.GripStyle = ToolStripGripStyle.Hidden;
            toolStripInd6.ImageScalingSize = new System.Drawing.Size(24, 24);
            toolStripInd6.Items.AddRange(new ToolStripItem[] { Ind_R2Bold, Ind_R2Italics, Ind_R2Underline, toolStripSeparator15, Ind_R2Align, Ind_R2Colour });
            toolStripInd6.LayoutStyle = ToolStripLayoutStyle.HorizontalStackWithOverflow;
            toolStripInd6.Location = new System.Drawing.Point(0, -1);
            toolStripInd6.Name = "toolStripInd6";
            toolStripInd6.Padding = new Padding(0, 0, 2, 0);
            toolStripInd6.RenderMode = ToolStripRenderMode.System;
            toolStripInd6.Size = new System.Drawing.Size(207, 39);
            toolStripInd6.TabIndex = 0;
            // 
            // toolStripDefTemplates
            // 
            toolStripDefTemplates.AutoSize = false;
            toolStripDefTemplates.CanOverflow = false;
            toolStripDefTemplates.Dock = DockStyle.None;
            toolStripDefTemplates.GripStyle = ToolStripGripStyle.Hidden;
            toolStripDefTemplates.ImageScalingSize = new System.Drawing.Size(24, 24);
            toolStripDefTemplates.Items.AddRange(new ToolStripItem[] { Def_LoadTemplate, Def_SaveTemplate });
            toolStripDefTemplates.LayoutStyle = ToolStripLayoutStyle.HorizontalStackWithOverflow;
            toolStripDefTemplates.Location = new System.Drawing.Point(0, -1);
            toolStripDefTemplates.Name = "toolStripDefTemplates";
            toolStripDefTemplates.Padding = new Padding(0, 0, 2, 0);
            toolStripDefTemplates.RenderMode = ToolStripRenderMode.System;
            toolStripDefTemplates.Size = new System.Drawing.Size(66, 39);
            toolStripDefTemplates.TabIndex = 0;
            // 
            // panelDefTemplate
            // 
            panelDefTemplate.Controls.Add(toolStripDefTemplates);
            panelDefTemplate.Location = new System.Drawing.Point(233, 5);
            panelDefTemplate.Margin = new Padding(3, 5, 3, 5);
            panelDefTemplate.Name = "panelDefTemplate";
            panelDefTemplate.Size = new System.Drawing.Size(64, 33);
            panelDefTemplate.TabIndex = 11;
            // 
            // tabDefault
            // 
            tabDefault.BackColor = System.Drawing.SystemColors.Control;
            tabDefault.Controls.Add(DefPanel);
            tabDefault.Location = new System.Drawing.Point(4, 4);
            tabDefault.Margin = new Padding(3, 5, 3, 5);
            tabDefault.Name = "tabDefault";
            tabDefault.Size = new System.Drawing.Size(336, 478);
            tabDefault.TabIndex = 3;
            tabDefault.Text = "Default";
            // 
            // DefPanel
            // 
            DefPanel.AutoScroll = true;
            DefPanel.Controls.Add(panelDefTemplate);
            DefPanel.Controls.Add(DefApplyDefaultsBtn);
            DefPanel.Controls.Add(DefgroupBox2);
            DefPanel.Controls.Add(DefgroupBox3);
            DefPanel.Controls.Add(DefgroupBox1);
            DefPanel.Location = new System.Drawing.Point(3, 5);
            DefPanel.Margin = new Padding(3, 5, 3, 5);
            DefPanel.Name = "DefPanel";
            DefPanel.Size = new System.Drawing.Size(306, 427);
            DefPanel.TabIndex = 3;
            // 
            // DefgroupBox3
            // 
            DefgroupBox3.Controls.Add(panel21);
            DefgroupBox3.Controls.Add(Def_PanelHeight);
            DefgroupBox3.Controls.Add(panelDef5);
            DefgroupBox3.Controls.Add(panelDef6);
            DefgroupBox3.Controls.Add(label5);
            DefgroupBox3.Location = new System.Drawing.Point(3, 272);
            DefgroupBox3.Margin = new Padding(3, 5, 3, 5);
            DefgroupBox3.Name = "DefgroupBox3";
            DefgroupBox3.Padding = new Padding(3, 5, 3, 5);
            DefgroupBox3.Size = new System.Drawing.Size(296, 147);
            DefgroupBox3.TabIndex = 3;
            DefgroupBox3.TabStop = false;
            DefgroupBox3.Text = "Display Panel";
            // 
            // panel21
            // 
            panel21.Controls.Add(toolStripDef7);
            panel21.Location = new System.Drawing.Point(8, 107);
            panel21.Margin = new Padding(3, 5, 3, 5);
            panel21.Name = "panel21";
            panel21.Size = new System.Drawing.Size(279, 33);
            panel21.TabIndex = 13;
            // 
            // toolStripDef7
            // 
            toolStripDef7.AutoSize = false;
            toolStripDef7.CanOverflow = false;
            toolStripDef7.Dock = DockStyle.None;
            toolStripDef7.GripStyle = ToolStripGripStyle.Hidden;
            toolStripDef7.ImageScalingSize = new System.Drawing.Size(24, 24);
            toolStripDef7.Items.AddRange(new ToolStripItem[] { Def_PanelFontBold, Def_PanelFontItalics, Def_PanelFontUnderline, Def_PanelFontShadow, Def_PanelFontOutline, Def_PanelFontList });
            toolStripDef7.LayoutStyle = ToolStripLayoutStyle.HorizontalStackWithOverflow;
            toolStripDef7.Location = new System.Drawing.Point(0, -1);
            toolStripDef7.Name = "toolStripDef7";
            toolStripDef7.Padding = new Padding(0, 0, 2, 0);
            toolStripDef7.RenderMode = ToolStripRenderMode.System;
            toolStripDef7.Size = new System.Drawing.Size(279, 39);
            toolStripDef7.TabIndex = 0;
            // 
            // Def_PanelFontBold
            // 
            Def_PanelFontBold.CheckOnClick = true;
            Def_PanelFontBold.DisplayStyle = ToolStripItemDisplayStyle.Image;
            Def_PanelFontBold.Image = (System.Drawing.Image)resources.GetObject("Def_PanelFontBold.Image");
            Def_PanelFontBold.ImageTransparentColor = System.Drawing.Color.Magenta;
            Def_PanelFontBold.Name = "Def_PanelFontBold";
            Def_PanelFontBold.Size = new System.Drawing.Size(29, 36);
            Def_PanelFontBold.MouseUp += Def_Items_MouseUp;
            // 
            // Def_PanelFontItalics
            // 
            Def_PanelFontItalics.CheckOnClick = true;
            Def_PanelFontItalics.DisplayStyle = ToolStripItemDisplayStyle.Image;
            Def_PanelFontItalics.Image = (System.Drawing.Image)resources.GetObject("Def_PanelFontItalics.Image");
            Def_PanelFontItalics.ImageTransparentColor = System.Drawing.Color.Magenta;
            Def_PanelFontItalics.Name = "Def_PanelFontItalics";
            Def_PanelFontItalics.Size = new System.Drawing.Size(29, 36);
            Def_PanelFontItalics.MouseUp += Def_Items_MouseUp;
            // 
            // Def_PanelFontUnderline
            // 
            Def_PanelFontUnderline.CheckOnClick = true;
            Def_PanelFontUnderline.DisplayStyle = ToolStripItemDisplayStyle.Image;
            Def_PanelFontUnderline.Image = (System.Drawing.Image)resources.GetObject("Def_PanelFontUnderline.Image");
            Def_PanelFontUnderline.ImageTransparentColor = System.Drawing.Color.Magenta;
            Def_PanelFontUnderline.Name = "Def_PanelFontUnderline";
            Def_PanelFontUnderline.Size = new System.Drawing.Size(29, 36);
            Def_PanelFontUnderline.MouseUp += Def_Items_MouseUp;
            // 
            // Def_PanelFontShadow
            // 
            Def_PanelFontShadow.CheckOnClick = true;
            Def_PanelFontShadow.DisplayStyle = ToolStripItemDisplayStyle.Image;
            Def_PanelFontShadow.Image = (System.Drawing.Image)resources.GetObject("Def_PanelFontShadow.Image");
            Def_PanelFontShadow.ImageTransparentColor = System.Drawing.Color.Magenta;
            Def_PanelFontShadow.Name = "Def_PanelFontShadow";
            Def_PanelFontShadow.Size = new System.Drawing.Size(29, 36);
            Def_PanelFontShadow.MouseUp += Def_Items_MouseUp;
            // 
            // Def_PanelFontOutline
            // 
            Def_PanelFontOutline.CheckOnClick = true;
            Def_PanelFontOutline.DisplayStyle = ToolStripItemDisplayStyle.Image;
            Def_PanelFontOutline.Image = (System.Drawing.Image)resources.GetObject("Def_PanelFontOutline.Image");
            Def_PanelFontOutline.ImageTransparentColor = System.Drawing.Color.Magenta;
            Def_PanelFontOutline.Name = "Def_PanelFontOutline";
            Def_PanelFontOutline.Size = new System.Drawing.Size(29, 36);
            Def_PanelFontOutline.MouseUp += Def_Items_MouseUp;
            // 
            // Def_PanelFontList
            // 
            Def_PanelFontList.DropDownWidth = 150;
            Def_PanelFontList.Name = "Def_PanelFontList";
            Def_PanelFontList.Size = new System.Drawing.Size(122, 39);
            Def_PanelFontList.SelectedIndexChanged += Def_PanelFontList_SelectedIndexChanged;
            // 
            // Def_PanelHeight
            // 
            Def_PanelHeight.Location = new System.Drawing.Point(238, 69);
            Def_PanelHeight.Margin = new Padding(3, 5, 3, 5);
            Def_PanelHeight.Maximum = new decimal(new int[] { 20, 0, 0, 0 });
            Def_PanelHeight.Minimum = new decimal(new int[] { 6, 0, 0, 0 });
            Def_PanelHeight.Name = "Def_PanelHeight";
            Def_PanelHeight.Size = new System.Drawing.Size(49, 27);
            Def_PanelHeight.TabIndex = 12;
            Def_PanelHeight.Value = new decimal(new int[] { 8, 0, 0, 0 });
            Def_PanelHeight.MouseUp += Def_PanelHeight_MouseUp;
            // 
            // panelDef5
            // 
            panelDef5.Controls.Add(toolStripDef5);
            panelDef5.Location = new System.Drawing.Point(8, 28);
            panelDef5.Margin = new Padding(3, 5, 3, 5);
            panelDef5.Name = "panelDef5";
            panelDef5.Size = new System.Drawing.Size(272, 33);
            panelDef5.TabIndex = 10;
            // 
            // toolStripDef5
            // 
            toolStripDef5.AutoSize = false;
            toolStripDef5.CanOverflow = false;
            toolStripDef5.Dock = DockStyle.None;
            toolStripDef5.GripStyle = ToolStripGripStyle.Hidden;
            toolStripDef5.ImageScalingSize = new System.Drawing.Size(24, 24);
            toolStripDef5.Items.AddRange(new ToolStripItem[] { Def_PanelAsR1, Def_PanelTextColour, toolStripSeparator14, Def_PanelTransparent, Def_PanelBackColour });
            toolStripDef5.LayoutStyle = ToolStripLayoutStyle.HorizontalStackWithOverflow;
            toolStripDef5.Location = new System.Drawing.Point(0, -1);
            toolStripDef5.Name = "toolStripDef5";
            toolStripDef5.Padding = new Padding(0, 0, 2, 0);
            toolStripDef5.RenderMode = ToolStripRenderMode.System;
            toolStripDef5.Size = new System.Drawing.Size(272, 39);
            toolStripDef5.TabIndex = 0;
            // 
            // Def_PanelAsR1
            // 
            Def_PanelAsR1.CheckOnClick = true;
            Def_PanelAsR1.DisplayStyle = ToolStripItemDisplayStyle.Image;
            Def_PanelAsR1.Image = (System.Drawing.Image)resources.GetObject("Def_PanelAsR1.Image");
            Def_PanelAsR1.ImageTransparentColor = System.Drawing.Color.Magenta;
            Def_PanelAsR1.Name = "Def_PanelAsR1";
            Def_PanelAsR1.Size = new System.Drawing.Size(29, 36);
            Def_PanelAsR1.ToolTipText = "Text Colour As Region 1";
            Def_PanelAsR1.MouseUp += Def_Items_MouseUp;
            // 
            // Def_PanelTextColour
            // 
            Def_PanelTextColour.AutoSize = false;
            Def_PanelTextColour.BackColor = System.Drawing.SystemColors.ActiveBorder;
            Def_PanelTextColour.DisplayStyle = ToolStripItemDisplayStyle.Text;
            Def_PanelTextColour.ImageTransparentColor = System.Drawing.Color.Magenta;
            Def_PanelTextColour.Name = "Def_PanelTextColour";
            Def_PanelTextColour.Size = new System.Drawing.Size(75, 22);
            Def_PanelTextColour.Text = "Text Colour";
            Def_PanelTextColour.MouseUp += Def_Items_MouseUp;
            // 
            // toolStripSeparator14
            // 
            toolStripSeparator14.Name = "toolStripSeparator14";
            toolStripSeparator14.Size = new System.Drawing.Size(6, 39);
            // 
            // Def_PanelTransparent
            // 
            Def_PanelTransparent.CheckOnClick = true;
            Def_PanelTransparent.DisplayStyle = ToolStripItemDisplayStyle.Image;
            Def_PanelTransparent.Image = (System.Drawing.Image)resources.GetObject("Def_PanelTransparent.Image");
            Def_PanelTransparent.ImageTransparentColor = System.Drawing.Color.Magenta;
            Def_PanelTransparent.Name = "Def_PanelTransparent";
            Def_PanelTransparent.Size = new System.Drawing.Size(29, 36);
            Def_PanelTransparent.ToolTipText = "Transparent Background";
            Def_PanelTransparent.MouseUp += Def_Items_MouseUp;
            // 
            // Def_PanelBackColour
            // 
            Def_PanelBackColour.AutoSize = false;
            Def_PanelBackColour.BackColor = System.Drawing.SystemColors.ActiveBorder;
            Def_PanelBackColour.DisplayStyle = ToolStripItemDisplayStyle.Text;
            Def_PanelBackColour.Image = (System.Drawing.Image)resources.GetObject("Def_PanelBackColour.Image");
            Def_PanelBackColour.ImageTransparentColor = System.Drawing.Color.Magenta;
            Def_PanelBackColour.Name = "Def_PanelBackColour";
            Def_PanelBackColour.Size = new System.Drawing.Size(75, 22);
            Def_PanelBackColour.Text = "Back Colour";
            Def_PanelBackColour.MouseUp += Def_Items_MouseUp;
            // 
            // panelDef6
            // 
            panelDef6.Controls.Add(toolStripDef6);
            panelDef6.Location = new System.Drawing.Point(8, 67);
            panelDef6.Margin = new Padding(3, 5, 3, 5);
            panelDef6.Name = "panelDef6";
            panelDef6.Size = new System.Drawing.Size(185, 33);
            panelDef6.TabIndex = 9;
            // 
            // toolStripDef6
            // 
            toolStripDef6.AutoSize = false;
            toolStripDef6.CanOverflow = false;
            toolStripDef6.Dock = DockStyle.None;
            toolStripDef6.GripStyle = ToolStripGripStyle.Hidden;
            toolStripDef6.ImageScalingSize = new System.Drawing.Size(24, 24);
            toolStripDef6.Items.AddRange(new ToolStripItem[] { Def_PanelShow, Def_PanelTitle, Def_PanelCopyright, Def_PanelSong, Def_PanelSlides, Def_PanelPrevNext });
            toolStripDef6.LayoutStyle = ToolStripLayoutStyle.HorizontalStackWithOverflow;
            toolStripDef6.Location = new System.Drawing.Point(0, -1);
            toolStripDef6.Name = "toolStripDef6";
            toolStripDef6.Padding = new Padding(0, 0, 2, 0);
            toolStripDef6.RenderMode = ToolStripRenderMode.System;
            toolStripDef6.Size = new System.Drawing.Size(194, 39);
            toolStripDef6.TabIndex = 0;
            // 
            // Def_PanelShow
            // 
            Def_PanelShow.CheckOnClick = true;
            Def_PanelShow.DisplayStyle = ToolStripItemDisplayStyle.Image;
            Def_PanelShow.Image = (System.Drawing.Image)resources.GetObject("Def_PanelShow.Image");
            Def_PanelShow.ImageTransparentColor = System.Drawing.Color.Magenta;
            Def_PanelShow.Name = "Def_PanelShow";
            Def_PanelShow.Size = new System.Drawing.Size(29, 36);
            Def_PanelShow.Tag = "list";
            Def_PanelShow.ToolTipText = "Show Display Panel";
            Def_PanelShow.MouseUp += Def_Items_MouseUp;
            // 
            // Def_PanelTitle
            // 
            Def_PanelTitle.CheckOnClick = true;
            Def_PanelTitle.DisplayStyle = ToolStripItemDisplayStyle.Image;
            Def_PanelTitle.Image = (System.Drawing.Image)resources.GetObject("Def_PanelTitle.Image");
            Def_PanelTitle.ImageTransparentColor = System.Drawing.Color.Magenta;
            Def_PanelTitle.Name = "Def_PanelTitle";
            Def_PanelTitle.Size = new System.Drawing.Size(29, 36);
            Def_PanelTitle.ToolTipText = "Show Title";
            Def_PanelTitle.MouseUp += Def_Items_MouseUp;
            // 
            // Def_PanelCopyright
            // 
            Def_PanelCopyright.CheckOnClick = true;
            Def_PanelCopyright.DisplayStyle = ToolStripItemDisplayStyle.Image;
            Def_PanelCopyright.Image = (System.Drawing.Image)resources.GetObject("Def_PanelCopyright.Image");
            Def_PanelCopyright.ImageTransparentColor = System.Drawing.Color.Magenta;
            Def_PanelCopyright.Name = "Def_PanelCopyright";
            Def_PanelCopyright.Size = new System.Drawing.Size(29, 36);
            Def_PanelCopyright.ToolTipText = "Show Copyright Information";
            Def_PanelCopyright.MouseUp += Def_Items_MouseUp;
            // 
            // Def_PanelSong
            // 
            Def_PanelSong.CheckOnClick = true;
            Def_PanelSong.DisplayStyle = ToolStripItemDisplayStyle.Image;
            Def_PanelSong.Image = (System.Drawing.Image)resources.GetObject("Def_PanelSong.Image");
            Def_PanelSong.ImageTransparentColor = System.Drawing.Color.Magenta;
            Def_PanelSong.Name = "Def_PanelSong";
            Def_PanelSong.Size = new System.Drawing.Size(29, 36);
            Def_PanelSong.Tag = "add";
            Def_PanelSong.ToolTipText = "Show Item Number";
            Def_PanelSong.MouseUp += Def_Items_MouseUp;
            // 
            // Def_PanelSlides
            // 
            Def_PanelSlides.CheckOnClick = true;
            Def_PanelSlides.DisplayStyle = ToolStripItemDisplayStyle.Image;
            Def_PanelSlides.Image = (System.Drawing.Image)resources.GetObject("Def_PanelSlides.Image");
            Def_PanelSlides.ImageTransparentColor = System.Drawing.Color.Magenta;
            Def_PanelSlides.Name = "Def_PanelSlides";
            Def_PanelSlides.Size = new System.Drawing.Size(29, 36);
            Def_PanelSlides.Tag = "open";
            Def_PanelSlides.ToolTipText = "Show Verse/Slide Indicators";
            Def_PanelSlides.MouseUp += Def_Items_MouseUp;
            // 
            // Def_PanelPrevNext
            // 
            Def_PanelPrevNext.CheckOnClick = true;
            Def_PanelPrevNext.DisplayStyle = ToolStripItemDisplayStyle.Image;
            Def_PanelPrevNext.Image = (System.Drawing.Image)resources.GetObject("Def_PanelPrevNext.Image");
            Def_PanelPrevNext.ImageTransparentColor = System.Drawing.Color.Magenta;
            Def_PanelPrevNext.Name = "Def_PanelPrevNext";
            Def_PanelPrevNext.Size = new System.Drawing.Size(29, 36);
            Def_PanelPrevNext.ToolTipText = "Show Previous/Next Item";
            Def_PanelPrevNext.MouseUp += Def_Items_MouseUp;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new System.Drawing.Point(190, 73);
            label5.Name = "label5";
            label5.Size = new System.Drawing.Size(54, 20);
            label5.TabIndex = 7;
            label5.Text = "Height";
            // 
            // DefgroupBox1
            // 
            DefgroupBox1.Controls.Add(panelDef2);
            DefgroupBox1.Controls.Add(panelDef1);
            DefgroupBox1.Location = new System.Drawing.Point(3, 45);
            DefgroupBox1.Margin = new Padding(3, 5, 3, 5);
            DefgroupBox1.Name = "DefgroupBox1";
            DefgroupBox1.Padding = new Padding(3, 5, 3, 5);
            DefgroupBox1.Size = new System.Drawing.Size(296, 109);
            DefgroupBox1.TabIndex = 1;
            DefgroupBox1.TabStop = false;
            DefgroupBox1.Text = "Default Layout";
            // 
            // panelDef2
            // 
            panelDef2.Controls.Add(toolStripDef2);
            panelDef2.Location = new System.Drawing.Point(8, 67);
            panelDef2.Margin = new Padding(3, 5, 3, 5);
            panelDef2.Name = "panelDef2";
            panelDef2.Size = new System.Drawing.Size(278, 33);
            panelDef2.TabIndex = 9;
            // 
            // toolStripDef2
            // 
            toolStripDef2.AutoSize = false;
            toolStripDef2.CanOverflow = false;
            toolStripDef2.Dock = DockStyle.None;
            toolStripDef2.GripStyle = ToolStripGripStyle.Hidden;
            toolStripDef2.ImageScalingSize = new System.Drawing.Size(24, 24);
            toolStripDef2.Items.AddRange(new ToolStripItem[] { Def_HeadAlign, toolStripSeparator26, Def_R1Align, Def_R1Colour, toolStripSeparator8, Def_R2Align, Def_R2Colour });
            toolStripDef2.LayoutStyle = ToolStripLayoutStyle.HorizontalStackWithOverflow;
            toolStripDef2.Location = new System.Drawing.Point(0, -1);
            toolStripDef2.Name = "toolStripDef2";
            toolStripDef2.Padding = new Padding(0, 0, 2, 0);
            toolStripDef2.RenderMode = ToolStripRenderMode.System;
            toolStripDef2.Size = new System.Drawing.Size(279, 39);
            toolStripDef2.TabIndex = 0;
            // 
            // Def_HeadAlign
            // 
            Def_HeadAlign.DisplayStyle = ToolStripItemDisplayStyle.Image;
            Def_HeadAlign.DropDownItems.AddRange(new ToolStripItem[] { Def_HeadAlignAsR1, Def_HeadAlignAsR2, Def_HeadAlignLeft, Def_HeadAlignCentre, Def_HeadAlignRight });
            Def_HeadAlign.Image = (System.Drawing.Image)resources.GetObject("Def_HeadAlign.Image");
            Def_HeadAlign.ImageTransparentColor = System.Drawing.Color.Magenta;
            Def_HeadAlign.Name = "Def_HeadAlign";
            Def_HeadAlign.Size = new System.Drawing.Size(38, 36);
            Def_HeadAlign.Tag = "0";
            Def_HeadAlign.Text = "toolStripDropDownButton1";
            Def_HeadAlign.DropDownItemClicked += Def_HeadAlign_DropDownItemClicked;
            // 
            // Def_HeadAlignAsR1
            // 
            Def_HeadAlignAsR1.Image = (System.Drawing.Image)resources.GetObject("Def_HeadAlignAsR1.Image");
            Def_HeadAlignAsR1.Name = "Def_HeadAlignAsR1";
            Def_HeadAlignAsR1.Size = new System.Drawing.Size(277, 26);
            Def_HeadAlignAsR1.Tag = "0";
            Def_HeadAlignAsR1.Text = "Headings Align As Region 1";
            // 
            // Def_HeadAlignAsR2
            // 
            Def_HeadAlignAsR2.Image = (System.Drawing.Image)resources.GetObject("Def_HeadAlignAsR2.Image");
            Def_HeadAlignAsR2.Name = "Def_HeadAlignAsR2";
            Def_HeadAlignAsR2.Size = new System.Drawing.Size(277, 26);
            Def_HeadAlignAsR2.Tag = "1";
            Def_HeadAlignAsR2.Text = "Headings Align As Region 2";
            // 
            // Def_HeadAlignLeft
            // 
            Def_HeadAlignLeft.Image = (System.Drawing.Image)resources.GetObject("Def_HeadAlignLeft.Image");
            Def_HeadAlignLeft.Name = "Def_HeadAlignLeft";
            Def_HeadAlignLeft.Size = new System.Drawing.Size(277, 26);
            Def_HeadAlignLeft.Tag = "2";
            Def_HeadAlignLeft.Text = "Headings Align Left";
            // 
            // Def_HeadAlignCentre
            // 
            Def_HeadAlignCentre.Image = (System.Drawing.Image)resources.GetObject("Def_HeadAlignCentre.Image");
            Def_HeadAlignCentre.Name = "Def_HeadAlignCentre";
            Def_HeadAlignCentre.Size = new System.Drawing.Size(277, 26);
            Def_HeadAlignCentre.Tag = "3";
            Def_HeadAlignCentre.Text = "Headings Align Centre";
            // 
            // Def_HeadAlignRight
            // 
            Def_HeadAlignRight.Image = (System.Drawing.Image)resources.GetObject("Def_HeadAlignRight.Image");
            Def_HeadAlignRight.Name = "Def_HeadAlignRight";
            Def_HeadAlignRight.Size = new System.Drawing.Size(277, 26);
            Def_HeadAlignRight.Tag = "4";
            Def_HeadAlignRight.Text = "Headings Align Right";
            // 
            // toolStripSeparator26
            // 
            toolStripSeparator26.Name = "toolStripSeparator26";
            toolStripSeparator26.Size = new System.Drawing.Size(6, 39);
            // 
            // Def_R1Align
            // 
            Def_R1Align.DisplayStyle = ToolStripItemDisplayStyle.Image;
            Def_R1Align.DropDownItems.AddRange(new ToolStripItem[] { Def_R1AlignLeft, Def_R1AlignCentre, Def_R1AlignRight });
            Def_R1Align.Image = (System.Drawing.Image)resources.GetObject("Def_R1Align.Image");
            Def_R1Align.ImageTransparentColor = System.Drawing.Color.Magenta;
            Def_R1Align.Name = "Def_R1Align";
            Def_R1Align.Size = new System.Drawing.Size(38, 36);
            Def_R1Align.Tag = "2";
            Def_R1Align.DropDownItemClicked += Def_R1Align_DropDownItemClicked;
            // 
            // Def_R1AlignLeft
            // 
            Def_R1AlignLeft.Image = (System.Drawing.Image)resources.GetObject("Def_R1AlignLeft.Image");
            Def_R1AlignLeft.Name = "Def_R1AlignLeft";
            Def_R1AlignLeft.Size = new System.Drawing.Size(237, 26);
            Def_R1AlignLeft.Tag = "1";
            Def_R1AlignLeft.Text = "Region 1 Align Left";
            // 
            // Def_R1AlignCentre
            // 
            Def_R1AlignCentre.Image = (System.Drawing.Image)resources.GetObject("Def_R1AlignCentre.Image");
            Def_R1AlignCentre.Name = "Def_R1AlignCentre";
            Def_R1AlignCentre.Size = new System.Drawing.Size(237, 26);
            Def_R1AlignCentre.Tag = "2";
            Def_R1AlignCentre.Text = "Region 1 Align Centre";
            // 
            // Def_R1AlignRight
            // 
            Def_R1AlignRight.Image = (System.Drawing.Image)resources.GetObject("Def_R1AlignRight.Image");
            Def_R1AlignRight.Name = "Def_R1AlignRight";
            Def_R1AlignRight.Size = new System.Drawing.Size(237, 26);
            Def_R1AlignRight.Tag = "3";
            Def_R1AlignRight.Text = "Region 1 Align Right";
            // 
            // Def_R1Colour
            // 
            Def_R1Colour.AutoSize = false;
            Def_R1Colour.BackColor = System.Drawing.SystemColors.ActiveBorder;
            Def_R1Colour.DisplayStyle = ToolStripItemDisplayStyle.Text;
            Def_R1Colour.ImageTransparentColor = System.Drawing.Color.Magenta;
            Def_R1Colour.Name = "Def_R1Colour";
            Def_R1Colour.Size = new System.Drawing.Size(54, 22);
            Def_R1Colour.Text = "R1 Col";
            Def_R1Colour.ToolTipText = "Region 1 Text Colour";
            Def_R1Colour.MouseUp += Def_Items_MouseUp;
            // 
            // toolStripSeparator8
            // 
            toolStripSeparator8.Name = "toolStripSeparator8";
            toolStripSeparator8.Size = new System.Drawing.Size(6, 39);
            // 
            // Def_R2Align
            // 
            Def_R2Align.DisplayStyle = ToolStripItemDisplayStyle.Image;
            Def_R2Align.DropDownItems.AddRange(new ToolStripItem[] { Def_R2AlignLeft, Def_R2AlignCentre, Def_R2AlignRight });
            Def_R2Align.Image = (System.Drawing.Image)resources.GetObject("Def_R2Align.Image");
            Def_R2Align.ImageTransparentColor = System.Drawing.Color.Magenta;
            Def_R2Align.Name = "Def_R2Align";
            Def_R2Align.Size = new System.Drawing.Size(38, 36);
            Def_R2Align.Tag = "2";
            Def_R2Align.DropDownItemClicked += Def_R2Align_DropDownItemClicked;
            // 
            // Def_R2AlignLeft
            // 
            Def_R2AlignLeft.Image = (System.Drawing.Image)resources.GetObject("Def_R2AlignLeft.Image");
            Def_R2AlignLeft.Name = "Def_R2AlignLeft";
            Def_R2AlignLeft.Size = new System.Drawing.Size(237, 26);
            Def_R2AlignLeft.Tag = "1";
            Def_R2AlignLeft.Text = "Region 2 Align Left";
            // 
            // Def_R2AlignCentre
            // 
            Def_R2AlignCentre.Image = (System.Drawing.Image)resources.GetObject("Def_R2AlignCentre.Image");
            Def_R2AlignCentre.Name = "Def_R2AlignCentre";
            Def_R2AlignCentre.Size = new System.Drawing.Size(237, 26);
            Def_R2AlignCentre.Tag = "2";
            Def_R2AlignCentre.Text = "Region 2 Align Centre";
            // 
            // Def_R2AlignRight
            // 
            Def_R2AlignRight.Image = (System.Drawing.Image)resources.GetObject("Def_R2AlignRight.Image");
            Def_R2AlignRight.Name = "Def_R2AlignRight";
            Def_R2AlignRight.Size = new System.Drawing.Size(237, 26);
            Def_R2AlignRight.Tag = "3";
            Def_R2AlignRight.Text = "Region 2 Align Right";
            // 
            // Def_R2Colour
            // 
            Def_R2Colour.AutoSize = false;
            Def_R2Colour.BackColor = System.Drawing.SystemColors.ActiveBorder;
            Def_R2Colour.DisplayStyle = ToolStripItemDisplayStyle.Text;
            Def_R2Colour.Image = (System.Drawing.Image)resources.GetObject("Def_R2Colour.Image");
            Def_R2Colour.ImageTransparentColor = System.Drawing.Color.Magenta;
            Def_R2Colour.Name = "Def_R2Colour";
            Def_R2Colour.Size = new System.Drawing.Size(54, 22);
            Def_R2Colour.Text = "R2 Col";
            Def_R2Colour.ToolTipText = "Region 2 Text Colour";
            Def_R2Colour.MouseUp += Def_Items_MouseUp;
            // 
            // panelDef1
            // 
            panelDef1.Controls.Add(toolStripDef1);
            panelDef1.Location = new System.Drawing.Point(8, 28);
            panelDef1.Margin = new Padding(3, 5, 3, 5);
            panelDef1.Name = "panelDef1";
            panelDef1.Size = new System.Drawing.Size(280, 33);
            panelDef1.TabIndex = 8;
            // 
            // toolStripDef1
            // 
            toolStripDef1.AutoSize = false;
            toolStripDef1.CanOverflow = false;
            toolStripDef1.Dock = DockStyle.None;
            toolStripDef1.GripStyle = ToolStripGripStyle.Hidden;
            toolStripDef1.ImageScalingSize = new System.Drawing.Size(24, 24);
            toolStripDef1.Items.AddRange(new ToolStripItem[] { Def_Head, Def_Region, Def_VAlign, Def_Shadow, Def_Outline, Def_Interlace, Def_Notations, Def_ToZero });
            toolStripDef1.LayoutStyle = ToolStripLayoutStyle.HorizontalStackWithOverflow;
            toolStripDef1.Location = new System.Drawing.Point(0, -1);
            toolStripDef1.Name = "toolStripDef1";
            toolStripDef1.Padding = new Padding(0, 0, 2, 0);
            toolStripDef1.RenderMode = ToolStripRenderMode.System;
            toolStripDef1.Size = new System.Drawing.Size(280, 39);
            toolStripDef1.TabIndex = 0;
            // 
            // Def_Head
            // 
            Def_Head.DisplayStyle = ToolStripItemDisplayStyle.Image;
            Def_Head.DropDownItems.AddRange(new ToolStripItem[] { Def_HeadNoTitles, Def_HeadAllTitles, Def_HeadFirstScreen });
            Def_Head.Image = (System.Drawing.Image)resources.GetObject("Def_Head.Image");
            Def_Head.ImageTransparentColor = System.Drawing.Color.Magenta;
            Def_Head.Name = "Def_Head";
            Def_Head.Size = new System.Drawing.Size(38, 36);
            Def_Head.Tag = "1";
            Def_Head.ToolTipText = "Display Title/Verse Headings";
            Def_Head.DropDownItemClicked += Def_Head_DropDownItemClicked;
            // 
            // Def_HeadNoTitles
            // 
            Def_HeadNoTitles.Image = (System.Drawing.Image)resources.GetObject("Def_HeadNoTitles.Image");
            Def_HeadNoTitles.Name = "Def_HeadNoTitles";
            Def_HeadNoTitles.Size = new System.Drawing.Size(281, 26);
            Def_HeadNoTitles.Tag = "0";
            Def_HeadNoTitles.Text = "No Headings";
            // 
            // Def_HeadAllTitles
            // 
            Def_HeadAllTitles.Image = (System.Drawing.Image)resources.GetObject("Def_HeadAllTitles.Image");
            Def_HeadAllTitles.Name = "Def_HeadAllTitles";
            Def_HeadAllTitles.Size = new System.Drawing.Size(281, 26);
            Def_HeadAllTitles.Tag = "1";
            Def_HeadAllTitles.Text = "Show All Headings";
            // 
            // Def_HeadFirstScreen
            // 
            Def_HeadFirstScreen.Image = (System.Drawing.Image)resources.GetObject("Def_HeadFirstScreen.Image");
            Def_HeadFirstScreen.Name = "Def_HeadFirstScreen";
            Def_HeadFirstScreen.Size = new System.Drawing.Size(281, 26);
            Def_HeadFirstScreen.Tag = "2";
            Def_HeadFirstScreen.Text = "Heading At First Screen Only";
            // 
            // Def_Region
            // 
            Def_Region.DisplayStyle = ToolStripItemDisplayStyle.Image;
            Def_Region.DropDownItems.AddRange(new ToolStripItem[] { Def_ShowRegion1, Def_ShowRegion2, Def_ShowRegionBoth });
            Def_Region.Image = (System.Drawing.Image)resources.GetObject("Def_Region.Image");
            Def_Region.ImageTransparentColor = System.Drawing.Color.Magenta;
            Def_Region.Name = "Def_Region";
            Def_Region.Size = new System.Drawing.Size(38, 36);
            Def_Region.Tag = "2";
            Def_Region.ToolTipText = "Show Region Text";
            Def_Region.DropDownItemClicked += Def_Region_DropDownItemClicked;
            // 
            // Def_ShowRegion1
            // 
            Def_ShowRegion1.Image = (System.Drawing.Image)resources.GetObject("Def_ShowRegion1.Image");
            Def_ShowRegion1.Name = "Def_ShowRegion1";
            Def_ShowRegion1.Size = new System.Drawing.Size(198, 26);
            Def_ShowRegion1.Tag = "0";
            Def_ShowRegion1.Text = "Region 1 Only";
            // 
            // Def_ShowRegion2
            // 
            Def_ShowRegion2.Image = (System.Drawing.Image)resources.GetObject("Def_ShowRegion2.Image");
            Def_ShowRegion2.Name = "Def_ShowRegion2";
            Def_ShowRegion2.Size = new System.Drawing.Size(198, 26);
            Def_ShowRegion2.Tag = "1";
            Def_ShowRegion2.Text = "Region 2 Only";
            // 
            // Def_ShowRegionBoth
            // 
            Def_ShowRegionBoth.Image = (System.Drawing.Image)resources.GetObject("Def_ShowRegionBoth.Image");
            Def_ShowRegionBoth.Name = "Def_ShowRegionBoth";
            Def_ShowRegionBoth.Size = new System.Drawing.Size(198, 26);
            Def_ShowRegionBoth.Tag = "2";
            Def_ShowRegionBoth.Text = "Regions 1 and 2";
            // 
            // Def_VAlign
            // 
            Def_VAlign.DisplayStyle = ToolStripItemDisplayStyle.Image;
            Def_VAlign.DropDownItems.AddRange(new ToolStripItem[] { Def_VAlignTop, Def_VAlignCentre, Def_VAlignBottom });
            Def_VAlign.Image = (System.Drawing.Image)resources.GetObject("Def_VAlign.Image");
            Def_VAlign.ImageTransparentColor = System.Drawing.Color.Magenta;
            Def_VAlign.Name = "Def_VAlign";
            Def_VAlign.Size = new System.Drawing.Size(38, 36);
            Def_VAlign.Tag = "1";
            Def_VAlign.ToolTipText = "Vertical Alignment";
            Def_VAlign.DropDownItemClicked += Def_VAlign_DropDownItemClicked;
            // 
            // Def_VAlignTop
            // 
            Def_VAlignTop.Image = (System.Drawing.Image)resources.GetObject("Def_VAlignTop.Image");
            Def_VAlignTop.Name = "Def_VAlignTop";
            Def_VAlignTop.Size = new System.Drawing.Size(181, 26);
            Def_VAlignTop.Tag = "0";
            Def_VAlignTop.Text = "Align Top";
            // 
            // Def_VAlignCentre
            // 
            Def_VAlignCentre.Image = (System.Drawing.Image)resources.GetObject("Def_VAlignCentre.Image");
            Def_VAlignCentre.Name = "Def_VAlignCentre";
            Def_VAlignCentre.Size = new System.Drawing.Size(181, 26);
            Def_VAlignCentre.Tag = "1";
            Def_VAlignCentre.Text = "Align Centre";
            // 
            // Def_VAlignBottom
            // 
            Def_VAlignBottom.Image = (System.Drawing.Image)resources.GetObject("Def_VAlignBottom.Image");
            Def_VAlignBottom.Name = "Def_VAlignBottom";
            Def_VAlignBottom.Size = new System.Drawing.Size(181, 26);
            Def_VAlignBottom.Tag = "2";
            Def_VAlignBottom.Text = "Align Bottom";
            // 
            // Def_Shadow
            // 
            Def_Shadow.CheckOnClick = true;
            Def_Shadow.DisplayStyle = ToolStripItemDisplayStyle.Image;
            Def_Shadow.Image = (System.Drawing.Image)resources.GetObject("Def_Shadow.Image");
            Def_Shadow.ImageTransparentColor = System.Drawing.Color.Magenta;
            Def_Shadow.Name = "Def_Shadow";
            Def_Shadow.Size = new System.Drawing.Size(29, 36);
            Def_Shadow.Tag = "open";
            Def_Shadow.ToolTipText = "Shadow Font";
            Def_Shadow.MouseUp += Def_Items_MouseUp;
            // 
            // Def_Outline
            // 
            Def_Outline.CheckOnClick = true;
            Def_Outline.DisplayStyle = ToolStripItemDisplayStyle.Image;
            Def_Outline.Image = (System.Drawing.Image)resources.GetObject("Def_Outline.Image");
            Def_Outline.ImageTransparentColor = System.Drawing.Color.Magenta;
            Def_Outline.Name = "Def_Outline";
            Def_Outline.Size = new System.Drawing.Size(29, 36);
            Def_Outline.Tag = "add";
            Def_Outline.ToolTipText = "Outline Font";
            Def_Outline.MouseUp += Def_Items_MouseUp;
            // 
            // Def_Interlace
            // 
            Def_Interlace.CheckOnClick = true;
            Def_Interlace.DisplayStyle = ToolStripItemDisplayStyle.Image;
            Def_Interlace.Image = (System.Drawing.Image)resources.GetObject("Def_Interlace.Image");
            Def_Interlace.ImageTransparentColor = System.Drawing.Color.Magenta;
            Def_Interlace.Name = "Def_Interlace";
            Def_Interlace.Size = new System.Drawing.Size(29, 36);
            Def_Interlace.ToolTipText = "Interlace Region1/Regions2";
            Def_Interlace.MouseUp += Def_Items_MouseUp;
            // 
            // Def_Notations
            // 
            Def_Notations.CheckOnClick = true;
            Def_Notations.DisplayStyle = ToolStripItemDisplayStyle.Image;
            Def_Notations.Image = (System.Drawing.Image)resources.GetObject("Def_Notations.Image");
            Def_Notations.ImageTransparentColor = System.Drawing.Color.Magenta;
            Def_Notations.Name = "Def_Notations";
            Def_Notations.Size = new System.Drawing.Size(29, 36);
            Def_Notations.ToolTipText = "Show Notations";
            Def_Notations.MouseUp += Def_Items_MouseUp;
            // 
            // Def_ToZero
            // 
            Def_ToZero.CheckOnClick = true;
            Def_ToZero.DisplayStyle = ToolStripItemDisplayStyle.Image;
            Def_ToZero.Image = (System.Drawing.Image)resources.GetObject("Def_ToZero.Image");
            Def_ToZero.ImageTransparentColor = System.Drawing.Color.Magenta;
            Def_ToZero.Name = "Def_ToZero";
            Def_ToZero.Size = new System.Drawing.Size(29, 36);
            Def_ToZero.ToolTipText = "To Capo 0";
            Def_ToZero.MouseUp += Def_Items_MouseUp;
            // 
            // columnHeader35
            // 
            columnHeader35.Width = 0;
            // 
            // InfoScreen_Delete
            // 
            InfoScreen_Delete.DisplayStyle = ToolStripItemDisplayStyle.Image;
            InfoScreen_Delete.Image = (System.Drawing.Image)resources.GetObject("InfoScreen_Delete.Image");
            InfoScreen_Delete.ImageTransparentColor = System.Drawing.Color.Magenta;
            InfoScreen_Delete.Name = "InfoScreen_Delete";
            InfoScreen_Delete.Size = new System.Drawing.Size(30, 28);
            InfoScreen_Delete.Tag = "delete";
            InfoScreen_Delete.ToolTipText = "Delete";
            InfoScreen_Delete.Click += InfoScreen_EditBtns_Click;
            // 
            // CMenuSongs_AddShow
            // 
            CMenuSongs_AddShow.Name = "CMenuSongs_AddShow";
            CMenuSongs_AddShow.Size = new System.Drawing.Size(162, 24);
            CMenuSongs_AddShow.Text = "Add && &Show";
            CMenuSongs_AddShow.Click += CMenuSongs_AddShow_Click;
            // 
            // toolStripSeparator38
            // 
            toolStripSeparator38.Name = "toolStripSeparator38";
            toolStripSeparator38.Size = new System.Drawing.Size(159, 6);
            // 
            // CMenuSongs_Edit
            // 
            CMenuSongs_Edit.Name = "CMenuSongs_Edit";
            CMenuSongs_Edit.Size = new System.Drawing.Size(162, 24);
            CMenuSongs_Edit.Text = "Edit item";
            CMenuSongs_Edit.Click += CMenuSongs_Edit_Click;
            // 
            // CMenuSongs_Copy
            // 
            CMenuSongs_Copy.Name = "CMenuSongs_Copy";
            CMenuSongs_Copy.Size = new System.Drawing.Size(162, 24);
            CMenuSongs_Copy.Text = "Copy item";
            CMenuSongs_Copy.Click += CMenuSongs_Copy_Click;
            // 
            // toolStripSeparator10
            // 
            toolStripSeparator10.Name = "toolStripSeparator10";
            toolStripSeparator10.Size = new System.Drawing.Size(159, 6);
            // 
            // CMenuSongs_Refresh
            // 
            CMenuSongs_Refresh.Name = "CMenuSongs_Refresh";
            CMenuSongs_Refresh.Size = new System.Drawing.Size(162, 24);
            CMenuSongs_Refresh.Text = "Refresh";
            CMenuSongs_Refresh.Click += CMenuSongs_Refresh_Click;
            // 
            // CMenuSongs_UnselectAll
            // 
            CMenuSongs_UnselectAll.Name = "CMenuSongs_UnselectAll";
            CMenuSongs_UnselectAll.Size = new System.Drawing.Size(162, 24);
            CMenuSongs_UnselectAll.Text = "&Unselect All";
            CMenuSongs_UnselectAll.Click += CMenuSongs_UnselectAll_Click;
            // 
            // SongFolder
            // 
            SongFolder.DropDownStyle = ComboBoxStyle.DropDownList;
            SongFolder.FormattingEnabled = true;
            SongFolder.Location = new System.Drawing.Point(3, 5);
            SongFolder.Margin = new Padding(3, 5, 3, 5);
            SongFolder.MaxDropDownItems = 12;
            SongFolder.Name = "SongFolder";
            SongFolder.Size = new System.Drawing.Size(75, 28);
            SongFolder.TabIndex = 0;
            SongFolder.SelectedIndexChanged += SongFolder_SelectedIndexChanged;
            // 
            // panelInfoScreen2
            // 
            panelInfoScreen2.Controls.Add(InfoScreentoolstrip2);
            panelInfoScreen2.Location = new System.Drawing.Point(88, 41);
            panelInfoScreen2.Margin = new Padding(3, 5, 3, 5);
            panelInfoScreen2.Name = "panelInfoScreen2";
            panelInfoScreen2.Size = new System.Drawing.Size(33, 188);
            panelInfoScreen2.TabIndex = 17;
            // 
            // InfoScreentoolstrip2
            // 
            InfoScreentoolstrip2.AutoSize = false;
            InfoScreentoolstrip2.CanOverflow = false;
            InfoScreentoolstrip2.Dock = DockStyle.None;
            InfoScreentoolstrip2.GripStyle = ToolStripGripStyle.Hidden;
            InfoScreentoolstrip2.ImageScalingSize = new System.Drawing.Size(24, 24);
            InfoScreentoolstrip2.Items.AddRange(new ToolStripItem[] { InfoScreen_New, InfoScreen_Edit, InfoScreen_Copy, InfoScreen_Move, InfoScreen_Delete });
            InfoScreentoolstrip2.LayoutStyle = ToolStripLayoutStyle.VerticalStackWithOverflow;
            InfoScreentoolstrip2.Location = new System.Drawing.Point(0, -1);
            InfoScreentoolstrip2.Name = "InfoScreentoolstrip2";
            InfoScreentoolstrip2.Padding = new Padding(0, 0, 2, 0);
            InfoScreentoolstrip2.RenderMode = ToolStripRenderMode.System;
            InfoScreentoolstrip2.Size = new System.Drawing.Size(33, 205);
            InfoScreentoolstrip2.TabIndex = 0;
            // 
            // InfoScreen_New
            // 
            InfoScreen_New.AutoSize = false;
            InfoScreen_New.DisplayStyle = ToolStripItemDisplayStyle.Image;
            InfoScreen_New.Image = (System.Drawing.Image)resources.GetObject("InfoScreen_New.Image");
            InfoScreen_New.ImageTransparentColor = System.Drawing.Color.Magenta;
            InfoScreen_New.Name = "InfoScreen_New";
            InfoScreen_New.Size = new System.Drawing.Size(23, 22);
            InfoScreen_New.Tag = "new";
            InfoScreen_New.ToolTipText = "New";
            InfoScreen_New.Click += InfoScreen_EditBtns_Click;
            // 
            // InfoScreen_Edit
            // 
            InfoScreen_Edit.AutoSize = false;
            InfoScreen_Edit.DisplayStyle = ToolStripItemDisplayStyle.Image;
            InfoScreen_Edit.Image = (System.Drawing.Image)resources.GetObject("InfoScreen_Edit.Image");
            InfoScreen_Edit.ImageTransparentColor = System.Drawing.Color.Magenta;
            InfoScreen_Edit.Name = "InfoScreen_Edit";
            InfoScreen_Edit.Size = new System.Drawing.Size(22, 22);
            InfoScreen_Edit.Tag = "edit";
            InfoScreen_Edit.ToolTipText = "Edit";
            InfoScreen_Edit.Click += InfoScreen_EditBtns_Click;
            // 
            // InfoScreen_Copy
            // 
            InfoScreen_Copy.AutoSize = false;
            InfoScreen_Copy.DisplayStyle = ToolStripItemDisplayStyle.Image;
            InfoScreen_Copy.Image = (System.Drawing.Image)resources.GetObject("InfoScreen_Copy.Image");
            InfoScreen_Copy.ImageTransparentColor = System.Drawing.Color.Magenta;
            InfoScreen_Copy.Name = "InfoScreen_Copy";
            InfoScreen_Copy.Size = new System.Drawing.Size(22, 22);
            InfoScreen_Copy.Tag = "copy";
            InfoScreen_Copy.ToolTipText = "Copy";
            InfoScreen_Copy.Click += InfoScreen_EditBtns_Click;
            // 
            // InfoScreen_Move
            // 
            InfoScreen_Move.DisplayStyle = ToolStripItemDisplayStyle.Image;
            InfoScreen_Move.Image = (System.Drawing.Image)resources.GetObject("InfoScreen_Move.Image");
            InfoScreen_Move.ImageTransparentColor = System.Drawing.Color.Magenta;
            InfoScreen_Move.Name = "InfoScreen_Move";
            InfoScreen_Move.Size = new System.Drawing.Size(30, 28);
            InfoScreen_Move.Tag = "move";
            InfoScreen_Move.ToolTipText = "Move";
            InfoScreen_Move.Click += InfoScreen_EditBtns_Click;
            // 
            // tabFiles
            // 
            tabFiles.BackColor = System.Drawing.SystemColors.Control;
            tabFiles.Controls.Add(panelInfoScreen2);
            tabFiles.Controls.Add(panelExternalFiles);
            tabFiles.Controls.Add(InfoScreenList);
            tabFiles.Location = new System.Drawing.Point(4, 4);
            tabFiles.Margin = new Padding(3, 5, 3, 5);
            tabFiles.Name = "tabFiles";
            tabFiles.Size = new System.Drawing.Size(336, 478);
            tabFiles.TabIndex = 4;
            tabFiles.Text = "InfoScr";
            // 
            // panelExternalFiles
            // 
            panelExternalFiles.Controls.Add(panelInfoScreen1);
            panelExternalFiles.Controls.Add(InfoScreenFolder);
            panelExternalFiles.Dock = DockStyle.Top;
            panelExternalFiles.Location = new System.Drawing.Point(0, 0);
            panelExternalFiles.Margin = new Padding(3, 5, 3, 5);
            panelExternalFiles.Name = "panelExternalFiles";
            panelExternalFiles.Size = new System.Drawing.Size(336, 40);
            panelExternalFiles.TabIndex = 16;
            // 
            // panelInfoScreen1
            // 
            panelInfoScreen1.Controls.Add(InfoScreentoolstrip1);
            panelInfoScreen1.Location = new System.Drawing.Point(153, 5);
            panelInfoScreen1.Margin = new Padding(3, 5, 3, 5);
            panelInfoScreen1.Name = "panelInfoScreen1";
            panelInfoScreen1.Size = new System.Drawing.Size(66, 29);
            panelInfoScreen1.TabIndex = 18;
            // 
            // InfoScreentoolstrip1
            // 
            InfoScreentoolstrip1.AutoSize = false;
            InfoScreentoolstrip1.CanOverflow = false;
            InfoScreentoolstrip1.Dock = DockStyle.None;
            InfoScreentoolstrip1.GripStyle = ToolStripGripStyle.Hidden;
            InfoScreentoolstrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            InfoScreentoolstrip1.Items.AddRange(new ToolStripItem[] { InfoScreen_OpenFolder, InfoScreen_Import });
            InfoScreentoolstrip1.Location = new System.Drawing.Point(0, -1);
            InfoScreentoolstrip1.Name = "InfoScreentoolstrip1";
            InfoScreentoolstrip1.Padding = new Padding(0, 0, 2, 0);
            InfoScreentoolstrip1.RenderMode = ToolStripRenderMode.System;
            InfoScreentoolstrip1.Size = new System.Drawing.Size(72, 37);
            InfoScreentoolstrip1.TabIndex = 5;
            // 
            // InfoScreen_OpenFolder
            // 
            InfoScreen_OpenFolder.DisplayStyle = ToolStripItemDisplayStyle.Image;
            InfoScreen_OpenFolder.Image = (System.Drawing.Image)resources.GetObject("InfoScreen_OpenFolder.Image");
            InfoScreen_OpenFolder.ImageTransparentColor = System.Drawing.Color.Magenta;
            InfoScreen_OpenFolder.Name = "InfoScreen_OpenFolder";
            InfoScreen_OpenFolder.Size = new System.Drawing.Size(29, 34);
            InfoScreen_OpenFolder.Tag = "";
            InfoScreen_OpenFolder.ToolTipText = "Open Folder";
            InfoScreen_OpenFolder.Click += InfoScreen_OpenFolder_Click;
            // 
            // InfoScreen_Import
            // 
            InfoScreen_Import.DisplayStyle = ToolStripItemDisplayStyle.Image;
            InfoScreen_Import.Image = (System.Drawing.Image)resources.GetObject("InfoScreen_Import.Image");
            InfoScreen_Import.ImageTransparentColor = System.Drawing.Color.Magenta;
            InfoScreen_Import.Name = "InfoScreen_Import";
            InfoScreen_Import.Size = new System.Drawing.Size(29, 34);
            InfoScreen_Import.ToolTipText = "Import an InfoScreen Into Folder";
            InfoScreen_Import.Click += InfoScreen_Import_Click;
            // 
            // InfoScreenFolder
            // 
            InfoScreenFolder.DropDownStyle = ComboBoxStyle.DropDownList;
            InfoScreenFolder.FormattingEnabled = true;
            InfoScreenFolder.Location = new System.Drawing.Point(3, 5);
            InfoScreenFolder.Margin = new Padding(3, 5, 3, 5);
            InfoScreenFolder.MaxDropDownItems = 12;
            InfoScreenFolder.Name = "InfoScreenFolder";
            InfoScreenFolder.Size = new System.Drawing.Size(140, 28);
            InfoScreenFolder.TabIndex = 17;
            InfoScreenFolder.SelectedIndexChanged += InfoScreenFolder_SelectedIndexChanged;
            // 
            // InfoScreenList
            // 
            InfoScreenList.Columns.AddRange(new ColumnHeader[] { columnHeader23, columnHeader24, columnHeader25, columnHeader26, columnHeader27, columnHeader28, columnHeader29 });
            InfoScreenList.ContextMenuStrip = CMenuFiles;
            InfoScreenList.FullRowSelect = true;
            InfoScreenList.HeaderStyle = ColumnHeaderStyle.None;
            InfoScreenList.LabelWrap = false;
            InfoScreenList.Location = new System.Drawing.Point(3, 41);
            InfoScreenList.Margin = new Padding(3, 5, 3, 5);
            InfoScreenList.Name = "InfoScreenList";
            InfoScreenList.ShowItemToolTips = true;
            InfoScreenList.Size = new System.Drawing.Size(45, 112);
            InfoScreenList.TabIndex = 5;
            InfoScreenList.UseCompatibleStateImageBehavior = false;
            InfoScreenList.View = View.Details;
            InfoScreenList.ItemDrag += InfoScreenList_ItemDrag;
            InfoScreenList.Enter += FormControl_Enter;
            InfoScreenList.KeyUp += InfoScreenList_KeyUp;
            InfoScreenList.Leave += FormControl_Leave;
            InfoScreenList.MouseDoubleClick += InfoScreenList_MouseDoubleClick;
            InfoScreenList.MouseUp += InfoScreenList_MouseUp;
            // 
            // columnHeader24
            // 
            columnHeader24.Width = 0;
            // 
            // columnHeader25
            // 
            columnHeader25.Width = 0;
            // 
            // columnHeader26
            // 
            columnHeader26.Width = 0;
            // 
            // columnHeader27
            // 
            columnHeader27.TextAlign = HorizontalAlignment.Center;
            columnHeader27.Width = 0;
            // 
            // columnHeader28
            // 
            columnHeader28.Width = 0;
            // 
            // columnHeader29
            // 
            columnHeader29.Width = 0;
            // 
            // CMenuSongs_SelectAll
            // 
            CMenuSongs_SelectAll.Name = "CMenuSongs_SelectAll";
            CMenuSongs_SelectAll.Size = new System.Drawing.Size(162, 24);
            CMenuSongs_SelectAll.Text = "Select &All";
            CMenuSongs_SelectAll.Click += CMenuSongs_SelectAll_Click;
            // 
            // toolStripContainerMain
            // 
            // 
            // toolStripContainerMain.ContentPanel
            // 
            toolStripContainerMain.ContentPanel.Controls.Add(splitContainerMain);
            toolStripContainerMain.ContentPanel.Margin = new Padding(3, 5, 3, 5);
            toolStripContainerMain.ContentPanel.Size = new System.Drawing.Size(979, 658);
            toolStripContainerMain.Dock = DockStyle.Fill;
            toolStripContainerMain.Location = new System.Drawing.Point(0, 30);
            toolStripContainerMain.Margin = new Padding(3, 5, 3, 5);
            toolStripContainerMain.Name = "toolStripContainerMain";
            toolStripContainerMain.Size = new System.Drawing.Size(979, 689);
            toolStripContainerMain.TabIndex = 1;
            toolStripContainerMain.Text = "toolStripContainer1";
            // 
            // toolStripContainerMain.TopToolStripPanel
            // 
            toolStripContainerMain.TopToolStripPanel.Controls.Add(toolStripMain);
            // 
            // splitContainerMain
            // 
            splitContainerMain.Dock = DockStyle.Fill;
            splitContainerMain.Location = new System.Drawing.Point(0, 0);
            splitContainerMain.Margin = new Padding(3, 5, 3, 5);
            splitContainerMain.Name = "splitContainerMain";
            // 
            // splitContainerMain.Panel1
            // 
            splitContainerMain.Panel1.Controls.Add(splitContainer1);
            // 
            // splitContainerMain.Panel2
            // 
            splitContainerMain.Panel2.Controls.Add(splitContainer2);
            splitContainerMain.Panel2MinSize = 50;
            splitContainerMain.Size = new System.Drawing.Size(979, 658);
            splitContainerMain.SplitterDistance = 348;
            splitContainerMain.SplitterWidth = 3;
            splitContainerMain.TabIndex = 0;
            splitContainerMain.Text = "splitContainer1";
            // 
            // splitContainer1
            // 
            splitContainer1.BorderStyle = BorderStyle.Fixed3D;
            splitContainer1.Dock = DockStyle.Fill;
            splitContainer1.Location = new System.Drawing.Point(0, 0);
            splitContainer1.Margin = new Padding(3, 5, 3, 5);
            splitContainer1.Name = "splitContainer1";
            splitContainer1.Orientation = Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(tabControlSource);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(tabControlLists);
            splitContainer1.Size = new System.Drawing.Size(348, 658);
            splitContainer1.SplitterDistance = 515;
            splitContainer1.SplitterWidth = 5;
            splitContainer1.TabIndex = 0;
            splitContainer1.Text = "splitContainer2";
            // 
            // tabControlSource
            // 
            tabControlSource.Alignment = TabAlignment.Bottom;
            tabControlSource.Controls.Add(tabFolders);
            tabControlSource.Controls.Add(tabFiles);
            tabControlSource.Controls.Add(tabPowerpoint);
            tabControlSource.Controls.Add(tabBibles);
            tabControlSource.Controls.Add(tabImages);
            tabControlSource.Controls.Add(tabMedia);
            tabControlSource.Controls.Add(tabDefault);
            tabControlSource.Dock = DockStyle.Fill;
            tabControlSource.Location = new System.Drawing.Point(0, 0);
            tabControlSource.Margin = new Padding(3, 5, 3, 5);
            tabControlSource.Name = "tabControlSource";
            tabControlSource.Padding = new System.Drawing.Point(5, 3);
            tabControlSource.SelectedIndex = 0;
            tabControlSource.Size = new System.Drawing.Size(344, 511);
            tabControlSource.TabIndex = 0;
            tabControlSource.SelectedIndexChanged += tabControlSource_SelectedIndexChanged;
            tabControlSource.Resize += tabControlSource_Resize;
            // 
            // tabFolders
            // 
            tabFolders.BackColor = System.Drawing.SystemColors.Control;
            tabFolders.Controls.Add(panelFolders);
            tabFolders.Controls.Add(SongsList);
            tabFolders.Controls.Add(SongFolder);
            tabFolders.Location = new System.Drawing.Point(4, 4);
            tabFolders.Margin = new Padding(3, 5, 3, 5);
            tabFolders.Name = "tabFolders";
            tabFolders.Padding = new Padding(3, 5, 3, 5);
            tabFolders.Size = new System.Drawing.Size(336, 478);
            tabFolders.TabIndex = 0;
            tabFolders.Text = "Folders";
            // 
            // panelFolders
            // 
            panelFolders.Controls.Add(toolStripFolders);
            panelFolders.Location = new System.Drawing.Point(88, 5);
            panelFolders.Margin = new Padding(3, 5, 3, 5);
            panelFolders.Name = "panelFolders";
            panelFolders.Size = new System.Drawing.Size(32, 33);
            panelFolders.TabIndex = 7;
            // 
            // toolStripFolders
            // 
            toolStripFolders.AutoSize = false;
            toolStripFolders.CanOverflow = false;
            toolStripFolders.Dock = DockStyle.None;
            toolStripFolders.GripStyle = ToolStripGripStyle.Hidden;
            toolStripFolders.ImageScalingSize = new System.Drawing.Size(24, 24);
            toolStripFolders.Items.AddRange(new ToolStripItem[] { Folders_WordCount });
            toolStripFolders.LayoutStyle = ToolStripLayoutStyle.HorizontalStackWithOverflow;
            toolStripFolders.Location = new System.Drawing.Point(0, -1);
            toolStripFolders.Name = "toolStripFolders";
            toolStripFolders.Padding = new Padding(0, 0, 2, 0);
            toolStripFolders.RenderMode = ToolStripRenderMode.System;
            toolStripFolders.Size = new System.Drawing.Size(32, 39);
            toolStripFolders.TabIndex = 0;
            // 
            // Folders_WordCount
            // 
            Folders_WordCount.CheckOnClick = true;
            Folders_WordCount.DisplayStyle = ToolStripItemDisplayStyle.Image;
            Folders_WordCount.Image = (System.Drawing.Image)resources.GetObject("Folders_WordCount.Image");
            Folders_WordCount.ImageTransparentColor = System.Drawing.Color.Magenta;
            Folders_WordCount.Name = "Folders_WordCount";
            Folders_WordCount.Size = new System.Drawing.Size(29, 36);
            Folders_WordCount.Tag = "wordcount";
            Folders_WordCount.ToolTipText = "Sort by CJK Word Count";
            Folders_WordCount.MouseUp += Folders_WordCount_MouseUp;
            // 
            // SongsList
            // 
            SongsList.Columns.AddRange(new ColumnHeader[] { columnHeader1, columnHeader2, columnHeader3, columnHeader4, columnHeader5, columnHeader6, columnHeader7 });
            SongsList.ContextMenuStrip = CMenuSongs;
            SongsList.FullRowSelect = true;
            SongsList.HeaderStyle = ColumnHeaderStyle.None;
            SongsList.LabelWrap = false;
            SongsList.Location = new System.Drawing.Point(3, 41);
            SongsList.Margin = new Padding(3, 5, 3, 5);
            SongsList.Name = "SongsList";
            SongsList.ShowItemToolTips = true;
            SongsList.Size = new System.Drawing.Size(45, 112);
            SongsList.TabIndex = 1;
            SongsList.UseCompatibleStateImageBehavior = false;
            SongsList.View = View.Details;
            SongsList.ItemDrag += SongsList_ItemDrag;
            SongsList.Enter += FormControl_Enter;
            SongsList.KeyUp += SongsList_KeyUp;
            SongsList.Leave += FormControl_Leave;
            SongsList.MouseDoubleClick += SongsList_MouseDoubleClick;
            SongsList.MouseUp += SongsList_MouseUp;
            // 
            // columnHeader2
            // 
            columnHeader2.Width = 0;
            // 
            // columnHeader3
            // 
            columnHeader3.Width = 0;
            // 
            // columnHeader4
            // 
            columnHeader4.Width = 0;
            // 
            // columnHeader5
            // 
            columnHeader5.TextAlign = HorizontalAlignment.Center;
            columnHeader5.Width = 0;
            // 
            // columnHeader6
            // 
            columnHeader6.Width = 0;
            // 
            // columnHeader7
            // 
            columnHeader7.Width = 0;
            // 
            // CMenuSongs
            // 
            CMenuSongs.ImageScalingSize = new System.Drawing.Size(24, 24);
            CMenuSongs.Items.AddRange(new ToolStripItem[] { CMenuSongs_SelectAll, CMenuSongs_UnselectAll, CMenuSongs_AddShow, toolStripSeparator38, CMenuSongs_Edit, CMenuSongs_Copy, toolStripSeparator10, CMenuSongs_Refresh });
            CMenuSongs.Name = "ContextMenuBibleText";
            CMenuSongs.Size = new System.Drawing.Size(163, 160);
            // 
            // tabPowerpoint
            // 
            tabPowerpoint.BackColor = System.Drawing.SystemColors.Control;
            tabPowerpoint.Controls.Add(flowLayoutExternalPowerPoint);
            tabPowerpoint.Controls.Add(panelPowerpoint2);
            tabPowerpoint.Controls.Add(PowerpointList);
            tabPowerpoint.Controls.Add(panelPowerpoint1);
            tabPowerpoint.Location = new System.Drawing.Point(4, 4);
            tabPowerpoint.Margin = new Padding(3, 5, 3, 5);
            tabPowerpoint.Name = "tabPowerpoint";
            tabPowerpoint.Size = new System.Drawing.Size(336, 478);
            tabPowerpoint.TabIndex = 5;
            tabPowerpoint.Text = "PowerP";
            // 
            // flowLayoutExternalPowerPoint
            // 
            flowLayoutExternalPowerPoint.AutoScroll = true;
            flowLayoutExternalPowerPoint.Location = new System.Drawing.Point(120, 68);
            flowLayoutExternalPowerPoint.Margin = new Padding(3, 5, 3, 5);
            flowLayoutExternalPowerPoint.Name = "flowLayoutExternalPowerPoint";
            flowLayoutExternalPowerPoint.Size = new System.Drawing.Size(79, 53);
            flowLayoutExternalPowerPoint.TabIndex = 18;
            // 
            // panelPowerpoint2
            // 
            panelPowerpoint2.Controls.Add(toolStripPowerpoint2);
            panelPowerpoint2.Location = new System.Drawing.Point(79, 41);
            panelPowerpoint2.Margin = new Padding(3, 5, 3, 5);
            panelPowerpoint2.Name = "panelPowerpoint2";
            panelPowerpoint2.Size = new System.Drawing.Size(33, 149);
            panelPowerpoint2.TabIndex = 20;
            // 
            // toolStripPowerpoint2
            // 
            toolStripPowerpoint2.AutoSize = false;
            toolStripPowerpoint2.CanOverflow = false;
            toolStripPowerpoint2.Dock = DockStyle.None;
            toolStripPowerpoint2.GripStyle = ToolStripGripStyle.Hidden;
            toolStripPowerpoint2.ImageScalingSize = new System.Drawing.Size(24, 24);
            toolStripPowerpoint2.Items.AddRange(new ToolStripItem[] { Powerpoint_Edit, Powerpoint_Copy, Powerpoint_Move, Powerpoint_Delete });
            toolStripPowerpoint2.LayoutStyle = ToolStripLayoutStyle.VerticalStackWithOverflow;
            toolStripPowerpoint2.Location = new System.Drawing.Point(0, -1);
            toolStripPowerpoint2.Name = "toolStripPowerpoint2";
            toolStripPowerpoint2.Padding = new Padding(0, 0, 2, 0);
            toolStripPowerpoint2.RenderMode = ToolStripRenderMode.System;
            toolStripPowerpoint2.Size = new System.Drawing.Size(33, 167);
            toolStripPowerpoint2.TabIndex = 0;
            // 
            // Powerpoint_Edit
            // 
            Powerpoint_Edit.AutoSize = false;
            Powerpoint_Edit.DisplayStyle = ToolStripItemDisplayStyle.Image;
            Powerpoint_Edit.Image = (System.Drawing.Image)resources.GetObject("Powerpoint_Edit.Image");
            Powerpoint_Edit.ImageTransparentColor = System.Drawing.Color.Magenta;
            Powerpoint_Edit.Name = "Powerpoint_Edit";
            Powerpoint_Edit.Size = new System.Drawing.Size(22, 22);
            Powerpoint_Edit.Tag = "edit";
            Powerpoint_Edit.ToolTipText = "Edit";
            Powerpoint_Edit.Click += Powerpoint_EditBtns_Click;
            // 
            // Powerpoint_Copy
            // 
            Powerpoint_Copy.AutoSize = false;
            Powerpoint_Copy.DisplayStyle = ToolStripItemDisplayStyle.Image;
            Powerpoint_Copy.Image = (System.Drawing.Image)resources.GetObject("Powerpoint_Copy.Image");
            Powerpoint_Copy.ImageTransparentColor = System.Drawing.Color.Magenta;
            Powerpoint_Copy.Name = "Powerpoint_Copy";
            Powerpoint_Copy.Size = new System.Drawing.Size(22, 22);
            Powerpoint_Copy.Tag = "copy";
            Powerpoint_Copy.ToolTipText = "Copy";
            Powerpoint_Copy.Click += Powerpoint_EditBtns_Click;
            // 
            // Powerpoint_Move
            // 
            Powerpoint_Move.DisplayStyle = ToolStripItemDisplayStyle.Image;
            Powerpoint_Move.Image = (System.Drawing.Image)resources.GetObject("Powerpoint_Move.Image");
            Powerpoint_Move.ImageTransparentColor = System.Drawing.Color.Magenta;
            Powerpoint_Move.Name = "Powerpoint_Move";
            Powerpoint_Move.Size = new System.Drawing.Size(30, 28);
            Powerpoint_Move.Tag = "move";
            Powerpoint_Move.ToolTipText = "Move";
            Powerpoint_Move.Click += Powerpoint_EditBtns_Click;
            // 
            // Powerpoint_Delete
            // 
            Powerpoint_Delete.DisplayStyle = ToolStripItemDisplayStyle.Image;
            Powerpoint_Delete.Image = (System.Drawing.Image)resources.GetObject("Powerpoint_Delete.Image");
            Powerpoint_Delete.ImageTransparentColor = System.Drawing.Color.Magenta;
            Powerpoint_Delete.Name = "Powerpoint_Delete";
            Powerpoint_Delete.Size = new System.Drawing.Size(30, 28);
            Powerpoint_Delete.Tag = "delete";
            Powerpoint_Delete.ToolTipText = "Delete";
            Powerpoint_Delete.Click += Powerpoint_EditBtns_Click;
            // 
            // PowerpointList
            // 
            PowerpointList.Columns.AddRange(new ColumnHeader[] { columnHeader30, columnHeader31, columnHeader32, columnHeader33, columnHeader34, columnHeader35, columnHeader36 });
            PowerpointList.ContextMenuStrip = CMenuFiles;
            PowerpointList.FullRowSelect = true;
            PowerpointList.HeaderStyle = ColumnHeaderStyle.None;
            PowerpointList.LabelWrap = false;
            PowerpointList.Location = new System.Drawing.Point(3, 41);
            PowerpointList.Margin = new Padding(3, 5, 3, 5);
            PowerpointList.Name = "PowerpointList";
            PowerpointList.ShowItemToolTips = true;
            PowerpointList.Size = new System.Drawing.Size(45, 112);
            PowerpointList.TabIndex = 19;
            PowerpointList.UseCompatibleStateImageBehavior = false;
            PowerpointList.View = View.Details;
            PowerpointList.ItemDrag += PowerpointList_ItemDrag;
            PowerpointList.Enter += FormControl_Enter;
            PowerpointList.KeyUp += PowerpointList_KeyUp;
            PowerpointList.Leave += FormControl_Leave;
            PowerpointList.MouseDoubleClick += PowerpointList_MouseDoubleClick;
            PowerpointList.MouseUp += PowerpointList_MouseUp;
            // 
            // columnHeader31
            // 
            columnHeader31.Width = 0;
            // 
            // columnHeader32
            // 
            columnHeader32.Width = 0;
            // 
            // columnHeader33
            // 
            columnHeader33.Width = 0;
            // 
            // columnHeader34
            // 
            columnHeader34.TextAlign = HorizontalAlignment.Center;
            columnHeader34.Width = 0;
            // 
            // tabControlLists
            // 
            tabControlLists.Alignment = TabAlignment.Bottom;
            tabControlLists.Controls.Add(tabWorshipList);
            tabControlLists.Controls.Add(tabPraiseBook);
            tabControlLists.Dock = DockStyle.Fill;
            tabControlLists.Location = new System.Drawing.Point(0, 0);
            tabControlLists.Margin = new Padding(3, 5, 3, 5);
            tabControlLists.Name = "tabControlLists";
            tabControlLists.Padding = new System.Drawing.Point(5, 3);
            tabControlLists.SelectedIndex = 0;
            tabControlLists.Size = new System.Drawing.Size(344, 134);
            tabControlLists.TabIndex = 0;
            tabControlLists.SelectedIndexChanged += tabControlLists_SelectedIndexChanged;
            tabControlLists.Resize += tabControlLists_Resize;
            // 
            // tabWorshipList
            // 
            tabWorshipList.BackColor = System.Drawing.SystemColors.Control;
            tabWorshipList.Controls.Add(panelWorshipList2);
            tabWorshipList.Controls.Add(panelWorshipList1);
            tabWorshipList.Controls.Add(WorshipListItems);
            tabWorshipList.Controls.Add(SessionList);
            tabWorshipList.Location = new System.Drawing.Point(4, 4);
            tabWorshipList.Margin = new Padding(3, 5, 3, 5);
            tabWorshipList.Name = "tabWorshipList";
            tabWorshipList.Padding = new Padding(3, 5, 3, 5);
            tabWorshipList.Size = new System.Drawing.Size(336, 101);
            tabWorshipList.TabIndex = 0;
            tabWorshipList.Text = "Worship List";
            // 
            // panelWorshipList2
            // 
            panelWorshipList2.Controls.Add(toolStripWorshipList2);
            panelWorshipList2.Location = new System.Drawing.Point(58, 41);
            panelWorshipList2.Margin = new Padding(3, 5, 3, 5);
            panelWorshipList2.Name = "panelWorshipList2";
            panelWorshipList2.Size = new System.Drawing.Size(33, 211);
            panelWorshipList2.TabIndex = 11;
            // 
            // toolStripWorshipList2
            // 
            toolStripWorshipList2.AutoSize = false;
            toolStripWorshipList2.CanOverflow = false;
            toolStripWorshipList2.Dock = DockStyle.None;
            toolStripWorshipList2.GripStyle = ToolStripGripStyle.Hidden;
            toolStripWorshipList2.ImageScalingSize = new System.Drawing.Size(24, 24);
            toolStripWorshipList2.Items.AddRange(new ToolStripItem[] { WL_Up, WL_Down, WL_Delete, toolStripSeparator6, WL_Word, WL_Notes });
            toolStripWorshipList2.LayoutStyle = ToolStripLayoutStyle.VerticalStackWithOverflow;
            toolStripWorshipList2.Location = new System.Drawing.Point(0, 0);
            toolStripWorshipList2.Name = "toolStripWorshipList2";
            toolStripWorshipList2.Padding = new Padding(0, 0, 2, 0);
            toolStripWorshipList2.RenderMode = ToolStripRenderMode.System;
            toolStripWorshipList2.Size = new System.Drawing.Size(33, 212);
            toolStripWorshipList2.TabIndex = 0;
            // 
            // WL_Up
            // 
            WL_Up.AutoSize = false;
            WL_Up.BackgroundImage = (System.Drawing.Image)resources.GetObject("WL_Up.BackgroundImage");
            WL_Up.BackgroundImageLayout = ImageLayout.Stretch;
            WL_Up.DisplayStyle = ToolStripItemDisplayStyle.Image;
            WL_Up.ImageTransparentColor = System.Drawing.Color.Magenta;
            WL_Up.Name = "WL_Up";
            WL_Up.Size = new System.Drawing.Size(22, 22);
            WL_Up.Tag = "up";
            WL_Up.ToolTipText = "Move Item Up";
            WL_Up.MouseUp += WL_Btn_MouseUp;
            // 
            // WL_Down
            // 
            WL_Down.AutoSize = false;
            WL_Down.BackgroundImage = (System.Drawing.Image)resources.GetObject("WL_Down.BackgroundImage");
            WL_Down.BackgroundImageLayout = ImageLayout.Stretch;
            WL_Down.DisplayStyle = ToolStripItemDisplayStyle.Image;
            WL_Down.ImageTransparentColor = System.Drawing.Color.Magenta;
            WL_Down.Name = "WL_Down";
            WL_Down.Size = new System.Drawing.Size(22, 22);
            WL_Down.Tag = "down";
            WL_Down.ToolTipText = "Move Item Down";
            WL_Down.MouseUp += WL_Btn_MouseUp;
            // 
            // WL_Delete
            // 
            WL_Delete.AutoSize = false;
            WL_Delete.BackgroundImage = (System.Drawing.Image)resources.GetObject("WL_Delete.BackgroundImage");
            WL_Delete.BackgroundImageLayout = ImageLayout.Stretch;
            WL_Delete.DisplayStyle = ToolStripItemDisplayStyle.Image;
            WL_Delete.ImageTransparentColor = System.Drawing.Color.Magenta;
            WL_Delete.Name = "WL_Delete";
            WL_Delete.Size = new System.Drawing.Size(22, 22);
            WL_Delete.Tag = "delete";
            WL_Delete.ToolTipText = "Delete";
            WL_Delete.MouseUp += WL_Btn_MouseUp;
            // 
            // toolStripSeparator6
            // 
            toolStripSeparator6.Name = "toolStripSeparator6";
            toolStripSeparator6.Size = new System.Drawing.Size(30, 6);
            // 
            // WL_Word
            // 
            WL_Word.AutoSize = false;
            WL_Word.BackgroundImage = (System.Drawing.Image)resources.GetObject("WL_Word.BackgroundImage");
            WL_Word.BackgroundImageLayout = ImageLayout.Stretch;
            WL_Word.DisplayStyle = ToolStripItemDisplayStyle.Image;
            WL_Word.ImageTransparentColor = System.Drawing.Color.Magenta;
            WL_Word.Name = "WL_Word";
            WL_Word.Size = new System.Drawing.Size(22, 22);
            WL_Word.Tag = "word";
            WL_Word.ToolTipText = "Generate RTF Document";
            WL_Word.MouseUp += WL_Btn_MouseUp;
            // 
            // WL_Notes
            // 
            WL_Notes.AutoSize = false;
            WL_Notes.BackgroundImage = (System.Drawing.Image)resources.GetObject("WL_Notes.BackgroundImage");
            WL_Notes.BackgroundImageLayout = ImageLayout.Stretch;
            WL_Notes.DisplayStyle = ToolStripItemDisplayStyle.Image;
            WL_Notes.ImageTransparentColor = System.Drawing.Color.Magenta;
            WL_Notes.Name = "WL_Notes";
            WL_Notes.Size = new System.Drawing.Size(22, 22);
            WL_Notes.ToolTipText = "Edit Session Notes";
            WL_Notes.MouseUp += WL_Btn_MouseUp;
            // 
            // panelWorshipList1
            // 
            panelWorshipList1.Controls.Add(toolStripWorshipList1);
            panelWorshipList1.Location = new System.Drawing.Point(88, 5);
            panelWorshipList1.Margin = new Padding(3, 5, 3, 5);
            panelWorshipList1.Name = "panelWorshipList1";
            panelWorshipList1.Size = new System.Drawing.Size(94, 33);
            panelWorshipList1.TabIndex = 7;
            // 
            // toolStripWorshipList1
            // 
            toolStripWorshipList1.AutoSize = false;
            toolStripWorshipList1.CanOverflow = false;
            toolStripWorshipList1.Dock = DockStyle.None;
            toolStripWorshipList1.GripStyle = ToolStripGripStyle.Hidden;
            toolStripWorshipList1.ImageScalingSize = new System.Drawing.Size(24, 24);
            toolStripWorshipList1.Items.AddRange(new ToolStripItem[] { WL_Manage, WL_Add, WL_Open });
            toolStripWorshipList1.LayoutStyle = ToolStripLayoutStyle.HorizontalStackWithOverflow;
            toolStripWorshipList1.Location = new System.Drawing.Point(0, -1);
            toolStripWorshipList1.Name = "toolStripWorshipList1";
            toolStripWorshipList1.Padding = new Padding(0, 0, 2, 0);
            toolStripWorshipList1.RenderMode = ToolStripRenderMode.System;
            toolStripWorshipList1.Size = new System.Drawing.Size(111, 39);
            toolStripWorshipList1.TabIndex = 0;
            // 
            // WL_Manage
            // 
            WL_Manage.DisplayStyle = ToolStripItemDisplayStyle.Image;
            WL_Manage.Image = (System.Drawing.Image)resources.GetObject("WL_Manage.Image");
            WL_Manage.ImageTransparentColor = System.Drawing.Color.Magenta;
            WL_Manage.Name = "WL_Manage";
            WL_Manage.Size = new System.Drawing.Size(29, 36);
            WL_Manage.Tag = "list";
            WL_Manage.ToolTipText = "Manage Worship Lists";
            WL_Manage.MouseUp += WL_Btn_MouseUp;
            // 
            // WL_Add
            // 
            WL_Add.DisplayStyle = ToolStripItemDisplayStyle.Image;
            WL_Add.Image = (System.Drawing.Image)resources.GetObject("WL_Add.Image");
            WL_Add.ImageTransparentColor = System.Drawing.Color.Magenta;
            WL_Add.Name = "WL_Add";
            WL_Add.Size = new System.Drawing.Size(29, 36);
            WL_Add.Tag = "add";
            WL_Add.ToolTipText = "Add to Worship List";
            WL_Add.MouseUp += WL_Btn_MouseUp;
            // 
            // WL_Open
            // 
            WL_Open.DisplayStyle = ToolStripItemDisplayStyle.Image;
            WL_Open.Image = (System.Drawing.Image)resources.GetObject("WL_Open.Image");
            WL_Open.ImageTransparentColor = System.Drawing.Color.Magenta;
            WL_Open.Name = "WL_Open";
            WL_Open.Size = new System.Drawing.Size(29, 36);
            WL_Open.Tag = "open";
            WL_Open.ToolTipText = "Add External Document to Worship List";
            WL_Open.MouseUp += WL_Btn_MouseUp;
            // 
            // WorshipListItems
            // 
            WorshipListItems.AllowDrop = true;
            WorshipListItems.Columns.AddRange(new ColumnHeader[] { columnHeader8, columnHeader9, columnHeader10, columnHeader11, columnHeader12, columnHeader13, columnHeader14 });
            WorshipListItems.ContextMenuStrip = CMenuWorship;
            WorshipListItems.FullRowSelect = true;
            WorshipListItems.HeaderStyle = ColumnHeaderStyle.None;
            WorshipListItems.LabelWrap = false;
            WorshipListItems.Location = new System.Drawing.Point(3, 41);
            WorshipListItems.Margin = new Padding(3, 5, 3, 5);
            WorshipListItems.Name = "WorshipListItems";
            WorshipListItems.ShowItemToolTips = true;
            WorshipListItems.Size = new System.Drawing.Size(45, 112);
            WorshipListItems.SmallImageList = imageListSys;
            WorshipListItems.TabIndex = 1;
            WorshipListItems.UseCompatibleStateImageBehavior = false;
            WorshipListItems.View = View.Details;
            WorshipListItems.ItemDrag += WorshipList_ItemDrag;
            WorshipListItems.DragDrop += WorshipList_DragDrop;
            WorshipListItems.DragEnter += WorshipList_DragEnter;
            WorshipListItems.DragOver += WorshipList_DragOver;
            WorshipListItems.DragLeave += WorshipList_DragLeave;
            WorshipListItems.DoubleClick += WorshipListItems_DoubleClick;
            WorshipListItems.Enter += FormControl_Enter;
            WorshipListItems.KeyUp += WorshipList_KeyUp;
            WorshipListItems.Leave += FormControl_Leave;
            WorshipListItems.MouseUp += WorshipList_MouseUp;
            // 
            // columnHeader9
            // 
            columnHeader9.Width = 0;
            // 
            // columnHeader10
            // 
            columnHeader10.Width = 0;
            // 
            // columnHeader11
            // 
            columnHeader11.Width = 0;
            // 
            // columnHeader12
            // 
            columnHeader12.Width = 0;
            // 
            // columnHeader13
            // 
            columnHeader13.Width = 0;
            // 
            // columnHeader14
            // 
            columnHeader14.Width = 0;
            // 
            // CMenuWorship
            // 
            CMenuWorship.ImageScalingSize = new System.Drawing.Size(24, 24);
            CMenuWorship.Items.AddRange(new ToolStripItem[] { CMenuWorship_SelectAll, CMenuWorship_UnselectAll, CMenuWorship_Clear, toolStripSeparator39, CMenuWorship_Edit, CMenuWorship_Play, CMenuWorship_PlayOnOutput, toolStripSeparator37, CMenuWorship_AddUsages });
            CMenuWorship.Name = "ContextMenuBibleText";
            CMenuWorship.Size = new System.Drawing.Size(280, 184);
            // 
            // CMenuWorship_SelectAll
            // 
            CMenuWorship_SelectAll.Name = "CMenuWorship_SelectAll";
            CMenuWorship_SelectAll.Size = new System.Drawing.Size(279, 24);
            CMenuWorship_SelectAll.Text = "Select &All";
            CMenuWorship_SelectAll.Click += CMenuWorship_SelectAll_Click;
            // 
            // CMenuWorship_UnselectAll
            // 
            CMenuWorship_UnselectAll.Name = "CMenuWorship_UnselectAll";
            CMenuWorship_UnselectAll.Size = new System.Drawing.Size(279, 24);
            CMenuWorship_UnselectAll.Text = "&Unselect All";
            CMenuWorship_UnselectAll.Click += CMenuWorship_UnselectAll_Click;
            // 
            // CMenuWorship_Clear
            // 
            CMenuWorship_Clear.Name = "CMenuWorship_Clear";
            CMenuWorship_Clear.Size = new System.Drawing.Size(279, 24);
            CMenuWorship_Clear.Text = "Clear Worship List";
            CMenuWorship_Clear.Click += CMenuWorship_Clear_Click;
            // 
            // toolStripSeparator39
            // 
            toolStripSeparator39.Name = "toolStripSeparator39";
            toolStripSeparator39.Size = new System.Drawing.Size(276, 6);
            // 
            // CMenuWorship_Edit
            // 
            CMenuWorship_Edit.Name = "CMenuWorship_Edit";
            CMenuWorship_Edit.Size = new System.Drawing.Size(279, 24);
            CMenuWorship_Edit.Text = "Edit item";
            CMenuWorship_Edit.Click += CMenuWorship_Edit_Click;
            // 
            // CMenuWorship_Play
            // 
            CMenuWorship_Play.Name = "CMenuWorship_Play";
            CMenuWorship_Play.Size = new System.Drawing.Size(279, 24);
            CMenuWorship_Play.Text = "Play Media";
            CMenuWorship_Play.Click += CMenuWorship_Play_Click;
            // 
            // CMenuWorship_PlayOnOutput
            // 
            CMenuWorship_PlayOnOutput.Name = "CMenuWorship_PlayOnOutput";
            CMenuWorship_PlayOnOutput.Size = new System.Drawing.Size(279, 24);
            CMenuWorship_PlayOnOutput.Text = "Play Media on Output Monitor";
            CMenuWorship_PlayOnOutput.Click += CMenuWorship_PlayOnOutput_Click;
            // 
            // toolStripSeparator37
            // 
            toolStripSeparator37.Name = "toolStripSeparator37";
            toolStripSeparator37.Size = new System.Drawing.Size(276, 6);
            // 
            // CMenuWorship_AddUsages
            // 
            CMenuWorship_AddUsages.Name = "CMenuWorship_AddUsages";
            CMenuWorship_AddUsages.Size = new System.Drawing.Size(279, 24);
            CMenuWorship_AddUsages.Text = "Add Songs to Usages";
            CMenuWorship_AddUsages.Click += CMenuWorship_AddUsages_Click;
            // 
            // SessionList
            // 
            SessionList.DropDownStyle = ComboBoxStyle.DropDownList;
            SessionList.FormattingEnabled = true;
            SessionList.Location = new System.Drawing.Point(3, 5);
            SessionList.Margin = new Padding(3, 5, 3, 5);
            SessionList.MaxDropDownItems = 12;
            SessionList.Name = "SessionList";
            SessionList.Size = new System.Drawing.Size(75, 28);
            SessionList.TabIndex = 0;
            SessionList.SelectedValueChanged += SessionList_SelectedValueChanged;
            // 
            // tabPraiseBook
            // 
            tabPraiseBook.BackColor = System.Drawing.SystemColors.Control;
            tabPraiseBook.Controls.Add(panelPraiseBook2);
            tabPraiseBook.Controls.Add(panelPraiseBook1);
            tabPraiseBook.Controls.Add(PraiseBookItems);
            tabPraiseBook.Controls.Add(PraiseBook);
            tabPraiseBook.Location = new System.Drawing.Point(4, 4);
            tabPraiseBook.Margin = new Padding(3, 5, 3, 5);
            tabPraiseBook.Name = "tabPraiseBook";
            tabPraiseBook.Padding = new Padding(3, 5, 3, 5);
            tabPraiseBook.Size = new System.Drawing.Size(336, 101);
            tabPraiseBook.TabIndex = 1;
            tabPraiseBook.Text = "Praise Book";
            // 
            // panelPraiseBook2
            // 
            panelPraiseBook2.Controls.Add(toolStripPraiseBook2);
            panelPraiseBook2.Location = new System.Drawing.Point(58, 41);
            panelPraiseBook2.Margin = new Padding(3, 5, 3, 5);
            panelPraiseBook2.Name = "panelPraiseBook2";
            panelPraiseBook2.Size = new System.Drawing.Size(33, 132);
            panelPraiseBook2.TabIndex = 12;
            // 
            // toolStripPraiseBook2
            // 
            toolStripPraiseBook2.AutoSize = false;
            toolStripPraiseBook2.CanOverflow = false;
            toolStripPraiseBook2.Dock = DockStyle.None;
            toolStripPraiseBook2.GripStyle = ToolStripGripStyle.Hidden;
            toolStripPraiseBook2.ImageScalingSize = new System.Drawing.Size(24, 24);
            toolStripPraiseBook2.Items.AddRange(new ToolStripItem[] { toolStripSeparator22, PB_Delete, toolStripSeparator7, PB_Word, PB_Html });
            toolStripPraiseBook2.LayoutStyle = ToolStripLayoutStyle.VerticalStackWithOverflow;
            toolStripPraiseBook2.Location = new System.Drawing.Point(0, 0);
            toolStripPraiseBook2.Name = "toolStripPraiseBook2";
            toolStripPraiseBook2.Padding = new Padding(0, 0, 2, 0);
            toolStripPraiseBook2.RenderMode = ToolStripRenderMode.System;
            toolStripPraiseBook2.Size = new System.Drawing.Size(33, 135);
            toolStripPraiseBook2.TabIndex = 0;
            // 
            // toolStripSeparator22
            // 
            toolStripSeparator22.Name = "toolStripSeparator22";
            toolStripSeparator22.Size = new System.Drawing.Size(30, 6);
            // 
            // PB_Delete
            // 
            PB_Delete.AutoSize = false;
            PB_Delete.DisplayStyle = ToolStripItemDisplayStyle.Image;
            PB_Delete.Image = (System.Drawing.Image)resources.GetObject("PB_Delete.Image");
            PB_Delete.ImageTransparentColor = System.Drawing.Color.Magenta;
            PB_Delete.Name = "PB_Delete";
            PB_Delete.Size = new System.Drawing.Size(23, 22);
            PB_Delete.Tag = "delete";
            PB_Delete.ToolTipText = "Delete";
            PB_Delete.MouseUp += PB_Btn_MouseUp;
            // 
            // toolStripSeparator7
            // 
            toolStripSeparator7.Name = "toolStripSeparator7";
            toolStripSeparator7.Size = new System.Drawing.Size(30, 6);
            // 
            // PB_Word
            // 
            PB_Word.AutoSize = false;
            PB_Word.DisplayStyle = ToolStripItemDisplayStyle.Image;
            PB_Word.Image = (System.Drawing.Image)resources.GetObject("PB_Word.Image");
            PB_Word.ImageTransparentColor = System.Drawing.Color.Magenta;
            PB_Word.Name = "PB_Word";
            PB_Word.Size = new System.Drawing.Size(22, 22);
            PB_Word.Tag = "word";
            PB_Word.ToolTipText = "Generate RTF Document";
            PB_Word.MouseUp += PB_Btn_MouseUp;
            // 
            // PB_Html
            // 
            PB_Html.AutoSize = false;
            PB_Html.DisplayStyle = ToolStripItemDisplayStyle.Image;
            PB_Html.Image = (System.Drawing.Image)resources.GetObject("PB_Html.Image");
            PB_Html.ImageTransparentColor = System.Drawing.Color.Magenta;
            PB_Html.Name = "PB_Html";
            PB_Html.Size = new System.Drawing.Size(22, 22);
            PB_Html.Tag = "ie";
            PB_Html.ToolTipText = "Generate HTML Document";
            PB_Html.MouseUp += PB_Btn_MouseUp;
            // 
            // panelPraiseBook1
            // 
            panelPraiseBook1.Controls.Add(toolStripPraiseBook1);
            panelPraiseBook1.Location = new System.Drawing.Point(88, 5);
            panelPraiseBook1.Margin = new Padding(3, 5, 3, 5);
            panelPraiseBook1.Name = "panelPraiseBook1";
            panelPraiseBook1.Size = new System.Drawing.Size(94, 33);
            panelPraiseBook1.TabIndex = 10;
            // 
            // toolStripPraiseBook1
            // 
            toolStripPraiseBook1.AutoSize = false;
            toolStripPraiseBook1.CanOverflow = false;
            toolStripPraiseBook1.Dock = DockStyle.None;
            toolStripPraiseBook1.GripStyle = ToolStripGripStyle.Hidden;
            toolStripPraiseBook1.ImageScalingSize = new System.Drawing.Size(24, 24);
            toolStripPraiseBook1.Items.AddRange(new ToolStripItem[] { PB_Manage, PB_Add, PB_WordCount });
            toolStripPraiseBook1.LayoutStyle = ToolStripLayoutStyle.HorizontalStackWithOverflow;
            toolStripPraiseBook1.Location = new System.Drawing.Point(0, -1);
            toolStripPraiseBook1.Name = "toolStripPraiseBook1";
            toolStripPraiseBook1.Padding = new Padding(0, 0, 2, 0);
            toolStripPraiseBook1.RenderMode = ToolStripRenderMode.System;
            toolStripPraiseBook1.Size = new System.Drawing.Size(111, 39);
            toolStripPraiseBook1.TabIndex = 0;
            // 
            // PB_Manage
            // 
            PB_Manage.DisplayStyle = ToolStripItemDisplayStyle.Image;
            PB_Manage.Image = (System.Drawing.Image)resources.GetObject("PB_Manage.Image");
            PB_Manage.ImageTransparentColor = System.Drawing.Color.Magenta;
            PB_Manage.Name = "PB_Manage";
            PB_Manage.Size = new System.Drawing.Size(29, 36);
            PB_Manage.Tag = "list";
            PB_Manage.ToolTipText = "Manage PraiseBooks";
            PB_Manage.MouseUp += PB_Btn_MouseUp;
            // 
            // PB_Add
            // 
            PB_Add.DisplayStyle = ToolStripItemDisplayStyle.Image;
            PB_Add.Image = (System.Drawing.Image)resources.GetObject("PB_Add.Image");
            PB_Add.ImageTransparentColor = System.Drawing.Color.Magenta;
            PB_Add.Name = "PB_Add";
            PB_Add.Size = new System.Drawing.Size(29, 36);
            PB_Add.Tag = "add";
            PB_Add.ToolTipText = "Add to PraiseBook";
            PB_Add.MouseUp += PB_Btn_MouseUp;
            // 
            // PB_WordCount
            // 
            PB_WordCount.CheckOnClick = true;
            PB_WordCount.DisplayStyle = ToolStripItemDisplayStyle.Image;
            PB_WordCount.Image = (System.Drawing.Image)resources.GetObject("PB_WordCount.Image");
            PB_WordCount.ImageTransparentColor = System.Drawing.Color.Magenta;
            PB_WordCount.Name = "PB_WordCount";
            PB_WordCount.Size = new System.Drawing.Size(29, 36);
            PB_WordCount.Tag = "wordcount";
            PB_WordCount.ToolTipText = "Sort by CJK Word Count";
            PB_WordCount.MouseUp += PB_Btn_MouseUp;
            // 
            // PraiseBookItems
            // 
            PraiseBookItems.AllowDrop = true;
            PraiseBookItems.Columns.AddRange(new ColumnHeader[] { columnHeader17, columnHeader18, columnHeader19, columnHeader20, columnHeader21, columnHeader22 });
            PraiseBookItems.ContextMenuStrip = CMenuPraiseB;
            PraiseBookItems.FullRowSelect = true;
            PraiseBookItems.HeaderStyle = ColumnHeaderStyle.None;
            PraiseBookItems.LabelWrap = false;
            PraiseBookItems.Location = new System.Drawing.Point(3, 41);
            PraiseBookItems.Margin = new Padding(3, 5, 3, 5);
            PraiseBookItems.Name = "PraiseBookItems";
            PraiseBookItems.ShowItemToolTips = true;
            PraiseBookItems.Size = new System.Drawing.Size(45, 112);
            PraiseBookItems.Sorting = SortOrder.Ascending;
            PraiseBookItems.TabIndex = 1;
            PraiseBookItems.UseCompatibleStateImageBehavior = false;
            PraiseBookItems.View = View.Details;
            PraiseBookItems.DragDrop += PraiseBookItems_DragDrop;
            PraiseBookItems.DragEnter += PraiseBookItems_DragEnter;
            PraiseBookItems.Enter += FormControl_Enter;
            PraiseBookItems.KeyUp += PraiseBookItems_KeyUp;
            PraiseBookItems.Leave += FormControl_Leave;
            PraiseBookItems.MouseUp += PraiseBookItems_MouseUp;
            // 
            // columnHeader17
            // 
            columnHeader17.Width = 0;
            // 
            // columnHeader18
            // 
            columnHeader18.Width = 0;
            // 
            // columnHeader19
            // 
            columnHeader19.Width = 0;
            // 
            // columnHeader20
            // 
            columnHeader20.Width = 0;
            // 
            // columnHeader21
            // 
            columnHeader21.Width = 0;
            // 
            // columnHeader22
            // 
            columnHeader22.Width = 0;
            // 
            // CMenuPraiseB
            // 
            CMenuPraiseB.ImageScalingSize = new System.Drawing.Size(24, 24);
            CMenuPraiseB.Items.AddRange(new ToolStripItem[] { CMenuPraiseB_SelectAll, CMenuPraiseB_UnselectAll, CMenuPraiseB_Clear, toolStripSeparator36, CMenuPraiseB_Edit });
            CMenuPraiseB.Name = "ContextMenuBibleText";
            CMenuPraiseB.Size = new System.Drawing.Size(216, 106);
            // 
            // CMenuPraiseB_SelectAll
            // 
            CMenuPraiseB_SelectAll.Name = "CMenuPraiseB_SelectAll";
            CMenuPraiseB_SelectAll.Size = new System.Drawing.Size(215, 24);
            CMenuPraiseB_SelectAll.Text = "Select &All";
            CMenuPraiseB_SelectAll.Click += CMenuPraiseB_SelectAll_Click;
            // 
            // CMenuPraiseB_UnselectAll
            // 
            CMenuPraiseB_UnselectAll.Name = "CMenuPraiseB_UnselectAll";
            CMenuPraiseB_UnselectAll.Size = new System.Drawing.Size(215, 24);
            CMenuPraiseB_UnselectAll.Text = "&Unselect All";
            CMenuPraiseB_UnselectAll.Click += CMenuPraiseB_UnselectAll_Click;
            // 
            // CMenuPraiseB_Clear
            // 
            CMenuPraiseB_Clear.Name = "CMenuPraiseB_Clear";
            CMenuPraiseB_Clear.Size = new System.Drawing.Size(215, 24);
            CMenuPraiseB_Clear.Text = "Clear PraiseBook List";
            CMenuPraiseB_Clear.Click += CMenuPraiseB_Clear_Click;
            // 
            // toolStripSeparator36
            // 
            toolStripSeparator36.Name = "toolStripSeparator36";
            toolStripSeparator36.Size = new System.Drawing.Size(212, 6);
            // 
            // CMenuPraiseB_Edit
            // 
            CMenuPraiseB_Edit.Name = "CMenuPraiseB_Edit";
            CMenuPraiseB_Edit.Size = new System.Drawing.Size(215, 24);
            CMenuPraiseB_Edit.Text = "Edit item";
            CMenuPraiseB_Edit.Click += CMenuPraiseB_Edit_Click;
            // 
            // PraiseBook
            // 
            PraiseBook.DropDownStyle = ComboBoxStyle.DropDownList;
            PraiseBook.FormattingEnabled = true;
            PraiseBook.Location = new System.Drawing.Point(3, 5);
            PraiseBook.Margin = new Padding(3, 5, 3, 5);
            PraiseBook.MaxDropDownItems = 12;
            PraiseBook.Name = "PraiseBook";
            PraiseBook.Size = new System.Drawing.Size(75, 28);
            PraiseBook.TabIndex = 0;
            PraiseBook.SelectedIndexChanged += PraiseBook_SelectedIndexChanged;
            // 
            // splitContainer2
            // 
            splitContainer2.BorderStyle = BorderStyle.Fixed3D;
            splitContainer2.Dock = DockStyle.Fill;
            splitContainer2.Location = new System.Drawing.Point(0, 0);
            splitContainer2.Margin = new Padding(3, 5, 3, 5);
            splitContainer2.Name = "splitContainer2";
            // 
            // splitContainer2.Panel1
            // 
            splitContainer2.Panel1.Controls.Add(splitContainerPreview);
            // 
            // splitContainer2.Panel2
            // 
            splitContainer2.Panel2.Controls.Add(splitContainerOutput);
            splitContainer2.Size = new System.Drawing.Size(628, 658);
            splitContainer2.SplitterDistance = 372;
            splitContainer2.SplitterWidth = 3;
            splitContainer2.TabIndex = 0;
            splitContainer2.Text = "splitContainer2";
            // 
            // splitContainerPreview
            // 
            splitContainerPreview.Dock = DockStyle.Fill;
            splitContainerPreview.Location = new System.Drawing.Point(0, 0);
            splitContainerPreview.Margin = new Padding(3, 5, 3, 5);
            splitContainerPreview.Name = "splitContainerPreview";
            splitContainerPreview.Orientation = Orientation.Horizontal;
            // 
            // splitContainerPreview.Panel1
            // 
            splitContainerPreview.Panel1.Controls.Add(panelPreviewTop);
            splitContainerPreview.Panel1.Controls.Add(panel9);
            splitContainerPreview.Panel1MinSize = 50;
            // 
            // splitContainerPreview.Panel2
            // 
            splitContainerPreview.Panel2.BackColor = System.Drawing.SystemColors.Control;
            splitContainerPreview.Panel2.Controls.Add(panelPreviewBottom);
            splitContainerPreview.Panel2.Controls.Add(panel7);
            splitContainerPreview.Panel2.Controls.Add(panel1);
            splitContainerPreview.Size = new System.Drawing.Size(368, 654);
            splitContainerPreview.SplitterDistance = 573;
            splitContainerPreview.SplitterWidth = 5;
            splitContainerPreview.TabIndex = 0;
            splitContainerPreview.Text = "splitContainer3";
            // 
            // panelPreviewTop
            // 
            panelPreviewTop.Controls.Add(flowLayoutPreviewPowerPoint);
            panelPreviewTop.Controls.Add(IndPanel);
            panelPreviewTop.Controls.Add(PreviewInfo);
            panelPreviewTop.Controls.Add(flowLayoutPreviewLyrics);
            panelPreviewTop.Dock = DockStyle.Fill;
            panelPreviewTop.Location = new System.Drawing.Point(0, 33);
            panelPreviewTop.Margin = new Padding(3, 5, 3, 5);
            panelPreviewTop.Name = "panelPreviewTop";
            panelPreviewTop.Size = new System.Drawing.Size(368, 540);
            panelPreviewTop.TabIndex = 1;
            panelPreviewTop.Resize += panelPreviewTop_Resize;
            // 
            // flowLayoutPreviewPowerPoint
            // 
            flowLayoutPreviewPowerPoint.AutoScroll = true;
            flowLayoutPreviewPowerPoint.Location = new System.Drawing.Point(3, 201);
            flowLayoutPreviewPowerPoint.Margin = new Padding(3, 5, 3, 5);
            flowLayoutPreviewPowerPoint.Name = "flowLayoutPreviewPowerPoint";
            flowLayoutPreviewPowerPoint.Size = new System.Drawing.Size(79, 53);
            flowLayoutPreviewPowerPoint.TabIndex = 5;
            flowLayoutPreviewPowerPoint.KeyUp += new KeyEventHandler(flowLayoutPreviewPowerPoint_KeyUp);
            // 
            // IndPanel
            // 
            IndPanel.AutoScroll = true;
            IndPanel.Controls.Add(panelIndTemplate);
            IndPanel.Controls.Add(IndgroupBox4);
            IndPanel.Controls.Add(IndgroupBox3);
            IndPanel.Controls.Add(IndgroupBox2);
            IndPanel.Controls.Add(IndgroupBox1);
            IndPanel.Controls.Add(Ind_checkBox);
            IndPanel.Location = new System.Drawing.Point(24, 11);
            IndPanel.Margin = new Padding(3, 5, 3, 5);
            IndPanel.Name = "IndPanel";
            IndPanel.Size = new System.Drawing.Size(338, 520);
            IndPanel.TabIndex = 2;
            // 
            // panelIndTemplate
            // 
            panelIndTemplate.Controls.Add(toolStripIndTemplates);
            panelIndTemplate.Location = new System.Drawing.Point(200, 5);
            panelIndTemplate.Margin = new Padding(3, 5, 3, 5);
            panelIndTemplate.Name = "panelIndTemplate";
            panelIndTemplate.Size = new System.Drawing.Size(64, 33);
            panelIndTemplate.TabIndex = 12;
            // 
            // toolStripIndTemplates
            // 
            toolStripIndTemplates.AutoSize = false;
            toolStripIndTemplates.CanOverflow = false;
            toolStripIndTemplates.Dock = DockStyle.None;
            toolStripIndTemplates.GripStyle = ToolStripGripStyle.Hidden;
            toolStripIndTemplates.ImageScalingSize = new System.Drawing.Size(24, 24);
            toolStripIndTemplates.Items.AddRange(new ToolStripItem[] { Ind_LoadTemplate, Ind_SaveTemplate });
            toolStripIndTemplates.LayoutStyle = ToolStripLayoutStyle.HorizontalStackWithOverflow;
            toolStripIndTemplates.Location = new System.Drawing.Point(0, -1);
            toolStripIndTemplates.Name = "toolStripIndTemplates";
            toolStripIndTemplates.Padding = new Padding(0, 0, 2, 0);
            toolStripIndTemplates.RenderMode = ToolStripRenderMode.System;
            toolStripIndTemplates.Size = new System.Drawing.Size(66, 39);
            toolStripIndTemplates.TabIndex = 0;
            // 
            // Ind_LoadTemplate
            // 
            Ind_LoadTemplate.DisplayStyle = ToolStripItemDisplayStyle.Image;
            Ind_LoadTemplate.Image = (System.Drawing.Image)resources.GetObject("Ind_LoadTemplate.Image");
            Ind_LoadTemplate.ImageTransparentColor = System.Drawing.Color.Magenta;
            Ind_LoadTemplate.Name = "Ind_LoadTemplate";
            Ind_LoadTemplate.Size = new System.Drawing.Size(29, 36);
            Ind_LoadTemplate.ToolTipText = "Load Settings Template";
            Ind_LoadTemplate.MouseUp += Ind_Items_MouseUp;
            // 
            // Ind_SaveTemplate
            // 
            Ind_SaveTemplate.DisplayStyle = ToolStripItemDisplayStyle.Image;
            Ind_SaveTemplate.Image = (System.Drawing.Image)resources.GetObject("Ind_SaveTemplate.Image");
            Ind_SaveTemplate.ImageTransparentColor = System.Drawing.Color.Magenta;
            Ind_SaveTemplate.Name = "Ind_SaveTemplate";
            Ind_SaveTemplate.Size = new System.Drawing.Size(29, 36);
            Ind_SaveTemplate.ToolTipText = "Save Settings as a Template";
            Ind_SaveTemplate.MouseUp += Ind_Items_MouseUp;
            // 
            // IndgroupBox4
            // 
            IndgroupBox4.Controls.Add(panelInd7);
            IndgroupBox4.Controls.Add(Ind_Reg2SizeUpDown);
            IndgroupBox4.Controls.Add(label6);
            IndgroupBox4.Controls.Add(Ind_Reg2TopUpDown);
            IndgroupBox4.Controls.Add(panelInd6);
            IndgroupBox4.Controls.Add(label7);
            IndgroupBox4.Location = new System.Drawing.Point(8, 407);
            IndgroupBox4.Margin = new Padding(3, 5, 3, 5);
            IndgroupBox4.Name = "IndgroupBox4";
            IndgroupBox4.Padding = new Padding(3, 5, 3, 5);
            IndgroupBox4.Size = new System.Drawing.Size(320, 109);
            IndgroupBox4.TabIndex = 3;
            IndgroupBox4.TabStop = false;
            IndgroupBox4.Text = "Region 2";
            // 
            // panelInd7
            // 
            panelInd7.Controls.Add(toolStripInd7);
            panelInd7.Location = new System.Drawing.Point(9, 67);
            panelInd7.Margin = new Padding(3, 5, 3, 5);
            panelInd7.Name = "panelInd7";
            panelInd7.Size = new System.Drawing.Size(201, 33);
            panelInd7.TabIndex = 12;
            // 
            // toolStripInd7
            // 
            toolStripInd7.AutoSize = false;
            toolStripInd7.CanOverflow = false;
            toolStripInd7.Dock = DockStyle.None;
            toolStripInd7.GripStyle = ToolStripGripStyle.Hidden;
            toolStripInd7.ImageScalingSize = new System.Drawing.Size(24, 24);
            toolStripInd7.Items.AddRange(new ToolStripItem[] { Ind_Reg2FontsList });
            toolStripInd7.LayoutStyle = ToolStripLayoutStyle.HorizontalStackWithOverflow;
            toolStripInd7.Location = new System.Drawing.Point(0, -1);
            toolStripInd7.Name = "toolStripInd7";
            toolStripInd7.Padding = new Padding(0, 0, 2, 0);
            toolStripInd7.RenderMode = ToolStripRenderMode.System;
            toolStripInd7.Size = new System.Drawing.Size(207, 39);
            toolStripInd7.TabIndex = 5;
            // 
            // Ind_Reg2FontsList
            // 
            Ind_Reg2FontsList.AutoSize = false;
            Ind_Reg2FontsList.DropDownStyle = ComboBoxStyle.DropDownList;
            Ind_Reg2FontsList.Items.AddRange(new object[] { "No Media", "Show Media", "Hide Media" });
            Ind_Reg2FontsList.MaxDropDownItems = 12;
            Ind_Reg2FontsList.Name = "Ind_Reg2FontsList";
            Ind_Reg2FontsList.Size = new System.Drawing.Size(195, 28);
            Ind_Reg2FontsList.SelectedIndexChanged += Ind_FontsList_SelectedIndexChanged;
            // 
            // Ind_Reg2SizeUpDown
            // 
            Ind_Reg2SizeUpDown.Location = new System.Drawing.Point(250, 68);
            Ind_Reg2SizeUpDown.Margin = new Padding(3, 5, 3, 5);
            Ind_Reg2SizeUpDown.Minimum = new decimal(new int[] { 6, 0, 0, 0 });
            Ind_Reg2SizeUpDown.Name = "Ind_Reg2SizeUpDown";
            Ind_Reg2SizeUpDown.Size = new System.Drawing.Size(59, 27);
            Ind_Reg2SizeUpDown.TabIndex = 3;
            Ind_Reg2SizeUpDown.Value = new decimal(new int[] { 6, 0, 0, 0 });
            Ind_Reg2SizeUpDown.MouseUp += Ind_FontSizeUpDown_MouseUp;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new System.Drawing.Point(216, 69);
            label6.Name = "label6";
            label6.Size = new System.Drawing.Size(39, 20);
            label6.TabIndex = 2;
            label6.Text = "Size:";
            // 
            // Ind_Reg2TopUpDown
            // 
            Ind_Reg2TopUpDown.Location = new System.Drawing.Point(250, 29);
            Ind_Reg2TopUpDown.Margin = new Padding(3, 5, 3, 5);
            Ind_Reg2TopUpDown.Name = "Ind_Reg2TopUpDown";
            Ind_Reg2TopUpDown.Size = new System.Drawing.Size(59, 27);
            Ind_Reg2TopUpDown.TabIndex = 1;
            Ind_Reg2TopUpDown.MouseUp += Ind_MarginUpDown_MouseUp;
            // 
            // panelInd6
            // 
            panelInd6.Controls.Add(toolStripInd6);
            panelInd6.Location = new System.Drawing.Point(9, 28);
            panelInd6.Margin = new Padding(3, 5, 3, 5);
            panelInd6.Name = "panelInd6";
            panelInd6.Size = new System.Drawing.Size(207, 33);
            panelInd6.TabIndex = 10;
            // 
            // FrmMain
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            AutoSize = true;
            ClientSize = new System.Drawing.Size(979, 752);
            Controls.Add(toolStripContainerMain);
            Controls.Add(menuStripMain);
            Controls.Add(statusStripMain);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            MainMenuStrip = menuStripMain;
            Margin = new Padding(3, 5, 3, 5);
            Name = "FrmMain";
            StartPosition = FormStartPosition.Manual;
            Text = "EasiSlides";
            TopMost = true;
            FormClosing += Form1_FormClosing;
            Load += Form1_Load;
            Resize += Form1_Resize;
            flowLayoutPanel1.ResumeLayout(false);
            panel1.ResumeLayout(false);
            panel3.ResumeLayout(false);
            panel3.PerformLayout();
            panelPreviewSessionNotes2.ResumeLayout(false);
            panelPreviewBottom.ResumeLayout(false);
            panel9.ResumeLayout(false);
            panelOutputTop.ResumeLayout(false);
            panelOutputTop.PerformLayout();
            panel10.ResumeLayout(false);
            panelOutputBottom.ResumeLayout(false);
            panelOutputLM1.ResumeLayout(false);
            panelOutputLM1.PerformLayout();
            panelOutputLM3.ResumeLayout(false);
            splitContainerOutput.Panel1.ResumeLayout(false);
            splitContainerOutput.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainerOutput).EndInit();
            splitContainerOutput.ResumeLayout(false);
            panel8.ResumeLayout(false);
            panel2.ResumeLayout(false);
            flowLayoutPanel2.ResumeLayout(false);
            panel6.ResumeLayout(false);
            IndgroupBox2.ResumeLayout(false);
            IndgroupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)Ind_BottomUpDown).EndInit();
            panelInd3.ResumeLayout(false);
            toolStripInd3.ResumeLayout(false);
            toolStripInd3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)Ind_RightUpDown).EndInit();
            panelInd2.ResumeLayout(false);
            toolStripInd2.ResumeLayout(false);
            toolStripInd2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)Ind_LeftUpDown).EndInit();
            panelInd5.ResumeLayout(false);
            toolStripInd5.ResumeLayout(false);
            toolStripInd5.PerformLayout();
            toolStripInd4.ResumeLayout(false);
            toolStripInd4.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)Ind_Reg1SizeUpDown).EndInit();
            IndgroupBox3.ResumeLayout(false);
            IndgroupBox3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)Ind_Reg1TopUpDown).EndInit();
            panelInd4.ResumeLayout(false);
            panel4.ResumeLayout(false);
            toolStrip1.ResumeLayout(false);
            toolStrip1.PerformLayout();
            IndgroupBox1.ResumeLayout(false);
            panelInd1.ResumeLayout(false);
            toolStripInd1.ResumeLayout(false);
            toolStripInd1.PerformLayout();
            CMenuImages.ResumeLayout(false);
            statusStripMain.ResumeLayout(false);
            statusStripMain.PerformLayout();
            toolStripMain.ResumeLayout(false);
            toolStripMain.PerformLayout();
            menuStripMain.ResumeLayout(false);
            menuStripMain.PerformLayout();
            panelBible2.ResumeLayout(false);
            toolStripBible2.ResumeLayout(false);
            toolStripBible2.PerformLayout();
            TabBibleVersions.ResumeLayout(false);
            tabImages.ResumeLayout(false);
            panelImagesTop.ResumeLayout(false);
            panelImage1.ResumeLayout(false);
            toolStripImage1.ResumeLayout(false);
            toolStripImage1.PerformLayout();
            panelPowerpoint1.ResumeLayout(false);
            panelExternalFiles1.ResumeLayout(false);
            toolStripPowerpoint1.ResumeLayout(false);
            toolStripPowerpoint1.PerformLayout();
            tabBibles.ResumeLayout(false);
            tabBibles.PerformLayout();
            CMenuBible.ResumeLayout(false);
            tabMedia.ResumeLayout(false);
            panel11.ResumeLayout(false);
            panelMedia1.ResumeLayout(false);
            toolStripMedia1.ResumeLayout(false);
            toolStripMedia1.PerformLayout();
            CMenuFiles.ResumeLayout(false);
            DefgroupBox2.ResumeLayout(false);
            panelDef4.ResumeLayout(false);
            toolStripDef4.ResumeLayout(false);
            toolStripDef4.PerformLayout();
            panelDef3.ResumeLayout(false);
            toolStripDef3.ResumeLayout(false);
            toolStripDef3.PerformLayout();
            toolStripInd6.ResumeLayout(false);
            toolStripInd6.PerformLayout();
            toolStripDefTemplates.ResumeLayout(false);
            toolStripDefTemplates.PerformLayout();
            panelDefTemplate.ResumeLayout(false);
            tabDefault.ResumeLayout(false);
            DefPanel.ResumeLayout(false);
            DefgroupBox3.ResumeLayout(false);
            DefgroupBox3.PerformLayout();
            panel21.ResumeLayout(false);
            toolStripDef7.ResumeLayout(false);
            toolStripDef7.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)Def_PanelHeight).EndInit();
            panelDef5.ResumeLayout(false);
            toolStripDef5.ResumeLayout(false);
            toolStripDef5.PerformLayout();
            panelDef6.ResumeLayout(false);
            toolStripDef6.ResumeLayout(false);
            toolStripDef6.PerformLayout();
            DefgroupBox1.ResumeLayout(false);
            panelDef2.ResumeLayout(false);
            toolStripDef2.ResumeLayout(false);
            toolStripDef2.PerformLayout();
            panelDef1.ResumeLayout(false);
            toolStripDef1.ResumeLayout(false);
            toolStripDef1.PerformLayout();
            panelInfoScreen2.ResumeLayout(false);
            InfoScreentoolstrip2.ResumeLayout(false);
            InfoScreentoolstrip2.PerformLayout();
            tabFiles.ResumeLayout(false);
            panelExternalFiles.ResumeLayout(false);
            panelInfoScreen1.ResumeLayout(false);
            InfoScreentoolstrip1.ResumeLayout(false);
            InfoScreentoolstrip1.PerformLayout();
            toolStripContainerMain.ContentPanel.ResumeLayout(false);
            toolStripContainerMain.TopToolStripPanel.ResumeLayout(false);
            toolStripContainerMain.TopToolStripPanel.PerformLayout();
            toolStripContainerMain.ResumeLayout(false);
            toolStripContainerMain.PerformLayout();
            splitContainerMain.Panel1.ResumeLayout(false);
            splitContainerMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainerMain).EndInit();
            splitContainerMain.ResumeLayout(false);
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            tabControlSource.ResumeLayout(false);
            tabFolders.ResumeLayout(false);
            panelFolders.ResumeLayout(false);
            toolStripFolders.ResumeLayout(false);
            toolStripFolders.PerformLayout();
            CMenuSongs.ResumeLayout(false);
            tabPowerpoint.ResumeLayout(false);
            panelPowerpoint2.ResumeLayout(false);
            toolStripPowerpoint2.ResumeLayout(false);
            toolStripPowerpoint2.PerformLayout();
            tabControlLists.ResumeLayout(false);
            tabWorshipList.ResumeLayout(false);
            panelWorshipList2.ResumeLayout(false);
            toolStripWorshipList2.ResumeLayout(false);
            toolStripWorshipList2.PerformLayout();
            panelWorshipList1.ResumeLayout(false);
            toolStripWorshipList1.ResumeLayout(false);
            toolStripWorshipList1.PerformLayout();
            CMenuWorship.ResumeLayout(false);
            tabPraiseBook.ResumeLayout(false);
            panelPraiseBook2.ResumeLayout(false);
            toolStripPraiseBook2.ResumeLayout(false);
            toolStripPraiseBook2.PerformLayout();
            panelPraiseBook1.ResumeLayout(false);
            toolStripPraiseBook1.ResumeLayout(false);
            toolStripPraiseBook1.PerformLayout();
            CMenuPraiseB.ResumeLayout(false);
            splitContainer2.Panel1.ResumeLayout(false);
            splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer2).EndInit();
            splitContainer2.ResumeLayout(false);
            splitContainerPreview.Panel1.ResumeLayout(false);
            splitContainerPreview.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainerPreview).EndInit();
            splitContainerPreview.ResumeLayout(false);
            panelPreviewTop.ResumeLayout(false);
            IndPanel.ResumeLayout(false);
            IndPanel.PerformLayout();
            panelIndTemplate.ResumeLayout(false);
            toolStripIndTemplates.ResumeLayout(false);
            toolStripIndTemplates.PerformLayout();
            IndgroupBox4.ResumeLayout(false);
            IndgroupBox4.PerformLayout();
            panelInd7.ResumeLayout(false);
            toolStripInd7.ResumeLayout(false);
            toolStripInd7.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)Ind_Reg2SizeUpDown).EndInit();
            ((System.ComponentModel.ISupportInitialize)Ind_Reg2TopUpDown).EndInit();
            panelInd6.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        private ToolStripMenuItem Ind_HeadFirstScreen;
        private Button PreviewBtnVerse1;
        private Button PreviewBtnVerse2;
        private Button PreviewBtnVerse3;
        private FlowLayoutPanel flowLayoutPanel1;
        private Button PreviewBtnVerse4;
        private Button PreviewBtnVerse5;
        private Button PreviewBtnVerse6;
        private Button PreviewBtnVerse7;
        private Button PreviewBtnVerse8;
        private Button PreviewBtnVerse9;
        private Button PreviewBtnVersePreChorus;
        private Button PreviewBtnVersePreChorus2;
        private Button PreviewBtnVerseChorus;
        private Button PreviewBtnVerseChorus2;
        private Button PreviewBtnVerseBridge;
        private Button PreviewBtnVerseBridge2;
        private Button PreviewBtnVerseEnding;
        private Panel panel7;
        private Panel panel1;
        private Panel panel3;
        private Button PreviewBtnSlideDown;
        private ToolTip toolTip1;
        private Button PreviewBtnSlideUp;
        private Button PreviewBtnItemDown;
        private Button PreviewBtnItemUp;
        private RadioButton IndradioButtonInfo;
        private RadioButton IndradioButtonFormat;
        private RadioButton IndradioButtonText;
        private CheckBox IndcbPreviewNotes;
        private Panel PreviewHolder;
        private Panel PreviewBack;
        private RichTextBox PreviewNotes;
        private Panel panelPreviewSessionNotes2;
        private Panel panelPreviewBottom;
        private ColumnHeader columnHeader15;
        private Panel panel9;
        private Button btnToLive;
        private Button btnToOutputMoveNext;
        private ListView PreviewPanelDisplayName;
        private ImageList imageListSys;
        private Button btnToOutput;
        private ToolStripMenuItem Ind_HeadAllTitles;
        private ToolStripDropDownButton Ind_Region;
        private ToolStripMenuItem Ind_ShowRegion1;
        private ToolStripMenuItem Ind_ShowRegion2;
        private ToolStripMenuItem Ind_ShowRegionBoth;
        private ToolStripDropDownButton Ind_VAlign;
        private ToolStripMenuItem Ind_VAlignTop;
        private ToolStripMenuItem Ind_VAlignCentre;
        private ToolStripMenuItem Ind_VAlignBottom;
        private Panel flowLayoutPreviewLyrics;
        private ToolStripButton Ind_Outline;
        private ToolStripButton Ind_Interlace;
        private ToolStripButton Ind_Notations;
        private CheckBox Ind_checkBox;
        private ToolStripButton Ind_Shadow;
        private RichTextBox PreviewInfo;
        private Panel panelOutputTop;
        private FlowLayoutPanel flowLayoutOutputPowerPoint;
        private Panel flowLayoutOutputLyrics;
        private TextBox OutputInfo;
        private Panel panel10;
        private CheckBox cbOutputBlack;
        private CheckBox cbOutputClear;
        private ListView OutputPanelDisplayName;
        private ColumnHeader columnHeader16;
        private CheckBox cbGoLive;
        private Panel panelOutputBottom;
        private Panel panelOutputLM1;
        private TextBox OutputTextBoxLM;
        private Panel panelOutputLM2;
        private Panel panelOutputLM3;
        private Button OutputBtnLMSend;
        private Button OutputBtnLMClear;
        private Panel OutputHolder;
        private Panel OutputBack;
        private ToolStripDropDownButton Ind_R2Italics;
        private ToolStripMenuItem Ind_R2Italics0;
        private ToolStripMenuItem Ind_R2Italics1;
        private ToolStripMenuItem Ind_R2Italics2;
        private SplitContainer splitContainerOutput;
        private Panel panel8;
        private Label labelGapItem;
        private Label labelHideText;
        private Label labelBlackScreen;
        private Panel panel2;
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
        private Button OutputBtnVerseBridge2;
        private Button OutputBtnVerseEnding;
        private Panel panel6;
        private Button OutputBtnSlideDown;
        private Button OutputBtnSlideUp;
        private Button OutputBtnItemDown;
        private Button OutputBtnItemUp;
        private Button OutputBtnRefAlert;
        private Button OutputBtnMedia;
        private Button OutputBtnJumpToNonRotate;
        private ToolStripMenuItem Ind_HeadNoTitles;
        private GroupBox IndgroupBox2;
        private NumericUpDown Ind_BottomUpDown;
        private Panel panelInd3;
        private ToolStrip toolStripInd3;
        private ToolStripComboBox Ind_TransItem;
        private ToolStripComboBox Ind_TransSlides;
        private NumericUpDown Ind_RightUpDown;
        private Panel panelInd2;
        private ToolStrip toolStripInd2;
        private ToolStripDropDownButton Ind_ImageMode;
        private ToolStripMenuItem Ind_ImageTile;
        private ToolStripMenuItem Ind_ImageCentre;
        private ToolStripMenuItem Ind_ImageBestFit;
        private ToolStripButton Ind_NoImage;
        private ToolStripButton Ind_BackColour;
        private ToolStripSeparator toolStripSeparator27;
        private ToolStripButton Ind_AssignMedia;
        private Label label3;
        private NumericUpDown Ind_LeftUpDown;
        private Label label2;
        private Label label1;
        private ToolStripButton Ind_R1Colour;
        private ToolStripMenuItem Ind_R1AlignRight;
        private ToolStripMenuItem Ind_R1AlignCentre;
        private ToolStripMenuItem Ind_R1AlignLeft;
        private ToolStripDropDownButton Ind_R1Align;
        private ToolStripSeparator toolStripSeparator13;
        private ToolStripButton Ind_R1Underline;
        private ToolStripMenuItem Ind_R1Italics2;
        private ToolStripMenuItem Ind_R1Italics1;
        private ToolStripMenuItem Ind_R1Italics0;
        private ToolStripDropDownButton Ind_R1Italics;
        private ToolStripButton Ind_R1Bold;
        private Panel panelInd5;
        private ToolStrip toolStripInd5;
        private ToolStripComboBox Ind_Reg1FontsList;
        private ToolStrip toolStripInd4;
        private NumericUpDown Ind_Reg1SizeUpDown;
        private Label labelBlackScreenOn;
        private Timer TimerToFront;
        private ToolStripButton Ind_R2Underline;
        private ToolStripSeparator toolStripSeparator15;
        private ToolStripDropDownButton Ind_R2Align;
        private ToolStripMenuItem Ind_R2AlignLeft;
        private ToolStripMenuItem Ind_R2AlignCentre;
        private ToolStripMenuItem Ind_R2AlignRight;
        private ToolStripButton Ind_R2Colour;
        private Label label7;
        private GroupBox IndgroupBox3;
        private NumericUpDown Ind_Reg1TopUpDown;
        private Panel panelInd4;
        private Label label4;
        private Panel panel4;
        private ToolStrip toolStrip1;
        private ToolStripDropDownButton Ind_HeadAlign;
        private ToolStripMenuItem Ind_HeadAlignAsR1;
        private ToolStripMenuItem Ind_HeadAlignAsR2;
        private ToolStripMenuItem Ind_HeadAlignLeft;
        private ToolStripMenuItem Ind_HeadAlignCentre;
        private ToolStripMenuItem Ind_HeadAlignRight;
        private ToolStripButton Ind_CapoDown;
        private ToolStripButton Ind_CapoUp;
        private ToolStripSeparator toolStripSeparator5;
        private ToolStripButton Ind_HideDisplayPanel;
        private GroupBox IndgroupBox1;
        private Panel panelInd1;
        private ToolStrip toolStripInd1;
        private ToolStripDropDownButton Ind_Head;
        private ToolStripMenuItem Menu_ImportFolder;
        private ToolStripMenuItem Menu_GoLiveWithPreview;
        private ToolStripMenuItem Menu_RefreshOutput;
        private ToolStripSeparator toolStripSeparator28;
        private ToolStripMenuItem Menu_BlackScreen;
        private ToolStripMenuItem Menu_ClearScreen;
        //private ToolStripMenuItem Menu_LiveCam;
        private ToolStripMenuItem Menu_StartShow;
        private ToolStripMenuItem Menu_RestartCurrentItem;
        private ToolStripMenuItem Menu_MainTools;
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
        private ToolStripMenuItem Menu_ClearAllFormatting;
        private ToolStripSeparator toolStripSeparator9;
        private ToolStripMenuItem Menu_ClearRegistrySettings;
        private ToolStripMenuItem Menu_EditSong;
        private ToolStripMenuItem Menu_MainOutput;
        private ToolStripSeparator toolStripSeparator19;
        private ToolStripMenuItem Menu_CopySong;
        private ToolStripMenuItem Menu_MoveSong;
        private ToolStripMenuItem Menu_DeleteSong;
        private ToolStripSeparator toolStripSeparator41;
        private ToolStripMenuItem Menu_SelectAll;
        private ToolStripMenuItem Menu_Find;
        private ToolStripMenuItem Menu_StatusBar;
        private ToolStripSeparator toolStripSeparator21;
        private ToolStripMenuItem Menu_ReArrangeSongFolders;
        private ToolStripMenuItem Menu_MainView;
        private ToolStripMenuItem Menu_EasiSlidesFolder;
        private ToolStripMenuItem Menu_Options;
        private ToolStripSeparator toolStripSeparator23;
        private ToolStripMenuItem Menu_Refresh;
        private ToolStripMenuItem Menu_PreviewNotations;
        private ToolStripMenuItem Menu_UseSongNumbering;
        private ToolStripStatusLabel StatusBarPanel2;
        private ToolStripStatusLabel StatusBarPanel3;
        private ToolStripStatusLabel StatusBarPanel4;
        private Button DefApplyDefaultsBtn;
        private Timer TimerFlasher;
        private OpenFileDialog openFileDialog1;
        private ToolStripStatusLabel StatusBarPanel1;
        private Timer TimerReMax;
        private ContextMenuStrip CMenuImages;
        private ToolStripMenuItem CMenuImages_AddItem;
        private ToolStripMenuItem CMenuImages_AddDefault;
        private ToolStripSeparator toolStripSeparator35;
        private ToolStripMenuItem CMenuImages_Refresh;
        private Timer TimerMessagingWindowOpen;
        private Timer TimerSearch;
        private SaveFileDialog saveFileDialog1;
        private StatusStrip statusStripMain;
        private ToolStripMenuItem Menu_About;
        private ToolStripMenuItem Menu_MainHelp;
        private ToolStripMenuItem Menu_Contents;
        private ToolStripMenuItem Menu_HelpWeb;
        private ToolStripSeparator toolStripSeparator31;
        private ToolStripMenuItem Menu_Register;
        private ToolStripMenuItem Menu_AddSong;
        private ToolStrip toolStripMain;
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
        private ToolStripButton Main_NoRotate;
        private ToolStripDropDownButton Main_RotateStyle;
        private ToolStripMenuItem Main_Rotate0;
        private ToolStripMenuItem Main_Rotate1;
        private ToolStripMenuItem Main_Rotate2;
        private ToolStripMenuItem Main_Rotate3;
        private ToolStripButton Main_Alerts;
        private ToolStripButton Main_Chinese;
        private ToolStripSeparator toolStripSeparator4;
        private ToolStripButton Main_Find;
        private ToolStripComboBox Main_QuickFind;
        private ToolStripButton Main_JumpA;
        private ToolStripButton Main_JumpB;
        private ToolStripButton Main_JumpC;
        private MenuStrip menuStripMain;
        private ToolStripMenuItem Menu_MainFile;
        private ToolStripMenuItem Menu_WorshipSessions;
        private ToolStripMenuItem Menu_PraiseBookTemplates;
        private ToolStripSeparator toolStripSeparator20;
        private ToolStripMenuItem Menu_ListingOfSelectedFolder;
        private ToolStripSeparator toolStripSeparator16;
        private ToolStripMenuItem Menu_EditHistoryList;
        private ToolStripSeparator toolStripSeparator18;
        private ToolStripMenuItem Menu_Exit;
        private ToolStripMenuItem Menu_MainEdit;
        private ToolStripButton Ind_R2Bold;
        private ToolStripButton Image_Import;
        private ToolStripMenuItem CMenuBible_CopyInfoScreen;
        private TextBox BibleUserLookup;
        private Panel panelBible2;
        private ToolStrip toolStripBible2;
        private ToolStripButton Bibles_Go;
        private ComboBox BookLookup;
        private ToolStripMenuItem CMenuBible_Copy;
        private TabControl TabBibleVersions;
        private TabPage tabPage1;
        private TabPage tabImages;
        private FlowLayoutPanel flowLayoutImages;
        private Panel panelImagesTop;
        private Panel panelImage1;
        private ToolStrip toolStripImage1;
        private ToolStripButton Image_OpenFolder;
        private ComboBox ImagesFolder;
        private Panel panelPowerpoint1;
        private ComboBox PowerpointFolder;
        private Panel panelExternalFiles1;
        private ToolStrip toolStripPowerpoint1;
        private ToolStripDropDownButton PP_ListType;
        private ToolStripMenuItem PP_ListStyle;
        private ToolStripMenuItem PP_PreviewStyle;
        private ToolStripButton PP_OpenFolder;
        private ToolStripButton PP_Import;
        private ToolStripSeparator toolStripSeparator24;
        private ColumnHeader columnHeader36;
        private ToolStripMenuItem CMenuBible_AddRegion2;
        private TabPage tabBibles;
        private RichTextBox BibleText;
        private ContextMenuStrip CMenuBible;
        private ToolStripMenuItem CMenuBible_SelectAll;
        private ToolStripMenuItem CMenuBible_UnselectAll;
        private ToolStripMenuItem CMenuBible_AddShow;
        private ToolStripSeparator toolStripSeparator17;
        private TabPage tabMedia;
        private Panel panel11;
        private Panel panelMedia1;
        private ToolStrip toolStripMedia1;
        private ToolStripButton Media_OpenFolder;
        private ToolStripButton Media_Import;
        private ComboBox MediaFolder;
        private ListView MediaList;
        private ColumnHeader columnHeader37;
        private ColumnHeader columnHeader38;
        private ColumnHeader columnHeader39;
        private ColumnHeader columnHeader40;
        private ColumnHeader columnHeader41;
        private ColumnHeader columnHeader42;
        private ColumnHeader columnHeader43;
        private ContextMenuStrip CMenuFiles;
        private ToolStripMenuItem CMenuFiles_SelectAll;
        private ToolStripMenuItem CMenuFiles_UnselectAll;
        private ToolStripMenuItem CMenuFiles_AddShow;
        private ToolStripSeparator toolStripSeparator12;
        private ToolStripMenuItem CMenuFiles_Edit;
        private ToolStripMenuItem CMenuFiles_Copy;
        private ToolStripSeparator toolStripSeparator25;
        private ToolStripMenuItem CMenuFiles_Refresh;
        private ToolStripButton Def_SaveTemplate;
        private GroupBox DefgroupBox2;
        private Panel panelDef4;
        private ToolStrip toolStripDef4;
        private ToolStripComboBox Def_TransItem;
        private ToolStripComboBox Def_TransSlides;
        private Panel panelDef3;
        private ToolStrip toolStripDef3;
        private ToolStripDropDownButton Def_ImageMode;
        private ToolStripMenuItem Def_ImageTile;
        private ToolStripMenuItem Def_ImageCentre;
        private ToolStripMenuItem Def_ImageBestFit;
        private ToolStripButton Def_NoImage;
        private ToolStripButton Def_BackColour;
        private ToolStripButton Def_AssignMedia;
        private ToolStripButton Def_LoadTemplate;
        private ToolStrip toolStripInd6;
        private ToolStrip toolStripDefTemplates;
        private Panel panelDefTemplate;
        private TabPage tabDefault;
        private Panel DefPanel;
        private GroupBox DefgroupBox3;
        private Panel panel21;
        private ToolStrip toolStripDef7;
        private ToolStripButton Def_PanelFontBold;
        private ToolStripButton Def_PanelFontItalics;
        private ToolStripButton Def_PanelFontUnderline;
        private ToolStripButton Def_PanelFontShadow;
        private ToolStripButton Def_PanelFontOutline;
        private ToolStripComboBox Def_PanelFontList;
        private NumericUpDown Def_PanelHeight;
        private Panel panelDef5;
        private ToolStrip toolStripDef5;
        private ToolStripButton Def_PanelAsR1;
        private ToolStripButton Def_PanelTextColour;
        private ToolStripSeparator toolStripSeparator14;
        private ToolStripButton Def_PanelTransparent;
        private ToolStripButton Def_PanelBackColour;
        private Panel panelDef6;
        private ToolStrip toolStripDef6;
        private ToolStripButton Def_PanelShow;
        private ToolStripButton Def_PanelTitle;
        private ToolStripButton Def_PanelCopyright;
        private ToolStripButton Def_PanelSong;
        private ToolStripButton Def_PanelSlides;
        private ToolStripButton Def_PanelPrevNext;
        private Label label5;
        private GroupBox DefgroupBox1;
        private Panel panelDef2;
        private ToolStrip toolStripDef2;
        private ToolStripDropDownButton Def_HeadAlign;
        private ToolStripMenuItem Def_HeadAlignAsR1;
        private ToolStripMenuItem Def_HeadAlignAsR2;
        private ToolStripMenuItem Def_HeadAlignLeft;
        private ToolStripMenuItem Def_HeadAlignCentre;
        private ToolStripMenuItem Def_HeadAlignRight;
        private ToolStripSeparator toolStripSeparator26;
        private ToolStripDropDownButton Def_R1Align;
        private ToolStripMenuItem Def_R1AlignLeft;
        private ToolStripMenuItem Def_R1AlignCentre;
        private ToolStripMenuItem Def_R1AlignRight;
        private ToolStripButton Def_R1Colour;
        private ToolStripSeparator toolStripSeparator8;
        private ToolStripDropDownButton Def_R2Align;
        private ToolStripMenuItem Def_R2AlignLeft;
        private ToolStripMenuItem Def_R2AlignCentre;
        private ToolStripMenuItem Def_R2AlignRight;
        private ToolStripButton Def_R2Colour;
        private Panel panelDef1;
        private ToolStrip toolStripDef1;
        private ToolStripDropDownButton Def_Head;
        private ToolStripMenuItem Def_HeadNoTitles;
        private ToolStripMenuItem Def_HeadAllTitles;
        private ToolStripMenuItem Def_HeadFirstScreen;
        private ToolStripDropDownButton Def_Region;
        private ToolStripMenuItem Def_ShowRegion1;
        private ToolStripMenuItem Def_ShowRegion2;
        private ToolStripMenuItem Def_ShowRegionBoth;
        private ToolStripDropDownButton Def_VAlign;
        private ToolStripMenuItem Def_VAlignTop;
        private ToolStripMenuItem Def_VAlignCentre;
        private ToolStripMenuItem Def_VAlignBottom;
        private ToolStripButton Def_Shadow;
        private ToolStripButton Def_Outline;
        private ToolStripButton Def_Interlace;
        private ToolStripButton Def_Notations;
        private ToolStripButton Def_ToZero;
        private ColumnHeader columnHeader35;
        private ToolStripButton InfoScreen_Delete;
        private ToolStripMenuItem CMenuSongs_AddShow;
        private ToolStripSeparator toolStripSeparator38;
        private ToolStripMenuItem CMenuSongs_Edit;
        private ToolStripMenuItem CMenuSongs_Copy;
        private ToolStripSeparator toolStripSeparator10;
        private ToolStripMenuItem CMenuSongs_Refresh;
        private ToolStripMenuItem CMenuSongs_UnselectAll;
        private ComboBox SongFolder;
        private Panel panelInfoScreen2;
        private ToolStrip InfoScreentoolstrip2;
        private ToolStripButton InfoScreen_New;
        private ToolStripButton InfoScreen_Edit;
        private ToolStripButton InfoScreen_Copy;
        private ToolStripButton InfoScreen_Move;
        private TabPage tabFiles;
        private Panel panelExternalFiles;
        private Panel panelInfoScreen1;
        private ToolStrip InfoScreentoolstrip1;
        private ToolStripButton InfoScreen_OpenFolder;
        private ToolStripButton InfoScreen_Import;
        private ComboBox InfoScreenFolder;
        private ListView InfoScreenList;
        private ColumnHeader columnHeader23;
        private ColumnHeader columnHeader24;
        private ColumnHeader columnHeader25;
        private ColumnHeader columnHeader26;
        private ColumnHeader columnHeader27;
        private ColumnHeader columnHeader28;
        private ColumnHeader columnHeader29;
        private ToolStripMenuItem CMenuSongs_SelectAll;
        private ToolStripContainer toolStripContainerMain;
        private SplitContainer splitContainerMain;
        private SplitContainer splitContainer1;
        private TabControl tabControlSource;
        private TabPage tabFolders;
        private Panel panelFolders;
        private ToolStrip toolStripFolders;
        private ToolStripButton Folders_WordCount;
        private ListView SongsList;
        private ColumnHeader columnHeader1;
        private ColumnHeader columnHeader2;
        private ColumnHeader columnHeader3;
        private ColumnHeader columnHeader4;
        private ColumnHeader columnHeader5;
        private ColumnHeader columnHeader6;
        private ColumnHeader columnHeader7;
        private ContextMenuStrip CMenuSongs;
        private TabPage tabPowerpoint;
        private FlowLayoutPanel flowLayoutExternalPowerPoint;
        private Panel panelPowerpoint2;
        private ToolStrip toolStripPowerpoint2;
        private ToolStripButton Powerpoint_Edit;
        private ToolStripButton Powerpoint_Copy;
        private ToolStripButton Powerpoint_Move;
        private ToolStripButton Powerpoint_Delete;
        private ListView PowerpointList;
        private ColumnHeader columnHeader30;
        private ColumnHeader columnHeader31;
        private ColumnHeader columnHeader32;
        private ColumnHeader columnHeader33;
        private ColumnHeader columnHeader34;
        private TabControl tabControlLists;
        private TabPage tabWorshipList;
        private Panel panelWorshipList2;
        private ToolStrip toolStripWorshipList2;
        private ToolStripButton WL_Up;
        private ToolStripButton WL_Down;
        private ToolStripButton WL_Delete;
        private ToolStripSeparator toolStripSeparator6;
        private ToolStripButton WL_Word;
        private ToolStripButton WL_Notes;
        private Panel panelWorshipList1;
        private ToolStrip toolStripWorshipList1;
        private ToolStripButton WL_Manage;
        private ToolStripButton WL_Add;
        private ToolStripButton WL_Open;
        private ListView WorshipListItems;
        private ColumnHeader columnHeader8;
        private ColumnHeader columnHeader9;
        private ColumnHeader columnHeader10;
        private ColumnHeader columnHeader11;
        private ColumnHeader columnHeader12;
        private ColumnHeader columnHeader13;
        private ColumnHeader columnHeader14;
        private ContextMenuStrip CMenuWorship;
        private ToolStripMenuItem CMenuWorship_SelectAll;
        private ToolStripMenuItem CMenuWorship_UnselectAll;
        private ToolStripMenuItem CMenuWorship_Clear;
        private ToolStripSeparator toolStripSeparator39;
        private ToolStripMenuItem CMenuWorship_Edit;
        private ToolStripMenuItem CMenuWorship_Play;
        private ToolStripMenuItem CMenuWorship_PlayOnOutput;
        private ToolStripSeparator toolStripSeparator37;
        private ToolStripMenuItem CMenuWorship_AddUsages;
        private ComboBox SessionList;
        private TabPage tabPraiseBook;
        private Panel panelPraiseBook2;
        private ToolStrip toolStripPraiseBook2;
        private ToolStripSeparator toolStripSeparator22;
        private ToolStripButton PB_Delete;
        private ToolStripSeparator toolStripSeparator7;
        private ToolStripButton PB_Word;
        private ToolStripButton PB_Html;
        private Panel panelPraiseBook1;
        private ToolStrip toolStripPraiseBook1;
        private ToolStripButton PB_Manage;
        private ToolStripButton PB_Add;
        private ToolStripButton PB_WordCount;
        private ListView PraiseBookItems;
        private ColumnHeader columnHeader17;
        private ColumnHeader columnHeader18;
        private ColumnHeader columnHeader19;
        private ColumnHeader columnHeader20;
        private ColumnHeader columnHeader21;
        private ColumnHeader columnHeader22;
        private ContextMenuStrip CMenuPraiseB;
        private ToolStripMenuItem CMenuPraiseB_SelectAll;
        private ToolStripMenuItem CMenuPraiseB_UnselectAll;
        private ToolStripMenuItem CMenuPraiseB_Clear;
        private ToolStripSeparator toolStripSeparator36;
        private ToolStripMenuItem CMenuPraiseB_Edit;
        private ComboBox PraiseBook;
        private SplitContainer splitContainer2;
        private SplitContainer splitContainerPreview;
        private Panel panelPreviewTop;
        private FlowLayoutPanel flowLayoutPreviewPowerPoint;
        private Panel IndPanel;
        private Panel panelIndTemplate;
        private ToolStrip toolStripIndTemplates;
        private ToolStripButton Ind_LoadTemplate;
        private ToolStripButton Ind_SaveTemplate;
        private GroupBox IndgroupBox4;
        private Panel panelInd7;
        private ToolStrip toolStripInd7;
        private ToolStripComboBox Ind_Reg2FontsList;
        private NumericUpDown Ind_Reg2SizeUpDown;
        private Label label6;
        private NumericUpDown Ind_Reg2TopUpDown;
        private Panel panelInd6;

        #endregion
    }
}
