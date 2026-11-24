using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Axicultus.Wpf.Captions;
using Xunit;

namespace Axicultus.Wpf.Tests.Shell;

// a035: the score of a hymn slide is its own transparent picture, apart from the background photo and the verse buttons.
public sealed class CaptionScorePictureTests
{
    private const long SlideWidth = 12192000, SlideHeight = 6858000;

    private static string Pic(string blip, long x, long y, long cx, long cy, string crop = "", string xfrm = "", bool hidden = false, bool link = false) =>
        $"""<p:pic><p:nvPicPr><p:cNvPr id="2" name="pic"{(hidden ? " hidden=\"1\"" : "")}>{(link ? "<a:hlinkClick r:id=\"img0\" action=\"ppaction://hlinksldjump\"/>" : "")}</p:cNvPr><p:cNvPicPr/><p:nvPr/></p:nvPicPr><p:blipFill><a:blip {blip}/>{crop}<a:stretch><a:fillRect/></a:stretch></p:blipFill><p:spPr><a:xfrm{xfrm}><a:off x="{x}" y="{y}"/><a:ext cx="{cx}" cy="{cy}"/></a:xfrm></p:spPr></p:pic>""";

    // Picture images[n], at x,y (EMU) and cx×cy.
    internal static string Image(int n, long cx, long cy, long x = 0, long y = 0, string crop = "", string xfrm = "", bool hidden = false) =>
        Pic($"r:embed=\"img{n}\"", x, y, cx, cy, crop, xfrm, hidden);

    // A group at x,y shown at cx×cy whose children are laid out from (5289, 3975) in chCx×chCy units, like the hymn decks' buttons.
    private static string Group(long x, long y, long cx, long cy, long chCx, long chCy, string children, string xfrm = "") =>
        $"""<p:grpSp><p:nvGrpSpPr><p:cNvPr id="3" name="group"/><p:cNvGrpSpPr/><p:nvPr/></p:nvGrpSpPr><p:grpSpPr><a:xfrm{xfrm}><a:off x="{x}" y="{y}"/><a:ext cx="{cx}" cy="{cy}"/><a:chOff x="5289" y="3975"/><a:chExt cx="{chCx}" cy="{chCy}"/></a:xfrm></p:grpSpPr>{children}</p:grpSp>""";

    // Each slide is its shapes; "img{n}" on every slide is images[n] at /ppt/media/image{n}.png.
    internal static string WriteDeck(string folder, byte[][] images, params string[] slides) =>
        WriteDeck(folder, [.. images.Select(bytes => ("image/png", "png", bytes))], slides);

    // Each image with its content type and extension.
    internal static string WriteDeck(string folder, (string Type, string Extension, byte[] Bytes)[] images, params string[] slides)
    {
        const string rel = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";
        var path = Path.Combine(folder, "악보.pptx");
        using var package = Package.Open(path, FileMode.Create);
        PackagePart Add(string name, string type, byte[] bytes)
        {
            var part = package.CreatePart(new Uri(name, UriKind.Relative), type, CompressionOption.Normal);
            using var stream = part.GetStream();
            stream.Write(bytes);
            return part;
        }
        var media = images.Select((image, n) => Add($"/ppt/media/image{n}.{image.Extension}", image.Type, image.Bytes)).ToArray();
        var ids = string.Concat(slides.Select((_, n) => $"<p:sldId id=\"{256 + n}\" r:id=\"s{n}\"/>"));
        var presentation = Add("/ppt/presentation.xml", "application/xml", Encoding.UTF8.GetBytes(
            $"""<p:presentation xmlns:p="http://schemas.openxmlformats.org/presentationml/2006/main" xmlns:r="{rel}"><p:sldIdLst>{ids}</p:sldIdLst><p:sldSz cx="{SlideWidth}" cy="{SlideHeight}"/></p:presentation>"""));
        package.CreateRelationship(presentation.Uri, TargetMode.Internal, rel + "/officeDocument");
        for (int n = 0; n < slides.Length; n++)
        {
            var slide = Add($"/ppt/slides/slide{n + 1}.xml", "application/xml", Encoding.UTF8.GetBytes(
                $"""<p:sld xmlns:p="http://schemas.openxmlformats.org/presentationml/2006/main" xmlns:a="http://schemas.openxmlformats.org/drawingml/2006/main" xmlns:r="{rel}"><p:cSld><p:spTree>{slides[n]}</p:spTree></p:cSld></p:sld>"""));
            presentation.CreateRelationship(PackUriHelper.GetRelativeUri(presentation.Uri, slide.Uri), TargetMode.Internal, rel + "/slide", $"s{n}");
            for (int m = 0; m < media.Length; m++)
                slide.CreateRelationship(PackUriHelper.GetRelativeUri(slide.Uri, media[m].Uri), TargetMode.Internal, rel + "/image", $"img{m}");
        }
        return path;
    }

    private static byte[] Png(int width, int height, Action<byte[]> paint)
    {
        var pixels = new byte[width * height * 4]; // Bgra32, all transparent
        paint(pixels);
        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(BitmapSource.Create(width, height, 96, 96, PixelFormats.Bgra32, null, pixels, width * 4)));
        using var stream = new MemoryStream();
        encoder.Save(stream);
        return stream.ToArray();
    }

    private static void Fill(byte[] pixels, int width, int x, int y, int w, int h, byte b, byte g, byte r, byte a)
    {
        for (int row = y; row < y + h; row++)
            for (int col = x; col < x + w; col++)
            {
                int i = (row * width + col) * 4;
                (pixels[i], pixels[i + 1], pixels[i + 2], pixels[i + 3]) = (b, g, r, a);
            }
    }

    // Black staves with a lyric line under each on a transparent picture, like the hymn decks' score pictures.
    internal static byte[] ScorePng() => Png(600, 450, pixels =>
    {
        foreach (var top in new[] { 100, 280 })
        {
            for (int line = 0; line < 5; line++) Fill(pixels, 600, 50, top + line * 10, 500, 2, 0, 0, 0, 255);
            Fill(pixels, 600, 150, top - 15, 10, 15, 0, 0, 0, 255); // a note above the staff
            Fill(pixels, 600, 80, top + 65, 400, 25, 0, 0, 0, 255); // its lyrics
        }
    });

    private static byte[] Photo(byte shade) => Png(64, 36, pixels => Fill(pixels, 64, 0, 0, 64, 36, shade, (byte)(shade / 2), 90, 255));

    private static T InFolder<T>(Func<string, T> test)
    {
        var folder = Directory.CreateTempSubdirectory();
        try { return test(folder.FullName); }
        finally { folder.Delete(true); }
    }

    internal const long Full = 9144000, Wide = 5143500; // 75% of the slide's width, 75% of its height

    [Fact]
    public void TheContentPictureIsTheOneVisiblePictureThatIsNeitherBackgroundNorButton() => StaHelper.RunOnSta(() => InFolder(folder =>
    {
        // img0 background photo, img1 the score, img2 a verse button, img3 another photo.
        var path = WriteDeck(folder, [Photo(40), ScorePng(), Photo(200), Photo(120)],
            // 1: the hymn slide: background over the whole slide, the score on it, a button.
            Image(0, SlideWidth, SlideHeight) + Image(1, Full, Wide, x: 1524000) + Image(2, 423862, 431800, x: 9920288, y: 6310313),
            // 2: a photo and the score in the same place: the one on top (the score).
            Image(3, Full, SlideHeight, x: 1524000) + Image(1, Full, SlideHeight, x: 1524000),
            // 3: the same, the photo on top.
            Image(1, Full, SlideHeight, x: 1524000) + Image(3, Full, SlideHeight, x: 1524000),
            // 4: a large picture under the slide-size background, the score on top of both.
            Image(3, Full, Wide, x: 1524000) + Image(0, SlideWidth, SlideHeight) + Image(1, 8000000, 5600000, x: 2000000),
            // 5: a hidden picture does not count.
            Image(1, Full, Wide) + Image(3, 10000000, 5400000, hidden: true),
            // 6: one score cut in two pieces (the top and the bottom of the same picture): the render.
            Image(1, Full, 1600000, crop: """<a:srcRect b="60000"/>""") + Image(1, Full, 2400000, y: 2000000, crop: """<a:srcRect t="40000"/>"""),
            // 7: a score filling 90% of the slide.
            Image(1, 11566000, 6507000, x: 300000, y: 150000),
            // 8: a rotated picture, a picture in a rotated group.
            Image(1, Full, Wide, xfrm: " rot=\"5400000\""),
            Group(0, 0, Full, Wide, Full, Wide, Image(1, Full, Wide, x: 5289, y: 3975), " rot=\"60000\""),
            // 10: a picture in a group is measured as the group shows it (267 child units stretched to the slide).
            Group(1524000, 0, Full, Wide, 267, 272, Image(1, 267, 200, x: 5289, y: 3975)),
            // 11: a picture in a group covering the score (only where the group's offsets put it).
            Image(1, Full, Wide, x: 1524000) + Group(1524000, 0, Full, Wide, 267, 272, Image(3, 267, 272, x: 5289, y: 3975)),
            // 12: a cropped picture keeps its crop as shares of each side; a negative share (a margin) counts as none.
            Image(1, Full, Wide, crop: """<a:srcRect l="25000" t="10000" r="-5000" b="50000"/>"""),
            // 13: background and a verse button (a slide link) only: the background is the one picture (no staves on it: the render); 14: a linked picture.
            Image(0, SlideWidth, SlideHeight) + Pic("r:embed=\"img2\"", 0, 0, 423862, 431800, link: true),
            Pic("r:link=\"img1\"", 0, 0, Full, Wide));
        var parts = CaptionDeck.Read(path).Slides.Select(s => s.Picture?.Part).ToArray();
        Assert.Equal(["/ppt/media/image1.png", "/ppt/media/image1.png", "/ppt/media/image3.png", "/ppt/media/image1.png",
            "/ppt/media/image1.png", null, "/ppt/media/image1.png", null, null, "/ppt/media/image1.png", "/ppt/media/image3.png",
            "/ppt/media/image1.png", "/ppt/media/image0.png", null], parts);
        var deck = CaptionDeck.Read(path);
        Assert.Equal(new CaptionPicture("/ppt/media/image1.png", 0, 0, 0, 0), deck.Slides[0].Picture);
        Assert.Equal(new CaptionPicture("/ppt/media/image1.png", 0.25, 0.10, 0, 0.50), deck.Slides[11].Picture);
        Assert.All(deck.Slides, slide => Assert.True(slide.HasPicture)); // no text on them: a picture of any kind makes a picture slide
        return 0;
    }));

    [Fact]
    public void TheContentPictureIsReadCroppedOverWhiteAndNotLargerThanTheCap() => StaHelper.RunOnSta(() => InFolder(folder =>
    {
        var picture = Png(200, 100, pixels =>
        {
            Fill(pixels, 200, 60, 0, 40, 50, 0, 0, 0, 128);     // half-transparent black
            Fill(pixels, 200, 100, 0, 100, 50, 0, 0, 255, 255); // opaque red
            Fill(pixels, 200, 0, 50, 200, 50, 255, 0, 0, 255);  // opaque blue below the crop
        });
        var huge = Png(3000, 120, pixels => Fill(pixels, 3000, 0, 0, 3000, 120, 0, 0, 0, 255));
        var path = WriteDeck(folder, [picture, huge],
            Image(0, Full, Wide, crop: """<a:srcRect l="25000" t="10000" r="-5000" b="50000"/>"""),
            Image(1, Full, Wide));
        var deck = CaptionDeck.Read(path);
        var image = CaptionDeck.ReadPicture(path, deck.Slides[0].Picture!);
        Assert.NotNull(image);
        Assert.True(image.IsFrozen);
        Assert.Equal((150, 40), (image.PixelWidth, image.PixelHeight)); // 50..200 × 10..50
        var bgra = new FormatConvertedBitmap(image, PixelFormats.Bgra32, null, 0);
        var pixels = new byte[150 * 40 * 4];
        bgra.CopyPixels(pixels, 150 * 4, 0);
        (int B, int G, int R) At(int x, int y) { int i = (y * 150 + x) * 4; return (pixels[i], pixels[i + 1], pixels[i + 2]); }
        Assert.Equal((255, 255, 255), At(5, 20));      // transparent: white
        Assert.InRange(At(30, 20).R, 120, 135);         // half-transparent black: grey
        Assert.Equal((0, 0, 255), At(100, 20));        // opaque red stays
        for (int y = 0; y < 40; y++) for (int x = 0; x < 150; x++) Assert.NotEqual((255, 0, 0), At(x, y)); // nothing of the cropped-off blue
        var capped = CaptionDeck.ReadPicture(path, deck.Slides[1].Picture!);
        Assert.Equal((CaptionDeck.MaxPictureSide, 96), (capped!.PixelWidth, capped.PixelHeight));
        Exception? why = null;
        Assert.Null(CaptionDeck.ReadPicture(path, new CaptionPicture("/ppt/media/missing.png", 0, 0, 0, 0), error => why = error));
        Assert.NotNull(why);
        Assert.Null(CaptionDeck.ReadPicture(Path.Combine(folder, "없음.pptx"), new CaptionPicture("/ppt/media/image0.png", 0, 0, 0, 0)));
        return 0;
    }));

    [Fact]
    public void ASlideUsesItsContentPictureOnlyWhenStavesAreFoundOnIt() => StaHelper.RunOnSta(() => InFolder(folder =>
    {
        var path = WriteDeck(folder, [Photo(40), ScorePng(), Photo(120)],
            Image(0, SlideWidth, SlideHeight) + Image(1, Full, Wide), // the score over a background photo
            Image(2, Full, Wide),                                     // a picture that is no score
            Image(0, SlideWidth, SlideHeight));                       // no content picture
        var deck = CaptionDeck.Read(path);
        var score = CaptionConsole.ScorePicture(deck.Path, deck.Slides[0]);
        Assert.NotNull(score);
        Assert.Equal((600, 450), (score.Value.Image.PixelWidth, score.Value.Image.PixelHeight)); // the picture, not the slide
        Assert.Equal(2, score.Value.Page.Rows.Length);
        Assert.Null(CaptionConsole.ScorePicture(deck.Path, deck.Slides[1])); // the render path is used
        Assert.Null(CaptionConsole.ScorePicture(deck.Path, deck.Slides[2]));
        var (slides, _) = CaptionConsole.PrepareScore(deck.Path, deck.Slides, CancellationToken.None);
        Assert.Equal([1], slides.Keys);
        return 0;
    }));

    [Fact]
    public void ACorruptPictureSendsItsSlideToTheRenderWithoutAnException() => StaHelper.RunOnSta(() => InFolder(folder =>
    {
        var path = WriteDeck(folder, [ScorePng(), ScorePng()], Image(0, Full, Wide), Image(1, Full, Wide));
        // Break image0's deflate data (as a damaged file does): reading it throws InvalidDataException inside the zip reader.
        var bytes = File.ReadAllBytes(path);
        var name = Encoding.ASCII.GetBytes("ppt/media/image0.png");
        int at = Enumerable.Range(0, bytes.Length - name.Length).First(i => bytes.AsSpan(i, name.Length).SequenceEqual(name)); // the local header
        int data = at + name.Length + BitConverter.ToUInt16(bytes, at - 2);
        for (int i = data; i < data + 200; i++) bytes[i] = 0xFF; // first byte: a reserved deflate block type
        File.WriteAllBytes(path, bytes);
        var deck = CaptionDeck.Read(path);
        var failures = new List<(int Slide, Exception Error)>();
        var (slides, target) = CaptionConsole.PrepareScore(deck.Path, deck.Slides, CancellationToken.None, (n, error) => failures.Add((n, error)));
        Assert.Equal([2], slides.Keys);              // slide 1 from the render, slide 2 from its picture
        Assert.Equal(1, Assert.Single(failures).Slide);
        Assert.True(target.Down > 0);
        Assert.Null(CaptionConsole.ScorePicture(deck.Path, deck.Slides[0]));
        return 0;
    }));

    private static BitmapSource Staves(int spacing, params (int Top, int Above)[] staves) => Staves(spacing, null, staves);

    // Staves (top, how high a note rises above it) with lyrics under each, black on white; extra paints more ink (x, y, w, h).
    private static BitmapSource Staves(int spacing, Action<Action<int, int, int, int>>? extra, params (int Top, int Above)[] staves)
    {
        const int width = 1200, height = 900;
        var pixels = new byte[width * height];
        Array.Fill(pixels, (byte)255);
        void Fill(int x, int y, int w, int h) { for (int r = Math.Max(0, y); r < y + h; r++) Array.Fill(pixels, (byte)0, r * width + x, w); }
        foreach (var (top, above) in staves)
        {
            for (int line = 0; line < 5; line++) Fill(100, top + line * spacing, 1000, 2);
            if (above > 0) Fill(300, top - above, 14, above);
            Fill(150, top + 6 * spacing + spacing / 2, 800, 2 * spacing); // lyrics under the staff
        }
        extra?.Invoke(Fill);
        var image = BitmapSource.Create(width, height, 96, 96, PixelFormats.Gray8, null, pixels, width);
        image.Freeze();
        return image;
    }

    private static RowExtent Target(params BitmapSource[] slides) => ScoreLayout.Target(slides.Select(ScoreLayout.Measure));

    private static int Height(ScoreRow row) => row.PadTop + row.Area.Height + row.PadBottom;

    [Fact]
    public void EveryRowOfADeckIsCutTheSameHeightForItsStaffSpacing() => StaHelper.RunOnSta(() =>
    {
        var high = Staves(12, (150, 36), (500, 36)); // notes three spacings above the staves
        var plain = Staves(18, (150, 0), (500, 0));  // larger staves, nothing above them
        var small = Staves(12, (150, 0), (500, 0));
        var target = Target(high, plain, small);    // six rows: the median, so the two high rows do not set it
        Assert.Equal(0, target.Up, 2);
        var slides = new[] { (Image: plain, Spacing: 18.0), (Image: small, Spacing: 12.0) };
        var ratios = slides.SelectMany(s => ScoreLayout.FindRows(s.Image, target).Select(r => Height(r) / s.Spacing)).ToArray();
        Assert.Equal(4, ratios.Length);
        Assert.All(ratios, ratio => Assert.InRange(ratio, ratios[0] * 0.95, ratios[0] * 1.05));
        Assert.All(ScoreLayout.FindRows(high, target), row => Assert.True(row.Area.Y <= row.StaffTop - 36)); // the high notes are in
        // The rows the console sends are those.
        var sent = slides.Select(s => CaptionConsole.CropRow(s.Image, 1, target)!.Value.Image.PixelHeight / s.Spacing).ToArray();
        Assert.InRange(sent[1], sent[0] * 0.95, sent[0] * 1.05);
        // A larger target still stops at the cut between the two rows; white makes up the rest.
        var rows = ScoreLayout.FindRows(plain, new RowExtent(30, 30));
        Assert.True(rows[0].Area.Y + rows[0].Area.Height <= rows[1].Area.Y);
        Assert.True(rows[0].PadBottom > 0 && rows[1].PadTop > 0);
        Assert.Empty(ScoreLayout.Measure(BitmapSource.Create(64, 64, 96, 96, PixelFormats.Gray8, null, new byte[64 * 64], 64)));
    });

    [Theory]
    [InlineData(4, false)] // 10 rows
    [InlineData(3, true)]  // 9 rows: the median
    public void AClefOnlyOnTheFirstRowIsNotCutOff(int plainSlides, bool oneRowSlide) => StaHelper.RunOnSta(() =>
    {
        // Slides with notes one spacing above every staff; on one, the first row's clef rises 3.5 spacings.
        var plain = Enumerable.Range(0, plainSlides).Select(_ => Staves(12, (150, 12), (500, 12)))
            .Concat(oneRowSlide ? [Staves(12, (150, 12))] : []).ToArray();
        var clef = Staves(12, (150, 42), (500, 12));
        var target = Target([.. plain, clef]);
        Assert.InRange(target.Up, 0.9, 1.1); // one row's clef does not set the deck's size
        var rows = ScoreLayout.FindRows(clef, target);
        Assert.True(rows[0].Area.Y <= 150 - 42, $"clef cut off: row starts at {rows[0].Area.Y}"); // its own ink stays in
        var image = CaptionConsole.CropRow(clef, 0, target)!.Value.Image;
        Assert.True(image.PixelHeight >= CaptionConsole.CropRow(plain[0], 0, target)!.Value.Image.PixelHeight);
        // Rows within the target are one height.
        Assert.Equal(Height(ScoreLayout.FindRows(plain[0], target)[0]), Height(rows[1]));
    });

    [Fact]
    public void ATitleTouchingTheFirstRowIsLeftOutOfItAndOfTheDecksSize() => StaHelper.RunOnSta(() =>
    {
        // Nine rows; on the first slide a title runs down to just above the first row's note (no clear gap between).
        var titled = Staves(12, fill => fill(200, 70, 400, 61), (150, 12), (500, 12));
        var others = Enumerable.Range(0, 3).Select(_ => Staves(12, (150, 12), (500, 12))).Append(Staves(12, (150, 12))).ToArray();
        Assert.InRange(ScoreLayout.Measure(titled)[0].Up, 0.9, 1.1); // the title is not joined to the staff: not the row's reach
        var target = Target([titled, .. others]);
        Assert.InRange(target.Up, 0.9, 1.1); // the title does not set the deck's size
        var rows = ScoreLayout.FindRows(titled, target);
        Assert.True(rows[0].Area.Y >= 131, $"title in the row: it starts at {rows[0].Area.Y}"); // the title ends at 130
        Assert.Equal(Height(ScoreLayout.FindRows(others[0], target)[0]), Height(rows[0]));
    });

    [Fact]
    public void ArtDrawnUnderTheLastRowIsLeftOutAndPadsNoOtherRow() => StaHelper.RunOnSta(() =>
    {
        // An opaque picture: a photo drawn right under the last row's lyrics (they end at 552).
        var photo = Staves(12, fill => fill(100, 564, 1000, 250), (150, 12), (450, 12));
        var plain = Enumerable.Range(0, 2).Select(_ => Staves(12, (150, 12), (450, 12))).ToArray();
        Assert.InRange(ScoreLayout.Measure(photo)[1].Down, 4.4, 4.6); // its lyrics, not the photo
        var target = Target([photo, .. plain]);
        Assert.InRange(target.Down, 4.4, 4.6);
        var rows = ScoreLayout.FindRows(photo, target);
        Assert.True(rows[1].Area.Y + rows[1].Area.Height <= 564, $"the photo is in the row: it ends at {rows[1].Area.Y + rows[1].Area.Height}");
        // No row gets more white than the little room around its own ink (half a spacing on a side).
        var all = plain.Append(photo).SelectMany(s => ScoreLayout.FindRows(s, target)).ToArray();
        Assert.All(all, row => Assert.True(row.PadTop <= 6 && row.PadBottom <= 6, $"padded {row.PadTop}+{row.PadBottom}"));
        Assert.All(all, row => Assert.Equal(Height(all[0]), Height(row)));
    });

    [Fact]
    public void DarkBandsOfAPhotoAreNoStaff() => StaHelper.RunOnSta(() =>
    {
        // Under the score, a photo whose dark bands line up like five staff lines, but with mostly ink between them.
        var image = Staves(12, fill =>
        {
            for (int y = 600; y < 650; y++)
                if ((y - 600) % 12 < 2) fill(100, y, 1000, 1);                // the bands
                else for (int x = 100; x < 1100; x += 9) fill(x, y, 5, 1);  // the photo between them
        }, (150, 12));
        Assert.Single(ScoreLayout.Layout(image).Rows); // (a content picture; a slide render finds staves as it always did)
    });

    // Five staff lines, 12 apart, at top.
    private static void Staff(Action<int, int, int, int> fill, int top)
    {
        for (int line = 0; line < 5; line++) fill(100, top + line * 12, 1000, 2);
    }

    private static ScoreRow[] PictureRows(BitmapSource image) => ScoreLayout.FindRows(image, Target(image));

    private static int End(ScoreRow row) => row.Area.Y + row.Area.Height;

    [Fact]
    public void ALyricNearerTheNextStaffStaysWholeInItsOwnRow() => StaHelper.RunOnSta(() =>
    {
        var image = Staves(12, fill =>
        {
            Staff(fill, 150);
            fill(150, 330, 800, 24);  // row 1's lyrics (to 354), nearer row 2's staff (198 .. 354 .. 400)
            Staff(fill, 400);
            fill(300, 376, 14, 24);   // a note above row 2's staff
            fill(150, 478, 800, 24);  // row 2's first verse
            fill(150, 512, 800, 24);  // and second verse (to 536)
        });
        var rows = PictureRows(image);
        Assert.Equal(2, rows.Length);
        Assert.True(End(rows[0]) >= 354, $"row 1's lyrics cut: it ends at {End(rows[0])}");
        Assert.InRange(rows[1].Area.Y, 355, 376); // nothing of row 1's lyrics, all of its own note
        Assert.True(End(rows[1]) >= 536, $"the second verse is not in: row 2 ends at {End(rows[1])}");
    });

    // The dark pixels of a picture.
    private static bool[] Mask(BitmapSource image)
    {
        var gray = new FormatConvertedBitmap(image, PixelFormats.Gray8, null, 0);
        var pixels = new byte[gray.PixelWidth * gray.PixelHeight];
        gray.CopyPixels(pixels, gray.PixelWidth, 0);
        return [.. pixels.Select(p => p < 160)];
    }

    // The share of a part's pixels (a mask over the picture) that the k-th row as sent shows dark.
    private static double Shows(BitmapSource picture, ScoreRow[] rows, int k, RowExtent target, bool[] part)
    {
        var row = rows[k];
        var sent = new FormatConvertedBitmap(CaptionConsole.CropRow(picture, k, target)!.Value.Image, PixelFormats.Gray8, null, 0);
        var shown = new byte[sent.PixelWidth * sent.PixelHeight];
        sent.CopyPixels(shown, sent.PixelWidth, 0);
        int width = picture.PixelWidth, all = 0, dark = 0;
        for (int at = 0; at < part.Length; at++)
        {
            if (!part[at]) continue;
            all++;
            int x = at % width - row.Area.X, y = at / width - row.Area.Y;
            if (x >= 0 && x < row.Area.Width && y >= 0 && y < row.Area.Height && shown[(y + row.PadTop) * sent.PixelWidth + x] < 180) dark++;
        }
        return all == 0 ? 0 : dark / (double)all;
    }

    // A content picture drawn like the decks: staves and notes, and text in a Korean font whose letters thin out at the
    // bottom. Each part can be drawn alone too, as a mask of where it is.
    private sealed class Page
    {
        public const int Width = 1200, Height = 900;
        private readonly List<Action<DrawingContext>> _parts = [];

        public bool[] Add(Action<DrawingContext> part) { _parts.Add(part); return Mask(Draw(part)); }

        public BitmapSource Picture() => Draw([.. _parts]);

        public static BitmapSource Draw(params Action<DrawingContext>[] parts)
        {
            var visual = new DrawingVisual();
            using (var context = visual.RenderOpen())
            {
                context.DrawRectangle(Brushes.White, null, new System.Windows.Rect(0, 0, Width, Height));
                foreach (var part in parts) part(context);
            }
            var bitmap = new RenderTargetBitmap(Width, Height, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(visual);
            bitmap.Freeze();
            return bitmap;
        }

        public static Action<DrawingContext> Box(double x, double y, double w, double h) =>
            context => context.DrawRectangle(Brushes.Black, null, new System.Windows.Rect(x, y, w, h));

        public static Action<DrawingContext> Staff(int top, int spacing = 12) =>
            context => { for (int line = 0; line < 5; line++) context.DrawRectangle(Brushes.Black, null, new System.Windows.Rect(100, top + line * spacing, 1000, 2)); };

        // Text whose lowest dark pixel row is bottom (its shadow, when given, drawn grey that far down and right; a serif
        // font for a tuplet's number or a chord, as the scores print them).
        public static Action<DrawingContext> Text(string text, double x, int bottom, double size, bool italic = false, int shadow = 0, bool bold = false,
            bool serif = false, Brush? ink = null, Brush? shade = null)
        {
            FormattedText Format(Brush brush) => new(text, System.Globalization.CultureInfo.GetCultureInfo("ko-KR"),
                System.Windows.FlowDirection.LeftToRight, new Typeface(new FontFamily(serif ? "Times New Roman" : "Malgun Gothic"), italic ? System.Windows.FontStyles.Italic : System.Windows.FontStyles.Normal,
                bold ? System.Windows.FontWeights.Bold : System.Windows.FontWeights.Normal, System.Windows.FontStretches.Normal), size, brush, 1.0);
            var mask = Mask(Draw(context => context.DrawText(Format(Brushes.Black), new System.Windows.Point(x, 0))));
            int low = Array.FindLastIndex(mask, p => p) / Width;
            return context =>
            {
                if (shadow > 0) context.DrawText(Format(shade ?? Brushes.Gray), new System.Windows.Point(x + shadow, bottom - low + shadow));
                context.DrawText(Format(ink ?? Brushes.Black), new System.Windows.Point(x, bottom - low));
            };
        }

        // A tuplet's bracket (a line with its ends bent down) from left to right at top, its serif italic 3 in the middle.
        public static Action<DrawingContext> Tuplet(int left, int right, int top, Brush? ink = null) => context =>
        {
            var brush = ink ?? Brushes.Black;
            int middle = (left + right) / 2;
            context.DrawRectangle(brush, null, new System.Windows.Rect(left, top, middle - 7 - left, 1.5));
            context.DrawRectangle(brush, null, new System.Windows.Rect(middle + 7, top, right - middle - 7, 1.5));
            context.DrawRectangle(brush, null, new System.Windows.Rect(left, top, 1.5, 8));
            context.DrawRectangle(brush, null, new System.Windows.Rect(right - 1.5, top, 1.5, 8));
            Text("3", middle - 5, top + 7, 17, italic: true, bold: true, serif: true, ink: brush)(context);
        };

        // A dark pixel on a part's lowest dark row.
        public static (int X, int Y) Lowest(bool[] part)
        {
            int y = Array.FindLastIndex(part, p => p) / Width;
            return (Enumerable.Range(0, Width).First(x => part[y * Width + x]), y);
        }
    }

    [Fact]
    public void LyricsWhoseLettersTouchTheNextStemStayWholeInTheirRow() => StaHelper.RunOnSta(() =>
    {
        var page = new Page();
        page.Add(Page.Staff(150));
        page.Add(Page.Staff(400));
        var lyrics = page.Add(Page.Text("주님의 크신 사랑 영원히 노래합니다", 160, 330, 34));
        var (x, y) = Page.Lowest(lyrics);
        // A note of the next row whose stem rises into a letter's lowest stroke, and a beam right under the letters.
        var music = page.Add(context =>
        {
            Page.Box(x, y, 2, 400 - y)(context);
            Page.Box(x, y + 4, 90, 5)(context);
            Page.Box(x + 88, y + 4, 2, 400 - y - 4)(context);
            Page.Box(x - 10, 396, 12, 9)(context);
        });
        page.Add(Page.Text("두 번째 줄 가사", 160, 480, 34));
        var picture = page.Picture();
        var target = Target(picture);
        var rows = ScoreLayout.FindRows(picture, target);
        Assert.Equal(2, rows.Length);
        Assert.True(Shows(picture, rows, 0, target, lyrics) >= 0.98, $"row 1 shows {Shows(picture, rows, 0, target, lyrics):P1} of its lyrics");
        Assert.True(Shows(picture, rows, 1, target, lyrics) <= 0.01, $"row 2 shows {Shows(picture, rows, 1, target, lyrics):P1} of row 1's lyrics");
        Assert.True(Shows(picture, rows, 1, target, music) >= 0.97, $"row 2 shows {Shows(picture, rows, 1, target, music):P1} of its stems and beam");
        Assert.True(Shows(picture, rows, 0, target, music) <= 0.03, "row 1 shows row 2's stems");
    });

    [Fact]
    public void ATitleNearTheStaffStaysOutAndTheKeySignatureBesideItIsWhole() => StaHelper.RunOnSta(() =>
    {
        var page = new Page();
        page.Add(Page.Staff(150));
        // A clef rising 2.5 spacings and two sharps crossing the top lines; the title right beside them, ending 6 pixels
        // over the top line without touching anything.
        var key = page.Add(context =>
        {
            Page.Box(108, 120, 12, 92)(context);
            Page.Box(135, 132, 3, 30)(context);
            Page.Box(145, 128, 3, 30)(context);
            Page.Box(132, 140, 18, 3)(context);
        });
        var title = page.Add(Page.Text("주 은혜 찬양 (새찬송가 301장)", 170, 143, 28));
        var three = page.Add(Page.Text("3", 900, 146, 12)); // a tuplet's number on the title's rows, far beside it
        page.Add(Page.Text("가사 첫 줄", 160, 250, 28));
        var picture = page.Picture();
        var plain = Page.Draw(Page.Staff(150), Page.Text("가사 첫 줄", 160, 250, 28));
        var target = Target(picture, plain, plain);
        Assert.InRange(target.Up, 0, 1); // the deck's size is not raised by the title
        var rows = ScoreLayout.FindRows(picture, target);
        Assert.True(Shows(picture, rows, 0, target, title) <= 0.01, $"the row shows {Shows(picture, rows, 0, target, title):P1} of the title");
        Assert.True(Shows(picture, rows, 0, target, key) >= 0.99, $"the row shows {Shows(picture, rows, 0, target, key):P1} of the clef and key");
        Assert.True(Shows(picture, rows, 0, target, three) >= 0.98, "the tuplet's number is left out with the title");
    });

    [Fact]
    public void ASlideNumberOverAFermataStaysOut() => StaHelper.RunOnSta(() =>
    {
        var page = new Page();
        page.Add(Page.Staff(150));
        var fermata = page.Add(context => { Page.Box(400, 120, 2, 30)(context); Page.Box(392, 110, 16, 4)(context); }); // a stem, a fermata 6 rows over it
        var number = page.Add(Page.Text("1/8", 380, 106, 28)); // 3 blank rows over the fermata, 2.9 spacings up to its top
        page.Add(Page.Text("가사 첫 줄", 160, 250, 28));
        var picture = page.Picture();
        var target = Target(picture);
        var rows = ScoreLayout.FindRows(picture, target);
        Assert.True(Shows(picture, rows, 0, target, number) <= 0.01, $"the row shows {Shows(picture, rows, 0, target, number):P1} of the slide number");
        Assert.True(Shows(picture, rows, 0, target, fermata) >= 0.99, "the fermata is cut");
    });

    [Fact]
    public void AChordLineOverTheSecondStaffGoesWithIt() => StaHelper.RunOnSta(() =>
    {
        var page = new Page();
        page.Add(Page.Staff(150));
        page.Add(Page.Text("가사 첫 줄", 160, 250, 28));
        page.Add(Page.Staff(400));
        page.Add(context => { Page.Box(600, 380, 2, 24)(context); Page.Box(590, 398, 12, 9)(context); }); // a note whose stem rises to 380
        var chords = page.Add(Page.Text("C      G7      Am      F      C/E      Dm7      G      C", 150, 376, 16)); // 3 blank rows over the stem
        page.Add(Page.Text("가사 둘째 줄", 160, 500, 28));
        var picture = page.Picture();
        var target = Target(picture);
        var rows = ScoreLayout.FindRows(picture, target);
        Assert.True(Shows(picture, rows, 1, target, chords) >= 0.98, $"row 2 shows {Shows(picture, rows, 1, target, chords):P1} of its chords");
        Assert.True(Shows(picture, rows, 0, target, chords) <= 0.01, "row 1 shows row 2's chords");
    });

    [Fact]
    public void ATripletsThreeOverTheSecondStaffGoesWithIt() => StaHelper.RunOnSta(() =>
    {
        var page = new Page();
        page.Add(Page.Staff(150));
        page.Add(Page.Text("가사 첫 줄", 160, 250, 28));
        page.Add(Page.Staff(400));
        page.Add(context =>
        {
            foreach (var x in new[] { 500, 540, 580 }) { Page.Box(x, 375, 2, 30)(context); Page.Box(x - 10, 400, 12, 9)(context); }
            Page.Box(500, 375, 82, 5)(context);
        });
        var three = page.Add(Page.Text("3", 536, 366, 12)); // 8 blank rows over the beam
        page.Add(Page.Text("가사 둘째 줄", 160, 500, 28));
        var picture = page.Picture();
        var target = Target(picture);
        var rows = ScoreLayout.FindRows(picture, target);
        Assert.True(Shows(picture, rows, 1, target, three) >= 0.98, $"row 2 shows {Shows(picture, rows, 1, target, three):P1} of its 3");
        Assert.True(Shows(picture, rows, 0, target, three) <= 0.01, "row 1 shows row 2's 3");
    });

    [Fact]
    public void ATripletsThreeInItsBracketUnderShadowedLyricsGoesWithTheBracket() => StaHelper.RunOnSta(() =>
    {
        var page = new Page();
        page.Add(Page.Staff(150));
        // Lyrics with a grey shadow 3 pixels down and right, ending at 333; the next row's tuplet bracket at 341 just under
        // them, its italic 3 between the bracket's halves starting right under the shadow.
        page.Add(Page.Text("갈멜산에서 쌓은 번제물을", 160, 330, 34, shadow: 3));
        page.Add(Page.Staff(400));
        page.Add(context =>
        {
            foreach (var x in new[] { 500, 540, 580 }) { Page.Box(x, x == 540 ? 356 : 341, 2, x == 540 ? 49 : 64)(context); Page.Box(x - 10, 400, 12, 9)(context); }
            Page.Box(500, 341, 30, 2)(context);
            Page.Box(553, 341, 29, 2)(context);
        });
        var three = page.Add(Page.Text("3", 534, 344, 13, italic: true)); // one blank row under the shadow
        page.Add(Page.Text("가사 둘째 줄", 160, 500, 28));
        var picture = page.Picture();
        var target = Target(picture);
        var rows = ScoreLayout.FindRows(picture, target);
        Assert.True(Shows(picture, rows, 1, target, three) >= 0.98, $"row 2 shows {Shows(picture, rows, 1, target, three):P1} of its 3");
        Assert.True(Shows(picture, rows, 0, target, three) <= 0.01, $"row 1 shows {Shows(picture, rows, 0, target, three):P1} of row 2's 3");
    });

    [Fact]
    public void WhatIsPaintedOverTakesTheBackgroundOfATintedPicture() => StaHelper.RunOnSta(() =>
    {
        var page = new Page();
        var tint = Color.FromRgb(200, 220, 240);
        page.Add(context => context.DrawRectangle(new SolidColorBrush(tint), null, new System.Windows.Rect(0, 0, Page.Width, Page.Height)));
        page.Add(Page.Staff(150));
        page.Add(context => Page.Box(108, 120, 12, 92)(context)); // a clef rising 2.5 spacings, beside the title
        var title = page.Add(Page.Text("주 은혜 찬양 (새찬송가 301장)", 170, 143, 28));
        page.Add(Page.Text("가사 첫 줄", 160, 250, 28));
        var picture = page.Picture();
        var target = Target(picture);
        var row = ScoreLayout.FindRows(picture, target)[0];
        Assert.True(row.Area.Y < 143, "the title's rows are not in the row: nothing to paint over");
        var sent = new FormatConvertedBitmap(CaptionConsole.CropRow(picture, 0, target)!.Value.Image, PixelFormats.Bgr32, null, 0);
        var pixels = new byte[sent.PixelWidth * sent.PixelHeight * 4];
        sent.CopyPixels(pixels, sent.PixelWidth * 4, 0);
        int checkedPixels = 0;
        for (int at = 0; at < title.Length; at++)
        {
            int x = at % Page.Width - row.Area.X, y = at / Page.Width - row.Area.Y;
            if (!title[at] || x < 0 || x >= row.Area.Width || y < 0 || y >= row.Area.Height) continue;
            int i = ((y + row.PadTop) * sent.PixelWidth + x) * 4;
            checkedPixels++;
            Assert.InRange(pixels[i], tint.B - 12, tint.B + 12);
            Assert.InRange(pixels[i + 1], tint.G - 12, tint.G + 12);
            Assert.InRange(pixels[i + 2], tint.R - 12, tint.R + 12);
        }
        Assert.True(checkedPixels > 100);
    });


    // Notes with their stems up (head at y, stem rising to top) at each x.
    private static Action<DrawingContext> Notes(int y, int top, params int[] xs) => context =>
    {
        foreach (var x in xs) { Page.Box(x - 10, y, 12, 9)(context); Page.Box(x, top, 2, y + 4 - top)(context); }
    };

    // The dark pixels of the part shown in the row (share).
    private static string Report(BitmapSource picture, ScoreRow[] rows, RowExtent target, bool[] part) =>
        string.Join(" ", Enumerable.Range(0, rows.Length).Select(k => $"row {k + 1} {Shows(picture, rows, k, target, part):P1}"));

    [Fact]
    public void AStaffWhoseLinesBreakForAFewPixelsKeepsItsClefMeasuresAndLyrics() => StaHelper.RunOnSta(() =>
    {
        var page = new Page();
        // Row 1's five lines break for 5 pixels at 400 (as at a bar line of a scan); the longer piece is on the right.
        page.Add(context => { for (int line = 0; line < 5; line++) { Page.Box(100, 150 + line * 12, 300, 2)(context); Page.Box(405, 150 + line * 12, 695, 2)(context); } });
        var left = page.Add(context =>
        {
            Page.Box(108, 128, 12, 84)(context); // a clef
            Page.Box(135, 140, 3, 30)(context);  // a key signature
            Page.Box(145, 136, 3, 30)(context);
            Notes(162, 130, 200, 260, 320, 380)(context);
        });
        page.Add(Notes(162, 130, 470, 570, 670, 770, 870, 970));
        var lyrics = page.Add(Page.Text("영화롭고도 밝은 천국 올라가서", 160, 250, 34, shadow: 3));
        page.Add(Page.Staff(400));
        page.Add(Page.Text("둘째 줄 가사입니다", 160, 500, 28));
        var picture = page.Picture();
        var target = Target(picture);
        var rows = ScoreLayout.FindRows(picture, target);
        Assert.Equal(2, rows.Length);
        Assert.True(Shows(picture, rows, 0, target, left) >= 0.99, "the clef, key and notes left of the break: " + Report(picture, rows, target, left));
        Assert.True(Shows(picture, rows, 0, target, lyrics) >= 0.98, "the lyrics: " + Report(picture, rows, target, lyrics));
    });

    [Fact]
    public void ATupletsThreeAndAFermataJustUnderShadowedLyricsGoWithTheNextRow() => StaHelper.RunOnSta(() =>
    {
        var page = new Page();
        page.Add(Page.Staff(150));
        // Row 1's lyrics with a grey shadow and a large verse number reaching 8 pixels lower (its shadow to 311).
        var lyrics = page.Add(Page.Text("내일일은 난몰라요 하루하루 살아요", 200, 300, 34, shadow: 3));
        var verse = page.Add(Page.Text("1", 150, 308, 48, shadow: 3));
        page.Add(Page.Staff(400));
        // Row 2: three beamed eighths under a tuplet's italic 3, and a note under a fermata; both marks start 2 rows under the shadow.
        page.Add(context =>
        {
            Notes(405, 360, 500, 540, 580)(context);
            Page.Box(500, 360, 82, 5)(context);
            Page.Box(700, 400, 12, 9)(context);
            Page.Box(700, 404, 2, 40)(context);
        });
        var three = page.Add(Page.Text("3", 535, 326, 16, italic: true));
        var fermata = page.Add(context =>
        {
            var arc = new StreamGeometry();
            using (var draw = arc.Open())
            {
                draw.BeginFigure(new System.Windows.Point(694, 326), true, true);
                draw.BezierTo(new System.Windows.Point(696, 310), new System.Windows.Point(716, 310), new System.Windows.Point(718, 326), true, false);
                draw.BezierTo(new System.Windows.Point(714, 316), new System.Windows.Point(698, 316), new System.Windows.Point(694, 326), true, false);
            }
            context.DrawGeometry(Brushes.Black, null, arc);
            context.DrawEllipse(Brushes.Black, null, new System.Windows.Point(706, 324), 2.5, 2.5);
        });
        page.Add(Page.Text("둘째 줄 가사입니다", 160, 500, 28));
        var picture = page.Picture();
        var target = Target(picture);
        var rows = ScoreLayout.FindRows(picture, target);
        Assert.Equal(2, rows.Length);
        Assert.True(Shows(picture, rows, 1, target, three) >= 0.98 && Shows(picture, rows, 0, target, three) <= 0.01, "the 3: " + Report(picture, rows, target, three));
        Assert.True(Shows(picture, rows, 1, target, fermata) >= 0.98 && Shows(picture, rows, 0, target, fermata) <= 0.01, "the fermata: " + Report(picture, rows, target, fermata));
        Assert.True(Shows(picture, rows, 0, target, lyrics) >= 0.98 && Shows(picture, rows, 1, target, lyrics) <= 0.01, "the lyrics: " + Report(picture, rows, target, lyrics));
        Assert.True(Shows(picture, rows, 0, target, verse) >= 0.98 && Shows(picture, rows, 1, target, verse) <= 0.01, "the verse number: " + Report(picture, rows, target, verse));
    });

    [Fact]
    public void ATupletUnderTheTitleIsInTheFirstRowAndTheTitleIsNot() => StaHelper.RunOnSta(() =>
    {
        var page = new Page();
        page.Add(Page.Staff(150));
        var title = page.Add(Page.Text("452장 내 모든 소원 기도의 제목", 200, 100, 30, shadow: 3));
        page.Add(context => { Notes(165, 125, 400, 440, 480)(context); Page.Box(400, 125, 82, 5)(context); });
        // The tuplet's bracket and 3 between the beam and the title, the 3 one blank row under the title's shadow.
        var tuplet = page.Add(context =>
        {
            Page.Box(398, 110, 30, 2)(context); Page.Box(398, 110, 2, 8)(context);
            Page.Box(454, 110, 30, 2)(context); Page.Box(482, 110, 2, 8)(context);
        });
        var three = page.Add(Page.Text("3", 434, 114, 14, italic: true));
        page.Add(Page.Text("가사 첫 줄 가사입니다", 160, 250, 28));
        var picture = page.Picture();
        var target = Target(picture);
        var rows = ScoreLayout.FindRows(picture, target);
        Assert.True(Shows(picture, rows, 0, target, tuplet) >= 0.98, "the bracket: " + Report(picture, rows, target, tuplet));
        Assert.True(Shows(picture, rows, 0, target, three) >= 0.98, "the 3: " + Report(picture, rows, target, three));
        Assert.True(Shows(picture, rows, 0, target, title) <= 0.01, "the title: " + Report(picture, rows, target, title));
    });

    // Row 1 (staff at 150, notes, shadowed lyrics ending at 300) and row 2 (staff at 400, its lyrics at 530); between them
    // what a test adds.
    private static Page TwoRows(out bool[] lyrics, string? upper = null)
    {
        var page = new Page();
        page.Add(Page.Staff(150));
        page.Add(Notes(176, 140, 200, 300, 400, 500, 600, 700, 800, 900, 1000));
        lyrics = page.Add(Page.Text(upper ?? "갈멜산에서 쌓은 번제물을 받으신 주", 160, 300, 34, shadow: 3));
        page.Add(Page.Staff(400));
        page.Add(Page.Text("두 번째 줄 가사입니다", 160, 530, 34, shadow: 3));
        return page;
    }

    [Fact]
    public void ALineOfTupletsJustUnderTheLyricsGoesWithTheNextRow() => StaHelper.RunOnSta(() =>
    {
        var page = TwoRows(out var lyrics);
        // Row 2's triplets: beamed eighths with stems up to 326, a bracket and a bold serif 3 over each just under the shadow.
        page.Add(context =>
        {
            foreach (int left in new[] { 250, 340, 430, 520 }) { Notes(405, 326, left, left + 40, left + 80)(context); Page.Box(left, 326, 82, 5)(context); }
        });
        var tuplets = page.Add(context => { foreach (int left in new[] { 250, 340, 430, 520 }) Page.Tuplet(left, left + 82, 308)(context); });
        var picture = page.Picture();
        var target = Target(picture);
        var rows = ScoreLayout.FindRows(picture, target);
        Assert.True(Shows(picture, rows, 1, target, tuplets) >= 0.98 && Shows(picture, rows, 0, target, tuplets) <= 0.01, "the tuplets: " + Report(picture, rows, target, tuplets));
        Assert.True(Shows(picture, rows, 0, target, lyrics) >= 0.98 && Shows(picture, rows, 1, target, lyrics) <= 0.01, "the lyrics: " + Report(picture, rows, target, lyrics));
    });

    [Fact]
    public void ALineOfChordsJustUnderTheLyricsGoesWithTheNextRow() => StaHelper.RunOnSta(() =>
    {
        var page = TwoRows(out var lyrics);
        page.Add(Notes(405, 345, 200, 400, 600, 800, 1000));
        // Row 2's chords in a serif font, 6 rows under the shadow, a sharp and a slash among them.
        var chords = page.Add(Page.Text("C           G7           Am           F♯m           D/F♯           G", 190, 326, 18, serif: true));
        var picture = page.Picture();
        var target = Target(picture);
        var rows = ScoreLayout.FindRows(picture, target);
        Assert.True(Shows(picture, rows, 1, target, chords) >= 0.98 && Shows(picture, rows, 0, target, chords) <= 0.01, "the chords: " + Report(picture, rows, target, chords));
        Assert.True(Shows(picture, rows, 0, target, lyrics) >= 0.98 && Shows(picture, rows, 1, target, lyrics) <= 0.01, "the lyrics: " + Report(picture, rows, target, lyrics));
    });

    [Fact]
    public void TupletsAndChordsBesideAndUnderTheTitleAreTheFirstRowsAndTheTitleIsNot() => StaHelper.RunOnSta(() =>
    {
        var page = new Page();
        page.Add(Page.Staff(150, 10));
        page.Add(context => { Notes(172, 118, 300, 340, 380)(context); Page.Box(300, 118, 82, 5)(context); Notes(176, 128, 700, 800, 900, 1000)(context); });
        // The title, a tuplet under its letters and a chord line beside it with a sharp and a slash (taller than 2 spacings).
        var title = page.Add(Page.Text("452장 내 모든 소원", 160, 90, 30, shadow: 3));
        var tuplet = page.Add(Page.Tuplet(300, 382, 100));
        var chords = page.Add(Page.Text("C        F♯m        D/F♯        G", 640, 112, 22, serif: true));
        page.Add(Page.Text("가사 첫 줄 가사입니다", 160, 260, 34, shadow: 3));
        var picture = page.Picture();
        var target = Target(picture);
        var rows = ScoreLayout.FindRows(picture, target);
        Assert.True(Shows(picture, rows, 0, target, tuplet) >= 0.98, "the tuplet: " + Report(picture, rows, target, tuplet));
        Assert.True(Shows(picture, rows, 0, target, chords) >= 0.98, "the chords: " + Report(picture, rows, target, chords));
        Assert.True(Shows(picture, rows, 0, target, title) <= 0.01, "the title: " + Report(picture, rows, target, title));
    });

    [Theory]
    [InlineData("주")]                              // one syllable
    [InlineData("2. 오")]                           // a verse number and one syllable
    [InlineData("주              님              은")] // syllables far apart
    [InlineData("성령의- 불로- 성령의- 불로-")]       // final consonants under the letters beside dashes
    public void AShortSecondVerseLineStaysWithItsRow(string verse) => StaHelper.RunOnSta(() =>
    {
        var page = TwoRows(out var lyrics, "주님의 크신 사랑 영원히 노래해");
        var second = page.Add(Page.Text(verse, 200, 346, 34, shadow: 3));
        page.Add(Notes(405, 372, 200, 300, 400, 500, 600, 700, 800));
        var picture = page.Picture();
        var target = Target(picture);
        var rows = ScoreLayout.FindRows(picture, target);
        Assert.True(Shows(picture, rows, 0, target, second) >= 0.98 && Shows(picture, rows, 1, target, second) <= 0.01, $"\"{verse}\": " + Report(picture, rows, target, second));
        Assert.True(Shows(picture, rows, 0, target, lyrics) >= 0.98, "the first verse: " + Report(picture, rows, target, lyrics));
    });

    [Fact]
    public void APaleBracketTouchingTheLyricsShadowGoesWithItsTuplet() => StaHelper.RunOnSta(() =>
    {
        var page = new Page();
        page.Add(Page.Staff(150));
        page.Add(Notes(176, 140, 200, 300, 400, 500, 600, 700, 800, 900, 1000));
        var pale = new SolidColorBrush(Color.FromRgb(175, 175, 175));
        var lyrics = page.Add(Page.Text("주님의 크신 사랑 영원히 노래해", 160, 300, 34, shadow: 3, shade: pale));
        page.Add(Page.Staff(400));
        page.Add(context => { Notes(405, 340, 500, 540, 580)(context); Page.Box(500, 340, 82, 5)(context); });
        // Row 2's bracket: dark arms and a thin grey line between them, its right half running along the lyrics' pale shadow.
        var grey = new SolidColorBrush(Color.FromRgb(120, 120, 120));
        var bracket = page.Add(context =>
        {
            Page.Box(500, 316, 1.5, 12)(context);
            Page.Box(580, 302, 1.5, 26)(context);
            context.DrawRectangle(grey, null, new System.Windows.Rect(501, 314, 30, 1));
            context.DrawLine(new Pen(grey, 0.9), new System.Windows.Point(546, 309), new System.Windows.Point(581, 302.5)); // thin and slanted: greys of all shades
            Page.Text("3", 537, 318, 14, italic: true, serif: true)(context);
        });
        page.Add(Page.Text("두 번째 줄 가사입니다", 160, 530, 34, shadow: 3, shade: pale));
        var picture = page.Picture();
        var target = Target(picture);
        var rows = ScoreLayout.FindRows(picture, target);
        Assert.True(Shows(picture, rows, 1, target, bracket) >= 0.97 && Shows(picture, rows, 0, target, bracket) <= 0.01, "the bracket: " + Report(picture, rows, target, bracket));
        Assert.True(Shows(picture, rows, 0, target, lyrics) >= 0.98, "the lyrics: " + Report(picture, rows, target, lyrics));
    });

    // Two rows as the hymn decks draw them: row 1's stems up, row 2's down (its ink starts at its staff), and a first
    // verse of 30px lyrics under row 1.
    private static Page Verses(out bool[] first, double size = 30, bool bold = true)
    {
        var page = new Page();
        page.Add(Page.Staff(150));
        page.Add(Page.Staff(400));
        page.Add(context =>
        {
            for (int x = 200; x < 1050; x += 70) { Page.Box(x - 10, 168, 12, 9)(context); Page.Box(x, 130, 2, 44)(context); Page.Box(x - 10, 418, 12, 9)(context); Page.Box(x - 10, 422, 2, 40)(context); }
        });
        first = page.Add(Page.Text("1. 주님의 크신 사랑 영원히 노래합니다", 160, 250, size, bold: bold, shadow: 2));
        return page;
    }

    [Theory]
    [InlineData("으", true)]
    [InlineData("스", true)]
    [InlineData("드", true)]
    [InlineData("2. 으", true)]
    [InlineData("으", false)]
    [InlineData("트", false)]
    [InlineData("의", false)]
    public void ASecondVerseOfOneSyllableOverAFlatVowelStaysWithItsRow(string verse, bool bold) => StaHelper.RunOnSta(() =>
    {
        var page = Verses(out var first, bold: bold);
        // A consonant over the flat vowel eu, the two apart by a few blank rows, under the first verse.
        var second = page.Add(Page.Text(verse, 400, 289, 30, bold: bold, shadow: 2));
        var picture = page.Picture();
        var target = Target(picture);
        var rows = ScoreLayout.FindRows(picture, target);
        Assert.True(Shows(picture, rows, 0, target, second) >= 0.98 && Shows(picture, rows, 1, target, second) <= 0.01, $"\"{verse}\": " + Report(picture, rows, target, second));
        Assert.True(Shows(picture, rows, 0, target, first) >= 0.98, "the first verse: " + Report(picture, rows, target, first));
    });

    // Two rows of notes every 70 pixels, their stems up, and row 1's lyrics; what a test draws over staff 1 is masked.
    private static Page OverStaffOne(Action<Page, List<bool[]>> draw, out bool[] marks)
    {
        var page = new Page();
        page.Add(Page.Staff(150));
        page.Add(Page.Staff(400));
        page.Add(context => { for (int x = 200; x < 1050; x += 70) { Page.Box(x - 10, 168, 12, 9)(context); Page.Box(x, 130, 2, 44)(context); Page.Box(x - 10, 418, 12, 9)(context); Page.Box(x, 380, 2, 44)(context); } });
        var parts = new List<bool[]>();
        draw(page, parts);
        marks = new bool[Page.Width * Page.Height];
        foreach (var part in parts) for (int at = 0; at < marks.Length; at++) marks[at] |= part[at];
        page.Add(Page.Text("주님의 크신 사랑 영원히 노래합니다", 160, 250, 30, bold: true, shadow: 2));
        return page;
    }

    [Fact]
    public void EightTripletsOverTheFirstStaffStayInItsRow() => StaHelper.RunOnSta(() =>
    {
        // Each a bracket broken by its italic 3, over the stems' tops, side by side across the staff.
        var page = OverStaffOne((page, parts) =>
        {
            int step = 850 / 8;
            for (int i = 0; i < 8; i++)
            {
                int x = 190 + i * step, w = step - 14;
                parts.Add(page.Add(context => { Page.Box(x, 112, w / 2 - 8, 2)(context); Page.Box(x, 112, 2, 7)(context); Page.Box(x + w / 2 + 8, 112, w - w / 2 - 8, 2)(context); Page.Box(x + w - 2, 112, 2, 7)(context); }));
                parts.Add(page.Add(Page.Text("3", x + w / 2 - 4, 118, 14, italic: true)));
            }
        }, out var tuplets);
        var picture = page.Picture();
        var target = Target(picture);
        var rows = ScoreLayout.FindRows(picture, target);
        Assert.True(Shows(picture, rows, 0, target, tuplets) >= 0.98, "the triplets: " + Report(picture, rows, target, tuplets));
    });

    [Fact]
    public void AChordOnEveryBeatOverTheFirstStaffStaysInItsRow() => StaHelper.RunOnSta(() =>
    {
        string[] names = ["G", "D/F#", "Em7", "C", "Am7", "D", "G/B", "C", "Dsus4", "D", "G", "Em"];
        var page = OverStaffOne((page, parts) =>
        {
            for (int i = 0, x = 196; x < 1040; i++, x += 70) parts.Add(page.Add(Page.Text(names[i % names.Length], x, 124, 20)));
        }, out var chords);
        var picture = page.Picture();
        var target = Target(picture);
        var rows = ScoreLayout.FindRows(picture, target);
        Assert.True(Shows(picture, rows, 0, target, chords) >= 0.98, "the chords: " + Report(picture, rows, target, chords));
    });

    // A small red title at the left, as close to the staff as the marks, and black marks over the notes: the title is no
    // mark (design 5h, round 10), the marks are.
    [Theory]
    [InlineData("tuplets")]
    [InlineData("chords")]
    public void ARedTitleOverTheFirstStaffStaysOutAndTheBlackMarksBesideItStayIn(string kind) => StaHelper.RunOnSta(() =>
    {
        bool[]? title = null;
        var page = OverStaffOne((page, parts) =>
        {
            title = page.Add(Page.Text("내 안에 사는 이", 110, 122, 16, bold: true, ink: new SolidColorBrush(Color.FromRgb(220, 30, 40))));
            if (kind == "tuplets")
                for (int i = 0; i < 4; i++)
                {
                    int x = 480 + i * 140, w = 126;
                    parts.Add(page.Add(context => { Page.Box(x, 112, w / 2 - 8, 2)(context); Page.Box(x, 112, 2, 7)(context); Page.Box(x + w / 2 + 8, 112, w - w / 2 - 8, 2)(context); Page.Box(x + w - 2, 112, 2, 7)(context); }));
                    parts.Add(page.Add(Page.Text("3", x + w / 2 - 4, 118, 14, italic: true)));
                }
            else
                foreach (var (name, x) in new[] { ("G", 476), ("D/F#", 616), ("Em7", 756), ("C", 896) }) parts.Add(page.Add(Page.Text(name, x, 124, 20)));
        }, out var marks);
        var picture = page.Picture();
        var target = Target(picture);
        var rows = ScoreLayout.FindRows(picture, target);
        Assert.True(Shows(picture, rows, 0, target, marks) >= 0.98, $"the {kind}: " + Report(picture, rows, target, marks));
        Assert.True(Shows(picture, rows, 0, target, title!) <= 0.01, "the title: " + Report(picture, rows, target, title!));
    });

    // Row 1's lyrics a few rows over row 2's mark under one of their flat syllables (geu, deu: a bar at the bottom): a
    // bracket with both arms, a flat slur (its ends a little lower than its middle).
    [Theory]
    [InlineData("그 크신 사랑 영원히", "그", "bracket", false, 4)]
    [InlineData("주님의 그 사랑 영원히", "그", "slur", true, 4)]
    [InlineData("주님 은혜 노래 드리세", "드", "slur", true, 2)]
    public void ANextRowsMarkUnderAFlatSyllableStaysWithItsRow(string line, string under, string kind, bool bold, int gap) => StaHelper.RunOnSta(() =>
    {
        var page = new Page();
        page.Add(Page.Staff(150));
        page.Add(context => { for (int x = 200; x < 1050; x += 70) { Page.Box(x - 10, 168, 12, 9)(context); Page.Box(x, 130, 2, 44)(context); } });
        page.Add(Page.Staff(420));
        page.Add(context => { for (int x = 200; x < 1050; x += 70) { Page.Box(x - 10, 438, 12, 9)(context); Page.Box(x, 395, 2, 48)(context); } });
        const int bottom = 381;
        int top = bottom + 1 + gap;
        var lyrics = page.Add(Page.Text(line, 300, bottom, 30, bold: bold, shadow: 2));
        int at = line.IndexOf(under);
        (int Left, int Right) Span(string text)
        {
            var ink = Mask(Page.Draw(Page.Text(text, 300, bottom, 30, bold: bold)));
            var xs = Enumerable.Range(0, ink.Length).Where(p => ink[p]).Select(p => p % Page.Width).ToList();
            return (xs.Min(), xs.Max());
        }
        var whole = Span(line[..(at + 1)]);
        int right = whole.Right, left = at == 0 ? whole.Left : Math.Max(Span(line[..at]).Right + 1, right - 28);
        var mark = kind == "bracket"
            ? page.Add(context => { int l = left - 4, w = Math.Max(24, right - left + 8); Page.Box(l, top, w, 2)(context); Page.Box(l, top, 2, 7)(context); Page.Box(l + w - 2, top, 2, 7)(context); })
            : page.Add(context =>
            {
                var arc = new StreamGeometry();
                using (var draw = arc.Open())
                {
                    draw.BeginFigure(new System.Windows.Point(left - 6, top + 3), false, false);
                    draw.QuadraticBezierTo(new System.Windows.Point((left + right + 4) / 2.0, top + 1), new System.Windows.Point(right + 10, top + 3), true, false);
                }
                context.DrawGeometry(null, new Pen(Brushes.Black, 3), arc);
            });
        var picture = page.Picture();
        var target = Target(picture);
        var rows = ScoreLayout.FindRows(picture, target);
        Assert.True(Shows(picture, rows, 1, target, mark) >= 0.97 && Shows(picture, rows, 0, target, mark) <= 0.01, $"the {kind} under {under}: " + Report(picture, rows, target, mark));
        Assert.True(Shows(picture, rows, 0, target, lyrics) >= 0.98, "the lyrics: " + Report(picture, rows, target, lyrics));
    });

    // A deck of score pictures: three slides of 30px lyrics and one whose own picture is given.
    internal static string LyricsDeck(string folder, BitmapSource own)
    {
        static byte[] Encode(BitmapSource picture)
        {
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(picture));
            using var stream = new MemoryStream();
            encoder.Save(stream);
            return stream.ToArray();
        }
        var large = Verses(out _);
        large.Add(Page.Text("그 사랑 안에서 우리 함께 걸어요", 160, 520, 30, bold: true, shadow: 2));
        var slide = (int n) => Image(n, Full, Full * Page.Height / Page.Width);
        return WriteDeck(folder, [Encode(large.Picture()), Encode(own)], slide(0), slide(0), slide(0), slide(1));
    }

    // A slide of 17px lyrics under row 1, nearer row 2's staff than row 1's (row 1's stems down, row 2's up).
    internal static BitmapSource SmallLyrics(out bool[] lyrics)
    {
        var page = new Page();
        page.Add(Page.Staff(150));
        page.Add(Page.Staff(400));
        page.Add(context =>
        {
            for (int x = 200; x < 1050; x += 70) { Page.Box(x - 10, 168, 12, 9)(context); Page.Box(x - 10, 172, 2, 40)(context); Page.Box(x - 10, 418, 12, 9)(context); Page.Box(x, 380, 2, 44)(context); }
        });
        lyrics = page.Add(Page.Text("주님의 크신 사랑 영원히 노래합니다", 160, 335, 17, bold: true, shadow: 1));
        return page.Picture();
    }

    [Fact]
    public void ASlideOfSmallerLyricsThanTheDecksKeepsThemInItsRow() => StaHelper.RunOnSta(() => InFolder(folder =>
    {
        var path = LyricsDeck(folder, SmallLyrics(out var lyrics));
        var deck = CaptionDeck.Read(path);
        var (slides, target) = CaptionConsole.PrepareScore(deck.Path, deck.Slides, CancellationToken.None);
        var (image, page) = slides[4]!.Value;
        Assert.Equal(Page.Width, image.PixelWidth);
        var rows = page.Fit(target);
        double Sent(int k)
        {
            var cut = (ScoreRow[])rows.Clone();
            cut[k] = cut[k] with { Erase = page.Erase(k) };
            var sent = new FormatConvertedBitmap(CaptionConsole.CropRow(image, k, cut)!.Value.Image, PixelFormats.Gray8, null, 0);
            var shown = new byte[sent.PixelWidth * sent.PixelHeight];
            sent.CopyPixels(shown, sent.PixelWidth, 0);
            int all = 0, dark = 0;
            for (int at = 0; at < lyrics.Length; at++)
            {
                if (!lyrics[at]) continue;
                all++;
                int x = at % Page.Width - cut[k].Area.X, y = at / Page.Width - cut[k].Area.Y;
                if (x >= 0 && x < cut[k].Area.Width && y >= 0 && y < cut[k].Area.Height && shown[(y + cut[k].PadTop) * sent.PixelWidth + x] < 180) dark++;
            }
            return dark / (double)all;
        }
        Assert.True(Sent(0) >= 0.98 && Sent(1) <= 0.01, $"the small lyrics: row 1 {Sent(0):P1}, row 2 {Sent(1):P1}");
        return 0;
    }));

    // A slide of 26px lyrics with a second verse of one 17px syllable right under them: a little smaller than the deck's
    // letters, so it is a lyric line by its own picture's letters and a mark by the deck's.
    internal static BitmapSource SomewhatSmallerLyrics()
    {
        var page = Verses(out _, size: 26);
        page.Add(Page.Text("주", 400, 276, 17, bold: true, shadow: 1));
        return page.Picture();
    }

    [Fact]
    public void ThePictureCacheDropsTheLeastRecentlyUsedSlideButNeverTheOneOnAir() => StaHelper.RunOnSta(() =>
    {
        var picture = Page.Draw(Page.Staff(150), Page.Text("가사 첫 줄", 160, 250, 28));
        var page = ScoreLayout.Layout(picture);
        var cache = new ScorePictures(2 * CaptionConsole.Bytes(picture, page));
        cache.Keep(("a.pptx", 1), picture, page);
        cache.Keep(("a.pptx", 2), picture, page);
        cache.OnAir = ("a.pptx", 1); // the oldest, on air
        cache.Keep(("a.pptx", 3), picture, page);
        Assert.True(cache.Contains(("a.pptx", 1)), "the slide on air was dropped");
        Assert.False(cache.Contains(("a.pptx", 2)));
        Assert.True(cache.Contains(("a.pptx", 3)), "the slide just read was dropped");
        cache.OnAir = null;
        Assert.True(cache.TryGet(("a.pptx", 1), out _)); // used again: slide 3 is now the least recent
        cache.Keep(("a.pptx", 4), picture, page);
        Assert.True(cache.Contains(("a.pptx", 1)) && cache.Contains(("a.pptx", 4)) && !cache.Contains(("a.pptx", 3)));
    });

    [Theory]
    [InlineData("다")]  // a syllable
    [InlineData("2.")]  // a verse number
    [InlineData("-")]   // a hyphen
    public void WhatLiesOnTheLyricsRowsBetweenTwoStemsOfTheNextRowStaysWithTheLyrics(string text) => StaHelper.RunOnSta(() =>
    {
        var page = new Page();
        page.Add(Page.Staff(150));
        page.Add(Notes(176, 140, 160, 220, 280, 340, 400, 460, 520, 580, 640, 700, 760, 820, 880, 940, 1000, 1060));
        var lyrics = page.Add(Page.Text("주님의 크신 사랑 영원히", 160, 330, 18, bold: true));
        var mark = page.Add(Page.Text(text, 700, 330, 18, bold: true));
        int right = Enumerable.Range(0, Page.Width * Page.Height).Where(at => mark[at]).Max(at => at % Page.Width);
        // Two notes of row 2 whose stems rise into the lyrics' rows, 3 pixels on either side of it (not touching it).
        page.Add(context => { Page.Box(695, 310, 2, 90)(context); Page.Box(right + 4, 310, 2, 90)(context); Page.Box(686, 396, 10, 8)(context); Page.Box(right - 5, 396, 10, 8)(context); });
        page.Add(Page.Staff(400));
        page.Add(Notes(426, 390, 160, 220, 280, 340, 400, 460, 520, 580));
        page.Add(Page.Text("두 번째 줄 가사입니다", 160, 490, 18, bold: true));
        var picture = page.Picture();
        var target = Target(picture);
        var rows = ScoreLayout.FindRows(picture, target);
        Assert.True(Shows(picture, rows, 0, target, mark) >= 0.98 && Shows(picture, rows, 1, target, mark) <= 0.01, $"\"{text}\": " + Report(picture, rows, target, mark));
        Assert.True(Shows(picture, rows, 0, target, lyrics) >= 0.98, "the lyrics: " + Report(picture, rows, target, lyrics));
    });

    [Fact]
    public void AFlagOfTheNextRowInTheLyricsRowsStaysWithItsNote() => StaHelper.RunOnSta(() =>
    {
        var page = new Page();
        page.Add(Page.Staff(150));
        page.Add(Notes(176, 140, 160, 220, 280, 340, 400, 460, 520, 580, 640, 700, 760, 820, 880, 940, 1000, 1060));
        var words = page.Add(Page.Text("주님의 크신 사랑", 160, 332, 20, bold: true));
        page.Add(Page.Text("영원히 노래합니다", 560, 332, 20, bold: true));
        int stem = Enumerable.Range(0, Page.Width * Page.Height).Where(at => words[at]).Max(at => at % Page.Width) + 30;
        // An eighth note of row 2 in the gap between the words: its stem rises to 314 and its flag hangs down to 329.
        var note = page.Add(context =>
        {
            Page.Box(stem - 12, 396, 12, 9)(context);
            Page.Box(stem, 314, 2, 86)(context);
            var flag = new StreamGeometry();
            using (var draw = flag.Open())
            {
                draw.BeginFigure(new System.Windows.Point(stem + 2, 314), true, true);
                draw.BezierTo(new System.Windows.Point(stem + 12, 318), new System.Windows.Point(stem + 14, 323), new System.Windows.Point(stem + 10, 329), true, false);
                draw.LineTo(new System.Windows.Point(stem + 2, 320), true, false);
            }
            context.DrawGeometry(Brushes.Black, null, flag);
        });
        page.Add(Page.Staff(400));
        page.Add(Notes(426, 390, 160, 220, 280, 340));
        page.Add(Page.Text("두 번째 줄 가사입니다", 160, 490, 20, bold: true));
        var picture = page.Picture();
        var target = Target(picture);
        var rows = ScoreLayout.FindRows(picture, target);
        Assert.True(Shows(picture, rows, 1, target, note) >= 0.98 && Shows(picture, rows, 0, target, note) <= 0.01, "the note and its flag: " + Report(picture, rows, target, note));
        Assert.True(Shows(picture, rows, 0, target, words) >= 0.98, "the lyrics: " + Report(picture, rows, target, words));
    });

    [Fact]
    public void ATextPlaceholderWithNoPositionOfItsOwnSendsTheSlideToTheRender() => StaHelper.RunOnSta(() => InFolder(folder =>
    {
        var score = Image(1, Full, Wide, x: 1524000);
        // A placeholder takes its place from the layout: where it shows is not known here.
        static string Placeholder(string text) =>
            $"""<p:sp><p:nvSpPr><p:cNvPr id="5" name="ph"/><p:cNvSpPr/><p:nvPr><p:ph type="body" idx="1"/></p:nvPr></p:nvSpPr><p:spPr/>{(text.Length > 0 ? $"<p:txBody><a:bodyPr/><a:p><a:r><a:t>{text}</a:t></a:r></a:p></p:txBody>" : "")}</p:sp>""";
        var path = WriteDeck(folder, [Photo(40), ScorePng()],
            score + Placeholder("1/13"),  // 1: its text over the score, maybe
            score + Placeholder(""),      // 2: an empty placeholder shows nothing
            Placeholder("1/13") + score); // 3: under the score
        var parts = CaptionDeck.Read(path).Slides.Select(s => s.Picture?.Part).ToArray();
        Assert.Null(parts[0]);
        Assert.Equal("/ppt/media/image1.png", parts[1]);
        Assert.Equal("/ppt/media/image1.png", parts[2]);
        return 0;
    }));

    [Fact]
    public void TwelveLargePicturesAreMeasuredQuickly() => StaHelper.RunOnSta(() =>
    {
        // 2400×1800 pictures of six staves with notes and lyric blocks; design 5g asks 1 s for 12 (this allows for a busy machine).
        var pictures = Enumerable.Range(0, 12).Select(seed =>
        {
            const int width = 2400, height = 1800;
            var pixels = new byte[width * height];
            Array.Fill(pixels, (byte)255);
            var random = new Random(seed);
            void Fill(int x, int y, int w, int h) { for (int r = y; r < y + h; r++) Array.Fill(pixels, (byte)0, r * width + x, w); }
            for (int staff = 0; staff < 6; staff++)
            {
                int top = 150 + staff * 270;
                for (int line = 0; line < 5; line++) Fill(60, top + line * 24, width - 120, 2);
                for (int x = 200; x < width - 150; x += 72)
                {
                    int y = top + random.Next(-2, 9) * 12;
                    Fill(x, y, 26, 18);
                    Fill(x + 24, y - 80, 3, 90);
                }
                for (int x = 200; x < width - 200; x += 60) Fill(x, top + 140, 40, 40); // lyric letters
            }
            var image = BitmapSource.Create(width, height, 96, 96, PixelFormats.Gray8, null, pixels, width);
            image.Freeze();
            return image;
        }).ToArray();
        ScoreLayout.Layout(pictures[0]); // warm up
        var clock = System.Diagnostics.Stopwatch.StartNew();
        var pages = pictures.Select(picture => ScoreLayout.Layout(picture)).ToArray();
        clock.Stop();
        Assert.All(pages, page => Assert.Equal(6, page.Rows.Length));
        Assert.True(clock.ElapsedMilliseconds < 4000, $"12 pictures measured in {clock.ElapsedMilliseconds} ms");
    });

    [Fact]
    public void ALyricLetterTouchingTheNextStemStaysWithItsLine() => StaHelper.RunOnSta(() =>
    {
        var image = Staves(12, fill =>
        {
            Staff(fill, 150);
            for (int letter = 0; letter < 10; letter++) fill(150 + 70 * letter, 330, 40, 24); // row 1's lyrics (330..353)
            fill(300, 354, 2, 46);    // a stem of row 2 running up into one letter (354..400)
            Staff(fill, 400);
            fill(150, 478, 800, 24);
        });
        var letters = Mask(Staves(12, fill => { for (int letter = 0; letter < 10; letter++) fill(150 + 70 * letter, 330, 40, 24); }));
        var stem = Mask(Staves(12, fill => fill(300, 360, 2, 40)));
        var target = Target(image);
        var rows = ScoreLayout.FindRows(image, target);
        Assert.True(Shows(image, rows, 0, target, letters) >= 0.99, "row 1's letters are cut");
        Assert.True(Shows(image, rows, 1, target, letters) <= 0.01, "row 2 shows row 1's letters");
        Assert.True(Shows(image, rows, 1, target, stem) >= 0.99, "row 2's stem is cut");
    });

    [Fact]
    public void MarksJustOverTheStaffBelongToItsRow() => StaHelper.RunOnSta(() =>
    {
        var image = Staves(12, fill =>
        {
            Staff(fill, 150);
            fill(150, 228, 800, 24);  // row 1's lyrics (to 252)
            Staff(fill, 450);
            fill(300, 426, 14, 24);   // a note above row 2's staff
            fill(296, 414, 22, 6);    // a fermata half a spacing over it (414..419)
            fill(600, 440, 8, 6);     // a triplet's 3 just over the staff
            fill(150, 528, 800, 24);
        });
        var rows = PictureRows(image);
        Assert.InRange(End(rows[0]), 252, 413); // all its lyrics, not the fermata
        Assert.True(rows[1].Area.Y <= 414, $"the fermata is cut off: row 2 starts at {rows[1].Area.Y}");
    });

    [Fact]
    public void MusicJoinedToTheStaffIsInAndATitleApartIsNot() => StaHelper.RunOnSta(() =>
    {
        // A beam over two stems rises 80 px (6.7 spacings) from the staff; the title above it does not touch it.
        var beamed = Staves(12, fill =>
        {
            fill(200, 100, 500, 40);                                          // the title (100..140)
            fill(300, 220, 3, 80); fill(497, 220, 3, 80); fill(300, 220, 200, 6); // stems and beam (220..300)
            Staff(fill, 300);
            fill(150, 378, 800, 24);
        });
        // A title the clef runs into may come along with it (the spec allows it); the clef is whole either way.
        var touching = Staves(12, fill =>
        {
            fill(200, 160, 500, 40);  // the title (160..200)
            fill(210, 200, 14, 100);  // a clef from it down to the staff
            Staff(fill, 300);
            fill(150, 378, 800, 24);
        });
        var target = Target(beamed, touching);
        var beam = ScoreLayout.FindRows(beamed, target)[0];
        Assert.InRange(beam.Area.Y, 141, 220); // the whole beam, no title
        var clef = Mask(Staves(12, fill => fill(210, 200, 14, 100)));
        Assert.True(Shows(touching, ScoreLayout.FindRows(touching, target), 0, target, clef) >= 0.99, "the clef under a title is cut");
    });

    [Fact]
    public void ThickBandsAreNoStaffButDenseNotesOnABlotchyBackgroundAre() => StaHelper.RunOnSta(() =>
    {
        // Thick parallel bands like a sunset photo drawn under the score.
        var bands = Staves(12, fill => { for (int line = 0; line < 5; line++) fill(100, 600 + line * 12, 1000, 5); }, (150, 12));
        Assert.Single(ScoreLayout.Layout(bands).Rows);
        // A slide render finds its staves as before content pictures (design 5d): the bands make a second staff there.
        Assert.Equal([150, 602], ScoreLayout.FindRows(bands).Select(r => r.StaffTop));
        // Three staves crowded with notes and stems on a picture with dark blotches.
        const int width = 1920, height = 1080, spacing = 10;
        var pixels = new byte[width * height];
        for (int y = 0; y < height; y++) for (int x = 0; x < width; x++) pixels[y * width + x] = (x / 40 + y / 40) % 3 == 0 ? (byte)90 : (byte)230;
        foreach (int top in new[] { 150, 450, 750 })
        {
            for (int line = 0; line < 5; line++) for (int x = 150; x < 1770; x++) pixels[(top + line * spacing) * width + x] = pixels[(top + line * spacing + 1) * width + x] = 20;
            for (int x = 220, j = 0; x < 1740; x += spacing + 2, j++)
            {
                int note = top + j % 9 * spacing / 2;
                for (int dy = -spacing / 2; dy <= spacing / 2; dy++)
                    for (int dx = -spacing * 2 / 3; dx <= spacing * 2 / 3; dx++)
                        if (dx * dx * 4 / (spacing * spacing + 1) + dy * dy * 4 / (spacing * spacing + 1) <= 1) pixels[(note + dy) * width + x + dx] = 20;
                for (int dy = 0; dy < 3 * spacing; dy++) pixels[(note - dy) * width + x + spacing / 2] = 20; // its stem
            }
        }
        var dense = BitmapSource.Create(width, height, 96, 96, PixelFormats.Gray8, null, pixels, width);
        Assert.Equal([150, 450, 750], ScoreLayout.Layout(dense).Rows.Select(r => r.StaffTop));
        Assert.Equal([150, 450, 750], ScoreLayout.FindRows(dense).Select(r => r.StaffTop));
    });

    [Fact]
    public void TheLastRowEndsWithItsLyricsNotWithTheArtUnderThem() => StaHelper.RunOnSta(() =>
    {
        // One row; its lyrics end at 252.
        var art = Staves(12, fill => fill(100, 264, 1000, 300), (150, 12));    // a picture band drawn right under them
        var verses = Staves(12, fill => fill(150, 262, 800, 24), (150, 12));  // a second verse line instead (to 286)
        Assert.InRange(End(PictureRows(art)[0]), 252, 263);
        Assert.True(End(PictureRows(verses)[0]) >= 286, $"the second verse is not in: the row ends at {End(PictureRows(verses)[0])}");
        // Lyrics printed over a photo read as one tall band: the last row still reaches as far as the row above it.
        var overPhoto = Staves(12, fill => fill(100, 510, 1000, 300), (150, 12), (450, 12));
        Assert.True(End(PictureRows(overPhoto)[1]) >= 552, $"the last row's lyrics are out: it ends at {End(PictureRows(overPhoto)[1])}");
    });

    [Fact]
    public void ADeckOfFewerThanTenRowsTakesTheMedianReach()
    {
        var few = ScoreLayout.Target([[new(1, 4), new(1, 4), new(3.5, 9)], [new(1, 4), new(1.5, 6)]]);
        Assert.Equal(new RowExtent(1, 4), few);
        var many = ScoreLayout.Target([[.. Enumerable.Range(1, 10).Select(n => new RowExtent(n, 10 - n))]]);
        Assert.Equal(new RowExtent(9, 8), many); // the 90th percentile
    }

    [Fact]
    public void APicturePatchLaidOverTheScoreSendsTheSlideToTheRender() => StaHelper.RunOnSta(() => InFolder(folder =>
    {
        var score = Image(1, Full, Wide, x: 1524000);
        var patch = Image(0, 800000, 300000, x: 3000000, y: 3000000);
        var path = WriteDeck(folder, [Photo(200), ScorePng()],
            score + patch,                                                            // 1: a word patched over with a picture
            score + Pic("r:embed=\"img0\"", 3000000, 3000000, 800000, 300000, link: true), // 2: a linked picture (a button)
            score + Group(9920288, 4700000, 423862, 431800, 267, 272,                 // 3: a verse button: its picture unlinked, its number linked
                Image(0, 267, 272, x: 5289, y: 3975) + Box(5366, 4010, 100, 192, "4", link: true)),
            patch + score,                                                            // 4: the patch under the score
            score + Image(0, 800000, 300000, x: 900000, y: 3000000));                 // 5: mostly beside the score
        var parts = CaptionDeck.Read(path).Slides.Select(s => s.Picture?.Part).ToArray();
        Assert.Equal([null, "/ppt/media/image1.png", "/ppt/media/image1.png", "/ppt/media/image1.png", "/ppt/media/image1.png"], parts);
        return 0;
    }));

    [Fact]
    public void RowsAreCutRightFromManyThreadsAtOnce()
    {
        var two = Staves(12, (150, 12), (500, 12));
        var three = Staves(12, (100, 12), (350, 12), (600, 12));
        int wrong = 0;
        // The same picture cut for a slide render (no target) and for a deck (a target) by turns: each gets its own rows.
        var deck = new RowExtent(6, 10);
        int plain = CaptionConsole.CropRow(two, 0)!.Value.Image.PixelHeight, sized = CaptionConsole.CropRow(two, 0, deck)!.Value.Image.PixelHeight;
        Assert.NotEqual(plain, sized);
        var threads = Enumerable.Range(0, 8).Select(i => new Thread(() =>
        {
            for (int n = 0; n < 40; n++)
            {
                var image = (i + n) % 2 == 0 ? two : three;
                RowExtent? target = (i + n) % 4 == 0 ? deck : null;
                var cut = CaptionConsole.CropRow(image, 0, target);
                if (cut?.Rows != (image == two ? 2 : 3)) Interlocked.Increment(ref wrong);
                if (image == two && cut?.Image.PixelHeight != (target == null ? plain : sized)) Interlocked.Increment(ref wrong);
            }
        })).ToList();
        threads.ForEach(t => { t.SetApartmentState(ApartmentState.STA); t.Start(); });
        threads.ForEach(t => t.Join());
        Assert.Equal(0, wrong);
    }

    [Fact]
    public void LyricsAreReadRowByRowFromTheRowsTheScoreSends() => StaHelper.RunOnSta(() => InFolder(folder =>
    {
        var path = WriteDeck(folder, [ScorePng()], Image(0, Full, Wide));
        var deck = CaptionDeck.Read(path);
        var (png, rows) = CaptionConsole.OcrPicture(deck.Path, deck.Slides[0])!.Value;
        var picture = CaptionConsole.ScorePicture(deck.Path, deck.Slides[0])!.Value.Image;
        var (_, target) = CaptionConsole.PrepareScore(deck.Path, deck.Slides, CancellationToken.None);
        // The picture read is the content picture, and its rows are the rows sent, staff for staff.
        var read = BitmapDecoder.Create(new MemoryStream(png), BitmapCreateOptions.None, BitmapCacheOption.OnLoad).Frames[0];
        Assert.Equal((picture.PixelWidth, picture.PixelHeight), (read.PixelWidth, read.PixelHeight));
        Assert.Equal(ScoreLayout.FindRows(picture, target).Select(r => (r.StaffTop, r.StaffBottom)), rows.Select(r => (r.StaffTop, r.StaffBottom)));
        // A line read under each staff (the lyrics at 165..190 and 345..370) goes to that staff's row.
        static OcrLine Line(int center, string text) => new([new(text, 100, center - 10, 200, 20)]);
        var page = new OcrPage("1.png", null, [Line(177, "첫째줄"), Line(357, "둘째줄")]);
        Assert.Equal(["첫째줄", "둘째줄"], LyricsReader.RowLyrics(page, rows));
        return 0;
    }));

    private static string Box(long x, long y, long cx, long cy, string text = "", string fill = "", bool link = false) =>
        $"""<p:sp><p:nvSpPr><p:cNvPr id="4" name="box">{(link ? "<a:hlinkClick r:id=\"img0\" action=\"ppaction://hlinksldjump\"/>" : "")}</p:cNvPr><p:cNvSpPr/><p:nvPr/></p:nvSpPr><p:spPr><a:xfrm><a:off x="{x}" y="{y}"/><a:ext cx="{cx}" cy="{cy}"/></a:xfrm>{fill}</p:spPr>{(text.Length > 0 ? $"<p:txBody><a:bodyPr/><a:p><a:r><a:t>{text}</a:t></a:r></a:p></p:txBody>" : "")}</p:sp>""";

    [Fact]
    public void ATextBoxLaidOverTheScoreSendsTheSlideToTheRender() => StaHelper.RunOnSta(() => InFolder(folder =>
    {
        var score = Image(1, Full, Wide, x: 1524000);
        const string white = """<a:solidFill><a:schemeClr val="bg1"/></a:solidFill>""";
        var path = WriteDeck(folder, [Photo(40), ScorePng()],
            score + Box(3000000, 3000000, 4000000, 600000, "그분말씀 좋-은 걸"),                  // 1: other lyrics over the score
            score + Box(3000000, 5300000, 4000000, 600000, "1절"),                              // 2: text under the score, not on it
            score + Box(9920288, 4700000, 423862, 431800, "4", link: true),                     // 3: a verse button on the score's corner
            score + Box(3000000, 3000000, 4000000, 600000, fill: white),                        // 4: a white patch over the score
            Box(3000000, 3000000, 4000000, 600000, "아래") + score,                              // 5: text under the score
            score + Group(1524000, 0, Full, Wide, 267, 272, Box(5389, 4100, 100, 30, "고친 가사")), // 6: text in a group, over the score
            score + Box(500000, 3000000, 1200000, 600000, "반쯤"));                             // 7: a box mostly beside the score
        var parts = CaptionDeck.Read(path).Slides.Select(s => s.Picture?.Part).ToArray();
        Assert.Equal([null, "/ppt/media/image1.png", "/ppt/media/image1.png", null, "/ppt/media/image1.png", null, "/ppt/media/image1.png"], parts);
        return 0;
    }));

    // A line (a connector) and an object frame (a table) at x,y and cx×cy.
    private static string Line(long x, long y, long cx, long cy, bool link = false) =>
        $"""<p:cxnSp><p:nvCxnSpPr><p:cNvPr id="5" name="line">{(link ? "<a:hlinkClick r:id=\"img0\" action=\"ppaction://hlinksldjump\"/>" : "")}</p:cNvPr><p:cNvCxnSpPr/><p:nvPr/></p:nvCxnSpPr><p:spPr><a:xfrm><a:off x="{x}" y="{y}"/><a:ext cx="{cx}" cy="{cy}"/></a:xfrm><a:prstGeom prst="line"><a:avLst/></a:prstGeom></p:spPr></p:cxnSp>""";

    private static string Table(long x, long y, long cx, long cy) =>
        $"""<p:graphicFrame><p:nvGraphicFramePr><p:cNvPr id="6" name="table"/><p:cNvGraphicFramePr/><p:nvPr/></p:nvGraphicFramePr><p:xfrm><a:off x="{x}" y="{y}"/><a:ext cx="{cx}" cy="{cy}"/></p:xfrm><a:graphic><a:graphicData uri="http://schemas.openxmlformats.org/drawingml/2006/table"/></a:graphic></p:graphicFrame>""";

    // A shape filled by its style (the theme's fill), with the given fill of its own (none: the style's shows).
    private static string Styled(long x, long y, long cx, long cy, string fill = "") =>
        $"""<p:sp><p:nvSpPr><p:cNvPr id="7" name="shape"/><p:cNvSpPr/><p:nvPr/></p:nvSpPr><p:spPr><a:xfrm><a:off x="{x}" y="{y}"/><a:ext cx="{cx}" cy="{cy}"/></a:xfrm><a:prstGeom prst="rect"><a:avLst/></a:prstGeom>{fill}</p:spPr><p:style><a:lnRef idx="2"/><a:fillRef idx="1"/><a:effectRef idx="0"/><a:fontRef idx="minor"/></p:style></p:sp>""";

    [Fact]
    public void ALineATableOrAStyledOrPatternedShapeOverTheScoreSendsTheSlideToTheRender() => StaHelper.RunOnSta(() => InFolder(folder =>
    {
        var score = Image(1, Full, Wide, x: 1524000);
        var path = WriteDeck(folder, [Photo(40), ScorePng()],
            score + Line(3000000, 3000000, 4000000, 0),                                          // 1: a line across the score
            score + Table(3000000, 3000000, 4000000, 600000),                                     // 2: a table over it
            score + Styled(3000000, 3000000, 4000000, 600000),                                    // 3: a shape the theme fills
            score + Styled(3000000, 3000000, 4000000, 600000, "<a:noFill/>"),                     // 4: the same, with no fill
            score + Box(3000000, 3000000, 4000000, 600000, fill: """<a:pattFill prst="pct5"/>"""), // 5: a patterned patch
            score + Line(9920288, 4700000, 423862, 0, link: true),                                // 6: a linked line (a button)
            score + Table(3000000, 5300000, 4000000, 600000));                                    // 7: a table under the score
        var parts = CaptionDeck.Read(path).Slides.Select(s => s.Picture?.Part ?? "render").ToArray();
        const string picture = "/ppt/media/image1.png";
        Assert.Equal(["render", "render", "render", picture, "render", picture, picture], parts);
        return 0;
    }));

    [Fact]
    public void ADrawingIsNoContentPictureAndIsNotRead() => StaHelper.RunOnSta(() => InFolder(folder =>
    {
        // The score as an EMF drawing: the render shows it; its bytes are never decoded.
        var path = WriteDeck(folder, [("image/x-emf", "emf", [1, 0, 0, 0, 0x6C, 0, 0, 0])], Image(0, Full, Wide, x: 1524000));
        var slide = Assert.Single(CaptionDeck.Read(path).Slides);
        Assert.True(slide.HasPicture); // no text: still a picture slide (its lyrics are read from the render)
        Assert.Null(slide.Picture);
        return 0;
    }));

    [Fact]
    public void AStaffWhoseMiddleLineIsLeftOutKeepsItsSpacing() => StaHelper.RunOnSta(() =>
    {
        // Lines at 100, 110, 120, 130 and 140; the middle one a thick band, left out of the staff.
        var image = Staves(10, fill => fill(100, 118, 1000, 5), (100, 0));
        var page = ScoreLayout.Layout(image);
        Assert.Single(page.Rows);
        Assert.Equal(10, page.Spacing[0], 3);
    });

    [Fact]
    public void ARowAtThePicturesTopIsPaddedWithWhite() => StaHelper.RunOnSta(() =>
    {
        var deck = Staves(12, (150, 36), (500, 36));
        var edge = Staves(12, (6, 0), (500, 36)); // the first staff touches the picture's top
        var target = Target(deck, edge);
        var rows = ScoreLayout.FindRows(edge, target);
        Assert.InRange(rows[0].Area.Y, 0, 5); // from just above the staff's top line (at 6)
        Assert.True(rows[0].PadTop > 0);
        Assert.Equal(Height(rows[1]), Height(rows[0]));
        var image = CaptionConsole.CropRow(edge, 0, target)!.Value.Image;
        Assert.Equal(Height(rows[1]), image.PixelHeight);
        var gray = new FormatConvertedBitmap(image, PixelFormats.Gray8, null, 0);
        var top = new byte[image.PixelWidth * rows[0].PadTop];
        gray.CopyPixels(new System.Windows.Int32Rect(0, 0, image.PixelWidth, rows[0].PadTop), top, image.PixelWidth, 0);
        Assert.All(top, p => Assert.Equal(255, p)); // white, not black
        // A staff on the picture's last pixel rows is measured within the picture.
        var bottom = Staves(12, fill => { Staff(fill, 150); Staff(fill, 850); });
        Assert.Equal(2, ScoreLayout.FindRows(bottom, target).Length);
    });
}
