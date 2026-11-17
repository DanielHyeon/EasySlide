using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Easislides.Wpf.Support;

internal static class RecycleBinFileDeleter
{
    public static bool Delete(string path)
    {
        if (!File.Exists(path))
        {
            return false;
        }

        try
        {
            var operation = new SHFILEOPSTRUCT
            {
                hwnd = IntPtr.Zero,
                wFunc = 3,
                pFrom = path + '\0' + '\0',
                pTo = null,
                fFlags = 0x40 | 0x10 | 0x04 | 0x0400,
                fAnyOperationsAborted = false,
                hNameMappings = IntPtr.Zero,
                lpszProgressTitle = null,
            };

            var result = SHFileOperation(ref operation);
            return result == 0 && !operation.fAnyOperationsAborted && !File.Exists(path);
        }
        catch
        {
            return false;
        }
    }

    [DllImport("shell32.dll", CharSet = CharSet.Auto)]
    private static extern int SHFileOperation(ref SHFILEOPSTRUCT lpFileOp);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    private struct SHFILEOPSTRUCT
    {
        public IntPtr hwnd;
        public uint wFunc;
        public string pFrom;
        public string? pTo;
        public ushort fFlags;
        [MarshalAs(UnmanagedType.Bool)] public bool fAnyOperationsAborted;
        public IntPtr hNameMappings;
        public string? lpszProgressTitle;
    }
}
