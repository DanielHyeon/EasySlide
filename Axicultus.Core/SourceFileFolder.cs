using System;
using System.Collections.Generic;
using System.IO;

namespace Axicultus.Module;

public static class SourceFileFolder
{
    // Keep the built-in folders in place, including their indices used by WinForms.
    public static int IncludeSelection<T>(IList<T> folders, string? configuredPath,
        Func<T, string> getPath, Func<string, T> create)
    {
        if (string.IsNullOrWhiteSpace(configuredPath) || !Path.IsPathFullyQualified(configuredPath))
            return folders.Count == 0 ? -1 : 0;

        var path = Path.TrimEndingDirectorySeparator(configuredPath);
        for (var index = 0; index < folders.Count; index++)
            if (string.Equals(path, Path.TrimEndingDirectorySeparator(getPath(folders[index])), StringComparison.OrdinalIgnoreCase))
                return index;

        folders.Add(create(path));
        return folders.Count - 1;
    }
}
