using System;
using System.IO;

namespace Axicultus;

public static class ProductStorage
{
    public const string ProductName = "Axicultus";
    public const string LegacyName = "EasiSlides";

    // Existing installations keep their chosen data directory. Fresh installations use the new name.
    public static string WorkingDirectory(string parent) => SelectDirectory(parent, ProductName, LegacyName);

    public static string WinFormsProfileDirectory(string parent) => Path.Combine(parent, "Axicultuss");

    public static string WinFormsWorkingDirectory(string parent)
    {
        var current = WinFormsProfileDirectory(parent);
        var previous = WorkingDirectory(parent);
        return !Directory.Exists(current) && Directory.Exists(previous) ? previous : current;
    }

    public static string WpfProfileDirectory(string parent) => Path.Combine(parent, ProductName);

    private static string SelectDirectory(string parent, string current, string previous)
    {
        string preferred = Path.Combine(parent, current);
        string legacy = Path.Combine(parent, previous);
        return !Directory.Exists(preferred) && Directory.Exists(legacy) ? legacy : preferred;
    }

    public static string SongDatabase(string workingFolder) =>
        SongDatabaseIn(Path.Combine(workingFolder, "Admin", "Database"));

    public static string SongDatabaseIn(string directory) => SelectFile(directory, "AxicultusDb.db", "EasiSlidesDb.db");

    public static string HelpFileIn(string directory) => SelectFile(directory, "AxicultusHelp.chm", "EasiSlidesHelp.chm");

    private static string SelectFile(string directory, string current, string previous)
    {
        string preferred = Path.Combine(directory, current);
        string legacy = Path.Combine(directory, previous);
        return !File.Exists(preferred) && File.Exists(legacy) ? legacy : preferred;
    }

    public static string ResolveLegacyBackground(string path, string? workingFolder)
    {
        const string oldImages = @"C:\EasiSlides\Images\";
        if (string.IsNullOrEmpty(path) || string.IsNullOrWhiteSpace(workingFolder)
            || !path.StartsWith(oldImages, StringComparison.OrdinalIgnoreCase) || File.Exists(path))
            return path;

        string relative = path[oldImages.Length..];
        if (Path.IsPathRooted(relative) || Array.IndexOf(relative.Split('\\', '/'), "..") >= 0)
            return path;

        string relocated = Path.Combine(workingFolder, "Images", relative);
        return File.Exists(relocated) ? relocated : path;
    }

    public static bool IsDocumentRoot(string name) => name is ProductName or LegacyName;
}
