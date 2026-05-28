using System;
using Easislides.Wpf.Rendering;
using Easislides.Wpf.Settings;

namespace Easislides.Wpf.Shell;

public delegate IOutputSurface OutputSurfaceFactory();

public interface IOutputSurface
{
    void Bind(OutputWindowViewModel viewModel);
    void ApplyPlacement(OutputWindowPlacement placement);
    void Show();
    void Close();
}

public interface IOutputWindowHost : IDisposable
{
}

public sealed class OutputWindowHost : IOutputWindowHost
{
    private readonly IOutputWindowService _output;
    private readonly ILiveSessionService _session;
    private readonly OutputSurfaceFactory _surfaceFactory;
    private readonly IOutputRenderer _renderer;
    private readonly ISettingsService? _settings;
    private IOutputSurface? _surface;
    private OutputWindowViewModel? _viewModel;
    private LiveSessionSnapshot _lastSession;
    private bool _disposed;

    public OutputWindowHost(
        IOutputWindowService output,
        ILiveSessionService session,
        OutputSurfaceFactory surfaceFactory)
        : this(
            output,
            session,
            surfaceFactory,
            new OutputRenderer(new ImageAssetService(), new TransitionEffectService()),
            settings: null)
    {
    }

    public OutputWindowHost(
        IOutputWindowService output,
        ILiveSessionService session,
        OutputSurfaceFactory surfaceFactory,
        IOutputRenderer renderer)
        : this(output, session, surfaceFactory, renderer, settings: null)
    {
    }

    public OutputWindowHost(
        IOutputWindowService output,
        ILiveSessionService session,
        OutputSurfaceFactory surfaceFactory,
        IOutputRenderer renderer,
        ISettingsService? settings)
    {
        _output = output ?? throw new ArgumentNullException(nameof(output));
        _session = session ?? throw new ArgumentNullException(nameof(session));
        _surfaceFactory = surfaceFactory ?? throw new ArgumentNullException(nameof(surfaceFactory));
        _renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
        _settings = settings;
        _lastSession = session.Current;

        _output.OutputChanged += OnOutputChanged;
        _session.SessionChanged += OnSessionChanged;

        ApplyOutput(_output.Current);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _output.OutputChanged -= OnOutputChanged;
        _session.SessionChanged -= OnSessionChanged;
        CloseSurface();
    }

    private void OnOutputChanged(object? sender, OutputWindowChangedEventArgs e)
        => ApplyOutput(e.State);

    private void OnSessionChanged(object? sender, LiveSessionChangedEventArgs e)
    {
        _lastSession = e.Snapshot;
        _viewModel?.ApplySession(e.Snapshot);
    }

    private void ApplyOutput(OutputWindowState state)
    {
        if (!state.IsOpen)
        {
            _viewModel?.ApplyOutput(state);
            CloseSurface();
            return;
        }

        EnsureSurface();
        _viewModel!.ApplyOutput(state);
        _surface!.ApplyPlacement(state.Placement);
        _surface.Show();
    }

    private void EnsureSurface()
    {
        if (_surface is not null)
        {
            return;
        }

        _viewModel = new OutputWindowViewModel(_renderer, _settings);
        _viewModel.ApplySession(_lastSession);

        _surface = _surfaceFactory();
        _surface.Bind(_viewModel);
    }

    private void CloseSurface()
    {
        _surface?.Close();
        _viewModel?.Dispose();
        _surface = null;
        _viewModel = null;
    }

}
