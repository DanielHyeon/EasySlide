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
                foreach (var lines in StaffDrawing.Lines(score).Chunk(2))
                {
                    number++;
                    if (number > 1)
                    {
                        string id = $"scoreSlide{number}";
                        slideList.Add(new XElement(Presentation + "sldId", new XAttribute("id", 255 + number), new XAttribute(Rel + "id", id)));
                        relations.Root!.Add(new XElement(relations.Root.Name.Namespace + "Relationship",
                            new XAttribute("Id", id), new XAttribute("Type", Rel.NamespaceName + "/slide"), new XAttribute("Target", $"slides/slide{number}.xml")));
                        types.Root!.Add(new XElement(types.Root.Name.Namespace + "Override", new XAttribute("PartName", $"/ppt/slides/slide{number}.xml"),
                            new XAttribute("ContentType", "application/vnd.openxmlformats-officedocument.presentationml.slide+xml")));
                        Write(zip, $"ppt/slides/slide{number}.xml", slide);
                        var imageRelation = slideRelations.Root!.Elements().Single(e => ((string?)e.Attribute("Type"))!.EndsWith("/image", StringComparison.Ordinal));
                        imageRelation.SetAttributeValue("Target", $"../media/image{number}.png");
                        Write(zip, $"ppt/slides/_rels/slide{number}.xml.rels", slideRelations);
                    }
                    string imagePath = $"ppt/media/image{number}.png";
                    zip.GetEntry(imagePath)?.Delete();
                    using var image = zip.CreateEntry(imagePath, CompressionLevel.NoCompression).Open();
                    Render(score.Title, lines, image);
                }
                Write(zip, "ppt/presentation.xml", presentation);
                Write(zip, "ppt/_rels/presentation.xml.rels", relations);
                Write(zip, "[Content_Types].xml", types);
            }
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
