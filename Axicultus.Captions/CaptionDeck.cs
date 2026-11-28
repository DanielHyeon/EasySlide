using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Text.RegularExpressions;
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
/// HasPicture: the slide has a content picture (a score, lyrics as an image), or a shown picture smaller than a
/// background, or little text (<see cref="CaptionDeck.ShortText"/>) and a shown picture of any size or a picture
/// background of its own (their lyrics read from the render); null when not known (.ppt).
/// Picture: the slide's content picture (a hymn's score apart from the background photo and the verse buttons), if any.
/// Boxes start with the slide's whole text (<see cref="CaptionDeck.AllTextKey"/>).
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
        double slideWidth = Number(size, "cx"), slideHeight = Number(size, "cy"), slideArea = slideWidth * slideHeight;
        var parsed = new Dictionary<Uri, XDocument?>(); // layouts and masters, read once (null: unreadable)
        foreach (var slideId in slideList?.Elements() ?? [])
        {
            cancellation.ThrowIfCancellationRequested();
            var id = slideId.Attributes().Single(a => a.Name.LocalName == "id" && a.Name.NamespaceName.Length > 0).Value;
            var relation = presentation.GetRelationship(id);
            var part = package.GetPart(PackUriHelper.ResolvePartUri(presentation.Uri, relation.TargetUri));
            var boxes = new List<CaptionBox>();
            var shapeXml = ReadXml(part);
            var presentationNs = shapeXml.Root!.Name.Namespace;
            int skipped = 0; // Fallback copies left out (counted in the numbers of unnamed boxes, as when they were listed)
            foreach (var shape in shapeXml.Descendants(presentationNs + "sp"))
            {
                var body = shape.Element(presentationNs + "txBody");
                if (body is null) continue;
                var text = Paragraphs(body);
                if (text.Length == 0) continue;
                var props = shape.Element(presentationNs + "nvSpPr");
                var name = (string?)props?.Element(presentationNs + "cNvPr")?.Attribute("name") ?? CaptionText.Format("CaptionConsole", "BoxName", boxes.Count + skipped + 1);
                var placeholder = props?.Descendants(presentationNs + "ph").FirstOrDefault();
                var key = placeholder is null ? "shape:" + name :
                    $"placeholder:{(string?)placeholder.Attribute("type") ?? "obj"}:{(string?)placeholder.Attribute("idx") ?? "0"}";
                // The mc:Fallback copy of a box its mc:Choice already listed is that box again (a050 design 11); one
                // only in the Fallback stays. Numbered all the same: an unnamed box keeps the number it had.
                if (shape.Ancestors().Any(e => e.Name.LocalName == "Fallback") && boxes.Any(box => box.Key == key)) { skipped++; continue; }
                boxes.Add(new(key, name, text));
            }
            var picture = ContentPicture(part, shapeXml, slideArea);
            var allText = AllText(part, shapeXml, slideWidth, slideHeight, parsed);
            // A picture slide: a content picture; or a shown picture smaller than a background (80% of the slide), whatever
            // the text (a score cut in pieces under a title); or little text and any shown picture or a picture background
            // of its own (a score set as the background, perhaps under a page mark).
            var pictures = ShownPictures(shapeXml).ToList();
            slides.Add(new([allText, .. boxes], picture != null
                || pictures.Any(area => slideArea <= 0 || area is not { } size || size < 0.80 * slideArea)
                || ShortText(allText.Text) && (pictures.Count > 0 || PictureBackground(shapeXml)), picture));
        }
        if (slides.Count == 0) throw new InvalidDataException(CaptionText.Get("CaptionConsole", "NoSlides"));
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
            var (area, turned, hidden) = Place(shape, xfrm);
            if (hidden) continue;
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
            if (!turned && area.Width * area.Height >= 0.10 * slideArea && embed != null && slide.RelationshipExists(embed)
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

    private static XElement? Child(XElement? parent, string name) => parent?.Elements().FirstOrDefault(e => e.Name.LocalName == name);
    private static bool Turned(XElement xfrm) => Number(xfrm, "rot") != 0 || (string?)xfrm.Attribute("flipH") is "1" or "true" || (string?)xfrm.Attribute("flipV") is "1" or "true";
    private static bool Hidden(XElement? properties) => (string?)Child(properties, "cNvPr")?.Attribute("hidden") is "1" or "true";
    // A shape's own frame: p:spPr/a:xfrm, or p:xfrm on an object frame (a table).
    private static XElement? Frame(XElement shape) => Child(Child(shape, "spPr"), "xfrm") ?? Child(shape, "xfrm");

    // The shape's rectangle on the slide from its frame (the groups around it move and scale it), whether it or a group
    // around it is turned, and whether a group around it is hidden.
    private static (Rect Area, bool Turned, bool Hidden) Place(XElement shape, XElement xfrm)
    {
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
        return (new Rect(x, y, Math.Max(0, width), Math.Max(0, height)), turned, hidden);
    }

    // A text body's paragraphs, one line each (a line break inside one starts a line too).
    private static string Paragraphs(XElement body) =>
        string.Join("\n", body.Elements().Where(e => e.Name.LocalName == "p")
            .Select(p => string.Concat(p.Descendants().Where(e => e.Name.LocalName is "t" or "br")
                .Select(e => e.Name.LocalName == "br" ? "\n" : e.Value)))).Trim();

    // The slide's whole text: shown text boxes and tables (a row a line, its cells a space apart) in reading order,
    // top to bottom in bands of 5% of the slide's height and left to right inside a band. Left out: hidden shapes, the
    // slide number, date and footer, small shapes at the slide's foot that carry a link or say little (verse
    // buttons, "2/8"), and shapes off the slide. A placeholder without a frame takes its layout's or master's; one
    // placed nowhere comes last.
    private static CaptionBox AllText(PackagePart slide, XDocument xml, double slideWidth, double slideHeight, Dictionary<Uri, XDocument?> parsed)
    {
        var texts = new List<(double Band, double Left, string Text)>();
        foreach (var shape in xml.Descendants().Where(e => e.Name.LocalName is "sp" or "graphicFrame" && e.Name.Namespace == xml.Root!.Name.Namespace))
        {
            if (shape.Ancestors().Any(e => e.Name.LocalName == "Fallback")) continue; // the same shape as its mc:Choice
            var names = Child(shape, shape.Name.LocalName == "sp" ? "nvSpPr" : "nvGraphicFramePr");
            var placeholder = Child(Child(names, "nvPr"), "ph");
            if (Hidden(names) || (string?)placeholder?.Attribute("type") is "sldNum" or "dt" or "ftr"
                || shape.Ancestors().Any(g => g.Name.LocalName == "grpSp" && Hidden(Child(g, "nvGrpSpPr")))) continue;
            string text = Child(shape, "txBody") is { } body ? Paragraphs(body) : string.Join("\n",
                shape.Descendants().Where(e => e.Name.LocalName == "tr").Select(row => string.Join(" ", row.Elements().Where(e => e.Name.LocalName == "tc")
                    .Select(cell => Child(cell, "txBody") is { } cellBody ? string.Join(" ", Paragraphs(cellBody).Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)) : "")
                    .Where(cell => cell.Length > 0))).Where(row => row.Length > 0));
            if (text.Length == 0) continue;
            if ((Frame(shape) ?? (placeholder is null ? null : Inherited(slide, placeholder, parsed))) is not { } xfrm)
            {
                texts.Add((double.MaxValue, 0, text));
                continue;
            }
            var (area, _, hidden) = Place(shape, xfrm);
            if (hidden) continue;
            if (slideWidth > 0 && slideHeight > 0 && (area.Top >= 0.88 * slideHeight && area.Height <= 0.08 * slideHeight && (text.Length <= 6 || HasLink(shape))
                || area.Right <= 0 || area.Bottom <= 0 || area.Left >= slideWidth || area.Top >= slideHeight)) continue;
            texts.Add((slideHeight > 0 ? Math.Floor(area.Top / (0.05 * slideHeight)) : area.Top, area.Left, text));
        }
        return AllText(texts.OrderBy(t => t.Band).ThenBy(t => t.Left).Select(t => t.Text));
    }

    // A link of any kind (a verse button, also the current verse's that goes nowhere) on the shape or its text, on a
    // group around it, or anywhere in its own group (a button's number beside its linked picture, as a035 counts buttons).
    private static bool HasLink(XElement shape)
    {
        static bool Link(XElement e) => e.Name.LocalName == "hlinkClick";
        return shape.Descendants().Any(Link)
            || shape.Ancestors().Any(g => g.Name.LocalName == "grpSp" && Child(g, "nvGrpSpPr")?.Descendants().Any(Link) == true)
            || shape.Parent is { Name.LocalName: "grpSp" } parent && parent.Descendants().Any(Link);
    }

    // The slide's own background is a picture (p:bg/p:bgPr/a:blipFill); a layout's or master's is not looked at.
    private static bool PictureBackground(XDocument xml) =>
        xml.Root?.Elements().FirstOrDefault(e => e.Name.LocalName == "cSld") is { } slide
        && Child(Child(slide, "bg"), "bgPr")?.Elements().Any(e => e.Name.LocalName == "blipFill") == true;

    // The pictures not hidden (themselves or by a group around them) and their area on the slide (null: no frame of their own).
    private static IEnumerable<double?> ShownPictures(XDocument xml) =>
        xml.Descendants(xml.Root!.Name.Namespace + "pic")
            .Where(pic => !Hidden(Child(pic, "nvPicPr")) && !pic.Ancestors().Any(g => g.Name.LocalName == "grpSp" && Hidden(Child(g, "nvGrpSpPr"))))
            .Select(pic => Frame(pic) is { } frame && Place(pic, frame).Area is var area ? area.Width * area.Height : (double?)null);

    // The frame a placeholder without its own takes, as PowerPoint inherits it: the layout's placeholder of the same idx
    // (else of the same type), else the master's of the same type (ctrTitle as title; subTitle, obj and none as body).
    // A layout or master that cannot be read is no frame: the box goes last, the deck is still read.
    private static XElement? Inherited(PackagePart slide, XElement placeholder, Dictionary<Uri, XDocument?> parsed)
    {
        static string TypeOf(XElement ph) => (string?)ph.Attribute("type") ?? "obj";
        static string OnMaster(string type) => type switch { "ctrTitle" => "title", "subTitle" or "obj" => "body", _ => type };
        // ph sits in nvPr, in the shape's non-visual properties, in the shape
        static XElement? FrameOf(XElement? ph) => ph?.Parent?.Parent?.Parent is { } shape ? Frame(shape) : null;
        (PackagePart Part, List<XElement> Placeholders)? Related(PackagePart part, string kind)
        {
            var relation = part.GetRelationships().FirstOrDefault(r => r.RelationshipType.EndsWith(kind, StringComparison.Ordinal) && r.TargetMode == TargetMode.Internal);
            if (relation is null) return null;
            var uri = PackUriHelper.ResolvePartUri(part.Uri, relation.TargetUri);
            if (!part.Package.PartExists(uri)) return null;
            var target = part.Package.GetPart(uri);
            if (!parsed.TryGetValue(uri, out var xml))
            {
                // Read once: one that cannot be read is remembered as not there.
                try { xml = ReadXml(target); }
                catch (Exception error) { System.Diagnostics.Debug.WriteLine(error.Message); xml = null; }
                parsed[uri] = xml;
            }
            if (xml is null) return null;
            return (target, xml.Descendants().Where(e => e.Name.LocalName == "ph").ToList());
        }
        try
        {
            string? idx = (string?)placeholder.Attribute("idx");
            if (Related(slide, "/slideLayout") is not var (layout, onLayout)) return null;
            var same = onLayout.FirstOrDefault(ph => idx != null && (string?)ph.Attribute("idx") == idx)
                ?? onLayout.FirstOrDefault(ph => TypeOf(ph) == TypeOf(placeholder));
            if (FrameOf(same) is { } frame) return frame;
            if (Related(layout, "/slideMaster") is not var (_, onMaster)) return null;
            var kind = OnMaster(TypeOf(same ?? placeholder));
            return FrameOf(onMaster.FirstOrDefault(ph => OnMaster(TypeOf(ph)) == kind));
        }
        catch (Exception error) // any fault in a layout or master (bad XML, an odd relationship) only loses the place
        {
            System.Diagnostics.Debug.WriteLine(error.Message);
            return null;
        }
    }

    /// <summary>The box every slide starts with: its whole text.</summary>
    public const string AllTextKey = "slide:all";

    /// <summary>
    /// The whole-text box of texts already in reading order: one line apart, blank lines kept to one, each line trimmed
    /// and its runs of white space made one space. A text that is
    /// only a number mark in ASCII digits ("1", "2.", "(3)", "2/4", "3-4", "12/16": a verse badge, a page mark) is left out.
    /// </summary>
    internal static CaptionBox AllText(IEnumerable<string> texts) => new(AllTextKey, CaptionText.Get("CaptionConsole", "AllText"),
        Regex.Replace(string.Join("\n", string.Join("\n", texts.Where(text => !string.IsNullOrWhiteSpace(text) && !NumberMark.IsMatch(text.Trim())))
            .Split('\n').Select(line => Spaces.Replace(line, " ").Trim())), @"\n\s*\n", "\n\n").Trim());

    // A run of white space inside a line, no-break and full-width spaces too (lyrics spaced out syllable by syllable).
    private static readonly Regex Spaces = new(@"[^\S\n]+", RegexOptions.CultureInvariant);

    private static readonly Regex NumberMark = new(@"^\(?[0-9]{1,2}(\.|\)|[/-][0-9]{1,2})?$", RegexOptions.CultureInvariant);

    /// <summary>Little text: at most 12 characters besides white space (a page mark, a word), none included.</summary>
    internal static bool ShortText(string text) => text.Count(c => !char.IsWhiteSpace(c)) <= 12;

    /// <summary>The slide with its whole-text box first, its boxes joined in the order given (a .ppt read by PowerPoint).</summary>
    public static CaptionSlide WithAllText(CaptionSlide slide) => slide with { Boxes = [AllText(slide.Boxes.Select(box => box.Text)), .. slide.Boxes] };


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
            if (width < 16 || height < 16) throw new InvalidDataException(CaptionText.Format("CaptionConsole", "PictureTooSmall", width, height));
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
