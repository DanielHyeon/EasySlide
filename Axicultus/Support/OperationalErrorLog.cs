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

    internal void OnUiException(object sender, DispatcherUnhandledExceptionEventArgs e)
        => Write("Dispatcher", e.Exception); // Leave Handled unset for WPF's default termination policy.

    internal void OnDomainException(object? sender, UnhandledExceptionEventArgs e)
        => Write($"AppDomain IsTerminating={e.IsTerminating}", e.ExceptionObject);

    internal void OnTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        Write("TaskScheduler", e.Exception);
        e.SetObserved();
    }
}
