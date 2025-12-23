using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Easislides.Util
{
    public class OfficeVersion
    {

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]

        static extern uint RegOpenKeyEx(UIntPtr hKey, string lpSubKey, uint ulOptions, int samDesired, out int phkResult);
        [DllImport("Advapi32.dll")]
        static extern uint RegCloseKey(int hKey);
        [DllImport("advapi32.dll", EntryPoint = "RegQueryValueEx")]
        static extern int RegQueryValueEx(int hKey, string lpValueName, int lpReserved, ref uint lpType,
            System.Text.StringBuilder lpData, ref uint lpcbData);
        private static UIntPtr HKEY_LOCAL_MACHINE = new UIntPtr(0x80000002u);
        private static UIntPtr HKEY_CURRENT_USER = new UIntPtr(0x80000001u);

        private Dictionary<string, string> LatestVersions = new Dictionary<string, string>();

        public OfficeVersion()
        {
            LatestVersions.Add("12.0", "Office2007");
            LatestVersions.Add("14.0", "Office2010");
            LatestVersions.Add("15.0", "Office2013");
            LatestVersions.Add("16.0", "Office2017 and higher ");
        }

        private string GetVersionNumberFromRegistry()
        {
            string regVersion;

            regVersion = GetVersionNumberFromRegistry("SOFTWARE\\Microsoft\\Office\\");
            if (regVersion == null)
                regVersion = GetVersionNumberFromRegistry("SOFTWARE\\Wow6432Node\\Microsoft\\Office\\");

            return regVersion;
        }
        private string GetVersionNumberFromRegistry(string key)
        {
            string version = null;
            foreach (string VerNo in LatestVersions.Keys)
            {
                string offPath = Reg64(HKEY_LOCAL_MACHINE, key + VerNo + "\\PowerPoint\\InstallRoot", "Path");
                if (offPath != null)
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
            string versionInstalled = LatestVersions[versionFromReg];

            bool? Office64BitFromReg = Off64Bit("SOFTWARE\\Microsoft\\Office\\", versionFromReg);

            if (Office64BitFromReg == null)
                Office64BitFromReg = Off64Bit("SOFTWARE\\Wow6432Node\\Microsoft\\Office\\", versionFromReg);

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

        public void SetPowerPointDisplayMonitor(string DisplayName)
        {
            string versionFromReg = GetVersionNumberFromRegistry();
            string versionInstalled = LatestVersions[versionFromReg];
            string bit32or64 = "64 bit"; 

            bool? Office64BitFromReg = Off64Bit("SOFTWARE\\Microsoft\\Office\\", versionFromReg);

            if (Office64BitFromReg == null)
                Office64BitFromReg = Off64Bit("SOFTWARE\\Wow6432Node\\Microsoft\\Office\\", versionFromReg);

            if (Office64BitFromReg.HasValue && Office64BitFromReg.Value)
            {
                versionInstalled += " (64 bit)";
                bit32or64 = "64 bit";
            }
            else if (Office64BitFromReg.HasValue && !Office64BitFromReg.Value)
            {
                versionInstalled += " (32 bit)";
                bit32or64 = "32 bit";
            }
            else
            {
                versionInstalled += " (Unknown bit)";
                bit32or64 = "Unknown bit";
            }

            if (bit32or64== "64 bit")
            {
                SaveRegValue("DisplayMonitor", DisplayName);
                SaveRegValue("UseAutoMonSelection", 0);
                SaveRegValue("UseMonMgr", 0);                
            }
            else if(bit32or64=="32 bit")
            {

            }
            else
            {
                //daniel
                //Office365일 경우 Unknown bit
                SaveRegValue("DisplayMonitor", DisplayName);
                SaveRegValue("UseAutoMonSelection", 0);
                SaveRegValue("UseMonMgr", 0);
            }
        }

        private bool? Off64Bit(string key, string version)
        {
            bool? Office64BitFromReg = null;
            string OfficeVersionKey = $@"{key}{version}\PowerPoint";

            string Bitness = Reg64(HKEY_LOCAL_MACHINE, OfficeVersionKey, "Bitness");
            if (Bitness == "x86")
                Office64BitFromReg = false;
            else if ((Bitness == "x64"))
                Office64BitFromReg = true;
            return Office64BitFromReg;
        }

        private string Reg64(UIntPtr parent, string key, string prop)
        {
            int ikey = 0;
            int bit36_64 = 0x0100;
            int query = 0x0001;
            try
            {
                uint res = RegOpenKeyEx(HKEY_LOCAL_MACHINE, key, 0, query | bit36_64, out ikey);
                if (0 != res) return null;
                uint type = 0;
                uint data = 1024;
                StringBuilder buffer = new StringBuilder(1024);
                RegQueryValueEx(ikey, prop, 0, ref type, buffer, ref data);
                string ver = buffer.ToString();
                return ver;
            }
            finally
            {
                if (0 != ikey) RegCloseKey(ikey);
            }
        }

        public static void SaveRegValue(string Name, string Value)
        {
            try
            {
                using (RegistryKey registryKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Office\16.0\PowerPoint\Options", true))
                {
                    registryKey.SetValue(Name, Value, RegistryValueKind.String);
                }
            }
            catch
            {
            }

        }

        public static void SaveRegValue(string Name, int Value)
        {
            try
            {
                using (RegistryKey registryKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Office\16.0\PowerPoint\Options", true))
                {
                    registryKey.SetValue(Name, Value, RegistryValueKind.DWord);
                }
            }
            catch
            {
            }
        }
    }
}
