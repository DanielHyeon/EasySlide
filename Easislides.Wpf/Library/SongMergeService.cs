using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Easislides.Wpf.Data;

namespace Easislides.Wpf.Library;

public sealed record SongMergeResult(string Lyrics, string Notations);

public interface ISongMergeService
{
    SongMergeResult Merge(SongDetail sourceA, SongDetail sourceB);
}

public sealed class SongMergeService : ISongMergeService
{
    private const string RegionTwoHeading = "[region 2]";

    public SongMergeResult Merge(SongDetail sourceA, SongDetail sourceB)
    {
        ArgumentNullException.ThrowIfNull(sourceA);
        ArgumentNullException.ThrowIfNull(sourceB);

        var notationA = ParseNotations(sourceA.Notations);
        var notationB = ParseNotations(sourceB.Notations);
        var output = MergeLines(ParseSections(sourceA.Lyrics), ParseSections(sourceB.Lyrics));
        var lyrics = string.Join("\n", output.Select(line => line.Text)).TrimEnd();
        var notations = BuildNotations(output, notationA, notationB);
        return new SongMergeResult(lyrics, notations);
    }

    private static IReadOnlyList<MergedLine> MergeLines(
        IReadOnlyList<LyricSection> sourceA,
        IReadOnlyList<LyricSection> sourceB)
    {
        var output = new List<MergedLine>();
        if (sourceA.Count == 0)
        {
            AddSourceBOnly(output, sourceB, new HashSet<int>());
            return output;
        }

        if (sourceA.Count == 1 && sourceA[0].Heading is null)
        {
            AddLines(output, sourceA[0].Lines, MergeSource.SourceA);
            var bLines = sourceB.SelectMany(section => section.Lines).ToArray();
            if (bLines.Length > 0)
            {
                output.Add(new MergedLine(RegionTwoHeading, MergeSource.Inserted, -1));
                AddLines(output, bLines, MergeSource.SourceB);
            }

            return output;
        }

        var consumedB = new HashSet<int>();
        foreach (var sectionA in sourceA)
        {
            if (sectionA.Heading is not null)
            {
                output.Add(new MergedLine(sectionA.Heading, MergeSource.Inserted, -1));
            }

            AddLines(output, sectionA.Lines, MergeSource.SourceA);
            var matchingB = FindMatchingSection(sourceB, sectionA.Heading, consumedB);
            if (matchingB is null)
            {
                continue;
            }

            consumedB.Add(matchingB.Value.Index);
            if (matchingB.Value.Section.Lines.Count > 0)
            {
                output.Add(new MergedLine(RegionTwoHeading, MergeSource.Inserted, -1));
                AddLines(output, matchingB.Value.Section.Lines, MergeSource.SourceB);
            }
        }

        AddSourceBOnly(output, sourceB, consumedB);
        return output;
    }

    private static void AddSourceBOnly(List<MergedLine> output, IReadOnlyList<LyricSection> sourceB, HashSet<int> consumedB)
    {
        for (var index = 0; index < sourceB.Count; index++)
        {
            if (consumedB.Contains(index))
            {
                continue;
            }

            var section = sourceB[index];
            if (section.Heading is not null)
            {
                output.Add(new MergedLine(section.Heading, MergeSource.Inserted, -1));
            }

            if (section.Lines.Count == 0)
            {
                continue;
            }

            output.Add(new MergedLine(RegionTwoHeading, MergeSource.Inserted, -1));
            AddLines(output, section.Lines, MergeSource.SourceB);
        }
    }

    private static (int Index, LyricSection Section)? FindMatchingSection(
        IReadOnlyList<LyricSection> sourceB,
        string? heading,
        HashSet<int> consumedB)
    {
        if (heading is null)
        {
            return null;
        }

        for (var index = 0; index < sourceB.Count; index++)
        {
            if (consumedB.Contains(index))
            {
                continue;
            }

            if (string.Equals(sourceB[index].Heading, heading, StringComparison.OrdinalIgnoreCase))
            {
                return (index, sourceB[index]);
            }
        }

        return null;
    }

    private static void AddLines(List<MergedLine> output, IEnumerable<SourceLine> lines, MergeSource source)
    {
        foreach (var line in lines)
        {
            output.Add(new MergedLine(line.Text, source, line.Index));
        }
    }

    private static IReadOnlyList<LyricSection> ParseSections(string lyrics)
    {
        var normalized = (lyrics ?? string.Empty).Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n').TrimEnd();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return [];
        }

        var sections = new List<LyricSection>();
        LyricSection? current = null;
        var lines = normalized.Split('\n');
        for (var index = 0; index < lines.Length; index++)
        {
            var line = lines[index].TrimEnd();
            if (IsHeading(line))
            {
                current = new LyricSection(line, []);
                sections.Add(current);
                continue;
            }

            current ??= AddSection(sections, null);
            current.Lines.Add(new SourceLine(line, index));
        }

        return sections;
    }

    private static LyricSection AddSection(List<LyricSection> sections, string? heading)
    {
        var section = new LyricSection(heading, []);
        sections.Add(section);
        return section;
    }

    private static bool IsHeading(string line)
        => line.Length >= 3 && line[0] == '[' && line[^1] == ']';

    private static IReadOnlyDictionary<int, string> ParseNotations(string notations)
    {
        var map = new Dictionary<int, string>();
        if (string.IsNullOrWhiteSpace(notations))
        {
            return map;
        }

        var index = 0;
        while (index < notations.Length)
        {
            var open = notations.IndexOf('(', index);
            if (open < 0)
            {
                break;
            }

            var separator = notations.IndexOf(';', open + 1);
            var close = notations.IndexOf(')', separator + 1);
            if (separator < 0 || close < 0)
            {
                break;
            }

            if (int.TryParse(notations[(open + 1)..separator], out var lineIndex))
            {
                map[lineIndex] = notations[(separator + 1)..close];
            }

            index = close + 1;
        }

        return map;
    }

    private static string BuildNotations(
        IReadOnlyList<MergedLine> output,
        IReadOnlyDictionary<int, string> notationA,
        IReadOnlyDictionary<int, string> notationB)
    {
        var builder = new StringBuilder();
        for (var index = 0; index < output.Count; index++)
        {
            var line = output[index];
            var sourceNotations = line.Source == MergeSource.SourceA ? notationA : notationB;
            if (line.Source == MergeSource.Inserted ||
                line.SourceLineIndex < 0 ||
                !sourceNotations.TryGetValue(line.SourceLineIndex, out var notation) ||
                string.IsNullOrWhiteSpace(notation))
            {
                continue;
            }

            builder.Append('(')
                .Append(index)
                .Append(';')
                .Append(notation)
                .Append(')');
        }

        return builder.ToString();
    }

    private sealed record SourceLine(string Text, int Index);

    private sealed record LyricSection(string? Heading, List<SourceLine> Lines);

    private sealed record MergedLine(string Text, MergeSource Source, int SourceLineIndex);

    private enum MergeSource
    {
        Inserted,
        SourceA,
        SourceB,
    }
}
