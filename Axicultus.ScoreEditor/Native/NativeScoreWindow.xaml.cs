using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Microsoft.Win32;

namespace Axicultus.ScoreEditor;

public partial class NativeScoreWindow : Window
{
    private readonly MuseScoreEngine engine;
    private readonly CancellationTokenSource closing = new();
    private string path;
    private MuseScoreSession? session;
    private int page;
    private bool busy, closeRequested;

    internal NativeScoreWindow(MuseScoreEngine engine, string path)
    {
        InitializeComponent();
        this.engine = engine;
        this.path = path;
        string[] names = ["C", "C♯", "D", "E♭", "E", "F", "F♯", "G", "A♭", "A", "B♭", "B"];
        Pitch.ItemsSource = Enumerable.Range(0, 128).Select(p => $"{names[p % 12]}{p / 12 - 1}  ({p})");
        Tools.IsEnabled = Editor.IsEnabled = false;
    }

    private async void LoadScore(object sender, RoutedEventArgs e) => await Run(async () =>
    {
        session = await engine.OpenAsync(path, closing.Token);
        Refresh(null);
    });

    private void Refresh(ScoreNote? selected, int? newPitch = null)
    {
        if (session == null) return;
        var notes = session.Score.Notes;
        Notes.ItemsSource = notes;
        Notes.SelectedItem = notes.FirstOrDefault(n => n.Track == selected?.Track && n.Tick == selected.Tick && n.Pitch == (newPitch ?? selected.Pitch))
            ?? notes.FirstOrDefault(n => n.Track == selected?.Track && n.Tick == selected.Tick && n.Index == selected.Index)
            ?? notes.FirstOrDefault();
        ShowPage((Notes.SelectedItem as ScoreNote)?.Page ?? 0);
        Apply.IsEnabled = Notes.SelectedItem != null;
        Undo.IsEnabled = session.CanUndo;
        Title = $"{Path.GetFileName(path)} · MuseScore 악보 편집{(session.HasChanges ? " *" : "")}";
        Status.Text = $"{session.Score.Staves}단 · {notes.Length}개 음표";
    }

    private void NoteChanged(object sender, SelectionChangedEventArgs e)
    {
        if (Notes.SelectedItem is not ScoreNote note) return;
        Pitch.SelectedIndex = note.Pitch;
        Lyric.Text = note.LyricText;
        if (note.Page != page) ShowPage(note.Page);
    }

    private void ShowPage(int index)
    {
        if (session == null) return;
        page = Math.Clamp(index, 0, session.Pages.Count - 1);
        var bitmap = new BitmapImage();
        using var input = File.OpenRead(session.Pages[page]);
        bitmap.BeginInit();
        bitmap.StreamSource = input;
        bitmap.CacheOption = BitmapCacheOption.OnLoad;
        bitmap.EndInit();
        bitmap.Freeze();
        Page.Source = bitmap;
        PageNumber.Text = $"{page + 1} / {session.Pages.Count}";
        Previous.IsEnabled = page > 0;
        Next.IsEnabled = page < session.Pages.Count - 1;
    }

    private void SelectPageNote(object sender, MouseButtonEventArgs e)
    {
        if (session == null || Page.ActualWidth == 0 || Page.ActualHeight == 0) return;
        var pt = e.GetPosition(Page);
        var nearest = session.Score.Notes.Where(n => n.Page == page)
            .Select(n => (Note: n, Distance: (new Point(n.X * Page.ActualWidth, n.Y * Page.ActualHeight) - pt).Length))
            .MinBy(n => n.Distance);
        if (nearest.Note != null && nearest.Distance <= 18)
        {
            Notes.SelectedItem = nearest.Note;
            Notes.ScrollIntoView(nearest.Note);
        }
    }

    private void PreviousClick(object sender, RoutedEventArgs e) => ShowPage(page - 1);
    private void NextClick(object sender, RoutedEventArgs e) => ShowPage(page + 1);
    private async void ApplyClick(object sender, RoutedEventArgs e) => await Run(async () =>
    {
        if (Notes.SelectedItem is not ScoreNote note || session == null) return;
        int pitch = Pitch.SelectedIndex;
        await session.EditAsync(note, pitch, Lyric.Text, closing.Token);
        Refresh(note, pitch);
    });
    private async void UndoClick(object sender, RoutedEventArgs e) => await Run(async () =>
    {
        var selected = Notes.SelectedItem as ScoreNote;
        await session!.UndoAsync(closing.Token);
        Refresh(selected);
    });

    private async Task Save(bool choose)
    {
        string target = path;
        if (choose || !Path.GetExtension(path).Equals(".mscz", StringComparison.OrdinalIgnoreCase))
        {
            var dialog = new SaveFileDialog { Filter = "MuseScore 압축 악보|*.mscz", DefaultExt = ".mscz",
                InitialDirectory = Path.GetDirectoryName(path), FileName = Path.ChangeExtension(path, ".mscz") };
            if (dialog.ShowDialog(this) != true) return;
            target = dialog.FileName;
        }
        await session!.SaveAsync(target, closing.Token);
        path = target;
        Refresh(Notes.SelectedItem as ScoreNote);
        Status.Text = $"저장됨 · {path}";
    }
    private async void SaveClick(object sender, RoutedEventArgs e) => await Run(() => Save(false));
    private async void SaveAsClick(object sender, RoutedEventArgs e) => await Run(() => Save(true));
    private async void ExportClick(object sender, RoutedEventArgs e) => await Run(async () =>
    {
        var dialog = new SaveFileDialog { Filter = "PowerPoint|*.pptx", DefaultExt = ".pptx",
            InitialDirectory = Path.GetDirectoryName(path), FileName = Path.ChangeExtension(path, ".pptx") };
        if (dialog.ShowDialog(this) != true) return;
        await session!.ExportPptxAsync(dialog.FileName, closing.Token);
        Status.Text = $"PPTX 내보내기 완료 · {dialog.FileName}";
    });

    private async Task Run(Func<Task> operation)
    {
        if (busy) return;
        busy = true;
        Tools.IsEnabled = Editor.IsEnabled = false;
        try { await operation(); }
        catch (OperationCanceledException) when (closing.IsCancellationRequested) { }
        catch (Exception ex) { Status.Text = ex.Message; }
        finally
        {
            busy = false;
            Tools.IsEnabled = Editor.IsEnabled = session != null;
            if (closeRequested) Close();
        }
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        base.OnClosing(e);
        if (!closeRequested && session?.HasChanges == true &&
            MessageBox.Show(this, "저장하지 않은 수정을 버리고 닫을까요?", "악보 편집", MessageBoxButton.YesNo, MessageBoxImage.Question,
                MessageBoxResult.No) != MessageBoxResult.Yes) { e.Cancel = true; return; }
        closeRequested = true;
        closing.Cancel();
        if (busy) e.Cancel = true;
    }
    protected override async void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        try { if (session != null) await session.DisposeAsync(); }
        finally { closing.Dispose(); }
    }
}
