using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Axicultus.Module;
using Axicultus.Wpf.Data;
using Axicultus.Wpf.Library;
using FluentAssertions;
using Xunit;

namespace Axicultus.Wpf.Tests.Data;

// Tests of an adversarial review, run on a temp copy of a real library; the originals are only read.
// Set AXICULTUS_REAL_SONG_DB to the song database: EsUsage.db and the two extension lists are taken from the same
// folder. Without them the tests are skipped. The numbers asserted (songs of folder 9, the extensions) are those
// of the library the review was made with. Each finding also has a small test of its own that needs no real file
// (AdminDatabaseRepositoryTests, SearchUsageServiceTests, SongFormatDataTests).
public class ReviewFindingsTests : IDisposable
{
    private sealed class RealDatabasesFactAttribute : FactAttribute
    {
        public RealDatabasesFactAttribute()
        {
            if (!File.Exists(RealSongDatabase) || !File.Exists(Path.Combine(Real, "EsUsage.db")))
            {
                Skip = "Set AXICULTUS_REAL_SONG_DB to a song database with EsUsage.db next to it (they are copied, never written).";
            }
        }
    }

    private static string RealSongDatabase => Environment.GetEnvironmentVariable("AXICULTUS_REAL_SONG_DB") ?? "";

    private static string Real => Path.GetDirectoryName(RealSongDatabase) ?? "";

    private readonly string _root = Path.Combine(Path.GetTempPath(), $"Axicultus_Review_{Guid.NewGuid():N}");
    private readonly string _db;
    private readonly string _usage;
    private readonly string _backups;

    public ReviewFindingsTests()
    {
        Directory.CreateDirectory(_root);
        _db = Path.Combine(_root, "EasiSlidesDb.db");
        _usage = Path.Combine(_root, "EsUsage.db");
        _backups = Path.Combine(_root, "Backups");
        File.Copy(RealSongDatabase, _db);
        File.Copy(Path.Combine(Real, "EsUsage.db"), _usage);
    }

    public void Dispose()
    {
        SQLiteConnection.ClearAllPools();
        try { Directory.Delete(_root, recursive: true); } catch (IOException) { } catch (UnauthorizedAccessException) { }
    }

    private T Scalar<T>(string path, string sql)
    {
        using var c = new SQLiteConnection(new SQLiteConnectionStringBuilder { DataSource = path, ReadOnly = true }.ToString());
        c.Open();
        using var cmd = new SQLiteCommand(sql, c);
        return (T)Convert.ChangeType(cmd.ExecuteScalar()!, typeof(T));
    }

    [RealDatabasesFact]
    public async Task ChorusCodeNul_SurvivesTheRealDatabase_AndCopyKeepsEveryColumn()
    {
        var sut = new AdminDatabaseRepository();
        var saved = await sut.SaveSongAsync(_db, _backups, new SongWriteModel(null, "It's 100% 주님의 恩典", "", 10, 0, "[1]\r\na\r\n[chorus]\r\nb\r\n[2]\r\nc",
            Sequence: "1,c,2,c", Capo: -1));
        saved.Succeeded.Should().BeTrue();
        int id = saved.AffectedSongIds[0];
        Scalar<string>(_db, $"SELECT hex(CAST(SEQUENCE AS BLOB)) FROM SONG WHERE SONGID={id}").Should().Be("01000200");
        Scalar<string>(_db, $"SELECT TITLE_1 FROM SONG WHERE SONGID={id}").Should().Be("It's 100% 주님의 恩典");
        Scalar<long>(_db, $"SELECT CAPO FROM SONG WHERE SONGID={id}").Should().Be(-1);
        Scalar<long>(_db, $"SELECT SONG_NUMBER FROM SONG WHERE SONGID={id}").Should().Be(0);
        var detail = await sut.GetSongDetailAsync(_db, id);
        detail!.Sequence.Should().Be("\u0001\0\u0002\0");
        SongExchangeSequence.FromWpfDatabase(detail.Sequence).Should().Be("1,c,2,c");
    }

    [RealDatabasesFact]
    public async Task EveryRealSong_CopiedThroughTheRepository_KeepsItsColumns()
    {
        // Copy 300 real songs (every 25th) as SongCopyViewModel builds the write model, compare the stored columns.
        var sut = new AdminDatabaseRepository();
        var all = await sut.GetSongsAsync(_db);
        var sources = all.Where((_, index) => index % 25 == 0).ToArray();
        var copies = new List<SongWriteModel>();
        foreach (var source in sources)
        {
            var d = (await sut.GetSongDetailAsync(_db, source.SongId))!;
            copies.Add(new SongWriteModel(null, d.Title, d.AlternateTitle, 13, d.SongNumber, d.Lyrics, d.Sequence, d.Writer, d.Copyright, d.Capo,
                d.Timing, d.Key, d.Notations, d.Category, d.LicenceAdmin1, d.LicenceAdmin2, d.BookReference, d.UserReference, d.Settings, d.FormatData,
                SequenceIsStored: true));
        }
        var report = await sut.SaveSongsAsync(_db, _backups, copies);
        report.Succeeded.Should().BeTrue();
        // The stroke key too: the copy has the title of its source, so it keeps the key WinForms sorts by.
        const string columns = "TITLE_1,TITLE_2,WRITER,COPYRIGHT,LYRICS,hex(CAST(SEQUENCE AS BLOB)),KEY,CATEGORY,MSC,CAPO,TIMING,SONG_NUMBER,LICENCE_ADMIN1,LICENCE_ADMIN2,BOOK_REFERENCE,USER_REFERENCE,SETTINGS,FORMATDATA";
        var differences = new List<string>();
        using var c = new SQLiteConnection(new SQLiteConnectionStringBuilder { DataSource = _db, ReadOnly = true }.ToString());
        c.Open();
        string[] Row(int id)
        {
            using var cmd = new SQLiteCommand($"SELECT {columns} FROM SONG WHERE SONGID={id}", c);
            using var r = cmd.ExecuteReader();
            r.Read();
            return Enumerable.Range(0, r.FieldCount).Select(i => r.IsDBNull(i) ? "" : Convert.ToString(r.GetValue(i))!).ToArray();
        }
        string[] names = columns.Split(',');
        for (int i = 0; i < sources.Length; i++)
        {
            var a = Row(sources[i].SongId);
            var b = Row(report.AffectedSongIds[i]);
            for (int k = 0; k < a.Length; k++)
                if (a[k] != b[k]) differences.Add($"{sources[i].SongId} {names[k]}: [{(a[k].Length > 40 ? a[k][..40] : a[k])}] -> [{(b[k].Length > 40 ? b[k][..40] : b[k])}]");
        }
        var byColumn = differences.GroupBy(d => d.Split(' ')[1]).Select(g => $"{g.Key}={g.Count()} e.g. {g.First()}");
        differences.Should().BeEmpty($"{differences.Count} column differences in {sources.Length} copies: {string.Join(" ;; ", byColumn)}");
    }

    [RealDatabasesFact]
    public async Task Backup_JustMade_IsKept_WhenOlderBackupsCarryLaterTimeStamps()
    {
        // The PC clock was set back (dead CMOS battery, wrong date fixed): the 20 backups on disk are "newer".
        Directory.CreateDirectory(_backups);
        for (int index = 0; index < AdminDatabaseRepository.BackupsKept; index++)
            File.WriteAllText(Path.Combine(_backups, $"EasiSlidesDb.20990101-000000-{index:000}.db"), "backup made before the clock went back");
        var sut = new AdminDatabaseRepository();

        var report = await sut.SaveFolderAsync(_db, _backups, new SongFolderWriteModel(13, "Renamed", true));

        report.Succeeded.Should().BeTrue();
        File.Exists(report.BackupPath).Should().BeTrue("the write reports this backup, and it is the only copy of the database before the write");
    }

    [RealDatabasesFact]
    public async Task UsageRow_AddedToTheRealEsUsageDb_CanBeDeleted()
    {
        // The real USAGE table declares REC_ID INTEGER without a key, so an INSERT that leaves it out stores NULL.
        var sut = new SearchUsageService(new AdminDatabaseRepository());
        long before = Scalar<long>(_usage, "SELECT COUNT(*) FROM USAGE");
        var day = new DateTime(2026, 11, 22);
        (await sut.AddUsageRecordsAsync(new UsageAddRequest(_usage, [new UsageAddRecord(day, "주일예배", "새 곡", 0, 1, "", "")]))).Succeeded.Should().BeTrue();
        var listed = await sut.GetUsageAsync(new UsageRequest(_usage, day, day, ""));
        listed.Records.Should().ContainSingle();

        var deleted = await sut.DeleteUsageRecordsAsync(_usage, [listed.Records[0].RecordId]);

        deleted.Succeeded.Should().BeTrue();
        Scalar<long>(_usage, "SELECT COUNT(*) FROM USAGE WHERE substr(WORSHIP_DATE,1,10)='2026-11-22'").Should().Be(0,
            $"the operator deleted the row (RecordId read back = {listed.Records[0].RecordId})");
        Scalar<long>(_usage, "SELECT COUNT(*) FROM USAGE").Should().Be(before, "no other row is deleted with it");
    }

    [RealDatabasesFact]
    public async Task Recover_LeavesASongThatIsNotDeleted_WhereItIs()
    {
        // WinForms recovers with "WHERE SongID=@id AND FolderNo=0". The recovery dialog's list can be stale:
        // the other app recovered the song and filed it elsewhere meanwhile.
        var sut = new AdminDatabaseRepository();
        long id = Scalar<long>(_db, "SELECT SONGID FROM SONG WHERE FOLDERNO = 10 LIMIT 1");

        var report = await sut.RecoverSongsAsync(_db, _backups, [new SongRecoveryRequest((int)id, 1)]);

        report.AffectedSongIds.Should().BeEmpty("the caller reports the song as skipped");
        Scalar<long>(_db, $"SELECT FOLDERNO FROM SONG WHERE SONGID={id}").Should().Be(10, "the song was not in the deleted folder");
    }

    [RealDatabasesFact]
    public async Task EveryStoredSequence_IsReadAsBefore_AndWrittenBackUnchanged()
    {
        // The reader of the build before the review. Every sequence the library holds must still read the same.
        static string Before(string value)
        {
            foreach (char c in value)
                if (char.IsControl(c) && !char.IsWhiteSpace(c)) return SongExchangeSequence.FromLegacy(value);
            if (value.Length > 0 && value != "p" && value.AsSpan().IndexOfAnyExcept("defgop") < 0) return SongExchangeSequence.FromLegacy(value);
            return value;
        }
        var stored = (await new AdminDatabaseRepository().GetSongsAsync(_db))
            .Select(song => song.Sequence ?? "").Where(sequence => sequence.Length > 0).Distinct().ToArray();
        stored.Should().NotBeEmpty();

        foreach (string sequence in stored)
        {
            string hex = Convert.ToHexString(System.Text.Encoding.Unicode.GetBytes(sequence));
            SongExchangeSequence.FromWpfDatabase(sequence).Should().Be(Before(sequence), $"stored sequence {hex}");
            SongExchangeSequence.ToDatabase(sequence).Should().Be(sequence, $"stored sequence {hex} is verse codes already");
        }
    }

    [RealDatabasesFact]
    public async Task Move1001Songs_ThenDeleteSongsOfSeveralFolders_KeepsEachOldFolder()
    {
        var sut = new AdminDatabaseRepository();
        var english = await sut.GetSongsAsync(_db, 9);
        english.Count.Should().Be(1001);
        foreach (var song in english) (await sut.GetSongDetailAsync(_db, song.SongId)).Should().NotBeNull();
        var current = await sut.GetMatchingSongsAsync(_db, english.Select(song => song.SongId).ToHashSet(), new HashSet<string>());
        current.Count.Should().Be(1001);
        var moved = await sut.MoveSongsAsync(_db, _backups, current.Select(song => new SongMoveRequest(song.SongId, song.FolderNo, 10)).ToArray());
        moved.Succeeded.Should().BeTrue();
        Scalar<long>(_db, "SELECT COUNT(*) FROM SONG WHERE FOLDERNO = 9").Should().Be(0);
        Scalar<long>(_db, $"SELECT COUNT(*) FROM SONG WHERE FOLDERNO = 10 AND OldFolder = 9 AND SONGID IN ({string.Join(",", english.Select(song => song.SongId))})").Should().Be(1001);

        var mixed = (await sut.GetSongsAsync(_db, 2)).Take(3).Concat((await sut.GetSongsAsync(_db, 11)).Take(2)).ToArray();
        var deleted = await sut.SoftDeleteSongsAsync(_db, _backups, mixed.Select(song => new SongDeleteRequest(song.SongId, song.FolderNo)).ToArray());
        deleted.Succeeded.Should().BeTrue();
        string ids = string.Join(",", mixed.Select(song => song.SongId));
        Scalar<long>(_db, $"SELECT COUNT(*) FROM SONG WHERE SONGID IN ({ids}) AND FOLDERNO = 0 AND OldFolder = 2").Should().Be(3);
        Scalar<long>(_db, $"SELECT COUNT(*) FROM SONG WHERE SONGID IN ({ids}) AND FOLDERNO = 0 AND OldFolder = 11").Should().Be(2);
        // WinForms writes LastModified as yyyy-MM-dd (DataUtil.ToStoredDate).
        Scalar<string>(_db, $"SELECT CAST(LastModified AS TEXT) FROM SONG WHERE SONGID = {mixed[0].SongId}")
            .Should().Be(DateTime.Now.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture));
    }

    [RealDatabasesFact]
    public void RealExtensionLists_Utf16WithBom_AreRead()
    {
        string work = Path.Combine(_root, "work");
        Directory.CreateDirectory(Path.Combine(work, "Admin", "Database"));
        foreach (var name in new[] { "AudioExtensions.txt", "VideoExtensions.txt" })
            File.Copy(Path.Combine(Real, name), Path.Combine(work, "Admin", "Database", name));
        SourceFileExtensions.Audio(work).Should().HaveCount(23).And.Contain(".opus").And.Contain(".aac");
        SourceFileExtensions.Video(work).Should().HaveCount(32).And.Contain(".3gp").And.Contain(".dvr-ms");
    }

    [RealDatabasesFact]
    public async Task ReorderFolders_TakesTheDeletedSongsOfAFolderAlong()
    {
        // The deleted songs of folder 1 (95 in the library of the review). The operator drags that folder one place down.
        var sut = new AdminDatabaseRepository();
        long deletedFromOne = Scalar<long>(_db, "SELECT COUNT(*) FROM SONG WHERE FOLDERNO = 0 AND OldFolder = 1");
        long deletedFromTwo = Scalar<long>(_db, "SELECT COUNT(*) FROM SONG WHERE FOLDERNO = 0 AND OldFolder = 2");
        string nameOfOne = Scalar<string>(_db, "SELECT Name FROM FOLDER WHERE FolderNo = 1");
        deletedFromOne.Should().BeGreaterThan(0);
        (await sut.ReorderFoldersAsync(_db, _backups, [new FolderOrderRequest(1, 2), new FolderOrderRequest(2, 1)])).Succeeded.Should().BeTrue();
        Scalar<string>(_db, "SELECT Name FROM FOLDER WHERE FolderNo = 2").Should().Be(nameOfOne);

        var deletedSongs = await sut.GetDeletedSongsAsync(_db);

        deletedSongs.Count(song => song.OriginalFolderName == nameOfOne).Should().Be((int)deletedFromOne, "recovery files a song in its OriginalFolderNo");
        Scalar<long>(_db, "SELECT COUNT(*) FROM SONG WHERE FOLDERNO = 0 AND OldFolder = 1").Should().Be(deletedFromTwo);
    }

    [RealDatabasesFact]
    public async Task SortKeysWrittenForRealTitles_AreWhatWinFormsWritesAndSortsBy()
    {
        // FrmMain orders its song list by the stored cjk_wordcount / cjk_strokecount. DataUtil.CJK_WordCount:
        static int WinFormsWordCount(string title)
        {
            if (title == "" || (title[0] > 0 && title[0] < 128)) return 0;
            int count = title.IndexOf('(') - 1;
            if (count < 0) count = title.Length;
            int beforeSpace = title.IndexOf(' ') - 1;
            if (beforeSpace > 0 && beforeSpace < count) count = beforeSpace;
            return count;
        }
        var sut = new AdminDatabaseRepository();
        var live = (await sut.GetSongsAsync(_db)).Where(song => song.FolderNo > 0).ToArray();
        var report = await sut.SaveSongsAsync(_db, _backups, live.Select(song => new SongWriteModel(null, song.Title, "", 13, 0, "x")).ToArray());
        report.Succeeded.Should().BeTrue();
        var wrongWordCount = new List<string>();
        int strokeKeyLost = 0;
        using var c = new SQLiteConnection(new SQLiteConnectionStringBuilder { DataSource = _db, ReadOnly = true }.ToString());
        c.Open();
        for (int i = 0; i < live.Length; i++)
        {
            using var cmd = new SQLiteCommand($"SELECT CJK_WORDCOUNT, CJK_STROKECOUNT FROM SONG WHERE SONGID={report.AffectedSongIds[i]}", c);
            using var r = cmd.ExecuteReader();
            r.Read();
            if (Convert.ToInt32(r[0]) != WinFormsWordCount(live[i].Title)) wrongWordCount.Add($"{live[i].Title}: {r[0]} (WinForms {WinFormsWordCount(live[i].Title)})");
            // The source row has a stroke key (the title starts with a character WinForms has a stroke count for); the copy must not have "000" + title.
            if (live[i].StrokeSortKey is { Length: > 0 } key && !key.StartsWith("000") && Convert.ToString(r[1])!.StartsWith("000")) strokeKeyLost++;
        }
        (wrongWordCount.Count + strokeKeyLost).Should().Be(0,
            $"of {live.Length} live titles, {wrongWordCount.Count} get another CJK_WORDCOUNT than WinForms writes (e.g. {string.Join(" | ", wrongWordCount.Take(4))}) " +
            $"and {strokeKeyLost} lose their stroke sort key");
    }

    [RealDatabasesFact]
    public async Task StrokeKeysOfRealTitles_AreRebuiltFromTheStrokeCountsTheLibraryHolds()
    {
        // Titles whose stored key matches them character by character (not those the conversion spoiled with "?"),
        // saved under a new title that no stored key can be carried for: the same characters in another song.
        // The key is built from the stroke counts read out of the stored keys, as DataUtil.CJK_StrokeCount builds it.
        var sut = new AdminDatabaseRepository();
        var sound = (await sut.GetSongsAsync(_db))
            .Where(song => song.FolderNo > 0 && song.StrokeSortKey is { Length: >= 7 } key && !key.StartsWith("000")
                && song.Title.Length is > 0 and <= 6 && key.Length == song.Title.Length * 7
                && Enumerable.Range(0, song.Title.Length).All(i => key.Substring(i * 7 + 2, 5) == ((int)song.Title[i]).ToString("00000")))
            .DistinctBy(song => song.Title).Take(400).ToArray();
        sound.Should().NotBeEmpty();
        // The title twice: no song has that title, so nothing is carried; the first six characters decide the key.
        var report = await sut.SaveSongsAsync(_db, _backups, sound.Select(song => new SongWriteModel(null, song.Title + song.Title, "", 13, 0, "x")).ToArray());
        report.Succeeded.Should().BeTrue();

        for (int i = 0; i < sound.Length; i++)
        {
            string doubled = sound[i].Title + sound[i].Title;
            string expected = string.Concat((sound[i].StrokeSortKey + sound[i].StrokeSortKey).Take(42)) + (doubled.Length > 6 ? doubled[6..] : "");
            Scalar<string>(_db, $"SELECT CJK_STROKECOUNT FROM SONG WHERE SONGID={report.AffectedSongIds[i]}").Should().Be(expected, sound[i].Title);
        }
    }
}
