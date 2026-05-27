using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
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
/// Thread-safety (리뷰 #6):
///   - HookManager 콜백은 별도 OS 스레드에서 호출되므로 TryHandleGlobal과 Register가 동시 진행 가능.
///   - _shortcuts를 ImmutableList로 관리하고 Interlocked.CompareExchange로 교체 — lock-free read.
///   - Register는 앱 시작 시 일괄 호출이 일반적이지만, 동적 등록도 안전.
///
/// 사용 예 (Sprint 0 PoC-A):
///   var reg = App.Services.GetRequiredService&lt;ShortcutRegistry&gt;();
///   reg.Register(new Shortcut(WpfKey.F5, WpfModifierKeys.None, "Live.NextSlide", true, "다음 슬라이드"));
///   reg.Bind("Live.NextSlide", () => _vm.NextSlide());
/// </summary>
public sealed class ShortcutRegistry
{
    private ImmutableList<Shortcut> _shortcuts = ImmutableList<Shortcut>.Empty;
    private readonly ConcurrentDictionary<string, Action> _bindings = new();

    /// <summary>등록된 단축키 목록 (UI 표시·충돌 검사용). 읽기는 lock-free.</summary>
    public IReadOnlyList<Shortcut> All => _shortcuts;

    /// <summary>단축키를 등록한다. 중복 키 조합은 예외. Thread-safe.</summary>
    public void Register(Shortcut shortcut)
    {
        ImmutableList<Shortcut> oldList, newList;
        do
        {
            oldList = _shortcuts;
            if (oldList.Any(s => s.Key == shortcut.Key && s.Modifiers == shortcut.Modifiers))
            {
                throw new InvalidOperationException(
                    $"단축키 충돌: {shortcut.DisplayText}는 이미 다른 명령에 사용 중입니다.");
            }
            newList = oldList.Add(shortcut);
        }
        // CAS — 다른 스레드가 동시에 _shortcuts를 변경했으면 재시도
        while (Interlocked.CompareExchange(ref _shortcuts, newList, oldList) != oldList);
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
        // ImmutableList 읽기 — lock 없음, 한 시점의 snapshot 사용
        var snapshot = _shortcuts;
        var shortcut = snapshot.FirstOrDefault(s => s.Key == key && s.Modifiers == modifiers);
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

        // HookManager 콜백은 별도 OS 스레드 — snapshot으로 lock-free 읽기
        var snapshot = _shortcuts;
        var shortcut = snapshot.FirstOrDefault(
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
