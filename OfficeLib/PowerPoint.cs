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
    public partial class PowerPoint
	{
		// Constants
		private const int NOTES_PLACEHOLDER_INDEX = 2;
		private const int SlideImageMaxDimension = 640;
		private const int MAX_VERSE_NUMBER = 12;
		private const int VERSE_CHORUS = 0;
		private const int VERSE_BRIDGE = 100;
		private const int VERSE_ENDING = 101;
		private const int VERSE_TAG = 102;
		private const int VERSE_WORSHIP = 103;
		private const int VERSE_PRAYER = 111;
		private const int VERSE_QUIET = 112;

		public Application prePowerPointApp;

		public string[,] ScreenDumpFullPath = new string[2, 0];

		// Screen device name of the output monitor, written to PowerPoint's DisplayMonitor setting by Run.
		public string displayName = "None";

		/// <summary>
		/// Attaches this wrapper to PowerPoint when it is not attached yet. A wrapper that already holds a live
		/// reference keeps it (EnsureAttached replaces one that no longer answers). The start-up attach runs on a
		/// worker thread while the UI thread may attach the same wrapper lazily: the first reference stored wins
		/// and the other is released, so no reference is orphaned (an orphaned one keeps PowerPoint alive after
		/// the app exits). Throws when PowerPoint cannot be started.
		/// </summary>
        public void Init()
		{
			Application existing = prePowerPointApp;
			if (existing != null && !existing.IsDisposed)
			{
				return;
			}
			using PowerPointGate gate = PowerPointGate.Enter(PowerPointCallWeight.Heavy, "attach to PowerPoint");
			if (gate == null)
			{
				Console.WriteLine("Init: PowerPoint is not responding; not attached");
				return;
			}
			Application created;
			try
			{
				created = new Application();
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error initializing PowerPoint Application: {ex.Message}");
				throw;
			}
			if (!ReferenceEquals(Interlocked.CompareExchange(ref prePowerPointApp, created, existing), existing))
			{
				// Another thread attached this wrapper meanwhile: keep its reference, release this one.
				try
				{
					created.Dispose();
				}
				catch (Exception ex)
				{
					Console.WriteLine($"Error releasing a duplicate PowerPoint reference: {ex.Message}");
				}
			}
		}

		/// <summary>
		/// The result of Open and Run when PowerPoint was not asked because it does not respond, or their group was
		/// given up: not a failure of the file or the copy (the pre-load does not report the file as not opened).
		/// </summary>
		public const string NotRespondingText = "PowerPoint is not responding";

		public string Open(string InPowerpointFullPath, ref ShowCopyList PowerpointList)
		{
			string showCopyName = "";
			Presentation presentation = null;
			using PowerPointGate gate = PowerPointGate.Enter(PowerPointCallWeight.Heavy, "open " + Path.GetFileName(InPowerpointFullPath));
			if (gate == null)
			{
				return NotRespondingText;
			}

			Presentations probe = null;
			try
			{
				probe = prePowerPointApp.Presentations;
				_ = probe.Count;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error accessing Presentations: {ex.Message}");
				return PowerPointCallLimit.CurrentAbandoned ? NotRespondingText : "Error: PowerPoint application not available";
			}
			finally
			{
				ReleaseProxy(probe);
			}

			try
			{
				// One entry per path: a path whose copy is gone (closed in PowerPoint, or PowerPoint restarted) gets
				// its entry replaced below, so the dead name cannot hide the new copy.
				int entry = FindListedEntry(InPowerpointFullPath, PowerpointList);
				showCopyName = (entry > 0) ? PowerpointList[entry].CopyName : "";
				bool isShowCopyOpen = entry > 0 && IsShowCopyOpen(showCopyName, PowerpointList[entry].SourcePath);
				// NetOffice disposes the proxies made from a proxy with it, so the collection is released only after
				// (or together with) the presentation, never while the presentation is kept for a later close.
				Presentations openTarget = null;
				try
				{
					if (!isShowCopyOpen)
					{
						openTarget = prePowerPointApp.Presentations;
						presentation = openTarget.Open(InPowerpointFullPath, MsoTriState.msoTrue, MsoTriState.msoTrue);
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
                            // The copy stays open in PowerPoint; the proxies are not needed any more (later calls look
                            // the copy up by name and mark).
                            ReleaseProxy(presentation);
                            presentation = null;
                        }
						ReleaseProxy(openTarget);
						openTarget = null;
					}
				}
				catch (Exception ex)
				{
					Console.WriteLine($"Error opening presentation '{InPowerpointFullPath}': {ex.Message}");
					// Close what was opened before the failure.
					if (presentation != null)
					{
						if (PowerPointCallLimit.CurrentAbandoned)
						{
							// Given up: closed (then released) when PowerPoint answers again. Its collection proxy is
							// kept too (releasing it would release the presentation's proxy with it).
							presentationsToClose.Add(presentation);
							openTarget = null;
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
					ReleaseProxy(openTarget);
					return PowerPointCallLimit.CurrentAbandoned ? NotRespondingText : "Error Loading: " + showCopyName;
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error in Open method: {ex.Message}");
				return PowerPointCallLimit.CurrentAbandoned ? NotRespondingText : "Error Loading: " + showCopyName;
			}
			return "";
		}

		public string Run(string InPowerpointFullPath, ref ShowCopyList PowerpointList)
		{
			string showCopyName = "";
			string inspectedPresentations = "";
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

				// The output monitor for PowerPoint's slide show. "None" (no output monitor configured) is not a monitor:
				// writing it would only overwrite the user's own PowerPoint setting.
				if (!string.IsNullOrWhiteSpace(displayName) && !string.Equals(displayName, "None", StringComparison.OrdinalIgnoreCase))
				{
					SaveRegValue(version, @"PowerPoint\Options", "DisplayMonitor", displayName);
				}

				int entry = FindListedEntry(InPowerpointFullPath, PowerpointList);
				showCopyName = (entry > 0) ? PowerpointList[entry].CopyName : "";
				inspectedPresentations = "";
				if (!(showCopyName != ""))
				{
					return "Cannot find opened Presentation: " + PowerpointList.Count + ", " + InPowerpointFullPath;
				}
				string mark = ShowCopyMark(PowerpointList[entry].SourcePath);
				bool unmarked = false;

				// Every proxy read through the collection is released with it at the end of the block.
				Application app = prePowerPointApp;
				// ReleaseProxy: kept instead of released when the group was given up (a release would wait).
				Presentations presentations = app.Presentations;
				// A presentation that cannot be read (not gone, not busy) is skipped: the copy may be another one.
				Exception unreadable = null;
				try
				{
					int count = presentations.Count;
					for (int i = 1; i <= count && !PowerPointCallLimit.CurrentAbandoned; i++)
					{
						Presentation candidate = presentations[i];
						string fullName = ReadCandidateName(() => candidate.FullName, ref unreadable);
						PowerPointCallLimit.Progress();
						if (fullName == null)
						{
							continue;
						}
						// For the failure text only: FullName is already read (no extra call per presentation).
						inspectedPresentations = inspectedPresentations + i + "(" + fullName + ") ";
						if (string.Equals(fullName, showCopyName, StringComparison.OrdinalIgnoreCase))
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
									currentSlideShowProcessId = 0;
									if (currentSlideShowWindow != IntPtr.Zero)
									{
										GetWindowThreadProcessId(currentSlideShowWindow, out currentSlideShowProcessId);
									}
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
					return "The copy opened for this item is no longer open (the presentation named " + showCopyName + " was not opened by Axicultus)";
				}
				if (unreadable != null)
				{
					return "Can't find Presentations: " + showCopyName + " (a presentation could not be read: " + unreadable.GetBaseException().Message + ").  " + inspectedPresentations;
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
			return "Can't find Presentations: " + showCopyName + ".  " + inspectedPresentations;
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
		private const string ShowCopyTagName = "AXICULTUS_SHOWCOPY";
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
		/// mark. A presentation that cannot be read is skipped; when the copy is not found and one could not be
		/// read, or PowerPoint cannot be read at all, it throws (the caller reports the file as not opened).
		/// </summary>
		private bool IsShowCopyOpen(string fullName, string path)
		{
			if (string.IsNullOrEmpty(fullName))
			{
				return false;
			}
			string mark = ShowCopyMark(path);
			Presentations presentations = prePowerPointApp.Presentations;
			Exception unreadable = null;
			try
			{
				int count = presentations.Count;
				for (int i = 1; i <= count && !PowerPointCallLimit.CurrentAbandoned; i++)
				{
					Presentation candidate = presentations[i];
					string candidateName = ReadCandidateName(() => candidate.FullName, ref unreadable);
					PowerPointCallLimit.Progress();
					if (!string.Equals(candidateName, fullName, StringComparison.OrdinalIgnoreCase))
					{
						continue;
					}
					try
					{
						if (HasShowCopyMark(candidate, mark))
						{
							return true;
						}
					}
					catch (Exception ex) when (!IsPowerPointGone(ex) && !IsPowerPointBusy(ex))
					{
						unreadable ??= ex;
					}
				}
				if (unreadable != null)
				{
					throw new InvalidOperationException("A PowerPoint presentation could not be read: " + unreadable.GetBaseException().Message, unreadable);
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
					// Busy, or the call was cancelled by the time limit: PowerPoint is still there, so the reference
					// and the run are kept (re-attaching now would also fail: the group is given up).
					if (IsPowerPointBusy(ex) || HasHResult(ex, RPC_E_CALL_CANCELED) || PowerPointCallLimit.CurrentAbandoned)
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
		private const int RPC_E_CALL_CANCELED = unchecked((int)0x80010002);
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

		/// <summary>Reads the actual slide of the owned show without advancing animations or changing navigation state.</summary>
		public int? ReadRunSlideNumber()
		{
			if (CurrentRunPresentationName == null || PowerPointCallLimit.Current != null)
				return null;
			using PowerPointGate gate = PowerPointGate.Enter(PowerPointCallWeight.Light, "read the live slide", capMs: 300);
			if (gate == null) return null;
			try
			{
				using RunPresentation run = FindRunPresentation();
				if (run == null) return null;
				var view = run.Presentation.SlideShowWindow.View;
				return view.State == PpSlideShowState.ppSlideShowRunning ? view.Slide.SlideIndex : null;
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine(ex.Message);
				return null;
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


        public void SetShowWindow(float Left, float Top, float Width, float Height)
        {
			using PowerPointGate gate = PowerPointGate.Enter(PowerPointCallWeight.Light, "place the slide show window");
			if (gate == null)
			{
				// Not asked: nothing is known about the slide show (see LastCallNotRunning).
				LastCallNotRunning = false;
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
		/// Show end: closes the presentations this app opened for the show (listed name and mark), then quits
		/// PowerPoint when nothing else is open in it. Returns a summary for the log.
		/// </summary>
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

			int trackedCopyCount = PowerpointList.Count;
            int openPresentationCount = 0;
			int closedCopyCount = 0;
			int unreadable = 0;
			string firstUnreadableError = null;
			bool enumerationFailed = false;
			HashSet<string> closedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			HashSet<string> failedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
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
					openPresentationCount = presentations.Count;
					PowerPointCallLimit.Progress();
					for (int presentationIndex = openPresentationCount; presentationIndex > 0; presentationIndex--)
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
							openPresentation = presentations[presentationIndex];
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
								Console.WriteLine($"Show end: reading presentation {presentationIndex} was cancelled: {ex.GetBaseException().Message}");
								break;
							}
							// Not known whether it is one of the copies: left open, reported below.
							unreadable++;
							firstUnreadableError ??= ex.GetBaseException().Message;
							Console.WriteLine($"Error reading presentation {presentationIndex} at show end: {ex.Message}");
							continue;
						}
						try
						{
							openPresentation.Close();
							closedCopyCount++;
							closedNames.Add(fullName);
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
							failedNames.Add(fullName);
							Console.WriteLine($"Error closing presentation '{fullName}': {ex.Message}");
						}
					}
				}
			}
			catch (Exception ex)
			{
				enumerationFailed = true;
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
				return "Abandoned: Opened PP=" + openPresentationCount + ", Total On List=" + trackedCopyCount + ", Numbers Closed=" + closedCopyCount;
			}
			if (unreadable > 0 && LastCloseError == null)
			{
				LastCloseError = unreadable + " open presentation(s) could not be read: " + firstUnreadableError;
			}
			// Only rows that may still be open stay listed, for the caller to try again later: copies whose Close
			// failed, and, when some presentations (or the collection) could not be read, every copy not closed
			// (it may be one of those). A copy that is positively gone or closed is dropped.
			KeepUnclosedRows(PowerpointList, failedNames, closedNames, keepAllNotClosed: unreadable > 0 || enumerationFailed);

			// Quit PowerPoint when nothing else is open in it (see QuitPowerPointApp).
			if (quitWhenEmpty)
			{
				QuitPowerPointApp(prePowerPointApp);
			}

			return "Opened PP=" + openPresentationCount + ", Total On List=" + trackedCopyCount + ", Numbers Closed=" + closedCopyCount + (enumerationFailed ? ". Error When Closing" : ". No Error");
		}

		private static void KeepUnclosedRows(ShowCopyList list, HashSet<string> failedNames, HashSet<string> closedNames, bool keepAllNotClosed)
		{
			List<ShowCopy> keep = new List<ShowCopy>();
			for (int i = 1; i <= list.Count; i++)
			{
				ShowCopy row = list[i];
				if (string.IsNullOrEmpty(row.CopyName) || string.IsNullOrEmpty(row.SourcePath))
				{
					continue;
				}
				if (failedNames.Contains(row.CopyName) || (keepAllNotClosed && !closedNames.Contains(row.CopyName)))
				{
					keep.Add(row);
				}
			}
			list.Clear();
			foreach (ShowCopy row in keep)
			{
				list.Add(row.SourcePath, row.CopyName);
			}
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




		/// <summary>
		/// Attaches this wrapper to PowerPoint again: a new Application reference first, then the old one is
		/// released. When attaching fails (or PowerPoint does not respond) the old reference is kept.
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
			catch (Exception ex)
			{
				// The old reference is kept (see above).
				Console.WriteLine($"Error attaching to PowerPoint: {ex.Message}");
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
                // Not asked: nothing is known about the slide show (see LastCallNotRunning).
                LastCallNotRunning = false;
                return;
            }
            try
            {
                // Inside the try: reading PowerPoint can throw (e.g. PowerPoint has exited).
                using RunPresentation run = FindRunSlideShow(out SlideShowWindow slideShowWindow);
                if (run == null) return;

                // Debounce repeated key presses.
                if ((DateTime.Now - lastClickTime).TotalMilliseconds < 500) return;
                lastClickTime = DateTime.Now;

                var view = slideShowWindow.View;

                // Move only when a different slide is asked for: going to the current slide resets its animations.
                if (slideNumber > 0 && view.Slide.SlideIndex != slideNumber)
                {
                    view.GotoSlide(slideNumber, MsoTriState.msoTrue);
                }

                slideShowWindow.Activate();

                // Read after a possible GotoSlide. Clicks, not effects: MainSequence.Count also counts
                // "With/After Previous" effects, so a slide with only automatic effects looked unfinished and
                // Next() moved the show to the next slide.
                int totalClicks = view.GetClickCount();
                int currentIndex = view.GetClickIndex();

                if (currentIndex < totalClicks)
                {
                    view.Next();
                    System.Diagnostics.Debug.WriteLine($"Playing animation {currentIndex + 1} of {totalClicks}");
                }
                else if (totalClicks > 0)
                {
                    // Every click played: start the slide's animations again from the first click.
                    System.Diagnostics.Debug.WriteLine("Animation finished. Replaying from the first click.");
                    view.GotoClick(0);
                    view.Next();
                }
                else
                {
                    // No click effects: Next() would leave the slide. Replay its automatic animations instead.
                    System.Diagnostics.Debug.WriteLine("No click animations. Replaying the slide.");
                    view.GotoSlide(view.Slide.SlideIndex, MsoTriState.msoTrue);
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
				// Not asked: nothing is known about the slide show (see LastCallNotRunning).
				LastCallNotRunning = false;
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
				// The same parser as the image builds (one notes read per slide, slides without a notes body skipped).
				ParseVersesFromPresentation(presentation, presentation.Slides.Count, ref SongVerses, ref Slide, SequenceSymbol);
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
				int verseNumber = Convert.ToInt32(InString);
				if (verseNumber >= 0 && verseNumber <= MAX_VERSE_NUMBER)
				{
					return verseNumber;
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
		/// Parses the verse markers in the slide notes into SongVerses and Slide.
		/// Returns false when a slide could not be read (its marker is then missing): such a result must not be
		/// cached. A slide whose notes page has no body placeholder is a slide without a marker, not an error:
		/// it is recognised by the placeholder count (reading the missing placeholder fails with the same generic
		/// "integer out of range" error as other bad reads, so the error itself cannot tell them apart).
		/// </summary>
		private bool ParseVersesFromPresentation(_Presentation presentation, int totalSlides, ref int[] SongVerses, ref int[,] Slide, string[] SequenceSymbol)
		{
			EnsureSlideMarkerCapacity(ref Slide, totalSlides);
			bool foundChorus = false;
			bool complete = true;
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
								if (!foundChorus)
								{
									Slide[i, 0] = verseIndicator;
									SongVerses[verseIndicator] = i;
									foundChorus = true;
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

		/// <summary>
		/// True when the slide images under FilePrefix are a complete export of the current version of FilePath
		/// (see SlideImageManifest); TotalSlides is then their number. Reads only the build record and checks
		/// that the image files exist: PowerPoint is never opened. Never throws.
		/// </summary>
		public bool IsBuildedFileCheck(string FilePath, string FilePrefix, ref int TotalSlides)
		{
			if (string.IsNullOrEmpty(FilePath) || string.IsNullOrEmpty(FilePrefix))
			{
				return false;
			}
			if (!SlideImageManifest.TryMatch(FilePrefix, FilePath, out int slideCount))
			{
				return false;
			}
			TotalSlides = slideCount;
			return true;
		}

		public bool IsBuildedFileCheck(string filePath, string filePrefix, ref int totalSlides, ref int[,] slides)
		{
			int count = 0;
			if (!IsBuildedFileCheck(filePath, filePrefix, ref count))
				return false;
			EnsureSlideMarkerCapacity(ref slides, count);
			totalSlides = count;
			return true;
		}

		public int Count(ref int[,] slides)
		{
			int count = Count();
			EnsureSlideMarkerCapacity(ref slides, count);
			return count;
		}

		/// <summary>
		/// Builds the output slide images of FilePath under FilePrefix and copies them under the matching preview
		/// prefix, unless a complete export of this version exists; then fills the verse data. Returns false when
		/// PowerPoint was not asked (not attached, not responding) or the build failed.
		/// </summary>
		public bool BuildScreenOutDumps(string FilePath, string FilePrefix, ref int TotalSlides, int MAX_VERSES, int DB_MAXSLIDES, ref int[] SongVerses, ref int[,] Slide, string[] SequenceSymbol)
		{
			return BuildSlideImages(FilePath, FilePrefix, copyToPreview: true, ref TotalSlides, MAX_VERSES, DB_MAXSLIDES, ref SongVerses, ref Slide, SequenceSymbol);
		}

		private const string OutputImagePrefixMarker = "~OUTPPPreview$";
		private const string PreviewImagePrefixMarker = "~PREPPPreview$";

		/// <summary>
		/// The one export routine of the slide images (output and preview).
		/// - Reuses the images when their build record matches this version of the file (no PowerPoint call
		///   except reading the verse markers, which the session cache usually answers).
		/// - Otherwise removes the record first, exports every slide, and writes the record only when all slides
		///   were exported: an interrupted export is rebuilt next time instead of showing stale slides.
		/// - Works on a local presentation, never the shared field: a COM call pumps messages, so another flow
		///   could otherwise close or replace it meanwhile.
		/// </summary>
		private bool BuildSlideImages(string FilePath, string FilePrefix, bool copyToPreview, ref int TotalSlides, int MAX_VERSES, int DB_MAXSLIDES, ref int[] SongVerses, ref int[,] Slide, string[] SequenceSymbol)
		{
			using PowerPointGate gate = PowerPointGate.Enter(PowerPointCallWeight.Heavy, "build the slide images of " + Path.GetFileName(FilePath));
			if (gate == null)
			{
				ResetVerses(MAX_VERSES, DB_MAXSLIDES, ref SongVerses, ref Slide);
				return false;
			}
			_Presentation working = null;
			try
			{
				ResetVerses(MAX_VERSES, DB_MAXSLIDES, ref SongVerses, ref Slide);
				if (prePowerPointApp == null)
				{
					Console.WriteLine("[BuildSlideImages] PowerPoint is not attached");
					return false;
				}
				string previewPrefix = copyToPreview && FilePrefix.Contains(OutputImagePrefixMarker)
					? FilePrefix.Replace(OutputImagePrefixMarker, PreviewImagePrefixMarker)
					: null;
				if (IsBuildedFileCheck(FilePath, FilePrefix, ref TotalSlides, ref Slide)
					&& (previewPrefix == null || SlideImageManifest.TryMatch(previewPrefix, FilePath, out _)))
				{
					Console.WriteLine($"[BuildSlideImages] Reusing {TotalSlides} slide images: {FilePrefix}");
					LoadVerses(FilePath, MAX_VERSES, DB_MAXSLIDES, ref SongVerses, ref Slide, SequenceSymbol);
					return true;
				}

				// Read before the file is opened: a save during the export leaves the record older than the file.
				SlideImageManifest.SourceVersion source = SlideImageManifest.ReadSource(FilePath);
				DateTime? lastWriteBeforeRead = GetLastWriteBeforeRead(FilePath);
				SlideImageManifest.Invalidate(FilePrefix);
				if (previewPrefix != null)
				{
					SlideImageManifest.Invalidate(previewPrefix);
				}

				// Read-only and titled (a cancelled open can be closed later, see OpenTemporary): the user's file is
				// never locked for writing.
				working = OpenTemporary(FilePath, MsoTriState.msoTrue, MsoTriState.msoFalse);
				int slideCount = working.Slides.Count;
				var exportSize = GetSlideExportSize(working);
				EnsureSlideMarkerCapacity(ref Slide, slideCount);
				TotalSlides = slideCount;
				Console.WriteLine($"[BuildSlideImages] Exporting {slideCount} slides to: {FilePrefix}");
				bool previewComplete = previewPrefix != null;
				for (int i = 1; i <= slideCount; i++)
				{
					string imagePath = SlideImageManifest.ImagePath(FilePrefix, i);
					working.Slides[i].Export(imagePath, "JPG", exportSize.Width, exportSize.Height);
					PowerPointCallLimit.Progress();
					if (previewPrefix != null && !CopySlideImage(imagePath, SlideImageManifest.ImagePath(previewPrefix, i)))
					{
						previewComplete = false;
					}
				}
				if (PowerPointCallLimit.CurrentAbandoned)
				{
					return false;
				}
				SlideImageManifest.Commit(FilePrefix, source, slideCount);
				if (previewComplete)
				{
					SlideImageManifest.Commit(previewPrefix, source, slideCount);
				}

				bool complete = ParseVersesFromPresentation(working, slideCount, ref SongVerses, ref Slide, SequenceSymbol);
				StoreCachedVerses(FilePath, lastWriteBeforeRead, complete, slideCount, MAX_VERSES, SongVerses, Slide);
				return true;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error building the slide images of '{FilePath}': {ex.Message}");
				return false;
			}
			finally
			{
				ClosePresentation(ref working);
			}
		}

		private static System.Drawing.Size GetSlideExportSize(_Presentation presentation)
		{
			PageSetup page = null;
			try
			{
				page = presentation.PageSetup;
				return GetSlideExportSize(page.SlideWidth, page.SlideHeight);
			}
			finally
			{
				ReleaseProxy(page);
			}
		}

		internal static System.Drawing.Size GetSlideExportSize(double width, double height)
		{
			if (!double.IsFinite(width) || !double.IsFinite(height) || width <= 0 || height <= 0)
				return new System.Drawing.Size(SlideImageMaxDimension, SlideImageMaxDimension * 3 / 4);

			double longest = Math.Max(width, height);
			return new System.Drawing.Size(
				Math.Max(1, (int)Math.Round(width / longest * SlideImageMaxDimension)),
				Math.Max(1, (int)Math.Round(height / longest * SlideImageMaxDimension)));
		}

		// Copies an exported slide image to the preview prefix. One retry for a file briefly held by another
		// process (a virus scanner, a thumbnail reader). False when it could not be copied: that preview set then
		// gets no build record and is exported again next time.
		private static bool CopySlideImage(string from, string to)
		{
			for (int attempt = 1; ; attempt++)
			{
				try
				{
					File.Copy(from, to, true);
					return true;
				}
				catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
				{
					if (attempt >= 2)
					{
						Console.WriteLine($"Copying '{from}' to the preview failed: {ex.Message}");
						return false;
					}
					Thread.Sleep(50);
				}
			}
		}

		/// <summary>
		/// Clears markers in the allocated rows, including the last slide. DB_MAXSLIDES is retained for existing callers.
		/// </summary>
		public static void ResetVerses(int MAX_VERSES, int DB_MAXSLIDES, ref int[] SongVerses, ref int[,] Slide)
		{
			for (int i = 1; i < Slide.GetLength(0); i++)
			{
				Slide[i, 0] = -1;
			}
			Array.Clear(SongVerses, 0, Math.Min(SongVerses.Length, MAX_VERSES + 1));
		}

		private static void EnsureSlideMarkerCapacity(ref int[,] slides, int slideCount)
		{
			int previousRows = slides.GetLength(0);
			if (slideCount < previousRows)
				return;
			var expanded = new int[checked(slideCount + 1), slides.GetLength(1)];
			Array.Copy(slides, expanded, slides.Length);
			// A missing note has no verse marker. Zero denotes a chorus.
			for (int row = Math.Max(1, previousRows); row <= slideCount; row++)
				expanded[row, 0] = -1;
			slides = expanded;
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
		internal static void StoreCachedVerses(string FilePath, DateTime? lastWriteBeforeRead, bool complete, int totalSlides, int MAX_VERSES, int[] SongVerses, int[,] Slide)
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
			EnsureSlideMarkerCapacity(ref Slide, entry.SlideTypes.Length - 1);
			Array.Copy(entry.SongVerses, SongVerses, Math.Min(Math.Min(entry.SongVerses.Length, SongVerses.Length), MAX_VERSES + 1));
			for (int i = 1; i < entry.SlideTypes.Length; i++)
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
		/// Builds the preview slide images of FilePath under FilePrefix, unless a complete export of this version
		/// exists; then fills the verse data (see BuildSlideImages).
		/// </summary>
		public bool BuildScreenDumps(string FilePath, string FilePrefix, ref int TotalSlides, int MAX_VERSES, int DB_MAXSLIDES, ref int[] SongVerses, ref int[,] Slide, string[] SequenceSymbol)
		{
			return BuildSlideImages(FilePath, FilePrefix, copyToPreview: false, ref TotalSlides, MAX_VERSES, DB_MAXSLIDES, ref SongVerses, ref Slide, SequenceSymbol);
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
				if (!string.IsNullOrEmpty(CacheFolder))
				{
					Directory.CreateDirectory(CacheFolder);
				}
				int count = Math.Clamp(MaxFileCount, 0, InFileList.Length);
				var paths = new string[2, count];
				List<string> listed = new List<string>(count);
				for (int i = 0; i < count; i++)
				{
					paths[0, i] = InFileList[i];
					paths[1, i] = ExternalPreviewCache.GetPreviewPath(CacheFolder, InFileList[i]);
					listed.Add(paths[1, i]);
				}
				ExternalPreviewCache.Prune(CacheFolder, listed, DateTime.UtcNow);
				firstScreenDumpFolder = CacheFolder;
				failedFirstScreenDumps.Clear();
				ScreenDumpFullPath = paths;
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
			int screenDumpIndex = PPNumbering - 1;
			if (screenDumpIndex < 0 || screenDumpIndex >= ScreenDumpFullPath.GetLength(1) || ScreenDumpFullPath[0, screenDumpIndex] == null || string.IsNullOrEmpty(ScreenDumpFullPath[1, screenDumpIndex]))
			{
				return false;
			}
			string presentationPath = ScreenDumpFullPath[0, screenDumpIndex];
			string previewPath = ScreenDumpFullPath[1, screenDumpIndex];
			if (File.Exists(previewPath) || ExternalPreviewCache.AdoptPendingExport(previewPath))
			{
				return true; // this version was exported before (or an earlier export finished late)
			}
			if (failedFirstScreenDumps.Contains(previewPath) || firstScreenDumpRunning)
			{
				return false;
			}
			firstScreenDumpRunning = true;
			try
			{
				if (prePowerPointApp == null)
				{
					// Not attached at start-up (PowerPoint failed to start then): attach now, on the UI thread.
					try
					{
						Init();
					}
					catch (Exception ex)
					{
						Console.WriteLine($"BuildOneFirstScreenDump: attaching to PowerPoint failed: {ex.Message}");
					}
					if (prePowerPointApp == null)
					{
						return false;
					}
				}
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
					var exportSize = GetSlideExportSize(tempPresentation);
					slide.Export(tempPath, "JPG", exportSize.Width, exportSize.Height);

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
            if (untitled == MsoTriState.msoFalse && IsFileOpenInPowerPoint(path))
            {
                // The file is open in PowerPoint already (the user's own window, most likely). A titled open could
                // hand back that very presentation, and the caller closes what it opened: the user's document would
                // be closed without a save prompt. An untitled copy is always a new presentation.
                untitled = MsoTriState.msoTrue;
            }
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

        /// <summary>
        /// True when a presentation whose FullName is path is open in this wrapper's PowerPoint, or when that cannot
        /// be told (a presentation could not be read): the safe answer for OpenTemporary.
        /// </summary>
        private bool IsFileOpenInPowerPoint(string path)
        {
            string fullPath;
            try
            {
                fullPath = Path.GetFullPath(path);
            }
            catch (Exception)
            {
                return true;
            }
            Presentations presentations = null;
            try
            {
                presentations = prePowerPointApp.Presentations;
                int count = presentations.Count;
                for (int i = 1; i <= count; i++)
                {
                    Presentation open = null;
                    try
                    {
                        open = presentations[i];
                        if (string.Equals(open.FullName, fullPath, StringComparison.OrdinalIgnoreCase))
                        {
                            return true;
                        }
                    }
                    finally
                    {
                        ReleaseProxy(open);
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Could not check whether '{path}' is open in PowerPoint: {ex.Message}");
                return true;
            }
            finally
            {
                ReleaseProxy(presentations);
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
                    ReleaseProxy(presentation as IDisposable);
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
					// Quit closes every open presentation without asking to save, so only quit when none is open,
					// no file is open in Protected View (those are not in Presentations), and the process was not
					// already running before Axicultus started (the user's PowerPoint, perhaps on its start screen).
					if (GetOpenPresentationCount(prePowerPointApp) == 0 && !PowerPointCallLimit.CurrentAbandoned
						&& GetProtectedViewWindowCount(prePowerPointApp) == 0 && !PowerPointCallLimit.CurrentAbandoned
						&& !IsUsersOwnPowerPoint(prePowerPointApp))
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
					// Released after Quit, or kept when the show-end group was given up (see ReleaseProxy).
					ReleaseProxy(prePowerPointApp);
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

		/// <summary>
		/// The number of files open in Protected View in the given PowerPoint (they are not in Presentations), or -1
		/// when it cannot be read: then PowerPoint is not quit.
		/// </summary>
		private static int GetProtectedViewWindowCount(Application app)
		{
			ProtectedViewWindows windows = null;
			try
			{
				windows = app.ProtectedViewWindows;
				int count = windows.Count;
				PowerPointCallLimit.Progress();
				return count;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error reading Protected View windows: {ex.Message}");
				return -1;
			}
			finally
			{
				ReleaseProxy(windows);
			}
		}

		/// <summary>
		/// True when the PowerPoint behind app is the user's own: its process was already running when this app
		/// started and it is visible (e.g. on its start screen). Axicultus must never quit it. False when it started
		/// later (Axicultus started it), when it is hidden (a PowerPoint left behind by an earlier run of the app),
		/// or when that cannot be told (then the presentation checks alone decide, as before).
		/// </summary>
		private static bool IsUsersOwnPowerPoint(Application app)
		{
			try
			{
				if (app.Visible != MsoTriState.msoTrue)
				{
					return false;
				}
				IntPtr hwnd = new IntPtr(app.HWND);
				if (hwnd == IntPtr.Zero || GetWindowThreadProcessId(hwnd, out uint processId) == 0 || processId == 0)
				{
					return false;
				}
				using Process powerPoint = Process.GetProcessById((int)processId);
				using Process self = Process.GetCurrentProcess();
				return powerPoint.StartTime < self.StartTime;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Could not tell when PowerPoint was started: {ex.Message}");
				return false;
			}
		}
	}
}
