//using JRO;
using Easislides.SQLite;
//using Easislides.Model.EasiSlidesDbDataSetTableAdapters;
using Easislides.Util;
//using Microsoft.Office.Interop.Access.Dao;
using Microsoft.Win32;
//using NetOffice.PowerPointApi;
using OfficeLib;
using System;
using System.Collections;
using System.Data;
using System.Data.OleDb;
using System.Data.SQLite;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using Easislides.Module;
using System.Threading;

//using NetOffice.DAOApi;

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
    internal unsafe partial class gf
    {

		public void FormatText(ref SongSettings InItem, Color PanelBackColour, int PanelBackColorAsScreen, Color PanelTextColor, int PaneltextColourAsRegion1)
		{
			FormatText(ref InItem, PanelBackColour, PanelBackColorAsScreen, PanelTextColor, PaneltextColourAsRegion1, UseDefault: true);
		}

		public static void FormatText(ref SongSettings InItem, Color PanelBackColour, int PanelBackColorAsScreen, Color PanelTextColor, int PaneltextColourAsRegion1, bool UseDefault)
		{
			Color[] array = new Color[2];
			int[] array2 = new int[2];
			int[] array3 = new int[4];
			int[] array4 = new int[4];
			int[] array5 = new int[4];
			int[] array6 = new int[2];
			string[] array7 = new string[2];
			int[] array8 = new int[2];
			int[] array9 = new int[2];
			int buffer_LS_Width = Buffer_LS_Width;
			int buffer_LS_Height = Buffer_LS_Height;
			double num = PreviewSampleFactor;
			int num2 = 0;
			int num3 = 0;
			int num4 = 0;
			int num5 = 0;
			if (UseDefault)
			{
				array[0] = ShowFontColour[0];
				array[1] = ShowFontColour[1];
				array2[0] = ShowFontAlign[0, 0];
				array2[1] = ShowFontAlign[0, 1];
				array3[0] = ShowFontBold[InItem.FolderNo, 0];
				array3[1] = ShowFontBold[InItem.FolderNo, 1];
				array4[0] = ShowFontItalic[InItem.FolderNo, 0];
				array4[1] = ShowFontItalic[InItem.FolderNo, 1];
				array4[2] = ShowFontItalic[InItem.FolderNo, 2];
				array4[3] = ShowFontItalic[InItem.FolderNo, 3];
				array5[0] = ShowFontUnderline[InItem.FolderNo, 0];
				array5[1] = ShowFontUnderline[InItem.FolderNo, 1];
				array6[0] = ShowFontRTL[InItem.FolderNo, 0];
				array6[1] = ShowFontRTL[InItem.FolderNo, 1];
				array7[0] = ShowFontName[InItem.FolderNo, 0];
				array7[1] = ShowFontName[InItem.FolderNo, 1];
				if (InItem.Type == "B")
				{
					array8[0] = InItem.Format.ShowFontSize[0];
					array8[1] = InItem.Format.ShowFontSize[1];
				}
				else
				{
					array8[0] = ShowFontSize[InItem.FolderNo, 0];
					array8[1] = ShowFontSize[InItem.FolderNo, 1];
				}
				array9[0] = ShowFontVPosition[InItem.FolderNo, 0];
				array9[1] = ShowFontVPosition[InItem.FolderNo, 1];
				num2 = ShowLeftMargin[InItem.FolderNo];
				num3 = ShowRightMargin[InItem.FolderNo];
				num4 = ShowBottomMargin[InItem.FolderNo];
				num5 = ((ShowSongHeadingsAlign == 1) ? array2[1] : ((ShowSongHeadingsAlign == 2) ? 1 : ((ShowSongHeadingsAlign == 3) ? 2 : ((ShowSongHeadingsAlign != 4) ? array2[0] : 3))));
			}
			else if (!UseDefault)
			{
				array[0] = InItem.Format.ShowFontColour[0];
				array[1] = InItem.Format.ShowFontColour[1];
				array2[0] = InItem.Format.ShowFontAlign[0];
				array2[1] = InItem.Format.ShowFontAlign[1];
				array3[0] = InItem.Format.ShowFontBold[0];
				array3[1] = InItem.Format.ShowFontBold[1];
				array4[0] = InItem.Format.ShowFontItalic[0];
				array4[1] = InItem.Format.ShowFontItalic[1];
				array4[2] = InItem.Format.ShowFontItalic[2];
				array4[3] = InItem.Format.ShowFontItalic[3];
				array5[0] = InItem.Format.ShowFontUnderline[0];
				array5[1] = InItem.Format.ShowFontUnderline[1];
				array7[0] = InItem.Format.ShowFontName[0];
				array7[1] = InItem.Format.ShowFontName[1];
				array8[0] = InItem.Format.ShowFontSize[0];
				array8[1] = InItem.Format.ShowFontSize[1];
				array9[0] = InItem.Format.ShowFontVPosition[0];
				array9[1] = InItem.Format.ShowFontVPosition[1];
				num2 = InItem.Format.ShowLeftMargin;
				num3 = InItem.Format.ShowRightMargin;
				num4 = InItem.Format.ShowBottomMargin;
				num5 = ((InItem.Format.ShowSongHeadingsAlign == 1) ? array2[1] : ((InItem.Format.ShowSongHeadingsAlign == 2) ? 1 : ((InItem.Format.ShowSongHeadingsAlign == 3) ? 2 : ((InItem.Format.ShowSongHeadingsAlign != 4) ? array2[0] : 3))));
			}
			int num6 = FolderHeadingOption[InItem.FolderNo];
			bool flag = (FolderHeadingFontBold[InItem.FolderNo, 0] > 0) ? true : false;
			bool flag2 = (FolderHeadingFontItalic[InItem.FolderNo, 0] > 0) ? true : false;
			bool flag3 = (FolderHeadingFontUnderline[InItem.FolderNo, 0] > 0) ? true : false;
			bool flag4 = (FolderHeadingFontItalic[InItem.FolderNo, 1] > 0) ? true : false;
			int num7 = num2 * buffer_LS_Width / 100;
			int num8 = buffer_LS_Width - (num7 + num3 * buffer_LS_Width / 100);
			int num9 = buffer_LS_Height - (int)(TopBorderFactor * (double)buffer_LS_Height + (BottomBorderFactor + 0.029999999329447746) * (double)buffer_LS_Height + (double)(num4 * buffer_LS_Height / 100));
			ShowTopBorderSize = (int)((double)buffer_LS_Height * TopBorderFactor);
			
			Image image = new Bitmap(num8, 1, PixelFormat.Format24bppRgb);
			Graphics graphics = Graphics.FromImage(image);
			SizeF layoutArea = new SizeF(num8, 32000f);
			int i;
			FontStyle fontStyle;
			int num11;
			double num12;
			for (i = 0; i <= 2; i++)
			{
				int num10 = (i < 2) ? i : 0;
				fontStyle = FontStyle.Regular;
				FontStyle fontStyle2 = FontStyle.Regular;
				if (i == 2 && num6 != 0)
				{
					if (num6 == 2)
					{
						if (flag)
						{
							fontStyle |= FontStyle.Bold;
							fontStyle2 |= FontStyle.Bold;
						}
						if (flag2)
						{
							fontStyle |= FontStyle.Italic;
						}
						if (flag4)
						{
							fontStyle2 |= FontStyle.Italic;
						}
						if (flag3)
						{
							fontStyle |= FontStyle.Underline;
							fontStyle2 |= FontStyle.Underline;
						}
					}
					else
					{
						if ((array3[num10] > 0) | flag)
						{
							fontStyle |= FontStyle.Bold;
							fontStyle2 |= FontStyle.Bold;
						}
						if ((array4[num10] > 0) | flag2)
						{
							fontStyle |= FontStyle.Italic;
						}
						if ((array4[num10] > 0) | flag4)
						{
							fontStyle2 |= FontStyle.Italic;
						}
						if ((array5[num10] > 0) | flag3)
						{
							fontStyle |= FontStyle.Underline;
							fontStyle2 |= FontStyle.Underline;
						}
					}
				}
				else
				{
					if (array3[num10] > 0)
					{
						fontStyle |= FontStyle.Bold;
						fontStyle2 |= FontStyle.Bold;
					}
					if (array4[num10] > 0)
					{
						fontStyle |= FontStyle.Italic;
					}
					if (array5[num10] > 0)
					{
						fontStyle |= FontStyle.Underline;
						fontStyle2 |= FontStyle.Bold;
					}
					if (array4[num10] > 0 || array4[num10 + 2] > 0)
					{
						fontStyle2 |= FontStyle.Italic;
					}
				}
				InItem.Lyrics[i].ForeColour = array[num10];
				switch ((i == 2) ? num5 : array2[num10])
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
				num11 = DisplayFontSize(array8[num10], buffer_LS_Width, i, InItem.FolderNo);
				num12 = (double)num11 / num;
				try
				{
					InItem.Lyrics[i].FS_Font = new Font(array7[num10], num11, fontStyle);
					InItem.Lyrics[i].FS_ChorusFont = new Font(array7[num10], num11, fontStyle2);
				}
				catch
				{
					InItem.Lyrics[i].FS_Font = new Font("Microsoft Sans Serif", num11, fontStyle);
					InItem.Lyrics[i].FS_ChorusFont = new Font("Microsoft Sans Serif", num11, fontStyle2);
				}
				try
				{
					InItem.Lyrics[i].Font = new Font(array7[num10], (float)((num12 > 0.0) ? num12 : 1.0), fontStyle);
					InItem.Lyrics[i].ChorusFont = new Font(array7[num10], (float)((num12 > 0.0) ? num12 : 1.0), fontStyle2);
				}
				catch
				{
					InItem.Lyrics[i].Font = new Font("Microsoft Sans Serif", (float)((num12 > 0.0) ? num12 : 1.0), fontStyle);
					InItem.Lyrics[i].ChorusFont = new Font("Microsoft Sans Serif", (float)((num12 > 0.0) ? num12 : 1.0), fontStyle2);
				}
			}
			double num13 = (double)graphics.MeasureString("A", InItem.Lyrics[2].FS_Font, layoutArea).Height * 1.1;
			if (num13 > (double)num9 / 4.0)
			{
				num13 = (double)num9 / 4.0;
			}
			for (i = 0; i <= 2; i++)
			{
				InItem.Lyrics[i].Visible = false;
				InItem.Lyrics[i].FS_Width = num8;
				InItem.Lyrics[i].FS_Left = num7;
				InItem.Lyrics[i].FS_Top = SetLyricsTopPos(array9[(i < 2) ? i : 0], buffer_LS_Height) + ((i == 0) ? ((int)num13) : 0);
				if (i == 2)
				{
					InItem.Lyrics[i].FS_Height = (int)num13;
					InItem.Lyrics[i].FS_Height_R2Bound = InItem.Lyrics[i].FS_Height;
					InItem.Lyrics[0].FS_Height_R2Bound = InItem.Lyrics[1].FS_Top - InItem.Lyrics[0].FS_Top;
					InItem.Lyrics[0].Height_R2Bound = Convert.ToInt32((double)InItem.Lyrics[0].FS_Height_R2Bound / num);
				}
				else
				{
					InItem.Lyrics[i].FS_Height = num9 - InItem.Lyrics[i].FS_Top;
					InItem.Lyrics[i].FS_Height_R2Bound = InItem.Lyrics[i].FS_Height;
				}
				InItem.Lyrics[i].Width = Convert.ToInt32((double)InItem.Lyrics[i].FS_Width / num);
				InItem.Lyrics[i].Left = Convert.ToInt32((double)InItem.Lyrics[i].FS_Left / num);
				InItem.Lyrics[i].Top = Convert.ToInt32((double)InItem.Lyrics[i].FS_Top / num);
				InItem.Lyrics[i].Height = Convert.ToInt32((double)InItem.Lyrics[i].FS_Height / num);
				InItem.Lyrics[i].Height_R2Bound = Convert.ToInt32((double)InItem.Lyrics[i].FS_Height_R2Bound / num);
			}
			i = 3;
			InItem.Lyrics[i].BackColour = PanelBackColour;
			InItem.Lyrics[i].Transparent = ((PanelBackColorAsScreen > 0) ? true : false);
			InItem.Lyrics[i].ForeColour = ((PaneltextColourAsRegion1 > 0) ? InItem.Lyrics[0].ForeColour : PanelTextColour);
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
			double num14 = BottomBorderFactor * (double)buffer_LS_Height;
			InItem.Lyrics[i].FS_Height = (int)(num14 * 0.8);
			InItem.Lyrics[i].FS_Top = buffer_LS_Height - InItem.Lyrics[i].FS_Height;
			num11 = 40;
			Font font;
			try
			{
				font = new Font(ShowDataDisplayFontName, num11, fontStyle);
			}
			catch
			{
				font = new Font("Microsoft Sans Serif", num11, fontStyle);
			}
			while ((graphics.MeasureString("A", font, layoutArea).Height > (float)(InItem.Lyrics[i].FS_Height * 9 / 20)) & (font.Size > 1f))
			{
				num11--;
				font = new Font(font.Name, num11, fontStyle);
			}
			num11 = (int)font.Size + 1;
			num12 = (double)font.Size / num;
			InItem.Lyrics[i].FS_Height_R2Bound = InItem.Lyrics[i].FS_Height;
			InItem.Lyrics[i].Width = (int)((double)InItem.Lyrics[i].FS_Width / num);
			InItem.Lyrics[i].Left = (int)((double)InItem.Lyrics[i].FS_Left / num);
			InItem.Lyrics[i].Top = (int)((double)InItem.Lyrics[i].FS_Top / num);
			InItem.Lyrics[i].Height = (int)((double)InItem.Lyrics[i].FS_Height / num);
			InItem.Lyrics[i].Height_R2Bound = (int)((double)InItem.Lyrics[i].FS_Height_R2Bound / num);
			InItem.Lyrics[i].FS_Font = new Font(font.Name, num11, fontStyle);
			InItem.Lyrics[i].Font = new Font(font.Name, (!(num12 > 0.0)) ? 1 : ((int)num12), fontStyle);
			//graphics.Dispose();
			//image.Dispose();
		}

		public static string RTFFormatNotationString(string InText, string InNotationString, Font MainFont, Font NotationFont)
		{
			string text = "";
			string text2 = "";
			int num = 0;
			int num2 = 0;
			string text3 = "";
			tbWorkspace.Font = MainFont;
			tbWorkspace.WordWrap = false;
			tbWorkspace.Text = InText;
			tbTempSpace.Font = NotationFont;
			tbTempSpace.WordWrap = false;
			tbTempSpace.Text = "";
			while (InNotationString != "")
			{
				text = InNotationString;
				text2 = DataUtil.ExtractOneInfo(ref text, ';');
				num = Convert.ToInt32(DataUtil.ExtractOneInfo(ref text, ';'));
				InNotationString = text;
				num2 = GetAssociatedLyricsLineCurPosX(ref tbWorkspace, num, 0);
				while (GetAssociatedLyricsLineCurPosX(ref tbTempSpace, tbTempSpace.Text.Length - 1) < num2 - 1)
				{
					text3 += " ";
					tbTempSpace.Text = text3;
					MarkSelectedRTB(ref tbTempSpace, 0, tbTempSpace.Text.Length, 2, MainFont, NotationFont);
				}
				text3 += (((text3.Length > 1) & (DataUtil.Right(text3, 1) != " ")) ? (" " + text2) : text2);
				tbTempSpace.Text = text3;
				MarkSelectedRTB(ref tbTempSpace, 0, tbTempSpace.Text.Length, 2, MainFont, NotationFont);
			}
			return text3;
		}

		public static string CombineLyricsAndNotations(string InText, string InNotations, Font MainFont, Font NotationFont, ref RichTextBox InWorkspace, ref RichTextBox InTempSpace)
		{
			if ((InNotations == "") | (InText == ""))
			{
				return InText;
			}
			StringBuilder stringBuilder = new StringBuilder();
			InWorkspace.Text = InText;
			MarkSelectedRTB(ref InWorkspace, 0, InWorkspace.Text.Length, 0, MainFont, NotationFont);
			int num = DataUtil.CountLf(InWorkspace.Text);
			int InMin = 0;
			int InMax = 0;
			string text = "";
			int num2 = ListNotationData(InNotations, ref NotationsArray, num);
			for (int i = 0; i < num; i++)
			{
				if (num2 > 0 && NotationsArray[i] != "")
				{
					GetMinMaxfromTextBox(InWorkspace, i, ref InMin, ref InMax);
					InTempSpace.Text = "";
					string text2 = "";
					while (NotationsArray[i].Length > 0)
					{
						text = NotationsArray[i];
						string text3 = DataUtil.ExtractOneInfo(ref text, ';');
						int inCurPos = Convert.ToInt32(DataUtil.ExtractOneInfo(ref text, ';'));
						NotationsArray[i] = text;
						int associatedLyricsLineCurPosX = GetAssociatedLyricsLineCurPosX(ref InWorkspace, inCurPos, InMin, InMax);
						while (GetAssociatedLyricsLineCurPosX(ref InTempSpace, InTempSpace.Text.Length - 1) < associatedLyricsLineCurPosX - 4)
						{
							text2 += " ";
							InTempSpace.Text = text2;
							MarkSelectedRTB(ref InTempSpace, 0, InTempSpace.Text.Length, 2, MainFont, NotationFont);
						}
						text2 += (((text2.Length > 1) & (DataUtil.Right(text2, 1) != " ")) ? (" " + text3) : text3);
						InTempSpace.Text = text2;
						MarkSelectedRTB(ref InTempSpace, 0, InTempSpace.Text.Length, 2, MainFont, NotationFont);
					}
					stringBuilder.Append(InTempSpace.Text + " »\n");
				}
				stringBuilder.Append(InWorkspace.Lines[i] + "\n");
			}
			if (DataUtil.Right(stringBuilder.ToString(), 1) == "\n")
			{
				return DataUtil.Left(stringBuilder.ToString(), stringBuilder.Length - 1);
			}
			return stringBuilder.ToString();
		}

		public static void FormatDisplayLyrics(ref SongSettings InItem, bool PrepareSlides, bool UseStoredSequence)
		{
			int num = InItem.UseDefaultFormat ? ShowNotations : InItem.Format.ShowNotations;
			int num2 = (!InItem.UseDefaultFormat) ? InItem.Format.TransposeOffset : ((ShowCapoZero == 1) ? IncrementChord(ref InItem.Capo, 0) : 0);
			int num3 = -1;
			ListViewItem listViewItem = new ListViewItem();
			int num4 = 0;
			InItem.CompleteLyrics = InItem.CompleteLyrics.Replace("\r\n", "\n");
			num4 = DataUtil.CountLf(InItem.CompleteLyrics);
			if (InItem != null && InItem.Capo < 0)
			{
				InItem.Capo = -1;
			}
			if (ShowRunning_ShowNotations == 1)
			{
				num = ((num <= 0) ? 1 : 0);
			}
			if (num == 1)
			{
				if (num2 > 0)
				{
					InItem.Notations = TransposeNotations(InItem.OriginalNotations, InItem.Format.PreviousTransposeOffset, num2, InItem.MusicKey);
				}
				else
				{
					InItem.Notations = InItem.OriginalNotations;
				}
			}
			for (int i = 0; i < 160; i++)
			{
				InItem.VersePresent[i] = false;
				InItem.VerseLineLoc[i, 0] = -1;
				InItem.VerseLineLoc[i, 1] = -1;
				InItem.VerseLineLoc[i, 2] = -1;
				InItem.VerseLineLoc[i, 3] = -1;
				InItem.VerseLineLoc[i, 4] = -1;
			}
			ListView listView = ExtractLyrics(InItem.CompleteLyrics, InItem.Notations);
			InItem.LyricsAndNotationsList.Items.Clear();
			int num5 = -1;
			num3 = -1;
			if (InItem.RotateStyle == 2)
			{
				InItem.SongSequence = InItem.RotateSequence;
			}
			InItem.SongBasicSequence = "";
			for (int i = 0; i < listView.Items.Count; i++)
			{
				num3 = DataUtil.StringToInt(listView.Items[i].Text);
				if (!InItem.VersePresent[num3])
				{
					InItem.VersePresent[num3] = true;
					if (num5 != num3)
					{
						InItem.SongBasicSequence += (char)num3;
						num5 = num3;
					}
				}
			}
			for (int i = 0; i < 160; i++)
			{
				if (!InItem.VersePresent[i])
				{
					continue;
				}
				for (int j = 0; j < listView.Items.Count; j++)
				{
					if (listView.Items[j].SubItems[0].Text == i.ToString() && listView.Items[j].SubItems[1].Text == "1")
					{
						string text = i.ToString();
						listViewItem = InItem.LyricsAndNotationsList.Items.Add(text);
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
						string text = i.ToString();
						listViewItem = InItem.LyricsAndNotationsList.Items.Add(text);
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
			int num6 = -1;
			num5 = -1;
			for (num3 = 0; num3 < 160; num3++)
			{
				if (!InItem.VersePresent[num3])
				{
					continue;
				}
				for (int i = 0; i < InItem.LyricsAndNotationsList.Items.Count; i++)
				{
					num5 = DataUtil.StringToInt(InItem.LyricsAndNotationsList.Items[i].SubItems[0].Text);
					num6 = DataUtil.StringToInt(InItem.LyricsAndNotationsList.Items[i].SubItems[1].Text);
					if (num3 == num5)
					{
						if (InItem.VerseLineLoc[num3, num6 * 2 - 1] < 0)
						{
							InItem.VerseLineLoc[num3, num6 * 2 - 1] = i;
						}
						InItem.VerseLineLoc[num3, num6 * 2] = i;
					}
				}
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
				BuildSlides(InItem, InItem.LyricsAndNotationsList, ref InItem.SongSequence, ref InItem.TotalSlides, ref InItem.SongVerses, ref InItem.ChorusSlides, ref InItem.Slide, num);
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
			string ResultString = "";
			string text = "";
			int num = ExtractOneNotationsLine(ref InNotationsData, ref ResultString);
			if (PreviousTransposeTo > TransposeTo)
			{
				TransposeTo -= 12;
			}
			int flatSharpKey = TransposeKey(ref StoredMusicKey, TransposeTo);
			while (num >= 0)
			{
				object obj = text;
				text = string.Concat(obj, "(", Convert.ToString(num), ';');
				text = text + TransposeOneNotationString(ResultString, TransposeTo, flatSharpKey) + ")";
				num = Convert.ToInt32(ExtractOneNotationsLine(ref InNotationsData, ref ResultString));
			}
			return text;
		}

		public static int ListNotationData(string InString, ref string[] NotationsArray, int MaxLineCount)
		{
			string ResultString = "";
			int num = 0;
			for (int i = 0; i <= MaxLineCount; i++)
			{
				NotationsArray[i] = "";
			}
			int num2 = ExtractOneNotationsLine(ref InString, ref ResultString);
			while (num2 >= 0)
			{
				NotationsArray[num2] = ResultString;
				num2 = ExtractOneNotationsLine(ref InString, ref ResultString);
				num++;
			}
			return num;
		}

    }
}
