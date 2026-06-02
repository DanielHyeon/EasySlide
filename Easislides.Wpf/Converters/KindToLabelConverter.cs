using System;
using System.Globalization;
using System.Windows.Data;
using Easislides.Wpf.Theme;

namespace Easislides.Wpf.Converters;

/// <summary>
/// 예배 순서 항목의 종류(string) → 사람이 읽는 한글 라벨("곡"·"성경"·"PowerPoint"·"미디어"·"공지"·"항목").
/// 목록 항목 툴팁("종류 — 제목")에 쓴다. 실제 매핑은 순수 헬퍼 <see cref="WorshipItemIcon.KindLabel"/> 가 한다.
/// </summary>
public sealed class KindToLabelConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => WorshipItemIcon.KindLabel(value as string);

    public object ConvertBack(object value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
