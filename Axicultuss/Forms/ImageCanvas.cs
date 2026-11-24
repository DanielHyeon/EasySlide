using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;

namespace Axicultus;

	internal class ImageCanvas : Control, IDisposable
	{
		// Constants
		private const float FONT_SIZE_RATIO = 12.5f;
		private const float SMALL_FONT_SIZE_RATIO = 24f;
		private const float RESOLUTION_MULTIPLIER = 2f;
		private const int SINGLE_DIGIT_OFFSET = 5;
		private const int MULTI_DIGIT_OFFSET = 12;

		// Content scaled for this thumbnail, without selection markers or the slide number.
		private Bitmap _thumbnailContent;
		private Size _sourceSize;
		private Rectangle _contentBounds;
		private long _cachedFileLength;
		private string _cachedFileName;
		private DateTime _cachedFileTime;

		// Retries of a picture that could not be made (JPG not there yet or still being written by the
		// PowerPoint export, a failed export). Every repaint (hover, resize, scroll) used to try again: a
		// PowerPoint export plus up to 150 ms of sleeping on the UI thread each time. Now a failed attempt
		// is retried by a UI timer after 0.5 s and 2 s, then every 30 s; paints in between draw only the
		// background. A successful load clears it; so does BuildNewImageThumbs giving the canvas a new build,
		// except for an external presentation whose picture path and file are unchanged (a resize or re-layout
		// keeps the pause, since each attempt may be a PowerPoint export).
		private static readonly int[] RetryDelaysMs = [500, 2000];
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

		internal void UpdateSlideSelection(int slide)
		{
			bool wasSelected = IsDrawnSelected;
			CurrentSelectedSlide = slide;
			if (IsDrawnSelected != wasSelected)
				MarkMarkersDirty();
		}

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

				if (FramesSlide)
				{
					// The canvas height follows the framed raster, so it covers the whole control; aspect
					// rounding must not leave a background line outside the selection band.
					(PosLeft, PosTop, PosWidth, PosHeight) = (0, 0, base.Width, base.Height);
					return;
				}

				float canvasRatio = (float)base.Width / base.Height;

				if (canvasRatio < ImageRatio)
				{
					PosWidth = base.Width;
					PosHeight = Math.Max(1, (int)Math.Round(PosWidth / ImageRatio));
					PosLeft = 0;
					PosTop = (base.Height - PosHeight) / 2;
				}
				else
				{
					PosHeight = base.Height;
					PosWidth = Math.Max(1, (int)Math.Round(PosHeight * ImageRatio));
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
			if (keyData is Keys.Up or Keys.Down)
			{
				return true;
			}
			return base.IsInputKey(keyData);
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		internal bool DrawGallerySelection { get; init; }

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
				if (!IsDisposed && !Disposing) e.Graphics.Clear(BackColor);
				return;
			}
			// Resized without a rebuild, or moved to another DPI. A framed thumbnail of a new width is made again
			// from its slide picture. Otherwise (or when the picture is gone or being re-exported) the slide number
			// is redrawn at its on-screen size over the cached raster, placed for the new size.
			// shortcut: a cached raster kept for a new width keeps the frame made for the old one, so the slide's
			// aspect inside the band can drift by about a pixel until the next rebuild.
			if (BadgeSizeStale && !(FramesSlide && _badgeCanvasSize.Width != Width && RebuildForNewWidth()))
			{
				_markersDirty = true;
				ResizeCanvas(Width, Height);
			}
			if (_markersDirty && !RerenderMarkers())
			{
				e.Graphics.Clear(BackColor);
				return;
			}

			DrawImage(e);
			if (DrawGallerySelection && IsDrawnSelected)
				ControlPaint.DrawBorder(e.Graphics, ClientRectangle, SystemColors.Highlight, ButtonBorderStyle.Solid);
		}

		// Makes a framed thumbnail's raster again for its new width from its slide picture (decoded again only
		// when the raster size changed). False when the picture is gone or cannot be read: the cached raster is
		// then kept.
		private bool RebuildForNewWidth()
		{
			if (string.IsNullOrEmpty(FileName) || !File.Exists(FileName))
				return false;
			try
			{
				if (!PrepareThumbnail(new FileInfo(FileName)))
					return false;
				DrawThumbnail();
				_markersDirty = false;
				ResizeCanvas(Width, Height);
				return true;
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"Thumbnail rebuild for a new width failed: {ex.Message}");
				return false;
			}
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

		private bool RerenderMarkers()
		{
			if (_thumbnailContent == null || _cachedFileName != FileName)
				return CalculateImage();
			try
			{
				DrawThumbnail();
				_markersDirty = false;
				return true;
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"Thumbnail marker redraw failed: {ex.Message}");
				IsImageReady = false;
				RememberFailure();
				return false;
			}
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
			if (NewImageArray is null || (uint)NewCanvasIndex >= (uint)NewImageArray.Length)
				return;
			BuildImageThumbnail(NewCanvasIndex, NewWidth, NewHeight, NewImageArray[NewCanvasIndex], NewCurSelectedSlide, NewBackColour, NewToolTip, NewExternalPowerPoint);
		}

		internal void BuildImageThumbnail(int canvasIndex, int requestedWidth, int requestedHeight, string sourcePath,
			int selectedSlide, Color background, ToolTip toolTip, bool externalPowerPoint)
		{
			try
			{
				int slideNumbering = canvasIndex + 1;
				string extension = Path.GetExtension(sourcePath).ToLowerInvariant();
				string imagePath = sourcePath;

				IsExternalPowerPoint = externalPowerPoint;
				CurrentSelectedSlide = selectedSlide;

				_isExternalPresentationPreview = false;
				if (IsPowerPoint)
				{
					SlideNumber = slideNumbering;
				}
				else
				{
					SlideNumber = 0;
					if (extension is ".ppt" or ".pptx")
					{
						// The cached picture of the presentation's current version (null: not listed or unreadable).
						// It is looked up only here: a presentation edited while its thumbnail stays on screen keeps
						// the old picture until the next rebuild (listing, re-layout, resize).
						imagePath = Gf.ExternalPPT.GetFirstScreenDumpPath(canvasIndex, imagePath) ?? "";
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
				                   && !MatchesCachedVersion(new FileInfo(imagePath));
				int width = Math.Max(1, requestedWidth);
				int height = !KeepThumbnailCellHeight && UsesSlideAspect && _thumbnailContent != null && _cachedFileName == imagePath
					? SlideAspectHeight(width, _sourceSize)
					: Math.Max(1, requestedHeight);
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
					BackColor = background;
					BorderWidthFactor = 0.04f;
					base.Visible = true;

					// An external presentation shows its own path, not the cache picture's hashed name.
					string toolTipText = _isExternalPresentationPreview ? sourcePath : imagePath;
					toolTip.SetToolTip(this, SlideNumber > 0 ? SlideNumber.ToString() : toolTipText);

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
			string slideNumberText = PowerPointSlideNumbering.ToString();
			float offsetX = PowerPointSlideNumbering < 10 ? SINGLE_DIGIT_OFFSET : MULTI_DIGIT_OFFSET;
			PointF position = new(storedImageWidth - (margin * 2 + offsetX), margin);

        using Font font = new("Microsoft Sans Serif", storedImageWidth / SMALL_FONT_SIZE_RATIO);
        SizeF textSize = graphics.MeasureString(slideNumberText, font);

        graphics.FillRectangle(Brushes.Black, position.X - 3, position.Y + 1, textSize.Width - 2, textSize.Height - 3);
        graphics.FillRectangle(Brushes.Yellow, position.X - 4, position.Y, textSize.Width - 2, textSize.Height - 3);
        graphics.DrawString(slideNumberText, font, Brushes.Black, position.X - 5, position.Y);
    }

		private void DrawBorder(Graphics graphics, Rectangle rect, float fontSize)
		{
			bool isSelected = IsDrawnSelected;
			Color surround = Parent?.BackColor ?? BackColor;
			Color borderColor = isSelected ? SelectedBorderColor : surround;
			using (Pen pen = new(borderColor, fontSize))
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
				using (Pen backPen = new(surround, fontSize))
				{
					graphics.DrawRectangle(backPen, rect);
				}
				float thinWidth = MarkerWidth(band, 3f, ThinBorderMinScreenPixels);
            using Pen thinPen = new(borderColor, thinWidth);
            DrawInsetRectangle(graphics, thinPen, rect, thinWidth);
        }
			else if (!isSelected && IsNextSlide)
			{
				float dashWidth = MarkerWidth(band, 5f, NextSlideMinScreenPixels);
            using Pen dashPen = new(NextSlideBorderColor, dashWidth);
            // Dash and gap lengths are multiples of the pen width.
            dashPen.DashPattern = [4f, 3f];
            DrawInsetRectangle(graphics, dashPen, rect, dashWidth);
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

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		internal bool KeepThumbnailCellHeight { get; init; }

		private bool UsesSlideAspect => IsPowerPoint || _isExternalPresentationPreview;

		private static int HeightForSlideImage(int width, Size source) =>
			Math.Max(1, (int)Math.Round((double)width * source.Height / source.Width));

		private static void ConfigureGraphics(Graphics graphics)
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

		// A PowerPoint thumbnail sized by its slide (not a fixed cell): the slide fills the frame, as in WPF.
		private bool FramesSlide => IsPowerPoint && !KeepThumbnailCellHeight;

		// Canvas height of a slide at this width.
		private int SlideAspectHeight(int width, Size source) =>
			IsPowerPoint ? PowerPointThumbnailHeight(width, source) : HeightForSlideImage(width, source);

		// Height of a PowerPoint thumbnail of this width for a slide of this size. It follows the framed raster, as
		// in WPF: (width - 2 x border band) x slide aspect + 2 x border band, so the slide fills the frame on every side.
		internal static int PowerPointThumbnailHeight(int width, Size slide)
		{
			Size raster = FramedRasterSize(Math.Max(1, width), slide);
			return Math.Max(1, (int)Math.Round((double)width * raster.Height / raster.Width));
		}

		private bool TryGetThumbnailSize(Size source, out Size size)
		{
			size = Size.Empty;
			if (!KeepThumbnailCellHeight && UsesSlideAspect)
				Height = SlideAspectHeight(Width, source);
			if (Width <= 0 || Height <= 0)
				return false;

			if (FramesSlide)
			{
				size = FramedRasterSize(Width, source);
				SetImageRatio(size.Width, size.Height);
				return true;
			}
			SetImageRatio(source.Width, source.Height);
			size = StoredSize(Width, Height, source);
			return true;
		}

		private const int MaximumRasterEdge = 1024;

		private static Size StoredSize(int width, int height, Size source)
		{
			int fittedWidth = 0, fittedHeight = 0, storedWidth = 0, storedHeight = 0;
			Gf.CalcImageToFit((float)source.Width / source.Height, width, height, ref fittedWidth, ref fittedHeight, true,
				ref storedWidth, ref storedHeight);
			storedWidth = Math.Max(1, (int)(storedWidth * RESOLUTION_MULTIPLIER));
			storedHeight = Math.Max(1, (int)(storedHeight * RESOLUTION_MULTIPLIER));

			// A panorama can exceed the normal cache size even when its short edge fits.
			int longestEdge = Math.Max(storedWidth, storedHeight);
			if (longestEdge > MaximumRasterEdge)
			{
				storedWidth = Math.Max(1, (int)((long)storedWidth * MaximumRasterEdge / longestEdge));
				storedHeight = Math.Max(1, (int)((long)storedHeight * MaximumRasterEdge / longestEdge));
			}
			return new Size(storedWidth, storedHeight);
		}

		// Raster of a PowerPoint thumbnail: twice the canvas width (at most MaximumRasterEdge), and the height that
		// fits the slide exactly inside the frame inset on every side (ContentBounds gives the same inset).
		private static Size FramedRasterSize(int width, Size source)
		{
			int rasterWidth = Math.Clamp((int)(width * RESOLUTION_MULTIPLIER), 1, MaximumRasterEdge);
			Size framed = Framed(rasterWidth);
			if (framed.Height > MaximumRasterEdge)
				framed = Framed(Math.Max(1, (int)((long)rasterWidth * MaximumRasterEdge / framed.Height)));
			return framed;

			Size Framed(int w)
			{
				int inset = FrameInset(w, int.MaxValue);
				int slideHeight = Math.Max(1, (int)Math.Round((double)(w - 2 * inset) * source.Height / source.Width));
				return new Size(w, slideHeight + 2 * inset);
			}
		}

		// The visible half of the border pen, rounded up: the slide picture meets the border band without
		// going under it.
		private static int FrameInset(int width, int height) =>
			Math.Min((int)Math.Ceiling(width / FONT_SIZE_RATIO / 2), (Math.Min(width, height) - 1) / 2);

		private bool MatchesCachedVersion(FileInfo file) =>
			_cachedFileName == FileName && _cachedFileTime == file.LastWriteTimeUtc && _cachedFileLength == file.Length;

		// The WPF slide number: 12 px bold text on a #F6E600 badge at least 18 px wide (a 16 px line), the same
		// size on screen whatever the thumbnail size. Sizes are device-independent pixels.
		private const string BadgeFontName = "Segoe UI";
		private const float BadgeFontDip = 12f;
		private const float BadgeHeightDip = 16f;
		private const float BadgeMinWidthDip = 18f;
		private const float BadgePaddingDip = 1f;
		private static readonly Color BadgeColor = Color.FromArgb(0xF6, 0xE6, 0x00);

		// The number over the top-right corner of the slide picture, as in the WPF thumbnail: its top and right
		// edges on the picture's. Its size is converted from screen pixels at the width the raster is shown at
		// (the canvas width, or less in a fixed cell); on a small picture it shrinks to at most half the picture
		// each way.
		private void DrawSlideNumberOnPicture(Graphics graphics, Rectangle picture, Size raster)
		{
			float shownWidth = Math.Min(Width, (float)Height * raster.Width / raster.Height);
			float scale = (shownWidth > 0 ? raster.Width / shownWidth : 1f) * DeviceDpi / 96f;
			string text = PowerPointSlideNumbering.ToString();
			using var format = new StringFormat(StringFormat.GenericTypographic)
			{
				Alignment = StringAlignment.Center,
				LineAlignment = StringAlignment.Center,
				FormatFlags = StringFormatFlags.NoWrap
			};
			float textWidth;
			using (var measured = new Font(BadgeFontName, BadgeFontDip * scale, FontStyle.Bold, GraphicsUnit.Pixel))
				textWidth = graphics.MeasureString(text, measured, PointF.Empty, format).Width;
			float width = Math.Max(BadgeMinWidthDip * scale, textWidth + 2 * Math.Max(1f, BadgePaddingDip * scale));
			float height = BadgeHeightDip * scale;
			float fit = Math.Min(1f, Math.Min(picture.Width / 2f / width, picture.Height / 2f / height));
			int badgeWidth = (int)Math.Ceiling(width * fit);
			int badgeHeight = (int)Math.Round(height * fit);
			if (badgeHeight < 2)
				return;
			var badge = new Rectangle(picture.Right - badgeWidth, picture.Top, badgeWidth, badgeHeight);
			using var font = new Font(BadgeFontName, BadgeFontDip * scale * fit, FontStyle.Bold, GraphicsUnit.Pixel);
			using var fill = new SolidBrush(BadgeColor);
			graphics.FillRectangle(fill, badge);
			graphics.DrawString(text, font, Brushes.Black, badge, format);
		}

		private Rectangle ContentBounds(Size size, Size source)
		{
			var bounds = new Rectangle(Point.Empty, size);
			if (IsPowerPoint)
			{
				// The slide fits inside the border band, keeping its aspect; a framed raster (FramesSlide) is made to
				// fit it exactly, so it fills the frame. The number is drawn over it.
				int inset = FrameInset(size.Width, size.Height);
				int roomWidth = size.Width - 2 * inset;
				int roomHeight = size.Height - 2 * inset;
				double scale = Math.Min((double)roomWidth / source.Width, (double)roomHeight / source.Height);
				int width = Math.Clamp((int)Math.Round(source.Width * scale), 1, roomWidth);
				int height = Math.Clamp((int)Math.Round(source.Height * scale), 1, roomHeight);
				// A pixel short from rounding: fill the frame rather than leave a one-pixel line.
				if (roomWidth - width <= 1 && roomHeight - height <= 1)
					(width, height) = (roomWidth, roomHeight);
				return new Rectangle((size.Width - width) / 2, (size.Height - height) / 2, width, height);
			}
			if (ShouldDrawPowerPointOverlay() && !UsesSlideAspect)
			{
				int margin = Math.Clamp(CalculateBorderMargin(size.Width, size.Height), 0,
					(Math.Min(size.Width, size.Height) - 1) / 2);
				bounds.Inflate(-margin, -margin);
			}
			return bounds;
		}

		private void DrawThumbnail()
		{
			Size size = _thumbnailContent.Size;
			bool replace = Image?.Size != size;
			Image target = replace ? new Bitmap(size.Width, size.Height, PixelFormat.Format24bppRgb) : Image;
			try
			{
				using Graphics graphics = Graphics.FromImage(target);
				ConfigureGraphics(graphics);
				graphics.Clear(IsPowerPoint ? Parent?.BackColor ?? BackColor : Color.Black);
				bool overlay = ShouldDrawPowerPointOverlay();
				var full = new Rectangle(Point.Empty, size);
				float borderWidth = size.Width / FONT_SIZE_RATIO;
				if (overlay && !UsesSlideAspect)
					DrawBorder(graphics, full, borderWidth);
				graphics.DrawImageUnscaled(_thumbnailContent, 0, 0);
				if (overlay)
				{
					if (UsesSlideAspect)
						DrawBorder(graphics, full, borderWidth);
					if (IsPowerPoint)
						DrawSlideNumberOnPicture(graphics, _contentBounds, size);
					else
						DrawSlideNumber(graphics, size.Width, size.Height,
							UsesSlideAspect ? CalculateBorderMargin(size.Width, size.Height) : _contentBounds.X);
				}
			}
			catch
			{
				if (replace) target.Dispose();
				throw;
			}
			if (replace)
			{
				Image?.Dispose();
				Image = target;
			}
			_badgeCanvasSize = Size;
			_badgeDpi = DeviceDpi;
		}

		// The canvas size and DPI the slide number was sized for (DrawSlideNumberOnPicture): a raster drawn
		// for another size or DPI shows the badge at the wrong size on screen.
		private Size _badgeCanvasSize;
		private int _badgeDpi;

		private bool BadgeSizeStale => IsPowerPoint && IsImageReady && (_badgeCanvasSize != Size || _badgeDpi != DeviceDpi);

		protected override void OnDpiChangedAfterParent(EventArgs e)
		{
			base.OnDpiChangedAfterParent(e);
			if (BadgeSizeStale)
				MarkMarkersDirty();
		}

		private Image DecodeThumbnail(Stream stream)
		{
			try
			{
				return System.Drawing.Image.FromStream(stream);
			}
			catch (Exception ex) when (_isExternalPresentationPreview && ex is (ArgumentException or OutOfMemoryException))
			{
				// Retry by exporting a fresh copy if the cached export cannot be decoded.
				Gf.ExternalPPT.DiscardFirstScreenDump(FileName);
				throw;
			}
		}

		private bool PrepareThumbnail(FileInfo file)
		{
			bool cached = _thumbnailContent != null && MatchesCachedVersion(file);
			if (cached && TryGetThumbnailSize(_sourceSize, out Size cachedSize)
				&& _thumbnailContent.Size == cachedSize && _contentBounds == ContentBounds(cachedSize, _sourceSize))
				return true;

			using var stream = new FileStream(file.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
			using var source = DecodeThumbnail(stream);
			Size sourceSize = source.Size;
			if (!TryGetThumbnailSize(sourceSize, out Size size))
				return false;
			Rectangle bounds = ContentBounds(size, sourceSize);
			var raster = new Bitmap(size.Width, size.Height, PixelFormat.Format32bppPArgb);
			try
			{
				using Graphics graphics = Graphics.FromImage(raster);
				ConfigureGraphics(graphics);
				// GDI+ normalizes translucent channels when it detaches a decoded image.
				// Preserve those values before bicubic scaling, without retaining the full-size bitmap.
				using Bitmap normalized = (source.Flags & (int)ImageFlags.HasAlpha) != 0
					? new Bitmap(source) : null;
				graphics.DrawImage(normalized ?? source, bounds);
			}
			catch
			{
				raster.Dispose();
				throw;
			}
			_thumbnailContent?.Dispose();
			_thumbnailContent = raster;
			_sourceSize = sourceSize;
			_contentBounds = bounds;
			_cachedFileName = FileName;
			_cachedFileTime = file.LastWriteTimeUtc;
			_cachedFileLength = file.Length;
			return true;
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
					string requestedPath = FileName;
					int requestedSlide = PowerPointSlideNumbering;
					_exporting = true;
					try
					{
						Gf.ExternalPPT.BuildOneFirstScreenDump(requestedSlide);
					}
					finally
					{
						_exporting = false;
					}
					// PowerPoint pumps messages during export. Scrolling may have recycled or disposed this cell.
					if (IsDisposed || Disposing) return false;
					if (requestedSlide != PowerPointSlideNumbering || requestedPath != FileName)
					{
						Invalidate();
						return false;
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

				if (!PrepareThumbnail(new FileInfo(FileName)))
					return false;
				DrawThumbnail();
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
				_thumbnailContent?.Dispose();
				_thumbnailContent = null;
				_cachedFileName = null;
			}
			base.Dispose(disposing);
		}
	}
