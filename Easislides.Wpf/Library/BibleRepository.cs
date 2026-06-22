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

/// <summary>
/// 성경 버전 관리에서 "추가" 후보 — HolyBibles 폴더엔 있으나 현재 보이지 않는 버전.
/// <see cref="IsHidden"/>=true 면 이전에 숨긴(삭제 예정) 버전(재추가 시 기존 이름 제안),
/// false 면 Biblefolder 에 행이 없는 신규 파일.
/// </summary>
public sealed record BibleAddableVersion(string FileName, string SuggestedName, bool IsHidden);

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

    /// <summary>
    /// 성경 선택 IdString 을 회중 화면 본문(절 단위 페이지·이중 언어면 <c>[region 2]</c> 포함) 가사로 확장한다.
    /// IdString 이 비었거나 형식 오류이거나 버전 파일을 못 찾으면 빈 문자열을 돌려 출력이 제목만 보이게 우아하게 폴백한다.
    /// 기본 구현은 빈 문자열 — 수많은 테스트 페이크가 일일이 구현하지 않도록 인터페이스 기본 메서드(DIM)로 둔다
    /// (실제 구절 확장은 <see cref="BibleRepository"/> 가 제공).
    /// </summary>
    string ExpandSelection(string workingFolder, string idString, bool showVerses) => string.Empty;

    /// <summary>
    /// 성경 버전의 표시 이름을 바꾼다(EsBiblesList.db 의 Biblefolder.NAME 컬럼만 UPDATE, 본문 파일은 불변).
    /// FILENAME 으로 행을 찾는다(버전당 파일 1:1). 새 이름이 비었거나 대상 파일이 없으면 false.
    /// 고유성(이름 중복) 검증은 호출자(VM)가 한다.
    /// </summary>
    bool RenameVersion(string workingFolder, string fileName, string newName);

    /// <summary>
    /// 보이는 성경 버전의 표시 순서를 바꾼다(Biblefolder.DISPLAYORDER 를 주어진 파일 순서대로 0,1,2,… 재기록).
    /// 목록에 없는 파일·숨김(DISPLAYORDER&lt;0) 버전은 건드리지 않는다. 매칭된 버전이 하나도 없으면 false.
    /// </summary>
    bool ReorderVersions(string workingFolder, IReadOnlyList<string> orderedFileNames);

    /// <summary>
    /// 성경 버전을 삭제한다 — 실제로는 DISPLAYORDER&lt;0 으로 숨겨 목록에서 제외(본문 파일·행 보존, 비파괴적).
    /// 되돌리려면 <see cref="AddVersion"/> 으로 재추가. 보이는 버전이 아니거나 없으면 false.
    /// </summary>
    bool DeleteVersion(string workingFolder, string fileName);

    /// <summary>
    /// "추가" 가능한 버전 목록 — HolyBibles 폴더의 성경 파일 중 현재 보이지 않는 것(숨김 행 + 신규 파일).
    /// 관리 UI 가 이 목록으로 추가 후보를 보여 준다.
    /// </summary>
    IReadOnlyList<BibleAddableVersion> GetAddableVersions(string workingFolder);

    /// <summary>
    /// 성경 버전을 추가(또는 숨김 복구)한다 — HolyBibles 에 파일이 있어야 한다. 숨김 행이 있으면 이름·순서를 되살리고,
    /// 없으면 새 행을 끝 순서로 INSERT. 파일이 없거나 이름이 비면 false.
    /// </summary>
    bool AddVersion(string workingFolder, string fileName, string name);
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

    public string ExpandSelection(string workingFolder, string idString, bool showVerses)
    {
        // IdString 이 비었거나 형식이 어긋나면 빈 본문(출력은 제목만) — 우아한 폴백.
        if (string.IsNullOrWhiteSpace(workingFolder) || !BibleSelectionId.TryParse(idString, out var selection))
        {
            return "";
        }

        var bibleFolder = Path.Combine(Path.GetFullPath(workingFolder), BibleFolderRelativePath);

        // 주 언어 절: 번호 표시 설정이면 "장:절 본문", 아니면 본문만.
        var region1Path = Path.Combine(bibleFolder, selection.Region1FileName);
        var region1 = ReadSelectionVerses(region1Path, selection.Ranges, includeReference: showVerses);

        // 보조 언어 절(이중 언어일 때만): 같은 절 번호를 또 붙이면 중복이라 본문만 싣는다.
        var region2 = selection.HasRegion2
            ? ReadSelectionVerses(Path.Combine(bibleFolder, selection.Region2FileName), selection.Ranges, includeReference: false)
            : Array.Empty<string>();

        return BiblePassageComposer.Compose(region1, region2);
    }

    // 한 버전 파일에서 선택 범위에 드는 구절 본문을 범위 순서대로 읽어 화면용 줄 목록으로 만든다.
    // 같은 책을 여러 범위가 참조해도 책당 한 번만 읽도록 캐시한다. 파일이 없으면 빈 목록(상위에서 빈 본문으로 폴백).
    private static IReadOnlyList<string> ReadSelectionVerses(
        string filePath,
        IReadOnlyList<BiblePassageRange> ranges,
        bool includeReference)
    {
        if (!File.Exists(filePath))
        {
            return Array.Empty<string>();
        }

        var rowsByBook = new Dictionary<int, IReadOnlyList<BibleVerseRow>>();
        var verses = new List<string>();
        foreach (var range in ranges)
        {
            if (!rowsByBook.TryGetValue(range.Book, out var rows))
            {
                rows = ReadVerses(filePath, range.Book);
                rowsByBook[range.Book] = rows;
            }

            foreach (var row in rows)
            {
                if (range.Contains(row.Chapter, row.Verse))
                {
                    verses.Add(includeReference ? $"{row.Chapter}:{row.Verse} {row.Text}" : row.Text);
                }
            }
        }

        return verses;
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

    public bool RenameVersion(string workingFolder, string fileName, string newName)
    {
        // 입력이 비면 변경 없음(VM 이 사전 검증하지만 저장소도 방어).
        if (string.IsNullOrWhiteSpace(workingFolder)
            || string.IsNullOrWhiteSpace(fileName)
            || string.IsNullOrWhiteSpace(newName))
        {
            return false;
        }

        var bibleListPath = Path.Combine(Path.GetFullPath(workingFolder), BibleListRelativePath);
        if (!File.Exists(bibleListPath))
        {
            return false;
        }

        using var connection = OpenConnection(bibleListPath);
        // 트랜잭션으로 원자적 변경(단일 컬럼 UPDATE 라 본문 파일·다른 행에 영향 없음).
        using var transaction = connection.BeginTransaction();
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        // DISPLAYORDER >= 0(= GetVersions 가 노출하는 보이는 버전)만 바꾼다.
        // 숨김(DISPLAYORDER<0) 행은 레거시에서 "삭제 예정" 임시 데이터라 rename 대상이 아니다.
        // 이렇게 하면 write 대상이 VM 의 중복 검사 집합(보이는 버전)과 정확히 일치한다.
        command.CommandText = "UPDATE Biblefolder SET NAME = @newName WHERE FILENAME = @fileName AND DISPLAYORDER >= 0;";
        command.Parameters.AddWithValue("@newName", newName.Trim());
        command.Parameters.AddWithValue("@fileName", fileName);
        var affected = command.ExecuteNonQuery();
        transaction.Commit();
        return affected > 0;
    }

    public bool ReorderVersions(string workingFolder, IReadOnlyList<string> orderedFileNames)
    {
        if (string.IsNullOrWhiteSpace(workingFolder) || orderedFileNames is null || orderedFileNames.Count == 0)
        {
            return false;
        }

        var bibleListPath = Path.Combine(Path.GetFullPath(workingFolder), BibleListRelativePath);
        if (!File.Exists(bibleListPath))
        {
            return false;
        }

        using var connection = OpenConnection(bibleListPath);
        using var transaction = connection.BeginTransaction();
        var affectedAny = false;
        var order = 0;
        foreach (var fileName in orderedFileNames)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                continue;
            }

            using var command = connection.CreateCommand();
            command.Transaction = transaction;
            // 보이는 버전(DISPLAYORDER>=0)만 재정렬한다 — 숨김 버전은 그대로 둔다.
            command.CommandText = "UPDATE Biblefolder SET DISPLAYORDER = @order WHERE FILENAME = @fileName AND DISPLAYORDER >= 0;";
            command.Parameters.AddWithValue("@order", order);
            command.Parameters.AddWithValue("@fileName", fileName);
            // 매칭된 보이는 버전에만 순서 번호를 매겨 조밀하게(0,1,2,…) 유지 — 알 수 없는 파일은 번호를 소모하지 않는다.
            if (command.ExecuteNonQuery() > 0)
            {
                affectedAny = true;
                order++;
            }
        }

        transaction.Commit();
        return affectedAny;
    }

    public bool DeleteVersion(string workingFolder, string fileName)
    {
        if (string.IsNullOrWhiteSpace(workingFolder) || string.IsNullOrWhiteSpace(fileName))
        {
            return false;
        }

        var bibleListPath = Path.Combine(Path.GetFullPath(workingFolder), BibleListRelativePath);
        if (!File.Exists(bibleListPath))
        {
            return false;
        }

        using var connection = OpenConnection(bibleListPath);
        using var transaction = connection.BeginTransaction();
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        // 삭제 = 숨김(DISPLAYORDER=-1). 본문 파일·행은 보존(되돌릴 수 있음). 이미 숨김/없는 행은 매칭 0 → false.
        command.CommandText = "UPDATE Biblefolder SET DISPLAYORDER = -1 WHERE FILENAME = @fileName AND DISPLAYORDER >= 0;";
        command.Parameters.AddWithValue("@fileName", fileName);
        var affected = command.ExecuteNonQuery();
        transaction.Commit();
        return affected > 0;
    }

    public IReadOnlyList<BibleAddableVersion> GetAddableVersions(string workingFolder)
    {
        if (string.IsNullOrWhiteSpace(workingFolder))
        {
            return [];
        }

        var root = Path.GetFullPath(workingFolder);
        var bibleListPath = Path.Combine(root, BibleListRelativePath);
        var bibleFolder = Path.Combine(root, BibleFolderRelativePath);

        // 현재 보이는 파일명·숨김 행(파일명→이름)을 모은다.
        var visible = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var hidden = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (File.Exists(bibleListPath))
        {
            using var connection = OpenConnection(bibleListPath);
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT NAME, FILENAME, DISPLAYORDER FROM Biblefolder;";
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var fileName = GetString(reader, "FILENAME");
                if (string.IsNullOrWhiteSpace(fileName))
                {
                    continue;
                }

                if (GetInt(reader, "DISPLAYORDER") >= 0)
                {
                    visible.Add(fileName);
                }
                else
                {
                    hidden[fileName] = GetString(reader, "NAME");
                }
            }
        }

        var result = new List<BibleAddableVersion>();
        // 숨김 버전(재추가 후보) — 기존 이름을 제안.
        foreach (var entry in hidden)
        {
            if (!visible.Contains(entry.Key))
            {
                var suggested = string.IsNullOrWhiteSpace(entry.Value)
                    ? Path.GetFileNameWithoutExtension(entry.Key)
                    : entry.Value;
                result.Add(new BibleAddableVersion(entry.Key, suggested, IsHidden: true));
            }
        }

        // HolyBibles 폴더의 신규 파일(Biblefolder 행이 전혀 없는 것) — 파일명을 기본 이름으로 제안.
        if (Directory.Exists(bibleFolder))
        {
            foreach (var path in Directory.EnumerateFiles(bibleFolder, "*.db"))
            {
                var fileName = Path.GetFileName(path);
                if (!visible.Contains(fileName) && !hidden.ContainsKey(fileName))
                {
                    result.Add(new BibleAddableVersion(fileName, Path.GetFileNameWithoutExtension(fileName), IsHidden: false));
                }
            }
        }

        return result;
    }

    public bool AddVersion(string workingFolder, string fileName, string name)
    {
        if (string.IsNullOrWhiteSpace(workingFolder)
            || string.IsNullOrWhiteSpace(fileName)
            || string.IsNullOrWhiteSpace(name))
        {
            return false;
        }

        var root = Path.GetFullPath(workingFolder);
        var bibleListPath = Path.Combine(root, BibleListRelativePath);
        if (!File.Exists(bibleListPath))
        {
            return false;
        }

        // 본문 파일이 HolyBibles 에 실제로 있어야 추가할 수 있다(빈 버전 방지).
        var filePath = Path.Combine(root, BibleFolderRelativePath, fileName);
        if (!File.Exists(filePath))
        {
            return false;
        }

        var trimmed = name.Trim();
        using var connection = OpenConnection(bibleListPath);
        using var transaction = connection.BeginTransaction();

        // 새 순서 = 보이는 버전 최대 DISPLAYORDER + 1(끝에 추가). 보이는 버전이 없으면 0.
        int nextOrder;
        using (var orderCommand = connection.CreateCommand())
        {
            orderCommand.Transaction = transaction;
            orderCommand.CommandText = "SELECT IFNULL(MAX(DISPLAYORDER), -1) + 1 FROM Biblefolder WHERE DISPLAYORDER >= 0;";
            nextOrder = Convert.ToInt32(orderCommand.ExecuteScalar());
        }

        // 같은 파일의 기존 행을 확인한다. 행이 없으면 INSERT, 숨김(DISPLAYORDER<0) 행이면 되살린다(UPDATE).
        // 이미 보이는 행(DISPLAYORDER>=0)이면 "추가" 대상이 아니므로 거부 — 기존 이름·순서를 덮어쓰지 않는다.
        int? existingOrder = null;
        using (var existsCommand = connection.CreateCommand())
        {
            existsCommand.Transaction = transaction;
            existsCommand.CommandText = "SELECT DISPLAYORDER FROM Biblefolder WHERE FILENAME = @fileName LIMIT 1;";
            existsCommand.Parameters.AddWithValue("@fileName", fileName);
            var raw = existsCommand.ExecuteScalar();
            if (raw is not null && raw is not DBNull)
            {
                existingOrder = Convert.ToInt32(raw);
            }
        }

        if (existingOrder is >= 0)
        {
            // 이미 보이는 버전 — 변경 없이 거부(using 이 트랜잭션을 롤백).
            return false;
        }

        var rowExists = existingOrder is < 0;

        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        if (rowExists)
        {
            command.CommandText = "UPDATE Biblefolder SET NAME = @name, DISPLAYORDER = @order WHERE FILENAME = @fileName;";
        }
        else
        {
            command.CommandText = """
                INSERT INTO Biblefolder (NAME, FILENAME, DESCRIPTION, COPYRIGHT, SONGFOLDER, SIZE, DISPLAYORDER)
                VALUES (@name, @fileName, '', '', 1, 80, @order);
                """;
        }

        command.Parameters.AddWithValue("@name", trimmed);
        command.Parameters.AddWithValue("@fileName", fileName);
        command.Parameters.AddWithValue("@order", nextOrder);
        var affected = command.ExecuteNonQuery();
        transaction.Commit();
        return affected > 0;
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
