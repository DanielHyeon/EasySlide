using System.Windows.Input;
using Easislides.Wpf.Input;
using FluentAssertions;
using Xunit;

namespace Easislides.Wpf.Tests.Input;

public class VerseJumpKeyMapTests
{
    [Theory]
    [InlineData(Key.D1, "1")]
    [InlineData(Key.D5, "5")]
    [InlineData(Key.D9, "9")]
    [InlineData(Key.NumPad1, "1")]
    [InlineData(Key.NumPad9, "9")]
    public void DigitKeys_MapToVerseNumberLabel(Key key, string expected)
        => VerseJumpKeyMap.MapKeyToLabel(key).Should().Be(expected);

    [Theory]
    [InlineData(Key.C, ModifierKeys.None, "c")]
    [InlineData(Key.B, ModifierKeys.None, "b")]
    [InlineData(Key.E, ModifierKeys.None, "e")]
    [InlineData(Key.P, ModifierKeys.None, "p")]
    [InlineData(Key.C, ModifierKeys.Shift, "c")]
    [InlineData(Key.B, ModifierKeys.Shift, "w")]
    [InlineData(Key.W, ModifierKeys.Shift, "w")]
    [InlineData(Key.E, ModifierKeys.Shift, "e")]
    [InlineData(Key.P, ModifierKeys.Shift, "q")]
    [InlineData(Key.Q, ModifierKeys.Shift, "q")]
    [InlineData(Key.T, ModifierKeys.Shift, "t")]
    public void FrmMainLetterKeys_MapToLegacySectionLabel(Key key, ModifierKeys modifiers, string expected)
        => VerseJumpKeyMap.MapKeyToLabel(key, modifiers).Should().Be(expected);

    [Theory]
    [InlineData(Key.D1, ModifierKeys.Shift, "1")]
    [InlineData(Key.D5, ModifierKeys.Shift, "5")]
    [InlineData(Key.NumPad9, ModifierKeys.Shift, "9")]
    public void DigitKeys_WithShift_StillMapToVerseNumberLabel(Key key, ModifierKeys modifiers, string expected)
        => VerseJumpKeyMap.MapKeyToLabel(key, modifiers).Should().Be(expected);

    [Theory]
    [InlineData(Key.D0)]
    [InlineData(Key.A)]
    [InlineData(Key.Z)]
    [InlineData(Key.F5)]
    [InlineData(Key.Enter)]
    [InlineData(Key.Space)]
    [InlineData(Key.Left)]
    public void UnmappedKeys_ReturnNull(Key key)
        => VerseJumpKeyMap.MapKeyToLabel(key).Should().BeNull();

    [Theory]
    [InlineData(Key.W, ModifierKeys.None)]
    [InlineData(Key.Q, ModifierKeys.None)]
    [InlineData(Key.T, ModifierKeys.None)]
    [InlineData(Key.B, ModifierKeys.Control)]
    [InlineData(Key.P, ModifierKeys.Alt)]
    [InlineData(Key.E, ModifierKeys.Windows)]
    public void NonFrmMainVerseShortcutCombinations_ReturnNull(Key key, ModifierKeys modifiers)
        => VerseJumpKeyMap.MapKeyToLabel(key, modifiers).Should().BeNull();

    [Fact]
    public void DigitAndNumPad_MapToSameLabel()
    {
        for (var n = 1; n <= 9; n++)
        {
            var top = (Key)((int)Key.D0 + n);
            var pad = (Key)((int)Key.NumPad0 + n);
            VerseJumpKeyMap.MapKeyToLabel(top).Should().Be(VerseJumpKeyMap.MapKeyToLabel(pad), $"{n} maps consistently");
        }
    }
}
