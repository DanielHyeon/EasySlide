using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using Easislides.Module;
using Easislides.Util;

namespace Easislides
{
	internal unsafe partial class Gf
	{

		public void FormatText(ref SongSettings InItem, Color PanelBackColour, int PanelBackColorAsScreen, Color PanelTextColor, int PaneltextColourAsRegion1)
		{
			FormatText(ref InItem, PanelBackColour, PanelBackColorAsScreen, PanelTextColor, PaneltextColourAsRegion1, UseDefault: true);
		}

		public static void FormatText(ref SongSettings InItem, Color PanelBackColour, int PanelBackColorAsScreen, Color PanelTextColor, int PaneltextColourAsRegion1, bool UseDefault)
		{
			//using var perf = PerfLog.Enabled ? PerfLog.Scope("FormatText", $"folder={(InItem != null ? InItem.FolderNo : -1)} type={(InItem != null ? InItem.Type : "null")} useDefault={UseDefault}") : null;
			var folder = Folders[InItem.FolderNo];
			var format = InItem.Format;
			ReadOnlySpan<int> fontAligns = UseDefault ? Folders[0].FontAlign : format.ShowFontAlign;
			ReadOnlySpan<int> fontBolds = UseDefault ? folder.FontBold : format.ShowFontBold;
			ReadOnlySpan<int> fontItalics = UseDefault ? folder.FontItalic : format.ShowFontItalic;
			ReadOnlySpan<int> fontUnderlines = UseDefault ? folder.FontUnderline : format.ShowFontUnderline;
			ReadOnlySpan<string> fontNames = UseDefault ? folder.FontName : format.ShowFontName;
			ReadOnlySpan<int> fontSizes = UseDefault && InItem.Type != "B" ? folder.FontSize : format.ShowFontSize;
			ReadOnlySpan<int> fontVPositions = UseDefault ? folder.FontVPosition : format.ShowFontVPosition;
			int buffer_LS_Width = Buffer_LS_Width;
			int buffer_LS_Height = Buffer_LS_Height;
			double previewFactor = PreviewSampleFactor;
			int leftMargin = UseDefault ? folder.LeftMargin : format.ShowLeftMargin;
			int rightMargin = UseDefault ? folder.RightMargin : format.ShowRightMargin;
			int bottomMargin = UseDefault ? folder.BottomMargin : format.ShowBottomMargin;
			int headingAlign = (UseDefault ? ShowSongHeadingsAlign : format.ShowSongHeadingsAlign) switch
			{
				1 => fontAligns[1],
				2 => 1,
				3 => 2,
				4 => 3,
				_ => fontAligns[0]
			};
			int headingOption = folder.HeadingOption;
			bool headingBold = folder.HeadingFontBold[0] > 0;
			bool headingItalic = folder.HeadingFontItalic[0] > 0;
			bool headingUnderline = folder.HeadingFontUnderline[0] > 0;
			bool headingChorusItalic = folder.HeadingFontItalic[1] > 0;
			int lyricsLeft = leftMargin * buffer_LS_Width / 100;
			int lyricsWidth = buffer_LS_Width - (lyricsLeft + rightMargin * buffer_LS_Width / 100);
			int lyricsAreaHeight = buffer_LS_Height - (int)(TopBorderFactor * (double)buffer_LS_Height + (BottomBorderFactor + 0.029999999329447746) * (double)buffer_LS_Height + (double)(bottomMargin * buffer_LS_Height / 100));
			ShowTopBorderSize = (int)((double)buffer_LS_Height * TopBorderFactor);

			using var image = new Bitmap(lyricsWidth, 1, PixelFormat.Format24bppRgb);
			using var graphics = Graphics.FromImage(image);
			SizeF layoutArea = new SizeF(lyricsWidth, 32000f);
			int i;
			FontStyle fontStyle;
			int fontSize;
			double previewFontSize;
			for (i = 0; i <= 2; i++)
			{
				int formatIndex = (i < 2) ? i : 0;
				fontStyle = FontStyle.Regular;
				FontStyle chorusFontStyle = FontStyle.Regular;
				if (i == 2 && headingOption != 0)
				{
					if (headingOption == 2)
					{
						if (headingBold)
						{
							fontStyle |= FontStyle.Bold;
							chorusFontStyle |= FontStyle.Bold;
						}
						if (headingItalic)
						{
							fontStyle |= FontStyle.Italic;
						}
						if (headingChorusItalic)
						{
							chorusFontStyle |= FontStyle.Italic;
						}
						if (headingUnderline)
						{
							fontStyle |= FontStyle.Underline;
							chorusFontStyle |= FontStyle.Underline;
						}
					}
					else
					{
						if (fontBolds[formatIndex] > 0 || headingBold)
						{
							fontStyle |= FontStyle.Bold;
							chorusFontStyle |= FontStyle.Bold;
						}
						if (fontItalics[formatIndex] > 0 || headingItalic)
						{
							fontStyle |= FontStyle.Italic;
						}
						if (fontItalics[formatIndex] > 0 || headingChorusItalic)
						{
							chorusFontStyle |= FontStyle.Italic;
						}
						if (fontUnderlines[formatIndex] > 0 || headingUnderline)
						{
							fontStyle |= FontStyle.Underline;
							chorusFontStyle |= FontStyle.Underline;
						}
					}
				}
				else
				{
					if (fontBolds[formatIndex] > 0)
					{
						fontStyle |= FontStyle.Bold;
						chorusFontStyle |= FontStyle.Bold;
					}
					if (fontItalics[formatIndex] > 0)
					{
						fontStyle |= FontStyle.Italic;
					}
					if (fontUnderlines[formatIndex] > 0)
					{
						fontStyle |= FontStyle.Underline;
						chorusFontStyle |= FontStyle.Underline;
					}
					if (fontItalics[formatIndex] > 0 || fontItalics[formatIndex + 2] > 0)
					{
						chorusFontStyle |= FontStyle.Italic;
					}
				}
				InItem.Lyrics[i].ForeColour = UseDefault ? ShowFontColour[formatIndex].ToArgb() : format.ShowFontColour[formatIndex];
				switch ((i == 2) ? headingAlign : fontAligns[formatIndex])
				{
					case 3:
						InItem.Lyrics[i].TextAlign = StringAlignment.Far;
						break;
					case 1:
						InItem.Lyrics[i].TextAlign = StringAlignment.Near;
						break;
					default:
						InItem.Lyrics[i].TextAlign = StringAlignment.Center;
						break;
				}
				fontSize = DisplayFontSize(fontSizes[formatIndex], buffer_LS_Width, i, InItem.FolderNo);
				previewFontSize = (double)fontSize / previewFactor;
				try
				{
					InItem.Lyrics[i].FS_Font = new Font(fontNames[formatIndex], fontSize, fontStyle);
					InItem.Lyrics[i].FS_ChorusFont = new Font(fontNames[formatIndex], fontSize, chorusFontStyle);
				}
				catch
				{
					InItem.Lyrics[i].FS_Font = new Font("Microsoft Sans Serif", fontSize, fontStyle);
					InItem.Lyrics[i].FS_ChorusFont = new Font("Microsoft Sans Serif", fontSize, chorusFontStyle);
				}
				try
				{
					InItem.Lyrics[i].Font = new Font(fontNames[formatIndex], (float)((previewFontSize > 0.0) ? previewFontSize : 1.0), fontStyle);
					InItem.Lyrics[i].ChorusFont = new Font(fontNames[formatIndex], (float)((previewFontSize > 0.0) ? previewFontSize : 1.0), chorusFontStyle);
				}
				catch
				{
					InItem.Lyrics[i].Font = new Font("Microsoft Sans Serif", (float)((previewFontSize > 0.0) ? previewFontSize : 1.0), fontStyle);
					InItem.Lyrics[i].ChorusFont = new Font("Microsoft Sans Serif", (float)((previewFontSize > 0.0) ? previewFontSize : 1.0), chorusFontStyle);
				}
			}
			double headingHeight = (double)graphics.MeasureString("A", InItem.Lyrics[2].FS_Font, layoutArea).Height * 1.1;
			if (headingHeight > (double)lyricsAreaHeight / 4.0)
			{
				headingHeight = (double)lyricsAreaHeight / 4.0;
			}
			for (i = 0; i <= 2; i++)
			{
				InItem.Lyrics[i].Visible = false;
				InItem.Lyrics[i].FS_Width = lyricsWidth;
				InItem.Lyrics[i].FS_Left = lyricsLeft;
				InItem.Lyrics[i].FS_Top = SetLyricsTopPos(fontVPositions[(i < 2) ? i : 0], buffer_LS_Height) + ((i == 0) ? ((int)headingHeight) : 0);
				if (i == 2)
				{
					InItem.Lyrics[i].FS_Height = (int)headingHeight;
					InItem.Lyrics[i].FS_Height_R2Bound = InItem.Lyrics[i].FS_Height;
					InItem.Lyrics[0].FS_Height_R2Bound = InItem.Lyrics[1].FS_Top - InItem.Lyrics[0].FS_Top;
					InItem.Lyrics[0].Height_R2Bound = Convert.ToInt32((double)InItem.Lyrics[0].FS_Height_R2Bound / previewFactor);
				}
				else
				{
					InItem.Lyrics[i].FS_Height = lyricsAreaHeight - InItem.Lyrics[i].FS_Top;
					InItem.Lyrics[i].FS_Height_R2Bound = InItem.Lyrics[i].FS_Height;
				}
				InItem.Lyrics[i].Width = Convert.ToInt32((double)InItem.Lyrics[i].FS_Width / previewFactor);
				InItem.Lyrics[i].Left = Convert.ToInt32((double)InItem.Lyrics[i].FS_Left / previewFactor);
				InItem.Lyrics[i].Top = Convert.ToInt32((double)InItem.Lyrics[i].FS_Top / previewFactor);
				InItem.Lyrics[i].Height = Convert.ToInt32((double)InItem.Lyrics[i].FS_Height / previewFactor);
				InItem.Lyrics[i].Height_R2Bound = Convert.ToInt32((double)InItem.Lyrics[i].FS_Height_R2Bound / previewFactor);
			}
			i = 3;
			InItem.Lyrics[i].BackColour = PanelBackColour.ToArgb();
			InItem.Lyrics[i].Transparent = ((PanelBackColorAsScreen > 0) ? true : false);
			InItem.Lyrics[i].ForeColour = ((PaneltextColourAsRegion1 > 0) ? InItem.Lyrics[0].ForeColour : PanelTextColour.ToArgb());
			InItem.Lyrics[i].FS_Width = buffer_LS_Width - buffer_LS_Width / 50;
			InItem.Lyrics[i].FS_Left = (buffer_LS_Width - InItem.Lyrics[i].FS_Width) / 2;
			fontStyle = FontStyle.Regular;
			if (ShowDataDisplayFontBold > 0)
			{
				fontStyle |= FontStyle.Bold;
			}
			if (ShowDataDisplayFontItalic > 0)
			{
				fontStyle |= FontStyle.Italic;
			}
			if (ShowDataDisplayFontUnderline > 0)
			{
				fontStyle |= FontStyle.Underline;
			}
			InItem.Lyrics[i].TextAlign = StringAlignment.Near;
			double bottomBorderHeight = BottomBorderFactor * (double)buffer_LS_Height;
			InItem.Lyrics[i].FS_Height = (int)(bottomBorderHeight * 0.8);
			InItem.Lyrics[i].FS_Top = buffer_LS_Height - InItem.Lyrics[i].FS_Height;
			fontSize = 40;
			Font font;
			try
			{
				font = new Font(ShowDataDisplayFontName, fontSize, fontStyle);
			}
			catch
			{
				font = new Font("Microsoft Sans Serif", fontSize, fontStyle);
			}
			while (graphics.MeasureString("A", font, layoutArea).Height > (float)(InItem.Lyrics[i].FS_Height * 9 / 20) && font.Size > 1f)
			{
				fontSize--;
				font = new Font(font.Name, fontSize, fontStyle);
			}
			fontSize = (int)font.Size + 1;
			previewFontSize = (double)font.Size / previewFactor;
			InItem.Lyrics[i].FS_Height_R2Bound = InItem.Lyrics[i].FS_Height;
			InItem.Lyrics[i].Width = (int)((double)InItem.Lyrics[i].FS_Width / previewFactor);
			InItem.Lyrics[i].Left = (int)((double)InItem.Lyrics[i].FS_Left / previewFactor);
			InItem.Lyrics[i].Top = (int)((double)InItem.Lyrics[i].FS_Top / previewFactor);
			InItem.Lyrics[i].Height = (int)((double)InItem.Lyrics[i].FS_Height / previewFactor);
			InItem.Lyrics[i].Height_R2Bound = (int)((double)InItem.Lyrics[i].FS_Height_R2Bound / previewFactor);
			InItem.Lyrics[i].FS_Font = new Font(font.Name, fontSize, fontStyle);
			InItem.Lyrics[i].Font = new Font(font.Name, (!(previewFontSize > 0.0)) ? 1 : ((int)previewFontSize), fontStyle);
			//graphics.Dispose();
			//image.Dispose();
		}

		public static string RTFFormatNotationString(string InText, string InNotationString, Font MainFont, Font NotationFont)
		{
			string remainingNotations = "";
			string chord = "";
			int lyricsPos = 0;
			int targetPosX = 0;
			string notationLine = "";
			tbWorkspace.Font = MainFont;
			tbWorkspace.WordWrap = false;
			tbWorkspace.Text = InText;
			tbTempSpace.Font = NotationFont;
			tbTempSpace.WordWrap = false;
			tbTempSpace.Text = "";
			while (InNotationString != "")
			{
				remainingNotations = InNotationString;
				chord = DataUtil.ExtractOneInfo(ref remainingNotations, ';');
				lyricsPos = Convert.ToInt32(DataUtil.ExtractOneInfo(ref remainingNotations, ';'));
				InNotationString = remainingNotations;
				targetPosX = GetAssociatedLyricsLineCurPosX(ref tbWorkspace, lyricsPos, 0);
				while (GetAssociatedLyricsLineCurPosX(ref tbTempSpace, tbTempSpace.Text.Length - 1) < targetPosX - 1)
				{
					notationLine += " ";
					tbTempSpace.Text = notationLine;
					MarkSelectedRTB(ref tbTempSpace, 0, tbTempSpace.Text.Length, 2, MainFont, NotationFont);
				}
				notationLine += ((notationLine.Length > 1 && DataUtil.Right(notationLine, 1) != " ") ? (" " + chord) : chord);
				tbTempSpace.Text = notationLine;
				MarkSelectedRTB(ref tbTempSpace, 0, tbTempSpace.Text.Length, 2, MainFont, NotationFont);
			}
			return notationLine;
		}

		// Keep offsets in the original RichEdit text when translating chord positions to lyric columns.
		internal static void ReadEditorNotations(RichTextBox editor, out string notations, out string lyrics)
		{
			// Before handle creation Text can still contain CRLF, whereas native caret offsets count LF.
			_ = editor.Handle;
			string text = editor.Text;
			notations = string.Empty;
			lyrics = text;
			if (!text.Contains('»'))
				return;

			var lyricText = new StringBuilder(text.Length);
			var records = new StringBuilder();
			int lyricRow = 0;
			int start = 0;
			while (start < text.Length)
			{
				int end = text.IndexOf('\n', start);
				if (end < 0)
					end = text.Length;
				var line = text.AsSpan(start, end - start);
				if (line.Contains('»') && end + 1 < text.Length)
				{
					int nextStart = end + 1;
					int nextEnd = text.IndexOf('\n', nextStart);
					if (nextEnd < 0)
						nextEnd = text.Length;
					var next = text.AsSpan(nextStart, nextEnd - nextStart);
					if (!next.IsEmpty && !next.Contains('»'))
					{
						records.Append('(').Append(lyricRow).Append(';');
						AppendEditorChords(records, editor, line, start, nextStart, next.Length);
						records.Append(')');
						line = next;
						end = nextEnd;
					}
				}

				if (line.Contains('»'))
					lyricText.Append(line.ToString().Replace(" »", "").Replace(" »", "").Replace("»", ""));
				else
					lyricText.Append(line);
				if (end < text.Length)
					lyricText.Append('\n');
				start = end + 1;
				lyricRow++;
			}
			notations = records.ToString();
			lyrics = lyricText.ToString();
		}

		private static void AppendEditorChords(StringBuilder records, RichTextBox editor, ReadOnlySpan<char> line,
			int chordLineStart, int lyricStart, int lyricLength)
		{
			int offset = 0;
			int previousX = int.MinValue;
			for (int start = 0; start < line.Length;)
			{
				if (line[start] is ' ' or '»')
				{
					start++;
					continue;
				}
				int length = line[start..].IndexOfAny(' ', '»');
				if (length < 0)
					break;
				int x = editor.GetPositionFromCharIndex(chordLineStart + start).X;
				if (x < previousX)
					offset = 0;
				while (offset <= lyricLength && editor.GetPositionFromCharIndex(lyricStart + offset).X <= x)
					offset++;
				previousX = x;
				records.Append(line.Slice(start, length)).Append(';').Append(Math.Max(0, offset - 1)).Append(';');
				start += length;
			}
		}

		internal static void TransposeEditorRegion(RichTextBox editor, int steps, int spelling, Font lyricFont,
			Font chordFont, RichTextBox workspace, RichTextBox scratch, out string notations, out string lyrics)
		{
			ReadEditorNotations(editor, out notations, out lyrics);
			if (notations.Length == 0)
			{
				if (editor.Text != lyrics)
					editor.Text = lyrics;
				return;
			}
			notations = TransposeNotations(notations, steps, spelling);
			string display = CombineLyricsAndNotations(lyrics, notations, lyricFont, chordFont, ref workspace, ref scratch);
			editor.Text = lyrics.EndsWith('\n') ? display + "\n" : display;
		}

		public static string CombineLyricsAndNotations(string InText, string InNotations, Font MainFont, Font NotationFont, ref RichTextBox InWorkspace, ref RichTextBox InTempSpace)
		{
			if (string.IsNullOrEmpty(InNotations) || string.IsNullOrEmpty(InText))
				return InText;

			InWorkspace.WordWrap = false;
			InWorkspace.Text = InText;
			InWorkspace.SelectAll();
			InWorkspace.SelectionFont = MainFont;
			InTempSpace.WordWrap = false;
			string lyrics = InWorkspace.Text;
			int lineCount = DataUtil.CountLf(lyrics) + (lyrics.EndsWith('\n') ? 0 : 1);
			var notationsByLine = GetNotationsByLine(InNotations, lineCount);
			var result = new StringBuilder(lyrics.Length + InNotations.Length);
			var chordLine = new StringBuilder();
			int lineStart = 0;
			for (int row = 0; row < lineCount; row++)
			{
				int lineEnd = lyrics.IndexOf('\n', lineStart);
				if (lineEnd < 0)
					lineEnd = lyrics.Length;
				if (notationsByLine.TryGetValue(row, out string notation) && notation.Length > 0)
				{
					chordLine.Clear();
					ReadOnlySpan<char> fields = notation;
					while (!fields.IsEmpty)
					{
						var chord = TakeNotationField(ref fields);
						int position = int.Parse(TakeNotationField(ref fields));
						int target = GetAssociatedLyricsLineCurPosX(ref InWorkspace, position, lineStart, lineEnd - 1) - 4;
						int padding = GetRichTextChordPadding(InTempSpace, NotationFont, chordLine, target);
						chordLine.Append(' ', padding);
						if (chordLine.Length > 1 && chordLine[^1] != ' ')
							chordLine.Append(' ');
						chordLine.Append(chord);
					}
					result.Append(chordLine).Append(" »\n");
				}
				result.Append(lyrics.AsSpan(lineStart, lineEnd - lineStart));
				if (row + 1 < lineCount)
					result.Append('\n');
				lineStart = lineEnd + 1;
			}
			return result.ToString();
		}

		private static int GetRichTextChordPadding(RichTextBox scratch, Font chordFont, StringBuilder line, int targetX)
		{
			if (targetX <= 0)
				return 0;

			// RichEdit aligns on the start of the last character, including for a single space.
			// Lay out a run once and look up the first caret position which reaches the target.
			int prefix = line.Length;
			int high = Math.Max(8, targetX / 4 + 2);
			int low = 0;
			int RightEdge(int spaces) => scratch.GetPositionFromCharIndex(Math.Max(0, prefix + spaces - 1)).X;
			while (true)
			{
				line.Append(' ', high);
				scratch.Text = line.ToString();
				// Set the character format explicitly. RichEdit's default font can substitute
				// different advances after Korean or right-to-left text.
				scratch.SelectAll();
				scratch.SelectionFont = chordFont;
				line.Length = prefix;
				if (RightEdge(0) >= targetX)
					return 0;
				int edge = RightEdge(high);
				if (edge >= targetX)
					break;
				if (low > 0 && edge <= RightEdge(low))
					return low;
				low = high;
				high *= 2;
			}
			while (high - low > 1)
			{
				int mid = low + (high - low) / 2;
				if (RightEdge(mid) < targetX)
					low = mid;
				else
					high = mid;
			}
			return high;
		}

		// The slide count can change when the operator toggles paragraph fitting mid-item.
		// Locate its first text line again even if repeated blank separators have collapsed.
		internal static bool RepaginateTextFile(ref SongSettings item)
		{
			if (item.Type != "T")
				return false;

			int firstLine = 0;
			int column = 1;
			if (item.CurSlide > 0 && item.CurSlide <= item.TotalSlides)
			{
				column = item.Slide[item.CurSlide, 1] >= 0 ? 1 : 3;
				firstLine = item.Slide[item.CurSlide, column];
			}
			var rows = item.LyricsAndNotationsList.Items;
			int linesBefore = 0;
			for (int i = 0; i < firstLine; i++)
				if (!string.IsNullOrWhiteSpace(rows[i].SubItems[2].Text))
					linesBefore++;

			FormatDisplayLyrics(ref item, PrepareSlides: true, UseStoredSequence: true);
			item.CurSlide = Math.Clamp(item.CurSlide, 1, Math.Max(1, item.TotalSlides));
			for (int line = 0; line < rows.Count; line++)
			{
				if (string.IsNullOrWhiteSpace(rows[line].SubItems[2].Text) || linesBefore-- > 0)
					continue;
				for (int slide = 1; slide <= item.TotalSlides; slide++)
				{
					if (item.Slide[slide, column] >= 0 && item.Slide[slide, column] <= line && line <= item.Slide[slide, column + 1])
					{
						item.CurSlide = slide;
						break;
					}
				}
				break;
			}
			return true;
		}

		private static bool FitWholeLyricPage(SongSettings item) =>
			item.Type != "B" && (!AutoTextOverflow || item.RotateStyle == 2 || (item.Type == "T" && FitTextFileParagraphs));

		public static void FormatDisplayLyrics(ref SongSettings InItem, bool PrepareSlides, bool UseStoredSequence)
		{
			//using var perf = PerfLog.Enabled ? PerfLog.Scope("FormatDisplayLyrics", $"prepare={PrepareSlides} useStored={UseStoredSequence} lyricsLen={(InItem != null && InItem.CompleteLyrics != null ? InItem.CompleteLyrics.Length : 0)}") : null;
			int showNotations = InItem.UseDefaultFormat ? ShowNotations : InItem.Format.ShowNotations;
			int transposeOffset = (!InItem.UseDefaultFormat) ? InItem.Format.TransposeOffset : ((ShowCapoZero == 1) ? IncrementChord(ref InItem.Capo, 0) : 0);
			ListViewItem listViewItem;
			InItem.CompleteLyrics = InItem.CompleteLyrics.Replace("\r\n", "\n");
			if (InItem != null && InItem.Capo < 0)
			{
				InItem.Capo = -1;
			}
			if (ShowRunning_ShowNotations == 1)
			{
				showNotations = ((showNotations <= 0) ? 1 : 0);
			}
			if (showNotations == 1)
			{
				if (transposeOffset > 0)
				{
					InItem.Notations = TransposeNotations(InItem.OriginalNotations, InItem.Format.PreviousTransposeOffset, transposeOffset, InItem.MusicKey);
				}
				else
				{
					InItem.Notations = InItem.OriginalNotations;
				}
			}
			Array.Clear(InItem.VersePresent);
			bool fitParagraphs = InItem.Type == "T" && FitTextFileParagraphs;
			string lyrics = fitParagraphs ? InItem.CompleteLyrics.Replace('\r', '\n') : InItem.CompleteLyrics;
			using ListView listView = ExtractLyrics(lyrics, InItem.Notations);
			InItem.LyricsAndNotationsList.Items.Clear();
			int highestVerse = 1;
			if (InItem.RotateStyle == 2)
			{
				InItem.SongSequence = InItem.RotateSequence;
			}
			InItem.SongBasicSequence = "";
			for (int i = 0; i < listView.Items.Count; i++)
			{
				var row = listView.Items[i];
				if (fitParagraphs && string.IsNullOrWhiteSpace(row.SubItems[2].Text))
					row.SubItems[2].Text = "";
				int verseNumber = DataUtil.StringToInt(row.Text);
				if (!InItem.VersePresent[verseNumber])
				{
					InItem.VersePresent[verseNumber] = true;
					InItem.SongBasicSequence += (char)verseNumber;
					highestVerse = Math.Max(highestVerse, verseNumber);
				}
			}
			for (int i = 0; i <= highestVerse; i++)
			{
				if (!InItem.VersePresent[i])
				{
					continue;
				}
				for (int j = 0; j < listView.Items.Count; j++)
				{
					if (listView.Items[j].SubItems[0].Text == i.ToString() && listView.Items[j].SubItems[1].Text == "1")
					{
						string verseKey = i.ToString();
						listViewItem = InItem.LyricsAndNotationsList.Items.Add(verseKey);
						listViewItem.SubItems.Add("1");
						listViewItem.SubItems.Add(listView.Items[j].SubItems[2].Text);
						listViewItem.SubItems.Add(listView.Items[j].SubItems[3].Text);
						listViewItem.SubItems.Add("");
					}
				}
				for (int j = 0; j < listView.Items.Count; j++)
				{
					if (listView.Items[j].SubItems[0].Text == i.ToString() && listView.Items[j].SubItems[1].Text == "2")
					{
						string verseKey = i.ToString();
						listViewItem = InItem.LyricsAndNotationsList.Items.Add(verseKey);
						listViewItem.SubItems.Add("2");
						listViewItem.SubItems.Add(listView.Items[j].SubItems[2].Text);
						listViewItem.SubItems.Add(listView.Items[j].SubItems[3].Text);
						listViewItem.SubItems.Add("");
						InItem.VersePresent[150] = true;
					}
				}
			}
			for (int i = InItem.LyricsAndNotationsList.Items.Count - 1; i >= 0; i--)
			{
				if (InItem.LyricsAndNotationsList.Items[i].SubItems[2].Text == "")
				{
					InItem.LyricsAndNotationsList.Items[i].Remove();
				}
				else
				{
					i = 0;
				}
			}
			for (int i = InItem.LyricsAndNotationsList.Items.Count - 1; i >= 1; i--)
			{
				if (InItem.LyricsAndNotationsList.Items[i].SubItems[2].Text == "" && InItem.LyricsAndNotationsList.Items[i - 1].SubItems[2].Text == "")
				{
					InItem.LyricsAndNotationsList.Items[i].Remove();
				}
			}
			if (InItem.LyricsAndNotationsList.Items.Count > 0 && InItem.LyricsAndNotationsList.Items[0].SubItems[2].Text == "")
			{
				InItem.LyricsAndNotationsList.Items[0].Remove();
			}
			if (highestVerse >= InItem.VerseLineLoc.GetLength(0))
				InItem.VerseLineLoc = new int[highestVerse + 1, 5];
			var locations = InItem.VerseLineLoc;
			for (int verse = 0; verse < locations.GetLength(0); verse++)
			{
				for (int column = 0; column < 5; column++)
					locations[verse, column] = -1;
			}
			for (int lineIndex = 0; lineIndex < InItem.LyricsAndNotationsList.Items.Count; lineIndex++)
			{
				var line = InItem.LyricsAndNotationsList.Items[lineIndex];
				int verse = DataUtil.StringToInt(line.Text);
				int endColumn = DataUtil.StringToInt(line.SubItems[1].Text) * 2;
				if (locations[verse, endColumn - 1] < 0)
					locations[verse, endColumn - 1] = lineIndex;
				locations[verse, endColumn] = lineIndex;
			}
			if (PrepareSlides)
			{
				if (UseStoredSequence)
				{
					ValidateSequence(ref InItem);
				}
				else
				{
					InItem.SongSequence = InItem.SongBasicSequence;
				}
				BuildSlides(InItem, InItem.LyricsAndNotationsList, ref InItem.SongSequence, ref InItem.TotalSlides, ref InItem.SongVerses, ref InItem.ChorusSlides, ref InItem.Slide, showNotations);
				if (InItem == null)
				{
				}
			}
			else
			{
				InItem.SongSequence = InItem.SongBasicSequence;
			}
		}

		public static string TransposeNotations(string InNotationsData, int PreviousTransposeTo, int TransposeTo, string StoredMusicKey)
		{
			if (PreviousTransposeTo > TransposeTo)
			{
				TransposeTo -= 12;
			}
			int flatSharpKey = TransposeKey(ref StoredMusicKey, TransposeTo);
			return TransposeNotations(InNotationsData, TransposeTo, flatSharpKey);
		}

		internal static string TransposeNotations(string data, int steps, int spelling)
		{
			ReadOnlySpan<char> remaining = data;
			if (!TryReadNotationRecord(ref remaining, out int lineNumber, out var chords) || lineNumber < 0)
				return string.Empty;
			var result = new StringBuilder();
			do
			{
				result.Append('(').Append(lineNumber).Append(';');
				AppendTransposedChords(result, chords, steps, spelling);
				result.Append(')');
			} while (TryReadNotationRecord(ref remaining, out lineNumber, out chords) && lineNumber >= 0);
			return result.ToString();
		}

		internal static Dictionary<int, string> GetNotationsByLine(string notationData, int lineCount)
		{
			var notations = new Dictionary<int, string>();
			ReadOnlySpan<char> remaining = notationData;
			while (TryReadNotationRecord(ref remaining, out int lineNumber, out var chords) && lineNumber >= 0)
			{
				if (lineNumber < lineCount)
					notations[lineNumber] = chords.ToString();
			}
			return notations;
		}

		public static int Load32HeaderData(string InFileName, string InContents, ref string[] ThisHeaderData)
		{
			return Load32HeaderData(InFileName, InContents, ref ThisHeaderData, '>');
		}

		public static int Load32HeaderData(string InFileName, string InContents, ref string[] ThisHeaderData, char SeparatorSym)
		{
			for (int i = 0; i < 255; i++)
			{
				ThisHeaderData[i] = "";
			}
			int openPos = InContents.IndexOf("[");
			int closePos = InContents.IndexOf("]", openPos + 1);
			if (openPos == 0 && closePos > 1)
			{
				openPos++;
				int dataStart = closePos + 1;
				string InFormatString = DataUtil.Mid(InContents, openPos, closePos - openPos + 1);
				if (DataUtil.Convertv32FormatString(ref InFormatString, SeparatorSym) == 320)
				{
					LoadHeaderData(InFormatString, ref ThisHeaderData, '>');
					return closePos;
				}
			}
			openPos = 0;
			closePos = 0;
			InContents = "";
			return closePos;
		}

		public static string ExtractHeaderInfo(string InString, int Index, char Separator)
		{
			// A key counts only at the start of the string or right after a separator (as LoadHeaderData reads
			// them): "1=" must not match the end of "21=", which gave keys 1..9 the value of 11..99 when those
			// came first.
			string key = Index + "=";
			int valueStart = InString.IndexOf(key);
			while (valueStart > 0 && InString[valueStart - 1] != Separator)
			{
				valueStart = InString.IndexOf(key, valueStart + 1);
			}
			if (valueStart >= 0)
			{
				valueStart += Index.ToString().Length + 1;
				int valueEnd = InString.IndexOf(Separator, valueStart);
				if (valueEnd <= valueStart)
				{
					return "";
				}
				return DataUtil.Mid(InString, valueStart, valueEnd - valueStart);
			}
			return "";
		}


		public static int LoadHeaderData(string InFormatString, ref string[] ThisHeaderData, char SeparatorSym)
		{
			for (int index = 0; index < HeaderDataMax; index++)
				ThisHeaderData[index] = string.Empty;
			if (InFormatString == "" || InFormatString[0] == '[')
			{
				return -1;
			}
			// Stored session fields always use '>' and the last occurrence of a key wins.
			ReadOnlySpan<char> header = InFormatString;
			foreach (Range field in header.Split('>'))
			{
				var entry = header[field];
				int equals = entry.IndexOf('=');
				var key = equals < 0 ? entry : entry[..equals];
				if (int.TryParse(key, out int index) && index > 0 && index < HeaderDataMax)
				{
					ThisHeaderData[index] = equals < 0 ? string.Empty : entry[(equals + 1)..].ToString();
				}
			}
			return 1;
		}

		public static void ApplyHeaderData()
		{
			ApplyHeaderData(0);
		}

		public static void ApplyHeaderData(int InMode)
		{
			static int GetNumeric(int index) => ExtractNumericData(HeaderData[index]);
			int headerValue = IndexFileVersion = GetNumeric(1);
			if (InMode != 1)
			{
				headerValue = GetNumeric(11);
				PanelBackColour = ((HeaderData[11] == "" || !ValidateColour(headerValue)) ? DefaultBackColour : Color.FromArgb(Convert.ToInt32(headerValue)));
				headerValue = GetNumeric(12);
				PanelBackColourTransparent = ((headerValue < 0 || headerValue > 1) ? 1 : headerValue);
				headerValue = GetNumeric(13);
				PanelTextColour = ((HeaderData[13] == "" || !ValidateColour(headerValue)) ? DefaultForeColour : Color.FromArgb(Convert.ToInt32(headerValue)));
				headerValue = GetNumeric(14);
				PanelTextColourAsRegion1 = ((headerValue < 0 || headerValue > 1) ? 1 : headerValue);
				headerValue = GetNumeric(15);
				headerValue = ((headerValue < 0) ? 31 : headerValue);
				ShowDataDisplayMode = DataUtil.GetBitValue(headerValue, 1);
				ShowDataDisplaySlides = DataUtil.GetBitValue(headerValue, 2);
				ShowDataDisplaySongs = DataUtil.GetBitValue(headerValue, 3);
				ShowDataDisplayTitle = DataUtil.GetBitValue(headerValue, 4);
				ShowDataDisplayCopyright = DataUtil.GetBitValue(headerValue, 5);
				ShowDataDisplayPrevNext = DataUtil.GetBitValue(headerValue, 6);
				headerValue = GetNumeric(16);
				headerValue = ((headerValue < 6 || headerValue > 20) ? 8 : headerValue);
				BottomBorderFactor = (double)headerValue / 100.0;
				ShowDataDisplayFontName = HeaderData[17];
				if (ShowDataDisplayFontName == "")
				{
					ShowDataDisplayFontName = "Microsoft Sans Serif";
				}
				headerValue = GetNumeric(18);
				headerValue = ((headerValue >= 0) ? headerValue : 0);
				ShowDataDisplayFontBold = DataUtil.GetBitValue(headerValue, 1);
				ShowDataDisplayFontItalic = DataUtil.GetBitValue(headerValue, 2);
				ShowDataDisplayFontUnderline = DataUtil.GetBitValue(headerValue, 3);
				ShowDataDisplayFontShadow = DataUtil.GetBitValue(headerValue, 4);
				ShowDataDisplayFontOutline = DataUtil.GetBitValue(headerValue, 5);
				headerValue = GetNumeric(19);
				ShowDataDisplayIndicatorsFontSize = ((headerValue < 8 || headerValue > 20) ? 8 : headerValue);
				headerValue = GetNumeric(21);
				ShowSongHeadings = ((headerValue < 0 || headerValue > 3) ? 1 : headerValue);
				headerValue = GetNumeric(22);
				headerValue = ((headerValue < 0) ? 2 : headerValue);
				UseShadowFont = DataUtil.GetBitValue(headerValue, 2);
				ShowNotations = DataUtil.GetBitValue(headerValue, 3);
				ShowCapoZero = DataUtil.GetBitValue(headerValue, 4);
				ShowInterlace = DataUtil.GetBitValue(headerValue, 5);
				UseOutlineFont = DataUtil.GetBitValue(headerValue, 6);
				headerValue = GetNumeric(23);
				ShowSongHeadingsAlign = ((!(headerValue < 0 || headerValue > 4)) ? headerValue : 0);
				headerValue = GetNumeric(25);
				ShowLyrics = ((headerValue < 0 || headerValue > 2) ? 2 : headerValue);
				headerValue = GetNumeric(26);
				ShowScreenColour[0] = ((HeaderData[26] == "" || !ValidateColour(headerValue)) ? DefaultBackColour : Color.FromArgb(Convert.ToInt32(headerValue)));
				headerValue = GetNumeric(27);
				ShowScreenColour[1] = ((HeaderData[27] == "" || !ValidateColour(headerValue)) ? ShowScreenColour[0] : Color.FromArgb(Convert.ToInt32(headerValue)));
				headerValue = GetNumeric(28);
				ShowScreenStyle = ((!(headerValue < 0 || headerValue > MaxBackgroundStyleIndex)) ? headerValue : 0);
				headerValue = GetNumeric(29);
				ShowFontColour[0] = ((HeaderData[29] == "" || !ValidateColour(headerValue)) ? DefaultForeColour : Color.FromArgb(Convert.ToInt32(headerValue)));
				headerValue = GetNumeric(30);
				ShowFontColour[1] = ((HeaderData[30] == "" || !ValidateColour(headerValue)) ? DefaultForeColour : Color.FromArgb(Convert.ToInt32(headerValue)));
				headerValue = GetNumeric(31);
				Folders[0].FontAlign[0] = ((headerValue < 0 || headerValue > 3) ? 2 : headerValue);
				headerValue = GetNumeric(32);
				Folders[0].FontAlign[1] = ((headerValue < 0 || headerValue > 3) ? 2 : headerValue);
				headerValue = GetNumeric(50);
				MediaOption = ((!(headerValue < 0 || headerValue > 3)) ? headerValue : 0);
				MediaLocation = HeaderData[51];
				headerValue = GetNumeric(52);
				MediaVolume = ((headerValue < 0 || headerValue > 100) ? 50 : headerValue);
				headerValue = GetNumeric(53);
				MediaBalance = ((!(headerValue < -100 || headerValue > 100)) ? headerValue : 0);
				headerValue = GetNumeric(54);
				MediaMute = DataUtil.GetBitValue(headerValue, 1);
				MediaRepeat = DataUtil.GetBitValue(headerValue, 2);
				MediaWidescreen = DataUtil.GetBitValue(headerValue, 3);
				headerValue = GetNumeric(55);
				MediaCaptureDeviceNumber = ((headerValue < 1 || headerValue > 5) ? 1 : headerValue);
				MediaOutputMonitorName = HeaderData[56];
				BackgroundPicture = HeaderData[61];
				headerValue = GetNumeric(62);
				BackgroundMode = (ImageMode)((headerValue < 0 || headerValue > 2) ? 2 : headerValue);
				headerValue = GetNumeric(63);
				ShowVerticalAlign = ((headerValue < 0 || headerValue > 2) ? 1 : headerValue);
				headerValue = GetNumeric(64);
				Folders[0].LeftMargin = ((headerValue < 0 || headerValue > 40) ? 2 : headerValue);
				headerValue = GetNumeric(65);
				Folders[0].RightMargin = ((headerValue < 0 || headerValue > 40) ? 2 : headerValue);
				headerValue = GetNumeric(66);
				Folders[0].BottomMargin = ((!(headerValue < 0 || headerValue > 100)) ? headerValue : 0);
				ShowItemTransition = GlobalImageCanvas.GetTransitionType(HeaderData[72]);
				ShowSlideTransition = GlobalImageCanvas.GetTransitionType(HeaderData[73]);
			}
			PraiseBookTextStyle.ReadHeader(HeaderData, PraiseBookTextStyles, BlackScreenColour);
			PB_ShowHeadings[0] = 1;
			PB_ShowHeadings[1] = 1;
			PB_ShowHeadings[2] = 0;
			PB_ShowHeadings[3] = 0;
			PB_LyricsPattern = 1;
			PB_ShowSection = 2;
			PB_ShowColumns = 2;
			PB_PageSize = 0;
			PB_Spacing[0] = 0;
			PB_Spacing[1] = 2;
			PB_ShowScreenBreaks = 1;
			PB_OneSongPerPage = 0;
			PB_CJKGroupStyle = SortBy.Alpha;
			headerValue = GetNumeric(151);
			PB_ShowHeadings[0] = DataUtil.GetBitValue(headerValue, 1);
			PB_ShowHeadings[1] = DataUtil.GetBitValue(headerValue, 2);
			PB_ShowHeadings[2] = DataUtil.GetBitValue(headerValue, 3);
			PB_ShowHeadings[3] = DataUtil.GetBitValue(headerValue, 4);
			headerValue = GetNumeric(153);
			PB_LyricsPattern = ((headerValue < 0 || headerValue > 1) ? 1 : headerValue);
			headerValue = GetNumeric(154);
			PB_ShowSection = ((headerValue < 0 || headerValue > 2) ? 2 : headerValue);
			headerValue = GetNumeric(155);
			PB_ShowColumns = ((headerValue < 1 || headerValue > 2) ? 2 : headerValue);
			headerValue = GetNumeric(156);
			PB_PageSize = ((!(headerValue < 0 || headerValue > 1)) ? headerValue : 0);
			headerValue = GetNumeric(170);
			PB_Spacing[0] = ((!(headerValue < 0 || headerValue > 5)) ? headerValue : 0);
			headerValue = GetNumeric(171);
			PB_Spacing[1] = ((headerValue < 1 || headerValue > 20) ? 2 : headerValue);
			headerValue = GetNumeric(172);
			PB_ShowScreenBreaks = ((headerValue < 0 || headerValue > 1) ? 1 : headerValue);
			headerValue = GetNumeric(173);
			PB_OneSongPerPage = ((!(headerValue < 0 || headerValue > 1)) ? headerValue : 0);
			headerValue = GetNumeric(174);
			PB_CJKGroupStyle = (SortBy)((!(headerValue < 0 || headerValue > 1)) ? headerValue : 0);
			headerValue = GetNumeric(180);
			PB_ShowNotations = DataUtil.GetBitValue(headerValue, 1);
			PB_ShowTiming = DataUtil.GetBitValue(headerValue, 2);
			PB_ShowKey = DataUtil.GetBitValue(headerValue, 3);
			PB_ShowCapo = DataUtil.GetBitValue(headerValue, 4);
			PB_CapoZero = ((headerValue >= 0) ? DataUtil.GetBitValue(headerValue, 5) : 0);
		}

		private static string GapFormatString(ref SongSettings InItem)
		{
			InItem.UseDefaultFormat = false;
			switch (GapItemOption)
			{
				case GapType.Black:
					InItem.Format.ShowScreenColour[0] = BlackScreenColour.ToArgb();
					InItem.Format.ShowScreenColour[1] = BlackScreenColour.ToArgb();
					InItem.Format.ShowScreenStyle = 11;
					InItem.Format.BackgroundPicture = "";
					break;
				case GapType.User:
					{
						InItem.Format.BackgroundPicture = GapItemLogoFile;
						string directoryName = Path.GetDirectoryName(InItem.Format.BackgroundPicture);
						if (directoryName == RootEasiSlidesDir + "Images\\Tiles")
						{
							InItem.Format.BackgroundMode = ImageMode.Tile;
						}
						else
						{
							InItem.Format.BackgroundMode = ImageMode.BestFit;
						}
						break;
					}
				case GapType.Default:
					InItem.Format.ShowScreenColour[0] = ShowScreenColour[0].ToArgb();
					InItem.Format.ShowScreenColour[1] = ShowScreenColour[1].ToArgb();
					InItem.Format.ShowScreenStyle = ShowScreenStyle;
					InItem.Format.BackgroundPicture = BackgroundPicture;
					InItem.Format.BackgroundMode = BackgroundMode;
					break;
				default:
					return "";
			}
			InItem.Format.ShowItemTransition = (GapItemUseFade ? 15 : 0);
			InItem.Format.ShowSlideTransition = InItem.Format.ShowItemTransition;
			StringBuilder stringBuilder = new StringBuilder();
			// SongFormat colours are already ARGB ints: serialise them as-is (same as the former .ToArgb()).
			stringBuilder.Append(Convert.ToString(26) + "=" + Convert.ToString(InItem.Format.ShowScreenColour[0]) + '>');
			stringBuilder.Append(Convert.ToString(27) + "=" + Convert.ToString(InItem.Format.ShowScreenColour[1]) + '>');
			stringBuilder.Append(Convert.ToString(28) + "=" + InItem.Format.ShowScreenStyle.ToString() + '>');
			stringBuilder.Append(Convert.ToString(50) + "=" + Convert.ToString(InItem.Format.MediaOption) + '>');
			stringBuilder.Append(Convert.ToString(51) + "=" + InItem.Format.MediaLocation + '>');
			stringBuilder.Append(Convert.ToString(52) + "=" + Convert.ToString(InItem.Format.MediaVolume) + '>');
			stringBuilder.Append(Convert.ToString(53) + "=" + Convert.ToString(InItem.Format.MediaBalance) + '>');
			int mediaFlags = InItem.Format.MediaMute + InItem.Format.MediaRepeat * 2 + InItem.Format.MediaWidescreen * 4;
			stringBuilder.Append(Convert.ToString(54) + "=" + mediaFlags.ToString() + '>');
			stringBuilder.Append(Convert.ToString(55) + "=" + Convert.ToString(InItem.Format.MediaCaptureDeviceNumber) + '>');
			stringBuilder.Append(Convert.ToString(56) + "=" + InItem.Format.MediaOutputMonitorName + '>');
			stringBuilder.Append(Convert.ToString(61) + "=" + InItem.Format.BackgroundPicture + '>');
			stringBuilder.Append(Convert.ToString(62) + "=" + Convert.ToString((int)InItem.Format.BackgroundMode) + '>');
			stringBuilder.Append(Convert.ToString(72) + "=" + tempScreen.GetTransitionText(InItem.Format.ShowItemTransition) + '>');
			stringBuilder.Append(Convert.ToString(73) + "=" + tempScreen.GetTransitionText(InItem.Format.ShowSlideTransition) + '>');
			return stringBuilder.ToString();
		}

		public static bool IsNewR2Format(string InLyrics)
		{
			ReadOnlySpan<char> lyrics = InLyrics;
			ReadOnlySpan<char> marker = VerseSymbol[REGION2];
			if (marker.IsEmpty)
				return false;

			int sectionCount = 0;
			ReadOnlySpan<char> secondSection = default;
			foreach (Range range in lyrics.Split(marker))
			{
				var section = lyrics[range];
				if (section.IsEmpty)
					continue;
				sectionCount++;
				if (sectionCount == 2)
					secondSection = section;
				else if (sectionCount > 2)
					return false;
			}
			if (sectionCount != 2)
				return false;
			if (secondSection.StartsWith("\r\n", StringComparison.Ordinal))
				secondSection = secondSection[2..];
			else if (secondSection[0] == '\n')
				secondSection = secondSection[1..];
			return ReadVerseIndicator(secondSection).Length > 0;
		}

		public static string GetSlideContents(SongSettings InItem, int CurSlide, int RegionNumber, Font InFont, bool PreviewNotations)
		{
			tbWorkspace.WordWrap = false;
			tbTempSpace.WordWrap = false;
			int startColumn = RegionNumber == 0 ? 1 : 3;
			int firstRow = InItem.Slide[CurSlide, startColumn];
			var rows = InItem.LyricsAndNotationsList.Items;
			int lastRow = Math.Min(InItem.Slide[CurSlide, startColumn + 1], rows.Count - 1);
			if (firstRow < 0 || firstRow > lastRow)
				return "\n";

			var lyrics = new StringBuilder();
			StringBuilder notes = null;
			for (int i = firstRow; i <= lastRow; i++)
			{
				var fields = rows[i].SubItems;
				lyrics.Append(fields[2].Text).Append('\n');
				if (PreviewNotations)
				{
					string chords = fields[3].Text;
					if (notes != null || chords.Length > 0)
					{
						notes ??= new StringBuilder();
						notes.Append('(').Append(i - firstRow).Append(';').Append(chords).Append(')');
					}
				}
			}

			string text = lyrics.ToString();
			if (notes != null)
				text = CombineLyricsAndNotations(text, notes.ToString(), InFont, InFont, ref tbWorkspace, ref tbTempSpace);
			else if (!PreviewNotations)
				SubstituteDashes(ref text, 0);
			if (InItem.Type == "B")
				text = text.Replace("\u0098", "");
			var content = text.AsSpan().TrimEnd("\r\n");
			if (text.EndsWith('\n') && content.Length == text.Length - 1)
				return text;
			return string.Concat(content, "\n");
		}

		public static string OldGetSlideContents(SongSettings InItem, int CurSlide, int RegionNumber, Font InFont, bool PreviewNotations)
		{
			int[,] slide = InItem.Slide;
			int startColumn = (RegionNumber == 0) ? 1 : 3;
			int endColumn = (RegionNumber == 0) ? 2 : 4;
			string slideText = "";
			if (PreviewNotations)
			{
				if (slide[CurSlide, startColumn] <= slide[CurSlide, endColumn])
				{
					bool hasNotations = false;
					for (int i = slide[CurSlide, startColumn]; i <= slide[CurSlide, endColumn]; i++)
					{
						if (InItem.LyricsAndNotationsList.Items[i].SubItems[3].Text != "")
						{
							hasNotations = true;
							i = slide[CurSlide, endColumn] + 1;
						}
					}
					for (int i = slide[CurSlide, startColumn]; i <= slide[CurSlide, endColumn]; i++)
					{
						if (hasNotations)
						{
							slideText = slideText + CombineLyricsAndNotations(InItem.LyricsAndNotationsList.Items[i].SubItems[2].Text, InItem.LyricsAndNotationsList.Items[i].SubItems[3].Text, InFont, InFont, ref tbWorkspace, ref tbTempSpace) + "\n";
						}
						slideText = slideText + InItem.LyricsAndNotationsList.Items[i].SubItems[2].Text + "\n";
					}
				}
			}
			else if (slide[CurSlide, startColumn] <= slide[CurSlide, endColumn])
			{
				for (int i = slide[CurSlide, startColumn]; i <= slide[CurSlide, endColumn]; i++)
				{
					slideText = slideText + InItem.LyricsAndNotationsList.Items[i].SubItems[2].Text + "\n";
				}
			}
			if (InItem.Type == "B")
			{
				slideText = slideText.Replace('\u0098'.ToString(), "");
			}
			return slideText;
		}

		public static string FormatNotationString(ListView InListView, string InString, string InNotation, Font MainFont, Font NotationsFont)
		{
			using var graphics = InListView.CreateGraphics();
			using var layout = StringFormat.GenericDefault;
			return FormatNotationString(graphics, layout, InString, InNotation, MainFont, NotationsFont);
		}

		private static string FormatNotationString(Graphics graphics, StringFormat layout, string lyrics, string notations, Font lyricFont, Font chordFont)
		{
			ReadOnlySpan<char> fields = notations;
			var chord = TakeNotationField(ref fields);
			var position = TakeNotationField(ref fields);
			if (chord.SequenceEqual("-1") || position.SequenceEqual("-1"))
				return " ";

			using var typographic = StringFormat.GenericTypographic;
			int markerWidth = (int)graphics.MeasureString("i", chordFont, 1000, typographic).Width;
			float spaceAdvance = (graphics.MeasureString("        i", chordFont, PointF.Empty, layout).Width
				- graphics.MeasureString("i", chordFont, PointF.Empty, layout).Width) / 8;
			var screenLine = new StringBuilder();
			var printLine = PB_PrinterSpaces > 0 ? new StringBuilder() : null;
			ReadOnlySpan<char> text = lyrics;
			while (!chord.SequenceEqual("-1") && !position.SequenceEqual("-1"))
			{
				var prefix = text[..Math.Clamp(int.Parse(position), 0, text.Length)];
				if (!prefix.IsEmpty && prefix[^1] == ' ')
					prefix = string.Concat(prefix[..^1], "i");
				int target = (int)graphics.MeasureString(prefix, lyricFont, 32000, layout).Width + markerWidth;
				int padding = GetChordPadding(graphics, layout, chordFont, screenLine, target, spaceAdvance);
				screenLine.Append(' ', padding);
				if (screenLine.Length > 0 && screenLine[^1] != ' ')
					screenLine.Append(' ');
				screenLine.Append(chord);
				printLine?.Append(' ', padding + padding / 12).Append(chord);
				chord = TakeNotationField(ref fields);
				position = TakeNotationField(ref fields);
			}
			return screenLine.Length == 0 ? " " : (printLine ?? screenLine).ToString();
		}

		private static int GetChordPadding(Graphics graphics, StringFormat layout, Font font, StringBuilder line, int targetWidth, float spaceAdvance)
		{
			int prefixLength = line.Length;
			float Measure(int spaces)
			{
				line.Append(' ', spaces).Append('i');
				float width = graphics.MeasureString(line.ToString(), font, PointF.Empty, layout).Width;
				line.Length = prefixLength;
				return width;
			}

			float currentWidth = Measure(0);
			if (currentWidth >= targetWidth || spaceAdvance <= 0)
				return 0;

			// The space advance gives a starting estimate. Measure the actual font run to
			// preserve kerning and choose the first space that reaches the original pixel target.
			int limit = (int)Math.Ceiling(targetWidth / spaceAdvance) + 2;
			int high = Math.Clamp((int)Math.Ceiling((targetWidth - currentWidth) / spaceAdvance), 1, limit);
			int low = 0;
			while (Measure(high) < targetWidth)
			{
				if (high == limit)
					return 0; // The font run could not reach the target with visible padding.
				low = high;
				high = Math.Min(high * 2, limit);
			}
			if (high - low > 1 && Measure(high - 1) < targetWidth)
				return high;
			while (high - low > 1)
			{
				int mid = low + (high - low) / 2;
				if (Measure(mid) < targetWidth)
					low = mid;
				else
					high = mid;
			}
			return high;
		}

		public static int GetAssociatedLyricsLineCurPosX(ref RichTextBox IntextBox, int InCurPos)
		{
			return GetAssociatedLyricsLineCurPosX(ref IntextBox, InCurPos, 0);
		}

		public static int GetAssociatedLyricsLineCurPosX(ref RichTextBox IntextBox, int InCurPos, int LyricsCurPosMin)
		{
			return GetAssociatedLyricsLineCurPosX(ref IntextBox, InCurPos, LyricsCurPosMin, 64000);
		}

		public static int GetAssociatedLyricsLineCurPosX(ref RichTextBox IntextBox, int InCurPos, int LyricsCurPosMin, int LyricsCurPosMax)
		{
			if (InCurPos < 0)
			{
				InCurPos = 0;
			}
			if (InCurPos > LyricsCurPosMax - LyricsCurPosMin + 1)
			{
				Point positionFromCharIndex = IntextBox.GetPositionFromCharIndex(LyricsCurPosMax - LyricsCurPosMin + 1);
				Point positionFromCharIndex2 = IntextBox.GetPositionFromCharIndex(LyricsCurPosMax - LyricsCurPosMin);
				return positionFromCharIndex.X + (positionFromCharIndex.X - positionFromCharIndex2.X);
			}
			return IntextBox.GetPositionFromCharIndex(InCurPos + LyricsCurPosMin).X;
		}


		public static void GetMinMaxfromTextBox(RichTextBox InBox, int InLineNumber, ref int InMin, ref int InMax)
		{
			string boxText = InBox.Text + "\n";
			InMax = -1;
			for (int i = 0; i <= InLineNumber; i++)
			{
				InMin = InMax + 1;
				InMax = boxText.IndexOf("\n", InMin);
				if (InMax < 0)
				{
					i = InLineNumber;
				}
			}
			InMax--;
		}

		public static void GetMinMaxfromTextString(string InString, int InLineNumber, ref int InMin, ref int InMax)
		{
			if (InString == "")
			{
				InMin = -1;
				InMax = -1;
			}
			int lineIndex = 0;
			string textWithLf = InString + "\n";
			InMax = -1;
			for (lineIndex = 0; lineIndex <= InLineNumber; lineIndex++)
			{
				InMin = InMax + 1;
				InMax = textWithLf.IndexOf("\n", InMin);
				if (InMax < 0)
				{
					InMin = -1;
					InMax = -1;
					lineIndex = InLineNumber;
				}
			}
			InMax--;
		}

		public static int GetVerseIndicator(string InString)
		{
			for (int i = 0; i <= 150; i++)
			{
				if ((i < 13 || (i >= 100 && i <= 112) || i == 150) && InString.IndexOf(VerseSymbol[i]) >= 0)
				{
					return i;
				}
			}
			return -1;
		}

		public static string TransposeOneNotationString(string NotationString, int TransposeTo, int FlatSharpKey)
		{
			if (string.IsNullOrEmpty(NotationString))
				return string.Empty;
			var result = new StringBuilder(NotationString.Length);
			AppendTransposedChords(result, NotationString, TransposeTo, FlatSharpKey);
			return result.ToString();
		}

		private static void AppendTransposedChords(StringBuilder result, ReadOnlySpan<char> fields, int steps, int spelling)
		{
			while (!fields.IsEmpty)
			{
				var chord = TakeNotationField(ref fields);
				if (steps == 0)
					result.Append(chord);
				else
					result.Append(TransposeChord(chord, steps, spelling));
				result.Append(';').Append(TakeNotationField(ref fields)).Append(';');
			}
		}

		private static ReadOnlySpan<char> TakeNotationField(ref ReadOnlySpan<char> fields)
		{
			if (fields.IsEmpty)
				return "-1"; // The position is missing after the last chord.
			int separator = fields.IndexOf(';');
			if (separator < 0)
			{
				var lastField = fields;
				fields = default;
				return lastField;
			}
			var field = fields[..separator];
			fields = fields[(separator + 1)..];
			return field;
		}

		public static int ExtractOneNotationsLine(ref string InString, ref string ResultString)
		{
			if (InString == null || InString == "")
			{
				ResultString = "";
				return -1;
			}
			ReadOnlySpan<char> remaining = InString;
			if (!TryReadNotationRecord(ref remaining, out int lineNumber, out var chords))
				return -1;
			InString = remaining.ToString();
			ResultString = chords.ToString();
			return lineNumber;
		}

		private static bool TryReadNotationRecord(ref ReadOnlySpan<char> remaining, out int lineNumber, out ReadOnlySpan<char> chords)
		{
			lineNumber = -1;
			chords = default;
			int opening = remaining.IndexOf('(');
			if (opening < 0)
				return false;
			var record = remaining[(opening + 1)..];
			int closing = ChordNotation.FindRecordEnd(record);
			if (closing < 0)
				return false;

			remaining = record[(closing + 1)..];
			record = record[..closing];
			if (!record.IsEmpty)
			{
				int separator = record.IndexOf(';');
				var number = separator < 0 ? record : record[..separator];
				// Legacy files treat a nonnumeric line label as zero and an empty record as the end.
				int.TryParse(number, out lineNumber);
				if (separator >= 0)
					chords = record[(separator + 1)..];
			}
			return true;
		}

		public static void ChangeNotationLineNumber(ref string InOneNotationLine, int InNewLineNumber)
		{
			if (InOneNotationLine.Length > 0)
			{
				string InString = InOneNotationLine;
				string chord = DataUtil.ExtractOneInfo(ref InString, ';');
				int lineNumber = Convert.ToInt32(DataUtil.ExtractOneInfo(ref InString, ';'));
				if (chord != "-1" && lineNumber >= 0)
				{
					InOneNotationLine = chord + ';' + InNewLineNumber + ';' + InString;
				}
			}
		}

		private static void AppendLyricSlide(ref int[,] slides, ref int slideCount)
		{
			int nextSlide = slideCount + 1;
			if (nextSlide >= slides.GetLength(0))
			{
				int rows = Math.Max(nextSlide + 1, slides.GetLength(0) * 2);
				var expanded = new int[rows, slides.GetLength(1)];
				Array.Copy(slides, expanded, slides.Length);
				slides = expanded;
			}
			slideCount = nextSlide;
		}

		public static void BuildSlides(SongSettings InItem, ListView LyricsLists, ref string SongSequence, ref int CurSongMaxSlides, ref int[] SongVerses, ref int[] ChorusSlides, ref int[,] Slide, int InShowNotations)
		{
			List<(int Start, int End)> splitRanges = InItem?.SplitScreens == true ? [] : null;
			int folderNo = 1;
			for (int i = 0; i <= 9; i++)
			{
				SongVerses[i] = 0;
				ChorusSlides[i] = 0;
			}
			CurSongMaxSlides = 0;
			if (InItem != null)
			{
				InItem.Verse2Present = InItem.VersePresent[2];
				folderNo = InItem.FolderNo;
			}
			Slide[0, 3] = -1;
			foreach (int verseIndex in SongSequence)
			{
				bool lastSubScreen = false;
				if (verseIndex < InItem.VerseLineLoc.GetLength(0) && InItem.VersePresent[verseIndex])
				{
					int verseScreenCount = GetVerseScreenCount(LyricsLists, InItem.VerseLineLoc[verseIndex, 1], InItem.VerseLineLoc[verseIndex, 2]);
					if (verseScreenCount > 0)
					{
						try
						{
							for (int j = 1; j <= verseScreenCount; j++)
							{
								int verseScreenLoc = GetVerseScreenLoc(LyricsLists, InItem.VerseLineLoc[verseIndex, 1], InItem.VerseLineLoc[verseIndex, 2], j);
								if (verseScreenLoc >= 0)
								{
									AppendLyricSlide(ref Slide, ref CurSongMaxSlides);
									if (j == 1)
									{
										Slide[CurSongMaxSlides, 0] = verseIndex;
									}
									else
									{
										Slide[CurSongMaxSlides, 0] = -1;
									}
									Slide[CurSongMaxSlides, 1] = verseScreenLoc;
									Slide[CurSongMaxSlides, 2] = GetVerseScreenEndLoc(LyricsLists, verseScreenLoc, InItem.VerseLineLoc[verseIndex, 2]);
									Slide[CurSongMaxSlides, 3] = -1;
									Slide[CurSongMaxSlides, 4] = -1;
									int endLine = Slide[CurSongMaxSlides, 2];
									int screenCount = endLine >= 0 && splitRanges != null
										? GetScreensRequired(InItem, InItem.Lyrics[0], ref LyricsLists, verseScreenLoc, endLine, splitRanges, InItem.VersePresent[150], 0, folderNo, InShowNotations)
										: 1;
									if (screenCount > 1)
									{
										CurSongMaxSlides--;
										for (int k = 0; k < screenCount; k++)
										{
											AppendLyricSlide(ref Slide, ref CurSongMaxSlides);
											if (j == 1 && k == 0)
											{
												Slide[CurSongMaxSlides, 0] = verseIndex;
											}
											else
											{
												Slide[CurSongMaxSlides, 0] = -1;
											}
											(Slide[CurSongMaxSlides, 1], Slide[CurSongMaxSlides, 2]) = splitRanges[k];
											Slide[CurSongMaxSlides, 3] = -1;
											Slide[CurSongMaxSlides, 4] = -1;
										}
									}
									if (InItem.VerseLineLoc[verseIndex, 3] >= 0 && InItem.VerseLineLoc[verseIndex, 4] >= InItem.VerseLineLoc[verseIndex, 3])
									{
										Slide[0, 3] = 1;
										if (j == verseScreenCount)
										{
											lastSubScreen = true;
										}
										BuildSlidesReg2(LyricsLists, InItem.VerseLineLoc[verseIndex, 3], InItem.VerseLineLoc[verseIndex, 4], ref Slide, ref CurSongMaxSlides, screenCount, j, lastSubScreen);
									}
								}
							}
						}
						catch
						{
						}
					}
					else
					{
						try
						{
							Slide[0, 3] = 2;
							verseScreenCount = GetVerseScreenCount(LyricsLists, InItem.VerseLineLoc[verseIndex, 3], InItem.VerseLineLoc[verseIndex, 4]);
							for (int j = 1; j <= verseScreenCount; j++)
							{
								int verseScreenLoc = GetVerseScreenLoc(LyricsLists, InItem.VerseLineLoc[verseIndex, 3], InItem.VerseLineLoc[verseIndex, 4], j);
								if (verseScreenLoc >= 0)
								{
									AppendLyricSlide(ref Slide, ref CurSongMaxSlides);
									if (j == 1)
									{
										Slide[CurSongMaxSlides, 0] = verseIndex;
									}
									else
									{
										Slide[CurSongMaxSlides, 0] = -1;
									}
									Slide[CurSongMaxSlides, 1] = -1;
									Slide[CurSongMaxSlides, 2] = -1;
									Slide[CurSongMaxSlides, 3] = verseScreenLoc;
									Slide[CurSongMaxSlides, 4] = GetVerseScreenEndLoc(LyricsLists, verseScreenLoc, InItem.VerseLineLoc[verseIndex, 4]);
									int endLine = Slide[CurSongMaxSlides, 4];
									if (endLine >= 0 && splitRanges != null && GetScreensRequired(InItem, InItem.Lyrics[1], ref LyricsLists, verseScreenLoc, endLine, splitRanges, Region2Present: true, 1, folderNo, InShowNotations) > 1)
									{
										CurSongMaxSlides--;
										for (int k = 0; k < splitRanges.Count; k++)
										{
											AppendLyricSlide(ref Slide, ref CurSongMaxSlides);
											if (k == 0)
											{
												Slide[CurSongMaxSlides, 0] = verseIndex;
											}
											else
											{
												Slide[CurSongMaxSlides, 0] = -1;
											}
											Slide[CurSongMaxSlides, 1] = -1;
											Slide[CurSongMaxSlides, 2] = -1;
											(Slide[CurSongMaxSlides, 3], Slide[CurSongMaxSlides, 4]) = splitRanges[k];
										}
									}
								}
							}
						}
						catch
						{
						}
					}
				}
			}
			try
			{
				int chorusCount = 1;
				for (int i = CurSongMaxSlides; i >= 1; i--)
				{
					if (Slide[i, 0] > 0 && Slide[i, 0] <= 9)
					{
						SongVerses[Slide[i, 0]] = i;
					}
					else if (Slide[i, 0] == 0 && chorusCount <= 9)
					{
						SongVerses[0] = i;
						ChorusSlides[chorusCount] = i;
						chorusCount++;
					}
				}
			}
			catch
			{
			}
		}


		// ========== GetVerseScreenCount (Line 2168) ==========

		public static int GetVerseScreenCount(ListView LyricsLists, int StartLoc, int EndLoc)
		{
			if (StartLoc > EndLoc || StartLoc < 0 || EndLoc < 0)
			{
				return -1;
			}
			int screenCount = 0;
			for (int i = StartLoc; i <= EndLoc; i++)
			{
				if (LyricsLists.Items[i].SubItems[2].Text == "" || i == EndLoc)
				{
					screenCount++;
				}
			}
			return screenCount;
		}

		public static int GetVerseScreenLoc(ListView LyricsLists, int StartLoc, int EndLoc, int InScreenNumber)
		{
			if (StartLoc > EndLoc)
			{
				return -1;
			}
			if (StartLoc + 1 <= EndLoc && LyricsLists.Items[StartLoc].SubItems[2].Text == "")
			{
				StartLoc++;
			}
			int screenNumber = 1;
			for (int i = StartLoc; i <= EndLoc; i++)
			{
				if (LyricsLists.Items[i].SubItems[2].Text == "")
				{
					screenNumber++;
				}
				if (InScreenNumber == screenNumber)
				{
					if (LyricsLists.Items[i].SubItems[2].Text != "")
					{
						return i;
					}
					if (i < EndLoc)
					{
						return i + 1;
					}
					return -1;
				}
			}
			return -1;
		}

		public static int GetVerseScreenEndLoc(ListView LyricsLists, int StartLoc, int EndLoc)
		{
			if (StartLoc > EndLoc)
			{
				return -1;
			}
			if (StartLoc == EndLoc)
			{
				return EndLoc;
			}
			if (LyricsLists.Items[StartLoc].SubItems[2].Text == "")
			{
				StartLoc++;
			}
			for (int i = StartLoc; i < EndLoc; i++)
			{
				if (LyricsLists.Items[i].SubItems[2].Text == "")
				{
					return i - 1;
				}
			}
			return EndLoc;
		}

		private static void BuildSlidesReg2(ListView LyricsLists, int StartLoc, int EndLoc, ref int[,] Slide, ref int CurSlideNumber, int region1ScreenCount, int InScreenNumber, bool LastSubScreen)
		{
			if (StartLoc > EndLoc || region1ScreenCount < 1)
			{
				return;
			}
			int screenStart = -1;
			int screenEnd = EndLoc;
			screenStart = GetVerseScreenLoc(LyricsLists, StartLoc, EndLoc, InScreenNumber);
			if (screenStart < 0)
			{
				return;
			}
			screenEnd = GetVerseScreenEndLoc(LyricsLists, screenStart, EndLoc);
			int nextLine = screenStart;
			for (int i = 1; i <= region1ScreenCount; i++)
			{
				int slideIndex = CurSlideNumber - region1ScreenCount + i;
				int region1LineCount = Slide[slideIndex, 2] - Slide[slideIndex, 1] + 1;
				if (region1LineCount <= 0 || nextLine < 0)
				{
					continue;
				}
				Slide[slideIndex, 3] = nextLine;
				if (nextLine < 0)
				{
					continue;
				}
				if (screenEnd - nextLine + 1 >= region1LineCount)
				{
					if (i == region1ScreenCount)
					{
						Slide[slideIndex, 4] = screenEnd;
						nextLine = -1;
					}
					else
					{
						Slide[slideIndex, 4] = nextLine + region1LineCount - 1;
						nextLine += region1LineCount;
					}
				}
				else
				{
					Slide[slideIndex, 4] = screenEnd;
					nextLine = -1;
				}
			}
			if (EndLoc <= screenEnd || !LastSubScreen)
			{
				return;
			}
			screenStart = screenEnd + 1;
			for (int j = 1; j <= GetVerseScreenCount(LyricsLists, screenStart, EndLoc); j++)
			{
				int verseScreenLoc = GetVerseScreenLoc(LyricsLists, screenStart, EndLoc, j);
				if (verseScreenLoc >= 0)
				{
					AppendLyricSlide(ref Slide, ref CurSlideNumber);
					Slide[CurSlideNumber, 0] = -1;
					Slide[CurSlideNumber, 1] = -1;
					Slide[CurSlideNumber, 2] = -1;
					Slide[CurSlideNumber, 3] = verseScreenLoc;
					Slide[CurSlideNumber, 4] = GetVerseScreenEndLoc(LyricsLists, verseScreenLoc, EndLoc);
				}
			}
		}


		// ========== GetScreensRequired (Line 2309) ==========

		private static int GetScreensRequired(SongSettings InItem, SongLyrics InLyricsFormat, ref ListView LyricsLists, int StartLoc, int EndLoc, List<(int Start, int End)> splitRanges, bool Region2Present, int RegionNumber, int FolderNo, int InShowNotations)
		{
			using Graphics g = LyricsLists.CreateGraphics();
			splitRanges.Clear();
			int startLine = StartLoc;
			int endline = EndLoc;
			while (GetOneScreen(InItem, InLyricsFormat, ref LyricsLists, g, startLine, ref endline, EndLoc, Region2Present, RegionNumber, FolderNo, InShowNotations) > 0)
			{
				splitRanges.Add((startLine, endline));
				startLine = endline + 1;
				endline = EndLoc;
			}
			return splitRanges.Count;
		}

		public static int GetOneScreen(SongSettings InItem, SongLyrics InLyricsFormat, ref ListView LyricsLists, Graphics g, int startline, ref int endline, int EndLoc, bool Region2Present, int RegionNumber, int FolderNo, int InShowNotations)
		{
			return GetOneScreen(InItem, InLyricsFormat, ref LyricsLists, g, startline, ref endline, EndLoc, Region2Present, RegionNumber, FolderNo, InShowNotations, FitAllIntoOneScreen: FitWholeLyricPage(InItem), UseLargestFontSize: false);
		}

		public static int GetOneScreen(SongSettings InItem, SongLyrics InLyricsFormat, ref ListView LyricsLists, Graphics g, int startline, ref int endline, int EndLoc, bool Region2Present, int RegionNumber, int FolderNo, int InShowNotations, bool FitAllIntoOneScreen, bool UseLargestFontSize)
		{
			if (endline < startline)
			{
				return 0;
			}
			using var initialFont = new Font(InLyricsFormat.Font.Name, InLyricsFormat.Font.Size, InLyricsFormat.Font.Style);
			Font MainFont = initialFont;
			SizeF layoutArea = new SizeF(InLyricsFormat.FS_Width, 32000f);
			double lineSpacingFactor = Folders[FolderNo].FontSpacingFactor[0] + ((InShowNotations > 0) ? NotationFontFactor : 0.0);
			int availableHeight = (int)((double)(Region2Present ? InLyricsFormat.FS_Height_R2Bound : InLyricsFormat.FS_Height) / lineSpacingFactor);
			if (FitAllIntoOneScreen)
			{
				var screenText = new StringBuilder();
				for (int i = startline; i <= endline; i++)
				{
					screenText.Append(LyricsLists.Items[i].SubItems[2].Text).Append('\n');
				}
				if (screenText.Length > 1)
					screenText.Length--;
				ReduceFontToFit(g, screenText.ToString(), ref MainFont, InLyricsFormat.FS_Width, availableHeight, MultiLine: true);
			}
			int lineHeight = (int)g.MeasureString("A", InLyricsFormat.FS_Font, layoutArea).Height;
			int usedHeight = 0;
			for (int i = startline; i <= EndLoc; i++)
			{
				if (i == startline)
				{
					ReduceFontToFit(g, LyricsLists.Items[i].SubItems[2].Text, ref MainFont, InLyricsFormat.FS_Width, availableHeight, MultiLine: true);
				}
				usedHeight += GetLinesRequiredAndAddBreakPlusFont(ref LyricsLists, MainFont, g, i, layoutArea.Width) * lineHeight;
				if (!FitAllIntoOneScreen && usedHeight > availableHeight)
				{
					endline = ((i > startline) ? (i - 1) : startline);
					return 1;
				}
			}
			endline = EndLoc;
			return 1;
		}

		public static string ActionWordWrapSpacesAtStart(ref string InString)
		{
			int prefixLength = WordWrapIgnoreStartSpaces;
			if (prefixLength <= 0 || InString.Length <= prefixLength)
				return "";

			ReadOnlySpan<char> prefix = InString.AsSpan(0, prefixLength);
			if (!prefix.Contains(' '))
				return "";

			var spacePositions = new StringBuilder();
			for (int i = 0; i < prefix.Length; i++)
			{
				if (prefix[i] == ' ')
					spacePositions.Append(i).Append(';');
			}
			InString = string.Create(InString.Length, (InString, prefixLength), static (chars, state) =>
			{
				state.InString.CopyTo(chars);
				chars[..state.prefixLength].Replace(' ', '_');
			});
			return spacePositions.ToString();
		}

		public static void ActionUndoWordWrapSpacesAtStart(ref string ExtractedText, ref string ReplacedLog)
		{
			int spacePos = DataUtil.StringToInt(DataUtil.ExtractOneInfo(ref ReplacedLog, ';'));
			string result = ExtractedText;
			while (spacePos < ExtractedText.Length && spacePos >= 0)
			{
				result = result.Remove(spacePos, 1);
				result = result.Insert(spacePos, " ");
				spacePos = DataUtil.StringToInt(DataUtil.ExtractOneInfo(ref ReplacedLog, ';'));
			}
			ExtractedText = result;
			if (spacePos > ExtractedText.Length)
			{
				ReplacedLog = spacePos.ToString() + ';' + ReplacedLog;
			}
			int baseOffset = spacePos;
			result = "";
			while (ReplacedLog != "")
			{
				spacePos = DataUtil.StringToInt(DataUtil.ExtractOneInfo(ref ReplacedLog, ';'));
				result = result + Convert.ToString(spacePos - baseOffset) + ';';
			}
			ReplacedLog = result;
		}

		public static int GetLinesRequiredAndAddBreakPlusFont(ref ListView LyricsLists, Font MainFont, Graphics g, int LyricsIndex, float InWidth)
		{
			string InString = LyricsLists.Items[LyricsIndex].SubItems[2].Text;
			ActionWordWrapSpacesAtStart(ref InString);
			StringBuilder breaks = new();
			int count = LyricLineMeasurement.Count(InString, MainFont, g, InWidth, breaks);
			if (count > 0)
				LyricsLists.Items[LyricsIndex].SubItems[4].Text = $"{count}>{MainFont.Size}>{breaks}";
			return count;
		}


		// ========== SubstituteDashes (Line 2587) ==========

		public static void SubstituteDashes(ref string ExtractedText, int InShowNotations)
		{
			if (InShowNotations < 1)
			{
				ExtractedText = ExtractedText.Replace(DashesString, DashesStringSubstitute);
				ExtractedText = ExtractedText.Replace(DashesStringSubstitute + "---", "");
				ExtractedText = ExtractedText.Replace(DashesStringSubstitute + "--", "");
				ExtractedText = ExtractedText.Replace(DashesStringSubstitute + "-", "");
				ExtractedText = ExtractedText.Replace(DashesStringSubstitute, "");
			}
		}

		public static void SubDivideOneOutputText(string InText, Font MainFont, Font NotationsFont, Graphics g, int InWidth, int InShowNotations, string NotationsString, int TextLength, int InSetLength, int StartPos, ref int EndExtractedTextPos, ref string ExtractedText)
		{
			int lastSpacePos = -1;
			bool hasSpace = false;
			int nextStartPos = 0;
			if (InSetLength == 0)
			{
				ExtractedText = InText;
				EndExtractedTextPos = TextLength;
				return;
			}
			if (InSetLength > 0)
			{
				for (int i = StartPos; i <= StartPos + InSetLength - 1; i++)
				{
					if (DataUtil.Mid(InText, i, 1) == " ")
					{
						StartPos++;
						InSetLength--;
					}
					else
					{
						i = StartPos + InSetLength;
					}
				}
				ExtractedText = DataUtil.Mid(InText, StartPos, InSetLength);
				EndExtractedTextPos = StartPos + InSetLength - 1;
				return;
			}
			for (int i = StartPos; i <= TextLength; i++)
			{
				if (DataUtil.Mid(InText, i, 1) == " ")
				{
					lastSpacePos = i;
					hasSpace = true;
					if (i == TextLength)
					{
						ExtractedText = DataUtil.Mid(InText, StartPos, TextLength - StartPos + 1);
						EndExtractedTextPos = TextLength;
					}
				}
				else if (g.MeasureString(DataUtil.Mid(InText, StartPos, i - StartPos + 1), MainFont).Width > (float)InWidth)
				{
					if (hasSpace)
					{
						ExtractedText = DataUtil.Mid(InText, StartPos, lastSpacePos - StartPos);
						nextStartPos = lastSpacePos + 1;
						EndExtractedTextPos = nextStartPos - 1;
						i = TextLength;
					}
					else
					{
						ExtractedText = DataUtil.Mid(InText, StartPos, i - StartPos);
						nextStartPos = i;
						EndExtractedTextPos = nextStartPos - 1;
						i = TextLength;
					}
				}
				else if (i == TextLength)
				{
					ExtractedText = DataUtil.Mid(InText, StartPos, TextLength - StartPos + 1);
					EndExtractedTextPos = TextLength;
				}
			}
			if (nextStartPos <= 0)
			{
				return;
			}
			for (int j = EndExtractedTextPos + 1; j <= TextLength; j++)
			{
				if (DataUtil.Mid(InText, j, 1) == " ")
				{
					EndExtractedTextPos++;
				}
				else
				{
					j = TextLength + 1;
				}
			}
		}

		public static void OldSwitchChineseLyricsNotationListView(ref ListView InLyricsAndNotationsList, int ChangeTo)
		{
			if (InLyricsAndNotationsList.Items.Count > 0)
			{
				for (int i = 0; i <= InLyricsAndNotationsList.Items.Count - 1; i++)
				{
					string InString = InLyricsAndNotationsList.Items[i].SubItems[2].Text;
					SwitchChinese(ref InString, ChangeTo);
					InLyricsAndNotationsList.Items[i].SubItems[2].Text = InString;
				}
			}
		}

		public static void SwitchChineseLyricsNotationListView(ref SongSettings InItem, int ChangeTo)
		{
			if (InItem.LyricsAndNotationsList.Items.Count > 0)
			{
				for (int i = 0; i <= InItem.LyricsAndNotationsList.Items.Count - 1; i++)
				{
					string InString = InItem.LyricsAndNotationsList.Items[i].SubItems[2].Text;
					SwitchChinese(ref InString, ChangeTo);
					InItem.LyricsAndNotationsList.Items[i].SubItems[2].Text = InString;
					SwitchChinese(ref InItem.Title, ChangeTo);
					SwitchChinese(ref InItem.Title2, ChangeTo);
					SwitchChinese(ref InItem.Copyright, ChangeTo);
				}
			}
		}

		public static int SwitchChinese(ref RichTextBox InTextBox)
		{
			string InString = InTextBox.Text;
			int conversionType = SwitchChinese(ref InString);
			string[] lines = InString.Split("\n"[0]);
			int lineStart = 0;
			int lineEnd = -1;
			int lineLength = 0;
			string lineText = "";
			lineStart = 0;
			SendMessage(InTextBox.Handle, 11u, 0u, 0u);
			for (int i = 0; i <= lines.GetUpperBound(0); i++)
			{
				lineEnd = InString.IndexOf("\n"[0], lineStart);
				SwitchChinese(ref lines[i], conversionType);
				lineText = lines[i];
				lineLength = lineEnd - lineStart;
				if (lineEnd >= 0)
				{
					InTextBox.SelectionStart = lineStart;
					InTextBox.SelectionLength = lineLength;
					InTextBox.SelectedText = lineText;
					lineStart = lineEnd + 1;
				}
				else if (lineStart <= InString.Length)
				{
					InTextBox.SelectionStart = lineStart;
					InTextBox.SelectionLength = InString.Length - lineStart;
					InTextBox.SelectedText = lineText;
				}
			}
			SendMessage(InTextBox.Handle, 11u, 1u, 0u);
			return conversionType;
		}

		public static int SwitchChinese(ref string InString)
		{
			return SwitchChinese(ref InString, -1);
		}

		public static int SwitchChinese(ref string InString, int SelectedType)
		{
			try
			{
				string placeholder = "\uE000#\uE001";
				char placeholderChar = '\uE002';
				string converted = InString;
				//str = Strings.StrConv(str, VbStrConv.SimplifiedChinese, CultureInfo.CurrentCulture.LCID);

				converted = getStrConv(converted, "utf-8");

				switch (SelectedType)
				{
					case 1:
						if (converted != InString)
						{
							InString = converted;
						}
						return 1;
					case 0:
						if (converted == InString)
						{
							converted = converted.Replace(placeholderChar.ToString(), placeholder);
							converted = getStrConv(converted, "utf-8");
							//str = Strings.StrConv(str, VbStrConv.TraditionalChinese, CultureInfo.CurrentCulture.LCID);
							converted = (InString = converted.Replace(placeholder, placeholderChar.ToString()));
						}
						return 0;
					default:
						if (converted == InString)
						{
							converted = converted.Replace(placeholderChar.ToString(), placeholder);
							converted = getStrConv(converted, "utf-8");
							//str = Strings.StrConv(str, VbStrConv.TraditionalChinese, CultureInfo.CurrentCulture.LCID);
							converted = (InString = converted.Replace(placeholder, placeholderChar.ToString()));
							return 0;
						}
						InString = converted;
						return 1;
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
				Console.WriteLine(e.StackTrace);
			}

			return 1;
		}

		static string getStrConv(string source, string codepage)
		{
			try
			{
				// Example codepage for simplified Chinese: x-cp20936
				System.Text.Encoding encoding = Encoding.GetEncoding(codepage);

				byte[] bytes = encoding.GetBytes(source);

				return encoding.GetString(bytes);
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
				Console.WriteLine(e.StackTrace);
			}

			return source;
		}

		public static void GetCurPosInLine(string instring, ref int CurPos)
		{
			MoveToPosInLine(instring, ref CurPos, 0);
		}

		public static void MoveToPosInLine(string instring, ref int CurPos, int InMode)
		{
			if (instring.Length == 0)
			{
				return;
			}
			bool found = false;
			int pos = CurPos + ((InMode == 0) ? (-1) : 0);
			string ch = "";
			while (pos >= 0 && pos < instring.Length && !found)
			{
				ch = DataUtil.Mid(instring, pos, 1);
				if (ch == "\r" || ch == "\n")
				{
					found = true;
					CurPos = pos + ((InMode == 0) ? 1 : 0);
					pos = 0;
				}
				else
				{
					pos += ((InMode != 0) ? 1 : (-1));
				}
			}
			if (!found)
			{
				CurPos = ((InMode != 0) ? instring.Length : 0);
			}
		}

		public static void InsertChordAboveCurrentLine(ref RichTextBox InTextBox, string InChordString)
		{
			bool isSlashChord = (InChordString[0].ToString() == "/") ? true : false;
			int insertPos = InTextBox.SelectionStart;
			int curPos = insertPos;
			int selectionStart = insertPos;
			int length = InTextBox.Text.Length;
			Point caretPoint = InTextBox.GetPositionFromCharIndex(insertPos);
			int lineFromCharIndex = InTextBox.GetLineFromCharIndex(insertPos);
			bool needChordLine = false;
			int markerPos = InTextBox.Text.IndexOf("»", insertPos);
			if (InTextBox.GetLineFromCharIndex(markerPos) != lineFromCharIndex)
			{
				markerPos = -1;
			}
			int chordLinePos = -1;
			if (markerPos >= 0)
			{
				chordLinePos = insertPos;
			}
			else
			{
				InChordString += " ";
				if (lineFromCharIndex < 1)
				{
					needChordLine = true;
				}
				else
				{
					string textFromPreviousLine = GetTextFromPreviousLine(InTextBox.Text, curPos);
					if (textFromPreviousLine.IndexOf("»") < 0)
					{
						needChordLine = true;
					}
				}
				if (needChordLine)
				{
					InsertIndicator(ref InTextBox, 151);
					InTextBox.SelectionStart -= 1;
					InsertIndicator(ref InTextBox, 152);
					insertPos = InTextBox.SelectionStart;
				}
				else
				{
					GetCurPosInLine(InTextBox.Text, ref insertPos);
					insertPos--;
				}
			}
			int endPos = -1;
			MoveToPosInLine(InTextBox.Text, ref endPos, 1);
			GetCurPosInLine(InTextBox.Text, ref insertPos);
			int lineStartPos = insertPos;
			InTextBox.SelectionStart = insertPos;
			markerPos = InTextBox.Text.IndexOf("»", insertPos);
			if (markerPos < 0)
			{
				return;
			}
			if (chordLinePos >= 0)
			{
				insertPos = chordLinePos;
			}
			InTextBox.SelectionStart = markerPos;
			Point markerPoint = InTextBox.GetPositionFromCharIndex(InTextBox.SelectionStart);
			while (markerPoint.X < caretPoint.X)
			{
				InTextBox.SelectedText = " ";
				markerPoint = InTextBox.GetPositionFromCharIndex(InTextBox.SelectionStart);
			}
			GetCurPosInLine(InTextBox.Text, ref insertPos);
			for (; InTextBox.GetPositionFromCharIndex(insertPos).X < caretPoint.X; insertPos++)
			{
			}
			bool atWordBoundary = false;
			while (insertPos > lineStartPos && insertPos < markerPos && !atWordBoundary)
			{
				if (InTextBox.Text[insertPos - 1].ToString() == " ")
				{
					atWordBoundary = true;
				}
				else if (InTextBox.Text[insertPos].ToString() == " ")
				{
					insertPos++;
					atWordBoundary = true;
				}
				else
				{
					insertPos++;
				}
			}
			Point nextCharPoint = new Point(-1, -1);
			int nextCharPos = -1;
			for (int j = insertPos; j <= markerPos; j++)
			{
				if (InTextBox.Text[j].ToString() != " ")
				{
					nextCharPoint = InTextBox.GetPositionFromCharIndex(j);
					nextCharPos = j;
					j = markerPos + 1;
				}
			}
			InTextBox.SelectionStart = insertPos;
			InTextBox.SelectedText = InChordString;
			InTextBox.SelectedText = "";
			if (nextCharPos >= 0)
			{
				nextCharPos += InChordString.Length - 1;
				int nextCharX = InTextBox.GetPositionFromCharIndex(nextCharPos).X;
				while (nextCharX >= nextCharPoint.X && InTextBox.Text[nextCharPos - 1].ToString() == " " && InTextBox.Text[nextCharPos - 2].ToString() == " ")
				{
					InTextBox.SelectionStart = nextCharPos;
					InTextBox.SelectionLength = 1;
					InTextBox.SelectedText = "";
					nextCharPos--;
					nextCharX = InTextBox.GetPositionFromCharIndex(nextCharPos).X;
				}
			}
			InTextBox.SelectionStart = ((chordLinePos >= 0) ? (chordLinePos + InChordString.Length) : (selectionStart + (InTextBox.Text.Length - length)));
			InTextBox.SelectedText = "";
		}

		public static string GetTextFromPreviousLine(string InString, int CurPos)
		{
			GetCurPosInLine(InString, ref CurPos);
			if (CurPos > 0)
			{
				CurPos--;
			}
			int lineEnd = CurPos;
			GetCurPosInLine(InString, ref CurPos);
			int lineLength = lineEnd - CurPos;
			string previousLine = "";
			if (lineLength > 0)
			{
				previousLine = DataUtil.Mid(InString, CurPos, lineLength);
			}
			return DataUtil.Trim(previousLine);
		}

		public static bool ValidateTitleDetails(string InString, string Heading)
		{
			char[] anyOf = new char[6]
			{
				'[',
				']',
				'*',
				'"',
				'<',
				'>'
			};
			if (InString.IndexOfAny(anyOf) >= 0)
			{
				MessageBox.Show(Heading + " must not contain the characters: ] [ * \" < >");
				return false;
			}
			if (InString.Length > 100)
			{
				MessageBox.Show(Heading + " is too long (" + Convert.ToString(InString.Length) + "), maximum length is 100 characters including spaces");
				return false;
			}
			return true;
		}

		public static void FormatPlainLyrics(ref RichTextBox InTextBox)
		{
			string normalizedText = InTextBox.Text.Replace("\r\n", "\n").Replace("\r", "");
			if (normalizedText.Length == 0)
			{
				InTextBox.Text = normalizedText;
				return;
			}

			string[] lines = normalizedText.Split('\n');
			System.Collections.Generic.List<string> outputLines = new System.Collections.Generic.List<string>(lines.Length);

			for (int i = 0; i < lines.Length; i++)
			{
				string line = lines[i];
				if (line.Length == 0)
				{
					outputLines.Add("");
					continue;
				}

				if (TryNormalizeVersePrefix(line, out string verseNormalized))
				{
					line = verseNormalized;
				}

				if (TryExtractChorus(line, out string chorusRemainder))
				{
					AppendHeading(outputLines, "[chorus]", chorusRemainder);
					continue;
				}

				if (TryExtractNumberHeading(line, out int verseNo, out string numberRemainder))
				{
					AppendHeading(outputLines, "[" + verseNo + "]", numberRemainder);
					continue;
				}

				outputLines.Add(line);
			}

			for (int i = 0; i < outputLines.Count; i++)
			{
				if (outputLines[i].IndexOf('\t') >= 0)
				{
					outputLines[i] = outputLines[i].Replace("\t", "");
				}
			}

			System.Collections.Generic.List<string> cleaned = new System.Collections.Generic.List<string>(outputLines.Count);
			for (int i = 0; i < outputLines.Count; i++)
			{
				string line = outputLines[i];
				if (IsHeadingLine(line))
				{
					if (cleaned.Count > 0 && cleaned[cleaned.Count - 1].Length == 0)
					{
						cleaned.RemoveAt(cleaned.Count - 1);
					}
					cleaned.Add(line);
					continue;
				}
				if (line.Length == 0 && cleaned.Count > 0 && IsHeadingLine(cleaned[cleaned.Count - 1]))
				{
					continue;
				}
				cleaned.Add(line);
			}

			System.Collections.Generic.List<string> collapsed = new System.Collections.Generic.List<string>(cleaned.Count);
			int emptyRun = 0;
			for (int i = 0; i < cleaned.Count; i++)
			{
				string line = cleaned[i];
				if (line.Length == 0)
				{
					emptyRun++;
					if (emptyRun <= 2)
					{
						collapsed.Add(line);
					}
				}
				else
				{
					emptyRun = 0;
					collapsed.Add(line);
				}
			}

			string result = string.Join("\n", collapsed);
			result = result.Replace("\t\n", "\n");
			result = result.Replace("\n\n[", "\n[");
			result = DataUtil.TrimEnd(result);
			if (result.IndexOf("[2]") >= 0 && result.IndexOf("[1]") < 0)
			{
				result = "[1]\n" + result;
			}
			InTextBox.Text = result;

			static void AppendHeading(System.Collections.Generic.List<string> target, string heading, string remainder)
			{
				target.Add(heading);
				if (!string.IsNullOrEmpty(remainder))
				{
					target.Add(remainder);
				}
			}

			static bool TryExtractNumberHeading(string line, out int verseNo, out string remainder)
			{
				verseNo = 0;
				remainder = "";
				if (line.Length == 0 || line[0] < '0' || line[0] > '9')
				{
					return false;
				}
				int index = 0;
				while (index < line.Length && line[index] >= '0' && line[index] <= '9')
				{
					index++;
				}
				if (!int.TryParse(line.Substring(0, index), out verseNo) || verseNo < 1 || verseNo > 99)
				{
					return false;
				}
				if (index < line.Length)
				{
					char next = line[index];
					if (next != '.' && next != ':' && next != ' ' && next != '\t')
					{
						return false;
					}
				}
				if (index < line.Length && (line[index] == '.' || line[index] == ':'))
				{
					index++;
				}
				while (index < line.Length && (line[index] == ' ' || line[index] == '\t'))
				{
					index++;
				}
				if (index < line.Length)
				{
					remainder = line.Substring(index);
				}
				return true;
			}

			static bool TryExtractChorus(string line, out string remainder)
			{
				remainder = "";
				if (line.Length < 6)
				{
					return false;
				}
				if (!line.StartsWith("chorus", StringComparison.OrdinalIgnoreCase))
				{
					return false;
				}
				int index = 6;
				if (index < line.Length && (line[index] == '.' || line[index] == ':'))
				{
					index++;
				}
				while (index < line.Length && (line[index] == ' ' || line[index] == '\t'))
				{
					index++;
				}
				if (index < line.Length)
				{
					remainder = line.Substring(index);
				}
				return true;
			}

			static bool TryNormalizeVersePrefix(string line, out string normalized)
			{
				normalized = line;
				int prefixLen = 0;
				if (line.StartsWith("verse", StringComparison.OrdinalIgnoreCase))
				{
					prefixLen = 5;
				}
				else if (line.StartsWith("ver", StringComparison.OrdinalIgnoreCase))
				{
					prefixLen = 3;
				}
				else if (line.StartsWith("v", StringComparison.OrdinalIgnoreCase))
				{
					prefixLen = 1;
				}
				if (prefixLen == 0 || line.Length <= prefixLen)
				{
					return false;
				}
				int index = prefixLen;
				while (index < line.Length && (line[index] == ' ' || line[index] == '\t'))
				{
					index++;
				}
				int startDigits = index;
				while (index < line.Length && line[index] >= '0' && line[index] <= '9')
				{
					index++;
				}
				if (index == startDigits)
				{
					return false;
				}
				if (!int.TryParse(line.Substring(startDigits, index - startDigits), out int verseNo) || verseNo < 1 || verseNo > 99)
				{
					return false;
				}
				normalized = verseNo + line.Substring(index);
				return true;
			}

			static bool IsHeadingLine(string line)
			{
				return line.Length >= 3 && line[0] == '[' && line[line.Length - 1] == ']';
			}
		}

		public static void Merge_Songs(SongSettings InItem1, SongSettings InItem2, ref string CombinedLyrics, ref string CombinedNotations)
		{
			FormatDisplayLyrics(ref InItem1, PrepareSlides: false, UseStoredSequence: true);
			FormatDisplayLyrics(ref InItem2, PrepareSlides: false, UseStoredSequence: true);
			ListViewItem listViewItem = new ListViewItem();
			TempItem1.LyricsAndNotationsList.Items.Clear();
			bool keepFirstHeading = false;
			for (int i = 0; i < InItem1.SongSequence.Length; i++)
			{
				int verseIndex = InItem1.SongSequence[i];
				if (i == 0 && (InItem1.CompleteLyrics.IndexOf(VerseSymbol[verseIndex]) >= 0 || InItem2.CompleteLyrics.IndexOf(VerseSymbol[verseIndex]) >= 0))
				{
					keepFirstHeading = true;
				}
				listViewItem = TempItem1.LyricsAndNotationsList.Items.Add(VerseSymbol[verseIndex]);
				listViewItem.SubItems.Add("");
				for (int j = InItem1.VerseLineLoc[verseIndex, 1]; j >= 0 && j <= InItem1.VerseLineLoc[verseIndex, 2]; j++)
				{
					listViewItem = TempItem1.LyricsAndNotationsList.Items.Add(InItem1.LyricsAndNotationsList.Items[j].SubItems[2].Text);
					listViewItem.SubItems.Add(InItem1.LyricsAndNotationsList.Items[j].SubItems[3].Text);
				}
				if (InItem2.VersePresent[verseIndex] && InItem2.CompleteLyrics != "")
				{
					listViewItem = TempItem1.LyricsAndNotationsList.Items.Add(VerseSymbol[150]);
					listViewItem.SubItems.Add("");
					for (int k = InItem2.VerseLineLoc[verseIndex, 1]; k >= 0 && k <= InItem2.VerseLineLoc[verseIndex, 2]; k++)
					{
						listViewItem = TempItem1.LyricsAndNotationsList.Items.Add(InItem2.LyricsAndNotationsList.Items[k].SubItems[2].Text);
						listViewItem.SubItems.Add(InItem2.LyricsAndNotationsList.Items[k].SubItems[3].Text);
					}
					InItem2.VersePresent[verseIndex] = false;
				}
			}
			for (int i = 0; i <= 112; i++)
			{
				if (InItem2.VersePresent[i] && InItem2.CompleteLyrics != "")
				{
					int verseIndex = i;
					listViewItem = TempItem1.LyricsAndNotationsList.Items.Add(VerseSymbol[verseIndex]);
					listViewItem.SubItems.Add("");
					listViewItem = TempItem1.LyricsAndNotationsList.Items.Add(VerseSymbol[150]);
					listViewItem.SubItems.Add("");
					for (int k = InItem2.VerseLineLoc[verseIndex, 1]; k >= 0 && k <= InItem2.VerseLineLoc[verseIndex, 2]; k++)
					{
						listViewItem = TempItem1.LyricsAndNotationsList.Items.Add(InItem2.LyricsAndNotationsList.Items[k].SubItems[2].Text);
						listViewItem.SubItems.Add(InItem2.LyricsAndNotationsList.Items[k].SubItems[3].Text);
					}
				}
			}
			if (!keepFirstHeading && TempItem1.LyricsAndNotationsList.Items.Count > 0)
			{
				TempItem1.LyricsAndNotationsList.Items[0].Remove();
			}
			CombinedLyrics = "";
			for (int i = 0; i < TempItem1.LyricsAndNotationsList.Items.Count; i++)
			{
				CombinedLyrics = CombinedLyrics + "\n" + TempItem1.LyricsAndNotationsList.Items[i].SubItems[0].Text;
			}
			CombinedLyrics = DataUtil.Mid(CombinedLyrics, 1);
			CombinedNotations = "";
			for (int i = 0; i < TempItem1.LyricsAndNotationsList.Items.Count; i++)
			{
				if (TempItem1.LyricsAndNotationsList.Items[i].SubItems[1].Text != "")
				{
					CombinedNotations = CombinedNotations + "(" + Convert.ToString(i) + ";" + TempItem1.LyricsAndNotationsList.Items[i].SubItems[1].Text + ")";
				}
			}
			CombinedLyrics = DataUtil.TrimEnd(CombinedLyrics);
		}

		public static bool UpdateRefString(string Instring, string InStringDelim, ref TextBox StoredTextBox, string StoredStringDelim)
		{
			string newEntries = "";
			bool found = false;
			string entry = "";
			string storedEntry = "";
			string[] entries = Instring.Split(InStringDelim[0]);
			string[] storedEntries = StoredTextBox.Text.Split(StoredStringDelim[0]);
			for (int i = 0; i <= entries.GetUpperBound(0); i++)
			{
				entry = DataUtil.Trim(entries[i]).ToLower();
				found = false;
				if (entry == "")
				{
					continue;
				}
				for (int j = 0; j <= storedEntries.GetUpperBound(0); j++)
				{
					storedEntry = DataUtil.Trim(storedEntries[j]).ToLower();
					if (entry == storedEntry)
					{
						found = true;
						j = storedEntries.GetUpperBound(0) + 1;
					}
				}
				if (!found)
				{
					newEntries = newEntries + ((newEntries != "") ? (StoredStringDelim + " ") : "") + DataUtil.Trim(entries[i]);
				}
			}
			if (newEntries != "")
			{
				StoredTextBox.Text = StoredTextBox.Text + ((StoredTextBox.Text != "") ? (StoredStringDelim + " ") : "") + newEntries;
				return true;
			}
			return false;
		}

		public static ListView ExtractLyrics(string InLyrics, string InNotations)
		{
			string OutText = "";
			string OutNotationString = "";
			string OutText2 = "";
			string OutNotationString2 = "";
			return ExtractLyrics(InLyrics, InNotations, ref OutText, ref OutNotationString, ref OutText2, ref OutNotationString2);
		}

		public static ListView ExtractLyrics(string InLyrics, string InNotations, ref string OutText1, ref string OutNotationString1, ref string OutText2, ref string OutNotationString2)
		{
			InLyrics = InLyrics.Replace("\r\n", "\n");
			if (IsNewR2Format(InLyrics))
			{
				return ExtractNewFormatLyrics(InLyrics, InNotations, ref OutText1, ref OutNotationString1, ref OutText2, ref OutNotationString2);
			}
			return ExtractDefaultFormatLyrics(InLyrics, InNotations, ref OutText1, ref OutNotationString1, ref OutText2, ref OutNotationString2);
		}

		public static ListView ExtractNewFormatLyrics(string InLyrics, string InNotations, ref string OutText1, ref string OutNotationString1, ref string OutText2, ref string OutNotationString2)
			=> ExtractLyricRegions(InLyrics, InNotations, true, ref OutText1, ref OutNotationString1, ref OutText2, ref OutNotationString2);

		public static ListView ExtractDefaultFormatLyrics(string InLyrics, string InNotations, ref string OutText1, ref string OutNotationString1, ref string OutText2, ref string OutNotationString2)
			=> ExtractLyricRegions(InLyrics, InNotations, false, ref OutText1, ref OutNotationString1, ref OutText2, ref OutNotationString2);

		private static ListView ExtractLyricRegions(string lyrics, string notations, bool separateRegions,
			ref string firstText, ref string firstNotations, ref string secondText, ref string secondNotations)
		{
			var region1Text = new StringBuilder();
			var region2Text = new StringBuilder();
			var region1Notes = new StringBuilder();
			var region2Notes = new StringBuilder();
			int region1Line = -1, region2Line = -1;
			int lineIndex = -1, notationLineIndex = -1;
			int verseNumber = 1;
			bool inSecondRegion = false;
			string currentVerseHeading = string.Empty;
			ReadOnlySpan<char> remainingNotes = notations;
			ReadOnlySpan<char> noteFields = default;
			ReadOnlySpan<char> text = lyrics;
			var headings = verseNumbersByHeading.GetAlternateLookup<ReadOnlySpan<char>>();
			var rows = new ListView();
			SetListViewColumns(rows, 4);
			foreach (Range range in text.Split('\n'))
			{
				lineIndex++;
				var line = text[range];
				// Stored notation is read in order, with at most one record consumed per lyric line.
				if (notationLineIndex < lineIndex && !remainingNotes.IsEmpty &&
					!TryReadNotationRecord(ref remainingNotes, out notationLineIndex, out noteFields))
					remainingNotes = default;

				bool isHeading = headings.TryGetValue(line, out int heading);
				if (isHeading && heading == REGION2)
				{
					inSecondRegion = true;
					if (!separateRegions && currentVerseHeading.Length > 0)
					{
						region2Text.Append(currentVerseHeading).Append('\n');
						region2Line++;
					}
					continue;
				}
				if (isHeading)
				{
					verseNumber = heading;
					currentVerseHeading = VerseSymbol[heading];
					// In the older format each new verse starts in region 1 again.
					if (!separateRegions)
						inSecondRegion = false;
				}

				int regionLine = inSecondRegion ? ++region2Line : ++region1Line;
				var regionText = inSecondRegion ? region2Text : region1Text;
				regionText.Append(line).Append('\n');
				var chords = lineIndex == notationLineIndex ? noteFields : default;
				if (!chords.IsEmpty)
				{
					var regionNotes = inSecondRegion ? region2Notes : region1Notes;
					regionNotes.Append('(').Append(regionLine).Append(';').Append(chords).Append(')');
				}
				if (!isHeading)
				{
					var row = rows.Items.Add(verseNumber.ToString());
					row.SubItems.Add(inSecondRegion ? "2" : "1");
					row.SubItems.Add(line.ToString());
					row.SubItems.Add(chords.ToString());
				}
			}
			firstText = FinishRegionText(region1Text);
			secondText = FinishRegionText(region2Text);
			firstNotations = region1Notes.ToString();
			secondNotations = region2Notes.ToString();
			return rows;

			static string FinishRegionText(StringBuilder region)
			{
				while (region.Length > 0 && region[^1] is '\r' or '\n')
					region.Length--;
				return region.ToString();
			}
		}

		public static string AddNotationLineNumber(string InOneNotationLine, int InNewLineNumber)
		{
			if (InOneNotationLine != "")
			{
				return "(" + InNewLineNumber + ';' + InOneNotationLine + ")";
			}
			return "";
		}

		public static string RTFCheck(string InString)
		{
			InString = InString.Replace("{", "\\{");
			return InString.Replace("}", "\\}");
		}

		public static string Html_MusicDisplayName(string title1, string title2, string HTMLIndexDir)
		{
			string DirPath = "";
			string FileName = "";
			if (GetMusicFileName(title1, title2, ref DirPath, ref FileName, StoreDirPath: true) == "")
			{
				return "";
			}
			int commonPrefixEnd = 0;
			if (DirPath == HTMLIndexDir)
			{
				return FileName;
			}
			for (int i = 0; i < HTMLIndexDir.Length; i++)
			{
				if (DataUtil.Mid(HTMLIndexDir, i, 1) != DataUtil.Mid(DirPath, i, 1))
				{
					commonPrefixEnd = i - 1;
					i = HTMLIndexDir.Length;
				}
			}
			if (commonPrefixEnd < 1)
			{
				return DirPath + FileName;
			}
			string relativePath = "";
			for (int i = commonPrefixEnd + 1; i <= HTMLIndexDir.Length; i++)
			{
				if (DataUtil.Mid(HTMLIndexDir, i, 1) == "\\")
				{
					relativePath += "..\\";
				}
			}
			relativePath += DataUtil.Mid(DirPath, commonPrefixEnd + 1);
			return relativePath + FileName;
		}

		// GetMusicFileName methods redirect to GetMediaFileName for compatibility
		public static void AlertSettings(AlertType InAlertType)
		{
			ParentalAlertLive = false;
			MessageAlertLive = false;
			AlertTimeRemaining = 0;
			Alert_FormattedMessage = "";
			Alert_MessageHeight = 0;
			Alert_MessageDisplayed = false;
			switch (InAlertType)
			{
				case AlertType.Parental:
					Alert_OriginalMessage = DataUtil.Trim(ParentalAlertHeading) + " " + ParentalAlertDetails;
					Alert_TextAlign = ParentalAlertTextAlign;
					Alert_VerticalAlign = ParentalAlertVerticalAlign;
					Alert_TextColour = ParentalAlertTextColour;
					Alert_BackColour = ParentalAlertBackColour;
					Alert_Flash = ParentalAlertFlash;
					Alert_Scroll = ParentalAlertScroll;
					Alert_Transparent = ParentalAlertTransparent;
					AlertTimeRemaining = ParentalAlertDuration;
					Alert_UserFont = GetReusedFont(Alert_UserFont, ref AlertUserFontKey, ParentalAlertFontName, ParentalAlertFontSize, ParentalAlertBold, ParentalAlertItalic, ParentalAlertUnderline);
					Alert_UserFontShadow = ParentalAlertShadow;
					Alert_UserFontOutline = ParentalAlertOutline;
					BuildAlertSequence();
					Alert_NewMessage = true;
					ParentalAlertLive = true;
					break;
				case AlertType.Message:
					Alert_OriginalMessage = MessageAlertDetails;
					Alert_TextAlign = MessageAlertTextAlign;
					Alert_VerticalAlign = MessageAlertVerticalAlign;
					Alert_TextColour = MessageAlertTextColour;
					Alert_BackColour = MessageAlertBackColour;
					Alert_Flash = MessageAlertFlash;
					Alert_Scroll = MessageAlertScroll;
					Alert_Transparent = MessageAlertTransparent;
					AlertTimeRemaining = MessageAlertDuration;
					Alert_UserFont = GetReusedFont(Alert_UserFont, ref AlertUserFontKey, MessageAlertFontName, MessageAlertFontSize, MessageAlertBold, MessageAlertItalic, MessageAlertUnderline);
					Alert_UserFontShadow = MessageAlertShadow;
					Alert_UserFontOutline = MessageAlertOutline;
					BuildAlertSequence();
					Alert_NewMessage = true;
					MessageAlertLive = true;
					break;
			}
		}

		public static void BuildAlertSequence()
		{
			string flashSequence = "";
			if (Alert_Flash)
			{
				flashSequence = "R" + '>' + "R" + '>' + "R" + '>' + "R" + '>';
			}
			Alert_FormatSequence = flashSequence;
			bool scrollMessage = Alert_Scroll;
			if ((Alert_OriginalMessage.Length + 30) * AlertGap > AlertTimeRemaining)
			{
				scrollMessage = false;
			}
			if (scrollMessage)
			{
				for (int i = 0; i < Alert_OriginalMessage.Length; i++)
				{
					Alert_FormatSequence = Alert_FormatSequence + "S" + '>';
				}
				Alert_FormattedMessage = "";
				Alert_MessageLength = 0;
			}
			else if (!scrollMessage)
			{
				for (int i = 0; i < Alert_OriginalMessage.Length; i++)
				{
					Alert_FormatSequence = Alert_FormatSequence + "A" + '>';
				}
				Alert_FormattedMessage = "";
				Alert_MessageLength = Alert_OriginalMessage.Length;
			}
			Alert_FormatSequence += flashSequence;
			for (int i = 1; i <= 30; i++)
			{
				Alert_FormatSequence = Alert_FormatSequence + "A" + '>';
			}
			if (Alert_Scroll)
			{
				Alert_FormatSequence = Alert_FormatSequence + "C" + '>';
			}
			Alert_FormatOriginalSequence = Alert_FormatSequence;
		}

		private const int AlertHistoryLimit = 20;

		internal static void RecordAlertHistory(ComboBox history, string filePath)
		{
			if (history.Items.Count == 0 || history.Text != history.Items[0].ToString())
			{
				history.Items.Insert(0, history.Text);
			}
			else if (history.Items.Count <= AlertHistoryLimit)
			{
				return;
			}

			while (history.Items.Count > AlertHistoryLimit)
			{
				history.Items.RemoveAt(history.Items.Count - 1);
			}
			SaveComboBoxToTextFile(ref history, filePath);
		}

		public static void LoadComboBoxFromTextFile(ref ComboBox InComboBox, string InTextFile)
		{
			InComboBox.Items.Clear();
			string InString = "";
			if (gfFileHelpers.LoadFileContents(InTextFile, ref InString))
			{
				InString = InString.Replace("\r\n", "\n");
				try
				{
					string[] lines = InString.Split("\n"[0]);
					for (int i = 0; i <= lines.GetUpperBound(0) && i < AlertHistoryLimit; i++)
					{
						if (DataUtil.Trim(lines[i]) != "")
						{
							InComboBox.Items.Add(DataUtil.Trim(lines[i]));
						}
					}
				}
				catch
				{
				}
			}
			InComboBox.Text = "";
		}

		public static void SaveComboBoxToTextFile(ref ComboBox InComboBox, string InTextFile)
		{
			string fileContents = "";
			for (int i = 0; i < InComboBox.Items.Count && i < AlertHistoryLimit; i++)
			{
				if (DataUtil.Trim(InComboBox.Items[i].ToString()) != "")
				{
					fileContents = fileContents + DataUtil.TrimEnd(DataUtil.Trim(InComboBox.Items[i].ToString())) + "\r\n";
				}
			}
			FileUtil.CreateNewFile(InTextFile, FileUtil.FileContentsType.DoubleByte, fileContents);
		}

		public static void LoadListViewFromTextFile(ref ListView InListView, string InTextFile)
		{
			InListView.Items.Clear();
			string InString = "";
			if (gfFileHelpers.LoadFileContents(InTextFile, ref InString))
			{
				InString = InString.Replace("\r\n", "\n");
				try
				{
					string[] lines = InString.Split("\n"[0]);
					for (int i = 0; i <= lines.GetUpperBound(0); i++)
					{
						if (DataUtil.Trim(lines[i]) != "")
						{
							InListView.Items.Add(DataUtil.Trim(lines[i]));
						}
					}
				}
				catch
				{
				}
			}
			InListView.Text = "";
		}

		public static void SaveListViewToTextFile(ref ListView InListView, string InTextFile)
		{
			string fileContents = "";
			for (int i = 0; i < InListView.Items.Count; i++)
			{
				if (DataUtil.Trim(InListView.Items[i].Text) != "")
				{
					fileContents = fileContents + DataUtil.TrimEnd(DataUtil.Trim(InListView.Items[i].Text)) + "\r\n";
				}
			}
			FileUtil.CreateNewFile(InTextFile, FileUtil.FileContentsType.DoubleByte, fileContents);
		}

		public static void MapLyricsBreak(ref ListView screenBreaks, ref RichTextBox editor, ref bool screenBreakListDone)
		{
			string lyrics = DataUtil.TrimStart(editor.Text);
			editor.Text = lyrics;
			screenBreaks.Items.Clear();
			editor.Focus();

			string currentHeading = ValidateVerseIndicator(lyrics, 0);
			AddItemToScreenBreak(ref screenBreaks, 0, currentHeading, 0);
			int nextScreenNumber = 1;
			for (int position = Math.Max(1, currentHeading.Length); position < lyrics.Length; position++)
			{
				if (lyrics[position] == '[')
				{
					string heading = ValidateVerseIndicator(lyrics, position);
					if (heading.Length == 0)
						continue;
					if (heading != currentHeading)
						nextScreenNumber = 0;
					currentHeading = heading;
					AddItemToScreenBreak(ref screenBreaks, position, currentHeading, nextScreenNumber++);
					position += heading.Length - 1;
				}
				else if (lyrics[position] == '\n' && position + 1 < lyrics.Length && lyrics[position + 1] == '\n')
				{
					position++;
					AddItemToScreenBreak(ref screenBreaks, position + 1, currentHeading, nextScreenNumber++);
				}
			}
			screenBreakListDone = true;
		}

		public static string ValidateVerseIndicator(string lyrics, int position)
		{
			ReadOnlySpan<char> text = lyrics;
			return (uint)position < (uint)text.Length ? ReadVerseIndicator(text[position..]) : string.Empty;
		}

		private static string ReadVerseIndicator(ReadOnlySpan<char> text)
		{
			if (text.IsEmpty || text[0] != '[')
				return string.Empty;

			var afterBracket = text[1..];
			int end = afterBracket.IndexOfAny('[', ']', '\n');
			if (end < 0 || afterBracket[end] != ']')
				return string.Empty;

			var heading = text[..(end + 2)];
			var headings = verseNumbersByHeading.GetAlternateLookup<ReadOnlySpan<char>>();
			return headings.TryGetValue(heading, out int verse) && verse != REGION2 ? VerseSymbol[verse] : string.Empty;
		}

		public static void AddItemToScreenBreak(ref ListView InListView, int NewPosition, string VerseSym, int ScreenBreakCount)
		{
			ListViewItem listViewItem = InListView.Items.Add(NewPosition.ToString());
			listViewItem.SubItems.Add(VerseSym);
			listViewItem.SubItems.Add(ScreenBreakCount.ToString());
		}

		public static void GetBreakPosition(ListView InListView, int CurPosition, int Direction, ref int NewPosition, ref int NewPositionLength, ref string LookupVerseSym, ref int LookupScreenCount)
		{
			if (InListView.Items.Count == 0)
			{
				LookupVerseSym = "";
				NewPosition = 0;
				NewPositionLength = -1;
				LookupScreenCount = 0;
				return;
			}
			int breakIndex = -1;
			if (Direction == 0)
			{
				int itemIndex = InListView.Items.Count - 1;
				while (true)
				{
					if (itemIndex >= 0)
					{
						if (DataUtil.StringToInt(InListView.Items[itemIndex].SubItems[0].Text) < CurPosition)
						{
							breakIndex = itemIndex;
							break;
						}
						itemIndex--;
						continue;
					}
					breakIndex = 0;
					break;
				}
			}
			else
			{
				int itemIndex = 0;
				while (true)
				{
					if (itemIndex < InListView.Items.Count)
					{
						if (DataUtil.StringToInt(InListView.Items[itemIndex].SubItems[0].Text) > CurPosition)
						{
							if (NewPositionLength == 0 && itemIndex > 0)
							{
								itemIndex--;
							}
							breakIndex = itemIndex;
							break;
						}
						itemIndex++;
						continue;
					}
					breakIndex = InListView.Items.Count - 1;
					break;
				}
			}
			NewPosition = DataUtil.StringToInt(InListView.Items[breakIndex].SubItems[0].Text);
			LookupVerseSym = InListView.Items[breakIndex].SubItems[1].Text;
			LookupScreenCount = DataUtil.StringToInt(InListView.Items[breakIndex].SubItems[2].Text);
			if (breakIndex < InListView.Items.Count - 1)
			{
				int nextPosition = DataUtil.StringToInt(InListView.Items[breakIndex + 1].SubItems[0].Text);
				NewPositionLength = nextPosition - NewPosition;
			}
			else
			{
				NewPositionLength = -1;
			}
		}

		public static void GetBreakPosition(ListView InListView, ref int NewPosition, ref int NewPositionLength, string LookupVerseSym, int LookupScreenCount)
		{
			if (InListView.Items.Count == 0)
			{
				LookupVerseSym = "";
				NewPosition = 0;
				NewPositionLength = 0;
				LookupScreenCount = 0;
				return;
			}
			int breakIndex = -1;
			int itemIndex = 0;
			while (true)
			{
				if (itemIndex < InListView.Items.Count)
				{
					if (InListView.Items[itemIndex].SubItems[1].Text == LookupVerseSym && DataUtil.StringToInt(InListView.Items[itemIndex].SubItems[2].Text) == LookupScreenCount)
					{
						breakIndex = itemIndex;
						break;
					}
					itemIndex++;
					continue;
				}
				if (breakIndex >= 0)
				{
					break;
				}
				LookupVerseSym = "";
				NewPosition = 0;
				NewPositionLength = 0;
				LookupScreenCount = 0;
				return;
			}
			NewPosition = DataUtil.StringToInt(InListView.Items[breakIndex].SubItems[0].Text);
			if (breakIndex < InListView.Items.Count - 1)
			{
				int nextPosition = DataUtil.StringToInt(InListView.Items[breakIndex + 1].SubItems[0].Text);
				NewPositionLength = nextPosition - NewPosition;
			}
			else
			{
				NewPositionLength = -1;
			}
		}

		internal static void FormatEditorLyrics(RichTextBox editor, ref string previousText, string[] headings,
			Font lyricFont, Font chordFont, bool formatAll = false)
		{
			// RichEdit returns LF offsets after its native handle has been created.
			_ = editor.Handle;
			string text = editor.Text;
			if (text.Length > 0)
			{
				int start = 0;
				int end = text.Length;
				if (!formatAll && previousText != null)
					FindEditedLines(text, previousText, editor.SelectionStart, out start, out end);
				if (start < end || start == text.Length)
					FormatLyricsRange(editor, text, start, end, headings, lyricFont, chordFont);
			}
			previousText = text;
		}

		private static void FindEditedLines(string text, string previous, int caret, out int start, out int end)
		{
			// Match the suffix beyond the caret first so that pasting a duplicate line formats the new copy.
			int after = text.Length;
			int oldAfter = previous.Length;
			while (after > caret && oldAfter > 0 && text[after - 1] == previous[oldAfter - 1])
			{
				after--;
				oldAfter--;
			}
			int first = text.AsSpan(0, after).CommonPrefixLength(previous.AsSpan(0, oldAfter));
			while (after > first && oldAfter > first && text[after - 1] == previous[oldAfter - 1])
			{
				after--;
				oldAfter--;
			}

			start = text.AsSpan(0, first).LastIndexOf('\n') + 1;
			// Adding or removing complete rows leaves the next row intact. A split or join
			// within a row also needs the unchanged suffix of that row to be styled.
			bool suffixStartsRow = (after == 0 || text[after - 1] == '\n')
				&& (oldAfter == 0 || previous[oldAfter - 1] == '\n');
			if (suffixStartsRow && (after > first || oldAfter > first))
				end = after;
			else
			{
				int newline = text.IndexOf('\n', after);
				end = newline < 0 ? text.Length : newline + 1;
			}
		}

		private static void FormatLyricsRange(RichTextBox editor, string text, int start, int end,
			string[] headings, Font lyricFont, Font chordFont)
		{
			int selectionStart = editor.SelectionStart;
			int selectionLength = editor.SelectionLength;
			bool suspendRedraw = editor.Visible;
			if (suspendRedraw)
				SendMessage(editor.Handle, WM_SETREDRAW, 0u, 0u);
			try
			{
				ApplyLyricStyle(editor, start, end - start, lyricFont, NormalTextColour);
				var range = text.AsSpan(start, end - start);
				foreach (string heading in headings.AsSpan())
				{
					if (string.IsNullOrEmpty(heading))
						continue;
					int offset = 0;
					while (offset < range.Length)
					{
						int match = range[offset..].IndexOf(heading.AsSpan(), StringComparison.Ordinal);
						if (match < 0)
							break;
						offset += match;
						editor.Select(start + offset, heading.Length);
						editor.SelectionColor = SelectedTextColour;
						if (heading != VerseSymbol[150])
							break;
						offset += heading.Length;
					}
				}

				int lineStart = start;
				while (lineStart < end)
				{
					int marker = text.IndexOf('»', lineStart, end - lineStart);
					if (marker < 0)
						break;
					lineStart += text.AsSpan(lineStart, marker - lineStart).LastIndexOf('\n') + 1;
					int newline = text.IndexOf('\n', marker, end - marker);
					int lineEnd = newline < 0 ? end : newline + 1;
					ApplyLyricStyle(editor, lineStart, lineEnd - lineStart, chordFont, NotationColour);
					lineStart = lineEnd;
				}
			}
			finally
			{
				editor.Select(selectionStart, selectionLength);
				if (suspendRedraw)
					SendMessage(editor.Handle, WM_SETREDRAW, 1u, 0u);
				editor.Refresh();
			}
		}

		private static void ApplyLyricStyle(RichTextBox editor, int start, int length, Font font, Color color)
		{
			editor.Select(start, length);
			editor.SelectionCharOffset = 0;
			editor.SelectionFont = font;
			editor.SelectionColor = color;
		}

		public static void MarkSelectedRTB(ref RichTextBox InTextBox, int SelStartPos, int SelLen, int InMode, Font MainFont, Font NotationFont)
		{
			SendMessage(InTextBox.Handle, 11u, 0u, 0u);
			InTextBox.SelectionStart = SelStartPos;
			InTextBox.SelectionLength = SelLen;
			switch (InMode)
			{
				case 0:
					InTextBox.SelectionCharOffset = 0;
					InTextBox.SelectionFont = MainFont;
					InTextBox.SelectionColor = NormalTextColour;
					break;
				case 1:
					InTextBox.SelectionCharOffset = 0;
					InTextBox.SelectionFont = MainFont;
					InTextBox.SelectionColor = SelectedTextColour;
					break;
				case 2:
					InTextBox.SelectionCharOffset = 0;
					InTextBox.SelectionFont = NotationFont;
					InTextBox.SelectionColor = NotationColour;
					break;
			}
			InTextBox.SelectionStart = SelStartPos + SelLen;
			InTextBox.SelectionLength = 0;
			InTextBox.SelectionColor = NormalTextColour;
			SendMessage(InTextBox.Handle, 11u, 1u, 0u);
			InTextBox.Refresh();
		}

		public static bool SaveXMLInfoScreen(SongSettings InItem, string InFileName, string[] InHeaderData, bool ReloadImageData, bool UseOriginalNotations)
		{
			try
			{
				using var writer = new XmlTextWriter(InFileName, Encoding.UTF8);
				return WriteInfoScreenDocument(writer, InItem, InHeaderData, ReloadImageData, UseOriginalNotations);
			}
			catch
			{
				return false;
			}
		}

		internal static string SaveInfoScreenCopy(SongSettings song, string directory)
		{
			string title = MakeTitleValidFileName(song.Title);
			string temp = Path.Combine(directory, Path.GetRandomFileName());
			var stream = new FileStream(temp, FileMode.CreateNew, FileAccess.Write, FileShare.None);
			try
			{
				using (stream)
				using (var writer = new XmlTextWriter(stream, Encoding.UTF8))
				{
					if (!WriteInfoScreenDocument(writer, song, null, false, false))
						return "";
				}

				for (int copy = 0; ; copy++)
				{
					string name = copy == 0 ? title : $"{title} - Copy ({copy})";
					string path = Path.Combine(directory, name + ".esi");
					try
					{
						File.Move(temp, path);
						return path;
					}
					catch (IOException) when (File.Exists(path)) { }
				}
			}
			finally
			{
				File.Delete(temp);
			}
		}

		private static bool WriteInfoScreenDocument(XmlTextWriter writer, SongSettings song, string[] header, bool reloadImage, bool originalNotations)
		{
			writer.Formatting = Formatting.Indented;
			writer.WriteStartDocument();
			writer.WriteStartElement("EasiSlides");
			if (!WriteXMLOneItem(ref writer, song, header, reloadImage, originalNotations))
				return false;
			writer.WriteEndDocument();
			return true;
		}

		public static void WriteXMLOneItem(ref XmlTextWriter xtw, SongSettings InItem, string[] InHeaderData, bool ReloadImageData)
		{
			WriteXMLOneItem(ref xtw, InItem, InHeaderData, ReloadImageData, UseOriginalNotations: true);
		}

		private static bool WriteXMLOneItem(ref XmlTextWriter xtw, SongSettings InItem, string[] InHeaderData, bool ReloadImageData, bool UseOriginalNotations)
		{
			string formatData = InItem.Format.FormatString;
			if (InHeaderData != null)
			{
				for (int i = 2; i <= 254; i++)
				{
					if (InHeaderData[i] != "" && InHeaderData[i] != null)
					{
						formatData = formatData + i + "=" + InHeaderData[i] + ">";
					}
				}
			}
			try
			{
				xtw.WriteStartElement("Item");
				xtw.WriteElementString("Title1", InItem.Title);
				xtw.WriteElementString("Title2", InItem.Title2);
				xtw.WriteElementString("Folder", Folders[InItem.FolderNo].Name);
				xtw.WriteElementString("SongNumber", InItem.SongNumber.ToString());
				InItem.CompleteLyrics = InItem.CompleteLyrics.Replace("\r\n", "\n");
				InItem.CompleteLyrics = InItem.CompleteLyrics.Replace("\n", "\r\n");
				xtw.WriteElementString("Contents", InItem.CompleteLyrics);
				xtw.WriteElementString("Notations", UseOriginalNotations ? InItem.OriginalNotations : InItem.Notations);
				xtw.WriteElementString("Sequence", ConvertSequenceToTextString(InItem.SongSequence));
				xtw.WriteElementString("Writer", InItem.Writer);
				xtw.WriteElementString("Copyright", InItem.Copyright);
				xtw.WriteElementString("Category", InItem.Category);
				xtw.WriteElementString("Timing", InItem.Timing);
				xtw.WriteElementString("MusicKey", InItem.MusicKey);
				xtw.WriteElementString("Capo", InItem.Capo.ToString());
				xtw.WriteElementString("LicenceAdmin1", InItem.Show_LicAdminInfo1);
				xtw.WriteElementString("LicenceAdmin2", InItem.Show_LicAdminInfo2);
				xtw.WriteElementString("BookReference", InItem.Book_Reference);
				xtw.WriteElementString("UserReference", InItem.User_Reference);
				xtw.WriteElementString("FormatData", formatData);
				xtw.WriteElementString("Settings", InItem.Settings);
				if (ReloadImageData && InItem.Format.BackgroundPicture != null && InItem.Format.BackgroundPicture != "")
				{
					Base64EncodeImageFile(ref xtw, "Image", InItem.Format.BackgroundPicture);
				}
				else if (InItem.Format.ImageString != "")
				{
					xtw.WriteElementString("Image", InItem.Format.ImageString);
				}
				xtw.WriteEndElement();
				return true;
			}
			catch
			{
				return false;
			}
		}

		public static void Base64EncodeImageFile(ref XmlTextWriter xtw, string InElementString, string InFileName)
		{
			FileInfo fileInfo = new FileInfo(InFileName);
			using FileStream fileStream = fileInfo.OpenRead();
			byte[] fileBytes = new byte[fileInfo.Length];
			fileStream.ReadExactly(fileBytes, 0, fileBytes.Length);
			//fileStream.Close();
			xtw.WriteStartElement(InElementString);
			xtw.WriteBase64(fileBytes, 0, fileBytes.Length);
			xtw.WriteEndElement();
		}

		public static string ConvertSequenceToTextString(string InSequence)
		{
			if (InSequence.Length > 0)
			{
				string sequenceText = "";
				for (int i = 0; i < InSequence.Length; i++)
				{
					int verseCode = InSequence[i];
					sequenceText = ((verseCode <= 0 || verseCode >= 13) ? (sequenceText + SequenceSymbol[verseCode]) : (sequenceText + verseCode));
					if (i < InSequence.Length - 1)
					{
						sequenceText += ",";
					}
				}
				return sequenceText;
			}
			return "";
		}

		public static string ConvertTextStringToSequence(string InSequence)
		{
			if (InSequence.Length > 0)
			{
				InSequence = InSequence.ToLower();
				string token = "";
				int verseCode = -1;
				string sequence = "";
				while (InSequence != "")
				{
					token = DataUtil.ExtractOneInfo(ref InSequence, ',');
					verseCode = -1;
					if (token != "")
					{
						switch (token)
						{
							case "c":
								verseCode = 0;
								break;
							case "b":
								verseCode = 100;
								break;
							case "w":
								verseCode = 103;
								break;
							case "e":
								verseCode = 101;
								break;
							case "t":
								verseCode = 102;
								break;
							case "p":
								verseCode = 111;
								break;
							case "q":
								verseCode = 112;
								break;
							default:
								verseCode = DataUtil.StringToInt(token, Minus1IfBlank: true);
								if (verseCode > 9 || verseCode < 0)
								{
									verseCode = -1;
								}
								break;
						}
					}
					if (verseCode >= 0)
					{
						sequence += (char)verseCode;
					}
				}
				return sequence;
			}
			return "";
		}

		public static string FormatMode(int part) => PraiseBookTextStyles[part].ToRtf(part);


		public static bool LoadDataIntoItem(ref SongSettings InItem, DataTable dt, string InID)
		{
			try
			{
				DataRow[] arrRows = null;
				arrRows = dt.Select("SONGID='" + InID + "'");

				if (arrRows != null && arrRows.Length > 0)
				{
					return LoadDataIntoItem(ref InItem, arrRows[0]);
				}

				InitialiseIndividualData(ref InItem);
			}
			catch
			{
			}
			return false;
		}

		public static bool LoadDataIntoItem(ref SongSettings InItem, DataRow dr)
		{
			InitialiseIndividualData(ref InItem);
			try
			{
				InItem.Title = DataUtil.GetDataString(dr, "Title_1");
				InItem.Title2 = DataUtil.GetDataString(dr, "Title_2");
				InItem.SongNumber = DataUtil.GetDataInt(dr, "song_number");
				InItem.CompleteLyrics = DataUtil.GetDataString(dr, "Lyrics");
				InItem.FolderNo = DataUtil.GetDataInt(dr, "FolderNo");
				InItem.Copyright = DataUtil.GetDataString(dr, "Copyright");
				InItem.Show_LicAdminInfo1 = DataUtil.GetDataString(dr, "LICENCE_ADMIN1");
				InItem.Show_LicAdminInfo2 = DataUtil.GetDataString(dr, "LICENCE_ADMIN2");
				InItem.Format.FormatString = DataUtil.GetDataString(dr, "FORMATDATA");
				InItem.Notations = DataUtil.GetDataString(dr, "msc");
				InItem.Capo = DataUtil.GetDataInt(dr, "capo", Minus1IfBlank: true);
				InItem.SongSequence = DataUtil.GetDataString(dr, "Sequence");
				InItem.Writer = DataUtil.GetDataString(dr, "Writer");
				InItem.Book_Reference = DataUtil.GetDataString(dr, "Book_Reference");
				InItem.User_Reference = DataUtil.GetDataString(dr, "User_Reference");
				InItem.Timing = DataUtil.GetDataString(dr, "timing");
				InItem.MusicKey = DataUtil.GetDataString(dr, "key");
				InItem.Category = DataUtil.GetDataString(dr, "category");
				InItem.Settings = DataUtil.GetDataString(dr, "SETTINGS");
				return true;
			}
			catch
			{
			}
			return false;
		}

		public static string LookUpMediaString(DShowLib Player)
		{
			switch (LookUpMediaInteger(Player))
			{
				case 1:
					return "Audio Only";
				case 2:
					return "Video";
				default:
					return "No Media Playing";
			}
		}

		public static string LookUpMediaString(DShowLib Player, int InCode, int WaitCount)
		{
			switch (InCode)
			{
				case -1:
					return "Selected Source Not Playable";
				case 0:
					{
						WaitCount /= 3;
						string dots = "";
						for (int i = 0; i < WaitCount; i++)
						{
							dots += ".";
						}
						return "Connecting to media..." + dots;
					}
				default:
					return "No Media Playing";
			}
		}

		public static int LookUpMediaInteger(DShowLib Player)
		{
			try
			{
				if (Player.newFilename != "")
				{
					if (Player.isVideo)
					{
						return 1;
					}
					return 2;
				}
				return 3;
			}
			catch
			{
				return 3;
			}
		}

		public static string GetMediaLocation(SongSettings InItem)
		{
			return GetMediaLocation(InItem.Format.MediaOption, InItem.Title, InItem.Title2, InItem.UseDefaultFormat, InItem.Type, InItem.Format.MediaLocation, InItem.Format.MediaCaptureDeviceNumber);
		}

		public static string GetMediaLocation(int InMediaOption, string InTitle1, string InTitle2, bool InUseDefaultFormat, string InType, string InMediaLocation, int InMediaCaptureDeviceNumber)
		{
			InMediaOption = ((InUseDefaultFormat && InType != "M") ? MediaOption : InMediaOption);
			switch (InMediaOption)
			{
				case 1:
					return GetMediaFileName(InTitle1, InTitle2);
				case 2:
					return (InUseDefaultFormat && InType != "M") ? MediaLocation : InMediaLocation;
				case 3:
					return "<<Capture>>" + (InUseDefaultFormat ? MediaCaptureDeviceNumber.ToString() : InMediaCaptureDeviceNumber.ToString());
				default:
					return "";
			}
		}

		public static void GetRotationStyle(ref SongSettings InItem)
		{
			string InString = InItem.RotateString;
			string InString2 = "";
			string sequenceText = "";
			// The rotate timings follow a '»' separator; also accept the mis-encoded "Â»" form.
			string[] parts = InString.Replace("Â»", "»").Split('»');
			if (parts.GetUpperBound(0) >= 0)
			{
				InString = parts[0];
				if (parts.GetUpperBound(0) >= 1)
				{
					InString2 = parts[1];
				}
			}
			int rotateStyle = DataUtil.StringToInt(DataUtil.ExtractOneInfo(ref InString, ';', RemoveExtract: true, MinusOneIfBlank: false));
			if (rotateStyle < 1 || rotateStyle > 2)
			{
				rotateStyle = 1;
			}
			InItem.RotateStyle = rotateStyle;
			int rotateValue = DataUtil.StringToInt(DataUtil.ExtractOneInfo(ref InString, ';', RemoveExtract: true, MinusOneIfBlank: false));
			if (rotateValue < 0 || rotateValue > 99999)
			{
				rotateValue = 0;
			}
			InItem.RotateGap = rotateValue;
			int rotateTotal = DataUtil.StringToInt(DataUtil.ExtractOneInfo(ref InString, ';', RemoveExtract: true, MinusOneIfBlank: false));
			if (rotateTotal < 0 || rotateTotal > 3599)
			{
				rotateTotal = 0;
			}
			InItem.RotateTotal = rotateTotal;
			sequenceText = InString;
			InItem.RotateTimings = "";
			InItem.RotateSequence = "";
			while (sequenceText != "")
			{
				rotateValue = DataUtil.StringToInt(DataUtil.ExtractOneInfo(ref sequenceText, ';', RemoveExtract: true, MinusOneIfBlank: true));
				if ((rotateValue >= 0 && rotateValue < 99) || (rotateValue >= 100 && rotateValue < 112))
				{
					InItem.RotateSequence += (char)rotateValue;
				}
			}
			int selectedStyle = rotateStyle;
			if (selectedStyle != 2)
			{
				return;
			}
			ListBox listBox = new ListBox();
			listBox.Sorted = false;
			while (InString2 != "")
			{
				rotateValue = DataUtil.StringToInt(DataUtil.ExtractOneInfo(ref InString2, ';', RemoveExtract: true, MinusOneIfBlank: true));
				if (rotateValue > 0 && rotateValue <= 99999)
				{
					listBox.Items.Add(rotateValue.ToString("00000"));
				}
			}
			if (listBox.Items.Count > 0)
			{
				listBox.Sorted = true;
				string timing = "";
				for (int i = 0; i < listBox.Items.Count; i++)
				{
					timing = DataUtil.StringToInt(listBox.Items[i].ToString()).ToString();
					InItem.RotateTimings = InItem.RotateTimings + timing + ';';
				}
			}
		}

		internal static int GetNextNonRotateItem(bool CurrentItemIsGapItem)
		{
			int startPresAt = StartPresAt;
			int searchFrom = StartPresAt;
			int firstNonRotate = -1;
			int secondNonRotate = -1;
			if (TotalWorshipListItems > 0)
			{
				int itemIndex = searchFrom + 1;
				for (itemIndex = searchFrom + 1; itemIndex <= TotalWorshipListItems; itemIndex++)
				{
					int itemRotateResult = GetItemRotateResult(TempItem2, WorshipSongs[itemIndex].IdString);
					if (itemRotateResult < 1)
					{
						if (firstNonRotate < 0)
						{
							firstNonRotate = itemIndex;
						}
						else if (secondNonRotate < 0)
						{
							secondNonRotate = itemIndex;
						}
						if (secondNonRotate > 0)
						{
							itemIndex = TotalWorshipListItems;
						}
					}
				}
				if (firstNonRotate > 0)
				{
					if (GapItemOption == GapType.None)
					{
						return firstNonRotate;
					}
					if (!CurrentItemIsGapItem)
					{
						return firstNonRotate - 1;
					}
					if (firstNonRotate == searchFrom + 1)
					{
						return firstNonRotate;
					}
					return firstNonRotate - 1;
				}
			}
			return startPresAt;
		}

		internal static int GetItemRotateResult(string InIDString)
		{
			return GetItemRotateResult(TempItem1, InIDString);
		}

		internal static int GetItemRotateResult(SongSettings InItem, string InIDString)
		{
			string idPrefix = DataUtil.Left(InIDString, 1);
			if (idPrefix == "I" || idPrefix == "D")
			{
				string InTitle = "";
				LoadIndividualData(ref InItem, InIDString, "", 1, ref InTitle);
				MediaBackgroundStyle mediaBackgroundType = GetMediaBackgroundType(InItem, UpdateVariables: false);
				switch (InItem.RotateStyle)
				{
					case 1:
						if (InItem.RotateGap > 0)
						{
							return (mediaBackgroundType != MediaBackgroundStyle.Video && mediaBackgroundType != MediaBackgroundStyle.SameAsPrevious) ? 1 : 2;
						}
						break;
					case 2:
						if (InItem.RotateTimings != "" || InItem.RotateTotal > 0)
						{
							return (mediaBackgroundType != MediaBackgroundStyle.Video && mediaBackgroundType != MediaBackgroundStyle.SameAsPrevious) ? 1 : 2;
						}
						break;
				}
			}
			return 0;
		}

		public static MediaBackgroundStyle GetMediaBackgroundType(SongSettings InItem, bool UpdateVariables)
		{
			string mediaLocation = GetMediaLocation(InItem);
			if (mediaLocation == "")
			{
				if (UpdateVariables)
				{
					CurrentMediaLocation = "";
				}
				return MediaBackgroundStyle.None;
			}
			if (mediaLocation == CurrentMediaLocation)
			{
				return MediaBackgroundStyle.SameAsPrevious;
			}
			MediaBackgroundStyle mediaType = GetMediaType(mediaLocation);
			if (UpdateVariables)
			{
				CurrentMediaLocation = mediaLocation;
				CurrentMediaIsVideo = ((mediaType == MediaBackgroundStyle.Video) ? true : false);
			}
			return mediaType;
		}

		public static MediaBackgroundStyle GetMediaType(string InLocation)
		{
			if (DataUtil.Left(InLocation, "<<Capture>>".Length) == "<<Capture>>")
			{
				return MediaBackgroundStyle.Video;
			}
			string extension = "";
			try
			{
				extension = Path.GetExtension(InLocation).ToLower();
			}
			catch
			{
				return MediaBackgroundStyle.Audio;
			}
			for (int i = 0; i < MediaFileExtensions.Count; i++)
			{
				if (MediaFileExtensions[i].Extension == extension)
				{
					if (MediaFileExtensions[i].TypeName == MediaBackgroundStyle.Video.ToString())
					{
						return MediaBackgroundStyle.Video;
					}
					return MediaBackgroundStyle.Audio;
				}
			}
			return MediaBackgroundStyle.Audio;
		}

		public static void SubDivideTextAndNotations(string InString, string InNotation, Font MainFont, Font NotationsFont, ref ListView TextNotationsList, int InWidth)
		{
			//using var perf = PerfLog.Enabled ? PerfLog.Scope("SubDivideTextAndNotations", $"textLen={(InString != null ? InString.Length : 0)} notationLen={(InNotation != null ? InNotation.Length : 0)} width={InWidth}") : null;
			InWidth /= 15;
			using var graphics = TextNotationsList.CreateGraphics();
			using var layout = StringFormat.GenericDefault;
			ListViewItem listViewItem = new ListViewItem();
			int charOffset = 0;
			TextNotationsList.Items.Clear();
			while (InString != "")
			{
				int length = InString.Length;
				for (int chunkLength = length; chunkLength >= 1; chunkLength--)
				{
					string chunk = DataUtil.Left(InString, chunkLength);
					// Consume at least one character even if a leading space is wider than the page.
					bool singleWord = !chunk.Contains(' ');
					if (chunkLength == 1 || ((singleWord || graphics.MeasureString(chunk, MainFont, 32000, layout).Width <= InWidth)
						&& (singleWord || chunk.EndsWith(' ') || chunkLength == length)))
					{
						listViewItem = TextNotationsList.Items.Add(chunk);
						listViewItem.SubItems.Add("");
						string InString2 = InNotation;
						StringBuilder notationBuilder = new StringBuilder();
						while (InString2 != "")
						{
							string chord = DataUtil.ExtractOneInfo(ref InString2, ';');
							string chordPos = DataUtil.ExtractOneInfo(ref InString2, ';');
							if (chord != "-1" && chordPos != "-1" && Convert.ToInt32(chordPos) >= charOffset)
							{
								if (Convert.ToInt32(chordPos) >= charOffset + chunkLength && chunkLength < length)
								{
									InString2 = "";
									continue;
								}
								notationBuilder.Append(chord);
								notationBuilder.Append(';');
								notationBuilder.Append(Convert.ToString(Convert.ToInt32(chordPos) - charOffset));
								notationBuilder.Append(';');
							}
						}
						listViewItem.SubItems.Add((notationBuilder.Length > 0) ? notationBuilder.ToString() : " ");
						listViewItem.SubItems.Add(Convert.ToString(charOffset));
						charOffset += chunkLength;
						InString = DataUtil.Mid(InString, chunkLength);
						chunkLength = 0;
					}
				}
			}
			for (int i = 0; i < TextNotationsList.Items.Count; i++)
			{
				var row = TextNotationsList.Items[i];
				row.SubItems[1].Text = FormatNotationString(graphics, layout, row.Text, row.SubItems[2].Text, MainFont, NotationsFont);
			}
		}

		public static void OldSubDivideTextAndNotations(string InString, string InNotation, Font MainFont, Font NotationsFont, ref ListView TextNotationsList, int InWidth)
		{
			InWidth /= 15;
			Graphics graphics = TextNotationsList.CreateGraphics();
			ListViewItem listViewItem = new ListViewItem();
			int charOffset = 0;
			TextNotationsList.Items.Clear();
			while (InString != "")
			{
				int length = InString.Length;
				for (int chunkLength = length; chunkLength >= 1; chunkLength--)
				{
					string chunk = DataUtil.Left(InString, chunkLength);
					if ((graphics.MeasureString(chunk, MainFont, 32000, StringFormat.GenericDefault).Width <= (float)InWidth || chunk.IndexOf(" ") < 0) && (DataUtil.Right(chunk, 1) == " " || chunkLength == length || chunk.IndexOf(" ") < 0))
					{
						listViewItem = TextNotationsList.Items.Add(chunk);
						listViewItem.SubItems.Add("");
						string InString2 = InNotation;
						string notations = "";
						while (InString2 != "")
						{
							string chord = DataUtil.ExtractOneInfo(ref InString2, ';');
							string chordPos = DataUtil.ExtractOneInfo(ref InString2, ';');
							if (chord != "-1" && chordPos != "-1" && Convert.ToInt32(chordPos) >= charOffset)
							{
								if (Convert.ToInt32(chordPos) >= charOffset + chunkLength && chunkLength < length)
								{
									InString2 = "";
									continue;
								}
								notations = notations + chord + ";" + Convert.ToString(Convert.ToInt32(chordPos) - charOffset) + ";";
							}
						}
						listViewItem.SubItems.Add((notations != "") ? notations : " ");
						listViewItem.SubItems.Add(Convert.ToString(charOffset));
						charOffset += chunkLength;
						InString = DataUtil.Mid(InString, chunkLength);
						chunkLength = 0;
					}
				}
			}
			for (int i = 0; i < TextNotationsList.Items.Count; i++)
			{
			}
		}

		public static void HighlightDisplaySlidesText(SongSettings InItem, ref RichTextBox InTextBox)
		{
			HighlightDisplaySlidesText(InItem, ref InTextBox, ScrollToCaret: true);
		}

		public static void HighlightDisplaySlidesText(SongSettings InItem, ref RichTextBox InTextBox, bool ScrollToCaret)
		{
			HighlightDisplaySlidesText(InItem, ref InTextBox, ScrollToCaret, BlackScreenColour, Color.Red);
		}

		public static void HighlightDisplaySlidesText(SongSettings InItem, ref RichTextBox InTextBox, bool ScrollToCaret, Color TextColour, Color HighlightColour)
		{
			InItem.CurSlide = ((InItem.CurSlide < 1) ? 1 : ((InItem.CurSlide > InItem.TotalSlides) ? InItem.TotalSlides : InItem.CurSlide));
			InTextBox.Select(0, InTextBox.Text.Length);
			InTextBox.SelectionColor = TextColour;
			InTextBox.Select(InItem.Slide[InItem.CurSlide, 5], InItem.Slide[InItem.CurSlide, 6]);
			InTextBox.SelectionColor = HighlightColour;
			InTextBox.SelectionLength = 0;
			if (ScrollToCaret)
			{
				InTextBox.Select(InItem.Slide[InItem.CurSlide, 5] + InItem.Slide[InItem.CurSlide, 6] + 90, 0);
				InTextBox.ScrollToCaret();
				InTextBox.Select(InItem.Slide[InItem.CurSlide, 5], 0);
				InTextBox.ScrollToCaret();
			}
		}

		public static void DisplaySlidesFormattedLyrics(ref SongSettings InItem, ref RichTextBox PInTextBox, ref RichTextBox OInTextBox, bool ScrollToCaret, bool PreviewNotations)
		{
			if (InItem.OutputStyleScreen)
			{
				DisplaySlidesFormattedLyrics(ref InItem, ref OInTextBox, ScrollToCaret, PreviewNotations);
			}
			else
			{
				DisplaySlidesFormattedLyrics(ref InItem, ref PInTextBox, ScrollToCaret, PreviewNotations);
			}
		}

		public static void DisplaySlidesFormattedLyrics(ref SongSettings InItem, ref RichTextBox InTextBox, bool ScrollToCaret, bool PreviewNotations)
		{
			//using var perf = PerfLog.Enabled ? PerfLog.Scope("DisplaySlidesFormattedLyrics", $"total={(InItem != null ? InItem.TotalSlides : 0)} previewNotations={PreviewNotations}") : null;
			InItem.CurSlide = ((InItem.CurSlide < 1) ? 1 : ((InItem.CurSlide > InItem.TotalSlides) ? InItem.TotalSlides : InItem.CurSlide));
			InItem.FolderNo = ((InItem.FolderNo <= 0) ? 1 : InItem.FolderNo);
			InTextBox.Text = "";
			if (InItem.Type == "P")
			{
				return;
			}
			StringBuilder textBuilder = new StringBuilder();
			int slideStart = 0;
			int slideLength = 0;
			for (int i = 1; i <= InItem.TotalSlides; i++)
			{
				slideStart = textBuilder.Length;
				int headingLength = 0;
				try
				{
					if (InItem.Slide[i, 0] >= 0)
					{
						if (i > 1)
						{
							textBuilder.Append('\n');
						}
						if (InItem.Slide[i, 0] == 0)
						{
							textBuilder.Append((Folders[InItem.FolderNo].LyricsHeading[1] != "") ? Folders[InItem.FolderNo].LyricsHeading[1] : "Chorus:");
						}
						else if (InItem.Slide[i, 0] == 102)
						{
							textBuilder.Append((Folders[InItem.FolderNo].LyricsHeading[1] != "") ? (Folders[InItem.FolderNo].LyricsHeading[1] + " (2)") : "Chorus 2:");
						}
						else if (InItem.Slide[i, 0] == 111)
						{
							textBuilder.Append((Folders[InItem.FolderNo].LyricsHeading[0] != "") ? Folders[InItem.FolderNo].LyricsHeading[0] : "Prechorus:");
						}
						else if (InItem.Slide[i, 0] == 112)
						{
							textBuilder.Append((Folders[InItem.FolderNo].LyricsHeading[0] != "") ? (Folders[InItem.FolderNo].LyricsHeading[0] + " (2)") : "Prechorus 2:");
						}
						else if (InItem.Slide[i, 0] == 100)
						{
							textBuilder.Append((Folders[InItem.FolderNo].LyricsHeading[2] != "") ? Folders[InItem.FolderNo].LyricsHeading[2] : "Bridge:");
						}
						else if (InItem.Slide[i, 0] == 103)
						{
							textBuilder.Append((Folders[InItem.FolderNo].LyricsHeading[2] != "") ? (Folders[InItem.FolderNo].LyricsHeading[2] + " (2)") : "Bridge 2:");
						}
						else if (InItem.Slide[i, 0] == 101)
						{
							textBuilder.Append((Folders[InItem.FolderNo].LyricsHeading[3] != "") ? Folders[InItem.FolderNo].LyricsHeading[3] : "Ending:");
						}
						else if (InItem.Verse2Present || (i > 1 && InItem.Slide[i, 0] == 1))
						{
							textBuilder.Append(VerseTitle[InItem.Slide[i, 0]] + ".");
							int verseNumber = InItem.Slide[i, 0];
						}
						headingLength = textBuilder.Length - slideStart;
					}
				}
				catch
				{
					MessageBox.Show("Error");
				}
				textBuilder.Append('\n');
				if (InItem.Slide[i, 2] >= 0)
				{
					textBuilder.Append(GetSlideContents(InItem, i, 0, InTextBox.Font, PreviewNotations));
				}
				if (InItem.Slide[i, 4] >= 0)
				{
					textBuilder.Append(GetSlideContents(InItem, i, 1, InTextBox.Font, PreviewNotations));
				}
				slideLength = textBuilder.Length - slideStart + 1;
				InItem.Slide[i, 5] = slideStart;
				InItem.Slide[i, 6] = slideLength;
				InItem.Slide[i, 7] = headingLength;
			}
			InTextBox.Text = textBuilder.ToString();
			for (int i = 1; i <= InItem.TotalSlides; i++)
			{
				if (InItem.Slide[i, 7] > 0)
				{
					InTextBox.Select(InItem.Slide[i, 5], InItem.Slide[i, 7]);
					InTextBox.SelectionFont = new Font(InTextBox.Font, FontStyle.Regular);
					InTextBox.SelectionLength = 0;
				}
			}
			HighlightDisplaySlidesText(InItem, ref InTextBox, ScrollToCaret);
		}










		private static void ReadIndividualFormatHeader(string format, Span<string> slots)
		{
			int terminator = format.IndexOf('>');
			slots.Fill(string.Empty);
			if (terminator < 0)
				return;

			Span<bool> seen = stackalloc bool[HeaderDataMax];
			seen.Clear();
			ReadOnlySpan<char> remaining = format;
			do
			{
				var entry = remaining[..terminator];
				remaining = remaining[(terminator + 1)..];
				int key = GetIndividualHeaderKey(entry, out int valueStart);
				if (key >= 0 && !seen[key])
				{
					slots[key] = entry[valueStart..].ToString();
					seen[key] = true;
				}
				terminator = remaining.IndexOf('>');
			} while (terminator >= 0);
		}

		private static int GetIndividualHeaderKey(ReadOnlySpan<char> entry, out int valueStart)
		{
			int equals = entry.IndexOf('=');
			valueStart = equals + 1;
			if (entry.IsEmpty)
				return -1;
			if (equals >= 0 && !entry[..equals].ContainsAnyExceptInRange('0', '9'))
			{
				if (equals is < 1 or > 3 || (equals > 1 && entry[0] == '0') ||
					!byte.TryParse(entry[..equals], NumberStyles.None, CultureInfo.InvariantCulture, out byte key) || key >= HeaderDataMax)
					return -1;

				// Combining marks in the value can affect the linguistic match of the preceding '='.
				return entry.IndexOf(entry[..valueStart], StringComparison.CurrentCulture) == 0 ? key : -1;
			}

			// Old headers used culture-sensitive IndexOf. For example, an ignorable character inside
			// "12=" can match, but the value still starts three UTF-16 code units into the entry.
			var comparison = CultureInfo.CurrentCulture.CompareInfo;
			int separator = comparison.IndexOf(entry, "=");
			if (separator <= 0)
				return -1;
			var legacyDigits = entry[..separator];
			Span<char> marker = stackalloc char[4];
			for (int key = 0; key < HeaderDataMax; key++)
			{
				key.TryFormat(marker, out int length, provider: CultureInfo.InvariantCulture);
				if (comparison.Compare(legacyDigits, marker[..length]) != 0)
					continue;
				marker[length] = '=';
				var prefix = marker[..(length + 1)];
				if (comparison.IndexOf(entry, prefix) == 0)
				{
					valueStart = prefix.Length;
					return key;
				}
			}
			return -1;
		}

		public static void LoadIndividualFormatData(ref SongSettings InItem, string InFormatString)
		{
			ReadIndividualFormatHeader(InFormatString, InItem.Format.HeaderData.AsSpan(0, HeaderDataMax));
			int folderNo = InItem.FolderNo;
			int region2FolderNo = folderNo;
			int region2FontIndex = 1;
			if (InItem.Type == "B")
			{
				region2FolderNo = InItem.HBR2_FolderNo;
				region2FontIndex = 0;
			}
			if (folderNo >= 0)
			{
				InItem.FolderNo = folderNo;
				Folders[0].FontBold[0] = Folders[folderNo].FontBold[0];
				Folders[0].FontBold[1] = Folders[region2FolderNo].FontBold[region2FontIndex];
				Folders[0].FontItalic[0] = Folders[folderNo].FontItalic[0];
				Folders[0].FontItalic[1] = Folders[region2FolderNo].FontItalic[region2FontIndex];
				Folders[0].FontUnderline[0] = Folders[folderNo].FontUnderline[0];
				Folders[0].FontUnderline[1] = Folders[region2FolderNo].FontUnderline[region2FontIndex];
				Folders[0].FontRTL[0] = Folders[folderNo].FontRTL[0];
				Folders[0].FontRTL[1] = Folders[region2FolderNo].FontRTL[region2FontIndex];
				Folders[0].FontBold[2] = Folders[folderNo].FontBold[0];
				Folders[0].FontBold[3] = Folders[region2FolderNo].FontBold[region2FontIndex];
				Folders[0].FontItalic[2] = Folders[folderNo].FontItalic[2];
				Folders[0].FontItalic[3] = Folders[region2FolderNo].FontItalic[3];
				Folders[0].FontUnderline[2] = Folders[folderNo].FontUnderline[0];
				Folders[0].FontUnderline[3] = Folders[region2FolderNo].FontUnderline[region2FontIndex];
				Folders[0].FontName[0] = Folders[folderNo].FontName[0];
				Folders[0].FontName[1] = Folders[region2FolderNo].FontName[region2FontIndex];
				Folders[0].FontSize[0] = Folders[folderNo].FontSize[0] * InItem.FontSizeFactor / 100;
				Folders[0].FontSize[1] = Folders[region2FolderNo].FontSize[region2FontIndex] * ((InItem.Type == "B") ? InItem.HBR2_FontSizeFactor : InItem.FontSizeFactor) / 100;
				if (Folders[0].FontSize[0] <= 0)
				{
					Folders[0].FontSize[0] = 6;
				}
				if (Folders[0].FontSize[1] <= 0)
				{
					Folders[0].FontSize[1] = 6;
				}
				Folders[0].BottomMargin = Folders[folderNo].BottomMargin;
				Folders[0].LeftMargin = Folders[folderNo].LeftMargin;
				Folders[0].RightMargin = Folders[folderNo].RightMargin;
				Folders[0].FontVPosition[0] = Folders[folderNo].FontVPosition[0];
				Folders[0].FontVPosition[1] = Folders[folderNo].FontVPosition[1];
				Folders[0].HeadingFontBold[0] = Folders[folderNo].HeadingFontBold[0];
				Folders[0].HeadingFontItalic[0] = Folders[folderNo].HeadingFontItalic[0];
				Folders[0].HeadingFontUnderline[0] = Folders[folderNo].HeadingFontUnderline[0];
				Folders[0].HeadingFontBold[1] = Folders[folderNo].HeadingFontBold[1];
				Folders[0].HeadingFontItalic[1] = Folders[folderNo].HeadingFontItalic[1];
				Folders[0].HeadingFontUnderline[1] = Folders[folderNo].HeadingFontUnderline[1];
				Folders[0].HeadingPercentSize = Folders[folderNo].HeadingPercentSize;
				Folders[0].HeadingOption = Folders[folderNo].HeadingOption;
				Folders[0].LineSpacing[0] = Folders[folderNo].LineSpacing[0];
				Folders[0].LineSpacing[1] = Folders[folderNo].LineSpacing[1];
			}
			int headerValue = ExtractNumericData(InItem.Format.HeaderData[21]);
			InItem.Format.ShowSongHeadings = ((headerValue < 0 || headerValue > 3) ? ShowSongHeadings : headerValue);
			headerValue = ExtractNumericData(InItem.Format.HeaderData[22]);
			InItem.Format.UseShadowFont = ((headerValue < 0) ? UseShadowFont : DataUtil.GetBitValue(headerValue, 2));
			InItem.Format.ShowNotations = ((headerValue < 0) ? ShowNotations : DataUtil.GetBitValue(headerValue, 3));
			InItem.Format.ShowInterlace = ((headerValue < 0) ? ShowInterlace : DataUtil.GetBitValue(headerValue, 5));
			InItem.Format.UseOutlineFont = ((headerValue < 0) ? UseOutlineFont : DataUtil.GetBitValue(headerValue, 6));
			InItem.Format.HideDisplayPanel = ((headerValue >= 0) ? DataUtil.GetBitValue(headerValue, 7) : 0);
			headerValue = ExtractNumericData(InItem.Format.HeaderData[23]);
			InItem.Format.ShowSongHeadingsAlign = ((headerValue < 0 || headerValue > 4) ? ShowSongHeadingsAlign : headerValue);
			headerValue = ExtractNumericData(InItem.Format.HeaderData[25]);
			InItem.Format.ShowLyrics = ((headerValue < 0 || headerValue > 2) ? ShowLyrics : headerValue);
			headerValue = ExtractNumericData(InItem.Format.HeaderData[26]);
			// SongFormat stores colours as ARGB ints: convert the Color result with .ToArgb() (FromArgb -> ToArgb round-trips the same value).
			InItem.Format.ShowScreenColour[0] = ((InItem.Format.HeaderData[26] == "" || !ValidateColour(headerValue)) ? ShowScreenColour[0] : Color.FromArgb(Convert.ToInt32(headerValue))).ToArgb();
			headerValue = ExtractNumericData(InItem.Format.HeaderData[27]);
			InItem.Format.ShowScreenColour[1] = ((InItem.Format.HeaderData[27] == "" || !ValidateColour(headerValue)) ? ShowScreenColour[0] : Color.FromArgb(Convert.ToInt32(headerValue))).ToArgb();
			headerValue = ExtractNumericData(InItem.Format.HeaderData[28]);
			InItem.Format.ShowScreenStyle = ((headerValue < 0 || headerValue > MaxBackgroundStyleIndex) ? ShowScreenStyle : headerValue);
			headerValue = ExtractNumericData(InItem.Format.HeaderData[29]);
			InItem.Format.ShowFontColour[0] = ((InItem.Format.HeaderData[29] == "" || !ValidateColour(headerValue)) ? ShowFontColour[0] : Color.FromArgb(Convert.ToInt32(headerValue))).ToArgb();
			headerValue = ExtractNumericData(InItem.Format.HeaderData[30]);
			InItem.Format.ShowFontColour[1] = ((InItem.Format.HeaderData[30] == "" || !ValidateColour(headerValue)) ? ShowFontColour[1] : Color.FromArgb(Convert.ToInt32(headerValue))).ToArgb();
			headerValue = ExtractNumericData(InItem.Format.HeaderData[31]);
			InItem.Format.ShowFontAlign[0] = ((headerValue < 1 || headerValue > 3) ? Folders[0].FontAlign[0] : headerValue);
			headerValue = ExtractNumericData(InItem.Format.HeaderData[32]);
			InItem.Format.ShowFontAlign[1] = ((headerValue < 1 || headerValue > 3) ? Folders[0].FontAlign[1] : headerValue);
			headerValue = ExtractNumericData(InItem.Format.HeaderData[41]);
			InItem.Format.ShowFontBold[0] = ((headerValue < 0 || headerValue > 127) ? Folders[0].FontBold[0] : DataUtil.GetBitValue(headerValue, 1));
			InItem.Format.ShowFontItalic[0] = ((headerValue < 0 || headerValue > 127) ? Folders[0].FontItalic[0] : DataUtil.GetBitValue(headerValue, 2));
			InItem.Format.ShowFontUnderline[0] = ((headerValue < 0 || headerValue > 127) ? Folders[0].FontUnderline[0] : DataUtil.GetBitValue(headerValue, 3));
			InItem.Format.ShowFontBold[2] = ((headerValue < 0 || headerValue > 127) ? Folders[0].FontBold[1] : DataUtil.GetBitValue(headerValue, 4));
			InItem.Format.ShowFontItalic[2] = ((headerValue < 0 || headerValue > 127) ? Folders[0].FontItalic[2] : DataUtil.GetBitValue(headerValue, 5));
			InItem.Format.ShowFontUnderline[2] = ((headerValue < 0 || headerValue > 127) ? Folders[0].FontUnderline[1] : DataUtil.GetBitValue(headerValue, 6));
			headerValue = ExtractNumericData(InItem.Format.HeaderData[42]);
			InItem.Format.ShowFontBold[1] = ((headerValue < 0 || headerValue > 127) ? Folders[0].FontBold[1] : DataUtil.GetBitValue(headerValue, 1));
			InItem.Format.ShowFontItalic[1] = ((headerValue < 0 || headerValue > 127) ? Folders[0].FontItalic[1] : DataUtil.GetBitValue(headerValue, 2));
			InItem.Format.ShowFontUnderline[1] = ((headerValue < 0 || headerValue > 127) ? Folders[0].FontUnderline[1] : DataUtil.GetBitValue(headerValue, 3));
			InItem.Format.ShowFontBold[3] = ((headerValue < 0 || headerValue > 127) ? Folders[0].FontBold[3] : DataUtil.GetBitValue(headerValue, 4));
			InItem.Format.ShowFontItalic[3] = ((headerValue < 0 || headerValue > 127) ? Folders[0].FontItalic[3] : DataUtil.GetBitValue(headerValue, 5));
			InItem.Format.ShowFontUnderline[3] = ((headerValue < 0 || headerValue > 127) ? Folders[0].FontUnderline[3] : DataUtil.GetBitValue(headerValue, 6));
			InItem.Format.ShowFontName[0] = ((InItem.Format.HeaderData[43] == "") ? Folders[0].FontName[0] : InItem.Format.HeaderData[43]);
			InItem.Format.ShowFontName[1] = ((InItem.Format.HeaderData[44] == "") ? Folders[0].FontName[1] : InItem.Format.HeaderData[44]);
			headerValue = ExtractNumericData(InItem.Format.HeaderData[45]);
			InItem.Format.ShowFontVPosition[0] = ((headerValue < 0 || headerValue > 100) ? Folders[0].FontVPosition[0] : headerValue);
			headerValue = ExtractNumericData(InItem.Format.HeaderData[46]);
			InItem.Format.ShowFontVPosition[1] = ((headerValue < InItem.Format.ShowFontVPosition[0] || headerValue > 100) ? Folders[0].FontVPosition[1] : headerValue);
			headerValue = ExtractNumericData(InItem.Format.HeaderData[47]);
			InItem.Format.ShowFontSize[0] = ((headerValue < 6 || headerValue > 100) ? Folders[0].FontSize[0] : headerValue);
			headerValue = ExtractNumericData(InItem.Format.HeaderData[48]);
			InItem.Format.ShowFontSize[1] = ((headerValue < 6 || headerValue > 100) ? Folders[0].FontSize[1] : headerValue);
			headerValue = ExtractNumericData(InItem.Format.HeaderData[71]);
			InItem.Format.TransposeOffset = ((!(headerValue < 0 || headerValue > 11)) ? headerValue : 0);
			InItem.Format.PreviousTransposeOffset = InItem.Format.TransposeOffset;
			InItem.Format.BackgroundPicture = InItem.Format.HeaderData[61];
			headerValue = ExtractNumericData(InItem.Format.HeaderData[50]);
			InItem.Format.MediaOption = ((headerValue < 0 || headerValue > 3) ? MediaOption : headerValue);
			InItem.Format.MediaLocation = InItem.Format.HeaderData[51];
			headerValue = ExtractNumericData(InItem.Format.HeaderData[52]);
			InItem.Format.MediaVolume = ((headerValue < 0 || headerValue > 100) ? MediaVolume : headerValue);
			headerValue = ExtractNumericData(InItem.Format.HeaderData[53]);
			InItem.Format.MediaBalance = ((headerValue < -100 || headerValue > 100) ? MediaBalance : headerValue);
			headerValue = ExtractNumericData(InItem.Format.HeaderData[54]);
			InItem.Format.MediaMute = ((headerValue < 0) ? MediaMute : DataUtil.GetBitValue(headerValue, 1));
			InItem.Format.MediaRepeat = ((headerValue < 0) ? MediaRepeat : DataUtil.GetBitValue(headerValue, 2));
			InItem.Format.MediaWidescreen = ((headerValue < 0) ? MediaWidescreen : DataUtil.GetBitValue(headerValue, 3));
			if (InItem.Type == "M")
			{
				InItem.Format.ShowSongHeadings = 0;
				InItem.Format.MediaOption = 2;
				InItem.Format.MediaLocation = InItem.ItemID;
			}
			headerValue = ExtractNumericData(InItem.Format.HeaderData[55]);
			InItem.Format.MediaCaptureDeviceNumber = ((headerValue < 1 || headerValue > 5) ? 1 : headerValue);
			InItem.Format.MediaOutputMonitorName = InItem.Format.HeaderData[56];
			headerValue = ExtractNumericData(InItem.Format.HeaderData[62]);
			InItem.Format.BackgroundMode = (ImageMode)((headerValue < 0 || headerValue > 2) ? ((int)BackgroundMode) : headerValue);
			headerValue = ExtractNumericData(InItem.Format.HeaderData[63]);
			InItem.Format.ShowVerticalAlign = ((headerValue < 0 || headerValue > 2) ? ShowVerticalAlign : headerValue);
			headerValue = ExtractNumericData(InItem.Format.HeaderData[64]);
			InItem.Format.ShowLeftMargin = ((headerValue < 0 || headerValue > 40) ? Folders[0].LeftMargin : headerValue);
			headerValue = ExtractNumericData(InItem.Format.HeaderData[65]);
			InItem.Format.ShowRightMargin = ((headerValue < 0 || headerValue > 40) ? Folders[0].RightMargin : headerValue);
			headerValue = ExtractNumericData(InItem.Format.HeaderData[66]);
			InItem.Format.ShowBottomMargin = ((headerValue < 0 || headerValue > 100) ? Folders[0].BottomMargin : headerValue);
			if (InItem.Format.HeaderData[72] != "")
			{
				InItem.Format.ShowItemTransition = GlobalImageCanvas.GetTransitionType(InItem.Format.HeaderData[72]);
			}
			else
			{
				InItem.Format.ShowItemTransition = ShowItemTransition;
			}
			if (InItem.Format.HeaderData[73] != "")
			{
				InItem.Format.ShowSlideTransition = GlobalImageCanvas.GetTransitionType(InItem.Format.HeaderData[73]);
			}
			else
			{
				InItem.Format.ShowSlideTransition = ShowSlideTransition;
			}
			InItem.Format.FormatString = InFormatString;
			InItem.UseDefaultFormat = ((InFormatString == "") ? true : false);
			if (InItem.Format.ImageString != "" && InItem.Format.BackgroundPicture != "")
			{
				InItem.Format.TempImageFileName = dumpImageToFile(Convert.FromBase64String(InItem.Format.ImageString), InItem.Format.BackgroundPicture);
			}
		}
	}
}
