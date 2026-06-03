using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Easislides.Wpf.Converters;

/// <summary>
/// 문자열이 비어있지 않으면 <see cref="Visibility.Visible"/>, 비었거나 공백뿐이면 <see cref="Visibility.Collapsed"/>.
/// 스테이지(Preview) 모니터가 가사 밴드를 "내용이 있을 때만" 보이게 하는 데 쓴다 — 출력 VM 의 본문 가시성
/// (BodyTextVisibility)은 외곽선·인터레이스 효과가 켜지면 Collapsed 가 되지만(출력은 별도 컨트롤로 그림),
/// 스테이지엔 그 컨트롤이 없어 그러면 가사가 통째로 사라진다. 그래서 효과 게이트 대신 본문 문자열 유무로만 판단한다.
/// </summary>
public sealed class StringToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => string.IsNullOrWhiteSpace(value as string) ? Visibility.Collapsed : Visibility.Visible;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
