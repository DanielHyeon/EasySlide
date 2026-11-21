using System;
using System.Globalization;
using System.Windows.Data;
using Axicultus.Core;
using Axicultus.Wpf.Shell;

namespace Axicultus.Wpf.Converters;

public sealed class KindToFrmIconConverter : IValueConverter
{
    public bool ForWorshipList { get; set; }

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not LiveQueueItem item) return string.Empty;
        var sourceType = LiveItemKindMatcher.ResolveLegacyTypeCode(item);
        if (sourceType == "W"
            || string.Equals(item.Kind, "W", StringComparison.OrdinalIgnoreCase)) return "word_badge";
        if (sourceType == "T" || item.Id.StartsWith("text-file:", StringComparison.OrdinalIgnoreCase)
            || string.Equals(item.Kind, "T", StringComparison.OrdinalIgnoreCase)
            || string.Equals(item.Kind, "Text", StringComparison.OrdinalIgnoreCase)
            || (ForWorshipList && item.Id.StartsWith("text:", StringComparison.OrdinalIgnoreCase)))
        {
            if (!ForWorshipList) return "text_t";
            return WorshipRowTitle.UsesBibleIcon(item.Title) ? "bible" : "clipboard_task_list_ltr";
        }
        if (LiveItemKindMatcher.IsSong(LiveItemKindMatcher.InferLegacyKind(item)))
            return ForWorshipList && WorshipRowTitle.UsesBibleIcon(item.Title) ? "bible" : "music_note_2";
        if (LiveItemKindMatcher.IsBible(item.Kind)) return "bible";
        if (LiveItemKindMatcher.IsPowerPoint(item.Kind)) return "powerpoint_badge";
        if (LiveItemKindMatcher.IsMedia(item.Kind)) return "video";
        if (LiveItemKindMatcher.IsNotice(item.Kind)) return "info";
        return "document_text";
    }

    public object ConvertBack(object value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
