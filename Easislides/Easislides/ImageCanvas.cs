using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;

namespace Easislides
{
	internal class ImageCanvas : Control, IDisposable
	{
		// Constants
		private const float FONT_SIZE_RATIO = 12.5f;
		private const float SMALL_FONT_SIZE_RATIO = 24f;
		private const float RESOLUTION_MULTIPLIER = 2f;
		private const int SINGLE_DIGIT_OFFSET = 5;
		private const int MULTI_DIGIT_OFFSET = 12;

		// Auto-properties
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Image Image { get; set; }

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public int PosLeft { get; set; }

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public int PosTop { get; set; }

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public int PosWidth { get; set; }

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public int PosHeight { get; set; }

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public string FileName { get; set; } = "";

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public float ImageRatio { get; set; } = 1f;

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool IsPowerPoint { get; set; }

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public int SlideNumber { get; set; }

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool IsImageReady { get; set; }

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public float BorderWidthFactor { get; set; }

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public int PowerPointSlideNumbering { get; set; }

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool IsExternalPowerPoint { get; set; }

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public int CurrentSelectedSlide { get; set; }

		// Legacy property names for backward compatibility
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool PowerPoint
		{
			get => IsPowerPoint;
			set => IsPowerPoint = value;
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Image image
		{
			get => Image;
			set => Image = value;
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool ExternalPP
		{
			get => IsExternalPowerPoint;
			set => IsExternalPowerPoint = value;
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public int PPSlideNumbering
		{
			get => PowerPointSlideNumbering;
			set => PowerPointSlideNumbering = value;
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public int CurSelectedSlide
		{
			get => CurrentSelectedSlide;
			set => CurrentSelectedSlide = value;
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool ImageDone
		{
			get => IsImageReady;
			set => IsImageReady = value;
		}

				public void SetImageRatio(int InImageWidth, int InImageHeight)
		{
			ImageRatio = (float)InImageWidth / (float)InImageHeight;
		}

		public void ResizeCanvas(int InCanvasWidth, int InCanvasHeight)
		{
			if (Image == null)
			{
				Console.WriteLine("Error: Image is null in ResizeCanvas.");
				return;
			}

			try
			{
				base.Width = InCanvasWidth;
				base.Height = InCanvasHeight;

				float canvasRatio = (float)base.Width / base.Height;
				float imageRatio = (float)Image.Width / Image.Height;

				if (canvasRatio < imageRatio)
				{
					PosWidth = base.Width;
					PosHeight = (int)Math.Round(PosWidth / imageRatio);
					PosLeft = 0;
					PosTop = (base.Height - PosHeight) / 2;
				}
				else
				{
					PosHeight = base.Height;
					PosWidth = (int)Math.Round(PosHeight * imageRatio);
					PosTop = 0;
					PosLeft = (base.Width - PosWidth) / 2;
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error in ResizeCanvas: {ex.Message}");
			}
		}



		public ImageCanvas()
		{
			SetStyle(ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, value: true);
			Cursor = Cursors.Hand;
			base.Visible = false;
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			if ((Image == null || string.IsNullOrEmpty(FileName)) && !base.Visible)
				return;

			e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
			e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
			e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

			if (!IsImageReady && !CalculateImage())
				return;

			DrawImage(e);
		}

		private void DrawImage(PaintEventArgs e)
		{
			e.Graphics.Clear(BackColor);
			e.Graphics.DrawImage(Image, new Rectangle(PosLeft, PosTop, PosWidth, PosHeight), new Rectangle(0, 0, Image.Width, Image.Height), GraphicsUnit.Pixel);
		}

		public void BuildNewImageThumbs(int NewCanvasIndex, int NewWidth, int NewHeight, ref string[] NewImageArray,
								int NewTotalCount, string NewPrefix, int NewCurSelectedSlide, Color NewBackColour,
								ToolTip NewToolTip, bool NewExternalPowerPoint)
		{
			try
			{
				int slideNumbering = NewCanvasIndex + 1;
				string extension = Path.GetExtension(NewImageArray[NewCanvasIndex]).ToLower();
				string imagePath = NewImageArray[NewCanvasIndex];

				base.Width = NewWidth;
				base.Height = NewHeight;

				IsExternalPowerPoint = NewExternalPowerPoint;
				CurrentSelectedSlide = NewCurSelectedSlide;

				if (IsPowerPoint)
				{
					SlideNumber = PowerPointSlideNumbering;
				}
				else
				{
					SlideNumber = 0;
					if (extension == ".ppt" || extension == ".pptx")
					{
						imagePath = $"{gf.ExtPPrefix}{gf.ExtPPrefix_Num}\\{Path.GetFileNameWithoutExtension(imagePath)}.jpg";
					}
				}

				if (FileName != imagePath || IsExternalPowerPoint)
				{
					FileName = imagePath;
					IsImageReady = false;
					BackColor = NewBackColour;
					BorderWidthFactor = 0.04f;
					base.Visible = true;

					NewToolTip.SetToolTip(this, SlideNumber > 0 ? SlideNumber.ToString() : imagePath);

					PowerPointSlideNumbering = slideNumbering;

					ResizeCanvas(base.Width, base.Height);

					Invalidate();
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error in BuildNewImageThumbs: {ex.Message}");
			}
		}

		private void DrawSlideNumber(Graphics graphics, int storedImageWidth, int storedImageHeight, int margin)
		{
			string text = PowerPointSlideNumbering.ToString();
			float offsetX = PowerPointSlideNumbering < 10 ? SINGLE_DIGIT_OFFSET : MULTI_DIGIT_OFFSET;
			PointF position = new PointF(storedImageWidth - (margin * 2 + offsetX), margin);

			using (Font font = new Font("Microsoft Sans Serif", storedImageWidth / SMALL_FONT_SIZE_RATIO))
			{
				SizeF textSize = graphics.MeasureString(text, font);

				graphics.FillRectangle(Brushes.Black, position.X - 3, position.Y + 1, textSize.Width - 2, textSize.Height - 3);
				graphics.FillRectangle(Brushes.Yellow, position.X - 4, position.Y, textSize.Width - 2, textSize.Height - 3);
				graphics.DrawString(text, font, Brushes.Black, position.X - 5, position.Y);
			}
		}

		private void DrawBorder(Graphics graphics, Rectangle rect, float fontSize)
		{
			Color borderColor = (CurrentSelectedSlide == PowerPointSlideNumbering) ? Color.Red : base.Parent.BackColor;
			using (Pen pen = new Pen(borderColor, fontSize))
			{
				graphics.DrawRectangle(pen, rect);
			}
		}

		public bool CalculateImage()
		{
			int newImageWidth = 0, newImageHeight = 0;
			int storedImageWidth = 0, storedImageHeight = 0;

			try
			{
				if (IsExternalPowerPoint)
				{
					gf.ExternalPPT.BuildOneFirstScreenDump(PowerPointSlideNumbering);
				}

				using System.Drawing.Image sourceImage = System.Drawing.Image.FromFile(FileName);
				SetImageRatio(sourceImage.Width, sourceImage.Height);
				gf.CalcImageToFit(ImageRatio, base.Width, base.Height, ref newImageWidth, ref newImageHeight, true, ref storedImageWidth, ref storedImageHeight);

				if (newImageWidth <= 0 || newImageHeight <= 0)
				{
					return false;
				}

				storedImageWidth = (int)(storedImageWidth * RESOLUTION_MULTIPLIER);
				storedImageHeight = (int)(storedImageHeight * RESOLUTION_MULTIPLIER);

				using Bitmap renderedImage = new(storedImageWidth, storedImageHeight, PixelFormat.Format24bppRgb);
				using Graphics graphics = Graphics.FromImage(renderedImage);

				graphics.SmoothingMode = SmoothingMode.AntiAlias;
				graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
				graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

				Rectangle fullRect = new(0, 0, storedImageWidth, storedImageHeight);

				if (IsPowerPoint || IsExternalPowerPoint)
				{
					float fontSize = storedImageWidth / FONT_SIZE_RATIO;
					int margin = storedImageWidth >= storedImageHeight
						? (int)(storedImageWidth * BorderWidthFactor)
						: (int)(storedImageHeight * BorderWidthFactor);

					Rectangle imageRect = new(margin, margin, storedImageWidth - 2 * margin, storedImageHeight - 2 * margin);
					graphics.DrawImage(sourceImage, imageRect);

					DrawSlideNumber(graphics, storedImageWidth, storedImageHeight, margin);
					DrawBorder(graphics, fullRect, fontSize);
				}
				else
				{
					graphics.DrawImage(sourceImage, fullRect);
				}

				Image = (System.Drawing.Image)renderedImage.Clone();

				IsImageReady = true;
				ResizeCanvas(base.Width, base.Height);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error in CalculateImage: {ex.Message}");
				IsImageReady = false;
				return false;
			}
			return true;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				Image?.Dispose();
				Image = null;
			}
			base.Dispose(disposing);
		}
	}
}
