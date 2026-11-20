using System;
using System.Globalization;
using System.Windows.Data;
using Axicultus.Wpf.Shell;

namespace Axicultus.Wpf.Converters;

public sealed class KindToFrmIconConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not LiveQueueItem item) return string.Empty;
        if (item.Id.StartsWith("esw:W:", StringComparison.OrdinalIgnoreCase)
            || string.Equals(item.Kind, "W", StringComparison.OrdinalIgnoreCase)) return "word_badge";
        if (item.Id.StartsWith("text-file:", StringComparison.OrdinalIgnoreCase)
            || item.Id.StartsWith("esw:T:", StringComparison.OrdinalIgnoreCase)
            || string.Equals(item.Kind, "T", StringComparison.OrdinalIgnoreCase)
            || string.Equals(item.Kind, "Text", StringComparison.OrdinalIgnoreCase)) return "text_t";
        if (LiveItemKindMatcher.IsSong(item.Kind)) return "music_note_2";
        if (LiveItemKindMatcher.IsBible(item.Kind)) return "bible";
        if (LiveItemKindMatcher.IsPowerPoint(item.Kind)) return "powerpoint_badge";
        if (LiveItemKindMatcher.IsMedia(item.Kind)) return "video";
        if (LiveItemKindMatcher.IsNotice(item.Kind)) return "info";
        return "document_text";
    }

    public object ConvertBack(object value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
