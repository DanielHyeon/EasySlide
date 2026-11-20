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
        => Apply(new WindowInteropHelper(window).EnsureHandle(), placement);

    internal static void Apply(nint handle, OutputWindowPlacement placement)
    {
        const uint keepZOrderAndActivation = 0x0004 | 0x0010;
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

    internal static bool IsSeparateAudienceMonitor(OutputWindowPlacement placement)
    {
        if (placement.AsSingleMonitor || Application.Current is not { } app || !app.Dispatcher.CheckAccess() || app.MainWindow is not { } shell)
            return false;
        var operatorHandle = new WindowInteropHelper(shell).Handle;
        if (operatorHandle == 0) return false;
        var center = new System.Drawing.Point((int)(placement.Left + placement.Width / 2), (int)(placement.Top + placement.Height / 2));
        return System.Windows.Forms.Screen.FromPoint(center).DeviceName != System.Windows.Forms.Screen.FromHandle(operatorHandle).DeviceName;
    }

    internal static void RevealSlideShow(nint handle, OutputWindowPlacement placement, bool playerHadFocus)
    {
        // The operator may have closed or moved the Office window while the video was playing.
        if (!GetWindowRect(handle, out var rect) || rect.Left != (int)placement.Left || rect.Top != (int)placement.Top ||
            rect.Right - rect.Left != (int)placement.Width || rect.Bottom - rect.Top != (int)placement.Height) return;
        bool separate = IsSeparateAudienceMonitor(placement);
        if (!separate && !playerHadFocus) return;
        var windowClass = new System.Text.StringBuilder(32);
        if (GetClassName(handle, windowClass, windowClass.Capacity) == 0 ||
            !windowClass.ToString().Equals("screenClass", StringComparison.OrdinalIgnoreCase)) return;
        SetTopmost(handle, separate);
        if (!separate) SetForegroundWindow(handle);
    }

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int GetClassName(nint window, System.Text.StringBuilder name, int count);

    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(nint window);

    internal static void SetTopmost(nint handle, bool topmost)
    {
        const uint noMoveSizeOrActivation = 0x0001 | 0x0002 | 0x0010;
        if (!SetWindowPos(handle, new IntPtr(topmost ? -1 : -2), 0, 0, 0, 0, noMoveSizeOrActivation))
            throw new Win32Exception(Marshal.GetLastWin32Error());
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct NativeRect { public int Left, Top, Right, Bottom; }

    [DllImport("user32.dll")]
    private static extern bool GetWindowRect(IntPtr window, out NativeRect rect);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool SetWindowPos(IntPtr window, IntPtr insertAfter, int x, int y,
        int width, int height, uint flags);
}
