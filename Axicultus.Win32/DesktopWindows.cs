using System;
using System.Drawing;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;

namespace Axicultus.Interop;

public static class DesktopWindows
{
    public static bool SetForegroundWindow(nint window) => PInvoke.SetForegroundWindow((HWND)window);
    public static bool SetWindowPos(nint window, nint after, int x, int y, int width, int height, uint flags)
        => PInvoke.SetWindowPos((HWND)window, (HWND)after, x, y, width, height, (SET_WINDOW_POS_FLAGS)flags);
    public static bool GetWindowRect(nint window, out Rectangle bounds)
    {
        bool success = PInvoke.GetWindowRect((HWND)window, out RECT rect);
        bounds = rect;
        return success;
    }
    public static uint GetWindowThreadProcessId(nint window, out uint processId)
        => PInvoke.GetWindowThreadProcessId((HWND)window, out processId);
    public static nint SendMessage(nint window, int message, nint wParam, nint lParam)
        => PInvoke.SendMessage((HWND)window, (uint)message, (WPARAM)(nuint)wParam, lParam);

    public static bool WindowHasClass(nint window, string name, StringComparison comparison = StringComparison.Ordinal)
    {
        Span<char> buffer = stackalloc char[256];
        int length = PInvoke.GetClassName((HWND)window, buffer);
        return length > 0 && buffer[..length].Equals(name, comparison);
    }

    public static string ReadWindowTitle(nint window, int capacity)
    {
        Span<char> buffer = (uint)capacity <= 512 ? stackalloc char[capacity] : new char[capacity];
        int length = PInvoke.GetWindowText((HWND)window, buffer);
        return new string(buffer[..length]);
    }

    public static bool EnumWindows(Func<nint, nint, bool> visit, nint state)
        => PInvoke.EnumWindows((window, param) => visit(window, param), state);
}
