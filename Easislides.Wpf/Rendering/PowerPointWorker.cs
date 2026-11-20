using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Easislides.Wpf.Captions;
using Easislides.Wpf.Interop;

namespace Easislides.Wpf.Rendering;

/// <summary>One private child, one request at a time. A failed IPC exchange discards the child and its byte stream.</summary>
public sealed class PowerPointWorker : IPowerPointRenderBackend, IDisposable
{
    public const string Switch = "--ppt-render-worker";
    private sealed record Job(string File, int Slide, int Width, int Height, bool TextOnly);
    private sealed record Reply(OfficePptSlideExport? Image, CaptionDeck? Deck, PowerPointRenderErrorKind Error, string? Message);
    private readonly SemaphoreSlim _gate = new(1, 1);
    private readonly CancellationTokenSource _closing = new();
    private Process? _worker;
    private readonly Func<Process> _start;

    public PowerPointWorker() : this(Start) { }
    internal PowerPointWorker(Func<Process> start) => _start = start;

    public async Task<PowerPointSlideSnapshot> RenderSlideAsync(PowerPointRenderRequest request, CancellationToken ct)
    {
        var reply = await SendAsync(new(request.FilePath, request.SlideNumber, request.PixelWidth, request.PixelHeight, false), ct).ConfigureAwait(false);
        var image = reply.Image ?? throw new InvalidDataException("PPT 워커가 이미지를 반환하지 않았습니다.");
        return new(request.FilePath, request.SlideNumber, image.SlideCount, image.PixelWidth, image.PixelHeight,
            image.ImageBytes, image.ContentType, DateTimeOffset.UtcNow) { RenderBounds = (request.PixelWidth, request.PixelHeight) };
    }

    public async Task<CaptionDeck> ReadLegacyDeckAsync(string path, CancellationToken ct)
    {
        using var limit = CancellationTokenSource.CreateLinkedTokenSource(ct);
        limit.CancelAfter(TimeSpan.FromSeconds(45));
        try
        {
            var reply = await SendAsync(new(path, 0, 0, 0, true), limit.Token).ConfigureAwait(false);
            return reply.Deck ?? throw new InvalidDataException("PPT 워커가 자막을 반환하지 않았습니다.");
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested && limit.IsCancellationRequested)
        {
            throw new TimeoutException("PPT 자막 변환 시간이 초과되었습니다.");
        }
    }

    private async Task<Reply> SendAsync(Job job, CancellationToken ct)
    {
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(ct, _closing.Token);
        var token = linked.Token;
        await _gate.WaitAsync(token).ConfigureAwait(false);
        try
        {
            token.ThrowIfCancellationRequested();
            if (_worker is { HasExited: true } exited) { exited.Dispose(); _worker = null; }
            var process = _worker ??= _start();
            Reply result;
            try
            {
                // A blocked synchronous Windows pipe may not observe a cancelled read until its writer exits.
                using var abort = token.Register(static state =>
                {
                    var child = (Process)state!;
                    try { if (!child.HasExited) child.Kill(); }
                    catch (InvalidOperationException) { }
                    catch (System.ComponentModel.Win32Exception ex) { Trace.WriteLine(ex.Message); }
                }, process);
                await process.StandardInput.WriteLineAsync(JsonSerializer.Serialize(job).AsMemory(), token).ConfigureAwait(false);
                await process.StandardInput.FlushAsync(token).ConfigureAwait(false);
                var line = await process.StandardOutput.ReadLineAsync(token).ConfigureAwait(false);
                result = JsonSerializer.Deserialize<Reply>(line ?? throw new EndOfStreamException("PPT 워커가 종료되었습니다."))!;
            }
            catch
            {
                Discard(process);
                _worker = null;
                token.ThrowIfCancellationRequested();
                throw;
            }
            if (result.Error != PowerPointRenderErrorKind.None)
                throw new PowerPointRenderException(result.Error, result.Message ?? "PPT 변환 실패");
            return result;
        }
        finally { _gate.Release(); }
    }

    private static Process Start()
    {
        var executable = Path.ChangeExtension(typeof(PowerPointWorker).Assembly.Location, ".exe");
        var start = new ProcessStartInfo(executable)
        {
            UseShellExecute = false, CreateNoWindow = true, RedirectStandardInput = true,
            RedirectStandardOutput = true, StandardInputEncoding = new UTF8Encoding(false), StandardOutputEncoding = Encoding.UTF8
        };
        start.ArgumentList.Add(Switch);
        return Process.Start(start) ?? throw new IOException("PPT 워커를 시작할 수 없습니다.");
    }

    private static void Discard(Process worker)
    {
        try { if (!worker.HasExited) { worker.Kill(); worker.WaitForExit(1000); } }
        catch (InvalidOperationException) { }
        finally { worker.Dispose(); }
    }

    public void Dispose()
    {
        _closing.Cancel();
        _gate.Wait();
        try
        {
            if (_worker is not { } worker) return;
            try { worker.StandardInput.Close(); }
            catch (IOException) { }
            if (!worker.WaitForExit(1200)) Discard(worker); else worker.Dispose();
            _worker = null;
        }
        finally { _gate.Release(); }
    }

    public static async Task RunAsync()
    {
        using var input = new StreamReader(Console.OpenStandardInput(), Encoding.UTF8);
        using var output = new StreamWriter(Console.OpenStandardOutput(), new UTF8Encoding(false)) { AutoFlush = true };
        using var office = new OfficePptSession();
        while (await input.ReadLineAsync().ConfigureAwait(false) is { } line)
        {
            Reply reply;
            try
            {
                var job = JsonSerializer.Deserialize<Job>(line) ?? throw new InvalidDataException("빈 PPT 요청");
                if (job.TextOnly)
                    reply = new(null, await office.ReadCaptionDeckAsync(job.File).ConfigureAwait(false), PowerPointRenderErrorKind.None, null);
                else
                    reply = new(await office.ExportSlideAsync(job.File, job.Slide, job.Width, job.Height).ConfigureAwait(false),
                        null, PowerPointRenderErrorKind.None, null);
            }
            catch (Exception ex)
            {
                var cause = ex.GetBaseException();
                var kind = cause.HResult == unchecked((int)0x80040154) ? PowerPointRenderErrorKind.MissingOffice :
                    cause is ArgumentOutOfRangeException ? PowerPointRenderErrorKind.InvalidRequest : PowerPointRenderErrorKind.CorruptFile;
                reply = new(null, null, kind, cause.Message);
            }
            await output.WriteLineAsync(JsonSerializer.Serialize(reply)).ConfigureAwait(false);
        }
    }
}
