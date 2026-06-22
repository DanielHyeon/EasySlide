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
	public partial class FrmGenerateDoc : Form
	{
		private enum DocGenType
		{
			Index,
			TitlesRef,
			Complete
		}

		private string[] FList = new string[82];

		private int[] SongFolderLog = new int[32000];

		private int[,] FolderFNum = new int[Gf.MAXSONGSFOLDERS, 2];

		private int CurrentSong;

		private int CurSlide;

		private bool ChorusDone;

		private bool BibleItem;

		private bool ShowFirstLineOnly;

		private string[] Verse = new string[160];

		private int[] ListViewVerseIndex = new int[160];

		private int MaxTextWidth;

		private string RTFLineandNotationsSpacing;

		private string DummyNotationSym = '\u0001'.ToString();

		private string[] ColoursList = new string[6];

		private RichTextBox RichTextBox1 = new RichTextBox();

		private SongSettings DocItem = new SongSettings();

		private ListView SubDivideList = new ListView();

		private int InTransposeOffset = 0;

		public FrmGenerateDoc()
		{
			InitializeComponent();
		}

		private void FrmFormatPraiseBookDoc_Load(object sender, EventArgs e)
		{
			UpdateFontFields(ref toolStripFont0, ref tbFontSize0, ref PanelFontColour0, Gf.PB_WordsBold[0], Gf.PB_WordsItalic[0], Gf.PB_WordsUnderline[0], Gf.PB_WordsSize[0], Gf.PB_WordsColour[0]);
			UpdateFontFields(ref toolStripFont1, ref tbFontSize1, ref PanelFontColour1, Gf.PB_WordsBold[1], Gf.PB_WordsItalic[1], Gf.PB_WordsUnderline[1], Gf.PB_WordsSize[1], Gf.PB_WordsColour[1]);
			UpdateFontFields(ref toolStripFont2, ref tbFontSize2, ref PanelFontColour2, Gf.PB_WordsBold[2], Gf.PB_WordsItalic[2], Gf.PB_WordsUnderline[2], Gf.PB_WordsSize[2], Gf.PB_WordsColour[2]);
			UpdateFontFields(ref toolStripFont3, ref tbFontSize3, ref PanelFontColour3, Gf.PB_WordsBold[3], Gf.PB_WordsItalic[3], Gf.PB_WordsUnderline[3], Gf.PB_WordsSize[3], Gf.PB_WordsColour[3]);
			UpdateFontFields(ref toolStripFont4, ref tbFontSize4, ref PanelFontColour4, Gf.PB_WordsBold[4], Gf.PB_WordsItalic[4], Gf.PB_WordsUnderline[4], Gf.PB_WordsSize[4], Gf.PB_WordsColour[4]);
			UpdateFontFields(ref toolStripFont5, ref tbFontSize5, ref PanelFontColour5, Gf.PB_WordsBold[5], Gf.PB_WordsItalic[5], Gf.PB_WordsUnderline[5], Gf.PB_WordsSize[5], Gf.PB_WordsColour[5]);
			optWords0.Checked = ((Gf.PB_ShowWords[0] == 1) ? true : false);
			optWords1.Checked = ((Gf.PB_ShowWords[1] == 1) ? true : false);
			optWords2.Checked = ((Gf.PB_ShowWords[2] == 1) ? true : false);
			optWords6.Checked = ((Gf.PB_ShowWords[6] == 1) ? true : false);
			optWords7.Checked = ((Gf.PB_ShowWords[7] == 1) ? true : false);
			optHeadings0.Checked = ((Gf.PB_ShowHeadings[0] == 1) ? true : false);
			optHeadings1.Checked = ((Gf.PB_ShowHeadings[1] == 1) ? true : false);
			optHeadings2.Checked = ((Gf.PB_ShowHeadings[2] == 1) ? true : false);
			optHeadings3.Checked = ((Gf.PB_ShowHeadings[3] == 1) ? true : false);
			if (Gf.PB_ShowSection == 0)
			{
				OptShowSection0.Checked = true;
			}
			else if (Gf.PB_ShowSection == 1)
			{
				OptShowSection1.Checked = true;
			}
			else if (Gf.PB_ShowSection == 2)
			{
				OptShowSection2.Checked = true;
			}
			if (Gf.PB_ShowColumns == 1)
			{
				OptShowColumns1.Checked = true;
			}
			else if (Gf.PB_ShowColumns == 2)
			{
				OptShowColumns2.Checked = true;
			}
			if (Gf.PB_LyricsPattern == 0)
			{
				OptLyricsPattern0.Checked = true;
			}
			else
			{
				OptLyricsPattern1.Checked = true;
			}
			if (Gf.PB_PageSize == 1)
			{
				OptPageSize1.Checked = true;
			}
			else
			{
				OptPageSize0.Checked = true;
			}
			optPrinterSpaces.Checked = ((Gf.PB_PrinterSpaces > 0) ? true : false);
			optWords5.Checked = ((Gf.PB_ShowNotations == 1) ? true : false);
			optShowTiming.Checked = ((Gf.PB_ShowTiming == 1) ? true : false);
			optShowKey.Checked = ((Gf.PB_ShowKey == 1) ? true : false);
			optShowCapo.Checked = ((Gf.PB_ShowCapo == 1) ? true : false);
			optCapoZero.Checked = ((Gf.PB_CapoZero == 1) ? true : false);
			tbSpacing0.Minimum = 0m;
			tbSpacing0.Maximum = 5m;
			tbSpacing0.Value = Gf.PB_Spacing[0];
			tbSpacing1.Minimum = 1m;
			tbSpacing1.Maximum = 20m;
			tbSpacing1.Value = Gf.PB_Spacing[1];
			optNewScreen.Checked = ((Gf.PB_ShowScreenBreaks == 1) ? true : false);
			optOneSongPerPage.Checked = ((Gf.PB_OneSongPerPage == 1) ? true : false);
			Mess1.Text = "NOTE - Generate will overwrite: " + Gf.PB_FullFileName;
			DocItem.Initialise();
			DocItem.SplitScreens = false;
			Gf.SetListViewColumns(SubDivideList, 6);
			Gf.PB_FormatChanged = false;
			InitRTF();
		}

		private void InitRTF()
		{
			Gf.RTFNewLine = "\\b0\\i0\\ulnone\\par ";
			Gf.RTFTabValue[0] = 0;
			Gf.RTFTabValue[1] = 500;
			Gf.RTFTabValue[2] = 1000;
			Gf.RTFTabValue[3] = 4400;
			Gf.RTFIndent[0] = "\\pard\\fi-" + Convert.ToString(Gf.RTFTabValue[0] + 400) + "\\li" + Convert.ToString(Gf.RTFTabValue[0] + 400);
			Gf.RTFIndent[1] = "\\pard\\fi-" + Convert.ToString(Gf.RTFTabValue[1] + 400) + "\\li" + Convert.ToString(Gf.RTFTabValue[1] + 400) + "\\tx" + Convert.ToString(Gf.RTFTabValue[1]);
			Gf.RTFIndent[2] = "\\pard\\fi-" + Convert.ToString(Gf.RTFTabValue[2] + 400) + "\\li" + Convert.ToString(Gf.RTFTabValue[2] + 400) + "\\tx" + Convert.ToString(Gf.RTFTabValue[2]);
			if (Gf.PB_ShowColumns == 1)
			{
				MaxTextWidth = Gf.RTFTabValue[3] + 4500;
				Gf.RTFIndent[3] = "\\pard\\fi-" + Convert.ToString(400) + "\\li" + Convert.ToString(400) + "\\ri680\\tqr\\tx" + Convert.ToString(MaxTextWidth);
			}
			else
			{
				MaxTextWidth = Gf.RTFTabValue[3];
				Gf.RTFIndent[3] = "\\pard\\fi-" + Convert.ToString(400) + "\\li" + Convert.ToString(400) + "\\ri680\\tqr\\tx" + Convert.ToString(MaxTextWidth);
			}
			Gf.RTFIndent[4] = "\\pard\\fi-" + Convert.ToString(Gf.RTFTabValue[1] + 400) + "\\li" + Convert.ToString(Gf.RTFTabValue[1] + 400) + "\\tx" + Convert.ToString(Gf.RTFTabValue[1]);
			Gf.RTFIndent[5] = "\\pard\\fi-" + Convert.ToString(Gf.RTFTabValue[1]) + "\\li" + Convert.ToString(Gf.RTFTabValue[1]) + "\\tx" + Convert.ToString(Gf.RTFTabValue[1]);
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
			UpdateFontInfo(toolStripFont0, tbFontSize0, PanelFontColour0, ref Gf.PB_WordsBold[0], ref Gf.PB_WordsItalic[0], ref Gf.PB_WordsUnderline[0], ref Gf.PB_WordsSize[0], ref Gf.PB_WordsColour[0]);
			UpdateFontInfo(toolStripFont1, tbFontSize1, PanelFontColour1, ref Gf.PB_WordsBold[1], ref Gf.PB_WordsItalic[1], ref Gf.PB_WordsUnderline[1], ref Gf.PB_WordsSize[1], ref Gf.PB_WordsColour[1]);
			UpdateFontInfo(toolStripFont2, tbFontSize2, PanelFontColour2, ref Gf.PB_WordsBold[2], ref Gf.PB_WordsItalic[2], ref Gf.PB_WordsUnderline[2], ref Gf.PB_WordsSize[2], ref Gf.PB_WordsColour[2]);
			UpdateFontInfo(toolStripFont3, tbFontSize3, PanelFontColour3, ref Gf.PB_WordsBold[3], ref Gf.PB_WordsItalic[3], ref Gf.PB_WordsUnderline[3], ref Gf.PB_WordsSize[3], ref Gf.PB_WordsColour[3]);
			UpdateFontInfo(toolStripFont4, tbFontSize4, PanelFontColour4, ref Gf.PB_WordsBold[4], ref Gf.PB_WordsItalic[4], ref Gf.PB_WordsUnderline[4], ref Gf.PB_WordsSize[4], ref Gf.PB_WordsColour[4]);
			UpdateFontInfo(toolStripFont5, tbFontSize5, PanelFontColour5, ref Gf.PB_WordsBold[5], ref Gf.PB_WordsItalic[5], ref Gf.PB_WordsUnderline[5], ref Gf.PB_WordsSize[5], ref Gf.PB_WordsColour[5]);
			UpdateFontInfo(toolStripFont2, tbFontSize2, PanelFontColour2, ref Gf.PB_WordsBold[6], ref Gf.PB_WordsItalic[6], ref Gf.PB_WordsUnderline[6], ref Gf.PB_WordsSize[6], ref Gf.PB_WordsColour[6]);
			UpdateFontInfo(toolStripFont2, tbFontSize2, PanelFontColour2, ref Gf.PB_WordsBold[7], ref Gf.PB_WordsItalic[7], ref Gf.PB_WordsUnderline[7], ref Gf.PB_WordsSize[7], ref Gf.PB_WordsColour[7]);
			Gf.PB_ShowWords[0] = (optWords0.Checked ? 1 : 0);
			Gf.PB_ShowWords[1] = (optWords1.Checked ? 1 : 0);
			Gf.PB_ShowWords[2] = (optWords2.Checked ? 1 : 0);
			Gf.PB_ShowWords[6] = (optWords6.Checked ? 1 : 0);
			Gf.PB_ShowWords[7] = (optWords7.Checked ? 1 : 0);
			Gf.PB_ShowHeadings[0] = (optHeadings0.Checked ? 1 : 0);
			Gf.PB_ShowHeadings[1] = (optHeadings1.Checked ? 1 : 0);
			Gf.PB_ShowHeadings[2] = (optHeadings2.Checked ? 1 : 0);
			Gf.PB_ShowHeadings[3] = (optHeadings3.Checked ? 1 : 0);
			if (OptShowSection1.Checked)
			{
				Gf.PB_ShowSection = 1;
			}
			else if (OptShowSection2.Checked)
			{
				Gf.PB_ShowSection = 2;
			}
			else
			{
				Gf.PB_ShowSection = 0;
			}
			Gf.PB_ShowColumns = (OptShowColumns1.Checked ? 1 : 2);
			Gf.PB_LyricsPattern = ((!OptLyricsPattern0.Checked) ? 1 : 0);
			Gf.PB_PageSize = ((!OptPageSize0.Checked) ? 1 : 0);
			Gf.PB_ShowScreenBreaks = (optNewScreen.Checked ? 1 : 0);
			Gf.PB_OneSongPerPage = (optOneSongPerPage.Checked ? 1 : 0);
			Gf.PB_Spacing[0] = (int)tbSpacing0.Value;
			Gf.PB_Spacing[1] = (int)tbSpacing1.Value;
			Gf.PB_PrinterSpaces = (optPrinterSpaces.Checked ? 1 : 0);
			RegUtil.SaveRegValue("options", "PrinterSpaces", Gf.PB_PrinterSpaces);
			Gf.PB_ShowNotations = (optWords5.Checked ? 1 : 0);
			Gf.PB_ShowTiming = (optShowTiming.Checked ? 1 : 0);
			Gf.PB_ShowKey = (optShowKey.Checked ? 1 : 0);
			Gf.PB_ShowCapo = (optShowCapo.Checked ? 1 : 0);
			Gf.PB_CapoZero = (optCapoZero.Checked ? 1 : 0);
			Gf.PB_FormatChanged = true;
		}

		private void PanelFontColour_Click(object sender, EventArgs e)
		{
			Panel panel = (Panel)sender;
			Color black = Color.Black;
			switch (DataUtil.ObjToInt(panel.Tag))
			{
			case 0:
				PanelFontColour0.BackColor = Gf.SelectNewColour(PanelFontColour0.BackColor);
				break;
			case 1:
				PanelFontColour1.BackColor = Gf.SelectNewColour(PanelFontColour1.BackColor);
				break;
			case 2:
				PanelFontColour2.BackColor = Gf.SelectNewColour(PanelFontColour2.BackColor);
				break;
			case 3:
				PanelFontColour3.BackColor = Gf.SelectNewColour(PanelFontColour3.BackColor);
				break;
			case 4:
				PanelFontColour4.BackColor = Gf.SelectNewColour(PanelFontColour4.BackColor);
				break;
			case 5:
				PanelFontColour5.BackColor = Gf.SelectNewColour(PanelFontColour5.BackColor);
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
				Gf.RunProcess(Gf.PB_FullFileName);
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
				using StreamWriter streamWriter = new StreamWriter(Gf.PB_FullFileName, append: false, FileUtil.Utf8WithBom);
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
					Gf.RTFLineSpacing = "";
					RTFLineandNotationsSpacing = "";
					for (int i = 0; i <= Gf.PB_Spacing[0]; i++)
					{
						Gf.RTFLineSpacing += Gf.RTFNewLine;
						RTFLineandNotationsSpacing += "\n";
					}
					streamWriter.Write(BuildRTFHeader());
					if (GenType == DocGenType.Complete || GenType == DocGenType.TitlesRef)
					{
						for (int i = 0; i < Gf.TotalPraiseBookItems; i++)
						{
							num2 = (i + 1) * 100 / Gf.TotalPraiseBookItems;
							ProgressBar1.Value = ((num2 > 100) ? 100 : num2);
							Invalidate();
							CurrentSong = i + 1;
							LoadItem(ref DocItem);
							SongFolderLog[i] = DocItem.FolderNo;
							string text3 = "";
							if (Gf.PB_ShowWords[0] > 0)
							{
								text = DataUtil.ExtractOneInfo(ref Gf.DocumentSongs[CurrentSong, 4], '>', RemoveExtract: false, MinusOneIfBlank: false);
								text3 = ((!(text == "")) ? text : Convert.ToString(i + 1));
							}
							if (Gf.PB_ShowWords[1] > 0)
							{
								text3 = ((Gf.PB_ShowWords[0] <= 0) ? (Gf.FormatMode(1) + DocItem.Title) : ($"{text3}.{Gf.FormatMode(1)} {DocItem.Title}"));
							}
							if ((Gf.PB_ShowWords[2] > 0) | (Gf.PB_ShowWords[6] > 0) | (Gf.PB_ShowWords[7] > 0))
							{
								if ((Gf.PB_ShowWords[0] > 0) & (Gf.PB_ShowWords[1] > 0))
								{
									streamWriter.Write(AddtoRTF(text3, 0, 0, 4, 0, 0));
								}
								else if ((Gf.PB_ShowWords[0] < 1) & (Gf.PB_ShowWords[1] > 0))
								{
									streamWriter.Write(AddtoRTF(text3, 0, 0, 4, 0, 0));
								}
								else if ((Gf.PB_ShowWords[0] > 0) & (Gf.PB_ShowWords[1] < 1))
								{
									text3 = $"{text3}.{Gf.FormatMode(2)} {DocItem.Copyright} {DocItem.Book_Reference} {DocItem.User_Reference}";
									streamWriter.Write(AddtoRTF(text3, 0, 0, 4, 0, 0));
								}
								if ((Gf.PB_ShowWords[2] > 0) & (DocItem.Copyright != ""))
								{
									streamWriter.Write(AddtoRTF(DocItem.Copyright, 0, 2, 0, 0, 0));
								}
								if ((Gf.PB_ShowWords[6] > 0) & (DocItem.Book_Reference != ""))
								{
									streamWriter.Write(AddtoRTF(DocItem.Book_Reference, 0, 2, 0, 0, 0));
								}
								if ((Gf.PB_ShowWords[7] > 0) & (DocItem.User_Reference != ""))
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
							if (Gf.PB_CapoZero > 0)
							{
								if (DocItem.MusicKey != "" && num3 >= 0)
								{
									text2 = DocItem.MusicKey;
									Gf.TransposeKey(ref text2, Gf.IncrementChord(ref num3, 0));
								}
							}
							else
							{
								string InString = Gf.DocumentSongs[CurrentSong, 4];
								DataUtil.ExtractOneInfo(ref InString, '>');
								InTransposeOffset = DataUtil.StringToInt(DataUtil.ExtractOneInfo(ref InString, '>', RemoveExtract: false, MinusOneIfBlank: false), Minus1IfBlank: false);
								if (InTransposeOffset != 0)
								{
									int CurPos = 0;
									string InKey = DocItem.MusicKey;
									Gf.TransposeKey(ref InKey, Gf.IncrementChord(ref CurPos, InTransposeOffset));
								}
							}
							string text4 = ((Gf.PB_ShowKey != 1) ? "" : ((text2 != "") ? ("Key: " + text2 + " ") : "")) + ((!((Gf.PB_ShowCapo == 1) & (Gf.PB_CapoZero < 1))) ? "" : ((num3 > 0) ? ("Capo " + Convert.ToString(num3) + " ") : "")) + ((Gf.PB_ShowTiming != 1) ? "" : ((DocItem.Timing != "") ? ("(" + DocItem.Timing + ")") : ""));
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
										if (Gf.PB_ShowHeadings[1] > 0)
										{
											tabNum = 2;
											if (!ChorusDone)
											{
												text5 = Gf.FolderLyricsHeading[DocItem.FolderNo, 1];
												ChorusDone = true;
											}
											else
											{
												ShowFirstLineOnly = true;
											}
										}
										else if (Gf.PB_ShowHeadings[0] > 0)
										{
											tabNum = ((!((Gf.PB_ShowHeadings[0] > 0) & DocItem.VersePresent[2])) ? 1 : 2);
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
										if (Gf.PB_ShowHeadings[1] <= 0)
										{
											tabNum = ((Gf.PB_ShowHeadings[0] <= 0) ? 1 : ((!((Gf.PB_ShowHeadings[0] > 0) & DocItem.VersePresent[2])) ? 1 : 2));
										}
										else
										{
											tabNum = 2;
											text5 = Gf.FolderLyricsHeading[DocItem.FolderNo, 1] + ((Gf.FolderLyricsHeading[DocItem.FolderNo, 1] != "") ? " (2)" : "");
										}
										formatOption = 4;
										flag = true;
									}
									else if (DocItem.Slide[CurSlide, 0] == 111)
									{
										if (Gf.PB_ShowHeadings[3] <= 0)
										{
											tabNum = ((Gf.PB_ShowHeadings[0] <= 0) ? 1 : ((!((Gf.PB_ShowHeadings[0] > 0) & DocItem.VersePresent[2])) ? 1 : 2));
										}
										else
										{
											tabNum = 2;
											text5 = Gf.FolderLyricsHeading[DocItem.FolderNo, 0];
										}
										formatOption = 4;
										flag = true;
									}
									else if (DocItem.Slide[CurSlide, 0] == 112)
									{
										if (Gf.PB_ShowHeadings[3] <= 0)
										{
											tabNum = ((Gf.PB_ShowHeadings[0] <= 0) ? 1 : ((!((Gf.PB_ShowHeadings[0] > 0) & DocItem.VersePresent[2])) ? 1 : 2));
										}
										else
										{
											tabNum = 2;
											text5 = Gf.FolderLyricsHeading[DocItem.FolderNo, 0] + ((Gf.FolderLyricsHeading[DocItem.FolderNo, 0] != "") ? " (2)" : "");
										}
										formatOption = 4;
										flag = true;
									}
									else if (DocItem.Slide[CurSlide, 0] == 100)
									{
										if (Gf.PB_ShowHeadings[2] <= 0)
										{
											tabNum = ((Gf.PB_ShowHeadings[0] <= 0) ? 1 : ((!((Gf.PB_ShowHeadings[0] > 0) & DocItem.VersePresent[2])) ? 1 : 2));
										}
										else
										{
											text5 = Gf.FolderLyricsHeading[DocItem.FolderNo, 2];
											tabNum = 2;
										}
										formatOption = 4;
										flag = true;
									}
									else if (DocItem.Slide[CurSlide, 0] == 103)
									{
										if (Gf.PB_ShowHeadings[2] <= 0)
										{
											tabNum = ((Gf.PB_ShowHeadings[0] <= 0) ? 1 : ((!((Gf.PB_ShowHeadings[0] > 0) & DocItem.VersePresent[2])) ? 1 : 2));
										}
										else
										{
											text5 = Gf.FolderLyricsHeading[DocItem.FolderNo, 2] + ((Gf.FolderLyricsHeading[DocItem.FolderNo, 2] != "") ? " (2)" : "");
											tabNum = 2;
										}
										formatOption = 4;
										flag = true;
									}
									else if (DocItem.Slide[CurSlide, 0] > 0)
									{
										if ((Gf.PB_ShowHeadings[0] > 0) & DocItem.VersePresent[2])
										{
											if (DocItem.Slide[CurSlide, 0] != 101)
											{
												text5 = Gf.VerseTitle[DocItem.Slide[CurSlide, 0]];
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
									else if (Gf.PB_ShowScreenBreaks > 0)
									{
										streamWriter.Write(AddtoRTF("", 0, formatOption, 0, 0, Gf.PB_Spacing[0]));
									}
									if (Gf.PB_ShowSection == 0)
									{
										if (DocItem.Slide[CurSlide, 2] >= 0)
										{
											streamWriter.Write(AddtoRTF(text5, 0, formatOption, tabNum, text5.Length, Gf.PB_Spacing[0], DocItem.Slide[CurSlide, 1], DocItem.Slide[CurSlide, 2]));
										}
									}
									else if (Gf.PB_ShowSection == 1)
									{
										if (DocItem.Slide[CurSlide, 4] >= 0)
										{
											streamWriter.Write(AddtoRTF(text5, 0, formatOption, tabNum, text5.Length, Gf.PB_Spacing[0], DocItem.Slide[CurSlide, 3], DocItem.Slide[CurSlide, 4]));
										}
									}
									else if (Gf.PB_ShowSection == 2)
									{
										bool flag2 = false;
										if (DocItem.Slide[CurSlide, 2] >= 0)
										{
											streamWriter.Write(AddtoRTF(text5, 0, formatOption, tabNum, text5.Length, Gf.PB_Spacing[0], DocItem.Slide[CurSlide, 1], DocItem.Slide[CurSlide, 2]));
											flag2 = true;
										}
										if (DocItem.Slide[CurSlide, 4] >= 0)
										{
											streamWriter.Write(AddtoRTF((!flag2) ? text5 : "", 1, formatOption, tabNum, (!flag2) ? text5.Length : 0, Gf.PB_Spacing[0], DocItem.Slide[CurSlide, 3], DocItem.Slide[CurSlide, 4]));
										}
									}
									if (ShowFirstLineOnly)
									{
										num4 += num - 1;
									}
								}
							}
							streamWriter.Write(AddtoRTF("", 0, formatOption, 0, 0, Gf.PB_Spacing[1] - 1));
							if (Gf.PB_OneSongPerPage > 0)
							{
								streamWriter.Write("\\page ");
							}
						}
					}
					if (Gf.PB_Layout == PraiseBookLayout.PraiseBook)
					{
						if (Gf.PB_OneSongPerPage < 1 && GenType == DocGenType.Complete)
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
						streamWriter.Write(AddtoRTF(Gf.RTFIndent[3] + "\\tab", 0, 3, 0, 0, 0));
						if (DataUtil.Left(Gf.DocumentSongs[1, 3], 1) != " ")
						{
							streamWriter.Write(AddtoRTF(Convert.ToString(Convert.ToInt32(Gf.DocumentSongs[1, 3])) + Gf.RTFIndent[3] + "\\tab", 0, 3, 0, 0, 0));
						}
						string inString = ((Gf.DocumentSongs[1, 2].Length < 4) ? (Gf.DocumentSongs[1, 2] + " ") : Gf.DocumentSongs[1, 2]) + Gf.RTFIndent[3] + "\\tab " + ((DataUtil.ExtractOneInfo(ref Gf.DocumentSongs[1, 4], '>', RemoveExtract: false, MinusOneIfBlank: false) == "") ? "1" : DataUtil.ExtractOneInfo(ref Gf.DocumentSongs[1, 4], '>', RemoveExtract: false, MinusOneIfBlank: false));
						streamWriter.Write(AddtoRTF(inString, 0, 3, 0, 0, 0));
						for (int i = 2; i <= Gf.TotalPraiseBookItems; i++)
						{
							if (DataUtil.ExtractOneInfo(ref Gf.DocumentSongs[i, 4], '>', RemoveExtract: false, MinusOneIfBlank: false) == "" && Gf.DocumentSongs[i, 3] != Gf.DocumentSongs[i - 1, 3])
							{
								if (DataUtil.Left(Gf.DocumentSongs[i, 3], 1) == " ")
								{
									streamWriter.Write(AddtoRTF(Gf.RTFIndent[3] + "\\tab", 0, 3, 0, 0, 0));
								}
								else if (Convert.ToInt32(Gf.DocumentSongs[i, 3]) < 17)
								{
									streamWriter.Write(AddtoRTF(Gf.RTFIndent[3] + "\\tab", 0, 3, 0, 0, 0));
									streamWriter.Write(AddtoRTF(Convert.ToString(Convert.ToInt32(Gf.DocumentSongs[i, 3])) + ((Convert.ToInt32(Gf.DocumentSongs[i, 3]) == 16) ? "+" : "") + Gf.RTFIndent[3] + "\\tab", 0, 3, 0, 0, 0));
								}
							}
							DocItem.FolderNo = SongFolderLog[i - 1];
							inString = ((Gf.DocumentSongs[i, 2].Length < 4) ? (Gf.DocumentSongs[i, 2] + " ") : Gf.DocumentSongs[i, 2]) + Gf.RTFIndent[3] + "\\tab " + ((DataUtil.ExtractOneInfo(ref Gf.DocumentSongs[1, 4], '>', RemoveExtract: false, MinusOneIfBlank: false) == "") ? i.ToString() : DataUtil.ExtractOneInfo(ref Gf.DocumentSongs[i, 4], '>', RemoveExtract: false, MinusOneIfBlank: false));
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
					MessageBox.Show("Error generating document " + Gf.PB_FullFileName + ". The document might be opened - please close it first!");
					return false;
				}
			}
			catch
			{
				MessageBox.Show("Error generating document " + Gf.PB_FullFileName + ". The document might be opened - please close it first!");
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
			FList[0] = Gf.ShowFontName[1, 0];
			RichTextBox1.Text = "";
			int num = 1;
			for (int i = 1; i < Gf.MAXSONGSFOLDERS; i++)
			{
				for (int j = 0; j <= 1; j++)
				{
					RichTextBox1.Focus();
					RichTextBox1.SelectionStart = RichTextBox1.Text.Length;
					RichTextBox1.SelectedText = ">";
					RichTextBox1.SelectionFont = new Font(Gf.ShowFontName[i, j], 11f, FontStyle.Regular);
					bool flag = false;
					for (int k = 1; k <= num; k++)
					{
						if (FList[k] == Gf.ShowFontName[i, j])
						{
							flag = true;
							FolderFNum[i, j] = k;
						}
					}
					if (!flag)
					{
						num++;
						FList[num] = Gf.ShowFontName[i, j];
						FolderFNum[i, j] = num;
						string text2 = text;
						text = text2 + "{\\f" + Convert.ToString(num) + "\\fnil " + Gf.ShowFontName[i, j] + ";}";
					}
				}
			}
			text += "}";
			for (int l = 0; l < 6; l++)
			{
				str3 = str3 + "\\red" + Gf.PB_WordsColour[l].R;
				str3 = str3 + "\\green" + Gf.PB_WordsColour[l].G;
				str3 = str3 + "\\blue" + Gf.PB_WordsColour[l].B + ";";
			}
			str3 += "}";
			string str5 = str2 + text + str3 + str4;
			string str6 = "{\\footer\\fs16\\sectd\\footery400\\pard\\qr {\\i " + Gf.PB_DocumentName + "\\par Document Generated by EasiSlides}{\\par }}";
			if (Gf.PB_ShowColumns == 1)
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
				num = 0;
				Section = 0;
			}
			string text = "";
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
			text7 = Gf.RTFIndent[TabNum];
			if (TabNum > 0 && TabNum < 4)
			{
				text3 = Gf.RTFTab;
			}
			else if (TabNum == 5)
			{
				text3 = Gf.RTFTab;
			}
			FontStyle fontStyle = FontStyle.Regular;
			if (Gf.PB_WordsBold[FormatOption] > 0)
			{
				fontStyle |= FontStyle.Bold;
			}
			if (Gf.PB_WordsItalic[FormatOption] > 0)
			{
				fontStyle |= FontStyle.Italic;
			}
			if (Gf.PB_WordsUnderline[FormatOption] > 0)
			{
				fontStyle |= FontStyle.Underline;
			}
			int num2 = Gf.PB_WordsSize[5];
			FontStyle style = fontStyle | FontStyle.Italic;
			Font mainFont = new Font(FList[FolderFNum[num, Section]], Gf.PB_WordsSize[FormatOption], fontStyle);
			Font notationsFont = new Font(FList[FolderFNum[num, Section]], num2, style);
			text5 = "\\f" + Convert.ToString(FolderFNum[num, Section]);
			string text11 = "\\fs" + Convert.ToString(num2 * 2) + Gf.FormatMode(5);
			bool flag = (Gf.FormatMode(5).IndexOf("\\ul ") >= 0) ? true : false;
			int transposeTo = 0;
			if (DocItem.Capo > 0)
			{
				transposeTo = Gf.IncrementChord(ref DocItem.Capo, 0);
			}
			if (PreLineSpacing > 0)
			{
				for (int i = 1; i <= PreLineSpacing; i++)
				{
					text5 += Gf.RTFNewLine;
				}
			}
			if (HeadingText == 0)
			{
				text5 += text3;
			}
			text6 = Gf.RTFIndent[TabNum];
			text6 += Gf.FormatMode(FormatOption);
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
					Gf.SubstituteDashes(ref text9, Gf.PB_ShowNotations);
					if (Gf.PB_ShowNotations == 1)
					{
						text8 = DocItem.LyricsAndNotationsList.Items[j].SubItems[3].Text;
						if (flag2 & !ShowFirstLineOnly)
						{
							if (Gf.PB_CapoZero == 1)
							{
								text8 = Gf.TransposeOneNotationString(text8, transposeTo, -1);
							}
							else if (InTransposeOffset > 0)
							{
								text8 = Gf.TransposeOneNotationString(text8, InTransposeOffset, -1);
							}
							Gf.SubDivideTextAndNotations(text9, text8, mainFont, notationsFont, ref SubDivideList, MaxTextWidth - 400 - 200);
							InString += Gf.FormatMode(FormatOption);
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
								InString = text14 + text11 + text12 + "\n" + DummyNotationSym + Gf.FormatMode(FormatOption) + SubDivideList.Items[k].SubItems[0].Text + text10 + "\n";
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
							InString = text14 + Gf.FormatMode(FormatOption) + text9 + text10 + "\n";
						}
					}
					else
					{
						string text14 = InString;
						InString = text14 + ((text8 != "") ? (text8 + "\n") : "") + Gf.FormatMode(FormatOption) + text9 + text10 + "\n";
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
						text4 = text4 + "\r\n" + Gf.RTFLineSpacing + text6;
					}
					else
					{
						string text15 = (DataUtil.Mid(InString, i + 2, 1) == DummyNotationSym) ? Gf.RTFNewLine : Gf.RTFLineSpacing;
						string text14 = text4;
						text4 = text14 + "\r\n" + text15 + text3 + text6;
					}
				}
				else if ((text != "\n") & (text != DummyNotationSym))
				{
					text4 += text;
				}
			}
			return text5 + text4 + "\r\n" + Gf.RTFNewLine;
		}

		private void LoadItem(ref SongSettings InItem)
		{
			string text = Gf.DocumentSongs[CurrentSong, 1];
			string inIDString = Gf.DocumentSongs[CurrentSong, 0];
			BibleItem = false;
			Gf.InitialiseIndividualData(ref InItem);
			if (text == "P")
			{
				InItem.Type = text;
				InItem.Title = Gf.RTFCheck(Gf.DocumentSongs[CurrentSong, 2]);
				InItem.CompleteLyrics = "(Powerpoint File)";
			}
			else if (text == "D")
			{
				Gf.LoadIndividualData(ref InItem, inIDString, "", 0);
			}
			else if (text == "B")
			{
				string InTitle = Gf.DocumentSongs[CurrentSong, 2];
				BibleItem = true;
				Gf.LoadIndividualData(ref InItem, inIDString, "", 0, ref InTitle);
				InItem.CompleteLyrics = InItem.CompleteLyrics.Replace('\u0098'.ToString(), " ");
			}
			else if (text == "T")
			{
				string InTitle = Gf.DocumentSongs[CurrentSong, 2];
				Gf.LoadIndividualData(ref InItem, inIDString, "", 0, ref InTitle);
			}
			else if (text == "I")
			{
				string InTitle = Gf.DocumentSongs[CurrentSong, 2];
				Gf.LoadIndividualData(ref InItem, inIDString, "", 0, ref InTitle);
				InItem.Title = InTitle;
			}
			else if (text == "W")
			{
				string InTitle = Gf.DocumentSongs[CurrentSong, 2];
				Gf.LoadIndividualData(ref InItem, inIDString, "", 0, ref InTitle);
			}
			InItem.Title = Gf.RTFCheck(InItem.Title);
			InItem.Title2 = Gf.RTFCheck(InItem.Title);
			InItem.CompleteLyrics = Gf.RTFCheck(InItem.CompleteLyrics);
			InItem.Copyright = Gf.RTFCheck(InItem.Writer + (((InItem.Writer != "") & (InItem.Copyright != "")) ? "; " : "") + InItem.Copyright);
			SongSettings obj = InItem;
			string copyright = obj.Copyright;
			obj.Copyright = copyright + ((InItem.Copyright == "") ? "" : " ") + InItem.Show_LicAdminInfo1 + ((InItem.Show_LicAdminInfo1 == "") ? "" : " ") + InItem.Show_LicAdminInfo2;
			Gf.FormatDisplayLyrics(ref InItem, PrepareSlides: true, (Gf.PB_LyricsPattern > 0) ? true : false);
		}

		private string FormatNotationString(string InString, string InNotation, Font MainFont, Font NotationsFont)
		{
			Graphics graphics = CreateGraphics();
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
				if (Gf.PB_PrinterSpaces > 0)
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
				if (Gf.PB_PrinterSpaces > 0)
				{
					return text4;
				}
				return text3;
			}
			return " ";
		}

		            }
}
