using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using Axicultus.Wpf.Captions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Axicultus.Wpf.Tests.Shell;

public sealed class ObsCaptionConnectionTests
{
    private sealed class Reports : IProgress<ObsStatus>
    {
        public TaskCompletionSource Connected { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource Restarted { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource ManualScene { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public void Report(ObsStatus value)
        {
            if (value.Scene == "예배") Connected.TrySetResult();
            if (value.Scene == "설교") ManualScene.TrySetResult();
            if (value.Scene == "재시작") Restarted.TrySetResult();
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task UnresponsiveProbeTimesOutOrCancelsWhenLeavingSettings(bool leaveSettings)
    {
        var builder = WebApplication.CreateSlimBuilder();
        builder.Logging.ClearProviders();
        builder.WebHost.ConfigureKestrel(k => k.Listen(IPAddress.Loopback, 0));
        await using var server = builder.Build();
        server.UseWebSockets();
        var opened = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var finish = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        server.MapGet("/", async context =>
        {
            using var socket = await context.WebSockets.AcceptWebSocketAsync();
            opened.TrySetResult();
            await finish.Task;
        });
        await server.StartAsync();
        try
        {
            using var cancellation = new CancellationTokenSource();
            var probe = ObsConnection.ProbeProgramSceneAsync(new Uri(server.Urls.Single()).Port, "", cancellation.Token);
            await opened.Task.WaitAsync(TimeSpan.FromSeconds(4));
            if (leaveSettings)
            {
                cancellation.Cancel();
                await Assert.ThrowsAnyAsync<OperationCanceledException>(() => probe);
            }
            else
            {
                var error = await Assert.ThrowsAsync<TimeoutException>(() => probe.WaitAsync(TimeSpan.FromSeconds(10)));
                Assert.Contains("5초", error.Message);
            }
        }
        finally { finish.TrySetResult(); }
    }

    [Fact]
    public async Task AuthenticatesReceivesManualSceneReportsPartialBatchAndReconnects()
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(15));
        var token = timeout.Token;
        var builder = WebApplication.CreateSlimBuilder();
        builder.Logging.ClearProviders();
        builder.WebHost.ConfigureKestrel(k => k.Listen(IPAddress.Loopback, 0));
        await using var server = builder.Build();
        server.UseWebSockets();
        int sessions = 0;
        var script = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        server.MapGet("/", async context =>
        {
            using var socket = await context.WebSockets.AcceptWebSocketAsync();
            try
            {
                await Send(0, new { rpcVersion = 1, authentication = new { salt = "salt", challenge = "nonce" } });
                var identify = await Read();
                static string Hash(string s) => Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(s)));
                if ((int?)identify["d"]?["eventSubscriptions"] == 0)
                {
                    if ((string?)identify["d"]?["authentication"] != Hash(Hash("secretsalt") + "nonce"))
                    {
                        await socket.CloseOutputAsync((WebSocketCloseStatus)4009, "Authentication failed", token);
                        return;
                    }
                    await Send(2, new { negotiatedRpcVersion = 1 });
                    var probe = (await Read())["d"]!;
                    Assert.Equal("GetCurrentProgramScene", (string?)probe["requestType"]);
                    await Send(7, new { requestId = (string?)probe["requestId"], requestStatus = new { result = true },
                        responseData = new { currentProgramSceneName = "설교" } });
                    var close = await socket.ReceiveAsync(new byte[512].AsMemory(), token);
                    Assert.Equal(WebSocketMessageType.Close, close.MessageType);
                    return;
                }
                int session = Interlocked.Increment(ref sessions);
                Assert.Equal(Hash(Hash("secret" + "salt") + "nonce"), (string?)identify["d"]?["authentication"]);
                await Send(2, new { negotiatedRpcVersion = 1 });
                var request = (await Read())["d"]!;
                Assert.Equal("GetSceneList", (string?)request["requestType"]);
                var current = session == 1 ? "예배" : "재시작";
                await Send(7, new { requestId = (string?)request["requestId"], responseData = new
                    { currentProgramSceneName = current, scenes = new[] { new { sceneName = current } } } });
                if (session > 1) { script.TrySetResult(); return; }
                await Send(5, new { eventType = "CurrentProgramSceneChanged", eventData = new { sceneName = "설교" } });
                request = (await Read())["d"]!;
                Assert.Equal("GetSourceScreenshot", (string?)request["requestType"]);
                Assert.Equal(320, (int?)request["requestData"]?["imageWidth"]);
                await Send(7, new { requestId = (string?)request["requestId"], requestStatus = new { result = true },
                    responseData = new { imageData = "data:image/jpeg;base64,AQID" } });
                request = (await Read())["d"]!;
                Assert.Equal("GetSceneItemId", (string?)request["requestType"]);
                await Send(7, new { requestId = (string?)request["requestId"], requestStatus = new { result = true }, responseData = new { sceneItemId = 7 } });
                var batch = await Read();
                Assert.Equal(8,(int?)batch["op"]);
                var data = batch["d"]!;
                Assert.True((bool)data["haltOnFailure"]!);
                Assert.Equal("SetSceneItemEnabled",(string?)data["requests"]?[0]?["requestType"]);
                Assert.Equal(7,(int?)data["requests"]?[0]?["requestData"]?["sceneItemId"]);
                Assert.Equal("SetCurrentProgramScene",(string?)data["requests"]?[1]?["requestType"]);
                await Send(9,new { requestId = (string?)data["requestId"], results = new[] { new { requestStatus = new { result = false } } } });
                await socket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "restart", token);
            }
            catch (Exception ex) { script.TrySetException(ex); }

            async Task Send(int op, object d) => await socket.SendAsync(JsonSerializer.SerializeToUtf8Bytes(new { op, d }).AsMemory(), WebSocketMessageType.Text, true, token);
            async Task<JsonNode> Read()
            {
                using var stream = new MemoryStream();
                var buffer = new byte[1024];
                ValueWebSocketReceiveResult frame;
                do
                {
                    frame = await socket.ReceiveAsync(buffer.AsMemory(), token);
                    if (frame.MessageType != WebSocketMessageType.Text) throw new IOException("Unexpected close");
                    stream.Write(buffer,0,frame.Count);
                } while (!frame.EndOfMessage);
                return JsonNode.Parse(stream.ToArray())!;
            }
        });
        await server.StartAsync(token);
        var reports = new Reports();
        await using var client = new ObsConnection(new Uri(server.Urls.Single()).Port, "secret", reports);
        client.Start();
        await reports.Connected.Task.WaitAsync(token);
        await reports.ManualScene.Task.WaitAsync(token);
        Assert.Equal("설교", await client.ProbeAsync(null, token));
        var denied = await Assert.ThrowsAnyAsync<IOException>(() => client.ProbeAsync("incorrect", token));
        Assert.Contains("거부", denied.Message);
        Assert.Equal(new byte[] { 1,2,3 },await client.ReadPreviewAsync("카메라", token));
        var error = await Assert.ThrowsAsync<IOException>(() => client.SwitchAsync("예배", "자막", true));
        Assert.Contains("일부 실패",error.Message);
        await script.Task.WaitAsync(token);
        await reports.Restarted.Task.WaitAsync(token);
    }
}
