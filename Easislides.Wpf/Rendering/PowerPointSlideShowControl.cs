using System;
using System.Threading;
using System.Threading.Tasks;
using Easislides.Wpf.Interop;

namespace Easislides.Wpf.Rendering;

public enum PowerPointSlideShowTarget
{
    Preview,
    Output,
}

public sealed record PowerPointSlideShowRequest(
    string FilePath,
    int SlideNumber,
    PowerPointSlideShowTarget Target);

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

    public OfficePowerPointSlideShowControl()
        : this(static () => new OfficePptSession())
    {
    }

    internal OfficePowerPointSlideShowControl(Func<OfficePptSession> sessionFactory)
        => _session = new Lazy<OfficePptSession>(sessionFactory);

    public Task StartAsync(
        PowerPointSlideShowRequest request,
        string outputMonitorName,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        return StartCoreAsync(request, outputMonitorName, cancellationToken);
    }

    private async Task StartCoreAsync(
        PowerPointSlideShowRequest request,
        string outputMonitorName,
        CancellationToken cancellationToken)
    {
        var started = await _session.Value.StartSlideShowAsync(
            request.FilePath,
            request.SlideNumber,
            outputMonitorName,
            cancellationToken).ConfigureAwait(false);
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
        return TriggerNextCoreAsync(request, cancellationToken);
    }

    private async Task TriggerNextCoreAsync(
        PowerPointSlideShowRequest request,
        CancellationToken cancellationToken)
    {
        var triggered = await _session.Value.TriggerSlideShowNextAsync(
            request.FilePath,
            request.SlideNumber,
            cancellationToken).ConfigureAwait(false);
        if (!triggered)
        {
            throw new InvalidOperationException("PowerPoint slide show window was not available.");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return _session.IsValueCreated
            ? _session.Value.CloseAsync()
            : Task.CompletedTask;
    }

    public void Dispose()
    {
        if (_session.IsValueCreated)
        {
            _session.Value.Dispose();
        }
    }
}
