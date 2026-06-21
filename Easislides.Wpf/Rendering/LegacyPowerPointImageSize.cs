namespace Easislides.Wpf.Rendering;

/// <summary>
/// FrmMain OfficeLib.PowerPoint uses Slide.Export JPG at 640x480.
/// Keep WPF PPT preview/thumbnail rendering on the same pixel contract for parity and fast preview.
/// </summary>
public static class LegacyPowerPointImageSize
{
    public const int Width = 640;
    public const int Height = 480;
}
