using System.Windows.Input;
using Easislides.Wpf.Input;
using FluentAssertions;
using Xunit;

namespace Easislides.Wpf.Tests.Input;

/// <summary>예배 순서 목록 포커스 시 키 동작(순수 함수) 검증.</summary>
public class WorshipListKeyMapTests
{
    [Theory]
    [InlineData(Key.Delete, ModifierKeys.None, true)]              // Delete = 제거
    [InlineData(Key.Delete, ModifierKeys.Control, false)]         // 수식 키 있으면 아님(다른 동작과 충돌 회피)
    [InlineData(Key.Delete, ModifierKeys.Shift, false)]
    [InlineData(Key.Back, ModifierKeys.None, false)]              // Backspace 는 제거 아님
    [InlineData(Key.D, ModifierKeys.None, false)]                 // 다른 키 아님
    [InlineData(Key.Enter, ModifierKeys.None, false)]
    public void IsRemoveSelectedItem_TrueOnlyForPlainDelete(Key key, ModifierKeys modifiers, bool expected)
        => WorshipListKeyMap.IsRemoveSelectedItem(key, modifiers).Should().Be(expected);
}
