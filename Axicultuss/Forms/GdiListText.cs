using static Axicultus.Interop.DesktopDrawing;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
using System;
using System.Drawing;

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

    private static unsafe int DrawCore(IntPtr hdc, string text, IntPtr font, Rectangle rect, Color colour, int flags)
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
                (PInvoke.SetBkMode((HDC)hdc, BACKGROUND_MODE.TRANSPARENT) == 0 ||
                 PInvoke.SetTextColor((HDC)hdc, (COLORREF)(uint)ColorTranslator.ToWin32(colour)).Value == uint.MaxValue))
                throw new InvalidOperationException("Cannot set the list text colours.");

            RECT bounds = rect;
            fixed (char* characters = text)
            {
                if (PInvoke.DrawText((HDC)hdc, characters, text.Length, ref bounds, (DRAW_TEXT_FORMAT)flags) == 0)
                    throw new InvalidOperationException("Windows could not draw or measure the list text.");
            }
            width = bounds.Width;
        }
        finally
        {
            restored = RestoreDC(hdc, saved);
        }
        if (!restored) throw new InvalidOperationException("Cannot restore the list drawing context.");
        return width;
    }

}
