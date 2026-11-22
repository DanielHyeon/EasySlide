using System;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using Axicultus.Wpf.Data;
using Axicultus.Wpf.Library;
using Axicultus.Wpf.Settings;
using FluentAssertions;
using Xunit;

namespace Axicultus.Wpf.Tests.Library;

[Collection("WPF Application")]
public class SongEditorWindowTests
{
    [Fact]
    public void Window_OffersTheChordMenu_AndMeasuresChordsInTheFontOfItsLyricPanes() => StaHelper.RunOnSta(() =>
    {
        string root = Path.Combine(Path.GetTempPath(), $"Axicultus_SongEditorWindow_{Guid.NewGuid():N}");
        var settings = new SettingsService(new SettingsServiceOptions(Path.Combine(root, "settings.json"), Path.Combine(root, "SettingsBackups")));
        var viewModel = new SongEditorViewModel(settings, new AdminDatabaseRepository());
        // App.xaml merges the control styles; the shared test application only has the tokens.
        var resources = System.Windows.Application.Current.Resources;
        if (!resources.Contains("EsButton.Secondary"))
        {
            resources.MergedDictionaries.Add(new System.Windows.ResourceDictionary
            {
                Source = new Uri("pack://application:,,,/Axicultus;component/Controls/EsButton.xaml", UriKind.Absolute),
            });
        }

        var window = new SongEditorWindow(viewModel);
        try
        {
            var chordMenu = (MenuItem)window.FindName("ChordMenu");
            chordMenu.Items.OfType<MenuItem>().Select(item => item.Header).Should()
                .StartWith(["G", "G#", "Ab"]).And.EndWith(["Minor", "Minor 7th", "Major 7th", "Bass"]);
            chordMenu.Items.OfType<MenuItem>().Last().Items.OfType<MenuItem>().First().Tag.Should().Be("/G");
            ((TextBox)window.FindName("Lyrics2Editor")).FontFamily.Should().Be(((TextBox)window.FindName("LyricsEditor")).FontFamily);

            // The font of the panes is proportional: the chord goes over the character, not over the column.
            viewModel.MeasureEditorText("W").Should().BeGreaterThan(viewModel.MeasureEditorText("i"));
            viewModel.MeasureEditorText("G  ").Should().BeGreaterThan(viewModel.MeasureEditorText("G"), "the spaces that place a chord count");
            viewModel.LyricsRegion1 = "illicit WWW";
            viewModel.InsertChord(1, "illicit ".Length, "C");

            viewModel.Notations.Should().Be("(0;C;8;)");
            viewModel.LyricsRegion1.Should().EndWith("C »\nillicit WWW");

            // The stored chord is shown at the same place when the song is opened again.
            new SongEditorChords { Measure = viewModel.MeasureEditorText }.Combine("illicit WWW", "(0;C;8;)")
                .Should().Be(viewModel.LyricsRegion1);
        }
        finally
        {
            window.Close();
            try { Directory.Delete(root, recursive: true); } catch (IOException) { }
        }
    });

    [Theory]
    [InlineData("G  C\nlyric", 2, "G  C »\nlyric", 5)]
    [InlineData("first\r\nG\r\nlyric", 8, "first\r\nG »\r\nlyric", 9)]
    public void F8_TurnsTheLineOfTheCaretIntoAChordLine(string text, int caret, string expected, int caretAfter) => StaHelper.RunOnSta(() =>
    {
        var lyrics = new TextBox { AcceptsReturn = true, Text = text };
        lyrics.Select(caret, 0);

        SongEditorWindow.MarkChordLine(lyrics);

        lyrics.Text.Should().Be(expected);
        lyrics.CaretIndex.Should().Be(caretAfter, "the caret is in front of the marker, as in FrmEditItem");
    });

    // A heading between a chord line and its lyric line would take the chords from the line: a heading has none.
    [Theory]
    [InlineData("[1]\nfirst line\nG   D »\nsecond verse", "second", "[1]\nfirst line\n[2]\nG   D »\nsecond verse")]
    [InlineData("[1]\r\nfirst line\r\nG   D »\r\nsecond verse", "second", "[1]\r\nfirst line\r\n[2]\r\nG   D »\r\nsecond verse")]
    [InlineData("G »\nfirst line", "first", "[2]\nG »\nfirst line")]
    [InlineData("[1]\nfirst line\nsecond verse", "second", "[1]\nfirst line\n[2]\nsecond verse")]
    [InlineData("\nsecond verse", "second", "\n[2]\nsecond verse")]
    public void InsertHeading_PutsItAboveTheChordLineOfTheCaretLine(string text, string caretAt, string expected) => StaHelper.RunOnSta(() =>
    {
        var lyrics = new TextBox { AcceptsReturn = true, Text = text };
        lyrics.Select(text.IndexOf(caretAt, StringComparison.Ordinal) + 2, 0);

        SongEditorWindow.InsertHeadingAtCurrentLine(lyrics, "[2]");

        lyrics.Text.Should().Be(expected);
    });
}
