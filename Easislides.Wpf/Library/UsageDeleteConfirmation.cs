using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Easislides.Wpf.Composites;
using Easislides.Wpf.Data;

namespace Easislides.Wpf.Library;

public sealed record UsageDeleteConfirmationRequest(
    int RecordCount,
    DateTime From,
    DateTime To,
    string Session,
    IReadOnlyList<UsageRecord> Records);

public interface IUsageDeleteConfirmation
{
    Task<bool> ConfirmAsync(UsageDeleteConfirmationRequest request);
}

public sealed class WpfUsageDeleteConfirmation : IUsageDeleteConfirmation
{
    public async Task<bool> ConfirmAsync(UsageDeleteConfirmationRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (FindAnchor() is not { } anchor)
        {
            return false;
        }

        var session = string.IsNullOrWhiteSpace(request.Session) ? "All sessions" : request.Session;
        var subtext = $"{request.From:yyyy-MM-dd} - {request.To:yyyy-MM-dd}, {session}.";
        return await SafetyConfirm.AskAsync(
            anchor,
            $"Delete {request.RecordCount} usage record(s)?",
            subtext,
            TimeSpan.FromSeconds(15)).ConfigureAwait(true);
    }

    private static FrameworkElement? FindAnchor()
    {
        var current = Application.Current;
        if (current is null)
        {
            return null;
        }

        return current.Windows
            .OfType<Window>()
            .FirstOrDefault(window => window.IsActive) as FrameworkElement
            ?? current.MainWindow as FrameworkElement;
    }
}
