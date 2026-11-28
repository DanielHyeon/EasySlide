using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using Axicultus.Wpf.Media;

namespace Axicultus.Wpf.Captions;

// What the caption tab (CaptionConsole) takes from the app that hosts it (a046 design 2.2, 4b; recorded in design 5).
// The host only reads its own state into these values and runs its own commands: every caption rule (what the tab
// follows, when a picked row is sent, which slide a deck is on, which songs are read) is the tab's.
// Threads: the tab reads every member and the host raises Changed and Replacing on the tab's UI thread; only
// ICaptionLiveSource.Changed may come from another thread. The host builds the CaptionSession on its UI thread, one that
// pumps messages: the session handles a change from another thread on the thread it was built on, so one built elsewhere
// never handles them.

/// <summary>What an item is to the captions.</summary>
public enum CaptionItemKind
{
    /// <summary>Nothing to caption (a gap).</summary>
    None,
    /// <summary>A song, a passage, a text: the output column shows its pages as text.</summary>
    Text,
    /// <summary>A PowerPoint deck (<see cref="CaptionItem.PptPath"/>): its slides are read from the file.</summary>
    PowerPoint,
    /// <summary>A video, a picture, a live camera: never sent to the output column by a pick.</summary>
    Media,
}

/// <summary>
/// An item of the worship order or of the output column. A deck (<see cref="CaptionItemKind.PowerPoint"/>) has its file
/// (PptPath) and the item's own slide (PptSlide, 1-based); other kinds have neither. For the output item of a host with an
/// <see cref="ICaptionHost.OutputPanel"/> PptSlide is the slide it was sent at (the panel tells the slide shown); for a
/// host without one it is the slide the output shows now, and the host raises <see cref="CaptionHostChange.Output"/> on
/// every turn. The picked row's PptSlide is not used (the preview column's slide is).
/// </summary>
public sealed record CaptionItem(string Id, string Title, CaptionItemKind Kind, string? PptPath = null, int PptSlide = 0);

/// <summary>
/// A page of the output column's text (song, passage, text): its index (0-based, as <see cref="ICaptionHost.GoToOutputPage"/>
/// takes it), its mark as the host shows it ("2/4"; empty: the tab shows "1/1"), its text, and whether it is the page shown.
/// </summary>
public sealed record CaptionPage(int Index, string Position, string Text, bool IsCurrent);

/// <summary>
/// A column that draws a deck (the output column, the preview column): the deck it holds now (null: none; it may still
/// hold the last deck while the next is drawn), the slide it shows (1-based, 0: none yet), and its small picture of that
/// slide (null: none yet).
/// </summary>
public sealed record CaptionSlidePanel(string? Path, int Slide, ImageSource? Picture);

/// <summary>What changed in the host (<see cref="ICaptionHost.Changed"/>).</summary>
public enum CaptionHostChange
{
    /// <summary><see cref="ICaptionHost.PlanName"/>: another worship order.</summary>
    Plan,
    /// <summary><see cref="ICaptionHost.OutputItem"/> or <see cref="ICaptionHost.OutputText"/>.</summary>
    Output,
    /// <summary><see cref="ICaptionHost.OutputPages"/> (raised for each page changed; the tab reads them once after).</summary>
    OutputPages,
    /// <summary>The output panel's slide or picture.</summary>
    OutputPanel,
    /// <summary>The output panel finished drawing a deck: its path, slide and picture are that deck's now.</summary>
    OutputPanelDrawn,
    /// <summary>The preview panel's slide or picture.</summary>
    PreviewPanel,
    /// <summary><see cref="ICaptionHost.Picked"/>.</summary>
    Picked,
}

/// <summary>A song of the song database (the read lyrics are put right from it, <see cref="SongLyrics"/>).</summary>
public sealed record CaptionSong(string Title, string AlternateTitle, string Lyrics, int FolderNo);

/// <summary>
/// The song database. The tab reads it once per version of its file (its write time, and its write-ahead log's), keeps
/// only the songs of a folder (FolderNo above 0), and reads a version that failed again only when the failure may pass.
/// </summary>
public interface ICaptionSongSource
{
    /// <summary>The database file (SQLite: its "-wal" file is looked at too); empty when there is none.</summary>
    string DatabasePath { get; }

    /// <summary>Every song of the database at path. Called off the UI thread.</summary>
    Task<IReadOnlyList<CaptionSong>> ReadAsync(string path);

    /// <summary>Whether a failure reading it came from the database being busy or locked (the tab tries again later).</summary>
    bool IsBusy(Exception error);
}

/// <summary>
/// A slide rendered for the captions: the encoded picture and its content type ("image/png", "image/jpeg"), or, when it
/// could not be made, Failure: why, in words the operator is shown (Bytes is then empty).
/// </summary>
public sealed record CaptionSlideRender(byte[] Bytes, string ContentType, string? Failure = null)
{
    public static CaptionSlideRender Failed(string why) => new([], "", why);
}

/// <summary>Large renders of a deck's slides (the score rows of a slide without a content picture, the lyrics read in it).</summary>
public interface ICaptionSlideRenderer
{
    /// <summary>The slide (1-based) at width × height pixels, within limit.</summary>
    Task<CaptionSlideRender> RenderAsync(string path, int slide, int width, int height, TimeSpan limit, CancellationToken cancellation);
}

/// <summary>A slide that could not be rendered or a deck that could not be read through the host's PowerPoint.</summary>
public sealed class CaptionRenderException(string message, Exception? innerException = null) : Exception(message, innerException);

/// <summary>
/// The worship order kept in the caption's recovery file. Each item is kept as the host writes it (the tab does not read
/// it); the host's item check (<see cref="CaptionRecoveryStore"/>'s isItem) tells a damaged one.
/// </summary>
public interface ICaptionQueueRecovery
{
    /// <summary>The worship order now and the index of the picked row (-1: none).</summary>
    (JsonElement[] Items, int Selected) Capture();

    /// <summary>Puts a kept worship order back under its name (plan) and picks its row selected (none when there is no such row).</summary>
    void Restore(IReadOnlyList<JsonElement> items, string plan, int selected);

    /// <summary>Raised before the host replaces the worship order (the tab saves its recovery first).</summary>
    event Action? Replacing;
}

/// <summary>
/// What the caption tab takes from its host app (a046 design 5). The tab reads Live, Recovery, Renderer, Camera and
/// whether ToggleOutputCamera is null once, when it is made; the other members each time it uses them.
/// </summary>
public interface ICaptionHost
{
    /// <summary>
    /// The audience screen (live or not, the deck on it). The same output the host's <see cref="CaptionSession"/> follows;
    /// the tab reads it for its own rules (a pick is sent only while not live, the deck on air comes first).
    /// </summary>
    ICaptionLiveSource Live { get; }

    /// <summary>Something below changed. Raised on the tab's UI thread.</summary>
    event EventHandler<CaptionHostChange>? Changed;

    // ---- the plan ----

    /// <summary>The name of the worship order now.</summary>
    string PlanName { get; }

    // ---- the output column (what the audience screen holds while not live) ----

    /// <summary>The output column's item, or null.</summary>
    CaptionItem? OutputItem { get; }

    /// <summary>The text the output column shows now ("" when none, as for a deck).</summary>
    string OutputText { get; }

    /// <summary>The output column's pages (none for a deck or without an item).</summary>
    IReadOnlyList<CaptionPage> OutputPages { get; }

    /// <summary>
    /// The output column's deck panel; null when the host has none of its own (then the output item's PptSlide is the
    /// slide the output shows now, and every turn is told as <see cref="CaptionHostChange.Output"/>).
    /// </summary>
    CaptionSlidePanel? OutputPanel { get; }

    /// <summary>The preview column's deck panel (its slide is the picked deck's slide); null when the host has none.</summary>
    CaptionSlidePanel? PreviewPanel { get; }

    /// <summary>Whether the output can be turned one step (1 next, -1 previous): a slide, a page.</summary>
    bool CanStepOutput(int direction);

    /// <summary>Turns the output one step, as the host's own next and previous do.</summary>
    Task StepOutputAsync(int direction);

    /// <summary>Shows the output column's page (0-based, <see cref="CaptionPage.Index"/>).</summary>
    void GoToOutputPage(int index);

    // ---- the picked row ----

    /// <summary>
    /// The row picked in the worship order, or null. The same object until the pick changes: the tab tells by reference
    /// whether the row is still the one it waited for (a host that makes the value anew returns the last one while the
    /// pick is the same row).
    /// </summary>
    CaptionItem? Picked { get; }

    /// <summary>Whether the picked row can be sent to the output column now.</summary>
    bool CanSendPicked { get; }

    /// <summary>
    /// The send of a row to the output column under way (the host's own, or one the tab asked for), or a completed task.
    /// </summary>
    Task PickedSending { get; }

    /// <summary>
    /// Puts the picked row in the output column (not on air), as the host's own "send to output" does; the task ends
    /// when it is there and fails with what went wrong (the tab tells and logs it).
    /// </summary>
    Task SendPickedAsync();

    // ---- decks ----

    /// <summary>Large slide renders; null when the host has none.</summary>
    ICaptionSlideRenderer? Renderer { get; }

    /// <summary>Reads an old-format deck (.ppt, .pps); null when the host cannot (such a deck is then not read).</summary>
    Func<string, CancellationToken, Task<CaptionDeck>>? ReadLegacyDeck { get; }

    // ---- songs, camera, recovery, errors ----

    /// <summary>The song database; null when the host has none.</summary>
    ICaptionSongSource? SongDatabase { get; }

    /// <summary>
    /// Who has the camera (the camera preview, its status, its flip, whether OBS has it), shared with the host's own
    /// camera use; null when the host has no camera of its own (the tab keeps one for the OBS camera preview).
    /// </summary>
    CameraOwnership? Camera { get; }

    /// <summary>OBS takes the camera: the host stops playing a live camera of its own, if it plays one.</summary>
    void StopLocalCamera();

    /// <summary>Turns the host's output camera on or off; null when it has none.</summary>
    Action? ToggleOutputCamera { get; }

    /// <summary>The worship order kept with the recovery; null when the host keeps its own.</summary>
    ICaptionQueueRecovery? Recovery { get; }

    /// <summary>Records a failure the operator is not shown at once (the host's error log).</summary>
    void WriteError(string action, Exception error);
}
