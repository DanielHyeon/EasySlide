using System;
using System.Windows.Input;

namespace Easislides.Wpf.Input;

/// <summary>
/// 단축키 정의 — ADR-0004 단일 소스 패턴.
/// HookManager(글로벌)와 WPF PreviewKeyDown(로컬) 양쪽에서 동일 정의 참조.
/// </summary>
/// <param name="Key">키 (예: F5, Space)</param>
/// <param name="Modifiers">조합 키 (Ctrl/Shift/Alt)</param>
/// <param name="CommandName">실행할 명령 식별자 (ICommand 매핑은 ShortcutRegistry에서)</param>
/// <param name="IsGlobal">true면 앱 비포커스 시에도 HookManager로 처리 (리모컨용)</param>
/// <param name="Description">설정 화면에 표시할 한글 설명</param>
public sealed record Shortcut(
    Key Key,
    ModifierKeys Modifiers,
    string CommandName,
    bool IsGlobal,
    string Description)
{
    /// <summary>키 조합을 사용자에게 보여줄 텍스트로 변환 (예: "Ctrl+Shift+L")</summary>
    public string DisplayText
    {
        get
        {
            var parts = new System.Collections.Generic.List<string>();
            if ((Modifiers & ModifierKeys.Control) != 0) parts.Add("Ctrl");
            if ((Modifiers & ModifierKeys.Shift) != 0) parts.Add("Shift");
            if ((Modifiers & ModifierKeys.Alt) != 0) parts.Add("Alt");
            if ((Modifiers & ModifierKeys.Windows) != 0) parts.Add("Win");
            parts.Add(Key.ToString());
            return string.Join("+", parts);
        }
    }
}
