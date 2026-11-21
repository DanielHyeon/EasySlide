using static Axicultus.Interop.DesktopWindows;
using System;
using System.Diagnostics;
using System.Windows.Threading;
using LegacyKeyEventArgs = System.Windows.Forms.KeyEventArgs;
using LegacyKeys = System.Windows.Forms.Keys;

namespace Axicultus.Wpf.Input;

public sealed class GlobalKeyEventArgs(LegacyKeys keyCode, LegacyKeys modifiers, bool externalWindow = false, nint window = 0) : EventArgs
{
    public LegacyKeys KeyCode { get; } = keyCode;
    public LegacyKeys Modifiers { get; } = modifiers;
    public nint Window { get; } = window;
    public bool ExternalWindow { get; } = externalWindow;
    public bool Handled { get; set; }
}

public interface IGlobalKeySource
{
    event EventHandler<GlobalKeyEventArgs>? ShortcutPressed;

    void Start();
    void Stop();
}

public interface IGlobalInputDispatcher
{
    void Post(Action action);
}

public interface IGlobalInputService : IDisposable
{
    bool IsRunning { get; }
    Exception? LastError { get; }

    bool Start();
    void Stop();
}

public sealed class GlobalInputService(
    ShortcutRegistry registry,
    IGlobalKeySource source,
    IGlobalInputDispatcher dispatcher) : IGlobalInputService
{
    private readonly ShortcutRegistry _registry = registry;
    private readonly IGlobalKeySource _source = source;
    private readonly IGlobalInputDispatcher _dispatcher = dispatcher;
    private bool _subscribed;
    private bool _disposed;
    private int _session;

    public bool IsRunning { get; private set; }
    public Exception? LastError { get; private set; }

    public bool Start()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (IsRunning)
        {
            return true;
        }

        try
        {
            _source.ShortcutPressed += OnKeyDown;
            _subscribed = true;
            _source.Start();
            IsRunning = true;
            LastError = null;
            return true;
        }
        catch (Exception ex)
        {
            LastError = ex;
            Debug.WriteLine($"[GlobalInputService] start failed: {ex}");
            CleanupAfterStartFailure();
            return false;
        }
    }

    public void Stop()
    {
        _session++;
        if (_subscribed)
        {
            _source.ShortcutPressed -= OnKeyDown;
            _subscribed = false;
        }

        try
        {
            _source.Stop();
        }
        catch (Exception ex)
        {
            LastError = ex;
            Debug.WriteLine($"[GlobalInputService] stop failed: {ex}");
        }
        finally
        {
            IsRunning = false;
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        Stop();
        _disposed = true;
    }

    private void OnKeyDown(object? sender, GlobalKeyEventArgs e)
    {
        if (!IsRunning) return;
        bool showSpace = e.KeyCode == LegacyKeys.Space && e.Modifiers == LegacyKeys.None &&
            _registry.IsPowerPointOutputWindow?.Invoke(e.Window) == true;
        Action? command;
        bool global = showSpace ? _registry.TryGetHandler(Shell.MainCommandIds.OutputSpace, out command)
            : _registry.TryResolveGlobal(e.KeyCode, e.Modifiers, out command);
        if (!global && (!e.ExternalWindow || !_registry.TryResolveMedia(e.KeyCode, e.Modifiers, out command))) return;

        try
        {
            PostCommand(command!);
            e.Handled = global && (!e.ExternalWindow || e.KeyCode is not (LegacyKeys.F7 or LegacyKeys.F8 or LegacyKeys.Up or LegacyKeys.Down));
        }
        catch (Exception ex)
        {
            RecordInputError(ex);
        }
    }

    private void PostCommand(Action command)
    {
        int session = _session;
        _dispatcher.Post(() =>
        {
            if (!IsRunning || session != _session) return;
            try { command(); }
            catch (Exception ex) { RecordInputError(ex); }
        });
    }

    private void RecordInputError(Exception ex)
    {
        LastError = ex;
        Debug.WriteLine($"[GlobalInputService] key handling failed: {ex}");
    }

    private void CleanupAfterStartFailure()
    {
        if (_subscribed)
        {
            _source.ShortcutPressed -= OnKeyDown;
            _subscribed = false;
        }

        try
        {
            _source.Stop();
        }
        catch
        {
            // Start already failed; preserve the original LastError.
        }

        IsRunning = false;
    }
}

public sealed class HookManagerGlobalKeySource : IGlobalKeySource
{
    private bool _running;

    public event EventHandler<GlobalKeyEventArgs>? ShortcutPressed;

    public void Start()
    {
        if (_running)
        {
            return;
        }

        Axicultus.HookManager.KeyDown += OnHookKeyDown;
        Axicultus.HookManager.KeyUp += OnHookKeyUp;
        _running = true;
    }

    public void Stop()
    {
        // Removing an absent handler is safe and retries a previous native release failure.
        _running = false;
        Axicultus.HookManager.KeyDown -= OnHookKeyDown;
        Axicultus.HookManager.KeyUp -= OnHookKeyUp;
    }

    private void OnHookKeyDown(object? sender, LegacyKeyEventArgs e)
    {
        if (e.KeyCode is not (LegacyKeys.Up or LegacyKeys.Down)) Forward(e, arrow: false);
    }

    private void OnHookKeyUp(object? sender, LegacyKeyEventArgs e)
    {
        if (e.KeyCode is LegacyKeys.Up or LegacyKeys.Down) Forward(e, arrow: true);
    }

    private void Forward(LegacyKeyEventArgs e, bool arrow)
    {
        if (e.Handled) return;
        nint foreground = 0;
        bool external = false;
        if (arrow || e.KeyCode is LegacyKeys.Escape or LegacyKeys.Space or LegacyKeys.Enter or LegacyKeys.S or LegacyKeys.M or LegacyKeys.F7 or LegacyKeys.F8)
        {
            foreground = Windows.Win32.PInvoke.GetForegroundWindow();
            if (foreground == 0 || GetWindowThreadProcessId(foreground, out var process) == 0) return;
            external = process != Environment.ProcessId;
            // Shell, output and modal windows handle their own Esc and arrow releases.
            if (!external && (arrow || e.KeyCode == LegacyKeys.Escape)) return;
        }
        var args = new GlobalKeyEventArgs(e.KeyCode, e.Modifiers, external, foreground);
        ShortcutPressed?.Invoke(this, args);
        e.Handled |= args.Handled;
    }
}

public sealed class WpfGlobalInputDispatcher(Dispatcher dispatcher) : IGlobalInputDispatcher
{
    private readonly Dispatcher _dispatcher = dispatcher;

    public void Post(Action action)
    {
        if (_dispatcher.HasShutdownStarted || _dispatcher.HasShutdownFinished)
            throw new InvalidOperationException("The input dispatcher is shutting down.");

        var operation = _dispatcher.BeginInvoke(DispatcherPriority.Input, action);
        if (operation.Status == DispatcherOperationStatus.Aborted)
            throw new InvalidOperationException("The input dispatcher rejected the command.");
    }
}
