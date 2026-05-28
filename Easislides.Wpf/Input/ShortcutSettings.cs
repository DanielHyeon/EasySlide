using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace Easislides.Wpf.Input;

public static class ShortcutSettings
{
    private static readonly StringComparer Comparer = StringComparer.OrdinalIgnoreCase;

    public static string GetSlotId(Shortcut shortcut)
    {
        ArgumentNullException.ThrowIfNull(shortcut);
        return GetSlotId(shortcut.CommandName, shortcut.IsGlobal);
    }

    public static string GetSlotId(string commandName, bool isGlobal)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(commandName);
        return $"{commandName}:{(isGlobal ? "global" : "local")}";
    }

    public static bool TryApplyGesture(Shortcut shortcut, string gesture, out Shortcut updated, out string error)
    {
        if (TryParseGesture(gesture, out var key, out var modifiers, out error))
        {
            updated = shortcut with { Key = key, Modifiers = modifiers };
            return true;
        }

        updated = shortcut;
        return false;
    }

    public static bool TryParseGesture(string gesture, out Key key, out ModifierKeys modifiers, out string error)
    {
        key = Key.None;
        modifiers = ModifierKeys.None;
        error = "";

        if (string.IsNullOrWhiteSpace(gesture))
        {
            error = "Shortcut gesture cannot be empty.";
            return false;
        }

        var parts = gesture
            .Split('+', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToArray();
        if (parts.Length == 0)
        {
            error = "Shortcut gesture cannot be empty.";
            return false;
        }

        foreach (var part in parts[..^1])
        {
            if (IsModifier(part, out var modifier))
            {
                modifiers |= modifier;
                continue;
            }

            error = $"'{part}' is not a supported shortcut modifier.";
            return false;
        }

        if (!Enum.TryParse(parts[^1], ignoreCase: true, out key) || key == Key.None)
        {
            error = $"'{parts[^1]}' is not a supported shortcut key.";
            return false;
        }

        if (IsModifierKey(key))
        {
            error = "Shortcut must include a non-modifier key.";
            return false;
        }

        return true;
    }

    public static string NormalizeGesture(string gesture)
    {
        if (!TryParseGesture(gesture, out var key, out var modifiers, out var error))
        {
            throw new ArgumentException(error, nameof(gesture));
        }

        return new Shortcut(key, modifiers, "", IsGlobal: false, "").DisplayText;
    }

    public static IReadOnlyList<Shortcut> ApplyOverrides(
        IEnumerable<Shortcut> defaults,
        IReadOnlyDictionary<string, string> overrides)
    {
        ArgumentNullException.ThrowIfNull(defaults);
        ArgumentNullException.ThrowIfNull(overrides);

        return defaults.Select(shortcut =>
        {
            var slotId = GetSlotId(shortcut);
            return overrides.TryGetValue(slotId, out var gesture) &&
                   TryApplyGesture(shortcut, gesture, out var updated, out _)
                ? updated
                : shortcut;
        }).ToArray();
    }

    private static bool IsModifier(string part, out ModifierKeys modifier)
    {
        if (Comparer.Equals(part, "Ctrl") || Comparer.Equals(part, "Control"))
        {
            modifier = ModifierKeys.Control;
            return true;
        }

        if (Comparer.Equals(part, "Shift"))
        {
            modifier = ModifierKeys.Shift;
            return true;
        }

        if (Comparer.Equals(part, "Alt"))
        {
            modifier = ModifierKeys.Alt;
            return true;
        }

        if (Comparer.Equals(part, "Win") || Comparer.Equals(part, "Windows"))
        {
            modifier = ModifierKeys.Windows;
            return true;
        }

        modifier = ModifierKeys.None;
        return false;
    }

    private static bool IsModifierKey(Key key)
        => key is Key.LeftCtrl or Key.RightCtrl
            or Key.LeftShift or Key.RightShift
            or Key.LeftAlt or Key.RightAlt
            or Key.LWin or Key.RWin;
}
