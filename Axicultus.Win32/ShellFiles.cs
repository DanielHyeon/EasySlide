using System;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Shell;

#if SHELL_X86
namespace Axicultus.Interop.Packed32;
#else
namespace Axicultus.Interop;
#endif

public static class ShellFiles
{
    public static unsafe int Operate(nint window, uint function, string from, string to, ushort flags,
        ref bool aborted, ref nint mappings, string title)
    {
#if !SHELL_X86
        // SHFILEOPSTRUCTW uses one-byte packing on 32-bit Windows and natural packing on 64-bit Windows.
        if (IntPtr.Size == 4)
            return Packed32.ShellFiles.Operate(window, function, from, to, flags, ref aborted, ref mappings, title);
#endif
        // Shell expects multistrings, including a second terminating NUL after the last path.
        if (from != null && !from.EndsWith('\0')) from += '\0';
        if (to != null && !to.EndsWith('\0')) to += '\0';
        fixed (char* source = from, destination = to, caption = title)
        {
            SHFILEOPSTRUCTW op = new()
            {
                hwnd = (HWND)window, wFunc = function, pFrom = new PCZZWSTR(source),
                pTo = new PCZZWSTR(destination), fFlags = flags, lpszProgressTitle = caption,
                fAnyOperationsAborted = aborted, hNameMappings = (void*)mappings
            };
            int result = PInvoke.SHFileOperation(ref op);
            aborted = op.fAnyOperationsAborted;
            mappings = (nint)op.hNameMappings;
            return result;
        }
    }
}
