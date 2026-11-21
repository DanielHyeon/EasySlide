using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Axicultus;

internal static class GdiListText
{
    private const int CalculateRectangle = 0x400, SingleLine = 0x20, NoPrefix = 0x800;

    internal static int Measure(IntPtr hdc, string text, IntPtr font)
        => string.IsNullOrEmpty(text) ? 0 : DrawCore(hdc, text, font, default, default, CalculateRectangle | SingleLine | NoPrefix);

    internal static void Draw(IntPtr hdc, string text, IntPtr font, Rectangle rect, Color colour, int flags)
    {
        if (!string.IsNullOrEmpty(text) && rect.Width > 0 && rect.Height > 0)
            DrawCore(hdc, text, font, rect, colour, flags);
    }

    private static int DrawCore(IntPtr hdc, string text, IntPtr font, Rectangle rect, Color colour, int flags)
    {
        int saved = SaveDC(hdc);
        if (saved == 0) throw new InvalidOperationException("Cannot save the list drawing context.");
        int width;
        bool restored;
        try
        {
            // WM_GETFONT can return NULL when a list uses the device context's default font.
            if (font != IntPtr.Zero && SelectObject(hdc, font) == IntPtr.Zero)
                throw new InvalidOperationException("Cannot select the list font.");
            if ((flags & CalculateRectangle) == 0 &&
                (SetBkMode(hdc, 1) == 0 || SetTextColor(hdc, ColorTranslator.ToWin32(colour)) == -1))
                throw new InvalidOperationException("Cannot set the list text colours.");

            NativeRect bounds = new() { Left = rect.Left, Top = rect.Top, Right = rect.Right, Bottom = rect.Bottom };
            if (DrawText(hdc, text, text.Length, ref bounds, flags) == 0)
                throw new InvalidOperationException("Windows could not draw or measure the list text.");
            width = bounds.Right - bounds.Left;
        }
        finally
        {
            restored = RestoreDC(hdc, saved);
        }
        if (!restored) throw new InvalidOperationException("Cannot restore the list drawing context.");
        return width;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct NativeRect { public int Left, Top, Right, Bottom; }

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int DrawText(IntPtr hdc, string text, int count, ref NativeRect rect, int flags);
    [DllImport("gdi32.dll")]
    private static extern int SaveDC(IntPtr hdc);
    [DllImport("gdi32.dll")]
    private static extern bool RestoreDC(IntPtr hdc, int saved);
    [DllImport("gdi32.dll")]
    private static extern IntPtr SelectObject(IntPtr hdc, IntPtr obj);
    [DllImport("gdi32.dll")]
    private static extern int SetTextColor(IntPtr hdc, int colour);
    [DllImport("gdi32.dll")]
    private static extern int SetBkMode(IntPtr hdc, int mode);
}
