using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Axicultus.Wpf.Library;

/// <summary>
/// The file extensions of each kind of source file: one list for the browse lists and the file dialogs.
/// Audio and video come from AudioExtensions.txt and VideoExtensions.txt, the lists the WinForms app reads
/// and the operator can edit; the built-in lists are used only when neither copy of a file can be read.
/// </summary>
public static class SourceFileExtensions
{
    /// <summary>What the PowerPoint browser (PowerPointSourceFiles.ListFiles) and the WinForms app list.</summary>
    public static IReadOnlyList<string> PowerPoint { get; } = [".ppt", ".pptx"];

    private static readonly string[] BuiltInAudio = [".mp3", ".wav", ".wma", ".m4a", ".aac", ".flac", ".ogg"];

    private static readonly string[] BuiltInVideo = [".mp4", ".m4v", ".mov", ".avi", ".wmv", ".mpg", ".mpeg", ".mkv", ".webm"];

    public static IReadOnlyList<string> Audio(string? workingFolder)
        => Read(workingFolder, DefaultsFolder, "AudioExtensions.txt", BuiltInAudio);

    public static IReadOnlyList<string> Video(string? workingFolder)
        => Read(workingFolder, DefaultsFolder, "VideoExtensions.txt", BuiltInVideo);

    /// <summary>Audio and video together, for a lookup by extension.</summary>
    public static IReadOnlySet<string> Media(string? workingFolder)
        => new HashSet<string>(Audio(workingFolder).Concat(Video(workingFolder)), StringComparer.OrdinalIgnoreCase);

    /// <summary>One entry of a file dialog filter: "Label (*.a;*.b)|*.a;*.b".</summary>
    public static string DialogFilter(string label, IEnumerable<string> extensions)
    {
        var patterns = string.Join(";", extensions.Select(extension => "*" + extension));
        return $"{label} ({patterns})|{patterns}";
    }

    // The build copies the shipped lists here.
    private static string DefaultsFolder => Path.Combine(AppContext.BaseDirectory, "Sys");

    // The working folder's copy is the one the WinForms app reads and writes; it wins over the shipped one.
    internal static IReadOnlyList<string> Read(string? workingFolder, string defaultsFolder, string fileName, IReadOnlyList<string> builtIn)
    {
        string?[] folders =
        [
            string.IsNullOrWhiteSpace(workingFolder) ? null : Path.Combine(workingFolder, "Admin", "Database"),
            defaultsFolder,
        ];
        foreach (var folder in folders)
        {
            if (folder is null)
            {
                continue;
            }

            try
            {
                var path = Path.Combine(folder, fileName);
                if (!File.Exists(path))
                {
                    continue;
                }

                // One extension on each line, with or without the dot, as the WinForms app accepts them.
                var extensions = File.ReadLines(path)
                    .Select(line => line.Trim().TrimStart('.').ToLowerInvariant())
                    .Where(extension => extension.Length > 0 && extension.IndexOfAny(Path.GetInvalidFileNameChars()) < 0)
                    .Select(extension => "." + extension)
                    .Distinct()
                    .ToArray();
                if (extensions.Length > 0)
                {
                    return extensions;
                }
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException)
            {
            }
        }

        return builtIn;
    }
}
