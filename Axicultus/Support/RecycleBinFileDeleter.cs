using System;
using System.IO;

namespace Axicultus.Wpf.Support;

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
            bool aborted = false;
            nint mappings = 0;
            int result = Axicultus.Interop.ShellFiles.Operate(0, 3, path + "\0\0", null,
                0x40 | 0x10 | 0x04 | 0x0400, ref aborted, ref mappings, null);
            return result == 0 && !aborted && !File.Exists(path);
        }
        catch
        {
            return false;
        }
    }
}
