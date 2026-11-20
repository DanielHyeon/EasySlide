using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using Axicultus.Wpf.Shell;

namespace Axicultus.Wpf.Platform;

internal static class DesktopWindowPlacement
{
    // Physical desktop pixels stay in one coordinate space even across a DPI boundary.
    internal static void Apply(Window window, OutputWindowPlacement placement)
    {
        const uint keepZOrderAndActivation = 0x0004 | 0x0010;
        var handle = new WindowInteropHelper(window).EnsureHandle();
        int x = (int)Math.Round(placement.Left);
        int y = (int)Math.Round(placement.Top);
        int width = Math.Max(1, (int)Math.Round(placement.Width));
        int height = Math.Max(1, (int)Math.Round(placement.Height));
        void Position()
        {
            if (!SetWindowPos(handle, IntPtr.Zero, x, y, width, height, keepZOrderAndActivation))
                throw new Win32Exception(Marshal.GetLastWin32Error());
        }

        Position();
        // A cross-monitor WM_DPICHANGED can apply its own suggested rectangle during the move.
        // Once the HWND is on the target monitor its DPI is settled.
        if (GetWindowRect(handle, out var actual) &&
            (actual.Left != x || actual.Top != y || actual.Right - actual.Left != width || actual.Bottom - actual.Top != height))
            Position();
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct NativeRect { public int Left, Top, Right, Bottom; }

    [DllImport("user32.dll")]
    private static extern bool GetWindowRect(IntPtr window, out NativeRect rect);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool SetWindowPos(IntPtr window, IntPtr insertAfter, int x, int y,
        int width, int height, uint flags);
}
