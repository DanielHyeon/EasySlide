using System;
using System.Threading;
using System.Threading.Tasks;
using Axicultus.Wpf.Interop;
using Axicultus.Wpf.Platform;
using Axicultus.Wpf.Shell;

namespace Axicultus.Wpf.Rendering;

public enum PowerPointSlideShowTarget
{
    Preview,
    Output,
}

public sealed record PowerPointSlideShowRequest(
    string FilePath,
    int SlideNumber,
    PowerPointSlideShowTarget Target);

public sealed record PowerPointOutputPosition(int Slide, int Count, OfficeLib.SlideImageManifest.SourceVersion? SourceVersion);

public interface IPowerPointSlideShowControl
{
    Task StartAsync(
        PowerPointSlideShowRequest request,
        string outputMonitorName,
        CancellationToken cancellationToken = default);

    Task TriggerNextAsync(
        PowerPointSlideShowRequest request,
        CancellationToken cancellationToken = default);

    Task StopAsync(CancellationToken cancellationToken = default);
    Task<int?> ReadOutputPositionAsync(string path) => Task.FromResult<int?>(null);
    async Task<PowerPointOutputPosition?> PollOutputAsync(string path) =>
        await ReadOutputPositionAsync(path).ConfigureAwait(false) is int slide ? new(slide, 0, null) : null;
    bool HasOutputShow => false;
    Task SetBlackoutAsync(bool black) => Task.CompletedTask;
    bool OwnsOutputWindow(nint window) => false;
    void RevealOutputAfterMedia(OutputWindowPlacement bounds, bool playerHadFocus) { }
    Task<int?> AdvanceOutputAsync(string path) => Task.FromResult<int?>(null);
}

public sealed class NullPowerPointSlideShowControl : IPowerPointSlideShowControl
{
    public static NullPowerPointSlideShowControl Instance { get; } = new();

    private NullPowerPointSlideShowControl()
    {
    }

    public Task StartAsync(
        PowerPointSlideShowRequest request,
        string outputMonitorName,
        CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public Task TriggerNextAsync(
        PowerPointSlideShowRequest request,
        CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public Task StopAsync(CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}

public sealed class OfficePowerPointSlideShowControl : IPowerPointSlideShowControl, IDisposable
{
    private readonly Lazy<OfficePptSession> _session;
    private readonly IDisplayService? _displays;
    private readonly IWindowPlacementService? _placement;
    private long _operatorAction;
    private OfficePptSession.OutputShow? Output => _session.IsValueCreated ? _session.Value.CurrentOutput : null;

    public Task<int?> ReadOutputPositionAsync(string path) =>
        Output is { } show && string.Equals(System.IO.Path.GetFullPath(path), show.Path, StringComparison.OrdinalIgnoreCase)
            ? _session.Value.ReadShowPositionAsync(show.Path)
            : Task.FromResult<int?>(null);

    public bool HasOutputShow => Output != null;
    public Task<PowerPointOutputPosition?> PollOutputAsync(string path)
    {
        long action = Volatile.Read(ref _operatorAction);
        return Output is { } show && string.Equals(System.IO.Path.GetFullPath(path), show.Path, StringComparison.OrdinalIgnoreCase)
            ? _session.Value.PollOutputAsync(show, () => Volatile.Read(ref _operatorAction) == action)
            : Task.FromResult<PowerPointOutputPosition?>(null);
    }
    public Task SetBlackoutAsync(bool black)
    {
        Interlocked.Increment(ref _operatorAction);
        return Output is { } show ? _session.Value.SetBlackoutAsync(show.Path, black) : Task.CompletedTask;
    }

    public bool OwnsOutputWindow(nint window) => window != 0 && Output?.Handle == window;

    public void RevealOutputAfterMedia(OutputWindowPlacement bounds, bool playerHadFocus)
    {
        if (Output is { Handle: not 0 } show)
            DesktopWindowPlacement.RevealSlideShow(show.Handle, bounds, playerHadFocus);
    }

    public Task<int?> AdvanceOutputAsync(string path)
    {
        Interlocked.Increment(ref _operatorAction);
        return Output is { } show && string.Equals(System.IO.Path.GetFullPath(path), show.Path, StringComparison.OrdinalIgnoreCase)
            ? _session.Value.AdvanceShowAsync(show.Path) : Task.FromResult<int?>(null);
    }

    public OfficePowerPointSlideShowControl(IDisplayService? displays = null, IWindowPlacementService? placement = null)
        : this(static () => new OfficePptSession())
    {
        _displays = displays;
        _placement = placement;
    }

    internal OfficePowerPointSlideShowControl(Func<OfficePptSession> sessionFactory)
        => _session = new Lazy<OfficePptSession>(sessionFactory);

    public Task StartAsync(
        PowerPointSlideShowRequest request,
        string outputMonitorName,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        Interlocked.Increment(ref _operatorAction);
        return StartCoreAsync(request, outputMonitorName, cancellationToken);
    }

    private async Task StartCoreAsync(
        PowerPointSlideShowRequest request,
        string outputMonitorName,
        CancellationToken cancellationToken)
    {
        bool isOutput = request.Target == PowerPointSlideShowTarget.Output;
        OutputWindowPlacement? bounds = isOutput && _displays != null && _placement != null
            ? _placement.CreateOutputPlacement(_displays.GetPreferredDisplay(outputMonitorName), windowed: false)
            : null;
        var started = await _session.Value.StartSlideShowAsync(
            request.FilePath,
            request.SlideNumber,
            outputMonitorName,
            cancellationToken, isOutput, bounds, bounds != null && DesktopWindowPlacement.IsSeparateAudienceMonitor(bounds)).ConfigureAwait(false);
        if (!started)
        {
            throw new InvalidOperationException("PowerPoint slide show window was not created.");
        }
    }

    public Task TriggerNextAsync(
        PowerPointSlideShowRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        Interlocked.Increment(ref _operatorAction);
        return TriggerNextCoreAsync(request, cancellationToken);
    }

    private async Task TriggerNextCoreAsync(
        PowerPointSlideShowRequest request,
        CancellationToken cancellationToken)
    {
        var triggered = await _session.Value.TriggerSlideShowNextAsync(
            request.FilePath,
            request.SlideNumber,
            cancellationToken, request.Target == PowerPointSlideShowTarget.Output).ConfigureAwait(false);
        if (!triggered)
        {
            throw new InvalidOperationException("PowerPoint slide show window was not available.");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Interlocked.Increment(ref _operatorAction);
        return _session.IsValueCreated
            ? _session.Value.CloseAsync()
            : Task.CompletedTask;
    }

    public void Dispose()
    {
        Interlocked.Increment(ref _operatorAction);
        if (_session.IsValueCreated)
        {
            _session.Value.Dispose();
        }
    }
}
