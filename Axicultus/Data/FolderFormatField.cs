using System;
using System.Globalization;
using Axicultus.Wpf.Localization;

namespace Axicultus.Wpf.Data;

// Column names are the fixed FOLDER schema used by both applications. Older databases may omit columns.
// The label is the text named after the column.
public sealed record FolderFormatField(string Column, double? Minimum = null, double? Maximum = null,
    bool Decimal = false, string[]? Flags = null)
{
    public string Label => UiText.Get(nameof(FolderFormatField), Column);

    private static string[] FlagLabels(params string[] keys) => Array.ConvertAll(keys, key => UiText.Get(nameof(FolderFormatField), key));
    private static readonly string[] BodyFlags = FlagLabels("FlagBold", "FlagItalic", "FlagUnderline", "FlagChorusBold", "FlagChorusItalic", "FlagChorusUnderline", "FlagRightToLeft");
    public static readonly FolderFormatField[] All = [
        new("PreChorusHeading"), new("ChorusHeading"), new("BridgeHeading"), new("EndingHeading"),
        new("HeadingSize", 0, 150), new("HeadingOption", 0, 2),
        new("BIUHeading", Flags: FlagLabels("FlagBold", "FlagItalic", "FlagUnderline", "FlagChorusBold", "FlagChorusItalic", "FlagChorusUnderline")),
        new("FontName0"), new("Size0", 6, 100), new("Vpos0", 0, 100),
        new("BIU0", Flags: BodyFlags), new("LineSpacing", 0.5, 2, true),
        new("FontName1"), new("Size1", 6, 100), new("Vpos1", 0, 100),
        new("BIU1", Flags: BodyFlags), new("LineSpacing2", 0.5, 2, true),
        new("LMargin", 0, 99), new("RMargin", 0, 99), new("BMargin", 0, 99)
    ];

    public bool IsValid(string text)
    {
        if (Flags is not null) return int.TryParse(text, out var flags) && flags >= 0;
        if (Minimum is null) return text.Length <= 255;
        return double.TryParse(text, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var value)
            && value >= Minimum && value <= Maximum && (Decimal || value == Math.Truncate(value));
    }
}
