using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Easislides.Module;
using Easislides.Properties;
using Easislides.Util;

namespace Easislides
{
	public class FrmGenerateDoc : Form
	{
		private enum DocGenType
		{
			Index,
			TitlesRef,
			Complete
		}

		private string[] FList = new string[82];

		private int[] SongFolderLog = new int[32000];

		private int[,] FolderFNum = new int[41, 2];

		private int CurrentSong;

		private int CurSlide;

		private bool ChorusDone;

		private bool BibleItem;

		private bool ShowFirstLineOnly;

		private string[] Verse = new string[160];

		private int SongFolderNo2;

		private int[] ListViewVerseIndex = new int[160];

		private int MaxTextWidth;

		private string RTFLineandNotationsSpacing;

		private string DummyNotationSym = '\u0001'.ToString();

		private string[] ColoursList = new string[6];

		private RichTextBox RichTextBox1 = new RichTextBox();

		private SongSettings DocItem = new SongSettings();

		private ListView SubDivideList = new ListView();

		private int InTransposeOffset = 0;

		private IContainer components = null;

		private GroupBox groupBox5;

		private GroupBox groupBox7;

		private GroupBox groupBox6;

		private GroupBox groupBox3;

		private Button BtnSaveExit;

		private Button BtnCancel;

		private Button BtnOK;

		private Label Mess1;

		private ProgressBar ProgressBar1;

		private GroupBox groupBox2;

		private GroupBox groupBox1;

		private Panel panel1;

		private NumericUpDown tbFontSize0;

		private Label label1;

		private ToolStrip toolStripFont0;

		private ToolStripButton toolStripButton100;

		private ToolStripButton toolStripButton101;

		private ToolStripButton toolStripButton102;

		private Panel panel5;

		private NumericUpDown tbFontSize4;

		private Label label5;

		private ToolStrip toolStripFont4;

		private ToolStripButton toolStripButton14;

		private ToolStripButton toolStripButton15;

		private ToolStripButton toolStripButton16;

		private Panel panel4;

		private NumericUpDown tbFontSize3;

		private Label label4;

		private ToolStrip toolStripFont3;

		private ToolStripButton toolStripButton10;

		private ToolStripButton toolStripButton11;

		private ToolStripButton toolStripButton12;

		private Panel panel3;

		private NumericUpDown tbFontSize2;

		private Label label3;

		private ToolStrip toolStripFont2;

		private ToolStripButton toolStripButton6;

		private ToolStripButton toolStripButton7;

		private ToolStripButton toolStripButton8;

		private Panel panel2;

		private NumericUpDown tbFontSize1;

		private Label label2;

		private ToolStrip toolStripFont1;

		private ToolStripButton toolStripButton2;

		private ToolStripButton toolStripButton3;

		private ToolStripButton toolStripButton4;

		private CheckBox optWords6;

		private CheckBox optWords2;

		private CheckBox optWords1;

		private CheckBox optWords0;

		private CheckBox optPrinterSpaces;

		private CheckBox optShowCapo;

		private CheckBox optShowKey;

		private CheckBox optWords5;

		private RadioButton OptShowSection1;

		private RadioButton OptShowSection0;

		private RadioButton OptShowSection2;

		private CheckBox optWords7;

		private CheckBox optHeadings2;

		private CheckBox optHeadings1;

		private CheckBox optHeadings0;

		private RadioButton OptShowColumns2;

		private RadioButton OptShowColumns1;

		private RadioButton OptLyricsPattern1;

		private RadioButton OptLyricsPattern0;

		private Panel panel6;

		private NumericUpDown tbSpacing1;

		private Label label6;

		private Panel panel7;

		private NumericUpDown tbSpacing0;

		private Label label7;

		private CheckBox optCapoZero;

		private CheckBox optShowTiming;

		private CheckBox optNewScreen;

		private Panel panel8;

		private NumericUpDown tbFontSize5;

		private Label label8;

		private ToolStrip toolStripFont5;

		private ToolStripButton toolStripButton1;

		private ToolStripButton toolStripButton5;

		private ToolStripButton toolStripButton9;

		private ToolTip toolTip1;

		private Panel PanelFontColour1;

		private Panel PanelFontColour5;

		private Panel PanelFontColour4;

		private Panel PanelFontColour3;

		private Panel PanelFontColour2;

		private Panel PanelFontColour0;

		private CheckBox optOneSongPerPage;

		private Button BtnIndexOnly;

		private Button BtnTitlesRef;

		private CheckBox optHeadings3;

		private GroupBox groupBox4;

		private RadioButton OptPageSize1;

		private RadioButton OptPageSize0;

		public FrmGenerateDoc()
		{
			InitializeComponent();
		}

		private void FrmFormatPraiseBookDoc_Load(object sender, EventArgs e)
		{
			UpdateFontFields(ref toolStripFont0, ref tbFontSize0, ref PanelFontColour0, gf.PB_WordsBold[0], gf.PB_WordsItalic[0], gf.PB_WordsUnderline[0], gf.PB_WordsSize[0], gf.PB_WordsColour[0]);
			UpdateFontFields(ref toolStripFont1, ref tbFontSize1, ref PanelFontColour1, gf.PB_WordsBold[1], gf.PB_WordsItalic[1], gf.PB_WordsUnderline[1], gf.PB_WordsSize[1], gf.PB_WordsColour[1]);
			UpdateFontFields(ref toolStripFont2, ref tbFontSize2, ref PanelFontColour2, gf.PB_WordsBold[2], gf.PB_WordsItalic[2], gf.PB_WordsUnderline[2], gf.PB_WordsSize[2], gf.PB_WordsColour[2]);
			UpdateFontFields(ref toolStripFont3, ref tbFontSize3, ref PanelFontColour3, gf.PB_WordsBold[3], gf.PB_WordsItalic[3], gf.PB_WordsUnderline[3], gf.PB_WordsSize[3], gf.PB_WordsColour[3]);
			UpdateFontFields(ref toolStripFont4, ref tbFontSize4, ref PanelFontColour4, gf.PB_WordsBold[4], gf.PB_WordsItalic[4], gf.PB_WordsUnderline[4], gf.PB_WordsSize[4], gf.PB_WordsColour[4]);
			UpdateFontFields(ref toolStripFont5, ref tbFontSize5, ref PanelFontColour5, gf.PB_WordsBold[5], gf.PB_WordsItalic[5], gf.PB_WordsUnderline[5], gf.PB_WordsSize[5], gf.PB_WordsColour[5]);
			optWords0.Checked = ((gf.PB_ShowWords[0] == 1) ? true : false);
			optWords1.Checked = ((gf.PB_ShowWords[1] == 1) ? true : false);
			optWords2.Checked = ((gf.PB_ShowWords[2] == 1) ? true : false);
			optWords6.Checked = ((gf.PB_ShowWords[6] == 1) ? true : false);
			optWords7.Checked = ((gf.PB_ShowWords[7] == 1) ? true : false);
			optHeadings0.Checked = ((gf.PB_ShowHeadings[0] == 1) ? true : false);
			optHeadings1.Checked = ((gf.PB_ShowHeadings[1] == 1) ? true : false);
			optHeadings2.Checked = ((gf.PB_ShowHeadings[2] == 1) ? true : false);
			optHeadings3.Checked = ((gf.PB_ShowHeadings[3] == 1) ? true : false);
			if (gf.PB_ShowSection == 0)
			{
				OptShowSection0.Checked = true;
			}
			else if (gf.PB_ShowSection == 1)
			{
				OptShowSection1.Checked = true;
			}
			else if (gf.PB_ShowSection == 2)
			{
				OptShowSection2.Checked = true;
			}
			if (gf.PB_ShowColumns == 1)
			{
				OptShowColumns1.Checked = true;
			}
			else if (gf.PB_ShowColumns == 2)
			{
				OptShowColumns2.Checked = true;
			}
			if (gf.PB_LyricsPattern == 0)
			{
				OptLyricsPattern0.Checked = true;
			}
			else
			{
				OptLyricsPattern1.Checked = true;
			}
			if (gf.PB_PageSize == 1)
			{
				OptPageSize1.Checked = true;
			}
			else
			{
				OptPageSize0.Checked = true;
			}
			optPrinterSpaces.Checked = ((gf.PB_PrinterSpaces > 0) ? true : false);
			optWords5.Checked = ((gf.PB_ShowNotations == 1) ? true : false);
			optShowTiming.Checked = ((gf.PB_ShowTiming == 1) ? true : false);
			optShowKey.Checked = ((gf.PB_ShowKey == 1) ? true : false);
			optShowCapo.Checked = ((gf.PB_ShowCapo == 1) ? true : false);
			optCapoZero.Checked = ((gf.PB_CapoZero == 1) ? true : false);
			tbSpacing0.Minimum = 0m;
			tbSpacing0.Maximum = 5m;
			tbSpacing0.Value = gf.PB_Spacing[0];
			tbSpacing1.Minimum = 1m;
			tbSpacing1.Maximum = 20m;
			tbSpacing1.Value = gf.PB_Spacing[1];
			optNewScreen.Checked = ((gf.PB_ShowScreenBreaks == 1) ? true : false);
			optOneSongPerPage.Checked = ((gf.PB_OneSongPerPage == 1) ? true : false);
			Mess1.Text = "NOTE - Generate will overwrite: " + gf.PB_FullFileName;
			DocItem.Initialise();
			DocItem.SplitScreens = false;
			gf.SetListViewColumns(SubDivideList, 6);
			gf.PB_FormatChanged = false;
			InitRTF();
		}

		private void InitRTF()
		{
			gf.RTFNewLine = "\\b0\\i0\\ulnone\\par ";
			gf.RTFTabValue[0] = 0;
			gf.RTFTabValue[1] = 500;
			gf.RTFTabValue[2] = 1000;
			gf.RTFTabValue[3] = 4400;
			gf.RTFIndent[0] = "\\pard\\fi-" + Convert.ToString(gf.RTFTabValue[0] + 400) + "\\li" + Convert.ToString(gf.RTFTabValue[0] + 400);
			gf.RTFIndent[1] = "\\pard\\fi-" + Convert.ToString(gf.RTFTabValue[1] + 400) + "\\li" + Convert.ToString(gf.RTFTabValue[1] + 400) + "\\tx" + Convert.ToString(gf.RTFTabValue[1]);
			gf.RTFIndent[2] = "\\pard\\fi-" + Convert.ToString(gf.RTFTabValue[2] + 400) + "\\li" + Convert.ToString(gf.RTFTabValue[2] + 400) + "\\tx" + Convert.ToString(gf.RTFTabValue[2]);
			if (gf.PB_ShowColumns == 1)
			{
				MaxTextWidth = gf.RTFTabValue[3] + 4500;
				gf.RTFIndent[3] = "\\pard\\fi-" + Convert.ToString(400) + "\\li" + Convert.ToString(400) + "\\ri680\\tqr\\tx" + Convert.ToString(MaxTextWidth);
			}
			else
			{
				MaxTextWidth = gf.RTFTabValue[3];
				gf.RTFIndent[3] = "\\pard\\fi-" + Convert.ToString(400) + "\\li" + Convert.ToString(400) + "\\ri680\\tqr\\tx" + Convert.ToString(MaxTextWidth);
			}
			gf.RTFIndent[4] = "\\pard\\fi-" + Convert.ToString(gf.RTFTabValue[1] + 400) + "\\li" + Convert.ToString(gf.RTFTabValue[1] + 400) + "\\tx" + Convert.ToString(gf.RTFTabValue[1]);
			gf.RTFIndent[5] = "\\pard\\fi-" + Convert.ToString(gf.RTFTabValue[1]) + "\\li" + Convert.ToString(gf.RTFTabValue[1]) + "\\tx" + Convert.ToString(gf.RTFTabValue[1]);
			RichTextBox1.Height = 0;
			RichTextBox1.Top = 0;
		}

		private void UpdateFontFields(ref ToolStrip InToolBar, ref NumericUpDown InUpDown, ref Panel InPanel, int b, int i, int u, int fsize, Color InColour)
		{
			((ToolStripButton)InToolBar.Items[0]).Checked = ((b == 1) ? true : false);
			((ToolStripButton)InToolBar.Items[1]).Checked = ((i == 1) ? true : false);
			((ToolStripButton)InToolBar.Items[2]).Checked = ((u == 1) ? true : false);
			InUpDown.Minimum = 4m;
			InUpDown.Maximum = 72m;
			InUpDown.Value = fsize;
			InPanel.BackColor = InColour;
		}

		private void UpdateFontInfo(ToolStrip InToolBar, NumericUpDown InUpDown, Panel InPanel, ref int b, ref int i, ref int u, ref int fsize, ref Color InColour)
		{
			b = (((ToolStripButton)InToolBar.Items[0]).Checked ? 1 : 0);
			i = (((ToolStripButton)InToolBar.Items[1]).Checked ? 1 : 0);
			u = (((ToolStripButton)InToolBar.Items[2]).Checked ? 1 : 0);
			fsize = (int)InUpDown.Value;
			InColour = InPanel.BackColor;
		}

		private void UpdatePBFormat()
		{
			UpdateFontInfo(toolStripFont0, tbFontSize0, PanelFontColour0, ref gf.PB_WordsBold[0], ref gf.PB_WordsItalic[0], ref gf.PB_WordsUnderline[0], ref gf.PB_WordsSize[0], ref gf.PB_WordsColour[0]);
			UpdateFontInfo(toolStripFont1, tbFontSize1, PanelFontColour1, ref gf.PB_WordsBold[1], ref gf.PB_WordsItalic[1], ref gf.PB_WordsUnderline[1], ref gf.PB_WordsSize[1], ref gf.PB_WordsColour[1]);
			UpdateFontInfo(toolStripFont2, tbFontSize2, PanelFontColour2, ref gf.PB_WordsBold[2], ref gf.PB_WordsItalic[2], ref gf.PB_WordsUnderline[2], ref gf.PB_WordsSize[2], ref gf.PB_WordsColour[2]);
			UpdateFontInfo(toolStripFont3, tbFontSize3, PanelFontColour3, ref gf.PB_WordsBold[3], ref gf.PB_WordsItalic[3], ref gf.PB_WordsUnderline[3], ref gf.PB_WordsSize[3], ref gf.PB_WordsColour[3]);
			UpdateFontInfo(toolStripFont4, tbFontSize4, PanelFontColour4, ref gf.PB_WordsBold[4], ref gf.PB_WordsItalic[4], ref gf.PB_WordsUnderline[4], ref gf.PB_WordsSize[4], ref gf.PB_WordsColour[4]);
			UpdateFontInfo(toolStripFont5, tbFontSize5, PanelFontColour5, ref gf.PB_WordsBold[5], ref gf.PB_WordsItalic[5], ref gf.PB_WordsUnderline[5], ref gf.PB_WordsSize[5], ref gf.PB_WordsColour[5]);
			UpdateFontInfo(toolStripFont2, tbFontSize2, PanelFontColour2, ref gf.PB_WordsBold[6], ref gf.PB_WordsItalic[6], ref gf.PB_WordsUnderline[6], ref gf.PB_WordsSize[6], ref gf.PB_WordsColour[6]);
			UpdateFontInfo(toolStripFont2, tbFontSize2, PanelFontColour2, ref gf.PB_WordsBold[7], ref gf.PB_WordsItalic[7], ref gf.PB_WordsUnderline[7], ref gf.PB_WordsSize[7], ref gf.PB_WordsColour[7]);
			gf.PB_ShowWords[0] = (optWords0.Checked ? 1 : 0);
			gf.PB_ShowWords[1] = (optWords1.Checked ? 1 : 0);
			gf.PB_ShowWords[2] = (optWords2.Checked ? 1 : 0);
			gf.PB_ShowWords[6] = (optWords6.Checked ? 1 : 0);
			gf.PB_ShowWords[7] = (optWords7.Checked ? 1 : 0);
			gf.PB_ShowHeadings[0] = (optHeadings0.Checked ? 1 : 0);
			gf.PB_ShowHeadings[1] = (optHeadings1.Checked ? 1 : 0);
			gf.PB_ShowHeadings[2] = (optHeadings2.Checked ? 1 : 0);
			gf.PB_ShowHeadings[3] = (optHeadings3.Checked ? 1 : 0);
			if (OptShowSection1.Checked)
			{
				gf.PB_ShowSection = 1;
			}
			else if (OptShowSection2.Checked)
			{
				gf.PB_ShowSection = 2;
			}
			else
			{
				gf.PB_ShowSection = 0;
			}
			gf.PB_ShowColumns = (OptShowColumns1.Checked ? 1 : 2);
			gf.PB_LyricsPattern = ((!OptLyricsPattern0.Checked) ? 1 : 0);
			gf.PB_PageSize = ((!OptPageSize0.Checked) ? 1 : 0);
			gf.PB_ShowScreenBreaks = (optNewScreen.Checked ? 1 : 0);
			gf.PB_OneSongPerPage = (optOneSongPerPage.Checked ? 1 : 0);
			gf.PB_Spacing[0] = (int)tbSpacing0.Value;
			gf.PB_Spacing[1] = (int)tbSpacing1.Value;
			gf.PB_PrinterSpaces = (optPrinterSpaces.Checked ? 1 : 0);
			RegUtil.SaveRegValue("options", "PrinterSpaces", gf.PB_PrinterSpaces);
			gf.PB_ShowNotations = (optWords5.Checked ? 1 : 0);
			gf.PB_ShowTiming = (optShowTiming.Checked ? 1 : 0);
			gf.PB_ShowKey = (optShowKey.Checked ? 1 : 0);
			gf.PB_ShowCapo = (optShowCapo.Checked ? 1 : 0);
			gf.PB_CapoZero = (optCapoZero.Checked ? 1 : 0);
			gf.PB_FormatChanged = true;
		}

		private void PanelFontColour_Click(object sender, EventArgs e)
		{
			Panel panel = (Panel)sender;
			Color black = Color.Black;
			switch (DataUtil.ObjToInt(panel.Tag))
			{
			case 0:
				PanelFontColour0.BackColor = gf.SelectNewColour(PanelFontColour0.BackColor);
				break;
			case 1:
				PanelFontColour1.BackColor = gf.SelectNewColour(PanelFontColour1.BackColor);
				break;
			case 2:
				PanelFontColour2.BackColor = gf.SelectNewColour(PanelFontColour2.BackColor);
				break;
			case 3:
				PanelFontColour3.BackColor = gf.SelectNewColour(PanelFontColour3.BackColor);
				break;
			case 4:
				PanelFontColour4.BackColor = gf.SelectNewColour(PanelFontColour4.BackColor);
				break;
			case 5:
				PanelFontColour5.BackColor = gf.SelectNewColour(PanelFontColour5.BackColor);
				break;
			}
		}

		private void BtnIndexOnly_Click(object sender, EventArgs e)
		{
			BtnStartPressed(DocGenType.Index);
		}

		private void BtnTitlesRef_Click(object sender, EventArgs e)
		{
			BtnStartPressed(DocGenType.TitlesRef);
		}

		private void BtnOK_Click(object sender, EventArgs e)
		{
			BtnStartPressed(DocGenType.Complete);
		}

		private void BtnStartPressed(DocGenType GenType)
		{
			UpdatePBFormat();
			if (StartGeneration(GenType))
			{
				gf.RunProcess(gf.PB_FullFileName);
			}
			Cursor = Cursors.Default;
			ProgressBar1.Value = 0;
			Mess1.Visible = true;
		}

		private void BtnSaveExit_Click(object sender, EventArgs e)
		{
			UpdatePBFormat();
			Close();
		}

		private bool StartGeneration(DocGenType GenType)
		{
			try
			{
				Cursor = Cursors.WaitCursor;
				InitRTF();
				using StreamWriter streamWriter = new StreamWriter(gf.PB_FullFileName, append: false, FileUtil.Utf8WithBom);
				try
				{
					streamWriter.AutoFlush = true;

					Mess1.Visible = false;
					ProgressBar1.Value = 0;
					ProgressBar1.Invalidate();
					int formatOption = 0;
					int num = 0;
					string text = "";
					int num2 = 0;
					string text2 = "";
					int num3 = 0;
					gf.RTFLineSpacing = "";
					RTFLineandNotationsSpacing = "";
					for (int i = 0; i <= gf.PB_Spacing[0]; i++)
					{
						gf.RTFLineSpacing += gf.RTFNewLine;
						RTFLineandNotationsSpacing += "\n";
					}
					streamWriter.Write(BuildRTFHeader());
					if (GenType == DocGenType.Complete || GenType == DocGenType.TitlesRef)
					{
						for (int i = 0; i < gf.TotalPraiseBookItems; i++)
						{
							num2 = (i + 1) * 100 / gf.TotalPraiseBookItems;
							ProgressBar1.Value = ((num2 > 100) ? 100 : num2);
							Invalidate();
							CurrentSong = i + 1;
							LoadItem(ref DocItem);
							SongFolderLog[i] = DocItem.FolderNo;
							string text3 = "";
							if (gf.PB_ShowWords[0] > 0)
							{
								text = DataUtil.ExtractOneInfo(ref gf.DocumentSongs[CurrentSong, 4], '>', RemoveExtract: false, MinusOneIfBlank: false);
								text3 = ((!(text == "")) ? text : Convert.ToString(i + 1));
							}
							if (gf.PB_ShowWords[1] > 0)
							{
								text3 = ((gf.PB_ShowWords[0] <= 0) ? (gf.FormatMode(1) + DocItem.Title) : ($"{text3}.{gf.FormatMode(1)} {DocItem.Title}"));
							}
							if ((gf.PB_ShowWords[2] > 0) | (gf.PB_ShowWords[6] > 0) | (gf.PB_ShowWords[7] > 0))
							{
								if ((gf.PB_ShowWords[0] > 0) & (gf.PB_ShowWords[1] > 0))
								{
									streamWriter.Write(AddtoRTF(text3, 0, 0, 4, 0, 0));
								}
								else if ((gf.PB_ShowWords[0] < 1) & (gf.PB_ShowWords[1] > 0))
								{
									streamWriter.Write(AddtoRTF(text3, 0, 0, 4, 0, 0));
								}
								else if ((gf.PB_ShowWords[0] > 0) & (gf.PB_ShowWords[1] < 1))
								{
									text3 = $"{text3}.{gf.FormatMode(2)} {DocItem.Copyright} {DocItem.Book_Reference} {DocItem.User_Reference}";
									streamWriter.Write(AddtoRTF(text3, 0, 0, 4, 0, 0));
								}
								if ((gf.PB_ShowWords[2] > 0) & (DocItem.Copyright != ""))
								{
									streamWriter.Write(AddtoRTF(DocItem.Copyright, 0, 2, 0, 0, 0));
								}
								if ((gf.PB_ShowWords[6] > 0) & (DocItem.Book_Reference != ""))
								{
									streamWriter.Write(AddtoRTF(DocItem.Book_Reference, 0, 2, 0, 0, 0));
								}
								if ((gf.PB_ShowWords[7] > 0) & (DocItem.User_Reference != ""))
								{
									streamWriter.Write(AddtoRTF(DocItem.User_Reference, 0, 2, 0, 0, 0));
								}
							}
							else if (text3.Length > 0)
							{
								streamWriter.Write(AddtoRTF(text3, 0, 0, 4, 0, 0));
							}
							num3 = DocItem.Capo;
							text2 = DocItem.MusicKey;
							InTransposeOffset = 0;
							if (gf.PB_CapoZero > 0)
							{
								if (DocItem.MusicKey != "" && num3 >= 0)
								{
									text2 = DocItem.MusicKey;
									gf.TransposeKey(ref text2, gf.IncrementChord(ref num3, 0));
								}
							}
							else
							{
								string InString = gf.DocumentSongs[CurrentSong, 4];
								DataUtil.ExtractOneInfo(ref InString, '>');
								InTransposeOffset = DataUtil.StringToInt(DataUtil.ExtractOneInfo(ref InString, '>', RemoveExtract: false, MinusOneIfBlank: false), Minus1IfBlank: false);
								if (InTransposeOffset != 0)
								{
									int CurPos = 0;
									string InKey = DocItem.MusicKey;
									gf.TransposeKey(ref InKey, gf.IncrementChord(ref CurPos, InTransposeOffset));
								}
							}
							string text4 = ((gf.PB_ShowKey != 1) ? "" : ((text2 != "") ? ("Key: " + text2 + " ") : "")) + ((!((gf.PB_ShowCapo == 1) & (gf.PB_CapoZero < 1))) ? "" : ((num3 > 0) ? ("Capo " + Convert.ToString(num3) + " ") : "")) + ((gf.PB_ShowTiming != 1) ? "" : ((DocItem.Timing != "") ? ("(" + DocItem.Timing + ")") : ""));
							if (text4 != "")
							{
								streamWriter.Write(AddtoRTF(text4, 0, 3, 0, 0, 0));
							}
							if (GenType == DocGenType.Complete)
							{
								int num4 = 0;
								for (int j = 1; j <= DocItem.TotalSlides; j++)
								{
									if (DocItem.Slide[j, 0] == 0)
									{
										num4 = j;
										num = 1;
									}
									if ((num4 > 0) & (DocItem.Slide[j, 0] < 0))
									{
										num++;
									}
								}
								int tabNum = 0;
								ChorusDone = false;
								for (num4 = 1; num4 <= DocItem.TotalSlides; num4++)
								{
									bool flag = false;
									ShowFirstLineOnly = false;
									CurSlide = num4;
									string text5 = "";
									if (DocItem.Slide[CurSlide, 0] == 0)
									{
										if (gf.PB_ShowHeadings[1] > 0)
										{
											tabNum = 2;
											if (!ChorusDone)
											{
												text5 = gf.FolderLyricsHeading[DocItem.FolderNo, 1];
												ChorusDone = true;
											}
											else
											{
												ShowFirstLineOnly = true;
											}
										}
										else if (gf.PB_ShowHeadings[0] > 0)
										{
											tabNum = ((!((gf.PB_ShowHeadings[0] > 0) & DocItem.VersePresent[2])) ? 1 : 2);
											if (!ChorusDone)
											{
												ChorusDone = true;
											}
											else
											{
												ShowFirstLineOnly = true;
											}
										}
										else
										{
											tabNum = 1;
											if (!ChorusDone)
											{
												ChorusDone = true;
											}
											else
											{
												ShowFirstLineOnly = true;
											}
										}
										formatOption = 4;
										flag = true;
									}
									else if (DocItem.Slide[CurSlide, 0] == 102)
									{
										if (gf.PB_ShowHeadings[1] <= 0)
										{
											tabNum = ((gf.PB_ShowHeadings[0] <= 0) ? 1 : ((!((gf.PB_ShowHeadings[0] > 0) & DocItem.VersePresent[2])) ? 1 : 2));
										}
										else
										{
											tabNum = 2;
											text5 = gf.FolderLyricsHeading[DocItem.FolderNo, 1] + ((gf.FolderLyricsHeading[DocItem.FolderNo, 1] != "") ? " (2)" : "");
										}
										formatOption = 4;
										flag = true;
									}
									else if (DocItem.Slide[CurSlide, 0] == 111)
									{
										if (gf.PB_ShowHeadings[3] <= 0)
										{
											tabNum = ((gf.PB_ShowHeadings[0] <= 0) ? 1 : ((!((gf.PB_ShowHeadings[0] > 0) & DocItem.VersePresent[2])) ? 1 : 2));
										}
										else
										{
											tabNum = 2;
											text5 = gf.FolderLyricsHeading[DocItem.FolderNo, 0];
										}
										formatOption = 4;
										flag = true;
									}
									else if (DocItem.Slide[CurSlide, 0] == 112)
									{
										if (gf.PB_ShowHeadings[3] <= 0)
										{
											tabNum = ((gf.PB_ShowHeadings[0] <= 0) ? 1 : ((!((gf.PB_ShowHeadings[0] > 0) & DocItem.VersePresent[2])) ? 1 : 2));
										}
										else
										{
											tabNum = 2;
											text5 = gf.FolderLyricsHeading[DocItem.FolderNo, 0] + ((gf.FolderLyricsHeading[DocItem.FolderNo, 0] != "") ? " (2)" : "");
										}
										formatOption = 4;
										flag = true;
									}
									else if (DocItem.Slide[CurSlide, 0] == 100)
									{
										if (gf.PB_ShowHeadings[2] <= 0)
										{
											tabNum = ((gf.PB_ShowHeadings[0] <= 0) ? 1 : ((!((gf.PB_ShowHeadings[0] > 0) & DocItem.VersePresent[2])) ? 1 : 2));
										}
										else
										{
											text5 = gf.FolderLyricsHeading[DocItem.FolderNo, 2];
											tabNum = 2;
										}
										formatOption = 4;
										flag = true;
									}
									else if (DocItem.Slide[CurSlide, 0] == 103)
									{
										if (gf.PB_ShowHeadings[2] <= 0)
										{
											tabNum = ((gf.PB_ShowHeadings[0] <= 0) ? 1 : ((!((gf.PB_ShowHeadings[0] > 0) & DocItem.VersePresent[2])) ? 1 : 2));
										}
										else
										{
											text5 = gf.FolderLyricsHeading[DocItem.FolderNo, 2] + ((gf.FolderLyricsHeading[DocItem.FolderNo, 2] != "") ? " (2)" : "");
											tabNum = 2;
										}
										formatOption = 4;
										flag = true;
									}
									else if (DocItem.Slide[CurSlide, 0] > 0)
									{
										if ((gf.PB_ShowHeadings[0] > 0) & DocItem.VersePresent[2])
										{
											if (DocItem.Slide[CurSlide, 0] != 101)
											{
												text5 = gf.VerseTitle[DocItem.Slide[CurSlide, 0]];
											}
											tabNum = 1;
										}
										else
										{
											tabNum = 0;
										}
										formatOption = 3;
										flag = true;
									}
									if (flag)
									{
										if (text5.Length > 0)
										{
											text5 += "\t";
										}
										if (!ShowFirstLineOnly)
										{
											streamWriter.Write(AddtoRTF("", 0, formatOption, 0, 0, 0));
										}
									}
									else if (gf.PB_ShowScreenBreaks > 0)
									{
										streamWriter.Write(AddtoRTF("", 0, formatOption, 0, 0, gf.PB_Spacing[0]));
									}
									if (gf.PB_ShowSection == 0)
									{
										if (DocItem.Slide[CurSlide, 2] >= 0)
										{
											streamWriter.Write(AddtoRTF(text5, 0, formatOption, tabNum, text5.Length, gf.PB_Spacing[0], DocItem.Slide[CurSlide, 1], DocItem.Slide[CurSlide, 2]));
										}
									}
									else if (gf.PB_ShowSection == 1)
									{
										if (DocItem.Slide[CurSlide, 4] >= 0)
										{
											streamWriter.Write(AddtoRTF(text5, 0, formatOption, tabNum, text5.Length, gf.PB_Spacing[0], DocItem.Slide[CurSlide, 3], DocItem.Slide[CurSlide, 4]));
										}
									}
									else if (gf.PB_ShowSection == 2)
									{
										bool flag2 = false;
										if (DocItem.Slide[CurSlide, 2] >= 0)
										{
											streamWriter.Write(AddtoRTF(text5, 0, formatOption, tabNum, text5.Length, gf.PB_Spacing[0], DocItem.Slide[CurSlide, 1], DocItem.Slide[CurSlide, 2]));
											flag2 = true;
										}
										if (DocItem.Slide[CurSlide, 4] >= 0)
										{
											streamWriter.Write(AddtoRTF((!flag2) ? text5 : "", 1, formatOption, tabNum, (!flag2) ? text5.Length : 0, gf.PB_Spacing[0], DocItem.Slide[CurSlide, 3], DocItem.Slide[CurSlide, 4]));
										}
									}
									if (ShowFirstLineOnly)
									{
										num4 += num - 1;
									}
								}
							}
							streamWriter.Write(AddtoRTF("", 0, formatOption, 0, 0, gf.PB_Spacing[1] - 1));
							if (gf.PB_OneSongPerPage > 0)
							{
								streamWriter.Write("\\page ");
							}
						}
					}
					if (gf.PB_Layout == PraiseBookLayout.PraiseBook)
					{
						if (gf.PB_OneSongPerPage < 1 && GenType == DocGenType.Complete)
						{
							streamWriter.Write("\\page ");
						}
					}
					else if (GenType == DocGenType.Complete)
					{
						streamWriter.Write(AddtoRTF("", 0, 3, 0, 0, 0));
						streamWriter.Write(AddtoRTF("", 0, 3, 0, 0, 0));
					}
					if (GenType == DocGenType.Complete || GenType == DocGenType.Index)
					{
						DocItem.FolderNo = SongFolderLog[0];
						streamWriter.Write(AddtoRTF("INDEX", 0, 0, 4, 0, 0));
						streamWriter.Write(AddtoRTF(gf.RTFIndent[3] + "\\tab", 0, 3, 0, 0, 0));
						if (DataUtil.Left(gf.DocumentSongs[1, 3], 1) != " ")
						{
							streamWriter.Write(AddtoRTF(Convert.ToString(Convert.ToInt32(gf.DocumentSongs[1, 3])) + gf.RTFIndent[3] + "\\tab", 0, 3, 0, 0, 0));
						}
						string inString = ((gf.DocumentSongs[1, 2].Length < 4) ? (gf.DocumentSongs[1, 2] + " ") : gf.DocumentSongs[1, 2]) + gf.RTFIndent[3] + "\\tab " + ((DataUtil.ExtractOneInfo(ref gf.DocumentSongs[1, 4], '>', RemoveExtract: false, MinusOneIfBlank: false) == "") ? "1" : DataUtil.ExtractOneInfo(ref gf.DocumentSongs[1, 4], '>', RemoveExtract: false, MinusOneIfBlank: false));
						streamWriter.Write(AddtoRTF(inString, 0, 3, 0, 0, 0));
						for (int i = 2; i <= gf.TotalPraiseBookItems; i++)
						{
							if (DataUtil.ExtractOneInfo(ref gf.DocumentSongs[i, 4], '>', RemoveExtract: false, MinusOneIfBlank: false) == "" && gf.DocumentSongs[i, 3] != gf.DocumentSongs[i - 1, 3])
							{
								if (DataUtil.Left(gf.DocumentSongs[i, 3], 1) == " ")
								{
									streamWriter.Write(AddtoRTF(gf.RTFIndent[3] + "\\tab", 0, 3, 0, 0, 0));
								}
								else if (Convert.ToInt32(gf.DocumentSongs[i, 3]) < 17)
								{
									streamWriter.Write(AddtoRTF(gf.RTFIndent[3] + "\\tab", 0, 3, 0, 0, 0));
									streamWriter.Write(AddtoRTF(Convert.ToString(Convert.ToInt32(gf.DocumentSongs[i, 3])) + ((Convert.ToInt32(gf.DocumentSongs[i, 3]) == 16) ? "+" : "") + gf.RTFIndent[3] + "\\tab", 0, 3, 0, 0, 0));
								}
							}
							DocItem.FolderNo = SongFolderLog[i - 1];
							inString = ((gf.DocumentSongs[i, 2].Length < 4) ? (gf.DocumentSongs[i, 2] + " ") : gf.DocumentSongs[i, 2]) + gf.RTFIndent[3] + "\\tab " + ((DataUtil.ExtractOneInfo(ref gf.DocumentSongs[1, 4], '>', RemoveExtract: false, MinusOneIfBlank: false) == "") ? i.ToString() : DataUtil.ExtractOneInfo(ref gf.DocumentSongs[i, 4], '>', RemoveExtract: false, MinusOneIfBlank: false));
							streamWriter.Write(AddtoRTF(inString, 0, 3, 0, 0, 0));
						}
					}
					ProgressBar1.Value = 100;
					streamWriter.Write("}");
					//streamWriter.Flush();
					//streamWriter.Close();
					return true;
				}
				catch
				{
					//streamWriter?.Flush();
					//streamWriter?.Close();
					MessageBox.Show("Error generating document " + gf.PB_FullFileName + ". The document might be opened - please close it first!");
					return false;
				}
			}
			catch
			{
				MessageBox.Show("Error generating document " + gf.PB_FullFileName + ". The document might be opened - please close it first!");
				return false;
			}
		}

		private string BuildRTFHeader()
		{
			string str = OptPageSize0.Checked ? "\\pgwsxn11906\\pghsxn16838" : "\\pgwsxn12240\\pghsxn15840";
			string str2 = "{\\rtf1\\ansi" + str + "\\ansicpg1252\\deff0\\deflang1033{\\fonttbl{\\f0\\fnil\\fcharset0 Microsoft Sans Serif;}";
			string text = "";
			string str3 = "{\\colortbl ;";
			string str4 = "\\viewkind1\\uc1\\pard\\f0\\fs17 ";
			for (int i = 1; i <= 81; i++)
			{
				FList[i] = "";
			}
			FList[0] = gf.ShowFontName[1, 0];
			RichTextBox1.Text = "";
			int num = 1;
			for (int i = 1; i < 41; i++)
			{
				for (int j = 0; j <= 1; j++)
				{
					RichTextBox1.Focus();
					RichTextBox1.SelectionStart = RichTextBox1.Text.Length;
					RichTextBox1.SelectedText = ">";
					RichTextBox1.SelectionFont = new Font(gf.ShowFontName[i, j], 11f, FontStyle.Regular);
					bool flag = false;
					for (int k = 1; k <= num; k++)
					{
						if (FList[k] == gf.ShowFontName[i, j])
						{
							flag = true;
							FolderFNum[i, j] = k;
						}
					}
					if (!flag)
					{
						num++;
						FList[num] = gf.ShowFontName[i, j];
						FolderFNum[i, j] = num;
						string text2 = text;
						text = text2 + "{\\f" + Convert.ToString(num) + "\\fnil " + gf.ShowFontName[i, j] + ";}";
					}
				}
			}
			text += "}";
			for (int l = 0; l < 6; l++)
			{
				str3 = str3 + "\\red" + gf.PB_WordsColour[l].R;
				str3 = str3 + "\\green" + gf.PB_WordsColour[l].G;
				str3 = str3 + "\\blue" + gf.PB_WordsColour[l].B + ";";
			}
			str3 += "}";
			string str5 = str2 + text + str3 + str4;
			string str6 = "{\\footer\\fs16\\sectd\\footery400\\pard\\qr {\\i " + gf.PB_DocumentName + "\\par Document Generated by EasiSlides}{\\par }}";
			if (gf.PB_ShowColumns == 1)
			{
				return str5 + str6 + "\\margr900\\margl1300\\margt900\\margb1000";
			}
			return str5 + str6 + "\\margr900\\margl1300\\margt900\\margb1000\\cols2\\colno1\\colw4500\\colsr750\\colno2\\colw4500";
		}

		private string AddtoRTF(string InString, int Section, int FormatOption, int TabNum, int HeadingText, int PreLineSpacing)
		{
			return AddtoRTF(InString, Section, FormatOption, TabNum, HeadingText, PreLineSpacing, -1);
		}

		private string AddtoRTF(string InString, int Section, int FormatOption, int TabNum, int HeadingText, int PreLineSpacing, int StartLoc)
		{
			return AddtoRTF(InString, Section, FormatOption, TabNum, HeadingText, PreLineSpacing, StartLoc, -1);
		}

		private string AddtoRTF(string InString, int Section, int FormatOption, int TabNum, int HeadingText, int PreLineSpacing, int StartLoc, int EndLoc)
		{
			int num = DocItem.FolderNo;
			if (BibleItem && Section == 1)
			{
				num = SongFolderNo2;
				Section = 0;
			}
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
			if (InString == null)
			{
				InString = "";
			}
			text7 = gf.RTFIndent[TabNum];
			if (TabNum > 0 && TabNum < 4)
			{
				text3 = gf.RTFTab;
			}
			else if (TabNum == 5)
			{
				text3 = gf.RTFTab;
			}
			FontStyle fontStyle = FontStyle.Regular;
			if (gf.PB_WordsBold[FormatOption] > 0)
			{
				fontStyle |= FontStyle.Bold;
			}
			if (gf.PB_WordsItalic[FormatOption] > 0)
			{
				fontStyle |= FontStyle.Italic;
			}
			if (gf.PB_WordsUnderline[FormatOption] > 0)
			{
				fontStyle |= FontStyle.Underline;
			}
			int num2 = gf.PB_WordsSize[5];
			FontStyle style = fontStyle | FontStyle.Italic;
			Font mainFont = new Font(FList[FolderFNum[num, Section]], gf.PB_WordsSize[FormatOption], fontStyle);
			Font notationsFont = new Font(FList[FolderFNum[num, Section]], num2, style);
			text5 = "\\f" + Convert.ToString(FolderFNum[num, Section]);
			string text11 = "\\fs" + Convert.ToString(num2 * 2) + gf.FormatMode(5);
			bool flag = (gf.FormatMode(5).IndexOf("\\ul ") >= 0) ? true : false;
			int transposeTo = 0;
			if (DocItem.Capo > 0)
			{
				transposeTo = gf.IncrementChord(ref DocItem.Capo, 0);
			}
			if (PreLineSpacing > 0)
			{
				for (int i = 1; i <= PreLineSpacing; i++)
				{
					text5 += gf.RTFNewLine;
				}
			}
			if (HeadingText == 0)
			{
				text5 += text3;
			}
			text6 = gf.RTFIndent[TabNum];
			text6 += gf.FormatMode(FormatOption);
			text4 = "";
			text8 = "";
			string text12 = "";
			if (StartLoc >= 0 && EndLoc >= 0)
			{
				if (ShowFirstLineOnly)
				{
					EndLoc = StartLoc;
				}
				bool flag2 = false;
				for (int j = StartLoc; j <= EndLoc; j++)
				{
					if (DocItem.LyricsAndNotationsList.Items[j].SubItems[3].Text != "")
					{
						flag2 = true;
						j = EndLoc;
					}
				}
				for (int j = StartLoc; j <= EndLoc; j++)
				{
					text9 = DocItem.LyricsAndNotationsList.Items[j].SubItems[2].Text + (ShowFirstLineOnly ? " ..." : "");
					gf.SubstituteDashes(ref text9, gf.PB_ShowNotations);
					if (gf.PB_ShowNotations == 1)
					{
						text8 = DocItem.LyricsAndNotationsList.Items[j].SubItems[3].Text;
						if (flag2 & !ShowFirstLineOnly)
						{
							if (gf.PB_CapoZero == 1)
							{
								text8 = gf.TransposeOneNotationString(text8, transposeTo, -1);
							}
							else if (InTransposeOffset > 0)
							{
								text8 = gf.TransposeOneNotationString(text8, InTransposeOffset, -1);
							}
							gf.SubDivideTextAndNotations(text9, text8, mainFont, notationsFont, ref SubDivideList, MaxTextWidth - 400 - 200);
							InString += gf.FormatMode(FormatOption);
							for (int k = 0; k < SubDivideList.Items.Count; k++)
							{
								text12 = SubDivideList.Items[k].SubItems[1].Text;
								if (flag)
								{
									string text13 = "";
									for (int l = 0; l < text12.Length; l++)
									{
										if (text12[l] == ' ')
										{
											if (l < 1 || text12[l - 1] != ' ')
											{
												text13 += "\\ulnone ";
											}
										}
										else if (l > 0 && text12[l - 1] == ' ')
										{
											text13 += "\\ul ";
										}
										text13 += text12[l];
									}
									text12 = text13;
								}
								string text14 = InString;
								InString = text14 + text11 + text12 + "\n" + DummyNotationSym + gf.FormatMode(FormatOption) + SubDivideList.Items[k].SubItems[0].Text + text10 + "\n";
							}
							if (DataUtil.Right(InString, 1) == "\n")
							{
								InString = DataUtil.Left(InString, InString.Length - 1);
							}
							InString = InString + text10 + "\n";
						}
						else
						{
							string text14 = InString;
							InString = text14 + gf.FormatMode(FormatOption) + text9 + text10 + "\n";
						}
					}
					else
					{
						string text14 = InString;
						InString = text14 + ((text8 != "") ? (text8 + "\n") : "") + gf.FormatMode(FormatOption) + text9 + text10 + "\n";
					}
				}
				InString = DataUtil.TrimEnd(InString).Replace("\n", "\r\n");
			}
			for (int i = 0; i < InString.Length; i++)
			{
				text = DataUtil.Mid(InString, i, 1);
				if ((text != "\r") & (text != "\n"))
				{
					int num3 = (!(text == "")) ? text[0] : '\0';
					if (num3 < 0)
					{
						num3 += 65536;
					}
					text4 = ((num3 <= 255) ? (text4 + text) : (text4 + "\\u" + num3.ToString("00000") + "?"));
				}
				else
				{
					text4 += text;
				}
			}
			InString = text4;
			text4 = text6;
			for (int i = 0; i < InString.Length; i++)
			{
				text = DataUtil.Mid(InString, i, 1);
				if (text == "\r")
				{
					if (ShowFirstLineOnly)
					{
						text4 = text4 + "\r\n" + text6;
						i = InString.Length;
					}
					else if (i == InString.Length - 1)
					{
						text4 = text4 + "\r\n" + gf.RTFLineSpacing + text6;
					}
					else
					{
						string text15 = (DataUtil.Mid(InString, i + 2, 1) == DummyNotationSym) ? gf.RTFNewLine : gf.RTFLineSpacing;
						string text14 = text4;
						text4 = text14 + "\r\n" + text15 + text3 + text6;
					}
				}
				else if ((text != "\n") & (text != DummyNotationSym))
				{
					text4 += text;
				}
			}
			return text5 + text4 + "\r\n" + gf.RTFNewLine;
		}

		private void LoadItem(ref SongSettings InItem)
		{
			string text = gf.DocumentSongs[CurrentSong, 1];
			string inIDString = gf.DocumentSongs[CurrentSong, 0];
			BibleItem = false;
			gf.InitialiseIndividualData(ref InItem);
			if (text == "P")
			{
				InItem.Type = text;
				InItem.Title = gf.RTFCheck(gf.DocumentSongs[CurrentSong, 2]);
				InItem.CompleteLyrics = "(Powerpoint File)";
			}
			else if (text == "D")
			{
				gf.LoadIndividualData(ref InItem, inIDString, "", 0);
			}
			else if (text == "B")
			{
				string InTitle = gf.DocumentSongs[CurrentSong, 2];
				BibleItem = true;
				gf.LoadIndividualData(ref InItem, inIDString, "", 0, ref InTitle);
				InItem.CompleteLyrics = InItem.CompleteLyrics.Replace('\u0098'.ToString(), " ");
			}
			else if (text == "T")
			{
				string InTitle = gf.DocumentSongs[CurrentSong, 2];
				gf.LoadIndividualData(ref InItem, inIDString, "", 0, ref InTitle);
			}
			else if (text == "I")
			{
				string InTitle = gf.DocumentSongs[CurrentSong, 2];
				gf.LoadIndividualData(ref InItem, inIDString, "", 0, ref InTitle);
				InItem.Title = InTitle;
			}
			else if (text == "W")
			{
				string InTitle = gf.DocumentSongs[CurrentSong, 2];
				gf.LoadIndividualData(ref InItem, inIDString, "", 0, ref InTitle);
			}
			InItem.Title = gf.RTFCheck(InItem.Title);
			InItem.Title2 = gf.RTFCheck(InItem.Title);
			InItem.CompleteLyrics = gf.RTFCheck(InItem.CompleteLyrics);
			InItem.Copyright = gf.RTFCheck(InItem.Writer + (((InItem.Writer != "") & (InItem.Copyright != "")) ? "; " : "") + InItem.Copyright);
			SongSettings obj = InItem;
			string copyright = obj.Copyright;
			obj.Copyright = copyright + ((InItem.Copyright == "") ? "" : " ") + InItem.Show_LicAdminInfo1 + ((InItem.Show_LicAdminInfo1 == "") ? "" : " ") + InItem.Show_LicAdminInfo2;
			gf.FormatDisplayLyrics(ref InItem, PrepareSlides: true, (gf.PB_LyricsPattern > 0) ? true : false);
		}

		private string FormatNotationString(string InString, string InNotation, Font MainFont, Font NotationsFont)
		{
			Graphics graphics = CreateGraphics();
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
				text3 = DataUtil.Left(text3, text3.Length - 1) + text5 + text2;
				if (gf.PB_PrinterSpaces > 0)
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
				if (gf.PB_PrinterSpaces > 0)
				{
					return text4;
				}
				return text3;
			}
			return " ";
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
            ComponentResourceManager resources = new ComponentResourceManager(typeof(FrmGenerateDoc));
            groupBox5 = new GroupBox();
            OptShowColumns2 = new RadioButton();
            OptShowColumns1 = new RadioButton();
            groupBox7 = new GroupBox();
            optOneSongPerPage = new CheckBox();
            optNewScreen = new CheckBox();
            panel6 = new Panel();
            tbSpacing1 = new NumericUpDown();
            label6 = new Label();
            panel7 = new Panel();
            tbSpacing0 = new NumericUpDown();
            label7 = new Label();
            groupBox6 = new GroupBox();
            OptLyricsPattern1 = new RadioButton();
            OptLyricsPattern0 = new RadioButton();
            optCapoZero = new CheckBox();
            optShowTiming = new CheckBox();
            optPrinterSpaces = new CheckBox();
            optShowCapo = new CheckBox();
            optShowKey = new CheckBox();
            optWords5 = new CheckBox();
            groupBox3 = new GroupBox();
            OptShowSection1 = new RadioButton();
            OptShowSection0 = new RadioButton();
            OptShowSection2 = new RadioButton();
            BtnSaveExit = new Button();
            BtnCancel = new Button();
            BtnOK = new Button();
            Mess1 = new Label();
            ProgressBar1 = new ProgressBar();
            groupBox2 = new GroupBox();
            optHeadings3 = new CheckBox();
            optWords7 = new CheckBox();
            optHeadings2 = new CheckBox();
            optHeadings1 = new CheckBox();
            optHeadings0 = new CheckBox();
            optWords6 = new CheckBox();
            optWords2 = new CheckBox();
            optWords1 = new CheckBox();
            optWords0 = new CheckBox();
            groupBox1 = new GroupBox();
            panel8 = new Panel();
            PanelFontColour5 = new Panel();
            tbFontSize5 = new NumericUpDown();
            toolStripFont5 = new ToolStrip();
            toolStripButton1 = new ToolStripButton();
            toolStripButton5 = new ToolStripButton();
            toolStripButton9 = new ToolStripButton();
            label8 = new Label();
            panel5 = new Panel();
            PanelFontColour4 = new Panel();
            tbFontSize4 = new NumericUpDown();
            toolStripFont4 = new ToolStrip();
            toolStripButton14 = new ToolStripButton();
            toolStripButton15 = new ToolStripButton();
            toolStripButton16 = new ToolStripButton();
            label5 = new Label();
            panel4 = new Panel();
            PanelFontColour3 = new Panel();
            tbFontSize3 = new NumericUpDown();
            toolStripFont3 = new ToolStrip();
            toolStripButton10 = new ToolStripButton();
            toolStripButton11 = new ToolStripButton();
            toolStripButton12 = new ToolStripButton();
            label4 = new Label();
            panel3 = new Panel();
            PanelFontColour2 = new Panel();
            tbFontSize2 = new NumericUpDown();
            toolStripFont2 = new ToolStrip();
            toolStripButton6 = new ToolStripButton();
            toolStripButton7 = new ToolStripButton();
            toolStripButton8 = new ToolStripButton();
            label3 = new Label();
            panel2 = new Panel();
            PanelFontColour1 = new Panel();
            tbFontSize1 = new NumericUpDown();
            toolStripFont1 = new ToolStrip();
            toolStripButton2 = new ToolStripButton();
            toolStripButton3 = new ToolStripButton();
            toolStripButton4 = new ToolStripButton();
            label2 = new Label();
            panel1 = new Panel();
            PanelFontColour0 = new Panel();
            tbFontSize0 = new NumericUpDown();
            toolStripFont0 = new ToolStrip();
            toolStripButton100 = new ToolStripButton();
            toolStripButton101 = new ToolStripButton();
            toolStripButton102 = new ToolStripButton();
            label1 = new Label();
            toolTip1 = new ToolTip(components);
            BtnIndexOnly = new Button();
            BtnTitlesRef = new Button();
            groupBox4 = new GroupBox();
            OptPageSize1 = new RadioButton();
            OptPageSize0 = new RadioButton();
            groupBox5.SuspendLayout();
            groupBox7.SuspendLayout();
            panel6.SuspendLayout();
            ((ISupportInitialize)tbSpacing1).BeginInit();
            panel7.SuspendLayout();
            ((ISupportInitialize)tbSpacing0).BeginInit();
            groupBox6.SuspendLayout();
            groupBox3.SuspendLayout();
            groupBox2.SuspendLayout();
            groupBox1.SuspendLayout();
            panel8.SuspendLayout();
            ((ISupportInitialize)tbFontSize5).BeginInit();
            toolStripFont5.SuspendLayout();
            panel5.SuspendLayout();
            ((ISupportInitialize)tbFontSize4).BeginInit();
            toolStripFont4.SuspendLayout();
            panel4.SuspendLayout();
            ((ISupportInitialize)tbFontSize3).BeginInit();
            toolStripFont3.SuspendLayout();
            panel3.SuspendLayout();
            ((ISupportInitialize)tbFontSize2).BeginInit();
            toolStripFont2.SuspendLayout();
            panel2.SuspendLayout();
            ((ISupportInitialize)tbFontSize1).BeginInit();
            toolStripFont1.SuspendLayout();
            panel1.SuspendLayout();
            ((ISupportInitialize)tbFontSize0).BeginInit();
            toolStripFont0.SuspendLayout();
            groupBox4.SuspendLayout();
            SuspendLayout();
            // 
            // groupBox5
            // 
            groupBox5.Controls.Add(OptShowColumns2);
            groupBox5.Controls.Add(OptShowColumns1);
            groupBox5.Location = new Point(131, 352);
            groupBox5.Margin = new Padding(4, 5, 4, 5);
            groupBox5.Name = "groupBox5";
            groupBox5.Padding = new Padding(4, 5, 4, 5);
            groupBox5.Size = new Size(93, 98);
            groupBox5.TabIndex = 2;
            groupBox5.TabStop = false;
            groupBox5.Text = "Columns";
            // 
            // OptShowColumns2
            // 
            OptShowColumns2.AutoSize = true;
            OptShowColumns2.Location = new Point(8, 58);
            OptShowColumns2.Margin = new Padding(4, 5, 4, 5);
            OptShowColumns2.Name = "OptShowColumns2";
            OptShowColumns2.Size = new Size(79, 24);
            OptShowColumns2.TabIndex = 1;
            OptShowColumns2.Text = "Double";
            // 
            // OptShowColumns1
            // 
            OptShowColumns1.AutoSize = true;
            OptShowColumns1.Location = new Point(8, 28);
            OptShowColumns1.Margin = new Padding(4, 5, 4, 5);
            OptShowColumns1.Name = "OptShowColumns1";
            OptShowColumns1.Size = new Size(71, 24);
            OptShowColumns1.TabIndex = 0;
            OptShowColumns1.Text = "Single";
            // 
            // groupBox7
            // 
            groupBox7.Controls.Add(optOneSongPerPage);
            groupBox7.Controls.Add(optNewScreen);
            groupBox7.Controls.Add(panel6);
            groupBox7.Controls.Add(panel7);
            groupBox7.Location = new Point(321, 283);
            groupBox7.Margin = new Padding(4, 5, 4, 5);
            groupBox7.Name = "groupBox7";
            groupBox7.Padding = new Padding(4, 5, 4, 5);
            groupBox7.Size = new Size(296, 168);
            groupBox7.TabIndex = 5;
            groupBox7.TabStop = false;
            groupBox7.Text = "Line Spacing";
            // 
            // optOneSongPerPage
            // 
            optOneSongPerPage.AutoSize = true;
            optOneSongPerPage.BackColor = Color.Transparent;
            optOneSongPerPage.Location = new Point(15, 132);
            optOneSongPerPage.Margin = new Padding(4, 5, 4, 5);
            optOneSongPerPage.Name = "optOneSongPerPage";
            optOneSongPerPage.Size = new Size(158, 24);
            optOneSongPerPage.TabIndex = 1;
            optOneSongPerPage.Text = "One Song per Page";
            optOneSongPerPage.UseVisualStyleBackColor = false;
            // 
            // optNewScreen
            // 
            optNewScreen.AutoSize = true;
            optNewScreen.BackColor = Color.Transparent;
            optNewScreen.Location = new Point(15, 103);
            optNewScreen.Margin = new Padding(4, 5, 4, 5);
            optNewScreen.Name = "optNewScreen";
            optNewScreen.Size = new Size(273, 24);
            optNewScreen.TabIndex = 0;
            optNewScreen.Text = "One blank line for each Screen Break";
            optNewScreen.UseVisualStyleBackColor = false;
            // 
            // panel6
            // 
            panel6.Controls.Add(tbSpacing1);
            panel6.Controls.Add(label6);
            panel6.Location = new Point(13, 63);
            panel6.Margin = new Padding(4, 5, 4, 5);
            panel6.Name = "panel6";
            panel6.Size = new Size(272, 38);
            panel6.TabIndex = 3;
            // 
            // tbSpacing1
            // 
            tbSpacing1.Location = new Point(215, 5);
            tbSpacing1.Margin = new Padding(4, 5, 4, 5);
            tbSpacing1.Maximum = new decimal(new int[] { 99, 0, 0, 0 });
            tbSpacing1.Name = "tbSpacing1";
            tbSpacing1.Size = new Size(51, 27);
            tbSpacing1.TabIndex = 0;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(4, 8);
            label6.Margin = new Padding(4, 0, 4, 0);
            label6.Name = "label6";
            label6.Size = new Size(142, 20);
            label6.TabIndex = 0;
            label6.Text = "Between each Song:";
            // 
            // panel7
            // 
            panel7.Controls.Add(tbSpacing0);
            panel7.Controls.Add(label7);
            panel7.Location = new Point(13, 26);
            panel7.Margin = new Padding(4, 5, 4, 5);
            panel7.Name = "panel7";
            panel7.Size = new Size(272, 38);
            panel7.TabIndex = 2;
            // 
            // tbSpacing0
            // 
            tbSpacing0.Location = new Point(215, 5);
            tbSpacing0.Margin = new Padding(4, 5, 4, 5);
            tbSpacing0.Maximum = new decimal(new int[] { 99, 0, 0, 0 });
            tbSpacing0.Name = "tbSpacing0";
            tbSpacing0.Size = new Size(51, 27);
            tbSpacing0.TabIndex = 0;
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(4, 8);
            label7.Margin = new Padding(4, 0, 4, 0);
            label7.Name = "label7";
            label7.Size = new Size(135, 20);
            label7.TabIndex = 0;
            label7.Text = "Between each Line:";
            // 
            // groupBox6
            // 
            groupBox6.Controls.Add(OptLyricsPattern1);
            groupBox6.Controls.Add(OptLyricsPattern0);
            groupBox6.Location = new Point(9, 352);
            groupBox6.Margin = new Padding(4, 5, 4, 5);
            groupBox6.Name = "groupBox6";
            groupBox6.Padding = new Padding(4, 5, 4, 5);
            groupBox6.Size = new Size(115, 98);
            groupBox6.TabIndex = 3;
            groupBox6.TabStop = false;
            groupBox6.Text = "Lyrics Pattern";
            // 
            // OptLyricsPattern1
            // 
            OptLyricsPattern1.AutoSize = true;
            OptLyricsPattern1.Location = new Point(8, 58);
            OptLyricsPattern1.Margin = new Padding(4, 5, 4, 5);
            OptLyricsPattern1.Name = "OptLyricsPattern1";
            OptLyricsPattern1.Size = new Size(94, 24);
            OptLyricsPattern1.TabIndex = 1;
            OptLyricsPattern1.Text = "Sequence";
            // 
            // OptLyricsPattern0
            // 
            OptLyricsPattern0.AutoSize = true;
            OptLyricsPattern0.Location = new Point(9, 28);
            OptLyricsPattern0.Margin = new Padding(4, 5, 4, 5);
            OptLyricsPattern0.Name = "OptLyricsPattern0";
            OptLyricsPattern0.Size = new Size(64, 24);
            OptLyricsPattern0.TabIndex = 0;
            OptLyricsPattern0.Text = "Basic";
            // 
            // optCapoZero
            // 
            optCapoZero.AutoSize = true;
            optCapoZero.BackColor = Color.Transparent;
            optCapoZero.Location = new Point(115, 109);
            optCapoZero.Margin = new Padding(4, 5, 4, 5);
            optCapoZero.Name = "optCapoZero";
            optCapoZero.Size = new Size(78, 24);
            optCapoZero.TabIndex = 8;
            optCapoZero.Text = "Capo 0";
            optCapoZero.UseVisualStyleBackColor = false;
            // 
            // optShowTiming
            // 
            optShowTiming.AutoSize = true;
            optShowTiming.BackColor = Color.Transparent;
            optShowTiming.Location = new Point(211, 108);
            optShowTiming.Margin = new Padding(4, 5, 4, 5);
            optShowTiming.Name = "optShowTiming";
            optShowTiming.Size = new Size(77, 24);
            optShowTiming.TabIndex = 11;
            optShowTiming.Text = "Timing";
            optShowTiming.UseVisualStyleBackColor = false;
            // 
            // optPrinterSpaces
            // 
            optPrinterSpaces.AutoSize = true;
            optPrinterSpaces.BackColor = Color.Transparent;
            optPrinterSpaces.Location = new Point(155, 218);
            optPrinterSpaces.Margin = new Padding(4, 5, 4, 5);
            optPrinterSpaces.Name = "optPrinterSpaces";
            optPrinterSpaces.Size = new Size(124, 24);
            optPrinterSpaces.TabIndex = 13;
            optPrinterSpaces.Text = "Printer Spaces";
            toolTip1.SetToolTip(optPrinterSpaces, "Add Printer Spaces");
            optPrinterSpaces.UseVisualStyleBackColor = false;
            // 
            // optShowCapo
            // 
            optShowCapo.AutoSize = true;
            optShowCapo.BackColor = Color.Transparent;
            optShowCapo.Location = new Point(211, 32);
            optShowCapo.Margin = new Padding(4, 5, 4, 5);
            optShowCapo.Name = "optShowCapo";
            optShowCapo.Size = new Size(66, 24);
            optShowCapo.TabIndex = 9;
            optShowCapo.Text = "Capo";
            optShowCapo.UseVisualStyleBackColor = false;
            // 
            // optShowKey
            // 
            optShowKey.AutoSize = true;
            optShowKey.BackColor = Color.Transparent;
            optShowKey.Location = new Point(211, 69);
            optShowKey.Margin = new Padding(4, 5, 4, 5);
            optShowKey.Name = "optShowKey";
            optShowKey.Size = new Size(55, 24);
            optShowKey.TabIndex = 10;
            optShowKey.Text = "Key";
            optShowKey.UseVisualStyleBackColor = false;
            // 
            // optWords5
            // 
            optWords5.AutoSize = true;
            optWords5.BackColor = Color.Transparent;
            optWords5.Location = new Point(13, 218);
            optWords5.Margin = new Padding(4, 5, 4, 5);
            optWords5.Name = "optWords5";
            optWords5.Size = new Size(96, 24);
            optWords5.TabIndex = 5;
            optWords5.Text = "Notations";
            optWords5.UseVisualStyleBackColor = false;
            // 
            // groupBox3
            // 
            groupBox3.Controls.Add(OptShowSection1);
            groupBox3.Controls.Add(OptShowSection0);
            groupBox3.Controls.Add(OptShowSection2);
            groupBox3.Location = new Point(9, 282);
            groupBox3.Margin = new Padding(4, 5, 4, 5);
            groupBox3.Name = "groupBox3";
            groupBox3.Padding = new Padding(4, 5, 4, 5);
            groupBox3.Size = new Size(304, 66);
            groupBox3.TabIndex = 1;
            groupBox3.TabStop = false;
            groupBox3.Text = "Regions";
            // 
            // OptShowSection1
            // 
            OptShowSection1.AutoSize = true;
            OptShowSection1.Location = new Point(200, 29);
            OptShowSection1.Margin = new Padding(4, 5, 4, 5);
            OptShowSection1.Name = "OptShowSection1";
            OptShowSection1.Size = new Size(89, 24);
            OptShowSection1.TabIndex = 2;
            OptShowSection1.Text = "Region 2";
            // 
            // OptShowSection0
            // 
            OptShowSection0.AutoSize = true;
            OptShowSection0.Location = new Point(95, 29);
            OptShowSection0.Margin = new Padding(4, 5, 4, 5);
            OptShowSection0.Name = "OptShowSection0";
            OptShowSection0.Size = new Size(89, 24);
            OptShowSection0.TabIndex = 1;
            OptShowSection0.Text = "Region 1";
            // 
            // OptShowSection2
            // 
            OptShowSection2.AutoSize = true;
            OptShowSection2.Location = new Point(11, 29);
            OptShowSection2.Margin = new Padding(4, 5, 4, 5);
            OptShowSection2.Name = "OptShowSection2";
            OptShowSection2.Size = new Size(61, 24);
            OptShowSection2.TabIndex = 0;
            OptShowSection2.Text = "Both";
            // 
            // BtnSaveExit
            // 
            BtnSaveExit.Location = new Point(8, 514);
            BtnSaveExit.Margin = new Padding(4, 5, 4, 5);
            BtnSaveExit.Name = "BtnSaveExit";
            BtnSaveExit.Size = new Size(111, 37);
            BtnSaveExit.TabIndex = 6;
            BtnSaveExit.Text = "Save && Close";
            BtnSaveExit.Click += BtnSaveExit_Click;
            // 
            // BtnCancel
            // 
            BtnCancel.DialogResult = DialogResult.Cancel;
            BtnCancel.Location = new Point(511, 514);
            BtnCancel.Margin = new Padding(4, 5, 4, 5);
            BtnCancel.Name = "BtnCancel";
            BtnCancel.Size = new Size(107, 37);
            BtnCancel.TabIndex = 9;
            BtnCancel.Text = "Close";
            // 
            // BtnOK
            // 
            BtnOK.Location = new Point(396, 514);
            BtnOK.Margin = new Padding(4, 5, 4, 5);
            BtnOK.Name = "BtnOK";
            BtnOK.Size = new Size(107, 37);
            BtnOK.TabIndex = 8;
            BtnOK.Text = "Generate";
            BtnOK.Click += BtnOK_Click;
            // 
            // Mess1
            // 
            Mess1.BackColor = SystemColors.Control;
            Mess1.Location = new Point(16, 480);
            Mess1.Margin = new Padding(4, 0, 4, 0);
            Mess1.Name = "Mess1";
            Mess1.Size = new Size(597, 20);
            Mess1.TabIndex = 39;
            // 
            // ProgressBar1
            // 
            ProgressBar1.Location = new Point(8, 474);
            ProgressBar1.Margin = new Padding(4, 5, 4, 5);
            ProgressBar1.Name = "ProgressBar1";
            ProgressBar1.Size = new Size(609, 32);
            ProgressBar1.Step = 1;
            ProgressBar1.Style = ProgressBarStyle.Continuous;
            ProgressBar1.TabIndex = 38;
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(optHeadings3);
            groupBox2.Controls.Add(optCapoZero);
            groupBox2.Controls.Add(optWords7);
            groupBox2.Controls.Add(optShowTiming);
            groupBox2.Controls.Add(optHeadings2);
            groupBox2.Controls.Add(optPrinterSpaces);
            groupBox2.Controls.Add(optHeadings1);
            groupBox2.Controls.Add(optShowCapo);
            groupBox2.Controls.Add(optHeadings0);
            groupBox2.Controls.Add(optShowKey);
            groupBox2.Controls.Add(optWords6);
            groupBox2.Controls.Add(optWords5);
            groupBox2.Controls.Add(optWords2);
            groupBox2.Controls.Add(optWords1);
            groupBox2.Controls.Add(optWords0);
            groupBox2.Location = new Point(321, 12);
            groupBox2.Margin = new Padding(4, 5, 4, 5);
            groupBox2.Name = "groupBox2";
            groupBox2.Padding = new Padding(4, 5, 4, 5);
            groupBox2.Size = new Size(296, 263);
            groupBox2.TabIndex = 4;
            groupBox2.TabStop = false;
            groupBox2.Text = "Show Details";
            // 
            // optHeadings3
            // 
            optHeadings3.AutoSize = true;
            optHeadings3.BackColor = Color.Transparent;
            optHeadings3.Location = new Point(155, 145);
            optHeadings3.Margin = new Padding(4, 5, 4, 5);
            optHeadings3.Name = "optHeadings3";
            optHeadings3.Size = new Size(140, 24);
            optHeadings3.TabIndex = 14;
            optHeadings3.Text = "PreChorus Head.";
            optHeadings3.UseVisualStyleBackColor = false;
            // 
            // optWords7
            // 
            optWords7.AutoSize = true;
            optWords7.BackColor = Color.Transparent;
            optWords7.Location = new Point(115, 71);
            optWords7.Margin = new Padding(4, 5, 4, 5);
            optWords7.Name = "optWords7";
            optWords7.Size = new Size(86, 24);
            optWords7.TabIndex = 7;
            optWords7.Text = "User Ref";
            optWords7.UseVisualStyleBackColor = false;
            // 
            // optHeadings2
            // 
            optHeadings2.AutoSize = true;
            optHeadings2.BackColor = Color.Transparent;
            optHeadings2.Location = new Point(155, 182);
            optHeadings2.Margin = new Padding(4, 5, 4, 5);
            optHeadings2.Name = "optHeadings2";
            optHeadings2.Size = new Size(136, 24);
            optHeadings2.TabIndex = 12;
            optHeadings2.Text = "Bridge Heading";
            optHeadings2.UseVisualStyleBackColor = false;
            // 
            // optHeadings1
            // 
            optHeadings1.AutoSize = true;
            optHeadings1.BackColor = Color.Transparent;
            optHeadings1.Location = new Point(13, 182);
            optHeadings1.Margin = new Padding(4, 5, 4, 5);
            optHeadings1.Name = "optHeadings1";
            optHeadings1.Size = new Size(137, 24);
            optHeadings1.TabIndex = 4;
            optHeadings1.Text = "Chorus Heading";
            optHeadings1.UseVisualStyleBackColor = false;
            // 
            // optHeadings0
            // 
            optHeadings0.AutoSize = true;
            optHeadings0.BackColor = Color.Transparent;
            optHeadings0.Location = new Point(13, 145);
            optHeadings0.Margin = new Padding(4, 5, 4, 5);
            optHeadings0.Name = "optHeadings0";
            optHeadings0.Size = new Size(127, 24);
            optHeadings0.TabIndex = 3;
            optHeadings0.Text = "Verse Heading";
            optHeadings0.UseVisualStyleBackColor = false;
            // 
            // optWords6
            // 
            optWords6.AutoSize = true;
            optWords6.BackColor = Color.Transparent;
            optWords6.Location = new Point(115, 34);
            optWords6.Margin = new Padding(4, 5, 4, 5);
            optWords6.Name = "optWords6";
            optWords6.Size = new Size(91, 24);
            optWords6.TabIndex = 6;
            optWords6.Text = "Book Ref";
            optWords6.UseVisualStyleBackColor = false;
            // 
            // optWords2
            // 
            optWords2.AutoSize = true;
            optWords2.BackColor = Color.Transparent;
            optWords2.Location = new Point(13, 108);
            optWords2.Margin = new Padding(4, 5, 4, 5);
            optWords2.Name = "optWords2";
            optWords2.Size = new Size(96, 24);
            optWords2.TabIndex = 2;
            optWords2.Text = "Copyright";
            optWords2.UseVisualStyleBackColor = false;
            // 
            // optWords1
            // 
            optWords1.AutoSize = true;
            optWords1.BackColor = Color.Transparent;
            optWords1.Location = new Point(13, 71);
            optWords1.Margin = new Padding(4, 5, 4, 5);
            optWords1.Name = "optWords1";
            optWords1.Size = new Size(98, 24);
            optWords1.TabIndex = 1;
            optWords1.Text = "Song Title";
            optWords1.UseVisualStyleBackColor = false;
            // 
            // optWords0
            // 
            optWords0.AutoSize = true;
            optWords0.BackColor = Color.Transparent;
            optWords0.Location = new Point(13, 34);
            optWords0.Margin = new Padding(4, 5, 4, 5);
            optWords0.Name = "optWords0";
            optWords0.Size = new Size(92, 24);
            optWords0.TabIndex = 0;
            optWords0.Text = "Song No.";
            optWords0.UseVisualStyleBackColor = false;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(panel8);
            groupBox1.Controls.Add(panel5);
            groupBox1.Controls.Add(panel4);
            groupBox1.Controls.Add(panel3);
            groupBox1.Controls.Add(panel2);
            groupBox1.Controls.Add(panel1);
            groupBox1.Location = new Point(9, 12);
            groupBox1.Margin = new Padding(4, 5, 4, 5);
            groupBox1.Name = "groupBox1";
            groupBox1.Padding = new Padding(4, 5, 4, 5);
            groupBox1.Size = new Size(304, 263);
            groupBox1.TabIndex = 0;
            groupBox1.TabStop = false;
            groupBox1.Text = "Font Size";
            // 
            // panel8
            // 
            panel8.Controls.Add(PanelFontColour5);
            panel8.Controls.Add(tbFontSize5);
            panel8.Controls.Add(toolStripFont5);
            panel8.Controls.Add(label8);
            panel8.Location = new Point(13, 211);
            panel8.Margin = new Padding(4, 5, 4, 5);
            panel8.Name = "panel8";
            panel8.Size = new Size(280, 38);
            panel8.TabIndex = 5;
            // 
            // PanelFontColour5
            // 
            PanelFontColour5.BackColor = Color.Black;
            PanelFontColour5.Cursor = Cursors.Hand;
            PanelFontColour5.Location = new Point(256, 6);
            PanelFontColour5.Margin = new Padding(4, 5, 4, 5);
            PanelFontColour5.Name = "PanelFontColour5";
            PanelFontColour5.Size = new Size(21, 28);
            PanelFontColour5.TabIndex = 2;
            PanelFontColour5.Tag = "5";
            toolTip1.SetToolTip(PanelFontColour5, "Set Notations Colour");
            PanelFontColour5.Click += PanelFontColour_Click;
            // 
            // tbFontSize5
            // 
            tbFontSize5.Location = new Point(200, 5);
            tbFontSize5.Margin = new Padding(4, 5, 4, 5);
            tbFontSize5.Maximum = new decimal(new int[] { 99, 0, 0, 0 });
            tbFontSize5.Name = "tbFontSize5";
            tbFontSize5.Size = new Size(51, 27);
            tbFontSize5.TabIndex = 1;
            // 
            // toolStripFont5
            // 
            toolStripFont5.AutoSize = false;
            toolStripFont5.CanOverflow = false;
            toolStripFont5.Dock = DockStyle.None;
            toolStripFont5.GripStyle = ToolStripGripStyle.Hidden;
            toolStripFont5.ImageScalingSize = new Size(20, 20);
            toolStripFont5.Items.AddRange(new ToolStripItem[] { toolStripButton1, toolStripButton5, toolStripButton9 });
            toolStripFont5.LayoutStyle = ToolStripLayoutStyle.HorizontalStackWithOverflow;
            toolStripFont5.Location = new Point(100, 0);
            toolStripFont5.Name = "toolStripFont5";
            toolStripFont5.RenderMode = ToolStripRenderMode.System;
            toolStripFont5.Size = new Size(95, 40);
            toolStripFont5.TabIndex = 0;
            // 
            // toolStripButton1
            // 
            toolStripButton1.CheckOnClick = true;
            toolStripButton1.DisplayStyle = ToolStripItemDisplayStyle.Image;
            toolStripButton1.Image = Resources.Bold;
            toolStripButton1.ImageTransparentColor = Color.Magenta;
            toolStripButton1.Name = "toolStripButton1";
            toolStripButton1.Size = new Size(29, 37);
            toolStripButton1.Tag = "0";
            // 
            // toolStripButton5
            // 
            toolStripButton5.CheckOnClick = true;
            toolStripButton5.DisplayStyle = ToolStripItemDisplayStyle.Image;
            toolStripButton5.Image = Resources.Italic;
            toolStripButton5.ImageTransparentColor = Color.Magenta;
            toolStripButton5.Name = "toolStripButton5";
            toolStripButton5.Size = new Size(29, 37);
            toolStripButton5.Tag = "1";
            // 
            // toolStripButton9
            // 
            toolStripButton9.CheckOnClick = true;
            toolStripButton9.DisplayStyle = ToolStripItemDisplayStyle.Image;
            toolStripButton9.Image = Resources.Underline;
            toolStripButton9.ImageTransparentColor = Color.Magenta;
            toolStripButton9.Name = "toolStripButton9";
            toolStripButton9.Size = new Size(29, 37);
            toolStripButton9.Tag = "2";
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new Point(4, 8);
            label8.Margin = new Padding(4, 0, 4, 0);
            label8.Name = "label8";
            label8.Size = new Size(77, 20);
            label8.TabIndex = 0;
            label8.Text = "Notations:";
            // 
            // panel5
            // 
            panel5.Controls.Add(PanelFontColour4);
            panel5.Controls.Add(tbFontSize4);
            panel5.Controls.Add(toolStripFont4);
            panel5.Controls.Add(label5);
            panel5.Location = new Point(13, 174);
            panel5.Margin = new Padding(4, 5, 4, 5);
            panel5.Name = "panel5";
            panel5.Size = new Size(280, 38);
            panel5.TabIndex = 4;
            // 
            // PanelFontColour4
            // 
            PanelFontColour4.BackColor = Color.Black;
            PanelFontColour4.Cursor = Cursors.Hand;
            PanelFontColour4.Location = new Point(256, 6);
            PanelFontColour4.Margin = new Padding(4, 5, 4, 5);
            PanelFontColour4.Name = "PanelFontColour4";
            PanelFontColour4.Size = new Size(21, 28);
            PanelFontColour4.TabIndex = 2;
            PanelFontColour4.Tag = "4";
            toolTip1.SetToolTip(PanelFontColour4, "Set Chorus/Bridge Colour");
            PanelFontColour4.Click += PanelFontColour_Click;
            // 
            // tbFontSize4
            // 
            tbFontSize4.Location = new Point(200, 5);
            tbFontSize4.Margin = new Padding(4, 5, 4, 5);
            tbFontSize4.Maximum = new decimal(new int[] { 99, 0, 0, 0 });
            tbFontSize4.Name = "tbFontSize4";
            tbFontSize4.Size = new Size(51, 27);
            tbFontSize4.TabIndex = 1;
            // 
            // toolStripFont4
            // 
            toolStripFont4.AutoSize = false;
            toolStripFont4.CanOverflow = false;
            toolStripFont4.Dock = DockStyle.None;
            toolStripFont4.GripStyle = ToolStripGripStyle.Hidden;
            toolStripFont4.ImageScalingSize = new Size(20, 20);
            toolStripFont4.Items.AddRange(new ToolStripItem[] { toolStripButton14, toolStripButton15, toolStripButton16 });
            toolStripFont4.LayoutStyle = ToolStripLayoutStyle.HorizontalStackWithOverflow;
            toolStripFont4.Location = new Point(100, 0);
            toolStripFont4.Name = "toolStripFont4";
            toolStripFont4.RenderMode = ToolStripRenderMode.System;
            toolStripFont4.Size = new Size(95, 40);
            toolStripFont4.TabIndex = 0;
            // 
            // toolStripButton14
            // 
            toolStripButton14.CheckOnClick = true;
            toolStripButton14.DisplayStyle = ToolStripItemDisplayStyle.Image;
            toolStripButton14.Image = Resources.Bold;
            toolStripButton14.ImageTransparentColor = Color.Magenta;
            toolStripButton14.Name = "toolStripButton14";
            toolStripButton14.Size = new Size(29, 37);
            toolStripButton14.Tag = "0";
            // 
            // toolStripButton15
            // 
            toolStripButton15.CheckOnClick = true;
            toolStripButton15.DisplayStyle = ToolStripItemDisplayStyle.Image;
            toolStripButton15.Image = Resources.Italic;
            toolStripButton15.ImageTransparentColor = Color.Magenta;
            toolStripButton15.Name = "toolStripButton15";
            toolStripButton15.Size = new Size(29, 37);
            toolStripButton15.Tag = "1";
            // 
            // toolStripButton16
            // 
            toolStripButton16.CheckOnClick = true;
            toolStripButton16.DisplayStyle = ToolStripItemDisplayStyle.Image;
            toolStripButton16.Image = Resources.Underline;
            toolStripButton16.ImageTransparentColor = Color.Magenta;
            toolStripButton16.Name = "toolStripButton16";
            toolStripButton16.Size = new Size(29, 37);
            toolStripButton16.Tag = "2";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(4, 8);
            label5.Margin = new Padding(4, 0, 4, 0);
            label5.Name = "label5";
            label5.Size = new Size(107, 20);
            label5.TabIndex = 0;
            label5.Text = "Chorus/Bridge:";
            // 
            // panel4
            // 
            panel4.Controls.Add(PanelFontColour3);
            panel4.Controls.Add(tbFontSize3);
            panel4.Controls.Add(toolStripFont3);
            panel4.Controls.Add(label4);
            panel4.Location = new Point(13, 137);
            panel4.Margin = new Padding(4, 5, 4, 5);
            panel4.Name = "panel4";
            panel4.Size = new Size(280, 38);
            panel4.TabIndex = 3;
            // 
            // PanelFontColour3
            // 
            PanelFontColour3.BackColor = Color.Black;
            PanelFontColour3.Cursor = Cursors.Hand;
            PanelFontColour3.Location = new Point(256, 6);
            PanelFontColour3.Margin = new Padding(4, 5, 4, 5);
            PanelFontColour3.Name = "PanelFontColour3";
            PanelFontColour3.Size = new Size(21, 28);
            PanelFontColour3.TabIndex = 2;
            PanelFontColour3.Tag = "3";
            toolTip1.SetToolTip(PanelFontColour3, "Set Verse Colour");
            PanelFontColour3.Click += PanelFontColour_Click;
            // 
            // tbFontSize3
            // 
            tbFontSize3.Location = new Point(200, 5);
            tbFontSize3.Margin = new Padding(4, 5, 4, 5);
            tbFontSize3.Maximum = new decimal(new int[] { 99, 0, 0, 0 });
            tbFontSize3.Name = "tbFontSize3";
            tbFontSize3.Size = new Size(51, 27);
            tbFontSize3.TabIndex = 1;
            // 
            // toolStripFont3
            // 
            toolStripFont3.AutoSize = false;
            toolStripFont3.CanOverflow = false;
            toolStripFont3.Dock = DockStyle.None;
            toolStripFont3.GripStyle = ToolStripGripStyle.Hidden;
            toolStripFont3.ImageScalingSize = new Size(20, 20);
            toolStripFont3.Items.AddRange(new ToolStripItem[] { toolStripButton10, toolStripButton11, toolStripButton12 });
            toolStripFont3.LayoutStyle = ToolStripLayoutStyle.HorizontalStackWithOverflow;
            toolStripFont3.Location = new Point(100, 0);
            toolStripFont3.Name = "toolStripFont3";
            toolStripFont3.RenderMode = ToolStripRenderMode.System;
            toolStripFont3.Size = new Size(95, 40);
            toolStripFont3.TabIndex = 0;
            // 
            // toolStripButton10
            // 
            toolStripButton10.CheckOnClick = true;
            toolStripButton10.DisplayStyle = ToolStripItemDisplayStyle.Image;
            toolStripButton10.Image = Resources.Bold;
            toolStripButton10.ImageTransparentColor = Color.Magenta;
            toolStripButton10.Name = "toolStripButton10";
            toolStripButton10.Size = new Size(29, 37);
            toolStripButton10.Tag = "0";
            // 
            // toolStripButton11
            // 
            toolStripButton11.CheckOnClick = true;
            toolStripButton11.DisplayStyle = ToolStripItemDisplayStyle.Image;
            toolStripButton11.Image = Resources.Italic;
            toolStripButton11.ImageTransparentColor = Color.Magenta;
            toolStripButton11.Name = "toolStripButton11";
            toolStripButton11.Size = new Size(29, 37);
            toolStripButton11.Tag = "1";
            // 
            // toolStripButton12
            // 
            toolStripButton12.CheckOnClick = true;
            toolStripButton12.DisplayStyle = ToolStripItemDisplayStyle.Image;
            toolStripButton12.Image = Resources.Underline;
            toolStripButton12.ImageTransparentColor = Color.Magenta;
            toolStripButton12.Name = "toolStripButton12";
            toolStripButton12.Size = new Size(29, 37);
            toolStripButton12.Tag = "2";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(4, 8);
            label4.Margin = new Padding(4, 0, 4, 0);
            label4.Name = "label4";
            label4.Size = new Size(47, 20);
            label4.TabIndex = 0;
            label4.Text = "Verse:";
            // 
            // panel3
            // 
            panel3.Controls.Add(PanelFontColour2);
            panel3.Controls.Add(tbFontSize2);
            panel3.Controls.Add(toolStripFont2);
            panel3.Controls.Add(label3);
            panel3.Location = new Point(13, 100);
            panel3.Margin = new Padding(4, 5, 4, 5);
            panel3.Name = "panel3";
            panel3.Size = new Size(280, 38);
            panel3.TabIndex = 2;
            // 
            // PanelFontColour2
            // 
            PanelFontColour2.BackColor = Color.Black;
            PanelFontColour2.Cursor = Cursors.Hand;
            PanelFontColour2.Location = new Point(256, 6);
            PanelFontColour2.Margin = new Padding(4, 5, 4, 5);
            PanelFontColour2.Name = "PanelFontColour2";
            PanelFontColour2.Size = new Size(21, 28);
            PanelFontColour2.TabIndex = 2;
            PanelFontColour2.Tag = "2";
            toolTip1.SetToolTip(PanelFontColour2, "Set Copyright/Ref Colour");
            PanelFontColour2.Click += PanelFontColour_Click;
            // 
            // tbFontSize2
            // 
            tbFontSize2.Location = new Point(200, 5);
            tbFontSize2.Margin = new Padding(4, 5, 4, 5);
            tbFontSize2.Maximum = new decimal(new int[] { 99, 0, 0, 0 });
            tbFontSize2.Name = "tbFontSize2";
            tbFontSize2.Size = new Size(51, 27);
            tbFontSize2.TabIndex = 1;
            // 
            // toolStripFont2
            // 
            toolStripFont2.AutoSize = false;
            toolStripFont2.CanOverflow = false;
            toolStripFont2.Dock = DockStyle.None;
            toolStripFont2.GripStyle = ToolStripGripStyle.Hidden;
            toolStripFont2.ImageScalingSize = new Size(20, 20);
            toolStripFont2.Items.AddRange(new ToolStripItem[] { toolStripButton6, toolStripButton7, toolStripButton8 });
            toolStripFont2.LayoutStyle = ToolStripLayoutStyle.HorizontalStackWithOverflow;
            toolStripFont2.Location = new Point(100, 0);
            toolStripFont2.Name = "toolStripFont2";
            toolStripFont2.RenderMode = ToolStripRenderMode.System;
            toolStripFont2.Size = new Size(95, 40);
            toolStripFont2.TabIndex = 0;
            // 
            // toolStripButton6
            // 
            toolStripButton6.CheckOnClick = true;
            toolStripButton6.DisplayStyle = ToolStripItemDisplayStyle.Image;
            toolStripButton6.Image = Resources.Bold;
            toolStripButton6.ImageTransparentColor = Color.Magenta;
            toolStripButton6.Name = "toolStripButton6";
            toolStripButton6.Size = new Size(29, 37);
            toolStripButton6.Tag = "0";
            // 
            // toolStripButton7
            // 
            toolStripButton7.CheckOnClick = true;
            toolStripButton7.DisplayStyle = ToolStripItemDisplayStyle.Image;
            toolStripButton7.Image = Resources.Italic;
            toolStripButton7.ImageTransparentColor = Color.Magenta;
            toolStripButton7.Name = "toolStripButton7";
            toolStripButton7.Size = new Size(29, 37);
            toolStripButton7.Tag = "1";
            // 
            // toolStripButton8
            // 
            toolStripButton8.CheckOnClick = true;
            toolStripButton8.DisplayStyle = ToolStripItemDisplayStyle.Image;
            toolStripButton8.Image = Resources.Underline;
            toolStripButton8.ImageTransparentColor = Color.Magenta;
            toolStripButton8.Name = "toolStripButton8";
            toolStripButton8.Size = new Size(29, 37);
            toolStripButton8.Tag = "2";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(4, 8);
            label3.Margin = new Padding(4, 0, 4, 0);
            label3.Name = "label3";
            label3.Size = new Size(105, 20);
            label3.TabIndex = 0;
            label3.Text = "Copyright/Ref:";
            // 
            // panel2
            // 
            panel2.Controls.Add(PanelFontColour1);
            panel2.Controls.Add(tbFontSize1);
            panel2.Controls.Add(toolStripFont1);
            panel2.Controls.Add(label2);
            panel2.Location = new Point(13, 63);
            panel2.Margin = new Padding(4, 5, 4, 5);
            panel2.Name = "panel2";
            panel2.Size = new Size(280, 38);
            panel2.TabIndex = 1;
            // 
            // PanelFontColour1
            // 
            PanelFontColour1.BackColor = Color.Black;
            PanelFontColour1.Cursor = Cursors.Hand;
            PanelFontColour1.Location = new Point(256, 6);
            PanelFontColour1.Margin = new Padding(4, 5, 4, 5);
            PanelFontColour1.Name = "PanelFontColour1";
            PanelFontColour1.Size = new Size(21, 28);
            PanelFontColour1.TabIndex = 2;
            PanelFontColour1.Tag = "1";
            toolTip1.SetToolTip(PanelFontColour1, "Set Song Title Colour");
            PanelFontColour1.Click += PanelFontColour_Click;
            // 
            // tbFontSize1
            // 
            tbFontSize1.Location = new Point(200, 5);
            tbFontSize1.Margin = new Padding(4, 5, 4, 5);
            tbFontSize1.Maximum = new decimal(new int[] { 99, 0, 0, 0 });
            tbFontSize1.Name = "tbFontSize1";
            tbFontSize1.Size = new Size(51, 27);
            tbFontSize1.TabIndex = 1;
            // 
            // toolStripFont1
            // 
            toolStripFont1.AutoSize = false;
            toolStripFont1.CanOverflow = false;
            toolStripFont1.Dock = DockStyle.None;
            toolStripFont1.GripStyle = ToolStripGripStyle.Hidden;
            toolStripFont1.ImageScalingSize = new Size(20, 20);
            toolStripFont1.Items.AddRange(new ToolStripItem[] { toolStripButton2, toolStripButton3, toolStripButton4 });
            toolStripFont1.LayoutStyle = ToolStripLayoutStyle.HorizontalStackWithOverflow;
            toolStripFont1.Location = new Point(100, 0);
            toolStripFont1.Name = "toolStripFont1";
            toolStripFont1.RenderMode = ToolStripRenderMode.System;
            toolStripFont1.Size = new Size(95, 40);
            toolStripFont1.TabIndex = 0;
            // 
            // toolStripButton2
            // 
            toolStripButton2.CheckOnClick = true;
            toolStripButton2.DisplayStyle = ToolStripItemDisplayStyle.Image;
            toolStripButton2.Image = Resources.Bold;
            toolStripButton2.ImageTransparentColor = Color.Magenta;
            toolStripButton2.Name = "toolStripButton2";
            toolStripButton2.Size = new Size(29, 37);
            toolStripButton2.Tag = "0";
            // 
            // toolStripButton3
            // 
            toolStripButton3.CheckOnClick = true;
            toolStripButton3.DisplayStyle = ToolStripItemDisplayStyle.Image;
            toolStripButton3.Image = Resources.Italic;
            toolStripButton3.ImageTransparentColor = Color.Magenta;
            toolStripButton3.Name = "toolStripButton3";
            toolStripButton3.Size = new Size(29, 37);
            toolStripButton3.Tag = "1";
            // 
            // toolStripButton4
            // 
            toolStripButton4.CheckOnClick = true;
            toolStripButton4.DisplayStyle = ToolStripItemDisplayStyle.Image;
            toolStripButton4.Image = Resources.Underline;
            toolStripButton4.ImageTransparentColor = Color.Magenta;
            toolStripButton4.Name = "toolStripButton4";
            toolStripButton4.Size = new Size(29, 37);
            toolStripButton4.Tag = "2";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(4, 8);
            label2.Margin = new Padding(4, 0, 4, 0);
            label2.Name = "label2";
            label2.Size = new Size(79, 20);
            label2.TabIndex = 0;
            label2.Text = "Song Title:";
            // 
            // panel1
            // 
            panel1.Controls.Add(PanelFontColour0);
            panel1.Controls.Add(tbFontSize0);
            panel1.Controls.Add(toolStripFont0);
            panel1.Controls.Add(label1);
            panel1.Location = new Point(13, 26);
            panel1.Margin = new Padding(4, 5, 4, 5);
            panel1.Name = "panel1";
            panel1.Size = new Size(280, 38);
            panel1.TabIndex = 0;
            // 
            // PanelFontColour0
            // 
            PanelFontColour0.BackColor = Color.Black;
            PanelFontColour0.Cursor = Cursors.Hand;
            PanelFontColour0.Location = new Point(256, 6);
            PanelFontColour0.Margin = new Padding(4, 5, 4, 5);
            PanelFontColour0.Name = "PanelFontColour0";
            PanelFontColour0.Size = new Size(21, 28);
            PanelFontColour0.TabIndex = 2;
            PanelFontColour0.Tag = "0";
            toolTip1.SetToolTip(PanelFontColour0, "Set Song Number Colour");
            PanelFontColour0.Click += PanelFontColour_Click;
            // 
            // tbFontSize0
            // 
            tbFontSize0.Location = new Point(200, 5);
            tbFontSize0.Margin = new Padding(4, 5, 4, 5);
            tbFontSize0.Maximum = new decimal(new int[] { 99, 0, 0, 0 });
            tbFontSize0.Name = "tbFontSize0";
            tbFontSize0.Size = new Size(51, 27);
            tbFontSize0.TabIndex = 1;
            // 
            // toolStripFont0
            // 
            toolStripFont0.AutoSize = false;
            toolStripFont0.CanOverflow = false;
            toolStripFont0.Dock = DockStyle.None;
            toolStripFont0.GripStyle = ToolStripGripStyle.Hidden;
            toolStripFont0.ImageScalingSize = new Size(20, 20);
            toolStripFont0.Items.AddRange(new ToolStripItem[] { toolStripButton100, toolStripButton101, toolStripButton102 });
            toolStripFont0.LayoutStyle = ToolStripLayoutStyle.HorizontalStackWithOverflow;
            toolStripFont0.Location = new Point(100, 0);
            toolStripFont0.Name = "toolStripFont0";
            toolStripFont0.RenderMode = ToolStripRenderMode.System;
            toolStripFont0.Size = new Size(95, 40);
            toolStripFont0.TabIndex = 0;
            // 
            // toolStripButton100
            // 
            toolStripButton100.CheckOnClick = true;
            toolStripButton100.DisplayStyle = ToolStripItemDisplayStyle.Image;
            toolStripButton100.Image = Resources.Bold;
            toolStripButton100.ImageTransparentColor = Color.Magenta;
            toolStripButton100.Name = "toolStripButton100";
            toolStripButton100.Size = new Size(29, 37);
            toolStripButton100.Tag = "0";
            // 
            // toolStripButton101
            // 
            toolStripButton101.CheckOnClick = true;
            toolStripButton101.DisplayStyle = ToolStripItemDisplayStyle.Image;
            toolStripButton101.Image = Resources.Italic;
            toolStripButton101.ImageTransparentColor = Color.Magenta;
            toolStripButton101.Name = "toolStripButton101";
            toolStripButton101.Size = new Size(29, 37);
            toolStripButton101.Tag = "1";
            // 
            // toolStripButton102
            // 
            toolStripButton102.CheckOnClick = true;
            toolStripButton102.DisplayStyle = ToolStripItemDisplayStyle.Image;
            toolStripButton102.Image = Resources.Underline;
            toolStripButton102.ImageTransparentColor = Color.Magenta;
            toolStripButton102.Name = "toolStripButton102";
            toolStripButton102.Size = new Size(29, 37);
            toolStripButton102.Tag = "2";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(4, 8);
            label1.Margin = new Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new Size(104, 20);
            label1.TabIndex = 0;
            label1.Text = "Song Number:";
            // 
            // BtnIndexOnly
            // 
            BtnIndexOnly.Location = new Point(167, 514);
            BtnIndexOnly.Margin = new Padding(4, 5, 4, 5);
            BtnIndexOnly.Name = "BtnIndexOnly";
            BtnIndexOnly.Size = new Size(107, 37);
            BtnIndexOnly.TabIndex = 7;
            BtnIndexOnly.Text = "Index Only";
            BtnIndexOnly.Click += BtnIndexOnly_Click;
            // 
            // BtnTitlesRef
            // 
            BtnTitlesRef.Location = new Point(281, 514);
            BtnTitlesRef.Margin = new Padding(4, 5, 4, 5);
            BtnTitlesRef.Name = "BtnTitlesRef";
            BtnTitlesRef.Size = new Size(107, 37);
            BtnTitlesRef.TabIndex = 40;
            BtnTitlesRef.Text = "Titles && Ref";
            BtnTitlesRef.Click += BtnTitlesRef_Click;
            // 
            // groupBox4
            // 
            groupBox4.Controls.Add(OptPageSize1);
            groupBox4.Controls.Add(OptPageSize0);
            groupBox4.Location = new Point(229, 352);
            groupBox4.Margin = new Padding(4, 5, 4, 5);
            groupBox4.Name = "groupBox4";
            groupBox4.Padding = new Padding(4, 5, 4, 5);
            groupBox4.Size = new Size(83, 98);
            groupBox4.TabIndex = 41;
            groupBox4.TabStop = false;
            groupBox4.Text = "Size";
            // 
            // OptPageSize1
            // 
            OptPageSize1.AutoSize = true;
            OptPageSize1.Location = new Point(7, 60);
            OptPageSize1.Margin = new Padding(4, 5, 4, 5);
            OptPageSize1.Name = "OptPageSize1";
            OptPageSize1.Size = new Size(68, 24);
            OptPageSize1.TabIndex = 1;
            OptPageSize1.Text = "Letter";
            // 
            // OptPageSize0
            // 
            OptPageSize0.AutoSize = true;
            OptPageSize0.Location = new Point(7, 28);
            OptPageSize0.Margin = new Padding(4, 5, 4, 5);
            OptPageSize0.Name = "OptPageSize0";
            OptPageSize0.Size = new Size(48, 24);
            OptPageSize0.TabIndex = 0;
            OptPageSize0.Text = "A4";
            // 
            // FrmGenerateDoc
            // 
            AcceptButton = BtnOK;
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(629, 569);
            Controls.Add(groupBox4);
            Controls.Add(BtnTitlesRef);
            Controls.Add(BtnIndexOnly);
            Controls.Add(Mess1);
            Controls.Add(groupBox5);
            Controls.Add(groupBox7);
            Controls.Add(groupBox6);
            Controls.Add(groupBox3);
            Controls.Add(BtnSaveExit);
            Controls.Add(BtnCancel);
            Controls.Add(BtnOK);
            Controls.Add(ProgressBar1);
            Controls.Add(groupBox2);
            Controls.Add(groupBox1);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Icon = (Icon)resources.GetObject("$this.Icon");
            Margin = new Padding(4, 5, 4, 5);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FrmGenerateDoc";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Generate RTF Document";
            Load += FrmFormatPraiseBookDoc_Load;
            groupBox5.ResumeLayout(false);
            groupBox5.PerformLayout();
            groupBox7.ResumeLayout(false);
            groupBox7.PerformLayout();
            panel6.ResumeLayout(false);
            panel6.PerformLayout();
            ((ISupportInitialize)tbSpacing1).EndInit();
            panel7.ResumeLayout(false);
            panel7.PerformLayout();
            ((ISupportInitialize)tbSpacing0).EndInit();
            groupBox6.ResumeLayout(false);
            groupBox6.PerformLayout();
            groupBox3.ResumeLayout(false);
            groupBox3.PerformLayout();
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            groupBox1.ResumeLayout(false);
            panel8.ResumeLayout(false);
            panel8.PerformLayout();
            ((ISupportInitialize)tbFontSize5).EndInit();
            toolStripFont5.ResumeLayout(false);
            toolStripFont5.PerformLayout();
            panel5.ResumeLayout(false);
            panel5.PerformLayout();
            ((ISupportInitialize)tbFontSize4).EndInit();
            toolStripFont4.ResumeLayout(false);
            toolStripFont4.PerformLayout();
            panel4.ResumeLayout(false);
            panel4.PerformLayout();
            ((ISupportInitialize)tbFontSize3).EndInit();
            toolStripFont3.ResumeLayout(false);
            toolStripFont3.PerformLayout();
            panel3.ResumeLayout(false);
            panel3.PerformLayout();
            ((ISupportInitialize)tbFontSize2).EndInit();
            toolStripFont2.ResumeLayout(false);
            toolStripFont2.PerformLayout();
            panel2.ResumeLayout(false);
            panel2.PerformLayout();
            ((ISupportInitialize)tbFontSize1).EndInit();
            toolStripFont1.ResumeLayout(false);
            toolStripFont1.PerformLayout();
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ((ISupportInitialize)tbFontSize0).EndInit();
            toolStripFont0.ResumeLayout(false);
            toolStripFont0.PerformLayout();
            groupBox4.ResumeLayout(false);
            groupBox4.PerformLayout();
            ResumeLayout(false);
        }
    }
}
