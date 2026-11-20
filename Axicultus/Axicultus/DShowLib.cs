#define DEBUG
using System;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Windows.Forms;
using DirectShowLib;
using Axicultus.Module;
using Axicultus.Util;

namespace Axicultus;

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

		public int currentVolume = 0;

		public bool isWidescreen = false;

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


		public DShowLib()
		{
			BackColor = Gf.TransparentColour;
			Bitmap bitmap = new(55, 25);
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
			int hr = 0;
			if (!(filename == string.Empty))
			{
				OpenInterfaces();
				hr = graphBuilder.RenderFile(filename, null);
				DsError.ThrowExceptionForHR(hr);
				int visibilityResult = CheckVisibility();
				hr = mediaEventEx.SetNotifyWindow(base.Handle, 32769, IntPtr.Zero);
				DsError.ThrowExceptionForHR(hr);
				if (isVideo)
				{
					try
					{
						hr = videoWindow.put_Owner(base.Handle);
						DsError.ThrowExceptionForHR(hr);
						hr = videoWindow.put_WindowStyle(WindowStyle.Child | WindowStyle.ClipSiblings | WindowStyle.ClipChildren);
						DsError.ThrowExceptionForHR(hr);
						// Position the video window before showing it, so it never
						// appears at its default size and then jumps into place.
						int sizeResult = InitVideoWindow();
						hr = videoWindow.put_Visible(OABool.True);
						DsError.ThrowExceptionForHR(hr);
						DsError.ThrowExceptionForHR(sizeResult);
						GetFrameStepInterface();
					}
					catch
					{
					}
				}
				else
				{
					hr = InitPlayerWindow();
					DsError.ThrowExceptionForHR(hr);
				}
				hr = mediaControl.Run();
				DsError.ThrowExceptionForHR(hr);
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
			int stopResult = 0;
			PauseClip();
			if (DataUtil.Left(newFilename, "<<Capture>>".Length) == "<<Capture>>")
			{
				PreviousMediaStoppedPosition = GetCurrentPosition();
			}
			if (mediaControl != null)
			{
				stopResult = mediaControl.Stop();
			}
			SetPlayState(PlayState.Stopped);
			isVideo = false;
			CloseInterfaces();
			SetPlayState(PlayState.Init);
			Media_Width = 0;
			Media_Height = 0;
			InitPlayerWindow();
		}

		public void StopClip()
		{
			int hr = 0;
			DsLong pCurrent = new(0L);
			if (mediaControl != null && mediaSeeking != null && (currentState == PlayState.Paused || currentState == PlayState.Running))
			{
				hr = mediaControl.Stop();
				SetPlayState(PlayState.Stopped);
				hr = mediaSeeking.SetPositions(pCurrent, AMSeekingSeekingFlags.AbsolutePositioning, null, AMSeekingSeekingFlags.NoPositioning);
				hr = mediaControl.Pause();
			}
		}

		public int InitVideoWindow()
		{
			int hr = 0;
			bool hasNativeWidescreenAspect = false;
			int pWidth;
			int pHeight;
			int aspectWidth;
			int aspectHeight;
			int plAspectX;
			int plAspectY;
			if (DataUtil.Left(newFilename, "<<Capture>>".Length) == "<<Capture>>")
			{
				if (videoWindow == null)
				{
					DebugMessage("videoWindow Quit: Null");
					return 0;
				}
				hr = basicVideo2.GetVideoSize(out pWidth, out pHeight);
				DebugMessage("InitVideoWindow Invideo " + pWidth + " " + pHeight);
				if (hr == -2147220971)
				{
					DebugMessage("InitVideoWindow Quit: GetVideoSize Error");
					return 0;
				}
				aspectWidth = Default_Width;
				aspectHeight = Default_Heigth;
				hr = basicVideo2.GetPreferredAspectRatio(out plAspectX, out plAspectY);
				DebugMessage("InitVideoWindow aspect " + pWidth + " " + pHeight);
				if (hr == -2147220971)
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
				hr = basicVideo2.GetVideoSize(out pWidth, out pHeight);
				DebugMessage("InitVideoWindow Invideo " + pWidth + " " + pHeight);
				if (hr == -2147220971)
				{
					DebugMessage("InitVideoWindow Quit: GetVideoSize Error");
					return 0;
				}
				aspectWidth = pWidth;
				aspectHeight = pHeight;
				hr = basicVideo2.GetPreferredAspectRatio(out plAspectX, out plAspectY);
				DebugMessage("InitVideoWindow aspect " + pWidth + " " + pHeight);
				if (hr == -2147220971)
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
				hasNativeWidescreenAspect = true;
				aspectWidth = aspectHeight * plAspectX / plAspectY;
			}
			if (isWidescreen && !hasNativeWidescreenAspect)
			{
				aspectWidth = aspectHeight * 16 / 9;
			}
			else if (plAspectX > 0 && plAspectY > 0)
			{
				aspectWidth = aspectHeight * plAspectX / plAspectY;
			}
			DebugMessage("Adjusted Media Size: " + aspectWidth + " " + aspectHeight);
			double videoAspectRatio = (double)aspectWidth / (double)aspectHeight;
			double outputAspectRatio = (double)Default_Width / (double)Default_Heigth;
			int left;
			int windowWidth;
			int windowHeight;
			int top;
			if (outputAspectRatio < videoAspectRatio)
			{
				left = Default_Left;
				windowWidth = Default_Width;
				windowHeight = (int)((double)windowWidth / videoAspectRatio);
				pHeight = (int)((double)pWidth / videoAspectRatio);
				top = ((Default_Align == VAlign.Top) ? Default_Top : ((Default_Align != VAlign.Centre) ? (Default_Top + (Default_Heigth - windowHeight)) : (Default_Top + (Default_Heigth - windowHeight) / 2)));
			}
			else
			{
				top = Default_Top;
				windowHeight = Default_Heigth;
				windowWidth = (int)((double)windowHeight * videoAspectRatio);
				pWidth = (int)((double)pHeight * videoAspectRatio);
				left = Default_Left + (Default_Width - windowWidth) / 2;
			}
			Media_Width = pWidth;
			Media_Height = pHeight;
			DebugMessage("Final Media Size: " + Media_Width + " " + Media_Height);
			// Setting Dock lays the control out synchronously. Application.DoEvents()
			// used to follow here; it let queued input (e.g. the media hot keys) re-enter
			// OpenClip/CloseClip while the graph was still being built.
			Dock = DockStyle.Fill;
			hr = videoWindow.SetWindowPosition(left, top, windowWidth, windowHeight);
			OABool hideCursor = OABool.True;
			videoWindow.HideCursor(hideCursor);
			return hr;
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
			int hr = 0;
			if (videoWindow == null || basicVideo2 == null)
			{
				isVideo = false;
				return -1;
			}
			isVideo = true;
			hr = videoWindow.get_Visible(out OABool _);
			if (hr < 0)
			{
				if (hr == -2147467262)
				{
					isVideo = false;
				}
				else
				{
					DsError.ThrowExceptionForHR(hr);
				}
			}
			return hr;
		}

		private bool GetFrameStepInterface()
		{
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
			int hr = 0;
			mediaControl?.StopWhenReady();
			SetPlayState(PlayState.Stopped);
			lock (this)
			{
				if (videoWindow != null)
				{
					try
					{
						hr = videoWindow.put_Visible(OABool.False);
						DsError.ThrowExceptionForHR(hr);
					}
					catch
					{
					}
					try
					{
						hr = videoWindow.put_Owner(IntPtr.Zero);
						DsError.ThrowExceptionForHR(hr);
					}
					catch
					{
					}
					try
					{
						if (mediaEventEx != null)
						{
							hr = mediaEventEx.SetNotifyWindow(IntPtr.Zero, 0, IntPtr.Zero);
							DsError.ThrowExceptionForHR(hr);
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
			if (currentState is PlayState.Paused or PlayState.Stopped)
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
			if (InVolume is < 0 or > 100)
			{
				InVolume = 20;
			}
			VolumeStored = PercentToLog(InVolume);
			ApplyVolumeSettings();
		}

		private static int PercentToLog(int InPct)
		{
			if (InPct is < 0 or > 100)
			{
				InPct = 50;
			}
			double adjustedPercent = (double)InPct + 0.01;
			return (int)(Math.Log10(adjustedPercent / 100.0) * 2500.0);
		}

		private void ApplyVolumeSettings()
		{
			currentVolume = VolumeStored;
			int plVolume = 0;
			basicAudio?.get_Volume(out plVolume);
			basicAudio?.put_Volume(currentVolume);
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
			if (InBalance is < -100 or > 100)
			{
				InBalance = 0;
			}
			basicAudio?.put_Balance(InBalance * 70);
		}

		public int SetMute(bool SetMuteOn)
		{
			int hr = 0;
			if (graphBuilder == null || basicAudio == null)
			{
				return 0;
			}
			hr = basicAudio.get_Volume(out currentVolume);
			if (hr == -1)
			{
				return 0;
			}
			if (hr < 0)
			{
				return hr;
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
			ResumeFromPreviousPosition = false;
			if (TryRestartLoadedClip())
			{
				return;
			}
			StopClip();
			OpenClip();
		}

		/// <summary>
		/// Rewinds the clip that is already loaded and runs it again. Going through
		/// OpenClip instead would close the graph, hide the video window and render
		/// the file again, which blanks the output for a moment.
		/// </summary>
		private bool TryRestartLoadedClip()
		{
			if (mediaControl == null || mediaSeeking == null)
			{
				return false;
			}
			if (newFilename == string.Empty || newFilename != curFilename || IsCaptureSource(newFilename))
			{
				return false;
			}
			try
			{
				mediaControl.Stop();
				DsLong start = new(0L);
				if (mediaSeeking.SetPositions(start, AMSeekingSeekingFlags.AbsolutePositioning, null, AMSeekingSeekingFlags.NoPositioning) < 0)
				{
					return false;
				}
				if (mediaControl.Run() < 0)
				{
					return false;
				}
			}
			catch
			{
				return false;
			}
			SetPlayState(PlayState.Running);
			return true;
		}

		private static bool IsCaptureSource(string fileName)
		{
			return DataUtil.Left(fileName, "<<Capture>>".Length) == "<<Capture>>";
		}

		public int GetCurrentPosition()
		{
			double pllTime = 0.0;
			mediaPosition?.get_CurrentPosition(out pllTime);
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
				mediaPosition?.put_CurrentPosition(llTime);
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
			int hr = 0;
			if (mediaEventEx == null)
			{
				return;
			}
			EventCode lEventCode;
			IntPtr lParam;
			IntPtr lParam2;
			while (mediaEventEx.GetEvent(out lEventCode, out lParam, out lParam2, 0) == 0)
			{
				hr = mediaEventEx.FreeEventParams(lEventCode, lParam, lParam2);
				if (lEventCode == EventCode.Complete)
				{
					DsLong pCurrent = new(0L);
					hr = mediaSeeking.SetPositions(pCurrent, AMSeekingSeekingFlags.AbsolutePositioning, null, AMSeekingSeekingFlags.NoPositioning);
					hr = mediaControl.Stop();
					if (LoopClip)
					{
						hr = mediaControl.Run();
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
				videoWindow?.NotifyOwnerMessage(m.HWnd, m.Msg, m.WParam, m.LParam);
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
			int hr = 0;
			IBaseFilter baseFilter = null;
			try
			{
				OpenInterfaces();
				hr = mediaEventEx.SetNotifyWindow(base.Handle, 32769, IntPtr.Zero);
				DsError.ThrowExceptionForHR(hr);
				hr = captureGraphBuilder.SetFiltergraph(graphBuilder);
				DsError.ThrowExceptionForHR(hr);
				baseFilter = FindCaptureDevice(currentInputDevice);
				hr = graphBuilder.AddFilter(baseFilter, "Video Capture");
				DsError.ThrowExceptionForHR(hr);
				bool configResult = SetConfigParms(captureGraphBuilder, baseFilter);
				hr = GetVideoStream(baseFilter, configResult);
				DsError.ThrowExceptionForHR(hr);
				DebugMessage("GetVideoStream hr=" + hr);
				Marshal.ReleaseComObject(baseFilter);
				//GC.Collect();
				hr = videoWindow.put_Owner(base.Handle);
				DsError.ThrowExceptionForHR(hr);
				hr = videoWindow.put_WindowStyle(WindowStyle.Child | WindowStyle.ClipSiblings | WindowStyle.ClipChildren);
				DsError.ThrowExceptionForHR(hr);
				int sizeResult = InitVideoWindow();
				hr = videoWindow.put_Visible(OABool.True);
				DsError.ThrowExceptionForHR(hr);
				DsError.ThrowExceptionForHR(sizeResult);
				GetFrameStepInterface();
				hr = mediaControl.Run();
				DsError.ThrowExceptionForHR(hr);
				isVideo = true;
				SetPlayState(PlayState.Running);
			}
			catch
			{
			}
		}

    public static IBaseFilter FindCaptureDevice(int InDeviceNumber)
		{
			using DsDevice captureDevice = GetCaptureDevice(InDeviceNumber);
			if (captureDevice == null || captureDevice.Mon == null)
			{
				throw new InvalidOperationException("No video capture device is available.");
			}
			Guid riidResult = typeof(IBaseFilter).GUID;
			// Bind the device's own moniker. The previous code bound an element of a
			// freshly allocated, empty IMoniker array, which was always null, so every
			// live camera start failed with a NullReferenceException.
			captureDevice.Mon.BindToObject(null, null, ref riidResult, out object ppvResult);
			return (IBaseFilter)ppvResult;
		}

		public static DsDevice GetCaptureDevice(int InDeviceNumber)
		{
			var devices = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);
			if (devices.Length == 0) return null;
			int selected = Math.Clamp(InDeviceNumber, 1, Math.Min(devices.Length, 5)) - 1;
			for (int i = 0; i < devices.Length; i++)
				if (i != selected) devices[i].Dispose();
			return devices[selected];
		}

		public static void ListCaptureDevices(ref ToolStripComboBox InComboBox)
		{
			var devices = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);
			try
			{
				InComboBox.Items.Clear();
				for (int i = 0; i < 5; i++)
					InComboBox.Items.Add($"{i + 1}. {(i < devices.Length ? devices[i].Name : "")}");
			}
			finally
			{
				foreach (var device in devices) device.Dispose();
			}
		}

		private int GetVideoStream(IBaseFilter sourceFilter, bool ConfigResult)
		{
			int hr = 0;
			FindCorrectVideoStream(sourceFilter, ConfigResult, ref hr);
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
			int hr = 0;
			if (mediaEventEx != null)
			{
				EventCode lEventCode;
				IntPtr lParam;
            IntPtr lParam2;
				while (mediaEventEx.GetEvent(out lEventCode, out lParam, out lParam2, 0) == 0)
				{
					hr = mediaEventEx.FreeEventParams(lEventCode, lParam, lParam2);
					DsError.ThrowExceptionForHR(hr);
				}
			}
		}

		internal static bool PreferCaptureFormat(int width, int height, int bestWidth, int bestHeight)
			=> width > 0 && height > 0 && (height > bestHeight ||
				(height == bestHeight && (long)width * 3 == (long)height * 4 && (long)bestWidth * 3 != (long)bestHeight * 4));

		private static bool SetConfigParms(ICaptureGraphBuilder2 capGraph, IBaseFilter capFilter)
		{
			int hr = capGraph.FindInterface(PinCategory.Preview, MediaType.Video, capFilter, typeof(IAMStreamConfig).GUID, out object instance);
			try
			{
				return hr >= 0 && instance is IAMStreamConfig config && ApplyCaptureFormat(config);
			}
			finally
			{
				if (instance != null && Marshal.IsComObject(instance)) Marshal.ReleaseComObject(instance);
			}
		}

		internal static bool ApplyCaptureFormat(IAMStreamConfig config)
		{
			IntPtr caps = IntPtr.Zero;
			AMMediaType best = null;
			try
			{
				if (config.GetNumberOfCapabilities(out int count, out int size) != 0 || count <= 0 || size <= 0)
					return false;
				caps = Marshal.AllocCoTaskMem(size);
				int bestWidth = 0, bestHeight = 0;
				for (int i = 0; i < count; i++)
				{
					AMMediaType candidate = null;
					try
					{
						if (config.GetStreamCaps(i, out candidate, caps) != 0 || candidate is null || candidate.formatPtr == IntPtr.Zero) continue;
						BitmapInfoHeader header;
						if (candidate.formatType == FormatType.VideoInfo && candidate.formatSize >= Marshal.SizeOf<VideoInfoHeader>())
							header = Marshal.PtrToStructure<VideoInfoHeader>(candidate.formatPtr).BmiHeader;
						else if (candidate.formatType == FormatType.VideoInfo2 && candidate.formatSize >= Marshal.SizeOf<VideoInfoHeader2>())
							header = Marshal.PtrToStructure<VideoInfoHeader2>(candidate.formatPtr).BmiHeader;
						else continue;
						if (header.Height == int.MinValue) continue;
						int height = Math.Abs(header.Height);
						if (!PreferCaptureFormat(header.Width, height, bestWidth, bestHeight)) continue;
						DsUtils.FreeAMMediaType(best);
						best = candidate;
						candidate = null;
						bestWidth = header.Width;
						bestHeight = height;
					}
					finally { DsUtils.FreeAMMediaType(candidate); }
				}
				return best != null && config.SetFormat(best) == 0;
			}
			finally
			{
				DsUtils.FreeAMMediaType(best);
				Marshal.FreeCoTaskMem(caps);
			}
		}

		public static void DebugMessage(string Message)
		{
			if (Gf.DualMonitorMode && Gf.ShowDebugVideoMessages)
			{
				MessageBox.Show(Message);
			}
		}
	}
