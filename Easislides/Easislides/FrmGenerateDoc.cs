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

		private ParagraphLayout paragraphs;

		private string rtfLineSpacing;

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
			UpdateFontFields(toolStripFont0, tbFontSize0, PanelFontColour0, Gf.PraiseBookTextStyles[0]);
			UpdateFontFields(toolStripFont1, tbFontSize1, PanelFontColour1, Gf.PraiseBookTextStyles[1]);
			UpdateFontFields(toolStripFont2, tbFontSize2, PanelFontColour2, Gf.PraiseBookTextStyles[2]);
			UpdateFontFields(toolStripFont3, tbFontSize3, PanelFontColour3, Gf.PraiseBookTextStyles[3]);
			UpdateFontFields(toolStripFont4, tbFontSize4, PanelFontColour4, Gf.PraiseBookTextStyles[4]);
			UpdateFontFields(toolStripFont5, tbFontSize5, PanelFontColour5, Gf.PraiseBookTextStyles[5]);
			optWords0.Checked = Gf.PraiseBookTextStyles[0].Visible;
			optWords1.Checked = Gf.PraiseBookTextStyles[1].Visible;
			optWords2.Checked = Gf.PraiseBookTextStyles[2].Visible;
			optWords6.Checked = Gf.PraiseBookTextStyles[6].Visible;
			optWords7.Checked = Gf.PraiseBookTextStyles[7].Visible;
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

		internal readonly struct ParagraphLayout
		{
			public int TextWidth { get; }
			public string IndexIndent { get; }

			public ParagraphLayout(int columnCount)
			{
				TextWidth = columnCount == 1 ? 8900 : 4400;
				IndexIndent = FormattableString.Invariant($@"\pard\fi-400\li400\ri680\tqr\tx{TextWidth}");
			}

			public string GetIndent(int tabNumber) => tabNumber switch
			{
				0 => @"\pard\fi-400\li400",
				1 or 4 => @"\pard\fi-900\li900\tx500",
				2 => @"\pard\fi-1400\li1400\tx1000",
				3 => IndexIndent,
				5 => @"\pard\fi-500\li500\tx500",
				_ => throw new ArgumentOutOfRangeException(nameof(tabNumber))
			};
		}

		private void InitRTF()
		{
			paragraphs = new ParagraphLayout(Gf.PB_ShowColumns);
			RichTextBox1.Height = 0;
			RichTextBox1.Top = 0;
		}

		private static void UpdateFontFields(ToolStrip toolbar, NumericUpDown size, Panel colour, PraiseBookTextStyle style)
		{
			((ToolStripButton)toolbar.Items[0]).Checked = style.Bold;
			((ToolStripButton)toolbar.Items[1]).Checked = style.Italic;
			((ToolStripButton)toolbar.Items[2]).Checked = style.Underline;
			size.Minimum = 4m;
			size.Maximum = 72m;
			size.Value = style.FontSize;
			colour.BackColor = style.Colour;
		}

		private static void UpdateFontInfo(ToolStrip toolbar, NumericUpDown size, Panel colour, ref PraiseBookTextStyle style)
		{
			style.Bold = ((ToolStripButton)toolbar.Items[0]).Checked;
			style.Italic = ((ToolStripButton)toolbar.Items[1]).Checked;
			style.Underline = ((ToolStripButton)toolbar.Items[2]).Checked;
			style.FontSize = (int)size.Value;
			style.Colour = colour.BackColor;
		}

		private void UpdatePBFormat()
		{
			UpdateFontInfo(toolStripFont0, tbFontSize0, PanelFontColour0, ref Gf.PraiseBookTextStyles[0]);
			UpdateFontInfo(toolStripFont1, tbFontSize1, PanelFontColour1, ref Gf.PraiseBookTextStyles[1]);
			UpdateFontInfo(toolStripFont2, tbFontSize2, PanelFontColour2, ref Gf.PraiseBookTextStyles[2]);
			UpdateFontInfo(toolStripFont3, tbFontSize3, PanelFontColour3, ref Gf.PraiseBookTextStyles[3]);
			UpdateFontInfo(toolStripFont4, tbFontSize4, PanelFontColour4, ref Gf.PraiseBookTextStyles[4]);
			UpdateFontInfo(toolStripFont5, tbFontSize5, PanelFontColour5, ref Gf.PraiseBookTextStyles[5]);
			UpdateFontInfo(toolStripFont2, tbFontSize2, PanelFontColour2, ref Gf.PraiseBookTextStyles[6]);
			UpdateFontInfo(toolStripFont2, tbFontSize2, PanelFontColour2, ref Gf.PraiseBookTextStyles[7]);
			Gf.PraiseBookTextStyles[0].Visible = optWords0.Checked;
			Gf.PraiseBookTextStyles[1].Visible = optWords1.Checked;
			Gf.PraiseBookTextStyles[2].Visible = optWords2.Checked;
			Gf.PraiseBookTextStyles[6].Visible = optWords6.Checked;
			Gf.PraiseBookTextStyles[7].Visible = optWords7.Checked;
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
					int chorusScreenCount = 0;
					string songNumber = "";
					int progressPercent = 0;
					string musicKey = "";
					int capo = 0;
					rtfLineSpacing = "";
					RTFLineandNotationsSpacing = "";
					for (int i = 0; i <= Gf.PB_Spacing[0]; i++)
					{
						rtfLineSpacing += Gf.RTFNewLine;
						RTFLineandNotationsSpacing += "\n";
					}
					streamWriter.Write(BuildRTFHeader());
					if (GenType == DocGenType.Complete || GenType == DocGenType.TitlesRef)
					{
						for (int i = 0; i < Gf.TotalPraiseBookItems; i++)
						{
							progressPercent = (i + 1) * 100 / Gf.TotalPraiseBookItems;
							ProgressBar1.Value = ((progressPercent > 100) ? 100 : progressPercent);
							Invalidate();
							CurrentSong = i + 1;
							LoadItem(ref DocItem);
							SongFolderLog[i] = DocItem.FolderNo;
							string titleLine = "";
							if (Gf.PraiseBookTextStyles[0].Visible)
							{
								songNumber = DataUtil.ExtractOneInfo(ref Gf.DocumentSongs[CurrentSong].NumberAndInfo, '>', RemoveExtract: false, MinusOneIfBlank: false);
								titleLine = ((songNumber != "") ? songNumber : Convert.ToString(i + 1));
							}
							if (Gf.PraiseBookTextStyles[1].Visible)
							{
								titleLine = (!Gf.PraiseBookTextStyles[0].Visible ? (Gf.FormatMode(1) + DocItem.Title) : ($"{titleLine}.{Gf.FormatMode(1)} {DocItem.Title}"));
							}
							if (Gf.PraiseBookTextStyles[2].Visible || Gf.PraiseBookTextStyles[6].Visible || Gf.PraiseBookTextStyles[7].Visible)
							{
								if (Gf.PraiseBookTextStyles[1].Visible)
								{
									streamWriter.Write(AddtoRTF(titleLine, 0, 0, 4, 0, 0));
								}
								else if (Gf.PraiseBookTextStyles[0].Visible)
								{
									titleLine = $"{titleLine}.{Gf.FormatMode(2)} {DocItem.Copyright} {DocItem.Book_Reference} {DocItem.User_Reference}";
									streamWriter.Write(AddtoRTF(titleLine, 0, 0, 4, 0, 0));
								}
								if (Gf.PraiseBookTextStyles[2].Visible && (DocItem.Copyright != ""))
								{
									streamWriter.Write(AddtoRTF(DocItem.Copyright, 0, 2, 0, 0, 0));
								}
								if (Gf.PraiseBookTextStyles[6].Visible && (DocItem.Book_Reference != ""))
								{
									streamWriter.Write(AddtoRTF(DocItem.Book_Reference, 0, 2, 0, 0, 0));
								}
								if (Gf.PraiseBookTextStyles[7].Visible && (DocItem.User_Reference != ""))
								{
									streamWriter.Write(AddtoRTF(DocItem.User_Reference, 0, 2, 0, 0, 0));
								}
							}
							else if (titleLine.Length > 0)
							{
								streamWriter.Write(AddtoRTF(titleLine, 0, 0, 4, 0, 0));
							}
							capo = DocItem.Capo;
							musicKey = DocItem.MusicKey;
							InTransposeOffset = 0;
							if (Gf.PB_CapoZero > 0)
							{
								if (DocItem.MusicKey != "" && capo >= 0)
								{
									musicKey = DocItem.MusicKey;
									Gf.TransposeKey(ref musicKey, Gf.IncrementChord(ref capo, 0));
								}
							}
							else
							{
								string InString = Gf.DocumentSongs[CurrentSong].NumberAndInfo;
								DataUtil.ExtractOneInfo(ref InString, '>');
								InTransposeOffset = DataUtil.StringToInt(DataUtil.ExtractOneInfo(ref InString, '>', RemoveExtract: false, MinusOneIfBlank: false), Minus1IfBlank: false);
								// Same condition and offset as the chord transposition in AddtoRTF, so the
								// printed key always matches the printed chords.
								if (InTransposeOffset > 0)
								{
									int CurPos = 0;
									Gf.TransposeKey(ref musicKey, Gf.IncrementChord(ref CurPos, InTransposeOffset));
								}
							}
							string keyLine = ((Gf.PB_ShowKey != 1) ? "" : ((musicKey != "") ? ("Key: " + musicKey + " ") : "")) + (((Gf.PB_ShowCapo == 1) && (Gf.PB_CapoZero < 1)) ? ((capo > 0) ? ("Capo " + Convert.ToString(capo) + " ") : "") : "") + ((Gf.PB_ShowTiming != 1) ? "" : ((DocItem.Timing != "") ? ("(" + DocItem.Timing + ")") : ""));
							if (keyLine != "")
							{
								streamWriter.Write(AddtoRTF(keyLine, 0, 3, 0, 0, 0));
							}
							if (GenType == DocGenType.Complete)
							{
								int chorusSlide = 0;
								for (int j = 1; j <= DocItem.TotalSlides; j++)
								{
									if (DocItem.Slide[j, 0] == 0)
									{
										chorusSlide = j;
										chorusScreenCount = 1;
									}
									if ((chorusSlide > 0) && (DocItem.Slide[j, 0] < 0))
									{
										chorusScreenCount++;
									}
								}
								int tabNum = 0;
								ChorusDone = false;
								for (int slideNo = 1; slideNo <= DocItem.TotalSlides; slideNo++)
								{
									bool isSectionStart = false;
									ShowFirstLineOnly = false;
									CurSlide = slideNo;
									string headingText = "";
									if (DocItem.Slide[CurSlide, 0] == 0)
									{
										if (Gf.PB_ShowHeadings[1] > 0)
										{
											tabNum = 2;
											if (!ChorusDone)
											{
												headingText = Gf.Folders[DocItem.FolderNo].LyricsHeading[1];
												ChorusDone = true;
											}
											else
											{
												ShowFirstLineOnly = true;
											}
										}
										else if (Gf.PB_ShowHeadings[0] > 0)
										{
											tabNum = ((!((Gf.PB_ShowHeadings[0] > 0) && DocItem.VersePresent[2])) ? 1 : 2);
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
										isSectionStart = true;
									}
									else if (DocItem.Slide[CurSlide, 0] == 102)
									{
										if (Gf.PB_ShowHeadings[1] <= 0)
										{
											tabNum = ((Gf.PB_ShowHeadings[0] <= 0) ? 1 : ((!((Gf.PB_ShowHeadings[0] > 0) && DocItem.VersePresent[2])) ? 1 : 2));
										}
										else
										{
											tabNum = 2;
											headingText = Gf.Folders[DocItem.FolderNo].LyricsHeading[1] + ((Gf.Folders[DocItem.FolderNo].LyricsHeading[1] != "") ? " (2)" : "");
										}
										formatOption = 4;
										isSectionStart = true;
									}
									else if (DocItem.Slide[CurSlide, 0] == 111)
									{
										if (Gf.PB_ShowHeadings[3] <= 0)
										{
											tabNum = ((Gf.PB_ShowHeadings[0] <= 0) ? 1 : ((!((Gf.PB_ShowHeadings[0] > 0) && DocItem.VersePresent[2])) ? 1 : 2));
										}
										else
										{
											tabNum = 2;
											headingText = Gf.Folders[DocItem.FolderNo].LyricsHeading[0];
										}
										formatOption = 4;
										isSectionStart = true;
									}
									else if (DocItem.Slide[CurSlide, 0] == 112)
									{
										if (Gf.PB_ShowHeadings[3] <= 0)
										{
											tabNum = ((Gf.PB_ShowHeadings[0] <= 0) ? 1 : ((!((Gf.PB_ShowHeadings[0] > 0) && DocItem.VersePresent[2])) ? 1 : 2));
										}
										else
										{
											tabNum = 2;
											headingText = Gf.Folders[DocItem.FolderNo].LyricsHeading[0] + ((Gf.Folders[DocItem.FolderNo].LyricsHeading[0] != "") ? " (2)" : "");
										}
										formatOption = 4;
										isSectionStart = true;
									}
									else if (DocItem.Slide[CurSlide, 0] == 100)
									{
										if (Gf.PB_ShowHeadings[2] <= 0)
										{
											tabNum = ((Gf.PB_ShowHeadings[0] <= 0) ? 1 : ((!((Gf.PB_ShowHeadings[0] > 0) && DocItem.VersePresent[2])) ? 1 : 2));
										}
										else
										{
											headingText = Gf.Folders[DocItem.FolderNo].LyricsHeading[2];
											tabNum = 2;
										}
										formatOption = 4;
										isSectionStart = true;
									}
									else if (DocItem.Slide[CurSlide, 0] == 103)
									{
										if (Gf.PB_ShowHeadings[2] <= 0)
										{
											tabNum = ((Gf.PB_ShowHeadings[0] <= 0) ? 1 : ((!((Gf.PB_ShowHeadings[0] > 0) && DocItem.VersePresent[2])) ? 1 : 2));
										}
										else
										{
											headingText = Gf.Folders[DocItem.FolderNo].LyricsHeading[2] + ((Gf.Folders[DocItem.FolderNo].LyricsHeading[2] != "") ? " (2)" : "");
											tabNum = 2;
										}
										formatOption = 4;
										isSectionStart = true;
									}
									else if (DocItem.Slide[CurSlide, 0] > 0)
									{
										if ((Gf.PB_ShowHeadings[0] > 0) && DocItem.VersePresent[2])
										{
											if (DocItem.Slide[CurSlide, 0] != 101)
											{
												headingText = Gf.VerseTitle[DocItem.Slide[CurSlide, 0]];
											}
											tabNum = 1;
										}
										else
										{
											tabNum = 0;
										}
										formatOption = 3;
										isSectionStart = true;
									}
									if (isSectionStart)
									{
										if (headingText.Length > 0)
										{
											headingText += "\t";
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
											streamWriter.Write(AddtoRTF(headingText, 0, formatOption, tabNum, headingText.Length, Gf.PB_Spacing[0], DocItem.Slide[CurSlide, 1], DocItem.Slide[CurSlide, 2]));
										}
									}
									else if (Gf.PB_ShowSection == 1)
									{
										if (DocItem.Slide[CurSlide, 4] >= 0)
										{
											streamWriter.Write(AddtoRTF(headingText, 0, formatOption, tabNum, headingText.Length, Gf.PB_Spacing[0], DocItem.Slide[CurSlide, 3], DocItem.Slide[CurSlide, 4]));
										}
									}
									else if (Gf.PB_ShowSection == 2)
									{
										bool region1Written = false;
										if (DocItem.Slide[CurSlide, 2] >= 0)
										{
											streamWriter.Write(AddtoRTF(headingText, 0, formatOption, tabNum, headingText.Length, Gf.PB_Spacing[0], DocItem.Slide[CurSlide, 1], DocItem.Slide[CurSlide, 2]));
											region1Written = true;
										}
										if (DocItem.Slide[CurSlide, 4] >= 0)
										{
											streamWriter.Write(AddtoRTF((!region1Written) ? headingText : "", 1, formatOption, tabNum, (!region1Written) ? headingText.Length : 0, Gf.PB_Spacing[0], DocItem.Slide[CurSlide, 3], DocItem.Slide[CurSlide, 4]));
										}
									}
									if (ShowFirstLineOnly)
									{
										slideNo += chorusScreenCount - 1;
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
						streamWriter.Write(AddtoRTF(paragraphs.IndexIndent + "\\tab", 0, 3, 0, 0, 0));
						if (DataUtil.Left(Gf.DocumentSongs[1].IndexGroup, 1) != " ")
						{
							streamWriter.Write(AddtoRTF(Convert.ToString(Convert.ToInt32(Gf.DocumentSongs[1].IndexGroup)) + paragraphs.IndexIndent + "\\tab", 0, 3, 0, 0, 0));
						}
						string inString = ((Gf.DocumentSongs[1].Title.Length < 4) ? (Gf.DocumentSongs[1].Title + " ") : Gf.DocumentSongs[1].Title) + paragraphs.IndexIndent + "\\tab " + ((DataUtil.ExtractOneInfo(ref Gf.DocumentSongs[1].NumberAndInfo, '>', RemoveExtract: false, MinusOneIfBlank: false) == "") ? "1" : DataUtil.ExtractOneInfo(ref Gf.DocumentSongs[1].NumberAndInfo, '>', RemoveExtract: false, MinusOneIfBlank: false));
						streamWriter.Write(AddtoRTF(inString, 0, 3, 0, 0, 0));
						for (int i = 2; i <= Gf.TotalPraiseBookItems; i++)
						{
							if (DataUtil.ExtractOneInfo(ref Gf.DocumentSongs[i].NumberAndInfo, '>', RemoveExtract: false, MinusOneIfBlank: false) == "" && Gf.DocumentSongs[i].IndexGroup != Gf.DocumentSongs[i - 1].IndexGroup)
							{
								if (DataUtil.Left(Gf.DocumentSongs[i].IndexGroup, 1) == " ")
								{
									streamWriter.Write(AddtoRTF(paragraphs.IndexIndent + "\\tab", 0, 3, 0, 0, 0));
								}
								else if (Convert.ToInt32(Gf.DocumentSongs[i].IndexGroup) < 17)
								{
									streamWriter.Write(AddtoRTF(paragraphs.IndexIndent + "\\tab", 0, 3, 0, 0, 0));
									streamWriter.Write(AddtoRTF(Convert.ToString(Convert.ToInt32(Gf.DocumentSongs[i].IndexGroup)) + ((Convert.ToInt32(Gf.DocumentSongs[i].IndexGroup) == 16) ? "+" : "") + paragraphs.IndexIndent + "\\tab", 0, 3, 0, 0, 0));
								}
							}
							DocItem.FolderNo = SongFolderLog[i - 1];
							inString = ((Gf.DocumentSongs[i].Title.Length < 4) ? (Gf.DocumentSongs[i].Title + " ") : Gf.DocumentSongs[i].Title) + paragraphs.IndexIndent + "\\tab " + ((DataUtil.ExtractOneInfo(ref Gf.DocumentSongs[i].NumberAndInfo, '>', RemoveExtract: false, MinusOneIfBlank: false) == "") ? i.ToString() : DataUtil.ExtractOneInfo(ref Gf.DocumentSongs[i].NumberAndInfo, '>', RemoveExtract: false, MinusOneIfBlank: false));
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
			string pageSize = OptPageSize0.Checked ? "\\pgwsxn11906\\pghsxn16838" : "\\pgwsxn12240\\pghsxn15840";
			string rtfStart = "{\\rtf1\\ansi" + pageSize + "\\ansicpg1252\\deff0\\deflang1033{\\fonttbl{\\f0\\fnil\\fcharset0 Microsoft Sans Serif;}";
			string fontTable = "";
			string colourTable = "{\\colortbl ;";
			string viewSettings = "\\viewkind1\\uc1\\pard\\f0\\fs17 ";
			for (int i = 1; i <= 81; i++)
			{
				FList[i] = "";
			}
			FList[0] = Gf.Folders[1].FontName[0];
			RichTextBox1.Text = "";
			int fontCount = 1;
			for (int i = 1; i < Gf.MAXSONGSFOLDERS; i++)
			{
				for (int j = 0; j <= 1; j++)
				{
					RichTextBox1.Focus();
					RichTextBox1.SelectionStart = RichTextBox1.Text.Length;
					RichTextBox1.SelectedText = ">";
					RichTextBox1.SelectionFont = new Font(Gf.Folders[i].FontName[j], 11f, FontStyle.Regular);
					bool fontFound = false;
					for (int k = 1; k <= fontCount; k++)
					{
						if (FList[k] == Gf.Folders[i].FontName[j])
						{
							fontFound = true;
							FolderFNum[i, j] = k;
						}
					}
					if (!fontFound)
					{
						fontCount++;
						FList[fontCount] = Gf.Folders[i].FontName[j];
						FolderFNum[i, j] = fontCount;
						fontTable = fontTable + "{\\f" + Convert.ToString(fontCount) + "\\fnil " + Gf.Folders[i].FontName[j] + ";}";
					}
				}
			}
			fontTable += "}";
			for (int l = 0; l < 6; l++)
			{
				colourTable = colourTable + "\\red" + Gf.PraiseBookTextStyles[l].Colour.R;
				colourTable = colourTable + "\\green" + Gf.PraiseBookTextStyles[l].Colour.G;
				colourTable = colourTable + "\\blue" + Gf.PraiseBookTextStyles[l].Colour.B + ";";
			}
			colourTable += "}";
			string header = rtfStart + fontTable + colourTable + viewSettings;
			string footer = "{\\footer\\fs16\\sectd\\footery400\\pard\\qr {\\i " + Gf.PB_DocumentName + "\\par Document Generated by EasiSlides}{\\par }}";
			if (Gf.PB_ShowColumns == 1)
			{
				return header + footer + "\\margr900\\margl1300\\margt900\\margb1000";
			}
			return header + footer + "\\margr900\\margl1300\\margt900\\margb1000\\cols2\\colno1\\colw4500\\colsr750\\colno2\\colw4500";
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
			int folderNo = DocItem.FolderNo;
			if (BibleItem && Section == 1)
			{
				folderNo = 0;
				Section = 0;
			}
			string currentChar = "";
			string tabPrefix = "";
			string rtfBody = "";
			string rtfPrefix = "";
			string notations = "";
			string lyricLine = "";
			string lineSuffix = "";
			if (InString == null)
			{
				InString = "";
			}
			if (TabNum > 0 && TabNum < 4)
			{
				tabPrefix = Gf.RTFTab;
			}
			else if (TabNum == 5)
			{
				tabPrefix = Gf.RTFTab;
			}
			FontStyle fontStyle = FontStyle.Regular;
			if (Gf.PraiseBookTextStyles[FormatOption].Bold)
			{
				fontStyle |= FontStyle.Bold;
			}
			if (Gf.PraiseBookTextStyles[FormatOption].Italic)
			{
				fontStyle |= FontStyle.Italic;
			}
			if (Gf.PraiseBookTextStyles[FormatOption].Underline)
			{
				fontStyle |= FontStyle.Underline;
			}
			int notationsFontSize = Gf.PraiseBookTextStyles[5].FontSize;
			FontStyle style = fontStyle | FontStyle.Italic;
			Font mainFont = new Font(FList[FolderFNum[folderNo, Section]], Gf.PraiseBookTextStyles[FormatOption].FontSize, fontStyle);
			Font notationsFont = new Font(FList[FolderFNum[folderNo, Section]], notationsFontSize, style);
			rtfPrefix = "\\f" + Convert.ToString(FolderFNum[folderNo, Section]);
			string notationsFormat = "\\fs" + Convert.ToString(notationsFontSize * 2) + Gf.FormatMode(5);
			bool underlineNotations = Gf.FormatMode(5).IndexOf("\\ul ") >= 0;
			int transposeTo = 0;
			if (DocItem.Capo > 0)
			{
				transposeTo = Gf.IncrementChord(ref DocItem.Capo, 0);
			}
			if (PreLineSpacing > 0)
			{
				for (int i = 1; i <= PreLineSpacing; i++)
				{
					rtfPrefix += Gf.RTFNewLine;
				}
			}
			if (HeadingText == 0)
			{
				rtfPrefix += tabPrefix;
			}
			string lineFormat = paragraphs.GetIndent(TabNum) + Gf.FormatMode(FormatOption);
			rtfBody = "";
			notations = "";
			string notationLine = "";
			if (StartLoc >= 0 && EndLoc >= 0)
			{
				if (ShowFirstLineOnly)
				{
					EndLoc = StartLoc;
				}
				bool hasNotations = false;
				for (int j = StartLoc; j <= EndLoc; j++)
				{
					if (DocItem.LyricsAndNotationsList.Items[j].SubItems[3].Text != "")
					{
						hasNotations = true;
						j = EndLoc;
					}
				}
				for (int j = StartLoc; j <= EndLoc; j++)
				{
					lyricLine = DocItem.LyricsAndNotationsList.Items[j].SubItems[2].Text + (ShowFirstLineOnly ? " ..." : "");
					Gf.SubstituteDashes(ref lyricLine, Gf.PB_ShowNotations);
					if (Gf.PB_ShowNotations == 1)
					{
						notations = DocItem.LyricsAndNotationsList.Items[j].SubItems[3].Text;
						if (hasNotations && !ShowFirstLineOnly)
						{
							if (Gf.PB_CapoZero == 1)
							{
								notations = Gf.TransposeOneNotationString(notations, transposeTo, -1);
							}
							else if (InTransposeOffset > 0)
							{
								notations = Gf.TransposeOneNotationString(notations, InTransposeOffset, -1);
							}
							Gf.SubDivideTextAndNotations(lyricLine, notations, mainFont, notationsFont, ref SubDivideList, paragraphs.TextWidth - 400 - 200);
							InString += Gf.FormatMode(FormatOption);
							for (int k = 0; k < SubDivideList.Items.Count; k++)
							{
								notationLine = SubDivideList.Items[k].SubItems[1].Text;
								if (underlineNotations)
								{
									string underlinedLine = "";
									for (int l = 0; l < notationLine.Length; l++)
									{
										if (notationLine[l] == ' ')
										{
											if (l < 1 || notationLine[l - 1] != ' ')
											{
												underlinedLine += "\\ulnone ";
											}
										}
										else if (l > 0 && notationLine[l - 1] == ' ')
										{
											underlinedLine += "\\ul ";
										}
										underlinedLine += notationLine[l];
									}
									notationLine = underlinedLine;
								}
								InString = InString + notationsFormat + notationLine + "\n" + DummyNotationSym + Gf.FormatMode(FormatOption) + SubDivideList.Items[k].SubItems[0].Text + lineSuffix + "\n";
							}
							if (DataUtil.Right(InString, 1) == "\n")
							{
								InString = DataUtil.Left(InString, InString.Length - 1);
							}
							InString = InString + lineSuffix + "\n";
						}
						else
						{
							InString = InString + Gf.FormatMode(FormatOption) + lyricLine + lineSuffix + "\n";
						}
					}
					else
					{
						InString = InString + ((notations != "") ? (notations + "\n") : "") + Gf.FormatMode(FormatOption) + lyricLine + lineSuffix + "\n";
					}
				}
				InString = DataUtil.TrimEnd(InString).Replace("\n", "\r\n");
			}
			for (int i = 0; i < InString.Length; i++)
			{
				currentChar = DataUtil.Mid(InString, i, 1);
				if ((currentChar != "\r") && (currentChar != "\n"))
				{
					int charCode = (currentChar != "") ? currentChar[0] : '\0';
					if (charCode < 0)
					{
						charCode += 65536;
					}
					rtfBody = ((charCode <= 255) ? (rtfBody + currentChar) : (rtfBody + "\\u" + charCode.ToString("00000") + "?"));
				}
				else
				{
					rtfBody += currentChar;
				}
			}
			InString = rtfBody;
			rtfBody = lineFormat;
			for (int i = 0; i < InString.Length; i++)
			{
				currentChar = DataUtil.Mid(InString, i, 1);
				if (currentChar == "\r")
				{
					if (ShowFirstLineOnly)
					{
						rtfBody = rtfBody + "\r\n" + lineFormat;
						i = InString.Length;
					}
					else if (i == InString.Length - 1)
					{
						rtfBody = rtfBody + "\r\n" + rtfLineSpacing + lineFormat;
					}
					else
					{
						string lineSpacing = (DataUtil.Mid(InString, i + 2, 1) == DummyNotationSym) ? Gf.RTFNewLine : rtfLineSpacing;
						rtfBody = rtfBody + "\r\n" + lineSpacing + tabPrefix + lineFormat;
					}
				}
				else if ((currentChar != "\n") && (currentChar != DummyNotationSym))
				{
					rtfBody += currentChar;
				}
			}
			return rtfPrefix + rtfBody + "\r\n" + Gf.RTFNewLine;
		}

		private void LoadItem(ref SongSettings InItem)
		{
			string itemType = Gf.DocumentSongs[CurrentSong].Type;
			string inIDString = Gf.DocumentSongs[CurrentSong].IdString;
			BibleItem = false;
			Gf.InitialiseIndividualData(ref InItem);
			if (itemType == "P")
			{
				InItem.Type = itemType;
				InItem.Title = Gf.RTFCheck(Gf.DocumentSongs[CurrentSong].Title);
				InItem.CompleteLyrics = "(Powerpoint File)";
			}
			else if (itemType == "D")
			{
				Gf.LoadIndividualData(ref InItem, inIDString, "", 0);
			}
			else if (itemType == "B")
			{
				string InTitle = Gf.DocumentSongs[CurrentSong].Title;
				BibleItem = true;
				Gf.LoadIndividualData(ref InItem, inIDString, "", 0, ref InTitle);
				InItem.CompleteLyrics = InItem.CompleteLyrics.Replace('\u0098'.ToString(), " ");
			}
			else if (itemType == "T")
			{
				string InTitle = Gf.DocumentSongs[CurrentSong].Title;
				Gf.LoadIndividualData(ref InItem, inIDString, "", 0, ref InTitle);
			}
			else if (itemType == "I")
			{
				string InTitle = Gf.DocumentSongs[CurrentSong].Title;
				Gf.LoadIndividualData(ref InItem, inIDString, "", 0, ref InTitle);
				InItem.Title = InTitle;
			}
			else if (itemType == "W")
			{
				string InTitle = Gf.DocumentSongs[CurrentSong].Title;
				Gf.LoadIndividualData(ref InItem, inIDString, "", 0, ref InTitle);
			}
			InItem.Title = Gf.RTFCheck(InItem.Title);
			InItem.Title2 = Gf.RTFCheck(InItem.Title);
			InItem.CompleteLyrics = Gf.RTFCheck(InItem.CompleteLyrics);
			InItem.Copyright = Gf.RTFCheck(InItem.Writer + (((InItem.Writer != "") && (InItem.Copyright != "")) ? "; " : "") + InItem.Copyright);
			InItem.Copyright = InItem.Copyright + ((InItem.Copyright == "") ? "" : " ") + InItem.Show_LicAdminInfo1 + ((InItem.Show_LicAdminInfo1 == "") ? "" : " ") + InItem.Show_LicAdminInfo2;
			Gf.FormatDisplayLyrics(ref InItem, PrepareSlides: true, (Gf.PB_LyricsPattern > 0) ? true : false);
		}

		private string FormatNotationString(string InString, string InNotation, Font MainFont, Font NotationsFont)
		{
			Graphics graphics = CreateGraphics();
			string lyricPrefix = "";
			int prefixWidth = 0;
			string marker = "i";
			int markerWidth = (int)graphics.MeasureString(marker, NotationsFont, 1000, StringFormat.GenericTypographic).Width;
			string notationLine = marker;
			string spacedNotationLine = "";
			int padCount = 0;
			string chord = DataUtil.ExtractOneInfo(ref InNotation, ';');
			string chordPosition = DataUtil.ExtractOneInfo(ref InNotation, ';');
			while ((chord != "-1") && (chordPosition != "-1"))
			{
				lyricPrefix = DataUtil.Left(InString, Convert.ToInt32(chordPosition));
				if (DataUtil.Right(lyricPrefix, 1) == " ")
				{
					lyricPrefix = DataUtil.Left(lyricPrefix, lyricPrefix.Length - 1) + marker;
				}
				prefixWidth = (int)graphics.MeasureString(lyricPrefix, MainFont, 32000, StringFormat.GenericDefault).Width;
				while (graphics.MeasureString(notationLine, NotationsFont, 32000, StringFormat.GenericDefault).Width < (float)(prefixWidth + markerWidth))
				{
					notationLine = DataUtil.Left(notationLine, notationLine.Length - 1) + " " + marker;
					padCount++;
				}
				notationLine = DataUtil.Left(notationLine, notationLine.Length - 1) + chord + marker;
				if (Gf.PB_PrinterSpaces > 0)
				{
					int extraSpaces = padCount / 12;
					for (int i = 1; i <= padCount + extraSpaces; i++)
					{
						spacedNotationLine += " ";
					}
					spacedNotationLine += chord;
				}
				padCount = 0;
				chord = DataUtil.ExtractOneInfo(ref InNotation, ';');
				chordPosition = DataUtil.ExtractOneInfo(ref InNotation, ';');
			}
			if (DataUtil.Right(notationLine, 1) == marker)
			{
				notationLine = DataUtil.Left(notationLine, notationLine.Length - 1);
			}
			if (notationLine != "")
			{
				if (Gf.PB_PrinterSpaces > 0)
				{
					return spacedNotationLine;
				}
				return notationLine;
			}
			return " ";
		}

		            }
}
