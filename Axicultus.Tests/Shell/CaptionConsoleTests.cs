using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Axicultus.Wpf.Captions;
using Axicultus.Wpf.Media;
using Axicultus.Wpf.Settings;
using Axicultus.Wpf.Shell;
using Axicultus.Wpf.Support;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Axicultus.Wpf.Tests.Shell;

public sealed class CaptionConsoleTests
{
    [Fact]
    public Task WindowPublishesOnlyOnTakeAndSuppressesImeComposition() => WithConsole(async (console, session, _) =>
    {
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
        console.Draft.SetCurrentValue(TextBox.TextProperty, "송출 중");
        editor.TakeFromShortcut(isRepeat: false);
        console.Instant.SetCurrentValue(CheckBox.IsCheckedProperty, true);
        await ClickAsync(console.HideButton);
        console.Draft.SetCurrentValue(TextBox.TextProperty, "대기 중");
        editor.TakeFromShortcut(isRepeat: true);
        Assert.Equal("", session.PublicText);
        Assert.False(console.Instant.IsChecked);
        Assert.Equal("송출 중", Assert.Single(session.History).Text);
        await ClickAsync(console.ShowButton);
        Assert.Equal("송출 중", session.PublicText);
        console.Close();
        Assert.False(console.IsVisible);
        Assert.Equal("송출 중", session.PublicText);
        console.Show();
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
            Assert.Contains("자막 PPT 열기", log);
            if (!corruptFile) Assert.Contains(nameof(FileNotFoundException), log);
        });

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

    private static async Task ClickAsync(Button button)
    {
        ((IInvokeProvider)new ButtonAutomationPeer(button)).Invoke();
        await button.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.ApplicationIdle);
    }

    private static Task WithConsole(Func<CaptionConsole, CaptionSession, DirectoryInfo, Task> exercise,
        string? expectedSavedDraft = null) => StaHelper.RunOnStaAsync(async () =>
    {
        var directory = Directory.CreateTempSubdirectory();
        try
        {
            var registrations = new ServiceCollection();
            App.ConfigureServices(registrations,
                new SettingsServiceOptions(Path.Combine(directory.FullName, "settings.json"), directory.FullName),
                Dispatcher.CurrentDispatcher);
            using var services = registrations.BuildServiceProvider();
            var main = services.GetRequiredService<MainViewModel>();
            var session = services.GetRequiredService<CaptionSession>();
            var store = services.GetRequiredService<CaptionRecoveryStore>();
            var console = new CaptionConsole(main, session, services.GetRequiredService<CaptionBroadcastServer>(), store,
                services.GetRequiredService<CameraOwnership>(), services.GetRequiredService<ObsOperator>(),
                new OperationalErrorLog(directory.FullName)) { ShowActivated = false, ShowInTaskbar = false };
            try
            {
                console.Show();
                await console.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.ApplicationIdle);
                await exercise(console, session, directory);
            }
            finally { console.EndSession(); }
            if (expectedSavedDraft != null)
            {
                store.Dispose(); // Flush the last queued recovery write.
                Assert.Equal(expectedSavedDraft, store.Read(main.CurrentWorshipListName)!.Draft);
            }
        }
        finally { directory.Delete(true); }
    });
}
