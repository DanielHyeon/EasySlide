using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Axicultus.Wpf.Composites;
using Axicultus.Wpf.Data;
using Axicultus.Wpf.Localization;

namespace Axicultus.Wpf.Library;

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

        var session = string.IsNullOrWhiteSpace(request.Session) ? UiText.Get("UsageDeleteConfirmation", "AllSessions") : request.Session;
        var subtext = $"{request.From:yyyy-MM-dd} - {request.To:yyyy-MM-dd}, {session}.";
        return await SafetyConfirm.AskAsync(
            anchor,
            UiText.Format("UsageDeleteConfirmation", "Question", request.RecordCount),
            subtext,
            TimeSpan.FromSeconds(15)).ConfigureAwait(true);
    }

    private static Window? FindAnchor()
    {
        var current = Application.Current;
        if (current is null)
        {
            return null;
        }

        return current.Windows
            .OfType<Window>()
            .FirstOrDefault(window => window.IsActive) ?? current.MainWindow;
    }
}
