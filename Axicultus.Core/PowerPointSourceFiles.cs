using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Axicultus.Module;

public enum PowerPointSortOrder { HymnNumber, Title }

public static partial class PowerPointSourceFiles
{
    private static readonly StringComparer NumberedNameOrder = StringComparer.Create(
        CultureInfo.GetCultureInfo("ko-KR"), CompareOptions.IgnoreCase | CompareOptions.NumericOrdering);

    // Examples: "1장_만복의 근원 하나님", "40장 찬송으로 보답할 수 없는", "찬151장(배경O)".
    [GeneratedRegex(@"^\s*(?:(?:새찬송가|통일찬송가|찬송가|찬송|찬)\s*)?([0-9]+)(?:\s*장|(?=$|[\s_.-]))[\s_.-]*")]
    private static partial Regex HymnPrefix();

    public static IEnumerable<T> Sort<T>(IEnumerable<T> files, Func<T, string> getPath, PowerPointSortOrder order)
    {
        var keyed = files.Select(file =>
        {
            var path = getPath(file);
            var name = Path.GetFileNameWithoutExtension(path);
            var prefix = HymnPrefix().Match(name);
            var title = prefix.Success && prefix.Length < name.Length ? name[prefix.Length..] : name;
            return (File: file, Path: path, Name: name, Number: prefix.Groups[1].Value, Title: title);
        });

        var sorted = order == PowerPointSortOrder.Title
            ? keyed.OrderBy(item => item.Title, NumberedNameOrder)
            : keyed.OrderBy(item => item.Number.Length == 0).ThenBy(item => item.Number, NumberedNameOrder);
        return sorted.ThenBy(item => item.Name, NumberedNameOrder)
            .ThenBy(item => item.Path, NumberedNameOrder)
            .ThenBy(item => item.Path, StringComparer.Ordinal)
            .Select(item => item.File);
    }

    public static string[] ListFiles(string folder, bool includeSubfolders, PowerPointSortOrder order = PowerPointSortOrder.HymnNumber)
    {
        if (!Directory.Exists(folder)) return [];
        try
        {
            var files = Directory.EnumerateFiles(folder, "*", SourceFileFolder.SearchOptions(includeSubfolders))
                .Where(path => (path.EndsWith(".ppt", StringComparison.OrdinalIgnoreCase) ||
                                path.EndsWith(".pptx", StringComparison.OrdinalIgnoreCase)) &&
                               !Path.GetFileName(path.AsSpan()).StartsWith("~$", StringComparison.Ordinal));
            return Sort(files, path => path, order).ToArray();
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { return []; }
    }

    public static string[] ListSubfolders(string root)
    {
        if (!Directory.Exists(root)) return [];
        try
        {
            // Both folder pickers reserve the first of 255 built-in entries for the root.
            return Directory.EnumerateDirectories(root, "*", SourceFileFolder.SearchOptions(true))
                .Order(NumberedNameOrder)
                .ThenBy(path => path, StringComparer.Ordinal)
                .Take(254)
                .ToArray();
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { return []; }
    }
}
