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
    internal unsafe partial class gf
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

		/// <summary>
		/// PowerPoint 슬라이드에 슬라이드 번호 오버레이와 선택 테두리를 그립니다.
		/// </summary>
		private static void DrawPowerPointOverlay(Graphics graphics, Image sourceImage, int storedImageWidth, int storedImageHeight,
			int slideNumber, int curSelectedSlide, Color parentBackColor)
		{
			using (Font font = new Font("Microsoft Sans Serif", (float)storedImageWidth * FONT_SIZE_RATIO))
			{
				int padding = (int)((storedImageWidth >= storedImageHeight) ? (storedImageWidth * PADDING_RATIO) : (storedImageHeight * PADDING_RATIO));
				Rectangle imageRect = new Rectangle(padding, padding, storedImageWidth - padding * 2, storedImageHeight - padding * 2);
				Rectangle outerRect = new Rectangle(0, 0, storedImageWidth, storedImageHeight);

				// 이미지 그리기
				graphics.DrawImage(sourceImage, imageRect);

				// 슬라이드 번호 텍스트
				string slideText = slideNumber.ToString();
				PointF textPosition = new PointF(padding + TEXT_OFFSET, padding + TEXT_OFFSET);
				SizeF textSize = graphics.MeasureString(slideText, font);

				using (SolidBrush shadowBrush = new SolidBrush(BlackScreenColour))
				using (SolidBrush yellowBrush = new SolidBrush(Color.Yellow))
				using (SolidBrush textBrush = new SolidBrush(BlackScreenColour))
				{
					// 텍스트 배경 (그림자 효과)
					graphics.FillRectangle(shadowBrush, textPosition.X + SHADOW_OFFSET, textPosition.Y + SHADOW_OFFSET, textSize.Width, textSize.Height);
					// 텍스트 배경 (노란색)
					graphics.FillRectangle(yellowBrush, textPosition.X, textPosition.Y, textSize.Width, textSize.Height);
					// 텍스트 그리기
					graphics.DrawString(slideText, font, textBrush, textPosition.X, textPosition.Y);
				}

				// 선택 테두리
				DrawSelectionBorder(graphics, outerRect, slideNumber, curSelectedSlide, parentBackColor, (int)font.Size);
			}
		}

		/// <summary>
		/// 외부 PowerPoint 파일에 선택 테두리를 그립니다.
		/// </summary>
		private static void DrawExternalPowerPointOverlay(Graphics graphics, Image sourceImage, int storedImageWidth, int storedImageHeight,
			int slideNumber, int curSelectedSlide, Color parentBackColor)
		{
			using (Font font = new Font("Microsoft Sans Serif", (float)storedImageWidth * FONT_SIZE_RATIO))
			{
				int padding = (int)((storedImageWidth >= storedImageHeight) ? (storedImageWidth * PADDING_RATIO) : (storedImageHeight * PADDING_RATIO));
				Rectangle imageRect = new Rectangle(padding, padding, storedImageWidth - padding * 2, storedImageHeight - padding * 2);
				Rectangle outerRect = new Rectangle(0, 0, storedImageWidth, storedImageHeight);

				// 이미지 그리기
				graphics.DrawImage(sourceImage, imageRect);

				// 선택 테두리
				DrawSelectionBorder(graphics, outerRect, slideNumber, curSelectedSlide, parentBackColor, (int)font.Size);
			}
		}

		/// <summary>
		/// 선택 상태에 따라 테두리를 그립니다.
		/// </summary>
		private static void DrawSelectionBorder(Graphics graphics, Rectangle rect, int slideNumber, int curSelectedSlide, Color parentBackColor, int penWidth)
		{
			Color borderColor = (curSelectedSlide == slideNumber) ? Color.Red : parentBackColor;
			using (Pen pen = new Pen(borderColor, penWidth))
			{
				graphics.DrawRectangle(pen, rect);
			}
		}

		public static void ShowThumbImage(ref ImageCanvas InCanvas, int InCanvasIndex, int InWidth, int InHeight, ref string[] FileArray, string[] ImageArray, int FirstRef, int MaxDisplay, ToolTip InToolTip, string InPrefix, int CurSelectedSlide, bool ExternalPP)
		{
			if (InCanvasIndex >= MaxDisplay)
			{
				InToolTip.SetToolTip(InCanvas, "");
				FileArray[InCanvasIndex] = "";
				InCanvas.FileName = "";
				InCanvas.Visible = false;
				return;
			}
			int num = FirstRef + InCanvasIndex + 1;
			string text = "";
			int posWidth = InCanvas.PosWidth;
			int posHeight = InCanvas.PosHeight;
			string text2 = ImageArray[FirstRef + InCanvasIndex];
			if (InCanvas.PowerPoint)
			{
				text = num.ToString();
				InCanvas.SlideNumber = num;
			}
			else
			{
				text = GetDisplayNameOnly(ref ImageArray[FirstRef + InCanvasIndex], UpdateByRef: false, KeepExt: true);
				InCanvas.SlideNumber = 0;
				if (DataUtil.Right(text2, 4) == ".ppt")
				{
					text2 = ExtPPrefix + Path.GetFileNameWithoutExtension(text2) + ".jpg";
				}
				if (InCanvas.FileName == text2 && !ExternalPP)
				{
					int NewImageWidth = 0;
					int NewImageHeight = 0;
					CalcImageToFit(InCanvas.ImageRatio, InCanvas.PosWidth, InCanvas.PosHeight, ref NewImageWidth, ref NewImageHeight);
					InCanvas.Width = NewImageWidth;
					InCanvas.Height = NewImageHeight;
					InCanvas.Left = InCanvas.PosLeft + (int)((float)(InCanvas.PosWidth - NewImageWidth) / 2f);
					InCanvas.Top = InCanvas.PosTop + (int)((float)(InCanvas.PosHeight - NewImageHeight) / 2f);
					return;
				}
			}
			int NewImageWidth2 = 0;
			int NewImageHeight2 = 0;
			int num2 = 0;
			int num3 = 0;
			int StoredImageWidth = 0;
			int StoredImageHeight = 0;
			try
			{
				// Load image using stream to avoid file lock
				Image image;
				using (var stream = new FileStream(text2, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
				{
					image = Image.FromStream(stream);
				}

				using (image)
				{
					InCanvas.SetImageRatio(image.Width, image.Height);
					CalcImageToFit(InCanvas.ImageRatio, posWidth, posHeight, ref NewImageWidth2, ref NewImageHeight2, ComputeStoredImage: true, ref StoredImageWidth, ref StoredImageHeight);
					if (!((NewImageWidth2 <= 0) | (NewImageHeight2 <= 0)))
					{
						// Create thumbnail image outside of using block to assign to InCanvas
						Image thumbnailImage = new Bitmap(StoredImageWidth, StoredImageHeight, PixelFormat.Format24bppRgb);
						using (Graphics graphics = Graphics.FromImage(thumbnailImage))
						{
							Rectangle rect = new Rectangle(0, 0, StoredImageWidth, StoredImageHeight);
							if (InCanvas.PowerPoint)
							{
								DrawPowerPointOverlay(graphics, image, StoredImageWidth, StoredImageHeight, num, CurSelectedSlide, InCanvas.Parent.BackColor);
							}
							else if (ExternalPP)
							{
								DrawExternalPowerPointOverlay(graphics, image, StoredImageWidth, StoredImageHeight, num, CurSelectedSlide, InCanvas.Parent.BackColor);
							}
							else
							{
								graphics.DrawImage(image, rect);
							}
						}

						num2 = (posWidth - NewImageWidth2) / 2;
						num3 = (posHeight - NewImageHeight2) / 2;
						InCanvas.Left = InCanvas.PosLeft + num2;
						InCanvas.Top = InCanvas.PosTop + num3;
						InCanvas.Width = NewImageWidth2;
						InCanvas.Height = NewImageHeight2;
						InToolTip.SetToolTip(InCanvas, text);
						FileArray[InCanvasIndex] = ImageArray[FirstRef + InCanvasIndex];
						InCanvas.FileName = text2;

						// Dispose old image if exists
						InCanvas.image?.Dispose();
						InCanvas.image = thumbnailImage;

						InCanvas.Visible = true;
						InCanvas.Invalidate();
					}
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"ShowThumbImage error: {ex.Message}");
			}
		}

		public static float SetImageRatio(int InImageWidth, int InImageHeight)
		{
			if (InImageHeight == 0)
			{
				Debug.WriteLine("SetImageRatio: Height is zero, returning default ratio 1.0");
				return 1f;
			}
			return (float)InImageWidth / InImageHeight;
		}

		public static void CalcImageToFit(float InImageRatio, int InContainerWidth, int InContainerHeight, ref int NewImageWidth, ref int NewImageHeight)
		{
			int StoredImageWidth = 0;
			int StoredImageHeight = 0;
			CalcImageToFit(InImageRatio, InContainerWidth, InContainerHeight, ref NewImageWidth, ref NewImageHeight, ComputeStoredImage: false, ref StoredImageWidth, ref StoredImageHeight);
		}

		public static void CalcImageToFit(float InImageRatio, int InContainerWidth, int InContainerHeight, ref int NewImageWidth, ref int NewImageHeight, bool ComputeStoredImage, ref int StoredImageWidth, ref int StoredImageHeight)
		{
			float num = (float)InContainerWidth / (float)InContainerHeight;
			if (num < InImageRatio)
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
				string text = EasiSlidesTempDir + fileNameWithoutExtension + GetUniqueID() + extension;
				using FileStream fileStream = new FileStream(text, FileMode.OpenOrCreate, FileAccess.Write);
				fileStream.Write(img, 0, img.Length);
				return text;
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
			Color color = InItem.Format.ShowScreenColour[0];
			Color color2 = InItem.Format.ShowScreenColour[1];
			int showScreenStyle = InItem.Format.ShowScreenStyle;
			string tempImageFileName = InItem.Format.TempImageFileName;
			string backgroundPicture = InItem.Format.BackgroundPicture;
			int backgroundMode = (int)InItem.Format.BackgroundMode;
			bool useDefaultFormat = InItem.UseDefaultFormat;
			string text = InItem.Lyrics[0].Text;
			string text2 = InItem.Lyrics[1].Text;
			string text3 = InItem.Lyrics[2].Text;
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
						InItem.Format.ShowScreenColour[0] = BlackScreenColour;
						InItem.Format.ShowScreenColour[1] = BlackScreenColour;
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
						InItem.Format.ShowScreenColour[0] = ShowScreenColour[0];
						InItem.Format.ShowScreenColour[1] = ShowScreenColour[1];
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
			string text = "";
			if (!ClearAll)
			{
				switch (ReferenceAlertSource)
				{
					case 1:
						text = InItem.Title;
						break;
					case 2:
						text = InItem.SongNumber.ToString();
						break;
					case 3:
						text = InItem.Book_Reference;
						break;
					case 4:
						text = InItem.User_Reference;
						break;
					default:
						text = "";
						break;
				}
			}
			if (ReferenceAlertUsePick & (text != "") & (ReferenceAlertPickName != ""))
			{
				int num = text.IndexOf(ReferenceAlertPickName);
				if (num >= 0)
				{
					int num2 = num + ReferenceAlertPickName.Length;
					if (text.Length == num2)
					{
						text = ReferenceAlertPickSubstitute;
					}
					else if (ReferenceAlertPickSeparator != "")
					{
						int num3 = text.IndexOfAny(ReferenceAlertPickSeparator.ToCharArray(), num2);
						num3 = ((num3 >= 0) ? num3 : text.Length);
						text = ((ReferenceAlertPickSubstitute == "") ? ReferenceAlertPickName : ReferenceAlertPickSubstitute) + DataUtil.Mid(text, num2, num3 - num2);
					}
					else
					{
						text = ((ReferenceAlertPickSubstitute == "") ? ReferenceAlertPickName : ReferenceAlertPickSubstitute) + DataUtil.Mid(text, num2, text.Length - num2);
					}
				}
				else if (ReferenceAlertBlankIfPickNotFound)
				{
					text = "";
				}
			}
			if (DoActiveIndicator && text == "")
			{
				text = " ";
			}
			if (text == "")
			{
				InScreen.StopRef();
				InScreen.RefDisplayString = "";
			}
			else
			{
				ReferenceAlertFont = GetNewFont(ReferenceAlertFontName, ReferenceAlertFontSize, ReferenceAlertBold, ReferenceAlertItalic, ReferenceAlertUnderline, ShowErrorMsg: false);
				InScreen.LoadRef(InItem, DataUtil.Left(text, 50), ReferenceAlertDuration, ReferenceAlertFont, ReferenceAlertScroll, ReferenceAlertFlash, ReferenceAlertTransparent, ReferenceAlertShadow, ReferenceAlertOutline, ReferenceAlertTextColour, ReferenceAlertBackColour, ReferenceAlertTextAlign, ReferenceAlertVerticalAlign, BottomBorderFactor);
			}
		}

		// 썸네일 이미지 관련 상수
		private const int THUMBNAIL_WIDTH = 200;
		private const int THUMBNAIL_HEIGHT = 150;
		private const float FONT_SIZE_RATIO = 1f / 12f;
		private const float PADDING_RATIO = 0.04f;

		// UI 레이아웃 관련 상수
		private const int IMAGE_SPACING = 5;  // 이미지 간격
		private const int TEXT_OFFSET = 2;    // 텍스트 위치 오프셋
		private const float SHADOW_OFFSET = 3f;  // 그림자 오프셋

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
				Graphics g = Graphics.FromImage(image);
				BackPattern.Fill(ref g, ShowScreenColour[0], ShowScreenColour[1], ShowScreenStyle, Buffer_LS_Width, Buffer_LS_Height, ref DefaultBackgroundID);
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
			bool flag = false;
			if ((ShowLiveCam & InItem.AtLiveScreen) || (InItem.OutputStyleScreen && ShowLiveBlack && TransitionAction != ImageTransitionControl.TransitionAction.AsFade))
			{
				flag = true;
			}
			if (InItem.PrevItemPP || flag)
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
