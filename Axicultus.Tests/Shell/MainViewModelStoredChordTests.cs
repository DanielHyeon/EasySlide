using System.Linq;
using System.Threading.Tasks;
using Axicultus.Wpf.Controls;
using Axicultus.Wpf.Data;
using Axicultus.Wpf.Settings;
using Axicultus.Wpf.Shell;
using FluentAssertions;
using Xunit;

namespace Axicultus.Wpf.Tests.Shell;

// The chords the database keeps apart from the lyrics (MSC), from a library song to the preview and the output.
public partial class MainViewModelTests
{
    private const string StoredChordMark = "\u2060";
    private const string StoredChordLyrics = "[1]\n주의 친절한 팔에 안기세\n우리 맘이 평안하리니\n[chorus]\n주의 팔에";
    private const string StoredChordRecords = "(1;G;0;Am;6;)(2;G;0;D;9;)(4;G;0;C;3;)";

    [Fact]
    public async Task LibrarySongWithStoredChords_ShowsThemInPreviewAndOutput_AndFollowsTheSwitchAndTranspose()
    {
        using var folder = TempSettingsFolder.Create();
        var settings = folder.CreateSettings();
        settings.Set(AxicultusSettingKeys.AdvanceNextItem, false);
        settings.Set(AxicultusSettingKeys.LyricsMonitorShowNotations, true);
        settings.Set(AxicultusSettingKeys.PreviewAreaShowNotations, true);
        var sut = CreateSut(settings: settings, seedSampleQueue: false);
        string verse = $"{StoredChordMark}G     Am\n주의 친절한 팔에 안기세\n{StoredChordMark}G        D\n우리 맘이 평안하리니";

        var item = sut.AddSong(new SongSummary(3327, "주의 친절한 팔에 안기세", "", 1, 0, "", "", StoredChordLyrics, Notations: StoredChordRecords))!;

        item.Lyrics.Should().Be(StoredChordLyrics, "the lyrics of the item stay as stored");
        sut.PreviewLyricsPages.Select(card => card.BodyText).Should().Equal(verse, $"{StoredChordMark}G  C\n주의 팔에");
        sut.OpenOutputCommand.Execute(null);
        await sut.GoLiveCommand.ExecuteAsync(null);
        sut.Session.Current.CurrentItemBodyText.Should().Be(verse);
        sut.Session.Current.NotationLines.Should().Equal(0, 2);

        sut.TransposeLiveUpCommand.Execute(null);
        sut.Session.Current.CurrentItemBodyText.Should().StartWith($"{StoredChordMark}G#    A#m\n");

        settings.Set(AxicultusSettingKeys.LyricsMonitorShowNotations, false);
        sut.Session.Current.CurrentItemBodyText.Should().Be("주의 친절한 팔에 안기세\n우리 맘이 평안하리니");
        sut.Session.Current.StageRegion1!.WithChords.Should().StartWith($"{StoredChordMark}G#    A#m\n", "the stage monitor has its own switch");
    }

    [Fact]
    public async Task SongWithStoredChords_OnOverflowPages_KeepsTheChordsOfEachLine()
    {
        using var folder = TempSettingsFolder.Create();
        var settings = folder.CreateSettings();
        settings.Set(AxicultusSettingKeys.AdvanceNextItem, false);
        settings.Set(AxicultusSettingKeys.AutoTextOverflow, true);
        settings.Set(AxicultusSettingKeys.LyricsMonitorShowNotations, true);
        var sut = CreateSut(settings: settings, seedSampleQueue: false);
        // A font this large leaves room for one line on a slide.
        sut.Library.Folders.Add(new SongFolderSummary(1, "Folder 1", false, 0, SlideLayout: new SongPageLayout(120, "Arial", 5, 4, 4, 0, 1, 100, 0)));
        var song = new SongSummary(3327, "주의 친절한 팔에 안기세", "", 1, 0, "", "", StoredChordLyrics, Notations: StoredChordRecords);
        int pagesWithoutChords = CountPages(song with { Notations = "" });

        sut.AddSong(song);
        sut.PreviewLyricsPages.Should().HaveCount(pagesWithoutChords).And.HaveCountGreaterThan(2, "the verse overflows");
        sut.GoToPreviewLyricsPageCommand.Execute(1);
        sut.OpenOutputCommand.Execute(null);
        await sut.GoLiveCommand.ExecuteAsync(null);

        sut.Session.Current.CurrentItemBodyText.Should().Be($"{StoredChordMark}G        D\n우리 맘이 평안하리니");

        int CountPages(SongSummary plain)
        {
            var other = CreateSut(settings: settings, seedSampleQueue: false);
            other.Library.Folders.Add(sut.Library.Folders[0]);
            other.AddSong(plain);
            return other.PreviewLyricsPages.Count;
        }
    }

    [Fact]
    public void EditedSong_BringsItsChordsToTheSelectedListItem()
    {
        var sut = CreateSut(seedSampleQueue: false);
        sut.AddSong(new SongSummary(3327, "주의 친절한 팔에 안기세", "", 1, 0, "", "", StoredChordLyrics));

        sut.UpdateSelectedSongQueueItem(
            new SongSummary(3327, "주의 친절한 팔에 안기세", "", 1, 0, "", "", StoredChordLyrics, Notations: StoredChordRecords), null, null)
            .Should().BeTrue();

        sut.SelectedItem!.Notations.Should().Be(StoredChordRecords);
    }

    [Fact]
    public void PreviewPane_DrawsTheStoredChords_WhenThePreviewSwitchIsOn()
    {
        // MainWindow.RefreshSample draws the preview pane from this projection.
        using var folder = TempSettingsFolder.Create();
        var settings = folder.CreateSettings();
        var sut = CreateSut(settings: settings, seedSampleQueue: false);
        sut.PreviewLibrarySong(new SongSummary(3327, "주의 친절한 팔에 안기세", "", 1, 0, "", "", StoredChordLyrics, Notations: StoredChordRecords));

        LiveSessionSnapshot Sample() => LiveSessionService.CreateSnapshot(LiveSessionSnapshot.Off,
            sut.ProjectPreviewLyricsPage(sut.PreviewPanelItem!, sut.LyricsPageIndex), string.Empty, LiveState.Active, false, false);

        Sample().NotationLines.Should().BeEmpty();
        Sample().CurrentItemBodyText.Should().Be(sut.PreviewLyricsText);

        settings.Set(AxicultusSettingKeys.PreviewAreaShowNotations, true);

        Sample().NotationLines.Should().Equal(0, 2);
        Sample().CurrentItemBodyText.Should().Be(sut.PreviewLyricsText).And.StartWith($"{StoredChordMark}G     Am\n");
    }
}
