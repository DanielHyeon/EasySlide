using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Controls;
using Axicultus.Wpf.Localization;
using DirectShowLib;
using NAudio.CoreAudioApi;

namespace Axicultus.Wpf.Media;

internal sealed record InputDevice(string Id, string Name, int CameraNumber = 0, bool IsDefault = false)
{
    public string DisplayName => IsDefault ? UiText.Format("DeviceCheckBackend", "WindowsDefault", Name) : Name;
}

internal sealed record MicrophoneReading(bool Muted, float Peak)
{
    public int Percent => Muted || !float.IsFinite(Peak) ? 0 : (int)Math.Round(Math.Clamp(Peak, 0, 1) * 100);
    public string Status => Muted ? UiText.Get("DeviceCheckBackend", "Muted") : Percent == 0 ? UiText.Get("DeviceCheckBackend", "WaitingForInput") : UiText.Get("DeviceCheckBackend", "SignalDetected");
}

internal interface IDeviceCheckBackend
{
    InputDevice[] Cameras();
    InputDevice[] Microphones();
    IDisposable StartCamera(InputDevice device, Image target, Action<string> signal, Action frame);
    void MonitorMicrophone(string id, Action<MicrophoneReading> report, CancellationToken stop);
}

internal sealed class DeviceCheckBackend : IDeviceCheckBackend
{
    public InputDevice[] Cameras()
    {
        var devices = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);
        try
        {
            return [.. devices.Select((device, i) => new InputDevice(device.DevicePath, device.Name, i + 1)).Where(device => !string.IsNullOrEmpty(device.Id))];
        }
        finally { foreach (var device in devices) device.Dispose(); }
    }

    public InputDevice[] Microphones() => AudioEndpoints(DataFlow.Capture);
    internal static InputDevice[] Speakers() => AudioEndpoints(DataFlow.Render);

    private static InputDevice[] AudioEndpoints(DataFlow flow)
    {
        using var enumerator = new MMDeviceEnumerator();
        using var preferred = enumerator.HasDefaultAudioEndpoint(flow, Role.Console)
            ? enumerator.GetDefaultAudioEndpoint(flow, Role.Console) : null;
        var defaultId = preferred?.ID;
        var list = new List<InputDevice>();
        foreach (var endpoint in enumerator.EnumerateAudioEndPoints(flow, DeviceState.Active))
        {
            using (endpoint)
                list.Add(new(endpoint.ID, endpoint.FriendlyName, IsDefault: endpoint.ID == defaultId));
        }
        return [.. list];
    }

    public IDisposable StartCamera(InputDevice device, Image target, Action<string> signal, Action frame)
    {
        var capture = new DirectShowVideo(target, device.CameraNumber, device.Id)
        {
            SignalChanged = signal,
            FramePresented = frame
        };
        try { capture.Play(); return capture; }
        catch { capture.Dispose(); throw; }
    }

    // Called on one worker thread so the WASAPI packet and all its COM resources share a lifetime.
    public void MonitorMicrophone(string id, Action<MicrophoneReading> report, CancellationToken stop)
    {
        stop.ThrowIfCancellationRequested();
        using var enumerator = new MMDeviceEnumerator();
        using var device = enumerator.GetDevice(id);
        using var client = device.AudioClient;
        client.Initialize(AudioClientShareMode.Shared, AudioClientStreamFlags.None,
            TimeSpan.FromMilliseconds(300).Ticks, 0, client.MixFormat, Guid.Empty);
        var packets = client.AudioCaptureClient;
        var meter = device.AudioMeterInformation;
        var volume = device.AudioEndpointVolume;
        stop.ThrowIfCancellationRequested();
        client.Start();
        try
        {
            do
            {
                if (device.State != DeviceState.Active) throw new IOException(UiText.Get("DeviceCheckBackend", "MicDisconnected"));
                while (!stop.IsCancellationRequested && packets.GetNextPacketSize() > 0)
                {
                    packets.GetBuffer(out int frames, out _);
                    packets.ReleaseBuffer(frames);
                }
                if (!stop.IsCancellationRequested) report(new(volume.Mute, meter.MasterPeakValue));
            } while (!stop.WaitHandle.WaitOne(125));
        }
        finally { client.Stop(); }
    }

    internal static string Failure(Exception error) => error.HResult switch
    {
        unchecked((int)0x80070005) => UiText.Get("DeviceCheckBackend", "AccessDenied"),
        unchecked((int)0x8889000A) => UiText.Get("DeviceCheckBackend", "DeviceInUse"),
        unchecked((int)0x88890004) or unchecked((int)0x80070490) => UiText.Get("DeviceCheckBackend", "DeviceGone"),
        _ => error.Message
    };
}
