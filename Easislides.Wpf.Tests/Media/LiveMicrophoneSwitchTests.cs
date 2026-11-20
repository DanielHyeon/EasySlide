using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using Easislides.Wpf.Captions;
using Easislides.Wpf.Media;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Easislides.Wpf.Tests.Media;

public sealed class LiveMicrophoneSwitchTests
{
    private sealed class Feed : ILiveSpeakerFeed
    {
        internal TaskCompletionSource Prepared = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly TaskCompletionSource _ended = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public Task Ready => Prepared.Task;
        public Task Ended => _ended.Task;
        internal bool Enabled, Closed;
        public void Enable() => Enabled = true;
        public void Dispose() { Closed = true; Enabled = false; _ended.TrySetResult(); }
    }

    [Fact]
    public async Task FailedSpeakerPreparationKeepsPreviousRoute()
    {
        var first = new Feed(); var second = new Feed();
        var queue = new Queue<Feed>([first, second]);
        using var router = new LiveMicrophoneRouter((_, _) => queue.Dequeue());
        var mic = new InputDevice("mic", "마이크");
        var start = router.ToSpeakerAsync(mic, new("a", "강대상"), null, default);
        Assert.False(first.Enabled);
        first.Prepared.SetResult();
        await start;
        var change = router.ToSpeakerAsync(mic, new("b", "예배실"), null, default);
        Assert.True(first.Enabled);
        second.Prepared.SetException(new IOException("분리됨"));
        await Assert.ThrowsAsync<IOException>(() => change);
        Assert.True(first.Enabled);
        Assert.True(second.Closed);
        Assert.Equal(("mic", "a"), router.LocalSelection);
        router.StopSpeaker();
        Assert.False(first.Enabled);
        Assert.Null(router.LocalSelection);
    }

    [Fact]
    public async Task ObsDeviceUpdatesAreScopedAndMovingBetweenDestinationsTransfersTheMic()
    {
        var builder = WebApplication.CreateSlimBuilder();
        builder.Logging.ClearProviders();
        builder.WebHost.ConfigureKestrel(k => k.Listen(IPAddress.Loopback, 0));
        await using var server = builder.Build();
        server.UseWebSockets();
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(12));
        var token = timeout.Token;
        var writes = new List<string>();
        var devices = new Dictionary<string, string> { ["mic"] = "a", ["camera"] = "a" };
        bool muted = false;
        var serverErrors = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        server.MapGet("/", async context =>
        {
            using var socket = await context.WebSockets.AcceptWebSocketAsync();
            try
            {
                await Send(0, new { rpcVersion = 1 });
                await Read();
                await Send(2, new { negotiatedRpcVersion = 1 });
                while (socket.State == WebSocketState.Open)
                {
                    var request = (await Read())["d"]!;
                    var type = (string)request["requestType"]!;
                    var args = request["requestData"];
                    var id = (string?)args?["inputUuid"] ?? "";
                    var prop = id == "camera" ? "video_device_id" : "device_id";
                    object data;
                    switch (type)
                    {
                        case "GetSceneList":
                            data = new { currentProgramSceneName = "예배", scenes = new[]{new { sceneName = "예배" }} }; break;
                        case "GetInputList":
                            bool camera = (string?)args?["inputKind"] == "dshow_input";
                            data = new { inputs = new[]{new { inputName = "강대상", inputUuid = camera ? "camera" : "mic" }} }; break;
                        case "GetSourceActive":
                            data = new { videoActive = (string?)args?["sourceUuid"] == "mic" }; break;
                        case "GetInputSettings":
                            data = new { inputSettings = new JsonObject { [prop] = devices[id] } }; break;
                        case "GetInputPropertiesListPropertyItems":
                            Assert.Equal(prop, (string?)args?["propertyName"]);
                            data = new { propertyItems = new[]{"a","b","offline"}.Select(x => new { itemName = x, itemValue = x, itemEnabled = x != "offline" }) }; break;
                        case "SetInputSettings":
                            Assert.True((bool)args!["overlay"]!);
                            Assert.Single(args["inputSettings"]!.AsObject());
                            devices[id] = (string)args["inputSettings"]![prop]!;
                            writes.Add(id+"="+devices[id]); data = new {}; break;
                        case "GetInputMute": data = new { inputMuted = muted }; break;
                        case "SetInputMute":
                            Assert.Equal("mic", id);
                            muted = (bool)args!["inputMuted"]!;
                            writes.Add(muted ? "mute" : "unmute"); data = new {}; break;
                        default: throw new InvalidDataException(type);
                    }
                    await Send(7, new { requestId = (string)request["requestId"]!, requestStatus = new { result = true }, responseData = data });
                }
            }
            catch (Exception ex) when (ex is OperationCanceledException or WebSocketException or EndOfStreamException) { }
            catch (Exception ex) { serverErrors.TrySetException(ex); }

            async Task Send(int op, object d) => await socket.SendAsync(JsonSerializer.SerializeToUtf8Bytes(new { op, d }).AsMemory(), WebSocketMessageType.Text, true, token);
            async Task<JsonNode> Read()
            {
                using var stream = new MemoryStream();
                var buffer = new byte[2048];
                ValueWebSocketReceiveResult frame;
                do
                {
                    frame = await socket.ReceiveAsync(buffer.AsMemory(), token);
                    if (frame.MessageType != WebSocketMessageType.Text) throw new EndOfStreamException();
                    stream.Write(buffer, 0, frame.Count);
                } while (!frame.EndOfMessage);
                return JsonNode.Parse(stream.ToArray())!;
            }
        });
        await server.StartAsync(token);
        var ready = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        await using var connection = new ObsConnection(new Uri(server.Urls.Single()).Port, "",
            new Progress<ObsStatus>(s => { if (s.Connected) ready.TrySetResult(); }));
        connection.Start();
        await ready.Task.WaitAsync(token);
        var video = Assert.Single(await connection.CaptureInputsAsync(true, token));
        var microphone = Assert.Single(await connection.CaptureInputsAsync(false, token));
        Assert.False(await video.IsActiveAsync(token));
        Assert.True(await microphone.IsActiveAsync(token));
        var choices = await microphone.DevicesAsync(token);
        Assert.Equal(new[]{"a","b"}, choices.Select(x => x.Id));
        Assert.True(choices[0].Selected);
        Assert.Empty(writes);
        await video.SetDeviceAsync("b", token);
        await Assert.ThrowsAsync<IOException>(() => video.SetDeviceAsync("offline", token));
        var feed = new Feed();
        var nextFeed = new Feed();
        var feeds = new Queue<Feed>([feed, nextFeed]);
        using var router = new LiveMicrophoneRouter((_, _) => feeds.Dequeue());
        await router.ToObsAsync(connection, microphone, choices[1], token);
        feed.Prepared.SetResult();
        await router.ToSpeakerAsync(new("local", "Local"), new("pa", "PA"), connection, token);
        Assert.True(feed.Enabled);
        Assert.True(muted);
        await router.ToObsAsync(connection, microphone, choices[1], token);
        Assert.True(feed.Closed);
        Assert.False(muted);
        Assert.Equal(new[]{"camera=b","mic=b","mute","unmute"}, writes);
        muted = true; // The operator muted this input directly in OBS.
        nextFeed.Prepared.SetResult();
        await router.ToSpeakerAsync(new("local", "Local"), new("pa", "PA"), connection, token);
        string outcome = await router.ToObsAsync(connection, microphone, choices[1], token);
        Assert.Contains("음소거", outcome);
        Assert.True(muted);
        Assert.Equal(new[]{"camera=b","mic=b","mute","unmute"}, writes);
        if (serverErrors.Task.IsFaulted) await serverErrors.Task;
    }
}
