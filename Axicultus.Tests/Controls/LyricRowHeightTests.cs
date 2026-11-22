using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Axicultus.Wpf.Controls;
using Axicultus.Wpf.Data;
using Axicultus.Wpf.Shell;
using FluentAssertions;
using Xunit;

namespace Axicultus.Wpf.Tests.Controls;

/// <summary>The distance from one row of lyrics or Bible text to the next on the output, against the WinForms rule.</summary>
public class LyricRowHeightTests
{
    // gfDisplay.DrawOneRegion: (int)(MeasureString("A", MainFont, layoutArea).Height * FontSpacingFactor), with
    // FontSpacingFactor = LineSpacing + 0.05 (ComputeShowLineSpacing) and MainFont the FOLDER points scaled from the
    // 960 reference to the output width in whole points (DisplayFontSize), measured on a bitmap of this machine.
    internal static double WinFormsRow(string font, double points, double lineSpacing)
    {
        using var bitmap = new System.Drawing.Bitmap(1, 1);
        using var graphics = System.Drawing.Graphics.FromImage(bitmap);
        using var gdi = new System.Drawing.Font(font, (float)points);
        return (int)(graphics.MeasureString("A", gdi, new System.Drawing.SizeF(1000, 32000)).Height * (lineSpacing + .05));
    }

    private static double OutputPoints(int points, int width) => (int)(points * width / 960d);

    private static SongPageLayout Folder(string font, int points, double lineSpacing, int region2Top = 40) =>
        new(points, font, 0, 5, 5, 0, lineSpacing, 100, 0)
        {
            Fields = new Dictionary<string, string>
            {
                ["LineSpacing2"] = lineSpacing.ToString(CultureInfo.InvariantCulture), ["Size1"] = points.ToString(CultureInfo.InvariantCulture),
                ["FontName1"] = font, ["Vpos1"] = region2Top.ToString(CultureInfo.InvariantCulture),
            },
        };

    private static OutputWindowViewModel Show(LiveQueueItem item, int width, int height)
    {
        var session = new LiveSessionService();
        session.GoLive(item, "projector");
        var view = new OutputWindowViewModel();
        view.ApplyOutput(new(true, new OutputDisplay("projector", "Projector", 0, 0, width, height, 1), new(0, 0, width, height, false)));
        view.ApplySession(session.Current);
        return view;
    }

    private static (double Top, double Height)[] Rows(LyricParagraphStyle? paragraph)
    {
        paragraph.Should().NotBeNull();
        var surface = new LyricParagraphSurface { Paragraph = paragraph };
        surface.Measure(new Size(paragraph!.Width, paragraph.Height));
        return [.. surface.Rows()];
    }

    private static void ShouldStepBy((double Top, double Height)[] rows, double expected, bool chords, string because)
    {
        rows.Length.Should().BeGreaterThan(0, because);
        for (int i = 0; i < rows.Length; i++)
        {
            // A chord row stands over each lyric row: NotationFontFactor (75%) of the lyric row (notationsLineHeight).
            double row = chords && i % 2 == 0 ? expected * 75 / 100 : expected;
            rows[i].Height.Should().BeApproximately(row, 1, $"row {i}: {because}");
            if (i > 0) (rows[i].Top - rows[i - 1].Top).Should().BeApproximately(rows[i - 1].Height, .01);
        }
    }

    public static TheoryData<string, int, double, int, int> Cases()
    {
        var data = new TheoryData<string, int, double, int, int>();
        foreach (var font in new[] { "Malgun Gothic", "Arial" })
            foreach (int points in new[] { 20, 36, 50 })
                foreach (double spacing in new[] { 1.0, 1.2 })
                    foreach (var (width, height) in new[] { (1920, 1080), (2048, 1152) })
                        data.Add(font, points, spacing, width, height);
        return data;
    }

    private const string Verse = "1:1 태초에 하나님이 천지를 창조하시니라 땅이 혼돈하고 공허하며 흑암이 깊음 위에 있고 하나님의 영은 수면 위에 운행하시니라";

    [Theory]
    [MemberData(nameof(Cases))]
    public void FolderLayout_SongAndBible_StepRowsByTheWinFormsRow(string font, int points, double spacing, int width, int height)
    {
        StaHelper.RunOnSta(() =>
        {
            double expected = WinFormsRow(font, OutputPoints(points, width), spacing);
            string at = $"{font} {points}pt x{spacing} at {width}x{height}";

            // Region 1 of a song, a wrapped line included, without and with chord rows.
            var song = new LiveQueueItem("song:1", "t", LiveItemKinds.Song)
            {
                Lyrics = "[1]\n주의 친절한 팔에\n안기세", Notations = "(1;G;0;Am;3;)(2;D;0;)", SongLayout = Folder(font, points, spacing),
            };
            var plain = Show(song with { ProjectedTextPage = "주의 친절한 팔에 안기세 우리 맘이 평안하리니 주의 친절한 팔에 안기세\n안기세" }, width, height);
            ShouldStepBy(Rows(plain.PrimaryParagraph), expected, false, "song " + at);
            var chords = Show(song with { ShowNotations = true }, width, height);
            chords.PrimaryParagraph!.NotationLines.Should().Equal(0, 2);
            ShouldStepBy(Rows(chords.PrimaryParagraph), expected, true, "song with chords " + at);

            // Region 2 of a two-language song, with chord rows.
            var twoLanguages = Show(new LiveQueueItem("song:2", "t", LiveItemKinds.Song)
            {
                Lyrics = "[1]\nAmazing grace\n[region 2]\n놀라운 은혜", Notations = "(1;G;0;)(3;D;0;)", ShowNotations = true,
                SongLayout = Folder(font, points, spacing, region2Top: 50),
            }, width, height);
            ShouldStepBy(Rows(twoLanguages.SecondaryParagraph), expected, true, "region 2 of a song " + at);

            // A Bible page in one version, and in two (region 2 from its own LineSpacing2).
            var bible = new LiveQueueItem("bible:1", "창 1:1", LiveItemKinds.Bible) { Lyrics = Verse, SongLayout = Folder(font, points, spacing) };
            ShouldStepBy(Rows(Show(bible with { ProjectedTextPage = Verse }, width, height).PrimaryParagraph), expected, false, "Bible " + at);
            var twoVersions = Show(bible with { ProjectedTextPage = "1:1 태초에" + SongPageLayout.Region2Break + "1:1 In the\n1:2 And the" }, width, height);
            ShouldStepBy(Rows(twoVersions.PrimaryParagraph), expected, false, "Bible region 1 " + at);
            ShouldStepBy(Rows(twoVersions.SecondaryParagraph), expected, false, "Bible region 2 " + at);
        });
    }

    [Theory]
    [InlineData("Malgun Gothic", 1920, 1080)]
    [InlineData("Arial", 2048, 1152)]
    public void ReducedFont_StepsRowsByTheWinFormsRowOfTheReducedFont(string font, int width, int height)
    {
        StaHelper.RunOnSta(() =>
        {
            // A first verse too high for a screen is cut with the font reduced from 50 to 40 points (SongPageLayout.BeginScreen).
            var view = Show(new LiveQueueItem("bible:1", "창 1:1", LiveItemKinds.Bible)
            {
                Lyrics = Verse, ProjectedTextPage = Verse, ProjectedFontScale = .8, SongLayout = Folder(font, 50, 1),
            }, width, height);
            view.PrimaryParagraph!.ShrinkToFit.Should().BeTrue();
            ShouldStepBy(Rows(view.PrimaryParagraph), WinFormsRow(font, OutputPoints(40, width), 1), false, $"{font} 40pt at {width}");
        });
    }

    [Fact]
    public void DefaultLayout_StepsRowsByTheGlobalSpacing()
    {
        StaHelper.RunOnSta(() =>
        {
            const int percent = 125; // LyricsMonitorLineSpacingPercent by default
            var song = new LiveQueueItem("song:1", "t", LiveItemKinds.Song) { Lyrics = "[1]\n주의 친절한 팔에\n안기세", Notations = "(1;G;0;)(2;D;0;)" };
            var view = Show(song, 1920, 1080);
            // No folder layout: the global font in pixels (48 by default) is the point font WinForms would measure.
            double expected = WinFormsRow(SystemFonts.MessageFontFamily.Source, view.BodyFontSize * 72 / 96, percent / 100d);

            view.PrimaryParagraph.Should().BeNull("the plain text block draws it");
            view.BodyLineHeight.Should().Be(expected);
            var block = new TextBlock
            {
                Text = view.BodyText, FontSize = view.BodyFontSize, FontFamily = SystemFonts.MessageFontFamily, LineHeight = view.BodyLineHeight,
            };
            block.Measure(new Size(1920, 1080));
            block.DesiredSize.Height.Should().BeApproximately(2 * expected, 2, "two rows of the WinForms row height");

            ShouldStepBy(Rows(Show(song with { ShowNotations = true }, 1920, 1080).PrimaryParagraph), expected, true, "default layout with chords");
        });
    }

    // The first row of ink: a taller row must not move the text down (WinForms draws it at the top of its row).
    private static int FirstInkRow(UIElement element, int width, int height)
    {
        element.Measure(new Size(width, height));
        element.Arrange(new Rect(0, 0, width, height));
        var bitmap = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
        bitmap.Render(element);
        var pixels = new byte[width * height * 4];
        bitmap.CopyPixels(pixels, width * 4, 0);
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                if (pixels[(y * width + x) * 4 + 3] > 128) return y;
        return -1;
    }

    [Theory]
    [InlineData("Malgun Gothic")]
    [InlineData("Arial")]
    public void TallerRows_KeepTheFirstRowWhereItStands(string font)
    {
        StaHelper.RunOnSta(() =>
        {
            var face = new Typeface(font);
            LyricParagraphSurface Surface(double leading) => new()
            {
                Paragraph = new LyricParagraphStyle("주의 친절한 Ag\n안기세", face, 80, leading, Brushes.White, TextAlignment.Left,
                    FlowDirection.LeftToRight, false, false, false, false, false, 900, 600, [], 75),
            };
            int natural = FirstInkRow(Surface(.5), 900, 600);
            natural.Should().BeGreaterThan(0);
            FirstInkRow(Surface(2), 900, 600).Should().BeInRange(natural - 1, natural + 1);

            // The plain text block keeps its first row by itself; the outline block is lifted as the surface.
            var block = new TextBlock { Text = "주의 친절한 Ag\n안기세", FontFamily = face.FontFamily, FontSize = 80, Foreground = Brushes.White, LineHeight = 160 };
            FirstInkRow(block, 900, 600).Should().BeInRange(natural - 1, natural + 1);
            var outlined = new OutlinedTextBlock
            {
                Text = "주의 친절한 Ag\n안기세", FontFamily = face.FontFamily, FontSize = 80, Fill = Brushes.White, LineHeight = 160,
                RenderTransform = new TranslateTransform(0, LyricParagraphSurface.LeadIn(face.FontFamily, 80, 160)),
            };
            FirstInkRow(outlined, 900, 600).Should().BeInRange(natural - 2, natural + 1, "its stroke reaches a pixel higher");
        });
    }
}
