using System;
using System.Globalization;
using System.IO;

namespace Axicultus.Core;

public static class WorshipRowTitle
{
    // WinForms displays the stem; WPF also shows the extension on imported text files.
    // Operators often decorate these titles with stars, squares or spaces.
    public static bool UsesBibleIcon(string title)
    {
        var name = Path.GetFileName(title.AsSpan().Trim());
        if (name.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
            name = name[..^4];

        var comparer = CultureInfo.InvariantCulture.CompareInfo;
        return comparer.Compare(name, "사도신경", CompareOptions.IgnoreSymbols) == 0
            || comparer.Compare(name, "주기도문", CompareOptions.IgnoreSymbols) == 0
            || comparer.Compare(name, "교회말씀", CompareOptions.IgnoreSymbols) == 0
            || comparer.Compare(name, "우리교회말씀", CompareOptions.IgnoreSymbols) == 0;
    }
}
