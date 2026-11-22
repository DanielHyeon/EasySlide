using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Axicultus.Wpf.Data;
using Axicultus.Wpf.Library;
using Axicultus.Wpf.Settings;
using Xunit;

namespace Axicultus.Wpf.Tests.Library;

// Adversarial tests of the song editor: fuzzing against the WinForms methods it mirrors, and the cases a review
// found. The tests that read real songs need AXICULTUS_REAL_SONG_DB (a song database, opened read-only) and are
// skipped without it; with it, the tests also write reports to <repo>/_review.
public class SongEditorAdversarialTests
{
    internal static string RealDb => Environment.GetEnvironmentVariable("AXICULTUS_REAL_SONG_DB") ?? "";

    private static string Root => Axicultus.Wpf.Tests.Accessibility.XamlAccessibilityScanner.FindRepoRoot();

    /// <summary>Runs when AXICULTUS_REAL_SONG_DB names a song database and the WinForms app is built; skipped otherwise.</summary>
    internal sealed class RealSongsFactAttribute : FactAttribute
    {
        public RealSongsFactAttribute()
        {
            if (!File.Exists(RealDb))
            {
                Skip = "Set AXICULTUS_REAL_SONG_DB to a song database (it is opened read-only).";
            }
            else if (!File.Exists(WinFormsAssembly))
            {
                Skip = $"Build the WinForms app first: {WinFormsAssembly}";
            }
        }
    }

    /// <summary>Runs when the WinForms app is built beside the tests; skipped otherwise.</summary>
    internal sealed class WinFormsBuiltFactAttribute : FactAttribute
    {
        public WinFormsBuiltFactAttribute()
        {
            if (!File.Exists(WinFormsAssembly))
            {
                Skip = $"Build the WinForms app first: {WinFormsAssembly}";
            }
        }
    }

    // A report for whoever runs the tests on real songs. Without the variable nothing is written.
    internal static void WriteReport(string name, string text, bool append = false)
    {
        if (!File.Exists(RealDb))
        {
            return;
        }

        string path = Path.Combine(Directory.CreateDirectory(Path.Combine(Root, "_review")).FullName, name);
        if (append)
        {
            File.AppendAllText(path, text);
        }
        else
        {
            File.WriteAllText(path, text);
        }
    }

    private static string WinFormsAssembly
    {
        get
        {
            var output = new DirectoryInfo(AppContext.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar));
            return Path.Combine(Root, "Axicultuss", "bin", output.Parent!.Name, output.Name, "Axicultuss.dll");
        }
    }

    // ── WinForms oracle: the app's own ExtractLyrics and Merge_Songs ─────────────────────────────

    private sealed class WinForms
    {
        private readonly MethodInfo _extract, _merge;
        private readonly Type _itemType;
        private readonly object _first, _second;

        public WinForms()
        {
            const BindingFlags statics = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
            var gf = Assembly.LoadFrom(WinFormsAssembly).GetType("Axicultus.Gf", throwOnError: true)!;
            var byRef = typeof(string).MakeByRefType();
            _extract = gf.GetMethod("ExtractLyrics", statics, [typeof(string), typeof(string), byRef, byRef, byRef, byRef])!;
            _merge = gf.GetMethod("Merge_Songs", statics)!;
            _itemType = _merge.GetParameters()[0].ParameterType;
            _first = Activator.CreateInstance(_itemType)!;
            _second = Activator.CreateInstance(_itemType)!;
            gf.GetMethod("InitializeVerseHeadings", statics)!.Invoke(null, null);
        }

        public (string T1, string N1, string T2, string N2) Extract(string lyrics, string notations)
        {
            object[] panes = [lyrics, notations, "", "", "", ""];
            (_extract.Invoke(null, panes) as IDisposable)?.Dispose();
            return ((string)panes[2], (string)panes[3], (string)panes[4], (string)panes[5]);
        }

        public (string Lyrics, string Notations) Merge(string t1, string n1, string t2, string n2)
        {
            Fill(_first, t1, n1);
            Fill(_second, t2, n2);
            object[] stored = [_first, _second, "", ""];
            _merge.Invoke(null, stored);
            return ((string)stored[2], (string)stored[3]);
        }

        private void Fill(object item, string lyrics, string notations)
        {
            _itemType.GetField("CompleteLyrics")!.SetValue(item, lyrics);
            _itemType.GetField("Notations")!.SetValue(item, notations);
            _itemType.GetField("OriginalNotations")!.SetValue(item, notations);
        }
    }

    // ── FrmEditItem itself: the form of the WinForms app and its own ValidateContents ────────────

    private sealed class FrmEditItemForm : IDisposable
    {
        private const BindingFlags Members = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
        private readonly Type _type;
        private readonly object _form;
        private readonly IDisposable _font;

        public FrmEditItemForm()
        {
            var assembly = Assembly.LoadFrom(WinFormsAssembly);
            var gf = assembly.GetType("Axicultus.Gf", throwOnError: true)!;
            gf.GetMethod("InitializeVerseHeadings", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)!.Invoke(null, null);
            _type = assembly.GetType("Axicultus.FrmEditItem", throwOnError: true)!;
            _form = Activator.CreateInstance(_type)!;
            _font = (IDisposable)Activator.CreateInstance(Field("MainFont").FieldType, "Arial", 12f)!;
            Field("MainFont").SetValue(_form, _font);
            Field("NotationFont").SetValue(_form, _font);
            Field("sArray").SetValue(_form, ((string)gf.GetField("SymbolsString")!.GetValue(null)!).Split(','));
            SetText("SongTitle", "Title");
            Field("InitLoad").SetValue(_form, false);
        }

        private FieldInfo Field(string name) => _type.GetField(name, Members)!;

        private string Text(string control)
        {
            object box = Field(control).GetValue(_form)!;
            return (string)box.GetType().GetProperty("Text")!.GetValue(box)!;
        }

        private void SetText(string control, string text)
        {
            object box = Field(control).GetValue(_form)!;
            box.GetType().GetProperty("Text")!.SetValue(box, text);
        }

        /// <summary>
        /// What FrmEditItem says to the two panes of a song: "" when ValidateContents accepts them and the
        /// sequence names only verses the form lists, else the message. A heading that shares its line is
        /// rewritten by the form through the clipboard, as in the app.
        /// </summary>
        public string Refusal(string pane1, string pane2, string sequence)
        {
            Field("IgnoreChange").SetValue(_form, true);
            SetText("tbLyrics1", pane1);
            SetText("tbLyrics2", pane2);
            Field("IgnoreChange").SetValue(_form, false);
            _type.GetMethod("UpdateVersesList", Members)!.Invoke(_form, null);
            if (!(bool)_type.GetMethod("ValidateContents", Members, Type.EmptyTypes)!.Invoke(_form, null)!)
            {
                // A label without a message shows its caption.
                string first = Text("LabeltbLyrics"), second = Text("LabeltbLyrics2");
                return (first.Contains("Region 1", StringComparison.Ordinal) ? "" : first) + (second.StartsWith("Additional", StringComparison.Ordinal) ? "" : second);
            }

            var present = (bool[])Field("VersePresent").GetValue(_form)!;
            return sequence.Any(code => code < 160 && !present[code]) ? "Sequence contains a verse which is not in the lyrics!" : "";
        }

        public void Dispose()
        {
            ((IDisposable)_form).Dispose();
            _font.Dispose();
        }
    }

    // ── Pane model: lyric lines with the chord fields of each line ───────────────────────────────

    internal static List<(string Line, string Fields)> Rows(string text, string notations)
    {
        var by = new Dictionary<int, string>();
        string rest = notations;
        while (SongEditorChords.TryReadRecord(ref rest, out int line, out string fields) && line >= 0)
        {
            by.TryAdd(line, fields);
        }

        return [.. text.Split('\n').Select((line, index) => (line, by.GetValueOrDefault(index, "")))];
    }

    internal static string TextOf(List<(string Line, string Fields)> rows) => string.Join('\n', rows.Select(row => row.Line));

    internal static string NotesOf(List<(string Line, string Fields)> rows)
    {
        var notes = new StringBuilder();
        for (int index = 0; index < rows.Count; index++)
        {
            if (rows[index].Fields.Length > 0)
            {
                notes.Append('(').Append(index).Append(';').Append(rows[index].Fields).Append(')');
            }
        }

        return notes.ToString();
    }

    // Display line of each model row; null when the pane does not show the model as chord line + lyric line.
    internal static int[]? Map(List<(string Line, string Fields)> rows, List<string> display)
    {
        var map = new int[rows.Count];
        int d = 0;
        for (int i = 0; i < rows.Count; i++)
        {
            if (rows[i].Fields.Length > 0 && d + 1 < display.Count && display[d].EndsWith(" »", StringComparison.Ordinal) && display[d + 1] == rows[i].Line)
            {
                d++;
            }

            if (d >= display.Count || display[d] != rows[i].Line)
            {
                return null;
            }

            map[i] = d++;
        }

        return d == display.Count ? map : null;
    }

    internal static bool IsHeading(string line) => line.Length >= 3 && line[0] == '[' && line[^1] == ']';

    private static string NamesOnly(string notations)
        => string.Join("|", SongEditorChords.NamesByLine(notations).OrderBy(pair => pair.Key).Select(pair => $"{pair.Key}:{pair.Value}"));

    internal sealed record Opened(SongEditorViewModel Vm, Repo Repo);

    internal static Opened Open(ISettingsService settings, string database, SongDetail song, IEnumerable<SongFolderSummary> folders)
    {
        var repo = new Repo();
        repo.Folders.AddRange(folders);
        repo.Details[song.SongId] = song;
        var vm = new SongEditorViewModel(settings, repo, repo, null, null);
        vm.LoadAsync(database, new SongFolderSummary(song.FolderNo, $"Folder {song.FolderNo}", true, 0),
            new SongSummary(song.SongId, song.Title, song.AlternateTitle, song.FolderNo, song.SongNumber, song.Category, song.Key, song.Lyrics)).GetAwaiter().GetResult();
        return new Opened(vm, repo);
    }

    internal static List<SongDetail> ReadRealSongs()
    {
        var songs = new List<SongDetail>();
        using var connection = new System.Data.SQLite.SQLiteConnection($"Data Source={RealDb};Read Only=True;Pooling=False");
        connection.Open();
        using var command = new System.Data.SQLite.SQLiteCommand("SELECT * FROM SONG ORDER BY SONGID", connection);
        using var reader = command.ExecuteReader();
        var ordinals = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        for (int i = 0; i < reader.FieldCount; i++)
        {
            ordinals[reader.GetName(i)] = i;
        }

        while (reader.Read())
        {
            string Text(string name) => ordinals.TryGetValue(name, out int column) && !reader.IsDBNull(column) ? Convert.ToString(reader.GetValue(column)) ?? "" : "";
            int Number(string name) => int.TryParse(Text(name), out int value) ? value : 0;
            songs.Add(new SongDetail(Number("SONGID"), Text("TITLE_1"), Text("TITLE_2"), Number("FOLDERNO"), Number("SONG_NUMBER"),
                Text("LYRICS"), Text("SEQUENCE"), Text("WRITER"), Text("COPYRIGHT"), Number("CAPO"), Text("TIMING"), Text("KEY"), Text("MSC"),
                Text("CATEGORY"), Text("LICENCE_ADMIN1"), Text("LICENCE_ADMIN2"), Text("BOOK_REFERENCE"), Text("USER_REFERENCE"),
                Text("SETTINGS"), Text("FORMATDATA")));
        }

        return songs;
    }

    private sealed class Tally
    {
        private readonly SortedDictionary<string, List<int>> _groups = new(StringComparer.Ordinal);

        public void Add(string key, int id)
        {
            if (!_groups.TryGetValue(key, out var ids))
            {
                _groups[key] = ids = [];
            }

            ids.Add(id);
        }

        public IEnumerable<string> Keys => _groups.Keys;

        public void Write(StringBuilder report, string title, int show = 40)
        {
            report.AppendLine(title);
            foreach (var (key, ids) in _groups)
            {
                report.AppendLine($"  {ids.Count,5}  {key}: {string.Join(' ', ids.Take(show))}{(ids.Count > show ? " ..." : "")}");
            }
        }
    }

    private static string Mask(string message) => System.Text.RegularExpressions.Regex.Replace(message, @"\[[^\]]+\]|\d+절|'[^']*'|\(\d+자\)", "…");

    // ── Bulk: every real song, ordinary edits, against the WinForms merge of the same edit ───────────

    [RealSongsFact]
    public void EveryRealSong_AfterAnOrdinaryEdit_IsStoredAsTheWinFormsMergeStoresIt()
    {
        StaHelper.RunOnSta(() =>
        {
            string root = Path.Combine(Path.GetTempPath(), $"Axicultus_Adversarial_{Guid.NewGuid():N}");
            Directory.CreateDirectory(root);
            string database = Path.Combine(root, "songs.db");
            File.WriteAllText(database, "");
            var settings = new SettingsService(new SettingsServiceOptions(Path.Combine(root, "settings.json"), Path.Combine(root, "SettingsBackups")));
            var wf = new WinForms();
            var songs = ReadRealSongs();
            var folders = songs.Select(song => Math.Max(song.FolderNo, 1)).Distinct().Order()
                .Select(folderNo => new SongFolderSummary(folderNo, $"Folder {folderNo}", true, 0)).ToList();

            var mismatch = new Tally();      // edit kind -> songs whose Lyrics/MSC differ from the WinForms merge of the same edit
            var refusedBy = new Tally();     // edit kind + message -> songs
            var newlyRefused = new Tally();  // refused after a harmless pane-1 edit although the untouched song saves
            var column = new Tally();        // column drift on a save that did not touch that column
            var region2 = new Tally();       // pane-1 edit that changed what region 2 reads back as
            var unmappable = new Tally();
            var details = new StringBuilder();
            int compared = 0;

            foreach (var stored in songs)
            {
                var song = stored with { FolderNo = Math.Max(stored.FolderNo, 1) };
                var panes = wf.Extract(song.Lyrics, song.Notations);
                var baseline = Open(settings, database, song, folders);
                string untouchedLyrics = baseline.Vm.Lyrics, untouchedNotes = baseline.Vm.Notations;
                baseline.Vm.SaveAsync().GetAwaiter().GetResult();
                bool untouchedRefused = baseline.Vm.ValidationMessage.Length > 0;
                if (untouchedRefused)
                {
                    refusedBy.Add("untouched | " + Mask(baseline.Vm.ValidationMessage), song.SongId);
                }
                else
                {
                    CheckColumns("untouched", baseline, song, lyricsTouched: false);
                }

                var untouchedSplit = SongEditorLyrics.Split(untouchedLyrics, untouchedNotes);

                void CheckColumns(string kind, Opened opened, SongDetail expected, bool lyricsTouched, bool titleTouched = false, bool capoTouched = false, bool folderTouched = false)
                {
                    var saved = opened.Repo.LastSong;
                    if (saved is null)
                    {
                        column.Add($"{kind} | nothing written", expected.SongId);
                        return;
                    }

                    void Same(string name, object? was, object? now)
                    {
                        if (!Equals(was, now))
                        {
                            column.Add($"{kind} | {name}", expected.SongId);
                            if (details.Length < 400_000)
                            {
                                details.AppendLine($"[{kind}] song {expected.SongId} {name}: stored <{Show(was)}> saved <{Show(now)}>");
                            }
                        }
                    }

                    if (!lyricsTouched)
                    {
                        Same("LYRICS vs untouched merge", untouchedLyrics, saved.Lyrics);
                        Same("MSC vs untouched merge", untouchedNotes, saved.Notations);
                    }

                    if (!titleTouched)
                    {
                        Same("TITLE_1", expected.Title, saved.Title);
                    }

                    Same("TITLE_2", expected.AlternateTitle, saved.AlternateTitle);
                    Same("SONG_NUMBER", expected.SongNumber, saved.SongNumber);
                    if (!folderTouched)
                    {
                        Same("FOLDERNO", expected.FolderNo, saved.FolderNo);
                    }

                    string savedSequence = Axicultus.Module.SongExchangeSequence.ToDatabase(saved.Sequence);
                    Same("SEQUENCE", expected.Sequence, savedSequence.Length > 100 ? savedSequence[..100] : savedSequence);
                    Same("WRITER", expected.Writer, saved.Writer);
                    Same("COPYRIGHT", expected.Copyright, saved.Copyright);
                    if (!capoTouched)
                    {
                        Same("CAPO (negative is -1)", Math.Max(expected.Capo, -1), saved.Capo);
                    }

                    Same("TIMING", expected.Timing, Cut(saved.Timing, 50));
                    Same("KEY", expected.Key, Cut(saved.Key, 20));
                    Same("CATEGORY", expected.Category, saved.Category);
                    Same("LICENCE_ADMIN1", expected.LicenceAdmin1, Cut(saved.LicenceAdmin1, 50));
                    Same("LICENCE_ADMIN2", expected.LicenceAdmin2, Cut(saved.LicenceAdmin2, 50));
                    Same("BOOK_REFERENCE", expected.BookReference, Cut(saved.BookReference, 50));
                    Same("USER_REFERENCE", expected.UserReference, saved.UserReference);
                    Same("SETTINGS", expected.Settings, saved.Settings);
                    Same("FORMATDATA", expected.FormatData, saved.FormatData);
                }

                // Field edits that leave the panes alone.
                foreach (string kind in (string[])["title", "capo", "folder"])
                {
                    var opened = Open(settings, database, song, folders);
                    switch (kind)
                    {
                        case "title": opened.Vm.Title = song.Title + "2"; break;
                        case "capo": opened.Vm.Capo = opened.Vm.Capo == 3 ? 4 : 3; break;
                        default:
                            var other = opened.Vm.Folders.FirstOrDefault(folder => folder.FolderNo != song.FolderNo);
                            if (other is null)
                            {
                                continue;
                            }

                            opened.Vm.SelectedFolder = other;
                            break;
                    }

                    if ((opened.Vm.Lyrics, opened.Vm.Notations) != (untouchedLyrics, untouchedNotes))
                    {
                        mismatch.Add($"{kind}: Lyrics/MSC moved by a field edit", song.SongId);
                    }

                    opened.Vm.SaveAsync().GetAwaiter().GetResult();
                    if (opened.Vm.ValidationMessage.Length > 0)
                    {
                        refusedBy.Add($"{kind} | {Mask(opened.Vm.ValidationMessage)}", song.SongId);
                        if (!untouchedRefused)
                        {
                            newlyRefused.Add($"{kind} | {Mask(opened.Vm.ValidationMessage)}", song.SongId);
                        }
                    }
                    else
                    {
                        CheckColumns(kind, opened, song, lyricsTouched: false, titleTouched: kind == "title", capoTouched: kind == "capo", folderTouched: kind == "folder");
                        if (kind == "folder" && opened.Repo.LastSong!.FolderNo == song.FolderNo)
                        {
                            column.Add("folder | FOLDERNO not moved", song.SongId);
                        }
                    }
                }

                // Text edits, pane 1 then pane 2.
                foreach (int pane in (int[])[1, 2])
                {
                    var rows = pane == 1 ? Rows(panes.T1, panes.N1) : Rows(panes.T2, panes.N2);
                    if (rows.Count == 1 && rows[0].Line.Length == 0)
                    {
                        continue;
                    }

                    foreach (string kind in (string[])["typo", "typo on a chord line", "insert in verse 2", "delete verse 2", "add verse at end", "add chord", "blank line at end"])
                    {
                        var opened = Open(settings, database, song, folders);
                        var vm = opened.Vm;
                        var display = (pane == 1 ? vm.LyricsRegion1 : vm.LyricsRegion2).Split('\n').ToList();
                        var model = rows.ToList();
                        var map = Map(model, display);
                        if (map is null)
                        {
                            unmappable.Add($"pane {pane}", song.SongId);
                            break;
                        }

                        bool HasChordLine(int row) => map[row] > 0 && (row == 0 ? map[row] == 1 : map[row] - map[row - 1] == 2);
                        int Plain(Func<int, bool> also) => Enumerable.Range(0, model.Count).FirstOrDefault(
                            row => model[row].Line.Trim().Length > 0 && !IsHeading(model[row].Line.Trim()) && !model[row].Line.Contains('»') && also(row), -1);
                        int secondVerse = model.FindIndex(row => row.Line == "[2]");
                        bool structural = false, positionsMayMove = false;
                        int? chordCaret = null;
                        int target;
                        switch (kind)
                        {
                            case "typo":
                                target = Plain(row => model[row].Fields.Length == 0);
                                if (target < 0) continue;
                                model[target] = (model[target].Line + "x", model[target].Fields);
                                display[map[target]] += "x";
                                break;
                            case "typo on a chord line":
                                target = Plain(row => model[row].Fields.Length > 0 && HasChordLine(row));
                                if (target < 0) continue;
                                model[target] = (model[target].Line + "x", model[target].Fields);
                                display[map[target]] += "x";
                                positionsMayMove = true;
                                break;
                            case "insert in verse 2":
                                target = secondVerse >= 0 && secondVerse + 1 < model.Count && !IsHeading(model[secondVerse + 1].Line) ? secondVerse + 1 : model.Count / 2;
                                if (IsHeading(model[target].Line) && target == model.Count - 1) continue;
                                model.Insert(target + 1, ("inserted line", ""));
                                display.Insert(map[target] + 1, "inserted line");
                                display[map[target]] += "\r"; // typed with Enter: the editor inserts "\r\n"
                                break;
                            case "delete verse 2":
                                if (secondVerse < 0) continue;
                                int end = model.FindIndex(secondVerse + 1, row => IsHeading(row.Line));
                                end = end < 0 ? model.Count : end;
                                int from = map[secondVerse] - (HasChordLine(secondVerse) ? 1 : 0);
                                int to = end == model.Count ? display.Count : map[end] - (HasChordLine(end) ? 1 : 0);
                                display.RemoveRange(from, to - from);
                                model.RemoveRange(secondVerse, end - secondVerse);
                                if (model.Count == 0) continue;
                                structural = true;
                                break;
                            case "add verse at end":
                                int next = Enumerable.Range(2, 97).First(number => !panes.T1.Contains($"[{number}]") && !panes.T2.Contains($"[{number}]"));
                                model.Add(($"[{next}]", ""));
                                model.Add(("added verse line", ""));
                                display[^1] += "\r";
                                display.Add($"[{next}]\r");
                                display.Add("added verse line");
                                structural = true;
                                break;
                            case "add chord":
                                target = Plain(row => model[row].Fields.Length == 0 && (row == 0 || !display[map[row] - 1].Contains('»')));
                                if (target < 0) continue;
                                model[target] = (model[target].Line, "G;0;");
                                chordCaret = display.Take(map[target]).Sum(line => line.Length + 1);
                                break;
                            default:
                                display.Add("");
                                model.Add(("", "")); // FrmEditItem reads the same trailing empty line from its text box
                                break;
                        }

                        if (chordCaret is int caret)
                        {
                            vm.InsertChord(pane, caret, "G");
                        }
                        else if (pane == 1)
                        {
                            vm.LyricsRegion1 = string.Join('\n', display);
                        }
                        else
                        {
                            vm.LyricsRegion2 = string.Join('\n', display);
                        }

                        var first = pane == 1 ? model : Rows(panes.T1, panes.N1);
                        var second = pane == 2 ? model : Rows(panes.T2, panes.N2);
                        var expected = wf.Merge(TextOf(first), NotesOf(first), TextOf(second), NotesOf(second));
                        compared++;
                        bool lyricsEqual = vm.Lyrics == expected.Lyrics;
                        bool notesEqual = positionsMayMove ? NamesOnly(vm.Notations) == NamesOnly(expected.Notations) : vm.Notations == expected.Notations;
                        if (!lyricsEqual || !notesEqual)
                        {
                            bool positionsOnly = lyricsEqual && NamesOnly(vm.Notations) == NamesOnly(expected.Notations);
                            mismatch.Add($"{(positionsOnly ? "chord positions only, " : "")}pane {pane} {kind}: {(lyricsEqual ? "" : "LYRICS ")}{(notesEqual ? "" : "MSC")}", song.SongId);
                            if (details.Length < 400_000)
                            {
                                details.AppendLine($"[pane {pane} {kind}] song {song.SongId}\n  WinForms MSC {expected.Notations}\n  editor   MSC {vm.Notations}\n  first lyric difference: {FirstDifference(expected.Lyrics, vm.Lyrics)}");
                            }
                        }

                        if (kind == "blank line at end" && vm.Lyrics != untouchedLyrics)
                        {
                            column.Add($"pane {pane} {kind} | LYRICS changed by a trailing Enter (same in WinForms: {vm.Lyrics == expected.Lyrics})", song.SongId);
                            if (details.Length < 400_000)
                            {
                                details.AppendLine($"[pane {pane} trailing Enter] song {song.SongId}: {FirstDifference(untouchedLyrics, vm.Lyrics)}");
                            }
                        }

                        if (positionsMayMove && vm.Notations != expected.Notations)
                        {
                            column.Add($"pane {pane} {kind} | chord positions of the edited line re-measured (character measure)", song.SongId);
                        }

                        // A pane-1 edit that adds or removes no heading must leave region 2 as it read before.
                        if (pane == 1 && !structural)
                        {
                            var after = SongEditorLyrics.Split(vm.Lyrics, vm.Notations);
                            if ((after.Text2, after.Notations2) != (untouchedSplit.Text2, untouchedSplit.Notations2))
                            {
                                region2.Add(kind, song.SongId);
                            }
                        }

                        vm.SaveAsync().GetAwaiter().GetResult();
                        if (vm.ValidationMessage.Length > 0)
                        {
                            refusedBy.Add($"pane {pane} {kind} | {Mask(vm.ValidationMessage)}", song.SongId);
                            if (!untouchedRefused)
                            {
                                newlyRefused.Add($"pane {pane} {kind} | {Mask(vm.ValidationMessage)}", song.SongId);
                            }
                        }
                        else
                        {
                            var saved = opened.Repo.LastSong!;
                            if ((saved.Lyrics, saved.Notations) != (vm.Lyrics, vm.Notations))
                            {
                                column.Add($"pane {pane} {kind} | saved text is not the derived text", song.SongId);
                            }

                            string savedSequence = Axicultus.Module.SongExchangeSequence.ToDatabase(saved.Sequence);
                            if (savedSequence != song.Sequence)
                            {
                                column.Add($"pane {pane} {kind} | SEQUENCE changed", song.SongId);
                            }

                            // Saved, opened again, saved untouched: the same text.
                            var again = SongEditorLyrics.Split(saved.Lyrics, saved.Notations);
                            if (SongEditorLyrics.Merge(again.Text1, again.Notations1, again.Text2, again.Notations2) != (saved.Lyrics, saved.Notations))
                            {
                                column.Add($"pane {pane} {kind} | not stable on a second save", song.SongId);
                            }
                        }
                    }
                }
            }

            var report = new StringBuilder().AppendLine($"songs: {songs.Count}; text edits compared with the WinForms merge: {compared}");
            mismatch.Write(report, "DIFFERENT FROM THE WINFORMS MERGE OF THE SAME EDIT:");
            region2.Write(report, "PANE-1 EDIT CHANGED REGION 2:");
            column.Write(report, "COLUMN DRIFT:");
            newlyRefused.Write(report, "REFUSED AFTER THE EDIT ALTHOUGH THE UNTOUCHED SONG SAVES:");
            refusedBy.Write(report, "ALL REFUSALS:", 12);
            unmappable.Write(report, "PANE NOT SHOWN AS CHORD LINE + LYRIC LINE (skipped):");
            WriteReport("edits-report.txt", report.ToString());
            WriteReport("edits-details.txt", details.ToString());
            try { Directory.Delete(root, recursive: true); } catch (IOException) { }

            // Two chord lines that look the same can hold different stored positions; once one of them is removed
            // the other is read as it is shown (SongEditorChords.Read), here with the character measure.
            Assert.DoesNotContain(mismatch.Keys, key => !(key.StartsWith("chord positions only, ", StringComparison.Ordinal) && key.Contains("delete verse 2", StringComparison.Ordinal)));
            Assert.Empty(region2.Keys);
            // An Enter after the last line is stored and dropped by the next save, in WinForms too; the positions of
            // a chord line the edit touched are measured again.
            Assert.DoesNotContain(column.Keys, key => !key.Contains(" blank line at end | ", StringComparison.Ordinal) && !key.Contains("re-measured", StringComparison.Ordinal));
            // Only an edit that adds or removes a heading can make a song break a rule it kept before.
            Assert.DoesNotContain(newlyRefused.Keys, key => !key.Contains(" add verse at end | ", StringComparison.Ordinal) && !key.Contains(" delete verse 2 | ", StringComparison.Ordinal));
        });

        static string Show(object? value) => (value?.ToString() ?? "").Replace("\n", "\\n").Replace("\r", "\\r") is { Length: > 160 } text ? text[..160] + "…" : (value?.ToString() ?? "").Replace("\n", "\\n").Replace("\r", "\\r");
        static string Cut(string value, int length) => value.Length <= length ? value : value[..length];
    }

    // Every real song, opened and saved untouched: what the editor refuses, FrmEditItem refuses too, asked of
    // the form itself. FrmEditItem refuses more: lyrics above the first heading, which the editor saves for a
    // song that is stored like that (SongEditorLyrics.ContentProblem). The form uses the clipboard for a heading
    // that shares its line; the text that was on the clipboard is put back.
    [RealSongsFact]
    public void EveryRealSong_TheEditorRefuses_IsRefusedByFrmEditItemItself()
    {
        StaHelper.RunOnSta(() =>
        {
            using var fixture = new Fixture();
            var wf = new WinForms();
            using var form = new FrmEditItemForm();
            var songs = ReadRealSongs();
            var folders = songs.Select(song => Math.Max(song.FolderNo, 1)).Distinct().Order()
                .Select(folderNo => new SongFolderSummary(folderNo, $"Folder {folderNo}", true, 0)).ToList();
            var both = new Tally();
            var onlyEditor = new Tally();
            var onlyForm = new Tally();
            string? clipboard = System.Windows.Clipboard.ContainsText() ? System.Windows.Clipboard.GetText() : null;
            try
            {
                foreach (var stored in songs)
                {
                    var song = stored with { FolderNo = Math.Max(stored.FolderNo, 1) };
                    var panes = wf.Extract(song.Lyrics, song.Notations);
                    string formSays = System.Text.RegularExpressions.Regex.Replace(form.Refusal(panes.T1, panes.T2, song.Sequence), @"\d+", "N").Replace("\n", "\\n");
                    var opened = Open(fixture.Settings, fixture.Database, song, folders);
                    opened.Vm.SaveAsync().GetAwaiter().GetResult();
                    string editorSays = Mask(opened.Vm.ValidationMessage);
                    if (editorSays.Length > 0 && formSays.Length > 0)
                    {
                        both.Add($"{editorSays} | {formSays}", song.SongId);
                    }
                    else if (editorSays.Length > 0)
                    {
                        onlyEditor.Add(editorSays, song.SongId);
                    }
                    else if (formSays.Length > 0)
                    {
                        onlyForm.Add(formSays, song.SongId);
                    }
                }
            }
            finally
            {
                if (clipboard is not null)
                {
                    System.Windows.Clipboard.SetText(clipboard);
                }
            }

            var report = new StringBuilder().AppendLine($"songs: {songs.Count}, each opened and saved untouched");
            both.Write(report, "REFUSED BY THE EDITOR AND BY FRMEDITITEM (editor | FrmEditItem):", 60);
            onlyEditor.Write(report, "REFUSED BY THE EDITOR ONLY:", 60);
            onlyForm.Write(report, "REFUSED BY FRMEDITITEM ONLY:", 60);
            WriteReport("untouched-editor-and-frmedititem.txt", report.ToString());
            Assert.Empty(onlyEditor.Keys);
            Assert.All(onlyForm.Keys, key => Assert.StartsWith("Indicator is required for the lyrics before", key));
        });
    }

    // ── Real data: what the stored songs hold that the editor treats specially ───────────────────

    [RealSongsFact]
    public void RealSongs_HoldNoChordRecordThePanesCannotShow()
    {
        var songs = ReadRealSongs();
        var tally = new Tally();
        var notes = new StringBuilder();
        foreach (var song in songs)
        {
            string lyrics = song.Lyrics.Replace("\r\n", "\n");
            string[] lines = lyrics.Split('\n');
            if (lyrics.Contains('»')) tally.Add("LYRICS holds the chord marker »", song.SongId);
            if (lyrics.Contains('\r')) tally.Add("LYRICS holds a lone CR", song.SongId);
            if (lyrics.Contains('\t')) tally.Add("LYRICS holds a tab", song.SongId);
            if (lines.Any(line => line != line.TrimEnd())) tally.Add("LYRICS has lines with trailing whitespace", song.SongId);
            if (song.Sequence.Length > 0) tally.Add("has a stored SEQUENCE", song.SongId);
            if (song.Sequence.Length > 100) tally.Add("SEQUENCE longer than 100", song.SongId);
            if (song.Sequence.Length > 0)
            {
                string shown;
                try { shown = Axicultus.Module.SongExchangeSequence.FromWpfDatabase(song.Sequence); }
                catch (InvalidDataException) { shown = song.Sequence; tally.Add("SEQUENCE has a code FromWpfDatabase cannot show", song.SongId); }
                if (Axicultus.Module.SongExchangeSequence.ToDatabase(shown) != song.Sequence) tally.Add("SEQUENCE does not round-trip through the sequence box", song.SongId);
            }

            if (song.Notations.Length == 0)
            {
                continue;
            }

            tally.Add("has MSC", song.SongId);
            string rest = song.Notations;
            int previous = -1;
            var seen = new HashSet<int>();
            while (SongEditorChords.TryReadRecord(ref rest, out int line, out string fields))
            {
                if (line < 0) { tally.Add("MSC: empty record", song.SongId); break; }
                if (line >= lines.Length) tally.Add("MSC: record beyond the last line", song.SongId);
                else if (lines[line].Length == 0) tally.Add("MSC: record on an empty line", song.SongId);
                else if (IsHeading(lines[line])) tally.Add("MSC: record on a heading line", song.SongId);
                else if (lines[line].Contains('»')) tally.Add("MSC: record on a line that holds »", song.SongId);
                if (line <= previous) tally.Add("MSC: records not ascending", song.SongId);
                if (!seen.Add(line)) tally.Add("MSC: two records for one line", song.SongId);
                previous = line;
                string[] parts = fields.Split(';');
                var positions = new List<int>();
                for (int i = 0; i + 1 < parts.Length; i += 2)
                {
                    if (parts[i].Length == 0) tally.Add("MSC: empty chord name", song.SongId);
                    if (parts[i].Contains(' ')) tally.Add("MSC: chord name with a space", song.SongId);
                    if (!int.TryParse(parts[i + 1], out int position)) tally.Add("MSC: position not a number", song.SongId);
                    else
                    {
                        if (line < lines.Length && position > lines[line].Length) tally.Add("MSC: chord beyond the end of its line", song.SongId);
                        if (positions.Contains(position)) tally.Add("MSC: two chords at one position", song.SongId);
                        if (positions.Count > 0 && position < positions[^1]) tally.Add("MSC: positions not ascending in a record", song.SongId);
                        positions.Add(position);
                    }
                }

                if (!fields.EndsWith(';')) tally.Add("MSC: record does not end with ;", song.SongId);
            }

            if (rest.Trim().Length > 0) tally.Add("MSC: text left after the last readable record", song.SongId);

            // Lines that look the same in a pane (chord line + lyric line) but hold different records.
            var split = SongEditorLyrics.Split(song.Lyrics, song.Notations);
            foreach (var (text, records, pane) in new[] { (split.Text1, split.Notations1, 1), (split.Text2, split.Notations2, 2) })
            {
                var chords = new SongEditorChords();
                string[] display = chords.Combine(text, records).Split('\n');
                var rows = Rows(text, records);
                var pairs = new Dictionary<(string, string), HashSet<string>>();
                int d = 0;
                foreach (var (line, fields) in rows)
                {
                    if (fields.Length > 0 && d + 1 < display.Length && display[d].EndsWith(" »", StringComparison.Ordinal) && display[d + 1] == line)
                    {
                        if (!pairs.TryGetValue((display[d], line), out var set)) pairs[(display[d], line)] = set = [];
                        set.Add(fields);
                        d++;
                    }

                    d++;
                }

                foreach (var pair in pairs.Where(pair => pair.Value.Count > 1))
                {
                    tally.Add($"pane {pane}: lines that look the same with different stored positions (character measure)", song.SongId);
                    notes.AppendLine($"song {song.SongId} pane {pane}: <{pair.Key.Item1}> over <{pair.Key.Item2}> = {string.Join(" | ", pair.Value)}");
                }

                // What a chord line shows against what is stored, with the character measure: lines where the
                // displayed chord line, read as typed, would give other positions than the stored ones.
                var fresh = new SongEditorChords();
                var reread = fresh.Read(string.Join('\n', display));
                if (reread.Notations != records) tally.Add($"pane {pane}: stored positions differ from what the chord line shows (character measure)", song.SongId);
            }
        }

        var report = new StringBuilder().AppendLine($"songs: {songs.Count}");
        tally.Write(report, "REAL DATA:", 60);
        report.Append(notes);
        WriteReport("real-stats.txt", report.ToString());
        // A record on an empty line, on a heading or on a line that holds the marker, or records out of order:
        // the panes show such a song only in part (see the facts below).
        Assert.DoesNotContain(tally.Keys, key => key.StartsWith("MSC: record", StringComparison.Ordinal)
            || key.StartsWith("MSC: two records", StringComparison.Ordinal) || key.StartsWith("MSC: empty record", StringComparison.Ordinal)
            || key.StartsWith("MSC: text left", StringComparison.Ordinal));
    }

    // ── Differential fuzz: the port against the WinForms methods it mirrors ──────────────────────

    [WinFormsBuiltFact]
    public void Fuzz_SplitAndMerge_AreTheWinFormsExtractLyricsAndMergeSongs()
    {
        StaHelper.RunOnSta(() =>
        {
            var wf = new WinForms();
            var random = new Random(20261122);
            string[] paneLines = ["", "", " ", "a", "b b", "가사 한 줄", "[1]", "[2]", "[3]", "[chorus]", "[bridge]", "[10]", "[ending]", "[prechorus]", "[1] x", "x [2]", "[CHORUS]", "[4]", "[chorus 2]", "\t", "c\rd"];
            string[] storedLines = [.. paneLines, "[region 2]", "[region 2]", "[region 2] ", "x[region 2]"];
            string[] records = ["G;0;", "Am;2;D;5;", "C", "F;x;", ";3;", "G/B;0;Am(add9);4;"];
            string Text(string[] alphabet) => string.Join(random.Next(8) == 0 ? "\r\n" : "\n", Enumerable.Range(0, random.Next(0, 11)).Select(_ => alphabet[random.Next(alphabet.Length)]));
            string Notes(int lineCount)
            {
                var notes = new StringBuilder();
                int line = 0;
                for (int n = random.Next(0, 5); n > 0; n--)
                {
                    line = random.Next(10) == 0 ? random.Next(0, lineCount + 3) : line + random.Next(0, 3);
                    notes.Append(random.Next(25) == 0 ? "()" : random.Next(25) == 0 ? "(x;G;0;)" : $"({line};{records[random.Next(records.Length)]})");
                }

                return random.Next(30) == 0 ? notes + "(3;G;0;" : notes.ToString();
            }

            var report = new StringBuilder();
            int splitDiffs = 0, mergeDiffs = 0, cases = 30000;
            for (int i = 0; i < cases; i++)
            {
                string lyrics = Text(storedLines), msc = Notes(lyrics.Split('\n').Length);
                string expectedSplit, actualSplit;
                try { var e = wf.Extract(lyrics, msc); expectedSplit = $"{e.T1}¦{e.N1}¦{e.T2}¦{e.N2}"; }
                catch (TargetInvocationException ex) { expectedSplit = "throws " + ex.InnerException!.GetType().Name; }
                try { var a = SongEditorLyrics.Split(lyrics, msc); actualSplit = $"{a.Text1}¦{a.Notations1}¦{a.Text2}¦{a.Notations2}"; }
                catch (Exception ex) { actualSplit = "throws " + ex.GetType().Name; }
                if (expectedSplit != actualSplit && splitDiffs++ < 25)
                {
                    report.AppendLine($"SPLIT lyrics <{Esc(lyrics)}> msc <{msc}>\n  WinForms <{Esc(expectedSplit)}>\n  editor   <{Esc(actualSplit)}>");
                }

                string t1 = Text(paneLines), t2 = random.Next(3) == 0 ? "" : Text(paneLines);
                string n1 = Notes(t1.Split('\n').Length), n2 = Notes(t2.Split('\n').Length);
                string expectedMerge, actualMerge;
                try { var e = wf.Merge(t1, n1, t2, n2); expectedMerge = $"{e.Lyrics}¦{e.Notations}"; }
                catch (TargetInvocationException ex) { expectedMerge = "throws " + ex.InnerException!.GetType().Name; }
                try { var a = SongEditorLyrics.Merge(t1, n1, t2, n2); actualMerge = $"{a.Lyrics}¦{a.Notations}"; }
                catch (Exception ex) { actualMerge = "throws " + ex.GetType().Name; }
                if (expectedMerge != actualMerge && mergeDiffs++ < 25)
                {
                    report.AppendLine($"MERGE pane1 <{Esc(t1)}> n1 <{n1}> pane2 <{Esc(t2)}> n2 <{n2}>\n  WinForms <{Esc(expectedMerge)}>\n  editor   <{Esc(actualMerge)}>");
                }
            }

            report.Insert(0, $"cases: {cases}; Split differs: {splitDiffs}; Merge differs: {mergeDiffs}\n");
            WriteReport("fuzz.txt", report.ToString());
            Assert.True(splitDiffs == 0 && mergeDiffs == 0, report.ToString());
        });

        static string Esc(string text) => text.Replace("\r", "\\r").Replace("\n", "\\n").Replace("\t", "\\t");
    }

    // ── Fuzz of the view model: what the operator can type, paste or click ───────────────────────

    [Fact]
    public void Fuzz_WhateverIsTypedOrClicked_NothingThrows_AndASaveLosesNoLyricLine()
    {
        using var fixture = new Fixture();
        var random = new Random(4242);
        string[] tokens = ["", "", " ", "a", "b b", "가사 한 줄", "[1]", "[2]", "[3]", "[chorus]", "[bridge]", "G »", "G  C »", "  Am   D7 »", " »", "»", "G » ",
            "x » y", "G;1 »", "A) »", "(", "[", "]", "G\tC »", "«a»", "[1] x", "a\u00A0", "\t", "G(b »", "[chorus] »", "G » »", "한 줄 더"];
        string[] whitespaceAfterMarker = ["G »\t", "G »\u00A0", "G »\u3000"]; // counted on their own: these used to throw
        var failures = new SortedDictionary<string, (int Count, string Example)>();
        var lost = new List<string>();
        int saved = 0, refused = 0, cases = 20000;
        void Note(string operation, Exception ex, string example)
        {
            string frame = ex.StackTrace?.Split('\n').FirstOrDefault(line => line.Contains("Axicultus.Wpf"))?.Trim() ?? "";
            string key = $"{operation}: {ex.GetType().Name} {frame}";
            failures[key] = failures.TryGetValue(key, out var seen) ? (seen.Count + 1, seen.Example) : (1, example);
        }

        for (int i = 0; i < cases; i++)
        {
            bool withKnown = i % 10 == 0;
            string Pane() => string.Join(random.Next(4) == 0 ? "\r\n" : "\n", Enumerable.Range(0, random.Next(0, 9))
                .Select(_ => withKnown && random.Next(6) == 0 ? whitespaceAfterMarker[random.Next(3)] : tokens[random.Next(tokens.Length)]));
            string pane1 = Pane(), pane2 = random.Next(3) == 0 ? "" : Pane();
            string example = $"pane1 <{Esc(pane1)}> pane2 <{Esc(pane2)}>";
            string kind = withKnown ? "[with whitespace after »] " : "";
            var opened = fixture.Open("Old line");
            var vm = opened.Vm;
            bool threw = false;
            try { vm.LyricsRegion1 = pane1; } catch (Exception ex) { Note(kind + "typing in pane 1", ex, example); threw = true; }
            try { vm.LyricsRegion2 = pane2; } catch (Exception ex) { Note(kind + "typing in pane 2", ex, example); threw = true; }
            if (random.Next(3) == 0)
            {
                int pane = random.Next(1, 3);
                string text = pane == 1 ? vm.LyricsRegion1 : vm.LyricsRegion2;
                try { vm.InsertChord(pane, random.Next(0, text.Length + 1), "Am"); } catch (Exception ex) { Note(kind + "insert chord", ex, example); threw = true; }
            }

            if (random.Next(4) == 0)
            {
                try { vm.TransposeSongUpCommand.Execute(null); } catch (Exception ex) { Note(kind + "transpose", ex, example); threw = true; }
            }

            string shown1 = vm.LyricsRegion1, shown2 = vm.LyricsRegion2;
            try { vm.SaveAsync().GetAwaiter().GetResult(); } catch (Exception ex) { Note(kind + "save", ex, example); continue; }
            if (opened.Repo.LastSong is not { } song)
            {
                refused++;
                continue;
            }

            saved++;
            if (threw)
            {
                Note(kind + "SAVED AFTER AN EXCEPTION (stale text)", new InvalidOperationException(), example + $" saved <{Esc(song.Lyrics)}>");
                continue;
            }

            // Every line of the panes that is neither empty, a heading nor a chord line is in the saved lyrics, once.
            static IEnumerable<string> Words(string pane)
            {
                string[] lines = pane.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
                for (int index = 0; index < lines.Length; index++)
                {
                    if (lines[index].Length == 0 || IsKnownHeading(lines[index]))
                    {
                        continue;
                    }

                    if (SongEditorChords.IsChordLine(lines, index))
                    {
                        continue; // its lyric line follows and is counted
                    }

                    yield return lines[index];
                }
            }

            var expected = Words(shown1).Concat(Words(shown2)).Order(StringComparer.Ordinal).ToList();
            var actual = song.Lyrics.Split('\n').Where(line => line.Length > 0 && !IsKnownHeading(line) && line != "[region 2]").Order(StringComparer.Ordinal).ToList();
            if (!expected.SequenceEqual(actual) && lost.Count < 30)
            {
                lost.Add($"{example}\n   shown1 <{Esc(shown1)}> shown2 <{Esc(shown2)}>\n   saved <{Esc(song.Lyrics)}> msc <{song.Notations}>");
            }
        }

        var report = new StringBuilder($"cases {cases}: saved {saved}, refused {refused}\nEXCEPTIONS:\n");
        foreach (var (key, (count, example)) in failures)
        {
            report.AppendLine($"  {count,6}  {key}\n          e.g. {example}");
        }

        report.AppendLine($"SAVED WITH A LYRIC LINE LOST OR ADDED (first {lost.Count}):");
        lost.ForEach(line => report.AppendLine("  " + line));
        WriteReport("fuzz-viewmodel.txt", report.ToString());
        Assert.True(failures.Count == 0 && lost.Count == 0, report.ToString());

        static string Esc(string text) => text.Replace("\r", "\\r").Replace("\n", "\\n").Replace("\t", "\\t").Replace("\u00A0", "\\u00A0").Replace("\u3000", "\\u3000");
        static bool IsKnownHeading(string line) => line is "[chorus]" or "[bridge]" or "[ending]" or "[chorus 2]" or "[bridge 2]" or "[prechorus]" or "[prechorus 2]"
            || (line.Length is 3 or 4 && line[0] == '[' && line[^1] == ']' && int.TryParse(line.AsSpan(1, line.Length - 2), System.Globalization.NumberStyles.None, null, out int verse) && verse is >= 1 and <= 99 && line[1] != '0');
    }

    // Well-formed panes (headings on their own lines, unique lyric lines, chord lines above some): after a save
    // every line is under its verse, in its region, in its order, with its chords.
    [Fact]
    public void Fuzz_WellFormedPanes_EveryLineKeepsItsVerseRegionAndChords()
    {
        using var fixture = new Fixture();
        var random = new Random(99);
        var problems = new List<string>();
        int saved = 0, refused = 0, cases = 6000;
        var refusals = new SortedDictionary<string, int>();
        for (int i = 0; i < cases; i++)
        {
            int count = random.Next(1, 6);
            var all = Enumerable.Range(1, count).ToList();
            if (random.Next(2) == 0) all.Add(0);
            if (random.Next(4) == 0) all.Add(100);
            List<int> Pick(bool mayBeEmpty)
            {
                var picked = all.Where(_ => random.Next(4) != 0).OrderBy(_ => random.Next()).ToList();
                return picked.Count == 0 && !mayBeEmpty ? [all[0]] : picked;
            }

            var verses1 = Pick(false);
            var verses2 = random.Next(3) == 0 ? [] : Pick(true);
            foreach (int missing in all.Where(verse => verse is >= 1 and <= 99 && !verses1.Contains(verse) && !verses2.Contains(verse)))
            {
                verses1.Add(missing);
            }

            var expected = new Dictionary<string, (int Pane, int Verse, string Chords)>();
            string Build(int pane, List<int> verses)
            {
                var text = new List<string>();
                foreach (int verse in verses)
                {
                    text.Add(SongEditorLyrics.Symbol(verse));
                    int lines = random.Next(1, 5);
                    for (int n = 0; n < lines; n++)
                    {
                        string line = $"p{pane} v{verse} line {n} 은혜 grace";
                        string chords = "";
                        if (random.Next(3) == 0)
                        {
                            bool two = random.Next(2) == 0;
                            text.Add(two ? "G     D7 »" : "Am »");
                            chords = two ? "G  D7" : "Am";
                        }

                        text.Add(line);
                        expected[line] = (pane, verse, chords);
                        if (n + 1 < lines && random.Next(5) == 0) text.Add("");
                    }
                }

                return string.Join(random.Next(3) == 0 ? "\r\n" : "\n", text) + (random.Next(4) == 0 ? "\n" : "");
            }

            string pane1 = Build(1, verses1), pane2 = verses2.Count == 0 ? "" : Build(2, verses2);
            var opened = fixture.Open("Old line");
            opened.Vm.LyricsRegion1 = pane1;
            opened.Vm.LyricsRegion2 = pane2;
            opened.Vm.SaveAsync().GetAwaiter().GetResult();
            if (opened.Repo.LastSong is not { } song)
            {
                refused++;
                string key = Mask(opened.Vm.ValidationMessage);
                refusals[key] = refusals.GetValueOrDefault(key) + 1;
                continue;
            }

            saved++;
            var split = SongEditorLyrics.Split(song.Lyrics, song.Notations);
            var actual = new Dictionary<string, (int Pane, int Verse, string Chords)>();
            var order = new List<string>();
            foreach (var (text, notes, pane) in new[] { (split.Text1, split.Notations1, 1), (split.Text2, split.Notations2, 2) })
            {
                var names = SongEditorChords.NamesByLine(notes);
                int verse = 1, index = 0;
                foreach (string line in text.Split('\n'))
                {
                    int at = index++;
                    int heading = Array.IndexOf([.. Enumerable.Range(0, 113).Select(SongEditorLyrics.Symbol)], line);
                    if (heading >= 0) { verse = heading; continue; }
                    if (line.Length == 0) continue;
                    if (!actual.TryAdd(line, (pane, verse, names.GetValueOrDefault(at, ""))))
                    {
                        problems.Add($"LINE TWICE <{line}>");
                    }

                    order.Add(line);
                }
            }

            string[] wantOrder = [.. expected.Keys.Where(line => expected[line].Pane == 1), .. expected.Keys.Where(line => expected[line].Pane == 2)];
            bool sameLines = expected.Count == actual.Count && expected.All(pair => actual.TryGetValue(pair.Key, out var got) && got == pair.Value);
            // Within a pane the lines of one verse keep their order (verses themselves follow pane 1).
            bool sameOrder = expected.Values.Select(value => (value.Pane, value.Verse)).Distinct()
                .All(group => wantOrder.Where(line => (expected[line].Pane, expected[line].Verse) == group)
                    .SequenceEqual(order.Where(line => actual.TryGetValue(line, out var got) && (got.Pane, got.Verse) == group)));
            if ((!sameLines || !sameOrder) && problems.Count < 20)
            {
                string Flat(string text) => text.Replace("\r", "\\r").Replace("\n", "\\n");
                problems.Add($"pane1 <{Flat(pane1)}>\n  pane2 <{Flat(pane2)}>\n  saved <{Flat(song.Lyrics)}> msc <{song.Notations}>");
            }
        }

        var report = new StringBuilder($"cases {cases}: saved {saved}, refused {refused}; problems {problems.Count}\n");
        foreach (var (key, number) in refusals) report.AppendLine($"  refused {number}: {key}");
        problems.ForEach(problem => report.AppendLine(problem));
        WriteReport("fuzz-wellformed.txt", report.ToString());
        Assert.Empty(problems);
    }

    // ── Small facts: each asserts what the claims promise ────────────────────────────────────────

    private sealed class Fixture : IDisposable
    {
        public Fixture()
        {
            Root = Path.Combine(Path.GetTempPath(), $"Axicultus_Adversarial_{Guid.NewGuid():N}");
            Directory.CreateDirectory(Root);
            Database = Path.Combine(Root, "songs.db");
            File.WriteAllText(Database, "");
            Settings = new SettingsService(new SettingsServiceOptions(Path.Combine(Root, "settings.json"), Path.Combine(Root, "SettingsBackups")));
        }

        public string Root { get; }

        public string Database { get; }

        public ISettingsService Settings { get; }

        public Opened Open(string lyrics, string notations = "", string sequence = "")
            => SongEditorAdversarialTests.Open(Settings, Database, new SongDetail(10, "Song", "", 2, 1, lyrics, sequence, "", "", -1, "", "", notations, "", "", "", "", "", "", ""),
                [new SongFolderSummary(2, "Folder 2", true, 1)]);

        public void Dispose()
        {
            try { Directory.Delete(Root, recursive: true); } catch (IOException) { }
        }
    }

    [Theory]
    [InlineData("G »\t\nAmazing grace")]       // a tab after the marker (pasted text)
    [InlineData("G » \nAmazing grace")]   // a no-break space after the marker (text pasted from a web page)
    [InlineData("G »　\nAmazing grace")]   // a full-width space typed with a Korean/Japanese/Chinese input method
    public void WhitespaceAfterTheChordMarker_DoesNotThrow_AndTheStoredTextFollowsThePane(string pane)
    {
        using var fixture = new Fixture();
        var opened = fixture.Open("Old line");

        var failure = Record.Exception(() => opened.Vm.LyricsRegion1 = pane);

        Assert.Null(failure);
        Assert.Equal("Amazing grace", opened.Vm.Lyrics);
    }

    // Stored LYRICS with such a line (another program wrote it): opening the song must not throw out of LoadAsync.
    [Fact]
    public void AStoredLineWithWhitespaceAfterTheMarker_OpensWithoutAnException()
    {
        using var fixture = new Fixture();

        var failure = Record.Exception(() => fixture.Open("G »\u00A0\nAmazing grace"));

        Assert.Null(failure);
    }

    [Fact]
    public void AChordRecordOnAnEmptyLine_IsNotSavedAsLyricText_ByAnUntouchedSave()
    {
        using var fixture = new Fixture();
        var opened = fixture.Open("first\n\nlast", "(1;G;0;)");

        opened.Vm.SaveAsync().GetAwaiter().GetResult();

        Assert.Equal("first\n\nlast", opened.Repo.LastSong?.Lyrics ?? opened.Vm.ValidationMessage);
    }

    [Fact]
    public void AChordRecordOnALyricLineThatHoldsTheMarker_IsNotSavedAsLyricText_ByAnUntouchedSave()
    {
        using var fixture = new Fixture();
        var opened = fixture.Open("glory » amen\nlast", "(0;G;0;)");

        opened.Vm.SaveAsync().GetAwaiter().GetResult();

        Assert.Equal("glory » amen\nlast", opened.Repo.LastSong?.Lyrics ?? opened.Vm.ValidationMessage);
    }

    // "What is saved is what the operator sees": a chord line the pane shows is saved, or the save is refused.
    [Theory]
    [InlineData("[1]\nfirst line\nG   D »\n[2]\nsecond verse")]   // the heading menu puts a heading between a chord line and its lyric line
    public void AChordLineThePaneShows_IsSavedOrRefused(string pane)
    {
        using var fixture = new Fixture();
        var opened = fixture.Open("Old line");
        opened.Vm.LyricsRegion1 = pane;

        opened.Vm.SaveAsync().GetAwaiter().GetResult();

        Assert.True(opened.Vm.ValidationMessage.Length > 0 || opened.Repo.LastSong!.Notations.Contains("G;"),
            $"saved without a word: LYRICS <{opened.Repo.LastSong?.Lyrics.Replace("\n", "\\n")}> MSC <{opened.Repo.LastSong?.Notations}>");
    }

    [Fact]
    public void RemovingTheFirstOfTwoLinesThatLookTheSame_KeepsTheStoredPositionsOfTheOther()
    {
        using var fixture = new Fixture();
        var opened = fixture.Open("Same line\nOther\nSame line", "(0;B7;0;Em;1;)(2;B7;0;Em;3;)");
        Assert.Equal("B7 Em »\nSame line\nOther\nB7 Em »\nSame line", opened.Vm.LyricsRegion1);

        opened.Vm.LyricsRegion1 = "Other\nB7 Em »\nSame line"; // the first verse line and its chords deleted

        Assert.Equal("(1;B7;0;Em;3;)", opened.Vm.Notations);
    }

    // A pane that starts with an empty line was saved without that line, as text the editor refused the next time.
    [Fact]
    public void APaneThatStartsWithAnEmptyLine_IsCheckedAsItWillBeStored_AndWhatIsStoredCanBeSavedAgain()
    {
        using var fixture = new Fixture();
        var opened = fixture.Open("x");
        opened.Vm.LyricsRegion1 = "\nintro\n[2]\nverse";
        opened.Vm.SaveAsync().GetAwaiter().GetResult();
        Assert.Equal("기본 가사: [2] 앞의 가사에도 절 표시가 필요합니다.", opened.Vm.ValidationMessage);
        Assert.Null(opened.Repo.LastSong);

        // A song that was stored like that before is saved, and saved again after an edit.
        var stored = fixture.Open("intro\n[2]\nverse");
        stored.Vm.Title = "Renamed";
        stored.Vm.SaveAsync().GetAwaiter().GetResult();
        Assert.Equal("", stored.Vm.ValidationMessage);

        var reopened = fixture.Open(stored.Repo.LastSong!.Lyrics);
        reopened.Vm.LyricsRegion1 += "\nmore";
        reopened.Vm.SaveAsync().GetAwaiter().GetResult();
        Assert.Equal("", reopened.Vm.ValidationMessage);
        Assert.Equal("intro\n[2]\nverse\nmore", reopened.Repo.LastSong!.Lyrics);
    }

    // Not fixed, the same in both apps: a lyric line that ends with the marker character and has a line below it
    // is a chord line to FrmEditItem and to this editor, already when the song is opened. LYRICS/MSC cannot tell
    // the two apart; that takes a change of the stored format. (0 real songs.)
    [Fact]
    public void ALyricLineThatEndsWithTheMarkerCharacter_IsStoredAsAChordLine_AsFrmEditItemStoresIt()
    {
        using var fixture = new Fixture();
        var opened = fixture.Open("Il dit: « Viens »\net suis-moi");

        opened.Vm.SaveAsync().GetAwaiter().GetResult();

        Assert.Equal("et suis-moi", opened.Repo.LastSong!.Lyrics);
        Assert.Equal("0:Il  dit:  «  Viens", NamesOnly(opened.Repo.LastSong.Notations));
    }

    [Fact]
    public void TypingInAThreeHundredLineSong_KeepsEveryChordRecord()
    {
        using var fixture = new Fixture();
        var lyrics = new StringBuilder();
        var notes = new StringBuilder();
        int line = 0;
        for (int verse = 1; verse <= 30; verse++)
        {
            lyrics.Append('[').Append(verse).Append("]\n");
            line++;
            for (int row = 0; row < 9; row++, line++)
            {
                lyrics.Append("주님의 은혜가 나를 덮으시네 Amazing grace how sweet the sound ").Append(verse).Append('.').Append(row).Append('\n');
                notes.Append('(').Append(line).Append(";G;0;C;8;D7;16;Em;24;)");
            }
        }

        var opened = fixture.Open(lyrics.ToString().TrimEnd('\n'), notes.ToString());
        var watch = System.Diagnostics.Stopwatch.StartNew();
        string text = opened.Vm.LyricsRegion1;
        int at = text.Length / 2;
        for (int key = 0; key < 100; key++)
        {
            text = text.Insert(at, "가");
            opened.Vm.LyricsRegion1 = text;
        }

        watch.Stop();
        Assert.Equal(270, opened.Vm.Notations.Count(character => character == '('));
        WriteReport("typing-timing.txt", $"300-line song, 270 chord lines, character measure: {watch.ElapsedMilliseconds / 100.0:0.00} ms per keystroke (view model only, no window)\n");
    }

    private static string FirstDifference(string expected, string actual)
    {
        string[] a = expected.Split('\n'), b = actual.Split('\n');
        int line = 0;
        while (line < a.Length && line < b.Length && a[line] == b[line])
        {
            line++;
        }

        return $"line {line}: WinForms \"{(line < a.Length ? a[line] : "<end>")}\" ({a.Length} lines), editor \"{(line < b.Length ? b[line] : "<end>")}\" ({b.Length} lines)";
    }

    internal sealed class Repo : IAdminDatabaseRepository, IAdminSongDetailRepository
    {
        public SongWriteModel? LastSong { get; private set; }

        public int Saves { get; private set; }

        public Dictionary<int, SongDetail> Details { get; } = [];

        public List<SongFolderSummary> Folders { get; } = [];

        public Func<SongWriteModel, AdminDatabaseWriteReport?>? OnSave { get; set; }

        public Task<SongDetail?> GetSongDetailAsync(string databasePath, int songId)
            => Task.FromResult(Details.TryGetValue(songId, out var detail) ? detail : null);

        public Task<AdminDatabaseSchemaInventory> AnalyzeSchemaAsync(string databasePath) => throw new NotSupportedException();

        public Task<IReadOnlyList<SongFolderSummary>> GetSongFoldersAsync(string databasePath)
            => Task.FromResult<IReadOnlyList<SongFolderSummary>>(Folders);

        public Task<IReadOnlyList<SongSummary>> GetSongsAsync(string databasePath, int? folderNo = null) => throw new NotSupportedException();

        public Task<IReadOnlyList<DeletedSongSummary>> GetDeletedSongsAsync(string databasePath) => throw new NotSupportedException();

        public Task<AdminDatabaseWriteReport> SaveFolderAsync(string databasePath, string backupRoot, SongFolderWriteModel folder) => throw new NotSupportedException();

        public Task<AdminDatabaseWriteReport> SoftDeleteFoldersAsync(string databasePath, string backupRoot, IReadOnlyList<FolderDeleteRequest> deletes) => throw new NotSupportedException();

        public Task<AdminDatabaseWriteReport> RecoverFoldersAsync(string databasePath, string backupRoot, IReadOnlyList<FolderRecoveryRequest> recoveries) => throw new NotSupportedException();

        public Task<AdminDatabaseWriteReport> SaveSongAsync(string databasePath, string backupRoot, SongWriteModel song)
        {
            Saves++;
            LastSong = song;
            return Task.FromResult(OnSave?.Invoke(song) ?? new AdminDatabaseWriteReport(true, AdminDatabaseWriteOperation.SaveSong, databasePath,
                Path.Combine(backupRoot, "backup.db"), [song.SongId ?? 44], [song.FolderNo], Issues: []));
        }

        public Task<AdminDatabaseWriteReport> MoveSongsAsync(string databasePath, string backupRoot, IReadOnlyList<SongMoveRequest> moves) => throw new NotSupportedException();

        public Task<AdminDatabaseWriteReport> SoftDeleteSongsAsync(string databasePath, string backupRoot, IReadOnlyList<SongDeleteRequest> deletes) => throw new NotSupportedException();

        public Task<AdminDatabaseWriteReport> RecoverSongsAsync(string databasePath, string backupRoot, IReadOnlyList<SongRecoveryRequest> recoveries) => throw new NotSupportedException();

        public Task<AdminDatabaseWriteReport> ReorderFoldersAsync(string databasePath, string backupRoot, IReadOnlyList<FolderOrderRequest> order) => throw new NotSupportedException();

        public Task<AdminDatabaseWriteReport> ReorderSongsAsync(string databasePath, string backupRoot, int folderNo, IReadOnlyList<SongOrderRequest> order) => throw new NotSupportedException();
    }
}

[Collection("WPF Application")]
public class SongEditorAdversarialWindowTests
{
    // Typing through the TextBox of the window (binding, font measure, preview), then the chord menu.
    [Fact]
    public void TypingInTheWindow_KeepsEveryChordRecord_AndTheChordMenuUpdatesThePane() => StaHelper.RunOnSta(() =>
    {
        string root = Path.Combine(Path.GetTempPath(), $"Axicultus_AdversarialWindow_{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        string database = Path.Combine(root, "songs.db");
        File.WriteAllText(database, "");
        var settings = new SettingsService(new SettingsServiceOptions(Path.Combine(root, "settings.json"), Path.Combine(root, "SettingsBackups")));
        var resources = System.Windows.Application.Current.Resources;
        if (!resources.Contains("EsButton.Secondary"))
        {
            resources.MergedDictionaries.Add(new System.Windows.ResourceDictionary
            {
                Source = new Uri("pack://application:,,,/Axicultus;component/Controls/EsButton.xaml", UriKind.Absolute),
            });
        }

        var lyrics = new System.Text.StringBuilder();
        var notes = new System.Text.StringBuilder();
        int line = 0;
        for (int verse = 1; verse <= 30; verse++)
        {
            lyrics.Append('[').Append(verse).Append("]\n");
            line++;
            for (int row = 0; row < 9; row++, line++)
            {
                lyrics.Append("주님의 은혜가 나를 덮으시네 Amazing grace how sweet the sound ").Append(verse).Append('.').Append(row).Append('\n');
                notes.Append('(').Append(line).Append(";G;0;C;8;D7;16;Em;24;)");
            }
        }

        var repo = new SongEditorAdversarialTests.Repo();
        repo.Folders.Add(new SongFolderSummary(2, "Folder 2", true, 1));
        repo.Details[10] = new SongDetail(10, "Song", "", 2, 1, lyrics.ToString().TrimEnd('\n'), "", "", "", -1, "", "", notes.ToString(), "", "", "", "", "", "", "");
        var vm = new SongEditorViewModel(settings, repo, repo, null, null);
        var window = new SongEditorWindow(vm);
        try
        {
            var load = System.Diagnostics.Stopwatch.StartNew();
            vm.LoadAsync(database, new SongFolderSummary(2, "Folder 2", true, 1), new SongSummary(10, "Song", "", 2, 1, "", "", "")).GetAwaiter().GetResult();
            load.Stop();
            var editor = (System.Windows.Controls.TextBox)window.FindName("LyricsEditor");
            System.Windows.Threading.Dispatcher.CurrentDispatcher.Invoke(() => { }, System.Windows.Threading.DispatcherPriority.ApplicationIdle);
            int at = editor.Text.IndexOf("주님의 은혜가 나를 덮으시네 Amazing grace how sweet the sound 15.4", StringComparison.Ordinal) + 3;
            var typing = System.Diagnostics.Stopwatch.StartNew();
            for (int key = 0; key < 60; key++)
            {
                editor.Select(at + key, 0);
                editor.SelectedText = "가"; // as typed: one undo unit, the binding updates the view model
            }

            typing.Stop();
            bool canUndoAfterTyping = editor.CanUndo;
            string notationsAfterTyping = vm.Notations;
            int caret = vm.InsertChord(1, editor.Text.IndexOf("15.5", StringComparison.Ordinal), "Am");
            System.Windows.Threading.Dispatcher.CurrentDispatcher.Invoke(() => { }, System.Windows.Threading.DispatcherPriority.ApplicationIdle);
            Assert.Equal(270, notationsAfterTyping.Count(character => character == '('));
            Assert.Equal(vm.LyricsRegion1, editor.Text);
            Assert.Contains("Am", editor.Text[..caret]);
            SongEditorAdversarialTests.WriteReport("window-typing.txt", $"300-line song with 270 chord lines in the window font: load {load.ElapsedMilliseconds} ms; typing into a lyric line under a chord line {typing.ElapsedMilliseconds / 60.0:0.0} ms per character (TextBox + binding + view model, window not shown)\n" +
                $"undo available after typing: {canUndoAfterTyping}; after the chord menu inserted a chord: {editor.CanUndo}; pane text follows the view model: {editor.Text == vm.LyricsRegion1}\n" +
                $"chord records changed by the typing: {string.Join(" ", System.Text.RegularExpressions.Regex.Matches(notationsAfterTyping, @"\([^)]*\)").Select(m => m.Value).Except(System.Text.RegularExpressions.Regex.Matches(notes.ToString(), @"\([^)]*\)").Select(m => m.Value)))}\n");
        }
        finally
        {
            window.Close();
            try { Directory.Delete(root, recursive: true); } catch (IOException) { }
        }
    });
    // The 35 real songs with chords, measured in the font of the window's lyric panes.
    [SongEditorAdversarialTests.RealSongsFact]
    public void RealChordSongs_InTheFontOfTheWindow_KeepTheirStoredPositions() => StaHelper.RunOnSta(() =>
    {
        string root = Path.Combine(Path.GetTempPath(), $"Axicultus_AdversarialWindow_{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        string database = Path.Combine(root, "songs.db");
        File.WriteAllText(database, "");
        var settings = new SettingsService(new SettingsServiceOptions(Path.Combine(root, "settings.json"), Path.Combine(root, "SettingsBackups")));
        var resources = System.Windows.Application.Current.Resources;
        if (!resources.Contains("EsButton.Secondary"))
        {
            resources.MergedDictionaries.Add(new System.Windows.ResourceDictionary
            {
                Source = new Uri("pack://application:,,,/Axicultus;component/Controls/EsButton.xaml", UriKind.Absolute),
            });
        }

        var host = new SongEditorViewModel(settings, new SongEditorAdversarialTests.Repo());
        var window = new SongEditorWindow(host);
        var report = new System.Text.StringBuilder();
        try
        {
            var measure = host.MeasureEditorText;
            var editor = (System.Windows.Controls.TextBox)window.FindName("LyricsEditor");
            report.AppendLine($"pane font: {editor.FontFamily} {editor.FontSize}; width of 'W' {measure("W"):0.00}, 'i' {measure("i"):0.00}, ' ' {measure("a b") - measure("ab"):0.00}, '가' {measure("가"):0.00}");
            int songs = 0, chordLines = 0, drifting = 0, untouchedDiffers = 0, editsTried = 0, editsWrong = 0;
            var lookAlike = new List<string>();
            var wrong = new List<string>();
            var driftingSongs = new SortedSet<int>();
            foreach (var stored in SongEditorAdversarialTests.ReadRealSongs().Where(song => song.Notations.Length > 0))
            {
                songs++;
                var song = stored with { FolderNo = Math.Max(stored.FolderNo, 1) };
                var repo = new SongEditorAdversarialTests.Repo();
                repo.Folders.Add(new SongFolderSummary(song.FolderNo, "Folder", true, 1));
                repo.Details[song.SongId] = song;
                var vm = new SongEditorViewModel(settings, repo, repo, null, null) { MeasureEditorText = measure };
                vm.LoadAsync(database, new SongFolderSummary(song.FolderNo, "Folder", true, 1),
                    new SongSummary(song.SongId, song.Title, "", song.FolderNo, song.SongNumber, "", "", song.Lyrics)).GetAwaiter().GetResult();
                if (vm.Notations != song.Notations)
                {
                    untouchedDiffers++;
                    report.AppendLine($"song {song.SongId}: untouched MSC differs\n  stored {song.Notations}\n  editor {vm.Notations}");
                }

                var split = SongEditorLyrics.Split(song.Lyrics, song.Notations);
                foreach (int pane in (int[])[1, 2])
                {
                    string paneText = pane == 1 ? vm.LyricsRegion1 : vm.LyricsRegion2;
                    var rows = SongEditorAdversarialTests.Rows(pane == 1 ? split.Text1 : split.Text2, pane == 1 ? split.Notations1 : split.Notations2);
                    var display = paneText.Split('\n').ToList();
                    var map = SongEditorAdversarialTests.Map(rows, display);
                    if (map is null)
                    {
                        report.AppendLine($"song {song.SongId} pane {pane}: not shown as chord line + lyric line");
                        continue;
                    }

                    bool HasChordLine(int row) => map[row] > 0 && (row == 0 ? map[row] == 1 : map[row] - map[row - 1] == 2);
                    var pairs = new Dictionary<(string, string), HashSet<string>>();
                    var fresh = new SongEditorChords { Measure = measure };
                    for (int row = 0; row < rows.Count; row++)
                    {
                        if (!HasChordLine(row))
                        {
                            continue;
                        }

                        chordLines++;
                        string chordLine = display[map[row] - 1];
                        if (!pairs.TryGetValue((chordLine, rows[row].Line), out var set)) pairs[(chordLine, rows[row].Line)] = set = [];
                        set.Add(rows[row].Fields);
                        string reread = fresh.Read(chordLine + "\n" + rows[row].Line).Notations;
                        if (reread != $"(0;{rows[row].Fields})")
                        {
                            drifting++;
                            driftingSongs.Add(song.SongId);
                            if (drifting <= 12) report.AppendLine($"song {song.SongId} pane {pane}: <{chordLine}> over <{rows[row].Line}> stored {rows[row].Fields} read as typed {reread}");
                        }
                    }

                    foreach (var pair in pairs.Where(pair => pair.Value.Count > 1))
                    {
                        lookAlike.Add($"song {song.SongId} pane {pane}: <{pair.Key.Item1}> over <{pair.Key.Item2}> = {string.Join(" | ", pair.Value)}");
                    }

                    if (pane != 1)
                    {
                        continue;
                    }

                    // Harmless edits elsewhere: remove one lyric line (with its chord line), or put a new line above it.
                    for (int row = 0; row < rows.Count; row++)
                    {
                        if (rows[row].Line.Trim().Length == 0 || SongEditorAdversarialTests.IsHeading(rows[row].Line))
                        {
                            continue;
                        }

                        foreach (bool remove in (bool[])[true, false])
                        {
                            var model = rows.ToList();
                            var shown = display.ToList();
                            int at = map[row] - (HasChordLine(row) ? 1 : 0);
                            if (remove)
                            {
                                shown.RemoveRange(at, map[row] - at + 1);
                                model.RemoveAt(row);
                            }
                            else
                            {
                                shown.Insert(at, "새로 넣은 줄");
                                model.Insert(row, ("새로 넣은 줄", ""));
                            }

                            vm.LyricsRegion1 = string.Join('\n', shown);
                            var expected = SongEditorLyrics.Merge(SongEditorAdversarialTests.TextOf(model), SongEditorAdversarialTests.NotesOf(model), split.Text2, split.Notations2);
                            editsTried++;
                            if ((vm.Lyrics, vm.Notations) != expected)
                            {
                                editsWrong++;
                                if (wrong.Count < 12) wrong.Add($"song {song.SongId}: {(remove ? "removed" : "inserted above")} pane-1 line {row} <{rows[row].Line}>\n  expected {expected.Notations}\n  editor   {vm.Notations}");
                            }
                        }
                    }

                    vm.LyricsRegion1 = paneText;
                }
            }

            report.Insert(0, $"songs with MSC {songs}; chord lines {chordLines}; untouched MSC differs from stored: {untouchedDiffers}\n" +
                $"chord lines whose stored positions are not what the shown chord line reads back as (they move when that line is touched): {drifting} in {driftingSongs.Count} songs ({string.Join(' ', driftingSongs)})\n" +
                $"single-line edits elsewhere (remove a lyric line / insert a line): {editsTried}; MSC of the other lines wrong: {editsWrong}\n" +
                $"look-alike lines with different stored positions: {lookAlike.Count}\n");
            lookAlike.ForEach(line => report.AppendLine(line));
            wrong.ForEach(line => report.AppendLine(line));
            SongEditorAdversarialTests.WriteReport("window-msc.txt", report.ToString());
            Assert.True(untouchedDiffers == 0 && editsWrong == 0, report.ToString());
        }
        finally
        {
            window.Close();
            try { Directory.Delete(root, recursive: true); } catch (IOException) { }
        }
    });
    // The operator types in the TextBox; the binding hands the text to the view model. What is saved must be what the pane shows.
    [Theory]
    [InlineData("New first line\nG »\u00A0\nAmazing grace", "New first line\nAmazing grace")]
    [InlineData("New first line\nG »\t\nAmazing grace", "New first line\nAmazing grace")]
    [InlineData("New first line\nG »\nAmazing grace", "New first line\nAmazing grace")]
    public void TextTypedInTheWindow_IsWhatIsSaved(string typed, string expected) => StaHelper.RunOnSta(() =>
    {
        string root = Path.Combine(Path.GetTempPath(), $"Axicultus_AdversarialWindow_{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        string database = Path.Combine(root, "songs.db");
        File.WriteAllText(database, "");
        var settings = new SettingsService(new SettingsServiceOptions(Path.Combine(root, "settings.json"), Path.Combine(root, "SettingsBackups")));
        var resources = System.Windows.Application.Current.Resources;
        if (!resources.Contains("EsButton.Secondary"))
        {
            resources.MergedDictionaries.Add(new System.Windows.ResourceDictionary
            {
                Source = new Uri("pack://application:,,,/Axicultus;component/Controls/EsButton.xaml", UriKind.Absolute),
            });
        }

        var repo = new SongEditorAdversarialTests.Repo();
        repo.Folders.Add(new SongFolderSummary(2, "Folder 2", true, 1));
        repo.Details[10] = new SongDetail(10, "Song", "", 2, 1, "Old line", "", "", "", -1, "", "", "", "", "", "", "", "", "", "");
        var vm = new SongEditorViewModel(settings, repo, repo, null, null);
        var window = new SongEditorWindow(vm);
        try
        {
            vm.LoadAsync(database, new SongFolderSummary(2, "Folder 2", true, 1), new SongSummary(10, "Song", "", 2, 1, "", "", "Old line")).GetAwaiter().GetResult();
            var editor = (System.Windows.Controls.TextBox)window.FindName("LyricsEditor");
            System.Windows.Threading.Dispatcher.CurrentDispatcher.Invoke(() => { }, System.Windows.Threading.DispatcherPriority.ApplicationIdle);
            Assert.Equal("Old line", editor.Text); // the binding is live

            editor.Text = typed;
            System.Windows.Threading.Dispatcher.CurrentDispatcher.Invoke(() => { }, System.Windows.Threading.DispatcherPriority.ApplicationIdle);
            Assert.Equal(typed, vm.LyricsRegion1); // the pane text reached the view model
            vm.SaveAsync().GetAwaiter().GetResult();

            Assert.NotNull(repo.LastSong);
            SongEditorAdversarialTests.WriteReport("window-typed.txt", append: true, text: $"typed <{typed.Replace("\n", "\\n")}> saved LYRICS <{repo.LastSong!.Lyrics.Replace("\n", "\\n")}> status <{vm.StatusMessage}> validation <{vm.ValidationMessage}>\n");
            Assert.Equal(expected, repo.LastSong!.Lyrics);
            Assert.Equal("1:G", string.Join("|", SongEditorChords.NamesByLine(repo.LastSong.Notations).Select(pair => $"{pair.Key}:{pair.Value}")));
        }
        finally
        {
            window.Close();
            try { Directory.Delete(root, recursive: true); } catch (IOException) { }
        }
    });
}
