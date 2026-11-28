using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using static Axicultus.Interop.DesktopWindows;

namespace Axicultus.Interop;

/// <summary>
/// A screen's scale and a window's place in physical desktop pixels (moved here from the WPF app's
/// SystemDisplayReader and DesktopWindowPlacement, which use them): the caption settings window is placed with them too.
/// </summary>
public static class DesktopPixels
{
    /// <summary>The scale of the screen holding the rectangle's centre (1 at 96 DPI).</summary>
    public static double Scale(Rectangle bounds)
    {
        var center = new Point(bounds.Left + bounds.Width / 2, bounds.Top + bounds.Height / 2);
        var monitor = Windows.Win32.PInvoke.MonitorFromPoint(center, Windows.Win32.Graphics.Gdi.MONITOR_FROM_FLAGS.MONITOR_DEFAULTTONEAREST);
        try
        {
            if (Windows.Win32.PInvoke.GetDpiForMonitor(monitor, 0, out uint dpi, out _) == 0 && dpi != 0)
                return dpi / 96d;
        }
        catch (DllNotFoundException) { }
        catch (EntryPointNotFoundException) { }
        using var graphics = Graphics.FromHwnd(IntPtr.Zero);
        return graphics.DpiX / 96d;
    }

    /// <summary>Puts the window at the rectangle. Physical desktop pixels stay in one coordinate space even across a DPI boundary.</summary>
    public static void Place(nint handle, double left, double top, double width, double height)
    {
        const uint keepZOrderAndActivation = 0x0004 | 0x0010;
        int x = (int)Math.Round(left);
        int y = (int)Math.Round(top);
        int w = Math.Max(1, (int)Math.Round(width));
        int h = Math.Max(1, (int)Math.Round(height));
        void Position()
        {
            if (!SetWindowPos(handle, IntPtr.Zero, x, y, w, h, keepZOrderAndActivation))
                throw new Win32Exception(Marshal.GetLastWin32Error());
        }

        Position();
        // A cross-monitor WM_DPICHANGED can apply its own suggested rectangle during the move.
        // Once the HWND is on the target monitor its DPI is settled.
        if (GetWindowRect(handle, out var actual) &&
            (actual.Left != x || actual.Top != y || actual.Right - actual.Left != w || actual.Bottom - actual.Top != h))
            Position();
    }
}
