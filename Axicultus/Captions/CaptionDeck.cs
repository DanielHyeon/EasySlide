using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Threading;
using System.Xml;
using System.Xml.Linq;

namespace Axicultus.Wpf.Captions;

public sealed record CaptionBox(string Key, string Name, string Text);
public sealed record CaptionSlide(CaptionBox[] Boxes);
public sealed record CaptionDeck(string Path, CaptionSlide[] Slides)
{
    // Resolve OPC relationships in presentation order. ZIP entry names do not encode slide order.
    public static CaptionDeck Read(string path, CancellationToken cancellation = default)
    {
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
        using var package = Package.Open(stream, FileMode.Open, FileAccess.Read);
        var root = package.GetRelationships().Single(r => r.RelationshipType.EndsWith("/officeDocument", StringComparison.Ordinal));
        var presentation = package.GetPart(PackUriHelper.ResolvePartUri(new Uri("/", UriKind.Relative), root.TargetUri));
        var slides = new List<CaptionSlide>();
        var xml = ReadXml(presentation);
        var slideList = xml.Root?.Elements().FirstOrDefault(e => e.Name.LocalName == "sldIdLst");
        foreach (var slideId in slideList?.Elements() ?? [])
        {
            cancellation.ThrowIfCancellationRequested();
            var id = slideId.Attributes().Single(a => a.Name.LocalName == "id" && a.Name.NamespaceName.Length > 0).Value;
            var relation = presentation.GetRelationship(id);
            var part = package.GetPart(PackUriHelper.ResolvePartUri(presentation.Uri, relation.TargetUri));
            var boxes = new List<CaptionBox>();
            var shapeXml = ReadXml(part);
            var presentationNs = shapeXml.Root!.Name.Namespace;
            foreach (var shape in shapeXml.Descendants(presentationNs + "sp"))
            {
                var body = shape.Element(presentationNs + "txBody");
                if (body is null) continue;
                var text = string.Join("\n", body.Elements().Where(e => e.Name.LocalName == "p")
                    .Select(p => string.Concat(p.Descendants().Where(e => e.Name.LocalName is "t" or "br")
                        .Select(e => e.Name.LocalName == "br" ? "\n" : e.Value)))).Trim();
                if (text.Length == 0) continue;
                var props = shape.Element(presentationNs + "nvSpPr");
                var name = (string?)props?.Element(presentationNs + "cNvPr")?.Attribute("name") ?? $"상자 {boxes.Count + 1}";
                var placeholder = props?.Descendants(presentationNs + "ph").FirstOrDefault();
                var key = placeholder is null ? "shape:" + name :
                    $"placeholder:{(string?)placeholder.Attribute("type") ?? "obj"}:{(string?)placeholder.Attribute("idx") ?? "0"}";
                boxes.Add(new(key, name, text));
            }
            slides.Add(new([.. boxes]));
        }
        if (slides.Count == 0) throw new InvalidDataException("슬라이드를 찾을 수 없습니다.");
        return new(System.IO.Path.GetFullPath(path), [.. slides]);
    }

    private static XDocument ReadXml(PackagePart part)
    {
        using var stream = part.GetStream(FileMode.Open, FileAccess.Read);
        using var reader = XmlReader.Create(stream, new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit, MaxCharactersInDocument = 16_000_000 });
        return XDocument.Load(reader);
    }
}
