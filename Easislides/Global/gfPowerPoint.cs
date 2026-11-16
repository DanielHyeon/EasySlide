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
using Easislides.Module;
using Easislides.Util;
using OfficeLib;

namespace Easislides
{
    internal unsafe partial class Gf
    {
		private const int ExecutableBufferSize = 260;
		private const int MaxPowerpointSequence = 49;
		private const int MaxSlideCount = 1000;
		private const int SongVerseInitCount = 10;
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
		private const float PowerpointShowWindowScale = 0.75f;

		private static readonly string[] MediaPlayerWindowKeywords =
		{
			"windows media player",
			"media player",
			"vlc media player",
			"vlc",
			"movies & tv",
			"films & tv"
		};

		private static readonly Dictionary<Keys, Keys> KeyRemapTable = new Dictionary<Keys, Keys>
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
		private static readonly Regex InvalidFileNameCharsRegex = new Regex(
			string.Format("[{0}]", Regex.Escape(new string(Path.GetInvalidFileNameChars()))),
			RegexOptions.Compiled);

		public static bool PowerpointPresent()
		{
			string text = Application.StartupPath + "\\Sys\\~temp.ppt";
			FileUtil.MakeDir(Application.StartupPath + "\\Sys");
			if (!File.Exists(text))
			{
				FileUtil.CreateNewFile(text);
			}
			text = '"' + text + '"';
			string lpResult = new string(' ', ExecutableBufferSize);
			if (FindExecutable(text, "", lpResult) > 32)
			{
				return true;
			}
			return false;
		}

		public static string SetPowerpointPreviewPrefix(SongSettings InItem, bool useTitlePrefix = false)
		{
			if (InItem.Type != "P")
			{
				return "";
			}

			if (useTitlePrefix)
			{
				string prefixItemName = InvalidFileNameCharsRegex.Replace(InItem.Title, "");

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
			InPPT.ResSetAllShowWindows(PowerpointList, TotalPowerpointItems);
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
		private static readonly HashSet<string> navigationErrorReported = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		private static readonly HashSet<string> missingFileReported = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		private static readonly HashSet<string> preloadFailureReported = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		private static bool attachFailureReported;

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
			InMainPPT.FollowRunOf(LivePP, InItem.Path, PowerpointList, TotalPowerpointItems);
			int count = InMainPPT.Count();
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
			for (int i = 1; i <= MaxSlideCount; i++)
			{
				InItem.Slide[i, 0] = -1;
			}
			Array.Fill(InItem.SongVerses, 0, 0, Math.Min(SongVerseInitCount, InItem.SongVerses.Length));
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

		public static int RunPowerpointSong(ref SongSettings InItem, ref PowerPoint InPPT, int StartingSlide, bool ShowResult)
		{
			ResetPowerpointVerses(ref InItem);
			InPPT.displayName = OutputMonitorName;
			SlideShowRunCount++;
			// Dual monitor show only: there the show window is transparent over a PowerPoint item, so the
			// slide show must sit above other applications' windows on the output monitor. In single monitor
			// mode the show window covers the screen and keeps itself on top with its own timer.
			InPPT.KeepSlideShowOnTop = DualMonitorMode;
			if (DualMonitorMode)
			{
				SetSlideShowMonitorBounds(InPPT);
			}

			string text = InPPT.Run(InItem.Path, ref PowerpointList, ref TotalPowerpointItems);
			if (!InPPT.LastRunStarted)
			{
				// The slide show of this item did not start. Nothing below may run: it would act on no slide show
				// at all (before the wrapper remembered its run it moved, measured and read the previous item's
				// slide show, PowerPoint's active presentation, and returned that deck's slide count). 0 slides is
				// what the readers already get when no presentation is open at all, and they cope with it
				// (status "x/0", slide keys find nothing to move). The verse data was reset above, so no verse
				// buttons.
				string runFailure = string.IsNullOrEmpty(text) ? "(no result)" : text;
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

			if (!ShowLiveCam && Gf.DualMonitorSelectAutoOption == 1)
			{
				float scalef = PowerpointShowWindowScale;


				if (DualMonitorMode)
				{
					InPPT.SetShowWindow((float)LS_Left * scalef, (float)LS_Top * scalef, (float)LS_Width * scalef, (float)LS_Height * scalef);
				}
				else
				{
					InPPT.SetShowWindow(LS_Left, LS_Top, (float)LS_Width * scalef, (float)LS_Height * scalef);
				}
			}
			// Once per item, after its last raise (Run, or SetShowWindow in custom output area mode, which may
			// first move the slide show onto the output monitor).
			if (InPPT.KeepSlideShowOnTop)
			{
				ReportSlideShowRaiseError(InPPT, InItem.Path, null);
			}

			InPPT.LoadVersesAndSlides(ref InItem.SongVerses, ref InItem.Slide, SequenceSymbol);
			return InPPT.Count();
		}

		public static void ClearUpPowerpointWindows()
		{
			// Before closing: if a presentation fails to close, its slide show must not stay on top.
			LowerSlideShowWindows("ClearUpPowerpointWindows", wait: false);
			// Show end (and app exit): nothing of this show's PowerPoint state carries over.
			ResetPowerPointShowState();
			string text = LivePP.ClearUpPowerpointWindows(ref PowerpointList, ref TotalPowerpointItems);
			if (text == "")
			{
				LivePP.QuitPowerPointApp(LivePP.prePowerPointApp);
			}
		}

		/// <summary>
		/// Called once when the application exits. Closes the presentations opened for the live show,
		/// quits PowerPoint when nothing else is open in it, and releases every PowerPoint reference the app holds.
		/// All wrappers share one PowerPoint process; releasing every reference here is what was observed
		/// to make the hidden PowerPoint exit when the app closes.
		/// The wrapper objects themselves stay in place so late callers do not hit a null wrapper.
		/// </summary>
		public static void ShutdownPowerpoint(PowerPoint InMainPPT)
		{
			try
			{
				ClearUpPowerpointWindows();
			}
			catch (Exception ex)
			{
				Trace.WriteLine($"ShutdownPowerpoint: clean-up of live presentations failed: {ex.Message}");
			}
			foreach (PowerPoint ppt in new[] { LivePP, PreviewPPT, OutputPPT, ExternalPPT, InMainPPT })
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
			switch (InDirection)
			{
				case KeyDirection.FirstOne:
					return Keys.Left;
				case KeyDirection.PrevOne:
					return Keys.Up;
				case KeyDirection.NextOne:
					return Keys.Down;
				case KeyDirection.LastOne:
					return Keys.Right;
				case KeyDirection.SpaceOne:
					return Keys.Space;
				default:
					return Keys.F5;
			}
		}

		public static bool RunProcess(string InProcessString)
		{
			try
			{
				ProcessStartInfo psi = new ProcessStartInfo
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
				if (!IsWindowVisible(hWnd))
					return true;

				StringBuilder sb = new StringBuilder(WindowTitleBufferSize);
				GetWindowText(hWnd, sb, WindowTitleBufferSize);
				string windowTitle = sb.ToString().ToLower();


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


				ProcessStartInfo psi = new ProcessStartInfo
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
						ShowWindow(foundHandle, SW_MAXIMIZE);
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
				ShowWindow(handle, SW_MAXIMIZE);


				Thread.Sleep(WindowSettleDelayMs);
				RECT windowRect;
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
						ShowWindow(handle, SW_MAXIMIZE);
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
}
