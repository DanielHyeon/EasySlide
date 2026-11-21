using System;
using System.Collections.Generic;
using System.IO;
using System.Security;
using Microsoft.Win32;

namespace Axicultus.Util;

public class OfficeVersion
{

    private Dictionary<string, string> LatestVersions = [];
    private readonly Func<string, string, string> readOfficeValue;

    public OfficeVersion()
        : this((key, name) => ReadRegistryString(RegistryHive.LocalMachine, key, name))
    {
    }

    internal OfficeVersion(
        Func<string, string, string> readOfficeValue)
    {
        this.readOfficeValue = readOfficeValue;
        LatestVersions.Add("12.0", "Office2007");
        LatestVersions.Add("14.0", "Office2010");
        LatestVersions.Add("15.0", "Office2013");
        LatestVersions.Add("16.0", "Office2017 and higher ");
    }

    private string GetVersionNumberFromRegistry()
    {
        string regVersion;

        regVersion = GetVersionNumberFromRegistry("SOFTWARE\\Microsoft\\Office\\") ?? GetVersionNumberFromRegistry("SOFTWARE\\Wow6432Node\\Microsoft\\Office\\");
        return regVersion;
    }
    private string GetVersionNumberFromRegistry(string key)
    {
        string version = null;
        foreach (string VerNo in LatestVersions.Keys)
        {
            string offPath = readOfficeValue(key + VerNo + "\\PowerPoint\\InstallRoot", "Path");
            if (!string.IsNullOrWhiteSpace(offPath))
            {
                version = VerNo;
                break;
            }
        }
        return version;
    }
    public string GetVersion()
    {
        string versionFromReg = GetVersionNumberFromRegistry();
        if (versionFromReg == null)
            return null;
        string versionInstalled = LatestVersions[versionFromReg];

        bool? Office64BitFromReg = Off64Bit("SOFTWARE\\Microsoft\\Office\\", versionFromReg) ?? Off64Bit("SOFTWARE\\Wow6432Node\\Microsoft\\Office\\", versionFromReg);
        if (Office64BitFromReg.HasValue && Office64BitFromReg.Value)
            versionInstalled += " (64 bit)";
        else if (Office64BitFromReg.HasValue && !Office64BitFromReg.Value)
            versionInstalled += " (32 bit)";
        else
        {
            versionInstalled += " (Unknown bit)";
        }

        return versionInstalled;
    }

    private bool? Off64Bit(string key, string version)
    {
        bool? Office64BitFromReg = null;
        string OfficeVersionKey = $@"{key}{version}\PowerPoint";

        string Bitness = readOfficeValue(OfficeVersionKey, "Bitness");
        if (Bitness == "x86")
            Office64BitFromReg = false;
        else if ((Bitness == "x64"))
            Office64BitFromReg = true;
        return Office64BitFromReg;
    }

    internal static string ReadRegistryString(RegistryHive hive, string path, string valueName)
    {
        try
        {
            using var root = RegistryKey.OpenBaseKey(hive, RegistryView.Registry64);
            using var key = root.OpenSubKey(path, writable: false);
            return key?.GetValue(valueName, null, RegistryValueOptions.DoNotExpandEnvironmentNames) as string;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or SecurityException)
        {
            System.Diagnostics.Trace.TraceWarning($"Office registry lookup failed for {path}: {ex.Message}");
            return null;
        }
    }

}
