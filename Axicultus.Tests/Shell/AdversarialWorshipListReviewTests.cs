using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Axicultus.Wpf.Controls;
using Axicultus.Wpf.Media;
using Axicultus.Wpf.Settings;
using Axicultus.Wpf.Shell;
using FluentAssertions;
using Xunit;

namespace Axicultus.Wpf.Tests.Shell;

// Worship-list review: lost work (clear, delete, restore, rename, damaged files, saving after an edit) and
// the row that is live when a song is in the list twice.
public partial class MainViewModelTests
{
    private static LiveQueueItem ReviewSong(string id, string title, string lyrics, bool fromLibrary)
        => new(id, title, LiveItemKinds.Song) { Lyrics = lyrics, SourceFolderNo = fromLibrary ? 1 : null };

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task GapAfterTheSecondRowOfTheSameSong_NextOpensTheRowAfterIt(bool fromLibrary)
    {
        // AddSong gives both rows the id "song:1"; a library song also carries its folder number.
        var settings = TempSettingsFolder.CreateDetachedSettings();
        settings.Set(AxicultusSettingKeys.GapItemOption, GapItemMode.Default).Succeeded.Should().BeTrue();
        var sut = CreateSut(settings: settings, seedSampleQueue: false);
        sut.LoadQueue([
            ReviewSong("song:1", "A", "a1", fromLibrary),
            ReviewSong("song:2", "B", "b1", fromLibrary),
            ReviewSong("song:1", "A", "a1", fromLibrary),
            ReviewSong("song:3", "C", "c1", fromLibrary),
        ]);
        sut.OpenOutputCommand.Execute(null);
        sut.SelectedItem = sut.Queue[2];
        await sut.GoLiveCommand.ExecuteAsync(null);

        await sut.NextOutputItemCommand.ExecuteAsync(null);
        sut.IsGapItemOnDisplay.Should().BeTrue();
        await sut.NextOutputItemCommand.ExecuteAsync(null);

        sut.Session.Current.CurrentItemTitle.Should().Be("C", "the gap follows the third row, so Next opens the fourth");
    }

    [Fact]
    public async Task RemovingTheOtherRowOfTheSameSong_LeavesTheLiveRowLive()
    {
        var settings = TempSettingsFolder.CreateDetachedSettings();
        settings.Set(AxicultusSettingKeys.AdvanceNextItem, true).Succeeded.Should().BeTrue();
        var sut = CreateSut(settings: settings, seedSampleQueue: false);
        var live = ReviewSong("song:1", "A", "a1\n\na2", fromLibrary: false);
        var other = ReviewSong("song:1", "A", "a1\n\na2", fromLibrary: false);
        sut.LoadQueue([live, ReviewSong("song:2", "B", "b1", fromLibrary: false), other]);
        sut.OpenOutputCommand.Execute(null);
        sut.SelectedItem = live;
        await sut.GoLiveCommand.ExecuteAsync(null);

        sut.SelectedItem = other;
        sut.RemoveSelectedItemCommand.Execute(null);

        sut.Session.Current.CurrentItemTitle.Should().Be("A", "the row on output was not removed");
        sut.LiveItemId.Should().Be("song:1", "the live row is still in the list");
        await sut.LiveNextShortcutCommand.ExecuteAsync(null);
        sut.Session.Current.CurrentItemTitle.Should().Be("A");
        sut.Session.Current.CurrentLyricsPageIndex.Should().Be(1, "Next shows the second verse of the live song");
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task RemovingTheOtherRowOfTheSameSong_NextStillShowsTheSecondVerse(bool advanceNextItem)
    {
        var settings = TempSettingsFolder.CreateDetachedSettings();
        settings.Set(AxicultusSettingKeys.AdvanceNextItem, advanceNextItem).Succeeded.Should().BeTrue();
        var sut = CreateSut(settings: settings, seedSampleQueue: false);
        var live = ReviewSong("song:1", "A", "a1\n\na2", fromLibrary: false);
        var other = ReviewSong("song:1", "A", "a1\n\na2", fromLibrary: false);
        sut.LoadQueue([live, ReviewSong("song:2", "B", "b1", fromLibrary: false), other]);
        sut.OpenOutputCommand.Execute(null);
        sut.SelectedItem = live;
        await sut.GoLiveCommand.ExecuteAsync(null);
        sut.SelectedItem = other;
        sut.RemoveSelectedItemCommand.Execute(null);

        await sut.LiveNextShortcutCommand.ExecuteAsync(null);

        (sut.Session.Current.CurrentItemTitle, sut.Session.Current.CurrentLyricsPageIndex).Should().Be(("A", 1));
    }

    [Theory]
    [InlineData("format")]
    [InlineData("sequence")]
    [InlineData("layout")]
    [InlineData("bible")]
    [InlineData("powerpoint")]
    public async Task LoadingAListWithADamagedFirstRow_DoesNotLeaveTheOldListPending(string damage)
    {
        using var folder = TempSettingsFolder.Create();
        var path = Path.Combine(folder.Root, "Lists");
        var store = new WorshipListStore(path);
        await store.SaveAsync("주일", [new("a", "A"), new("b", "B")]);
        var first = damage switch
        {
            "format" => new LiveQueueItem("song:1", "곡", LiveItemKinds.Song) { Lyrics = "[1]\n가사", FormatData = "29=x>47=>>=\0>1=99999999999999999999>" },
            "sequence" => new LiveQueueItem("song:1", "곡", LiveItemKinds.Song) { Lyrics = "[1]\n가사", Sequence = "((,,\0 999999999999999999999" },
            "layout" => new LiveQueueItem("song:1", "곡", LiveItemKinds.Song) { Lyrics = "[1]\n가사", SourceFolderNo = int.MaxValue,
                SongLayout = new Axicultus.Wpf.Data.SongPageLayout(0, "", -1, -1, -1, -1, 0, 0, -1) },
            "bible" => new LiveQueueItem("bible:;;;;;;;;;;;;", "성경", LiveItemKinds.Bible),
            _ => new LiveQueueItem("powerpoint:<>|?*", "덱", LiveItemKinds.PowerPoint) { ContentPath = "<>|?*:\0.pptx", SlideNumber = int.MinValue },
        };
        await store.SaveAsync("수요", [first, new("c", "C")]);
        using var vm = CreateSut(seedSampleQueue: false, settings: folder.CreateSettings(), worshipLists: store);
        await vm.LoadWorshipListAsync("주일");

        await vm.Invoking(shell => shell.LoadWorshipListAsync("수요")).Should().NotThrowAsync();
        vm.CurrentWorshipListName.Should().Be("수요");
        vm.HasPendingWorshipListSave.Should().BeFalse();
    }

    [Fact]
    public async Task ClearingAListWithUnsavedEdits_PutsTheEditedListInTrash()
    {
        // FrmMain saves after every change, so the list it moves to Trash is the list on screen.
        using var folder = TempSettingsFolder.Create();
        var store = new WorshipListStore(Path.Combine(folder.Root, "Lists"));
        await store.SaveAsync("주일", [new("a", "A")]);
        using var vm = CreateSut(seedSampleQueue: false, settings: folder.CreateSettings(), worshipLists: store);
        await vm.LoadWorshipListAsync("주일");
        vm.AddTextItem("새 광고").Should().NotBeNull();
        vm.HasPendingWorshipListSave.Should().BeTrue();

        vm.ArchiveAndClearCurrentWorshipList();
        (await vm.SavePendingWorshipListChangesAsync()).Should().BeTrue("the app closes");

        var restored = store.RestoreFromTrash("주일");
        (await store.LoadAsync(restored)).Should().HaveCount(2, "the item added before the clear belongs to the list");
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ADroppedListFile_GivesTheSameSongRow_WhetherOrNotItsFolderIsShown(bool folderShownInLibraryTab)
    {
        using var folder = TempSettingsFolder.Create();
        var eswPath = Path.Combine(folder.Root, "drop.esw");
        await File.WriteAllTextAsync(eswPath,
            "<Axicultus><ListItem><Item><ItemID>D77</ItemID><Title1>Dropped song</Title1><Folder /></Item></ListItem></Axicultus>");
        var repo = new StubSongDetailRepository(SampleSongDetail(77, "Dropped song", "[1]\n1절\n[chorus]\n후렴", sequence: "1 C 1"));
        var sut = CreateSut(settings: SettingsWithSongDatabase(folder), songDetail: repo, seedSampleQueue: false);
        if (folderShownInLibraryTab)
        {
            sut.Library.Songs.Add(new Axicultus.Wpf.Data.SongSummary(77, "Dropped song", "", 1, 77, "", "", "[1]\n1절\n[chorus]\n후렴") { Sequence = "1 C 1" });
        }

        sut.AddExternalFilesRelativeTo([eswPath], targetItem: null).Should().Be(1);

        sut.Queue[0].Lyrics.Should().Be("[1]\n1절\n[chorus]\n후렴");
        sut.Queue[0].Sequence.Should().Be("1 C 1", "the song's verse order decides what the audience sees");
        sut.Queue[0].SourceFolderNo.Should().Be(1);
    }

    [Fact]
    public async Task OpeningADamagedListFileDuringAShow_KeepsTheQueueAndTheShow()
    {
        // The store already keeps the queue for a list it cannot read (IOException); a cut-off file is the same case.
        using var folder = TempSettingsFolder.Create();
        var path = Path.Combine(folder.Root, "Lists");
        var store = new WorshipListStore(path);
        await store.SaveAsync("주일", [new LiveQueueItem("song:1", "A", LiveItemKinds.Song) { Lyrics = "a1" }]);
        await File.WriteAllTextAsync(Path.Combine(path, "수요.json"), "[ { \"Id\": \"song:2\", \"Title\": \"B\", \"Ki");
        using var vm = CreateSut(seedSampleQueue: false, settings: folder.CreateSettings(), worshipLists: store);
        await vm.LoadWorshipListAsync("주일");
        vm.OpenOutputCommand.Execute(null);
        await vm.GoLiveCommand.ExecuteAsync(null);
        vm.Session.Current.State.Should().Be(LiveState.Active);

        await vm.LoadWorshipListAsync("수요");

        vm.Queue.Should().ContainSingle("a list that cannot be read must not replace the list on screen");
        vm.Session.Current.State.Should().Be(LiveState.Active, "the show must not end because a file is damaged");
    }

    private static async Task<string[]> ReviewSavedListsAsync(WorshipListStore store)
    {
        var lists = new System.Collections.Generic.List<string>();
        foreach (var name in store.ListNames())
        {
            lists.Add(string.Join(",", (await store.LoadAsync(name)).Select(item => item.Id)));
        }

        return [.. lists];
    }

    [Fact]
    public async Task RestoringAClearedListFromTrash_SurvivesTheNextLoadOrClose()
    {
        // Clear moves the file to Trash but the queue keeps the list's name; a restore sees no name clash.
        using var folder = TempSettingsFolder.Create();
        var store = new WorshipListStore(Path.Combine(folder.Root, "Lists"));
        await store.SaveAsync("주일", [new("a", "A"), new("b", "B")]);
        using var vm = CreateSut(seedSampleQueue: false, settings: folder.CreateSettings(), worshipLists: store);
        await vm.LoadWorshipListAsync("주일");
        vm.ArchiveAndClearCurrentWorshipList();
        vm.AddTextItem("새 광고").Should().NotBeNull();

        vm.RestoreWorshipLists(["주일"]).Should().Be(1);
        await vm.LoadWorshipListAsync("주일");

        (await ReviewSavedListsAsync(store)).Should().Contain("a,b", "the restored list must not be overwritten by the pending save");
    }

    [Fact]
    public async Task RenamingAListToTheNameOfAnUnsavedCurrentList_DoesNotLoseIt()
    {
        using var folder = TempSettingsFolder.Create();
        var store = new WorshipListStore(Path.Combine(folder.Root, "Lists"));
        await store.SaveAsync("수요", [new("c", "C"), new("d", "D")]);
        using var vm = CreateSut(seedSampleQueue: false, settings: folder.CreateSettings(), worshipLists: store);
        const string xml = "<Axicultus><ListItem><Item><ItemID>T1</ItemID><Title1>광고</Title1><Folder /></Item></ListItem></Axicultus>";
        await vm.OpenLegacySessionAsync("가져온 순서", xml, importCopy: true);

        vm.RenameWorshipList("수요", "가져온 순서");
        (await vm.SavePendingWorshipListChangesAsync()).Should().BeTrue("the app closes");

        (await ReviewSavedListsAsync(store)).Should().Contain("c,d");
    }

    [Fact]
    public async Task DeletingTheCurrentListWithUnsavedEdits_PutsTheEditedListInTrash()
    {
        using var folder = TempSettingsFolder.Create();
        var store = new WorshipListStore(Path.Combine(folder.Root, "Lists"));
        await store.SaveAsync("주일", [new("a", "A")]);
        using var vm = CreateSut(seedSampleQueue: false, settings: folder.CreateSettings(), worshipLists: store);
        await vm.LoadWorshipListAsync("주일");
        vm.AddTextItem("새 광고").Should().NotBeNull();

        vm.DeleteWorshipList("주일");

        var restored = store.RestoreFromTrash("주일");
        (await store.LoadAsync(restored)).Should().HaveCount(2);
    }

    [Fact]
    public async Task ADuplicatedSongRow_IsRefreshedFromTheDatabaseOnLoad()
    {
        using var folder = TempSettingsFolder.Create();
        var store = new InMemoryWorshipListStore();
        var repo = new StubSongDetailRepository(SampleSongDetail(42, "곡", "[1]\n고친 가사"));
        var sut = CreateSut(settings: SettingsWithSongDatabase(folder), worshipLists: store, songDetail: repo, seedSampleQueue: false);
        sut.LoadQueue([new LiveQueueItem("song:42", "곡", LiveItemKinds.Song) { Lyrics = "[1]\n틀린 가사" }]);
        sut.SelectedItem = sut.Queue[0];
        sut.DuplicateSelectedItem().Should().NotBeNull();
        await sut.SaveWorshipListAsync("주일");

        await sut.LoadWorshipListAsync("주일");

        sut.Queue.Select(item => item.Lyrics).Should().Equal("[1]\n고친 가사", "[1]\n고친 가사");
    }

    [Fact]
    public async Task GoLiveWithAnotherClip_WhileAClipPlays_ShowsTheNewClip()
    {
        // FrmLaunchShow PlayLiveMedia: the media player follows the live item.
        using var folder = TempSettingsFolder.Create();
        var first = Path.Combine(folder.Root, "welcome.avi");
        var second = Path.Combine(folder.Root, "offering.avi");
        File.WriteAllText(first, "media placeholder");
        File.WriteAllText(second, "media placeholder");
        var clip = new LiveQueueItem("media:welcome", "Welcome", LiveItemKinds.Media) { ContentPath = first };
        var next = new LiveQueueItem("media:offering", "Offering", LiveItemKinds.Media) { ContentPath = second };
        using var sut = CreateSut(seedSampleQueue: false);
        sut.LoadQueue([clip, next]);
        sut.OpenOutputCommand.Execute(null);
        sut.Media.PlayPauseCommand.Execute(null);
        sut.Media.State.Should().Be(MediaPlaybackState.Playing);

        sut.SelectedItem = next;
        await sut.GoLiveCommand.ExecuteAsync(null);

        sut.Session.Current.CurrentItemTitle.Should().Be("Offering");
        sut.Media.Source.Should().Be(second, "the live item is the second clip");
    }

    [Fact]
    public void StopAndPlay_AfterSelectingAnotherClip_PlaysTheSelectedClip()
    {
        using var folder = TempSettingsFolder.Create();
        var first = Path.Combine(folder.Root, "welcome.avi");
        var second = Path.Combine(folder.Root, "offering.avi");
        File.WriteAllText(first, "media placeholder");
        File.WriteAllText(second, "media placeholder");
        var clip = new LiveQueueItem("media:welcome", "Welcome", LiveItemKinds.Media) { ContentPath = first };
        var next = new LiveQueueItem("media:offering", "Offering", LiveItemKinds.Media) { ContentPath = second };
        using var sut = CreateSut(seedSampleQueue: false);
        sut.LoadQueue([clip, next]);
        sut.Media.PlayPauseCommand.Execute(null);
        sut.Media.State.Should().Be(MediaPlaybackState.Playing);

        sut.SelectedItem = next;
        sut.Media.StopCommand.Execute(null);
        sut.Media.PlayPauseCommand.Execute(null);

        sut.Media.Source.Should().Be(second, "the selected row is the second clip");
    }

    [Fact]
    public async Task AFailedReplacingLoad_DoesNotLeaveTheOldListPendingWithTheNewRows()
    {
        // Mechanism probe: anything thrown between Queue.Clear and the end of LoadQueue.
        using var folder = TempSettingsFolder.Create();
        var path = Path.Combine(folder.Root, "Lists");
        var store = new WorshipListStore(path);
        await store.SaveAsync("주일", [new("a", "A"), new("b", "B")]);
        using var vm = CreateSut(seedSampleQueue: false, settings: folder.CreateSettings(), worshipLists: store);
        await vm.LoadWorshipListAsync("주일");

        static System.Collections.Generic.IEnumerable<LiveQueueItem> Broken()
        {
            yield return new LiveQueueItem("x", "X");
            throw new IOException("the second row cannot be built");
        }

        vm.Invoking(shell => shell.LoadQueue(Broken())).Should().Throw<IOException>();
        (await vm.SavePendingWorshipListChangesAsync()).Should().BeTrue("the app closes");

        (await new WorshipListStore(path).LoadAsync("주일")).Select(item => item.Id).Should().Equal("a", "b");
    }

    private sealed class GatedLoadWorshipListStore(IWorshipListStore inner) : IWorshipListStore
    {
        public TaskCompletionSource? Gate { get; set; }

        public Task SaveAsync(string name, System.Collections.Generic.IReadOnlyList<LiveQueueItem> items, System.Threading.CancellationToken cancellationToken = default)
            => inner.SaveAsync(name, items, cancellationToken);

        public async Task<System.Collections.Generic.IReadOnlyList<LiveQueueItem>> LoadAsync(string name, System.Threading.CancellationToken cancellationToken = default)
        {
            if (Gate is { } gate)
            {
                Gate = null;
                await gate.Task;
            }

            return await inner.LoadAsync(name, cancellationToken);
        }

        public System.Collections.Generic.IReadOnlyList<string> ListNames() => inner.ListNames();

        public void Delete(string name) => inner.Delete(name);

        public void MoveToTrash(string name) => inner.MoveToTrash(name);

        public void Rename(string oldName, string newName) => inner.Rename(oldName, newName);
    }

    [Fact]
    public async Task AnEditIsSavedOnceTheUiIsIdle_AndTheEditsOfOneActionAreOneSave()
    {
        // FrmMain saves after every change. The window hands the view model a scheduler (dispatcher, idle).
        using var folder = TempSettingsFolder.Create();
        var path = Path.Combine(folder.Root, "Lists");
        var store = new WorshipListStore(path);
        await store.SaveAsync("주일", [new("a", "A")]);
        using var vm = CreateSut(seedSampleQueue: false, settings: folder.CreateSettings(), worshipLists: store);
        var scheduled = new System.Collections.Generic.List<System.Action>();
        vm.WorshipListSaveScheduler = scheduled.Add;
        await vm.LoadWorshipListAsync("주일");
        scheduled.Should().BeEmpty("loading a list is not an edit");

        vm.AddTextItem("광고 1").Should().NotBeNull();
        vm.AddTextItem("광고 2").Should().NotBeNull();
        scheduled.Should().ContainSingle("a drag of many rows must not write the file once per row");
        var status = vm.StatusText;
        scheduled[0]();
        await WaitUntilAsync(() => !vm.HasPendingWorshipListSave, "the scheduled save writes the list");

        (await new WorshipListStore(path).LoadAsync("주일")).Should().HaveCount(3, "no close or list switch was needed");
        vm.StatusText.Should().Be(status, "the save does not replace the status line of the edit");
        vm.AddTextItem("광고 3").Should().NotBeNull();
        scheduled.Should().HaveCount(2, "the next edit schedules the next save");
    }

    [Fact]
    public async Task AnEditMadeWhileAnotherListIsBeingRead_IsSavedToTheListItWasMadeIn()
    {
        var inner = new InMemoryWorshipListStore();
        await inner.SaveAsync("주일", [new("a", "A")]);
        await inner.SaveAsync("수요", [new("c", "C")]);
        var store = new GatedLoadWorshipListStore(inner);
        using var vm = CreateSut(seedSampleQueue: false, worshipLists: store);
        await vm.LoadWorshipListAsync("주일");

        var gate = store.Gate = new TaskCompletionSource();
        var load = vm.LoadWorshipListAsync("수요");
        vm.AddTextItem("늦은 광고").Should().NotBeNull();
        gate.SetResult();
        await load;

        (await inner.LoadAsync("주일")).Should().HaveCount(2, "the edit belongs to the list that was on screen");
        vm.CurrentWorshipListName.Should().Be("수요");
        vm.Queue.Select(item => item.Id).Should().Equal("c");
        vm.HasPendingWorshipListSave.Should().BeFalse();
    }

    [Fact]
    public async Task ARecoveredQueue_IsAnEditOfItsOwnList_AndNeverOfAnotherList()
    {
        // The caption console's "예배 편집 복구": the operator asked for the saved rows.
        var store = new InMemoryWorshipListStore();
        await store.SaveAsync("주일", [new("a", "A")]);
        using var vm = CreateSut(seedSampleQueue: false, worshipLists: store);
        await vm.LoadWorshipListAsync("주일");

        vm.LoadRecoveredQueue([new("r1", "R1"), new("r2", "R2")], "주일");
        vm.HasPendingWorshipListSave.Should().BeTrue("the recovered rows are saved like any other edit");
        (await vm.SavePendingWorshipListChangesAsync()).Should().BeTrue();
        (await store.LoadAsync("주일")).Select(item => item.Id).Should().Equal("r1", "r2");

        vm.LoadRecoveredQueue([new("x", "X")], "수요");
        vm.CurrentWorshipListName.Should().BeEmpty("rows recovered for another list must not be saved over this one");
        vm.HasPendingWorshipListSave.Should().BeFalse();
        vm.HasUnnamedWorshipListChanges.Should().BeTrue("closing asks before they are dropped");
    }

    [Fact]
    public async Task RestoringAClearedListBeforeAnyEdit_KeepsItApartFromTheListOnScreen()
    {
        // After Clear the list on screen is empty, keeps its name and has no file: the restored list must still
        // get the time-stamp name, or the first row added on screen would be saved over it.
        using var folder = TempSettingsFolder.Create();
        var store = new WorshipListStore(Path.Combine(folder.Root, "Lists"));
        await store.SaveAsync("주일", [new("a", "A"), new("b", "B")]);
        using var vm = CreateSut(seedSampleQueue: false, settings: folder.CreateSettings(), worshipLists: store);
        await vm.LoadWorshipListAsync("주일");
        vm.ArchiveAndClearCurrentWorshipList();

        vm.RestoreWorshipLists(["주일"]).Should().Be(1);
        vm.AddTextItem("새 광고").Should().NotBeNull();
        (await vm.SavePendingWorshipListChangesAsync()).Should().BeTrue("the app closes");

        (await ReviewSavedListsAsync(store)).Should().Contain("a,b");
    }

    [Fact]
    public async Task RowsAddedToAQueueWithoutAName_AreReportedForTheClosePrompt()
    {
        using var vm = CreateSut(seedSampleQueue: false, worshipLists: new InMemoryWorshipListStore());
        vm.LoadQueue([new("a", "A")]);
        vm.HasUnnamedWorshipListChanges.Should().BeFalse("a queue that was loaded has no edits");

        vm.AddTextItem("광고").Should().NotBeNull();
        vm.HasUnnamedWorshipListChanges.Should().BeTrue("nothing saves these rows");

        await vm.SaveWorshipListAsync("주일");
        vm.HasUnnamedWorshipListChanges.Should().BeFalse();
    }

    [Fact]
    public async Task RenamingAList_CarriesItsSessionNotes()
    {
        using var folder = TempSettingsFolder.Create();
        var notes = new WorshipSessionNotesService(Path.Combine(folder.Root, "Notes"));
        var store = new InMemoryWorshipListStore();
        await store.SaveAsync("주일", [new("a", "A")]);
        notes.SetNotes("주일", "찬양 후 광고").Should().BeTrue();
        using var vm = CreateSut(seedSampleQueue: false, settings: folder.CreateSettings(), worshipLists: store, sessionNotes: notes);

        vm.RenameWorshipList("주일", "주일 1부").Should().BeTrue();

        notes.GetNotes("주일 1부").Should().Be("찬양 후 광고");
    }

    [Fact]
    public async Task RemovingSeveralRowsWithTheLiveRow_PutsTheNewSelectionOnOutput_AndAnEmptiedListEndsTheShow()
    {
        // FrmMain RemoveWorshipListSong + UpdateOutputCurItemNo.
        using var sut = CreateSut(seedSampleQueue: false);
        sut.LoadQueue([
            ReviewSong("song:1", "A", "a1", fromLibrary: false),
            ReviewSong("song:2", "B", "b1", fromLibrary: false),
            ReviewSong("song:3", "C", "c1", fromLibrary: false),
        ]);
        sut.OpenOutputCommand.Execute(null);
        sut.SelectedItem = sut.Queue[1];
        await sut.GoLiveCommand.ExecuteAsync(null);

        var rows = new[] { sut.Queue[0], sut.Queue[1] };
        sut.SelectedQueueRows = () => rows;
        sut.SelectedItem = sut.Queue[1];
        sut.RemoveSelectedItemCommand.Execute(null);

        sut.Queue.Select(item => item.Title).Should().Equal("C");
        sut.Session.Current.CurrentItemTitle.Should().Be("C", "the newly selected row is copied to output and shown");

        sut.SelectedQueueRows = null;
        sut.SelectedItem = sut.Queue[0];
        sut.RemoveSelectedItemCommand.Execute(null);
        sut.Session.Current.State.Should().Be(LiveState.Off, "a show ends when the list becomes empty");
    }

    [Fact]
    public async Task TheLiveRowIsARow_NotAnId_ForTheBadgeAndAfterAPageMoveUnderBlack()
    {
        using var sut = CreateSut(seedSampleQueue: false);
        sut.LoadQueue([
            ReviewSong("song:1", "A", "a1\n\na2", fromLibrary: false),
            ReviewSong("song:1", "A", "a1\n\na2", fromLibrary: false),
            ReviewSong("song:3", "C", "c1", fromLibrary: false),
        ]);
        sut.OpenOutputCommand.Execute(null);
        sut.SelectedItem = sut.Queue[1];
        await sut.GoLiveCommand.ExecuteAsync(null);

        sut.LiveRow.Should().BeSameAs(sut.Queue[1]);
        var badge = new Axicultus.Wpf.Composites.LiveRowBadgeConverter();
        var culture = System.Globalization.CultureInfo.InvariantCulture;
        badge.Convert([sut.Queue[0], sut.LiveRow!], typeof(System.Windows.Visibility), null, culture).Should().Be(System.Windows.Visibility.Collapsed);
        badge.Convert([sut.Queue[1], sut.LiveRow!], typeof(System.Windows.Visibility), null, culture).Should().Be(System.Windows.Visibility.Visible);

        await sut.ToggleOutputBlackCommand.ExecuteAsync(null);
        await sut.NextOutputSlideCommand.ExecuteAsync(null);
        sut.Session.Current.CurrentLyricsPageIndex.Should().Be(1);
        sut.LiveRow.Should().BeSameAs(sut.Queue[1], "a page move under Black must not hand the live row to the first copy");
        await sut.NextOutputItemCommand.ExecuteAsync(null);
        sut.Session.Current.CurrentItemTitle.Should().Be("C");
    }
}