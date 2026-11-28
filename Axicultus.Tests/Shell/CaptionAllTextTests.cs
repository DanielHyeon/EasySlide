using System;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Text;
using System.Text.Json;
using Axicultus.Wpf.Captions;
using Axicultus.Wpf.Shell;
using Xunit;

namespace Axicultus.Wpf.Tests.Shell;

// a036: every slide has a "slide's whole text" box, read top to bottom and left to right, like a TXT item.
public sealed class CaptionAllTextTests
{
    private const long SlideWidth = 12192000, SlideHeight = 6858000;
    private const string Rel = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";
    private const string Namespaces = $"""xmlns:p="http://schemas.openxmlformats.org/presentationml/2006/main" xmlns:a="http://schemas.openxmlformats.org/drawingml/2006/main" xmlns:r="{Rel}" xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006" """;

    private static string At(long x, long y, long cx, long cy) => $"""<a:xfrm><a:off x="{x}" y="{y}"/><a:ext cx="{cx}" cy="{cy}"/></a:xfrm>""";

    private static string Paragraphs(string[] lines) =>
        string.Concat(lines.Select(line => line.Length == 0 ? "<a:p/>" : $"<a:p><a:r><a:t>{line}</a:t></a:r></a:p>"));

    private static string Shape(string name, string placeholder, string xfrm, bool hidden, params string[] lines) =>
        $"""<p:sp><p:nvSpPr><p:cNvPr id="9" name="{name}"{(hidden ? " hidden=\"1\"" : "")}/><p:cNvSpPr/><p:nvPr>{placeholder}</p:nvPr></p:nvSpPr><p:spPr>{xfrm}</p:spPr><p:txBody><a:bodyPr/>{Paragraphs(lines)}</p:txBody></p:sp>""";

    private static string Box(string name, long x, long y, long cx, long cy, params string[] lines) => Shape(name, "", At(x, y, cx, cy), false, lines);

    private static string Group(long x, long y, long cx, long cy, long chX, long chY, long chCx, long chCy, string children, bool hidden = false) =>
        $"""<p:grpSp><p:nvGrpSpPr><p:cNvPr id="3" name="group"{(hidden ? " hidden=\"1\"" : "")}/><p:cNvGrpSpPr/><p:nvPr/></p:nvGrpSpPr><p:grpSpPr><a:xfrm><a:off x="{x}" y="{y}"/><a:ext cx="{cx}" cy="{cy}"/><a:chOff x="{chX}" y="{chY}"/><a:chExt cx="{chCx}" cy="{chCy}"/></a:xfrm></p:grpSpPr>{children}</p:grpSp>""";

    private static string Table(long x, long y, params string[][][] rows) =>
        $"""<p:graphicFrame><p:nvGraphicFramePr><p:cNvPr id="6" name="표 1"/><p:cNvGraphicFramePr/><p:nvPr/></p:nvGraphicFramePr><p:xfrm><a:off x="{x}" y="{y}"/><a:ext cx="8000000" cy="1000000"/></p:xfrm><a:graphic><a:graphicData uri="http://schemas.openxmlformats.org/drawingml/2006/table"><a:tbl><a:tblGrid/>{string.Concat(rows.Select(row => "<a:tr h=\"370840\">" + string.Concat(row.Select(cell => $"<a:tc><a:txBody><a:bodyPr/>{Paragraphs(cell)}</a:txBody><a:tcPr/></a:tc>")) + "</a:tr>"))}</a:tbl></a:graphicData></a:graphic></p:graphicFrame>""";

    private static string Tree(string shapes) => $"<p:spTree>{shapes}</p:spTree>";

    // Each slide is its p:cSld content; every slide uses the one layout, which uses the one master; "img{n}" is images[n].
    internal static string WriteDeck(string folder, string layout, string master, byte[][] images, params string[] slides)
    {
        var path = Path.Combine(folder, "글자.pptx");
        using var package = Package.Open(path, FileMode.Create);
        PackagePart Add(string name, byte[] bytes, string type = "application/xml")
        {
            var part = package.CreatePart(new Uri(name, UriKind.Relative), type, CompressionOption.Normal);
            using var stream = part.GetStream();
            stream.Write(bytes);
            return part;
        }
        byte[] Xml(string root, string content) => Encoding.UTF8.GetBytes($"<p:{root} {Namespaces}><p:cSld>{content}</p:cSld></p:{root}>");
        var media = images.Select((bytes, n) => Add($"/ppt/media/image{n}.png", bytes, "image/png")).ToArray();
        var ids = string.Concat(slides.Select((_, n) => $"<p:sldId id=\"{256 + n}\" r:id=\"s{n}\"/>"));
        var presentation = Add("/ppt/presentation.xml", Encoding.UTF8.GetBytes(
            $"""<p:presentation {Namespaces}><p:sldIdLst>{ids}</p:sldIdLst><p:sldSz cx="{SlideWidth}" cy="{SlideHeight}"/></p:presentation>"""));
        package.CreateRelationship(presentation.Uri, TargetMode.Internal, Rel + "/officeDocument");
        var masterPart = Add("/ppt/slideMasters/slideMaster1.xml", Xml("sldMaster", Tree(master)));
        var layoutPart = Add("/ppt/slideLayouts/slideLayout1.xml", Xml("sldLayout", Tree(layout)));
        layoutPart.CreateRelationship(PackUriHelper.GetRelativeUri(layoutPart.Uri, masterPart.Uri), TargetMode.Internal, Rel + "/slideMaster", "m1");
        for (int n = 0; n < slides.Length; n++)
        {
            var slide = Add($"/ppt/slides/slide{n + 1}.xml", Xml("sld", slides[n]));
            presentation.CreateRelationship(PackUriHelper.GetRelativeUri(presentation.Uri, slide.Uri), TargetMode.Internal, Rel + "/slide", $"s{n}");
            slide.CreateRelationship(PackUriHelper.GetRelativeUri(slide.Uri, layoutPart.Uri), TargetMode.Internal, Rel + "/slideLayout", "l1");
            for (int m = 0; m < media.Length; m++)
                slide.CreateRelationship(PackUriHelper.GetRelativeUri(slide.Uri, media[m].Uri), TargetMode.Internal, Rel + "/image", $"img{m}");
        }
        return path;
    }

    // A deck on the plain layout below.
    internal static string WriteDeck(string folder, byte[][] images, params string[] slides) => WriteDeck(folder, Layout, Master, images, slides);

    // One box per line, named "TextBox 1", "TextBox 2"…, top to bottom.
    internal static string TextSlide(params string[] lines) =>
        Tree(string.Concat(lines.Select((line, n) => Box($"TextBox {n + 1}", 600000, 300000 + n * 1000000, 8000000, 600000, line))));

    // Named boxes, one under the other.
    internal static string Slide(params (string Name, string Text)[] boxes) =>
        Tree(string.Concat(boxes.Select((box, n) => Box(box.Name, 600000, 300000 + n * 1700000, 8000000, 600000, box.Text))));

    // images[image] as a score picture with a short text under it.
    internal static string ScoreSlide(int image, string text) =>
        Tree(CaptionScorePictureTests.Image(image, CaptionScorePictureTests.Full, CaptionScorePictureTests.Wide, x: 1524000)
            + Box("남은 글", 600000, 5500000, 3000000, 600000, text));

    // A grey picture without staves (a photo).
    internal static byte[] Png(int width, int height)
    {
        var pixels = new byte[width * height];
        Array.Fill(pixels, (byte)128);
        var encoder = new System.Windows.Media.Imaging.PngBitmapEncoder();
        encoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(
            System.Windows.Media.Imaging.BitmapSource.Create(width, height, 96, 96, System.Windows.Media.PixelFormats.Gray8, null, pixels, width)));
        using var stream = new MemoryStream();
        encoder.Save(stream);
        return stream.ToArray();
    }

    // images[image] as a score picture, alone on the slide.
    internal static string ScoreSlide(int image) =>
        Tree(CaptionScorePictureTests.Image(image, CaptionScorePictureTests.Full, CaptionScorePictureTests.Wide, x: 1524000));

    // The layout's title takes its place from the master; the body placeholder idx 1 is placed by the layout.
    private const string Master = """<p:sp><p:nvSpPr><p:cNvPr id="2" name="Title 1"/><p:cNvSpPr/><p:nvPr><p:ph type="title"/></p:nvPr></p:nvSpPr><p:spPr><a:xfrm><a:off x="838200" y="365125"/><a:ext cx="10515600" cy="1325563"/></a:xfrm></p:spPr></p:sp>""";
    private const string Layout = """<p:sp><p:nvSpPr><p:cNvPr id="2" name="Title 1"/><p:cNvSpPr/><p:nvPr><p:ph type="title"/></p:nvPr></p:nvSpPr><p:spPr/></p:sp><p:sp><p:nvSpPr><p:cNvPr id="3" name="Content 2"/><p:cNvSpPr/><p:nvPr><p:ph idx="1"/></p:nvPr></p:nvSpPr><p:spPr><a:xfrm><a:off x="838200" y="1825625"/><a:ext cx="10515600" cy="4351338"/></a:xfrm></p:spPr></p:sp>""";

    private static CaptionDeck TheDeck(string folder)
    {
        var background = CaptionScorePictureTests.ScorePng();
        var path = WriteDeck(folder, Layout, Master, [background, CaptionScorePictureTests.ScorePng()],
            // 1: title and body (body first in stacking order), the title placed by the master through the layout, the body
            // by the layout; slide number, date and footer; a placeholder placed nowhere goes last.
            Tree(Shape("Content 2", """<p:ph idx="1"/>""", "", false, "본문1", "본문2")
                + Shape("Slide Number", """<p:ph type="sldNum" sz="quarter" idx="12"/>""", At(8610600, 6356350, 2743200, 365125), false, "1")
                + Shape("Footer", """<p:ph type="ftr" sz="quarter" idx="11"/>""", "", false, "바닥글")
                + Shape("Date", """<p:ph type="dt" sz="half" idx="10"/>""", At(838200, 6356350, 2743200, 365125), false, "2026-11-24")
                + Shape("Title 1", """<p:ph type="title"/>""", "", false, "제목")
                + Shape("Subtitle 9", """<p:ph type="subTitle" idx="13"/>""", "", false, "끝")),
            // 2: boxes named per slide; two in one band read left to right.
            Tree(Box("TextBox 7", 6500000, 400000, 4000000, 600000, "오른쪽") + Box("TextBox 3", 600000, 500000, 4000000, 600000, "왼쪽")
                + Box("TextBox 9", 600000, 3000000, 4000000, 600000, "아래")),
            // 3: a group places its children (half height from child y 5000000); a hidden group shows nothing.
            Tree(Box("TextBox 5", 600000, 1500000, 3000000, 400000, "밖")
                + Group(600000, 1000000, 6000000, 2000000, 0, 5000000, 12000000, 4000000,
                    Box("g1", 0, 5000000, 12000000, 600000, "그룹1") + Box("g2", 0, 7000000, 12000000, 600000, "그룹2"))
                + Group(600000, 300000, 6000000, 600000, 0, 0, 6000000, 600000, Box("g3", 0, 0, 6000000, 600000, "숨은 그룹"), hidden: true)),
            // 4: a table (one line per row, cells joined by a space) under a notice; a hidden box.
            Tree(Shape("숨김", "", At(600000, 200000, 4000000, 400000), true, "숨김")
                + Table(600000, 1500000, [["이름"], ["직분"], ["담당", "부서"]], [["홍길동"], [], ["장로"]])
                + Box("광고", 600000, 300000, 8000000, 600000, "광고")),
            // 5: the slide's foot: a small verse button is left out, a tall box and a body line just above it stay;
            // boxes off the slide are left out; blank lines inside a box shrink to one.
            Tree(Box("1절", 600000, 6100000, 1000000, 400000, "1절")
                + Box("큰", 700000, 6100000, 10000000, 700000, "큰 상자")
                + Box("마지막", 600000, 5900000, 10000000, 342900, "마지막 줄")
                + Box("왼쪽 밖", -3000000, 1000000, 2000000, 400000, "왼쪽 밖")
                + Box("아래 밖", 600000, 7000000, 4000000, 400000, "아래 밖")
                + Box("본문", 600000, 300000, 10000000, 2000000, "본문", "", "", "끝줄")),
            // 6: a picture background fill under text.
            """<p:bg><p:bgPr><a:blipFill><a:blip r:embed="img0"/><a:stretch><a:fillRect/></a:stretch></a:blipFill><a:effectLst/></p:bgPr></p:bg>"""
                + Tree(Box("글", 600000, 300000, 8000000, 600000, "이번 주 수요예배는 저녁 일곱 시 반입니다")),
            // 7: a slide-size background picture under text.
            Tree(CaptionScorePictureTests.Image(0, SlideWidth, SlideHeight) + Box("글", 600000, 300000, 8000000, 600000, "성탄 축하 발표회에 온 가족을 초대합니다")),
            // 8: a score picture alone.
            ScoreSlide(1));
        return CaptionDeck.Read(path);
    }

    private static T InFolder<T>(Func<string, T> test)
    {
        var folder = Directory.CreateTempSubdirectory();
        try { return test(folder.FullName); }
        finally { folder.Delete(true); }
    }

    [Fact]
    public void EverySlideStartsWithItsWholeTextReadTopToBottomAndLeftToRight() => StaHelper.RunOnSta(() => InFolder(folder =>
    {
        var deck = TheDeck(folder);
        Assert.All(deck.Slides, slide => Assert.Equal((CaptionDeck.AllTextKey, "슬라이드 전체 글자"), (slide.Boxes[0].Key, slide.Boxes[0].Name)));
        Assert.Equal([
            "제목\n본문1\n본문2\n끝",
            "왼쪽\n오른쪽\n아래",
            "그룹1\n밖\n그룹2",
            "광고\n이름 직분 담당 부서\n홍길동 장로",
            "본문\n\n끝줄\n마지막 줄\n큰 상자",
            "이번 주 수요예배는 저녁 일곱 시 반입니다",
            "성탄 축하 발표회에 온 가족을 초대합니다",
            ""], deck.Slides.Select(slide => slide.Boxes[0].Text));
        // The boxes themselves stay as they were, after the whole text.
        Assert.Contains(deck.Slides[1].Boxes, box => box.Key == "shape:TextBox 3" && box.Text == "왼쪽");
        Assert.Contains(deck.Slides[0].Boxes, box => box.Key == "placeholder:title:0" && box.Text == "제목");
        return 0;
    }));

    [Fact]
    public void ABackgroundUnderTextIsNoPictureSlide() => StaHelper.RunOnSta(() => InFolder(folder =>
    {
        var deck = TheDeck(folder);
        Assert.Equal(new bool?[] { false, false, false, false, false, false, false, true }, deck.Slides.Select(slide => slide.HasPicture));
        return 0;
    }));

    [Fact]
    public void AShownPictureOnASlideWithoutTextMakesAPictureSlideAndAPictureUnderTextDoesNot() => StaHelper.RunOnSta(() => InFolder(folder =>
    {
        var score = CaptionScorePictureTests.ScorePng();
        var path = WriteDeck(folder, [score],
            // 1: the score as two pictures (a035 leaves it to the render), no text box.
            Tree(CaptionScorePictureTests.Image(0, CaptionScorePictureTests.Full, 1600000, crop: """<a:srcRect b="60000"/>""")
                + CaptionScorePictureTests.Image(0, CaptionScorePictureTests.Full, 2400000, y: 2000000, crop: """<a:srcRect t="40000"/>""")),
            // 2: a notice with a small picture beside its text.
            Tree(Box("글", 600000, 300000, 6000000, 600000, "다음 주일은 전교인 체육대회입니다") + CaptionScorePictureTests.Image(0, 2000000, 1500000, x: 8000000, y: 300000)),
            // 3: a picture background fill only.
            """<p:bg><p:bgPr><a:blipFill><a:blip r:embed="img0"/><a:stretch><a:fillRect/></a:stretch></a:blipFill></p:bgPr></p:bg>""" + Tree(""),
            // 4: a slide-size picture alone that a035 leaves to the render (turned), no text.
            Tree(CaptionScorePictureTests.Image(0, SlideWidth, SlideHeight, xfrm: " rot=\"60000\"")),
            // 5: text over a slide-size background picture.
            Tree(CaptionScorePictureTests.Image(0, SlideWidth, SlideHeight) + Box("글", 600000, 300000, 8000000, 600000, "성탄 축하 발표회에 온 가족을 초대합니다")),
            // 6: a hidden picture only.
            Tree(CaptionScorePictureTests.Image(0, CaptionScorePictureTests.Full, CaptionScorePictureTests.Wide, hidden: true)),
            // 7: a short word left over the score pieces (12 letters or fewer).
            Tree(CaptionScorePictureTests.Image(0, CaptionScorePictureTests.Full, 1600000) + CaptionScorePictureTests.Image(0, CaptionScorePictureTests.Full, 2400000, y: 2000000)
                + Box("남은 글", 600000, 4500000, 3000000, 600000, "에게")),
            // 8: the score as the slide's background, a page mark over it (the mark is left out of the whole text).
            """<p:bg><p:bgPr><a:blipFill><a:blip r:embed="img0"/><a:stretch><a:fillRect/></a:stretch></a:blipFill></p:bgPr></p:bg>"""
                + Tree(Box("쪽", 9000000, 300000, 1000000, 400000, "3/8")),
            // 9: thirteen letters over a background fill: a text slide.
            """<p:bg><p:bgPr><a:blipFill><a:blip r:embed="img0"/><a:stretch><a:fillRect/></a:stretch></a:blipFill></p:bgPr></p:bg>"""
                + Tree(Box("글", 600000, 300000, 8000000, 600000, "가나다라마바사 아자차카타파하")),
            // 10: a hymn's long title over its score cut in two pieces.
            Tree(Box("제목", 600000, 100000, 10000000, 500000, "79장 주 하나님 지으신 모든 세계")
                + CaptionScorePictureTests.Image(0, CaptionScorePictureTests.Full, 1600000, y: 700000)
                + CaptionScorePictureTests.Image(0, CaptionScorePictureTests.Full, 2400000, y: 2500000)));
        var deck = CaptionDeck.Read(path);
        // (2: a picture smaller than a background counts whatever the text, as before a036; 5 and 9: long text over a
        // slide-size picture or a picture background is a text slide)
        Assert.Equal(new bool?[] { true, true, true, true, false, false, true, true, false, true }, deck.Slides.Select(slide => slide.HasPicture));
        Assert.Null(deck.Slides[0].Picture); // the render shows it; its lyrics are read from the render
        Assert.Null(deck.Slides[3].Picture);
        Assert.Equal("", deck.Slides[7].Boxes[0].Text);
        return 0;
    }));

    [Fact]
    public void AtTheSlidesFootOnlyALinkOrAShortMarkIsLeftOut() => StaHelper.RunOnSta(() => InFolder(folder =>
    {
        const long foot = 6200000, low = 342900; // 90% down, 5% high
        const string link = """<a:hlinkClick r:id="" action="ppaction://hlinksldjump"/>""";
        // A verse button: a link to a slide, or the current verse's that goes nowhere.
        static string Button(long x, string action, string text) =>
            $"""<p:sp><p:nvSpPr><p:cNvPr id="8" name="절 버튼"><a:hlinkClick r:id="" action="{action}"/></p:cNvPr><p:cNvSpPr/><p:nvPr/></p:nvSpPr><p:spPr>{At(x, foot, 3000000, low)}</p:spPr><p:txBody><a:bodyPr/>{Paragraphs([text])}</p:txBody></p:sp>""";
        // The link on the text only, on the group around it, or on a picture beside it in its group.
        var onText = Box("글 링크", 600000, foot, 3000000, low, "런 링크가 걸린 긴 글자입니다").Replace("<a:r><a:t>", $"<a:r><a:rPr lang=\"ko-KR\">{link}</a:rPr><a:t>");
        var onGroup = $"""<p:grpSp><p:nvGrpSpPr><p:cNvPr id="3" name="g">{link}</p:cNvPr><p:cNvGrpSpPr/><p:nvPr/></p:nvGrpSpPr><p:grpSpPr/>{Box("그룹 글", 4000000, foot, 3000000, low, "그룹 링크가 걸린 긴 글자")}</p:grpSp>""";
        var besidePicture = $"""<p:grpSp><p:nvGrpSpPr><p:cNvPr id="3" name="g2"/><p:cNvGrpSpPr/><p:nvPr/></p:nvGrpSpPr><p:grpSpPr/><p:pic><p:nvPicPr><p:cNvPr id="7" name="btn">{link}</p:cNvPr><p:cNvPicPr/><p:nvPr/></p:nvPicPr><p:blipFill/><p:spPr>{At(8000000, foot, 300000, low)}</p:spPr></p:pic>{Box("옆 글", 8400000, foot, 3000000, low, "형제 링크 옆의 긴 글자")}</p:grpSp>""";
        var path = WriteDeck(folder, [],
            Tree(Box("본문", 600000, 300000, 8000000, 600000, "재정 보고")
                + Box("잔액", 600000, foot, 6000000, low, "잔 액 1,234,000 원")
                + Button(600000, "ppaction://hlinksldjump", "1.세상을 구원하기 위해")
                + Button(4000000, "ppaction://noaction", "3.사람의 마음속엔 온 가지 소원")
                + Box("쪽", 9000000, foot, 1000000, low, "2/8")),
            Tree(Box("본문", 600000, 300000, 8000000, 600000, "본문") + onText + onGroup + besidePicture));
        var deck = CaptionDeck.Read(path);
        Assert.Equal("재정 보고\n잔 액 1,234,000 원", deck.Slides[0].Boxes[0].Text);
        Assert.Equal("본문", deck.Slides[1].Boxes[0].Text);
        return 0;
    }));

    [Fact]
    public void ANumberMarkBoxIsNoPartOfTheWholeTextButAnAmountOrAHymnNumberIs() => StaHelper.RunOnSta(() => InFolder(folder =>
    {
        var path = WriteDeck(folder, [],
            Tree(string.Concat(new[] { "1", "2.", "(3)", "4)", "12/16", "3-4" }.Select((mark, n) => Box($"번호 {n}", 600000 + n * 1500000, 300000, 1000000, 400000, mark)))
                + Box("가사", 600000, 1500000, 8000000, 600000, "주 하나님 지으신 모든 세계")
                + Box("금액", 600000, 3000000, 4000000, 600000, "1,234,000")
                + Box("장", 600000, 4000000, 4000000, 600000, "123")
                // digits of other scripts are no mark (full-width one, Arabic-Indic three)
                + Box("전각", 600000, 5000000, 1000000, 400000, "１") + Box("아랍", 2000000, 5000000, 1000000, 400000, "٣")));
        Assert.Equal("주 하나님 지으신 모든 세계\n1,234,000\n123\n１\n٣", CaptionDeck.Read(path).Slides[0].Boxes[0].Text);
        // The same in an old deck's boxes.
        var old = CaptionDeck.WithAllText(new CaptionSlide([new("shape:1", "1", "2/4"), new("shape:2", "2", "가사 한 줄")]));
        Assert.Equal("가사 한 줄", old.Boxes[0].Text);
        return 0;
    }));

    [Fact]
    public void APlaceholderTakesItsPlaceAsPowerPointInheritsIt() => StaHelper.RunOnSta(() => InFolder(folder =>
    {
        static string Placeholder(string ph, string xfrm, string text = "") =>
            $"""<p:sp><p:nvSpPr><p:cNvPr id="5" name="ph"/><p:cNvSpPr/><p:nvPr>{ph}</p:nvPr></p:nvSpPr><p:spPr>{xfrm}</p:spPr>{(text.Length > 0 ? $"<p:txBody><a:bodyPr/>{Paragraphs([text])}</p:txBody>" : "")}</p:sp>""";
        // A: the layout's idx 2 has no frame; on the master idx 2 is the date at the foot, but an obj placeholder takes the
        // master's body (by type), so the short body text is not taken for a foot mark and left out.
        var masterA = Placeholder("""<p:ph type="title"/>""", At(838200, 365125, 10515600, 1325563))
            + Placeholder("""<p:ph type="body" idx="1"/>""", At(838200, 1825625, 10515600, 4351338))
            + Placeholder("""<p:ph type="dt" sz="half" idx="2"/>""", At(838200, 6356350, 2743200, 365125));
        var a = WriteDeck(Directory.CreateDirectory(Path.Combine(folder, "a")).FullName,
            Placeholder("""<p:ph idx="2"/>""", "") + Placeholder("""<p:ph type="title"/>""", ""), masterA, [],
            Tree(Placeholder("""<p:ph idx="2"/>""", "", "아멘") + Placeholder("""<p:ph type="title"/>""", "", "제목")));
        Assert.Equal("제목\n아멘", CaptionDeck.Read(a).Slides[0].Boxes[0].Text);
        // A2: a centered title and a subtitle without frames on the layout take the master's title and body.
        var a2 = WriteDeck(Directory.CreateDirectory(Path.Combine(folder, "a2")).FullName,
            Placeholder("""<p:ph type="ctrTitle"/>""", "") + Placeholder("""<p:ph type="subTitle" idx="1"/>""", ""), masterA, [],
            Tree(Placeholder("""<p:ph type="subTitle" idx="1"/>""", "", "부제") + Placeholder("""<p:ph type="ctrTitle"/>""", "", "큰제목")));
        Assert.Equal("큰제목\n부제", CaptionDeck.Read(a2).Slides[0].Boxes[0].Text);
        // A layout that cannot be read: the placeholder goes last and the deck is still read.
        var broken = WriteDeck(Directory.CreateDirectory(Path.Combine(folder, "broken")).FullName, "<p:sp>", masterA, [],
            Tree(Placeholder("""<p:ph type="title"/>""", "", "제목") + Box("본문", 600000, 3000000, 8000000, 600000, "본문")));
        Assert.Equal("본문\n제목", CaptionDeck.Read(broken).Slides[0].Boxes[0].Text);
        return 0;
    }));

    [Fact]
    public void AShapeInAHiddenGroupAndTheFallbackCopyOfAShapeAreLeftOut() => StaHelper.RunOnSta(() => InFolder(folder =>
    {
        var frameless = """<p:sp><p:nvSpPr><p:cNvPr id="5" name="x"/><p:cNvSpPr/><p:nvPr/></p:nvSpPr><p:spPr/><p:txBody><a:bodyPr/><a:p><a:r><a:t>숨긴 그룹 안 위치 없는 글</a:t></a:r></a:p></p:txBody></p:sp>""";
        var hidden = $"""<p:grpSp><p:nvGrpSpPr><p:cNvPr id="3" name="g" hidden="1"/><p:cNvGrpSpPr/><p:nvPr/></p:nvGrpSpPr><p:grpSpPr/>{frameless}</p:grpSp>""";
        var formula = Box("수식", 600000, 1500000, 8000000, 600000, "수식 글");
        var twice = $"""<mc:AlternateContent><mc:Choice Requires="a14">{formula}</mc:Choice><mc:Fallback>{formula}</mc:Fallback></mc:AlternateContent>""";
        var path = WriteDeck(folder, [], Tree(Box("본문", 600000, 300000, 8000000, 600000, "본문") + hidden + twice));
        Assert.Equal("본문\n수식 글", CaptionDeck.Read(path).Slides[0].Boxes[0].Text);
        return 0;
    }));

    [Fact]
    public void LyricsReadUnderStavesBecomeAScoreSlidesWholeTextWhenItSaysLittle()
    {
        using var session = new CaptionSession(new LiveSessionService().ForCaptions());
        session.Prepare(new CaptionDeck("deck.pptx", [
            new([new(CaptionDeck.AllTextKey, "슬라이드 전체 글자", "에게")], true), // a word left over the score (a page mark never reaches the whole text)
            new([new(CaptionDeck.AllTextKey, "슬라이드 전체 글자", "사진")], true),
            new([new(CaptionDeck.AllTextKey, "슬라이드 전체 글자", "이번 주 수요예배는 저녁 일곱 시 반입니다")], true)]));
        session.AddRecognized(1, "주 하나님 지으신 모든 세계", staves: true);
        session.AddRecognized(2, "사진 속 간판 글씨"); // no staves: a photo
        session.AddRecognized(3, "주 하나님", staves: true); // a text slide says more than 12 letters
        Assert.Equal("주 하나님 지으신 모든 세계", session.TextFor(1, CaptionDeck.AllTextKey));
        Assert.Equal("사진", session.TextFor(2, CaptionDeck.AllTextKey));
        Assert.Equal("이번 주 수요예배는 저녁 일곱 시 반입니다", session.TextFor(3, CaptionDeck.AllTextKey));
        Assert.Equal("주 하나님 지으신 모든 세계", session.TextFor(1, CaptionSession.RecognizedKey));
    }

    [Fact]
    public void FollowingTheRecognizedLyricsATextSlideGivesItsWholeText()
    {
        using var session = new CaptionSession(new LiveSessionService().ForCaptions());
        session.Prepare(new CaptionDeck("deck.pptx", [
            new([new(CaptionDeck.AllTextKey, "슬라이드 전체 글자", "")], true),
            new([new(CaptionDeck.AllTextKey, "슬라이드 전체 글자", "광고 말씀")], false)]));
        session.AddRecognized(1, "주 하나님");
        session.AddRecognized(2, ""); // nothing read on a slide without a picture
        session.SetVisible(true);
        session.Follow(1, CaptionSession.RecognizedKey);
        Assert.Equal("주 하나님", session.PublicText);
        session.MoveToSlide(2);
        Assert.Equal("광고 말씀", session.PublicText);
    }

    [Fact]
    public void OnlyTextThatReadsAsKoreanLyricsReplacesAScoreSlidesWholeText()
    {
        using var session = new CaptionSession(new LiveSessionService().ForCaptions());
        CaptionSlide Empty() => new([new(CaptionDeck.AllTextKey, "슬라이드 전체 글자", "")], true);
        session.Prepare(new CaptionDeck("deck.pptx", [Empty(), Empty(), Empty(), Empty(), Empty()]));
        session.AddRecognized(1, "하*날1까 `", staves: true);                 // three syllables (a photo misread)
        session.AddRecognized(2, "주 하나님 love you all", staves: true);   // four syllables, a quarter of the letters
        session.AddRecognized(3, "주 하나님 지으신", staves: true);          // lyrics
        session.AddRecognized(4, "주 하나님 지으신");                         // not under staves
        session.AddRecognized(5, "주하나님 abc", staves: true);              // four syllables of seven letters: 57%
        Assert.Equal(["", "", "주 하나님 지으신", "", ""], session.Deck!.Slides.Select(slide => slide.Boxes[0].Text));
        Assert.Equal("하*날1까 `", session.TextFor(1, CaptionSession.RecognizedKey)); // the recognized box keeps what was read
        // Only letters count: digits, punctuation and note signs read from a score do not.
        Assert.True(CaptionSession.Lyrical("1. 주 하나님 (2x)!!"));        // 4 syllables of 5 letters
        Assert.False(CaptionSession.Lyrical("1. 주 J 하 | 나 o 님 (2x)")); // 4 of 7 letters: 57%
        Assert.False(CaptionSession.Lyrical("하*날1까 `"));                 // 3 syllables
    }

    [Fact]
    public void RunsOfSpacesInsideALineOfTheWholeTextBecomeOneSpace() => StaHelper.RunOnSta(() => InFolder(folder =>
    {
        var path = WriteDeck(folder, [],
            Tree(Box("가사", 600000, 300000, 10000000, 1200000, "겟 세    마  네  동  산", "  주님의\t\t땀  ")));
        Assert.Equal("겟 세 마 네 동 산\n주님의 땀", CaptionDeck.Read(path).Slides[0].Boxes[0].Text);
        var old = CaptionDeck.WithAllText(new CaptionSlide([new("shape:A", "A", "겟 세    마  네\n\t 동 산  ")]));
        Assert.Equal("겟 세 마 네\n동 산", old.Boxes[0].Text);
        // No-break and full-width spaces too.
        var wide = CaptionDeck.WithAllText(new CaptionSlide([new("shape:A", "A", "성령의  불길　타오르네 ")]));
        Assert.Equal("성령의 불길 타오르네", wide.Boxes[0].Text);
        return 0;
    }));

    [Fact]
    public void ABoxMissingOnTheNextSlideGivesThatSlidesWholeTextInsteadOfNothing() => StaHelper.RunOnSta(() => InFolder(folder =>
    {
        using var session = new CaptionSession(new LiveSessionService().ForCaptions());
        session.Prepare(TheDeck(folder));
        session.SetVisible(true);
        session.Follow(2, "shape:TextBox 3");
        Assert.Equal("왼쪽", session.PublicText);
        session.MoveToSlide(3); // no "TextBox 3" here
        Assert.Equal("그룹1\n밖\n그룹2", session.PublicText);
        session.Overrides[3] = "고친 문구"; // a typed caption still comes first
        Assert.Equal("고친 문구", session.TextFor(3, "shape:TextBox 3"));
        Assert.Equal("", session.TextFor(9, "shape:TextBox 3"));
        return 0;
    }));

    [Fact]
    public void TheOldDecksWholeTextJoinsItsBoxesInTheGivenOrder()
    {
        var slide = CaptionDeck.WithAllText(new CaptionSlide([new("shape:A", "A", "첫 줄\n\n\n\n둘째 줄"), new("shape:B", "B", "  \n셋째 줄"), new("shape:C", "C", "")]));
        Assert.Equal(new CaptionBox(CaptionDeck.AllTextKey, "슬라이드 전체 글자", "첫 줄\n\n둘째 줄\n\n셋째 줄"), slide.Boxes[0]);
        Assert.Equal(["shape:A", "shape:B", "shape:C"], slide.Boxes.Skip(1).Select(box => box.Key));
        Assert.Null(slide.HasPicture); // not known for .ppt
        Assert.Equal("", CaptionDeck.WithAllText(new CaptionSlide([])).Boxes.Single().Text);
    }

    [Fact]
    public void TheDisplayChoiceIsKeptAndAnOldOrUnknownOneReadsAsAuto()
    {
        var directory = Directory.CreateTempSubdirectory();
        try
        {
            var path = Path.Combine(directory.FullName, "caption-style.json");
            File.WriteAllText(path, """{"Color":"#FFE14D","Box":false,"Lines":2}"""); // written before a036
            using (var old = new CaptionSession(new LiveSessionService().ForCaptions(), null, path))
            {
                Assert.Equal(("auto", "#FFE14D", false, 2), (old.Style.Display, old.Style.Color, old.Style.Box, old.Style.Lines));
                old.Display = "score";
            }
            Assert.Equal("score", JsonDocument.Parse(File.ReadAllText(path)).RootElement.GetProperty("Display").GetString());
            using (var reopened = new CaptionSession(new LiveSessionService().ForCaptions(), null, path))
                Assert.Equal(("score", "#FFE14D", 2), (reopened.Display, reopened.Style.Color, reopened.Style.Lines));
            File.WriteAllText(path, """{"Color":"#FFE14D","Box":true,"Display":"fancy"}""");
            using (var unknown = new CaptionSession(new LiveSessionService().ForCaptions(), null, path))
                Assert.Equal("auto", unknown.Display);
            using var fresh = new CaptionSession(new LiveSessionService().ForCaptions());
            Assert.Equal("auto", fresh.Display);
            fresh.Display = "nonsense";
            Assert.Equal("auto", fresh.Display);
            foreach (var display in new[] { "text", "score", "scoretext", "auto" })
            {
                fresh.Display = display;
                Assert.Equal(display, fresh.Display);
            }
        }
        finally { directory.Delete(true); }
    }
}
