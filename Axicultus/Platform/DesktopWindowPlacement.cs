using static Axicultus.Interop.DesktopWindows;
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

    // (Axicultus.Interop.DesktopPixels; the caption settings window is placed the same way)
    internal static void Apply(nint handle, OutputWindowPlacement placement)
        => Axicultus.Interop.DesktopPixels.Place(handle, placement.Left, placement.Top, placement.Width, placement.Height);

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
        if (!WindowHasClass(handle, "screenClass", StringComparison.OrdinalIgnoreCase)) return;
        SetTopmost(handle, separate);
        if (!separate) SetForegroundWindow(handle);
    }

    internal static void SetTopmost(nint handle, bool topmost)
    {
        const uint noMoveSizeOrActivation = 0x0001 | 0x0002 | 0x0010;
        if (!SetWindowPos(handle, new IntPtr(topmost ? -1 : -2), 0, 0, 0, 0, noMoveSizeOrActivation))
            throw new Win32Exception(Marshal.GetLastWin32Error());
    }

}
