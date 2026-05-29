using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;

namespace Easislides.Wpf.Library;

public enum BibleSearchMatchMode
{
    AllWords,
    AnyWord,
    ExactPhrase,
}

public sealed record BibleVersion(
    int Index,
    string Name,
    string Description,
    string Copyright,
    string FileName,
    string FilePath,
    int SongFolderNo,
    int Size,
    bool SupportsPartialWordSearch);

public sealed record BibleBook(int Number, string Name);

public sealed record BibleVerseLocation(
    int VersionIndex,
    int BookNumber,
    int Chapter,
    int Verse,
    int Start,
    int Length);

public sealed record BiblePassageResult(
    string Text,
    IReadOnlyList<BibleVerseLocation> Locations,
    bool IsSequential,
    bool WasLimited);

public sealed record BibleSelection(string IdString, string Title);

public interface IBibleRepository
{
    IReadOnlyList<BibleVersion> GetVersions(string workingFolder);

    IReadOnlyList<BibleBook> GetBooks(BibleVersion version);

    BiblePassageResult LoadBook(BibleVersion version, int bookNumber, bool showVerses);

    BiblePassageResult Search(
        BibleVersion version,
        IReadOnlyList<BibleBook> books,
        string searchText,
        BibleSearchMatchMode matchMode,
        bool showVerses,
        int maxResults = 3000);

    BibleSelection BuildSelection(
        BibleVersion version,
        IReadOnlyList<BibleBook> books,
        BiblePassageResult result,
        int selectionStart,
        int selectionLength,
        int maxSequentialSelection = 100,
        int maxAdHocSelection = 100);

    BibleSelection ChangeSelectionVersions(
        string currentTitle,
        string currentIdString,
        BibleVersion region1,
        BibleVersion? region2);
}

public sealed class BibleRepository : IBibleRepository
{
    private const string BibleListRelativePath = @"Admin\Database\EsBiblesList.db";
    private const string BibleFolderRelativePath = "HolyBibles";

    public IReadOnlyList<BibleVersion> GetVersions(string workingFolder)
    {
        if (string.IsNullOrWhiteSpace(workingFolder))
        {
            return [];
        }

        var root = Path.GetFullPath(workingFolder);
        var bibleListPath = Path.Combine(root, BibleListRelativePath);
        if (!File.Exists(bibleListPath))
        {
            return [];
        }

        var bibleFolder = Path.Combine(root, BibleFolderRelativePath);
        var versions = new List<BibleVersion>();
        using var connection = OpenConnection(bibleListPath);
        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT NAME, FILENAME, DESCRIPTION, COPYRIGHT, SONGFOLDER, SIZE, DISPLAYORDER
            FROM Biblefolder
            WHERE DISPLAYORDER >= 0
            ORDER BY DISPLAYORDER, NAME;
            """;

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var fileName = GetString(reader, "FILENAME");
            var filePath = Path.Combine(bibleFolder, fileName);
            var songFolder = ClampSongFolder(GetInt(reader, "SONGFOLDER"));
            var size = ClampSize(GetInt(reader, "SIZE"));
            versions.Add(new BibleVersion(
                versions.Count,
                GetString(reader, "NAME"),
                GetString(reader, "DESCRIPTION"),
                GetString(reader, "COPYRIGHT"),
                fileName,
                filePath,
                songFolder,
                size,
                SupportsPartialWordSearch: HasPartialWordSearchMarker(filePath)));
        }

        return versions;
    }

    public IReadOnlyList<BibleBook> GetBooks(BibleVersion version)
    {
        ArgumentNullException.ThrowIfNull(version);
        if (!File.Exists(version.FilePath))
        {
            return [];
        }

        var books = new List<BibleBook>();
        using var connection = OpenConnection(version.FilePath);
        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT verse, bibletext
            FROM Bible
            WHERE book = 0 AND chapter = 10 AND verse > 0 AND verse <= 66
            ORDER BY verse;
            """;

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            books.Add(new BibleBook(GetInt(reader, "verse"), GetString(reader, "bibletext")));
        }

        return books;
    }

    public BiblePassageResult LoadBook(BibleVersion version, int bookNumber, bool showVerses)
    {
        ArgumentNullException.ThrowIfNull(version);
        var rows = ReadVerses(version.FilePath, bookNumber);
        return BuildResult(version, rows, GetBookName([], bookNumber), showVerses, isSequential: true, maxResults: int.MaxValue);
    }

    public BiblePassageResult Search(
        BibleVersion version,
        IReadOnlyList<BibleBook> books,
        string searchText,
        BibleSearchMatchMode matchMode,
        bool showVerses,
        int maxResults = 3000)
    {
        ArgumentNullException.ThrowIfNull(version);
        ArgumentNullException.ThrowIfNull(books);

        var terms = SplitTerms(searchText);
        if (terms.Count == 0)
        {
            return new BiblePassageResult("", [], IsSequential: false, WasLimited: false);
        }

        var rows = ReadVerses(version.FilePath, bookNumber: null)
            .Where(row => Matches(row.Text, terms, matchMode, version.SupportsPartialWordSearch))
            .Take(Math.Max(maxResults, 1) + 1)
            .ToArray();

        var limited = rows.Length > Math.Max(maxResults, 1);
        var page = limited ? rows.Take(Math.Max(maxResults, 1)).ToArray() : rows;
        return BuildResult(version, page, books, showVerses, isSequential: false, Math.Max(maxResults, 1), limited);
    }

    public BibleSelection BuildSelection(
        BibleVersion version,
        IReadOnlyList<BibleBook> books,
        BiblePassageResult result,
        int selectionStart,
        int selectionLength,
        int maxSequentialSelection = 100,
        int maxAdHocSelection = 100)
    {
        ArgumentNullException.ThrowIfNull(version);
        ArgumentNullException.ThrowIfNull(books);
        ArgumentNullException.ThrowIfNull(result);
        if (result.Locations.Count == 0)
        {
            return new BibleSelection("", "");
        }

        var selected = GetSelectedLocations(result, selectionStart, selectionLength);
        var limit = result.IsSequential ? maxSequentialSelection : maxAdHocSelection;
        selected = selected.Take(Math.Max(limit, 1)).ToArray();
        if (selected.Count == 0)
        {
            return new BibleSelection("", "");
        }

        return result.IsSequential
            ? BuildSequentialSelection(version, books, selected)
            : BuildAdHocSelection(version, books, selected);
    }

    public BibleSelection ChangeSelectionVersions(
        string currentTitle,
        string currentIdString,
        BibleVersion region1,
        BibleVersion? region2)
    {
        ArgumentNullException.ThrowIfNull(region1);
        if (string.IsNullOrWhiteSpace(currentIdString))
        {
            return new BibleSelection("", "");
        }

        var parts = currentIdString.Split(';');
        if (parts.Length < 4)
        {
            return new BibleSelection("", "");
        }

        var tail = string.Join(';', parts.Skip(3));
        var idString = $"{parts[0]};{region1.FileName};{region2?.FileName ?? ""};{tail}";
        var baseTitle = StripVersionSuffix(currentTitle);
        var suffix = region2 is null ? region1.Name : $"{region1.Name}/{region2.Name}";
        return new BibleSelection(idString, $"{baseTitle} ({suffix})");
    }

    private static BiblePassageResult BuildResult(
        BibleVersion version,
        IReadOnlyList<BibleVerseRow> rows,
        IReadOnlyList<BibleBook> books,
        bool showVerses,
        bool isSequential,
        int maxResults,
        bool wasLimited = false)
    {
        var text = new StringBuilder();
        var locations = new List<BibleVerseLocation>();
        foreach (var row in rows.Take(maxResults))
        {
            var start = text.Length;
            if (isSequential)
            {
                var prefix = $"{row.Chapter}:{row.Verse} ";
                text.Append(prefix);
            }
            else
            {
                text.Append(GetBookName(books, row.Book)).Append(' ')
                    .Append(row.Chapter).Append(':').Append(row.Verse).Append(' ');
            }

            if (showVerses)
            {
                text.Append(row.Text);
            }

            text.Append("\n\n");
            locations.Add(new BibleVerseLocation(
                version.Index,
                row.Book,
                row.Chapter,
                row.Verse,
                start,
                text.Length - start));
        }

        return new BiblePassageResult(TrimEndNewLines(text), locations, isSequential, wasLimited);
    }

    private static BiblePassageResult BuildResult(
        BibleVersion version,
        IReadOnlyList<BibleVerseRow> rows,
        string bookName,
        bool showVerses,
        bool isSequential,
        int maxResults)
        => BuildResult(version, rows, [new BibleBook(rows.FirstOrDefault()?.Book ?? 0, bookName)], showVerses, isSequential, maxResults);

    private static BibleSelection BuildSequentialSelection(
        BibleVersion version,
        IReadOnlyList<BibleBook> books,
        IReadOnlyList<BibleVerseLocation> selected)
    {
        var first = selected[0];
        var last = selected[^1];
        var id = new StringBuilder($"0;{version.FileName};;");
        foreach (var group in GroupSequentialLocations(selected))
        {
            id.Append(group.BookNumber).Append(';')
                .Append(group.StartChapter).Append(';')
                .Append(group.StartVerse).Append(';')
                .Append(group.EndChapter).Append(';')
                .Append(group.EndVerse).Append(';');
        }

        var title = GetBookName(books, first.BookNumber) + " " + FormatRange(first, last) + $" ({version.Name})";
        return new BibleSelection(id.ToString(), title);
    }

    private static BibleSelection BuildAdHocSelection(
        BibleVersion version,
        IReadOnlyList<BibleBook> books,
        IReadOnlyList<BibleVerseLocation> selected)
    {
        var id = new StringBuilder($"1;{version.FileName};;");
        var title = new StringBuilder();
        foreach (var location in selected)
        {
            id.Append(location.BookNumber).Append(';')
                .Append(location.Chapter).Append(';')
                .Append(location.Verse).Append(';')
                .Append(location.Chapter).Append(';')
                .Append(location.Verse).Append(';');
            title.Append(TrimBookName(GetBookName(books, location.BookNumber)))
                .Append(' ')
                .Append(location.Chapter)
                .Append(':')
                .Append(location.Verse)
                .Append(',');
        }

        var titleText = title.ToString().TrimEnd(',');
        if (titleText.Length > 60)
        {
            titleText = titleText[..60] + " .. ";
        }

        return new BibleSelection(id.ToString(), $"{titleText} ({version.Name})");
    }

    private static IReadOnlyList<BibleVerseLocation> GetSelectedLocations(
        BiblePassageResult result,
        int selectionStart,
        int selectionLength)
    {
        var start = Math.Max(selectionStart + 2, 0);
        var end = Math.Max(start, selectionStart + Math.Max(selectionLength, 0));
        var firstIndex = result.Locations.ToList().FindIndex(location => ContainsOffset(location, start));
        if (firstIndex < 0)
        {
            firstIndex = 0;
        }

        var lastIndex = firstIndex;
        for (var index = firstIndex; index < result.Locations.Count; index++)
        {
            if (ContainsOffset(result.Locations[index], end) || result.Locations[index].Start <= end)
            {
                lastIndex = index;
                continue;
            }

            break;
        }

        return result.Locations.Skip(firstIndex).Take(lastIndex - firstIndex + 1).ToArray();
    }

    private static bool ContainsOffset(BibleVerseLocation location, int offset)
        => offset >= location.Start && offset <= location.Start + location.Length;

    private static IEnumerable<(int BookNumber, int StartChapter, int StartVerse, int EndChapter, int EndVerse)> GroupSequentialLocations(
        IReadOnlyList<BibleVerseLocation> locations)
    {
        var start = locations[0];
        var end = start;
        for (var index = 1; index < locations.Count; index++)
        {
            var next = locations[index];
            if (next.BookNumber == end.BookNumber && next.Chapter == end.Chapter)
            {
                end = next;
                continue;
            }

            yield return (start.BookNumber, start.Chapter, start.Verse, end.Chapter, end.Verse);
            start = end = next;
        }

        yield return (start.BookNumber, start.Chapter, start.Verse, end.Chapter, end.Verse);
    }

    private static string FormatRange(BibleVerseLocation first, BibleVerseLocation last)
    {
        var start = $"{first.Chapter}:{first.Verse}";
        if (first.Chapter == last.Chapter && first.Verse == last.Verse)
        {
            return start;
        }

        return first.Chapter == last.Chapter
            ? $"{start}-{last.Verse}"
            : $"{start} - {last.Chapter}:{last.Verse}";
    }

    private static IReadOnlyList<BibleVerseRow> ReadVerses(string filePath, int? bookNumber)
    {
        if (!File.Exists(filePath))
        {
            return [];
        }

        var rows = new List<BibleVerseRow>();
        using var connection = OpenConnection(filePath);
        using var command = connection.CreateCommand();
        command.CommandText = bookNumber is null
            ? "SELECT book, chapter, verse, bibletext FROM Bible WHERE book > 0 ORDER BY book, chapter, verse;"
            : "SELECT book, chapter, verse, bibletext FROM Bible WHERE book = @book ORDER BY chapter, verse;";
        if (bookNumber is not null)
        {
            command.Parameters.AddWithValue("@book", bookNumber.Value);
        }

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            rows.Add(new BibleVerseRow(
                GetInt(reader, "book"),
                GetInt(reader, "chapter"),
                GetInt(reader, "verse"),
                GetString(reader, "bibletext")));
        }

        return rows;
    }

    private static bool Matches(
        string text,
        IReadOnlyList<string> terms,
        BibleSearchMatchMode matchMode,
        bool supportsPartialWord)
    {
        var normalized = text.ToLowerInvariant();
        return matchMode switch
        {
            BibleSearchMatchMode.AnyWord => terms.Any(term => ContainsTerm(normalized, term, supportsPartialWord)),
            BibleSearchMatchMode.ExactPhrase => normalized.Contains(string.Join(' ', terms), StringComparison.Ordinal),
            _ => terms.All(term => ContainsTerm(normalized, term, supportsPartialWord)),
        };
    }

    private static bool ContainsTerm(string normalizedText, string term, bool supportsPartialWord)
        => supportsPartialWord
            ? normalizedText.Contains(term, StringComparison.Ordinal)
            : normalizedText.Split([' ', '\t', '\r', '\n', ',', '.', ';', ':', '!', '?', '"', '\'', '(', ')', '[', ']'], StringSplitOptions.RemoveEmptyEntries)
                .Any(word => string.Equals(word, term, StringComparison.Ordinal));

    private static IReadOnlyList<string> SplitTerms(string searchText)
        => (searchText ?? "")
            .Trim()
            .ToLowerInvariant()
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    private static bool HasPartialWordSearchMarker(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                return false;
            }

            using var connection = OpenConnection(filePath);
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM Bible WHERE book = 0 AND chapter = 0 AND verse = 20;";
            return Convert.ToInt32(command.ExecuteScalar()) > 0;
        }
        catch (SQLiteException)
        {
            return false;
        }
    }

    private static SQLiteConnection OpenConnection(string databasePath)
    {
        var connection = new SQLiteConnection($"Data Source={databasePath}");
        connection.Open();
        return connection;
    }

    private static string GetString(SQLiteDataReader reader, string column)
    {
        var ordinal = reader.GetOrdinal(column);
        return reader.IsDBNull(ordinal) ? "" : Convert.ToString(reader.GetValue(ordinal))?.Trim() ?? "";
    }

    private static int GetInt(SQLiteDataReader reader, string column)
    {
        var ordinal = reader.GetOrdinal(column);
        return reader.IsDBNull(ordinal) ? 0 : Convert.ToInt32(reader.GetValue(ordinal));
    }

    private static int ClampSongFolder(int value)
        => value < 1 ? 1 : value;

    private static int ClampSize(int value)
        => value is < 5 or > 200 ? 80 : value;

    private static string GetBookName(IReadOnlyList<BibleBook> books, int bookNumber)
        => books.FirstOrDefault(book => book.Number == bookNumber)?.Name ?? $"Book {bookNumber}";

    private static string TrimBookName(string bookName)
        => bookName.Length <= 4 ? bookName.Trim() : bookName[..4].Trim();

    private static string StripVersionSuffix(string title)
    {
        var index = title.IndexOf('(', StringComparison.Ordinal);
        return index > 0 ? title[..index].Trim() : title.Trim();
    }

    private static string TrimEndNewLines(StringBuilder builder)
        => builder.ToString().TrimEnd('\r', '\n');

    private sealed record BibleVerseRow(int Book, int Chapter, int Verse, string Text);
}
