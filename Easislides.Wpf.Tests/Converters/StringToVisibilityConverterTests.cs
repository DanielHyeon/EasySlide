using System.Globalization;
using System.Windows;
using Easislides.Wpf.Converters;
using FluentAssertions;
using Xunit;

namespace Easislides.Wpf.Tests.Converters;

/// <summary>증분160-E — 문자열 유무 → Visibility(스테이지 가사 밴드를 내용이 있을 때만 보이게).</summary>
public class StringToVisibilityConverterTests
{
    [Theory]
    [InlineData("가사 한 줄", true)]
    [InlineData("", false)]
    [InlineData("   ", false)]   // 공백뿐도 숨김.
    [InlineData(null, false)]
    public void Convert_NonEmptyString_IsVisible(string? input, bool expectedVisible)
    {
        var sut = new StringToVisibilityConverter();

        var result = sut.Convert(input, typeof(Visibility), null, CultureInfo.InvariantCulture);

        result.Should().Be(expectedVisible ? Visibility.Visible : Visibility.Collapsed);
    }
}
