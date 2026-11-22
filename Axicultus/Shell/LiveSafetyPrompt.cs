using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Axicultus.Wpf.Composites;
using Axicultus.Wpf.Settings;

namespace Axicultus.Wpf.Shell;

public sealed record LiveSafetyRequest(
    string ActionName,
    string Question,
    string Subtext,
    TimeSpan Timeout);

public interface ILiveSafetyPrompt
{
    Task<bool> ConfirmAsync(LiveSafetyRequest request, CancellationToken cancellationToken = default);
}

public sealed class WpfLiveSafetyPrompt(ISettingsService settings) : ILiveSafetyPrompt
{
    public async Task<bool> ConfirmAsync(LiveSafetyRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();
        if (!settings.Get(AxicultusSettingKeys.UseSafetyConfirmations)) return true;

        if (Application.Current?.MainWindow is not FrameworkElement mainWindow)
        {
            return false;
        }

        var anchor = Mouse.DirectlyOver as FrameworkElement ?? Keyboard.FocusedElement as FrameworkElement;
        // The prompt is for the operator: an anchor on the audience or stage window (pointer parked on the
        // projector, or focus left there) would pop it up in front of the congregation.
        if (anchor is null || !anchor.IsVisible || !ReferenceEquals(Window.GetWindow(anchor), mainWindow))
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
