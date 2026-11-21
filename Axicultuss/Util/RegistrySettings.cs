using System;
using System.IO;
using System.Security;
using Microsoft.Win32;

namespace Axicultus.Util;

internal interface IRegistrySettingsStore
{
    object ReadValue(string section, string name);
    void WriteValue(string section, string name, object value, RegistryValueKind kind);
    void DeleteValue(string section, string name);
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
        catch (Exception ex) when (IsStorageFailure(ex))
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
        catch (Exception ex) when (IsStorageFailure(ex))
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
        catch (Exception ex) when (IsStorageFailure(ex))
        {
            LogFailure("delete", section, name, ex);
            return false;
        }
    }

    private void LogFailure(string operation, string section, string name, Exception error) =>
        logFailure($"Settings {operation} {section}\\{name} failed ({error.GetType().Name}).");

    internal static bool IsStorageFailure(Exception error) =>
        error is System.Text.Json.JsonException or FormatException or IOException or SecurityException or UnauthorizedAccessException or ArgumentException or InvalidOperationException;
}
