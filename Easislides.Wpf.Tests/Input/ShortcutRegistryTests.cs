using System;
using System.Windows.Input;
using Easislides.Wpf.Input;
using FluentAssertions;
using Xunit;
using LegacyKeys = System.Windows.Forms.Keys;

namespace Easislides.Wpf.Tests.Input;

/// <summary>
/// ShortcutRegistry 단위 테스트 — ADR-0004 단축키 단일 소스 검증.
///
/// 현재는 Application.Current 의존성이 없지만, ShortcutRegistry가 향후 Resources를 참조하게 되면
/// race condition 발생 가능. 미래 회귀 방지를 위해 동일 collection 소속으로 직렬화.
/// </summary>
[Collection("WPF Application")]
public class ShortcutRegistryTests
{
    [Fact]
    public void Register_AddsShortcut_ToCollection()
    {
        var sut = new ShortcutRegistry();
        var s = new Shortcut(Key.F5, ModifierKeys.None, "Live.NextSlide", true, "다음 슬라이드");

        sut.Register(s);

        sut.All.Should().ContainSingle().Which.Should().Be(s);
    }

    [Fact]
    public void Register_DuplicateKey_Throws()
    {
        var sut = new ShortcutRegistry();
        sut.Register(new Shortcut(Key.F5, ModifierKeys.None, "Cmd.A", true, "A"));

        var act = () => sut.Register(new Shortcut(Key.F5, ModifierKeys.None, "Cmd.B", false, "B"));

        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*F5*");
    }

    [Fact]
    public void Register_SameKeyDifferentModifiers_Allowed()
    {
        var sut = new ShortcutRegistry();
        sut.Register(new Shortcut(Key.F, ModifierKeys.None, "Cmd.PlainF", true, "F"));
        sut.Register(new Shortcut(Key.F, ModifierKeys.Control, "Cmd.CtrlF", false, "Ctrl+F"));

        sut.All.Should().HaveCount(2);
    }

    [Fact]
    public void TryHandle_BoundCommand_ExecutesAndReturnsTrue()
    {
        var sut = new ShortcutRegistry();
        var executed = false;
        sut.Register(new Shortcut(Key.S, ModifierKeys.Control, "Edit.Save", false, "저장"));
        sut.Bind("Edit.Save", () => executed = true);

        var handled = sut.TryHandle(Key.S, ModifierKeys.Control);

        handled.Should().BeTrue();
        executed.Should().BeTrue();
    }

    [Fact]
    public void TryHandle_UnregisteredKey_ReturnsFalse()
    {
        var sut = new ShortcutRegistry();

        var handled = sut.TryHandle(Key.Z, ModifierKeys.None);

        handled.Should().BeFalse();
    }

    [Fact]
    public void TryHandle_UnboundCommand_ReturnsFalse()
    {
        var sut = new ShortcutRegistry();
        sut.Register(new Shortcut(Key.S, ModifierKeys.Control, "Edit.Save", false, "저장"));
        // Bind 호출 안 함

        var handled = sut.TryHandle(Key.S, ModifierKeys.Control);

        handled.Should().BeFalse();
    }

    [Fact]
    public void TryHandleGlobal_OnlyHandlesGlobalShortcuts()
    {
        var sut = new ShortcutRegistry();
        var executed = false;
        sut.Register(new Shortcut(Key.F5, ModifierKeys.None, "Live.NextSlide", IsGlobal: true, "F5 글로벌"));
        sut.Register(new Shortcut(Key.F6, ModifierKeys.None, "Edit.Save", IsGlobal: false, "F6 로컬"));
        sut.Bind("Live.NextSlide", () => executed = true);

        var handledGlobalF5 = sut.TryHandleGlobal(LegacyKeys.F5, LegacyKeys.None);
        var handledLocalF6 = sut.TryHandleGlobal(LegacyKeys.F6, LegacyKeys.None);

        handledGlobalF5.Should().BeTrue();
        executed.Should().BeTrue();
        handledLocalF6.Should().BeFalse("로컬 단축키는 TryHandleGlobal에서 처리되면 안 됨");
    }

    [Fact]
    public void TryHandle_HandlerThrowsException_ReturnsFalseAndDoesNotPropagate()
    {
        var sut = new ShortcutRegistry();
        sut.Register(new Shortcut(Key.X, ModifierKeys.None, "Bug.Throws", false, "예외 발생"));
        sut.Bind("Bug.Throws", () => throw new InvalidOperationException("simulated"));

        var act = () => sut.TryHandle(Key.X, ModifierKeys.None);

        // 핸들러 예외는 swallow돼야 함 (라이브 운영 중 한 핸들러 실패가 전체 UI 멈춤 유발 방지)
        act.Should().NotThrow();
        var result = sut.TryHandle(Key.X, ModifierKeys.None);
        result.Should().BeFalse();
    }

    [Fact]
    public void DisplayText_FormatsModifiersAndKey()
    {
        var s = new Shortcut(Key.S, ModifierKeys.Control | ModifierKeys.Shift, "Cmd", false, "Test");
        s.DisplayText.Should().Be("Ctrl+Shift+S");

        var space = new Shortcut(Key.Space, ModifierKeys.None, "Cmd", false, "Test");
        space.DisplayText.Should().Be("Space");
    }
}
