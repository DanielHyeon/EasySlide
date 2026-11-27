using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;
using Axicultus.Wpf.Captions;
using Axicultus.Wpf.Media;
using Axicultus.Wpf.Rendering;
using Axicultus.Wpf.Settings;
using Axicultus.Wpf.Shell;
using Axicultus.Wpf.Support;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Axicultus.Wpf.Tests.Shell;

// In the WPF Application collection: the console lays out with the application's resources, which other tests in that
// collection change (the interface size); sharing the collection keeps them from running at the same time.
[Collection("WPF Application")]
public sealed partial class CaptionConsoleTests
{
    [Fact]
    public Task WindowPublishesOnlyOnTakeAndSuppressesImeComposition() => WithConsole(async (console, session, _) =>
    {
        Assert.True(console.Instant.IsChecked); // typing goes on air by default
        console.Instant.SetCurrentValue(CheckBox.IsCheckedProperty, false);
        console.Draft.SetCurrentValue(TextBox.TextProperty, "첫 번째 문장");
        Assert.Equal("", session.PublicText);
        await ClickAsync(console.TakeButton);
        Assert.Equal("첫 번째 문장", session.PublicText);
        Assert.Equal("첫 번째 문장", Assert.Single(session.History).Text);

        console.Instant.SetCurrentValue(CheckBox.IsCheckedProperty, true);
        var keyboard = InputManager.Current.PrimaryKeyboardDevice;
        var composition = new TextComposition(InputManager.Current, console.Draft, "한");
        console.Draft.RaiseEvent(new TextCompositionEventArgs(keyboard, composition)
            { RoutedEvent = TextCompositionManager.PreviewTextInputStartEvent });
        console.Draft.SetCurrentValue(TextBox.TextProperty, "ㅎ");
        Assert.Equal("첫 번째 문장", session.PublicText);
        Assert.False(console.TakeButton.IsEnabled);
        console.Draft.SetCurrentValue(TextBox.TextProperty, "한");
        console.Draft.RaiseEvent(new TextCompositionEventArgs(keyboard, composition)
            { RoutedEvent = TextCompositionManager.TextInputEvent, Handled = true });
        await console.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.ApplicationIdle);
        Assert.Equal("한", session.PublicText);
        Assert.Equal("첫 번째 문장", Assert.Single(session.History).Text);
    }, expectedSavedDraft: "한");

    [Fact]
    public Task HidingKeepsNewDraftOffAirAndHeldShortcutCannotAddHistory() => WithConsole(async (console, session, _) =>
    {
        var editor = Assert.IsType<CaptionConsoleViewModel>(console.DataContext);
        Assert.False(session.Visible); // a new console starts hidden
        console.Draft.SetCurrentValue(TextBox.TextProperty, "송출 중");
        Assert.Equal("", session.PublicText); // typing alone does not reveal it
        editor.TakeFromShortcut(isRepeat: false); // sending (Enter) does
        Assert.Equal("송출 중", session.PublicText);
        console.Instant.SetCurrentValue(CheckBox.IsCheckedProperty, true);
        await ClickAsync(console.AirButton);
        console.Draft.SetCurrentValue(TextBox.TextProperty, "대기 중");
        editor.TakeFromShortcut(isRepeat: true);
        Assert.Equal("", session.PublicText);
        Assert.True(console.Instant.IsChecked);
        Assert.Equal("송출 중", Assert.Single(session.History).Text);
        await ClickAsync(console.AirButton);
        Assert.Equal("대기 중", session.PublicText); // what was typed while hidden goes out on Show
        console.Visibility = Visibility.Collapsed; // the preview tab shown: the caption stays on air, the box keeps its words
        await console.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.ApplicationIdle);
        Assert.Equal("대기 중", session.PublicText);
        console.Visibility = Visibility.Visible;
        Assert.Equal("대기 중", console.Draft.Text);
    }, expectedSavedDraft: "대기 중");

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public Task FailedDeckReadKeepsFollowingTheCurrentDeckAndLogsTheError(bool corruptFile) =>
        WithConsole(async (console, session, directory) =>
        {
            var previous = new CaptionDeck("live.pptx", [new([new("body", "본문", "송출 중")])]);
            session.Prepare(previous);
            session.Follow(1, "body");
            var published = session.Published;
            var path = Path.Combine(directory.FullName, "새 자료.pptx");
            if (corruptFile) await File.WriteAllTextAsync(path, "invalid PPT");
            await console.PrepareDeckAsync(path, 1);
            Assert.Contains("PPT 읽기 실패", console.Status.Text);
            Assert.Same(previous, session.Deck);
            Assert.True(session.Following);
            Assert.Same(published, session.Published);
            var log = File.ReadAllText(Assert.Single(directory.GetFiles("*.log")).FullName);
            Assert.Contains("open caption PPT", log);
            if (!corruptFile) Assert.Contains(nameof(FileNotFoundException), log);
        });

    [Fact]
    public Task ReadingAnotherDeckStopsTheLastDecksScorePreparation() => WithConsole(async (console, _, directory) =>
    {
        var slide = CaptionScorePictureTests.Image(0, CaptionScorePictureTests.Full, CaptionScorePictureTests.Wide);
        var large = CaptionScorePictureTests.WriteDeck(directory.CreateSubdirectory("a").FullName, [CaptionScorePictureTests.ScorePng()], [.. Enumerable.Repeat(slide, 400)]);
        var next = CaptionScorePictureTests.WriteDeck(directory.CreateSubdirectory("b").FullName, [CaptionScorePictureTests.ScorePng()], slide);
        Assert.True(await console.PrepareDeckAsync(large, 1));
        var preparing = (Task)typeof(CaptionConsole).GetField("_scorePrepared", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!.GetValue(console)!;
        Assert.False(preparing.IsCompleted, "the large deck was prepared before the next one came");
        Assert.True(await console.PrepareDeckAsync(next, 1));
        await Task.WhenAny(preparing, Task.Delay(TimeSpan.FromSeconds(60)));
        Assert.True(preparing.IsCanceled, $"the last deck's preparation went on: {preparing.Status}");
    });

    [Fact]
    public Task ASlideReadAgainAfterTheCacheDroppedItIsCutAsItWasPrepared() => WithConsole(async (console, session, directory) =>
    {
        var path = CaptionScorePictureTests.LyricsDeck(directory.FullName, CaptionScorePictureTests.SomewhatSmallerLyrics());
        Assert.True(await console.PrepareDeckAsync(path, 1));
        await console.ScorePreparation;
        var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
        var cache = (ScorePictures)typeof(CaptionConsole).GetField("_pictures", flags)!.GetValue(console)!;
        var deck = session.Deck!;
        Assert.True(cache.TryGet((deck.Path, 4), out var prepared));
        cache.Clear(); // dropped to make room
        await (Task)typeof(CaptionConsole).GetMethod("LoadPictureAsync", flags)!.Invoke(console, [deck, 4])!;
        Assert.True(cache.TryGet((deck.Path, 4), out var again), "the slide was not read again");
        Assert.Equal(prepared.Page.Rows, again.Page.Rows);
        for (int k = 0; k < prepared.Page.Rows.Length; k++)
            Assert.True(prepared.Page.Erase(k).SequenceEqual(again.Page.Erase(k)), $"row {k + 1} is painted over differently");
    });

    [Fact]
    public Task WhileTheSlidesPictureIsReadAgainTheLastRowIsTakenDown() => WithConsole(async (console, session, directory) =>
    {
        var path = CaptionScorePictureTests.LyricsDeck(directory.FullName, CaptionScorePictureTests.SomewhatSmallerLyrics());
        Assert.True(await console.PrepareDeckAsync(path, 1));
        await console.ScorePreparation;
        await ClickAsync(console.AirButton); // on air
        console.ScreenMode.IsChecked = true;
        console.ScoreOnly.IsChecked = true;
        session.Follow(2, "");
        var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
        ((ScorePictures)typeof(CaptionConsole).GetField("_pictures", flags)!.GetValue(console)!).Clear(); // dropped from the cache
        session.SetImage(System.Windows.Media.Imaging.BitmapSource.Create(4, 2, 96, 96, System.Windows.Media.PixelFormats.Gray8, null, new byte[8], 4));
        Assert.NotNull(session.PublicImage); // the slide's last row on air
        typeof(CaptionConsole).GetMethod("UpdateScore", flags)!.Invoke(console, null);
        Assert.Null(session.PublicImage); // while the picture is read again, the last row does not stay
    });

    [Fact]
    public Task WithoutARendererTheContentPicturesLyricsAreStillRead() => WithConsole(async (console, session, directory) =>
    {
        // No PowerPoint renderer here: the slides with a content picture need none, so their reading goes on (to the
        // OCR helper, missing in the tests) instead of stopping before it.
        var path = CaptionScorePictureTests.LyricsDeck(directory.FullName, CaptionScorePictureTests.SomewhatSmallerLyrics());
        Assert.True(await console.PrepareDeckAsync(path, 1));
        Assert.Contains("ocr", console.Status.Text);
    });

    [Fact]
    public Task ReadingTheNextSongsDeckTakesTheLastSongsScoreRowDown() => WithConsole(async (console, session, directory) =>
    {
        await ClickAsync(console.AirButton); // on air
        session.SetImage(System.Windows.Media.Imaging.BitmapSource.Create(4, 2, 96, 96, System.Windows.Media.PixelFormats.Gray8, null, new byte[8], 4));
        Assert.NotNull(session.PublicImage); // the last song's row is on air
        var path = CaptionScorePictureTests.WriteDeck(directory.FullName, [CaptionScorePictureTests.ScorePng()],
            CaptionScorePictureTests.Image(0, CaptionScorePictureTests.Full, CaptionScorePictureTests.Wide));
        Assert.True(await console.PrepareDeckAsync(path, 1));
        Assert.Null(session.PublicImage); // while this deck's pictures are read, none of the last song's rows stays
    });

    [Fact]
    public Task ANewlyReadDeckStartsOnTheSlidesWholeText() => WithConsole(async (console, session, directory) =>
    {
        var first = CaptionAllTextTests.WriteDeck(directory.CreateSubdirectory("a").FullName, [], CaptionAllTextTests.TextSlide("제목", "본문"));
        var next = CaptionAllTextTests.WriteDeck(directory.CreateSubdirectory("b").FullName, [], CaptionAllTextTests.TextSlide("다른 제목", "다른 본문"));
        Assert.True(await console.PrepareDeckAsync(first, 1));
        Assert.Equal(CaptionDeck.AllTextKey, Assert.IsType<CaptionBox>(console.Settings.Boxes.SelectedItem).Key);
        console.Settings.Boxes.SelectedItem = console.Settings.Boxes.Items.Cast<CaptionBox>().Single(box => box.Key == "shape:TextBox 1"); // the title
        session.Follow(1, "shape:TextBox 1");
        Assert.True(await console.PrepareDeckAsync(next, 1));
        // Not the last deck's box, though this deck has one by that name.
        Assert.Equal(CaptionDeck.AllTextKey, Assert.IsType<CaptionBox>(console.Settings.Boxes.SelectedItem).Key);
        Assert.Equal("다른 제목\n다른 본문", console.Settings.Cue.Text);
    });

    [Fact]
    public Task AutoSendsTheScoreOnAScoreSlideAndTheTextOnATextSlide() => WithConsole(async (console, session, directory) =>
    {
        var path = CaptionAllTextTests.WriteDeck(directory.FullName, [CaptionScorePictureTests.ScorePng()],
            CaptionAllTextTests.TextSlide("설교 제목", "본문 말씀"), CaptionAllTextTests.ScoreSlide(0));
        Assert.True(await console.PrepareDeckAsync(path, 1));
        await console.ScorePreparation;
        Assert.True(console.AutoShow.IsChecked); // the default
        await ClickAsync(console.AirButton); // on air
        console.ScreenMode.IsChecked = true;
        session.Follow(1, CaptionDeck.AllTextKey);
        Assert.Null(session.PublicImage);
        Assert.Equal("설교 제목\n본문 말씀", session.PublicText);
        session.MoveToSlide(2);
        Assert.NotNull(session.PublicImage);
        Assert.Equal(("picture", ""), (console.ScoreSource, session.PublicText)); // the score only
        session.MoveToSlide(1);
        Assert.Null(session.PublicImage);
        Assert.Equal("설교 제목\n본문 말씀", session.PublicText);
        console.TextOnly.IsChecked = true; // the operator's choice is kept
        Assert.Equal("text", session.Display);
        session.MoveToSlide(2);
        Assert.Null(session.PublicImage);
        console.ScoreAndText.IsChecked = true;
        Assert.Equal("scoretext", session.Display);
        console.AutoShow.IsChecked = true;
        Assert.Equal("auto", session.Display);
    });

    [Theory]
    [InlineData(false)]
    [InlineData(true)] // the settings window where the boxes now are, open
    public Task TheChosenBoxStaysChosenOverASlideWithoutIt(bool settingsOpen) => WithConsole(async (console, session, directory) =>
    {
        var path = CaptionAllTextTests.WriteDeck(directory.FullName, [],
            CaptionAllTextTests.Slide(("TextBox 3", "일장 상자"), ("TextBox 5", "일장 다른")),
            CaptionAllTextTests.Slide(("TextBox 9", "이장")),
            CaptionAllTextTests.Slide(("TextBox 3", "삼장 상자"), ("TextBox 6", "삼장 다른")));
        Assert.True(await console.PrepareDeckAsync(path, 1));
        await ClickAsync(console.AirButton);
        console.ScreenMode.IsChecked = true;
        session.Follow(1, CaptionDeck.AllTextKey);
        console.Settings.Boxes.SelectedItem = console.Settings.Boxes.Items.Cast<CaptionBox>().Single(box => box.Key == "shape:TextBox 3"); // the operator's pick
        Assert.Equal("일장 상자", session.PublicText);
        session.MoveToSlide(2); // no TextBox 3: its whole text, nothing selected in the list
        Assert.Equal("이장", session.PublicText);
        Assert.Null(console.Settings.Boxes.SelectedItem);
        Assert.Equal("이장", console.Settings.Cue.Text);
        Assert.Equal("shape:TextBox 3", session.BoxKey);
        session.MoveToSlide(3); // TextBox 3 again
        Assert.Equal("삼장 상자", session.PublicText);
        Assert.Equal("shape:TextBox 3", Assert.IsType<CaptionBox>(console.Settings.Boxes.SelectedItem).Key);
        // The recovery file keeps the operator's pick too.
        var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
        session.MoveToSlide(2);
        typeof(CaptionConsole).GetMethod("Changed", flags)!.Invoke(console, null);
        await (Task)typeof(CaptionConsole).GetMethod("SaveAsync", flags)!.Invoke(console, null)!;
        var store = (CaptionRecoveryStore)typeof(CaptionConsole).GetField("_store", flags)!.GetValue(console)!;
        Assert.Equal("shape:TextBox 3", store.Read((string)typeof(CaptionConsole).GetField("_plan", flags)!.GetValue(console)!)!.Box);
        // On a slide without the chosen box the operator picks the whole text: that is the choice now.
        console.Settings.Boxes.SelectedItem = console.Settings.Boxes.Items.Cast<CaptionBox>().Single(box => box.Key == CaptionDeck.AllTextKey);
        Assert.Equal(CaptionDeck.AllTextKey, session.BoxKey);
        session.MoveToSlide(3);
        Assert.Equal("삼장 상자\n삼장 다른", session.PublicText);
    }, openSettings: settingsOpen);

    // OCR pages for the slide files RecognizeDeckAsync writes ("{slide}.png"): a line under each of the two staves of
    // CaptionScorePictureTests.ScorePng, or one line for a photo slide.
    private static OcrPage[] ReadLyrics(IReadOnlyList<string> files, params int[] photos) => [.. files.Select(file =>
    {
        static OcrLine At(int center, string text) => new([new OcrWord(text, 100, center - 10, 200, 20)]);
        var slide = int.Parse(Path.GetFileNameWithoutExtension(file));
        return photos.Contains(slide) ? new OcrPage(file, null, [At(177, "사진 속 간판")])
            : new OcrPage(file, null, [At(177, $"{slide}장 첫 줄"), At(357, $"{slide}장 둘째 줄")]);
    })];

    // A renderer whose slides are blank (no staves: a photo).
    private sealed class BlankRender : IPowerPointRenderService
    {
        public Task<PowerPointRenderResult> RenderSlideAsync(PowerPointRenderRequest request, CancellationToken cancellationToken = default)
        {
            var encoder = new System.Windows.Media.Imaging.PngBitmapEncoder();
            var pixels = new byte[192 * 108];
            Array.Fill(pixels, (byte)255);
            encoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(System.Windows.Media.Imaging.BitmapSource.Create(192, 108, 96, 96, System.Windows.Media.PixelFormats.Gray8, null, pixels, 192)));
            using var stream = new MemoryStream();
            encoder.Save(stream);
            return Task.FromResult(new PowerPointRenderResult(PowerPointRenderErrorKind.None,
                new PowerPointSlideSnapshot(request.FilePath, request.SlideNumber, 3, 192, 108, stream.ToArray(), "image/png", DateTimeOffset.UtcNow), null, false, TimeSpan.Zero));
        }

        public void ClearCache() { }
    }

    // A renderer whose slides all show two staves with a lyric line under each, at the heights ReadLyrics puts its
    // lines (a photo's roof planks read as five lines look like this too).
    private sealed class StaffRender : IPowerPointRenderService
    {
        public Task<PowerPointRenderResult> RenderSlideAsync(PowerPointRenderRequest request, CancellationToken cancellationToken = default)
        {
            var encoder = new System.Windows.Media.Imaging.PngBitmapEncoder();
            encoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(ScoreSlide(false, 90, 260)));
            using var stream = new MemoryStream();
            encoder.Save(stream);
            return Task.FromResult(new PowerPointRenderResult(PowerPointRenderErrorKind.None,
                new PowerPointSlideSnapshot(request.FilePath, request.SlideNumber, 2, 1200, 800, stream.ToArray(), "image/png", DateTimeOffset.UtcNow), null, false, TimeSpan.Zero));
        }

        public void ClearCache() { }
    }

    [Fact]
    public Task AOneSlideDeckWithTypedTextKeepsItsTextAfterTheLyricsAreRead() => WithConsole(async (console, session, directory) =>
    {
        console.ReadLyrics = (files, _) => Task.FromResult(ReadLyrics(files));
        // One slide: the choir's typed words under a picture (the "same on every slide" of one slide proves nothing).
        var typed = CaptionAllTextTests.WriteDeck(directory.CreateSubdirectory("typed").FullName, [CaptionScorePictureTests.ScorePng()],
            CaptionAllTextTests.ScoreSlide(0, "할렐루야 찬양대 찬양하네 빛 되신 구주"));
        Assert.True(await console.PrepareDeckAsync(typed, 1));
        await console.ScorePreparation;
        await console.LyricsReading;
        Assert.Equal(CaptionDeck.AllTextKey, Assert.IsType<CaptionBox>(console.Settings.Boxes.SelectedItem).Key);
        Assert.Equal("할렐루야 찬양대 찬양하네 빛 되신 구주", session.TextFor(1, CaptionDeck.AllTextKey));
        // One slide with nothing typed: the lyrics read are the caption.
        var empty = CaptionAllTextTests.WriteDeck(directory.CreateSubdirectory("empty").FullName, [CaptionScorePictureTests.ScorePng()],
            CaptionAllTextTests.ScoreSlide(0));
        Assert.True(await console.PrepareDeckAsync(empty, 1));
        await console.ScorePreparation;
        await console.LyricsReading;
        Assert.Equal(CaptionSession.RecognizedKey, Assert.IsType<CaptionBox>(console.Settings.Boxes.SelectedItem).Key);
    });

    [Fact]
    public Task AOneSlideDeckWithAShortTitleSwitchesToLyricsThatReadAsLyrics() => WithConsole(async (console, session, directory) =>
    {
        static OcrPage[] Read(IReadOnlyList<string> files, string line) =>
            [.. files.Select(file => new OcrPage(file, null, [new OcrLine([new OcrWord(line, 100, 167, 200, 20)])]))];
        // A short title over the lyrics picture: Korean lyrics read from it become the caption.
        console.ReadLyrics = (files, _) => Task.FromResult(Read(files, "주 하나님 지으신 모든 세계"));
        var lyrics = CaptionAllTextTests.WriteDeck(directory.CreateSubdirectory("lyrics").FullName, [CaptionScorePictureTests.ScorePng()],
            CaptionAllTextTests.ScoreSlide(0, "찬양대 2부"));
        Assert.True(await console.PrepareDeckAsync(lyrics, 1));
        await console.ScorePreparation;
        await console.LyricsReading;
        Assert.Equal(CaptionSession.RecognizedKey, Assert.IsType<CaptionBox>(console.Settings.Boxes.SelectedItem).Key);
        // The same with a misread photo: the title stays.
        console.ReadLyrics = (files, _) => Task.FromResult(Read(files, "하*날1까 `"));
        var photo = CaptionAllTextTests.WriteDeck(directory.CreateSubdirectory("photo").FullName, [CaptionScorePictureTests.ScorePng()],
            CaptionAllTextTests.ScoreSlide(0, "찬양대 2부"));
        Assert.True(await console.PrepareDeckAsync(photo, 1));
        await console.ScorePreparation;
        await console.LyricsReading;
        Assert.Equal(CaptionDeck.AllTextKey, Assert.IsType<CaptionBox>(console.Settings.Boxes.SelectedItem).Key);
        Assert.Equal("찬양대 2부", session.TextFor(1, CaptionDeck.AllTextKey));
    });

    [Fact]
    public Task APictureThatCouldNotBeMeasuredLeavesTheRenderItsStaves() => WithConsole(async (console, session, directory) =>
    {
        // The content picture is damaged: whether it shows staves is not known, so the render's staves count.
        var path = CaptionAllTextTests.WriteDeck(directory.FullName, [[0x89, 0x50, 0x4E, 0x47, 1, 2, 3, 4, 5, 6, 7, 8]], CaptionAllTextTests.ScoreSlide(0));
        console.ReadLyrics = (files, _) => Task.FromResult(ReadLyrics(files));
        Assert.True(await console.PrepareDeckAsync(path, 1));
        await console.ScorePreparation;
        await console.LyricsReading;
        Assert.Equal("1장 첫 줄\n1장 둘째 줄", session.Deck!.Slides[0].Boxes[0].Text);
    }, render: new StaffRender());

    [Fact]
    public Task APhotoWhoseRenderLooksLikeAStaffDoesNotReplaceTheWholeText() => WithConsole(async (console, session, directory) =>
    {
        // 1: a photo as the slide's content picture (no staves on it), whose render shows five lines; 2: a score cut in two
        // pieces (no content picture: the render is its picture). Neither has text; both renders show a staff.
        string Pieces() => "<p:spTree>" + CaptionScorePictureTests.Image(0, CaptionScorePictureTests.Full, 1600000)
            + CaptionScorePictureTests.Image(0, CaptionScorePictureTests.Full, 2400000, y: 2000000) + "</p:spTree>";
        var path = CaptionAllTextTests.WriteDeck(directory.FullName, [CaptionScorePictureTests.ScorePng(), CaptionAllTextTests.Png(64, 36)],
            CaptionAllTextTests.ScoreSlide(1), Pieces());
        console.ReadLyrics = (files, _) => Task.FromResult(ReadLyrics(files, 1));
        Assert.True(await console.PrepareDeckAsync(path, 1));
        await console.ScorePreparation;
        await console.LyricsReading;
        Assert.Equal("", session.Deck!.Slides[0].Boxes[0].Text); // the photo's text replaces nothing
        Assert.Equal("사진 속 간판", session.Deck.Slides[0].Boxes.Single(box => box.Key == CaptionSession.RecognizedKey).Text);
        Assert.Equal("2장 첫 줄\n2장 둘째 줄", session.Deck.Slides[1].Boxes[0].Text); // the score in pieces still does
    }, render: new StaffRender());

    private static readonly System.Reflection.BindingFlags Private = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;

    [Fact]
    public Task LyricsReadAfterTheScoreRowWentOnAirKeepTheRowAndThenReplaceShortWholeText() => WithConsole(async (console, session, directory) =>
    {
        // Two score slides and a photo slide, no text on any: the whole texts are the same (empty) everywhere.
        var photo = CaptionAllTextTests.Png(64, 36);
        var path = CaptionAllTextTests.WriteDeck(directory.FullName, [CaptionScorePictureTests.ScorePng(), photo],
            CaptionAllTextTests.ScoreSlide(0), CaptionAllTextTests.ScoreSlide(0), CaptionAllTextTests.ScoreSlide(1));
        var reading = new TaskCompletionSource();
        console.ReadLyrics = async (files, _) => { await reading.Task; return ReadLyrics(files, 3); };
        Assert.True(await console.PrepareDeckAsync(path, 1));
        await console.ScorePreparation;
        await ClickAsync(console.AirButton);
        console.ScreenMode.IsChecked = true;
        console.ScoreOnly.IsChecked = true;
        session.Follow(1, CaptionDeck.AllTextKey);
        typeof(CaptionConsole).GetMethod("Next", Private)!.Invoke(console, [null, null]);
        Assert.Contains("2/2줄", console.ScoreRowLabel.Text);
        reading.SetResult(); // the lyrics are read while row 2 is on air
        await console.LyricsReading;
        Assert.Contains("2/2줄", console.ScoreRowLabel.Text); // the row on air stays
        Assert.Equal(1, session.CurrentSlide);
        // The offer looked at the whole texts first (all empty: the recognized lyrics are chosen) ...
        Assert.Equal(CaptionSession.RecognizedKey, session.BoxKey);
        // ... then a score slide's empty whole text became its lyrics; the photo's text replaced nothing.
        Assert.Equal("1장 첫 줄\n1장 둘째 줄", session.TextFor(1, CaptionDeck.AllTextKey));
        Assert.Equal("", session.TextFor(3, CaptionDeck.AllTextKey));
        Assert.Equal("사진 속 간판", session.Deck!.Slides[2].Boxes.Single(box => box.Key == CaptionSession.RecognizedKey).Text);
    }, render: new BlankRender());

    [Theory]
    [InlineData(false)]
    [InlineData(true)] // the settings window where the boxes now are, open
    public Task NewLyricsForTheSlideOnAirChangeOnlyItsCaptionText(bool settingsOpen) => WithConsole(async (console, session, directory) =>
    {
        var path = CaptionAllTextTests.WriteDeck(directory.FullName, [CaptionScorePictureTests.ScorePng()],
            CaptionAllTextTests.ScoreSlide(0), CaptionAllTextTests.ScoreSlide(0));
        Assert.True(await console.PrepareDeckAsync(path, 1));
        await console.ScorePreparation;
        await ClickAsync(console.AirButton);
        console.ScreenMode.IsChecked = true;
        console.ScoreAndText.IsChecked = true;
        session.Follow(1, CaptionDeck.AllTextKey);
        typeof(CaptionConsole).GetMethod("Next", Private)!.Invoke(console, [null, null]);
        Assert.Contains("2/2줄", console.ScoreRowLabel.Text);
        int sent = 0;
        session.PublishedChanged += _ => sent++;
        session.AddRecognized(1, "새로 읽은 가사", staves: true);
        Assert.Contains("2/2줄", console.ScoreRowLabel.Text); // the row stays
        Assert.Equal(("새로 읽은 가사", 1), (session.PublicText, sent)); // the caption text alone went out again
        session.AddRecognized(1, "새로 읽은 가사", staves: true);
        Assert.Equal(1, sent); // the same text: nothing went out
        // The operator looks at slide 2 and edits its caption, not saved yet; lyrics for the slide on air come.
        console.Settings.Slides.SelectedItem = 2;
        console.Settings.Cue.Text = "고치는 중";
        session.AddRecognized(1, "또 읽은 가사", staves: true);
        typeof(CaptionConsole).GetMethod("RefreshBoxes", Private)!.Invoke(console, [CaptionSession.RecognizedKey]); // the offer
        Assert.Equal((2, "고치는 중"), (console.Settings.Slides.SelectedItem, console.Settings.Cue.Text));
        Assert.Contains("2/2줄", console.ScoreRowLabel.Text);
    }, openSettings: settingsOpen);

    [Fact]
    public Task AWholeTextReplacedByLyricsSendsEachRowItsOwnLineUnderTheScore() => WithConsole(async (console, session, directory) =>
    {
        // A score with a short word under it, then a notice: their whole texts differ, so the recognized lyrics are not chosen.
        var path = CaptionAllTextTests.WriteDeck(directory.FullName, [CaptionScorePictureTests.ScorePng()],
            CaptionAllTextTests.ScoreSlide(0, "에게"), CaptionAllTextTests.TextSlide("다음 주일은 전교인 체육대회입니다"));
        console.ReadLyrics = (files, _) => Task.FromResult(ReadLyrics(files));
        Assert.True(await console.PrepareDeckAsync(path, 1));
        await console.ScorePreparation;
        await console.LyricsReading;
        Assert.Equal(CaptionDeck.AllTextKey, Assert.IsType<CaptionBox>(console.Settings.Boxes.SelectedItem).Key);
        Assert.Equal("1장 첫 줄\n1장 둘째 줄", session.TextFor(1, CaptionDeck.AllTextKey)); // the short text replaced
        await ClickAsync(console.AirButton);
        console.ScreenMode.IsChecked = true;
        console.ScoreAndText.IsChecked = true;
        session.Follow(1, CaptionDeck.AllTextKey);
        Assert.Equal("1장 첫 줄", session.PublicText);
        typeof(CaptionConsole).GetMethod("Next", Private)!.Invoke(console, [null, null]);
        Assert.Equal("1장 둘째 줄", session.PublicText);
    });

    [Fact]
    public Task ARestoredDeckHasItsLyricsReadAgainAndKeepsTheRestoredChoice() => WithConsole(async (console, session, directory) =>
    {
        var path = CaptionAllTextTests.WriteDeck(directory.FullName, [CaptionScorePictureTests.ScorePng()],
            CaptionAllTextTests.ScoreSlide(0), CaptionAllTextTests.ScoreSlide(0));
        console.ReadLyrics = (files, _) => Task.FromResult(ReadLyrics(files));
        var plan = (string)typeof(CaptionConsole).GetField("_plan", Private)!.GetValue(console)!;
        CaptionRecovery Saved(string box) => new(1, 1, plan, "", path, 2, box, [], [], []);
        // Restored on the whole text: the reading does not switch to the recognized lyrics (all whole texts are empty).
        await console.RestoreAsync(Saved(CaptionDeck.AllTextKey));
        await console.LyricsReading;
        Assert.Equal(CaptionDeck.AllTextKey, Assert.IsType<CaptionBox>(console.Settings.Boxes.SelectedItem).Key);
        Assert.Equal("2장 첫 줄\n2장 둘째 줄", session.TextFor(2, CaptionSession.RecognizedKey));
        // Restored on the recognized lyrics: they are there again.
        await console.RestoreAsync(Saved(CaptionSession.RecognizedKey));
        await console.LyricsReading;
        Assert.Equal(CaptionSession.RecognizedKey, Assert.IsType<CaptionBox>(console.Settings.Boxes.SelectedItem).Key);
        Assert.Equal("2장 첫 줄\n2장 둘째 줄", console.Settings.Cue.Text);
        // A saved choice of no box is the whole text, as when the console opens.
        await console.RestoreAsync(Saved(""));
        await console.LyricsReading;
        Assert.Equal(CaptionDeck.AllTextKey, Assert.IsType<CaptionBox>(console.Settings.Boxes.SelectedItem).Key);
    });

    [Fact]
    public Task ARecoveryWhoseDeckIsGoneChangesNothing() => WithConsole(async (console, session, directory) =>
    {
        var editor = Assert.IsType<CaptionConsoleViewModel>(console.DataContext);
        console.Draft.SetCurrentValue(TextBox.TextProperty, "지금 초안");
        var queue = new LiveQueueItem("song:1", "지금 곡", LiveItemKinds.Song) { Lyrics = "[1]\n가사" };
        var main = (MainViewModel)typeof(CaptionConsole).GetField("_main", Private)!.GetValue(console)!;
        main.LoadQueue([queue]);
        var plan = (string)typeof(CaptionConsole).GetField("_plan", Private)!.GetValue(console)!;
        await console.RestoreAsync(new(1, 1, plan, "저장한 초안", Path.Combine(directory.FullName, "없는.pptx"), 1, CaptionDeck.AllTextKey, [], [], []));
        Assert.Equal("지금 초안", editor.DraftText);
        Assert.Same(queue, Assert.Single(main.Queue));
        Assert.Contains("찾을 수 없어", console.Status.Text);
        Assert.Null(session.Deck);
        await Task.CompletedTask;
    });

    [Fact]
    public Task TheDisplayChosenElsewhereChecksItsRadio() => WithConsole((console, session, _) =>
    {
        session.SetStyle(session.Style with { Display = "scoretext" });
        Assert.True(console.ScoreAndText.IsChecked);
        Assert.Equal("scoretext", session.Display);
        session.SetStyle(session.Style with { Display = "fancy" }); // cleaned to auto
        Assert.Equal("auto", session.Style.Display);
        Assert.True(console.AutoShow.IsChecked);
        return Task.CompletedTask;
    });

    [Fact]
    public Task WithoutARowOfTheSlideTheLastRowIsTakenDownAndTheArrowsTurnSlides() => WithConsole(async (console, session, directory) =>
    {
        // Scores cut in two (the render shows them) and no renderer here: no row of the slide can be made.
        string Pieces() => """<p:spTree>""" + CaptionScorePictureTests.Image(0, CaptionScorePictureTests.Full, 1600000)
            + CaptionScorePictureTests.Image(0, CaptionScorePictureTests.Full, 2400000, y: 2000000) + "</p:spTree>";
        var path = CaptionAllTextTests.WriteDeck(directory.FullName, [CaptionScorePictureTests.ScorePng()], Pieces(), Pieces(), Pieces());
        Assert.True(await console.PrepareDeckAsync(path, 1));
        await ClickAsync(console.AirButton);
        console.ScreenMode.IsChecked = true;
        console.ScoreOnly.IsChecked = true;
        session.SetImage(System.Windows.Media.Imaging.BitmapSource.Create(4, 2, 96, 96, System.Windows.Media.PixelFormats.Gray8, null, new byte[8], 4)); // a row of another slide on air
        session.Follow(2, CaptionDeck.AllTextKey);
        Assert.Null(session.PublicImage); // taken down
        typeof(CaptionConsole).GetMethod("Next", Private)!.Invoke(console, [null, null]);
        Assert.Equal(3, session.CurrentSlide);
    });

    // A renderer whose first renders of slide 2 fail (PowerPoint busy), then give a score.
    private sealed class FailingRender(int failures) : IPowerPointRenderService
    {
        private int _asked;

        public Task<PowerPointRenderResult> RenderSlideAsync(PowerPointRenderRequest request, CancellationToken cancellationToken = default) =>
            request.SlideNumber == 2 && ++_asked <= failures
                ? Task.FromResult(new PowerPointRenderResult(PowerPointRenderErrorKind.Timeout, null, "PowerPoint 응답 없음", false, TimeSpan.Zero))
                : new StaffRender().RenderSlideAsync(request, cancellationToken);

        public void ClearCache() { }
    }

    // The score of a slide comes from its render and the render fails: it is tried again while the slide is on air, and
    // after the last failure the operator is told (never left at "making the score picture"); the text goes out meanwhile.
    [Theory]
    [InlineData(2, true)] // fails twice: the third render gives the row
    [InlineData(int.MaxValue, false)]
    public Task AFailedRenderOfTheSlideOnAirIsTriedAgainAndThenTold(int failures, bool row) => WithConsole(async (console, session, directory) =>
    {
        var path = CaptionAllTextTests.WriteDeck(directory.FullName, [],
            CaptionAllTextTests.TextSlide("일장"), CaptionAllTextTests.TextSlide("이장"), CaptionAllTextTests.TextSlide("삼장"));
        console.RenderRetryDelay = TimeSpan.FromMilliseconds(20);
        console.ScreenDeck = () => (path, 2);
        console.ScoreOnly.IsChecked = true;
        console.ScreenMode.IsChecked = true;
        await Pump(console, 1500);
        Assert.Equal((true, 2), (session.Following, session.CurrentSlide));
        Assert.Equal(row, session.ReadyImage != null);
        if (row) Assert.Equal("", console.Status.Text);
        else
        {
            Assert.StartsWith("악보 그림을 만들 수 없습니다", console.Status.Text);
            Assert.Contains("PowerPoint 응답 없음", console.Status.Text);
            Assert.Equal("이장", session.ReadyText); // the slide's text is the caption
        }
    }, render: new FailingRender(failures));

    [Theory]
    [InlineData(false)]
    [InlineData(true)] // the settings window where the boxes now are, open
    public Task AnUnsavedCaptionEditStaysWhileTheSlideOnAirMoves(bool settingsOpen) => WithConsole(async (console, session, directory) =>
    {
        var path = CaptionAllTextTests.WriteDeck(directory.FullName, [],
            CaptionAllTextTests.TextSlide("일장"), CaptionAllTextTests.TextSlide("이장"), CaptionAllTextTests.TextSlide("삼장"));
        Assert.True(await console.PrepareDeckAsync(path, 1));
        console.ScreenMode.IsChecked = true;
        session.Follow(1, CaptionDeck.AllTextKey);
        console.Settings.Cue.Text = "일장 고침"; // typed, not saved
        session.MoveToSlide(2);
        Assert.Equal((1, "일장 고침"), (console.Settings.Slides.SelectedItem, console.Settings.Cue.Text)); // the edit stays with its slide
        typeof(CaptionConsole).GetMethod("SaveOverride", Private)!.Invoke(console, [null, null]);
        Assert.Equal("일장 고침", session.TextFor(1, CaptionDeck.AllTextKey));
        session.MoveToSlide(3); // saved: the view follows again
        Assert.Equal((3, "삼장"), (console.Settings.Slides.SelectedItem, console.Settings.Cue.Text));
    }, openSettings: settingsOpen);

    [Theory]
    [InlineData(false)]
    [InlineData(true)] // the settings window where the boxes now are, open
    public Task PickingABoxOnAnotherSlideKeepsTheViewAndTheRowOnAir(bool settingsOpen) => WithConsole(async (console, session, directory) =>
    {
        var slide = CaptionAllTextTests.ScoreSlide(0, "에게");
        var path = CaptionAllTextTests.WriteDeck(directory.FullName, [CaptionScorePictureTests.ScorePng()], slide, slide, slide, slide, slide);
        Assert.True(await console.PrepareDeckAsync(path, 1));
        await console.ScorePreparation;
        await ClickAsync(console.AirButton);
        console.ScreenMode.IsChecked = true;
        console.ScoreOnly.IsChecked = true;
        session.Follow(1, CaptionDeck.AllTextKey);
        typeof(CaptionConsole).GetMethod("Next", Private)!.Invoke(console, [null, null]);
        Assert.Contains("2/2줄", console.ScoreRowLabel.Text);
        console.Settings.Slides.SelectedItem = 5; // the operator looks at slide 5 and picks its box
        console.Settings.Boxes.SelectedItem = console.Settings.Boxes.Items.Cast<CaptionBox>().Single(box => box.Key == "shape:남은 글");
        Assert.Equal("shape:남은 글", session.BoxKey);
        Assert.Equal((5, 1), (console.Settings.Slides.SelectedItem, session.CurrentSlide));
        Assert.Contains("2/2줄", console.ScoreRowLabel.Text);
    }, openSettings: settingsOpen);

    [Fact]
    public Task SwitchingToTheLyricsUnderAScoreRowGoesOutOnce() => WithConsole(async (console, session, directory) =>
    {
        var path = CaptionAllTextTests.WriteDeck(directory.FullName, [CaptionScorePictureTests.ScorePng()],
            CaptionAllTextTests.ScoreSlide(0), CaptionAllTextTests.ScoreSlide(0));
        var reading = new TaskCompletionSource();
        console.ReadLyrics = async (files, _) => { await reading.Task; return ReadLyrics(files); };
        Assert.True(await console.PrepareDeckAsync(path, 1));
        await console.ScorePreparation;
        await ClickAsync(console.AirButton);
        console.ScreenMode.IsChecked = true;
        console.ScoreAndText.IsChecked = true;
        session.Follow(1, CaptionDeck.AllTextKey);
        typeof(CaptionConsole).GetMethod("Next", Private)!.Invoke(console, [null, null]);
        var sent = new System.Collections.Generic.List<string>();
        session.PublishedChanged += published => sent.Add(published.Text);
        reading.SetResult();
        await console.LyricsReading;
        Assert.Equal(CaptionSession.RecognizedKey, session.BoxKey);
        Assert.Equal(["1장 둘째 줄"], sent); // the row's own line, with no flash of all the lyrics before it
    });

    [Fact]
    public Task WhileTheScoreIsMadeNextTurnsTheSlideInAuto() => WithConsole(async (console, session, directory) =>
    {
        var path = CaptionAllTextTests.WriteDeck(directory.FullName, [CaptionScorePictureTests.ScorePng()],
            CaptionAllTextTests.TextSlide("설교 제목"), CaptionAllTextTests.ScoreSlide(0), CaptionAllTextTests.ScoreSlide(0));
        Assert.True(await console.PrepareDeckAsync(path, 1));
        await console.ScorePreparation;
        console.ScreenMode.IsChecked = true;
        // The score pictures are being made (as right after the deck is read): the score slide sends its text meanwhile.
        var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
        typeof(CaptionConsole).GetField("_scoreDeck", flags)!.SetValue(console, null);
        typeof(CaptionConsole).GetField("_preparingScore", flags)!.SetValue(console, session.Deck);
        session.Follow(2, CaptionDeck.AllTextKey);
        Assert.Null(session.PublicImage);
        typeof(CaptionConsole).GetMethod("Next", flags)!.Invoke(console, [null, null]);
        Assert.Equal(3, session.CurrentSlide); // not a row of a score that is not on air
        typeof(CaptionConsole).GetMethod("Previous", flags)!.Invoke(console, [null, null]);
        Assert.Equal(2, session.CurrentSlide);
    });

    [Fact]
    public Task AKeptDisplayChoiceIsCheckedWhenTheConsoleOpens() => WithConsole((console, session, _) =>
    {
        Assert.Equal("score", session.Display);
        Assert.Equal((false, false, true, false), (console.AutoShow.IsChecked, console.TextOnly.IsChecked, console.ScoreOnly.IsChecked, console.ScoreAndText.IsChecked));
        return Task.CompletedTask;
    }, savedStyle: """{"Color":"#FFE14D","Box":true,"Lines":2,"Display":"score"}""");

    [Fact]
    public void RestoringADraftCannotPublishOrRequestAnotherSave()
    {
        using var session = new CaptionSession(new LiveSessionService());
        session.Publish("현재 송출");
        var editor = new CaptionConsoleViewModel(session) { IsInstant = true, IsRestoring = true };
        int saves = 0;
        editor.SaveRequested += () => saves++;
        editor.DraftText = "복구한 초안";
        editor.TakeFromShortcut(isRepeat: false);
        Assert.False(editor.TakeCommand.CanExecute(null));
        Assert.Equal("현재 송출", session.PublicText);
        Assert.Empty(session.History);
        Assert.Equal(0, saves);
    }

    [Fact]
    public void EnterSendsTheLineEmptiesTheBoxAndKeepsTheCaption()
    {
        using var session = new CaptionSession(new LiveSessionService());
        var editor = new CaptionConsoleViewModel(session) { IsInstant = true };
        session.SetVisible(true); // Show
        editor.DraftText = "주님은 나의 목자";
        Assert.Equal("주님은 나의 목자", session.PublicText);
        editor.TakeAndClear();
        Assert.Equal("", editor.DraftText);
        Assert.Equal("주님은 나의 목자", session.PublicText);
        editor.DraftText = "내게";
        Assert.Equal("내게", session.PublicText);
    }

    [Fact]
    public void StyleAndScoreImageReachThePublishedCaptionAndTheStyleIsKept() => StaHelper.RunOnSta(() =>
    {
        var directory = Directory.CreateTempSubdirectory();
        try
        {
            var path = Path.Combine(directory.FullName, "caption-style.json");
            using (var session = new CaptionSession(new LiveSessionService(), null, path))
            {
                session.Publish("자막");
                session.SetStyle(new CaptionStyle("#FFE14D", false));
                Assert.Equal(("자막", "#FFE14D", false), (session.Published.Text, session.Published.Color, session.Published.Box));
                var score = System.Windows.Media.Imaging.BitmapSource.Create(4, 2, 96, 96, System.Windows.Media.PixelFormats.Gray8, null, new byte[8], 4);
                session.SetImage(score);
                Assert.StartsWith("data:image/jpeg;base64,", session.Published.Image);
                Assert.Equal("", session.Published.Text); // score only
                Assert.True(session.HasText);
                Assert.False(session.HasPublicText);
                session.SetImage(score, withText: true); // score + caption
                Assert.Equal("자막", session.Published.Text);
                Assert.NotNull(session.Published.Image);
                Assert.NotNull(session.PublicImage);
                session.SetImage(null);
                Assert.Equal("자막", session.Published.Text);
            }
            using var reopened = new CaptionSession(new LiveSessionService(), null, path);
            Assert.Equal(new CaptionStyle("#FFE14D", false), reopened.Style);
            Assert.Equal(CaptionStyle.DefaultColor, CaptionStyle.Parse("red;background:url(x)", true).Color);
        }
        finally { directory.Delete(true); }
    });

    [Fact]
    public void ScoreHalfIsCutFromASlidePictureOwnedByAnotherThread() => StaHelper.RunOnSta(() =>
    {
        System.Windows.Media.Imaging.WriteableBitmap? slide = null;
        var ready = new System.Threading.ManualResetEventSlim();
        var owner = new System.Threading.Thread(() =>
        {
            var pixels = new byte[4 * 4];
            for (int i = 8; i < 16; i++) pixels[i] = 255; // bottom half white
            slide = new(4, 4, 96, 96, System.Windows.Media.PixelFormats.Gray8, null);
            slide.WritePixels(new System.Windows.Int32Rect(0, 0, 4, 4), pixels, 4, 0);
            ready.Set();
            Dispatcher.Run();
        });
        owner.SetApartmentState(System.Threading.ApartmentState.STA);
        owner.Start();
        ready.Wait();
        try
        {
            // No staff on it: the slide is cut in halves.
            var (bottom, rows, found) = CaptionConsole.CropRow(slide!, 1)!.Value;
            var top = CaptionConsole.CropRow(slide!, 0)!.Value.Image;
            Assert.Equal((2, false), (rows, found));
            Assert.True(bottom.IsFrozen);
            Assert.Equal((4, 2), (bottom.PixelWidth, bottom.PixelHeight));
            var row = new byte[8];
            bottom.CopyPixels(row, 4, 0);
            Assert.All(row, b => Assert.Equal(255, b));
            top.CopyPixels(row, 4, 0);
            Assert.All(row, b => Assert.Equal(0, b));
        }
        finally { Dispatcher.FromThread(owner)!.InvokeShutdown(); owner.Join(); }
    });

    // A slide like the hymn decks: title, then staves with a lyric line under each, at uneven heights.
    internal static System.Windows.Media.Imaging.BitmapSource ScoreSlide(bool lightOnDark, params int[] staffTops)
    {
        const int width = 1200, height = 800;
        byte paper = lightOnDark ? (byte)20 : (byte)235, ink = lightOnDark ? (byte)240 : (byte)15;
        var pixels = new byte[width * height];
        Array.Fill(pixels, paper);
        void Fill(int x, int y, int w, int h) { for (int r = y; r < y + h; r++) Array.Fill(pixels, ink, r * width + x, w); }
        Fill(200, 20, 300, 30); // title
        foreach (var top in staffTops)
        {
            for (int line = 0; line < 5; line++) Fill(100, top + line * 12, 900, 2); // staff
            Fill(300, top - 20, 14, 20); // a note above the staff
            Fill(150, top + 80, 700, 34); // lyrics under it
        }
        Fill(100, 770, 450, 18); // the verse buttons at the foot of the slide
        Fill(0, 300, 40, 470);   // a background picture beside the lower staves
        return System.Windows.Media.Imaging.BitmapSource.Create(width, height, 96, 96, System.Windows.Media.PixelFormats.Gray8, null, pixels, width);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ScoreRowsAreFoundWhereverTheStavesAre(bool lightOnDark) => StaHelper.RunOnSta(() =>
    {
        var rows = ScoreLayout.FindRows(ScoreSlide(lightOnDark, 90, 330, 560));
        Assert.Equal(3, rows.Length);
        Assert.InRange(rows[0].Area.Y, 51, 70); // the title (20..50) is left out, the note above the staff is in
        Assert.All(rows, row => Assert.Equal(rows[0].Area.Height, row.Area.Height)); // one height: the staves show the same size
        for (int k = 0; k < 3; k++)
        {
            int staffTop = new[] { 90, 330, 560 }[k];
            var area = rows[k].Area;
            Assert.InRange(area.Y, 0, staffTop - 20);                   // the note above the staff is in
            Assert.True(area.Y + area.Height >= staffTop + 80 + 34);    // its lyrics are in
            Assert.InRange(area.X, 60, 100);                            // empty slide and the background picture are left out
            Assert.InRange(area.X + area.Width, 1000, 1199);
            Assert.Equal((staffTop, staffTop + 48), (rows[k].StaffTop, rows[k].StaffBottom));
        }
        Assert.InRange(rows[2].Area.Y + rows[2].Area.Height, 560 + 80 + 34, 769); // the verse buttons are not in

        // The same deck with the staves elsewhere.
        var moved = ScoreLayout.FindRows(ScoreSlide(lightOnDark, 200, 450));
        Assert.Equal(2, moved.Length);
        Assert.InRange(moved[1].Area.Y, 200 + 80 + 34, 450 - 20);
    });

    [Fact]
    public void AScoreRowIsCutFromTheSlide() => StaHelper.RunOnSta(() =>
    {
        var (row, rows, found) = CaptionConsole.CropRow(ScoreSlide(false, 90, 330), 1)!.Value;
        Assert.Equal((2, true), (rows, found));
        Assert.True(row.IsFrozen);
        Assert.InRange(row.PixelHeight, 330 + 80 + 34 - 310, 800);
    });

    private static OcrLine Line(int y, params string[] words) =>
        new([.. words.Select((word, i) => new OcrWord(word, 100 + i * 120, y, 100, 40))]);

    [Fact]
    public void ReadLyricsLoseTheirHoldDashesAndNotesReadAsLetters()
    {
        Assert.Equal("받으소서", LyricsReader.Clean(Line(0, "받으-소-서---")));
        Assert.Equal("주여 내 맘을 받아 주소서", LyricsReader.Clean(Line(0, "주여", "내", "맘을-", "받아", "주소-서")));
        Assert.Equal("", LyricsReader.Clean(Line(0, "OI", "J", "8")));      // note heads and rests
        Assert.Equal("Amazing grace", LyricsReader.Clean(Line(0, "Amazing", "grace")));
    }

    [Fact]
    public void EachScoreRowGetsTheLyricsUnderItsStaffOnly()
    {
        var page = new OcrPage("1.png", null,
        [
            Line(40, "나의", "빰으소서"),            // the title above the first staff
            Line(150, "OI"),                         // a sign inside the staff
            Line(300, "주가", "기뻐하는-"),          // row 1 lyrics
            Line(610, "주의", "성전", "되게", "하소서"), // row 2 lyrics
            Line(760, "1.나의", "맘", "받으소서"),    // the verse buttons under the last row
        ]);
        ScoreRow[] rows =
        [
            new(new System.Windows.Int32Rect(0, 0, 1000, 420), 120, 200),
            new(new System.Windows.Int32Rect(0, 420, 1000, 300), 470, 540),
        ];
        Assert.Equal(["주가 기뻐하는", "주의 성전 되게 하소서"], LyricsReader.RowLyrics(page, rows));
        Assert.Equal(["나의 빰으소서\n주가 기뻐하는\n주의 성전 되게 하소서\n1.나의 맘 받으소서"], LyricsReader.RowLyrics(page, [])); // not a score
    }

    // A line read from left to right over [top, bottom), one word per space-separated part.
    private static OcrLine At(int x, int top, int bottom, string text) =>
        new([.. text.Split(' ').Select((word, i) => new OcrWord(word, x + i * 60, top, 50, bottom - top))]);

    [Fact]
    public void LyricsBelowARowsAreaGoToTheStaffAboveThem()
    {
        ScoreRow[] rows =
        [
            new(new System.Windows.Int32Rect(0, 100, 1000, 200), 150, 230), // the render's row ends at 300, above its lyrics
            new(new System.Windows.Int32Rect(0, 400, 1000, 200), 450, 530),
        ];
        var page = new OcrPage("1.png", null,
        [
            At(100, 20, 80, "나의 노래"),        // the title above the first staff
            At(100, 310, 350, "주님의 사랑은"),  // row 1 lyrics, under row 1's area
            At(100, 540, 580, "나는 주를"),      // row 2 lyrics, inside row 2's area
            At(100, 650, 690, "1절 2절"),        // the verse buttons under the last row
        ]);
        Assert.Equal(["주님의 사랑은", "나는 주를"], LyricsReader.RowLyrics(page, rows));
    }

    [Fact]
    public void LyricsJustBelowTheLastRowsAreaAreKeptAndAVerseButtonFarBelowIsNot()
    {
        // A render's rows that both end above their lyrics: staves about 95 high, lyrics about 1.4 staff heights under them.
        ScoreRow[] rows =
        [
            new(new System.Windows.Int32Rect(0, 85, 1920, 299), 214, 308),
            new(new System.Windows.Int32Rect(0, 410, 1920, 300), 539, 634),
        ];
        var page = new OcrPage("1.png", null,
        [
            At(273, 400, 482, "따스한 곁을"),   // row 1 lyrics, under row 1's area
            At(736, 725, 812, "주님 앞에"),     // row 2 lyrics, under the last row's area
            At(105, 990, 1030, "1절 2절"),     // a verse button far below the last staff
        ]);
        Assert.Equal(["따스한 곁을", "주님 앞에"], LyricsReader.RowLyrics(page, rows));
    }

    [Fact]
    public void FragmentsOfOneLineAreJoinedLeftToRightAndTwoLinesStayApart()
    {
        ScoreRow[] rows = [new(new System.Windows.Int32Rect(0, 100, 1400, 400), 150, 230)];
        var page = new OcrPage("1.png", null,
        [
            At(800, 296, 336, "르고"),           // read a little higher than the rest of its line
            At(100, 300, 340, "주자 비 강"),
            At(500, 306, 346, "같이 흐"),
            At(100, 380, 420, "주 손길 치료하네"), // the next line, one line height lower
        ]);
        Assert.Equal(["주자 비 강 같이 흐 르고\n주 손길 치료하네"], LyricsReader.RowLyrics(page, rows));
    }

    [Fact]
    public void ARenderedScoreSlideKeepsItsLyricsAndLeavesItsVerseButtonsOut()
    {
        // Lines and rows as read from a 1440x1080 render of a two-staff slide: the render's rows end above the
        // lyrics, and a third "row" is found over the verse buttons and the page number at the bottom.
        ScoreRow[] rows =
        [
            new(new System.Windows.Int32Rect(0, 87, 1440, 222), 180, 264),
            new(new System.Windows.Int32Rect(0, 446, 1440, 178), 495, 579),
            new(new System.Windows.Int32Rect(0, 923, 1440, 157), 1016, 1066),
        ];
        var page = new OcrPage("1.png", null,
        [
            At(193, 344, 420, "주님의 사랑은- 영원히"),
            At(781, 659, 734, "나는 주를"),
            At(85, 971, 1004, "1 세상 흔들리고"),
            At(441, 972, 1004, "- 2 믿음 흔들리고"),
            At(1213, 1015, 1050, "2/10"),
        ]);
        Assert.Equal(["주님의 사랑은 영원히", "나는 주를", ""], LyricsReader.RowLyrics(page, rows));
    }

    [Fact]
    public void ALineReadInPiecesUnderEachStaffIsPutBackInOrder()
    {
        // A render whose lyrics were read as two pieces per line at slightly different heights (the pieces' places as
        // read; the second piece starts nearer here, as this helper's words are narrower than the render's).
        ScoreRow[] rows =
        [
            new(new System.Windows.Int32Rect(0, 102, 1440, 283), 143, 230),
            new(new System.Windows.Int32Rect(0, 403, 1440, 283), 444, 531),
        ];
        var page = new OcrPage("1.png", null,
        [
            At(136, 303, 375, "주자 비 강 같 이 흐"),
            At(405, 608, 681, "주 손길 치"),
            At(800, 306, 367, "르 고"),
            At(892, 609, 681, "료 하네"),
        ]);
        Assert.Equal(["주자 비 강 같 이 흐 르 고", "주 손길 치 료 하네"], LyricsReader.RowLyrics(page, rows));
    }

    [Fact]
    public void AMarkReadInsideAStaffIsNotJoinedToTheLyricsUnderIt()
    {
        ScoreRow[] rows = [new(new System.Windows.Int32Rect(0, 100, 1000, 300), 150, 230)];
        var page = new OcrPage("1.png", null,
        [
            At(100, 198, 258, "리"),          // a note read as a letter, centered inside the staff
            At(300, 220, 280, "주님의 사랑"),  // the lyrics just under the staff, at nearly the same height
        ]);
        Assert.Equal(["주님의 사랑"], LyricsReader.RowLyrics(page, rows));
    }

    [Fact]
    public void ASectionNameAboveTheNextStaffStaysOutOfTheRowAbove()
    {
        ScoreRow[] rows =
        [
            new(new System.Windows.Int32Rect(0, 100, 1000, 200), 150, 230),
            new(new System.Windows.Int32Rect(0, 300, 1000, 300), 450, 530),
        ];
        var page = new OcrPage("1.png", null,
        [
            At(100, 250, 290, "주님의 사랑"),  // row 1 lyrics, inside row 1's area
            At(100, 300, 340, "후렴"),        // a section name over the next staff, within reach of the staff above
            At(100, 540, 580, "나는 주를"),    // row 2 lyrics
        ]);
        Assert.Equal(["주님의 사랑", "나는 주를"], LyricsReader.RowLyrics(page, rows));
    }

    [Fact]
    public void ALineInTwoRowsAreasGoesToTheStaffNearestAboveIt()
    {
        ScoreRow[] rows =
        [
            new(new System.Windows.Int32Rect(0, 100, 1000, 400), 150, 230), // its area reaches into the next row's
            new(new System.Windows.Int32Rect(0, 300, 1000, 400), 350, 430),
        ];
        var page = new OcrPage("1.png", null,
        [
            At(100, 240, 280, "주님의 사랑"),  // under staff 1 only
            At(100, 450, 490, "나는 주를"),    // in both areas, under staff 2
        ]);
        Assert.Equal(["주님의 사랑", "나는 주를"], LyricsReader.RowLyrics(page, rows));
    }

    [Fact]
    public void RowsGivenOutOfOrderGetTheirOwnLyricsBack()
    {
        ScoreRow[] rows =
        [
            new(new System.Windows.Int32Rect(0, 400, 1000, 200), 450, 530),
            new(new System.Windows.Int32Rect(0, 100, 1000, 200), 150, 230),
        ];
        var page = new OcrPage("1.png", null, [At(100, 250, 290, "주님의 사랑"), At(100, 540, 580, "나는 주를")]);
        Assert.Equal(["나는 주를", "주님의 사랑"], LyricsReader.RowLyrics(page, rows));
    }

    [Fact]
    public void TwoColumnsSideBySideStayTwoLines()
    {
        var page = new OcrPage("1.png", null, [At(100, 300, 340, "주 사랑"), At(900, 300, 340, "Lord's love")]);
        Assert.Equal(["주 사랑\nLord's love"], LyricsReader.RowLyrics(page, []));                     // a photo, no staves
        ScoreRow[] rows = [new(new System.Windows.Int32Rect(0, 100, 1400, 300), 150, 230)];
        Assert.Equal(["주 사랑\nLord's love"], LyricsReader.RowLyrics(page, rows));
    }

    [Fact]
    public void TwoVerseLinesCloseTogetherStayApart()
    {
        ScoreRow[] rows = [new(new System.Windows.Int32Rect(0, 100, 1000, 400), 150, 230)];
        var page = new OcrPage("1.png", null,
        [
            At(100, 325, 365, "2 나는 주를"),   // verse 2, a little more than half a line under verse 1
            At(100, 280, 320, "1 주님의 사랑"),
        ]);
        Assert.Equal(["1 주님의 사랑\n2 나는 주를"], LyricsReader.RowLyrics(page, rows));
    }

    [Theory]
    [InlineData(116, true)]  // 1.45 staff heights under the staff
    [InlineData(128, false)] // 1.6 staff heights under the staff
    public void ALineOutsideTheRowsIsLyricsOnlyWithinReachOfItsStaff(int below, bool kept)
    {
        ScoreRow[] rows = [new(new System.Windows.Int32Rect(0, 100, 1000, 160), 150, 230)]; // area ends at 260
        var page = new OcrPage("1.png", null, [At(100, 230 + below - 20, 230 + below + 20, "주님 앞에")]);
        Assert.Equal([kept ? "주님 앞에" : ""], LyricsReader.RowLyrics(page, rows));
    }

    [Fact]
    public void RecognizedLyricsBecomeABoxAndTheCaptionWhenChosen()
    {
        using var session = new CaptionSession(new LiveSessionService());
        session.Prepare(new CaptionDeck("deck.pptx", [new([new("label", "버튼", "1.나의 맘")], true), new([new("label", "버튼", "1.나의 맘")], true)]));
        session.Follow(1, CaptionSession.RecognizedKey);
        session.SetVisible(true);
        Assert.Equal("", session.PublicText);
        session.AddRecognized(1, "주가 기뻐하는");
        session.AddRecognized(1, "주가 기뻐하는\n주의 성전"); // read again: replaced, not added
        Assert.Equal("주가 기뻐하는\n주의 성전", session.PublicText);
        Assert.Single(session.Deck!.Slides[0].Boxes, box => box.Key == CaptionSession.RecognizedKey);
        Assert.Equal("1.나의 맘", session.TextFor(1, "label"));
    }

    [Fact]
    public void MirroringShowsTheOutputScreenTextEvenWhenNotLiveUntilTextIsTyped()
    {
        var live = new LiveSessionService(); // never goes live
        using var session = new CaptionSession(live);
        session.ShowScreenText("무시됨");
        Assert.Equal("", session.PublicText);
        session.Mirror(true);
        session.ShowScreenText("내 영혼아 찬양하라");
        Assert.Equal("", session.PublicText); // following the screen does not reveal a hidden caption
        session.SetVisible(true); // Show
        Assert.Equal("내 영혼아 찬양하라", session.PublicText);
        Assert.True(session.HasAudienceText);
        session.Publish("직접 입력");
        session.ShowScreenText("다른 가사");
        Assert.False(session.Mirroring);
        Assert.Equal("직접 입력", session.PublicText);
    }

    [Fact]
    public void DestinationsChooseCameraOnlyObsOnlyOrBoth() => StaHelper.RunOnSta(() =>
    {
        var directory = Directory.CreateTempSubdirectory();
        try
        {
            var path = Path.Combine(directory.FullName, "caption-style.json");
            using (var session = new CaptionSession(new LiveSessionService(), null, path))
            {
                var sent = new System.Collections.Generic.List<PublishedCaption>();
                session.PublishedChanged += _ => sent.Add(session.ForObs);
                session.Publish("자막");
                Assert.Equal(("자막", true), (session.ForObs.Text, session.HasAudienceText)); // both

                session.ObsEnabled = false; // camera only
                Assert.Equal(("", true), (session.ForObs.Text, session.HasAudienceText));
                Assert.Equal("", sent[^1].Text); // the OBS page is told to drop it

                session.ObsEnabled = true;
                session.AudienceEnabled = false; // OBS only
                Assert.Equal(("자막", false), (session.ForObs.Text, session.HasAudienceText));
                Assert.Equal("자막", sent[^1].Text);
                session.SyncShow = true; // kept with the destinations
                session.CaptionLines = 9; // clamped to 3
                Assert.Equal(3, session.CaptionLines);
                session.CaptionLines = 2;
                Assert.Equal(2, sent[^1].Lines); // the OBS page sizes its area from it
                Assert.Equal(.27, sent[^1].ScoreHeight); // the medium score size by default
                session.ScoreSize = 0;
                Assert.Equal(.18, sent[^1].ScoreHeight);
            }
            using var reopened = new CaptionSession(new LiveSessionService(), null, path);
            Assert.Equal((false, true, true, 2, 0), (reopened.AudienceEnabled, reopened.ObsEnabled, reopened.SyncShow, reopened.CaptionLines, reopened.ScoreSize));
        }
        finally { directory.Delete(true); }
    });

    // ---- a037: the operator console ----

    // Screens in pixels: a 1080p main screen at 100%.
    private static readonly CaptionScreen MainScreen = new(new(0, 0, 1920, 1080), new(0, 0, 1920, 1040), 1, true);

    [Fact]
    public void OnASmallScreenTheMinimumIsTheWorkArea()
    {
        var small = new CaptionScreen(new(0, 0, 1024, 768), new(0, 0, 1024, 708), 1.25, true); // 1024×768 at 125%
        Assert.Equal(new System.Windows.Size(819.2, 566.4), CaptionWindowPlacement.Minimum(new(900, 620), small));
        Assert.Equal(new System.Windows.Size(900, 620), CaptionWindowPlacement.Minimum(new(900, 620), MainScreen));
    }

    // A key pressed with the focus on target, as the window sees it before the control does.
    internal static bool Press(System.Windows.UIElement target, Key key)
    {
        var args = new KeyEventArgs(Keyboard.PrimaryDevice, System.Windows.PresentationSource.FromVisual(target), 0, key)
            { RoutedEvent = Keyboard.PreviewKeyDownEvent };
        target.RaiseEvent(args);
        return args.Handled;
    }

    [Fact]
    public Task SpaceShowsAndHidesTheCaptionOutsideTheTextBoxes() => WithConsole((console, session, _) =>
    {
        var editor = Assert.IsType<CaptionConsoleViewModel>(console.DataContext);
        Assert.False(Press(console.Draft, Key.Space)); // a space typed into the caption
        Assert.False(session.Visible);
        Assert.True(Press(console, Key.Space));
        Assert.True(session.Visible);
        Assert.True(Press(console, Key.Space));
        Assert.False(session.Visible);
        editor.IsComposing = true; // a Hangul syllable being put together
        Assert.False(Press(console, Key.Space));
        Assert.False(Press(console, Key.ImeProcessed));
        Assert.False(session.Visible);
        editor.IsComposing = false;
        Assert.True(Press(console, Key.Space));
        Assert.True(session.Visible);
        Assert.True(Press(console, Key.Escape));
        Assert.False(session.Visible);
        return Task.CompletedTask;
    });

    [Fact]
    public Task NumberKeysChooseTheDisplayOutsideTheTextBoxes() => WithConsole(async (console, session, _) =>
    {
        Assert.False(Press(console, Key.D2)); // typing: the display buttons are not there, nor are their keys
        Assert.Equal("auto", session.Display);
        console.ScreenMode.IsChecked = true;
        await console.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.ApplicationIdle);
        Assert.True(Press(console, Key.D2));
        Assert.Equal(("text", true), (session.Display, console.TextOnly.IsChecked));
        Assert.True(Press(console, Key.D3));
        Assert.Equal("score", session.Display);
        Assert.True(Press(console, Key.NumPad4));
        Assert.Equal("scoretext", session.Display);
        Assert.False(Press(console.Draft, Key.D1)); // a digit typed into a text box
        Assert.Equal("scoretext", session.Display);
        Assert.True(Press(console, Key.D1));
        Assert.Equal(("auto", true), (session.Display, console.AutoShow.IsChecked));
    });

    [Fact]
    public Task ArrowKeysStepTheFollowedDeck() => WithConsole(async (console, session, directory) =>
    {
        var path = CaptionAllTextTests.WriteDeck(directory.FullName, [],
            CaptionAllTextTests.TextSlide("일장"), CaptionAllTextTests.TextSlide("이장"), CaptionAllTextTests.TextSlide("삼장"));
        Assert.True(await console.PrepareDeckAsync(path, 1));
        console.ScreenMode.IsChecked = true;
        await console.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.ApplicationIdle);
        session.Follow(1, CaptionDeck.AllTextKey);
        Assert.True(Press(console, Key.Right));
        Assert.True(Press(console, Key.Down));
        Assert.Equal(3, session.CurrentSlide);
        Assert.True(Press(console, Key.Left));
        Assert.Equal(2, session.CurrentSlide);
        Assert.True(Press(console, Key.Up));
        Assert.Equal(1, session.CurrentSlide);
    });

    [Fact]
    public Task TheAirButtonShowsWhetherTheCaptionIsOnAir() => WithConsole(async (console, session, _) =>
    {
        Assert.Contains("숨김", (string)console.AirButton.Content); // a new console starts hidden
        await ClickAsync(console.AirButton);
        Assert.True(session.Visible);
        Assert.Contains("자막 나가는 중", (string)console.AirButton.Content);
        session.SetVisible(false); // hidden elsewhere (Esc, a new worship list)
        await console.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.ApplicationIdle);
        Assert.Contains("숨김", (string)console.AirButton.Content);
    });

    [Fact]
    public Task TheNextStepIsTheNextRowThenTheNextSlideAndNothingAtTheEnd() => WithConsole(async (console, session, directory) =>
    {
        var editor = Assert.IsType<CaptionConsoleViewModel>(console.DataContext);
        var path = CaptionAllTextTests.WriteDeck(directory.FullName, [CaptionScorePictureTests.ScorePng()],
            CaptionAllTextTests.ScoreSlide(0), CaptionAllTextTests.ScoreSlide(0));
        Assert.True(await console.PrepareDeckAsync(path, 1));
        await console.ScorePreparation;
        await ClickAsync(console.AirButton);
        console.ScreenMode.IsChecked = true;
        console.ScoreOnly.IsChecked = true;
        session.Follow(1, CaptionDeck.AllTextKey);
        Assert.Contains("1/2줄", console.ScoreRowLabel.Text);
        Assert.Equal(("1장 1줄", "1장 2줄"), (editor.CurrentStep?.Label, editor.NextStep?.Label)); // the same slide's second row
        typeof(CaptionConsole).GetMethod("Next", Private)!.Invoke(console, [null, null]);
        Assert.Equal(("1장 2줄", "2장 1줄"), (editor.CurrentStep?.Label, editor.NextStep?.Label));
        Assert.Same(editor.Steps[2], editor.NextStep); // the strip's own step (marked after a next, not only the first time)
        typeof(CaptionConsole).GetMethod("Next", Private)!.Invoke(console, [null, null]);
        Assert.Equal("2장 2줄", editor.NextStep?.Label);
        typeof(CaptionConsole).GetMethod("Next", Private)!.Invoke(console, [null, null]);
        Assert.Contains("2/2줄", console.ScoreRowLabel.Text);
        Assert.Null(editor.NextStep); // the end
    });

    [Fact]
    public Task WhileTheScoreIsMadeTheNextStepIsTheNextSlide() => WithConsole(async (console, session, directory) =>
    {
        var editor = Assert.IsType<CaptionConsoleViewModel>(console.DataContext);
        var path = CaptionAllTextTests.WriteDeck(directory.FullName, [CaptionScorePictureTests.ScorePng()],
            CaptionAllTextTests.TextSlide("설교 제목"), CaptionAllTextTests.ScoreSlide(0), CaptionAllTextTests.ScoreSlide(0));
        Assert.True(await console.PrepareDeckAsync(path, 1));
        await console.ScorePreparation;
        console.ScreenMode.IsChecked = true;
        typeof(CaptionConsole).GetField("_scoreDeck", Private)!.SetValue(console, null);
        typeof(CaptionConsole).GetField("_preparingScore", Private)!.SetValue(console, session.Deck);
        session.Follow(2, CaptionDeck.AllTextKey);
        Assert.Equal("3장", editor.NextStep?.Label); // no row of slide 2 is on air
    });

    [Fact]
    public Task PickingAStepInTheStripMovesOnlyTheCaption() => WithConsole(async (console, session, directory) =>
    {
        var editor = Assert.IsType<CaptionConsoleViewModel>(console.DataContext);
        var path = CaptionAllTextTests.WriteDeck(directory.FullName, [],
            CaptionAllTextTests.TextSlide("일장", "둘째 줄"), CaptionAllTextTests.TextSlide("이장"), CaptionAllTextTests.TextSlide("삼장"));
        Assert.True(await console.PrepareDeckAsync(path, 1));
        var main = (MainViewModel)typeof(CaptionConsole).GetField("_main", Private)!.GetValue(console)!;
        int screenChanges = 0;
        main.Session.SessionChanged += (_, _) => screenChanges++;
        await ClickAsync(console.AirButton);
        Assert.False(console.StepStrip.IsVisible); // typing: no deck followed
        console.ScreenMode.IsChecked = true;
        session.SyncShow = true; // even with sync on, a step picked moves the caption only
        session.Follow(1, CaptionDeck.AllTextKey);
        await console.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.ApplicationIdle);
        Assert.True(console.StepStrip.IsVisible);
        Assert.Equal(["1장", "2장", "3장"], editor.Steps.Select(step => step.Label));
        Assert.Equal("일장", editor.Steps[0].Text); // the first line
        Assert.Equal(editor.Steps[0], editor.CurrentStep);
        console.StepStrip.SelectedItem = editor.Steps[2];
        Assert.Equal((3, "삼장"), (session.CurrentSlide, session.PublicText));
        Assert.Equal(editor.Steps[2], editor.CurrentStep);
        Assert.DoesNotContain("송출 화면", console.Status.Text);
        Assert.Equal(0, screenChanges);
        session.SyncShow = false;
    });

    [Fact]
    public Task AScoreStepGoesToItsRowInOneCaption() => WithConsole(async (console, session, directory) =>
    {
        var editor = Assert.IsType<CaptionConsoleViewModel>(console.DataContext);
        var path = CaptionAllTextTests.WriteDeck(directory.FullName, [CaptionScorePictureTests.ScorePng()],
            CaptionAllTextTests.ScoreSlide(0), CaptionAllTextTests.ScoreSlide(0));
        console.ReadLyrics = (files, _) => Task.FromResult(ReadLyrics(files)); // each row its own line of lyrics
        Assert.True(await console.PrepareDeckAsync(path, 1));
        await console.ScorePreparation;
        await console.LyricsReading;
        await ClickAsync(console.AirButton);
        console.ScreenMode.IsChecked = true;
        console.ScoreAndText.IsChecked = true;
        session.Follow(1, CaptionSession.RecognizedKey);
        Assert.Equal("1장 첫 줄", session.PublicText);
        Assert.Equal(["1장 1줄", "1장 2줄", "2장 1줄", "2장 2줄"], editor.Steps.Select(step => step.Label));
        Assert.All(editor.Steps, step => Assert.NotNull(step.Thumbnail));
        var sent = new List<string>();
        session.PublishedChanged += published => sent.Add(published.Text);
        console.StepStrip.SelectedItem = editor.Steps[3];
        Assert.Equal(2, session.CurrentSlide);
        Assert.Contains("2/2줄", console.ScoreRowLabel.Text);
        Assert.Equal(["2장 둘째 줄"], sent); // no flash of the slide's first row
        Assert.Equal(editor.Steps[3], editor.CurrentStep);
    });

    [Fact]
    public Task ConnectionAndDeviceSettingsAreInTheirOwnWindow() => WithConsole(async (console, _, _) =>
    {
        string[] moved = ["Host", "Port", "Password", "AutoObs", "AddObsSource", "CameraSource", "PreviewObsCamera", "Scenes", "SourceName",
            "Scene1", "Scene2", "Scene3", "Scene4", "RecoverButton", "Slides", "Boxes", "Cue"];
        foreach (var name in moved)
        {
            Assert.Null(console.FindName(name));
            Assert.NotNull(console.Settings.FindName(name));
        }
        Assert.Empty(Descendants(console).OfType<PasswordBox>());
        foreach (var name in new[] { "LinesBox", "ScoreSizeBox", "BoxCheck", "ToCamera", "ToObs", "ObsLabel", "SyncShow", "StepStrip" })
            Assert.NotNull(console.FindName(name));
        Assert.NotNull(console.Strip.FindName("AirButton")); // on the tab strip
        Assert.False(console.Settings.IsVisible);
        await ClickAsync(console.SettingsButton); // not modal: this returns with the window open
        Assert.True(console.Settings.IsVisible);
        Assert.Same(Window.GetWindow(console), console.Settings.Owner); // the main window
        console.Settings.Close(); // hidden, kept with what was typed in it
        Assert.False(console.Settings.IsVisible);
        await ClickAsync(console.SettingsButton);
        Assert.True(console.Settings.IsVisible);
    });

    [Fact]
    public Task EveryControlOfTheConsoleAndItsSettingsHasItsOwnName() => WithConsole(async (console, session, directory) =>
    {
        // A followed deck, so the strip has steps (their containers are made for the list, not in the window's tree).
        var path = CaptionAllTextTests.WriteDeck(directory.FullName, [],
            CaptionAllTextTests.TextSlide("일장"), CaptionAllTextTests.TextSlide("이장"), CaptionAllTextTests.TextSlide("삼장"));
        Assert.True(await console.PrepareDeckAsync(path, 1));
        console.ScreenMode.IsChecked = true;
        session.Follow(1, CaptionDeck.AllTextKey);
        await ClickAsync(console.SettingsButton);
        console.StepStrip.UpdateLayout();
        var steps = console.StepStrip.Items.Cast<object>().Select(item => console.StepStrip.ItemContainerGenerator.ContainerFromItem(item))
            .OfType<Control>().ToList();
        Assert.Equal(3, steps.Count);
        // The tab with its strip (in the window it is in), and the settings window.
        foreach (var window in new System.Windows.Window[] { Window.GetWindow(console), console.Settings })
        {
            var names = Descendants(window).OfType<Control>()
                .Where(c => c is ButtonBase or TextBox or PasswordBox or ComboBox or ListBox)
                .Concat(window != console.Settings ? steps : [])
                .Select(c => (c, Name: AccessibleName(c))).ToList();
            Assert.All(names, n => Assert.False(string.IsNullOrWhiteSpace(n.Name), $"{window.GetType().Name}: {n.c.GetType().Name} {n.c.Name} has no name"));
            var twice = names.GroupBy(n => n.Name).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
            Assert.True(twice.Count == 0, $"{window.GetType().Name}: the same name twice: {string.Join(", ", twice)}");
        }
    });

    [Fact]
    public Task WhileHiddenThePreviewShowsTheTypedCaptionDimAndNothingGoesOut() => WithConsole(async (console, session, _) =>
    {
        console.Draft.SetCurrentValue(TextBox.TextProperty, "주님은 나의 목자"); // typed as it goes (the default)
        await console.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.ApplicationIdle);
        Assert.False(session.Visible);
        Assert.Equal(("", null, "", false), (session.Published.Text, session.Published.Image, session.ForObs.Text, session.HasAudienceText));
        Assert.Equal(Visibility.Visible, console.HeldCaption.Visibility);
        Assert.Equal("주님은 나의 목자", console.HeldCaption.LowerThirdText.Text.Replace("\u2060", ""));
        Assert.Equal(0.5, console.HeldCaption.Opacity);
        Assert.Equal(Visibility.Collapsed, console.PreviewCaption.Visibility);
        Assert.True(console.HiddenBadge.IsVisible);
        await ClickAsync(console.AirButton); // on air: the preview is what goes out
        Assert.Equal("주님은 나의 목자", session.PublicText);
        Assert.Equal(Visibility.Collapsed, console.HeldCaption.Visibility);
        Assert.Equal(Visibility.Visible, console.PreviewCaption.Visibility);
        Assert.False(console.HiddenBadge.IsVisible);
    });

    [Fact]
    public Task WhileHiddenThePreviewShowsTheFollowedScoreRow() => WithConsole(async (console, session, directory) =>
    {
        var path = CaptionAllTextTests.WriteDeck(directory.FullName, [CaptionScorePictureTests.ScorePng()],
            CaptionAllTextTests.ScoreSlide(0), CaptionAllTextTests.ScoreSlide(0));
        Assert.True(await console.PrepareDeckAsync(path, 1));
        await console.ScorePreparation;
        console.ScreenMode.IsChecked = true;
        console.ScoreOnly.IsChecked = true;
        session.Follow(1, CaptionDeck.AllTextKey);
        Assert.Contains("1/2줄", console.ScoreRowLabel.Text);
        Assert.Equal((null, "", false), (session.Published.Image, session.ForObs.Text, session.HasAudienceText)); // nothing goes out
        Assert.NotNull(session.ReadyImage);
        Assert.Same(session.ReadyImage, console.HeldCaption.LowerThirdImage.Source);
        Assert.Equal(Visibility.Visible, console.HeldCaption.Visibility);
    });

    // The main window at its least (780×540): a low preview column. The tab scrolls; nothing is cut, the steps keep a row.
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public Task InALowColumnTheTabScrollsAndKeepsARowOfSteps(bool typing) => WithConsole(async (console, _, _) =>
    {
        if (!typing) console.ScreenMode.IsChecked = true;
        var host = Window.GetWindow(console);
        (host.Width, host.Height) = (320, 420);
        await console.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.ApplicationIdle);
        await console.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.ApplicationIdle); // (the column's least height, then its layout)
        Assert.True(console.Body.ScrollableHeight > 0, "scrolled, not cut");
        Assert.True(console.Column.ActualHeight >= console.Column.MinHeight - 0.5);
        if (typing) Assert.True(console.Draft.ActualHeight >= console.Draft.MinHeight - 0.5 && console.Draft.MinHeight > 40, $"the box {console.Draft.ActualHeight}");
        else Assert.True(console.StepStrip.ActualHeight >= 73.5, $"the steps {console.StepStrip.ActualHeight}");
        // A tall column gives the height left to the steps or the box, and does not scroll.
        (host.Width, host.Height) = (Column.Width, Column.Height);
        await console.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.ApplicationIdle);
        Assert.Equal(0, console.Body.ScrollableHeight);
        Assert.True((typing ? console.Draft.ActualHeight : console.StepStrip.ActualHeight) > 150);
    });

    // The name a screen reader reads: the control's own, else its label's (LabeledBy).
    private static string AccessibleName(System.Windows.UIElement control)
    {
        var name = UIElementAutomationPeer.CreatePeerForElement(control)?.GetName() ?? "";
        return name.Length == 0 && System.Windows.Automation.AutomationProperties.GetLabeledBy(control) is { } label
            ? UIElementAutomationPeer.CreatePeerForElement(label)?.GetName() ?? "" : name;
    }

    // The logical tree: collapsed panels too.
    private static IEnumerable<System.Windows.DependencyObject> Descendants(System.Windows.DependencyObject parent)
    {
        foreach (var child in System.Windows.LogicalTreeHelper.GetChildren(parent).OfType<System.Windows.DependencyObject>())
        {
            yield return child;
            foreach (var below in Descendants(child)) yield return below;
        }
    }

    internal static async Task ClickAsync(Button button)
    {
        ((IInvokeProvider)new ButtonAutomationPeer(button)).Invoke();
        await button.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.ApplicationIdle);
    }

    // The window the tab is in (the main window, or the test's stand-in): its focus scope.
    internal static IInputElement? Focused(CaptionConsole console) => FocusManager.GetFocusedElement(Window.GetWindow(console));

    internal static void Focus(CaptionConsole console, IInputElement element) => FocusManager.SetFocusedElement(Window.GetWindow(console), element);

    // The preview column of a maximized 1080p main window: where the caption tab is.
    internal static readonly Size Column = new(720, 960);

    // savedStyle: caption-style.json as kept from an earlier session.
    // openSettings: the settings window is open (shown, owned by the main window) while the exercise runs; show: false,
    // the tab is made in its stand-in window but not shown; appLook: the app's resources (only where no other test made
    // an Application); screens: the work areas the settings window fits in (a 1080p screen unless given).
    // live: a live session of its own (the main window does not see it; nothing renders a deck), with the native show.
    // The tab is in a stand-in for the main window: the tab strip over it, the size of the preview column.
    internal static Task WithConsole(Func<CaptionConsole, CaptionSession, DirectoryInfo, Task> exercise,
        string? expectedSavedDraft = null, string? savedStyle = null, IPowerPointRenderService? render = null,
        bool openSettings = false, bool show = true, bool appLook = false,
        Func<IReadOnlyList<CaptionScreen>>? screens = null, LiveSessionService? live = null, IPowerPointSlideShowControl? nativeShow = null,
        bool asOpened = false) => StaHelper.RunOnStaAsync(async () =>
    {
        var directory = Directory.CreateTempSubdirectory();
        try
        {
            if (savedStyle != null) File.WriteAllText(Path.Combine(directory.FullName, "caption-style.json"), savedStyle);
            if (appLook && System.Windows.Application.Current == null) new App().InitializeComponent(); // the app's theme and control styles
            var registrations = new ServiceCollection();
            App.ConfigureServices(registrations,
                new SettingsServiceOptions(Path.Combine(directory.FullName, "settings.json"), directory.FullName),
                Dispatcher.CurrentDispatcher);
            using var services = registrations.BuildServiceProvider();
            var main = services.GetRequiredService<MainViewModel>();
            using var own = live == null ? null : new CaptionSession(live, nativeShow, Path.Combine(directory.FullName, "caption-style.json"));
            var session = own ?? services.GetRequiredService<CaptionSession>();
            var store = services.GetRequiredService<CaptionRecoveryStore>();
            var console = new CaptionConsole(main, session, services.GetRequiredService<CaptionBroadcastServer>(), store,
                services.GetRequiredService<CameraOwnership>(), services.GetRequiredService<ObsOperator>(),
                new OperationalErrorLog(directory.FullName), render, screens ?? (() => [MainScreen]))
            {
                Songs = _ => Task.FromResult<IReadOnlyList<SongLyrics.Song>>([]), // no song database (a test gives its songs)
            };
            DockPanel.SetDock(console.Strip, Dock.Top);
            var host = new Window
            {
                ShowActivated = false, ShowInTaskbar = false, Width = Column.Width, Height = Column.Height,
                Content = new DockPanel { Children = { console.Strip, console } },
            };
            // The tests start typing (they pick following themselves) unless they check how the tab opens.
            if (!asOpened) console.DirectMode.IsChecked = true;
            try
            {
                if (show) host.Show();
                await console.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.ApplicationIdle);
                if (openSettings)
                {
                    await ClickAsync(console.SettingsButton);
                    Assert.True(console.Settings.IsVisible);
                }
                await exercise(console, session, directory);
            }
            finally
            {
                console.EndSession();
                host.Close();
            }
            if (expectedSavedDraft != null)
            {
                store.Dispose(); // Flush the last queued recovery write.
                Assert.Equal(expectedSavedDraft, store.Read(main.CurrentWorshipListName)!.Draft);
            }
        }
        finally { directory.Delete(true); }
    });
}
