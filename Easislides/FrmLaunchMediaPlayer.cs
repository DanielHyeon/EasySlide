using Easislides.Module;
using Easislides.Util;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace Easislides
{
	public class FrmLaunchMediaPlayer : Form
	{
		public delegate void Message(int MsgCode, string MsgString);

		private IContainer components = null;

		private System.Windows.Forms.Timer TimerAttemptConnect;

		private System.Windows.Forms.Timer TimerRefresh;

		private bool FormFirstLoad = true;

		private int AttemptConnectCount = 0;

		private int MaxAttemptConnectCount = 60;

		private string PreviousItemLocation = "";

		private bool ItemFirstPlay = false;

		private Cursor LiveCursor;

		private Thread timerThread;

		private DShowLib DShowPlayer = new DShowLib();

		public event Message OnMessage;

		protected override void Dispose(bool disposing)
		{
			if (disposing && components != null)
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		private void InitializeComponent()
		{
			components = new System.ComponentModel.Container();
			TimerAttemptConnect = new System.Windows.Forms.Timer(components);
			TimerRefresh = new System.Windows.Forms.Timer(components);
			SuspendLayout();
			TimerRefresh.Enabled = true;
			TimerRefresh.Interval = 500;
			//TimerRefresh.Tick += new System.EventHandler(TimerRefresh_Tick);
			base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
			base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			BackColor = System.Drawing.Color.Black;
			base.ClientSize = new System.Drawing.Size(125, 110);
			base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			base.Name = "FrmLaunchMediaPlayer";
			base.ShowInTaskbar = false;
			base.VisibleChanged += new System.EventHandler(FrmLaunchMediaPlayer_VisibleChanged);
			base.FormClosing += new System.Windows.Forms.FormClosingEventHandler(FrmLaunchMediaPlayer_FormClosing);
			base.Load += new System.EventHandler(FrmLaunchMediaPlayer_Load);
			ResumeLayout(false);
		}

		public FrmLaunchMediaPlayer()
		{
			InitializeComponent();
		}

		private void FrmLaunchMediaPlayer_Load(object sender, EventArgs e)
		{
			Bitmap bitmap = new Bitmap(55, 25);
			//Graphics graphics = Graphics.FromImage(bitmap);
			IntPtr hicon = bitmap.GetHicon();
			LiveCursor = new Cursor(hicon);
			InitForm();
			//bitmap.Dispose();
			//graphics.Dispose();
		}

		private void InitForm()
		{
			SetShowWindow();
			BackColor = Color.Black;
			if (gf.WMP_Present)
			{
				InitMediaPlayer();
				timerThread = new Thread(ThreadProc);
				timerThread.IsBackground = true;
				timerThread.Start();
			}
			Remote_SendScreenToBack();
		}

		private void InitMediaPlayer()
		{
			if (gf.WMP_Present)
			{
				DShowPlayer.Parent = this;
				DShowPlayer.Location = new Point(0, 0);
				DShowPlayer.Dock = DockStyle.None;
				ApplyPlayerBounds(gf.LS_Width, gf.LS_Height);
				DShowPlayer.ForeColorChanged += DShowPlayer_ForeColorChanged;
			}
		}

		public void ApplyOutputMonitor(string monitorName)
		{
			Screen targetScreen = null;
			if (!string.IsNullOrEmpty(monitorName))
			{
				foreach (Screen screen in Screen.AllScreens)
				{
					if (string.Equals(screen.DeviceName, monitorName, StringComparison.OrdinalIgnoreCase))
					{
						targetScreen = screen;
						break;
					}
				}
			}
			if (targetScreen == null && !string.IsNullOrEmpty(gf.OutputMonitorName))
			{
				foreach (Screen screen in Screen.AllScreens)
				{
					if (string.Equals(screen.DeviceName, gf.OutputMonitorName, StringComparison.OrdinalIgnoreCase))
					{
						targetScreen = screen;
						break;
					}
				}
			}
			if (targetScreen == null)
			{
				targetScreen = Screen.PrimaryScreen;
			}
			if (targetScreen == null)
			{
				return;
			}
			gf.CurrentMediaOutputMonitorName = targetScreen.DeviceName;
			SetShowWindow(targetScreen.Bounds);
		}

		private void SetShowWindow(Rectangle bounds)
		{
			base.Left = bounds.Left;
			base.Top = bounds.Top;
			base.Width = bounds.Width;
			base.Height = bounds.Height;
			if (gf.WMP_Present)
			{
				ApplyPlayerBounds(bounds.Width, bounds.Height);
			}
		}

		private void ApplyPlayerBounds(int screenWidth, int screenHeight)
		{
			int num = screenWidth * gf.VideoSize / 100;
			int num2 = screenHeight * gf.VideoSize / 100;
			int num3 = (screenWidth - num) / 2 - 1;
			num3 = ((num3 >= 0) ? num3 : 0);
			int t = (gf.VideoVAlign == 1) ? ((screenHeight - num2) / 2) : ((gf.VideoVAlign == 2) ? (screenHeight - num2) : 0);
			DShowPlayer.SetDefaultSize(num3, t, num, num2, (VAlign)gf.VideoVAlign);
		}

		private void DShowPlayer_ForeColorChanged(object sender, EventArgs e)
		{
			if (DShowPlayer.ForeColor == Color.Red)
			{
				if (gf.ShowRunning)
				{
					if (DShowPlayer.MouseBtnPressed == MouseButtons.Left)
					{
						this.OnMessage(11, "");
					}
					else if (DShowPlayer.MouseBtnPressed == MouseButtons.Right)
					{
						this.OnMessage(12, "");
					}
				}
				return;
			}
			if (DShowPlayer.ForeColor == Color.Blue)
			{
				if (gf.ShowRunning)
				{
					this.OnMessage(13, "");
				}
				return;
			}
			switch (DShowPlayer.currentState)
			{
			case PlayState.Paused:
				break;
			case PlayState.Running:
			{
				TimerAttemptConnect.Stop();
				gf.MediaNotifyItemConnecting = false;
				if (ItemFirstPlay)
				{
					if (!gf.ShowLiveCam)
					{
						gf.MediaLiveItemStartTime = DateTime.Now;
						gf.MediaPlayedLapseTime = new TimeSpan(0L);
					}
				}
				else
				{
					gf.MediaDoRotate = false;
				}
				ItemFirstPlay = false;
				bool loopClip = false;
				int clipDuration = DShowPlayer.GetClipDuration();
				if (clipDuration == gf.LiveItem.RotateTotal || (gf.LiveItem.RotateStyle == 2 && gf.LiveItem.RotateTotal == 0))
				{
					if (!gf.ShowLiveCam)
					{
						gf.MediaLengthAsRotateLength = true;
					}
				}
				else
				{
					if (!gf.ShowLiveCam)
					{
						gf.MediaLengthAsRotateLength = false;
					}
					if (gf.LiveItem.Format.MediaRepeat > 0 && !gf.RestartCurrentItem)
					{
						loopClip = true;
					}
				}
				DShowPlayer.LoopClip = loopClip;
				break;
			}
			case PlayState.Stopped:
				DShowPlayer.currentVolume = 0;
				if (!gf.ShowLiveCam)
				{
					gf.MediaPlayedLapseTime = DateTime.Now.Subtract(gf.MediaLiveItemStartTime) + new TimeSpan(0, 0, 1);
				}
				break;
			}
		}

		public void Remote_StopItem()
		{
			if (gf.WMP_Present)
			{
				DShowPlayer.CloseClip();
			}
		}

		public void Remote_PauseItem()
		{
			if (gf.WMP_Present)
			{
				DShowPlayer.PauseClip();
			}
		}

		public void Remote_PausePlayItem()
		{
			if (gf.WMP_Present)
			{
				DShowPlayer.PausePlayClip();
			}
		}

		public int Remote_LoadItem()
		{
			if (gf.WMP_Present)
			{
				return LoadMedia(ResumeFromPreviousPosition: false, FirstLoad: true);
			}
			return 0;
		}

		public int Remote_ResumeItem()
		{
			if (gf.WMP_Present)
			{
				return LoadMedia(ResumeFromPreviousPosition: true, FirstLoad: false);
			}
			return 0;
		}

		public void Remote_ResumeItemFromStart()
		{
			if (gf.WMP_Present)
			{
				DShowPlayer.ResumeFromStart();
			}
		}

		public int Remote_LoadLiveCam()
		{
			if (gf.WMP_Present)
			{
				return LoadLiveCam();
			}
			return 0;
		}

		public int Remote_UpdateLiveCam()
		{
			if (gf.WMP_Present)
			{
				return UpdateLiveCam();
			}
			return 0;
		}

		public void Remote_ClearScreen()
		{
			if (gf.WMP_Present)
			{
				DShowPlayer.CloseClip();
			}
		}

		public int Remote_RepeatItem()
		{
			if (gf.WMP_Present)
			{
				gf.MediaNotifyRepeatItem = false;
				return LoadMedia(ResumeFromPreviousPosition: false, FirstLoad: false);
			}
			return 0;
		}

		public void Remote_StopShow()
		{
			Remote_StopItem();
			base.Left = 100;
			base.Top = 200;
			Hide();
		}

		public bool Remote_ItemPlayingStatus()
		{
			if (gf.WMP_Present && DShowPlayer.currentState == PlayState.Running)
			{
				return true;
			}
			return false;
		}

		public void Remote_RefreshMediaWindow()
		{
			if (gf.WMP_Present)
			{
				try
				{
					int left = DShowPlayer.Left;
					int top = DShowPlayer.Top;
					int width = DShowPlayer.Width;
					int height = DShowPlayer.Height;
					DShowPlayer.Width = 0;
					DShowPlayer.Height = 0;
					DShowPlayer.Width = width;
					DShowPlayer.Height = height;
					DShowPlayer.Invalidate();
				}
				catch
				{
				}
			}
		}

		public void Remote_SendScreenToBack()
		{
			SendToBack();
		}

		public bool MediaIsVideo()
		{
			if (gf.WMP_Present)
			{
				return !DShowPlayer.isVideo;
			}
			return false;
		}

		public void ThreadProc()
		{
			try
			{
				MethodInvoker method = UpdateProgress;
				while (true)
				{
					BeginInvoke(method);
					Thread.Sleep(750);
				}
			}
			catch (ThreadInterruptedException)
			{
			}
			catch (Exception)
			{
			}
		}

		private void UpdateProgress()
		{
			if (DShowPlayer.currentState != PlayState.Running || gf.ShowLiveCam)
			{
				return;
			}
			int currentPosition = DShowPlayer.GetCurrentPosition();
			if (currentPosition < 0)
			{
				return;
			}
			if ((int)gf.MediaPlayedLapseTime.TotalSeconds <= currentPosition)
			{
				if (!gf.ShowLiveCam && gf.MediaDoRotate)
				{
					gf.MediaPlayedLapseTime = new TimeSpan(0, 0, currentPosition);
				}
			}
			else if (!gf.ShowLiveCam && gf.MediaDoRotate)
			{
				gf.MediaPlayedLapseTime = DateTime.Now.Subtract(gf.MediaLiveItemStartTime);
			}
		}

		private void SetShowWindow()
		{
			base.Left = gf.LS_Left;
			base.Top = gf.LS_Top;
			base.Height = gf.LS_Height;
			base.Width = gf.LS_Width;
		}

		private void FrmLaunchMediaPlayer_FormClosing(object sender, FormClosingEventArgs e)
		{
			DShowPlayer.TidyUp();
		}

		private int LoadMedia(bool ResumeFromPreviousPosition, bool FirstLoad)
		{
			string mediaLocation = gf.GetMediaLocation(gf.LiveItem);
			int currentInputDevice = 1;
			if (mediaLocation == "")
			{
				DShowPlayer.CloseClip();
				return 0;
			}
			if (DataUtil.Left(mediaLocation, "<<Capture>>".Length) == "<<Capture>>")
			{
				currentInputDevice = DataUtil.StringToInt(DataUtil.Mid(mediaLocation, "<<Capture>>".Length));
			}
			DShowPlayer.SetMute((gf.LiveItem.Format.MediaMute > 0) ? true : false);
			DShowPlayer.SetWideScreen((gf.LiveItem.Format.MediaWidescreen > 0) ? true : false, ResizeWindow: false);
			gf.MediaResetStartTime = true;
			DShowPlayer.newFilename = mediaLocation;
			DShowPlayer.currentInputDevice = currentInputDevice;
			PreviousItemLocation = mediaLocation;
			ItemFirstPlay = true;
			DShowPlayer.ResumeFromPreviousPosition = ResumeFromPreviousPosition;
			DShowPlayer.OpenClip();
			DShowPlayer.SetVolume(gf.LiveItem.Format.MediaVolume);
			if (DShowPlayer.currentState == PlayState.Running)
			{
				int result = (!DShowPlayer.isVideo) ? 1 : 2;
				if (FirstLoad)
				{
					DShowPlayer.PauseClip();
				}
				return result;
			}
			return 0;
		}

		private int LoadLiveCam()
		{
			DShowPlayer.SetMute(gf.LiveCamMute);
			DShowPlayer.SetWideScreen(gf.LiveCamWidescreen, ResizeWindow: false);
			gf.MediaResetStartTime = true;
			DShowPlayer.newFilename = "<<Capture>>" + gf.LiveCamNumber;
			DShowPlayer.currentInputDevice = gf.LiveCamNumber;
			PreviousItemLocation = DShowPlayer.newFilename;
			ItemFirstPlay = true;
			DShowPlayer.ResumeFromPreviousPosition = false;
			DShowPlayer.OpenClip();
			DShowPlayer.SetVolume(gf.LiveCamVolume);
			if (DShowPlayer.currentState == PlayState.Running)
			{
				return (!DShowPlayer.isVideo) ? 1 : 2;
			}
			return 0;
		}

		private int UpdateLiveCam()
		{
			DShowPlayer.SetMute(gf.LiveCamMute);
			DShowPlayer.SetWideScreen(gf.LiveCamWidescreen, ResizeWindow: true);
			if (PreviousItemLocation != "<<Capture>>" + gf.LiveCamNumber)
			{
				gf.MediaResetStartTime = true;
				DShowPlayer.newFilename = "<<Capture>>" + gf.LiveCamNumber;
				DShowPlayer.currentInputDevice = gf.LiveCamNumber;
				PreviousItemLocation = DShowPlayer.newFilename;
				ItemFirstPlay = true;
				DShowPlayer.ResumeFromPreviousPosition = false;
				DShowPlayer.OpenClip();
				DShowPlayer.SetVolume(gf.LiveCamVolume);
			}
			if (DShowPlayer.currentState == PlayState.Running)
			{
				return (!DShowPlayer.isVideo) ? 1 : 2;
			}
			return 0;
		}

		private void FrmLaunchMediaPlayer_VisibleChanged(object sender, EventArgs e)
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
			}
		}

		private void TimerRefresh_Tick(object sender, EventArgs e)
		{
		}

		public bool Remote_GetMediaTimings(ref string Duration, ref string Position, ref int intDuration, ref int intPosition)
		{
			if (gf.WMP_Present && DShowPlayer.newFilename != "")
			{
				Duration = ((DShowPlayer.newFilename != "") ? DShowPlayer.GetClipDurationString() : "00:00");
				Position = DShowPlayer.GetCurrentPositionString();
				intDuration = DShowPlayer.GetClipDuration();
				intPosition = DShowPlayer.GetCurrentPosition();
				return true;
			}
			Duration = "";
			Position = "";
			return false;
		}
	}
}
