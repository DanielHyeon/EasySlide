using System;
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
    private IOutputSurface? _surface;
    private OutputWindowViewModel? _viewModel;
    private LiveSessionSnapshot _lastSession;
    private bool _disposed;

    public OutputWindowHost(
        IOutputWindowService output,
        ILiveSessionService session,
        OutputSurfaceFactory surfaceFactory)
    {
        _output = output;
        _session = session;
        _surfaceFactory = surfaceFactory;
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

        _viewModel = new OutputWindowViewModel();
        _viewModel.ApplySession(_lastSession);

        _surface = _surfaceFactory();
        _surface.Bind(_viewModel);
    }

    private void CloseSurface()
    {
        _surface?.Close();
        _surface = null;
        _viewModel = null;
    }

}
