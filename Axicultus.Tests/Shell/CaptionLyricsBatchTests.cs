using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Axicultus.Wpf.Captions;
using Axicultus.Wpf.Shell;
using Xunit;

namespace Axicultus.Wpf.Tests.Shell;

/// <summary>
/// a048 design 1, 2: the lyrics in slide pictures are read in batches (pictures and command line both limited), each
/// batch with its own time limit; a failed batch leaves its pictures unread and the others' lyrics read. The OCR helper
/// is a stand-in here.
/// </summary>
public sealed class CaptionLyricsBatchTests
{
    public enum Failure { ExitCode, Timeout, BrokenOutput, NotStarted }

    private static readonly TimeSpan ShortTime = TimeSpan.FromMilliseconds(300);

    private static string[] Images(int count, int length = 60) =>
        [.. Enumerable.Range(1, count).Select(n => Path.Combine(@"C:\Temp\Axicultus\ocr", new string('a', Math.Max(1, length - 30)), $"{n}.png"))];

    private static string Json(IReadOnlyList<string> images) =>
        JsonSerializer.Serialize(images.Select(image => new OcrPage(image, null, [new OcrLine([new OcrWord("주 사랑", 10, 10, 40, 20)])])).ToArray());

    // The helper as it is asked: each run's pictures kept; the run numbered `failing` fails as told.
    private static LyricsReader.HelperRun Helper(List<string[]> runs, int failing = -1, Failure failure = Failure.ExitCode, int noKorean = -1) =>
        async (images, token) =>
        {
            runs.Add([.. images]);
            int run = runs.Count - 1;
            if (run == noKorean) return (3, "", "no ko");
            if (run != failing) return (0, Json(images), "");
            switch (failure)
            {
                case Failure.ExitCode: return (1, "", "helper broke");
                case Failure.BrokenOutput: return (0, "[{\"File\":", "");
                case Failure.NotStarted: throw new Win32Exception(2);
                default:
                    await Task.Delay(Timeout.Infinite, token);
                    throw new InvalidOperationException("not reached");
            }
        };

    [Theory]
    [InlineData(60)]  // the picture count limits a batch
    [InlineData(700)] // the command line's length does
    public async Task HundredsOfPicturesAreReadInBatchesUnderTheCommandLineLimit(int pathLength)
    {
        var images = Images(400, pathLength);
        var runs = new List<string[]>();
        var pages = await LyricsReader.ReadAsync(images, Helper(runs), ShortTime, CancellationToken.None);
        Assert.True(runs.Count > 1, $"{runs.Count} run(s)");
        Assert.All(runs, batch =>
        {
            Assert.InRange(batch.Length, 1, LyricsReader.BatchPictures);
            Assert.InRange(batch.Sum(image => image.Length + 3), 0, LyricsReader.BatchCharacters); // (a space and two quotes each)
        });
        Assert.Equal(images, runs.SelectMany(batch => batch)); // every picture once, in order
        Assert.Equal(images, pages.Select(page => page.File));
        Assert.All(pages, page => Assert.Null(page.Error));
    }

    [Theory]
    [InlineData(1, Failure.Timeout)] // the spec's scenario: the second of three batches runs out of time
    [InlineData(0, Failure.ExitCode)]
    [InlineData(1, Failure.ExitCode)]
    [InlineData(1, Failure.BrokenOutput)]
    [InlineData(0, Failure.NotStarted)]
    [InlineData(2, Failure.Timeout)]
    public async Task AFailedBatchLeavesItsPicturesUnreadAndTheOtherBatchesRead(int failing, Failure failure)
    {
        var images = Images(3 * LyricsReader.BatchPictures - 5);
        var runs = new List<string[]>();
        var pages = await LyricsReader.ReadAsync(images, Helper(runs, failing, failure), ShortTime, CancellationToken.None);
        Assert.Equal(3, runs.Count);
        Assert.Equal(runs[failing], pages.Where(page => page.Error != null).Select(page => page.File));
        Assert.All(pages.Where(page => page.Error != null), page => Assert.Empty(page.Lines));
        Assert.Equal(images.Except(runs[failing]), pages.Where(page => page.Error == null).Select(page => page.File));
        Assert.All(pages.Where(page => page.Error == null), page => Assert.NotEmpty(page.Lines));
    }

    [Fact]
    public async Task WhenEveryBatchFailsTheReadingFailsAsBefore()
    {
        var images = Images(2 * LyricsReader.BatchPictures);
        var runs = new List<string[]>();
        LyricsReader.HelperRun broken = (batch, _) =>
        {
            runs.Add([.. batch]);
            return Task.FromResult((1, "", "helper broke"));
        };
        var error = await Assert.ThrowsAsync<IOException>(() => LyricsReader.ReadAsync(images, broken, ShortTime, CancellationToken.None));
        Assert.Equal(CaptionText.Format("CaptionConsole", "OcrError", 1, "helper broke"), error.Message);
        Assert.Equal(2, runs.Count); // every batch was tried
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public async Task NoKoreanOcrEndsTheReadingWithTheSameNotice(int run)
    {
        var images = Images(3 * LyricsReader.BatchPictures);
        var runs = new List<string[]>();
        var error = await Assert.ThrowsAsync<LyricsReader.NoKoreanOcrException>(() => LyricsReader.ReadAsync(images, Helper(runs, noKorean: run), ShortTime, CancellationToken.None));
        Assert.Equal(CaptionText.Format("CaptionConsole", "OcrNoKorean", "no ko"), error.Message);
        Assert.Equal(run + 1, runs.Count); // the batches after it are not run
    }

    // A helper that runs out of time on the runs numbered `hang` and reads the others.
    private static LyricsReader.HelperRun Hanging(List<string[]> runs, params int[] hang) => async (images, token) =>
    {
        runs.Add([.. images]);
        if (!hang.Contains(runs.Count - 1)) return (0, Json(images), "");
        await Task.Delay(Timeout.Infinite, token);
        return (0, "", "");
    };

    // a048 design 8: a helper that hangs on every batch is waited for twice, whatever the deck's size.
    [Fact]
    public async Task AHelperThatAlwaysHangsIsWaitedForTwoBatchesOnly()
    {
        var images = Images(10 * LyricsReader.BatchPictures);
        var runs = new List<string[]>();
        await Assert.ThrowsAsync<TimeoutException>(() =>
            LyricsReader.ReadAsync(images, Hanging(runs, [.. Enumerable.Range(0, 10)]), ShortTime, CancellationToken.None));
        Assert.Equal(2, runs.Count);
    }

    [Fact]
    public async Task TwoBatchesOutOfTimeInARowEndTheReadingAndLeaveTheRestUnread()
    {
        var images = Images(5 * LyricsReader.BatchPictures);
        var runs = new List<string[]>();
        var pages = await LyricsReader.ReadAsync(images, Hanging(runs, 1, 2), ShortTime, CancellationToken.None);
        Assert.Equal(3, runs.Count); // the batches after the second in a row are not run
        Assert.Equal(images, pages.Select(page => page.File)); // every picture told, read or not
        Assert.Equal(runs[0], pages.Where(page => page.Error == null).Select(page => page.File));
    }

    [Fact]
    public async Task BatchesOutOfTimeWithAReadOneBetweenDoNotEndTheReading()
    {
        var images = Images(4 * LyricsReader.BatchPictures);
        var runs = new List<string[]>();
        var pages = await LyricsReader.ReadAsync(images, Hanging(runs, 0, 2), ShortTime, CancellationToken.None);
        Assert.Equal(4, runs.Count);
        Assert.Equal(runs[1].Concat(runs[3]), pages.Where(page => page.Error == null).Select(page => page.File));
    }

    [Fact]
    public async Task CancellingEndsTheReadingAtOnce()
    {
        var images = Images(3 * LyricsReader.BatchPictures);
        var runs = new List<string[]>();
        using var cancel = new CancellationTokenSource();
        LyricsReader.HelperRun helper = async (batch, token) =>
        {
            runs.Add([.. batch]);
            if (runs.Count < 2) return (0, Json(batch), "");
            cancel.Cancel(); // the operator leaves the deck while the second batch is read
            await Task.Delay(Timeout.Infinite, token);
            return (0, "", "");
        };
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => LyricsReader.ReadAsync(images, helper, TimeSpan.FromMinutes(1), cancel.Token));
        Assert.Equal(2, runs.Count); // (not taken for a batch out of time: no third batch)
    }
}

public sealed partial class CaptionConsoleTests
{
    private static string Unread(int slides, int unread) => CaptionText.Format("CaptionConsole", "LyricsPartlyRead", slides, unread);

    // a048 design 3, 10 (the spec's scenario of the notice while a score row is on air): following the screen with a score row on air, the slides whose
    // lyrics could not be read are told, and stay told when the row is drawn again; the read slides' lyrics are used.
    // Another deck takes the notice down.
    [Fact]
    public Task SlidesWhoseLyricsCouldNotBeReadStayToldWhileAScoreRowIsOnAir() => WithConsole(async (console, session, directory) =>
    {
        var path = CaptionAllTextTests.WriteDeck(directory.CreateSubdirectory("score").FullName, [CaptionScorePictureTests.ScorePng()],
            CaptionAllTextTests.ScoreSlide(0), CaptionAllTextTests.ScoreSlide(0), CaptionAllTextTests.ScoreSlide(0));
        var reading = new TaskCompletionSource();
        console.ReadLyrics = async (files, _) =>
        {
            await reading.Task;
            return [.. ReadLyrics(files).Select(page =>
                Path.GetFileNameWithoutExtension(page.File) == "2" ? new OcrPage(page.File, "OCR helper error (1)", []) : page)];
        };
        Assert.True(await console.PrepareDeckAsync(path, 1));
        await console.ScorePreparation;
        await ClickAsync(console.AirButton);
        console.ScreenMode.IsChecked = true;
        console.ScoreOnly.IsChecked = true;
        session.Follow(1, CaptionDeck.AllTextKey);
        reading.SetResult();
        await console.LyricsReading;
        Assert.Contains("1장 첫 줄", session.TextFor(1, CaptionSession.RecognizedKey));
        Assert.Equal("", session.TextFor(2, CaptionSession.RecognizedKey));
        Assert.Contains("3장 첫 줄", session.TextFor(3, CaptionSession.RecognizedKey));
        Assert.NotNull(session.PublicImage); // a score row on air
        typeof(CaptionConsole).GetMethod("UpdateScore", Private)!.Invoke(console, null); // the row drawn again
        await Pump(console, 200);
        Assert.Contains(Unread(3, 1), console.DeckLabel.Text);

        console.ReadLyrics = (files, _) => Task.FromResult(ReadLyrics(files));
        var other = CaptionAllTextTests.WriteDeck(directory.CreateSubdirectory("other").FullName, [CaptionScorePictureTests.ScorePng()],
            CaptionAllTextTests.ScoreSlide(0));
        Assert.True(await console.PrepareDeckAsync(other, 1));
        Assert.DoesNotContain(Unread(3, 1), console.DeckLabel.Text);
        await console.ScorePreparation;
        await console.LyricsReading;
        Assert.DoesNotContain(Unread(3, 1), console.DeckLabel.Text);
    });

    // a048 design 14 (the spec's scenario of no slide read): when no slide could be read (every batch out of time), the deck's
    // label tells all of them unread, and that stays when the score row on air is drawn again.
    [Fact]
    public Task WhenNoSlideCouldBeReadAllAreToldUnreadWhileAScoreRowIsOnAir() => WithConsole(async (console, session, directory) =>
    {
        var path = CaptionAllTextTests.WriteDeck(directory.FullName, [CaptionScorePictureTests.ScorePng()],
            CaptionAllTextTests.ScoreSlide(0), CaptionAllTextTests.ScoreSlide(0), CaptionAllTextTests.ScoreSlide(0));
        var reading = new TaskCompletionSource();
        console.ReadLyrics = async (files, _) => { await reading.Task; throw new TimeoutException(); };
        Assert.True(await console.PrepareDeckAsync(path, 1));
        await console.ScorePreparation;
        await ClickAsync(console.AirButton);
        console.ScreenMode.IsChecked = true;
        console.ScoreOnly.IsChecked = true;
        session.Follow(1, CaptionDeck.AllTextKey);
        reading.SetResult();
        await console.LyricsReading;
        Assert.StartsWith(CaptionText.Format("CaptionConsole", "LyricsReadFailed", ""), console.Status.Text); // the detail, as before
        Assert.NotNull(session.PublicImage); // a score row on air
        typeof(CaptionConsole).GetMethod("UpdateScore", Private)!.Invoke(console, null); // the row drawn again
        await Pump(console, 200);
        Assert.Equal(Path.GetFileName(path) + " · " + Unread(3, 3), console.DeckLabel.Text);
    });

    // a048 design 16: a reading that fails before it starts (its picture folder cannot be made) ends as a read failure,
    // and the next deck opens and is shown.
    [Fact]
    public Task AReadingThatCannotStartLeavesTheNextDeckToOpen() => WithConsole(async (console, session, directory) =>
    {
        var blocker = Path.Combine(directory.FullName, "blocker");
        File.WriteAllText(blocker, ""); // a file: no folder can be made under it
        console.OcrFolder = Path.Combine(blocker, "ocr");
        console.ReadLyrics = (files, _) => Task.FromResult(ReadLyrics(files));
        var first = CaptionAllTextTests.WriteDeck(directory.CreateSubdirectory("first").FullName, [CaptionScorePictureTests.ScorePng()],
            CaptionAllTextTests.ScoreSlide(0), CaptionAllTextTests.ScoreSlide(0));
        Assert.True(await console.PrepareDeckAsync(first, 1));
        var failure = await Record.ExceptionAsync(() => console.LyricsReading);
        var told = console.Status.Text;

        console.OcrFolder = Path.Combine(directory.FullName, "ocr");
        var next = CaptionAllTextTests.WriteDeck(directory.CreateSubdirectory("next").FullName, [CaptionScorePictureTests.ScorePng()],
            CaptionAllTextTests.ScoreSlide(0), CaptionAllTextTests.ScoreSlide(0), CaptionAllTextTests.ScoreSlide(0));
        Assert.True(await console.PrepareDeckAsync(next, 1));
        Assert.Null(failure); // the first reading ended, not faulted,
        Assert.StartsWith(CaptionText.Format("CaptionConsole", "LyricsReadFailed", ""), told); // as a read failure
        Assert.Equal(next, session.Deck!.Path);
        Assert.Equal(3, console.Settings.Slides.Items.Count); // shown
        await console.ScorePreparation;
        await console.LyricsReading;
        Assert.Contains("3장 첫 줄", session.TextFor(3, CaptionSession.RecognizedKey));
    });

    // a048 design 17: a reading that fails after the caption left its deck writes nothing over the next deck's label or
    // status (the error is still logged).
    [Fact]
    public Task AFailureOfALeftDecksReadingDoesNotWriteOverTheNextDeck() => WithConsole(async (console, session, directory) =>
    {
        var failing = new TaskCompletionSource();
        var started = new TaskCompletionSource();
        console.ReadLyrics = async (files, _) => { started.TrySetResult(); await failing.Task; throw new IOException("picture not written"); };
        var first = CaptionAllTextTests.WriteDeck(directory.CreateSubdirectory("first").FullName, [CaptionScorePictureTests.ScorePng()],
            CaptionAllTextTests.ScoreSlide(0), CaptionAllTextTests.ScoreSlide(0));
        Assert.True(await console.PrepareDeckAsync(first, 1));
        var firstReading = console.LyricsReading;
        await started.Task.WaitAsync(TimeSpan.FromSeconds(10)); // the left deck's reading is under way

        console.ReadLyrics = (files, _) => Task.FromResult(ReadLyrics(files));
        var next = CaptionAllTextTests.WriteDeck(directory.CreateSubdirectory("next").FullName, [CaptionScorePictureTests.ScorePng()],
            CaptionAllTextTests.ScoreSlide(0));
        Assert.True(await console.PrepareDeckAsync(next, 1));
        await console.ScorePreparation;
        await console.LyricsReading;
        var (label, status) = (console.DeckLabel.Text, console.Status.Text);
        failing.SetResult(); // the left deck's reading fails now
        await firstReading;
        Assert.Equal((label, status), (console.DeckLabel.Text, console.Status.Text));
        Assert.Contains("picture not written", string.Concat(directory.GetFiles("errors-*.log").Select(file => File.ReadAllText(file.FullName))));
    });

    // a048 design 19: when some batches fail, each distinct reason (the exit code and error text, the time limit) is
    // logged once for the reading.
    [Fact]
    public Task TheReasonsSomeSlidesCouldNotBeReadAreLoggedOnce() => WithConsole(async (console, session, directory) =>
    {
        var path = CaptionAllTextTests.WriteDeck(directory.FullName, [CaptionScorePictureTests.ScorePng()],
            CaptionAllTextTests.ScoreSlide(0), CaptionAllTextTests.ScoreSlide(0), CaptionAllTextTests.ScoreSlide(0));
        string broke = CaptionText.Format("CaptionConsole", "OcrError", 1, "helper broke");
        console.ReadLyrics = (files, _) => Task.FromResult<OcrPage[]>([.. ReadLyrics(files).Select(page =>
            Path.GetFileNameWithoutExtension(page.File) == "1" ? page : new OcrPage(page.File, broke, []))]);
        Assert.True(await console.PrepareDeckAsync(path, 1));
        await console.ScorePreparation;
        await console.LyricsReading;
        Assert.Contains(Unread(3, 2), console.DeckLabel.Text);
        var log = string.Concat(directory.GetFiles("errors-*.log").Select(file => File.ReadAllText(file.FullName)));
        Assert.Equal(1, log.Split(broke).Length - 1); // two slides, one reason, one line
    });

    // The PC without Korean OCR keeps its own notice: no slide is told unread.
    [Fact]
    public Task WithoutKoreanOcrNoSlideIsToldUnread() => WithConsole(async (console, session, directory) =>
    {
        var path = CaptionAllTextTests.WriteDeck(directory.FullName, [CaptionScorePictureTests.ScorePng()],
            CaptionAllTextTests.ScoreSlide(0), CaptionAllTextTests.ScoreSlide(0));
        string notice = CaptionText.Format("CaptionConsole", "OcrNoKorean", "no ko");
        console.ReadLyrics = (files, _) => Task.FromException<OcrPage[]>(new LyricsReader.NoKoreanOcrException(notice));
        Assert.True(await console.PrepareDeckAsync(path, 1));
        await console.ScorePreparation;
        await console.LyricsReading;
        Assert.Equal(Path.GetFileName(path), console.DeckLabel.Text);
        Assert.Equal(CaptionText.Format("CaptionConsole", "LyricsReadFailed", notice), console.Status.Text);
    });

    // a048 design 9 (the spec's scenario of going to a song's lyrics and back while reading): the screen going to a song's text does not end the
    // deck's reading, because the session keeps the deck and uses it again without reading it when the screen comes back.
    [Fact]
    public Task TheScreenGoingToASongsTextDoesNotEndTheDecksReading() => WithConsole(async (console, session, directory) =>
    {
        var path = CaptionAllTextTests.WriteDeck(directory.FullName, [CaptionScorePictureTests.ScorePng()],
            CaptionAllTextTests.ScoreSlide(0), CaptionAllTextTests.ScoreSlide(0));
        var started = new TaskCompletionSource<CancellationToken>();
        var reading = new TaskCompletionSource();
        console.ReadLyrics = async (files, token) =>
        {
            started.TrySetResult(token);
            await reading.Task;
            return ReadLyrics(files);
        };
        Assert.True(await console.PrepareDeckAsync(path, 1));
        var deck = session.Deck;
        var lyricsReading = console.LyricsReading;
        var token = await started.Task.WaitAsync(TimeSpan.FromSeconds(10));
        console.ScreenDeck = () => (path, 1);
        console.ScreenMode.IsChecked = true; // following the screen, which shows the deck
        await Pump(console, 100);
        var sync = typeof(CaptionConsole).GetMethod("SyncScreenAsync", Private)!;
        console.ScreenDeck = () => null; // the output screen shows a song's text
        await (Task)sync.Invoke(console, null)!;
        Assert.False(token.IsCancellationRequested);
        console.ScreenDeck = () => (path, 1); // and the deck again
        await (Task)sync.Invoke(console, null)!;
        Assert.Same(deck, session.Deck); // (kept: not read again)
        Assert.Same(lyricsReading, console.LyricsReading);
        Assert.False(token.IsCancellationRequested);
        reading.SetResult();
        await lyricsReading;
        Assert.Contains("1장 첫 줄", session.TextFor(1, CaptionSession.RecognizedKey));
        Assert.Contains("2장 첫 줄", session.TextFor(2, CaptionSession.RecognizedKey));
    });

    // a048 design 11: every slide whose picture was to be read counts, one whose picture could not be made among the
    // unread; a slide read twice (the same file in two pages) is read once.
    [Fact]
    public Task TheSlidesToldUnreadAreCountedFromTheSlidesToBeRead() => WithConsole(async (console, session, directory) =>
    {
        // 1, 2: scores in content pictures; 3: a score cut in pieces (no content picture: its render is its picture, and
        // there is no renderer here).
        string pieces = "<p:spTree>" + CaptionScorePictureTests.Image(0, CaptionScorePictureTests.Full, 1600000)
            + CaptionScorePictureTests.Image(0, CaptionScorePictureTests.Full, 2400000, y: 2000000) + "</p:spTree>";
        var path = CaptionAllTextTests.WriteDeck(directory.FullName, [CaptionScorePictureTests.ScorePng()],
            CaptionAllTextTests.ScoreSlide(0), CaptionAllTextTests.ScoreSlide(0), pieces);
        IReadOnlyList<string>? sent = null;
        console.ReadLyrics = (files, _) =>
        {
            sent = files;
            var pages = ReadLyrics(files);
            return Task.FromResult<OcrPage[]>([.. pages, pages[0]]);
        };
        Assert.True(await console.PrepareDeckAsync(path, 1));
        await console.ScorePreparation;
        await console.LyricsReading;
        Assert.Equal(new[] { "1", "2" }, sent!.Select(file => Path.GetFileNameWithoutExtension(file)!).ToArray());
        Assert.Contains(Unread(3, 1), console.DeckLabel.Text);
    });

    // a048 design 9 (the spec's scenario of moving to another deck while reading): the reading of a deck the caption has left is ended, so it
    // starts no further batch (LyricsReader stops at the cancellation); its end tells nothing over the next deck.
    [Fact]
    public Task TheReadingOfALeftDeckIsEnded() => WithConsole(async (console, session, directory) =>
    {
        var first = CaptionAllTextTests.WriteDeck(directory.CreateSubdirectory("first").FullName, [CaptionScorePictureTests.ScorePng()],
            CaptionAllTextTests.ScoreSlide(0), CaptionAllTextTests.ScoreSlide(0));
        var started = new TaskCompletionSource<CancellationToken>();
        console.ReadLyrics = async (files, token) =>
        {
            started.TrySetResult(token);
            await Task.Delay(Timeout.Infinite, token);
            return [];
        };
        Assert.True(await console.PrepareDeckAsync(first, 1));
        var firstReading = console.LyricsReading;
        var token = await started.Task.WaitAsync(TimeSpan.FromSeconds(10));
        Assert.False(token.IsCancellationRequested);

        console.ReadLyrics = (files, _) => Task.FromResult(ReadLyrics(files));
        var next = CaptionAllTextTests.WriteDeck(directory.CreateSubdirectory("next").FullName, [CaptionScorePictureTests.ScorePng()],
            CaptionAllTextTests.ScoreSlide(0));
        Assert.True(await console.PrepareDeckAsync(next, 1));
        Assert.True(token.IsCancellationRequested); // the left deck's reading is ended
        await firstReading.WaitAsync(TimeSpan.FromSeconds(10));
        await console.ScorePreparation;
        await console.LyricsReading;
        Assert.Contains("1장 첫 줄", session.TextFor(1, CaptionSession.RecognizedKey));
        Assert.DoesNotContain(CaptionText.Format("CaptionConsole", "LyricsReadFailed", ""), console.Status.Text);
        Assert.Empty(directory.GetFiles("errors-*.log"));
    });

    // a048 design 7 (the spec's scenario): the worship order captured for the recovery and put back has the song's
    // chords and capo.
    [Fact]
    public Task TheRecoveredWorshipOrderKeepsASongsChordsAndCapo() => WithConsole(async (console, _, _) =>
    {
        var host = (MainViewModelCaptionHost)console.HostApp;
        var song = new LiveQueueItem("song:a048", "주 하나님 지으신 모든 세계", LiveItemKinds.Song)
            { Lyrics = "[1]\n주 하나님 지으신 모든 세계", Capo = 2, Notations = "(1;G;0;)" };
        host.Main.LoadQueue([song, new LiveQueueItem("ppt:1", "광고", LiveItemKinds.PowerPoint) { ContentPath = @"C:\광고.pptx" }]);
        ICaptionQueueRecovery recovery = host;
        var (items, _) = recovery.Capture();
        host.Main.LoadQueue([]);
        recovery.Restore(items, host.Main.CurrentWorshipListName, 0);
        Assert.Equal((2, "(1;G;0;)"), (host.Main.Queue[0].Capo, host.Main.Queue[0].Notations));
        Assert.Equal((0, (string?)null), (host.Main.Queue[1].Capo, host.Main.Queue[1].Notations));
        await Task.CompletedTask;
    });

    [Fact]
    public Task WhenEverySlideIsReadNoSlideIsToldUnread() => WithConsole(async (console, session, directory) =>
    {
        var path = CaptionAllTextTests.WriteDeck(directory.FullName, [CaptionScorePictureTests.ScorePng()],
            CaptionAllTextTests.ScoreSlide(0), CaptionAllTextTests.ScoreSlide(0));
        console.ReadLyrics = (files, _) => Task.FromResult(ReadLyrics(files));
        Assert.True(await console.PrepareDeckAsync(path, 1));
        await console.ScorePreparation;
        await console.LyricsReading;
        Assert.Contains("2장 첫 줄", session.TextFor(2, CaptionSession.RecognizedKey));
        Assert.Equal(CaptionText.Get("CaptionConsole", "RecognizedChosen"), console.Status.Text); // (the offer's notice, as before)
        Assert.Equal(CaptionText.Format("CaptionConsole", "LyricsFound", Path.GetFileName(path), 2), console.DeckLabel.Text);
    });
}
