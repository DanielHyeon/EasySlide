using System;
using System.IO;

namespace Axicultus.Wpf.Settings;

internal static class WpfProfileUpgrade
{
    private const string CompletionFile = ".profile-imported-v2";

    internal static void Import(string appData)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(appData);
        string destination = ProductStorage.WpfProfileDirectory(appData);
        CheckPath(destination);
        string marker = Path.Combine(destination, CompletionFile);
        if (CheckPath(marker) is { } markerAttributes && !markerAttributes.HasFlag(FileAttributes.Directory)) return;

        bool imported = false;
        foreach (string name in new[] { "AxicultusNext", "EasiSlidesNext" })
        {
            string source = Path.Combine(appData, name);
            if (CheckPath(source) is null) continue;
            CopyDirectory(source, destination, profileRoot: true);
            imported = true;
        }

        if (imported)
        {
            try { using var completed = new FileStream(marker, FileMode.CreateNew, FileAccess.Write, FileShare.Read); }
            catch (IOException) when (CheckPath(marker) is { } existing && !existing.HasFlag(FileAttributes.Directory)) { }
        }
    }

    private static void CopyDirectory(string source, string destination, bool profileRoot = false)
    {
        CheckPath(destination);
        Directory.CreateDirectory(destination);
        foreach (string sourcePath in Directory.EnumerateFileSystemEntries(source))
        {
            string name = Path.GetFileName(sourcePath);
            if (profileRoot && name.Equals(CompletionFile, StringComparison.OrdinalIgnoreCase)) continue;
            var attributes = CheckPath(sourcePath) ?? throw new FileNotFoundException("Profile entry disappeared.", sourcePath);
            string target = Path.Combine(destination, name);
            if (attributes.HasFlag(FileAttributes.Directory))
                CopyDirectory(sourcePath, target);
            else
                CopyMissingFile(sourcePath, target);
        }
    }

    private static void CopyMissingFile(string source, string destination)
    {
        if (CheckPath(destination) is { } existing && !existing.HasFlag(FileAttributes.Directory)) return;
        string staging = Path.Combine(Path.GetDirectoryName(destination)!, Path.GetRandomFileName());
        using var input = File.OpenRead(source);
        var output = new FileStream(staging, FileMode.CreateNew, FileAccess.Write, FileShare.None);
        try
        {
            using (output) input.CopyTo(output);
            try { File.Move(staging, destination); }
            catch (IOException) when (CheckPath(destination) is { } winner && !winner.HasFlag(FileAttributes.Directory)) { }
        }
        finally { File.Delete(staging); }
    }

    private static FileAttributes? CheckPath(string path)
    {
        FileAttributes attributes;
        try { attributes = File.GetAttributes(path); }
        catch (FileNotFoundException) { return null; }
        catch (DirectoryNotFoundException) { return null; }
        if (attributes.HasFlag(FileAttributes.ReparsePoint))
            throw new IOException($"설정 이전 경로에 연결 지점이 있습니다. {path}");
        return attributes;
    }
}
