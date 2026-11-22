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

namespace Axicultus.Wpf.Captions;

/// <summary>Read-only browser source. Each browser owns a bounded latest-value queue.</summary>
public sealed class CaptionBroadcastServer : IDisposable
{
    public const string Url = "http://127.0.0.1:17948/";
    private readonly int _port;
    internal string Address { get; private set; } = Url;
    private readonly CaptionSession _session;
    private readonly Lock _sync = new();
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
        _latest = session.ForObs;
        _session.PublishedChanged += Publish;
    }

    private void Publish(PublishedCaption _)
    {
        var state = _session.ForObs;
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
        body{display:flex;flex-direction:column;align-items:center;justify-content:flex-end}
        /* Fixed caption area (1-3 lines of 6.4vh, same as the camera screen); fit() shrinks long text instead of growing it. */
        #caption{box-sizing:border-box;width:88%;height:calc(19.2vh + 1.5vh);margin-bottom:5vh;padding:.75vh 2.75vh;border-radius:2vh;
          background:#101010cf;display:flex;align-items:center;justify-content:center}
        #caption.plain{background:transparent;text-shadow:0 0 .12em #000,0 0 .12em #000,.05em .05em .1em #000}
        #caption.empty{display:none}
        #text{width:100%;color:white;text-align:center;font:600 5vh/1.28 'Malgun Gothic',sans-serif;white-space:pre-wrap;overflow-wrap:anywhere}
        #score{display:none;max-width:92%;max-height:42vh;margin-bottom:4vh;border-radius:.3em}</style>
        <img id="score" alt=""><div id="caption" class="empty"><div id="text"></div></div><script>
        const caption=document.getElementById('caption'),text=document.getElementById('text'),score=document.getElementById('score');
        let lines=3;
        function fit(){const h=innerHeight;caption.style.height='calc('+(6.4*lines)+'vh + 1.5vh)';
          const room=caption.clientHeight-h*.015;let s=h*.05;text.style.fontSize=s+'px';
          while(text.offsetHeight>room+.5&&s>h*.0175){s*=.94;text.style.fontSize=s+'px'}
          if(text.offsetHeight>room+.5)caption.style.height=(text.offsetHeight+h*.015)+'px'}
        function show(c){text.textContent=c.Text;text.style.color=c.Color;caption.className=(c.Text?'':'empty ')+(c.Box?'':'plain');
          lines=Math.min(3,Math.max(1,c.Lines||3));
          if(c.Image){score.src=c.Image;score.style.display='block';score.style.marginBottom=c.Text?'1vh':'4vh';score.style.maxHeight=c.Text?'34vh':'42vh'}
          else{score.removeAttribute('src');score.style.display='none'}
          if(c.Text)fit()}
        addEventListener('resize',fit);
        function connect(){const ws=new WebSocket('ws://'+location.host+'/captions');
          ws.onmessage=e=>show(JSON.parse(e.data));
          ws.onclose=()=>{show({Text:'',Color:'#FFFFFF',Box:true});setTimeout(connect,1000)};
          ws.onerror=()=>ws.close()}connect();</script></html>
        """;
}
