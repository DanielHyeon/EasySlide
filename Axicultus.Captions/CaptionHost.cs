using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Axicultus.Wpf.Captions;

// Draft (a046 bundle A): what the caption tab takes from the app that hosts it, as a046 design 2.2 lists it. Nothing uses
// it yet: the caption tab (CaptionConsole) still reads the WPF app's MainViewModel. Bundle B moves the tab here, makes
// it use this, gives the WPF app its implementation and records the final form in design 5.

/// <summary>What an item on the output is to the captions.</summary>
public enum CaptionItemKind
{
    /// <summary>No item, or one with nothing to caption (a gap).</summary>
    None,
    /// <summary>A song, a passage, a text: its pages are text.</summary>
    Text,
    /// <summary>A PowerPoint deck: its slides are read from the file.</summary>
    PowerPoint,
    /// <summary>A video, a picture, a live camera.</summary>
    Media,
}

/// <summary>An item of the worship order or of the output column. PptPath and PptSlide (1-based) for a deck only.</summary>
public sealed record CaptionItem(string Id, string Title, CaptionItemKind Kind, string? PptPath = null, int PptSlide = 0);

/// <summary>A page of the output's text: its mark ("2/4"), its text, and whether it is the page shown.</summary>
public sealed record CaptionPage(string Label, string Text, bool IsCurrent);

/// <summary>
/// The output as the caption tab follows it, live or not: the live state and the deck on air (LivePptPath, LivePptSlide),
/// the output column's item, its text now and its pages, and whether the output column's slide picture is drawn.
/// </summary>
public sealed record CaptionOutput(
    CaptionLiveState State,
    CaptionItem? Item,
    string Text,
    IReadOnlyList<CaptionPage> Pages,
    string? LivePptPath = null,
    int LivePptSlide = 0,
    bool SlidePictureReady = false);

/// <summary>A song of the song database as the read lyrics are put right from it (<see cref="SongLyrics"/>).</summary>
public sealed record CaptionSong(string Title, string AlternateTitle, string Lyrics);

/// <summary>The song database. The caption tab reads it once per version and keeps the rules of when to read it again.</summary>
public interface ICaptionSongSource
{
    /// <summary>The version of the database now (its file and write times); empty when there is no database.</summary>
    string VersionKey { get; }

    Task<IReadOnlyList<CaptionSong>> ReadAsync(CancellationToken cancellation);

    /// <summary>A failure that may pass (the file busy or locked): that version is read again later.</summary>
    bool IsPassing(Exception error);
}

/// <summary>Large renders of a deck's slides (the score rows of a slide without a content picture).</summary>
public interface ICaptionSlideRenderer
{
    Task<BitmapSource?> RenderAsync(string path, int slide, CancellationToken cancellation);
}

/// <summary>The worship order kept with the caption's recovery file. The captions keep the items as they are given.</summary>
public interface ICaptionQueueRecovery
{
    /// <summary>The worship order now and the index of the picked item (-1: none).</summary>
    (JsonElement[] Items, int Selected) Capture();

    /// <summary>Puts a kept worship order back under its name.</summary>
    void Restore(IReadOnlyList<JsonElement> items, string plan);

    /// <summary>Raised before the worship order is replaced (the recovery is saved first).</summary>
    event EventHandler? Replacing;
}

/// <summary>What the caption tab takes from its host app.</summary>
public interface ICaptionHost
{
    // the plan

    /// <summary>The name of the worship order now.</summary>
    string PlanName { get; }

    event EventHandler? PlanChanged;

    // the output

    CaptionOutput Output { get; }

    /// <summary>Raised when anything in <see cref="Output"/> changed.</summary>
    event EventHandler? OutputChanged;

    /// <summary>Whether the output can be turned one step (1 next, -1 previous).</summary>
    bool CanStepOutput(int direction);

    void StepOutput(int direction);

    /// <summary>Shows the output's page (0-based, as in <see cref="CaptionOutput.Pages"/>).</summary>
    void GoToOutputPage(int index);

    // the picked item

    /// <summary>The item picked in the worship order, if any.</summary>
    CaptionItem? Picked { get; }

    event EventHandler? PickedChanged;

    bool CanSendPicked { get; }

    /// <summary>The picked item is on its way to the output column.</summary>
    bool IsSendingPicked { get; }

    /// <summary>Puts the picked item in the output column (not on air).</summary>
    void SendPicked();

    // slide pictures

    /// <summary>The small picture of the deck's slide the output or preview column holds, or null.</summary>
    ImageSource? SlidePicture(string path, int slide);

    event EventHandler? SlidePicturesChanged;

    /// <summary>Large slide renders; null when the host has none.</summary>
    ICaptionSlideRenderer? Renderer { get; }

    /// <summary>Reads an old-format deck (.ppt, .pps); null when the host cannot.</summary>
    Func<string, CancellationToken, Task<CaptionDeck>>? ReadLegacyDeck { get; }

    // songs, camera, recovery, errors

    /// <summary>The song database; null when the host has none.</summary>
    ICaptionSongSource? Songs { get; }

    /// <summary>Takes a live camera the host plays locally off it (OBS takes the camera).</summary>
    void ReleaseLocalCamera();

    /// <summary>Turns the output camera on or off; null when the host has none.</summary>
    Action? ToggleOutputCamera { get; }

    /// <summary>The worship order kept with the recovery; null when the host keeps its own.</summary>
    ICaptionQueueRecovery? Recovery { get; }

    /// <summary>Records a failure the operator is not shown at once (the host's error log).</summary>
    void WriteError(string action, Exception error);
}
