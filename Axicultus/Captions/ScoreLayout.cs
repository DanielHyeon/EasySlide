using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Axicultus.Wpf.Captions;

/// <summary>
/// A row of a score slide: a staff (five long horizontal lines) with the notes just above it and the lyrics under it.
/// The title, empty slide and background are left out, and every row of a slide is cut the same size, so each staff
/// shows at the same scale on the camera. PadTop/PadBottom: white to add above and below Area to reach the deck's size.
/// Erase: for each pixel of Area (row by row), ScoreLayout.Erased where it shows another row's ink or ink of no row and is to be
/// painted over, ScoreLayout.Paper where it shows no ink (content pictures).
/// </summary>
internal sealed record ScoreRow(Int32Rect Area, int StaffTop, int StaffBottom, int PadTop = 0, int PadBottom = 0, byte[]? Erase = null);

/// <summary>How far a row reaches past its staff, in staff spacings: Up above the top line, Down under the bottom line.</summary>
internal readonly record struct RowExtent(double Up, double Down);

/// <summary>
/// A content picture's rows, measured once: each row's own area (no white added, nothing painted over yet), its own reach
/// and its staff spacing, and who owns each pixel of the picture. <see cref="Fit"/> adds the white that makes the rows
/// the deck's size; <see cref="Erase"/> gives what is painted over in a row, worked out the first time it is asked for.
/// </summary>
internal sealed class ScorePage
{
    // Per pixel of the picture: 0 no ink, 1 ink of no row, 2 + k row k's ink.
    private readonly byte[] _owner;
    private readonly int _stride;
    private readonly object?[] _erase;
    private static readonly object Nothing = new();

    public ScorePage(ScoreRow[] rows, RowExtent[] reach, double[] spacing, byte[] owner, int stride)
    {
        (Rows, Reach, Spacing, _owner, _stride) = (rows, reach, spacing, owner, stride);
        _erase = new object?[rows.Length];
    }

    public ScoreRow[] Rows { get; }
    public RowExtent[] Reach { get; }
    public double[] Spacing { get; }

    /// <summary>The rows padded with white to the deck's target (see <see cref="ScoreLayout.Target"/>), without their marks.</summary>
    public ScoreRow[] Fit(RowExtent target) => [.. Rows.Select((row, k) =>
    {
        double spacing = Spacing[k];
        int pad = Math.Max(2, (int)(spacing / 2));
        int wantTop = row.StaffTop - (int)Math.Round(Math.Max(Reach[k].Up, target.Up) * spacing) - pad;
        int wantBottom = row.StaffBottom + (int)Math.Round(Math.Max(Reach[k].Down, target.Down) * spacing) + pad;
        return row with { PadTop = Math.Max(0, row.Area.Y - wantTop), PadBottom = Math.Max(0, wantBottom - row.Area.Y - row.Area.Height) };
    })];

    /// <summary>The k-th row's <see cref="ScoreRow.Erase"/> marks (kept once made; any thread).</summary>
    public byte[]? Erase(int k)
    {
        if (Volatile.Read(ref _erase[k]) is { } kept) return kept as byte[];
        var marks = Foreign(Rows[k].Area, (byte)(2 + k), Math.Max(2, (int)(Spacing[k] / 2)));
        Volatile.Write(ref _erase[k], (object?)marks ?? Nothing);
        return marks;
    }

    /// <summary>The memory it takes beside the picture.</summary>
    public long Bytes => _owner.Length + _erase.Sum(marks => (long)((marks as byte[])?.Length ?? 0));

    // For each pixel of the area: Erased where it shows ink that is not the row's own or lies within halo of such ink (its
    // glow) and is not the row's own ink; Paper where it is no ink at all and stays; else 0 (ink that stays). Null when
    // nothing is erased.
    private byte[]? Foreign(Int32Rect area, byte row, int halo)
    {
        int w = area.Width, h = area.Height;
        var owner = _owner;
        var foreign = new bool[256];
        for (int o = 1; o < 256; o++) foreign[o] = o != row;
        // Along each pixel row first: foreign ink within halo on either side (the nearest one each way).
        var marks = new byte[w * h];
        bool any = false;
        for (int y = 0; y < h; y++)
        {
            int line = (area.Y + y) * _stride + area.X, at = y * w, last = -halo - 1;
            bool seen = false;
            for (int x = 0; x < w; x++)
            {
                if (foreign[owner[line + x]]) { last = x; seen = true; }
                if (x - last <= halo) marks[at + x] = 1;
            }
            if (!seen) continue;
            any = true;
            for (int x = w - 1, next = w + halo + 1; x >= 0; x--)
            {
                if (foreign[owner[line + x]]) next = x;
                if (next - x <= halo) marks[at + x] = 1;
            }
        }
        if (!any) return null;
        // Then down each column the same way (bit 2: such a pixel within halo above).
        var above = new int[w];
        Array.Fill(above, -halo - 1);
        for (int y = 0, at = 0; y < h; y++)
            for (int x = 0; x < w; x++, at++)
            {
                if (marks[at] == 1) above[x] = y;
                if (y - above[x] <= halo) marks[at] |= 2;
            }
        var below = above;
        Array.Fill(below, h + halo + 1);
        for (int y = h - 1; y >= 0; y--)
        {
            int line = (area.Y + y) * _stride + area.X, at = y * w;
            for (int x = 0; x < w; x++)
            {
                byte mark = marks[at + x], o = owner[line + x];
                if ((mark & 1) != 0) below[x] = y;
                marks[at + x] = o == row ? (byte)0 : (mark & 2) != 0 || below[x] - y <= halo ? ScoreLayout.Erased : o == 0 ? ScoreLayout.Paper : (byte)0;
            }
        }
        return marks;
    }
}

internal static class ScoreLayout
{
    // Left and Right: where the staff lines run. A background picture beside the score lies outside them.
    private sealed record Staff(int Top, int Bottom, double Spacing, int Left, int Right, Line[] Lines);

    // A long horizontal line: its middle pixel row, its first and last pixel rows and where it runs.
    private readonly record struct Line(int Y, int First, int Last, int Left, int Right);

    /// <summary>
    /// Rows top to bottom; empty when the slide has no staff. Without a target (a slide render) the rows are cut as
    /// they always were. With one (the deck's, see <see cref="Target"/>; a content picture) a row is its staff, the ink
    /// joined to it and the lyrics under it, and white pads it to the target, so rows show their staves the same size;
    /// a row whose own ink reaches farther keeps it.
    /// </summary>
    internal static ScoreRow[] FindRows(BitmapSource image, RowExtent? target = null)
    {
        if (target is { } deck) { var page = Layout(image); return [.. page.Fit(deck).Select((row, k) => row with { Erase = page.Erase(k) })]; }
        if (Read(image) is not var (pixels, width, height, ink, _, _)) return [];
        var staves = FindStaves(pixels, width, height, ink, content: false);
        return staves.Count == 0 ? [] : SlideRows(staves, pixels, width, height, ink);
    }

    /// <summary>How far each row of a content picture reaches past its staff (its own ink), top to bottom; empty when no staff.</summary>
    internal static RowExtent[] Measure(BitmapSource image) => Layout(image).Reach;

    private static readonly ScorePage Empty = new([], [], [], [], 0);

    /// <summary>
    /// A content picture's rows (design 5g, 5h), measured once from the picture alone (its own lyric letter height, so
    /// every reading of it cuts the same); no rows when no staff is found on it.
    /// </summary>
    internal static ScorePage Layout(BitmapSource image)
    {
        if (Read(image) is not var (pixels, width, height, ink, strong, medium)) return Empty;
        var staves = FindStaves(pixels, width, height, ink, content: true);
        return staves.Count == 0 ? Empty : new Picture(staves, pixels, width, height, ink, strong, medium, image).Rows();
    }

    // Which pixels of the picture's top rows are of a saturated colour (the largest and smallest of red, green and blue
    // 60 or more apart): a title or a logo is drawn in colour, music marks in black or grey (design 5h, round 10).
    private static bool[] Colored(BitmapSource image, int width, int rows)
    {
        var color = new FormatConvertedBitmap(new CroppedBitmap(image, new Int32Rect(0, 0, width, rows)), PixelFormats.Bgr32, null, 0);
        var bgr = new byte[width * rows * 4];
        color.CopyPixels(bgr, width * 4, 0);
        var colored = new bool[width * rows];
        for (int at = 0, i = 0; at < colored.Length; at++, i += 4)
            colored[at] = Math.Max(bgr[i], Math.Max(bgr[i + 1], bgr[i + 2])) - Math.Min(bgr[i], Math.Min(bgr[i + 1], bgr[i + 2])) >= 60;
        return colored;
    }

    /// <summary>
    /// The deck's row size from its slides' rows (each slide's <see cref="Measure"/>): on each side the 90th percentile
    /// of the reach when there are 10 rows or more, else the median, so one row's title or high notes do not make every
    /// row taller.
    /// </summary>
    internal static RowExtent Target(IEnumerable<RowExtent[]> slides)
    {
        var rows = slides.SelectMany(rows => rows).ToArray();
        static double Typical(double[] values)
        {
            Array.Sort(values);
            int n = values.Length;
            return n == 0 ? 0 : n >= 10 ? values[(int)Math.Ceiling(0.9 * n) - 1] : (values[(n - 1) / 2] + values[n / 2]) / 2;
        }
        return new(Typical([.. rows.Select(r => r.Up)]), Typical([.. rows.Select(r => r.Down)]));
    }

    // The picture's grey levels and which of them are ink: dark notes on a light slide, or light notes on a dark one
    // (the background decides; thin staff lines come out grey, so ink is anything clearly off the slide's average).
    // Strong ink is well past that (letters and notes, not their shadow or glow).
    private static (byte[] Pixels, int Width, int Height, bool[] Ink, bool[] Strong, bool[] Medium)? Read(BitmapSource image)
    {
        var gray = new FormatConvertedBitmap(image, PixelFormats.Gray8, null, 0);
        int width = gray.PixelWidth, height = gray.PixelHeight;
        if (width < 16 || height < 16) return null;
        var pixels = new byte[width * height];
        gray.CopyPixels(pixels, width, 0);
        long sum = 0;
        foreach (var p in pixels) sum += p;
        int mean = (int)(sum / pixels.Length);
        int dark = Math.Min(180, mean - 40), light = Math.Max(150, mean + 60);
        bool[] ink = new bool[256], strong = new bool[256], medium = new bool[256];
        for (int p = 0; p < 256; p++)
        {
            ink[p] = mean >= 128 ? p < dark : p > light;
            strong[p] = mean >= 128 ? p < dark * 2 / 3 : p > light + (255 - light) / 3;
            medium[p] = mean >= 128 ? p < dark * 5 / 6 : p > light + (255 - light) / 6;
        }
        return (pixels, width, height, ink, strong, medium);
    }

    // content: a content picture's staves (design 5d: lines must be thin and have paper beside them); a slide render's
    // are found as they always were.
    private static List<Staff> FindStaves(byte[] pixels, int width, int height, bool[] ink, bool content)
    {
        int minRun = width / 4;
        var lineRows = new List<(int Y, int Left, int Right)>();
        for (int y = 0; y < height; y++)
        {
            int run = 0, best = 0, bestEnd = 0, gap = 0, row = y * width;
            for (int x = 0; x < width; x++)
            {
                if (ink[pixels[row + x]]) { run += gap + 1; gap = 0; }
                else if (run > 0 && ++gap > 3) { run = 0; gap = 0; } // a staff line may break for a few pixels
                if (run > best) { best = run; bestEnd = x; }
            }
            if (best >= minRun) lineRows.Add((y, bestEnd - best + 1, bestEnd));
        }

        // A line is one to a few pixel rows thick.
        var lines = new List<Line>();
        for (int i = 0; i < lineRows.Count;)
        {
            int start = i;
            while (i + 1 < lineRows.Count && lineRows[i + 1].Y - lineRows[i].Y <= 1) i++;
            var rows = lineRows.GetRange(start, i - start + 1);
            lines.Add(new((rows[0].Y + rows[^1].Y) / 2, rows[0].Y, rows[^1].Y, rows.Min(r => r.Left), rows.Max(r => r.Right)));
            i++;
        }

        // Five evenly spaced lines make a staff; a lone long line (a box border, a dash) does not. In a content picture a
        // band thicker than a quarter of the widest spacing (a bold lyric line, a photo's band) is no staff line at all,
        // and the staff's lines are those with paper beside them; its spacing is the usual gap between them, so a line
        // left out in the middle does not widen it.
        var staves = new List<Staff>();
        int maxSpacing = Math.Max(4, height / 22);
        if (content) lines.RemoveAll(l => l.Last - l.First + 1 > Math.Max(3, maxSpacing / 4));
        for (int i = 0; i < lines.Count;)
        {
            int start = i;
            while (i + 1 < lines.Count && lines[i + 1].Y - lines[i].Y <= maxSpacing) i++;
            var run = lines.GetRange(start, i - start + 1);
            if (run.Count >= 4 && (content ? StaffLines(pixels, width, height, run, ink) : run) is { Count: >= 4 } staff)
                staves.Add(new(staff[0].Y, staff[^1].Y, content ? Spacing(staff) : (staff[^1].Y - staff[0].Y) / (double)(staff.Count - 1),
                    staff.Min(l => l.Left), staff.Max(l => l.Right), [.. staff]));
            i++;
        }
        return staves;
    }

    // The spacing of a staff's lines: the whole height over the number of usual gaps in it (the middle gap between
    // adjacent lines), so a line left out does not make the others look farther apart.
    private static double Spacing(List<Line> staff)
    {
        var gaps = staff.Zip(staff.Skip(1), (a, b) => b.Y - a.Y).Order().ToList();
        double usual = gaps[gaps.Count / 2], height = staff[^1].Y - staff[0].Y;
        return height / Math.Max(1, Math.Round(height / usual));
    }

    // The share of a staff line's length that must have paper right above and below it.
    private const double PaperBeside = 0.25;

    // The staff lines among a run of long lines: thin, with paper right above and below them along much of their
    // length (notes or a dark blotch cover stretches of them). The bands of a photo drawn in the picture, beside the
    // staff or alone, are thick or have no paper beside them, and are left out.
    private static List<Line> StaffLines(byte[] pixels, int width, int height, List<Line> lines, bool[] ink)
    {
        int stretch = Math.Max(3, (lines[^1].Y - lines[0].Y) / (lines.Count - 1) / 2);
        double Paper(Line line)
        {
            int above = line.First - 2, below = line.Last + 2, run = 0, clear = 0;
            bool Clear(int x) => (above < 0 || !ink[pixels[above * width + x]]) && (below >= height || !ink[pixels[below * width + x]]);
            for (int x = line.Left; x <= line.Right + 1; x++)
            {
                if (x <= line.Right && Clear(x)) { run++; continue; }
                if (run >= stretch) clear += run; // a texture's specks of paper are no stretch of it
                run = 0;
            }
            return clear / (double)(line.Right - line.Left + 1);
        }
        var kept = lines.Where(l => Paper(l) >= PaperBeside).ToList();
        if (kept.Count < 2) return kept;
        int thick = Math.Max(3, (kept[^1].Y - kept[0].Y) / (kept.Count - 1) / 4);
        kept.RemoveAll(l => l.Last - l.First + 1 > thick);
        return kept;
    }

    // A slide render's rows, as before content pictures: every row of the slide gets one size.
    private static ScoreRow[] SlideRows(List<Staff> staves, byte[] pixels, int width, int height, bool[] ink)
    {
        // Bounds no row may cross: the slide's top, the gaps between staves, the end of the last lyrics.
        var cuts = new List<int> { 0 };
        for (int k = 1; k < staves.Count; k++)
            cuts.Add(Gap(pixels, width, staves[k - 1], staves[k], ink));
        cuts.Add(height);

        // How far each row's ink reaches above and below its staff: notes and clef above, notes and lyrics below.
        var above = staves.Select((staff, k) => Above(pixels, width, staff, cuts[k], ink)).ToArray();
        var below = staves.Select((staff, k) => End(pixels, width, cuts[k + 1], staff, ink) - staff.Bottom).ToArray();
        // One size for every row. The last row's lyrics end is the least sure (buttons or a picture may follow),
        // so the rows above it decide when there are any.
        int up = above.Max();
        int down = staves.Count > 1 ? below[..^1].Max() : Math.Min(below[0], (int)(10 * staves[0].Spacing));
        int pad = Math.Max(2, (int)(staves[0].Spacing / 2));
        // Same height, so the staves show the same size; each its own width, so no empty slide beside a shorter staff.
        return [.. staves.Select((staff, k) =>
        {
            int top = Math.Max(cuts[k], staff.Top - up - pad), bottom = Math.Min(cuts[k + 1], staff.Bottom + down + pad);
            return new ScoreRow(Trim(pixels, width, Span(staff, width), top, Math.Max(top + 1, bottom), ink), staff.Top, staff.Bottom);
        })];
    }

    /// <summary>Marks in <see cref="ScoreRow.Erase"/>.</summary>
    internal const byte Erased = 2, Paper = 1;

    // Spacings: how near a small mark or line sits to a staff's ink to be its own; how high that line may be; how high
    // a lyric band and how far apart the bands under the last staff may be (lyric lines reach 5.7, art drawn under them
    // 12 and more).
    private const double Near = 2.5, Small = 2, Lyric = 6, Far = 3;

    // The share of the lyric letter height a syllable must have to be lyrics (design 5h); smaller ones are marks.
    private const double Letter = 0.6;

    // A content picture's rows (design 5f, 5g). Ink is given out in whole pieces, never cut by a line: the strong ink
    // joined to a staff (and all ink within its lines) is that staff's row; the other strong ink falls into text lines
    // (lyrics, verses, a title, chords), each going whole to one row or none, and into marks (a tuplet's number, a
    // fermata), each going to the staff nearest to it. A shadow or glow goes with the strong ink it hangs on. A row
    // reaches as far as its own ink; any other ink in that reach is painted over with a little halo (its glow), so a
    // row shows only its own ink.
    private sealed class Picture
    {
        private const byte None = 0, Nobody = 1; // owner codes; row k is Row + k
        private const byte Row = 2;
        private readonly List<Staff> _staves;
        private readonly byte[] _pixels;
        private readonly int _width, _height, _n;
        private readonly bool[] _ink;
        // Per pixel: > 0 the component of its strong ink; < 0 -(the strong pixel + 1) its shadow or glow hangs on; 0 none.
        private readonly int[] _label;
        // Per pixel: who owns it (None: no ink).
        private readonly byte[] _owner;
        private readonly List<Part> _parts = [];
        private readonly int[] _cut;
        private readonly BitmapSource _image;
        private bool[]? _colored; // per pixel over the first staff: a saturated colour (read when first asked)

        // A component: its box, its pixel count, the staves whose lines it touches (Lo..Hi, -1 none).
        private sealed class Part
        {
            public int Top = int.MaxValue, Bottom = -1, Left = int.MaxValue, Right = -1, Count, Lo = -1, Hi = -1;
            public int Owner = Nobody;
            public bool Pale; // pale ink apart from strong ink (a shadow's far edge, a pale line)
            public Text? Line;
            public int Height => Bottom - Top + 1;
            public int Width => Right - Left + 1;
            public void Add(int x, int y)
            {
                if (y < Top) Top = y;
                if (y > Bottom) Bottom = y;
                if (x < Left) Left = x;
                if (x > Right) Right = x;
                Count++;
            }
        }

        // A text line (or a block of ink standing for one): its members and where they lie.
        private sealed class Text(List<int> members)
        {
            public readonly List<int> Members = members;
            public int Top, Bottom, Left, Right, Owner = Nobody;
            public bool Words; // a line of lyrics (not a block or a mark)
            public int Count; // its pixels
            public int Height => Bottom - Top + 1;
            public int Width => Right - Left + 1;
        }

        public Picture(List<Staff> staves, byte[] pixels, int width, int height, bool[] ink, bool[] strong, bool[] medium, BitmapSource image)
        {
            _pixels = pixels;
            _width = width;
            _height = height;
            _n = staves.Count;
            _ink = ink;
            _staves = [.. staves.Select(Widen)];
            _label = new int[width * height];
            _owner = new byte[width * height];
            _image = image;
            _cut = new int[Math.Max(1, _n - 1)];
            Array.Fill(_cut, int.MinValue);
            Group(strong, medium);

        }

        // The staff as wide as its lines run: pieces of them on the same rows joined across gaps of up to 2 spacings
        // (a line may break at a bar line or a scan's flaw), where three lines or more show ink.
        private Staff Widen(Staff staff)
        {
            int need = Math.Min(3, staff.Lines.Length - 1), gap = (int)(2 * staff.Spacing);
            bool Lined(int x)
            {
                int count = 0;
                foreach (var line in staff.Lines)
                    for (int y = Math.Max(0, line.First - 1); y <= Math.Min(_height - 1, line.Last + 1); y++)
                        if (_ink[_pixels[y * _width + x]]) { count++; break; }
                return count >= need;
            }
            int left = staff.Left, right = staff.Right;
            for (int x = left - 1, blank = 0; x >= 0 && blank <= gap; x--) { if (Lined(x)) { left = x; blank = 0; } else blank++; }
            for (int x = right + 1, blank = 0; x < _width && blank <= gap; x++) { if (Lined(x)) { right = x; blank = 0; } else blank++; }
            return staff with { Left = left, Right = right };
        }

        // Components of the strong ink and of all ink within the staves' lines (8-connected); then each shadow or glow
        // pixel hangs on the strong pixel it reaches first, and ink that reaches none (a pale line of its own) makes
        // components of its own.
        private void Group(bool[] strong, bool[] medium)
        {
            int width = _width, height = _height;
            var staffOf = new int[height];
            Array.Fill(staffOf, -1);
            for (int k = 0; k < _n; k++)
                for (int y = Math.Max(0, _staves[k].Top - 1); y <= Math.Min(height - 1, _staves[k].Bottom + 1); y++) staffOf[y] = k;
            // First pass: provisional labels joined by union-find.
            var parent = new int[1024];
            int next = 1;
            int Root(int a)
            {
                while (parent[a] != a) { parent[a] = parent[parent[a]]; a = parent[a]; }
                return a;
            }
            int Join(int a, int b)
            {
                a = Root(a); b = Root(b);
                if (a == b) return a;
                if (a < b) { parent[b] = a; return a; }
                parent[a] = b;
                return b;
            }
            var label = _label;
            for (int y = 0; y < height; y++)
            {
                int row = y * width, k = staffOf[y];
                int left = k >= 0 ? _staves[k].Left : int.MaxValue, right = k >= 0 ? _staves[k].Right : -1;
                for (int x = 0; x < width; x++)
                {
                    byte p = _pixels[row + x];
                    if (!strong[p] && !(_ink[p] && x >= left && x <= right)) continue;
                    int at = row + x, found = 0;
                    if (x > 0 && label[at - 1] > 0) found = label[at - 1];
                    if (y > 0)
                    {
                        int up = at - width;
                        if (x > 0 && label[up - 1] > 0) found = found == 0 ? label[up - 1] : Join(found, label[up - 1]);
                        if (label[up] > 0) found = found == 0 ? label[up] : Join(found, label[up]);
                        if (x + 1 < width && label[up + 1] > 0) found = found == 0 ? label[up + 1] : Join(found, label[up + 1]);
                    }
                    if (found == 0)
                    {
                        if (next == parent.Length) Array.Resize(ref parent, parent.Length * 2);
                        parent[next] = next;
                        found = next++;
                    }
                    label[at] = found;
                }
            }

            // Second pass: final labels 1.. and each component's box.
            var final = new int[next];
            _parts.Add(new Part()); // 0: none
            for (int y = 0; y < height; y++)
            {
                int row = y * width, k = staffOf[y];
                for (int x = 0; x < width; x++)
                {
                    int at = row + x;
                    if (label[at] == 0) continue;
                    int root = Root(label[at]);
                    if (final[root] == 0) { final[root] = _parts.Count; _parts.Add(new Part()); }
                    var part = _parts[label[at] = final[root]];
                    part.Add(x, y);
                    if (k >= 0 && x >= _staves[k].Left && x <= _staves[k].Right)
                    {
                        if (part.Lo < 0 || k < part.Lo) part.Lo = k;
                        if (k > part.Hi) part.Hi = k;
                    }
                }
            }

            // Shadows and glows: breadth first from the strong ink through the other ink, first through the darker grey
            // (a pale stroke of a mark, the arm of a tuplet's bracket: it follows the mark it is drawn with, and joins
            // the mark's pieces) and then through the rest (a shadow's paler grey, which follows the nearest).
            var weak = new List<int>();
            for (int at = 0; at < label.Length; at++)
                if (label[at] == 0 && _ink[_pixels[at]]) weak.Add(at);
            var queue = new int[weak.Count];
            int head = 0, tail = 0;
            var joined = Enumerable.Range(0, _parts.Count).ToArray();
            int Linked(int c) { while (joined[c] != c) c = joined[c] = joined[joined[c]]; return c; }
            int PartOf(int at) => label[at] > 0 ? label[at] : label[at] < 0 ? label[-label[at] - 1] : 0;
            bool Tiny(int c) { var p = _parts[c]; double s = SpacingNear((p.Top + p.Bottom) / 2.0); return p.Lo < 0 && p.Height <= 1.5 * s && p.Width <= 6 * s; }
            for (int pass = 0; pass < 2; pass++)
            {
                bool darker = pass == 0;
                head = tail = 0;
                foreach (var at in weak)
                {
                    if (label[at] != 0 || darker && !medium[_pixels[at]]) continue;
                    int source = Source(at, at % width, at / width);
                    if (source < 0) continue;
                    label[at] = label[source] > 0 ? -(source + 1) : label[source];
                    queue[tail++] = at;
                }
                while (head < tail)
                {
                    int at = queue[head++], x = at % width, y = at / width, mine = PartOf(at);
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        int ny = y + dy;
                        if (ny < 0 || ny >= height) continue;
                        for (int dx = -1; dx <= 1; dx++)
                        {
                            int nx = x + dx;
                            if (nx < 0 || nx >= width) continue;
                            int near = ny * width + nx;
                            if (label[near] != 0)
                            {
                                // darker grey meeting another piece's: one mark (small pieces apart from the staves only, not letters)
                                int other = darker ? PartOf(near) : 0;
                                if (other > 0 && other != mine && Tiny(other) && Tiny(mine)) joined[Linked(other)] = Linked(mine);
                                continue;
                            }
                            if (!_ink[_pixels[near]] || darker && !medium[_pixels[near]]) continue;
                            label[near] = label[at];
                            queue[tail++] = near;
                        }
                    }
                }
            }
            // Pieces joined by darker grey are one component.
            bool linked = false;
            for (int c = 1; c < _parts.Count; c++)
            {
                int root = Linked(c);
                if (root == c) continue;
                linked = true;
                var (from, to) = (_parts[c], _parts[root]);
                (to.Top, to.Bottom, to.Left, to.Right, to.Count) = (Math.Min(to.Top, from.Top), Math.Max(to.Bottom, from.Bottom),
                    Math.Min(to.Left, from.Left), Math.Max(to.Right, from.Right), to.Count + from.Count);
                from.Count = 0;
            }
            if (linked)
                for (int at = 0; at < label.Length; at++)
                    if (label[at] > 0) label[at] = Linked(label[at]);

            // The rest is pale ink apart from strong ink: a shadow or glow beyond a letter's light outline hangs on the
            // strong ink it comes close to (each pixel the nearest, the farther ones with the piece); anything else (a pale
            // line of its own, the far side of a photo) makes components of its own.
            const int Apart = int.MinValue;
            var piece = new List<int>();
            foreach (var at in weak)
            {
                if (label[at] != 0) continue;
                piece.Clear();
                piece.Add(at);
                label[at] = Apart;
                for (int i = 0; i < piece.Count; i++)
                {
                    int here = piece[i], x = here % width, y = here / width;
                    for (int dy = -1; dy <= 1; dy++)
                        for (int dx = -1; dx <= 1; dx++)
                        {
                            int nx = x + dx, ny = y + dy;
                            if (nx < 0 || ny < 0 || nx >= width || ny >= height) continue;
                            int near = ny * width + nx;
                            if (label[near] != 0 || !_ink[_pixels[near]]) continue;
                            label[near] = Apart;
                            piece.Add(near);
                        }
                }
                double spacing = SpacingNear(at / width);
                int reach = Math.Max(3, (int)(spacing / 3)), close = 0, any = -1, high = int.MaxValue, low = -1;
                var sources = new int[piece.Count];
                for (int i = 0; i < piece.Count; i++)
                {
                    if ((sources[i] = Nearest(piece[i] % width, piece[i] / width, reach)) >= 0) { close++; any = sources[i]; }
                    (high, low) = (Math.Min(high, piece[i] / width), Math.Max(low, piece[i] / width));
                }
                // (all of a glow around letters; of a large pale area, a photo, only what lies close to strong ink)
                bool whole = close > 0 && (2 * close >= piece.Count || low - high + 1 <= 3 * spacing);
                var rest = new List<int>();
                for (int i = 0; i < piece.Count; i++)
                    if (whole || sources[i] >= 0) label[piece[i]] = -((sources[i] >= 0 ? sources[i] : any) + 1);
                    else { label[piece[i]] = 0; rest.Add(piece[i]); }
                foreach (var start in rest)
                {
                    if (label[start] != 0) continue;
                    var part = new Part { Pale = true };
                    int id = _parts.Count;
                    _parts.Add(part);
                    var stack = new Stack<int>();
                    label[start] = id;
                    stack.Push(start);
                    while (stack.Count > 0)
                    {
                        int here = stack.Pop(), x = here % width, y = here / width;
                        part.Add(x, y);
                        for (int dy = -1; dy <= 1; dy++)
                            for (int dx = -1; dx <= 1; dx++)
                            {
                                int nx = x + dx, ny = y + dy;
                                if (nx < 0 || ny < 0 || nx >= width || ny >= height) continue;
                                int near = ny * width + nx;
                                if (label[near] != 0 || !_ink[_pixels[near]]) continue;
                                label[near] = id;
                                stack.Push(near);
                            }
                    }
                }
            }
        }

        // The strong pixel nearest to x, y within reach (square rings outward), or -1.
        private int Nearest(int x, int y, int reach)
        {
            bool Of(int at) => _label[at] > 0; // strong ink
            for (int d = 1; d <= reach; d++)
            {
                int top = y - d, bottom = y + d, left = Math.Max(0, x - d), right = Math.Min(_width - 1, x + d);
                for (int nx = left; nx <= right; nx++)
                {
                    if (top >= 0 && Of(top * _width + nx)) return top * _width + nx;
                    if (bottom < _height && Of(bottom * _width + nx)) return bottom * _width + nx;
                }
                for (int ny = Math.Max(0, top + 1); ny <= Math.Min(_height - 1, bottom - 1); ny++)
                {
                    if (x - d >= 0 && Of(ny * _width + x - d)) return ny * _width + x - d;
                    if (x + d < _width && Of(ny * _width + x + d)) return ny * _width + x + d;
                }
            }
            return -1;
        }

        // A strong pixel (or one already hanging on one) next to the pixel, or -1.
        private int Source(int at, int x, int y)
        {
            for (int dy = -1; dy <= 1; dy++)
                for (int dx = -1; dx <= 1; dx++)
                {
                    int nx = x + dx, ny = y + dy;
                    if (nx < 0 || ny < 0 || nx >= _width || ny >= _height) continue;
                    int near = ny * _width + nx;
                    if (_label[near] != 0) return near;
                }
            return -1;
        }

        private double SpacingNear(double y)
        {
            int best = 0;
            for (int k = 1; k < _n; k++)
                if (Math.Abs(y - (_staves[k].Top + _staves[k].Bottom) / 2.0) < Math.Abs(y - (_staves[best].Top + _staves[best].Bottom) / 2.0)) best = k;
            return _staves[best].Spacing;
        }

        // The cut between staves k and k+1 (the emptiest gap), for ink joined across both.
        private int Cut(int k) => _cut[k] != int.MinValue ? _cut[k] : _cut[k] = Gap(_pixels, _width, _staves[k], _staves[k + 1], _ink);

        // The row a pixel of a staff's component belongs to: across two staves, the cut parts it; past the next
        // staff (or far under the last) it is no one's (a frame running down the picture).
        private byte StaffRow(Part part, int y)
        {
            int k = part.Lo;
            while (k < part.Hi && y > Cut(k)) k++;
            int yMin = k > 0 ? _staves[k - 1].Bottom + 2 : 0;
            int yMax = k < _n - 1 ? _staves[k + 1].Top - 2 : Math.Min(_height - 1, _staves[k].Bottom + (int)(Lyric * _staves[k].Spacing));
            return y < yMin || y > yMax ? Nobody : (byte)(Row + k);
        }

        public ScorePage Rows()
        {
            int width = _width, height = _height, n = _n;
            var label = _label;
            var owner = _owner;
            // 1. The staves' own ink, and how high and low it lies in each column.
            var parts = _parts.ToArray();
            var code = new byte[parts.Length]; // a staff component's row when all of it is that row's, 255 when it is parted
            for (int c = 1; c < parts.Length; c++)
            {
                var part = parts[c];
                if (part.Lo < 0) continue;
                code[c] = part.Lo == part.Hi && StaffRow(part, part.Top) == Row + part.Lo && StaffRow(part, part.Bottom) == Row + part.Lo ? (byte)(Row + part.Lo) : (byte)255;
            }
            var topOf = new int[n][];
            var bottomOf = new int[n][];
            for (int k = 0; k < n; k++) { topOf[k] = new int[width]; bottomOf[k] = new int[width]; Array.Fill(topOf[k], int.MaxValue); Array.Fill(bottomOf[k], -1); }
            for (int y = 0; y < height; y++)
                for (int x = 0, at = y * width; x < width; x++, at++)
                {
                    int c = label[at];
                    if (c <= 0 || code[c] == 0) continue;
                    byte o = code[c] == 255 ? StaffRow(parts[c], y) : code[c];
                    owner[at] = o;
                    if (o < Row) continue;
                    int k = o - Row;
                    if (y < topOf[k][x]) topOf[k][x] = y;
                    if (y > bottomOf[k][x]) bottomOf[k][x] = y;
                }
            var box = new (int Top, int Bottom, int Left, int Right)[n];
            for (int k = 0; k < n; k++)
            {
                box[k] = (int.MaxValue, -1, int.MaxValue, -1);
                for (int x = 0; x < width; x++)
                    if (bottomOf[k][x] >= 0)
                        box[k] = (Math.Min(box[k].Top, topOf[k][x]), Math.Max(box[k].Bottom, bottomOf[k][x]), Math.Min(box[k].Left, x), Math.Max(box[k].Right, x));
                if (box[k].Bottom < 0) box[k] = (_staves[k].Top, _staves[k].Bottom, _staves[k].Left, _staves[k].Right);
            }


            // 2. Syllables (pieces over and under each other: a letter's parts, its final consonant, a fermata's dot), then
            // by the picture's lyric letter height H (design 5h): syllables of 0.6 H or more lying on the same rows are lines
            // of lyrics (or a title), however few; smaller ones are marks (a tuplet's number or bracket, a chord, a
            // fermata, a measure number), each going its own way. Lines and marks never join.
            var loose = Enumerable.Range(1, _parts.Count - 1).Where(c => _parts[c].Lo < 0 && _parts[c].Count > 0).ToList();
            var syllables = Syllables(loose);
            double medianSpacing = _staves.Select(s => s.Spacing).Order().ElementAt(n / 2);
            double letter = (LyricHeight(syllables) ?? 2) * medianSpacing;
            var texts = new List<Text>();
            var marks = new List<Text>();
            var words = new List<Text>();
            foreach (var syllable in syllables)
            {
                double spacing = SpacingNear((syllable.Top + syllable.Bottom) / 2.0);
                if (syllable.Width >= width / 4 || syllable.Height > Lyric * spacing) texts.Add(syllable); // a block: a lyric bar, a photo, a frame
                else if (syllable.Height >= Letter * letter && syllable.Width * 4 > syllable.Height && !syllable.Members.All(c => _parts[c].Pale))
                    words.Add(syllable); // (not a thin upright stroke, an arm or a stem, nor pale ink alone, a photo's)
                else marks.Add(syllable);
            }
            // An upright stroke of letter height right beside a letter on its rows is that word's vowel (the upright i of eui or i).
            var vowels = marks.Where(m => m.Height >= Letter * letter && m.Width * 4 <= m.Height
                && words.Any(w => SameRows(w, m) && Apart(w, m) <= Letter * letter)).ToList();
            marks.RemoveAll(vowels.Contains);
            words.AddRange(vowels);
            // Over the first staff a title's letters stand side by side; a letter-sized piece far from any other on its
            // rows, in a line of marks far apart (a chord with the fermata under it among the other chords), is a mark,
            // not a title. (A picture alone there, a logo, is neither.)
            bool SameRows(Text a, Text b) => 2 * (Math.Min(a.Bottom, b.Bottom) - Math.Max(a.Top, b.Top) + 1) >= Math.Min(a.Height, b.Height);
            double Apart(Text a, Text b) => Math.Max(b.Left - a.Right, a.Left - b.Right);
            var alone = words.Where(w => (w.Top + w.Bottom) / 2.0 < _staves[0].Top
                && !words.Any(o => o != w && SameRows(w, o) && Apart(w, o) <= letter)
                && syllables.Any(o => o != w && SameRows(w, o) && Apart(w, o) > letter)).ToList();
            words.RemoveAll(alone.Contains);
            marks.AddRange(alone);
            var lines = LyricLines(words);
            texts.AddRange(lines);
            // A full stop, a dash, a comma or a small letter ("men" of "A-men") lying within a line's rows is the line's.
            marks.RemoveAll(mark =>
            {
                var home = lines.FirstOrDefault(l => mark.Top >= l.Top - 1 && mark.Bottom <= l.Bottom + 1);
                home?.Members.AddRange(mark.Members);
                return home != null;
            });
            // Over the first staff, a title in smaller letters than the lyrics: marks (and the title's letters of lyric size)
            // on the same rows, each within twice the tallest one's height of the next (a word space), with 3 or more letters
            // among them (as tall as half the tallest, not flat) of which most start too high to be the first row's marks
            // (see 4.), are a line of text. Not by their number: a row of tuplets or of chords over the staff can be long.
            bool Over(Text t) => (t.Top + t.Bottom) / 2.0 < _staves[0].Top;
            double markTop = box[0].Top - Near * _staves[0].Spacing;
            foreach (var row in Rows([.. marks.Where(Over).Concat(words.Where(Over))]))
            {
                var run = new List<Text>();
                foreach (var mark in row.OrderBy(m => m.Left).Append(null))
                {
                    bool next = mark != null && (run.Count == 0 || mark.Left - run.Max(m => m.Right) <= 2 * Math.Max(mark.Height, run.Max(m => m.Height)));
                    if (next) { run.Add(mark!); continue; }
                    int tallest = run.Count == 0 ? 0 : run.Max(m => m.Height);
                    var letters = run.Where(m => 2 * m.Height >= tallest && m.Width < 2.5 * m.Height).ToList();
                    if (letters.Count >= 3 && 2 * letters.Count(m => m.Top < markTop) > letters.Count)
                    {
                        // (only what lies on the letters' rows: a tuplet bracket under the title is the first row's)
                        var heights = letters.Select(m => m.Height).Order().ToArray();
                        double top = letters.Min(m => m.Top), bottom = letters.Select(m => m.Bottom).Order().ElementAt(3 * letters.Count / 4) + heights[heights.Length / 2] / 4.0;
                        var title = run.Where(m => marks.Contains(m) && (m.Top + m.Bottom) / 2.0 >= top && (m.Top + m.Bottom) / 2.0 <= bottom).ToList();
                        marks.RemoveAll(title.Contains);
                        if (title.Count > 0) texts.Add(new Text([.. title.SelectMany(m => m.Members)]) { Words = true });
                    }
                    run = mark == null ? [] : [mark];
                }
            }
            foreach (var text in texts) Seal(text);

            // 3. Lines and blocks: beside a staff, that row; across staves, no one's; above the first staff no one's (a
            // title); between two staves the upper row's, unless it is small and right on the lower staff's ink and nearer
            // to it and the upper row keeps something of its own; under the last staff its lyric lines, one after the other.
            var between = Enumerable.Range(0, Math.Max(0, n - 1)).Select(_ => new List<Text>()).ToArray();
            var under = new List<(int Top, int Bottom, int Count, int Middle, Action<int> Give)>();
            foreach (var text in texts)
            {
                int beside = Enumerable.Range(0, n).Count(k => text.Bottom >= _staves[k].Top && text.Top <= _staves[k].Bottom);
                double middle = (text.Top + text.Bottom) / 2.0, spacing = SpacingNear(middle);
                if (beside >= 2 || !text.Words && text.Height > 4 * spacing && text.Width <= Math.Max(3, spacing / 3)) text.Owner = Nobody; // a frame
                else if (Staffed(middle) is int k and >= 0) text.Owner = beside == 1 && text.Height <= 8 * spacing ? Row + k : Nobody;
                else if (middle < _staves[0].Top) text.Owner = Nobody;
                else if (middle > _staves[n - 1].Bottom) under.Add((text.Top, text.Bottom, text.Count, (text.Left + text.Right) / 2, row => text.Owner = row));
                else between[Enumerable.Range(0, n - 1).First(k => middle < _staves[k + 1].Top)].Add(text);
            }
            int GapOf(double middle) => Enumerable.Range(0, n - 1).First(k => middle < _staves[k + 1].Top);
            var marked = marks.Where(m => Staffed((m.Top + m.Bottom) / 2.0) < 0).ToLookup(m =>
            {
                double middle = (m.Top + m.Bottom) / 2.0;
                return middle < _staves[0].Top || middle > _staves[n - 1].Bottom ? -1 : GapOf(middle);
            });
            for (int k = 0; k < n - 1; k++)
            {
                var (upper, lower) = (_staves[k], _staves[k + 1]);
                var list = between[k];
                bool Down(Text t) => t.Top >= box[k + 1].Top - Near * lower.Spacing && t.Height <= Small * lower.Spacing
                    && box[k + 1].Top - t.Bottom < t.Top - box[k].Bottom;
                var down = list.Where(Down).ToList();
                // (unless the upper row would keep nothing of its own between the staves: then that is its lyrics)
                bool keeps = list.Count > down.Count || down.Count > 0 && marked[k].Any(m => m.Bottom < down.Min(t => t.Top));
                if (!keeps && box[k].Bottom < upper.Bottom + 3 * upper.Spacing) down.Clear();
                foreach (var text in list) text.Owner = Row + (down.Contains(text) ? k + 1 : k);
            }

            // 4. Marks, one by one: beside a staff, that row; above the first staff, within 2.5 spacings of its ink (a
            // title over them or not); between two staves the one nearest to it, but never across the upper row's lines
            // (a mark under the lyrics is the lower row's); under the last staff with its lyric lines.
            var tinted = marks.Where(m => Staffed((m.Top + m.Bottom) / 2.0) < 0 && (m.Top + m.Bottom) / 2.0 < _staves[0].Top && Colorful(m)).ToHashSet();
            foreach (var part in marks)
            {
                double middle = (part.Top + part.Bottom) / 2.0;
                if (Staffed(middle) is int k and >= 0) { part.Owner = Row + k; continue; }
                if (middle < _staves[0].Top)
                {
                    // (a title or a logo in colour is not a mark, close to the staff or not; nor is its grey shadow or outline)
                    bool title = tinted.Contains(part) || tinted.Any(t => part.Left <= t.Right + 3 && part.Right >= t.Left - 3 && part.Top <= t.Bottom + 3 && part.Bottom >= t.Top - 3);
                    part.Owner = part.Top >= box[0].Top - Near * _staves[0].Spacing && !title ? Row : Nobody;
                    continue;
                }
                if (middle > _staves[n - 1].Bottom) { under.Add((part.Top, part.Bottom, part.Count, (part.Left + part.Right) / 2, row => part.Owner = row)); continue; }
                int upperRow = GapOf(middle);
                bool blocked = between[upperRow].Any(t => t.Owner == Row + upperRow && t.Bottom < part.Top);
                // (a staff with no ink over or under it, beside a shorter staff, is not near it)
                int? lowerInk = Reach(topOf[upperRow + 1], part, below: true), upperInk = Reach(bottomOf[upperRow], part, below: false);
                int toLower = lowerInk ?? (upperInk == null ? _staves[upperRow + 1].Top - part.Bottom : int.MaxValue);
                int toUpper = upperInk ?? (lowerInk == null ? part.Top - _staves[upperRow].Bottom : int.MaxValue);
                part.Owner = Row + (blocked || toLower < toUpper ? upperRow + 1 : upperRow);
            }
            texts.AddRange(marks);

            // Under the last staff: bands of what lies there, each at most 6 spacings high and at most 3 spacings below the
            // one before; a taller band (a photo or art drawn in the picture) and all below it stay out.
            var last = _staves[n - 1];
            int end = box[n - 1].Bottom;
            // (specks, a photo's or a texture's, start no band: they go with the bands they lie in; what is not under the
            // staff at all, as art beside it, is not its lyrics)
            int speck = Math.Max(4, (int)(last.Spacing * last.Spacing / 8));
            var specks = under.Where(item => item.Count < speck).ToList();
            under.RemoveAll(item => item.Count < speck || item.Middle < last.Left || item.Middle > last.Right);
            under.Sort((a, b) => a.Top.CompareTo(b.Top));
            int bridge = Math.Max(2, (int)(last.Spacing / 6)), besideFrom = end + 1;
            for (int i = 0; i < under.Count;)
            {
                int j = i, bandTop = under[i].Top, bandBottom = under[i].Bottom;
                while (j + 1 < under.Count && under[j + 1].Top <= bandBottom + bridge + 1) bandBottom = Math.Max(bandBottom, under[++j].Bottom);
                int below = Math.Max(bandTop, end + 1);
                if (bandBottom > end && (below - end - 1 > Far * last.Spacing || bandBottom - below + 1 > Lyric * last.Spacing))
                {
                    besideFrom = Math.Min(besideFrom, bandTop); // what of it lies beside its lowest notes (below)
                    break;
                }
                for (int m = i; m <= j; m++) under[m].Give(Row + n - 1);
                end = Math.Max(end, bandBottom);
                i = j + 1;
            }
            foreach (var item in specks) if (item.Bottom <= end) item.Give(Row + n - 1);


            // 5. Every pixel's owner: a component's, its shadow's strong pixel's; the lyrics printed over a photo (one
            // tall band with it) reach at least as far as the shortest row above them.
            foreach (var text in texts) foreach (var c in text.Members) _parts[c].Owner = text.Owner;
            for (int c = 1; c < parts.Length; c++) if (code[c] == 0) code[c] = (byte)parts[c].Owner;
            var lowest = box.Select(b => b.Bottom).ToArray();
            for (int y = 0; y < height; y++)
                for (int x = 0, at = y * width; x < width; x++, at++)
                {
                    int c = label[at];
                    if (c <= 0 || parts[c].Lo >= 0) continue;
                    byte o = owner[at] = code[c];
                    if (o >= Row && y > lowest[o - Row]) lowest[o - Row] = y;
                }
            var lastSpan = Span(last, width); // (as before content pictures: what lies far beside the staff is no lyrics)
            for (int y = besideFrom; y <= end; y++)
                for (int at = y * width + lastSpan.Left, stop = y * width + lastSpan.Right; at <= stop; at++)
                    if (label[at] > 0 && owner[at] == Nobody && parts[label[at]].Lo < 0) owner[at] = (byte)(Row + n - 1);
            if (n > 1)
            {
                double least = Enumerable.Range(0, n - 1).Min(k => (lowest[k] + 1 - _staves[k].Bottom) / _staves[k].Spacing);
                int reach = Math.Min(height - 1, last.Bottom + (int)Math.Round(least * last.Spacing) - 1);
                for (int y = end + 1; y <= reach; y++)
                    for (int at = y * width + lastSpan.Left, stop = y * width + lastSpan.Right; at <= stop; at++)
                        if (label[at] > 0 && owner[at] == Nobody && parts[label[at]].Lo < 0) owner[at] = (byte)(Row + n - 1);
            }

            // A letter of a row's lyrics touching the next staff's stem came with that staff's ink: it goes back.
            for (int k = 0; k < n - 1; k++)
                foreach (var text in between[k])
                    if (text.Words && text.Owner == Row + k) Return(text, k);


            // 6. Rows: their own ink's reach (shadows too), and what is painted over within it.
            box = Boxes();
            var own = _staves.Select((s, k) => new RowExtent((s.Top - box[k].Top) / s.Spacing, (box[k].Bottom + 1 - s.Bottom) / s.Spacing)).ToArray();
            var rows = _staves.Select((staff, k) =>
            {
                int room = Margin(staff);
                int top = Math.Max(0, Math.Min(box[k].Top - room, staff.Top)), bottom = Math.Min(height, Math.Max(box[k].Bottom + 1 + room, staff.Bottom + 1));
                int left = Math.Max(0, box[k].Left - width / 60), right = Math.Min(width - 1, box[k].Right + width / 60);
                var area = new Int32Rect(left, top, right - left + 1, bottom - top);
                return new ScoreRow(area, staff.Top, staff.Bottom);
            }).ToArray();

            return new(rows, own, [.. _staves.Select(s => s.Spacing)], _owner, _width);
        }

        // The staff whose lines' rows hold y, or -1.
        private int Staffed(double y)
        {
            for (int k = 0; k < _n; k++) if (y >= _staves[k].Top && y <= _staves[k].Bottom) return k;
            return -1;
        }

        // How far the part lies from a staff's ink over or under it (its columns and 2 spacings on either side); null
        // when that ink is not in those columns.
        private int? Reach(int[] profile, Text part, bool below)
        {
            int room = (int)(2 * SpacingNear((part.Top + part.Bottom) / 2.0)), best = int.MaxValue;
            for (int x = Math.Max(0, part.Left - room); x <= Math.Min(_width - 1, part.Right + room); x++)
            {
                if (below ? profile[x] == int.MaxValue : profile[x] < 0) continue;
                best = Math.Min(best, below ? profile[x] - part.Bottom : part.Top - profile[x]);
            }
            return best == int.MaxValue ? null : best;
        }

        private Text Seal(Text text)
        {
            text.Top = text.Members.Min(c => _parts[c].Top);
            text.Bottom = text.Members.Max(c => _parts[c].Bottom);
            text.Left = text.Members.Min(c => _parts[c].Left);
            text.Right = text.Members.Max(c => _parts[c].Right);
            text.Count = text.Members.Sum(c => _parts[c].Count);
            foreach (var c in text.Members) _parts[c].Line = text;
            return text;
        }

        // Syllables (design 5h): pieces on the same columns, over and under each other with a small gap or none (a
        // letter's parts, its final consonant, a shadow's lower edge, a fermata's dot), are one.
        private List<Text> Syllables(List<int> loose)
        {
            // How far apart a piece may lie from the one over or under it: a quarter of its height, of a flat piece (a
            // letter's part, as wide as it is tall or wider) up to half (not of a dash or a bracket's line, though).
            static double Leeway(Part p) => 0.25 * Math.Max(p.Height, Math.Min(p.Width, 2 * p.Height));
            // Which pieces lie in each 32-pixel column band, so only neighbours are compared.
            const int Cell = 32;
            var group = loose.ToDictionary(c => c, c => c);
            int Root(int c) { while (group[c] != c) c = group[c] = group[group[c]]; return c; }
            var columns = new Dictionary<int, List<int>>();
            foreach (var c in loose)
                for (int cx = _parts[c].Left / Cell; cx <= _parts[c].Right / Cell; cx++)
                    (columns.TryGetValue(cx, out var list) ? list : columns[cx] = []).Add(c);
            foreach (var list in columns.Values)
            {
                list.Sort((a, b) => _parts[a].Top.CompareTo(_parts[b].Top));
                for (int i = 0; i < list.Count; i++)
                {
                    var a = _parts[list[i]];
                    double most = 0.4 * SpacingNear(a.Bottom), gapLimit = Math.Min(most, Leeway(a));
                    for (int j = i + 1; j < list.Count; j++)
                    {
                        var b = _parts[list[j]];
                        int gap = b.Top - a.Bottom - 1;
                        if (gap > Math.Max(1, most)) break;
                        // (a speck in the gap does not hide the larger piece under it)
                        if (gap > Math.Max(1, Math.Max(gapLimit, Math.Min(most, Leeway(b))))) continue;
                        // (one over the other, not overlapping: a mark drawn into a letter's rows stays apart; a shadow's edge
                        // under the letter, within its columns, is the letter's)
                        if (gap < -2 && !(b.Left >= a.Left - 2 && b.Right <= a.Right + 4 && b.Bottom > a.Bottom) && !a.Pale && !b.Pale) continue;
                        if (Math.Min(a.Right, b.Right) < Math.Max(a.Left, b.Left)) continue;
                        group[Root(list[i])] = Root(list[j]);
                    }
                }
            }
            JoinFlatVowels(loose, Root, (upper, lower) => group[Root(lower)] = Root(upper));
            var syllables = loose.GroupBy(Root).Select(members => Seal(new Text([.. members]))).ToList();
            // A mark right under a letter (a tuplet's number under the title, a fermata under a slide number) makes it
            // much taller than the letters beside it: what lies under their rows is taken apart again. (The letters'
            // height leaves out dashes and specks; a final consonant keeps its letter as tall as the others.)
            foreach (var row in Rows(syllables))
            {
                if (row.Count < 3) continue;
                int highest = row.Max(s => s.Height);
                var letters = row.Where(s => 2 * s.Height >= highest).Select(s => s.Height).Order().ToArray();
                int tall = letters[letters.Length / 2];
                foreach (var syllable in row.Where(s => s.Members.Count > 1 && s.Height * 20 > tall * 27).ToList())
                {
                    var apart = syllable.Members.Where(c => _parts[c].Top >= syllable.Top + 0.8 * tall).ToList();
                    if (apart.Count == 0 || apart.Count == syllable.Members.Count) continue;
                    syllable.Members.RemoveAll(apart.Contains);
                    Seal(syllable);
                    syllables.AddRange(apart.Select(c => Seal(new Text([c]))));
                }
            }
            return syllables;
        }

        // The share of a mark's own ink pixels that are of a saturated colour; most of them is a title's or a logo's.
        private double Share(Text mark)
        {
            var map = _colored ??= Colored(_image, _width, Math.Clamp(_staves[0].Top, 1, _height));
            int all = 0, colored = 0;
            for (int y = mark.Top; y <= mark.Bottom; y++)
                for (int x = mark.Left, at = y * _width + mark.Left; x <= mark.Right; x++, at++)
                    if (_label[at] > 0 && mark.Members.Contains(_label[at]))
                    {
                        all++;
                        if (at < map.Length && map[at]) colored++;
                    }
            return all == 0 ? 0 : colored / (double)all;
        }

        private bool Colorful(Text mark) => Share(mark) >= 0.5;

        // A consonant over a flat vowel (the syllables eu, seu, deu, teu, nu: the flat eu or u under the consonant) is one
        // syllable even where the font leaves them farther apart than the pieces' own heights allow (design 5h, rounds 8
        // and 9). The lower piece is flat, its top row a bar across nearly all its width with nothing hanging at one end
        // (not a bracket's arm or a fermata's arch); it is no wider than 1.5 consonants nor than the line's syllables; it
        // lies under a consonant, the upper part of a letter (not a whole letter, not a bar: a tenuto or a slur under
        // geu, deu or reul is a mark) whose syllable has no flat vowel yet; and the gap is no more than about the line's
        // gaps inside its syllables, or than a stacked consonant's height allows (a large bold seu leaves half a consonant
        // between them).
        private void JoinFlatVowels(List<int> loose, Func<int, int> root, Action<int, int> join)
        {
            Text Box(List<int> members) => new(members)
            {
                Top = members.Min(c => _parts[c].Top), Bottom = members.Max(c => _parts[c].Bottom),
                Left = members.Min(c => _parts[c].Left), Right = members.Max(c => _parts[c].Right),
            };
            bool Bar(Part p) => p.Width >= 3 * p.Height;
            bool Over(Part a, Part b) => Math.Min(a.Right, b.Right) - Math.Max(a.Left, b.Left) + 1 >= 0.6 * Math.Min(a.Width, b.Width);
            var groups = loose.GroupBy(root).Select(g => Box([.. g])).ToList();
            // The syllables of more than one piece, row by row: their widths and the gaps between their stacked pieces.
            List<int> Gaps(Text s)
            {
                var pieces = s.Members.Select(c => _parts[c]).OrderBy(p => p.Top).ToList();
                var gaps = new List<int>();
                for (int i = 1; i < pieces.Count; i++)
                    if (pieces.Take(i).LastOrDefault(p => Over(p, pieces[i]) && p.Bottom < pieces[i].Top) is { } above) gaps.Add(pieces[i].Top - above.Bottom - 1);
                return gaps;
            }
            (double Width, double Gap)? Typical(IEnumerable<Text> made)
            {
                // (a syllable is about as wide as the letters are tall: the upper quarter of the heights of the line's pieces;
                // the gaps, those between stacked pieces of a letter, or a tenth of a letter where it has none)
                var list = made.ToList();
                if (list.Count < 8) return null;
                var heights = list.Select(s => s.Height).Order().ToList();
                var gaps = list.Where(s => s.Members.Count > 1).SelectMany(Gaps).Order().ToList();
                double width = heights[3 * heights.Count / 4];
                return (width, gaps.Count > 0 ? gaps[gaps.Count / 2] : width / 10);
            }
            var rows = Rows(groups);
            var everywhere = Typical(groups);
            var byRow = rows.ToDictionary(row => row, row => Typical(row) ?? everywhere);
            var home = rows.SelectMany(row => row.Select(s => (s, row))).ToDictionary(e => e.s, e => e.row);
            var of = groups.SelectMany(s => s.Members.Select(c => (c, s))).ToDictionary(e => e.c, e => e.s);
            foreach (var lower in loose.Where(c => of[c].Members.Count == 1).OrderBy(c => _parts[c].Top))
            {
                var b = _parts[lower];
                if (b.Width < 1.5 * b.Height || b.Width > 6 * SpacingNear(b.Top)) continue;
                // the consonant right over it
                int upper = loose.Where(c => c != lower && _parts[c].Bottom < b.Top && Over(_parts[c], b))
                    .OrderByDescending(c => _parts[c].Bottom).FirstOrDefault();
                if (upper == 0) continue;
                var a = _parts[upper];
                var syllable = of[upper];
                // (the consonant begins its letter: nothing of it over the consonant, as over the final consonant of reul)
                if (4 * a.Height < a.Width || a.Height > 4.5 * SpacingNear(a.Bottom) || b.Width > 1.5 * a.Width || a.Width > 1.5 * b.Width
                    || syllable.Members.Any(c => Bar(_parts[c]) && _parts[c].Width >= 0.5 * a.Width)
                    || (home.TryGetValue(syllable, out var row) ? byRow[row] : everywhere) is not var (width, gap)
                    || b.Width > width || a.Height > 0.7 * width || b.Top - a.Bottom - 1 > Math.Max(3 * gap + 1, 0.8 * a.Height)
                    || loose.Any(c => c != upper && _parts[c].Bottom < a.Top && _parts[c].Bottom >= a.Top - gap - 2 && Over(_parts[c], a))) continue;
                int bar = 0, barBottom = Math.Min(b.Bottom, b.Top + Math.Max(1, b.Height / 4)), under = 0;
                long middle = 0;
                for (int x = b.Left; x <= b.Right; x++)
                {
                    for (int y = b.Top; y <= barBottom; y++)
                        if (_label[y * _width + x] == lower) { bar++; break; }
                    for (int y = barBottom + 1; y <= b.Bottom; y++)
                        if (_label[y * _width + x] == lower) { under++; middle += x; }
                }
                if (bar < 0.7 * b.Width || under > 0 && Math.Abs(middle / (double)under - (b.Left + b.Right) / 2.0) > 0.2 * b.Width) continue;
                join(upper, lower);
                syllable.Members.Add(lower);
                of[lower] = syllable;
            }
        }

        // The picture's lyric letter height in spacings: of the rows of syllables between the staves and under the last
        // one (2 or more syllables, at most 4.5 spacings high), the tallest row's upper-quarter height (the lyrics, not
        // smaller letters beside them on its rows); null when there is none.
        private double? LyricHeight(List<Text> syllables)
        {
            var last = _staves[_n - 1];
            var candidates = syllables.Where(s =>
            {
                double middle = (s.Top + s.Bottom) / 2.0, spacing = SpacingNear(middle);
                return middle > _staves[0].Bottom && middle < last.Bottom + Lyric * last.Spacing && Staffed(middle) < 0
                    && s.Height >= 0.5 * spacing && s.Height <= 4.5 * spacing && s.Width < _width / 4;
            }).OrderBy(s => s.Top).ToList();
            double? best = null;
            foreach (var row in Rows(candidates))
            {
                if (row.Count < 2) continue;
                var heights = row.Select(s => s.Height / SpacingNear((s.Top + s.Bottom) / 2.0)).Order().ToArray();
                double middle = heights[3 * heights.Length / 4];
                if (best == null || middle > best) best = middle;
            }
            return best;
        }

        // Pieces on the same rows (each overlapping another by half the shorter one's height), top to bottom.
        private static List<List<Text>> Rows(List<Text> pieces)
        {
            var rows = new List<List<Text>>();
            foreach (var piece in pieces.OrderBy(p => p.Top))
            {
                var home = rows.FirstOrDefault(row => row.Any(o => 2 * (Math.Min(piece.Bottom, o.Bottom) - Math.Max(piece.Top, o.Top) + 1) >= Math.Min(piece.Height, o.Height)));
                if (home != null) home.Add(piece); else rows.Add([piece]);
            }
            return rows;
        }

        // Lines of lyrics (or a title): the lyric-sized syllables on the same rows, however few.
        private List<Text> LyricLines(List<Text> words) =>
            [.. Rows(words).Select(row => Seal(new Text([.. row.SelectMany(s => s.Members)]) { Words = true }))];

        // A letter of the line touching a stem of the next staff: the staff's ink within the line's rows that leaves
        // them down one stem goes back to the line from where it meets the stem, when it lies beside the line's letters.
        // A flag (one side of a stem running up through it), a beam (more stems) and the stem itself stay.
        private void Return(Text line, int k)
        {
            byte from = (byte)(Row + k + 1), to = (byte)(Row + k);
            var lower = _staves[k + 1];
            int top = line.Top, bottom = Math.Min(_height - 2, line.Bottom + Math.Max(2, (int)(0.15 * lower.Spacing)));
            if (bottom < top) return;
            int stem = Math.Max(3, (int)(0.3 * lower.Spacing)), w = _width, rows = bottom - top + 1;
            var letters = line.Members.Select(c => _parts[c]).Where(p => p.Height * 2 >= line.Bottom - line.Top + 1).ToList();
            int letter = letters.Count == 0 ? line.Bottom - line.Top + 1 : letters.Select(p => p.Height).Order().ElementAt(letters.Count / 2);
            // Which piece each pixel of the rows belongs to (0 none).
            var pieceOf = new int[w * rows];
            int pieces = 0;
            bool Mine(int x, int y) => x >= 0 && x < w && y >= top && y <= bottom && _label[y * w + x] > 0 && _owner[y * w + x] == from;
            bool In(int x, int y, int id) => x >= 0 && x < w && y >= top && y <= bottom && pieceOf[(y - top) * w + x] == id;
            for (int y = top; y <= bottom; y++)
                for (int x = 0; x < w; x++)
                {
                    if (pieceOf[(y - top) * w + x] != 0 || !Mine(x, y)) continue;
                    int id = ++pieces;
                    pieceOf[(y - top) * w + x] = id;
                    var piece = new List<int> { y * w + x };
                    for (int i = 0; i < piece.Count; i++)
                    {
                        int px = piece[i] % w, py = piece[i] / w;
                        for (int dy = -1; dy <= 1; dy++)
                            for (int dx = -1; dx <= 1; dx++)
                            {
                                int nx = px + dx, ny = py + dy;
                                if (!Mine(nx, ny) || pieceOf[(ny - top) * w + nx] != 0) continue;
                                pieceOf[(ny - top) * w + nx] = id;
                                piece.Add(ny * w + nx);
                            }
                    }
                    // Where it leaves the rows downward: one run no wider than a stem.
                    int s0 = int.MaxValue, s1 = -1, exits = 0, pieceTop = bottom;
                    foreach (var at in piece)
                    {
                        int px = at % w, py = at / w;
                        pieceTop = Math.Min(pieceTop, py);
                        if (py != bottom) continue;
                        bool down = false;
                        for (int dx = -1; dx <= 1 && !down; dx++)
                            down = px + dx >= 0 && px + dx < w && _label[at + w + dx] > 0 && _owner[at + w + dx] == from;
                        if (!down) continue;
                        exits++;
                        s0 = Math.Min(s0, px);
                        s1 = Math.Max(s1, px);
                    }
                    if (exits == 0 || s1 - s0 + 1 > stem || exits != s1 - s0 + 1) continue;
                    // Up the stem to where the piece widens: there the letter meets it.
                    int contact = -1;
                    var stemRuns = new Dictionary<int, (int Left, int Right)>();
                    for (int yy = bottom; yy >= pieceTop; yy--)
                    {
                        int l = -1;
                        for (int xx = s0 - 1; xx <= s1 + 1; xx++) if (In(xx, yy, id)) { l = xx; break; }
                        if (l < 0) break;
                        while (In(l - 1, yy, id)) l--;
                        int r = l;
                        while (In(r + 1, yy, id)) r++;
                        if (r - l + 1 > stem + 1) { contact = yy; break; }
                        stemRuns[yy] = (l, r);
                        (s0, s1) = (l, r);
                    }
                    if (contact < 0) continue; // a stem alone
                    bool OnStem(int at) => stemRuns.TryGetValue(at / w, out var run) && at % w >= run.Left && at % w <= run.Right;
                    int left = int.MaxValue, right = -1, high = int.MaxValue, low = -1;
                    foreach (var at in piece)
                    {
                        if (OnStem(at)) continue;
                        left = Math.Min(left, at % w); right = Math.Max(right, at % w);
                        high = Math.Min(high, at / w); low = Math.Max(low, at / w);
                    }
                    if (right < 0) continue;
                    bool through = true;
                    for (int yy = pieceTop; yy <= contact && through; yy++)
                    {
                        bool inked = false;
                        for (int xx = s0; xx <= s1 && !inked; xx++) inked = In(xx, yy, id);
                        through = inked;
                    }
                    if (through && (left >= s0 - 1 || right <= s1 + 1)) continue; // a flag: on one side of a stem running up through it
                    bool beside = false;
                    foreach (var c in line.Members)
                    {
                        var p = _parts[c];
                        if (p.Bottom >= high && p.Top <= low && Math.Max(p.Left, left) - Math.Min(p.Right, right) <= 1.5 * letter) { beside = true; break; }
                    }
                    if (!beside) continue;
                    // The letter is what lies within the line's rows (or above where it meets the stem) and off the stem.
                    int upTo = Math.Max(line.Bottom, contact);
                    foreach (var at in piece)
                        if (at / w <= upTo || !OnStem(at)) _owner[at] = to;
                }
        }

        // Each row's ink (shadows too): its top, bottom, left and right pixel. Each shadow or glow pixel first takes the
        // owner of the strong pixel it hangs on.
        private (int Top, int Bottom, int Left, int Right)[] Boxes()
        {
            var boxes = Enumerable.Repeat((Top: int.MaxValue, Bottom: -1, Left: int.MaxValue, Right: -1), _n).ToArray();
            var owner = _owner;
            var label = _label;
            for (int y = 0; y < _height; y++)
                for (int x = 0, at = y * _width; x < _width; x++, at++)
                {
                    if (label[at] < 0) { int source = at; while (label[source] < 0) source = -label[source] - 1; owner[at] = owner[source]; }
                    int k = owner[at] - Row;
                    if (k < 0) continue;
                    ref var box = ref boxes[k];
                    if (y < box.Top) box.Top = y;
                    if (y > box.Bottom) box.Bottom = y;
                    if (x < box.Left) box.Left = x;
                    if (x > box.Right) box.Right = x;
                }
            for (int k = 0; k < _n; k++)
                if (boxes[k].Bottom < 0) boxes[k] = (_staves[k].Top, _staves[k].Bottom, _staves[k].Left, _staves[k].Right);
            return boxes;
        }
    }

    // The little room kept around a row's ink.
    private static int Margin(Staff staff) => Math.Max(1, (int)Math.Round(0.25 * staff.Spacing));

    // Notes, ledger lines and the clef touch the staff from above; a title sits apart, past a clear gap.
    private static int Above(byte[] pixels, int width, Staff staff, int limit, bool[] ink)
    {
        var span = Span(staff, width);
        int quiet = (span.Right - span.Left) / 200, blank = Math.Max(3, (int)(1.5 * staff.Spacing)), run = 0, reach = 0;
        int most = (int)(5 * staff.Spacing);
        for (int d = 1; d <= most && staff.Top - d >= limit; d++)
        {
            if (InkIn(pixels, width, span, staff.Top - d, ink) > quiet) { reach = d; run = 0; }
            else if (++run >= blank) break;
        }
        return reach;
    }

    // Where a row's notes, lyrics and title can be: the staff's width and a little more. Everything is measured
    // inside it, so a background picture or a slide button beside the score counts as empty slide.
    private static (int Left, int Right) Span(Staff staff, int width)
    {
        int pad = Math.Max((int)(4 * staff.Spacing), width / 60);
        return (Math.Max(0, staff.Left - pad), Math.Min(width - 1, staff.Right + pad));
    }

    private static int InkIn(byte[] pixels, int width, (int Left, int Right) span, int y, bool[] ink)
    {
        int count = 0;
        for (int x = span.Left, row = y * width; x <= span.Right; x++) if (ink[pixels[row + x]]) count++;
        return count;
    }

    // A row ends after the lyrics under its staff when a clear gap follows (the next row, slide buttons, a footer),
    // at the last ink before that gap; at limit at the latest.
    private static int End(byte[] pixels, int width, int limit, Staff staff, bool[] ink)
    {
        var span = Span(staff, width);
        int quiet = (span.Right - span.Left) / 200, blank = Math.Max(3, (int)(2 * staff.Spacing)), run = 0, last = staff.Bottom;
        bool lyrics = false;
        for (int y = staff.Bottom + (int)staff.Spacing; y < limit; y++)
        {
            if (InkIn(pixels, width, span, y, ink) > quiet) { lyrics = true; run = 0; last = y + 1; }
            else if (lyrics && ++run >= blank) return last;
        }
        return lyrics ? last : Math.Min(limit, staff.Bottom + (int)(2 * staff.Spacing)); // no lyrics: just the notes under the staff
    }

    // Left and right of the notes is empty slide; leaving it out makes the score larger on the camera.
    private static Int32Rect Trim(byte[] pixels, int width, (int Left, int Right) span, int top, int bottom, bool[] ink)
    {
        int left = span.Right + 1, right = -1;
        for (int y = top; y < bottom; y++)
            for (int x = span.Left, row = y * width; x <= span.Right; x++)
                if (ink[pixels[row + x]]) { if (x < left) left = x; if (x > right) right = x; }
        if (right < left) return new(span.Left, top, span.Right - span.Left + 1, bottom - top);
        int pad = width / 60;
        left = Math.Max(0, left - pad);
        right = Math.Min(width - 1, right + pad);
        return new(left, top, right - left + 1, bottom - top);
    }

    // The cut between two staves: the middle of the emptiest stretch between the lyrics above and the notes below.
    private static int Gap(byte[] pixels, int width, Staff above, Staff below, bool[] ink)
    {
        int from = above.Bottom + (int)(2 * above.Spacing), to = below.Top - (int)below.Spacing;
        if (to <= from) return (above.Bottom + below.Top) / 2;
        var (a, b) = (Span(above, width), Span(below, width));
        var span = (Math.Min(a.Left, b.Left), Math.Max(a.Right, b.Right));
        var counts = new int[to - from];
        for (int y = from; y < to; y++) counts[y - from] = InkIn(pixels, width, span, y, ink);
        int quiet = counts.Min() + (span.Item2 - span.Item1) / 200;
        int bestStart = 0, bestLength = 0;
        for (int i = 0; i < counts.Length;)
        {
            if (counts[i] > quiet) { i++; continue; }
            int start = i;
            while (i < counts.Length && counts[i] <= quiet) i++;
            if (i - start >= bestLength) { bestStart = start; bestLength = i - start; } // the lower one on a tie
        }
        return from + bestStart + bestLength / 2;
    }
}
