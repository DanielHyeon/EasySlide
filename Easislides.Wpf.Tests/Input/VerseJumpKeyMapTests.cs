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
    [InlineData(Key.C, "c")] // 후렴
    [InlineData(Key.B, "b")] // 브릿지
    [InlineData(Key.T, "t")]
    [InlineData(Key.W, "w")]
    [InlineData(Key.E, "e")]
    [InlineData(Key.P, "p")]
    [InlineData(Key.Q, "q")]
    public void LetterKeys_MapToLowercaseSectionLabel(Key key, string expected)
        => VerseJumpKeyMap.MapKeyToLabel(key).Should().Be(expected);

    [Theory]
    [InlineData(Key.D0)]   // 0 은 절 번호로 쓰지 않음
    [InlineData(Key.A)]    // 약속되지 않은 문자
    [InlineData(Key.Z)]
    [InlineData(Key.F5)]   // 기능 키
    [InlineData(Key.Enter)]
    [InlineData(Key.Space)]
    [InlineData(Key.Left)] // 방향 키(순차 이동은 별도 단축키)
    public void UnmappedKeys_ReturnNull(Key key)
        => VerseJumpKeyMap.MapKeyToLabel(key).Should().BeNull();

    [Fact]
    public void DigitAndNumPad_MapToSameLabel()
    {
        // 상단 숫자열과 넘패드가 같은 절 라벨로 매핑되는지(운영자 키패드 선택 자유).
        for (var n = 1; n <= 9; n++)
        {
            var top = (Key)((int)Key.D0 + n);
            var pad = (Key)((int)Key.NumPad0 + n);
            VerseJumpKeyMap.MapKeyToLabel(top).Should().Be(VerseJumpKeyMap.MapKeyToLabel(pad), $"{n} 은 상단/넘패드 동일");
        }
    }
}
