using System;
using System.Globalization;
using System.Windows.Data;
using Easislides.Wpf.Library;

namespace Easislides.Wpf.Converters;

/// <summary>
/// 곡 목록 한 줄의 표시 문자열을 만드는 다중 바인딩 변환기 — 입력 [제목(string), 곡번호(int), 곡번호표시(bool)] →
/// "번호. 제목" 또는 "제목"(레거시 Use Song Numbering). 실제 규칙은 순수 헬퍼 <see cref="SongListFormatting.FormatRow"/> 에 있다.
/// </summary>
public sealed class SongRowDisplayConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values is null || values.Length < 3)
        {
            return string.Empty;
        }

        var title = values[0] as string;
        var songNumber = values[1] is int n ? n : 0;
        var useNumbering = values[2] is bool b && b;
        return SongListFormatting.FormatRow(title, songNumber, useNumbering);
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
