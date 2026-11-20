using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using NAudio.CoreAudioApi;

namespace Easislides.Wpf.Media;

internal interface ILiveSpeakerFeed : IDisposable
{
    Task Ready { get; }
    Task Ended { get; }
    void Enable();
}

// Open both shared endpoints on one MTA worker. Discard input until the operator's route is committed.
internal sealed class LiveSpeakerFeed : ILiveSpeakerFeed
{
    private readonly TaskCompletionSource _ready = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly CancellationTokenSource _stop = new();
    private readonly object _lifetime = new();
    private bool _finished;
    private volatile bool _enabled;
    public Task Ready => _ready.Task;
    public Task Ended { get; }

    internal LiveSpeakerFeed(string microphone, string speaker)
    {
        Ended = Task.Run(() =>
        {
            try { Run(microphone, speaker, _stop.Token); }
            catch (Exception ex) { _ready.TrySetException(ex); throw; }
            finally
            {
                _ready.TrySetCanceled();
                lock (_lifetime) { _finished = true; _stop.Dispose(); }
            }
        });
    }

    public void Enable() => _enabled = true;
    public void Dispose()
    {
        _enabled = false;
        lock (_lifetime) { if (!_finished) _stop.Cancel(); }
    }

    private void Run(string microphone, string speaker, CancellationToken ct)
    {
        using var devices = new MMDeviceEnumerator();
        using var input = devices.GetDevice(microphone);
        using var output = devices.GetDevice(speaker);
        using var source = input.AudioClient;
        using var sink = output.AudioClient;
        var format = source.MixFormat;
        long duration = TimeSpan.FromMilliseconds(50).Ticks;
        source.Initialize(AudioClientShareMode.Shared, AudioClientStreamFlags.None, duration, 0, format, Guid.Empty);
        sink.Initialize(AudioClientShareMode.Shared, AudioClientStreamFlags.AutoConvertPcm | AudioClientStreamFlags.SrcDefaultQuality,
            duration, 0, format, Guid.Empty);
        var capture = source.AudioCaptureClient;
        var render = sink.AudioRenderClient;
        int capacity = sink.BufferSize;
        byte[] samples = new byte[checked(capacity * format.BlockAlign)];
        ct.ThrowIfCancellationRequested();
        bool capturing = false;
        sink.Start();
        try
        {
            source.Start();
            capturing = true;
            do
            {
                while (!ct.IsCancellationRequested && capture.GetNextPacketSize() > 0)
                {
                    var data = capture.GetBuffer(out int frames, out var flags);
                    try
                    {
                        if (frames == 0) continue;
                        int count = Math.Min(frames, capacity - sink.CurrentPadding);
                        if (count <= 0) continue;
                        var destination = render.GetBuffer(count);
                        var silent = !_enabled || flags.HasFlag(AudioClientBufferFlags.Silent);
                        try
                        {
                            if (!silent)
                            {
                                int length = count * format.BlockAlign;
                                Marshal.Copy(IntPtr.Add(data, (frames - count) * format.BlockAlign), samples, 0, length);
                                Marshal.Copy(samples, 0, destination, length);
                            }
                        }
                        finally { render.ReleaseBuffer(count, silent ? AudioClientBufferFlags.Silent : AudioClientBufferFlags.None); }
                        _ready.TrySetResult();
                    }
                    finally { capture.ReleaseBuffer(frames); }
                }
            } while (!ct.WaitHandle.WaitOne(5));
        }
        finally { try { if (capturing) source.Stop(); } finally { sink.Stop(); } }
    }
}
