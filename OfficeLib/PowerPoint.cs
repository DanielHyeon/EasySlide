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
			using PowerPointGate gate = PowerPointGate.Enter(PowerPointCallWeight.Heavy, "attach to PowerPoint");
			if (gate == null)
			{
				Console.WriteLine("Init: PowerPoint is not responding; not attached");
				return;
			}
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

		/// <summary>
		/// The result of Open and Run when PowerPoint was not asked because it does not respond, or their group was
		/// given up: not a failure of the file or the copy (the pre-load does not report the file as not opened).
		/// </summary>
		public const string NotRespondingText = "PowerPoint is not responding";

		public string Open(string InPowerpointFullPath, ref ShowCopyList PowerpointList)
		{
			string text = "";
			Presentation presentation = null;
			using PowerPointGate gate = PowerPointGate.Enter(PowerPointCallWeight.Heavy, "open " + Path.GetFileName(InPowerpointFullPath));
			if (gate == null)
			{
				return NotRespondingText;
			}

			try
			{
				_ = prePowerPointApp.Presentations.Count;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error accessing Presentations: {ex.Message}");
				return PowerPointCallLimit.CurrentAbandoned ? NotRespondingText : "Error: PowerPoint application not available";
			}

			try
			{
				// One entry per path: a path whose copy is gone (closed in PowerPoint, or PowerPoint restarted) gets
				// its entry replaced below, so the dead name cannot hide the new copy.
				int entry = FindListedEntry(InPowerpointFullPath, PowerpointList);
				text = (entry > 0) ? PowerpointList[entry].CopyName : "";
				bool flag = entry > 0 && IsShowCopyOpen(text, PowerpointList[entry].SourcePath);
				try
				{
					if (!flag)
					{
						presentation = prePowerPointApp.Presentations.Open(InPowerpointFullPath, MsoTriState.msoTrue, MsoTriState.msoTrue);
						if (presentation != null)
						{
							// The mark that tells this copy from a user's presentation with the same "PresentationN"
							// name (names start again from 1 when PowerPoint is restarted).
							Tags tags = presentation.Tags;
							try
							{
								tags.Add(ShowCopyTagName, ShowCopyMark(InPowerpointFullPath));
							}
							finally
							{
								ReleaseProxy(tags);
							}
							// Tags.Add marks the copy as changed: without this, closing it by hand in PowerPoint's own
							// window asks the operator whether to save it (seen with PowerPoint 16). Presentation.Close
							// called from the app does not ask either way.
							presentation.Saved = MsoTriState.msoTrue;
                            string copyName = presentation.FullName;
                            int row;
                            if (entry > 0)
                            {
                                row = entry;
                                PowerpointList[row].SourcePath = InPowerpointFullPath;
                                PowerpointList[row].CopyName = copyName;
                            }
                            else
                            {
                                row = PowerpointList.Add(InPowerpointFullPath, copyName);
                            }
                            ClearStaleRowsWithName(copyName, row, PowerpointList);
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
						if (PowerPointCallLimit.CurrentAbandoned)
						{
							// Given up: closed (then released) when PowerPoint answers again.
							presentationsToClose.Add(presentation);
						}
						else
						{
							try
							{
								presentation.Close();
							}
							catch { }
							ReleaseProxy(presentation);
						}
					}
					return PowerPointCallLimit.CurrentAbandoned ? NotRespondingText : "Error Loading: " + text;
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error in Open method: {ex.Message}");
				return PowerPointCallLimit.CurrentAbandoned ? NotRespondingText : "Error Loading: " + text;
			}
			return "";
		}

		public string Run(string InPowerpointFullPath, ref ShowCopyList PowerpointList)
		{
			string text = "";
			string text2 = "";
			SlideShowRaiseError = null;
			SlideShowRaiseSkipped = null;
			LastRunStarted = false;
			currentSlideShowWindow = IntPtr.Zero;
			ForgetRunPresentation();
			using PowerPointGate gate = PowerPointGate.Enter(PowerPointCallWeight.Heavy, "start the slide show of " + Path.GetFileName(InPowerpointFullPath));
			if (gate == null)
			{
				return NotRespondingText;
			}
			try
			{
				string version = prePowerPointApp.Version;

				SaveRegValue(version, @"PowerPoint\Options", "DisplayMonitor", displayName);

				int entry = FindListedEntry(InPowerpointFullPath, PowerpointList);
				text = (entry > 0) ? PowerpointList[entry].CopyName : "";
				text2 = "";
				if (!(text != ""))
				{
					return "Cannot find opened Presentation: " + PowerpointList.Count + ", " + InPowerpointFullPath;
				}
				string mark = ShowCopyMark(PowerpointList[entry].SourcePath);
				bool unmarked = false;

				// Every proxy read through the collection is released with it at the end of the block.
				Application app = prePowerPointApp;
				// ReleaseProxy: kept instead of released when the group was given up (a release would wait).
				Presentations presentations = app.Presentations;
				try
				{
					int count = presentations.Count;
					for (int i = 1; i <= count && !PowerPointCallLimit.CurrentAbandoned; i++)
					{
						Presentation candidate = presentations[i];
						string fullName = candidate.FullName;
						PowerPointCallLimit.Progress();
						text2 = text2 + i + "-" + candidate.Name + "(" + fullName + ") ";
						if (string.Equals(fullName, text, StringComparison.OrdinalIgnoreCase))
						{
							if (!HasShowCopyMark(candidate, mark))
							{
								// The name of this item's copy, but not the copy: a presentation the app did not
								// open (PowerPoint was restarted and gave the name again). Never run.
								unmarked = true;
								continue;
							}
							SlideShowSettings settings = candidate.SlideShowSettings;
							SlideShowWindow window = null;
							try
							{
								DisablePresenterView(settings);
								window = settings.Run();
								PowerPointCallLimit.Progress();
								LastRunStarted = true;
								RememberRunPresentation(fullName, i, mark, app);
								if (KeepSlideShowOnTop)
								{
									// The slide show window's title holds FullName ("PresentationN"); Name can be
									// another text for these untitled copies (seen with PowerPoint 16: the first
									// slide's title).
									currentSlideShowWindow = FindSlideShowWindow(window, () => fullName);
									RaiseCurrentSlideShowWindow();
								}
							}
							finally
							{
								ReleaseProxy(window);
								ReleaseProxy(settings);
							}

							return "Presentation Run OK";
						}
					}
				}
				finally
				{
					ReleaseProxy(presentations);
				}
				if (PowerPointCallLimit.CurrentAbandoned)
				{
					// Given up before the slide show started: PowerPoint did not answer, the copy was not looked at.
					return NotRespondingText;
				}
				if (unmarked)
				{
					return "The copy opened for this item is no longer open (the presentation named " + text + " was not opened by EasiSlides)";
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error running presentation: {ex.Message}");
				if (PowerPointCallLimit.CurrentAbandoned)
				{
					LastRunStarted = false;
					ForgetRunPresentation();
					return NotRespondingText;
				}
				return "Error: " + PowerpointList.Count + ", " + InPowerpointFullPath;
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

		[DllImport("user32.dll", SetLastError = true)]
		private static extern IntPtr SendMessageTimeout(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam, uint fuFlags, uint uTimeout, out IntPtr lpdwResult);

		private const uint WM_NULL = 0x0000;
		private const uint SMTO_ABORTIFHUNG = 0x0002;
		private const int ERROR_TIMEOUT = 1460;
		// LowerWindow (wait): how long a slide show window may take to answer before it is lowered synchronously.
		private const uint LowerAnswerTimeoutMs = 300;

		/// <summary>What ProbePowerPoint found.</summary>
		public enum PowerPointAnswer
		{
			/// <summary>No PowerPoint process in this session: a call fails at once, nothing can hang.</summary>
			NotRunning,
			/// <summary>Every PowerPoint process of this session answered.</summary>
			Answers,
			/// <summary>A PowerPoint process has no window to ask (starting or exiting): not known.</summary>
			NoWindow,
			/// <summary>A PowerPoint process did not answer within the time.</summary>
			NotResponding
		}

		/// <summary>
		/// Asks PowerPoint whether it answers before a group of calls that must not hang the UI thread (show end,
		/// app exit): WM_NULL through SendMessageTimeout (SMTO_ABORTIFHUNG) to the main window (class
		/// "PPTFrameClass") of every PowerPoint process in this session. That window belongs to the thread that
		/// serves PowerPoint's COM calls, and it exists, hidden, also when PowerPoint was started by the app and never
		/// shown, so a healthy hidden PowerPoint answers at once (measured: under 1 ms; a suspended one: no answer,
		/// about timeoutMs). A process without that window is asked through any other top-level window of it. No COM
		/// call: the processes and windows are enumerated. The app's proxies cannot tell which process they reach
		/// without a call, and PowerPoint runs one process per session, so every process is asked; one that does not
		/// answer makes the result NotResponding. detail says what was found (for the log).
		/// </summary>
		public static PowerPointAnswer ProbePowerPoint(int timeoutMs, out string detail)
		{
			HashSet<uint> processIds = GetPowerPointProcessIds();
			if (processIds.Count == 0)
			{
				detail = "no PowerPoint process";
				return PowerPointAnswer.NotRunning;
			}
			Dictionary<uint, List<IntPtr>> frames = new Dictionary<uint, List<IntPtr>>();
			Dictionary<uint, IntPtr> others = new Dictionary<uint, IntPtr>();
			EnumWindows((handle, unused) =>
			{
				GetWindowThreadProcessId(handle, out uint processId);
				if (!processIds.Contains(processId))
				{
					return true;
				}
				StringBuilder className = new StringBuilder(64);
				GetClassName(handle, className, className.Capacity);
				if (className.ToString() == "PPTFrameClass")
				{
					if (!frames.TryGetValue(processId, out List<IntPtr> list))
					{
						frames[processId] = list = new List<IntPtr>();
					}
					list.Add(handle);
				}
				else if (!others.ContainsKey(processId))
				{
					others[processId] = handle;
				}
				return true;
			}, IntPtr.Zero);
			PowerPointAnswer result = PowerPointAnswer.Answers;
			StringBuilder text = new StringBuilder();
			foreach (uint processId in processIds)
			{
				List<IntPtr> targets = frames.TryGetValue(processId, out List<IntPtr> found) ? found : new List<IntPtr>();
				if (targets.Count == 0 && others.TryGetValue(processId, out IntPtr other))
				{
					targets.Add(other);
				}
				if (text.Length > 0)
				{
					text.Append("; ");
				}
				if (targets.Count == 0)
				{
					text.Append("POWERPNT ").Append(processId).Append(": no window");
					if (result == PowerPointAnswer.Answers)
					{
						result = PowerPointAnswer.NoWindow;
					}
					continue;
				}
				// PowerPoint has one main window per open presentation, all on the same thread: one that answers or
				// times out says it for all of them. The next one is asked only when a window failed at once (e.g. it
				// was closed meanwhile), so a process never takes more than about timeoutMs.
				// A process counts as not responding only when a send really timed out (ERROR_TIMEOUT, or about the
				// whole time elapsed) or Windows reports the window hung; a send that fails at once on every window
				// (window destroyed meanwhile, access denied) says nothing about PowerPoint: not known (NoWindow).
				Stopwatch watch = Stopwatch.StartNew();
				bool answered = false;
				bool timedOut = false;
				int lastError = 0;
				foreach (IntPtr target in targets)
				{
					Stopwatch one = Stopwatch.StartNew();
					if (SendMessageTimeout(target, WM_NULL, IntPtr.Zero, IntPtr.Zero, SMTO_ABORTIFHUNG, (uint)timeoutMs, out _) != IntPtr.Zero)
					{
						answered = true;
						break;
					}
					lastError = Marshal.GetLastWin32Error();
					if (lastError == ERROR_TIMEOUT || one.ElapsedMilliseconds >= timeoutMs * 9L / 10 || IsHungAppWindow(target))
					{
						timedOut = true;
						break;
					}
					if (watch.ElapsedMilliseconds >= timeoutMs / 2)
					{
						break;
					}
				}
				text.Append("POWERPNT ").Append(processId)
					.Append(answered ? ": answered in " : (timedOut ? ": no answer in " : ": could not be asked in ")).Append(watch.ElapsedMilliseconds).Append(" ms");
				if (!answered)
				{
					text.Append(" (error ").Append(lastError).Append(')');
				}
				if (timedOut)
				{
					result = PowerPointAnswer.NotResponding;
				}
				else if (!answered && result == PowerPointAnswer.Answers)
				{
					result = PowerPointAnswer.NoWindow;
				}
			}
			detail = text.ToString();
			return result;
		}

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
			else if (IsHungAppWindow(handle)
				|| SendMessageTimeout(handle, WM_NULL, IntPtr.Zero, IntPtr.Zero, SMTO_ABORTIFHUNG, LowerAnswerTimeoutMs, out _) == IntPtr.Zero)
			{
				// A sent SetWindowPos waits until the window's thread handles it; a suspended PowerPoint is not
				// reported hung by IsHungAppWindow (measured), so the window is asked first (WM_NULL, a short wait).
				// It does not answer: the lowering is posted (carried out when PowerPoint handles messages again, before
				// any COM call that reaches it later, since those are queued behind it) and the window stays on the
				// list (skipped), so a later call lowers it again.
				skipped = true;
				uint posted = flags | SWP_ASYNCWINDOWPOS;
				return SetWindowPos(handle, HWND_NOTOPMOST, 0, 0, 0, 0, posted) && SetWindowPos(handle, HWND_BOTTOM, 0, 0, 0, 0, posted);
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
				bool lowered = LowerWindow(handle, wait, out bool skipped);
				if (!lowered)
				{
					int code = Marshal.GetLastWin32Error();
					error = (error == null ? "" : error + "; ") + "window 0x" + handle.ToString("X") + ": Win32 error " + code + " (" + new System.ComponentModel.Win32Exception(code).Message + ")";
				}
				if (skipped)
				{
					// PowerPoint is not responding (the lowering was posted): kept in the list, lowered again by a later
					// call (at the latest at show end).
					lock (raisedSlideShowWindowsLock)
					{
						raisedSlideShowWindows.Add((handle, processId));
					}
				}
			}
			return error;
		}

		public string GetPresentationName(string InPowerpointFullPath, ShowCopyList PowerpointList)
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

			int entry = FindListedEntry(InPowerpointFullPath, PowerpointList);
			return (entry > 0) ? PowerpointList[entry].CopyName : "";
		}

		/// <summary>
		/// The row of PowerpointList that holds the copy opened for this path (compared without case: Windows
		/// paths), or 0. Open keeps one row per path (it replaces the row when it opens a new copy), so this is
		/// the newest copy's row; the last match is taken in case an older list has more than one.
		/// </summary>
		private static int FindListedEntry(string InPowerpointFullPath, ShowCopyList PowerpointList)
		{
            if (string.IsNullOrEmpty(InPowerpointFullPath) || PowerpointList == null)
            {
                return 0;
            }
            for (int i = PowerpointList.Count; i >= 1; i--)
            {
                if (string.Equals(PowerpointList[i].SourcePath, InPowerpointFullPath, StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }
            }
            return 0;
        }

		// The mark Open puts on every copy it opens for the show (a presentation tag, in memory only: the copies
		// are untitled and never saved). Its value is this process's run token and the copy's source path, so a
		// presentation is the app's copy only when the name AND the mark match: a user's presentation that got the
		// same "PresentationN" name (PowerPoint restarted during the show) has no mark, and a copy left by an
		// earlier run of the app (crashed) has another token. Shared by all wrappers (they all drive one PowerPoint).
		private const string ShowCopyTagName = "EASISLIDES_SHOWCOPY";
		private static readonly string ShowCopyRunToken = Guid.NewGuid().ToString("N");

		private static string ShowCopyMark(string path)
		{
			return ShowCopyRunToken + "|" + path;
		}

		/// <summary>
		/// True when the presentation carries this mark. A missing tag reads as an empty string. Throws when the
		/// tag cannot be read.
		/// </summary>
		private static bool HasShowCopyMark(Presentation presentation, string mark)
		{
			Tags tags = presentation.Tags;
			try
			{
				return string.Equals(tags[ShowCopyTagName], mark, StringComparison.OrdinalIgnoreCase);
			}
			finally
			{
				ReleaseProxy(tags);
			}
		}

		/// <summary>
		/// The source path of the listed copy this presentation is, or null: a row has its name AND that row's
		/// mark. Every row with the name is tried: a stale row (its copy gone, e.g. after a PowerPoint restart) can
		/// carry the name PowerPoint has since given to another path's copy. The tag is read once, and only when
		/// some row has the name. Throws when the presentation cannot be read.
		/// </summary>
		private static string GetShowCopyPath(Presentation presentation, string fullName, ShowCopyList PowerpointList)
		{
			if (string.IsNullOrEmpty(fullName) || PowerpointList == null)
			{
				return null;
			}
			string tag = null;
			int last = PowerpointList.Count;
			for (int i = 1; i <= last; i++)
			{
				if (string.Equals(PowerpointList[i].CopyName, fullName, StringComparison.OrdinalIgnoreCase))
				{
					if (tag == null)
					{
						Tags tags = presentation.Tags;
						try
						{
							tag = tags[ShowCopyTagName] ?? "";
						}
						finally
						{
							ReleaseProxy(tags);
						}
					}
					string path = PowerpointList[i].SourcePath;
					if (string.Equals(tag, ShowCopyMark(path), StringComparison.OrdinalIgnoreCase))
					{
						return path;
					}
				}
			}
			return null;
		}

		/// <summary>
		/// After Open gave a row a new copy's name: any other row still holding that name is stale (names are
		/// unique within one PowerPoint process, so its copy is gone). Its name is cleared, so that row reads as
		/// "not opened" (Run reports the item as not started, the next pre-load opens it again) and can never be
		/// matched against this copy. The row itself stays: no row number moves.
		/// </summary>
		private static void ClearStaleRowsWithName(string fullName, int keepRow, ShowCopyList PowerpointList)
		{
            for (int i = 1; i <= PowerpointList.Count; i++)
            {
                if (i != keepRow && string.Equals(PowerpointList[i].CopyName, fullName, StringComparison.OrdinalIgnoreCase))
                {
                    PowerpointList[i].CopyName = "";
                }
            }
        }

		/// <summary>
		/// True when the copy listed under this name for this path is open: a presentation has the name and the
		/// mark. Throws when PowerPoint cannot be read (the caller reports the file as not opened).
		/// </summary>
		private bool IsShowCopyOpen(string fullName, string path)
		{
			if (string.IsNullOrEmpty(fullName))
			{
				return false;
			}
			string mark = ShowCopyMark(path);
			Presentations presentations = prePowerPointApp.Presentations;
			try
			{
				int count = presentations.Count;
				for (int i = 1; i <= count && !PowerPointCallLimit.CurrentAbandoned; i++)
				{
					Presentation candidate = presentations[i];
					if (string.Equals(candidate.FullName, fullName, StringComparison.OrdinalIgnoreCase) && HasShowCopyMark(candidate, mark))
					{
						return true;
					}
					PowerPointCallLimit.Progress();
				}
				return false;
			}
			finally
			{
				ReleaseProxy(presentations);
			}
		}

		// The presentation whose slide show the last successful Run of this wrapper started, by FullName (the key
		// PowerpointList and the show-end close use), with its index in Presentations when it was last found.
		// Every call after the run (slide movement, slide count, verses, window placement) acts on this
		// presentation only, never on PowerPoint's ActivePresentation: that is whichever presentation PowerPoint
		// last activated, which after a failed run is the previous item's, and can be the user's own document.
		// The index is a hint: presentations opened or closed since (thumbnails are built from files opened in
		// the same PowerPoint, also during a show) shift it, so the name is checked on every use and the
		// collection is searched again when it does not match. No proxy is kept between calls: the wrapper's
		// Application proxy is replaced by every pre-load (NewApp), which releases the proxies under it.
		private string runPresentationName;
		private int runPresentationIndex;
		private int runPresentationGeneration;
		// The mark (see ShowCopyMark) of the run's presentation, and the Application proxy through which it was
		// last verified. Names are unique within one PowerPoint process, and a proxy reaches one process only (when
		// it dies, calls fail with RPC_E_DISCONNECTED), so the presentation found at the remembered index under that
		// same proxy is the verified one: no tag is read on a slide key. The mark is read again when the proxy has
		// changed since (NewApp at a pre-load, EnsureAttached, FollowRunOf) or the presentation is not at the
		// remembered index (rescan).
		private string runPresentationMark;
		private Application runMarkVerifiedApp;

		// Counts the show-end closes of the presentations opened for a show (ClearUpPowerpointWindows, on any
		// wrapper). A name remembered before such a close refers to a closed presentation, and PowerPoint gives
		// the same "PresentationN" names again after a restart, so it is not used any more.
		private static int closedPresentationsGeneration;

		/// <summary>
		/// The show's copies are closed, or are to be treated as gone (the show end skipped them because PowerPoint
		/// did not answer, or PowerPoint is no longer running): no wrapper acts on a run name or mark it remembered
		/// from them any more, since PowerPoint gives the same "PresentationN" names again. No COM call.
		/// </summary>
		public static void ForgetShowCopyNames()
		{
			closedPresentationsGeneration++;
		}

		/// <summary>
		/// Written by every call on this wrapper's run (First, Last, GotoSlide, Count, SetShowWindow,
		/// LoadVersesAndSlides, ImplementPowerpointSlideMovement, SafePlayNext) on every path. True when the call
		/// found nothing to act on: the last Run did not start a slide show, its presentation is gone, or (for
		/// the calls that need the slide show, i.e. all but Count and LoadVersesAndSlides) its slide show has
		/// ended. Also true when ImplementPowerpointSlideMovement found the slide show gone right after an error
		/// (it ended on that move). False otherwise, including when the call failed with an error or because
		/// PowerPoint was busy: that is not a slide show that has ended. Callers that do not look at it behave
		/// as before when the slide show runs.
		/// </summary>
		public bool LastCallNotRunning { get; private set; }

		/// <summary>
		/// True when the slide show the last Run started was not found by a later call: it was ended (Esc in the
		/// slide show) or closed in PowerPoint, or PowerPoint exited. Reset by the next Run, and when a later
		/// call finds the slide show again.
		/// </summary>
		public bool RunShowLost { get; private set; }

		/// <summary>
		/// The exception the last ImplementPowerpointSlideMovement caught while moving a running slide show, or
		/// null. The caller logs and reports it.
		/// </summary>
		public Exception LastNavigationError { get; private set; }

		private string CurrentRunPresentationName =>
			(runPresentationName != null && runPresentationGeneration == closedPresentationsGeneration) ? runPresentationName : null;

		private void RememberRunPresentation(string fullName, int index, string mark, Application verifiedThrough)
		{
			runPresentationName = fullName;
			runPresentationIndex = index;
			runPresentationGeneration = closedPresentationsGeneration;
			runPresentationMark = mark;
			runMarkVerifiedApp = verifiedThrough;
			RunShowLost = false;
		}

		private void ForgetRunPresentation()
		{
			runPresentationName = null;
			runPresentationIndex = 0;
			runPresentationMark = null;
			runMarkVerifiedApp = null;
			RunShowLost = false;
		}

		/// <summary>
		/// Stops this wrapper from acting on the slide show its last Run (or FollowRunOf) gave it: later calls do
		/// nothing (LastCallNotRunning) until the next Run. Used when that slide show is not the one on the
		/// projector, and for an item this wrapper does not run.
		/// </summary>
		public void ForgetRun()
		{
			ForgetRunPresentation();
		}

		/// <summary>
		/// The item run this wrapper made was given up after its slide show started (PowerPoint stopped answering
		/// during the calls after Run): the run counts as not started, and nothing acts on it (fix-ppt-show-call-timeout).
		/// The slide show window it raised stays on the list of raised windows, so it is lowered.
		/// </summary>
		public void ForgetAbandonedRun()
		{
			LastRunStarted = false;
			ForgetRunPresentation();
		}

		/// <summary>
		/// Looks the run's slide show up once and records the result (RunShowLost, LastCallNotRunning), without
		/// acting on it: FOUND clears RunShowLost, GONE sets it, UNKNOWN changes nothing. Used when another
		/// wrapper driving the same slide show found it gone, and to ask again about a slide show believed ended
		/// (it may have been started again). One lookup. Never throws.
		/// </summary>
		public void RefreshRunShowState()
		{
			LastCallNotRunning = false;
			using PowerPointGate gate = PowerPointGate.Enter(PowerPointCallWeight.Light, "look up the slide show");
			if (gate == null)
			{
				// UNKNOWN: nothing is known, nothing changes.
				return;
			}
			try
			{
				using RunPresentation run = FindRunSlideShow(out _);
			}
			catch (Exception ex)
			{
				// UNKNOWN: nothing is known, nothing changes.
				Console.WriteLine($"Error checking the slide show: {ex.Message}");
			}
		}

		/// <summary>
		/// Attaches this wrapper to PowerPoint again (NewApp) when its Application proxy no longer answers:
		/// PowerPoint was closed or restarted since the proxy was made. The probe is one cross-process call
		/// (Application.Version): well under a millisecond while PowerPoint runs, and it fails at once when the
		/// process is gone. A busy PowerPoint counts as alive. The run is forgotten on a re-attach: it belonged to
		/// the old process. Call only where PowerPoint is wanted anyway (new Application() starts it). Returns true
		/// when it attached again; attachFailed is true when it had to attach again and could not (the old
		/// reference is kept).
		/// </summary>
		public bool EnsureAttached(out bool attachFailed)
		{
			attachFailed = false;
			using PowerPointGate gate = PowerPointGate.Enter(PowerPointCallWeight.Light, "check the PowerPoint reference");
			if (gate == null)
			{
				// Not asked: the reference is kept, as for a busy PowerPoint.
				return false;
			}
			Application app = prePowerPointApp;
			if (app != null && !app.IsDisposed)
			{
				try
				{
					_ = app.Version;
					return false;
				}
				catch (Exception ex)
				{
					if (IsPowerPointBusy(ex))
					{
						return false;
					}
					Console.WriteLine($"PowerPoint reference no longer answers: {ex.Message}");
				}
			}
			ForgetRunPresentation();
			Application before = prePowerPointApp;
			NewApp();
			bool attached = !ReferenceEquals(before, prePowerPointApp);
			attachFailed = !attached;
			return attached;
		}

		/// <summary>
		/// Makes this wrapper act on the slide show another wrapper ran for the same presentation, when this
		/// wrapper is not running that presentation itself. In a dual monitor show the main form and the show
		/// window each run the item and drive the same slide show; at show start only the show window runs the
		/// item that is already live, and the main form's slide keys must still move it. Nothing changes when
		/// this wrapper already runs that presentation, or when the other wrapper does not (or its show is gone).
		/// </summary>
		public void FollowRunOf(PowerPoint other, string InPowerpointFullPath, ShowCopyList PowerpointList)
		{
			if (other == null || ReferenceEquals(other, this) || other.RunShowLost)
			{
				return;
			}
			string otherName = other.CurrentRunPresentationName;
			if (otherName == null)
			{
				return;
			}
			string name = GetPresentationName(InPowerpointFullPath, PowerpointList);
			if (!string.Equals(name, otherName, StringComparison.OrdinalIgnoreCase))
			{
				return;
			}
			if (string.Equals(CurrentRunPresentationName, otherName, StringComparison.OrdinalIgnoreCase) && !RunShowLost)
			{
				return;
			}
			runPresentationName = otherName;
			runPresentationIndex = other.runPresentationIndex;
			runPresentationGeneration = other.runPresentationGeneration;
			runPresentationMark = other.runPresentationMark;
			// Verified through the other wrapper's proxy: this wrapper reads the mark once on its first lookup.
			runMarkVerifiedApp = null;
			RunShowLost = false;
		}

		/// <summary>
		/// The presentation of this wrapper's run, with the proxies it was read through. Disposing it releases
		/// every proxy obtained through them (NetOffice releases child proxies with their parent).
		/// </summary>
		private sealed class RunPresentation : IDisposable
		{
			private readonly List<IDisposable> owned = new List<IDisposable>();

			public RunPresentation(Presentations collection, Presentation presentation)
			{
				owned.Add(collection);
				Presentation = presentation;
			}

			public Presentation Presentation { get; }

			public void Own(IDisposable proxy)
			{
				owned.Add(proxy);
			}

			public void Dispose()
			{
				for (int i = owned.Count - 1; i >= 0; i--)
				{
					try
					{
						// Kept instead of released when the group was given up (a release would wait).
						ReleaseProxy(owned[i]);
					}
					catch (Exception ex)
					{
						Console.WriteLine($"Error releasing a PowerPoint proxy: {ex.Message}");
					}
				}
				owned.Clear();
			}
		}

		/// <summary>
		/// The run's slide show could not be looked up: PowerPoint answered with an error that does not say
		/// whether the slide show still runs (busy, a server fault, a failed call). Nothing is concluded from it.
		/// </summary>
		private sealed class RunLookupException : Exception
		{
			public RunLookupException(string message, Exception inner) : base(message, inner)
			{
			}
		}

		private const int RPC_E_CALL_REJECTED = unchecked((int)0x80010001);
		private const int RPC_E_SERVERCALL_RETRYLATER = unchecked((int)0x8001010A);
		private const int RPC_E_DISCONNECTED = unchecked((int)0x80010108);
		private const int RPC_S_SERVER_UNAVAILABLE = unchecked((int)0x800706BA);

		// PowerPoint is busy (e.g. a modal dialog is open in it): the call failed, but PowerPoint is there.
		private static bool IsPowerPointBusy(Exception ex)
		{
			return HasHResult(ex, RPC_E_CALL_REJECTED) || HasHResult(ex, RPC_E_SERVERCALL_RETRYLATER);
		}

		// The PowerPoint process is gone (closed, crashed): its slide shows are gone with it.
		private static bool IsPowerPointGone(Exception ex)
		{
			return HasHResult(ex, RPC_E_DISCONNECTED) || HasHResult(ex, RPC_S_SERVER_UNAVAILABLE);
		}

		private static bool HasHResult(Exception ex, int hresult)
		{
			for (Exception e = ex; e != null; e = e.InnerException)
			{
				if (e.HResult == hresult)
				{
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Finds the presentation of this wrapper's run by name and mark (the remembered index first; the mark is
		/// read only when the proxy changed since it was verified or on a rescan, see runMarkVerifiedApp). Three
		/// outcomes:
		/// FOUND: returns it (also when other presentations could not be read).
		/// GONE: returns null. Only when positively established: no run is remembered; PowerPoint is gone (the
		/// Application proxy is null or disposed, or a call failed with RPC_E_DISCONNECTED or
		/// RPC_S_SERVER_UNAVAILABLE); or every presentation in the collection was read without an error and none
		/// has the run's name and mark (a presentation with the name but without the mark is not the run's).
		/// UNKNOWN: throws RunLookupException: the collection could not be read, PowerPoint is busy, or the run's
		/// presentation was not found while at least one other presentation could not be read (it may have
		/// been the run's).
		/// </summary>
		private RunPresentation FindRunPresentation()
		{
			string name = CurrentRunPresentationName;
			Application app = prePowerPointApp;
			if (name == null || app == null || app.IsDisposed)
			{
				return null;
			}
			Presentations presentations = null;
			Exception unreadable = null;
			try
			{
				presentations = app.Presentations;
				int count = presentations.Count;
				int hint = runPresentationIndex;
				if (hint >= 1 && hint <= count)
				{
					Presentation candidate = presentations[hint];
					if (string.Equals(ReadCandidateName(() => candidate.FullName, ref unreadable), name, StringComparison.OrdinalIgnoreCase))
					{
						// Same proxy as the last verification: the same presentation (see runMarkVerifiedApp).
						if (ReferenceEquals(runMarkVerifiedApp, app))
						{
							return new RunPresentation(presentations, candidate);
						}
						if (IsMarkedCandidate(candidate, ref unreadable))
						{
							runMarkVerifiedApp = app;
							return new RunPresentation(presentations, candidate);
						}
					}
				}
				for (int i = 1; i <= count; i++)
				{
					if (i == hint)
					{
						continue;
					}
					if (PowerPointCallLimit.CurrentAbandoned)
					{
						// Given up: not known (UNKNOWN), and no further call.
						throw new InvalidOperationException("The PowerPoint calls were given up (PowerPoint is not responding)");
					}
					Presentation candidate = presentations[i];
					if (string.Equals(ReadCandidateName(() => candidate.FullName, ref unreadable), name, StringComparison.OrdinalIgnoreCase)
						&& IsMarkedCandidate(candidate, ref unreadable))
					{
						runPresentationIndex = i;
						runMarkVerifiedApp = app;
						return new RunPresentation(presentations, candidate);
					}
				}
			}
			catch (Exception ex)
			{
				DisposeQuietly(presentations);
				if (IsPowerPointGone(ex))
				{
					Console.WriteLine($"PowerPoint is gone: {ex.Message}");
					return null;
				}
				throw new RunLookupException("The PowerPoint presentation could not be looked up: " + ex.GetBaseException().Message, ex);
			}
			DisposeQuietly(presentations);
			if (unreadable != null)
			{
				// Not found, but not every presentation could be read.
				throw new RunLookupException("The PowerPoint presentation could not be looked up: " + unreadable.GetBaseException().Message, unreadable);
			}
			// Every presentation was read and none is the run's.
			return null;
		}

		/// <summary>
		/// Reads one candidate's name during a scan. A candidate that cannot be read is skipped (null) and
		/// remembered in unreadable, so the scan goes on to the others. PowerPoint gone or busy is not a
		/// candidate's problem: rethrown, so the scan ends (GONE, or UNKNOWN without scanning a busy PowerPoint).
		/// </summary>
		private static string ReadCandidateName(Func<string> read, ref Exception unreadable)
		{
			try
			{
				return read();
			}
			catch (Exception ex)
			{
				if (IsPowerPointGone(ex) || IsPowerPointBusy(ex))
				{
					throw;
				}
				unreadable ??= ex;
				return null;
			}
		}

		/// <summary>
		/// During a scan: true when the candidate (which has the run's name) carries the run's mark. A candidate
		/// whose mark cannot be read counts as unreadable, like ReadCandidateName (PowerPoint gone or busy is
		/// rethrown). False with a readable tag means another presentation got the name: not the run's.
		/// </summary>
		private bool IsMarkedCandidate(Presentation candidate, ref Exception unreadable)
		{
			string mark = runPresentationMark;
			if (mark == null)
			{
				return false;
			}
			try
			{
				return HasShowCopyMark(candidate, mark);
			}
			catch (Exception ex)
			{
				if (IsPowerPointGone(ex) || IsPowerPointBusy(ex))
				{
					throw;
				}
				unreadable ??= ex;
				return false;
			}
		}

		private static void DisposeQuietly(IDisposable proxy)
		{
			try
			{
				ReleaseProxy(proxy);
			}
			catch
			{
			}
		}

		/// <summary>
		/// FindRunPresentation, and records the result: GONE sets LastCallNotRunning (and RunShowLost when a run
		/// is remembered); FOUND clears LastCallNotRunning but leaves RunShowLost (finding the presentation says
		/// nothing about its slide show); UNKNOWN throws RunLookupException with LastCallNotRunning false.
		/// </summary>
		private RunPresentation FindRunPresentationNoted()
		{
			LastCallNotRunning = false;
			RunPresentation run = FindRunPresentation();
			if (run == null)
			{
				NoteRunShow(false);
			}
			return run;
		}

		/// <summary>
		/// The presentation of this wrapper's run and its slide show window. Three outcomes:
		/// FOUND: returns them; LastCallNotRunning and RunShowLost are cleared.
		/// GONE: returns null; LastCallNotRunning is set, and RunShowLost when a run is remembered. Only when
		/// positively established: the presentation is GONE (see FindRunPresentation), or
		/// Application.SlideShowWindows was read without an error and none of its windows belongs to the run's
		/// presentation (compared by Presentation.FullName). The error PowerPoint raises for
		/// Presentation.SlideShowWindow is not used to decide: it only sends the lookup to that enumeration.
		/// UNKNOWN: throws RunLookupException with LastCallNotRunning false; RunShowLost is unchanged.
		/// The window proxy is released with the returned object.
		/// </summary>
		private RunPresentation FindRunSlideShow(out SlideShowWindow window)
		{
			LastCallNotRunning = false;
			window = null;
			RunPresentation run = FindRunPresentation();
			if (run != null)
			{
				try
				{
					window = run.Presentation.SlideShowWindow;
				}
				catch (Exception first)
				{
					try
					{
						window = FindSlideShowWindowOfRun(run, first);
					}
					catch
					{
						run.Dispose();
						throw;
					}
				}
				if (window == null)
				{
					run.Dispose();
					run = null;
				}
			}
			NoteRunShow(run != null);
			return run;
		}

		/// <summary>
		/// Reads Application.SlideShowWindows: the window that belongs to the run's presentation (also when other
		/// windows could not be read), or null when every window was read without an error and none does (or
		/// PowerPoint is gone). Throws RunLookupException when the collection could not be read, PowerPoint is
		/// busy, or the run's window was not found while at least one window could not be read. The proxies are
		/// owned by run.
		/// </summary>
		private SlideShowWindow FindSlideShowWindowOfRun(RunPresentation run, Exception first)
		{
			string name = CurrentRunPresentationName;
			Exception unreadable = null;
			try
			{
				SlideShowWindows windows = prePowerPointApp.SlideShowWindows;
				run.Own(windows);
				int count = windows.Count;
				for (int i = 1; i <= count; i++)
				{
					SlideShowWindow candidate = windows[i];
					if (string.Equals(ReadCandidateName(() => candidate.Presentation.FullName, ref unreadable), name, StringComparison.OrdinalIgnoreCase))
					{
						return candidate;
					}
				}
				if (unreadable == null)
				{
					// Every slide show window was read and none is the run's.
					return null;
				}
			}
			catch (Exception ex)
			{
				if (IsPowerPointGone(ex))
				{
					return null;
				}
				throw new RunLookupException("The PowerPoint slide show could not be looked up: " + first.GetBaseException().Message + " / " + ex.GetBaseException().Message, ex);
			}
			// Not found, but not every slide show window could be read.
			throw new RunLookupException("The PowerPoint slide show could not be looked up: " + first.GetBaseException().Message + " / " + unreadable.GetBaseException().Message, unreadable);
		}

		private void NoteRunShow(bool running)
		{
			LastCallNotRunning = !running;
			if (CurrentRunPresentationName != null)
			{
				RunShowLost = !running;
			}
		}

		public void First()
		{
			using PowerPointGate gate = PowerPointGate.Enter(PowerPointCallWeight.Light, "go to the first slide");
			if (gate == null)
			{
				LastCallNotRunning = false;
				return;
			}
			try
			{
				using RunPresentation run = FindRunSlideShow(out SlideShowWindow window);
				if (run == null)
				{
					return;
				}
				window.View.First();
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error going to first slide: {ex.Message}");
			}
		}

		public void Last()
		{
			using PowerPointGate gate = PowerPointGate.Enter(PowerPointCallWeight.Light, "go to the last slide");
			if (gate == null)
			{
				LastCallNotRunning = false;
				return;
			}
			try
			{
				using RunPresentation run = FindRunSlideShow(out SlideShowWindow window);
				if (run == null)
				{
					return;
				}
				window.View.Last();
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
				LastCallNotRunning = false;
				return;
			}

			using PowerPointGate gate = PowerPointGate.Enter(PowerPointCallWeight.Light, "go to slide " + StartingSlide);
			if (gate == null)
			{
				LastCallNotRunning = false;
				return;
			}
			try
			{
				using RunPresentation run = FindRunSlideShow(out SlideShowWindow window);
				if (run == null)
				{
					return;
				}
				window.View.GotoSlide(StartingSlide);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error going to slide {StartingSlide}: {ex.Message}");
			}
		}

		/// <summary>
		/// The slide count of the presentation this wrapper ran; 0 when there is none (or it cannot be read).
		/// </summary>
		public int Count()
		{
			using PowerPointGate gate = PowerPointGate.Enter(PowerPointCallWeight.Light, "read the slide count");
			if (gate == null)
			{
				LastCallNotRunning = false;
				return 0;
			}
			try
			{
				using RunPresentation run = FindRunPresentationNoted();
				if (run == null)
				{
					return 0;
				}
				return run.Presentation.Slides.Count;
			}
			catch (Exception)
			{
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
			using PowerPointGate gate = PowerPointGate.Enter(PowerPointCallWeight.Light, "place the slide show window");
			if (gate == null)
			{
				return;
			}
            try
            {
				using RunPresentation run = FindRunSlideShow(out SlideShowWindow window);
				if (run != null)
				{
					window.Left = Left;
					window.Top = Top;
					window.Width = Width;
					window.Height = Height;
				}
            }
            catch (Exception)
            {
				// Ignored: the window stays where PowerPoint put it.
            }
			// Moving the window can change its place in the z-order and its monitor (custom output area): raise
			// the window found by Run again. This is the item's last raise; SlideShowRaiseError is its result.
			if (KeepSlideShowOnTop)
			{
				RaiseCurrentSlideShowWindow();
			}
        }

		/// <summary>
		/// Shrinks the slide show windows of the presentations this app opened for the show (PowerpointList) to
		/// one pixel at the primary monitor's corner (live camera / video items). Other presentations in the same
		/// PowerPoint may be the user's own (also one with a listed name but without the copy's mark); their slide
		/// shows are left alone.
		/// </summary>
        public void ResSetAllShowWindows(ShowCopyList PowerpointList)
		{
			using PowerPointGate gate = PowerPointGate.Enter(PowerPointCallWeight.Light, "shrink the slide show windows");
			if (gate == null)
			{
				return;
			}
			Presentations presentations = null;
			try
			{
				presentations = prePowerPointApp.Presentations;
				int count = presentations.Count;
				for (int i = 1; i <= count && !PowerPointCallLimit.CurrentAbandoned; i++)
				{
					PowerPointCallLimit.Progress();
					try
					{
						Presentation presentation = presentations[i];
						if (GetShowCopyPath(presentation, presentation.FullName, PowerpointList) == null)
						{
							continue;
						}
						// Throws when this presentation has no slide show running: nothing to shrink.
						SlideShowWindow window = presentation.SlideShowWindow;
						window.Left = 0f;
						window.Top = 0f;
						window.Width = 1f;
						window.Height = 1f;
					}
					catch (Exception)
					{
						// Ignored: this presentation has no slide show running, or PowerPoint refused the change.
					}
				}
			}
			catch (Exception)
            {
				// Ignored: PowerPoint is not available.
            }
			finally
			{
				ReleaseProxy(presentations);
			}
		}

		/// <summary>
		/// daniel
		/// �� ���α׷����� �����?�Ŀ�����Ʈ�� ����
		/// �ٸ� �۾� ������ �� �Ŀ�����Ʈ ������ ���� ����
		/// </summary>
		/// <param name="PowerpointList"></param>
		/// <returns></returns>
		public string ClearUpPowerpointWindows(ref ShowCopyList PowerpointList)
		{
			return ClearUpPowerpointWindows(ref PowerpointList, quitWhenEmpty: true);
		}

		/// <summary>
		/// As above; quitWhenEmpty false leaves PowerPoint running and this wrapper attached after the copies are
		/// closed (show start: the pre-load attaches to PowerPoint right after, it must not meet an exiting one).
		/// Each completed read and Close is reported to the show-end time limit (PowerPointCallLimit.Progress).
		/// </summary>
		public string ClearUpPowerpointWindows(ref ShowCopyList PowerpointList, bool quitWhenEmpty)
		{
			LastCloseFailedCount = 0;
			LastCloseFailedName = null;
			LastCloseError = null;
			if (PowerpointList.Count < 1)
			{
				return "";
			}

			int num = PowerpointList.Count;
            int num2 = 0;
			int num3 = 0;
			int unreadable = 0;
			string firstUnreadableError = null;
			bool flag = false;
			// The presentations of this show are closed now: no wrapper may act on a name it remembered from it.
			ForgetShowCopyNames();
			ForgetRunPresentation();
			// Every proxy read through the collection is released with it at the end (ReleaseProxy: kept instead
			// when the group's time limit ran out).
			Presentations presentations = null;
			try
			{
				Application app = prePowerPointApp;
				if (app != null && !app.IsDisposed)
                {
					presentations = app.Presentations;
					num2 = presentations.Count;
					PowerPointCallLimit.Progress();
					for (int num4 = num2; num4 > 0; num4--)
					{
						if (PowerPointCallLimit.Current?.Expired() == true)
						{
							break;
						}
						// Close only the copies this app opened for the show: a listed name AND the copy's mark.
						// PowerPoint is single-instance, so other presentations in it may be the user's own, also
						// one that got a listed "PresentationN" name after a restart (Close does not ask to save).
						Presentation openPresentation;
						string fullName;
						try
						{
							openPresentation = presentations[num4];
							fullName = openPresentation.FullName;
							string copyPath = GetShowCopyPath(openPresentation, fullName, PowerpointList);
							PowerPointCallLimit.Progress();
							if (copyPath == null)
							{
								continue;
							}
						}
						catch (Exception ex)
						{
							if (PowerPointCallLimit.CurrentAbandoned)
							{
								// Cancelled by the time limit: no further call.
								Console.WriteLine($"Show end: reading presentation {num4} was cancelled: {ex.GetBaseException().Message}");
								break;
							}
							// Not known whether it is one of the copies: left open, reported below.
							unreadable++;
							firstUnreadableError ??= ex.GetBaseException().Message;
							Console.WriteLine($"Error reading presentation {num4} at show end: {ex.Message}");
							continue;
						}
						try
						{
							openPresentation.Close();
							num3++;
							PowerPointCallLimit.Progress();
						}
						catch (Exception ex)
						{
							if (PowerPointCallLimit.CurrentAbandoned)
							{
								Console.WriteLine($"Show end: closing '{fullName}' was cancelled: {ex.GetBaseException().Message}");
								break;
							}
							LastCloseFailedCount++;
							LastCloseFailedName ??= fullName;
							Console.WriteLine($"Error closing presentation '{fullName}': {ex.Message}");
						}
					}
				}
			}
			catch (Exception ex)
			{
				flag = true;
				LastCloseError = "the open presentations could not be read: " + ex.GetBaseException().Message;
			}
			finally
			{
				ReleaseProxy(presentations);
			}
			if (PowerPointCallLimit.CurrentAbandoned)
			{
				// PowerPoint stopped answering: nothing more is asked of it, the list stays for a later attempt, and
				// this wrapper's proxy is not used again.
				// The caller reads PowerPointCallLimit.CurrentAbandoned; the text is for its log.
				DropPowerPointApp();
				return "Abandoned: Opened PP=" + num2 + ", Total On List=" + num + ", Numbers Closed=" + num3;
			}
			if (unreadable > 0 && LastCloseError == null)
			{
				LastCloseError = unreadable + " open presentation(s) could not be read: " + firstUnreadableError;
			}
			PowerpointList.Clear();

			//daniel PowerPoint App ����
			if (quitWhenEmpty)
			{
				QuitPowerPointApp(prePowerPointApp);
			}

			return "Opened PP=" + num2 + ", Total On List=" + num + ", Numbers Closed=" + num3 + (flag ? ". Error When Closing" : ". No Error");
		}

		/// <summary>
		/// Set by the last ClearUpPowerpointWindows: how many of the show's copies (listed name and mark) failed
		/// to close, and the first one's name. Their slide show windows were already taken off the top.
		/// </summary>
		public int LastCloseFailedCount { get; private set; }

		/// <inheritdoc cref="LastCloseFailedCount"/>
		public string LastCloseFailedName { get; private set; }

		/// <summary>
		/// Set by the last ClearUpPowerpointWindows when it could not tell whether every copy was closed (the
		/// presentations, or some of them, could not be read); null otherwise.
		/// </summary>
		public string LastCloseError { get; private set; }

		/// <summary>
		/// Returns the number of presentations open in the given PowerPoint, or -1 when it cannot be read.
		/// </summary>
		private static int GetOpenPresentationCount(Application app)
		{
			try
			{
				int count = app.Presentations.Count;
				PowerPointCallLimit.Progress();
				return count;
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
			ForgetRunPresentation();
			ReleaseApplicationReference();
		}

		// Proxies that were not released because PowerPoint did not answer (see ReleaseProxy). Kept referenced, so
		// the garbage collector does not release them either (that would make the UI thread wait for PowerPoint).
		private static readonly List<IDisposable> heldProxies = new List<IDisposable>();

		/// <summary>How many proxies wait to be released (ReleaseHeldProxies) after PowerPoint did not answer.</summary>
		public static int HeldProxyCount => heldProxies.Count;

		/// <summary>
		/// Releases a proxy, unless the current show-end / exit group of calls was abandoned (PowerPoint does not
		/// answer): releasing a proxy is a call to PowerPoint that waits for it and is not cancelled by the group's
		/// message filter (measured with a suspended PowerPoint: the release returned only when PowerPoint was
		/// resumed). The proxy is then kept, unused, until ReleaseHeldProxies after PowerPoint answers again.
		/// </summary>
		private static void ReleaseProxy(IDisposable proxy)
		{
			if (proxy == null)
			{
				return;
			}
			if (PowerPointCallLimit.CurrentAbandoned)
			{
				heldProxies.Add(proxy);
				return;
			}
			try
			{
				proxy.Dispose();
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error releasing a PowerPoint proxy: {ex.Message}");
			}
		}

		/// <summary>
		/// Releases the proxies kept by ReleaseProxy and DropPowerPointApp. Call only after PowerPoint answered
		/// (ProbePowerPoint), or when it is not running (a release then fails at once).
		/// </summary>
		public static void ReleaseHeldProxies()
		{
			IDisposable[] proxies = heldProxies.ToArray();
			heldProxies.Clear();
			// In the order they were kept: a collection before the Application it was read through.
			for (int i = 0; i < proxies.Length; i++)
			{
				try
				{
					proxies[i].Dispose();
				}
				catch (Exception ex)
				{
					Console.WriteLine($"Error releasing a held PowerPoint proxy: {ex.Message}");
				}
			}
		}

		/// <summary>
		/// PowerPoint did not answer at show end or app exit: this wrapper stops using its Application proxy (it is
		/// kept unreleased, see ReleaseProxy) and forgets its run. The next use attaches again (NewApp).
		/// </summary>
		public void DropPowerPointApp()
		{
			ForgetRunPresentation();
			Application app = prePowerPointApp;
			prePowerPointApp = null;
			if (app != null && !app.IsDisposed)
			{
				heldProxies.Add(app);
			}
		}

		/// <summary>
		/// Releases the Application proxy only. The remembered run presentation stays: NewApp attaches again to
		/// the same PowerPoint, where the presentation keeps its name (NewApp runs at every pre-load, also during
		/// a show, while an item's slide show is on the projector).
		/// </summary>
		private void ReleaseApplicationReference()
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
			using PowerPointGate gate = PowerPointGate.Enter(PowerPointCallWeight.Heavy, "attach to PowerPoint");
			if (gate == null)
			{
				// Not attached: the old reference is kept, as when attaching fails.
				return;
			}
			try
			{
				// Create the new reference first so a failure keeps the old, working one.
				// Then release the old reference instead of orphaning it, so shutdown can release every reference.
				Application newApp = new Application();
				ReleaseApplicationReference();
				prePowerPointApp = newApp;
			}
			catch (Exception)
            {
				// 예외 무시
            }
		}

        private DateTime lastClickTime = DateTime.MinValue;

        /// <summary>
        /// Plays the next animation step of the slide show of this wrapper's run. Does nothing when that slide
        /// show is not running (LastCallNotRunning). Never throws.
        /// </summary>
        public void SafePlayNext(int slideNumber = -1)
        {
            using PowerPointGate gate = PowerPointGate.Enter(PowerPointCallWeight.Light, "play the next animation");
            if (gate == null)
            {
                return;
            }
            try
            {
                // Inside the try: reading PowerPoint can throw (e.g. PowerPoint has exited).
                using RunPresentation run = FindRunSlideShow(out SlideShowWindow slideShowWindow);
                if (run == null) return;

                // 1. Debounce (kept).
                if ((DateTime.Now - lastClickTime).TotalMilliseconds < 500) return;
                lastClickTime = DateTime.Now;

                var view = slideShowWindow.View;

                // [핵심 수정 사항] 
                // 입력된 번호가 유효하고(>0), '현재 보고 있는 슬라이드'와 '다를 때만' 이동합니다.
                // 같을 때 이동하면 애니메이션이 리셋되기 때문입니다.
                if (slideNumber > 0 && view.Slide.SlideIndex != slideNumber)
                {
                    // 다른 슬라이드로 이동할 때만 실행
                    view.GotoSlide(slideNumber, MsoTriState.msoTrue);
                }

                slideShowWindow.Activate();

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

		/// <summary>
		/// Moves the slide show of this wrapper's run (see FindRunSlideShow) and returns the slide now shown, or
		/// -1 at the end of the show (also put in InCurSlide).
		/// - No PowerPoint reference (prePowerPointApp null): returns 1, InCurSlide unchanged, LastCallNotRunning.
		/// - Slide show not running (not started, ended before this call): nothing is moved, InCurSlide is
		///   returned unchanged, LastCallNotRunning.
		/// - The slide show ended on this move (Next on the last slide ends it when PowerPoint's "End with black
		///   slide" option is off, and the View calls after it fail): treated like the natural end, -1, with
		///   LastCallNotRunning and RunShowLost; not an error.
		/// - Any other error is not thrown: it is kept in LastNavigationError for the caller to log and report,
		///   InCurSlide is returned unchanged, LastCallNotRunning is false.
		/// </summary>
		public int ImplementPowerpointSlideMovement(ref int InCurSlide, int InCurMaxSlide, OfficeLibKeys InKey, int InSlideNo)
		{
			LastNavigationError = null;
			if (prePowerPointApp == null)
			{
				LastCallNotRunning = true;
				return 1;
			}
			using PowerPointGate gate = PowerPointGate.Enter(PowerPointCallWeight.Light, "move the slide show");
			if (gate == null)
			{
				// Not asked: nothing moved, the slide show is not "ended" (the screen stays as it is); the caller
				// reports the error.
				LastNavigationError = new InvalidOperationException("PowerPoint is not responding");
				LastCallNotRunning = false;
				return InCurSlide;
			}
			try
			{

                // The slide show of the presentation this wrapper ran (slide show running), never the active one.
                using RunPresentation run = FindRunSlideShow(out SlideShowWindow slideShowWindow);
                if (run == null)
                {
                    // Not running: the current slide number is returned unchanged.
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
			catch (Exception ex)
			{
				Console.WriteLine($"Error moving the slide show: {ex.Message}");
				// A failed lookup (UNKNOWN) is not looked up again: nothing would be learnt; the next key tries again.
				if (PowerPointCallLimit.CurrentAbandoned)
				{
					// Given up: PowerPoint did not answer. Not an end of the slide show; nothing more is asked.
					Console.WriteLine($"Moving the slide show was given up: {ex.GetBaseException().Message}");
					LastNavigationError = new InvalidOperationException("PowerPoint is not responding");
					LastCallNotRunning = false;
					return InCurSlide;
				}
				if (!(ex is RunLookupException) && RunSlideShowEnded())
				{
					// The slide show ended on this move: the end of the item, as when PowerPoint reports
					// ppSlideShowDone above (-1). LastCallNotRunning and RunShowLost were set by the lookup.
					InCurSlide = -1;
					return InCurSlide;
				}
				// UNKNOWN, or the slide show still runs: logged and reported by the caller. The item on the output
				// stays as it is.
				LastNavigationError = ex;
				LastCallNotRunning = false;
            }
			return InCurSlide;
		}

		/// <summary>
		/// After an error while moving: looks the run's slide show up again (FindRunSlideShow). True only for
		/// GONE, which is positively established (the presentation or every slide show window was read and none
		/// is the run's, or PowerPoint is gone); LastCallNotRunning and RunShowLost are then set. False for FOUND
		/// (the slide show still runs: the error was the move's own) and for UNKNOWN (the lookup failed too;
		/// nothing is recorded).
		/// </summary>
		private bool RunSlideShowEnded()
		{
			try
			{
				using RunPresentation run = FindRunSlideShow(out _);
				return run == null;
			}
			catch (Exception)
			{
				return false;
			}
		}

		/// <summary>
		/// Reads the verse markers from the slide notes of the presentation this wrapper ran. Nothing is read
		/// when there is none (the caller has reset the verse data).
		/// </summary>
        public void LoadVersesAndSlides(ref int[] SongVerses, ref int[,] Slide, string[] SequenceSymbol)
		{
			using PowerPointGate gate = PowerPointGate.Enter(PowerPointCallWeight.Light, "read the verse markers");
			if (gate == null)
			{
				return;
			}
			try
			{
				using RunPresentation run = FindRunPresentationNoted();
				if (run == null)
				{
					return;
				}
				Presentation presentation = run.Presentation;
				int num = -1;
				bool flag = false;
				SequenceSymbol.GetUpperBound(0);
				for (int i = 1; i <= presentation.Slides.Count && !PowerPointCallLimit.CurrentAbandoned; i++)
				{
					PowerPointCallLimit.Progress();
					try
					{
						if (presentation.Slides[i].NotesPage.Shapes.Placeholders[NOTES_PLACEHOLDER_INDEX].PlaceholderFormat.Type == PpPlaceholderType.ppPlaceholderBody &&
							presentation.Slides[i].NotesPage.Shapes.Placeholders[NOTES_PLACEHOLDER_INDEX].TextFrame.TextRange.Text.Length == 1)
						{
							num = GetVerseIndicator(presentation.Slides[i].NotesPage.Shapes.Placeholders[NOTES_PLACEHOLDER_INDEX].TextFrame.TextRange.Text);
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
		/// Returns false when a slide could not be read (its marker is then missing): such a result must not be
		/// cached. A slide whose notes page has no body placeholder is a slide without a marker, not an error:
		/// it is recognised by the placeholder count (reading the missing placeholder fails with the same generic
		/// "integer out of range" error as other bad reads, so the error itself cannot tell them apart).
		/// </summary>
		private bool ParseVersesFromPresentation(_Presentation presentation, int totalSlides, ref int[] SongVerses, ref int[,] Slide, string[] SequenceSymbol)
		{
			bool flag = false;
			bool complete = true;
			SequenceSymbol.GetUpperBound(0);
			for (int i = 1; i <= totalSlides; i++)
			{
				if (PowerPointCallLimit.CurrentAbandoned)
				{
					return false;
				}
				PowerPointCallLimit.Progress();
				try
				{
					Placeholders placeholders = presentation.Slides[i].NotesPage.Shapes.Placeholders;
					if (placeholders.Count < NOTES_PLACEHOLDER_INDEX)
					{
						continue;
					}
					Shape notesBody = placeholders[NOTES_PLACEHOLDER_INDEX];
					string notesText;
					if (notesBody.PlaceholderFormat.Type == PpPlaceholderType.ppPlaceholderBody &&
						(notesText = notesBody.TextFrame.TextRange.Text).Length == 1)
					{
						int verseIndicator = GetVerseIndicator(notesText);
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
					complete = false;
					Console.WriteLine($"Error processing verse for slide {i}: {ex.Message}");
				}
			}
			return complete;
		}

		/// <summary>
		/// The file's last write time, read before it is opened for parsing: a save while it is parsed then
		/// leaves the cache entry older than the file, so the next check parses it again. Null when it cannot be
		/// read (nothing is cached then).
		/// </summary>
		private static DateTime? GetLastWriteBeforeRead(string FilePath)
		{
			try
			{
				return (!string.IsNullOrEmpty(FilePath) && File.Exists(FilePath)) ? File.GetLastWriteTimeUtc(FilePath) : (DateTime?)null;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error reading the last write time of '{FilePath}': {ex.Message}");
				return null;
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
				using PowerPointGate gate = PowerPointGate.Enter(PowerPointCallWeight.Heavy, "read the slide count of " + Path.GetFileName(FilePath));
				if (gate == null)
				{
					// Not asked: as when the file cannot be opened (not cached, so it is read again next time).
					return false;
				}
				_Presentation tempPresentation = presentation;
				bool shouldClose = false;
				try
				{
					if (tempPresentation == null)
					{
						tempPresentation = OpenTemporary(FilePath, MsoTriState.msoFalse, MsoTriState.msoTrue);
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
				using PowerPointGate gate = PowerPointGate.Enter(PowerPointCallWeight.Heavy, "read the slide count of " + Path.GetFileName(FilePath));
				if (gate == null)
				{
					// Not asked: as when the file cannot be opened (not cached, so it is read again next time).
					return false;
				}
				_Presentation tempPresentation = presentation;
				bool shouldClose = false;
				try
				{
					if (tempPresentation == null)
					{
						tempPresentation = OpenTemporary(FilePath, MsoTriState.msoFalse, MsoTriState.msoTrue);
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
				using PowerPointGate gate = PowerPointGate.Enter(PowerPointCallWeight.Heavy, "read the slide count of " + Path.GetFileName(FilePath));
				if (gate == null)
				{
					// Not asked: as when the file cannot be opened (not cached, so it is read again next time).
					return false;
				}
				_Presentation tempPresentation = presentation;
				bool shouldClose = false;
				try
				{
					if (tempPresentation == null)
					{
						tempPresentation = OpenTemporary(FilePath, MsoTriState.msoFalse, MsoTriState.msoTrue);
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
			using PowerPointGate gate = PowerPointGate.Enter(PowerPointCallWeight.Heavy, "build the slide images of " + Path.GetFileName(FilePath));
			if (gate == null)
			{
				ResetVerses(MAX_VERSES, DB_MAXSLIDES, ref SongVerses, ref Slide);
				return false;
			}
			try
			{
				Console.WriteLine($"[BuildScreenOutDumps] FilePath='{FilePath}', FilePrefix='{FilePrefix}'");
				ResetVerses(MAX_VERSES, DB_MAXSLIDES, ref SongVerses, ref Slide);
				if (prePowerPointApp == null)
				{
					Console.WriteLine("[BuildScreenOutDumps] prePowerPointApp is null");
					return result;
				}
				DateTime? lastWriteBeforeRead = GetLastWriteBeforeRead(FilePath);

				// ✅ 먼저 캐시 체크 (프레젠테이션을 열기 전에)
			Console.WriteLine($"[DEBUG BuildScreenOutDumps] Calling IsBuildedFileCheck...");
			bool isBuilt = IsBuildedFileCheck(FilePath, FilePrefix, ref TotalSlides);
			Console.WriteLine($"[DEBUG BuildScreenOutDumps] IsBuildedFileCheck returned: {isBuilt}");
			if (isBuilt)
			{
				Console.WriteLine($"Output preview images already exist (cache hit) for: {FilePrefix}");
					
					// 캐시 히트: Verse 정보 파싱을 위해서만 프레젠테이션 열기
					presentation = OpenTemporary(FilePath, MsoTriState.msoFalse, MsoTriState.msoTrue);
					Console.WriteLine(FilePath);
					if (presentation != null)
					{
						bool complete = ParseVersesFromPresentation(presentation, TotalSlides, ref SongVerses, ref Slide, SequenceSymbol);
						StoreCachedVerses(FilePath, lastWriteBeforeRead, complete, TotalSlides, MAX_VERSES, SongVerses, Slide);
					}
					return result;
				}

				// 캐시 미스: 이미지를 새로 생성해야 함
				presentation = OpenTemporary(FilePath, MsoTriState.msoFalse, MsoTriState.msoFalse);
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
					PowerPointCallLimit.Progress();
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
				bool versesComplete = ParseVersesFromPresentation(presentation, TotalSlides, ref SongVerses, ref Slide, SequenceSymbol);
				StoreCachedVerses(FilePath, lastWriteBeforeRead, versesComplete, TotalSlides, MAX_VERSES, SongVerses, Slide);

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

		/// <summary>
		/// Caches the parsed verse data under the file's last write time read BEFORE the file was opened
		/// (lastWriteBeforeRead), and only when every slide was read (complete).
		/// </summary>
		private static void StoreCachedVerses(string FilePath, DateTime? lastWriteBeforeRead, bool complete, int totalSlides, int MAX_VERSES, int[] SongVerses, int[,] Slide)
		{
			try
			{
				if (string.IsNullOrEmpty(FilePath) || lastWriteBeforeRead == null || !File.Exists(FilePath))
				{
					return;
				}
				if (!complete)
				{
					// A slide could not be read: its marker may be missing. Read again next time.
					_verseCache.Remove(FilePath);
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
				_verseCache[FilePath] = (lastWriteBeforeRead.Value, verses, types);
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
				DateTime? lastWriteBeforeRead = GetLastWriteBeforeRead(FilePath);
				using PowerPointGate gate = PowerPointGate.Enter(PowerPointCallWeight.Heavy, "read the verse markers of " + Path.GetFileName(FilePath));
				if (gate == null)
				{
					return false;
				}
				try
				{
					versePresentation = OpenTemporary(FilePath, MsoTriState.msoTrue, MsoTriState.msoTrue);
					int slideCount = versePresentation.Slides.Count;
					bool complete = ParseVersesFromPresentation(versePresentation, slideCount, ref SongVerses, ref Slide, SequenceSymbol);
					StoreCachedVerses(FilePath, lastWriteBeforeRead, complete, slideCount, MAX_VERSES, SongVerses, Slide);
					if (PowerPointCallLimit.CurrentAbandoned)
					{
						// Given up: the markers are only partly read (and not cached).
						ResetVerses(MAX_VERSES, DB_MAXSLIDES, ref SongVerses, ref Slide);
						return false;
					}
					return true;
				}
				finally
				{
					// Inside the group: closed (or kept, when it was given up) before the gate ends.
					ClosePresentation(ref versePresentation);
				}
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
			using PowerPointGate gate = PowerPointGate.Enter(PowerPointCallWeight.Heavy, "build the slide images of " + Path.GetFileName(FilePath));
			if (gate == null)
			{
				ResetVerses(MAX_VERSES, DB_MAXSLIDES, ref SongVerses, ref Slide);
				return false;
			}
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

				DateTime? lastWriteBeforeRead = GetLastWriteBeforeRead(FilePath);
				presentation = OpenTemporary(FilePath, MsoTriState.msoFalse, MsoTriState.msoFalse);
				TotalSlides = presentation.Slides.Count;
				if ((presentation == null) & (TotalSlides > 0))
				{
					return result;
				}

				for (int k = 1; k <= TotalSlides; k++)
				{
					presentation.Slides[k].Export(FilePrefix + Convert.ToString(k) + ".jpg", "JPG", EXPORT_WIDTH, EXPORT_HEIGHT);
					PowerPointCallLimit.Progress();
				}

				// Verse 정보 파싱
				bool complete = ParseVersesFromPresentation(presentation, TotalSlides, ref SongVerses, ref Slide, SequenceSymbol);
				StoreCachedVerses(FilePath, lastWriteBeforeRead, complete, TotalSlides, MAX_VERSES, SongVerses, Slide);

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

		// The cache folder of the last BuildFirstScreenDump (see ExternalPreviewCache).
		private string firstScreenDumpFolder;

		// Pictures (one per presentation version: the name holds path, time and size) whose presentation could not be
		// opened or exported (corrupt, password-protected, no slides): not tried again until the next listing or a
		// new version of the file. UI thread only.
		private readonly HashSet<string> failedFirstScreenDumps = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

		// True while BuildOneFirstScreenDump talks to PowerPoint. A COM call pumps messages, so another thumbnail's
		// paint can call in meanwhile (inside the running group, where the gate does not stop it); it is refused and
		// retried by that thumbnail's timer. UI thread only.
		private bool firstScreenDumpRunning;

		/// <summary>
		/// Records the listed presentations (ScreenDumpFullPath[0, i]) and the cached first-slide picture of each one's
		/// current version (ScreenDumpFullPath[1, i], see ExternalPreviewCache), then prunes the cache. No PowerPoint
		/// call: the pictures are exported on demand by BuildOneFirstScreenDump, only when missing.
		/// </summary>
		public bool BuildFirstScreenDump(string[] InFileList, int MaxFileCount, string CacheFolder)
		{
			bool result = true;
			try
			{
				firstScreenDumpFolder = CacheFolder;
				// A new listing tries the presentations that could not be exported again.
				failedFirstScreenDumps.Clear();
				if (!string.IsNullOrEmpty(CacheFolder))
				{
					Directory.CreateDirectory(CacheFolder);
				}
				int count = Math.Min(Math.Min(MaxFileCount, InFileList.Length), ScreenDumpFullPath.GetLength(1));
				List<string> listed = new List<string>(count);
				for (int i = 0; i < count; i++)
				{
					ScreenDumpFullPath[0, i] = InFileList[i];
					ScreenDumpFullPath[1, i] = ExternalPreviewCache.GetPreviewPath(CacheFolder, InFileList[i]);
					listed.Add(ScreenDumpFullPath[1, i]);
				}
				ExternalPreviewCache.Prune(CacheFolder, listed, DateTime.UtcNow);
				return result;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error in BuildFirstScreenDump: {ex.Message}");
				return false;
			}
		}

		/// <summary>
		/// The cached first-slide picture path of listed presentation index (0-based), recomputed from the file's
		/// current version so an edit since the listing gets a new picture. Null when index does not hold
		/// presentationPath or the presentation cannot be read.
		/// </summary>
		public string GetFirstScreenDumpPath(int index, string presentationPath)
		{
			if (index < 0 || index >= ScreenDumpFullPath.GetLength(1)
				|| !string.Equals(ScreenDumpFullPath[0, index], presentationPath, StringComparison.OrdinalIgnoreCase))
			{
				return null;
			}
			ScreenDumpFullPath[1, index] = ExternalPreviewCache.GetPreviewPath(firstScreenDumpFolder, presentationPath);
			return ScreenDumpFullPath[1, index];
		}

		/// <summary>
		/// Deletes a cached first-slide picture (a GetFirstScreenDumpPath result) that could not be loaded, so the next
		/// BuildOneFirstScreenDump exports it again. Only a picture file in the cache folder is ever deleted.
		/// </summary>
		public bool DiscardFirstScreenDump(string previewPath)
		{
			return ExternalPreviewCache.DiscardPreview(firstScreenDumpFolder, previewPath);
		}

		/// <summary>
		/// Exports the first slide of listed presentation PPNumbering (1-based) to its cached picture, unless that
		/// picture already exists. The export goes to a temporary file that is renamed only when it is complete.
		/// Returns false without calling PowerPoint when PowerPoint is not attached, another export is running, or
		/// this version already failed since the listing.
		/// </summary>
		public bool BuildOneFirstScreenDump(int PPNumbering)
		{
			int num = PPNumbering - 1;
			if (num < 0 || num >= ScreenDumpFullPath.GetLength(1) || ScreenDumpFullPath[0, num] == null || string.IsNullOrEmpty(ScreenDumpFullPath[1, num]))
			{
				return false;
			}
			string presentationPath = ScreenDumpFullPath[0, num];
			string previewPath = ScreenDumpFullPath[1, num];
			if (File.Exists(previewPath) || ExternalPreviewCache.AdoptPendingExport(previewPath))
			{
				return true; // this version was exported before (or an earlier export finished late)
			}
			if (failedFirstScreenDumps.Contains(previewPath) || prePowerPointApp == null || firstScreenDumpRunning)
			{
				return false;
			}
			firstScreenDumpRunning = true;
			try
			{
				using PowerPointGate gate = PowerPointGate.Enter(PowerPointCallWeight.Heavy, "build the first slide image of " + Path.GetFileName(presentationPath));
				if (gate == null)
				{
					return false;
				}
				// A local presentation, never the shared field: no other flow can close or replace it meanwhile.
				_Presentation tempPresentation = null;
				Slides slides = null;
				Slide slide = null;
				string tempPath = ExternalPreviewCache.GetTempExportPath(previewPath);
				try
				{
					// Read-only, titled, windowless: the user's file is never locked for writing, and a cancelled open
					// is closed later (OpenTemporary).
					tempPresentation = OpenTemporary(presentationPath, MsoTriState.msoTrue, MsoTriState.msoFalse);
					slides = tempPresentation.Slides;
					slide = slides[1];
					slide.Export(tempPath, "JPG", EXPORT_WIDTH, EXPORT_HEIGHT);

					return ExternalPreviewCache.CommitExport(tempPath, previewPath);
				}
				catch (Exception ex)
				{
					Console.WriteLine($"Error in BuildOneFirstScreenDump: {ex.Message}");
					// A cancelled export may still be written later by PowerPoint: then the prune removes it.
					ExternalPreviewCache.DiscardTemp(tempPath);
					if (!PowerPointCallLimit.CurrentAbandoned && !IsBusyOrCancelled(ex))
					{
						// The file itself cannot be opened or exported: not again for this version.
						failedFirstScreenDumps.Add(previewPath);
					}
					return false;
				}
				finally
				{
					ReleaseProxy(slide);
					ReleaseProxy(slides);
					ClosePresentation(ref tempPresentation);
				}
			}
			finally
			{
				firstScreenDumpRunning = false;
			}
		}

		// PowerPoint was busy (a dialog, another call) or the call was cancelled: the file may be fine, so it is retried.
		private static bool IsBusyOrCancelled(Exception ex)
		{
			for (Exception current = ex; current != null; current = current.InnerException)
			{
				if (current is COMException com)
				{
					uint code = unchecked((uint)com.HResult);
					if (code == 0x80010001 || code == 0x8001010A || code == 0x80010002)
					{
						return true; // RPC_E_CALL_REJECTED, RPC_E_SERVERCALL_RETRYLATER, RPC_E_CALL_CANCELED
					}
				}
			}
			return false;
		}

        /// <summary>
        /// Opens a file as a temporary, windowless presentation (slide count, slide images, verse markers). When the
        /// open is cancelled because its group was given up and the presentation is titled (its FullName is the
        /// path), the path is remembered: the open may still complete inside PowerPoint, and that presentation is
        /// closed when PowerPoint answers again (CloseLeftOpenPresentations). An untitled one ("PresentationN")
        /// cannot be told from a presentation the user made, so it is not remembered.
        /// </summary>
        private _Presentation OpenTemporary(string path, MsoTriState readOnly, MsoTriState untitled)
        {
            try
            {
                return prePowerPointApp.Presentations.Open(path, readOnly, untitled, MsoTriState.msoFalse);
            }
            catch
            {
                if (PowerPointCallLimit.CurrentAbandoned && untitled == MsoTriState.msoFalse)
                {
                    cancelledTitledOpens.Add((this, path));
                }
                throw;
            }
        }

        // Temporary presentations (and a show copy whose Open failed) that a given-up group could not close: closed,
        // then released, when PowerPoint answers again (CloseLeftOpenPresentations). UI thread only.
        private static readonly List<_Presentation> presentationsToClose = new List<_Presentation>();

        // Titled temporary opens that were cancelled with no presentation returned (see OpenTemporary).
        private static readonly List<(PowerPoint Wrapper, string Path)> cancelledTitledOpens = new List<(PowerPoint, string)>();

        /// <summary>How many presentations wait to be closed when PowerPoint answers again.</summary>
        public static int LeftOpenCount => presentationsToClose.Count + cancelledTitledOpens.Count;

        /// <summary>
        /// PowerPoint answers again (PowerPointGate, or the show-end package after its own check): closes the
        /// temporary presentations given-up groups left open, so a hidden PowerPoint is not kept running by them
        /// (QuitPowerPointApp quits only when nothing is open). A cancelled titled open is closed only when a
        /// presentation with exactly that FullName has no window (a document the user opened has one). Call inside a
        /// limited group; whatever is not done when the group is given up stays for the next time.
        /// </summary>
        public static int CloseLeftOpenPresentations()
        {
            int closedCount = 0;
            _Presentation[] left = presentationsToClose.ToArray();
            presentationsToClose.Clear();
            for (int i = 0; i < left.Length; i++)
            {
                if (PowerPointCallLimit.CurrentAbandoned)
                {
                    presentationsToClose.Add(left[i]);
                    continue;
                }
                try
                {
                    left[i].Close();
                    closedCount++;
                    PowerPointCallLimit.Progress();
                    ReleaseProxy(left[i] as IDisposable);
                }
                catch (Exception ex)
                {
                    if (PowerPointCallLimit.CurrentAbandoned)
                    {
                        presentationsToClose.Add(left[i]);
                        continue;
                    }
                    // Gone already (closed in PowerPoint, PowerPoint exited): nothing left to close.
                    Console.WriteLine($"Closing a presentation left open: {ex.GetBaseException().Message}");
                    ReleaseProxy(left[i] as IDisposable);
                }
            }
            (PowerPoint Wrapper, string Path)[] opens = cancelledTitledOpens.ToArray();
            cancelledTitledOpens.Clear();
            foreach ((PowerPoint wrapper, string path) in opens)
            {
                Application app = wrapper.prePowerPointApp;
                if (PowerPointCallLimit.CurrentAbandoned)
                {
                    cancelledTitledOpens.Add((wrapper, path));
                    continue;
                }
                if (app == null || app.IsDisposed)
                {
                    continue;
                }
                Presentations presentations = null;
                try
                {
                    presentations = app.Presentations;
                    for (int i = presentations.Count; i >= 1 && !PowerPointCallLimit.CurrentAbandoned; i--)
                    {
                        Presentation candidate = presentations[i];
                        if (string.Equals(candidate.FullName, path, StringComparison.OrdinalIgnoreCase) && candidate.Windows.Count == 0)
                        {
                            candidate.Close();
                            closedCount++;
                            Console.WriteLine($"Closed a windowless presentation of {path} left by a cancelled open");
                        }
                        PowerPointCallLimit.Progress();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Looking for a presentation left by a cancelled open: {ex.GetBaseException().Message}");
                }
                finally
                {
                    ReleaseProxy(presentations);
                }
                if (PowerPointCallLimit.CurrentAbandoned)
                {
                    cancelledTitledOpens.Add((wrapper, path));
                }
            }
            return closedCount;
        }

        public void ClosePresentation(ref _Presentation presentation)
        {
            try
            {
                if (presentation != null)
                {
                    if (PowerPointCallLimit.CurrentAbandoned)
                    {
                        // The group was given up: not closed now (the call would be cancelled) and not released (a
                        // release would wait for PowerPoint); closed when PowerPoint answers again.
                        presentationsToClose.Add(presentation);
                        return;
                    }
                    try
                    {
                        presentation.Close();
                    }
                    catch
                    {
                        if (PowerPointCallLimit.CurrentAbandoned)
                        {
                            // Given up during the Close.
                            presentationsToClose.Add(presentation);
                            return;
                        }
                        throw;
                    }
                    ReleaseProxy(presentation as IDisposable);  // NetOffice���� �����ϴ� Dispose() ���?
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
					if (GetOpenPresentationCount(prePowerPointApp) == 0 && !PowerPointCallLimit.CurrentAbandoned)
					{
						try
						{
							prePowerPointApp.Quit();
						}
						catch (Exception e)
						{
							// Not killed: PowerPoint is one process and may hold the user's documents. Releasing
							// every reference (ShutdownPowerpoint) is what lets a hidden PowerPoint exit.
							Console.WriteLine($"Error quitting PowerPoint: {e.Message}");
						}
					}
					// Kept unreleased when the show-end group's time ran out (ReleaseProxy).
					ReleaseProxy(prePowerPointApp);  // Dispose()?? Quit() ???Ŀ? ???
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
					ForgetRunPresentation();
				}
			}
		}

		[DllImport("user32.dll")]
		private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

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




