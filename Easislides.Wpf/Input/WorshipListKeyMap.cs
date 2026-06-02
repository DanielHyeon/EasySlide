using System.Windows.Input;

namespace Easislides.Wpf.Input;

/// <summary>
/// 예배 순서 목록(QueueList)에 포커스가 있을 때의 키 동작을 정하는 순수 함수 — 목록 컨트롤의 KeyDown 핸들러가 위임한다.
/// 목록에 포커스가 있을 때만 동작하므로(컨트롤 레벨 KeyDown) 검색창 등 다른 텍스트 입력의 키를 가로채지 않는다.
/// 순수 함수라 STA·UI 없이 단위 테스트가 쉽다.
/// </summary>
public static class WorshipListKeyMap
{
    /// <summary>
    /// Delete 키(수식 키 없음)면 "선택 항목 제거"로 본다 — 목록 항목 삭제의 표준 키. 다른 키나 수식 키가 있으면 false.
    /// </summary>
    public static bool IsRemoveSelectedItem(Key key, ModifierKeys modifiers)
        => key == Key.Delete && modifiers == ModifierKeys.None;
}
