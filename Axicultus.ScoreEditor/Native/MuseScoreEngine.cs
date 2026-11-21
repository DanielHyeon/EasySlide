using System.ComponentModel;
using System.Diagnostics;
using System.IO.Compression;
using System.Text.Json;
using System.Xml;
using System.Xml.Linq;

namespace Axicultus.ScoreEditor;

/// <summary>Calls MuseScore Studio's converter and engraving extension in a separate process.</summary>
public sealed class MuseScoreEngine
{
    private readonly string executable;
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    private static readonly Lazy<string> Bridge = new(() =>
    {
        using var reader = new StreamReader(typeof(MuseScoreEngine).Assembly.GetManifestResourceStream("MuseScoreBridge")!);
        return reader.ReadToEnd();
    });

    public MuseScoreEngine(string executable) => this.executable = Path.GetFullPath(executable);

    public static MuseScoreEngine? Find()
    {
        string? configured = Environment.GetEnvironmentVariable("AXICULTUS_MUSESCORE_EXE");
        string local = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        string programs = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
        string? found = new[] { configured, Path.Combine(local, "Axicultus", "Engines", "MuseScore-4.6.5", "bin", "MuseScore4.exe"),
            Path.Combine(programs, "MuseScore 4", "bin", "MuseScore4.exe") }.FirstOrDefault(File.Exists);
        return found is null ? null : new(found);
    }

    public Task<MuseScoreSession> OpenAsync(string score, CancellationToken token = default)
        => MuseScoreSession.OpenAsync(this, Path.GetFullPath(score), token);

    internal Task ConvertAsync(string input, string output, CancellationToken token)
        => RunAsync(["-o", output, input], Path.GetDirectoryName(output)!, token);

    internal async Task<ScoreInspection> InspectAsync(string input, string directory, CancellationToken token)
    {
        string reply = Path.Combine(directory, "inspection.mscz");
        try
        {
            await InvokeAsync(input, reply, new { operation = "inspect" }, token).ConfigureAwait(false);
            using var zip = ZipFile.OpenRead(reply);
            var entry = zip.Entries.First(e => !e.FullName.Contains('/') && e.Name.EndsWith(".mscx", StringComparison.OrdinalIgnoreCase));
            using var stream = entry.Open();
            using var reader = XmlReader.Create(stream);
            while (reader.ReadToFollowing("metaTag"))
            {
                if (reader.GetAttribute("name") == "AxicultusInspection")
                    return JsonSerializer.Deserialize<ScoreInspection>(reader.ReadElementContentAsString(), Json)
                        ?? throw new InvalidDataException("MuseScore가 악보 정보를 반환하지 않았습니다.");
            }
            throw new InvalidDataException("MuseScore 확장 응답이 없습니다. MuseScore Studio 4.6.5를 확인해 주세요.");
        }
        finally { File.Delete(reply); }
    }

    internal async Task EditAsync(string input, string output, ScoreNote note, int pitch, string? lyric, CancellationToken token)
    {
        await InvokeAsync(input, output, new { operation = "edit", note.Track, note.Tick, note.Index, originalPitch = note.Pitch, pitch, lyric }, token).ConfigureAwait(false);
        // The engraving writer handles notation, parts and pictures. Project-level mixer
        // and view settings belong to Studio and aren't emitted by its notation writer.
        using var source = ZipFile.OpenRead(input);
        using var target = ZipFile.Open(output, ZipArchiveMode.Update);
        foreach (var entry in source.Entries.Where(e => e.Name is "audiosettings.json" or "viewsettings.json"))
        {
            if (target.GetEntry(entry.FullName) is not null) continue;
            using var data = entry.Open();
            using var copy = target.CreateEntry(entry.FullName).Open();
            data.CopyTo(copy);
        }
    }

    internal async Task<string[]> RenderAsync(string input, string directory, CancellationToken token, bool forProjection = false)
    {
        string output = Path.Combine(directory, "page.png");
        string[] arguments = forProjection
            ? ["-r", "240", "-T", "32", "-o", output, input]
            : ["-r", "144", "-o", output, input];
        await RunAsync(arguments, directory, token).ConfigureAwait(false);
        var pages = Directory.GetFiles(directory, "page*.png").OrderBy(p =>
            int.TryParse(Path.GetFileNameWithoutExtension(p).AsSpan(4).TrimStart('-'), out int page) ? page : 0).ToArray();
        if (pages.Length == 0) throw new InvalidDataException("MuseScore가 악보 페이지를 출력하지 않았습니다.");
        return pages;
    }

    private async Task InvokeAsync(string input, string output, object command, CancellationToken token)
    {
        // 4.6's macro runner has no filesystem or argument API. Each invocation gets a
        // private URI so two editors can run concurrently without sharing a request.
        string id = Guid.NewGuid().ToString("N");
        string uri = "musescore://extensions/axicultus/score-" + id;
        string extensions = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "MuseScore", "MuseScore4", "extensions");
        var folder = Directory.CreateDirectory(Path.Combine(extensions, "axicultus-" + id));
        try
        {
            string script = "var request = " + JsonSerializer.Serialize(command, Json) + ";\n" + Bridge.Value;
            await File.WriteAllTextAsync(Path.Combine(folder.FullName, "main.js"), script, token).ConfigureAwait(false);
            var manifest = new { uri, type = "composite", title = "Axicultus", ui_context = "Any",
                actions = new[] { new { code = "process", type = "macros", show_on_appmenu = false, path = "main.js" } } };
            string staged = Path.Combine(folder.FullName, "manifest.tmp");
            await File.WriteAllTextAsync(staged, JsonSerializer.Serialize(manifest), token).ConfigureAwait(false);
            File.Move(staged, Path.Combine(folder.FullName, "manifest.json"));
            await RunAsync(["--extension", uri + "?action=process", "-o", output, input], Path.GetDirectoryName(output)!, token).ConfigureAwait(false);
        }
        finally { folder.Delete(true); }
    }

    private async Task RunAsync(string[] arguments, string directory, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        var start = new ProcessStartInfo(executable) { UseShellExecute = false, CreateNoWindow = true,
            WorkingDirectory = directory, RedirectStandardError = true, RedirectStandardOutput = true };
        foreach (string arg in arguments) start.ArgumentList.Add(arg);
        using var worker = new Process { StartInfo = start };
        try { worker.Start(); }
        catch (Win32Exception ex) { throw new IOException("MuseScore Studio 실행 파일을 열 수 없습니다.", ex); }
        using var limit = CancellationTokenSource.CreateLinkedTokenSource(token);
        limit.CancelAfter(TimeSpan.FromMinutes(2));
        var stdout = worker.StandardOutput.BaseStream.CopyToAsync(Stream.Null);
        var stderr = worker.StandardError.ReadToEndAsync();
        try { await worker.WaitForExitAsync(limit.Token).ConfigureAwait(false); }
        catch (OperationCanceledException)
        {
            try { worker.Kill(entireProcessTree: true); } catch (InvalidOperationException) { }
            await worker.WaitForExitAsync().ConfigureAwait(false);
            await Task.WhenAll(stdout, stderr).ConfigureAwait(false);
            token.ThrowIfCancellationRequested();
            throw new TimeoutException("MuseScore가 2분 안에 응답하지 않았습니다.");
        }
        await Task.WhenAll(stdout, stderr).ConfigureAwait(false);
        if (worker.ExitCode != 0)
            throw new InvalidDataException($"MuseScore 처리 실패 (종료 코드 {worker.ExitCode}). {(await stderr.ConfigureAwait(false)).Trim()}");
    }
}

public sealed record ScoreInspection(int Staves, ScoreNote[] Notes);
public sealed record ScoreNote(int Track, int Tick, int Index, int Pitch, int Measure, string Lyric, int Page, double X, double Y)
{
    public int Staff => Track / 4 + 1;
    public int Voice => Track % 4 + 1;
    public string LyricText
    {
        get
        {
            try { return XElement.Parse("<lyric>" + Lyric + "</lyric>").Value; }
            catch (XmlException) { return Lyric; }
        }
    }
}
