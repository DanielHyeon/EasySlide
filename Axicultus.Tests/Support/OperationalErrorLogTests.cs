using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Threading;
using Axicultus.Wpf.Support;
using Xunit;

namespace Axicultus.Wpf.Tests.Support;

public sealed class OperationalErrorLogTests
{
    [Fact]
    public Task UnexpectedDispatcherErrorIsLoggedAndLeftUnhandled() => StaHelper.RunOnStaAsync(async () =>
    {
        var directory = Directory.CreateTempSubdirectory();
        var log = new OperationalErrorLog(directory.FullName);
        var dispatcher = Dispatcher.CurrentDispatcher;
        var observed = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        void Observe(object sender, DispatcherUnhandledExceptionEventArgs args)
        {
            observed.SetResult(args.Handled);
            args.Handled = true; // Only the test keeps this dispatcher running.
        }

        dispatcher.UnhandledException += log.OnUiException;
        dispatcher.UnhandledException += Observe;
        try
        {
            _ = dispatcher.BeginInvoke(new Action(() => throw new InvalidOperationException("test UI failure")));
            Assert.False(await observed.Task.WaitAsync(TimeSpan.FromSeconds(5)));
            string entry = File.ReadAllText(Assert.Single(directory.GetFiles("*.log")).FullName);
            Assert.Contains("Dispatcher", entry);
            Assert.Contains("InvalidOperationException: test UI failure", entry);
        }
        finally
        {
            dispatcher.UnhandledException -= Observe;
            dispatcher.UnhandledException -= log.OnUiException;
            directory.Delete(true);
        }
    });

    [Fact]
    public void TaskAndDomainFailuresUseTheSameFileAndLoggingFailureDoesNotThrow()
    {
        var directory = Directory.CreateTempSubdirectory();
        try
        {
            var log = new OperationalErrorLog(directory.FullName);
            var failure = new IOException("test I/O failure");
            var task = new UnobservedTaskExceptionEventArgs(new AggregateException(failure));
            log.OnTaskException(null, task);
            Assert.True(task.Observed);
            log.OnDomainException(this, new UnhandledExceptionEventArgs(failure, true));
            var file = Assert.Single(directory.GetFiles("*.log"));
            var text = File.ReadAllText(file.FullName);
            Assert.Contains("TaskScheduler", text);
            Assert.Contains("IsTerminating=True", text);
            var blocked = new OperationalErrorLog(file.FullName);
            blocked.Write("작업", failure);
        }
        finally { directory.Delete(true); }
    }
}
