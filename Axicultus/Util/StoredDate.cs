using System;
using System.Globalization;

namespace Axicultus.Util;

/// <summary>
/// Dates kept as text (SONG.LastModified, registry values). They are written as yyyy-MM-dd whatever the Windows
/// regional format is; reading also accepts what older versions wrote. Shared by the WinForms and WPF apps
/// (the WPF project links this file).
/// </summary>
internal static class StoredDate
{
    public const string Format = "yyyy-MM-dd";

    private static readonly string[] IsoFormats =
    [
        "yyyy-MM-dd", "yyyy-MM-dd HH:mm:ss", "yyyy-MM-dd HH:mm:ss.FFFFFFF", "yyyy-MM-ddTHH:mm:ss", "yyyy-MM-ddTHH:mm:ss.FFFFFFFK",
    ];

    private static readonly CultureInfo Korean = CultureInfo.GetCultureInfo("ko-KR");
    private static readonly CultureInfo English = CultureInfo.GetCultureInfo("en-US");

    public static string ToText(DateTime value)
    {
        return value.ToString(Format, CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Reads a stored date: the ISO forms above, or DateTime.ToString() of the current, Korean or English (US)
    /// regional format, which older versions wrote through a DataRow (for example "2026-11-18" or "2026. 11. 18."
    /// followed by the Korean AM designator and a time, or "11/18/2026 12:00:00 AM"). False when none fits.
    /// </summary>
    public static bool TryParse(object value, out DateTime result)
    {
        if (value is DateTime dateTime)
        {
            result = dateTime;
            return true;
        }
        string dateText = (value is null or DBNull) ? "" : (Convert.ToString(value, CultureInfo.InvariantCulture) ?? "").Trim();
        if (dateText.Length == 0)
        {
            result = default;
            return false;
        }
        // RoundtripKind: a value with an offset keeps its own date instead of moving to local time.
        if (DateTime.TryParseExact(dateText, IsoFormats, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out result))
        {
            return true;
        }
        foreach (CultureInfo culture in new[] { CultureInfo.CurrentCulture, Korean, English })
        {
            if (DateTime.TryParse(dateText, culture, DateTimeStyles.AllowWhiteSpaces, out result))
            {
                return true;
            }
        }
        result = default;
        return false;
    }

    /// <summary>True when the text already starts with yyyy-MM-dd (what the SQL date conditions compare).</summary>
    public static bool StartsWithIsoDate(string text)
    {
        return text != null && text.Length >= 10
            && DateTime.TryParseExact(text.Substring(0, 10), Format, CultureInfo.InvariantCulture, DateTimeStyles.None, out _);
    }
}
