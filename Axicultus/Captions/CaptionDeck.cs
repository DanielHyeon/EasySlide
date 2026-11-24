using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml;
using System.Xml.Linq;

namespace Axicultus.Wpf.Captions;

public sealed record CaptionBox(string Key, string Name, string Text);
/// <summary>A picture inside the deck (its package part) and its a:srcRect crop: the share cut off each side, 0..1.</summary>
public sealed record CaptionPicture(string Part, double CropLeft, double CropTop, double CropRight, double CropBottom);
/// <summary>
/// HasPicture: the slide holds a picture (a score, lyrics as an image); null when not known (.ppt).
/// Picture: the slide's content picture (a hymn's score apart from the background photo and the verse buttons), if any.
/// </summary>
public sealed record CaptionSlide(CaptionBox[] Boxes, bool? HasPicture = null, CaptionPicture? Picture = null);
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
        var size = xml.Root?.Elements().FirstOrDefault(e => e.Name.LocalName == "sldSz");
        double slideArea = Number(size, "cx") * Number(size, "cy");
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
            slides.Add(new([.. boxes], shapeXml.Descendants().Any(e => e.Name.LocalName == "blip"), ContentPicture(part, shapeXml, slideArea)));
        }
        if (slides.Count == 0) throw new InvalidDataException("슬라이드를 찾을 수 없습니다.");
        return new(System.IO.Path.GetFullPath(path), [.. slides]);
    }

    /// <summary>The longest side a picture is decoded at; a larger one is scaled down while decoding.</summary>
    internal const int MaxPictureSide = 2400;

    // The one picture that shows the slide's content (a hymn's score), or null when there is not exactly one:
    // pictures are taken in stacking order (later is on top) with their rectangles on the slide (groups move and scale
    // their children). Candidates are embedded, not hidden, not rotated or flipped and at least 10% of the slide.
    // A picture 95% covered by any picture above it does not show; a candidate of 80% or more is the background when
    // another candidate is left. Two or more (a score cut in pieces, a photo beside it) leave the slide to the render,
    // and so does a text box, a filled shape (its own fill or its style's), a line, a table or other object frame, or a
    // picture patch laid over the picture (other verses, corrected lyrics,
    // chords) or text in a placeholder above it whose place the slide does not give; slide links (verse buttons) do not count.
    private static CaptionPicture? ContentPicture(PackagePart slide, XDocument xml, double slideArea)
    {
        if (slideArea <= 0) return null;
        static XElement? Child(XElement? parent, string name) => parent?.Elements().FirstOrDefault(e => e.Name.LocalName == name);
        static bool Turned(XElement xfrm) => Number(xfrm, "rot") != 0 || (string?)xfrm.Attribute("flipH") is "1" or "true" || (string?)xfrm.Attribute("flipV") is "1" or "true";
        static bool Hidden(XElement? properties) => (string?)Child(properties, "cNvPr")?.Attribute("hidden") is "1" or "true";
        var p = xml.Root!.Name.Namespace;
        // Every shown picture and shape in stacking order: its rectangle on the slide, the picture it could be, and
        // whether it shows something over a picture (a shape with text or a fill, a picture that is no button).
        var shown = new List<(Rect Area, bool Picture, CaptionPicture? Candidate, bool Overlay)>();
        static bool Lettered(XElement shape) => Child(shape, "txBody")?.Descendants().Any(e => e.Name.LocalName == "t" && !string.IsNullOrWhiteSpace(e.Value)) == true;
        // (a line or connector, a table, chart or other object frame lies over the picture as a shape does)
        foreach (var shape in xml.Descendants().Where(e => e.Name == p + "pic" || e.Name == p + "sp" || e.Name == p + "cxnSp" || e.Name == p + "graphicFrame"))
        {
            bool picture = shape.Name == p + "pic", objectFrame = shape.Name == p + "graphicFrame", line = shape.Name == p + "cxnSp";
            var properties = Child(shape, "spPr");
            var names = Child(shape, picture ? "nvPicPr" : objectFrame ? "nvGraphicFramePr" : line ? "nvCxnSpPr" : "nvSpPr");
            if (Hidden(names)) continue;
            if ((objectFrame ? Child(shape, "xfrm") : Child(properties, "xfrm")) is not { } xfrm)
            {
                // A placeholder takes its place from the layout, not known here: its text may lie over the picture.
                if (!picture && Lettered(shape) && !shape.Ancestors().Any(e => e.Name.LocalName == "grpSp")) shown.Add((Rect.Empty, false, null, true));
                continue;
            }
            double x = Number(Child(xfrm, "off"), "x"), y = Number(Child(xfrm, "off"), "y");
            double width = Number(Child(xfrm, "ext"), "cx"), height = Number(Child(xfrm, "ext"), "cy");
            bool turned = Turned(xfrm), hidden = false;
            foreach (var group in shape.Ancestors().Where(e => e.Name.LocalName == "grpSp"))
            {
                hidden |= Hidden(Child(group, "nvGrpSpPr"));
                if (Child(Child(group, "grpSpPr"), "xfrm") is not { } frame) continue;
                turned |= Turned(frame);
                double childWidth = Number(Child(frame, "chExt"), "cx"), childHeight = Number(Child(frame, "chExt"), "cy");
                double scaleX = childWidth > 0 ? Number(Child(frame, "ext"), "cx") / childWidth : 1;
                double scaleY = childHeight > 0 ? Number(Child(frame, "ext"), "cy") / childHeight : 1;
                x = Number(Child(frame, "off"), "x") + (x - Number(Child(frame, "chOff"), "x")) * scaleX;
                y = Number(Child(frame, "off"), "y") + (y - Number(Child(frame, "chOff"), "y")) * scaleY;
                width *= scaleX;
                height *= scaleY;
            }
            if (hidden) continue;
            var area = new Rect(x, y, Math.Max(0, width), Math.Max(0, height));
            // A slide link (a verse button: a picture and a number, in a group or alone) is not part of what is sung:
            // the hymn decks put their buttons on the score picture's corner.
            static bool Linked(XElement? names) => Child(names, "cNvPr")?.Elements().Any(e => e.Name.LocalName == "hlinkClick") == true;
            bool button = Linked(names)
                || shape.Ancestors().Any(g => g.Name.LocalName == "grpSp" && Linked(Child(g, "nvGrpSpPr")))
                || (shape.Parent is { Name.LocalName: "grpSp" } parent && parent.Descendants().Any(e => e.Name.LocalName == "hlinkClick"));
            if (objectFrame || line)
            {
                // (a straight line has no height or no width: it counts by its length)
                shown.Add((line ? new Rect(area.X, area.Y, Math.Max(1, area.Width), Math.Max(1, area.Height)) : area, false, null, !button));
                continue;
            }
            if (!picture)
            {
                bool text = Lettered(shape);
                // (a fill of its own, or the theme's fill its style names, unless it says it has none)
                bool filled = properties?.Elements().Any(e => e.Name.LocalName is "solidFill" or "blipFill" or "gradFill" or "pattFill") == true
                    || properties?.Elements().Any(e => e.Name.LocalName == "noFill") != true
                        && Number(Child(Child(shape, "style"), "fillRef"), "idx") > 0;
                shown.Add((area, false, null, !button && (text || filled)));
                continue;
            }
            var fill = Child(shape, "blipFill");
            var embed = Child(fill, "blip")?.Attributes().FirstOrDefault(a => a.Name.LocalName == "embed")?.Value;
            CaptionPicture? candidate = null;
            if (!turned && width * height >= 0.10 * slideArea && embed != null && slide.RelationshipExists(embed)
                && slide.GetRelationship(embed) is { TargetMode: TargetMode.Internal } relation
                && Raster(slide.Package, PackUriHelper.ResolvePartUri(slide.Uri, relation.TargetUri)))
            {
                var crop = Child(fill, "srcRect");
                double Share(string side) => Math.Clamp(Number(crop, side) / 100_000, 0, 1);
                candidate = new(PackUriHelper.ResolvePartUri(slide.Uri, relation.TargetUri).OriginalString, Share("l"), Share("t"), Share("r"), Share("b"));
            }
            shown.Add((area, true, candidate, !button)); // a picture patch laid on the score (a corrected word) shows too
        }
        static double Overlap(Rect a, Rect b) =>
            Math.Max(0, Math.Min(a.Right, b.Right) - Math.Max(a.Left, b.Left)) * Math.Max(0, Math.Min(a.Bottom, b.Bottom) - Math.Max(a.Top, b.Top));
        static double Size(Rect r) => r.Width * r.Height;
        var left = shown.Select((s, i) => (s.Area, s.Candidate, Index: i)).Where(s => s.Candidate != null &&
            !shown.Skip(s.Index + 1).Any(above => above.Picture && Overlap(above.Area, s.Area) >= 0.95 * Size(s.Area))).ToList();
        if (left.Count > 1) left.RemoveAll(s => Size(s.Area) >= 0.80 * slideArea);
        if (left.Count != 1) return null;
        var chosen = left[0];
        // A shape or picture above it, mostly (half or more of it) inside it, changes what the slide shows: the render shows that.
        bool covered = shown.Skip(chosen.Index + 1).Any(above => above.Overlay && (above.Area.IsEmpty
            || Size(above.Area) > 0 && Overlap(above.Area, chosen.Area) >= 0.5 * Size(above.Area)));
        return covered ? null : chosen.Candidate;
    }


    // Only a picture the decoder reads (PNG, JPEG, GIF, BMP, TIFF) can be a content picture; a drawing (EMF, WMF) is
    // left to the render, without trying to read it.
    private static bool Raster(Package package, Uri part) =>
        package.PartExists(part) && package.GetPart(part).ContentType.ToLowerInvariant() is
            "image/png" or "image/jpeg" or "image/jpg" or "image/pjpeg" or "image/gif" or "image/bmp" or "image/x-bmp" or "image/tiff";

    private static double Number(XElement? element, string attribute) =>
        double.TryParse((string?)element?.Attribute(attribute), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var value) ? value : 0;

    /// <summary>
    /// The picture as the slide shows it (its crop applied) over white, frozen, its longer side at most
    /// <see cref="MaxPictureSide"/>; null when it cannot be read for any reason (failed is told why).
    /// The deck is opened again read-only, shared with PowerPoint and the show.
    /// </summary>
    public static BitmapSource? ReadPicture(string deckPath, CaptionPicture picture, Action<Exception>? failed = null)
    {
        try
        {
            using var stream = new FileStream(deckPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
            using var package = Package.Open(stream, FileMode.Open, FileAccess.Read);
            using var bytes = new MemoryStream();
            using (var part = package.GetPart(new Uri(picture.Part, UriKind.Relative)).GetStream(FileMode.Open, FileAccess.Read)) part.CopyTo(bytes);
            const BitmapCreateOptions options = BitmapCreateOptions.PreservePixelFormat | BitmapCreateOptions.IgnoreColorProfile;
            bytes.Position = 0;
            var size = BitmapDecoder.Create(bytes, options, BitmapCacheOption.None).Frames[0]; // the header only
            int fullWidth = size.PixelWidth, fullHeight = size.PixelHeight;
            bytes.Position = 0;
            var decoded = new BitmapImage();
            decoded.BeginInit();
            decoded.StreamSource = bytes;
            decoded.CacheOption = BitmapCacheOption.OnLoad;
            decoded.CreateOptions = options;
            if (Math.Max(fullWidth, fullHeight) > MaxPictureSide)
            {
                if (fullWidth >= fullHeight) decoded.DecodePixelWidth = MaxPictureSide;
                else decoded.DecodePixelHeight = MaxPictureSide;
            }
            decoded.EndInit();
            int left = (int)Math.Round(decoded.PixelWidth * picture.CropLeft), top = (int)Math.Round(decoded.PixelHeight * picture.CropTop);
            int width = decoded.PixelWidth - (int)Math.Round(decoded.PixelWidth * picture.CropRight) - left;
            int height = decoded.PixelHeight - (int)Math.Round(decoded.PixelHeight * picture.CropBottom) - top;
            if (width < 16 || height < 16) throw new InvalidDataException($"그림이 너무 작습니다 ({width}×{height}).");
            var shown = new FormatConvertedBitmap(new CroppedBitmap(decoded, new Int32Rect(left, top, width, height)), PixelFormats.Pbgra32, null, 0);
            var pixels = new byte[width * height * 4];
            shown.CopyPixels(pixels, width * 4, 0);
            // Premultiplied colour: over white, each channel gains the white its transparency lets through.
            for (int i = 0; i < pixels.Length; i += 4)
            {
                byte clear = (byte)(255 - pixels[i + 3]);
                pixels[i] += clear;
                pixels[i + 1] += clear;
                pixels[i + 2] += clear;
                pixels[i + 3] = 255;
            }
            var image = BitmapSource.Create(width, height, 96, 96, PixelFormats.Bgr32, null, pixels, width * 4);
            image.Freeze();
            return image;
        }
        catch (Exception error) // any failure (a corrupt part, an odd format, no memory) sends the slide to the render
        {
            System.Diagnostics.Debug.WriteLine(error.Message);
            failed?.Invoke(error);
            return null;
        }
    }

    private static XDocument ReadXml(PackagePart part)
    {
        using var stream = part.GetStream(FileMode.Open, FileAccess.Read);
        using var reader = XmlReader.Create(stream, new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit, MaxCharactersInDocument = 16_000_000 });
        return XDocument.Load(reader);
    }
}
