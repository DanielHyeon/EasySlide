using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Easislides.Wpf.Shell;

namespace Easislides.Wpf.Converters;

/// <summary>
/// 예배 순서 항목의 "LIVE" 표시 가시성 변환기 — 입력 [항목 Id(string), 라이브 항목 Id(string)] →
/// 두 Id 가 같으면 Visible(이 항목이 송출 중), 아니면 Collapsed. 실제 판단은 순수 헬퍼 <see cref="WorshipLiveState.IsLive"/> 가 한다.
/// </summary>
public sealed class LiveItemBadgeConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values is null || values.Length < 2)
        {
            return Visibility.Collapsed;
        }

        var itemId = values[0] as string;
        var liveItemId = values[1] as string;
        return WorshipLiveState.IsLive(itemId, liveItemId) ? Visibility.Visible : Visibility.Collapsed;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
