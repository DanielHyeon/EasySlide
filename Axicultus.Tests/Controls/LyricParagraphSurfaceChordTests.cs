using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Axicultus.Wpf.Controls;
using Axicultus.Wpf.Shell;
using FluentAssertions;
using Xunit;

namespace Axicultus.Wpf.Tests.Controls;

/// <summary>Where the lyric surface draws the chords the database stores with a character position.</summary>
public class LyricParagraphSurfaceChordTests
{
    private static readonly Typeface Face = new("Malgun Gothic");
    private const double Size = 40;
    private const int ChordPercent = 75;

    private static LyricParagraphSurface Measured(string lyric, string record, TextAlignment alignment, double width)
    {
        string text = LyricsDisplayFormatter.ExpandNotations(LyricsDisplayFormatter.WithStoredChords(lyric, record), true);
        var surface = new LyricParagraphSurface
        {
            Paragraph = new LyricParagraphStyle(text, Face, Size, 1.2, Brushes.White, alignment, FlowDirection.LeftToRight,
                false, false, false, false, false, width, 600, [0], ChordPercent),
        };
        surface.Measure(new Size(width, 600));
        return surface;
    }

    private static double Width(LyricParagraphSurface surface, string text, double size = Size) =>
        new FormattedText(text, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, Face, size, Brushes.White,
            VisualTreeHelper.GetDpi(surface).PixelsPerDip).WidthIncludingTrailingWhitespace;

    [Fact]
    public void StoredChords_StartAtTheLeftEdgeOfTheirLyricCharacter()
    {
        StaHelper.RunOnSta(() =>
        {
            // Song 3327, line 1: Am is stored on character 6 of the line, the space after "주의 친절한".
            const string lyric = "주의 친절한 팔에 안기세";
            var centred = Measured(lyric, "(0;G;0;Am;6;)", TextAlignment.Center, 900);
            double left = (900 - Width(centred, lyric)) / 2;

            var chords = centred.PlacedChords().ToArray();
            chords.Select(chord => (chord.Line, chord.Chord)).Should().Equal((0, "G"), (0, "Am"));
            chords[0].X.Should().BeApproximately(left, 1);
            chords[1].X.Should().BeApproximately(left + Width(centred, "주의 친절한"), 1);

            var leftAligned = Measured(lyric, "(0;G;0;Am;6;)", TextAlignment.Left, 900).PlacedChords().ToArray();
            leftAligned[0].X.Should().BeApproximately(0, 1);
            leftAligned[1].X.Should().BeApproximately(Width(centred, "주의 친절한"), 1);
        });
    }

    [Fact]
    public void StoredChords_OnOneCharacterOrAtTheEndOfTheLine_FollowEachOther()
    {
        StaHelper.RunOnSta(() =>
        {
            // Song 3508: C and D are both on character 9. Song 3315: D and Em are both at the end of "And I".
            const string lyric = "뭇천사도 나와서 영접하네";
            var surface = Measured(lyric, "(0;G;0;Em;4;C;9;D;9;G;10;)", TextAlignment.Left, 900);
            var chords = surface.PlacedChords().ToArray();
            double chordSize = Size * ChordPercent / 100;

            chords.Select(chord => chord.Chord).Should().Equal("G", "Em", "C", "D", "G");
            chords[2].X.Should().BeApproximately(Width(surface, "뭇천사도 나와서 "), 1);
            chords[3].X.Should().BeGreaterThan(chords[2].X + Width(surface, "C", chordSize), "D does not cover C");
            chords[4].X.Should().BeGreaterThan(chords[3].X + Width(surface, "D", chordSize));
            chords[4].X.Should().BeLessThan(Width(surface, "뭇천사도 나와서 영접"), "G stays by its own character");

            var end = Measured("And I", "(0;G;3;D;5;Em;5;)", TextAlignment.Left, 900);
            var atEnd = end.PlacedChords().ToArray();
            atEnd[0].X.Should().BeApproximately(Width(end, "And"), 1);
            atEnd[1].X.Should().BeGreaterThan(atEnd[0].X + Width(end, "G", chordSize));
            atEnd[2].X.Should().BeGreaterThan(atEnd[1].X + Width(end, "D", chordSize));
            Measured("And I", "(0;D;5;)", TextAlignment.Left, 900).PlacedChords().Single().X
                .Should().BeApproximately(Width(end, "And I"), 1, "a chord stored at the end starts where the line ends");
        });
    }

    [Fact]
    public void StoredChords_OverALyricLineThatWraps_StandOverTheRowOfTheirCharacter()
    {
        StaHelper.RunOnSta(() =>
        {
            // Am is stored on the space before "팔에": in the narrow box that word starts a later row.
            const string lyric = "주의 친절한 팔에 안기세";
            var wide = Measured(lyric, "(0;G;0;Am;6;)", TextAlignment.Left, 900);
            var narrow = Measured(lyric, "(0;G;0;Am;6;)", TextAlignment.Left, 200);

            var chords = narrow.PlacedChords().ToArray();
            chords.Select(chord => chord.Chord).Should().Equal("G", "Am");
            chords[0].Line.Should().Be(0);
            chords[1].Line.Should().BeGreaterThan(1, "a lyric row lies between the two chord rows");
            chords[1].X.Should().BeApproximately(0, 1, "the row of 팔에 starts at the left edge");
            narrow.DesiredSize.Height.Should().BeGreaterThan(wide.DesiredSize.Height);

            // A wrapped line with a hanging indent keeps the chord line as text.
            string text = LyricsDisplayFormatter.ExpandNotations(LyricsDisplayFormatter.WithStoredChords(lyric, "(0;G;0;Am;6;)"), true);
            var indented = new LyricParagraphSurface
            {
                Paragraph = new LyricParagraphStyle(text, Face, Size, 1.2, Brushes.White, TextAlignment.Left, FlowDirection.LeftToRight,
                    false, false, false, true, false, 200, 600, [0], ChordPercent),
            };
            indented.Measure(new Size(200, 600));
            indented.PlacedChords().Should().BeEmpty();

            // Both draw their chord row.
            foreach (var surface in new[] { wide, narrow })
            {
                surface.Arrange(new Rect(surface.DesiredSize));
                int width = (int)surface.DesiredSize.Width, chordRow = (int)(Size * ChordPercent / 100);
                var bitmap = new RenderTargetBitmap(width, chordRow, 96, 96, PixelFormats.Pbgra32);
                bitmap.Render(surface);
                byte[] pixels = new byte[width * chordRow * 4];
                bitmap.CopyPixels(pixels, width * 4, 0);
                pixels.Should().Contain(value => value != 0);
            }
        });
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void OutputAndStage_PlaceTheStoredChordsOfTheLiveSong(bool projectorShowsChords)
    {
        StaHelper.RunOnSta(() =>
        {
            // The preview pane, the output window and the stage monitor all draw what this view model hands
            // the surface; the stage has its own switch (on by default) and shows chords either way.
            var session = new LiveSessionService();
            session.GoLive(new LiveQueueItem("song:3327", "주의 친절한 팔에 안기세", LiveItemKinds.Song)
            {
                Lyrics = "[1]\n주의 친절한 팔에 안기세\n우리 맘이 평안하리니", Notations = "(1;G;0;Am;6;)(2;G;0;D;9;)",
                ShowNotations = projectorShowsChords,
            }, "projector");
            using var view = new OutputWindowViewModel();
            view.ApplyOutput(new(true, new OutputDisplay("projector", "Projector", 0, 0, 1920, 1080, 1), new(0, 0, 1920, 1080, false)));
            view.ApplySession(session.Current);

            string[] Placed(LyricParagraphStyle? paragraph)
            {
                if (paragraph == null) return [];
                var surface = new LyricParagraphSurface { Paragraph = paragraph };
                surface.Measure(new Size(paragraph.Width, paragraph.Height));
                return surface.PlacedChords().Select(chord => $"{chord.Line}:{chord.Chord}").ToArray();
            }

            Placed(view.StageParagraph).Should().Equal("0:G", "0:Am", "2:G", "2:D");
            if (projectorShowsChords) Placed(view.PrimaryParagraph).Should().Equal("0:G", "0:Am", "2:G", "2:D");
            else view.BodyText.Should().Be("주의 친절한 팔에 안기세\n우리 맘이 평안하리니");
            // A page that is too tall with its chord rows is drawn smaller; without chords nothing changes.
            (view.PrimaryParagraph?.ShrinkToFit ?? false).Should().Be(projectorShowsChords);
        });
    }

    [Fact]
    public void TypedChords_KeepTheirOwnLine()
    {
        StaHelper.RunOnSta(() =>
        {
            var surface = new LyricParagraphSurface
            {
                Paragraph = new LyricParagraphStyle("G  Am\n주의 친절한 팔에 안기세", Face, Size, 1.2, Brushes.White, TextAlignment.Center,
                    FlowDirection.LeftToRight, false, false, false, false, false, 900, 600, [0], ChordPercent),
            };
            surface.Measure(new Size(900, 600));

            surface.PlacedChords().Should().BeEmpty();
        });
    }
}
