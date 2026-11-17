using System;
using System.Diagnostics;
using System.Threading;

namespace OfficeLib
{
	/// <summary>How a group of PowerPoint calls is limited (fix-ppt-show-call-timeout P2).</summary>
	public enum PowerPointCallWeight
	{
		/// <summary>Slide movement, counts, state lookups, window moves: no probe, given up after LightStallMs without a completed call.</summary>
		Light,
		/// <summary>Opening files, starting PowerPoint, running a slide show, exporting slide images: probe first, then HeavyStallMs.</summary>
		Heavy
	}

	/// <summary>
	/// The one gate every group of PowerPoint calls made on the UI thread passes (fix-ppt-show-call-timeout P1-P3).
	/// The wrapper's public methods enter it; a caller that makes several calls as one piece of work (the pre-load,
	/// an item run) enters it around them, and the calls inside then belong to that group (a nested Enter returns a
	/// scope that does nothing, or null once the group was given up). The show-end and app-exit groups of
	/// fix-ppt-show-end-timeout begin their PowerPointCallLimit themselves; calls inside them are nested here and
	/// keep that behaviour.
	///
	/// - UI thread only: the gate works on an STA thread (the UI thread, the only STA thread that calls PowerPoint).
	///   On any other thread (the start-up Init on a worker thread) Enter is a pass-through: no probe, no limit, no
	///   state change, as before this gate. So the gate's static state is read and written by the UI thread only.
	/// - Light group: no probe; PowerPointCallLimit with LightStallMs without a completed call.
	/// - Heavy group: ProbePowerPoint (HeavyProbeMs) first, unless a group completed or a probe answered within
	///   RecentAnswerMs; then HeavyStallMs. When the probe finds no PowerPoint process, the call starts PowerPoint:
	///   StartStallMs. The whole-group cap (HeavyCapMs) is long enough not to be a factor: heavy groups are limited
	///   by the stall time.
	/// - A group that is given up, or a failed probe, puts PowerPoint in the "not responding" state (also set from
	///   the show-end package, NoteNotResponding). While it is set, a group asks again (ReprobeMs) at most every
	///   ReprobeIntervalMs and is skipped at once otherwise (Enter returns null: the caller returns its existing
	///   "could not reach PowerPoint" result). An answer clears the state: the presentations a given-up group left
	///   open are closed first (PowerPoint.CloseLeftOpenPresentations), then the proxies kept unreleased are
	///   released. Every change raises StateChanged.
	/// </summary>
	public sealed class PowerPointGate : IDisposable
	{
		public const int LightStallMs = 5000;
		public const int LightCapMs = 30000;
		public const int HeavyProbeMs = 2000;
		public const int HeavyStallMs = 30000;
		public const int HeavyCapMs = 30 * 60 * 1000;
		public const int StartStallMs = 60000;
		public const int ReprobeMs = 300;
		public const int ReprobeIntervalMs = 2000;
		// A heavy group within this time of the last answer is not probed first.
		public const int RecentAnswerMs = 1000;
		// A group that takes longer than this is logged (Trace) with its time, also when it completed.
		public const int SlowGroupLogMs = 2000;

		private static readonly PowerPointGate Nested = new PowerPointGate(null, null, PowerPointCallWeight.Light);
		private static readonly Stopwatch Clock = Stopwatch.StartNew();
		private static long lastProbeMs = long.MinValue / 2;
		// When a group last completed without being given up, or a probe last answered.
		private static long lastAnswerMs = long.MinValue / 2;

		/// <summary>True while PowerPoint is believed not to answer (see the class remarks).</summary>
		public static bool NotResponding { get; private set; }

		/// <summary>The state changed: (notResponding, reason). Raised on the UI thread.</summary>
		public static event Action<bool, string> StateChanged;

		/// <summary>Log lines of the gate (skips, slow groups).</summary>
		public static event Action<string> Trace;

		private readonly PowerPointCallLimit limit;
		private readonly string what;
		private readonly PowerPointCallWeight weight;

		private PowerPointGate(PowerPointCallLimit limit, string what, PowerPointCallWeight weight)
		{
			this.limit = limit;
			this.what = what;
			this.weight = weight;
		}

		private static bool OnUiThread => Thread.CurrentThread.GetApartmentState() == ApartmentState.STA;

		/// <summary>
		/// Enters the gate for a group of calls. Returns null when the group must not run (PowerPoint is not
		/// responding, or the group this call belongs to was given up): the caller then makes no PowerPoint call and
		/// returns its "could not reach PowerPoint" result. Dispose the returned scope when the group ends.
		/// capMs: the most the whole group may take (0: the weight's default).
		/// </summary>
		public static PowerPointGate Enter(PowerPointCallWeight weight, string what, int capMs = 0)
		{
			if (!OnUiThread)
			{
				return Nested;
			}
			PowerPointCallLimit current = PowerPointCallLimit.Current;
			if (current != null)
			{
				return current.Abandoned ? null : Nested;
			}
			if (NotResponding)
			{
				if (Clock.ElapsedMilliseconds - lastProbeMs < ReprobeIntervalMs)
				{
					OnTrace("PowerPoint gate: '" + what + "' skipped (PowerPoint is not responding)");
					return null;
				}
				PowerPoint.PowerPointAnswer again = PowerPoint.ProbePowerPoint(ReprobeMs, out string againDetail);
				lastProbeMs = Clock.ElapsedMilliseconds;
				if (again == PowerPoint.PowerPointAnswer.NotResponding || again == PowerPoint.PowerPointAnswer.NoWindow)
				{
					// Not answered, or not known (NoWindow): nothing is released or closed on that.
					OnTrace("PowerPoint gate: '" + what + "' skipped (asked again: " + againDetail + ")");
					return null;
				}
				if (!Recover("PowerPoint answers again (" + againDetail + ")"))
				{
					OnTrace("PowerPoint gate: '" + what + "' skipped (PowerPoint stopped answering again)");
					return null;
				}
			}
			int stallMs;
			if (weight == PowerPointCallWeight.Heavy)
			{
				if (Clock.ElapsedMilliseconds - lastAnswerMs < RecentAnswerMs)
				{
					// A group completed (or a probe answered) a moment ago: not asked again; the stall limit applies.
					stallMs = HeavyStallMs;
				}
				else
				{
					PowerPoint.PowerPointAnswer answer = PowerPoint.ProbePowerPoint(HeavyProbeMs, out string detail);
					lastProbeMs = Clock.ElapsedMilliseconds;
					if (answer == PowerPoint.PowerPointAnswer.NotResponding)
					{
						SetState(true, "'" + what + "' not started: PowerPoint did not answer (" + detail + ")");
						return null;
					}
					// No PowerPoint process: the group starts PowerPoint, which can take long.
					stallMs = answer == PowerPoint.PowerPointAnswer.NotRunning ? StartStallMs : HeavyStallMs;
					if (answer == PowerPoint.PowerPointAnswer.Answers)
					{
						lastAnswerMs = lastProbeMs;
					}
				}
			}
			else
			{
				stallMs = LightStallMs;
			}
			int cap = capMs > 0 ? capMs : (weight == PowerPointCallWeight.Heavy ? HeavyCapMs : LightCapMs);
			PowerPointCallLimit begun = PowerPointCallLimit.Begin(stallMs, Math.Max(cap, stallMs));
			return begun == null ? Nested : new PowerPointGate(begun, what, weight);
		}

		/// <summary>
		/// The show-end package (fix-ppt-show-end-timeout) found PowerPoint not answering or gave its calls up: the
		/// state is set as if a group had been given up, so the next group is skipped by the ReprobeIntervalMs rule
		/// instead of probing again at once. UI thread only.
		/// </summary>
		public static void NoteNotResponding(string reason)
		{
			if (!OnUiThread)
			{
				return;
			}
			lastProbeMs = Clock.ElapsedMilliseconds;
			SetState(true, reason);
		}

		public void Dispose()
		{
			if (limit == null)
			{
				return;
			}
			bool abandoned = limit.Abandoned;
			long elapsed = limit.ElapsedMilliseconds;
			long sinceProgress = limit.SinceProgressMilliseconds;
			limit.Dispose();
			if (abandoned)
			{
				lastProbeMs = Clock.ElapsedMilliseconds;
				SetState(true, "'" + what + "' given up after " + elapsed + " ms (" + sinceProgress + " ms after the last completed call)");
				return;
			}
			lastAnswerMs = Clock.ElapsedMilliseconds;
			if (elapsed >= SlowGroupLogMs)
			{
				OnTrace("PowerPoint gate: '" + what + "' (" + weight + ") took " + elapsed + " ms");
			}
		}

		/// <summary>
		/// PowerPoint answered again: closes the presentations given-up groups left open (one light group), releases
		/// the proxies kept unreleased, and clears the state. Returns false (state kept) when the closing was given
		/// up: PowerPoint stopped answering again.
		/// </summary>
		private static bool Recover(string reason)
		{
			bool closed;
			using (PowerPointCallLimit group = PowerPointCallLimit.Begin(LightStallMs, LightCapMs))
			{
				int count = PowerPoint.CloseLeftOpenPresentations();
				closed = group == null || !group.Abandoned;
				if (count > 0)
				{
					OnTrace("PowerPoint gate: " + count + " presentation(s) left open by given-up groups were closed");
				}
			}
			lastProbeMs = Clock.ElapsedMilliseconds;
			if (!closed)
			{
				return false;
			}
			lastAnswerMs = lastProbeMs;
			PowerPoint.ReleaseHeldProxies();
			SetState(false, reason);
			return true;
		}

		private static void SetState(bool notResponding, string reason)
		{
			if (NotResponding == notResponding)
			{
				return;
			}
			NotResponding = notResponding;
			try
			{
				StateChanged?.Invoke(notResponding, reason);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"PowerPointGate: state change handler failed: {ex.Message}");
			}
		}

		private static void OnTrace(string text)
		{
			try
			{
				Trace?.Invoke(text);
			}
			catch
			{
			}
		}
	}
}
