using System.Windows.Input;

namespace Easislides.Wpf.Input;

/// <summary>미디어 플레이어 전역 키가 일으킬 동작(레거시 FrmMain.HandleMediaPlayerKey 대응).</summary>
public enum MediaPlayerKeyAction
{
    /// <summary>해당 키는 미디어 동작이 아님(다른 단축키가 처리하도록 넘긴다).</summary>
    None,

    /// <summary>재생/일시정지 토글(Space).</summary>
    PlayPause,

    /// <summary>
    /// 정지(Esc·S). 레거시 S 와 동일하게 재생만 멈춘다. 단, 레거시 Esc 는 정지에 더해 영상을 숨겨
    /// 가사 화면으로 되돌렸는데(stop+hide), 그 "영상 내리고 가사 복귀"(Unload)는 이번 범위가 아니라 후속이다.
    /// </summary>
    Stop,

    /// <summary>처음부터 다시 재생(Enter — 레거시 Remote_ResumeItemFromStart).</summary>
    Restart,

    /// <summary>음소거 토글(M).</summary>
    ToggleMute,
}

/// <summary>
/// 미디어가 라이브로 재생 중일 때 누른 키를 미디어 동작으로 바꾼다(레거시 미디어 플레이어 전역키
/// Esc/Space/Enter/S/M, FrmMain.HandleMediaPlayerKey). 순수 매핑이라 키→동작만 결정하고,
/// "지금 미디어가 있는가"(CanExecute) 판단·실행은 호출 측(View)이 한다.
/// </summary>
public static class MediaPlayerKeyMap
{
    /// <summary>키 하나를 미디어 동작으로 바꾼다. 매핑이 없으면 <see cref="MediaPlayerKeyAction.None"/>.</summary>
    public static MediaPlayerKeyAction MapKey(Key key) => key switch
    {
        Key.Space => MediaPlayerKeyAction.PlayPause,
        Key.Escape => MediaPlayerKeyAction.Stop,
        Key.S => MediaPlayerKeyAction.Stop,
        // Key.Enter 와 Key.Return 은 같은 값이라 한 갈래로 처리된다.
        Key.Enter => MediaPlayerKeyAction.Restart,
        Key.M => MediaPlayerKeyAction.ToggleMute,
        _ => MediaPlayerKeyAction.None,
    };
}

/// <summary>
/// 미디어 키를 "지금 실제로 실행할 동작"으로 라우팅한다(순수 결정 — 포커스·수식키·실행가능 여부를 모두 고려).
/// <para>
/// View 의 키 핸들러가 모아둔 상황(수식키 있나·텍스트/버튼에 포커스 있나·그 동작이 지금 가능한가)을 받아
/// 소비할 동작 하나(또는 <see cref="MediaPlayerKeyAction.None"/>=넘김)를 돌려준다. 실제 명령 실행은 View 가 한다.
/// 이렇게 분리해야 "미디어 없으면 Space 는 평소대로 다음 항목" "버튼에 포커스 있으면 Space/Enter 는 그 버튼 클릭"
/// 같은 우선순위 규칙을 단위 테스트로 고정할 수 있다(예전엔 View 안에 묻혀 테스트 불가였음).
/// </para>
/// </summary>
public static class MediaPlayerKeyRouter
{
    /// <param name="key">눌린 키.</param>
    /// <param name="hasModifiers">Ctrl/Alt/Shift 등 수식 키가 눌렸는가(눌렸으면 미디어 키로 안 씀).</param>
    /// <param name="isTextInputFocused">텍스트박스·편집 콤보에 포커스가 있는가(있으면 타이핑 보존 — 안 가로챔).</param>
    /// <param name="isButtonFocused">버튼류에 포커스가 있는가(있으면 Space/Enter 는 그 버튼을 눌러야 하므로 안 가로챔).</param>
    /// <param name="canExecute">그 동작의 미디어 명령이 지금 실행 가능한가(미디어가 라이브인가) 판단 콜백.</param>
    /// <returns>소비할 미디어 동작. 넘겨야 하면 <see cref="MediaPlayerKeyAction.None"/>.</returns>
    public static MediaPlayerKeyAction Resolve(
        Key key,
        bool hasModifiers,
        bool isTextInputFocused,
        bool isButtonFocused,
        System.Func<MediaPlayerKeyAction, bool> canExecute)
    {
        if (hasModifiers || isTextInputFocused)
        {
            return MediaPlayerKeyAction.None;
        }

        var action = MediaPlayerKeyMap.MapKey(key);
        if (action == MediaPlayerKeyAction.None)
        {
            return MediaPlayerKeyAction.None;
        }

        // 버튼에 포커스가 있으면 Space/Enter 는 그 버튼을 활성화해야 한다(WPF 기본 동작 보존) — 미디어가 안 가로챈다.
        // Esc/S/M 은 버튼 활성화 키가 아니라 버튼 포커스와 상관없이 미디어가 처리해도 안전하다.
        if (isButtonFocused && action is MediaPlayerKeyAction.PlayPause or MediaPlayerKeyAction.Restart)
        {
            return MediaPlayerKeyAction.None;
        }

        // 지금 그 동작이 실행 가능(미디어가 라이브)할 때만 소비. 아니면 넘겨서 평소 단축키(Space=다음 항목 등)가 살아 있게.
        return canExecute(action) ? action : MediaPlayerKeyAction.None;
    }
}
