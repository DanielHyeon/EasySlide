using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Axicultus.Wpf.Captions;
using Xunit;

namespace Axicultus.Wpf.Tests.Shell;

public sealed partial class CaptionConsoleTests
{
    // Songs as the song database holds them: verse tags, blank lines, chords after the marker.
    private static readonly SongLyrics.Song AcceptMyHeart = new("나의맘받으소서", "",
        "[1]\n나의 맘 받으소서»G D\n오셔서 주님의 처소 삼으소서Â»Em\n\n[2]\n주님의 성전 되게 하소서\n[chorus]\n내맘에 거하여 주옵소서");
    private static readonly SongLyrics.Song MyGodAccept = new("나의하나님받으소서", "", "[1]\n나의 하나님 받으소서\n주께 드리는 예물을");

    [Fact]
    public void LettersMisreadFromAPictureArePutRightFromTheSongDatabase()
    {
        var corrected = SongLyrics.Correct(@"C:\decks\나의 맘 받으소서 오셔서 주님의 처소 삼으소서.pptx",
            [["나의 맘 받으소서\n오셔서 주님의 저소 삽으소서"], ["내맘에 기하여 주음소서"]], [MyGodAccept, AcceptMyHeart]);
        Assert.Equal([["나의 맘 받으소서\n오셔서 주님의 처소 삼으소서"], ["내맘에 거하여 주옵소서"]], corrected);
    }

    [Fact]
    public void LyricsOfASongNotInTheDatabaseStayAsRead()
    {
        string[][] read = [["하늘 위에 빛나는 별", "바다 위에 부는 바람"], ["산과 들에 피는 꽃"]];
        Assert.Equal(read, SongLyrics.Correct(@"C:\decks\글자.pptx", read, [MyGodAccept, AcceptMyHeart]));
    }

    [Fact]
    public void ALineLikeNoPartOfTheSongStaysAsReadAndTheOthersAreCorrected()
    {
        var corrected = SongLyrics.Correct(@"C:\decks\글자.pptx",
            [["나의 맘 받으소서", "오셔서 주님의 저소 삽으소서"], ["내맘에 기하여 주음소서", "사진 속 간판 글씨"]], [AcceptMyHeart]);
        Assert.Equal([["나의 맘 받으소서", "오셔서 주님의 처소 삼으소서"], ["내맘에 거하여 주옵소서", "사진 속 간판 글씨"]], corrected);
    }

    [Fact]
    public void OfTwoSongsAlikeTheOneMostLinesAreLikeIsTaken()
    {
        var trust = new SongLyrics.Song("세상흔들리고", "", "주님의 사랑은 영원히 변하지 않네\n나는 주를 신뢰해\n세상 흔들리고 사람들 변해도");
        var praise = new SongLyrics.Song("주님의사랑은", "", "주님의 사랑은 영원히 끝이 없도다\n나는 주를 찬양해\n하늘과 땅이 다 변해도");
        var corrected = SongLyrics.Correct(@"C:\decks\글자.pptx",
            [["주님의 사랑은 영원이 변하지 않네", "나는 주를 신뢰매"], ["세상 흔들리고 사람들 변해도"]], [praise, trust]);
        Assert.Equal([["주님의 사랑은 영원히 변하지 않네", "나는 주를 신뢰해"], ["세상 흔들리고 사람들 변해도"]], corrected);
    }

    [Fact]
    public void AChorusReadOnSeveralSlidesIsCorrectedOnEachWithoutAddingWords()
    {
        var corrected = SongLyrics.Correct(@"C:\decks\나의 맘 받으소서.pptx",
            [["내맘에 기하여 주옵소서"], ["내맘에 거하여 주음소서"], ["내 맘에 거하여 주옴소서"]], [AcceptMyHeart]);
        // (the third read "naemame" as two words: its words are not joined, only the misread one is put right)
        Assert.Equal([["내맘에 거하여 주옵소서"], ["내맘에 거하여 주옵소서"], ["내 맘에 거하여 주옵소서"]], corrected);
    }

    [Fact]
    public void AWordEachVerseChangesStaysAsReadAgainstTheOneVerseInTheDatabase()
    {
        var victory = new SongLyrics.Song("승리는내것일세", "",
            "[1]\n승리는 내 것일세\n승리는 내 것일세\n구세주의 보혈로써 승리는 내 것일세\n승리만은 내 것일세");
        string[][] read = [["소망은 내 것일세", "소망은 내 것일세"], ["구세주의 보혈로써 소망은 내 것일세", "구원만은 내 것일세"]];
        Assert.Equal(read, SongLyrics.Correct(@"C:\decks\승리는 내것일세.pptx", read, [victory]));
    }

    [Fact]
    public void AWordTheReadingMissedIsNotAddedAndTheWordsReadStay()
    {
        var look = new SongLyrics.Song("괴로울때주님의얼굴보라", "", "괴로울 때 주님의 얼굴 보라\n눈을 들어 주를 보라\n평화의 주님 바라보아라");
        var corrected = SongLyrics.Correct(@"C:\decks\괴로울 때 주님의 얼굴 보라.pptx",
            [["괴로울 때 주님의 얼골 보라", "눈을 주를 보라"], ["평화의 주님 바라보아라"]], [look]);
        Assert.Equal([["괴로울 때 주님의 얼굴 보라", "눈을 주를 보라"], ["평화의 주님 바라보아라"]], corrected);
    }

    [Fact]
    public void AReadWordThatIsAWordOfTheSongStaysEvenWhereTheSongHasAnother()
    {
        // "yesu" lines up with "yesuneun" here, but the song sings "yesu" too: it is read right.
        var christ = new SongLyrics.Song("예수는그리스도", "", "예수는 그리스도\n예수 이름 높이세\n예수는 나의 주");
        var corrected = SongLyrics.Correct(@"C:\decks\예수는 그리스도.pptx",
            [["예수 그리스도", "예수 이름 높이세"], ["예수는 나외 주"]], [christ]);
        Assert.Equal([["예수 그리스도", "예수 이름 높이세"], ["예수는 나의 주"]], corrected);
    }

    [Fact]
    public void ChordsInTheDatabaseNeverReachTheLyrics()
    {
        Assert.DoesNotContain("»", AcceptMyHeart.Words);
        Assert.DoesNotContain("Em", AcceptMyHeart.Words);
        Assert.DoesNotContain("G D", AcceptMyHeart.Words);
        var corrected = SongLyrics.Correct(@"C:\decks\글자.pptx",
            [["나의 맘 받으소서", "오셔서 주님의 저소 삽으소서", "내맘에 기하여 주음소서"]], [AcceptMyHeart]);
        Assert.Equal([["나의 맘 받으소서", "오셔서 주님의 처소 삼으소서", "내맘에 거하여 주옵소서"]], corrected);
    }

    [Fact]
    public void TextThatIsNotHangulStaysWhereItWasRead()
    {
        var corrected = SongLyrics.Correct(@"C:\decks\나의 맘 받으소서.pptx",
            [["1. 나의 맘 받으소서 (2x)", "오셔서 주님의 저소 삽으소서!", "내맘에 기하여 주음소서 Amen"]], [AcceptMyHeart]);
        Assert.Equal([["1. 나의 맘 받으소서 (2x)", "오셔서 주님의 처소 삼으소서!", "내맘에 거하여 주옵소서 Amen"]], corrected);
    }

    [Fact]
    public void AFewShortLinesLikeAnUnrelatedSongDoNotMakeItTheDecksSong()
    {
        // One line, like a part of a song whose title is nothing like the deck's name: too little to be sure.
        var other = new SongLyrics.Song("예수전하세", "", "[3]\n길이요 진리요\n생명되신 예수님\n그 이름 믿으면 생명을 얻었네");
        string[][] read = [["생명을 얻네"]];
        Assert.Equal(read, SongLyrics.Correct(@"C:\decks\아름다운 그 이름.pptx", read, [other]));
        Assert.Null(SongLyrics.Find(@"C:\decks\아름다운 그 이름.pptx", read, [other]));
    }

    [Fact]
    public void AShortTitleInTheDecksNameDoesNotMakeTheSongSure()
    {
        // Two short lines like the song, and its two-syllable title inside the deck's name: still not sure.
        var praise = new SongLyrics.Song("찬양", "", "주를 찬양해\n주님을 높여\n온 땅이 노래해");
        string[][] read = [["주를 찬양혜", "주님을 놉여"]];
        Assert.Null(SongLyrics.Find(@"C:\decks\주일예배 찬양.pptx", read, [praise]));
        Assert.Equal(read, SongLyrics.Correct(@"C:\decks\주일예배 찬양.pptx", read, [praise]));
    }

    [Fact]
    public Task CorrectedLyricsFillTheRecognizedBoxAndTheOperatorsSavedTextStillWins() => WithConsole(async (console, session, directory) =>
    {
        static OcrLine At(int center, string text) => new([new OcrWord(text, 100, center - 10, 200, 20)]);
        var path = CaptionAllTextTests.WriteDeck(directory.FullName, [CaptionScorePictureTests.ScorePng()],
            CaptionAllTextTests.ScoreSlide(0), CaptionAllTextTests.ScoreSlide(0));
        var reading = new TaskCompletionSource();
        console.ReadLyrics = async (files, _) =>
        {
            await reading.Task;
            return [.. files.Select(file => Path.GetFileNameWithoutExtension(file) == "1"
                ? new OcrPage(file, null, [At(177, "나의 맘 받으소서"), At(357, "오셔서 주님의 저소 삽으소서")])
                : new OcrPage(file, null, [At(177, "주님의 성전 되게 하소서"), At(357, "내맘에 기하여 주음소서")]))];
        };
        console.Songs = _ => Task.FromResult<IReadOnlyList<SongLyrics.Song>>([MyGodAccept, AcceptMyHeart]);
        Assert.True(await console.PrepareDeckAsync(path, 1));
        await console.ScorePreparation;
        session.Overrides[1] = "운영자가 고친 문구"; // saved by the operator while the lyrics are being read
        reading.SetResult();
        await console.LyricsReading;
        string Recognized(int slide) => session.Deck!.Slides[slide - 1].Boxes.Single(box => box.Key == CaptionSession.RecognizedKey).Text;
        Assert.Equal("나의 맘 받으소서\n오셔서 주님의 처소 삼으소서", Recognized(1));
        Assert.Equal("주님의 성전 되게 하소서\n내맘에 거하여 주옵소서", Recognized(2));
        Assert.Equal("운영자가 고친 문구", session.TextFor(1, CaptionSession.RecognizedKey));
        Assert.Equal("주님의 성전 되게 하소서\n내맘에 거하여 주옵소서", session.TextFor(2, CaptionSession.RecognizedKey));
    });

    [Fact]
    public Task ASongDatabaseThatCannotBeReadLeavesTheLyricsAsRead() => WithConsole(async (console, session, directory) =>
    {
        static OcrLine At(int center, string text) => new([new OcrWord(text, 100, center - 10, 200, 20)]);
        console.ReadLyrics = (files, _) => Task.FromResult<OcrPage[]>([.. files.Select(file =>
            new OcrPage(file, null, [At(177, "나의 맘 받으소서"), At(357, "오셔서 주님의 저소 삽으소서")]))]);
        console.Songs = _ => Task.FromException<IReadOnlyList<SongLyrics.Song>>(new InvalidDataException("broken database"));
        var path = CaptionAllTextTests.WriteDeck(directory.FullName, [CaptionScorePictureTests.ScorePng()], CaptionAllTextTests.ScoreSlide(0));
        Assert.True(await console.PrepareDeckAsync(path, 1));
        await console.ScorePreparation;
        await console.LyricsReading;
        Assert.Equal("나의 맘 받으소서\n오셔서 주님의 저소 삽으소서",
            session.Deck!.Slides[0].Boxes.Single(box => box.Key == CaptionSession.RecognizedKey).Text);
    });

    [Fact]
    public void ShortCommonLinesAreNoEvidenceForASongNotInTheDatabase()
    {
        // "hallelujah" and "amen" are in many songs: a deck of a song the database lacks is not taken for one of them.
        var long_ = new SongLyrics.Song("할렐루야주를찬양", "",
            "[1]\n할렐루야 할렐루야\n주님을 찬양하라 아멘\n[2]\n온 땅이여 찬양하라\n할렐루야 아멘\n[3]\n영원히 찬양하라\n할렐루야 아멘 아멘");
        string[][] read = [["할렐루야", "아멘", "하늘 위에 빛나는 별"], ["할렐루야", "아멘", "바다 위에 부는 바람"], ["할렐루야", "아멘", "산과 들에 피는 꽃"]];
        Assert.Null(SongLyrics.Find(@"C:\decks\글자.pptx", read, [long_]));
        Assert.Equal(read, SongLyrics.Correct(@"C:\decks\글자.pptx", read, [long_]));
    }

    // A song database as the repository tests make theirs (the legacy schema), with folder 1 and the given songs.
    private static void WriteSongDatabase(string path, params (string Title, string Lyrics)[] songs)
    {
        if (File.Exists(path)) File.Delete(path);
        using var connection = new System.Data.SQLite.SQLiteConnection($"Data Source={path};Version=3;");
        connection.Open();
        void Run(string sql, params (string Name, object Value)[] values)
        {
            using var command = new System.Data.SQLite.SQLiteCommand(sql, connection);
            foreach (var (name, value) in values) command.Parameters.AddWithValue(name, value);
            command.ExecuteNonQuery();
        }
        Run("PRAGMA user_version = 4;");
        Run("CREATE TABLE FOLDER (FolderNo INTEGER PRIMARY KEY, Name TEXT, Use TEXT);");
        Run("""
            CREATE TABLE SONG (SONGID INTEGER PRIMARY KEY, TITLE_1 TEXT, TITLE_2 TEXT, WRITER TEXT, COPYRIGHT TEXT,
                CJK_WordCount TEXT, CJK_StrokeCount TEXT, LYRICS TEXT, SEQUENCE TEXT, KEY TEXT, CAPO INTEGER, TIMING TEXT,
                MSC TEXT, CATEGORY TEXT, FOLDERNO INTEGER, SONG_NUMBER INTEGER, LICENCE_ADMIN1 TEXT, LICENCE_ADMIN2 TEXT,
                BOOK_REFERENCE TEXT, USER_REFERENCE TEXT, SETTINGS TEXT, FORMATDATA TEXT, LastModified TEXT,
                OldFolder INTEGER DEFAULT 0);
            """);
        Run("CREATE TABLE LICENCE (ADMINISTRATOR TEXT, REF TEXT);");
        Run("INSERT INTO FOLDER (FolderNo, Name, Use) VALUES (1, 'Songs', 'True');");
        int id = 0;
        foreach (var (title, lyrics) in songs)
            Run("INSERT INTO SONG (SONGID, TITLE_1, TITLE_2, LYRICS, FOLDERNO, SONG_NUMBER) VALUES (@id, @title, '', @lyrics, 1, @id);",
                ("@id", ++id), ("@title", title), ("@lyrics", lyrics));
    }

    [Fact]
    public Task TheSongDatabaseIsReadOnceRetoldNeverAndReadAgainWhenItChanges() => WithConsole(async (console, session, directory) =>
    {
        static OcrLine At(int center, string text) => new([new OcrWord(text, 100, center - 10, 200, 20)]);
        var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
        var main = (Axicultus.Wpf.Shell.MainViewModel)typeof(CaptionConsole).GetField("_main", flags)!.GetValue(console)!;
        var cache = typeof(CaptionConsole).GetField("_songs", flags)!;
        var database = Path.Combine(directory.FullName, "AxicultusDb.db");
        const string lyrics = "[1]\n나의 맘 받으소서\n오셔서 주님의 처소 삼으소서\n[chorus]\n내맘에 거하여 주옵소서";
        WriteSongDatabase(database, ("나의맘받으소서", lyrics), ("나의하나님받으소서", "나의 하나님 받으소서\n주께 드리는 예물을"));
        main.Library.DatabasePath = database;
        console.Songs = null; // the console's own reading of the song database
        console.ReadLyrics = (files, _) => Task.FromResult<OcrPage[]>([.. files.Select(file => Path.GetFileNameWithoutExtension(file) == "1"
            ? new OcrPage(file, null, [At(177, "오셔서 주님의 저소 삽으소서"), At(357, "내맘에 기하여 주음소서")])
            : new OcrPage(file, null, [At(177, "나의 맘 받으소서"), At(357, "오셔서 주님의 처소 삼으소서")]))]);
        int deckNo = 0;
        async Task<string> ReadDeckAsync()
        {
            var path = CaptionAllTextTests.WriteDeck(directory.CreateSubdirectory($"deck{++deckNo}").FullName,
                [CaptionScorePictureTests.ScorePng()], CaptionAllTextTests.ScoreSlide(0), CaptionAllTextTests.ScoreSlide(0));
            Assert.True(await console.PrepareDeckAsync(path, 1));
            await console.ScorePreparation;
            await console.LyricsReading;
            return session.Deck!.Slides[0].Boxes.Single(box => box.Key == CaptionSession.RecognizedKey).Text;
        }
        int Told() => directory.GetFiles("errors-*.log").Sum(file => File.ReadAllText(file.FullName).Split("correct lyrics from the song database").Length - 1);
        const string corrected = "오셔서 주님의 처소 삼으소서\n내맘에 거하여 주옵소서", read = "오셔서 주님의 저소 삽으소서\n내맘에 기하여 주음소서";

        Assert.Equal(corrected, await ReadDeckAsync()); // read from the database
        var first = cache.GetValue(console);
        Assert.Equal(corrected, await ReadDeckAsync());
        Assert.Same(((ValueTuple<string, Task<IReadOnlyList<SongLyrics.Song>>>)first!).Item2,
            ((ValueTuple<string, Task<IReadOnlyList<SongLyrics.Song>>>)cache.GetValue(console)!).Item2); // not read again

        File.WriteAllText(database, "not a song database"); // broken: told once, the lyrics as read
        File.SetLastWriteTimeUtc(database, DateTime.UtcNow.AddMinutes(1));
        Assert.Equal(read, await ReadDeckAsync());
        Assert.Equal(read, await ReadDeckAsync());
        Assert.Equal(1, Told());

        WriteSongDatabase(database, ("나의맘받으소서", lyrics)); // mended: read again
        File.SetLastWriteTimeUtc(database, DateTime.UtcNow.AddMinutes(2));
        Assert.Equal(corrected, await ReadDeckAsync());
        Assert.Equal(1, Told());
    });
}
