using Axicultus.Module;
using Axicultus.Util;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Axicultus;

	public partial class FrmLaunchMediaPlayer : Form
	{
		public delegate void Message(int MsgCode, string MsgString);

		private string PreviousItemLocation = "";

		private bool ItemFirstPlay = false;

		private Rectangle? outputBounds;

		private DShowLib DShowPlayer = new();

		public event Message OnMessage;

		public FrmLaunchMediaPlayer()
		{
			InitializeComponent();
			StartPosition = FormStartPosition.Manual;
			TimerRefresh.Tick += (_, _) => UpdateProgress();
		}

		// Live audio can play with the form hidden. Initialize the DirectShow control once
		// for both the hidden player and the standalone output window.
		internal void PreparePlayer()
		{
			if (!Gf.WMP_Present || DShowPlayer.Parent != null)
				return;
			DShowPlayer.Parent = this;
			DShowPlayer.Location = Point.Empty;
			ApplyPlayerBounds(ClientSize.Width, ClientSize.Height);
			DShowPlayer.ForeColorChanged += DShowPlayer_ForeColorChanged;
			TimerRefresh.Start();
		}

		private void FrmLaunchMediaPlayer_Load(object sender, EventArgs e)
		{
			Bounds = outputBounds ?? new Rectangle(Gf.LS_Left, Gf.LS_Top, Gf.LS_Width, Gf.LS_Height);
			PreparePlayer();
		}

		protected override bool ShowWithoutActivation => true;

		internal void ApplyOutputMonitor(MonitorPlacement placement)
		{
			outputBounds = placement.Bounds;
			Gf.CurrentMediaOutputMonitorName = placement.DeviceName;
			SetShowWindow(placement.Bounds);
		}

		private void SetShowWindow(Rectangle bounds)
		{
			Bounds = bounds;
			if (Gf.WMP_Present)
			{
				ApplyPlayerBounds(bounds.Width, bounds.Height);
				if (DShowPlayer.isVideo)
					DShowPlayer.InitVideoWindow();
			}
		}

		private void ApplyPlayerBounds(int screenWidth, int screenHeight)
		{
			int playerWidth = screenWidth * Gf.VideoSize / 100;
			int playerHeight = screenHeight * Gf.VideoSize / 100;
			int playerLeft = (screenWidth - playerWidth) / 2 - 1;
			playerLeft = ((playerLeft >= 0) ? playerLeft : 0);
			int playerTop = (Gf.VideoVAlign == 1) ? ((screenHeight - playerHeight) / 2) : ((Gf.VideoVAlign == 2) ? (screenHeight - playerHeight) : 0);
			DShowPlayer.SetDefaultSize(playerLeft, playerTop, playerWidth, playerHeight, (VAlign)Gf.VideoVAlign);
		}

		private void DShowPlayer_ForeColorChanged(object sender, EventArgs e)
		{
			if (DShowPlayer.ForeColor == Color.Red)
			{
				if (Gf.ShowRunning)
				{
					if (DShowPlayer.MouseBtnPressed == MouseButtons.Left)
					{
						OnMessage?.Invoke(11, "");
					}
					else if (DShowPlayer.MouseBtnPressed == MouseButtons.Right)
					{
						OnMessage?.Invoke(12, "");
					}
				}
				return;
			}
			if (DShowPlayer.ForeColor == Color.Blue)
			{
				if (Gf.ShowRunning)
				{
					OnMessage?.Invoke(13, "");
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
				Gf.MediaNotifyItemConnecting = false;
				if (ItemFirstPlay)
				{
					if (!Gf.ShowLiveCam)
					{
						Gf.MediaLiveItemStartTime = DateTime.Now;
						Gf.MediaPlayedLapseTime = new TimeSpan(0L);
					}
				}
				else
				{
					Gf.MediaDoRotate = false;
				}
				ItemFirstPlay = false;
				bool loopClip = false;
				int clipDuration = DShowPlayer.GetClipDuration();
				if (clipDuration == Gf.LiveItem.RotateTotal || (Gf.LiveItem.RotateStyle == 2 && Gf.LiveItem.RotateTotal == 0))
				{
					if (!Gf.ShowLiveCam)
					{
						Gf.MediaLengthAsRotateLength = true;
					}
				}
				else
				{
					if (!Gf.ShowLiveCam)
					{
						Gf.MediaLengthAsRotateLength = false;
					}
					if (Gf.LiveItem.Format.MediaRepeat > 0 && !Gf.RestartCurrentItem)
					{
						loopClip = true;
					}
				}
				DShowPlayer.LoopClip = loopClip;
				break;
			}
			case PlayState.Stopped:
				DShowPlayer.currentVolume = 0;
				if (!Gf.ShowLiveCam)
				{
					Gf.MediaPlayedLapseTime = DateTime.Now.Subtract(Gf.MediaLiveItemStartTime) + new TimeSpan(0, 0, 1);
				}
				break;
			}
		}

		public void Remote_StopItem()
		{
			if (Gf.WMP_Present)
			{
				DShowPlayer.CloseClip();
			}
		}

		public void Remote_PauseItem()
		{
			if (Gf.WMP_Present)
			{
				DShowPlayer.PauseClip();
			}
		}

		public void Remote_PausePlayItem()
		{
			if (Gf.WMP_Present)
			{
				DShowPlayer.PausePlayClip();
			}
		}

		public int Remote_LoadItem()
		{
			if (Gf.WMP_Present)
			{
				return LoadMedia(ResumeFromPreviousPosition: false, FirstLoad: true);
			}
			return 0;
		}

		/// <summary>
		/// Loads the live item's media and leaves it running. Unlike Remote_LoadItem,
		/// which pauses on the first frame so the item can be started later, this
		/// builds the DirectShow graph once and plays it straight away.
		/// </summary>
		public int Remote_PlayItem()
		{
			if (Gf.WMP_Present)
			{
				return LoadMedia(ResumeFromPreviousPosition: false, FirstLoad: false);
			}
			return 0;
		}

		public int Remote_ResumeItem()
		{
			if (Gf.WMP_Present)
			{
				return LoadMedia(ResumeFromPreviousPosition: true, FirstLoad: false);
			}
			return 0;
		}

		public void Remote_ResumeItemFromStart()
		{
			if (Gf.WMP_Present)
			{
				DShowPlayer.ResumeFromStart();
			}
		}

		public int Remote_LoadLiveCam()
		{
			if (Gf.WMP_Present)
			{
				return LoadLiveCam();
			}
			return 0;
		}

		public int Remote_UpdateLiveCam()
		{
			if (Gf.WMP_Present)
			{
				return UpdateLiveCam();
			}
			return 0;
		}

		public void Remote_ClearScreen()
		{
			if (Gf.WMP_Present)
			{
				DShowPlayer.CloseClip();
			}
		}

		public int Remote_RepeatItem()
		{
			if (Gf.WMP_Present)
			{
				Gf.MediaNotifyRepeatItem = false;
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
			if (Gf.WMP_Present && DShowPlayer.currentState == PlayState.Running)
			{
				return true;
			}
			return false;
		}

		public void Remote_RefreshMediaWindow()
		{
			if (Gf.WMP_Present)
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
			if (Gf.WMP_Present)
			{
				return !DShowPlayer.isVideo;
			}
			return false;
		}

		private void UpdateProgress()
		{
			if (DShowPlayer.currentState != PlayState.Running || Gf.ShowLiveCam)
			{
				return;
			}
			int currentPosition = DShowPlayer.GetCurrentPosition();
			if (currentPosition < 0)
			{
				return;
			}
			if ((int)Gf.MediaPlayedLapseTime.TotalSeconds <= currentPosition)
			{
				if (!Gf.ShowLiveCam && Gf.MediaDoRotate)
				{
					Gf.MediaPlayedLapseTime = new TimeSpan(0, 0, currentPosition);
				}
			}
			else if (!Gf.ShowLiveCam && Gf.MediaDoRotate)
			{
				Gf.MediaPlayedLapseTime = DateTime.Now.Subtract(Gf.MediaLiveItemStartTime);
			}
		}

		private void FrmLaunchMediaPlayer_FormClosing(object sender, FormClosingEventArgs e)
		{
			DShowPlayer.TidyUp();
		}

		private int LoadMedia(bool ResumeFromPreviousPosition, bool FirstLoad)
		{
			string mediaLocation = Gf.GetMediaLocation(Gf.LiveItem);
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
			DShowPlayer.SetMute((Gf.LiveItem.Format.MediaMute > 0) ? true : false);
			DShowPlayer.SetWideScreen((Gf.LiveItem.Format.MediaWidescreen > 0) ? true : false, ResizeWindow: false);
			Gf.MediaResetStartTime = true;
			DShowPlayer.newFilename = mediaLocation;
			DShowPlayer.currentInputDevice = currentInputDevice;
			PreviousItemLocation = mediaLocation;
			ItemFirstPlay = true;
			DShowPlayer.ResumeFromPreviousPosition = ResumeFromPreviousPosition;
			DShowPlayer.OpenClip();
			DShowPlayer.SetVolume(Gf.LiveItem.Format.MediaVolume);
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
			DShowPlayer.SetMute(Gf.LiveCamMute);
			DShowPlayer.SetWideScreen(Gf.LiveCamWidescreen, ResizeWindow: false);
			Gf.MediaResetStartTime = true;
			DShowPlayer.newFilename = "<<Capture>>" + Gf.LiveCamNumber;
			DShowPlayer.currentInputDevice = Gf.LiveCamNumber;
			PreviousItemLocation = DShowPlayer.newFilename;
			ItemFirstPlay = true;
			DShowPlayer.ResumeFromPreviousPosition = false;
			DShowPlayer.OpenClip();
			DShowPlayer.SetVolume(Gf.LiveCamVolume);
			if (DShowPlayer.currentState == PlayState.Running)
			{
				return (!DShowPlayer.isVideo) ? 1 : 2;
			}
			return 0;
		}

		private int UpdateLiveCam()
		{
			DShowPlayer.SetMute(Gf.LiveCamMute);
			DShowPlayer.SetWideScreen(Gf.LiveCamWidescreen, ResizeWindow: true);
			if (PreviousItemLocation != "<<Capture>>" + Gf.LiveCamNumber)
			{
				Gf.MediaResetStartTime = true;
				DShowPlayer.newFilename = "<<Capture>>" + Gf.LiveCamNumber;
				DShowPlayer.currentInputDevice = Gf.LiveCamNumber;
				PreviousItemLocation = DShowPlayer.newFilename;
				ItemFirstPlay = true;
				DShowPlayer.ResumeFromPreviousPosition = false;
				DShowPlayer.OpenClip();
				DShowPlayer.SetVolume(Gf.LiveCamVolume);
			}
			if (DShowPlayer.currentState == PlayState.Running)
			{
				return (!DShowPlayer.isVideo) ? 1 : 2;
			}
			return 0;
		}

		public bool Remote_GetMediaTimings(ref string Duration, ref string Position, ref int intDuration, ref int intPosition)
		{
			if (Gf.WMP_Present && DShowPlayer.newFilename != "")
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
