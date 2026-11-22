using System;
using System.IO;
using System.Linq;

namespace Axicultus.Wpf.Settings;

internal static class WpfProfileUpgrade
{
    private const string CompletionFile = ".profile-imported-v2";

    // The copy is a convenience: a failure is reported and the caller still starts the app.
    internal static bool TryImport(string appData, Action<Exception> report)
    {
        try
        {
            Import(appData);
            return true;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or System.Security.SecurityException)
        {
            report(ex);
            return false;
        }
    }

    private const string SettingsFile = "settings.json";
    private static readonly string[] EarlierProfiles = ["AxicultusNext", "EasiSlidesNext"];

    // True while an earlier profile still holds a settings file that the current profile does not have: its copy
    // failed. Saving settings now would create the file and the retry on the next start would skip it for good.
    internal static bool EarlierSettingsPending(string appData)
    {
        string destination = ProductStorage.WpfProfileDirectory(appData);
        return !File.Exists(Path.Combine(destination, SettingsFile))
            && !File.Exists(Path.Combine(destination, CompletionFile))
            && EarlierProfiles.Any(name => File.Exists(Path.Combine(appData, name, SettingsFile)));
    }

    internal static void Import(string appData)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(appData);
        string destination = ProductStorage.WpfProfileDirectory(appData);
        CheckPath(destination);
        string marker = Path.Combine(destination, CompletionFile);
        if (CheckPath(marker) is { } markerAttributes && !markerAttributes.HasFlag(FileAttributes.Directory)) return;

        // settings.json first, on its own: whatever fails afterwards, the next start does not begin from defaults
        // and then keep them (an existing file is never overwritten).
        foreach (string name in EarlierProfiles)
        {
            if (CheckPath(Path.Combine(appData, name)) is null) continue;
            string settings = Path.Combine(appData, name, SettingsFile);
            if (CheckPath(settings) is not { } attributes || attributes.HasFlag(FileAttributes.Directory)) continue;
            Directory.CreateDirectory(destination);
            CopyMissingFile(settings, Path.Combine(destination, SettingsFile));
        }

        bool imported = false;
        foreach (string name in EarlierProfiles)
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
        // Files first: settings.json is copied before a subfolder that may fail.
        foreach (string sourcePath in Directory.EnumerateFiles(source).Concat(Directory.EnumerateDirectories(source)))
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
