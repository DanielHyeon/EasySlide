using System;
using System.IO;
using System.Security;
using Microsoft.Win32;

namespace Easislides.Util;

internal interface IRegistrySettingsStore
{
    object ReadValue(string section, string name);
    void WriteValue(string section, string name, object value, RegistryValueKind kind);
    void DeleteValue(string section, string name);
}

internal sealed class WindowsRegistrySettingsStore(RegistryKey root, string basePath) : IRegistrySettingsStore
{
    public object ReadValue(string section, string name)
    {
        using var key = root.OpenSubKey(basePath + section, writable: false);
        if (key == null)
            return null;

        object value = key.GetValue(name);
        if (value == null && ContainsValue(key, name))
            throw new IOException("The registry value exists but could not be read.");
        return value;
    }

    public void WriteValue(string section, string name, object value, RegistryValueKind kind)
    {
        using var key = root.CreateSubKey(basePath + section)
            ?? throw new IOException("Could not open the registry key for writing.");
        key.SetValue(name, value, kind);
    }

    public void DeleteValue(string section, string name)
    {
        using var key = root.OpenSubKey(basePath + section, writable: true);
        if (key == null)
            return;

        key.DeleteValue(name, throwOnMissingValue: false);
        // DeleteValue는 일부 네이티브 오류를 예외로 전달하지 않는다.
        if (ContainsValue(key, name))
            throw new IOException("The registry value remains after deletion.");
    }

    private static bool ContainsValue(RegistryKey key, string name) =>
        Array.Exists(key.GetValueNames(), valueName => string.Equals(valueName, name ?? "", StringComparison.OrdinalIgnoreCase));
}

internal sealed class RegistrySettings(IRegistrySettingsStore store, Action<string> logFailure)
{
    internal bool TryReadValue(string section, string name, out object value)
    {
        try
        {
            value = store.ReadValue(section, name);
            return true;
        }
        catch (Exception ex) when (IsRegistryFailure(ex))
        {
            value = null;
            LogFailure("read", section, name, ex);
            return false;
        }
    }

    internal bool TryWriteValue(string section, string name, object value, RegistryValueKind kind)
    {
        try
        {
            store.WriteValue(section, name, value, kind);
            return true;
        }
        catch (Exception ex) when (IsRegistryFailure(ex))
        {
            LogFailure("write", section, name, ex);
            return false;
        }
    }

    internal bool TryDeleteValue(string section, string name)
    {
        try
        {
            store.DeleteValue(section, name);
            return true;
        }
        catch (Exception ex) when (IsRegistryFailure(ex))
        {
            LogFailure("delete", section, name, ex);
            return false;
        }
    }

    private void LogFailure(string operation, string section, string name, Exception error) =>
        logFailure($"Registry {operation} {section}\\{name} failed ({error.GetType().Name}).");

    private static bool IsRegistryFailure(Exception error) =>
        error is IOException or SecurityException or UnauthorizedAccessException or ArgumentException or InvalidOperationException;
}
