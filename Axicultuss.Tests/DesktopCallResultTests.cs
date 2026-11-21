using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using Axicultus.Util;
using Microsoft.Win32;
using Xunit;

namespace Axicultus.Tests;

public class DesktopCallResultTests
{
    [Fact]
    public void OfficeReaderHandlesLongUnicodeStringsAndMissingOrNonStringValues()
    {
        string path = @"Software\Axicultus.Tests\" + Guid.NewGuid().ToString("N");
        using var root = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry64);
        try
        {
            using (var key = root.CreateSubKey(path))
            {
                key.SetValue("Path", new string('한', 1600));
                key.SetValue("Invalid", 17);
            }
            Assert.Equal(new string('한', 1600), OfficeVersion.ReadRegistryString(RegistryHive.CurrentUser, path, "Path"));
            Assert.Null(OfficeVersion.ReadRegistryString(RegistryHive.CurrentUser, path, "Bitness"));
            Assert.Null(OfficeVersion.ReadRegistryString(RegistryHive.CurrentUser, path, "Invalid"));
            Assert.Null(OfficeVersion.ReadRegistryString(RegistryHive.CurrentUser, path + @"\absent", "Path"));
        }
        finally { root.DeleteSubKeyTree(path, throwOnMissingSubKey: false); }
    }

    [Theory]
    [InlineData(0, false, true)]
    [InlineData(0, true, false)]
    [InlineData(5, false, false)]
    public void RecycleReportsShellErrorsAndCancellation(int error, bool aborted, bool expected)
    {
        string file = Path.GetTempFileName();
        try
        {
            bool result = Gf.RecycleBin(file, (ref SHFILEOPSTRUCT operation) =>
            {
                File.Delete(file);
                operation.fAnyOperationsAborted = aborted;
                return error;
            });
            Assert.Equal(expected, result);
        }
        finally { File.Delete(file); }
    }

    [Fact]
    public void MissingFileDoesNotReportSuccessfulRecycle()
    {
        string path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".tmp");
        Assert.False(Gf.RecycleBin(path));
    }

    [Fact]
    public void TextMeasurementDistinguishesAnEmptyTitleFromAnInvalidDeviceContext()
    {
        Assert.Equal(0, GdiListText.Measure(IntPtr.Zero, "", IntPtr.Zero));
        Assert.Throws<InvalidOperationException>(() => GdiListText.Measure(IntPtr.Zero, "Song", IntPtr.Zero));
    }

    [Fact]
    public void MeasuringAndDrawingRestoreTheDeviceContextColourAndProduceText()
    {
        using var bitmap = new Bitmap(120, 35);
        using var graphics = Graphics.FromImage(bitmap);
        graphics.Clear(Color.White);
        var hdc = graphics.GetHdc();
        var font = SystemFonts.DefaultFont.ToHfont();
        try
        {
            int original = GetTextColor(hdc);
            Assert.True(GdiListText.Measure(hdc, "Praise", font) > 0);
            GdiListText.Draw(hdc, "Praise", font, new Rectangle(0, 0, 120, 35), Color.Blue, 0x20);
            Assert.Equal(original, GetTextColor(hdc));
        }
        finally
        {
            graphics.ReleaseHdc(hdc);
            DeleteObject(font);
        }
        Assert.Contains(System.Linq.Enumerable.Range(0, 120 * 35), i => bitmap.GetPixel(i % 120, i / 120).ToArgb() != Color.White.ToArgb());
    }

    [DllImport("gdi32.dll")] private static extern int GetTextColor(IntPtr hdc);
    [DllImport("gdi32.dll")] private static extern bool DeleteObject(IntPtr handle);
}
