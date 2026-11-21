using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Xml.Linq;
using Microsoft.Win32;

namespace Axicultus.ScoreEditor;

public static class SongScoreEditor
{
    public static void Show(nint owner, string title)
    {
        var window = new ScoreEditorWindow(title);
        if (owner != 0) new WindowInteropHelper(window).Owner = owner;
        window.ShowDialog();
    }
}

public partial class ScoreEditorWindow : Window
{
    private readonly string songTitle;
    private MelodyScore score;
    private sealed record EditHistory(int Measure, XElement? Original, int PreviousBar, int PreviousIndex, bool WasAtEnd, int Revision);
    private readonly Stack<EditHistory> undo = new();
    private int bar, index, revision, savedRevision, nextRevision;
    private bool append;
    private string? path;
    private const string Filter = "MuseScore 악보 (*.mscz;*.mscx)|*.mscz;*.mscx";
    private static readonly string[] Durations = ["whole", "half", "quarter", "eighth", "16th", "32nd"];
    private static readonly string[] PitchNames = ["C", "C♯", "D", "D♯", "E", "F", "F♯", "G", "G♯", "A", "A♯", "B"];

    internal ScoreEditorWindow(string title)
    {
        InitializeComponent();
        songTitle = title;
        score = MelodyScore.Create(title);
        NotePitch.ItemsSource = Enumerable.Range(0, 128).Select(p => $"{PitchNames[p % 12]}{p / 12 - 1}").ToArray();
        NoteLength.ItemsSource = new[] { "온음표", "2분", "4분", "8분", "16분", "32분" };
        NotePitch.SelectedIndex = 60;
        NoteLength.SelectedIndex = 2;
        RefreshScore();
    }

    private void RefreshScore()
    {
        Systems.Children.Clear();
        foreach (var line in StaffDrawing.Lines(score))
        {
            var row = new StaffRow(line, !append && line.Bar == bar ? index : -1);
            row.MouseLeftButtonDown += (_, e) =>
            {
                bar = line.Bar;
                var point = e.GetPosition(row);
                index = StaffDrawing.Hit(line, point.X, row.ActualWidth);
                append = false;
                if (ClickToEnter.IsChecked == true)
                {
                    NotePitch.SelectedIndex = StaffDrawing.PitchAt(line, point.Y);
                    Edit(false);
                    return;
                }
                var entry = line.Events[index - line.Offset];
                if (MelodyScore.Pitch(entry) is int pitch) NotePitch.SelectedIndex = pitch;
                var duration = MelodyScore.Duration(entry);
                int length = Array.IndexOf(Durations, duration.Type);
                if (length >= 0) { NoteLength.SelectedIndex = length; Dot.IsChecked = duration.Dots == 1; }
                Lyrics.Text = MelodyScore.Lyric(entry);
                RefreshScore();
            };
            Systems.Children.Add(row);
            if (line.Bar == bar && index >= line.Offset && index < line.Offset + line.Events.Length)
                row.Loaded += (_, _) => row.BringIntoView();
        }
        UndoButton.IsEnabled = undo.Count != 0;
        Title = $"악보 편집 · {score.Title}{(revision == savedRevision ? "" : " *")}";
        Position.Text = append ? "마지막 마디 끝 · 입력하면 다음 마디를 만듭니다." : $"{bar + 1}마디 · {index + 1}번째 음표 또는 쉼표";
    }

    private sealed class StaffRow(StaffLine line, int selection) : FrameworkElement
    {
        protected override Size MeasureOverride(Size size) => new(double.IsInfinity(size.Width) ? 700 : size.Width, StaffDrawing.Height);
        protected override void OnRender(DrawingContext dc) => StaffDrawing.Draw(dc, line, ActualWidth, selection);
    }

    private void Edit(bool rest)
    {
        Run(() =>
        {
            int targetBar = bar, targetIndex = index;
            if (append) { targetBar++; targetIndex = 0; }
            var history = Capture(targetBar, append);
            if (append) score.AddMeasure();
            try
            {
                score.SetEvent(targetBar, targetIndex, new(Durations[NoteLength.SelectedIndex], Dot.IsChecked == true ? 1 : 0),
                    rest ? null : NotePitch.SelectedIndex, Lyrics.Text);
            }
            catch { if (append) score.RestoreMeasure(targetBar, null); throw; }
            Remember(history);
            bar = targetBar;
            index = targetIndex + 1;
            if (index >= MelodyScore.Events(score.Measures[bar]).Length)
            {
                if (bar == score.Measures.Length - 1) { index--; append = true; }
                else { bar++; index = 0; append = false; }
            }
            else append = false;
            Lyrics.Clear();
            RefreshScore();
        });
    }

    private EditHistory Capture(int measure, bool added) =>
        new(measure, added ? null : new XElement(score.Measures[measure]), bar, index, append, revision);

    private void Remember(EditHistory history)
    {
        undo.Push(history);
        revision = ++nextRevision;
    }
    private void LyricsClick(object sender, RoutedEventArgs e) => Run(() =>
    {
        if (append) throw new InvalidOperationException("가사를 넣을 음표를 선택해 주세요.");
        var history = Capture(bar, false);
        score.SetLyric(bar, index, Lyrics.Text);
        Remember(history);
        RefreshScore();
    });
    private void NoteClick(object sender, RoutedEventArgs e) => Edit(false);
    private void RestClick(object sender, RoutedEventArgs e) => Edit(true);
    private void LyricKey(object sender, KeyEventArgs e) { if (e.Key == Key.Enter) { Edit(false); e.Handled = true; } }
    private void MeasureClick(object sender, RoutedEventArgs e)
    {
        var history = Capture(score.Measures.Length, true);
        score.AddMeasure();
        Remember(history);
        bar = score.Measures.Length - 1; index = 0; append = false;
        RefreshScore();
    }
    private void UndoClick(object sender, RoutedEventArgs e)
    {
        if (undo.TryPop(out var edit))
        {
            score.RestoreMeasure(edit.Measure, edit.Original);
            (bar, index, append, revision) = (edit.PreviousBar, edit.PreviousIndex, edit.WasAtEnd, edit.Revision);
            RefreshScore();
        }
    }
    private void OpenClick(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog { Filter = Filter };
        if (dialog.ShowDialog(this) == true && CanLeave())
            Run(() => Reset(MelodyScore.Open(dialog.FileName), dialog.FileName));
    }
    private void NewClick(object sender, RoutedEventArgs e) { if (CanLeave()) Reset(MelodyScore.Create(songTitle), null); }
    private void Reset(MelodyScore next, string? file)
    {
        score = next; path = file; bar = index = revision = savedRevision = 0; append = false;
        Lyrics.Clear(); undo.Clear(); RefreshScore();
    }
    private bool Save(bool choose)
    {
        string? target = path;
        if (choose || target == null)
        {
            var dialog = new SaveFileDialog { Filter = "MuseScore 압축 악보|*.mscz|MuseScore XML 악보|*.mscx",
                DefaultExt = ".mscz", FileName = SuggestedName() };
            if (dialog.ShowDialog(this) != true) return false;
            target = dialog.FileName;
        }
        return Run(() => { score.Save(target); path = target; savedRevision = revision; RefreshScore(); });
    }
    private string SuggestedName() => string.Concat(score.Title.Split(Path.GetInvalidFileNameChars()));
    private void SaveClick(object sender, RoutedEventArgs e) => Save(false);
    private void SaveAsClick(object sender, RoutedEventArgs e) => Save(true);
    protected override void OnPreviewKeyDown(KeyEventArgs e)
    {
        base.OnPreviewKeyDown(e);
        if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.S) { Save(false); e.Handled = true; return; }
        if (Keyboard.FocusedElement is System.Windows.Controls.TextBox || Keyboard.Modifiers != ModifierKeys.None) return;
        if (e.Key == Key.N) { ClickToEnter.IsChecked = ClickToEnter.IsChecked != true; e.Handled = true; return; }
        if (ClickToEnter.IsChecked != true) return;
        if (e.Key is >= Key.A and <= Key.G)
        {
            int[] semitones = [9, 11, 0, 2, 4, 5, 7];
            int pitch = NotePitch.SelectedIndex;
            int step = semitones[e.Key - Key.A];
            NotePitch.SelectedIndex = Enumerable.Range(0, 128).Where(p => p % 12 == step).MinBy(p => Math.Abs(p - pitch));
            Edit(false);
        }
        else if (e.Key == Key.D0) Edit(true);
        else if (e.Key is >= Key.D2 and <= Key.D7) NoteLength.SelectedIndex = Key.D7 - e.Key;
        else if (e.Key == Key.OemPeriod) Dot.IsChecked = Dot.IsChecked != true;
        else return;
        e.Handled = true;
    }
    private void ExportClick(object sender, RoutedEventArgs e)
    {
        var dialog = new SaveFileDialog { Filter = "PowerPoint|*.pptx", DefaultExt = ".pptx", FileName = SuggestedName() };
        if (dialog.ShowDialog(this) == true) Run(() => ScoreSlides.Export(score, dialog.FileName));
    }
    private bool CanLeave() => revision == savedRevision || MessageBox.Show(this, "편집한 악보를 저장할까요?", "악보 편집",
        MessageBoxButton.YesNoCancel) switch { MessageBoxResult.Yes => Save(false), MessageBoxResult.No => true, _ => false };
    protected override void OnClosing(CancelEventArgs e) { if (!CanLeave()) e.Cancel = true; base.OnClosing(e); }
    private bool Run(Action action)
    {
        try { action(); return true; }
        catch (Exception ex) when (ex is IOException or InvalidDataException or UnauthorizedAccessException or InvalidOperationException or System.Xml.XmlException or FormatException or OverflowException)
        {
            MessageBox.Show(this, ex.Message, "악보 편집", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }
    }
}
