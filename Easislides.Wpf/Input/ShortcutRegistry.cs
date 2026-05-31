using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using LegacyKeys = System.Windows.Forms.Keys;
using WpfKey = System.Windows.Input.Key;
using WpfModifierKeys = System.Windows.Input.ModifierKeys;

namespace Easislides.Wpf.Input;

public sealed class ShortcutRegistry
{
    private ImmutableList<Shortcut> _shortcuts = ImmutableList<Shortcut>.Empty;
    private readonly ConcurrentDictionary<string, Action> _bindings = new();

    public IReadOnlyList<Shortcut> All => _shortcuts;

    public void Register(Shortcut shortcut)
    {
        ArgumentNullException.ThrowIfNull(shortcut);

        while (true)
        {
            var oldList = _shortcuts;
            if (oldList.Any(s => s.Key == shortcut.Key && s.Modifiers == shortcut.Modifiers))
            {
                throw new InvalidOperationException(
                    $"Shortcut collision: {shortcut.DisplayText} is already registered.");
            }

            var newList = oldList.Add(shortcut);
            if (Interlocked.CompareExchange(ref _shortcuts, newList, oldList) == oldList)
            {
                return;
            }
        }
    }

    public void Bind(string commandName, Action handler)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(commandName);
        ArgumentNullException.ThrowIfNull(handler);

        _bindings[commandName] = handler;
    }

    public bool TryHandle(WpfKey key, WpfModifierKeys modifiers)
    {
        var shortcut = _shortcuts.FirstOrDefault(s => s.Key == key && s.Modifiers == modifiers);
        return shortcut is not null && Invoke(shortcut.CommandName);
    }

    public bool TryHandleGlobal(LegacyKeys legacyKey, LegacyKeys legacyModifiers)
    {
        if (!Enum.TryParse<WpfKey>(legacyKey.ToString(), out var wpfKey))
        {
            return false;
        }

        var wpfModifiers = ToWpfModifiers(legacyModifiers);
        var shortcut = _shortcuts.FirstOrDefault(
            s => s.IsGlobal && s.Key == wpfKey && s.Modifiers == wpfModifiers);

        return shortcut is not null && Invoke(shortcut.CommandName);
    }

    private static WpfModifierKeys ToWpfModifiers(LegacyKeys legacyModifiers)
    {
        var modifiers = WpfModifierKeys.None;
        if ((legacyModifiers & LegacyKeys.Control) != 0) modifiers |= WpfModifierKeys.Control;
        if ((legacyModifiers & LegacyKeys.Shift) != 0) modifiers |= WpfModifierKeys.Shift;
        if ((legacyModifiers & LegacyKeys.Alt) != 0) modifiers |= WpfModifierKeys.Alt;
        return modifiers;
    }

    // 명령 id 로 바인딩된 동작을 직접 실행한다(명령 팔레트 §7.4 — 키 입력이 아니라 검색·선택으로 실행).
    // 바인딩이 없거나 동작이 예외를 던지면 false. 단축키 경로(TryHandle)와 동일한 Invoke 를 공유한다.
    public bool TryInvoke(string commandName)
    {
        if (string.IsNullOrWhiteSpace(commandName))
        {
            return false;
        }

        return Invoke(commandName);
    }

    private bool Invoke(string commandName)
    {
        if (!_bindings.TryGetValue(commandName, out var handler))
        {
            return false;
        }

        try
        {
            handler();
            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ShortcutRegistry] {commandName} failed: {ex}");
            return false;
        }
    }
}
