using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Threading;
using System.Windows.Forms;
using Axicultus.Module;
using Axicultus.Util;

namespace Axicultus;

internal unsafe partial class Gf
{
		// Display Constants
		private const int DefaultLyricsWidth = 960;
		private const int MinFontSize = 1;
		private const float MaxLayoutHeight = 32000f;
		private const float MaxFontSize = 100f;
		private const float FontSizeReduction = 4f;
		private const int DefaultInitialFontSize = 30;
		private const float FontSizeStep = 1f;
		private const int InterlaceHeightPercentage = 90;
		private const int BufferLSHeightDivisor30 = 30;
		private const int BufferLSHeightDivisor40 = 40;
		private const int FontThresholdDivisor = 30;

		public static int DisplayFontSize(int InFontSize, int InLyricsWidth, int InNum, int Folderno)
		{
			if (InNum == 2)
			{
				try
				{
					InFontSize = InFontSize * Folders[Folderno].HeadingPercentSize / 100;
				}
				catch
				{
				}
			}
			int calculatedSize = InFontSize * InLyricsWidth / DefaultLyricsWidth;
			return (calculatedSize < MinFontSize) ? MinFontSize : calculatedSize;
		}

		public static int ReduceFontToFit(Graphics g, string InText, ref Font MainFont, int InWidth, int InHeight)
		{
			return ReduceFontToFit(g, InText, ref MainFont, InWidth, InHeight, MultiLine: false);
		}

		public static int ReduceFontToFit(Graphics g, string InText, ref Font MainFont, int InWidth, int InHeight, bool MultiLine)
		{
			SizeF layoutArea = new(InWidth, MaxLayoutHeight);
			if (MultiLine)
			{
				while ((RenderingCache.MeasureString(g, InText, MainFont, layoutArea).Height > (float)InHeight) && (MainFont.Size > MinFontSize))
				{
					MainFont = RenderingCache.GetFont(MainFont.Name, MainFont.Size - FontSizeStep, MainFont.Style);
				}
			}
			else
			{
				SizeF textSize = RenderingCache.MeasureString(g, InText, MainFont);
				while (MainFont.Size > MinFontSize && (textSize.Width > (float)InWidth || textSize.Height > (float)InHeight))
				{
					MainFont = RenderingCache.GetFont(MainFont.Name, MainFont.Size - FontSizeStep, MainFont.Style);
					textSize = RenderingCache.MeasureString(g, InText, MainFont);
				}
			}
			return (int)RenderingCache.MeasureString(g, InText, MainFont).Height;
		}

		public static int IncreaseFontToLargest(Graphics g, string InText, ref Font MainFont, int InWidth, int InHeight)
		{
			bool OnlyOneDisplayLine = false;
			return IncreaseFontToLargest(g, InText, ref MainFont, InWidth, InHeight, ref OnlyOneDisplayLine);
		}

		public static int IncreaseFontToLargest(Graphics g, string InText, ref Font MainFont, int InWidth, int InHeight, ref bool OnlyOneDisplayLine)
		{
			ReduceFontToFit(g, InText, ref MainFont, InWidth, InHeight);
			SizeF layoutArea = new(InWidth, MaxLayoutHeight);
			while ((RenderingCache.MeasureString(g, InText, MainFont, layoutArea).Height < (float)InHeight) && (MainFont.Size <= MaxFontSize))
			{
				MainFont = RenderingCache.GetFont(MainFont.Name, MainFont.Size + FontSizeStep, MainFont.Style);
			}
			MainFont = RenderingCache.GetFont(MainFont.Name, MainFont.Size - FontSizeReduction, MainFont.Style);
			int result = (int)RenderingCache.MeasureString(g, InText, MainFont, layoutArea).Height;
			if (RenderingCache.MeasureString(g, InText, MainFont).Width < (float)InWidth)
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

		private struct DisplaySettings
		{
			public int ShowNotations;
			public int ShowInterlace;
			public int ShowVerticalAlign;
			public int ItemTransition;
			public int SlideTransition;
			public int UseShadowFont;
			public int UseOutlineFont;
			public int HideDisplayPanel;
		}

		private static DisplaySettings ExtractDisplaySettings(ref SongSettings InItem)
		{
			DisplaySettings settings = new()
        {
				ShowNotations = InItem.UseDefaultFormat ? ShowNotations : InItem.Format.ShowNotations,
				ShowInterlace = InItem.UseDefaultFormat ? ShowInterlace : InItem.Format.ShowInterlace,
				ShowVerticalAlign = InItem.UseDefaultFormat ? ShowVerticalAlign : InItem.Format.ShowVerticalAlign,
				ItemTransition = InItem.UseDefaultFormat ? ShowItemTransition : InItem.Format.ShowItemTransition,
				SlideTransition = InItem.UseDefaultFormat ? ShowSlideTransition : InItem.Format.ShowSlideTransition,
				UseShadowFont = InItem.UseDefaultFormat ? UseShadowFont : InItem.Format.UseShadowFont,
				UseOutlineFont = InItem.UseDefaultFormat ? UseOutlineFont : InItem.Format.UseOutlineFont,
				HideDisplayPanel = (!InItem.UseDefaultFormat) ? InItem.Format.HideDisplayPanel : ((ShowDataDisplayMode <= 0) ? 1 : 0)
			};

			InItem.Format.ShowLyrics = (InItem.UseDefaultFormat ? ShowLyrics : InItem.Format.ShowLyrics);
			InItem.Format.ShowSongHeadings = (InItem.UseDefaultFormat ? ShowSongHeadings : InItem.Format.ShowSongHeadings);
			InItem.Format.ShowSongHeadingsAlign = (InItem.UseDefaultFormat ? ShowSongHeadingsAlign : InItem.Format.ShowSongHeadingsAlign);

			return settings;
		}

		private static void DetermineLyricsHeading(ref SongSettings InItem)
		{
			InItem.CurSlide = ((InItem.CurSlide <= 0) ? 1 : ((InItem.CurSlide > InItem.TotalSlides) ? InItem.TotalSlides : InItem.CurSlide));

			if (InItem.CurSlide <= 0)
			{
				return;
			}

			if (InItem.Slide[InItem.CurSlide, 0] < 0)
			{
				InItem.Lyrics[2].Text = string.Empty;
				return;
			}

			if (InItem.CurSlide == 1)
			{
				InItem.Lyrics[2].Text = InItem.Title;
				return;
			}

			int slideType = InItem.Slide[InItem.CurSlide, 0];
			switch (slideType)
			{
				case 0:
					InItem.Lyrics[2].Text = Folders[InItem.FolderNo].LyricsHeading[1];
					break;
				case 102:
					InItem.Lyrics[2].Text = Folders[InItem.FolderNo].LyricsHeading[1] + ((Folders[InItem.FolderNo].LyricsHeading[1] != "") ? " (2)" : "");
					break;
				case 111:
					InItem.Lyrics[2].Text = Folders[InItem.FolderNo].LyricsHeading[0];
					break;
				case 112:
					InItem.Lyrics[2].Text = Folders[InItem.FolderNo].LyricsHeading[0] + ((Folders[InItem.FolderNo].LyricsHeading[0] != "") ? " (2)" : "");
					break;
				case 100:
					InItem.Lyrics[2].Text = Folders[InItem.FolderNo].LyricsHeading[2];
					break;
				case 103:
					InItem.Lyrics[2].Text = Folders[InItem.FolderNo].LyricsHeading[2] + ((Folders[InItem.FolderNo].LyricsHeading[2] != "") ? " (2)" : "");
					break;
				case 101:
					InItem.Lyrics[2].Text = Folders[InItem.FolderNo].LyricsHeading[3];
					break;
				default:
					if (InItem.Verse2Present || (InItem.CurSlide > 1 && slideType == 1))
					{
						InItem.Lyrics[2].Text = VerseTitle[slideType];
					}
					else
					{
						InItem.Lyrics[2].Text = string.Empty;
					}
					break;
			}
		}

		private static void ApplyDisplayOverrides(ref DisplaySettings settings)
		{
			if (ShowRunning_ShowNotations == 1)
			{
				settings.ShowNotations = ((settings.ShowNotations <= 0) ? 1 : 0);
			}
			settings.ShowVerticalAlign = (settings.ShowVerticalAlign + ShowRunning_ShowVerticalAlign) % 3;
		}

		private static void ConfigureTransition(ref SongSettings InItem, ref ImageTransitionControl InPictureBox, DisplaySettings settings)
		{
			if (InItem.FirstShowing)
			{
				InPictureBox.TransitionType = (ImageTransitionControl.TransitionTypes)settings.ItemTransition;
				if (LicAdminEnforceDisplay)
				{
					InItem.Show_LicAdim = true;
				}
			}
			else
			{
				InPictureBox.TransitionType = (ImageTransitionControl.TransitionTypes)settings.SlideTransition;
			}
		}

		private static void UpdateBackground(ref SongSettings InItem, ref ImageTransitionControl InPictureBox, ImageTransitionControl.TransitionAction TransitionAction, bool RedoBackground)
		{
			if (InItem.FirstShowing || RedoBackground || ComputeTransition(InItem, ref InPictureBox, TransitionAction) != 0)
			{
				if (InItem.Format.MediaTransparent || (ShowLiveCam && InItem.AtLiveScreen))
				{
					SetTransparentBackground(InItem, ref InPictureBox);
				}
				else
				{
					SetShowBackground(InItem, ref InPictureBox, InItem.Type != "G");
				}
			}
			else if (ShowLiveCam && InItem.AtLiveScreen)
			{
				SetTransparentBackground(InItem, ref InPictureBox);
			}
		}

		public static bool ShowDBSlide(ref SongSettings InItem, ref ImageTransitionControl InPictureBox, bool DoActiveIndicator, ImageTransitionControl.TransitionAction TransitionAction, bool RedoBackground)
		{
			DisplaySettings settings = ExtractDisplaySettings(ref InItem);
			DetermineLyricsHeading(ref InItem);
			ApplyDisplayOverrides(ref settings);
			ConfigureTransition(ref InItem, ref InPictureBox, settings);
			UpdateBackground(ref InItem, ref InPictureBox, TransitionAction, RedoBackground);

			if (InItem.Type != "")
			{
				DrawText(ref InItem, ref InPictureBox, InItem.LyricsAndNotationsList, settings.UseShadowFont, settings.UseOutlineFont, settings.ShowNotations, settings.ShowInterlace, settings.ShowVerticalAlign, settings.HideDisplayPanel, TransitionAction, DoActiveIndicator, ClearAll: false);
			}
			return true;
		}

		public static void DrawText(ref SongSettings InItem, ref ImageTransitionControl PPictureBox, ref ImageTransitionControl OPictureBox, ListView LyricsAndNotationsList)
		{
			if (InItem.OutputStyleScreen)
			{
				DrawText(ref InItem, ref OPictureBox, LyricsAndNotationsList, InItem.Format.UseShadowFont, InItem.Format.UseOutlineFont, InItem.Format.ShowNotations, InItem.Format.ShowInterlace, InItem.Format.ShowVerticalAlign, InItem.Format.HideDisplayPanel, StoredSlideTransitionAction(InItem), DoActiveIndicator: false, ClearAll: false);
			}
			else
			{
				DrawText(ref InItem, ref PPictureBox, LyricsAndNotationsList, InItem.Format.UseShadowFont, InItem.Format.UseOutlineFont, InItem.Format.ShowNotations, InItem.Format.ShowInterlace, InItem.Format.ShowVerticalAlign, InItem.Format.HideDisplayPanel, StoredSlideTransitionAction(InItem), DoActiveIndicator: false, ClearAll: false);
			}
		}

		public static void DrawText(ref SongSettings InItem, ref ImageTransitionControl InPictureBox, ListView LyricsAndNotationsList, bool DoActiveIndicator, bool ClearAll)
		{
			DrawText(ref InItem, ref InPictureBox, LyricsAndNotationsList, InItem.Format.UseShadowFont, InItem.Format.UseOutlineFont, InItem.Format.ShowNotations, InItem.Format.ShowInterlace, InItem.Format.ShowVerticalAlign, InItem.Format.HideDisplayPanel, StoredSlideTransitionAction(InItem, ClearAll), DoActiveIndicator, ClearAll);
		}

		/// <summary>
		/// The action for showing the item's stored transition. The format stores transitions as TransitionTypes
		/// indexes (0 = None, 1 = Ascend, ...); they used to be cast to TransitionAction (None, AsStored,
		/// AsStoredItem, AsStoredSlide, AsFade), so Ascend, Away and Blinds Vertical were played as an old
		/// transition, the item transition and Fade. The transition that ComputeTransition will take (the item's
		/// on the first showing, else the slide's) being None keeps the None path (no transition run).
		/// clearAll: a reset (ResetPictureBox) asks for no transition by zeroing the slide transition only, so
		/// then the slide value decides, also on a first showing.
		/// </summary>
		internal static ImageTransitionControl.TransitionAction StoredSlideTransitionAction(SongSettings item, bool clearAll = false)
		{
			int stored = (item.FirstShowing && !clearAll) ? item.Format.ShowItemTransition : item.Format.ShowSlideTransition;
			return (stored == 0) ? ImageTransitionControl.TransitionAction.None : ImageTransitionControl.TransitionAction.AsStoredSlide;
		}

		/// <summary>As StoredSlideTransitionAction, for showing an item from its start: the item transition.</summary>
		internal static ImageTransitionControl.TransitionAction StoredItemTransitionAction(SongSettings item)
		{
			return (item.Format.ShowItemTransition == 0) ? ImageTransitionControl.TransitionAction.None : ImageTransitionControl.TransitionAction.AsStoredItem;
		}

		public static void DrawText(ref SongSettings InItem, ref ImageTransitionControl InPictureBox, ListView LyricsAndNotationsList, int InUseShadowFont, int InUseOutlineFont, int InShowNotations, int InShowInterlace, int InShowVerticalAlign, int InHideDisplayPanel, ImageTransitionControl.TransitionAction TransitionAction, bool DoActiveIndicator, bool ClearAll)
		{
			DrawTextOptions options = new()
        {
				UseShadowFont = InUseShadowFont,
				UseOutlineFont = InUseOutlineFont,
				ShowNotations = InShowNotations,
				ShowInterlace = InShowInterlace,
				ShowVerticalAlign = InShowVerticalAlign,
				HideDisplayPanel = InHideDisplayPanel,
				TransitionAction = TransitionAction,
				DoActiveIndicator = DoActiveIndicator,
				ClearAll = ClearAll
			};
			DrawTextCore(ref InItem, ref InPictureBox, LyricsAndNotationsList, ref options);
		}

		private struct DrawTextOptions
		{
			public int UseShadowFont;
			public int UseOutlineFont;
			public int ShowNotations;
			public int ShowInterlace;
			public int ShowVerticalAlign;
			public int HideDisplayPanel;
			public ImageTransitionControl.TransitionAction TransitionAction;
			public bool DoActiveIndicator;
			public bool ClearAll;
		}

		private static void DrawTextCore(ref SongSettings InItem, ref ImageTransitionControl InPictureBox, ListView LyricsAndNotationsList, ref DrawTextOptions options)
		{
			bool interlaceEnabled = (options.ShowInterlace == 1);
			bool liveCamOnShow = false;
			bool fitAllIntoOneScreen = FitWholeLyricPage(InItem);
			int width;
			int height;
			if (!EnsureBackgroundAndSize(ref InItem, ref InPictureBox, out width, out height))
			{
				return;
			}
			ResetLyricsLayout(ref InItem);
			EnsureImageBuffers(ref InPictureBox);
			ComputeTransition(InItem, ref InPictureBox, options.TransitionAction);
			// The transition starts from what the screen shows now: the layers shown before this draw.
			// Each screen draws into its own buffers, into the one of each pair it does not show now
			// (never the layers referred to here), so the starting layers stay intact and nothing is
			// copied. Set before drawing (also when no transition follows), so the Current layers never
			// point at the buffer being drawn. If this draw throws halfway while an earlier transition is
			// still running, that transition goes on with Current = New = the layers shown before this
			// draw: transitions that blend the text layers alone (TransitBackPictureAction None: Ascend,
			// Descend, Doors...) then blend the new text with itself, so the old text disappears at once
			// instead of fading out. The screen is never black and never shows the half-drawn buffer.
			// Setting Current after drawing would be worse: on such a failure the half-drawn buffer
			// would become the running transition's starting picture. The new layers become
			// NewTextImage / NewPanelImage only after everything has been drawn (end of this method).
			InPictureBox.CurrentTextImage = InPictureBox.NewTextImage;
			InPictureBox.CurrentPanelImage = InPictureBox.NewPanelImage;
			Image textImage;
			Graphics g = InPictureBox.GetTextDrawGraphics(width, height, out textImage);

			Image panelImage;
			Graphics panelGraphics = InPictureBox.GetPanelDrawGraphics(width, height, out panelImage);
			if (ShowRunning_UseShadowFont == 1)
			{
				options.UseShadowFont = ((options.UseShadowFont <= 0) ? 1 : 0);
			}
			if (ShowRunning_UseOutlineFont == 1)
			{
				options.UseOutlineFont = ((options.UseOutlineFont <= 0) ? 1 : 0);
			}
			if (ShowRunning_ShowInterlace == 1)
			{
				interlaceEnabled = !interlaceEnabled;
			}
			if (ShowLiveCam && InItem.AtLiveScreen)
			{
				options.ClearAll = true;
				liveCamOnShow = true;
			}
			else if (InItem.OutputStyleScreen && ShowLiveClear)
			{
				options.ClearAll = true;
			}
			if (InItem.OutputStyleScreen && ShowLiveBlack)
			{
				options.ClearAll = true;
				g.Clear(BlackScreenColour);
				panelGraphics.Clear(BlackScreenColour);
			}
			if (!options.ClearAll && InItem.Lyrics[0].Font != null)
			{
				if (((ShowDataDisplayMode == 1 && options.HideDisplayPanel == 0) || InItem.Show_LicAdim) && InItem.Type != "G")
				{
					DrawDisplayPanel(InItem, options.HideDisplayPanel, ref InPictureBox, panelGraphics);
				}
				if (InItem.Type is "D" or "B" or "T" or "I" or "W" or "M")
				{
					RenderSongRegions(ref InItem, ref InPictureBox, LyricsAndNotationsList, ref g, options.UseShadowFont, options.UseOutlineFont, options.ShowNotations, options.ShowInterlace, options.ShowVerticalAlign, interlaceEnabled, fitAllIntoOneScreen);
				}
				if ((options.ShowNotations == 1) && (InItem.Capo > 0))
				{
					DrawCapoSettings(InItem, g);
				}
			}
			bool firstShowing = InItem.FirstShowing;
			FinalizeTransitionAndGo(ref InItem, ref InPictureBox, options.ClearAll);
			InPictureBox.NewPanelImage = panelImage;
			InPictureBox.NewTextImage = textImage;
			if (firstShowing || options.DoActiveIndicator)
			{
				LoadReferenceAlert(ref InPictureBox, InItem, options.ClearAll, options.DoActiveIndicator);
			}
			InPictureBox.Go(options.TransitionAction, firstShowing, options.ClearAll, options.DoActiveIndicator, liveCamOnShow);
		}

		private static bool EnsureBackgroundAndSize(ref SongSettings InItem, ref ImageTransitionControl InPictureBox, out int width, out int height)
		{
			width = 0;
			height = 0;
			if ((InPictureBox.Width <= 0) || (InPictureBox.Height <= 0))
			{
				return false;
			}
			if (InPictureBox.NewBackgroundPicture == null)
			{
				SetShowBackground(InItem, ref InPictureBox);
			}
			width = InPictureBox.NewBackgroundPicture.Width;
			height = InPictureBox.NewBackgroundPicture.Height;
			return true;
		}

		private static void ResetLyricsLayout(ref SongSettings InItem)
		{
			for (int i = 0; i <= 2; i++)
			{
				InItem.Lyrics[i].FS_TopOffset = 0;
				InItem.Lyrics[i].FS_OneLyricAndNotationHeight = 0;
				InItem.Lyrics[i].FS_InterlaceGapHeight = 0;
				InItem.Lyrics[i].FS_InterlaceLinePattern = string.Empty;
			}
		}

		private static void EnsureImageBuffers(ref ImageTransitionControl InPictureBox)
		{
			// The copies made here are the screen's own (AdoptImage): it disposes each once none of its fields
			// refers to it, and when it is disposed. BackgroundImage stays where it is (the screen owns it too,
			// through SetDefaultBackgroundPicture); only copies are made here.
			if (InPictureBox.CurrentCombinedImage == null)
			{
				InPictureBox.CurrentBackgroundPicture = InPictureBox.AdoptImage((Image)InPictureBox.BackgroundImage.Clone());
				InPictureBox.CurrentCombinedImage = InPictureBox.AdoptImage((Image)InPictureBox.BackgroundImage.Clone());
			}
			// CurrentTextImage / CurrentPanelImage are not made here: DrawTextCore sets them to the New
			// layers right after this, before anything reads them.
			if (InPictureBox.NewTextImage == null)
			{
				InPictureBox.NewTextImage = InPictureBox.AdoptImage((Image)InPictureBox.BackgroundImage.Clone());
			}
			if (InPictureBox.NewPanelImage == null)
			{
				InPictureBox.NewPanelImage = InPictureBox.AdoptImage((Image)InPictureBox.BackgroundImage.Clone());
			}
		}

		private static void RenderSongRegions(ref SongSettings InItem, ref ImageTransitionControl InPictureBox, ListView LyricsAndNotationsList, ref Graphics g, int useShadowFont, int useOutlineFont, int showNotations, int showInterlace, int showVerticalAlign, bool interlaceEnabled, bool fitAllIntoOneScreen)
		{
			int showLyricsMode = (InItem.Format.ShowLyrics + ShowRunning_ShowLyrics) % 3;
			int showHeadings = (InItem.Format.ShowSongHeadings == 1 || (InItem.Format.ShowSongHeadings == 2 && InItem.FirstShowing)) ? 1 : 0;
			showHeadings = (showHeadings + ShowRunning_ShowSongHeadings) % 2;
			if (InItem.Type == "M")
			{
				showHeadings = 0;
			}
			InItem.CurSlideIsVerse = false;
			int slideIndex = InItem.CurSlide;
			if (InItem.Slide[slideIndex, 0] < 0)
			{
				slideIndex--;
				while (slideIndex >= 0 && InItem.Slide[slideIndex, 0] < 0)
				{
					slideIndex--;
				}
				if (slideIndex < 0)
				{
					slideIndex = 0;
				}
			}
			InItem.CurSlideIsVerse = (InItem.Slide[slideIndex, 0] is > 0 and < 99);
			bool onlyOneRegionShown = false;
			if ((showLyricsMode < 2) || (InItem.Slide[InItem.CurSlide, 1] < 0) || (InItem.Slide[InItem.CurSlide, 3] < 0))
			{
				onlyOneRegionShown = true;
				interlaceEnabled = false;
			}
			int region2Height = 0;
			int region0Height = 0;
			int region1Height = 0;
			Font mainFontHeader = RenderingCache.GetFont("Microsoft Sans Serif", DefaultInitialFontSize, FontStyle.Regular);
			Font mainFontRegion0 = RenderingCache.GetFont("Microsoft Sans Serif", DefaultInitialFontSize, FontStyle.Regular);
			Font mainFontRegion1 = RenderingCache.GetFont("Microsoft Sans Serif", DefaultInitialFontSize, FontStyle.Regular);
			Font notationsFont = RenderingCache.GetFont("Microsoft Sans Serif", DefaultInitialFontSize, FontStyle.Regular);
			Font notationsFont2 = RenderingCache.GetFont("Microsoft Sans Serif", DefaultInitialFontSize, FontStyle.Regular);
			int topOffset = 0;
			int separatorHeight = 0;
			region2Height = GetOneRegionHeight(ref InItem, ref InPictureBox, 2, LyricsAndNotationsList, ref g, useShadowFont, useOutlineFont, showNotations, onlyOneRegionShown, ref mainFontHeader, ref notationsFont, showInterlace, fitAllIntoOneScreen, UseLargestFontSize);
			if (showLyricsMode is 0 or 2)
			{
				region0Height = GetOneRegionHeight(ref InItem, ref InPictureBox, 0, LyricsAndNotationsList, ref g, useShadowFont, useOutlineFont, showNotations, onlyOneRegionShown, ref mainFontRegion0, ref notationsFont, showInterlace, fitAllIntoOneScreen, UseLargestFontSize);
			}
			if (showLyricsMode is 1 or 2)
			{
				region1Height = GetOneRegionHeight(ref InItem, ref InPictureBox, 1, LyricsAndNotationsList, ref g, useShadowFont, useOutlineFont, showNotations, onlyOneRegionShown, ref mainFontRegion1, ref notationsFont2, showInterlace, fitAllIntoOneScreen, UseLargestFontSize);
				if (onlyOneRegionShown)
				{
					InItem.Lyrics[1].FS_Top = InItem.Lyrics[0].FS_Top;
				}
			}
			switch (showVerticalAlign)
			{
				case 0:
					topOffset = ((showHeadings <= 0) ? (-region2Height) : 0);
					break;
				case 1:
					topOffset = (InItem.Lyrics[0].FS_Height - (region0Height + region1Height + separatorHeight + ((showHeadings <= 0) ? region2Height : ((InItem.Lyrics[2].Text == "") ? region2Height : 0)))) / 2;
					break;
				case 2:
					topOffset = InItem.Lyrics[0].FS_Height - (region0Height + region1Height + separatorHeight);
					break;
			}
			topOffset += ShowTopBorderSize;
			if (showLyricsMode is 0 or 2)
			{
				DrawOneRegion(ref InItem, ref InPictureBox, 0, LyricsAndNotationsList, ref g, useShadowFont, useOutlineFont, showNotations, onlyOneRegionShown, ref mainFontRegion0, ref notationsFont, topOffset, interlaceEnabled, showVerticalAlign, 0, fitAllIntoOneScreen, UseLargestFontSize);
			}
			if (showLyricsMode is 1 or 2)
			{
				DrawOneRegion(ref InItem, ref InPictureBox, 1, LyricsAndNotationsList, ref g, useShadowFont, useOutlineFont, showNotations, onlyOneRegionShown, ref mainFontRegion1, ref notationsFont2, topOffset, interlaceEnabled, showVerticalAlign, region0Height + separatorHeight, fitAllIntoOneScreen, UseLargestFontSize);
			}
			if (showHeadings > 0)
			{
				DrawOneRegion(ref InItem, ref InPictureBox, 2, LyricsAndNotationsList, ref g, useShadowFont, useOutlineFont, showNotations, onlyOneRegionShown, ref mainFontHeader, ref notationsFont2, topOffset, interlaceEnabled, showVerticalAlign, 0, fitAllIntoOneScreen, UseLargestFontSize);
			}
		}

		private static void FinalizeTransitionAndGo(ref SongSettings InItem, ref ImageTransitionControl InPictureBox, bool clearAll)
		{
			if (InItem.FirstShowing)
			{
				InPictureBox.TransitionTime = ((ShowRunning && InItem.OutputStyleScreen && !InItem.AtLiveScreen) ? 0.7f : 1.5f);
			}
			else
			{
				InPictureBox.TransitionTime = ((ShowRunning && InItem.OutputStyleScreen && !InItem.AtLiveScreen) ? 0.1f : 0.6f);
			}
			if (!clearAll)
			{
				InItem.FirstShowing = false;
			}
			// CurrentTextImage / CurrentPanelImage (the start of the transition) are taken in
			// DrawTextCore before drawing; nothing to copy here.
		}

		public static int DrawOneRegion(ref SongSettings InItem, ref ImageTransitionControl InPictureBox, int RegNum, ListView LyricsAndNotationsList, ref Graphics g, int InUseShadowFont, int InUseOutlineFont, int InShowNotations, bool OnlyOneRegionShown, ref Font MainFont, ref Font NotationsFont, int OffsetAfterAlignment, bool InterlaceOption, int InShowVerticalAlign, int Region1Height, bool FitAllIntoOneScreen, bool UseLargestFontSize)
		{
			if (InItem.LyricsAndNotationsList.Items.Count == 0)
			{
				return 0;
			}
			int HeightOffset = 0;
			int lineStartIndex = -1;
			int lineEndIndex = -1;
			string lyricsText = "";
			int regionLeft = InItem.Lyrics[RegNum].FS_Left;
			int topPosition = InItem.Lyrics[RegNum].FS_Top;
			int regionWidth = InItem.Lyrics[RegNum].FS_Width;
			int regionHeight = (InItem.Slide[0, 3] > 0 && !OnlyOneRegionShown) ? InItem.Lyrics[RegNum].FS_Height_R2Bound : InItem.Lyrics[RegNum].FS_Height;
			SizeF layoutArea = new(regionWidth, MaxLayoutHeight);
			int oneLineHeight = (int)((double)RenderingCache.MeasureString(g, "A", MainFont, layoutArea).Height * Folders[InItem.FolderNo].FontSpacingFactor[(RegNum != 0) ? 1 : 0]);
			int notationsLineHeight = (int)((double)oneLineHeight * ((MainFont.Size >= 2f) ? NotationFontFactor : 1.0));
			int notationsLineTextVOffset = 0;
			string interlaceLinePattern = (RegNum == 1) ? InItem.Lyrics[RegNum].FS_InterlaceLinePattern : string.Empty;
			int linesRequired = 0;
			switch (RegNum)
			{
				case 0:
					lineStartIndex = InItem.Slide[InItem.CurSlide, 1];
					lineEndIndex = InItem.Slide[InItem.CurSlide, 2];
					break;
				case 1:
					lineStartIndex = InItem.Slide[InItem.CurSlide, 3];
					lineEndIndex = InItem.Slide[InItem.CurSlide, 4];
					break;
				case 2:
					lyricsText = InItem.Lyrics[RegNum].Text;
					topPosition += OffsetAfterAlignment;
					DrawOneLine(rect_normal: new RectangleF(regionLeft, topPosition + InItem.Lyrics[RegNum].FS_TopOffset, regionWidth, regionHeight), InItem: ref InItem, InPictureBox: ref InPictureBox, InLyrics: InItem.Lyrics[2], RegionNumber: 2, Slide: InItem.Slide, LyricsAndNotationsList: LyricsAndNotationsList, g: ref g, MainFont: MainFont, NotationsFont: NotationsFont, OneLineHeight: oneLineHeight, NotationsLineHeight: 0, NotationsLineTextVOffset: 0, InHeight: regionHeight, LyricsText: lyricsText, InUseShadowFont: InUseShadowFont, InUseOutlineFont: InUseOutlineFont, InShowNotations: 0);
					return 0;
			}
			if ((lineStartIndex < 0) || (lineEndIndex < 0))
			{
				return 0;
			}
			if (OnlyOneRegionShown)
			{
				topPosition += OffsetAfterAlignment;
			}
			else
			{
				switch (RegNum)
				{
					case 0:
						topPosition += OffsetAfterAlignment;
						if (InterlaceOption)
						{
							InItem.Lyrics[0].FS_InterlaceGapHeight = InItem.Lyrics[1].FS_OneLyricAndNotationHeight;
						}
						break;
					case 1:
						if (InterlaceOption)
						{
							topPosition = InItem.Lyrics[0].FS_Top + OffsetAfterAlignment + (int)((double)InItem.Lyrics[0].FS_OneLyricAndNotationHeight * InterlaceHeightPercentage / 100.0);
							InItem.Lyrics[1].FS_InterlaceGapHeight = InItem.Lyrics[0].FS_OneLyricAndNotationHeight;
							break;
						}
						topPosition = InItem.Lyrics[0].FS_Top + OffsetAfterAlignment + Region1Height + Buffer_LS_Height / BufferLSHeightDivisor30;
						if (LineBetweenRegions)
						{
							OutputOneLineToScreen(InItem, "<<DrawLine>>", MainFont, g, Color.FromArgb(InItem.Lyrics[RegNum].ForeColour), StringAlignment.Center, InUseShadowFont, InUseOutlineFont, regionLeft, topPosition - Buffer_LS_Height / BufferLSHeightDivisor40, regionWidth, 0);
						}
						break;
				}
			}
			InItem.Lyrics[RegNum].FS_TopOffset = topPosition;
			RectangleF regionRect = new(regionLeft, topPosition, regionWidth, regionHeight);
			if (lineStartIndex <= lineEndIndex)
			{
				for (int i = lineStartIndex; i <= lineEndIndex; i++)
				{
					linesRequired = 0;
					DrawOneLine(ref InItem, ref InPictureBox, InItem.Lyrics[RegNum], RegNum, InItem.Slide, LyricsAndNotationsList, ref g, MainFont, NotationsFont, oneLineHeight, notationsLineHeight, notationsLineTextVOffset, regionRect, regionHeight, lyricsText, InUseShadowFont, InUseOutlineFont, InShowNotations, i, ref HeightOffset, ref linesRequired, InterlaceOption, interlaceLinePattern);
					if (RegNum == 0)
					{
						SongLyrics region1Lyrics = InItem.Lyrics[1];
						region1Lyrics.FS_InterlaceLinePattern = region1Lyrics.FS_InterlaceLinePattern + Convert.ToString(linesRequired) + '>';
					}
				}
			}
			return (int)MainFont.Size;
		}

		/// <summary>
		/// Draws one line of lyrics - Simplified overload without line tracking.
		/// </summary>
		public static void DrawOneLine(ref SongSettings InItem, ref ImageTransitionControl InPictureBox, SongLyrics InLyrics, int RegionNumber, int[,] Slide, ListView LyricsAndNotationsList, ref Graphics g, Font MainFont, Font NotationsFont, int OneLineHeight, int NotationsLineHeight, int NotationsLineTextVOffset, RectangleF rect_normal, int InHeight, string LyricsText, int InUseShadowFont, int InUseOutlineFont, int InShowNotations)
		{
			int heightOffset = 0;
			int linesRequired = 0;
			DrawOneLine(ref InItem, ref InPictureBox, InLyrics, RegionNumber, Slide, LyricsAndNotationsList, ref g, MainFont, NotationsFont, OneLineHeight, NotationsLineHeight, NotationsLineTextVOffset, rect_normal, InHeight, LyricsText, InUseShadowFont, InUseOutlineFont, InShowNotations, -1, ref heightOffset, ref linesRequired, false, string.Empty);
		}

		/// <summary>
		/// Draws one line of lyrics - Overload with current line index.
		/// </summary>
		public static void DrawOneLine(ref SongSettings InItem, ref ImageTransitionControl InPictureBox, SongLyrics InLyrics, int RegionNumber, int[,] Slide, ListView LyricsAndNotationsList, ref Graphics g, Font MainFont, Font NotationsFont, int OneLineHeight, int NotationsLineHeight, int NotationsLineTextVOffset, RectangleF rect_normal, int InHeight, string LyricsText, int InUseShadowFont, int InUseOutlineFont, int InShowNotations, int CurLine)
		{
			int heightOffset = 0;
			int linesRequired = 0;
			DrawOneLine(ref InItem, ref InPictureBox, InLyrics, RegionNumber, Slide, LyricsAndNotationsList, ref g, MainFont, NotationsFont, OneLineHeight, NotationsLineHeight, NotationsLineTextVOffset, rect_normal, InHeight, LyricsText, InUseShadowFont, InUseOutlineFont, InShowNotations, CurLine, ref heightOffset, ref linesRequired, false, string.Empty);
		}

		/// <summary>
		/// Draws one line of lyrics - Overload with height offset tracking.
		/// </summary>
		public static void DrawOneLine(ref SongSettings InItem, ref ImageTransitionControl InPictureBox, SongLyrics InLyrics, int RegionNumber, int[,] Slide, ListView LyricsAndNotationsList, ref Graphics g, Font MainFont, Font NotationsFont, int OneLineHeight, int NotationsLineHeight, int NotationsLineTextVOffset, RectangleF rect_normal, int InHeight, string LyricsText, int InUseShadowFont, int InUseOutlineFont, int InShowNotations, int CurLine, ref int HeightOffset)
		{
			int linesRequired = 0;
			DrawOneLine(ref InItem, ref InPictureBox, InLyrics, RegionNumber, Slide, LyricsAndNotationsList, ref g, MainFont, NotationsFont, OneLineHeight, NotationsLineHeight, NotationsLineTextVOffset, rect_normal, InHeight, LyricsText, InUseShadowFont, InUseOutlineFont, InShowNotations, CurLine, ref HeightOffset, ref linesRequired, false, string.Empty);
		}

		/// <summary>
		/// Draws one line of lyrics - Overload with height and lines tracking.
		/// </summary>
		public static void DrawOneLine(ref SongSettings InItem, ref ImageTransitionControl InPictureBox, SongLyrics InLyrics, int RegionNumber, int[,] Slide, ListView LyricsAndNotationsList, ref Graphics g, Font MainFont, Font NotationsFont, int OneLineHeight, int NotationsLineHeight, int NotationsLineTextVOffset, RectangleF rect_normal, int InHeight, string LyricsText, int InUseShadowFont, int InUseOutlineFont, int InShowNotations, int CurLine, ref int HeightOffset, ref int LinesRequired)
		{
			DrawOneLine(ref InItem, ref InPictureBox, InLyrics, RegionNumber, Slide, LyricsAndNotationsList, ref g, MainFont, NotationsFont, OneLineHeight, NotationsLineHeight, NotationsLineTextVOffset, rect_normal, InHeight, LyricsText, InUseShadowFont, InUseOutlineFont, InShowNotations, CurLine, ref HeightOffset, ref LinesRequired, false, string.Empty);
		}

		/// <summary>
		/// Draws one line of lyrics - Overload with interlace option support.
		/// </summary>
		public static void DrawOneLine(ref SongSettings InItem, ref ImageTransitionControl InPictureBox, SongLyrics InLyrics, int RegionNumber, int[,] Slide, ListView LyricsAndNotationsList, ref Graphics g, Font MainFont, Font NotationsFont, int OneLineHeight, int NotationsLineHeight, int NotationsLineTextVOffset, RectangleF rect_normal, int InHeight, string LyricsText, int InUseShadowFont, int InUseOutlineFont, int InShowNotations, int CurLine, ref int HeightOffset, ref int LinesRequired, bool InterlaceOption)
		{
			DrawOneLine(ref InItem, ref InPictureBox, InLyrics, RegionNumber, Slide, LyricsAndNotationsList, ref g, MainFont, NotationsFont, OneLineHeight, NotationsLineHeight, NotationsLineTextVOffset, rect_normal, InHeight, LyricsText, InUseShadowFont, InUseOutlineFont, InShowNotations, CurLine, ref HeightOffset, ref LinesRequired, InterlaceOption, string.Empty);
		}

		/// <summary>
		/// Draws one line of lyrics with full parameter control - Complete implementation.
		/// All other DrawOneLine overloads delegate to this method for actual rendering.
		/// Handles line wrapping, notations, interlace mode, and height tracking.
		/// </summary>
		public static void DrawOneLine(ref SongSettings InItem, ref ImageTransitionControl InPictureBox, SongLyrics InLyrics, int RegionNumber, int[,] Slide, ListView LyricsAndNotationsList, ref Graphics g, Font MainFont, Font NotationsFont, int OneLineHeight, int NotationsLineHeight, int NotationsLineTextVOffset, RectangleF rect_normal, int InHeight, string LyricsText, int InUseShadowFont, int InUseOutlineFont, int InShowNotations, int CurLine, ref int HeightOffset, ref int LinesRequired, bool InterlaceOption, string InterlaceLinePattern)
		{
			int startPos = 0;
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
				ListViewItem item = InItem.LyricsAndNotationsList.Items[CurLine];
				LyricsText = item.SubItems[2].Text;
				notationsString = item.SubItems[3].Text;
				InString = item.SubItems[4].Text;
			}
			if (InterlaceOption && RegionNumber == 1)
			{
				while ((RenderingCache.MeasureString(g, LyricsText, MainFont, 100000).Width + 10f > rect_normal.Width * (float)R2_MaxLinesPermitted) && (MainFont.Size > 1f))
				{
					MainFont = RenderingCache.GetFont(InLyrics.Font.Name, MainFont.Size - 1f, InLyrics.Font.Style);
					NotationsFont = RenderingCache.GetFont(InLyrics.Font.Name, Convert.ToInt32((double)MainFont.Size * NotationFontFactor), InLyrics.Font.Style);
				}
			}
			LinesRequired++;
			string ReplacedLog = "";
			if (UseLargestFontSize)
			{
				InString = "";
				ReplacedLog = ActionWordWrapSpacesAtStart(ref LyricsText);
			}
			int setLength = Convert.ToInt32(DataUtil.ExtractOneInfo(ref InString, '>'));
			if (setLength > 0)
			{
				if (setLength == 1)
				{
					setLength = 0;
				}
				else
				{
					DataUtil.ExtractOneInfo(ref InString, '>');
					setLength = Convert.ToInt32(DataUtil.ExtractOneInfo(ref InString, '>'));
				}
			}
			string remainingText = DisplayTextByWidthOneLine(InItem, LyricsText, InLyrics, RegionNumber, MainFont, NotationsFont, g, (int)rect_normal.Left, (int)rect_normal.Top, (int)rect_normal.Width, ref HeightOffset, OneLineHeight, NotationsLineHeight, NotationsLineTextVOffset, startPos, InUseShadowFont, InUseOutlineFont, InShowNotations, notationsString, ref EndExtractedTextPos, ref R2_MaxLinesPermitted, InterlaceOption, setLength, IsWrappedText: false, ref ReplacedLog);
			if (setLength > 0)
			{
				setLength = Convert.ToInt32(DataUtil.ExtractOneInfo(ref InString, '>'));
			}
			while (remainingText != "" && EndExtractedTextPos > 0)
			{
				startPos = EndExtractedTextPos + 1;
				LinesRequired++;
				remainingText = DisplayTextByWidthOneLine(InItem, LyricsText, InLyrics, RegionNumber, MainFont, NotationsFont, g, (int)rect_normal.Left, (int)rect_normal.Top, (int)rect_normal.Width, ref HeightOffset, OneLineHeight, NotationsLineHeight, NotationsLineTextVOffset, startPos, InUseShadowFont, InUseOutlineFont, InShowNotations, notationsString, ref EndExtractedTextPos, ref R2_MaxLinesPermitted, InterlaceOption, setLength, IsWrappedText: true, ref ReplacedLog);
				if (setLength > 0)
				{
					setLength = Convert.ToInt32(DataUtil.ExtractOneInfo(ref InString, '>'));
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
			R2_MaxLinesPermitted--;
			SubDivideOneOutputText(InText, MainFont, NotationsFont, g, InWidth, InShowNotations, NotationsString, length, InSetLength, StartPos, ref EndExtractedTextPos, ref ExtractedText);
			HeightOffset += ((InShowNotations == 1) ? NotationsLineHeight : 0);
			ActionUndoWordWrapSpacesAtStart(ref ExtractedText, ref ReplacedLog);
			SubstituteDashes(ref ExtractedText, InShowNotations);
			int textLeft = OutputOneLineToScreen(InItem, ExtractedText, MainFont, g, Color.FromArgb(InLyrics.ForeColour), InLyrics.TextAlign, InUseShadowFont, InUseOutlineFont, InLeft, InTop + HeightOffset, InWidth, 0, IsWrappedText && WordWrapLeftAlignIndent && InItem.Type != "I" && RegionNumber != 2);
			if (InShowNotations == 1)
			{
				int measureFontSize = (int)MainFont.Size - 2;
				if (measureFontSize < 1)
				{
					measureFontSize = 1;
				}
				Font measureFont = RenderingCache.GetFont(MainFont.Name, measureFontSize, MainFont.Style);
				HeightOffset -= NotationsLineHeight;
				int notationsShift = 0;
				while (NotationsString != "")
				{
					string notation = DataUtil.ExtractOneInfo(ref NotationsString, ';');
					int notationPos = Convert.ToInt32(DataUtil.ExtractOneInfo(ref NotationsString, ';'));
					if (notation == "-1" || notationPos < 0 || notationPos < StartPos)
					{
						continue;
					}
					for (int i = StartPos; i <= EndExtractedTextPos; i++)
					{
						if (i == notationPos && (i != EndExtractedTextPos || EndExtractedTextPos == length))
						{
							int iLen = i - StartPos;
							string textBeforeNotation = DataUtil.Mid(InText, StartPos, iLen);
							int textBeforeWidth = (int)RenderingCache.MeasureString(g, textBeforeNotation, measureFont).Width;
							OutputOneLineToScreen(InItem, notation, NotationsFont, g, Color.FromArgb(InLyrics.ForeColour), StringAlignment.Near, InUseShadowFont, InUseOutlineFont, textLeft + textBeforeWidth + notationsShift, InTop + HeightOffset + NotationsLineTextVOffset, InWidth, 0, IsWrappedText && WordWrapLeftAlignIndent && InItem.Type != "I");
							notationsShift += (int)((i == EndExtractedTextPos) ? RenderingCache.MeasureString(g, notation + "S", NotationsFont).Width : 0f);
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
			float outlineWidth = InFont.Size / (float)AdjustedOutlineThreshold;
			string superScript = "";
			if (InItem.Type == "B" && ExtractedText.Contains('\u0098'))
			{
				string[] parts = ExtractedText.Split('\u0098');
				if (parts.GetUpperBound(0) > 0)
				{
					superScript = DataUtil.Trim(parts[0]);
					ExtractedText = DataUtil.Trim(parts[1]);
				}
			}
			if (outlineWidth < 1f)
			{
				outlineWidth = 1f;
			}
			int emSize = (int)((double)InFont.Size * 1.2999999523162842);
			int lineLeft = x;
			Pen pen = RenderingCache.GetPen(BlackScreenColour, outlineWidth);
			SolidBrush shadowBrush = RenderingCache.GetBrush(BlackScreenColour);
			SolidBrush textBrush = RenderingCache.GetBrush(InColour);
			StringFormat stringFormat;
			switch (alignformat)
			{
				case StringAlignment.Center:
					stringFormat = StringFormatCenter;
					x += w / 2;
					break;
				case StringAlignment.Far:
					stringFormat = StringFormatFar;
					x += w;
					break;
				default:
					stringFormat = StringFormatNear;
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
			// One FontFamily for every string of this line (made when first needed), disposed when the method
			// ends. GraphicsPath.AddString copies the outlines, so nothing refers to it afterwards.
			using LineFontFamily lineFamily = new(InFont.Name);
        using GraphicsPath graphicsPath = new();
        using GraphicsPath measurePath = new();
        g.SmoothingMode = SmoothingMode.AntiAlias;
        int superScriptWidth = 0;
        int pathLeft = 0;
        int lineThickness = emSize / 10;
        if (lineThickness < 1)
        {
            lineThickness = 1;
        }
        int textTop = y;
        if (h > 0)
        {
            measurePath.AddString(ExtractedText, lineFamily.Family, (int)InFont.Style, emSize, new Rectangle(superScriptWidth + x, y, w, h), stringFormat);
            int boundsHeight = (int)measurePath.GetBounds().Height + 10;
            textTop += h - boundsHeight;
        }
        if (InUseShadowFont > 0)
        {
            int shadowOffset = (int)(InFont.Size / 22f) + 1;
            if (superScript != "" && alignformat == StringAlignment.Near)
            {
                superScriptWidth = DrawSuperScript(graphicsPath, superScript, InFont, emSize, new Rectangle(x + shadowOffset, y + shadowOffset, (h != 0) ? w : 0, h), AlignLeft: true);
                g.FillPath(shadowBrush, graphicsPath);
                graphicsPath.Reset();
            }
            if (ExtractedText == "<<DrawLine>>")
            {
                graphicsPath.AddRectangle(new Rectangle(lineLeft, y, w, lineThickness));
            }
            else if (h == 0)
            {
                graphicsPath.AddString(ExtractedText, lineFamily.Family, (int)InFont.Style, emSize, new Point(superScriptWidth + x + shadowOffset, y + shadowOffset), stringFormat);
            }
            else
            {
                graphicsPath.AddString(ExtractedText, lineFamily.Family, (int)InFont.Style, emSize, new Rectangle(superScriptWidth + x + shadowOffset, textTop + shadowOffset, w, h), stringFormat);
            }
            g.FillPath(shadowBrush, graphicsPath);
            pathLeft = (int)graphicsPath.GetBounds().Left;
            graphicsPath.Reset();
            if (superScript != "" && alignformat != 0)
            {
                DrawSuperScript(graphicsPath, superScript, InFont, emSize, new Rectangle(pathLeft, y + shadowOffset, (h != 0) ? w : 0, h), AlignLeft: false);
                g.FillPath(shadowBrush, graphicsPath);
                graphicsPath.Reset();
            }
        }
        if (superScript != "" && alignformat == StringAlignment.Near)
        {
            superScriptWidth = DrawSuperScript(graphicsPath, superScript, InFont, emSize, new Rectangle(x, y, (h != 0) ? w : 0, h), AlignLeft: true);
            g.FillPath(textBrush, graphicsPath);
            if (InUseOutlineFont > 0)
            {
                g.DrawPath(pen, graphicsPath);
            }
            graphicsPath.Reset();
        }
        if (ExtractedText == "<<DrawLine>>")
        {
            graphicsPath.AddRectangle(new Rectangle(lineLeft, y, w, lineThickness));
        }
        else if (h == 0)
        {
            graphicsPath.AddString(ExtractedText, lineFamily.Family, (int)InFont.Style, emSize, new Point(superScriptWidth + x, y), stringFormat);
        }
        else
        {
            graphicsPath.AddString(ExtractedText, lineFamily.Family, (int)InFont.Style, emSize, new Rectangle(superScriptWidth + x, textTop, w, h), stringFormat);
        }
        g.FillPath(textBrush, graphicsPath);
        if (InUseOutlineFont > 0)
        {
            g.DrawPath(pen, graphicsPath);
        }
        pathLeft = (int)graphicsPath.GetBounds().Left;
        graphicsPath.Reset();
        if (superScript != "" && alignformat != 0)
        {
            DrawSuperScript(graphicsPath, superScript, InFont, emSize, new Rectangle(pathLeft, y, (h != 0) ? w : 0, h), AlignLeft: false);
            g.FillPath(textBrush, graphicsPath);
            if (InUseOutlineFont > 0)
            {
                g.DrawPath(pen, graphicsPath);
            }
            graphicsPath.Reset();
        }
        if (h == 0)
        {
            return pathLeft;
        }
        return textTop;
    }

		public static int DrawSuperScript(GraphicsPath pth, string InText, Font InFont, int InFontSize, Rectangle InRectangle, bool AlignLeft)
		{
			StringFormat stringFormat = AlignLeft ? StringFormatNear : StringFormatFar;
			InFontSize = InFontSize * 7 / 10;
			using (FontFamily fontFamily = new(InFont.Name))
			{
				if (InRectangle.Height == 0)
				{
					pth.AddString(InText, fontFamily, (int)InFont.Style, InFontSize, new Point(InRectangle.Left, InRectangle.Top), stringFormat);
				}
				else
				{
					pth.AddString(InText, fontFamily, (int)InFont.Style, InFontSize, InRectangle, stringFormat);
				}
			}
			int pathWidth = (int)pth.GetBounds().Width;
			int gap = pathWidth / InText.Length * 2 / 3;
			return pathWidth + gap;
		}

		/// <summary>
		/// The FontFamily of one OutputOneLineToScreen call: made on first use (a line may need none, e.g.
		/// a drawn line), shared by every AddString of the call, disposed with the call.
		/// </summary>
		private sealed class LineFontFamily(string name) : IDisposable
		{
			private readonly string name = name;
			private FontFamily family;

        public FontFamily Family => family ??= new FontFamily(name);

			public void Dispose()
			{
				family?.Dispose();
				family = null;
			}
		}

		public static void DrawCapoSettings(SongSettings InItem, Graphics g)
		{
			SongLyrics songLyrics = InItem.Lyrics[3];
			int fontSize = (int)songLyrics.FS_Font.Size;
			int capoLeft = songLyrics.FS_Left;
			int capoTop = songLyrics.FS_Top;
			int capoWidth = songLyrics.FS_Width;
			int capoHeight = songLyrics.FS_Height;
			Font font = RenderingCache.GetFont(songLyrics.FS_Font.Name, Convert.ToInt32(fontSize), songLyrics.FS_Font.Style);
			SolidBrush brush = RenderingCache.GetBrush(Color.FromArgb(songLyrics.ForeColour));
			int textTop = capoTop + capoHeight / 2;
			g.DrawString(layoutRectangle: new RectangleF(capoLeft, textTop, capoWidth, capoHeight), s: "Capo " + Convert.ToString(InItem.Capo), font: font, brush: brush, format: StringFormatFar);
		}

		private const int DisplayPanelMeasureMaxWidth = 10000;
		private const float DisplayPanelMeasureScale = 1.18f;
		private const int DisplayPanelFontCacheSize = 64;
		private const int DisplayPanelMeasureCacheSize = 512;
		private const int DisplayPanelTextCacheSize = 64;

		private static readonly StringFormat DisplayPanelFormatNear = new()
    {
			Alignment = StringAlignment.Near
		};

		private static readonly StringFormat DisplayPanelFormatFar = new()
    {
			Alignment = StringAlignment.Far
		};

		private static readonly StringFormat StringFormatNear = new()
    {
			Alignment = StringAlignment.Near
		};

		private static readonly StringFormat StringFormatCenter = new()
    {
			Alignment = StringAlignment.Center
		};

		private static readonly StringFormat StringFormatFar = new()
    {
			Alignment = StringAlignment.Far
		};

		private static class RenderingCache
		{
			private const int GlobalFontCacheSize = 128;
			private const int GlobalBrushCacheSize = 64;
			private const int GlobalMeasureCacheSize = 1024;
			private const int GlobalPenCacheSize = 32;

			public static readonly LruCache<FontKey, Font> Fonts = new(GlobalFontCacheSize, null);
			public static readonly Dictionary<int, SolidBrush> Brushes = [];
			public static readonly LruCache<MeasureKey, SizeF> Measures = new(GlobalMeasureCacheSize, null);
			public static readonly Dictionary<PenKey, Pen> Pens = [];
			private static readonly Lock _brushLock = new();
			private static readonly Lock _penLock = new();

			public static SolidBrush GetBrush(Color color)
			{
				int key = color.ToArgb();
				lock (_brushLock)
				{
					if (!Brushes.TryGetValue(key, out SolidBrush brush))
					{
						brush = new SolidBrush(color);
						Brushes[key] = brush;
					}
					return brush;
				}
			}

			public static Font GetFont(string name, float size, FontStyle style)
			{
				if (size < 1f)
				{
					size = 1f;
				}
				FontKey key = new(name, (int)size, style);
				return Fonts.GetOrAdd(key, _ => new Font(name, size, style));
			}

			public static Pen GetPen(Color color, float width)
			{
				PenKey key = new(color.ToArgb(), width);
				lock (_penLock)
				{
					if (!Pens.TryGetValue(key, out Pen pen))
					{
						pen = new Pen(color, width);
						Pens[key] = pen;
					}
					return pen;
				}
			}

			public static SizeF MeasureString(Graphics g, string text, Font font, int maxWidth = 10000)
			{
				if (string.IsNullOrEmpty(text) || g == null || font == null)
				{
					return SizeF.Empty;
				}
				if (maxWidth <= 0)
				{
					return SafeMeasureString(g, text, font);
				}
				MeasureKey key = new(FontKey.FromFont(font), text, maxWidth, 1.0f);
				return Measures.GetOrAdd(key, _ => SafeMeasureString(g, text, font, maxWidth));
			}

			public static SizeF MeasureString(Graphics g, string text, Font font, SizeF layoutArea)
			{
				if (string.IsNullOrEmpty(text) || g == null || font == null)
				{
					return SizeF.Empty;
				}
				if (layoutArea.Width <= 0f || layoutArea.Height <= 0f
					|| float.IsNaN(layoutArea.Width) || float.IsNaN(layoutArea.Height)
					|| float.IsInfinity(layoutArea.Width) || float.IsInfinity(layoutArea.Height))
				{
					return SafeMeasureString(g, text, font);
				}
				MeasureKey key = new(FontKey.FromFont(font), text, (int)layoutArea.Width, layoutArea.Height);
				return Measures.GetOrAdd(key, _ => SafeMeasureString(g, text, font, layoutArea));
			}

			private static SizeF SafeMeasureString(Graphics g, string text, Font font)
			{
				try
				{
					if (font.Size <= 0f)
					{
						font = new Font(font.FontFamily, 1f, font.Style);
					}
					return g.MeasureString(text, font);
				}
				catch (ArgumentException)
				{
					return SizeF.Empty;
				}
				catch (ObjectDisposedException)
				{
					return SizeF.Empty;
				}
			}

			private static SizeF SafeMeasureString(Graphics g, string text, Font font, int maxWidth)
			{
				try
				{
					if (font.Size <= 0f)
					{
						font = new Font(font.FontFamily, 1f, font.Style);
					}
					return g.MeasureString(text, font, maxWidth);
				}
				catch (ArgumentException)
				{
					return SafeMeasureString(g, text, font);
				}
				catch (ObjectDisposedException)
				{
					return SizeF.Empty;
				}
			}

			private static SizeF SafeMeasureString(Graphics g, string text, Font font, SizeF layoutArea)
			{
				try
				{
					if (font.Size <= 0f)
					{
						font = new Font(font.FontFamily, 1f, font.Style);
					}
					return g.MeasureString(text, font, layoutArea);
				}
				catch (ArgumentException)
				{
					return SafeMeasureString(g, text, font);
				}
				catch (ObjectDisposedException)
				{
					return SizeF.Empty;
				}
			}
		}

		private struct PenKey(int colorArgb, float width) : IEquatable<PenKey>
		{
			public readonly int ColorArgb = colorArgb;
			public readonly int WidthKey = (int)(width * 100f);

        public bool Equals(PenKey other)
			{
				return ColorArgb == other.ColorArgb && WidthKey == other.WidthKey;
			}

			public override bool Equals(object obj)
			{
				return obj is PenKey other && Equals(other);
			}

			public override int GetHashCode()
			{
				unchecked
				{
					return (ColorArgb * 397) ^ WidthKey;
				}
			}
		}

		private static readonly Dictionary<int, SolidBrush> DisplayPanelBrushCache = [];
		private static readonly LruCache<FontKey, Font> DisplayPanelFontCache = new(DisplayPanelFontCacheSize, null);
		private static readonly LruCache<MeasureKey, int> DisplayPanelMeasureCache = new(DisplayPanelMeasureCacheSize, null);
		private static readonly LruCache<TextCompositeKey, string> DisplayPanelTextCompositeCache = new(DisplayPanelTextCacheSize, null);

		private static SolidBrush GetCachedBrush(Color color)
		{
			int key = color.ToArgb();
			lock (DisplayPanelBrushCache)
			{
				if (DisplayPanelBrushCache.TryGetValue(key, out SolidBrush brush))
				{
					return brush;
				}
				brush = new SolidBrush(color);
				DisplayPanelBrushCache[key] = brush;
				return brush;
			}
		}

		private static Font GetCachedFont(string name, int size, FontStyle style)
		{
			if (size < 1)
			{
				size = 1;
			}
			FontKey key = new(name, size, style);
			return DisplayPanelFontCache.GetOrAdd(key, _ => new Font(name, size, style));
		}

		private static int MeasureStringWidthCached(Graphics g, string text, Font font, int maxWidth, float scale)
		{
			if (string.IsNullOrEmpty(text))
			{
				return 0;
			}
			MeasureKey key = new(FontKey.FromFont(font), text, maxWidth, scale);
			return DisplayPanelMeasureCache.GetOrAdd(key, _ => (int)(g.MeasureString(text, font, maxWidth).Width * scale));
		}

		private static string BuildLicenseTextCached(SongSettings item)
		{
			TextCompositeKey key = new(item.Writer, item.Copyright, item.Show_LicAdminInfo1, item.Show_LicAdminInfo2);
			return DisplayPanelTextCompositeCache.GetOrAdd(key, _ =>
			{
				string writer = item.Writer ?? "";
				string copyright = item.Copyright ?? "";
				string lic1 = item.Show_LicAdminInfo1 ?? "";
				string lic2 = item.Show_LicAdminInfo2 ?? "";
				string writerAndCopyright = writer + (((writer != "") && (copyright != "")) ? "; " : "") + copyright;
				string baseText = writerAndCopyright;
				return baseText + ((baseText == "") ? "" : " ") + lic1 + ((lic1 == "") ? "" : " ") + lic2;
			});
		}

		private static string TrimTextToWidth(Graphics g, string text, Font font, int maxWidth, float scale)
		{
			if (string.IsNullOrEmpty(text) || maxWidth <= 0)
			{
				return "";
			}
			int fullWidth = MeasureStringWidthCached(g, text, font, DisplayPanelMeasureMaxWidth, scale);
			if (fullWidth <= maxWidth)
			{
				return text;
			}
			float ratio = (float)maxWidth / (float)fullWidth;
			int targetLength = (int)(ratio * (float)(text.Length - 3));
			if (targetLength < 0)
			{
				targetLength = 0;
			}
			return DataUtil.Left(text, targetLength) + "...";
		}

		private struct FontKey(string name, int size, FontStyle style) : IEquatable<FontKey>
		{
			public readonly string Name = name ?? "";
			public readonly int Size = size;
			public readonly FontStyle Style = style;

        public static FontKey FromFont(Font font)
			{
				return new FontKey(font.Name, (int)font.Size, font.Style);
			}

			public bool Equals(FontKey other)
			{
				return Size == other.Size && Style == other.Style && string.Equals(Name, other.Name, StringComparison.Ordinal);
			}

			public override bool Equals(object obj)
			{
				return obj is FontKey other && Equals(other);
			}

			public override int GetHashCode()
			{
				unchecked
				{
					int hash = 17;
					hash = hash * 31 + StringComparer.Ordinal.GetHashCode(Name);
					hash = hash * 31 + Size;
					hash = hash * 31 + (int)Style;
					return hash;
				}
			}
		}

		private struct MeasureKey(Gf.FontKey fontKey, string text, int maxWidth, float scale) : IEquatable<MeasureKey>
		{
			public readonly FontKey FontKey = fontKey;
			public readonly string Text = text ?? "";
			public readonly int MaxWidth = maxWidth;
			public readonly int MaxHeight = (int)(scale * 1000f);

        public bool Equals(MeasureKey other)
			{
				return MaxWidth == other.MaxWidth
					&& MaxHeight == other.MaxHeight
					&& FontKey.Equals(other.FontKey)
					&& string.Equals(Text, other.Text, StringComparison.Ordinal);
			}

			public override bool Equals(object obj)
			{
				return obj is MeasureKey other && Equals(other);
			}

			public override int GetHashCode()
			{
				unchecked
				{
					int hash = 17;
					hash = hash * 31 + FontKey.GetHashCode();
					hash = hash * 31 + StringComparer.Ordinal.GetHashCode(Text);
					hash = hash * 31 + MaxWidth;
					hash = hash * 31 + MaxHeight;
					return hash;
				}
			}
		}

		private struct TextCompositeKey(string writer, string copyright, string lic1, string lic2) : IEquatable<TextCompositeKey>
		{
			public readonly string Writer = writer ?? "";
			public readonly string Copyright = copyright ?? "";
			public readonly string Lic1 = lic1 ?? "";
			public readonly string Lic2 = lic2 ?? "";

        public bool Equals(TextCompositeKey other)
			{
				return string.Equals(Writer, other.Writer, StringComparison.Ordinal)
					&& string.Equals(Copyright, other.Copyright, StringComparison.Ordinal)
					&& string.Equals(Lic1, other.Lic1, StringComparison.Ordinal)
					&& string.Equals(Lic2, other.Lic2, StringComparison.Ordinal);
			}

			public override bool Equals(object obj)
			{
				return obj is TextCompositeKey other && Equals(other);
			}

			public override int GetHashCode()
			{
				unchecked
				{
					int hash = 17;
					hash = hash * 31 + StringComparer.Ordinal.GetHashCode(Writer);
					hash = hash * 31 + StringComparer.Ordinal.GetHashCode(Copyright);
					hash = hash * 31 + StringComparer.Ordinal.GetHashCode(Lic1);
					hash = hash * 31 + StringComparer.Ordinal.GetHashCode(Lic2);
					return hash;
				}
			}
		}

		private sealed class LruCache<TKey, TValue>(int capacity, Action<TValue> onEvict)
    {
			private struct CacheItem(TKey key, TValue value)
        {
				public TKey Key = key;
				public TValue Value = value;
        }

        private readonly int _capacity = Math.Max(1, capacity);
			private readonly Dictionary<TKey, LinkedListNode<CacheItem>> _map = [];
			private readonly LinkedList<CacheItem> _list = new();
			private readonly Action<TValue> _onEvict = onEvict;
			private readonly Lock _syncRoot = new();

        public TValue GetOrAdd(TKey key, Func<TKey, TValue> factory)
			{
				lock (_syncRoot)
				{
					if (_map.TryGetValue(key, out LinkedListNode<CacheItem> existing))
					{
						_list.Remove(existing);
						_list.AddFirst(existing);
						return existing.Value.Value;
					}
					TValue value = factory(key);
					AddInternal(key, value);
					return value;
				}
			}

			private void AddInternal(TKey key, TValue value)
			{
				LinkedListNode<CacheItem> node = new(new CacheItem(key, value));
				_list.AddFirst(node);
				_map[key] = node;

				if (_map.Count <= _capacity)
				{
					return;
				}

				LinkedListNode<CacheItem> last = _list.Last;
				if (last == null)
				{
					return;
				}
				_list.RemoveLast();
				_map.Remove(last.Value.Key);
				if (_onEvict != null)
				{
					_onEvict(last.Value.Value);
				}
			}
		}

		public static void DrawDisplayPanel(SongSettings InItem, int InHideDisplayPanel, ref ImageTransitionControl InPictureBox, Graphics g)
		{
			if (InItem.ItemID == "")
			{
				return;
			}
			SongLyrics songLyrics = InItem.Lyrics[3];
			int panelFontSize = (int)songLyrics.FS_Font.Size;
			int lineTop = 0;
			int leftMargin = 0;
			int fontSize = 1;
			int titleGap = 0;
			Color inColour = (PanelTextColourAsRegion1 > 0) ? Color.FromArgb(songLyrics.ForeColour) : PanelTextColour;
			int panelLeft = songLyrics.FS_Left;
			int panelTop = songLyrics.FS_Top;
			int panelWidth = songLyrics.FS_Width;
			int panelHeight = songLyrics.FS_Height;
			if (PanelBackColourTransparent < 1)
			{
				g.FillRectangle(GetCachedBrush(PanelBackColour), 0, panelTop - 5, Buffer_LS_Width, panelHeight + 5);
			}
			Font inFont = GetCachedFont(songLyrics.FS_Font.Name, panelFontSize, songLyrics.FS_Font.Style);
			lineTop = panelTop;
			string licenseText = BuildLicenseTextCached(InItem);
			string prevTitle = InItem.PrevTitle ?? "";
			string nextTitle = InItem.NextTitle ?? "";
			StringAlignment alignNear = DisplayPanelFormatNear.Alignment;
			StringAlignment alignFar = DisplayPanelFormatFar.Alignment;
			if (ShowDataDisplayMode > 0 && InHideDisplayPanel == 0)
			{
				int prevNextWidth = 0;
				int prevWidth = 0;
				int nextWidth = 0;
				int prevMarkerWidth = 0;
				int nextMarkerWidth = 0;
				float measureScale = DisplayPanelMeasureScale;
				int prevNextAreaWidth = (int)((float)panelWidth * 0.15f);
				int maxTitleWidth = (int)((float)panelWidth * ((InItem.TotalSlides > 20) ? 0.15f : 0.25f));
				int textAreaWidth = panelWidth;
				fontSize = DisplayFontSize(11, Buffer_LS_Width, 3, 1);
				inFont = GetCachedFont(songLyrics.FS_Font.Name, fontSize, songLyrics.FS_Font.Style);
				int indicatorsHeight = fontSize * panelHeight / panelFontSize;
				int indicatorsTop = panelTop + (panelHeight - indicatorsHeight);
				if (ShowDataDisplayPrevNext > 0)
				{
					textAreaWidth -= prevNextAreaWidth;
					string prevMarker = (prevTitle == "") ? "...  " : "<<";
					string nextMarker = (nextTitle == "") ? "...  " : ">>";
					prevMarkerWidth = MeasureStringWidthCached(g, prevMarker, inFont, DisplayPanelMeasureMaxWidth, measureScale);
					nextMarkerWidth = MeasureStringWidthCached(g, nextMarker, inFont, DisplayPanelMeasureMaxWidth, measureScale);
					prevWidth = MeasureStringWidthCached(g, prevTitle, inFont, DisplayPanelMeasureMaxWidth, measureScale);
					nextWidth = MeasureStringWidthCached(g, nextTitle, inFont, DisplayPanelMeasureMaxWidth, measureScale);
					if (maxTitleWidth > 0)
					{
						int prevAllowed = maxTitleWidth - prevMarkerWidth;
						if (prevWidth > 0 && prevWidth > prevAllowed)
						{
							prevTitle = TrimTextToWidth(g, prevTitle, inFont, prevAllowed, measureScale);
						}
						int nextAllowed = maxTitleWidth - nextMarkerWidth;
						if (nextWidth > 0 && nextWidth > nextAllowed)
						{
							nextTitle = TrimTextToWidth(g, nextTitle, inFont, nextAllowed, measureScale);
						}
					}
					prevTitle += prevMarker;
					nextTitle += nextMarker;
					prevWidth = MeasureStringWidthCached(g, prevTitle, inFont, DisplayPanelMeasureMaxWidth, measureScale);
					nextWidth = MeasureStringWidthCached(g, nextTitle, inFont, DisplayPanelMeasureMaxWidth, measureScale);
					prevNextWidth = ((prevWidth > nextWidth) ? prevWidth : nextWidth);
					RectangleF markerRect = new(panelWidth - prevWidth, indicatorsTop + 1, prevWidth, indicatorsHeight);
					OutputOneLineToScreen(InItem, prevTitle, inFont, g, inColour, alignFar, ShowDataDisplayFontShadow, ShowDataDisplayFontOutline, (int)markerRect.Left, (int)markerRect.Top, (int)markerRect.Width, 0);
					markerRect = new RectangleF(panelWidth - nextWidth, indicatorsTop + indicatorsHeight / 2, nextWidth, indicatorsHeight);
					OutputOneLineToScreen(InItem, nextTitle, inFont, g, inColour, alignFar, ShowDataDisplayFontShadow, ShowDataDisplayFontOutline, (int)markerRect.Left, (int)markerRect.Top, (int)markerRect.Width, 0);
					prevNextWidth = ((prevWidth > nextWidth) ? prevWidth : nextWidth);
				}
				int indicatorGap = 4;
				RectangleF rect_slidesinfo = new(0f, indicatorsTop, panelWidth - (prevNextWidth + indicatorGap), indicatorsHeight);
				textAreaWidth = DP_SetSlideIndicators(InItem, ref InPictureBox, ref g, inFont, rect_slidesinfo) - indicatorGap;
				lineTop = panelTop + panelHeight / 2;
				panelFontSize = DisplayFontSize(panelFontSize, Buffer_LS_Width, 3, 1);
				fontSize = ((ShowDataDisplayTitle > 0) ? (panelFontSize * 7 / 8) : panelFontSize);
				if (fontSize < 1)
				{
					fontSize = 1;
				}
				inFont = GetCachedFont(songLyrics.FS_Font.Name, fontSize, songLyrics.FS_Font.Style);
				bool licenseShown = false;
				int titleHeight = panelHeight;
				if ((ShowDataDisplayCopyright > 0 || InItem.Show_LicAdim) && licenseText != "")
				{
					int licenseTop = 0;
					int licenseWidth = MeasureStringWidthCached(g, licenseText, inFont, DisplayPanelMeasureMaxWidth, measureScale);
					if (licenseWidth > textAreaWidth - leftMargin)
					{
						ReduceFontToFit(g, licenseText, ref inFont, textAreaWidth - leftMargin, panelHeight / 2, MultiLine: true);
					}
					RectangleF licenseRect = new(leftMargin + indicatorGap, lineTop, textAreaWidth - leftMargin, panelHeight / 2 + 2);
					licenseTop = OutputOneLineToScreen(InItem, licenseText, inFont, g, inColour, alignNear, ShowDataDisplayFontShadow, ShowDataDisplayFontOutline, (int)licenseRect.Left, (int)licenseRect.Top, (int)licenseRect.Width, panelHeight / 2);
					titleHeight = licenseTop - panelTop;
					lineTop = licenseTop - titleHeight;
					licenseShown = true;
				}
				else
				{
					lineTop = panelTop;
				}
				titleGap = panelHeight * 10 / 100;
				titleHeight -= titleGap;
				lineTop += titleGap;
				inFont = GetCachedFont(songLyrics.FS_Font.Name, panelFontSize * 12 / 10, songLyrics.FS_Font.Style);
				if (ShowDataDisplayTitle > 0)
				{
					string title = InItem.Title;
					int titleWidth = MeasureStringWidthCached(g, title, inFont, DisplayPanelMeasureMaxWidth, measureScale);
					titleHeight = ((titleHeight > 2) ? (titleHeight - 2) : 2);
					if (titleWidth > textAreaWidth)
					{
						ReduceFontToFit(g, title, ref inFont, textAreaWidth - leftMargin, titleHeight, !licenseShown);
					}
					if (inFont.Size >= 25f)
					{
						lineTop -= 3;
					}
					else if (inFont.Size <= 21f)
					{
						lineTop += 2;
					}
					RectangleF titleRect = new(leftMargin + indicatorGap, lineTop + (licenseShown ? 3 : (-2)), textAreaWidth - leftMargin, titleHeight);
					OutputOneLineToScreen(InItem, title, inFont, g, inColour, alignNear, ShowDataDisplayFontShadow, ShowDataDisplayFontOutline, (int)titleRect.Left, (int)titleRect.Top, (int)titleRect.Width, titleHeight);
				}
				InItem.Show_LicAdim = false;
			}
			else if (InItem.Show_LicAdim)
			{
				lineTop = panelTop + panelHeight / 2;
				RectangleF licenseRect = new(leftMargin, lineTop, panelWidth, panelHeight / 2);
				OutputOneLineToScreen(InItem, licenseText, inFont, g, inColour, alignNear, ShowDataDisplayFontShadow, ShowDataDisplayFontOutline, (int)licenseRect.Left, (int)licenseRect.Top, (int)licenseRect.Width, 0);
				InItem.Show_LicAdim = false;
			}
		}

		public static int DataDisplaySlides(SongSettings InItem, ref Graphics g, Font tempFont, Color In_TextColour, RectangleF rect_slidesinfo, int VersesSymOffsetTop, int SlidesSymOffsetTop, int RectOffsetTop, int OffsetLeft, bool DisplayIndicators)
		{
			int curSlide = InItem.CurSlide;
			int totalSlides = InItem.TotalSlides;
			string slideLabel = "";
			string symbolLabel = "";
			Color color = In_TextColour;
			int firstShown = (curSlide - 1) / 10 * 10 + 1;
			int lastShown = (totalSlides > firstShown + 9) ? (firstShown + 9) : totalSlides;
			int shownCount = lastShown - firstShown + 1;
			int startLeft = OffsetLeft;
			for (int i = 1; i <= 11; i++)
			{
				symbolLabel = "";
				slideLabel = "";
				bool isCurrent = false;
				if (i <= shownCount)
				{
					int slideNo = firstShown + i - 1;
					slideLabel = ((slideNo < 10) ? " " : "") + slideNo.ToString("0") + " ";
					isCurrent = curSlide == slideNo;
					if (InItem.Slide[slideNo, 0] is >= 0 and <= 10 or >= 100 and <= 112)
					{
						symbolLabel = " " + ConvertSequenceSymbol(SequenceSymbol[InItem.Slide[slideNo, 0]]);
					}
				}
				else if (i == 11 && totalSlides > lastShown)
				{
					slideLabel = ".. " + totalSlides.ToString("00");
				}
				int labelWidth = (int)g.MeasureString(slideLabel, tempFont, 10000).Width;
				if (DisplayIndicators)
				{
					if (isCurrent)
					{
                    using Pen framePen = new(In_TextColour);
                    g.DrawRectangle(framePen, OffsetLeft + 1, RectOffsetTop, labelWidth + 1, rect_slidesinfo.Height * 2f / 5f);
                }
					color = (isCurrent ? Color.Red : In_TextColour);
					OutputOneLineToScreen(InItem, symbolLabel, tempFont, g, color, StringAlignment.Near, ShowDataDisplayFontShadow, ShowDataDisplayFontOutline, OffsetLeft, VersesSymOffsetTop, OffsetLeft + labelWidth, 0);
					OutputOneLineToScreen(InItem, slideLabel, tempFont, g, color, StringAlignment.Near, ShowDataDisplayFontShadow, ShowDataDisplayFontOutline, OffsetLeft, SlidesSymOffsetTop, OffsetLeft + labelWidth, 0);
				}
				OffsetLeft += labelWidth;
			}
			startLeft = OffsetLeft - startLeft;
			return startLeft;
		}

		public static void MessageOverSplashScreen(string InString)
		{
			SplashScreenBack = true;
			MessageBox.Show(InString);
			SplashScreenBack = false;
		}

		public static void ComputeShowLineSpacing()
		{
			for (int i = 0; i < MAXSONGSFOLDERS; i++)
			{
				Folders[i].FontSpacingFactor[0] = Folders[i].LineSpacing[0] + 0.05;
				Folders[i].FontSpacingFactor[1] = Folders[i].LineSpacing[1] + 0.05;
			}
		}

		public static int SetLyricsTopPos(int TopSetting, int ScreenHeight)
		{
			return ScreenHeight * TopSetting / 100;
		}

		public static void ResetShowRunningSettings()
		{
			ShowRunning_ShowDataDisplayMode = 0;
			ShowRunning_ShowSongHeadings = 0;
			ShowRunning_ShowLyrics = 0;
			ShowRunning_UseShadowFont = 0;
			ShowRunning_UseOutlineFont = 0;
			ShowRunning_ShowNotations = 0;
			ShowRunning_ShowInterlace = 0;
			ShowRunning_ShowVerticalAlign = 0;
		}

		public static int DP_SetSlideIndicators(SongSettings InItem, ref ImageTransitionControl InPic, ref Graphics g, Font tempFont, RectangleF rect_slidesinfo)
		{
			int itemNo = (InItem.Source == ItemSource.WorshipList) ? InItem.CurItemNo : 0;
			int totalWorshipListItems = TotalWorshipListItems;
			Color color = (PanelTextColourAsRegion1 > 0) ? Color.FromArgb(InItem.Lyrics[3].ForeColour) : PanelTextColour;
			using StringFormat stringFormat = new();
			string totalText = "";
			string itemText = "";
			FontStyle fontStyle = FontStyle.Regular;
			if (ShowDataDisplayFontBold > 0)
			{
				fontStyle |= FontStyle.Bold;
			}
			if (ShowDataDisplayFontItalic > 0)
			{
				fontStyle |= FontStyle.Italic;
			}
			// The caller's font is cached and stays the caller's; this styled copy is made for this call only.
			using Font styledFont = new(tempFont.Name, tempFont.Size, fontStyle);
			tempFont = styledFont;
			int lineHeight = (int)g.MeasureString("1", tempFont, 10000).Height;
			int topRowY = (int)(rect_slidesinfo.Top + rect_slidesinfo.Height / 20f);
			int bottomRowY = (int)(rect_slidesinfo.Top + rect_slidesinfo.Height / 2f);
			int separatorY = bottomRowY - lineHeight / 20;
			int indicatorsLeft = 0;
			if (ShowDataDisplaySlides > 0)
			{
				int slidesWidth = DataDisplaySlides(InItem, ref g, tempFont, color, rect_slidesinfo, topRowY, bottomRowY, separatorY, 0, DisplayIndicators: false);
				indicatorsLeft = (int)rect_slidesinfo.Width - slidesWidth;
				DataDisplaySlides(InItem, ref g, tempFont, color, rect_slidesinfo, topRowY, bottomRowY, separatorY, indicatorsLeft, DisplayIndicators: true);
				indicatorsLeft -= (int)g.MeasureString("1", tempFont, 10000).Width;
			}
			else
			{
				indicatorsLeft = (int)rect_slidesinfo.Width;
			}
			if (ShowDataDisplaySongs > 0)
			{
				itemText = ((itemNo > 0) ? itemNo.ToString() : "A");
				int itemTextWidth = (int)g.MeasureString(itemText, tempFont, 10000).Width;
				totalText = totalWorshipListItems.ToString();
				int totalTextWidth = (int)g.MeasureString(totalText, tempFont, 10000).Width;
				indicatorsLeft -= totalTextWidth;
				OutputOneLineToScreen(InItem, itemText, tempFont, g, color, stringFormat.Alignment, ShowDataDisplayFontShadow, ShowDataDisplayFontOutline, indicatorsLeft + (totalTextWidth - itemTextWidth) / 2, topRowY, indicatorsLeft + totalTextWidth, 0);
				OutputOneLineToScreen(InItem, totalText, tempFont, g, color, stringFormat.Alignment, ShowDataDisplayFontShadow, ShowDataDisplayFontOutline, indicatorsLeft, bottomRowY, indicatorsLeft + totalTextWidth, 0);
            using Pen separatorPen = new(color);
            g.DrawLine(separatorPen, indicatorsLeft, separatorY, indicatorsLeft + totalTextWidth, separatorY);
        }
			return indicatorsLeft;
		}
}
