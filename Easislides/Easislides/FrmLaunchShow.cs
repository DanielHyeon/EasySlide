using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Easislides.Module;
using Easislides.Util;
using OfficeLib;

namespace Easislides
{
	public partial class FrmLaunchShow : Form
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

		private bool FormFirstLoad = true;

		private bool StayTopMost = true;

		private MouseButtons mouse_btn;

		private int mousedown_timelapse;

		private string[] InHeaderData = new string[255];

		private ImageTransitionControl LiveScreen = new ImageTransitionControl { Name = "LiveScreen" };

		private int ItemRotationNextTiming = 0;

		private int ItemRotationNextSlideNumber = 0;

		private int prevRefMode = 0;

		private string tempRotateTimings = "";

		private bool FirstItemLoaded = true;

		private bool FirstItemBeingProcessed = true;

		private bool CurItemRotates = false;

		private string LiveMediaDuration = "";

		private string LiveMediaPosition = "";

		private int intLiveMediaDuration = 0;

		private bool CurMediaPlayingStatus = false;

		private int LoadRepaintCount = 0;

		private Cursor LiveCursor;

		//private FrmLaunchMediaPlayer MediaPlayerWindow = new FrmLaunchMediaPlayer();

		//private FrmLyricsScreen LyricsWindow = new FrmLyricsScreen();

		public event Message OnMessage;

		// An end-show request posted from FormClosing that has not run yet (see PostEndShowRequest).
		private bool endShowRequestPending = false;

		//Active �Ǿ� ���� ��쿡��?Active ȣ�� �� �� �ֵ��� ����
		private bool isActivated = false;

				//Active �Ǿ� ���� ��쿡��?Active ȣ��
		protected override void OnActivated(EventArgs e)
		{
			isActivated = true;

			base.OnActivated(e);
		}

		public FrmLaunchShow()
		{
			InitializeComponent();
		}

		private void FrmLaunchShow_Load(object sender, EventArgs e)
		{
			StartInitCompleted = false;
			if (FormFirstLoad)
			{
				Bitmap bitmap = new Bitmap(55, 25);
				Graphics graphics = Graphics.FromImage(bitmap);
				IntPtr hicon = bitmap.GetHicon();
				LiveCursor = new Cursor(hicon);
				Cursor = LiveCursor;
				InitForm();
				LiveScreen.MouseUp += FrmLiveShow_MouseUp;
				LiveScreen.MouseDown += FrmLiveShow_MouseDown;
				//MediaPlayerWindow.OnMessage += MediaPlayerWindow_OnMessage;
				//LyricsWindow.OnMessage += LyricsWindow_OnMessage;
				LiveScreen.Cursor = LiveCursor;
				bitmap.Dispose();
				graphics.Dispose();
			}
			FirstLoadCompleted = true;
			StartInitCompleted = true;
		}

		/// <summary>
		/// True once the initialisation of the current show start has run to the end: Load on the
		/// window's first show, InitForm from FrmLaunchShow_VisibleChanged on every later show. Set to
		/// false when that initialisation begins; hiding the window does not change it. An exception in
		/// either does not leave Show(): WinForms reports it through Application.ThreadException and
		/// Show() returns normally, so Start_Presentation checks this afterwards.
		/// </summary>
		internal bool StartInitCompleted { get; private set; }

		/// <summary>
		/// True once FrmLaunchShow_Load has run to the end. Load runs only once per window, so a window
		/// whose Load failed never gets what only Load does (cursor, mouse handlers).
		/// </summary>
		internal bool FirstLoadCompleted { get; private set; }

		private void InitForm()
		{
			BackColor = Gf.TransparentColour;
			base.TransparencyKey = Gf.TransparentColour;
			LiveScreen.Parent = this;
			LiveScreen.Dock = DockStyle.Fill;
			Gf.SetLiveShowScreenSaverSettings();
			SetShowWindow(max: true);
			TimerSingleScreen.Interval = Gf.AlertGap;
			TimerRemote.Interval = Gf.AlertGap;
			TimerMouseDown.Interval = Gf.AlertGap;
			TimerOpacity.Interval = Gf.AlertGap;
			Gf.LiveItem.Initialise();
			Gf.RestartItemActioned = true;
			Gf.SetDefaultBackScreen(ref LiveScreen);

			//if (Gf.LyricsMonitorNumber > 0 || Gf.LMSelectAutoOption > 0)
			//{
			//    Gf.tbLyricsMonitorSpace.Font = new Font(Gf.tbLyricsMonitorSpace.Font.Name, Gf.DisplayFontSize(Gf.LMMainFontSize, Gf.LM_Width, 1, 1), Gf.tbLyricsMonitorSpace.Font.Style);
			//    try
			//    {
			//        RemoteControlLyricsWindow(LyricsWindowAction.Show);
			//    }
			//    catch
			//    {
			//    }
			//}

			//if (Gf.LyricsMonitorName == DisplayInfo.getSecondryDisplayName() || Gf.LMSelectAutoOption > 0)
			//{
			//	Gf.tbLyricsMonitorSpace.Font = new Font(Gf.tbLyricsMonitorSpace.Font.Name, Gf.DisplayFontSize(Gf.LMMainFontSize, Gf.LM_Width, 1, 1), Gf.tbLyricsMonitorSpace.Font.Style);
			//	try
			//	{
			//		RemoteControlLyricsWindow(LyricsWindowAction.Show);
			//	}
			//	catch
			//	{
			//	}
			//}

			Gf.MediaResetStartTime = true;
			ResetMediaSettings();
			InitMediaWindow();
			FirstItemBeingProcessed = true;
			if (Gf.OutputItem.Type == "G")
			{
				string InTitle = "";
				Gf.LiveItem.CurItemNo = Gf.StartPresAt;
				LoadItem(ref Gf.LiveItem, "G1", "", 0, ref InTitle, ImageTransitionControl.TransitionAction.None, ReLoadIfCaptureDevice: false);
			}
			else
			{
				LoadWorshipListItemToLive((!Gf.AdHocItemPresent) ? Gf.StartPresAt : 0, Gf.OutputItem.CurSlide, ImageTransitionControl.TransitionAction.AsStored);
			}
			if (Gf.MessageAlertRequested)
			{
				Remote_MessageAlertRequested();
			}
			else if (Gf.ParentalAlertRequested)
			{
				Remote_ParentalAlertRequested();
			}
			FirstItemBeingProcessed = false;
			//if (Gf.ShowLiveCam)
			//{
			//	Remote_LiveCamStartStop();
			//}
			if (Gf.DualMonitorMode)
			{
				base.TopMost = true;
				return;
			}
			Gf.HideTaskBar();
			SetScreenOnTop(StartTimer: true);
		}

		private void InitMediaWindow()
		{
			if (Gf.WMP_Present)
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
			Gf.LiveItem.Format.MediaTransparent = true;
			Gf.MediaCurrentItemIsVideo = true;
			ShowSlide(ref Gf.LiveItem, ImageTransitionControl.TransitionAction.None);
			Gf.MinimizePowerPointWindows(ref Gf.LivePP);
		}

		public void Remote_ItemIsNotVideo()
		{
			Gf.MediaCurrentItemIsVideo = false;
			Gf.LiveItem.Format.MediaTransparent = false;
		}

		public void Remote_WorshipListChanged()
		{
			//RemoteControlLyricsWindow(LyricsWindowAction.Remote_WorshipListChanged);
		}

		//public void Remote_LiveCamStartStop()
		//{
		//	if (Gf.ShowLiveCam)
		//	{
		//		CurMediaDoRotate = Gf.MediaDoRotate;
		//		TimerRotate.Stop();
		//		RemoteControlMediaPlayerWindow(MediaPlayerWindowAction.Remote_LoadLiveCam);
		//		ShowSlide(ref Gf.LiveItem, ImageTransitionControl.TransitionAction.None);
		//		ItemMediaChangedSinceLiveCam = false;
		//		Gf.MinimizePowerPointWindows(ref Gf.LivePP);
		//		return;
		//	}
		//	if (Gf.LiveItem.Type == "P")
		//	{
		//		Gf.RunPowerpointSong(ref Gf.LiveItem, ref Gf.LivePP, Gf.LiveItem.CurSlide);
		//	}
		//	if (ItemMediaChangedSinceLiveCam)
		//	{
		//		Remote_SongChanged(ReLoadIfCaptureDevice: true);
		//		return;
		//	}
		//	Gf.MediaLiveItemStartTime = DateTime.Now.Subtract(Gf.MediaPlayedLapseTime);
		//	TimerRotate.Start();
		//	RemoteControlMediaPlayerWindow(MediaPlayerWindowAction.Remote_ResumeItem);
		//	Gf.MediaDoRotate = CurMediaDoRotate;
		//	if (Gf.MediaDoRotate)
		//	{
		//		RemoteControlMediaPlayerWindow(MediaPlayerWindowAction.Remote_GetMediaTimings);
		//		Gf.MediaLiveItemStartTime = DateTime.Now.Subtract(new TimeSpan(0, 0, intLiveMediaPosition));
		//	}
		//	ShowSlide(ref Gf.LiveItem, ImageTransitionControl.TransitionAction.None, DoActiveIndicator: false, RedoBackground: true);
		//}

		//public void Remote_LiveCamUpdate()
		//{
		//	if (Gf.ShowLiveCam)
		//	{
		//		RemoteControlMediaPlayerWindow(MediaPlayerWindowAction.Remote_UpdateLiveCam);
		//	}
		//}

		private void SetShowWindow(bool max)
		{
			if (max)
			{
				base.Left = Gf.LS_Left;
				base.Top = Gf.LS_Top;
				base.Height = Gf.LS_Height;
				base.Width = Gf.LS_Width;
			}
			else
			{
				base.Left = Gf.LS_Left;
				base.Top = Gf.LS_Top;
				base.Height = 0;
				base.Width = 0;
			}
		}
		private string GetMediaOutputMonitorName(SongSettings item)
		{
			if (item != null && item.Format != null && !string.IsNullOrEmpty(item.Format.MediaOutputMonitorName))
			{
				return item.Format.MediaOutputMonitorName;
			}
			if (!string.IsNullOrEmpty(Gf.MediaOutputMonitorName))
			{
				return Gf.MediaOutputMonitorName;
			}
			return Gf.OutputMonitorName;
		}

		/// <summary>
		/// The live PowerPoint item's slide show has ended (found by a slide movement here or in the main form):
		/// this window stops being transparent over it and shows the default background, as for an item whose
		/// slide show did not start.
		/// </summary>
		internal void PaintEndedPowerPointItem()
		{
			if (!Gf.ShowRunning || Gf.LiveItem.Type != "P" || !Gf.LivePowerPointEnded || !IsHandleCreated || IsDisposed)
			{
				return;
			}
			SetShowWindow(max: true);
			Gf.ResetPictureBox(ref Gf.LiveItem, ref LiveScreen, GapType.Default, ImageTransitionControl.TransitionAction.None);
		}

		/// <summary>
		/// The live PowerPoint item's slide show, believed ended, runs again: this window becomes transparent over
		/// it again, with the steps a started item gets in LoadItem (transparent background, window size for the
		/// panel overlay option, the text layer).
		/// </summary>
		internal void ResumeStartedPowerPointItem()
		{
			if (!Gf.ShowRunning || Gf.LiveItem.Type != "P" || Gf.LivePowerPointRunState != Gf.LivePowerPointRun.Started
				|| Gf.LivePP.RunShowLost || !IsHandleCreated || IsDisposed)
			{
				return;
			}
			Gf.SetTransparentBackground(Gf.LiveItem, ref LiveScreen);
			SetShowWindow(max: !(Gf.DualMonitorMode && Gf.NoPowerpointPanelOverlay));
			Gf.DrawText(ref Gf.LiveItem, ref LiveScreen, Gf.LiveItem.LyricsAndNotationsList, DoActiveIndicator: false, ClearAll: false);
		}

		/// <summary>
		/// Puts this window back above the PowerPoint slide show (dual monitor show), without activating it.
		/// The main form runs the slide show a second time for the same item; when that run comes after this
		/// window's own (the item was changed from this window: auto-rotate), the slide show is made topmost
		/// after this window and would hide the alerts and references drawn here.
		/// </summary>
		internal void KeepAboveSlideShow()
		{
			if (!Gf.DualMonitorMode || !StayTopMost || !Visible || !IsHandleCreated)
			{
				return;
			}
			Gf.KeepWindowTopMost(Handle);
		}

		private System.Windows.Forms.Timer lowerSlideShowsTimer;
		private DateTime lowerSlideShowsDeadline;
		private int lowerSlideShowsRunCount;
		private const int LowerSlideShowsPollMs = 40;
		// Longest item transition is 1.5 s; a screen that is not painted must not keep the slide shows up.
		private const int LowerSlideShowsMaxWaitMs = 3000;

		/// <summary>
		/// After a non-PowerPoint item was put on this window (dual monitor show): takes the slide shows the app
		/// made topmost off the top, so the part of the output monitor this window does not cover shows what was
		/// there before the show, not an earlier slide. Done once this window shows the new item completely: while
		/// it is still (partly) transparent, the slide show must stay behind it. The next PowerPoint item raises its
		/// own slide show again; a slide show run before the lowering is due cancels it.
		/// </summary>
		private void LowerSlideShowsWhenItemShown()
		{
			if (!Gf.DualMonitorMode || !PowerPoint.HasRaisedSlideShowWindows)
			{
				return;
			}
			if (lowerSlideShowsTimer == null)
			{
				lowerSlideShowsTimer = new System.Windows.Forms.Timer { Interval = LowerSlideShowsPollMs };
				lowerSlideShowsTimer.Tick += LowerSlideShowsTimer_Tick;
				Disposed += (sender, e) =>
				{
					lowerSlideShowsTimer?.Stop();
					lowerSlideShowsTimer?.Dispose();
					lowerSlideShowsTimer = null;
				};
			}
			lowerSlideShowsRunCount = Gf.SlideShowRunCount;
			lowerSlideShowsDeadline = DateTime.UtcNow.AddMilliseconds(LowerSlideShowsMaxWaitMs);
			lowerSlideShowsTimer.Stop();
			lowerSlideShowsTimer.Start();
		}

		private void LowerSlideShowsTimer_Tick(object sender, EventArgs e)
		{
			if (lowerSlideShowsRunCount != Gf.SlideShowRunCount || !Gf.ShowRunning)
			{
				// A PowerPoint item followed (its slide show is up again), or the show ended (it lowers them).
				lowerSlideShowsTimer.Stop();
				return;
			}
			if (LiveScreen.TransitionPending && DateTime.UtcNow < lowerSlideShowsDeadline)
			{
				return;
			}			lowerSlideShowsTimer.Stop();
			try
			{
				// WM_PAINT comes before WM_TIMER, so the last frame is normally painted already; make sure.
				LiveScreen.Update();
			}
			catch
			{
			}
			// Synchronous: a PowerPoint item that follows raises its window synchronously too, so this lowering
			// can never be applied after that raise.
			Gf.LowerSlideShowWindows("FrmLaunchShow: non-PowerPoint item shown", wait: true);
		}

		private void SetScreenOnTop(bool StartTimer)
		{
			StayTopMost = StartTimer;
			base.TopMost = StayTopMost;
			if (StartTimer && !Gf.DualMonitorMode)
			{
				Cursor.Position = new Point(Gf.LS_Left, Gf.LS_Height - 1);
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

		protected override void WndProc(ref System.Windows.Forms.Message m)
		{
			if (m.Msg == FrmMain.WM_QUERYENDSESSION)
			{
				if (Modal)
				{
					// WinForms ends a modal dialog while it is only asked (DialogResult = Cancel), so a
					// single-monitor show would end even when the shutdown is then cancelled. Answer "yes"
					// without that; WM_ENDSESSION with wParam TRUE closes the window when the session ends.
					m.Result = (IntPtr)1;
					return;
				}
				FrmMain.QueryEndSessionDepth++;
				try
				{
					base.WndProc(ref m);
				}
				finally
				{
					FrmMain.QueryEndSessionDepth--;
				}
				return;
			}
			if (m.Msg == FrmMain.WM_ENDSESSION && m.WParam != IntPtr.Zero)
			{
				FrmMain.SessionEnded = true;
				FrmMain.EndSessionDepth++;
				try
				{
					base.WndProc(ref m);
				}
				finally
				{
					FrmMain.EndSessionDepth--;
				}
				return;
			}
			base.WndProc(ref m);
		}

		private void FrmLaunchShow_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (FrmMain.QueryEndSessionDepth > 0)
			{
				// Asked while Windows queries whether it may end the session (this window's
				// WM_QUERYENDSESSION, or the main form's, which raises FormOwnerClosing on an owned modal
				// show): the shutdown can still be cancelled and the show go on, so nothing is cleaned up
				// here. When the session really ends, OnFormClosed does it.
				return;
			}
			// WinForms keeps WindowsShutDown as the close reason after a cancelled shutdown; outside the
			// query it is an ordinary request from outside (e.g. Task Manager's End task).
			CloseReason reason = (e.CloseReason == CloseReason.WindowsShutDown) ? CloseReason.TaskManagerClosing : e.CloseReason;
			if (!Modal && Gf.ShowRunning && IsSelfCloseReason(reason))
			{
				// Non-modal show (the main form does not wait for this window): a close that does not
				// come from the main form would dispose this window while the main form still shows
				// the show as running. Keep the window and let the main form end the show on its
				// normal path (Remote_EndShow -> GoLive(false) -> StopShow, which hides this window).
				// The request is posted, not called: it runs after this handler has returned.
				e.Cancel = true;
				PostEndShowRequest();
				return;
			}
			FormClosingCleanup();
		}

		protected override void OnFormClosed(FormClosedEventArgs e)
		{
			// The session ends (WM_ENDSESSION with wParam TRUE, to this window or to the main form that owns a
			// modal show): no FormClosing came for this close, or it came at the query and did nothing. The show
			// may have been ended meanwhile (the main form's closing work: StopShow).
			if (FrmMain.EndSessionDepth > 0 && Gf.ShowRunning)
			{
				FormClosingCleanup();
			}
			base.OnFormClosed(e);
		}

		private static bool IsSelfCloseReason(CloseReason reason)
		{
			// Application exit, Windows shutdown and owner / MDI parent closes go through unchanged.
			return reason == CloseReason.UserClosing || reason == CloseReason.TaskManagerClosing || reason == CloseReason.None;
		}

		private void PostEndShowRequest()
		{
			if (endShowRequestPending || !IsHandleCreated)
			{
				return;
			}
			endShowRequestPending = true;
			BeginInvoke((MethodInvoker)delegate
			{
				endShowRequestPending = false;
				// The show may have been stopped from the main form meanwhile: nothing left to end.
				if (Gf.ShowRunning)
				{
					this.OnMessage?.Invoke(10, "");
				}
				// The main form ends the show only in dual monitor mode (Remote_EndShow). If it did
				// not, end it here: a close request must never leave the window open.
				if (Gf.ShowRunning && Visible)
				{
					StopShowAndTellMainForm();
				}
			});
		}

		/// <summary>
		/// Ends the show from this window (Esc outside dual monitor mode, or a close request the main
		/// form did not act on). In a non-modal show the main form is then told, and brings its show
		/// button, status row and worship list marks to "show off" (Remote_ShowStopped); the show is
		/// already stopped by then, so the main form does not stop it a second time.
		/// </summary>
		private void StopShowAndTellMainForm()
		{
			StopShow();
			if (!Modal)
			{
				// A modal show is brought to "off" by Start_Presentation once ShowDialog returns.
				this.OnMessage?.Invoke(11, "");
			}
		}

		private void FormClosingCleanup()
		{
			Gf.ResetShowRunningSettings();
			Gf.MessageAlertLive = false;
			Gf.ParentalAlertLive = false;
			Gf.ShowRunning = false;
			TimerRotate.Stop();
			TimerSingleScreen.Stop();
			TimerRemote.Stop();
			TimerMouseDown.Stop();
			lowerSlideShowsTimer?.Stop();
			if (Modal)
			{
				// Modal show (ShowDialog in Start_Presentation, only when DisplayInfo.SizeLaunchDisplay set
				// single monitor mode: custom mode "as single monitor", or output monitor None): the show
				// window navigated on its own while the main form waited, and Start_Presentation reloads
				// the output from these right after ShowDialog returns.
				// Non-modal show (Show, also on an ordinary one-screen PC): the main form kept OutputItem
				// current itself; LiveItem can be stale (e.g. the list was reordered during the show), so
				// nothing is written back.
				Gf.OutputItem.Type = Gf.LiveItem.Type;
				Gf.OutputItem.CurItemNo = ((Gf.LiveItem.CurItemNo < 0) ? 1 : Gf.LiveItem.CurItemNo);
				Gf.OutputItem.CurSlide = ((Gf.LiveItem.CurSlide < 1) ? 1 : Gf.LiveItem.CurSlide);
			}
			Gf.RestoreScreenSaverSettings();
			Gf.ClearUpPowerpointWindows();
			Gf.RefreshWindowsDesktop();
			Gf.ShowTaskBar();
		}

		public void StopShow()
		{
			try
			{
				RemoteControlMediaPlayerWindow(MediaPlayerWindowAction.Remote_StopShow);
				Gf.DrawText(ref Gf.LiveItem, ref LiveScreen, Gf.LiveItem.LyricsAndNotationsList, DoActiveIndicator: false, ClearAll: true);
			}
			catch
			{
			}
			try
			{
				//RemoteControlLyricsWindow(LyricsWindowAction.Remote_StopShow);
			}
			catch
			{
			}
			FormClosingCleanup();
			Hide();
			Gf.RefreshWindowsDesktop();
		}

		public void Remote_DefaultBackgroundChanged()
		{
			Gf.SetDefaultBackScreen(ref LiveScreen);
			Gf.SetShowBackground(Gf.LiveItem, ref LiveScreen);
			ShowSlide(ref Gf.LiveItem, ImageTransitionControl.TransitionAction.None);
		}

		public void Remote_BackgroundChanged()
		{
			Gf.SetShowBackground(Gf.LiveItem, ref LiveScreen);
			ShowSlide(ref Gf.LiveItem, ImageTransitionControl.TransitionAction.None);
		}

		public void Remote_MoveToItemChanged()
		{
			MoveToLiveItem(Gf.LiveItem, Gf.MainAction_MoveToItemKeyDirection, Gf.OutputItem.CurSlide);
		}

		public void Remote_SongChanged(bool ReLoadIfCaptureDevice)
		{
			if (Gf.OutputItem.Type == "G")
			{
				ShowSlide(ref Gf.LiveItem, ImageTransitionControl.TransitionAction.None);
				if (Gf.LiveItem.Type != "P")
				{
					// This window shows an opaque item now (a PowerPoint item stays transparent).
					LowerSlideShowsWhenItemShown();
				}
			}
			else
			{
				LoadWorshipListItemToLive(Gf.DualMonitorMode ? Gf.OutputItem.CurItemNo : Gf.LiveItem.CurItemNo, Gf.OutputItem.CurSlide, Gf.MainAction_SongChanged_Transaction, ReLoadIfCaptureDevice);
			}
			Gf.LaunchShowUpdateDone = true;
		}

		public void Remote_SlideChanged(int InDirection)
		{
			if (Gf.LiveItem.CurItemNo == Gf.OutputItem.CurItemNo)
			{
				if (Gf.LiveItem.Type == "P")
				{
					Gf.LiveItem.CurSlide = Gf.OutputItem.CurSlide;
					MoveToSlideLiveItem(Gf.LiveItem, KeyDirection.Refresh);
				}
				else
				{
					Gf.LiveItem.CurSlide = Gf.OutputItem.CurSlide;
					MoveToSlideLiveItem(Gf.LiveItem, KeyDirection.Refresh);
				}
			}
			else
			{
				LoadWorshipListItemToLive(Gf.OutputItem.CurItemNo, Gf.OutputItem.CurSlide, ImageTransitionControl.TransitionAction.AsStored);
			}
		}

		public void Remote_SongJumpTo()
		{
			if (Gf.OutputItem.Type == "G")
			{
				string InTitle = "";
				LoadItem(ref Gf.LiveItem, "G1", "", 0, ref InTitle, ImageTransitionControl.TransitionAction.None, ReLoadIfCaptureDevice: false);
			}
			else
			{
				MoveToLiveItem(Gf.LiveItem, KeyDirection.Refresh);
			}
		}

		public void Remote_LiveBlackClearChanged()
		{
			if (Gf.LiveItem.Type == "P")
			{
				Gf.DrawText(ref Gf.LiveItem, ref LiveScreen, Gf.LiveItem.LyricsAndNotationsList, DoActiveIndicator: false, ClearAll: false);
			}
			else
			{
				ShowSlide(ref Gf.LiveItem, Gf.GapItemUseFade ? ImageTransitionControl.TransitionAction.AsFade : ImageTransitionControl.TransitionAction.None);
			}
		}

		public void Remote_FormatChanged()
		{
			if (Gf.LiveItem.Type != "P")
			{
				RefreshSlidesFonts(ref Gf.LiveItem, Gf.MainAction_SongChanged_Transaction);
			}
		}

		public void Remote_PanelChanged()
		{
			if (Gf.LiveItem.Type == "P")
			{
				Gf.DrawText(ref Gf.LiveItem, ref LiveScreen, Gf.LiveItem.LyricsAndNotationsList, DoActiveIndicator: false, ClearAll: false);
			}
			else
			{
				RefreshSlidesFonts(ref Gf.LiveItem, Gf.MainAction_SongChanged_Transaction);
			}
		}

		public void Remote_ChineseChanged()
		{
			Gf.SwitchChineseLyricsNotationListView(ref Gf.LiveItem, Gf.SwitchChinese(ref Gf.LiveItem.CompleteLyrics));
			if (Gf.LiveItem.Type != "P")
			{
				ShowSlide(ref Gf.LiveItem, ImageTransitionControl.TransitionAction.None);
			}
		}

		public void Remote_MessageAlertRequested()
		{
			Gf.MessageAlertRequested = false;
			Gf.AlertSettings(AlertType.Message);
			LiveScreen.StartAlert(Gf.LiveItem, Gf.Alert_OriginalMessage, Gf.AlertTimeRemaining, Gf.Alert_UserFont, Gf.Alert_Scroll, Gf.Alert_Flash, Gf.Alert_Transparent, Gf.Alert_UserFontShadow, Gf.Alert_UserFontOutline, Gf.Alert_TextColour, Gf.Alert_BackColour, Gf.Alert_TextAlign, Gf.Alert_VerticalAlign, Gf.BottomBorderFactor);
		}

		public void Remote_ParentalAlertRequested()
		{
			Gf.ParentalAlertRequested = false;
			Gf.AlertSettings(AlertType.Parental);
			LiveScreen.StartAlert(Gf.LiveItem, Gf.Alert_OriginalMessage, Gf.AlertTimeRemaining, Gf.Alert_UserFont, Gf.Alert_Scroll, Gf.Alert_Flash, Gf.Alert_Transparent, Gf.Alert_UserFontShadow, Gf.Alert_UserFontOutline, Gf.Alert_TextColour, Gf.Alert_BackColour, Gf.Alert_TextAlign, Gf.Alert_VerticalAlign, Gf.BottomBorderFactor);
		}

		public void Remote_ReferenceAlertRequested(bool NewStatus)
		{
			QueryShowActive(NewStatus);
		}

		public void Remote_LyricsAlertRequested()
		{
			Gf.LyricsAlertRequested = false;
			//RemoteControlLyricsWindow(LyricsWindowAction.Remote_LyricsAlertChanged);
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
			if (Gf.ShowLiveCam)
			{
				return "";
			}
			string result = "";
			if (Gf.AutoRotateOn && CurItemRotates)
			{
				LiveMediaPosition = new DateTime(Gf.MediaPlayedLapseTime.Ticks).ToString("mm:ss");
				LiveMediaDuration = new DateTime(new TimeSpan(0, 0, Gf.LiveItem.RotateTotal).Ticks).ToString("mm:ss");
				result = LiveMediaPosition + ((Gf.LiveItem.RotateTotal > 0) ? (" [" + LiveMediaDuration + "]") : "");
			}
			else if (Gf.LiveItem.Type == "M")
			{
				RemoteControlMediaPlayerWindow(MediaPlayerWindowAction.Remote_GetMediaTimings);
				if (LiveMediaDuration != "")
				{
					result = LiveMediaPosition + " [" + LiveMediaDuration + "]";
				}
			}
			return result;
		}

		//public string Remote_MediaItemPausePlay()
		//{
		//	if (Gf.CurrentMediaLocation != "" && !Gf.ShowLiveCam)
		//	{
		//		RemoteControlMediaPlayerWindow(MediaPlayerWindowAction.Remote_PausePlayItem);
		//		Gf.MediaDoRotate = false;
		//	}
		//	return "";
		//}

		private void TimerRemote_Tick(object sender, EventArgs e)
		{
		}

		/// <summary>
		/// daniel v2.2 ����
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		// �Ŀ�����Ʈ�� ������ �������� ���� ���?�̴��?�ڵ� �ϸ� ������ �����?�ȵ�
		//private void TimerSingleScreen_Tick(object sender, EventArgs e)
		//{
		//	base.TopMost = StayTopMost;
		//	if (StayTopMost && Cursor.Position.X >= base.Left && Cursor.Position.X <= base.Left + base.Width && Cursor.Position.Y >= base.Top && Cursor.Position.Y <= base.Top + base.Bottom)
		//	{
		//		Activate();
		//		Cursor.Position = new Point(Gf.LS_Left, Gf.LS_Height - 1);
		//	}
		//}


		// �Ŀ�����Ʈ�� ������ �������� ���� ���?���?�ǵ��� ����
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
			if (!Gf.DualMonitorMode)
			{
				mousedown_timelapse += Gf.AlertGap;
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
			if (Gf.DualMonitorMode)
			{
				Gf.ReMapKeyBoard(ref KeyCode);
				if (KeyCode == Keys.Escape || KeyCode == Keys.Subtract || KeyCode == Keys.OemMinus || KeyCode == Keys.F12)
				{
					this.OnMessage(10, "");
				}
				return;
			}
			Gf.ReMapKeyBoard(ref KeyCode);
			if (KeyCode == Keys.Escape || KeyCode == Keys.Subtract || KeyCode == Keys.OemMinus || KeyCode == Keys.F12)
			{
				//daniel
				//esc Ű�� ������ �����̵� � ���� ��
				StopShowAndTellMainForm();
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
				Gf.ShowLiveBlack = !Gf.ShowLiveBlack;
				ShowSlide(ref Gf.LiveItem, Gf.GapItemUseFade ? ImageTransitionControl.TransitionAction.AsFade : ImageTransitionControl.TransitionAction.None);
				return;
			case Keys.F3:
				Gf.ShowLiveClear = !Gf.ShowLiveClear;
				ShowSlide(ref Gf.LiveItem, Gf.GapItemUseFade ? ImageTransitionControl.TransitionAction.AsFade : ImageTransitionControl.TransitionAction.None);
				return;
			//case Keys.F4:
			//	Gf.ShowLiveCam = !Gf.ShowLiveCam;
			//	Remote_LiveCamStartStop();
			//	return;
			case Keys.F5:
				Gf.RestartItemActioned = false;
				Gf.AutoRotateOn = false;
				MoveToLiveItem(InItem, KeyDirection.Refresh);
				return;
			//case Keys.F9:
			//	{
			//		SetScreenOnTop(StartTimer: false);
			//		FrmSingleMonitorAlert frmSingleMonitorAlert = new FrmSingleMonitorAlert();
			//		DialogResult dialogResult = frmSingleMonitorAlert.ShowDialog();
			//		if (dialogResult == DialogResult.OK)
			//		{
			//			LiveScreen.StartAlert(Gf.LiveItem, Gf.Alert_OriginalMessage, Gf.AlertTimeRemaining, Gf.Alert_UserFont, Gf.Alert_Scroll, Gf.Alert_Flash, Gf.Alert_Transparent, Gf.Alert_UserFontShadow, Gf.Alert_UserFontOutline, Gf.Alert_TextColour, Gf.Alert_BackColour, Gf.Alert_TextAlign, Gf.Alert_VerticalAlign, Gf.BottomBorderFactor);
			//		}
			//		SetScreenOnTop(StartTimer: true);
			//		return;
			//	}
				//case Keys.F6:
				//	Gf.SwitchChineseLyricsNotationListView(ref Gf.LiveItem, Gf.SwitchChinese(ref Gf.LiveItem.CompleteLyrics));
				//	ShowSlide(ref Gf.LiveItem, ImageTransitionControl.TransitionAction.None);
				//	return;
				case Keys.A:
				Gf.AutoRotateOn = !Gf.AutoRotateOn;
				Gf.RestartCurrentItem = false;
				TimerRotate.Start();
				return;
			//case Keys.M:
			//	Remote_MediaItemPausePlay();
			//	return;
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
				if (InItem.CurSlide < InItem.TotalSlides || Gf.AdvanceNextItem)
				{
					MoveToSlideLiveItem(InItem, KeyDirection.NextOne);
				}
				return;
			case Keys.Space:
				MoveToSlideLiveItem(InItem, KeyDirection.NextOne);
				return;
			case Keys.G:
				if (Gf.GapItemOption == GapType.None)
				{
					Gf.GapItemOption = Gf.AltGapItemOption;
					Gf.AltGapItemOption = GapType.None;
				}
				else
				{
					Gf.AltGapItemOption = Gf.GapItemOption;
					Gf.GapItemOption = GapType.None;
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
				ShowSlide(ref Gf.LiveItem, ImageTransitionControl.TransitionAction.None, DoActiveIndicator: true, RedoBackground: false);
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
			Gf.ShowDataDisplayMode = ((Gf.ShowDataDisplayMode <= 0) ? 1 : 0);
			ShowSlide(ref Gf.LiveItem, ImageTransitionControl.TransitionAction.None);
		}

		private void ToggleShowHeader()
		{
			Gf.ShowRunning_ShowSongHeadings = ((Gf.ShowRunning_ShowSongHeadings <= 0) ? 1 : 0);
			ShowSlide(ref Gf.LiveItem, ImageTransitionControl.TransitionAction.None);
		}

		private void ToggleUseShadowFont()
		{
			Gf.ShowRunning_UseShadowFont = ((Gf.ShowRunning_UseShadowFont <= 0) ? 1 : 0);
			ShowSlide(ref Gf.LiveItem, ImageTransitionControl.TransitionAction.None);
		}

		private void ToggleUseOutlineFont()
		{
			Gf.ShowRunning_UseOutlineFont = ((Gf.ShowRunning_UseOutlineFont <= 0) ? 1 : 0);
			ShowSlide(ref Gf.LiveItem, ImageTransitionControl.TransitionAction.None);
		}

		private void ToggleShowNotations()
		{
			Gf.ShowRunning_ShowNotations = ((Gf.ShowRunning_ShowNotations <= 0) ? 1 : 0);
			ShowSong(ref Gf.LiveItem, 1, ImageTransitionControl.TransitionAction.None);
			//RemoteControlLyricsWindow(LyricsWindowAction.Remote_ItemChanged);
		}

		private void ToggleInterlace()
		{
			Gf.ShowRunning_ShowInterlace = ((Gf.ShowRunning_ShowInterlace <= 0) ? 1 : 0);
			ShowSlide(ref Gf.LiveItem, ImageTransitionControl.TransitionAction.None);
		}

		private void ToggleVerticalAlignment()
		{
			if (Gf.ShowRunning_ShowVerticalAlign == 0)
			{
				Gf.ShowRunning_ShowVerticalAlign = 1;
			}
			else if (Gf.ShowRunning_ShowVerticalAlign == 1)
			{
				Gf.ShowRunning_ShowVerticalAlign = 2;
			}
			else if (Gf.ShowRunning_ShowVerticalAlign == 2)
			{
				Gf.ShowRunning_ShowVerticalAlign = 0;
			}
			ShowSlide(ref Gf.LiveItem, ImageTransitionControl.TransitionAction.None);
		}

		private void ToggleShowLyrics()
		{
			if (Gf.ShowRunning_ShowLyrics == 0)
			{
				Gf.ShowRunning_ShowLyrics = 1;
			}
			else if (Gf.ShowRunning_ShowLyrics == 1)
			{
				Gf.ShowRunning_ShowLyrics = 2;
			}
			else if (Gf.ShowRunning_ShowLyrics == 2)
			{
				Gf.ShowRunning_ShowLyrics = 0;
			}
			ShowSlide(ref Gf.LiveItem, ImageTransitionControl.TransitionAction.None);
		}

		private void FrmLiveShow_MouseDown(object sender, MouseEventArgs e)
		{
			DoMouseDown(e.Button);
		}

		public void DoMouseDown(MouseButtons InBtn)
		{
			if (!Gf.DualMonitorMode)
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
			if (Gf.DualMonitorMode)
			{
				return;
			}
			if (mousedown_timelapse < 400)
			{
				if (mouse_btn == MouseButtons.Left)
				{
					MoveToSlideLiveItem(Gf.LiveItem, KeyDirection.NextOne);
				}
				else if (mouse_btn == MouseButtons.Right)
				{
					MoveToSlideLiveItem(Gf.LiveItem, KeyDirection.PrevOne);
				}
			}
			else if (mouse_btn == MouseButtons.Left)
			{
				MoveToLiveItem(Gf.LiveItem, KeyDirection.NextOne);
			}
			else if (mouse_btn == MouseButtons.Right)
			{
				MoveToLiveItem(Gf.LiveItem, KeyDirection.PrevOne);
			}
			mousedown_timelapse = 0;
			TimerMouseDown.Stop();
		}

		protected override bool ProcessCmdKey(ref System.Windows.Forms.Message msg, Keys keyData)
		{
			ItemKeyPressed(Gf.LiveItem, keyData);
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
				// A slide show believed ended is asked about again (one lookup): it may run again.
				Gf.ReprobeEndedLivePowerPoint();
				if (Gf.LivePowerPointNotStarted || Gf.LivePowerPointEnded)
				{
					// The item's slide show did not start, or has ended: this window must not be transparent over
					// whatever is below it (the desktop, another application, an earlier slide show). The default
					// background, as for any item this window cannot show.
					SetShowWindow(max: true);
					Gf.ResetPictureBox(ref InItem, ref LiveScreen, GapType.Default, TransitionAction);
					return true;
				}
				if (InItem.CurSlide < 1)
				{
					// -1 is "the end of the show was reached": the last slide, as the main form shows it.
					InItem.CurSlide = Math.Max(1, InItem.TotalSlides);
				}
				if ((Gf.DualMonitorMode && Gf.NoPowerpointPanelOverlay && InItem.Type == "P")/* || (Gf.DualMonitorMode && Gf.LiveCamNoPanelOverlay && Gf.ShowLiveCam)*/)
				{
					SetShowWindow(max: false);
				}
				else
				{
					SetShowWindow(max: true);

				}
				// An item this window did not run as a slide show (live camera show) moves no deck: LivePP's run
				// is an earlier item's.
				if (Gf.LivePowerPointRunState != Gf.LivePowerPointRun.None)
				{
					Gf.LivePP.ImplementPowerpointSlideMovement(ref InItem.CurSlide, InItem.TotalSlides, (OfficeLibKeys)Keys.None, InItem.CurSlide);
					Gf.ReportPowerpointNavigation(Gf.LivePP, InItem);
					if (Gf.LivePowerPointEnded)
					{
						PaintEndedPowerPointItem();
						return true;
					}
				}
				if (Gf.ShowLiveCam)
				{
					Gf.SetTransparentBackground(InItem, ref LiveScreen);
				}
				Gf.DrawText(ref InItem, ref LiveScreen, InItem.LyricsAndNotationsList, DoActiveIndicator: false, ClearAll: false);
				//RemoteControlLyricsWindow(LyricsWindowAction.Remote_LyricsChanged);
				return true;
			}
			if (TransitionAction == ImageTransitionControl.TransitionAction.AsStored)
			{
			}
			if ((Gf.DualMonitorMode && Gf.NoMediaPanelOverlay && InItem.Type == "M") || (Gf.DualMonitorMode && Gf.LiveCamNoPanelOverlay && Gf.ShowLiveCam))
			{
				SetShowWindow(max: false);
			}
			else
			{
				SetShowWindow(max: true);
			}
			InItem.TotalItems = Gf.TotalWorshipListItems;
			bool flag = Gf.ShowDBSlide(ref InItem, ref LiveScreen, DoActiveIndicator, TransitionAction, RedoBackground);
			if (!flag)
			{
				Gf.ResetPictureBox(ref InItem, ref LiveScreen, GapType.Default, TransitionAction);
			}
			//RemoteControlLyricsWindow(LyricsWindowAction.Remote_LyricsChanged);
			return flag;
		}

		private void ShowSong(ref SongSettings InItem, int StartingSlide, ImageTransitionControl.TransitionAction TransitionAction)
		{
			InItem.CurSlide = StartingSlide;
			RefreshSlidesFonts(ref InItem, TransitionAction);
		}

		private bool RefreshSlidesFonts(ref SongSettings InItem, ImageTransitionControl.TransitionAction TransitionAction)
		{
			Gf.FormatText(ref InItem, Gf.PanelBackColour, Gf.PanelBackColourTransparent, Gf.PanelTextColour, Gf.PanelTextColourAsRegion1, InItem.UseDefaultFormat);
			Gf.FormatDisplayLyrics(ref InItem, PrepareSlides: true, UseStoredSequence: true);
			Gf.DisplaySlidesFormattedLyrics(ref InItem, ref Gf.tbLyricsMonitorSpace, ScrollToCaret: true, Gf.LMShowNotations);
			return ShowSlide(ref InItem, TransitionAction);
		}

		private void MoveToLiveItem(SongSettings InItem, KeyDirection InIndex)
		{
			MoveToLiveItem(InItem, InIndex, 0);
		}

		private void MoveToLiveItem(SongSettings InItem, KeyDirection InDirection, int SlideNo)
		{
			Gf.Launch_StartPresAt = (Gf.DualMonitorMode ? Gf.Launch_StartPresAt : Gf.StartPresAt);
			switch (InDirection)
			{
			case KeyDirection.FirstOne:
				LoadWorshipListItemToLive(1, SlideNo, ImageTransitionControl.TransitionAction.AsStored);
				break;
			case KeyDirection.PrevOne:
				if (Gf.Launch_StartPresAt <= Gf.TotalWorshipListItems)
				{
					LoadWorshipListItemToLive(Gf.AdHocItemPresent ? Gf.Launch_StartPresAt : (Gf.Launch_StartPresAt - ((!(InItem.Type == "G")) ? 1 : 0)), SlideNo, ImageTransitionControl.TransitionAction.AsStored);
				}
				break;
			case KeyDirection.NextOne:
			{
				if (Gf.GapItemOption == GapType.None)
				{
					LoadWorshipListItemToLive(Gf.Launch_StartPresAt + 1, SlideNo, ImageTransitionControl.TransitionAction.AsStored);
					break;
				}
				if (Gf.Launch_StartPresAt <= Gf.TotalWorshipListItems && (Gf.Launch_StartPresAt == 0 || (InItem.Type == "G" && Gf.Launch_StartPresAt != Gf.TotalWorshipListItems)))
				{
					LoadWorshipListItemToLive(Gf.Launch_StartPresAt + 1, SlideNo, ImageTransitionControl.TransitionAction.AsStored);
					break;
				}
				string InTitle = "";
				LoadItem(ref InItem, "G1", "", 0, ref InTitle, ImageTransitionControl.TransitionAction.None, ReLoadIfCaptureDevice: false);
				break;
			}
			case KeyDirection.LastOne:
				LoadWorshipListItemToLive(Gf.TotalWorshipListItems, SlideNo, ImageTransitionControl.TransitionAction.AsStored);
				break;
			default:
				LoadWorshipListItemToLive(Gf.Launch_StartPresAt, SlideNo, ImageTransitionControl.TransitionAction.AsStored);
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
			if (Gf.AdHocItemPresent)
			{
				if (Selecteditem > 0)
				{
					Gf.AdHocItemPresent = false;
				}
				if (Selecteditem > Gf.TotalWorshipListItems)
				{
					Selecteditem = Gf.TotalWorshipListItems;
				}
			}
			else
			{
				if (Gf.TotalWorshipListItems == 0)
				{
					return false;
				}
				if (Selecteditem < 1)
				{
					Selecteditem = 1;
				}
				else if (Selecteditem > Gf.TotalWorshipListItems)
				{
					Selecteditem = Gf.TotalWorshipListItems;
				}
			}
			string inIDString = Gf.WorshipSongs[Selecteditem, 0];
			string inFormatString = Gf.WorshipSongs[Selecteditem, 4];
			string InTitle = Gf.WorshipSongs[Selecteditem, 2];
			Gf.LiveItem.CurItemNo = Selecteditem;
			Gf.StartPresAt = (Gf.AdHocItemPresent ? Gf.StartPresAt : Gf.LiveItem.CurItemNo);
			Gf.LiveItem.Source = ((Gf.LiveItem.CurItemNo > 0) ? ItemSource.WorshipList : Gf.OutputItem.Source);
			Gf.LiveItem.OutputStyleScreen = true;
			Gf.LiveItem.AtLiveScreen = true;
			LoadItem(ref Gf.LiveItem, inIDString, inFormatString, SlideNo, ref InTitle, TransitionAction, ReLoadIfCaptureDevice);
			return true;
		}

		private void MoveToSlideLiveItem(SongSettings InItem, KeyDirection InDirection)
		{
			if (Gf.AdvanceNextItem)
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
				else if (InDirection == KeyDirection.NextOne && InItem.CurItemNo < Gf.TotalWorshipListItems && InItem.CurSlide >= InItem.TotalSlides)
				{
					if (InItem.Type == "P" && Gf.LivePowerPointRunState != Gf.LivePowerPointRun.None)
					{
						int num = Gf.LivePP.ImplementPowerpointSlideMovement(ref InItem.CurSlide, InItem.TotalSlides, (OfficeLibKeys)Gf.ReMapKeyDirectionToPowerpoint(InDirection));
						// The end of the item, so the next item: PowerPoint reached the end of the show (-1, also
						// when the show ended on this move), or there is no slide show to move (not started, or
						// ended earlier). A navigation error or a busy PowerPoint is not an end: the item stays.
						bool itemEnded = num <= 0 || (Gf.LivePP.LastCallNotRunning && Gf.LivePP.LastNavigationError == null);
						if (!itemEnded)
						{
							Gf.ReportPowerpointNavigation(Gf.LivePP, InItem);
							Gf.DrawText(ref InItem, ref LiveScreen, InItem.LyricsAndNotationsList, DoActiveIndicator: false, ClearAll: false);
							return;
						}
					}
					MoveToLiveItem(InItem, KeyDirection.NextOne, 0);
					return;
				}
			}
			if (Gf.ShowRunning & (InItem.Type == "P"))
			{
				// A slide show believed ended is asked about again (one lookup): it may run again.
				Gf.ReprobeEndedLivePowerPoint();
				if (Gf.LivePowerPointNotStarted || Gf.LivePowerPointEnded)
				{
					// No slide show to move; the default background stays (see ShowSlide).
					return;
				}
				// An item this window did not run as a slide show (live camera show) moves no deck.
				if (Gf.LivePowerPointRunState != Gf.LivePowerPointRun.None)
				{
					Gf.LivePP.ImplementPowerpointSlideMovement(ref InItem.CurSlide, InItem.TotalSlides, (OfficeLibKeys)Gf.ReMapKeyDirectionToPowerpoint(InDirection));
					Gf.ReportPowerpointNavigation(Gf.LivePP, InItem);
					if (InItem.CurSlide < 1)
					{
						// The end of the show was reached (-1): the last slide, as the main form shows it. Left at -1,
						// later keys and refreshes would act on slide -1 (and on slide 0 with no slide count).
						InItem.CurSlide = Math.Max(1, InItem.TotalSlides);
					}
					if (Gf.LivePowerPointEnded)
					{
						// Ended on this move, or found ended now.
						PaintEndedPowerPointItem();
						return;
					}
				}
				Gf.DrawText(ref InItem, ref LiveScreen, InItem.LyricsAndNotationsList, DoActiveIndicator: false, ClearAll: false);
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
				else if (Gf.GapItemOption == GapType.None)
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
			if (Gf.RestartItemActioned)
			{
				Gf.RestartCurrentItem = false;
			}
			else
			{
				Gf.RestartItemActioned = true;
				Gf.RestartCurrentItem = true;
			}
			string text = DataUtil.Left(InIDString, 1);
			// Set again below when this item runs a slide show. The serial tells the main form this state is new.
			Gf.LivePowerPointRunState = Gf.LivePowerPointRun.None;
			Gf.LiveLoadSerial++;
			if (text == "P")
			{
				// Before any PowerPoint work: a lowering still waiting after a non-PowerPoint item must not run.
				Gf.PowerPointItemLoading();
				lowerSlideShowsTimer?.Stop();
			}
			string prevTitle = "";
			string nextTitle = "";
			bool flag = false;
			if (Gf.TotalWorshipListItems > 0)
			{
				int num = -1;
				int num2 = -1;
				if (InItem.CurItemNo == 0)
				{
					num = Gf.StartPresAt;
					num2 = Gf.StartPresAt + 1;
				}
				else
				{
					num = Gf.StartPresAt - 1;
					num2 = Gf.StartPresAt + 1;
				}
				if (num < 1 && InItem.CurItemNo == 0)
				{
					num = 1;
				}
				if (num2 > Gf.TotalWorshipListItems)
				{
					num2 = ((InItem.CurItemNo != 0) ? (-1) : Gf.TotalWorshipListItems);
				}
				if (num == num2 && num == 0)
				{
					num = -1;
				}
				prevTitle = ((num >= 1) ? Gf.RemoveMusicSym(Gf.WorshipSongs[num, 2]) : "");
				nextTitle = ((num2 >= 1) ? Gf.RemoveMusicSym(Gf.WorshipSongs[num2, 2]) : "");
			}
			Gf.InitialiseIndividualData(ref InItem, (text == "G" && Media_NextItemHasSameMedia()) ? GapMedia.SameAsPrevious : GapMedia.SessionMedia, "");
			InItem.PrevTitle = prevTitle;
			InItem.NextTitle = nextTitle;
			Gf.LoadIndividualData(ref InItem, InIDString, "", StartingSlide, ref InTitle);
			if (InItem.Type == "I" || text == "G")
			{
				InFormatString = InItem.Format.FormatString;
				TransitionAction = ImageTransitionControl.TransitionAction.AsStored;
			}
			Gf.LoadIndividualFormatData(ref InItem, InFormatString);
			if (Gf.ShowLiveCam & !FirstItemBeingProcessed)
			{
				if (Gf.GetMediaLocation(InItem) != Gf.CurrentMediaLocation || Gf.CurrentMediaLocation == "")
				{
					Gf.CurrentMediaLocation = "";
			Gf.CurrentMediaOutputMonitorName = "";
				}
				if (text == "P")
				{
					Gf.MinimizePowerPointWindows(ref Gf.LivePP);
					// This item is not run as a slide show: LivePP's run is an earlier item's, which nothing
					// here may move.
					Gf.LivePP.ForgetRun();
				}
				flag = true;
			}
			if (text == "P" && !flag)
			{
				SetScreenOnTop(StartTimer: false);
				Gf.FormatText(ref InItem, Gf.PanelBackColour, Gf.PanelBackColourTransparent, Gf.PanelTextColour, Gf.PanelTextColourAsRegion1, InItem.UseDefaultFormat);
				InItem.TotalSlides = Gf.RunPowerpointSong(ref InItem, ref Gf.LivePP, StartingSlide);
				bool slideShowStarted = Gf.LivePP.LastRunStarted;
				Gf.LivePowerPointRunState = slideShowStarted ? Gf.LivePowerPointRun.Started : Gf.LivePowerPointRun.NotStarted;
				ResetMediaSettings();
				if (slideShowStarted)
				{
					// Transparent over the slide show. An item that did not start gets the default background
					// instead (ShowSlide below).
					Gf.SetTransparentBackground(Gf.LiveItem, ref LiveScreen);
				}
				InItem.Format.ShowItemTransition = 0;
				InItem.Format.ShowSlideTransition = 0;
				if (Gf.ShowLiveCam)
				{
					Gf.MinimizePowerPointWindows(ref Gf.LivePP);
				}
				else if (slideShowStarted)
				{
					Gf.DrawText(ref InItem, ref LiveScreen, InItem.LyricsAndNotationsList, DoActiveIndicator: false, ClearAll: false);
				}
				if (Gf.DualMonitorMode && Gf.NoPowerpointPanelOverlay && slideShowStarted)
				{
					SetShowWindow(max: false);
				}
				CurItemRotates = false;
				ShowSlide(ref Gf.LiveItem, ImageTransitionControl.TransitionAction.None, DoActiveIndicator: false, RedoBackground: true);
				RemoteControlMediaPlayerWindow(MediaPlayerWindowAction.Remote_ClearScreen);
				Gf.tbLyricsMonitorSpace.Text = "";
				//RemoteControlLyricsWindow(LyricsWindowAction.Remote_ItemChanged);
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
			MediaBackgroundStyle mediaBackgroundStyle = Gf.GetMediaBackgroundType(InItem, UpdateVariables: true);
			if (InItem.Format.MediaOption == 3 && ReLoadIfCaptureDevice)
			{
				mediaBackgroundStyle = ((!Gf.MediaCurrentItemIsVideo) ? MediaBackgroundStyle.Audio : MediaBackgroundStyle.Video);
			}
			Gf.FormatText(ref InItem, Gf.PanelBackColour, Gf.PanelBackColourTransparent, Gf.PanelTextColour, Gf.PanelTextColourAsRegion1, InItem.UseDefaultFormat);
			Gf.FormatDisplayLyrics(ref InItem, PrepareSlides: true, UseStoredSequence: true);
			Gf.DisplaySlidesFormattedLyrics(ref InItem, ref Gf.tbLyricsMonitorSpace, ScrollToCaret: true, Gf.LMShowNotations);
			if (flag)
			{
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
				mediaBackgroundStyle2 = ((!Gf.MediaCurrentItemIsVideo) ? MediaBackgroundStyle.Audio : MediaBackgroundStyle.Video);
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
			bool itemHasExplicitMedia = InItem.Type == "M" ||
				!string.IsNullOrEmpty(InItem.Format.HeaderData[50]) ||
				!string.IsNullOrEmpty(InItem.Format.HeaderData[51]) ||
				!string.IsNullOrEmpty(InItem.Format.HeaderData[55]) ||
				!string.IsNullOrEmpty(InItem.Format.HeaderData[56]);
			if (mediaBackgroundStyle3 == MediaBackgroundStyle.Video && itemHasExplicitMedia)
			{
                Gf.MediaCurrentItemIsVideo = true;
                InItem.Format.MediaTransparent = true;
            }
            else
			{
				Gf.MediaCurrentItemIsVideo = false;
				InItem.Format.MediaTransparent = false;
			}
			if (Gf.ShowLiveCam)
			{
				Gf.MinimizePowerPointWindows(ref Gf.LivePP);
			}
			else
			{
				ShowSlide(ref InItem, TransitionAction, DoActiveIndicator: false, RedoBackground: true);
			}
			//RemoteControlLyricsWindow(LyricsWindowAction.Remote_ItemChanged);
			if (InItem.Format.MediaTransparent)
			{
				Gf.MinimizePowerPointWindows(ref Gf.LivePP);
			}
			else
			{
				LowerSlideShowsWhenItemShown();
			}
			tempRotateTimings = "";
			if (Gf.AutoRotateOn)
			{
				RemoteControlMediaPlayerWindow(MediaPlayerWindowAction.Remote_ResumeItem);
			}
			else if (Gf.RestartCurrentItem && mediaBackgroundStyle2 != 0)
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
			if (Gf.CurrentMediaLocation == "" || Gf.LiveItem.CurItemNo == Gf.TotalWorshipListItems || Gf.StartPresAt == Gf.TotalWorshipListItems)
			{
				return false;
			}
			try
			{
				int num = Gf.StartPresAt + 1;
				string inTitle = Gf.WorshipSongs[num, 2];
				string text = Gf.WorshipSongs[num, 1];
				string Title = "";
				string FormatString = Gf.WorshipSongs[num, 4];
				if (text == "D")
				{
					Title = Gf.LookupDBTitle2(DataUtil.StringToInt(DataUtil.Mid(Gf.WorshipSongs[num, 0], 1)));
				}
				else if (text == "I")
				{
					Gf.GetTitle2AndFormatFromInfoFile(DataUtil.Mid(Gf.WorshipSongs[num, 0], 1), ref Title, ref FormatString);
				}
				int inMediaOption = DataUtil.StringToInt(Gf.ExtractHeaderInfo(FormatString, 50, '>'));
				bool inUseDefaultFormat = (FormatString == "") ? true : false;
				string inMediaLocation = Gf.ExtractHeaderInfo(FormatString, 51, '>');
				int inMediaCaptureDeviceNumber = DataUtil.StringToInt(Gf.ExtractHeaderInfo(FormatString, 55, '>'));
				string inMediaOutputMonitorName = Gf.ExtractHeaderInfo(FormatString, 56, '>');
				if (inMediaOutputMonitorName == "" && Gf.MediaOutputMonitorName != "")
				{
					inMediaOutputMonitorName = Gf.MediaOutputMonitorName;
				}
				if (inMediaOutputMonitorName == "")
				{
					inMediaOutputMonitorName = Gf.OutputMonitorName;
				}
				string mediaLocation = Gf.GetMediaLocation(inMediaOption, inTitle, Title, inUseDefaultFormat, text, inMediaLocation, inMediaCaptureDeviceNumber);
				if (Gf.CurrentMediaLocation == mediaLocation)
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
			Gf.CurrentMediaLocation = "";
			Gf.CurrentMediaOutputMonitorName = "";
			Gf.CurrentMediaIsVideo = false;
			Gf.MediaNotifyRepeatItem = false;
		}

		private void Start_ItemRotate(int InRotateGap, string InRotateTimings, MediaBackgroundStyle MediaBackground)
		{
			tempRotateTimings = InRotateTimings;
			if (Gf.LiveItem.RotateStyle == 2)
			{
				ItemRotationNextTiming = DataUtil.StringToInt(DataUtil.ExtractOneInfo(ref tempRotateTimings, ';', RemoveExtract: true, MinusOneIfBlank: false));
			}
			else
			{
				ItemRotationNextTiming = Gf.LiveItem.RotateGap;
			}
			ItemRotationNextSlideNumber = 1;
			Gf.MediaDoRotate = ((MediaBackground != 0) ? true : false);
			if (Gf.LiveItem.RotateStyle == 2)
			{
				if (Gf.LiveItem.RotateTotal == 0)
				{
					if (Gf.MediaDoRotate)
					{
						RemoteControlMediaPlayerWindow(MediaPlayerWindowAction.Remote_GetMediaTimings);
						Gf.LiveItem.RotateTotal = intLiveMediaDuration;
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
						Gf.LiveItem.RotateTotal = num + 10;
					}
				}
				else
				{
					RemoteControlMediaPlayerWindow(MediaPlayerWindowAction.Remote_GetMediaTimings);
					int num3 = intLiveMediaDuration;
					if (num3 != Gf.LiveItem.RotateTotal)
					{
						Gf.MediaDoRotate = false;
					}
				}
			}
			Gf.MediaLiveItemStartTime = DateTime.Now;
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
			if ((Gf.AutoRotateOn && CurItemRotates) || Gf.RestartCurrentItem)
			{
				if (!Gf.MediaDoRotate)
				{
					Gf.MediaPlayedLapseTime = DateTime.Now.Subtract(Gf.MediaLiveItemStartTime);
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
			switch (Gf.LiveItem.RotateStyle)
			{
			case 1:
				if (Gf.LiveItem.RotateGap < 0)
				{
					TimerRotate.Stop();
				}
				else
				{
					if (!(Gf.MediaPlayedLapseTime.TotalSeconds > 0.0) || !(Gf.MediaPlayedLapseTime.TotalSeconds >= (double)ItemRotationNextTiming))
					{
						break;
					}
					if (Gf.LiveItem.CurSlide < Gf.LiveItem.TotalSlides)
					{
						Gf.LiveItem.CurSlide++;
						MoveToSlideLiveItem(Gf.LiveItem, KeyDirection.Refresh);
						ItemRotationNextTiming += Gf.LiveItem.RotateGap;
						if (Gf.ShowRunning)
						{
							this.OnMessage(9, "");
						}
						break;
					}
					TimerRotate.Stop();
					int num = ImplementAutoRotateOption();
					if (num >= 0)
					{
						if (num == Gf.LiveItem.CurItemNo)
						{
							ItemRotationNextTiming = (int)Gf.MediaPlayedLapseTime.TotalSeconds + Gf.LiveItem.RotateGap;
							Gf.LiveItem.CurSlide = 1;
							MoveToSlideLiveItem(Gf.LiveItem, KeyDirection.Refresh);
							if (Gf.ShowRunning)
							{
								this.OnMessage(9, "");
							}
							TimerRotate.Start();
							break;
						}
						prevRefMode = Gf.ReferenceAlertSource;
						Gf.ReferenceAlertSource = ((num != Gf.LiveItem.CurItemNo) ? Gf.ReferenceAlertSource : 0);
						LoadWorshipListItemToLive(num, 1, ImageTransitionControl.TransitionAction.AsStored);
						Gf.MediaLiveItemStartTime = DateTime.Now;
						Gf.ReferenceAlertSource = prevRefMode;
						if (Gf.ShowRunning)
						{
							this.OnMessage(7, "");
						}
					}
					else if (Gf.GapItemOption != 0)
					{
						Gf.StartPresAt = (Gf.AdHocItemPresent ? Gf.StartPresAt : Gf.LiveItem.CurItemNo);
						Gf.Launch_StartPresAt = Gf.StartPresAt;
						MoveToLiveItem(Gf.LiveItem, KeyDirection.NextOne);
						if (Gf.ShowRunning)
						{
							this.OnMessage(8, "");
						}
					}
				}
				break;
			case 2:
				if ((!(Gf.MediaPlayedLapseTime.TotalSeconds > 0.0) || !(Gf.MediaPlayedLapseTime.TotalSeconds >= (double)ItemRotationNextTiming)) && !(Gf.MediaPlayedLapseTime.TotalSeconds >= (double)Gf.LiveItem.RotateTotal))
				{
					break;
				}
				if (Gf.MediaPlayedLapseTime.TotalSeconds >= (double)Gf.LiveItem.RotateTotal)
				{
					TimerRotate.Stop();
					int num = ImplementAutoRotateOption();
					if (num >= 0)
					{
						prevRefMode = Gf.ReferenceAlertSource;
						Gf.ReferenceAlertSource = ((num != Gf.LiveItem.CurItemNo) ? Gf.ReferenceAlertSource : 0);
						if (Gf.MediaLengthAsRotateLength)
						{
							RemoteControlMediaPlayerWindow(MediaPlayerWindowAction.Remote_StopItem);
						}
						LoadWorshipListItemToLive(num, 1, ImageTransitionControl.TransitionAction.AsStored);
						Gf.MediaLiveItemStartTime = DateTime.Now;
						Gf.ReferenceAlertSource = prevRefMode;
						if (Gf.ShowRunning)
						{
							this.OnMessage(7, "");
						}
					}
					else if (Gf.GapItemOption != 0)
					{
						Gf.StartPresAt = (Gf.AdHocItemPresent ? Gf.StartPresAt : Gf.LiveItem.CurItemNo);
						Gf.Launch_StartPresAt = Gf.StartPresAt;
						MoveToLiveItem(Gf.LiveItem, KeyDirection.NextOne);
						if (Gf.ShowRunning)
						{
							this.OnMessage(8, "");
						}
					}
					break;
				}
				if (ItemRotationNextSlideNumber < Gf.LiveItem.TotalSlides && ItemRotationNextTiming > 0)
				{
					ItemRotationNextSlideNumber++;
					Gf.LiveItem.CurSlide = ItemRotationNextSlideNumber;
					MoveToSlideLiveItem(Gf.LiveItem, KeyDirection.Refresh);
					if (Gf.ShowRunning)
					{
						this.OnMessage(9, "");
					}
				}
				ItemRotationNextTiming = DataUtil.StringToInt(DataUtil.ExtractOneInfo(ref tempRotateTimings, ';', RemoveExtract: true, MinusOneIfBlank: false));
				if (ItemRotationNextTiming <= 0)
				{
					ItemRotationNextTiming = Gf.LiveItem.RotateTotal;
				}
				break;
			}
		}

		private int ImplementAutoRotateOption()
		{
			int num = -1;
			if (Gf.RestartCurrentItem)
			{
				return -1;
			}
			if (Gf.AutoRotateOn)
			{
				switch (Gf.AutoRotateStyle)
				{
				case 0:
					return -1;
				case 1:
					return (!Gf.AdHocItemPresent) ? Gf.StartPresAt : 0;
				case 2:
					num = Rotate_FindNextItem(GetPreviousIfNoNext: false);
					if (num == Gf.LiveItem.CurItemNo)
					{
						return -1;
					}
					Gf.StartPresAt = num;
					return Gf.StartPresAt;
				default:
					num = Rotate_FindNextItem(GetPreviousIfNoNext: true);
					if (num == Gf.LiveItem.CurItemNo)
					{
						return Gf.LiveItem.CurItemNo;
					}
					Gf.StartPresAt = num;
					return Gf.StartPresAt;
				}
			}
			return -1;
		}

		private int Rotate_FindNextItem(bool GetPreviousIfNoNext)
		{
			if (Gf.LiveItem.CurItemNo == Gf.TotalWorshipListItems || Gf.StartPresAt == Gf.TotalWorshipListItems)
			{
				if (GetPreviousIfNoNext)
				{
					return Rotate_FindPreviousItem(Gf.LiveItem.CurItemNo);
				}
				return Gf.LiveItem.CurItemNo;
			}
			try
			{
				int num = Gf.StartPresAt + 1;
				int itemRotateResult = Gf.GetItemRotateResult(Gf.WorshipSongs[num, 0]);
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
					return Gf.StartPresAt + 1;
				}
				if (Gf.AdHocItemPresent && GetPreviousIfNoNext)
				{
					int num2 = Gf.StartPresAt;
					try
					{
						num2 = Gf.GetItemRotateResult(Gf.WorshipSongs[Gf.StartPresAt, 0]);
					}
					catch
					{
					}
					int result = Rotate_FindPreviousItem(Gf.StartPresAt);
					if (num2 < 1)
					{
						return Gf.LiveItem.CurItemNo;
					}
					return result;
				}
			}
			catch
			{
			}
			if (GetPreviousIfNoNext)
			{
				return Rotate_FindPreviousItem(Gf.LiveItem.CurItemNo);
			}
			return Gf.LiveItem.CurItemNo;
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
				int itemRotateResult = Gf.GetItemRotateResult(Gf.WorshipSongs[num, 0]);
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
					// First show: Load (raised before this) did the initialisation and set StartInitCompleted.
					FormFirstLoad = false;
				}
				else
				{
					// A later show of the same window: Load does not run again, this is its initialisation.
					StartInitCompleted = false;
					InitForm();
					StartInitCompleted = true;
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
					//MediaPlayerWindow.Show();
					break;
				case MediaPlayerWindowAction.SendToBack:
					//MediaPlayerWindow.SendToBack();
					break;
				case MediaPlayerWindowAction.Remote_StopShow:
					//MediaPlayerWindow.Remote_StopShow();
					break;
				case MediaPlayerWindowAction.Remote_ClearScreen:
					//MediaPlayerWindow.Remote_ClearScreen();
					break;
				case MediaPlayerWindowAction.Remote_LoadItem:
					//MediaPlayerWindow.ApplyOutputMonitor(GetMediaOutputMonitorName(Gf.LiveItem));
					//result = (MediaBackgroundStyle)MediaPlayerWindow.Remote_LoadItem();
					break;
				case MediaPlayerWindowAction.Remote_ResumeItem:
					//MediaPlayerWindow.ApplyOutputMonitor(GetMediaOutputMonitorName(Gf.LiveItem));
					//result = (MediaBackgroundStyle)MediaPlayerWindow.Remote_ResumeItem();
					break;
				case MediaPlayerWindowAction.Remote_RepeatItem:
					//MediaPlayerWindow.ApplyOutputMonitor(GetMediaOutputMonitorName(Gf.LiveItem));
					//result = (MediaBackgroundStyle)MediaPlayerWindow.Remote_RepeatItem();
					break;
				case MediaPlayerWindowAction.Remote_ResumeItemFromStart:
					//MediaPlayerWindow.Remote_ResumeItemFromStart();
					break;
				case MediaPlayerWindowAction.Remote_PauseItem:
					//MediaPlayerWindow.Remote_PauseItem();
					break;
				case MediaPlayerWindowAction.Remote_StopItem:
					//MediaPlayerWindow.Remote_StopItem();
					break;
				case MediaPlayerWindowAction.Remote_LoadLiveCam:
					//MediaPlayerWindow.ApplyOutputMonitor(GetMediaOutputMonitorName(Gf.LiveItem));
					//result = (MediaBackgroundStyle)MediaPlayerWindow.Remote_LoadLiveCam();
					break;
				case MediaPlayerWindowAction.Remote_UpdateLiveCam:
					//MediaPlayerWindow.ApplyOutputMonitor(GetMediaOutputMonitorName(Gf.LiveItem));
					//result = (MediaBackgroundStyle)MediaPlayerWindow.Remote_UpdateLiveCam();
					break;
				case MediaPlayerWindowAction.Remote_RefreshMediaWindow:
					//MediaPlayerWindow.Remote_RefreshMediaWindow();
					break;
				case MediaPlayerWindowAction.Remote_SendScreenToBack:
					//MediaPlayerWindow.Remote_SendScreenToBack();
					break;
				case MediaPlayerWindowAction.Remote_GetMediaTimings:
					//MediaPlayerWindow.Remote_GetMediaTimings(ref LiveMediaDuration, ref LiveMediaPosition, ref intLiveMediaDuration, ref intLiveMediaPosition);
					break;
				case MediaPlayerWindowAction.Remote_ItemPlayingStatus:
					//CurMediaPlayingStatus = MediaPlayerWindow.Remote_ItemPlayingStatus();
					break;
				case MediaPlayerWindowAction.Remote_PausePlayItem:
					//MediaPlayerWindow.Remote_PausePlayItem();
					break;
				}
			}
			catch
			{
				try
				{
					//MediaPlayerWindow = new FrmLaunchMediaPlayer();
					//MediaPlayerWindow.OnMessage += MediaPlayerWindow_OnMessage;
					result = RemoteControlMediaPlayerWindow(InAction);
				}
				catch
				{
				}
			}
			return result;
		}

		//private void RemoteControlLyricsWindow(LyricsWindowAction InAction)
		//{
		//	//if (Gf.LyricsMonitorNumber > 0 || Gf.LMSelectAutoOption > 0)//Gf.GetSecondryMonitorIndex()
		//	if (Gf.LyricsMonitorName == DisplayInfo.getSecondryDisplayName() || Gf.LMSelectAutoOption > 0)
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

		//private void RemoteControlLyricsWindow(LyricsWindowAction InAction)
		//{
		//	//if (Gf.LyricsMonitorNumber > 0 || Gf.LMSelectAutoOption > 0)//Gf.GetSecondryMonitorIndex()
		//	if (Gf.LyricsMonitorNumber == Gf.GetSecondryMonitorIndex() || Gf.LMSelectAutoOption > 0)
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
		}

		private void FrmLaunchShow_Leave(object sender, EventArgs e)
		{
		}

		private void GotoNextNonRotateItem()
		{
			int nextNonRotateItem = Gf.GetNextNonRotateItem((Gf.LiveItem.Type == "G") ? true : false);
			bool flag = false;
			if (Gf.GapItemOption == GapType.None)
			{
				if (nextNonRotateItem != Gf.StartPresAt)
				{
					Gf.StartPresAt = nextNonRotateItem;
					MoveToLiveItem(Gf.LiveItem, KeyDirection.Refresh);
				}
			}
			else if (nextNonRotateItem == Gf.StartPresAt)
			{
				if (Gf.LiveItem.Type != "G")
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
				Gf.StartPresAt = nextNonRotateItem;
				Gf.Launch_StartPresAt = Gf.StartPresAt;
				string InTitle = "";
				LoadItem(ref Gf.LiveItem, "G1", "", 0, ref InTitle, ImageTransitionControl.TransitionAction.None, ReLoadIfCaptureDevice: false);
			}
		}

		private void TimerToFront_Tick(object sender, EventArgs e)
		{
			if (Gf.DualMonitorMode)
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
