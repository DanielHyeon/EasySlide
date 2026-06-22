using System;
using System.Windows;
using System.Windows.Input;
using Easislides.Wpf.Input;
using Microsoft.Extensions.DependencyInjection;
using LegacyKeys = System.Windows.Forms.Keys;
using LegacyHookManager = Easislides.HookManager;

namespace Easislides.Wpf.Poc;

/// <summary>
/// PoC-A: HookManager 전역 후킹 ↔ WPF PreviewKeyDown 충돌 검증.
/// 합격 시 Sprint 1 진입 가능. 실패 시 아키텍처 재논의 (계획서 §8.3).
/// </summary>
public partial class PocAHookTest : Window
{
    private readonly ShortcutRegistry _registry;
    private bool _hookEnabled;

    public PocAHookTest()
    {
        InitializeComponent();
        _registry = App.Services.GetRequiredService<ShortcutRegistry>();

        SetupShortcuts();
    }

    private void SetupShortcuts()
    {
        // 글로벌: F5 = NextSlide (리모컨)
        TryRegister(new Shortcut(Key.F5, ModifierKeys.None, "Live.NextSlide", IsGlobal: true, "다음 슬라이드 (리모컨)"));
        // 로컬: Ctrl+F = Find (편집 폼 전용)
        TryRegister(new Shortcut(Key.F, ModifierKeys.Control, "Edit.Find", IsGlobal: false, "찾기"));
        // 충돌 시나리오: Space는 글로벌 + 로컬 양쪽 등록 가능하지만 같은 키 + 같은 modifier는 중복 금지.
        // 글로벌만 등록하여 글로벌 우선 동작 확인.
        TryRegister(new Shortcut(Key.Space, ModifierKeys.None, "Live.NextSlide.Space", IsGlobal: true, "다음 슬라이드 (Space)"));

        _registry.Bind("Live.NextSlide", () => Log("[CMD] Live.NextSlide 실행 (F5)"));
        _registry.Bind("Edit.Find", () => Log("[CMD] Edit.Find 실행 (Ctrl+F)"));
        _registry.Bind("Live.NextSlide.Space", () => Log("[CMD] Live.NextSlide 실행 (Space)"));

        Log($"등록 완료: {_registry.All.Count}개 단축키");
        foreach (var s in _registry.All)
        {
            Log($"  · {s.DisplayText,-15} → {s.CommandName,-25} [{(s.IsGlobal ? "글로벌" : "로컬")}] {s.Description}");
        }
    }

    private void TryRegister(Shortcut s)
    {
        try { _registry.Register(s); }
        catch (Exception ex) { Log($"[ERR] 등록 실패: {s.DisplayText} — {ex.Message}"); }
    }

    private void OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        // ADR-0004 우선순위: 로컬 처리 시도 → 처리됨이면 라우트 종료
        var modifiers = Keyboard.Modifiers;
        Log($"[WPF PreviewKeyDown] Key={e.Key}, Modifiers={modifiers}");

        if (_registry.TryHandle(e.Key, modifiers))
        {
            e.Handled = true;
            Log("  → 로컬(WPF) 핸들러가 처리, e.Handled=true");
        }
    }

    private void OnEnableHook_Click(object sender, RoutedEventArgs e)
    {
        if (_hookEnabled) return;

        // HookManager는 정적 클래스 — 이벤트 구독으로 글로벌 후킹 활성화
        LegacyHookManager.KeyDown += OnGlobalKeyDown;
        _hookEnabled = true;

        BtnEnableHook.IsEnabled = false;
        BtnDisableHook.IsEnabled = true;
        Log("");
        Log("✓ 글로벌 후킹 활성 — 이제 다른 앱이 활성 상태여도 F5/Space가 잡힙니다.");
        Log("  (잡힌 키는 본 창 비활성 상태에서도 아래 로그에 표시됨)");
    }

    private void OnDisableHook_Click(object sender, RoutedEventArgs e)
    {
        if (!_hookEnabled) return;

        LegacyHookManager.KeyDown -= OnGlobalKeyDown;
        _hookEnabled = false;

        BtnEnableHook.IsEnabled = true;
        BtnDisableHook.IsEnabled = false;
        Log("✗ 글로벌 후킹 비활성");
    }

    private void OnGlobalKeyDown(object? sender, System.Windows.Forms.KeyEventArgs e)
    {
        // HookManager는 Forms.KeyEventArgs로 콜백 — Dispatcher로 UI 스레드 안전 갱신
        Dispatcher.InvokeAsync(() =>
        {
            Log($"[HookMgr KeyDown] KeyCode={e.KeyCode}, Modifiers={e.Modifiers}");

            if (_registry.TryHandleGlobal(e.KeyCode, e.Modifiers))
            {
                Log("  → 글로벌 핸들러가 처리. 다른 앱이 활성이어도 EasiSlides 명령 실행됨.");
                // SuppressKeyPress = true로 OS 단계에서 키 차단 (다른 앱에 전달 안 됨)
                e.SuppressKeyPress = true;
            }
        });
    }

    private void OnClearLog_Click(object sender, RoutedEventArgs e)
    {
        LogTextBlock.Text = string.Empty;
    }

    private void Log(string message)
    {
        var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
        LogTextBlock.Text += $"{timestamp}  {message}\n";
        LogScroll.ScrollToEnd();
    }

    protected override void OnClosed(EventArgs e)
    {
        if (_hookEnabled)
            LegacyHookManager.KeyDown -= OnGlobalKeyDown;
        base.OnClosed(e);
    }
}
