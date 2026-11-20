using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using Axicultus.Wpf.Shell;
using LegacyKeys = System.Windows.Forms.Keys;
using WpfKey = System.Windows.Input.Key;
using WpfModifierKeys = System.Windows.Input.ModifierKeys;

namespace Axicultus.Wpf.Input;

public sealed class ShortcutRegistry
{
    private sealed record Registrations(
        ImmutableList<Shortcut> Ordered,
        ImmutableDictionary<(WpfKey, WpfModifierKeys), Shortcut> ByGesture);

    private Registrations _registered = new([],
        []);
    private sealed record Binding(Action Execute, Func<bool>? CanExecute);
    private readonly ConcurrentDictionary<string, Binding> _bindings = new();

    internal Func<nint, bool>? IsPowerPointOutputWindow { get; set; }
    internal Func<LegacyKeys, Action?>? ResolveOutputMediaKey { private get; set; }

    internal bool TryResolveMedia(LegacyKeys key, LegacyKeys modifiers, [NotNullWhen(true)] out Action? handler)
    {
        handler = modifiers == LegacyKeys.None ? ResolveOutputMediaKey?.Invoke(key) : null;
        return handler != null;
    }

    public IReadOnlyList<Shortcut> All => Volatile.Read(ref _registered).Ordered;

    public void Register(Shortcut shortcut)
    {
        ArgumentNullException.ThrowIfNull(shortcut);
        var gesture = (shortcut.Key, shortcut.Modifiers);
        while (true)
        {
            var current = Volatile.Read(ref _registered);
            if (current.ByGesture.ContainsKey(gesture))
                throw new InvalidOperationException($"Shortcut collision: {shortcut.DisplayText} is already registered.");

            var updated = new Registrations(current.Ordered.Add(shortcut), current.ByGesture.Add(gesture, shortcut));
            if (ReferenceEquals(Interlocked.CompareExchange(ref _registered, updated, current), current)) return;
        }
    }

    internal void ReplaceShortcuts(IEnumerable<Shortcut> shortcuts)
    {
        var list = shortcuts.GroupBy(s => (s.Key, s.Modifiers)).Select(group =>
        {
            var first = group.First();
            return group.FirstOrDefault(s => s.IsGlobal && s.CommandName == first.CommandName) ?? first;
        }).ToImmutableList();
        Volatile.Write(ref _registered, new Registrations(list, list.ToImmutableDictionary(s => (s.Key, s.Modifiers))));
    }

    public void Bind(string commandName, Action handler, Func<bool>? canExecute = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(commandName);
        ArgumentNullException.ThrowIfNull(handler);

        _bindings[commandName] = new Binding(handler, canExecute);
    }

    public bool TryHandle(WpfKey key, WpfModifierKeys modifiers)
    {
        return FindShortcut(key, modifiers) is { } shortcut && Invoke(shortcut.CommandName);
    }

    internal bool TryResolveGlobal(LegacyKeys key, LegacyKeys modifiers, [NotNullWhen(true)] out Action? handler)
    {
        if (key == LegacyKeys.Escape && modifiers == LegacyKeys.None && TryGetHandler(MainCommandIds.EndShow, out handler)) return true;
        var shortcut = FindShortcut(System.Windows.Input.KeyInterop.KeyFromVirtualKey((int)key), ToWpfModifiers(modifiers), globalOnly: true);
        handler = null;
        return shortcut is { IsGlobal: true } && TryGetHandler(shortcut.CommandName, out handler);
    }

    public bool TryHandleGlobal(LegacyKeys key, LegacyKeys modifiers) =>
        TryResolveGlobal(key, modifiers, out var handler) && Execute(handler);

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

    private bool Invoke(string commandName) => TryGetHandler(commandName, out var handler) && Execute(handler);

    internal bool TryGetHandler(string name, [NotNullWhen(true)] out Action? handler)
    {
        handler = null;
        if (!_bindings.TryGetValue(name, out var binding) || binding.CanExecute?.Invoke() == false)
            return false;
        handler = binding.Execute;
        return true;
    }

    internal bool IsGlobalGesture(WpfKey key, WpfModifierKeys modifiers) =>
        FindShortcut(key, modifiers) is { IsGlobal: true };

    private Shortcut? FindShortcut(WpfKey key, WpfModifierKeys modifiers, bool globalOnly = false)
    {
        var gestures = Volatile.Read(ref _registered).ByGesture;
        if (gestures.TryGetValue((key, modifiers), out var exact) && (!globalOnly || exact.IsGlobal)) return exact;
        if (modifiers == WpfModifierKeys.None) return null;

        // FrmMain checks KeyCode for F7–F10 and only the Ctrl bit for arrows.
        var legacyModifier = key is WpfKey.Up or WpfKey.Down ? modifiers & WpfModifierKeys.Control : WpfModifierKeys.None;
        if (!gestures.TryGetValue((key, legacyModifier), out var candidate) || !candidate.IsGlobal) return null;
        return (key, candidate.CommandName) switch
        {
            (WpfKey.F7, MainCommandIds.LivePreviewToOutputClearBlack) or (WpfKey.F8, MainCommandIds.LivePreviewToOutput)
                or (WpfKey.F9 or WpfKey.F10, MainCommandIds.LiveBlack) or (WpfKey.Up, MainCommandIds.LivePrevious)
                or (WpfKey.Down, MainCommandIds.LiveNext) => candidate,
            _ => null
        };
    }

    private static bool Execute(Action handler)
    {
        try
        {
            handler();
            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ShortcutRegistry] Command failed: {ex}");
            return false;
        }
    }
}
