using System;
using System.Globalization;
using System.Linq;

namespace Axicultus.Wpf.Media;

public sealed record ItemMediaOptions(int Mode = -1, string FileName = "", int? Volume = null, int? Balance = null,
    int? Flags = null, int Camera = 1, string Monitor = "")
{
    public static ItemMediaOptions Read(string? format)
    {
        var options = new ItemMediaOptions();
        foreach (string field in (format ?? "").Split('>', StringSplitOptions.RemoveEmptyEntries))
        {
            int split = field.IndexOf('=');
            if (split < 1) continue;
            string value = field[(split + 1)..];
            bool number = int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int n);
            options = field[..split] switch
            {
                "50" when number && n is >= 0 and <= 3 => options with { Mode = n },
                "51" => options with { FileName = value },
                "52" when number && n is >= 0 and <= 100 => options with { Volume = n },
                "53" when number && n is >= -100 and <= 100 => options with { Balance = n },
                "54" when number && n >= 0 => options with { Flags = n },
                "55" when number && n is >= 1 and <= 5 => options with { Camera = n },
                "56" => options with { Monitor = value },
                _ => options
            };
        }
        return options;
    }

    public string ApplyTo(string? format)
    {
        var fields = (format ?? "").Split('>', StringSplitOptions.RemoveEmptyEntries)
            .Where(field => field.IndexOf('=') is not 2 || !int.TryParse(field.AsSpan(0, 2), out int key) || key is < 50 or > 56).ToList();
        void Set(int key, object? value)
        {
            if (value is not null && !string.Equals(value.ToString(), "", StringComparison.Ordinal))
                fields.Add(FormattableString.Invariant($"{key}={value}"));
        }
        if (Mode >= 0) Set(50, Mode);
        Set(51, FileName);
        Set(52, Volume);
        Set(53, Balance);
        Set(54, Flags);
        if (Mode == 3 || Camera != 1) Set(55, Camera);
        Set(56, Monitor);
        return fields.Count == 0 ? "" : string.Join('>', fields) + ">";
    }
}
