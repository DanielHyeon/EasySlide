using System;
using System.Diagnostics;
using System.Windows.Threading;
using LegacyKeyEventArgs = System.Windows.Forms.KeyEventArgs;
using LegacyKeys = System.Windows.Forms.Keys;

namespace Easislides.Wpf.Input;

public sealed class GlobalKeyEventArgs : EventArgs
{
    public GlobalKeyEventArgs(LegacyKeys keyCode, LegacyKeys modifiers)
    {
        KeyCode = keyCode;
        Modifiers = modifiers;
    }

    public LegacyKeys KeyCode { get; }
    public LegacyKeys Modifiers { get; }
    public bool Handled { get; set; }
}

public interface IGlobalKeySource
{
    event EventHandler<GlobalKeyEventArgs>? KeyDown;

    void Start();
    void Stop();
}

public interface IGlobalInputDispatcher
{
    void Invoke(Action action);
}

public interface IGlobalInputService : IDisposable
{
    bool IsRunning { get; }
    Exception? LastError { get; }

    bool Start();
    void Stop();
}

public sealed class GlobalInputService : IGlobalInputService
{
    private readonly ShortcutRegistry _registry;
    private readonly IGlobalKeySource _source;
    private readonly IGlobalInputDispatcher _dispatcher;
    private bool _subscribed;
    private bool _disposed;

    public GlobalInputService(
        ShortcutRegistry registry,
        IGlobalKeySource source,
        IGlobalInputDispatcher dispatcher)
    {
        _registry = registry;
        _source = source;
        _dispatcher = dispatcher;
    }

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
            _source.KeyDown += OnKeyDown;
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
        if (_subscribed)
        {
            _source.KeyDown -= OnKeyDown;
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
        try
        {
            _dispatcher.Invoke(() =>
            {
                e.Handled = _registry.TryHandleGlobal(e.KeyCode, e.Modifiers);
            });
        }
        catch (Exception ex)
        {
            LastError = ex;
            e.Handled = false;
            Debug.WriteLine($"[GlobalInputService] key handling failed: {ex}");
        }
    }

    private void CleanupAfterStartFailure()
    {
        if (_subscribed)
        {
            _source.KeyDown -= OnKeyDown;
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

    public event EventHandler<GlobalKeyEventArgs>? KeyDown;

    public void Start()
    {
        if (_running)
        {
            return;
        }

        Easislides.HookManager.KeyDown += OnHookKeyDown;
        _running = true;
    }

    public void Stop()
    {
        if (!_running)
        {
            return;
        }

        Easislides.HookManager.KeyDown -= OnHookKeyDown;
        _running = false;
    }

    private void OnHookKeyDown(object sender, LegacyKeyEventArgs e)
    {
        var args = new GlobalKeyEventArgs(e.KeyCode, e.Modifiers);
        KeyDown?.Invoke(this, args);
        e.Handled = args.Handled;
    }
}

public sealed class WpfGlobalInputDispatcher : IGlobalInputDispatcher
{
    private readonly Dispatcher _dispatcher;

    public WpfGlobalInputDispatcher(Dispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    public void Invoke(Action action)
    {
        if (_dispatcher.CheckAccess())
        {
            action();
            return;
        }

        _dispatcher.Invoke(action);
    }
}
