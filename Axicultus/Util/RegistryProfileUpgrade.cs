using Microsoft.Win32;

namespace Axicultus.Util;

internal static class RegistryProfileUpgrade
{
    internal const string CurrentPath = @"Software\Axicultuss";
    private const string PreviousPath = @"Software\Axicultus";
    private const string LegacyPath = @"Software\EasiSlides";
    private const string Imported = "SettingsProfileImportedV2";
    private const string PreviousMarker = "LegacyProfileImported";

    // The old product remains usable. A failed copy can resume without overwriting newer values.
    internal static void Import(RegistryKey userRoot)
    {
        using (var target = userRoot.OpenSubKey(CurrentPath))
            if (target?.GetValue(Imported) is 1) return;

        using var previous = userRoot.OpenSubKey(PreviousPath);
        using var legacy = previous?.GetValue(PreviousMarker) is 1 ? null : userRoot.OpenSubKey(LegacyPath);
        if (previous is null && legacy is null) return;
        using var current = userRoot.CreateSubKey(CurrentPath);
        if (current.GetValue(Imported) is 1) return;
        if (previous is not null) CopyMissing(previous, current, profileRoot: true);
        if (legacy is not null) CopyMissing(legacy, current, profileRoot: true);
        current.SetValue(Imported, 1, RegistryValueKind.DWord);
    }

    private static void CopyMissing(RegistryKey source, RegistryKey destination, bool profileRoot = false)
    {
        foreach (string name in source.GetValueNames())
        {
            if (profileRoot && (string.Equals(name, PreviousMarker, System.StringComparison.OrdinalIgnoreCase)
                || string.Equals(name, Imported, System.StringComparison.OrdinalIgnoreCase))) continue;
            if (destination.GetValue(name, null, RegistryValueOptions.DoNotExpandEnvironmentNames) != null) continue;
            object value = source.GetValue(name, null, RegistryValueOptions.DoNotExpandEnvironmentNames);
            if (value != null) destination.SetValue(name, value, source.GetValueKind(name));
        }
        foreach (string name in source.GetSubKeyNames())
        {
            using var child = source.OpenSubKey(name, writable: false);
            if (child is null) continue;
            using var target = destination.CreateSubKey(name);
            CopyMissing(child, target);
        }
    }

    internal static void Reset(RegistryKey userRoot)
    {
        userRoot.DeleteSubKeyTree(CurrentPath, throwOnMissingSubKey: false);
        using var cleared = userRoot.CreateSubKey(CurrentPath);
        cleared.SetValue(Imported, 1, RegistryValueKind.DWord);
    }
}
