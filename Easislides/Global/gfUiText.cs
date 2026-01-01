using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Easislides.Module;
using Easislides.Util;

namespace Easislides
{
	internal unsafe partial class gf
	{
		private enum SlideType
		{
			Chorus = 0,
			Chorus2 = 102,
			PreChorus = 111,
			PreChorus2 = 112,
			Bridge = 100,
			Bridge2 = 103,
			Ending = 101
		}
		public static void DisplayRichTextBoxSeries(ref SongSettings InItem, ref Panel InPanel, ref RichTextBox[] InRichTextBox, bool ScrollToCaret, bool PreviewNotations)
		{
			InItem.CurSlide = ((InItem.CurSlide < 1) ? 1 : ((InItem.CurSlide > InItem.TotalSlides) ? InItem.TotalSlides : InItem.CurSlide));
			InItem.FolderNo = ((InItem.FolderNo <= 0) ? 1 : InItem.FolderNo);
			if (InRichTextBox.Length <= 1 || InRichTextBox[1] == null || InItem.Type == "P")
			{
				return;
			}
			if (InItem.Slide == null)
			{
				return;
			}

			int maxSlides = InItem.Slide.GetLength(0) - 1;
			int totalSlides = (InItem.TotalSlides > maxSlides) ? maxSlides : InItem.TotalSlides;

			using (Font selectionFont = new Font("Microsoft Sans Serif", InRichTextBox[1].Font.Size, InRichTextBox[1].Font.Style))
			{
				for (int i = 1; i <= totalSlides; i++)
				{
					StringBuilder builder = new StringBuilder();
					int initialLength = builder.Length;
					int headingLength = 0;

					if (InItem.Slide[i, 0] >= 0)
					{
						if (i > 1)
						{
							builder.Append('\n');
						}

						int slideTypeValue = InItem.Slide[i, 0];
						if (slideTypeValue == (int)SlideType.Chorus)
						{
							builder.Append((FolderLyricsHeading[InItem.FolderNo, 1] != "") ? FolderLyricsHeading[InItem.FolderNo, 1] : "Chorus:");
						}
						else if (slideTypeValue == (int)SlideType.Chorus2)
						{
							builder.Append((FolderLyricsHeading[InItem.FolderNo, 1] != "") ? (FolderLyricsHeading[InItem.FolderNo, 1] + " (2)") : "Chorus 2:");
						}
						else if (slideTypeValue == (int)SlideType.PreChorus)
						{
							builder.Append((FolderLyricsHeading[InItem.FolderNo, 0] != "") ? FolderLyricsHeading[InItem.FolderNo, 0] : "Prechorus:");
						}
						else if (slideTypeValue == (int)SlideType.PreChorus2)
						{
							builder.Append((FolderLyricsHeading[InItem.FolderNo, 0] != "") ? (FolderLyricsHeading[InItem.FolderNo, 0] + " (2)") : "Prechorus 2:");
						}
						else if (slideTypeValue == (int)SlideType.Bridge)
						{
							builder.Append((FolderLyricsHeading[InItem.FolderNo, 2] != "") ? FolderLyricsHeading[InItem.FolderNo, 2] : "Bridge:");
						}
						else if (slideTypeValue == (int)SlideType.Bridge2)
						{
							builder.Append((FolderLyricsHeading[InItem.FolderNo, 2] != "") ? (FolderLyricsHeading[InItem.FolderNo, 2] + " (2)") : "Bridge 2:");
						}
						else if (slideTypeValue == (int)SlideType.Ending)
						{
							builder.Append((FolderLyricsHeading[InItem.FolderNo, 3] != "") ? FolderLyricsHeading[InItem.FolderNo, 3] : "Ending:");
						}
						else if (InItem.Verse2Present || (i > 1 && slideTypeValue == 1))
						{
							builder.Append(VerseTitle[slideTypeValue]).Append('.');
						}
						headingLength = builder.Length - initialLength;
					}

					builder.Append('\n');
					if (InItem.Slide[i, 2] >= 0)
					{
						builder.Append(GetSlideContents(InItem, i, 0, InRichTextBox[1].Font, PreviewNotations));
					}
					if (InItem.Slide[i, 4] >= 0)
					{
						builder.Append(GetSlideContents(InItem, i, 1, InRichTextBox[1].Font, PreviewNotations));
					}

					string text = DataUtil.TrimStart(builder.ToString());
					text = DataUtil.TrimEnd(text);

					if (InRichTextBox[i] != null)
					{
						InRichTextBox[i].Text = text;
						InRichTextBox[i].SelectAll();
						InRichTextBox[i].SelectionFont = selectionFont;
						InRichTextBox[i].SelectionStart = 0;
						InRichTextBox[i].SelectionLength = 0;
					}

					int totalLength = text.Length - initialLength + 1;
					InItem.Slide[i, 5] = initialLength;
					InItem.Slide[i, 6] = totalLength;
					InItem.Slide[i, 7] = headingLength;
				}
			}
			HighlightRichTextBox(ref InRichTextBox, ref InPanel, InItem, OnEnterPanel: false, ScrollToCaret);
		}

		public static void RefreshWindowsDesktop()
		{
			InvalidateRect(IntPtr.Zero, IntPtr.Zero, bErase: true);
		}

		public static void ClipboardCopyTextBox(RichTextBox InTextBox)
		{
			Clipboard.SetDataObject(InTextBox.SelectedText ?? "");
		}

		public static void ClipboardPasteTextBox(RichTextBox InTextBox, int Location)
		{
			InTextBox.SelectionLength = 0;
			InTextBox.SelectionStart = Location;
			InTextBox.Focus();
			InTextBox.Paste();
		}

		public static void ClipboardPasteTextBox(RichTextBox InTextBox, int Location, string InPasteString)
		{
			Clipboard.SetDataObject(InPasteString.Replace("\r\n", "\n").Replace("\r", "\n").Replace("\v", "\n"));
			InTextBox.SelectionLength = 0;
			InTextBox.SelectionStart = Location;
			InTextBox.Focus();
			InTextBox.Paste();
		}

		public static void InsertIndicator(ref RichTextBox InTextBox, int InNum)
		{
			int CurPos = InTextBox.SelectionStart;
			string selectedText = (InNum == 152) ? " »" : VerseSymbol[InNum];
			string text = "";
			switch (InNum)
			{
				case 151:
					GetCurPosInLine(InTextBox.Text, ref CurPos);
					InTextBox.SelectionStart = CurPos;
					break;
				case 152:
					MoveToPosInLine(InTextBox.Text, ref CurPos, 1);
					InTextBox.SelectionStart = CurPos;
					break;
				default:
					GetCurPosInLine(InTextBox.Text, ref CurPos);
					InTextBox.SelectionStart = CurPos;
					text = (((DataUtil.Mid(InTextBox.Text, CurPos, 1) == "\r") | (DataUtil.Mid(InTextBox.Text, CurPos, 1) == "\n")) ? "" : "\r\n");
					break;
			}
			InTextBox.SelectedText = selectedText;
			if (text != "")
			{
				InTextBox.SelectedText = text;
			}
			if (InNum == 152)
			{
				InTextBox.SelectionStart -= 1;
			}
		}

		public static void LoadBlankCaptureDevices(ref ToolStripComboBox InComboBoxDevice)
		{
			LoadBlankCaptureDevices(ref InComboBoxDevice, null);
		}

		public static void LoadBlankCaptureDevices(ref ToolStripComboBox InComboBoxDevice, ToolStripComboBox InComboBoxMedium)
		{
			InComboBoxDevice.Items.Clear();
			for (int i = 1; i <= 10; i++)
			{
				InComboBoxDevice.Items.Add(i + ".");
			}

			if (InComboBoxMedium != null)
			{
				InComboBoxMedium.Items.Clear();
				InComboBoxMedium.Items.Add("Video");
			}
		}

		public static void HighlightRichTextBox(ref RichTextBox[] InRichTextBox, ref Panel InPanel, SongSettings InItem, bool OnEnterPanel, bool ScrollToTop)
		{
			if (OnEnterPanel)
			{
				Control_Enter(InPanel);
			}
			else
			{
				Control_Leave(InPanel);
			}
			for (int i = 1; i <= InItem.TotalSlides; i++)
			{
				if (InRichTextBox[i] == null)
				{
					continue;
				}
				if (OnEnterPanel)
				{
					Control_Enter(InRichTextBox[i]);
					if ((string)InRichTextBox[i].Tag == InItem.CurSlide.ToString() && !InItem.GapItemOnDisplay)
					{
						InRichTextBox[i].ForeColor = TextRegionSlideTextColour;
						InRichTextBox[i].BackColor = TextRegionSlideBackColour;
						int top = InRichTextBox[i].Top;
						int num = top;
						if (ScrollToTop)
						{
							bool flag = (top <= 0) ? true : false;
							while (!flag)
							{
								SendMessage(InPanel.Handle, 277u, 3u, 0u);
								top = InRichTextBox[i].Top;
								if (top < num && top > 0)
								{
									num = top;
								}
								else
								{
									flag = true;
								}
							}
						}
						InRichTextBox[i].Focus();
						InPanel.ScrollControlIntoView(InRichTextBox[i]);
						if (!ScrollToTop && i < InItem.TotalSlides)
						{
							top = InRichTextBox[i].Top;
							int num2 = 0;
							while (top > 5 && num2 < 5)
							{
								SendMessage(InPanel.Handle, 277u, 1u, 0u);
								top = InRichTextBox[i].Top;
								num2++;
							}
						}
					}
					else
					{
						InRichTextBox[i].ForeColor = NormalTextColour;
					}
				}
				else
				{
					Control_Leave(InRichTextBox[i]);
					if ((string)InRichTextBox[i].Tag == InItem.CurSlide.ToString())
					{
						InRichTextBox[i].ForeColor = TextRegionSlideTextColour;
						InRichTextBox[i].BackColor = TextRegionSlideBackColour;
					}
					else
					{
						InRichTextBox[i].ForeColor = NormalTextColour;
					}
				}
			}
		}

		public static void Control_Enter(Control InControl)
		{
			InControl.BackColor = UseFocusedTextRegionColour ? FocusedTextRegionColour : NormalTextRegionBackColour;
			if (InControl.Name == "Main_QuickFind")
			{
				((TextBox)InControl).SelectAll();
			}
		}

		public static void Control_Leave(Control InControl)
		{
			InControl.BackColor = NormalTextRegionBackColour;
		}

		public static void SetEnterColour(ref Color InBackground)
		{
			InBackground = UseFocusedTextRegionColour ? FocusedTextRegionColour : NormalTextRegionBackColour;
		}

		public static void SetLeaveColor(ref Color InBackground)
		{
			InBackground = NormalTextRegionBackColour;
		}
	}
}
