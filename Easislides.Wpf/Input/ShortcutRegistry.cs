using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Input;
using LegacyKeys = System.Windows.Forms.Keys;
using WpfKey = System.Windows.Input.Key;
using WpfModifierKeys = System.Windows.Input.ModifierKeys;

namespace Easislides.Wpf.Input;

/// <summary>
/// ADR-0004 단일 소스 — 단축키 등록/조회/실행 중앙 관리.
/// 글로벌(IsGlobal=true) 항목은 HookManager가 처리, 그 외는 WPF PreviewKeyDown이 처리.
///
/// 사용 예 (Sprint 0 PoC-A):
///   var reg = App.Services.GetRequiredService&lt;ShortcutRegistry&gt;();
///   reg.Register(new Shortcut(WpfKey.F5, WpfModifierKeys.None, "Live.NextSlide", true, "다음 슬라이드"));
///   reg.Bind("Live.NextSlide", () => _vm.NextSlide());
/// </summary>
public sealed class ShortcutRegistry
{
    private readonly List<Shortcut> _shortcuts = new();
    private readonly ConcurrentDictionary<string, Action> _bindings = new();

    /// <summary>등록된 단축키 목록 (UI 표시·충돌 검사용).</summary>
    public IReadOnlyList<Shortcut> All => _shortcuts.AsReadOnly();

    /// <summary>단축키를 등록한다. 중복 키 조합은 예외.</summary>
    public void Register(Shortcut shortcut)
    {
        if (_shortcuts.Any(s => s.Key == shortcut.Key && s.Modifiers == shortcut.Modifiers))
        {
            throw new InvalidOperationException(
                $"단축키 충돌: {shortcut.DisplayText}는 이미 다른 명령에 사용 중입니다.");
        }
        _shortcuts.Add(shortcut);
    }

    /// <summary>명령 식별자에 실제 핸들러를 바인딩한다.</summary>
    public void Bind(string commandName, Action handler)
    {
        _bindings[commandName] = handler;
    }

    /// <summary>
    /// WPF PreviewKeyDown 측에서 호출 — 로컬 단축키 처리.
    /// 처리됨 true 반환 시 호출자가 e.Handled = true 처리.
    /// </summary>
    public bool TryHandle(WpfKey key, WpfModifierKeys modifiers)
    {
        var shortcut = _shortcuts.FirstOrDefault(s => s.Key == key && s.Modifiers == modifiers);
        if (shortcut is null) return false;
        return Invoke(shortcut.CommandName);
    }

    /// <summary>
    /// HookManager 측에서 호출 — WinForms.Keys를 WPF.Key로 변환 후 처리.
    /// Sprint 0 PoC-A 시나리오: 앱 비포커스 상태에서도 작동해야 함.
    /// </summary>
    public bool TryHandleGlobal(LegacyKeys legacyKey, LegacyKeys legacyModifiers)
    {
        // WinForms.Keys → WPF.Key 변환 (열거자 값 일부 일치 — Enum.TryParse 활용)
        if (!Enum.TryParse<WpfKey>(legacyKey.ToString(), out var wpfKey))
            return false;

        var wpfMods = WpfModifierKeys.None;
        if ((legacyModifiers & LegacyKeys.Control) != 0) wpfMods |= WpfModifierKeys.Control;
        if ((legacyModifiers & LegacyKeys.Shift) != 0) wpfMods |= WpfModifierKeys.Shift;
        if ((legacyModifiers & LegacyKeys.Alt) != 0) wpfMods |= WpfModifierKeys.Alt;

        var shortcut = _shortcuts.FirstOrDefault(
            s => s.IsGlobal && s.Key == wpfKey && s.Modifiers == wpfMods);
        if (shortcut is null) return false;
        return Invoke(shortcut.CommandName);
    }

    private bool Invoke(string commandName)
    {
        if (_bindings.TryGetValue(commandName, out var handler))
        {
            try
            {
                handler();
                return true;
            }
            catch (Exception ex)
            {
                // Sprint 1 이후: 로깅 인프라(Serilog 등)로 교체
                System.Diagnostics.Debug.WriteLine($"[ShortcutRegistry] {commandName} 실행 실패: {ex}");
                return false;
            }
        }
        return false;
    }
}
