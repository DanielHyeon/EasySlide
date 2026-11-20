using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Threading;
using System.Windows.Forms;
using Axicultus.Module;
using Axicultus.Util;

namespace Axicultus;

internal partial class ImageTransitionControl : Control
	{
		private enum PanelType
		{
			NoAction,
			Current,
			CurrentWithTrans,
			New,
			NewWithTrans
		}

		public enum TransitionTypes
		{
			None,
			Ascend,
			Away,
			Blinds_H,
			Blinds_V,
			BowTie,
			Checkerboard,
			Circle,
			CircularWipe,
			Cross,
			Descend,
			Diamond,
			Dissolve,
			DoorsOpen,
			DoorsClose,
			Fade,
			FanUp,
			Flip_H,
			Flip_V,
			Gentle_Zoom,
			Heart,
			Mesh,
			InTop,
			InLeft,
			InRight,
			InBottom,
			InTopLeft,
			InTopRight,
			InBottomLeft,
			InBottomRight,
			Mosaic,
			OutTop,
			OutLeft,
			OutRight,
			OutBottom,
			OutTopLeft,
			OutTopRight,
			OutBottomLeft,
			OutBottomRight,
			Oval,
			RandomBars,
			Rectangle,
			RectangleIn,
			RevealTopDown,
			RevealLeftRight,
			RevealRightLeft,
			RevealDownUp,
			Scroll,
			Spin,
			Spiral,
			Star,
			Stretch_H,
			Stretch_V,
			Wedge,
			WindMill,
			Zoom_Away,
			Zoom_In,
			Zoom_Out
		}

		public enum TransitionAction
		{
			None,
			AsStored,
			AsStoredItem,
			AsStoredSlide,
			AsFade
		}

		public enum BackPicturesTransition
		{
			None,
			CurrentOnly,
			NewOnly,
			BothBackgrounds
		}

		public const int MaxTransitions = 58;

		private System.Threading.Timer t;

		private Graphics wsg;

		private string[] _transitionItem = new string[58];

		private bool ItemFirstShowing = false;

		// Cells (RandomBars, Dissolve, Mosaic) shown so far. UI thread only: Go (ResetBitsArray) and paint.
		private HashSet<int> BitsArraySet = [];

		private int TotalBitsArraySet = 0;

		private int DissolveSize = 0;

		private int DissolveCountX = 0;

		private int DissolveCountY = 0;

		private int DissolveTotal = 0;

		// Published frames may both be visible during a transition. Compose into a third
		// slot only while both are in use. Ordinary instant updates alternate two slots.
		private readonly Bitmap[] _compositeBuffers = new Bitmap[3];

		internal readonly record struct BackgroundFileKey(string Path, DateTime ModifiedUtc,
			long Length, ImageMode Mode, Size Buffer, Size Viewport);
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		internal BackgroundFileKey? RenderedBackground { get; set; }

		// What callers set (TransitionType); the control never changes it.
		private TransitionTypes _transitionType = TransitionTypes.None;

		// The type of the transition Go started last: TransitionType then, or Fade for AsFade. Only Tick,
		// paint and ResetBitsArray read it. Written on the UI thread by Go, under _transitionLock (Tick reads it).
		private TransitionTypes _activeTransitionType = TransitionTypes.None;

		private int _nHDivs = 10;

		private int _nVDivs = 8;

		private string _imageFileName = "";

		private int _picMode = 0;

		private Image _currentCombinedImage;

		private Image _newCombinedImage;

		private Image _newBackgroundPicture;

		private Image _currentBackgroundPicture;

		private Image _newTextImage;

		private Image _currentTextImage;

		private Image _newPanelImage;

		private Image _currentPanelImage;

		// The work space of the transitions that compose a frame first (Doors, Flip, Gentle Zoom, Zoom In,
		// Stretch, Slide In/Out/Reveal): this screen's own, kept from frame to frame during a transition and
		// made again only when its size changes (it was a new picture-size bitmap every frame); released when
		// the transition reaches 100 % and in Dispose. Never shown or handed out, not in _ownImages. See
		// CreateFrameWorkSpace.
		private Bitmap _workSpace;

		private BackPicturesTransition _transitBackPictureAction = BackPicturesTransition.BothBackgrounds;

		private string _backgroundID = "";

		private TimeSpan _transitionTime = new(0, 0, 0, 1, 0);

		// Progress of the running transition, written by Go, Tick (thread-pool thread) and
		// EndFailedTransition under _transitionLock.
		private float _currentPercentage = 0f;

		// _currentPercentage as one frame sees it: taken once at the start of the frame (PaintFrame), so a
		// Tick writing in the middle of a frame cannot change it halfway. UI thread (paint) only.
		private float _framePercentage = 0f;

		// When the running transition started (Timestamp).
		private long _startTimestamp;

		// The alert and the reference text drawn over the picture (see TextOverlay); made in the constructor.
		private readonly TextOverlay _alert;

		private readonly TextOverlay _reference;

		public string RefDisplayString = "";

		// Cached objects for performance optimization
		private ImageAttributes _cachedImageAttributes;
		private ColorMatrix _cachedColorMatrix;
		private static readonly Random _random = new();

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public TransitionTypes TransitionType
		{
			get
			{
				return _transitionType;
			}
			set
			{
				_transitionType = value;
			}
		}

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int HorizontalDivisions
		{
			get
			{
				return _nHDivs;
			}
			set
			{
				// The Checkerboard and Mesh steps divide by it; 0 or less would give a step of 0 (endless loop).
				if (value <= 0)
				{
					value = 10;
				}
				_nHDivs = value;
			}
		}

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int VerticalDivisions
		{
			get
			{
				return _nVDivs;
			}
			set
			{
				if (value <= 0)
				{
					value = 8;
				}
				_nVDivs = value;
			}
		}

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string ImageFileName
		{
			get
			{
				return _imageFileName;
			}
			set
			{
				_imageFileName = value;
			}
		}

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int PicMode
		{
			get
			{
				return _picMode;
			}
			set
			{
				_picMode = value;
			}
		}

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Image CurrentCombinedImage
		{
			get
			{
				return _currentCombinedImage;
			}
			set
			{
				Image replaced = _currentCombinedImage;
				_currentCombinedImage = value;
				ReleaseOwnedImage(replaced);
			}
		}

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Image NewCombinedImage
		{
			get
			{
				return _newCombinedImage;
			}
			set
			{
				Image replaced = _newCombinedImage;
				_newCombinedImage = value;
				ReleaseOwnedImage(replaced);
			}
		}

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Image NewBackgroundPicture
		{
			get
			{
				return _newBackgroundPicture;
			}
			set
			{
				Image replaced = _newBackgroundPicture;
				if (!ReferenceEquals(replaced, value))
					RenderedBackground = null;
				_newBackgroundPicture = value;
				ReleaseOwnedImage(replaced);
			}
		}

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Image CurrentBackgroundPicture
		{
			get
			{
				return _currentBackgroundPicture;
			}
			set
			{
				Image replaced = _currentBackgroundPicture;
				_currentBackgroundPicture = value;
				ReleaseOwnedImage(replaced);
			}
		}

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Image NewTextImage
		{
			get
			{
				return _newTextImage;
			}
			set
			{
				Image replaced = _newTextImage;
				_newTextImage = value;
				ReleaseOwnedImage(replaced);
			}
		}

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Image CurrentTextImage
		{
			get
			{
				return _currentTextImage;
			}
			set
			{
				Image replaced = _currentTextImage;
				_currentTextImage = value;
				ReleaseOwnedImage(replaced);
			}
		}

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Image NewPanelImage
		{
			get
			{
				return _newPanelImage;
			}
			set
			{
				Image replaced = _newPanelImage;
				_newPanelImage = value;
				ReleaseOwnedImage(replaced);
			}
		}

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Image CurrentPanelImage
		{
			get
			{
				return _currentPanelImage;
			}
			set
			{
				Image replaced = _currentPanelImage;
				_currentPanelImage = value;
				ReleaseOwnedImage(replaced);
			}
		}

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public BackPicturesTransition TransitBackPictureAction
		{
			get
			{
				return _transitBackPictureAction;
			}
			set
			{
				_transitBackPictureAction = value;
			}
		}

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string BackgroundID
		{
			get
			{
				return _backgroundID;
			}
			set
			{
				_backgroundID = value;
			}
		}

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
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

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public float AlertTransitionTime
		{
			get
			{
				return _alert.TransitionTime;
			}
			set
			{
				_alert.TransitionTime = value;
			}
		}

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public float AlertMessageTransitionTime
		{
			get
			{
				return _alert.MessageTransitionTime;
			}
			set
			{
				_alert.MessageTransitionTime = value;
			}
		}

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public float RefTransitionTime
		{
			get
			{
				return _reference.TransitionTime;
			}
			set
			{
				_reference.TransitionTime = value;
			}
		}

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public float RefMessageTransitionTime
		{
			get
			{
				return _reference.MessageTransitionTime;
			}
			set
			{
				_reference.MessageTransitionTime = value;
			}
		}

		public void BuildTransitionArray()
		{
			_transitionItem[0] = "None";
			_transitionItem[1] = "Ascend";
			_transitionItem[2] = "Away";
			_transitionItem[3] = "Blinds Horizontal";
			_transitionItem[4] = "Blinds Vertical";
			_transitionItem[5] = "Bow Tie";
			_transitionItem[6] = "Checkerboard";
			_transitionItem[7] = "Circle";
			_transitionItem[8] = "Circular Wipe";
			_transitionItem[9] = "Cross";
			_transitionItem[10] = "Descend";
			_transitionItem[11] = "Diamond";
			_transitionItem[12] = "Dissolve";
			_transitionItem[13] = "Doors Open";
			_transitionItem[14] = "Doors Close";
			_transitionItem[15] = "Fade";
			_transitionItem[16] = "Fan Up";
			_transitionItem[17] = "Flip Horizontal";
			_transitionItem[18] = "Flip Vertical";
			_transitionItem[19] = "Gentle Zoom";
			_transitionItem[20] = "Heart";
			_transitionItem[21] = "Mesh";
			_transitionItem[22] = "In Top";
			_transitionItem[23] = "In Left";
			_transitionItem[24] = "In Right";
			_transitionItem[25] = "In Bottom";
			_transitionItem[26] = "In Top Left";
			_transitionItem[27] = "In Top Right";
			_transitionItem[28] = "In Bottom Left";
			_transitionItem[29] = "In Bottom Right";
			_transitionItem[30] = "Mosaic";
			_transitionItem[31] = "Out Top";
			_transitionItem[32] = "Out Left";
			_transitionItem[33] = "Out Right";
			_transitionItem[34] = "Out Bottom";
			_transitionItem[35] = "Out Top Left";
			_transitionItem[36] = "Out Top Right";
			_transitionItem[37] = "Out Bottom Left";
			_transitionItem[38] = "Out Bottom Right";
			_transitionItem[39] = "Oval";
			_transitionItem[40] = "Random Bars";
			_transitionItem[41] = "Rectangle";
			_transitionItem[42] = "Rectangle In";
			_transitionItem[43] = "Reveal Top Down";
			_transitionItem[44] = "Reveal Left Right";
			_transitionItem[45] = "Reveal Right Left";
			_transitionItem[46] = "Reveal Down Up";
			_transitionItem[47] = "Scroll";
			_transitionItem[48] = "Spin";
			_transitionItem[49] = "Spiral";
			_transitionItem[50] = "Star";
			_transitionItem[51] = "Stretch Horizontal";
			_transitionItem[52] = "Stretch Vertical";
			_transitionItem[53] = "Wedge";
			_transitionItem[54] = "WindMill";
			_transitionItem[55] = "Zoom Away";
			_transitionItem[56] = "Zoom In";
			_transitionItem[57] = "Zoom Out";
		}

		public void ResetBitsArray()
		{
			BitsArraySet.Clear();
			TotalBitsArraySet = 0;
			int sizeDivisor = (_activeTransitionType == TransitionTypes.Dissolve) ? 90000 : 20000;
			DissolveSize = base.Height * base.Width / sizeDivisor + 1;
			DissolveCountX = base.Width / DissolveSize + 1;
			DissolveCountY = base.Height / DissolveSize + 1;
			DissolveTotal = DissolveCountX * DissolveCountY;
		}

		public void BuildTransitionsList(ref ToolStripComboBox InCombo)
		{
			BuildTransitionsList(ref InCombo, TransitionTypes.None);
		}

		public void BuildTransitionsList(ref ToolStripComboBox InCombo, TransitionTypes SelectedTransition)
		{
			InCombo.Items.Clear();
			for (int i = 0; i < 58; i++)
			{
				InCombo.Items.Add(_transitionItem[i]);
			}
			InCombo.SelectedIndex = (int)SelectedTransition;
		}

		public int GetTransitionType(string SelectedTransitionText)
		{
			SelectedTransitionText = DataUtil.Trim(SelectedTransitionText);
			if (SelectedTransitionText == "")
			{
				return 0;
			}
			for (int i = 0; i < 58; i++)
			{
				if (_transitionItem[i] == SelectedTransitionText)
				{
					return i;
				}
			}
			return 0;
		}

		public string GetTransitionText(int InTransitionType)
		{
			try
			{
				return _transitionItem[InTransitionType];
			}
			catch
			{
			}
			return _transitionItem[0];
		}

		public ImageTransitionControl()
		{
			_alert = new TextOverlay(this, OverlayKind.Alert);
			_reference = new TextOverlay(this, OverlayKind.Reference);
			SetStyle(ControlStyles.UserPaint | ControlStyles.Opaque | ControlStyles.ResizeRedraw | ControlStyles.SupportsTransparentBackColor | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, value: true);
			BuildTransitionArray();
			
			// Initialize cached objects for performance
			_cachedImageAttributes = new ImageAttributes();
			_cachedColorMatrix = new ColorMatrix();
			
			BackColor = Gf.TransparentColour;
		}

		/// <summary>
		/// Puts the default background picture in. Gf.SetDefaultBackScreen makes a new one for this screen
		/// alone at every call (show start, default background changed), so it is this screen's own: the one
		/// it replaces is disposed once no field of this screen refers to it any more (it may still be a
		/// background picture of the item shown now).
		/// </summary>
		public void SetDefaultBackgroundPicture(Image InBitmap)
		{
			Image replaced = BackgroundImage;
			BackgroundImage = InBitmap;
			AdoptImage(InBitmap);
			ReleaseOwnedImage(replaced);
		}

		// Guards _transitionGeneration and the progress written by Tick (thread-pool thread).
		private readonly Lock _transitionLock = new();
		// Bumped whenever a transition starts or is ended early; a Tick carries the value it was started with.
		private int _transitionGeneration;
		// Adopted backgrounds, copies and overflow composites are released after the last
		// picture field drops them. Reusable buffers have their own lifetime on this control.
		private readonly HashSet<Image> _ownImages = [];

		/// <summary>
		/// Makes <paramref name="image"/>, a picture just made for this screen alone, this screen's own
		/// (see _ownImages) and returns it. UI thread only.
		/// </summary>
		internal Image AdoptImage(Image image)
		{
			if (image != null)
			{
				_ownImages.Add(image);
			}
			return image;
		}

		// Draw buffers of this screen for Gf.DrawText (text layer and panel layer), two per layer and
		// never more. Gf.DrawText takes the layer shown before the draw as the transition's starting
		// layer (CurrentTextImage / CurrentPanelImage, no copy), draws into the other buffer, and makes
		// that NewTextImage / NewPanelImage only when the whole draw has succeeded. A draw that throws
		// halfway therefore leaves the shown layer unchanged. Every screen has its own buffers; a
		// buffer is reused while its size stays the same.
		private readonly Bitmap[] _textDrawBuffers = new Bitmap[2];
		private readonly Graphics[] _textDrawGraphics = new Graphics[2];
		private readonly Bitmap[] _panelDrawBuffers = new Bitmap[2];
		private readonly Graphics[] _panelDrawGraphics = new Graphics[2];

		/// <summary>Cleared text draw buffer of this screen that is not shown now (UI thread only).</summary>
		internal Graphics GetTextDrawGraphics(int width, int height, out Image image)
		{
			return GetHiddenDrawBuffer(_textDrawBuffers, _textDrawGraphics, _newTextImage, width, height, out image);
		}

		/// <summary>Cleared panel draw buffer of this screen that is not shown now (UI thread only).</summary>
		internal Graphics GetPanelDrawGraphics(int width, int height, out Image image)
		{
			return GetHiddenDrawBuffer(_panelDrawBuffers, _panelDrawGraphics, _newPanelImage, width, height, out image);
		}

		private static Graphics GetHiddenDrawBuffer(Bitmap[] buffers, Graphics[] graphics, Image shownImage, int width, int height, out Image image)
		{
			int index = ReferenceEquals(shownImage, buffers[0]) ? 1 : 0;
			if (buffers[index] == null || buffers[index].Width != width || buffers[index].Height != height)
			{
				// Not shown, so no paint can reach it: safe to replace.
				graphics[index]?.Dispose();
				buffers[index]?.Dispose();
				graphics[index] = null;
				buffers[index] = null;
				buffers[index] = new Bitmap(width, height, PixelFormat.Format32bppPArgb);
				graphics[index] = Graphics.FromImage(buffers[index]);
			}
			Graphics g = graphics[index];
			// The Graphics is reused: a draw that threw halfway may have left its state changed.
			// Start every draw from the state a new Graphics had before the buffers were reused.
			g.ResetTransform();
			g.ResetClip();
			g.SmoothingMode = SmoothingMode.Default;
			g.InterpolationMode = InterpolationMode.Default;
			g.PixelOffsetMode = PixelOffsetMode.Default;
			g.CompositingMode = CompositingMode.SourceOver;
			g.CompositingQuality = CompositingQuality.Default;
			g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
			g.Clear(Color.Transparent);
			image = buffers[index];
			return g;
		}

		/// <summary>
		/// True while a transition started by Go has not painted its last (100 %) frame yet. A screen shown
		/// without a transition is not counted (its picture is painted at the next paint; see Control.Update).
		/// Read on the UI thread.
		/// </summary>
		internal bool TransitionPending => t != null;

		private Bitmap GetCompositeBuffer(Size size)
		{
			int available = -1;
			Bitmap reusable = null;
			for (int i = 0; i < _compositeBuffers.Length; i++)
			{
				Bitmap buffer = _compositeBuffers[i];
				if (buffer != null && IsReferenced(buffer))
					continue;
				if (buffer?.Size == size)
					reusable ??= buffer;
				else
				{
					buffer?.Dispose();
					_compositeBuffers[i] = null;
					available = i;
				}
			}
			if (reusable != null)
				return reusable;
			if (available >= 0)
				return _compositeBuffers[available] = new Bitmap(size.Width, size.Height, PixelFormat.Format32bppPArgb);

			// A caller may have assigned past composites to additional picture fields.
			return (Bitmap)AdoptImage(new Bitmap(size.Width, size.Height, PixelFormat.Format32bppPArgb));
		}

		public void Go(TransitionAction TransitionAction, bool InFirstShowing, bool ClearAll, bool DoActiveIndicator, bool LiveCamOnShow)
		{
			ItemFirstShowing = InFirstShowing || DoActiveIndicator;
			Bitmap composed = GetCompositeBuffer(NewBackgroundPicture.Size);
			try
			{
				using Graphics graphics = Graphics.FromImage(composed);
				if (LiveCamOnShow)
					graphics.Clear(Gf.TransparentColour);
				else
				{
					graphics.CompositingMode = CompositingMode.SourceCopy;
					graphics.DrawImageUnscaled(NewBackgroundPicture, 0, 0);
					graphics.CompositingMode = CompositingMode.SourceOver;
					graphics.DrawImageUnscaled(NewTextImage, 0, 0);
					graphics.DrawImageUnscaled(NewPanelImage, 0, 0);
				}
			}
			catch
			{
				ReleaseOwnedImage(composed);
				throw;
			}

			if (TransitionAction == TransitionAction.None)
			{
				lock (_transitionLock)
				{
					// Ends a transition that may still be running: its Ticks no longer write progress.
					_transitionGeneration++;
					_currentPercentage = 100f;
				}
				// Its timer is disposed here too, as the 100 % paint would (a running timer stops ticking by
				// itself at 100 %, but is disposed only by the 100 % paint, EndFailedTransition or Go).
				t?.Dispose();
				t = null;
				if (ClearAll)
				{
					StopRef();
				}
				NewCombinedImage = composed;
				Invalidate();
				return;
			}
			if (NewCombinedImage != null)
			{
				CurrentCombinedImage = NewCombinedImage;
			}
			NewCombinedImage = composed;
			// Dispose previous timer to prevent multiple timers running
			t?.Dispose();
			var tickState = new TransitionTickState();
			lock (_transitionLock)
			{
				tickState.Generation = ++_transitionGeneration;
				// AsFade fades this one transition; TransitionType itself (what callers set) is left as it is.
				_activeTransitionType = (TransitionAction == TransitionAction.AsFade) ? TransitionTypes.Fade : _transitionType;
				_currentPercentage = 0f;
				_startTimestamp = Timestamp();
			}
			// Created stopped and started only once its callback can see it (tickState.Timer), so even a first
			// Tick that comes at once can stop it at 100 %.
			System.Threading.Timer timer = new(Tick, tickState, System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
			tickState.Timer = timer;
			t = timer;
			timer.Change(40, 40);
			ResetBitsArray();
			if (ClearAll)
			{
				StopRef();
			}
			Invalidate();
		}

		private void Tick(object state)
		{
			// Runs on a thread-pool thread. A Tick of a transition that has ended (replaced by Go,
			// ended after a failed paint) must not write progress: it could put a finished screen back
			// to a half blend with no timer left to finish it. The check and the write are one step
			// under the lock that also ends transitions.
			if (state is not TransitionTickState tickState)
			{
				return;
			}
			bool complete;
			lock (_transitionLock)
			{
				if (tickState.Generation != _transitionGeneration)
				{
					return;
				}
				_currentPercentage = ((_activeTransitionType == TransitionTypes.None) ? 100f : Convert.ToSingle(100.0 / _transitionTime.TotalSeconds * SecondsSince(_startTimestamp)));
				if (_currentPercentage > 100f)
				{
					_currentPercentage = 100f;
				}
				complete = _currentPercentage >= 100f;
			}
			if (complete)
			{
				// The last frame is due: the 100 % paint disposes the timer. Until then (a hidden screen may
				// not paint for a long time) it no longer ticks.
				StopTicking(tickState);
			}
			InvalidateFromTimer();
		}

		// What a transition's Tick is started with: its generation (see Tick) and its own timer, so a Tick
		// only ever stops the timer it runs on.
		private sealed class TransitionTickState : TimerHandle
		{
			public int Generation;
		}

		// A timer as its own callback sees it (set right after the timer is made).
		private class TimerHandle
		{
			public volatile System.Threading.Timer Timer;
		}

		/// <summary>
		/// Stops the timer of <paramref name="handle"/> firing (it stays to be disposed by its owner). Called
		/// from that timer's own callback; the owner may have disposed it meanwhile. Never throws.
		/// </summary>
		private static void StopTicking(TimerHandle handle)
		{
			try
			{
				handle?.Timer?.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
			}
			catch (ObjectDisposedException)
			{
			}
		}

		// A start timestamp so far back that any progress measured from it is complete: StopAlert / StopRef
		// (it was the date 2005-01-01 with the wall clock).
		private const long ExpiredTimestamp = long.MinValue / 4;

		/// <summary>
		/// Now, for transition and overlay progress: Stopwatch ticks, which only move forward (the wall clock,
		/// DateTime.Now, can step back with a time sync or a manual change, which made the progress negative).
		/// </summary>
		private static long Timestamp()
		{
			return System.Diagnostics.Stopwatch.GetTimestamp();
		}

		/// <summary>Seconds since <paramref name="timestamp"/> (Timestamp), never negative.</summary>
		private static double SecondsSince(long timestamp)
		{
			double seconds = (double)(System.Diagnostics.Stopwatch.GetTimestamp() - timestamp) / System.Diagnostics.Stopwatch.Frequency;
			return (seconds > 0.0) ? seconds : 0.0;
		}

		/// <summary>
		/// Invalidate for the timer callbacks (Tick, TextOverlay.Tick), which run on thread-pool threads.
		/// Timer.Dispose does not wait for a callback that is already running, so one can reach this while the
		/// control is being disposed (the show window closing, the app exiting). Invalidate there could try to
		/// create the handle on this thread and throw; an exception in a timer callback ends the process.
		/// </summary>
		private void InvalidateFromTimer()
		{
			if (IsDisposed || Disposing || !IsHandleCreated)
			{
				return;
			}
			try
			{
				Invalidate();
			}
			catch (Exception ex)
			{
				// The handle can still be destroyed between the check and Invalidate (ObjectDisposedException,
				// InvalidOperationException, Win32Exception from a handle re-created on this thread). Any
				// exception here would end the process; the next paint, if the screen still exists, catches up.
				System.Diagnostics.Trace.WriteLine("ImageTransitionControl timer repaint skipped: " + ex.Message);
			}
		}

		/// <summary>
		/// Disposes a picture this screen owns (a combined image Go made, a background picture or copy
		/// adopted with AdoptImage) once no field of this screen refers to it any more: paint and Go read
		/// pictures only through these fields, on this (UI) thread. Pictures of another owner are not this
		/// screen's and are left alone.
		/// </summary>
		private void ReleaseOwnedImage(Image image)
		{
			if (image == null || IsReferenced(image))
			{
				return;
			}
			if (_ownImages.Remove(image))
			{
				image.Dispose();
			}
		}

		private bool IsReferenced(Image image)
		{
			return ReferenceEquals(image, _currentCombinedImage) || ReferenceEquals(image, _newCombinedImage)
				|| ReferenceEquals(image, _newBackgroundPicture) || ReferenceEquals(image, _currentBackgroundPicture)
				|| ReferenceEquals(image, _newTextImage) || ReferenceEquals(image, _currentTextImage)
				|| ReferenceEquals(image, _newPanelImage) || ReferenceEquals(image, _currentPanelImage)
				|| ReferenceEquals(image, BackgroundImage);
		}

		private void DrawDefaultBackPattern(PaintEventArgs e)
		{
			Image background = BackgroundImage;
			if (background == null)
			{
				// No default background yet (a paint before SetDefaultBackgroundPicture): black, as the
				// screen shows before a show starts. It threw at every paint.
				e.Graphics.FillRectangle(Brushes.Black, base.ClientRectangle);
				return;
			}
			DrawImageToOutput(e, background, base.ClientRectangle, new Rectangle(0, 0, background.Width, background.Height), null);
		}

		private void DrawBackPatternOnWorkSpace(ref Graphics g, Image InBackgroundImage)
		{
			if (InBackgroundImage == null)
			{
				// No default background yet: black, as DrawDefaultBackPattern draws then (it used to throw).
				g.FillRectangle(Brushes.Black, 0, 0, _newBackgroundPicture.Width, _newBackgroundPicture.Height);
				return;
			}
			g.DrawImage(InBackgroundImage, new Rectangle(0, 0, _newBackgroundPicture.Width, _newBackgroundPicture.Height), 0, 0, _newBackgroundPicture.Width, _newBackgroundPicture.Height, GraphicsUnit.Pixel);
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			// An exception leaving OnPaint makes WinForms stop painting this control for good (red
			// cross); on the show window that would be the projector until restart. A failed frame is
			// therefore caught here: logged (repeats only counted), shown in the main form's error
			// notice, and skipped. A failed transition frame also ends the transition (see
			// EndFailedTransition). The next paint is attempted normally.
			try
			{
				PaintScreen(e);
				_failedPaintRetries = 0;
			}
			catch (Exception ex)
			{
				FrmMain.WriteDebugLogThrottled("ImageTransitionControl.OnPaint (" + Name + ")", ex);
				FrmMain.ReportErrorNotice("Screen " + Name + " could not be drawn: " + ex.Message);
				EndFailedTransition();
				// The control paints double-buffered: WinForms puts the back buffer on the screen even
				// when this paint threw, so an empty or half-drawn frame would show black where nothing
				// was drawn, and stay until the next slide. Cover it with the last complete picture.
				PaintLastCompletePicture(e);
				RequestRetryPaint();
			}
		}

		// Retry paints requested in a row after failed paints (RequestRetryPaint); a paint that succeeds resets it.
		private int _failedPaintRetries;
		private const int MaxFailedPaintRetries = 3;

		/// <summary>
		/// Draws the last complete picture of this screen over a frame whose paint failed: the combined
		/// image Go composed last (background, text and panel of the item shown now), which is also
		/// what the screen shows at the end of a transition. Alert and reference text, drawn on top in
		/// each paint, are missing from this frame. Draws nothing when there is no such picture yet.
		/// Allocates no bitmap and never throws.
		/// </summary>
		private void PaintLastCompletePicture(PaintEventArgs e)
		{
			Image picture = _newCombinedImage;
			if (picture == null)
			{
				return;
			}
			try
			{
				Graphics g = e.Graphics;
				// The failed paint may have left a transform (zoom transitions), a clip (shape
				// transitions) or another mode set.
				g.ResetTransform();
				g.ResetClip();
				g.CompositingMode = CompositingMode.SourceCopy;
				g.DrawImage(picture, base.ClientRectangle, 0, 0, picture.Width, picture.Height, GraphicsUnit.Pixel);
			}
			catch
			{
			}
		}

		/// <summary>
		/// Asks for one more paint after a failed one, posted (not from inside this paint), at most
		/// MaxFailedPaintRetries times in a row: a paint that keeps failing cannot repaint itself in a
		/// loop. Never throws.
		/// </summary>
		private void RequestRetryPaint()
		{
			if (_failedPaintRetries >= MaxFailedPaintRetries)
			{
				return;
			}
			_failedPaintRetries++;
			try
			{
				if (IsHandleCreated && !IsDisposed)
				{
					BeginInvoke((MethodInvoker)delegate
					{
						if (!IsDisposed)
						{
							Invalidate();
						}
					});
				}
			}
			catch
			{
			}
		}

		/// <summary>
		/// Treats the running transition as finished after a paint failed. The transition timer stops
		/// ticking by itself only at 100 % and is disposed by the 100 % paint, so a frame failing before
		/// that would keep it invalidating (about 25 times a second, every frame failing again). The next
		/// paint then draws the new image as the end of a transition does.
		/// </summary>
		private void EndFailedTransition()
		{
			System.Threading.Timer timer = t;
			t = null;
			timer?.Dispose();
			lock (_transitionLock)
			{
				// Ticks of this transition still queued do not write progress any more (see Tick).
				_transitionGeneration++;
				_currentPercentage = 100f;
			}
		}

		// GDI+ objects made for one transition frame (the work space's Graphics, paths, regions, matrices).
		// PaintScreen disposes them when the frame ends, also when it throws. Most branches dispose them
		// themselves as well; disposing twice is harmless. UI thread only (paint).
		private readonly List<IDisposable> _frameObjects = [];

		/// <summary>Registers an object made for the frame being painted (see _frameObjects) and returns it.</summary>
		private T FrameObject<T>(T disposable) where T : IDisposable
		{
			_frameObjects.Add(disposable);
			return disposable;
		}

		/// <summary>
		/// Gets the work space (_workSpace) ready for this frame, cleared to transparent as a new bitmap is,
		/// and makes its Graphics (wsg, released when the frame ends, also when it throws; a new one each
		/// frame, so no transform or clip of the last frame is left on it). The bitmap is kept for the next
		/// frame and made again only when the size changes. UI thread only (paint).
		/// </summary>
		private void CreateFrameWorkSpace(int width, int height)
		{
			if (_workSpace == null || _workSpace.Width != width || _workSpace.Height != height)
			{
				Bitmap replaced = _workSpace;
				_workSpace = null;
				replaced?.Dispose();
				_workSpace = new Bitmap(width, height, WorkSpaceFormat);
			}
			wsg = FrameObject(Graphics.FromImage(_workSpace));
			wsg.Clear(Color.Transparent);
		}

		private const PixelFormat WorkSpaceFormat = PixelFormat.Format32bppPArgb;

		/// <summary>Disposes the objects of the frame just painted, newest first (a Graphics before its bitmap). Never throws.</summary>
		private void ReleaseFrameObjects()
		{
			for (int i = _frameObjects.Count - 1; i >= 0; i--)
			{
				try
				{
					_frameObjects[i].Dispose();
				}
				catch
				{
				}
			}
			_frameObjects.Clear();
		}

		private void PaintScreen(PaintEventArgs e)
		{
			if (_currentCombinedImage == null || _newCombinedImage == null)
			{
				DrawDefaultBackPattern(e);
				return;
			}
			try
			{
				PaintFrame(e);
			}
			finally
			{
				ReleaseFrameObjects();
				// A shape transition's clip and a zoom transition's transform end with the frame.
				try
				{
					e.Graphics.ResetClip();
					e.Graphics.ResetTransform();
				}
				catch
				{
				}
			}
			base.OnPaint(e);
		}

		private void PaintFrame(PaintEventArgs e)
		{
			// One progress value for the whole frame (see _framePercentage).
			lock (_transitionLock)
			{
				_framePercentage = _currentPercentage;
			}
			GraphicsPath graphicsPath = null;
			GraphicsPath graphicsPath2 = null;
			if (_framePercentage < 100f)
			{
				if (_activeTransitionType == TransitionTypes.None)
				{
					DrawNewImageOpaque(e);
				}
				else if (_activeTransitionType == TransitionTypes.Ascend)
				{
					// Reusing cached ImageAttributes
					// Reusing cached ColorMatrix
					float slideDistance = 0f;
					int offsetY = 0;
					float alpha = 0f;
					if (TransitBackPictureAction == BackPicturesTransition.None)
					{
						DrawImageToOutput(e, _currentBackgroundPicture, base.ClientRectangle, new Rectangle(0, 0, _currentBackgroundPicture.Width, _currentBackgroundPicture.Height), null);
						alpha = 0.003921569f * (255f * (1f - _framePercentage / 60f));
						alpha = (_cachedColorMatrix.Matrix33 = ((alpha > 1f) ? 1f : ((alpha < 0f) ? 0f : alpha)));
						_cachedImageAttributes.SetColorMatrix(_cachedColorMatrix);
						DrawImageToOutput(e, _currentTextImage, base.ClientRectangle, new Rectangle(0, 0, _newCombinedImage.Width, _newCombinedImage.Height), _cachedImageAttributes, PanelType.Current);
						alpha = 0.003921569f * (510f * _framePercentage / 100f);
						alpha = (_cachedColorMatrix.Matrix33 = ((alpha > 1f) ? 1f : ((alpha < 0f) ? 0f : alpha)));
						_cachedImageAttributes.SetColorMatrix(_cachedColorMatrix);
						slideDistance = 0.15f * (float)_newCombinedImage.Height;
						offsetY = (int)(slideDistance * _framePercentage / 100f - slideDistance);
						DrawImageToOutput(e, _newTextImage, base.ClientRectangle, new Rectangle(0, offsetY, _newCombinedImage.Width, _newCombinedImage.Height), _cachedImageAttributes, PanelType.New);
					}
					else
					{
						Draw_CurrentImage(e);
						_cachedColorMatrix.Matrix33 = 0.003921569f * (255f * _framePercentage / 100f);
						_cachedImageAttributes.SetColorMatrix(_cachedColorMatrix);
						slideDistance = 0.15f * (float)_newCombinedImage.Height;
						offsetY = (int)(slideDistance * _framePercentage / 100f - slideDistance);
						DrawImageToOutput(e, _newCombinedImage, base.ClientRectangle, new Rectangle(0, offsetY, _newCombinedImage.Width, _newCombinedImage.Height), _cachedImageAttributes);
					}
				}
				else if (_activeTransitionType == TransitionTypes.Descend)
				{
					// Reusing cached ImageAttributes
					// Reusing cached ColorMatrix
					float slideDistance = 0f;
					int offsetY = 0;
					float alpha = 0f;
					if (TransitBackPictureAction == BackPicturesTransition.None)
					{
						DrawImageToOutput(e, _currentBackgroundPicture, base.ClientRectangle, new Rectangle(0, 0, _currentBackgroundPicture.Width, _currentBackgroundPicture.Height), null);
						alpha = 0.003921569f * (255f * (1f - _framePercentage / 60f));
						alpha = (_cachedColorMatrix.Matrix33 = ((alpha > 1f) ? 1f : ((alpha < 0f) ? 0f : alpha)));
						_cachedImageAttributes.SetColorMatrix(_cachedColorMatrix);
						DrawImageToOutput(e, _currentTextImage, base.ClientRectangle, new Rectangle(0, 0, _newCombinedImage.Width, _newCombinedImage.Height), _cachedImageAttributes, PanelType.Current);
						alpha = 0.003921569f * (510f * _framePercentage / 100f);
						alpha = (_cachedColorMatrix.Matrix33 = ((alpha > 1f) ? 1f : ((alpha < 0f) ? 0f : alpha)));
						_cachedImageAttributes.SetColorMatrix(_cachedColorMatrix);
						slideDistance = 0.15f * (float)_newCombinedImage.Height;
						offsetY = (int)(slideDistance - slideDistance * _framePercentage / 100f);
						DrawImageToOutput(e, _newTextImage, base.ClientRectangle, new Rectangle(0, offsetY, _newCombinedImage.Width, _newCombinedImage.Height), _cachedImageAttributes, PanelType.New);
					}
					else
					{
						Draw_CurrentImage(e);
						_cachedColorMatrix.Matrix33 = 0.003921569f * (255f * _framePercentage / 100f);
						_cachedImageAttributes.SetColorMatrix(_cachedColorMatrix);
						slideDistance = 0.15f * (float)_newCombinedImage.Height;
						offsetY = (int)(slideDistance - slideDistance * _framePercentage / 100f);
						DrawImageToOutput(e, _newCombinedImage, base.ClientRectangle, new Rectangle(0, offsetY, _newCombinedImage.Width, _newCombinedImage.Height), _cachedImageAttributes);
					}
				}
				else if (_activeTransitionType == TransitionTypes.DoorsOpen)
				{
					CreateFrameWorkSpace(_newBackgroundPicture.Width, _newBackgroundPicture.Height);
					try
					{
						int leftSrcX = 0;
						int leftSrcY = 0;
						int leftSrcWidth = Convert.ToInt32(_newBackgroundPicture.Width / 2);
						int leftSrcHeight = _newBackgroundPicture.Height;
						Rectangle leftDestRect = new(0, 0, Convert.ToInt32((float)(_newBackgroundPicture.Width / 2) - (float)_newBackgroundPicture.Width * _framePercentage / 200f), _newBackgroundPicture.Height);
						int rightSrcX = Convert.ToInt32(_newBackgroundPicture.Width / 2);
						int rightSrcY = 0;
						int rightSrcWidth = Convert.ToInt32(_newBackgroundPicture.Width / 2);
						int rightSrcHeight = _newBackgroundPicture.Height;
						Rectangle rightDestRect = new(Convert.ToInt32((float)(_newBackgroundPicture.Width / 2) + (float)_newBackgroundPicture.Width * _framePercentage / 200f), 0, Convert.ToInt32((float)(_newBackgroundPicture.Width / 2) - (float)_newBackgroundPicture.Width * _framePercentage / 200f), _newBackgroundPicture.Height);
						if (TransitBackPictureAction == BackPicturesTransition.None)
						{
							Draw_NewImage(e);
							wsg.DrawImage(_newBackgroundPicture, leftDestRect, leftDestRect.X, leftDestRect.Y, leftDestRect.Width, leftDestRect.Height, GraphicsUnit.Pixel);
							wsg.DrawImage(_newBackgroundPicture, rightDestRect, rightDestRect.X, rightDestRect.Y, rightDestRect.Width, rightDestRect.Height, GraphicsUnit.Pixel);
							wsg.DrawImage(_currentTextImage, leftDestRect, leftSrcX, leftSrcY, leftSrcWidth, leftSrcHeight, GraphicsUnit.Pixel);
							wsg.DrawImage(_currentTextImage, rightDestRect, rightSrcX, rightSrcY, rightSrcWidth, rightSrcHeight, GraphicsUnit.Pixel);
							DrawImageToOutput(e, _workSpace, base.ClientRectangle, new Rectangle(0, 0, _workSpace.Width, _workSpace.Height), null, PanelType.Current);
						}
						else
						{
							Draw_NewImage(e);
							wsg.DrawImage(_currentCombinedImage, leftDestRect, leftSrcX, leftSrcY, leftSrcWidth, leftSrcHeight, GraphicsUnit.Pixel);
							wsg.DrawImage(_currentCombinedImage, rightDestRect, rightSrcX, rightSrcY, rightSrcWidth, rightSrcHeight, GraphicsUnit.Pixel);
							DrawImageToOutput(e, _workSpace, base.ClientRectangle, new Rectangle(0, 0, _workSpace.Width, _workSpace.Height), null);
						}
					}
					finally
					{
						// The work space's Graphics is made for this frame only (the bitmap is kept).
						wsg.Dispose();
					}
				}
				else if (_activeTransitionType == TransitionTypes.DoorsClose)
				{
					CreateFrameWorkSpace(_newBackgroundPicture.Width, _newBackgroundPicture.Height);
					try
					{
						int leftSrcX = 0;
						int leftSrcY = 0;
						int leftSrcWidth = Convert.ToInt32(_newBackgroundPicture.Width / 2);
						int leftSrcHeight = _newBackgroundPicture.Height;
						Rectangle leftDestRect = new(0, 0, Convert.ToInt32((float)_newBackgroundPicture.Width * _framePercentage / 200f), _newBackgroundPicture.Height);
						int rightSrcX = Convert.ToInt32(_newBackgroundPicture.Width / 2);
						int rightSrcY = 0;
						int rightSrcWidth = Convert.ToInt32(_newBackgroundPicture.Width / 2);
						int rightSrcHeight = _newBackgroundPicture.Height;
						Rectangle rightDestRect = new(Convert.ToInt32((float)_newBackgroundPicture.Width - (float)_newBackgroundPicture.Width * _framePercentage / 200f), 0, Convert.ToInt32((float)_newBackgroundPicture.Width * _framePercentage / 200f), _newBackgroundPicture.Height);
						if (TransitBackPictureAction == BackPicturesTransition.None)
						{
							Draw_CurrentImage(e);
							wsg.DrawImage(_newBackgroundPicture, leftDestRect, leftDestRect.X, leftDestRect.Y, leftDestRect.Width, leftDestRect.Height, GraphicsUnit.Pixel);
							wsg.DrawImage(_newBackgroundPicture, rightDestRect, rightDestRect.X, rightDestRect.Y, rightDestRect.Width, rightDestRect.Height, GraphicsUnit.Pixel);
							wsg.DrawImage(_newTextImage, leftDestRect, leftSrcX, leftSrcY, leftSrcWidth, leftSrcHeight, GraphicsUnit.Pixel);
							wsg.DrawImage(_newTextImage, rightDestRect, rightSrcX, rightSrcY, rightSrcWidth, rightSrcHeight, GraphicsUnit.Pixel);
							DrawImageToOutput(e, _workSpace, base.ClientRectangle, new Rectangle(0, 0, _workSpace.Width, _workSpace.Height), null, PanelType.New);
						}
						else
						{
							Draw_CurrentImage(e);
							wsg.DrawImage(_newCombinedImage, leftDestRect, leftSrcX, leftSrcY, leftSrcWidth, leftSrcHeight, GraphicsUnit.Pixel);
							wsg.DrawImage(_newCombinedImage, rightDestRect, rightSrcX, rightSrcY, rightSrcWidth, rightSrcHeight, GraphicsUnit.Pixel);
							DrawImageToOutput(e, _workSpace, base.ClientRectangle, new Rectangle(0, 0, _workSpace.Width, _workSpace.Height), null);
						}
					}
					finally
					{
						// The work space's Graphics is made for this frame only (the bitmap is kept).
						wsg.Dispose();
					}
				}
				else if (_activeTransitionType == TransitionTypes.Blinds_H)
				{
					Draw_CurrentImage(e);
					for (int i = 0; i <= _nHDivs; i++)
					{
						DrawImageToOutput(SourceRectangle: new Rectangle(0, i * (_newCombinedImage.Height / _nHDivs), _newCombinedImage.Width, Convert.ToInt32((float)(_newCombinedImage.Height / _nHDivs) * _framePercentage / 100f)), DestinationRectangle: new Rectangle(0, i * (base.Height / _nHDivs), base.Width, Convert.ToInt32((float)(base.Height / _nHDivs) * _framePercentage / 100f)), e: e, InImage: _newCombinedImage, InImageAttributes: null);
					}
				}
				else if (_activeTransitionType == TransitionTypes.Blinds_V)
				{
					Draw_CurrentImage(e);
					for (int j = 0; j <= _nVDivs; j++)
					{
						DrawImageToOutput(SourceRectangle: new Rectangle(j * (_newCombinedImage.Width / _nVDivs), 0, Convert.ToInt32((float)(_newCombinedImage.Width / _nVDivs) * _framePercentage / 100f), _newCombinedImage.Height), DestinationRectangle: new Rectangle(j * (base.Width / _nVDivs), 0, Convert.ToInt32((float)(base.Width / _nVDivs) * _framePercentage / 100f), base.Height), e: e, InImage: _newCombinedImage, InImageAttributes: null);
					}
				}
				else if (_activeTransitionType == TransitionTypes.Rectangle)
				{
					Draw_CurrentImage(e);
					graphicsPath = FrameObject(new GraphicsPath());
					int halfWidth = Convert.ToInt32((float)base.Width * _framePercentage / 200f);
					int halfHeight = Convert.ToInt32((float)base.Height * _framePercentage / 200f);
					graphicsPath.AddRectangle(new Rectangle(Convert.ToInt32(base.Width / 2 - halfWidth), Convert.ToInt32(base.Height / 2 - halfHeight), Convert.ToInt32(2 * halfWidth), Convert.ToInt32(2 * halfHeight)));
					e.Graphics.SetClip(graphicsPath, CombineMode.Replace);
					Draw_NewImage(e);
					graphicsPath.Dispose();
				}
				else if (_activeTransitionType == TransitionTypes.RectangleIn)
				{
					// The current picture stays inside a rectangle that shrinks to the centre, the new one
					// shows around it. Drawn in the order of the other shape transitions (current picture
					// with alert and reference text first, then the new picture inside the clip), so the
					// alert and reference text drawn after this branch, inside the same clip, come out once.
					Draw_CurrentImage(e);
					graphicsPath = FrameObject(new GraphicsPath());
					// Measured from the edges inwards, so the first frame's rectangle is exactly the client
					// area (also for an odd width or height).
					int insetX = Convert.ToInt32((float)base.Width * _framePercentage / 200f);
					int insetY = Convert.ToInt32((float)base.Height * _framePercentage / 200f);
					graphicsPath.AddRectangle(new Rectangle(insetX, insetY, Math.Max(0, base.Width - 2 * insetX), Math.Max(0, base.Height - 2 * insetY)));
					e.Graphics.SetClip(base.ClientRectangle, CombineMode.Replace);
					e.Graphics.SetClip(graphicsPath, CombineMode.Exclude);
					Draw_NewImage(e);
					graphicsPath.Dispose();
				}
				else if (_activeTransitionType == TransitionTypes.Checkerboard)
				{
					Draw_CurrentImage(e);
					graphicsPath = FrameObject(new GraphicsPath());
					int cellWidth = Convert.ToInt32((float)base.Width * _framePercentage / 100f) / _nHDivs;
					// At least 1 px: on a screen smaller than the divisions the steps were 0 and the loops never ended.
					int cellHeight = Math.Max(1, base.Height / _nVDivs);
					int stepX = Math.Max(1, base.Width / _nHDivs);
					int rowIndex = 0;
					for (int i = 0; i <= base.Height; i += cellHeight)
					{
						for (int j = 0; j <= base.Width; j += stepX)
						{
							Rectangle rect = new(j, i, cellWidth, cellHeight);
							if ((rowIndex & 1) == 1)
							{
								rect.Offset(base.Width / (2 * _nVDivs), 0);
							}
							graphicsPath.AddRectangle(rect);
							if (_framePercentage >= 50f && (rowIndex & 1) == 1 && j == 0)
							{
								rect.Offset(-(base.Width / _nHDivs), 0);
								graphicsPath.AddRectangle(rect);
							}
						}
						rowIndex++;
					}
					Region region = FrameObject(new Region(graphicsPath));
					e.Graphics.SetClip(region, CombineMode.Replace);
					Draw_NewImage(e);
					region.Dispose();
					graphicsPath.Dispose();
				}
				else if (_activeTransitionType == TransitionTypes.Circle)
				{
					Draw_CurrentImage(e);
					graphicsPath = FrameObject(new GraphicsPath());
					int radius = Convert.ToInt32((float)base.Width * 1.414f * _framePercentage / 200f);
					graphicsPath.AddEllipse(Convert.ToInt32(base.Width / 2 - radius), Convert.ToInt32(base.Height / 2 - radius), Convert.ToInt32(2 * radius), Convert.ToInt32(2 * radius));
					e.Graphics.SetClip(graphicsPath, CombineMode.Replace);
					Draw_NewImage(e);
					graphicsPath.Dispose();
				}
				else if (_activeTransitionType == TransitionTypes.Cross)
				{
					Draw_CurrentImage(e);
					graphicsPath = FrameObject(new GraphicsPath());
					graphicsPath2 = FrameObject(new GraphicsPath());
					int armHalfLength = Convert.ToInt32((float)base.Width * _framePercentage / 200f);
					int armHalfWidth = Convert.ToInt32((float)base.Height * _framePercentage / 40f);
					graphicsPath.AddRectangle(new Rectangle(Convert.ToInt32(base.Width / 2 - armHalfWidth), Convert.ToInt32(base.Height / 2 - armHalfLength), Convert.ToInt32(2 * armHalfWidth), Convert.ToInt32(2 * armHalfLength)));
					e.Graphics.SetClip(graphicsPath, CombineMode.Replace);
					graphicsPath.AddRectangle(new Rectangle(Convert.ToInt32(base.Width / 2 - armHalfLength), Convert.ToInt32(base.Height / 2 - armHalfWidth), Convert.ToInt32(2 * armHalfLength), Convert.ToInt32(2 * armHalfWidth)));
					e.Graphics.SetClip(graphicsPath, CombineMode.Union);
					Draw_NewImage(e);
					graphicsPath.Dispose();
				}
				else if (_activeTransitionType == TransitionTypes.Diamond)
				{
					Draw_CurrentImage(e);
					int halfWidth = Convert.ToInt32((float)base.Width * _framePercentage / 100f);
					int halfHeight = Convert.ToInt32((float)base.Height * _framePercentage / 100f);
					int centerX = Convert.ToInt32(base.Width / 2);
					int centerY = Convert.ToInt32(base.Height / 2);
					graphicsPath = FrameObject(new GraphicsPath());
					graphicsPath.AddPolygon((ReadOnlySpan<Point>)[
                    new(centerX, centerY - halfHeight),
						new(centerX - halfWidth, centerY),
						new(centerX, centerY + halfHeight),
						new(centerX + halfWidth, centerY)
					]);
					e.Graphics.SetClip(graphicsPath, CombineMode.Replace);
					Draw_NewImage(e);
					graphicsPath.Dispose();
				}
				else if (_activeTransitionType == TransitionTypes.Fade)
				{
					Draw_CurrentImage(e);
					// Reusing cached ImageAttributes
					// Reusing cached ColorMatrix
					_cachedColorMatrix.Matrix33 = 0.003921569f * (255f * _framePercentage / 100f);
					_cachedImageAttributes.SetColorMatrix(_cachedColorMatrix);
					DrawImageToOutput(e, _newCombinedImage, base.ClientRectangle, new Rectangle(0, 0, _newCombinedImage.Width, _newCombinedImage.Height), _cachedImageAttributes);
				}
				else if (_activeTransitionType == TransitionTypes.Flip_H)
				{
					BackPicturesTransition transitBackPictureAction = TransitBackPictureAction;
					if (transitBackPictureAction == BackPicturesTransition.BothBackgrounds)
					{
						DrawDefaultBackPattern(e);
						if (_framePercentage < 50f)
						{
							DrawImageToOutput(e, _currentCombinedImage, new Rectangle(0, Convert.ToInt32((float)base.Height * _framePercentage / 100f), base.ClientRectangle.Width, Convert.ToInt32((float)base.Height - (float)base.Height * _framePercentage / 50f)), new Rectangle(0, 0, _currentCombinedImage.Width, _currentCombinedImage.Height), null);
						}
						else
						{
							DrawImageToOutput(e, _newCombinedImage, new Rectangle(0, Convert.ToInt32((float)base.Height - (float)base.Height * _framePercentage / 100f), base.ClientRectangle.Width, Convert.ToInt32((float)base.Height * (_framePercentage - 50f) / 50f)), new Rectangle(0, 0, _currentCombinedImage.Width, _currentCombinedImage.Height), null);
						}
					}
					else
					{
						CreateFrameWorkSpace(_newBackgroundPicture.Width, _newBackgroundPicture.Height);
						int srcX = 0;
						int srcY = 0;
						int srcWidth = _currentCombinedImage.Width;
						int srcHeight = _currentCombinedImage.Height;
						int destX = 0;
						int destY = 0;
						int destWidth = _currentCombinedImage.Width;
						int destHeight = 0;
						switch (TransitBackPictureAction)
						{
							case BackPicturesTransition.CurrentOnly:
								DrawBackPatternOnWorkSpace(ref wsg, BackgroundImage);
								if (_framePercentage < 50f)
								{
									destY = Convert.ToInt32((float)_currentCombinedImage.Height * _framePercentage / 100f);
									destHeight = Convert.ToInt32((float)_currentCombinedImage.Height - (float)_currentCombinedImage.Height * _framePercentage / 50f);
									Rectangle destRect = new(destX, destY, destWidth, destHeight);
									wsg.DrawImage(_currentCombinedImage, destRect, srcX, srcY, srcWidth, srcHeight, GraphicsUnit.Pixel);
									DrawImageToOutput(e, _workSpace, base.ClientRectangle, new Rectangle(0, 0, _workSpace.Width, _workSpace.Height), null);
								}
								else
								{
									destY = Convert.ToInt32((float)_currentCombinedImage.Height - (float)_currentCombinedImage.Height * _framePercentage / 100f);
									destHeight = Convert.ToInt32((float)_currentCombinedImage.Height * (_framePercentage - 50f) / 50f);
									Rectangle destRect = new(destX, destY, destWidth, destHeight);
									wsg.DrawImage(_newTextImage, destRect, srcX, srcY, srcWidth, srcHeight, GraphicsUnit.Pixel);
									wsg.DrawImage(_newPanelImage, destRect, srcX, srcY, srcWidth, srcHeight, GraphicsUnit.Pixel);
									DrawImageToOutput(e, _workSpace, base.ClientRectangle, new Rectangle(0, 0, _workSpace.Width, _workSpace.Height), null);
								}
								break;
							case BackPicturesTransition.NewOnly:
								DrawBackPatternOnWorkSpace(ref wsg, BackgroundImage);
								if (_framePercentage < 50f)
								{
									destY = Convert.ToInt32((float)_currentCombinedImage.Height * _framePercentage / 100f);
									destHeight = Convert.ToInt32((float)_currentCombinedImage.Height - (float)_currentCombinedImage.Height * _framePercentage / 50f);
									Rectangle destRect = new(destX, destY, destWidth, destHeight);
									wsg.DrawImage(_currentTextImage, destRect, srcX, srcY, srcWidth, srcHeight, GraphicsUnit.Pixel);
									wsg.DrawImage(_currentPanelImage, destRect, srcX, srcY, srcWidth, srcHeight, GraphicsUnit.Pixel);
									DrawImageToOutput(e, _workSpace, base.ClientRectangle, new Rectangle(0, 0, _workSpace.Width, _workSpace.Height), null);
								}
								else
								{
									destY = Convert.ToInt32((float)_currentCombinedImage.Height - (float)_currentCombinedImage.Height * _framePercentage / 100f);
									destHeight = Convert.ToInt32((float)_currentCombinedImage.Height * (_framePercentage - 50f) / 50f);
									Rectangle destRect = new(destX, destY, destWidth, destHeight);
									wsg.DrawImage(_newCombinedImage, destRect, srcX, srcY, srcWidth, srcHeight, GraphicsUnit.Pixel);
									DrawImageToOutput(e, _workSpace, base.ClientRectangle, new Rectangle(0, 0, _workSpace.Width, _workSpace.Height), null);
								}
								break;
							default:
								DrawBackPatternOnWorkSpace(ref wsg, CurrentBackgroundPicture);
								if (_framePercentage < 50f)
								{
									destY = Convert.ToInt32((float)_currentCombinedImage.Height * _framePercentage / 100f);
									destHeight = Convert.ToInt32((float)_currentCombinedImage.Height - (float)_currentCombinedImage.Height * _framePercentage / 50f);
									Rectangle destRect = new(destX, destY, destWidth, destHeight);
									wsg.DrawImage(_currentTextImage, destRect, srcX, srcY, srcWidth, srcHeight, GraphicsUnit.Pixel);
									DrawImageToOutput(e, _workSpace, base.ClientRectangle, new Rectangle(0, 0, _workSpace.Width, _workSpace.Height), null, PanelType.Current);
								}
								else
								{
									destY = Convert.ToInt32((float)_currentCombinedImage.Height - (float)_currentCombinedImage.Height * _framePercentage / 100f);
									destHeight = Convert.ToInt32((float)_currentCombinedImage.Height * (_framePercentage - 50f) / 50f);
									Rectangle destRect = new(destX, destY, destWidth, destHeight);
									wsg.DrawImage(_newTextImage, destRect, srcX, srcY, srcWidth, srcHeight, GraphicsUnit.Pixel);
									DrawImageToOutput(e, _workSpace, base.ClientRectangle, new Rectangle(0, 0, _workSpace.Width, _workSpace.Height), null, PanelType.New);
								}
								break;
						}
						wsg.Dispose();
					}
					
				}
				else if (_activeTransitionType == TransitionTypes.Flip_V)
				{
					BackPicturesTransition transitBackPictureAction = TransitBackPictureAction;
					if (transitBackPictureAction == BackPicturesTransition.BothBackgrounds)
					{
						DrawDefaultBackPattern(e);
						if (_framePercentage < 50f)
						{
							DrawImageToOutput(e, _currentCombinedImage, new Rectangle(Convert.ToInt32((float)base.Width * _framePercentage / 100f), 0, Convert.ToInt32((float)base.Width - (float)base.Width * _framePercentage / 50f), base.ClientRectangle.Height), new Rectangle(0, 0, _currentCombinedImage.Width, _currentCombinedImage.Height), null);
						}
						else
						{
							DrawImageToOutput(e, _newCombinedImage, new Rectangle(Convert.ToInt32((float)base.Width - (float)base.Width * _framePercentage / 100f), 0, Convert.ToInt32((float)base.Width * (_framePercentage - 50f) / 50f), base.ClientRectangle.Height), new Rectangle(0, 0, _currentCombinedImage.Width, _currentCombinedImage.Height), null);
						}
					}
					else
					{
						CreateFrameWorkSpace(_newBackgroundPicture.Width, _newBackgroundPicture.Height);
						int srcX = 0;
						int srcY = 0;
						int srcWidth = _currentCombinedImage.Width;
						int srcHeight = _currentCombinedImage.Height;
						int destX = 0;
						int destY = 0;
						int destWidth = 0;
						int destHeight = _currentCombinedImage.Height;
						switch (TransitBackPictureAction)
						{
							case BackPicturesTransition.CurrentOnly:
								DrawBackPatternOnWorkSpace(ref wsg, BackgroundImage);
								if (_framePercentage < 50f)
								{
									destX = Convert.ToInt32((float)_currentCombinedImage.Width * _framePercentage / 100f);
									destWidth = Convert.ToInt32((float)_currentCombinedImage.Width - (float)_currentCombinedImage.Width * _framePercentage / 50f);
									Rectangle destRect = new(destX, destY, destWidth, destHeight);
									wsg.DrawImage(_currentCombinedImage, destRect, srcX, srcY, srcWidth, srcHeight, GraphicsUnit.Pixel);
									DrawImageToOutput(e, _workSpace, base.ClientRectangle, new Rectangle(0, 0, _workSpace.Width, _workSpace.Height), null);
								}
								else
								{
									destX = Convert.ToInt32((float)_currentCombinedImage.Width - (float)_currentCombinedImage.Width * _framePercentage / 100f);
									destWidth = Convert.ToInt32((float)_currentCombinedImage.Width * (_framePercentage - 50f) / 50f);
									Rectangle destRect = new(destX, destY, destWidth, destHeight);
									wsg.DrawImage(_newTextImage, destRect, srcX, srcY, srcWidth, srcHeight, GraphicsUnit.Pixel);
									wsg.DrawImage(_newPanelImage, destRect, srcX, srcY, srcWidth, srcHeight, GraphicsUnit.Pixel);
									DrawImageToOutput(e, _workSpace, base.ClientRectangle, new Rectangle(0, 0, _workSpace.Width, _workSpace.Height), null);
								}
								break;
							case BackPicturesTransition.NewOnly:
								DrawBackPatternOnWorkSpace(ref wsg, BackgroundImage);
								if (_framePercentage < 50f)
								{
									destX = Convert.ToInt32((float)_currentCombinedImage.Width * _framePercentage / 100f);
									destWidth = Convert.ToInt32((float)_currentCombinedImage.Width - (float)_currentCombinedImage.Width * _framePercentage / 50f);
									Rectangle destRect = new(destX, destY, destWidth, destHeight);
									wsg.DrawImage(_currentTextImage, destRect, srcX, srcY, srcWidth, srcHeight, GraphicsUnit.Pixel);
									DrawImageToOutput(e, _workSpace, base.ClientRectangle, new Rectangle(0, 0, _workSpace.Width, _workSpace.Height), null, PanelType.CurrentWithTrans);
								}
								else
								{
									destX = Convert.ToInt32((float)_currentCombinedImage.Width - (float)_currentCombinedImage.Width * _framePercentage / 100f);
									destWidth = Convert.ToInt32((float)_currentCombinedImage.Width * (_framePercentage - 50f) / 50f);
									Rectangle destRect = new(destX, destY, destWidth, destHeight);
									wsg.DrawImage(_newCombinedImage, destRect, srcX, srcY, srcWidth, srcHeight, GraphicsUnit.Pixel);
									DrawImageToOutput(e, _workSpace, base.ClientRectangle, new Rectangle(0, 0, _workSpace.Width, _workSpace.Height), null);
								}
								break;
							default:
								DrawBackPatternOnWorkSpace(ref wsg, CurrentBackgroundPicture);
								if (_framePercentage < 50f)
								{
									destX = Convert.ToInt32((float)_currentCombinedImage.Width * _framePercentage / 100f);
									destWidth = Convert.ToInt32((float)_currentCombinedImage.Width - (float)_currentCombinedImage.Width * _framePercentage / 50f);
									Rectangle destRect = new(destX, destY, destWidth, destHeight);
									wsg.DrawImage(_currentTextImage, destRect, srcX, srcY, srcWidth, srcHeight, GraphicsUnit.Pixel);
									DrawImageToOutput(e, _workSpace, base.ClientRectangle, new Rectangle(0, 0, _workSpace.Width, _workSpace.Height), null, PanelType.Current);
								}
								else
								{
									destX = Convert.ToInt32((float)_currentCombinedImage.Width - (float)_currentCombinedImage.Width * _framePercentage / 100f);
									destWidth = Convert.ToInt32((float)_currentCombinedImage.Width * (_framePercentage - 50f) / 50f);
									Rectangle destRect = new(destX, destY, destWidth, destHeight);
									wsg.DrawImage(_newTextImage, destRect, srcX, srcY, srcWidth, srcHeight, GraphicsUnit.Pixel);
									DrawImageToOutput(e, _workSpace, base.ClientRectangle, new Rectangle(0, 0, _workSpace.Width, _workSpace.Height), null, PanelType.New);
								}
								break;
						}
						wsg.Dispose();
					}
				}
				else if (_activeTransitionType == TransitionTypes.Gentle_Zoom)
				{
					float zoom = 0f;
					if (TransitBackPictureAction == BackPicturesTransition.None)
					{
						zoom = 0.85f + 0.15f * _framePercentage / 100f;
						if (zoom < 0f)
						{
							zoom = 0.85f;
						}
						else if (zoom > 1f)
						{
							zoom = 1f;
						}
						Draw_CurrentImage(e);
						float dx = _newBackgroundPicture.Width / 2;
						float dy = _newBackgroundPicture.Height / 2;
						CreateFrameWorkSpace(_newBackgroundPicture.Width, _newBackgroundPicture.Height);
						Rectangle destRect = new(-_newBackgroundPicture.Width / 2, -1 - _newBackgroundPicture.Height / 2, _newBackgroundPicture.Width, _newBackgroundPicture.Height);
						Rectangle rectangle = new(destRect.Width / 2 - (int)(zoom * (float)destRect.Width / 2f), destRect.Height / 2 - (int)(zoom * (float)destRect.Height / 2f), (int)(zoom * (float)destRect.Width), (int)(zoom * (float)destRect.Height));
						wsg.DrawImage(_newBackgroundPicture, rectangle, rectangle, GraphicsUnit.Pixel);
						Matrix matrix = FrameObject(new Matrix(zoom, 0f, 0f, zoom, dx, dy));
						wsg.Transform = matrix;
						wsg.DrawImage(_newTextImage, destRect, 0, 0, _newBackgroundPicture.Width, _newBackgroundPicture.Height, GraphicsUnit.Pixel);
						DrawImageToOutput(e, _workSpace, base.ClientRectangle, new Rectangle(0, 0, _workSpace.Width, _workSpace.Height), null, PanelType.New);
						matrix.Dispose();
						wsg.Dispose();
					}
					else
					{
						zoom = 0.8f + 0.2f * _framePercentage / 100f;
						if (zoom < 0f)
						{
							zoom = 0.8f;
						}
						else if (zoom > 1f)
						{
							zoom = 1f;
						}
						// Reusing cached ImageAttributes
						// Reusing cached ColorMatrix
						float alpha = 0f;
						alpha = 0.003921569f * (255f * (1f - _framePercentage / 60f));
						alpha = (_cachedColorMatrix.Matrix33 = ((alpha > 1f) ? 1f : ((alpha < 0f) ? 0f : alpha)));
						_cachedImageAttributes.SetColorMatrix(_cachedColorMatrix);
						DrawImageToOutput(e, _currentCombinedImage, base.ClientRectangle, new Rectangle(0, 0, _currentBackgroundPicture.Width, _currentBackgroundPicture.Height), _cachedImageAttributes);
						alpha = 0.003921569f * (510f * _framePercentage / 100f);
						alpha = (_cachedColorMatrix.Matrix33 = ((alpha > 1f) ? 1f : ((alpha < 0f) ? 0f : alpha)));
						_cachedImageAttributes.SetColorMatrix(_cachedColorMatrix);
						float dx = base.Width / 2;
						float dy = base.Height / 2;
						Matrix matrix = FrameObject(new Matrix(zoom, 0f, 0f, zoom, dx, dy));
						e.Graphics.Transform = matrix;
						DrawImageToOutput(e, _newCombinedImage, new Rectangle(-base.Width / 2, -1 - base.Height / 2, base.Width, base.Height), new Rectangle(0, 0, _newCombinedImage.Width, _newCombinedImage.Height), _cachedImageAttributes);
						matrix.Dispose();
					}
				}
				else if (_activeTransitionType == TransitionTypes.Oval)
				{
					Draw_CurrentImage(e);
					graphicsPath = FrameObject(new GraphicsPath());
					int radiusX = Convert.ToInt32((float)base.Width * 1.414f * _framePercentage / 200f);
					int radiusY = Convert.ToInt32((float)base.Height * 1.2f * _framePercentage / 200f);
					graphicsPath.AddEllipse(Convert.ToInt32(base.Width / 2 - radiusX), Convert.ToInt32(base.Height / 2 - radiusY), Convert.ToInt32(2 * radiusX), Convert.ToInt32(2 * radiusY));
					e.Graphics.SetClip(graphicsPath, CombineMode.Replace);
					Draw_NewImage(e);
					graphicsPath.Dispose();
				}
				else if (_activeTransitionType == TransitionTypes.Mesh)
				{
					Draw_CurrentImage(e);
					int columnCount = _nHDivs * 5;
					int rowCount = _nVDivs * 8;
					graphicsPath = FrameObject(new GraphicsPath());
					int cellWidth = Convert.ToInt32((float)base.Width * _framePercentage / 100f) / columnCount;
					// At least 1 px: on a screen under 64 px high (or 50 px wide) the steps were 0 and the loops never ended.
					int cellHeight = Math.Max(1, base.Height / rowCount);
					int stepX = Math.Max(1, base.Width / columnCount);
					int rowIndex = 0;
					for (int i = 0; i <= base.Height; i += cellHeight)
					{
						for (int j = 0; j <= base.Width; j += stepX)
						{
							Rectangle rect = new(j, i, cellWidth, cellHeight);
							if ((rowIndex & 1) == 1)
							{
								rect.Offset(base.Width / (2 * rowCount), 0);
							}
							graphicsPath.AddRectangle(rect);
							if (_framePercentage >= 50f && (rowIndex & 1) == 1 && j == 0)
							{
								rect.Offset(-(base.Width / columnCount), 0);
								graphicsPath.AddRectangle(rect);
							}
						}
						rowIndex++;
					}
					Region region = FrameObject(new Region(graphicsPath));
					e.Graphics.SetClip(region, CombineMode.Replace);
					Draw_NewImage(e);
					region.Dispose();
					graphicsPath.Dispose();
				}
				else if (_activeTransitionType == TransitionTypes.InLeft)
				{
					Draw_TranSlideIn(e, _activeTransitionType);
				}
				else if (_activeTransitionType == TransitionTypes.InTop)
				{
					Draw_TranSlideIn(e, _activeTransitionType);
				}
				else if (_activeTransitionType == TransitionTypes.InRight)
				{
					Draw_TranSlideIn(e, _activeTransitionType);
				}
				else if (_activeTransitionType == TransitionTypes.InBottom)
				{
					Draw_TranSlideIn(e, _activeTransitionType);
				}
				else if (_activeTransitionType == TransitionTypes.InTopLeft)
				{
					Draw_TranSlideIn(e, _activeTransitionType);
				}
				else if (_activeTransitionType == TransitionTypes.InTopRight)
				{
					Draw_TranSlideIn(e, _activeTransitionType);
				}
				else if (_activeTransitionType == TransitionTypes.InBottomLeft)
				{
					Draw_TranSlideIn(e, _activeTransitionType);
				}
				else if (_activeTransitionType == TransitionTypes.InBottomRight)
				{
					Draw_TranSlideIn(e, _activeTransitionType);
				}
				else if (_activeTransitionType == TransitionTypes.OutLeft)
				{
					Draw_TranSlideOut(e, _activeTransitionType);
				}
				else if (_activeTransitionType == TransitionTypes.OutTop)
				{
					Draw_TranSlideOut(e, _activeTransitionType);
				}
				else if (_activeTransitionType == TransitionTypes.OutRight)
				{
					Draw_TranSlideOut(e, _activeTransitionType);
				}
				else if (_activeTransitionType == TransitionTypes.OutBottom)
				{
					Draw_TranSlideOut(e, _activeTransitionType);
				}
				else if (_activeTransitionType == TransitionTypes.OutTopLeft)
				{
					Draw_TranSlideOut(e, _activeTransitionType);
				}
				else if (_activeTransitionType == TransitionTypes.OutTopRight)
				{
					Draw_TranSlideOut(e, _activeTransitionType);
				}
				else if (_activeTransitionType == TransitionTypes.OutBottomLeft)
				{
					Draw_TranSlideOut(e, _activeTransitionType);
				}
				else if (_activeTransitionType == TransitionTypes.OutBottomRight)
				{
					Draw_TranSlideOut(e, _activeTransitionType);
				}
				else if (_activeTransitionType == TransitionTypes.RevealLeftRight)
				{
					Draw_TranSlideReveal(e, _activeTransitionType);
				}
				else if (_activeTransitionType == TransitionTypes.RevealTopDown)
				{
					Draw_TranSlideReveal(e, _activeTransitionType);
				}
				else if (_activeTransitionType == TransitionTypes.RevealRightLeft)
				{
					Draw_TranSlideReveal(e, _activeTransitionType);
				}
				else if (_activeTransitionType == TransitionTypes.RevealDownUp)
				{
					Draw_TranSlideReveal(e, _activeTransitionType);
				}
				else if (_activeTransitionType == TransitionTypes.Scroll)
				{
					int offsetY = (int)((float)_newCombinedImage.Height * _framePercentage / 100f);
					if (TransitBackPictureAction == BackPicturesTransition.None)
					{
						DrawImageToOutput(e, _currentBackgroundPicture, base.ClientRectangle, new Rectangle(0, 0, _currentBackgroundPicture.Width, _currentBackgroundPicture.Height), null);
						DrawImageToOutput(e, _currentTextImage, base.ClientRectangle, new Rectangle(0, offsetY, _currentBackgroundPicture.Width, _currentBackgroundPicture.Height), null, PanelType.Current);
						DrawImageToOutput(e, _newTextImage, base.ClientRectangle, new Rectangle(0, offsetY - _newCombinedImage.Height, _newCombinedImage.Width, _newCombinedImage.Height), null);
					}
					else
					{
						DrawImageToOutput(e, _currentCombinedImage, base.ClientRectangle, new Rectangle(0, offsetY, _currentBackgroundPicture.Width, _currentBackgroundPicture.Height), null);
						DrawImageToOutput(e, _newCombinedImage, base.ClientRectangle, new Rectangle(0, offsetY - _newCombinedImage.Height, _newCombinedImage.Width, _newCombinedImage.Height), null);
					}
				}
				else if (_activeTransitionType == TransitionTypes.Spin)
				{
					float angle = 360f * _framePercentage / 100f;
					float dx = base.Width / 2;
					float dy = base.Height / 2;
					float scale = 1f * _framePercentage / 100f;
					if (scale == 0f)
					{
						scale = 0.01f;
					}
					if (TransitBackPictureAction == BackPicturesTransition.None)
					{
						DrawImageToOutput(e, _newBackgroundPicture, base.ClientRectangle, new Rectangle(0, 0, _newBackgroundPicture.Width, _newBackgroundPicture.Height), null);
						// Reusing cached ImageAttributes
						// Reusing cached ColorMatrix
						_cachedColorMatrix.Matrix33 = 0.003921569f * (255f * (1f - _framePercentage / 100f));
						_cachedImageAttributes.SetColorMatrix(_cachedColorMatrix);
						DrawImageToOutput(e, _currentTextImage, base.ClientRectangle, new Rectangle(0, 0, _newCombinedImage.Width, _newCombinedImage.Height), _cachedImageAttributes, PanelType.Current);
						DrawImageToOutput(e, _currentPanelImage, base.ClientRectangle, new Rectangle(0, 0, _newCombinedImage.Width, _newCombinedImage.Height), null, PanelType.Current);
						Matrix matrix = FrameObject(new Matrix(scale, 0f, 0f, scale, dx, dy));
						matrix.Rotate(angle, MatrixOrder.Prepend);
						e.Graphics.Transform = matrix;
						DrawImageToOutput(e, _newTextImage, new Rectangle(-base.Width / 2, -base.Height / 2, base.Width, base.Height), new Rectangle(0, 0, _newCombinedImage.Width, _newCombinedImage.Height), null);
						matrix.Dispose();
					}
					else
					{
						Draw_CurrentImage(e);
						Matrix matrix = FrameObject(new Matrix(scale, 0f, 0f, scale, dx, dy));
						matrix.Rotate(angle, MatrixOrder.Prepend);
						e.Graphics.Transform = matrix;
						DrawImageToOutput(e, _newCombinedImage, new Rectangle(-base.Width / 2, -base.Height / 2, base.Width, base.Height), new Rectangle(0, 0, _newCombinedImage.Width, _newCombinedImage.Height), null);
						matrix.Dispose();
					}
				}
				else if (_activeTransitionType == TransitionTypes.Spiral)
				{
					double angleStep = Math.PI / 100.0;
					double radiusStep = Math.Max(base.Width * 2 / 3, base.Height * 2 / 3) / 100;
					float centerX = (float)base.Width * 0.59f;
					float centerY = (float)base.Height * 0.1475f;
					double spiralPos = _framePercentage - 100f;
					double spiralEnd = _framePercentage;
					if (spiralPos < 0.0)
					{
						spiralPos = 0.0;
					}
					double spiralAngle = angleStep * spiralEnd;
					PointF position = new(Convert.ToSingle((double)centerX + spiralPos * radiusStep * Math.Cos(spiralAngle)), Convert.ToSingle((double)centerY + spiralPos * radiusStep * Math.Sin(spiralAngle)));
					spiralAngle = angleStep * spiralPos;
					while (spiralPos <= spiralEnd)
					{
						PointF nextPoint = new(Convert.ToSingle((double)centerX + spiralPos * radiusStep * Math.Cos(spiralAngle)), Convert.ToSingle((double)centerY + spiralPos * radiusStep * Math.Sin(spiralAngle)));
						position = nextPoint;
						spiralPos += 0.1;
						spiralAngle += angleStep / 10.0;
					}
					int destWidth = (int)(_framePercentage * (float)base.ClientRectangle.Width / 100f);
					int destHeight = (int)(_framePercentage * (float)base.ClientRectangle.Height / 100f);
					Rectangle destinationRect = new((int)position.X, (int)position.Y, destWidth, destHeight);
					if (TransitBackPictureAction == BackPicturesTransition.None)
					{
						// Reusing cached ImageAttributes
						// Reusing cached ColorMatrix
						_cachedColorMatrix.Matrix33 = 0.003921569f * (255f * (1f - _framePercentage / 200f));
						_cachedImageAttributes.SetColorMatrix(_cachedColorMatrix);
						DrawImageToOutput(e, _currentBackgroundPicture, base.ClientRectangle, new Rectangle(0, 0, _currentBackgroundPicture.Width, _currentBackgroundPicture.Height), null);
						DrawImageToOutput(e, _currentTextImage, base.ClientRectangle, new Rectangle(0, 0, _currentTextImage.Width, _currentTextImage.Height), _cachedImageAttributes, PanelType.Current);
						DrawImageToOutput(e, _currentPanelImage, base.ClientRectangle, new Rectangle(0, 0, _currentTextImage.Width, _currentTextImage.Height), null, PanelType.Current);
						DrawImageToOutput(e, _newTextImage, destinationRect, new Rectangle(0, 0, _newTextImage.Width, _newTextImage.Height), null, PanelType.NewWithTrans);
					}
					else
					{
						Draw_CurrentImage(e);
						DrawImageToOutput(e, _newCombinedImage, destinationRect, new Rectangle(0, 0, _newCombinedImage.Width, _newCombinedImage.Height), null);
					}
				}
				else if (_activeTransitionType == TransitionTypes.Star)
				{
					Draw_CurrentImage(e);
					Span<PointF> starPoints = stackalloc PointF[11];
					float starSize = Convert.ToInt32((float)base.Width * _framePercentage / 130f);
					float centerX = Convert.ToInt32(base.Width / 2);
					float centerY = Convert.ToInt32(base.Height / 2);
					float angle = 0f;
					bool isInnerPoint = false;
					int pointIndex = 0;
					while ((double)angle <= Math.PI * 2.0)
					{
						float radius = starSize * 0.65f + starSize * (float)((!isInnerPoint) ? 1 : 0);
						starPoints[pointIndex] = new PointF(centerX + (float)Math.Cos((double)angle - Math.PI / 2.0) * radius, centerY + (float)Math.Sin((double)angle - Math.PI / 2.0) * radius);
						angle += (float)Math.PI / 5f;
						isInnerPoint = !isInnerPoint;
						pointIndex++;
					}
					starPoints[pointIndex] = starPoints[0];
					graphicsPath = FrameObject(new GraphicsPath());
					graphicsPath.AddPolygon(starPoints);
					e.Graphics.SetClip(graphicsPath, CombineMode.Replace);
					Draw_NewImage(e);
					graphicsPath.Dispose();
				}
				else if (_activeTransitionType == TransitionTypes.Heart)
				{
					Draw_CurrentImage(e);
					int halfSize = Convert.ToInt32((float)base.Height * _framePercentage / 100f);
					int centerX = Convert.ToInt32(base.Width / 2);
					int centerY = Convert.ToInt32(base.Height * 3 / 5);
					int lobeSize = (int)Math.Sqrt(halfSize * halfSize * 2);
					int lobeOffset = (lobeSize - halfSize) / 2;
					graphicsPath = FrameObject(new GraphicsPath());
					graphicsPath.AddEllipse(new Rectangle(centerX - lobeSize + lobeOffset, centerY - lobeSize + lobeOffset, lobeSize, lobeSize));
					e.Graphics.SetClip(graphicsPath, CombineMode.Replace);
					graphicsPath.AddEllipse(new Rectangle(centerX - lobeOffset, centerY - lobeSize + lobeOffset, lobeSize, lobeSize));
					e.Graphics.SetClip(graphicsPath, CombineMode.Replace);
					graphicsPath.AddPolygon((ReadOnlySpan<Point>)[
                    new(centerX, centerY - halfSize),
						new(centerX - halfSize, centerY),
						new(centerX, centerY + halfSize),
						new(centerX + halfSize, centerY)
					]);
					e.Graphics.SetClip(graphicsPath, CombineMode.Union);
					Draw_NewImage(e);
					graphicsPath.Dispose();
				}
				else if (_activeTransitionType == TransitionTypes.Stretch_H)
				{
					Draw_CurrentImage(e);
					if (TransitBackPictureAction == BackPicturesTransition.None)
					{
						int destX = Convert.ToInt32(((float)_newBackgroundPicture.Width - (float)_newBackgroundPicture.Width * _framePercentage / 100f) / 2f);
						int destY = 0;
						int destWidth = Convert.ToInt32((float)_newBackgroundPicture.Width * _framePercentage / 100f);
						int destHeight = _newBackgroundPicture.Height;
						Rectangle destRect = new(destX, destY, destWidth, destHeight);
						CreateFrameWorkSpace(_newBackgroundPicture.Width, _newBackgroundPicture.Height);
						wsg.DrawImage(_newBackgroundPicture, destRect, destRect, GraphicsUnit.Pixel);
						wsg.DrawImage(_newTextImage, destRect, 0, 0, _newBackgroundPicture.Width, _newBackgroundPicture.Height, GraphicsUnit.Pixel);
						DrawImageToOutput(e, _workSpace, base.ClientRectangle, new Rectangle(0, 0, _workSpace.Width, _workSpace.Height), null, PanelType.New);
						wsg.Dispose();
					}
					else
					{
						DrawImageToOutput(e, _newCombinedImage, new Rectangle(Convert.ToInt32(((float)base.Width - (float)base.Width * _framePercentage / 100f) / 2f), 0, Convert.ToInt32((float)base.Width * _framePercentage / 100f), base.ClientRectangle.Height), new Rectangle(0, 0, _currentCombinedImage.Width, _currentCombinedImage.Height), null);
					}
				}
				else if (_activeTransitionType == TransitionTypes.Stretch_V)
				{
					Draw_CurrentImage(e);
					if (TransitBackPictureAction == BackPicturesTransition.None)
					{
						int destX = 0;
						int destY = Convert.ToInt32(((float)_newBackgroundPicture.Height - (float)_newBackgroundPicture.Height * _framePercentage / 100f) / 2f);
						int destWidth = _newBackgroundPicture.Width;
						int destHeight = Convert.ToInt32((float)_newBackgroundPicture.Height * _framePercentage / 100f);
						Rectangle destRect = new(destX, destY, destWidth, destHeight);
						
						CreateFrameWorkSpace(_newBackgroundPicture.Width, _newBackgroundPicture.Height);
						wsg.DrawImage(_newBackgroundPicture, destRect, destRect, GraphicsUnit.Pixel);
						wsg.DrawImage(_newTextImage, destRect, 0, 0, _newBackgroundPicture.Width, _newBackgroundPicture.Height, GraphicsUnit.Pixel);
						DrawImageToOutput(e, _workSpace, base.ClientRectangle, new Rectangle(0, 0, _workSpace.Width, _workSpace.Height), null, PanelType.New);
						wsg.Dispose();
					}
					else
					{
						DrawImageToOutput(e, _newCombinedImage, new Rectangle(0, Convert.ToInt32(((float)base.Height - (float)base.Height * _framePercentage / 100f) / 2f), base.ClientRectangle.Width, Convert.ToInt32((float)base.Height * _framePercentage / 100f)), new Rectangle(0, 0, _currentCombinedImage.Width, _currentCombinedImage.Height), null);
					}
				}
				else if (_activeTransitionType == TransitionTypes.Away)
				{
					if (TransitBackPictureAction == BackPicturesTransition.None)
					{
						DrawImageToOutput(e, _newCombinedImage, base.ClientRectangle, new Rectangle(0, 0, _newBackgroundPicture.Width, _newBackgroundPicture.Height), null);
						float dx = 0f;
						float dy = 0f;
						float scale = 1f;
						// Reusing cached ImageAttributes
						// Reusing cached ColorMatrix
						_cachedColorMatrix.Matrix33 = 0.003921569f * (255f * (1f - _framePercentage / 100f));
						_cachedImageAttributes.SetColorMatrix(_cachedColorMatrix);
						dx = base.Width / 2;
						dy = base.Height;
						scale = 1f - 1f * _framePercentage / 100f;
						if (scale == 0f)
						{
							scale = 0.01f;
						}
						Matrix matrix = FrameObject(new Matrix(scale, 0f, 0f, scale, dx, (float)base.Height * (1f - scale)));
						e.Graphics.Transform = matrix;
						DrawImageToOutput(e, _currentTextImage, new Rectangle(-base.Width / 2, 0, base.Width, base.Height), new Rectangle(0, 0, _newCombinedImage.Width, _newCombinedImage.Height), _cachedImageAttributes, PanelType.CurrentWithTrans);
						matrix.Dispose();
					}
					else
					{
						Draw_NewImage(e);
						float scale = 1f - 1f * _framePercentage / 100f;
						if (scale == 0f)
						{
							scale = 0.01f;
						}
						float dx = base.Width / 2;
						float dy = base.Height;
						Matrix matrix = FrameObject(new Matrix(scale, 0f, 0f, scale, dx, dy * (1f - scale)));
						e.Graphics.Transform = matrix;
						if (TransitBackPictureAction == BackPicturesTransition.NewOnly)
						{
							DrawImageToOutput(e, _currentTextImage, new Rectangle(-base.Width / 2, -1 - base.Height / 20, base.Width, base.Height), new Rectangle(0, 0, _newCombinedImage.Width, _newCombinedImage.Height), null, PanelType.CurrentWithTrans);
						}
						else
						{
							DrawImageToOutput(e, _currentCombinedImage, new Rectangle(-base.Width / 2, -1 - base.Height / 20, base.Width, base.Height), new Rectangle(0, 0, _newCombinedImage.Width, _newCombinedImage.Height), null);
						}
						matrix.Dispose();
					}
				}
				else if (_activeTransitionType == TransitionTypes.Zoom_Away)
				{
					if (TransitBackPictureAction == BackPicturesTransition.None)
					{
						DrawImageToOutput(e, _newBackgroundPicture, base.ClientRectangle, new Rectangle(0, 0, _newBackgroundPicture.Width, _newBackgroundPicture.Height), null);
						float dx = 0f;
						float dy = 0f;
						// Reusing cached ImageAttributes
						// Reusing cached ColorMatrix
						_cachedColorMatrix.Matrix33 = 0.003921569f * (255f * (1f - _framePercentage / 100f));
						_cachedImageAttributes.SetColorMatrix(_cachedColorMatrix);
						dx = base.Width / 2;
						dy = base.Height;
						float scale = 1f;
						scale = 1f - 1f * _framePercentage / 100f;
						if (scale == 0f)
						{
							scale = 0.01f;
						}
						Matrix matrix = FrameObject(new Matrix(scale, 0f, 0f, scale, dx, (float)base.Height * (1f - scale)));
						e.Graphics.Transform = matrix;
						DrawImageToOutput(e, _currentTextImage, new Rectangle(-base.Width / 2, 0, base.Width, base.Height), new Rectangle(0, 0, _newCombinedImage.Width, _newCombinedImage.Height), _cachedImageAttributes, PanelType.Current);
						dx = base.Width / 2;
						dy = base.Height / 3;
						scale = 0.2f + 0.8f * _framePercentage / 100f;
						if (scale == 0f)
						{
							scale = 0.01f;
						}
						matrix = FrameObject(new Matrix(scale, 0f, 0f, scale, dx, dy));
						e.Graphics.Transform = matrix;
						DrawImageToOutput(e, _newTextImage, new Rectangle(-base.Width / 2, -1 - base.Height / 3, base.Width, base.Height), new Rectangle(0, 0, _newCombinedImage.Width, _newCombinedImage.Height), null, PanelType.New);
						matrix.Dispose();
					}
					else
					{
						DrawDefaultBackPattern(e);
						float dx = base.Width / 2;
						float dy = base.Height / 3;
						float scale = 0.2f + 0.8f * _framePercentage / 100f;
						if (scale == 0f)
						{
							scale = 0.01f;
						}
						Matrix matrix = FrameObject(new Matrix(scale, 0f, 0f, scale, dx, dy));
						e.Graphics.Transform = matrix;
						if (TransitBackPictureAction == BackPicturesTransition.CurrentOnly)
						{
							DrawImageToOutput(e, _newTextImage, new Rectangle(-base.Width / 2, -1 - base.Height / 3, base.Width, base.Height), new Rectangle(0, 0, _newCombinedImage.Width, _newCombinedImage.Height), null);
						}
						else
						{
							DrawImageToOutput(e, _newCombinedImage, new Rectangle(-base.Width / 2, -1 - base.Height / 3, base.Width, base.Height), new Rectangle(0, 0, _newCombinedImage.Width, _newCombinedImage.Height), null);
						}
						scale = 1f - 1f * _framePercentage / 100f;
						if (scale == 0f)
						{
							scale = 0.01f;
						}
						dx = base.Width / 2;
						dy = base.Height;
						matrix = FrameObject(new Matrix(scale, 0f, 0f, scale, dx, dy * (1f - scale)));
						e.Graphics.Transform = matrix;
						if (TransitBackPictureAction == BackPicturesTransition.NewOnly)
						{
							DrawImageToOutput(e, _currentTextImage, new Rectangle(-base.Width / 2, -1 - base.Height / 20, base.Width, base.Height), new Rectangle(0, 0, _newCombinedImage.Width, _newCombinedImage.Height), null, PanelType.Current);
						}
						else
						{
							DrawImageToOutput(e, _currentCombinedImage, new Rectangle(-base.Width / 2, -1 - base.Height / 20, base.Width, base.Height), new Rectangle(0, 0, _newCombinedImage.Width, _newCombinedImage.Height), null);
						}
						matrix.Dispose();
					}
				}
				else if (_activeTransitionType == TransitionTypes.CircularWipe)
				{
					Draw_CurrentImage(e);
					float angle = 360f * _framePercentage / 100f;
					float diameter = (float)base.Width * 1.414f;
					graphicsPath = FrameObject(new GraphicsPath());
					graphicsPath.AddPie(((float)base.Width - diameter) / 2f, ((float)base.Height - diameter) / 2f, diameter, diameter, 180f, angle);
					e.Graphics.SetClip(graphicsPath, CombineMode.Replace);
					Draw_NewImage(e);
					graphicsPath.Dispose();
				}
				else if (_activeTransitionType == TransitionTypes.Wedge)
				{
					Draw_CurrentImage(e);
					float angle = 360f * _framePercentage / 100f;
					float diameter = (float)base.Width * 1.414f;
					graphicsPath = FrameObject(new GraphicsPath());
					graphicsPath.AddPie(((float)base.Width - diameter) / 2f, ((float)base.Height - diameter) / 2f, diameter, diameter, 270f - angle / 2f, angle);
					e.Graphics.SetClip(graphicsPath, CombineMode.Replace);
					Draw_NewImage(e);
					graphicsPath.Dispose();
				}
				else if (_activeTransitionType == TransitionTypes.BowTie)
				{
					Draw_CurrentImage(e);
					float angle = 180f * _framePercentage / 100f;
					float diameter = (float)base.Width * 1.414f;
					graphicsPath = FrameObject(new GraphicsPath());
					graphicsPath.AddPie(((float)base.Width - diameter) / 2f, ((float)base.Height - diameter) / 2f, diameter, diameter, 270f - angle / 2f, angle);
					graphicsPath.AddPie(((float)base.Width - diameter) / 2f, ((float)base.Height - diameter) / 2f, diameter, diameter, 90f - angle / 2f, angle);
					e.Graphics.SetClip(graphicsPath, CombineMode.Replace);
					Draw_NewImage(e);
					graphicsPath.Dispose();
				}
				else if (_activeTransitionType == TransitionTypes.FanUp)
				{
					Draw_CurrentImage(e);
					float angle = 150f * _framePercentage / 100f;
					float diameter = (float)base.Width * 1.7f * 1.414f;
					graphicsPath = FrameObject(new GraphicsPath());
					graphicsPath.AddPie(((float)base.Width - diameter) / 2f, (float)base.Height - diameter * 0.9f, diameter, diameter, 90f - angle / 2f, angle);
					e.Graphics.SetClip(graphicsPath, CombineMode.Replace);
					Draw_NewImage(e);
					graphicsPath.Dispose();
				}
				else if (_activeTransitionType == TransitionTypes.WindMill)
				{
					Draw_CurrentImage(e);
					float angle = 90f * _framePercentage / 100f;
					float diameter = (float)base.Width * 1.414f;
					graphicsPath = FrameObject(new GraphicsPath());
					graphicsPath.AddPie(((float)base.Width - diameter) / 2f, ((float)base.Height - diameter) / 2f, diameter, diameter, 270f, angle);
					graphicsPath.AddPie(((float)base.Width - diameter) / 2f, ((float)base.Height - diameter) / 2f, diameter, diameter, 0f, angle);
					graphicsPath.AddPie(((float)base.Width - diameter) / 2f, ((float)base.Height - diameter) / 2f, diameter, diameter, 90f, angle);
					graphicsPath.AddPie(((float)base.Width - diameter) / 2f, ((float)base.Height - diameter) / 2f, diameter, diameter, 180f, angle);
					e.Graphics.SetClip(graphicsPath, CombineMode.Replace);
					Draw_NewImage(e);
					graphicsPath.Dispose();
				}
				else if (_activeTransitionType == TransitionTypes.RandomBars)
				{
					Draw_CurrentImage(e);
					graphicsPath = FrameObject(new GraphicsPath());
					// Using static _random field instead
					int barIndex = 0;
					int barCount = base.Height / 2 + 1;
					int barsToAdd = (int)((float)barCount * _framePercentage / 100f) - TotalBitsArraySet;
					// Bars 0..barCount-1 are drawn; the walk wrapped only past Height, adding bars that are never drawn,
					// and one bar fewer than due was added per frame (the reveal lagged and snapped at 100 %).
					for (int k = 0; k < barsToAdd; k++)
					{
						barIndex = _random.Next(barCount);
						// At most one full lap: ends even if the set already holds every bar (e.g. indices left
						// from another transition type after a failed draw).
						int steps = 0;
						while (BitsArraySet.Contains(barIndex) && steps++ < barCount)
						{
							barIndex++;
							if (barIndex >= barCount)
							{
								barIndex = 0;
							}
						}
						if (steps > barCount)
						{
							break;
						}
						BitsArraySet.Add(barIndex);
					}
					TotalBitsArraySet = 0;
					int barWidth = base.Width;
					for (int k = 0; k <= barCount; k++)
					{
						if (BitsArraySet.Contains(k))
						{
							TotalBitsArraySet++;
							graphicsPath.AddRectangle(new Rectangle(0, k * 2, barWidth, 2));
						}
					}
					e.Graphics.SetClip(graphicsPath, CombineMode.Replace);
					Draw_NewImage(e);
					graphicsPath.Dispose();
				}
				else if (_activeTransitionType == TransitionTypes.Dissolve)
				{
					Draw_CurrentImage(e);
					graphicsPath = FrameObject(new GraphicsPath());
					// Using static _random field instead
					int cellIndex = 0;
					int cellsToAdd = (int)((float)DissolveTotal * _framePercentage / 100f) - TotalBitsArraySet;
					for (int k = 0; k < cellsToAdd; k++)
					{
						cellIndex = _random.Next(DissolveTotal);
						BitsArraySet.Add(cellIndex);
					}
					TotalBitsArraySet = 0;
					int cellRow = 0;
					int cellColumn = 0;
					for (int k = 0; k < DissolveTotal; k++)
					{
						if (BitsArraySet.Contains(k))
						{
							cellRow = k / DissolveCountX;
							cellColumn = k - cellRow * DissolveCountX; // column 0..DissolveCountX-1 (was shifted one cell left)
							TotalBitsArraySet++;
							graphicsPath.AddRectangle(new Rectangle(cellColumn * DissolveSize, cellRow * DissolveSize, DissolveSize, DissolveSize));
						}
					}
					e.Graphics.SetClip(graphicsPath, CombineMode.Replace);
					Draw_NewImage(e);
					graphicsPath.Dispose();
				}
				else if (_activeTransitionType == TransitionTypes.Mosaic)
				{
					Draw_CurrentImage(e);
					graphicsPath = FrameObject(new GraphicsPath());
					// Using static _random field instead
					int cellIndex = 0;
					int cellsToAdd = (int)((float)DissolveTotal * _framePercentage / 100f) - TotalBitsArraySet;
					for (int k = 0; k < cellsToAdd; k++)
					{
						cellIndex = _random.Next(DissolveTotal);
						// At most one full lap (see RandomBars).
						int steps = 0;
						while (BitsArraySet.Contains(cellIndex) && steps++ < DissolveTotal)
						{
							cellIndex++;
							if (cellIndex >= DissolveTotal)
							{
								cellIndex = 0;
							}
						}
						if (steps > DissolveTotal)
						{
							break;
						}
						BitsArraySet.Add(cellIndex);
					}
					TotalBitsArraySet = 0;
					int cellRow = 0;
					int cellColumn = 0;
					for (int k = 0; k < DissolveTotal; k++)
					{
						if (BitsArraySet.Contains(k))
						{
							cellRow = k / DissolveCountX;
							cellColumn = k - cellRow * DissolveCountX; // column 0..DissolveCountX-1 (was shifted one cell left)
							TotalBitsArraySet++;
							graphicsPath.AddRectangle(new Rectangle(cellColumn * DissolveSize, cellRow * DissolveSize, DissolveSize, DissolveSize));
						}
					}
					e.Graphics.SetClip(graphicsPath, CombineMode.Replace);
					Draw_NewImage(e);
					graphicsPath.Dispose();
				}
				else if (_activeTransitionType == TransitionTypes.Zoom_In)
				{
					Draw_CurrentImage(e);
					float scale = 0.2f + 0.8f * _framePercentage / 100f;
					if (scale == 0f)
					{
						scale = 0.01f;
					}
					if (TransitBackPictureAction == BackPicturesTransition.None)
					{
						scale = (_framePercentage + 20f) / 100f;
						if (scale == 0f)
						{
							scale = 0.01f;
						}
						else if (scale > 1f)
						{
							scale = 1f;
						}
						float dx = _newBackgroundPicture.Width / 2;
						float dy = _newBackgroundPicture.Height / 2;
						
						CreateFrameWorkSpace(_newBackgroundPicture.Width, _newBackgroundPicture.Height);
						Rectangle destRect = new(-_newBackgroundPicture.Width / 2, -1 - _newBackgroundPicture.Height / 2, _newBackgroundPicture.Width, _newBackgroundPicture.Height);
						Rectangle rectangle = new(destRect.Width / 2 - (int)(scale * (float)destRect.Width / 2f), destRect.Height / 2 - (int)(scale * (float)destRect.Height / 2f), (int)(scale * (float)destRect.Width), (int)(scale * (float)destRect.Height));
						wsg.DrawImage(_newBackgroundPicture, rectangle, rectangle, GraphicsUnit.Pixel);
						Matrix matrix = FrameObject(new Matrix(scale, 0f, 0f, scale, dx, dy));
						wsg.Transform = matrix;
						wsg.DrawImage(_newTextImage, destRect, 0, 0, _newBackgroundPicture.Width, _newBackgroundPicture.Height, GraphicsUnit.Pixel);
						DrawImageToOutput(e, _workSpace, base.ClientRectangle, new Rectangle(0, 0, _workSpace.Width, _workSpace.Height), null, PanelType.New);
						matrix.Dispose();
						wsg.Dispose();
					}
					else
					{
						float dx = base.Width / 2;
						float dy = base.Height / 2;
						Matrix matrix = FrameObject(new Matrix(scale, 0f, 0f, scale, dx, dy));
						e.Graphics.Transform = matrix;
						DrawImageToOutput(e, _newCombinedImage, new Rectangle(-base.Width / 2, -1 - base.Height / 2, base.Width, base.Height), new Rectangle(0, 0, _newCombinedImage.Width, _newCombinedImage.Height), null);
						matrix.Dispose();
					}
				}
				else if (_activeTransitionType == TransitionTypes.Zoom_Out)
				{
					float scale = 2f - _framePercentage / 100f;
					if (scale == 0f)
					{
						scale = 0.01f;
					}
					if (TransitBackPictureAction == BackPicturesTransition.None)
					{
						DrawImageToOutput(e, _newBackgroundPicture, base.ClientRectangle, new Rectangle(0, 0, _newBackgroundPicture.Width, _newBackgroundPicture.Height), null);
						float dx = base.Width / 2;
						float dy = base.Height / 2;
						Matrix matrix = FrameObject(new Matrix(scale, 0f, 0f, scale, dx, dy * scale));
						e.Graphics.Transform = matrix;
						DrawImageToOutput(e, _newTextImage, new Rectangle(-base.Width / 2, -1 - base.Height / 2, base.Width, base.Height), new Rectangle(0, 0, _newCombinedImage.Width, _newCombinedImage.Height), null, PanelType.New);
						matrix.Dispose();
					}
					else
					{
						float dx = base.Width / 2;
						float dy = base.Height / 2;
						Matrix matrix = FrameObject(new Matrix(scale, 0f, 0f, scale, dx, dy * scale));
						e.Graphics.Transform = matrix;
						DrawImageToOutput(e, _newCombinedImage, new Rectangle(-base.Width / 2, -1 - base.Height / 2, base.Width, base.Height), new Rectangle(0, 0, _newCombinedImage.Width, _newCombinedImage.Height), null);
						matrix.Dispose();
					}
				}
				if (_alert.OverallPercentage < 100f)
				{
					AddAlertTextToImage(e);
				}
				if (_reference.OverallPercentage < 100f)
				{
					AddRefTextToImage(e);
				}
			}
			else
			{
				t?.Dispose();
				t = null;
				if (ItemFirstShowing)
				{
					RefGo();
					ItemFirstShowing = false;
				}
				// The transition is over: give back the work-space bitmap (picture size, ~8 MB at 1080p, ~33 MB at
				// 4K) until the next one needs it. Its Graphics belonged to an earlier frame and is already disposed.
				_workSpace?.Dispose();
				_workSpace = null;
				if (_alert.Running || _reference.Running)
				{
					_alert.EndIfComplete();
					_reference.EndIfComplete();
				}
				ReLoadNewImage(e);
				AddAlertTextToImage(e);
				AddRefTextToImage(e);
			}
		}

		private void ReLoadNewImage(PaintEventArgs e)
		{
			DrawNewImageOpaque(e);
		}

		/// <summary>
		/// Draws the new combined image over the whole screen, replacing what was there: on black, as the
		/// memory-DC copy this replaces did (its bitmap started black and was copied over the screen), so a
		/// picture with transparent parts or scaled edges comes out the same. Straight into the paint's
		/// Graphics: no screen-size bitmap or memory DC per paint.
		/// </summary>
		private void DrawNewImageOpaque(PaintEventArgs e)
		{
			Rectangle client = base.ClientRectangle;
			e.Graphics.FillRectangle(Brushes.Black, client);
			e.Graphics.DrawImage(_newCombinedImage, client, new Rectangle(0, 0, _newCombinedImage.Width, _newCombinedImage.Height), GraphicsUnit.Pixel);
		}

		private void Draw_CurrentImage(PaintEventArgs e)
		{
			if (_framePercentage < 100f)
			{
				DrawImageToOutput(e, _currentCombinedImage, base.ClientRectangle, new Rectangle(0, 0, _currentCombinedImage.Width, _currentCombinedImage.Height), null);
			}
			AddAlertTextToImage(e);
			AddRefTextToImage(e);
		}

		private void Draw_NewImage(PaintEventArgs e)
		{
			DrawImageToOutput(e, _newCombinedImage, base.ClientRectangle, new Rectangle(0, 0, _newCombinedImage.Width, _newCombinedImage.Height), null);
		}

		/// <summary>
		/// Draws (part of) a picture into this paint's Graphics, with the current or new panel layer over it
		/// when <paramref name="InPanelType"/> says so (the panel scaled like the picture, or taken from the same
		/// source rectangle for the ...WithTrans types), and with <paramref name="InImageAttributes"/> (fades)
		/// on both when given.
		/// </summary>
		private void DrawImageToOutput(PaintEventArgs e, Image InImage, Rectangle DestinationRectangle, Rectangle SourceRectangle, ImageAttributes InImageAttributes, PanelType InPanelType = PanelType.NoAction)
		{
			if (InImageAttributes == null)
			{
				e.Graphics.DrawImage(InImage, DestinationRectangle, SourceRectangle, GraphicsUnit.Pixel);
				switch (InPanelType)
				{
				case PanelType.Current:
					e.Graphics.DrawImage(CurrentPanelImage, DestinationRectangle, new Rectangle(0, 0, SourceRectangle.Width, SourceRectangle.Height), GraphicsUnit.Pixel);
					break;
				case PanelType.CurrentWithTrans:
					e.Graphics.DrawImage(CurrentPanelImage, DestinationRectangle, SourceRectangle, GraphicsUnit.Pixel);
					break;
				case PanelType.New:
					e.Graphics.DrawImage(NewPanelImage, DestinationRectangle, new Rectangle(0, 0, SourceRectangle.Width, SourceRectangle.Height), GraphicsUnit.Pixel);
					break;
				case PanelType.NewWithTrans:
					e.Graphics.DrawImage(NewPanelImage, DestinationRectangle, SourceRectangle, GraphicsUnit.Pixel);
					break;
				}
			}
			else
			{
				e.Graphics.DrawImage(InImage, DestinationRectangle, SourceRectangle.Left, SourceRectangle.Top, SourceRectangle.Width, SourceRectangle.Height, GraphicsUnit.Pixel, InImageAttributes);
				switch (InPanelType)
				{
				case PanelType.Current:
					e.Graphics.DrawImage(CurrentPanelImage, DestinationRectangle, 0, 0, SourceRectangle.Width, SourceRectangle.Height, GraphicsUnit.Pixel, InImageAttributes);
					break;
				case PanelType.CurrentWithTrans:
					e.Graphics.DrawImage(CurrentPanelImage, DestinationRectangle, SourceRectangle.Left, SourceRectangle.Top, SourceRectangle.Width, SourceRectangle.Height, GraphicsUnit.Pixel, InImageAttributes);
					break;
				case PanelType.New:
					e.Graphics.DrawImage(NewPanelImage, DestinationRectangle, 0, 0, SourceRectangle.Width, SourceRectangle.Height, GraphicsUnit.Pixel, InImageAttributes);
					break;
				case PanelType.NewWithTrans:
					e.Graphics.DrawImage(NewPanelImage, DestinationRectangle, SourceRectangle.Left, SourceRectangle.Top, SourceRectangle.Width, SourceRectangle.Height, GraphicsUnit.Pixel, InImageAttributes);
					break;
				}
			}
		}

		private void Draw_TranSlideIn(PaintEventArgs e, TransitionTypes InTransType)
		{
			float offsetY = 0f;
			float offsetX = 0f;
			// Replaced by every case below; only an unknown type would keep it (its 1 px shift down is as it was).
			Matrix transform = FrameObject(new Matrix(1f, 0f, 0f, 1f, 0f, 1f));
			switch (InTransType)
			{
			case TransitionTypes.InLeft:
				offsetX = (float)_newBackgroundPicture.Width * _framePercentage / 100f - (float)_newBackgroundPicture.Width;
				transform = FrameObject(new Matrix(1f, 0f, 0f, 1f, offsetX, 0f));
				break;
			case TransitionTypes.InTop:
				offsetY = (float)_newBackgroundPicture.Height * _framePercentage / 100f - (float)_newBackgroundPicture.Height;
				transform = FrameObject(new Matrix(1f, 0f, 0f, 1f, 0f, offsetY));
				break;
			case TransitionTypes.InRight:
				offsetX = (float)_newBackgroundPicture.Width - (float)_newBackgroundPicture.Width * _framePercentage / 100f;
				transform = FrameObject(new Matrix(1f, 0f, 0f, 1f, offsetX, 0f));
				break;
			case TransitionTypes.InBottom:
				offsetY = (float)_newBackgroundPicture.Height - (float)_newBackgroundPicture.Height * _framePercentage / 100f;
				transform = FrameObject(new Matrix(1f, 0f, 0f, 1f, 0f, offsetY));
				break;
			case TransitionTypes.InTopLeft:
				offsetY = (float)_newBackgroundPicture.Height * _framePercentage / 100f - (float)_newBackgroundPicture.Height;
				offsetX = (float)_newBackgroundPicture.Width * _framePercentage / 100f - (float)_newBackgroundPicture.Width;
				transform = FrameObject(new Matrix(1f, 0f, 0f, 1f, offsetX, offsetY));
				break;
			case TransitionTypes.InTopRight:
				offsetY = (float)_newBackgroundPicture.Height * _framePercentage / 100f - (float)_newBackgroundPicture.Height;
				offsetX = (float)_newBackgroundPicture.Width - (float)_newBackgroundPicture.Width * _framePercentage / 100f;
				transform = FrameObject(new Matrix(1f, 0f, 0f, 1f, offsetX, offsetY));
				break;
			case TransitionTypes.InBottomLeft:
				offsetY = (float)_newBackgroundPicture.Height - (float)_newBackgroundPicture.Height * _framePercentage / 100f;
				offsetX = (float)_newBackgroundPicture.Width * _framePercentage / 100f - (float)_newBackgroundPicture.Width;
				transform = FrameObject(new Matrix(1f, 0f, 0f, 1f, offsetX, offsetY));
				break;
			case TransitionTypes.InBottomRight:
				offsetY = (float)_newBackgroundPicture.Height - (float)_newBackgroundPicture.Height * _framePercentage / 100f;
				offsetX = (float)_newBackgroundPicture.Width - (float)_newBackgroundPicture.Width * _framePercentage / 100f;
				transform = FrameObject(new Matrix(1f, 0f, 0f, 1f, offsetX, offsetY));
				break;
			}
			if (TransitBackPictureAction == BackPicturesTransition.None)
			{
				int x = (offsetX >= 0f) ? ((int)offsetX) : 0;
				int y = (offsetY >= 0f) ? ((int)offsetY) : 0;
				int width = (offsetX >= 0f) ? (_newBackgroundPicture.Width - (int)offsetX) : (_newBackgroundPicture.Width + (int)offsetX);
				int height = (offsetY >= 0f) ? (_newBackgroundPicture.Height - (int)offsetY) : (_newBackgroundPicture.Height + (int)offsetY);
				Rectangle rectangle = new(x, y, width, height);
				// Only this branch uses the work space (it was made, and cleared, in the other one too).
				CreateFrameWorkSpace(_newBackgroundPicture.Width, _newBackgroundPicture.Height);
				wsg.DrawImage(_currentCombinedImage, new Rectangle(0, 0, _newBackgroundPicture.Width, _newBackgroundPicture.Height), 0, 0, _newBackgroundPicture.Width, _newBackgroundPicture.Height, GraphicsUnit.Pixel);
				wsg.DrawImage(_newBackgroundPicture, rectangle, rectangle, GraphicsUnit.Pixel);
				wsg.DrawImage(_newTextImage, new Rectangle((int)offsetX, (int)offsetY, _newBackgroundPicture.Width, _newBackgroundPicture.Height), 0, 0, _newBackgroundPicture.Width, _newBackgroundPicture.Height, GraphicsUnit.Pixel);
				DrawImageToOutput(e, _workSpace, base.ClientRectangle, new Rectangle(0, 0, _workSpace.Width, _workSpace.Height), null, PanelType.New);
				wsg.Dispose();
			}
			else
			{
				Draw_CurrentImage(e);
				e.Graphics.Transform = transform;
				Draw_NewImage(e);
			}
		}

		private void Draw_TranSlideOut(PaintEventArgs e, TransitionTypes InTransType)
		{
			float offsetY = 0f;
			float offsetX = 0f;
			// Replaced by every case below; only an unknown type would keep it (its 1 px shift down is as it was).
			Matrix transform = FrameObject(new Matrix(1f, 0f, 0f, 1f, 0f, 1f));
			switch (InTransType)
			{
			case TransitionTypes.OutLeft:
				offsetX = (float)(-_newBackgroundPicture.Width) * _framePercentage / 100f;
				transform = FrameObject(new Matrix(1f, 0f, 0f, 1f, offsetX, 0f));
				break;
			case TransitionTypes.OutTop:
				offsetY = (float)(-_newBackgroundPicture.Height) * _framePercentage / 100f;
				transform = FrameObject(new Matrix(1f, 0f, 0f, 1f, 0f, offsetY));
				break;
			case TransitionTypes.OutRight:
				offsetX = (float)_newBackgroundPicture.Width * _framePercentage / 100f;
				transform = FrameObject(new Matrix(1f, 0f, 0f, 1f, offsetX, 0f));
				break;
			case TransitionTypes.OutBottom:
				offsetY = (float)_newBackgroundPicture.Height * _framePercentage / 100f;
				transform = FrameObject(new Matrix(1f, 0f, 0f, 1f, 0f, offsetY));
				break;
			case TransitionTypes.OutTopLeft:
				offsetY = (float)(-_newBackgroundPicture.Height) * _framePercentage / 100f;
				offsetX = (float)(-_newBackgroundPicture.Width) * _framePercentage / 100f;
				transform = FrameObject(new Matrix(1f, 0f, 0f, 1f, offsetX, offsetY));
				break;
			case TransitionTypes.OutTopRight:
				offsetY = (float)(-_newBackgroundPicture.Height) * _framePercentage / 100f;
				offsetX = (float)_newBackgroundPicture.Width * _framePercentage / 100f;
				transform = FrameObject(new Matrix(1f, 0f, 0f, 1f, offsetX, offsetY));
				break;
			case TransitionTypes.OutBottomLeft:
				offsetY = (float)_newBackgroundPicture.Height * _framePercentage / 100f;
				offsetX = (float)(-_newBackgroundPicture.Width) * _framePercentage / 100f;
				transform = FrameObject(new Matrix(1f, 0f, 0f, 1f, offsetX, offsetY));
				break;
			case TransitionTypes.OutBottomRight:
				offsetY = (float)_newBackgroundPicture.Height * _framePercentage / 100f;
				offsetX = (float)_newBackgroundPicture.Width * _framePercentage / 100f;
				transform = FrameObject(new Matrix(1f, 0f, 0f, 1f, offsetX, offsetY));
				break;
			}
			if (TransitBackPictureAction == BackPicturesTransition.None)
			{
				int x = (offsetX >= 0f) ? ((int)offsetX) : 0;
				int y = (offsetY >= 0f) ? ((int)offsetY) : 0;
				int width = (offsetX >= 0f) ? (_newBackgroundPicture.Width - (int)offsetX) : (_newBackgroundPicture.Width + (int)offsetX);
				int height = (offsetY >= 0f) ? (_newBackgroundPicture.Height - (int)offsetY) : (_newBackgroundPicture.Height + (int)offsetY);
				Rectangle rectangle = new(x, y, width, height);
				// Only this branch uses the work space (it was made, and cleared, in the other one too).
				CreateFrameWorkSpace(_newBackgroundPicture.Width, _newBackgroundPicture.Height);
				wsg.DrawImage(NewCombinedImage, new Rectangle(0, 0, _newBackgroundPicture.Width, _newBackgroundPicture.Height), 0, 0, _newBackgroundPicture.Width, _newBackgroundPicture.Height, GraphicsUnit.Pixel);
				wsg.DrawImage(_newBackgroundPicture, rectangle, rectangle, GraphicsUnit.Pixel);
				wsg.DrawImage(_currentTextImage, new Rectangle((int)offsetX, (int)offsetY, _newBackgroundPicture.Width, _newBackgroundPicture.Height), 0, 0, _newBackgroundPicture.Width, _newBackgroundPicture.Height, GraphicsUnit.Pixel);
				DrawImageToOutput(e, _workSpace, base.ClientRectangle, new Rectangle(0, 0, _workSpace.Width, _workSpace.Height), null, PanelType.Current);
				wsg.Dispose();
			}
			else
			{
				Draw_NewImage(e);
				e.Graphics.Transform = transform;
				Draw_CurrentImage(e);
			}
		}

		private void Draw_TranSlideReveal(PaintEventArgs e, TransitionTypes InTransType)
		{
			CreateFrameWorkSpace(NewCombinedImage.Width, NewCombinedImage.Height);
			wsg.DrawImage(NewCombinedImage, new Rectangle(0, 0, NewCombinedImage.Width, NewCombinedImage.Height), 0, 0, NewCombinedImage.Width, NewCombinedImage.Height, GraphicsUnit.Pixel);
			int visibleX = 0;
			int visibleY = 0;
			int width = NewCombinedImage.Width;
			int height = NewCombinedImage.Height;
			float progress = 1f * _framePercentage / 100f;
			switch (InTransType)
			{
			case TransitionTypes.RevealLeftRight:
				visibleX = (int)((float)_currentCombinedImage.Width * progress);
				width = _currentCombinedImage.Width - visibleX;
				break;
			case TransitionTypes.RevealTopDown:
				visibleY = (int)((float)_currentCombinedImage.Height * progress);
				height = _currentCombinedImage.Height - visibleY;
				break;
			case TransitionTypes.RevealRightLeft:
				width = (int)((float)_currentCombinedImage.Width * (1f - progress));
				break;
			case TransitionTypes.RevealDownUp:
				height = (int)((float)_currentCombinedImage.Height * (1f - progress));
				break;
			}
			Rectangle rectangle = new(visibleX, visibleY, width, height);
			wsg.DrawImage(_currentCombinedImage, rectangle, rectangle, GraphicsUnit.Pixel);
			DrawImageToOutput(e, _workSpace, base.ClientRectangle, new Rectangle(0, 0, _workSpace.Width, _workSpace.Height), null);
			wsg.Dispose();
		}

		public void StartAlert(SongSettings InItem, string InString, int InDuration, Font InFont, bool InUseScroll, bool InUseFlash, bool InTransparent, bool InUseShadow, bool InUseOutline, Color InTextColour, Color InBackColour, int InAlignment, int InVerticalAlignment, double BottomBorderFactor)
		{
			if (_newBackgroundPicture != null)
			{
				// Cheap checks for setups that cannot succeed (zero-width message bitmap, font size <= 0).
				if (string.IsNullOrWhiteSpace(InString) || InFont == null || InFont.Size <= 1f)
				{
					AbortAlert("StartAlert skipped: empty message or font too small");
					return;
				}
				// The alert bitmaps are drawn by OnPaint while the alert runs, so they must stay alive
				// until the next alert replaces them (or the control is disposed). Release the previous
				// ones first; paint runs on this (UI) thread, so it cannot observe the gap.
				_alert.ReleaseImages();
				try
				{
					_alert.Build(InItem, InString, InDuration, InFont, InUseScroll, InUseFlash, InTransparent, InUseShadow, InUseOutline, InTextColour, InBackColour, InAlignment, InVerticalAlignment);
					_alert.Start();
					Invalidate();
				}
				catch (Exception ex)
				{
					// Never let a failed alert reach the caller (FrmMain.RemoteControlLiveShow would replace the show window).
					AbortAlert("StartAlert failed: " + ex);
				}
			}
		}

		private void AddAlertTextToImage(PaintEventArgs e)
		{
			_alert.Paint(e);
		}

		public void StopAlert()
		{
			_alert.Expire();
		}

		// Leaves no alert running: releases the alert images and Graphics, stops the alert timer.
		private void AbortAlert(string reason)
		{
			System.Diagnostics.Trace.WriteLine("ImageTransitionControl." + reason);
			_alert.ReleaseImages();
			_alert.Stop();
			Invalidate();
		}

		public void LoadRef(SongSettings InItem, string InString, int InDuration, Font InFont, bool InUseScroll, bool InUseFlash, bool InTransparent, bool InUseShadow, bool InUseOutline, Color InTextColour, Color InBackColour, int InAlignment, int InVerticalAlignment, double BottomBorderFactor)
		{
			// The reference bitmaps are drawn by OnPaint while the reference runs, so they stay alive
			// until the next LoadRef replaces them (or the control is disposed).
			_reference.ReleaseImages();
			if (string.IsNullOrEmpty(InString))
			{
				// No reference at all: prepare nothing, stop a running reference, and keep RefDisplayString
				// empty so RefGo and paint draw nothing. Whitespace is not "no reference": the operator's toggle
				// passes " " on purpose (active indicator), which is set up normally and shows an empty box.
				RefDisplayString = "";
				StopRef();
				return;
			}
			RefDisplayString = InString;
			try
			{
				// Sized from the picture like everything here (paint scales the result to the screen): sizing
				// from the control's client height made the reference about 3.6 times smaller on the operator's
				// small Output/Preview screens than on the projector.
				_reference.Build(InItem, InString, InDuration, InFont, InUseScroll, InUseFlash, InTransparent, InUseShadow, InUseOutline, InTextColour, InBackColour, InAlignment, InVerticalAlignment);
			}
			catch (Exception ex)
			{
				System.Diagnostics.Trace.WriteLine("ImageTransitionControl.LoadRef failed: " + ex);
				// Leave no partially built (or disposed) bitmap reachable from paint, and nothing for RefGo to start.
				_reference.ReleaseImages();
				RefDisplayString = "";
				StopRef();
			}
		}

		private void RefGo()
		{
			if (RefDisplayString != "")
			{
				_reference.Start();
			}
		}

		private void AddRefTextToImage(PaintEventArgs e)
		{
			_reference.Paint(e);
		}

		public void StopRef()
		{
			// The timer is stopped here too: the 100 % paint stops it only while the reference runs, so a
			// stopped reference kept repainting the whole screen every 150 ms until the next one started.
			_reference.Stop();
			// The reference box is erased by the next paint (that was the leaked timer's tick before).
			if (IsHandleCreated && !IsDisposed)
			{
				Invalidate();
			}
		}

		public bool RefStatus()
		{
			return _reference.Running;
		}
	
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				// Stop and release the transition timer.
				t?.Dispose();
				// The alert and the reference: their timers, setup Graphics and bitmaps.
				_alert?.Dispose();
				_reference?.Dispose();

				// Graphics objects.
				wsg?.Dispose();
				foreach (var buffer in _compositeBuffers)
					buffer?.Dispose();

				// Dispose images adopted by this control. Borrowed picture fields have other owners.
				foreach (Image owned in _ownImages)
				{
					owned.Dispose();
				}
				_ownImages.Clear();
				// This screen's own layer buffers; their Graphics first, before the bitmaps they draw on.
				for (int i = 0; i < 2; i++)
				{
					_textDrawGraphics[i]?.Dispose();
					_panelDrawGraphics[i]?.Dispose();
					_textDrawBuffers[i]?.Dispose();
					_panelDrawBuffers[i]?.Dispose();
				}
				_workSpace?.Dispose();
				_workSpace = null;

				// Cached objects cleanup
				_cachedImageAttributes?.Dispose();
				// ColorMatrix does not have Dispose method
				_cachedColorMatrix = null;
			}

			base.Dispose(disposing);
		}
	}
