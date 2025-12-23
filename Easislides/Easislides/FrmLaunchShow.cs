using Easislides.Module;
using Easislides.Util;
using OfficeLib;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Easislides
{
	public class FrmLaunchShow : Form
	{
		public delegate void Message(int MsgCode, string MsgString);

		private enum MediaPlayerWindowAction
		{
			Show,
			SendToBack,
			Remote_StopShow,
			Remote_ClearScreen,
			Remote_LoadItem,
			Remote_ResumeItem,
			Remote_RepeatItem,
			Remote_StopItem,
			Remote_ResumeItemFromStart,
			Remote_PauseItem,
			Remote_PausePlayItem,
			Remote_LoadLiveCam,
			Remote_UpdateLiveCam,
			Remote_RefreshMediaWindow,
			Remote_SendScreenToBack,
			Remote_GetMediaTimings,
			Remote_ItemPlayingStatus
		}

		private enum LyricsWindowAction
		{
			Show,
			Remote_StopShow,
			Remote_LyricsChanged,
			Remote_ItemChanged,
			Remote_NotationsChanged,
			Remote_WorshipListChanged,
			Remote_LyricsAlertChanged
		}

		private IContainer components = null;

		private Timer TimerSingleScreen;

		private Timer TimerRemote;

		private Timer TimerMouseDown;

		private Timer TimerOpacity;

		private Timer TimerRotate;

		private ImageList imageList1;

		private Timer TimerToFront;

		private bool FormFirstLoad = true;

		private bool StayTopMost = true;

		private MouseButtons mouse_btn;

		private int mousedown_timelapse;

		private string[] InHeaderData = new string[255];

		private ImageTransitionControl LiveScreen = new ImageTransitionControl();

		private int ItemRotationNextTiming = 0;

		private int ItemRotationNextSlideNumber = 0;

		private int prevRefMode = 0;

		private string tempRotateTimings = "";

		private bool FirstItemLoaded = true;

		private bool ItemMediaChangedSinceLiveCam = false;

		private bool FirstItemBeingProcessed = true;

		private bool CursorOverForm = false;

		private bool CurItemRotates = false;

		private string LiveMediaDuration = "";

		private string LiveMediaPosition = "";

		private int intLiveMediaDuration = 0;

		private int intLiveMediaPosition = 0;

		private bool CurMediaDoRotate = false;

		private bool CurMediaPlayingStatus = false;

		private int LoadRepaintCount = 0;

		private Cursor LiveCursor;

		private FrmLaunchMediaPlayer MediaPlayerWindow = new FrmLaunchMediaPlayer();

		private FrmLyricsScreen LyricsWindow = new FrmLyricsScreen();

		public event Message OnMessage;

		//Active 되어 있을 경우에만 Active 호출 할 수 있도록 선언
		private bool isActivated = false;

		protected override void Dispose(bool disposing)
		{
			if (disposing && components != null)
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		//Active 되어 있을 경우에만 Active 호출
		protected override void OnActivated(EventArgs e)
		{
			isActivated = true;

			base.OnActivated(e);
		}

		private void InitializeComponent()
		{
			components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmLaunchShow));
			TimerSingleScreen = new System.Windows.Forms.Timer(components);
			TimerRemote = new System.Windows.Forms.Timer(components);
			TimerMouseDown = new System.Windows.Forms.Timer(components);
			TimerOpacity = new System.Windows.Forms.Timer(components);
			TimerRotate = new System.Windows.Forms.Timer(components);
			imageList1 = new System.Windows.Forms.ImageList(components);
			TimerToFront = new System.Windows.Forms.Timer(components);
			SuspendLayout();
			TimerSingleScreen.Tick += new System.EventHandler(TimerSingleScreen_Tick);
			TimerRemote.Tick += new System.EventHandler(TimerRemote_Tick);
			TimerMouseDown.Enabled = true;
			TimerMouseDown.Interval = 200;
			TimerMouseDown.Tick += new System.EventHandler(TimerMouseDown_Tick);
			TimerOpacity.Tick += new System.EventHandler(TimerOpacity_Tick);
			TimerRotate.Interval = 500;
			TimerRotate.Tick += new System.EventHandler(TimerRotate_Tick);
			imageList1.ImageStream = (System.Windows.Forms.ImageListStreamer)resources.GetObject("imageList1.ImageStream");
			imageList1.TransparentColor = System.Drawing.Color.Transparent;
			imageList1.Images.SetKeyName(0, "Blank.gif");
			TimerToFront.Enabled = true;
			TimerToFront.Tick += new System.EventHandler(TimerToFront_Tick);
			base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
			base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			BackColor = System.Drawing.Color.Black;
			base.ClientSize = new System.Drawing.Size(102, 72);
			base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			base.Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
			base.KeyPreview = true;
			base.Name = "FrmLaunchShow";
			base.ShowIcon = false;
			base.ShowInTaskbar = false;
			Text = "Live";
			base.TopMost = true;
			base.TransparencyKey = System.Drawing.Color.FromArgb(128, 64, 192);
			base.Load += new System.EventHandler(FrmLaunchShow_Load);
			base.Enter += new System.EventHandler(FrmLaunchShow_Enter);
			base.VisibleChanged += new System.EventHandler(FrmLaunchShow_VisibleChanged);
			base.Leave += new System.EventHandler(FrmLaunchShow_Leave);
			base.FormClosing += new System.Windows.Forms.FormClosingEventHandler(FrmLaunchShow_FormClosing);
			ResumeLayout(false);
		}

		public FrmLaunchShow()
		{
			InitializeComponent();
		}

		private void FrmLaunchShow_Load(object sender, EventArgs e)
		{
			if (FormFirstLoad)
			{
				Bitmap bitmap = new Bitmap(55, 25);
				//Graphics graphics = Graphics.FromImage(bitmap);
				IntPtr hicon = bitmap.GetHicon();
				LiveCursor = new Cursor(hicon);
				Cursor = LiveCursor;
				InitForm();
				LiveScreen.MouseUp += FrmLiveShow_MouseUp;
				LiveScreen.MouseDown += FrmLiveShow_MouseDown;
				MediaPlayerWindow.OnMessage += MediaPlayerWindow_OnMessage;
				LyricsWindow.OnMessage += LyricsWindow_OnMessage;
				LiveScreen.Cursor = LiveCursor;
				//bitmap.Dispose();
				//graphics.Dispose();
			}
		}

		private void InitForm()
		{
			BackColor = gf.TransparentColour;
			base.TransparencyKey = gf.TransparentColour;
			LiveScreen.Parent = this;
			LiveScreen.Dock = DockStyle.Fill;
			gf.SetLiveShowScreenSaverSettings();
			SetShowWindow(max: true);
			TimerSingleScreen.Interval = gf.AlertGap;
			TimerRemote.Interval = gf.AlertGap;
			TimerMouseDown.Interval = gf.AlertGap;
			TimerOpacity.Interval = gf.AlertGap;
			gf.LiveItem.Initialise();
			gf.RestartItemActioned = true;
			gf.SetDefaultBackScreen(ref LiveScreen);

			//if (gf.LyricsMonitorNumber > 0 || gf.LMSelectAutoOption > 0)
			//{
			//    gf.tbLyricsMonitorSpace.Font = new Font(gf.tbLyricsMonitorSpace.Font.Name, gf.DisplayFontSize(gf.LMMainFontSize, gf.LM_Width, 1, 1), gf.tbLyricsMonitorSpace.Font.Style);
			//    try
			//    {
			//        RemoteControlLyricsWindow(LyricsWindowAction.Show);
			//    }
			//    catch
			//    {
			//    }
			//}

			if (gf.LyricsMonitorName == gf.GetSecondryMonitorName() || gf.LMSelectAutoOption > 0)
			{
				gf.tbLyricsMonitorSpace.Font = new Font(gf.tbLyricsMonitorSpace.Font.Name, gf.DisplayFontSize(gf.LMMainFontSize, gf.LM_Width, 1, 1), gf.tbLyricsMonitorSpace.Font.Style);
				try
				{
					RemoteControlLyricsWindow(LyricsWindowAction.Show);
				}
				catch
				{
				}
			}

			gf.MediaResetStartTime = true;
			ResetMediaSettings();
			InitMediaWindow();
			FirstItemBeingProcessed = true;
			if (gf.OutputItem.Type == "G")
			{
				string InTitle = "";
				gf.LiveItem.CurItemNo = gf.StartPresAt;
				LoadItem(ref gf.LiveItem, "G1", "", 0, ref InTitle, ImageTransitionControl.TransitionAction.None, ReLoadIfCaptureDevice: false);
			}
			else
			{
				LoadWorshipListItemToLive((!gf.AdHocItemPresent) ? gf.StartPresAt : 0, gf.OutputItem.CurSlide, ImageTransitionControl.TransitionAction.AsStored);
			}
			if (gf.MessageAlertRequested)
			{
				Remote_MessageAlertRequested();
			}
			else if (gf.ParentalAlertRequested)
			{
				Remote_ParentalAlertRequested();
			}
			FirstItemBeingProcessed = false;
			if (gf.ShowLiveCam)
			{
				Remote_LiveCamStartStop();
			}
			if (gf.DualMonitorMode)
			{
				base.TopMost = true;
				return;
			}
			gf.HideTaskBar();
			SetScreenOnTop(StartTimer: true);
		}

		private void InitMediaWindow()
		{
			if (gf.WMP_Present)
			{
				try
				{
					RemoteControlMediaPlayerWindow(MediaPlayerWindowAction.Show);
					RemoteControlMediaPlayerWindow(MediaPlayerWindowAction.SendToBack);
				}
				catch
				{
				}
			}
		}

		private void MediaPlayerWindow_OnMessage(int MsgCode, string MsgString)
		{
			switch (MsgCode)
			{
			case 3:
				Remote_ItemIsVideo();
				break;
			case 11:
				DoMouseDown(MouseButtons.Left);
				break;
			case 12:
				DoMouseDown(MouseButtons.Right);
				break;
			case 13:
				DoMouseUp();
				break;
			}
		}

		private void LyricsWindow_OnMessage(int MsgCode, string MsgString)
		{
			switch (MsgCode)
			{
			case 3:
				Remote_ItemIsVideo();
				break;
			case 4:
				Remote_ItemIsNotVideo();
				break;
			}
		}

		public void Remote_ItemIsVideo()
		{
			gf.LiveItem.Format.MediaTransparent = true;
			gf.MediaCurrentItemIsVideo = true;
			ShowSlide(ref gf.LiveItem, ImageTransitionControl.TransitionAction.None);
			gf.MinimizePowerPointWindows(ref gf.LivePP);
		}

		public void Remote_ItemIsNotVideo()
		{
			gf.MediaCurrentItemIsVideo = false;
			gf.LiveItem.Format.MediaTransparent = false;
		}

		public void Remote_WorshipListChanged()
		{
			RemoteControlLyricsWindow(LyricsWindowAction.Remote_WorshipListChanged);
		}

		public void Remote_LiveCamStartStop()
		{
			if (gf.ShowLiveCam)
			{
				CurMediaDoRotate = gf.MediaDoRotate;
				TimerRotate.Stop();
				RemoteControlMediaPlayerWindow(MediaPlayerWindowAction.Remote_LoadLiveCam);
				ShowSlide(ref gf.LiveItem, ImageTransitionControl.TransitionAction.None);
				ItemMediaChangedSinceLiveCam = false;
				gf.MinimizePowerPointWindows(ref gf.LivePP);
				return;
			}
			if (gf.LiveItem.Type == "P")
			{
				gf.RunPowerpointSong(ref gf.LiveItem, ref gf.LivePP, gf.LiveItem.CurSlide);
			}
			if (ItemMediaChangedSinceLiveCam)
			{
				Remote_SongChanged(ReLoadIfCaptureDevice: true);
				return;
			}
			gf.MediaLiveItemStartTime = DateTime.Now.Subtract(gf.MediaPlayedLapseTime);
			TimerRotate.Start();
			RemoteControlMediaPlayerWindow(MediaPlayerWindowAction.Remote_ResumeItem);
			gf.MediaDoRotate = CurMediaDoRotate;
			if (gf.MediaDoRotate)
			{
				RemoteControlMediaPlayerWindow(MediaPlayerWindowAction.Remote_GetMediaTimings);
				gf.MediaLiveItemStartTime = DateTime.Now.Subtract(new TimeSpan(0, 0, intLiveMediaPosition));
			}
			ShowSlide(ref gf.LiveItem, ImageTransitionControl.TransitionAction.None, DoActiveIndicator: false, RedoBackground: true);
		}

		public void Remote_LiveCamUpdate()
		{
			if (gf.ShowLiveCam)
			{
				RemoteControlMediaPlayerWindow(MediaPlayerWindowAction.Remote_UpdateLiveCam);
			}
		}

		private void SetShowWindow(bool max)
		{
			if (max)
			{
				base.Left = gf.LS_Left;
				base.Top = gf.LS_Top;
				base.Height = gf.LS_Height;
				base.Width = gf.LS_Width;
			}
			else
			{
				base.Left = gf.LS_Left;
				base.Top = gf.LS_Top;
				base.Height = 0;
				base.Width = 0;
			}
		}

		private void SetScreenOnTop(bool StartTimer)
		{
			StayTopMost = StartTimer;
			base.TopMost = StayTopMost;
			if (StartTimer && !gf.DualMonitorMode)
			{
				Cursor.Position = new Point(gf.LS_Left, gf.LS_Height - 1);
				TimerSingleScreen.Start();
			}
			else
			{
				TimerSingleScreen.Stop();
			}
		}

		private void TimerOpacity_Tick(object sender, EventArgs e)
		{
			base.Opacity = 100.0;
			TimerOpacity.Enabled = false;
		}

		private void FrmLaunchShow_FormClosing(object sender, FormClosingEventArgs e)
		{
			FormClosingCleanup();
		}

		private void FormClosingCleanup()
		{
			gf.ResetShowRunningSettings();
			gf.MessageAlertLive = false;
			gf.ParentalAlertLive = false;
			gf.ShowRunning = false;
			TimerRotate.Stop();
			TimerSingleScreen.Stop();
			TimerRemote.Stop();
			TimerMouseDown.Stop();
			gf.OutputItem.Type = gf.LiveItem.Type;
			gf.OutputItem.CurItemNo = ((gf.LiveItem.CurItemNo < 0) ? 1 : gf.LiveItem.CurItemNo);
			gf.OutputItem.CurSlide = ((gf.LiveItem.CurSlide < 1) ? 1 : gf.LiveItem.CurSlide);
			gf.RestoreScreenSaverSettings();
			gf.ClearUpPowerpointWindows();
			gf.RefreshWindowsDesktop();
			gf.ShowTaskBar();
		}

		public void StopShow()
		{
			try
			{
				RemoteControlMediaPlayerWindow(MediaPlayerWindowAction.Remote_StopShow);
				gf.DrawText(ref gf.LiveItem, ref LiveScreen, gf.LiveItem.LyricsAndNotationsList, DoActiveIndicator: false, ClearAll: true);
			}
			catch
			{
			}
			try
			{
				RemoteControlLyricsWindow(LyricsWindowAction.Remote_StopShow);
			}
			catch
			{
			}
			FormClosingCleanup();
			Hide();
			gf.RefreshWindowsDesktop();
		}

		public void Remote_DefaultBackgroundChanged()
		{
			gf.SetDefaultBackScreen(ref LiveScreen);
			gf.SetShowBackground(gf.LiveItem, ref LiveScreen);
			ShowSlide(ref gf.LiveItem, ImageTransitionControl.TransitionAction.None);
		}

		public void Remote_BackgroundChanged()
		{
			gf.SetShowBackground(gf.LiveItem, ref LiveScreen);
			ShowSlide(ref gf.LiveItem, ImageTransitionControl.TransitionAction.None);
		}

		public void Remote_MoveToItemChanged()
		{
			MoveToLiveItem(gf.LiveItem, gf.MainAction_MoveToItemKeyDirection, gf.OutputItem.CurSlide);
		}

		public void Remote_SongChanged(bool ReLoadIfCaptureDevice)
		{
			if (gf.OutputItem.Type == "G")
			{
				ShowSlide(ref gf.LiveItem, ImageTransitionControl.TransitionAction.None);
			}
			else
			{
				LoadWorshipListItemToLive(gf.DualMonitorMode ? gf.OutputItem.CurItemNo : gf.LiveItem.CurItemNo, gf.OutputItem.CurSlide, gf.MainAction_SongChanged_Transaction, ReLoadIfCaptureDevice);
			}
			gf.LaunchShowUpdateDone = true;
		}

		public void Remote_SlideChanged(int InDirection)
		{
			if (gf.LiveItem.CurItemNo == gf.OutputItem.CurItemNo)
			{
				if (gf.LiveItem.Type == "P")
				{
					gf.LiveItem.CurSlide = gf.OutputItem.CurSlide;
					MoveToSlideLiveItem(gf.LiveItem, KeyDirection.Refresh);
				}
				else
				{
					gf.LiveItem.CurSlide = gf.OutputItem.CurSlide;
					MoveToSlideLiveItem(gf.LiveItem, KeyDirection.Refresh);
				}
			}
			else
			{
				LoadWorshipListItemToLive(gf.OutputItem.CurItemNo, gf.OutputItem.CurSlide, ImageTransitionControl.TransitionAction.AsStored);
			}
		}

		public void Remote_SongJumpTo()
		{
			if (gf.OutputItem.Type == "G")
			{
				string InTitle = "";
				LoadItem(ref gf.LiveItem, "G1", "", 0, ref InTitle, ImageTransitionControl.TransitionAction.None, ReLoadIfCaptureDevice: false);
			}
			else
			{
				MoveToLiveItem(gf.LiveItem, KeyDirection.Refresh);
			}
		}

		public void Remote_LiveBlackClearChanged()
		{
			if (gf.LiveItem.Type == "P")
			{
				gf.DrawText(ref gf.LiveItem, ref LiveScreen, gf.LiveItem.LyricsAndNotationsList, DoActiveIndicator: false, ClearAll: false);
			}
			else
			{
				ShowSlide(ref gf.LiveItem, gf.GapItemUseFade ? ImageTransitionControl.TransitionAction.AsFade : ImageTransitionControl.TransitionAction.None);
			}
		}

		public void Remote_FormatChanged()
		{
			if (gf.LiveItem.Type != "P")
			{
				RefreshSlidesFonts(ref gf.LiveItem, gf.MainAction_SongChanged_Transaction);
			}
		}

		public void Remote_PanelChanged()
		{
			if (gf.LiveItem.Type == "P")
			{
				gf.DrawText(ref gf.LiveItem, ref LiveScreen, gf.LiveItem.LyricsAndNotationsList, DoActiveIndicator: false, ClearAll: false);
			}
			else
			{
				RefreshSlidesFonts(ref gf.LiveItem, gf.MainAction_SongChanged_Transaction);
			}
		}

		public void Remote_ChineseChanged()
		{
			gf.SwitchChineseLyricsNotationListView(ref gf.LiveItem, gf.SwitchChinese(ref gf.LiveItem.CompleteLyrics));
			if (gf.LiveItem.Type != "P")
			{
				ShowSlide(ref gf.LiveItem, ImageTransitionControl.TransitionAction.None);
			}
		}

		public void Remote_MessageAlertRequested()
		{
			gf.MessageAlertRequested = false;
			gf.AlertSettings(AlertType.Message);
			LiveScreen.StartAlert(gf.LiveItem, gf.Alert_OriginalMessage, gf.AlertTimeRemaining, gf.Alert_UserFont, gf.Alert_Scroll, gf.Alert_Flash, gf.Alert_Transparent, gf.Alert_UserFontShadow, gf.Alert_UserFontOutline, gf.Alert_TextColour, gf.Alert_BackColour, gf.Alert_TextAlign, gf.Alert_VerticalAlign, gf.BottomBorderFactor);
		}

		public void Remote_ParentalAlertRequested()
		{
			gf.ParentalAlertRequested = false;
			gf.AlertSettings(AlertType.Parental);
			LiveScreen.StartAlert(gf.LiveItem, gf.Alert_OriginalMessage, gf.AlertTimeRemaining, gf.Alert_UserFont, gf.Alert_Scroll, gf.Alert_Flash, gf.Alert_Transparent, gf.Alert_UserFontShadow, gf.Alert_UserFontOutline, gf.Alert_TextColour, gf.Alert_BackColour, gf.Alert_TextAlign, gf.Alert_VerticalAlign, gf.BottomBorderFactor);
		}

		public void Remote_ReferenceAlertRequested(bool NewStatus)
		{
			QueryShowActive(NewStatus);
		}

		public void Remote_LyricsAlertRequested()
		{
			gf.LyricsAlertRequested = false;
			RemoteControlLyricsWindow(LyricsWindowAction.Remote_LyricsAlertChanged);
		}

		public void Remote_RotateOnOffChanged()
		{
			TimerRotate.Start();
		}

		public void Remote_StopShow()
		{
			try
			{
				RemoteControlMediaPlayerWindow(MediaPlayerWindowAction.Remote_StopShow);
			}
			catch
			{
			}
		}

		public void Remote_RefreshMediaWindow()
		{
			try
			{
				RemoteControlMediaPlayerWindow(MediaPlayerWindowAction.Remote_RefreshMediaWindow);
			}
			catch
			{
			}
		}

		public string Remote_GetMediaTimings()
		{
			if (gf.ShowLiveCam)
			{
				return "";
			}
			string result = "";
			if (gf.AutoRotateOn && CurItemRotates)
			{
				LiveMediaPosition = new DateTime(gf.MediaPlayedLapseTime.Ticks).ToString("mm:ss");
				LiveMediaDuration = new DateTime(new TimeSpan(0, 0, gf.LiveItem.RotateTotal).Ticks).ToString("mm:ss");
				result = LiveMediaPosition + ((gf.LiveItem.RotateTotal > 0) ? (" [" + LiveMediaDuration + "]") : "");
			}
			else if (gf.LiveItem.Type == "M")
			{
				RemoteControlMediaPlayerWindow(MediaPlayerWindowAction.Remote_GetMediaTimings);
				if (LiveMediaDuration != "")
				{
					result = LiveMediaPosition + " [" + LiveMediaDuration + "]";
				}
			}
			return result;
		}

		public string Remote_MediaItemPausePlay()
		{
			if (gf.CurrentMediaLocation != "" && !gf.ShowLiveCam)
			{
				RemoteControlMediaPlayerWindow(MediaPlayerWindowAction.Remote_PausePlayItem);
				gf.MediaDoRotate = false;
			}
			return "";
		}

		private void TimerRemote_Tick(object sender, EventArgs e)
		{
		}

		/// <summary>
		/// daniel v2.2 수정
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		// 파워포인트에 동영상 컨텐츠가 있을 경우 이대로 코딩 하면 동영상 재생이 안됨
		//private void TimerSingleScreen_Tick(object sender, EventArgs e)
		//{
		//	base.TopMost = StayTopMost;
		//	if (StayTopMost && Cursor.Position.X >= base.Left && Cursor.Position.X <= base.Left + base.Width && Cursor.Position.Y >= base.Top && Cursor.Position.Y <= base.Top + base.Bottom)
		//	{
		//		Activate();
		//		Cursor.Position = new Point(gf.LS_Left, gf.LS_Height - 1);
		//	}
		//}

		
		// 파워포인트에 동영상 컨텐츠가 있을 경우 재생 되도록 수정
		private void TimerSingleScreen_Tick(object sender, EventArgs e)
        {
            base.TopMost = StayTopMost;
			if (StayTopMost && !isActivated)
			{
				Activate();
			}
		}

        private void TimerMouseDown_Tick(object sender, EventArgs e)
		{
			if (!gf.DualMonitorMode)
			{
				mousedown_timelapse += gf.AlertGap;
			}
			else
			{
				TimerMouseDown.Stop();
			}
		}
		/// <summary>
		/// daniel
		/// </summary>
		/// <param name="InItem"></param>
		/// <param name="KeyCode"></param>
		private void ItemKeyPressed(SongSettings InItem, Keys KeyCode)
		{
			if (gf.DualMonitorMode)
			{
				gf.ReMapKeyBoard(ref KeyCode);
				if (KeyCode == Keys.Escape || KeyCode == Keys.Subtract || KeyCode == Keys.OemMinus || KeyCode == Keys.F12)
				{
					this.OnMessage(10, "");
				}
				return;
			}
			gf.ReMapKeyBoard(ref KeyCode);
			if (KeyCode == Keys.Escape || KeyCode == Keys.Subtract || KeyCode == Keys.OemMinus || KeyCode == Keys.F12)
			{
				//daniel
				//esc 키를 누르면 슬라이드 쇼가 종료 됨
				StopShow();
				return;
			}
			int num;
			switch (KeyCode)
			{
			case Keys.F1:
			{
				SetScreenOnTop(StartTimer: false);
				FrmHelp frmHelp = new FrmHelp();
				frmHelp.ShowDialog();
				SetScreenOnTop(StartTimer: true);
				return;
			}
			case Keys.F9 or Keys.F10:
				gf.ShowLiveBlack = !gf.ShowLiveBlack;
				ShowSlide(ref gf.LiveItem, gf.GapItemUseFade ? ImageTransitionControl.TransitionAction.AsFade : ImageTransitionControl.TransitionAction.None);
				return;
			case Keys.F3:
				gf.ShowLiveClear = !gf.ShowLiveClear;
				ShowSlide(ref gf.LiveItem, gf.GapItemUseFade ? ImageTransitionControl.TransitionAction.AsFade : ImageTransitionControl.TransitionAction.None);
				return;
			case Keys.F4:
				gf.ShowLiveCam = !gf.ShowLiveCam;
				Remote_LiveCamStartStop();
				return;
			case Keys.F5:
				gf.RestartItemActioned = false;
				gf.AutoRotateOn = false;
				MoveToLiveItem(InItem, KeyDirection.Refresh);
				return;
			//case Keys.F9:
			//{
			//	SetScreenOnTop(StartTimer: false);
			//	FrmSingleMonitorAlert frmSingleMonitorAlert = new FrmSingleMonitorAlert();
			//	DialogResult dialogResult = frmSingleMonitorAlert.ShowDialog();
			//	if (dialogResult == DialogResult.OK)
			//	{
			//		LiveScreen.StartAlert(gf.LiveItem, gf.Alert_OriginalMessage, gf.AlertTimeRemaining, gf.Alert_UserFont, gf.Alert_Scroll, gf.Alert_Flash, gf.Alert_Transparent, gf.Alert_UserFontShadow, gf.Alert_UserFontOutline, gf.Alert_TextColour, gf.Alert_BackColour, gf.Alert_TextAlign, gf.Alert_VerticalAlign, gf.BottomBorderFactor);
			//	}
			//	SetScreenOnTop(StartTimer: true);
			//	return;
			//}
			//case Keys.F6:
			//	gf.SwitchChineseLyricsNotationListView(ref gf.LiveItem, gf.SwitchChinese(ref gf.LiveItem.CompleteLyrics));
			//	ShowSlide(ref gf.LiveItem, ImageTransitionControl.TransitionAction.None);
			//	return;
			case Keys.A:
				gf.AutoRotateOn = !gf.AutoRotateOn;
				gf.RestartCurrentItem = false;
				TimerRotate.Start();
				return;
			case Keys.M:
				Remote_MediaItemPausePlay();
				return;
			case Keys.J:
				if (InItem.OutputStyleScreen)
				{
					GotoNextNonRotateItem();
				}
				return;
			case Keys.D:
				ToggleShowDataDisplayMode();
				return;
			case Keys.H:
				ToggleShowHeader();
				return;
			case Keys.R:
				ToggleShowLyrics();
				return;
			case Keys.S:
				ToggleUseShadowFont();
				return;
			case Keys.O:
				ToggleUseOutlineFont();
				return;
			case Keys.I:
				ToggleInterlace();
				return;
			case Keys.V:
				ToggleVerticalAlignment();
				return;
			case Keys.N:
				ToggleShowNotations();
				return;
			case Keys.Home:
				MoveToLiveItem(InItem, KeyDirection.FirstOne);
				return;
			case Keys.Prior:
				MoveToLiveItem(InItem, KeyDirection.PrevOne);
				return;
			default:
				num = ((KeyCode != Keys.Tab) ? 1 : 0);
				break;
			case Keys.Next:
				num = 0;
				break;
			}
			if (num == 0)
			{
				MoveToLiveItem(InItem, KeyDirection.NextOne);
				return;
			}
			int num2;
			switch (KeyCode)
			{
			case Keys.End:
				MoveToLiveItem(InItem, KeyDirection.LastOne);
				return;
			case Keys.Tab:
				MoveToLiveItem(InItem, KeyDirection.NextOne);
				return;
			case Keys.Up:
				MoveToSlideLiveItem(InItem, KeyDirection.PrevOne);
				return;
			case Keys.Left:
				MoveToSlideLiveItem(InItem, KeyDirection.FirstOne);
				return;
			case Keys.Right:
				MoveToSlideLiveItem(InItem, KeyDirection.LastOne);
				return;
			case Keys.Down:
				if (InItem.CurSlide < InItem.TotalSlides || gf.AdvanceNextItem)
				{
					MoveToSlideLiveItem(InItem, KeyDirection.NextOne);
				}
				return;
			case Keys.Space:
				MoveToSlideLiveItem(InItem, KeyDirection.NextOne);
				return;
			case Keys.G:
				if (gf.GapItemOption == GapType.None)
				{
					gf.GapItemOption = gf.AltGapItemOption;
					gf.AltGapItemOption = GapType.None;
				}
				else
				{
					gf.AltGapItemOption = gf.GapItemOption;
					gf.GapItemOption = GapType.None;
				}
				return;
			default:
				num2 = ((KeyCode != (Keys)65602) ? 1 : 0);
				break;
			case Keys.W:
				num2 = 0;
				break;
			}
			if (num2 == 0)
			{
				JumpToVerseType(InItem, 103);
				return;
			}
			int num3;
			switch (KeyCode)
			{
			case Keys.B:
				JumpToVerseType(InItem, 100);
				return;
			default:
				num3 = ((KeyCode != (Keys)65616) ? 1 : 0);
				break;
			case Keys.Q:
				num3 = 0;
				break;
			}
			if (num3 == 0)
			{
				JumpToVerseType(InItem, 112);
				return;
			}
			int num4;
			switch (KeyCode)
			{
			case Keys.P:
				JumpToVerseType(InItem, 111);
				return;
			case Keys.E:
				JumpToVerseType(InItem, 101);
				return;
			case Keys.Z:
				QueryShowActive(!LiveScreen.RefStatus());
				return;
			default:
				num4 = ((KeyCode != (Keys)65603) ? 1 : 0);
				break;
			case Keys.T:
				num4 = 0;
				break;
			}
			if (num4 == 0)
			{
				JumpToVerseType(InItem, 102);
				return;
			}
			switch (KeyCode)
			{
			default:
				return;
			case Keys.C:
				KeyCode = Keys.D0;
				break;
			case Keys.D0:
			case Keys.D1:
			case Keys.D2:
			case Keys.D3:
			case Keys.D4:
			case Keys.D5:
			case Keys.D6:
			case Keys.D7:
			case Keys.D8:
			case Keys.D9:
				break;
			}
			if (InItem.SongVerses[(int)(KeyCode - 48)] > 0)
			{
				InItem.CurSlide = InItem.SongVerses[(int)(KeyCode - 48)];
				MoveToSlideLiveItem(InItem, KeyDirection.Refresh);
				KeyCode = Keys.None;
			}
		}

		private void QueryShowActive(bool NewStatus)
		{
			if (NewStatus)
			{
				ShowSlide(ref gf.LiveItem, ImageTransitionControl.TransitionAction.None, DoActiveIndicator: true, RedoBackground: false);
			}
			else
			{
				LiveScreen.StopRef();
			}
		}

		private void JumpToVerseType(SongSettings InItem, int InOtherVerse)
		{
			int num = 1;
			while (true)
			{
				if (num <= InItem.TotalSlides)
				{
					if (InItem.Slide[num, 0] == InOtherVerse)
					{
						break;
					}
					num++;
					continue;
				}
				return;
			}
			InItem.CurSlide = num;
			MoveToSlideLiveItem(InItem, KeyDirection.Refresh);
		}

		private void ToggleShowDataDisplayMode()
		{
			gf.ShowDataDisplayMode = ((gf.ShowDataDisplayMode <= 0) ? 1 : 0);
			ShowSlide(ref gf.LiveItem, ImageTransitionControl.TransitionAction.None);
		}

		private void ToggleShowHeader()
		{
			gf.ShowRunning_ShowSongHeadings = ((gf.ShowRunning_ShowSongHeadings <= 0) ? 1 : 0);
			ShowSlide(ref gf.LiveItem, ImageTransitionControl.TransitionAction.None);
		}

		private void ToggleUseShadowFont()
		{
			gf.ShowRunning_UseShadowFont = ((gf.ShowRunning_UseShadowFont <= 0) ? 1 : 0);
			ShowSlide(ref gf.LiveItem, ImageTransitionControl.TransitionAction.None);
		}

		private void ToggleUseOutlineFont()
		{
			gf.ShowRunning_UseOutlineFont = ((gf.ShowRunning_UseOutlineFont <= 0) ? 1 : 0);
			ShowSlide(ref gf.LiveItem, ImageTransitionControl.TransitionAction.None);
		}

		private void ToggleShowNotations()
		{
			gf.ShowRunning_ShowNotations = ((gf.ShowRunning_ShowNotations <= 0) ? 1 : 0);
			ShowSong(ref gf.LiveItem, 1, ImageTransitionControl.TransitionAction.None);
			RemoteControlLyricsWindow(LyricsWindowAction.Remote_ItemChanged);
		}

		private void ToggleInterlace()
		{
			gf.ShowRunning_ShowInterlace = ((gf.ShowRunning_ShowInterlace <= 0) ? 1 : 0);
			ShowSlide(ref gf.LiveItem, ImageTransitionControl.TransitionAction.None);
		}

		private void ToggleVerticalAlignment()
		{
			if (gf.ShowRunning_ShowVerticalAlign == 0)
			{
				gf.ShowRunning_ShowVerticalAlign = 1;
			}
			else if (gf.ShowRunning_ShowVerticalAlign == 1)
			{
				gf.ShowRunning_ShowVerticalAlign = 2;
			}
			else if (gf.ShowRunning_ShowVerticalAlign == 2)
			{
				gf.ShowRunning_ShowVerticalAlign = 0;
			}
			ShowSlide(ref gf.LiveItem, ImageTransitionControl.TransitionAction.None);
		}

		private void ToggleShowLyrics()
		{
			if (gf.ShowRunning_ShowLyrics == 0)
			{
				gf.ShowRunning_ShowLyrics = 1;
			}
			else if (gf.ShowRunning_ShowLyrics == 1)
			{
				gf.ShowRunning_ShowLyrics = 2;
			}
			else if (gf.ShowRunning_ShowLyrics == 2)
			{
				gf.ShowRunning_ShowLyrics = 0;
			}
			ShowSlide(ref gf.LiveItem, ImageTransitionControl.TransitionAction.None);
		}

		private void FrmLiveShow_MouseDown(object sender, MouseEventArgs e)
		{
			DoMouseDown(e.Button);
		}

		public void DoMouseDown(MouseButtons InBtn)
		{
			if (!gf.DualMonitorMode)
			{
				mouse_btn = InBtn;
				mousedown_timelapse = 0;
				TimerMouseDown.Start();
			}
		}

		private void FrmLiveShow_MouseUp(object sender, MouseEventArgs e)
		{
			DoMouseUp();
		}

		public void DoMouseUp()
		{
			if (gf.DualMonitorMode)
			{
				return;
			}
			if (mousedown_timelapse < 400)
			{
				if (mouse_btn == MouseButtons.Left)
				{
					MoveToSlideLiveItem(gf.LiveItem, KeyDirection.NextOne);
				}
				else if (mouse_btn == MouseButtons.Right)
				{
					MoveToSlideLiveItem(gf.LiveItem, KeyDirection.PrevOne);
				}
			}
			else if (mouse_btn == MouseButtons.Left)
			{
				MoveToLiveItem(gf.LiveItem, KeyDirection.NextOne);
			}
			else if (mouse_btn == MouseButtons.Right)
			{
				MoveToLiveItem(gf.LiveItem, KeyDirection.PrevOne);
			}
			mousedown_timelapse = 0;
			TimerMouseDown.Stop();
		}

		protected override bool ProcessCmdKey(ref System.Windows.Forms.Message msg, Keys keyData)
		{
			ItemKeyPressed(gf.LiveItem, keyData);
			return base.ProcessCmdKey(ref msg, keyData);
		}

		private bool ShowSlide(ref SongSettings InItem, ImageTransitionControl.TransitionAction TransitionAction)
		{
			return ShowSlide(ref InItem, TransitionAction, DoActiveIndicator: false, RedoBackground: false);
		}

		private bool ShowSlide(ref SongSettings InItem, ImageTransitionControl.TransitionAction TransitionAction, bool DoActiveIndicator, bool RedoBackground)
		{
			if (InItem.Type == "P")
			{
				if ((gf.DualMonitorMode && gf.NoPowerpointPanelOverlay && InItem.Type == "P") || (gf.DualMonitorMode && gf.LiveCamNoPanelOverlay && gf.ShowLiveCam))
				{
					SetShowWindow(max: false);
				}
				else
				{
					SetShowWindow(max: true);
					
				}
				gf.LivePP.ImplementPowerpointSlideMovement(ref InItem.CurSlide, InItem.TotalSlides, (OfficeLibKeys)Keys.None, InItem.CurSlide);
				if (gf.ShowLiveCam)
				{
					gf.SetTransparentBackground(InItem, ref LiveScreen);
				}
				gf.DrawText(ref InItem, ref LiveScreen, InItem.LyricsAndNotationsList, DoActiveIndicator: false, ClearAll: false);
				RemoteControlLyricsWindow(LyricsWindowAction.Remote_LyricsChanged);
				return true;
			}
			if (TransitionAction == ImageTransitionControl.TransitionAction.AsStored)
			{
			}
			if ((gf.DualMonitorMode && gf.NoMediaPanelOverlay && InItem.Type == "M") || (gf.DualMonitorMode && gf.LiveCamNoPanelOverlay && gf.ShowLiveCam))
			{
				SetShowWindow(max: false);
			}
			else
			{
				SetShowWindow(max: true);
			}
			InItem.TotalItems = gf.TotalWorshipListItems;
			bool flag = gf.ShowDBSlide(ref InItem, ref LiveScreen, DoActiveIndicator, TransitionAction, RedoBackground);
			if (!flag)
			{
				gf.ResetPictureBox(ref InItem, ref LiveScreen, GapType.Default, TransitionAction);
			}
			RemoteControlLyricsWindow(LyricsWindowAction.Remote_LyricsChanged);
			return flag;
		}

		private void ShowSong(ref SongSettings InItem, int StartingSlide, ImageTransitionControl.TransitionAction TransitionAction)
		{
			InItem.CurSlide = StartingSlide;
			RefreshSlidesFonts(ref InItem, TransitionAction);
		}

		private bool RefreshSlidesFonts(ref SongSettings InItem, ImageTransitionControl.TransitionAction TransitionAction)
		{
			gf.FormatText(ref InItem, gf.PanelBackColour, gf.PanelBackColourTransparent, gf.PanelTextColour, gf.PanelTextColourAsRegion1, InItem.UseDefaultFormat);
			gf.FormatDisplayLyrics(ref InItem, PrepareSlides: true, UseStoredSequence: true);
			gf.DisplaySlidesFormattedLyrics(ref InItem, ref gf.tbLyricsMonitorSpace, ScrollToCaret: true, gf.LMShowNotations);
			return ShowSlide(ref InItem, TransitionAction);
		}

		private void MoveToLiveItem(SongSettings InItem, KeyDirection InIndex)
		{
			MoveToLiveItem(InItem, InIndex, 0);
		}

		private void MoveToLiveItem(SongSettings InItem, KeyDirection InDirection, int SlideNo)
		{
			gf.Launch_StartPresAt = (gf.DualMonitorMode ? gf.Launch_StartPresAt : gf.StartPresAt);
			switch (InDirection)
			{
			case KeyDirection.FirstOne:
				LoadWorshipListItemToLive(1, SlideNo, ImageTransitionControl.TransitionAction.AsStored);
				break;
			case KeyDirection.PrevOne:
				if (gf.Launch_StartPresAt <= gf.TotalWorshipListItems)
				{
					LoadWorshipListItemToLive(gf.AdHocItemPresent ? gf.Launch_StartPresAt : (gf.Launch_StartPresAt - ((!(InItem.Type == "G")) ? 1 : 0)), SlideNo, ImageTransitionControl.TransitionAction.AsStored);
				}
				break;
			case KeyDirection.NextOne:
			{
				if (gf.GapItemOption == GapType.None)
				{
					LoadWorshipListItemToLive(gf.Launch_StartPresAt + 1, SlideNo, ImageTransitionControl.TransitionAction.AsStored);
					break;
				}
				if (gf.Launch_StartPresAt <= gf.TotalWorshipListItems && (gf.Launch_StartPresAt == 0 || (InItem.Type == "G" && gf.Launch_StartPresAt != gf.TotalWorshipListItems)))
				{
					LoadWorshipListItemToLive(gf.Launch_StartPresAt + 1, SlideNo, ImageTransitionControl.TransitionAction.AsStored);
					break;
				}
				string InTitle = "";
				LoadItem(ref InItem, "G1", "", 0, ref InTitle, ImageTransitionControl.TransitionAction.None, ReLoadIfCaptureDevice: false);
				break;
			}
			case KeyDirection.LastOne:
				LoadWorshipListItemToLive(gf.TotalWorshipListItems, SlideNo, ImageTransitionControl.TransitionAction.AsStored);
				break;
			default:
				LoadWorshipListItemToLive(gf.Launch_StartPresAt, SlideNo, ImageTransitionControl.TransitionAction.AsStored);
				break;
			}
		}

		private void OldDoGapItem(SongSettings InItem)
		{
		}

		private bool LoadWorshipListItemToLive(int Selecteditem, int SlideNo, ImageTransitionControl.TransitionAction TransitionAction)
		{
			return LoadWorshipListItemToLive(Selecteditem, SlideNo, TransitionAction, ReLoadIfCaptureDevice: false);
		}

		private bool LoadWorshipListItemToLive(int Selecteditem, int SlideNo, ImageTransitionControl.TransitionAction TransitionAction, bool ReLoadIfCaptureDevice)
		{
			if (gf.AdHocItemPresent)
			{
				if (Selecteditem > 0)
				{
					gf.AdHocItemPresent = false;
				}
				if (Selecteditem > gf.TotalWorshipListItems)
				{
					Selecteditem = gf.TotalWorshipListItems;
				}
			}
			else
			{
				if (gf.TotalWorshipListItems == 0)
				{
					return false;
				}
				if (Selecteditem < 1)
				{
					Selecteditem = 1;
				}
				else if (Selecteditem > gf.TotalWorshipListItems)
				{
					Selecteditem = gf.TotalWorshipListItems;
				}
			}
			string inIDString = gf.WorshipSongs[Selecteditem, 0];
			string inFormatString = gf.WorshipSongs[Selecteditem, 4];
			string InTitle = gf.WorshipSongs[Selecteditem, 2];
			gf.LiveItem.CurItemNo = Selecteditem;
			gf.StartPresAt = (gf.AdHocItemPresent ? gf.StartPresAt : gf.LiveItem.CurItemNo);
			gf.LiveItem.Source = ((gf.LiveItem.CurItemNo > 0) ? ItemSource.WorshipList : gf.OutputItem.Source);
			gf.LiveItem.OutputStyleScreen = true;
			gf.LiveItem.AtLiveScreen = true;
			LoadItem(ref gf.LiveItem, inIDString, inFormatString, SlideNo, ref InTitle, TransitionAction, ReLoadIfCaptureDevice);
			return true;
		}

		private void MoveToSlideLiveItem(SongSettings InItem, KeyDirection InDirection)
		{
			if (gf.AdvanceNextItem)
			{
				if (InDirection == KeyDirection.PrevOne)
				{
					if (InItem.Type == "G")
					{
						MoveToLiveItem(InItem, KeyDirection.Refresh, 30000);
						return;
					}
					if (InItem.CurItemNo > 1 && InItem.CurSlide < 2)
					{
						MoveToLiveItem(InItem, KeyDirection.PrevOne, 30000);
						return;
					}
				}
				else if (InDirection == KeyDirection.NextOne && InItem.CurItemNo < gf.TotalWorshipListItems && InItem.CurSlide >= InItem.TotalSlides)
				{
					if (InItem.Type == "P")
					{
						int num = gf.LivePP.ImplementPowerpointSlideMovement(ref InItem.CurSlide, InItem.TotalSlides, (OfficeLibKeys)gf.ReMapKeyDirectionToPowerpoint(InDirection));
						if (num > 0)
						{
							gf.DrawText(ref InItem, ref LiveScreen, InItem.LyricsAndNotationsList, DoActiveIndicator: false, ClearAll: false);
							return;
						}
					}
					MoveToLiveItem(InItem, KeyDirection.NextOne, 0);
					return;
				}
			}
			if (gf.ShowRunning & (InItem.Type == "P"))
			{
				gf.LivePP.ImplementPowerpointSlideMovement(ref InItem.CurSlide, InItem.TotalSlides, (OfficeLibKeys)gf.ReMapKeyDirectionToPowerpoint(InDirection));
				gf.DrawText(ref InItem, ref LiveScreen, InItem.LyricsAndNotationsList, DoActiveIndicator: false, ClearAll: false);
				return;
			}
			switch (InDirection)
			{
			case KeyDirection.FirstOne:
				InItem.CurSlide = 1;
				break;
			case KeyDirection.PrevOne:
				if (InItem.CurSlide > 2)
				{
					InItem.CurSlide--;
				}
				else
				{
					InItem.CurSlide = 1;
				}
				break;
			case KeyDirection.NextOne:
				if (InItem.CurSlide < InItem.TotalSlides)
				{
					InItem.CurSlide++;
				}
				else if (gf.GapItemOption == GapType.None)
				{
					InItem.CurSlide = InItem.TotalSlides;
				}
				else
				{
					MoveToLiveItem(InItem, KeyDirection.NextOne, 0);
				}
				break;
			case KeyDirection.LastOne:
				InItem.CurSlide = InItem.TotalSlides;
				break;
			}
			ShowSlide(ref InItem, ImageTransitionControl.TransitionAction.AsStored);
		}

		private void LoadItem(ref SongSettings InItem, string InIDString, string InFormatString, int StartingSlide, ref string InTitle, ImageTransitionControl.TransitionAction TransitionAction, bool ReLoadIfCaptureDevice)
		{
			Stop_TimerRotate();
			if (gf.RestartItemActioned)
			{
				gf.RestartCurrentItem = false;
			}
			else
			{
				gf.RestartItemActioned = true;
				gf.RestartCurrentItem = true;
			}
			string text = DataUtil.Left(InIDString, 1);
			string prevTitle = "";
			string nextTitle = "";
			bool flag = false;
			if (gf.TotalWorshipListItems > 0)
			{
				int num = -1;
				int num2 = -1;
				if (InItem.CurItemNo == 0)
				{
					num = gf.StartPresAt;
					num2 = gf.StartPresAt + 1;
				}
				else
				{
					num = gf.StartPresAt - 1;
					num2 = gf.StartPresAt + 1;
				}
				if (num < 1 && InItem.CurItemNo == 0)
				{
					num = 1;
				}
				if (num2 > gf.TotalWorshipListItems)
				{
					num2 = ((InItem.CurItemNo != 0) ? (-1) : gf.TotalWorshipListItems);
				}
				if (num == num2 && num == 0)
				{
					num = -1;
				}
				prevTitle = ((num >= 1) ? gf.RemoveMusicSym(gf.WorshipSongs[num, 2]) : "");
				nextTitle = ((num2 >= 1) ? gf.RemoveMusicSym(gf.WorshipSongs[num2, 2]) : "");
			}
			gf.InitialiseIndividualData(ref InItem, (text == "G" && Media_NextItemHasSameMedia()) ? GapMedia.SameAsPrevious : GapMedia.SessionMedia, "");
			InItem.PrevTitle = prevTitle;
			InItem.NextTitle = nextTitle;
			gf.LoadIndividualData(ref InItem, InIDString, "", StartingSlide, ref InTitle);
			if (InItem.Type == "I" || text == "G")
			{
				InFormatString = InItem.Format.FormatString;
				TransitionAction = ImageTransitionControl.TransitionAction.AsStored;
			}
			gf.LoadIndividualFormatData(ref InItem, InFormatString);
			if (gf.ShowLiveCam & !FirstItemBeingProcessed)
			{
				if (gf.GetMediaLocation(InItem) != gf.CurrentMediaLocation || gf.CurrentMediaLocation == "")
				{
					ItemMediaChangedSinceLiveCam = true;
					gf.CurrentMediaLocation = "";
				}
				if (text == "P")
				{
					gf.MinimizePowerPointWindows(ref gf.LivePP);
				}
				flag = true;
			}
			else
			{
				ItemMediaChangedSinceLiveCam = true;
			}
			if (text == "P" && !flag)
			{
				SetScreenOnTop(StartTimer: false);
				gf.FormatText(ref InItem, gf.PanelBackColour, gf.PanelBackColourTransparent, gf.PanelTextColour, gf.PanelTextColourAsRegion1, InItem.UseDefaultFormat);
				InItem.TotalSlides = gf.RunPowerpointSong(ref InItem, ref gf.LivePP, StartingSlide);
				ResetMediaSettings();
				gf.SetTransparentBackground(gf.LiveItem, ref LiveScreen);
				InItem.Format.ShowItemTransition = 0;
				InItem.Format.ShowSlideTransition = 0;
				if (gf.ShowLiveCam)
				{
					gf.MinimizePowerPointWindows(ref gf.LivePP);
				}
				else
				{
					gf.DrawText(ref InItem, ref LiveScreen, InItem.LyricsAndNotationsList, DoActiveIndicator: false, ClearAll: false);
				}
				if (gf.DualMonitorMode && gf.NoPowerpointPanelOverlay)
				{
					SetShowWindow(max: false);
				}
				CurItemRotates = false;
				ShowSlide(ref gf.LiveItem, ImageTransitionControl.TransitionAction.None, DoActiveIndicator: false, RedoBackground: true);
				RemoteControlMediaPlayerWindow(MediaPlayerWindowAction.Remote_ClearScreen);
				gf.tbLyricsMonitorSpace.Text = "";
				RemoteControlLyricsWindow(LyricsWindowAction.Remote_ItemChanged);
				SetScreenOnTop(StartTimer: true);
				return;
			}
			int num3;
			switch (text)
			{
			default:
				num3 = ((!(text == "G")) ? 1 : 0);
				break;
			case "D":
			case "B":
			case "T":
			case "I":
			case "W":
			case "M":
				num3 = 0;
				break;
			}
			if (num3 != 0)
			{
				return;
			}
			MediaBackgroundStyle mediaBackgroundStyle = gf.GetMediaBackgroundType(InItem, UpdateVariables: true);
			if (InItem.Format.MediaOption == 3 && ReLoadIfCaptureDevice)
			{
				mediaBackgroundStyle = ((!gf.MediaCurrentItemIsVideo) ? MediaBackgroundStyle.Audio : MediaBackgroundStyle.Video);
			}
			gf.FormatText(ref InItem, gf.PanelBackColour, gf.PanelBackColourTransparent, gf.PanelTextColour, gf.PanelTextColourAsRegion1, InItem.UseDefaultFormat);
			gf.FormatDisplayLyrics(ref InItem, PrepareSlides: true, UseStoredSequence: true);
			gf.DisplaySlidesFormattedLyrics(ref InItem, ref gf.tbLyricsMonitorSpace, ScrollToCaret: true, gf.LMShowNotations);
			if (flag)
			{
				if (mediaBackgroundStyle != MediaBackgroundStyle.SameAsPrevious)
				{
					ItemMediaChangedSinceLiveCam = true;
				}
				return;
			}
			MediaBackgroundStyle mediaBackgroundStyle2 = MediaBackgroundStyle.None;
			switch (mediaBackgroundStyle)
			{
			case MediaBackgroundStyle.Audio:
				mediaBackgroundStyle2 = RemoteControlMediaPlayerWindow(MediaPlayerWindowAction.Remote_LoadItem);
				break;
			case MediaBackgroundStyle.Video:
				mediaBackgroundStyle2 = RemoteControlMediaPlayerWindow(MediaPlayerWindowAction.Remote_LoadItem);
				break;
			case MediaBackgroundStyle.SameAsPrevious:
				mediaBackgroundStyle2 = ((!gf.MediaCurrentItemIsVideo) ? MediaBackgroundStyle.Audio : MediaBackgroundStyle.Video);
				RemoteControlMediaPlayerWindow(MediaPlayerWindowAction.Remote_ItemPlayingStatus);
				if (!CurMediaPlayingStatus)
				{
					RemoteControlMediaPlayerWindow(MediaPlayerWindowAction.Remote_LoadItem);
				}
				break;
			default:
				ResetMediaSettings();
				RemoteControlMediaPlayerWindow(MediaPlayerWindowAction.Remote_StopItem);
				mediaBackgroundStyle2 = MediaBackgroundStyle.None;
				break;
			}
			MediaBackgroundStyle mediaBackgroundStyle3 = mediaBackgroundStyle2;
			if (mediaBackgroundStyle3 == MediaBackgroundStyle.Video)
			{
				gf.MediaCurrentItemIsVideo = true;
				InItem.Format.MediaTransparent = true;
			}
			else
			{
				gf.MediaCurrentItemIsVideo = false;
				InItem.Format.MediaTransparent = false;
			}
			if (gf.ShowLiveCam)
			{
				gf.MinimizePowerPointWindows(ref gf.LivePP);
			}
			else
			{
				ShowSlide(ref InItem, TransitionAction, DoActiveIndicator: false, RedoBackground: true);
			}
			RemoteControlLyricsWindow(LyricsWindowAction.Remote_ItemChanged);
			if (InItem.Format.MediaTransparent)
			{
				gf.MinimizePowerPointWindows(ref gf.LivePP);
			}
			tempRotateTimings = "";
			if (gf.AutoRotateOn)
			{
				RemoteControlMediaPlayerWindow(MediaPlayerWindowAction.Remote_ResumeItem);
			}
			else if (gf.RestartCurrentItem && mediaBackgroundStyle2 != 0)
			{
				RemoteControlMediaPlayerWindow(MediaPlayerWindowAction.Remote_ResumeItemFromStart);
			}
			int rotateStyle = InItem.RotateStyle;
			if (rotateStyle == 1)
			{
				if (InItem.RotateGap > 0)
				{
					Start_ItemRotate(InItem.RotateGap, "", mediaBackgroundStyle2);
				}
				else
				{
					CurItemRotates = false;
				}
			}
			else if (InItem.RotateTimings != "" || InItem.RotateTotal >= 0)
			{
				Start_ItemRotate(InItem.RotateGap, InItem.RotateTimings, mediaBackgroundStyle2);
			}
			else
			{
				CurItemRotates = false;
			}
			if (tempRotateTimings != "" && FirstItemLoaded)
			{
				InItem.CurSlide = 1;
			}
			FirstItemLoaded = false;
		}

		private bool Media_NextItemHasSameMedia()
		{
			if (gf.CurrentMediaLocation == "" || gf.LiveItem.CurItemNo == gf.TotalWorshipListItems || gf.StartPresAt == gf.TotalWorshipListItems)
			{
				return false;
			}
			try
			{
				int num = gf.StartPresAt + 1;
				string inTitle = gf.WorshipSongs[num, 2];
				string text = gf.WorshipSongs[num, 1];
				string Title = "";
				string FormatString = gf.WorshipSongs[num, 4];
				if (text == "D")
				{
					Title = gf.LookupDBTitle2(DataUtil.StringToInt(DataUtil.Mid(gf.WorshipSongs[num, 0], 1)));
				}
				else if (text == "I")
				{
					gf.GetTitle2AndFormatFromInfoFile(DataUtil.Mid(gf.WorshipSongs[num, 0], 1), ref Title, ref FormatString);
				}
				int inMediaOption = DataUtil.StringToInt(gf.ExtractHeaderInfo(FormatString, 50, '>'));
				bool inUseDefaultFormat = (FormatString == "") ? true : false;
				string inMediaLocation = gf.ExtractHeaderInfo(FormatString, 51, '>');
				int inMediaCaptureDeviceNumber = DataUtil.StringToInt(gf.ExtractHeaderInfo(FormatString, 55, '>'));
				string mediaLocation = gf.GetMediaLocation(inMediaOption, inTitle, Title, inUseDefaultFormat, text, inMediaLocation, inMediaCaptureDeviceNumber);
				if (gf.CurrentMediaLocation == mediaLocation)
				{
					return true;
				}
				return false;
			}
			catch
			{
				return false;
			}
		}

		private void ResetMediaSettings()
		{
			gf.CurrentMediaLocation = "";
			gf.CurrentMediaIsVideo = false;
			gf.MediaNotifyRepeatItem = false;
		}

		private void Start_ItemRotate(int InRotateGap, string InRotateTimings, MediaBackgroundStyle MediaBackground)
		{
			tempRotateTimings = InRotateTimings;
			if (gf.LiveItem.RotateStyle == 2)
			{
				ItemRotationNextTiming = DataUtil.StringToInt(DataUtil.ExtractOneInfo(ref tempRotateTimings, ';', RemoveExtract: true, MinusOneIfBlank: false));
			}
			else
			{
				ItemRotationNextTiming = gf.LiveItem.RotateGap;
			}
			ItemRotationNextSlideNumber = 1;
			gf.MediaDoRotate = ((MediaBackground != 0) ? true : false);
			if (gf.LiveItem.RotateStyle == 2)
			{
				if (gf.LiveItem.RotateTotal == 0)
				{
					if (gf.MediaDoRotate)
					{
						RemoteControlMediaPlayerWindow(MediaPlayerWindowAction.Remote_GetMediaTimings);
						gf.LiveItem.RotateTotal = intLiveMediaDuration;
					}
					else
					{
						int num = 1;
						int num2 = 1;
						while (InRotateTimings.Length > 0)
						{
							num2 = DataUtil.StringToInt(DataUtil.ExtractOneInfo(ref InRotateTimings, ';', RemoveExtract: true, MinusOneIfBlank: false));
							if (num2 > num)
							{
								num = num2;
							}
						}
						gf.LiveItem.RotateTotal = num + 10;
					}
				}
				else
				{
					RemoteControlMediaPlayerWindow(MediaPlayerWindowAction.Remote_GetMediaTimings);
					int num3 = intLiveMediaDuration;
					if (num3 != gf.LiveItem.RotateTotal)
					{
						gf.MediaDoRotate = false;
					}
				}
			}
			gf.MediaLiveItemStartTime = DateTime.Now;
			CurItemRotates = true;
			TimerRotate.Start();
		}

		private void Stop_TimerRotate()
		{
			TimerRotate.Stop();
			tempRotateTimings = "";
		}

		private void TimerRotate_Tick(object sender, EventArgs e)
		{
			if ((gf.AutoRotateOn && CurItemRotates) || gf.RestartCurrentItem)
			{
				if (!gf.MediaDoRotate)
				{
					gf.MediaPlayedLapseTime = DateTime.Now.Subtract(gf.MediaLiveItemStartTime);
				}
				DoRotate();
			}
			else
			{
				TimerRotate.Stop();
			}
		}

		private void DoRotate()
		{
			switch (gf.LiveItem.RotateStyle)
			{
			case 1:
				if (gf.LiveItem.RotateGap < 0)
				{
					TimerRotate.Stop();
				}
				else
				{
					if (!(gf.MediaPlayedLapseTime.TotalSeconds > 0.0) || !(gf.MediaPlayedLapseTime.TotalSeconds >= (double)ItemRotationNextTiming))
					{
						break;
					}
					if (gf.LiveItem.CurSlide < gf.LiveItem.TotalSlides)
					{
						gf.LiveItem.CurSlide++;
						MoveToSlideLiveItem(gf.LiveItem, KeyDirection.Refresh);
						ItemRotationNextTiming += gf.LiveItem.RotateGap;
						if (gf.ShowRunning)
						{
							this.OnMessage(9, "");
						}
						break;
					}
					TimerRotate.Stop();
					int num = ImplementAutoRotateOption();
					if (num >= 0)
					{
						if (num == gf.LiveItem.CurItemNo)
						{
							ItemRotationNextTiming = (int)gf.MediaPlayedLapseTime.TotalSeconds + gf.LiveItem.RotateGap;
							gf.LiveItem.CurSlide = 1;
							MoveToSlideLiveItem(gf.LiveItem, KeyDirection.Refresh);
							if (gf.ShowRunning)
							{
								this.OnMessage(9, "");
							}
							TimerRotate.Start();
							break;
						}
						prevRefMode = gf.ReferenceAlertSource;
						gf.ReferenceAlertSource = ((num != gf.LiveItem.CurItemNo) ? gf.ReferenceAlertSource : 0);
						LoadWorshipListItemToLive(num, 1, ImageTransitionControl.TransitionAction.AsStored);
						gf.MediaLiveItemStartTime = DateTime.Now;
						gf.ReferenceAlertSource = prevRefMode;
						if (gf.ShowRunning)
						{
							this.OnMessage(7, "");
						}
					}
					else if (gf.GapItemOption != 0)
					{
						gf.StartPresAt = (gf.AdHocItemPresent ? gf.StartPresAt : gf.LiveItem.CurItemNo);
						gf.Launch_StartPresAt = gf.StartPresAt;
						MoveToLiveItem(gf.LiveItem, KeyDirection.NextOne);
						if (gf.ShowRunning)
						{
							this.OnMessage(8, "");
						}
					}
				}
				break;
			case 2:
				if ((!(gf.MediaPlayedLapseTime.TotalSeconds > 0.0) || !(gf.MediaPlayedLapseTime.TotalSeconds >= (double)ItemRotationNextTiming)) && !(gf.MediaPlayedLapseTime.TotalSeconds >= (double)gf.LiveItem.RotateTotal))
				{
					break;
				}
				if (gf.MediaPlayedLapseTime.TotalSeconds >= (double)gf.LiveItem.RotateTotal)
				{
					TimerRotate.Stop();
					int num = ImplementAutoRotateOption();
					if (num >= 0)
					{
						prevRefMode = gf.ReferenceAlertSource;
						gf.ReferenceAlertSource = ((num != gf.LiveItem.CurItemNo) ? gf.ReferenceAlertSource : 0);
						if (gf.MediaLengthAsRotateLength)
						{
							RemoteControlMediaPlayerWindow(MediaPlayerWindowAction.Remote_StopItem);
						}
						LoadWorshipListItemToLive(num, 1, ImageTransitionControl.TransitionAction.AsStored);
						gf.MediaLiveItemStartTime = DateTime.Now;
						gf.ReferenceAlertSource = prevRefMode;
						if (gf.ShowRunning)
						{
							this.OnMessage(7, "");
						}
					}
					else if (gf.GapItemOption != 0)
					{
						gf.StartPresAt = (gf.AdHocItemPresent ? gf.StartPresAt : gf.LiveItem.CurItemNo);
						gf.Launch_StartPresAt = gf.StartPresAt;
						MoveToLiveItem(gf.LiveItem, KeyDirection.NextOne);
						if (gf.ShowRunning)
						{
							this.OnMessage(8, "");
						}
					}
					break;
				}
				if (ItemRotationNextSlideNumber < gf.LiveItem.TotalSlides && ItemRotationNextTiming > 0)
				{
					ItemRotationNextSlideNumber++;
					gf.LiveItem.CurSlide = ItemRotationNextSlideNumber;
					MoveToSlideLiveItem(gf.LiveItem, KeyDirection.Refresh);
					if (gf.ShowRunning)
					{
						this.OnMessage(9, "");
					}
				}
				ItemRotationNextTiming = DataUtil.StringToInt(DataUtil.ExtractOneInfo(ref tempRotateTimings, ';', RemoveExtract: true, MinusOneIfBlank: false));
				if (ItemRotationNextTiming <= 0)
				{
					ItemRotationNextTiming = gf.LiveItem.RotateTotal;
				}
				break;
			}
		}

		private int ImplementAutoRotateOption()
		{
			int num = -1;
			if (gf.RestartCurrentItem)
			{
				return -1;
			}
			if (gf.AutoRotateOn)
			{
				switch (gf.AutoRotateStyle)
				{
				case 0:
					return -1;
				case 1:
					return (!gf.AdHocItemPresent) ? gf.StartPresAt : 0;
				case 2:
					num = Rotate_FindNextItem(GetPreviousIfNoNext: false);
					if (num == gf.LiveItem.CurItemNo)
					{
						return -1;
					}
					gf.StartPresAt = num;
					return gf.StartPresAt;
				default:
					num = Rotate_FindNextItem(GetPreviousIfNoNext: true);
					if (num == gf.LiveItem.CurItemNo)
					{
						return gf.LiveItem.CurItemNo;
					}
					gf.StartPresAt = num;
					return gf.StartPresAt;
				}
			}
			return -1;
		}

		private int Rotate_FindNextItem(bool GetPreviousIfNoNext)
		{
			if (gf.LiveItem.CurItemNo == gf.TotalWorshipListItems || gf.StartPresAt == gf.TotalWorshipListItems)
			{
				if (GetPreviousIfNoNext)
				{
					return Rotate_FindPreviousItem(gf.LiveItem.CurItemNo);
				}
				return gf.LiveItem.CurItemNo;
			}
			try
			{
				int num = gf.StartPresAt + 1;
				int itemRotateResult = gf.GetItemRotateResult(gf.WorshipSongs[num, 0]);
				if (itemRotateResult > 0)
				{
					if (itemRotateResult == 2)
					{
						try
						{
						}
						catch
						{
						}
					}
					return gf.StartPresAt + 1;
				}
				if (gf.AdHocItemPresent && GetPreviousIfNoNext)
				{
					int num2 = gf.StartPresAt;
					try
					{
						num2 = gf.GetItemRotateResult(gf.WorshipSongs[gf.StartPresAt, 0]);
					}
					catch
					{
					}
					int result = Rotate_FindPreviousItem(gf.StartPresAt);
					if (num2 < 1)
					{
						return gf.LiveItem.CurItemNo;
					}
					int num3 = 0;
					return result;
				}
			}
			catch
			{
			}
			if (GetPreviousIfNoNext)
			{
				return Rotate_FindPreviousItem(gf.LiveItem.CurItemNo);
			}
			return gf.LiveItem.CurItemNo;
		}

		private int Rotate_FindPreviousItem(int InItemNo)
		{
			if (InItemNo <= 1)
			{
				return InItemNo;
			}
			try
			{
				int num = InItemNo - 1;
				int itemRotateResult = gf.GetItemRotateResult(gf.WorshipSongs[num, 0]);
				if (itemRotateResult > 0)
				{
					if (itemRotateResult == 2)
					{
						try
						{
							RemoteControlMediaPlayerWindow(MediaPlayerWindowAction.Remote_StopItem);
						}
						catch
						{
						}
					}
					return Rotate_FindPreviousItem(InItemNo - 1);
				}
			}
			catch
			{
			}
			return InItemNo;
		}

		private void FrmLaunchShow_VisibleChanged(object sender, EventArgs e)
		{
			if (base.Visible)
			{
				if (FormFirstLoad)
				{
					FormFirstLoad = false;
				}
				else
				{
					InitForm();
				}
				Activate();
			}
		}

		private MediaBackgroundStyle RemoteControlMediaPlayerWindow(MediaPlayerWindowAction InAction)
		{
			MediaBackgroundStyle result = MediaBackgroundStyle.None;
			try
			{
				switch (InAction)
				{
				case MediaPlayerWindowAction.Show:
					MediaPlayerWindow.Show();
					break;
				case MediaPlayerWindowAction.SendToBack:
					MediaPlayerWindow.SendToBack();
					break;
				case MediaPlayerWindowAction.Remote_StopShow:
					MediaPlayerWindow.Remote_StopShow();
					break;
				case MediaPlayerWindowAction.Remote_ClearScreen:
					MediaPlayerWindow.Remote_ClearScreen();
					break;
				case MediaPlayerWindowAction.Remote_LoadItem:
					result = (MediaBackgroundStyle)MediaPlayerWindow.Remote_LoadItem();
					break;
				case MediaPlayerWindowAction.Remote_ResumeItem:
					result = (MediaBackgroundStyle)MediaPlayerWindow.Remote_ResumeItem();
					break;
				case MediaPlayerWindowAction.Remote_RepeatItem:
					result = (MediaBackgroundStyle)MediaPlayerWindow.Remote_RepeatItem();
					break;
				case MediaPlayerWindowAction.Remote_ResumeItemFromStart:
					MediaPlayerWindow.Remote_ResumeItemFromStart();
					break;
				case MediaPlayerWindowAction.Remote_PauseItem:
					MediaPlayerWindow.Remote_PauseItem();
					break;
				case MediaPlayerWindowAction.Remote_StopItem:
					MediaPlayerWindow.Remote_StopItem();
					break;
				case MediaPlayerWindowAction.Remote_LoadLiveCam:
					result = (MediaBackgroundStyle)MediaPlayerWindow.Remote_LoadLiveCam();
					break;
				case MediaPlayerWindowAction.Remote_UpdateLiveCam:
					result = (MediaBackgroundStyle)MediaPlayerWindow.Remote_UpdateLiveCam();
					break;
				case MediaPlayerWindowAction.Remote_RefreshMediaWindow:
					MediaPlayerWindow.Remote_RefreshMediaWindow();
					break;
				case MediaPlayerWindowAction.Remote_SendScreenToBack:
					MediaPlayerWindow.Remote_SendScreenToBack();
					break;
				case MediaPlayerWindowAction.Remote_GetMediaTimings:
					MediaPlayerWindow.Remote_GetMediaTimings(ref LiveMediaDuration, ref LiveMediaPosition, ref intLiveMediaDuration, ref intLiveMediaPosition);
					break;
				case MediaPlayerWindowAction.Remote_ItemPlayingStatus:
					CurMediaPlayingStatus = MediaPlayerWindow.Remote_ItemPlayingStatus();
					break;
				case MediaPlayerWindowAction.Remote_PausePlayItem:
					MediaPlayerWindow.Remote_PausePlayItem();
					break;
				}
			}
			catch
			{
				try
				{
					MediaPlayerWindow = new FrmLaunchMediaPlayer();
					MediaPlayerWindow.OnMessage += MediaPlayerWindow_OnMessage;
					result = RemoteControlMediaPlayerWindow(InAction);
				}
				catch
				{
				}
			}
			return result;
		}

		private void RemoteControlLyricsWindow(LyricsWindowAction InAction)
		{
			//if (gf.LyricsMonitorNumber > 0 || gf.LMSelectAutoOption > 0)//gf.GetSecondryMonitorIndex()
			if (gf.LyricsMonitorName == gf.GetSecondryMonitorName() || gf.LMSelectAutoOption > 0)
			{
				try
				{
					switch (InAction)
					{
						case LyricsWindowAction.Show:
							LyricsWindow.Show();
							break;
						case LyricsWindowAction.Remote_StopShow:
							LyricsWindow.Remote_StopShow();
							break;
						case LyricsWindowAction.Remote_ItemChanged:
							LyricsWindow.Remote_ItemChanged();
							break;
						case LyricsWindowAction.Remote_LyricsChanged:
							LyricsWindow.Remote_LyricsChanged();
							break;
						case LyricsWindowAction.Remote_NotationsChanged:
							LyricsWindow.Remote_NotationsChanged();
							break;
						case LyricsWindowAction.Remote_WorshipListChanged:
							LyricsWindow.Remote_WorshipListChanged();
							break;
						case LyricsWindowAction.Remote_LyricsAlertChanged:
							LyricsWindow.Remote_LyricsAlertChanged();
							break;
					}
				}
				catch
				{
					try
					{
						LyricsWindow = new FrmLyricsScreen();
						LyricsWindow.OnMessage += LyricsWindow_OnMessage;
						RemoteControlLyricsWindow(InAction);
					}
					catch
					{
					}
				}
			}
		}

		//private void RemoteControlLyricsWindow(LyricsWindowAction InAction)
		//{
		//	//if (gf.LyricsMonitorNumber > 0 || gf.LMSelectAutoOption > 0)//gf.GetSecondryMonitorIndex()
		//	if (gf.LyricsMonitorNumber == gf.GetSecondryMonitorIndex() || gf.LMSelectAutoOption > 0)
		//	{
		//		try
		//		{
		//			switch (InAction)
		//			{
		//				case LyricsWindowAction.Show:
		//					LyricsWindow.Show();
		//					break;
		//				case LyricsWindowAction.Remote_StopShow:
		//					LyricsWindow.Remote_StopShow();
		//					break;
		//				case LyricsWindowAction.Remote_ItemChanged:
		//					LyricsWindow.Remote_ItemChanged();
		//					break;
		//				case LyricsWindowAction.Remote_LyricsChanged:
		//					LyricsWindow.Remote_LyricsChanged();
		//					break;
		//				case LyricsWindowAction.Remote_NotationsChanged:
		//					LyricsWindow.Remote_NotationsChanged();
		//					break;
		//				case LyricsWindowAction.Remote_WorshipListChanged:
		//					LyricsWindow.Remote_WorshipListChanged();
		//					break;
		//				case LyricsWindowAction.Remote_LyricsAlertChanged:
		//					LyricsWindow.Remote_LyricsAlertChanged();
		//					break;
		//			}
		//		}
		//		catch
		//		{
		//			try
		//			{
		//				LyricsWindow = new FrmLyricsScreen();
		//				LyricsWindow.OnMessage += LyricsWindow_OnMessage;
		//				RemoteControlLyricsWindow(InAction);
		//			}
		//			catch
		//			{
		//			}
		//		}
		//	}
		//}

		private void FrmLaunchShow_Enter(object sender, EventArgs e)
		{
			CursorOverForm = true;
		}

		private void FrmLaunchShow_Leave(object sender, EventArgs e)
		{
			CursorOverForm = false;
		}

		private void GotoNextNonRotateItem()
		{
			int nextNonRotateItem = gf.GetNextNonRotateItem((gf.LiveItem.Type == "G") ? true : false);
			bool flag = false;
			if (gf.GapItemOption == GapType.None)
			{
				if (nextNonRotateItem != gf.StartPresAt)
				{
					gf.StartPresAt = nextNonRotateItem;
					MoveToLiveItem(gf.LiveItem, KeyDirection.Refresh);
				}
			}
			else if (nextNonRotateItem == gf.StartPresAt)
			{
				if (gf.LiveItem.Type != "G")
				{
					flag = true;
				}
			}
			else if (nextNonRotateItem > 1)
			{
				flag = true;
			}
			if (flag)
			{
				gf.StartPresAt = nextNonRotateItem;
				gf.Launch_StartPresAt = gf.StartPresAt;
				string InTitle = "";
				LoadItem(ref gf.LiveItem, "G1", "", 0, ref InTitle, ImageTransitionControl.TransitionAction.None, ReLoadIfCaptureDevice: false);
			}
		}

		private void TimerToFront_Tick(object sender, EventArgs e)
		{
			if (gf.DualMonitorMode)
			{
				TimerToFront.Stop();
			}
			else if (LoadRepaintCount > 8)
			{
				TimerToFront.Stop();
				Focus();
				base.TopMost = false;
			}
			else
			{
				LoadRepaintCount++;
			}
		}
	}
}
