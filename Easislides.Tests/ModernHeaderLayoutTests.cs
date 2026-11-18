using System;
using System.Drawing;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Windows.Forms;
using Xunit;

namespace Easislides.Tests;

public class ModernHeaderLayoutTests
{
    [Theory]
    [InlineData(false, 368)]
    [InlineData(true, 249)]
    public void TitleAndIconButtonsShareOneContinuousHeaderFrame(bool output, int width)
    {
        ExceptionDispatchInfo failure = null;
        var thread = new Thread(() =>
        {
            try
            {
                using var header = new Panel { Size = new Size(width, 33) };
                using var title = new ListView { Dock = DockStyle.Fill, BorderStyle = BorderStyle.FixedSingle };
                using var first = new Button { Dock = DockStyle.Right, Width = 38, Text = "1" };
                using var middle = new Button { Dock = DockStyle.Right, Width = 38, Text = "2" };
                using var right = new Button { Dock = DockStyle.Right, Width = 48, Text = "3" };
                using var gap = new Panel { Dock = DockStyle.Right, Width = 16 };
                header.Controls.AddRange([first, middle, title, right]);
                if (output)
                {
                    header.Controls.Add(gap);
                    header.Controls.SetChildIndex(gap, header.Controls.GetChildIndex(right));
                }

                typeof(FrmMain).GetMethod("ApplyHeaderTint", BindingFlags.Static | BindingFlags.NonPublic)
                    .Invoke(null, [header, title, output ? Color.MistyRose : Color.AliceBlue]);
                header.Width += 48;
                header.PerformLayout();

                Assert.Equal(BorderStyle.None, title.BorderStyle);
                Assert.Equal(1, title.Left);
                Assert.Equal(first.Left, title.Right);
                Assert.Equal(header.ClientSize.Width - 1, right.Right);
                foreach (Control item in header.Controls)
                {
                    Assert.Equal(1, item.Top);
                    Assert.Equal(header.ClientSize.Height - 1, item.Bottom);
                }

                using var bitmap = new Bitmap(header.Width, header.Height);
                header.DrawToBitmap(bitmap, header.ClientRectangle);
                Assert.Equal(bitmap.GetPixel(0, 0), bitmap.GetPixel(right.Left + 4, 0));
                Assert.Equal(bitmap.GetPixel(0, 0), bitmap.GetPixel(right.Left + 4, header.Height - 1));
                Assert.NotEqual(header.BackColor.ToArgb(), bitmap.GetPixel(right.Left + 4, 0).ToArgb());
            }
            catch (Exception ex)
            {
                failure = ExceptionDispatchInfo.Capture(ex);
            }
        }) { IsBackground = true };
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        Assert.True(thread.Join(TimeSpan.FromSeconds(15)));
        failure?.Throw();
    }
}
