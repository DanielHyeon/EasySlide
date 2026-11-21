using static Axicultus.Interop.DesktopWindows;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using Axicultus.Module;
using Axicultus.Util;
using OfficeLib;

namespace Axicultus;

internal unsafe partial class Gf
{
		private const int ExecutableBufferSize = 260;
		private const int MaxPowerpointSequence = 49;
		private const int MainWindowWaitCount = 80;
		private const int MainWindowWaitDelayMs = 100;
		private const int WindowInitialSearchDelayMs = 1500;
		private const int WindowRetryDelayMs = 800;
		private const int WindowRetryCount = 5;
		private const int WindowHandleFallbackDelayMs = 1000;
		private const int WindowHandleRetryDelayMs = 500;
		private const int WindowSettleDelayMs = 300;
		private const int ForegroundDelayMs = 200;
		private const int WindowBoundsSlack = 100;
		private const int WindowTitleBufferSize = 256;
		internal static RectangleF PowerPointWindowPoints(Rectangle bounds, float systemDpi)
		{
			float pointsPerLogicalPixel = 72f / systemDpi;
			return new RectangleF(bounds.X * pointsPerLogicalPixel, bounds.Y * pointsPerLogicalPixel,
				bounds.Width * pointsPerLogicalPixel, bounds.Height * pointsPerLogicalPixel);
		}

		private static readonly string[] MediaPlayerWindowKeywords =
		[
			"windows media player",
			"media player",
			"vlc media player",
			"vlc",
			"movies & tv",
			"films & tv"
		];

		private static readonly Dictionary<Keys, Keys> KeyRemapTable = new()
    {
			{ Keys.Home, Keys.Left },
			{ Keys.Prior, Keys.Up },
			{ Keys.Next, Keys.Down },
			{ Keys.End, Keys.Right },
			{ Keys.Left, Keys.Home },
			{ Keys.Up, Keys.Prior },
			{ Keys.Down, Keys.Next },
			{ Keys.Right, Keys.End }
		};
		private static readonly Regex InvalidFileNameCharsRegex = new(
			string.Format("[{0}]", Regex.Escape(new string(Path.GetInvalidFileNameChars()))),
			RegexOptions.Compiled);

		internal static bool HasAssociatedExecutable(string path)
		{
			Span<char> executablePath = stackalloc char[ExecutableBufferSize];
			return (nint)PInvoke.FindExecutable(path, null, executablePath) > 32;
		}

		public static string SetPowerpointPreviewPrefix(SongSettings InItem, bool useTitlePrefix = false)
		{
			if (InItem.Type != "P")
			{
				return "";
			}

			if (useTitlePrefix)
			{
				// The title keeps the names readable; the path key keeps two items with the same title but
				// different files from sharing (and showing each other's) slide images.
				string prefixItemName = InvalidFileNameCharsRegex.Replace(InItem.Title, "") + "$" + OfficeLib.SlideImageManifest.PrefixKey(InItem.Path);

				if (InItem.OutputStyleScreen)
				{
					OUTPPFullPath = OUTPPPrefix + "$" + prefixItemName + "$";
					return OUTPPFullPath;
				}
				PREPPFullPath = PREPPPrefix + "$" + prefixItemName + "$";
				return PREPPFullPath;
			}

			if (InItem.OutputStyleScreen)
			{
				if ((OUTPPSequence < 0) | (OUTPPSequence >= MaxPowerpointSequence))
				{
					OUTPPSequence = 0;
				}
				OUTPPSequence++;
				OUTPPFullPath = OUTPPPrefix + OUTPPSequence;
				return OUTPPFullPath;
			}
			if ((PREPPSequence < 0) | (PREPPSequence >= MaxPowerpointSequence))
			{
				PREPPSequence = 0;
			}
			PREPPSequence++;
			PREPPFullPath = PREPPPrefix + PREPPSequence;
			return PREPPFullPath;
		}

		public static void MinimizePowerPointWindows(ref PowerPoint InPPT)
		{
			// The slide show windows are shrunk to one pixel at the primary monitor's corner: they must not
			// stay topmost there.
			LowerSlideShowWindows("MinimizePowerPointWindows", wait: true);
			InPPT.ResSetAllShowWindows(PowerpointList);
		}

		/// <summary>
		/// Collects the result of one run of an item and reports it once the item's runs are done. In a dual
		/// monitor show an item runs twice (main form and show window, in either order, one right after the
		/// other on the UI thread); the report is posted, so it runs after both. The notice is shown only when
		/// none of the item's runs made its slide show topmost (or that window is no longer topmost): a run that
		/// does not find the window after another run raised it is not a failure. A failure that repeats is
		/// logged in full once. "Not raised on purpose" (output area on the operator's monitor) is never
		/// reported; it is logged once per session. A run that did not start is reported in both monitor modes
		/// (in single monitor mode only that is collected: nothing is raised there).
		/// </summary>
		private static void ReportSlideShowRaiseError(PowerPoint InPPT, string itemPath, string runFailure)
		{
			if (reportPostedAtUtc != DateTime.MinValue && (DateTime.UtcNow - reportPostedAtUtc).TotalMilliseconds > ReportPostLostAfterMs)
			{
				// The posted report never ran (e.g. the main form's handle was recreated): report what was
				// collected now, so a lost post cannot keep reports off for the rest of the session.
				ReportPendingSlideShowRaiseError();
			}
			if (reportItemPath != null && !string.Equals(reportItemPath, itemPath, StringComparison.OrdinalIgnoreCase))
			{
				// Another item's results are still waiting for the posted report: they are complete.
				ReportPendingSlideShowRaiseError();
			}
			reportItemPath = itemPath ?? "";
			if (InPPT.SlideShowRaiseSkipped != null)
			{
				if (!slideShowRaiseSkippedLogged)
				{
					slideShowRaiseSkippedLogged = true;
					FrmMain.WriteDebugLog("RunPowerpointSong: slide show not brought to the front on purpose: " + InPPT.SlideShowRaiseSkipped + " (logged once per session)");
				}
			}
			else if (InPPT.SlideShowRaiseError == null && InPPT.CurrentSlideShowWindow != IntPtr.Zero)
			{
				reportItemRaisedWindow = InPPT.CurrentSlideShowWindow;
			}
			else if (InPPT.SlideShowRaiseError != null)
			{
				reportItemError = InPPT.SlideShowRaiseError;
			}
			if (runFailure != null)
			{
				reportItemRunFailure = runFailure;
			}
			if (reportPostedAtUtc != DateTime.MinValue)
			{
				return;
			}
			FrmMain mainForm = FrmMain.frmMain;
			if (mainForm == null || !mainForm.IsHandleCreated || mainForm.IsDisposed)
			{
				ReportPendingSlideShowRaiseError();
				return;
			}
			reportPostedAtUtc = DateTime.UtcNow;
			try
			{
				mainForm.BeginInvoke((MethodInvoker)ReportPendingSlideShowRaiseError);
			}
			catch
			{
				ReportPendingSlideShowRaiseError();
			}
		}

		// The item whose run results wait for the posted report, and what its runs did.
		private static string reportItemPath;
		private static IntPtr reportItemRaisedWindow;
		private static string reportItemError;
		private static string reportItemRunFailure;
		// When the report was posted; MinValue when none is waiting.
		private static DateTime reportPostedAtUtc = DateTime.MinValue;
		private const int ReportPostLostAfterMs = 5000;
		private static bool slideShowRaiseSkippedLogged;

		private static void ReportPendingSlideShowRaiseError()
		{
			reportPostedAtUtc = DateTime.MinValue;
			string error = reportItemError;
			string runFailure = reportItemRunFailure;
			IntPtr raisedWindow = reportItemRaisedWindow;
			string itemPath = reportItemPath;
			reportItemPath = null;
			reportItemError = null;
			reportItemRunFailure = null;
			reportItemRaisedWindow = IntPtr.Zero;
			if (PowerPoint.IsTopMostWindow(raisedWindow))
			{
				return;
			}
			if (!string.IsNullOrEmpty(runFailure))
			{
				FrmMain.WriteDebugLogThrottled("RunPowerpointSong: start the slide show", new InvalidOperationException(runFailure));
				// A missing file has already been reported for this item in this show (ValidatePowerpointItem).
				if (!missingFileReported.Contains(itemPath ?? ""))
				{
					FrmMain.ReportErrorNotice("The PowerPoint item could not be started: " + runFailure);
				}
				return;
			}
			if (string.IsNullOrEmpty(error))
			{
				return;
			}
			FrmMain.WriteDebugLogThrottled("RunPowerpointSong: bring the slide show to the front", new InvalidOperationException(error));
			FrmMain.ReportErrorNotice("The PowerPoint slide show may be hidden behind another window on the output monitor: " + error);
		}

		/// <summary>
		/// Called at the very start of loading a PowerPoint item (main form and show window), before any
		/// PowerPoint work that can pump messages: a lowering still waiting after a non-PowerPoint item
		/// (FrmLaunchShow) must not run any more, it could hit the slide show this item is about to raise.
		/// </summary>
		internal static void PowerPointItemLoading()
		{
			SlideShowRunCount++;
		}

		/// <summary>
		/// What the show window's last item load did with PowerPoint (set by FrmLaunchShow.LoadItem): None for
		/// an item it did not run as a slide show (not a PowerPoint item, or a live camera show), Started or
		/// NotStarted after RunPowerpointSong for a PowerPoint item.
		/// </summary>
		internal enum LivePowerPointRun
		{
			None,
			Started,
			NotStarted
		}

		internal static LivePowerPointRun LivePowerPointRunState;

		/// <summary>
		/// The show window's run of the live PowerPoint item did not start (RunPowerpointSong failed). Together
		/// with LivePowerPointEnded this says the projector does not show the live item's slide show. LivePP is
		/// the run that puts the slides on the output in both monitor modes; in dual monitor mode the main form's
		/// run only drives the same slide show. Remembered state only: PowerPoint is not asked.
		/// </summary>
		internal static bool LivePowerPointNotStarted => LivePowerPointRunState == LivePowerPointRun.NotStarted;

		/// <summary>
		/// The show window's run of the live PowerPoint item started, and a later call found its slide show gone
		/// (ended in PowerPoint, Esc, closed, PowerPoint exited). Remembered state only.
		/// </summary>
		internal static bool LivePowerPointEnded => LivePowerPointRunState == LivePowerPointRun.Started && LivePP.RunShowLost;

		/// <summary>
		/// The main form's PowerPoint wrapper (set by FrmMain at start-up), so the pre-load can attach it again
		/// when its PowerPoint is gone. Released with the others by ShutdownPowerpoint.
		/// </summary>
		internal static PowerPoint MainFormPPT;

		// Notices given in this show, so each is given once: ended slide show (per item and run), navigation
		// error (per item), missing file (per item), file not pre-opened (per file). Cleared at show start and end.
		private static string showLostReportedPath;
		private static readonly HashSet<string> navigationErrorReported = [with(StringComparer.OrdinalIgnoreCase)];
		private static readonly HashSet<string> missingFileReported = [with(StringComparer.OrdinalIgnoreCase)];
		private static readonly HashSet<string> preloadFailureReported = [with(StringComparer.OrdinalIgnoreCase)];
		private static bool attachFailureReported;
		private static bool powerpointMaxFilesReported;

		/// <summary>
		/// True the first time in this show the worship list is found to hold more PowerPoint files than
		/// PP_MaxFiles while the show runs (ValidateWorshipListItems without message boxes).
		/// </summary>
		internal static bool FirstPowerpointMaxFilesReport()
		{
			if (powerpointMaxFilesReported)
			{
				return false;
			}
			powerpointMaxFilesReported = true;
			return true;
		}

		/// <summary>
		/// Counts the show window's item loads (FrmLaunchShow.LoadItem). The main form uses it to tell whether
		/// the show window's PowerPoint run state belongs to a load it has not seen yet, and not to an earlier
		/// load of the same file.
		/// </summary>
		internal static int LiveLoadSerial;

		/// <summary>
		/// True the first time in this show the main form's PowerPoint reference could not be attached again.
		/// </summary>
		internal static bool FirstAttachFailureReport()
		{
			if (attachFailureReported)
			{
				return false;
			}
			attachFailureReported = true;
			return true;
		}

		/// <summary>
		/// Called at show start and show end: no PowerPoint state of the previous show carries over (the status
		/// row must not say NOT STARTED for a show that has not loaded its first item yet).
		/// </summary>
		internal static void ResetPowerPointShowState()
		{
			attachFailureReported = false;
			powerpointMaxFilesReported = false;
			LivePowerPointRunState = LivePowerPointRun.None;
			showLostReportedPath = null;
			navigationErrorReported.Clear();
			missingFileReported.Clear();
			preloadFailureReported.Clear();
		}

		/// <summary>
		/// True the first time in this show a missing PowerPoint file is reported for this path.
		/// </summary>
		internal static bool FirstMissingFileReport(string path)
		{
			return missingFileReported.Add(path ?? "");
		}

		/// <summary>
		/// True the first time in this show the pre-load reports this file as not opened.
		/// </summary>
		internal static bool FirstPreloadFailureReport(string path)
		{
			return preloadFailureReported.Add(path ?? "");
		}

		/// <summary>
		/// After a slide movement on a PowerPoint item: logs (throttled) an error the movement caught and reports
		/// it once per item in this show; reports once per item when the item's slide show has ended or been
		/// closed in PowerPoint. The item on the output is never changed here.
		/// </summary>
		internal static void ReportPowerpointNavigation(PowerPoint InPPT, SongSettings InItem)
		{
			Exception error = InPPT.LastNavigationError;
			if (error != null)
			{
				FrmMain.WriteDebugLogThrottled("PowerPoint slide movement", error);
				if (navigationErrorReported.Add(InItem?.Path ?? ""))
				{
					FrmMain.ReportErrorNotice("The PowerPoint slide could not be moved: " + error.GetBaseException().Message);
				}
				return;
			}
			if (!InPPT.LastCallNotRunning && !ReferenceEquals(InPPT, LivePP) && LivePowerPointEnded)
			{
				// The main form's wrapper moved the slide show, so it runs: the show window's earlier "ended"
				// verdict is asked again (one lookup) and, when it runs, the show window shows it again.
				ReprobeEndedLivePowerPoint();
			}
			ReportPowerpointShowEnded(InPPT, InItem);
		}

		/// <summary>
		/// The live item's slide show was believed ended: looks it up once more (the verdict may have been
		/// wrong, or the operator started it again). When it runs, the show window becomes transparent over it
		/// again. Returns true when it runs. No timer calls this: only operations do, one lookup each.
		/// </summary>
		internal static bool ReprobeEndedLivePowerPoint()
		{
			if (!LivePowerPointEnded)
			{
				return LivePowerPointRunState == LivePowerPointRun.Started;
			}
			LivePP.RefreshRunShowState();
			if (LivePowerPointEnded)
			{
				return false;
			}
			FrmMain.WriteDebugLog("PowerPoint: the live item's slide show runs again");
			FrmMain.frmMain?.LivePowerPointResumed();
			return true;
		}

		/// <summary>
		/// Reports once per item when the slide show the wrapper ran for it is found gone (Esc in the slide show,
		/// presentation closed, PowerPoint exited, or ended by the last Next). Nothing when it is running, or when
		/// it never started (that was reported by RunPowerpointSong). When the main form's wrapper found it gone,
		/// the show window's wrapper looks once too (they drive the same slide show; it is the truth for the
		/// status row), and the show window then stops being transparent over it.
		/// </summary>
		internal static void ReportPowerpointShowEnded(PowerPoint InPPT, SongSettings InItem)
		{
			if (!InPPT.RunShowLost)
			{
				return;
			}
			if (!ReferenceEquals(InPPT, LivePP) && LivePowerPointRunState == LivePowerPointRun.Started && !LivePP.RunShowLost)
			{
				LivePP.RefreshRunShowState();
			}
			if (LivePowerPointEnded)
			{
				FrmMain.frmMain?.LivePowerPointEnded();
			}
			string path = InItem?.Path ?? "";
			if (string.Equals(showLostReportedPath, path, StringComparison.OrdinalIgnoreCase))
			{
				return;
			}
			showLostReportedPath = path;
			FrmMain.WriteDebugLog("PowerPoint: the slide show of '" + path + "' is no longer running (ended or closed in PowerPoint)");
			FrmMain.ReportErrorNotice("The PowerPoint slide show of this item is no longer running (it was ended or closed in PowerPoint): " + Path.GetFileName(path));
		}

		/// <summary>
		/// Dual monitor show, decision (a) of fix-ppt-wrong-presentation round 2: the main form's own run of the
		/// output item did not start, but the show window's run of the same item did. The main form takes the
		/// show window's run (FollowRunOf) and reads the slide count and verse data from it, as RunPowerpointSong
		/// does after a run. Returns the slide count (0 when it could not be taken).
		/// </summary>
		internal static int AdoptLivePowerpointRun(ref SongSettings InItem, PowerPoint InMainPPT)
		{
			InMainPPT.FollowRunOf(LivePP, InItem.Path, PowerpointList);
			int count = InMainPPT.Count(ref InItem.Slide);
			if (count <= 0)
			{
				// Not taken: the item keeps its slide count and verse data.
				return 0;
			}
			// Verse data from this session's verse cache (filled when the slide images were built), so no slide
			// notes are read through PowerPoint on a key press; from the slide show's notes only when the cache
			// does not have the file.
			if (!InMainPPT.LoadVerses(InItem.Path, MAX_VERSES, DB_MAXSLIDES, ref InItem.SongVerses, ref InItem.Slide, SequenceSymbol, allowOpen: false))
			{
				ResetPowerpointVerses(ref InItem);
				InMainPPT.LoadVersesAndSlides(ref InItem.SongVerses, ref InItem.Slide, SequenceSymbol);
			}
			return count;
		}

		/// <summary>
		/// Decision (b): the show window's run did not start while the main form's did. The main form stops
		/// driving that slide show (it is not on the projector) and shows the item as not started.
		/// </summary>
		internal static void DropMainPowerpointRun(ref SongSettings InItem, PowerPoint InMainPPT)
		{
			InMainPPT.ForgetRun();
			ResetPowerpointVerses(ref InItem);
			InItem.TotalSlides = 0;
			InItem.CurSlide = 1;
		}

		private static void ResetPowerpointVerses(ref SongSettings InItem)
		{
			PowerPoint.ResetVerses(MAX_VERSES, DB_MAXSLIDES, ref InItem.SongVerses, ref InItem.Slide);
		}

		/// <summary>
		/// Puts a window of this app at the top of the topmost windows without activating it.
		/// </summary>
		internal static void KeepWindowTopMost(IntPtr handle)
		{
			SetWindowPos(handle, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE);
		}

		/// <summary>
		/// Counts the slide show runs (RunPowerpointSong). A delayed lowering of the slide show windows
		/// (FrmLaunchShow, after a non-PowerPoint item) is dropped when a slide show was run since it was asked for.
		/// </summary>
		internal static int SlideShowRunCount;

		/// <summary>
		/// Tells the wrapper which monitor the output goes to (the monitor holding most of the output area
		/// LS_Left/LS_Top/LS_Width/LS_Height) and which monitor the main form is on: the slide show is made
		/// topmost only when it is on the first and not on the second.
		/// </summary>
		private static void SetSlideShowMonitorBounds(PowerPoint InPPT)
		{
			Rectangle output = Screen.FromRectangle(new Rectangle(LS_Left, LS_Top, Math.Max(1, LS_Width), Math.Max(1, LS_Height))).Bounds;
			FrmMain mainForm = FrmMain.frmMain;
			Rectangle operatorScreen = (mainForm != null && mainForm.IsHandleCreated) ? Screen.FromHandle(mainForm.Handle).Bounds : Screen.PrimaryScreen.Bounds;
			InPPT.OutputMonitorBounds = (output.Left, output.Top, output.Right, output.Bottom);
			InPPT.OperatorMonitorBounds = (operatorScreen.Left, operatorScreen.Top, operatorScreen.Right, operatorScreen.Bottom);
		}

		/// <summary>
		/// Takes the topmost state off slide show windows that a crashed or killed run left topmost (app start,
		/// show start). Window enumeration only; logs how many were found.
		/// </summary>
		internal static void LowerLeftoverSlideShowWindows(string where)
		{
			try
			{
				int count = PowerPoint.LowerLeftoverTopMostSlideShowWindows();
				if (count > 0)
				{
					FrmMain.WriteDebugLog(where + ": " + count + " topmost PowerPoint slide show window(s) left from an earlier run were taken off the top");
				}
			}
			catch (Exception ex)
			{
				FrmMain.WriteDebugLogThrottled(where + ": lower leftover slide show windows", ex);
			}
		}

		/// <summary>
		/// Takes the topmost state off the slide show windows the app made topmost (see RunPowerpointSong).
		/// </summary>
		internal static void LowerSlideShowWindows(string where, bool wait)
		{
			string error = PowerPoint.LowerRaisedSlideShowWindows(wait);
			if (error == null)
			{
				return;
			}
			FrmMain.WriteDebugLogThrottled(where + ": take the slide show windows off the top", new InvalidOperationException(error));
			FrmMain.ReportErrorNotice("A PowerPoint slide show window could not be taken off the top of the screen: " + error);
		}

		public static int RunPowerpointSong(ref SongSettings InItem, ref PowerPoint InPPT, int StartingSlide)
		{
			return RunPowerpointSong(ref InItem, ref InPPT, StartingSlide, ShowResult: false);
		}

		/// <summary>
		/// The show's pre-load did not open the PowerPoint files (PowerPoint did not answer, or stopped answering):
		/// they are opened when a PowerPoint item is run (fix-ppt-show-call-timeout P5).
		/// </summary>
		internal static bool PowerPointPreloadPending;

		private static bool powerPointGateHooked;

		/// <summary>
		/// fix-ppt-show-call-timeout P3: every change of the gate's "not responding" state is logged and told once.
		/// Called once at start-up (FrmMain), before the main form's PowerPoint wrapper is attached.
		/// </summary>
		internal static void HookPowerPointGate()
		{
			if (powerPointGateHooked)
			{
				return;
			}
			powerPointGateHooked = true;
			PowerPointGate.StateChanged += (notResponding, reason) =>
			{
				FrmMain.WriteDebugLog("PowerPoint gate: " + (notResponding ? "PowerPoint is not responding: " : "PowerPoint answers again: ") + reason);
				FrmMain.ReportErrorNotice(notResponding
					? "PowerPoint is not responding: PowerPoint items cannot be shown until it answers again"
					: "PowerPoint answers again: load a PowerPoint item again to show it");
			};
			PowerPointGate.Trace += text => FrmMain.WriteDebugLog(text);
		}

		public static int RunPowerpointSong(ref SongSettings InItem, ref PowerPoint InPPT, int StartingSlide, bool ShowResult)
		{
			if (PowerPointPreloadPending && ShowRunning)
			{
				// The show's files were not opened: the existing mid-show pre-load opens them now (skipped again,
				// at once, while PowerPoint does not answer; this item then does not start).
				FrmMain.WriteDebugLog("RunPowerpointSong: the PowerPoint files were not pre-loaded; pre-loading them now");
				gfFileHelpers.PreLoadPowerpointFiles(ref LivePP, ref WorshipSongs);
			}
			ResetPowerpointVerses(ref InItem);
			SlideShowRunCount++;
			// Dual monitor show only: there the show window is transparent over a PowerPoint item, so the
			// slide show must sit above other applications' windows on the output monitor. In single monitor
			// mode the show window covers the screen and keeps itself on top with its own timer.
			var target = !ShowLiveCam && Gf.DualMonitorSelectAutoOption == 1
				? new Rectangle(LS_Left, LS_Top, LS_Width, LS_Height)
				: MonitorLayout.FindDisplay(DisplayInfo.GetDisplays(), DisplayInfo.ActiveOutputMonitorName)?.Bounds ?? Rectangle.Empty;
			InPPT.TargetBounds = target.Width > 0 && target.Height > 0 ? (target.Left, target.Top, target.Width, target.Height) : null;
			InPPT.KeepSlideShowOnTop = DualMonitorMode;
			if (DualMonitorMode)
			{
				SetSlideShowMonitorBounds(InPPT);
			}

			// One heavy group for the whole item run (fix-ppt-show-call-timeout G4): Run, the slide placement, the
			// window placement, the verse markers and the slide count share its limits. Given up at any point, the
			// item takes the "not started" path below.
			string runMessage;
			int count = 0;
			bool givenUp;
			using (PowerPointGate gate = PowerPointGate.Enter(PowerPointCallWeight.Heavy, "run the PowerPoint item " + Path.GetFileName(InItem.Path)))
			{
				if (gate == null)
				{
					// Not asked: no slide show was started, and nothing of an earlier run of this wrapper counts.
					InPPT.ForgetAbandonedRun();
					runMessage = PowerPoint.NotRespondingText;
				}
				else
				{
					runMessage = InPPT.Run(InItem.Path, ref PowerpointList);
					if (InPPT.LastRunStarted)
					{
						count = RunPowerpointSongCalls(ref InItem, InPPT, StartingSlide);
					}
				}
				givenUp = PowerPointCallLimit.CurrentAbandoned;
			}
			if (givenUp && InPPT.LastRunStarted)
			{
				// Given up after the slide show started: the item counts as not started (default background, status
				// row, notice); the wrapper does not act on that slide show any more.
				InPPT.ForgetAbandonedRun();
				ResetPowerpointVerses(ref InItem);
				runMessage = PowerPoint.NotRespondingText;
			}
			if (!InPPT.LastRunStarted)
			{
				// The slide show of this item did not start. Nothing below may run: it would act on no slide show
				// at all (before the wrapper remembered its run it moved, measured and read the previous item's
				// slide show, PowerPoint's active presentation, and returned that deck's slide count). 0 slides is
				// what the readers already get when no presentation is open at all, and they cope with it
				// (status "x/0", slide keys find nothing to move). The verse data was reset above, so no verse
				// buttons.
				string runFailure = string.IsNullOrEmpty(runMessage) ? "(no result)" : runMessage;
				if (InPPT.KeepSlideShowOnTop)
				{
					// The previous PowerPoint item's slide show must not stay on top under the transparent show
					// window (waiting, as after a non-PowerPoint item).
					LowerSlideShowWindows("RunPowerpointSong: item not started", wait: true);
				}
				// Both monitor modes; one notice per item (the main form and the show window both run it).
				ReportSlideShowRaiseError(InPPT, InItem.Path, runFailure);
				InItem.CurSlide = 1;
				return 0;
			}
			// Once per item, after its last raise (Run, or SetShowWindow in custom output area mode, which may
			// first move the slide show onto the output monitor).
			if (InPPT.KeepSlideShowOnTop)
			{
				ReportSlideShowRaiseError(InPPT, InItem.Path, null);
			}
			return count;
		}

		/// <summary>
		/// The calls of RunPowerpointSong after its slide show started (inside its group): the starting slide, the
		/// window placement, the verse markers; returns the slide count.
		/// </summary>
		private static int RunPowerpointSongCalls(ref SongSettings InItem, PowerPoint InPPT, int StartingSlide)
		{
			// A new slide show runs: its end, when a later call finds it gone, is reported again.
			showLostReportedPath = null;
			if (StartingSlide < 2)
			{
				InPPT.First();
				InItem.CurSlide = 1;
			}
			else if (StartingSlide > InPPT.Count())
			{
				InPPT.Last();
				InItem.CurSlide = InPPT.Count();
			}
			else if (InPPT.Count() > 0)
			{
				InPPT.GotoSlide(StartingSlide);
				InItem.CurSlide = StartingSlide;
			}

			if (!InPPT.PositionedOnTarget && InPPT.TargetBounds is { } target)
			{
				// WinForms runs system-DPI aware. Its desktop coordinates use the system DPI.
				using var desktop = Graphics.FromHwnd(IntPtr.Zero);
				var bounds = PowerPointWindowPoints(new Rectangle(target.Left, target.Top, target.Width, target.Height), desktop.DpiX);
				InPPT.SetShowWindow(bounds.Left, bounds.Top, bounds.Width, bounds.Height);
			}

			InPPT.LoadVersesAndSlides(ref InItem.SongVerses, ref InItem.Slide, SequenceSymbol);
			return InPPT.Count(ref InItem.Slide);
		}

		// Show end and app exit (fix-ppt-show-end-timeout): PowerPoint must answer a window message within the probe
		// time before it is called (T1), and the calls that follow are given up when none of them has completed for
		// PowerPointEndStallMs, or when they have run for PowerPointEndCapMs in all (T2, PowerPointCallLimit).
		private const int PowerPointProbeTimeoutMs = 1000;
		// App exit outside a Windows session end: a false "not responding" there leaves a hidden PowerPoint running.
		private const int PowerPointExitProbeTimeoutMs = 3000;
		private const int PowerPointEndStallMs = 5000;
		private const int PowerPointEndCapMs = 30000;

		// T3: the show copies a show end could not close because PowerPoint did not answer (rows as in
		// PowerpointList: source path, copy name), and whether a clean-up (these copies, the quit, or proxies that
		// were not released) waits for PowerPoint. Tried again at the next show start and at app exit.
		private static ShowCopyList pendingShowCopies = new();
		private static bool powerPointCleanUpPending;

		public static void ClearUpPowerpointWindows()
		{
			EndPowerPointShow("Show end", null);
		}

		/// <summary>
		/// Probe time at app exit: 3 s, but 1 s while Windows ends the session (Windows may end the process at any
		/// moment there, and ends PowerPoint itself).
		/// </summary>
		private static int ExitProbeTimeoutMs => FrmMain.EndSessionDepth > 0 ? PowerPointProbeTimeoutMs : PowerPointExitProbeTimeoutMs;

		/// <summary>
		/// Show end (exitWrappers null) or app exit (the wrappers whose references are released afterwards): the
		/// slide show windows taken off the top, the show's PowerPoint state reset, then the show's copies closed and
		/// PowerPoint quit when nothing else is open in it, as before. At app exit also the clean-up an earlier show
		/// end could not do. PowerPoint is asked first whether it answers (T1); the calls run under the time limit
		/// (T2). Returns false when PowerPoint did not answer or stopped answering: the copies are kept for a later
		/// attempt, the wrapper's proxy is dropped, nothing waits.
		/// </summary>
		private static bool EndPowerPointShow(string where, PowerPoint[] exitWrappers)
		{
			bool atExit = exitWrappers != null;
			// Before closing: if a presentation fails to close, its slide show must not stay on top.
			LowerSlideShowWindows("ClearUpPowerpointWindows", wait: false);
			// Show end (and app exit): nothing of this show's PowerPoint state carries over.
			ResetPowerPointShowState();
			// No copy listed and no reference: no PowerPoint call would be made (a show of lyrics items only).
			bool liveWork = PowerpointList.Count > 0 || LivePP.prePowerPointApp != null;
			bool retryWork = atExit && powerPointCleanUpPending;
			bool releaseWork = false;
			if (atExit)
			{
				foreach (PowerPoint ppt in exitWrappers)
				{
					releaseWork |= ppt?.prePowerPointApp != null;
				}
				releaseWork |= PowerPoint.HeldProxyCount > 0 || PowerPoint.LeftOpenCount > 0;
			}
			if (!liveWork && !retryWork && !releaseWork)
			{
				return true;
			}
			PowerPoint.PowerPointAnswer answer = PowerPoint.ProbePowerPoint(atExit ? ExitProbeTimeoutMs : PowerPointProbeTimeoutMs, out string detail);
			if (answer == PowerPoint.PowerPointAnswer.NotResponding)
			{
				PowerPointNotAnswering(where, "PowerPoint did not answer (" + detail + ")", atExit);
				return false;
			}
        using PowerPointCallLimit limit = PowerPointCallLimit.Begin(PowerPointEndStallMs, PowerPointEndCapMs);
        if (answer == PowerPoint.PowerPointAnswer.Answers && PowerPoint.LeftOpenCount > 0)
        {
            // Temporary presentations a given-up group left open (fix-ppt-show-call-timeout G3): closed first,
            // so the quit below is not prevented by them.
            PowerPoint.CloseLeftOpenPresentations();
        }
        if (retryWork && !RetryPendingCleanUpCalls(where, answer, atExit))
        {
            PowerPointNotAnswering(where, "PowerPoint stopped answering: the clean-up left by an earlier show end was abandoned " + LimitText(limit), atExit);
            return false;
        }
        if (liveWork)
        {
            string cleanupReport = LivePP.ClearUpPowerpointWindows(ref PowerpointList);
            if (!PowerPointCallLimit.CurrentAbandoned)
            {
                ReportShowEndCloseFailures(cleanupReport);
                if (PowerpointList.Count > 0)
                {
                    // Copies that may still be open (Close failed, or not every presentation could be read):
                    // tried again at the next show start and at app exit instead of being forgotten.
                    KeepShowCopiesForLater();
                    powerPointCleanUpPending = true;
                }
                if (cleanupReport == "")
                {
                    LivePP.QuitPowerPointApp(LivePP.prePowerPointApp);
                }
            }
            if (PowerPointCallLimit.CurrentAbandoned)
            {
                PowerPointNotAnswering(where, "PowerPoint stopped answering: the calls were abandoned " + LimitText(limit) + " (" + cleanupReport + ")", atExit);
                return false;
            }
        }
        return true;
		}

		private static string LimitText(PowerPointCallLimit limit)
		{
			return limit == null ? "" : "after " + limit.ElapsedMilliseconds + " ms (" + limit.SinceProgressMilliseconds + " ms after the last completed call)";
		}

		/// <summary>
		/// PowerPoint did not answer at a show end or app exit (T1), or stopped answering during the calls (T2):
		/// nothing more is asked of it. The live wrapper stops using its proxy (kept unreleased), the show's copies
		/// are kept for the next attempt (next show start, app exit), names remembered from them are forgotten,
		/// logged; at a show end the operator is told when a copy could not be closed. When every copy was closed
		/// already (PowerPoint stopped answering at the quit), there is nothing to tell: logged only.
		/// </summary>
		private static void PowerPointNotAnswering(string where, string reason, bool atExit)
		{
			// The PowerPoint gate of the show (fix-ppt-show-call-timeout G9) takes the same state, so the pre-load that
			// follows a show start is skipped at once instead of asking PowerPoint again.
			PowerPointGate.NoteNotResponding(where + ": " + reason);
			LivePP.DropPowerPointApp();
			KeepShowCopiesForLater();
			PowerPoint.ForgetShowCopyNames();
			powerPointCleanUpPending = true;
			if (atExit)
			{
				FrmMain.WriteDebugLog(where + ": " + reason + "; " + pendingShowCopies.Count + " show copy(ies) not closed and no PowerPoint reference released (a release would wait for it). A hidden PowerPoint started by Axicultuss may remain running.");
				return;
			}
			if (pendingShowCopies.Count == 0)
			{
				FrmMain.WriteDebugLog(where + ": " + reason + "; every show copy was closed, PowerPoint was not quit (tried again at the next show start and app exit)");
				return;
			}
			FrmMain.WriteDebugLog(where + ": " + reason + "; the PowerPoint clean-up was skipped (" + pendingShowCopies.Count + " show copy(ies) kept for the next show start and app exit)");
			FrmMain.ReportErrorNotice("PowerPoint is not responding: the show's PowerPoint copy could not be closed (tried again at the next show start)");
		}

		/// <summary>
		/// Moves the rows of PowerpointList to the copies kept for a later clean-up (once per name) and empties the
		/// list, as a show end does: the next show opens new copies, and those get other names (names are unique in
		/// one PowerPoint, and these copies keep theirs while they stay open). A kept copy is closed later only when
		/// its name AND its mark match (ClearUpPowerpointWindows), so a presentation that is not one of the app's
		/// copies is never closed.
		/// </summary>
		private static void KeepShowCopiesForLater()
		{
			int last = PowerpointList.Count;
			for (int i = 1; i <= last; i++)
			{
				string path = PowerpointList[i].SourcePath;
				string name = PowerpointList[i].CopyName;
				if (string.IsNullOrEmpty(path) || string.IsNullOrEmpty(name))
				{
					continue;
				}
				bool known = false;
				for (int k = 1; k <= pendingShowCopies.Count && !known; k++)
				{
					known = string.Equals(pendingShowCopies[k].CopyName, name, StringComparison.OrdinalIgnoreCase)
						&& string.Equals(pendingShowCopies[k].SourcePath, path, StringComparison.OrdinalIgnoreCase);
				}
				if (!known)
				{
					pendingShowCopies.Add(path, name);
				}
			}
			PowerpointList.Clear();
		}

		/// <summary>
		/// Show start (before the pre-load opens this show's copies): when an earlier show end could not clean up
		/// because PowerPoint did not answer, tries it again, under the same rules (T1, T2). PowerPoint is not quit
		/// here: the pre-load attaches to it right after.
		/// </summary>
		internal static void RetryPowerPointCleanUp(string where)
		{
			if (!powerPointCleanUpPending)
			{
				return;
			}
			try
			{
				PowerPoint.PowerPointAnswer answer = PowerPoint.ProbePowerPoint(PowerPointProbeTimeoutMs, out string detail);
				if (answer == PowerPoint.PowerPointAnswer.NotResponding)
				{
					FrmMain.WriteDebugLog(where + ": PowerPoint still does not answer (" + detail + "); the clean-up left by an earlier show end waits (" + pendingShowCopies.Count + " show copy(ies))");
					PowerPointGate.NoteNotResponding(where + ": PowerPoint still does not answer (" + detail + ")");
					if (pendingShowCopies.Count > 0)
					{
						FrmMain.ReportErrorNotice("PowerPoint is still not responding: the PowerPoint copy of an earlier show could not be closed");
					}
					return;
				}
				using PowerPointCallLimit limit = PowerPointCallLimit.Begin(PowerPointEndStallMs, PowerPointEndCapMs);
				if (!RetryPendingCleanUpCalls(where, answer, atExit: false))
				{
					PowerPointNotAnswering(where, "PowerPoint stopped answering: the clean-up left by an earlier show end was abandoned " + LimitText(limit), atExit: false);
				}
			}
			catch (Exception ex)
			{
				FrmMain.WriteDebugLogThrottled(where + ": clean-up left by an earlier show end", ex);
			}
		}

		/// <summary>
		/// The calls of a retried clean-up, after PowerPoint answered, could not be asked (NoWindow), or is not
		/// running. Releases the proxies kept unreleased (only after Answers or NotRunning: a release waits for
		/// PowerPoint), closes the kept copies (name and mark, as at a show end) through the live wrapper, attached
		/// again first when it dropped its proxy, and at app exit quits PowerPoint when nothing else is open in it
		/// (also when no copy was kept: the quit itself may be what was left). Returns false when the time limit ran
		/// out (what is still kept stays kept). The pending flag stays set when something could not be checked.
		/// </summary>
		private static bool RetryPendingCleanUpCalls(string where, PowerPoint.PowerPointAnswer answer, bool atExit)
		{
			if (answer == PowerPoint.PowerPointAnswer.NotRunning)
			{
				// The copies went with the process; a release of a proxy to it fails at once. A new PowerPoint gives
				// the same names again.
				PowerPoint.ReleaseHeldProxies();
				PowerPoint.ForgetShowCopyNames();
				FrmMain.WriteDebugLog(where + ": PowerPoint is no longer running; the " + pendingShowCopies.Count + " show copy(ies) an earlier show end could not close are gone with it");
				pendingShowCopies.Clear();
				powerPointCleanUpPending = false;
				return true;
			}
			bool answered = answer == PowerPoint.PowerPointAnswer.Answers;
			if (answered)
			{
				PowerPoint.CloseLeftOpenPresentations();
				if (PowerPointCallLimit.CurrentAbandoned)
				{
					return false;
				}
				PowerPoint.ReleaseHeldProxies();
			}
			if (pendingShowCopies.Count > 0 || atExit)
			{
				if (LivePP.prePowerPointApp == null)
				{
					LivePP.NewApp();
				}
				if (PowerPointCallLimit.CurrentAbandoned)
				{
					return false;
				}
				if (LivePP.prePowerPointApp == null)
				{
					// Nothing could be checked: the kept copies stay kept for the next attempt.
					FrmMain.WriteDebugLog(where + ": PowerPoint could not be attached again; the " + pendingShowCopies.Count + " show copy(ies) an earlier show end could not close stay kept");
					return true;
				}
				if (pendingShowCopies.Count > 0)
				{
					int kept = pendingShowCopies.Count;
					string cleanupReport = LivePP.ClearUpPowerpointWindows(ref pendingShowCopies, quitWhenEmpty: atExit);
					if (PowerPointCallLimit.CurrentAbandoned)
					{
						return false;
					}
					FrmMain.WriteDebugLog(where + ": PowerPoint answers again; the " + kept + " show copy(ies) an earlier show end could not close were looked for and closed (" + cleanupReport + ")");
					int failed = LivePP.LastCloseFailedCount;
					if (failed > 0 || LivePP.LastCloseError != null)
					{
						string message = failed > 0
							? "A PowerPoint copy of an earlier show could not be closed: " + LivePP.LastCloseFailedName
							: "Not every PowerPoint copy of an earlier show could be checked for closing: " + LivePP.LastCloseError;
						FrmMain.WriteDebugLog(where + ": " + message);
						FrmMain.ReportErrorNotice(message);
					}
				}
				else
				{
					// App exit with no copy kept: only the quit was left (PowerPoint stopped answering at it).
					LivePP.QuitPowerPointApp(LivePP.prePowerPointApp);
					if (PowerPointCallLimit.CurrentAbandoned)
					{
						return false;
					}
					FrmMain.WriteDebugLog(where + ": PowerPoint answers again; the quit left by an earlier show end was done (only when nothing is open in it)");
				}
			}
			// Copies still not closed (ClearUpPowerpointWindows leaves them listed), and proxies not released because
			// PowerPoint could not be asked (NoWindow), keep the clean-up pending.
			powerPointCleanUpPending = pendingShowCopies.Count > 0 || (!answered && PowerPoint.HeldProxyCount > 0);
			return true;
		}

		/// <summary>
		/// Logs and reports the copies the show-end clean-up could not close (their slide shows stay open in
		/// PowerPoint, already taken off the top), or that it could not check. PowerPoint is then not quit
		/// (QuitPowerPointApp only quits when nothing is open), as before.
		/// </summary>
		private static void ReportShowEndCloseFailures(string summary)
		{
			int failed = LivePP.LastCloseFailedCount;
			string error = LivePP.LastCloseError;
			if (failed == 0 && error == null)
			{
				return;
			}
			string message = (failed > 0)
				? (failed == 1
					? "A PowerPoint presentation of this show could not be closed: " + LivePP.LastCloseFailedName
					: failed + " PowerPoint presentations of this show could not be closed (first: " + LivePP.LastCloseFailedName + ")")
				: "Not every PowerPoint presentation of this show could be checked for closing: " + error;
			if (failed > 0 && error != null)
			{
				message += "; " + error;
			}
			FrmMain.WriteDebugLog("ClearUpPowerpointWindows: " + message + " (" + summary + ")");
			FrmMain.ReportErrorNotice(message);
		}

		/// <summary>
		/// Called once when the application exits. Closes the presentations opened for the live show,
		/// quits PowerPoint when nothing else is open in it, and releases every PowerPoint reference the app holds.
		/// All wrappers share one PowerPoint process; releasing every reference here is what was observed
		/// to make the hidden PowerPoint exit when the app closes.
		/// The wrapper objects themselves stay in place so late callers do not hit a null wrapper.
		/// A release waits for PowerPoint and cannot be cancelled, so PowerPoint is asked once more right before the
		/// releases; when it does not answer they are skipped (the process exit leaves them). A hang between that
		/// question and a release can still make the release wait.
		/// </summary>
		public static void ShutdownPowerpoint(PowerPoint InMainPPT)
		{
			PowerPoint[] wrappers = [LivePP, PreviewPPT, OutputPPT, ExternalPPT, InMainPPT];
			bool answered = true;
			try
			{
				answered = EndPowerPointShow("App exit", wrappers);
			}
			catch (Exception ex)
			{
				Trace.WriteLine($"ShutdownPowerpoint: clean-up of live presentations failed: {ex.Message}");
			}
			if (!answered)
			{
				// PowerPoint does not answer: releasing a reference would wait for it (logged by EndPowerPointShow).
				return;
			}
			bool anyReference = PowerPoint.HeldProxyCount > 0;
			foreach (PowerPoint ppt in wrappers)
			{
				anyReference |= ppt?.prePowerPointApp != null;
			}
			if (!anyReference)
			{
				return;
			}
			PowerPoint.PowerPointAnswer answer = PowerPoint.ProbePowerPoint(ExitProbeTimeoutMs, out string detail);
			if (answer == PowerPoint.PowerPointAnswer.NotResponding)
			{
				FrmMain.WriteDebugLog("App exit: PowerPoint did not answer before the references were released (" + detail + "); no PowerPoint reference released (a release would wait for it). A hidden PowerPoint started by Axicultuss may remain running.");
				return;
			}
			if (answer != PowerPoint.PowerPointAnswer.NoWindow)
			{
				PowerPoint.ReleaseHeldProxies();
			}
			foreach (PowerPoint ppt in wrappers)
			{
				ppt?.ReleasePowerPointApp();
			}
		}

		public static void ReMapKeyBoard(ref Keys InKey)
		{
			if (KeyBoardOption != 1)
			{
				return;
			}

			if (KeyRemapTable.TryGetValue(InKey, out Keys mappedKey))
			{
				InKey = mappedKey;
			}
		}

		public static int ImplementSlideMovement(ref int InCurSlide, int InCurMaxSlide, Keys InKey, int InSlideNo)
		{
			switch (InKey)
			{
				case Keys.Left:
					InCurSlide = 1;
					break;
				case Keys.Up:
					InCurSlide = ((InCurSlide <= 2) ? 1 : (InCurSlide - 1));
					break;
				case Keys.Down:
					InCurSlide = ((InCurSlide < InCurMaxSlide) ? (InCurSlide + 1) : InCurMaxSlide);
					break;
				case Keys.Right:
					InCurSlide = InCurMaxSlide;
					break;
				case Keys.None:
					InCurSlide = InSlideNo;
					break;
			}
			return InCurSlide;
		}

		public static Keys ReMapKeyDirectionToPowerpoint(KeyDirection InDirection)
		{
        return InDirection switch
        {
            KeyDirection.FirstOne => Keys.Left,
            KeyDirection.PrevOne => Keys.Up,
            KeyDirection.NextOne => Keys.Down,
            KeyDirection.LastOne => Keys.Right,
            KeyDirection.SpaceOne => Keys.Space,
            _ => Keys.F5,
        };
    }

		public static bool RunProcess(string InProcessString)
		{
			try
			{
				ProcessStartInfo psi = new()
            {
					WindowStyle = ProcessWindowStyle.Normal,
					FileName = InProcessString,
					UseShellExecute = true
				};
				Process.Start(psi);

				//.net core??�쎌�????�쎈????媛숈????�쎈????�쎌?????�쏙?? ??�쎌????�쎌???�쎌???�쏙?? ??�쎌????�쎌??
				//Process process = Process.Start(InProcessString);
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
				Console.WriteLine(e.StackTrace);
				return false;
			}
			return true;
		}

		private static IntPtr FindRecentMediaPlayerWindow(string fileName)
		{
			IntPtr foundWindow = IntPtr.Zero;
			string fileNameLower = Path.GetFileName(fileName).ToLower();

			EnumWindows((hWnd, lParam) =>
			{
				if (!PInvoke.IsWindowVisible((HWND)hWnd))
					return true;

				string windowTitle = ReadWindowTitle(hWnd, WindowTitleBufferSize).ToLower();
				if (windowTitle.Length == 0) return true;


				if (!string.IsNullOrEmpty(windowTitle))
				{
					string fileNameWithoutExt = Path.GetFileNameWithoutExtension(fileNameLower);

					if (windowTitle.Contains(fileNameWithoutExt) ||
						windowTitle.Contains(MediaPlayerWindowKeywords[0]) ||
						windowTitle.Contains(MediaPlayerWindowKeywords[1]) ||
						windowTitle.Contains(MediaPlayerWindowKeywords[2]) ||
						windowTitle.Contains(MediaPlayerWindowKeywords[3]) ||
						windowTitle.Contains(MediaPlayerWindowKeywords[4]) ||
						windowTitle.Contains(MediaPlayerWindowKeywords[5]))
					{
						foundWindow = hWnd;
						return false; // 李얠�??�㈃ 以묐??
					}
				}

				return true; // ?�꾩??寃??
			}, IntPtr.Zero);

			return foundWindow;
		}

		private static IntPtr FindMediaPlayerWindowWithRetries(string fileName, int initialDelayMs, int retryDelayMs, int retryCount)
		{
			if (initialDelayMs > 0)
			{
				Thread.Sleep(initialDelayMs);
			}

			IntPtr handle = FindRecentMediaPlayerWindow(fileName);
			for (int retry = 0; retry < retryCount && handle == IntPtr.Zero; retry++)
			{
				Thread.Sleep(retryDelayMs);
				handle = FindRecentMediaPlayerWindow(fileName);
			}

			return handle;
		}

		private static IntPtr WaitForMainWindowHandle(Process process, int maxWaitCount, int waitDelayMs)
		{
			int waitCount = 0;
			while (process.MainWindowHandle == IntPtr.Zero && waitCount < maxWaitCount)
			{
				Thread.Sleep(waitDelayMs);
				process.Refresh();
				waitCount++;
			}

			return process.MainWindowHandle;
		}

		public static bool RunProcessOnMonitor(string InProcessString, string monitorName)
		{
			try
			{

				Screen targetScreen = null;
				foreach (Screen screen in Screen.AllScreens)
				{
					if (screen.DeviceName == monitorName)
					{
						targetScreen = screen;
						break;
					}
				}

				if (monitorName == "Primary")
				{
					string secondaryName = DisplayInfo.getSecondryDisplayName();
					foreach (Screen screen in Screen.AllScreens)
					{
						if (screen.DeviceName == secondaryName)
						{
							targetScreen = screen;
							break;
						}
					}
				}

				if (targetScreen == null)
				{
	
					return RunProcess(InProcessString);
				}


				ProcessStartInfo psi = new()
            {
					WindowStyle = ProcessWindowStyle.Normal,
					FileName = InProcessString,
					UseShellExecute = true
				};
				Process process = Process.Start(psi);

				if (process == null)
				{
					IntPtr foundHandle = FindMediaPlayerWindowWithRetries(
						InProcessString,
						WindowInitialSearchDelayMs,
						WindowRetryDelayMs,
						WindowRetryCount);

					if (foundHandle != IntPtr.Zero)
					{

						Rectangle targetBounds = targetScreen.Bounds;
						SetWindowPos(foundHandle, HWND_TOP,
							targetBounds.X, targetBounds.Y,
							targetBounds.Width, targetBounds.Height,
							SWP_SHOWWINDOW);
						SetForegroundWindow(foundHandle);
						PInvoke.ShowWindow((HWND)foundHandle, (SHOW_WINDOW_CMD)SW_MAXIMIZE);
						return true;
					}
					else
					{
						Console.WriteLine("RunProcessOnMonitor: process媛 null??��?李쎈�?李얠??????�뒿??�떎.");
						return false;
					}
				}


				IntPtr handle = WaitForMainWindowHandle(process, MainWindowWaitCount, MainWindowWaitDelayMs);


				if (handle == IntPtr.Zero)
				{
					handle = FindMediaPlayerWindowWithRetries(
						InProcessString,
						WindowHandleFallbackDelayMs,
						WindowHandleRetryDelayMs,
						WindowRetryCount);


					if (handle == IntPtr.Zero)
					{

						return true; 
					}
				}

				Rectangle bounds = targetScreen.Bounds;


				Thread.Sleep(WindowSettleDelayMs);


				SetWindowPos(handle, HWND_TOP,
					bounds.X, bounds.Y,
					bounds.Width, bounds.Height,
					SWP_SHOWWINDOW);


				SetForegroundWindow(handle);


				Thread.Sleep(ForegroundDelayMs);
				PInvoke.ShowWindow((HWND)handle, (SHOW_WINDOW_CMD)SW_MAXIMIZE);


				Thread.Sleep(WindowSettleDelayMs);
				Rectangle windowRect;
				if (GetWindowRect(handle, out windowRect))
				{

					if (windowRect.Left < bounds.Left - WindowBoundsSlack ||
						windowRect.Top < bounds.Top - WindowBoundsSlack ||
						windowRect.Left > bounds.Right + WindowBoundsSlack)
					{
						
						SetWindowPos(handle, HWND_TOP,
							bounds.X, bounds.Y,
							bounds.Width, bounds.Height,
							SWP_SHOWWINDOW);
						Thread.Sleep(MainWindowWaitDelayMs);
						PInvoke.ShowWindow((HWND)handle, (SHOW_WINDOW_CMD)SW_MAXIMIZE);
					}
				}

				return true;
			}
			catch (Exception e)
			{
				Console.WriteLine("RunProcessOnMonitor failed: " + e.Message);
				Console.WriteLine(e.StackTrace);
				return false;
			}
		}

}
