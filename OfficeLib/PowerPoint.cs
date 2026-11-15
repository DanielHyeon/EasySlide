using System;
using System.IO;
using System.Security.Cryptography;
using System.Linq;
using NetOffice.PowerPointApi;
using NetOffice.OfficeApi.Enums;
using Application = NetOffice.PowerPointApi.Application;
using System.Runtime.InteropServices;
using NetOffice.PowerPointApi.Enums;
using System.Reflection;
using Microsoft.Win32;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Runtime.Versioning;
using System.Text;

namespace OfficeLib
{
	public delegate void PreviewEvent(int index);
	public delegate void PreViewToOutputEvent();


    public class PowerPoint
	{
		// 상수 정의
		private const int NOTES_PLACEHOLDER_INDEX = 2;
		private const int EXPORT_WIDTH = 640;
		private const int EXPORT_HEIGHT = 480;
		private const int MAX_VERSE_NUMBER = 12;
		private const int VERSE_CHORUS = 0;
		private const int VERSE_BRIDGE = 100;
		private const int VERSE_ENDING = 101;
		private const int VERSE_TAG = 102;
		private const int VERSE_WORSHIP = 103;
		private const int VERSE_PRAYER = 111;
		private const int VERSE_QUIET = 112;

		public PreviewEvent preViewEvent;

		public PreViewToOutputEvent preViewToOutputEvent;

		public Application prePowerPointApp;

        public _Presentation presentation;

		public string[,] ScreenDumpFullPath = new string[2, 10000];

		public bool isEditable = false;

		public bool isLive = false;

		//������Ʈ���� ���?�Ǵ� Screen Device Name
		public string displayName = "None";

		// ✅ 캐싱: 파일 체크 결과 저장 (성능 최적화)
		private Dictionary<string, (bool Exists, DateTime LastCheck, int TotalSlides)> _fileCheckCache = new Dictionary<string, (bool, DateTime, int)>();
		private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(5);
		private readonly Dictionary<string, string> _fileHashCache = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
		private readonly object _fileHashCacheLock = new object();

        public void Init()
		{
			try
			{
				prePowerPointApp = new Application();
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error initializing PowerPoint Application: {ex.Message}");
				throw;
			}
		}

		public string Open(string InPowerpointFullPath, ref string[,] PowerpointList, ref int TotalPowerpointItems)
		{
			string text = "";
			Presentation presentation = null;

			try
			{
				_ = prePowerPointApp.Presentations.Count;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error accessing Presentations: {ex.Message}");
				return "Error: PowerPoint application not available";
			}

			try
			{
				if (TotalPowerpointItems < 0)
				{
					TotalPowerpointItems = 0;
				}
				text = GetPresentationName(InPowerpointFullPath, PowerpointList, TotalPowerpointItems);
				bool flag = false;
				if (text != "")
				{
					for (int i = 1; i <= prePowerPointApp.Presentations.Count; i++)
					{
						if (flag)
						{
							break;
						}
						if (prePowerPointApp.Presentations[i].Name == text)
						{
							flag = true;
						}
					}
				}
				try
				{
					if (!flag)
					{
						presentation = prePowerPointApp.Presentations.Open(InPowerpointFullPath, MsoTriState.msoTrue, MsoTriState.msoTrue);
						if (presentation != null)
						{
							PowerpointList[TotalPowerpointItems + 1, 0] = InPowerpointFullPath;
							PowerpointList[TotalPowerpointItems + 1, 1] = presentation.FullName;
							TotalPowerpointItems++;
							// presentation은 Presentations 컬렉션에서 관리되므로 여기서는 Dispose하지 않음
						}
					}
				}
				catch (Exception ex)
				{
					Console.WriteLine($"Error opening presentation '{InPowerpointFullPath}': {ex.Message}");
					// 에러 발생 시 열린 presentation 정리
					if (presentation != null)
					{
						try
						{
							presentation.Close();
							presentation.Dispose();
						}
						catch { }
					}
					return "Error Loading: " + text;
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error in Open method: {ex.Message}");
				return "Error Loading: " + text;
			}
			return "";
		}

		public void Run(int InIndex)
		{
			try
			{
				using SlideShowSettings settings = prePowerPointApp.Presentations[InIndex].SlideShowSettings;
				DisablePresenterView(settings);
				settings.Run();
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error running presentation at index {InIndex}: {ex.Message}");
			}
		}

		public string Run(string InPowerpointFullPath, ref string[,] PowerpointList, ref int TotalPowerpointItems)
		{
			string text = "";
			string text2 = "";
			SlideShowRaiseError = null;
			SlideShowRaiseSkipped = null;
			LastRunStarted = false;
			currentSlideShowWindow = IntPtr.Zero;			try
			{
				string version = prePowerPointApp.Version;

				SaveRegValue(version, @"PowerPoint\Options", "DisplayMonitor", displayName);

				text = GetPresentationName(InPowerpointFullPath, PowerpointList, TotalPowerpointItems);
				text2 = "";
				if (!(text != ""))
				{
					return "Cannot find opened Presentation: " + TotalPowerpointItems + ", " + InPowerpointFullPath;
				}

				for (int i = 1; i <= prePowerPointApp.Presentations.Count; i++)
				{
					string text3 = text2;
					text2 = text3 + i + "-" + prePowerPointApp.Presentations[i].Name + "(" + prePowerPointApp.Presentations[i].FullName + ") ";
					if (string.Equals(prePowerPointApp.Presentations[i].FullName, text, StringComparison.OrdinalIgnoreCase))
					{
						using SlideShowSettings settings = prePowerPointApp.Presentations[i].SlideShowSettings;
						DisablePresenterView(settings);
						using SlideShowWindow window = settings.Run();
						LastRunStarted = true;
						if (KeepSlideShowOnTop)
						{
							int index = i;
							currentSlideShowWindow = FindSlideShowWindow(window, () => prePowerPointApp.Presentations[index].Name);							RaiseCurrentSlideShowWindow();
						}

						return "Presentation Run OK";
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error running presentation: {ex.Message}");
				return "Error: " + TotalPowerpointItems + ", " + InPowerpointFullPath;
			}
			return "Can't find Presentations: " + text + ".  " + text2;
		}

		/// <summary>
		/// Turns PowerPoint's Presenter View off before a slide show starts; otherwise it follows the user's
		/// "Use Presenter View" option and opens on the control monitor. PowerPoint stores this as the user's
		/// default (Options\UseMonMgr). Older PowerPoint versions without this property: log and start the show anyway.
		/// </summary>
		private static void DisablePresenterView(SlideShowSettings settings)
		{
			try
			{
				settings.ShowPresenterView = MsoTriState.msoFalse;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"ShowPresenterView could not be turned off: {ex.Message}");
			}
		}

		[DllImport("user32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool IsWindow(IntPtr hWnd);

		[DllImport("user32.dll")]
		private static extern IntPtr GetAncestor(IntPtr hWnd, uint gaFlags);

		[DllImport("user32.dll", EntryPoint = "GetWindowLongPtrW")]
		private static extern IntPtr GetWindowLongPtr64(IntPtr hWnd, int nIndex);

		[DllImport("user32.dll", EntryPoint = "GetWindowLongW")]
		private static extern int GetWindowLong32(IntPtr hWnd, int nIndex);

		private static bool IsTopMost(IntPtr handle)
		{
			const int GWL_EXSTYLE = -20;
			const long WS_EX_TOPMOST = 0x0008;
			long exStyle = (IntPtr.Size == 8) ? GetWindowLongPtr64(handle, GWL_EXSTYLE).ToInt64() : GetWindowLong32(handle, GWL_EXSTYLE);
			return (exStyle & WS_EX_TOPMOST) != 0;
		}

		private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
		private static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);
		private static readonly IntPtr HWND_BOTTOM = new IntPtr(1);
		private const uint GA_ROOT = 2;
		private const uint SWP_NOSIZE = 0x0001;
		private const uint SWP_NOMOVE = 0x0002;
		private const uint SWP_NOACTIVATE = 0x0010;
		private const uint SWP_SHOWWINDOW = 0x0040;
		private const uint SWP_ASYNCWINDOWPOS = 0x4000;

		/// <summary>
		/// Set by the caller before <see cref="Run(string, ref string[,], ref int)"/> and
		/// <see cref="SetShowWindow"/>: make the slide show window topmost without activating it, while it is on
		/// the output monitor (<see cref="OutputMonitorBounds"/>). PowerPoint runs in its own process, so Windows
		/// does not let it come to the front by itself while another application's window covers the output
		/// monitor.
		/// </summary>
		public bool KeepSlideShowOnTop { get; set; }

		/// <summary>
		/// Why the slide show window was not made topmost by the last Run or SetShowWindow, or null.
		/// </summary>
		public string SlideShowRaiseError { get; private set; }

		// Slide show windows made topmost by any wrapper (all wrappers drive the same PowerPoint process, and
		// the show end clean-up runs on one of them only). Each entry keeps the owning process id, so a
		// handle that Windows has reused for another window is never touched.
		private static readonly List<(IntPtr Handle, uint ProcessId)> raisedSlideShowWindows = new List<(IntPtr, uint)>();
		private static readonly object raisedSlideShowWindowsLock = new object();

		/// <summary>
		/// True while a slide show window made topmost by a wrapper has not been lowered again.
		/// </summary>
		public static bool HasRaisedSlideShowWindows
		{
			get
			{
				lock (raisedSlideShowWindowsLock)
				{
					return raisedSlideShowWindows.Count > 0;
				}
			}
		}

		// The slide show window of the presentation this wrapper ran last (found once per Run, raised again
		// after SetShowWindow), or zero.
		private IntPtr currentSlideShowWindow;

		/// <summary>
		/// Finds the top-level window of the slide show just started: SlideShowWindow.HWND when PowerPoint returns
		/// it, otherwise the PowerPoint "screenClass" window with the presentation's name in its title. The window
		/// may not be visible yet right after Run, so the title search is tried up to SlideShowSearchAttempts
		/// times with SlideShowSearchDelayMs between tries: worst case 200 ms of waiting plus five window
		/// enumerations (a few ms each) on the calling thread. Returns zero (and sets SlideShowRaiseError) when
		/// it is not found.
		/// </summary>
		private IntPtr FindSlideShowWindow(SlideShowWindow window, Func<string> getPresentationName)
		{
			IntPtr handle = IntPtr.Zero;
			string handleError = null;
			if (!slideShowHwndUnreadable)
			{
				try
				{
					handle = (window == null) ? IntPtr.Zero : new IntPtr(window.HWND);
				}
				catch (Exception ex)
				{
					// Observed with PowerPoint 16 (Microsoft 365): SlideShowWindow.HWND cannot be read through
					// late binding (COMException DISP_E_MEMBERNOTFOUND, wrapped by NetOffice), on every run: not
					// tried again in this process. Any other failure (e.g. PowerPoint busy) is tried again next run.
					if (IsMemberNotFound(ex))
					{
						slideShowHwndUnreadable = true;
					}
					handleError = ex.GetBaseException().Message;
				}
			}
			if (handle != IntPtr.Zero && IsWindow(handle))
			{
				IntPtr root = GetAncestor(handle, GA_ROOT);
				return (root != IntPtr.Zero) ? root : handle;
			}
			string presentationName = null;
			try
			{
				presentationName = getPresentationName();
			}
			catch (Exception ex)
			{
				handleError = (handleError == null ? "" : handleError + "; ") + "presentation name: " + ex.GetBaseException().Message;
			}
			HashSet<uint> powerPointProcessIds = GetPowerPointProcessIds();
			for (int attempt = 0; attempt < SlideShowSearchAttempts; attempt++)
			{
				if (attempt > 0)
				{
					Thread.Sleep(SlideShowSearchDelayMs);
				}
				// The loose " name" match only on the last try: until then a user's own running show with a
				// similar title must not be taken while ours is not visible yet.
				handle = FindSlideShowWindowByTitle(presentationName, powerPointProcessIds, allowLooseMatch: attempt == SlideShowSearchAttempts - 1);
				if (handle != IntPtr.Zero)
				{
					return handle;
				}
			}
			SlideShowRaiseError = "the slide show window of '" + presentationName + "' was not found" + (handleError == null ? "" : " (" + handleError + ")");
			return IntPtr.Zero;
		}

		private const int DISP_E_MEMBERNOTFOUND = unchecked((int)0x80020003);

		private static bool IsMemberNotFound(Exception ex)
		{
			for (Exception e = ex; e != null; e = e.InnerException)
			{
				if (e.HResult == DISP_E_MEMBERNOTFOUND)
				{
					return true;
				}
			}
			return false;
		}

		private const int SlideShowSearchAttempts = 5;
		private const int SlideShowSearchDelayMs = 50;

		// Set once SlideShowWindow.HWND has thrown: this PowerPoint never returns it.
		private static bool slideShowHwndUnreadable;

		/// <summary>
		/// Makes the slide show window found by the last Run (of this wrapper) topmost, without activating it,
		/// but only while it is on the output monitor (see OutputMonitorBounds). Sets SlideShowRaiseError when
		/// it is not raised.
		/// </summary>
		private void RaiseCurrentSlideShowWindow()
		{
			IntPtr handle = currentSlideShowWindow;
			if (handle == IntPtr.Zero)
			{
				// FindSlideShowWindow has set the reason.
				return;
			}
			SlideShowRaiseSkipped = null;
			if (!IsWindow(handle))
			{
				SlideShowRaiseError = "the slide show window was closed";
				return;
			}
			SlideShowPlacement placement = GetPlacement(handle, out string where);
			if (placement != SlideShowPlacement.OutputMonitor)
			{
				// A topmost full-screen window on the operator's monitor would cover the main form while the
				// keyboard focus stays in it: leave it where PowerPoint put it. If an earlier raise of this item
				// made it topmost (custom output area: SetShowWindow moved it), undo that, but only when it is on
				// the operator's monitor; elsewhere (on neither monitor) it stays as it is and is reported.
				if (placement == SlideShowPlacement.OperatorMonitor && IsTopMost(handle))
				{
					LowerWindow(handle, wait: true, out bool skipped);
					if (!skipped)
					{
						lock (raisedSlideShowWindowsLock)
						{
							raisedSlideShowWindows.RemoveAll(w => w.Handle == handle);
						}
					}
				}
				if (OutputMonitorBounds == OperatorMonitorBounds)
				{
					// The output area is on the operator's own monitor: not raising is intended there (as before
					// this change), not a failure.
					SlideShowRaiseError = null;
					SlideShowRaiseSkipped = "the output area is on the operator's monitor (" + where + ")";
					return;
				}
				SlideShowRaiseError = "the slide show is not on the output monitor (" + where + ")";
				return;
			}
			const uint flags = SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE | SWP_SHOWWINDOW;
			bool ok = true;
			if (!IsTopMost(handle))
			{
				// Observed with PowerPoint 16 (Microsoft 365): a slide show window that came up behind other
				// windows ignores HWND_TOPMOST and HWND_TOP (SetWindowPos succeeds, nothing changes), but accepts
				// HWND_NOTOPMOST (top of the non-topmost windows), and after that HWND_TOPMOST.
				ok = SetWindowPos(handle, HWND_NOTOPMOST, 0, 0, 0, 0, flags);
			}
			ok = ok && SetWindowPos(handle, HWND_TOPMOST, 0, 0, 0, 0, flags);
			if (ok && !IsFirstTopMostSlideShow(handle))
			{
				// Already topmost but under another slide show (an earlier item's) that is topmost too, and
				// PowerPoint did not accept the move: take the long way once.
				ok = SetWindowPos(handle, HWND_NOTOPMOST, 0, 0, 0, 0, flags) && SetWindowPos(handle, HWND_TOPMOST, 0, 0, 0, 0, flags);
			}
			if (!ok)
			{
				int code = Marshal.GetLastWin32Error();
				SlideShowRaiseError = "SetWindowPos failed with Win32 error " + code + " (" + new System.ComponentModel.Win32Exception(code).Message + ")";
				return;
			}
			if (!IsTopMost(handle))
			{
				SlideShowRaiseError = "the slide show window did not become topmost";
				return;
			}
			SlideShowRaiseError = null;
			GetWindowThreadProcessId(handle, out uint processId);
			lock (raisedSlideShowWindowsLock)
			{
				raisedSlideShowWindows.RemoveAll(w => w.Handle == handle || !IsSameWindow(w.Handle, w.ProcessId));
				raisedSlideShowWindows.Add((handle, processId));
			}
		}

		/// <summary>
		/// Set by the caller before Run: the screen bounds (left, top, right, bottom) of the output monitor and
		/// of the monitor the operator's window is on. The slide show is raised only when the centre of its
		/// window is on the output monitor and not on the operator's monitor.
		/// </summary>
		public (int Left, int Top, int Right, int Bottom) OutputMonitorBounds { get; set; }

		/// <inheritdoc cref="OutputMonitorBounds"/>
		public (int Left, int Top, int Right, int Bottom) OperatorMonitorBounds { get; set; }

		private enum SlideShowPlacement
		{
			Unknown,
			OutputMonitor,
			OperatorMonitor,
			Elsewhere
		}

		/// <summary>
		/// Where the centre of the slide show window is: on the output monitor (and that is not the operator's
		/// monitor), on the operator's monitor, or on neither.
		/// </summary>
		private SlideShowPlacement GetPlacement(IntPtr handle, out string where)
		{
			if (!GetWindowRect(handle, out RECT rect))
			{
				where = "window position unknown";
				return SlideShowPlacement.Unknown;
			}
			int x = rect.Left + (rect.Right - rect.Left) / 2;
			int y = rect.Top + (rect.Bottom - rect.Top) / 2;
			where = "window centre " + x + "," + y;
			if (Contains(OperatorMonitorBounds, x, y))
			{
				return SlideShowPlacement.OperatorMonitor;
			}
			if (OutputMonitorBounds != OperatorMonitorBounds && Contains(OutputMonitorBounds, x, y))
			{
				return SlideShowPlacement.OutputMonitor;
			}
			return SlideShowPlacement.Elsewhere;
		}

		/// <summary>
		/// Set instead of SlideShowRaiseError when the last Run or SetShowWindow did not raise the slide show on
		/// purpose (the output area is on the operator's own monitor); null otherwise.
		/// </summary>
		public string SlideShowRaiseSkipped { get; private set; }

		/// <summary>
		/// True when the last Run(path, ...) started the slide show (SlideShowSettings.Run returned); false
		/// when it returned an error text instead.
		/// </summary>
		public bool LastRunStarted { get; private set; }

		/// <summary>
		/// The slide show window found by the last Run of this wrapper, or zero.
		/// </summary>
		public IntPtr CurrentSlideShowWindow => currentSlideShowWindow;

		/// <summary>
		/// True when the window exists and is topmost.
		/// </summary>
		public static bool IsTopMostWindow(IntPtr handle)
		{
			return handle != IntPtr.Zero && IsWindow(handle) && IsTopMost(handle);
		}

		private static bool Contains((int Left, int Top, int Right, int Bottom) bounds, int x, int y)
		{
			return x >= bounds.Left && x < bounds.Right && y >= bounds.Top && y < bounds.Bottom;
		}

		/// <summary>
		/// True when no other visible topmost slide show window is above this one.
		/// </summary>
		private static bool IsFirstTopMostSlideShow(IntPtr handle)
		{
			IntPtr first = IntPtr.Zero;
			EnumWindows((h, unused) =>
			{
				if (!IsWindowVisible(h) || !IsTopMost(h))
				{
					return true;
				}
				StringBuilder className = new StringBuilder(64);
				GetClassName(h, className, className.Capacity);
				if (className.ToString() != "screenClass")
				{
					return true;
				}
				first = h;
				return false;
			}, IntPtr.Zero);
			return first == handle;
		}

		[StructLayout(LayoutKind.Sequential)]
		private struct RECT
		{
			public int Left, Top, Right, Bottom;
		}

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

		private static HashSet<uint> GetPowerPointProcessIds()
		{
			HashSet<uint> powerPointProcessIds = new HashSet<uint>();
			int sessionId = Process.GetCurrentProcess().SessionId;
			foreach (Process process in Process.GetProcessesByName("POWERPNT"))
			{
				using (process)
				{
					try
					{
						if (process.SessionId == sessionId)
						{
							powerPointProcessIds.Add((uint)process.Id);
						}
					}
					catch
					{
					}
				}
			}
			return powerPointProcessIds;
		}

		/// <summary>
		/// The visible slide show window (class "screenClass") of a PowerPoint process whose title ends with
		/// a dash, white space and the presentation's name ("PowerPoint Slide Show  -  Presentation1"), so a
		/// user's "Meeting Presentation1" does not match "Presentation1". Only when no window ends that way,
		/// a single window whose title ends with " " + name is accepted, and only when allowLooseMatch is set.
		/// Zero when none is found.
		/// </summary>
		private static IntPtr FindSlideShowWindowByTitle(string presentationName, HashSet<uint> powerPointProcessIds, bool allowLooseMatch)
		{
			if (string.IsNullOrEmpty(presentationName))
			{
				return IntPtr.Zero;
			}
			if (powerPointProcessIds.Count == 0)
			{
				return IntPtr.Zero;
			}
			System.Text.RegularExpressions.Regex separated = new System.Text.RegularExpressions.Regex(
				"[-–—]\\s+" + System.Text.RegularExpressions.Regex.Escape(presentationName) + "$",
				System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.CultureInvariant);
			string suffix = " " + presentationName;
			IntPtr exact = IntPtr.Zero;
			List<IntPtr> loose = new List<IntPtr>();
			EnumWindows((handle, unused) =>
			{
				GetWindowThreadProcessId(handle, out uint processId);
				if (!powerPointProcessIds.Contains(processId) || !IsWindowVisible(handle))
				{
					return true;
				}
				StringBuilder className = new StringBuilder(64);
				GetClassName(handle, className, className.Capacity);
				if (className.ToString() != "screenClass")
				{
					return true;
				}
				StringBuilder title = new StringBuilder(512);
				GetWindowText(handle, title, title.Capacity);
				string text = title.ToString().TrimEnd();
				if (separated.IsMatch(text))
				{
					exact = handle;
					return false;
				}
				if (text.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
				{
					loose.Add(handle);
				}
				return true;
			}, IntPtr.Zero);
			if (exact != IntPtr.Zero)
			{
				return exact;
			}
			return (allowLooseMatch && loose.Count == 1) ? loose[0] : IntPtr.Zero;
		}

		/// <summary>
		/// Takes the topmost state off every visible or hidden PowerPoint slide show window ("screenClass") in this
		/// session that is topmost, and puts it behind the other windows. PowerPoint does not make its slide show
		/// windows topmost by itself, so these were raised by this app, e.g. by a run that crashed or was killed
		/// during a show. Nothing is closed (it may be the user's document). Window enumeration only: does not
		/// start PowerPoint and returns at once when it is not running. Returns how many windows were changed.
		/// </summary>
		public static int LowerLeftoverTopMostSlideShowWindows()
		{
			HashSet<uint> powerPointProcessIds = GetPowerPointProcessIds();
			if (powerPointProcessIds.Count == 0)
			{
				return 0;
			}
			List<IntPtr> topMost = new List<IntPtr>();
			EnumWindows((handle, unused) =>
			{
				GetWindowThreadProcessId(handle, out uint processId);
				if (!powerPointProcessIds.Contains(processId) || !IsTopMost(handle))
				{
					return true;
				}
				StringBuilder className = new StringBuilder(64);
				GetClassName(handle, className, className.Capacity);
				if (className.ToString() == "screenClass")
				{
					topMost.Add(handle);
				}
				return true;
			}, IntPtr.Zero);
			foreach (IntPtr handle in topMost)
			{
				// Posted: app start and show start must not hang on a hung PowerPoint. These are earlier runs'
				// windows; this show raises only windows of its own presentations, opened after this point, so a
				// lowering still queued in PowerPoint cannot land on a window this show raises.
				LowerWindow(handle, wait: false, out _);
			}
			return topMost.Count;
		}

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool IsHungAppWindow(IntPtr hWnd);

		/// <summary>
		/// HWND_NOTOPMOST alone leaves the window above every other normal window; then it is sent to the bottom,
		/// so the windows that were on the output monitor before are seen again.
		/// wait = true: done before returning, so a later raise of the same window (made synchronously) always
		/// comes after it. If PowerPoint does not respond (IsHungAppWindow), nothing is done (skipped = true):
		/// waiting would hang the caller, and a posted lowering could be applied after a later raise (a sent
		/// SetWindowPos is handled before posted ones). wait = false: posted (SWP_ASYNCWINDOWPOS), never blocks;
		/// only for windows no later raise of this show can target. Returns false when SetWindowPos failed.
		/// </summary>
		private static bool LowerWindow(IntPtr handle, bool wait, out bool skipped)
		{
			skipped = false;
			uint flags = SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE;
			if (!wait)
			{
				flags |= SWP_ASYNCWINDOWPOS;
			}
			else if (IsHungAppWindow(handle))
			{
				skipped = true;
				return true;
			}
			return SetWindowPos(handle, HWND_NOTOPMOST, 0, 0, 0, 0, flags) && SetWindowPos(handle, HWND_BOTTOM, 0, 0, 0, 0, flags);
		}

		private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool IsWindowVisible(IntPtr hWnd);

		[DllImport("user32.dll", CharSet = CharSet.Unicode)]
		private static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

		[DllImport("user32.dll", CharSet = CharSet.Unicode)]
		private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

		private static bool IsSameWindow(IntPtr handle, uint processId)
		{
			if (!IsWindow(handle))
			{
				return false;
			}
			GetWindowThreadProcessId(handle, out uint currentProcessId);
			return currentProcessId == processId;
		}

		/// <summary>
		/// Takes the topmost state off every slide show window that a wrapper made topmost and that still exists,
		/// and puts it behind the other windows.
		/// Call before the presentations are closed at the end of a show (in case closing fails) and before the
		/// slide show windows are moved out of the way. wait: see LowerWindow (true while the show goes on, so a
		/// later raise cannot be overtaken by this lowering; false at show end, which must not hang on PowerPoint).
		/// Returns null, or a description of the windows that could not be changed.
		/// </summary>
		public static string LowerRaisedSlideShowWindows(bool wait)
		{
			List<(IntPtr Handle, uint ProcessId)> windows;
			lock (raisedSlideShowWindowsLock)
			{
				windows = new List<(IntPtr, uint)>(raisedSlideShowWindows);
				raisedSlideShowWindows.Clear();
			}
			string error = null;
			foreach ((IntPtr handle, uint processId) in windows)
			{
				if (!IsSameWindow(handle, processId))
				{
					continue;
				}
				if (!LowerWindow(handle, wait, out bool skipped))
				{
					int code = Marshal.GetLastWin32Error();
					error = (error == null ? "" : error + "; ") + "window 0x" + handle.ToString("X") + ": Win32 error " + code + " (" + new System.ComponentModel.Win32Exception(code).Message + ")";
				}
				else if (skipped)
				{
					// PowerPoint is not responding: kept in the list, lowered by a later call (at the latest at show end).
					lock (raisedSlideShowWindowsLock)
					{
						raisedSlideShowWindows.Add((handle, processId));
					}
				}
			}
			return error;
		}

		public string GetPresentationName(string InPowerpointFullPath, string[,] PowerpointList, int TotalPowerpointItems)
		{
			if (string.IsNullOrEmpty(InPowerpointFullPath))
			{
				Console.WriteLine("GetPresentationName: InPowerpointFullPath is null or empty");
				return "";
			}

			if (PowerpointList == null)
			{
				Console.WriteLine("GetPresentationName: PowerpointList is null");
				return "";
			}

			try
			{
				for (int i = 1; i <= TotalPowerpointItems; i++)
				{
					try
					{
						if (PowerpointList[i, 0] == InPowerpointFullPath)
						{
							return PowerpointList[i, 1];
						}
					}
					catch (Exception ex)
					{
						Console.WriteLine($"Error accessing PowerpointList at index {i}: {ex.Message}");
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error in GetPresentationName: {ex.Message}");
			}
			return "";
		}

		public void First()
		{
			try
			{
				prePowerPointApp.ActivePresentation.SlideShowWindow.View.First();
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error going to first slide: {ex.Message}");
			}
		}

		public void Last()
		{
			try
			{
				prePowerPointApp.ActivePresentation.SlideShowWindow.View.Last();
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error going to last slide: {ex.Message}");
			}
		}

		public void GotoSlide(int StartingSlide)
		{
			if (StartingSlide <= 0)
			{
				Console.WriteLine($"GotoSlide: Invalid slide number {StartingSlide}");
				return;
			}

			try
			{
				prePowerPointApp.ActivePresentation.SlideShowWindow.View.GotoSlide(StartingSlide);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error going to slide {StartingSlide}: {ex.Message}");
			}
		}

		public int Count()
		{
			try
			{
				return prePowerPointApp.ActivePresentation.Slides.Count;
			}
			catch (Exception)
		{
			// ActivePresentation이 없거나 접근 불가 시 0 반환
			return 0;
		}
		}


		public static void SaveRegValue(string Version, string Section, string Name, string Value)
		{
			try
			{
				using (RegistryKey registryKey = Registry.CurrentUser.CreateSubKey($@"Software\Microsoft\Office\{Version}\{Section}"))
				{
					registryKey.SetValue(Name, Value, RegistryValueKind.String);
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error saving registry value {Name}: {ex.Message}");
			}
		}

		public static object GetRegValue(string Version, string Section, string Name, string defaultValue)
		{
			try
			{
				using (RegistryKey registryKey = Registry.CurrentUser.CreateSubKey($@"Software\Microsoft\Office\{Version}\{Section}"))
				{
					return registryKey.GetValue(Name, defaultValue);
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error getting registry value {Name}: {ex.Message}");
				return defaultValue;
			}
		}

        public void SetShowWindow(float Left, float Top, float Width, float Height)
        {
            try
            {
                prePowerPointApp.ActivePresentation.SlideShowWindow.Left = Left;
                prePowerPointApp.ActivePresentation.SlideShowWindow.Top = Top;
                prePowerPointApp.ActivePresentation.SlideShowWindow.Width = Width;
                prePowerPointApp.ActivePresentation.SlideShowWindow.Height = Height;
            }
            catch (Exception)
            {
				// 예외 무시
            }
			// Moving the window can change its place in the z-order and its monitor (custom output area): raise
			// the window found by Run again. This is the item's last raise; SlideShowRaiseError is its result.
			if (KeepSlideShowOnTop)
			{
				RaiseCurrentSlideShowWindow();
			}
        }

        public void ResSetAllShowWindows()
		{
			try
			{
				if (prePowerPointApp.Presentations.Count > 0)
				{
					for (int i = 1; i <= prePowerPointApp.Presentations.Count; i++)
					{
						try
						{
							prePowerPointApp.Presentations[i].SlideShowWindow.Left = 0f;
							prePowerPointApp.Presentations[i].SlideShowWindow.Top = 0f;
							prePowerPointApp.Presentations[i].SlideShowWindow.Width = 1f;
							prePowerPointApp.Presentations[i].SlideShowWindow.Height = 1f;
						}
						catch (Exception)
            {
				// 예외 무시
            }
					}
				}
			}
			catch (Exception)
            {
				// 예외 무시
            }
		}

		/// <summary>
		/// daniel
		/// �� ���α׷����� �����?�Ŀ�����Ʈ�� ����
		/// �ٸ� �۾� ������ �� �Ŀ�����Ʈ ������ ���� ����
		/// </summary>
		/// <param name="PowerpointList"></param>
		/// <param name="TotalPowerpointItems"></param>
		/// <returns></returns>
		public string ClearUpPowerpointWindows(ref string[,] PowerpointList, ref int TotalPowerpointItems)
		{
			if (TotalPowerpointItems < 1)
			{
				return "";
			}

			int num = TotalPowerpointItems;
			int num2 = 0;
			int num3 = 0;
			bool flag = false;
			try
			{
				if(prePowerPointApp != null && prePowerPointApp.Presentations != null)
                {
					num2 = prePowerPointApp.Presentations.Count;
					if (TotalPowerpointItems > 0 && prePowerPointApp.Presentations.Count > 0)
					{
						for (int num4 = prePowerPointApp.Presentations.Count; num4 > 0; num4--)
						{
							try
							{
								// Close only the presentations this app opened. PowerPoint is single-instance,
								// so other presentations in it may be the user's own (Close does not ask to save).
								Presentation openPresentation = prePowerPointApp.Presentations[num4];
								if (IsListedPresentation(openPresentation.FullName, PowerpointList, num))
								{
									openPresentation.Close();
									num3++;
								}
							}
							catch
							{ }
						}
					}
				}
			}
			catch
			{
				flag = true;
			}
			TotalPowerpointItems = 0;

			//daniel PowerPoint App ����
			QuitPowerPointApp(prePowerPointApp);

			return "Opened PP=" + num2 + ", Total On List=" + num + ", Numbers Closed=" + num3 + (flag ? ". Error When Closing" : ". No Error");
		}

		private static bool IsListedPresentation(string FullName, string[,] PowerpointList, int TotalPowerpointItems)
		{
			if (string.IsNullOrEmpty(FullName) || PowerpointList == null)
			{
				return false;
			}
			int last = Math.Min(TotalPowerpointItems, PowerpointList.GetUpperBound(0));
			for (int i = 1; i <= last; i++)
			{
				if (string.Equals(PowerpointList[i, 1], FullName, StringComparison.OrdinalIgnoreCase))
				{
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Returns the number of presentations open in the given PowerPoint, or -1 when it cannot be read.
		/// </summary>
		private static int GetOpenPresentationCount(Application app)
		{
			try
			{
				return app.Presentations.Count;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error reading open presentations: {ex.Message}");
				return -1;
			}
		}

		/// <summary>
		/// Releases this wrapper's reference to PowerPoint without quitting it.
		/// Releasing every wrapper's reference at shutdown is what was observed to make the hidden
		/// PowerPoint exit when the app closes.
		/// </summary>
		public void ReleasePowerPointApp()
		{
			Application app = prePowerPointApp;
			prePowerPointApp = null;
			if (app == null)
			{
				return;
			}
			try
			{
				if (!app.IsDisposed)
				{
					app.Dispose();
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error releasing PowerPoint application: {ex.Message}");
			}
		}




		/// daniel 
		/// �Ŀ�����Ʈ Slide Show�� �ɶ� ������ â�� Minimized ���·� �ٲ۴�.
		/// </summary>
		public void NewApp()
		{
			try
			{
				// Create the new reference first so a failure keeps the old, working one.
				// Then release the old reference instead of orphaning it, so shutdown can release every reference.
				Application newApp = new Application();
				ReleasePowerPointApp();
				prePowerPointApp = newApp;
			}
			catch (Exception)
            {
				// 예외 무시
            }
		}

        private DateTime lastClickTime = DateTime.MinValue;

        public void SafePlayNext(int slideNumber = -1)
        {
            var activePresentation = prePowerPointApp.ActivePresentation;
            if (activePresentation == null || activePresentation.SlideShowWindow == null) return;

            // 1. 디바운스 (유지)
            if ((DateTime.Now - lastClickTime).TotalMilliseconds < 500) return;
            lastClickTime = DateTime.Now;

            try
            {
                var view = activePresentation.SlideShowWindow.View;

                // [핵심 수정 사항] 
                // 입력된 번호가 유효하고(>0), '현재 보고 있는 슬라이드'와 '다를 때만' 이동합니다.
                // 같을 때 이동하면 애니메이션이 리셋되기 때문입니다.
                if (slideNumber > 0 && view.Slide.SlideIndex != slideNumber)
                {
                    // 다른 슬라이드로 이동할 때만 실행
                    view.GotoSlide(slideNumber, MsoTriState.msoTrue);
                }

                activePresentation.SlideShowWindow.Activate();

                // 현재 상태 다시 조회 (GotoSlide를 했을 수도 있으므로 여기서 조회)
                int totalClicks = view.Slide.TimeLine.MainSequence.Count;
                int currentIndex = view.GetClickIndex();

                // 2. 애니메이션 실행 로직
                if (currentIndex < totalClicks)
                {
                    view.Next();
                    System.Diagnostics.Debug.WriteLine($"Playing animation {currentIndex + 1} of {totalClicks}");
                }
                else
                {
                    // 3. 애니메이션 끝난 후 처리
                    System.Diagnostics.Debug.WriteLine("Animation finished. Resetting loop.");

                    // 만약 '다음 슬라이드'로 자동으로 넘어가길 원하시면:
                    // view.Next(); 

                    // 현재 로직 유지 (반복 재생):
                    view.GotoClick(0);
                    view.Next();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error: " + ex.Message);
            }
        }
        /// <summary>
        /// ?�블?�릭 ??PowerPoint ?�라?�드??창을 ?�성?�하�??�니메이???�영?�을 ?�생?�는 메서??
        /// </summary>
        public void ActivateSlideShowAndTriggerClick(int slideNumber = -1)
		{
			try
			{
				if (prePowerPointApp == null)
				{
					return;
				}

				_Presentation activePresentation = prePowerPointApp.ActivePresentation;
				if (activePresentation != null && activePresentation.SlideShowWindow != null)
				{
					// ?�재 ?�라?�드 ?�인
					int currentSlide = activePresentation.SlideShowWindow.View.Slide.SlideIndex;

					// ?�라?�드 번호가 지?�되?�고, ?�재 ?�라?�드?� ?�른 경우?�만 ?�동
					if (slideNumber > 0)
					{
						activePresentation.SlideShowWindow.View.GotoSlide(slideNumber, MsoTriState.msoTrue);
						//Thread.Sleep(200);
					}

					activePresentation.SlideShowWindow.Activate();
					activePresentation.SlideShowWindow.View.GotoClick(0);
					activePresentation.SlideShowWindow.View.Next();
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error activating SlideShow: {ex.Message}");
			}
		}

		public int ImplementPowerpointSlideMovement(ref int InCurSlide, int InCurMaxSlide, OfficeLibKeys InKey)
		{
			try
			{
				return ImplementPowerpointSlideMovement(ref InCurSlide, InCurMaxSlide, InKey, 1);
			}
			catch
			{
				return InCurSlide;
			}
		} 

		public int ImplementPowerpointSlideMovement(ref int InCurSlide, int InCurMaxSlide, OfficeLibKeys InKey, int InSlideNo)
		{
			try
			{
				if (prePowerPointApp == null)
				{
					return 1;
				}

                _Presentation activePresentation = prePowerPointApp.ActivePresentation;
                
                // SlideShowWindow가 존재하는지 확인 (슬라이드쇼가 실행 중이어야 함)
                SlideShowWindow slideShowWindow = null;
                try
                {
                    slideShowWindow = activePresentation.SlideShowWindow;
                }
                catch (COMException)
                {
                    // 슬라이드쇼가 실행 중이 아니면 현재 슬라이드 번호를 그대로 반환
                    return InCurSlide;
                }
                
                if (slideShowWindow == null)
                {
                    return InCurSlide;
                }
            
                switch (InKey)
				{
					case OfficeLibKeys.Left:
                        slideShowWindow.View.First();
                        InCurSlide = 1;
                        break;
					case OfficeLibKeys.Up:
						slideShowWindow.View.Previous();
						InCurSlide = slideShowWindow.View.Slide.SlideIndex;
                        break;
					case OfficeLibKeys.Down:
						slideShowWindow.View.Next();
						if (slideShowWindow.View.State == PpSlideShowState.ppSlideShowDone)
						{
							slideShowWindow.View.Last();
                            InCurSlide = -1;
						}
						else
						{
							InCurSlide = slideShowWindow.View.Slide.SlideIndex;
                        }
                        break;
					case OfficeLibKeys.Right:
						slideShowWindow.View.Last();
						InCurSlide = InCurMaxSlide;
                        break;
					case OfficeLibKeys.None:
						slideShowWindow.View.GotoSlide(InSlideNo, MsoTriState.msoFalse);                        
                        InCurSlide = InSlideNo;
                        break;
					case OfficeLibKeys.Space:

						if (InCurSlide == -1)
							return InCurSlide;

                        if (slideShowWindow.View.State == PpSlideShowState.ppSlideShowDone)
						{
							//slideShowWindow.View.Last();
                            InCurSlide = -1;
						}
						else
						{

                            slideShowWindow.Activate();
                            slideShowWindow.View.Next();

                            if (slideShowWindow.View.State == PpSlideShowState.ppSlideShowDone)
							{
								InCurSlide = -1;
                            }
							else
							{
								if (InCurSlide != slideShowWindow.View.Slide.SlideIndex)
								{
                                    InCurSlide = slideShowWindow.View.Slide.SlideIndex;
                                }
                            }
                        }
                        break;
					case OfficeLibKeys.F5:
						slideShowWindow.View.GotoSlide(InCurSlide, MsoTriState.msoFalse);
                        break;
				}
            }
			catch
			{
				if(preViewToOutputEvent != null)
					preViewToOutputEvent();
            }
			return InCurSlide;
		}

        public void LoadVersesAndSlides(ref int[] SongVerses, ref int[,] Slide, string[] SequenceSymbol)
		{
			try
			{
				int num = -1;
				bool flag = false;
				SequenceSymbol.GetUpperBound(0);
				for (int i = 1; i <= prePowerPointApp.ActivePresentation.Slides.Count; i++)
				{
					try
					{
						if (prePowerPointApp.ActivePresentation.Slides[i].NotesPage.Shapes.Placeholders[NOTES_PLACEHOLDER_INDEX].PlaceholderFormat.Type == PpPlaceholderType.ppPlaceholderBody &&
							prePowerPointApp.ActivePresentation.Slides[i].NotesPage.Shapes.Placeholders[NOTES_PLACEHOLDER_INDEX].TextFrame.TextRange.Text.Length == 1)
						{
							num = GetVerseIndicator(prePowerPointApp.ActivePresentation.Slides[i].NotesPage.Shapes.Placeholders[NOTES_PLACEHOLDER_INDEX].TextFrame.TextRange.Text);
							if (num >= 0)
							{
								if (num == VERSE_CHORUS)
								{
									if (!flag)
									{
										Slide[i, 0] = num;
										SongVerses[num] = i;
										flag = true;
									}
								}
								else if (num <= MAX_VERSE_NUMBER)
								{
									SongVerses[num] = i;
									Slide[i, 0] = num;
								}
								else
								{
									Slide[i, 0] = num;
								}
							}
						}
					}
					catch (Exception ex)
					{
						Console.WriteLine($"Error loading verse for slide {i}: {ex.Message}");
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error in LoadVersesAndSlides: {ex.Message}");
			}
		}

		public int GetVerseIndicator(string InString)
		{
			if (string.IsNullOrEmpty(InString))
			{
				return -1;
			}
			InString = InString.ToLower().Trim();
			try
			{
				int num = Convert.ToInt32(InString);
				if (num >= 0 && num <= MAX_VERSE_NUMBER)
				{
					return num;
				}
				return -1;
			}
			catch
			{
				if (InString.Length == 1)
				{
					try
					{
						switch (InString[0])
						{
							case 'c':
								return VERSE_CHORUS;
							case 'b':
								return VERSE_BRIDGE;
							case 'e':
								return VERSE_ENDING;
							case 't':
								return VERSE_TAG;
							case 'w':
								return VERSE_WORSHIP;
							case 'p':
								return VERSE_PRAYER;
							case 'q':
								return VERSE_QUIET;
						}
					}
					catch (Exception ex)
					{
						Console.WriteLine($"Error parsing verse character '{InString}': {ex.Message}");
					}
				}
			}
			return -1;
		}

		/// <summary>
		/// 프레젠테이션에서 Verse 정보를 파싱하여 배열에 저장
		/// </summary>
		private void ParseVersesFromPresentation(_Presentation presentation, int totalSlides, ref int[] SongVerses, ref int[,] Slide, string[] SequenceSymbol)
		{
			bool flag = false;
			SequenceSymbol.GetUpperBound(0);
			for (int i = 1; i <= totalSlides; i++)
			{
				try
				{
					if (presentation.Slides[i].NotesPage.Shapes.Placeholders[NOTES_PLACEHOLDER_INDEX].PlaceholderFormat.Type == PpPlaceholderType.ppPlaceholderBody &&
						presentation.Slides[i].NotesPage.Shapes.Placeholders[NOTES_PLACEHOLDER_INDEX].TextFrame.TextRange.Text.Length == 1)
					{
						int verseIndicator = GetVerseIndicator(presentation.Slides[i].NotesPage.Shapes.Placeholders[NOTES_PLACEHOLDER_INDEX].TextFrame.TextRange.Text);
						if (verseIndicator >= 0)
						{
							if (verseIndicator == VERSE_CHORUS)
							{
								if (!flag)
								{
									Slide[i, 0] = verseIndicator;
									SongVerses[verseIndicator] = i;
									flag = true;
								}
							}
							else if (verseIndicator <= MAX_VERSE_NUMBER)
							{
								SongVerses[verseIndicator] = i;
								Slide[i, 0] = verseIndicator;
							}
							else
							{
								Slide[i, 0] = verseIndicator;
							}
						}
					}
				}
				catch (Exception ex)
				{
					Console.WriteLine($"Error processing verse for slide {i}: {ex.Message}");
				}
			}
		}

		public bool IsFileChanged(string FilePath)
		{
			if (string.IsNullOrEmpty(FilePath) || !File.Exists(FilePath))
			{
				return true;
			}

			string currentHash = PowerPoint.GetMD5(FilePath);

			lock (_fileHashCacheLock)
			{
				if (_fileHashCache.TryGetValue(FilePath, out var previousHash) &&
					string.Equals(previousHash, currentHash, StringComparison.OrdinalIgnoreCase))
				{
					return false;
				}

				_fileHashCache[FilePath] = currentHash;
			}

			return true;
		}


		/// <summary>
		/// JPG 파일이 이미 빌드되었는지 확인
		/// </summary>
		/// <param name="FilePath">PowerPoint 파일 경로</param>
		/// <param name="FilePrefix">JPG 파일 접두사</param>
		/// <param name="TotalSlides">총 슬라이드 수 (출력)</param>
		/// <returns>true: 이미 빌드됨(재빌드 불필요), false: 재빌드 필요</returns>
		public bool IsBuildedFileCheck(string FilePath, string FilePrefix, ref int TotalSlides, _Presentation presentation = null)
	{
		if (string.IsNullOrEmpty(FilePath))
		{
			Console.WriteLine($"FilePath is null or empty (FilePrefix='{FilePrefix}')");
			return false;
		}

		var pptFileInfo = new FileInfo(FilePath);
		if (!pptFileInfo.Exists)
		{
			Console.WriteLine($"PPT file not found: {FilePath}");
			return false;
		}

		Console.WriteLine($"[DEBUG] PPT LastWriteTimeUtc: {pptFileInfo.LastWriteTimeUtc} ({pptFileInfo.LastWriteTimeUtc.Ticks})");

		// ✅ 캐싱: 파일 수정 시간을 포함한 캐시 키 생성
		string cacheKey = FilePath + "|" + FilePrefix + "|" + pptFileInfo.LastWriteTimeUtc.Ticks;
		if (_fileCheckCache.ContainsKey(cacheKey))
		{
			var cached = _fileCheckCache[cacheKey];
			if (DateTime.Now - cached.LastCheck < _cacheExpiration)
			{
				TotalSlides = cached.TotalSlides;
				Console.WriteLine($"[Cache Hit] File check: {Path.GetFileName(FilePath)}, Slides: {TotalSlides}");
				return cached.Exists;
			}
		}

		// 이전 캐시 키 패턴과 일치하는 항목 제거 (파일이 수정되었으므로)
		var oldCacheKeys = _fileCheckCache.Keys.Where(k => k.StartsWith(FilePath + "|" + FilePrefix + "|")).ToList();
		foreach (var oldKey in oldCacheKeys)
		{
			if (oldKey != cacheKey)
			{
				_fileCheckCache.Remove(oldKey);
			}
		}

		FileInfo jpgFileInfo = new FileInfo(FilePrefix + Convert.ToString(1) + ".jpg");
		Console.WriteLine($"Checking JPG: {jpgFileInfo.FullName}");

		// JPG 파일이 없으면 재빌드 필요
		if (!jpgFileInfo.Exists)
		{
			Console.WriteLine("JPG file not found - rebuild required");
			
			// 슬라이드 수를 가져오기 위해 프레젠테이션 열기 (필요한 경우)
			if (TotalSlides == 0 || presentation == null)
			{
				_Presentation tempPresentation = presentation;
				bool shouldClose = false;
				try
				{
					if (tempPresentation == null)
					{
						tempPresentation = prePowerPointApp.Presentations.Open(FilePath, MsoTriState.msoFalse, MsoTriState.msoTrue, MsoTriState.msoFalse);
						shouldClose = true;
					}
					TotalSlides = tempPresentation.Slides.Count;
					Console.WriteLine($"Presentation opened for slide count: {Path.GetFileName(FilePath)}, Slides: {TotalSlides}");
				}
				catch (Exception ex)
				{
					Console.WriteLine($"Error opening presentation to get slide count: {ex.Message}");
				}
				finally
				{
					if (shouldClose && tempPresentation != null)
						ClosePresentation(ref tempPresentation);
				}
			}
			
			_fileCheckCache[cacheKey] = (false, DateTime.Now, TotalSlides);
			return false;
		}

		// JPG가 PPT보다 최신이거나 같으면 이미 빌드됨
		Console.WriteLine($"[DEBUG] Comparing times - JPG: {jpgFileInfo.LastWriteTimeUtc} ({jpgFileInfo.LastWriteTimeUtc.Ticks}), PPT: {pptFileInfo.LastWriteTimeUtc} ({pptFileInfo.LastWriteTimeUtc.Ticks})");
		if (jpgFileInfo.LastWriteTimeUtc >= pptFileInfo.LastWriteTimeUtc)
		{
			Console.WriteLine($"JPG is newer than or equal to PPT - already built (PPT: {pptFileInfo.LastWriteTimeUtc}, JPG: {jpgFileInfo.LastWriteTimeUtc})");
			
			// 슬라이드 수를 가져오기 위해 프레젠테이션 열기 (필요한 경우)
			if (TotalSlides == 0 || presentation == null)
			{
				_Presentation tempPresentation = presentation;
				bool shouldClose = false;
				try
				{
					if (tempPresentation == null)
					{
						tempPresentation = prePowerPointApp.Presentations.Open(FilePath, MsoTriState.msoFalse, MsoTriState.msoTrue, MsoTriState.msoFalse);
						shouldClose = true;
					}
					TotalSlides = tempPresentation.Slides.Count;
					Console.WriteLine($"Presentation opened for slide count: {Path.GetFileName(FilePath)}, Slides: {TotalSlides}");
				}
				catch (Exception ex)
				{
					Console.WriteLine($"Error opening presentation to get slide count: {ex.Message}");
					_fileCheckCache[cacheKey] = (false, DateTime.Now, TotalSlides);
					return false;
				}
				finally
				{
					if (shouldClose && tempPresentation != null)
						ClosePresentation(ref tempPresentation);
				}
			}
			
			_fileCheckCache[cacheKey] = (true, DateTime.Now, TotalSlides);
			Console.WriteLine($"[Cache Store] Already built: {Path.GetFileName(FilePath)}, Slides: {TotalSlides}");
			return true;
		}
		else
		{
			// PPT가 JPG보다 최신이면 재빌드 필요
			Console.WriteLine($"PPT is newer than JPG - rebuild required (PPT: {pptFileInfo.LastWriteTimeUtc}, JPG: {jpgFileInfo.LastWriteTimeUtc})");
			
			// 슬라이드 수를 가져오기 위해 프레젠테이션 열기 (필요한 경우)
			if (TotalSlides == 0 || presentation == null)
			{
				_Presentation tempPresentation = presentation;
				bool shouldClose = false;
				try
				{
					if (tempPresentation == null)
					{
						tempPresentation = prePowerPointApp.Presentations.Open(FilePath, MsoTriState.msoFalse, MsoTriState.msoTrue, MsoTriState.msoFalse);
						shouldClose = true;
					}
					TotalSlides = tempPresentation.Slides.Count;
					Console.WriteLine($"Presentation opened for slide count: {Path.GetFileName(FilePath)}, Slides: {TotalSlides}");
				}
				catch (Exception ex)
				{
					Console.WriteLine($"Error opening presentation to get slide count: {ex.Message}");
				}
				finally
				{
					if (shouldClose && tempPresentation != null)
						ClosePresentation(ref tempPresentation);
				}
			}
			
			_fileCheckCache[cacheKey] = (false, DateTime.Now, TotalSlides);
			return false;
		}
	}



		private static string GetMD5(string path)
		{
			using (var fs = File.OpenRead(path))
			using (var md5 = new MD5CryptoServiceProvider())
				return string.Join("", md5.ComputeHash(fs).ToArray().Select(i => i.ToString("X2")));
		}

		/// <summary>
		/// 비동기 버전의 BuildScreenPreDumps - UI 블로킹 방지
		/// </summary>
		public bool BuildScreenOutDumps(string FilePath, string FilePrefix, ref int TotalSlides, int MAX_VERSES, int DB_MAXSLIDES, ref int[] SongVerses, ref int[,] Slide, string[] SequenceSymbol)
		{
			bool result = true;
			try
			{
				Console.WriteLine($"[BuildScreenOutDumps] FilePath='{FilePath}', FilePrefix='{FilePrefix}'");
				ResetVerses(MAX_VERSES, DB_MAXSLIDES, ref SongVerses, ref Slide);
				if (prePowerPointApp == null)
				{
					Console.WriteLine("[BuildScreenOutDumps] prePowerPointApp is null");
					return result;
				}

				// ✅ 먼저 캐시 체크 (프레젠테이션을 열기 전에)
			Console.WriteLine($"[DEBUG BuildScreenOutDumps] Calling IsBuildedFileCheck...");
			bool isBuilt = IsBuildedFileCheck(FilePath, FilePrefix, ref TotalSlides);
			Console.WriteLine($"[DEBUG BuildScreenOutDumps] IsBuildedFileCheck returned: {isBuilt}");
			if (isBuilt)
			{
				Console.WriteLine($"Output preview images already exist (cache hit) for: {FilePrefix}");
					
					// 캐시 히트: Verse 정보 파싱을 위해서만 프레젠테이션 열기
					presentation = prePowerPointApp.Presentations.Open(FilePath, MsoTriState.msoFalse, MsoTriState.msoTrue, MsoTriState.msoFalse);
					Console.WriteLine(FilePath);
					if (presentation != null)
					{
						ParseVersesFromPresentation(presentation, TotalSlides, ref SongVerses, ref Slide, SequenceSymbol);
						StoreCachedVerses(FilePath, TotalSlides, MAX_VERSES, SongVerses, Slide);
					}
					return result;
				}

				// 캐시 미스: 이미지를 새로 생성해야 함
				presentation = prePowerPointApp.Presentations.Open(FilePath, MsoTriState.msoFalse, MsoTriState.msoFalse, MsoTriState.msoFalse);
				Console.WriteLine(FilePath);
				if ((presentation == null) & (TotalSlides > 0))
				{
					return result;
				}
				TotalSlides = presentation.Slides.Count;
				String strPreFileName = "";
				String strOutFileName = "";

				Console.WriteLine($"Building {TotalSlides} output preview images to: {FilePrefix}");
				for (int i = 1; i <= TotalSlides; i++)
				{
					strOutFileName = FilePrefix + Convert.ToString(i) + ".jpg";
					presentation.Slides[i].Export(strOutFileName, "JPG", EXPORT_WIDTH, EXPORT_HEIGHT);
					Console.WriteLine($"Exported slide {i} to: {strOutFileName}");
					//Console.WriteLine("SlideID:{0}, SlideNumber: {1} ", Pres.Slides.Item(l).SlideID, Pres.Slides.Item(l).SlideNumber);
					strPreFileName = strOutFileName.Replace("~OUTPPPreview$", "~PREPPPreview$");

					// 파일 복사 재시도 (파일이 다른 프로세스에 의해 잠겨있을 수 있음)
					int retryCount = 0;
					bool copySucceeded = false;
					while (retryCount < 3 && !copySucceeded)
					{
						try
						{
							File.Copy(strOutFileName, strPreFileName, true);
							Console.WriteLine($"Copied to preview: {strPreFileName}");
							copySucceeded = true;
						}
						catch (IOException ex)
						{
							retryCount++;
							if (retryCount < 3)
							{
								Console.WriteLine($"File copy retry {retryCount}/3: {ex.Message}");
								System.Threading.Thread.Sleep(100);
							}
							else
							{
								Console.WriteLine($"File copy failed after 3 attempts: {ex.Message}");
								// 복사 실패시에도 계속 진행 (다음 슬라이드 처리)
							}
						}
					}
				}
				Console.WriteLine($"Completed building {TotalSlides} output preview images");

			// ✅ 이미지 생성 완료 후 캐시 업데이트
			UpdateImageBuildCache(FilePath, FilePrefix, TotalSlides);

			// Verse 정보 파싱
				ParseVersesFromPresentation(presentation, TotalSlides, ref SongVerses, ref Slide, SequenceSymbol);
				StoreCachedVerses(FilePath, TotalSlides, MAX_VERSES, SongVerses, Slide);

				return result;
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
				Console.WriteLine(e.StackTrace);
				return false;
			}
			finally
			{
				ClosePresentation(ref presentation);
			}
		}


// ✅ 이미지 생성 완료 후 캐시 업데이트 헬퍼 메서드
private void UpdateImageBuildCache(string FilePath, string FilePrefix, int TotalSlides)
{
	var pptFileInfo = new FileInfo(FilePath);
	string cacheKey = FilePath + "|" + FilePrefix + "|" + pptFileInfo.LastWriteTimeUtc.Ticks;
	_fileCheckCache[cacheKey] = (true, DateTime.Now, TotalSlides);
	Console.WriteLine($"[Cache Store] Images built successfully: {Path.GetFileName(FilePath)}, Slides: {TotalSlides}");
}

		/// <summary>
		/// Clears all verse data (same range as BuildScreenDumps) so nothing from a previously loaded item survives.
		/// </summary>
		public static void ResetVerses(int MAX_VERSES, int DB_MAXSLIDES, ref int[] SongVerses, ref int[,] Slide)
		{
			for (int i = 1; i < DB_MAXSLIDES; i++)
			{
				Slide[i, 0] = -1;
			}
			for (int j = 0; j <= MAX_VERSES; j++)
			{
				SongVerses[j] = 0;
			}
		}

		// Verse data already parsed in this session, keyed by file path; the entry is valid only while the
		// file's last write time is unchanged. Static (shared by all wrappers) because every caller runs on
		// the UI thread, so no locking is needed.
		private static readonly Dictionary<string, (DateTime LastWriteUtc, int[] SongVerses, int[] SlideTypes)> _verseCache =
			new Dictionary<string, (DateTime, int[], int[])>(StringComparer.OrdinalIgnoreCase);

		private static void StoreCachedVerses(string FilePath, int totalSlides, int MAX_VERSES, int[] SongVerses, int[,] Slide)
		{
			try
			{
				if (string.IsNullOrEmpty(FilePath) || !File.Exists(FilePath))
				{
					return;
				}
				int slideCount = Math.Max(0, Math.Min(totalSlides, Slide.GetLength(0) - 1));
				int[] verses = new int[MAX_VERSES + 1];
				Array.Copy(SongVerses, verses, verses.Length);
				int[] types = new int[slideCount + 1];
				for (int i = 1; i <= slideCount; i++)
				{
					types[i] = Slide[i, 0];
				}
				_verseCache[FilePath] = (File.GetLastWriteTimeUtc(FilePath), verses, types);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error caching verses: {ex.Message}");
			}
		}

		// Resets the verse data, then copies the cached values when an up-to-date entry exists.
		private static bool TryLoadCachedVerses(string FilePath, int MAX_VERSES, int DB_MAXSLIDES, ref int[] SongVerses, ref int[,] Slide)
		{
			ResetVerses(MAX_VERSES, DB_MAXSLIDES, ref SongVerses, ref Slide);
			if (string.IsNullOrEmpty(FilePath) || !_verseCache.TryGetValue(FilePath, out var entry))
			{
				return false;
			}
			if (!File.Exists(FilePath) || File.GetLastWriteTimeUtc(FilePath) != entry.LastWriteUtc)
			{
				// Stale: the file changed (or is gone) since it was parsed.
				_verseCache.Remove(FilePath);
				return false;
			}
			Array.Copy(entry.SongVerses, SongVerses, Math.Min(entry.SongVerses.Length, MAX_VERSES + 1));
			for (int i = 1; i < entry.SlideTypes.Length && i < DB_MAXSLIDES; i++)
			{
				Slide[i, 0] = entry.SlideTypes[i];
			}
			return true;
		}

		/// <summary>
		/// Re-reads the verse markers from the slide notes without exporting images
		/// (used when the slide images are already built). Uses the values cached in this session when the
		/// file is unchanged; otherwise opens the file, unless allowOpen is false.
		/// Returns false on any failure (verse data is then left reset); never throws.
		/// </summary>
		public bool LoadVerses(string FilePath, int MAX_VERSES, int DB_MAXSLIDES, ref int[] SongVerses, ref int[,] Slide, string[] SequenceSymbol, bool allowOpen = true)
		{
			_Presentation versePresentation = null;
			try
			{
				if (TryLoadCachedVerses(FilePath, MAX_VERSES, DB_MAXSLIDES, ref SongVerses, ref Slide))
				{
					return true;
				}
				if (!allowOpen || prePowerPointApp == null || string.IsNullOrEmpty(FilePath) || !File.Exists(FilePath))
				{
					return false;
				}
				versePresentation = prePowerPointApp.Presentations.Open(FilePath, MsoTriState.msoTrue, MsoTriState.msoTrue, MsoTriState.msoFalse);
				int slideCount = versePresentation.Slides.Count;
				ParseVersesFromPresentation(versePresentation, slideCount, ref SongVerses, ref Slide, SequenceSymbol);
				StoreCachedVerses(FilePath, slideCount, MAX_VERSES, SongVerses, Slide);
				return true;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error in LoadVerses: {ex.Message}");
				try
				{
					// Do not leave partially parsed markers behind.
					ResetVerses(MAX_VERSES, DB_MAXSLIDES, ref SongVerses, ref Slide);
				}
				catch
				{
				}
				return false;
			}
			finally
			{
				ClosePresentation(ref versePresentation);
			}
		}

		/// <summary>
		/// daniel
		/// </summary>
		/// <param name="FilePath"></param>
		/// <param name="FilePrefix"></param>
		/// <param name="TotalSlides"></param>
		/// <param name="MAX_VERSES"></param>
		/// <param name="DB_MAXSLIDES"></param>
		/// <param name="SongVerses"></param>
		/// <param name="Slide"></param>
		/// <param name="SequenceSymbol"></param>
		/// <returns></returns>
		public bool BuildScreenDumps(string FilePath, string FilePrefix, ref int TotalSlides, int MAX_VERSES, int DB_MAXSLIDES, ref int[] SongVerses, ref int[,] Slide, string[] SequenceSymbol)
		{
			bool result = true;
			try
			{
				Console.WriteLine($"[BuildScreenDumps] FilePath='{FilePath}', FilePrefix='{FilePrefix}'");
				for (int i = 1; i < DB_MAXSLIDES; i++)
				{
					Slide[i, 0] = -1;
				}
				for (int j = 0; j <= MAX_VERSES; j++)
				{
					SongVerses[j] = 0;
				}
				if (prePowerPointApp == null)
				{
					Console.WriteLine("[BuildScreenDumps] prePowerPointApp is null");
					return result;
				}

				presentation = prePowerPointApp.Presentations.Open(FilePath, MsoTriState.msoFalse, MsoTriState.msoFalse, MsoTriState.msoFalse);
				TotalSlides = presentation.Slides.Count;
				if ((presentation == null) & (TotalSlides > 0))
				{
					return result;
				}

				for (int k = 1; k <= TotalSlides; k++)
				{
					presentation.Slides[k].Export(FilePrefix + Convert.ToString(k) + ".jpg", "JPG", EXPORT_WIDTH, EXPORT_HEIGHT);
				}

				// Verse 정보 파싱
				ParseVersesFromPresentation(presentation, TotalSlides, ref SongVerses, ref Slide, SequenceSymbol);
				StoreCachedVerses(FilePath, TotalSlides, MAX_VERSES, SongVerses, Slide);

				return result;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error in BuildScreenDumps: {ex.Message}");
				return false;
			}
			finally
			{
				ClosePresentation(ref presentation);
			}
		}

		public int StringToInt(string InValue)
		{
			bool minus1IfBlank = false;
			return StringToInt(InValue, minus1IfBlank);
		}

		public int StringToInt(string InValue, bool Minus1IfBlank)
		{
			try
			{
				return Convert.ToInt32(InValue);
			}
			catch
			{
				return Minus1IfBlank ? (-1) : 0;
			}
		}

		public bool BuildFirstScreenDump(string[] InFileList, int MaxFileCount, string DumpFolderPrefix)
		{
			bool result = true;
			try
			{
				if (prePowerPointApp == null)
				{
					return result;
				}
				for (int i = 0; i < MaxFileCount; i++)
				{
					ScreenDumpFullPath[0, i] = InFileList[i];
					ScreenDumpFullPath[1, i] = DumpFolderPrefix + Path.GetFileNameWithoutExtension(InFileList[i]) + ".jpg";
				}
				return result;
			}
			catch
			{
				return false;
			}
			finally
			{
				ClosePresentation(ref presentation);
			}
		}

		public bool BuildOneFirstScreenDump(int PPNumbering)
		{
			int num = PPNumbering - 1;
			if (num < 0 || ScreenDumpFullPath[0, num] == null)
			{
				return false;
			}
			bool result = true;
			try
			{
				if (prePowerPointApp == null)
				{
					return result;
				}
				presentation = prePowerPointApp.Presentations.Open(ScreenDumpFullPath[0, num], MsoTriState.msoFalse, MsoTriState.msoFalse, MsoTriState.msoFalse);
				presentation.Slides[1].Export(ScreenDumpFullPath[1, num], "JPG", EXPORT_WIDTH, EXPORT_HEIGHT);

				return result;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error in BuildOneFirstScreenDump: {ex.Message}");
				return false;
			}
			finally
            {
				ClosePresentation(ref presentation);
			}
		}

        public void ClosePresentation(ref _Presentation presentation)
        {
            try
            {
                if (presentation != null)
                {
                    presentation.Close();
                    presentation.Dispose();  // NetOffice���� �����ϴ� Dispose() ���?
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error closing presentation: {e.Message}");
            }
            finally
            {
                presentation = null;
            }
        }

		public void QuitPowerPointApp(Application prePowerPointApp)
		{
			try
			{
				if (prePowerPointApp != null && !prePowerPointApp.IsDisposed)
				{
					// PowerPoint is single-instance, so this may be the user's own PowerPoint.
					// Quit closes every open presentation without asking to save, so only quit when none is open.
					if (GetOpenPresentationCount(prePowerPointApp) == 0)
					{
						try
						{
							prePowerPointApp.Quit();
						}
						catch (Exception e)
						{
							Console.WriteLine($"Error quitting PowerPoint: {e.Message}");
							ForceKillPowerPoint(prePowerPointApp);
						}
					}
					prePowerPointApp.Dispose();  // Dispose()?? Quit() ???Ŀ? ???
				}
			}
			catch (ObjectDisposedException)
			{
				Console.WriteLine("PowerPoint application was already disposed.");
			}
			catch (Exception e)
			{
				Console.WriteLine($"Error releasing PowerPoint: {e.Message}");
			}
			finally
			{
				if (ReferenceEquals(prePowerPointApp, this.prePowerPointApp))
				{
					this.prePowerPointApp = null;
				}
			}
		}

		private void ForceKillPowerPoint(Application prePowerPointApp)
		{
			try
			{
				int processId = TryGetPowerPointProcessId(prePowerPointApp);
				if (processId <= 0)
				{
					Console.WriteLine("PowerPoint process id could not be resolved; skip force kill.");
					return;
				}

				Process.GetProcessById(processId).Kill();
				Console.WriteLine($"PowerPoint process (PID={processId}) was forcibly terminated.");
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error killing PowerPoint process: {ex.Message}");
			}
		}

		[DllImport("user32.dll")]
		private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

		private static int TryGetPowerPointProcessId(Application prePowerPointApp)
		{
			if (prePowerPointApp == null)
			{
				return 0;
			}

			try
			{
				int hwnd = prePowerPointApp.HWND;
				if (hwnd == 0)
				{
					return 0;
				}

				GetWindowThreadProcessId(new IntPtr(hwnd), out uint pid);
				return (int)pid;
			}
			catch
			{
				return 0;
			}
		}

		public void ClosePresentation_Old(ref _Presentation presentation)
		{
			try
			{
				if (presentation != null)
				{
					presentation.Close();

					presentation.Dispose(true);
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
				Console.WriteLine(e.StackTrace);
			}
			finally
			{
				presentation = null;
			}
		}

		public void QuitPowerPointApp_Old()
		{
			try
			{
				if (prePowerPointApp != null)
				{
                    prePowerPointApp.Quit();

                    prePowerPointApp.Dispose();
                    prePowerPointApp = null;
                }
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
				Console.WriteLine(e.StackTrace);
			}
			finally
			{
				if (prePowerPointApp != null)
				{
					prePowerPointApp.Dispose();
					prePowerPointApp = null;
				}
			}
		}
	}

	/// <summary>
	/// BuildScreenPreDumpsAsync의 결과를 담는 클래스
	/// </summary>
	public class BuildScreenDumpsResult
	{
		public bool Success { get; set; }
		public int TotalSlides { get; set; }
		public int[] SongVerses { get; set; }
		public int[,] Slide { get; set; }
	}
}




