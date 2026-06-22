//using NetOffice.DAOApi;
using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Data;
using Easislides.Util;
//using System.Data.SQLite;
using Easislides.SQLite;
using Easislides.Module;
using System.Collections.Generic;

#if SQLite
using DbConnection = System.Data.SQLite.SQLiteConnection;
using DbDataAdapter = System.Data.SQLite.SQLiteDataAdapter;
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
    public partial class FrmOptions : Form
    {
        private const int SampleSplitterTop = 6;

        private const int SampleSplitterBottom = 117;

        private int CurFolder;

        private bool InitFormLoad = true;

        private float SampleSplitterVerticalIncrement = 0.96f;

        private float SampleSplitterHorizontalIncrement = 1.4f;

        private string[,] tempFolderLyricsHeading = new string[Gf.MAXSONGSFOLDERS, 4];

        private string[] tempFolderName = new string[Gf.MAXSONGSFOLDERS];

        private int[] tempFolderGroupStyle = new int[Gf.MAXSONGSFOLDERS];

        private bool[] tempFolderUse = new bool[Gf.MAXSONGSFOLDERS];

        private int[,] tempShowFontVPosition = new int[Gf.MAXSONGSFOLDERS, 2];

        private int[,] tempShowFontSize = new int[Gf.MAXSONGSFOLDERS, 2];

        private int[,] tempShowFontVPositionMax = new int[Gf.MAXSONGSFOLDERS, 2];

        private int[,] tempShowFontVPositionMin = new int[Gf.MAXSONGSFOLDERS, 2];

        private int[] tempLeftMargin = new int[Gf.MAXSONGSFOLDERS];

        private int[] tempRightMargin = new int[Gf.MAXSONGSFOLDERS];

        private int[] tempBottomMargin = new int[Gf.MAXSONGSFOLDERS];

        private string[,] tempShowFontName = new string[Gf.MAXSONGSFOLDERS, 2];

        private bool[,] tempShowFontBold = new bool[Gf.MAXSONGSFOLDERS, 4];

        private bool[,] tempShowFontItalic = new bool[Gf.MAXSONGSFOLDERS, 4];

        private bool[,] tempShowFontUnderline = new bool[Gf.MAXSONGSFOLDERS, 4];

        private bool[,] tempShowFontRTL = new bool[Gf.MAXSONGSFOLDERS, 2];

        public int[] tempFolderHeadingPercentSize = new int[Gf.MAXSONGSFOLDERS];

        public int[] tempFolderHeadingOption = new int[Gf.MAXSONGSFOLDERS];

        public bool[,] tempFolderHeadingFontBold = new bool[Gf.MAXSONGSFOLDERS, 2];

        public bool[,] tempFolderHeadingFontItalic = new bool[Gf.MAXSONGSFOLDERS, 2];

        public bool[,] tempFolderHeadingFontUnderline = new bool[Gf.MAXSONGSFOLDERS, 2];

        public double[,] tempShowLineSpacing = new double[Gf.MAXSONGSFOLDERS, 2];

        private bool LoadTempPos = false;

        private int tempHB_TotalVersions = 0;

		public FrmOptions()
        {
            InitializeComponent();
        }

        private void FrmOptions_Load(object sender, EventArgs e)
        {
            for (int i = 1; i < Gf.MAXSONGSFOLDERS; i++)
            {
                tempFolderName[i] = Gf.FolderName[i];
                tempFolderGroupStyle[i] = (int)Gf.FolderGroupStyle[i];
                tempFolderUse[i] = ((Gf.FolderUse[i] > 0) ? true : false);
                tempLeftMargin[i] = Gf.ShowLeftMargin[i];
                tempRightMargin[i] = Gf.ShowRightMargin[i];
                tempBottomMargin[i] = Gf.ShowBottomMargin[i];
                tempFolderHeadingPercentSize[i] = Gf.FolderHeadingPercentSize[i];
                tempFolderHeadingOption[i] = Gf.FolderHeadingOption[i];
                tempFolderHeadingFontBold[i, 0] = ((Gf.FolderHeadingFontBold[i, 0] > 0) ? true : false);
                tempFolderHeadingFontItalic[i, 0] = ((Gf.FolderHeadingFontItalic[i, 0] > 0) ? true : false);
                tempFolderHeadingFontUnderline[i, 0] = ((Gf.FolderHeadingFontUnderline[i, 0] > 0) ? true : false);
                tempFolderHeadingFontBold[i, 1] = ((Gf.FolderHeadingFontBold[i, 1] > 0) ? true : false);
                tempFolderHeadingFontItalic[i, 1] = ((Gf.FolderHeadingFontItalic[i, 1] > 0) ? true : false);
                tempFolderHeadingFontUnderline[i, 1] = ((Gf.FolderHeadingFontUnderline[i, 1] > 0) ? true : false);
                tempShowLineSpacing[i, 0] = Gf.ShowLineSpacing[i, 0];
                tempShowLineSpacing[i, 1] = Gf.ShowLineSpacing[i, 1];
                for (int j = 0; j < 4; j++)
                {
                    tempFolderLyricsHeading[i, j] = Gf.FolderLyricsHeading[i, j];
                }
                for (int j = 0; j <= 1; j++)
                {
                    tempShowFontSize[i, j] = Gf.ShowFontSize[i, j];
                    tempShowFontBold[i, j] = ((Gf.ShowFontBold[i, j] > 0) ? true : false);
                    tempShowFontItalic[i, j] = ((Gf.ShowFontItalic[i, j] > 0) ? true : false);
                    tempShowFontUnderline[i, j] = ((Gf.ShowFontUnderline[i, j] > 0) ? true : false);
                    tempShowFontRTL[i, j] = ((Gf.ShowFontRTL[i, j] > 0) ? true : false);
                    tempShowFontBold[i, j + 2] = ((Gf.ShowFontBold[i, j + 2] > 0) ? true : false);
                    tempShowFontItalic[i, j + 2] = ((Gf.ShowFontItalic[i, j + 2] > 0) ? true : false);
                    tempShowFontUnderline[i, j + 2] = ((Gf.ShowFontUnderline[i, j + 2] > 0) ? true : false);
                    tempShowFontName[i, j] = Gf.ShowFontName[i, j];
                    tempShowFontVPosition[i, j] = Gf.ShowFontVPosition[i, j];
                }
                tempShowFontVPositionMax[i, 0] = Gf.ShowFontVPosition[i, 1];
                tempShowFontVPositionMin[i, 1] = Gf.ShowFontVPosition[i, 0];
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
            Gf.BuildFontsList(ref ComboFontName0);
            Gf.BuildFontsList(ref ComboFontName1);
            Gf.BuildFontsList(ref MessageComboFont);
            Gf.BuildFontsList(ref ParentalComboFont);
            Gf.BuildFontsList(ref ReferenceComboFont);
            CurFolder = Gf.CurMainSelectedFolder;
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
            if (Gf.Options_SelectedTabIndex <= tabControl1.TabCount)
            {
                tabControl1.SelectedIndex = Gf.Options_SelectedTabIndex;
            }
            if (Gf.ShowRunning)
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
            if (Gf.Options_BibleListChanged)
            {
                SaveBibleChanges();
            }
            //SaveLicenceChanges();
            Gf.Options_SelectedTabIndex = tabControl1.SelectedIndex;
            base.DialogResult = DialogResult.OK;
            Close();
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            Gf.Options_SelectedTabIndex = tabControl1.SelectedIndex;
            base.DialogResult = DialogResult.Cancel;
            Close();
        }

        private void SaveVariables()
        {
            for (int i = 1; i < Gf.MAXSONGSFOLDERS; i++)
            {
                if ((Gf.FolderName[i] != tempFolderName[i]) | (Gf.FolderGroupStyle[i] != (SortBy)tempFolderGroupStyle[i]) | (Gf.FolderUse[i] != (tempFolderUse[i] ? 1 : 0)))
                {
                    Gf.Options_FolderListChanged = true;
                }
                Gf.FolderName[i] = tempFolderName[i];
                Gf.FolderGroupStyle[i] = (SortBy)tempFolderGroupStyle[i];
                Gf.FolderUse[i] = (tempFolderUse[i] ? 1 : 0);
                if (Gf.ShowLeftMargin[i] != tempLeftMargin[i] || Gf.ShowRightMargin[i] != tempRightMargin[i] || Gf.ShowBottomMargin[i] != tempBottomMargin[i] || Gf.ShowLineSpacing[i, 0] != tempShowLineSpacing[i, 0] || Gf.ShowLineSpacing[i, 1] != tempShowLineSpacing[i, 1] || Gf.FolderHeadingPercentSize[i] != tempFolderHeadingPercentSize[i] || Gf.FolderHeadingOption[i] != tempFolderHeadingOption[i] || Gf.FolderHeadingFontBold[i, 0] != (tempFolderHeadingFontBold[i, 0] ? 1 : 0) || Gf.FolderHeadingFontItalic[i, 0] != (tempFolderHeadingFontItalic[i, 0] ? 1 : 0) || Gf.FolderHeadingFontUnderline[i, 0] != (tempFolderHeadingFontUnderline[i, 0] ? 1 : 0) || Gf.FolderHeadingFontBold[i, 1] != (tempFolderHeadingFontBold[i, 1] ? 1 : 0) || Gf.FolderHeadingFontItalic[i, 1] != (tempFolderHeadingFontItalic[i, 1] ? 1 : 0) || Gf.FolderHeadingFontUnderline[i, 1] != (tempFolderHeadingFontUnderline[i, 1] ? 1 : 0))
                {
                    Gf.ShowLeftMargin[i] = tempLeftMargin[i];
                    Gf.ShowRightMargin[i] = tempRightMargin[i];
                    Gf.ShowBottomMargin[i] = tempBottomMargin[i];
                    Gf.ShowLineSpacing[i, 0] = tempShowLineSpacing[i, 0];
                    Gf.ShowLineSpacing[i, 1] = tempShowLineSpacing[i, 1];
                    Gf.FolderHeadingPercentSize[i] = tempFolderHeadingPercentSize[i];
                    Gf.FolderHeadingOption[i] = tempFolderHeadingOption[i];
                    Gf.FolderHeadingFontBold[i, 0] = (tempFolderHeadingFontBold[i, 0] ? 1 : 0);
                    Gf.FolderHeadingFontItalic[i, 0] = (tempFolderHeadingFontItalic[i, 0] ? 1 : 0);
                    Gf.FolderHeadingFontUnderline[i, 0] = (tempFolderHeadingFontUnderline[i, 0] ? 1 : 0);
                    Gf.FolderHeadingFontBold[i, 1] = (tempFolderHeadingFontBold[i, 1] ? 1 : 0);
                    Gf.FolderHeadingFontItalic[i, 1] = (tempFolderHeadingFontItalic[i, 1] ? 1 : 0);
                    Gf.FolderHeadingFontUnderline[i, 1] = (tempFolderHeadingFontUnderline[i, 1] ? 1 : 0);
                    Gf.Options_FolderFormatChanged = true;
                }
                for (int j = 0; j < 4; j++)
                {
                    if (Gf.FolderLyricsHeading[i, j] != tempFolderLyricsHeading[i, j])
                    {
                        Gf.Options_FolderFormatChanged = true;
                    }
                    Gf.FolderLyricsHeading[i, j] = tempFolderLyricsHeading[i, j];
                }
                for (int j = 0; j <= 1; j++)
                {
                    if (Gf.ShowFontSize[i, j] != tempShowFontSize[i, j] || Gf.ShowFontBold[i, j] != (tempShowFontBold[i, j] ? 1 : 0) || Gf.ShowFontItalic[i, j] != (tempShowFontItalic[i, j] ? 1 : 0) || Gf.ShowFontUnderline[i, j] != (tempShowFontUnderline[i, j] ? 1 : 0) || Gf.ShowFontRTL[i, j] != (tempShowFontRTL[i, j] ? 1 : 0) || Gf.ShowFontBold[i, j + 2] != (tempShowFontBold[i, j + 2] ? 1 : 0) || Gf.ShowFontItalic[i, j + 2] != (tempShowFontItalic[i, j + 2] ? 1 : 0) || Gf.ShowFontUnderline[i, j + 2] != (tempShowFontUnderline[i, j + 2] ? 1 : 0) || Gf.ShowFontName[i, j] != tempShowFontName[i, j] || Gf.ShowFontVPosition[i, j] != tempShowFontVPosition[i, j])
                    {
                        Gf.Options_FolderFormatChanged = true;
                    }
                    Gf.ShowFontSize[i, j] = tempShowFontSize[i, j];
                    Gf.ShowFontBold[i, j] = (tempShowFontBold[i, j] ? 1 : 0);
                    Gf.ShowFontItalic[i, j] = (tempShowFontItalic[i, j] ? 1 : 0);
                    Gf.ShowFontUnderline[i, j] = (tempShowFontUnderline[i, j] ? 1 : 0);
                    Gf.ShowFontRTL[i, j] = (tempShowFontRTL[i, j] ? 1 : 0);
                    Gf.ShowFontBold[i, j + 2] = (tempShowFontBold[i, j + 2] ? 1 : 0);
                    Gf.ShowFontItalic[i, j + 2] = (tempShowFontItalic[i, j + 2] ? 1 : 0);
                    Gf.ShowFontUnderline[i, j + 2] = (tempShowFontUnderline[i, j + 2] ? 1 : 0);
                    Gf.ShowFontName[i, j] = tempShowFontName[i, j];
                    Gf.ShowFontVPosition[i, j] = tempShowFontVPosition[i, j];
                }
            }
            Gf.ComputeShowLineSpacing();
            Gf.UsePowerpointTab = checkBoxPPTab.Checked;
            Gf.NoPowerpointPanelOverlay = checkBoxPPNoPanel.Checked;
            Gf.UseMediaTab = checkBoxMediaTab.Checked;
            Gf.NoMediaPanelOverlay = checkBoxMediaNoPanel.Checked;
            Gf.ShowLyricsMonitorAlertBox = checkBoxLMBox.Checked;
            if (Gf.UseLargestFontSize != cbUseLargestFont.Checked || Gf.AutoTextOverflow != cbAutoTextOverflow.Checked || Gf.LineBetweenRegions != cbLineBetweenRegions.Checked || Gf.WordWrapLeftAlignIndent != cbWordWrapLeftAlignIndent.Checked)
            {
                Gf.UseLargestFontSize = cbUseLargestFont.Checked;
                Gf.AutoTextOverflow = cbAutoTextOverflow.Checked;
                Gf.LineBetweenRegions = cbLineBetweenRegions.Checked;
                Gf.WordWrapLeftAlignIndent = cbWordWrapLeftAlignIndent.Checked;
                Gf.Options_FolderFormatChanged = true;
            }
            Gf.AdvanceNextItem = cbAdvanceNextItem.Checked;
            if (rbGapItemOption1.Checked)
            {
                Gf.GapItemOption = GapType.Black;
            }
            else if (rbGapItemOption2.Checked)
            {
                Gf.GapItemOption = GapType.Default;
            }
            else if (rbGapItemOption3.Checked)
            {
                Gf.GapItemOption = GapType.User;
            }
            else
            {
                Gf.GapItemOption = GapType.None;
            }
            Gf.AltGapItemOption = GapType.None;
            Gf.GapItemLogoFile = DataUtil.Trim(tbGapLogoLocation.Text);
            Gf.GapItemUseFade = cbGapItemUseFade.Checked;
            Gf.NotationFontFactor = (float)NotationFontFactorUpDown.Value / 100f;
            Gf.HB_MaxVersesSelection = (int)VersesMaxUpDown.Value;
            Gf.HB_MaxAdhocVersesSelection = (int)AdhocVersesMaxUpDown.Value;
            Gf.PP_MaxFiles = (int)PPMaxUpDown.Value;
            if (EditHistoryMaxUpDown.Value != (decimal)Gf.MaxUserEditHistory)
            {
                Gf.Options_MaxHistoryListChanged = true;
            }
            Gf.MaxUserEditHistory = (int)EditHistoryMaxUpDown.Value;
            if (Gf.MaxUserEditHistory > Gf.AbsoluteMaxHitoryItems)
            {
                Gf.MaxUserEditHistory = Gf.AbsoluteMaxHitoryItems;
            }
            if (Gf.PreviewArea_ShowNotations != cbPreviewShowNotations.Checked || Gf.PreviewArea_FontSize != (int)PreviewFontUpDown.Value || Gf.PreviewArea_LineBetweenScreens != cbLineBetweenScreens.Checked)
            {
                Gf.Options_PreviewAreaChanged = true;
            }
            Gf.PreviewArea_ShowNotations = cbPreviewShowNotations.Checked;
            Gf.PreviewArea_LineBetweenScreens = cbLineBetweenScreens.Checked;
            Gf.PreviewArea_FontSize = (int)PreviewFontUpDown.Value;
            //Gf.OutputMonitorNumber = DualMonitorList.SelectedIndex;
            if (DualMonitorList.SelectedItem != null)
            {
                Gf.OutputMonitorName = DualMonitorList.SelectedItem.ToString();
            }

            Gf.DMAlwaysUseSecondaryMonitor = DM_AlwaysUseSecondaryMonitor.Checked;

            //if (Gf.DMAlwaysUseSecondaryMonitor & (Gf.OutputMonitorNumber == 0) & (DualMonitorList.Items.Count > 0))
            //{
            //    Gf.OutputMonitorNumber = 1;
            //    Gf.GetScreenNumber(ref Gf.OutputMonitorNumber, -1);
            //}

            // UseSecondaryMonitor 모드 인 경우
            if (Gf.DMAlwaysUseSecondaryMonitor & (DualMonitorList.Items.Count > 0))
            {
                //Gf.OutputMonitorNumber = Gf.GetSecondryMonitorIndex();

                Gf.OutputMonitorName = DisplayInfo.getSecondryDisplayName();
            }

            // daniel 추가 2024년 04월 07일
            // 다중 모니터일 경우 파워포인트 슬라이드 쇼의 디스플레이를 설정

            new OfficeVersion().SetPowerPointDisplayMonitor(Gf.OutputMonitorName);

            if (Gf.DualMonitorSelectAutoOption != ((!optDM0.Checked) ? 1 : 0) || (int)DM1UpDownTop.Value != Gf.DMOption1Top || (int)DM1UpDownLeft.Value != Gf.DMOption1Left || (int)DM1UpDownWidth.Value != Gf.DMOption1Width || DM_CustomAsSingleMonitor.Checked != Gf.DMOption1AsSingleMonitor)
            {
                Gf.Options_DMChanged = true;
            }

            // daniel
            // 스크린 비율 설정 추가
            Gf.isScreenWideMode = optWide.Checked;

            Gf.DualMonitorSelectAutoOption = ((!optDM0.Checked) ? 1 : 0);

            Gf.DMOption1Top = (int)DM1UpDownTop.Value;
            Gf.DMOption1Left = (int)DM1UpDownLeft.Value;
            Gf.DMOption1Width = (int)DM1UpDownWidth.Value;
            //Gf.LMOption1Height = (int)LM1UpDownWidth.Value * 3 / 4;
            //if (Gf.LMOption1Height < 1)
            //{
            //	Gf.LMOption1Height = 1;
            //}
            //wide mode가 아닐 경우

            if (!Gf.isScreenWideMode)
                Gf.DMOption1Height = (int)DM1UpDownWidth.Value * 3 / 4;
            else
                Gf.DMOption1Height = (int)DM1UpDownWidth.Value;

            Gf.DMOption1AsSingleMonitor = DM_CustomAsSingleMonitor.Checked;

            //Gf.LyricsMonitorNumber = LyricsMonitorList.SelectedIndex;

            if (LyricsMonitorList?.SelectedItem?.ToString() != null)
                Gf.LyricsMonitorName = LyricsMonitorList.SelectedItem.ToString();

            Gf.LMAlwaysUseSecondaryMonitor = LM_AlwaysUse.Checked;
            //if (Gf.LMAlwaysUseSecondaryMonitor & (Gf.LyricsMonitorNumber == 0) & (DualMonitorList.Items.Count > 0))
            //{
            //    Gf.LyricsMonitorNumber = 1;
            //    Gf.GetScreenNumber(ref Gf.LyricsMonitorNumber, (Gf.DualMonitorSelectAutoOption == 0) ? Gf.OutputMonitorNumber : (-1));
            //}

            if (Gf.LMAlwaysUseSecondaryMonitor & (DualMonitorList.Items.Count > 0))
            {
                DisplayInfo.GetDisplayName(ref Gf.LyricsMonitorName, (Gf.DualMonitorSelectAutoOption == 0) ? Gf.OutputMonitorName : "None");
            }

            if (Gf.LMSelectAutoOption != ((!optLM0.Checked) ? 1 : 0) || (int)LM1UpDownTop.Value != Gf.LMOption1Top || (int)LM1UpDownLeft.Value != Gf.LMOption1Left || (int)LM1UpDownWidth.Value != Gf.LMOption1Width)
            {
                Gf.Options_DMChanged = true;
            }
            Gf.LMSelectAutoOption = ((!optLM0.Checked) ? 1 : 0);
            Gf.LMOption1Top = (int)LM1UpDownTop.Value;
            Gf.LMOption1Left = (int)LM1UpDownLeft.Value;
            Gf.LMOption1Width = (int)LM1UpDownWidth.Value;
            //Gf.LMOption1Height = (int)LM1UpDownWidth.Value * 3 / 4;
            //if (Gf.LMOption1Height < 1)
            //{
            //	Gf.LMOption1Height = 1;
            //}
            // Daniel Park 수정 2023년 12월 24일
            if (!Gf.isScreenWideMode)
                Gf.DMOption1Height = (int)DM1UpDownWidth.Value * 3 / 4;
            else
            {
                Gf.DMOption1Height = (int)DM1UpDownHeight.Value * 5 / 3;
            }
            //Gf.DMOption1Height = (int)DM1UpDownHeight.Value;

            Gf.LMTextColour = btnLMTextColour.ForeColor;
            Gf.LMHighlightColour = btnLMHighlightColour.ForeColor;
            Gf.LMBackColour = btnLMBackColour.ForeColor;
            Gf.LMShowNotations = cbLMShowNotations.Checked;
            Gf.LMMainFontSize = (int)LMUpDownFontSize.Value;
            Gf.LMNotationsFontSize = (int)LMNotationsUpDownFontSize.Value;
            Gf.LMFontBold = LM_Bold.Checked;
            Gf.LMFontItalic = LM_Italic.Checked;
            Gf.LMFontUnderline = LM_Underline.Checked;
            Gf.LMFontFormat = (Gf.LMFontBold ? 1 : 0) + (Gf.LMFontItalic ? 1 : 0) * 2 + (Gf.LMFontUnderline ? 1 : 0) * 4;
            Gf.DisableSreenSaver = cbDisableScreenSaver.Checked;
            Gf.VideoSize = (int)VideoSizeUpDown1.Value;
            Gf.VideoVAlign = DataUtil.ObjToInt(Video_VAlign.Tag);
            if (Gf.FocusedTextRegionColour != btnTextRegionChangeColour.ForeColor || Gf.TextRegionSlideTextColour != btnTextRegionSlideTextColour.ForeColor || Gf.TextRegionSlideBackColour != btnTextRegionSlideBackColour.ForeColor || Gf.UseFocusedTextRegionColour != TextRegionUseColour.Checked)
            {
                Gf.Options_PreviewAreaChanged = true;
            }
            Gf.FocusedTextRegionColour = btnTextRegionChangeColour.ForeColor;
            Gf.TextRegionSlideTextColour = btnTextRegionSlideTextColour.ForeColor;
            Gf.TextRegionSlideBackColour = btnTextRegionSlideBackColour.ForeColor;
            Gf.UseFocusedTextRegionColour = TextRegionUseColour.Checked;
            Gf.AutoFocusTextRegion = false;
            Gf.JumpToA = toolStripJumpA.SelectedIndex + 1;
            Gf.JumpToB = toolStripJumpB.SelectedIndex + 1;
            Gf.JumpToC = toolStripJumpC.SelectedIndex + 1;
            Gf.LiveCamNumber = cbCaptureDevices.SelectedIndex + 1;
            Gf.LiveCamVolume = TrackBarVolume.Value;
            Gf.LiveCamBalance = TrackBarBalance.Value;
            Gf.LiveCamMute = cbMute.Checked;
            Gf.LiveCamWidescreen = cbWidescreen.Checked;
            Gf.LiveCamNoPanelOverlay = checkBoxLiveCamNoPanel.Checked;
            Gf.ParentalAlertDuration = (int)ParentalAlertUpDown.Value;
            Gf.ParentalAlertScroll = Parental_Scroll.Checked;
            Gf.ParentalAlertFlash = Parental_Flash.Checked;
            Gf.ParentalAlertTransparent = Parental_Transparent.Checked;
            Gf.ParentalAlertHeading = ParentalAlert.Text;
            Gf.ParentalAlertFontName = ((ParentalComboFont.Text != "") ? ParentalComboFont.Text : "Microsoft Sans Serif");
            Gf.ParentalAlertFontSize = (int)ParentalSizeUpDown.Value;
            Gf.ParentalAlertBold = Parental_Bold.Checked;
            Gf.ParentalAlertItalic = Parental_Italics.Checked;
            Gf.ParentalAlertUnderline = Parental_Underline.Checked;
            Gf.ParentalAlertShadow = Parental_Shadow.Checked;
            Gf.ParentalAlertOutline = Parental_Outline.Checked;
            Gf.ParentalAlertFontFormat = (Gf.ParentalAlertBold ? 1 : 0) + (Gf.ParentalAlertItalic ? 1 : 0) * 2 + (Gf.ParentalAlertUnderline ? 1 : 0) * 4 + (Gf.ParentalAlertShadow ? 1 : 0) * 8 + (Gf.ParentalAlertOutline ? 1 : 0) * 16;
            Gf.ParentalAlertTextColour = btnParentalChangeTextColour.ForeColor;
            Gf.ParentalAlertBackColour = btnParentalChangeBackColour.ForeColor;
            Gf.ParentalAlertTextAlign = DataUtil.ObjToInt(Parental_Align.Tag);
            Gf.ParentalAlertVerticalAlign = DataUtil.ObjToInt(Parental_VAlign.Tag);
            Gf.MessageAlertDuration = (int)MessageAlertDurationUpDown.Value;
            Gf.MessageAlertScroll = Message_Scroll.Checked;
            Gf.MessageAlertFlash = Message_Flash.Checked;
            Gf.MessageAlertTransparent = Message_Transparent.Checked;
            Gf.MessageAlertFontName = ((MessageComboFont.Text != "") ? MessageComboFont.Text : "Microsoft Sans Serif");
            Gf.MessageAlertFontSize = (int)MessageSizeUpDown.Value;
            Gf.MessageAlertBold = Message_Bold.Checked;
            Gf.MessageAlertItalic = Message_Italics.Checked;
            Gf.MessageAlertUnderline = Message_Underline.Checked;
            Gf.MessageAlertShadow = Message_Shadow.Checked;
            Gf.MessageAlertOutline = Message_Outline.Checked;
            Gf.MessageAlertFontFormat = (Gf.MessageAlertBold ? 1 : 0) + (Gf.MessageAlertItalic ? 1 : 0) * 2 + (Gf.MessageAlertUnderline ? 1 : 0) * 4 + (Gf.MessageAlertShadow ? 1 : 0) * 8 + (Gf.MessageAlertOutline ? 1 : 0) * 16;
            Gf.MessageAlertTextColour = btnMessageChangeTextColour.ForeColor;
            Gf.MessageAlertBackColour = btnMessageChangeBackColour.ForeColor;
            Gf.MessageAlertTextAlign = DataUtil.ObjToInt(Message_Align.Tag);
            Gf.MessageAlertVerticalAlign = DataUtil.ObjToInt(Message_VAlign.Tag);
            Gf.ReferenceAlertDuration = (int)ReferenceAlertDurationUpDown.Value;
            Gf.ReferenceAlertScroll = Reference_Scroll.Checked;
            Gf.ReferenceAlertFlash = Reference_Flash.Checked;
            Gf.ReferenceAlertTransparent = Reference_Transparent.Checked;
            Gf.ReferenceAlertFontName = ((ReferenceComboFont.Text != "") ? ReferenceComboFont.Text : "Microsoft Sans Serif");
            Gf.ReferenceAlertFontSize = (int)ReferenceSizeUpDown.Value;
            Gf.ReferenceAlertBold = Reference_Bold.Checked;
            Gf.ReferenceAlertItalic = Reference_Italics.Checked;
            Gf.ReferenceAlertUnderline = Reference_Underline.Checked;
            Gf.ReferenceAlertShadow = Reference_Shadow.Checked;
            Gf.ReferenceAlertOutline = Reference_Outline.Checked;
            Gf.ReferenceAlertFontFormat = (Gf.ReferenceAlertBold ? 1 : 0) + (Gf.ReferenceAlertItalic ? 1 : 0) * 2 + (Gf.ReferenceAlertUnderline ? 1 : 0) * 4 + (Gf.ReferenceAlertShadow ? 1 : 0) * 8 + (Gf.ReferenceAlertOutline ? 1 : 0) * 16;
            Gf.ReferenceAlertTextColour = btnReferenceChangeTextColour.ForeColor;
            Gf.ReferenceAlertBackColour = btnReferenceChangeBackColour.ForeColor;
            Gf.ReferenceAlertTextAlign = DataUtil.ObjToInt(Reference_Align.Tag);
            Gf.ReferenceAlertVerticalAlign = DataUtil.ObjToInt(Reference_VAlign.Tag);
            Gf.ReferenceAlertUsePick = cbPick.Checked;
            Gf.ReferenceAlertBlankIfPickNotFound = cbPickBlank.Checked;
            Gf.ReferenceAlertPickName = tbPick.Text;
            Gf.ReferenceAlertPickSubstitute = tbSubstitute.Text;
            if (Reference_Source1.Checked)
            {
                Gf.ReferenceAlertSource = 1;
            }
            else if (Reference_Source2.Checked)
            {
                Gf.ReferenceAlertSource = 2;
            }
            else if (Reference_Source3.Checked)
            {
                Gf.ReferenceAlertSource = 3;
            }
            else if (Reference_Source4.Checked)
            {
                Gf.ReferenceAlertSource = 4;
            }
            else
            {
                Gf.ReferenceAlertSource = 0;
            }
            if (Gf.MediaDir != tbMusicLocation.Text)
            {
                Gf.MediaDir = tbMusicLocation.Text;
                Gf.Options_MediaDirChanged = true;
            }
            Gf.KeyBoardOption = ((!rbKeyBoardOpt0.Checked) ? 1 : 0);

            //daniel
            //Global Keyboard F7, F8
            Gf.GlobalHookKey_F7 = ChkGlobalHookF7.Checked;
            Gf.GlobalHookKey_F8 = ChkGlobalHookF8.Checked;

            //daniel
            //Global Keyboard F9, F10
            Gf.GlobalHookKey_F9 = ChkGlobalHookF9.Checked;
            Gf.GlobalHookKey_F10 = ChkGlobalHookF10.Checked;

            Gf.GlobalHookKey_Arrow = ChkGlobalHookArrow.Checked;
            Gf.GlobalHookKey_CtrlArrow = ChkGlobalHookCtrlArrow.Checked;

            if (Gf.GlobalHookKey_F7 || Gf.GlobalHookKey_F8 || Gf.GlobalHookKey_F9 || Gf.GlobalHookKey_F10)
            {
                FrmMain.frmMain.RemoveHookBlackScreen();
                FrmMain.frmMain.AddHookBlackScreen();
            }
            else
            {
                FrmMain.frmMain.RemoveHookBlackScreen();
            }
            FrmMain.frmMain.UpdateBlackScreenShortcut();

            if (Gf.GlobalHookKey_Arrow || Gf.GlobalHookKey_CtrlArrow)
            {
                FrmMain.frmMain.RemoveHookSlideUpDown();
                FrmMain.frmMain.AddHookSlideUpDown();
            }
            else
            {
                FrmMain.frmMain.RemoveHookSlideUpDown();
            }

            Gf.SaveConfigSettings();
            DisplayInfo.SizeLaunchDisplay();
        }

        private void BuildFolderList()
        {
            ListViewItem listViewItem = new ListViewItem();
            SongFolder.Items.Clear();
            toolStripJumpA.Items.Clear();
            toolStripJumpB.Items.Clear();
            toolStripJumpC.Items.Clear();
            for (int i = 1; i < Gf.MAXSONGSFOLDERS; i++)
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
            for (int i = 1; i < Gf.MAXSONGSFOLDERS; i++)
            {
                if (Gf.JumpToA == i)
                {
                    toolStripJumpA.SelectedIndex = i - 1;
                }
                if (Gf.JumpToB == i)
                {
                    toolStripJumpB.SelectedIndex = i - 1;
                }
                if (Gf.JumpToC == i)
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
            CurFolder = Gf.GetSelectedIndex(SongFolder) + 1;
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
                Gf.UpdatePosUpDowns(ref FontPositionUpDown0, ref FontPositionUpDown1, ref FontPositionUpDownBottom, ref tempShowFontVPosition[CurFolder, 0], ref tempShowFontVPosition[CurFolder, 1], tempBottomMargin[CurFolder]);
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
            Gf.AssignDropDownItem(SelectedMenuItemName: (tempFolderHeadingFontItalic[CurFolder, 0] && tempFolderHeadingFontItalic[CurFolder, 1]) ? HeadingsFont_Italics1.Name : ((!tempFolderHeadingFontItalic[CurFolder, 1]) ? HeadingsFont_Italics0.Name : HeadingsFont_Italics2.Name), SelectedBtn: ref HeadingsFont_Italics, InMenuItem1: HeadingsFont_Italics0, InMenuItem2: HeadingsFont_Italics1, InMenuItem3: HeadingsFont_Italics2);
            HeadingsFont_Underline.Checked = tempFolderHeadingFontUnderline[CurFolder, 0];
        }

        private void UpdateFontsFields()
        {
            ComboFontName0.Text = tempShowFontName[CurFolder, 0];
            ComboFontName1.Text = tempShowFontName[CurFolder, 1];
            FontSizeUpDown0.Value = tempShowFontSize[CurFolder, 0];
            FontSizeUpDown1.Value = tempShowFontSize[CurFolder, 1];
            ToolBarFont_R1Bold.Checked = tempShowFontBold[CurFolder, 0];
            Gf.AssignDropDownItem(SelectedMenuItemName: (tempShowFontItalic[CurFolder, 0] && tempShowFontItalic[CurFolder, 2]) ? ToolBarFont_R1Italics1.Name : ((!tempShowFontItalic[CurFolder, 2]) ? ToolBarFont_R1Italics0.Name : ToolBarFont_R1Italics2.Name), SelectedBtn: ref ToolBarFont_R1Italics, InMenuItem1: ToolBarFont_R1Italics0, InMenuItem2: ToolBarFont_R1Italics1, InMenuItem3: ToolBarFont_R1Italics2);
            ToolBarFont_R1Underline.Checked = tempShowFontUnderline[CurFolder, 0];
            ToolBarFont_R1RTL.Checked = tempShowFontRTL[CurFolder, 0];
            ToolBarFont_R2Bold.Checked = tempShowFontBold[CurFolder, 1];
            Gf.AssignDropDownItem(SelectedMenuItemName: (tempShowFontItalic[CurFolder, 1] && tempShowFontItalic[CurFolder, 3]) ? ToolBarFont_R2Italics1.Name : ((!tempShowFontItalic[CurFolder, 3]) ? ToolBarFont_R2Italics0.Name : ToolBarFont_R2Italics2.Name), SelectedBtn: ref ToolBarFont_R2Italics, InMenuItem1: ToolBarFont_R2Italics0, InMenuItem2: ToolBarFont_R2Italics1, InMenuItem3: ToolBarFont_R2Italics2);
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
            Gf.AssignDropDownItem(ref HeadingsFont_Italics, e.ClickedItem.Name, HeadingsFont_Italics0, HeadingsFont_Italics1, HeadingsFont_Italics2);
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
            Gf.AssignDropDownItem(ref ToolBarFont_R1Italics, e.ClickedItem.Name, ToolBarFont_R1Italics0, ToolBarFont_R1Italics1, ToolBarFont_R1Italics2);
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
            Gf.AssignDropDownItem(ref ToolBarFont_R2Italics, e.ClickedItem.Name, ToolBarFont_R2Italics0, ToolBarFont_R2Italics1, ToolBarFont_R2Italics2);
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
            if (!((SongFolder.Items.Count == 0) | (Gf.GetSelectedIndex(SongFolder) < 0)))
            {
                SongFolder.Items[Gf.GetSelectedIndex(SongFolder)].BeginEdit();
            }
        }

        private void SongFolder_AfterLabelEdit(object sender, LabelEditEventArgs e)
        {
            int selectedIndex = Gf.GetSelectedIndex(SongFolder);
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
                using DataTable datatable = DbController.GetDataTable(Gf.ConnectSQLiteDef + Gf.BiblesListFileName, fullSearchString);
                BibleList.Items.Clear();
                if (datatable.Rows.Count > 0)
                {

                    foreach (DataRow dr in datatable.Rows)
                    {
                        if (tempHB_TotalVersions <= 250)
                        {
                            listViewItem = BibleList.Items.Add(DataUtil.GetDataString(dr, "name"));
                            string InFileName = DataUtil.GetDataString(dr, "filename");
                            if (File.Exists(Gf.BibleDir + InFileName))
                            {
                                listViewItem.ImageIndex = 4;
                                listViewItem.SubItems.Add(DataUtil.GetDataString(dr, "description"));
                            }
                            else
                            {
                                listViewItem.ImageIndex = 27;
                                listViewItem.SubItems.Add("** Cannot find Bible - please check Filename!");
                            }
                            listViewItem.SubItems.Add(Gf.GetDisplayNameOnly(ref InFileName, UpdateByRef: false, KeepExt: true));
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
            for (int i = 1; i < Gf.MAXSONGSFOLDERS; i++)
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
            string[] files = Directory.GetFiles(Gf.BibleDir, "*.mdb");
            try
            {
                string[] array = files;
                foreach (string text2 in array)
                {
                    text = text2;
                    if (Gf.LookUpBibleName(text, ref Name, ref Description, ref Copyright, ref Info) && !BibleFileNameOnBibleList(text))
                    {
                        listViewItem = BibleSearchList.Items.Add(Name);
                        listViewItem.ImageIndex = 4;
                        listViewItem.SubItems.Add(Description);
                        listViewItem.SubItems.Add(Gf.GetDisplayNameOnly(ref text, UpdateByRef: false, KeepExt: true));
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
            Gf.GetDisplayNameOnly(ref InFileName, UpdateByRef: true, KeepExt: true);
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
            if (Gf.GetSelectedIndex(BibleSearchList) < 0)
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
            Gf.Options_BibleListChanged = true;
        }

        private void btnBibleRemove_Click(object sender, EventArgs e)
        {
            if (BibleList.Items.Count == 0)
            {
                MessageBox.Show("There are no Bibles to remove!");
                return;
            }
            if (Gf.GetSelectedIndex(BibleList) < 0)
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
            Gf.Options_BibleListChanged = true;
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
            int selectedIndex = Gf.GetSelectedIndex(BibleList);
            if (selectedIndex >= 0)
            {
                string fileName = Gf.BibleDir + BibleList.Items[selectedIndex].SubItems[2].Text;
                string Name = "";
                string Description = "";
                string Copyright = "";
                string Info = "";
                if (Gf.LookUpBibleName(fileName, ref Name, ref Description, ref Copyright, ref Info) && Info != "")
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
            int selectedIndex = Gf.GetSelectedIndex(BibleList);
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
                Gf.Options_BibleListChanged = true;
            }
        }

        private void MoveBibleDown()
        {
            int count = BibleList.Items.Count;
            if (count <= 1)
            {
                return;
            }
            int selectedIndex = Gf.GetSelectedIndex(BibleList);
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
                Gf.Options_BibleListChanged = true;
            }
        }

        public void SaveBibleChanges()
        {
            using DbConnection connection = DbController.GetDbConnection(Gf.ConnectStringSQLiteDef + Gf.BiblesListFileName);
            DbDataAdapter sQLiteDataAdapter;
            DataTable dataTable;
            try
            {
                string query = "select * from Biblefolder where NAME like \"%\" ";

                (sQLiteDataAdapter, dataTable) = DbController.GetDataAdapter(connection, query);

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
                    (sQLiteDataAdapter, dataTable) = DbController.GetDataAdapter(connection, "select * from Biblefolder where FILENAME = \"" + BibleList.Items[i].SubItems[2].Text + "\"");

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
                        (sQLiteDataAdapter, dataTable) = DbController.GetDataAdapter(connection, query);

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

                    Gf.HB_Versions[i, 1] = BibleList.Items[i].Text;
                    Gf.HB_Versions[i, 2] = BibleList.Items[i].SubItems[1].Text;
                    Gf.HB_Versions[i, 4] = Gf.BibleDir + BibleList.Items[i].SubItems[2].Text;
                    Gf.HB_Versions[i, 3] = BibleList.Items[i].SubItems[3].Text;
                    Gf.HB_Versions[i, 5] = BibleList.Items[i].SubItems[4].Text;
                    Gf.HB_Versions[i, 6] = BibleList.Items[i].SubItems[5].Text;
                }

                Gf.HB_TotalVersions = BibleList.Items.Count;
                (sQLiteDataAdapter, dataTable) = DbController.GetDataAdapter(connection, "select * from Biblefolder where displayorder < 0 ");

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

        private void btnBibleNameChange_Click(object sender, EventArgs e)
        {
            int selectedIndex = Gf.GetSelectedIndex(BibleList);
            if (selectedIndex < 0)
            {
                return;
            }
            Gf.Rename_String = BibleList.Items[selectedIndex].Text;
            Gf.Rename_ExistingString = "";
            for (int i = 0; i < BibleList.Items.Count; i++)
            {
                if (selectedIndex != i)
                {
                    Gf.Rename_ExistingString = Gf.Rename_ExistingString + BibleList.Items[i].Text + ";";
                }
            }
            FrmBibleRename frmBibleRename = new FrmBibleRename();
            if (frmBibleRename.ShowDialog() == DialogResult.OK)
            {
                BibleList.Items[selectedIndex].Text = Gf.Rename_String;
                Gf.Options_BibleListChanged = true;
            }
        }

        private void BibleList_SelectedIndexChanged(object sender, EventArgs e)
        {
            BibleListIndexChanged();
        }

        private void BibleListIndexChanged()
        {
            int selectedIndex = Gf.GetSelectedIndex(BibleList);
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
            int selectedIndex = Gf.GetSelectedIndex(BibleList);
            if (selectedIndex >= 0)
            {
                BibleList.Items[selectedIndex].SubItems[4].Text = ((BibleAssociatedFolder.Text == "") ? "1" : Convert.ToString(BibleAssociatedFolder.SelectedIndex + 1));
                if (!InitFormLoad)
                {
                    Gf.Options_BibleListChanged = true;
                }
            }
        }

        private void BibleFontSizeUpDown_ValueChanged(object sender, EventArgs e)
        {
            int selectedIndex = Gf.GetSelectedIndex(BibleList);
            if (selectedIndex >= 0)
            {
                BibleList.Items[selectedIndex].SubItems[5].Text = BibleFontSizeUpDown.Value.ToString();
                if (!InitFormLoad)
                {
                    Gf.Options_BibleListChanged = true;
                }
            }
        }

        private void BuildLicencesList()
        {
            AdminLic4.Text = Gf.LicAdmin_List[4, 0];
            AdminLic5.Text = Gf.LicAdmin_List[5, 0];
            AdminLic6.Text = Gf.LicAdmin_List[6, 0];
            AdminLic7.Text = Gf.LicAdmin_List[7, 0];
            AdminLic8.Text = Gf.LicAdmin_List[8, 0];
            AdminLicNo3.Text = Gf.LicAdmin_List[3, 1];
            AdminLicNo4.Text = Gf.LicAdmin_List[4, 1];
            AdminLicNo5.Text = Gf.LicAdmin_List[5, 1];
            AdminLicNo6.Text = Gf.LicAdmin_List[6, 1];
            AdminLicNo7.Text = Gf.LicAdmin_List[7, 1];
            AdminLicNo8.Text = Gf.LicAdmin_List[8, 1];
            AdminLicPreview3.Text = Gf.LicAdmin_List[3, 2];
            AdminLicPreview4.Text = Gf.LicAdmin_List[4, 2];
            AdminLicPreview5.Text = Gf.LicAdmin_List[5, 2];
            AdminLicPreview6.Text = Gf.LicAdmin_List[6, 2];
            AdminLicPreview7.Text = Gf.LicAdmin_List[7, 2];
            AdminLicPreview8.Text = Gf.LicAdmin_List[8, 2];
            tbNumberSymbol.Text = Gf.LicAdminNoSymbol;
            cbEnforceDisplay.Checked = Gf.LicAdminEnforceDisplay;
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

        private void LoadGeneralSetting()
        {
            VersesMaxUpDown.Value = Gf.HB_MaxVersesSelection;
            AdhocVersesMaxUpDown.Value = Gf.HB_MaxAdhocVersesSelection;
            PPMaxUpDown.Value = Gf.PP_MaxFiles;
            EditHistoryMaxUpDown.Value = Gf.MaxUserEditHistory;
            BuildMonitorsList();
            cbDisableScreenSaver.Checked = Gf.DisableSreenSaver;
            VideoSizeUpDown1.Value = Gf.VideoSize;
            Gf.AssignDropDownItem(SelectedMenuItemName: (Gf.VideoVAlign == 0) ? Video_VAlignTop.Name : ((Gf.VideoVAlign != 1) ? Video_VAlignBottom.Name : Video_VAlignCentre.Name), SelectedBtn: ref Video_VAlign, InMenuItem1: Video_VAlignTop, InMenuItem2: Video_VAlignCentre, InMenuItem3: Video_VAlignBottom);
            UpdateVideoSizeSample();
            DM_AlwaysUseSecondaryMonitor.Checked = Gf.DMAlwaysUseSecondaryMonitor;
            if (Gf.DualMonitorSelectAutoOption == 0)
            {
                optDM0.Checked = true;
            }
            else
            {
                optDM1.Checked = true;
            }
            // daniel
            // 스크린 옵션 추가
            optWide.Checked = (Gf.isScreenWideMode) ? true : false;

            DM1UpDownTop.Value = Gf.DMOption1Top;
            DM1UpDownLeft.Value = Gf.DMOption1Left;
            DM1UpDownWidth.Value = (Gf.DMOption1Width < 1) ? 1 : Gf.DMOption1Width;
            DM1UpDownHeight.Value = (Gf.DMOption1Height < 1) ? 1 : Gf.DMOption1Height;

            //DM1UpDownHeight.Value = Gf.DMOption1Height;

            DM_CustomAsSingleMonitor.Checked = Gf.DMOption1AsSingleMonitor;

            DisplayInfo.GetDisplayName(ref Gf.OutputMonitorName, "None");

            int outputMonitorIndex = DualMonitorList.FindString(Gf.OutputMonitorName);

            DualMonitorList.SelectedIndex = outputMonitorIndex;

            LM_AlwaysUse.Checked = Gf.LMAlwaysUseSecondaryMonitor;
            if (Gf.LMSelectAutoOption == 1)
            {
                optLM1.Checked = true;
            }
            else
            {
                optLM0.Checked = true;
            }
            LM1UpDownTop.Value = Gf.LMOption1Top;
            LM1UpDownLeft.Value = Gf.LMOption1Left;
            LM1UpDownWidth.Value = ((Gf.LMOption1Width < 1) ? 1 : Gf.LMOption1Width);
            LM1UpDownHeight.Value = ((Gf.LMOption1Height < 1) ? 1 : Gf.LMOption1Height);
            /// Gf.DualMonitorSelectAutoOption :
            //Gf.GetScreenNumber(ref Gf.LyricsMonitorNumber, (Gf.DualMonitorSelectAutoOption == 0) ? Gf.OutputMonitorNumber : (-1));

            if (Gf.LyricsMonitorName != "None")
            {
                DisplayInfo.GetDisplayName(ref Gf.LyricsMonitorName, (Gf.DualMonitorSelectAutoOption == 0) ? Gf.OutputMonitorName : "None");
            }

            int lyricsMonitorIndex = LyricsMonitorList.FindString(Gf.LyricsMonitorName);

            LyricsMonitorList.SelectedIndex = lyricsMonitorIndex;

            btnLMTextColour.ForeColor = Gf.LMTextColour;
            btnLMHighlightColour.ForeColor = Gf.LMHighlightColour;
            btnLMBackColour.ForeColor = Gf.LMBackColour;
            PreviewFontUpDown.Value = ((Gf.PreviewArea_FontSize >= 8) ? Gf.PreviewArea_FontSize : 8);
            LMUpDownFontSize.Value = ((Gf.LMMainFontSize >= 8 && Gf.LMMainFontSize <= 40) ? Gf.LMMainFontSize : 20);
            LMNotationsUpDownFontSize.Value = ((Gf.LMNotationsFontSize >= 8 && Gf.LMNotationsFontSize <= 40) ? Gf.LMNotationsFontSize : 20);
            cbLMShowNotations.Checked = Gf.LMShowNotations;
            LM_Bold.Checked = Gf.LMFontBold;
            LM_Italic.Checked = Gf.LMFontItalic;
            LM_Underline.Checked = Gf.LMFontUnderline;
            btnTextRegionChangeColour.ForeColor = Gf.FocusedTextRegionColour;
            btnTextRegionSlideTextColour.ForeColor = Gf.TextRegionSlideTextColour;
            btnTextRegionSlideBackColour.ForeColor = Gf.TextRegionSlideBackColour;
            TextRegionUseColour.Checked = Gf.UseFocusedTextRegionColour;
            checkBoxPPTab.Checked = Gf.UsePowerpointTab;
            checkBoxPPNoPanel.Checked = Gf.NoPowerpointPanelOverlay;
            checkBoxMediaTab.Checked = Gf.UseMediaTab;
            checkBoxMediaNoPanel.Checked = Gf.NoMediaPanelOverlay;
            checkBoxLMBox.Checked = Gf.ShowLyricsMonitorAlertBox;
            cbUseLargestFont.Checked = Gf.UseLargestFontSize;
            cbAutoTextOverflow.Checked = Gf.AutoTextOverflow;
            cbAdvanceNextItem.Checked = Gf.AdvanceNextItem;
            cbLineBetweenRegions.Checked = Gf.LineBetweenRegions;
            cbWordWrapLeftAlignIndent.Checked = Gf.WordWrapLeftAlignIndent;
            if (Gf.GapItemOption == GapType.Black)
            {
                rbGapItemOption1.Checked = true;
            }
            else if (Gf.GapItemOption == GapType.Default)
            {
                rbGapItemOption2.Checked = true;
            }
            else if (Gf.GapItemOption == GapType.User)
            {
                rbGapItemOption3.Checked = true;
            }
            else
            {
                rbGapItemOption0.Checked = true;
            }
            tbGapLogoLocation.Text = Gf.GapItemLogoFile;
            cbGapItemUseFade.Checked = Gf.GapItemUseFade;
            NotationFontFactorUpDown.Value = (int)(Gf.NotationFontFactor * 100.0);
            cbPreviewShowNotations.Checked = Gf.PreviewArea_ShowNotations;
            cbLineBetweenScreens.Checked = Gf.PreviewArea_LineBetweenScreens;
            PreviewFontUpDown.Value = ((Gf.PreviewArea_FontSize >= 8) ? Gf.PreviewArea_FontSize : 8);
            tbMusicLocation.Text = Gf.MediaDir;
            tbMusicLocation.BackColor = tbGapLogoLocation.BackColor;
            Gf.LoadBlankCaptureDevices(ref cbCaptureDevices);
            if (Gf.WMP_Present)
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
            cbCaptureDevices.SelectedIndex = Gf.LiveCamNumber - 1;
            TrackBarVolume.Value = (((Gf.LiveCamVolume >= 0) & (Gf.LiveCamVolume <= 100)) ? Gf.LiveCamVolume : 30);
            TrackBarBalance.Value = (((Gf.LiveCamBalance >= -100) & (Gf.LiveCamBalance <= 100)) ? Gf.LiveCamBalance : 0);
            cbMute.Checked = Gf.LiveCamMute;
            cbWidescreen.Checked = Gf.LiveCamWidescreen;
            checkBoxLiveCamNoPanel.Checked = Gf.LiveCamNoPanelOverlay;
            MessageAlertDurationUpDown.Value = Gf.MessageAlertDuration;
            Message_Scroll.Checked = Gf.MessageAlertScroll;
            Message_Flash.Checked = Gf.MessageAlertFlash;
            Message_Transparent.Checked = Gf.MessageAlertTransparent;
            MessageComboFont.Text = Gf.MessageAlertFontName;
            MessageSizeUpDown.Value = Gf.MessageAlertFontSize;
            btnMessageChangeTextColour.ForeColor = Gf.MessageAlertTextColour;
            btnMessageChangeBackColour.ForeColor = Gf.MessageAlertBackColour;
            Gf.AssignDropDownItem(SelectedMenuItemName: (Gf.MessageAlertTextAlign == 1) ? Message_AlignLeft.Name : ((Gf.MessageAlertTextAlign != 2) ? Message_AlignRight.Name : Message_AlignCentre.Name), SelectedBtn: ref Message_Align, InMenuItem1: Message_AlignLeft, InMenuItem2: Message_AlignCentre, InMenuItem3: Message_AlignRight);
            Gf.AssignDropDownItem(SelectedMenuItemName: (Gf.MessageAlertVerticalAlign != 0) ? Message_VAlignBottom.Name : Message_VAlignTop.Name, SelectedBtn: ref Message_VAlign, InMenuItem1: Message_VAlignTop, InMenuItem2: Message_VAlignBottom);
            Message_Bold.Checked = Gf.MessageAlertBold;
            Message_Italics.Checked = Gf.MessageAlertItalic;
            Message_Underline.Checked = Gf.MessageAlertUnderline;
            Message_Shadow.Checked = Gf.MessageAlertShadow;
            Message_Outline.Checked = Gf.MessageAlertOutline;
            ParentalAlertUpDown.Value = Gf.ParentalAlertDuration;
            Parental_Scroll.Checked = Gf.ParentalAlertScroll;
            Parental_Flash.Checked = Gf.ParentalAlertFlash;
            Parental_Transparent.Checked = Gf.ParentalAlertTransparent;
            ParentalComboFont.Text = Gf.ParentalAlertFontName;
            ParentalSizeUpDown.Value = Gf.ParentalAlertFontSize;
            btnParentalChangeTextColour.ForeColor = Gf.ParentalAlertTextColour;
            btnParentalChangeBackColour.ForeColor = Gf.ParentalAlertBackColour;
            ParentalAlert.Text = Gf.ParentalAlertHeading;
            Gf.AssignDropDownItem(SelectedMenuItemName: (Gf.ParentalAlertTextAlign == 1) ? Parental_AlignLeft.Name : ((Gf.ParentalAlertTextAlign != 2) ? Parental_AlignRight.Name : Parental_AlignCentre.Name), SelectedBtn: ref Parental_Align, InMenuItem1: Parental_AlignLeft, InMenuItem2: Parental_AlignCentre, InMenuItem3: Parental_AlignRight);
            Gf.AssignDropDownItem(SelectedMenuItemName: (Gf.ParentalAlertVerticalAlign != 0) ? Parental_VAlignBottom.Name : Parental_VAlignTop.Name, SelectedBtn: ref Parental_VAlign, InMenuItem1: Parental_VAlignTop, InMenuItem2: Parental_VAlignBottom);
            Parental_Bold.Checked = Gf.ParentalAlertBold;
            Parental_Italics.Checked = Gf.ParentalAlertItalic;
            Parental_Underline.Checked = Gf.ParentalAlertUnderline;
            Parental_Shadow.Checked = Gf.ParentalAlertShadow;
            Parental_Outline.Checked = Gf.ParentalAlertOutline;
            ReferenceAlertDurationUpDown.Value = Gf.ReferenceAlertDuration;
            Reference_Scroll.Checked = Gf.ReferenceAlertScroll;
            Reference_Flash.Checked = Gf.ReferenceAlertFlash;
            Reference_Transparent.Checked = Gf.ReferenceAlertTransparent;
            ReferenceComboFont.Text = Gf.ReferenceAlertFontName;
            ReferenceSizeUpDown.Value = Gf.ReferenceAlertFontSize;
            btnReferenceChangeTextColour.ForeColor = Gf.ReferenceAlertTextColour;
            btnReferenceChangeBackColour.ForeColor = Gf.ReferenceAlertBackColour;
            Gf.AssignDropDownItem(SelectedMenuItemName: (Gf.ReferenceAlertTextAlign == 1) ? Reference_AlignLeft.Name : ((Gf.ReferenceAlertTextAlign != 2) ? Reference_AlignRight.Name : Reference_AlignCentre.Name), SelectedBtn: ref Reference_Align, InMenuItem1: Reference_AlignLeft, InMenuItem2: Reference_AlignCentre, InMenuItem3: Reference_AlignRight);
            Gf.AssignDropDownItem(SelectedMenuItemName: (Gf.ReferenceAlertVerticalAlign == 0) ? Reference_VAlignTop.Name : ((Gf.ReferenceAlertVerticalAlign != 2) ? Reference_VAlignCentre.Name : Reference_VAlignBottom.Name), SelectedBtn: ref Reference_VAlign, InMenuItem1: Reference_VAlignTop, InMenuItem2: Reference_VAlignCentre, InMenuItem3: Reference_VAlignBottom);
            Reference_Bold.Checked = Gf.ReferenceAlertBold;
            Reference_Italics.Checked = Gf.ReferenceAlertItalic;
            Reference_Underline.Checked = Gf.ReferenceAlertUnderline;
            Reference_Shadow.Checked = Gf.ReferenceAlertShadow;
            Reference_Outline.Checked = Gf.ReferenceAlertOutline;
            cbPick.Checked = Gf.ReferenceAlertUsePick;
            cbPickBlank.Checked = Gf.ReferenceAlertBlankIfPickNotFound;
            tbPick.Text = Gf.ReferenceAlertPickName;
            tbSubstitute.Text = Gf.ReferenceAlertPickSubstitute;
            if (Gf.ReferenceAlertSource == 1)
            {
                Reference_Source1.Checked = true;
            }
            else if (Gf.ReferenceAlertSource == 2)
            {
                Reference_Source2.Checked = true;
            }
            else if (Gf.ReferenceAlertSource == 3)
            {
                Reference_Source3.Checked = true;
            }
            else if (Gf.ReferenceAlertSource == 4)
            {
                Reference_Source4.Checked = true;
            }
            else
            {
                Reference_Source0.Checked = true;
            }
            if (Gf.KeyBoardOption == 0)
            {
                rbKeyBoardOpt0.Checked = true;
            }
            else
            {
                rbKeyBoardOpt1.Checked = true;
            }

            ///Global Hook F7 F8
            ChkGlobalHookF7.Checked = Gf.GlobalHookKey_F7;
            ChkGlobalHookF8.Checked = Gf.GlobalHookKey_F8;

            ///Global Hook F9 F10
            ChkGlobalHookF9.Checked = Gf.GlobalHookKey_F9;
            ChkGlobalHookF10.Checked = Gf.GlobalHookKey_F10;

            ChkGlobalHookArrow.Checked = Gf.GlobalHookKey_Arrow;
            ChkGlobalHookCtrlArrow.Checked = Gf.GlobalHookKey_CtrlArrow;

        }

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
                groupBoxDM.Enabled = !Gf.ShowRunning;
            }
            else
            {
                LyricsMonitorList.Enabled = (optLM0.Checked ? true : false);
                LM_AlwaysUse.Enabled = (optLM0.Checked ? true : false);
                LM1UpDownTop.Enabled = (optLM1.Checked ? true : false);
                LM1UpDownLeft.Enabled = (optLM1.Checked ? true : false);
                LM1UpDownWidth.Enabled = (optLM1.Checked ? true : false);
                groupBoxLM.Enabled = !Gf.ShowRunning;
            }
        }

        private void DM1UpDownWidth_ValueChanged(object sender, EventArgs e)
        {
            //wide mode가 아닐 경우
            if (!Gf.isScreenWideMode)
                Gf.DMOption1Height = (int)DM1UpDownWidth.Value * 3 / 4;
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
            Gf.SelectColorFromBtn(ref btnLMTextColour, ref ColourSymbol);
        }

        private void btnLMHighlightColour_Click(object sender, EventArgs e)
        {
            Color ColourSymbol = btnLMHighlightColour.ForeColor;
            Gf.SelectColorFromBtn(ref btnLMHighlightColour, ref ColourSymbol);
        }

        private void btnLMBackColour_Click(object sender, EventArgs e)
        {
            Color ColourSymbol = btnLMBackColour.ForeColor;
            Gf.SelectColorFromBtn(ref btnLMBackColour, ref ColourSymbol);
        }


        public void BuildMonitorsList()
        {
            DualMonitorList.Items.Clear();
            LyricsMonitorList.Items.Clear();

            // DisplayInfo의 static 메서드를 사용하여 모니터 리스트 가져오기
            List<string> allMonitors = DisplayInfo.GetAllMonitorsList();
            List<string> nonPrimaryMonitors = DisplayInfo.GetNonPrimaryMonitorsList();

            // DualMonitorList에 모든 모니터 추가
            foreach (string monitor in allMonitors)
            {
                DualMonitorList.Items.Add(monitor);
            }

            // LyricsMonitorList에 Primary가 아닌 모니터들과 "None" 추가
            foreach (string monitor in nonPrimaryMonitors)
            {
                LyricsMonitorList.Items.Add(monitor);
            }
        }

        private void Monitor_Info_Click(object sender, EventArgs e)
        {
            if (DualMonitorList.Items.Count > 0)
            {
                int OutTop = 0;
                int OutLeft = 0;
                int OutWidth = 0;
                int OutHeight = 0;
                DisplayInfo.GetDisplayInfo(DualMonitorList.SelectedItem.ToString(), ref OutTop, ref OutLeft, ref OutWidth, ref OutHeight);

                MessageBox.Show(DualMonitorList.Text + " Setup: \nTop:\t" + Convert.ToString(OutTop) + "\nLeft:\t" + Convert.ToString(OutLeft) + "\nWidth:\t" + Convert.ToString(OutWidth) + "\nHeight:\t" + Convert.ToString(OutHeight));
            }
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
                DisplayInfo.GetDisplayInfo(LyricsMonitorList.SelectedItem.ToString(), ref OutTop, ref OutLeft, ref OutWidth, ref OutHeight);
                //Gf.GetScreenInfo(LyricsMonitorList.SelectedIndex, ref OutTop, ref OutLeft, ref OutWidth, ref OutHeight);
                MessageBox.Show(LyricsMonitorList.Text + " Setup: \nTop:\t" + Convert.ToString(OutTop) + "\nLeft:\t" + Convert.ToString(OutLeft) + "\nWidth:\t" + Convert.ToString(OutWidth) + "\nHeight:\t" + Convert.ToString(OutHeight));
            }
        }

        private void btnTextRegionChangeColour_Click(object sender, EventArgs e)
        {
            Color ColourSymbol = btnTextRegionChangeColour.ForeColor;
            Gf.SelectColorFromBtn(ref btnTextRegionChangeColour, ref ColourSymbol);
        }

        private void btnTextRegionSlideTextColour_Click(object sender, EventArgs e)
        {
            Color ColourSymbol = btnTextRegionSlideTextColour.ForeColor;
            Gf.SelectColorFromBtn(ref btnTextRegionSlideTextColour, ref ColourSymbol);
        }

        private void btnTextRegionSlideBackColour_Click(object sender, EventArgs e)
        {
            Color ColourSymbol = btnTextRegionSlideBackColour.ForeColor;
            Gf.SelectColorFromBtn(ref btnTextRegionSlideBackColour, ref ColourSymbol);
        }

        private void Message_Align_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            Gf.AssignDropDownItem(ref Message_Align, e.ClickedItem.Name, Message_AlignLeft, Message_AlignCentre, Message_AlignRight);
        }

        private void Message_VAlign_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            Gf.AssignDropDownItem(ref Message_VAlign, e.ClickedItem.Name, Message_VAlignTop, Message_VAlignBottom);
        }

        private void Parental_Align_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            Gf.AssignDropDownItem(ref Parental_Align, e.ClickedItem.Name, Parental_AlignLeft, Parental_AlignCentre, Parental_AlignRight);
        }

        private void Parental_VAlign_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            Gf.AssignDropDownItem(ref Parental_VAlign, e.ClickedItem.Name, Parental_VAlignTop, Parental_VAlignBottom);
        }

        private void Reference_Align_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            Gf.AssignDropDownItem(ref Reference_Align, e.ClickedItem.Name, Reference_AlignLeft, Reference_AlignCentre, Reference_AlignRight);
        }

        private void Reference_VAlign_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            Gf.AssignDropDownItem(ref Reference_VAlign, e.ClickedItem.Name, Reference_VAlignTop, Reference_VAlignCentre, Reference_VAlignBottom);
        }

        private void btnParentalChangeTextColour_Click(object sender, EventArgs e)
        {
            Color ColourSymbol = btnParentalChangeTextColour.ForeColor;
            Gf.SelectColorFromBtn(ref btnParentalChangeTextColour, ref ColourSymbol);
        }

        private void btnParentalChangeBackColour_Click(object sender, EventArgs e)
        {
            Color ColourSymbol = btnParentalChangeBackColour.ForeColor;
            Gf.SelectColorFromBtn(ref btnParentalChangeBackColour, ref ColourSymbol);
        }

        private void btnMessageChangeTextColour_Click(object sender, EventArgs e)
        {
            Color ColourSymbol = btnMessageChangeTextColour.ForeColor;
            Gf.SelectColorFromBtn(ref btnMessageChangeTextColour, ref ColourSymbol);
        }

        private void btnMessageChangeBackColour_Click(object sender, EventArgs e)
        {
            Color ColourSymbol = btnMessageChangeBackColour.ForeColor;
            Gf.SelectColorFromBtn(ref btnMessageChangeBackColour, ref ColourSymbol);
        }

        private void btnReferenceChangeTextColour_Click(object sender, EventArgs e)
        {
            Color ColourSymbol = btnReferenceChangeTextColour.ForeColor;
            Gf.SelectColorFromBtn(ref btnReferenceChangeTextColour, ref ColourSymbol);
        }

        private void btnReferenceChangeBackColour_Click(object sender, EventArgs e)
        {
            Color ColourSymbol = btnReferenceChangeBackColour.ForeColor;
            Gf.SelectColorFromBtn(ref btnReferenceChangeBackColour, ref ColourSymbol);
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
            folderBrowserDialog1.SelectedPath = ((DataUtil.Trim(tbMusicLocation.Text) != "") ? DataUtil.Trim(tbMusicLocation.Text) : Gf.MediaDir);
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
                text = Gf.ImagesDir;
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
            Gf.AssignDropDownItem(ref Video_VAlign, e.ClickedItem.Name, Video_VAlignTop, Video_VAlignCentre, Video_VAlignBottom);
            UpdateVideoSizeSample();
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
