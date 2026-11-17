using System;
using System.Drawing;
using System.Windows.Forms;
using Easislides.Module;

namespace Easislides
{
	internal partial class ImageTransitionControl
	{
		private enum OverlayKind
		{
			Alert,
			Reference
		}

		/// <summary>
		/// One text box drawn over the picture: the alert or the reference. Holds the box's bitmaps, its
		/// timer and its progress, builds the box (Build), runs it (Start, Tick) and draws it at every paint
		/// while it runs (Paint). The two kinds share all of it; where they differ (see OverlayKind) the
		/// code says so.
		/// Threading: Tick runs on a thread-pool thread (System.Threading.Timer). It only writes the two
		/// percentages (the alert's also expires its start time when no alert is live, as StopAlert does)
		/// and asks for a repaint through InvalidateFromTimer. Everything else runs on the UI thread, the
		/// thread that paints, so paint never sees a bitmap being replaced or disposed.
		/// The bitmaps are this overlay's own: made by Build, disposed by ReleaseImages (called by StartAlert /
		/// LoadRef before a new setup and after a failed one, and by Build itself so it never overwrites live
		/// bitmaps) and Dispose. They are not part of the control's _ownImages.
		/// </summary>
		private sealed class TextOverlay : IDisposable
		{
			// Seconds the whole message stays shown after each pass (both kinds).
			private const int GapDuration = 7;

			private readonly ImageTransitionControl _owner;

			private readonly OverlayKind _kind;

			// Alert: 6 characters a second and 20 flash frames; reference: 8 and 28.
			private readonly float _initialCharPerSecond;

			private readonly int _flashCountMax;

			private Image _background;

			private Image _message;

			private Image _backgroundInverse;

			private Image _messageInverse;

			// Used while Build draws only.
			private Graphics _backgroundGraphics;

			private Graphics _messageGraphics;

			private Graphics _backgroundInverseGraphics;

			private Graphics _messageInverseGraphics;

			// Layout in picture coordinates (paint scales it to the control).
			private int _messageOriginX = 0;

			private int _messageOriginY = 0;

			private int _originY = 0;

			private Rectangle _overallBackGroundRect = default(Rectangle);

			private Rectangle _messageBackGroundRect = default(Rectangle);

			private bool _scroll = false;

			private bool _flash = false;

			private int _occurrences = 0;

			private float _charPerSecond;

			private bool _gapRunning = false;

			private int _flashCount = 0;

			private bool _running = false;

			private DateTime _startTime;

			private DateTime _messageStartTime;

			private DateTime _gapStartTime;

			// Written by Tick (thread-pool thread), read by paint.
			private float _overallPercentage = 100f;

			private float _messagePercentage = 100f;

			private System.Threading.Timer _timer;

			private TimeSpan _transitionTime = new TimeSpan(0, 0, 0, 1, 0);

			private TimeSpan _messageTransitionTime = new TimeSpan(0, 0, 0, 1, 0);

			public TextOverlay(ImageTransitionControl owner, OverlayKind kind)
			{
				_owner = owner;
				_kind = kind;
				_initialCharPerSecond = (kind == OverlayKind.Alert) ? 6f : 8f;
				_flashCountMax = (kind == OverlayKind.Alert) ? 20 : 28;
				_charPerSecond = _initialCharPerSecond;
			}

			public bool Running => _running;

			public float OverallPercentage => _overallPercentage;

			public float TransitionTime
			{
				get
				{
					return Convert.ToSingle(_transitionTime.TotalSeconds);
				}
				set
				{
					_transitionTime = new TimeSpan(0, 0, 0, 0, Convert.ToInt32(1000f * value));
				}
			}

			public float MessageTransitionTime
			{
				get
				{
					return Convert.ToSingle(_messageTransitionTime.TotalSeconds);
				}
				set
				{
					_messageTransitionTime = new TimeSpan(0, 0, 0, 0, Convert.ToInt32(1000f * value));
				}
			}

			/// <summary>
			/// Builds the box for <paramref name="text"/> on the owner's new background picture: draws the
			/// bitmaps and sets the layout and timing. Does not start it (Start). Throws when it cannot be
			/// built (no background picture, no font, a font too small to make); the caller then releases
			/// what was made. UI thread only.
			/// </summary>
			public void Build(SongSettings item, string text, int duration, Font font, bool scroll, bool flash, bool transparent, bool shadow, bool outline, Color textColour, Color backColour, int alignment, int verticalAlignment)
			{
				// The callers release first already; this keeps a later caller from leaking the previous bitmaps.
				ReleaseImages();
				Image picture = _owner._newBackgroundPicture;
				int pictureWidth = picture.Width;
				int pictureHeight = picture.Height;
				int width = pictureWidth;
				// Height the text may take, from the bottom border. The reference computes it in float
				// (3.1f, 1.1f), as it always has; 1.1 x is its margin for vertical alignment 1.
				int heightLimit;
				int referenceMargin = 0;
				if (_kind == OverlayKind.Alert)
				{
					heightLimit = (int)(Gf.MinBottomBorderFactor * (double)pictureHeight * 1.2);
				}
				else
				{
					referenceMargin = (int)(Gf.MinBottomBorderFactor * (double)(float)pictureHeight * 1.1000000238418579);
					heightLimit = (int)(Gf.MinBottomBorderFactor * (double)(float)pictureHeight * 3.0999999046325684);
				}
				if (width < 1)
				{
					width = 1;
				}
				if (heightLimit < 1)
				{
					heightLimit = 1;
				}
				_scroll = scroll;
				_flash = flash;
				// A new box never inherits the previous one's gap or flashing (a box replaced while it was
				// flashing kept flashing in the new one's first gap, even with flashing off).
				_gapRunning = false;
				_flashCount = 0;
				int border = width / 20;
				// Made for this setup only and disposed at its end. Nothing keeps it: the text is drawn into
				// the bitmaps here and only sizes are stored. ReduceFontToFit may replace the font with one of
				// Gf's rendering cache, which is not disposed here.
				Font setupFont = null;
				try
				{
					// The alert's background bitmaps have a fixed height (the limit + 6, or the box height when that
					// is taller) and are made first; the reference's are exactly the box height, so it measures on
					// a bitmap of its own first.
					if (_kind == OverlayKind.Alert)
					{
						CreateBackgrounds(width, heightLimit + 6);
					}
					else
					{
						_background = new Bitmap(width, heightLimit);
						_backgroundGraphics = Graphics.FromImage(_background);
					}
					setupFont = new Font(font.Name, font.Size - 1f, font.Style);
					font = setupFont;
					// Width less a border on each side, as the right-aligned layout below uses.
					int textHeight;
					if (_kind == OverlayKind.Alert)
					{
						textHeight = (int)((double)Gf.ReduceFontToFit(_backgroundGraphics, text, ref font, width - border * 2, heightLimit - 2) * 1.1);
					}
					else
					{
						textHeight = Gf.ReduceFontToFit(_backgroundGraphics, text, ref font, width - border * 2, heightLimit);
					}
					_messageBackGroundRect = new Rectangle(0, 0, (int)_backgroundGraphics.MeasureString(text, font).Width, textHeight);
					int boxHeight = textHeight + 4;
					if (_kind == OverlayKind.Reference || boxHeight > _background.Height)
					{
						// The reference's always; the alert's only when its box came out taller than the fixed
						// height (a large font: text up to 1.1 x the limit), whose bottom rows then showed the
						// picture instead of the box. Normal alerts keep their bitmaps, so they paint as before.
						_backgroundGraphics.Dispose();
						_backgroundGraphics = null;
						_background.Dispose();
						_background = null;
						_backgroundInverseGraphics?.Dispose();
						_backgroundInverseGraphics = null;
						_backgroundInverse?.Dispose();
						_backgroundInverse = null;
						CreateBackgrounds(width, boxHeight);
					}

					// The text goes on the normal message bitmap; the inverse one stays background-only. The
					// bitmap is exactly the text height with the text at the top: paint draws the text-height rows,
					// so text drawn lower lost its bottom rows (the reference's before e38fd19; the alert's, drawn
					// 2 px down in a box-height bitmap, until the alert got the same). The alert's message is placed
					// those 2 px lower in the box instead (_messageOriginY), so its text stays where it was.
					int messageHeight = textHeight;
					_message = new Bitmap(_messageBackGroundRect.Width, messageHeight);
					_messageGraphics = Graphics.FromImage(_message);
					_messageGraphics.Clear(transparent ? Color.Transparent : backColour);

					_messageInverse = new Bitmap(_messageBackGroundRect.Width, messageHeight);
					_messageInverseGraphics = Graphics.FromImage(_messageInverse);
					_messageInverseGraphics.Clear(transparent ? Color.Transparent : backColour);

					Gf.OutputOneLineToScreen(item, text, font, _messageGraphics, textColour, StringAlignment.Center, shadow ? 1 : 0, outline ? 1 : 0, 0, 0, _messageBackGroundRect.Width, 0);

					// Vertical alignment. Alert: 0 top, anything else bottom. Reference: 0 top, 2 bottom,
					// anything else bottom raised by twice its margin.
					int top;
					if (_kind == OverlayKind.Alert)
					{
						top = (verticalAlignment != 0) ? (pictureHeight - boxHeight) : 0;
					}
					else
					{
						switch (verticalAlignment)
						{
						case 0:
							top = 0;
							break;
						case 2:
							top = pictureHeight - boxHeight;
							break;
						default:
							top = pictureHeight - (referenceMargin * 2 + boxHeight);
							break;
						}
					}
					Rectangle box = default(Rectangle);
					box.Y = top;
					box.Height = boxHeight;
					if (alignment == 3)
					{
						box.X = width - (_messageBackGroundRect.Width + border * 2);
						box.Width = _messageBackGroundRect.Width + border * 2;
						_messageOriginX = box.X + _messageBackGroundRect.Width + border / 2;
					}
					else if (alignment == 1)
					{
						box.X = 0;
						box.Width = _messageBackGroundRect.Width + border * 2;
						_messageOriginX = box.X + _messageBackGroundRect.Width + border;
					}
					else
					{
						box.X = 0;
						box.Width = pictureWidth;
						_messageOriginX = (pictureWidth - _messageBackGroundRect.Width) / 2 + _messageBackGroundRect.Width + border / 2;
					}
					// The text 1 px below the box top (reference) or 3 px (alert: 1 + the 2 px it was drawn down in
					// its old message bitmap). Both leave the text inside the box (text height + 4).
					_messageOriginY = box.Top + ((_kind == OverlayKind.Alert) ? 3 : 1);
					_originY = box.Top;
					_overallBackGroundRect = new Rectangle(0, 0, width, boxHeight);
					using (SolidBrush brush = new SolidBrush(transparent ? Color.Transparent : backColour))
					{
						_backgroundGraphics.FillRectangle(brush, box.X, 0, box.Width, box.Height);
						_backgroundInverseGraphics.FillRectangle(brush, box.X, 0, box.Width, box.Height);
					}
					// Setup is finished: the Graphics objects are no longer needed, the bitmaps are.
					DisposeGraphics();
					TransitionTime = duration;
					float messageDuration = MessageTransitionTime;
					ComputeOccurrences(text, TransitionTime, ref messageDuration);
					if (_kind == OverlayKind.Reference)
					{
						// The reference passes once per showing.
						_occurrences = 1;
					}
					MessageTransitionTime = messageDuration;
					_gapRunning = false;
				}
				finally
				{
					setupFont?.Dispose();
				}
			}

			private void CreateBackgrounds(int width, int height)
			{
				_background = new Bitmap(width, height);
				_backgroundGraphics = Graphics.FromImage(_background);
				_backgroundGraphics.Clear(Color.Transparent);

				_backgroundInverse = new Bitmap(width, height);
				_backgroundInverseGraphics = Graphics.FromImage(_backgroundInverse);
				_backgroundInverseGraphics.Clear(Color.Transparent);
			}

			private void ComputeOccurrences(string text, float overallDuration, ref float messageDuration)
			{
				_charPerSecond = _initialCharPerSecond;
				int num = (int)((float)text.Length / _charPerSecond) + GapDuration;
				if ((float)num > overallDuration)
				{
					// One pass and the gap do not fit: a single pass, sped up to end when the gap starts
					// (duration - gap). When the gap alone takes the whole duration there is no such time (it
					// made the speed negative); the pass then gets the whole duration. Never slower than the
					// configured speed, and the configured speed when the duration is not positive.
					float passTime = overallDuration - (float)GapDuration;
					if (passTime <= 0f)
					{
						passTime = overallDuration;
					}
					if (passTime > 0f)
					{
						_charPerSecond = Math.Max(_initialCharPerSecond, (float)text.Length / passTime);
					}
					_occurrences = 1;
				}
				else
				{
					_occurrences = (int)(overallDuration / (float)num);
					if (_occurrences == 0)
					{
						_occurrences = 1;
					}
				}
				// Positive: the speed is at least the configured one.
				messageDuration = (float)text.Length / _charPerSecond;
			}

			/// <summary>Shows the box from the start and starts its timer. UI thread only.</summary>
			public void Start()
			{
				_overallPercentage = 0f;
				_messagePercentage = 0f;
				// From the first pass: no gap or flashing left over from an earlier showing (RefGo restarts
				// the same reference at each first showing without building it again).
				_gapRunning = false;
				_flashCount = 0;
				_running = true;
				_startTime = DateTime.Now;
				_messageStartTime = DateTime.Now;
				// Dispose previous timer to prevent multiple timers running
				_timer?.Dispose();
				_timer = new System.Threading.Timer(Tick, null, 150, 150);
			}

			private void Tick(object state)
			{
				if (_kind == OverlayKind.Alert && (!Gf.ParentalAlertLive & !Gf.MessageAlertLive))
				{
					Expire();
				}
				TimeSpan timeSpan = DateTime.Now.Subtract(_startTime);
				_overallPercentage = Convert.ToSingle(100.0 / _transitionTime.TotalSeconds * timeSpan.TotalSeconds);
				if (_overallPercentage > 100f)
				{
					_overallPercentage = 100f;
				}
				TimeSpan timeSpan2 = DateTime.Now.Subtract(_messageStartTime);
				_messagePercentage = Convert.ToSingle(100.0 / _messageTransitionTime.TotalSeconds * timeSpan2.TotalSeconds);
				if (_messagePercentage > 100f)
				{
					_messagePercentage = 100f;
				}
				_owner.InvalidateFromTimer();
			}

			/// <summary>Makes the next Tick find the box's time over (StopAlert).</summary>
			public void Expire()
			{
				_startTime = new DateTime(2005, 1, 1);
			}

			/// <summary>Ends the box now: not drawn any more, timer stopped. The bitmaps are kept.</summary>
			public void Stop()
			{
				Expire();
				_running = false;
				DisposeTimer();
			}

			/// <summary>Ends the box once its time is over; called by the last (100 %) paint of a transition.</summary>
			public void EndIfComplete()
			{
				if (_overallPercentage >= 100f)
				{
					_running = false;
					DisposeTimer();
				}
			}

			private void DisposeTimer()
			{
				_timer?.Dispose();
				_timer = null;
			}

			/// <summary>
			/// Draws the box as far as it has come, and moves it on: a finished pass starts the gap (and the
			/// flashing), a finished gap starts the next pass. UI thread (paint) only.
			/// </summary>
			public void Paint(PaintEventArgs e)
			{
				if (!_running || _message == null)
				{
					return;
				}
				float scaleX = (float)_owner.ClientRectangle.Width / (float)_owner._newBackgroundPicture.Width;
				float scaleY = (float)_owner.ClientRectangle.Height / (float)_owner._newBackgroundPicture.Height;
				Rectangle backgroundRect = new Rectangle(0, (int)((float)_originY * scaleY), (int)((float)_overallBackGroundRect.Width * scaleX), (int)((float)_overallBackGroundRect.Height * scaleY));
				int messageHeight = (int)((float)_messageBackGroundRect.Height * scaleY);
				// Reference only: on a small screen the box, scaled down, can come out no taller than the text;
				// it is then made 2 px taller than the text. The same box in every frame (pass, gap, flashing):
				// the gap and flash frames used the unadjusted height, so the box jumped between them.
				if (_kind == OverlayKind.Reference && messageHeight >= backgroundRect.Height)
				{
					backgroundRect.Height = messageHeight + 2;
				}
				e.Graphics.ResetTransform();
				if (_messagePercentage < 100f)
				{
					e.Graphics.DrawImage(_background, backgroundRect, _overallBackGroundRect, GraphicsUnit.Pixel);
					if (_scroll)
					{
						DrawMessage(e, _message, (int)((float)_messageBackGroundRect.Width * _messagePercentage / 100f), scaleX, scaleY, messageHeight);
					}
					else
					{
						DrawMessage(e, _message, _messageBackGroundRect.Width, scaleX, scaleY, messageHeight);
					}
				}
				else if (_gapRunning)
				{
					if (_flashCount > 0)
					{
						DrawFlash(e, backgroundRect, scaleX, scaleY, messageHeight);
						return;
					}
					e.Graphics.DrawImage(_background, backgroundRect, _overallBackGroundRect, GraphicsUnit.Pixel);
					DrawMessage(e, _message, _messageBackGroundRect.Width, scaleX, scaleY, messageHeight);
					if (DateTime.Now.Subtract(_gapStartTime).TotalSeconds >= (double)GapDuration)
					{
						if (_occurrences > 1)
						{
							_occurrences--;
							_messageStartTime = DateTime.Now;
							_messagePercentage = 0f;
						}
						_gapRunning = false;
					}
				}
				else
				{
					e.Graphics.DrawImage(_background, backgroundRect, _overallBackGroundRect, GraphicsUnit.Pixel);
					DrawMessage(e, _message, _messageBackGroundRect.Width, scaleX, scaleY, messageHeight);
					if (_flash)
					{
						_flashCount = _flashCountMax;
					}
					_gapStartTime = DateTime.Now;
					_gapRunning = true;
				}
			}

			/// <summary>Draws the first <paramref name="shownWidth"/> pixels of the message, ending at its origin.</summary>
			private void DrawMessage(PaintEventArgs e, Image message, int shownWidth, float scaleX, float scaleY, int messageHeight)
			{
				Rectangle destination = new Rectangle((int)((float)(_messageOriginX - shownWidth) * scaleX), (int)((float)_messageOriginY * scaleY), (int)((float)shownWidth * scaleX), messageHeight);
				e.Graphics.DrawImage(message, destination, new Rectangle(0, 0, shownWidth, _messageBackGroundRect.Height), GraphicsUnit.Pixel);
			}

			private void DrawFlash(PaintEventArgs e, Rectangle backgroundRect, float scaleX, float scaleY, int messageHeight)
			{
				int shownWidth = (int)((float)_messageBackGroundRect.Width * _messagePercentage / 100f);
				if (((_flashCount > 8) & (_flashCount < 12)) || ((_flashCount > 0) & (_flashCount < 5)))
				{
					e.Graphics.DrawImage(_backgroundInverse, backgroundRect, _overallBackGroundRect, GraphicsUnit.Pixel);
					DrawMessage(e, _messageInverse, shownWidth, scaleX, scaleY, messageHeight);
				}
				else
				{
					e.Graphics.DrawImage(_background, backgroundRect, _overallBackGroundRect, GraphicsUnit.Pixel);
					DrawMessage(e, _message, shownWidth, scaleX, scaleY, messageHeight);
				}
				_flashCount--;
			}

			private void DisposeGraphics()
			{
				_backgroundGraphics?.Dispose();
				_backgroundGraphics = null;
				_backgroundInverseGraphics?.Dispose();
				_backgroundInverseGraphics = null;
				_messageGraphics?.Dispose();
				_messageGraphics = null;
				_messageInverseGraphics?.Dispose();
				_messageInverseGraphics = null;
			}

			/// <summary>
			/// Disposes the bitmaps (and the setup Graphics). UI thread only, the thread that paints, so paint
			/// never sees a disposed bitmap. _message is the paint guard, so it is cleared first.
			/// </summary>
			public void ReleaseImages()
			{
				DisposeGraphics();
				_message?.Dispose();
				_message = null;
				_messageInverse?.Dispose();
				_messageInverse = null;
				_background?.Dispose();
				_background = null;
				_backgroundInverse?.Dispose();
				_backgroundInverse = null;
			}

			public void Dispose()
			{
				DisposeTimer();
				ReleaseImages();
			}
		}
	}
}
