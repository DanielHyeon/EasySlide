using System;

namespace Easislides.Core;

public static class SlideAspectRatio
{
    public static (int Left, int Top, int Width, int Height) Fit(int left, int top, int availableWidth, int availableHeight, bool wide)
    {
        if (availableWidth <= 0 || availableHeight <= 0) return default;
        var (x, y) = Aspect(wide);
        int width = (int)Math.Max(1, Math.Min(availableWidth, (long)availableHeight * x / y));
        int height = (int)Math.Max(1, Math.Min(availableHeight, (long)availableWidth * y / x));
        return (left + (availableWidth - width) / 2, top + (availableHeight - height) / 2, width, height);
    }

    public static int HeightForWidth(int width, bool wide)
    {
        var (x, y) = Aspect(wide);
        return (int)Math.Max(1, (long)width * y / x);
    }

    private static (int Width, int Height) Aspect(bool wide) => wide ? (16, 9) : (4, 3);
}
