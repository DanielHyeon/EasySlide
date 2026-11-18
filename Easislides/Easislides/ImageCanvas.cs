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

		// Private fields for caching
		private Image _cachedSourceImage;
		private string _cachedFileName;
		private DateTime _cachedFileTime;

		// Retries of a picture that could not be made (JPG not there yet or still being written by the
		// PowerPoint export, a failed export). Every repaint (hover, resize, scroll) used to try again: a
		// PowerPoint export plus up to 150 ms of sleeping on the UI thread each time. Now a failed attempt
		// is retried by a UI timer after 0.5 s and 2 s, then every 30 s; paints in between draw only the
		// background. A successful load clears it; so does BuildNewImageThumbs giving the canvas a new build,
		// except for an external presentation whose picture path and file are unchanged (a resize or re-layout
		// keeps the pause, since each attempt may be a PowerPoint export).
		private static readonly int[] RetryDelaysMs = { 500, 2000 };
		private const int FailedRetryPauseMs = 30000;
		private int _failureCount;
		private long _retryAtTicks; // Environment.TickCount64: no attempt before it
		private System.Windows.Forms.Timer _retryTimer;

		// The canvas shows the first slide of an external presentation (PowerPoint tab, thumbnail listing): FileName
		// is its cached picture (Gf.ExternalPPT.GetFirstScreenDumpPath), exported only when it does not exist yet.
		private bool _isExternalPresentationPreview;
		// An export of this canvas is running: a paint re-entered meanwhile (PowerPoint calls pump messages) does
		// not start another one.
		private bool _exporting;

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

		// Display markers. The border is baked into the rendered thumbnail, so a change that can
		// alter what is drawn re-renders it from the cached source image on the next paint.
		// A change that cannot alter this thumbnail (for example the selected-border width of an
		// unselected thumbnail) only stores the value; the next full render uses it.
		private Color _selectedBorderColor = Color.Red;
		private bool _thinSelectedBorder;
		private bool _isNextSlide;
		private Color _nextSlideBorderColor = ModernUiPalette.NextSlide;
		private bool _markersDirty;

		// True when this thumbnail is drawn with the selected border. DrawBorder uses this same
		// test. A canvas that was never built has both numbers at 0 and also counts as selected
		// here, but draws nothing; callers that look for the selected slide skip unbuilt canvases.
		[Browsable(false)]
		public bool IsDrawnSelected => CurrentSelectedSlide == PowerPointSlideNumbering;

		// Border colour of the selected PowerPoint thumbnail. Red is the previous look;
		// FrmMain overrides it per column when the modern look is enabled.
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Color SelectedBorderColor
		{
			get => _selectedBorderColor;
			set
			{
				if (_selectedBorderColor.ToArgb() != value.ToArgb())
				{
					_selectedBorderColor = value;
					if (IsDrawnSelected)
					{
						MarkMarkersDirty();
					}
				}
			}
		}

		// Draws the selected border at a third of its normal visible width, at least 3 screen
		// pixels (live column, slide selected but not actually on the screen).
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool ThinSelectedBorder
		{
			get => _thinSelectedBorder;
			set
			{
				if (_thinSelectedBorder != value)
				{
					_thinSelectedBorder = value;
					if (IsDrawnSelected)
					{
						MarkMarkersDirty();
					}
				}
			}
		}

		// Draws a dashed border in NextSlideBorderColor (the slide after the selected one).
		// Not drawn on the selected thumbnail itself.
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool IsNextSlide
		{
			get => _isNextSlide;
			set
			{
				if (_isNextSlide != value)
				{
					_isNextSlide = value;
					if (!IsDrawnSelected)
					{
						MarkMarkersDirty();
					}
				}
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Color NextSlideBorderColor
		{
			get => _nextSlideBorderColor;
			set
			{
				if (_nextSlideBorderColor.ToArgb() != value.ToArgb())
				{
					_nextSlideBorderColor = value;
					if (_isNextSlide && !IsDrawnSelected)
					{
						MarkMarkersDirty();
					}
				}
			}
		}
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

				if (canvasRatio < ImageRatio)
				{
					PosWidth = base.Width;
					PosHeight = (int)Math.Round(PosWidth / ImageRatio);
					PosLeft = 0;
					PosTop = (base.Height - PosHeight) / 2;
				}
				else
				{
					PosHeight = base.Height;
					PosWidth = (int)Math.Round(PosHeight * ImageRatio);
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
			SetStyle(ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.Selectable, value: true);
			Cursor = Cursors.Hand;
			base.Visible = false;
			TabStop = true;
		}

		protected override bool IsInputKey(Keys keyData)
		{
			if (keyData == Keys.Up || keyData == Keys.Down)
			{
				return true;
			}
			return base.IsInputKey(keyData);
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			if (!base.Visible)
				return;
			if (Image == null && string.IsNullOrEmpty(FileName))
				return;

			e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
			e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
			e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

			if (!IsImageReady && (RetryPending() || !CalculateImage()))
			{
				// Nothing to show (yet): the background, not whatever the screen held there before.
				e.Graphics.Clear(BackColor);
				return;
			}
			if (_markersDirty && !RerenderMarkers())
			{
				e.Graphics.Clear(BackColor);
				return;
			}

			DrawImage(e);
		}

		private bool RetryPending()
		{
			return _failureCount > 0 && Environment.TickCount64 < _retryAtTicks;
		}

		// A failed attempt: the next one waits 0.5 s, 2 s, then 30 s each time; the timer repaints then,
		// so the thumbnail recovers without any input.
		private void RememberFailure()
		{
			_failureCount++;
			int delay = (_failureCount <= RetryDelaysMs.Length) ? RetryDelaysMs[_failureCount - 1] : FailedRetryPauseMs;
			_retryAtTicks = Environment.TickCount64 + delay;
			if (IsDisposed)
			{
				return;
			}
			if (_retryTimer == null)
			{
				_retryTimer = new System.Windows.Forms.Timer();
				_retryTimer.Tick += RetryTimer_Tick;
			}
			_retryTimer.Stop();
			_retryTimer.Interval = delay;
			_retryTimer.Start();
		}

		private void RetryTimer_Tick(object sender, EventArgs e)
		{
			_retryTimer?.Stop();
			if (!IsDisposed && IsHandleCreated && !IsImageReady)
			{
				Invalidate();
			}
		}

		private void ClearFailures()
		{
			_failureCount = 0;
			_retryAtTicks = 0;
			_retryTimer?.Stop();
		}

		private void MarkMarkersDirty()
		{
			_markersDirty = true;
			if (base.Visible && IsHandleCreated && !IsDisposed)
			{
				Invalidate();
			}
		}

		// Re-renders the thumbnail from the cached source image so only the border changes.
		// No file access and no PowerPoint export. Falls back to a full reload when the cache
		// does not belong to the current file.
		// Returns false when there is nothing to draw.
		private bool RerenderMarkers()
		{
			_markersDirty = false;
			if (_cachedSourceImage == null || _cachedFileName != FileName)
			{
				IsImageReady = false;
				return CalculateImage();
			}
			try
			{
				if (TryCalculateTargetSizes(_cachedSourceImage, out int storedImageWidth, out int storedImageHeight))
				{
					Bitmap rendered = RenderImage(_cachedSourceImage, storedImageWidth, storedImageHeight);
					Image?.Dispose();
					Image = rendered;
					ResizeCanvas(base.Width, base.Height);
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error in RerenderMarkers: {ex.Message}");
			}
			return Image != null;
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

				IsExternalPowerPoint = NewExternalPowerPoint;
				CurrentSelectedSlide = NewCurSelectedSlide;

				_isExternalPresentationPreview = false;
				if (IsPowerPoint)
				{
					SlideNumber = PowerPointSlideNumbering;
				}
				else
				{
					SlideNumber = 0;
					if (extension == ".ppt" || extension == ".pptx")
					{
						// The cached picture of the presentation's current version (null: not listed or unreadable).
						// It is looked up only here: a presentation edited while its thumbnail stays on screen keeps
						// the old picture until the next rebuild (listing, re-layout, resize).
						imagePath = Gf.ExternalPPT.GetFirstScreenDumpPath(NewCanvasIndex, imagePath) ?? "";
						_isExternalPresentationPreview = true;
					}
				}

				// Re-render when (a) the path changed, (b) always for external PPT (re-rendered from the
				// cached picture; exported only when it is missing), or (c) the same path was re-built on
				// disk (mtime advanced past our cache).
				bool pathChanged = FileName != imagePath;
				bool fileTouched = !pathChanged
				                   && !string.IsNullOrEmpty(imagePath)
				                   && File.Exists(imagePath)
				                   && new FileInfo(imagePath).LastWriteTimeUtc != _cachedFileTime;
				int width = Math.Max(1, NewWidth);
				int height = UsesSlideAspect && _cachedSourceImage != null && _cachedFileName == imagePath
					? HeightForSlideImage(width, _cachedSourceImage)
					: Math.Max(1, NewHeight);
				Size = new Size(width, height);
				if (pathChanged || fileTouched || IsExternalPowerPoint)
				{
					FileName = imagePath;
					IsImageReady = false;
					// A new build (new path, rebuilt file, or the listing redone): try it now, even after a failure.
					// An external presentation whose picture is unchanged (a resize, a re-layout) keeps its retry
					// pause: each attempt may be a PowerPoint export.
					if (!_isExternalPresentationPreview || pathChanged || fileTouched)
					{
						ClearFailures();
					}
					BackColor = NewBackColour;
					BorderWidthFactor = 0.04f;
					base.Visible = true;

					// An external presentation shows its own path, not the cache picture's hashed name.
					string toolTipText = _isExternalPresentationPreview ? NewImageArray[NewCanvasIndex] : imagePath;
					NewToolTip.SetToolTip(this, SlideNumber > 0 ? SlideNumber.ToString() : toolTipText);

					PowerPointSlideNumbering = slideNumbering;

					// ResizeCanvas는 CalculateImage에서 이미지 로드 후 호출됨
					// ResizeCanvas(base.Width, base.Height);

					Invalidate();
				}
				else if (IsImageReady)
				{
					ResizeCanvas(width, height);
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
			bool isSelected = IsDrawnSelected;
			Color surround = Parent?.BackColor ?? BackColor;
			Color borderColor = isSelected ? SelectedBorderColor : surround;
			using (Pen pen = new Pen(borderColor, fontSize))
			{
				graphics.DrawRectangle(pen, rect);
			}
			if (!ThinSelectedBorder && !IsNextSlide)
			{
				return;
			}

			// The pen above is centred on the bitmap edge, so half of it is visible: that is
			// the thick selected band. The markers below are measured against it and drawn
			// inset, so their whole width lies inside the bitmap.
			float band = fontSize / 2f;
			if (isSelected && ThinSelectedBorder)
			{
				using (Pen backPen = new Pen(surround, fontSize))
				{
					graphics.DrawRectangle(backPen, rect);
				}
				float thinWidth = MarkerWidth(band, 3f, ThinBorderMinScreenPixels);
				using (Pen thinPen = new Pen(borderColor, thinWidth))
				{
					DrawInsetRectangle(graphics, thinPen, rect, thinWidth);
				}
			}
			else if (!isSelected && IsNextSlide)
			{
				float dashWidth = MarkerWidth(band, 5f, NextSlideMinScreenPixels);
				using (Pen dashPen = new Pen(NextSlideBorderColor, dashWidth))
				{
					// Dash and gap lengths are multiples of the pen width.
					dashPen.DashPattern = new float[] { 4f, 3f };
					DrawInsetRectangle(graphics, dashPen, rect, dashWidth);
				}
			}
		}

		// Minimum visible marker widths in screen pixels (design D9, D12).
		private const float ThinBorderMinScreenPixels = 3f;
		private const float NextSlideMinScreenPixels = 2f;

		// A fraction of the thick band, at least the given screen pixels (the thumbnail is rendered
		// at RESOLUTION_MULTIPLIER times its screen size), never wider than the band itself.
		private static float MarkerWidth(float band, float divisor, float minScreenPixels)
		{
			float width = Math.Max(band / divisor, minScreenPixels * RESOLUTION_MULTIPLIER);
			return Math.Min(width, band);
		}

		private static void DrawInsetRectangle(Graphics graphics, Pen pen, Rectangle rect, float width)
		{
			float inset = width / 2f;
			graphics.DrawRectangle(pen, rect.X + inset, rect.Y + inset, rect.Width - width, rect.Height - width);
		}
		private bool ShouldDrawPowerPointOverlay()
		{
			return IsPowerPoint || IsExternalPowerPoint;
		}

		private bool UsesSlideAspect => IsPowerPoint || _isExternalPresentationPreview;

		private static int HeightForSlideImage(int width, Image source) =>
			Math.Max(1, (int)Math.Round((double)width * source.Height / source.Width));

		private void ConfigureGraphics(Graphics graphics)
		{
			graphics.SmoothingMode = SmoothingMode.AntiAlias;
			graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
			graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
		}

		private int CalculateBorderMargin(int storedImageWidth, int storedImageHeight)
		{
			int baseSize = storedImageWidth >= storedImageHeight ? storedImageWidth : storedImageHeight;
			return (int)(baseSize * BorderWidthFactor);
		}

		private bool TryCalculateTargetSizes(System.Drawing.Image sourceImage, out int storedImageWidth, out int storedImageHeight)
		{
			int newImageWidth = 0;
			int newImageHeight = 0;
			storedImageWidth = 0;
			storedImageHeight = 0;

			SetImageRatio(sourceImage.Width, sourceImage.Height);
			Gf.CalcImageToFit(ImageRatio, base.Width, base.Height, ref newImageWidth, ref newImageHeight, true, ref storedImageWidth, ref storedImageHeight);

			if (newImageWidth <= 0 || newImageHeight <= 0)
			{
				return false;
			}

			storedImageWidth = (int)(storedImageWidth * RESOLUTION_MULTIPLIER);
			storedImageHeight = (int)(storedImageHeight * RESOLUTION_MULTIPLIER);

			return storedImageWidth > 0 && storedImageHeight > 0;
		}

		/// <summary>
		/// margin 클램프와 inner size guard만 유지
		/// </summary>
		/// <param name="sourceImage"></param>
		/// <param name="storedImageWidth"></param>
		/// <param name="storedImageHeight"></param>
		/// <returns></returns> <summary>

		private Bitmap RenderImage(System.Drawing.Image sourceImage, int storedImageWidth, int storedImageHeight)
		{
			Bitmap renderedImage = new(storedImageWidth, storedImageHeight, PixelFormat.Format24bppRgb);
			using Graphics graphics = Graphics.FromImage(renderedImage);

			ConfigureGraphics(graphics);

			Rectangle fullRect = new(0, 0, storedImageWidth, storedImageHeight);

			if (ShouldDrawPowerPointOverlay())
			{
				float fontSize = storedImageWidth / FONT_SIZE_RATIO;
				int margin = CalculateBorderMargin(storedImageWidth, storedImageHeight);
				if (UsesSlideAspect)
				{
					graphics.DrawImage(sourceImage, fullRect);
					DrawBorder(graphics, fullRect, fontSize);
					DrawSlideNumber(graphics, storedImageWidth, storedImageHeight, margin);
					return renderedImage;
				}
				int maxMargin = (Math.Min(storedImageWidth, storedImageHeight) - 1) / 2;
				if (maxMargin < 0)
				{
					maxMargin = 0;
				}
				if (margin > maxMargin)
				{
					margin = maxMargin;
				}

				int innerWidth = storedImageWidth - 2 * margin;
				int innerHeight = storedImageHeight - 2 * margin;
				if (innerWidth <= 0 || innerHeight <= 0)
				{
					graphics.DrawImage(sourceImage, fullRect);
					return renderedImage;
				}

				DrawBorder(graphics, fullRect, fontSize);
				Rectangle imageRect = new(margin, margin, innerWidth, innerHeight);
				graphics.DrawImage(sourceImage, imageRect);

				DrawSlideNumber(graphics, storedImageWidth, storedImageHeight, margin);
			}
			else
			{
				graphics.DrawImage(sourceImage, fullRect);
			}

			return renderedImage;
		}

        public bool CalculateImage()
		{
			try
			{
				if (_isExternalPresentationPreview && !string.IsNullOrEmpty(FileName) && !File.Exists(FileName))
				{
					if (_exporting)
					{
						// The outer attempt of this canvas is still exporting; it draws the result.
						return false;
					}
					// No picture of this version yet: export it once. When it is not made (a failure, or refused
					// because another thumbnail's export is running), the missing-file check below calls
					// RememberFailure, so the retry timer tries again.
					_exporting = true;
					try
					{
						Gf.ExternalPPT.BuildOneFirstScreenDump(PowerPointSlideNumbering);
					}
					finally
					{
						_exporting = false;
					}
				}

				// PowerPoint Interop's Slide.Export returns before the JPG is guaranteed to be visible
				// (AV scanners, file system flush): a missing file is retried by the timer (RememberFailure)
				// instead of sleeping in paint.
				if (string.IsNullOrEmpty(FileName) || !File.Exists(FileName))
				{
					Console.WriteLine($"Image file not found: {FileName}");
					IsImageReady = false;
					RememberFailure();
					return false;
				}

				// Get file modification time
				var fileInfo = new FileInfo(FileName);
				var currentFileTime = fileInfo.LastWriteTimeUtc;

				// Check if we can use cached source image (same file name AND same modification time)
				System.Drawing.Image sourceImage;
				if (_cachedFileName == FileName && _cachedFileTime == currentFileTime && _cachedSourceImage != null)
				{
					sourceImage = _cachedSourceImage;
					Console.WriteLine($"[ImageCanvas Cache Hit] Using cached image: {Path.GetFileName(FileName)}");
				}
				else
				{
					// Clear old cached image if filename changed or file was modified. Forget it entirely: if the
					// load below throws, a later visit to the old file must not take the disposed bitmap as a hit.
					_cachedSourceImage?.Dispose();
					_cachedSourceImage = null;
					_cachedFileName = null;

					// MSDN: Image.FromStream requires the stream to remain open for the Image's
					// lifetime. Clone into a standalone Bitmap so the file/stream can be released
					// safely and DrawImage cannot fail with GDI+ generic errors on lazy pixel reads.
					try
					{
						using (var stream = new FileStream(FileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
						using (var tempImage = System.Drawing.Image.FromStream(stream))
						{
							_cachedSourceImage = new Bitmap(tempImage);
						}
					}
					catch (Exception ex) when (_isExternalPresentationPreview && (ex is ArgumentException || ex is OutOfMemoryException))
					{
						// GDI+ could not decode the cached picture: delete it so the retry exports it again
						// instead of loading the same broken file forever.
						Gf.ExternalPPT.DiscardFirstScreenDump(FileName);
						throw;
					}
					_cachedFileName = FileName;
					_cachedFileTime = currentFileTime;
					sourceImage = _cachedSourceImage;
					Console.WriteLine($"[ImageCanvas Cache Miss] Loaded new image: {Path.GetFileName(FileName)} (Modified: {currentFileTime})");
				}

				if (UsesSlideAspect)
					Height = HeightForSlideImage(Width, sourceImage);

				if (!TryCalculateTargetSizes(sourceImage, out int storedImageWidth, out int storedImageHeight))
				{
					return false;
				}

				// Render the thumbnail and assign directly (no Clone needed)
				Image?.Dispose(); // Dispose old image
				Image = RenderImage(sourceImage, storedImageWidth, storedImageHeight);

				IsImageReady = true;
				_markersDirty = false;
				ClearFailures();
				ResizeCanvas(base.Width, base.Height);
			}
			catch (Exception ex)
			{
				// Also a JPG still locked or half written by the export: retried like a missing one.
				Console.WriteLine($"Error in CalculateImage: {ex.Message}");
				IsImageReady = false;
				RememberFailure();
				return false;
			}
			return true;
		}

        protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				_retryTimer?.Dispose();
				_retryTimer = null;
				Image?.Dispose();
				Image = null;
				_cachedSourceImage?.Dispose();
				_cachedSourceImage = null;
				_cachedFileName = null;
			}
			base.Dispose(disposing);
		}
	}
}
