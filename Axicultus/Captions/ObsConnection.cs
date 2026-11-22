using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;

namespace Axicultus.Wpf.Captions;

public sealed record ObsStatus(string Message, string Scene, string[] Scenes)
{
    public bool Connected { get; init; }
}

/// <summary>One receive loop per identified OBS connection. Commands are never replayed on reconnect.</summary>
public sealed partial class ObsConnection(string host, int port, string password, IProgress<ObsStatus> status) : IAsyncDisposable
{
    private sealed class SessionRejected(string message) : IOException(message);

    private sealed class Connection(ClientWebSocket socket)
    {
        public System.Buffers.ArrayBufferWriter<byte> Message { get; } = new(4096);
        public ClientWebSocket Socket { get; } = socket;
        public ConcurrentDictionary<string, TaskCompletionSource<JsonObject>> Pending { get; } = new();
        public SemaphoreSlim Send { get; } = new(1, 1);
    }

    private readonly CancellationTokenSource _stop = new();
    private Connection? _active;
    private Task? _run;
    public void Start() => _run ??= Task.Run(() => RunAsync(_stop.Token));

    private async Task RunAsync(CancellationToken ct)
    {
        int retry = 0;
        while (!ct.IsCancellationRequested)
        {
            using var socket = new ClientWebSocket();
            var connection = new Connection(socket);
            try
            {
                using var handshake = CancellationTokenSource.CreateLinkedTokenSource(ct);
                handshake.CancelAfter(TimeSpan.FromSeconds(8));
                await IdentifyAsync(connection, host, port, password, 4, handshake.Token);
                var request = Guid.NewGuid().ToString("N");
                await SendAsync(connection, 6, new { requestType = "GetSceneList", requestId = request }, handshake.Token);
                string[] names = [];
                string scene = "";
                long changes = 0;
                while (!ct.IsCancellationRequested)
                {
                    var data = await ReadAsync(connection, names.Length == 0 ? handshake.Token : ct);
                    var payload = data["d"]!.AsObject();
                    var opcode = (int?)data["op"];
                    if (opcode == 5 && (string?)payload["eventType"] == "CurrentProgramSceneChanged")
                    {
                        scene = (string?)payload["eventData"]?["sceneName"] ?? "";
                        changes++;
                        status.Report(new("연결됨", scene, names) { Connected = true });
                    }
                    else if (opcode == 5 && (string?)payload["eventType"] == "SceneListChanged" && payload["eventData"]?["scenes"] is JsonArray updated)
                    {
                        names = [.. updated.Select(s => (string)s!["sceneName"]!)];
                        status.Report(new("연결됨", scene, names) { Connected = true });
                    }
                    else if (opcode == 7 && (string?)payload["requestId"] == request)
                    {
                        var result = payload["responseData"] ?? throw new InvalidDataException("씬 목록 요청 실패");
                        names = [.. result["scenes"]!.AsArray().Select(s => (string)s!["sceneName"]!)];
                        if (changes == 0) scene = (string?)result["currentProgramSceneName"] ?? "";
                        Volatile.Write(ref _active, connection);
                        retry = 0;
                        status.Report(new("연결됨", scene, names) { Connected = true });
                    }
                    else if (opcode is 7 or 9 && payload["requestId"] is JsonNode id &&
                        connection.Pending.TryRemove((string)id!, out var waiting))
                        waiting.TrySetResult(payload);
                }
            }
            catch (Exception) when (ct.IsCancellationRequested) { break; }
            catch (SessionRejected ex) { status.Report(new(ex.Message, "", [])); break; }
            catch (Exception ex)
            {
                status.Report(new("OBS 재연결 대기 · " + ex.Message, "", []));
            }
            finally
            {
                Interlocked.CompareExchange(ref _active, null, connection);
                socket.Abort();
                foreach (var item in connection.Pending.Values) item.TrySetException(new IOException("OBS 연결이 종료되었습니다."));
            }
            if (!ct.IsCancellationRequested) await Task.Delay(TimeSpan.FromSeconds(Math.Min(5, 1 << Math.Min(3,retry++))),ct);
        }
    }

    private static async Task IdentifyAsync(Connection connection, string host, int port, string password, int subscriptions, CancellationToken ct)
    {
        await connection.Socket.ConnectAsync(new UriBuilder("ws", host, port).Uri, ct);
        var hello = await ReadAsync(connection, ct);
        if ((int?)hello["op"] != 0) throw new InvalidDataException("OBS Hello 응답을 확인하세요.");
        var identify = new JsonObject { ["rpcVersion"] = 1, ["eventSubscriptions"] = subscriptions };
        if (hello["d"]?["authentication"] is JsonObject auth)
        {
            string Hash(string value) => Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
            identify["authentication"] = Hash(Hash(password + (string?)auth["salt"]) + (string?)auth["challenge"]);
        }
        await SendAsync(connection, 1, identify, ct);
        if ((int?)(await ReadAsync(connection, ct))["op"] != 2) throw new InvalidDataException("OBS 인증을 확인하세요.");
    }

    internal Task<string> ProbeAsync(string? temporaryPassword, CancellationToken ct)
        => ProbeProgramSceneAsync(host, port, temporaryPassword ?? password, ct);

    internal static async Task<string> ProbeProgramSceneAsync(string host, int port, string password, CancellationToken ct)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
        timeout.CancelAfter(TimeSpan.FromSeconds(5));
        using var socket = new ClientWebSocket();
        var probe = new Connection(socket);
        try
        {
            await IdentifyAsync(probe, host, port, password, 0, timeout.Token);
            var id = Guid.NewGuid().ToString("N");
            await SendAsync(probe, 6, new { requestId = id, requestType = "GetCurrentProgramScene" }, timeout.Token);
            var response = await ReadAsync(probe, timeout.Token);
            var payload = response["d"];
            if ((int?)response["op"] != 7 || (string?)payload?["requestId"] != id ||
                (bool?)payload?["requestStatus"]?["result"] != true ||
                (string?)payload?["responseData"]?["currentProgramSceneName"] is not { Length: > 0 } scene)
                throw new IOException("OBS에서 현재 프로그램 씬을 확인할 수 없습니다.");
            await socket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "", timeout.Token);
            return scene;
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            throw new TimeoutException("OBS가 5초 동안 응답하지 않았습니다.");
        }
        catch (WebSocketException ex) when (!ct.IsCancellationRequested)
        {
            throw new IOException($"{host}:{port} · OBS 실행 상태, WebSocket 서버 포트, 원격이면 공유기 포트포워딩을 확인하세요.", ex);
        }
        finally { probe.Send.Dispose(); }
    }

    public async Task SwitchAsync(string scene, string sourceName, bool enabled)
    {
        var connection = Volatile.Read(ref _active) ?? throw new IOException("OBS 연결을 기다리세요.");
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(_stop.Token);
        timeout.CancelAfter(TimeSpan.FromSeconds(5));
        var token = timeout.Token;
        if (string.IsNullOrWhiteSpace(sourceName))
        {
            await RequestAsync("SetCurrentProgramScene", new { sceneName = scene });
            return;
        }
        var found = await RequestAsync("GetSceneItemId", new { sceneName = scene, sourceName });
        int item = (int)found["responseData"]!["sceneItemId"]!;
        object[] requests = [
            new { requestType = "SetSceneItemEnabled", requestData = new { sceneName = scene, sceneItemId = item, sceneItemEnabled = enabled } },
            new { requestType = "SetCurrentProgramScene", requestData = new { sceneName = scene } }];
        var batch = await ExchangeAsync(connection, 8, id => new { requestId = id, haltOnFailure = true, executionType = 0, requests }, token);
        var results = batch["results"]!.AsArray();
        if (results.Count != requests.Length || results.Any(x => (bool?)x?["requestStatus"]?["result"] != true))
            throw new IOException("OBS 배치 일부 실패. 실제 씬과 소스 상태를 확인하세요.");

        async Task<JsonObject> RequestAsync(string type, object args)
        {
            var reply = await ExchangeAsync(connection, 6, id => new { requestId = id, requestType = type, requestData = args }, token);
            if ((bool?)reply["requestStatus"]?["result"] != true) throw new IOException(type + " · " + reply["requestStatus"]?["comment"]);
            return reply;
        }
    }

    public const string CaptionSourceName = "Axicultus 자막";

    /// <summary>
    /// Puts the caption browser source on top of the current program scene, canvas-sized. An existing source of that
    /// name is reused (its URL and size refreshed), so pressing it in another scene adds the same source there.
    /// Returns the scene name and whether the source was newly created.
    /// </summary>
    public async Task<(string Scene, bool Created)> AddCaptionSourceAsync(string url)
    {
        var connection = Volatile.Read(ref _active) ?? throw new IOException("OBS 연결을 기다리세요.");
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(_stop.Token);
        timeout.CancelAfter(TimeSpan.FromSeconds(10));
        var token = timeout.Token;

        var scene = (string)(await RequestAsync("GetCurrentProgramScene", new { }))!["currentProgramSceneName"]!;
        var video = (await RequestAsync("GetVideoSettings", new { }))!;
        var settings = new { url, width = (int)video["baseWidth"]!, height = (int)video["baseHeight"]! };
        var inputs = (await RequestAsync("GetInputList", new { inputKind = "browser_source" }))!["inputs"]!.AsArray();
        bool created = !inputs.Any(input => (string?)input?["inputName"] == CaptionSourceName);
        int item;
        if (created)
            item = (int)(await RequestAsync("CreateInput", new
            {
                sceneName = scene, inputName = CaptionSourceName, inputKind = "browser_source", inputSettings = settings, sceneItemEnabled = true
            }))!["sceneItemId"]!;
        else
        {
            await RequestAsync("SetInputSettings", new { inputName = CaptionSourceName, inputSettings = settings, overlay = true });
            item = await RequestAsync("GetSceneItemId", new { sceneName = scene, sourceName = CaptionSourceName }, optional: true) is { } found
                ? (int)found["sceneItemId"]!
                : (int)(await RequestAsync("CreateSceneItem", new { sceneName = scene, sourceName = CaptionSourceName, sceneItemEnabled = true }))!["sceneItemId"]!;
            await RequestAsync("SetSceneItemEnabled", new { sceneName = scene, sceneItemId = item, sceneItemEnabled = true });
        }
        // Index 0 is the bottom of OBS's source list; the caption must sit above the camera.
        int top = (await RequestAsync("GetSceneItemList", new { sceneName = scene }))!["sceneItems"]!.AsArray().Count - 1;
        await RequestAsync("SetSceneItemIndex", new { sceneName = scene, sceneItemId = item, sceneItemIndex = top });
        return (scene, created);

        async Task<JsonNode?> RequestAsync(string type, object args, bool optional = false)
        {
            var reply = await ExchangeAsync(connection, 6, id => new { requestId = id, requestType = type, requestData = args }, token);
            if ((bool?)reply["requestStatus"]?["result"] == true) return reply["responseData"] ?? new JsonObject();
            if (optional) return null;
            throw new IOException(type + " · " + reply["requestStatus"]?["comment"]);
        }
    }

    public async Task<byte[]> ReadPreviewAsync(string source, CancellationToken cancellation)
    {
        var connection = Volatile.Read(ref _active) ?? throw new IOException("OBS 연결 대기");
        using var limit = CancellationTokenSource.CreateLinkedTokenSource(_stop.Token, cancellation);
        limit.CancelAfter(TimeSpan.FromSeconds(3));
        var response = await ExchangeAsync(connection, 6, id => new
        {
            requestId = id, requestType = "GetSourceScreenshot",
            requestData = new { sourceName = source, imageFormat = "jpg", imageWidth = 640, imageHeight = 360 }
        }, limit.Token).ConfigureAwait(false);
        if ((bool?)response["requestStatus"]?["result"] != true)
            throw new IOException((string?)response["requestStatus"]?["comment"] ?? "OBS 카메라 신호 없음");
        var image = (string?)response["responseData"]?["imageData"];
        const string prefix = "data:image/jpeg;base64,";
        const string alternate = "data:image/jpg;base64,";
        var offset = image?.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) == true ? prefix.Length :
            image?.StartsWith(alternate, StringComparison.OrdinalIgnoreCase) == true ? alternate.Length : 0;
        if (offset == 0) throw new InvalidDataException("OBS 미리보기 이미지 형식을 확인하세요.");
        return Convert.FromBase64String(image![offset..]);
    }

    private static async Task<JsonObject> ExchangeAsync(Connection connection, int op, Func<string,object> build, CancellationToken token)
    {
        var id = Guid.NewGuid().ToString("N");
        var result = new TaskCompletionSource<JsonObject>(TaskCreationOptions.RunContinuationsAsynchronously);
        connection.Pending[id] = result;
        try
        {
            await SendAsync(connection, op, build(id), token);
            return await result.Task.WaitAsync(token);
        }
        finally { connection.Pending.TryRemove(id, out _); }
    }

    private static async Task SendAsync(Connection connection, int op, object payload, CancellationToken ct)
    {
        var bytes = JsonSerializer.SerializeToUtf8Bytes(new { op, d = payload });
        await connection.Send.WaitAsync(ct);
        try { await connection.Socket.SendAsync(bytes.AsMemory(), WebSocketMessageType.Text, true, ct); }
        finally { connection.Send.Release(); }
    }

    private static async Task<JsonObject> ReadAsync(Connection connection, CancellationToken ct)
    {
        var data = connection.Message;
        data.ResetWrittenCount();
        ValueWebSocketReceiveResult frame;
        do
        {
            // One reader owns this connection throughout handshake and event delivery.
            int available = Math.Min(4096, 1_048_577 - data.WrittenCount);
            frame = await connection.Socket.ReceiveAsync(data.GetMemory(available)[..available], ct);
            if (frame.MessageType == WebSocketMessageType.Close && (int?)connection.Socket.CloseStatus is 4009 or 4011)
                throw new SessionRejected("OBS가 인증 또는 세션을 거부했습니다. 설정을 확인한 뒤 연결하세요.");
            if (frame.MessageType != WebSocketMessageType.Text || data.WrittenCount + frame.Count > 1_048_576)
                throw new IOException("OBS 연결 또는 응답 형식을 확인하세요.");
            data.Advance(frame.Count);
        } while (!frame.EndOfMessage);
        return JsonNode.Parse(data.WrittenSpan)!.AsObject();
    }

    public async ValueTask DisposeAsync()
    {
        _stop.Cancel();
        if (_run != null) try { await _run.ConfigureAwait(false); } catch (OperationCanceledException) { }
        _stop.Dispose();
    }
}
