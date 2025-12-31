using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;
using Easislides.Module;
using Easislides.Util;

namespace Easislides
{
    internal unsafe class ImageTransitionControl : Control
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

		private const int MaxBits = 150000;

		private System.Threading.Timer t;

		private Graphics wsg;

		private string[] _transitionItem = new string[58];

		private bool ItemFirstShowing = false;

		// Improved BitsArray: Track only changed items using HashSet for O(1) lookup
		private HashSet<int> BitsArraySet = new HashSet<int>();
		private readonly object _bitsArrayLock = new object();

		private int TotalBitsArraySet = 0;

		private int DissolveSize = 0;

		private int DissolveCountX = 0;

		private int DissolveCountY = 0;

		private int DissolveTotal = 0;

		private Graphics Newg;

		private Image Newbmp;

		private TransitionTypes _itemTransitionType = TransitionTypes.None;

		private TransitionTypes _slideTransitionType = TransitionTypes.None;

		private TransitionTypes _transitionType = TransitionTypes.None;

		private TransitionTypes _previousTransitionType = TransitionTypes.None;

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

		private Image _imageWorkSpace;

		// Bitmap pool for reuse to reduce GC pressure
		private readonly ConcurrentBag<Bitmap> _bitmapPool = new ConcurrentBag<Bitmap>();
		private readonly int _maxPoolSize = 10;
		private readonly object _poolLock = new object();

		private BackPicturesTransition _transitBackPictureAction = BackPicturesTransition.BothBackgrounds;

		private bool _itemChanged = true;

		private string _backgroundID = "";

		private TimeSpan _transitionTime = new TimeSpan(0, 0, 0, 1, 0);

		private float _currentPercentage = 0f;

		private DateTime _startTime;

		private Image AlertimageBackground;

		private Image AlertimageMessage;

		private Image AlertimageBackground_Inverse;

		private Image AlertimageMessage_Inverse;

		private Graphics alertg;

		private Graphics alertMessageg;

		private Graphics alertg_Inverse;

		private Graphics alertMessageg_Inverse;

		private int AlertMessageOriginX = 0;

		private int AlertMessageOriginY = 0;

		private int AlertOccurences = 0;

		private int AlertGapDuration = 7;

		private float AlertCharPerSecond = 6f;

		private bool AlertGapRunning = false;

		private int AlertFlashCount = 0;

		private int AlertFlashCountMax = 20;

		private bool AlertFlashAtStart = false;

		private int AlertBorder = 20;

		private Rectangle AlertOverallBackGroundRect = default(Rectangle);

		private Rectangle AlertMessageBackGroundRect = default(Rectangle);

		private int AlertOriginY = 0;

		private bool AlertScroll = false;

		private bool AlertFlash = false;

		private bool AlertTransparent = true;

		private DateTime AlertStartTime;

		private DateTime AlertMessageStartTime;

		private DateTime AlertGapStartTime;

		private float AlertOverallCurrentPercentage = 100f;

		private float AlertMessagePercentage = 100f;

		private bool AlertRunning = false;

		private int AlertAlign = 1;

		private System.Threading.Timer ta;

		private TimeSpan _alertTransitionTime = new TimeSpan(0, 0, 0, 1, 0);

		private TimeSpan _alertMessageTransitionTime = new TimeSpan(0, 0, 0, 1, 0);

		private Image RefimageBackground;

		private Image RefimageMessage;

		private Image RefimageBackground_Inverse;

		private Image RefimageMessage_Inverse;

		private Graphics refg;

		private Graphics refMessageg;

		private Graphics refg_Inverse;

		private Graphics refMessageg_Inverse;

		private int RefMessageOriginX = 0;

		private int RefMessageOriginY = 0;

		private int RefOccurences = 0;

		private int RefGapDuration = 7;

		private float RefCharPerSecond = 8f;

		private bool RefGapRunning = false;

		private int RefFlashCount = 0;

		private int RefFlashCountMax = 28;

		private bool RefFlashAtStart = false;

		private int RefBorder = 25;

		private Rectangle RefOverallBackGroundRect = default(Rectangle);

		private Rectangle RefMessageBackGroundRect = default(Rectangle);

		private Rectangle ClientBackgroundReducedRect = default(Rectangle);

		private Rectangle ClientMessageReducedRect = default(Rectangle);

		private int RefOriginY = 0;

		private bool RefScroll = false;

		private bool RefFlash = false;

		private bool RefTransparent = true;

		private DateTime RefStartTime;

		private DateTime RefMessageStartTime;

		private DateTime RefGapStartTime;

		private float RefOverallCurrentPercentage = 100f;

		private float RefMessagePercentage = 100f;

		private bool RefRunning = false;

		private int RefAlign = 1;

		private System.Threading.Timer tr;

		private TimeSpan _refTransitionTime = new TimeSpan(0, 0, 0, 1, 0);

		public string RefDisplayString = "";

		private TimeSpan _refMessageTransitionTime = new TimeSpan(0, 0, 0, 1, 0);

		private delegate* unmanaged[Stdcall]<IntPtr, IntPtr> CreateCompatibleDC = (delegate* unmanaged[Stdcall]<IntPtr, IntPtr>)(delegate*<IntPtr, IntPtr>)&gf.CreateCompatibleDC;

		private delegate* unmanaged[Stdcall]<IntPtr, int, int, IntPtr> CreateCompatibleBitmap = (delegate* unmanaged[Stdcall]<IntPtr, int, int, IntPtr>)(delegate*<IntPtr, int, int, IntPtr>)&gf.CreateCompatibleBitmap;

		private delegate* unmanaged[Stdcall]<IntPtr, IntPtr, IntPtr> SelectObject = (delegate* unmanaged[Stdcall]<IntPtr, IntPtr, IntPtr>)(delegate*<IntPtr, IntPtr, IntPtr>)&gf.SelectObject;

		private delegate* unmanaged[Stdcall]<IntPtr, IntPtr> DeleteDC = (delegate* unmanaged[Stdcall]<IntPtr, IntPtr>)(delegate*<IntPtr, IntPtr>)&gf.DeleteDC;

		private delegate* unmanaged[Stdcall]<IntPtr, IntPtr> DeleteObject = (delegate* unmanaged[Stdcall]<IntPtr, IntPtr>)(delegate*<IntPtr, IntPtr>)&gf.DeleteObject;

		private delegate* unmanaged[Stdcall]<IntPtr, int, int, int, int, IntPtr, int, int, int, bool> BitBlt = (delegate* unmanaged[Stdcall]<IntPtr, int, int, int, int, IntPtr, int, int, int, bool>)(delegate*<IntPtr, int, int, int, int, IntPtr, int, int, int, bool>)&gf.BitBlt;
		// Cached objects for performance optimization
		private ImageAttributes _cachedImageAttributes;
		private ColorMatrix _cachedColorMatrix;
		private static readonly Random _random = new Random();

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
        public TransitionTypes PreviousTransitionType
		{
			get
			{
				return _previousTransitionType;
			}
			set
			{
				_previousTransitionType = value;
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
				if (value == 0)
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
				if (value == 0)
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
				_currentCombinedImage = value;
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
				_newCombinedImage = value;
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
				_newBackgroundPicture = value;
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
				_currentBackgroundPicture = value;
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
				_newTextImage = value;
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
				_currentTextImage = value;
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
				_newPanelImage = value;
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
				_currentPanelImage = value;
			}
		}

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Image ImageWorkSpace
		{
			get
			{
				return _imageWorkSpace;
			}
			set
			{
				_imageWorkSpace = value;
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
        public bool ItemChanged
		{
			get
			{
				return _itemChanged;
			}
			set
			{
				_itemChanged = value;
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
				return Convert.ToSingle(_alertTransitionTime.TotalSeconds);
			}
			set
			{
				_alertTransitionTime = new TimeSpan(0, 0, 0, 0, Convert.ToInt32(1000f * value));
			}
		}

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public float AlertMessageTransitionTime
		{
			get
			{
				return Convert.ToSingle(_alertMessageTransitionTime.TotalSeconds);
			}
			set
			{
				_alertMessageTransitionTime = new TimeSpan(0, 0, 0, 0, Convert.ToInt32(1000f * value));
			}
		}

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public float RefTransitionTime
		{
			get
			{
				return Convert.ToSingle(_refTransitionTime.TotalSeconds);
			}
			set
			{
				_refTransitionTime = new TimeSpan(0, 0, 0, 0, Convert.ToInt32(1000f * value));
			}
		}

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public float RefMessageTransitionTime
		{
			get
			{
				return Convert.ToSingle(_refMessageTransitionTime.TotalSeconds);
			}
			set
			{
				_refMessageTransitionTime = new TimeSpan(0, 0, 0, 0, Convert.ToInt32(1000f * value));
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
			lock (_bitsArrayLock)
			{
				BitsArraySet.Clear(); // Much faster than iterating through 60000 items
				TotalBitsArraySet = 0;
			}
			int num = (TransitionType == TransitionTypes.Dissolve) ? 90000 : 20000;
			DissolveSize = base.Height * base.Width / num + 1;
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
			SetStyle(ControlStyles.UserPaint | ControlStyles.Opaque | ControlStyles.ResizeRedraw | ControlStyles.SupportsTransparentBackColor | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, value: true);
			BuildTransitionArray();
			
			// Initialize cached objects for performance
			_cachedImageAttributes = new ImageAttributes();
			_cachedColorMatrix = new ColorMatrix();
			
			BackColor = gf.TransparentColour;
		}

		public void SetDefaultBackgroundPicture(Image InBitmap)
		{
			BackgroundImage = InBitmap;
		}

		public void Go(TransitionAction TransitionAction, bool InFirstShowing, bool ClearAll, bool DoActiveIndicator, bool LiveCamOnShow)
		{
			ItemFirstShowing = ((InFirstShowing || DoActiveIndicator) ? true : false);
			Newbmp = new Bitmap(NewBackgroundPicture.Width, NewBackgroundPicture.Height, PixelFormat.Format32bppPArgb);
			Newg = Graphics.FromImage(Newbmp);
			if (LiveCamOnShow)
			{
				Newg.Clear(gf.TransparentColour);
			}
			else
			{
				Newg.DrawImageUnscaled(NewBackgroundPicture, 0, 0);
				Newg.DrawImageUnscaled(NewTextImage, 0, 0);
				Newg.DrawImageUnscaled(NewPanelImage, 0, 0);
			}
			Newg.Dispose();

			if (TransitionAction == TransitionAction.None)
			{
				_currentPercentage = 100f;
				if (ClearAll)
				{
					StopRef();
				}
				NewCombinedImage = Newbmp;
				Invalidate();
				return;
			}
			if (NewCombinedImage != null)
			{
				CurrentCombinedImage = NewCombinedImage;
			}
			ImageWorkSpace = NewCombinedImage;
			NewCombinedImage = Newbmp;
			_previousTransitionType = _transitionType;
			if (TransitionAction == TransitionAction.AsFade)
			{
				_transitionType = TransitionTypes.Fade;
			}
			// Dispose previous timer to prevent multiple timers running
			t?.Dispose();
			t = new System.Threading.Timer(Tick, null, 40, 40);
			_currentPercentage = 0f;
			_startTime = DateTime.Now;
			ResetBitsArray();
			if (ClearAll)
			{
				StopRef();
			}
			Invalidate();
		}

		private void Tick(object state)
		{
			TimeSpan timeSpan = DateTime.Now.Subtract(_startTime);
			_currentPercentage = ((TransitionType == TransitionTypes.None) ? 100f : Convert.ToSingle(100.0 / _transitionTime.TotalSeconds * timeSpan.TotalSeconds));
			if (_currentPercentage > 100f)
			{
				_currentPercentage = 100f;
			}
			Invalidate();
		}

		private void DrawDefaultBackPattern(PaintEventArgs e)
		{
			DrawImageToOutput(e, BackgroundImage, base.ClientRectangle, new Rectangle(0, 0, BackgroundImage.Width, BackgroundImage.Height), DrawAlert: false, null);
		}

		private void DrawBackPatternOnWorkSpace(ref Graphics g, Image InBackgroundImage)
		{
			g.DrawImage(InBackgroundImage, new Rectangle(0, 0, _newBackgroundPicture.Width, _newBackgroundPicture.Height), 0, 0, _newBackgroundPicture.Width, _newBackgroundPicture.Height, GraphicsUnit.Pixel);
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			if (_currentCombinedImage == null || _newCombinedImage == null)
			{
				DrawDefaultBackPattern(e);
				return;
			}
			GraphicsPath graphicsPath = null;
			GraphicsPath graphicsPath2 = null;
			if (_currentPercentage < 100f)
			{
				if (TransitionType == TransitionTypes.None)
				{
					Draw_NewImageBitBlt(e);
				}
				else if (TransitionType == TransitionTypes.Ascend)
				{
					// Reusing cached ImageAttributes
					// Reusing cached ColorMatrix
					float num = 0f;
					int num2 = 0;
					float num3 = 0f;
					if (TransitBackPictureAction == BackPicturesTransition.None)
					{
						DrawImageToOutput(e, _currentBackgroundPicture, base.ClientRectangle, new Rectangle(0, 0, _currentBackgroundPicture.Width, _currentBackgroundPicture.Height), DrawAlert: false, null);
						num3 = 0.003921569f * (255f * (1f - _currentPercentage / 60f));
						num3 = (_cachedColorMatrix.Matrix33 = ((num3 > 1f) ? 1f : ((num3 < 0f) ? 0f : num3)));
						_cachedImageAttributes.SetColorMatrix(_cachedColorMatrix);
						DrawImageToOutput(e, _currentTextImage, base.ClientRectangle, new Rectangle(0, 0, _newCombinedImage.Width, _newCombinedImage.Height), DrawAlert: true, _cachedImageAttributes, PanelType.Current);
						num3 = 0.003921569f * (510f * _currentPercentage / 100f);
						num3 = (_cachedColorMatrix.Matrix33 = ((num3 > 1f) ? 1f : ((num3 < 0f) ? 0f : num3)));
						_cachedImageAttributes.SetColorMatrix(_cachedColorMatrix);
						num = 0.15f * (float)_newCombinedImage.Height;
						num2 = (int)(num * _currentPercentage / 100f - num);
						DrawImageToOutput(e, _newTextImage, base.ClientRectangle, new Rectangle(0, num2, _newCombinedImage.Width, _newCombinedImage.Height), DrawAlert: true, _cachedImageAttributes, PanelType.New);
					}
					else
					{
						Draw_CurrentImage(e);
						_cachedColorMatrix.Matrix33 = 0.003921569f * (255f * _currentPercentage / 100f);
						_cachedImageAttributes.SetColorMatrix(_cachedColorMatrix);
						num = 0.15f * (float)_newCombinedImage.Height;
						num2 = (int)(num * _currentPercentage / 100f - num);
						DrawImageToOutput(e, _newCombinedImage, base.ClientRectangle, new Rectangle(0, num2, _newCombinedImage.Width, _newCombinedImage.Height), DrawAlert: true, _cachedImageAttributes);
					}
					_cachedImageAttributes.Dispose();
				}
				else if (TransitionType == TransitionTypes.Descend)
				{
					// Reusing cached ImageAttributes
					// Reusing cached ColorMatrix
					float num = 0f;
					int num2 = 0;
					float num3 = 0f;
					if (TransitBackPictureAction == BackPicturesTransition.None)
					{
						DrawImageToOutput(e, _currentBackgroundPicture, base.ClientRectangle, new Rectangle(0, 0, _currentBackgroundPicture.Width, _currentBackgroundPicture.Height), DrawAlert: false, null);
						num3 = 0.003921569f * (255f * (1f - _currentPercentage / 60f));
						num3 = (_cachedColorMatrix.Matrix33 = ((num3 > 1f) ? 1f : ((num3 < 0f) ? 0f : num3)));
						_cachedImageAttributes.SetColorMatrix(_cachedColorMatrix);
						DrawImageToOutput(e, _currentTextImage, base.ClientRectangle, new Rectangle(0, 0, _newCombinedImage.Width, _newCombinedImage.Height), DrawAlert: true, _cachedImageAttributes, PanelType.Current);
						num3 = 0.003921569f * (510f * _currentPercentage / 100f);
						num3 = (_cachedColorMatrix.Matrix33 = ((num3 > 1f) ? 1f : ((num3 < 0f) ? 0f : num3)));
						_cachedImageAttributes.SetColorMatrix(_cachedColorMatrix);
						num = 0.15f * (float)_newCombinedImage.Height;
						num2 = (int)(num - num * _currentPercentage / 100f);
						DrawImageToOutput(e, _newTextImage, base.ClientRectangle, new Rectangle(0, num2, _newCombinedImage.Width, _newCombinedImage.Height), DrawAlert: true, _cachedImageAttributes, PanelType.New);
					}
					else
					{
						Draw_CurrentImage(e);
						_cachedColorMatrix.Matrix33 = 0.003921569f * (255f * _currentPercentage / 100f);
						_cachedImageAttributes.SetColorMatrix(_cachedColorMatrix);
						num = 0.15f * (float)_newCombinedImage.Height;
						num2 = (int)(num - num * _currentPercentage / 100f);
						DrawImageToOutput(e, _newCombinedImage, base.ClientRectangle, new Rectangle(0, num2, _newCombinedImage.Width, _newCombinedImage.Height), DrawAlert: true, _cachedImageAttributes);
					}
					_cachedImageAttributes.Dispose();
				}
				else if (TransitionType == TransitionTypes.DoorsOpen)
				{
					_imageWorkSpace = new Bitmap(_newBackgroundPicture.Width, _newBackgroundPicture.Height);
					wsg = Graphics.FromImage(_imageWorkSpace);
					wsg.Clear(Color.Transparent);
					int srcX = 0;
					int srcY = 0;
					int srcWidth = Convert.ToInt32(_newBackgroundPicture.Width / 2);
					int height = _newBackgroundPicture.Height;
					Rectangle destRect = new Rectangle(0, 0, Convert.ToInt32((float)(_newBackgroundPicture.Width / 2) - (float)_newBackgroundPicture.Width * _currentPercentage / 200f), _newBackgroundPicture.Height);
					int srcX2 = Convert.ToInt32(_newBackgroundPicture.Width / 2);
					int srcY2 = 0;
					int srcWidth2 = Convert.ToInt32(_newBackgroundPicture.Width / 2);
					int height2 = _newBackgroundPicture.Height;
					Rectangle destRect2 = new Rectangle(Convert.ToInt32((float)(_newBackgroundPicture.Width / 2) + (float)_newBackgroundPicture.Width * _currentPercentage / 200f), 0, Convert.ToInt32((float)(_newBackgroundPicture.Width / 2) - (float)_newBackgroundPicture.Width * _currentPercentage / 200f), _newBackgroundPicture.Height);
					if (TransitBackPictureAction == BackPicturesTransition.None)
					{
						Draw_NewImage(e);
						wsg.DrawImage(_newBackgroundPicture, destRect, destRect.X, destRect.Y, destRect.Width, destRect.Height, GraphicsUnit.Pixel);
						wsg.DrawImage(_newBackgroundPicture, destRect2, destRect2.X, destRect2.Y, destRect2.Width, destRect2.Height, GraphicsUnit.Pixel);
						wsg.DrawImage(_currentTextImage, destRect, srcX, srcY, srcWidth, height, GraphicsUnit.Pixel);
						wsg.DrawImage(_currentTextImage, destRect2, srcX2, srcY2, srcWidth2, height2, GraphicsUnit.Pixel);
						DrawImageToOutput(e, _imageWorkSpace, base.ClientRectangle, new Rectangle(0, 0, _imageWorkSpace.Width, _imageWorkSpace.Height), DrawAlert: true, null, PanelType.Current);
					}
					else
					{
						Draw_NewImage(e);
						wsg.DrawImage(_currentCombinedImage, destRect, srcX, srcY, srcWidth, height, GraphicsUnit.Pixel);
						wsg.DrawImage(_currentCombinedImage, destRect2, srcX2, srcY2, srcWidth2, height2, GraphicsUnit.Pixel);
						DrawImageToOutput(e, _imageWorkSpace, base.ClientRectangle, new Rectangle(0, 0, _imageWorkSpace.Width, _imageWorkSpace.Height), DrawAlert: true, null);
					}
					_imageWorkSpace.Dispose();
				}
				else if (TransitionType == TransitionTypes.DoorsClose)
				{
					_imageWorkSpace = new Bitmap(_newBackgroundPicture.Width, _newBackgroundPicture.Height);
					wsg = Graphics.FromImage(_imageWorkSpace);
					wsg.Clear(Color.Transparent);
					int srcX = 0;
					int srcY = 0;
					int srcWidth = Convert.ToInt32(_newBackgroundPicture.Width / 2);
					int height = _newBackgroundPicture.Height;
					Rectangle destRect = new Rectangle(0, 0, Convert.ToInt32((float)_newBackgroundPicture.Width * _currentPercentage / 200f), _newBackgroundPicture.Height);
					int srcX2 = Convert.ToInt32(_newBackgroundPicture.Width / 2);
					int srcY2 = 0;
					int srcWidth2 = Convert.ToInt32(_newBackgroundPicture.Width / 2);
					int height2 = _newBackgroundPicture.Height;
					Rectangle destRect2 = new Rectangle(Convert.ToInt32((float)_newBackgroundPicture.Width - (float)_newBackgroundPicture.Width * _currentPercentage / 200f), 0, Convert.ToInt32((float)_newBackgroundPicture.Width * _currentPercentage / 200f), _newBackgroundPicture.Height);
					if (TransitBackPictureAction == BackPicturesTransition.None)
					{
						Draw_CurrentImage(e);
						wsg.DrawImage(_newBackgroundPicture, destRect, destRect.X, destRect.Y, destRect.Width, destRect.Height, GraphicsUnit.Pixel);
						wsg.DrawImage(_newBackgroundPicture, destRect2, destRect2.X, destRect2.Y, destRect2.Width, destRect2.Height, GraphicsUnit.Pixel);
						wsg.DrawImage(_newTextImage, destRect, srcX, srcY, srcWidth, height, GraphicsUnit.Pixel);
						wsg.DrawImage(_newTextImage, destRect2, srcX2, srcY2, srcWidth2, height2, GraphicsUnit.Pixel);
						DrawImageToOutput(e, _imageWorkSpace, base.ClientRectangle, new Rectangle(0, 0, _imageWorkSpace.Width, _imageWorkSpace.Height), DrawAlert: true, null, PanelType.New);
					}
					else
					{
						Draw_CurrentImage(e);
						wsg.DrawImage(_newCombinedImage, destRect, srcX, srcY, srcWidth, height, GraphicsUnit.Pixel);
						wsg.DrawImage(_newCombinedImage, destRect2, srcX2, srcY2, srcWidth2, height2, GraphicsUnit.Pixel);
						DrawImageToOutput(e, _imageWorkSpace, base.ClientRectangle, new Rectangle(0, 0, _imageWorkSpace.Width, _imageWorkSpace.Height), DrawAlert: true, null);
					}
					_imageWorkSpace.Dispose();
				}
				else if (TransitionType == TransitionTypes.Blinds_H)
				{
					Draw_CurrentImage(e);
					for (int i = 0; i <= _nHDivs; i++)
					{
						DrawImageToOutput(SourceRectangle: new Rectangle(0, i * (_newCombinedImage.Height / _nHDivs), _newCombinedImage.Width, Convert.ToInt32((float)(_newCombinedImage.Height / _nHDivs) * _currentPercentage / 100f)), DestinationRectangle: new Rectangle(0, i * (base.Height / _nHDivs), base.Width, Convert.ToInt32((float)(base.Height / _nHDivs) * _currentPercentage / 100f)), e: e, InImage: _newCombinedImage, DrawAlert: true, InImageAttributes: null);
					}
				}
				else if (TransitionType == TransitionTypes.Blinds_V)
				{
					Draw_CurrentImage(e);
					for (int j = 0; j <= _nVDivs; j++)
					{
						DrawImageToOutput(SourceRectangle: new Rectangle(j * (_newCombinedImage.Width / _nVDivs), 0, Convert.ToInt32((float)(_newCombinedImage.Width / _nVDivs) * _currentPercentage / 100f), _newCombinedImage.Height), DestinationRectangle: new Rectangle(j * (base.Width / _nVDivs), 0, Convert.ToInt32((float)(base.Width / _nVDivs) * _currentPercentage / 100f), base.Height), e: e, InImage: _newCombinedImage, DrawAlert: true, InImageAttributes: null);
					}
				}
				else if (TransitionType == TransitionTypes.Rectangle)
				{
					Draw_CurrentImage(e);
					graphicsPath = new GraphicsPath();
					int num8 = Convert.ToInt32((float)base.Width * _currentPercentage / 200f);
					int num9 = Convert.ToInt32((float)base.Height * _currentPercentage / 200f);
					graphicsPath.AddRectangle(new Rectangle(Convert.ToInt32(base.Width / 2 - num8), Convert.ToInt32(base.Height / 2 - num9), Convert.ToInt32(2 * num8), Convert.ToInt32(2 * num9)));
					e.Graphics.SetClip(graphicsPath, CombineMode.Replace);
					//Draw_NewImage(e);
					graphicsPath.Dispose();
				}
				else if (TransitionType == TransitionTypes.RectangleIn)
				{
					Draw_NewImage(e);
					graphicsPath = new GraphicsPath();
					int num8 = Convert.ToInt32((float)base.Width * (100f - _currentPercentage) / 200f);
					int num9 = Convert.ToInt32((float)base.Height * (100f - _currentPercentage) / 200f);
					graphicsPath.AddRectangle(new Rectangle(Convert.ToInt32(base.Width / 2 - num8), Convert.ToInt32(base.Height / 2 - num9), Convert.ToInt32(2 * num8), Convert.ToInt32(2 * num9)));
					e.Graphics.SetClip(graphicsPath, CombineMode.Replace);
					//Draw_CurrentImage(e);
					graphicsPath.Dispose();
				}
				else if (TransitionType == TransitionTypes.Checkerboard)
				{
					Draw_CurrentImage(e);
					graphicsPath = new GraphicsPath();
					int width = Convert.ToInt32((float)base.Width * _currentPercentage / 100f) / _nHDivs;
					int num10 = base.Height / _nVDivs;
					int num11 = 0;
					for (int i = 0; i <= base.Height; i += num10)
					{
						for (int j = 0; j <= base.Width; j += base.Width / _nHDivs)
						{
							Rectangle rect = new Rectangle(j, i, width, num10);
							if ((num11 & 1) == 1)
							{
								rect.Offset(base.Width / (2 * _nVDivs), 0);
							}
							graphicsPath.AddRectangle(rect);
							if (_currentPercentage >= 50f && (num11 & 1) == 1 && j == 0)
							{
								rect.Offset(-(base.Width / _nHDivs), 0);
								graphicsPath.AddRectangle(rect);
							}
						}
						num11++;
					}
					Region region = new Region(graphicsPath);
					e.Graphics.SetClip(region, CombineMode.Replace);
					Draw_NewImage(e);
					region.Dispose();
					graphicsPath.Dispose();
				}
				else if (TransitionType == TransitionTypes.Circle)
				{
					Draw_CurrentImage(e);
					graphicsPath = new GraphicsPath();
					int num8 = Convert.ToInt32((float)base.Width * 1.414f * _currentPercentage / 200f);
					graphicsPath.AddEllipse(Convert.ToInt32(base.Width / 2 - num8), Convert.ToInt32(base.Height / 2 - num8), Convert.ToInt32(2 * num8), Convert.ToInt32(2 * num8));
					e.Graphics.SetClip(graphicsPath, CombineMode.Replace);
					Draw_NewImage(e);
					graphicsPath.Dispose();
				}
				else if (TransitionType == TransitionTypes.Cross)
				{
					Draw_CurrentImage(e);
					graphicsPath = new GraphicsPath();
					graphicsPath2 = new GraphicsPath();
					int num8 = Convert.ToInt32((float)base.Width * _currentPercentage / 200f);
					int num9 = Convert.ToInt32((float)base.Height * _currentPercentage / 40f);
					graphicsPath.AddRectangle(new Rectangle(Convert.ToInt32(base.Width / 2 - num9), Convert.ToInt32(base.Height / 2 - num8), Convert.ToInt32(2 * num9), Convert.ToInt32(2 * num8)));
					e.Graphics.SetClip(graphicsPath, CombineMode.Replace);
					graphicsPath.AddRectangle(new Rectangle(Convert.ToInt32(base.Width / 2 - num8), Convert.ToInt32(base.Height / 2 - num9), Convert.ToInt32(2 * num8), Convert.ToInt32(2 * num9)));
					e.Graphics.SetClip(graphicsPath, CombineMode.Union);
					Draw_NewImage(e);
					graphicsPath.Dispose();
				}
				else if (TransitionType == TransitionTypes.Diamond)
				{
					Draw_CurrentImage(e);
					int num8 = Convert.ToInt32((float)base.Width * _currentPercentage / 100f);
					int num9 = Convert.ToInt32((float)base.Height * _currentPercentage / 100f);
					int num12 = Convert.ToInt32(base.Width / 2);
					int num13 = Convert.ToInt32(base.Height / 2);
					graphicsPath = new GraphicsPath();
					graphicsPath.AddPolygon(new Point[4]
					{
						new Point(num12, num13 - num9),
						new Point(num12 - num8, num13),
						new Point(num12, num13 + num9),
						new Point(num12 + num8, num13)
					});
					e.Graphics.SetClip(graphicsPath, CombineMode.Replace);
					Draw_NewImage(e);
					graphicsPath.Dispose();
				}
				else if (TransitionType == TransitionTypes.Fade)
				{
					Draw_CurrentImage(e);
					// Reusing cached ImageAttributes
					// Reusing cached ColorMatrix
					_cachedColorMatrix.Matrix33 = 0.003921569f * (255f * _currentPercentage / 100f);
					_cachedImageAttributes.SetColorMatrix(_cachedColorMatrix);
					DrawImageToOutput(e, _newCombinedImage, base.ClientRectangle, new Rectangle(0, 0, _newCombinedImage.Width, _newCombinedImage.Height), DrawAlert: true, _cachedImageAttributes);
					_cachedImageAttributes.Dispose();
				}
				else if (TransitionType == TransitionTypes.Flip_H)
				{
					BackPicturesTransition transitBackPictureAction = TransitBackPictureAction;
					if (transitBackPictureAction == BackPicturesTransition.BothBackgrounds)
					{
						DrawDefaultBackPattern(e);
						if (_currentPercentage < 50f)
						{
							DrawImageToOutput(e, _currentCombinedImage, new Rectangle(0, Convert.ToInt32((float)base.Height * _currentPercentage / 100f), base.ClientRectangle.Width, Convert.ToInt32((float)base.Height - (float)base.Height * _currentPercentage / 50f)), new Rectangle(0, 0, _currentCombinedImage.Width, _currentCombinedImage.Height), DrawAlert: true, null);
						}
						else
						{
							DrawImageToOutput(e, _newCombinedImage, new Rectangle(0, Convert.ToInt32((float)base.Height - (float)base.Height * _currentPercentage / 100f), base.ClientRectangle.Width, Convert.ToInt32((float)base.Height * (_currentPercentage - 50f) / 50f)), new Rectangle(0, 0, _currentCombinedImage.Width, _currentCombinedImage.Height), DrawAlert: true, null);
						}
					}
					else
					{
						_imageWorkSpace = new Bitmap(_newBackgroundPicture.Width, _newBackgroundPicture.Height);
						wsg = Graphics.FromImage(_imageWorkSpace);
						wsg.Clear(Color.Transparent);
						int srcX3 = 0;
						int srcY3 = 0;
						int width2 = _currentCombinedImage.Width;
						int height3 = _currentCombinedImage.Height;
						int x = 0;
						int num14 = 0;
						int width3 = _currentCombinedImage.Width;
						int num15 = 0;
						switch (TransitBackPictureAction)
						{
							case BackPicturesTransition.CurrentOnly:
								DrawBackPatternOnWorkSpace(ref wsg, BackgroundImage);
								if (_currentPercentage < 50f)
								{
									num14 = Convert.ToInt32((float)_currentCombinedImage.Height * _currentPercentage / 100f);
									num15 = Convert.ToInt32((float)_currentCombinedImage.Height - (float)_currentCombinedImage.Height * _currentPercentage / 50f);
									Rectangle destRect3 = new Rectangle(x, num14, width3, num15);
									wsg.DrawImage(_currentCombinedImage, destRect3, srcX3, srcY3, width2, height3, GraphicsUnit.Pixel);
									DrawImageToOutput(e, _imageWorkSpace, base.ClientRectangle, new Rectangle(0, 0, _imageWorkSpace.Width, _imageWorkSpace.Height), DrawAlert: true, null);
								}
								else
								{
									num14 = Convert.ToInt32((float)_currentCombinedImage.Height - (float)_currentCombinedImage.Height * _currentPercentage / 100f);
									num15 = Convert.ToInt32((float)_currentCombinedImage.Height * (_currentPercentage - 50f) / 50f);
									Rectangle destRect3 = new Rectangle(x, num14, width3, num15);
									wsg.DrawImage(_newTextImage, destRect3, srcX3, srcY3, width2, height3, GraphicsUnit.Pixel);
									wsg.DrawImage(_newPanelImage, destRect3, srcX3, srcY3, width2, height3, GraphicsUnit.Pixel);
									DrawImageToOutput(e, _imageWorkSpace, base.ClientRectangle, new Rectangle(0, 0, _imageWorkSpace.Width, _imageWorkSpace.Height), DrawAlert: true, null);
								}
								break;
							case BackPicturesTransition.NewOnly:
								DrawBackPatternOnWorkSpace(ref wsg, BackgroundImage);
								if (_currentPercentage < 50f)
								{
									num14 = Convert.ToInt32((float)_currentCombinedImage.Height * _currentPercentage / 100f);
									num15 = Convert.ToInt32((float)_currentCombinedImage.Height - (float)_currentCombinedImage.Height * _currentPercentage / 50f);
									Rectangle destRect3 = new Rectangle(x, num14, width3, num15);
									wsg.DrawImage(_currentTextImage, destRect3, srcX3, srcY3, width2, height3, GraphicsUnit.Pixel);
									wsg.DrawImage(_currentPanelImage, destRect3, srcX3, srcY3, width2, height3, GraphicsUnit.Pixel);
									DrawImageToOutput(e, _imageWorkSpace, base.ClientRectangle, new Rectangle(0, 0, _imageWorkSpace.Width, _imageWorkSpace.Height), DrawAlert: true, null);
								}
								else
								{
									num14 = Convert.ToInt32((float)_currentCombinedImage.Height - (float)_currentCombinedImage.Height * _currentPercentage / 100f);
									num15 = Convert.ToInt32((float)_currentCombinedImage.Height * (_currentPercentage - 50f) / 50f);
									Rectangle destRect3 = new Rectangle(x, num14, width3, num15);
									wsg.DrawImage(_newCombinedImage, destRect3, srcX3, srcY3, width2, height3, GraphicsUnit.Pixel);
									DrawImageToOutput(e, _imageWorkSpace, base.ClientRectangle, new Rectangle(0, 0, _imageWorkSpace.Width, _imageWorkSpace.Height), DrawAlert: true, null);
								}
								break;
							default:
								DrawBackPatternOnWorkSpace(ref wsg, CurrentBackgroundPicture);
								if (_currentPercentage < 50f)
								{
									num14 = Convert.ToInt32((float)_currentCombinedImage.Height * _currentPercentage / 100f);
									num15 = Convert.ToInt32((float)_currentCombinedImage.Height - (float)_currentCombinedImage.Height * _currentPercentage / 50f);
									Rectangle destRect3 = new Rectangle(x, num14, width3, num15);
									wsg.DrawImage(_currentTextImage, destRect3, srcX3, srcY3, width2, height3, GraphicsUnit.Pixel);
									DrawImageToOutput(e, _imageWorkSpace, base.ClientRectangle, new Rectangle(0, 0, _imageWorkSpace.Width, _imageWorkSpace.Height), DrawAlert: true, null, PanelType.Current);
								}
								else
								{
									num14 = Convert.ToInt32((float)_currentCombinedImage.Height - (float)_currentCombinedImage.Height * _currentPercentage / 100f);
									num15 = Convert.ToInt32((float)_currentCombinedImage.Height * (_currentPercentage - 50f) / 50f);
									Rectangle destRect3 = new Rectangle(x, num14, width3, num15);
									wsg.DrawImage(_newTextImage, destRect3, srcX3, srcY3, width2, height3, GraphicsUnit.Pixel);
									DrawImageToOutput(e, _imageWorkSpace, base.ClientRectangle, new Rectangle(0, 0, _imageWorkSpace.Width, _imageWorkSpace.Height), DrawAlert: true, null, PanelType.New);
								}
								break;
						}
						wsg.Dispose();
						_imageWorkSpace.Dispose();
					}
					
				}
				else if (TransitionType == TransitionTypes.Flip_V)
				{
					BackPicturesTransition transitBackPictureAction = TransitBackPictureAction;
					if (transitBackPictureAction == BackPicturesTransition.BothBackgrounds)
					{
						DrawDefaultBackPattern(e);
						if (_currentPercentage < 50f)
						{
							DrawImageToOutput(e, _currentCombinedImage, new Rectangle(Convert.ToInt32((float)base.Width * _currentPercentage / 100f), 0, Convert.ToInt32((float)base.Width - (float)base.Width * _currentPercentage / 50f), base.ClientRectangle.Height), new Rectangle(0, 0, _currentCombinedImage.Width, _currentCombinedImage.Height), DrawAlert: true, null);
						}
						else
						{
							DrawImageToOutput(e, _newCombinedImage, new Rectangle(Convert.ToInt32((float)base.Width - (float)base.Width * _currentPercentage / 100f), 0, Convert.ToInt32((float)base.Width * (_currentPercentage - 50f) / 50f), base.ClientRectangle.Height), new Rectangle(0, 0, _currentCombinedImage.Width, _currentCombinedImage.Height), DrawAlert: true, null);
						}
					}
					else
					{
						_imageWorkSpace = new Bitmap(_newBackgroundPicture.Width, _newBackgroundPicture.Height);
						wsg = Graphics.FromImage(_imageWorkSpace);
						wsg.Clear(Color.Transparent);
						int srcX3 = 0;
						int srcY3 = 0;
						int width2 = _currentCombinedImage.Width;
						int height3 = _currentCombinedImage.Height;
						int x = 0;
						int num14 = 0;
						int width3 = 0;
						int num15 = _currentCombinedImage.Height;
						switch (TransitBackPictureAction)
						{
							case BackPicturesTransition.CurrentOnly:
								DrawBackPatternOnWorkSpace(ref wsg, BackgroundImage);
								if (_currentPercentage < 50f)
								{
									x = Convert.ToInt32((float)_currentCombinedImage.Width * _currentPercentage / 100f);
									width3 = Convert.ToInt32((float)_currentCombinedImage.Width - (float)_currentCombinedImage.Width * _currentPercentage / 50f);
									Rectangle destRect3 = new Rectangle(x, num14, width3, num15);
									wsg.DrawImage(_currentCombinedImage, destRect3, srcX3, srcY3, width2, height3, GraphicsUnit.Pixel);
									DrawImageToOutput(e, _imageWorkSpace, base.ClientRectangle, new Rectangle(0, 0, _imageWorkSpace.Width, _imageWorkSpace.Height), DrawAlert: true, null);
								}
								else
								{
									x = Convert.ToInt32((float)_currentCombinedImage.Width - (float)_currentCombinedImage.Width * _currentPercentage / 100f);
									width3 = Convert.ToInt32((float)_currentCombinedImage.Width * (_currentPercentage - 50f) / 50f);
									Rectangle destRect3 = new Rectangle(x, num14, width3, num15);
									wsg.DrawImage(_newTextImage, destRect3, srcX3, srcY3, width2, height3, GraphicsUnit.Pixel);
									wsg.DrawImage(_newPanelImage, destRect3, srcX3, srcY3, width2, height3, GraphicsUnit.Pixel);
									DrawImageToOutput(e, _imageWorkSpace, base.ClientRectangle, new Rectangle(0, 0, _imageWorkSpace.Width, _imageWorkSpace.Height), DrawAlert: true, null);
								}
								break;
							case BackPicturesTransition.NewOnly:
								DrawBackPatternOnWorkSpace(ref wsg, BackgroundImage);
								if (_currentPercentage < 50f)
								{
									x = Convert.ToInt32((float)_currentCombinedImage.Width * _currentPercentage / 100f);
									width3 = Convert.ToInt32((float)_currentCombinedImage.Width - (float)_currentCombinedImage.Width * _currentPercentage / 50f);
									Rectangle destRect3 = new Rectangle(x, num14, width3, num15);
									wsg.DrawImage(_currentTextImage, destRect3, srcX3, srcY3, width2, height3, GraphicsUnit.Pixel);
									DrawImageToOutput(e, _imageWorkSpace, base.ClientRectangle, new Rectangle(0, 0, _imageWorkSpace.Width, _imageWorkSpace.Height), DrawAlert: true, null, PanelType.CurrentWithTrans);
								}
								else
								{
									x = Convert.ToInt32((float)_currentCombinedImage.Width - (float)_currentCombinedImage.Width * _currentPercentage / 100f);
									width3 = Convert.ToInt32((float)_currentCombinedImage.Width * (_currentPercentage - 50f) / 50f);
									Rectangle destRect3 = new Rectangle(x, num14, width3, num15);
									wsg.DrawImage(_newCombinedImage, destRect3, srcX3, srcY3, width2, height3, GraphicsUnit.Pixel);
									DrawImageToOutput(e, _imageWorkSpace, base.ClientRectangle, new Rectangle(0, 0, _imageWorkSpace.Width, _imageWorkSpace.Height), DrawAlert: true, null);
								}
								break;
							default:
								DrawBackPatternOnWorkSpace(ref wsg, CurrentBackgroundPicture);
								if (_currentPercentage < 50f)
								{
									x = Convert.ToInt32((float)_currentCombinedImage.Width * _currentPercentage / 100f);
									width3 = Convert.ToInt32((float)_currentCombinedImage.Width - (float)_currentCombinedImage.Width * _currentPercentage / 50f);
									Rectangle destRect3 = new Rectangle(x, num14, width3, num15);
									wsg.DrawImage(_currentTextImage, destRect3, srcX3, srcY3, width2, height3, GraphicsUnit.Pixel);
									DrawImageToOutput(e, _imageWorkSpace, base.ClientRectangle, new Rectangle(0, 0, _imageWorkSpace.Width, _imageWorkSpace.Height), DrawAlert: true, null, PanelType.Current);
								}
								else
								{
									x = Convert.ToInt32((float)_currentCombinedImage.Width - (float)_currentCombinedImage.Width * _currentPercentage / 100f);
									width3 = Convert.ToInt32((float)_currentCombinedImage.Width * (_currentPercentage - 50f) / 50f);
									Rectangle destRect3 = new Rectangle(x, num14, width3, num15);
									wsg.DrawImage(_newTextImage, destRect3, srcX3, srcY3, width2, height3, GraphicsUnit.Pixel);
									DrawImageToOutput(e, _imageWorkSpace, base.ClientRectangle, new Rectangle(0, 0, _imageWorkSpace.Width, _imageWorkSpace.Height), DrawAlert: true, null, PanelType.New);
								}
								break;
						}
						wsg.Dispose();
						_imageWorkSpace.Dispose();
					}
				}
				else if (TransitionType == TransitionTypes.Gentle_Zoom)
				{
					float num16 = 0f;
					if (TransitBackPictureAction == BackPicturesTransition.None)
					{
						num16 = 0.85f + 0.15f * _currentPercentage / 100f;
						if (num16 < 0f)
						{
							num16 = 0.85f;
						}
						else if (num16 > 1f)
						{
							num16 = 1f;
						}
						Draw_CurrentImage(e);
						float dx = _newBackgroundPicture.Width / 2;
						float dy = _newBackgroundPicture.Height / 2;
						_imageWorkSpace = new Bitmap(_newBackgroundPicture.Width, _newBackgroundPicture.Height);
						wsg = Graphics.FromImage(_imageWorkSpace);
						wsg.Clear(Color.Transparent);
						Rectangle destRect4 = new Rectangle(-_newBackgroundPicture.Width / 2, -1 - _newBackgroundPicture.Height / 2, _newBackgroundPicture.Width, _newBackgroundPicture.Height);
						Rectangle rectangle = new Rectangle(destRect4.Width / 2 - (int)(num16 * (float)destRect4.Width / 2f), destRect4.Height / 2 - (int)(num16 * (float)destRect4.Height / 2f), (int)(num16 * (float)destRect4.Width), (int)(num16 * (float)destRect4.Height));
						wsg.DrawImage(_newBackgroundPicture, rectangle, rectangle, GraphicsUnit.Pixel);
						Matrix matrix = new Matrix(num16, 0f, 0f, num16, dx, dy);
						wsg.Transform = matrix;
						wsg.DrawImage(_newTextImage, destRect4, 0, 0, _newBackgroundPicture.Width, _newBackgroundPicture.Height, GraphicsUnit.Pixel);
						DrawImageToOutput(e, _imageWorkSpace, base.ClientRectangle, new Rectangle(0, 0, _imageWorkSpace.Width, _imageWorkSpace.Height), DrawAlert: true, null, PanelType.New);
						matrix.Dispose();
						wsg.Dispose();
						_imageWorkSpace.Dispose();
					}
					else
					{
						num16 = 0.8f + 0.2f * _currentPercentage / 100f;
						if (num16 < 0f)
						{
							num16 = 0.8f;
						}
						else if (num16 > 1f)
						{
							num16 = 1f;
						}
						// Reusing cached ImageAttributes
						// Reusing cached ColorMatrix
						float num3 = 0f;
						num3 = 0.003921569f * (255f * (1f - _currentPercentage / 60f));
						num3 = (_cachedColorMatrix.Matrix33 = ((num3 > 1f) ? 1f : ((num3 < 0f) ? 0f : num3)));
						_cachedImageAttributes.SetColorMatrix(_cachedColorMatrix);
						DrawImageToOutput(e, _currentCombinedImage, base.ClientRectangle, new Rectangle(0, 0, _currentBackgroundPicture.Width, _currentBackgroundPicture.Height), DrawAlert: false, _cachedImageAttributes);
						num3 = 0.003921569f * (510f * _currentPercentage / 100f);
						num3 = (_cachedColorMatrix.Matrix33 = ((num3 > 1f) ? 1f : ((num3 < 0f) ? 0f : num3)));
						_cachedImageAttributes.SetColorMatrix(_cachedColorMatrix);
						float dx = base.Width / 2;
						float dy = base.Height / 2;
						Matrix matrix2 = new Matrix(num16, 0f, 0f, num16, dx, dy);
						e.Graphics.Transform = matrix2;
						DrawImageToOutput(e, _newCombinedImage, new Rectangle(-base.Width / 2, -1 - base.Height / 2, base.Width, base.Height), new Rectangle(0, 0, _newCombinedImage.Width, _newCombinedImage.Height), DrawAlert: true, _cachedImageAttributes);
						matrix2.Dispose();
						_cachedImageAttributes.Dispose();
					}
				}
				else if (TransitionType == TransitionTypes.Oval)
				{
					Draw_CurrentImage(e);
					graphicsPath = new GraphicsPath();
					int num8 = Convert.ToInt32((float)base.Width * 1.414f * _currentPercentage / 200f);
					int num9 = Convert.ToInt32((float)base.Height * 1.2f * _currentPercentage / 200f);
					graphicsPath.AddEllipse(Convert.ToInt32(base.Width / 2 - num8), Convert.ToInt32(base.Height / 2 - num9), Convert.ToInt32(2 * num8), Convert.ToInt32(2 * num9));
					e.Graphics.SetClip(graphicsPath, CombineMode.Replace);
					Draw_NewImage(e);
					graphicsPath.Dispose();
				}
				else if (TransitionType == TransitionTypes.Mesh)
				{
					Draw_CurrentImage(e);
					int num19 = _nHDivs * 5;
					int num20 = _nVDivs * 8;
					graphicsPath = new GraphicsPath();
					int width = Convert.ToInt32((float)base.Width * _currentPercentage / 100f) / num19;
					int num10 = base.Height / num20;
					int num11 = 0;
					for (int i = 0; i <= base.Height; i += num10)
					{
						for (int j = 0; j <= base.Width; j += base.Width / num19)
						{
							Rectangle rect = new Rectangle(j, i, width, num10);
							if ((num11 & 1) == 1)
							{
								rect.Offset(base.Width / (2 * num20), 0);
							}
							graphicsPath.AddRectangle(rect);
							if (_currentPercentage >= 50f && (num11 & 1) == 1 && j == 0)
							{
								rect.Offset(-(base.Width / num19), 0);
								graphicsPath.AddRectangle(rect);
							}
						}
						num11++;
					}
					Region region = new Region(graphicsPath);
					e.Graphics.SetClip(region, CombineMode.Replace);
					Draw_NewImage(e);
					region.Dispose();
					graphicsPath.Dispose();
				}
				else if (TransitionType == TransitionTypes.InLeft)
				{
					Draw_TranSlideIn(e, TransitionType);
				}
				else if (TransitionType == TransitionTypes.InTop)
				{
					Draw_TranSlideIn(e, TransitionType);
				}
				else if (TransitionType == TransitionTypes.InRight)
				{
					Draw_TranSlideIn(e, TransitionType);
				}
				else if (TransitionType == TransitionTypes.InBottom)
				{
					Draw_TranSlideIn(e, TransitionType);
				}
				else if (TransitionType == TransitionTypes.InTopLeft)
				{
					Draw_TranSlideIn(e, TransitionType);
				}
				else if (TransitionType == TransitionTypes.InTopRight)
				{
					Draw_TranSlideIn(e, TransitionType);
				}
				else if (TransitionType == TransitionTypes.InBottomLeft)
				{
					Draw_TranSlideIn(e, TransitionType);
				}
				else if (TransitionType == TransitionTypes.InBottomRight)
				{
					Draw_TranSlideIn(e, TransitionType);
				}
				else if (TransitionType == TransitionTypes.OutLeft)
				{
					Draw_TranSlideOut(e, TransitionType);
				}
				else if (TransitionType == TransitionTypes.OutTop)
				{
					Draw_TranSlideOut(e, TransitionType);
				}
				else if (TransitionType == TransitionTypes.OutRight)
				{
					Draw_TranSlideOut(e, TransitionType);
				}
				else if (TransitionType == TransitionTypes.OutBottom)
				{
					Draw_TranSlideOut(e, TransitionType);
				}
				else if (TransitionType == TransitionTypes.OutTopLeft)
				{
					Draw_TranSlideOut(e, TransitionType);
				}
				else if (TransitionType == TransitionTypes.OutTopRight)
				{
					Draw_TranSlideOut(e, TransitionType);
				}
				else if (TransitionType == TransitionTypes.OutBottomLeft)
				{
					Draw_TranSlideOut(e, TransitionType);
				}
				else if (TransitionType == TransitionTypes.OutBottomRight)
				{
					Draw_TranSlideOut(e, TransitionType);
				}
				else if (TransitionType == TransitionTypes.OutLeft)
				{
					Draw_TranSlideOut(e, TransitionType);
				}
				else if (TransitionType == TransitionTypes.OutTop)
				{
					Draw_TranSlideOut(e, TransitionType);
				}
				else if (TransitionType == TransitionTypes.OutRight)
				{
					Draw_TranSlideOut(e, TransitionType);
				}
				else if (TransitionType == TransitionTypes.OutBottom)
				{
					Draw_TranSlideOut(e, TransitionType);
				}
				else if (TransitionType == TransitionTypes.RevealLeftRight)
				{
					Draw_TranSlideReveal(e, TransitionType);
				}
				else if (TransitionType == TransitionTypes.RevealTopDown)
				{
					Draw_TranSlideReveal(e, TransitionType);
				}
				else if (TransitionType == TransitionTypes.RevealRightLeft)
				{
					Draw_TranSlideReveal(e, TransitionType);
				}
				else if (TransitionType == TransitionTypes.RevealDownUp)
				{
					Draw_TranSlideReveal(e, TransitionType);
				}
				else if (TransitionType == TransitionTypes.Scroll)
				{
					int num2 = (int)((float)_newCombinedImage.Height * _currentPercentage / 100f);
					if (TransitBackPictureAction == BackPicturesTransition.None)
					{
						DrawImageToOutput(e, _currentBackgroundPicture, base.ClientRectangle, new Rectangle(0, 0, _currentBackgroundPicture.Width, _currentBackgroundPicture.Height), DrawAlert: false, null);
						DrawImageToOutput(e, _currentTextImage, base.ClientRectangle, new Rectangle(0, num2, _currentBackgroundPicture.Width, _currentBackgroundPicture.Height), DrawAlert: false, null, PanelType.Current);
						DrawImageToOutput(e, _newTextImage, base.ClientRectangle, new Rectangle(0, num2 - _newCombinedImage.Height, _newCombinedImage.Width, _newCombinedImage.Height), DrawAlert: true, null);
					}
					else
					{
						DrawImageToOutput(e, _currentCombinedImage, base.ClientRectangle, new Rectangle(0, num2, _currentBackgroundPicture.Width, _currentBackgroundPicture.Height), DrawAlert: false, null);
						DrawImageToOutput(e, _newCombinedImage, base.ClientRectangle, new Rectangle(0, num2 - _newCombinedImage.Height, _newCombinedImage.Width, _newCombinedImage.Height), DrawAlert: true, null);
					}
				}
				else if (TransitionType == TransitionTypes.Spin)
				{
					float angle = 360f * _currentPercentage / 100f;
					float dx2 = base.Width / 2;
					float dy2 = base.Height / 2;
					float num16 = 1f * _currentPercentage / 100f;
					if (num16 == 0f)
					{
						num16 = 0.01f;
					}
					if (TransitBackPictureAction == BackPicturesTransition.None)
					{
						DrawImageToOutput(e, _newBackgroundPicture, base.ClientRectangle, new Rectangle(0, 0, _newBackgroundPicture.Width, _newBackgroundPicture.Height), DrawAlert: true, null);
						// Reusing cached ImageAttributes
						// Reusing cached ColorMatrix
						_cachedColorMatrix.Matrix33 = 0.003921569f * (255f * (1f - _currentPercentage / 100f));
						_cachedImageAttributes.SetColorMatrix(_cachedColorMatrix);
						DrawImageToOutput(e, _currentTextImage, base.ClientRectangle, new Rectangle(0, 0, _newCombinedImage.Width, _newCombinedImage.Height), DrawAlert: false, _cachedImageAttributes, PanelType.Current);
						DrawImageToOutput(e, _currentPanelImage, base.ClientRectangle, new Rectangle(0, 0, _newCombinedImage.Width, _newCombinedImage.Height), DrawAlert: false, null, PanelType.Current);
						_cachedImageAttributes.Dispose();
						Matrix matrix = new Matrix(num16, 0f, 0f, num16, dx2, dy2);
						matrix.Rotate(angle, MatrixOrder.Prepend);
						e.Graphics.Transform = matrix;
						DrawImageToOutput(e, _newTextImage, new Rectangle(-base.Width / 2, -base.Height / 2, base.Width, base.Height), new Rectangle(0, 0, _newCombinedImage.Width, _newCombinedImage.Height), DrawAlert: true, null);
						matrix.Dispose();
					}
					else
					{
						Draw_CurrentImage(e);
						Matrix matrix2 = new Matrix(num16, 0f, 0f, num16, dx2, dy2);
						matrix2.Rotate(angle, MatrixOrder.Prepend);
						e.Graphics.Transform = matrix2;
						DrawImageToOutput(e, _newCombinedImage, new Rectangle(-base.Width / 2, -base.Height / 2, base.Width, base.Height), new Rectangle(0, 0, _newCombinedImage.Width, _newCombinedImage.Height), DrawAlert: true, null);
						matrix2.Dispose();
					}
				}
				else if (TransitionType == TransitionTypes.Spiral)
				{
					double num21 = Math.PI / 100.0;
					double num22 = Math.Max(base.Width * 2 / 3, base.Height * 2 / 3) / 100;
					float num23 = (float)base.Width * 0.59f;
					float num24 = (float)base.Height * 0.1475f;
					double num25 = _currentPercentage - 100f;
					double num26 = _currentPercentage;
					if (num25 < 0.0)
					{
						num25 = 0.0;
					}
					double num27 = num21 * num26;
					PointF pointF = new PointF(Convert.ToSingle((double)num23 + num25 * num22 * Math.Cos(num27)), Convert.ToSingle((double)num24 + num25 * num22 * Math.Sin(num27)));
					num27 = num21 * num25;
					while (num25 <= num26)
					{
						PointF pointF2 = new PointF(Convert.ToSingle((double)num23 + num25 * num22 * Math.Cos(num27)), Convert.ToSingle((double)num24 + num25 * num22 * Math.Sin(num27)));
						pointF = pointF2;
						num25 += 0.1;
						num27 += num21 / 10.0;
					}
					int width4 = (int)(_currentPercentage * (float)base.ClientRectangle.Width / 100f);
					int height4 = (int)(_currentPercentage * (float)base.ClientRectangle.Height / 100f);
					Rectangle destinationRectangle3 = new Rectangle((int)pointF.X, (int)pointF.Y, width4, height4);
					if (TransitBackPictureAction == BackPicturesTransition.None)
					{
						// Reusing cached ImageAttributes
						// Reusing cached ColorMatrix
						_cachedColorMatrix.Matrix33 = 0.003921569f * (255f * (1f - _currentPercentage / 200f));
						_cachedImageAttributes.SetColorMatrix(_cachedColorMatrix);
						DrawImageToOutput(e, _currentBackgroundPicture, base.ClientRectangle, new Rectangle(0, 0, _currentBackgroundPicture.Width, _currentBackgroundPicture.Height), DrawAlert: true, null);
						DrawImageToOutput(e, _currentTextImage, base.ClientRectangle, new Rectangle(0, 0, _currentTextImage.Width, _currentTextImage.Height), DrawAlert: false, _cachedImageAttributes, PanelType.Current);
						DrawImageToOutput(e, _currentPanelImage, base.ClientRectangle, new Rectangle(0, 0, _currentTextImage.Width, _currentTextImage.Height), DrawAlert: false, null, PanelType.Current);
						DrawImageToOutput(e, _newTextImage, destinationRectangle3, new Rectangle(0, 0, _newTextImage.Width, _newTextImage.Height), DrawAlert: true, null, PanelType.NewWithTrans);
						_cachedImageAttributes.Dispose();
					}
					else
					{
						Draw_CurrentImage(e);
						DrawImageToOutput(e, _newCombinedImage, destinationRectangle3, new Rectangle(0, 0, _newCombinedImage.Width, _newCombinedImage.Height), DrawAlert: true, null);
					}
				}
				else if (TransitionType == TransitionTypes.Star)
				{
					Draw_CurrentImage(e);
					PointF[] array = new PointF[11];
					float num28 = Convert.ToInt32((float)base.Width * _currentPercentage / 130f);
					float num29 = Convert.ToInt32(base.Width / 2);
					float num30 = Convert.ToInt32(base.Height / 2);
					float num31 = 0f;
					bool flag = false;
					int num32 = 0;
					while ((double)num31 <= Math.PI * 2.0)
					{
						float num33 = num28 * 0.65f + num28 * (float)((!flag) ? 1 : 0);
						array[num32] = new PointF(num29 + (float)Math.Cos((double)num31 - Math.PI / 2.0) * num33, num30 + (float)Math.Sin((double)num31 - Math.PI / 2.0) * num33);
						num31 += (float)Math.PI / 5f;
						flag = !flag;
						num32++;
					}
					array[num32] = array[0];
					graphicsPath = new GraphicsPath();
					graphicsPath.AddPolygon(array);
					e.Graphics.SetClip(graphicsPath, CombineMode.Replace);
					Draw_NewImage(e);
					graphicsPath.Dispose();
				}
				else if (TransitionType == TransitionTypes.Heart)
				{
					Draw_CurrentImage(e);
					int num9 = Convert.ToInt32((float)base.Height * _currentPercentage / 100f);
					int num12 = Convert.ToInt32(base.Width / 2);
					int num13 = Convert.ToInt32(base.Height * 3 / 5);
					int num34 = (int)Math.Sqrt(num9 * num9 * 2);
					int num35 = (num34 - num9) / 2;
					graphicsPath = new GraphicsPath();
					graphicsPath.AddEllipse(new Rectangle(num12 - num34 + num35, num13 - num34 + num35, num34, num34));
					e.Graphics.SetClip(graphicsPath, CombineMode.Replace);
					graphicsPath.AddEllipse(new Rectangle(num12 - num35, num13 - num34 + num35, num34, num34));
					e.Graphics.SetClip(graphicsPath, CombineMode.Replace);
					graphicsPath.AddPolygon(new Point[4]
					{
						new Point(num12, num13 - num9),
						new Point(num12 - num9, num13),
						new Point(num12, num13 + num9),
						new Point(num12 + num9, num13)
					});
					e.Graphics.SetClip(graphicsPath, CombineMode.Union);
					Draw_NewImage(e);
					graphicsPath.Dispose();
				}
				else if (TransitionType == TransitionTypes.Stretch_H)
				{
					Draw_CurrentImage(e);
					if (TransitBackPictureAction == BackPicturesTransition.None)
					{
						int x = Convert.ToInt32(((float)_newBackgroundPicture.Width - (float)_newBackgroundPicture.Width * _currentPercentage / 100f) / 2f);
						int num14 = 0;
						int width3 = Convert.ToInt32((float)_newBackgroundPicture.Width * _currentPercentage / 100f);
						int num15 = _newBackgroundPicture.Height;
						Rectangle destRect4 = new Rectangle(x, num14, width3, num15);
						_imageWorkSpace = new Bitmap(_newBackgroundPicture.Width, _newBackgroundPicture.Height);
						wsg = Graphics.FromImage(_imageWorkSpace);
						wsg.Clear(Color.Transparent);
						wsg.DrawImage(_newBackgroundPicture, destRect4, destRect4, GraphicsUnit.Pixel);
						wsg.DrawImage(_newTextImage, destRect4, 0, 0, _newBackgroundPicture.Width, _newBackgroundPicture.Height, GraphicsUnit.Pixel);
						DrawImageToOutput(e, _imageWorkSpace, base.ClientRectangle, new Rectangle(0, 0, _imageWorkSpace.Width, _imageWorkSpace.Height), DrawAlert: true, null, PanelType.Current);
						wsg.Dispose();
						_imageWorkSpace.Dispose();
					}
					else
					{
						DrawImageToOutput(e, _newCombinedImage, new Rectangle(Convert.ToInt32(((float)base.Width - (float)base.Width * _currentPercentage / 100f) / 2f), 0, Convert.ToInt32((float)base.Width * _currentPercentage / 100f), base.ClientRectangle.Height), new Rectangle(0, 0, _currentCombinedImage.Width, _currentCombinedImage.Height), DrawAlert: true, null);
					}
				}
				else if (TransitionType == TransitionTypes.Stretch_V)
				{
					Draw_CurrentImage(e);
					if (TransitBackPictureAction == BackPicturesTransition.None)
					{
						int x = 0;
						int num14 = Convert.ToInt32(((float)_newBackgroundPicture.Height - (float)_newBackgroundPicture.Height * _currentPercentage / 100f) / 2f);
						int width3 = _newBackgroundPicture.Width;
						int num15 = Convert.ToInt32((float)_newBackgroundPicture.Height * _currentPercentage / 100f);
						Rectangle destRect4 = new Rectangle(x, num14, width3, num15);
						
						_imageWorkSpace = new Bitmap(_newBackgroundPicture.Width, _newBackgroundPicture.Height);
						wsg = Graphics.FromImage(_imageWorkSpace);
						wsg.Clear(Color.Transparent);
						wsg.DrawImage(_newBackgroundPicture, destRect4, destRect4, GraphicsUnit.Pixel);
						wsg.DrawImage(_newTextImage, destRect4, 0, 0, _newBackgroundPicture.Width, _newBackgroundPicture.Height, GraphicsUnit.Pixel);
						DrawImageToOutput(e, _imageWorkSpace, base.ClientRectangle, new Rectangle(0, 0, _imageWorkSpace.Width, _imageWorkSpace.Height), DrawAlert: true, null, PanelType.Current);
						wsg.Dispose();
						_imageWorkSpace.Dispose();
					}
					else
					{
						DrawImageToOutput(e, _newCombinedImage, new Rectangle(0, Convert.ToInt32(((float)base.Height - (float)base.Height * _currentPercentage / 100f) / 2f), base.ClientRectangle.Width, Convert.ToInt32((float)base.Height * _currentPercentage / 100f)), new Rectangle(0, 0, _currentCombinedImage.Width, _currentCombinedImage.Height), DrawAlert: true, null);
					}
				}
				else if (TransitionType == TransitionTypes.Away)
				{
					if (TransitBackPictureAction == BackPicturesTransition.None)
					{
						DrawImageToOutput(e, _newCombinedImage, base.ClientRectangle, new Rectangle(0, 0, _newBackgroundPicture.Width, _newBackgroundPicture.Height), DrawAlert: false, null);
						float dx = 0f;
						float dy = 0f;
						float num16 = 1f;
						// Reusing cached ImageAttributes
						// Reusing cached ColorMatrix
						_cachedColorMatrix.Matrix33 = 0.003921569f * (255f * (1f - _currentPercentage / 100f));
						_cachedImageAttributes.SetColorMatrix(_cachedColorMatrix);
						dx = base.Width / 2;
						dy = base.Height;
						num16 = 1f - 1f * _currentPercentage / 100f;
						if (num16 == 0f)
						{
							num16 = 0.01f;
						}
						Matrix matrix = new Matrix(num16, 0f, 0f, num16, dx, (float)base.Height * (1f - num16));
						e.Graphics.Transform = matrix;
						DrawImageToOutput(e, _currentTextImage, new Rectangle(-base.Width / 2, 0, base.Width, base.Height), new Rectangle(0, 0, _newCombinedImage.Width, _newCombinedImage.Height), DrawAlert: false, _cachedImageAttributes, PanelType.CurrentWithTrans);
						_cachedImageAttributes.Dispose();
						matrix.Dispose();
					}
					else
					{
						Draw_NewImage(e);
						float num16 = 1f - 1f * _currentPercentage / 100f;
						if (num16 == 0f)
						{
							num16 = 0.01f;
						}
						float dx = base.Width / 2;
						float dy = base.Height;
						Matrix matrix2 = new Matrix(num16, 0f, 0f, num16, dx, dy * (1f - num16));
						num16 = 1f - 1f * _currentPercentage / 100f;
						if (num16 == 0f)
						{
							num16 = 0.01f;
						}
						dx = base.Width / 2;
						dy = base.Height;
						matrix2 = new Matrix(num16, 0f, 0f, num16, dx, dy * (1f - num16));
						e.Graphics.Transform = matrix2;
						if (TransitBackPictureAction == BackPicturesTransition.NewOnly)
						{
							DrawImageToOutput(e, _currentTextImage, new Rectangle(-base.Width / 2, -1 - base.Height / 20, base.Width, base.Height), new Rectangle(0, 0, _newCombinedImage.Width, _newCombinedImage.Height), DrawAlert: true, null, PanelType.CurrentWithTrans);
						}
						else
						{
							DrawImageToOutput(e, _currentCombinedImage, new Rectangle(-base.Width / 2, -1 - base.Height / 20, base.Width, base.Height), new Rectangle(0, 0, _newCombinedImage.Width, _newCombinedImage.Height), DrawAlert: true, null);
						}
						matrix2.Dispose();
					}
				}
				else if (TransitionType == TransitionTypes.Zoom_Away)
				{
					if (TransitBackPictureAction == BackPicturesTransition.None)
					{
						DrawImageToOutput(e, _newBackgroundPicture, base.ClientRectangle, new Rectangle(0, 0, _newBackgroundPicture.Width, _newBackgroundPicture.Height), DrawAlert: false, null);
						float dx = 0f;
						float dy = 0f;
						// Reusing cached ImageAttributes
						// Reusing cached ColorMatrix
						_cachedColorMatrix.Matrix33 = 0.003921569f * (255f * (1f - _currentPercentage / 100f));
						_cachedImageAttributes.SetColorMatrix(_cachedColorMatrix);
						dx = base.Width / 2;
						dy = base.Height;
						float num16 = 1f;
						num16 = 1f - 1f * _currentPercentage / 100f;
						if (num16 == 0f)
						{
							num16 = 0.01f;
						}
						Matrix matrix = new Matrix(num16, 0f, 0f, num16, dx, (float)base.Height * (1f - num16));
						e.Graphics.Transform = matrix;
						DrawImageToOutput(e, _currentTextImage, new Rectangle(-base.Width / 2, 0, base.Width, base.Height), new Rectangle(0, 0, _newCombinedImage.Width, _newCombinedImage.Height), DrawAlert: false, _cachedImageAttributes, PanelType.Current);
						_cachedImageAttributes.Dispose();
						dx = base.Width / 2;
						dy = base.Height / 3;
						num16 = 0.2f + 0.8f * _currentPercentage / 100f;
						if (num16 == 0f)
						{
							num16 = 0.01f;
						}
						matrix = new Matrix(num16, 0f, 0f, num16, dx, dy);
						e.Graphics.Transform = matrix;
						DrawImageToOutput(e, _newTextImage, new Rectangle(-base.Width / 2, -1 - base.Height / 3, base.Width, base.Height), new Rectangle(0, 0, _newCombinedImage.Width, _newCombinedImage.Height), DrawAlert: true, null, PanelType.New);
						matrix.Dispose();
					}
					else
					{
						DrawDefaultBackPattern(e);
						float dx = base.Width / 2;
						float dy = base.Height / 3;
						float num16 = 0.2f + 0.8f * _currentPercentage / 100f;
						if (num16 == 0f)
						{
							num16 = 0.01f;
						}
						Matrix matrix2 = new Matrix(num16, 0f, 0f, num16, dx, dy);
						e.Graphics.Transform = matrix2;
						if (TransitBackPictureAction == BackPicturesTransition.CurrentOnly)
						{
							DrawImageToOutput(e, _newTextImage, new Rectangle(-base.Width / 2, -1 - base.Height / 3, base.Width, base.Height), new Rectangle(0, 0, _newCombinedImage.Width, _newCombinedImage.Height), DrawAlert: false, null);
						}
						else
						{
							DrawImageToOutput(e, _newCombinedImage, new Rectangle(-base.Width / 2, -1 - base.Height / 3, base.Width, base.Height), new Rectangle(0, 0, _newCombinedImage.Width, _newCombinedImage.Height), DrawAlert: false, null);
						}
						num16 = 1f - 1f * _currentPercentage / 100f;
						if (num16 == 0f)
						{
							num16 = 0.01f;
						}
						dx = base.Width / 2;
						dy = base.Height;
						matrix2 = new Matrix(num16, 0f, 0f, num16, dx, dy * (1f - num16));
						e.Graphics.Transform = matrix2;
						if (TransitBackPictureAction == BackPicturesTransition.NewOnly)
						{
							DrawImageToOutput(e, _currentTextImage, new Rectangle(-base.Width / 2, -1 - base.Height / 20, base.Width, base.Height), new Rectangle(0, 0, _newCombinedImage.Width, _newCombinedImage.Height), DrawAlert: true, null, PanelType.Current);
						}
						else
						{
							DrawImageToOutput(e, _currentCombinedImage, new Rectangle(-base.Width / 2, -1 - base.Height / 20, base.Width, base.Height), new Rectangle(0, 0, _newCombinedImage.Width, _newCombinedImage.Height), DrawAlert: true, null);
						}
						matrix2.Dispose();
					}
				}
				else if (TransitionType == TransitionTypes.CircularWipe)
				{
					Draw_CurrentImage(e);
					float angle = 360f * _currentPercentage / 100f;
					float num36 = (float)base.Width * 1.414f;
					graphicsPath = new GraphicsPath();
					graphicsPath.AddPie(((float)base.Width - num36) / 2f, ((float)base.Height - num36) / 2f, num36, num36, 180f, angle);
					e.Graphics.SetClip(graphicsPath, CombineMode.Replace);
					Draw_NewImage(e);
					graphicsPath.Dispose();
				}
				else if (TransitionType == TransitionTypes.Wedge)
				{
					Draw_CurrentImage(e);
					float angle = 360f * _currentPercentage / 100f;
					float num36 = (float)base.Width * 1.414f;
					graphicsPath = new GraphicsPath();
					graphicsPath.AddPie(((float)base.Width - num36) / 2f, ((float)base.Height - num36) / 2f, num36, num36, 270f - angle / 2f, angle);
					e.Graphics.SetClip(graphicsPath, CombineMode.Replace);
					Draw_NewImage(e);
					graphicsPath.Dispose();
				}
				else if (TransitionType == TransitionTypes.BowTie)
				{
					Draw_CurrentImage(e);
					float angle = 180f * _currentPercentage / 100f;
					float num36 = (float)base.Width * 1.414f;
					graphicsPath = new GraphicsPath();
					graphicsPath.AddPie(((float)base.Width - num36) / 2f, ((float)base.Height - num36) / 2f, num36, num36, 270f - angle / 2f, angle);
					graphicsPath.AddPie(((float)base.Width - num36) / 2f, ((float)base.Height - num36) / 2f, num36, num36, 90f - angle / 2f, angle);
					e.Graphics.SetClip(graphicsPath, CombineMode.Replace);
					Draw_NewImage(e);
					graphicsPath.Dispose();
				}
				else if (TransitionType == TransitionTypes.FanUp)
				{
					Draw_CurrentImage(e);
					float angle = 150f * _currentPercentage / 100f;
					float num36 = (float)base.Width * 1.7f * 1.414f;
					graphicsPath = new GraphicsPath();
					graphicsPath.AddPie(((float)base.Width - num36) / 2f, (float)base.Height - num36 * 0.9f, num36, num36, 90f - angle / 2f, angle);
					e.Graphics.SetClip(graphicsPath, CombineMode.Replace);
					Draw_NewImage(e);
					graphicsPath.Dispose();
				}
				else if (TransitionType == TransitionTypes.WindMill)
				{
					Draw_CurrentImage(e);
					float angle = 90f * _currentPercentage / 100f;
					float num36 = (float)base.Width * 1.414f;
					graphicsPath = new GraphicsPath();
					graphicsPath.AddPie(((float)base.Width - num36) / 2f, ((float)base.Height - num36) / 2f, num36, num36, 270f, angle);
					graphicsPath.AddPie(((float)base.Width - num36) / 2f, ((float)base.Height - num36) / 2f, num36, num36, 0f, angle);
					graphicsPath.AddPie(((float)base.Width - num36) / 2f, ((float)base.Height - num36) / 2f, num36, num36, 90f, angle);
					graphicsPath.AddPie(((float)base.Width - num36) / 2f, ((float)base.Height - num36) / 2f, num36, num36, 180f, angle);
					e.Graphics.SetClip(graphicsPath, CombineMode.Replace);
					Draw_NewImage(e);
					graphicsPath.Dispose();
				}
				else if (TransitionType == TransitionTypes.RandomBars)
				{
					Draw_CurrentImage(e);
					graphicsPath = new GraphicsPath();
					// Using static _random field instead
					int num37 = 0;
					int num38 = base.Height / 2 + 1;
					int num39 = (int)((float)num38 * _currentPercentage / 100f) - TotalBitsArraySet;
					lock (_bitsArrayLock)
					{
					for (int k = 0; k < num39 - 1; k++)
					{
						num37 = _random.Next(num38);
						while (BitsArraySet.Contains(num37))
						{
							num37++;
							if (num37 > base.Height)
							{
								num37 = 0;
							}
						}
						BitsArraySet.Add(num37);
					}
					TotalBitsArraySet = 0;
					}
					int width5 = base.Width;
					for (int k = 0; k <= num38; k++)
					{
						if (BitsArraySet.Contains(k))
						{
							TotalBitsArraySet++;
							graphicsPath.AddRectangle(new Rectangle(0, k * 2, width5, 2));
						}
					}
					e.Graphics.SetClip(graphicsPath, CombineMode.Replace);
					Draw_NewImage(e);
					graphicsPath.Dispose();
				}
				else if (TransitionType == TransitionTypes.Dissolve)
				{
					Draw_CurrentImage(e);
					graphicsPath = new GraphicsPath();
					// Using static _random field instead
					int num40 = 0;
					int num41 = (int)((float)DissolveTotal * _currentPercentage / 100f) - TotalBitsArraySet;
					lock (_bitsArrayLock)
				{
				for (int k = 0; k < num41 - 1; k++)
					{
						num40 = _random.Next(DissolveTotal);
						BitsArraySet.Add(num40);
					}
					TotalBitsArraySet = 0;
					}
					int num42 = 0;
					int num43 = 0;
					for (int k = 0; k < DissolveTotal; k++)
					{
						if (BitsArraySet.Contains(k))
						{
							num42 = k / DissolveCountX;
							num43 = k - num42 * DissolveCountX - 1;
							TotalBitsArraySet++;
							graphicsPath.AddRectangle(new Rectangle(num43 * DissolveSize, num42 * DissolveSize, DissolveSize, DissolveSize));
						}
					}
					e.Graphics.SetClip(graphicsPath, CombineMode.Replace);
					Draw_NewImage(e);
					graphicsPath.Dispose();
				}
				else if (TransitionType == TransitionTypes.Mosaic)
				{
					Draw_CurrentImage(e);
					graphicsPath = new GraphicsPath();
					// Using static _random field instead
					int num40 = 0;
					int num41 = (int)((float)DissolveTotal * _currentPercentage / 100f) - TotalBitsArraySet;
					lock (_bitsArrayLock)
				{
				for (int k = 0; k < num41 - 1; k++)
					{
						num40 = _random.Next(DissolveTotal);
						while (BitsArraySet.Contains(num40))
						{
							num40++;
							if (num40 > DissolveTotal)
							{
								num40 = 0;
							}
						}
						BitsArraySet.Add(num40);
					}
					TotalBitsArraySet = 0;
					}
					int num42 = 0;
					int num43 = 0;
					for (int k = 0; k < DissolveTotal; k++)
					{
						if (BitsArraySet.Contains(k))
						{
							num42 = k / DissolveCountX;
							num43 = k - num42 * DissolveCountX - 1;
							TotalBitsArraySet++;
							graphicsPath.AddRectangle(new Rectangle(num43 * DissolveSize, num42 * DissolveSize, DissolveSize, DissolveSize));
						}
					}
					e.Graphics.SetClip(graphicsPath, CombineMode.Replace);
					Draw_NewImage(e);
					graphicsPath.Dispose();
				}
				else if (TransitionType == TransitionTypes.Zoom_In)
				{
					Draw_CurrentImage(e);
					float num16 = 0.2f + 0.8f * _currentPercentage / 100f;
					if (num16 == 0f)
					{
						num16 = 0.01f;
					}
					if (TransitBackPictureAction == BackPicturesTransition.None)
					{
						num16 = (_currentPercentage + 20f) / 100f;
						if (num16 == 0f)
						{
							num16 = 0.01f;
						}
						else if (num16 > 1f)
						{
							num16 = 1f;
						}
						float dx = _newBackgroundPicture.Width / 2;
						float dy = _newBackgroundPicture.Height / 2;
						
						_imageWorkSpace = new Bitmap(_newBackgroundPicture.Width, _newBackgroundPicture.Height);
						wsg = Graphics.FromImage(_imageWorkSpace);
						wsg.Clear(Color.Transparent);
						Rectangle destRect4 = new Rectangle(-_newBackgroundPicture.Width / 2, -1 - _newBackgroundPicture.Height / 2, _newBackgroundPicture.Width, _newBackgroundPicture.Height);
						Rectangle rectangle = new Rectangle(destRect4.Width / 2 - (int)(num16 * (float)destRect4.Width / 2f), destRect4.Height / 2 - (int)(num16 * (float)destRect4.Height / 2f), (int)(num16 * (float)destRect4.Width), (int)(num16 * (float)destRect4.Height));
						wsg.DrawImage(_newBackgroundPicture, rectangle, rectangle, GraphicsUnit.Pixel);
						Matrix matrix = new Matrix(num16, 0f, 0f, num16, dx, dy);
						wsg.Transform = matrix;
						wsg.DrawImage(_newTextImage, destRect4, 0, 0, _newBackgroundPicture.Width, _newBackgroundPicture.Height, GraphicsUnit.Pixel);
						DrawImageToOutput(e, _imageWorkSpace, base.ClientRectangle, new Rectangle(0, 0, _imageWorkSpace.Width, _imageWorkSpace.Height), DrawAlert: true, null, PanelType.Current);
						matrix.Dispose();
						wsg.Dispose();
						_imageWorkSpace.Dispose();
					}
					else
					{
						float dx = base.Width / 2;
						float dy = base.Height / 2;
						Matrix matrix2 = new Matrix(num16, 0f, 0f, num16, dx, dy);
						e.Graphics.Transform = matrix2;
						DrawImageToOutput(e, _newCombinedImage, new Rectangle(-base.Width / 2, -1 - base.Height / 2, base.Width, base.Height), new Rectangle(0, 0, _newCombinedImage.Width, _newCombinedImage.Height), DrawAlert: true, null);
						matrix2.Dispose();
					}
				}
				else if (TransitionType == TransitionTypes.Zoom_Out)
				{
					float num16 = 2f - _currentPercentage / 100f;
					if (num16 == 0f)
					{
						num16 = 0.01f;
					}
					if (TransitBackPictureAction == BackPicturesTransition.None)
					{
						DrawImageToOutput(e, _newBackgroundPicture, base.ClientRectangle, new Rectangle(0, 0, _newBackgroundPicture.Width, _newBackgroundPicture.Height), DrawAlert: false, null);
						float dx = base.Width / 2;
						float dy = base.Height / 2;
						Matrix matrix = new Matrix(num16, 0f, 0f, num16, dx, dy * num16);
						e.Graphics.Transform = matrix;
						DrawImageToOutput(e, _newTextImage, new Rectangle(-base.Width / 2, -1 - base.Height / 2, base.Width, base.Height), new Rectangle(0, 0, _newCombinedImage.Width, _newCombinedImage.Height), DrawAlert: true, null, PanelType.Current);
						matrix.Dispose();
					}
					else
					{
						float dx = base.Width / 2;
						float dy = base.Height / 2;
						Matrix matrix2 = new Matrix(num16, 0f, 0f, num16, dx, dy * num16);
						e.Graphics.Transform = matrix2;
						DrawImageToOutput(e, _newCombinedImage, new Rectangle(-base.Width / 2, -1 - base.Height / 2, base.Width, base.Height), new Rectangle(0, 0, _newCombinedImage.Width, _newCombinedImage.Height), DrawAlert: true, null);
						matrix2.Dispose();
					}
				}
				if (AlertOverallCurrentPercentage < 100f)
				{
					AddAlertTextToImage(e);
				}
				if (RefOverallCurrentPercentage < 100f)
				{
					AddRefTextToImage(e);
				}
			}
			else
			{
				if (t != null)
				{
					t.Dispose();
				}
				t = null;
				if (ItemFirstShowing)
				{
					RefGo();
					ItemFirstShowing = false;
				}
				_transitionType = _previousTransitionType;
				if (AlertRunning || RefRunning)
				{
					if (AlertOverallCurrentPercentage >= 100f)
					{
						AlertRunning = false;
						if (ta != null)
						{
							ta.Dispose();
						}
						ta = null;
					}
					if (RefOverallCurrentPercentage >= 100f)
					{
						RefRunning = false;
						if (tr != null)
						{
							tr.Dispose();
						}
						tr = null;
					}
				}
				ReLoadNewImage(e);
				AddAlertTextToImage(e);
				AddRefTextToImage(e);
			}
			base.OnPaint(e);
		}

		private void ReLoadNewImage(PaintEventArgs e)
		{
			DrawImageToOutputBitBlt(e, _newCombinedImage, base.ClientRectangle, new Rectangle(0, 0, _newCombinedImage.Width, _newCombinedImage.Height), DrawAlert: false, null, PanelType.NoAction);
		}

		private void Draw_CurrentImage(PaintEventArgs e)
		{
			if (_currentPercentage < 100f)
			{
				DrawImageToOutput(e, _currentCombinedImage, base.ClientRectangle, new Rectangle(0, 0, _currentCombinedImage.Width, _currentCombinedImage.Height), DrawAlert: false, null);
			}
			AddAlertTextToImage(e);
			AddRefTextToImage(e);
		}

		private void Draw_NewImage(PaintEventArgs e)
		{
			DrawImageToOutput(e, _newCombinedImage, base.ClientRectangle, new Rectangle(0, 0, _newCombinedImage.Width, _newCombinedImage.Height), DrawAlert: true, null);
		}

		private void Draw_NewImageBitBlt(PaintEventArgs e)
		{
			DrawImageToOutputBitBlt(e, _newCombinedImage, base.ClientRectangle, new Rectangle(0, 0, _newCombinedImage.Width, _newCombinedImage.Height), DrawAlert: true, null, PanelType.NoAction);
		}

		private void DrawImageToOutput(PaintEventArgs e, Image InImage, Rectangle DestinationRectangle, Rectangle SourceRectangle, bool DrawAlert, ImageAttributes InImageAttributes)
		{
			DrawImageToOutput(e, InImage, DestinationRectangle, SourceRectangle, DrawAlert, InImageAttributes, PanelType.NoAction);
		}

		private void DrawImageToOutput(PaintEventArgs e, Image InImage, Rectangle DestinationRectangle, Rectangle SourceRectangle, bool DrawAlert, ImageAttributes InImageAttributes, PanelType InPanelType)
		{
			DrawImageToOutputNormal(e, InImage, DestinationRectangle, SourceRectangle, DrawAlert, InImageAttributes, InPanelType);
		}

		private void DrawImageToOutputNormal(PaintEventArgs e, Image InImage, Rectangle DestinationRectangle, Rectangle SourceRectangle, bool DrawAlert, ImageAttributes InImageAttributes, PanelType InPanelType)
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

		/// <summary>
		/// daniel v2.2 ????
		/// function pointer ????? ???? ???? ???
		/// </summary>
		/// <param name="e"></param>
		/// <param name="InImage"></param>
		/// <param name="DestinationRectangle"></param>
		/// <param name="SourceRectangle"></param>
		/// <param name="DrawAlert"></param>
		/// <param name="InImageAttributes"></param>
		/// <param name="InPanelType"></param>
		private void DrawImageToOutputBitBlt(PaintEventArgs e, Image InImage, Rectangle DestinationRectangle, Rectangle SourceRectangle, bool DrawAlert, ImageAttributes InImageAttributes, PanelType InPanelType)
		{
			IntPtr hdc = e.Graphics.GetHdc();
			//IntPtr intPtr = gf.CreateCompatibleDC(hdc);
			IntPtr intPtr = CreateCompatibleDC(hdc);
			//IntPtr intPtr2 = gf.CreateCompatibleBitmap(hdc, DestinationRectangle.Width, DestinationRectangle.Height);
			IntPtr intPtr2 = CreateCompatibleBitmap(hdc, DestinationRectangle.Width, DestinationRectangle.Height);
			//gf.SelectObject(intPtr, intPtr2);
			IntPtr intPtr3 = SelectObject(intPtr, intPtr2);

			Graphics graphics = Graphics.FromHdc(intPtr);
			if (InImageAttributes == null)
			{
				graphics.DrawImage(InImage, DestinationRectangle, SourceRectangle, GraphicsUnit.Pixel);
				switch (InPanelType)
				{
				case PanelType.Current:
					graphics.DrawImage(CurrentPanelImage, DestinationRectangle, new Rectangle(0, 0, SourceRectangle.Width, SourceRectangle.Height), GraphicsUnit.Pixel);
					break;
				case PanelType.CurrentWithTrans:
					graphics.DrawImage(CurrentPanelImage, DestinationRectangle, SourceRectangle, GraphicsUnit.Pixel);
					break;
				case PanelType.New:
					graphics.DrawImage(NewPanelImage, DestinationRectangle, new Rectangle(0, 0, SourceRectangle.Width, SourceRectangle.Height), GraphicsUnit.Pixel);
					break;
				case PanelType.NewWithTrans:
					graphics.DrawImage(NewPanelImage, DestinationRectangle, SourceRectangle, GraphicsUnit.Pixel);
					break;
				}
			}
			else
			{
				graphics.DrawImage(InImage, DestinationRectangle, SourceRectangle.Left, SourceRectangle.Top, SourceRectangle.Width, SourceRectangle.Height, GraphicsUnit.Pixel, InImageAttributes);
				switch (InPanelType)
				{
				case PanelType.Current:
					graphics.DrawImage(CurrentPanelImage, DestinationRectangle, 0, 0, SourceRectangle.Width, SourceRectangle.Height, GraphicsUnit.Pixel, InImageAttributes);
					break;
				case PanelType.CurrentWithTrans:
					graphics.DrawImage(CurrentPanelImage, DestinationRectangle, SourceRectangle.Left, SourceRectangle.Top, SourceRectangle.Width, SourceRectangle.Height, GraphicsUnit.Pixel, InImageAttributes);
					break;
				case PanelType.New:
					graphics.DrawImage(NewPanelImage, DestinationRectangle, 0, 0, SourceRectangle.Width, SourceRectangle.Height, GraphicsUnit.Pixel, InImageAttributes);
					break;
				case PanelType.NewWithTrans:
					graphics.DrawImage(NewPanelImage, DestinationRectangle, SourceRectangle.Left, SourceRectangle.Top, SourceRectangle.Width, SourceRectangle.Height, GraphicsUnit.Pixel, InImageAttributes);
					break;
				}
			}

			IntPtr hdc2 = graphics.GetHdc();
			BitBlt(hdc, 0, 0, DestinationRectangle.Width, DestinationRectangle.Height, hdc2, 0, 0, 13369376);
			//gf.BitBlt(hdc, 0, 0, DestinationRectangle.Width, DestinationRectangle.Height, hdc2, 0, 0, 13369376);
			graphics.ReleaseHdc(hdc2);

			//gf.DeleteDC(intPtr);
			DeleteDC(intPtr);
			//gf.DeleteObject(intPtr);
			DeleteObject(intPtr);
			//gf.DeleteObject(intPtr2);
			DeleteObject(intPtr2);

			DeleteObject(intPtr3);

			e.Graphics.ReleaseHdc(hdc);

			graphics.Dispose();

			// GC.Collect() removed - forced GC causes performance degradation
	}

		private void Draw_TranSlideIn(PaintEventArgs e, TransitionTypes InTransType)
		{
			_imageWorkSpace = new Bitmap(_newBackgroundPicture.Width, _newBackgroundPicture.Height);
			wsg = Graphics.FromImage(_imageWorkSpace);
			wsg.Clear(Color.Transparent);
			float num = 0f;
			float num2 = 0f;
			Matrix transform = new Matrix(1f, 0f, 0f, 1f, 0f, 1f);
			switch (InTransType)
			{
			case TransitionTypes.InLeft:
				num2 = (float)_newBackgroundPicture.Width * _currentPercentage / 100f - (float)_newBackgroundPicture.Width;
				transform = new Matrix(1f, 0f, 0f, 1f, num2, 0f);
				break;
			case TransitionTypes.InTop:
				num = (float)_newBackgroundPicture.Height * _currentPercentage / 100f - (float)_newBackgroundPicture.Height;
				transform = new Matrix(1f, 0f, 0f, 1f, 0f, num);
				break;
			case TransitionTypes.InRight:
				num2 = (float)_newBackgroundPicture.Width - (float)_newBackgroundPicture.Width * _currentPercentage / 100f;
				transform = new Matrix(1f, 0f, 0f, 1f, num2, 0f);
				break;
			case TransitionTypes.InBottom:
				num = (float)_newBackgroundPicture.Height - (float)_newBackgroundPicture.Height * _currentPercentage / 100f;
				transform = new Matrix(1f, 0f, 0f, 1f, 0f, num);
				break;
			case TransitionTypes.InTopLeft:
				num = (float)_newBackgroundPicture.Height * _currentPercentage / 100f - (float)_newBackgroundPicture.Height;
				num2 = (float)_newBackgroundPicture.Width * _currentPercentage / 100f - (float)_newBackgroundPicture.Width;
				transform = new Matrix(1f, 0f, 0f, 1f, num2, num);
				break;
			case TransitionTypes.InTopRight:
				num = (float)_newBackgroundPicture.Height * _currentPercentage / 100f - (float)_newBackgroundPicture.Height;
				num2 = (float)_newBackgroundPicture.Width - (float)_newBackgroundPicture.Width * _currentPercentage / 100f;
				transform = new Matrix(1f, 0f, 0f, 1f, num2, num);
				break;
			case TransitionTypes.InBottomLeft:
				num = (float)_newBackgroundPicture.Height - (float)_newBackgroundPicture.Height * _currentPercentage / 100f;
				num2 = (float)_newBackgroundPicture.Width * _currentPercentage / 100f - (float)_newBackgroundPicture.Width;
				transform = new Matrix(1f, 0f, 0f, 1f, num2, num);
				break;
			case TransitionTypes.InBottomRight:
				num = (float)_newBackgroundPicture.Height - (float)_newBackgroundPicture.Height * _currentPercentage / 100f;
				num2 = (float)_newBackgroundPicture.Width - (float)_newBackgroundPicture.Width * _currentPercentage / 100f;
				transform = new Matrix(1f, 0f, 0f, 1f, num2, num);
				break;
			}
			if (TransitBackPictureAction == BackPicturesTransition.None)
			{
				int x = (num2 >= 0f) ? ((int)num2) : 0;
				int y = (num >= 0f) ? ((int)num) : 0;
				int width = (num2 >= 0f) ? (_newBackgroundPicture.Width - (int)num2) : (_newBackgroundPicture.Width + (int)num2);
				int height = (num >= 0f) ? (_newBackgroundPicture.Height - (int)num) : (_newBackgroundPicture.Height + (int)num);
				Rectangle rectangle = new Rectangle(x, y, width, height);
				wsg.DrawImage(_currentCombinedImage, new Rectangle(0, 0, _newBackgroundPicture.Width, _newBackgroundPicture.Height), 0, 0, _newBackgroundPicture.Width, _newBackgroundPicture.Height, GraphicsUnit.Pixel);
				wsg.DrawImage(_newBackgroundPicture, rectangle, rectangle, GraphicsUnit.Pixel);
				wsg.DrawImage(_newTextImage, new Rectangle((int)num2, (int)num, _newBackgroundPicture.Width, _newBackgroundPicture.Height), 0, 0, _newBackgroundPicture.Width, _newBackgroundPicture.Height, GraphicsUnit.Pixel);
				DrawImageToOutput(e, _imageWorkSpace, base.ClientRectangle, new Rectangle(0, 0, _imageWorkSpace.Width, _imageWorkSpace.Height), DrawAlert: true, null, PanelType.New);
			}
			else
			{
				Draw_CurrentImage(e);
				e.Graphics.Transform = transform;
				Draw_NewImage(e);
			}
			wsg.Dispose();
			_imageWorkSpace.Dispose();
		}

		private void Draw_TranSlideOut(PaintEventArgs e, TransitionTypes InTransType)
		{
			_imageWorkSpace = new Bitmap(_newBackgroundPicture.Width, _newBackgroundPicture.Height);
			wsg = Graphics.FromImage(_imageWorkSpace);
			wsg.Clear(Color.Transparent);
			float num = 0f;
			float num2 = 0f;
			Matrix transform = new Matrix(1f, 0f, 0f, 1f, 0f, 1f);
			switch (InTransType)
			{
			case TransitionTypes.OutLeft:
				num2 = (float)(-_newBackgroundPicture.Width) * _currentPercentage / 100f;
				transform = new Matrix(1f, 0f, 0f, 1f, num2, 0f);
				break;
			case TransitionTypes.OutTop:
				num = (float)(-_newBackgroundPicture.Height) * _currentPercentage / 100f;
				transform = new Matrix(1f, 0f, 0f, 1f, 0f, num);
				break;
			case TransitionTypes.OutRight:
				num2 = (float)_newBackgroundPicture.Width * _currentPercentage / 100f;
				transform = new Matrix(1f, 0f, 0f, 1f, num2, 0f);
				break;
			case TransitionTypes.OutBottom:
				num = (float)_newBackgroundPicture.Height * _currentPercentage / 100f;
				transform = new Matrix(1f, 0f, 0f, 1f, 0f, num);
				break;
			case TransitionTypes.OutTopLeft:
				num = (float)(-_newBackgroundPicture.Height) * _currentPercentage / 100f;
				num2 = (float)(-_newBackgroundPicture.Width) * _currentPercentage / 100f;
				transform = new Matrix(1f, 0f, 0f, 1f, num2, num);
				break;
			case TransitionTypes.OutTopRight:
				num = (float)(-_newBackgroundPicture.Height) * _currentPercentage / 100f;
				num2 = (float)_newBackgroundPicture.Width * _currentPercentage / 100f;
				transform = new Matrix(1f, 0f, 0f, 1f, num2, num);
				break;
			case TransitionTypes.OutBottomLeft:
				num = (float)_newBackgroundPicture.Height * _currentPercentage / 100f;
				num2 = (float)(-_newBackgroundPicture.Width) * _currentPercentage / 100f;
				transform = new Matrix(1f, 0f, 0f, 1f, num2, num);
				break;
			case TransitionTypes.OutBottomRight:
				num = (float)_newBackgroundPicture.Height * _currentPercentage / 100f;
				num2 = (float)_newBackgroundPicture.Width * _currentPercentage / 100f;
				transform = new Matrix(1f, 0f, 0f, 1f, num2, num);
				break;
			}
			if (TransitBackPictureAction == BackPicturesTransition.None)
			{
				int x = (num2 >= 0f) ? ((int)num2) : 0;
				int y = (num >= 0f) ? ((int)num) : 0;
				int width = (num2 >= 0f) ? (_newBackgroundPicture.Width - (int)num2) : (_newBackgroundPicture.Width + (int)num2);
				int height = (num >= 0f) ? (_newBackgroundPicture.Height - (int)num) : (_newBackgroundPicture.Height + (int)num);
				Rectangle rectangle = new Rectangle(x, y, width, height);
				wsg.DrawImage(NewCombinedImage, new Rectangle(0, 0, _newBackgroundPicture.Width, _newBackgroundPicture.Height), 0, 0, _newBackgroundPicture.Width, _newBackgroundPicture.Height, GraphicsUnit.Pixel);
				wsg.DrawImage(_newBackgroundPicture, rectangle, rectangle, GraphicsUnit.Pixel);
				wsg.DrawImage(_currentTextImage, new Rectangle((int)num2, (int)num, _newBackgroundPicture.Width, _newBackgroundPicture.Height), 0, 0, _newBackgroundPicture.Width, _newBackgroundPicture.Height, GraphicsUnit.Pixel);
				DrawImageToOutput(e, _imageWorkSpace, base.ClientRectangle, new Rectangle(0, 0, _imageWorkSpace.Width, _imageWorkSpace.Height), DrawAlert: true, null, PanelType.Current);
			}
			else
			{
				Draw_NewImage(e);
				e.Graphics.Transform = transform;
				Draw_CurrentImage(e);
			}
			wsg.Dispose();
			_imageWorkSpace.Dispose();
		}

		private void Draw_TranSlideReveal(PaintEventArgs e, TransitionTypes InTransType)
		{
			_imageWorkSpace = new Bitmap(NewCombinedImage.Width, NewCombinedImage.Height);
			wsg = Graphics.FromImage(_imageWorkSpace);
			wsg.DrawImage(NewCombinedImage, new Rectangle(0, 0, NewCombinedImage.Width, NewCombinedImage.Height), 0, 0, NewCombinedImage.Width, NewCombinedImage.Height, GraphicsUnit.Pixel);
			int num = 0;
			int num2 = 0;
			int width = NewCombinedImage.Width;
			int height = NewCombinedImage.Height;
			float num3 = 1f * _currentPercentage / 100f;
			switch (InTransType)
			{
			case TransitionTypes.RevealLeftRight:
				num = (int)((float)_currentCombinedImage.Width * num3);
				width = _currentCombinedImage.Width - num;
				break;
			case TransitionTypes.RevealTopDown:
				num2 = (int)((float)_currentCombinedImage.Height * num3);
				height = _currentCombinedImage.Height - num2;
				break;
			case TransitionTypes.RevealRightLeft:
				width = (int)((float)_currentCombinedImage.Width * (1f - num3));
				break;
			case TransitionTypes.RevealDownUp:
				height = (int)((float)_currentCombinedImage.Height * (1f - num3));
				break;
			}
			Rectangle rectangle = new Rectangle(num, num2, width, height);
			wsg.DrawImage(_currentCombinedImage, rectangle, rectangle, GraphicsUnit.Pixel);
			DrawImageToOutput(e, _imageWorkSpace, base.ClientRectangle, new Rectangle(0, 0, _imageWorkSpace.Width, _imageWorkSpace.Height), DrawAlert: true, null);
			wsg.Dispose();
			_imageWorkSpace.Dispose();
		}

		private void AlertTick(object state)
		{
			if (!gf.ParentalAlertLive & !gf.MessageAlertLive)
			{
				StopAlert();
			}
			TimeSpan timeSpan = DateTime.Now.Subtract(AlertStartTime);
			AlertOverallCurrentPercentage = Convert.ToSingle(100.0 / _alertTransitionTime.TotalSeconds * timeSpan.TotalSeconds);
			if (AlertOverallCurrentPercentage > 100f)
			{
				AlertOverallCurrentPercentage = 100f;
			}
			TimeSpan timeSpan2 = DateTime.Now.Subtract(AlertMessageStartTime);
			AlertMessagePercentage = Convert.ToSingle(100.0 / _alertMessageTransitionTime.TotalSeconds * timeSpan2.TotalSeconds);
			if (AlertMessagePercentage > 100f)
			{
				AlertMessagePercentage = 100f;
			}
			Invalidate();
		}

		public void StartAlert(SongSettings InItem, string InString, int InDuration, Font InFont, bool InUseScroll, bool InUseFlash, bool InTransparent, bool InUseShadow, bool InUseOutline, Color InTextColour, Color InBackColour, int InAlignment, int InVerticalAlignment, double BottomBorderFactor)
		{
			if (_newBackgroundPicture != null)
			{
				int num = _newBackgroundPicture.Width;
				int num2 = (int)(gf.MinBottomBorderFactor * (double)_newBackgroundPicture.Height * 1.2);
				if (num < 1)
				{
					num = 1;
				}
				if (num2 < 1)
				{
					num2 = 1;
				}
				AlertAlign = InAlignment;
				AlertScroll = InUseScroll;
				AlertFlash = InUseFlash;
				AlertTransparent = InTransparent;

				AlertimageBackground = new Bitmap(num, num2 + 6);
				alertg = Graphics.FromImage(AlertimageBackground);
				alertg.Clear(Color.Transparent);
				alertg.Dispose();
				AlertimageBackground.Dispose();

				AlertimageBackground_Inverse = new Bitmap(num, num2 + 6);
				alertg_Inverse = Graphics.FromImage(AlertimageBackground_Inverse);
				alertg_Inverse.Clear(Color.Transparent);
				AlertBorder = num / 20;
				InFont = new Font(InFont.Name, InFont.Size - 1f, InFont.Style);
				int num3 = (int)((double)gf.ReduceFontToFit(alertg, InString, ref InFont, num - AlertBorder * 2, num2 - 2) * 1.1);
				AlertMessageBackGroundRect = new Rectangle(0, 0, (int)alertg.MeasureString(InString, InFont).Width, num3);
				num2 = num3 + 4;
				alertg_Inverse.Dispose();
				AlertimageBackground_Inverse.Dispose();

				AlertimageMessage = new Bitmap(AlertMessageBackGroundRect.Width, num2);
				alertMessageg = Graphics.FromImage(AlertimageMessage);
				alertMessageg.Clear(AlertTransparent ? Color.Transparent : InBackColour);
				alertMessageg.Dispose();
				AlertimageMessage.Dispose();

				AlertimageMessage_Inverse = new Bitmap(AlertMessageBackGroundRect.Width, num2);
				alertMessageg_Inverse = Graphics.FromImage(AlertimageMessage_Inverse);
				alertMessageg_Inverse.Clear(AlertTransparent ? Color.Transparent : InBackColour);
				alertMessageg_Inverse.Dispose();
				AlertimageMessage_Inverse.Dispose();

				gf.OutputOneLineToScreen(InItem, InString, InFont, alertMessageg, InTextColour, StringAlignment.Center, InUseShadow ? 1 : 0, InUseOutline ? 1 : 0, 0, (num2 - AlertMessageBackGroundRect.Height) / 2, AlertMessageBackGroundRect.Width, 0);
				Rectangle rectangle = default(Rectangle);
				rectangle.Y = ((InVerticalAlignment != 0) ? (_newBackgroundPicture.Height - num2) : 0);
				rectangle.Height = num2;
				if (AlertAlign == 3)
				{
					rectangle.X = num - (AlertMessageBackGroundRect.Width + AlertBorder * 2);
					rectangle.Width = AlertMessageBackGroundRect.Width + AlertBorder * 2;
					AlertMessageOriginX = rectangle.X + AlertMessageBackGroundRect.Width + AlertBorder / 2;
				}
				else if (AlertAlign == 1)
				{
					rectangle.X = 0;
					rectangle.Width = AlertMessageBackGroundRect.Width + AlertBorder * 2;
					AlertMessageOriginX = rectangle.X + AlertMessageBackGroundRect.Width + AlertBorder;
				}
				else
				{
					rectangle.X = 0;
					rectangle.Width = _newBackgroundPicture.Width;
					AlertMessageOriginX = (_newBackgroundPicture.Width - AlertMessageBackGroundRect.Width) / 2 + AlertMessageBackGroundRect.Width + AlertBorder / 2;
				}
				AlertMessageOriginY = rectangle.Top + 1;
				AlertOriginY = rectangle.Top;
				AlertOverallBackGroundRect = new Rectangle(0, 0, num, rectangle.Height);
				alertg.FillRectangle(new SolidBrush(AlertTransparent ? Color.Transparent : InBackColour), rectangle.X, 0, rectangle.Width, rectangle.Height);
				alertg_Inverse.FillRectangle(new SolidBrush(AlertTransparent ? Color.Transparent : InBackColour), rectangle.X, 0, rectangle.Width, rectangle.Height);
				AlertTransitionTime = InDuration;
				float MessageDuration = AlertMessageTransitionTime;
				AlertComputeOccurences(InString, AlertTransitionTime, ref MessageDuration, ref AlertOccurences);
				AlertMessageTransitionTime = MessageDuration;
				AlertGapRunning = false;
				AlertOverallCurrentPercentage = 0f;
				AlertMessagePercentage = 0f;
				AlertRunning = true;
				AlertStartTime = DateTime.Now;
				AlertMessageStartTime = DateTime.Now;
				Invalidate();
				// Dispose previous timer to prevent multiple timers running
				ta?.Dispose();
				ta = new System.Threading.Timer(AlertTick, null, 150, 150);
			}
		}

		private void AlertComputeOccurences(string InString, float OverallDurationTime, ref float MessageDuration, ref int Occurences)
		{
			AlertCharPerSecond = 6f;
			int num = (int)((float)InString.Length / AlertCharPerSecond) + AlertGapDuration;
			if ((float)num > OverallDurationTime)
			{
				AlertCharPerSecond = (float)InString.Length / (OverallDurationTime - (float)AlertGapDuration);
				Occurences = 1;
			}
			else
			{
				Occurences = (int)(OverallDurationTime / (float)num);
				if (Occurences == 0)
				{
					Occurences = 1;
				}
			}
			MessageDuration = (float)InString.Length / AlertCharPerSecond;
			if (MessageDuration < 0f)
			{
				MessageDuration = OverallDurationTime;
			}
		}

		private void AddAlertTextToImage(PaintEventArgs e)
		{
			if (!AlertRunning || AlertimageMessage == null)
			{
				return;
			}
			float num = (float)base.ClientRectangle.Width / (float)_newBackgroundPicture.Width;
			float num2 = (float)base.ClientRectangle.Height / (float)_newBackgroundPicture.Height;
			Rectangle rectangle = default(Rectangle);
			e.Graphics.ResetTransform();
			if (AlertMessagePercentage < 100f)
			{
				rectangle = new Rectangle(0, (int)((float)AlertOriginY * num2), (int)((float)AlertOverallBackGroundRect.Width * num), (int)((float)AlertOverallBackGroundRect.Height * num2));
				e.Graphics.DrawImage(AlertimageBackground, rectangle, AlertOverallBackGroundRect, GraphicsUnit.Pixel);
				if (AlertScroll)
				{
					int num3 = (int)((float)AlertMessageBackGroundRect.Width * AlertMessagePercentage / 100f);
					rectangle = new Rectangle((int)((float)(AlertMessageOriginX - num3) * num), (int)((float)AlertMessageOriginY * num2), (int)((float)num3 * num), (int)((float)AlertMessageBackGroundRect.Height * num2));
					e.Graphics.DrawImage(AlertimageMessage, rectangle, new Rectangle(0, 0, num3, AlertMessageBackGroundRect.Height), GraphicsUnit.Pixel);
				}
				else
				{
					rectangle = new Rectangle((int)((float)(AlertMessageOriginX - AlertMessageBackGroundRect.Width) * num), (int)((float)AlertMessageOriginY * num2), (int)((float)AlertMessageBackGroundRect.Width * num), (int)((float)AlertMessageBackGroundRect.Height * num2));
					e.Graphics.DrawImage(AlertimageMessage, rectangle, AlertMessageBackGroundRect, GraphicsUnit.Pixel);
				}
			}
			else if (AlertGapRunning)
			{
				if (AlertFlashCount > 0)
				{
					DoAlertFlash(e, UsePercentageTime: true);
					return;
				}
				rectangle = new Rectangle(0, (int)((float)AlertOriginY * num2), (int)((float)AlertOverallBackGroundRect.Width * num), (int)((float)AlertOverallBackGroundRect.Height * num2));
				e.Graphics.DrawImage(AlertimageBackground, rectangle, AlertOverallBackGroundRect, GraphicsUnit.Pixel);
				rectangle = new Rectangle((int)((float)(AlertMessageOriginX - AlertMessageBackGroundRect.Width) * num), (int)((float)AlertMessageOriginY * num2), (int)((float)AlertMessageBackGroundRect.Width * num), (int)((float)AlertMessageBackGroundRect.Height * num2));
				e.Graphics.DrawImage(AlertimageMessage, rectangle, AlertMessageBackGroundRect, GraphicsUnit.Pixel);
				if (DateTime.Now.Subtract(AlertGapStartTime).TotalSeconds >= (double)AlertGapDuration)
				{
					if (AlertOccurences > 1)
					{
						AlertOccurences--;
						AlertMessageStartTime = DateTime.Now;
						AlertMessagePercentage = 0f;
					}
					AlertGapRunning = false;
				}
			}
			else
			{
				rectangle = new Rectangle(0, (int)((float)AlertOriginY * num2), (int)((float)AlertOverallBackGroundRect.Width * num), (int)((float)AlertOverallBackGroundRect.Height * num2));
				e.Graphics.DrawImage(AlertimageBackground, rectangle, AlertOverallBackGroundRect, GraphicsUnit.Pixel);
				rectangle = new Rectangle((int)((float)(AlertMessageOriginX - AlertMessageBackGroundRect.Width) * num), (int)((float)AlertMessageOriginY * num2), (int)((float)AlertMessageBackGroundRect.Width * num), (int)((float)AlertMessageBackGroundRect.Height * num2));
				e.Graphics.DrawImage(AlertimageMessage, rectangle, AlertMessageBackGroundRect, GraphicsUnit.Pixel);
				if (AlertFlash)
				{
					AlertFlashCount = AlertFlashCountMax;
				}
				AlertGapStartTime = DateTime.Now;
				AlertGapRunning = true;
			}
		}

		private bool DoAlertFlash(PaintEventArgs e, bool UsePercentageTime)
		{
			float num = (float)base.ClientRectangle.Width / (float)_newBackgroundPicture.Width;
			float num2 = (float)base.ClientRectangle.Height / (float)_newBackgroundPicture.Height;
			Rectangle rectangle = default(Rectangle);
			float num3 = UsePercentageTime ? AlertMessagePercentage : 0f;
			if (((AlertFlashCount > 8) & (AlertFlashCount < 12)) || ((AlertFlashCount > 0) & (AlertFlashCount < 5)))
			{
				rectangle = new Rectangle(0, (int)((float)AlertOriginY * num2), (int)((float)AlertOverallBackGroundRect.Width * num), (int)((float)AlertOverallBackGroundRect.Height * num2));
				e.Graphics.DrawImage(AlertimageBackground_Inverse, rectangle, AlertOverallBackGroundRect, GraphicsUnit.Pixel);
				int num4 = (int)((float)AlertMessageBackGroundRect.Width * num3 / 100f);
				rectangle = new Rectangle((int)((float)(AlertMessageOriginX - num4) * num), (int)((float)AlertMessageOriginY * num2), (int)((float)num4 * num), (int)((float)AlertMessageBackGroundRect.Height * num2));
				e.Graphics.DrawImage(AlertimageMessage_Inverse, rectangle, new Rectangle(0, 0, num4, AlertMessageBackGroundRect.Height), GraphicsUnit.Pixel);
			}
			else
			{
				rectangle = new Rectangle(0, (int)((float)AlertOriginY * num2), (int)((float)AlertOverallBackGroundRect.Width * num), (int)((float)AlertOverallBackGroundRect.Height * num2));
				e.Graphics.DrawImage(AlertimageBackground, rectangle, AlertOverallBackGroundRect, GraphicsUnit.Pixel);
				int num4 = (int)((float)AlertMessageBackGroundRect.Width * num3 / 100f);
				rectangle = new Rectangle((int)((float)(AlertMessageOriginX - num4) * num), (int)((float)AlertMessageOriginY * num2), (int)((float)num4 * num), (int)((float)AlertMessageBackGroundRect.Height * num2));
				e.Graphics.DrawImage(AlertimageMessage, rectangle, new Rectangle(0, 0, num4, AlertMessageBackGroundRect.Height), GraphicsUnit.Pixel);
			}
			AlertFlashCount--;
			return (AlertFlashCount > 0) ? true : false;
		}

		public void StopAlert()
		{
			AlertStartTime = new DateTime(2005, 1, 1);
		}

		private void RefTick(object state)
		{
			TimeSpan timeSpan = DateTime.Now.Subtract(RefStartTime);
			RefOverallCurrentPercentage = Convert.ToSingle(100.0 / _refTransitionTime.TotalSeconds * timeSpan.TotalSeconds);
			if (RefOverallCurrentPercentage > 100f)
			{
				RefOverallCurrentPercentage = 100f;
			}
			TimeSpan timeSpan2 = DateTime.Now.Subtract(RefMessageStartTime);
			RefMessagePercentage = Convert.ToSingle(100.0 / _refMessageTransitionTime.TotalSeconds * timeSpan2.TotalSeconds);
			if (RefMessagePercentage > 100f)
			{
				RefMessagePercentage = 100f;
			}
			Invalidate();
		}

		public void LoadRef(SongSettings InItem, string InString, int InDuration, Font InFont, bool InUseScroll, bool InUseFlash, bool InTransparent, bool InUseShadow, bool InUseOutline, Color InTextColour, Color InBackColour, int InAlignment, int InVerticalAlignment, double BottomBorderFactor)
		{
			RefDisplayString = InString;
			if (!(InString == ""))
			{
				try
				{
					int num = _newBackgroundPicture.Width;
					int num2 = (int)(gf.MinBottomBorderFactor * (double)(float)_newBackgroundPicture.Height * 1.1000000238418579);
					int num3 = (int)(gf.MinBottomBorderFactor * (double)(float)base.ClientSize.Height * 3.0999999046325684);
					if (num < 1)
					{
						num = 1;
					}
					if (num3 < 1)
					{
						num3 = 1;
					}
					RefAlign = InAlignment;
					RefScroll = InUseScroll;
					RefFlash = InUseFlash;
					RefTransparent = InTransparent;
					RefBorder = num / 20;
					RefimageBackground = new Bitmap(num, num3);
					refg = Graphics.FromImage(RefimageBackground);
					InFont = new Font(InFont.Name, InFont.Size - 1f, InFont.Style);
					int num4 = gf.ReduceFontToFit(refg, InString, ref InFont, num - RefBorder, num3);
					RefMessageBackGroundRect = new Rectangle(0, 0, (int)refg.MeasureString(InString, InFont).Width, num4);
					num3 = num4 + 4;
					num2 = num2 * 2 + num3;
					refg.Dispose();
					RefimageBackground.Dispose();

					RefimageBackground = new Bitmap(num, num3);
					refg = Graphics.FromImage(RefimageBackground);
					refg.Clear(Color.Transparent);
					refg.Dispose();
					RefimageBackground.Dispose();

					RefimageBackground_Inverse = new Bitmap(num, num3);
					refg_Inverse = Graphics.FromImage(RefimageBackground_Inverse);
					refg_Inverse.Clear(Color.Transparent);
					refg_Inverse.Dispose();
					RefimageBackground_Inverse.Dispose();

					RefimageMessage = new Bitmap(RefMessageBackGroundRect.Width, RefMessageBackGroundRect.Height);
					refMessageg = Graphics.FromImage(RefimageMessage);
					refMessageg.Clear(RefTransparent ? Color.Transparent : InBackColour);
					refMessageg.Dispose();
					RefimageMessage.Dispose();

					RefimageMessage_Inverse = new Bitmap(RefMessageBackGroundRect.Width, RefMessageBackGroundRect.Height);
					refMessageg_Inverse = Graphics.FromImage(RefimageMessage_Inverse);
					refMessageg_Inverse.Clear(RefTransparent ? Color.Transparent : InBackColour);
					gf.OutputOneLineToScreen(InItem, InString, InFont, refMessageg, InTextColour, StringAlignment.Center, InUseShadow ? 1 : 0, InUseOutline ? 1 : 0, 0, (num3 - RefMessageBackGroundRect.Height) / 2, RefMessageBackGroundRect.Width, 0);
					refMessageg_Inverse.Dispose();
					RefimageMessage_Inverse.Dispose();

					Rectangle rectangle = default(Rectangle);
					switch (InVerticalAlignment)
					{
					case 0:
						num2 = 0;
						break;
					case 2:
						num2 = _newBackgroundPicture.Height - num3;
						break;
					default:
						num2 = _newBackgroundPicture.Height - num2;
						break;
					}
					rectangle.Height = num3;
					if (RefAlign == 3)
					{
						rectangle.X = num - (RefMessageBackGroundRect.Width + RefBorder * 2);
						rectangle.Y = num2;
						rectangle.Width = RefMessageBackGroundRect.Width + RefBorder * 2;
						RefMessageOriginX = rectangle.X + RefMessageBackGroundRect.Width + RefBorder / 2;
					}
					else if (RefAlign == 1)
					{
						rectangle.X = 0;
						rectangle.Y = num2;
						rectangle.Width = RefMessageBackGroundRect.Width + RefBorder * 2;
						RefMessageOriginX = rectangle.X + RefMessageBackGroundRect.Width + RefBorder;
					}
					else
					{
						rectangle.X = 0;
						rectangle.Y = num2;
						rectangle.Width = _newBackgroundPicture.Width;
						RefMessageOriginX = (_newBackgroundPicture.Width - RefMessageBackGroundRect.Width) / 2 + RefMessageBackGroundRect.Width + RefBorder / 2;
					}
					RefMessageOriginY = rectangle.Top + 1;
					RefOriginY = rectangle.Top;
					RefOverallBackGroundRect = new Rectangle(0, 0, num, num3);
					refg.FillRectangle(new SolidBrush(RefTransparent ? Color.Transparent : InBackColour), rectangle.X, 0, rectangle.Width, rectangle.Height);
					refg_Inverse.FillRectangle(new SolidBrush(RefTransparent ? Color.Transparent : InBackColour), rectangle.X, 0, rectangle.Width, rectangle.Height);
					RefTransitionTime = InDuration;
					float MessageDuration = RefMessageTransitionTime;
					RefComputeOccurences(InString, RefTransitionTime, ref MessageDuration, ref RefOccurences);
					RefOccurences = 1;
					RefMessageTransitionTime = MessageDuration;
					RefGapRunning = false;
				}
				catch
				{
				}
			}
		}

		private void RefGo()
		{
			if (RefDisplayString != "")
			{
				RefOverallCurrentPercentage = 0f;
				RefMessagePercentage = 0f;
				RefRunning = true;
				RefStartTime = DateTime.Now;
				RefMessageStartTime = DateTime.Now;
				// Dispose previous timer to prevent multiple timers running
				tr?.Dispose();
				tr = new System.Threading.Timer(RefTick, null, 150, 150);
			}
		}

		private void RefComputeOccurences(string InString, float OverallDurationTime, ref float MessageDuration, ref int Occurences)
		{
			RefCharPerSecond = 8f;
			int num = (int)((float)InString.Length / RefCharPerSecond) + RefGapDuration;
			if ((float)num > OverallDurationTime)
			{
				RefCharPerSecond = (float)InString.Length / (OverallDurationTime - (float)RefGapDuration);
				Occurences = 1;
			}
			else
			{
				Occurences = (int)(OverallDurationTime / (float)num);
				if (Occurences == 0)
				{
					Occurences = 1;
				}
			}
			MessageDuration = (float)InString.Length / RefCharPerSecond;
			if (MessageDuration < 0f)
			{
				MessageDuration = OverallDurationTime;
			}
		}

		private void AddRefTextToImage(PaintEventArgs e)
		{
			if (!RefRunning || RefimageMessage == null)
			{
				return;
			}
			float num = (float)base.ClientRectangle.Width / (float)_newBackgroundPicture.Width;
			float num2 = (float)base.ClientRectangle.Height / (float)_newBackgroundPicture.Height;
			ClientBackgroundReducedRect = new Rectangle(0, (int)((float)RefOriginY * num2), (int)((float)RefOverallBackGroundRect.Width * num), (int)((float)RefOverallBackGroundRect.Height * num2));
			int num3 = (int)((float)RefMessageBackGroundRect.Height * num2);
			if (num3 >= ClientBackgroundReducedRect.Height)
			{
				ClientBackgroundReducedRect.Height = num3 + 2;
			}
			e.Graphics.ResetTransform();
			if (RefMessagePercentage < 100f)
			{
				e.Graphics.DrawImage(RefimageBackground, ClientBackgroundReducedRect, RefOverallBackGroundRect, GraphicsUnit.Pixel);
				if (RefScroll)
				{
					int num4 = (int)((float)RefMessageBackGroundRect.Width * RefMessagePercentage / 100f);
					ClientMessageReducedRect = new Rectangle((int)((float)(RefMessageOriginX - num4) * num), (int)((float)RefMessageOriginY * num2), (int)((float)num4 * num), num3);
					e.Graphics.DrawImage(RefimageMessage, ClientMessageReducedRect, new Rectangle(0, 0, num4, RefMessageBackGroundRect.Height), GraphicsUnit.Pixel);
				}
				else
				{
					ClientMessageReducedRect = new Rectangle((int)((float)(RefMessageOriginX - RefMessageBackGroundRect.Width) * num), (int)((float)RefMessageOriginY * num2), (int)((float)RefMessageBackGroundRect.Width * num), num3);
					e.Graphics.DrawImage(RefimageMessage, ClientMessageReducedRect, RefMessageBackGroundRect, GraphicsUnit.Pixel);
				}
			}
			else if (RefGapRunning)
			{
				if (RefFlashCount > 0)
				{
					DoRefFlash(e, UsePercentageTime: true);
					return;
				}
				ClientMessageReducedRect = new Rectangle(0, (int)((float)RefOriginY * num2), (int)((float)RefOverallBackGroundRect.Width * num), (int)((float)RefOverallBackGroundRect.Height * num2));
				e.Graphics.DrawImage(RefimageBackground, ClientMessageReducedRect, RefOverallBackGroundRect, GraphicsUnit.Pixel);
				ClientMessageReducedRect = new Rectangle((int)((float)(RefMessageOriginX - RefMessageBackGroundRect.Width) * num), (int)((float)RefMessageOriginY * num2), (int)((float)RefMessageBackGroundRect.Width * num), (int)((float)RefMessageBackGroundRect.Height * num2));
				e.Graphics.DrawImage(RefimageMessage, ClientMessageReducedRect, RefMessageBackGroundRect, GraphicsUnit.Pixel);
				if (DateTime.Now.Subtract(RefGapStartTime).TotalSeconds >= (double)RefGapDuration)
				{
					if (RefOccurences > 1)
					{
						RefOccurences--;
						RefMessageStartTime = DateTime.Now;
						RefMessagePercentage = 0f;
					}
					RefGapRunning = false;
				}
			}
			else
			{
				e.Graphics.DrawImage(RefimageBackground, ClientBackgroundReducedRect, RefOverallBackGroundRect, GraphicsUnit.Pixel);
				ClientMessageReducedRect = new Rectangle((int)((float)(RefMessageOriginX - RefMessageBackGroundRect.Width) * num), (int)((float)RefMessageOriginY * num2), (int)((float)RefMessageBackGroundRect.Width * num), num3);
				e.Graphics.DrawImage(RefimageMessage, ClientMessageReducedRect, RefMessageBackGroundRect, GraphicsUnit.Pixel);
				if (RefFlash)
				{
					RefFlashCount = RefFlashCountMax;
				}
				RefGapStartTime = DateTime.Now;
				RefGapRunning = true;
			}
		}

		private bool DoRefFlash(PaintEventArgs e, bool UsePercentageTime)
		{
			float num = UsePercentageTime ? RefMessagePercentage : 0f;
			float num2 = (float)base.ClientRectangle.Width / (float)_newBackgroundPicture.Width;
			float num3 = (float)base.ClientRectangle.Height / (float)_newBackgroundPicture.Height;
			ClientBackgroundReducedRect = new Rectangle(0, (int)((float)RefOriginY * num3), (int)((float)RefOverallBackGroundRect.Width * num2), (int)((float)RefOverallBackGroundRect.Height * num3));
			int num4 = (int)((float)RefMessageBackGroundRect.Width * num / 100f);
			ClientMessageReducedRect = new Rectangle((int)((float)(RefMessageOriginX - num4) * num2), (int)((float)RefMessageOriginY * num3), (int)((float)num4 * num2), (int)((float)RefMessageBackGroundRect.Height * num3));
			if (((RefFlashCount > 8) & (RefFlashCount < 12)) || ((RefFlashCount > 0) & (RefFlashCount < 5)))
			{
				e.Graphics.DrawImage(RefimageBackground_Inverse, ClientBackgroundReducedRect, RefOverallBackGroundRect, GraphicsUnit.Pixel);
				e.Graphics.DrawImage(RefimageMessage_Inverse, ClientMessageReducedRect, new Rectangle(0, 0, num4, RefMessageBackGroundRect.Height), GraphicsUnit.Pixel);
			}
			else
			{
				e.Graphics.DrawImage(RefimageBackground, ClientBackgroundReducedRect, RefOverallBackGroundRect, GraphicsUnit.Pixel);
				e.Graphics.DrawImage(RefimageMessage, ClientMessageReducedRect, new Rectangle(0, 0, num4, RefMessageBackGroundRect.Height), GraphicsUnit.Pixel);
			}
			RefFlashCount--;
			return (RefFlashCount > 0) ? true : false;
		}

		public void StopRef()
		{
			RefStartTime = new DateTime(2005, 1, 1);
			RefRunning = false;
		}

		public bool RefStatus()
		{
			return RefRunning;
		}
	
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				// ?�?�머 ?��? �??�제
				t?.Dispose();
				ta?.Dispose();
				tr?.Dispose();

				// Graphics 객체 ?�리
				wsg?.Dispose();
				Newg?.Dispose();
				alertg?.Dispose();
				alertMessageg?.Dispose();
				alertg_Inverse?.Dispose();
				alertMessageg_Inverse?.Dispose();
				refg?.Dispose();
				refMessageg?.Dispose();
				refg_Inverse?.Dispose();
				refMessageg_Inverse?.Dispose();

				// Image 객체 ?�리
				Newbmp?.Dispose();
				_currentCombinedImage?.Dispose();
				_newCombinedImage?.Dispose();
				_newBackgroundPicture?.Dispose();
				_currentBackgroundPicture?.Dispose();
				_newTextImage?.Dispose();
				_currentTextImage?.Dispose();
				_newPanelImage?.Dispose();
				_currentPanelImage?.Dispose();
				_imageWorkSpace?.Dispose();
				AlertimageBackground?.Dispose();
				AlertimageMessage?.Dispose();
				AlertimageBackground_Inverse?.Dispose();
				AlertimageMessage_Inverse?.Dispose();
				RefimageBackground?.Dispose();
				RefimageMessage?.Dispose();
				RefimageBackground_Inverse?.Dispose();
				RefimageMessage_Inverse?.Dispose();
			
				// Cached objects cleanup
				_cachedImageAttributes?.Dispose();
				// ColorMatrix does not have Dispose method
				_cachedColorMatrix = null;

				// Bitmap pool cleanup
				ClearBitmapPool();
			}

			base.Dispose(disposing);
		}

	// Bitmap pool methods for reuse to reduce GC pressure
	private Bitmap GetBitmapFromPool(int width, int height)
	{
		lock (_poolLock)
		{
			if (_bitmapPool.TryTake(out Bitmap bitmap))
			{
				// Check if the bitmap size matches
				if (bitmap.Width == width && bitmap.Height == height)
				{
					return bitmap;
				}
				else
				{
					// Size mismatch, dispose and create new
					bitmap.Dispose();
				}
			}

			// No suitable bitmap in pool, create new
			return new Bitmap(width, height);
		}
	}

	private void ReturnBitmapToPool(Bitmap bitmap)
	{
		if (bitmap == null) return;

		lock (_poolLock)
		{
			if (_bitmapPool.Count < _maxPoolSize)
			{
				_bitmapPool.Add(bitmap);
			}
			else
			{
				// Pool is full, dispose the bitmap
				bitmap.Dispose();
			}
		}
	}

	private void ClearBitmapPool()
	{
		lock (_poolLock)
		{
			while (_bitmapPool.TryTake(out Bitmap bitmap))
			{
				bitmap.Dispose();
			}
		}
	}
}
}
