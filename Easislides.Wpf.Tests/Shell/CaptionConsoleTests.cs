using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Easislides.Wpf.Captions;
using Easislides.Wpf.Media;
using Easislides.Wpf.Settings;
using Easislides.Wpf.Shell;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Easislides.Wpf.Tests.Shell;

public sealed class CaptionConsoleTests
{
    [Fact]
    public Task WindowPublishesOnlyOnTakeAndSuppressesImeComposition() => StaHelper.RunOnStaAsync(async () =>
    {
        var dir = Directory.CreateTempSubdirectory();
        try
        {
            var registrations = new ServiceCollection();
            App.ConfigureServices(registrations, new SettingsServiceOptions(Path.Combine(dir.FullName,"settings.json"),dir.FullName),
                Dispatcher.CurrentDispatcher);
            using var services = registrations.BuildServiceProvider();
            var main = services.GetRequiredService<MainViewModel>();
            var state = services.GetRequiredService<CaptionSession>();
            var store = services.GetRequiredService<CaptionRecoveryStore>();
            var server = services.GetRequiredService<CaptionBroadcastServer>();
            var console = new CaptionConsole(main, state, server, store, services.GetRequiredService<CameraOwnership>(), services.GetRequiredService<ObsOperator>())
                { ShowActivated = false, ShowInTaskbar = false };
            try
            {
                console.Show();
                await console.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.ApplicationIdle);
                console.Draft.Text = "첫 번째 문장";
                Assert.Equal("", state.PublicText);
                console.TakeButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                Assert.Equal("첫 번째 문장", state.PublicText);
                console.Instant.IsChecked = true;
                var keyboard = InputManager.Current.PrimaryKeyboardDevice;
                var composition = new TextComposition(InputManager.Current, console.Draft, "한");
                console.Draft.RaiseEvent(new TextCompositionEventArgs(keyboard, composition)
                    { RoutedEvent = TextCompositionManager.PreviewTextInputStartEvent });
                console.Draft.Text = "ㅎ";
                Assert.Equal("첫 번째 문장", state.PublicText);
                console.Draft.Text = "한";
                console.Draft.RaiseEvent(new TextCompositionEventArgs(keyboard, composition)
                    { RoutedEvent = TextCompositionManager.TextInputEvent, Handled = true });
                await console.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.ApplicationIdle);
                Assert.Equal("한",state.PublicText);
                console.Close();
                Assert.False(console.IsVisible);
                Assert.Equal("한",state.PublicText);
                console.Show();
                Assert.Equal("한",console.Draft.Text);
            }
            finally { console.EndSession(); }
            store.Dispose();
            Assert.Equal("한",store.Read(main.CurrentWorshipListName)!.Draft);
        }
        finally { dir.Delete(true); }
    });
}
