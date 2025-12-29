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

		public static int DisplayFontSize(int InFontSize, int InLyricsWidth, int InNum, int Folderno)
		{
			if (InNum == 2)
			{
				try
				{
					InFontSize = InFontSize * FolderHeadingPercentSize[Folderno] / 100;
				}
				catch
				{
				}
			}
			int num = InFontSize * InLyricsWidth / 960;
			return (num < 1) ? 1 : num;
		}

		public static int ReduceFontToFit(Graphics g, string InText, ref Font MainFont, int InWidth, int InHeight)
		{
			return ReduceFontToFit(g, InText, ref MainFont, InWidth, InHeight, MultiLine: false);
		}

		public static int ReduceFontToFit(Graphics g, string InText, ref Font MainFont, int InWidth, int InHeight, bool MultiLine)
		{
			SizeF layoutArea = new SizeF(InWidth, 32000f);
			if (MultiLine)
			{
				while ((g.MeasureString(InText, MainFont, layoutArea).Height > (float)InHeight) & (MainFont.Size > 1f))
				{
					MainFont = new Font(MainFont.Name, MainFont.Size - 1f, MainFont.Style);
				}
			}
			else
			{
				while (MainFont.Size > 1f && (g.MeasureString(InText, MainFont).Width > (float)InWidth || g.MeasureString(InText, MainFont).Height > (float)InHeight))
				{
					MainFont = new Font(MainFont.Name, MainFont.Size - 1f, MainFont.Style);
				}
			}
			return (int)g.MeasureString(InText, MainFont).Height;
		}

		public static int IncreaseFontToLargest(Graphics g, string InText, ref Font MainFont, int InWidth, int InHeight)
		{
			bool OnlyOneDisplayLine = false;
			return IncreaseFontToLargest(g, InText, ref MainFont, InWidth, InHeight, ref OnlyOneDisplayLine);
		}

		public static int IncreaseFontToLargest(Graphics g, string InText, ref Font MainFont, int InWidth, int InHeight, ref bool OnlyOneDisplayLine)
		{
			ReduceFontToFit(g, InText, ref MainFont, InWidth, InHeight);
			SizeF layoutArea = new SizeF(InWidth, 32000f);
			while ((g.MeasureString(InText, MainFont, layoutArea).Height < (float)InHeight) & (MainFont.Size <= 100f))
			{
				MainFont = new Font(MainFont.Name, MainFont.Size + 1f, MainFont.Style);
			}
			MainFont = new Font(MainFont.Name, MainFont.Size - 4f, MainFont.Style);
			int result = (int)g.MeasureString(InText, MainFont, layoutArea).Height;
			if (g.MeasureString(InText, MainFont).Width < (float)InWidth)
			{
				OnlyOneDisplayLine = true;
			}
			return result;
		}

		public static bool ShowDBSlide(ref SongSettings InItem, ref ImageTransitionControl PInPictureBox, ref ImageTransitionControl OInPictureBox, bool DoActiveIndicator, ImageTransitionControl.TransitionAction TransitionAction)
		{
			if (InItem.OutputStyleScreen)
			{
				return ShowDBSlide(ref InItem, ref OInPictureBox, DoActiveIndicator, TransitionAction, RedoBackground: false);
			}
			return ShowDBSlide(ref InItem, ref PInPictureBox, DoActiveIndicator, TransitionAction, RedoBackground: false);
		}

		public static bool ShowDBSlide(ref SongSettings InItem, ref ImageTransitionControl InPictureBox, bool DoActiveIndicator, ImageTransitionControl.TransitionAction TransitionAction, bool RedoBackground)
		{
			int num = InItem.UseDefaultFormat ? ShowSongHeadingsAlign : InItem.Format.ShowSongHeadingsAlign;
			int num2 = InItem.UseDefaultFormat ? ShowNotations : InItem.Format.ShowNotations;
			int inShowInterlace = InItem.UseDefaultFormat ? ShowInterlace : InItem.Format.ShowInterlace;
			int num3 = InItem.UseDefaultFormat ? ShowVerticalAlign : InItem.Format.ShowVerticalAlign;
			int transitionType = InItem.UseDefaultFormat ? ShowItemTransition : InItem.Format.ShowItemTransition;
			int transitionType2 = InItem.UseDefaultFormat ? ShowSlideTransition : InItem.Format.ShowSlideTransition;
			InItem.Format.ShowLyrics = (InItem.UseDefaultFormat ? ShowLyrics : InItem.Format.ShowLyrics);
			InItem.Format.ShowSongHeadings = (InItem.UseDefaultFormat ? ShowSongHeadings : InItem.Format.ShowSongHeadings);
			InItem.Format.ShowSongHeadingsAlign = (InItem.UseDefaultFormat ? ShowSongHeadingsAlign : InItem.Format.ShowSongHeadingsAlign);
			int inUseShadowFont = InItem.UseDefaultFormat ? UseShadowFont : InItem.Format.UseShadowFont;
			int inUseOutlineFont = InItem.UseDefaultFormat ? UseOutlineFont : InItem.Format.UseOutlineFont;
			int inHideDisplayPanel = (!InItem.UseDefaultFormat) ? InItem.Format.HideDisplayPanel : ((ShowDataDisplayMode <= 0) ? 1 : 0);
			InItem.CurSlide = ((InItem.CurSlide <= 0) ? 1 : ((InItem.CurSlide > InItem.TotalSlides) ? InItem.TotalSlides : InItem.CurSlide));
			if (InItem.CurSlide > 0)
			{
				if (InItem.Slide[InItem.CurSlide, 0] >= 0)
				{
					if (InItem.CurSlide > 1)
					{
						if (InItem.Slide[InItem.CurSlide, 0] == 0)
						{
							InItem.Lyrics[2].Text = FolderLyricsHeading[InItem.FolderNo, 1];
						}
						else if (InItem.Slide[InItem.CurSlide, 0] == 102)
						{
							InItem.Lyrics[2].Text = FolderLyricsHeading[InItem.FolderNo, 1] + ((FolderLyricsHeading[InItem.FolderNo, 1] != "") ? " (2)" : "");
						}
						else if (InItem.Slide[InItem.CurSlide, 0] == 111)
						{
							InItem.Lyrics[2].Text = FolderLyricsHeading[InItem.FolderNo, 0];
						}
						else if (InItem.Slide[InItem.CurSlide, 0] == 112)
						{
							InItem.Lyrics[2].Text = FolderLyricsHeading[InItem.FolderNo, 0] + ((FolderLyricsHeading[InItem.FolderNo, 0] != "") ? " (2)" : "");
						}
						else if (InItem.Slide[InItem.CurSlide, 0] == 100)
						{
							InItem.Lyrics[2].Text = FolderLyricsHeading[InItem.FolderNo, 2];
						}
						else if (InItem.Slide[InItem.CurSlide, 0] == 103)
						{
							InItem.Lyrics[2].Text = FolderLyricsHeading[InItem.FolderNo, 2] + ((FolderLyricsHeading[InItem.FolderNo, 2] != "") ? " (2)" : "");
						}
						else if (InItem.Slide[InItem.CurSlide, 0] == 101)
						{
							InItem.Lyrics[2].Text = FolderLyricsHeading[InItem.FolderNo, 3];
						}
						else if (InItem.Verse2Present || (InItem.CurSlide > 1 && InItem.Slide[InItem.CurSlide, 0] == 1))
						{
							InItem.Lyrics[2].Text = VerseTitle[InItem.Slide[InItem.CurSlide, 0]];
						}
						else
						{
							InItem.Lyrics[2].Text = "";
						}
					}
					else
					{
						InItem.Lyrics[2].Text = InItem.Title;
					}
				}
				else
				{
					InItem.Lyrics[2].Text = "";
				}
			}
			if (ShowRunning_ShowNotations == 1)
			{
				num2 = ((num2 <= 0) ? 1 : 0);
			}
			num3 = (num3 + ShowRunning_ShowVerticalAlign) % 3;
			if (InItem.FirstShowing)
			{
				InPictureBox.TransitionType = (ImageTransitionControl.TransitionTypes)transitionType;
				if (LicAdminEnforceDisplay)
				{
					InItem.Show_LicAdim = true;
				}
			}
			else
			{
				InPictureBox.TransitionType = (ImageTransitionControl.TransitionTypes)transitionType2;
			}
			if (InItem.FirstShowing || RedoBackground || ComputeTransition(InItem, ref InPictureBox, TransitionAction) != 0)
			{
				if (InItem.Format.MediaTransparent || (ShowLiveCam && InItem.AtLiveScreen))
				{
					SetTransparentBackground(InItem, ref InPictureBox);
				}
				else
				{
					SetShowBackground(InItem, ref InPictureBox, (!(InItem.Type == "G")) ? true : false);
				}
			}
			else if (ShowLiveCam && InItem.AtLiveScreen)
			{
				SetTransparentBackground(InItem, ref InPictureBox);
			}
			if (InItem.Type != "")
			{
				DrawText(ref InItem, ref InPictureBox, InItem.LyricsAndNotationsList, inUseShadowFont, inUseOutlineFont, num2, inShowInterlace, num3, inHideDisplayPanel, TransitionAction, DoActiveIndicator, ClearAll: false);
			}
			return true;
		}

		public static void DrawText(ref SongSettings InItem, ref ImageTransitionControl PPictureBox, ref ImageTransitionControl OPictureBox, ListView LyricsAndNotationsList)
		{
			if (InItem.OutputStyleScreen)
			{
				DrawText(ref InItem, ref OPictureBox, LyricsAndNotationsList, InItem.Format.UseShadowFont, InItem.Format.UseOutlineFont, InItem.Format.ShowNotations, InItem.Format.ShowInterlace, InItem.Format.ShowVerticalAlign, InItem.Format.HideDisplayPanel, (ImageTransitionControl.TransitionAction)InItem.Format.ShowSlideTransition, DoActiveIndicator: false, ClearAll: false);
			}
			else
			{
				DrawText(ref InItem, ref PPictureBox, LyricsAndNotationsList, InItem.Format.UseShadowFont, InItem.Format.UseOutlineFont, InItem.Format.ShowNotations, InItem.Format.ShowInterlace, InItem.Format.ShowVerticalAlign, InItem.Format.HideDisplayPanel, (ImageTransitionControl.TransitionAction)InItem.Format.ShowSlideTransition, DoActiveIndicator: false, ClearAll: false);
			}
		}

		public static void DrawText(ref SongSettings InItem, ref ImageTransitionControl InPictureBox, ListView LyricsAndNotationsList, bool DoActiveIndicator, bool ClearAll)
		{
			DrawText(ref InItem, ref InPictureBox, LyricsAndNotationsList, InItem.Format.UseShadowFont, InItem.Format.UseOutlineFont, InItem.Format.ShowNotations, InItem.Format.ShowInterlace, InItem.Format.ShowVerticalAlign, InItem.Format.HideDisplayPanel, (ImageTransitionControl.TransitionAction)InItem.Format.ShowSlideTransition, DoActiveIndicator, ClearAll);
		}

		public static void DrawText(ref SongSettings InItem, ref ImageTransitionControl InPictureBox, ListView LyricsAndNotationsList, int InUseShadowFont, int InUseOutlineFont, int InShowNotations, int InShowInterlace, int InShowVerticalAlign, int InHideDisplayPanel, ImageTransitionControl.TransitionAction TransitionAction, bool DoActiveIndicator, bool ClearAll)
		{
			bool flag = (InShowInterlace == 1) ? true : false;
			bool liveCamOnShow = false;
			bool fitAllIntoOneScreen = ((!AutoTextOverflow || InItem.RotateStyle == 2) && InItem.Type != "B") ? true : false;
			int num = 0;
			int num2 = 0;
			if ((InPictureBox.Width <= 0) | (InPictureBox.Height <= 0))
			{
				return;
			}
			if (InPictureBox.NewBackgroundPicture == null)
			{
				SetShowBackground(InItem, ref InPictureBox);
			}
			int width = InPictureBox.NewBackgroundPicture.Width;
			int height = InPictureBox.NewBackgroundPicture.Height;
			for (int i = 0; i <= 2; i++)
			{
				InItem.Lyrics[i].FS_TopOffset = 0;
				InItem.Lyrics[i].FS_OneLyricAndNotationHeight = 0;
				InItem.Lyrics[i].FS_InterlaceGapHeight = 0;
				InItem.Lyrics[i].FS_InterlaceLinePattern = "";
			}
			Image image = new Bitmap(width, height, PixelFormat.Format32bppPArgb);
			Graphics g = Graphics.FromImage(image);
			g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
			g.Clear(Color.Transparent);
			//g.Dispose();
			//image.Dispose();

			Image image2 = new Bitmap(width, height, PixelFormat.Format32bppPArgb);
			Graphics graphics = Graphics.FromImage(image2);
			graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
			graphics.Clear(Color.Transparent);
			//graphics.Dispose();
			//image2.Dispose();

			if (InPictureBox.CurrentCombinedImage == null)
			{
				InPictureBox.CurrentBackgroundPicture = (Image)InPictureBox.BackgroundImage.Clone();
				InPictureBox.CurrentTextImage = (Image)InPictureBox.BackgroundImage.Clone();
				InPictureBox.CurrentCombinedImage = (Image)InPictureBox.BackgroundImage.Clone();
			}
			if (InPictureBox.CurrentPanelImage == null)
			{
				InPictureBox.CurrentPanelImage = (Image)InPictureBox.BackgroundImage.Clone();
			}
			if (InPictureBox.NewTextImage == null)
			{
				InPictureBox.NewTextImage = (Image)InPictureBox.BackgroundImage.Clone();
			}
			if (InPictureBox.NewPanelImage == null)
			{
				InPictureBox.NewPanelImage = (Image)InPictureBox.BackgroundImage.Clone();
			}
			ComputeTransition(InItem, ref InPictureBox, TransitionAction);
			if (InPictureBox.TransitionType != 0)
			{
				InPictureBox.CurrentTextImage = (Image)InPictureBox.NewTextImage.Clone();
				InPictureBox.CurrentPanelImage = (Image)InPictureBox.NewPanelImage.Clone();
			}
			if (ShowRunning_UseShadowFont == 1)
			{
				InUseShadowFont = ((InUseShadowFont <= 0) ? 1 : 0);
			}
			if (ShowRunning_UseOutlineFont == 1)
			{
				InUseOutlineFont = ((InUseOutlineFont <= 0) ? 1 : 0);
			}
			if (ShowRunning_ShowInterlace == 1)
			{
				flag = !flag;
			}
			if (ShowLiveCam & InItem.AtLiveScreen)
			{
				ClearAll = true;
				liveCamOnShow = true;
			}
			else if (InItem.OutputStyleScreen && ShowLiveClear)
			{
				ClearAll = true;
			}
			if (InItem.OutputStyleScreen && ShowLiveBlack)
			{
				ClearAll = true;
				g.Clear(BlackScreenColour);
				graphics.Clear(BlackScreenColour);
			}
			if (ClearAll)
			{
				InPictureBox.NewPanelImage = image2;
			}
			else if (InItem.Lyrics[0].Font == null)
			{
				InPictureBox.NewPanelImage = image2;
			}
			else
			{
				if (((ShowDataDisplayMode == 1 && InHideDisplayPanel == 0) | InItem.Show_LicAdim) && InItem.Type != "G")
				{
					DrawDisplayPanel(InItem, InHideDisplayPanel, ref InPictureBox, graphics);
				}
				InPictureBox.NewPanelImage = image2;
				int num3 = (int)InItem.Lyrics[1].FS_Font.Size;
				if ((InItem.Type == "D") | (InItem.Type == "B") | (InItem.Type == "T") | (InItem.Type == "I") | (InItem.Type == "W") | (InItem.Type == "M"))
				{
					int num4 = (InItem.Format.ShowLyrics + ShowRunning_ShowLyrics) % 3;
					int num5 = (InItem.Format.ShowSongHeadings == 1 || (InItem.Format.ShowSongHeadings == 2 && InItem.FirstShowing)) ? 1 : 0;
					num5 = (num5 + ShowRunning_ShowSongHeadings) % 2;
					if (InItem.Type == "M")
					{
						num5 = 0;
					}
					InItem.CurSlideIsVerse = false;
					int num6 = InItem.CurSlide;
					if (InItem.Slide[num6, 0] < 0)
					{
						num6--;
						while (num6 >= 0 && InItem.Slide[num6, 0] < 0)
						{
							num6--;
						}
						if (num6 < 0)
						{
							num6 = 0;
						}
					}
					InItem.CurSlideIsVerse = (InItem.Slide[num6, 0] > 0 && InItem.Slide[num6, 0] < 99);
					bool flag2 = false;
					if ((num4 < 2) | (InItem.Slide[InItem.CurSlide, 1] < 0) | (InItem.Slide[InItem.CurSlide, 3] < 0))
					{
						flag2 = true;
						flag = false;
					}
					int num7 = 0;
					int num8 = 0;
					int num9 = 0;
					Font MainFont = new Font("Microsoft Sans Serif", 30f);
					Font MainFont2 = new Font("Microsoft Sans Serif", 30f);
					Font MainFont3 = new Font("Microsoft Sans Serif", 30f);
					Font NotationsFont = new Font("Microsoft Sans Serif", 30f);
					Font NotationsFont2 = new Font("Microsoft Sans Serif", 30f);
					int num10 = 0;
					int num11 = 0;
					num7 = GetOneRegionHeight(ref InItem, ref InPictureBox, 2, LyricsAndNotationsList, ref g, InUseShadowFont, InUseOutlineFont, InShowNotations, flag2, ref MainFont, ref NotationsFont, InShowInterlace, fitAllIntoOneScreen, UseLargestFontSize);
					if ((num4 == 0) | (num4 == 2))
					{
						num8 = GetOneRegionHeight(ref InItem, ref InPictureBox, 0, LyricsAndNotationsList, ref g, InUseShadowFont, InUseOutlineFont, InShowNotations, flag2, ref MainFont2, ref NotationsFont, InShowInterlace, fitAllIntoOneScreen, UseLargestFontSize);
					}
					if ((num4 == 1) | (num4 == 2))
					{
						num9 = GetOneRegionHeight(ref InItem, ref InPictureBox, 1, LyricsAndNotationsList, ref g, InUseShadowFont, InUseOutlineFont, InShowNotations, flag2, ref MainFont3, ref NotationsFont2, InShowInterlace, fitAllIntoOneScreen, UseLargestFontSize);
						if (flag2)
						{
							InItem.Lyrics[1].FS_Top = InItem.Lyrics[0].FS_Top;
						}
					}
					switch (InShowVerticalAlign)
					{
						case 0:
							num10 = ((num5 <= 0) ? (-num7) : 0);
							break;
						case 1:
							num10 = (InItem.Lyrics[0].FS_Height - (num8 + num9 + num11 + ((num5 <= 0) ? num7 : ((!(InItem.Lyrics[2].Text != "")) ? num7 : 0)))) / 2;
							break;
						case 2:
							num10 = InItem.Lyrics[0].FS_Height - (num8 + num9 + num11);
							break;
					}
					num10 += ShowTopBorderSize;
					if ((num4 == 0) | (num4 == 2))
					{
						DrawOneRegion(ref InItem, ref InPictureBox, 0, LyricsAndNotationsList, ref g, InUseShadowFont, InUseOutlineFont, InShowNotations, flag2, ref MainFont2, ref NotationsFont, num10, flag, InShowVerticalAlign, 0, fitAllIntoOneScreen, UseLargestFontSize);
					}
					if ((num4 == 1) | (num4 == 2))
					{
						DrawOneRegion(ref InItem, ref InPictureBox, 1, LyricsAndNotationsList, ref g, InUseShadowFont, InUseOutlineFont, InShowNotations, flag2, ref MainFont3, ref NotationsFont2, num10, flag, InShowVerticalAlign, num8 + num11, fitAllIntoOneScreen, UseLargestFontSize);
					}
					if (num5 > 0)
					{
						DrawOneRegion(ref InItem, ref InPictureBox, 2, LyricsAndNotationsList, ref g, InUseShadowFont, InUseOutlineFont, InShowNotations, flag2, ref MainFont, ref NotationsFont2, num10, flag, InShowVerticalAlign, 0, fitAllIntoOneScreen, UseLargestFontSize);
					}
				}
				if ((InShowNotations == 1) & (InItem.Capo > 0))
				{
					DrawCapoSettings(InItem, g);
				}
			}
			bool firstShowing = InItem.FirstShowing;
			if (InItem.FirstShowing)
			{
				InPictureBox.TransitionTime = ((ShowRunning && InItem.OutputStyleScreen && !InItem.AtLiveScreen) ? 0.7f : 1.5f);
				InPictureBox.ItemChanged = true;
			}
			else
			{
				InPictureBox.TransitionTime = ((ShowRunning && InItem.OutputStyleScreen && !InItem.AtLiveScreen) ? 0.1f : 0.6f);
				InPictureBox.ItemChanged = false;
			}
			if (!ClearAll)
			{
				InItem.FirstShowing = false;
			}
			if (InPictureBox.TransitionType != 0)
			{
				InPictureBox.CurrentTextImage = (Image)InPictureBox.NewTextImage.Clone();
			}
			InPictureBox.NewTextImage = image;
			if (firstShowing || DoActiveIndicator)
			{
				LoadReferenceAlert(ref InPictureBox, InItem, ClearAll, DoActiveIndicator);
			}
			InPictureBox.Go(TransitionAction, firstShowing, ClearAll, DoActiveIndicator, liveCamOnShow);
			//GC.Collect();
		}

		public static int DrawOneRegion(ref SongSettings InItem, ref ImageTransitionControl InPictureBox, int RegNum, ListView LyricsAndNotationsList, ref Graphics g, int InUseShadowFont, int InUseOutlineFont, int InShowNotations, bool OnlyOneRegionShown, ref Font MainFont, ref Font NotationsFont, int OffsetAfterAlignment, bool InterlaceOption, int InShowVerticalAlign, int Region1Height, bool FitAllIntoOneScreen, bool UseLargestFontSize)
		{
			if (InItem.LyricsAndNotationsList.Items.Count == 0)
			{
				return 0;
			}
			int HeightOffset = 0;
			int num = -1;
			int num2 = -1;
			string lyricsText = "";
			int fS_Left = InItem.Lyrics[RegNum].FS_Left;
			int num3 = InItem.Lyrics[RegNum].FS_Top;
			int fS_Width = InItem.Lyrics[RegNum].FS_Width;
			int num4 = (InItem.Slide[0, 3] > 0 && !OnlyOneRegionShown) ? InItem.Lyrics[RegNum].FS_Height_R2Bound : InItem.Lyrics[RegNum].FS_Height;
			SizeF layoutArea = new SizeF(fS_Width, 32000f);
			int num5 = (int)((double)g.MeasureString("A", MainFont, layoutArea).Height * MainFontSpacingFactor[InItem.FolderNo, (RegNum != 0) ? 1 : 0]);
			int notationsLineHeight = (int)((double)num5 * ((MainFont.Size >= 2f) ? NotationFontFactor : 1.0));
			int notationsLineTextVOffset = 0;
			string interlaceLinePattern = (RegNum == 1) ? InItem.Lyrics[RegNum].FS_InterlaceLinePattern : "";
			int num6 = 0;
			switch (RegNum)
			{
				case 0:
					num = InItem.Slide[InItem.CurSlide, 1];
					num2 = InItem.Slide[InItem.CurSlide, 2];
					break;
				case 1:
					num = InItem.Slide[InItem.CurSlide, 3];
					num2 = InItem.Slide[InItem.CurSlide, 4];
					break;
				case 2:
					lyricsText = InItem.Lyrics[RegNum].Text;
					num3 += OffsetAfterAlignment;
					DrawOneLine(rect_normal: new RectangleF(fS_Left, num3 + InItem.Lyrics[RegNum].FS_TopOffset, fS_Width, num4), InItem: ref InItem, InPictureBox: ref InPictureBox, InLyrics: InItem.Lyrics[2], RegionNumber: 2, Slide: InItem.Slide, LyricsAndNotationsList: LyricsAndNotationsList, g: ref g, MainFont: MainFont, NotationsFont: NotationsFont, OneLineHeight: num5, NotationsLineHeight: 0, NotationsLineTextVOffset: 0, InHeight: num4, LyricsText: lyricsText, InUseShadowFont: InUseShadowFont, InUseOutlineFont: InUseOutlineFont, InShowNotations: 0);
					return 0;
			}
			if ((num < 0) | (num2 < 0))
			{
				return 0;
			}
			if (OnlyOneRegionShown)
			{
				num3 += OffsetAfterAlignment;
			}
			else
			{
				switch (RegNum)
				{
					case 0:
						num3 += OffsetAfterAlignment;
						if (InterlaceOption)
						{
							InItem.Lyrics[0].FS_InterlaceGapHeight = InItem.Lyrics[1].FS_OneLyricAndNotationHeight;
						}
						break;
					case 1:
						if (InterlaceOption)
						{
							num3 = InItem.Lyrics[0].FS_Top + OffsetAfterAlignment + (int)((double)InItem.Lyrics[0].FS_OneLyricAndNotationHeight * 0.9);
							InItem.Lyrics[1].FS_InterlaceGapHeight = InItem.Lyrics[0].FS_OneLyricAndNotationHeight;
							break;
						}
						num3 = InItem.Lyrics[0].FS_Top + OffsetAfterAlignment + Region1Height + Buffer_LS_Height / 30;
						if (LineBetweenRegions)
						{
							OutputOneLineToScreen(InItem, "<<DrawLine>>", MainFont, g, InItem.Lyrics[RegNum].ForeColour, StringAlignment.Center, InUseShadowFont, InUseOutlineFont, fS_Left, num3 - Buffer_LS_Height / 40, fS_Width, 0);
						}
						break;
				}
			}
			InItem.Lyrics[RegNum].FS_TopOffset = num3;
			RectangleF rect_normal2 = new RectangleF(fS_Left, num3, fS_Width, num4);
			RectangleF rectangleF = new RectangleF(rect_normal2.Left + MainFont.Size / 30f + 1f, rect_normal2.Top + MainFont.Size / 30f + 1f, rect_normal2.Width, rect_normal2.Height);
			if (num <= num2)
			{
				for (int i = num; i <= num2; i++)
				{
					num6 = 0;
					DrawOneLine(ref InItem, ref InPictureBox, InItem.Lyrics[RegNum], RegNum, InItem.Slide, LyricsAndNotationsList, ref g, MainFont, NotationsFont, num5, notationsLineHeight, notationsLineTextVOffset, rect_normal2, num4, lyricsText, InUseShadowFont, InUseOutlineFont, InShowNotations, i, ref HeightOffset, ref num6, InterlaceOption, interlaceLinePattern);
					if (RegNum == 0)
					{
						SongLyrics obj = InItem.Lyrics[1];
						obj.FS_InterlaceLinePattern = obj.FS_InterlaceLinePattern + Convert.ToString(num6) + '>';
					}
				}
			}
			return (int)MainFont.Size;
		}

		public static void DrawOneLine(ref SongSettings InItem, ref ImageTransitionControl InPictureBox, SongLyrics InLyrics, int RegionNumber, int[,] Slide, ListView LyricsAndNotationsList, ref Graphics g, Font MainFont, Font NotationsFont, int OneLineHeight, int NotationsLineHeight, int NotationsLineTextVOffset, RectangleF rect_normal, int InHeight, string LyricsText, int InUseShadowFont, int InUseOutlineFont, int InShowNotations)
		{
			DrawOneLine(ref InItem, ref InPictureBox, InLyrics, RegionNumber, Slide, LyricsAndNotationsList, ref g, MainFont, NotationsFont, OneLineHeight, NotationsLineHeight, NotationsLineTextVOffset, rect_normal, InHeight, LyricsText, InUseShadowFont, InUseOutlineFont, InShowNotations, -1);
		}

		public static void DrawOneLine(ref SongSettings InItem, ref ImageTransitionControl InPictureBox, SongLyrics InLyrics, int RegionNumber, int[,] Slide, ListView LyricsAndNotationsList, ref Graphics g, Font MainFont, Font NotationsFont, int OneLineHeight, int NotationsLineHeight, int NotationsLineTextVOffset, RectangleF rect_normal, int InHeight, string LyricsText, int InUseShadowFont, int InUseOutlineFont, int InShowNotations, int CurLine)
		{
			int HeightOffset = 0;
			int LinesRequired = 0;
			DrawOneLine(ref InItem, ref InPictureBox, InLyrics, RegionNumber, Slide, LyricsAndNotationsList, ref g, MainFont, NotationsFont, OneLineHeight, NotationsLineHeight, NotationsLineTextVOffset, rect_normal, InHeight, LyricsText, InUseShadowFont, InUseOutlineFont, InShowNotations, CurLine, ref HeightOffset, ref LinesRequired, InterlaceOption: false);
		}

		public static void DrawOneLine(ref SongSettings InItem, ref ImageTransitionControl InPictureBox, SongLyrics InLyrics, int RegionNumber, int[,] Slide, ListView LyricsAndNotationsList, ref Graphics g, Font MainFont, Font NotationsFont, int OneLineHeight, int NotationsLineHeight, int NotationsLineTextVOffset, RectangleF rect_normal, int InHeight, string LyricsText, int InUseShadowFont, int InUseOutlineFont, int InShowNotations, int CurLine, ref int HeightOffset)
		{
			int LinesRequired = 0;
			DrawOneLine(ref InItem, ref InPictureBox, InLyrics, RegionNumber, Slide, LyricsAndNotationsList, ref g, MainFont, NotationsFont, OneLineHeight, NotationsLineHeight, NotationsLineTextVOffset, rect_normal, InHeight, LyricsText, InUseShadowFont, InUseOutlineFont, InShowNotations, CurLine, ref HeightOffset, ref LinesRequired, InterlaceOption: false);
		}

		public static void DrawOneLine(ref SongSettings InItem, ref ImageTransitionControl InPictureBox, SongLyrics InLyrics, int RegionNumber, int[,] Slide, ListView LyricsAndNotationsList, ref Graphics g, Font MainFont, Font NotationsFont, int OneLineHeight, int NotationsLineHeight, int NotationsLineTextVOffset, RectangleF rect_normal, int InHeight, string LyricsText, int InUseShadowFont, int InUseOutlineFont, int InShowNotations, int CurLine, ref int HeightOffset, ref int LinesRequired)
		{
			DrawOneLine(ref InItem, ref InPictureBox, InLyrics, RegionNumber, Slide, LyricsAndNotationsList, ref g, MainFont, NotationsFont, OneLineHeight, NotationsLineHeight, NotationsLineTextVOffset, rect_normal, InHeight, LyricsText, InUseShadowFont, InUseOutlineFont, InShowNotations, CurLine, ref HeightOffset, ref LinesRequired, InterlaceOption: false);
		}

		public static void DrawOneLine(ref SongSettings InItem, ref ImageTransitionControl InPictureBox, SongLyrics InLyrics, int RegionNumber, int[,] Slide, ListView LyricsAndNotationsList, ref Graphics g, Font MainFont, Font NotationsFont, int OneLineHeight, int NotationsLineHeight, int NotationsLineTextVOffset, RectangleF rect_normal, int InHeight, string LyricsText, int InUseShadowFont, int InUseOutlineFont, int InShowNotations, int CurLine, ref int HeightOffset, ref int LinesRequired, bool InterlaceOption)
		{
			DrawOneLine(ref InItem, ref InPictureBox, InLyrics, RegionNumber, Slide, LyricsAndNotationsList, ref g, MainFont, NotationsFont, OneLineHeight, NotationsLineHeight, NotationsLineTextVOffset, rect_normal, InHeight, LyricsText, InUseShadowFont, InUseOutlineFont, InShowNotations, CurLine, ref HeightOffset, ref LinesRequired, InterlaceOption, "");
		}

		public static void DrawOneLine(ref SongSettings InItem, ref ImageTransitionControl InPictureBox, SongLyrics InLyrics, int RegionNumber, int[,] Slide, ListView LyricsAndNotationsList, ref Graphics g, Font MainFont, Font NotationsFont, int OneLineHeight, int NotationsLineHeight, int NotationsLineTextVOffset, RectangleF rect_normal, int InHeight, string LyricsText, int InUseShadowFont, int InUseOutlineFont, int InShowNotations, int CurLine, ref int HeightOffset, ref int LinesRequired, bool InterlaceOption, string InterlaceLinePattern)
		{
			StringFormat stringFormat = new StringFormat();
			SizeF sizeF = new SizeF(rect_normal.Width, 32000f);
			int startPos = 0;
			int num = 0;
			int EndExtractedTextPos = -1;
			int R2_MaxLinesPermitted = Convert.ToInt32(DataUtil.ExtractOneInfo(ref InterlaceLinePattern, '>'));
			if (R2_MaxLinesPermitted < 0)
			{
				R2_MaxLinesPermitted = 0;
			}
			string notationsString;
			string InString;
			if (CurLine < 0)
			{
				if (LyricsText == "")
				{
					return;
				}
				notationsString = "";
				InString = "";
			}
			else
			{
				LyricsText = InItem.LyricsAndNotationsList.Items[CurLine].SubItems[2].Text;
				notationsString = InItem.LyricsAndNotationsList.Items[CurLine].SubItems[3].Text;
				InString = InItem.LyricsAndNotationsList.Items[CurLine].SubItems[4].Text;
			}
			if (InterlaceOption && RegionNumber == 1)
			{
				while ((g.MeasureString(LyricsText, MainFont, 100000).Width + 10f > rect_normal.Width * (float)R2_MaxLinesPermitted) & (MainFont.Size > 1f))
				{
					MainFont = new Font(InLyrics.Font.Name, MainFont.Size - 1f, InLyrics.Font.Style);
					NotationsFont = new Font(InLyrics.Font.Name, Convert.ToInt32((double)MainFont.Size * NotationFontFactor), InLyrics.Font.Style);
				}
			}
			LinesRequired++;
			string ReplacedLog = "";
			if (UseLargestFontSize)
			{
				InString = "";
				ReplacedLog = ActionWordWrapSpacesAtStart(ref LyricsText);
			}
			int num2 = Convert.ToInt32(DataUtil.ExtractOneInfo(ref InString, '>'));
			if (num2 > 0)
			{
				if (num2 == 1)
				{
					num2 = 0;
				}
				else
				{
					DataUtil.ExtractOneInfo(ref InString, '>');
					num2 = Convert.ToInt32(DataUtil.ExtractOneInfo(ref InString, '>'));
				}
			}
			string a = DisplayTextByWidthOneLine(InItem, LyricsText, InLyrics, RegionNumber, MainFont, NotationsFont, g, (int)rect_normal.Left, (int)rect_normal.Top, (int)rect_normal.Width, ref HeightOffset, OneLineHeight, NotationsLineHeight, NotationsLineTextVOffset, startPos, InUseShadowFont, InUseOutlineFont, InShowNotations, notationsString, ref EndExtractedTextPos, ref R2_MaxLinesPermitted, InterlaceOption, num2, IsWrappedText: false, ref ReplacedLog);
			if (num2 > 0)
			{
				num2 = Convert.ToInt32(DataUtil.ExtractOneInfo(ref InString, '>'));
			}
			while (a != "" && EndExtractedTextPos > 0)
			{
				startPos = EndExtractedTextPos + 1;
				LinesRequired++;
				a = DisplayTextByWidthOneLine(InItem, LyricsText, InLyrics, RegionNumber, MainFont, NotationsFont, g, (int)rect_normal.Left, (int)rect_normal.Top, (int)rect_normal.Width, ref HeightOffset, OneLineHeight, NotationsLineHeight, NotationsLineTextVOffset, startPos, InUseShadowFont, InUseOutlineFont, InShowNotations, notationsString, ref EndExtractedTextPos, ref R2_MaxLinesPermitted, InterlaceOption, num2, IsWrappedText: true, ref ReplacedLog);
				if (num2 > 0)
				{
					num2 = Convert.ToInt32(DataUtil.ExtractOneInfo(ref InString, '>'));
				}
			}
			if (R2_MaxLinesPermitted > 0 && InterlaceOption)
			{
				HeightOffset += (InLyrics.FS_OneLyricAndNotationHeight + InLyrics.FS_InterlaceGapHeight) * R2_MaxLinesPermitted;
			}
		}

		public static string DisplayTextByWidthOneLine(SongSettings InItem, string InText, SongLyrics InLyrics, int RegionNumber, Font MainFont, Font NotationsFont, Graphics g, int InLeft, int InTop, int InWidth, ref int HeightOffset, int OneLineHeight, int NotationsLineHeight, int NotationsLineTextVOffset, int StartPos, int InUseShadowFont, int InUseOutlineFont, int InShowNotations, string NotationsString, ref int EndExtractedTextPos, ref int R2_MaxLinesPermitted, bool InterlaceOption, int InSetLength, bool IsWrappedText, ref string ReplacedLog)
		{
			int length = InText.Length;
			if (length == 0)
			{
				EndExtractedTextPos = -1;
				return "";
			}
			string ExtractedText = "";
			bool flag = false;
			R2_MaxLinesPermitted--;
			SubDivideOneOutputText(InText, MainFont, NotationsFont, g, InWidth, InShowNotations, NotationsString, length, InSetLength, StartPos, ref EndExtractedTextPos, ref ExtractedText);
			int num = (int)(double)g.MeasureString(ExtractedText, MainFont).Width;
			HeightOffset += ((InShowNotations == 1) ? NotationsLineHeight : 0);
			ActionUndoWordWrapSpacesAtStart(ref ExtractedText, ref ReplacedLog);
			SubstituteDashes(ref ExtractedText, InShowNotations);
			int num2 = OutputOneLineToScreen(InItem, ExtractedText, MainFont, g, InLyrics.ForeColour, InLyrics.TextAlign, InUseShadowFont, InUseOutlineFont, InLeft, InTop + HeightOffset, InWidth, 0, (IsWrappedText && WordWrapLeftAlignIndent && InItem.Type != "I" && RegionNumber != 2) ? true : false);
			if (InShowNotations == 1)
			{
				int num3 = (int)MainFont.Size - 2;
				if (num3 < 1)
				{
					num3 = 1;
				}
				Font font = new Font(MainFont.Name, num3);
				HeightOffset -= NotationsLineHeight;
				int num4 = 0;
				while (NotationsString != "")
				{
					string text = DataUtil.ExtractOneInfo(ref NotationsString, ';');
					int num5 = Convert.ToInt32(DataUtil.ExtractOneInfo(ref NotationsString, ';'));
					if (!(text != "-1") || num5 < 0 || num5 < StartPos)
					{
						continue;
					}
					for (int i = StartPos; i <= EndExtractedTextPos; i++)
					{
						if ((((i == num5) ? 1 : 0) & ((i != EndExtractedTextPos) ? 1 : ((EndExtractedTextPos == length) ? 1 : 0))) != 0)
						{
							int iLen = i - StartPos;
							string text2 = DataUtil.Mid(InText, StartPos, iLen);
							int num6 = (int)g.MeasureString(text2, font).Width;
							OutputOneLineToScreen(InItem, text, NotationsFont, g, InLyrics.ForeColour, StringAlignment.Near, InUseShadowFont, InUseOutlineFont, num2 + num6 + num4, InTop + HeightOffset + NotationsLineTextVOffset, InWidth, 0, (IsWrappedText && WordWrapLeftAlignIndent && InItem.Type != "I") ? true : false);
							num4 += (int)((i == EndExtractedTextPos) ? g.MeasureString(text + "S", NotationsFont).Width : 0f);
							i = EndExtractedTextPos;
						}
					}
				}
				HeightOffset += NotationsLineHeight;
			}
			HeightOffset += (InterlaceOption ? (InLyrics.FS_OneLyricAndNotationHeight + InLyrics.FS_InterlaceGapHeight) : OneLineHeight);
			return DataUtil.Right(InText, length - EndExtractedTextPos - 1);
		}

		public static int OutputOneLineToScreen(SongSettings InItem, string ExtractedText, Font InFont, Graphics g, Color InColour, StringAlignment alignformat, int InUseShadowFont, int InUseOutlineFont, int x, int y, int w, int h)
		{
			return OutputOneLineToScreen(InItem, ExtractedText, InFont, g, InColour, alignformat, InUseShadowFont, InUseOutlineFont, x, y, w, h, IndentLeftAligned: false);
		}

		public static int OutputOneLineToScreen(SongSettings InItem, string ExtractedText, Font InFont, Graphics g, Color InColour, StringAlignment alignformat, int InUseShadowFont, int InUseOutlineFont, int x, int y, int w, int h, bool IndentLeftAligned)
		{
			GraphicsPath graphicsPath = new GraphicsPath();
			GraphicsPath graphicsPath2 = new GraphicsPath();
			float num = InFont.Size / (float)AdjustedOutlineThreshold;
			string text = "";
			if (InItem.Type == "B" && ExtractedText.IndexOf('\u0098') >= 0)
			{
				string[] array = ExtractedText.Split('\u0098');
				if (array.GetUpperBound(0) > 0)
				{
					text = DataUtil.Trim(array[0]);
					ExtractedText = DataUtil.Trim(array[1]);
				}
			}
			if (num < 1f)
			{
				num = 1f;
			}
			Pen pen = new Pen(BlackScreenColour, num);
			int num2 = (int)((double)InFont.Size * 1.2999999523162842);
			int x2 = x;
			StringFormat stringFormat = new StringFormat();
			switch (alignformat)
			{
				case StringAlignment.Center:
					x += w / 2;
					break;
				case StringAlignment.Far:
					x += w;
					break;
				default:
					if (IndentLeftAligned)
					{
						ExtractedText = "  " + ExtractedText;
					}
					break;
			}
			if (InFont.Size <= 1f)
			{
				return x;
			}
			stringFormat.Alignment = alignformat;
			g.SmoothingMode = SmoothingMode.AntiAlias;
			int num3 = 0;
			int num4 = 0;
			int num5 = num2 / 10;
			if (num5 < 1)
			{
				num5 = 1;
			}
			int num6 = y;
			if (h > 0)
			{
				graphicsPath2.AddString(ExtractedText, new FontFamily(InFont.Name), (int)InFont.Style, num2, new Rectangle(num3 + x, y, w, h), stringFormat);
				int num7 = (int)graphicsPath2.GetBounds().Height + 10;
				num6 += h - num7;
			}
			if (InUseShadowFont > 0)
			{
				int num8 = (int)(InFont.Size / 22f) + 1;
				if (text != "" && alignformat == StringAlignment.Near)
				{
					num3 = DrawSuperScript(graphicsPath, text, InFont, num2, new Rectangle(x + num8, y + num8, (h != 0) ? w : 0, h), AlignLeft: true);
					g.FillPath(new SolidBrush(BlackScreenColour), graphicsPath);
					graphicsPath.Reset();
				}
				if (ExtractedText == "<<DrawLine>>")
				{
					graphicsPath.AddRectangle(new Rectangle(x2, y, w, num5));
				}
				else if (h == 0)
				{
					graphicsPath.AddString(ExtractedText, new FontFamily(InFont.Name), (int)InFont.Style, num2, new Point(num3 + x + num8, y + num8), stringFormat);
				}
				else
				{
					graphicsPath.AddString(ExtractedText, new FontFamily(InFont.Name), (int)InFont.Style, num2, new Rectangle(num3 + x + num8, num6 + num8, w, h), stringFormat);
				}
				g.FillPath(new SolidBrush(BlackScreenColour), graphicsPath);
				num4 = (int)graphicsPath.GetBounds().Left;
				graphicsPath.Reset();
				if (text != "" && alignformat != 0)
				{
					DrawSuperScript(graphicsPath, text, InFont, num2, new Rectangle(num4, y + num8, (h != 0) ? w : 0, h), AlignLeft: false);
					g.FillPath(new SolidBrush(BlackScreenColour), graphicsPath);
					graphicsPath.Reset();
				}
			}
			if (text != "" && alignformat == StringAlignment.Near)
			{
				num3 = DrawSuperScript(graphicsPath, text, InFont, num2, new Rectangle(x, y, (h != 0) ? w : 0, h), AlignLeft: true);
				g.FillPath(new SolidBrush(InColour), graphicsPath);
				if (InUseOutlineFont > 0)
				{
					g.DrawPath(pen, graphicsPath);
				}
				graphicsPath.Reset();
			}
			if (ExtractedText == "<<DrawLine>>")
			{
				graphicsPath.AddRectangle(new Rectangle(x2, y, w, num5));
			}
			else if (h == 0)
			{
				graphicsPath.AddString(ExtractedText, new FontFamily(InFont.Name), (int)InFont.Style, num2, new Point(num3 + x, y), stringFormat);
			}
			else
			{
				graphicsPath.AddString(ExtractedText, new FontFamily(InFont.Name), (int)InFont.Style, num2, new Rectangle(num3 + x, num6, w, h), stringFormat);
			}
			g.FillPath(new SolidBrush(InColour), graphicsPath);
			if (InUseOutlineFont > 0)
			{
				g.DrawPath(pen, graphicsPath);
			}
			num4 = (int)graphicsPath.GetBounds().Left;
			graphicsPath.Reset();
			if (text != "" && alignformat != 0)
			{
				DrawSuperScript(graphicsPath, text, InFont, num2, new Rectangle(num4, y, (h != 0) ? w : 0, h), AlignLeft: false);
				g.FillPath(new SolidBrush(InColour), graphicsPath);
				if (InUseOutlineFont > 0)
				{
					g.DrawPath(pen, graphicsPath);
				}
				graphicsPath.Reset();
			}
			if (InUseOutlineFont > 0)
			{
				g.DrawPath(pen, graphicsPath);
			}
			if (h == 0)
			{
				return num4;
			}
			return num6;
		}

		public static int DrawSuperScript(GraphicsPath pth, string InText, Font InFont, int InFontSize, Rectangle InRectangle, bool AlignLeft)
		{
			StringFormat stringFormat = new StringFormat();
			if (AlignLeft)
			{
				stringFormat.Alignment = StringAlignment.Near;
			}
			else
			{
				stringFormat.Alignment = StringAlignment.Far;
			}
			InFontSize = InFontSize * 7 / 10;
			if (InRectangle.Height == 0)
			{
				pth.AddString(InText, new FontFamily(InFont.Name), (int)InFont.Style, InFontSize, new Point(InRectangle.Left, InRectangle.Top), stringFormat);
			}
			else
			{
				pth.AddString(InText, new FontFamily(InFont.Name), (int)InFont.Style, InFontSize, InRectangle, stringFormat);
			}
			int num = (int)pth.GetBounds().Width;
			int num2 = num / InText.Length * 2 / 3;
			return num + num2;
		}

		public static void DrawCapoSettings(SongSettings InItem, Graphics g)
		{
			SongLyrics songLyrics = InItem.Lyrics[3];
			StringFormat stringFormat = new StringFormat();
			int value = (int)songLyrics.FS_Font.Size;
			int fS_Left = songLyrics.FS_Left;
			int fS_Top = songLyrics.FS_Top;
			int fS_Width = songLyrics.FS_Width;
			int fS_Height = songLyrics.FS_Height;
			stringFormat.Alignment = StringAlignment.Far;
			Font font = new Font(songLyrics.FS_Font.Name, Convert.ToInt32(value), songLyrics.FS_Font.Style);
			int num = fS_Top + fS_Height / 2;
			g.DrawString(layoutRectangle: new RectangleF(fS_Left, num, fS_Width, fS_Height), s: "Capo " + Convert.ToString(InItem.Capo), font: font, brush: new SolidBrush(songLyrics.ForeColour), format: stringFormat);
		}

		public static void DrawDisplayPanel(SongSettings InItem, int InHideDisplayPanel, ref ImageTransitionControl InPictureBox, Graphics g)
		{
			if (InItem.ItemID == "")
			{
				return;
			}
			SongLyrics songLyrics = InItem.Lyrics[3];
			StringFormat stringFormat = new StringFormat();
			int num = (int)songLyrics.FS_Font.Size;
			int num2 = 0;
			int num3 = 0;
			int num4 = 1;
			int num5 = 0;
			Color inColour = (PanelTextColourAsRegion1 > 0) ? songLyrics.ForeColour : PanelTextColour;
			int fS_Left = songLyrics.FS_Left;
			int fS_Top = songLyrics.FS_Top;
			int fS_Width = songLyrics.FS_Width;
			int fS_Height = songLyrics.FS_Height;
			if (PanelBackColourTransparent < 1)
			{
				g.FillRectangle(new SolidBrush(PanelBackColour), 0, fS_Top - 5, Buffer_LS_Width, fS_Height + 5);
			}
			Font inFont = new Font(songLyrics.FS_Font.Name, num, songLyrics.FS_Font.Style);
			num2 = fS_Top;
			string text = InItem.Writer + (((InItem.Writer != "") & (InItem.Copyright != "")) ? "; " : "") + InItem.Copyright;
			string text2 = text;
			text = text2 + ((text == "") ? "" : " ") + InItem.Show_LicAdminInfo1 + ((InItem.Show_LicAdminInfo1 == "") ? "" : " ") + InItem.Show_LicAdminInfo2;
			string text3 = InItem.PrevTitle;
			string text4 = InItem.NextTitle;
			if (ShowDataDisplayMode > 0 && InHideDisplayPanel == 0)
			{
				int num6 = 0;
				int num7 = 0;
				int num8 = 0;
				int num9 = 0;
				int num10 = 0;
				float num11 = 1.18f;
				int num12 = (int)((float)fS_Width * 0.15f);
				int num13 = (int)((float)fS_Width * ((InItem.TotalSlides > 20) ? 0.15f : 0.25f));
				int num14 = (int)((float)fS_Width * (0.34f + (float)(ShowDataDisplayIndicatorsFontSize - 8) * 0.032f));
				int num15 = fS_Width;
				num4 = DisplayFontSize(11, Buffer_LS_Width, 3, 1);
				inFont = new Font(songLyrics.FS_Font.Name, num4, songLyrics.FS_Font.Style);
				int num16 = num4 * fS_Height / num;
				int num17 = fS_Top + (fS_Height - num16);
				if (ShowDataDisplayPrevNext > 0)
				{
					num15 -= num12;
					string text5 = "<<";
					string text6 = ">>";
					if (text3 == "")
					{
						text5 = "...  ";
					}
					if (text4 == "")
					{
						text6 = "...  ";
					}
					num9 = (int)(g.MeasureString(text5, inFont, 10000).Width * num11);
					num10 = (int)(g.MeasureString(text6, inFont, 10000).Width * num11);
					num7 = (int)(g.MeasureString(text3, inFont, 10000).Width * num11);
					num8 = (int)(g.MeasureString(text4, inFont, 10000).Width * num11);
					if (num13 > 0)
					{
						float num18 = 0f;
						if (num7 > 0 && num7 > num13 - num9)
						{
							num18 = (float)(num13 - num9) / (float)num7;
							text3 = DataUtil.Left(text3, (int)(num18 * (float)(text3.Length - 3))) + "...";
						}
						if (num8 > 0 && num8 > num13 - num10)
						{
							num18 = (float)(num13 - num10) / (float)num8;
							text4 = DataUtil.Left(text4, (int)(num18 * (float)(text4.Length - 3))) + "...";
						}
					}
					text3 += text5;
					text4 += text6;
					num7 = (int)(g.MeasureString(text3, inFont, 10000).Width * num11);
					num8 = (int)(g.MeasureString(text4, inFont, 10000).Width * num11);
					num6 = ((num7 > num8) ? num7 : num8);
					stringFormat.Alignment = StringAlignment.Far;
					RectangleF rectangleF = new RectangleF(fS_Width - num7, num17 + 1, num7, num16);
					OutputOneLineToScreen(InItem, text3, inFont, g, inColour, stringFormat.Alignment, ShowDataDisplayFontShadow, ShowDataDisplayFontOutline, (int)rectangleF.Left, (int)rectangleF.Top, (int)rectangleF.Width, 0);
					rectangleF = new RectangleF(fS_Width - num8, num17 + num16 / 2, num8, num16);
					OutputOneLineToScreen(InItem, text4, inFont, g, inColour, stringFormat.Alignment, ShowDataDisplayFontShadow, ShowDataDisplayFontOutline, (int)rectangleF.Left, (int)rectangleF.Top, (int)rectangleF.Width, 0);
					num6 = ((num7 > num8) ? num7 : num8);
				}
				stringFormat.Alignment = StringAlignment.Near;
				int num19 = 4;
				RectangleF rect_slidesinfo = new RectangleF(0f, num17, fS_Width - (num6 + num19), num16);
				num15 = DP_SetSlideIndicators(InItem, ref InPictureBox, ref g, inFont, rect_slidesinfo) - num19;
				num2 = fS_Top + fS_Height / 2;
				num = DisplayFontSize(num, Buffer_LS_Width, 3, 1);
				num4 = ((ShowDataDisplayTitle > 0) ? (num * 7 / 8) : num);
				if (num4 < 1)
				{
					num4 = 1;
				}
				inFont = new Font(songLyrics.FS_Font.Name, num4, songLyrics.FS_Font.Style);
				bool flag = false;
				int num20 = fS_Height;
				if ((ShowDataDisplayCopyright > 0 || InItem.Show_LicAdim) && text != "")
				{
					int num21 = 0;
					int num22 = (int)(g.MeasureString(text, inFont, 10000).Width * num11);
					if (num22 > num15 - num3)
					{
						ReduceFontToFit(g, text, ref inFont, num15 - num3, fS_Height / 2, MultiLine: true);
					}
					RectangleF rectangleF2 = new RectangleF(num3 + num19, num2, num15 - num3, fS_Height / 2 + 2);
					num21 = OutputOneLineToScreen(InItem, text, inFont, g, inColour, stringFormat.Alignment, ShowDataDisplayFontShadow, ShowDataDisplayFontOutline, (int)rectangleF2.Left, (int)rectangleF2.Top, (int)rectangleF2.Width, fS_Height / 2);
					num20 = num21 - fS_Top;
					num2 = num21 - num20;
					flag = true;
				}
				else
				{
					num2 = fS_Top;
				}
				num5 = fS_Height * 10 / 100;
				num20 -= num5;
				num2 += num5;
				inFont = new Font(songLyrics.FS_Font.Name, num * 12 / 10, songLyrics.FS_Font.Style);
				if (ShowDataDisplayTitle > 0)
				{
					string title = InItem.Title;
					int num23 = (int)(g.MeasureString(title, inFont, 10000).Width * num11);
					num20 = ((num20 > 2) ? (num20 - 2) : 2);
					if (num23 > num15)
					{
						ReduceFontToFit(g, title, ref inFont, num15 - num3, num20, (!flag) ? true : false);
					}
					if (inFont.Size >= 25f)
					{
						num2 -= 3;
					}
					else if (inFont.Size <= 21f)
					{
						num2 += 2;
					}
					RectangleF rectangleF3 = new RectangleF(num3 + num19, num2 + (flag ? 3 : (-2)), num15 - num3, num20);
					OutputOneLineToScreen(InItem, title, inFont, g, inColour, stringFormat.Alignment, ShowDataDisplayFontShadow, ShowDataDisplayFontOutline, (int)rectangleF3.Left, (int)rectangleF3.Top, (int)rectangleF3.Width, num20);
				}
				InItem.Show_LicAdim = false;
			}
			else if (InItem.Show_LicAdim)
			{
				num2 = fS_Top + fS_Height / 2;
				RectangleF rectangleF2 = new RectangleF(num3, num2, fS_Width, fS_Height / 2);
				OutputOneLineToScreen(InItem, text, inFont, g, inColour, stringFormat.Alignment, ShowDataDisplayFontShadow, ShowDataDisplayFontOutline, (int)rectangleF2.Left, (int)rectangleF2.Top, (int)rectangleF2.Width, 0);
				InItem.Show_LicAdim = false;
			}
		}

		public static int DataDisplaySlides(SongSettings InItem, ref Graphics g, Font tempFont, Color In_TextColour, RectangleF rect_slidesinfo, int VersesSymOffsetTop, int SlidesSymOffsetTop, int RectOffsetTop, int OffsetLeft, bool DisplayIndicators)
		{
			int curSlide = InItem.CurSlide;
			int totalSlides = InItem.TotalSlides;
			string text = "";
			string text2 = "";
			Color color = In_TextColour;
			StringFormat stringFormat = new StringFormat();
			int num = (totalSlides <= 10) ? totalSlides : 11;
			int num2 = (curSlide - 1) / 10 * 10 + 1;
			int num3 = (totalSlides > num2 + 9) ? (num2 + 9) : totalSlides;
			int num4 = num3 - num2 + 1;
			int num5 = OffsetLeft;
			for (int i = 1; i <= 11; i++)
			{
				text2 = "";
				text = "";
				bool flag = false;
				if (i <= num4)
				{
					int num6 = num2 + i - 1;
					text = ((num6 < 10) ? " " : "") + num6.ToString("0") + " ";
					flag = ((curSlide == num6) ? true : false);
					if (((InItem.Slide[num6, 0] >= 0) & (InItem.Slide[num6, 0] <= 10)) | ((InItem.Slide[num6, 0] >= 100) & (InItem.Slide[num6, 0] <= 112)))
					{
						text2 = " " + ConvertSequenceSymbol(SequenceSymbol[InItem.Slide[num6, 0]]);
					}
				}
				else if (i == 11 && totalSlides > num3)
				{
					text = ".. " + totalSlides.ToString("00");
				}
				int num7 = (int)g.MeasureString(text, tempFont, 10000).Width;
				if (DisplayIndicators)
				{
					if (flag)
					{
						g.DrawRectangle(new Pen(In_TextColour), OffsetLeft + 1, RectOffsetTop, num7 + 1, rect_slidesinfo.Height * 2f / 5f);
					}
					else
					{
						color = Color.White;
					}
					color = (flag ? Color.Red : In_TextColour);
					OutputOneLineToScreen(InItem, text2, tempFont, g, color, stringFormat.Alignment, ShowDataDisplayFontShadow, ShowDataDisplayFontOutline, OffsetLeft, VersesSymOffsetTop, OffsetLeft + num7, 0);
					OutputOneLineToScreen(InItem, text, tempFont, g, color, stringFormat.Alignment, ShowDataDisplayFontShadow, ShowDataDisplayFontOutline, OffsetLeft, SlidesSymOffsetTop, OffsetLeft + num7, 0);
				}
				OffsetLeft += num7;
			}
			num5 = OffsetLeft - num5;
			OffsetLeft += (int)g.MeasureString("A", tempFont, 10000).Width;
			return num5;
		}
    }
}
