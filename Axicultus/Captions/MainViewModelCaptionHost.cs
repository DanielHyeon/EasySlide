using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Axicultus.Wpf.Media;
using Axicultus.Wpf.Rendering;
using Axicultus.Wpf.Shell;
using Axicultus.Wpf.Support;

namespace Axicultus.Wpf.Captions;

/// <summary>
/// The WPF app's main view model as the caption tab's host (a046 design 5): its values read as the caption library's,
/// its commands run as they are. The caption rules are the tab's; nothing here decides what the captions do.
/// </summary>
internal sealed class MainViewModelCaptionHost : ICaptionHost, ICaptionQueueRecovery, ICaptionSongSource, ICaptionSlideRenderer
{
    private readonly IPowerPointRenderService? _render;
    private readonly OperationalErrorLog _errors;
    private EventHandler<CaptionHostChange>? _changed;
    // The caption view of the row last read as Picked: one object per picked row, as Picked promises.
    private (LiveQueueItem Source, CaptionItem View)? _picked;

    public MainViewModelCaptionHost(MainViewModel main, CameraOwnership? camera, IPowerPointRenderService? render, OperationalErrorLog? errors = null)
    {
        Main = main;
        Camera = camera;
        _render = render;
        _errors = errors ?? OperationalErrorLog.Default;
        Live = main.Session.ForCaptions();
    }

    /// <summary>The view model it reads (the tests drive it).</summary>
    internal MainViewModel Main { get; }

    public ICaptionLiveSource Live { get; }

    // Main is listened to only while someone listens here (a tab ended lets go of it).
    public event EventHandler<CaptionHostChange>? Changed
    {
        add
        {
            if (value == null) return;
            if (_changed == null) Listen(true);
            _changed += value;
        }
        remove
        {
            if (value == null || _changed == null) return;
            _changed -= value;
            if (_changed == null) Listen(false);
        }
    }

    private void Listen(bool on)
    {
        if (on)
        {
            Main.PropertyChanged += MainChanged;
            Main.OutputLyricsPages.CollectionChanged += PagesChanged;
            Main.OutputPowerPoint.PropertyChanged += OutputPanelChanged;
            Main.PowerPoint.PropertyChanged += PreviewPanelChanged;
        }
        else
        {
            Main.PropertyChanged -= MainChanged;
            Main.OutputLyricsPages.CollectionChanged -= PagesChanged;
            Main.OutputPowerPoint.PropertyChanged -= OutputPanelChanged;
            Main.PowerPoint.PropertyChanged -= PreviewPanelChanged;
        }
    }

    private void Raise(CaptionHostChange change) => _changed?.Invoke(this, change);

    private void MainChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainViewModel.CurrentWorshipListName)) Raise(CaptionHostChange.Plan);
        else if (e.PropertyName is nameof(MainViewModel.OutputLyricsText) or nameof(MainViewModel.OutputItem)) Raise(CaptionHostChange.Output);
        else if (e.PropertyName == nameof(MainViewModel.SelectedItem)) Raise(CaptionHostChange.Picked);
    }

    private void PagesChanged(object? sender, NotifyCollectionChangedEventArgs e) => Raise(CaptionHostChange.OutputPages);

    private void OutputPanelChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(PowerPointPreviewViewModel.State))
        {
            if (Main.OutputPowerPoint.State == PowerPointPreviewState.Ready) Raise(CaptionHostChange.OutputPanelDrawn);
        }
        else if (IsSlideOrPicture(e)) Raise(CaptionHostChange.OutputPanel);
    }

    private void PreviewPanelChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (IsSlideOrPicture(e)) Raise(CaptionHostChange.PreviewPanel);
    }

    private static bool IsSlideOrPicture(PropertyChangedEventArgs e) =>
        e.PropertyName is nameof(PowerPointPreviewViewModel.PreviewImage) or nameof(PowerPointPreviewViewModel.SlideNumber);

    // ---- values ----

    public string PlanName => Main.CurrentWorshipListName;

    public CaptionItem? OutputItem => Main.OutputItem is { } item ? View(item) : null;

    public string OutputText => Main.OutputLyricsText ?? "";

    public IReadOnlyList<CaptionPage> OutputPages =>
        [.. Main.OutputLyricsPages.Select(page => new CaptionPage(page.PageIndex, page.PositionText, page.BodyText, page.IsCurrent))];

    public CaptionSlidePanel? OutputPanel => Panel(Main.OutputPowerPoint);

    public CaptionSlidePanel? PreviewPanel => Panel(Main.PowerPoint);

    private static CaptionSlidePanel Panel(PowerPointPreviewViewModel panel) => new(panel.LoadedContentPath, panel.SlideNumber, panel.PreviewImage);

    public CaptionItem? Picked
    {
        get
        {
            if (Main.SelectedItem is not { } item) return null;
            if (_picked is { } kept && ReferenceEquals(kept.Source, item)) return kept.View;
            var view = View(item);
            _picked = (item, view);
            return view;
        }
    }

    // A deck as the output column draws it (its kind inferred from its file when the row does not say), a media row by
    // its own kind (as the picked row's send has always looked at it).
    internal static CaptionItem View(LiveQueueItem item) =>
        item is { ContentPath.Length: > 0 } && LiveItemKindMatcher.IsPowerPoint(LiveItemKindMatcher.InferLegacyKind(item))
            ? new(item.Id, item.Title, CaptionItemKind.PowerPoint, item.ContentPath, item.SlideNumber)
            : new(item.Id, item.Title, LiveItemKindMatcher.IsMedia(item.Kind) ? CaptionItemKind.Media : CaptionItemKind.Text);

    // ---- commands ----

    public bool CanStepOutput(int direction) => Step(direction).CanExecute(null);

    public Task StepOutputAsync(int direction) => Step(direction).ExecuteAsync(null);

    private CommunityToolkit.Mvvm.Input.IAsyncRelayCommand Step(int direction) =>
        direction > 0 ? Main.NextOutputSlideCommand : Main.PreviousOutputSlideCommand;

    public void GoToOutputPage(int index) => Main.GoToOutputLyricsPageCommand.Execute(index);

    public bool CanSendPicked => Main.SendWorshipSelectionToOutputCommand.CanExecute(null);

    public Task PickedSending => Main.SendWorshipSelectionToOutputCommand.ExecutionTask ?? Task.CompletedTask;

    public void SendPicked() => Main.SendWorshipSelectionToOutputCommand.Execute(null);

    // ---- decks ----

    public ICaptionSlideRenderer? Renderer => _render == null ? null : this;

    async Task<CaptionSlideRender> ICaptionSlideRenderer.RenderAsync(string path, int slide, int width, int height, TimeSpan limit, CancellationToken cancellation)
    {
        var result = await _render!.RenderSlideAsync(new(path, slide, width, height, limit), cancellation);
        return result.Succeeded ? new(result.Slide!.ImageBytes, result.Slide.ContentType)
            : CaptionSlideRender.Failed(result.ErrorMessage ?? result.ErrorKind.ToString());
    }

    public Func<string, CancellationToken, Task<CaptionDeck>>? ReadLegacyDeck => ReadLegacyDeckAsync;

    // One worker for this one conversion (its end ends the child process).
    private static async Task<CaptionDeck> ReadLegacyDeckAsync(string path, CancellationToken cancellation)
    {
        using var worker = new PowerPointWorker();
        try { return await worker.ReadLegacyDeckAsync(path, cancellation).ConfigureAwait(false); }
        catch (PowerPointRenderException error) { throw new CaptionRenderException(error.Message, error); }
    }

    // ---- songs ----

    public ICaptionSongSource? SongDatabase => this;

    string ICaptionSongSource.DatabasePath => Main.SongDatabasePath;

    async Task<IReadOnlyList<CaptionSong>> ICaptionSongSource.ReadAsync(string path) =>
        [.. (await Main.Library.ReadSongsAsync(path).ConfigureAwait(false)).Select(song => new CaptionSong(song.Title, song.AlternateTitle, song.Lyrics, song.FolderNo))];

    bool ICaptionSongSource.IsBusy(Exception error) => error is System.Data.SQLite.SQLiteException
        { ResultCode: System.Data.SQLite.SQLiteErrorCode.Busy or System.Data.SQLite.SQLiteErrorCode.Locked };

    // ---- camera ----

    public CameraOwnership? Camera { get; }

    public void StopLocalCamera()
    {
        if (Main.Media.MediaType == "Live Camera") Main.Media.Unload();
    }

    public Action? ToggleOutputCamera => () => Main.ToggleOutputCameraCommand.Execute(null);

    // ---- recovery ----

    public ICaptionQueueRecovery? Recovery => this;

    (JsonElement[] Items, int Selected) ICaptionQueueRecovery.Capture() =>
        ([.. Main.Queue.Select(ToRecovery)], Main.SelectedItem == null ? -1 : Main.Queue.IndexOf(Main.SelectedItem));

    void ICaptionQueueRecovery.Restore(IReadOnlyList<JsonElement> items, string plan, int selected)
    {
        Main.LoadRecoveredQueue(items.Select(FromRecovery), plan);
        if (selected >= 0 && selected < Main.Queue.Count) Main.SelectedItem = Main.Queue[selected];
    }

    event Action? ICaptionQueueRecovery.Replacing
    {
        add => Main.QueueReplacing += value;
        remove => Main.QueueReplacing -= value;
    }

    // The worship order's rows in the recovery file: as the worship list keeps them (WorshipListItemDto, the shape the
    // file has always had). A row never changes (a changed row is a new object), so each is serialized once, on the UI
    // thread, however often the recovery is saved.
    internal static JsonElement ToRecovery(LiveQueueItem item) => RecoveryRow(item).Value;

    /// <summary>The kept serialized form of the row (a test sees that it is made once).</summary>
    internal static StrongBox<JsonElement> RecoveryRow(LiveQueueItem item) => RecoveryRows.GetValue(item, row => new(JsonSerializer.SerializeToElement(
        new WorshipListItemDto(row.Id, row.Title, row.Kind, row.SlideNumber, row.Lyrics, row.ContentPath, row.FormatData, row.UseIndividualFormatting,
            row.Sequence, row.SongLayout, row.SourceFolderNo, row.Capo, row.Notations))));

    private static readonly ConditionalWeakTable<LiveQueueItem, StrongBox<JsonElement>> RecoveryRows = new();

    internal static LiveQueueItem FromRecovery(JsonElement row)
    {
        var dto = row.Deserialize<WorshipListItemDto>()!;
        return new(dto.Id, dto.Title, dto.Kind)
        {
            SlideNumber = dto.SlideNumber, Lyrics = dto.Lyrics, ContentPath = dto.ContentPath, FormatData = dto.FormatData,
            UseIndividualFormatting = dto.UseIndividualFormatting, Sequence = dto.Sequence, SongLayout = dto.SongLayout, SourceFolderNo = dto.SourceFolderNo,
            Capo = dto.Capo, Notations = dto.Notations, // (a row written before a048 has neither: capo 0, no chords)
        };
    }

    /// <summary>The recovery store's check of a kept row (a damaged one sends the store to the file's backup).</summary>
    internal static bool IsRecoverableItem(JsonElement row)
    {
        try { return row.Deserialize<WorshipListItemDto>() is { Id: not null, Title: not null, Kind: not null }; }
        catch (JsonException) { return false; }
    }

    // ---- errors ----

    public void WriteError(string action, Exception error) => _errors.Write(action, error);
}
