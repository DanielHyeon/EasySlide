using System;
using System.Threading.Tasks;

namespace Axicultus.Wpf.Captions;

/// <summary>What the audience screen shows, as the captions see it: the host maps its own live states to these.</summary>
public enum CaptionLiveState
{
    /// <summary>Nothing goes out (not live).</summary>
    Off,
    /// <summary>Live, on standby.</summary>
    Standby,
    /// <summary>Live, the content on the screen.</summary>
    Active,
    /// <summary>Live, the screen blacked out or cleared.</summary>
    Hidden,
}

/// <summary>The live output: its state and, when a PowerPoint deck is on it, the deck's file and slide (1-based; 0 without one).</summary>
public sealed record CaptionLiveSnapshot(CaptionLiveState State, string? PptPath, int PptSlideNumber)
{
    public static CaptionLiveSnapshot Off { get; } = new(CaptionLiveState.Off, null, 0);
}

/// <summary>The live output the caption session follows (<see cref="CaptionSession"/>).</summary>
public interface ICaptionLiveSource
{
    /// <summary>
    /// The output now. The same object until the output changes (the session drops a show reading taken over a change
    /// by comparing references).
    /// </summary>
    CaptionLiveSnapshot Current { get; }

    /// <summary>Raised on every change of the output (and on a refresh of the same output), with the output then.</summary>
    event EventHandler<CaptionLiveSnapshot>? Changed;
}

/// <summary>The native PowerPoint show on the output screen, when the host runs one.</summary>
public interface ICaptionSlideShow
{
    /// <summary>The slide the output show of this deck is on now, or null when not known (no such show).</summary>
    Task<int?> ReadSlideAsync(string path);
}
