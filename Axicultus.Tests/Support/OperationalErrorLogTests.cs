using System;
using System.IO;
using System.Linq;
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

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public Task DispatcherErrorIsHandledOnlyWhileTheShellReportsARunningShow(bool showRunning) => StaHelper.RunOnStaAsync(async () =>
    {
        var directory = Directory.CreateTempSubdirectory();
        Exception? reported = null;
        var log = new OperationalErrorLog(directory.FullName)
        {
            ReportDuringShow = error => { reported = error; return showRunning; },
        };
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
            Assert.Equal(showRunning, await observed.Task.WaitAsync(TimeSpan.FromSeconds(5)));
            Assert.IsType<InvalidOperationException>(reported);
            Assert.Contains("test UI failure", File.ReadAllText(Assert.Single(directory.GetFiles("*.log")).FullName));
        }
        finally
        {
            dispatcher.UnhandledException -= Observe;
            dispatcher.UnhandledException -= log.OnUiException;
            directory.Delete(true);
        }
    });

    [Fact]
    public Task RepeatingDispatcherErrorStopsBeingHandledAndIsLoggedOnce() => StaHelper.RunOnStaAsync(async () =>
    {
        var directory = Directory.CreateTempSubdirectory();
        int reports = 0;
        var log = new OperationalErrorLog(directory.FullName) { ReportDuringShow = _ => { reports++; return true; } };
        var dispatcher = Dispatcher.CurrentDispatcher;
        var handled = new System.Collections.Generic.List<bool>();
        const int thrown = OperationalErrorLog.MaxHandledPerWindow + 20;
        var done = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        void Observe(object sender, DispatcherUnhandledExceptionEventArgs args)
        {
            handled.Add(args.Handled);
            args.Handled = true; // Only the test keeps this dispatcher running.
            if (handled.Count == thrown) done.SetResult();
        }

        dispatcher.UnhandledException += log.OnUiException;
        dispatcher.UnhandledException += Observe;
        try
        {
            for (int i = 0; i < thrown; i++)
                _ = dispatcher.BeginInvoke(new Action(() => throw new InvalidOperationException("render timer failure")));
            await done.Task.WaitAsync(TimeSpan.FromSeconds(5));

            Assert.All(handled.GetRange(0, OperationalErrorLog.MaxHandledPerWindow), Assert.True);
            Assert.All(handled.GetRange(OperationalErrorLog.MaxHandledPerWindow, 20), Assert.False);
            Assert.Equal(OperationalErrorLog.MaxHandledPerWindow, reports);
            var lines = File.ReadAllLines(Assert.Single(directory.GetFiles("*.log")).FullName);
            Assert.Equal(OperationalErrorLog.MaxHandledPerWindow + 1, lines.Count(line => line.Contains("[Dispatcher]")));
            Assert.Single(lines, line => line.Contains("no longer handled"));
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
