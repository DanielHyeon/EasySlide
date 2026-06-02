using System.Windows.Input;
using Easislides.Wpf.Input;
using FluentAssertions;
using Xunit;

namespace Easislides.Wpf.Tests.Input;

public class MediaPlayerKeyMapTests
{
    [Theory]
    // 레거시 FrmMain.HandleMediaPlayerKey 와 동일한 매핑.
    [InlineData(Key.Space, MediaPlayerKeyAction.PlayPause)]
    [InlineData(Key.Escape, MediaPlayerKeyAction.Stop)]
    [InlineData(Key.S, MediaPlayerKeyAction.Stop)]
    [InlineData(Key.Enter, MediaPlayerKeyAction.Restart)]
    [InlineData(Key.M, MediaPlayerKeyAction.ToggleMute)]
    public void MapKey_ReturnsLegacyAction(Key key, MediaPlayerKeyAction expected)
        => MediaPlayerKeyMap.MapKey(key).Should().Be(expected);

    [Fact]
    public void MapKey_EnterAndReturn_AreSameAction()
    {
        // WPF 에서 Key.Enter 와 Key.Return 은 같은 값 → 둘 다 "처음부터 다시 재생".
        MediaPlayerKeyMap.MapKey(Key.Return).Should().Be(MediaPlayerKeyAction.Restart);
        MediaPlayerKeyMap.MapKey(Key.Enter).Should().Be(MediaPlayerKeyAction.Restart);
    }

    [Theory]
    // 미디어 동작이 아닌 키는 None — 평소 단축키(절 점프·다음 항목 등)가 처리하도록 넘긴다.
    [InlineData(Key.A)]
    [InlineData(Key.D1)]
    [InlineData(Key.F12)]
    [InlineData(Key.Left)]
    [InlineData(Key.P)]
    public void MapKey_UnmappedKey_ReturnsNone(Key key)
        => MediaPlayerKeyMap.MapKey(key).Should().Be(MediaPlayerKeyAction.None);

    // ── 라우터(우선순위·포커스·실행가능 결정) ─────────────────────────────────────────
    // 모든 동작이 실행 가능(미디어 라이브)하다고 보는 콜백.
    private static bool AllExecutable(MediaPlayerKeyAction _) => true;

    [Fact]
    public void Resolve_MediaLive_NoFocus_ReturnsMappedAction()
    {
        // 미디어가 라이브이고 입력/버튼 포커스가 없으면 매핑된 동작을 그대로 소비한다.
        MediaPlayerKeyRouter.Resolve(Key.Space, hasModifiers: false, isTextInputFocused: false, isButtonFocused: false, AllExecutable)
            .Should().Be(MediaPlayerKeyAction.PlayPause);
        MediaPlayerKeyRouter.Resolve(Key.S, false, false, false, AllExecutable)
            .Should().Be(MediaPlayerKeyAction.Stop);
        MediaPlayerKeyRouter.Resolve(Key.M, false, false, false, AllExecutable)
            .Should().Be(MediaPlayerKeyAction.ToggleMute);
    }

    [Fact]
    public void Resolve_NoMedia_ReturnsNone_SoNormalShortcutSurvives()
    {
        // 미디어가 없으면(어떤 동작도 실행 불가) Space 는 None → 평소처럼 "다음 항목" 단축키가 동작한다.
        MediaPlayerKeyRouter.Resolve(Key.Space, false, false, false, _ => false)
            .Should().Be(MediaPlayerKeyAction.None);
    }

    [Fact]
    public void Resolve_WithModifiers_ReturnsNone()
    {
        // 수식 키(Ctrl 등)가 섞이면 미디어 키로 가로채지 않는다.
        MediaPlayerKeyRouter.Resolve(Key.Space, hasModifiers: true, false, false, AllExecutable)
            .Should().Be(MediaPlayerKeyAction.None);
    }

    [Fact]
    public void Resolve_TextInputFocused_ReturnsNone()
    {
        // 텍스트 입력 중이면(검색창 등) Space 타이핑을 가로채지 않는다.
        MediaPlayerKeyRouter.Resolve(Key.Space, false, isTextInputFocused: true, false, AllExecutable)
            .Should().Be(MediaPlayerKeyAction.None);
    }

    [Fact]
    public void Resolve_ButtonFocused_SpaceAndEnter_ReturnNone_ButEscSMStillAct()
    {
        // 버튼에 포커스가 있으면 Space/Enter 는 그 버튼을 눌러야 하므로 미디어가 안 가로챈다(회귀 방지).
        MediaPlayerKeyRouter.Resolve(Key.Space, false, false, isButtonFocused: true, AllExecutable)
            .Should().Be(MediaPlayerKeyAction.None, "Space 는 포커스된 버튼을 클릭해야 함");
        MediaPlayerKeyRouter.Resolve(Key.Enter, false, false, isButtonFocused: true, AllExecutable)
            .Should().Be(MediaPlayerKeyAction.None, "Enter 는 포커스된 버튼을 클릭해야 함");

        // Esc/S/M 은 버튼 활성화 키가 아니라 버튼 포커스와 무관하게 미디어가 처리한다.
        MediaPlayerKeyRouter.Resolve(Key.Escape, false, false, isButtonFocused: true, AllExecutable)
            .Should().Be(MediaPlayerKeyAction.Stop);
        MediaPlayerKeyRouter.Resolve(Key.S, false, false, isButtonFocused: true, AllExecutable)
            .Should().Be(MediaPlayerKeyAction.Stop);
        MediaPlayerKeyRouter.Resolve(Key.M, false, false, isButtonFocused: true, AllExecutable)
            .Should().Be(MediaPlayerKeyAction.ToggleMute);
    }

    [Fact]
    public void Resolve_UnmappedKey_ReturnsNone()
        => MediaPlayerKeyRouter.Resolve(Key.A, false, false, false, AllExecutable)
            .Should().Be(MediaPlayerKeyAction.None);
}
