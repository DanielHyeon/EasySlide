using System;
using System.Collections.Generic;
using System.Net;
using System.Net.WebSockets;
using System.Text.Json;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Easislides.Wpf.Captions;

/// <summary>Read-only browser source. Each browser owns a bounded latest-value queue.</summary>
public sealed class CaptionBroadcastServer : IDisposable
{
    public const string Url = "http://127.0.0.1:17948/";
    private readonly int _port;
    internal string Address { get; private set; } = Url;
    private readonly CaptionSession _session;
    private readonly object _sync = new();
    private readonly HashSet<Channel<PublishedCaption>> _clients = [];
    private PublishedCaption _latest;
    private WebApplication? _server;
    private bool _disposed;
    private readonly SemaphoreSlim _start = new(1, 1);
    public CaptionBroadcastServer(CaptionSession session) : this(session, 17948) { }
    internal CaptionBroadcastServer(CaptionSession session, int port)
    {
        _port = port;
        _session = session;
        _latest = session.Published;
        _session.PublishedChanged += Publish;
    }

    private void Publish(PublishedCaption state)
    {
        lock (_sync)
        {
            _latest = state;
            foreach (var client in _clients) client.Writer.TryWrite(state);
        }
    }

    public Task StartAsync() => Task.Run(StartCoreAsync);

    private async Task StartCoreAsync()
    {
        await _start.WaitAsync().ConfigureAwait(false);
        try
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            if (_server != null) return;
            var builder = WebApplication.CreateSlimBuilder(new WebApplicationOptions { Args = [], ApplicationName = typeof(CaptionBroadcastServer).Assembly.FullName });
            builder.Configuration.Sources.Clear();
            builder.Logging.ClearProviders();
            builder.WebHost.ConfigureKestrel(options => options.Listen(IPAddress.Loopback, _port));
            var app = builder.Build();
            app.UseWebSockets();
            app.MapGet("/", () => Results.Content(Page, "text/html; charset=utf-8"));
            app.MapGet("/captions", ServeAsync);
            try { await app.StartAsync().ConfigureAwait(false); }
            catch { await app.DisposeAsync().ConfigureAwait(false); throw; }
            Address = System.Linq.Enumerable.Single(app.Urls).TrimEnd('/') + "/";
            _server = app;
        }
        finally { _start.Release(); }
    }

    private async Task ServeAsync(HttpContext context)
    {
        if (!context.WebSockets.IsWebSocketRequest ||
            (context.Request.Headers.Origin.Count > 0 && context.Request.Headers.Origin.ToString() != $"http://127.0.0.1:{context.Connection.LocalPort}"))
        { context.Response.StatusCode = 400; return; }
        using var socket = await context.WebSockets.AcceptWebSocketAsync();
        var client = Channel.CreateBounded<PublishedCaption>(new BoundedChannelOptions(1) { FullMode = BoundedChannelFullMode.DropOldest });
        lock (_sync)
        {
            if (_disposed) return;
            _clients.Add(client);
            client.Writer.TryWrite(_latest);
        }
        using var end = CancellationTokenSource.CreateLinkedTokenSource(context.RequestAborted);
        var reader = WatchCloseAsync(socket, end);
        try
        {
            await foreach (var value in client.Reader.ReadAllAsync(end.Token))
                await socket.SendAsync(JsonSerializer.SerializeToUtf8Bytes(value).AsMemory(), WebSocketMessageType.Text, true, end.Token);
        }
        catch (OperationCanceledException) { }
        catch (WebSocketException) { }
        finally
        {
            lock (_sync) _clients.Remove(client);
            end.Cancel();
            socket.Abort();
            await reader;
        }
    }

    private static async Task WatchCloseAsync(WebSocket socket, CancellationTokenSource end)
    {
        try { await socket.ReceiveAsync(new byte[128].AsMemory(), end.Token); }
        catch (Exception error) when (error is WebSocketException or OperationCanceledException) { }
        finally { end.Cancel(); }
    }

    public void Dispose()
    {
        _start.Wait();
        try
        {
            if (_disposed) return;
            _session.PublishedChanged -= Publish;
            lock (_sync)
            {
                _disposed = true;
                foreach (var client in _clients) client.Writer.TryComplete();
            }
            _server?.DisposeAsync().AsTask().GetAwaiter().GetResult();
        }
        finally { _start.Release(); }
    }

    private const string Page = """
        <!doctype html><html lang="ko"><meta charset="utf-8"><style>
        html,body{margin:0;height:100%;overflow:hidden;background:transparent}
        body{display:flex;align-items:flex-end;justify-content:center}
        #caption{max-width:88%;margin-bottom:5vh;border-radius:.4em;padding:.15em .55em;background:#101010cf;
          color:white;text-align:center;font:600 5vh/1.28 'Malgun Gothic',sans-serif;white-space:pre-wrap;overflow-wrap:anywhere}
        #caption:empty{display:none}</style><div id="caption"></div><script>
        const caption=document.getElementById('caption');
        function connect(){const ws=new WebSocket('ws://'+location.host+'/captions');
          ws.onmessage=e=>{caption.textContent=JSON.parse(e.data).Text};
          ws.onclose=()=>{caption.textContent='';setTimeout(connect,1000)};
          ws.onerror=()=>ws.close()}connect();</script></html>
        """;
}
