using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using LegacyKeys = System.Windows.Forms.Keys;
using WpfKey = System.Windows.Input.Key;
using WpfModifierKeys = System.Windows.Input.ModifierKeys;

namespace Easislides.Wpf.Input;

public sealed class ShortcutRegistry
{
    private sealed record Registrations(
        ImmutableList<Shortcut> Ordered,
        ImmutableDictionary<(WpfKey, WpfModifierKeys), Shortcut> ByGesture);

    private Registrations _registered = new(ImmutableList<Shortcut>.Empty,
        ImmutableDictionary<(WpfKey, WpfModifierKeys), Shortcut>.Empty);
    private readonly ConcurrentDictionary<string, Action> _bindings = new();

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

    public void Bind(string commandName, Action handler)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(commandName);
        ArgumentNullException.ThrowIfNull(handler);

        _bindings[commandName] = handler;
    }

    public bool TryHandle(WpfKey key, WpfModifierKeys modifiers)
    {
        return Volatile.Read(ref _registered).ByGesture.TryGetValue((key, modifiers), out var shortcut)
            && Invoke(shortcut.CommandName);
    }

    internal bool TryResolveGlobal(LegacyKeys key, LegacyKeys modifiers, [NotNullWhen(true)] out Action? handler)
    {
        var gesture = (System.Windows.Input.KeyInterop.KeyFromVirtualKey((int)key), ToWpfModifiers(modifiers));
        handler = null;
        return Volatile.Read(ref _registered).ByGesture.TryGetValue(gesture, out var shortcut)
            && shortcut.IsGlobal && _bindings.TryGetValue(shortcut.CommandName, out handler);
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

    private bool Invoke(string commandName)
    {
        if (!_bindings.TryGetValue(commandName, out var handler))
        {
            return false;
        }

        return Execute(handler);
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
