using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using NetOffice.PowerPointApi;

namespace OfficeLib
{
	/// <summary>
	/// The slide show windows (finding, raising onto the output monitor, lowering at show end) and the probe
	/// that asks the PowerPoint process whether it answers. Win32 window calls only, apart from
	/// SlideShowWindow.HWND.
	/// </summary>
	public partial class PowerPoint
	{
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
		private const uint SWP_NOZORDER = 0x0004;
		private const uint SWP_NOACTIVATE = 0x0010;
		private const uint SWP_SHOWWINDOW = 0x0040;
		private const uint SWP_ASYNCWINDOWPOS = 0x4000;

		/// <summary>
		/// Set by the caller before <see cref="Run(string, ref ShowCopyList)"/> and
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

		public (int Left, int Top, int Width, int Height)? TargetBounds { get; set; }
		public bool PositionedOnTarget { get; private set; }

		// The slide show window of the presentation this wrapper ran last (found once per Run, raised again
		// after SetShowWindow), or zero, and the process that owned it when it was found: a handle Windows has
		// reused for another window since is never raised.
		private IntPtr currentSlideShowWindow;
		private uint currentSlideShowProcessId;

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
			var handle = FindSlideShowWindow(window, getPresentationName, out string error);
			if (handle == IntPtr.Zero) SlideShowRaiseError = error;
			return handle;
		}

		public static IntPtr FindSlideShowWindow(SlideShowWindow window, Func<string> getPresentationName, out string error)
		{
			error = null;
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
			error = "the slide show window of '" + presentationName + "' was not found" + (handleError == null ? "" : " (" + handleError + ")");
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
			if (!IsSameWindow(handle, currentSlideShowProcessId))
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
				string windowTitle = WithoutReadOnlyCaption(title.ToString());
				if (separated.IsMatch(windowTitle))
				{
					exact = handle;
					return false;
				}
				if (windowTitle.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
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

		// Microsoft 365 appends a localized status field when the presentation was opened read-only.
		internal static string WithoutReadOnlyCaption(string title)
		{
			var caption = title.AsSpan().TrimEnd();
			// The English status itself contains a hyphen. Match the whole field before looking for its delimiter.
			foreach (var status in ReadOnlyCaptions)
			{
				if (!caption.EndsWith(status, StringComparison.OrdinalIgnoreCase)) continue;
				var prefix = caption[..^status.Length];
				if (prefix.IsEmpty || !char.IsWhiteSpace(prefix[^1])) continue;
				prefix = prefix.TrimEnd();
				if (prefix.Length > 1 && prefix[^1] is '-' or '–' or '—' && char.IsWhiteSpace(prefix[^2]))
					return prefix[..^1].TrimEnd().ToString();
			}
			return title.TrimEnd();
		}

		private static readonly string[] ReadOnlyCaptions = ["읽기 전용", "Read-Only", "Read Only"];

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
			StringBuilder probeDetails = new StringBuilder();
			foreach (uint processId in processIds)
			{
				List<IntPtr> targets = frames.TryGetValue(processId, out List<IntPtr> found) ? found : new List<IntPtr>();
				if (targets.Count == 0 && others.TryGetValue(processId, out IntPtr other))
				{
					targets.Add(other);
				}
				if (probeDetails.Length > 0)
				{
					probeDetails.Append("; ");
				}
				if (targets.Count == 0)
				{
					probeDetails.Append("POWERPNT ").Append(processId).Append(": no window");
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
				probeDetails.Append("POWERPNT ").Append(processId)
					.Append(answered ? ": answered in " : (timedOut ? ": no answer in " : ": could not be asked in ")).Append(watch.ElapsedMilliseconds).Append(" ms");
				if (!answered)
				{
					probeDetails.Append(" (error ").Append(lastError).Append(')');
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
			detail = probeDetails.ToString();
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
				if (skipped || !lowered)
				{
					// PowerPoint is not responding (the lowering was posted), or the lowering failed: kept in the list,
					// lowered again by a later call (at the latest at show end).
					lock (raisedSlideShowWindowsLock)
					{
						raisedSlideShowWindows.Add((handle, processId));
					}
				}
			}
			return error;
		}
	}
}
