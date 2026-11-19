using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using Easislides.Module;
using Easislides.Util;

namespace Easislides
{
    internal unsafe partial class Gf
    {

		public static int FormatImageContainers(ref ImageCanvas[] InCanvas, int MaxCanvas, int InImagesCount, int w, int h, int LeftOffset, int TopOffset)
		{
			int result = 0;
			for (int i = 0; i < MaxCanvas; i++)
			{
				InCanvas[i].PosWidth = w;
				InCanvas[i].PosHeight = h;
				InCanvas[i].PosLeft = (w + IMAGE_SPACING) * (i % ThumbImagesPerRow) + LeftOffset;
				InCanvas[i].PosTop = TopOffset + (h + IMAGE_SPACING) * (i / ThumbImagesPerRow);
				if (i < InImagesCount)
				{
					InCanvas[i].Visible = true;
					result = i + 1;
				}
				else
				{
					InCanvas[i].Visible = false;
				}
			}
			return result;
		}

		public static void CalcImageToFit(float InImageRatio, int InContainerWidth, int InContainerHeight, ref int NewImageWidth, ref int NewImageHeight, bool ComputeStoredImage, ref int StoredImageWidth, ref int StoredImageHeight)
		{
			float containerAspectRatio = (float)InContainerWidth / (float)InContainerHeight;
			if (containerAspectRatio < InImageRatio)
			{
				NewImageWidth = InContainerWidth;
				NewImageHeight = (int)((float)NewImageWidth / InImageRatio);
				if (ComputeStoredImage)
				{
					StoredImageWidth = THUMBNAIL_WIDTH;
					StoredImageHeight = (int)((float)StoredImageWidth / InImageRatio);
				}
			}
			else
			{
				NewImageHeight = InContainerHeight;
				NewImageWidth = (int)((float)NewImageHeight * InImageRatio);
				if (ComputeStoredImage)
				{
					StoredImageHeight = THUMBNAIL_HEIGHT;
					StoredImageWidth = (int)((float)StoredImageHeight * InImageRatio);
				}
			}
		}

		public static string dumpImageToFile(byte[] img, string InFileName)
		{
			try
			{
				string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(InFileName);
				string extension = Path.GetExtension(InFileName);
				string tempImagePath = EasiSlidesTempDir + fileNameWithoutExtension + GetUniqueID() + extension;
				using FileStream fileStream = new FileStream(tempImagePath, FileMode.OpenOrCreate, FileAccess.Write);
				fileStream.Write(img, 0, img.Length);
				return tempImagePath;
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"dumpImageToFile error: {ex.Message}");
				return "";
			}
		}

		private static readonly HashSet<string> SupportedImageExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
		{
			".bmp", ".jpg", ".jpeg", ".ico", ".gif"
		};

		public static bool SupportedImageFormat(string InFileName)
		{
			string extension = Path.GetExtension(InFileName);
			return SupportedImageExtensions.Contains(extension);
		}

		public static void SetTransparentBackground(SongSettings InItem, ref ImageTransitionControl InPic)
		{
			SetColoursFormat(InItem, ref InPic, SetTransparent: true);
		}

		public static void ResetPictureBox(ref SongSettings InItem, ref ImageTransitionControl InScreen, GapType GapItemBackground, ImageTransitionControl.TransitionAction InTransAction)
		{
			// 원래 색상을 잠시 보관했다가 마지막에 복원 — SongFormat 색상이 ARGB int 라 int 로 보관(변환 불필요).
			int color = InItem.Format.ShowScreenColour[0];
			int color2 = InItem.Format.ShowScreenColour[1];
			int showScreenStyle = InItem.Format.ShowScreenStyle;
			string tempImageFileName = InItem.Format.TempImageFileName;
			string backgroundPicture = InItem.Format.BackgroundPicture;
			int backgroundMode = (int)InItem.Format.BackgroundMode;
			bool useDefaultFormat = InItem.UseDefaultFormat;
			string savedRegion1Lyrics = InItem.Lyrics[0].Text;
			string savedRegion2Lyrics = InItem.Lyrics[1].Text;
			string savedHeading = InItem.Lyrics[2].Text;
			string songSequence = InItem.SongSequence;
			InItem.SongSequence = "";
			InItem.Lyrics[0].Text = "";
			InItem.Lyrics[1].Text = "";
			InItem.Lyrics[2].Text = "";
			InItem.Format.BackgroundPicture = "";
			InItem.Format.MediaLocation = "";
			if (InTransAction != ImageTransitionControl.TransitionAction.AsStored)
			{
				InItem.Format.ShowSlideTransition = 0;
			}
			if (InItem.OutputStyleScreen)
			{
				switch (GapItemBackground)
				{
					case GapType.Black:
						InItem.UseDefaultFormat = false;
						InItem.Format.ShowScreenColour[0] = BlackScreenColour.ToArgb();
						InItem.Format.ShowScreenColour[1] = BlackScreenColour.ToArgb();
						InItem.Format.ShowScreenStyle = 11;
						InItem.Format.TempImageFileName = "";
						InItem.Format.BackgroundPicture = "";
						InItem.Format.ShowSlideTransition = (GapItemUseFade ? 15 : 0);
						break;
					case GapType.User:
						{
							InItem.UseDefaultFormat = false;
							InItem.Format.TempImageFileName = GapItemLogoFile;
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
							InItem.Format.ShowSlideTransition = (GapItemUseFade ? 15 : 0);
							break;
						}
					case GapType.Default:
						InItem.UseDefaultFormat = false;
						InItem.Format.ShowScreenColour[0] = ShowScreenColour[0].ToArgb();
						InItem.Format.ShowScreenColour[1] = ShowScreenColour[1].ToArgb();
						InItem.Format.ShowScreenStyle = ShowScreenStyle;
						InItem.Format.TempImageFileName = BackgroundPicture;
						InItem.Format.BackgroundPicture = BackgroundPicture;
						InItem.Format.BackgroundMode = BackgroundMode;
						InItem.Format.ShowSlideTransition = (GapItemUseFade ? 15 : 0);
						break;
				}
			}
			SetShowBackground(InItem, ref InScreen, FallBackToDefault: false);
			InItem.UseDefaultFormat = useDefaultFormat;
			InItem.Format.ShowScreenColour[0] = color;
			InItem.Format.ShowScreenColour[1] = color2;
			InItem.Format.ShowScreenStyle = showScreenStyle;
			InItem.Format.BackgroundPicture = backgroundPicture;
			InItem.Format.TempImageFileName = tempImageFileName;
			InItem.Format.BackgroundMode = (ImageMode)backgroundMode;
			DrawText(ref InItem, ref InScreen, InItem.LyricsAndNotationsList, DoActiveIndicator: false, ClearAll: true);
		}

		public static void LoadReferenceAlert(ref ImageTransitionControl InScreen, SongSettings InItem, bool ClearAll, bool DoActiveIndicator)
		{
			InScreen.StopRef();
			string referenceText = "";
			if (!ClearAll)
			{
				switch (ReferenceAlertSource)
				{
					case 1:
						referenceText = InItem.Title;
						break;
					case 2:
						referenceText = InItem.SongNumber.ToString();
						break;
					case 3:
						referenceText = InItem.Book_Reference;
						break;
					case 4:
						referenceText = InItem.User_Reference;
						break;
					default:
						referenceText = "";
						break;
				}
			}
			if (ReferenceAlertUsePick & (referenceText != "") & (ReferenceAlertPickName != ""))
			{
				int pickNameStart = referenceText.IndexOf(ReferenceAlertPickName);
				if (pickNameStart >= 0)
				{
					int pickNameEnd = pickNameStart + ReferenceAlertPickName.Length;
					if (referenceText.Length == pickNameEnd)
					{
						referenceText = ReferenceAlertPickSubstitute;
					}
					else if (ReferenceAlertPickSeparator != "")
					{
						int separatorIndex = referenceText.IndexOfAny(ReferenceAlertPickSeparator.ToCharArray(), pickNameEnd);
						separatorIndex = ((separatorIndex >= 0) ? separatorIndex : referenceText.Length);
						referenceText = ((ReferenceAlertPickSubstitute == "") ? ReferenceAlertPickName : ReferenceAlertPickSubstitute) + DataUtil.Mid(referenceText, pickNameEnd, separatorIndex - pickNameEnd);
					}
					else
					{
						referenceText = ((ReferenceAlertPickSubstitute == "") ? ReferenceAlertPickName : ReferenceAlertPickSubstitute) + DataUtil.Mid(referenceText, pickNameEnd, referenceText.Length - pickNameEnd);
					}
				}
				else if (ReferenceAlertBlankIfPickNotFound)
				{
					referenceText = "";
				}
			}
			if (DoActiveIndicator && referenceText == "")
			{
				referenceText = " ";
			}
			if (referenceText == "")
			{
				InScreen.StopRef();
				InScreen.RefDisplayString = "";
			}
			else
			{
				ReferenceAlertFont = GetReusedFont(ReferenceAlertFont, ref ReferenceAlertFontKey, ReferenceAlertFontName, ReferenceAlertFontSize, ReferenceAlertBold, ReferenceAlertItalic, ReferenceAlertUnderline);
				InScreen.LoadRef(InItem, DataUtil.Left(referenceText, 50), ReferenceAlertDuration, ReferenceAlertFont, ReferenceAlertScroll, ReferenceAlertFlash, ReferenceAlertTransparent, ReferenceAlertShadow, ReferenceAlertOutline, ReferenceAlertTextColour, ReferenceAlertBackColour, ReferenceAlertTextAlign, ReferenceAlertVerticalAlign, BottomBorderFactor);
			}
		}

		// Size of the stored thumbnail picture (CalcImageToFit with ComputeStoredImage).
		private const int THUMBNAIL_WIDTH = 200;
		private const int THUMBNAIL_HEIGHT = 150;

		// Gap between thumbnail containers (FormatImageContainers).
		private const int IMAGE_SPACING = 5;

		public static void SetDefaultBackScreen(ref ImageTransitionControl InScreen)
		{
			try
			{
				switch (Buffer_LS_Width)
				{
					case <= 0 when Buffer_LS_Height == 768:
						Buffer_LS_Width = 1024;
						break;
					case <= 0 when Buffer_LS_Height == 800:
						Buffer_LS_Width = 1280;
						break;
					case <= 0 when Buffer_LS_Height == 1024:
						Buffer_LS_Width = 1280;
						break;
					case <= 0 when Buffer_LS_Height == 900:
						Buffer_LS_Width = 1440;
						break;
					case <= 0 when Buffer_LS_Height == 1050:
						Buffer_LS_Width = 1680;
						break;
					case <= 0 when Buffer_LS_Height == 1200:
						Buffer_LS_Width = 1600;
						break;
					case <= 0 when Buffer_LS_Height == 1536:
						Buffer_LS_Width = 2048;
						break;
					case <= 0 when Buffer_LS_Height == 1920:
						Buffer_LS_Width = 1080;
						break;
					case <= 0 when Buffer_LS_Height == 2560:
						Buffer_LS_Width = 1440;
						break;
					case <= 0 when Buffer_LS_Height == 3840:
						Buffer_LS_Width = 2160;
						break;
					case <= 0 when Buffer_LS_Height == 7680:
						Buffer_LS_Width = 4320;
						break;
				}

				Image image = new Bitmap(Buffer_LS_Width, Buffer_LS_Height);
				try
				{
					using Graphics g = Graphics.FromImage(image);
					Graphics pattern = g;
					BackPattern.Fill(ref pattern, ShowScreenColour[0], ShowScreenColour[1], ShowScreenStyle, Buffer_LS_Width, Buffer_LS_Height, ref DefaultBackgroundID);
				}
				catch
				{
					// Not handed to the screen: nothing else refers to it.
					image.Dispose();
					throw;
				}
				// The screen owns it from here (it releases the one it replaces).
				InScreen.SetDefaultBackgroundPicture(image);
			}
			catch (Exception e)
			{
				Console.WriteLine(e.StackTrace);
				Console.WriteLine(e.Message);
			}
		}

		public static ImageTransitionControl.TransitionTypes ComputeTransition(SongSettings InItem, ref ImageTransitionControl InPictureBox, ImageTransitionControl.TransitionAction TransitionAction)
		{
			bool suppressTransition = false;
			if ((ShowLiveCam & InItem.AtLiveScreen) || (InItem.OutputStyleScreen && ShowLiveBlack && TransitionAction != ImageTransitionControl.TransitionAction.AsFade))
			{
				suppressTransition = true;
			}
			if (InItem.PrevItemPP || suppressTransition)
			{
				InPictureBox.TransitionType = ImageTransitionControl.TransitionTypes.None;
			}
			else if (TransitionAction != ImageTransitionControl.TransitionAction.AsStored)
			{
				if (InItem.FirstShowing)
				{
					InPictureBox.TransitionType = (ImageTransitionControl.TransitionTypes)InItem.Format.ShowItemTransition;
				}
				else
				{
					switch (TransitionAction)
					{
						case ImageTransitionControl.TransitionAction.AsStoredItem:
							InPictureBox.TransitionType = (ImageTransitionControl.TransitionTypes)InItem.Format.ShowItemTransition;
							break;
						case ImageTransitionControl.TransitionAction.None:
							InPictureBox.TransitionType = ImageTransitionControl.TransitionTypes.None;
							break;
						default:
							InPictureBox.TransitionType = (ImageTransitionControl.TransitionTypes)InItem.Format.ShowSlideTransition;
							break;
					}
				}
			}
			return InPictureBox.TransitionType;
		}
	}
}
