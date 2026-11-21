using System.Windows;
using System.Windows.Controls;

namespace Axicultus.Wpf.Library;

// 레거시 대체: FrmEditItem (+ FrmEditNotes notation 부분 통합).
public partial class SongEditorWindow : Window
{
    private readonly SongEditorViewModel _viewModel;

    public SongEditorWindow(SongEditorViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        _viewModel.Saved += OnSaved;
        DataContext = viewModel;
        for (int number = 1; number <= 10; number++)
            VerseNumbers.Items.Add(new MenuItem { Header = $"{number}절", Tag = $"[{number}]" });
    }

    private void InsertSection_Click(object sender, RoutedEventArgs e)
    {
        if (e.OriginalSource is not MenuItem { Tag: string marker }) return;
        InsertHeadingAtCurrentLine(LyricsEditor, marker);
        LyricsEditor.Focus();
        e.Handled = true;
    }

    internal static void InsertHeadingAtCurrentLine(TextBox lyrics, string marker)
    {
        string text = lyrics.Text;
        int caret = lyrics.SelectionStart;
        int lineStart = caret == 0 ? 0 : text.LastIndexOf('\n', caret - 1) + 1;
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
