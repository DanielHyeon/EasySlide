using System.Xml.Linq;
using System.IO.Compression;

namespace Axicultus.ScoreEditor;

public sealed class MelodyScore
{
    private readonly XDocument document;
    private readonly XElement staff;
    private string? archivePath;
    private string scoreEntry = "score.mscx";
    private static readonly int[] SharpTpc = [14, 21, 16, 23, 18, 13, 20, 15, 22, 17, 24, 19];

    private MelodyScore(XDocument source)
    {
        document = source;
        if ((string?)source.Root?.Attribute("version") is not { } version || !version.StartsWith("4.", StringComparison.Ordinal) ||
            source.Root?.Name != "museScore" || source.Root.Element("Score") is not { } score)
            throw new InvalidDataException("MuseScore 4 악보를 선택해 주세요.");
        var staves = score.Elements("Staff").ToArray();
        if (staves.Length != 1 || score.Elements("Part").Count() != 1 ||
            score.Descendants("defaultClef").Concat(score.Descendants("concertClefType")).Any(c => c.Value != "G") ||
            score.Descendants("transposeChromatic").Any(c => c.Value != "0"))
            throw new InvalidDataException("편집할 수 있는 악보는 높은음자리표의 단선율 한 보표입니다.");
        staff = staves[0];
        if (!staff.Elements("Measure").Any()) throw new InvalidDataException("악보에 마디가 없습니다.");
        foreach (var measure in Measures)
        {
            var voices = measure.Elements("voice").ToArray();
            if (voices.Length != 1 || measure.Attribute("len") != null ||
                measure.Descendants("TimeSig").Any(t => (string?)t.Element("sigN") != "4" || (string?)t.Element("sigD") != "4") ||
                voices[0].Elements().Any(c => c.Name.LocalName is not ("Chord" or "Rest" or "TimeSig" or "KeySig" or "Clef" or "BarLine")) ||
                measure.Descendants("KeySig").Any(k => (int?)k.Element("accidental") is not (null or 0)))
                throw new InvalidDataException("현재 편집 범위는 다장조 4/4박자, 한 성부입니다.");
            var events = Events(measure);
            if (events.Any(e => e.Name == "Chord" && (e.Elements("Note").Count() != 1 || Pitch(e) is not (>= 0 and <= 127))) ||
                events.Any(e => e.Descendants("Spanner").Any() || e.Element("Tuplet") != null || e.Element("grace") != null) ||
                events.Sum(e => Duration(e).Units) != 16384)
                throw new InvalidDataException("단선율 음표와 쉼표로 채운 마디를 열 수 있습니다.");
        }
    }

    public string Title => (string?)document.Root!.Element("Score")!.Elements("metaTag")
        .FirstOrDefault(e => (string?)e.Attribute("name") == "workTitle") ?? "";
    public XElement[] Measures => staff.Elements("Measure").ToArray();
    internal void RestoreMeasure(int bar, XElement? original)
    {
        if (original is null) Measures[bar].Remove();
        else Measures[bar].ReplaceWith(original);
    }
    public static XElement[] Events(XElement measure) => measure.Element("voice")!.Elements()
        .Where(e => e.Name == "Chord" || e.Name == "Rest").ToArray();
    public static int? Pitch(XElement entry) => (int?)entry.Element("Note")?.Element("pitch");
    private static XElement? FirstVerse(XElement chord) =>
        chord.Elements("Lyrics").FirstOrDefault(e => ((int?)e.Element("no") ?? 0) == 0);
    public static string Lyric(XElement entry) => (string?)FirstVerse(entry)?.Element("text") ?? "";
    public static ScoreDuration Duration(XElement entry)
    {
        string type = (string?)entry.Element("durationType") ?? "";
        return new(type == "measure" ? "whole" : type, (int?)entry.Element("dots") ?? 0);
    }

    public static MelodyScore Create(string title)
    {
        var score = new XElement("Score", new XElement("Division", 480),
            new XElement("metaTag", new XAttribute("name", "workTitle"), title),
            new XElement("Part", new XElement("Staff", new XAttribute("id", 1),
                new XElement("StaffType", new XAttribute("group", "pitched"), new XElement("name", "stdNormal"))),
                new XElement("trackName", "Melody"), new XElement("Instrument", new XAttribute("id", "piano"),
                    new XElement("longName", "Melody"), new XElement("instrumentId", "keyboard.piano"),
                    new XElement("Channel", new XElement("program", new XAttribute("value", 0))))),
            new XElement("Staff", new XAttribute("id", 1), new XElement("Measure", new XElement("voice",
                new XElement("TimeSig", new XElement("sigN", 4), new XElement("sigD", 4)), NewEvent(new("whole"), null, "")))));
        return new(new XDocument(new XElement("museScore", new XAttribute("version", "4.00"), score)));
    }

    public void AddMeasure() => staff.Add(new XElement("Measure", new XElement("voice", NewEvent(new("whole"), null, ""))));

    public void SetLyric(int bar, int index, string text) => SetLyric(Events(Measures[bar])[index], text);

    private static void SetLyric(XElement chord, string text)
    {
        if (chord.Name != "Chord") throw new InvalidOperationException("가사를 넣을 음표를 선택해 주세요.");
        var lyrics = FirstVerse(chord);
        if (lyrics == null)
        {
            if (text.Length > 0) chord.Element("Note")!.AddBeforeSelf(new XElement("Lyrics", new XElement("text", text)));
        }
        else if (text.Length == 0) lyrics.Remove();
        else lyrics.SetElementValue("text", text);
    }

    public void SetEvent(int bar, int index, ScoreDuration duration, int? pitch, string lyric)
    {
        if (pitch is < 0 or > 127) throw new ArgumentOutOfRangeException(nameof(pitch));
        var events = Events(Measures[bar]);
        var target = events[index];
        int available = Duration(target).Units;
        int end = index;
        while (available < duration.Units && end + 1 < events.Length && events[end + 1].Name == "Rest")
            available += Duration(events[++end]).Units;
        if (duration.Units > available) throw new InvalidOperationException("다음 음표 또는 마디선을 넘어갑니다. 짧은 음가를 선택해 주세요.");
        // Preserve the selected note's articulations, extra lyric verses and engraving overrides on ordinary edits.
        var replacement = NewEvent(duration, pitch, lyric);
        if (target.Name == replacement.Name)
        {
            replacement = new XElement(target);
            replacement.SetElementValue("durationType", duration.Type);
            replacement.SetElementValue("dots", duration.Dots == 0 ? null : duration.Dots);
            replacement.Element("duration")?.Remove();
            if (pitch.HasValue)
            {
                var note = replacement.Element("Note")!;
                if (Pitch(target) != pitch)
                {
                    note.SetElementValue("pitch", pitch.Value);
                    note.SetElementValue("tpc", SharpTpc[pitch.Value % 12]);
                    note.Element("tpc2")?.Remove();
                    note.Element("Accidental")?.Remove();
                }
                SetLyric(replacement, lyric);
            }
        }
        var nodes = new List<XElement> { replacement };
        int left = available - duration.Units;
        foreach (var type in ScoreDuration.Types.Skip(2))
        {
            var rest = new ScoreDuration(type);
            while (left >= rest.Units) { nodes.Add(NewEvent(rest, null, "")); left -= rest.Units; }
        }
        if (left != 0) throw new InvalidOperationException("이 길이의 쉼표는 분할할 수 없습니다.");
        for (int i = index + 1; i <= end; i++) events[i].Remove();
        target.ReplaceWith(nodes);
    }

    private static XElement NewEvent(ScoreDuration duration, int? pitch, string lyric)
    {
        var e = new XElement(pitch.HasValue ? "Chord" : "Rest");
        if (duration.Dots != 0) e.Add(new XElement("dots", duration.Dots));
        e.Add(new XElement("durationType", duration.Type));
        if (pitch is int midi)
        {
            if (lyric.Length > 0) e.Add(new XElement("Lyrics", new XElement("text", lyric)));
            e.Add(new XElement("Note", new XElement("pitch", midi), new XElement("tpc", SharpTpc[midi % 12])));
        }
        return e;
    }

    public static MelodyScore Open(string path)
    {
        if (!IsArchive(path)) return new(XDocument.Load(path));
        using var archive = ZipFile.OpenRead(path);
        string? entryName = null;
        if (archive.GetEntry("META-INF/container.xml") is { } manifest)
        {
            using var stream = manifest.Open();
            entryName = XDocument.Load(stream).Descendants().Where(e => e.Name.LocalName == "rootfile")
                .Select(e => (string?)e.Attribute("full-path")).FirstOrDefault(p => p?.EndsWith(".mscx", StringComparison.OrdinalIgnoreCase) == true);
        }
        var entry = entryName == null ? archive.Entries.FirstOrDefault(e => e.Name == e.FullName &&
            e.Name.EndsWith(".mscx", StringComparison.OrdinalIgnoreCase)) : archive.GetEntry(entryName);
        if (entry == null) throw new InvalidDataException("압축 파일에서 본 악보를 찾을 수 없습니다.");
        using var input = entry.Open();
        return new(XDocument.Load(input)) { archivePath = path, scoreEntry = entry.FullName };
    }

    private static bool IsArchive(string path) => Path.GetExtension(path).Equals(".mscz", StringComparison.OrdinalIgnoreCase);
    public void Save(string path)
    {
        string temp = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            if (IsArchive(path))
            {
                if (archivePath != null) File.Copy(archivePath, temp);
                using var archive = ZipFile.Open(temp, archivePath != null ? ZipArchiveMode.Update : ZipArchiveMode.Create);
                if (archivePath != null)
                {
                    archive.GetEntry(scoreEntry)!.Delete();
                    if (archive.GetEntry("Thumbnails/thumbnail.png") is { } thumbnail)
                    {
                        thumbnail.Delete();
                        if (archive.GetEntry("META-INF/container.xml") is { } manifestEntry)
                        {
                            XDocument manifest;
                            using (var input = manifestEntry.Open()) manifest = XDocument.Load(input);
                            manifest.Descendants().Where(e => (string?)e.Attribute("full-path") == "Thumbnails/thumbnail.png").Remove();
                            manifestEntry.Delete();
                            using var outputManifest = archive.CreateEntry("META-INF/container.xml").Open();
                            manifest.Save(outputManifest);
                        }
                    }
                }
                else
                {
                    using var manifest = archive.CreateEntry("META-INF/container.xml").Open();
                    new XDocument(new XElement("container", new XElement("rootfiles",
                        new XElement("rootfile", new XAttribute("full-path", scoreEntry))))).Save(manifest);
                }
                using var output = archive.CreateEntry(scoreEntry).Open();
                document.Save(output);
            }
            else document.Save(temp);
            File.Move(temp, path, true);
            if (IsArchive(path)) archivePath = path;
        }
        finally { File.Delete(temp); }
    }
}
