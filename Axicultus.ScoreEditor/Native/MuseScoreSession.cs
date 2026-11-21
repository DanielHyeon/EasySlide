namespace Axicultus.ScoreEditor;

/// <summary>A working copy and undo history shared by the WPF and WinForms hosts.</summary>
public sealed class MuseScoreSession : IAsyncDisposable
{
    private sealed record Revision(string File, ScoreInspection Score, string[] Pages);
    private readonly DirectoryInfo workspace = Directory.CreateTempSubdirectory("Axicultus-score-");
    private readonly SemaphoreSlim gate = new(1);
    private readonly MuseScoreEngine engine;
    private readonly Stack<Revision> history = new();
    private Revision current = null!;
    private Revision? saved;
    private bool disposed;

    private MuseScoreSession(MuseScoreEngine engine) => this.engine = engine;
    public ScoreInspection Score => current.Score;
    public IReadOnlyList<string> Pages => current.Pages;
    public bool CanUndo => history.Count > 0;
    public bool HasChanges => current != saved;

    internal static async Task<MuseScoreSession> OpenAsync(MuseScoreEngine engine, string path, CancellationToken token)
    {
        if (!File.Exists(path)) throw new FileNotFoundException("악보를 찾을 수 없습니다.", path);
        var session = new MuseScoreSession(engine);
        try
        {
            var folder = session.NewRevisionFolder();
            string file = Path.Combine(folder.FullName, "score.mscz");
            if (Path.GetExtension(path).Equals(".mscz", StringComparison.OrdinalIgnoreCase))
                await CopyAsync(path, file, token).ConfigureAwait(false);
            else if (Path.GetExtension(path).Equals(".mscx", StringComparison.OrdinalIgnoreCase))
                await engine.ConvertAsync(path, file, token).ConfigureAwait(false);
            else throw new InvalidDataException("MuseScore 악보(.mscz 또는 .mscx)를 선택해 주세요.");
            session.current = session.saved = await session.ReadRevisionAsync(file, token).ConfigureAwait(false);
            return session;
        }
        catch { await session.DisposeAsync().ConfigureAwait(false); throw; }
    }

    public async Task EditAsync(ScoreNote note, int pitch, string lyric, CancellationToken token = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(pitch);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(pitch, 127);
        await gate.WaitAsync(token).ConfigureAwait(false);
        try
        {
            ObjectDisposedException.ThrowIf(disposed, this);
            if (!current.Score.Notes.Any(n => ReferenceEquals(n, note)))
                throw new InvalidOperationException("악보가 변경되었습니다. 수정할 음표를 다시 선택해 주세요.");
            if (pitch == note.Pitch && lyric == note.LyricText) return;
            var folder = NewRevisionFolder();
            try
            {
                string file = Path.Combine(folder.FullName, "score.mscz");
                await engine.EditAsync(current.File, file, note, pitch, lyric == note.LyricText ? null : lyric, token).ConfigureAwait(false);
                var next = await ReadRevisionAsync(file, token).ConfigureAwait(false);
                history.Push(current);
                current = next;
            }
            catch { folder.Delete(true); throw; }
        }
        finally { gate.Release(); }
    }

    public async Task UndoAsync(CancellationToken token = default)
    {
        await gate.WaitAsync(token).ConfigureAwait(false);
        try
        {
            ObjectDisposedException.ThrowIf(disposed, this);
            if (history.TryPop(out var previous))
            {
                string discarded = Path.GetDirectoryName(current.File)!;
                current = previous;
                Directory.Delete(discarded, true);
            }
        }
        finally { gate.Release(); }
    }

    public async Task SaveAsync(string path, CancellationToken token = default)
    {
        if (!Path.GetExtension(path).Equals(".mscz", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("저장 형식은 MuseScore 압축 악보(.mscz)입니다.", nameof(path));
        await gate.WaitAsync(token).ConfigureAwait(false);
        try
        {
            ObjectDisposedException.ThrowIf(disposed, this);
            string temp = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
            try
            {
                await CopyAsync(current.File, temp, token).ConfigureAwait(false);
                token.ThrowIfCancellationRequested();
                File.Move(temp, path, true);
                saved = current;
            }
            finally { File.Delete(temp); }
        }
        finally { gate.Release(); }
    }

    public async Task ExportPptxAsync(string path, CancellationToken token = default)
    {
        await gate.WaitAsync(token).ConfigureAwait(false);
        try
        {
            ObjectDisposedException.ThrowIf(disposed, this);
            var output = workspace.CreateSubdirectory(Guid.NewGuid().ToString("N"));
            try
            {
                var pages = await engine.RenderAsync(current.File, output.FullName, token, forProjection: true).ConfigureAwait(false);
                if (pages.Length != current.Pages.Length) throw new InvalidDataException("미리보기와 송출 악보의 페이지 수가 다릅니다.");
                await Task.Run(() => ScoreSlides.ExportPages(pages, path, token), token).ConfigureAwait(false);
            }
            finally { output.Delete(true); }
        }
        finally { gate.Release(); }
    }

    private async Task<Revision> ReadRevisionAsync(string file, CancellationToken token)
    {
        var dir = Path.GetDirectoryName(file)!;
        var score = await engine.InspectAsync(file, dir, token).ConfigureAwait(false);
        var pages = await engine.RenderAsync(file, dir, token).ConfigureAwait(false);
        return new(file, score, pages);
    }
    private DirectoryInfo NewRevisionFolder() => workspace.CreateSubdirectory(Guid.NewGuid().ToString("N"));
    private static async Task CopyAsync(string source, string destination, CancellationToken token)
    {
        await using var input = File.OpenRead(source);
        await using var output = new FileStream(destination, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, true);
        await input.CopyToAsync(output, token).ConfigureAwait(false);
    }
    public async ValueTask DisposeAsync()
    {
        await gate.WaitAsync().ConfigureAwait(false);
        try
        {
            if (disposed) return;
            disposed = true;
            workspace.Delete(true);
        }
        finally { gate.Release(); }
    }
}
