using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Axicultus.Wpf.Captions;
using Axicultus.Wpf.Localization;

namespace Axicultus.Wpf.Media;

// All route changes are initiated on the operator dispatcher. Only an explicitly selected OBS source is managed.
internal sealed class LiveMicrophoneRouter(Func<string, string, ILiveSpeakerFeed> openSpeaker) : IDisposable
{
    private ILiveSpeakerFeed? _speaker;
    private ObsCaptureInput? _obs;
    private readonly HashSet<(int, string)> _mutedByRoute = [];
    internal (string Mic, string Speaker)? LocalSelection { get; private set; }
    internal bool HasObsSelection => _obs != null;
    internal bool IsSelected(ObsCaptureInput input) => _obs?.Key == input.Key;
    internal event Action<string>? Failed;

    internal async Task<string> ToSpeakerAsync(InputDevice mic, InputDevice speaker, ObsConnection? connection, CancellationToken ct)
    {
        if (LocalSelection == (mic.Id, speaker.Id)) return $"{mic.Name} → {speaker.Name}";
        var next = openSpeaker(mic.Id, speaker.Id);
        bool committed = false;
        ObsCaptureInput? newlyMuted = null;
        _ = ObserveAsync(next);
        try
        {
            try { await next.Ready.WaitAsync(TimeSpan.FromSeconds(5), ct); }
            catch (TimeoutException) { throw new MediaPlaybackException(UiText.Get("LiveMicrophoneRouter", "NoInput")); }
            bool obsUnavailable = false;
            if (_obs != null)
            {
                if (connection == null || !connection.IsIdentified) obsUnavailable = true;
                else
                {
                    var inputs = await connection.CaptureInputsAsync(false, ct);
                    var previous = Array.Find(inputs, input => input.Key == _obs.Key);
                    if (previous == null) obsUnavailable = true;
                    else if (await SilenceAsync(previous, ct)) newlyMuted = previous;
                }
            }
            ct.ThrowIfCancellationRequested();
            if (next.Ended.IsCompleted) { await next.Ended; throw new MediaPlaybackException(UiText.Get("LiveMicrophoneRouter", "NewMicStopped")); }
            StopSpeaker();
            _speaker = next;
            LocalSelection = (mic.Id, speaker.Id);
            _obs = null;
            next.Enable();
            committed = true;
            return $"{mic.Name} → {speaker.Name}" + (obsUnavailable ? UiText.Get("LiveMicrophoneRouter", "CheckPreviousObsMute") : "");
        }
        finally
        {
            if (!committed)
            {
                next.Dispose();
                if (newlyMuted != null) await RestoreAsync(newlyMuted);
            }
        }
    }

    internal async Task<string> ToObsAsync(ObsConnection connection, ObsCaptureInput input, ObsDeviceChoice device, CancellationToken ct)
    {
        await input.SetDeviceAsync(device.Id, ct);
        bool muted = await input.IsMutedAsync(ct);
        ObsCaptureInput? previous = null;
        bool previousUnavailable = false;
        try
        {
            if (_obs != null && _obs.Key != input.Key)
            {
                var previousInput = _obs.SameSession(input) ? _obs :
                    Array.Find(await connection.CaptureInputsAsync(false, ct), candidate => candidate.Key == _obs.Key);
                if (previousInput == null) previousUnavailable = true;
                else if (await SilenceAsync(previousInput, ct)) previous = previousInput;
            }
            ct.ThrowIfCancellationRequested();
            if (_mutedByRoute.Contains(input.Key))
            {
                if (muted) await input.SetMutedAsync(false, ct);
                muted = false;
                _mutedByRoute.Remove(input.Key);
            }
        }
        catch
        {
            if (previous != null) await RestoreAsync(previous);
            throw;
        }
        StopSpeaker();
        _obs = input;
        return $"{device.Name} → OBS · {input.Name}" + (muted ? UiText.Get("LiveMicrophoneRouter", "MutedSuffix") : "") +
            (previousUnavailable ? UiText.Get("LiveMicrophoneRouter", "CheckPreviousObsMute") : "");
    }

    private async Task<bool> SilenceAsync(ObsCaptureInput input, CancellationToken ct)
    {
        if (await input.IsMutedAsync(ct)) return false;
        await input.SetMutedAsync(true, ct);
        _mutedByRoute.Add(input.Key);
        return true;
    }

    private async Task RestoreAsync(ObsCaptureInput input)
    {
        await input.SetMutedAsync(false, CancellationToken.None);
        _mutedByRoute.Remove(input.Key);
    }

    private async Task ObserveAsync(ILiveSpeakerFeed feed)
    {
        try { await feed.Ended; }
        catch (Exception ex)
        {
            if (ReferenceEquals(feed, _speaker))
            {
                StopSpeaker();
                Failed?.Invoke(UiText.Format("LiveMicrophoneRouter", "MicDisconnected", ex.Message));
            }
        }
    }

    internal void StopSpeaker()
    {
        var previous = _speaker;
        _speaker = null;
        LocalSelection = null;
        previous?.Dispose();
    }
    public void Dispose() => StopSpeaker();
}
