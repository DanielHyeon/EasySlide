using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Easislides.Wpf.Composites;

namespace Easislides.Wpf.Shell;

public sealed record LiveSafetyRequest(
    string ActionName,
    string Question,
    string Subtext,
    TimeSpan Timeout);

public interface ILiveSafetyPrompt
{
    Task<bool> ConfirmAsync(LiveSafetyRequest request, CancellationToken cancellationToken = default);
}

public sealed class WpfLiveSafetyPrompt : ILiveSafetyPrompt
{
    public async Task<bool> ConfirmAsync(LiveSafetyRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (Application.Current?.MainWindow is not FrameworkElement mainWindow)
        {
            return false;
        }

        var anchor = Mouse.DirectlyOver as FrameworkElement ?? Keyboard.FocusedElement as FrameworkElement;
        if (anchor is null || !anchor.IsVisible)
        {
            anchor = mainWindow;
        }

        var confirmTask = SafetyConfirm.AskAsync(anchor, request.Question, request.Subtext, request.Timeout);
        if (!cancellationToken.CanBeCanceled)
        {
            return await confirmTask.ConfigureAwait(true);
        }

        var cancellationTask = Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
        var completed = await Task.WhenAny(confirmTask, cancellationTask).ConfigureAwait(true);
        return completed == confirmTask && await confirmTask.ConfigureAwait(true);
    }
}
