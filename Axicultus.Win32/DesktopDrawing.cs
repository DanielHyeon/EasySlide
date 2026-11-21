using System;
using System.Drawing;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
using Windows.Win32.UI.Controls;

namespace Axicultus.Interop;

// WinForms gives owner-draw handlers borrowed HDCs and image-list handles as IntPtr.
public static class DesktopDrawing
{
    public static int SaveDC(nint dc) => PInvoke.SaveDC((HDC)dc);
    public static bool RestoreDC(nint dc, int saved) => PInvoke.RestoreDC((HDC)dc, saved);
    public static int ExcludeClipRect(nint dc, int left, int top, int right, int bottom)
        => (int)PInvoke.ExcludeClipRect((HDC)dc, left, top, right, bottom);
    public static nint CreateCompatibleDC(nint dc) => PInvoke.CreateCompatibleDC((HDC)dc);
    public static nint CreateCompatibleBitmap(nint dc, int width, int height)
        => PInvoke.CreateCompatibleBitmap((HDC)dc, width, height);
    public static bool DeleteDC(nint dc) => PInvoke.DeleteDC((HDC)dc);
    public static unsafe bool SetViewportOrigin(nint dc, int x, int y)
        => PInvoke.SetViewportOrgEx((HDC)dc, x, y, null);
    public static bool BitBlt(nint dest, int x, int y, int width, int height, nint source, int sx, int sy, int rop)
        => PInvoke.BitBlt((HDC)dest, x, y, width, height, (HDC)source, sx, sy, (ROP_CODE)rop);
    public static int FillRect(nint dc, Rectangle bounds, nint brush)
        => PInvoke.FillRect((HDC)dc, (RECT)bounds, (HBRUSH)brush);
    public static bool DrawFocusRect(nint dc, Rectangle bounds)
        => PInvoke.DrawFocusRect((HDC)dc, (RECT)bounds);
    public static nint CreateSolidBrush(int colour) => PInvoke.CreateSolidBrush((COLORREF)(uint)colour);
    public static nint SelectObject(nint dc, nint obj) => PInvoke.SelectObject((HDC)dc, (HGDIOBJ)obj);
    public static bool DeleteObject(nint obj) => PInvoke.DeleteObject((HGDIOBJ)obj);
    public static bool ImageList_DrawEx(nint list, int index, nint dc, int x, int y, int width, int height,
        int background, int foreground, int style) => PInvoke.ImageList_DrawEx((HIMAGELIST)list, index,
        (HDC)dc, x, y, width, height, (COLORREF)(uint)background, (COLORREF)(uint)foreground, (IMAGE_LIST_DRAW_STYLE)style);
}
