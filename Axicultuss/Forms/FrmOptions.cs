//using NetOffice.DAOApi;
using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Axicultus.Util;
using Axicultus.Module;
using Axicultus.Global;
using System.Collections.Generic;

namespace Axicultus;

public partial class FrmOptions : Form
{
    private const int SampleSplitterTop = 6;

    private const int SampleSplitterBottom = 117;

    private int CurFolder;

    private bool InitFormLoad = true;

    private float SampleSplitterVerticalIncrement = 0.96f;

    private float SampleSplitterHorizontalIncrement = 1.4f;

    // Slots 1–40 are cloned on load and written back on OK. Slot 0 has its own defaults.
    private readonly FolderSettings[] tempFolders = new FolderSettings[Gf.MAXSONGSFOLDERS];

    private int[,] tempShowFontVPositionMax = new int[Gf.MAXSONGSFOLDERS, 2];

    private int[,] tempShowFontVPositionMin = new int[Gf.MAXSONGSFOLDERS, 2];

    private bool LoadTempPos = false;

    private BibleVersionCatalog originalBibleVersions;

		public FrmOptions()
    {
        tempFolders[0] = new FolderSettings { Name = null };
        InitializeComponent();
        InitModernUi();
    }

    private void FrmOptions_Load(object sender, EventArgs e)
    {
        for (int i = 1; i < Gf.MAXSONGSFOLDERS; i++)
        {
            // Name, GroupStyle, margins, heading size and option, line spacing, headings, font names, sizes and
            // positions are copied as they are. The flags are reduced to 1 or 0 (> 0 gives 1), as the bool copies
            // were, so a stored 2 or -1 is written back as 1 or 0 on OK, as before.
            tempFolders[i] = Gf.Folders[i].Clone();
            tempFolders[i].Use = ((Gf.Folders[i].Use > 0) ? 1 : 0);
            tempFolders[i].HeadingFontBold[0] = ((Gf.Folders[i].HeadingFontBold[0] > 0) ? 1 : 0);
            tempFolders[i].HeadingFontItalic[0] = ((Gf.Folders[i].HeadingFontItalic[0] > 0) ? 1 : 0);
            tempFolders[i].HeadingFontUnderline[0] = ((Gf.Folders[i].HeadingFontUnderline[0] > 0) ? 1 : 0);
            tempFolders[i].HeadingFontBold[1] = ((Gf.Folders[i].HeadingFontBold[1] > 0) ? 1 : 0);
            tempFolders[i].HeadingFontItalic[1] = ((Gf.Folders[i].HeadingFontItalic[1] > 0) ? 1 : 0);
            tempFolders[i].HeadingFontUnderline[1] = ((Gf.Folders[i].HeadingFontUnderline[1] > 0) ? 1 : 0);
            for (int j = 0; j <= 1; j++)
            {
                tempFolders[i].FontBold[j] = ((Gf.Folders[i].FontBold[j] > 0) ? 1 : 0);
                tempFolders[i].FontItalic[j] = ((Gf.Folders[i].FontItalic[j] > 0) ? 1 : 0);
                tempFolders[i].FontUnderline[j] = ((Gf.Folders[i].FontUnderline[j] > 0) ? 1 : 0);
                tempFolders[i].FontRTL[j] = ((Gf.Folders[i].FontRTL[j] > 0) ? 1 : 0);
                tempFolders[i].FontBold[j + 2] = ((Gf.Folders[i].FontBold[j + 2] > 0) ? 1 : 0);
                tempFolders[i].FontItalic[j + 2] = ((Gf.Folders[i].FontItalic[j + 2] > 0) ? 1 : 0);
                tempFolders[i].FontUnderline[j + 2] = ((Gf.Folders[i].FontUnderline[j + 2] > 0) ? 1 : 0);
            }
            tempShowFontVPositionMax[i, 0] = Gf.Folders[i].FontVPosition[1];
            tempShowFontVPositionMin[i, 1] = Gf.Folders[i].FontVPosition[0];
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
        if (Gf.Options_BibleListChanged)
        {
            try
            {
                SaveBibleChanges();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Could not save the Bible list", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }
        SaveVariables();
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
            if ((Gf.Folders[i].Name != tempFolders[i].Name) | (Gf.Folders[i].GroupStyle != tempFolders[i].GroupStyle) | (Gf.Folders[i].Use != tempFolders[i].Use))
            {
                Gf.Options_FolderListChanged = true;
            }
            Gf.Folders[i].Name = tempFolders[i].Name;
            Gf.Folders[i].GroupStyle = tempFolders[i].GroupStyle;
            Gf.Folders[i].Use = tempFolders[i].Use;
            if (Gf.Folders[i].LeftMargin != tempFolders[i].LeftMargin || Gf.Folders[i].RightMargin != tempFolders[i].RightMargin || Gf.Folders[i].BottomMargin != tempFolders[i].BottomMargin || Gf.Folders[i].LineSpacing[0] != tempFolders[i].LineSpacing[0] || Gf.Folders[i].LineSpacing[1] != tempFolders[i].LineSpacing[1] || Gf.Folders[i].HeadingPercentSize != tempFolders[i].HeadingPercentSize || Gf.Folders[i].HeadingOption != tempFolders[i].HeadingOption || Gf.Folders[i].HeadingFontBold[0] != tempFolders[i].HeadingFontBold[0] || Gf.Folders[i].HeadingFontItalic[0] != tempFolders[i].HeadingFontItalic[0] || Gf.Folders[i].HeadingFontUnderline[0] != tempFolders[i].HeadingFontUnderline[0] || Gf.Folders[i].HeadingFontBold[1] != tempFolders[i].HeadingFontBold[1] || Gf.Folders[i].HeadingFontItalic[1] != tempFolders[i].HeadingFontItalic[1] || Gf.Folders[i].HeadingFontUnderline[1] != tempFolders[i].HeadingFontUnderline[1])
            {
                Gf.Folders[i].LeftMargin = tempFolders[i].LeftMargin;
                Gf.Folders[i].RightMargin = tempFolders[i].RightMargin;
                Gf.Folders[i].BottomMargin = tempFolders[i].BottomMargin;
                Gf.Folders[i].LineSpacing[0] = tempFolders[i].LineSpacing[0];
                Gf.Folders[i].LineSpacing[1] = tempFolders[i].LineSpacing[1];
                Gf.Folders[i].HeadingPercentSize = tempFolders[i].HeadingPercentSize;
                Gf.Folders[i].HeadingOption = tempFolders[i].HeadingOption;
                Gf.Folders[i].HeadingFontBold[0] = tempFolders[i].HeadingFontBold[0];
                Gf.Folders[i].HeadingFontItalic[0] = tempFolders[i].HeadingFontItalic[0];
                Gf.Folders[i].HeadingFontUnderline[0] = tempFolders[i].HeadingFontUnderline[0];
                Gf.Folders[i].HeadingFontBold[1] = tempFolders[i].HeadingFontBold[1];
                Gf.Folders[i].HeadingFontItalic[1] = tempFolders[i].HeadingFontItalic[1];
                Gf.Folders[i].HeadingFontUnderline[1] = tempFolders[i].HeadingFontUnderline[1];
                Gf.Options_FolderFormatChanged = true;
            }
            for (int j = 0; j < 4; j++)
            {
                if (Gf.Folders[i].LyricsHeading[j] != tempFolders[i].LyricsHeading[j])
                {
                    Gf.Options_FolderFormatChanged = true;
                }
                Gf.Folders[i].LyricsHeading[j] = tempFolders[i].LyricsHeading[j];
            }
            for (int j = 0; j <= 1; j++)
            {
                if (Gf.Folders[i].FontSize[j] != tempFolders[i].FontSize[j] || Gf.Folders[i].FontBold[j] != tempFolders[i].FontBold[j] || Gf.Folders[i].FontItalic[j] != tempFolders[i].FontItalic[j] || Gf.Folders[i].FontUnderline[j] != tempFolders[i].FontUnderline[j] || Gf.Folders[i].FontRTL[j] != tempFolders[i].FontRTL[j] || Gf.Folders[i].FontBold[j + 2] != tempFolders[i].FontBold[j + 2] || Gf.Folders[i].FontItalic[j + 2] != tempFolders[i].FontItalic[j + 2] || Gf.Folders[i].FontUnderline[j + 2] != tempFolders[i].FontUnderline[j + 2] || Gf.Folders[i].FontName[j] != tempFolders[i].FontName[j] || Gf.Folders[i].FontVPosition[j] != tempFolders[i].FontVPosition[j])
                {
                    Gf.Options_FolderFormatChanged = true;
                }
                Gf.Folders[i].FontSize[j] = tempFolders[i].FontSize[j];
                Gf.Folders[i].FontBold[j] = tempFolders[i].FontBold[j];
                Gf.Folders[i].FontItalic[j] = tempFolders[i].FontItalic[j];
                Gf.Folders[i].FontUnderline[j] = tempFolders[i].FontUnderline[j];
                Gf.Folders[i].FontRTL[j] = tempFolders[i].FontRTL[j];
                Gf.Folders[i].FontBold[j + 2] = tempFolders[i].FontBold[j + 2];
                Gf.Folders[i].FontItalic[j + 2] = tempFolders[i].FontItalic[j + 2];
                Gf.Folders[i].FontUnderline[j + 2] = tempFolders[i].FontUnderline[j + 2];
                Gf.Folders[i].FontName[j] = tempFolders[i].FontName[j];
                Gf.Folders[i].FontVPosition[j] = tempFolders[i].FontVPosition[j];
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
        Gf.FitTextFileParagraphs = cbFitTextFileParagraphs.Checked;
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
        string outputName = DualMonitorList.SelectedItem as string ?? Gf.OutputMonitorName;
        string lyricsName = LyricsMonitorList.SelectedItem as string ?? Gf.LyricsMonitorName;
        int outputMode = optDM0.Checked ? 0 : 1;
        int lyricsMode = optLM0.Checked ? 0 : 1;
        bool wide = optWide.Checked;

        Gf.Options_DMChanged = Gf.Options_DMChanged || Gf.isScreenWideMode != wide ||
            Gf.OutputMonitorName != outputName || Gf.LyricsMonitorName != lyricsName ||
            Gf.DMAlwaysUseSecondaryMonitor != DM_AlwaysUseSecondaryMonitor.Checked ||
            Gf.LMAlwaysUseSecondaryMonitor != LM_AlwaysUse.Checked ||
            Gf.DualMonitorSelectAutoOption != outputMode || Gf.LMSelectAutoOption != lyricsMode ||
            Gf.DMOption1Top != (int)DM1UpDownTop.Value || Gf.DMOption1Left != (int)DM1UpDownLeft.Value ||
            Gf.DMOption1Width != (int)DM1UpDownWidth.Value || Gf.DMOption1AsSingleMonitor != DM_CustomAsSingleMonitor.Checked ||
            Gf.LMOption1Top != (int)LM1UpDownTop.Value || Gf.LMOption1Left != (int)LM1UpDownLeft.Value ||
            Gf.LMOption1Width != (int)LM1UpDownWidth.Value;

        Gf.isScreenWideMode = wide;
        Gf.OutputMonitorName = outputName;
        Gf.DMAlwaysUseSecondaryMonitor = DM_AlwaysUseSecondaryMonitor.Checked;
        Gf.DualMonitorSelectAutoOption = outputMode;
        Gf.DMOption1Top = (int)DM1UpDownTop.Value;
        Gf.DMOption1Left = (int)DM1UpDownLeft.Value;
        Gf.DMOption1Width = (int)DM1UpDownWidth.Value;
        Gf.DMOption1Height = MonitorLayout.HeightForWidth(Gf.DMOption1Width, wide);
        Gf.DMOption1AsSingleMonitor = DM_CustomAsSingleMonitor.Checked;

        Gf.LyricsMonitorName = lyricsName;
        Gf.LMAlwaysUseSecondaryMonitor = LM_AlwaysUse.Checked;
        Gf.LMSelectAutoOption = lyricsMode;
        Gf.LMOption1Top = (int)LM1UpDownTop.Value;
        Gf.LMOption1Left = (int)LM1UpDownLeft.Value;
        Gf.LMOption1Width = (int)LM1UpDownWidth.Value;
        Gf.LMOption1Height = MonitorLayout.HeightForWidth(Gf.LMOption1Width, wide);
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
        ListViewItem listViewItem = new();
        SongFolder.Items.Clear();
        toolStripJumpA.Items.Clear();
        toolStripJumpB.Items.Clear();
        toolStripJumpC.Items.Clear();
        for (int i = 1; i < Gf.MAXSONGSFOLDERS; i++)
        {
            listViewItem = SongFolder.Items.Add(tempFolders[i].Name);
            if (tempFolders[i].Use > 0)
            {
                listViewItem.ImageIndex = 25;
            }
            else
            {
                listViewItem.ImageIndex = 26;
            }
            toolStripJumpA.Items.Add(tempFolders[i].Name);
            toolStripJumpB.Items.Add(tempFolders[i].Name);
            toolStripJumpC.Items.Add(tempFolders[i].Name);
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
        cbFolderUse.Checked = tempFolders[CurFolder].Use > 0;
        ApplySettings();
    }

    private void Apply_FolderUse()
    {
        tempFolders[CurFolder].Use = (cbFolderUse.Checked ? 1 : 0);
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
            SongFolder.Items[CurFolder - 1].ImageIndex = ((tempFolders[CurFolder].Use > 0) ? 25 : 26);
            ShowLineSpacingMaxUpDown.Value = (decimal)(((tempFolders[CurFolder].LineSpacing[0] >= 0.5) & (tempFolders[CurFolder].LineSpacing[0] <= 3.0)) ? tempFolders[CurFolder].LineSpacing[0] : 1.0);
            ShowLineSpacing2MaxUpDown.Value = (decimal)(((tempFolders[CurFolder].LineSpacing[1] >= 0.5) & (tempFolders[CurFolder].LineSpacing[1] <= 3.0)) ? tempFolders[CurFolder].LineSpacing[1] : 1.0);
            tbLyricsHeading0.Text = tempFolders[CurFolder].LyricsHeading[0];
            tbLyricsHeading1.Text = tempFolders[CurFolder].LyricsHeading[1];
            tbLyricsHeading2.Text = tempFolders[CurFolder].LyricsHeading[2];
            tbLyricsHeading3.Text = tempFolders[CurFolder].LyricsHeading[3];
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
            SelectedFolderGroupBox.Text = "Settings for the " + tempFolders[CurFolder].Name + " Folder";
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
            Gf.UpdatePosUpDowns(ref FontPositionUpDown0, ref FontPositionUpDown1, ref FontPositionUpDownBottom, ref tempFolders[CurFolder].FontVPosition[0], ref tempFolders[CurFolder].FontVPosition[1], tempFolders[CurFolder].BottomMargin);
            LeftMarginUpDown.Value = tempFolders[CurFolder].LeftMargin;
            RightMarginUpDown.Value = tempFolders[CurFolder].RightMargin;
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
        ShowHeadingsPercentSizeUpDown.Value = (((tempFolders[CurFolder].HeadingPercentSize >= 0) & (tempFolders[CurFolder].HeadingPercentSize <= 150)) ? tempFolders[CurFolder].HeadingPercentSize : 100);
        if (tempFolders[CurFolder].HeadingOption == 2)
        {
            ComboLyricsHeading.SelectedIndex = 0;
        }
        else if (tempFolders[CurFolder].HeadingOption == 1)
        {
            ComboLyricsHeading.SelectedIndex = 1;
        }
        else
        {
            ComboLyricsHeading.SelectedIndex = 2;
        }
        HeadingsFont_Bold.Checked = tempFolders[CurFolder].HeadingFontBold[0] > 0;
        Gf.AssignDropDownItem(SelectedMenuItemName: ((tempFolders[CurFolder].HeadingFontItalic[0] > 0) && (tempFolders[CurFolder].HeadingFontItalic[1] > 0)) ? HeadingsFont_Italics1.Name : ((!(tempFolders[CurFolder].HeadingFontItalic[1] > 0)) ? HeadingsFont_Italics0.Name : HeadingsFont_Italics2.Name), SelectedBtn: ref HeadingsFont_Italics, InMenuItem1: HeadingsFont_Italics0, InMenuItem2: HeadingsFont_Italics1, InMenuItem3: HeadingsFont_Italics2);
        HeadingsFont_Underline.Checked = tempFolders[CurFolder].HeadingFontUnderline[0] > 0;
    }

    private void UpdateFontsFields()
    {
        ComboFontName0.Text = tempFolders[CurFolder].FontName[0];
        ComboFontName1.Text = tempFolders[CurFolder].FontName[1];
        FontSizeUpDown0.Value = tempFolders[CurFolder].FontSize[0];
        FontSizeUpDown1.Value = tempFolders[CurFolder].FontSize[1];
        ToolBarFont_R1Bold.Checked = tempFolders[CurFolder].FontBold[0] > 0;
        Gf.AssignDropDownItem(SelectedMenuItemName: ((tempFolders[CurFolder].FontItalic[0] > 0) && (tempFolders[CurFolder].FontItalic[2] > 0)) ? ToolBarFont_R1Italics1.Name : ((!(tempFolders[CurFolder].FontItalic[2] > 0)) ? ToolBarFont_R1Italics0.Name : ToolBarFont_R1Italics2.Name), SelectedBtn: ref ToolBarFont_R1Italics, InMenuItem1: ToolBarFont_R1Italics0, InMenuItem2: ToolBarFont_R1Italics1, InMenuItem3: ToolBarFont_R1Italics2);
        ToolBarFont_R1Underline.Checked = tempFolders[CurFolder].FontUnderline[0] > 0;
        ToolBarFont_R1RTL.Checked = tempFolders[CurFolder].FontRTL[0] > 0;
        ToolBarFont_R2Bold.Checked = tempFolders[CurFolder].FontBold[1] > 0;
        Gf.AssignDropDownItem(SelectedMenuItemName: ((tempFolders[CurFolder].FontItalic[1] > 0) && (tempFolders[CurFolder].FontItalic[3] > 0)) ? ToolBarFont_R2Italics1.Name : ((!(tempFolders[CurFolder].FontItalic[3] > 0)) ? ToolBarFont_R2Italics0.Name : ToolBarFont_R2Italics2.Name), SelectedBtn: ref ToolBarFont_R2Italics, InMenuItem1: ToolBarFont_R2Italics0, InMenuItem2: ToolBarFont_R2Italics1, InMenuItem3: ToolBarFont_R2Italics2);
        ToolBarFont_R2Underline.Checked = tempFolders[CurFolder].FontUnderline[1] > 0;
        ToolBarFont_R2RTL.Checked = tempFolders[CurFolder].FontRTL[1] > 0;
        UpdateFontDisplay();
    }

    private void HeadingsFont_Click(object sender, EventArgs e)
    {
        tempFolders[CurFolder].HeadingFontBold[0] = (HeadingsFont_Bold.Checked ? 1 : 0);
        tempFolders[CurFolder].HeadingFontUnderline[0] = (HeadingsFont_Underline.Checked ? 1 : 0);
    }

    private void HeadingsFont_Italics_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
    {
        Gf.AssignDropDownItem(ref HeadingsFont_Italics, e.ClickedItem.Name, HeadingsFont_Italics0, HeadingsFont_Italics1, HeadingsFont_Italics2);
        int italicMode = DataUtil.ObjToInt(HeadingsFont_Italics.Tag);
        if (italicMode == 2)
        {
            tempFolders[CurFolder].HeadingFontItalic[0] = 0;
            tempFolders[CurFolder].HeadingFontItalic[1] = 1;
        }
        else
        {
            tempFolders[CurFolder].HeadingFontItalic[0] = ((italicMode > 0) ? 1 : 0);
            tempFolders[CurFolder].HeadingFontItalic[1] = ((italicMode > 0) ? 1 : 0);
        }
    }

    private void ComboFontName0_SelectedIndexChanged(object sender, EventArgs e)
    {
        tempFolders[CurFolder].FontName[0] = ComboFontName0.Text;
        UpdateFontDisplay();
    }

    private void ComboFontName1_SelectedIndexChanged(object sender, EventArgs e)
    {
        tempFolders[CurFolder].FontName[1] = ComboFontName1.Text;
        UpdateFontDisplay();
    }

    private void tbLyricsHeading_TextChanged(object sender, EventArgs e)
    {
        if (!InitFormLoad)
        {
            tempFolders[CurFolder].LyricsHeading[0] = tbLyricsHeading0.Text;
            tempFolders[CurFolder].LyricsHeading[1] = tbLyricsHeading1.Text;
            tempFolders[CurFolder].LyricsHeading[2] = tbLyricsHeading2.Text;
            tempFolders[CurFolder].LyricsHeading[3] = tbLyricsHeading3.Text;
        }
    }

    private void ComboLyricsHeading_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (!InitFormLoad)
        {
            if (ComboLyricsHeading.SelectedIndex == 0)
            {
                tempFolders[CurFolder].HeadingOption = 0;
            }
            else if (ComboLyricsHeading.SelectedIndex == 1)
            {
                tempFolders[CurFolder].HeadingOption = 1;
            }
            else if (ComboLyricsHeading.SelectedIndex == 2)
            {
                tempFolders[CurFolder].HeadingOption = 2;
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
            tempFolders[CurFolder].BottomMargin = (int)FontPositionUpDownBottom.Value;
            ApplySettings();
        }
    }

    private void LeftRightMarginUpDown_ValueChanged(object sender, EventArgs e)
    {
        if (!LoadTempPos)
        {
            tempFolders[CurFolder].LeftMargin = (int)LeftMarginUpDown.Value;
            tempFolders[CurFolder].RightMargin = (int)RightMarginUpDown.Value;
            ApplySettings();
        }
    }

    private void UpdateRegionsPosTempValues(int InArea)
    {
        if (InArea != 0 && InArea == 1 && FontPositionUpDown1.Value < FontPositionUpDown0.Value)
        {
            FontPositionUpDown1.Value = FontPositionUpDown0.Value;
        }
        tempFolders[CurFolder].FontVPosition[0] = (int)FontPositionUpDown0.Value;
        tempShowFontVPositionMax[CurFolder, 0] = (int)FontPositionUpDown1.Value;
        tempFolders[CurFolder].FontVPosition[1] = (int)FontPositionUpDown1.Value;
        tempShowFontVPositionMin[CurFolder, 0] = (int)FontPositionUpDown0.Value;
        ApplySettings();
    }

    private void cbFolderUse_CheckedChanged(object sender, EventArgs e)
    {
        Apply_FolderUse();
    }

    private void FontSizeUpDown0_ValueChanged(object sender, EventArgs e)
    {
        tempFolders[CurFolder].FontSize[0] = (int)FontSizeUpDown0.Value;
    }

    private void FontSizeUpDown1_ValueChanged(object sender, EventArgs e)
    {
        tempFolders[CurFolder].FontSize[1] = (int)FontSizeUpDown1.Value;
    }

    private void ToolBarFont_R1_Click(object sender, EventArgs e)
    {
        ToolStripButton toolStripButton = (ToolStripButton)sender;
        bool @checked = toolStripButton.Checked;
        string name = toolStripButton.Name;
        if (name == "ToolBarFont_R1Bold")
        {
            tempFolders[CurFolder].FontBold[0] = (@checked ? 1 : 0);
        }
        else if (name == "ToolBarFont_R1Underline")
        {
            tempFolders[CurFolder].FontUnderline[0] = (@checked ? 1 : 0);
        }
        else if (name == "ToolBarFont_R1RTL")
        {
            tempFolders[CurFolder].FontRTL[0] = (@checked ? 1 : 0);
        }
        UpdateFontDisplay();
    }

    private void ToolBarFont_R1Italics_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
    {
        Gf.AssignDropDownItem(ref ToolBarFont_R1Italics, e.ClickedItem.Name, ToolBarFont_R1Italics0, ToolBarFont_R1Italics1, ToolBarFont_R1Italics2);
        int italicMode = DataUtil.ObjToInt(ToolBarFont_R1Italics.Tag);
        if (italicMode == 2)
        {
            tempFolders[CurFolder].FontItalic[0] = 0;
            tempFolders[CurFolder].FontItalic[2] = 1;
        }
        else
        {
            tempFolders[CurFolder].FontItalic[0] = ((italicMode > 0) ? 1 : 0);
            tempFolders[CurFolder].FontItalic[2] = ((italicMode > 0) ? 1 : 0);
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
            tempFolders[CurFolder].FontBold[1] = (@checked ? 1 : 0);
        }
        else if (name == "ToolBarFont_R2Underline")
        {
            tempFolders[CurFolder].FontUnderline[1] = (@checked ? 1 : 0);
        }
        else if (name == "ToolBarFont_R2RTL")
        {
            tempFolders[CurFolder].FontRTL[1] = (@checked ? 1 : 0);
        }
        UpdateFontDisplay();
    }

    private void ToolBarFont_R2Italics_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
    {
        Gf.AssignDropDownItem(ref ToolBarFont_R2Italics, e.ClickedItem.Name, ToolBarFont_R2Italics0, ToolBarFont_R2Italics1, ToolBarFont_R2Italics2);
        int italicMode = DataUtil.ObjToInt(ToolBarFont_R2Italics.Tag);
        if (italicMode == 2)
        {
            tempFolders[CurFolder].FontItalic[1] = 0;
            tempFolders[CurFolder].FontItalic[3] = 1;
        }
        else
        {
            tempFolders[CurFolder].FontItalic[1] = ((italicMode > 0) ? 1 : 0);
            tempFolders[CurFolder].FontItalic[3] = ((italicMode > 0) ? 1 : 0);
        }
        UpdateFontDisplay();
    }

    private void UpdateFontDisplay()
    {
        try
        {
            FontStyle fontStyle = FontStyle.Regular;
            if (tempFolders[CurFolder].FontBold[0] > 0)
            {
                fontStyle |= FontStyle.Bold;
            }
            if ((tempFolders[CurFolder].FontItalic[0] > 0) || (tempFolders[CurFolder].FontItalic[2] > 0))
            {
                fontStyle |= FontStyle.Italic;
            }
            if (tempFolders[CurFolder].FontUnderline[0] > 0)
            {
                fontStyle |= FontStyle.Underline;
            }
            labelPreviewCentreTop.Font = new Font(tempFolders[CurFolder].FontName[0], 11f, fontStyle);
            fontStyle = FontStyle.Regular;
            if (tempFolders[CurFolder].FontBold[1] > 0)
            {
                fontStyle |= FontStyle.Bold;
            }
            if ((tempFolders[CurFolder].FontItalic[1] > 0) || (tempFolders[CurFolder].FontItalic[3] > 0))
            {
                fontStyle |= FontStyle.Italic;
            }
            if (tempFolders[CurFolder].FontUnderline[1] > 0)
            {
                fontStyle |= FontStyle.Underline;
            }
            labelPreviewCentreBottom.Font = new Font(tempFolders[CurFolder].FontName[1], 11f, fontStyle);
        }
        catch
        {
        }
    }

    private void ShowLineSpacingMaxUpDown_ValueChanged(object sender, EventArgs e)
    {
        tempFolders[CurFolder].LineSpacing[0] = (double)ShowLineSpacingMaxUpDown.Value;
    }

    private void ShowLineSpacing2MaxUpDown_ValueChanged(object sender, EventArgs e)
    {
        tempFolders[CurFolder].LineSpacing[1] = (double)ShowLineSpacing2MaxUpDown.Value;
    }

    private bool ValidateFolderRename(string NewFolderName, int SelectedItem)
    {
        for (int i = 0; i <= SongFolder.Items.Count - 1; i++)
        {
            if (SelectedItem != i && string.Equals(SongFolder.Items[i].Text, NewFolderName, StringComparison.OrdinalIgnoreCase))
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
                tempFolders[selectedIndex + 1].Name = label;
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
        originalBibleVersions = null;
        BibleList.Items.Clear();
        try
        {
            originalBibleVersions = BibleVersionCatalog.Load(Gf.ConnectStringBibleDB);
            foreach (var version in originalBibleVersions)
                AddBibleRow(version);
            if (BibleList.Items.Count > 0)
                BibleList.Items[0].Selected = true;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }

    private void AddBibleRow(BibleVersion version)
    {
        bool available = File.Exists(Gf.BibleDir + version.FileName);
        string description = available ? version.Description : "** Cannot find Bible - please check Filename!";
        BibleList.Items.Add(new ListViewItem([version.Name, description, Path.GetFileName(version.FileName),
            version.Copyright, version.SongFolder.ToString(), version.FontSize.ToString()], available ? 4 : 27)
            { Tag = version });
    }

    private void BuildBibleAssociatedFolder()
    {
        BibleAssociatedFolder.Items.Clear();
        for (int i = 1; i < Gf.MAXSONGSFOLDERS; i++)
        {
            BibleAssociatedFolder.Items.Add(tempFolders[i].Name);
        }
        if (BibleAssociatedFolder.Items.Count > 0)
        {
            BibleAssociatedFolder.SelectedIndex = 0;
        }
    }

    private void btnBibleSearch_Click(object sender, EventArgs e)
    {
        Cursor = Cursors.WaitCursor;
        BibleSearchList.Items.Clear();
        var listedFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (ListViewItem row in BibleList.Items)
            listedFiles.Add(((BibleVersion)row.Tag).FileName);
        try
        {
            foreach (string path in FindUnlistedBibleFiles(Gf.BibleDir, listedFiles))
            {
                string name = "", description = "", copyright = "", info = "";
                if (Gf.LookUpBibleName(path, ref name, ref description, ref copyright, ref info))
                {
                    BibleSearchList.Items.Add(new ListViewItem([name, description, Path.GetFileName(path), copyright], 4));
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
        finally
        {
            Cursor = Cursors.Default;
        }
    }

    internal static IEnumerable<string> FindUnlistedBibleFiles(string directory, HashSet<string> listedFiles)
    {
        foreach (string path in Directory.EnumerateFiles(directory))
        {
            string extension = Path.GetExtension(path);
            if ((extension.Equals(".db", StringComparison.OrdinalIgnoreCase) ||
                 extension.Equals(".mdb", StringComparison.OrdinalIgnoreCase)) && !listedFiles.Contains(Path.GetFileName(path)))
                yield return path;
        }
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
        for (int index = BibleSearchList.Items.Count - 1; index >= 0; index--)
        {
            var result = BibleSearchList.Items[index];
            if (result.Selected)
            {
                AddBibleRow(new(null, result.Text, result.SubItems[2].Text, result.SubItems[1].Text,
                    result.SubItems[3].Text, 1, 80));
                result.Remove();
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
        for (int bibleIndex = BibleList.Items.Count - 1; bibleIndex >= 0; bibleIndex--)
        {
            if (BibleList.Items[bibleIndex].Selected)
            {
                BibleList.Items[bibleIndex].Remove();
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
            MoveSelectedBible(-1);
        }
        else if (name == "Bibles_Down")
        {
            MoveSelectedBible(1);
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
            string fileName = Gf.BibleDir + ((BibleVersion)BibleList.Items[selectedIndex].Tag).FileName;
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

    private void MoveSelectedBible(int offset)
    {
        int index = Gf.GetSelectedIndex(BibleList);
        int destination = index + offset;
        if (index < 0 || destination < 0 || destination >= BibleList.Items.Count)
            return;

        var item = BibleList.Items[index];
        item.Remove();
        BibleList.Items.Insert(destination, item);
        item.Selected = true;
        item.Focused = true;
        item.EnsureVisible();
        Gf.Options_BibleListChanged = true;
    }

    public void SaveBibleChanges()
    {
        if (originalBibleVersions == null)
            throw new InvalidOperationException("Reopen Options to reload the Bible list before saving.");
        var edited = new List<BibleVersion>(BibleList.Items.Count);
        foreach (ListViewItem item in BibleList.Items)
            edited.Add((BibleVersion)item.Tag);
        originalBibleVersions.Save(Gf.ConnectStringBibleDB, edited);
    }

    private void btnBibleNameChange_Click(object sender, EventArgs e)
    {
        int selectedIndex = Gf.GetSelectedIndex(BibleList);
        if (selectedIndex < 0)
        {
            return;
        }
        var item = BibleList.Items[selectedIndex];
        var otherNames = new List<string>(BibleList.Items.Count - 1);
        foreach (ListViewItem other in BibleList.Items)
        {
            if (other != item) otherNames.Add(other.Text);
        }
        using var dialog = new FrmBibleRename(item.Text, otherNames);
        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            item.Tag = ((BibleVersion)item.Tag) with { Name = dialog.BibleName };
            item.Text = dialog.BibleName;
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
            var version = (BibleVersion)BibleList.Items[selectedIndex].Tag;
            bool wasLoading = InitFormLoad;
            InitFormLoad = true;
            BibleAssociatedFolder.SelectedIndex = version.SongFolder - 1;
            BibleFontSizeUpDown.Value = version.FontSize;
            InitFormLoad = wasLoading;
        }
    }

    private void BibleAssociatedFolder_SelectedIndexChanged(object sender, EventArgs e)
    {
        int index = Gf.GetSelectedIndex(BibleList);
        if (InitFormLoad || index < 0 || BibleAssociatedFolder.SelectedIndex < 0)
            return;
        var item = BibleList.Items[index];
        int folder = BibleAssociatedFolder.SelectedIndex + 1;
        item.Tag = ((BibleVersion)item.Tag) with { SongFolder = folder };
        item.SubItems[4].Text = folder.ToString();
        Gf.Options_BibleListChanged = true;
    }

    private void BibleFontSizeUpDown_ValueChanged(object sender, EventArgs e)
    {
        int index = Gf.GetSelectedIndex(BibleList);
        if (InitFormLoad || index < 0)
            return;
        var item = BibleList.Items[index];
        int size = (int)BibleFontSizeUpDown.Value;
        item.Tag = ((BibleVersion)item.Tag) with { FontSize = size };
        item.SubItems[5].Text = size.ToString();
        Gf.Options_BibleListChanged = true;
    }

    private void BuildLicencesList()
    {
        AdminLic4.Text = Gf.LicenseAdministrators[4].Name;
        AdminLic5.Text = Gf.LicenseAdministrators[5].Name;
        AdminLic6.Text = Gf.LicenseAdministrators[6].Name;
        AdminLic7.Text = Gf.LicenseAdministrators[7].Name;
        AdminLic8.Text = Gf.LicenseAdministrators[8].Name;
        AdminLicNo3.Text = Gf.LicenseAdministrators[3].Number;
        AdminLicNo4.Text = Gf.LicenseAdministrators[4].Number;
        AdminLicNo5.Text = Gf.LicenseAdministrators[5].Number;
        AdminLicNo6.Text = Gf.LicenseAdministrators[6].Number;
        AdminLicNo7.Text = Gf.LicenseAdministrators[7].Number;
        AdminLicNo8.Text = Gf.LicenseAdministrators[8].Number;
        AdminLicPreview3.Text = Gf.LicenseAdministrators[3].DisplayText;
        AdminLicPreview4.Text = Gf.LicenseAdministrators[4].DisplayText;
        AdminLicPreview5.Text = Gf.LicenseAdministrators[5].DisplayText;
        AdminLicPreview6.Text = Gf.LicenseAdministrators[6].DisplayText;
        AdminLicPreview7.Text = Gf.LicenseAdministrators[7].DisplayText;
        AdminLicPreview8.Text = Gf.LicenseAdministrators[8].DisplayText;
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
        DM_CustomAsSingleMonitor.Checked = Gf.DMOption1AsSingleMonitor;
        DualMonitorList.SelectedIndex = DualMonitorList.FindStringExact(Gf.OutputMonitorName);
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
        UpdateCustomMonitorHeights();
        LyricsMonitorList.SelectedIndex = LyricsMonitorList.FindStringExact(Gf.LyricsMonitorName);
        btnLMTextColour.ForeColor = Gf.LMTextColour;
        btnLMHighlightColour.ForeColor = Gf.LMHighlightColour;
        btnLMBackColour.ForeColor = Gf.LMBackColour;
        PreviewFontUpDown.Value = ((Gf.PreviewArea_FontSize >= 8) ? Gf.PreviewArea_FontSize : 8);
        LMUpDownFontSize.Value = ((Gf.LMMainFontSize is >= 8 and <= 40) ? Gf.LMMainFontSize : 20);
        LMNotationsUpDownFontSize.Value = ((Gf.LMNotationsFontSize is >= 8 and <= 40) ? Gf.LMNotationsFontSize : 20);
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
        cbFitTextFileParagraphs.Checked = Gf.FitTextFileParagraphs;
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
                DShowLib.ListCaptureDevices(ref cbCaptureDevices);
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

    private void DM1UpDownWidth_ValueChanged(object sender, EventArgs e) => UpdateCustomMonitorHeights();

    private void LM1UpDownWidth_ValueChanged(object sender, EventArgs e) => UpdateCustomMonitorHeights();

    private void UpdateCustomMonitorHeights()
    {
        DM1UpDownHeight.Value = MonitorLayout.HeightForWidth((int)DM1UpDownWidth.Value, optWide.Checked);
        LM1UpDownHeight.Value = MonitorLayout.HeightForWidth((int)LM1UpDownWidth.Value, optWide.Checked);
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

        DualMonitorList.Items.Add(MonitorLayout.Auto);
        foreach (var display in DisplayInfo.GetDisplays())
        {
            DualMonitorList.Items.Add(display.Name);
            if (!display.IsPrimary)
                LyricsMonitorList.Items.Add(display.Name);
        }
        DualMonitorList.Items.Add(MonitorLayout.None);
        LyricsMonitorList.Items.Add(MonitorLayout.None);

        // 연결이 끊긴 설정도 보존하여 다시 연결되면 사용한다.
        AddSavedMonitor(DualMonitorList, Gf.OutputMonitorName);
        AddSavedMonitor(LyricsMonitorList, Gf.LyricsMonitorName);
    }

    private static void AddSavedMonitor(ToolStripComboBox list, string name)
    {
        if (!string.IsNullOrWhiteSpace(name) && list.FindStringExact(name) < 0)
            list.Items.Add(name);
    }

    private void Monitor_Info_Click(object sender, EventArgs e)
    {
        var displays = DisplayInfo.GetDisplays();
        string name = DualMonitorList.SelectedItem as string;
        if (name == MonitorLayout.Auto)
            name = MonitorLayout.ResolveOutput(displays, name, DM_AlwaysUseSecondaryMonitor.Checked, optWide.Checked).DeviceName;
        ShowMonitorInfo(MonitorLayout.FindDisplay(displays, name), "No Output Monitor");
    }

    private void LyricsMonitor_Info_Click(object sender, EventArgs e)
    {
        var display = MonitorLayout.FindDisplay(DisplayInfo.GetDisplays(), LyricsMonitorList.SelectedItem as string);
        ShowMonitorInfo(display is { IsPrimary: false } ? display : null, "No Lyrics Monitor");
    }

    private static void ShowMonitorInfo(MonitorDevice display, string missingMessage)
    {
        if (display == null)
        {
            MessageBox.Show(missingMessage);
            return;
        }
        Rectangle bounds = display.Bounds;
        MessageBox.Show($"{display.Name} Setup:\nTop:\t{bounds.Top}\nLeft:\t{bounds.Left}\nWidth:\t{bounds.Width}\nHeight:\t{bounds.Height}");
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
        tempFolders[CurFolder].HeadingPercentSize = (int)ShowHeadingsPercentSizeUpDown.Value;
    }

    private void rbHeadingFontSettings_CheckedChanged(object sender, EventArgs e)
    {
        Control control = (Control)sender;
        if (Convert.ToInt32(control.Tag) == 0)
        {
            tempFolders[CurFolder].HeadingOption = 0;
        }
        else if (Convert.ToInt32(control.Tag) == 1)
        {
            tempFolders[CurFolder].HeadingOption = 1;
        }
        else if (Convert.ToInt32(control.Tag) == 2)
        {
            tempFolders[CurFolder].HeadingOption = 2;
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
        string initialDirectory = (tbGapLogoLocation.Text != "") ? Path.GetDirectoryName(tbGapLogoLocation.Text) : "";
        if (initialDirectory == "")
        {
            initialDirectory = Gf.ImagesDir;
        }
        OpenFileDialog1.Filter = "Image Files (*.jpg,*.jpeg,*.bmp,*.gif,*.ico)|*.jpg;*.jpeg;*.bmp;*.gif;*.ico";
        OpenFileDialog1.InitialDirectory = initialDirectory;
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
        int sampleWidth = panelVideoHolder.Width * (int)VideoSizeUpDown1.Value / 100;
        int sampleHeight = sampleWidth * 3 / 4;
        int sampleLeft = (panelVideoHolder.Width - sampleWidth) / 2 - 1;
        sampleLeft = ((sampleLeft >= 0) ? sampleLeft : 0);
        panelVideoSize.Left = sampleLeft;
        panelVideoSize.Width = sampleWidth;
        panelVideoSize.Height = sampleHeight;
        var sampleTop = DataUtil.ObjToInt(Video_VAlign.Tag) switch
        {
            2 => panelVideoHolder.Height - sampleHeight,
            1 => (panelVideoHolder.Height - sampleHeight) / 2,
            _ => 0,
        };
        int top = sampleTop;
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

    private void optWide_CheckedChanged(object sender, EventArgs e) => UpdateCustomMonitorHeights();

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
