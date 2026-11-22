using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Axicultus.Module;
using Axicultus.Util;

namespace Axicultus;

	internal unsafe partial class Gf
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
		internal static string[] BuildLyricsSlideTexts(SongSettings item, Font font, bool showNotations)
		{
			item.CurSlide = item.TotalSlides <= 0 ? 0 : Math.Clamp(item.CurSlide, 1, item.TotalSlides);
			item.FolderNo = Math.Max(1, item.FolderNo);
			if (item.Slide == null || item.Type == "P" || item.TotalSlides <= 0)
				return [];
			int totalSlides = Math.Min(item.TotalSlides, item.Slide.GetLength(0) - 1);
			var slides = new string[totalSlides];
			var builder = new StringBuilder();
			for (int i = 1; i <= totalSlides; i++)
			{
				builder.Clear();
				int headingLength = 0;

				if (item.Slide[i, 0] >= 0)
				{
					if (i > 1)
					{
						builder.Append('\n');
					}

					int slideTypeValue = item.Slide[i, 0];
					if (slideTypeValue == (int)SlideType.Chorus)
					{
						builder.Append((Folders[item.FolderNo].LyricsHeading[1] != "") ? Folders[item.FolderNo].LyricsHeading[1] : "Chorus:");
					}
					else if (slideTypeValue == (int)SlideType.Chorus2)
					{
						builder.Append((Folders[item.FolderNo].LyricsHeading[1] != "") ? (Folders[item.FolderNo].LyricsHeading[1] + " (2)") : "Chorus 2:");
					}
					else if (slideTypeValue == (int)SlideType.PreChorus)
					{
						builder.Append((Folders[item.FolderNo].LyricsHeading[0] != "") ? Folders[item.FolderNo].LyricsHeading[0] : "Prechorus:");
					}
					else if (slideTypeValue == (int)SlideType.PreChorus2)
					{
						builder.Append((Folders[item.FolderNo].LyricsHeading[0] != "") ? (Folders[item.FolderNo].LyricsHeading[0] + " (2)") : "Prechorus 2:");
					}
					else if (slideTypeValue == (int)SlideType.Bridge)
					{
						builder.Append((Folders[item.FolderNo].LyricsHeading[2] != "") ? Folders[item.FolderNo].LyricsHeading[2] : "Bridge:");
					}
					else if (slideTypeValue == (int)SlideType.Bridge2)
					{
						builder.Append((Folders[item.FolderNo].LyricsHeading[2] != "") ? (Folders[item.FolderNo].LyricsHeading[2] + " (2)") : "Bridge 2:");
					}
					else if (slideTypeValue == (int)SlideType.Ending)
					{
						builder.Append((Folders[item.FolderNo].LyricsHeading[3] != "") ? Folders[item.FolderNo].LyricsHeading[3] : "Ending:");
					}
					else if (item.Verse2Present || (i > 1 && slideTypeValue == 1))
					{
						builder.Append(VerseTitle[slideTypeValue]).Append('.');
					}
					headingLength = builder.Length;
				}

				builder.Append('\n');
				if (item.Slide[i, 2] >= 0)
				{
					builder.Append(GetSlideContents(item, i, 0, font, showNotations));
				}
				if (item.Slide[i, 4] >= 0)
				{
					builder.Append(GetSlideContents(item, i, 1, font, showNotations));
				}

				string slideText = DataUtil.TrimStart(builder.ToString());
				slideText = DataUtil.TrimEnd(slideText);

				slides[i - 1] = slideText;

				item.Slide[i, 5] = 0;
				item.Slide[i, 6] = slideText.Length + 1;
				item.Slide[i, 7] = headingLength;
			}
			return slides;
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
			string trailingLineBreak = "";
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
					// A chord line belongs to the lyric line below it: the heading goes above the two, or the save
					// would drop the chords.
					string text = InTextBox.Text;
					int aboveStart = CurPos < 2 ? 0 : text.LastIndexOf('\n', CurPos - 2) + 1;
					if (CurPos > 0 && CurPos <= text.Length && text[aboveStart..CurPos].TrimEnd().EndsWith('»'))
					{
						CurPos = aboveStart;
					}
					InTextBox.SelectionStart = CurPos;
					trailingLineBreak = (((DataUtil.Mid(InTextBox.Text, CurPos, 1) == "\r") | (DataUtil.Mid(InTextBox.Text, CurPos, 1) == "\n")) ? "" : "\r\n");
					break;
			}
			InTextBox.SelectedText = selectedText;
			if (trailingLineBreak != "")
			{
				InTextBox.SelectedText = trailingLineBreak;
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
