using System.Buffers.Binary;
using System.IO.Compression;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Linq;

namespace Axicultus.ScoreEditor;

public static class ScoreSlides
{
    private static readonly XNamespace Presentation = "http://schemas.openxmlformats.org/presentationml/2006/main";
    private static readonly XNamespace Rel = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";

    public static void Export(MelodyScore score, string path)
        => WritePages(StaffDrawing.Lines(score).Chunk(2).Select(lines =>
            new SlideImage(1920, 1080, output => Render(score.Title, lines, output))), path, default);

    private sealed record SlideImage(int Width, int Height, Action<Stream> Write);

    internal static void ExportPages(IEnumerable<string> paths, string path, CancellationToken token)
        => WritePages(paths.Select(file =>
        {
            using var input = File.OpenRead(file);
            Span<byte> header = stackalloc byte[24];
            input.ReadExactly(header);
            int width = BinaryPrimitives.ReadInt32BigEndian(header[16..]);
            int height = BinaryPrimitives.ReadInt32BigEndian(header[20..]);
            if (width <= 0 || height <= 0) throw new InvalidDataException("잘못된 페이지 크기입니다.");
            return new SlideImage(width, height, output => { using var png = File.OpenRead(file); png.CopyTo(output); });
        }), path, token);

    private static void WritePages(IEnumerable<SlideImage> pages, string path, CancellationToken token)
    {
        var temp = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            using (var template = typeof(ScoreSlides).Assembly.GetManifestResourceStream("ScoreSlideTemplate")!)
            using (var output = File.Create(temp)) template.CopyTo(output);
            using (var zip = ZipFile.Open(temp, ZipArchiveMode.Update))
            {
                var presentation = Read(zip, "ppt/presentation.xml");
                var relations = Read(zip, "ppt/_rels/presentation.xml.rels");
                var types = Read(zip, "[Content_Types].xml");
                var slide = Read(zip, "ppt/slides/slide1.xml");
                var slideRelations = Read(zip, "ppt/slides/_rels/slide1.xml.rels");
                var slideList = presentation.Root!.Element(Presentation + "sldIdLst")!;
                int number = 0;
                var size = presentation.Root.Element(Presentation + "sldSz")!;
                int width = (int)size.Attribute("cx")!, height = (int)size.Attribute("cy")!;
                XNamespace drawing = "http://schemas.openxmlformats.org/drawingml/2006/main";
                var transform = slide.Descendants(Presentation + "pic").Single().Element(Presentation + "spPr")!.Element(drawing + "xfrm")!;
                foreach (var page in pages)
                {
                    token.ThrowIfCancellationRequested();
                    number++;
                    if (number > 1)
                    {
                        string id = $"scoreSlide{number}";
                        slideList.Add(new XElement(Presentation + "sldId", new XAttribute("id", 255 + number), new XAttribute(Rel + "id", id)));
                        relations.Root!.Add(new XElement(relations.Root.Name.Namespace + "Relationship",
                            new XAttribute("Id", id), new XAttribute("Type", Rel.NamespaceName + "/slide"), new XAttribute("Target", $"slides/slide{number}.xml")));
                        types.Root!.Add(new XElement(types.Root.Name.Namespace + "Override", new XAttribute("PartName", $"/ppt/slides/slide{number}.xml"),
                            new XAttribute("ContentType", "application/vnd.openxmlformats-officedocument.presentationml.slide+xml")));
                        var imageRelation = slideRelations.Root!.Elements().Single(e => ((string?)e.Attribute("Type"))!.EndsWith("/image", StringComparison.Ordinal));
                        imageRelation.SetAttributeValue("Target", $"../media/image{number}.png");
                        Write(zip, $"ppt/slides/_rels/slide{number}.xml.rels", slideRelations);
                    }
                    double scale = Math.Min((double)width / page.Width, (double)height / page.Height);
                    int cx = (int)Math.Round(page.Width * scale), cy = (int)Math.Round(page.Height * scale);
                    transform.Element(drawing + "off")!.SetAttributeValue("x", (width - cx) / 2);
                    transform.Element(drawing + "off")!.SetAttributeValue("y", (height - cy) / 2);
                    transform.Element(drawing + "ext")!.SetAttributeValue("cx", cx);
                    transform.Element(drawing + "ext")!.SetAttributeValue("cy", cy);
                    Write(zip, $"ppt/slides/slide{number}.xml", slide);
                    string imagePath = $"ppt/media/image{number}.png";
                    zip.GetEntry(imagePath)?.Delete();
                    using var image = zip.CreateEntry(imagePath, CompressionLevel.NoCompression).Open();
                    page.Write(image);
                }
                if (number == 0) throw new InvalidDataException("출력할 악보 페이지가 없습니다.");
                Write(zip, "ppt/presentation.xml", presentation);
                Write(zip, "ppt/_rels/presentation.xml.rels", relations);
                Write(zip, "[Content_Types].xml", types);
            }
            token.ThrowIfCancellationRequested();
            File.Move(temp, path, true);
        }
        finally { File.Delete(temp); }
    }

    private static void Render(string title, StaffLine[] lines, Stream output)
    {
        var visual = new DrawingVisual();
        using (var dc = visual.RenderOpen())
        {
            dc.DrawRectangle(Brushes.White, null, new Rect(0, 0, 960, 540));
            StaffDrawing.Label(dc, title, 30, 15, 29, 900);
            for (int i = 0; i < lines.Length; i++)
            {
                dc.PushTransform(new TranslateTransform(24, 75 + i * 215));
                StaffDrawing.Draw(dc, lines[i], 912);
                dc.Pop();
            }
        }
        var bitmap = new RenderTargetBitmap(1920, 1080, 192, 192, PixelFormats.Pbgra32);
        bitmap.Render(visual);
        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(bitmap));
        encoder.Save(output);
    }

    private static XDocument Read(ZipArchive zip, string name)
    {
        using var stream = zip.GetEntry(name)!.Open();
        return XDocument.Load(stream);
    }
    private static void Write(ZipArchive zip, string name, XDocument xml)
    {
        zip.GetEntry(name)?.Delete();
        using var stream = zip.CreateEntry(name).Open();
        xml.Save(stream);
    }
}
