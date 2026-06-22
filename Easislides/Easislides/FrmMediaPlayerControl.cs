using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Easislides.Module;
using Easislides.Util;

namespace Easislides
{
	public partial class FrmMediaPlayerControl : Form
	{
		private enum ControlsBtn
		{
			PlayPausebtn,
			Stopbtn,
			FFbtn,
			FRbtn,
			Closebtn
		}

		private bool InitLoad = true;

		private double TimeIncrement = 1.0;

		private bool PreviousMuteState = false;

		private string Option1MediaFile = "";

		private DShowLib DShowPlayer = new DShowLib();

		private bool PlayerOK = false;

		public FrmMediaPlayerControl()
		{
			InitializeComponent();
		}

		private void FrmMediaPlayerControl_Load(object sender, EventArgs e)
		{
			groupBox1.Enabled = ((!(Gf.Temp_MediaItemType == "M") || Gf.MPC_Type != MPCType.Individual) ? true : false);
			if (Gf.MPC_Type == MPCType.Individual)
			{
				Text = "Assign Media " + ((Gf.Temp_MediaTitle1 != "") ? "for " : "") + Gf.Temp_MediaTitle1;
			}
			else
			{
				Text = "Assign Media - Default Settings";
			}
			SourceOption1.Text = "Play Media File based on Item Title (if any)";
			Gf.LoadBlankCaptureDevices(ref cbCaptureDevices);
			InitMediaPlayer();
			LoadOutputMonitors();
			tbSourceLocation.Text = Gf.Temp_MediaLocation;
			cbCaptureDevices.SelectedIndex = Gf.Temp_MediaCaptureDeviceNumber - 1;
			TrackBarVolume.Value = (((Gf.Temp_MediaVolume >= 0) & (Gf.Temp_MediaVolume <= 100)) ? Gf.Temp_MediaVolume : 50);
			TrackBarBalance.Value = (((Gf.Temp_MediaBalance >= -100) & (Gf.Temp_MediaBalance <= 100)) ? Gf.Temp_MediaBalance : 0);
			cbMute.Checked = ((Gf.Temp_MediaMute > 0) ? true : false);
			cbRepeat.Checked = ((Gf.Temp_MediaRepeat > 0) ? true : false);
			cbWidescreen.Checked = ((Gf.Temp_MediaWidescreen > 0) ? true : false);
			LabelMediaType.Text = "";
			LabelResolution.Text = "";
			AssignSourceOption(Gf.Temp_MediaOption);
			ApplySoundControls(ApplyMute: false);
			TimerTrack.Start();
			InitLoad = false;
		}

		private void InitMediaPlayer()
		{
			if (Gf.WMP_Present)
			{
				try
				{
					DShowPlayer.Parent = this;
					DShowPlayer.Parent = panel1;
					DShowPlayer.Location = new Point(0, 0);
					DShowPlayer.SetDefaultSize(0, 0, panel1.Width, panel1.Height, (VAlign)Gf.VideoVAlign);
					DShowPlayer.ForeColorChanged += DShowPlayer_ForeColorChanged;
					DShowPlayer.ListCaptureDevices(ref cbCaptureDevices);
					PlayerOK = true;
				}
				catch
				{
					PlayerOK = false;
				}
			}
			if (PlayerOK)
			{
				DShowPlayer.Dock = DockStyle.Fill;
				DShowPlayer.newFilename = tbSourceLocation.Text;
				panelNoPlayer.Visible = false;
				EnableMediaControls(MediaOn: true);
				DShowPlayer.Visible = true;
			}
			else
			{
				EnableMediaControls(MediaOn: false);
			}
		}

		private void LoadOutputMonitors()
		{
			cbOutputMonitor.Items.Clear();
			cbOutputMonitor.Items.Add("Default (Output Monitor)");
			foreach (Screen screen in Screen.AllScreens)
			{
				cbOutputMonitor.Items.Add(screen.DeviceName);
			}
			SelectOutputMonitor(Gf.Temp_MediaOutputMonitorName);
		}

		private void SelectOutputMonitor(string monitorName)
		{
			if (!string.IsNullOrEmpty(monitorName))
			{
				int index = cbOutputMonitor.Items.IndexOf(monitorName);
				if (index >= 0)
				{
					cbOutputMonitor.SelectedIndex = index;
					return;
				}
			}
			cbOutputMonitor.SelectedIndex = 0;
		}

		private void EnableMediaControls(bool MediaOn)
		{
			panelNoPlayer.Visible = !MediaOn;
			panelPlayBtns.Enabled = MediaOn;
		}

		private void AssignSourceOption(int InOption)
		{
			switch (InOption)
			{
			case 1:
				SourceOption1.Checked = true;
				break;
			case 2:
				SourceOption2.Checked = true;
				break;
			case 3:
				SourceOption3.Checked = true;
				break;
			default:
				SourceOption0.Checked = true;
				break;
			}
		}

		private void TrackBarVolume_ValueChanged(object sender, EventArgs e)
		{
			if (!InitLoad)
			{
				ApplySoundControls(ApplyMute: false);
			}
		}

		private void TrackBarBalance_ValueChanged(object sender, EventArgs e)
		{
			if (!InitLoad)
			{
				ApplySoundControls(ApplyMute: false);
			}
		}

		private void cbMute_CheckedChanged(object sender, EventArgs e)
		{
			if (!InitLoad)
			{
				ApplySoundControls(ApplyMute: false);
			}
		}

		private void cbRepeat_CheckedChanged(object sender, EventArgs e)
		{
			if (!InitLoad)
			{
				ApplySoundControls(ApplyMute: false);
			}
		}

		private void cbWidescreen_CheckedChanged(object sender, EventArgs e)
		{
			SetWideScreen(cbWidescreen.Checked);
		}

		private void SetWideScreen(bool InMode)
		{
			if (PlayerOK)
			{
				DShowPlayer.SetWideScreen(InMode, ResizeWindow: true);
				LabelResolution.Text = DShowPlayer.GetVideoSize();
			}
		}

		private void ApplySoundControls(bool ApplyMute)
		{
			if (PlayerOK)
			{
				DShowPlayer.SetVolume(TrackBarVolume.Value);
				DShowPlayer.SetBalance(TrackBarBalance.Value);
				DShowPlayer.SetMute(ApplyMute || cbMute.Checked);
				DShowPlayer.LoopClip = cbRepeat.Checked;
			}
		}

		private void PlayPauseBtn_Click(object sender, EventArgs e)
		{
			ApplyPlayControls(ControlsBtn.PlayPausebtn);
		}

		private void StopBtn_Click(object sender, EventArgs e)
		{
			ApplyPlayControls(ControlsBtn.Stopbtn);
		}

		private void FastReverseBtn_MouseDown(object sender, MouseEventArgs e)
		{
			ApplyPlayControls(ControlsBtn.FRbtn);
		}

		private void FastReverseBtn_MouseUp(object sender, MouseEventArgs e)
		{
			ReturnToPreviousState();
		}

		private void FastForwardBtn_MouseDown(object sender, MouseEventArgs e)
		{
			ApplyPlayControls(ControlsBtn.FFbtn);
		}

		private void FastForwardBtn_MouseUp(object sender, MouseEventArgs e)
		{
			ReturnToPreviousState();
		}

		private void StorePreviousStatus()
		{
			PreviousMuteState = cbMute.Checked;
		}

		private void ReturnToPreviousState()
		{
			TimerFast.Stop();
			ApplySoundControls(ApplyMute: false);
		}

		private void ApplyPlayControls(ControlsBtn InAction)
		{
			if (!PlayerOK)
			{
				return;
			}
			TimerFast.Stop();
			switch (InAction)
			{
			case ControlsBtn.PlayPausebtn:
			{
				if (!SourceOption3.Checked && (DShowPlayer.currentState == PlayState.Running || DShowPlayer.currentState == PlayState.Paused))
				{
					DShowPlayer.PausePlayClip();
					break;
				}
				tbSourceLocation.Text = DataUtil.Trim(tbSourceLocation.Text);
				int selectedSourceOption = GetSelectedSourceOption();
				try
				{
					switch (selectedSourceOption)
					{
					case 1:
						Option1MediaFile = Gf.GetMediaFileName(Gf.Temp_MediaTitle1, Gf.Temp_MediaTitle2);
						if (Option1MediaFile == "")
						{
							SourceOption1.Text = "Play Media File based on Item Title (if any)";
							toolTip1.SetToolTip(SourceOption1, "");
						}
						else
						{
							SourceOption1.Text = Option1MediaFile;
							toolTip1.SetToolTip(SourceOption1, SourceOption1.Text);
						}
						DShowPlayer.newFilename = Option1MediaFile;
						break;
					case 2:
						DShowPlayer.newFilename = tbSourceLocation.Text;
						break;
					case 3:
						DShowPlayer.newFilename = "<<Capture>>";
						DShowPlayer.currentInputDevice = cbCaptureDevices.SelectedIndex + 1;
						break;
					default:
						DShowPlayer.newFilename = "";
						break;
					}
					SetWideScreen(cbWidescreen.Checked);
					if (selectedSourceOption == 3 || DShowPlayer.newFilename != "")
					{
						DShowPlayer.OpenClip();
						LabelMediaType.Text = DShowPlayer.GetStatusText();
						LabelResolution.Text = DShowPlayer.GetVideoSize();
					}
					else
					{
						ResetMediaMessages();
					}
				}
				catch
				{
					DShowPlayer.newFilename = "";
					ResetMediaMessages();
				}
				return;
			}
			case ControlsBtn.Stopbtn:
				DShowPlayer.StopClip();
				break;
			case ControlsBtn.FFbtn:
				StorePreviousStatus();
				ApplySoundControls(ApplyMute: true);
				IncrementCurrentPosition(1.0);
				TimeIncrement = 5.0;
				TimerFast.Start();
				break;
			case ControlsBtn.FRbtn:
				ApplySoundControls(ApplyMute: true);
				StorePreviousStatus();
				IncrementCurrentPosition(-1.0);
				TimeIncrement = -5.0;
				TimerFast.Start();
				break;
			case ControlsBtn.Closebtn:
				DShowPlayer.StopClip();
				break;
			}
			Cursor = Cursors.Default;
		}

		private void ResetMediaMessages()
		{
			if (PlayerOK)
			{
				LabelMediaType.Text = DShowPlayer.GetStatusText();
				LabelResolution.Text = DShowPlayer.GetVideoSize();
			}
			else
			{
				LabelMediaType.Text = "";
				LabelResolution.Text = "";
			}
			Cursor = Cursors.Default;
		}

		private void DShowPlayer_ForeColorChanged(object sender, EventArgs e)
		{
			switch (DShowPlayer.currentState)
			{
			case PlayState.Running:
				PlayPauseBtn.Enabled = true;
				StopBtn.Enabled = true;
				PlayPauseBtn.Text = "Pause";
				LabelMediaType.Text = DShowPlayer.GetStatusText();
				LabelResolution.Text = DShowPlayer.GetVideoSize();
				Cursor = Cursors.Default;
				break;
			case PlayState.Paused:
				PlayPauseBtn.Text = "Play";
				Cursor = Cursors.Default;
				break;
			case PlayState.Stopped:
				StopBtn.Enabled = false;
				PlayPauseBtn.Text = "Play";
				Cursor = Cursors.Default;
				break;
			default:
				StopBtn.Enabled = false;
				PlayPauseBtn.Text = "Play";
				break;
			}
		}

		private void FrmMediaPlayerControl_FormClosing(object sender, FormClosingEventArgs e)
		{
			ApplyPlayControls(ControlsBtn.Closebtn);
			DShowPlayer.TidyUp();
			TimerTrack.Stop();
			TimerFast.Stop();
		}

		private void BtnOK_Click(object sender, EventArgs e)
		{
			Gf.Temp_MediaOption = GetSelectedSourceOption();
			Gf.Temp_MediaLocation = DataUtil.Trim(tbSourceLocation.Text);
			Gf.Temp_MediaCaptureDeviceNumber = cbCaptureDevices.SelectedIndex + 1;
			Gf.Temp_MediaOutputMonitorName = (cbOutputMonitor.SelectedIndex > 0) ? cbOutputMonitor.SelectedItem.ToString() : "";
			Gf.Temp_MediaVolume = TrackBarVolume.Value;
			Gf.Temp_MediaBalance = TrackBarBalance.Value;
			Gf.Temp_MediaMute = (cbMute.Checked ? 1 : 0);
			Gf.Temp_MediaRepeat = (cbRepeat.Checked ? 1 : 0);
			Gf.Temp_MediaWidescreen = (cbWidescreen.Checked ? 1 : 0);
		}

		private int GetSelectedSourceOption()
		{
			if (SourceOption1.Checked)
			{
				return 1;
			}
			if (SourceOption2.Checked)
			{
				return 2;
			}
			if (SourceOption3.Checked)
			{
				return 3;
			}
			return 0;
		}

		private void LocationBtn_MouseUp(object sender, MouseEventArgs e)
		{
			OpenFileDialog1.Filter = gfFileHelpers.GetOpenFileDialogMediaString();
			OpenFileDialog1.InitialDirectory = Gf.MediaDir;
			OpenFileDialog1.AddExtension = true;
			tbSourceLocation.Text = DataUtil.Trim(tbSourceLocation.Text);
			OpenFileDialog1.FileName = tbSourceLocation.Text;
			bool flag = false;
			try
			{
				if (OpenFileDialog1.ShowDialog() == DialogResult.OK)
				{
					ApplyPlayControls(ControlsBtn.Stopbtn);
					tbSourceLocation.Text = OpenFileDialog1.FileName;
					if (PlayerOK)
					{
						DShowPlayer.newFilename = tbSourceLocation.Text;
					}
					ApplySoundControls(ApplyMute: false);
				}
			}
			catch
			{
				flag = true;
			}
			if (flag)
			{
				try
				{
					OpenFileDialog1.FileName = "";
					if (OpenFileDialog1.ShowDialog() == DialogResult.OK)
					{
						ApplyPlayControls(ControlsBtn.Stopbtn);
						tbSourceLocation.Text = OpenFileDialog1.FileName;
						if (PlayerOK)
						{
							DShowPlayer.newFilename = tbSourceLocation.Text;
						}
						ApplySoundControls(ApplyMute: false);
					}
				}
				catch
				{
				}
			}
		}

		private void TimerFast_Tick(object sender, EventArgs e)
		{
			IncrementCurrentPosition(TimeIncrement);
		}

		private void IncrementCurrentPosition(double InIncrement)
		{
			if (PlayerOK)
			{
				DShowPlayer.SetCurrentPosition((double)DShowPlayer.GetCurrentPosition() + InIncrement);
				SetDurationSettings();
			}
		}

		private void SetDurationSettings()
		{
			if (PlayerOK && DShowPlayer.GetClipDuration() > 0)
			{
				SetDurationSettings(ResetAll: false);
			}
			else
			{
				SetDurationSettings(ResetAll: true);
			}
		}

		private void SetDurationSettings(bool ResetAll)
		{
			if (ResetAll)
			{
				if (LabelMediaType.Text != "" && ((LabelMediaType.Text[0] == 'A') | (LabelMediaType.Text[0] == 'V')))
				{
					LabelDuration.Text = "Streaming Contents";
				}
				else
				{
					LabelDuration.Text = "00:00";
				}
				LabelPosition.Text = "00:00";
				TrackBarDuration.Maximum = 0;
				TrackBarDuration.Value = 0;
			}
			else if (PlayerOK)
			{
				LabelDuration.Text = ((DShowPlayer.newFilename != "") ? DShowPlayer.GetClipDurationString() : "00:00");
				LabelPosition.Text = DShowPlayer.GetCurrentPositionString();
				TrackBarDuration.Maximum = DShowPlayer.GetClipDuration();
				TrackBarDuration.Value = ((DShowPlayer.GetCurrentPosition() > TrackBarDuration.Maximum) ? TrackBarDuration.Maximum : DShowPlayer.GetCurrentPosition());
			}
			else
			{
				LabelDuration.Text = "00:00";
				LabelPosition.Text = "00:00";
				TrackBarDuration.Maximum = 1000;
				TrackBarDuration.Value = 0;
			}
		}

		private void TimerTrack_Tick(object sender, EventArgs e)
		{
			SetDurationSettings();
		}

		private void TrackBarDuration_Scroll(object sender, EventArgs e)
		{
			if (PlayerOK)
			{
				DShowPlayer.SetCurrentPosition(TrackBarDuration.Value);
			}
		}

		private void tbSourceLocation_TextChanged(object sender, EventArgs e)
		{
			if (!InitLoad)
			{
				SourceOption2.Checked = true;
			}
		}

		private void TimerAttemptConnect_Tick(object sender, EventArgs e)
		{
		}

		private void SourceOption3_CheckedChanged(object sender, EventArgs e)
		{
			if (SourceOption3.Checked)
			{
				RestartInputDevice();
			}
		}

		private void RestartInputDevice()
		{
			ApplyPlayControls(ControlsBtn.Stopbtn);
			ApplyPlayControls(ControlsBtn.PlayPausebtn);
		}

		private void SourceOption2_CheckedChanged(object sender, EventArgs e)
		{
			if (SourceOption2.Checked)
			{
				ApplyPlayControls(ControlsBtn.Stopbtn);
			}
		}

		private void cbCaptureDevicesAndTypes_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (!InitLoad && SourceOption3.Checked)
			{
				RestartInputDevice();
			}
		}
	}
}
