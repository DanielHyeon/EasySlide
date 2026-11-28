using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Axicultus.Wpf.Captions;
using Axicultus.Wpf.Shell;
using Xunit;

namespace Axicultus.Wpf.Tests.Shell;

/// <summary>
/// a048 design 4, 5, 7: the caption and OBS settings files read an empty, missing or malformed value as its default
/// (a file that cannot be read at all as the default settings), and the recovery of the worship order keeps a song's
/// chords and capo.
/// </summary>
public sealed class CaptionSettingsFileTests
{
    [Theory]
    [InlineData("""{"Color":null,"Box":true}""", CaptionStyle.DefaultColor, true, 3, 1, 0d)] // the spec's scenario
    [InlineData("""{"Color":null,"Box":false,"Lines":2}""", CaptionStyle.DefaultColor, false, 2, 1, 0d)]
    [InlineData("""{"Color":"blue","Box":false}""", CaptionStyle.DefaultColor, false, 3, 1, 0d)]
    [InlineData("""{"Color":"#ffe14d","Lines":9,"Score":-3,"Preview":-1}""", "#FFE14D", true, 3, 0, 0d)]
    [InlineData("""{"Color":"#FFE14D","Display":null,"Preview":240}""", "#FFE14D", true, 3, 1, 240d)]
    // a048 design 12: one bad item is that item's default only; the others keep the file's values.
    [InlineData("""{"Color":"#FFE14D","Box":null,"Lines":2}""", "#FFE14D", true, 2, 1, 0d)] // the spec's scenario
    [InlineData("""{"Color":"#FFE14D","Box":false,"Lines":null}""", "#FFE14D", false, 3, 1, 0d)]
    [InlineData("""{"Color":"#FFE14D","Box":false,"Lines":"2"}""", "#FFE14D", false, 3, 1, 0d)]
    [InlineData("""{"Color":"#FFE14D","Box":false,"Preview":1e400}""", "#FFE14D", false, 3, 1, 0d)]
    [InlineData("""{"Color":"#FFE14D","Box":false,"Score":99999999999}""", "#FFE14D", false, 3, 1, 0d)]
    [InlineData("""{"Color":"#FFE14D","Box":false,"Display":5,"Preview":240}""", "#FFE14D", false, 3, 1, 240d)]
    [InlineData("""{"Color":42,"Box":false,"Lines":2}""", CaptionStyle.DefaultColor, false, 2, 1, 0d)]
    [InlineData("""{"Color":{"r":1},"Box":false,"Lines":2,"Obs":[]}""", CaptionStyle.DefaultColor, false, 2, 1, 0d)]
    // a048 design 15: an item named twice is its last value (as a plain reading had it); the others are kept.
    [InlineData("""{"Color":"#FFE14D","Box":false,"Color":"#00FF00"}""", "#00FF00", false, 3, 1, 0d)]
    [InlineData("""{"Color":"#FFE14D","Lines":2,"Lines":1,"Box":false}""", "#FFE14D", false, 1, 1, 0d)]
    [InlineData("not json at all", CaptionStyle.DefaultColor, true, 3, 1, 0d)]
    [InlineData("""[1,2]""", CaptionStyle.DefaultColor, true, 3, 1, 0d)]
    [InlineData("null", CaptionStyle.DefaultColor, true, 3, 1, 0d)]
    [InlineData("", CaptionStyle.DefaultColor, true, 3, 1, 0d)]
    public void ACaptionStyleFileWithAnEmptyOrBadValueStartsTheSessionWithItsDefault(string saved, string color, bool box, int lines, int score, double preview)
    {
        var directory = Directory.CreateTempSubdirectory();
        try
        {
            var path = Path.Combine(directory.FullName, "caption-style.json");
            File.WriteAllText(path, saved);
            using (var session = new CaptionSession(new LiveSessionService().ForCaptions(), null, path))
            {
                Assert.Equal((color, box, lines, score, preview, CaptionStyle.AutoDisplay),
                    (session.Style.Color, session.Style.Box, session.Style.Lines, session.Style.Score, session.Style.Preview, session.Style.Display));
                Assert.NotNull(session.CaptionForeground); // the look can be drawn
            }
            Assert.Equal(saved, File.ReadAllText(path)); // the file is not written back
        }
        finally { directory.Delete(true); }
    }

    // A look changed after a file with one bad item keeps the file's other items (they were not lost to the defaults).
    [Fact]
    public void ALookChangedAfterABadItemKeepsTheOtherItems()
    {
        var directory = Directory.CreateTempSubdirectory();
        try
        {
            var path = Path.Combine(directory.FullName, "caption-style.json");
            File.WriteAllText(path, """{"Color":"#FFE14D","Box":null,"Lines":2,"Sync":true}""");
            using (var session = new CaptionSession(new LiveSessionService().ForCaptions(), null, path))
                session.SetStyle(session.Style with { Box = false });
            using var saved = JsonDocument.Parse(File.ReadAllText(path));
            Assert.Equal(("#FFE14D", false, 2, true), (saved.RootElement.GetProperty("Color").GetString(), saved.RootElement.GetProperty("Box").GetBoolean(),
                saved.RootElement.GetProperty("Lines").GetInt32(), saved.RootElement.GetProperty("Sync").GetBoolean()));
        }
        finally { directory.Delete(true); }
    }

    [Fact]
    public void AStyleWithoutAColorIsTheDefaultColor()
    {
        Assert.Equal(CaptionStyle.DefaultColor, CaptionStyle.Parse(null, true).Color);
        Assert.Equal(CaptionStyle.DefaultColor, CaptionStyle.Parse("", true).Color);
        Assert.Equal("#ABCDEF", CaptionStyle.Parse("#abcdef", true).Color);
    }

    private static string WriteObs(DirectoryInfo directory, string profile)
    {
        var path = Path.Combine(directory.FullName, "obs.json");
        File.WriteAllText(path, profile);
        return path;
    }

    [Theory]
    [InlineData("""{"Port":4455,"AutoConnect":false,"Scenes":["설교",null,"",null]}""")]                 // no Secret (the spec's scenario)
    [InlineData("""{"Port":4455,"AutoConnect":false,"Secret":null,"Scenes":["설교",null,"",null]}""")]   // Secret: null
    public Task AnObsProfileWithoutASecretHasNoSavedPasswordAndEmptySceneNames(string profile) => StaHelper.RunOnStaAsync(async () =>
    {
        var directory = Directory.CreateTempSubdirectory();
        try
        {
            WriteObs(directory, profile);
            using var obs = new ObsOperator(directory.FullName, Dispatcher.CurrentDispatcher);
            await obs.StartAsync();
            Assert.False(obs.HasSavedPassword);
            Assert.Equal(new[] { "설교", "", "", "" }, obs.Shortcuts);
            Assert.Equal(CaptionText.Get("CaptionConsole", "NotConnected"), obs.Status.Message); // the usual notice, no error
            Assert.Null(obs.Connection);
        }
        finally { directory.Delete(true); }
    });

    [Fact]
    public Task AnObsProfileWithANullSecretStillConnectsByItself() => StaHelper.RunOnStaAsync(async () =>
    {
        var directory = Directory.CreateTempSubdirectory();
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        int port = ((IPEndPoint)listener.LocalEndpoint).Port;
        try
        {
            WriteObs(directory, $$"""{"Port":{{port}},"AutoConnect":true,"Secret":null,"Scenes":["","","",""]}""");
            using var obs = new ObsOperator(directory.FullName, Dispatcher.CurrentDispatcher);
            await obs.StartAsync();
            Assert.False(obs.HasSavedPassword);
            Assert.NotNull(obs.Connection); // connecting without a password, as with an empty saved one
            Assert.Equal(CaptionText.Get("CaptionConsole", "Connecting"), obs.Status.Message);
        }
        finally { directory.Delete(true); }
    });

    // A password saved as this PC's user saves it (obs.json's Secret).
    internal static string Secret(string password) =>
        Convert.ToBase64String(ProtectedData.Protect(Encoding.UTF8.GetBytes(password), null, DataProtectionScope.CurrentUser));

    // a048 design 12 (the spec's scenario of an OBS profile without a port): a missing or bad item is that item's default; the host, the
    // saved password and the camera are still the file's.
    [Theory]
    [InlineData("""{"Host":"192.0.2.10","Secret":"@","Camera":"cam"}""")]                                       // no Port, Scenes, AutoConnect
    [InlineData("""{"Host":"192.0.2.10","Port":"4455x","AutoConnect":false,"Secret":"@","Scenes":null,"Camera":"cam"}""")]
    [InlineData("""{"Host":"192.0.2.10","Port":0,"AutoConnect":false,"Secret":"@","Scenes":["a"],"Camera":"cam"}""")]
    [InlineData("""{"Host":"192.0.2.10","Port":70000,"AutoConnect":null,"Secret":"@","Scenes":["","","",""],"Camera":"cam"}""")]
    [InlineData("""{"Host":"192.0.2.10","Port":99999999999,"AutoConnect":"yes","Secret":"@","Scenes":[1,2,3,4],"Camera":"cam"}""")]
    // a048 design 15: an item named twice is its last value; the others are kept
    [InlineData("""{"Host":"192.0.2.99","Port":4455,"Secret":"@","Host":"192.0.2.10","Camera":"cam"}""")]
    [InlineData("""{"Host":"192.0.2.10","Port":5000,"Secret":"@","Camera":"cam","Port":4455}""")]
    public Task AnObsProfileWithAMissingOrBadItemKeepsItsOtherItems(string profile) => StaHelper.RunOnStaAsync(async () =>
    {
        var directory = Directory.CreateTempSubdirectory();
        try
        {
            WriteObs(directory, profile.Replace("\"@\"", "\"" + Secret("operator-obs-password") + "\""));
            using var obs = new ObsOperator(directory.FullName, Dispatcher.CurrentDispatcher);
            await obs.StartAsync();
            Assert.Equal(("192.0.2.10", 4455, "cam", false, true), (obs.Host, obs.Port, obs.CameraSource, obs.AutoConnect, obs.HasSavedPassword));
            Assert.Equal(new[] { "", "", "", "" }, obs.Shortcuts);
            Assert.Equal(CaptionText.Get("CaptionConsole", "NotConnected"), obs.Status.Message);
            Assert.Null(obs.Connection);
        }
        finally { directory.Delete(true); }
    });

    // a048 design 13 (the spec's scenario of an OBS profile from another PC): a saved password from another PC (or another Windows user)
    // cannot be decrypted: it is no saved password, there is no automatic connection and the usual notice shows; a password
    // typed by the operator connects and is saved anew.
    [Theory]
    [InlineData("AAAAAAAA")]     // base64, not this user's protected data
    [InlineData("not base64!")]
    public Task AnObsSecretThatCannotBeDecryptedIsNoSavedPassword(string secret) => StaHelper.RunOnStaAsync(async () =>
    {
        var directory = Directory.CreateTempSubdirectory();
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        int port = ((IPEndPoint)listener.LocalEndpoint).Port;
        try
        {
            var path = WriteObs(directory, $$"""{"Port":{{port}},"AutoConnect":true,"Secret":"{{secret}}","Scenes":["","","",""]}""");
            using var obs = new ObsOperator(directory.FullName, Dispatcher.CurrentDispatcher);
            await obs.StartAsync();
            Assert.Equal(port, obs.Port);
            Assert.False(obs.HasSavedPassword);
            Assert.Null(obs.Connection); // no automatic connection
            Assert.Equal(CaptionText.Get("CaptionConsole", "NotConnected"), obs.Status.Message);
            await obs.ConfigureAsync("", port, "typed-password", true, ["", "", "", ""]);
            Assert.True(obs.HasSavedPassword);
            Assert.NotNull(obs.Connection);
            Assert.DoesNotContain(secret, File.ReadAllText(path));
        }
        finally { directory.Delete(true); }
    });

    [Fact]
    public void TheRecoveryKeepsASongsChordsAndCapo()
    {
        var song = new LiveQueueItem("song:7", "주 하나님 지으신 모든 세계", LiveItemKinds.Song)
        {
            Lyrics = "[1]\n주 하나님 지으신 모든 세계", Capo = 2, Notations = "(1;G;0;)(1;D;4;)", SourceFolderNo = 3,
        };
        var row = MainViewModelCaptionHost.ToRecovery(song);
        Assert.True(MainViewModelCaptionHost.IsRecoverableItem(row));
        var back = MainViewModelCaptionHost.FromRecovery(row);
        Assert.Equal((2, "(1;G;0;)(1;D;4;)"), (back.Capo, back.Notations));
        Assert.Equal((song.Id, song.Title, song.Kind, song.Lyrics, song.SourceFolderNo), (back.Id, back.Title, back.Kind, back.Lyrics, back.SourceFolderNo));
    }

    [Fact]
    public void AnOldRecoveryRowWithoutChordsOrCapoIsReadWithNone()
    {
        using var old = JsonDocument.Parse("""{"Id":"song:7","Title":"주 하나님","Kind":"Song","SlideNumber":2,"Lyrics":"[1]\n주 하나님","ContentPath":null,"FormatData":null,"UseIndividualFormatting":true,"Sequence":null,"SongLayout":null,"SourceFolderNo":3}""");
        Assert.True(MainViewModelCaptionHost.IsRecoverableItem(old.RootElement));
        var back = MainViewModelCaptionHost.FromRecovery(old.RootElement);
        Assert.Equal(("song:7", 2, 0, (string?)null), (back.Id, back.SlideNumber, back.Capo, back.Notations));
    }
}

public sealed partial class CaptionConsoleTests
{
    // a048 design 12: the caption tab connects with the saved settings when it is first seen and writes them back; an
    // obs.json with an item missing (no Port) keeps its host and saved password through that write.
    [Fact]
    public Task TheCaptionTabsFirstConnectionKeepsTheHostAndPasswordOfAProfileWithoutAPort() => WithTabSeenAfterStart(
        $$"""{"Host":"192.0.2.10","AutoConnect":false,"Secret":"{{CaptionSettingsFileTests.Secret("operator-obs-password")}}","Scenes":["설교","","",""]}""",
        async (obs, path) =>
        {
            Assert.Equal(("192.0.2.10", 4455, true, "설교"), (obs.Host, obs.Port, obs.HasSavedPassword, obs.Shortcuts[0]));
            using var saved = JsonDocument.Parse(File.ReadAllText(path));
            Assert.Equal(("192.0.2.10", 4455), (saved.RootElement.GetProperty("Host").GetString(), saved.RootElement.GetProperty("Port").GetInt32()));
            Assert.Equal("operator-obs-password", Decrypt(saved.RootElement.GetProperty("Secret").GetString()!));
            await Task.CompletedTask;
        });

    // a048 design 13: a saved password this PC cannot decrypt is no saved password, but the tab's first connection (which
    // writes the settings back) keeps it in the file as it was; a password the operator types then replaces it.
    [Fact]
    public Task TheCaptionTabsFirstConnectionKeepsASecretThisPcCannotDecrypt() => WithTabSeenAfterStart(
        """{"Host":"192.0.2.10","AutoConnect":false,"Secret":"AAAAAAAA","Scenes":["","","",""]}""",
        async (obs, path) =>
        {
            Assert.False(obs.HasSavedPassword);
            using (var saved = JsonDocument.Parse(File.ReadAllText(path)))
                Assert.Equal("AAAAAAAA", saved.RootElement.GetProperty("Secret").GetString());
            await obs.ConfigureAsync(obs.Host, obs.Port, "typed-password", false, ["", "", "", ""]);
            Assert.True(obs.HasSavedPassword);
            using var typed = JsonDocument.Parse(File.ReadAllText(path));
            Assert.Equal("typed-password", Decrypt(typed.RootElement.GetProperty("Secret").GetString()!));
        });

    private static string Decrypt(string secret) =>
        Encoding.UTF8.GetString(ProtectedData.Unprotect(Convert.FromBase64String(secret), null, DataProtectionScope.CurrentUser));

    // obs.json as given, OBS started as the main window does before it makes the tab, then the tab seen: the server
    // starts and OBS is connected with the saved settings, which are written back; then check.
    private static Task WithTabSeenAfterStart(string profile, Func<ObsOperator, string, Task> check) => StaHelper.RunOnStaAsync(async () =>
    {
        var directory = Directory.CreateTempSubdirectory();
        try
        {
            var path = Path.Combine(directory.FullName, "obs.json");
            File.WriteAllText(path, profile);
            var host = new FakeCaptionHost();
            using var session = new CaptionSession(host.Live, null, Path.Combine(directory.FullName, "caption-style.json"));
            using var server = new CaptionBroadcastServer(session, 0);
            using var store = new CaptionRecoveryStore(Path.Combine(directory.FullName, "CaptionRecovery"));
            using var obs = new ObsOperator(directory.FullName, Dispatcher.CurrentDispatcher);
            await obs.StartAsync(); // as the main window does before it makes the tab
            var console = new CaptionConsole(host, session, server, store, obs, () => [FakeCaptionHost.Screen]);
            DockPanel.SetDock(console.Strip, Dock.Top);
            var window = new Window
            {
                ShowActivated = false, ShowInTaskbar = false, Width = 720, Height = 960,
                Content = new DockPanel { Children = { console.Strip, console } },
            };
            console.Strip.CaptionTab.IsChecked = true;
            try
            {
                window.Show();
                for (int i = 0; i < 100 && obs.Connection == null; i++) await Pump(console, 50);
                Assert.NotNull(obs.Connection);
                Assert.NotEqual(profile, File.ReadAllText(path)); // written back
                await check(obs, path);
            }
            finally
            {
                console.EndSession();
                window.Close();
            }
        }
        finally { directory.Delete(true); }
    });
}
