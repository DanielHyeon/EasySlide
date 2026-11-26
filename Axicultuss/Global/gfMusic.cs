using System;
using System.Windows.Forms;
using Axicultus.Localization;
using Axicultus.Module;

namespace Axicultus;

internal unsafe partial class Gf
{
    private readonly record struct Pitch(string Flat, string Sharp, string MinorFlat, string MinorSharp,
        bool MajorUsesSharps, bool MinorUsesSharps)
    {
        public string MajorKey => MajorUsesSharps ? Sharp : Flat;
        public string MinorKey => MinorUsesSharps ? MinorSharp : MinorFlat;
    }

    // The chord insertion menus start on G. Key preferences also choose the spelling of chords in that key.
    private static readonly Pitch[] ChromaticScale =
    [
        new("G",  "G",  "Gm",  "Gm",  true,  false),
        new("Ab", "G#", "Abm", "G#m", false, true),
        new("A",  "A",  "Am",  "Am",  true,  true),
        new("Bb", "A#", "Bbm", "A#m", false, false),
        new("B",  "B",  "Bm",  "Bm",  true,  true),
        new("C",  "C",  "Cm",  "Cm",  true,  false),
        new("Db", "C#", "Dbm", "C#m", false, true),
        new("D",  "D",  "Dm",  "Dm",  true,  false),
        new("Eb", "D#", "Ebm", "D#m", false, true),
        new("E",  "E",  "Em",  "Em",  true,  true),
        new("F",  "F",  "Fm",  "Fm",  false, false),
        new("Gb", "F#", "Gbm", "F#m", true,  true)
    ];

    private static int ShiftPitch(int index, int semitones)
    {
        int pitch = (int)(((long)index + semitones) % ChromaticScale.Length);
        return pitch < 0 ? pitch + ChromaticScale.Length : pitch;
    }

    public static int IncrementChord(ref int CurPos, int TransposeTo) => ShiftPitch(CurPos, TransposeTo);

    private static int ReadRoot(ReadOnlySpan<char> chord, out int length)
    {
        length = 0;
        if (chord.IsEmpty)
            return -1;
        int natural = chord[0] switch { 'G' => 0, 'A' => 2, 'B' => 4, 'C' => 5, 'D' => 7, 'E' => 9, 'F' => 10, _ => -1 };
        if (natural < 0)
            return -1;
        length = 1;
        if (chord.Length >= 2 && chord[1] is '#' or 'b')
        {
            int adjacent = ShiftPitch(natural, chord[1] == '#' ? 1 : -1);
            // An unsupported spelling such as E# keeps the # as part of the suffix.
            if (ChromaticScale[adjacent].Flat != ChromaticScale[adjacent].Sharp)
            {
                length = 2;
                return adjacent;
            }
        }
        return natural;
    }

    public static int TransposeKey(ref string InKey, int TransposeStep)
    {
        if (InKey == "")
            return TransposeStep > 0 ? 1 : 0;
        int index = ReadRoot(InKey, out _);
        if (index < 0)
            return -1;
        bool minor = InKey.EndsWith('m');
        var source = ChromaticScale[index];
        if (InKey != (minor ? source.MinorKey : source.MajorKey))
            return -1;
        var target = ChromaticScale[ShiftPitch(index, TransposeStep)];
        InKey = minor ? target.MinorKey : target.MajorKey;
        return (minor ? target.MinorUsesSharps : target.MajorUsesSharps) ? 1 : 0;
    }

    internal static int TransposeEditorKeyAndCapo(ComboBox keySelector, ComboBox capoSelector, int steps)
    {
        string key = keySelector.Text;
        int spelling = TransposeKey(ref key, steps);
        keySelector.Text = key;
        // Index 0 is unspecified. Indices 1 through 12 represent capo frets 0 through 11.
        if (capoSelector.SelectedIndex > 0)
            capoSelector.SelectedIndex = ShiftPitch(capoSelector.SelectedIndex - 1, -steps) + 1;
        return spelling;
    }

    public static string TransposeChord(string InNotation, int TransposeStep, int FlatSharpKey)
    {
        if (string.IsNullOrEmpty(InNotation) || TransposeStep == 0)
            return InNotation;
        return TransposeChord(InNotation.AsSpan(), TransposeStep, FlatSharpKey);
    }

    private static string TransposeChord(ReadOnlySpan<char> chord, int steps, int spelling)
    {
        bool useSharps = spelling switch { 0 => false, 1 => true, _ => steps > 0 };
        int slash = ChordNotation.FindBassSeparator(chord);
        if (slash < 0)
            return TransposePart(chord, steps, useSharps);
        var bass = chord[(slash + 1)..];
        int nextSlash = ChordNotation.FindBassSeparator(bass);
        if (nextSlash >= 0)
            bass = bass[..nextSlash];
        return string.Concat(TransposePart(chord[..slash], steps, useSharps), "/",
            TransposePart(bass, steps, useSharps));
    }

    private static string TransposePart(ReadOnlySpan<char> chord, int steps, bool useSharps)
    {
        int prefix = !chord.IsEmpty && chord[0] is '[' or '{' ? 1 : 0;
        int index = ReadRoot(chord[prefix..], out int length);
        if (index < 0)
            return chord.ToString();
        var target = ChromaticScale[ShiftPitch(index, steps)];
        string root = useSharps ? target.Sharp : target.Flat;
        if (prefix == 0 && length == chord.Length)
            return root;
        return string.Concat(chord[..prefix], root, chord[(prefix + length)..]);
    }

    internal static void AddChordMenuItems(ContextMenuStrip menu, int region, EventHandler clicked)
    {
        menu.Items.Add(new ToolStripSeparator());
        var minor = new ToolStripMenuItem(UiText.Get("gfMusic", "Minor"));
        var minorSeventh = new ToolStripMenuItem(UiText.Get("gfMusic", "MinorSeventh"));
        var majorSeventh = new ToolStripMenuItem(UiText.Get("gfMusic", "MajorSeventh"));
        var bass = new ToolStripMenuItem(UiText.Get("gfMusic", "Bass"));
        menu.Items.AddRange([minor, minorSeventh, majorSeventh, bass]);
        string regionTag = region.ToString();
        foreach (var pitch in ChromaticScale)
        {
            Add(minor.DropDownItems, pitch.MinorFlat);
            Add(minorSeventh.DropDownItems, pitch.MinorFlat + "7");
            Add(majorSeventh.DropDownItems, pitch.Sharp + "7");
            Add(bass.DropDownItems, "/" + pitch.Sharp);
            Add(menu.Items, pitch.Sharp);
            if (pitch.Flat != pitch.Sharp)
            {
                Add(minor.DropDownItems, pitch.MinorSharp);
                Add(minorSeventh.DropDownItems, pitch.MinorSharp + "7");
                Add(majorSeventh.DropDownItems, pitch.Flat + "7");
                Add(menu.Items, pitch.Flat);
            }
        }

        void Add(ToolStripItemCollection items, string chord)
            => items.Add(new ToolStripMenuItem(chord, null, clicked) { Tag = regionTag });
    }
}
