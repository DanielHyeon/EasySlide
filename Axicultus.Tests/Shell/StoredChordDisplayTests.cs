using System.Linq;
using Axicultus.Wpf.Controls;
using Axicultus.Wpf.Data;
using Axicultus.Wpf.Shell;
using FluentAssertions;
using Xunit;

namespace Axicultus.Wpf.Tests.Shell;

/// <summary>
/// Chords the database keeps apart from the lyrics (MSC) on the display: from the queue item to the pages.
/// The lyrics and records of the first three tests are those of real songs.
/// </summary>
public class StoredChordDisplayTests
{
    // LyricsDisplayFormatter.PlacedChordMark: starts a chord line written from stored chords.
    private const string M = "\u2060";

    private const string ArmsLyrics =
        "[1]\n주의 친절한 팔에 안기세\n우리 맘이 평안하리니\n항상 기쁘고 복이 되겠네\n영원하신 팔에 안기세\n" +
        "[chorus]\n주의 팔에\n그 크신 팔에 안기세\n주의 팔에\n영원하신 팔에 안기세\n" +
        "[2]\n날이 갈수록 주의 사랑이\n두루 광명하게 비치고\n천성 가는길 편히 가리니\n영원하신 팔에 안기세\n" +
        "[3]\n주의 보좌로 나아갈 때에\n기뻐 찬미 소리 외치고\n겁과 두려움 없어 지리니\n영원하신 팔에 안기세";

    private const string ArmsChords =
        "(1;G;0;Am;6;)(2;G;0;D;9;)(3;G;0;Am;6;)(4;G;0;D;7;G;8;)(6;G;0;C;3;)(7;G;0;D;9;)(8;G;0;C;3;)(9;G;0;D;6;G;8;)";

    private static readonly string ArmsVerse1 =
        $"{M}G     Am\n주의 친절한 팔에 안기세\n{M}G        D\n우리 맘이 평안하리니\n" +
        $"{M}G     Am\n항상 기쁘고 복이 되겠네\n{M}G      D {M}G\n영원하신 팔에 안기세";

    private static readonly string ArmsChorus =
        $"{M}G  C\n주의 팔에\n{M}G        D\n그 크신 팔에 안기세\n{M}G  C\n주의 팔에\n{M}G     D G\n영원하신 팔에 안기세";

    private static LiveQueueItem Arms(int page, bool showNotations, string? sequence = null) =>
        new("song:3327", "주의 친절한 팔에 안기세", LiveItemKinds.Song)
        {
            Lyrics = ArmsLyrics, Notations = ArmsChords, Sequence = sequence, LyricsPageIndex = page, ShowNotations = showNotations,
        };

    private static LiveSessionSnapshot Show(LiveQueueItem item) =>
        LiveSessionService.CreateSnapshot(LiveSessionSnapshot.Off, item, string.Empty, LiveState.Active, false, false);

    [Fact]
    public void StoredChords_StandAboveTheirLines_TheLineCountedWithHeadings()
    {
        // The record index counts every line of the stored lyrics: "[1]" is line 0, "[chorus]" line 5.
        var verse = Show(Arms(0, true));
        verse.CurrentItemBodyText.Should().Be(ArmsVerse1);
        verse.NotationLines.Should().Equal(0, 2, 4, 6);

        Show(Arms(1, true)).CurrentItemBodyText.Should().Be(ArmsChorus);
        Show(Arms(2, true)).CurrentItemBodyText.Should().Be(
            "날이 갈수록 주의 사랑이\n두루 광명하게 비치고\n천성 가는길 편히 가리니\n영원하신 팔에 안기세", "verse 2 has no record");
    }

    [Fact]
    public void StoredChords_KeepTheCharacterTheyStandOver_AlsoWhenTwoAreOnOneCharacter()
    {
        // Song 3508, verse 4: C and D are both stored on character 9.
        var shown = LyricsDisplayFormatter.ExpandNotations(
            LyricsDisplayFormatter.WithStoredChords("[4]\n뭇천사도 나와서 영접하네", "(1;G;0;Em;4;C;9;D;9;G;10;)"), true);

        var lines = shown.Split('\n');
        lines.Should().HaveCount(3);
        lines[2].Should().Be("뭇천사도 나와서 영접하네");
        LyricsDisplayFormatter.ReadPlacedChords(lines[1]).Should().Equal(("G", 0), ("Em", 4), ("C", 9), ("D", 9), ("G", 10));
        lines[1].Replace(M, "").Should().Be("G   Em   C D G", "shown as plain text the chords stay apart");
        LyricsDisplayFormatter.ReadPlacedChords("G   Em").Should().BeNull("chords typed after the marker have no position");
    }

    [Fact]
    public void StoredChords_SkipBlankLinesAndStayOnTheirScreen()
    {
        // Song 6762, verse 1: the stored lines end with a space, line 5 is the blank line between two screens,
        // and E7 is stored one character after the end of its line.
        const string lyrics = "[1]\n우리 주의 성령이 내게 임하여 \n주를 찬양합니다\n우리 주의 성령이 내게 임하여 \n주를 찬양합니다 \n\n" +
            "찬양합니다 찬양합니다 \n주를 찬양합니다 \n찬양합니다 찬양합니다 \n주를 찬양합니다";
        const string chords = "(1;B7;0;Em;3;C;12;)(2;B7;3;Em;7;)(3;B7;0;Em;3;C;12;)(4;B7;3;Em;7;E7;9;)" +
            "(6;Am;2;Em;8;)(7;B7;3;Em;7;E7;9;)(8;Am;2;Em;8;)(9;B7;3;Em;7;)";
        var item = new LiveQueueItem("song:6762", "우리 주의 성령이", LiveItemKinds.Song) { Lyrics = lyrics, Notations = chords, ShowNotations = true };

        Show(item).CurrentItemBodyText.Should().StartWith($"{M}B7 Em       C\n우리 주의 성령이 내게 임하여\n{M}   B7  Em\n주를 찬양합니다\n");
        var second = Show(item with { LyricsPageIndex = 1 }).CurrentItemBodyText.Split('\n');
        second.Should().HaveCount(8);
        second[0].Should().Be($"{M}  Am    Em");
        second[1].Should().Be("찬양합니다 찬양합니다");
        LyricsDisplayFormatter.ReadPlacedChords(second[2]).Should().Equal(("B7", 3), ("Em", 7), ("E7", 8));
    }

    [Fact]
    public void StoredChords_AreReadInOrder_OneRecordAtMostPerLine()
    {
        // gfLyrics.ExtractLyricRegions: the record of line 1 comes after that of line 2 and is passed over,
        // and reading it uses up line 3, so the record of line 3 is read too late.
        LyricsDisplayFormatter.WithStoredChords("zero\none\ntwo\nthree\nfour", "(2;A;0;)(1;B;0;)(3;C;0;)(4;D;0;)")
            .Should().Be($"zero\none\ntwo»{M}A;0;\nthree\nfour");
        LyricsDisplayFormatter.WithStoredChords("zero\none", "(0;A;0;)(1;B;1;").Should().Be($"zero»{M}A;0;\none", "an unfinished record ends the list");
    }

    [Fact]
    public void StoredChords_OnAHeadingOrAnEmptyLine_AreNotShown()
    {
        const string lyrics = "[1]\none\n\ntwo\n[region 2]\nthree";

        LyricsDisplayFormatter.WithStoredChords(lyrics, "(0;A;0;)(2;B;0;)(4;C;0;)(5;D;2;)").Should().Be($"[1]\none\n\ntwo\n[region 2]\nthree»{M}D;2;");
        LyricsDisplayFormatter.WithStoredChords(lyrics, "").Should().BeSameAs(lyrics);
        LyricsDisplayFormatter.WithStoredChords(lyrics, null).Should().BeSameAs(lyrics);
    }

    [Fact]
    public void StoredChords_FollowTheSequence()
    {
        Show(Arms(2, true, "1,c,1,c")).CurrentItemBodyText.Should().Be(ArmsVerse1, "the third page is verse 1 again");
        Show(Arms(3, true, "1,c,1,c")).CurrentItemBodyText.Should().Be(ArmsChorus);
    }

    [Fact]
    public void StoredChords_OfTheSecondRegion_AreCountedPastTheRegionMarker()
    {
        var item = new LiveQueueItem("song:1", "Amazing grace", LiveItemKinds.Song)
        {
            Lyrics = "[1]\nAmazing grace\n[region 2]\n놀라운 은혜\n[2]\nSecond verse\n[region 2]\n둘째 절",
            Notations = "(1;G;0;C;8;)(3;D;0;A;4;)(7;Em;1;)",
            ShowNotations = true,
        };

        var first = Show(item);
        first.CurrentItemBodyText.Should().Be($"{M}G       C\nAmazing grace");
        first.CurrentItemBodyText2.Should().Be($"{M}D   A\n놀라운 은혜");
        first.NotationLines.Should().Equal(0);
        first.NotationLines2.Should().Equal(0);

        var second = Show(item with { LyricsPageIndex = 1 });
        second.CurrentItemBodyText.Should().Be("Second verse");
        second.CurrentItemBodyText2.Should().Be($"{M} Em\n둘째 절");
    }

    [Fact]
    public void StoredChords_AreTransposedLikeTypedOnes_AndStayOnTheirCharacters()
    {
        var item = new LiveQueueItem("song:1", "t", LiveItemKinds.Song)
        {
            Lyrics = "stored line here\ntyped line » G  Am", Notations = "(0;G;0;Am;7;D/F#;12;)", ShowNotations = true, TransposeSemitones = 2,
        };

        var lines = Show(item).CurrentItemBodyText.Split('\n');

        LyricsDisplayFormatter.ReadPlacedChords(lines[0]).Should().Equal(("A", 0), ("Bm", 7), ("E/G#", 12));
        lines[2].Should().Be("A  Bm");
    }

    [Fact]
    public void ALineWithTypedAndStoredChords_ShowsOneChordLine()
    {
        var item = new LiveQueueItem("song:1", "t", LiveItemKinds.Song)
        {
            Lyrics = "[1]\n가사 한 줄 » Am F\n둘째 줄 » C", Notations = "(1;G;0;D;3;)", ShowNotations = true,
        };

        Show(item).CurrentItemBodyText.Should().Be($"{M}G  D\n가사 한 줄\nC\n둘째 줄", "the stored chords are shown for line 1, the typed ones for line 2");
        Show(item with { ShowNotations = false }).CurrentItemBodyText.Should().Be("가사 한 줄\n둘째 줄");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("1,c,2,c,3,c")]
    public void WithNotationsOff_ASongWithStoredChords_ShowsThePagesItShowsWithoutThem(string? sequence)
    {
        var plain = new LiveQueueItem("song:3327", "t", LiveItemKinds.Song) { Lyrics = ArmsLyrics, Sequence = sequence };
        var withChords = plain with { Notations = ArmsChords };
        int pages = LyricsDisplayFormatter.ToVersePages(ArmsLyrics, sequence).Count;

        LyricsDisplayFormatter.ToVersePages(withChords.LyricsWithChords, sequence).Should().Equal(LyricsDisplayFormatter.ToVersePages(ArmsLyrics, sequence));
        LyricsDisplayFormatter.GetSectionLabels(withChords.LyricsWithChords, sequence).Should().Equal(LyricsDisplayFormatter.GetSectionLabels(ArmsLyrics, sequence));
        for (int page = 0; page < pages; page++)
        {
            var shown = Show(withChords with { LyricsPageIndex = page });
            var today = Show(plain with { LyricsPageIndex = page });
            shown.CurrentItemBodyText.Should().Be(today.CurrentItemBodyText);
            shown.CurrentItemBodyText2.Should().Be(today.CurrentItemBodyText2);
            shown.NotationLines.Should().BeEmpty();
            shown.CurrentSectionLabel.Should().Be(today.CurrentSectionLabel);
        }

        // The stage monitor has its own switch and gets the chords either way.
        Show(withChords).StageRegion1!.WithChords.Should().Be(ArmsVerse1);
        Show(withChords).StageRegion1!.Lyrics.Should().Be(Show(plain).CurrentItemBodyText);
    }

    [Fact]
    public void StoredChords_StayOnTheirLines_WhenVersesAreGatheredAsFrmMainGathersThem()
    {
        // The record index counts the stored lines; which screen a line ends up on does not move its chords:
        // the text before "[1]" and verse 1 are one screen, the second chorus block belongs to the first.
        var item = new LiveQueueItem("song:1", "t", LiveItemKinds.Song)
        {
            Lyrics = "작사 작곡\n[1]\nverse one\n[chorus]\nrefrain\n[2]\nverse two\n[chorus]\nagain\n \nlast",
            Notations = "(2;G;0;)(4;D;2;)(6;C;6;)(8;Em;1;)(9;A;0;)(10;F;0;)",
            ShowNotations = true,
        };

        Show(item).CurrentItemBodyText.Should().Be($"작사 작곡\n{M}G\nverse one");
        Show(item with { LyricsPageIndex = 1 }).CurrentItemBodyText.Should().Be($"{M}  D\nrefrain\n{M} Em\nagain\n\n{M}F\nlast", "a line of spaces takes no chord");
        Show(item with { LyricsPageIndex = 2 }).CurrentItemBodyText.Should().Be($"{M}      C\nverse two");
        Show(item with { LyricsPageIndex = 1, ShowNotations = false }).CurrentItemBodyText.Should().Be("refrain\nagain\n\nlast");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("1,c,2,c")]
    public void OverflowPages_KeepTheChordsOfTheirLines_AndAreTheSamePagesWithNotationsOff(string? sequence)
    {
        // A slide this low holds one line: every lyric line is a page of its own (MainViewModel.GetSongPages).
        var layout = new SongPageLayout(60, "Arial", 10, 6, 2, 0, 1, 80, 0);
        var item = Arms(0, true, sequence);

        var today = layout.Paginate(ArmsLyrics, sequence, null, 100, 6);
        var pages = layout.Paginate(item.LyricsWithChords!, sequence, null, 100, 6);

        pages.Labels.Should().Equal(today.Labels);
        pages.Bodies.Select(body => LyricsDisplayFormatter.FormatSongSlide(body, false, 0))
            .Should().Equal(today.Bodies.Select(body => LyricsDisplayFormatter.FormatSongSlide(body, false, 0)));
        today.Bodies.Should().HaveCountGreaterThan(8).And.OnlyContain(body => !body.Contains('\n'));

        // Page 3 is the fourth line of verse 1, page 4 the first line of the chorus.
        var fourth = Show(item with { LyricsPageIndex = 3, ProjectedTextPage = pages.Bodies[3] });
        fourth.CurrentItemBodyText.Should().Be($"{M}G      D {M}G\n영원하신 팔에 안기세");
        fourth.NotationLines.Should().Equal(0);
        Show(item with { LyricsPageIndex = 4, ProjectedTextPage = pages.Bodies[4] }).CurrentItemBodyText.Should().Be($"{M}G  C\n주의 팔에");
        Show(item with { LyricsPageIndex = 3, ProjectedTextPage = pages.Bodies[3], ShowNotations = false }).CurrentItemBodyText
            .Should().Be("영원하신 팔에 안기세");
    }
}
