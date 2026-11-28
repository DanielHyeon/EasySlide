using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml.Linq;
using Axicultus.Wpf.Captions;
using Axicultus.Wpf.Shell;
using Xunit;

namespace Axicultus.Wpf.Tests.Shell;

/// <summary>a046: the caption code is a library of its own (Axicultus.Captions), with its texts and a default look.</summary>
public sealed class CaptionLibraryTests
{
    private static readonly string Root = FindRoot();

    private static string FindRoot()
    {
        for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir != null; dir = dir.Parent)
            if (File.Exists(Path.Combine(dir.FullName, "Axicultus.sln"))) return dir.FullName;
        throw new DirectoryNotFoundException("Axicultus.sln");
    }

    [Fact]
    public void TheLibraryUsesNoHostApp()
    {
        var references = typeof(CaptionSession).Assembly.GetReferencedAssemblies().Select(name => name.Name).ToList();
        Assert.DoesNotContain("Axicultus", references);
        Assert.DoesNotContain("Axicultuss", references);
    }

    // ---- texts ----

    [Fact]
    public void TheCaptionTextsHaveTheKeysTheAppHadInBothLanguages()
    {
        var before = File.ReadLines(Path.Combine(Root, "Axicultus.Tests", "Localization", "CaptionTextKeys.txt"))
            .Where(line => line.Length > 0 && !line.StartsWith('#')).ToHashSet(StringComparer.Ordinal);
        Assert.NotEmpty(before);
        // The keys of every area of the library, per language (a text added later is welcome; none of these may go).
        string folder = Path.Combine(Root, "Axicultus.Captions", "Localization", "Text");
        HashSet<string> Keys(string language) => Directory.EnumerateFiles(folder, "*.resx")
            .Where(path => !path.EndsWith(".ko.resx", StringComparison.OrdinalIgnoreCase))
            .Select(path => Path.GetFileNameWithoutExtension(path))
            .SelectMany(area => XDocument.Load(Path.Combine(folder, area + language + ".resx"))
                .Root!.Elements("data").Select(e => area + "." + (string)e.Attribute("name")!))
            .ToHashSet(StringComparer.Ordinal);
        var english = Keys("");
        var korean = Keys(".ko");
        Assert.True(before.IsSubsetOf(english), "missing in English: " + string.Join(", ", before.Except(english)));
        Assert.True(before.IsSubsetOf(korean), "missing in Korean: " + string.Join(", ", before.Except(korean)));
        Assert.True(english.SetEquals(korean), $"English only: {string.Join(", ", english.Except(korean))}; Korean only: {string.Join(", ", korean.Except(english))}");
        // and the library finds each of them at run time, in English and in Korean: each language's own resources only
        // (no fall back from Korean to English)
        foreach (var culture in new[] { CultureInfo.InvariantCulture, CultureInfo.GetCultureInfo("ko") })
        {
            var missing = before.Where(key => CaptionText.Catalog.Manager(key[..key.IndexOf('.')])
                .GetResourceSet(culture, createIfNotExists: true, tryParents: false)?.GetString(key[(key.IndexOf('.') + 1)..]) is null).ToList();
            Assert.True(missing.Count == 0, $"{culture.Name}: {string.Join(", ", missing)}");
        }
        Assert.Equal("자막", CaptionText.Catalog.Manager("CaptionConsole").GetString("Tab", CultureInfo.GetCultureInfo("ko")));
    }

    // ---- default look ----

    // The colour, font and type keys the caption screens use (a046 design 1).
    private static readonly string[] ThemeKeys =
    [
        "Brush.Surface.Page", "Brush.Surface.Subtle", "Brush.Surface.Card",
        "Brush.Text.Primary", "Brush.Text.Secondary", "Brush.Text.Tertiary", "Brush.Text.Inverse",
        "Brush.Border.Default", "Brush.Border.Strong", "Brush.Border.Focus",
        "Brush.Accent.Primary", "Brush.Accent.PrimaryHover", "Brush.Accent.PrimaryPressed",
        "Brush.Live.Active", "Brush.Live.Standby", "Brush.Live.Hidden", "Brush.Live.OnActive", "Brush.Live.OnStandby", "Brush.Live.OnHidden",
        "Font.Primary", "Type.Body.FontSize", "Type.BodyLarge.FontSize", "Type.Button.FontSize", "Type.Title3.FontSize",
    ];

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void TheDefaultLookHasEveryKeyTheCaptionScreensUse(bool dark) => StaHelper.RunOnSta(() =>
    {
        var look = CaptionTheme.Defaults(dark);
        foreach (var key in ThemeKeys)
        {
            object? value = look[key];
            Assert.True(key switch
            {
                "Font.Primary" => value is FontFamily { Source: var source } && !source.Contains("pack:", StringComparison.Ordinal),
                _ when key.StartsWith("Type.", StringComparison.Ordinal) => value is double size && size > 0,
                _ => value is SolidColorBrush,
            }, $"{key}: {value}");
        }
        // the same values as the app's tokens they mirror (Font.Primary leaves out the app's bundled font on purpose)
        var app = AppTokens(dark);
        foreach (var key in ThemeKeys.Where(key => key != "Font.Primary"))
        {
            object mine = look[key] is SolidColorBrush brush ? brush.Color.ToString() : look[key];
            object theirs = key.StartsWith("Type.", StringComparison.Ordinal)
                ? double.Parse(app[key], CultureInfo.InvariantCulture)
                : ((Color)ColorConverter.ConvertFromString(app[key])).ToString();
            Assert.True(Equals(mine, theirs), $"{key}: {mine}, the app's token {theirs}");
        }
        // every key the caption XAML and code ask for (in the app and in the library) is one of them
        var used = new[] { Path.Combine(Root, "Axicultus", "Captions"), Path.Combine(Root, "Axicultus.Captions") }
            .SelectMany(folder => Directory.EnumerateFiles(folder, "*.*", SearchOption.AllDirectories))
            .Where(file => file.EndsWith(".xaml", StringComparison.Ordinal) || file.EndsWith(".cs", StringComparison.Ordinal))
            .Where(file => !file.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}") && !file.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}")
                && !file.Contains($"{Path.DirectorySeparatorChar}Theme{Path.DirectorySeparatorChar}"))
            .SelectMany(file => Regex.Matches(File.ReadAllText(file), @"(?:DynamicResource |"")((?:Brush|Font|Type)\.[A-Za-z0-9.]+?)(?:""|\}|\s)").Select(m => m.Groups[1].Value))
            .ToHashSet();
        Assert.NotEmpty(used);
        Assert.Empty(used.Except(ThemeKeys));
    });

    // The app's token values as written (Axicultus\Theme\Tokens): a brush's colour (through its {StaticResource Color.*}),
    // a type size.
    private static Dictionary<string, string> AppTokens(bool dark)
    {
        XNamespace x = "http://schemas.microsoft.com/winfx/2006/xaml";
        var values = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var file in new[] { dark ? "Colors.Dark.xaml" : "Colors.Light.xaml", "Typography.xaml" })
            foreach (var element in XDocument.Load(Path.Combine(Root, "Axicultus", "Theme", "Tokens", file)).Root!.Elements())
                if ((string?)element.Attribute(x + "Key") is { } key)
                    values[key] = (string?)element.Attribute("Color") ?? element.Value.Trim();
        foreach (var key in values.Keys.ToList())
            if (Regex.Match(values[key], @"^\{StaticResource ([^}]+)\}$") is { Success: true } reference) values[key] = values[reference.Groups[1].Value];
        return values;
    }

    [Fact]
    public void TheDefaultLookIsPutOnAnElementAndReplacedWhenTheModeChanges() => StaHelper.RunOnSta(() =>
    {
        var panel = new Border();
        CaptionTheme.Apply(panel, dark: false);
        var light = Assert.IsType<SolidColorBrush>(panel.FindResource("Brush.Surface.Page")).Color;
        CaptionTheme.Apply(panel, dark: true);
        Assert.Single(panel.Resources.MergedDictionaries);
        Assert.NotEqual(light, Assert.IsType<SolidColorBrush>(panel.FindResource("Brush.Surface.Page")).Color);
    });

    // ---- the WPF app's live session as the captions see it ----

    [Fact]
    public void TheLiveSessionIsSeenAsOneSnapshotPerChange()
    {
        var live = new LiveSessionService();
        var source = live.ForCaptions();
        Assert.Equal(CaptionLiveSnapshot.Off, source.Current);
        Assert.Same(source.Current, source.Current);
        var seen = new List<CaptionLiveSnapshot>();
        void Seen(object? sender, CaptionLiveSnapshot snapshot) => seen.Add(snapshot);
        source.Changed += Seen;
        live.GoLive(new("p", "찬양", "PPT") { ContentPath = @"C:\songs.pptx", SlideNumber = 2 }, "projector");
        var active = Assert.Single(seen);
        Assert.Equal(new CaptionLiveSnapshot(CaptionLiveState.Active, @"C:\songs.pptx", 2), active);
        Assert.Same(active, source.Current);
        live.Refresh(); // the same output again: the same object
        Assert.Same(active, seen[^1]);
        live.HideOutput(blackout: true);
        Assert.Equal(CaptionLiveState.Hidden, source.Current.State);
        Assert.NotSame(active, source.Current);
        source.Changed -= Seen;
        live.Stop();
        Assert.Equal(3, seen.Count); // no longer told
        Assert.Equal(CaptionLiveState.Off, source.Current.State);
    }

    [Fact]
    public void AnEventWithAnOlderSnapshotLeavesTheCurrentOneAsItWas()
    {
        var live = new ScriptedLive();
        var source = live.ForCaptions();
        var seen = new List<CaptionLiveSnapshot>();
        source.Changed += (_, snapshot) => seen.Add(snapshot);
        var old = live.Current;
        live.Current = new LiveSessionSnapshot(Axicultus.Wpf.Controls.LiveState.Active, "", "", IsBlackout: false);
        var current = source.Current;
        live.Raise(old); // a late event about the output before
        Assert.Equal(CaptionLiveState.Off, seen[^1].State);
        Assert.Same(current, source.Current);
        live.Raise(live.Current); // the output now: told as the same object
        Assert.Same(current, seen[^1]);
        Assert.Same(current, source.Current);
    }

    [Fact]
    public void TheServiceIsHeldOnlyWhileSomeoneListens()
    {
        var live = new ScriptedLive();
        var source = live.ForCaptions();
        EventHandler<CaptionLiveSnapshot> first = (_, _) => { }, second = (_, _) => { };
        source.Changed += null;
        Assert.Equal(0, live.Listeners);
        source.Changed += first;
        source.Changed += null;
        source.Changed += second;
        Assert.Equal(1, live.Listeners);
        source.Changed -= first;
        Assert.Equal(1, live.Listeners);
        source.Changed -= second;
        Assert.Equal(0, live.Listeners);
        source.Changed -= second; // (once more: nothing to drop)
        source.Changed += null;
        Assert.Equal(0, live.Listeners);
        using (new CaptionSession(source)) Assert.Equal(1, live.Listeners);
        Assert.Equal(0, live.Listeners); // the session let go when it was disposed
    }

    // A live session that raises what a test tells it to and counts who listens.
    private sealed class ScriptedLive : ILiveSessionService
    {
        private EventHandler<LiveSessionChangedEventArgs>? _changed;
        public int Listeners => _changed?.GetInvocationList().Length ?? 0;
        public event EventHandler<LiveSessionChangedEventArgs>? SessionChanged { add => _changed += value; remove => _changed -= value; }
        public LiveSessionSnapshot Current { get; set; } = LiveSessionSnapshot.Off;
        public void Raise(LiveSessionSnapshot snapshot) => _changed?.Invoke(this, new LiveSessionChangedEventArgs(snapshot));
        public void GoLive(LiveQueueItem item, string outputMonitorName) => throw new NotSupportedException();
        public void UpdateHiddenContent(LiveQueueItem item, string outputMonitorName) => throw new NotSupportedException();
        public void HideOutput(bool blackout) => throw new NotSupportedException();
        public void ClearOutput() => throw new NotSupportedException();
        public void Restore() => throw new NotSupportedException();
        public void Refresh() => throw new NotSupportedException();
        public void SetLyricsAlertMessage(string text) => throw new NotSupportedException();
        public void SetAudienceAlert(bool parental, OutputAlert? alert) => throw new NotSupportedException();
        public void SetReferenceAlert(bool visible, string text) => throw new NotSupportedException();
        public void Stop() => throw new NotSupportedException();
    }

    [Fact]
    public void EveryLiveStateHasItsCaptionState()
        => Assert.Equal(new[] { "Off", "Standby", "Active", "Hidden" },
            Enum.GetValues<Axicultus.Wpf.Controls.LiveState>().Select(state => LiveSessionCaptions.Map(state).ToString()).ToArray());
}
