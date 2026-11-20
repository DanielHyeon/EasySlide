using System;
using System.IO;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;

namespace Axicultus.Wpf.Captions;

internal sealed record ObsDeviceChoice(string Id, string Name, bool Selected);

internal sealed class ObsCaptureInput(string name, string id, int port, object session, string kind, string property,
    Func<string, JsonObject, CancellationToken, Task<JsonObject>> request)
{
    internal string Name => name;
    internal bool SameSession(ObsCaptureInput other) => ReferenceEquals(session, other.Session);
    private object Session => session;
    internal (int, string) Key => (port, id.Length == 0 ? name : id);

    private JsonObject Target() => id.Length == 0 ? new() { ["inputName"] = name } : new() { ["inputUuid"] = id };

    internal async Task<bool> IsActiveAsync(CancellationToken ct)
    {
        var target = id.Length == 0 ? new JsonObject { ["sourceName"] = name } : new JsonObject { ["sourceUuid"] = id };
        return (bool)(await request("GetSourceActive", target, ct))["videoActive"]!;
    }

    private async Task<string?> ReadDeviceAsync(CancellationToken ct)
    {
        var settings = await request("GetInputSettings", Target(), ct);
        if ((string?)settings["inputKind"] is { } actual && actual != kind)
            throw new IOException("OBS 입력 종류가 변경되었습니다. 목록을 다시 여세요.");
        return (string?)settings["inputSettings"]?[property];
    }

    internal async Task<ObsDeviceChoice[]> DevicesAsync(CancellationToken ct)
    {
        var selected = await ReadDeviceAsync(ct);
        if (selected == null)
        {
            var defaults = await request("GetInputDefaultSettings", new() { ["inputKind"] = kind }, ct);
            selected = (string?)defaults["defaultInputSettings"]?[property];
        }
        var args = Target();
        args["propertyName"] = property;
        var data = await request("GetInputPropertiesListPropertyItems", args, ct);
        return [.. data["propertyItems"]!.AsArray()
            .Where(x => (bool?)x?["itemEnabled"] != false && (string?)x?["itemValue"] is { Length: > 0 })
            .Select(x => new ObsDeviceChoice((string)x!["itemValue"]!, (string?)x["itemName"] ?? (string)x["itemValue"]!,
                selected == (string?)x["itemValue"]))];
    }

    internal async Task SetDeviceAsync(string deviceId, CancellationToken ct)
    {
        var choice = Array.Find(await DevicesAsync(ct), d => d.Id == deviceId)
            ?? throw new IOException("장치가 분리되었습니다. 목록을 다시 여세요.");
        if (choice.Selected) return;
        var args = Target();
        args["inputSettings"] = new JsonObject { [property] = deviceId };
        args["overlay"] = true;
        await request("SetInputSettings", args, ct);
        if (await ReadDeviceAsync(ct) != deviceId) throw new IOException("OBS에서 선택한 장치의 적용을 확인할 수 없습니다.");
    }

    internal async Task<bool> IsMutedAsync(CancellationToken ct) =>
        (bool)(await request("GetInputMute", Target(), ct))["inputMuted"]!;

    internal Task SetMutedAsync(bool muted, CancellationToken ct)
    {
        var args = Target();
        args["inputMuted"] = muted;
        return request("SetInputMute", args, ct);
    }
}

public sealed partial class ObsConnection
{
    internal bool IsIdentified => Volatile.Read(ref _active) != null;

    internal async Task<ObsCaptureInput[]> CaptureInputsAsync(bool camera, CancellationToken ct)
    {
        var session = Volatile.Read(ref _active) ?? throw new IOException("카메라 하단 자막 → OBS에서 먼저 연결하세요.");
        var kind = camera ? "dshow_input" : "wasapi_input_capture";
        var reply = await Request("GetInputList", new() { ["inputKind"] = kind }, ct);
        return [.. reply["inputs"]!.AsArray().Where(x => x?["inputName"] != null)
            .Select(x => new ObsCaptureInput((string)x!["inputName"]!, (string?)x["inputUuid"] ?? "", port, session,
                kind, camera ? "video_device_id" : "device_id", Request))];

        async Task<JsonObject> Request(string type, JsonObject args, CancellationToken cancellation)
        {
            if (!ReferenceEquals(session, Volatile.Read(ref _active)))
                throw new IOException("OBS 연결이 변경되었습니다. 장치 목록을 다시 여세요.");
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellation, _stop.Token);
            timeout.CancelAfter(TimeSpan.FromSeconds(4));
            try
            {
                var result = await ExchangeAsync(session, 6, id => new { requestId = id, requestType = type, requestData = args }, timeout.Token);
                if ((bool?)result["requestStatus"]?["result"] != true)
                    throw new IOException("OBS 요청 실패 · " + ((string?)result["requestStatus"]?["comment"] ?? type));
                return result["responseData"]?.AsObject() ?? [];
            }
            catch (OperationCanceledException) when (!cancellation.IsCancellationRequested)
            {
                throw new IOException("OBS 응답이 중단되었습니다. 연결 상태를 확인하세요.");
            }
        }
    }
}
