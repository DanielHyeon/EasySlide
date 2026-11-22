using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Axicultus.Wpf.Support;

internal sealed class OperationalErrorLog(string directory)
{
    internal static OperationalErrorLog Default { get; } = new(Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Axicultus", "Logs"));

    private readonly Lock _gate = new();

    internal void Write(string operation, object exception)
    {
        try
        {
            var now = DateTimeOffset.UtcNow;
            var entry = $"{now:O} [{operation}] {exception}{Environment.NewLine}";
            lock (_gate)
            {
                Directory.CreateDirectory(directory);
                var path = Path.Combine(directory, $"errors-{now:yyyyMMdd}-{Environment.ProcessId}.log");
                File.AppendAllText(path, entry);
            }
        }
        catch (Exception writeError)
        {
            // Reporting an error must preserve the original recovery or termination path.
            Debug.WriteLine(writeError);
        }
    }

    /// <summary>
    /// Set by the shell: tells the operator about the error and returns true while a show is on the audience screen.
    /// </summary>
    internal Func<Exception, bool>? ReportDuringShow { get; set; }

    // A failure that keeps repeating (a render timer, a layout pass) would otherwise write the log and the
    // operator message on the UI thread many times a second for the rest of the service: a frozen console.
    internal const int MaxHandledPerWindow = 5;
    internal static readonly TimeSpan HandledWindow = TimeSpan.FromSeconds(10);
    private long _windowStart = Environment.TickCount64;
    private int _inWindow;

    internal void OnUiException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        long now = Environment.TickCount64;
        if (now - _windowStart > HandledWindow.TotalMilliseconds)
        {
            _windowStart = now;
            _inWindow = 0;
        }

        if (++_inWindow > MaxHandledPerWindow)
        {
            // Runaway failure: one line, then WPF's default termination policy as without a show.
            if (_inWindow == MaxHandledPerWindow + 1)
                Write("Dispatcher", $"More than {MaxHandledPerWindow} errors within {HandledWindow.TotalSeconds:0} s; no longer handled. Last: {e.Exception.GetType().FullName}: {e.Exception.Message}");
            return;
        }

        Write("Dispatcher", e.Exception);
        // A running show must stay on the audience screen. Without one, Handled stays unset for WPF's default
        // termination policy.
        try
        {
            if (ReportDuringShow?.Invoke(e.Exception) == true) e.Handled = true;
        }
        catch (Exception reportError)
        {
            Write("Dispatcher report", reportError);
        }
    }

    internal void OnDomainException(object? sender, UnhandledExceptionEventArgs e)
        => Write($"AppDomain IsTerminating={e.IsTerminating}", e.ExceptionObject);

    internal void OnTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        Write("TaskScheduler", e.Exception);
        e.SetObserved();
    }
}
