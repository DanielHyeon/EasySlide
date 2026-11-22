using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Axicultus.Wpf.Library;

// 레거시 대체: FrmEditItem (+ FrmEditNotes notation 부분 통합).
public partial class SongEditorWindow : Window
{
    private readonly SongEditorViewModel _viewModel;
    private TextBox _activeLyrics;

    public SongEditorWindow(SongEditorViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        _activeLyrics = LyricsEditor;
        // A chord is stored with the lyric character it stands over, so the panes' own font decides the positions.
        _viewModel.MeasureEditorText = MeasureLyricText;
        _viewModel.Saved += OnSaved;
        DataContext = viewModel;
        for (int number = 1; number <= 10; number++)
            VerseNumbers.Items.Add(new MenuItem { Header = $"{number}절", Tag = $"[{number}]" });
        foreach (var (group, chords) in viewModel.ChordMenu)
        {
            ItemsControl parent = ChordMenu;
            if (group.Length > 0)
                ChordMenu.Items.Add(parent = new MenuItem { Header = group });
            foreach (string chord in chords)
                parent.Items.Add(new MenuItem { Header = chord, Tag = chord });
        }
    }

    private double MeasureLyricText(string text)
        => new FormattedText(
            text,
            CultureInfo.CurrentUICulture,
            FlowDirection.LeftToRight,
            new Typeface(LyricsEditor.FontFamily, LyricsEditor.FontStyle, LyricsEditor.FontWeight, LyricsEditor.FontStretch),
            LyricsEditor.FontSize,
            LyricsEditor.Foreground,
            new NumberSubstitution(),
            TextOptions.GetTextFormattingMode(LyricsEditor),
            VisualTreeHelper.GetDpi(this).PixelsPerDip).WidthIncludingTrailingWhitespace;

    private void Lyrics_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        => _activeLyrics = (TextBox)sender;

    private void InsertSection_Click(object sender, RoutedEventArgs e)
    {
        if (e.OriginalSource is not MenuItem { Tag: string marker }) return;
        InsertHeadingAtCurrentLine(_activeLyrics, marker);
        _activeLyrics.Focus();
        e.Handled = true;
    }

    private void InsertChord_Click(object sender, RoutedEventArgs e)
    {
        if (e.OriginalSource is not MenuItem { Tag: string chord }) return;
        var lyrics = _activeLyrics;
        int caret = _viewModel.InsertChord(lyrics == Lyrics2Editor ? 2 : 1, lyrics.CaretIndex, chord);
        lyrics.CaretIndex = System.Math.Min(caret, lyrics.Text.Length);
        lyrics.Focus();
        e.Handled = true;
    }

    // FrmEditItem F8: turns the line of the caret into a chord line.
    private void Lyrics_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.F8) return;
        MarkChordLine((TextBox)sender);
        e.Handled = true;
    }

    internal static void MarkChordLine(TextBox lyrics)
    {
        string text = lyrics.Text;
        int lineEnd = text.IndexOf('\n', lyrics.CaretIndex);
        lineEnd = lineEnd < 0 ? text.Length : lineEnd;
        if (lineEnd > 0 && text[lineEnd - 1] == '\r') lineEnd--;
        lyrics.Select(lineEnd, 0);
        lyrics.SelectedText = " »";
        lyrics.Select(lineEnd + 1, 0);
    }

    private void WrapLyrics_Click(object sender, RoutedEventArgs e)
        => LyricsEditor.TextWrapping = Lyrics2Editor.TextWrapping =
            WrapLyrics.IsChecked == true ? TextWrapping.Wrap : TextWrapping.NoWrap;

    internal static void InsertHeadingAtCurrentLine(TextBox lyrics, string marker)
    {
        string text = lyrics.Text;
        int caret = lyrics.SelectionStart;
        int lineStart = caret == 0 ? 0 : text.LastIndexOf('\n', caret - 1) + 1;
        // A chord line belongs to the lyric line below it: the heading goes above the two.
        int aboveStart = lineStart < 2 ? 0 : text.LastIndexOf('\n', lineStart - 2) + 1;
        if (lineStart > 0 && text[aboveStart..lineStart].TrimEnd().EndsWith('»'))
        {
            lineStart = aboveStart;
        }

        int firstBreak = text.IndexOf('\n');
        string newline = firstBreak >= 0 && (firstBreak == 0 || text[firstBreak - 1] != '\r') ? "\n" : System.Environment.NewLine;
        string heading = marker + newline;
        lyrics.Select(lineStart, 0);
        lyrics.SelectedText = heading;
        lyrics.Select(lineStart + heading.Length, 0);
    }

    protected override void OnClosed(System.EventArgs e)
    {
        _viewModel.Saved -= OnSaved;
        _viewModel.Dispose();
        base.OnClosed(e);
    }

    private void OnSaved(object? sender, SongEditorSavedEventArgs e)
    {
        DialogResult = true;
        Close();
    }

    private void ScoreClick(object sender, RoutedEventArgs e) =>
        Axicultus.ScoreEditor.SongScoreEditor.Show(new System.Windows.Interop.WindowInteropHelper(this).Handle, _viewModel.Title);

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
