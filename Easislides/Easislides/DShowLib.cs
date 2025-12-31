#define DEBUG
using System;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Windows.Forms;
using DirectShowLib;
using Easislides.Module;
using Easislides.Util;

namespace Easislides
{
	internal class DShowLib : Control
	{
        private const int VolumeFull = 0;

		private const int VolumeSilence = -10000;

		private const int WM_GRAPHNOTIFY = 32769;

		public int VolumeStored = 2000;

		private IGraphBuilder graphBuilder = null;

		private IMediaControl mediaControl = null;

		private IMediaEventEx mediaEventEx = null;

		private IVideoWindow videoWindow = null;

		private IBasicAudio basicAudio = null;

		private IBasicVideo2 basicVideo2 = null;

		private IMediaSeeking mediaSeeking = null;

		private IVideoFrameStep frameStep = null;

		private ICaptureGraphBuilder2 captureGraphBuilder = null;

		public IMediaPosition mediaPosition = null;

		public string newFilename = string.Empty;

		public string curFilename = string.Empty;

		public int currentInputDevice = 1;

//		private string currentInputDeviceType = "Video";

//		private Guid currentInputDeviceGuid;

		public bool isVideo = false;

		private bool isFullScreen = false;

		public int currentVolume = 0;

		public bool isWidescreen = false;

		private double currentPlaybackRate = 1.0;

		private IntPtr hDrain = IntPtr.Zero;

		public PlayState currentState = PlayState.Stopped;

		public bool LoopClip = false;

		public int PreviousMediaStoppedPosition = 0;

		public bool ResumeFromPreviousPosition = false;

		public int Default_Left = 0;

		public int Default_Top = 0;

		public int Default_Width = 0;

		public int Default_Heigth = 0;

		public VAlign Default_Align = VAlign.Centre;

		public int Media_Width = 0;

		public int Media_Height = 0;

		public MouseButtons MouseBtnPressed = MouseButtons.Left;

		private Cursor LiveCursor;

		private static int RCount = 0;

		public DShowLib()
		{
			BackColor = gf.TransparentColour;
			Bitmap bitmap = new Bitmap(55, 25);
			//Graphics graphics = Graphics.FromImage(bitmap);
			IntPtr hicon = bitmap.GetHicon();
			LiveCursor = new Cursor(hicon);
			Cursor = LiveCursor;
			base.MouseDown += DShowLib_MouseDown;
			base.MouseUp += DShowLib_MouseUp;
            //bitmap.Dispose();
            //graphics.Dispose();
        }

		private void DShowLib_MouseDown(object sender, MouseEventArgs e)
		{
			MouseBtnPressed = e.Button;
			ForeColor = Color.Red;
		}

		private void DShowLib_MouseUp(object sender, MouseEventArgs e)
		{
			ForeColor = Color.Blue;
		}

		public void SetDefaultSize(int l, int t, int w, int h, VAlign a)
		{
			Default_Left = l;
			Default_Top = t;
			Default_Width = w;
			Default_Heigth = h - 1;
			Default_Align = a;
		}

		private void SetPlayState(PlayState InState)
		{
			currentState = InState;
			ForeColor = ((ForeColor == Color.Black) ? Color.White : Color.Black);
		}

		public void OpenClip()
		{
			try
			{
				if (!(newFilename == string.Empty))
				{
					if (currentState != PlayState.Running || !ResumeFromPreviousPosition)
					{
						CloseClip();
						if (DataUtil.Left(newFilename, "<<Capture>>".Length) == "<<Capture>>")
						{
							CaptureVideo();
						}
						else
						{
							PlayMovieInWindow(newFilename);
						}
					}
					curFilename = newFilename;
				}
			}
			catch
			{
				CloseClip();
			}
		}

		private void PlayMovieInWindow(string filename)
		{
			int num = 0;
			if (!(filename == string.Empty))
			{
				OpenInterfaces();
				num = graphBuilder.RenderFile(filename, null);
				DsError.ThrowExceptionForHR(num);
				int num2 = CheckVisibility();
				num = mediaEventEx.SetNotifyWindow(base.Handle, 32769, IntPtr.Zero);
				DsError.ThrowExceptionForHR(num);
				if (isVideo)
				{
					try
					{
						num = videoWindow.put_Owner(base.Handle);
						DsError.ThrowExceptionForHR(num);
						num = videoWindow.put_WindowStyle(WindowStyle.Child | WindowStyle.ClipSiblings | WindowStyle.ClipChildren);
						DsError.ThrowExceptionForHR(num);
						num = videoWindow.put_Visible(OABool.True);
						DsError.ThrowExceptionForHR(num);
						num = InitVideoWindow();
						DsError.ThrowExceptionForHR(num);
						GetFrameStepInterface();
					}
					catch
					{
					}
				}
				else
				{
					num = InitPlayerWindow();
					DsError.ThrowExceptionForHR(num);
				}
				isFullScreen = false;
				currentPlaybackRate = 1.0;
				num = mediaControl.Run();
				DsError.ThrowExceptionForHR(num);
				if (ResumeFromPreviousPosition)
				{
					SetCurrentPosition(PreviousMediaStoppedPosition);
				}
				SetPlayState(PlayState.Running);
			}
		}

		private void OpenInterfaces()
		{
			graphBuilder = (IGraphBuilder)new FilterGraph();
			captureGraphBuilder = (ICaptureGraphBuilder2)new CaptureGraphBuilder2();
			mediaControl = (IMediaControl)graphBuilder;
			mediaEventEx = (IMediaEventEx)graphBuilder;
			mediaSeeking = (IMediaSeeking)graphBuilder;
			mediaPosition = (IMediaPosition)graphBuilder;
			basicVideo2 = (IBasicVideo2)graphBuilder;
			videoWindow = (IVideoWindow)graphBuilder;
			basicAudio = (IBasicAudio)graphBuilder;
		}

		public void CloseClip()
		{
			int num = 0;
			PauseClip();
			if (DataUtil.Left(newFilename, "<<Capture>>".Length) == "<<Capture>>")
			{
				PreviousMediaStoppedPosition = GetCurrentPosition();
			}
			if (mediaControl != null)
			{
				num = mediaControl.Stop();
			}
			SetPlayState(PlayState.Stopped);
			isVideo = false;
			isFullScreen = false;
			CloseInterfaces();
			SetPlayState(PlayState.Init);
			Media_Width = 0;
			Media_Height = 0;
			InitPlayerWindow();
		}

		public void StopClip()
		{
			int num = 0;
			DsLong pCurrent = new DsLong(0L);
			if (mediaControl != null && mediaSeeking != null && (currentState == PlayState.Paused || currentState == PlayState.Running))
			{
				num = mediaControl.Stop();
				SetPlayState(PlayState.Stopped);
				num = mediaSeeking.SetPositions(pCurrent, AMSeekingSeekingFlags.AbsolutePositioning, null, AMSeekingSeekingFlags.NoPositioning);
				num = mediaControl.Pause();
			}
		}

		public int InitVideoWindow()
		{
			int num = 0;
			bool flag = false;
			int pWidth;
			int pHeight;
			int num2;
			int num3;
			int plAspectX;
			int plAspectY;
			if (DataUtil.Left(newFilename, "<<Capture>>".Length) == "<<Capture>>")
			{
				if (videoWindow == null)
				{
					DebugMessage("videoWindow Quit: Null");
					return 0;
				}
				num = basicVideo2.GetVideoSize(out pWidth, out pHeight);
				DebugMessage("InitVideoWindow Invideo " + pWidth + " " + pHeight);
				if (num == -2147220971)
				{
					DebugMessage("InitVideoWindow Quit: GetVideoSize Error");
					return 0;
				}
				num2 = Default_Width;
				num3 = Default_Heigth;
				num = basicVideo2.GetPreferredAspectRatio(out plAspectX, out plAspectY);
				DebugMessage("InitVideoWindow aspect " + pWidth + " " + pHeight);
				if (num == -2147220971)
				{
					DebugMessage("InitVideoWindow Quit: GetPreferredAspectRatio Error");
					return 0;
				}
			}
			else
			{
				if (basicVideo2 == null)
				{
					DebugMessage("basicVideo2 Quit: Null");
					return 0;
				}
				num = basicVideo2.GetVideoSize(out pWidth, out pHeight);
				DebugMessage("InitVideoWindow Invideo " + pWidth + " " + pHeight);
				if (num == -2147220971)
				{
					DebugMessage("InitVideoWindow Quit: GetVideoSize Error");
					return 0;
				}
				num2 = pWidth;
				num3 = pHeight;
				num = basicVideo2.GetPreferredAspectRatio(out plAspectX, out plAspectY);
				DebugMessage("InitVideoWindow aspect " + pWidth + " " + pHeight);
				if (num == -2147220971)
				{
					DebugMessage("InitVideoWindow Quit: GetPreferredAspectRatio Error");
					return 0;
				}
			}
			if (pWidth <= 0 || pHeight <= 0)
			{
				DebugMessage("Cannot continue with video because of invalid video size");
				return -1;
			}
			if (plAspectX == 16 && plAspectY == 9)
			{
				flag = true;
				num2 = num3 * plAspectX / plAspectY;
			}
			if (isWidescreen && !flag)
			{
				num2 = num3 * 16 / 9;
			}
			else if (plAspectX > 0 && plAspectY > 0)
			{
				num2 = num3 * plAspectX / plAspectY;
			}
			DebugMessage("Adjusted Media Size: " + num2 + " " + num3);
			double num4 = (double)num2 / (double)num3;
			double num5 = (double)Default_Width / (double)Default_Heigth;
			int left;
			int num6;
			int num7;
			int top;
			if (num5 < num4)
			{
				left = Default_Left;
				num6 = Default_Width;
				num7 = (int)((double)num6 / num4);
				pHeight = (int)((double)pWidth / num4);
				top = ((Default_Align == VAlign.Top) ? Default_Top : ((Default_Align != VAlign.Centre) ? (Default_Top + (Default_Heigth - num7)) : (Default_Top + (Default_Heigth - num7) / 2)));
			}
			else
			{
				top = Default_Top;
				num7 = Default_Heigth;
				num6 = (int)((double)num7 * num4);
				pWidth = (int)((double)pHeight * num4);
				left = Default_Left + (Default_Width - num6) / 2;
			}
			Media_Width = pWidth;
			Media_Height = pHeight;
			DebugMessage("Final Media Size: " + Media_Width + " " + Media_Height);
			Dock = DockStyle.Fill;
			Application.DoEvents();
			num = videoWindow.SetWindowPosition(left, top, num6, num7);
			OABool hideCursor = OABool.True;
			videoWindow.HideCursor(hideCursor);
			return num;
		}

		public string GetVideoSize()
		{
			if (Media_Width > 0)
			{
				return Media_Width + " x " + Media_Height;
			}
			return "";
		}

		private int CheckVisibility()
		{
			int num = 0;
			if (videoWindow == null || basicVideo2 == null)
			{
				isVideo = false;
				return -1;
			}
			isVideo = true;
			num = videoWindow.get_Visible(out OABool _);
			if (num < 0)
			{
				if (num == -2147467262)
				{
					isVideo = false;
				}
				else
				{
					DsError.ThrowExceptionForHR(num);
				}
			}
			return num;
		}

		private bool GetFrameStepInterface()
		{
			int num = 0;
			IVideoFrameStep videoFrameStep = null;
			videoFrameStep = (IVideoFrameStep)graphBuilder;
			if (videoFrameStep.CanStep(0, null) == 0)
			{
				frameStep = videoFrameStep;
				return true;
			}
			Marshal.ReleaseComObject(videoFrameStep);
			//GC.Collect();
			return false;
		}

		public void CloseInterfaces()
		{
			int num = 0;
			if (mediaControl != null)
			{
				mediaControl.StopWhenReady();
			}
			SetPlayState(PlayState.Stopped);
			lock (this)
			{
				if (videoWindow != null)
				{
					try
					{
						num = videoWindow.put_Visible(OABool.False);
						DsError.ThrowExceptionForHR(num);
					}
					catch
					{
					}
					try
					{
						num = videoWindow.put_Owner(IntPtr.Zero);
						DsError.ThrowExceptionForHR(num);
					}
					catch
					{
					}
					try
					{
						if (mediaEventEx != null)
						{
							num = mediaEventEx.SetNotifyWindow(IntPtr.Zero, 0, IntPtr.Zero);
							DsError.ThrowExceptionForHR(num);
						}
					}
					catch
					{
					}
				}
				if (mediaSeeking != null)
				{
					mediaSeeking = null;
				}
				if (mediaPosition != null)
				{
					mediaPosition = null;
				}
				if (basicAudio != null)
				{
					basicAudio = null;
				}
				if (basicVideo2 != null)
				{
					basicVideo2 = null;
				}
				if (frameStep != null)
				{
					frameStep = null;
				}
				if (mediaControl != null)
				{
					mediaControl = null;
				}
				if (mediaEventEx != null)
				{
					mediaEventEx = null;
				}
				if (videoWindow != null)
				{
					videoWindow = null;
				}
				if (graphBuilder != null)
				{
					try
					{
						Marshal.ReleaseComObject(graphBuilder);
						graphBuilder = null;
					}
					catch
					{
					}
				}
				if (captureGraphBuilder != null)
				{
					try
					{
						Marshal.ReleaseComObject(captureGraphBuilder);
						captureGraphBuilder = null;
					}
					catch
					{
					}
				}
				//GC.Collect();
			}
		}

		public void PausePlayClip()
		{
			if (mediaControl == null)
			{
				return;
			}
			if (currentState == PlayState.Paused || currentState == PlayState.Stopped)
			{
				if (mediaControl.Run() >= 0)
				{
					SetPlayState(PlayState.Running);
				}
			}
			else if (mediaControl.Pause() >= 0)
			{
				SetPlayState(PlayState.Paused);
			}
		}

		public void PlayClip()
		{
			if (mediaControl != null && currentState == PlayState.Paused && mediaControl.Run() >= 0)
			{
				SetPlayState(PlayState.Running);
			}
		}

		public void PauseClip()
		{
			if (mediaControl != null && currentState == PlayState.Running && mediaControl.Pause() >= 0)
			{
				SetPlayState(PlayState.Paused);
			}
		}

		public void SetVolume(int InVolume)
		{
			if (InVolume < 0 || InVolume > 100)
			{
				InVolume = 20;
			}
			VolumeStored = PercentToLog(InVolume);
			ApplyVolumeSettings();
		}

		private int PercentToLog(int InPct)
		{
			if (InPct < 0 || InPct > 100)
			{
				InPct = 50;
			}
			double num = (double)InPct + 0.01;
			return (int)(Math.Log10(num / 100.0) * 2500.0);
		}

		private void ApplyVolumeSettings()
		{
			currentVolume = VolumeStored;
			int plVolume = 0;
			if (basicAudio != null)
			{
				basicAudio.get_Volume(out plVolume);
			}
			if (basicAudio != null)
			{
				basicAudio.put_Volume(currentVolume);
			}
		}

		public void SetWideScreen(bool InMode, bool ResizeWindow)
		{
			isWidescreen = InMode;
			if (ResizeWindow)
			{
				InitVideoWindow();
			}
		}

		public void SetBalance(int InBalance)
		{
			if (InBalance < -100 || InBalance > 100)
			{
				InBalance = 0;
			}
			if (basicAudio != null)
			{
				basicAudio.put_Balance(InBalance * 70);
			}
		}

		public int SetMute(bool SetMuteOn)
		{
			int num = 0;
			if (graphBuilder == null || basicAudio == null)
			{
				return 0;
			}
			num = basicAudio.get_Volume(out currentVolume);
			if (num == -1)
			{
				return 0;
			}
			if (num < 0)
			{
				return num;
			}
			if (SetMuteOn)
			{
				currentVolume = -10000;
			}
			else
			{
				currentVolume = VolumeStored;
			}
			return basicAudio.put_Volume(currentVolume);
		}

		public string GetStatusText()
		{
			if (currentState == PlayState.Running)
			{
				if (isVideo)
				{
					return "Video";
				}
				return "Audio Only";
			}
			return "No Media Playing";
		}

		public void ResumeFromStart()
		{
			StopClip();
			ResumeFromPreviousPosition = false;
			OpenClip();
		}

		public int GetCurrentPosition()
		{
			double pllTime = 0.0;
			if (mediaPosition != null)
			{
				mediaPosition.get_CurrentPosition(out pllTime);
			}
			return (int)pllTime;
		}

		public string GetCurrentPositionString()
		{
			int currentPosition = GetCurrentPosition();
			return new DateTime(new TimeSpan(0, 0, currentPosition).Ticks).ToString("mm:ss");
		}

		public int SetCurrentPosition(double llTime)
		{
			try
			{
				if (mediaPosition != null)
				{
					mediaPosition.put_CurrentPosition(llTime);
				}
			}
			catch
			{
			}
			return (int)llTime;
		}

		public int GetClipDuration()
		{
			double pLength = 0.0;
			if (mediaPosition != null)
			{
				mediaPosition.get_Duration(out pLength);
				if (DataUtil.Left(curFilename, "<<Capture>>".Length) == "<<Capture>>")
				{
					pLength = 86400.0;
				}
			}
			return (int)pLength;
		}

		public string GetClipDurationString()
		{
			int clipDuration = GetClipDuration();
			return new DateTime(new TimeSpan(0, 0, clipDuration).Ticks).ToString("mm:ss");
		}

		private void HandleGraphEvent_MediaFile()
		{
			int num = 0;
			if (mediaEventEx == null)
			{
				return;
			}
			EventCode lEventCode;
			IntPtr lParam;
			IntPtr lParam2;
			while (mediaEventEx.GetEvent(out lEventCode, out lParam, out lParam2, 0) == 0)
			{
				num = mediaEventEx.FreeEventParams(lEventCode, lParam, lParam2);
				if (lEventCode == EventCode.Complete)
				{
					DsLong pCurrent = new DsLong(0L);
					num = mediaSeeking.SetPositions(pCurrent, AMSeekingSeekingFlags.AbsolutePositioning, null, AMSeekingSeekingFlags.NoPositioning);
					num = mediaControl.Stop();
					if (LoopClip)
					{
						num = mediaControl.Run();
					}
				}
			}
		}

		protected override void WndProc(ref Message m)
		{
			int msg = m.Msg;
			if (msg == 32769)
			{
				if (DataUtil.Left(newFilename, "<<Capture>>".Length) == "<<Capture>>")
				{
					HandleGraphEvent_CaptureDevice();
				}
				else
				{
					HandleGraphEvent_MediaFile();
				}
			}
			try
			{
				if (videoWindow != null)
				{
					videoWindow.NotifyOwnerMessage(m.HWnd, m.Msg, m.WParam, m.LParam);
				}
			}
			catch
			{
			}
			base.WndProc(ref m);
		}

		private int InitPlayerWindow()
		{
			base.ClientSize = new Size(0, 0);
			return 0;
		}

		public void TidyUp()
		{
			if (DataUtil.Left(newFilename, "<<Capture>>".Length) != "<<Capture>>")
			{
				StopClip();
			}
			CloseInterfaces();
		}

		public void CaptureVideo()
		{
			int num = 0;
			IBaseFilter baseFilter = null;
			try
			{
				OpenInterfaces();
				num = mediaEventEx.SetNotifyWindow(base.Handle, 32769, IntPtr.Zero);
				DsError.ThrowExceptionForHR(num);
				num = captureGraphBuilder.SetFiltergraph(graphBuilder);
				DsError.ThrowExceptionForHR(num);
				baseFilter = FindCaptureDevice(currentInputDevice);
				num = graphBuilder.AddFilter(baseFilter, "Video Capture");
				DsError.ThrowExceptionForHR(num);
				bool configResult = SetConfigParms(captureGraphBuilder, baseFilter);
				num = GetVideoStream(baseFilter, configResult);
				DsError.ThrowExceptionForHR(num);
				DebugMessage("GetVideoStream hr=" + num);
				Marshal.ReleaseComObject(baseFilter);
				//GC.Collect();
				num = videoWindow.put_Owner(base.Handle);
				DsError.ThrowExceptionForHR(num);
				num = videoWindow.put_WindowStyle(WindowStyle.Child | WindowStyle.ClipSiblings | WindowStyle.ClipChildren);
				DsError.ThrowExceptionForHR(num);
				num = videoWindow.put_Visible(OABool.True);
				DsError.ThrowExceptionForHR(num);
				num = InitVideoWindow();
				DsError.ThrowExceptionForHR(num);
				GetFrameStepInterface();
				num = mediaControl.Run();
				DsError.ThrowExceptionForHR(num);
				isVideo = true;
				SetPlayState(PlayState.Running);
			}
			catch
			{
			}
		}

        public IBaseFilter FindCaptureDevice(int InDeviceNumber)
		{
			DsDevice captureDevice = GetCaptureDevice(InDeviceNumber);
			Guid riidResult = typeof(IBaseFilter).GUID;

            IMoniker[] moniker = new IMoniker[1];

            //daniel change 19/11/10
            //captureDevice.Mon.BindToObject(null, null, ref riidResult, out object ppvResult);
            moniker[0].BindToObject(null, null, ref riidResult, out object ppvResult);

            return (IBaseFilter)ppvResult;
		}

		public DsDevice GetCaptureDevice(int InDeviceNumber)
		{
			if (InDeviceNumber < 1)
			{
				InDeviceNumber = 1;
			}
			ArrayList arrayList = new ArrayList();
			DsDevice[] devicesOfCat = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);
			int num = 0;
			DsDevice[] array = devicesOfCat;
			foreach (DsDevice dsDevice in array)
			{
				num++;
			}
			if (num > 5)
			{
				num = 5;
			}
			if (num == 0)
			{
				return null;
			}
			if (InDeviceNumber > num)
			{
				InDeviceNumber = num;
			}
			InDeviceNumber--;
			return devicesOfCat[InDeviceNumber];
		}

		public void ListCaptureDevices(ref ToolStripComboBox InComboBox)
		{
			ArrayList arrayList = new ArrayList();
			DsDevice[] devicesOfCat = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);
			int num = 0;
			InComboBox.Items.Clear();
			DsDevice[] array = devicesOfCat;
			foreach (DsDevice dsDevice in array)
			{
				num++;
				if (num <= 5)
				{
					InComboBox.Items.Add(num + ". " + dsDevice.Name);
				}
			}
			if (num < 5)
			{
				for (int j = num + 1; j <= 5; j++)
				{
					InComboBox.Items.Add(j + ". ");
				}
			}
		}

		private int GetVideoStream(IBaseFilter sourceFilter, bool ConfigResult)
		{
			int hr = 0;
			int num = FindCorrectVideoStream(sourceFilter, ConfigResult, ref hr);
			return hr;
		}

		private int FindCorrectVideoStream(IBaseFilter sourceFilter, bool ConfigResult, ref int hr)
		{
			hr = -1;
			hr = captureGraphBuilder.RenderStream(PinCategory.Preview, MediaType.Interleaved, sourceFilter, null, null);
			if (hr >= 0)
			{
				return 0;
			}
			hr = captureGraphBuilder.RenderStream(PinCategory.Capture, MediaType.Interleaved, sourceFilter, null, null);
			if (hr >= 0)
			{
				return 0;
			}
			hr = captureGraphBuilder.RenderStream(PinCategory.Preview, MediaType.Video, sourceFilter, null, null);
			if (hr >= 0)
			{
				return 1;
			}
			hr = captureGraphBuilder.RenderStream(PinCategory.Capture, MediaType.Video, sourceFilter, null, null);
			return 1;
		}

		public void HandleGraphEvent_CaptureDevice()
		{
			int num = 0;
			if (mediaEventEx != null)
			{
				EventCode lEventCode;
				IntPtr lParam;
                IntPtr lParam2;
				while (mediaEventEx.GetEvent(out lEventCode, out lParam, out lParam2, 0) == 0)
				{
					num = mediaEventEx.FreeEventParams(lEventCode, lParam, lParam2);
					DsError.ThrowExceptionForHR(num);
				}
			}
		}

		private bool IsMatchedConfigcaps(VideoStreamConfigCaps Caps, AMMediaType MediaType, int SizeX, int SizeY)
		{
			bool flag = true;
			if (flag)
			{
				flag = (SizeX >= Caps.MinOutputSize.Width && SizeX <= Caps.MaxOutputSize.Width && SizeY >= Caps.MinOutputSize.Height && SizeY <= Caps.MaxOutputSize.Height);
			}
			if (flag)
			{
				RCount++;
			}
			Debug.WriteLine("result = " + flag);
			return flag;
		}

		private int SetPreviewFormat(IBaseFilter sourceFilter)
		{
			DsGuid pCategory = PinCategory.Capture;
			DsGuid pType = MediaType.Interleaved;
			DsGuid g = typeof(IAMStreamConfig).GUID;
			bool flag = captureGraphBuilder.FindInterface(pCategory, pType, sourceFilter, g, out object ppint) == 0;
			pType = MediaType.Video;
			flag = (captureGraphBuilder.FindInterface(pCategory, pType, sourceFilter, g, out ppint) == 0);
			IAMStreamConfig iAMStreamConfig = ppint as IAMStreamConfig;
			iAMStreamConfig.GetNumberOfCapabilities(out int piCount, out int _);
			int[,] array = new int[1000, 2];
			for (int i = 0; i < piCount; i++)
			{
				IntPtr intPtr;
				try
				{
					intPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(VideoStreamConfigCaps)));
				}
				catch (Exception)
				{
					return -1;
				}
				int streamCaps = iAMStreamConfig.GetStreamCaps(i, out AMMediaType ppmt, intPtr);
				Marshal.ThrowExceptionForHR(streamCaps);
				VideoStreamConfigCaps videoStreamConfigCaps = (VideoStreamConfigCaps)Marshal.PtrToStructure(intPtr, typeof(VideoStreamConfigCaps));
				array[i, 0] = videoStreamConfigCaps.MaxOutputSize.Width;
				array[i, 1] = videoStreamConfigCaps.MaxOutputSize.Height;
				if (i == 0)
				{
					streamCaps = iAMStreamConfig.SetFormat(ppmt);
				}
				try
				{
				}
				catch
				{
				}
			}
			return 0;
		}

		private void SetPreviewFormatOriginal(int w, int h, IBaseFilter sourceFilter)
		{
			DsGuid pCategory = PinCategory.Capture;
			DsGuid pType = MediaType.Interleaved;
			DsGuid g = typeof(IAMStreamConfig).GUID;
			int num = 768;
			int num2 = 576;
			Debug.WriteLine("Interleaved: " + (captureGraphBuilder.FindInterface(pCategory, pType, sourceFilter, g, out object ppint) == 0));
			pType = MediaType.Video;
			Debug.WriteLine("Video: " + (captureGraphBuilder.FindInterface(pCategory, pType, sourceFilter, g, out ppint) == 0));
			IAMStreamConfig iAMStreamConfig = ppint as IAMStreamConfig;
			Debug.WriteLine("iStrConf == null: " + (iAMStreamConfig == null));
			iAMStreamConfig.GetNumberOfCapabilities(out int piCount, out int _);
			Debug.WriteLine("Capcount == " + piCount);
			for (int i = 0; i < piCount; i++)
			{
				IntPtr intPtr;
				try
				{
					intPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(VideoStreamConfigCaps)));
				}
				catch (Exception ex)
				{
					MessageBox.Show(ex.Message);
					return;
				}
				int streamCaps = iAMStreamConfig.GetStreamCaps(i, out AMMediaType ppmt, intPtr);
				Marshal.ThrowExceptionForHR(streamCaps);
				VideoStreamConfigCaps caps = (VideoStreamConfigCaps)Marshal.PtrToStructure(intPtr, typeof(VideoStreamConfigCaps));
				if (IsMatchedConfigcaps(caps, ppmt, num, num2))
				{
					if (object.Equals(ppmt.formatType, FormatType.VideoInfo))
					{
						VideoInfoHeader videoInfoHeader = (VideoInfoHeader)Marshal.PtrToStructure(ppmt.formatPtr, typeof(VideoInfoHeader));
						Debug.WriteLine("VideoInfoHeader == null : " + (videoInfoHeader == null));
						videoInfoHeader.TargetRect = new DsRect(0, 0, num, num2);
						videoInfoHeader.BmiHeader.Width = num;
						videoInfoHeader.BmiHeader.Height = num2;
						Debug.WriteLine("FormatType.VideoInfo");
					}
					else if (object.Equals(ppmt.formatType, FormatType.VideoInfo2))
					{
						VideoInfoHeader2 videoInfoHeader2 = (VideoInfoHeader2)Marshal.PtrToStructure(ppmt.formatPtr, typeof(VideoInfoHeader2));
						Debug.WriteLine((videoInfoHeader2 == null).ToString());
						videoInfoHeader2.TargetRect = new DsRect(0, 0, num, num2);
						videoInfoHeader2.BmiHeader.Width = num;
						videoInfoHeader2.BmiHeader.Height = num2;
						Debug.WriteLine("FormatType.VideoInfo2");
					}
					streamCaps = iAMStreamConfig.SetFormat(ppmt);
					Debug.WriteLine("SetFormat: " + streamCaps);
					Marshal.FreeCoTaskMem(intPtr);
					if (streamCaps != 0)
					{
					}
				}
			}
			Debug.WriteLine("RCount == " + RCount);
		}

		private bool SetConfigParms(ICaptureGraphBuilder2 capGraph, IBaseFilter capFilter)
		{
			int num = capGraph.FindInterface(PinCategory.Preview, MediaType.Video, capFilter, typeof(IAMStreamConfig).GUID, out object ppint);
			if (num < 0)
			{
				DebugMessage("SetConfigParms Preview Pin failed");
				return false;
			}
			IAMStreamConfig iAMStreamConfig = ppint as IAMStreamConfig;
			try
			{
				if (iAMStreamConfig == null)
				{
					DebugMessage("videoStreamConfig failed");
					return false;
				}
				IntPtr intPtr;
				try
				{
					intPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(VideoStreamConfigCaps)));
				}
				catch (Exception)
				{
					DebugMessage("pscc failed");
					return false;
				}
				int[,] array = new int[1000, 2];
				int num2 = 0;
				int num3 = 0;
				int num4 = 0;
				int num5 = 0;
				num = iAMStreamConfig.GetNumberOfCapabilities(out int piCount, out int _);
				VideoInfoHeader videoInfoHeader = new VideoInfoHeader();
				AMMediaType pmt;
				for (int i = 0; i < piCount; i++)
				{
					num = iAMStreamConfig.GetStreamCaps(i, out pmt, intPtr);
					VideoStreamConfigCaps videoStreamConfigCaps = (VideoStreamConfigCaps)Marshal.PtrToStructure(intPtr, typeof(VideoStreamConfigCaps));
					array[i, 0] = videoStreamConfigCaps.MaxOutputSize.Width;
					array[i, 1] = videoStreamConfigCaps.MaxOutputSize.Height;
					if (array[i, 1] == array[i, 0] * 3 / 4)
					{
						if (array[i, 1] > num2)
						{
							num2 = array[i, 1];
							num4 = i;
						}
					}
					else if (array[i, 1] > num3)
					{
						num3 = array[i, 1];
						num5 = i;
					}
				}
				num = iAMStreamConfig.GetFormat(out pmt);
				DsError.ThrowExceptionForHR(num);
				DebugMessage("videoStreamConfig.GetFormat OK");
				int num6 = (num2 < num3) ? num5 : num4;
				Marshal.PtrToStructure(pmt.formatPtr, (object)videoInfoHeader);
				videoInfoHeader.BmiHeader.Width = array[num6, 0];
				videoInfoHeader.BmiHeader.Height = array[num6, 1];
				DebugMessage("Index selected: " + num6 + " - " + array[num6, 0] + "x" + array[num6, 1]);
				Marshal.StructureToPtr((object)videoInfoHeader, pmt.formatPtr, fDeleteOld: false);
				num = iAMStreamConfig.SetFormat(pmt);
				DsError.ThrowExceptionForHR(num);
				DebugMessage("videoStreamConfig.SetFormat OK");
				DsUtils.FreeAMMediaType(pmt);
				pmt = null;
			}
			finally
			{
				Marshal.ReleaseComObject(iAMStreamConfig);
				//GC.Collect();
			}
			return true;
		}

		public void DebugMessage(string Message)
		{
			if (gf.DualMonitorMode && gf.ShowDebugVideoMessages)
			{
				MessageBox.Show(Message);
			}
		}
	}
}
