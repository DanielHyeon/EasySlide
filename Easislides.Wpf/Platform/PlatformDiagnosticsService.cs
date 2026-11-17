using System;
using System.Collections.Generic;
using System.Linq;
using Easislides.Wpf.Input;
using Easislides.Wpf.Shell;

namespace Easislides.Wpf.Platform;

public sealed record PlatformDiagnosticsSnapshot(
    DateTimeOffset CapturedAt,
    int DisplayCount,
    string? PrimaryDisplayId,
    string? PreferredDisplayId,
    bool IsGlobalInputRunning,
    string? GlobalInputError,
    int CommandCount,
    int DefaultShortcutCount,
    int GlobalShortcutCount,
    IReadOnlyList<string> Warnings)
{
    public bool HasDisplays => DisplayCount > 0;

    public bool HasWarnings => Warnings.Count > 0;
}

public interface IPlatformDiagnosticsService
{
    PlatformDiagnosticsSnapshot Capture();
}

public sealed class PlatformDiagnosticsService : IPlatformDiagnosticsService
{
    private readonly IDisplayService _display;
    private readonly IGlobalInputService _globalInput;
    private readonly ICommandCatalog _commands;

    public PlatformDiagnosticsService(
        IDisplayService display,
        IGlobalInputService globalInput,
        ICommandCatalog commands)
    {
        _display = display;
        _globalInput = globalInput;
        _commands = commands;
    }

    public PlatformDiagnosticsSnapshot Capture()
    {
        var warnings = new List<string>();
        var displays = Array.Empty<OutputDisplay>();
        string? primaryDisplayId = null;
        string? preferredDisplayId = null;

        try
        {
            displays = _display.GetDisplays().ToArray();
            primaryDisplayId = _display.GetPrimaryDisplay().Id;
            preferredDisplayId = _display.GetPreferredDisplay().Id;
            if (displays.Length == 0)
            {
                warnings.Add("No displays detected.");
            }
        }
        catch (Exception ex)
        {
            warnings.Add($"Display diagnostics failed: {FormatException(ex)}");
        }

        var globalInputError = _globalInput.LastError is null ? null : FormatException(_globalInput.LastError);
        if (globalInputError is not null)
        {
            warnings.Add($"Global input error: {globalInputError}");
        }

        var commands = _commands.All;
        var shortcuts = _commands.GetDefaultShortcuts();
        AddCatalogWarnings(commands, shortcuts, warnings);

        return new PlatformDiagnosticsSnapshot(
            DateTimeOffset.UtcNow,
            displays.Length,
            primaryDisplayId,
            preferredDisplayId,
            _globalInput.IsRunning,
            globalInputError,
            commands.Count,
            shortcuts.Count,
            shortcuts.Count(shortcut => shortcut.IsGlobal),
            warnings.AsReadOnly());
    }

    private static void AddCatalogWarnings(
        IReadOnlyList<CommandDescriptor> commands,
        IReadOnlyList<Shortcut> shortcuts,
        ICollection<string> warnings)
    {
        var duplicateCommandIds = commands
            .GroupBy(command => command.Id, StringComparer.OrdinalIgnoreCase)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .OrderBy(id => id, StringComparer.OrdinalIgnoreCase)
            .ToArray();
        if (duplicateCommandIds.Length > 0)
        {
            warnings.Add($"Duplicate command ids: {string.Join(", ", duplicateCommandIds)}");
        }

        var duplicateShortcuts = shortcuts
            .GroupBy(FormatShortcut, StringComparer.Ordinal)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .OrderBy(signature => signature, StringComparer.Ordinal)
            .ToArray();
        if (duplicateShortcuts.Length > 0)
        {
            warnings.Add($"Duplicate shortcuts: {string.Join(", ", duplicateShortcuts)}");
        }
    }

    private static string FormatException(Exception ex) => $"{ex.GetType().Name}: {ex.Message}";

    private static string FormatShortcut(Shortcut shortcut)
    {
        var scope = shortcut.IsGlobal ? "Global" : "Local";
        return $"{scope} {shortcut.Modifiers}+{shortcut.Key}";
    }
}
