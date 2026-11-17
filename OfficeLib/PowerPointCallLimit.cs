using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace OfficeLib
{
	/// <summary>
	/// A time limit for a group of PowerPoint calls made on the UI thread at show end and app exit (decision T2 of
	/// fix-ppt-show-end-timeout). The limit follows progress: the group is abandoned when no step of it has completed
	/// for stallMs (the caller reports each completed step with Progress), or when the whole group has run for capMs.
	/// A healthy PowerPoint that is slow with many copies is therefore not cut off, and a hung one is given up about
	/// stallMs after its last answer. While the group runs, a COM message filter is registered on the thread; once the
	/// group is abandoned, the filter cancels the call that is waiting (it fails with RPC_E_CALL_CANCELED) and every
	/// later call of the group as soon as COM asks the filter about it. The caller then stops calling PowerPoint and
	/// does not release the proxies it holds (a release waits for PowerPoint without asking the filter, see
	/// PowerPoint.ReleaseProxy).
	///
	/// COM asks the filter (MessagePending) only when a message arrives while the call waits, so a timer posts
	/// WM_NULL to the thread every 100 ms while the group runs. Measured with a suspended PowerPoint (a scratch
	/// program, 2026-11-17): without these posts the filter was asked irregularly; with them, every call was
	/// cancelled within 0.1 s of the limit; the proxies answered again once PowerPoint resumed, and a cancelled
	/// Close was not carried out afterwards.
	///
	/// Only one group at a time, on an STA thread (the UI thread). The filter that was registered before is restored
	/// when the group ends, and a call PowerPoint rejects as busy is handled as before (RetryRejectedCall answers as
	/// that filter, or as COM does without a filter: cancel at once, RPC_E_CALL_REJECTED). Outside a group nothing
	/// changes.
	/// </summary>
	public sealed class PowerPointCallLimit : IDisposable
	{
		private const int PumpIntervalMs = 100;
		private const uint WM_NULL = 0;

		[ComImport, Guid("00000016-0000-0000-C000-000000000046"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
		internal interface IOleMessageFilter
		{
			[PreserveSig] int HandleInComingCall(int dwCallType, IntPtr hTaskCaller, int dwTickCount, IntPtr lpInterfaceInfo);
			[PreserveSig] int RetryRejectedCall(IntPtr hTaskCallee, int dwTickCount, int dwRejectType);
			[PreserveSig] int MessagePending(IntPtr hTaskCallee, int dwTickCount, int dwPendingType);
		}

		[DllImport("ole32.dll")]
		private static extern int CoRegisterMessageFilter(IOleMessageFilter lpMessageFilter, out IOleMessageFilter lplpMessageFilter);

		[DllImport("kernel32.dll")]
		private static extern uint GetCurrentThreadId();

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool PostThreadMessage(uint idThread, uint msg, IntPtr wParam, IntPtr lParam);

		[ComVisible(true)]
		internal sealed class Filter : IOleMessageFilter
		{
			private const int SERVERCALL_ISHANDLED = 0;
			private const int PENDINGMSG_CANCELCALL = 0;
			private const int PENDINGMSG_WAITDEFPROCESS = 2;

			private readonly PowerPointCallLimit owner;

			public Filter(PowerPointCallLimit owner)
			{
				this.owner = owner;
			}

			public int HandleInComingCall(int dwCallType, IntPtr hTaskCaller, int dwTickCount, IntPtr lpInterfaceInfo)
			{
				IOleMessageFilter previous = owner.previousFilter;
				return previous != null ? previous.HandleInComingCall(dwCallType, hTaskCaller, dwTickCount, lpInterfaceInfo) : SERVERCALL_ISHANDLED;
			}

			public int RetryRejectedCall(IntPtr hTaskCallee, int dwTickCount, int dwRejectType)
			{
				IOleMessageFilter previous = owner.previousFilter;
				return previous != null ? previous.RetryRejectedCall(hTaskCallee, dwTickCount, dwRejectType) : -1;
			}

			public int MessagePending(IntPtr hTaskCallee, int dwTickCount, int dwPendingType)
			{
				if (owner.Expired())
				{
					return PENDINGMSG_CANCELCALL;
				}
				IOleMessageFilter previous = owner.previousFilter;
				return previous != null ? previous.MessagePending(hTaskCallee, dwTickCount, dwPendingType) : PENDINGMSG_WAITDEFPROCESS;
			}
		}

		private readonly Stopwatch clock = Stopwatch.StartNew();
		private readonly int stallMs;
		private readonly int capMs;
		private readonly uint threadId;
		private long lastProgressMs;
		private Filter filter;
		private IOleMessageFilter previousFilter;
		private bool registered;
		private Timer pump;

		private static PowerPointCallLimit current;

		/// <summary>The group running on the calling thread (the UI thread), or null on any other thread.</summary>
		public static PowerPointCallLimit Current
		{
			get
			{
				PowerPointCallLimit limit = current;
				return (limit != null && limit.threadId == GetCurrentThreadId()) ? limit : null;
			}
		}

		/// <summary>
		/// True inside a group that was given up (a call was cancelled, or would be): the one signal the wrapper and
		/// its callers act on.
		/// </summary>
		public static bool CurrentAbandoned => Current?.Abandoned == true;

		/// <summary>The group is given up: its calls are cancelled. Set by the filter, or by Expired.</summary>
		public bool Abandoned { get; private set; }

		public long ElapsedMilliseconds => clock.ElapsedMilliseconds;

		/// <summary>Time since the last completed step (or since the group started).</summary>
		public long SinceProgressMilliseconds => clock.ElapsedMilliseconds - Interlocked.Read(ref lastProgressMs);

		private PowerPointCallLimit(int stallMs, int capMs)
		{
			this.stallMs = stallMs;
			this.capMs = capMs;
			threadId = GetCurrentThreadId();
		}

		/// <summary>
		/// Starts a group: given up after stallMs without a completed step, or after capMs in all. Returns null when
		/// a group is already running (the calls then belong to that group). Off an STA thread no filter can be
		/// registered: only the Expired checks between calls apply.
		/// </summary>
		public static PowerPointCallLimit Begin(int stallMs, int capMs)
		{
			if (current != null)
			{
				return null;
			}
			PowerPointCallLimit limit = new PowerPointCallLimit(stallMs, capMs);
			if (Thread.CurrentThread.GetApartmentState() == ApartmentState.STA)
			{
				limit.filter = new Filter(limit);
				try
				{
					int hr = CoRegisterMessageFilter(limit.filter, out IOleMessageFilter previous);
					if (hr == 0)
					{
						limit.previousFilter = previous;
						limit.registered = true;
					}
					else
					{
						Console.WriteLine($"PowerPointCallLimit: CoRegisterMessageFilter failed, HRESULT 0x{hr:X8}; the group runs without a message filter");
					}
				}
				catch (Exception ex)
				{
					Console.WriteLine($"PowerPointCallLimit: message filter not registered: {ex.Message}");
				}
			}
			if (limit.registered)
			{
				uint target = limit.threadId;
				limit.pump = new Timer(_ => PostThreadMessage(target, WM_NULL, IntPtr.Zero, IntPtr.Zero), null, PumpIntervalMs, PumpIntervalMs);
			}
			current = limit;
			return limit;
		}

		/// <summary>
		/// A step of the group completed (a presentation read, a Close, the quit check): the stall time starts again.
		/// Does nothing outside a group, on another thread, or once the group was given up.
		/// </summary>
		public static void Progress()
		{
			PowerPointCallLimit limit = Current;
			if (limit != null && !limit.Abandoned)
			{
				Interlocked.Exchange(ref limit.lastProgressMs, limit.clock.ElapsedMilliseconds);
			}
		}

		/// <summary>
		/// True (and the group given up) once no step has completed for stallMs, or the group has run for capMs.
		/// The filter asks it while a call waits; the caller asks it between calls.
		/// </summary>
		public bool Expired()
		{
			if (!Abandoned)
			{
				long now = clock.ElapsedMilliseconds;
				if (now - Interlocked.Read(ref lastProgressMs) >= stallMs || now >= capMs)
				{
					Abandoned = true;
				}
			}
			return Abandoned;
		}

		public void Dispose()
		{
			pump?.Dispose();
			pump = null;
			if (registered)
			{
				registered = false;
				try
				{
					int hr = CoRegisterMessageFilter(previousFilter, out _);
					if (hr != 0)
					{
						Console.WriteLine($"PowerPointCallLimit: restoring the previous message filter failed, HRESULT 0x{hr:X8}");
					}
				}
				catch (Exception ex)
				{
					Console.WriteLine($"PowerPointCallLimit: previous message filter not restored: {ex.Message}");
				}
			}
			// CoRegisterMessageFilter keeps its own reference to a restored filter; the one obtained at Begin is released.
			if (previousFilter != null && Marshal.IsComObject(previousFilter))
			{
				try
				{
					Marshal.ReleaseComObject(previousFilter);
				}
				catch (Exception ex)
				{
					Console.WriteLine($"PowerPointCallLimit: previous message filter reference not released: {ex.Message}");
				}
			}
			previousFilter = null;
			if (ReferenceEquals(current, this))
			{
				current = null;
			}
		}
	}
}
