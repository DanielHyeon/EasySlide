using System;
using System.Globalization;
using System.Windows.Data;
using Easislides.Wpf.Theme;
using Wpf.Ui.Controls;

namespace Easislides.Wpf.Converters;

/// <summary>
/// 예배 순서 항목의 종류(string) → Fluent <see cref="SymbolRegular"/> 아이콘 변환기 — 목록 항목 앞에 종류 아이콘을 보여 준다.
/// 실제 매핑 규칙은 순수 헬퍼 <see cref="WorshipItemIcon.SymbolForKind"/> 에 있다(테스트는 그 함수만 검증).
/// </summary>
public sealed class KindToSymbolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => WorshipItemIcon.SymbolForKind(value as string);

    public object ConvertBack(object value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
